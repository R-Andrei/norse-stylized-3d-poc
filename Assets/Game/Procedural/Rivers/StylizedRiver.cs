using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.Splines;
using ProgrammaticStylized3D.Geometry.Ground;

namespace ProgrammaticStylized3D.Rivers
{
    public enum StylizedRiverQuality
    {
        Low,
        Medium,
        High
    }

    public enum StylizedRiverWaterBodyPreset
    {
        ClearStream,
        BalancedRiver,
        DeepWater,
        Custom
    }

    public enum StylizedRiverChannelCharacterPreset
    {
        Engineered,
        SmoothNatural,
        Irregular,
        Rugged,
        Custom
    }

    public enum StylizedRiverSurfaceState
    {
        Liquid,
        Frozen,
        Custom
    }

    public enum StylizedRiverMotionPreset
    {
        Still,
        Calm,
        Flowing,
        Furious,
        Custom
    }

    public enum StylizedRiverMotionDebugView
    {
        Final = 0,
        BankMask = 1,
        MacroHeight = 2,
        SurfaceNormal = 3,
        CurrentAccent = 4,
        LiquidFactor = 5
    }

    public enum StylizedRiverRefractionPreset
    {
        None,
        Clear,
        Balanced,
        Distorted,
        Custom
    }

    public enum StylizedRiverRefractionDebugView
    {
        Final = 0,
        RefractedScene = 1,
        Offset = 2,
        DepthInfluence = 3,
        ShoreMask = 4,
        SampleValidity = 5,
        IceDiffusion = 6
    }

    public enum StylizedRiverDisturbancePreset
    {
        None,
        Subtle,
        Balanced,
        Reactive,
        Custom
    }

    public enum StylizedRiverDisturbanceDebugView
    {
        Final = 0,
        Height = 1,
        Velocity = 2,
        Normal = 3,
        Intensity = 4,
        FieldCoordinates = 5,
        StaticPressureTarget = 6,
        StaticWakeSource = 7,
        WakeEnergy = 8,
        FinalWakeGeometryHeight = 19
    }

    public enum StylizedRiverIceBodyPreset
    {
        ClearIce,
        CloudyIce,
        DeepBlueIce,
        Custom
    }

    public enum StylizedRiverBodyDebugView
    {
        Final = 0,
        VerticalDepth = 1,
        DepthBlend = 2,
        Transmission = 3,
        BodyCoverage = 4,
        SceneColour = 5,
        DepthValidity = 6,
        SurfaceCoverage = 7,
        CombinedLighting = 8,
        AmbientLighting = 9,
        SunLighting = 10,
        LocalLighting = 11,
        FreezeAmount = 12
    }

