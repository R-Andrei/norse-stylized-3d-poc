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
}
