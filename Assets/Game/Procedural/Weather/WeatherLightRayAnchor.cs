using UnityEngine;
using UnityEngine.Serialization;

namespace ProgrammaticStylized3D.Weather
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("PS3D/Weather/Weather LightRay Anchor")]
    public sealed class WeatherLightRayAnchor : MonoBehaviour
    {
        private const int CurrentIntensityDefaultsVersion = 2;
        private const int CurrentSurfaceControlsVersion = 2;
        private const float DefaultSurfaceSpotLightIntensity = 0.20f;
        private const float DefaultScreenSpaceSurfaceIntensity = 0f;

        private const float LegacyDefaultAtmosphericIntensity = 0.28f;
        private const float LegacyDefaultGroundLightIntensity = 0.42f;
        private const float LegacyDefaultSurfaceLightIntensity = 0.28f;
        private const float LegacyDefaultCloudCompensationIntensity = 0.45f;

        private const float PreviousDefaultAtmosphericIntensity = 0.12f;
        private const float PreviousDefaultGroundLightIntensity = 0.05f;
        private const float PreviousDefaultSurfaceLightIntensity = 0.08f;
        private const float PreviousDefaultCloudCompensationIntensity = 0.05f;

        private const float DefaultAtmosphericIntensity = 0.09f;
        private const float DefaultGroundLightIntensity = 0.015f;
        private const float DefaultSurfaceLightIntensity = 0.025f;
        private const float DefaultCloudCompensationIntensity = 0.01f;

        [SerializeField, HideInInspector]
        private int intensityDefaultsVersion;

        [SerializeField, HideInInspector]
        private int surfaceControlsVersion;

        [Header("Binding and Policy")]
        [SerializeField]
        private WeatherLightRayController controllerOverride;

        [SerializeField]
        private WeatherLightRayPreset presetOverride;

        [SerializeField]
        private bool previewInEditMode = true;

        [SerializeField]
        private WeatherLightRaySourceKind sourceKind =
            WeatherLightRaySourceKind.Sun;

        [SerializeField]
        private WeatherLightRayCloudPolicy cloudPolicy =
            WeatherLightRayCloudPolicy.IgnoreClouds;

        [SerializeField]
        private WeatherLightRaySourceGatePolicy sourceGatePolicy =
            WeatherLightRaySourceGatePolicy.RequireActiveSource;

        [Header("Lifecycle")]
        [SerializeField]
        private WeatherLightRayLifetimePolicy lifetimePolicy =
            WeatherLightRayLifetimePolicy.Permanent;

        [SerializeField, Min(0f)]
        private float fadeInDurationSeconds = 1.1f;

        [SerializeField, Min(0f)]
        private float holdDurationSeconds = 5f;

        [SerializeField, Min(0f)]
        private float fadeOutDurationSeconds = 1.4f;

        [SerializeField]
        private bool externallyControlledVisible = true;

        [Header("Continuous Beam Shape")]
        [SerializeField, Min(0.5f)]
        private float heightMetres = 18f;

        [SerializeField, Range(0f, 75f)]
        private float maximumVisualLeanDegrees = 25f;

        [SerializeField, Min(
            WeatherLightRayAreaLayout.MinimumDiameterMetres)]
        private float areaDiameterMetres = 4.8f;

        [SerializeField]
        private Vector2 beamWidthRatioRange = new Vector2(1f, 1.25f);

        [SerializeField, HideInInspector,
            FormerlySerializedAs("beamCount"),
            FormerlySerializedAs("strandCount")]
        private int legacyBeamCount = 5;

        [SerializeField, HideInInspector,
            FormerlySerializedAs("beamWidthRangeMetres")]
        private Vector2 legacyBeamWidthRangeMetres =
            new Vector2(0.45f, 0.75f);

        [SerializeField, Range(
            WeatherLightRayAreaLayout.MinimumBeamSpacingMetres,
            WeatherLightRayAreaLayout.MaximumBeamSpacingMetres),
            FormerlySerializedAs("legacyBeamSpacingMetres")]
        private float beamSpacingMetres =
            WeatherLightRayAreaLayout.DefaultBeamSpacingMetres;

        [SerializeField, HideInInspector,
            FormerlySerializedAs("beamPacking")]
        private float legacyBeamPacking = 0.3f;

        [SerializeField, Range(0f, 0.75f)]
        private float beamIntensityVariation = 0.18f;

        [SerializeField, Range(0.01f, 1f)]
        private float beamEdgeSoftness = 0.55f;

        [SerializeField, Range(0f, 0.75f)]
        private float beamSoftnessVariation = 0.35f;

        [SerializeField, Range(0.001f, 0.49f)]
        private float upperFade = 0.1f;

        [SerializeField, Range(0.001f, 0.49f)]
        private float groundFade = 0.12f;

        [SerializeField, Range(0f, 1f)]
        private float contactPlaneOpacity = 0.35f;

        [Header("Atmospheric Appearance")]
        [SerializeField]
        [ColorUsage(false, true)]
        private Color colourMultiplier = Color.white;

        [SerializeField, Range(0f, 1f)]
        private float warmthContribution = 0.35f;

        [SerializeField, FormerlySerializedAs("shaftIntensity"), Min(0f)]
        private float atmosphericIntensity = DefaultAtmosphericIntensity;

        [SerializeField, FormerlySerializedAs("scatterSoftness"), Range(0f, 1f)]
        private float softeningStrength = 0.35f;

        [SerializeField, Range(0f, 1f)]
        private float cameraIntersectionFade = 0.9f;

        [Header("Surface Illumination")]
        [SerializeField, Range(0f, 1f)]
        private float surfaceSpotLightIntensity =
            DefaultSurfaceSpotLightIntensity;

        [SerializeField, FormerlySerializedAs("surfaceIlluminationIntensity"),
            Range(0f, 1f)]
        private float screenSpaceSurfaceIntensity =
            DefaultScreenSpaceSurfaceIntensity;

        [SerializeField, HideInInspector]
        private float groundLightIntensity = DefaultGroundLightIntensity;

        [SerializeField, HideInInspector]
        private float surfaceLightIntensity = DefaultSurfaceLightIntensity;

        [SerializeField, HideInInspector]
        private float cloudCompensationIntensity =
            DefaultCloudCompensationIntensity;

        [SerializeField, HideInInspector,
            FormerlySerializedAs("footprintRadiusMetres")]
        private float legacyFootprintRadiusMetres = 2.4f;

        [SerializeField, Range(0f, 1f)]
        private float edgeSoftness = 0.42f;

        [SerializeField, HideInInspector, FormerlySerializedAs("footprintIrregularity")]
        private float legacyFootprintIrregularity = 0.2f;

        [SerializeField, HideInInspector]
        private float coreEmphasis = 0.2f;

        [Header("Seeded Beam Evolution")]
        [SerializeField]
        private bool overrideControllerEvolution;

        [SerializeField]
        private WeatherLightRayEvolutionPreset evolutionPreset =
            WeatherLightRayEvolutionPreset.Subtle;

        [SerializeField, Range(0f, 1f)]
        private float evolutionStrength = 0.35f;

        [SerializeField, Range(0f, 1f)]
        private float evolutionSpeed = 0.25f;

        [SerializeField, Min(1)]
        private int variationSeed = 7319;

        [Header("Local Instance Overrides")]
        [SerializeField, Min(0f)]
        private float localIntensityMultiplier = 1f;

        [SerializeField]
        private bool overridePresetBeamSpacing;

        private WeatherLightRayController registeredController;
        private WeatherLightRayHandle handle;
        private string lastError = string.Empty;
        private uint lifecycleRevision = 1u;

        public WeatherLightRayController ControllerOverride =>
            controllerOverride;
        public WeatherLightRayPreset PresetOverride => presetOverride;
        public bool PreviewInEditMode => previewInEditMode;
        public WeatherLightRaySourceKind SourceKind => sourceKind;
        public WeatherLightRayCloudPolicy CloudPolicy => cloudPolicy;
        public WeatherLightRaySourceGatePolicy SourceGatePolicy =>
            sourceGatePolicy;
        public WeatherLightRayLifetimePolicy LifetimePolicy =>
            lifetimePolicy;
        public bool ExternallyControlledVisible =>
            externallyControlledVisible;
        public uint LifecycleRevision => lifecycleRevision;
        public float HeightMetres => heightMetres;
        public float MaximumVisualLeanDegrees =>
            Mathf.Clamp(maximumVisualLeanDegrees, 0f, 75f);
        public float AreaDiameterMetres => areaDiameterMetres;
        public float BeamSpacingMetres => beamSpacingMetres;
        public bool OverridePresetBeamSpacing => overridePresetBeamSpacing;
        public float LocalIntensityMultiplier => Mathf.Max(0f, localIntensityMultiplier);
        public WeatherLightRayAreaLayout AreaLayout =>
            WeatherLightRayAreaLayout.Calculate(
                areaDiameterMetres,
                beamSpacingMetres);
        public int BeamCount => AreaLayout.BeamCount;
        public float BeamPitchMetres => AreaLayout.BeamPitchMetres;
        public Vector2 BeamWidthRatioRange => beamWidthRatioRange;
        public float FootprintRadiusMetres => AreaLayout.RadiusMetres;
        public Color ColourMultiplier => colourMultiplier;
        public float WarmthContribution => warmthContribution;
        public float AtmosphericIntensity => atmosphericIntensity;
        public float SurfaceSpotLightIntensity =>
            surfaceSpotLightIntensity;
        public float ScreenSpaceSurfaceIntensity =>
            screenSpaceSurfaceIntensity;
        public float FadeInDurationSeconds => fadeInDurationSeconds;
        public float HoldDurationSeconds => holdDurationSeconds;
        public float FadeOutDurationSeconds => fadeOutDurationSeconds;
        public bool OverrideControllerEvolution => overrideControllerEvolution;
        public WeatherLightRayEvolutionPreset EvolutionPreset => evolutionPreset;
        public float EvolutionStrength => ResolveEvolutionStrength();
        public float EvolutionSpeed => ResolveEvolutionSpeed();
        public uint VariationSeed => (uint)Mathf.Max(1, variationSeed);
        public WeatherLightRayController RegisteredController =>
            registeredController;
        public WeatherLightRayHandle Handle => handle;
        public string LastError => lastError;

        private void OnEnable()
        {
            MigrateIntensityDefaults();
            MigrateSurfaceControls();
            RefreshRegistration();
        }

        private void OnDisable()
        {
            ReleaseRegistration();
        }

        private void OnDestroy()
        {
            ReleaseRegistration();
        }

        private void OnValidate()
        {
            MigrateIntensityDefaults();
            MigrateSurfaceControls();

            heightMetres = Mathf.Max(0.5f, heightMetres);
            maximumVisualLeanDegrees = Mathf.Clamp(
                maximumVisualLeanDegrees,
                0f,
                75f);
            areaDiameterMetres = Mathf.Max(
                WeatherLightRayAreaLayout.MinimumDiameterMetres,
                !float.IsNaN(areaDiameterMetres) &&
                    !float.IsInfinity(areaDiameterMetres)
                    ? areaDiameterMetres
                    : WeatherLightRayAreaLayout.MinimumDiameterMetres);
            beamWidthRatioRange = NormalizeRange(
                beamWidthRatioRange,
                1f,
                2f);
            legacyBeamCount = Mathf.Clamp(legacyBeamCount, 2, 12);
            legacyBeamWidthRangeMetres = NormalizeRange(
                legacyBeamWidthRangeMetres,
                0.05f,
                4f);
            beamSpacingMetres = Mathf.Clamp(
                !float.IsNaN(beamSpacingMetres) &&
                    !float.IsInfinity(beamSpacingMetres)
                    ? beamSpacingMetres
                    : WeatherLightRayAreaLayout.DefaultBeamSpacingMetres,
                WeatherLightRayAreaLayout.MinimumBeamSpacingMetres,
                WeatherLightRayAreaLayout.MaximumBeamSpacingMetres);
            legacyBeamPacking = Mathf.Clamp01(legacyBeamPacking);
            beamIntensityVariation = Mathf.Clamp(
                beamIntensityVariation,
                0f,
                0.75f);
            beamEdgeSoftness = Mathf.Clamp(
                beamEdgeSoftness,
                0.01f,
                1f);
            beamSoftnessVariation = Mathf.Clamp(
                beamSoftnessVariation,
                0f,
                0.75f);
            upperFade = Mathf.Clamp(upperFade, 0.001f, 0.49f);
            groundFade = Mathf.Clamp(groundFade, 0.001f, 0.49f);
            contactPlaneOpacity = Mathf.Clamp01(contactPlaneOpacity);
            warmthContribution = Mathf.Clamp01(warmthContribution);
            atmosphericIntensity = Mathf.Max(0f, atmosphericIntensity);
            softeningStrength = Mathf.Clamp01(softeningStrength);
            cameraIntersectionFade = Mathf.Clamp01(
                cameraIntersectionFade);
            surfaceSpotLightIntensity = Mathf.Clamp01(
                surfaceSpotLightIntensity);
            screenSpaceSurfaceIntensity = Mathf.Clamp01(
                screenSpaceSurfaceIntensity);
            groundLightIntensity = Mathf.Max(
                0f,
                groundLightIntensity);
            surfaceLightIntensity = Mathf.Max(
                0f,
                surfaceLightIntensity);
            cloudCompensationIntensity = Mathf.Max(
                0f,
                cloudCompensationIntensity);
            legacyFootprintRadiusMetres = Mathf.Clamp(
                legacyFootprintRadiusMetres,
                0.1f,
                20f);
            edgeSoftness = Mathf.Clamp01(edgeSoftness);
            legacyFootprintIrregularity = Mathf.Clamp01(
                legacyFootprintIrregularity);
            coreEmphasis = Mathf.Max(0f, coreEmphasis);
            evolutionStrength = Mathf.Clamp01(evolutionStrength);
            evolutionSpeed = Mathf.Clamp01(evolutionSpeed);
            fadeInDurationSeconds = Mathf.Max(
                0f,
                fadeInDurationSeconds);
            holdDurationSeconds = Mathf.Max(
                0f,
                holdDurationSeconds);
            fadeOutDurationSeconds = Mathf.Max(
                0f,
                fadeOutDurationSeconds);
            variationSeed = Mathf.Max(1, variationSeed);
            localIntensityMultiplier = Mathf.Max(0f, localIntensityMultiplier);

            if (lifetimePolicy == WeatherLightRayLifetimePolicy.Timed)
            {
                lifecycleRevision = NextRevision(lifecycleRevision);
            }

            if (isActiveAndEnabled)
            {
                RefreshRegistration();
            }
        }

        private void Update()
        {
            if (Application.isPlaying || previewInEditMode)
            {
                RefreshRegistration();
            }
        }

        /// <summary>
        /// Builds request-local state only. Shared visual presentation is
        /// applied by the Controller from the ray's resolved preset before
        /// the descriptor may become active. Legacy serialized appearance
        /// fields remain on the component solely for deferred migration and
        /// are deliberately not consulted here.
        /// </summary>
        public WeatherLightRayDescriptor BuildLocalDescriptor()
        {
            return new WeatherLightRayDescriptor(
                sourceKind,
                WeatherLightRayOriginKind.Authored,
                cloudPolicy,
                lifetimePolicy,
                sourceGatePolicy,
                WeatherLightRayMovementPolicy.Static,
                heightMetres,
                maximumVisualLeanDegrees,
                areaDiameterMetres,
                beamSpacingMetres,
                Vector2.one,
                0f,
                0.5f,
                0f,
                0.1f,
                0.1f,
                0f,
                Color.white,
                0f,
                1f,
                0f,
                0f,
                0f,
                0f,
                1f,
                0f,
                0f,
                0.5f,
                WeatherLightRayEvolutionPreset.Static,
                0f,
                0f,
                fadeInDurationSeconds,
                holdDurationSeconds,
                fadeOutDurationSeconds,
                0,
                VariationSeed);
        }

        private float ResolveEvolutionStrength()
        {
            switch (evolutionPreset)
            {
                case WeatherLightRayEvolutionPreset.Static:
                    return 0f;
                case WeatherLightRayEvolutionPreset.Subtle:
                    return 0.35f;
                case WeatherLightRayEvolutionPreset.Living:
                    return 0.65f;
                default:
                    return Mathf.Clamp01(evolutionStrength);
            }
        }

        private float ResolveEvolutionSpeed()
        {
            switch (evolutionPreset)
            {
                case WeatherLightRayEvolutionPreset.Static:
                    return 0f;
                case WeatherLightRayEvolutionPreset.Subtle:
                    return 0.25f;
                case WeatherLightRayEvolutionPreset.Living:
                    return 0.50f;
                default:
                    return Mathf.Clamp01(evolutionSpeed);
            }
        }

        private void MigrateIntensityDefaults()
        {
            if (intensityDefaultsVersion >= CurrentIntensityDefaultsVersion)
            {
                return;
            }

            atmosphericIntensity = MigrateKnownDefault(
                atmosphericIntensity,
                LegacyDefaultAtmosphericIntensity,
                PreviousDefaultAtmosphericIntensity,
                DefaultAtmosphericIntensity);
            groundLightIntensity = MigrateKnownDefault(
                groundLightIntensity,
                LegacyDefaultGroundLightIntensity,
                PreviousDefaultGroundLightIntensity,
                DefaultGroundLightIntensity);
            surfaceLightIntensity = MigrateKnownDefault(
                surfaceLightIntensity,
                LegacyDefaultSurfaceLightIntensity,
                PreviousDefaultSurfaceLightIntensity,
                DefaultSurfaceLightIntensity);
            cloudCompensationIntensity = MigrateKnownDefault(
                cloudCompensationIntensity,
                LegacyDefaultCloudCompensationIntensity,
                PreviousDefaultCloudCompensationIntensity,
                DefaultCloudCompensationIntensity);

            intensityDefaultsVersion = CurrentIntensityDefaultsVersion;
        }


        private void MigrateSurfaceControls()
        {
            if (surfaceControlsVersion >= CurrentSurfaceControlsVersion)
            {
                return;
            }

            // AF4 explicitly disables the former post-composite surface path
            // by default and promotes one real URP Spot Light as the primary
            // material-lighting response. The migration intentionally resets
            // every pre-AF4 complement value to zero as approved by the user.
            surfaceSpotLightIntensity = DefaultSurfaceSpotLightIntensity;
            screenSpaceSurfaceIntensity =
                DefaultScreenSpaceSurfaceIntensity;
            surfaceControlsVersion = CurrentSurfaceControlsVersion;
        }

        private static float MigrateKnownDefault(
            float currentValue,
            float legacyDefault,
            float previousDefault,
            float currentDefault)
        {
            return Mathf.Approximately(currentValue, legacyDefault) ||
                Mathf.Approximately(currentValue, previousDefault)
                    ? currentDefault
                    : currentValue;
        }

        public void RestartTimedLifecycle()
        {
            lifecycleRevision = NextRevision(lifecycleRevision);
            RefreshRegistration();
        }

        public void SetExternallyControlledVisible(bool visible)
        {
            if (externallyControlledVisible == visible)
            {
                return;
            }

            externallyControlledVisible = visible;
            RefreshRegistration();
        }

        public void RefreshRegistration()
        {
            WeatherLightRayController desiredController =
                controllerOverride != null
                    ? controllerOverride
                    : WeatherLightRayController.PublishedController;

            if (registeredController != desiredController)
            {
                ReleaseRegistration();
                registeredController = desiredController;
            }

            if (registeredController == null)
            {
                handle = default;
                lastError =
                    "No published Weather LightRay Controller is available.";
                return;
            }

            if (!registeredController.TryRegisterOrUpdateAuthoredRay(
                    this,
                    ref handle,
                    out string error))
            {
                lastError = error;
                return;
            }

            lastError = string.Empty;
        }

        private void ReleaseRegistration()
        {
            if (registeredController != null && handle.IsValid)
            {
                registeredController.ReleaseAuthoredRay(this, handle);
            }

            registeredController = null;
            handle = default;
        }

        private static Vector2 NormalizeRange(
            Vector2 range,
            float minimum,
            float maximum)
        {
            float min = Mathf.Clamp(
                Mathf.Min(range.x, range.y),
                minimum,
                maximum);
            float max = Mathf.Clamp(
                Mathf.Max(range.x, range.y),
                min,
                maximum);
            return new Vector2(min, max);
        }

        private static uint NextRevision(uint revision)
        {
            revision++;
            return revision == 0u ? 1u : revision;
        }
    }
}