    public enum StylizedRiverDebugView
    {
        Final = 0,
        Depth = 1,
        Normals = 2,
        FoamState = 3,
        FinalFoam = 4,
        Refraction = 5,
        PlanarReflection = 6,
        WaveEdgeMask = 7
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SplineContainer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class StylizedRiver : MonoBehaviour
    {
        public const string CompatibleShaderName =
            "PS3D/Stylized River Water";

        public const float MinimumStaticPressureProfileChangeInterval = 0.5f;
        public const float MaximumStaticPressureProfileChangeInterval = 3f;
        public const float DefaultStaticPressureProfileChangeIntervalMin =
            0.75f;
        public const float DefaultStaticPressureProfileChangeIntervalMax =
            1.75f;
        public const float MinimumStaticWakeVariationInterval = 0.5f;
        public const float MaximumStaticWakeVariationInterval = 2f;
        public const float DefaultStaticWakeVariationIntervalMin = 0.6f;
        public const float DefaultStaticWakeVariationIntervalMax = 0.9f;

        private const string LegacyCurrentObjectName =
            "__PS3D_RiverCurrentAccents";

        private const string LegacyStaticFoamObjectName =
            "__PS3D_RiverStaticFoam";

        private const string CorridorObjectName =
            "__PS3D_RiverCorridor";

        private const string BodyShaderResourcePath =
            "PS3DRiver/Shaders/SH_CleanStylizedRiver";

        private const string NormalTextureResourcePath =
            "PS3DRiver/Textures/T_RiverNormal";


        [Header("Setup")]
        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private bool liveRegeneration = true;
        [SerializeField] private bool reverseFlow;

        [Header("River Domain")]
        [Tooltip("World-space metres between authoritative river-domain samples.")]
        [Min(0.05f)]
        [SerializeField] private float domainSampleSpacing = 0.5f;

        [Tooltip("Cumulative distance assigned by a future connected-river assembler.")]
        [SerializeField] private float connectedRiverDistanceOffset;

        [Header("Channel")]
        [Range(0.5f, 20f)]
        [SerializeField] private float width = 4f;

        [Range(0.1f, 12f)]
        [SerializeField] private float bankBlend = 2.5f;

        [Range(0.1f, 6f)]
        [SerializeField] private float depth = 1.1f;

        [Range(0f, 1f)]
        [SerializeField] private float bedFlatness = 0.62f;

        [SerializeField]
        private StylizedRiverBankProfile bankProfile =
            StylizedRiverBankProfile.Natural;

        [FormerlySerializedAs("bankOverlap")]
        [SerializeField, HideInInspector]
        private float legacyManualBankOverlap;

        [Tooltip("Additional hidden shoreline overlap added beyond the corridor generator's safe automatic value. This can only increase the generated overlap.")]
        [Min(0f)]
        [SerializeField] private float additionalShorelineOverlap;

        [Tooltip("Minimum vertical separation maintained between the visible wet surface and generated terrain.")]
        [Range(0.005f, 0.5f)]
        [SerializeField] private float shorelineWetClearance = 0.05f;

        [Tooltip("Minimum terrain cover above the hidden outer edge of the water mesh.")]
        [Range(0.005f, 0.5f)]
        [SerializeField] private float shorelineBankCover = 0.05f;

        [Tooltip("Reserved downward water displacement for later surface-motion stages. Stage 2 should remain at zero.")]
        [Range(0f, 1f)]
        [SerializeField] private float reservedDownwardSurfaceDisplacement;

        [FormerlySerializedAs("carvingStrength")]
        [Range(0f, 1f)]
        [SerializeField] private float terrainConformity = 1f;

        [Header("Natural Channel Variation")]
        [SerializeField]
        private StylizedRiverChannelCharacterPreset channelCharacterPreset =
            StylizedRiverChannelCharacterPreset.Engineered;

        [SerializeField] private int naturalVariationSeed = 1701;

        [Tooltip("Maximum vertical variation applied only to the bottom region of the riverbed, in metres.")]
        [Range(0f, 2f)]
        [SerializeField] private float bedRoughness;

        [Tooltip("Typical physical size of riverbed height features, in metres.")]
        [Range(0.5f, 30f)]
        [SerializeField] private float bedRoughnessScale = 6f;

        [Tooltip("How far upward through the submerged bed profile roughness may extend. Zero preserves floor-only roughness. One reaches the maximum safe lower-slope area while keeping the shoreline band smooth.")]
        [Range(0f, 1f)]
        [SerializeField] private float bedRoughnessReach = 0.5f;

        [Tooltip("Maximum smooth left/right shoreline deviation from the configured width, in metres.")]
        [Range(0f, 4f)]
        [SerializeField] private float shorelineIrregularity;

        [Tooltip("Typical length of shoreline widening and narrowing features, in metres.")]
        [Range(1.5f, 50f)]
        [SerializeField] private float shorelineIrregularityScale = 12f;

        [Tooltip("Zero keeps both banks correlated. One allows the left and right banks to vary independently.")]
        [Range(0f, 1f)]
        [SerializeField] private float bankAsymmetry = 0.5f;

        [Header("Surface Mesh")]
        [SerializeField]
        private StylizedRiverQuality quality =
            StylizedRiverQuality.Medium;

        [Tooltip("Raises water above the carved bed to avoid depth fighting.")]
        [Range(0f, 0.25f)]
        [SerializeField] private float surfaceOffset = 0.035f;

        [Header("Water Body")]
        [SerializeField]
        private StylizedRiverWaterBodyPreset bodyPreset =
            StylizedRiverWaterBodyPreset.BalancedRiver;

        [SerializeField]
        private Color shallowColor =
            new Color(0.458f, 0.802f, 0.798f, 1f);

        [SerializeField]
        private Color deepColor =
            new Color(0f, 0.310f, 0.594f, 1f);

        [Tooltip("How strongly the riverbed remains visible through the water.")]
        [Range(0f, 1f)]
        [SerializeField] private float clarity = 0.62f;

        [Tooltip("World-space vertical depth at which the body reaches its deep-water appearance.")]
        [Range(0.1f, 8f)]
        [SerializeField] private float bodyDepthRange = 1.4f;

        [Tooltip("Controls whether the shallow-to-deep transition is soft or pronounced.")]
        [Range(0f, 1f)]
        [SerializeField] private float bodyDepthContrast = 0.5f;

        [Tooltip("Controls how strongly the water volume colours the scene beneath it.")]
        [Range(0f, 1f)]
        [FormerlySerializedAs("bodyStrength")]
        [SerializeField] private float waterTintStrength = 0.72f;

        [Tooltip("Controls how clearly the air-water boundary remains visible, even in shallow clear water.")]
        [Range(0f, 1f)]
        [SerializeField] private float surfacePresence = 0.46f;

        [Header("Surface State")]
        [SerializeField]
        private StylizedRiverSurfaceState surfaceState =
            StylizedRiverSurfaceState.Liquid;

        [Tooltip("Continuous liquid-to-frozen value used only when Surface State is Custom. Zero is liquid and one is frozen.")]
        [Range(0f, 1f)]
        [SerializeField] private float customFreezeAmount;

        [Header("Frozen Body")]
        [SerializeField]
        private StylizedRiverIceBodyPreset iceBodyPreset =
            StylizedRiverIceBodyPreset.CloudyIce;

        [SerializeField]
        private Color iceColor =
            new Color(0.56f, 0.78f, 0.90f, 1f);

        [Tooltip("How much of the lit scene beneath the ice remains visible.")]
        [Range(0f, 1f)]
        [SerializeField] private float iceTransmission = 0.16f;

        [Tooltip("Optical thickness of the frozen sheet. Higher values make the ice more opaque.")]
        [Range(0f, 1f)]
        [SerializeField] private float iceThickness = 0.72f;

        [Tooltip("How cloudy and internally scattered the ice appears.")]
        [Range(0f, 1f)]
        [SerializeField] private float iceCloudiness = 0.58f;

        [Tooltip("How strongly the frozen air-ice boundary remains visible.")]
        [Range(0f, 1f)]
        [SerializeField] private float iceSurfacePresence = 0.86f;

        [Tooltip("How strongly cloudy ice broadens and brightens its light response.")]
        [Range(0f, 1f)]
        [SerializeField] private float iceScattering = 0.68f;

        [Header("Lighting Response")]
        [Tooltip("Zero keeps authored colours largely fixed. One makes the body fully dependent on actual scene lighting.")]
        [Range(0f, 1f)]
        [SerializeField] private float lightDependence = 1f;

        [Tooltip("Strength of environment and ambient illumination.")]
        [Range(0f, 2f)]
        [SerializeField] private float ambientResponse = 1f;

        [Tooltip("Strength of the main directional sun or moon light.")]
        [Range(0f, 2f)]
        [SerializeField] private float sunResponse = 1f;

        [Tooltip("Strength of point, spot, and additional directional lights.")]
        [Range(0f, 3f)]
        [SerializeField] private float localLightResponse = 1f;

        [Tooltip("Zero uses light brightness only. One allows lights to fully tint the river.")]
        [Range(0f, 1f)]
        [SerializeField] private float lightColorInfluence = 0.80f;

        [Tooltip("Minimum retained body illumination when no meaningful light reaches the river.")]
        [Range(0f, 0.5f)]
        [SerializeField] private float minimumNightVisibility = 0.025f;

        [Tooltip("Master strength for real-time shadowing of the river's intrinsic water or ice contribution.")]
        [Range(0f, 1f)]
        [SerializeField] private float shadowResponse = 1f;

        [Tooltip("How strongly the main-light shadow affects liquid water's intrinsic tint and surface lighting. Low values keep the already-shadowed underwater scene dominant.")]
        [Range(0f, 1f)]
        [SerializeField] private float liquidSurfaceShadowResponse = 0.08f;

        [Tooltip("How strongly the main-light shadow affects the frozen ice body and surface.")]
        [Range(0f, 1f)]
        [SerializeField] private float iceSurfaceShadowResponse = 0.65f;

        [Tooltip("Advanced diffuse wrap used to keep low-angle sun transitions stable.")]
        [Range(0f, 1f)]
        [SerializeField] private float diffuseWrap = 0.22f;

        [Header("Surface Motion")]
        [SerializeField]
        private StylizedRiverMotionPreset motionPreset =
            StylizedRiverMotionPreset.Still;

        [Tooltip("Downstream travel speed in metres per second.")]
        [Range(0f, 12f)]
        [SerializeField] private float flowSpeed;

        [Tooltip("Maximum vertical displacement of the liquid surface, in metres.")]
        [Range(0f, 1.25f)]
        [SerializeField] private float motionWaveHeight;

        [Tooltip("Typical physical length of the displaced macro waves, in metres.")]
        [Range(0.5f, 30f)]
        [SerializeField] private float motionWaveLength = 5f;

        [Tooltip("Controls whether macro waves remain broad or become sharper and more crest-like.")]
        [Range(0f, 1f)]
        [SerializeField] private float motionWaveSteepness = 0.35f;

        [Tooltip("Strength of small flow-aligned normal detail.")]
        [Range(0f, 2f)]
        [SerializeField] private float motionDetailStrength;

        [Tooltip("Typical physical size of surface-detail features, in metres.")]
        [Range(0.15f, 12f)]
        [SerializeField] private float motionDetailScale = 1.4f;

        [Tooltip("How strongly the flow pattern evolves and breaks apart instead of only translating.")]
        [Range(0f, 1f)]
        [SerializeField] private float motionTurbulence = 0.25f;

        [Tooltip("Strength of broad downstream surface modulation. This is not foam.")]
        [Range(0f, 1f)]
        [SerializeField] private float currentAccentStrength;

        [Tooltip("Typical physical length of current accents, in metres.")]
        [Range(0.5f, 30f)]
        [SerializeField] private float currentAccentScale = 5f;

        [Tooltip("Amount of macro displacement retained at the visible shoreline. The hidden overlap still fades safely to zero before the raw mesh edge.")]
        [Range(0f, 1f)]
        [SerializeField] private float shoreMotion = 0.35f;

        [Tooltip("Distance inside the visible shoreline over which full centre-channel motion blends toward Shore Motion.")]
        [Range(0.05f, 5f)]
        [SerializeField] private float shoreMotionWidth = 0.75f;

        [SerializeField]
        private StylizedRiverMotionDebugView motionDebugView =
            StylizedRiverMotionDebugView.Final;

        [Header("Refraction and Optical Distortion")]
        [SerializeField]
        private StylizedRiverRefractionPreset refractionPreset =
            StylizedRiverRefractionPreset.None;

        [Tooltip("Maximum liquid screen-space distortion. Mild values are intentional; excessive refraction easily detaches submerged objects from their real positions.")]
        [Range(0f, 0.02f)]
        [SerializeField] private float liquidRefractionStrength;

        [Tooltip("How strongly actual water depth increases liquid refraction. Zero applies equal strength at all depths; one suppresses distortion in very shallow water.")]
        [Range(0f, 1f)]
        [SerializeField] private float refractionDepthInfluence = 0.55f;

        [Tooltip("How strongly the final Stage 3 surface normal drives optical distortion.")]
        [Range(0f, 1f)]
        [SerializeField] private float refractionNormalInfluence = 0.65f;

        [Tooltip("Amount of liquid refraction retained where water visibly meets the bank. Distortion still fades to zero inside the hidden overlap.")]
        [Range(0f, 1f)]
        [SerializeField] private float shoreRefraction = 0.22f;

        [Tooltip("How aggressively displaced samples are rejected when they cross scene-depth discontinuities such as rocks, banks, and foreground objects.")]
        [Range(0f, 1f)]
        [SerializeField] private float depthEdgeProtection = 0.88f;

        [Tooltip("Reduces refraction only where an object-to-background depth jump would otherwise contract the object silhouette and expose unavailable background information. Uses no additional texture samples.")]
        [SerializeField] private bool preserveObjectSilhouettes = true;

        [Tooltip("Static screen-space warping through fully frozen ice.")]
        [Range(0f, 0.012f)]
        [SerializeField] private float iceDistortionStrength = 0.0015f;

        [Tooltip("Additional quality-scaled softening of the transmitted scene beneath ice. Ice Cloudiness also contributes automatically.")]
        [Range(0f, 1f)]
        [SerializeField] private float iceDiffusion = 0.28f;

        [SerializeField]
        private StylizedRiverRefractionDebugView refractionDebugView =
            StylizedRiverRefractionDebugView.Final;

        [Header("Runtime Disturbance and Interaction")]
        [Tooltip("Enables Stage 5 runtime interactions. When disabled, no disturbance textures are allocated or simulated.")]
        [SerializeField] private bool runtimeDisturbances;

        // Retained for serialized/API compatibility with the earlier combined
        // disturbance preset. It is intentionally hidden because Stage 5 is now
        // authored per feature.
        [HideInInspector]
        [SerializeField]
        private StylizedRiverDisturbancePreset disturbancePreset =
            StylizedRiverDisturbancePreset.Custom;

        [SerializeField]
        private StylizedRiverDisturbanceDebugView disturbanceDebugView =
            StylizedRiverDisturbanceDebugView.Final;

        [Header("Pressure")]
        [Tooltip("Controls the shared Pressure response. The current stationary implementation selects a point between its computed minimum and maximum feasible heights; future dynamic sources will use the same response setting with relative-motion source preparation.")]
        [Range(0f, 1f)]
        [SerializeField] private float staticPressureStrength = 0.65f;

        [Tooltip("Controls how abruptly attached Pressure descends away from its source contact. Higher values produce a steeper contact face without increasing the computed height ceiling.")]
        [Range(0.5f, 4f)]
        [SerializeField] private float staticPressureContactSharpness = 2.8f;

        [Tooltip("Controls the shared Pressure profile-variation envelope. Zero keeps the prepared source profile stable.")]
        [Range(0f, 2f)]
        [SerializeField] private float staticPressureWaveResponse = 1f;

        [Tooltip("Shortest randomized time, in seconds, between lateral pressure-profile changes.")]
        [Range(
            MinimumStaticPressureProfileChangeInterval,
            MaximumStaticPressureProfileChangeInterval)]
        [SerializeField]
        private float staticPressureProfileChangeIntervalMin =
            DefaultStaticPressureProfileChangeIntervalMin;

        [Tooltip("Longest randomized time, in seconds, between lateral pressure-profile changes. The morph duration scales with the selected interval and completes before the next change.")]
        [Range(
            MinimumStaticPressureProfileChangeInterval,
            MaximumStaticPressureProfileChangeInterval)]
        [SerializeField]
        private float staticPressureProfileChangeIntervalMax =
            DefaultStaticPressureProfileChangeIntervalMax;

        [Header("Wake")]
        [Tooltip("Controls shared Wake energy. Stationary geometry and dynamic emitters prepare different source footprints but use this same response strength.")]
        [Range(0f, 3f)]
        [SerializeField] private float obstructionWakeStrength = 1.50f;

        [Tooltip("Controls shared Wake persistence and downstream reach for stationary and dynamic sources.")]
        [Range(0.25f, 3f)]
        [SerializeField] private float obstructionWakeReach = 1f;

        [Tooltip("Controls shared Wake source width. For stationary geometry this shapes the attached lee and rear releases; for dynamic emitters it scales the swept wake footprint. Downstream widening is controlled separately.")]
        [Range(0.5f, 2f)]
        [SerializeField] private float obstructionWakeSpread = 1f;

        [Tooltip("Controls the shared Wake variation envelope. Stationary sources use spatial lee/release profiles; dynamic sources derive most variation from movement and will use the same envelope in the full relative-motion source model.")]
        [Range(0f, 1f)]
        [SerializeField] private float obstructionWakeVariation = 0.35f;

        [Tooltip("Shortest randomized time, in seconds, between stationary wake-source variation targets.")]
        [Range(
            MinimumStaticWakeVariationInterval,
            MaximumStaticWakeVariationInterval)]
        [SerializeField]
        private float obstructionWakeVariationIntervalMin =
            DefaultStaticWakeVariationIntervalMin;

        [Tooltip("Longest randomized time, in seconds, between stationary wake-source variation targets. Transitions occupy approximately 85% of the selected interval.")]
        [Range(
            MinimumStaticWakeVariationInterval,
            MaximumStaticWakeVariationInterval)]
        [SerializeField]
        private float obstructionWakeVariationIntervalMax =
            DefaultStaticWakeVariationIntervalMax;

        [Tooltip("Controls how quickly the shared transported wake field spreads laterally. Stationary and dynamic wake sources use the same persistent field and response rules.")]
        [Range(0.35f, 1.25f)]
        [SerializeField] private float obstructionWakeWidening = 0.65f;

        [Tooltip("Maximum positive surface height produced by the compact core of the shared transported wake field. Static and dynamic wakes use the same bounded geometry response.")]
        [Range(0f, 0.40f)]
        [SerializeField] private float obstructionWakeSurfaceHeight = 0.08f;

        [Tooltip("Controls how tightly transported wake energy is concentrated into visible surface geometry. Lower values retain a broader, stronger response; higher values restrict geometry to the strongest wake core without changing the underlying energy field.")]
        [Range(0.80f, 3f)]
        [SerializeField]
        private float obstructionWakeSurfaceCompactness = 1.50f;

        // Legacy serialized fields are retained only so older scenes and
        // prefabs deserialize without data loss. They are hidden and mirrored
        // from the canonical Wake controls in ValidateSettings().
        //
        // Architecture contract:
        // - stationary geometry and dynamic emitters prepare different source
        //   data because their geometry and motion are different;
        // - after preparation, both use the same Wake strength, reach, spread,
        //   variation envelope, transport, widening, geometry, and normals.
        [HideInInspector, SerializeField]
        private float movingTrailStrength = 1.35f;
        [HideInInspector, SerializeField]
        private float movingTrailPersistence = 0.65f;
        [HideInInspector, SerializeField]
        private float movingTrailWidth = 1f;

        [Header("Impact Ripples")]
        [Tooltip("Controls height and velocity injected by impact events.")]
        [Range(0f, 3f)]
        [SerializeField] private float impactRippleStrength = 1.35f;

        [Tooltip("Controls broad ripple propagation speed in approximate metres per second.")]
        [Range(0.2f, 2.5f)]
        [SerializeField] private float impactRipplePropagation = 1.05f;

        [Tooltip("Controls impact-ripple energy loss per second. Higher values fade faster.")]
        [Range(0.1f, 3f)]
        [SerializeField] private float impactRippleDecay = 0.85f;

        [HideInInspector, SerializeField, Range(0f, 1f)]
        private float impactRippleTestDistanceNormalized = 0.5f;

        [HideInInspector, SerializeField, Range(-1f, 1f)]
        private float impactRippleTestAcrossNormalized;

        [HideInInspector, SerializeField]
        private ImpactRippleEventSettings impactRippleTestEvent =
            ImpactRippleEventSettings.CreateEntryDefaults();

        [Header("Water Body Validation")]
        [SerializeField]
        private StylizedRiverBodyDebugView bodyDebugView =
            StylizedRiverBodyDebugView.Final;

        // Deferred-stage settings are retained for serialized compatibility.
        // The Stage 2 shader intentionally does not consume them yet.
        [SerializeField]
        private Color horizonColor =
            new Color(0.58f, 0.91f, 0.94f, 0.35f);

        [FormerlySerializedAs("waterHighlightColor")]
        [SerializeField]
        private Color specularColor =
            new Color(1f, 1f, 1f, 0.35f);

        [Range(0f, 1f)]
        [SerializeField] private float opacity = 0.82f;

        [Range(0f, 1f)]
        [SerializeField] private float shallowOpacity = 0.42f;

        [Range(0f, 1f)]
        [SerializeField] private float deepOpacity = 0.82f;

        [Min(0.01f)]
        [SerializeField] private float depthFadeDistance = 0.4f;

        [Tooltip("Zero or one keeps a continuous gradient. Two or more posterizes the depth colour.")]
        [Range(0f, 12f)]
        [SerializeField] private float depthBands;

        [SerializeField] private bool useHsvColorBlend = true;

        [Range(0.25f, 20f)]
        [SerializeField] private float horizonPower = 5.1f;

        [Header("Refraction")]
        [Min(0.0001f)]
        [SerializeField] private float refractionScale = 0.01f;

        [Range(0f, 2f)]
        [SerializeField] private float refractionSpeed = 0.053f;

        [Range(0f, 0.05f)]
        [SerializeField] private float refractionStrength = 0.0065f;

        [Header("Surface Normals")]
        [SerializeField] private Texture2D normalTexture;

        [Min(0.0001f)]
        [SerializeField] private float normalScale = 0.012f;

        [Range(0f, 2f)]
        [SerializeField] private float normalSpeed = 0.073f;

        [Range(0f, 2f)]
        [SerializeField] private float normalStrength = 0.277f;

        [Header("Gerstner Waves")]
        [Range(0.15f, 12f)]
        [SerializeField] private float waveScale = 2.8f;

        [Range(0f, 4f)]
        [SerializeField] private float waveSpeed = 0.57f;

        [Range(0f, 0.5f)]
        [SerializeField] private float waveHeight = 0.088f;

        [SerializeField]
        private Vector4 waveDirections =
            new Vector4(0f, 0.5f, 1f, 0.2f);

        [Tooltip("Normalized distance from the centre where wave damping begins. One is the outer bank edge.")]
        [Range(0f, 0.99f)]
        [SerializeField] private float waveEdgeDampingStart = 0.65f;

        [Range(0f, 1f)]
        [SerializeField] private float waveHeightColorStrength = 0.12f;

        [Header("Lighting")]
        [Range(0f, 1f)]
        [SerializeField] private float lightingSmoothness = 0.587f;

        [Range(0f, 1f)]
        [SerializeField] private float lightingHardness = 1f;

        [Range(0f, 4f)]
        [SerializeField] private float specularStrength = 0.65f;

        [Range(1f, 8f)]
        [SerializeField] private float lightingSteps = 4f;

        [Header("Advanced")]
        [SerializeField] private Material bodyMaterial;

        [Range(1, 9999)]
        [SerializeField] private int visualSeed = 1731;

        [SerializeField]
        private StylizedRiverDebugView debugView =
            StylizedRiverDebugView.Final;

        private Texture statefulFoamTexture;
        private Color statefulFoamColor = new Color(0.94f, 0.985f, 1f, 0.78f);
        private float statefulFoamStrength;
        private float statefulFoamThreshold = 0.16f;
        private float statefulFoamSoftness = 0.025f;
        private float statefulFoamBandWidth = 0.14f;
        private float statefulFoamContactStrength = 0.16f;
        private float statefulFoamContactDepth = 0.20f;

        private static readonly int ShallowColorId = Shader.PropertyToID("_ShallowColor");
        private static readonly int DeepColorId = Shader.PropertyToID("_DeepColor");
        private static readonly int ClarityId = Shader.PropertyToID("_Clarity");
        private static readonly int BodyDepthRangeId = Shader.PropertyToID("_BodyDepthRange");
        private static readonly int BodyDepthContrastId = Shader.PropertyToID("_BodyDepthContrast");
        private static readonly int WaterTintStrengthId = Shader.PropertyToID("_WaterTintStrength");
        private static readonly int SurfacePresenceId = Shader.PropertyToID("_SurfacePresence");

        private static readonly int FreezeAmountId = Shader.PropertyToID("_FreezeAmount");
        private static readonly int IceColorId = Shader.PropertyToID("_IceColor");
        private static readonly int IceTransmissionId = Shader.PropertyToID("_IceTransmission");
        private static readonly int IceThicknessId = Shader.PropertyToID("_IceThickness");
        private static readonly int IceCloudinessId = Shader.PropertyToID("_IceCloudiness");
        private static readonly int IceSurfacePresenceId = Shader.PropertyToID("_IceSurfacePresence");
        private static readonly int IceScatteringId = Shader.PropertyToID("_IceScattering");

        private static readonly int LightDependenceId = Shader.PropertyToID("_LightDependence");
        private static readonly int AmbientResponseId = Shader.PropertyToID("_AmbientResponse");
        private static readonly int SunResponseId = Shader.PropertyToID("_SunResponse");
        private static readonly int LocalLightResponseId = Shader.PropertyToID("_LocalLightResponse");
        private static readonly int LightColorInfluenceId = Shader.PropertyToID("_LightColorInfluence");
        private static readonly int MinimumNightVisibilityId = Shader.PropertyToID("_MinimumNightVisibility");
        private static readonly int ShadowResponseId = Shader.PropertyToID("_ShadowResponse");
        private static readonly int LiquidSurfaceShadowResponseId =
            Shader.PropertyToID("_LiquidSurfaceShadowResponse");
        private static readonly int IceSurfaceShadowResponseId =
            Shader.PropertyToID("_IceSurfaceShadowResponse");
        private static readonly int DiffuseWrapId = Shader.PropertyToID("_DiffuseWrap");

        private static readonly int MotionDetailTextureId = Shader.PropertyToID("_MotionDetailTexture");
        private static readonly int FlowSpeedMotionId = Shader.PropertyToID("_MotionFlowSpeed");
        private static readonly int MotionWaveHeightId = Shader.PropertyToID("_MotionWaveHeight");
        private static readonly int MotionWaveLengthId = Shader.PropertyToID("_MotionWaveLength");
        private static readonly int MotionWaveSteepnessId = Shader.PropertyToID("_MotionWaveSteepness");
        private static readonly int MotionDetailStrengthId = Shader.PropertyToID("_MotionDetailStrength");
        private static readonly int MotionDetailScaleId = Shader.PropertyToID("_MotionDetailScale");
        private static readonly int MotionTurbulenceId = Shader.PropertyToID("_MotionTurbulence");
        private static readonly int CurrentAccentStrengthMotionId = Shader.PropertyToID("_CurrentAccentStrength");
        private static readonly int CurrentAccentScaleMotionId = Shader.PropertyToID("_CurrentAccentScale");
        private static readonly int ShoreMotionId = Shader.PropertyToID("_ShoreMotion");
        private static readonly int ShoreMotionWidthId = Shader.PropertyToID("_ShoreMotionWidth");
        private static readonly int MotionDebugViewId = Shader.PropertyToID("_MotionDebugView");
        private static readonly int MotionTimeId = Shader.PropertyToID("_MotionTime");
        private static readonly int MotionSeedId = Shader.PropertyToID("_MotionSeed");

        private static readonly int LiquidRefractionStrengthStage4Id =
            Shader.PropertyToID("_LiquidRefractionStrength");
        private static readonly int RefractionDepthInfluenceStage4Id =
            Shader.PropertyToID("_RefractionDepthInfluence");
        private static readonly int RefractionNormalInfluenceStage4Id =
            Shader.PropertyToID("_RefractionNormalInfluence");
        private static readonly int ShoreRefractionStage4Id =
            Shader.PropertyToID("_ShoreRefraction");
        private static readonly int RefractionEdgeProtectionStage4Id =
            Shader.PropertyToID("_RefractionEdgeProtection");
        private static readonly int PreserveObjectSilhouettesStage4Id =
            Shader.PropertyToID("_PreserveObjectSilhouettes");
        private static readonly int IceDistortionStrengthStage4Id =
            Shader.PropertyToID("_IceDistortionStrength");
        private static readonly int IceDiffusionStage4Id =
            Shader.PropertyToID("_IceDiffusion");
        private static readonly int RefractionQualityStage4Id =
            Shader.PropertyToID("_RefractionQuality");
        private static readonly int RefractionDebugViewStage4Id =
            Shader.PropertyToID("_RefractionDebugView");

        private static readonly int DisturbanceEnabledStage5Id =
            Shader.PropertyToID("_DisturbanceEnabled");

        private static readonly int DomainFallbackDepthId = Shader.PropertyToID("_DomainFallbackDepth");
        private static readonly int BodyDebugViewId = Shader.PropertyToID("_BodyDebugView");
        private static readonly int HorizonColorId = Shader.PropertyToID("_HorizonColor");
        private static readonly int SpecularColorId = Shader.PropertyToID("_SpecularColor");
        private static readonly int OpacityId = Shader.PropertyToID("_Opacity");
        private static readonly int ShallowOpacityId = Shader.PropertyToID("_ShallowOpacity");
        private static readonly int DeepOpacityId = Shader.PropertyToID("_DeepOpacity");
        private static readonly int DepthFadeDistanceId = Shader.PropertyToID("_DepthFadeDistance");
        private static readonly int DepthBandsId = Shader.PropertyToID("_DepthBands");
        private static readonly int UseHsvBlendId = Shader.PropertyToID("_UseHSVBlend");
        private static readonly int HorizonPowerId = Shader.PropertyToID("_HorizonPower");

        private static readonly int RefractionScaleId = Shader.PropertyToID("_RefractionScale");
        private static readonly int RefractionSpeedId = Shader.PropertyToID("_RefractionSpeed");
        private static readonly int RefractionStrengthId = Shader.PropertyToID("_RefractionStrength");

        private static readonly int NormalTextureId = Shader.PropertyToID("_NormalTexture");
        private static readonly int NormalScaleId = Shader.PropertyToID("_NormalScale");
        private static readonly int NormalSpeedId = Shader.PropertyToID("_NormalSpeed");
        private static readonly int NormalStrengthId = Shader.PropertyToID("_NormalStrength");

        private static readonly int SurfaceFoamColorId = Shader.PropertyToID("_SurfaceFoamColor");
        private static readonly int SurfaceFoamColorBlendId = Shader.PropertyToID("_SurfaceFoamColorBlend");

        private static readonly int WaveLengthId = Shader.PropertyToID("_WaveLength");
        private static readonly int WaveSpeedId = Shader.PropertyToID("_WaveSpeed");
        private static readonly int WaveSteepnessId = Shader.PropertyToID("_WaveSteepness");
        private static readonly int WaveDirectionsId = Shader.PropertyToID("_WaveDirections");
        private static readonly int WaveEdgeDampingStartId = Shader.PropertyToID("_WaveEdgeDampingStart");
        private static readonly int WaveHeightColorStrengthId = Shader.PropertyToID("_WaveHeightColorStrength");

        private static readonly int LightingSmoothnessId = Shader.PropertyToID("_LightingSmoothness");
        private static readonly int LightingHardnessId = Shader.PropertyToID("_LightingHardness");
        private static readonly int SpecularStrengthId = Shader.PropertyToID("_SpecularStrength");
        private static readonly int LightingStepsId = Shader.PropertyToID("_LightingSteps");

        private static readonly int RiverWidthId = Shader.PropertyToID("_RiverWidth");
        private static readonly int RiverLengthId = Shader.PropertyToID("_RiverLength");
        private static readonly int FlowDirectionId = Shader.PropertyToID("_FlowDirection");
        private static readonly int RiverTimeId = Shader.PropertyToID("_RiverTime");
        private static readonly int VisualSeedId = Shader.PropertyToID("_VisualSeed");
        private static readonly int DebugViewId = Shader.PropertyToID("_DebugView");

        private static readonly int ExternalFoamFieldId = Shader.PropertyToID("_ExternalFoamField");
        private static readonly int ExternalFoamStrengthId = Shader.PropertyToID("_ExternalFoamStrength");
        private static readonly int ExternalFoamThresholdId = Shader.PropertyToID("_ExternalFoamThreshold");
        private static readonly int ExternalFoamSoftnessId = Shader.PropertyToID("_ExternalFoamSoftness");
        private static readonly int ExternalFoamBandWidthId = Shader.PropertyToID("_ExternalFoamBandWidth");
        private static readonly int ExternalFoamContactStrengthId = Shader.PropertyToID("_ExternalFoamContactStrength");
        private static readonly int ExternalFoamContactDepthId = Shader.PropertyToID("_ExternalFoamContactDepth");

        private static readonly int PlanarReflectionTextureId = Shader.PropertyToID("_PlanarReflectionTexture");
        private static readonly int PlanarReflectionVpId = Shader.PropertyToID("_PlanarReflectionVP");
        private static readonly int PlanarReflectionStrengthId = Shader.PropertyToID("_PlanarReflectionStrength");
        private static readonly int PlanarReflectionDistortionId = Shader.PropertyToID("_PlanarReflectionDistortion");
        private static readonly int PlanarReflectionAvailableId = Shader.PropertyToID("_PlanarReflectionAvailable");

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh surfaceMesh;

        private GameObject corridorObject;
        private MeshFilter corridorMeshFilter;
        private MeshRenderer corridorMeshRenderer;
        private MeshCollider corridorMeshCollider;
        private Mesh corridorMesh;
        private Mesh corridorColliderMesh;
        private StylizedRiverCorridorBuildResult corridorBuildResult;
        private bool corridorTightBendWarningReported;

        private Material temporaryBodyMaterial;
        private MaterialPropertyBlock bodyProperties;
        private bool incompatibleMaterialWarningReported;
        private bool missingWaterLayerWarningReported;

        private Texture2D defaultNormalTexture;

        private Texture planarReflectionTexture;
        private Matrix4x4 planarReflectionVp = Matrix4x4.identity;
        private float planarReflectionStrength;
        private float planarReflectionDistortion;
        private bool planarReflectionAvailable;

        private RiverDomainSnapshot riverDomain = RiverDomainSnapshot.Empty;
        private int riverDomainVersion;

        private float riverLength;
        private float averageSurfaceHeight;
        private float riverTime;
        private double lastEditorTime;
        private double pendingRegenerationTime;
        private bool pendingRegeneration;
        private bool subscribedToSplineChanges;
        private StylizedRiverDisturbanceRuntime disturbanceRuntime;

        public event Action<RiverDomainSnapshot> DomainChanged;

        public SplineContainer SplineContainer => ResolveSplineContainer();
        public StylizedRiverQuality Quality => quality;
        public float Width => width;
        public float BankBlend => bankBlend;
        public float Depth => depth;
        public float SurfaceOffset => surfaceOffset;
        public StylizedRiverWaterBodyPreset BodyPreset => bodyPreset;
        public float Clarity => clarity;
        public float BodyDepthRange => bodyDepthRange;
        public float BodyDepthContrast => bodyDepthContrast;
        public float WaterTintStrength => waterTintStrength;
        public float SurfacePresence => surfacePresence;
        public StylizedRiverSurfaceState SurfaceState => surfaceState;
        public StylizedRiverIceBodyPreset IceBodyPreset => iceBodyPreset;
        public float FreezeAmount => ResolveFreezeAmount();
        public float LightDependence => lightDependence;
        public StylizedRiverMotionPreset MotionPreset => motionPreset;
        public float FlowSpeedMetresPerSecond => flowSpeed;
        public float MotionWaveHeight => motionWaveHeight;
        public float MotionWaveLength => motionWaveLength;
        public float MotionWaveSteepness => motionWaveSteepness;
        public float MotionTurbulence => motionTurbulence;
        public float MotionTime => riverTime;
        public float MotionSeed => visualSeed;
        public float ShoreMotion => shoreMotion;
        public bool RuntimeDisturbancesEnabled => runtimeDisturbances;
        public StylizedRiverDisturbancePreset DisturbancePreset =>
            disturbancePreset;
        public StylizedRiverDisturbanceDebugView DisturbanceDebugView =>
            ResolveDisturbanceDebugView(disturbanceDebugView);

        // Canonical Stage 5 response settings are source-agnostic. The
        // stationary registry path and the dynamic-emitter path prepare
        // different source data, but both consume these Pressure/Wake rules.
        public float PressureStrength => staticPressureStrength;
        public float PressureContactSharpness =>
            staticPressureContactSharpness;
        public float PressureProfileVariation =>
            staticPressureWaveResponse;
        public float PressureProfileChangeIntervalMin =>
            staticPressureProfileChangeIntervalMin;
        public float PressureProfileChangeIntervalMax =>
            staticPressureProfileChangeIntervalMax;

        public float WakeStrength => obstructionWakeStrength;
        public float WakeReach => obstructionWakeReach;
        public float WakeSpread => obstructionWakeSpread;
        public float WakeVariation => obstructionWakeVariation;
        public float WakeVariationIntervalMin =>
            obstructionWakeVariationIntervalMin;
        public float WakeVariationIntervalMax =>
            obstructionWakeVariationIntervalMax;
        public float WakeWidening => obstructionWakeWidening;
        public float WakeSurfaceHeight =>
            obstructionWakeSurfaceHeight;
        public float WakeSurfaceCompactness =>
            obstructionWakeSurfaceCompactness;

        // Compatibility aliases preserve existing serialized/code contracts.
        // They do not represent separate visual systems.
        public float StaticPressureStrength => PressureStrength;
        public float StaticPressureContactSharpness =>
            PressureContactSharpness;
        public float StaticPressureProfileVariation =>
            PressureProfileVariation;
        public float StaticPressureProfileChangeIntervalMin =>
            PressureProfileChangeIntervalMin;
        public float StaticPressureProfileChangeIntervalMax =>
            PressureProfileChangeIntervalMax;
        public float StaticPressureWaveResponse =>
            PressureProfileVariation;

        public float ObstructionWakeStrength => WakeStrength;
        public float ObstructionWakeReach => WakeReach;
        public float ObstructionWakeSpread => WakeSpread;
        public float ObstructionWakeVariation => WakeVariation;
        public float ObstructionWakeVariationIntervalMin =>
            WakeVariationIntervalMin;
        public float ObstructionWakeVariationIntervalMax =>
            WakeVariationIntervalMax;
        public float ObstructionWakeWidening => WakeWidening;
        public float ObstructionWakeSurfaceHeight =>
            WakeSurfaceHeight;
        public float ObstructionWakeSurfaceCompactness =>
            WakeSurfaceCompactness;

        // Legacy Moving Trail accessors now resolve to the canonical Wake
        // controls. Persistence remains normalized for older callers.
        public float MovingTrailStrength => WakeStrength;
        public float MovingTrailPersistence =>
            Mathf.InverseLerp(0.25f, 3f, WakeReach);
        public float MovingTrailWidth => WakeSpread;
        public float ImpactRippleStrength => impactRippleStrength;
        public float ImpactRipplePropagation => impactRipplePropagation;
        public float ImpactRippleDecay => impactRippleDecay;
        public float ImpactRippleTestDistanceNormalized =>
            impactRippleTestDistanceNormalized;
        public float ImpactRippleTestAcrossNormalized =>
            impactRippleTestAcrossNormalized;
        public ImpactRippleEventSettings ImpactRippleTestEvent =>
            impactRippleTestEvent;
        public float ResolvedImpactRippleMaximumHeight =>
            ResolveImpactRippleMaximumHeight();
        public float ResolvedInteractionMinimumWavelength =>
            ResolveInteractionMinimumWavelength();

        // Compatibility aliases for existing emitter and renderer integrations.
        // They are no longer backed by exposed global Stage 5 controls.
        public float DisturbanceStrength => WakeStrength;
        public float DisturbancePersistence => MovingTrailPersistence;
        public float DisturbancePropagationSpeed => impactRipplePropagation;
        public float DisturbanceAdvection => 1f;
        public float DisturbanceGeometryStrength => 1f;
        public float DisturbanceNormalStrength => 1f;
        public float DisturbanceShoreInteraction => 0.65f;
        public float DisturbanceMaximumHeight =>
            ResolveImpactRippleMaximumHeight();
        public float DisturbanceMinimumWavelength =>
            ResolveInteractionMinimumWavelength();
        public float LiquidFactor => ResolveLiquidFactor();
        public float ResolvedMaximumDownwardMotion =>
            ResolveLiquidFactor() * motionWaveHeight;
        public float ResolvedSurfaceLongitudinalSpacing =>
            ResolveSurfaceLongitudinalSpacing();
        public float VisibleHalfWidth => width * 0.5f;
        public float VisibleWidth => width;
        public float AutomaticShorelineOverlap =>
            ResolveAutomaticShorelineOverlap();
        public float AdditionalShorelineOverlap =>
            additionalShorelineOverlap;
        public float ResolvedShorelineOverlap =>
            AutomaticShorelineOverlap + additionalShorelineOverlap;
        public float GeneratedSurfaceHalfWidth =>
            VisibleHalfWidth + ResolvedShorelineOverlap;
        public float GeneratedSurfaceWidth =>
            GeneratedSurfaceHalfWidth * 2f;
        public float ShorelineWetClearance => shorelineWetClearance;
        public float ShorelineBankCover => shorelineBankCover;
        public float ReservedDownwardSurfaceDisplacement =>
            reservedDownwardSurfaceDisplacement;
        public float TerrainConformity => terrainConformity;
        public StylizedRiverChannelCharacterPreset ChannelCharacterPreset =>
            channelCharacterPreset;
        public int NaturalVariationSeed => naturalVariationSeed;
        public float BedRoughness => bedRoughness;
        public float BedRoughnessScale => bedRoughnessScale;
        public float BedRoughnessReach => bedRoughnessReach;
        public float ShorelineIrregularity => shorelineIrregularity;
        public float ShorelineIrregularityScale => shorelineIrregularityScale;
        public float BankAsymmetry => bankAsymmetry;
        public float ResolvedBedRoughness =>
            ResolveNaturalVariationSettings().ResolveSafeBedRoughness(
                depth,
                shorelineWetClearance + Mathf.Max(
                    reservedDownwardSurfaceDisplacement,
                    ResolvedMaximumDownwardMotion));
        public float ResolvedMinimumVisibleWidth =>
            ResolveDomainVisibleWidthRange(out float minimum, out _)
                ? minimum
                : width;
        public float ResolvedMaximumVisibleWidth =>
            ResolveDomainVisibleWidthRange(out _, out float maximum)
                ? maximum
                : width;
        public float CorridorOuterWidth => corridorBuildResult.MaximumOuterWidth;
        public float CorridorHandoffWidth => corridorBuildResult.MaximumHandoffWidth;
        public float CorridorIntegrationApronWidth =>
            corridorBuildResult.IntegrationApronWidth;
        public int CorridorRingCount => corridorBuildResult.RingCount;
        public int CorridorAcrossVertexCount =>
            corridorBuildResult.AcrossVertexCount;
        public int CorridorTriangleCount => corridorBuildResult.TriangleCount;
        public int CorridorColliderTriangleCount =>
            corridorBuildResult.ColliderTriangleCount;
        public bool CorridorUsesGroundHeightField =>
            corridorBuildResult.UsedGroundHeightField;
        public bool CorridorHasTightBendWarning =>
            corridorBuildResult.TightBendWarning;
        public RiverDomainSnapshot Domain => riverDomain ?? RiverDomainSnapshot.Empty;
        public float DomainSampleSpacing => domainSampleSpacing;
        public float ConnectedRiverDistanceOffset => connectedRiverDistanceOffset;
        public float RiverLength => riverLength;
        public float GlobalDistanceMinimum => Domain.GlobalDistanceMinimum;
        public float GlobalDistanceMaximum => Domain.GlobalDistanceMaximum;
        public float AverageSurfaceHeight => averageSurfaceHeight;
        public int VisualSeed => visualSeed;
        public float FlowDirection => reverseFlow ? -1f : 1f;
        public MeshRenderer SurfaceRenderer => meshRenderer != null ? meshRenderer : GetComponent<MeshRenderer>();

        internal float EvaluateMotionMacroHeight(
            float globalDistance,
            float lateralMetres,
            float time)
        {
            const float twoPi = 6.28318530718f;
            float wavelength = Mathf.Max(0.25f, motionWaveLength);
            float phaseSpeed = flowSpeed * twoPi / wavelength;
            float seedPhase = MotionFrac(visualSeed * 0.01371f) * twoPi;
            Vector2 noiseCoordinate = new Vector2(
                globalDistance / Mathf.Max(1f, wavelength * 1.8f),
                lateralMetres / Mathf.Max(1f, wavelength * 0.8f));
            float evolvingNoise = EvaluateMotionValueNoise(
                noiseCoordinate +
                new Vector2(
                    -time * flowSpeed /
                    Mathf.Max(1f, wavelength * 5f),
                    time * 0.035f));
            float distortion =
                (evolvingNoise * 2f - 1f) *
                Mathf.Clamp01(motionTurbulence) *
                1.65f;
            float phase =
                globalDistance * twoPi / wavelength -
                time * phaseSpeed +
                seedPhase +
                distortion;
            float crossPhase =
                lateralMetres * twoPi /
                Mathf.Max(0.75f, wavelength * 1.35f);
            float primary = Mathf.Sin(
                phase +
                Mathf.Sin(crossPhase) * motionTurbulence * 0.55f);
            float secondary = Mathf.Sin(
                phase * 1.73f -
                crossPhase * 0.42f +
                seedPhase * 0.31f +
                time * phaseSpeed * 0.21f);
            float combined = primary * 0.72f + secondary * 0.28f;
            float sign = combined > 0f
                ? 1f
                : combined < 0f
                    ? -1f
                    : 0f;
            float crest = sign * Mathf.Pow(
                Mathf.Abs(combined),
                Mathf.Lerp(
                    1f,
                    0.58f,
                    Mathf.Clamp01(motionWaveSteepness)));
            return crest * Mathf.Max(0f, motionWaveHeight);
        }

        private static float EvaluateMotionValueNoise(Vector2 coordinate)
        {
            float cellX = Mathf.Floor(coordinate.x);
            float cellY = Mathf.Floor(coordinate.y);
            float fractionX = MotionFrac(coordinate.x);
            float fractionY = MotionFrac(coordinate.y);
            fractionX = fractionX * fractionX *
                        (3f - 2f * fractionX);
            fractionY = fractionY * fractionY *
                        (3f - 2f * fractionY);

            float a = EvaluateMotionHash21(cellX, cellY);
            float b = EvaluateMotionHash21(cellX + 1f, cellY);
            float c = EvaluateMotionHash21(cellX, cellY + 1f);
            float d = EvaluateMotionHash21(cellX + 1f, cellY + 1f);
            return Mathf.Lerp(
                Mathf.Lerp(a, b, fractionX),
                Mathf.Lerp(c, d, fractionX),
                fractionY);
        }

        private static float EvaluateMotionHash21(float x, float y)
        {
            float px = MotionFrac(x * 123.34f);
            float py = MotionFrac(y * 456.21f);
            float offset =
                px * (px + 45.32f) +
                py * (py + 45.32f);
            px += offset;
            py += offset;
            return MotionFrac(px * py);
        }

        private static float MotionFrac(float value)
        {
            return value - Mathf.Floor(value);
        }

        public int SurfaceTriangleCount =>
            surfaceMesh != null && surfaceMesh.subMeshCount > 0
                ? (int)surfaceMesh.GetIndexCount(0) / 3
                : 0;

        private void Reset()
        {
            splineContainer = GetComponent<SplineContainer>();
            AssignWaterLayer();
        }

        private void OnEnable()
        {
            disturbanceDebugView =
                ResolveDisturbanceDebugView(disturbanceDebugView);
            CacheComponents();
            ResolveSplineContainer();
            AssignWaterLayer();
            SubscribeToSplineChanges();
            RemoveLegacyGeneratedObjects();
            EnsureSurfaceOutput();
            EnsureCorridorOutput();
            EnsureDisturbanceRuntime();
            SetRendererEnabled(true);
            RegenerateAll();
            lastEditorTime = Time.realtimeSinceStartupAsDouble;
        }

        private void OnDisable()
        {
            UnsubscribeFromSplineChanges();
            SetRendererEnabled(false);
        }

        private static StylizedRiverDisturbanceDebugView
            ResolveDisturbanceDebugView(
                StylizedRiverDisturbanceDebugView value)
        {
            int rawValue = (int)value;
            if ((rawValue >= 0 && rawValue <= 8) || rawValue == 19)
            {
                return value;
            }

            if (rawValue == 9 || rawValue == 10 || rawValue == 11 ||
                rawValue == 20)
            {
                return StylizedRiverDisturbanceDebugView.StaticWakeSource;
            }

            if (rawValue == 12 || rawValue == 18)
            {
                return StylizedRiverDisturbanceDebugView.
                    FinalWakeGeometryHeight;
            }

            if (rawValue >= 13 && rawValue <= 17)
            {
                return StylizedRiverDisturbanceDebugView.WakeEnergy;
            }

            return StylizedRiverDisturbanceDebugView.Final;
        }

        private void OnValidate()
        {
            disturbanceDebugView =
                ResolveDisturbanceDebugView(disturbanceDebugView);
            ValidateSettings();
            CacheComponents();
            ResolveSplineContainer();
            AssignWaterLayer();
            RemoveLegacyGeneratedObjects();
            EnsureSurfaceOutput();
            EnsureCorridorOutput();
            EnsureDisturbanceRuntime();
            ApplyVisualSettings();

            if (liveRegeneration)
            {
                RequestRegeneration();
            }
        }

        private void Update()
        {
            double now = Time.realtimeSinceStartupAsDouble;

            float deltaTime =
                Application.isPlaying
                    ? Time.deltaTime
                    : Mathf.Clamp((float)(now - lastEditorTime), 0f, 0.1f);

            lastEditorTime = now;

            if (pendingRegeneration && now >= pendingRegenerationTime)
            {
                pendingRegeneration = false;
                RegenerateAll();
            }

            if (deltaTime <= 0f)
            {
                return;
            }

            riverTime = Mathf.Repeat(riverTime + deltaTime, 4096f);
            ApplyAnimationClock();
        }

        [ContextMenu("Regenerate River and Ground")]
        public void RegenerateAll()
        {
            ValidateSettings();
            CacheComponents();
            ResolveSplineContainer();
            AssignWaterLayer();
            RemoveLegacyGeneratedObjects();
            EnsureSurfaceOutput();
            EnsureCorridorOutput();
            BuildRiverDomain();
            BuildSurface();

            if (!NotifyParentGround())
            {
                BuildCorridor();
            }

            ApplyVisualSettings();
            NotifyReflectionSurfaceChanged();
            NotifyFoamSimulationChanged();
        }

        [ContextMenu("Rebuild Surface Only")]
        public void RebuildSurfaceOnly()
        {
            ValidateSettings();
            CacheComponents();
            ResolveSplineContainer();
            AssignWaterLayer();
            RemoveLegacyGeneratedObjects();
            EnsureSurfaceOutput();
            EnsureCorridorOutput();
            BuildRiverDomain();
            BuildSurface();
            BuildCorridor();
            ApplyVisualSettings();
            NotifyReflectionSurfaceChanged();
            NotifyFoamSimulationChanged();
        }

        /// <summary>
        /// Rebuilds only the dedicated visible river corridor after the parent
        /// generated ground has refreshed its base-height field and concealed
        /// broad ground mesh.
        /// </summary>
        public void RebuildCorridorFromGround()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            ValidateSettings();
            CacheComponents();
            EnsureRiverDomain();
            EnsureCorridorOutput();
            BuildCorridor();
        }

