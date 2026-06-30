using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ProgrammaticStylized3D.Geometry;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    public readonly struct GeneratedRiverDisturbanceDiagnostics
    {
        public GeneratedRiverDisturbanceDiagnostics(
            StylizedRiver river,
            bool active,
            float acrossWidth,
            float alongLength,
            float localRiverWidth,
            float blockageRatio,
            float effectivePadding,
            float effectiveAmplitude,
            float effectiveWakeStrength,
            float maximumAllowedAmplitude,
            bool heightClampReached,
            float representativeSupportHeight,
            float pressureMinimumHeight,
            float pressureMaximumHeight,
            float pressureStrength,
            float waveAllowance,
            bool staticPressureEnabled,
            float contactSharpness,
            float waveResponse,
            bool obstructionWakeEnabled,
            float obstructionWakeReach,
            float obstructionWakeSpread,
            string status)
            : this(
                river,
                active,
                acrossWidth,
                alongLength,
                localRiverWidth,
                blockageRatio,
                effectivePadding,
                effectiveAmplitude,
                effectiveWakeStrength,
                maximumAllowedAmplitude,
                heightClampReached,
                representativeSupportHeight,
                pressureMinimumHeight,
                pressureMaximumHeight,
                pressureStrength,
                waveAllowance,
                staticPressureEnabled,
                contactSharpness,
                waveResponse,
                obstructionWakeEnabled,
                obstructionWakeReach,
                obstructionWakeSpread,
                0.35f,
                status)
        {
        }

        public GeneratedRiverDisturbanceDiagnostics(
            StylizedRiver river,
            bool active,
            float acrossWidth,
            float alongLength,
            float localRiverWidth,
            float blockageRatio,
            float effectivePadding,
            float effectiveAmplitude,
            float effectiveWakeStrength,
            float maximumAllowedAmplitude,
            bool heightClampReached,
            float representativeSupportHeight,
            float pressureMinimumHeight,
            float pressureMaximumHeight,
            float pressureStrength,
            float waveAllowance,
            bool staticPressureEnabled,
            float contactSharpness,
            float waveResponse,
            bool obstructionWakeEnabled,
            float obstructionWakeReach,
            float obstructionWakeSpread,
            float obstructionWakeVariation,
            string status)
            : this(
                river,
                active,
                acrossWidth,
                alongLength,
                localRiverWidth,
                blockageRatio,
                effectivePadding,
                effectiveAmplitude,
                effectiveWakeStrength,
                maximumAllowedAmplitude,
                heightClampReached,
                representativeSupportHeight,
                pressureMinimumHeight,
                pressureMaximumHeight,
                pressureStrength,
                waveAllowance,
                0f,
                staticPressureEnabled,
                contactSharpness,
                waveResponse,
                obstructionWakeEnabled,
                obstructionWakeReach,
                obstructionWakeSpread,
                obstructionWakeVariation,
                status)
        {
        }

        public GeneratedRiverDisturbanceDiagnostics(
            StylizedRiver river,
            bool active,
            float acrossWidth,
            float alongLength,
            float localRiverWidth,
            float blockageRatio,
            float effectivePadding,
            float effectiveAmplitude,
            float effectiveWakeStrength,
            float maximumAllowedAmplitude,
            bool heightClampReached,
            float representativeSupportHeight,
            float pressureMinimumHeight,
            float pressureMaximumHeight,
            float pressureStrength,
            float waveAllowance,
            float supportInspectionHeight,
            bool staticPressureEnabled,
            float contactSharpness,
            float waveResponse,
            bool obstructionWakeEnabled,
            float obstructionWakeReach,
            float obstructionWakeSpread,
            string status)
            : this(
                river,
                active,
                acrossWidth,
                alongLength,
                localRiverWidth,
                blockageRatio,
                effectivePadding,
                effectiveAmplitude,
                effectiveWakeStrength,
                maximumAllowedAmplitude,
                heightClampReached,
                representativeSupportHeight,
                pressureMinimumHeight,
                pressureMaximumHeight,
                pressureStrength,
                waveAllowance,
                supportInspectionHeight,
                staticPressureEnabled,
                contactSharpness,
                waveResponse,
                obstructionWakeEnabled,
                obstructionWakeReach,
                obstructionWakeSpread,
                0.35f,
                status)
        {
        }

        public GeneratedRiverDisturbanceDiagnostics(
            StylizedRiver river,
            bool active,
            float acrossWidth,
            float alongLength,
            float localRiverWidth,
            float blockageRatio,
            float effectivePadding,
            float effectiveAmplitude,
            float effectiveWakeStrength,
            float maximumAllowedAmplitude,
            bool heightClampReached,
            float representativeSupportHeight,
            float pressureMinimumHeight,
            float pressureMaximumHeight,
            float pressureStrength,
            float waveAllowance,
            float supportInspectionHeight,
            bool staticPressureEnabled,
            float contactSharpness,
            float waveResponse,
            bool obstructionWakeEnabled,
            float obstructionWakeReach,
            float obstructionWakeSpread,
            float obstructionWakeVariation,
            string status)
        {
            River = river;
            Active = active;
            AcrossWidth = acrossWidth;
            AlongLength = alongLength;
            LocalRiverWidth = localRiverWidth;
            BlockageRatio = blockageRatio;
            EffectivePadding = effectivePadding;
            EffectiveAmplitude = effectiveAmplitude;
            EffectiveWakeStrength = effectiveWakeStrength;
            MaximumAllowedAmplitude = maximumAllowedAmplitude;
            HeightClampReached = heightClampReached;
            RepresentativeSupportHeight = representativeSupportHeight;
            PressureMinimumHeight = pressureMinimumHeight;
            PressureMaximumHeight = pressureMaximumHeight;
            PressureStrength = pressureStrength;
            WaveAllowance = waveAllowance;
            SupportInspectionHeight = supportInspectionHeight;
            StaticPressureEnabled = staticPressureEnabled;
            ContactSharpness = contactSharpness;
            ProfileVariation = waveResponse;
            ObstructionWakeEnabled = obstructionWakeEnabled;
            ObstructionWakeReach = obstructionWakeReach;
            ObstructionWakeSpread = obstructionWakeSpread;
            ObstructionWakeVariation = obstructionWakeVariation;
            Status = status ?? string.Empty;
        }

        public StylizedRiver River { get; }
        public bool Active { get; }
        public float AcrossWidth { get; }
        public float AlongLength { get; }
        public float LocalRiverWidth { get; }
        public float BlockageRatio { get; }
        public float EffectivePadding { get; }
        public float EffectiveAmplitude { get; }
        public float EffectiveWakeStrength { get; }
        public float MaximumAllowedAmplitude { get; }
        public bool HeightClampReached { get; }
        public float RepresentativeSupportHeight { get; }
        public float PressureMinimumHeight { get; }
        public float PressureMaximumHeight { get; }
        public float PressureStrength { get; }
        public float WaveAllowance { get; }
        public float SupportInspectionHeight { get; }
        public bool StaticPressureEnabled { get; }
        public float ContactSharpness { get; }
        public float ProfileVariation { get; }

        // Compatibility alias for diagnostics consumers compiled against the
        // previous wave-triggered profile implementation.
        public float WaveResponse => ProfileVariation;

        public bool ObstructionWakeEnabled { get; }
        public float ObstructionWakeReach { get; }
        public float ObstructionWakeSpread { get; }
        public float ObstructionWakeVariation { get; }
        public string Status { get; }
    }

    public readonly struct RiverObstacleExclusionFootprint
    {
        public RiverObstacleExclusionFootprint(
            EntityId sourceId,
            float globalDistance,
            float acrossMetres,
            float surfaceHalfWidth,
            float alongHalfLength,
            float acrossHalfWidth,
            Vector2[] contour)
        {
            SourceId = sourceId;
            GlobalDistance = globalDistance;
            AcrossMetres = acrossMetres;
            SurfaceHalfWidth = surfaceHalfWidth;
            AlongHalfLength = alongHalfLength;
            AcrossHalfWidth = acrossHalfWidth;
            Contour = contour ?? Array.Empty<Vector2>();
        }

        public EntityId SourceId { get; }
        public float GlobalDistance { get; }
        public float AcrossMetres { get; }
        public float SurfaceHalfWidth { get; }
        public float AlongHalfLength { get; }
        public float AcrossHalfWidth { get; }
        public Vector2[] Contour { get; }
    }

#if UNITY_EDITOR
    public readonly struct GeneratedRiverPressureProfileDebugData
    {
        public GeneratedRiverPressureProfileDebugData(
            StylizedRiver river,
            Vector3 worldPosition,
            float acrossHalfWidth,
            float requestedProfileWidthPixels,
            int lateralSampleCount,
            int verticalSupportSlices,
            float supportInspectionHeight,
            float targetHeight,
            float supportModulationReserve,
            int validRowCount,
            int supportLimitedBelowTargetRowCount,
            int endpointTaperRowCount,
            int targetHeightRowCount,
            Vector2 cachedBaseHeightRange,
            Vector2 currentHeightRange,
            Vector2 localCeilingRange,
            Vector2 currentMultiplierRange,
            Vector2 interiorBaseHeightRange,
            Vector2 interiorCeilingRange,
            float maximumAdjacentBaseHeightDifference,
            float maximumAdjacentCurrentHeightDifference,
            float maximumAdjacentBaseContactShift,
            float maximumAdjacentCurrentContactShift,
            Vector2 rowThicknessRange,
            float medianRowThickness,
            float maximumResolvedCrestDepthPercent,
            float maximumResolvedPressureEndDepthPercent,
            int geometryClampedRowCount,
            int protectedDownstreamRegionViolationRowCount,
            float protectedDownstreamStartPercent,
            Vector2 appliedMultiplierBounds,
            Vector4[] baseSamples,
            Vector4[] currentSamples,
            float[] downstreamBoundaries)
        {
            River = river;
            WorldPosition = worldPosition;
            AcrossHalfWidth = acrossHalfWidth;
            RequestedProfileWidthPixels = requestedProfileWidthPixels;
            LateralSampleCount = lateralSampleCount;
            VerticalSupportSlices = verticalSupportSlices;
            SupportInspectionHeight = supportInspectionHeight;
            TargetHeight = targetHeight;
            SupportModulationReserve = supportModulationReserve;
            ValidRowCount = validRowCount;
            SupportLimitedBelowTargetRowCount =
                supportLimitedBelowTargetRowCount;
            EndpointTaperRowCount = endpointTaperRowCount;
            TargetHeightRowCount = targetHeightRowCount;
            CachedBaseHeightRange = cachedBaseHeightRange;
            CurrentHeightRange = currentHeightRange;
            LocalCeilingRange = localCeilingRange;
            CurrentMultiplierRange = currentMultiplierRange;
            InteriorBaseHeightRange = interiorBaseHeightRange;
            InteriorCeilingRange = interiorCeilingRange;
            MaximumAdjacentBaseHeightDifference =
                maximumAdjacentBaseHeightDifference;
            MaximumAdjacentCurrentHeightDifference =
                maximumAdjacentCurrentHeightDifference;
            MaximumAdjacentBaseContactShift =
                maximumAdjacentBaseContactShift;
            MaximumAdjacentCurrentContactShift =
                maximumAdjacentCurrentContactShift;
            RowThicknessRange = rowThicknessRange;
            MedianRowThickness = medianRowThickness;
            MaximumResolvedCrestDepthPercent =
                maximumResolvedCrestDepthPercent;
            MaximumResolvedPressureEndDepthPercent =
                maximumResolvedPressureEndDepthPercent;
            GeometryClampedRowCount = geometryClampedRowCount;
            ProtectedDownstreamRegionViolationRowCount =
                protectedDownstreamRegionViolationRowCount;
            ProtectedDownstreamStartPercent =
                protectedDownstreamStartPercent;
            AppliedMultiplierBounds = appliedMultiplierBounds;
            BaseSamples = baseSamples ?? Array.Empty<Vector4>();
            CurrentSamples = currentSamples ?? Array.Empty<Vector4>();
            DownstreamBoundaries = downstreamBoundaries ??
                Array.Empty<float>();
        }

        public StylizedRiver River { get; }
        public Vector3 WorldPosition { get; }
        public float AcrossHalfWidth { get; }
        public float RequestedProfileWidthPixels { get; }
        public int LateralSampleCount { get; }
        public int VerticalSupportSlices { get; }
        public float SupportInspectionHeight { get; }
        public float TargetHeight { get; }
        public float SupportModulationReserve { get; }
        public int ValidRowCount { get; }
        public int SupportLimitedBelowTargetRowCount { get; }
        public int EndpointTaperRowCount { get; }
        public int TargetHeightRowCount { get; }
        public Vector2 CachedBaseHeightRange { get; }
        public Vector2 CurrentHeightRange { get; }
        public Vector2 LocalCeilingRange { get; }
        public Vector2 CurrentMultiplierRange { get; }
        public Vector2 InteriorBaseHeightRange { get; }
        public Vector2 InteriorCeilingRange { get; }
        public float MaximumAdjacentBaseHeightDifference { get; }
        public float MaximumAdjacentCurrentHeightDifference { get; }
        public float MaximumAdjacentBaseContactShift { get; }
        public float MaximumAdjacentCurrentContactShift { get; }
        public Vector2 RowThicknessRange { get; }
        public float MedianRowThickness { get; }
        public float MaximumResolvedCrestDepthPercent { get; }
        public float MaximumResolvedPressureEndDepthPercent { get; }
        public int GeometryClampedRowCount { get; }
        public int ProtectedDownstreamRegionViolationRowCount { get; }
        public float ProtectedDownstreamStartPercent { get; }
        public Vector2 AppliedMultiplierBounds { get; }
        public Vector4[] BaseSamples { get; }
        public Vector4[] CurrentSamples { get; }
        public float[] DownstreamBoundaries { get; }

        public bool IsValid =>
            River != null &&
            LateralSampleCount > 0 &&
            BaseSamples != null &&
            CurrentSamples != null &&
            DownstreamBoundaries != null &&
            BaseSamples.Length == LateralSampleCount &&
            CurrentSamples.Length == LateralSampleCount &&
            DownstreamBoundaries.Length == LateralSampleCount;
    }

