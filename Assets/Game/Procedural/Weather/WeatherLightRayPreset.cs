using UnityEngine;

namespace ProgrammaticStylized3D.Weather
{
    /// <summary>
    /// WEATHER LIGHTRAY VISUAL-PRESET CONTRACT — APPEARANCE ONLY.
    ///
    /// This asset owns beam, surface, vegetation, geometry, and evolution
    /// presentation. It must never decide when it is active, which runtime
    /// source is required, whether clouds are required, or how automatic rays
    /// are populated. Those policies belong to the runtime request owner and,
    /// eventually, the Weather orchestration layer. The legacy SourceKind field
    /// is retained only for serialized compatibility until curated asset
    /// migration and must not be read by production request or population code.
    /// </summary>
    [CreateAssetMenu(
        fileName = "WeatherLightRayPreset",
        menuName = "PS3D/Weather/LightRay Preset")]
    public sealed class WeatherLightRayPreset : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string displayName = "LightRay Preset";
        [SerializeField]
        [Tooltip(
            "Legacy serialized metadata only. Runtime request owners and " +
            "Weather orchestration must supply source dependencies explicitly.")]
        private WeatherLightRaySourceKind sourceKind =
            WeatherLightRaySourceKind.Sun;

        [Header("Shared Atmospheric Presentation")]
        [SerializeField] private Color colourMultiplier = Color.white;
        [SerializeField, Range(0f, 1f)] private float warmthContribution = 0.5f;
        [SerializeField, Min(0f)] private float atmosphericIntensity = 0.2f;
        [SerializeField, Range(0f, 1f)] private float softeningStrength = 0.55f;
        [SerializeField, Range(0f, 1f)] private float cameraIntersectionFade = 0.92f;
        [SerializeField, Range(0f, 1f)] private float screenSpaceSurfaceIntensity;

        [Header("Beam Composition")]
        [SerializeField, Range(
            WeatherLightRayAreaLayout.MinimumBeamSpacingMetres,
            WeatherLightRayAreaLayout.MaximumBeamSpacingMetres)]
        private float beamSpacingMetres = WeatherLightRayAreaLayout.DefaultBeamSpacingMetres;
        [SerializeField] private Vector2 beamWidthRatioRange = new Vector2(1f, 1.25f);
        [SerializeField, Range(0f, 0.75f)] private float beamIntensityVariation = 0.6f;
        [SerializeField, Range(0.01f, 1f)] private float beamEdgeSoftness = 0.45f;
        [SerializeField, Range(0f, 0.75f)] private float beamSoftnessVariation = 0.45f;
        [SerializeField, Range(0.001f, 0.49f)] private float upperFade = 0.49f;
        [SerializeField, Range(0.001f, 0.49f)] private float groundFade = 0.2f;
        [SerializeField, Range(0f, 1f)] private float contactPlaneOpacity = 0.15f;

        [Header("Surface Response")]
        [SerializeField, Range(0f, 1f)] private float surfaceSpotLightIntensity = 0.4f;
        [SerializeField, Range(0f, 1f)] private float footprintEdgeSoftness = 1f;
        [SerializeField, Range(0f, 1f)] private float accentLineIntensity = 0.4f;
        [SerializeField, Range(0f, 1f)] private float vegetationAccentCoverage = 0.3f;
        [SerializeField, Range(0f, 1f)] private float vegetationAccentSoftness = 0.5f;

        [Header("Evolution")]
        [SerializeField] private WeatherLightRayEvolutionPreset evolutionPreset = WeatherLightRayEvolutionPreset.Subtle;
        [SerializeField, Range(0f, 1f)] private float evolutionStrength = 0.35f;
        [SerializeField, Range(0f, 1f)] private float evolutionSpeed = 0.25f;

        [Header("Default Spawn Geometry")]
        [SerializeField, Min(0.001f)] private float defaultHeightMetres = 27.7f;
        [SerializeField, Range(0f, 75f)] private float defaultMaximumVisualLeanDegrees = 25.2f;
        [SerializeField, Min(WeatherLightRayAreaLayout.MinimumDiameterMetres)]
        private float defaultAreaDiameterMetres = 4.91f;


        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        /// <summary>
        /// Legacy serialized metadata. Do not use for runtime selection,
        /// source gating, or automatic population.
        /// </summary>
        public WeatherLightRaySourceKind SourceKind => sourceKind;
        public Color ColourMultiplier => colourMultiplier;
        public float WarmthContribution => Mathf.Clamp01(warmthContribution);
        public float AtmosphericIntensity => Mathf.Max(0f, atmosphericIntensity);
        public float SofteningStrength => Mathf.Clamp01(softeningStrength);
        public float CameraIntersectionFade => Mathf.Clamp01(cameraIntersectionFade);
        public float ScreenSpaceSurfaceIntensity => Mathf.Clamp01(screenSpaceSurfaceIntensity);
        public float BeamSpacingMetres => Mathf.Clamp(beamSpacingMetres, WeatherLightRayAreaLayout.MinimumBeamSpacingMetres, WeatherLightRayAreaLayout.MaximumBeamSpacingMetres);
        public Vector2 BeamWidthRatioRange => beamWidthRatioRange;
        public float BeamIntensityVariation => Mathf.Clamp(beamIntensityVariation, 0f, 0.75f);
        public float BeamEdgeSoftness => Mathf.Clamp(beamEdgeSoftness, 0.01f, 1f);
        public float BeamSoftnessVariation => Mathf.Clamp(beamSoftnessVariation, 0f, 0.75f);
        public float UpperFade => Mathf.Clamp(upperFade, 0.001f, 0.49f);
        public float GroundFade => Mathf.Clamp(groundFade, 0.001f, 0.49f);
        public float ContactPlaneOpacity => Mathf.Clamp01(contactPlaneOpacity);
        public float SurfaceSpotLightIntensity => Mathf.Clamp01(surfaceSpotLightIntensity);
        public float FootprintEdgeSoftness => Mathf.Clamp01(footprintEdgeSoftness);
        public float AccentLineIntensity => Mathf.Clamp01(accentLineIntensity);
        public float VegetationAccentCoverage => Mathf.Clamp01(vegetationAccentCoverage);
        public float VegetationAccentSoftness => Mathf.Clamp01(vegetationAccentSoftness);
        public WeatherLightRayEvolutionPreset EvolutionPreset => evolutionPreset;
        public float EvolutionStrength => ResolveEvolutionStrength(evolutionPreset, evolutionStrength);
        public float EvolutionSpeed => ResolveEvolutionSpeed(evolutionPreset, evolutionSpeed);
        public float DefaultHeightMetres => Mathf.Max(0.001f, defaultHeightMetres);
        public float DefaultMaximumVisualLeanDegrees => Mathf.Clamp(defaultMaximumVisualLeanDegrees, 0f, 75f);
        public float DefaultAreaDiameterMetres => Mathf.Max(WeatherLightRayAreaLayout.MinimumDiameterMetres, defaultAreaDiameterMetres);