        [ContextMenu("Clear Generated River")]
        public void ClearGenerated()
        {
            if (surfaceMesh != null)
            {
                surfaceMesh.Clear();
            }

            if (corridorMesh != null)
            {
                corridorMesh.Clear();
            }

            if (corridorColliderMesh != null)
            {
                corridorColliderMesh.Clear();
            }

            if (corridorMeshCollider != null)
            {
                corridorMeshCollider.sharedMesh = null;
            }

            corridorBuildResult = default;
            riverDomainVersion++;
            riverDomain = new RiverDomainSnapshot(
                Array.Empty<StylizedRiverSplineSample>(),
                0f,
                domainSampleSpacing,
                connectedRiverDistanceOffset,
                reverseFlow,
                riverDomainVersion);
            riverLength = 0f;
            averageSurfaceHeight = transform.position.y + surfaceOffset;
            DomainChanged?.Invoke(Domain);
            ApplyVisualSettings();
            NotifyReflectionSurfaceChanged();
            NotifyFoamSimulationChanged();
        }


        public void ApplyChannelCharacterPreset()
        {
            ApplyChannelCharacterPreset(channelCharacterPreset);
        }

        public void ApplyChannelCharacterPreset(
            StylizedRiverChannelCharacterPreset preset)
        {
            channelCharacterPreset = preset;

            switch (preset)
            {
                case StylizedRiverChannelCharacterPreset.Engineered:
                    bedRoughness = 0f;
                    bedRoughnessScale = 8f;
                    bedRoughnessReach = 0f;
                    shorelineIrregularity = 0f;
                    shorelineIrregularityScale = 14f;
                    bankAsymmetry = 0.5f;
                    break;

                case StylizedRiverChannelCharacterPreset.SmoothNatural:
                    bedRoughness = 0.10f;
                    bedRoughnessScale = 8f;
                    bedRoughnessReach = 0.45f;
                    shorelineIrregularity = 0.25f;
                    shorelineIrregularityScale = 14f;
                    bankAsymmetry = 0.40f;
                    break;

                case StylizedRiverChannelCharacterPreset.Irregular:
                    bedRoughness = 0.24f;
                    bedRoughnessScale = 5.5f;
                    bedRoughnessReach = 0.60f;
                    shorelineIrregularity = 0.50f;
                    shorelineIrregularityScale = 9f;
                    bankAsymmetry = 0.65f;
                    break;

                case StylizedRiverChannelCharacterPreset.Rugged:
                    bedRoughness = 0.48f;
                    bedRoughnessScale = 3.5f;
                    bedRoughnessReach = 0.70f;
                    shorelineIrregularity = 0.85f;
                    shorelineIrregularityScale = 6f;
                    bankAsymmetry = 0.82f;
                    break;

                case StylizedRiverChannelCharacterPreset.Custom:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(preset),
                        preset,
                        "Unsupported channel-character preset.");
            }