#endif


    [Serializable]
    public struct ImpactRippleEventSettings
    {
        public const float MinimumRadius = 0.05f;
        public const float MaximumRadius = 12f;
        public const float MinimumSignedImpulse = -8f;
        public const float MaximumSignedImpulse = 8f;
        public const float MinimumInitialElevation = -0.40f;
        public const float MaximumInitialElevation = 0.40f;
        public const float MinimumSharpness = 0.25f;
        public const float MaximumSharpness = 4f;
        public const float LegacyShape = 0.5f;
        public const float LegacySharpness = 1f;

        [Tooltip("Initial injection radius in world metres. This sets the starting footprint only; river Propagation, Decay, and Flow Dissipation determine how far the ripple later travels and expands.")]
        [Range(MinimumRadius, MaximumRadius)]
        [SerializeField] private float radius;

        [Tooltip("Signed velocity-like kick, multiplied by the river's Impact Ripple Strength. Positive values use the entry/impact polarity; negative values reverse the pattern for withdrawal or suction. Zero leaves only Initial Elevation and any independent normal contribution.")]
        [Range(MinimumSignedImpulse, MaximumSignedImpulse)]
        [SerializeField] private float signedImpulse;

        [Tooltip("Immediate signed surface displacement in metres, multiplied by river Strength and Geometry Contribution. Positive raises the initial centre region; negative lowers it. This is independent from the velocity-like Signed Impulse.")]
        [Range(MinimumInitialElevation, MaximumInitialElevation)]
        [SerializeField] private float initialElevation;

        [Tooltip("Balances the analytic centre and ring pattern. Lower values pull the ring inward and emphasize the centre depression; higher values move and strengthen the ring while reducing the centre depression. A value of 0.5 preserves the original Stage 5 shape.")]
        [Range(0f, 1f)]
        [SerializeField] private float shape;

        [Tooltip("Controls concentration inside the same Radius. Lower values make the centre and rings broader and softer; higher values make them narrower and sharper. This does not change propagation speed or final lifetime.")]
        [Range(MinimumSharpness, MaximumSharpness)]
        [SerializeField] private float sharpness;

        [Tooltip("Scales the event's height, initial elevation, and propagated velocity channels. Zero removes persistent geometric wave motion; a nonzero Normal Contribution can still create normal-only detail.")]
        [Range(0f, 1f)]
        [SerializeField] private float geometryContribution;

        [Tooltip("Scales the event's normal-only detail used by lighting and refraction. This does not add bulk water height or propagated velocity.")]
        [Range(0f, 1f)]
        [SerializeField] private float normalContribution;

        public float Radius => radius;
        public float SignedImpulse => signedImpulse;
        public float InitialElevation => initialElevation;
        public float Shape => shape;
        public float Sharpness => sharpness;
        public float GeometryContribution => geometryContribution;
        public float NormalContribution => normalContribution;

        public ImpactRippleEventSettings(
            float radius,
            float signedImpulse,
            float initialElevation,
            float shape,
            float sharpness,
            float geometryContribution,
            float normalContribution)
        {
            this.radius = radius;
            this.signedImpulse = signedImpulse;
            this.initialElevation = initialElevation;
            this.shape = shape;
            this.sharpness = sharpness;
            this.geometryContribution = geometryContribution;
            this.normalContribution = normalContribution;
        }

        public ImpactRippleEventSettings WithSignsReversed()
        {
            return new ImpactRippleEventSettings(
                radius,
                -signedImpulse,
                -initialElevation,
                shape,
                sharpness,
                geometryContribution,
                normalContribution);
        }

        public ImpactRippleEventSettings Sanitized()
        {
            return new ImpactRippleEventSettings(
                Mathf.Clamp(radius, MinimumRadius, MaximumRadius),
                Mathf.Clamp(
                    signedImpulse,
                    MinimumSignedImpulse,
                    MaximumSignedImpulse),
                Mathf.Clamp(
                    initialElevation,
                    MinimumInitialElevation,
                    MaximumInitialElevation),
                Mathf.Clamp01(shape),
                Mathf.Clamp(
                    sharpness,
                    MinimumSharpness,
                    MaximumSharpness),
                Mathf.Clamp01(geometryContribution),
                Mathf.Clamp01(normalContribution));
        }

        public static ImpactRippleEventSettings CreateEntryDefaults()
        {
            return new ImpactRippleEventSettings(
                0.40f,
                1f,
                0f,
                LegacyShape,
                LegacySharpness,
                0.65f,
                1f);
        }

        public static ImpactRippleEventSettings CreateExitDefaults()
        {
            return new ImpactRippleEventSettings(
                0.35f,
                -0.55f,
                0f,
                LegacyShape,
                LegacySharpness,
                0.65f,
                1f);
        }

        public static ImpactRippleEventSettings CreateLegacy(
            float radius,
            float strength,
            float geometryContribution,
            float normalContribution)
        {
            return new ImpactRippleEventSettings(
                radius,
                strength,
                0f,
                LegacyShape,
                LegacySharpness,
                geometryContribution,
                normalContribution);
        }
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(StylizedRiver))]
    [AddComponentMenu("")]
    public sealed class StylizedRiverDisturbanceRuntime : MonoBehaviour
    {
        private const string ComputeResourcePath =
            "PS3DRiver/Compute/CS_RiverDisturbance";
        private const float ChunkLengthMetres = 32f;
        private const int ThreadGroupSize = 8;
        private const float StationarySpeedStart = 0.08f;
        private const float MovingSpeedFull = 0.45f;
        private const double SourceStaleSeconds = 0.35;
        private const float StaticOnlySimulationRate = 12f;
        public const float MaximumStaticPressureHeightMetres = 1.25f;
        private const float MaximumStaticPressureModulation = 1.16f;
        private const float RippleStabilitySafety = 0.42f;
        private const float RippleInjectionEnvelopeRadius = 1.15f;
        private const int MaximumRippleSubsteps = 32;
        private const double RippleSubstepDiagnosticWindowSeconds = 5.0;
        private const double PerformanceDiagnosticWindowSeconds = 5.0;
        private const float MinimumImpactReservationLifetime = 0.25f;
        private const float RippleReservationLookAheadSteps = 2f;
        private const float RippleReservationPaddingCells = 2f;
        private const float GoldenPhaseStep = 0.61803398875f;
        private const float AutomaticBoundsHorizontalPadding = 0.5f;
        private const float DefaultGeneratedFootprintPadding = 0.12f;
        private const float AutomaticBoundsVerticalPadding = 1.25f;
        private const int GeneratedSourcesPerFrame = 1;
        private const float StaticPressureProfileUpdateRate = 12f;
        private const float StaticPressureProfileTransitionFraction = 0.85f;
        private const float StaticWakeVariationUpdateRate = 12f;
        private const float StaticWakeVariationTransitionFraction = 0.85f;
        private const float StaticPressureMinimumProfileMultiplier = 0.58f;
        private const int MaximumStaticContourPoints =
            RiverDisturbanceFootprintResolver.MaximumContourPoints;

        private enum PerformanceDispatchCategory
        {
            RippleSimulation,
            WakeSimulation,
            ImpactInjection,
            WakeInjection,
            StaticPressureBake,
            StaticWakeBake,
            RippleBoundaryBake,
            Clear
        }

        private static uint sourcePhaseSequence = 1;

        private static readonly List<StylizedRiverDisturbanceRuntime>
            ActiveRuntimes = new();
        private static readonly Dictionary<EntityId, GeneratedRiverDisturbanceDiagnostics>
            GeneratedSourceDiagnostics = new();

        private static readonly int DisturbanceEnabledId =
            Shader.PropertyToID("_DisturbanceEnabled");
        private static readonly int DisturbancePreviousId =
            Shader.PropertyToID("_DisturbanceFieldPrevious");
        private static readonly int DisturbanceCurrentId =
            Shader.PropertyToID("_DisturbanceFieldCurrent");
        private static readonly int DisturbanceStaticTargetId =
            Shader.PropertyToID("_DisturbanceStaticTarget");
        private static readonly int DisturbanceStaticWakeSourceId =
            Shader.PropertyToID("_DisturbanceStaticWakeSource");
        private static readonly int DisturbanceRippleBoundaryId =
            Shader.PropertyToID("_DisturbanceRippleBoundary");
        private static readonly int DisturbanceStaticWakeTexelSizeId =
            Shader.PropertyToID("_DisturbanceStaticWakeTexelSize");
        private static readonly int DisturbanceWakePreviousId =
            Shader.PropertyToID("_DisturbanceWakePrevious");
        private static readonly int DisturbanceWakeCurrentId =
            Shader.PropertyToID("_DisturbanceWakeCurrent");
        private static readonly int DisturbanceWakeInterpolationId =
            Shader.PropertyToID("_DisturbanceWakeInterpolation");
        private static readonly int DisturbanceInterpolationId =
            Shader.PropertyToID("_DisturbanceInterpolation");
        private static readonly int DisturbanceGlobalStartId =
            Shader.PropertyToID("_DisturbanceGlobalStart");
        private static readonly int DisturbanceFieldLengthId =
            Shader.PropertyToID("_DisturbanceFieldLength");
        private static readonly int DisturbanceGeometryStrengthId =
            Shader.PropertyToID("_DisturbanceGeometryStrength");
        private static readonly int DisturbanceNormalStrengthId =
            Shader.PropertyToID("_DisturbanceNormalStrength");
        private static readonly int DisturbanceShoreInteractionId =
            Shader.PropertyToID("_DisturbanceShoreInteraction");
        private static readonly int DisturbanceMaximumHeightId =
            Shader.PropertyToID("_DisturbanceMaximumHeight");
        private static readonly int DisturbanceStaticMaximumHeightId =
            Shader.PropertyToID("_DisturbanceStaticMaximumHeight");
        private static readonly int DisturbanceWakeGeometryHeightId =
            Shader.PropertyToID("_DisturbanceWakeGeometryHeight");
        private static readonly int DisturbanceWakeGeometryCompactnessId =
            Shader.PropertyToID("_DisturbanceWakeGeometryCompactness");
        private static readonly int DisturbanceDebugViewId =
            Shader.PropertyToID("_DisturbanceDebugView");
        private static readonly int DisturbanceFragmentDetailId =
            Shader.PropertyToID("_DisturbanceFragmentDetail");

        private readonly Dictionary<EntityId, ContinuousSource> continuousSources =
            new();
        private readonly Dictionary<EntityId, EntityId>
            continuousSourceIdsByOwner = new();
        private readonly HashSet<EntityId> ownershipConflictWarningOwnerIds =
            new();
        private readonly List<EntityId> staleSourceIds = new();
        private readonly List<EntityId> staticPressureProfileSourceIds = new();
        private readonly List<EntityId> staticWakeVariationSourceIds = new();
        private readonly List<ImpactCommand> pendingImpacts = new();
        private readonly List<ImpactReservation> activeImpactReservations =
            new();
        private readonly List<IGeneratedGeometrySource>
            generatedGeometryScratch = new();
        private readonly HashSet<EntityId> automaticGeneratedSourceIds =
            new();
        private readonly HashSet<EntityId>
            refreshedAutomaticGeneratedSourceIds = new();
        private readonly Vector4[] staticContourUpload =
            new Vector4[MaximumStaticContourPoints];
        private readonly Vector4[] staticPressureProfileUpload =
            new Vector4[
                RiverDisturbanceFootprintResolver.
                    MaximumPressureSupportLateralSamples];
        private readonly Vector4[] staticPressureGeometryUpload =
            new Vector4[
                RiverDisturbanceFootprintResolver.
                    MaximumPressureSupportLateralSamples];
        private readonly Vector4[] staticWakeVariationProfileUpload =
            new Vector4[
                RiverDisturbanceFootprintResolver.
                    MaximumPressureSupportLateralSamples];

        private StylizedRiver river;
        private MeshRenderer surfaceRenderer;
        private MaterialPropertyBlock propertyBlock;
        private ComputeShader computeShader;
        private RenderTexture stateA;
        private RenderTexture stateB;
        private RenderTexture staticTarget;
        private RenderTexture staticWakeSource;
        private RenderTexture rippleBoundary;
        private RenderTexture wakeA;
        private RenderTexture wakeB;
        private RenderTexture currentWake;
        private RenderTexture previousWake;
        private RenderTexture writeWake;
        private RenderTexture currentState;
        private RenderTexture previousState;
        private RenderTexture writeState;
        private ComputeBuffer rippleMetricBuffer;
        private float[] rippleMetricMinimumAlongCell = Array.Empty<float>();
        private float[] rippleMetricMinimumLateralCell = Array.Empty<float>();
        private float[] rippleChunkMaximumInverseLength = Array.Empty<float>();
        private float[] rippleChunkMinimumCellSize = Array.Empty<float>();
        private double[] chunkActiveUntil = Array.Empty<double>();
        private bool[] chunkActive = Array.Empty<bool>();
        private bool[] chunkHasStaticSource = Array.Empty<bool>();
        private double[] wakeChunkActiveUntil = Array.Empty<double>();
        private double[] staticWakeChunkReleaseDuration = Array.Empty<double>();
        private bool[] wakeChunkActive = Array.Empty<bool>();

        private int clearKernel = -1;
        private int injectRippleKernel = -1;
        private int injectWakeKernel = -1;
        private int bakeStaticPressureKernel = -1;
        private int finalizeStaticPressureKernel = -1;
        private int bakeStaticWakeSourceKernel = -1;
        private int bakeRippleBoundaryBaseKernel = -1;
        private int bakeRippleBoundaryObstacleKernel = -1;
        private int applyRippleBoundaryKernel = -1;
        private int simulateRippleKernel = -1;
        private int simulateWakeKernel = -1;
        private int fieldWidth;
        private int fieldHeight;
        private int chunkCount;
        private int resolutionPerChunk;
        private int wakeResolutionPerChunk;
        private int wakeFieldWidth;
        private int wakeFieldHeight;
        private int domainVersion = -1;
        private float fieldLength;
        private float validFieldLength;
        private int validFieldWidth;
        private int validWakeFieldWidth;
        private float averageSurfaceHalfWidth = 1f;
        private float simulationAccumulator;
        private float staticPressureProfileAccumulator;
        private float staticWakeVariationAccumulator;
        private float simulationInterpolation = 1f;
        private float wakeInterpolation = 1f;
        private double lastRuntimeTime;
        private double lastActivityTime;
        private bool supportWarningReported;
        private bool allocationWarningReported;
        private bool resourcesDirty = true;
        private bool staticPressureTargetDirty = true;
        private bool staticWakeSourceDirty = true;
        private bool rippleBoundaryDirty = true;
        private int validStaticSourceCount;
        private int validStaticWakeSourceCount;
        private int obstacleGeometryVersion;
        private int rippleCollisionSourceCount;
        private bool generatedGeometryRegistryDirty = true;
        private bool generatedGeometryRefreshInProgress;
        private int generatedGeometryRefreshIndex;
        private Bounds generatedGeometryRefreshBounds;
        private bool wasFrozen;
        private int impactsInjectedLastStep;
        private int currentRippleSubstepCount;
        private int maximumRecentRippleSubstepCount;
        private float activeRippleMinimumCellSize;
        private bool rippleSubstepLimitReached;
        private double rippleSubstepDiagnosticWindowStart;
        private int lastUpdateComputeDispatchCount;
        private int recentPeakComputeDispatchCount;
        private long lastUpdateThreadGroupCount;
        private long recentPeakThreadGroupCount;
        private long lastUpdateCellIterationCount;
        private long recentPeakCellIterationCount;
        private int lastUpdateRippleSimulationDispatchCount;
        private int lastUpdateWakeSimulationDispatchCount;
        private int lastUpdateImpactInjectionDispatchCount;
        private int lastUpdateWakeInjectionDispatchCount;
        private int lastUpdateStaticPressureBakeDispatchCount;
        private int lastUpdateStaticWakeBakeDispatchCount;
        private int lastUpdateRippleBoundaryBakeDispatchCount;
        private int lastUpdateClearDispatchCount;
        private int lastUpdateFieldRebuildCount;
        private int recentPeakFieldRebuildCount;
        private double performanceDiagnosticWindowStart;

        public bool IsSupported =>
            SystemInfo.supportsComputeShaders &&
            SystemInfo.SupportsRenderTextureFormat(
                RenderTextureFormat.ARGBHalf) &&
            SystemInfo.SupportsRenderTextureFormat(
                RenderTextureFormat.RGHalf);
        public bool IsAllocated => currentState != null;
        public bool IsSleeping =>
            !HasActiveChunks() &&
            continuousSources.Count == 0 &&
            pendingImpacts.Count == 0 &&
            activeImpactReservations.Count == 0;
        public int FieldWidth => fieldWidth;
        public int FieldHeight => fieldHeight;
        public int ChunkCount => chunkCount;
        public int ActiveChunkCount => CountActiveChunks();
        public int WakeFieldWidth => wakeFieldWidth;
        public int WakeFieldHeight => wakeFieldHeight;
        public RenderTexture CurrentWakeTexture => currentWake;
        public RenderTexture CurrentRippleTexture => currentState;
        public RenderTexture StaticWakeSourceTexture => staticWakeSource;
        // Stage 6 consumes the already accepted stationary Pressure target as
        // a read-only Pressure Support input. Foam never writes to or reinterprets
        // the Stage 5 field.
        public RenderTexture StaticPressureTexture => staticTarget;
        public Vector2Int WakeTextureDimensions => currentWake != null
            ? new Vector2Int(currentWake.width, currentWake.height)
            : Vector2Int.one;
        public Vector2Int RippleTextureDimensions => currentState != null
            ? new Vector2Int(currentState.width, currentState.height)
            : Vector2Int.one;
        public Vector2Int StaticWakeTextureDimensions => staticWakeSource != null
            ? new Vector2Int(staticWakeSource.width, staticWakeSource.height)
            : Vector2Int.one;
        public Vector2Int StaticPressureTextureDimensions => staticTarget != null
            ? new Vector2Int(staticTarget.width, staticTarget.height)
            : Vector2Int.one;
        public int ActiveWakeChunkCount => CountActiveWakeChunks();
        public int ContinuousSourceCount => continuousSources.Count;
        public int PendingImpactCount => pendingImpacts.Count;
        public int ActiveImpactReservationCount =>
            activeImpactReservations.Count;
        public float LongestImpactReservationRemainingSeconds =>
            ResolveLongestImpactReservationRemainingSeconds();
        public int ImpactsInjectedLastStep => impactsInjectedLastStep;
        public int CurrentRippleSubstepCount => currentRippleSubstepCount;
        public int MaximumRecentRippleSubstepCount =>
            maximumRecentRippleSubstepCount;
        public int RippleMetricRowCount =>
            rippleMetricBuffer != null ? fieldWidth : 0;
        public int RippleBoundaryWidth =>
            rippleBoundary != null ? rippleBoundary.width : 0;
        public int RippleBoundaryHeight =>
            rippleBoundary != null ? rippleBoundary.height : 0;
        public int RippleCollisionSourceCount =>
            rippleCollisionSourceCount;
        public float ActiveRippleMinimumCellSize =>
            activeRippleMinimumCellSize;
        public bool RippleSubstepLimitReached =>
            rippleSubstepLimitReached;
        public int RegisteredStationarySourceCount =>
            CountRegisteredStationarySources();
        public int ValidStaticPressureSourceCount => validStaticSourceCount;
        public int ValidStaticWakeSourceCount => validStaticWakeSourceCount;
        public int ObstacleGeometryVersion => obstacleGeometryVersion;
        public int LastUpdateComputeDispatchCount =>
            lastUpdateComputeDispatchCount;
        public int RecentPeakComputeDispatchCount =>
            recentPeakComputeDispatchCount;
        public long LastUpdateThreadGroupCount =>
            lastUpdateThreadGroupCount;
        public long RecentPeakThreadGroupCount =>
            recentPeakThreadGroupCount;
        public long LastUpdateCellIterationCount =>
            lastUpdateCellIterationCount;
        public long RecentPeakCellIterationCount =>
            recentPeakCellIterationCount;
        public int LastUpdateRippleSimulationDispatchCount =>
            lastUpdateRippleSimulationDispatchCount;
        public int LastUpdateWakeSimulationDispatchCount =>
            lastUpdateWakeSimulationDispatchCount;
        public int LastUpdateImpactInjectionDispatchCount =>
            lastUpdateImpactInjectionDispatchCount;
        public int LastUpdateWakeInjectionDispatchCount =>
            lastUpdateWakeInjectionDispatchCount;
        public int LastUpdateStaticPressureBakeDispatchCount =>
            lastUpdateStaticPressureBakeDispatchCount;
        public int LastUpdateStaticWakeBakeDispatchCount =>
            lastUpdateStaticWakeBakeDispatchCount;
        public int LastUpdateRippleBoundaryBakeDispatchCount =>
            lastUpdateRippleBoundaryBakeDispatchCount;
        public int LastUpdateClearDispatchCount =>
            lastUpdateClearDispatchCount;
        public int LastUpdateFieldRebuildCount =>
            lastUpdateFieldRebuildCount;
        public int RecentPeakFieldRebuildCount =>
            recentPeakFieldRebuildCount;
        public long RippleStateMemoryBytes =>
            IsAllocated ? (long)fieldWidth * fieldHeight * 8L * 2L : 0L;
        public long StaticPressureMemoryBytes =>
            IsAllocated ? (long)fieldWidth * fieldHeight * 8L : 0L;
        public long RippleBoundaryMemoryBytes =>
            rippleBoundary != null
                ? (long)fieldWidth * fieldHeight * 4L
                : 0L;
        public long WakeFieldMemoryBytes =>
            IsAllocated
                ? (long)wakeFieldWidth * wakeFieldHeight * 8L * 3L
                : 0L;
        public long RippleMetricMemoryBytes =>
            rippleMetricBuffer != null ? (long)fieldWidth * 32L : 0L;
        public float SimulationRate => ResolveSimulationRate();
        public float WakeSimulationRate => ResolveSimulationRate();
        public long EstimatedMemoryBytes =>
            RippleStateMemoryBytes +
            StaticPressureMemoryBytes +
            RippleBoundaryMemoryBytes +
            WakeFieldMemoryBytes +
            RippleMetricMemoryBytes;

        private struct StaticWakeLeeVariationState
        {
            public int SampleCount;
            public float[] CurrentDepthMultipliers;
            public float[] TransitionStartDepthMultipliers;
            public float[] TargetDepthMultipliers;
            public float[] CurrentLengthMultipliers;
            public float[] TransitionStartLengthMultipliers;
            public float[] TargetLengthMultipliers;
            public float[] CurrentTrailingEdgeOffsets;
            public float[] TransitionStartTrailingEdgeOffsets;
            public float[] TargetTrailingEdgeOffsets;
            public float[] RawScratch;
            public float[] SmoothedScratch;
            public float Transition;
            public float TransitionDuration;
            public float SelectedInterval;
            public uint EventIndex;
            public double NextEventTime;
            public bool ScheduleInitialized;
            public int ProfileFamily;
        }

        private struct StaticWakeReleaseVariationState
        {
            public float CurrentLateralOffset;
            public float TransitionStartLateralOffset;
            public float TargetLateralOffset;
            public float CurrentEnergyMultiplier;
            public float TransitionStartEnergyMultiplier;
            public float TargetEnergyMultiplier;
            public float CurrentWidthMultiplier;
            public float TransitionStartWidthMultiplier;
            public float TargetWidthMultiplier;
            public float CurrentDownstreamOffset;
            public float TransitionStartDownstreamOffset;
            public float TargetDownstreamOffset;
            public float Transition;
            public float TransitionDuration;
            public float SelectedInterval;
            public uint EventIndex;
            public double NextEventTime;
            public bool ScheduleInitialized;
        }

        private readonly struct StaticWakeBakeVariationParameters
        {
            public StaticWakeBakeVariationParameters(
                StaticWakeLeeVariationState lee,
                StaticWakeReleaseVariationState left,
                StaticWakeReleaseVariationState right)
            {
                Lee = lee;
                Left = left;
                Right = right;
            }

            public StaticWakeLeeVariationState Lee { get; }
            public StaticWakeReleaseVariationState Left { get; }
            public StaticWakeReleaseVariationState Right { get; }
        }

        private struct ContinuousSource
        {
            public Vector3 WorldPosition;
            public float StartDistance;
            public float EndDistance;
            public float StartAcrossNormalized;
            public float EndAcrossNormalized;
            public float AcrossHalfWidth;
            public float AlongHalfLength;
            public float Strength;
            public float GeometryContribution;
            public float NormalContribution;
            public float StaticTargetHeightMetres;
            public float StaticPressureAcrossHalfWidth;
            public float StaticPressureAlongHalfLength;
            public Vector2[] StaticPressureContour;
            public RiverDisturbancePressureBakeProfile StaticPressureProfile;
            public RiverDisturbancePressureBakeProfile StaticPressureBaseProfile;
            // Exact generated mesh retained for future editor-time solid data.
            // Runtime Foam consumes StaticContour; Static Pressure still has
            // its older independent scan and is explicitly marked for a future
            // shared-data refactor.
            public MeshFilter ObstacleExclusionMeshFilter;
            public float[] StaticPressureCurrentMultipliers;
            public float[] StaticPressureTransitionStartMultipliers;
            public float[] StaticPressureTargetMultipliers;
            public float[] StaticPressureRawScratch;
            public float[] StaticPressureSmoothedScratch;
            public float StaticPressureProfileTransition;
            public float StaticPressureProfileTransitionDuration;
            public float StaticPressureProfileChangeIntervalMin;
            public float StaticPressureProfileChangeIntervalMax;
            public uint StaticPressureProfileEventIndex;
            public double StaticPressureNextProfileEventTime;
            public bool StaticPressureProfileScheduleInitialized;
            public float StaticWakeAmplitude;
            public float StaticContactSharpness;
            public float StaticWakeReachMultiplier;
            public float StaticWakeSpreadMultiplier;
            public float StaticWakeVariation;
            public StaticWakeLeeVariationState StaticWakeLeeVariation;
            public StaticWakeReleaseVariationState
                StaticWakeLeftReleaseVariation;
            public StaticWakeReleaseVariationState
                StaticWakeRightReleaseVariation;
            public float StaticWakeVariationIntervalMin;
            public float StaticWakeVariationIntervalMax;
            public float StaticProfileVariation;
            public Vector2[] StaticContour;
            public bool RippleCollisionEnabled;
            public float RippleCollisionAcrossHalfWidth;
            public float RippleCollisionAlongHalfLength;
            public Vector2[] RippleCollisionContour;
            public float MovementSpeed;
            public float Phase;
            public EntityId OwnerId;
            public bool IsStatic;
            public bool StationaryObstruction;
            public double LastSeen;
        }

        private struct ImpactCommand
        {
            public float Distance;
            public float AcrossNormalized;
            public Vector2 WorldPositionXZ;
            public float Radius;
            public float SignedImpulse;
            public float InitialElevation;
            public float Shape;
            public float Sharpness;
            public float GeometryContribution;
            public float NormalContribution;
        }

        private struct ImpactReservation
        {
            public double EndTime;
            public float AgeSeconds;
            public float MinimumLifetime;
            public float MaximumLifetime;
            public float CurrentDistance;
            public float CurrentRadius;
            public float CurrentMagnitude;
            public float MinimumReservedDistance;
            public float MaximumReservedDistance;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RippleMetricRowData
        {
            // Must match RippleMetricRowData in CS_RiverDisturbance.compute.
            public Vector4 CentreAndTangent;
            public Vector4 SideAndWidths;
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            ActiveRuntimes.Clear();
            GeneratedSourceDiagnostics.Clear();
            sourcePhaseSequence = 1;
        }

        public static bool TryGetGeneratedSourceDiagnostics(
            IGeneratedGeometrySource source,
            out GeneratedRiverDisturbanceDiagnostics diagnostics)
        {
            diagnostics = default;
            if (source == null)
            {
                return false;
            }

            MeshFilter meshFilter = source.GeometryMeshFilter;
            return meshFilter != null &&
                   GeneratedSourceDiagnostics.TryGetValue(
                       meshFilter.GetEntityId(),
                       out diagnostics);
        }

#if UNITY_EDITOR
        public static bool TryGetGeneratedSourcePressureProfileDebugData(
            IGeneratedGeometrySource source,
            out GeneratedRiverPressureProfileDebugData debugData)
        {
            debugData = default;
            if (source == null)
            {
                return false;
            }

            MeshFilter meshFilter = source.GeometryMeshFilter;
            if (meshFilter == null)
            {
                return false;
            }

            EntityId sourceId = meshFilter.GetEntityId();
            if (!GeneratedSourceDiagnostics.TryGetValue(
                    sourceId,
                    out GeneratedRiverDisturbanceDiagnostics diagnostics))
            {
                return false;
            }

            for (int runtimeIndex = 0;
                 runtimeIndex < ActiveRuntimes.Count;
                 runtimeIndex++)
            {
                StylizedRiverDisturbanceRuntime runtime =
                    ActiveRuntimes[runtimeIndex];
                if (runtime == null ||
                    runtime.river == null ||
                    runtime.river != diagnostics.River ||
                    !runtime.continuousSources.TryGetValue(
                        sourceId,
                        out ContinuousSource continuousSource))
                {
                    continue;
                }

                RiverDisturbancePressureBakeProfile baseProfile =
                    continuousSource.StaticPressureBaseProfile;
                RiverDisturbancePressureBakeProfile currentProfile =
                    continuousSource.StaticPressureProfile;
                if (!baseProfile.IsValid ||
                    !currentProfile.IsValid ||
                    baseProfile.LateralSampleCount !=
                        currentProfile.LateralSampleCount)
                {
                    return false;
                }

                int sampleCount = baseProfile.LateralSampleCount;
                Vector2 appliedMultiplierBounds = sampleCount >= 64
                    ? new Vector2(0.86f, 1.10f)
                    : sampleCount >= 32
                        ? new Vector2(0.82f, 1.12f)
                        : new Vector2(
                            StaticPressureMinimumProfileMultiplier,
                            MaximumStaticPressureModulation);
                float targetHeight = diagnostics.EffectiveAmplitude;
                if (!baseProfile.HasGeometryBounds ||
                    !currentProfile.HasGeometryBounds)
                {
                    return false;
                }

                const float protectedDownstreamStartFraction = 0.50f;
                const float insideGateDownstreamTailPixels = 0.45f;
                float cellSizeX = runtime.fieldLength /
                    Mathf.Max(1, runtime.fieldWidth);
                float pressureInsideOverlapMetres = Mathf.Clamp(
                    Mathf.Max(0.08f, cellSizeX * 0.35f),
                    0.08f,
                    0.16f);
                float pressureInsideOverlapPixels =
                    pressureInsideOverlapMetres /
                    Mathf.Max(0.001f, cellSizeX);
                float crestInsetPixels = sampleCount >= 64
                    ? 1.50f
                    : sampleCount >= 32
                        ? 1.00f
                        : 0.75f;
                float minimumInsideOverlapPixels = sampleCount >= 64
                    ? 3.5f
                    : sampleCount >= 32
                        ? 2.5f
                        : 1.5f;
                float requestedInsideOverlapPixels = Mathf.Max(
                    minimumInsideOverlapPixels,
                    pressureInsideOverlapPixels);
                List<float> validRowThicknesses = new();

                float baseMinimum = float.PositiveInfinity;
                float baseMaximum = float.NegativeInfinity;
                float currentMinimum = float.PositiveInfinity;
                float currentMaximum = float.NegativeInfinity;
                float ceilingMinimum = float.PositiveInfinity;
                float ceilingMaximum = float.NegativeInfinity;
                float multiplierMinimum = float.PositiveInfinity;
                float multiplierMaximum = float.NegativeInfinity;
                float interiorBaseMinimum = float.PositiveInfinity;
                float interiorBaseMaximum = float.NegativeInfinity;
                float interiorCeilingMinimum = float.PositiveInfinity;
                float interiorCeilingMaximum = float.NegativeInfinity;
                float maximumAdjacentBaseHeightDifference = 0f;
                float maximumAdjacentCurrentHeightDifference = 0f;
                float maximumAdjacentBaseContactShift = 0f;
                float maximumAdjacentCurrentContactShift = 0f;
                float previousBaseHeight = 0f;
                float previousCurrentHeight = 0f;
                float previousBaseContact = 0f;
                float previousCurrentContact = 0f;
                bool hasPreviousValidRow = false;
                int validRowCount = 0;
                int supportLimitedBelowTargetRowCount = 0;
                int endpointTaperRowCount = 0;
                int targetHeightRowCount = 0;
                float rowThicknessMinimum = float.PositiveInfinity;
                float rowThicknessMaximum = float.NegativeInfinity;
                float maximumResolvedCrestDepthPercent = 0f;
                float maximumResolvedPressureEndDepthPercent = 0f;
                int geometryClampedRowCount = 0;
                int protectedDownstreamRegionViolationRowCount = 0;

                for (int row = 0; row < sampleCount; row++)
                {
                    Vector4 baseSample = baseProfile.Samples[row];
                    Vector4 currentSample = currentProfile.Samples[row];
                    if (baseSample.z <= 0.0001f ||
                        baseSample.w <= 0.0001f)
                    {
                        hasPreviousValidRow = false;
                        continue;
                    }

                    validRowCount++;
                    baseMinimum = Mathf.Min(baseMinimum, baseSample.z);
                    baseMaximum = Mathf.Max(baseMaximum, baseSample.z);
                    currentMinimum = Mathf.Min(
                        currentMinimum,
                        currentSample.z);
                    currentMaximum = Mathf.Max(
                        currentMaximum,
                        currentSample.z);
                    ceilingMinimum = Mathf.Min(
                        ceilingMinimum,
                        baseSample.w);
                    ceilingMaximum = Mathf.Max(
                        ceilingMaximum,
                        baseSample.w);

                    float row01 = sampleCount > 1
                        ? row / (float)(sampleCount - 1)
                        : 0.5f;
                    float lateral01 = Mathf.Abs(row01 * 2f - 1f);
                    float endpointTaper = 1f - Mathf.SmoothStep(
                        0f,
                        1f,
                        Mathf.InverseLerp(0.82f, 1f, lateral01));
                    if (endpointTaper < 0.999f)
                    {
                        endpointTaperRowCount++;
                    }

                    float untaperedBaseHeight = endpointTaper > 0.0001f
                        ? baseSample.z / endpointTaper
                        : 0f;
                    float untaperedCeilingHeight = endpointTaper > 0.0001f
                        ? baseSample.w / endpointTaper
                        : 0f;

                    if (lateral01 <= 0.82f)
                    {
                        interiorBaseMinimum = Mathf.Min(
                            interiorBaseMinimum,
                            baseSample.z);
                        interiorBaseMaximum = Mathf.Max(
                            interiorBaseMaximum,
                            baseSample.z);
                        interiorCeilingMinimum = Mathf.Min(
                            interiorCeilingMinimum,
                            baseSample.w);
                        interiorCeilingMaximum = Mathf.Max(
                            interiorCeilingMaximum,
                            baseSample.w);
                    }

                    if (endpointTaper > 0.0001f &&
                        untaperedCeilingHeight <
                            targetHeight - 0.0005f)
                    {
                        supportLimitedBelowTargetRowCount++;
                    }

                    if (endpointTaper > 0.0001f &&
                        untaperedBaseHeight >= targetHeight - 0.0005f)
                    {
                        targetHeightRowCount++;
                    }

                    float multiplier = 1f;
                    if (continuousSource.
                            StaticPressureCurrentMultipliers != null &&
                        continuousSource.
                            StaticPressureCurrentMultipliers.Length ==
                            sampleCount)
                    {
                        multiplier = continuousSource.
                            StaticPressureCurrentMultipliers[row];
                    }
                    else if (baseSample.z > 0.0001f)
                    {
                        multiplier = currentSample.z / baseSample.z;
                    }

                    multiplierMinimum = Mathf.Min(
                        multiplierMinimum,
                        multiplier);
                    multiplierMaximum = Mathf.Max(
                        multiplierMaximum,
                        multiplier);

                    float baseContact =
                        baseSample.x + baseSample.y * baseSample.z;
                    float currentContact =
                        currentSample.x +
                        currentSample.y * currentSample.z;

                    float downstreamBoundary =
                        baseProfile.DownstreamBoundaries[row];
                    float rowThickness =
                        downstreamBoundary - baseSample.x;
                    if (rowThickness > 0.005f)
                    {
                        validRowThicknesses.Add(rowThickness);
                        rowThicknessMinimum = Mathf.Min(
                            rowThicknessMinimum,
                            rowThickness);
                        rowThicknessMaximum = Mathf.Max(
                            rowThicknessMaximum,
                            rowThickness);

                        float protectedDownstreamStart = Mathf.Lerp(
                            baseSample.x,
                            downstreamBoundary,
                            protectedDownstreamStartFraction);
                        float requestedCrest = baseSample.x +
                            Mathf.Max(
                                0f,
                                currentSample.y * currentSample.z) +
                            crestInsetPixels * cellSizeX;
                        float resolvedCrest = Mathf.Min(
                            requestedCrest,
                            protectedDownstreamStart);
                        float requestedPressureEnd = resolvedCrest +
                            (requestedInsideOverlapPixels +
                             insideGateDownstreamTailPixels) * cellSizeX;
                        float resolvedPressureEnd = Mathf.Min(
                            requestedPressureEnd,
                            protectedDownstreamStart);

                        if (requestedCrest >
                                protectedDownstreamStart + 0.0001f ||
                            requestedPressureEnd >
                                protectedDownstreamStart + 0.0001f)
                        {
                            geometryClampedRowCount++;
                        }

                        if (resolvedPressureEnd >
                            protectedDownstreamStart + 0.0001f)
                        {
                            protectedDownstreamRegionViolationRowCount++;
                        }

                        maximumResolvedCrestDepthPercent = Mathf.Max(
                            maximumResolvedCrestDepthPercent,
                            Mathf.Clamp01(
                                (resolvedCrest - baseSample.x) /
                                rowThickness) * 100f);
                        maximumResolvedPressureEndDepthPercent = Mathf.Max(
                            maximumResolvedPressureEndDepthPercent,
                            Mathf.Clamp01(
                                (resolvedPressureEnd - baseSample.x) /
                                rowThickness) * 100f);
                    }

                    if (hasPreviousValidRow)
                    {
                        maximumAdjacentBaseHeightDifference = Mathf.Max(
                            maximumAdjacentBaseHeightDifference,
                            Mathf.Abs(baseSample.z - previousBaseHeight));
                        maximumAdjacentCurrentHeightDifference = Mathf.Max(
                            maximumAdjacentCurrentHeightDifference,
                            Mathf.Abs(
                                currentSample.z -
                                previousCurrentHeight));
                        maximumAdjacentBaseContactShift = Mathf.Max(
                            maximumAdjacentBaseContactShift,
                            Mathf.Abs(baseContact - previousBaseContact));
                        maximumAdjacentCurrentContactShift = Mathf.Max(
                            maximumAdjacentCurrentContactShift,
                            Mathf.Abs(
                                currentContact -
                                previousCurrentContact));
                    }

                    previousBaseHeight = baseSample.z;
                    previousCurrentHeight = currentSample.z;
                    previousBaseContact = baseContact;
                    previousCurrentContact = currentContact;
                    hasPreviousValidRow = true;
                }

                if (validRowCount == 0)
                {
                    return false;
                }

                if (float.IsInfinity(interiorBaseMinimum))
                {
                    interiorBaseMinimum = baseMinimum;
                    interiorBaseMaximum = baseMaximum;
                }

                if (float.IsInfinity(interiorCeilingMinimum))
                {
                    interiorCeilingMinimum = ceilingMinimum;
                    interiorCeilingMaximum = ceilingMaximum;
                }

                int lateralFieldResolution = runtime.river.Quality switch
                {
                    StylizedRiverQuality.Low => 32,
                    StylizedRiverQuality.Medium => 48,
                    StylizedRiverQuality.High => 64,
                    _ => 48
                };
                float requestedProfileWidthPixels =
                    Mathf.Max(
                        0.10f,
                        continuousSource.
                            StaticPressureAcrossHalfWidth * 2f) /
                    Mathf.Max(0.10f, diagnostics.LocalRiverWidth) *
                    lateralFieldResolution;

                if (validRowThicknesses.Count == 0)
                {
                    return false;
                }

                validRowThicknesses.Sort();
                int middleIndex = validRowThicknesses.Count / 2;
                float medianRowThickness =
                    validRowThicknesses.Count % 2 == 0
                        ? (validRowThicknesses[middleIndex - 1] +
                           validRowThicknesses[middleIndex]) * 0.5f
                        : validRowThicknesses[middleIndex];

                debugData =
                    new GeneratedRiverPressureProfileDebugData(
                        runtime.river,
                        continuousSource.WorldPosition,
                        continuousSource.
                            StaticPressureAcrossHalfWidth,
                        requestedProfileWidthPixels,
                        sampleCount,
                        RiverDisturbanceFootprintResolver.
                            PressureSupportHeightSlices,
                        diagnostics.SupportInspectionHeight,
                        targetHeight,
                        MaximumStaticPressureModulation,
                        validRowCount,
                        supportLimitedBelowTargetRowCount,
                        endpointTaperRowCount,
                        targetHeightRowCount,
                        new Vector2(baseMinimum, baseMaximum),
                        new Vector2(currentMinimum, currentMaximum),
                        new Vector2(ceilingMinimum, ceilingMaximum),
                        new Vector2(
                            multiplierMinimum,
                            multiplierMaximum),
                        new Vector2(
                            interiorBaseMinimum,
                            interiorBaseMaximum),
                        new Vector2(
                            interiorCeilingMinimum,
                            interiorCeilingMaximum),
                        maximumAdjacentBaseHeightDifference,
                        maximumAdjacentCurrentHeightDifference,
                        maximumAdjacentBaseContactShift,
                        maximumAdjacentCurrentContactShift,
                        new Vector2(
                            rowThicknessMinimum,
                            rowThicknessMaximum),
                        medianRowThickness,
                        maximumResolvedCrestDepthPercent,
                        maximumResolvedPressureEndDepthPercent,
                        geometryClampedRowCount,
                        protectedDownstreamRegionViolationRowCount,
                        protectedDownstreamStartFraction * 100f,
                        appliedMultiplierBounds,
                        baseProfile.Samples,
                        currentProfile.Samples,
                        baseProfile.DownstreamBoundaries);
                return debugData.IsValid;
            }

            return false;
        }

#endif

        private void OnEnable()
        {
            river = GetComponent<StylizedRiver>();
            surfaceRenderer = river != null ? river.SurfaceRenderer : null;
            propertyBlock ??= new MaterialPropertyBlock();

            if (!ActiveRuntimes.Contains(this))
            {
                ActiveRuntimes.Add(this);
            }

            if (river != null)
            {
                river.DomainChanged += HandleDomainChanged;
            }

            GeneratedGeometryRegistry.SourceAdded +=
                HandleGeneratedGeometrySourceAdded;
            GeneratedGeometryRegistry.SourceRemoved +=
                HandleGeneratedGeometrySourceRemoved;
            GeneratedGeometryRegistry.SourceChanged +=
                HandleGeneratedGeometrySourceChanged;

            lastRuntimeTime = Time.realtimeSinceStartupAsDouble;
            resourcesDirty = true;
            generatedGeometryRegistryDirty = true;
            BindDisabled();
        }

        private void OnDisable()
        {
            ActiveRuntimes.Remove(this);

            if (river != null)
            {
                river.DomainChanged -= HandleDomainChanged;
            }

            GeneratedGeometryRegistry.SourceAdded -=
                HandleGeneratedGeometrySourceAdded;
            GeneratedGeometryRegistry.SourceRemoved -=
                HandleGeneratedGeometrySourceRemoved;
            GeneratedGeometryRegistry.SourceChanged -=
                HandleGeneratedGeometrySourceChanged;

            RemoveOwnedGeneratedDiagnostics();
            BindDisabled();
            ReleaseResources();
            continuousSources.Clear();
            continuousSourceIdsByOwner.Clear();
            obstacleGeometryVersion = 0;
            ownershipConflictWarningOwnerIds.Clear();
            automaticGeneratedSourceIds.Clear();
            refreshedAutomaticGeneratedSourceIds.Clear();
            generatedGeometryScratch.Clear();
            staticPressureProfileSourceIds.Clear();
            generatedGeometryRefreshInProgress = false;
            generatedGeometryRefreshIndex = 0;
            pendingImpacts.Clear();
            activeImpactReservations.Clear();
        }

        private void OnDestroy()
        {
            ReleaseResources();
        }

        private void LateUpdate()
        {
            BeginPerformanceDiagnosticsUpdate();

            if (river == null)
            {
                river = GetComponent<StylizedRiver>();
            }

            if (river == null ||
                !river.isActiveAndEnabled ||
                !river.RuntimeDisturbancesEnabled)
            {
                BindDisabled();
                ReleaseResources();
                return;
            }

            surfaceRenderer = river.SurfaceRenderer;

            if (!Application.isPlaying)
            {
                BindDisabled();
                return;
            }

            if (!IsSupported)
            {
                if (!supportWarningReported)
                {
                    Debug.LogWarning(
                        $"StylizedRiver disturbance field on '{name}' is disabled because compute shaders or required half-float random-write textures are unavailable.",
                        this);
                    supportWarningReported = true;
                }

                BindDisabled();
                return;
            }

            supportWarningReported = false;

            if (river.LiquidFactor <= 0.0001f)
            {
                if (!wasFrozen)
                {
                    ClearField();
                }

                // Impact requests can arrive after the freeze-transition clear
                // but before this runtime updates again. Discard them every
                // fully frozen frame so no event can survive and replay after
                // thawing.
                pendingImpacts.Clear();
                activeImpactReservations.Clear();
                impactsInjectedLastStep = 0;
                currentRippleSubstepCount = 0;
                maximumRecentRippleSubstepCount = 0;
                activeRippleMinimumCellSize = 0f;
                rippleSubstepLimitReached = false;
                rippleSubstepDiagnosticWindowStart = 0.0;
                wasFrozen = true;
                BindDisabled();
                return;
            }

            wasFrozen = false;

            if (generatedGeometryRegistryDirty ||
                generatedGeometryRefreshInProgress)
            {
                RefreshGeneratedGeometrySources();
            }

            double now = Time.realtimeSinceStartupAsDouble;
            float deltaTime = Mathf.Clamp(
                (float)(now - lastRuntimeTime),
                0f,
                0.1f);
            lastRuntimeTime = now;

            CleanupStaleSources(now);
            UpdateStaticPressureProfiles(deltaTime, now);
            UpdateStaticWakeVariations(deltaTime, now);

            bool requiresField =
                pendingImpacts.Count > 0 ||
                activeImpactReservations.Count > 0 ||
                continuousSources.Count > 0 ||
                HasActiveChunks();

            if (!requiresField)
            {
                if (currentState != null &&
                    now - lastActivityTime > 10.0)
                {
                    ReleaseResources();
                }

                BindDisabled();
                return;
            }

            if (!EnsureResources())
            {
                BindDisabled();
                return;
            }

            SetValidDomainComputeParameters();
            float interval = 1f / Mathf.Max(1f, ResolveSimulationRate());
            simulationAccumulator = Mathf.Min(
                simulationAccumulator + deltaTime,
                interval * 2.5f);

            int stepCount = 0;
            while (simulationAccumulator >= interval && stepCount < 2)
            {
                SimulateStep(interval, now);
                simulationAccumulator -= interval;
                stepCount++;
            }

            if (simulationAccumulator >= interval)
            {
                simulationAccumulator = 0f;
            }

            simulationInterpolation = Mathf.Clamp01(
                simulationAccumulator / interval);
            wakeInterpolation = simulationInterpolation;

            BindField();
        }

        public void NotifyRiverChanged()
        {
            resourcesDirty = true;
            staticPressureTargetDirty = true;
            staticWakeSourceDirty = true;
            rippleBoundaryDirty = true;
            generatedGeometryRegistryDirty = true;
        }

        public void ClearField()
        {
            if (computeShader != null)
            {
                DispatchClear(stateA, fieldWidth, fieldHeight, 0, fieldWidth);
                DispatchClear(stateB, fieldWidth, fieldHeight, 0, fieldWidth);
                DispatchClear(wakeA, wakeFieldWidth, wakeFieldHeight, 0, wakeFieldWidth);
                DispatchClear(wakeB, wakeFieldWidth, wakeFieldHeight, 0, wakeFieldWidth);
            }

            Array.Clear(chunkActive, 0, chunkActive.Length);
            Array.Clear(chunkActiveUntil, 0, chunkActiveUntil.Length);
            Array.Clear(wakeChunkActive, 0, wakeChunkActive.Length);
            Array.Clear(wakeChunkActiveUntil, 0, wakeChunkActiveUntil.Length);
            Array.Clear(chunkHasStaticSource, 0, chunkHasStaticSource.Length);
            Array.Clear(
                staticWakeChunkReleaseDuration,
                0,
                staticWakeChunkReleaseDuration.Length);
            staticWakeSourceDirty = true;
            pendingImpacts.Clear();
            activeImpactReservations.Clear();
            impactsInjectedLastStep = 0;
            currentRippleSubstepCount = 0;
            maximumRecentRippleSubstepCount = 0;
            activeRippleMinimumCellSize = 0f;
            rippleSubstepLimitReached = false;
            rippleSubstepDiagnosticWindowStart = 0.0;
            simulationAccumulator = 0f;
            staticWakeVariationAccumulator = 0f;
            simulationInterpolation = 1f;
            wakeInterpolation = 1f;
        }

        public bool EmitImpact(
            Vector3 worldPosition,
            float radius,
            float strength,
            float geometryContribution = 1f,
            float normalContribution = 1f)
        {
            return EmitImpact(
                worldPosition,
                ImpactRippleEventSettings.CreateLegacy(
                    radius,
                    strength,
                    geometryContribution,
                    normalContribution));
        }

        public bool EmitImpact(
            Vector3 worldPosition,
            ImpactRippleEventSettings eventSettings)
        {
            if (river == null ||
                !river.isActiveAndEnabled ||
                !river.RuntimeDisturbancesEnabled ||
                river.LiquidFactor <= 0.0001f ||
                !river.TryProjectWorldPoint(
                    worldPosition,
                    out StylizedRiverProjection projection) ||
                !projection.IsInside)
            {
                return false;
            }

            ImpactRippleEventSettings sanitized =
                eventSettings.Sanitized();
            StylizedRiverSplineSample sample =
                river.SampleAtLocalDistance(projection.LocalDistance);
            float surfaceHalfWidth = Mathf.Max(
                0.05f,
                sample.GetSurfaceHalfWidth(projection.AcrossMetres));
            Vector3 projectedSurfacePosition =
                projection.SurfacePoint +
                projection.Side * projection.AcrossMetres;

            pendingImpacts.Add(
                new ImpactCommand
                {
                    Distance = projection.GlobalDistance,
                    AcrossNormalized = Mathf.Clamp(
                        projection.AcrossMetres / surfaceHalfWidth,
                        -1f,
                        1f),
                    WorldPositionXZ = new Vector2(
                        projectedSurfacePosition.x,
                        projectedSurfacePosition.z),
                    Radius = sanitized.Radius,
                    SignedImpulse = sanitized.SignedImpulse,
                    InitialElevation = sanitized.InitialElevation,
                    Shape = sanitized.Shape,
                    Sharpness = sanitized.Sharpness,
                    GeometryContribution =
                        sanitized.GeometryContribution,
                    NormalContribution =
                        sanitized.NormalContribution
                });

            lastActivityTime = Time.realtimeSinceStartupAsDouble;
            return true;
        }

        public bool RegisterStaticSource(
            EntityId sourceId,
            EntityId ownerId,
            Vector3 worldPosition,
            float acrossHalfWidth,
            float alongHalfLength,
            float strength,
            float geometryContribution,
            float normalContribution,
            float targetHeightFraction = -1f,
            float staticWakeAmplitude = -1f,
            float responseStiffness = 1f,
            float wakeReachMultiplier = 1f,
            float unsteadiness = 1f,
            IReadOnlyList<Vector2> contour = null,
            float explicitTargetHeightMetres = -1f,
            float pressureAcrossHalfWidth = -1f,
            float pressureAlongHalfLength = -1f,
            IReadOnlyList<Vector2> pressureContour = null,
            RiverDisturbancePressureBakeProfile pressureProfile = default,
            MeshFilter obstacleExclusionMeshFilter = null,
            bool deferStaticTargetRebuild = false,
            float wakeSpreadMultiplier = 1f,
            float profileChangeIntervalMin =
                StylizedRiver.DefaultStaticPressureProfileChangeIntervalMin,
            float profileChangeIntervalMax =
                StylizedRiver.DefaultStaticPressureProfileChangeIntervalMax,
            float wakeVariation = 0.35f,
            float wakeVariationIntervalMin =
                StylizedRiver.DefaultStaticWakeVariationIntervalMin,
            float wakeVariationIntervalMax =
                StylizedRiver.DefaultStaticWakeVariationIntervalMax,
            bool rippleCollisionEnabled = true,
            float rippleCollisionAcrossHalfWidth = -1f,
            float rippleCollisionAlongHalfLength = -1f,
            IReadOnlyList<Vector2> rippleCollisionContour = null)
        {
            if (river == null ||
                !river.isActiveAndEnabled ||
                !river.RuntimeDisturbancesEnabled ||
                !river.TryProjectWorldPoint(
                    worldPosition,
                    out StylizedRiverProjection projection) ||
                !projection.IsInside)
            {
                RemoveContinuousSource(sourceId);
                return false;
            }

            if (!TryClaimContinuousSourceOwner(
                    ownerId,
                    sourceId,
                    true))
            {
                RemoveContinuousSource(sourceId);
                return false;
            }

            StylizedRiverSplineSample sample =
                river.SampleAtLocalDistance(projection.LocalDistance);
            float surfaceHalfWidth = Mathf.Max(
                0.05f,
                sample.GetSurfaceHalfWidth(projection.AcrossMetres));
            float phase = ResolveSourcePhase(sourceId);
            float resolvedHeightMetres = explicitTargetHeightMetres >= 0f
                ? Mathf.Clamp(
                    explicitTargetHeightMetres,
                    0f,
                    MaximumStaticPressureHeightMetres)
                : targetHeightFraction >= 0f
                    ? river.ResolvedImpactRippleMaximumHeight *
                      Mathf.Clamp01(targetHeightFraction)
                    : Mathf.Clamp(
                        Mathf.Max(0f, strength) *
                        Mathf.Clamp01(geometryContribution) *
                        0.040f,
                        0f,
                        MaximumStaticPressureHeightMetres);
            float resolvedWakeAmplitude = staticWakeAmplitude >= 0f
                ? Mathf.Max(0f, staticWakeAmplitude)
                : Mathf.Max(0f, strength) *
                  Mathf.Clamp01(normalContribution) *
                  0.22f;
            RiverDisturbancePressureBakeProfile basePressureProfile =
                ClonePressureProfile(pressureProfile);
            RiverDisturbancePressureBakeProfile animatedPressureProfile =
                ClonePressureProfile(pressureProfile);
            float[] currentProfileMultipliers =
                CreateUnitPressureProfileMultipliers(basePressureProfile);
            float[] transitionStartMultipliers =
                CreateUnitPressureProfileMultipliers(basePressureProfile);
            float[] targetProfileMultipliers =
                CreateUnitPressureProfileMultipliers(basePressureProfile);
            float[] rawProfileScratch =
                CreatePressureProfileScratch(basePressureProfile);
            float[] smoothedProfileScratch =
                CreatePressureProfileScratch(basePressureProfile);
            int wakeVariationSampleCount =
                ResolveStaticWakeVariationLateralSampleCount(
                    acrossHalfWidth,
                    surfaceHalfWidth * 2f);
            StaticWakeLeeVariationState wakeLeeVariation =
                CreateStaticWakeLeeVariationState(
                    wakeVariationSampleCount);
            StaticWakeReleaseVariationState leftWakeVariation =
                CreateStaticWakeReleaseVariationState();
            StaticWakeReleaseVariationState rightWakeVariation =
                CreateStaticWakeReleaseVariationState();

            continuousSources[sourceId] =
                new ContinuousSource
                {
                    WorldPosition = worldPosition,
                    StartDistance = projection.GlobalDistance,
                    EndDistance = projection.GlobalDistance,
                    StartAcrossNormalized = Mathf.Clamp(
                        projection.AcrossMetres / surfaceHalfWidth,
                        -1f,
                        1f),
                    EndAcrossNormalized = Mathf.Clamp(
                        projection.AcrossMetres / surfaceHalfWidth,
                        -1f,
                        1f),
                    AcrossHalfWidth = Mathf.Max(
                        0.05f,
                        acrossHalfWidth),
                    AlongHalfLength = Mathf.Max(
                        0.05f,
                        alongHalfLength),
                    Strength = Mathf.Max(0f, strength),
                    GeometryContribution = Mathf.Clamp01(
                        geometryContribution),
                    NormalContribution = Mathf.Clamp01(
                        normalContribution),
                    StaticTargetHeightMetres = resolvedHeightMetres,
                    StaticPressureAcrossHalfWidth = pressureAcrossHalfWidth > 0f
                        ? Mathf.Max(0.05f, pressureAcrossHalfWidth)
                        : Mathf.Max(0.05f, acrossHalfWidth),
                    StaticPressureAlongHalfLength = pressureAlongHalfLength > 0f
                        ? Mathf.Max(0.05f, pressureAlongHalfLength)
                        : Mathf.Max(0.05f, alongHalfLength),
                    StaticPressureContour = CopyStaticContour(
                        pressureContour ?? contour),
                    StaticPressureProfile = animatedPressureProfile,
                    StaticPressureBaseProfile = basePressureProfile,
                    ObstacleExclusionMeshFilter =
                        obstacleExclusionMeshFilter,
                    StaticPressureCurrentMultipliers =
                        currentProfileMultipliers,
                    StaticPressureTransitionStartMultipliers =
                        transitionStartMultipliers,
                    StaticPressureTargetMultipliers =
                        targetProfileMultipliers,
                    StaticPressureRawScratch = rawProfileScratch,
                    StaticPressureSmoothedScratch =
                        smoothedProfileScratch,
                    StaticPressureProfileTransition = 1f,
                    StaticPressureProfileTransitionDuration = 0f,
                    StaticPressureProfileChangeIntervalMin = Mathf.Clamp(
                        Mathf.Min(
                            profileChangeIntervalMin,
                            profileChangeIntervalMax),
                        StylizedRiver.MinimumStaticPressureProfileChangeInterval,
                        StylizedRiver.MaximumStaticPressureProfileChangeInterval),
                    StaticPressureProfileChangeIntervalMax = Mathf.Clamp(
                        Mathf.Max(
                            profileChangeIntervalMin,
                            profileChangeIntervalMax),
                        StylizedRiver.MinimumStaticPressureProfileChangeInterval,
                        StylizedRiver.MaximumStaticPressureProfileChangeInterval),
                    StaticPressureProfileEventIndex = 0u,
                    StaticPressureNextProfileEventTime = 0.0,
                    StaticPressureProfileScheduleInitialized = false,
                    StaticWakeAmplitude = resolvedWakeAmplitude,
                    StaticContactSharpness = Mathf.Clamp(
                        responseStiffness,
                        0.5f,
                        4f),
                    StaticWakeReachMultiplier = Mathf.Clamp(
                        wakeReachMultiplier,
                        0.25f,
                        3f),
                    StaticWakeSpreadMultiplier = Mathf.Clamp(
                        wakeSpreadMultiplier,
                        0.5f,
                        2f),
                    StaticWakeVariation = Mathf.Clamp01(wakeVariation),
                    StaticWakeLeeVariation = wakeLeeVariation,
                    StaticWakeLeftReleaseVariation = leftWakeVariation,
                    StaticWakeRightReleaseVariation = rightWakeVariation,
                    StaticWakeVariationIntervalMin = Mathf.Clamp(
                        Mathf.Min(
                            wakeVariationIntervalMin,
                            wakeVariationIntervalMax),
                        StylizedRiver.MinimumStaticWakeVariationInterval,
                        StylizedRiver.MaximumStaticWakeVariationInterval),
                    StaticWakeVariationIntervalMax = Mathf.Clamp(
                        Mathf.Max(
                            wakeVariationIntervalMin,
                            wakeVariationIntervalMax),
                        StylizedRiver.MinimumStaticWakeVariationInterval,
                        StylizedRiver.MaximumStaticWakeVariationInterval),
                    StaticProfileVariation = Mathf.Clamp(
                        unsteadiness,
                        0f,
                        2f),
                    StaticContour = CopyStaticContour(contour),
                    RippleCollisionEnabled = rippleCollisionEnabled,
                    RippleCollisionAcrossHalfWidth =
                        rippleCollisionAcrossHalfWidth > 0f
                            ? Mathf.Max(
                                0.05f,
                                rippleCollisionAcrossHalfWidth)
                            : Mathf.Max(0.05f, acrossHalfWidth),
                    RippleCollisionAlongHalfLength =
                        rippleCollisionAlongHalfLength > 0f
                            ? Mathf.Max(
                                0.05f,
                                rippleCollisionAlongHalfLength)
                            : Mathf.Max(0.05f, alongHalfLength),
                    RippleCollisionContour = CopyStaticContour(
                        rippleCollisionContour ?? contour),
                    MovementSpeed = 0f,
                    Phase = phase,
                    OwnerId = ownerId,
                    IsStatic = true,
                    StationaryObstruction = true,
                    LastSeen = double.PositiveInfinity
                };

            if (!deferStaticTargetRebuild)
            {
                staticPressureTargetDirty = true;
                staticWakeSourceDirty = true;
                rippleBoundaryDirty = true;
            }

            lastActivityTime = Time.realtimeSinceStartupAsDouble;
            return true;
        }

        public bool UpdateContinuousSource(
            EntityId sourceId,
            EntityId ownerId,
            Vector3 previousWorldPosition,
            Vector3 currentWorldPosition,
            float sampleDeltaTime,
            float acrossHalfWidth,
            float alongHalfLength,
            float strength,
            float geometryContribution,
            float normalContribution,
            bool stationaryObstruction)
        {
            if (river == null ||
                !river.isActiveAndEnabled ||
                !river.RuntimeDisturbancesEnabled ||
                !river.TryProjectWorldPoint(
                    currentWorldPosition,
                    out StylizedRiverProjection currentProjection) ||
                !currentProjection.IsInside)
            {
                RemoveContinuousSource(sourceId);
                return false;
            }

            if (!TryClaimContinuousSourceOwner(
                    ownerId,
                    sourceId,
                    false))
            {
                RemoveContinuousSource(sourceId);
                return false;
            }

            bool previousValid =
                river.TryProjectWorldPoint(
                    previousWorldPosition,
                    out StylizedRiverProjection previousProjection) &&
                previousProjection.IsInside;

            if (!previousValid)
            {
                previousProjection = currentProjection;
            }

            StylizedRiverSplineSample currentSample =
                river.SampleAtLocalDistance(
                    currentProjection.LocalDistance);
            StylizedRiverSplineSample previousSample =
                river.SampleAtLocalDistance(
                    previousProjection.LocalDistance);

            float currentSurfaceHalf = Mathf.Max(
                0.05f,
                currentSample.GetSurfaceHalfWidth(
                    currentProjection.AcrossMetres));
            float previousSurfaceHalf = Mathf.Max(
                0.05f,
                previousSample.GetSurfaceHalfWidth(
                    previousProjection.AcrossMetres));

            float riverSpaceTravel = new Vector2(
                currentProjection.GlobalDistance -
                previousProjection.GlobalDistance,
                currentProjection.AcrossMetres -
                previousProjection.AcrossMetres).magnitude;

            if (continuousSources.TryGetValue(
                    sourceId,
                    out ContinuousSource previousSource) &&
                previousSource.IsStatic)
            {
                staticPressureTargetDirty = true;
                staticWakeSourceDirty = true;
            }

            continuousSources[sourceId] =
                new ContinuousSource
                {
                    WorldPosition = currentWorldPosition,
                    StartDistance = previousProjection.GlobalDistance,
                    EndDistance = currentProjection.GlobalDistance,
                    StartAcrossNormalized = Mathf.Clamp(
                        previousProjection.AcrossMetres /
                        previousSurfaceHalf,
                        -1f,
                        1f),
                    EndAcrossNormalized = Mathf.Clamp(
                        currentProjection.AcrossMetres /
                        currentSurfaceHalf,
                        -1f,
                        1f),
                    AcrossHalfWidth = Mathf.Max(
                        0.05f,
                        acrossHalfWidth),
                    AlongHalfLength = Mathf.Max(
                        0.05f,
                        alongHalfLength),
                    Strength = Mathf.Max(0f, strength),
                    GeometryContribution = Mathf.Clamp01(
                        geometryContribution),
                    NormalContribution = Mathf.Clamp01(
                        normalContribution),
                    StaticTargetHeightMetres = 0f,
                    StaticWakeAmplitude = 0f,
                    StaticContactSharpness = 1f,
                    StaticWakeReachMultiplier = 1f,
                    StaticWakeSpreadMultiplier = 1f,
                    StaticProfileVariation = 1f,
                    StaticContour = Array.Empty<Vector2>(),
                    RippleCollisionEnabled = false,
                    RippleCollisionAcrossHalfWidth = 0f,
                    RippleCollisionAlongHalfLength = 0f,
                    RippleCollisionContour = Array.Empty<Vector2>(),
                    MovementSpeed =
                        riverSpaceTravel /
                        Mathf.Max(0.001f, sampleDeltaTime),
                    Phase = ResolveSourcePhase(sourceId),
                    OwnerId = ownerId,
                    IsStatic = false,
                    StationaryObstruction = stationaryObstruction,
                    LastSeen = Time.realtimeSinceStartupAsDouble
                };

            lastActivityTime = Time.realtimeSinceStartupAsDouble;
            return true;
        }

        public bool ContainsContinuousSource(EntityId sourceId)
        {
            return continuousSources.ContainsKey(sourceId);
        }

        /// <summary>
        /// Copies the exact generated meshes currently registered as static
        /// river obstructions. Retained for future editor-time exact interval
        /// baking; runtime Foam now consumes waterline contour footprints.
        /// </summary>
        public void CopyObstacleExclusionMeshFiltersTo(
            List<MeshFilter> output)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            output.Clear();
            if (river == null || !river.Domain.IsValid)
            {
                return;
            }

            foreach (KeyValuePair<EntityId, ContinuousSource> pair in
                     continuousSources)
            {
                ContinuousSource source = pair.Value;
                MeshFilter meshFilter = source.ObstacleExclusionMeshFilter;
                if (!source.IsStatic ||
                    meshFilter == null ||
                    meshFilter.sharedMesh == null ||
                    !meshFilter.gameObject.activeInHierarchy ||
                    output.Contains(meshFilter))
                {
                    continue;
                }

                output.Add(meshFilter);
            }

            output.Sort((left, right) =>
                left.GetEntityId().CompareTo(right.GetEntityId()));
        }

        /// <summary>
        /// Copies mesh-derived waterline contours for the Stage 6 Foam
        /// Obstacle Footprint. Runtime Foam uses this surface silhouette
        /// instead of rescanning full triangle meshes on Play startup.
        /// </summary>
        public void CopyObstacleExclusionFootprintsTo(
            List<RiverObstacleExclusionFootprint> output)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            output.Clear();
            if (river == null || !river.Domain.IsValid)
            {
                return;
            }

            foreach (KeyValuePair<EntityId, ContinuousSource> pair in
                     continuousSources)
            {
                ContinuousSource source = pair.Value;
                MeshFilter meshFilter = source.ObstacleExclusionMeshFilter;
                if (!source.IsStatic ||
                    source.StaticContour == null ||
                    source.StaticContour.Length < 3 ||
                    meshFilter == null ||
                    meshFilter.sharedMesh == null ||
                    !meshFilter.gameObject.activeInHierarchy ||
                    !river.TryProjectWorldPoint(
                        source.WorldPosition,
                        out StylizedRiverProjection projection) ||
                    !projection.IsInside)
                {
                    continue;
                }

                StylizedRiverSplineSample sample =
                    river.SampleAtLocalDistance(projection.LocalDistance);
                float surfaceHalfWidth = Mathf.Max(
                    0.05f,
                    sample.GetSurfaceHalfWidth(projection.AcrossMetres));
                output.Add(
                    new RiverObstacleExclusionFootprint(
                        pair.Key,
                        projection.GlobalDistance,
                        projection.AcrossMetres,
                        surfaceHalfWidth,
                        source.AlongHalfLength,
                        source.AcrossHalfWidth,
                        CopyStaticContour(source.StaticContour)));
            }

            output.Sort((left, right) =>
                left.SourceId.CompareTo(right.SourceId));
        }

        public void RemoveContinuousSource(EntityId sourceId)
        {
            if (!continuousSources.TryGetValue(
                    sourceId,
                    out ContinuousSource source))
            {
                return;
            }

            if (source.IsStatic)
            {
                staticPressureTargetDirty = true;
                staticWakeSourceDirty = true;
                rippleBoundaryDirty = true;
            }

            if (continuousSourceIdsByOwner.TryGetValue(
                    source.OwnerId,
                    out EntityId ownedSourceId) &&
                EntityIdsEqual(ownedSourceId, sourceId))
            {
                continuousSourceIdsByOwner.Remove(source.OwnerId);
                ownershipConflictWarningOwnerIds.Remove(source.OwnerId);
            }

            continuousSources.Remove(sourceId);
        }

        private bool TryClaimContinuousSourceOwner(
            EntityId ownerId,
            EntityId sourceId,
            bool staticRegistrySource)
        {
            if (continuousSources.TryGetValue(
                    sourceId,
                    out ContinuousSource previousSource) &&
                !EntityIdsEqual(previousSource.OwnerId, ownerId) &&
                continuousSourceIdsByOwner.TryGetValue(
                    previousSource.OwnerId,
                    out EntityId previousOwnerSourceId) &&
                EntityIdsEqual(previousOwnerSourceId, sourceId))
            {
                continuousSourceIdsByOwner.Remove(previousSource.OwnerId);
            }

            if (continuousSourceIdsByOwner.TryGetValue(
                    ownerId,
                    out EntityId existingSourceId) &&
                !EntityIdsEqual(existingSourceId, sourceId))
            {
                if (!continuousSources.TryGetValue(
                        existingSourceId,
                        out ContinuousSource existingSource))
                {
                    continuousSourceIdsByOwner.Remove(ownerId);
                }
                else if (staticRegistrySource && !existingSource.IsStatic)
                {
                    RemoveContinuousSource(existingSourceId);
                }
                else
                {
                    ReportOwnershipConflict(ownerId);
                    return false;
                }
            }

            continuousSourceIdsByOwner[ownerId] = sourceId;
            ownershipConflictWarningOwnerIds.Remove(ownerId);
            return true;
        }

        private void ReportOwnershipConflict(EntityId ownerId)
        {
            if (!ownershipConflictWarningOwnerIds.Add(ownerId))
            {
                return;
            }

            Debug.LogWarning(
                $"River disturbance continuous-source ownership conflict " +
                $"on '{name}' for physical owner {ownerId}. Generated " +
                "stationary geometry takes precedence, and a second " +
                "continuous source for the same GameObject was rejected.",
                this);
        }

        private static bool EntityIdsEqual(EntityId left, EntityId right)
        {
            return EqualityComparer<EntityId>.Default.Equals(left, right);
        }

        public bool EmitDebugImpact(
            float distanceNormalized,
            float acrossNormalized,
            ImpactRippleEventSettings eventSettings)
        {
            if (!TryResolveDebugImpactWorldPosition(
                    distanceNormalized,
                    acrossNormalized,
                    out Vector3 worldPosition))
            {
                return false;
            }

            return EmitImpact(worldPosition, eventSettings);
        }

        public bool EmitDebugOppositeSignImpact(
            float distanceNormalized,
            float acrossNormalized,
            ImpactRippleEventSettings eventSettings)
        {
            return EmitDebugImpact(
                distanceNormalized,
                acrossNormalized,
                eventSettings.WithSignsReversed());
        }

        public bool EmitDebugOverlappingPair(
            float distanceNormalized,
            float acrossNormalized,
            ImpactRippleEventSettings eventSettings)
        {
            if (river == null || !river.Domain.IsValid)
            {
                return false;
            }

            float localDistance =
                river.Domain.LocalLength *
                Mathf.Clamp01(distanceNormalized);
            StylizedRiverSplineSample sample =
                river.Domain.SampleAtLocalDistance(localDistance);
            float baseHalfWidth =
                acrossNormalized < 0f
                    ? sample.LeftSurfaceHalfWidth
                    : sample.RightSurfaceHalfWidth;
            float baseAcrossMetres =
                Mathf.Clamp(acrossNormalized, -1f, 1f) *
                Mathf.Max(0.05f, baseHalfWidth);
            float availableLeft =
                Mathf.Max(0f, sample.LeftSurfaceHalfWidth + baseAcrossMetres);
            float availableRight =
                Mathf.Max(0f, sample.RightSurfaceHalfWidth - baseAcrossMetres);
            float maximumOffset =
                Mathf.Max(0f, Mathf.Min(availableLeft, availableRight) - 0.05f);
            float offset = Mathf.Min(
                eventSettings.Sanitized().Radius * 0.45f,
                maximumOffset * 0.5f);

            if (offset <= 0.001f)
            {
                return EmitDebugImpact(
                    distanceNormalized,
                    acrossNormalized,
                    eventSettings);
            }

            Vector3 leftPosition =
                sample.SurfacePoint +
                sample.Side * (baseAcrossMetres - offset);
            Vector3 rightPosition =
                sample.SurfacePoint +
                sample.Side * (baseAcrossMetres + offset);

            bool leftEmitted = EmitImpact(leftPosition, eventSettings);
            bool rightEmitted = EmitImpact(rightPosition, eventSettings);
            return leftEmitted || rightEmitted;
        }

        public bool EmitDebugNearShore(
            float distanceNormalized,
            float acrossNormalized,
            ImpactRippleEventSettings eventSettings)
        {
            float side = acrossNormalized < 0f ? -0.82f : 0.82f;
            return EmitDebugImpact(
                distanceNormalized,
                side,
                eventSettings);
        }

        private bool TryResolveDebugImpactWorldPosition(
            float distanceNormalized,
            float acrossNormalized,
            out Vector3 worldPosition)
        {
            worldPosition = default;
            if (river == null || !river.Domain.IsValid)
            {
                return false;
            }

            float localDistance =
                river.Domain.LocalLength *
                Mathf.Clamp01(distanceNormalized);
            StylizedRiverSplineSample sample =
                river.Domain.SampleAtLocalDistance(localDistance);
            float clampedAcross = Mathf.Clamp(
                acrossNormalized,
                -0.95f,
                0.95f);
            float halfWidth =
                clampedAcross < 0f
                    ? sample.LeftSurfaceHalfWidth
                    : sample.RightSurfaceHalfWidth;
            float acrossMetres =
                clampedAcross * Mathf.Max(0.05f, halfWidth);
            worldPosition =
                sample.SurfacePoint +
                sample.Side * acrossMetres;
            return true;
        }

        public static bool TryFindContainingRiver(
            Vector3 worldPosition,
            float maximumVerticalDistance,
            out StylizedRiverDisturbanceRuntime runtime,
            out StylizedRiverProjection projection)
        {
            runtime = null;
            projection = default;
            float bestVerticalDistance = float.PositiveInfinity;

            for (int index = ActiveRuntimes.Count - 1; index >= 0; index--)
            {
                StylizedRiverDisturbanceRuntime candidate =
                    ActiveRuntimes[index];

                if (candidate == null)
                {
                    ActiveRuntimes.RemoveAt(index);
                    continue;
                }

                if (!candidate.isActiveAndEnabled)
                {
                    continue;
                }

                StylizedRiver candidateRiver = candidate.river;
                if (candidateRiver == null ||
                    !candidateRiver.isActiveAndEnabled ||
                    !candidateRiver.RuntimeDisturbancesEnabled ||
                    !candidateRiver.TryProjectWorldPoint(
                        worldPosition,
                        out StylizedRiverProjection candidateProjection) ||
                    !candidateProjection.IsInside)
                {
                    continue;
                }

                float verticalDistance = Mathf.Abs(
                    worldPosition.y -
                    candidateProjection.SurfacePoint.y);

                if (verticalDistance > maximumVerticalDistance ||
                    verticalDistance >= bestVerticalDistance)
                {
                    continue;
                }

                runtime = candidate;
                projection = candidateProjection;
                bestVerticalDistance = verticalDistance;
            }

            return runtime != null;
        }

        private void HandleDomainChanged(RiverDomainSnapshot snapshot)
        {
            resourcesDirty = true;
            staticPressureTargetDirty = true;
            staticWakeSourceDirty = true;
            rippleBoundaryDirty = true;
            generatedGeometryRegistryDirty = true;
        }

        private void HandleGeneratedGeometrySourceAdded(
            IGeneratedGeometrySource source)
        {
            generatedGeometryRegistryDirty = true;
        }

        private void HandleGeneratedGeometrySourceRemoved(
            IGeneratedGeometrySource source)
        {
            generatedGeometryRegistryDirty = true;
        }

        private void HandleGeneratedGeometrySourceChanged(
            IGeneratedGeometrySource source)
        {
            generatedGeometryRegistryDirty = true;
        }

        private void RefreshGeneratedGeometrySources()
        {
            if (river == null ||
                !river.isActiveAndEnabled ||
                !river.RuntimeDisturbancesEnabled ||
                !river.Domain.IsValid ||
                !river.TryGetSurfaceBounds(out Bounds currentRiverBounds))
            {
                generatedGeometryRefreshInProgress = false;
                generatedGeometryRefreshIndex = 0;
                return;
            }

            if (!generatedGeometryRefreshInProgress)
            {
                refreshedAutomaticGeneratedSourceIds.Clear();
                GeneratedGeometryRegistry.CopySourcesTo(
                    generatedGeometryScratch);

                currentRiverBounds.Expand(
                    new Vector3(
                        AutomaticBoundsHorizontalPadding * 2f,
                        AutomaticBoundsVerticalPadding * 2f,
                        AutomaticBoundsHorizontalPadding * 2f));

                generatedGeometryRefreshBounds = currentRiverBounds;
                generatedGeometryRefreshIndex = 0;
                generatedGeometryRefreshInProgress = true;

                // New registry events may set this back to true while the
                // current refresh is in flight. In that case another refresh
                // begins after this budgeted pass completes.
                generatedGeometryRegistryDirty = false;
            }

            int processedThisFrame = 0;
            while (generatedGeometryRefreshIndex <
                       generatedGeometryScratch.Count &&
                   processedThisFrame < GeneratedSourcesPerFrame)
            {
                IGeneratedGeometrySource source =
                    generatedGeometryScratch[generatedGeometryRefreshIndex++];
                ProcessGeneratedGeometrySource(source);
                processedThisFrame++;
            }

            if (generatedGeometryRefreshIndex <
                generatedGeometryScratch.Count)
            {
                return;
            }

            foreach (EntityId sourceId in automaticGeneratedSourceIds)
            {
                if (!refreshedAutomaticGeneratedSourceIds.Contains(sourceId))
                {
                    RemoveContinuousSource(sourceId);
                    RemoveGeneratedDiagnostic(sourceId);
                }
            }

            automaticGeneratedSourceIds.Clear();
            automaticGeneratedSourceIds.UnionWith(
                refreshedAutomaticGeneratedSourceIds);
            generatedGeometryRefreshInProgress = false;
            generatedGeometryRefreshIndex = 0;
            obstacleGeometryVersion++;
            staticPressureTargetDirty = true;
            staticWakeSourceDirty = true;
            rippleBoundaryDirty = true;
            lastActivityTime = Time.realtimeSinceStartupAsDouble;
        }

        private ResolvedGeneratedRiverInteraction ResolveGeneratedInteraction(
            GeneratedRiverInteractionSettings settings)
        {
            settings?.Validate();

            GeneratedRiverFeatureMode pressureMode = settings != null
                ? settings.StaticPressureMode
                : GeneratedRiverFeatureMode.Inherit;
            GeneratedRiverFeatureMode wakeMode = settings != null
                ? settings.ObstructionWakeMode
                : GeneratedRiverFeatureMode.Inherit;
            GeneratedRiverRippleCollisionMode rippleCollisionMode =
                settings != null
                    ? settings.ImpactRippleCollisionMode
                    : GeneratedRiverRippleCollisionMode.Inherit;

            bool pressureEnabled =
                pressureMode != GeneratedRiverFeatureMode.Disabled;
            bool wakeEnabled =
                wakeMode != GeneratedRiverFeatureMode.Disabled;
            bool rippleCollisionEnabled =
                rippleCollisionMode !=
                GeneratedRiverRippleCollisionMode.Disabled;

            float pressureStrength =
                pressureMode == GeneratedRiverFeatureMode.Custom
                    ? settings.StaticPressureStrength
                    : river.PressureStrength;
            float contactSharpness =
                pressureMode == GeneratedRiverFeatureMode.Custom
                    ? settings.StaticPressureContactSharpness
                    : river.PressureContactSharpness;
            float profileVariation =
                pressureMode == GeneratedRiverFeatureMode.Custom
                    ? settings.StaticPressureProfileVariation
                    : river.PressureProfileVariation;
            float profileChangeIntervalMin =
                pressureMode == GeneratedRiverFeatureMode.Custom
                    ? settings.StaticPressureProfileChangeIntervalMin
                    : river.PressureProfileChangeIntervalMin;
            float profileChangeIntervalMax =
                pressureMode == GeneratedRiverFeatureMode.Custom
                    ? settings.StaticPressureProfileChangeIntervalMax
                    : river.PressureProfileChangeIntervalMax;
            float wakeStrength =
                wakeMode == GeneratedRiverFeatureMode.Custom
                    ? settings.ObstructionWakeStrength
                    : river.WakeStrength;
            float wakeReach =
                wakeMode == GeneratedRiverFeatureMode.Custom
                    ? settings.ObstructionWakeReach
                    : river.WakeReach;
            float wakeSpread =
                wakeMode == GeneratedRiverFeatureMode.Custom
                    ? settings.ObstructionWakeSpread
                    : river.WakeSpread;
            float wakeVariation =
                wakeMode == GeneratedRiverFeatureMode.Custom
                    ? settings.ObstructionWakeVariation
                    : river.WakeVariation;

            return new ResolvedGeneratedRiverInteraction(
                pressureEnabled,
                pressureStrength,
                contactSharpness,
                profileVariation,
                profileChangeIntervalMin,
                profileChangeIntervalMax,
                wakeEnabled,
                wakeStrength,
                wakeReach,
                wakeSpread,
                wakeVariation,
                rippleCollisionEnabled);
        }

        private void ProcessGeneratedGeometrySource(
            IGeneratedGeometrySource source)
        {
            if (source == null ||
                (source is UnityEngine.Object unityObject &&
                 unityObject == null) ||
                !source.IsSolidGeometry ||
                !source.IsStaticGeometry)
            {
                return;
            }

            GeneratedRiverInteractionSettings authoredSettings =
                source is IGeneratedRiverInteractionSource interactionSource
                    ? interactionSource.RiverInteractionSettings
                    : null;

            if (authoredSettings != null &&
                authoredSettings.Participation ==
                GeneratedRiverInteractionParticipation.Disabled)
            {
                return;
            }

            ResolvedGeneratedRiverInteraction interaction =
                ResolveGeneratedInteraction(authoredSettings);

            if (!interaction.StaticPressureEnabled &&
                !interaction.ObstructionWakeEnabled &&
                !interaction.ImpactRippleCollisionEnabled)
            {
                return;
            }

            MeshFilter meshFilter = source.GeometryMeshFilter;
            if (meshFilter == null ||
                meshFilter.sharedMesh == null ||
                !meshFilter.gameObject.activeInHierarchy ||
                !RiverDisturbanceFootprintResolver.TryGetWorldBounds(
                    meshFilter,
                    out Bounds sourceBounds) ||
                !generatedGeometryRefreshBounds.Intersects(sourceBounds) ||
                !river.TryProjectWorldPoint(
                    sourceBounds.center,
                    out StylizedRiverProjection boundsProjection))
            {
                return;
            }

            StylizedRiverSplineSample boundsSample =
                river.SampleAtLocalDistance(
                    boundsProjection.LocalDistance);
            float preliminaryRiverWidth = Mathf.Max(
                0.10f,
                boundsSample.LeftSurfaceHalfWidth +
                boundsSample.RightSurfaceHalfWidth);
            float effectivePadding = ResolveAutomaticFootprintPadding(
                preliminaryRiverWidth,
                DefaultGeneratedFootprintPadding);

            if (!RiverDisturbanceFootprintResolver.TryResolveBoundsOnly(
                    river,
                    meshFilter,
                    effectivePadding,
                    out RiverDisturbanceFootprint footprint,
                    out string footprintStatus) ||
                !river.TryProjectWorldPoint(
                    footprint.WorldPosition,
                    out StylizedRiverProjection footprintProjection) ||
                !footprintProjection.IsInside)
            {
                return;
            }

            StylizedRiverSplineSample sample =
                river.SampleAtLocalDistance(
                    footprintProjection.LocalDistance);
            float localRiverWidth = Mathf.Max(
                0.10f,
                sample.LeftSurfaceHalfWidth +
                sample.RightSurfaceHalfWidth);
            float unpaddedAcrossHalfWidth = Mathf.Max(
                0.05f,
                footprint.AcrossHalfWidth - effectivePadding);
            float blockageRatio = Mathf.Clamp01(
                unpaddedAcrossHalfWidth * 2f / localRiverWidth);
            float blockageInfluence = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.InverseLerp(
                    0.04f,
                    0.55f,
                    blockageRatio));

            RiverDisturbanceFootprint pressureFootprint = footprint;
            RiverDisturbanceFootprint collisionFootprint = footprint;
            if ((interaction.StaticPressureEnabled ||
                 interaction.ImpactRippleCollisionEnabled) &&
                RiverDisturbanceFootprintResolver.TryResolveBoundsOnly(
                    river,
                    meshFilter,
                    0f,
                    out RiverDisturbanceFootprint rawFootprint,
                    out _))
            {
                pressureFootprint = rawFootprint;
                collisionFootprint = rawFootprint;
            }

            RiverDisturbancePressureBakeProfile pressureProfile = default;
            float waveAllowance = 0f;
            float representativeSupportHeight = 0f;
            float minimumAllowedPressureHeight = 0f;
            float maximumAllowedPressureHeight = 0f;
            float targetPressureHeight = 0f;
            float unboundedPressureMaximum = 0f;
            float supportInspectionHeight = 0f;
            bool heightClampReached = false;
            string pressureStatus = "Static pressure disabled.";

            if (interaction.StaticPressureEnabled)
            {
                waveAllowance = Mathf.Clamp(
                    river.MotionWaveHeight * 1.15f + 0.04f,
                    0.04f,
                    0.45f);
                float absoluteFlowSpeed =
                    Mathf.Abs(river.FlowSpeedMetresPerSecond);
                float velocityHead =
                    absoluteFlowSpeed * absoluteFlowSpeed /
                    (2f * Mathf.Max(0.001f, Physics.gravity.magnitude));
                float blockageCoefficient = Mathf.Lerp(
                    0.90f,
                    2.60f,
                    blockageInfluence);

                // Flow determines demand; local height-aware support remains
                // the hard ceiling. The stylized coefficient deliberately
                // makes the former Strong result approximately the new safe
                // lower response for ordinary gameplay-speed rivers.
                unboundedPressureMaximum =
                    velocityHead * blockageCoefficient * 5.00f;

                // Focus the fixed eight support slices on the height range
                // that this river can actually request. The wave allowance and
                // safety margin preserve headroom without spending vertical
                // resolution on irrelevant upper geometry.
                supportInspectionHeight =
                    Mathf.Min(
                        MaximumStaticPressureHeightMetres,
                        unboundedPressureMaximum *
                        MaximumStaticPressureModulation) +
                    waveAllowance + 0.10f;
                int pressureLateralSampleCount =
                    ResolveStaticPressureLateralSampleCount(
                        pressureFootprint.AcrossHalfWidth,
                        localRiverWidth);

                // Performance cap: automatic generated sources may not
                // height-slice or rescan triangles on Play startup. Use the
                // cached footprint contour as the pressure support source.
                if (!RiverDisturbanceFootprintResolver
                    .TryResolvePressureSupportFromFootprint(
                        pressureFootprint,
                        supportInspectionHeight,
                        pressureLateralSampleCount,
                        out RiverDisturbancePressureSupportProfile pressureSupport,
                        out pressureStatus))
                {
                    return;
                }

                representativeSupportHeight =
                    pressureSupport.RepresentativeHeight;
                float supportBudget = Mathf.Max(
                    0f,
                    representativeSupportHeight - waveAllowance);
                float supportCeiling = Mathf.Min(
                    supportBudget / MaximumStaticPressureModulation,
                    MaximumStaticPressureHeightMetres /
                    MaximumStaticPressureModulation);
                maximumAllowedPressureHeight = Mathf.Min(
                    supportCeiling,
                    unboundedPressureMaximum);
                minimumAllowedPressureHeight = Mathf.Min(
                    maximumAllowedPressureHeight,
                    maximumAllowedPressureHeight * 0.35f +
                    Mathf.Min(0.050f, supportCeiling * 0.10f));
                targetPressureHeight = Mathf.Lerp(
                    minimumAllowedPressureHeight,
                    maximumAllowedPressureHeight,
                    interaction.StaticPressureStrength);
                heightClampReached =
                    unboundedPressureMaximum > supportCeiling + 0.0001f;

                if (targetPressureHeight > 0.0001f &&
                    !RiverDisturbanceFootprintResolver.TryBuildPressureBakeProfile(
                        pressureSupport,
                        targetPressureHeight,
                        MaximumStaticPressureModulation,
                        out pressureProfile))
                {
                    return;
                }
            }

            float wakeAmplitude = 0f;
            if (interaction.ObstructionWakeEnabled)
            {
                float wakeFlowFactor = Mathf.Lerp(
                    0.20f,
                    1.35f,
                    Mathf.InverseLerp(
                        0.05f,
                        2.5f,
                        Mathf.Abs(river.FlowSpeedMetresPerSecond)));
                wakeAmplitude = Mathf.Max(
                    0f,
                    (0.55f + blockageInfluence * 1.15f) *
                    wakeFlowFactor *
                    interaction.ObstructionWakeStrength);
            }

            EntityId sourceId = meshFilter.GetEntityId();
            EntityId ownerId = meshFilter.gameObject.GetEntityId();
            if (!RegisterStaticSource(
                    sourceId,
                    ownerId,
                    footprint.WorldPosition,
                    footprint.AcrossHalfWidth,
                    footprint.AlongHalfLength,
                    1f,
                    1f,
                    1f,
                    -1f,
                    wakeAmplitude,
                    interaction.StaticPressureContactSharpness,
                    interaction.ObstructionWakeReach,
                    interaction.StaticPressureProfileVariation,
                    footprint.Contour,
                    targetPressureHeight,
                    pressureFootprint.AcrossHalfWidth,
                    pressureFootprint.AlongHalfLength,
                    pressureFootprint.Contour,
                    pressureProfile,
                    meshFilter,
                    true,
                    interaction.ObstructionWakeSpread,
                    interaction.StaticPressureProfileChangeIntervalMin,
                    interaction.StaticPressureProfileChangeIntervalMax,
                    interaction.ObstructionWakeVariation,
                    river.WakeVariationIntervalMin,
                    river.WakeVariationIntervalMax,
                    interaction.ImpactRippleCollisionEnabled,
                    collisionFootprint.AcrossHalfWidth,
                    collisionFootprint.AlongHalfLength,
                    collisionFootprint.Contour))
            {
                return;
            }

            refreshedAutomaticGeneratedSourceIds.Add(sourceId);
            GeneratedSourceDiagnostics[sourceId] =
                new GeneratedRiverDisturbanceDiagnostics(
                    river,
                    true,
                    footprint.AcrossHalfWidth * 2f,
                    footprint.AlongHalfLength * 2f,
                    localRiverWidth,
                    blockageRatio,
                    effectivePadding,
                    targetPressureHeight,
                    wakeAmplitude,
                    maximumAllowedPressureHeight,
                    heightClampReached,
                    representativeSupportHeight,
                    minimumAllowedPressureHeight,
                    maximumAllowedPressureHeight,
                    interaction.StaticPressureStrength,
                    waveAllowance,
                    supportInspectionHeight,
                    interaction.StaticPressureEnabled,
                    interaction.StaticPressureContactSharpness,
                    interaction.StaticPressureProfileVariation,
                    interaction.ObstructionWakeEnabled,
                    interaction.ObstructionWakeReach,
                    interaction.ObstructionWakeSpread,
                    interaction.ObstructionWakeVariation,
                    footprintStatus + " " + pressureStatus + " " +
                    $"Contour {footprint.Contour.Length} points; " +
                    $"blockage {blockageRatio:P0}; " +
                    $"pressure strength {interaction.StaticPressureStrength:P0}; " +
                    "ripple collision " +
                    (interaction.ImpactRippleCollisionEnabled
                        ? "enabled."
                        : "disabled."));
        }

        private int ResolveStaticPressureLateralSampleCount(
            float pressureAcrossHalfWidth,
            float localRiverWidth)
        {
            int localFieldHeight = river.Quality switch
            {
                StylizedRiverQuality.Low => 32,
                StylizedRiverQuality.Medium => 48,
                StylizedRiverQuality.High => 64,
                _ => 48
            };
            float profilePixelWidth =
                Mathf.Max(0.10f, pressureAcrossHalfWidth * 2f) /
                Mathf.Max(0.10f, localRiverWidth) *
                localFieldHeight;
            return RiverDisturbanceFootprintResolver.
                ResolvePressureSupportLateralSampleCount(
                    Mathf.CeilToInt(profilePixelWidth));
        }

        private int ResolveStaticWakeVariationLateralSampleCount(
            float wakeAcrossHalfWidth,
            float localRiverWidth)
        {
            int localFieldHeight = wakeFieldHeight > 0
                ? wakeFieldHeight
                : river.Quality switch
                {
                    StylizedRiverQuality.Low => 32,
                    StylizedRiverQuality.Medium => 48,
                    StylizedRiverQuality.High => 64,
                    _ => 48
                };
            float profilePixelWidth =
                Mathf.Max(0.10f, wakeAcrossHalfWidth * 2f) /
                Mathf.Max(0.10f, localRiverWidth) *
                localFieldHeight;
            return RiverDisturbanceFootprintResolver.
                ResolvePressureSupportLateralSampleCount(
                    Mathf.CeilToInt(profilePixelWidth));
        }

        private float ResolveAutomaticFootprintPadding(
            float localRiverWidth,
            float authoredPadding)
        {
            int localResolutionPerChunk = river.Quality switch
            {
                StylizedRiverQuality.Low => 64,
                StylizedRiverQuality.Medium => 96,
                StylizedRiverQuality.High => 128,
                _ => 96
            };
            int localFieldHeight = river.Quality switch
            {
                StylizedRiverQuality.Low => 32,
                StylizedRiverQuality.Medium => 48,
                StylizedRiverQuality.High => 64,
                _ => 48
            };
            float longitudinalFieldCell =
                ChunkLengthMetres / localResolutionPerChunk;
            float lateralFieldCell =
                localRiverWidth / Mathf.Max(1, localFieldHeight);
            float surfaceSpacing = Mathf.Max(
                0.05f,
                river.ResolvedSurfaceLongitudinalSpacing);
            float resolutionMinimum = Mathf.Max(
                0.12f,
                longitudinalFieldCell * 0.70f,
                lateralFieldCell * 0.65f,
                surfaceSpacing * 0.55f);
            return Mathf.Max(
                Mathf.Max(0f, authoredPadding),
                resolutionMinimum);
        }

        private void RemoveGeneratedDiagnostic(EntityId sourceId)
        {
            if (GeneratedSourceDiagnostics.TryGetValue(
                    sourceId,
                    out GeneratedRiverDisturbanceDiagnostics diagnostics) &&
                diagnostics.River == river)
            {
                GeneratedSourceDiagnostics.Remove(sourceId);
            }
        }

        private void RemoveOwnedGeneratedDiagnostics()
        {
            foreach (EntityId sourceId in automaticGeneratedSourceIds)
            {
                RemoveGeneratedDiagnostic(sourceId);
            }
        }

        private bool EnsureResources()
        {
            if (river == null || !river.Domain.IsValid)
            {
                return false;
            }

            if (!resourcesDirty &&
                currentState != null &&
                currentWake != null &&
                rippleBoundary != null &&
                domainVersion == river.Domain.Version)
            {
                return true;
            }

            ReleaseResources();
            RecordFieldRebuild();

            computeShader = Resources.Load<ComputeShader>(
                ComputeResourcePath);

            if (computeShader == null)
            {
                Debug.LogError(
                    $"StylizedRiver on '{name}' could not load compute shader Resources/{ComputeResourcePath}.",
                    this);
                return false;
            }

            clearKernel = computeShader.FindKernel("ClearRange");
            injectRippleKernel = computeShader.FindKernel("InjectRipple");
            injectWakeKernel = computeShader.FindKernel("InjectWake");
            bakeStaticPressureKernel = computeShader.FindKernel("BakeStaticPressure");
            finalizeStaticPressureKernel = computeShader.FindKernel("FinalizeStaticPressure");
            bakeStaticWakeSourceKernel = computeShader.FindKernel("BakeStaticWakeSource");
            bakeRippleBoundaryBaseKernel =
                computeShader.FindKernel("BakeRippleBoundaryBase");
            bakeRippleBoundaryObstacleKernel =
                computeShader.FindKernel("BakeRippleBoundaryObstacle");
            applyRippleBoundaryKernel =
                computeShader.FindKernel("ApplyRippleBoundary");
            simulateRippleKernel = computeShader.FindKernel("SimulateRipple");
            simulateWakeKernel = computeShader.FindKernel("SimulateWake");

            chunkCount = Mathf.Max(
                1,
                Mathf.CeilToInt(
                    river.Domain.LocalLength /
                    ChunkLengthMetres));

            resolutionPerChunk = river.Quality switch
            {
                StylizedRiverQuality.Low => 64,
                StylizedRiverQuality.Medium => 96,
                StylizedRiverQuality.High => 128,
                _ => 96
            };
            wakeResolutionPerChunk = river.Quality switch
            {
                StylizedRiverQuality.Low => 48,
                StylizedRiverQuality.Medium => 96,
                StylizedRiverQuality.High => 128,
                _ => 96
            };

            int maximumTextureSize = SystemInfo.maxTextureSize;
            if (!TryResolveChunkedTextureWidth(
                    chunkCount,
                    resolutionPerChunk,
                    16,
                    maximumTextureSize,
                    out resolutionPerChunk,
                    out fieldWidth) ||
                !TryResolveChunkedTextureWidth(
                    chunkCount,
                    wakeResolutionPerChunk,
                    16,
                    maximumTextureSize,
                    out wakeResolutionPerChunk,
                    out wakeFieldWidth))
            {
                ReportAllocationFailure(maximumTextureSize);
                return false;
            }

            fieldHeight = river.Quality switch
            {
                StylizedRiverQuality.Low => 32,
                StylizedRiverQuality.Medium => 48,
                StylizedRiverQuality.High => 64,
                _ => 48
            };
            wakeFieldHeight = river.Quality switch
            {
                StylizedRiverQuality.Low => 20,
                StylizedRiverQuality.Medium => 32,
                StylizedRiverQuality.High => 48,
                _ => 32
            };
            if (fieldHeight > maximumTextureSize ||
                wakeFieldHeight > maximumTextureSize)
            {
                ReportAllocationFailure(maximumTextureSize);
                return false;
            }

            fieldLength = chunkCount * ChunkLengthMetres;
            validFieldLength = river.Domain.LocalLength;
            validFieldWidth = ResolveValidColumnCount(
                fieldWidth,
                validFieldLength,
                fieldLength);
            validWakeFieldWidth = ResolveValidColumnCount(
                wakeFieldWidth,
                validFieldLength,
                fieldLength);
            allocationWarningReported = false;
            averageSurfaceHalfWidth = ResolveAverageSurfaceHalfWidth();
            domainVersion = river.Domain.Version;
            SetValidDomainComputeParameters();

            if (!BuildRippleMetricData())
            {
                ReleaseResources();
                return false;
            }

            stateA = CreateFieldTexture(
                "PS3D_RiverDisturbance_RippleA",
                fieldWidth,
                fieldHeight);
            stateB = CreateFieldTexture(
                "PS3D_RiverDisturbance_RippleB",
                fieldWidth,
                fieldHeight);
            staticTarget = CreateFieldTexture(
                "PS3D_RiverDisturbance_StaticPressure",
                fieldWidth,
                fieldHeight);
            rippleBoundary = CreateBoundaryTexture(
                "PS3D_RiverDisturbance_RippleBoundary",
                fieldWidth,
                fieldHeight);
            wakeA = CreateFieldTexture(
                "PS3D_RiverDisturbance_WakeA",
                wakeFieldWidth,
                wakeFieldHeight);
            wakeB = CreateFieldTexture(
                "PS3D_RiverDisturbance_WakeB",
                wakeFieldWidth,
                wakeFieldHeight);
            staticWakeSource = CreateFieldTexture(
                "PS3D_RiverDisturbance_StaticWakeSource",
                wakeFieldWidth,
                wakeFieldHeight);
            currentState = stateA;
            previousState = stateA;
            writeState = stateB;
            currentWake = wakeA;
            previousWake = wakeA;
            writeWake = wakeB;

            chunkActiveUntil = new double[chunkCount];
            chunkActive = new bool[chunkCount];
            chunkHasStaticSource = new bool[chunkCount];
            wakeChunkActiveUntil = new double[chunkCount];
            staticWakeChunkReleaseDuration = new double[chunkCount];
            wakeChunkActive = new bool[chunkCount];

            DispatchClear(stateA, fieldWidth, fieldHeight, 0, fieldWidth);
            DispatchClear(stateB, fieldWidth, fieldHeight, 0, fieldWidth);
            DispatchClear(staticTarget, fieldWidth, fieldHeight, 0, fieldWidth);
            DispatchClear(wakeA, wakeFieldWidth, wakeFieldHeight, 0, wakeFieldWidth);
            DispatchClear(wakeB, wakeFieldWidth, wakeFieldHeight, 0, wakeFieldWidth);
            DispatchClear(
                staticWakeSource,
                wakeFieldWidth,
                wakeFieldHeight,
                0,
                wakeFieldWidth);
            simulationAccumulator = 0f;
            staticWakeVariationAccumulator = 0f;
            simulationInterpolation = 1f;
            wakeInterpolation = 1f;
            validStaticSourceCount = 0;
            validStaticWakeSourceCount = 0;
            staticPressureTargetDirty = true;
            staticWakeSourceDirty = true;
            rippleBoundaryDirty = true;
            resourcesDirty = false;
            RebuildRippleBoundary(Time.realtimeSinceStartupAsDouble);
            return true;
        }

        private static bool TryResolveChunkedTextureWidth(
            int chunks,
            int desiredResolutionPerChunk,
            int minimumResolutionPerChunk,
            int maximumTextureSize,
            out int resolvedResolutionPerChunk,
            out int resolvedWidth)
        {
            resolvedResolutionPerChunk = 0;
            resolvedWidth = 0;
            if (chunks < 1 ||
                maximumTextureSize < minimumResolutionPerChunk ||
                (long)chunks * minimumResolutionPerChunk > maximumTextureSize)
            {
                return false;
            }

            resolvedResolutionPerChunk = Math.Min(
                desiredResolutionPerChunk,
                maximumTextureSize / chunks);
            if (resolvedResolutionPerChunk < minimumResolutionPerChunk)
            {
                return false;
            }

            long width = (long)resolvedResolutionPerChunk * chunks;
            if (width < 1 || width > maximumTextureSize)
            {
                return false;
            }

            resolvedWidth = (int)width;
            return true;
        }

        private static int ResolveValidColumnCount(
            int textureWidth,
            float validLength,
            float storageLength)
        {
            if (textureWidth <= 1)
            {
                return Mathf.Clamp(textureWidth, 0, 1);
            }

            float lastValidIndex =
                Mathf.Clamp01(validLength / Mathf.Max(0.001f, storageLength)) *
                (textureWidth - 1);
            // Include the first sample at or beyond the endpoint as a
            // deliberate one-cell outflow guard. Unlike the old padded tail,
            // it cannot own sources and all following columns are hard-zeroed.
            return Mathf.Clamp(
                Mathf.CeilToInt(lastValidIndex) + 1,
                1,
                textureWidth);
        }

        private void SetValidDomainComputeParameters()
        {
            if (computeShader == null)
            {
                return;
            }

            computeShader.SetInt("_ValidFieldWidth", validFieldWidth);
            computeShader.SetInt("_ValidWakeWidth", validWakeFieldWidth);
        }

        private void ReportAllocationFailure(int maximumTextureSize)
        {
            if (allocationWarningReported)
            {
                return;
            }

            Debug.LogWarning(
                $"StylizedRiver disturbance field on '{name}' is disabled " +
                $"because the required textures for {chunkCount} chunks " +
                $"cannot fit within the hardware texture limit of " +
                $"{maximumTextureSize} pixels. The field requires at least " +
                "16 columns per chunk.",
                this);
            allocationWarningReported = true;
        }

        private RenderTexture CreateFieldTexture(
            string textureName,
            int width,
            int height)
        {
            RenderTexture texture = new RenderTexture(
                width,
                height,
                0,
                RenderTextureFormat.ARGBHalf,
                RenderTextureReadWrite.Linear)
            {
                name = textureName,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            texture.Create();
            return texture;
        }

        private RenderTexture CreateBoundaryTexture(
            string textureName,
            int width,
            int height)
        {
            RenderTexture texture = new RenderTexture(
                width,
                height,
                0,
                RenderTextureFormat.RGHalf,
                RenderTextureReadWrite.Linear)
            {
                name = textureName,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            texture.Create();
            return texture;
        }

        private void ReleaseResources()
        {
            ReleaseBuffer(ref rippleMetricBuffer);
            ReleaseTexture(ref stateA);
            ReleaseTexture(ref stateB);
            ReleaseTexture(ref staticTarget);
            ReleaseTexture(ref staticWakeSource);
            ReleaseTexture(ref rippleBoundary);
            ReleaseTexture(ref wakeA);
            ReleaseTexture(ref wakeB);
            currentState = null;
            previousState = null;
            writeState = null;
            currentWake = null;
            previousWake = null;
            writeWake = null;
            computeShader = null;
            clearKernel = -1;
            injectRippleKernel = -1;
            injectWakeKernel = -1;
            bakeStaticPressureKernel = -1;
            finalizeStaticPressureKernel = -1;
            bakeStaticWakeSourceKernel = -1;
            bakeRippleBoundaryBaseKernel = -1;
            bakeRippleBoundaryObstacleKernel = -1;
            applyRippleBoundaryKernel = -1;
            simulateRippleKernel = -1;
            simulateWakeKernel = -1;
            fieldWidth = 0;
            fieldHeight = 0;
            wakeFieldWidth = 0;
            wakeFieldHeight = 0;
            chunkCount = 0;
            resolutionPerChunk = 0;
            wakeResolutionPerChunk = 0;
            fieldLength = 0f;
            validFieldLength = 0f;
            validFieldWidth = 0;
            validWakeFieldWidth = 0;
            domainVersion = -1;
            rippleMetricMinimumAlongCell = Array.Empty<float>();
            rippleMetricMinimumLateralCell = Array.Empty<float>();
            rippleChunkMaximumInverseLength = Array.Empty<float>();
            rippleChunkMinimumCellSize = Array.Empty<float>();
            activeRippleMinimumCellSize = 0f;
            rippleSubstepLimitReached = false;
            activeImpactReservations.Clear();
            chunkActiveUntil = Array.Empty<double>();
            chunkActive = Array.Empty<bool>();
            chunkHasStaticSource = Array.Empty<bool>();
            wakeChunkActiveUntil = Array.Empty<double>();
            staticWakeChunkReleaseDuration = Array.Empty<double>();
            wakeChunkActive = Array.Empty<bool>();
            validStaticSourceCount = 0;
            validStaticWakeSourceCount = 0;
            rippleCollisionSourceCount = 0;
            staticPressureTargetDirty = true;
            staticWakeSourceDirty = true;
            rippleBoundaryDirty = true;
            resourcesDirty = true;
        }

        private static void ReleaseBuffer(ref ComputeBuffer buffer)
        {
            if (buffer == null)
            {
                return;
            }

            buffer.Release();
            buffer = null;
        }

        private static void ReleaseTexture(ref RenderTexture texture)
        {
            if (texture == null)
            {
                return;
            }

            if (texture.IsCreated())
            {
                texture.Release();
            }

            if (Application.isPlaying)
            {
                Destroy(texture);
            }
            else
            {
                DestroyImmediate(texture);
            }

            texture = null;
        }

        private void SimulateStep(float deltaTime, double now)
        {
            // TODO: Tighten these broad dirty flags so source/profile changes
            // rebuild only affected Static Pressure, Static Wake, and ripple
            // boundary textures instead of whole-pass targets.
            if (staticPressureTargetDirty)
            {
                RebuildStaticPressureTarget(now);
            }

            if (staticWakeSourceDirty)
            {
                RebuildStaticWakeSource(now);
            }

            if (rippleBoundaryDirty)
            {
                RebuildRippleBoundary(now);
            }

            float reservationLookAhead =
                ResolveImpactReservationLookAhead(deltaTime);
            ResetRippleChunkReservationDeadlines(now);
            UpdateImpactReservations(
                now,
                deltaTime,
                reservationLookAhead);
            ExpireChunks(now);
            ExpireWakeChunks(now);

            impactsInjectedLastStep = pendingImpacts.Count;
            for (int index = 0; index < pendingImpacts.Count; index++)
            {
                ImpactCommand impact = pendingImpacts[index];
                ImpactReservation reservation =
                    CreateImpactReservation(impact, now);
                if (UpdateImpactReservation(
                        ref reservation,
                        now,
                        0f,
                        reservationLookAhead))
                {
                    activeImpactReservations.Add(reservation);
                }
                DispatchRippleInjection(impact);
            }

            pendingImpacts.Clear();

            foreach (KeyValuePair<EntityId, ContinuousSource> pair in continuousSources)
            {
                ContinuousSource source = pair.Value;
                if (source.IsStatic)
                {
                    continue;
                }

                float absoluteFlowSpeed =
                    Mathf.Abs(river.FlowSpeedMetresPerSecond);
                float movementBlend = source.StationaryObstruction
                    ? Mathf.SmoothStep(
                        0f,
                        1f,
                        Mathf.InverseLerp(
                            StationarySpeedStart,
                            MovingSpeedFull,
                            source.MovementSpeed))
                    : 1f;
                float flowInfluence = Mathf.Lerp(
                    0.35f,
                    1.25f,
                    Mathf.InverseLerp(0f, 2.5f, absoluteFlowSpeed));
                float movementInfluence = Mathf.Lerp(
                    0.45f,
                    1.55f,
                    Mathf.InverseLerp(0f, 3f, source.MovementSpeed));
                // Source-local strength is multiplied by the river's
                // canonical Wake Strength; there is no separate dynamic-wake
                // visual rule or source-specific response afterward.
                float wakeStrength =
                    source.Strength *
                    river.WakeStrength *
                    Mathf.Clamp01(source.NormalContribution) *
                    flowInfluence *
                    Mathf.Lerp(0.65f, movementInfluence, movementBlend);

                float segmentCentre =
                    (source.StartDistance + source.EndDistance) * 0.5f;
                float segmentHalfLength = Mathf.Abs(
                    source.EndDistance - source.StartDistance) * 0.5f;
                // Dynamic emitters prepare a swept source footprint, while
                // stationary geometry prepares cached contour releases. Both
                // then consume the same canonical Wake response settings.
                float wakeReach = ResolveObstructionWakeLength(
                    source.AcrossHalfWidth,
                    source.AlongHalfLength,
                    absoluteFlowSpeed) * river.WakeReach;
                MarkWakeActive(
                    segmentCentre + wakeReach * 0.5f,
                    segmentHalfLength + wakeReach * 0.5f +
                    Mathf.Max(source.AcrossHalfWidth, source.AlongHalfLength),
                    now);

                StylizedRiverSplineSample sample =
                    river.Domain.SampleAtGlobalDistance(source.EndDistance);
                float surfaceHalf = Mathf.Max(
                    0.05f,
                    sample.GetSurfaceHalfWidth(source.EndAcrossNormalized));

                DispatchWakeInjection(
                    source,
                    surfaceHalf,
                    wakeStrength,
                    movementBlend,
                    deltaTime);
            }

            SimulateRippleField(deltaTime);
            SimulateWakeField(deltaTime, now);
            simulationInterpolation = 0f;
            wakeInterpolation = 0f;
        }

        private void SimulateRippleField(float deltaTime)
        {
            if (!HasRippleActiveChunks())
            {
                activeRippleMinimumCellSize = 0f;
                rippleSubstepLimitReached = false;
                RecordRippleSubstepDiagnostics(0);
                return;
            }

            float propagationSpeed = Mathf.Max(
                0.01f,
                river.ImpactRipplePropagation);
            float inverseLength = ResolveActiveRippleStabilityInverseLength(
                out activeRippleMinimumCellSize);
            float maximumStableStep =
                RippleStabilitySafety /
                Mathf.Max(0.0001f, propagationSpeed * inverseLength);
            int requiredSubstepCount = Mathf.Max(
                1,
                Mathf.CeilToInt(deltaTime / maximumStableStep));
            rippleSubstepLimitReached =
                requiredSubstepCount > MaximumRippleSubsteps;
            int substepCount = Mathf.Min(
                requiredSubstepCount,
                MaximumRippleSubsteps);
            RecordRippleSubstepDiagnostics(substepCount);
            float substepDelta = deltaTime / substepCount;
            float dampingPerSecond = river.ResolvedImpactRippleDecay;
            float centrelineCellSize = Mathf.Max(
                0.001f,
                fieldLength / Mathf.Max(1, fieldWidth - 1));

            for (int substep = 0; substep < substepCount; substep++)
            {
                float advectionPixels =
                    Mathf.Abs(river.FlowSpeedMetresPerSecond) *
                    substepDelta /
                    centrelineCellSize;

                computeShader.SetInts("_FieldSize", fieldWidth, fieldHeight);
                computeShader.SetFloat("_DeltaTime", substepDelta);
                computeShader.SetFloat("_PropagationSpeed", propagationSpeed);
                computeShader.SetFloat("_DampingPerSecond", dampingPerSecond);
                computeShader.SetFloat(
                    "_AdvectionPixels",
                    advectionPixels);
                computeShader.SetInt("_RippleMetricCount", fieldWidth);
                computeShader.SetFloat(
                    "_MaximumHeight",
                    river.ResolvedImpactRippleMaximumHeight);
                computeShader.SetBuffer(
                    simulateRippleKernel,
                    "_RippleMetricData",
                    rippleMetricBuffer);
                computeShader.SetTexture(
                    simulateRippleKernel,
                    "_RippleBoundaryRead",
                    rippleBoundary);
                computeShader.SetTexture(
                    simulateRippleKernel,
                    "_StateRead",
                    currentState);
                computeShader.SetTexture(
                    simulateRippleKernel,
                    "_StateWrite",
                    writeState);

                DispatchRippleActiveRanges();

                RenderTexture oldCurrent = currentState;
                currentState = writeState;
                previousState = oldCurrent;
                writeState = oldCurrent;
            }
        }

        private void DispatchRippleActiveRanges()
        {
            int groupStart = -1;
            for (int chunk = 0; chunk <= chunkCount; chunk++)
            {
                bool active = chunk < chunkCount && chunkActive[chunk];
                if (active && groupStart < 0)
                {
                    groupStart = chunk;
                }

                if (active || groupStart < 0)
                {
                    continue;
                }

                int groupCount = chunk - groupStart;
                int xOffset = groupStart * resolutionPerChunk;
                int width = groupCount * resolutionPerChunk;
                computeShader.SetInt("_DispatchXOffset", xOffset);
                computeShader.SetInt("_DispatchWidth", width);
                DispatchCompute(
                    simulateRippleKernel,
                    Mathf.CeilToInt(width / (float)ThreadGroupSize),
                    Mathf.CeilToInt(fieldHeight / (float)ThreadGroupSize),
                    1,
                    PerformanceDispatchCategory.RippleSimulation,
                    width,
                    fieldHeight);
                groupStart = -1;
            }
        }

        private void SimulateWakeField(float deltaTime, double now)
        {
            if (!HasWakeActiveChunks())
            {
                return;
            }

            float cellSizeX = fieldLength / Mathf.Max(1, wakeFieldWidth);
            float cellSizeY =
                averageSurfaceHalfWidth * 2f /
                Mathf.Max(1, wakeFieldHeight - 1);
            float advectionPixels =
                Mathf.Abs(river.FlowSpeedMetresPerSecond) *
                deltaTime /
                Mathf.Max(0.001f, cellSizeX);
            const float decayPerSecond = 1.15f;
            float lateralSpread = river.WakeWidening;
            float flowFactor = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.InverseLerp(
                    0.05f,
                    1.25f,
                    Mathf.Abs(river.FlowSpeedMetresPerSecond)));

            computeShader.SetInts(
                "_WakeFieldSize",
                wakeFieldWidth,
                wakeFieldHeight);
            computeShader.SetFloat("_WakeDeltaTime", deltaTime);
            computeShader.SetFloat("_WakeAdvectionPixels", advectionPixels);
            computeShader.SetFloat("_WakeCellSizeX", cellSizeX);
            computeShader.SetFloat("_WakeCellSizeY", cellSizeY);
            computeShader.SetFloat("_WakeLateralSpread", lateralSpread);
            computeShader.SetFloat("_WakeDecayPerSecond", decayPerSecond);
            computeShader.SetFloat("_WakeSourceRate", 1.45f);
            computeShader.SetFloat("_WakeFlowFactor", flowFactor);
            computeShader.SetFloat("_WakeTime", river.MotionTime);
            computeShader.SetFloat("_WakeGradientStrength", 0.32f);
            computeShader.SetTexture(
                simulateWakeKernel,
                "_WakeRead",
                currentWake);
            computeShader.SetTexture(
                simulateWakeKernel,
                "_WakeWrite",
                writeWake);
            computeShader.SetTexture(
                simulateWakeKernel,
                "_StaticWakeSourceRead",
                staticWakeSource);

            DispatchWakeActiveRanges();

            RenderTexture oldCurrent = currentWake;
            currentWake = writeWake;
            previousWake = oldCurrent;
            writeWake = oldCurrent;
        }

        private void DispatchWakeActiveRanges()
        {
            int groupStart = -1;
            for (int chunk = 0; chunk <= chunkCount; chunk++)
            {
                bool active = chunk < chunkCount && wakeChunkActive[chunk];
                if (active && groupStart < 0)
                {
                    groupStart = chunk;
                }

                if (active || groupStart < 0)
                {
                    continue;
                }

                int groupCount = chunk - groupStart;
                int xOffset = groupStart * wakeResolutionPerChunk;
                int width = groupCount * wakeResolutionPerChunk;
                computeShader.SetInt("_WakeDispatchXOffset", xOffset);
                computeShader.SetInt("_WakeDispatchWidth", width);
                DispatchCompute(
                    simulateWakeKernel,
                    Mathf.CeilToInt(width / (float)ThreadGroupSize),
                    Mathf.CeilToInt(wakeFieldHeight / (float)ThreadGroupSize),
                    1,
                    PerformanceDispatchCategory.WakeSimulation,
                    width,
                    wakeFieldHeight);
                groupStart = -1;
            }
        }

        private void DispatchRippleInjection(ImpactCommand impact)
        {
            float centreX = GlobalDistanceToPixel(impact.Distance);
            float centreY = AcrossToPixel(impact.AcrossNormalized);
            ResolveRippleInjectionRadiusPixels(
                centreX,
                impact.Radius * RippleInjectionEnvelopeRadius,
                out float radiusX,
                out float radiusY);
            int minX = Mathf.Clamp(
                Mathf.FloorToInt(centreX - radiusX - 2f),
                0,
                fieldWidth - 1);
            int maxX = Mathf.Clamp(
                Mathf.CeilToInt(centreX + radiusX + 2f),
                0,
                fieldWidth - 1);
            int minY = Mathf.Clamp(
                Mathf.FloorToInt(centreY - radiusY - 2f),
                0,
                fieldHeight - 1);
            int maxY = Mathf.Clamp(
                Mathf.CeilToInt(centreY + radiusY + 2f),
                0,
                fieldHeight - 1);
            int width = Mathf.Max(1, maxX - minX + 1);
            int height = Mathf.Max(1, maxY - minY + 1);
            float resolvedStrength =
                river.ResolvedImpactRippleStrength;
            float signedImpulse =
                impact.SignedImpulse * resolvedStrength;

            computeShader.SetInts("_FieldSize", fieldWidth, fieldHeight);
            computeShader.SetInts(
                "_RippleInjectRect",
                minX,
                minY,
                width,
                height);
            computeShader.SetVector(
                "_RippleInjectWorldPosition",
                new Vector4(
                    impact.WorldPositionXZ.x,
                    impact.WorldPositionXZ.y,
                    0f,
                    0f));
            computeShader.SetFloat(
                "_RippleInjectRadiusMetres",
                impact.Radius);
            computeShader.SetFloat(
                "_RippleInjectHeight",
                signedImpulse *
                Mathf.Clamp01(impact.GeometryContribution) *
                0.028f);
            computeShader.SetFloat(
                "_RippleInjectElevation",
                impact.InitialElevation *
                resolvedStrength *
                Mathf.Clamp01(impact.GeometryContribution));
            computeShader.SetFloat(
                "_RippleInjectVelocity",
                signedImpulse *
                Mathf.Clamp01(impact.GeometryContribution) *
                0.68f);
            computeShader.SetFloat(
                "_RippleInjectNormalDetail",
                signedImpulse *
                Mathf.Clamp01(impact.NormalContribution) *
                0.12f);
            computeShader.SetFloat(
                "_RippleInjectShape",
                Mathf.Clamp01(impact.Shape));
            computeShader.SetFloat(
                "_RippleInjectSharpness",
                Mathf.Clamp(
                    impact.Sharpness,
                    ImpactRippleEventSettings.MinimumSharpness,
                    ImpactRippleEventSettings.MaximumSharpness));
            computeShader.SetFloat(
                "_RippleInjectRidgeEmphasis",
                river.ImpactRippleRidgeEmphasis);
            computeShader.SetFloat(
                "_MaximumHeight",
                river.ResolvedImpactRippleMaximumHeight);
            computeShader.SetInt("_RippleMetricCount", fieldWidth);
            computeShader.SetBuffer(
                injectRippleKernel,
                "_RippleMetricData",
                rippleMetricBuffer);
            computeShader.SetTexture(
                injectRippleKernel,
                "_RippleBoundaryRead",
                rippleBoundary);
            computeShader.SetTexture(
                injectRippleKernel,
                "_StateWrite",
                currentState);
            DispatchCompute(
                injectRippleKernel,
                Mathf.CeilToInt(width / (float)ThreadGroupSize),
                Mathf.CeilToInt(height / (float)ThreadGroupSize),
                1,
                PerformanceDispatchCategory.ImpactInjection,
                width,
                height);
        }

        private void DispatchWakeInjection(
            ContinuousSource source,
            float surfaceHalfWidth,
            float wakeStrength,
            float movementBlend,
            float simulationDeltaTime)
        {
            float startX = WakeGlobalDistanceToPixel(source.StartDistance);
            float endX = WakeGlobalDistanceToPixel(source.EndDistance);
            float startY = WakeAcrossToPixel(source.StartAcrossNormalized);
            float endY = WakeAcrossToPixel(source.EndAcrossNormalized);
            float cellSizeX = fieldLength / Mathf.Max(1, wakeFieldWidth);
            float cellSizeY =
                surfaceHalfWidth * 2f / Mathf.Max(1, wakeFieldHeight);
            float alongPixels =
                source.AlongHalfLength / Mathf.Max(0.001f, cellSizeX);
            float acrossPixels =
                source.AcrossHalfWidth * river.WakeSpread /
                Mathf.Max(0.001f, cellSizeY);
            int minX = Mathf.Clamp(
                Mathf.FloorToInt(
                    Mathf.Min(startX, endX) - alongPixels * 1.25f - 2f),
                0,
                wakeFieldWidth - 1);
            int maxX = Mathf.Clamp(
                Mathf.CeilToInt(
                    Mathf.Max(startX, endX) + alongPixels * 2.0f + 3f),
                0,
                wakeFieldWidth - 1);
            int minY = Mathf.Clamp(
                Mathf.FloorToInt(
                    Mathf.Min(startY, endY) - acrossPixels * 1.40f - 2f),
                0,
                wakeFieldHeight - 1);
            int maxY = Mathf.Clamp(
                Mathf.CeilToInt(
                    Mathf.Max(startY, endY) + acrossPixels * 1.40f + 2f),
                0,
                wakeFieldHeight - 1);
            int width = Mathf.Max(1, maxX - minX + 1);
            int height = Mathf.Max(1, maxY - minY + 1);

            computeShader.SetInts(
                "_WakeFieldSize",
                wakeFieldWidth,
                wakeFieldHeight);
            computeShader.SetInts(
                "_WakeInjectRect",
                minX,
                minY,
                width,
                height);
            computeShader.SetVector(
                "_WakeInjectStart",
                new Vector4(startX, startY, 0f, 0f));
            computeShader.SetVector(
                "_WakeInjectEnd",
                new Vector4(endX, endY, 0f, 0f));
            computeShader.SetVector(
                "_WakeInjectFootprintPixels",
                new Vector4(
                    Mathf.Max(1f, alongPixels),
                    Mathf.Max(1f, acrossPixels),
                    0f,
                    0f));
            computeShader.SetFloat(
                "_WakeInjectStrength",
                Mathf.Max(0f, wakeStrength));
            computeShader.SetFloat(
                "_WakeInjectMovementBlend",
                Mathf.Clamp01(movementBlend));
            computeShader.SetFloat(
                "_WakeInjectPersistence",
                river.WakeReach);
            computeShader.SetFloat(
                "_WakeInjectDeltaTime",
                Mathf.Max(0.0001f, simulationDeltaTime));
            computeShader.SetTexture(
                injectWakeKernel,
                "_WakeWrite",
                currentWake);
            DispatchCompute(
                injectWakeKernel,
                Mathf.CeilToInt(width / (float)ThreadGroupSize),
                Mathf.CeilToInt(height / (float)ThreadGroupSize),
                1,
                PerformanceDispatchCategory.WakeInjection,
                width,
                height);
        }

        private void RebuildRippleBoundary(double now)
        {
            if (rippleBoundary == null ||
                computeShader == null ||
                rippleMetricBuffer == null)
            {
                return;
            }

            RecordFieldRebuild();
            computeShader.SetInts("_FieldSize", fieldWidth, fieldHeight);
            computeShader.SetInt("_RippleMetricCount", fieldWidth);
            computeShader.SetFloat(
                "_RippleShoreReflection",
                river.ImpactRippleShoreReflection);
            computeShader.SetBuffer(
                bakeRippleBoundaryBaseKernel,
                "_RippleMetricData",
                rippleMetricBuffer);
            computeShader.SetTexture(
                bakeRippleBoundaryBaseKernel,
                "_RippleBoundaryWrite",
                rippleBoundary);
            DispatchCompute(
                bakeRippleBoundaryBaseKernel,
                Mathf.CeilToInt(fieldWidth / (float)ThreadGroupSize),
                Mathf.CeilToInt(fieldHeight / (float)ThreadGroupSize),
                1,
                PerformanceDispatchCategory.RippleBoundaryBake,
                fieldWidth,
                fieldHeight);

            rippleCollisionSourceCount = 0;
            foreach (KeyValuePair<EntityId, ContinuousSource> pair in
                     continuousSources)
            {
                ContinuousSource source = pair.Value;
                if (!source.IsStatic ||
                    !source.RippleCollisionEnabled)
                {
                    continue;
                }

                if (DispatchRippleBoundaryObstacle(source))
                {
                    rippleCollisionSourceCount++;
                }
            }

            ApplyRippleBoundaryToState(stateA);
            ApplyRippleBoundaryToState(stateB);
            rippleBoundaryDirty = false;
            lastActivityTime = now;
        }

        private bool DispatchRippleBoundaryObstacle(
            ContinuousSource source)
        {
            if (!river.TryProjectWorldPoint(
                    source.WorldPosition,
                    out StylizedRiverProjection projection) ||
                !projection.IsInside)
            {
                return false;
            }

            float centreX = GlobalDistanceToPixel(source.StartDistance);
            float centreY = AcrossToPixel(source.StartAcrossNormalized);
            float edgeWidth = Mathf.Max(
                0.025f,
                ResolveRippleBoundaryEdgeWidth(centreX));
            float envelopeRadius = Mathf.Max(
                source.RippleCollisionAlongHalfLength,
                source.RippleCollisionAcrossHalfWidth) +
                edgeWidth * 3f;
            ResolveRippleInjectionRadiusPixels(
                centreX,
                envelopeRadius,
                out float radiusX,
                out float radiusY);

            int minX = Mathf.Clamp(
                Mathf.FloorToInt(centreX - radiusX - 2f),
                0,
                fieldWidth - 1);
            int maxX = Mathf.Clamp(
                Mathf.CeilToInt(centreX + radiusX + 2f),
                0,
                fieldWidth - 1);
            int minY = Mathf.Clamp(
                Mathf.FloorToInt(centreY - radiusY - 2f),
                0,
                fieldHeight - 1);
            int maxY = Mathf.Clamp(
                Mathf.CeilToInt(centreY + radiusY + 2f),
                0,
                fieldHeight - 1);
            int width = Mathf.Max(1, maxX - minX + 1);
            int height = Mathf.Max(1, maxY - minY + 1);

            StylizedRiverSplineSample sample =
                river.Domain.SampleAtGlobalDistance(source.StartDistance);
            Vector3 downstream3 = sample.Tangent * river.FlowDirection;
            downstream3.y = 0f;
            downstream3 = downstream3.sqrMagnitude > 0.0001f
                ? downstream3.normalized
                : Vector3.forward;
            Vector3 across3 = sample.Side;
            across3.y = 0f;
            across3 = across3.sqrMagnitude > 0.0001f
                ? across3.normalized
                : Vector3.Cross(Vector3.up, downstream3).normalized;

            int contourCount = Mathf.Min(
                source.RippleCollisionContour != null
                    ? source.RippleCollisionContour.Length
                    : 0,
                MaximumStaticContourPoints);
            for (int index = 0;
                 index < MaximumStaticContourPoints;
                 index++)
            {
                if (index < contourCount)
                {
                    Vector2 point = source.RippleCollisionContour[index];
                    staticContourUpload[index] = new Vector4(
                        point.x,
                        point.y,
                        0f,
                        0f);
                }
                else
                {
                    staticContourUpload[index] = Vector4.zero;
                }
            }

            computeShader.SetInts("_FieldSize", fieldWidth, fieldHeight);
            computeShader.SetInt("_RippleMetricCount", fieldWidth);
            computeShader.SetInts(
                "_RippleObstacleRect",
                minX,
                minY,
                width,
                height);
            computeShader.SetVector(
                "_RippleObstacleWorldCentre",
                new Vector4(
                    source.WorldPosition.x,
                    source.WorldPosition.z,
                    0f,
                    0f));
            computeShader.SetVector(
                "_RippleObstacleDownstream",
                new Vector4(
                    downstream3.x,
                    downstream3.z,
                    0f,
                    0f));
            computeShader.SetVector(
                "_RippleObstacleAcross",
                new Vector4(
                    across3.x,
                    across3.z,
                    0f,
                    0f));
            computeShader.SetVector(
                "_RippleObstacleHalfSizeMetres",
                new Vector4(
                    source.RippleCollisionAlongHalfLength,
                    source.RippleCollisionAcrossHalfWidth,
                    0f,
                    0f));
            computeShader.SetFloat(
                "_RippleObstacleEdgeWidthMetres",
                edgeWidth);
            computeShader.SetFloat(
                "_RippleObstacleReflection",
                river.ImpactRippleObstacleReflection);
            computeShader.SetInt("_StaticContourCount", contourCount);
            computeShader.SetVectorArray(
                "_StaticContour",
                staticContourUpload);
            computeShader.SetBuffer(
                bakeRippleBoundaryObstacleKernel,
                "_RippleMetricData",
                rippleMetricBuffer);
            computeShader.SetTexture(
                bakeRippleBoundaryObstacleKernel,
                "_RippleBoundaryWrite",
                rippleBoundary);
            DispatchCompute(
                bakeRippleBoundaryObstacleKernel,
                Mathf.CeilToInt(width / (float)ThreadGroupSize),
                Mathf.CeilToInt(height / (float)ThreadGroupSize),
                1,
                PerformanceDispatchCategory.RippleBoundaryBake,
                width,
                height);
            return true;
        }

        private float ResolveRippleBoundaryEdgeWidth(float centreX)
        {
            int row = Mathf.Clamp(
                Mathf.RoundToInt(centreX),
                0,
                Mathf.Max(0, fieldWidth - 1));
            float along =
                row < rippleMetricMinimumAlongCell.Length
                    ? rippleMetricMinimumAlongCell[row]
                    : fieldLength / Mathf.Max(1, fieldWidth - 1);
            float lateral =
                row < rippleMetricMinimumLateralCell.Length
                    ? rippleMetricMinimumLateralCell[row]
                    : along;
            return Mathf.Min(
                Mathf.Max(0.001f, along),
                Mathf.Max(0.001f, lateral)) * 0.50f;
        }

        private void ApplyRippleBoundaryToState(RenderTexture state)
        {
            if (state == null)
            {
                return;
            }

            computeShader.SetInts("_FieldSize", fieldWidth, fieldHeight);
            computeShader.SetTexture(
                applyRippleBoundaryKernel,
                "_RippleBoundaryRead",
                rippleBoundary);
            computeShader.SetTexture(
                applyRippleBoundaryKernel,
                "_StateWrite",
                state);
            DispatchCompute(
                applyRippleBoundaryKernel,
                Mathf.CeilToInt(fieldWidth / (float)ThreadGroupSize),
                Mathf.CeilToInt(fieldHeight / (float)ThreadGroupSize),
                1,
                PerformanceDispatchCategory.RippleBoundaryBake,
                fieldWidth,
                fieldHeight);
        }

        private void RebuildStaticPressureTarget(double now)
        {
            if (staticTarget == null || computeShader == null)
            {
                return;
            }

            RecordFieldRebuild();
            DispatchClear(
                staticTarget,
                fieldWidth,
                fieldHeight,
                0,
                fieldWidth);

            validStaticSourceCount = 0;

            foreach (KeyValuePair<EntityId, ContinuousSource> pair in
                     continuousSources)
            {
                ContinuousSource source = pair.Value;
                if (!source.IsStatic ||
                    source.StaticTargetHeightMetres <= 0.0001f ||
                    !river.TryProjectWorldPoint(
                        source.WorldPosition,
                        out StylizedRiverProjection projection) ||
                    !projection.IsInside)
                {
                    continue;
                }

                StylizedRiverSplineSample sample =
                    river.SampleAtLocalDistance(projection.LocalDistance);
                float surfaceHalfWidth = Mathf.Max(
                    0.05f,
                    sample.GetSurfaceHalfWidth(projection.AcrossMetres));
                float acrossNormalized = Mathf.Clamp(
                    projection.AcrossMetres / surfaceHalfWidth,
                    -1f,
                    1f);

                DispatchStaticPressureBake(
                    projection.GlobalDistance,
                    acrossNormalized,
                    surfaceHalfWidth,
                    source.StaticPressureAcrossHalfWidth,
                    source.StaticPressureAlongHalfLength,
                    source.StaticTargetHeightMetres,
                    source.StaticContactSharpness,
                    source.StaticPressureProfile.IsValid
                        ? 0f
                        : source.StaticProfileVariation,
                    source.Phase,
                    source.StaticPressureContour,
                    source.StaticPressureProfile);
                validStaticSourceCount++;
            }

            if (validStaticSourceCount > 0)
            {
                computeShader.SetInts(
                    "_FieldSize",
                    fieldWidth,
                    fieldHeight);
                computeShader.SetVector(
                    "_StaticCellSize",
                    new Vector4(
                        fieldLength / Mathf.Max(1, fieldWidth),
                        averageSurfaceHalfWidth * 2f /
                        Mathf.Max(1, fieldHeight),
                        0f,
                        0f));
                computeShader.SetTexture(
                    finalizeStaticPressureKernel,
                    "_StaticPressureWrite",
                    staticTarget);
                DispatchCompute(
                    finalizeStaticPressureKernel,
                    Mathf.CeilToInt(fieldWidth / (float)ThreadGroupSize),
                    Mathf.CeilToInt(fieldHeight / (float)ThreadGroupSize),
                    1,
                    PerformanceDispatchCategory.StaticPressureBake,
                    fieldWidth,
                    fieldHeight);
            }

            staticPressureTargetDirty = false;
            lastActivityTime = now;
        }

        private void RebuildStaticWakeSource(double now)
        {
            if (staticWakeSource == null || computeShader == null)
            {
                return;
            }

            RecordFieldRebuild();
            DispatchClear(
                staticWakeSource,
                wakeFieldWidth,
                wakeFieldHeight,
                0,
                wakeFieldWidth);
            ReleaseStaticWakeChunkReservations(now);

            float absoluteFlowSpeed =
                Mathf.Abs(river.FlowSpeedMetresPerSecond);
            validStaticWakeSourceCount = 0;

            foreach (KeyValuePair<EntityId, ContinuousSource> pair in
                     continuousSources)
            {
                ContinuousSource source = pair.Value;
                if (!source.IsStatic ||
                    source.StaticWakeAmplitude <= 0.0001f ||
                    !river.TryProjectWorldPoint(
                        source.WorldPosition,
                        out StylizedRiverProjection projection) ||
                    !projection.IsInside)
                {
                    continue;
                }

                StylizedRiverSplineSample sample =
                    river.SampleAtLocalDistance(projection.LocalDistance);
                float surfaceHalfWidth = Mathf.Max(
                    0.05f,
                    sample.GetSurfaceHalfWidth(projection.AcrossMetres));
                float acrossNormalized = Mathf.Clamp(
                    projection.AcrossMetres / surfaceHalfWidth,
                    -1f,
                    1f);
                float wakeLength = ResolveObstructionWakeLength(
                    source.AcrossHalfWidth,
                    source.AlongHalfLength,
                    absoluteFlowSpeed) *
                    source.StaticWakeReachMultiplier;
                double releaseDurationSeconds =
                    ResolveStaticWakeReleaseDuration(
                        wakeLength,
                        source.StaticWakeReachMultiplier,
                        absoluteFlowSpeed);

                DispatchStaticWakeSourceBake(
                    projection.GlobalDistance,
                    acrossNormalized,
                    surfaceHalfWidth,
                    source);

                MarkStaticWakeRange(
                    projection.GlobalDistance,
                    source.AlongHalfLength,
                    wakeLength,
                    releaseDurationSeconds);
                validStaticWakeSourceCount++;
            }

            staticWakeSourceDirty = false;
            lastActivityTime = now;
        }

        private void DispatchStaticPressureBake(
            float globalDistance,
            float acrossNormalized,
            float surfaceHalfWidth,
            float acrossHalfWidth,
            float alongHalfLength,
            float targetHeightMetres,
            float responseStiffness,
            float unsteadiness,
            float phase,
            Vector2[] contour,
            RiverDisturbancePressureBakeProfile pressureProfile)
        {
            DispatchStaticBakeCommon(
                globalDistance,
                acrossNormalized,
                surfaceHalfWidth,
                acrossHalfWidth,
                alongHalfLength,
                contour,
                fieldWidth,
                fieldHeight,
                targetHeightMetres,
                0f,
                1f,
                1f,
                responseStiffness,
                unsteadiness,
                default,
                phase,
                bakeStaticPressureKernel,
                staticTarget,
                true,
                pressureProfile);
        }

        private void DispatchStaticWakeSourceBake(
            float globalDistance,
            float acrossNormalized,
            float surfaceHalfWidth,
            ContinuousSource source)
        {
            DispatchStaticBakeCommon(
                globalDistance,
                acrossNormalized,
                surfaceHalfWidth,
                source.AcrossHalfWidth,
                source.AlongHalfLength,
                source.StaticContour,
                wakeFieldWidth,
                wakeFieldHeight,
                0f,
                source.StaticWakeAmplitude,
                source.StaticWakeReachMultiplier,
                source.StaticWakeSpreadMultiplier,
                1f,
                0f,
                new StaticWakeBakeVariationParameters(
                    source.StaticWakeLeeVariation,
                    source.StaticWakeLeftReleaseVariation,
                    source.StaticWakeRightReleaseVariation),
                source.Phase,
                bakeStaticWakeSourceKernel,
                staticWakeSource,
                false,
                default);
        }

        private void DispatchStaticBakeCommon(
            float globalDistance,
            float acrossNormalized,
            float surfaceHalfWidth,
            float acrossHalfWidth,
            float alongHalfLength,
            Vector2[] contour,
            int targetWidth,
            int targetHeight,
            float targetHeightMetres,
            float wakeAmplitude,
            float wakePersistence,
            float wakeSpread,
            float responseStiffness,
            float unsteadiness,
            StaticWakeBakeVariationParameters wakeVariation,
            float phase,
            int kernel,
            RenderTexture targetTexture,
            bool pressurePass,
            RiverDisturbancePressureBakeProfile pressureProfile)
        {
            float centreX = FieldGlobalDistanceToPixel(
                globalDistance,
                targetWidth);
            float centreY = FieldAcrossToPixel(
                acrossNormalized,
                targetHeight);
            float cellSizeX = fieldLength / Mathf.Max(1, targetWidth);
            float cellSizeY =
                surfaceHalfWidth * 2f / Mathf.Max(1, targetHeight);
            float alongPixels =
                alongHalfLength / Mathf.Max(0.001f, cellSizeX);
            float acrossPixels =
                acrossHalfWidth / Mathf.Max(0.001f, cellSizeY);
            float pressureDepthMetres = pressurePass
                ? Mathf.Clamp(
                    Mathf.Max(
                        0.22f,
                        alongHalfLength * 2f * 0.08f,
                        cellSizeX * 1.15f,
                        river.ResolvedSurfaceLongitudinalSpacing * 1.50f),
                    0.22f,
                    0.48f)
                : 0f;
            float pressureInsideOverlapMetres = pressurePass
                ? Mathf.Clamp(
                    Mathf.Max(0.08f, cellSizeX * 0.35f),
                    0.08f,
                    0.16f)
                : 0f;
            float pressureDepthPixels = pressurePass
                ? pressureDepthMetres / Mathf.Max(0.001f, cellSizeX)
                : 0f;
            float pressureInsideOverlapPixels = pressurePass
                ? pressureInsideOverlapMetres / Mathf.Max(0.001f, cellSizeX)
                : 0f;
            float longitudinalExpansion = pressurePass ? 1f : 1.75f;
            float lateralExpansion = pressurePass
                ? 1.20f
                : 1.55f * Mathf.Clamp(wakeSpread, 0.5f, 2f);

            int minX = Mathf.Clamp(
                Mathf.FloorToInt(
                    pressurePass
                        ? centreX - alongPixels - pressureDepthPixels - 3f
                        : centreX - alongPixels * longitudinalExpansion - 4f),
                0,
                targetWidth - 1);
            int maxX = Mathf.Clamp(
                Mathf.CeilToInt(
                    pressurePass
                        ? centreX + alongPixels + 3f
                        : centreX + alongPixels * longitudinalExpansion + 5f),
                0,
                targetWidth - 1);
            int minY = Mathf.Clamp(
                Mathf.FloorToInt(
                    centreY - acrossPixels * lateralExpansion - 4f),
                0,
                targetHeight - 1);
            int maxY = Mathf.Clamp(
                Mathf.CeilToInt(
                    centreY + acrossPixels * lateralExpansion + 4f),
                0,
                targetHeight - 1);
            int width = Mathf.Max(1, maxX - minX + 1);
            int height = Mathf.Max(1, maxY - minY + 1);
            int contourCount = Mathf.Min(
                contour != null ? contour.Length : 0,
                MaximumStaticContourPoints);
            for (int index = 0; index < MaximumStaticContourPoints; index++)
            {
                if (index < contourCount)
                {
                    Vector2 point = contour[index];
                    staticContourUpload[index] = new Vector4(
                        point.x / Mathf.Max(0.001f, cellSizeX),
                        point.y / Mathf.Max(0.001f, cellSizeY),
                        0f,
                        0f);
                }
                else
                {
                    staticContourUpload[index] = Vector4.zero;
                }
            }

            bool pressureGeometryValid =
                pressurePass &&
                pressureProfile.IsValid &&
                pressureProfile.HasGeometryBounds;
            for (int index = 0;
                 index < staticPressureProfileUpload.Length;
                 index++)
            {
                if (pressurePass &&
                    pressureProfile.IsValid &&
                    index < pressureProfile.Samples.Length)
                {
                    Vector4 sample = pressureProfile.Samples[index];
                    staticPressureProfileUpload[index] = new Vector4(
                        sample.x / Mathf.Max(0.001f, cellSizeX),
                        sample.y / Mathf.Max(0.001f, cellSizeX),
                        sample.z,
                        sample.w);
                    staticPressureGeometryUpload[index] =
                        pressureGeometryValid &&
                        index < pressureProfile.DownstreamBoundaries.Length
                            ? new Vector4(
                                pressureProfile.DownstreamBoundaries[index] /
                                Mathf.Max(0.001f, cellSizeX),
                                0f,
                                0f,
                                0f)
                            : Vector4.zero;
                }
                else
                {
                    staticPressureProfileUpload[index] = Vector4.zero;
                    staticPressureGeometryUpload[index] = Vector4.zero;
                }
            }

            StaticWakeLeeVariationState wakeLeeVariation =
                wakeVariation.Lee;
            int wakeVariationProfileCount =
                !pressurePass &&
                HasValidStaticWakeLeeVariationState(wakeLeeVariation)
                    ? wakeLeeVariation.SampleCount
                    : 0;
            for (int index = 0;
                 index < staticWakeVariationProfileUpload.Length;
                 index++)
            {
                staticWakeVariationProfileUpload[index] =
                    index < wakeVariationProfileCount
                        ? new Vector4(
                            wakeLeeVariation.
                                CurrentDepthMultipliers[index],
                            wakeLeeVariation.
                                CurrentLengthMultipliers[index],
                            wakeLeeVariation.
                                CurrentTrailingEdgeOffsets[index],
                            1f)
                        : new Vector4(1f, 1f, 0f, 0f);
            }

            computeShader.SetInts("_FieldSize", targetWidth, targetHeight);
            computeShader.SetInts(
                "_StaticRect",
                minX,
                minY,
                width,
                height);
            computeShader.SetVector(
                "_StaticCentre",
                new Vector4(centreX, centreY, 0f, 0f));
            computeShader.SetVector(
                "_StaticHalfSizePixels",
                new Vector4(
                    Mathf.Max(1f, alongPixels),
                    Mathf.Max(1f, acrossPixels),
                    0f,
                    0f));
            computeShader.SetVector(
                "_StaticCellSize",
                new Vector4(cellSizeX, cellSizeY, 0f, 0f));
            computeShader.SetInt("_StaticContourCount", contourCount);
            computeShader.SetVectorArray(
                "_StaticContour",
                staticContourUpload);
            computeShader.SetVectorArray(
                "_StaticPressureProfile",
                staticPressureProfileUpload);
            computeShader.SetVectorArray(
                "_StaticPressureGeometry",
                staticPressureGeometryUpload);
            computeShader.SetVectorArray(
                "_StaticWakeVariationProfile",
                staticWakeVariationProfileUpload);
            computeShader.SetInt(
                "_StaticWakeVariationProfileCount",
                wakeVariationProfileCount);
            computeShader.SetFloat(
                "_StaticWakeVariationProfileHalfWidthPixels",
                acrossPixels);
            computeShader.SetInt(
                "_StaticPressureGeometryValid",
                pressureGeometryValid ? 1 : 0);
            computeShader.SetInt(
                "_StaticPressureProfileCount",
                pressurePass && pressureProfile.IsValid
                    ? pressureProfile.LateralSampleCount
                    : 0);
            computeShader.SetFloat(
                "_StaticPressureProfileHalfWidthPixels",
                pressurePass && pressureProfile.IsValid
                    ? pressureProfile.AcrossHalfWidth /
                      Mathf.Max(0.001f, cellSizeY)
                    : acrossPixels);
            computeShader.SetInt(
                "_StaticPressureProfileValid",
                pressurePass && pressureProfile.IsValid ? 1 : 0);
            computeShader.SetFloat(
                "_StaticTargetHeight",
                Mathf.Clamp(
                    targetHeightMetres,
                    0f,
                    MaximumStaticPressureHeightMetres));
            computeShader.SetFloat(
                "_StaticPressureDepthPixels",
                pressureDepthPixels);
            computeShader.SetFloat(
                "_StaticPressureInsideOverlapPixels",
                pressureInsideOverlapPixels);
            computeShader.SetFloat(
                "_StaticMaximumHeight",
                MaximumStaticPressureHeightMetres);
            computeShader.SetFloat(
                "_StaticWakeSourceStrength",
                Mathf.Clamp(wakeAmplitude, 0f, 4f));
            computeShader.SetFloat(
                "_StaticWakePersistence",
                Mathf.Clamp(wakePersistence, 0.25f, 3f));
            computeShader.SetFloat(
                "_StaticWakeSpread",
                Mathf.Clamp(wakeSpread, 0.5f, 2f));
            StaticWakeReleaseVariationState leftRelease =
                wakeVariation.Left;
            StaticWakeReleaseVariationState rightRelease =
                wakeVariation.Right;
            computeShader.SetVector(
                "_StaticWakeLeftReleaseVariation",
                new Vector4(
                    leftRelease.CurrentLateralOffset,
                    leftRelease.CurrentEnergyMultiplier,
                    leftRelease.CurrentWidthMultiplier,
                    leftRelease.CurrentDownstreamOffset));
            computeShader.SetVector(
                "_StaticWakeRightReleaseVariation",
                new Vector4(
                    rightRelease.CurrentLateralOffset,
                    rightRelease.CurrentEnergyMultiplier,
                    rightRelease.CurrentWidthMultiplier,
                    rightRelease.CurrentDownstreamOffset));
            computeShader.SetFloat(
                "_StaticPhase",
                Mathf.Repeat(phase, 1f));
            computeShader.SetFloat(
                "_StaticContactSharpness",
                Mathf.Clamp(responseStiffness, 0.5f, 4f));
            computeShader.SetFloat(
                "_StaticWaveResponse",
                Mathf.Clamp(unsteadiness, 0f, 2f));
            computeShader.SetFloat(
                "_MaximumHeight",
                river.ResolvedImpactRippleMaximumHeight);
            computeShader.SetTexture(
                kernel,
                pressurePass
                    ? "_StaticPressureWrite"
                    : "_StaticWakeSourceWrite",
                targetTexture);
            DispatchCompute(
                kernel,
                Mathf.CeilToInt(width / (float)ThreadGroupSize),
                Mathf.CeilToInt(height / (float)ThreadGroupSize),
                1,
                pressurePass
                    ? PerformanceDispatchCategory.StaticPressureBake
                    : PerformanceDispatchCategory.StaticWakeBake,
                width,
                height);
        }

        private void MarkStaticWakeRange(
            float globalDistance,
            float alongHalfLength,
            float wakeLength,
            double releaseDurationSeconds)
        {
            float sourceLocal =
                globalDistance - river.Domain.GlobalDistanceMinimum;
            float upstreamReach = alongHalfLength * 0.80f;
            // Keep one full downstream chunk active beyond the authored
            // reach so advection and lateral diffusion cannot terminate at
            // the source-range boundary.
            float downstreamReach = Mathf.Max(
                wakeLength,
                alongHalfLength * 1.20f) +
                ChunkLengthMetres;
            float minimumLocal = Mathf.Clamp(
                sourceLocal - upstreamReach,
                0f,
                validFieldLength);
            float maximumLocal = Mathf.Clamp(
                sourceLocal + downstreamReach,
                0f,
                validFieldLength);
            int minimumChunk = Mathf.Clamp(
                Mathf.FloorToInt(minimumLocal / ChunkLengthMetres),
                0,
                chunkCount - 1);
            int maximumChunk = Mathf.Clamp(
                Mathf.FloorToInt(maximumLocal / ChunkLengthMetres),
                0,
                chunkCount - 1);

            for (int chunk = minimumChunk; chunk <= maximumChunk; chunk++)
            {
                if (!wakeChunkActive[chunk])
                {
                    int xOffset = chunk * wakeResolutionPerChunk;
                    DispatchClear(
                        wakeA,
                        wakeFieldWidth,
                        wakeFieldHeight,
                        xOffset,
                        wakeResolutionPerChunk);
                    DispatchClear(
                        wakeB,
                        wakeFieldWidth,
                        wakeFieldHeight,
                        xOffset,
                        wakeResolutionPerChunk);
                    wakeChunkActive[chunk] = true;
                }

                chunkHasStaticSource[chunk] = true;
                staticWakeChunkReleaseDuration[chunk] = Math.Max(
                    staticWakeChunkReleaseDuration[chunk],
                    releaseDurationSeconds);
            }
        }

        private void ReleaseStaticWakeChunkReservations(double now)
        {
            for (int chunk = 0; chunk < chunkCount; chunk++)
            {
                if (!chunkHasStaticSource[chunk])
                {
                    staticWakeChunkReleaseDuration[chunk] = 0.0;
                    continue;
                }

                chunkHasStaticSource[chunk] = false;
                wakeChunkActive[chunk] = true;
                wakeChunkActiveUntil[chunk] = Math.Max(
                    wakeChunkActiveUntil[chunk],
                    now + Math.Max(
                        1.5,
                        staticWakeChunkReleaseDuration[chunk]));
                staticWakeChunkReleaseDuration[chunk] = 0.0;
            }
        }

        private void MarkWakeActive(
            float globalDistance,
            float radius,
            double now)
        {
            float localDistance = Mathf.Clamp(
                globalDistance - river.Domain.GlobalDistanceMinimum,
                0f,
                validFieldLength);
            int centreChunk = Mathf.Clamp(
                Mathf.FloorToInt(localDistance / ChunkLengthMetres),
                0,
                chunkCount - 1);
            int radiusChunks = Mathf.CeilToInt(
                radius / ChunkLengthMetres) + 1;
            double activeDuration = Mathf.Lerp(
                2.0f,
                10.0f,
                Mathf.InverseLerp(0.25f, 3f, river.WakeReach));

            for (int chunk = centreChunk - radiusChunks;
                 chunk <= centreChunk + radiusChunks;
                 chunk++)
            {
                if (chunk < 0 || chunk >= chunkCount)
                {
                    continue;
                }

                if (!wakeChunkActive[chunk])
                {
                    int xOffset = chunk * wakeResolutionPerChunk;
                    DispatchClear(
                        wakeA,
                        wakeFieldWidth,
                        wakeFieldHeight,
                        xOffset,
                        wakeResolutionPerChunk);
                    DispatchClear(
                        wakeB,
                        wakeFieldWidth,
                        wakeFieldHeight,
                        xOffset,
                        wakeResolutionPerChunk);
                    wakeChunkActive[chunk] = true;
                }

                wakeChunkActiveUntil[chunk] = Math.Max(
                    wakeChunkActiveUntil[chunk],
                    now + activeDuration);
            }

            lastActivityTime = now;
        }

        private static float ResolveObstructionWakeLength(
            float acrossHalfWidth,
            float alongHalfLength,
            float absoluteFlowSpeed)
        {
            float footprintScale = Mathf.Max(
                acrossHalfWidth * 1.20f,
                alongHalfLength * 1.40f);
            return footprintScale *
                   (1f + Mathf.Min(3f, absoluteFlowSpeed) * 0.12f);
        }

        private static double ResolveStaticWakeReleaseDuration(
            float wakeLength,
            float wakeReachMultiplier,
            float absoluteFlowSpeed)
        {
            // Mirror the current persistent-wake decay envelope while
            // also retaining enough time to transport the resolved source
            // reach at the current flow speed.
            float persistence = Mathf.Clamp(
                wakeReachMultiplier,
                0.25f,
                3f) / 3f;
            float persistenceScale = Mathf.Lerp(
                0.72f,
                1.65f,
                Mathf.Clamp01(persistence));
            float decayTailSeconds =
                Mathf.Log(100f) * persistenceScale / 1.15f;
            float transportSeconds =
                Mathf.Max(0f, wakeLength) /
                Mathf.Max(0.25f, absoluteFlowSpeed);

            return Mathf.Clamp(
                Mathf.Max(decayTailSeconds, transportSeconds),
                1.5f,
                12f);
        }

        private static Vector2[] CopyStaticContour(
            IReadOnlyList<Vector2> contour)
        {
            if (contour == null || contour.Count < 3)
            {
                return Array.Empty<Vector2>();
            }

            int count = Mathf.Min(
                contour.Count,
                MaximumStaticContourPoints);
            Vector2[] result = new Vector2[count];
            for (int index = 0; index < count; index++)
            {
                result[index] = contour[index];
            }

            return result;
        }

        private void UpdateStaticWakeVariations(
            float deltaTime,
            double now)
        {
            if (river == null || deltaTime <= 0f)
            {
                return;
            }

            staticWakeVariationAccumulator += deltaTime;
            float updateInterval =
                1f / Mathf.Max(1f, StaticWakeVariationUpdateRate);
            if (staticWakeVariationAccumulator < updateInterval)
            {
                return;
            }

            float variationDeltaTime = Mathf.Min(
                staticWakeVariationAccumulator,
                0.25f);
            staticWakeVariationAccumulator = 0f;
            staticWakeVariationSourceIds.Clear();

            foreach (KeyValuePair<EntityId, ContinuousSource> pair in
                     continuousSources)
            {
                ContinuousSource source = pair.Value;
                if (source.IsStatic &&
                    source.StaticWakeAmplitude > 0.0001f)
                {
                    staticWakeVariationSourceIds.Add(pair.Key);
                }
            }

            bool anyVariationChanged = false;
            for (int sourceIndex = 0;
                 sourceIndex < staticWakeVariationSourceIds.Count;
                 sourceIndex++)
            {
                EntityId sourceId =
                    staticWakeVariationSourceIds[sourceIndex];
                if (!continuousSources.TryGetValue(
                        sourceId,
                        out ContinuousSource source))
                {
                    continue;
                }

                bool sourceChanged;
                if (source.StaticWakeVariation <= 0.0001f)
                {
                    sourceChanged = ResetStaticWakeVariation(ref source);
                }
                else
                {
                    float sourcePhase = source.Phase;
                    float variationAmount = source.StaticWakeVariation;
                    float intervalMin =
                        source.StaticWakeVariationIntervalMin;
                    float intervalMax =
                        source.StaticWakeVariationIntervalMax;
                    sourceChanged =
                        UpdateStaticWakeLeeVariation(
                            ref source.StaticWakeLeeVariation,
                            sourcePhase,
                            variationAmount,
                            intervalMin,
                            intervalMax,
                            now,
                            variationDeltaTime,
                            updateInterval) |
                        UpdateStaticWakeReleaseVariation(
                            ref source.StaticWakeLeftReleaseVariation,
                            sourcePhase,
                            variationAmount,
                            intervalMin,
                            intervalMax,
                            now,
                            variationDeltaTime,
                            updateInterval,
                            11.17f) |
                        UpdateStaticWakeReleaseVariation(
                            ref source.StaticWakeRightReleaseVariation,
                            sourcePhase,
                            variationAmount,
                            intervalMin,
                            intervalMax,
                            now,
                            variationDeltaTime,
                            updateInterval,
                            23.41f);
                }

                if (sourceChanged)
                {
                    anyVariationChanged = true;
                }

                continuousSources[sourceId] = source;
            }

            if (anyVariationChanged)
            {
                staticWakeSourceDirty = true;
            }
        }

        private static bool ResetStaticWakeVariation(
            ref ContinuousSource source)
        {
            bool changed =
                ResetStaticWakeLeeVariation(
                    ref source.StaticWakeLeeVariation) |
                ResetStaticWakeReleaseVariation(
                    ref source.StaticWakeLeftReleaseVariation) |
                ResetStaticWakeReleaseVariation(
                    ref source.StaticWakeRightReleaseVariation);
            return changed;
        }

        private static bool UpdateStaticWakeLeeVariation(
            ref StaticWakeLeeVariationState state,
            float sourcePhase,
            float variationAmount,
            float intervalMin,
            float intervalMax,
            double now,
            float deltaTime,
            float updateInterval)
        {
            if (!HasValidStaticWakeLeeVariationState(state))
            {
                return false;
            }

            if (!state.ScheduleInitialized)
            {
                state.SelectedInterval = ResolveStaticWakeVariationInterval(
                    sourcePhase,
                    intervalMin,
                    intervalMax,
                    state.EventIndex,
                    3.17f);
                state.NextEventTime = now + state.SelectedInterval;
                state.ScheduleInitialized = true;
            }
            else if (now >= state.NextEventTime &&
                     state.Transition >= 1f)
            {
                BeginStaticWakeLeeVariationTransition(
                    ref state,
                    sourcePhase,
                    variationAmount,
                    intervalMin,
                    intervalMax,
                    now,
                    updateInterval);
            }

            if (state.Transition >= 1f ||
                state.TransitionDuration <= 0.0001f)
            {
                return false;
            }

            state.Transition = Mathf.Min(
                1f,
                state.Transition + deltaTime / state.TransitionDuration);
            ApplyStaticWakeLeeVariationTransition(ref state);
            return true;
        }

        private static bool UpdateStaticWakeReleaseVariation(
            ref StaticWakeReleaseVariationState state,
            float sourcePhase,
            float variationAmount,
            float intervalMin,
            float intervalMax,
            double now,
            float deltaTime,
            float updateInterval,
            float scheduleSalt)
        {
            if (!state.ScheduleInitialized)
            {
                state.SelectedInterval = ResolveStaticWakeVariationInterval(
                    sourcePhase,
                    intervalMin,
                    intervalMax,
                    state.EventIndex,
                    scheduleSalt);
                state.NextEventTime = now + state.SelectedInterval;
                state.ScheduleInitialized = true;
            }
            else if (now >= state.NextEventTime &&
                     state.Transition >= 1f)
            {
                BeginStaticWakeReleaseVariationTransition(
                    ref state,
                    sourcePhase,
                    variationAmount,
                    intervalMin,
                    intervalMax,
                    now,
                    updateInterval,
                    scheduleSalt);
            }

            if (state.Transition >= 1f ||
                state.TransitionDuration <= 0.0001f)
            {
                return false;
            }

            state.Transition = Mathf.Min(
                1f,
                state.Transition + deltaTime / state.TransitionDuration);
            ApplyStaticWakeReleaseVariationTransition(ref state);
            return true;
        }

        private static void BeginStaticWakeLeeVariationTransition(
            ref StaticWakeLeeVariationState state,
            float sourcePhase,
            float variationAmount,
            float intervalMin,
            float intervalMax,
            double now,
            float updateInterval)
        {
            Array.Copy(
                state.CurrentDepthMultipliers,
                state.TransitionStartDepthMultipliers,
                state.SampleCount);
            Array.Copy(
                state.CurrentLengthMultipliers,
                state.TransitionStartLengthMultipliers,
                state.SampleCount);
            Array.Copy(
                state.CurrentTrailingEdgeOffsets,
                state.TransitionStartTrailingEdgeOffsets,
                state.SampleCount);

            state.EventIndex++;
            GenerateStaticWakeLeeTargetProfile(
                ref state,
                sourcePhase,
                variationAmount);
            state.Transition = 0f;
            state.SelectedInterval = ResolveStaticWakeVariationInterval(
                sourcePhase,
                intervalMin,
                intervalMax,
                state.EventIndex,
                4.73f);
            state.TransitionDuration = Mathf.Clamp(
                state.SelectedInterval *
                    StaticWakeVariationTransitionFraction,
                updateInterval,
                state.SelectedInterval);
            state.NextEventTime = now + state.SelectedInterval;
        }

        private static void BeginStaticWakeReleaseVariationTransition(
            ref StaticWakeReleaseVariationState state,
            float sourcePhase,
            float variationAmount,
            float intervalMin,
            float intervalMax,
            double now,
            float updateInterval,
            float scheduleSalt)
        {
            state.TransitionStartLateralOffset =
                state.CurrentLateralOffset;
            state.TransitionStartEnergyMultiplier =
                state.CurrentEnergyMultiplier;
            state.TransitionStartWidthMultiplier =
                state.CurrentWidthMultiplier;
            state.TransitionStartDownstreamOffset =
                state.CurrentDownstreamOffset;

            state.EventIndex++;
            GenerateStaticWakeReleaseTarget(
                ref state,
                sourcePhase,
                state.EventIndex,
                variationAmount,
                scheduleSalt);
            state.Transition = 0f;
            state.SelectedInterval = ResolveStaticWakeVariationInterval(
                sourcePhase,
                intervalMin,
                intervalMax,
                state.EventIndex,
                scheduleSalt + 2.31f);
            state.TransitionDuration = Mathf.Clamp(
                state.SelectedInterval *
                    StaticWakeVariationTransitionFraction,
                updateInterval,
                state.SelectedInterval);
            state.NextEventTime = now + state.SelectedInterval;
        }

        private static float ResolveStaticWakeVariationInterval(
            float sourcePhase,
            float authoredIntervalMin,
            float authoredIntervalMax,
            uint eventIndex,
            float salt)
        {
            float intervalMin = Mathf.Clamp(
                Mathf.Min(authoredIntervalMin, authoredIntervalMax),
                StylizedRiver.MinimumStaticWakeVariationInterval,
                StylizedRiver.MaximumStaticWakeVariationInterval);
            float intervalMax = Mathf.Clamp(
                Mathf.Max(authoredIntervalMin, authoredIntervalMax),
                StylizedRiver.MinimumStaticWakeVariationInterval,
                StylizedRiver.MaximumStaticWakeVariationInterval);
            return Mathf.Lerp(
                intervalMin,
                intervalMax,
                StaticWakeVariationRandom01(
                    sourcePhase,
                    eventIndex,
                    salt));
        }

        private static void GenerateStaticWakeLeeTargetProfile(
            ref StaticWakeLeeVariationState state,
            float sourcePhase,
            float variationAmount)
        {
            float variation = Mathf.Clamp01(variationAmount);
            int family = Mathf.Min(
                5,
                Mathf.FloorToInt(
                    StaticWakeVariationRandom01(
                        sourcePhase,
                        state.EventIndex,
                        0.31f) * 6f));
            state.ProfileFamily = family;

            GenerateStaticWakeVariationPattern(
                state.RawScratch,
                state.SmoothedScratch,
                state.SampleCount,
                sourcePhase,
                state.EventIndex,
                0.67f,
                family);
            for (int index = 0; index < state.SampleCount; index++)
            {
                state.TargetDepthMultipliers[index] = Mathf.Clamp(
                    1f + state.SmoothedScratch[index] *
                    0.20f * variation,
                    0.80f,
                    1.20f);
            }

            GenerateStaticWakeVariationPattern(
                state.RawScratch,
                state.SmoothedScratch,
                state.SampleCount,
                sourcePhase,
                state.EventIndex,
                1.13f,
                (family + 2) % 6);
            for (int index = 0; index < state.SampleCount; index++)
            {
                state.TargetLengthMultipliers[index] = Mathf.Clamp(
                    1f + state.SmoothedScratch[index] *
                    0.15f * variation,
                    0.85f,
                    1.15f);
            }

            GenerateStaticWakeVariationPattern(
                state.RawScratch,
                state.SmoothedScratch,
                state.SampleCount,
                sourcePhase,
                state.EventIndex,
                1.79f,
                (family + 4) % 6);
            for (int index = 0; index < state.SampleCount; index++)
            {
                state.TargetTrailingEdgeOffsets[index] =
                    state.SmoothedScratch[index] *
                    0.75f * variation;
            }
        }

        private static void GenerateStaticWakeVariationPattern(
            float[] raw,
            float[] smoothed,
            int sampleCount,
            float sourcePhase,
            uint eventIndex,
            float salt,
            int family)
        {
            float phaseA = StaticWakeVariationRandom01(
                sourcePhase,
                eventIndex,
                salt + 0.17f) * Mathf.PI * 2f;
            float phaseB = StaticWakeVariationRandom01(
                sourcePhase,
                eventIndex,
                salt + 0.53f) * Mathf.PI * 2f;
            float direction = StaticWakeVariationRandom01(
                sourcePhase,
                eventIndex,
                salt + 0.91f) >= 0.5f
                    ? 1f
                    : -1f;

            for (int index = 0; index < sampleCount; index++)
            {
                float across01 = sampleCount > 1
                    ? index / (float)(sampleCount - 1)
                    : 0.5f;
                float signedAcross = across01 * 2f - 1f;
                float centreShape =
                    1f - signedAcross * signedAcross;
                float edgeShape =
                    Mathf.Abs(signedAcross) * 2f - 1f;
                float shape = family switch
                {
                    0 =>
                        direction * signedAcross +
                        Mathf.Sin(
                            across01 * Mathf.PI * 2f + phaseA) *
                        0.18f,
                    1 =>
                        direction * centreShape +
                        Mathf.Sin(
                            across01 * Mathf.PI * 2f + phaseA) *
                        0.20f,
                    2 =>
                        direction * edgeShape +
                        Mathf.Cos(
                            across01 * Mathf.PI * 2f + phaseA) *
                        0.16f,
                    3 =>
                        Mathf.Cos(
                            across01 * Mathf.PI * 2f + phaseA) *
                        0.82f +
                        direction * signedAcross * 0.18f,
                    4 =>
                        Mathf.Sin(
                            across01 * Mathf.PI * 2f + phaseA) *
                        0.70f +
                        Mathf.Sin(
                            across01 * Mathf.PI * 4f + phaseB) *
                        0.22f,
                    _ =>
                        Mathf.Cos(
                            across01 * Mathf.PI * 2f + phaseA) *
                        0.56f +
                        Mathf.Sin(
                            across01 * Mathf.PI * 4f + phaseB) *
                        0.26f +
                        direction * signedAcross * 0.14f
                };
                float edgeInfluence = Mathf.Lerp(
                    0.38f,
                    1f,
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        1f - Mathf.Abs(signedAcross)));
                raw[index] = shape * edgeInfluence;
            }

            SmoothStaticWakeVariationPattern(
                raw,
                smoothed,
                sampleCount);
            SmoothStaticWakeVariationPattern(
                smoothed,
                raw,
                sampleCount);

            float mean = 0f;
            for (int index = 0; index < sampleCount; index++)
            {
                mean += raw[index];
            }
            mean /= Mathf.Max(1, sampleCount);

            float maximumMagnitude = 0f;
            for (int index = 0; index < sampleCount; index++)
            {
                smoothed[index] = raw[index] - mean;
                maximumMagnitude = Mathf.Max(
                    maximumMagnitude,
                    Mathf.Abs(smoothed[index]));
            }

            float normalization = maximumMagnitude > 0.0001f
                ? 1f / maximumMagnitude
                : 0f;
            for (int index = 0; index < sampleCount; index++)
            {
                smoothed[index] *= normalization;
            }
        }

        private static void SmoothStaticWakeVariationPattern(
            float[] source,
            float[] destination,
            int sampleCount)
        {
            for (int index = 0; index < sampleCount; index++)
            {
                float centre = source[index];
                float left = index > 0
                    ? source[index - 1]
                    : centre;
                float right = index + 1 < sampleCount
                    ? source[index + 1]
                    : centre;
                destination[index] =
                    (left + centre * 2f + right) * 0.25f;
            }
        }

        private static void GenerateStaticWakeReleaseTarget(
            ref StaticWakeReleaseVariationState state,
            float sourcePhase,
            uint eventIndex,
            float variationAmount,
            float salt)
        {
            float variation = Mathf.Clamp01(variationAmount);
            state.TargetLateralOffset =
                StaticWakeVariationRandomSigned(
                    sourcePhase,
                    eventIndex,
                    salt + 0.19f) *
                0.15f * variation;
            state.TargetEnergyMultiplier = Mathf.Clamp(
                1f + StaticWakeVariationRandomSigned(
                    sourcePhase,
                    eventIndex,
                    salt + 0.47f) *
                0.20f * variation,
                0.80f,
                1.20f);
            state.TargetWidthMultiplier = Mathf.Clamp(
                1f + StaticWakeVariationRandomSigned(
                    sourcePhase,
                    eventIndex,
                    salt + 0.83f) *
                0.12f * variation,
                0.88f,
                1.12f);
            state.TargetDownstreamOffset =
                StaticWakeVariationRandomSigned(
                    sourcePhase,
                    eventIndex,
                    salt + 1.21f) *
                0.50f * variation;
        }

        private static void ApplyStaticWakeLeeVariationTransition(
            ref StaticWakeLeeVariationState state)
        {
            float interpolation = Mathf.SmoothStep(
                0f,
                1f,
                state.Transition);
            for (int index = 0; index < state.SampleCount; index++)
            {
                state.CurrentDepthMultipliers[index] = Mathf.Lerp(
                    state.TransitionStartDepthMultipliers[index],
                    state.TargetDepthMultipliers[index],
                    interpolation);
                state.CurrentLengthMultipliers[index] = Mathf.Lerp(
                    state.TransitionStartLengthMultipliers[index],
                    state.TargetLengthMultipliers[index],
                    interpolation);
                state.CurrentTrailingEdgeOffsets[index] = Mathf.Lerp(
                    state.TransitionStartTrailingEdgeOffsets[index],
                    state.TargetTrailingEdgeOffsets[index],
                    interpolation);
            }
        }

        private static void ApplyStaticWakeReleaseVariationTransition(
            ref StaticWakeReleaseVariationState state)
        {
            float interpolation = Mathf.SmoothStep(
                0f,
                1f,
                state.Transition);
            state.CurrentLateralOffset = Mathf.Lerp(
                state.TransitionStartLateralOffset,
                state.TargetLateralOffset,
                interpolation);
            state.CurrentEnergyMultiplier = Mathf.Lerp(
                state.TransitionStartEnergyMultiplier,
                state.TargetEnergyMultiplier,
                interpolation);
            state.CurrentWidthMultiplier = Mathf.Lerp(
                state.TransitionStartWidthMultiplier,
                state.TargetWidthMultiplier,
                interpolation);
            state.CurrentDownstreamOffset = Mathf.Lerp(
                state.TransitionStartDownstreamOffset,
                state.TargetDownstreamOffset,
                interpolation);
        }

        private static bool ResetStaticWakeLeeVariation(
            ref StaticWakeLeeVariationState state)
        {
            if (!HasValidStaticWakeLeeVariationState(state))
            {
                return false;
            }

            bool changed = false;
            for (int index = 0; index < state.SampleCount; index++)
            {
                changed |=
                    Mathf.Abs(state.CurrentDepthMultipliers[index] - 1f) >
                        0.0001f ||
                    Mathf.Abs(state.CurrentLengthMultipliers[index] - 1f) >
                        0.0001f ||
                    Mathf.Abs(state.CurrentTrailingEdgeOffsets[index]) >
                        0.0001f;
                state.CurrentDepthMultipliers[index] = 1f;
                state.TransitionStartDepthMultipliers[index] = 1f;
                state.TargetDepthMultipliers[index] = 1f;
                state.CurrentLengthMultipliers[index] = 1f;
                state.TransitionStartLengthMultipliers[index] = 1f;
                state.TargetLengthMultipliers[index] = 1f;
                state.CurrentTrailingEdgeOffsets[index] = 0f;
                state.TransitionStartTrailingEdgeOffsets[index] = 0f;
                state.TargetTrailingEdgeOffsets[index] = 0f;
            }

            state.Transition = 1f;
            state.TransitionDuration = 0f;
            state.SelectedInterval = 0f;
            state.EventIndex = 0u;
            state.NextEventTime = 0.0;
            state.ScheduleInitialized = false;
            state.ProfileFamily = 0;
            return changed;
        }

        private static bool ResetStaticWakeReleaseVariation(
            ref StaticWakeReleaseVariationState state)
        {
            bool changed =
                Mathf.Abs(state.CurrentLateralOffset) > 0.0001f ||
                Mathf.Abs(state.CurrentEnergyMultiplier - 1f) > 0.0001f ||
                Mathf.Abs(state.CurrentWidthMultiplier - 1f) > 0.0001f ||
                Mathf.Abs(state.CurrentDownstreamOffset) > 0.0001f;
            state = CreateStaticWakeReleaseVariationState();
            return changed;
        }

        private static float StaticWakeVariationRandom01(
            float sourcePhase,
            uint eventIndex,
            float salt)
        {
            float input =
                sourcePhase * 43.117f +
                eventIndex * 13.731f +
                salt * 23.419f;
            return Mathf.Repeat(
                Mathf.Sin(input) * 43758.5453f,
                1f);
        }

        private static float StaticWakeVariationRandomSigned(
            float sourcePhase,
            uint eventIndex,
            float salt)
        {
            return StaticWakeVariationRandom01(
                sourcePhase,
                eventIndex,
                salt) * 2f - 1f;
        }

        private void UpdateStaticPressureProfiles(
            float deltaTime,
            double now)
        {
            if (river == null || deltaTime <= 0f)
            {
                return;
            }

            staticPressureProfileAccumulator += deltaTime;
            float updateInterval =
                1f / Mathf.Max(1f, StaticPressureProfileUpdateRate);
            if (staticPressureProfileAccumulator < updateInterval)
            {
                return;
            }

            float profileDeltaTime = Mathf.Min(
                staticPressureProfileAccumulator,
                0.25f);
            staticPressureProfileAccumulator = 0f;
            staticPressureProfileSourceIds.Clear();

            foreach (KeyValuePair<EntityId, ContinuousSource> pair in
                     continuousSources)
            {
                ContinuousSource source = pair.Value;
                if (source.IsStatic &&
                    source.StaticTargetHeightMetres > 0.0001f &&
                    source.StaticPressureProfile.IsValid &&
                    source.StaticPressureBaseProfile.IsValid &&
                    HasValidPressureProfileState(source))
                {
                    staticPressureProfileSourceIds.Add(pair.Key);
                }
            }

            bool anyProfileChanged = false;
            for (int sourceIndex = 0;
                 sourceIndex < staticPressureProfileSourceIds.Count;
                 sourceIndex++)
            {
                EntityId sourceId =
                    staticPressureProfileSourceIds[sourceIndex];
                if (!continuousSources.TryGetValue(
                        sourceId,
                        out ContinuousSource source))
                {
                    continue;
                }

                if (source.StaticProfileVariation > 0.0001f)
                {
                    if (!source.StaticPressureProfileScheduleInitialized)
                    {
                        float initialInterval =
                            ResolveStaticPressureProfileChangeInterval(
                                source,
                                source.StaticPressureProfileEventIndex,
                                2.03f);
                        source.StaticPressureNextProfileEventTime =
                            now + initialInterval;
                        source.StaticPressureProfileScheduleInitialized = true;
                    }
                    else if (
                        now >= source.StaticPressureNextProfileEventTime &&
                        source.StaticPressureProfileTransition >= 1f)
                    {
                        BeginStaticPressureProfileTransition(
                            ref source,
                            now,
                            updateInterval);
                    }
                }
                else
                {
                    source.StaticPressureProfileScheduleInitialized = false;
                }

                if (source.StaticPressureProfileTransition < 1f &&
                    source.StaticPressureProfileTransitionDuration >
                        0.0001f)
                {
                    source.StaticPressureProfileTransition =
                        Mathf.Min(
                            1f,
                            source.StaticPressureProfileTransition +
                            profileDeltaTime /
                            source.StaticPressureProfileTransitionDuration);
                    ApplyStaticPressureProfileTransition(ref source);
                    anyProfileChanged = true;
                }

                continuousSources[sourceId] = source;
            }

            if (anyProfileChanged)
            {
                // The cached geometry remains unchanged. Only the compact
                // lateral height profiles are rebaked, once after all sources
                // have advanced this update.
                staticPressureTargetDirty = true;
            }
        }

        private void BeginStaticPressureProfileTransition(
            ref ContinuousSource source,
            double now,
            float updateInterval)
        {
            Array.Copy(
                source.StaticPressureCurrentMultipliers,
                source.StaticPressureTransitionStartMultipliers,
                source.StaticPressureCurrentMultipliers.Length);

            source.StaticPressureProfileEventIndex++;
            GenerateStaticPressureTargetProfile(ref source);
            source.StaticPressureProfileTransition = 0f;

            float selectedInterval =
                ResolveStaticPressureProfileChangeInterval(
                    source,
                    source.StaticPressureProfileEventIndex,
                    2.89f);
            source.StaticPressureProfileTransitionDuration = Mathf.Clamp(
                selectedInterval *
                    StaticPressureProfileTransitionFraction,
                updateInterval,
                selectedInterval);
            source.StaticPressureNextProfileEventTime =
                now + selectedInterval;
        }

        private static float ResolveStaticPressureProfileChangeInterval(
            ContinuousSource source,
            uint eventIndex,
            float salt)
        {
            float intervalMin = Mathf.Clamp(
                Mathf.Min(
                    source.StaticPressureProfileChangeIntervalMin,
                    source.StaticPressureProfileChangeIntervalMax),
                StylizedRiver.MinimumStaticPressureProfileChangeInterval,
                StylizedRiver.MaximumStaticPressureProfileChangeInterval);
            float intervalMax = Mathf.Clamp(
                Mathf.Max(
                    source.StaticPressureProfileChangeIntervalMin,
                    source.StaticPressureProfileChangeIntervalMax),
                StylizedRiver.MinimumStaticPressureProfileChangeInterval,
                StylizedRiver.MaximumStaticPressureProfileChangeInterval);
            return Mathf.Lerp(
                intervalMin,
                intervalMax,
                StaticPressureProfileRandom01(
                    source.Phase,
                    eventIndex,
                    salt));
        }

        private static void GenerateStaticPressureTargetProfile(
            ref ContinuousSource source)
        {
            Vector4[] baseSamples =
                source.StaticPressureBaseProfile.Samples;
            float[] target = source.StaticPressureTargetMultipliers;
            int sampleCount = baseSamples.Length;
            float response = Mathf.Clamp01(
                source.StaticProfileVariation * 0.75f);
            int family = Mathf.Min(
                4,
                Mathf.FloorToInt(
                    StaticPressureProfileRandom01(
                        source.Phase,
                        source.StaticPressureProfileEventIndex,
                        0.11f) *
                    5f));
            float phaseA =
                StaticPressureProfileRandom01(
                    source.Phase,
                    source.StaticPressureProfileEventIndex,
                    0.37f) *
                Mathf.PI * 2f;
            float phaseB =
                StaticPressureProfileRandom01(
                    source.Phase,
                    source.StaticPressureProfileEventIndex,
                    0.73f) *
                Mathf.PI * 2f;
            float direction =
                StaticPressureProfileRandom01(
                    source.Phase,
                    source.StaticPressureProfileEventIndex,
                    1.19f) >= 0.5f
                    ? 1f
                    : -1f;
            float centreDirection =
                StaticPressureProfileRandom01(
                    source.Phase,
                    source.StaticPressureProfileEventIndex,
                    1.61f) >= 0.5f
                    ? 1f
                    : -1f;
            float familyAmplitude = family == 0 ? 0.18f : 0.48f;
            float amplitude = familyAmplitude * response;
            float minimumProfileMultiplier = sampleCount >= 64
                ? 0.86f
                : sampleCount >= 32
                    ? 0.82f
                    : StaticPressureMinimumProfileMultiplier;
            float maximumProfileMultiplier = sampleCount >= 64
                ? 1.10f
                : sampleCount >= 32
                    ? 1.12f
                    : MaximumStaticPressureModulation;
            float[] raw = source.StaticPressureRawScratch;
            float[] smoothed = source.StaticPressureSmoothedScratch;
            float rawSum = 0f;
            int validCount = 0;

            for (int index = 0; index < sampleCount; index++)
            {
                if (baseSamples[index].w <= 0.0001f ||
                    baseSamples[index].z <= 0.0001f)
                {
                    raw[index] = 1f;
                    target[index] = 1f;
                    continue;
                }

                float across01 = sampleCount > 1
                    ? index / (float)(sampleCount - 1)
                    : 0.5f;
                float signedAcross = across01 * 2f - 1f;
                float centreShape =
                    1f - 4f *
                    (across01 - 0.5f) *
                    (across01 - 0.5f);
                float shape = family switch
                {
                    0 =>
                        Mathf.Sin(
                            across01 * Mathf.PI * 2f + phaseA) *
                        0.22f,
                    1 =>
                        direction * -signedAcross +
                        Mathf.Sin(
                            across01 * Mathf.PI * 2f + phaseA) *
                        0.18f,
                    2 =>
                        centreDirection * centreShape +
                        Mathf.Sin(
                            across01 * Mathf.PI * 2f + phaseA) *
                        0.16f,
                    3 =>
                        Mathf.Cos(
                            across01 * Mathf.PI * 4f + phaseA) *
                        0.70f +
                        direction * signedAcross * 0.18f,
                    _ =>
                        Mathf.Sin(
                            across01 * Mathf.PI * 4f + phaseA) *
                        0.52f +
                        Mathf.Sin(
                            across01 * Mathf.PI * 6f + phaseB) *
                        0.12f
                };

                raw[index] = Mathf.Max(0.05f, 1f + amplitude * shape);
                rawSum += raw[index];
                validCount++;
            }

            float rawMean = validCount > 0
                ? rawSum / validCount
                : 1f;
            for (int index = 0; index < sampleCount; index++)
            {
                if (baseSamples[index].w <= 0.0001f ||
                    baseSamples[index].z <= 0.0001f)
                {
                    smoothed[index] = 1f;
                    continue;
                }

                float centre = raw[index] / Mathf.Max(0.0001f, rawMean);
                float left = index > 0 &&
                             baseSamples[index - 1].w > 0.0001f
                    ? raw[index - 1] / Mathf.Max(0.0001f, rawMean)
                    : centre;
                float right = index + 1 < sampleCount &&
                              baseSamples[index + 1].w > 0.0001f
                    ? raw[index + 1] / Mathf.Max(0.0001f, rawMean)
                    : centre;
                smoothed[index] =
                    (left + centre * 2f + right) * 0.25f;
            }

            float smoothedSum = 0f;
            validCount = 0;
            for (int index = 0; index < sampleCount; index++)
            {
                if (baseSamples[index].w <= 0.0001f ||
                    baseSamples[index].z <= 0.0001f)
                {
                    continue;
                }

                smoothedSum += smoothed[index];
                validCount++;
            }

            float smoothedMean = validCount > 0
                ? smoothedSum / validCount
                : 1f;
            for (int index = 0; index < sampleCount; index++)
            {
                if (baseSamples[index].w <= 0.0001f ||
                    baseSamples[index].z <= 0.0001f)
                {
                    target[index] = 1f;
                    continue;
                }

                target[index] = Mathf.Clamp(
                    smoothed[index] /
                    Mathf.Max(0.0001f, smoothedMean),
                    minimumProfileMultiplier,
                    maximumProfileMultiplier);
            }
        }

        private static void ApplyStaticPressureProfileTransition(
            ref ContinuousSource source)
        {
            Vector4[] baseSamples =
                source.StaticPressureBaseProfile.Samples;
            Vector4[] animatedSamples =
                source.StaticPressureProfile.Samples;
            float interpolation = Mathf.SmoothStep(
                0f,
                1f,
                source.StaticPressureProfileTransition);

            for (int index = 0; index < baseSamples.Length; index++)
            {
                Vector4 baseSample = baseSamples[index];
                if (baseSample.w <= 0.0001f ||
                    baseSample.z <= 0.0001f)
                {
                    animatedSamples[index] = baseSample;
                    source.StaticPressureCurrentMultipliers[index] = 1f;
                    continue;
                }

                float multiplier = Mathf.Lerp(
                    source.StaticPressureTransitionStartMultipliers[index],
                    source.StaticPressureTargetMultipliers[index],
                    interpolation);
                source.StaticPressureCurrentMultipliers[index] = multiplier;
                baseSample.z = Mathf.Min(
                    baseSample.w,
                    baseSample.z * multiplier);
                animatedSamples[index] = baseSample;
            }
        }

        private static bool HasValidPressureProfileState(
            ContinuousSource source)
        {
            if (!source.StaticPressureBaseProfile.IsValid ||
                !source.StaticPressureProfile.IsValid)
            {
                return false;
            }

            int sampleCount =
                source.StaticPressureBaseProfile.Samples.Length;
            return sampleCount > 0 &&
                   source.StaticPressureProfile.Samples.Length ==
                       sampleCount &&
                   source.StaticPressureCurrentMultipliers != null &&
                   source.StaticPressureTransitionStartMultipliers != null &&
                   source.StaticPressureTargetMultipliers != null &&
                   source.StaticPressureRawScratch != null &&
                   source.StaticPressureSmoothedScratch != null &&
                   source.StaticPressureCurrentMultipliers.Length ==
                       sampleCount &&
                   source.StaticPressureTransitionStartMultipliers.Length ==
                       sampleCount &&
                   source.StaticPressureTargetMultipliers.Length ==
                       sampleCount &&
                   source.StaticPressureRawScratch.Length ==
                       sampleCount &&
                   source.StaticPressureSmoothedScratch.Length ==
                       sampleCount;
        }

        private static RiverDisturbancePressureBakeProfile
            ClonePressureProfile(
                RiverDisturbancePressureBakeProfile source)
        {
            if (!source.IsValid)
            {
                return default;
            }

            Vector4[] samples = new Vector4[source.Samples.Length];
            Array.Copy(source.Samples, samples, source.Samples.Length);
            float[] downstreamBoundaries = source.HasGeometryBounds
                ? new float[source.DownstreamBoundaries.Length]
                : Array.Empty<float>();
            if (downstreamBoundaries.Length > 0)
            {
                Array.Copy(
                    source.DownstreamBoundaries,
                    downstreamBoundaries,
                    source.DownstreamBoundaries.Length);
            }

            return new RiverDisturbancePressureBakeProfile(
                source.AcrossHalfWidth,
                source.LateralSampleCount,
                samples,
                downstreamBoundaries);
        }

        private static float[] CreateUnitPressureProfileMultipliers(
            RiverDisturbancePressureBakeProfile profile)
        {
            if (!profile.IsValid)
            {
                return Array.Empty<float>();
            }

            float[] multipliers = new float[profile.Samples.Length];
            for (int index = 0; index < multipliers.Length; index++)
            {
                multipliers[index] = 1f;
            }

            return multipliers;
        }

        private static float[] CreatePressureProfileScratch(
            RiverDisturbancePressureBakeProfile profile)
        {
            return profile.IsValid
                ? new float[profile.Samples.Length]
                : Array.Empty<float>();
        }

        private static StaticWakeLeeVariationState
            CreateStaticWakeLeeVariationState(int sampleCount)
        {
            int resolvedSampleCount =
                RiverDisturbanceFootprintResolver.
                    ResolvePressureSupportLateralSampleCount(sampleCount);
            return new StaticWakeLeeVariationState
            {
                SampleCount = resolvedSampleCount,
                CurrentDepthMultipliers = CreateFilledFloatArray(
                    resolvedSampleCount,
                    1f),
                TransitionStartDepthMultipliers = CreateFilledFloatArray(
                    resolvedSampleCount,
                    1f),
                TargetDepthMultipliers = CreateFilledFloatArray(
                    resolvedSampleCount,
                    1f),
                CurrentLengthMultipliers = CreateFilledFloatArray(
                    resolvedSampleCount,
                    1f),
                TransitionStartLengthMultipliers = CreateFilledFloatArray(
                    resolvedSampleCount,
                    1f),
                TargetLengthMultipliers = CreateFilledFloatArray(
                    resolvedSampleCount,
                    1f),
                CurrentTrailingEdgeOffsets = new float[
                    resolvedSampleCount],
                TransitionStartTrailingEdgeOffsets = new float[
                    resolvedSampleCount],
                TargetTrailingEdgeOffsets = new float[
                    resolvedSampleCount],
                RawScratch = new float[resolvedSampleCount],
                SmoothedScratch = new float[resolvedSampleCount],
                Transition = 1f,
                TransitionDuration = 0f,
                SelectedInterval = 0f,
                EventIndex = 0u,
                NextEventTime = 0.0,
                ScheduleInitialized = false,
                ProfileFamily = 0
            };
        }

        private static StaticWakeReleaseVariationState
            CreateStaticWakeReleaseVariationState()
        {
            return new StaticWakeReleaseVariationState
            {
                CurrentLateralOffset = 0f,
                TransitionStartLateralOffset = 0f,
                TargetLateralOffset = 0f,
                CurrentEnergyMultiplier = 1f,
                TransitionStartEnergyMultiplier = 1f,
                TargetEnergyMultiplier = 1f,
                CurrentWidthMultiplier = 1f,
                TransitionStartWidthMultiplier = 1f,
                TargetWidthMultiplier = 1f,
                CurrentDownstreamOffset = 0f,
                TransitionStartDownstreamOffset = 0f,
                TargetDownstreamOffset = 0f,
                Transition = 1f,
                TransitionDuration = 0f,
                SelectedInterval = 0f,
                EventIndex = 0u,
                NextEventTime = 0.0,
                ScheduleInitialized = false
            };
        }

        private static float[] CreateFilledFloatArray(
            int length,
            float value)
        {
            float[] result = new float[Mathf.Max(0, length)];
            for (int index = 0; index < result.Length; index++)
            {
                result[index] = value;
            }
            return result;
        }

        private static bool HasValidStaticWakeLeeVariationState(
            StaticWakeLeeVariationState state)
        {
            int sampleCount = state.SampleCount;
            return sampleCount > 0 &&
                   sampleCount <=
                       RiverDisturbanceFootprintResolver.
                           MaximumPressureSupportLateralSamples &&
                   state.CurrentDepthMultipliers != null &&
                   state.TransitionStartDepthMultipliers != null &&
                   state.TargetDepthMultipliers != null &&
                   state.CurrentLengthMultipliers != null &&
                   state.TransitionStartLengthMultipliers != null &&
                   state.TargetLengthMultipliers != null &&
                   state.CurrentTrailingEdgeOffsets != null &&
                   state.TransitionStartTrailingEdgeOffsets != null &&
                   state.TargetTrailingEdgeOffsets != null &&
                   state.RawScratch != null &&
                   state.SmoothedScratch != null &&
                   state.CurrentDepthMultipliers.Length == sampleCount &&
                   state.TransitionStartDepthMultipliers.Length ==
                       sampleCount &&
                   state.TargetDepthMultipliers.Length == sampleCount &&
                   state.CurrentLengthMultipliers.Length == sampleCount &&
                   state.TransitionStartLengthMultipliers.Length ==
                       sampleCount &&
                   state.TargetLengthMultipliers.Length == sampleCount &&
                   state.CurrentTrailingEdgeOffsets.Length == sampleCount &&
                   state.TransitionStartTrailingEdgeOffsets.Length ==
                       sampleCount &&
                   state.TargetTrailingEdgeOffsets.Length == sampleCount &&
                   state.RawScratch.Length == sampleCount &&
                   state.SmoothedScratch.Length == sampleCount;
        }

        private static float StaticPressureProfileRandom01(
            float sourcePhase,
            uint eventIndex,
            float salt)
        {
            float input =
                sourcePhase * 37.719f +
                eventIndex * 11.137f +
                salt * 19.913f;
            return Mathf.Repeat(
                Mathf.Sin(input) * 43758.5453f,
                1f);
        }

        private float ResolveSourcePhase(EntityId sourceId)
        {
            if (continuousSources.TryGetValue(
                    sourceId,
                    out ContinuousSource source))
            {
                return source.Phase;
            }

            float phase = Mathf.Repeat(
                sourcePhaseSequence * GoldenPhaseStep,
                1f);
            sourcePhaseSequence++;
            return phase;
        }

        private void BeginPerformanceDiagnosticsUpdate()
        {
            double now = Time.realtimeSinceStartupAsDouble;
            if (performanceDiagnosticWindowStart <= 0.0 ||
                now - performanceDiagnosticWindowStart >=
                PerformanceDiagnosticWindowSeconds)
            {
                performanceDiagnosticWindowStart = now;
                recentPeakComputeDispatchCount = 0;
                recentPeakThreadGroupCount = 0L;
                recentPeakCellIterationCount = 0L;
                recentPeakFieldRebuildCount = 0;
            }

            lastUpdateComputeDispatchCount = 0;
            lastUpdateThreadGroupCount = 0L;
            lastUpdateCellIterationCount = 0L;
            lastUpdateRippleSimulationDispatchCount = 0;
            lastUpdateWakeSimulationDispatchCount = 0;
            lastUpdateImpactInjectionDispatchCount = 0;
            lastUpdateWakeInjectionDispatchCount = 0;
            lastUpdateStaticPressureBakeDispatchCount = 0;
            lastUpdateStaticWakeBakeDispatchCount = 0;
            lastUpdateRippleBoundaryBakeDispatchCount = 0;
            lastUpdateClearDispatchCount = 0;
            lastUpdateFieldRebuildCount = 0;
        }

        public void ResetPerformanceDiagnosticPeaks()
        {
            performanceDiagnosticWindowStart =
                Time.realtimeSinceStartupAsDouble;
            recentPeakComputeDispatchCount = 0;
            recentPeakThreadGroupCount = 0L;
            recentPeakCellIterationCount = 0L;
            recentPeakFieldRebuildCount = 0;
        }

        private void RecordFieldRebuild()
        {
            lastUpdateFieldRebuildCount++;
            recentPeakFieldRebuildCount = Mathf.Max(
                recentPeakFieldRebuildCount,
                lastUpdateFieldRebuildCount);
        }

        private void DispatchCompute(
            int kernel,
            int groupCountX,
            int groupCountY,
            int groupCountZ,
            PerformanceDispatchCategory category,
            int processedWidth,
            int processedHeight)
        {
            computeShader.Dispatch(
                kernel,
                groupCountX,
                groupCountY,
                groupCountZ);

            lastUpdateComputeDispatchCount++;
            long threadGroups =
                (long)Mathf.Max(0, groupCountX) *
                Mathf.Max(0, groupCountY) *
                Mathf.Max(0, groupCountZ);
            long cellIterations =
                (long)Mathf.Max(0, processedWidth) *
                Mathf.Max(0, processedHeight);
            lastUpdateThreadGroupCount += threadGroups;
            lastUpdateCellIterationCount += cellIterations;

            switch (category)
            {
                case PerformanceDispatchCategory.RippleSimulation:
                    lastUpdateRippleSimulationDispatchCount++;
                    break;
                case PerformanceDispatchCategory.WakeSimulation:
                    lastUpdateWakeSimulationDispatchCount++;
                    break;
                case PerformanceDispatchCategory.ImpactInjection:
                    lastUpdateImpactInjectionDispatchCount++;
                    break;
                case PerformanceDispatchCategory.WakeInjection:
                    lastUpdateWakeInjectionDispatchCount++;
                    break;
                case PerformanceDispatchCategory.StaticPressureBake:
                    lastUpdateStaticPressureBakeDispatchCount++;
                    break;
                case PerformanceDispatchCategory.StaticWakeBake:
                    lastUpdateStaticWakeBakeDispatchCount++;
                    break;
                case PerformanceDispatchCategory.RippleBoundaryBake:
                    lastUpdateRippleBoundaryBakeDispatchCount++;
                    break;
                case PerformanceDispatchCategory.Clear:
                    lastUpdateClearDispatchCount++;
                    break;
            }

            recentPeakComputeDispatchCount = Mathf.Max(
                recentPeakComputeDispatchCount,
                lastUpdateComputeDispatchCount);
            recentPeakThreadGroupCount = Math.Max(
                recentPeakThreadGroupCount,
                lastUpdateThreadGroupCount);
            recentPeakCellIterationCount = Math.Max(
                recentPeakCellIterationCount,
                lastUpdateCellIterationCount);
        }

        private int CountRegisteredStationarySources()
        {
            int count = 0;
            foreach (ContinuousSource source in continuousSources.Values)
            {
                if (source.IsStatic)
                {
                    count++;
                }
            }

            return count;
        }

        private void DispatchClear(
            RenderTexture texture,
            int textureWidth,
            int textureHeight,
            int xOffset,
            int width)
        {
            if (texture == null || computeShader == null || clearKernel < 0)
            {
                return;
            }

            int safeOffset = Mathf.Clamp(xOffset, 0, Mathf.Max(0, textureWidth - 1));
            int safeWidth = Mathf.Clamp(width, 0, textureWidth - safeOffset);
            if (safeWidth <= 0)
            {
                return;
            }

            computeShader.SetInts("_FieldSize", textureWidth, textureHeight);
            computeShader.SetInt("_DispatchXOffset", safeOffset);
            computeShader.SetInt("_DispatchWidth", safeWidth);
            computeShader.SetTexture(clearKernel, "_StateWrite", texture);
            DispatchCompute(
                clearKernel,
                Mathf.CeilToInt(safeWidth / (float)ThreadGroupSize),
                Mathf.CeilToInt(textureHeight / (float)ThreadGroupSize),
                1,
                PerformanceDispatchCategory.Clear,
                safeWidth,
                textureHeight);
        }

        private ImpactReservation CreateImpactReservation(
            ImpactCommand impact,
            double now)
        {
            float resolvedStrength =
                river.ResolvedImpactRippleStrength;
            float geometryContribution =
                Mathf.Clamp01(impact.GeometryContribution);
            float normalContribution =
                Mathf.Clamp01(impact.NormalContribution);
            float ridgeReservationScale = Mathf.Max(
                1f,
                river.ImpactRippleRidgeEmphasis);
            float impulseMagnitude =
                Mathf.Abs(impact.SignedImpulse) *
                resolvedStrength *
                Mathf.Max(geometryContribution, normalContribution) *
                ridgeReservationScale;
            float elevationMagnitude =
                Mathf.Abs(impact.InitialElevation) *
                resolvedStrength *
                geometryContribution /
                0.028f;
            float initialMagnitude = Mathf.Max(
                0.0001f,
                Mathf.Max(impulseMagnitude, elevationMagnitude));
            float minimumVisibleEnergy = Mathf.Max(
                0.0001f,
                river.ImpactRippleMinimumVisibleEnergy);
            float effectiveDecay =
                river.ResolvedImpactRippleDecay;
            float analyticLifetime = initialMagnitude > minimumVisibleEnergy
                ? Mathf.Log(initialMagnitude / minimumVisibleEnergy) /
                  effectiveDecay
                : MinimumImpactReservationLifetime;
            float maximumLifetime = Mathf.Max(
                MinimumImpactReservationLifetime,
                river.ImpactRippleMaximumLifetime);
            float lifetime = Mathf.Clamp(
                analyticLifetime,
                MinimumImpactReservationLifetime,
                maximumLifetime);
            float initialRadius = Mathf.Max(
                ImpactRippleEventSettings.MinimumRadius,
                impact.Radius * RippleInjectionEnvelopeRadius);

            return new ImpactReservation
            {
                EndTime = now + lifetime,
                AgeSeconds = 0f,
                MinimumLifetime = MinimumImpactReservationLifetime,
                MaximumLifetime = maximumLifetime,
                CurrentDistance = impact.Distance,
                CurrentRadius = initialRadius,
                CurrentMagnitude = initialMagnitude,
                MinimumReservedDistance =
                    impact.Distance - initialRadius,
                MaximumReservedDistance =
                    impact.Distance + initialRadius
            };
        }

        private float ResolveImpactReservationLookAhead(
            float deltaTime)
        {
            float updateInterval = 1f / Mathf.Max(
                1f,
                ResolveSimulationRate());
            return Mathf.Max(deltaTime, updateInterval) *
                   RippleReservationLookAheadSteps;
        }

        private void UpdateImpactReservations(
            double now,
            float simulationDeltaTime,
            float lookAhead)
        {
            for (int index = activeImpactReservations.Count - 1;
                 index >= 0;
                 index--)
            {
                ImpactReservation reservation =
                    activeImpactReservations[index];
                if (!UpdateImpactReservation(
                        ref reservation,
                        now,
                        simulationDeltaTime,
                        lookAhead))
                {
                    activeImpactReservations.RemoveAt(index);
                    continue;
                }

                activeImpactReservations[index] = reservation;
            }
        }

        private bool UpdateImpactReservation(
            ref ImpactReservation reservation,
            double now,
            float simulationDeltaTime,
            float lookAhead)
        {
            float elapsed = Mathf.Max(0f, simulationDeltaTime);
            float propagationSpeed = Mathf.Max(
                0.01f,
                river.ImpactRipplePropagation);
            float advectionSpeed = Mathf.Abs(
                river.FlowSpeedMetresPerSecond);
            float effectiveDecay =
                river.ResolvedImpactRippleDecay;
            float minimumVisibleEnergy = Mathf.Max(
                0.0001f,
                river.ImpactRippleMinimumVisibleEnergy);

            reservation.AgeSeconds += elapsed;
            reservation.CurrentDistance += advectionSpeed * elapsed;
            reservation.CurrentRadius += propagationSpeed * elapsed;
            reservation.CurrentMagnitude *= Mathf.Exp(
                -effectiveDecay * elapsed);

            bool minimumLifetimeElapsed =
                reservation.AgeSeconds >= reservation.MinimumLifetime;
            if (reservation.AgeSeconds >= reservation.MaximumLifetime ||
                (minimumLifetimeElapsed &&
                 reservation.CurrentMagnitude <= minimumVisibleEnergy))
            {
                return false;
            }

            float remainingAnalyticLifetime =
                reservation.CurrentMagnitude > minimumVisibleEnergy
                    ? Mathf.Log(
                        reservation.CurrentMagnitude /
                        minimumVisibleEnergy) /
                      effectiveDecay
                    : 0f;
            float minimumRemainingLifetime = Mathf.Max(
                0f,
                reservation.MinimumLifetime - reservation.AgeSeconds);
            float maximumRemainingLifetime = Mathf.Max(
                0f,
                reservation.MaximumLifetime - reservation.AgeSeconds);
            float remainingLifetime = Mathf.Clamp(
                remainingAnalyticLifetime,
                minimumRemainingLifetime,
                maximumRemainingLifetime);
            reservation.EndTime = now + remainingLifetime;

            float predictedTime = Mathf.Min(
                lookAhead,
                remainingLifetime);
            float predictedCentre =
                reservation.CurrentDistance +
                advectionSpeed * predictedTime;
            float predictedRadius =
                reservation.CurrentRadius +
                propagationSpeed * predictedTime;
            float padding = ResolveRippleReservationPaddingMetres(
                predictedCentre);

            reservation.MinimumReservedDistance = Mathf.Min(
                reservation.MinimumReservedDistance,
                predictedCentre - predictedRadius - padding);
            reservation.MaximumReservedDistance = Mathf.Max(
                reservation.MaximumReservedDistance,
                predictedCentre + predictedRadius + padding);

            MarkActiveInterval(
                reservation.MinimumReservedDistance,
                reservation.MaximumReservedDistance,
                reservation.EndTime,
                now);
            return true;
        }

        private float ResolveRippleReservationPaddingMetres(
            float globalDistance)
        {
            if (rippleMetricMinimumAlongCell.Length == 0 ||
                fieldWidth <= 1)
            {
                return 0.25f;
            }

            int row = Mathf.Clamp(
                Mathf.RoundToInt(GlobalDistanceToPixel(globalDistance)),
                0,
                rippleMetricMinimumAlongCell.Length - 1);
            float cellSize = Mathf.Max(
                rippleMetricMinimumAlongCell[row],
                rippleMetricMinimumLateralCell.Length > row
                    ? rippleMetricMinimumLateralCell[row]
                    : 0f);
            return Mathf.Max(
                0.05f,
                cellSize * RippleReservationPaddingCells);
        }

        private void ResetRippleChunkReservationDeadlines(double now)
        {
            for (int chunk = 0; chunk < chunkActiveUntil.Length; chunk++)
            {
                if (chunkActive[chunk])
                {
                    chunkActiveUntil[chunk] = now;
                }
            }
        }

        private void MarkActiveInterval(
            float minimumGlobalDistance,
            float maximumGlobalDistance,
            double activeUntil,
            double now)
        {
            float domainMinimum = river.Domain.GlobalDistanceMinimum;
            float minimumLocalDistance = Mathf.Clamp(
                minimumGlobalDistance - domainMinimum,
                0f,
                validFieldLength);
            float maximumLocalDistance = Mathf.Clamp(
                maximumGlobalDistance - domainMinimum,
                0f,
                validFieldLength);
            if (maximumLocalDistance < minimumLocalDistance)
            {
                float swap = minimumLocalDistance;
                minimumLocalDistance = maximumLocalDistance;
                maximumLocalDistance = swap;
            }

            int firstChunk = Mathf.Clamp(
                Mathf.FloorToInt(
                    minimumLocalDistance / ChunkLengthMetres),
                0,
                chunkCount - 1);
            int lastChunk = Mathf.Clamp(
                Mathf.FloorToInt(
                    maximumLocalDistance / ChunkLengthMetres),
                0,
                chunkCount - 1);

            for (int chunk = firstChunk; chunk <= lastChunk; chunk++)
            {
                if (!chunkActive[chunk])
                {
                    int xOffset = chunk * resolutionPerChunk;
                    DispatchClear(
                        stateA,
                        fieldWidth,
                        fieldHeight,
                        xOffset,
                        resolutionPerChunk);
                    DispatchClear(
                        stateB,
                        fieldWidth,
                        fieldHeight,
                        xOffset,
                        resolutionPerChunk);
                    chunkActive[chunk] = true;
                }

                chunkActiveUntil[chunk] = Math.Max(
                    chunkActiveUntil[chunk],
                    activeUntil);
            }

            lastActivityTime = Math.Max(lastActivityTime, now);
        }

        private void ExpireChunks(double now)
        {
            for (int chunk = 0; chunk < chunkCount; chunk++)
            {
                if (!chunkActive[chunk] ||
                    now < chunkActiveUntil[chunk])
                {
                    continue;
                }

                int xOffset = chunk * resolutionPerChunk;
                DispatchClear(
                    stateA,
                    fieldWidth,
                    fieldHeight,
                    xOffset,
                    resolutionPerChunk);
                DispatchClear(
                    stateB,
                    fieldWidth,
                    fieldHeight,
                    xOffset,
                    resolutionPerChunk);
                chunkActive[chunk] = false;
                chunkActiveUntil[chunk] = 0.0;
            }
        }

        private void ExpireWakeChunks(double now)
        {
            for (int chunk = 0; chunk < chunkCount; chunk++)
            {
                if (!wakeChunkActive[chunk] ||
                    chunkHasStaticSource[chunk] ||
                    now < wakeChunkActiveUntil[chunk])
                {
                    continue;
                }

                int xOffset = chunk * wakeResolutionPerChunk;
                DispatchClear(
                    wakeA,
                    wakeFieldWidth,
                    wakeFieldHeight,
                    xOffset,
                    wakeResolutionPerChunk);
                DispatchClear(
                    wakeB,
                    wakeFieldWidth,
                    wakeFieldHeight,
                    xOffset,
                    wakeResolutionPerChunk);
                wakeChunkActive[chunk] = false;
                wakeChunkActiveUntil[chunk] = 0.0;
            }
        }

        private void CleanupStaleSources(double now)
        {
            staleSourceIds.Clear();

            foreach (KeyValuePair<EntityId, ContinuousSource> pair in continuousSources)
            {
                if (!pair.Value.IsStatic &&
                    now - pair.Value.LastSeen > SourceStaleSeconds)
                {
                    staleSourceIds.Add(pair.Key);
                }
            }

            for (int index = 0; index < staleSourceIds.Count; index++)
            {
                RemoveContinuousSource(staleSourceIds[index]);
            }
        }

        private float ResolveLongestImpactReservationRemainingSeconds()
        {
            if (activeImpactReservations.Count == 0)
            {
                return 0f;
            }

            double now = Time.realtimeSinceStartupAsDouble;
            double longest = 0.0;
            for (int index = 0;
                 index < activeImpactReservations.Count;
                 index++)
            {
                longest = Math.Max(
                    longest,
                    activeImpactReservations[index].EndTime - now);
            }

            return Mathf.Max(0f, (float)longest);
        }

        private void RecordRippleSubstepDiagnostics(int substepCount)
        {
            currentRippleSubstepCount = Mathf.Max(0, substepCount);
            double now = Time.realtimeSinceStartupAsDouble;

            if (rippleSubstepDiagnosticWindowStart <= 0.0 ||
                now - rippleSubstepDiagnosticWindowStart >=
                RippleSubstepDiagnosticWindowSeconds)
            {
                rippleSubstepDiagnosticWindowStart = now;
                maximumRecentRippleSubstepCount =
                    currentRippleSubstepCount;
                return;
            }

            maximumRecentRippleSubstepCount = Mathf.Max(
                maximumRecentRippleSubstepCount,
                currentRippleSubstepCount);
        }

        private float ResolveSimulationRate()
        {
            float qualityRate = river != null
                ? river.Quality switch
                {
                    StylizedRiverQuality.Low => 12f,
                    StylizedRiverQuality.Medium => 20f,
                    StylizedRiverQuality.High => 30f,
                    _ => 20f
                }
                : 20f;

            return HasStaticSources() &&
                   !HasDynamicSources() &&
                   pendingImpacts.Count == 0 &&
                   activeImpactReservations.Count == 0 &&
                   !HasRippleActiveChunks()
                ? Mathf.Min(qualityRate, StaticOnlySimulationRate)
                : qualityRate;
        }

        private bool HasStaticSources()
        {
            foreach (ContinuousSource source in continuousSources.Values)
            {
                if (source.IsStatic)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasDynamicSources()
        {
            foreach (ContinuousSource source in continuousSources.Values)
            {
                if (!source.IsStatic)
                {
                    return true;
                }
            }

            return false;
        }

        private float ResolveAverageSurfaceHalfWidth()
        {
            if (river == null || !river.Domain.IsValid)
            {
                return 1f;
            }

            double sum = 0.0;
            for (int index = 0; index < river.Domain.SampleCount; index++)
            {
                StylizedRiverSplineSample sample = river.Domain.Samples[index];
                sum +=
                    (sample.LeftSurfaceHalfWidth +
                     sample.RightSurfaceHalfWidth) * 0.5;
            }

            return Mathf.Max(
                0.25f,
                (float)(sum / river.Domain.SampleCount));
        }

        private bool BuildRippleMetricData()
        {
            if (river == null ||
                !river.Domain.IsValid ||
                fieldWidth < 2 ||
                fieldHeight < 2 ||
                chunkCount < 1 ||
                resolutionPerChunk < 1)
            {
                return false;
            }

            ReleaseBuffer(ref rippleMetricBuffer);

            try
            {
                Vector2[] centres = new Vector2[fieldWidth];
                Vector2[] tangents = new Vector2[fieldWidth];
                Vector2[] sides = new Vector2[fieldWidth];
                float[] leftWidths = new float[fieldWidth];
                float[] rightWidths = new float[fieldWidth];
                rippleMetricMinimumAlongCell = new float[fieldWidth];
                rippleMetricMinimumLateralCell = new float[fieldWidth];

                float longitudinalDenominator = Mathf.Max(1, fieldWidth - 1);
                for (int row = 0; row < fieldWidth; row++)
                {
                    float orientedDistance = Mathf.Min(
                        row / longitudinalDenominator * fieldLength,
                        validFieldLength);
                    ResolveRippleMetricRow(
                        orientedDistance,
                        out centres[row],
                        out tangents[row],
                        out sides[row],
                        out leftWidths[row],
                        out rightWidths[row]);
                }

                float nominalAlongCell = Mathf.Max(
                    0.001f,
                    fieldLength / longitudinalDenominator);
                float lateralDenominator = Mathf.Max(1, fieldHeight - 1);

                for (int row = 0; row < fieldWidth; row++)
                {
                    float minimumLateral = float.PositiveInfinity;
                    for (int lateral = 0; lateral < fieldHeight - 1; lateral++)
                    {
                        float acrossA =
                            lateral / lateralDenominator * 2f - 1f;
                        float acrossB =
                            (lateral + 1) / lateralDenominator * 2f - 1f;
                        Vector2 positionA = ResolveRippleMetricWorldPosition(
                            centres[row],
                            sides[row],
                            leftWidths[row],
                            rightWidths[row],
                            acrossA);
                        Vector2 positionB = ResolveRippleMetricWorldPosition(
                            centres[row],
                            sides[row],
                            leftWidths[row],
                            rightWidths[row],
                            acrossB);
                        float distance = Vector2.Distance(positionA, positionB);
                        if (distance > 0.0001f)
                        {
                            minimumLateral = Mathf.Min(
                                minimumLateral,
                                distance);
                        }
                    }

                    rippleMetricMinimumLateralCell[row] =
                        float.IsPositiveInfinity(minimumLateral)
                            ? Mathf.Max(
                                0.001f,
                                Mathf.Min(leftWidths[row], rightWidths[row]) *
                                2f / lateralDenominator)
                            : minimumLateral;
                }

                for (int row = 0; row < fieldWidth; row++)
                {
                    float minimumAlong = float.PositiveInfinity;
                    for (int lateral = 0; lateral < fieldHeight; lateral++)
                    {
                        float across =
                            lateral / lateralDenominator * 2f - 1f;
                        Vector2 centrePosition = ResolveRippleMetricWorldPosition(
                            centres[row],
                            sides[row],
                            leftWidths[row],
                            rightWidths[row],
                            across);

                        if (row > 0)
                        {
                            Vector2 previousPosition =
                                ResolveRippleMetricWorldPosition(
                                    centres[row - 1],
                                    sides[row - 1],
                                    leftWidths[row - 1],
                                    rightWidths[row - 1],
                                    across);
                            float distance = Vector2.Distance(
                                centrePosition,
                                previousPosition);
                            if (distance > 0.0001f)
                            {
                                minimumAlong = Mathf.Min(
                                    minimumAlong,
                                    distance);
                            }
                        }

                        if (row + 1 < fieldWidth)
                        {
                            Vector2 nextPosition =
                                ResolveRippleMetricWorldPosition(
                                    centres[row + 1],
                                    sides[row + 1],
                                    leftWidths[row + 1],
                                    rightWidths[row + 1],
                                    across);
                            float distance = Vector2.Distance(
                                centrePosition,
                                nextPosition);
                            if (distance > 0.0001f)
                            {
                                minimumAlong = Mathf.Min(
                                    minimumAlong,
                                    distance);
                            }
                        }
                    }

                    rippleMetricMinimumAlongCell[row] =
                        float.IsPositiveInfinity(minimumAlong)
                            ? nominalAlongCell
                            : minimumAlong;
                }

                RippleMetricRowData[] upload =
                    new RippleMetricRowData[fieldWidth];
                rippleChunkMaximumInverseLength = new float[chunkCount];
                rippleChunkMinimumCellSize = new float[chunkCount];
                for (int chunk = 0; chunk < chunkCount; chunk++)
                {
                    rippleChunkMinimumCellSize[chunk] =
                        float.PositiveInfinity;
                }

                for (int row = 0; row < fieldWidth; row++)
                {
                    float minimumAlong = Mathf.Max(
                        0.001f,
                        rippleMetricMinimumAlongCell[row]);
                    float minimumLateral = Mathf.Max(
                        0.001f,
                        rippleMetricMinimumLateralCell[row]);
                    upload[row] = new RippleMetricRowData
                    {
                        CentreAndTangent = new Vector4(
                            centres[row].x,
                            centres[row].y,
                            tangents[row].x,
                            tangents[row].y),
                        SideAndWidths = new Vector4(
                            sides[row].x,
                            sides[row].y,
                            leftWidths[row],
                            rightWidths[row])
                    };

                    int chunk = Mathf.Clamp(
                        row / resolutionPerChunk,
                        0,
                        chunkCount - 1);
                    float inverseLength = Mathf.Sqrt(
                        1f / (minimumAlong * minimumAlong) +
                        1f / (minimumLateral * minimumLateral));
                    rippleChunkMaximumInverseLength[chunk] = Mathf.Max(
                        rippleChunkMaximumInverseLength[chunk],
                        inverseLength);
                    rippleChunkMinimumCellSize[chunk] = Mathf.Min(
                        rippleChunkMinimumCellSize[chunk],
                        Mathf.Min(minimumAlong, minimumLateral));
                }

                for (int chunk = 0; chunk < chunkCount; chunk++)
                {
                    if (rippleChunkMaximumInverseLength[chunk] <= 0f)
                    {
                        rippleChunkMaximumInverseLength[chunk] =
                            Mathf.Sqrt(2f) / nominalAlongCell;
                    }

                    if (float.IsPositiveInfinity(
                            rippleChunkMinimumCellSize[chunk]))
                    {
                        rippleChunkMinimumCellSize[chunk] =
                            nominalAlongCell;
                    }
                }

                rippleMetricBuffer = new ComputeBuffer(
                    upload.Length,
                    sizeof(float) * 8,
                    ComputeBufferType.Structured);
                rippleMetricBuffer.SetData(upload);
                return true;
            }
            catch (Exception exception)
            {
                ReleaseBuffer(ref rippleMetricBuffer);
                Debug.LogError(
                    $"StylizedRiver on '{name}' could not build its " +
                    $"Impact Ripple metric buffer. {exception.Message}",
                    this);
                return false;
            }
        }

        private void ResolveRippleMetricRow(
            float orientedDistance,
            out Vector2 centre,
            out Vector2 tangent,
            out Vector2 side,
            out float leftWidth,
            out float rightWidth)
        {
            float clampedDistance = Mathf.Clamp(
                orientedDistance,
                0f,
                river.Domain.LocalLength);
            StylizedRiverSplineSample sample =
                river.Domain.SampleAtOrientedDistance(clampedDistance);
            Vector3 surfacePoint = sample.SurfacePoint;
            float extrapolation = Mathf.Max(
                0f,
                orientedDistance - river.Domain.LocalLength);

            if (extrapolation > 0.0001f)
            {
                Vector3 downstreamTangent = river.Domain.ReverseFlow
                    ? -sample.Tangent
                    : sample.Tangent;
                downstreamTangent.y = 0f;
                if (downstreamTangent.sqrMagnitude > 0.000001f)
                {
                    surfacePoint +=
                        downstreamTangent.normalized * extrapolation;
                }
            }

            Vector2 resolvedSide = new Vector2(
                sample.Side.x,
                sample.Side.z);
            if (resolvedSide.sqrMagnitude <= 0.000001f)
            {
                resolvedSide = Vector2.right;
            }
            else
            {
                resolvedSide.Normalize();
            }

            Vector3 downstreamTangent3 = river.Domain.ReverseFlow
                ? -sample.Tangent
                : sample.Tangent;
            Vector2 resolvedTangent = new Vector2(
                downstreamTangent3.x,
                downstreamTangent3.z);
            if (resolvedTangent.sqrMagnitude <= 0.000001f)
            {
                resolvedTangent = new Vector2(
                    -resolvedSide.y,
                    resolvedSide.x);
            }
            else
            {
                resolvedTangent.Normalize();
            }

            centre = new Vector2(surfacePoint.x, surfacePoint.z);
            tangent = resolvedTangent;
            side = resolvedSide;
            leftWidth = Mathf.Max(0.05f, sample.LeftSurfaceHalfWidth);
            rightWidth = Mathf.Max(0.05f, sample.RightSurfaceHalfWidth);
        }

        private static Vector2 ResolveRippleMetricWorldPosition(
            Vector2 centre,
            Vector2 side,
            float leftWidth,
            float rightWidth,
            float acrossNormalized)
        {
            float clampedAcross = Mathf.Clamp(acrossNormalized, -1f, 1f);
            float width = clampedAcross < 0f
                ? leftWidth
                : rightWidth;
            return centre + side * (clampedAcross * width);
        }

        private float ResolveActiveRippleStabilityInverseLength(
            out float minimumCellSize)
        {
            float maximumInverseLength = 0f;
            minimumCellSize = float.PositiveInfinity;

            for (int chunk = 0; chunk < chunkCount; chunk++)
            {
                if (chunk >= chunkActive.Length || !chunkActive[chunk])
                {
                    continue;
                }

                if (chunk < rippleChunkMaximumInverseLength.Length)
                {
                    maximumInverseLength = Mathf.Max(
                        maximumInverseLength,
                        rippleChunkMaximumInverseLength[chunk]);
                }

                if (chunk < rippleChunkMinimumCellSize.Length)
                {
                    minimumCellSize = Mathf.Min(
                        minimumCellSize,
                        rippleChunkMinimumCellSize[chunk]);
                }
            }

            if (maximumInverseLength <= 0f)
            {
                float fallbackCell = Mathf.Max(
                    0.001f,
                    fieldLength / Mathf.Max(1, fieldWidth - 1));
                maximumInverseLength = Mathf.Sqrt(2f) / fallbackCell;
                minimumCellSize = fallbackCell;
            }
            else if (float.IsPositiveInfinity(minimumCellSize))
            {
                minimumCellSize = 0f;
            }

            return maximumInverseLength;
        }

        private void ResolveRippleInjectionRadiusPixels(
            float centreX,
            float radiusMetres,
            out float radiusX,
            out float radiusY)
        {
            float nominalAlongCell = Mathf.Max(
                0.001f,
                fieldLength / Mathf.Max(1, fieldWidth - 1));
            int estimateRadius = Mathf.CeilToInt(
                radiusMetres / nominalAlongCell) + 2;
            int minRow = Mathf.Clamp(
                Mathf.FloorToInt(centreX) - estimateRadius,
                0,
                fieldWidth - 1);
            int maxRow = Mathf.Clamp(
                Mathf.CeilToInt(centreX) + estimateRadius,
                0,
                fieldWidth - 1);
            float minimumAlong = ResolveMinimumMetricValue(
                rippleMetricMinimumAlongCell,
                minRow,
                maxRow,
                nominalAlongCell);
            radiusX = radiusMetres / Mathf.Max(0.001f, minimumAlong);

            minRow = Mathf.Clamp(
                Mathf.FloorToInt(centreX - radiusX) - 2,
                0,
                fieldWidth - 1);
            maxRow = Mathf.Clamp(
                Mathf.CeilToInt(centreX + radiusX) + 2,
                0,
                fieldWidth - 1);
            minimumAlong = ResolveMinimumMetricValue(
                rippleMetricMinimumAlongCell,
                minRow,
                maxRow,
                minimumAlong);
            float minimumLateral = ResolveMinimumMetricValue(
                rippleMetricMinimumLateralCell,
                minRow,
                maxRow,
                nominalAlongCell);
            radiusX = radiusMetres / Mathf.Max(0.001f, minimumAlong);
            radiusY = radiusMetres / Mathf.Max(0.001f, minimumLateral);
        }

        private static float ResolveMinimumMetricValue(
            float[] values,
            int minimumIndex,
            int maximumIndex,
            float fallback)
        {
            if (values == null || values.Length == 0)
            {
                return Mathf.Max(0.001f, fallback);
            }

            int safeMinimum = Mathf.Clamp(
                minimumIndex,
                0,
                values.Length - 1);
            int safeMaximum = Mathf.Clamp(
                maximumIndex,
                safeMinimum,
                values.Length - 1);
            float minimum = float.PositiveInfinity;
            for (int index = safeMinimum; index <= safeMaximum; index++)
            {
                float value = values[index];
                if (value > 0.0001f)
                {
                    minimum = Mathf.Min(minimum, value);
                }
            }

            return float.IsPositiveInfinity(minimum)
                ? Mathf.Max(0.001f, fallback)
                : minimum;
        }

        private float GlobalDistanceToPixel(float globalDistance)
        {
            return FieldGlobalDistanceToPixel(globalDistance, fieldWidth);
        }

        private float WakeGlobalDistanceToPixel(float globalDistance)
        {
            return FieldGlobalDistanceToPixel(globalDistance, wakeFieldWidth);
        }

        private float FieldGlobalDistanceToPixel(
            float globalDistance,
            int targetWidth)
        {
            float localDistance = Mathf.Clamp(
                globalDistance - river.Domain.GlobalDistanceMinimum,
                0f,
                validFieldLength);
            return localDistance / Mathf.Max(0.001f, fieldLength) *
                   Mathf.Max(0, targetWidth - 1);
        }

        private float AcrossToPixel(float acrossNormalized)
        {
            return FieldAcrossToPixel(acrossNormalized, fieldHeight);
        }

        private float WakeAcrossToPixel(float acrossNormalized)
        {
            return FieldAcrossToPixel(acrossNormalized, wakeFieldHeight);
        }

        private static float FieldAcrossToPixel(
            float acrossNormalized,
            int targetHeight)
        {
            return
                (Mathf.Clamp(acrossNormalized, -1f, 1f) * 0.5f + 0.5f) *
                Mathf.Max(0, targetHeight - 1);
        }

        private bool HasActiveChunks()
        {
            return HasRippleActiveChunks() || HasWakeActiveChunks();
        }

        private bool HasRippleActiveChunks()
        {
            for (int index = 0; index < chunkActive.Length; index++)
            {
                if (chunkActive[index])
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasWakeActiveChunks()
        {
            for (int index = 0; index < wakeChunkActive.Length; index++)
            {
                if (wakeChunkActive[index])
                {
                    return true;
                }
            }

            return false;
        }

        private int CountActiveChunks()
        {
            int count = 0;
            for (int index = 0; index < chunkCount; index++)
            {
                bool rippleActive =
                    index < chunkActive.Length && chunkActive[index];
                bool wakeActive =
                    index < wakeChunkActive.Length && wakeChunkActive[index];
                if (rippleActive || wakeActive)
                {
                    count++;
                }
            }

            return count;
        }

        private int CountActiveWakeChunks()
        {
            int count = 0;
            for (int index = 0; index < wakeChunkActive.Length; index++)
            {
                if (wakeChunkActive[index])
                {
                    count++;
                }
            }

            return count;
        }

        private void BindField()
        {
            if (surfaceRenderer == null ||
                currentState == null ||
                previousState == null ||
                currentWake == null ||
                previousWake == null ||
                staticTarget == null ||
                staticWakeSource == null ||
                rippleBoundary == null)
            {
                BindDisabled();
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            surfaceRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(DisturbanceEnabledId, 1f);
            propertyBlock.SetTexture(
                DisturbancePreviousId,
                previousState);
            propertyBlock.SetTexture(
                DisturbanceCurrentId,
                currentState);
            propertyBlock.SetTexture(
                DisturbanceStaticTargetId,
                staticTarget);
            propertyBlock.SetTexture(
                DisturbanceRippleBoundaryId,
                rippleBoundary);
            propertyBlock.SetTexture(
                DisturbanceStaticWakeSourceId,
                staticWakeSource);
            propertyBlock.SetVector(
                DisturbanceStaticWakeTexelSizeId,
                new Vector4(
                    1f / Mathf.Max(1, staticWakeSource.width),
                    1f / Mathf.Max(1, staticWakeSource.height),
                    staticWakeSource.width,
                    staticWakeSource.height));
            propertyBlock.SetTexture(
                DisturbanceWakePreviousId,
                previousWake);
            propertyBlock.SetTexture(
                DisturbanceWakeCurrentId,
                currentWake);
            propertyBlock.SetFloat(
                DisturbanceInterpolationId,
                simulationInterpolation);
            propertyBlock.SetFloat(
                DisturbanceWakeInterpolationId,
                wakeInterpolation);
            propertyBlock.SetFloat(
                DisturbanceGlobalStartId,
                river.Domain.GlobalDistanceMinimum);
            propertyBlock.SetFloat(
                DisturbanceFieldLengthId,
                Mathf.Max(0.001f, fieldLength));
            propertyBlock.SetFloat(
                DisturbanceGeometryStrengthId,
                river.DisturbanceGeometryStrength);
            propertyBlock.SetFloat(
                DisturbanceNormalStrengthId,
                river.DisturbanceNormalStrength);
            propertyBlock.SetFloat(
                DisturbanceShoreInteractionId,
                river.DisturbanceShoreInteraction);
            propertyBlock.SetFloat(
                DisturbanceMaximumHeightId,
                river.ResolvedImpactRippleMaximumHeight);
            propertyBlock.SetFloat(
                DisturbanceStaticMaximumHeightId,
                MaximumStaticPressureHeightMetres);
            propertyBlock.SetFloat(
                DisturbanceWakeGeometryHeightId,
                river.WakeSurfaceHeight);
            propertyBlock.SetFloat(
                DisturbanceWakeGeometryCompactnessId,
                river.WakeSurfaceCompactness);
            propertyBlock.SetFloat(
                DisturbanceDebugViewId,
                (float)river.DisturbanceDebugView);
            propertyBlock.SetFloat(
                DisturbanceFragmentDetailId,
                river.Quality == StylizedRiverQuality.Low ? 0f : 1f);
            surfaceRenderer.SetPropertyBlock(propertyBlock);
        }

        private void BindDisabled()
        {
            if (surfaceRenderer == null && river != null)
            {
                surfaceRenderer = river.SurfaceRenderer;
            }

            if (surfaceRenderer == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            surfaceRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(DisturbanceEnabledId, 0f);
            propertyBlock.SetFloat(DisturbanceWakeGeometryHeightId, 0f);
            propertyBlock.SetFloat(
                DisturbanceWakeGeometryCompactnessId,
                1.50f);
            propertyBlock.SetFloat(DisturbanceFragmentDetailId, 0f);
            propertyBlock.SetFloat(DisturbanceWakeInterpolationId, 1f);
            surfaceRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
