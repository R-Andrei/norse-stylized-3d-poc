using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Programmatic Stylized 3D/Rivers/River Disturbance Emitter")]
    public sealed class StylizedRiverDisturbanceEmitter : MonoBehaviour
    {
        [Tooltip("Optional explicit river. Leave empty to locate the active river footprint automatically.")]
        [SerializeField] private StylizedRiver explicitRiver;

        [Tooltip("Automatically chooses the enabled river whose footprint contains this object.")]
        [SerializeField] private bool autoDetectRiver = true;

        [Tooltip("Maximum allowed vertical distance from the mean river surface when automatically detecting contact.")]
        [Min(0.05f)]
        [SerializeField] private float verticalContactTolerance = 1.25f;

        [Tooltip("Approximate water-contact radius in metres. This scales the generic impact, swept wake, or flow-obstruction profile; it does not classify the object as a rock, player, log, or another type.")]
        [Range(0.05f, 8f)]
        [SerializeField] private float radius = 0.35f;

        [Tooltip("Local source strength. Values above the normal production range are available for deliberate visual stress testing.")]
        [Range(0f, 8f)]
        [SerializeField] private float strength = 1f;

        [Tooltip("Contribution to broad geometric wake and ripple height.")]
        [Range(0f, 1f)]
        [SerializeField] private float geometryContribution = 0.65f;

        [Tooltip("Contribution to fine lighting and refraction disturbance without intentionally adding bulk water height.")]
        [Range(0f, 1f)]
        [SerializeField] private float normalContribution = 1f;

        [Tooltip("When movement is slow, this source becomes a generic flow obstruction with an upstream pressure crest and a weaker downstream wake. Disable this for sources that should only react while moving.")]
        [SerializeField] private bool stationaryObstruction = true;

        [Tooltip("Creates one impact after an observed outside-to-inside river transition. Merely enabling an object that already starts in water does not emit an impact.")]
        [SerializeField] private bool emitEntryImpact = true;

        [Tooltip("Creates one smaller impact when the source leaves the river.")]
        [SerializeField] private bool emitExitImpact;

        [Tooltip("CPU source-registration interval. Swept injection bridges movement between samples, so this need not run every frame.")]
        [Range(0.025f, 0.2f)]
        [SerializeField] private float sourceUpdateInterval = 0.05f;

        private StylizedRiverDisturbanceRuntime currentRuntime;
        private Vector3 previousSamplePosition;
        private float updateAccumulator;
        private bool wasInside;
        private bool hasObservedContactState;
        private EntityId SourceId => GetEntityId();

        private void OnEnable()
        {
            previousSamplePosition = transform.position;
            updateAccumulator = sourceUpdateInterval;
            wasInside = false;
            hasObservedContactState = false;
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            updateAccumulator += Time.deltaTime;
            if (updateAccumulator < sourceUpdateInterval)
            {
                return;
            }

            float sampleDelta = Mathf.Max(0.001f, updateAccumulator);
            updateAccumulator = 0f;
            Vector3 currentPosition = transform.position;

            StylizedRiverDisturbanceRuntime resolvedRuntime =
                ResolveRuntime(currentPosition);

            bool isInside = resolvedRuntime != null;

            StylizedRiverDisturbanceRuntime previousRuntime =
                currentRuntime;

            if (resolvedRuntime != currentRuntime)
            {
                previousRuntime?.RemoveContinuousSource(SourceId);
                currentRuntime = resolvedRuntime;
                previousSamplePosition = currentPosition;
            }

            bool observedEntry =
                hasObservedContactState &&
                !wasInside &&
                isInside;

            bool observedExit =
                hasObservedContactState &&
                wasInside &&
                !isInside;

            if (isInside)
            {
                if (observedEntry && emitEntryImpact)
                {
                    currentRuntime.EmitImpact(
                        currentPosition,
                        radius * 1.15f,
                        strength,
                        geometryContribution,
                        normalContribution);
                }

                currentRuntime.UpdateContinuousSource(
                    SourceId,
                    previousSamplePosition,
                    currentPosition,
                    sampleDelta,
                    radius,
                    strength,
                    geometryContribution,
                    normalContribution,
                    stationaryObstruction);
            }
            else if (observedExit && emitExitImpact && previousRuntime != null)
            {
                previousRuntime.EmitImpact(
                    previousSamplePosition,
                    radius,
                    strength * 0.55f,
                    geometryContribution,
                    normalContribution);
            }

            if (!isInside)
            {
                currentRuntime?.RemoveContinuousSource(SourceId);
            }

            hasObservedContactState = true;
            wasInside = isInside;
            previousSamplePosition = currentPosition;
        }

        private void OnDisable()
        {
            currentRuntime?.RemoveContinuousSource(SourceId);
            currentRuntime = null;
            wasInside = false;
            hasObservedContactState = false;
        }

        [ContextMenu("Emit Impact Now")]
        public void EmitImpactNow()
        {
            StylizedRiverDisturbanceRuntime runtime =
                ResolveRuntime(transform.position);

            runtime?.EmitImpact(
                transform.position,
                radius,
                strength,
                geometryContribution,
                normalContribution);
        }

        private StylizedRiverDisturbanceRuntime ResolveRuntime(
            Vector3 worldPosition)
        {
            if (explicitRiver != null)
            {
                StylizedRiverDisturbanceRuntime runtime =
                    explicitRiver.GetOrCreateDisturbanceRuntime();

                if (runtime != null &&
                    explicitRiver.RuntimeDisturbancesEnabled &&
                    explicitRiver.TryProjectWorldPoint(
                        worldPosition,
                        out StylizedRiverProjection projection) &&
                    projection.IsInside &&
                    Mathf.Abs(
                        worldPosition.y -
                        projection.SurfacePoint.y) <=
                    verticalContactTolerance)
                {
                    return runtime;
                }

                return null;
            }

            if (!autoDetectRiver)
            {
                return null;
            }

            return StylizedRiverDisturbanceRuntime.TryFindContainingRiver(
                worldPosition,
                verticalContactTolerance,
                out StylizedRiverDisturbanceRuntime runtimeFound,
                out _)
                ? runtimeFound
                : null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
