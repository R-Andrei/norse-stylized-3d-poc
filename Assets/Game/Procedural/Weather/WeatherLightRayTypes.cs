using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Weather
{
    public enum WeatherLightRaySourceKind
    {
        Sun = 0,
        Moon = 1,
        Independent = 2
    }

    public enum WeatherLightRayPresetControlMode
    {
        Manual = 0,
        SelectionProfile = 1
    }

    public enum WeatherLightRayCycleSourceMode
    {
        TimeOfDay = 0,
        ManualNormalizedValue = 1,
        ExternalRuntimeOverride = 2
    }

    public enum WeatherLightRayDirectionMode
    {
        ControllerDirectionalSource = 0,
        Vertical = 1,
        FixedWorldDirection = 2
    }

    public enum WeatherLightRaySourceAvailabilityPolicy
    {
        Ignore = 0,
        Require = 1,
        MultiplyActivation = 2
    }

    public enum WeatherLightRayCloudProjectionMode
    {
        None = 0,
        CloudControllerDirectionalSource = 1
    }

    public enum WeatherLightRayCloudDataRequirement
    {
        Ignored = 0,
        Optional = 1,
        Required = 2
    }

    public enum WeatherLightRaySpatialCloudPolicy
    {
        AnyPosition = 0,
        ClearFootprint = 1,
        DistinctCloudOpening = 2
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


    public enum WeatherLightRayEvolutionPreset
    {
        Static = 0,
        Subtle = 1,
        Living = 2,
        Custom = 3
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
        RawContinuousBeams = 1,
        SurfaceIllumination = 2,
        SoftenedContinuousBeams = 4
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


    public enum WeatherLightRaySpawnPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }

    [Serializable]
    public readonly struct WeatherLightRaySpawnRequest
    {
        public readonly Vector3 BaseCentreWorld;
        /// <summary>
        /// Optional per-instance direction override. Vector3.zero means
        /// inherit the active source direction. A finite non-zero vector is
        /// normalized and used instead.
        /// </summary>
        public readonly Vector3 RayDirectionWorld;
        public readonly WeatherLightRaySourceKind SourceKind;
        public readonly float AreaDiameterMetres;
        public readonly bool OverrideHeight;
        public readonly float HeightMetres;
        public readonly bool OverrideMaximumVisualLean;
        public readonly float MaximumVisualLeanDegrees;
        public readonly bool OverrideBeamSpacing;
        public readonly float BeamSpacingMetres;
        public readonly uint VariationSeed;
        public readonly float LocalIntensityMultiplier;
        public readonly WeatherLightRayCloudPolicy CloudPolicy;
        public readonly WeatherLightRayLifetimePolicy LifetimePolicy;
        public readonly WeatherLightRaySourceGatePolicy SourceGatePolicy;
        public readonly WeatherLightRayMovementPolicy MovementPolicy;
        public readonly float FadeInDurationSeconds;
        public readonly float HoldDurationSeconds;
        public readonly float FadeOutDurationSeconds;
        public readonly int GameplayChannel;
        public readonly long ExternalIdentity;
        public readonly WeatherLightRaySpawnPriority Priority;
        public readonly bool InitiallyVisible;

        public WeatherLightRaySpawnRequest(
            Vector3 baseCentreWorld,
            float areaDiameterMetres,
            uint variationSeed,
            float localIntensityMultiplier = 1f,
            WeatherLightRayLifetimePolicy lifetimePolicy =
                WeatherLightRayLifetimePolicy.ExternallyControlled,
            float fadeInDurationSeconds = 0.75f,
            float holdDurationSeconds = 5f,
            float fadeOutDurationSeconds = 0.75f,
            bool initiallyVisible = true,
            Vector3 rayDirectionWorld = default,
            WeatherLightRaySourceKind sourceKind =
                WeatherLightRaySourceKind.Sun,
            bool overrideHeight = false,
            float heightMetres = 0f,
            bool overrideMaximumVisualLean = false,
            float maximumVisualLeanDegrees = 0f,
            bool overrideBeamSpacing = false,
            float beamSpacingMetres =
                WeatherLightRayAreaLayout.DefaultBeamSpacingMetres,
            WeatherLightRayCloudPolicy cloudPolicy =
                WeatherLightRayCloudPolicy.IgnoreClouds,
            WeatherLightRaySourceGatePolicy sourceGatePolicy =
                WeatherLightRaySourceGatePolicy.RequireActiveSource,
            WeatherLightRayMovementPolicy movementPolicy =
                WeatherLightRayMovementPolicy.Static,
            int gameplayChannel = 0,
            long externalIdentity = 0,
            WeatherLightRaySpawnPriority priority =
                WeatherLightRaySpawnPriority.Normal)
        {
            BaseCentreWorld = baseCentreWorld;
            RayDirectionWorld = rayDirectionWorld;
            SourceKind = sourceKind;
            AreaDiameterMetres = Mathf.Max(
                WeatherLightRayAreaLayout.MinimumDiameterMetres,
                areaDiameterMetres);
            OverrideHeight = overrideHeight;
            HeightMetres = Mathf.Max(0.001f, heightMetres);
            OverrideMaximumVisualLean = overrideMaximumVisualLean;
            MaximumVisualLeanDegrees = Mathf.Clamp(
                maximumVisualLeanDegrees,
                0f,
                75f);
            OverrideBeamSpacing = overrideBeamSpacing;
            BeamSpacingMetres = Mathf.Clamp(
                beamSpacingMetres,
                WeatherLightRayAreaLayout.MinimumBeamSpacingMetres,
                WeatherLightRayAreaLayout.MaximumBeamSpacingMetres);
            VariationSeed = variationSeed == 0u ? 1u : variationSeed;
            LocalIntensityMultiplier = Mathf.Max(
                0f,
                localIntensityMultiplier);
            CloudPolicy = cloudPolicy;
            LifetimePolicy = lifetimePolicy;
            SourceGatePolicy = sourceGatePolicy;
            MovementPolicy = movementPolicy;
            FadeInDurationSeconds = Mathf.Max(0f, fadeInDurationSeconds);
            HoldDurationSeconds = Mathf.Max(0f, holdDurationSeconds);
            FadeOutDurationSeconds = Mathf.Max(0f, fadeOutDurationSeconds);
            GameplayChannel = gameplayChannel;
            ExternalIdentity = externalIdentity;
            Priority = priority;
            InitiallyVisible = initiallyVisible;
        }
    }

    [Serializable]
    public readonly struct WeatherLightRayUpdateRequest
    {
        public readonly WeatherLightRaySpawnRequest SpawnRequest;
        public readonly bool ResetLifecycle;

        public WeatherLightRayUpdateRequest(
            in WeatherLightRaySpawnRequest spawnRequest,
            bool resetLifecycle = false)
        {
            SpawnRequest = spawnRequest;
            ResetLifecycle = resetLifecycle;
        }
    }

    public interface IWeatherLightRayCloudClearanceProvider
    {
        bool TryResolveOpening(
            in WeatherLightRayCloudQuery query,
            out WeatherLightRayCloudOpening opening);
    }

    [Serializable]
    public readonly struct WeatherLightRayCloudQuery
    {
        public readonly WeatherLightRaySourceKind SourceKind;
        public readonly Vector3 SearchCentreWorld;
        /// <summary>
        /// Optional per-instance direction preference. Vector3.zero means
        /// inherit the active source direction. A finite non-zero vector is
        /// an explicit direction override.
        /// </summary>
        public readonly Vector3 PreferredRayDirectionWorld;
        public readonly float MinimumDiameterMetres;
        public readonly float MaximumDiameterMetres;
        public readonly float MinimumConfidence;
        public readonly long IdentityHint;

        public WeatherLightRayCloudQuery(
            WeatherLightRaySourceKind sourceKind,
            Vector3 searchCentreWorld,
            float minimumDiameterMetres,
            float maximumDiameterMetres,
            float minimumConfidence = 0f,
            Vector3 preferredRayDirectionWorld = default,
            long identityHint = 0)
        {
            SourceKind = sourceKind;
            SearchCentreWorld = searchCentreWorld;
            PreferredRayDirectionWorld = preferredRayDirectionWorld;
            MinimumDiameterMetres = Mathf.Max(
                WeatherLightRayAreaLayout.MinimumDiameterMetres,
                minimumDiameterMetres);
            MaximumDiameterMetres = Mathf.Max(
                MinimumDiameterMetres,
                maximumDiameterMetres);
            MinimumConfidence = Mathf.Clamp01(minimumConfidence);
            IdentityHint = identityHint;
        }
    }

    [Serializable]
    public readonly struct WeatherLightRayCloudOpening
    {
        public readonly long StableIdentity;
        public readonly WeatherLightRaySourceKind SourceKind;
        public readonly Vector3 BaseCentreWorld;
        /// <summary>
        /// Optional per-instance direction override. Vector3.zero means
        /// inherit the active source direction. A finite non-zero vector is
        /// normalized and used as the opening's explicit direction.
        /// </summary>
        public readonly Vector3 RayDirectionWorld;
        public readonly float AreaDiameterMetres;
        public readonly float ClearanceStrength;
        public readonly float EdgeSoftnessSignal;
        public readonly float Confidence;
        public readonly uint DataVersion;

        public WeatherLightRayCloudOpening(
            long stableIdentity,
            WeatherLightRaySourceKind sourceKind,
            Vector3 baseCentreWorld,
            Vector3 rayDirectionWorld,
            float areaDiameterMetres,
            float clearanceStrength,
            float edgeSoftnessSignal,
            float confidence,
            uint dataVersion = 0u)
        {
            StableIdentity = stableIdentity;
            SourceKind = sourceKind;
            BaseCentreWorld = baseCentreWorld;
            RayDirectionWorld = rayDirectionWorld;
            AreaDiameterMetres = Mathf.Max(
                WeatherLightRayAreaLayout.MinimumDiameterMetres,
                areaDiameterMetres);
            ClearanceStrength = Mathf.Clamp01(clearanceStrength);
            EdgeSoftnessSignal = Mathf.Clamp01(edgeSoftnessSignal);
            Confidence = Mathf.Clamp01(confidence);
            DataVersion = dataVersion;
        }
    }

    [Serializable]
    public readonly struct WeatherLightRayCloudSpawnSettings
    {
        public readonly uint VariationSeed;
        public readonly float LocalIntensityMultiplier;
        public readonly WeatherLightRayLifetimePolicy LifetimePolicy;
        public readonly float FadeInDurationSeconds;
        public readonly float HoldDurationSeconds;
        public readonly float FadeOutDurationSeconds;
        public readonly bool InitiallyVisible;
        public readonly bool OverrideHeight;
        public readonly float HeightMetres;
        public readonly bool OverrideMaximumVisualLean;
        public readonly float MaximumVisualLeanDegrees;
        public readonly bool OverrideBeamSpacing;
        public readonly float BeamSpacingMetres;
        public readonly WeatherLightRayCloudPolicy RuntimeCloudPolicy;
        public readonly WeatherLightRaySourceGatePolicy SourceGatePolicy;
        public readonly WeatherLightRayMovementPolicy MovementPolicy;
        public readonly int GameplayChannel;
        public readonly WeatherLightRaySpawnPriority Priority;
        public readonly bool ResetLifecycleOnUpdate;

        public WeatherLightRayCloudSpawnSettings(
            uint variationSeed,
            float localIntensityMultiplier = 1f,
            WeatherLightRayLifetimePolicy lifetimePolicy =
                WeatherLightRayLifetimePolicy.ExternallyControlled,
            float fadeInDurationSeconds = 0.75f,
            float holdDurationSeconds = 5f,
            float fadeOutDurationSeconds = 0.75f,
            bool initiallyVisible = true,
            bool overrideHeight = false,
            float heightMetres = 0f,
            bool overrideMaximumVisualLean = false,
            float maximumVisualLeanDegrees = 0f,
            bool overrideBeamSpacing = false,
            float beamSpacingMetres =
                WeatherLightRayAreaLayout.DefaultBeamSpacingMetres,
            WeatherLightRayCloudPolicy runtimeCloudPolicy =
                WeatherLightRayCloudPolicy.IgnoreClouds,
            WeatherLightRaySourceGatePolicy sourceGatePolicy =
                WeatherLightRaySourceGatePolicy.RequireActiveSource,
            WeatherLightRayMovementPolicy movementPolicy =
                WeatherLightRayMovementPolicy.Static,
            int gameplayChannel = 0,
            WeatherLightRaySpawnPriority priority =
                WeatherLightRaySpawnPriority.Normal,
            bool resetLifecycleOnUpdate = false)
        {
            VariationSeed = variationSeed == 0u ? 1u : variationSeed;
            LocalIntensityMultiplier = Mathf.Max(
                0f,
                localIntensityMultiplier);
            LifetimePolicy = lifetimePolicy;
            FadeInDurationSeconds = Mathf.Max(0f, fadeInDurationSeconds);
            HoldDurationSeconds = Mathf.Max(0f, holdDurationSeconds);
            FadeOutDurationSeconds = Mathf.Max(0f, fadeOutDurationSeconds);
            InitiallyVisible = initiallyVisible;
            OverrideHeight = overrideHeight;
            HeightMetres = Mathf.Max(0.001f, heightMetres);
            OverrideMaximumVisualLean = overrideMaximumVisualLean;
            MaximumVisualLeanDegrees = Mathf.Clamp(
                maximumVisualLeanDegrees,
                0f,
                75f);
            OverrideBeamSpacing = overrideBeamSpacing;
            BeamSpacingMetres = Mathf.Clamp(
                beamSpacingMetres,
                WeatherLightRayAreaLayout.MinimumBeamSpacingMetres,
                WeatherLightRayAreaLayout.MaximumBeamSpacingMetres);
            RuntimeCloudPolicy = runtimeCloudPolicy;
            SourceGatePolicy = sourceGatePolicy;
            MovementPolicy = movementPolicy;
            GameplayChannel = gameplayChannel;
            Priority = priority;
            ResetLifecycleOnUpdate = resetLifecycleOnUpdate;
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

    public readonly struct WeatherLightRayAreaLayout
    {
        public const float MinimumDiameterMetres = 0.60f;
        public const float DefaultBeamSpacingMetres = 1.05f;
        public const float MinimumBeamSpacingMetres = 0.20f;
        public const float MaximumBeamSpacingMetres = 4.00f;
        public const float MinimumAdjacentOverlapRatio = 0.28f;
        public const float MaximumAdjacentOverlapRatio = 0.50f;
        public const float RepresentativeAdjacentOverlapRatio = 0.39f;
        public const int MinimumBeamCount = 2;

        public readonly float DiameterMetres;
        public readonly float RadiusMetres;
        public readonly int BeamCount;
        public readonly float BeamPitchMetres;

        public float AverageAtmosphericBeamWidthMetres =>
            DiameterMetres / Mathf.Max(
                1f,
                BeamCount -
                    (BeamCount - 1) *
                    RepresentativeAdjacentOverlapRatio);

        public float AverageAtmosphericOverlapMetres => BeamCount > 1
            ? AverageAtmosphericBeamWidthMetres *
                RepresentativeAdjacentOverlapRatio
            : 0f;

        private WeatherLightRayAreaLayout(
            float diameterMetres,
            int beamCount,
            float beamPitchMetres)
        {
            DiameterMetres = diameterMetres;
            RadiusMetres = diameterMetres * 0.5f;
            BeamCount = beamCount;
            BeamPitchMetres = beamPitchMetres;
        }

        public static WeatherLightRayAreaLayout Calculate(
            float requestedDiameterMetres,
            float requestedBeamSpacingMetres = DefaultBeamSpacingMetres)
        {
            float diameter = Mathf.Max(
                MinimumDiameterMetres,
                !float.IsNaN(requestedDiameterMetres) &&
                    !float.IsInfinity(requestedDiameterMetres)
                    ? requestedDiameterMetres
                    : MinimumDiameterMetres);
            float beamSpacing = Mathf.Clamp(
                !float.IsNaN(requestedBeamSpacingMetres) &&
                    !float.IsInfinity(requestedBeamSpacingMetres)
                    ? requestedBeamSpacingMetres
                    : DefaultBeamSpacingMetres,
                MinimumBeamSpacingMetres,
                MaximumBeamSpacingMetres);
            double normalizedSegments =
                (double)diameter / beamSpacing;
            double requestedSegments = Math.Ceiling(
                normalizedSegments - 1e-9);
            int segmentCount = requestedSegments >= int.MaxValue - 1d
                ? int.MaxValue - 1
                : Math.Max(
                    MinimumBeamCount - 1,
                    (int)requestedSegments);
            int beamCount = segmentCount + 1;
            float beamPitch = diameter / segmentCount;
            return new WeatherLightRayAreaLayout(
                diameter,
                beamCount,
                beamPitch);
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
        public readonly float MaximumVisualLeanDegrees;

        public readonly float AreaDiameterMetres;
        public readonly float FootprintRadiusMetres;
        public readonly int BeamCount;
        public readonly float BeamSpacingMetres;
        public readonly float BeamPitchMetres;
        public readonly Vector2 BeamWidthRatioRange;
        public readonly float BeamIntensityVariation;
        public readonly float BeamEdgeSoftness;
        public readonly float BeamSoftnessVariation;
        public readonly float UpperFade;
        public readonly float GroundFade;
        public readonly float ContactPlaneOpacity;

        public readonly Color ColourMultiplier;
        public readonly float WarmthContribution;
        public readonly float AtmosphericIntensity;
        public readonly float SofteningStrength;
        public readonly float CameraIntersectionFade;

        public readonly float SurfaceSpotLightIntensity;
        public readonly float ScreenSpaceSurfaceIntensity;
        public readonly float FootprintEdgeSoftness;

        public readonly WeatherLightRayEvolutionPreset EvolutionPreset;
        public readonly float EvolutionStrength;
        public readonly float EvolutionSpeed;

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
            float maximumVisualLeanDegrees,
            float areaDiameterMetres,
            float beamSpacingMetres,
            Vector2 beamWidthRatioRange,
            float beamIntensityVariation,
            float beamEdgeSoftness,
            float beamSoftnessVariation,
            float upperFade,
            float groundFade,
            float contactPlaneOpacity,
            Color colourMultiplier,
            float warmthContribution,
            float atmosphericIntensity,
            float softeningStrength,
            float cameraIntersectionFade,
            float surfaceSpotLightIntensity,
            float screenSpaceSurfaceIntensity,
            float footprintEdgeSoftness,
            WeatherLightRayEvolutionPreset evolutionPreset,
            float evolutionStrength,
            float evolutionSpeed,
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
            MaximumVisualLeanDegrees = Mathf.Clamp(
                maximumVisualLeanDegrees,
                0f,
                75f);

            float resolvedBeamSpacing = Mathf.Clamp(
                !float.IsNaN(beamSpacingMetres) &&
                    !float.IsInfinity(beamSpacingMetres)
                    ? beamSpacingMetres
                    : WeatherLightRayAreaLayout.DefaultBeamSpacingMetres,
                WeatherLightRayAreaLayout.MinimumBeamSpacingMetres,
                WeatherLightRayAreaLayout.MaximumBeamSpacingMetres);
            WeatherLightRayAreaLayout layout =
                WeatherLightRayAreaLayout.Calculate(
                    areaDiameterMetres,
                    resolvedBeamSpacing);
            AreaDiameterMetres = layout.DiameterMetres;
            FootprintRadiusMetres = layout.RadiusMetres;
            BeamCount = layout.BeamCount;
            BeamSpacingMetres = resolvedBeamSpacing;
            BeamPitchMetres = layout.BeamPitchMetres;

            float widthRatioMinimum = Mathf.Clamp(
                Mathf.Min(
                    beamWidthRatioRange.x,
                    beamWidthRatioRange.y),
                1f,
                2f);
            float widthRatioMaximum = Mathf.Clamp(
                Mathf.Max(
                    beamWidthRatioRange.x,
                    beamWidthRatioRange.y),
                widthRatioMinimum,
                2f);
            BeamWidthRatioRange = new Vector2(
                widthRatioMinimum,
                widthRatioMaximum);
            BeamIntensityVariation = Mathf.Clamp(
                beamIntensityVariation,
                0f,
                0.75f);
            BeamEdgeSoftness = Mathf.Clamp(
                beamEdgeSoftness,
                0.01f,
                1f);
            BeamSoftnessVariation = Mathf.Clamp(
                beamSoftnessVariation,
                0f,
                0.75f);
            UpperFade = Mathf.Clamp(upperFade, 0.001f, 0.49f);
            GroundFade = Mathf.Clamp(groundFade, 0.001f, 0.49f);
            ContactPlaneOpacity = Mathf.Clamp01(contactPlaneOpacity);
            ColourMultiplier = colourMultiplier;
            WarmthContribution = Mathf.Clamp01(warmthContribution);
            AtmosphericIntensity = Mathf.Max(0f, atmosphericIntensity);
            SofteningStrength = Mathf.Clamp01(softeningStrength);
            CameraIntersectionFade = Mathf.Clamp01(
                cameraIntersectionFade);
            SurfaceSpotLightIntensity = Mathf.Clamp01(
                surfaceSpotLightIntensity);
            ScreenSpaceSurfaceIntensity = Mathf.Clamp01(
                screenSpaceSurfaceIntensity);
            FootprintEdgeSoftness = Mathf.Clamp01(
                footprintEdgeSoftness);
            EvolutionPreset = evolutionPreset;
            EvolutionStrength = Mathf.Clamp01(evolutionStrength);
            EvolutionSpeed = Mathf.Clamp01(evolutionSpeed);
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
        public readonly uint EvolutionCurrentSeed;
        public readonly uint EvolutionNextSeed;
        public readonly float EvolutionBlend;
        public readonly float EvolutionDurationSeconds;
        public readonly int CompletedEvolutionTransitions;

        public WeatherLightRaySnapshot(
            WeatherLightRayHandle handle,
            WeatherLightRayDescriptor descriptor,
            WeatherLightRayLifecycleState lifecycleState,
            Vector3 baseCentreWorld,
            Vector3 rayDirectionWorld,
            double spawnTime,
            double holdOrExpiryTime,
            float currentIntensity,
            float currentCloudTransmission,
            uint evolutionCurrentSeed,
            uint evolutionNextSeed,
            float evolutionBlend,
            float evolutionDurationSeconds,
            int completedEvolutionTransitions)
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
            EvolutionCurrentSeed = evolutionCurrentSeed == 0u ? 1u : evolutionCurrentSeed;
            EvolutionNextSeed = evolutionNextSeed == 0u ? EvolutionCurrentSeed : evolutionNextSeed;
            EvolutionBlend = Mathf.Clamp01(evolutionBlend);
            EvolutionDurationSeconds = Mathf.Max(0f, evolutionDurationSeconds);
            CompletedEvolutionTransitions = Mathf.Max(0, completedEvolutionTransitions);
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
        public int BeamCount => Descriptor.BeamCount;
        public float AreaDiameterMetres =>
            Descriptor.AreaDiameterMetres;
        public float BeamPitchMetres =>
            Descriptor.BeamPitchMetres;
        public Vector2 BeamWidthRatioRange =>
            Descriptor.BeamWidthRatioRange;
        public float FootprintRadiusMetres =>
            Descriptor.FootprintRadiusMetres;
        public float VisualIntensityMultiplier =>
            Descriptor.AtmosphericIntensity;
        public float SurfaceSpotLightIntensity =>
            Descriptor.SurfaceSpotLightIntensity;
        public float ScreenSpaceSurfaceIntensity =>
            Descriptor.ScreenSpaceSurfaceIntensity;
        public Color ColourMultiplier => Descriptor.ColourMultiplier;
        public float WarmthContribution => Descriptor.WarmthContribution;
        public float EdgeSoftness => Descriptor.FootprintEdgeSoftness;
        public WeatherLightRayEvolutionPreset EvolutionPreset =>
            Descriptor.EvolutionPreset;
        public float EvolutionStrength => Descriptor.EvolutionStrength;
        public float EvolutionSpeed => Descriptor.EvolutionSpeed;
        public float FadeInDuration => Descriptor.FadeInDuration;
        public float FadeOutDuration => Descriptor.FadeOutDuration;
        public int GameplayChannel => Descriptor.GameplayChannel;
        public uint VariationSeed => Descriptor.VariationSeed;
    }
}
