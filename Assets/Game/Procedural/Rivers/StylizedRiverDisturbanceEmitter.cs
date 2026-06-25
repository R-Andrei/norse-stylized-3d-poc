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
        private const int CurrentImpactSettingsVersion = 1;

        [Tooltip("Optional fixed river target. When assigned, this emitter submits only to that river and does not choose another river automatically.")]
        [SerializeField] private StylizedRiver explicitRiver;

        [Tooltip("When no Explicit River is assigned, searches enabled rivers and uses the one whose water footprint contains the emitter position within Vertical Contact Tolerance.")]
        [SerializeField] private bool autoDetectRiver = true;

        [Tooltip("Maximum vertical separation, in metres, between the emitter pivot and the detected river surface. Larger values accept objects farther above or below the water; this affects contact detection only.")]
        [Min(0.05f)]
        [SerializeField] private float verticalContactTolerance = 1.25f;

        [SerializeField, HideInInspector]
        private LegacySourceMobility sourceMobility =
            LegacySourceMobility.Dynamic;

        [Header("Source Footprint")]
        [Tooltip("Uses independent half-width and half-length values for the continuous dynamic Wake footprint. Disable this to use one linked half-size in both directions. Impact Ripple radius is configured separately in each event.")]
        [SerializeField] private bool useSeparateFootprintDimensions;

        [FormerlySerializedAs("radius")]
        [Tooltip("Continuous Wake footprint half-size in metres, used both across and along the river when Separate Footprint Dimensions is disabled. This does not set Entry or Exit Impact radius.")]
        [Range(0.05f, 8f)]
        [SerializeField] private float linkedFootprintRadius = 0.35f;

        [Tooltip("Continuous Wake footprint half-width measured across the local river, in metres. This controls source preparation only and does not set Impact Ripple radius.")]
        [Range(0.05f, 12f)]
        [SerializeField] private float acrossFlowHalfWidth = 0.35f;

        [Tooltip("Continuous Wake footprint half-length measured along the local river, in metres. This controls source preparation only and does not set Impact Ripple radius.")]
        [Range(0.05f, 12f)]
        [SerializeField] private float alongFlowHalfLength = 0.35f;

        [Header("Influence")]
        [Tooltip("Local continuous-Wake multiplier applied before the river's canonical Wake Strength. Zero disables this emitter's continuous Wake; higher values strengthen only this source. It does not affect Entry or Exit Impact Ripples.")]
        [Range(0f, 8f)]
        [SerializeField] private float strength = 1f;

        [Tooltip("Scales this emitter's continuous Wake geometry contribution. Zero keeps the source from adding Wake height; it does not change the independent Impact Ripple event profiles.")]
        [Range(0f, 1f)]
        [SerializeField] private float geometryContribution = 0.65f;

        [Tooltip("Scales this emitter's continuous Wake normal/intensity contribution used by lighting and refraction. It does not change the independent Impact Ripple event profiles.")]
        [Range(0f, 1f)]
        [SerializeField] private float normalContribution = 1f;

        [Tooltip("When enabled, a slow or stopped emitter blends toward the runtime's stationary-obstruction Wake source instead of behaving as a fully moving trail. Disable it when Wake should depend only on movement.")]
        [SerializeField] private bool stationaryObstruction = true;

        [Header("Impact Ripples")]
        [Tooltip("Emits the Entry Impact profile once after the runtime observes an outside-to-inside river transition. Starting or enabling the component while already inside water intentionally does not count as an entry.")]
        [SerializeField] private bool emitEntryImpact = true;

        [Tooltip("Independent Entry Impact profile: starting radius, signed impulse, immediate elevation, shape, sharpness, and geometry/normal contributions. These values do not modify the continuous Wake source.")]
        [SerializeField]
        private ImpactRippleEventSettings entryImpact =
            ImpactRippleEventSettings.CreateEntryDefaults();

        [Tooltip("Emits the Exit / Suction profile once after an observed inside-to-outside river transition. Use a negative Signed Impulse for suction-like behavior.")]
        [SerializeField] private bool emitExitImpact;

        [Tooltip("Independent Exit / Suction profile: starting radius, signed impulse, immediate elevation, shape, sharpness, and geometry/normal contributions. These values do not modify the continuous Wake source.")]
        [SerializeField]
        private ImpactRippleEventSettings exitImpact =
            ImpactRippleEventSettings.CreateExitDefaults();

        [SerializeField, HideInInspector]
        private int impactSettingsVersion;

        [Header("Runtime Sampling")]
        [Tooltip("Seconds between CPU contact and movement samples. Lower values react sooner but cost more CPU work; swept Wake submission bridges the path between samples so movement is not reduced to isolated points.")]
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
            MigrateImpactSettingsIfRequired();

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
            MigrateImpactSettingsIfRequired();

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
            entryImpact = entryImpact.Sanitized();
            exitImpact = exitImpact.Sanitized();
            sourceUpdateInterval = Mathf.Clamp(
                sourceUpdateInterval,
                0.025f,
                0.2f);
        }

        private void MigrateImpactSettingsIfRequired()
        {
            if (impactSettingsVersion >= CurrentImpactSettingsVersion)
            {
                return;
            }

            entryImpact = new ImpactRippleEventSettings(
                ResolvedImpactRadius * 1.15f,
                strength,
                0f,
                ImpactRippleEventSettings.LegacyShape,
                ImpactRippleEventSettings.LegacySharpness,
                geometryContribution,
                normalContribution);
            exitImpact = new ImpactRippleEventSettings(
                ResolvedImpactRadius,
                -strength * 0.55f,
                0f,
                ImpactRippleEventSettings.LegacyShape,
                ImpactRippleEventSettings.LegacySharpness,
                geometryContribution,
                normalContribution);
            impactSettingsVersion = CurrentImpactSettingsVersion;
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
                        entryImpact);
                }

                // The emitter submits only source-local movement, footprint,
                // and influence. The detecting river applies the canonical
                // Wake settings shared with stationary geometry.
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
                    exitImpact);
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
                entryImpact);
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
