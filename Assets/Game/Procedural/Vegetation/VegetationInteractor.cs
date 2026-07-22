using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Vegetation
{
    public enum VegetationInteractionDirectionMode
    {
        Radial = 0,
        WorldXBiased = 1,
        Hybrid = 2
    }

    public enum VegetationTrailMode
    {
        Off = 0,
        Timed = 1,
        SessionPersistent = 2
    }

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
            VegetationInteractionDirectionMode directionMode,
            float worldXBias,
            float worldZStrength,
            int priority)
        {
            Source = source;
            StartXZ = startXZ;
            EndXZ = endXZ;
            Radius = radius;
            BendStrength = bendStrength;
            FlattenStrength = flattenStrength;
            MovementBlend = movementBlend;
            DirectionMode = directionMode;
            WorldXBias = worldXBias;
            WorldZStrength = worldZStrength;
            Priority = priority;
        }

        public VegetationInteractor Source { get; }
        public Vector2 StartXZ { get; }
        public Vector2 EndXZ { get; }
        public float Radius { get; }
        public float BendStrength { get; }
        public float FlattenStrength { get; }
        public float MovementBlend { get; }
        public VegetationInteractionDirectionMode DirectionMode { get; }
        public float WorldXBias { get; }
        public float WorldZStrength { get; }
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

        [Header("Direction Shaping")]
        [SerializeField]
        [Tooltip("Radial preserves the current radial/movement response. World X Biased uses the fixed map X axis and ignores actor movement direction. Hybrid applies the same world-X bias after the current radial/movement response.")]
        private VegetationInteractionDirectionMode directionMode =
            VegetationInteractionDirectionMode.Radial;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("For World X Biased and Hybrid modes, blends the displacement direction toward fixed world ±X. Zero preserves the mode's source direction; one uses the complete world-X target.")]
        private float worldXBias = 0.85f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("For World X Biased and Hybrid modes, multiplies the final biased world-Z component. Zero removes world-Z displacement regardless of World X Bias; one retains the biased Z component.")]
        private float worldZStrength = 0.20f;

        [Header("Movement Response")]
        [SerializeField, Range(0f, 1f)]
        [Tooltip("For Radial and Hybrid modes, blends radial parting toward the object's movement direction as speed rises. World X Biased ignores actor movement direction.")]
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

        [Header("Historical Trample Trail")]
        [SerializeField]
        [Tooltip("Off writes no historical state. Timed holds the stored state for Recovery Delay Seconds, then returns during Recovery Duration Seconds. Session Persistent lasts until the Ground trample field or scene resets.")]
        private VegetationTrailMode trailMode = VegetationTrailMode.Off;

        [SerializeField, Min(0.05f)]
        [Tooltip("World-space XZ radius of the stored trail. This is independent from the immediate interaction radius.")]
        private float trailRadius = 0.9f;

        [SerializeField, Range(0f, 2f)]
        private float trailBendStrength = 0.8f;

        [SerializeField, Range(0f, 1f)]
        private float trailFlattenStrength = 0.65f;

        [SerializeField, Range(0f, 300f)]
        [Tooltip("How long a Timed trail remains fully displaced before recovery begins. Ignored by Session Persistent mode.")]
        private float recoveryDelaySeconds = 6f;

        [SerializeField, Range(0.05f, 30f)]
        [Tooltip("How long a Timed trail takes to return after its recovery delay. Recovery uses the fixed asymmetric slow-fast-slow curve: approximately 15% restored at 50% time and 90% restored at 90% time. Ignored by Session Persistent mode.")]
        private float recoveryDurationSeconds = 2f;

        [SerializeField, Range(0f, 20f)]
        [Tooltip("Minimum movement speed required before this object writes historical trail state.")]
        private float minimumTrailSpeed = 0.4f;

        [SerializeField, Range(0.01f, 5f)]
        [Tooltip("Minimum accumulated movement between historical swept-capsule writes.")]
        private float trailStampSpacing = 0.2f;

        [SerializeField]
        [Tooltip("Higher-priority historical writers are retained first if a Ground field exceeds its capacity.")]
        private int trailPriority;

        private Vector3 previousSamplePosition;
        private bool sampleInitialized;

        public static IReadOnlyList<VegetationInteractor> ActiveInteractors =>
            ActiveInteractorsInternal;
        public float InteractionRadius => interactionRadius;
        public float BendStrength => bendStrength;
        public float FlattenStrength => flattenStrength;
        public VegetationInteractionDirectionMode DirectionMode => directionMode;
        public float WorldXBias => worldXBias;
        public float WorldZStrength => worldZStrength;
        public float MovementDirectionInfluence => movementDirectionInfluence;
        public float FullMovementResponseSpeed => fullMovementResponseSpeed;
        public float MaximumSweepDistance => maximumSweepDistance;
        public int Priority => priority;
        public VegetationTrailMode TrailMode => trailMode;
        public float TrailRadius => trailRadius;
        public float TrailBendStrength => trailBendStrength;
        public float TrailFlattenStrength => trailFlattenStrength;
        public float TrailRecoveryDelaySeconds => recoveryDelaySeconds;
        public float TrailRecoveryDurationSeconds => recoveryDurationSeconds;
        public float MinimumTrailSpeed => minimumTrailSpeed;
        public float TrailStampSpacing => trailStampSpacing;
        public int TrailPriority => trailPriority;

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
            if (!System.Enum.IsDefined(
                    typeof(VegetationInteractionDirectionMode),
                    directionMode))
            {
                directionMode = VegetationInteractionDirectionMode.Radial;
            }
            worldXBias = Mathf.Clamp01(worldXBias);
            worldZStrength = Mathf.Clamp01(worldZStrength);
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
            if (!System.Enum.IsDefined(typeof(VegetationTrailMode), trailMode))
            {
                trailMode = VegetationTrailMode.Off;
            }
            trailRadius = Mathf.Clamp(trailRadius, 0.05f, 20f);
            trailBendStrength = Mathf.Clamp(trailBendStrength, 0f, 2f);
            trailFlattenStrength = Mathf.Clamp01(trailFlattenStrength);
            recoveryDelaySeconds = Mathf.Clamp(
                recoveryDelaySeconds,
                0f,
                300f);
            recoveryDurationSeconds = Mathf.Clamp(
                recoveryDurationSeconds,
                0.05f,
                30f);
            minimumTrailSpeed = Mathf.Clamp(minimumTrailSpeed, 0f, 20f);
            trailStampSpacing = Mathf.Clamp(trailStampSpacing, 0.01f, 5f);
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
                directionMode,
                worldXBias,
                worldZStrength,
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
            if (trailMode != VegetationTrailMode.Off)
            {
                Gizmos.color = new Color(1f, 0.55f, 0.1f, 0.85f);
                Gizmos.DrawWireSphere(centre, trailRadius);
                Gizmos.color = new Color(0.25f, 1f, 0.35f, 0.85f);
            }
            if (sampleInitialized)
            {
                Gizmos.DrawLine(previousSamplePosition, centre);
            }
            Gizmos.color = previousColor;
        }
    }
}
