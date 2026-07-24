using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Weather
{
    public enum WeatherLightRaySourceKind
    {
        Sun = 0,
        Moon = 1
    }

    public enum WeatherLightRayOriginKind
    {
        Procedural = 0,
        Authored = 1,
        GameplayRequested = 2
    }

    public enum WeatherLightRayCloudPolicy
    {
        RespectClouds = 0,
        IgnoreClouds = 1
    }

    public enum WeatherLightRayLifetimePolicy
    {
        Timed = 0,
        Permanent = 1,
        ExternallyControlled = 2
    }

    public enum WeatherLightRaySourceGatePolicy
    {
        RequireActiveSource = 0,
        IgnoreSourceGate = 1
    }

    public enum WeatherLightRayMovementPolicy
    {
        Static = 0,
        CloudLocked = 1,
        LimitedWander = 2,
        FollowTarget = 3
    }

    public enum WeatherLightRayLifecycleState
    {
        Inactive = 0,
        FadingIn = 1,
        Holding = 2,
        FadingOut = 3,
        Suspended = 4
    }

    public enum WeatherLightRayRenderDebugView
    {
        FinalComposite = 0,
        StrandAtmosphere = 1,
        SurfaceInfluence = 2,
        CloudCompensation = 3,
        ScatteredStrands = 4,
        EnvelopeHaze = 5
    }

    public enum WeatherCloudTransmissionStatus
    {
        ClearSky = 0,
        Stable = 1,
        EvolutionUnstable = 2,
        Unavailable = 3,
        Error = 4
    }

    [Serializable]
    public readonly struct WeatherLightRayHandle :
        IEquatable<WeatherLightRayHandle>
    {
        public readonly int SlotIndex;
        public readonly uint Generation;

        public WeatherLightRayHandle(int slotIndex, uint generation)
        {
            SlotIndex = slotIndex;
            Generation = generation;
        }

        public bool IsValid => SlotIndex >= 0 && Generation != 0u;

        public bool Equals(WeatherLightRayHandle other)
        {
            return SlotIndex == other.SlotIndex &&
                Generation == other.Generation;
        }

        public override bool Equals(object value)
        {
            return value is WeatherLightRayHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (SlotIndex * 397) ^ (int)Generation;
            }
        }

        public static bool operator ==(
            WeatherLightRayHandle left,
            WeatherLightRayHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            WeatherLightRayHandle left,
            WeatherLightRayHandle right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return IsValid
                ? $"LightRay[{SlotIndex}:{Generation}]"
                : "LightRay[Invalid]";
        }
    }

    public readonly struct WeatherCloudTransmissionSample
    {
        public readonly WeatherCloudTransmissionStatus Status;
        public readonly float Transmission;
        public readonly Vector2 CookieUv;
        public readonly Vector2 CookieOffset;
        public readonly bool UsesCloudField;
        public readonly string Error;

        public WeatherCloudTransmissionSample(
            WeatherCloudTransmissionStatus status,
            float transmission,
            Vector2 cookieUv,
            Vector2 cookieOffset,
            bool usesCloudField,
            string error)
        {
            Status = status;
            Transmission = Mathf.Clamp01(transmission);
            CookieUv = cookieUv;
            CookieOffset = cookieOffset;
            UsesCloudField = usesCloudField;
            Error = error ?? string.Empty;
        }

        public bool IsStable =>
            Status == WeatherCloudTransmissionStatus.ClearSky ||
            Status == WeatherCloudTransmissionStatus.Stable;

        public bool IsUsable =>
            Status == WeatherCloudTransmissionStatus.ClearSky ||
            Status == WeatherCloudTransmissionStatus.Stable ||
            Status == WeatherCloudTransmissionStatus.EvolutionUnstable;

        public static WeatherCloudTransmissionSample ClearSky()
        {
            return new WeatherCloudTransmissionSample(
                WeatherCloudTransmissionStatus.ClearSky,
                1f,
                Vector2.zero,
                Vector2.zero,
                false,
                string.Empty);
        }

        public static WeatherCloudTransmissionSample Unavailable(
            string error)
        {
            return new WeatherCloudTransmissionSample(
                WeatherCloudTransmissionStatus.Unavailable,
                0f,
                Vector2.zero,
                Vector2.zero,
                false,
                error);
        }

        public static WeatherCloudTransmissionSample Failure(string error)
        {
            return new WeatherCloudTransmissionSample(
                WeatherCloudTransmissionStatus.Error,
                0f,
                Vector2.zero,
                Vector2.zero,
                false,
                error);
        }
    }

    public readonly struct WeatherLightRaySourceState
    {
        public readonly WeatherLightRaySourceKind Kind;
        public readonly Light SourceLight;
        public readonly WeatherLightRaySourceProfile Profile;
        public readonly Vector3 RayDirectionWorld;
        public readonly Vector3 DirectionToSourceWorld;
        public readonly Color Colour;
        public readonly float Intensity;
        public readonly float Elevation;
        public readonly float AvailabilityWeight;
        public readonly bool Available;
        public readonly string UnavailableReason;

        public WeatherLightRaySourceState(
            WeatherLightRaySourceKind kind,
            Light sourceLight,
            WeatherLightRaySourceProfile profile,
            Vector3 rayDirectionWorld,
            Vector3 directionToSourceWorld,
            Color colour,
            float intensity,
            float elevation,
            float availabilityWeight,
            bool available,
            string unavailableReason)
        {
            Kind = kind;
            SourceLight = sourceLight;
            Profile = profile;
            RayDirectionWorld = rayDirectionWorld;
            DirectionToSourceWorld = directionToSourceWorld;
            Colour = colour;
            Intensity = Mathf.Max(0f, intensity);
            Elevation = elevation;
            AvailabilityWeight = Mathf.Clamp01(availabilityWeight);
            Available = available;
            UnavailableReason = unavailableReason ?? string.Empty;
        }
    }

    public readonly struct WeatherLightRayDescriptor
    {
        public readonly WeatherLightRaySourceKind SourceKind;
        public readonly WeatherLightRayOriginKind OriginKind;
        public readonly WeatherLightRayCloudPolicy CloudPolicy;
        public readonly WeatherLightRayLifetimePolicy LifetimePolicy;
        public readonly WeatherLightRaySourceGatePolicy SourceGatePolicy;
        public readonly WeatherLightRayMovementPolicy MovementPolicy;

        public readonly float Height;
        public readonly Vector2 BaseEllipseAxes;
        public readonly Vector2 TopEllipseAxes;
        public readonly float VisualEnvelopeRadiusScale;
        public readonly float VisualEnvelopeEdgeSoftness;
        public readonly float MaximumVisualLeanDegrees;

        public readonly int StrandCount;
        public readonly Vector2 StrandWidthRange;
        public readonly float StrandSpread;
        public readonly float StrandPositionVariation;
        public readonly float StrandIntensityVariation;
        public readonly float StrandLengthVariation;
        public readonly float StrandTaper;
        public readonly float StrandEdgeSoftness;
        public readonly float StrandClusterBias;

        public readonly Color ColourMultiplier;
        public readonly float WarmthContribution;
        public readonly float StrandIntensity;
        public readonly float EnvelopeHazeIntensity;
        public readonly float ScatterLength;
        public readonly float ScatterSoftness;
        public readonly float HeightFade;
        public readonly float CameraIntersectionFade;

        public readonly float GroundLightMultiplier;
        public readonly float VisibleSurfaceLightMultiplier;
        public readonly float CloudCompensationMultiplier;
        public readonly float FootprintEdgeSoftness;
        public readonly float FootprintIrregularity;
        public readonly float CoreEmphasis;

        public readonly float IntensityFluctuationStrength;
        public readonly float IntensityFluctuationSpeed;
        public readonly float WidthBreathingStrength;
        public readonly float LateralDriftStrength;
        public readonly float PatternEvolutionSpeed;
        public readonly float PerStrandPhaseVariation;

        public readonly float FadeInDuration;
        public readonly float HoldDuration;
        public readonly float FadeOutDuration;
        public readonly int GameplayChannel;
        public readonly uint VariationSeed;

        public WeatherLightRayDescriptor(
            WeatherLightRaySourceKind sourceKind,
            WeatherLightRayOriginKind originKind,
            WeatherLightRayCloudPolicy cloudPolicy,
            WeatherLightRayLifetimePolicy lifetimePolicy,
            WeatherLightRaySourceGatePolicy sourceGatePolicy,
            WeatherLightRayMovementPolicy movementPolicy,
            float height,
            Vector2 baseEllipseAxes,
            Vector2 topEllipseAxes,
            float visualEnvelopeRadiusScale,
            float visualEnvelopeEdgeSoftness,
            float maximumVisualLeanDegrees,
            int strandCount,
            Vector2 strandWidthRange,
            float strandSpread,
            float strandPositionVariation,
            float strandIntensityVariation,
            float strandLengthVariation,
            float strandTaper,
            float strandEdgeSoftness,
            float strandClusterBias,
            Color colourMultiplier,
            float warmthContribution,
            float strandIntensity,
            float envelopeHazeIntensity,
            float scatterLength,
            float scatterSoftness,
            float heightFade,
            float cameraIntersectionFade,
            float groundLightMultiplier,
            float visibleSurfaceLightMultiplier,
            float cloudCompensationMultiplier,
            float footprintEdgeSoftness,
            float footprintIrregularity,
            float coreEmphasis,
            float intensityFluctuationStrength,
            float intensityFluctuationSpeed,
            float widthBreathingStrength,
            float lateralDriftStrength,
            float patternEvolutionSpeed,
            float perStrandPhaseVariation,
            float fadeInDuration,
            float holdDuration,
            float fadeOutDuration,
            int gameplayChannel,
            uint variationSeed)
        {
            SourceKind = sourceKind;
            OriginKind = originKind;
            CloudPolicy = cloudPolicy;
            LifetimePolicy = lifetimePolicy;
            SourceGatePolicy = sourceGatePolicy;
            MovementPolicy = movementPolicy;
            Height = Mathf.Max(0.001f, height);
            BaseEllipseAxes = new Vector2(
                Mathf.Max(0.001f, baseEllipseAxes.x),
                Mathf.Max(0.001f, baseEllipseAxes.y));
            TopEllipseAxes = new Vector2(
                Mathf.Max(0.001f, topEllipseAxes.x),
                Mathf.Max(0.001f, topEllipseAxes.y));
            VisualEnvelopeRadiusScale = Mathf.Clamp(
                visualEnvelopeRadiusScale,
                0.1f,
                2f);
            VisualEnvelopeEdgeSoftness = Mathf.Clamp(
                visualEnvelopeEdgeSoftness,
                0.01f,
                1f);
            MaximumVisualLeanDegrees = Mathf.Clamp(
                maximumVisualLeanDegrees,
                0f,
                75f);
            StrandCount = Mathf.Clamp(strandCount, 1, 8);
            float widthMinimum = Mathf.Clamp(
                Mathf.Min(strandWidthRange.x, strandWidthRange.y),
                0.01f,
                0.5f);
            float widthMaximum = Mathf.Clamp(
                Mathf.Max(strandWidthRange.x, strandWidthRange.y),
                widthMinimum,
                0.5f);
            StrandWidthRange = new Vector2(widthMinimum, widthMaximum);
            StrandSpread = Mathf.Clamp01(strandSpread);
            StrandPositionVariation = Mathf.Clamp01(
                strandPositionVariation);
            StrandIntensityVariation = Mathf.Clamp01(
                strandIntensityVariation);
            StrandLengthVariation = Mathf.Clamp01(
                strandLengthVariation);
            StrandTaper = Mathf.Clamp01(strandTaper);
            StrandEdgeSoftness = Mathf.Clamp(
                strandEdgeSoftness,
                0.01f,
                1f);
            StrandClusterBias = Mathf.Clamp01(strandClusterBias);
            ColourMultiplier = colourMultiplier;
            WarmthContribution = Mathf.Clamp01(warmthContribution);
            StrandIntensity = Mathf.Max(0f, strandIntensity);
            EnvelopeHazeIntensity = Mathf.Max(
                0f,
                envelopeHazeIntensity);
            ScatterLength = Mathf.Clamp(scatterLength, 0f, 8f);
            ScatterSoftness = Mathf.Clamp01(scatterSoftness);
            HeightFade = Mathf.Clamp(heightFade, 0.001f, 0.49f);
            CameraIntersectionFade = Mathf.Clamp01(
                cameraIntersectionFade);
            GroundLightMultiplier = Mathf.Max(
                0f,
                groundLightMultiplier);
            VisibleSurfaceLightMultiplier = Mathf.Max(
                0f,
                visibleSurfaceLightMultiplier);
            CloudCompensationMultiplier = Mathf.Max(
                0f,
                cloudCompensationMultiplier);
            FootprintEdgeSoftness = Mathf.Clamp(
                footprintEdgeSoftness,
                0.01f,
                1f);
            FootprintIrregularity = Mathf.Clamp01(
                footprintIrregularity);
            CoreEmphasis = Mathf.Max(0f, coreEmphasis);
            IntensityFluctuationStrength = Mathf.Clamp(
                intensityFluctuationStrength,
                0f,
                0.5f);
            IntensityFluctuationSpeed = Mathf.Max(
                0f,
                intensityFluctuationSpeed);
            WidthBreathingStrength = Mathf.Clamp(
                widthBreathingStrength,
                0f,
                0.35f);
            LateralDriftStrength = Mathf.Clamp(
                lateralDriftStrength,
                0f,
                0.25f);
            PatternEvolutionSpeed = Mathf.Max(
                0f,
                patternEvolutionSpeed);
            PerStrandPhaseVariation = Mathf.Clamp01(
                perStrandPhaseVariation);
            FadeInDuration = Mathf.Max(0f, fadeInDuration);
            HoldDuration = Mathf.Max(0f, holdDuration);
            FadeOutDuration = Mathf.Max(0f, fadeOutDuration);
            GameplayChannel = gameplayChannel;
            VariationSeed = variationSeed == 0u ? 1u : variationSeed;
        }
    }

    public readonly struct WeatherLightRaySnapshot
    {
        public readonly WeatherLightRayHandle Handle;
        public readonly WeatherLightRayDescriptor Descriptor;
        public readonly WeatherLightRayLifecycleState LifecycleState;
        public readonly Vector3 BaseCentreWorld;
        public readonly Vector3 RayDirectionWorld;
        public readonly double SpawnTime;
        public readonly double HoldOrExpiryTime;
        public readonly float CurrentIntensity;
        public readonly float CurrentCloudTransmission;

        public WeatherLightRaySnapshot(
            WeatherLightRayHandle handle,
            WeatherLightRayDescriptor descriptor,
            WeatherLightRayLifecycleState lifecycleState,
            Vector3 baseCentreWorld,
            Vector3 rayDirectionWorld,
            double spawnTime,
            double holdOrExpiryTime,
            float currentIntensity,
            float currentCloudTransmission)
        {
            Handle = handle;
            Descriptor = descriptor;
            LifecycleState = lifecycleState;
            BaseCentreWorld = baseCentreWorld;
            RayDirectionWorld = rayDirectionWorld;
            SpawnTime = spawnTime;
            HoldOrExpiryTime = holdOrExpiryTime;
            CurrentIntensity = Mathf.Clamp01(currentIntensity);
            CurrentCloudTransmission = Mathf.Clamp01(
                currentCloudTransmission);
        }

        public WeatherLightRaySourceKind SourceKind =>
            Descriptor.SourceKind;
        public WeatherLightRayOriginKind OriginKind =>
            Descriptor.OriginKind;
        public WeatherLightRayCloudPolicy CloudPolicy =>
            Descriptor.CloudPolicy;
        public WeatherLightRayLifetimePolicy LifetimePolicy =>
            Descriptor.LifetimePolicy;
        public WeatherLightRaySourceGatePolicy SourceGatePolicy =>
            Descriptor.SourceGatePolicy;
        public WeatherLightRayMovementPolicy MovementPolicy =>
            Descriptor.MovementPolicy;
        public float Height => Descriptor.Height;
        public Vector2 BaseEllipseAxes => Descriptor.BaseEllipseAxes;
        public Vector2 TopEllipseAxes => Descriptor.TopEllipseAxes;
        public float VisualIntensityMultiplier =>
            Descriptor.StrandIntensity;
        public float GroundLightMultiplier =>
            Descriptor.GroundLightMultiplier;
        public float VisibleSurfaceLightMultiplier =>
            Descriptor.VisibleSurfaceLightMultiplier;
        public float SurfaceLightMultiplier =>
            Descriptor.VisibleSurfaceLightMultiplier;
        public float CloudCompensationMultiplier =>
            Descriptor.CloudCompensationMultiplier;
        public Color ColourMultiplier => Descriptor.ColourMultiplier;
        public float WarmthContribution => Descriptor.WarmthContribution;
        public float EdgeSoftness => Descriptor.FootprintEdgeSoftness;
        public float CoreEmphasis => Descriptor.CoreEmphasis;
        public float FluctuationStrength =>
            Descriptor.IntensityFluctuationStrength;
        public float FluctuationSpeed =>
            Descriptor.IntensityFluctuationSpeed;
        public float FadeInDuration => Descriptor.FadeInDuration;
        public float FadeOutDuration => Descriptor.FadeOutDuration;
        public int GameplayChannel => Descriptor.GameplayChannel;
        public uint VariationSeed => Descriptor.VariationSeed;
    }
}
