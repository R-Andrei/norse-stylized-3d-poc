using UnityEngine;
using UnityEngine.InputSystem;

namespace ProgrammaticStylized3D.Input
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [AddComponentMenu("Programmatic Stylized 3D/Input/Player Blockout Movement")]
    public sealed class PlayerBlockoutMovement : MonoBehaviour
    {
        private const string MoveActionPath = "Player/Move";
        private const string PointActionPath = "UI/Point";
        private const float InputEpsilon = 0.000001f;

        [Header("Input")]
        [Tooltip("Existing project input actions containing Player/Move and UI/Point.")]
        [SerializeField] private InputActionAsset inputActions;

        [Tooltip("Camera used to project the cursor onto the player's horizontal facing plane.")]
        [SerializeField] private Camera viewCamera;

        [Header("Movement")]
        [Tooltip("Maximum planar movement speed in metres per second.")]
        [Min(0f)]
        [SerializeField] private float maximumSpeed = 5f;

        [Tooltip("Maximum planar deceleration after movement input is released, in metres per second squared.")]
        [Min(0f)]
        [SerializeField] private float deceleration = 40f;

        [Header("Facing")]
        [Tooltip("Maximum cursor-facing yaw speed in degrees per second.")]
        [Min(0f)]
        [SerializeField] private float rotationSpeed = 720f;

        private Rigidbody body;
        private InputAction moveAction;
        private InputAction pointAction;
        private bool enabledMoveAction;
        private bool enabledPointAction;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            if (!TryResolveDependencies())
            {
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (!TryResolveDependencies())
            {
                enabled = false;
                return;
            }

            if (!moveAction.enabled)
            {
                moveAction.Enable();
                enabledMoveAction = true;
            }

            if (!pointAction.enabled)
            {
                pointAction.Enable();
                enabledPointAction = true;
            }
        }

        private void OnDisable()
        {
            if (enabledMoveAction && moveAction != null)
            {
                moveAction.Disable();
                enabledMoveAction = false;
            }

            if (enabledPointAction && pointAction != null)
            {
                pointAction.Disable();
                enabledPointAction = false;
            }

            if (body != null)
            {
                Vector3 velocity = body.linearVelocity;
                body.linearVelocity = new Vector3(0f, velocity.y, 0f);
            }
        }

        private void OnValidate()
        {
            maximumSpeed = Mathf.Max(0f, maximumSpeed);
            deceleration = Mathf.Max(0f, deceleration);
            rotationSpeed = Mathf.Max(0f, rotationSpeed);
        }

        private void FixedUpdate()
        {
            ApplyMovement();
            ApplyCursorFacing();
        }

        private bool TryResolveDependencies()
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody>();
            }

            if (inputActions == null)
            {
                Debug.LogError(
                    "PlayerBlockoutMovement requires the project InputActionAsset.",
                    this);
                return false;
            }

            moveAction ??= inputActions.FindAction(MoveActionPath);
            pointAction ??= inputActions.FindAction(PointActionPath);
            if (moveAction == null || pointAction == null)
            {
                Debug.LogError(
                    $"PlayerBlockoutMovement requires actions {MoveActionPath} and {PointActionPath}.",
                    this);
                return false;
            }

            if (viewCamera == null)
            {
                Debug.LogError(
                    "PlayerBlockoutMovement requires a view camera.",
                    this);
                return false;
            }

            return true;
        }

        private void ApplyMovement()
        {
            Vector2 input = Vector2.ClampMagnitude(
                moveAction.ReadValue<Vector2>(),
                1f);
            Vector3 desiredVelocity =
                new Vector3(input.x, 0f, input.y) * maximumSpeed;
            Vector3 currentVelocity = body.linearVelocity;
            Vector3 currentPlanarVelocity =
                new Vector3(currentVelocity.x, 0f, currentVelocity.z);
            Vector3 nextPlanarVelocity = input.sqrMagnitude > InputEpsilon
                ? desiredVelocity
                : Vector3.MoveTowards(
                    currentPlanarVelocity,
                    Vector3.zero,
                    deceleration * Time.fixedDeltaTime);

            body.linearVelocity = new Vector3(
                nextPlanarVelocity.x,
                currentVelocity.y,
                nextPlanarVelocity.z);
        }

        private void ApplyCursorFacing()
        {
            Vector2 cursorPosition = pointAction.ReadValue<Vector2>();
            Ray cursorRay = viewCamera.ScreenPointToRay(cursorPosition);
            var facingPlane = new Plane(Vector3.up, body.position);
            if (!facingPlane.Raycast(cursorRay, out float distance))
            {
                return;
            }

            Vector3 facingDirection = cursorRay.GetPoint(distance) - body.position;
            facingDirection.y = 0f;
            if (facingDirection.sqrMagnitude <= InputEpsilon)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(
                facingDirection,
                Vector3.up);
            Quaternion nextRotation = Quaternion.RotateTowards(
                body.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime);
            body.MoveRotation(nextRotation);
        }
    }

}