            ValidateSettings();
            RegenerateAll();
        }

        public void MarkChannelCharacterCustom()
        {
            channelCharacterPreset =
                StylizedRiverChannelCharacterPreset.Custom;
        }

        public void ApplyMotionPreset()
        {
            ApplyMotionPreset(motionPreset);
        }

        public void ApplyMotionPreset(StylizedRiverMotionPreset preset)
        {
            motionPreset = preset;

            switch (preset)
            {
                case StylizedRiverMotionPreset.Still:
                    flowSpeed = 0f;
                    motionWaveHeight = 0f;
                    motionWaveLength = 6f;
                    motionWaveSteepness = 0f;
                    motionDetailStrength = 0f;
                    motionDetailScale = 1.8f;
                    motionTurbulence = 0f;
                    currentAccentStrength = 0f;
                    currentAccentScale = 6f;
                    shoreMotion = 0f;
                    shoreMotionWidth = 0.9f;
                    break;

                case StylizedRiverMotionPreset.Calm:
                    flowSpeed = 0.35f;
                    motionWaveHeight = 0.018f;
                    motionWaveLength = 6.5f;
                    motionWaveSteepness = 0.18f;
                    motionDetailStrength = 0.18f;
                    motionDetailScale = 1.8f;
                    motionTurbulence = 0.25f;
                    currentAccentStrength = 0.03f;
                    currentAccentScale = 7f;
                    shoreMotion = 0.40f;
                    shoreMotionWidth = 1.0f;
                    break;

                case StylizedRiverMotionPreset.Flowing:
                    flowSpeed = 1.2f;
                    motionWaveHeight = 0.07f;
                    motionWaveLength = 4.8f;
                    motionWaveSteepness = 0.42f;
                    motionDetailStrength = 0.42f;
                    motionDetailScale = 1.15f;
                    motionTurbulence = 0.58f;
                    currentAccentStrength = 0.16f;
                    currentAccentScale = 4.8f;
                    shoreMotion = 0.58f;
                    shoreMotionWidth = 0.75f;
                    break;

                case StylizedRiverMotionPreset.Furious:
                    flowSpeed = 2.1f;
                    motionWaveHeight = 0.24f;
                    motionWaveLength = 3.2f;
                    motionWaveSteepness = 0.72f;
                    motionDetailStrength = 0.82f;
                    motionDetailScale = 0.72f;
                    motionTurbulence = 0.90f;
                    currentAccentStrength = 0.42f;
                    currentAccentScale = 2.8f;
                    shoreMotion = 0.78f;
                    shoreMotionWidth = 0.45f;
                    break;

                case StylizedRiverMotionPreset.Custom:
                    break;
            }

            ValidateSettings();
            RebuildSurfaceOnly();
        }