        public WeatherLightRayDescriptor ApplyTo(
            in WeatherLightRayDescriptor localDescriptor,
            bool overrideBeamSpacing,
            float localBeamSpacingMetres,
            float localIntensityMultiplier,
            WeatherLightRayPreset previousPresentationPreset = null,
            float presentationBlend = 1f)
        {
            float blend = Mathf.Clamp01(presentationBlend);
            Color resolvedColour = previousPresentationPreset != null
                ? Color.Lerp(previousPresentationPreset.ColourMultiplier, ColourMultiplier, blend)
                : ColourMultiplier;
            float resolvedWarmth = previousPresentationPreset != null
                ? Mathf.Lerp(previousPresentationPreset.WarmthContribution, WarmthContribution, blend)
                : WarmthContribution;
            float resolvedAtmosphericIntensity = previousPresentationPreset != null
                ? Mathf.Lerp(previousPresentationPreset.AtmosphericIntensity, AtmosphericIntensity, blend)
                : AtmosphericIntensity;
            float resolvedSoftening = previousPresentationPreset != null
                ? Mathf.Lerp(previousPresentationPreset.SofteningStrength, SofteningStrength, blend)
                : SofteningStrength;
            float resolvedCameraFade = previousPresentationPreset != null
                ? Mathf.Lerp(previousPresentationPreset.CameraIntersectionFade, CameraIntersectionFade, blend)
                : CameraIntersectionFade;
            float resolvedSurfaceSpot = previousPresentationPreset != null
                ? Mathf.Lerp(previousPresentationPreset.SurfaceSpotLightIntensity, SurfaceSpotLightIntensity, blend)
                : SurfaceSpotLightIntensity;
            float resolvedScreenSurface = previousPresentationPreset != null
                ? Mathf.Lerp(previousPresentationPreset.ScreenSpaceSurfaceIntensity, ScreenSpaceSurfaceIntensity, blend)
                : ScreenSpaceSurfaceIntensity;
            float resolvedFootprintSoftness = previousPresentationPreset != null
                ? Mathf.Lerp(previousPresentationPreset.FootprintEdgeSoftness, FootprintEdgeSoftness, blend)
                : FootprintEdgeSoftness;
            return new WeatherLightRayDescriptor(
                localDescriptor.SourceKind,
                localDescriptor.OriginKind,
                localDescriptor.CloudPolicy,
                localDescriptor.LifetimePolicy,
                localDescriptor.SourceGatePolicy,
                localDescriptor.MovementPolicy,
                localDescriptor.Height,
                localDescriptor.MaximumVisualLeanDegrees,
                localDescriptor.AreaDiameterMetres,
                overrideBeamSpacing ? localBeamSpacingMetres : BeamSpacingMetres,
                BeamWidthRatioRange,
                BeamIntensityVariation,
                BeamEdgeSoftness,
                BeamSoftnessVariation,
                UpperFade,
                GroundFade,
                ContactPlaneOpacity,
                resolvedColour,
                resolvedWarmth,
                resolvedAtmosphericIntensity * Mathf.Max(0f, localIntensityMultiplier),
                resolvedSoftening,
                resolvedCameraFade,
                resolvedSurfaceSpot,
                resolvedScreenSurface,
                resolvedFootprintSoftness,
                EvolutionPreset,
                EvolutionStrength,
                EvolutionSpeed,
                localDescriptor.FadeInDuration,
                localDescriptor.HoldDuration,
                localDescriptor.FadeOutDuration,
                localDescriptor.GameplayChannel,
                localDescriptor.VariationSeed);
        }

        private static float ResolveEvolutionStrength(WeatherLightRayEvolutionPreset preset, float custom)
        {
            return preset switch
            {
                WeatherLightRayEvolutionPreset.Static => 0f,
                WeatherLightRayEvolutionPreset.Subtle => 0.35f,
                WeatherLightRayEvolutionPreset.Living => 0.65f,
                _ => Mathf.Clamp01(custom)
            };
        }

        private static float ResolveEvolutionSpeed(WeatherLightRayEvolutionPreset preset, float custom)
        {
            return preset switch
            {
                WeatherLightRayEvolutionPreset.Static => 0f,
                WeatherLightRayEvolutionPreset.Subtle => 0.25f,
                WeatherLightRayEvolutionPreset.Living => 0.5f,
                _ => Mathf.Clamp01(custom)
            };
        }
    }
}
