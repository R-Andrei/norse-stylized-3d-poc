using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Vegetation
{
    internal readonly struct VegetationInteractorSample
    {
        public VegetationInteractorSample(
            VegetationInteractor source,
            Vector2 startXZ,
            Vector2 endXZ,
            float radius,
            float bendStrength,
            float flattenStrength,
            float movementBlend,
            int priority)
        {
            Source = source;
            StartXZ = startXZ;
            EndXZ = endXZ;
            Radius = radius;
            BendStrength = bendStrength;
            FlattenStrength = flattenStrength;
            MovementBlend = movementBlend;
            Priority = priority;
        }

        public VegetationInteractor Source { get; }
        public Vector2 StartXZ { get; }
        public Vector2 EndXZ { get; }
        public float Radius { get; }
        public float BendStrength { get; }
        public float FlattenStrength { get; }
        public float MovementBlend { get; }
        public int Priority { get; }
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("PS3D/Vegetation/Vegetation Interactor")]
    public sealed class VegetationInteractor : MonoBehaviour
    {
        private static readonly List<VegetationInteractor> ActiveInteractorsInternal =
            new List<VegetationInteractor>();

        [Header("Immediate Grass Displacement")]
        [SerializeField, Min(0.05f)]
        [Tooltip("World-space XZ radius affected by this object.")]
        private float interactionRadius = 0.55f;

        [SerializeField, Range(0f, 2f)]
        [Tooltip("Strength of the horizontal grass separation written into the immediate interaction field.")]
        private float bendStrength = 1f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Temporary vertical flattening written while this object occupies or sweeps through grass. This does not create persistent trail state.")]
        private float flattenStrength = 0.20f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Blends radial parting toward the object's movement direction as speed rises.")]
        private float movementDirectionInfluence = 0.35f;

        [SerializeField, Min(0.05f)]
        [Tooltip("Object speed in metres per second that reaches the full configured movement-direction influence.")]
        private float fullMovementResponseSpeed = 2f;

        [SerializeField, Min(0.1f)]
        [Tooltip("Movement longer than this between interaction updates is treated as a teleport and does not stamp a long swept path.")]
        private float maximumSweepDistance = 5f;

        [SerializeField]
        [Tooltip("Higher-priority interactors are retained first if the active field exceeds its configured record capacity.")]
        private int priority;

        private Vector3 previousSamplePosition;
        private bool sampleInitialized;

        public static IReadOnlyList<VegetationInteractor> ActiveInteractors =>
            ActiveInteractorsInternal;
        public float InteractionRadius => interactionRadius;
        public float BendStrength => bendStrength;
        public float FlattenStrength => flattenStrength;
        public float MovementDirectionInfluence => movementDirectionInfluence;
        public float FullMovementResponseSpeed => fullMovementResponseSpeed;
        public float MaximumSweepDistance => maximumSweepDistance;
        public int Priority => priority;

        private void OnEnable()
        {
            if (!ActiveInteractorsInternal.Contains(this))
            {
                ActiveInteractorsInternal.Add(this);
            }

            ResetSampleHistory();
        }

        private void OnDisable()
        {
            ActiveInteractorsInternal.Remove(this);
            sampleInitialized = false;
        }

        private void OnDestroy()
        {
            ActiveInteractorsInternal.Remove(this);
        }

        private void OnValidate()
        {
            interactionRadius = Mathf.Clamp(interactionRadius, 0.05f, 20f);
            bendStrength = Mathf.Clamp(bendStrength, 0f, 2f);
            flattenStrength = Mathf.Clamp01(flattenStrength);
            movementDirectionInfluence = Mathf.Clamp01(
                movementDirectionInfluence);
            fullMovementResponseSpeed = Mathf.Clamp(
                fullMovementResponseSpeed,
                0.05f,
                50f);
            maximumSweepDistance = Mathf.Clamp(
                maximumSweepDistance,
                0.1f,
                100f);
        }

        internal VegetationInteractorSample CaptureSample(float deltaTime)
        {
            Vector3 currentPosition = transform.position;
            Vector3 startPosition = sampleInitialized
                ? previousSamplePosition
                : currentPosition;
            Vector2 startXZ = new Vector2(startPosition.x, startPosition.z);
            Vector2 endXZ = new Vector2(currentPosition.x, currentPosition.z);
            float distance = Vector2.Distance(startXZ, endXZ);
            if (distance > maximumSweepDistance)
            {
                startXZ = endXZ;
                distance = 0f;
            }

            float speed = deltaTime > 0.000001f
                ? distance / deltaTime
                : 0f;
            float movementBlend = movementDirectionInfluence *
                Mathf.Clamp01(speed / Mathf.Max(0.05f, fullMovementResponseSpeed));

            previousSamplePosition = currentPosition;
            sampleInitialized = true;
            return new VegetationInteractorSample(
                this,
                startXZ,
                endXZ,
                interactionRadius,
                bendStrength,
                flattenStrength,
                movementBlend,
                priority);
        }

        public void ResetSampleHistory()
        {
            previousSamplePosition = transform.position;
            sampleInitialized = true;
        }

        private void OnDrawGizmosSelected()
        {
            Color previousColor = Gizmos.color;
            Gizmos.color = new Color(0.25f, 1f, 0.35f, 0.85f);
            Vector3 centre = transform.position;
            Gizmos.DrawWireSphere(centre, interactionRadius);
            if (sampleInitialized)
            {
                Gizmos.DrawLine(previousSamplePosition, centre);
            }
            Gizmos.color = previousColor;
        }
    }
}