        public void ApplyDisturbancePreset()
        {
            ApplyDisturbancePreset(disturbancePreset);
        }

        public void ApplyDisturbancePreset(
            StylizedRiverDisturbancePreset preset)
        {
            disturbancePreset = preset;

            switch (preset)
            {
                case StylizedRiverDisturbancePreset.None:
                    runtimeDisturbances = false;
                    staticPressureStrength = 0f;
                    obstructionWakeStrength = 0f;
                    obstructionWakeVariation = 0f;
                    obstructionWakeVariationIntervalMin =
                        DefaultStaticWakeVariationIntervalMin;
                    obstructionWakeVariationIntervalMax =
                        DefaultStaticWakeVariationIntervalMax;
                    obstructionWakeWidening = 0.65f;
                    impactRippleStrength = 0f;
                    break;

                case StylizedRiverDisturbancePreset.Subtle:
                    runtimeDisturbances = true;
                    staticPressureStrength = 0f;
                    staticPressureContactSharpness = 2.2f;
                    staticPressureWaveResponse = 0.6f;
                    staticPressureProfileChangeIntervalMin =
                        DefaultStaticPressureProfileChangeIntervalMin;
                    staticPressureProfileChangeIntervalMax =
                        DefaultStaticPressureProfileChangeIntervalMax;
                    obstructionWakeStrength = 0.90f;
                    obstructionWakeReach = 0.75f;
                    obstructionWakeSpread = 0.85f;
                    obstructionWakeVariation = 0.35f;
                    obstructionWakeVariationIntervalMin =
                        DefaultStaticWakeVariationIntervalMin;
                    obstructionWakeVariationIntervalMax =
                        DefaultStaticWakeVariationIntervalMax;
                    obstructionWakeWidening = 0.65f;
                    impactRippleStrength = 0.90f;
                    impactRipplePropagation = 0.82f;
                    impactRippleDecay = 1.25f;
                    break;

                case StylizedRiverDisturbancePreset.Balanced:
                    runtimeDisturbances = true;
                    staticPressureStrength = 0.65f;
                    staticPressureContactSharpness = 2.8f;
                    staticPressureWaveResponse = 1f;
                    staticPressureProfileChangeIntervalMin =
                        DefaultStaticPressureProfileChangeIntervalMin;
                    staticPressureProfileChangeIntervalMax =
                        DefaultStaticPressureProfileChangeIntervalMax;
                    obstructionWakeStrength = 1.50f;
                    obstructionWakeReach = 1f;
                    obstructionWakeSpread = 1f;
                    obstructionWakeVariation = 0.35f;
                    obstructionWakeVariationIntervalMin =
                        DefaultStaticWakeVariationIntervalMin;
                    obstructionWakeVariationIntervalMax =
                        DefaultStaticWakeVariationIntervalMax;
                    obstructionWakeWidening = 0.65f;
                    impactRippleStrength = 1.35f;
                    impactRipplePropagation = 1.05f;
                    impactRippleDecay = 0.85f;
                    break;

                case StylizedRiverDisturbancePreset.Reactive:
                    runtimeDisturbances = true;
                    staticPressureStrength = 0.90f;
                    staticPressureContactSharpness = 2.8f;
                    staticPressureWaveResponse = 1.35f;
                    staticPressureProfileChangeIntervalMin =
                        DefaultStaticPressureProfileChangeIntervalMin;
                    staticPressureProfileChangeIntervalMax =
                        DefaultStaticPressureProfileChangeIntervalMax;
                    obstructionWakeStrength = 2f;
                    obstructionWakeReach = 1.35f;
                    obstructionWakeSpread = 1.25f;
                    obstructionWakeVariation = 0.35f;
                    obstructionWakeVariationIntervalMin =
                        DefaultStaticWakeVariationIntervalMin;
                    obstructionWakeVariationIntervalMax =
                        DefaultStaticWakeVariationIntervalMax;
                    obstructionWakeWidening = 0.65f;
                    impactRippleStrength = 1.8f;
                    impactRipplePropagation = 1.3f;
                    impactRippleDecay = 0.55f;
                    break;

                case StylizedRiverDisturbancePreset.Custom:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(preset),
                        preset,
                        "Unsupported disturbance preset.");
            }

