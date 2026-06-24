using ProgrammaticStylized3D.Geometry;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProgrammaticStylized3D.Rivers
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Programmatic Stylized 3D/Rivers/River Disturbance Emitter")]
    public sealed class StylizedRiverDisturbanceEmitter : MonoBehaviour
    {
        private enum LegacySourceMobility
        {
            Static,
            Dynamic
        }

        private const float MinimumFootprintHalfExtent = 0.05f;

        [Tooltip("Optional explicit river. Leave empty to locate the active river footprint automatically.")]
        [SerializeField] private StylizedRiver explicitRiver;

        [Tooltip("Automatically chooses the enabled river whose footprint contains this object.")]
        [SerializeField] private bool autoDetectRiver = true;

        [Tooltip("Maximum allowed vertical distance from the mean river surface when automatically detecting contact.")]
        [Min(0.05f)]
        [SerializeField] private float verticalContactTolerance = 1.25f;

        [SerializeField, HideInInspector]
        private LegacySourceMobility sourceMobility =
            LegacySourceMobility.Dynamic;

        [Header("Source Footprint")]
        [Tooltip("Uses separate across-flow and along-flow dimensions. Disable this to use one linked radius for compact dynamic sources.")]
        [SerializeField] private bool useSeparateFootprintDimensions;

        [FormerlySerializedAs("radius")]
        [Tooltip("Linked water-contact half-size in metres. Used for both footprint dimensions when Separate Footprint Dimensions is disabled.")]
        [Range(0.05f, 8f)]
        [SerializeField] private float linkedFootprintRadius = 0.35f;

        [Tooltip("Half-width of the water-contact footprint measured across the river, in metres.")]
        [Range(0.05f, 12f)]
        [SerializeField] private float acrossFlowHalfWidth = 0.35f;

        [Tooltip("Half-length of the water-contact footprint measured along the river, in metres.")]
        [Range(0.05f, 12f)]
        [SerializeField] private float alongFlowHalfLength = 0.35f;

        [Header("Influence")]
        [Tooltip("Local source strength. Values above the normal production range are available for deliberate visual stress testing.")]
        [Range(0f, 8f)]
        [SerializeField] private float strength = 1f;

        [Tooltip("Contribution to broad geometric wake and ripple height.")]
        [Range(0f, 1f)]
        [SerializeField] private float geometryContribution = 0.65f;

        [Tooltip("Contribution to fine lighting and refraction disturbance without intentionally adding bulk water height.")]
        [Range(0f, 1f)]
        [SerializeField] private float normalContribution = 1f;

        [Tooltip("When movement is slow, the dynamic source becomes a generic flow obstruction.")]
        [SerializeField] private bool stationaryObstruction = true;

        [Tooltip("Creates one impact after an observed outside-to-inside river transition. Merely enabling a source already in water does not emit an impact.")]
        [SerializeField] private bool emitEntryImpact = true;

        [Tooltip("Creates one smaller impact when the source leaves the river.")]
        [SerializeField] private bool emitExitImpact;

        [Tooltip("CPU source-registration interval. Swept injection bridges movement between samples.")]
        [Range(0.025f, 0.2f)]
        [SerializeField] private float sourceUpdateInterval = 0.05f;

        private StylizedRiverDisturbanceRuntime currentRuntime;
        private Vector3 previousSamplePosition;
        private float updateAccumulator;
        private bool wasInside;
        private bool hasObservedContactState;
        private bool legacyStaticWarningIssued;

        private EntityId SourceId => GetEntityId();

        private bool IsLegacyStaticEmitter =>
            sourceMobility == LegacySourceMobility.Static;

        private float ManualAcrossHalfWidth =>
            Mathf.Max(
                MinimumFootprintHalfExtent,
                useSeparateFootprintDimensions
                    ? acrossFlowHalfWidth
                    : linkedFootprintRadius);

        private float ManualAlongHalfLength =>
            Mathf.Max(
                MinimumFootprintHalfExtent,
                useSeparateFootprintDimensions
                    ? alongFlowHalfLength
                    : linkedFootprintRadius);

        private float ResolvedImpactRadius =>
            Mathf.Max(ManualAcrossHalfWidth, ManualAlongHalfLength);

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                WarnIfLegacyStaticEmitter();
            }

            previousSamplePosition = transform.position;
            updateAccumulator = sourceUpdateInterval;
            wasInside = false;
            hasObservedContactState = false;
        }

        private void OnValidate()
        {
            verticalContactTolerance = Mathf.Max(
                0.05f,
                verticalContactTolerance);
            linkedFootprintRadius = Mathf.Clamp(
                linkedFootprintRadius,
                0.05f,
                8f);
            acrossFlowHalfWidth = Mathf.Clamp(
                acrossFlowHalfWidth,
                0.05f,
                12f);
            alongFlowHalfLength = Mathf.Clamp(
                alongFlowHalfLength,
                0.05f,
                12f);
            strength = Mathf.Clamp(strength, 0f, 8f);
            geometryContribution = Mathf.Clamp01(geometryContribution);
            normalContribution = Mathf.Clamp01(normalContribution);
            sourceUpdateInterval = Mathf.Clamp(
                sourceUpdateInterval,
                0.025f,
                0.2f);
        }

        private void Update()
        {
            if (!Application.isPlaying || IsLegacyStaticEmitter)
            {
                return;
            }

            UpdateDynamicSource();
        }

        private void UpdateDynamicSource()
        {
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
            StylizedRiverDisturbanceRuntime previousRuntime =
                currentRuntime;
            bool isInside = resolvedRuntime != null;

            if (resolvedRuntime != currentRuntime)
            {
                ChangeRuntime(resolvedRuntime);
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
                        ResolvedImpactRadius * 1.15f,
                        strength,
                        geometryContribution,
                        normalContribution);
                }

                currentRuntime.UpdateContinuousSource(
                    SourceId,
                    previousSamplePosition,
                    currentPosition,
                    sampleDelta,
                    ManualAcrossHalfWidth,
                    ManualAlongHalfLength,
                    strength,
                    geometryContribution,
                    normalContribution,
                    stationaryObstruction);
            }
            else if (observedExit && emitExitImpact && previousRuntime != null)
            {
                previousRuntime.EmitImpact(
                    previousSamplePosition,
                    ResolvedImpactRadius,
                    strength * 0.55f,
                    geometryContribution,
                    normalContribution);
            }

            hasObservedContactState = true;
            wasInside = isInside;
            previousSamplePosition = currentPosition;
        }

        private void OnDisable()
        {
            ChangeRuntime(null);
            wasInside = false;
            hasObservedContactState = false;
        }

        [ContextMenu("Emit Impact Now")]
        public void EmitImpactNow()
        {
            if (IsLegacyStaticEmitter)
            {
                WarnIfLegacyStaticEmitter();
                return;
            }

            StylizedRiverDisturbanceRuntime runtime =
                ResolveRuntime(transform.position);

            runtime?.EmitImpact(
                transform.position,
                ResolvedImpactRadius,
                strength,
                geometryContribution,
                normalContribution);
        }

        private void WarnIfLegacyStaticEmitter()
        {
            if (!IsLegacyStaticEmitter || legacyStaticWarningIssued)
            {
                return;
            }

            legacyStaticWarningIssued = true;
            Debug.LogWarning(
                "Legacy Static River Disturbance Emitter is obsolete and inert. " +
                "Stationary geometry is handled through the generated-geometry " +
                "registry. Remove this component.",
                this);
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

        private void ChangeRuntime(
            StylizedRiverDisturbanceRuntime nextRuntime)
        {
            if (nextRuntime == currentRuntime)
            {
                return;
            }

            currentRuntime?.RemoveContinuousSource(SourceId);
            currentRuntime = nextRuntime;
        }

        private void OnDrawGizmosSelected()
        {
            if (IsLegacyStaticEmitter)
            {
                return;
            }

            Gizmos.DrawWireSphere(
                transform.position,
                Mathf.Max(
                    ManualAcrossHalfWidth,
                    ManualAlongHalfLength));
        }
    }
}