            ValidateSettings();
            EnsureDisturbanceRuntime();
            RebuildSurfaceOnly();
            disturbanceRuntime?.NotifyRiverChanged();
        }

        public StylizedRiverDisturbanceRuntime GetOrCreateDisturbanceRuntime()
        {
            EnsureDisturbanceRuntime();
            return disturbanceRuntime;
        }

        public void ApplyRefractionPreset()
        {
            ApplyRefractionPreset(refractionPreset);
        }

        public void ApplyRefractionPreset(
            StylizedRiverRefractionPreset preset)
        {
            refractionPreset = preset;

            switch (preset)
            {
                case StylizedRiverRefractionPreset.None:
                    liquidRefractionStrength = 0f;
                    refractionDepthInfluence = 0.55f;
                    refractionNormalInfluence = 0.65f;
                    shoreRefraction = 0f;
                    depthEdgeProtection = 0.95f;
                    iceDistortionStrength = 0f;
                    iceDiffusion = 0f;
                    break;

                case StylizedRiverRefractionPreset.Clear:
                    liquidRefractionStrength = 0.0012f;
                    refractionDepthInfluence = 0.35f;
                    refractionNormalInfluence = 0.45f;
                    shoreRefraction = 0.14f;
                    depthEdgeProtection = 0.94f;
                    iceDistortionStrength = 0.0008f;
                    iceDiffusion = 0.10f;
                    break;

                case StylizedRiverRefractionPreset.Balanced:
                    liquidRefractionStrength = 0.0025f;
                    refractionDepthInfluence = 0.55f;
                    refractionNormalInfluence = 0.65f;
                    shoreRefraction = 0.22f;
                    depthEdgeProtection = 0.88f;
                    iceDistortionStrength = 0.0015f;
                    iceDiffusion = 0.28f;
                    break;

                case StylizedRiverRefractionPreset.Distorted:
                    liquidRefractionStrength = 0.0045f;
                    refractionDepthInfluence = 0.72f;
                    refractionNormalInfluence = 0.85f;
                    shoreRefraction = 0.30f;
                    depthEdgeProtection = 0.80f;
                    iceDistortionStrength = 0.0025f;
                    iceDiffusion = 0.45f;
                    break;

                case StylizedRiverRefractionPreset.Custom:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(preset),
                        preset,
                        "Unsupported refraction preset.");
            }

            ValidateSettings();
            ApplyVisualSettings();
        }

        public void ApplyWaterBodyPreset()
        {
            ApplyWaterBodyPreset(bodyPreset);
        }

        public void ApplyWaterBodyPreset(
            StylizedRiverWaterBodyPreset preset)
        {
            bodyPreset = preset;

            switch (preset)
            {
                case StylizedRiverWaterBodyPreset.ClearStream:
                    shallowColor = new Color(0.62f, 0.88f, 0.82f, 1f);
                    deepColor = new Color(0.18f, 0.55f, 0.62f, 1f);
                    clarity = 0.90f;
                    bodyDepthRange = 0.90f;
                    bodyDepthContrast = 0.24f;
                    waterTintStrength = 0.42f;
                    surfacePresence = 0.32f;
                    break;

                case StylizedRiverWaterBodyPreset.DeepWater:
                    shallowColor = new Color(0.28f, 0.62f, 0.68f, 1f);
                    deepColor = new Color(0.015f, 0.12f, 0.25f, 1f);
                    clarity = 0.28f;
                    bodyDepthRange = 2.40f;
                    bodyDepthContrast = 0.68f;
                    waterTintStrength = 0.90f;
                    surfacePresence = 0.58f;
                    break;

                case StylizedRiverWaterBodyPreset.BalancedRiver:
                    shallowColor = new Color(0.458f, 0.802f, 0.798f, 1f);
                    deepColor = new Color(0f, 0.310f, 0.594f, 1f);
                    clarity = 0.62f;
                    bodyDepthRange = 1.40f;
                    bodyDepthContrast = 0.50f;
                    waterTintStrength = 0.72f;
                    surfacePresence = 0.46f;
                    break;

                case StylizedRiverWaterBodyPreset.Custom:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(preset),
                        preset,
                        "Unsupported water-body preset.");
            }

            ValidateSettings();
            ApplyVisualSettings();
        }

        public void MarkWaterBodyCustom()
        {
            bodyPreset = StylizedRiverWaterBodyPreset.Custom;
        }

        public void ApplyIceBodyPreset()
        {
            ApplyIceBodyPreset(iceBodyPreset);
        }

        public void ApplyIceBodyPreset(
            StylizedRiverIceBodyPreset preset)
        {
            iceBodyPreset = preset;

            switch (preset)
            {
                case StylizedRiverIceBodyPreset.ClearIce:
                    iceColor = new Color(0.66f, 0.86f, 0.96f, 1f);
                    iceTransmission = 0.48f;
                    iceThickness = 0.28f;
                    iceCloudiness = 0.12f;
                    iceSurfacePresence = 0.72f;
                    iceScattering = 0.32f;
                    break;

                case StylizedRiverIceBodyPreset.CloudyIce:
                    iceColor = new Color(0.56f, 0.78f, 0.90f, 1f);
                    iceTransmission = 0.16f;
                    iceThickness = 0.72f;
                    iceCloudiness = 0.58f;
                    iceSurfacePresence = 0.86f;
                    iceScattering = 0.68f;
                    break;

                case StylizedRiverIceBodyPreset.DeepBlueIce:
                    iceColor = new Color(0.22f, 0.46f, 0.68f, 1f);
                    iceTransmission = 0.10f;
                    iceThickness = 0.88f;
                    iceCloudiness = 0.38f;
                    iceSurfacePresence = 0.92f;
                    iceScattering = 0.52f;
                    break;

                case StylizedRiverIceBodyPreset.Custom:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(preset),
                        preset,
                        "Unsupported ice-body preset.");
            }

            ValidateSettings();
            ApplyVisualSettings();
        }

        public void MarkIceBodyCustom()
        {
            iceBodyPreset = StylizedRiverIceBodyPreset.Custom;
        }

        /// <summary>
        /// Switches the authored river surface between liquid and frozen
        /// endpoints. This is an instantaneous state change; no visible
        /// freeze/thaw transition is simulated.
        /// </summary>
        public void SetFrozen(bool frozen)
        {
            SetSurfaceState(
                frozen
                    ? StylizedRiverSurfaceState.Frozen
                    : StylizedRiverSurfaceState.Liquid);
        }

        public void SetSurfaceState(
            StylizedRiverSurfaceState state)
        {
            if (surfaceState == state)
            {
                return;
            }

            surfaceState = state;
            ValidateSettings();
            ApplyVisualSettings();
            NotifyReflectionSurfaceChanged();
            NotifyFoamSimulationChanged();
        }

        public void SetCustomFreezeAmount(float amount)
        {
            customFreezeAmount = Mathf.Clamp01(amount);
            surfaceState = StylizedRiverSurfaceState.Custom;
            ApplyVisualSettings();
            NotifyReflectionSurfaceChanged();
            NotifyFoamSimulationChanged();
        }

        private float ResolveLiquidFactor()
        {
            return 1f - ResolveFreezeAmount();
        }

        private float ResolveFreezeAmount()
        {
            return surfaceState switch
            {
                StylizedRiverSurfaceState.Liquid => 0f,
                StylizedRiverSurfaceState.Frozen => 1f,
                StylizedRiverSurfaceState.Custom =>
                    Mathf.Clamp01(customFreezeAmount),
                _ => 0f
            };
        }

        public void ConfigureConnectedDomain(
            float distanceOffset,
            bool isReverseFlow)
        {
            bool changed =
                !Mathf.Approximately(
                    connectedRiverDistanceOffset,
                    distanceOffset) ||
                reverseFlow != isReverseFlow;

            if (!changed)
            {
                return;
            }

            connectedRiverDistanceOffset = distanceOffset;
            reverseFlow = isReverseFlow;
            RebuildSurfaceOnly();
        }

        public float BuildSharedSplineSamples(
            List<StylizedRiverSplineSample> targetSamples)
        {
            if (targetSamples == null)
            {
                throw new ArgumentNullException(nameof(targetSamples));
            }

            EnsureRiverDomain();
            targetSamples.Clear();

            for (int index = 0; index < Domain.SampleCount; index++)
            {
                targetSamples.Add(Domain.Samples[index]);
            }

            return Domain.LocalLength;
        }

        public bool TryProjectWorldPoint(
            Vector3 worldPoint,
            out StylizedRiverProjection projection)
        {
            EnsureRiverDomain();
            return Domain.TryProjectWorldPoint(worldPoint, out projection);
        }

        public StylizedRiverSplineSample SampleAtLocalDistance(
            float localDistance)
        {
            EnsureRiverDomain();
            return Domain.SampleAtLocalDistance(localDistance);
        }

        public StylizedRiverSplineSample SampleAtOrientedDistance(
            float orientedDistance)
        {
            EnsureRiverDomain();
            return Domain.SampleAtOrientedDistance(orientedDistance);
        }

        public StylizedRiverSplineSample SampleAtGlobalDistance(
            float globalDistance)
        {
            EnsureRiverDomain();
            return Domain.SampleAtGlobalDistance(globalDistance);
        }

        public Vector3 RiverToWorld(
            float localDistance,
            float acrossMetres,
            float heightOffset = 0f)
        {
            EnsureRiverDomain();
            return Domain.RiverToWorld(
                localDistance,
                acrossMetres,
                heightOffset);
        }

        [ContextMenu("Validate River Domain Contract")]
        public void ValidateRiverDomainContract()
        {
            EnsureRiverDomain();

            bool valid = Domain.ValidateContract(out string report);

            if (valid)
            {
                Debug.Log(report, this);
            }
            else
            {
                Debug.LogError(report, this);
            }
        }

        public StylizedRiverGroundSnapshot CreateGroundSnapshot(
            Transform groundTransform)
        {
            if (groundTransform == null)
            {
                throw new ArgumentNullException(nameof(groundTransform));
            }

            EnsureRiverDomain();

            if (!Domain.IsValid)
            {
                return default;
            }

            Vector3[] localPoints = new Vector3[Domain.SampleCount];
            Vector3[] localSides = new Vector3[Domain.SampleCount];
            float[] leftVisibleHalfWidths = new float[Domain.SampleCount];
            float[] rightVisibleHalfWidths = new float[Domain.SampleCount];
            float[] leftSurfaceHalfWidths = new float[Domain.SampleCount];
            float[] rightSurfaceHalfWidths = new float[Domain.SampleCount];

            for (int index = 0; index < Domain.SampleCount; index++)
            {
                StylizedRiverSplineSample sample = Domain.Samples[index];

                localPoints[index] =
                    groundTransform.InverseTransformPoint(
                        sample.SurfacePoint);
                localSides[index] =
                    groundTransform
                        .InverseTransformDirection(sample.Side)
                        .normalized;

                leftVisibleHalfWidths[index] = sample.LeftHalfWidth;
                rightVisibleHalfWidths[index] = sample.RightHalfWidth;
                leftSurfaceHalfWidths[index] =
                    sample.LeftSurfaceHalfWidth;
                rightSurfaceHalfWidths[index] =
                    sample.RightSurfaceHalfWidth;
            }

            return new StylizedRiverGroundSnapshot(
                localPoints,
                localSides,
                leftVisibleHalfWidths,
                rightVisibleHalfWidths,
                leftSurfaceHalfWidths,
                rightSurfaceHalfWidths,
                bankBlend,
                depth,
                bedFlatness,
                bankProfile,
                terrainConformity,
                ResolveGroundGridSpacing(),
                shorelineWetClearance,
                shorelineBankCover,
                reservedDownwardSurfaceDisplacement);
        }

        public bool UsesSpline(Spline spline)
        {
            SplineContainer container = ResolveSplineContainer();

            if (container == null || spline == null)
            {
                return false;
            }

            for (int index = 0; index < container.Splines.Count; index++)
            {
                if (ReferenceEquals(container.Splines[index], spline))
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetSurfaceBounds(out Bounds bounds)
        {
            MeshRenderer renderer = SurfaceRenderer;

            if (renderer == null || surfaceMesh == null || surfaceMesh.vertexCount == 0)
            {
                bounds = default;
                return false;
            }

            bounds = renderer.bounds;
            return true;
        }

        public void SetExternalFoamTexture(Texture texture, float strength)
        {
            SetStatefulFoamTexture(
                texture,
                statefulFoamColor,
                strength,
                0.13f,
                0.025f,
                0.14f,
                0.16f,
                0.20f);
        }

        public void SetStatefulFoamTexture(
            Texture texture,
            Color color,
            float strength,
            float threshold,
            float softness,
            float bandWidth,
            float contactStrength,
            float contactDepth)
        {
            statefulFoamTexture = texture;
            statefulFoamStrength = Mathf.Clamp01(strength);
            statefulFoamColor = color;
            statefulFoamThreshold = Mathf.Clamp01(threshold);
            statefulFoamSoftness = Mathf.Clamp(softness, 0.001f, 0.25f);
            statefulFoamBandWidth = Mathf.Clamp(bandWidth, 0.01f, 0.5f);
            statefulFoamContactStrength = Mathf.Clamp01(contactStrength);
            statefulFoamContactDepth = Mathf.Max(0.001f, contactDepth);

            ApplyVisualSettings();
        }

        // Compatibility bridge for older callers.
        [Obsolete("Use SetExternalFoamTexture(Texture, float) instead.")]
        public void SetDynamicFoamTexture(Texture texture)
        {
            SetExternalFoamTexture(texture, texture != null ? 1f : 0f);
        }

        [Obsolete("Use RegenerateAll or SetExternalFoamTexture instead.")]
        public void RefreshFoamTextureBinding()
        {
            ApplyVisualSettings();
        }

        public void ClearExternalFoamTexture()
        {
            statefulFoamTexture = null;
            statefulFoamStrength = 0f;
            ApplyVisualSettings();
        }

        public void SetPlanarReflectionData(
            Texture texture,
            Matrix4x4 viewProjection,
            float strength,
            float distortion,
            bool available)
        {
            planarReflectionTexture = texture;
            planarReflectionVp = viewProjection;
            planarReflectionStrength = Mathf.Clamp01(strength);
            planarReflectionDistortion = Mathf.Clamp(distortion, 0f, 0.1f);
            planarReflectionAvailable = available && texture != null;
            ApplyBodyProperties();
        }

        public void ClearPlanarReflectionData()
        {
            planarReflectionTexture = null;
            planarReflectionVp = Matrix4x4.identity;
            planarReflectionStrength = 0f;
            planarReflectionDistortion = 0f;
            planarReflectionAvailable = false;
            ApplyBodyProperties();
        }

        private void SynchronizeLegacyMovingTrailFields()
        {
            movingTrailStrength = WakeStrength;
            movingTrailPersistence = Mathf.InverseLerp(
                0.25f,
                3f,
                WakeReach);
            movingTrailWidth = WakeSpread;
        }

        private void ValidateSettings()
        {
            width = Mathf.Max(0.5f, width);
            bankBlend = Mathf.Max(0.1f, bankBlend);
            depth = Mathf.Max(0.1f, depth);
            bedFlatness = Mathf.Clamp01(bedFlatness);
            legacyManualBankOverlap = Mathf.Max(0f, legacyManualBankOverlap);
            additionalShorelineOverlap =
                Mathf.Clamp(additionalShorelineOverlap, 0f, 8f);
            shorelineWetClearance =
                Mathf.Clamp(shorelineWetClearance, 0.005f, 0.5f);
            shorelineBankCover =
                Mathf.Clamp(shorelineBankCover, 0.005f, 0.5f);
            reservedDownwardSurfaceDisplacement =
                Mathf.Clamp(reservedDownwardSurfaceDisplacement, 0f, 1f);
            terrainConformity = Mathf.Clamp01(terrainConformity);
            bedRoughness = Mathf.Clamp(bedRoughness, 0f, 2f);
            bedRoughnessScale = Mathf.Clamp(bedRoughnessScale, 0.5f, 30f);
            bedRoughnessReach = Mathf.Clamp01(bedRoughnessReach);
            shorelineIrregularity =
                Mathf.Clamp(shorelineIrregularity, 0f, 4f);
            shorelineIrregularityScale =
                Mathf.Clamp(shorelineIrregularityScale, 1.5f, 50f);
            bankAsymmetry = Mathf.Clamp01(bankAsymmetry);
            surfaceOffset = Mathf.Clamp(surfaceOffset, 0f, 0.25f);
            domainSampleSpacing = Mathf.Max(0.05f, domainSampleSpacing);

            clarity = Mathf.Clamp01(clarity);
            bodyDepthRange = Mathf.Clamp(bodyDepthRange, 0.1f, 8f);
            bodyDepthContrast = Mathf.Clamp01(bodyDepthContrast);
            waterTintStrength = Mathf.Clamp01(waterTintStrength);
            surfacePresence = Mathf.Clamp01(surfacePresence);

            customFreezeAmount = Mathf.Clamp01(customFreezeAmount);
            iceTransmission = Mathf.Clamp01(iceTransmission);
            iceThickness = Mathf.Clamp01(iceThickness);
            iceCloudiness = Mathf.Clamp01(iceCloudiness);
            iceSurfacePresence = Mathf.Clamp01(iceSurfacePresence);
            iceScattering = Mathf.Clamp01(iceScattering);

            lightDependence = Mathf.Clamp01(lightDependence);
            ambientResponse = Mathf.Clamp(ambientResponse, 0f, 2f);
            sunResponse = Mathf.Clamp(sunResponse, 0f, 2f);
            localLightResponse = Mathf.Clamp(localLightResponse, 0f, 3f);
            lightColorInfluence = Mathf.Clamp01(lightColorInfluence);
            minimumNightVisibility =
                Mathf.Clamp(minimumNightVisibility, 0f, 0.5f);
            shadowResponse = Mathf.Clamp01(shadowResponse);
            liquidSurfaceShadowResponse =
                Mathf.Clamp01(liquidSurfaceShadowResponse);
            iceSurfaceShadowResponse =
                Mathf.Clamp01(iceSurfaceShadowResponse);
            diffuseWrap = Mathf.Clamp01(diffuseWrap);

            flowSpeed = Mathf.Clamp(flowSpeed, 0f, 12f);
            motionWaveHeight = Mathf.Clamp(motionWaveHeight, 0f, 1.25f);
            motionWaveLength = Mathf.Clamp(motionWaveLength, 0.5f, 30f);
            motionWaveSteepness = Mathf.Clamp01(motionWaveSteepness);
            motionDetailStrength = Mathf.Clamp(motionDetailStrength, 0f, 2f);
            motionDetailScale = Mathf.Clamp(motionDetailScale, 0.15f, 12f);
            motionTurbulence = Mathf.Clamp01(motionTurbulence);
            currentAccentStrength = Mathf.Clamp01(currentAccentStrength);
            currentAccentScale = Mathf.Clamp(currentAccentScale, 0.5f, 30f);
            shoreMotion = Mathf.Clamp01(shoreMotion);
            shoreMotionWidth = Mathf.Clamp(shoreMotionWidth, 0.05f, 5f);

            liquidRefractionStrength =
                Mathf.Clamp(liquidRefractionStrength, 0f, 0.02f);
            refractionDepthInfluence =
                Mathf.Clamp01(refractionDepthInfluence);
            refractionNormalInfluence =
                Mathf.Clamp01(refractionNormalInfluence);
            shoreRefraction = Mathf.Clamp01(shoreRefraction);
            depthEdgeProtection = Mathf.Clamp01(depthEdgeProtection);
            iceDistortionStrength =
                Mathf.Clamp(iceDistortionStrength, 0f, 0.012f);
            iceDiffusion = Mathf.Clamp01(iceDiffusion);

            staticPressureStrength = Mathf.Clamp01(
                staticPressureStrength);
            staticPressureContactSharpness = Mathf.Clamp(
                staticPressureContactSharpness,
                0.5f,
                4f);
            staticPressureWaveResponse = Mathf.Clamp(
                staticPressureWaveResponse,
                0f,
                2f);

            if (staticPressureProfileChangeIntervalMin <= 0f &&
                staticPressureProfileChangeIntervalMax <= 0f)
            {
                staticPressureProfileChangeIntervalMin =
                    DefaultStaticPressureProfileChangeIntervalMin;
                staticPressureProfileChangeIntervalMax =
                    DefaultStaticPressureProfileChangeIntervalMax;
            }

            staticPressureProfileChangeIntervalMin = Mathf.Clamp(
                staticPressureProfileChangeIntervalMin,
                MinimumStaticPressureProfileChangeInterval,
                MaximumStaticPressureProfileChangeInterval);
            staticPressureProfileChangeIntervalMax = Mathf.Clamp(
                staticPressureProfileChangeIntervalMax,
                MinimumStaticPressureProfileChangeInterval,
                MaximumStaticPressureProfileChangeInterval);

            if (staticPressureProfileChangeIntervalMin >
                staticPressureProfileChangeIntervalMax)
            {
                (staticPressureProfileChangeIntervalMin,
                    staticPressureProfileChangeIntervalMax) =
                    (staticPressureProfileChangeIntervalMax,
                        staticPressureProfileChangeIntervalMin);
            }

            obstructionWakeStrength = Mathf.Clamp(
                obstructionWakeStrength,
                0f,
                3f);
            obstructionWakeReach = Mathf.Clamp(
                obstructionWakeReach,
                0.25f,
                3f);
            obstructionWakeSpread = Mathf.Clamp(
                obstructionWakeSpread,
                0.5f,
                2f);
            obstructionWakeVariation = Mathf.Clamp01(
                obstructionWakeVariation);
            obstructionWakeVariationIntervalMin = Mathf.Clamp(
                obstructionWakeVariationIntervalMin,
                MinimumStaticWakeVariationInterval,
                MaximumStaticWakeVariationInterval);
            obstructionWakeVariationIntervalMax = Mathf.Clamp(
                obstructionWakeVariationIntervalMax,
                MinimumStaticWakeVariationInterval,
                MaximumStaticWakeVariationInterval);
            if (obstructionWakeVariationIntervalMin >
                obstructionWakeVariationIntervalMax)
            {
                (obstructionWakeVariationIntervalMin,
                    obstructionWakeVariationIntervalMax) =
                    (obstructionWakeVariationIntervalMax,
                        obstructionWakeVariationIntervalMin);
            }
            obstructionWakeWidening = Mathf.Clamp(
                obstructionWakeWidening,
                0.35f,
                1.25f);
            obstructionWakeSurfaceHeight = Mathf.Clamp(
                obstructionWakeSurfaceHeight,
                0f,
                0.40f);
            obstructionWakeSurfaceCompactness = Mathf.Clamp(
                obstructionWakeSurfaceCompactness,
                0.80f,
                3f);
            SynchronizeLegacyMovingTrailFields();
            impactRippleStrength = Mathf.Clamp(
                impactRippleStrength,
                0f,
                3f);
            impactRipplePropagation = Mathf.Clamp(
                impactRipplePropagation,
                0.2f,
                2.5f);
            impactRippleDecay = Mathf.Clamp(
                impactRippleDecay,
                0.1f,
                3f);
            impactRippleTestDistanceNormalized = Mathf.Clamp01(
                impactRippleTestDistanceNormalized);
            impactRippleTestAcrossNormalized = Mathf.Clamp(
                impactRippleTestAcrossNormalized,
                -1f,
                1f);
            if (impactRippleTestEvent.Radius <
                ImpactRippleEventSettings.MinimumRadius)
            {
                impactRippleTestEvent =
                    ImpactRippleEventSettings.CreateEntryDefaults();
            }
            else
            {
                impactRippleTestEvent =
                    impactRippleTestEvent.Sanitized();
            }

            opacity = Mathf.Clamp01(opacity);
            shallowOpacity = Mathf.Clamp01(shallowOpacity);
            deepOpacity = Mathf.Clamp01(deepOpacity);
            depthFadeDistance = Mathf.Max(0.01f, depthFadeDistance);
            depthBands = Mathf.Clamp(depthBands, 0f, 12f);
            horizonPower = Mathf.Clamp(horizonPower, 0.25f, 20f);

            refractionScale = Mathf.Max(0.0001f, refractionScale);
            refractionSpeed = Mathf.Clamp(refractionSpeed, 0f, 2f);
            refractionStrength = Mathf.Clamp(refractionStrength, 0f, 0.05f);

            normalScale = Mathf.Max(0.0001f, normalScale);
            normalSpeed = Mathf.Clamp(normalSpeed, 0f, 2f);
            normalStrength = Mathf.Clamp(normalStrength, 0f, 2f);

            waveScale = Mathf.Clamp(waveScale, 0.15f, 12f);
            waveSpeed = Mathf.Clamp(waveSpeed, 0f, 4f);
            waveHeight = Mathf.Clamp(waveHeight, 0f, 0.5f);
            waveEdgeDampingStart = Mathf.Clamp(waveEdgeDampingStart, 0f, 0.99f);
            waveHeightColorStrength = Mathf.Clamp01(waveHeightColorStrength);

            lightingSmoothness = Mathf.Clamp01(lightingSmoothness);
            lightingHardness = Mathf.Clamp01(lightingHardness);
            specularStrength = Mathf.Clamp(specularStrength, 0f, 4f);
            lightingSteps = Mathf.Clamp(lightingSteps, 1f, 8f);
            visualSeed = Mathf.Clamp(visualSeed, 1, 9999);
            statefulFoamStrength = Mathf.Clamp01(statefulFoamStrength);
        }

        private void CacheComponents()
        {
            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
            }

            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }
        }

        private SplineContainer ResolveSplineContainer()
        {
            if (splineContainer == null)
            {
                splineContainer = GetComponent<SplineContainer>();
            }

            return splineContainer;
        }

        private void AssignWaterLayer()
        {
            int waterLayer = LayerMask.NameToLayer("Water");

            if (waterLayer < 0)
            {
                if (!missingWaterLayerWarningReported)
                {
                    Debug.LogWarning(
                        $"StylizedRiver on '{name}' could not find the required 'Water' layer.",
                        this);
                    missingWaterLayerWarningReported = true;
                }

                return;
            }

            missingWaterLayerWarningReported = false;
            gameObject.layer = waterLayer;
        }

        private void SetRendererEnabled(bool enabled)
        {
            if (meshRenderer != null)
            {
                meshRenderer.enabled = enabled;
            }

            if (corridorMeshRenderer != null)
            {
                corridorMeshRenderer.enabled = enabled;
            }
        }

        private void EnsureDisturbanceRuntime()
        {
            if (disturbanceRuntime == null)
            {
                disturbanceRuntime =
                    GetComponent<StylizedRiverDisturbanceRuntime>();
            }

            if (runtimeDisturbances && disturbanceRuntime == null)
            {
                disturbanceRuntime =
                    gameObject.AddComponent<StylizedRiverDisturbanceRuntime>();
            }

            if (disturbanceRuntime != null)
            {
                disturbanceRuntime.enabled = runtimeDisturbances;
                disturbanceRuntime.NotifyRiverChanged();
            }
        }

        private void EnsureSurfaceOutput()
        {
            CacheComponents();

            if (surfaceMesh == null)
            {
                surfaceMesh = new Mesh
                {
                    name = "PS3D_StylizedRiverSurface",
                    hideFlags = HideFlags.DontSave
                };

                surfaceMesh.MarkDynamic();
            }

            if (meshFilter != null)
            {
                meshFilter.sharedMesh = surfaceMesh;
            }

            if (meshRenderer != null)
            {
                meshRenderer.sharedMaterial = ResolveBodyMaterial();
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                meshRenderer.receiveShadows = true;
                meshRenderer.sortingOrder = 0;
            }
        }

        private void EnsureCorridorOutput()
        {
            if (corridorObject == null)
            {
                Transform existing = transform.Find(CorridorObjectName);

                if (existing != null)
                {
                    corridorObject = existing.gameObject;
                }
            }

            if (!HasRequiredCorridorComponents(corridorObject))
            {
                ReplaceCorridorOutputObject();
            }

            // Re-fetch every reference after creation or replacement. This is
            // intentionally not based on cached component fields: partially
            // generated scene children can survive script reloads and those
            // cached Unity object references may no longer be valid.
            corridorMeshFilter = corridorObject.GetComponent<MeshFilter>();
            corridorMeshRenderer = corridorObject.GetComponent<MeshRenderer>();
            corridorMeshCollider = corridorObject.GetComponent<MeshCollider>();

            if (corridorMeshFilter == null ||
                corridorMeshRenderer == null ||
                corridorMeshCollider == null)
            {
                Debug.LogError(
                    $"StylizedRiver on '{name}' could not create a valid '{CorridorObjectName}' output with MeshFilter, MeshRenderer, and MeshCollider components.",
                    this);
                return;
            }

            if (corridorMesh == null)
            {
                corridorMesh = new Mesh
                {
                    name = "PS3D_StylizedRiverCorridor",
                    hideFlags = HideFlags.DontSave
                };
            }

            if (corridorColliderMesh == null)
            {
                corridorColliderMesh = new Mesh
                {
                    name = "PS3D_StylizedRiverCorridorCollider",
                    hideFlags = HideFlags.DontSave
                };
            }

            corridorMeshFilter.sharedMesh = corridorMesh;
            corridorMeshCollider.convex = false;

            GeneratedGround ground =
                GetComponentInParent<GeneratedGround>();

            if (ground != null)
            {
                corridorObject.layer = ground.gameObject.layer;
                corridorMeshRenderer.sharedMaterial = ground.SharedMaterial;
            }
            else
            {
                corridorObject.layer = 0;
            }

            corridorMeshRenderer.shadowCastingMode =
                ShadowCastingMode.On;
            corridorMeshRenderer.receiveShadows = true;
        }

        private static bool HasRequiredCorridorComponents(GameObject target)
        {
            return target != null &&
                   target.GetComponent<MeshFilter>() != null &&
                   target.GetComponent<MeshRenderer>() != null &&
                   target.GetComponent<MeshCollider>() != null;
        }

        private void ReplaceCorridorOutputObject()
        {
            GameObject previous = corridorObject;

            // Supplying all required types in the constructor avoids exposing a
            // partially assembled corridor child during editor validation and
            // hierarchy callbacks.
            GameObject replacement = new GameObject(
                CorridorObjectName,
                typeof(MeshFilter),
                typeof(MeshRenderer),
                typeof(MeshCollider));

            replacement.transform.SetParent(transform, false);
            replacement.transform.localPosition = Vector3.zero;
            replacement.transform.localRotation = Quaternion.identity;
            replacement.transform.localScale = Vector3.one;

            corridorObject = replacement;
            corridorMeshFilter = replacement.GetComponent<MeshFilter>();
            corridorMeshRenderer = replacement.GetComponent<MeshRenderer>();
            corridorMeshCollider = replacement.GetComponent<MeshCollider>();

            if (previous == null || previous == replacement)
            {
                return;
            }

            MeshFilter previousFilter = previous.GetComponent<MeshFilter>();
            if (previousFilter != null && previousFilter.sharedMesh == corridorMesh)
            {
                previousFilter.sharedMesh = null;
            }

            MeshCollider previousCollider = previous.GetComponent<MeshCollider>();
            if (previousCollider != null &&
                (previousCollider.sharedMesh == corridorMesh ||
                 previousCollider.sharedMesh == corridorColliderMesh))
            {
                previousCollider.sharedMesh = null;
            }

            // Rename first so delayed destruction in Play Mode cannot be found
            // as the active generated corridor on a subsequent lookup.
            previous.name = $"{CorridorObjectName}_Invalid";

            if (Application.isPlaying)
            {
                Destroy(previous);
            }
            else
            {
                DestroyImmediate(previous);
            }
        }

        private Material ResolveBodyMaterial()
        {
            if (bodyMaterial != null &&
                bodyMaterial.shader != null &&
                bodyMaterial.shader.name == CompatibleShaderName)
            {
                incompatibleMaterialWarningReported = false;
                DestroyTemporaryMaterial(ref temporaryBodyMaterial);
                return bodyMaterial;
            }

            if (bodyMaterial != null && !incompatibleMaterialWarningReported)
            {
                Debug.LogWarning(
                    $"StylizedRiver on '{name}' ignored Body Material Override '{bodyMaterial.name}' because it does not use shader '{CompatibleShaderName}'.",
                    this);

                incompatibleMaterialWarningReported = true;
            }

            if (temporaryBodyMaterial != null)
            {
                Shader temporaryShader = temporaryBodyMaterial.shader;

                if (temporaryShader != null &&
                    temporaryShader.name == CompatibleShaderName)
                {
                    return temporaryBodyMaterial;
                }

                // Unity hot reload can preserve private serializable fields. Reject a
                // cached temporary material left over from the deleted river shader.
                DestroyTemporaryMaterial(ref temporaryBodyMaterial);
            }

            Shader shader = Resources.Load<Shader>(BodyShaderResourcePath);

            if (shader == null)
            {
                shader = Shader.Find(CompatibleShaderName);
            }

            if (shader == null)
            {
                Debug.LogError(
                    $"StylizedRiver on '{name}' could not load shader '{CompatibleShaderName}'.",
                    this);
                return null;
            }

            temporaryBodyMaterial = new Material(shader)
            {
                name = "M_PS3D_StylizedRiver_Temporary",
                hideFlags = HideFlags.DontSave
            };

            return temporaryBodyMaterial;
        }

        private void ResolveDefaultTextures()
        {
            if (defaultNormalTexture == null)
            {
                defaultNormalTexture = Resources.Load<Texture2D>(NormalTextureResourcePath);
            }

        }

        private StylizedRiverNaturalVariationSettings
            ResolveNaturalVariationSettings()
        {
            return new StylizedRiverNaturalVariationSettings(
                naturalVariationSeed,
                bedRoughness,
                bedRoughnessScale,
                bedRoughnessReach,
                shorelineIrregularity,
                shorelineIrregularityScale,
                bankAsymmetry);
        }

        private bool ResolveDomainVisibleWidthRange(
            out float minimum,
            out float maximum)
        {
            minimum = float.PositiveInfinity;
            maximum = 0f;

            RiverDomainSnapshot domain = Domain;

            if (domain == null || !domain.IsValid)
            {
                minimum = width;
                maximum = width;
                return false;
            }

            for (int index = 0; index < domain.SampleCount; index++)
            {
                StylizedRiverSplineSample sample = domain.Samples[index];
                float localWidth =
                    sample.LeftHalfWidth + sample.RightHalfWidth;
                minimum = Mathf.Min(minimum, localWidth);
                maximum = Mathf.Max(maximum, localWidth);
            }

            if (float.IsPositiveInfinity(minimum))
            {
                minimum = width;
                maximum = width;
                return false;
            }

            return true;
        }

        private float ResolveGroundGridSpacing()
        {
            GeneratedGround ground =
                GetComponentInParent<GeneratedGround>();

            if (ground != null)
            {
                return Mathf.Max(0.01f, ground.GridSpacing);
            }

            // Rivers are normally children of GeneratedGround. This fallback
            // keeps standalone test rivers valid without inventing a second
            // shoreline system.
            return Mathf.Max(0.25f, domainSampleSpacing);
        }

        private float ResolveAutomaticShorelineOverlap()
        {
            // The dedicated corridor owns the shoreline topology, so overlap no
            // longer scales with the coarse generated-ground grid. It only needs
            // enough hidden bank width to cover the flat water surface safely.
            return Mathf.Clamp(width * 0.08f, 0.20f, 0.75f);
        }

        private void EnsureRiverDomain()
        {
            if (riverDomain == null || !riverDomain.IsValid)
            {
                BuildRiverDomain();
            }
        }

        private void BuildRiverDomain()
        {
            riverDomainVersion++;

            riverDomain =
                StylizedRiverGeometry.BuildDomain(
                    ResolveSplineContainer(),
                    domainSampleSpacing,
                    width,
                    ResolvedShorelineOverlap,
                    surfaceOffset,
                    connectedRiverDistanceOffset,
                    reverseFlow,
                    riverDomainVersion,
                    ResolveNaturalVariationSettings());

            riverLength = Domain.LocalLength;

            if (!Domain.IsValid)
            {
                averageSurfaceHeight = transform.position.y + surfaceOffset;
                DomainChanged?.Invoke(Domain);
                return;
            }

            double heightSum = 0.0;

            for (int index = 0; index < Domain.SampleCount; index++)
            {
                heightSum += Domain.Samples[index].SurfaceHeight;
            }

            averageSurfaceHeight =
                (float)(heightSum / Domain.SampleCount);

            DomainChanged?.Invoke(Domain);
        }

        private void BuildSurface()
        {
            StylizedRiverGeometry.BuildSurfaceMesh(
                transform,
                Domain,
                ResolveCrossSegments(),
                ResolveSurfaceLongitudinalSpacing(),
                Mathf.Max(
                    ResolvedMaximumDownwardMotion,
                    runtimeDisturbances
                        ? StylizedRiverDisturbanceRuntime
                            .MaximumStaticPressureHeightMetres
                        : 0f),
                surfaceMesh);
        }

        private void BuildCorridor()
        {
            EnsureCorridorOutput();

            GeneratedGround ground =
                GetComponentInParent<GeneratedGround>();

            corridorBuildResult =
                StylizedRiverCorridorGeometry.BuildMeshes(
                    transform,
                    Domain,
                    ground,
                    quality,
                    depth,
                    bedFlatness,
                    bankBlend,
                    bankProfile,
                    terrainConformity,
                    shorelineWetClearance,
                    shorelineBankCover,
                    Mathf.Max(
                        reservedDownwardSurfaceDisplacement,
                        ResolvedMaximumDownwardMotion),
                    ResolveNaturalVariationSettings(),
                    corridorMesh,
                    corridorColliderMesh);

            if (corridorMeshCollider != null)
            {
                corridorMeshCollider.sharedMesh = null;

                if (corridorBuildResult.IsValid)
                {
                    corridorMeshCollider.sharedMesh = corridorColliderMesh;
                }
            }

            if (corridorMeshRenderer != null)
            {
                corridorMeshRenderer.enabled =
                    enabled &&
                    corridorBuildResult.IsValid;

                if (ground != null)
                {
                    corridorMeshRenderer.sharedMaterial =
                        ground.SharedMaterial;
                }
            }

            if (corridorBuildResult.TightBendWarning)
            {
                if (!corridorTightBendWarningReported)
                {
                    Debug.LogWarning(
                        $"StylizedRiver on '{name}' contains a bend whose radius is small relative to the generated river width. Inspect the corridor for inside-bank pinching.",
                        this);

                    corridorTightBendWarningReported = true;
                }
            }
            else
            {
                corridorTightBendWarningReported = false;
            }
        }

        private void ApplyVisualSettings()
        {
            EnsureSurfaceOutput();
            ApplyBodyProperties();
        }

        private void ApplyBodyProperties()
        {
            if (meshRenderer == null)
            {
                return;
            }

            ResolveDefaultTextures();

            bodyProperties ??= new MaterialPropertyBlock();
            bodyProperties.Clear();

            bodyProperties.SetColor(ShallowColorId, shallowColor);
            bodyProperties.SetColor(DeepColorId, deepColor);
            bodyProperties.SetFloat(ClarityId, clarity);
            bodyProperties.SetFloat(BodyDepthRangeId, bodyDepthRange);
            bodyProperties.SetFloat(BodyDepthContrastId, bodyDepthContrast);
            bodyProperties.SetFloat(WaterTintStrengthId, waterTintStrength);
            bodyProperties.SetFloat(SurfacePresenceId, surfacePresence);

            bodyProperties.SetFloat(FreezeAmountId, ResolveFreezeAmount());
            bodyProperties.SetColor(IceColorId, iceColor);
            bodyProperties.SetFloat(IceTransmissionId, iceTransmission);
            bodyProperties.SetFloat(IceThicknessId, iceThickness);
            bodyProperties.SetFloat(IceCloudinessId, iceCloudiness);
            bodyProperties.SetFloat(
                IceSurfacePresenceId,
                iceSurfacePresence);
            bodyProperties.SetFloat(IceScatteringId, iceScattering);

            bodyProperties.SetFloat(LightDependenceId, lightDependence);
            bodyProperties.SetFloat(AmbientResponseId, ambientResponse);
            bodyProperties.SetFloat(SunResponseId, sunResponse);
            bodyProperties.SetFloat(
                LocalLightResponseId,
                localLightResponse);
            bodyProperties.SetFloat(
                LightColorInfluenceId,
                lightColorInfluence);
            bodyProperties.SetFloat(
                MinimumNightVisibilityId,
                minimumNightVisibility);
            bodyProperties.SetFloat(ShadowResponseId, shadowResponse);
            bodyProperties.SetFloat(
                LiquidSurfaceShadowResponseId,
                liquidSurfaceShadowResponse);
            bodyProperties.SetFloat(
                IceSurfaceShadowResponseId,
                iceSurfaceShadowResponse);
            bodyProperties.SetFloat(DiffuseWrapId, diffuseWrap);

            bodyProperties.SetTexture(
                MotionDetailTextureId,
                normalTexture != null ? normalTexture : defaultNormalTexture);
            bodyProperties.SetFloat(FlowSpeedMotionId, flowSpeed);
            bodyProperties.SetFloat(MotionWaveHeightId, motionWaveHeight);
            bodyProperties.SetFloat(MotionWaveLengthId, motionWaveLength);
            bodyProperties.SetFloat(MotionWaveSteepnessId, motionWaveSteepness);
            bodyProperties.SetFloat(MotionDetailStrengthId, motionDetailStrength);
            bodyProperties.SetFloat(MotionDetailScaleId, motionDetailScale);
            bodyProperties.SetFloat(MotionTurbulenceId, motionTurbulence);
            bodyProperties.SetFloat(CurrentAccentStrengthMotionId, currentAccentStrength);
            bodyProperties.SetFloat(CurrentAccentScaleMotionId, currentAccentScale);
            bodyProperties.SetFloat(ShoreMotionId, shoreMotion);
            bodyProperties.SetFloat(ShoreMotionWidthId, shoreMotionWidth);
            bodyProperties.SetFloat(MotionDebugViewId, (float)motionDebugView);
            bodyProperties.SetFloat(MotionTimeId, riverTime);
            bodyProperties.SetFloat(MotionSeedId, visualSeed);

            bodyProperties.SetFloat(
                LiquidRefractionStrengthStage4Id,
                liquidRefractionStrength);
            bodyProperties.SetFloat(
                RefractionDepthInfluenceStage4Id,
                refractionDepthInfluence);
            bodyProperties.SetFloat(
                RefractionNormalInfluenceStage4Id,
                refractionNormalInfluence);
            bodyProperties.SetFloat(
                ShoreRefractionStage4Id,
                shoreRefraction);
            bodyProperties.SetFloat(
                RefractionEdgeProtectionStage4Id,
                depthEdgeProtection);
            bodyProperties.SetFloat(
                PreserveObjectSilhouettesStage4Id,
                preserveObjectSilhouettes ? 1f : 0f);
            bodyProperties.SetFloat(
                IceDistortionStrengthStage4Id,
                iceDistortionStrength);
            bodyProperties.SetFloat(
                IceDiffusionStage4Id,
                iceDiffusion);
            bodyProperties.SetFloat(
                RefractionQualityStage4Id,
                quality switch
                {
                    StylizedRiverQuality.Low => 0f,
                    StylizedRiverQuality.Medium => 1f,
                    StylizedRiverQuality.High => 2f,
                    _ => 1f
                });
            bodyProperties.SetFloat(
                RefractionDebugViewStage4Id,
                (float)refractionDebugView);

            // The runtime component re-enables and binds Stage 5 in LateUpdate.
            // Keeping the main river path neutral preserves exact Stage 4 output
            // when disturbances are disabled or unsupported.
            bodyProperties.SetFloat(DisturbanceEnabledStage5Id, 0f);

            bodyProperties.SetFloat(DomainFallbackDepthId, Mathf.Max(0.01f, depth));
            bodyProperties.SetFloat(BodyDebugViewId, (float)bodyDebugView);
            bodyProperties.SetColor(HorizonColorId, horizonColor);
            bodyProperties.SetColor(SpecularColorId, specularColor);
            bodyProperties.SetFloat(OpacityId, opacity);
            bodyProperties.SetFloat(ShallowOpacityId, shallowOpacity);
            bodyProperties.SetFloat(DeepOpacityId, deepOpacity);
            bodyProperties.SetFloat(DepthFadeDistanceId, depthFadeDistance);
            bodyProperties.SetFloat(DepthBandsId, depthBands);
            bodyProperties.SetFloat(UseHsvBlendId, useHsvColorBlend ? 1f : 0f);
            bodyProperties.SetFloat(HorizonPowerId, horizonPower);

            bodyProperties.SetFloat(RefractionScaleId, refractionScale);
            bodyProperties.SetFloat(RefractionSpeedId, refractionSpeed);
            bodyProperties.SetFloat(RefractionStrengthId, refractionStrength);

            bodyProperties.SetTexture(
                NormalTextureId,
                normalTexture != null ? normalTexture : defaultNormalTexture);
            bodyProperties.SetFloat(NormalScaleId, normalScale);
            bodyProperties.SetFloat(NormalSpeedId, normalSpeed);
            bodyProperties.SetFloat(NormalStrengthId, normalStrength);

            bodyProperties.SetColor(SurfaceFoamColorId, statefulFoamColor);
            bodyProperties.SetFloat(SurfaceFoamColorBlendId, 1f);

            bodyProperties.SetFloat(WaveLengthId, waveScale);
            bodyProperties.SetFloat(WaveSpeedId, waveSpeed);
            bodyProperties.SetFloat(WaveSteepnessId, waveHeight);
            bodyProperties.SetVector(WaveDirectionsId, waveDirections);
            bodyProperties.SetFloat(WaveEdgeDampingStartId, waveEdgeDampingStart);
            bodyProperties.SetFloat(WaveHeightColorStrengthId, waveHeightColorStrength);

            bodyProperties.SetFloat(LightingSmoothnessId, lightingSmoothness);
            bodyProperties.SetFloat(LightingHardnessId, lightingHardness);
            bodyProperties.SetFloat(SpecularStrengthId, specularStrength);
            bodyProperties.SetFloat(LightingStepsId, lightingSteps);

            bodyProperties.SetFloat(RiverWidthId, Mathf.Max(0.01f, VisibleWidth));
            bodyProperties.SetFloat(RiverLengthId, Mathf.Max(0.01f, riverLength));
            bodyProperties.SetFloat(FlowDirectionId, FlowDirection);
            bodyProperties.SetFloat(RiverTimeId, riverTime);
            bodyProperties.SetFloat(VisualSeedId, visualSeed);
            bodyProperties.SetFloat(DebugViewId, (float)debugView);

            bodyProperties.SetTexture(
                ExternalFoamFieldId,
                statefulFoamTexture != null
                    ? statefulFoamTexture
                    : Texture2D.blackTexture);
            bodyProperties.SetFloat(ExternalFoamStrengthId, statefulFoamStrength);
            bodyProperties.SetFloat(ExternalFoamThresholdId, statefulFoamThreshold);
            bodyProperties.SetFloat(ExternalFoamSoftnessId, statefulFoamSoftness);
            bodyProperties.SetFloat(ExternalFoamBandWidthId, statefulFoamBandWidth);
            bodyProperties.SetFloat(ExternalFoamContactStrengthId, statefulFoamContactStrength);
            bodyProperties.SetFloat(ExternalFoamContactDepthId, statefulFoamContactDepth);

            bodyProperties.SetTexture(
                PlanarReflectionTextureId,
                planarReflectionTexture != null
                    ? planarReflectionTexture
                    : Texture2D.blackTexture);
            bodyProperties.SetMatrix(PlanarReflectionVpId, planarReflectionVp);
            bodyProperties.SetFloat(
                PlanarReflectionStrengthId,
                planarReflectionStrength);
            bodyProperties.SetFloat(
                PlanarReflectionDistortionId,
                planarReflectionDistortion);
            bodyProperties.SetFloat(
                PlanarReflectionAvailableId,
                planarReflectionAvailable ? 1f : 0f);

            meshRenderer.SetPropertyBlock(bodyProperties);
        }

        private void ApplyAnimationClock()
        {
            if (meshRenderer == null)
            {
                return;
            }

            bodyProperties ??= new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(bodyProperties);
            bodyProperties.SetFloat(RiverTimeId, riverTime);
            bodyProperties.SetFloat(MotionTimeId, riverTime);
            meshRenderer.SetPropertyBlock(bodyProperties);
        }

        private void NotifyFoamSimulationChanged()
        {
            StylizedRiverFoamSimulation simulation =
                GetComponent<StylizedRiverFoamSimulation>();

            if (simulation != null)
            {
                simulation.NotifyRiverChanged();
            }
        }

        private void RequestRegeneration()
        {
            pendingRegeneration = true;
            pendingRegenerationTime =
                Time.realtimeSinceStartupAsDouble + 0.08;
        }

        private void SubscribeToSplineChanges()
        {
            if (subscribedToSplineChanges)
            {
                return;
            }

            Spline.Changed += OnSplineChanged;
            subscribedToSplineChanges = true;
        }

        private void UnsubscribeFromSplineChanges()
        {
            if (!subscribedToSplineChanges)
            {
                return;
            }

            Spline.Changed -= OnSplineChanged;
            subscribedToSplineChanges = false;
        }

        private void OnSplineChanged(
            Spline spline,
            int knotIndex,
            SplineModification modification)
        {
            if (!liveRegeneration || !UsesSpline(spline))
            {
                return;
            }

            RequestRegeneration();
        }

        private bool NotifyParentGround()
        {
            GeneratedGround ground = GetComponentInParent<GeneratedGround>();

            if (ground == null)
            {
                return false;
            }

            ground.NotifyRiverChanged(this);
            return true;
        }

        private void NotifyReflectionSurfaceChanged()
        {
            StylizedRiverPlanarReflection reflection =
                GetComponent<StylizedRiverPlanarReflection>();

            if (reflection != null)
            {
                reflection.RequestRender();
            }
        }

        private void RemoveLegacyGeneratedObjects()
        {
            RemoveGeneratedChild(LegacyCurrentObjectName);
            RemoveGeneratedChild(LegacyStaticFoamObjectName);
        }

        private void RemoveGeneratedChild(string childName)
        {
            Transform child = transform.Find(childName);

            if (child == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }

        private float ResolveImpactRippleMaximumHeight()
        {
            float waveAllowance = motionWaveHeight * 0.20f;
            float strengthAllowance = impactRippleStrength * 0.035f;
            return Mathf.Clamp(
                0.06f + waveAllowance + strengthAllowance,
                0.06f,
                0.28f);
        }

        private float ResolveInteractionMinimumWavelength()
        {
            return quality switch
            {
                StylizedRiverQuality.Low => 1.60f,
                StylizedRiverQuality.Medium => 1.20f,
                StylizedRiverQuality.High => 0.90f,
                _ => 1.20f
            };
        }

        private float ResolveSurfaceLongitudinalSpacing()
        {
            float resolvedSpacing = Mathf.Max(0.05f, domainSampleSpacing);
            float liquidFactor = ResolveLiquidFactor();

            int intervalsPerWave = quality switch
            {
                StylizedRiverQuality.Low => 8,
                StylizedRiverQuality.Medium => 10,
                StylizedRiverQuality.High => 12,
                _ => 10
            };

            if (motionWaveHeight > 0.0001f && liquidFactor > 0.0001f)
            {
                float waveSpacing =
                    motionWaveLength / Mathf.Max(1, intervalsPerWave);
                resolvedSpacing = Mathf.Min(resolvedSpacing, waveSpacing);
            }

            if (runtimeDisturbances && liquidFactor > 0.0001f)
            {
                float disturbanceSpacing =
                    ResolveInteractionMinimumWavelength() /
                    Mathf.Max(1, intervalsPerWave);
                resolvedSpacing = Mathf.Min(
                    resolvedSpacing,
                    disturbanceSpacing);
            }

            return Mathf.Clamp(
                resolvedSpacing,
                0.05f,
                Mathf.Max(0.05f, domainSampleSpacing));
        }

        private int ResolveCrossSegments()
        {
            int baseSegments = quality switch
            {
                StylizedRiverQuality.Low => 6,
                StylizedRiverQuality.Medium => 12,
                StylizedRiverQuality.High => 20,
                _ => 12
            };

            if (!runtimeDisturbances || ResolveLiquidFactor() <= 0.0001f)
            {
                return baseSegments;
            }

            int intervalsPerDisturbance = quality switch
            {
                StylizedRiverQuality.Low => 5,
                StylizedRiverQuality.Medium => 7,
                StylizedRiverQuality.High => 9,
                _ => 7
            };
            int maximumSegments = quality switch
            {
                StylizedRiverQuality.Low => 24,
                StylizedRiverQuality.Medium => 40,
                StylizedRiverQuality.High => 64,
                _ => 40
            };
            float targetSpacing =
                ResolveInteractionMinimumWavelength() /
                Mathf.Max(1, intervalsPerDisturbance);
            int requiredSegments = Mathf.CeilToInt(
                GeneratedSurfaceWidth / Mathf.Max(0.05f, targetSpacing));

            return Mathf.Clamp(
                Mathf.Max(baseSegments, requiredSegments),
                baseSegments,
                maximumSegments);
        }

        private static void DestroyTemporaryMaterial(ref Material material)
        {
            if (material == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(material);
            }
            else
            {
                DestroyImmediate(material);
            }

            material = null;
        }

        private void OnDestroy()
        {
            UnsubscribeFromSplineChanges();

            if (meshFilter != null && meshFilter.sharedMesh == surfaceMesh)
            {
                meshFilter.sharedMesh = null;
            }

            if (surfaceMesh != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(surfaceMesh);
                }
                else
                {
                    DestroyImmediate(surfaceMesh);
                }

                surfaceMesh = null;
            }

            if (corridorMeshCollider != null &&
                (corridorMeshCollider.sharedMesh == corridorMesh ||
                 corridorMeshCollider.sharedMesh == corridorColliderMesh))
            {
                corridorMeshCollider.sharedMesh = null;
            }

            if (corridorMeshFilter != null &&
                corridorMeshFilter.sharedMesh == corridorMesh)
            {
                corridorMeshFilter.sharedMesh = null;
            }

            if (corridorMesh != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(corridorMesh);
                }
                else
                {
                    DestroyImmediate(corridorMesh);
                }

                corridorMesh = null;
            }

            if (corridorColliderMesh != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(corridorColliderMesh);
                }
                else
                {
                    DestroyImmediate(corridorColliderMesh);
                }

                corridorColliderMesh = null;
            }

            if (corridorObject != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(corridorObject);
                }
                else
                {
                    DestroyImmediate(corridorObject);
                }

                corridorObject = null;
            }

            DestroyTemporaryMaterial(ref temporaryBodyMaterial);
            RemoveLegacyGeneratedObjects();
        }
    }
}
