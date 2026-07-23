using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.Splines;
#if UNITY_EDITOR
using UnityEditor;
#endif
using ProgrammaticStylized3D.Geometry;
using ProgrammaticStylized3D.Geometry.Ground;

namespace ProgrammaticStylized3D.Rivers
{
    public enum StylizedRiverQuality
    {
        Low,
        Medium,
        High
    }

    public enum StylizedRiverFoamGridMode
    {
        FixedMetric = 0,
        LegacyNormalizedAcross = 1
    }

    public enum StylizedRiverFoamFixedMetricCellSize
    {
        [InspectorName("Quality Default")]
        QualityDefault = 0,
        [InspectorName("0.25 m")]
        Metres0_25 = 1,
        [InspectorName("0.20 m")]
        Metres0_20 = 2,
        [InspectorName("0.15 m")]
        Metres0_15 = 3,
        [InspectorName("0.10 m")]
        Metres0_10 = 4
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

    public enum StylizedRiverFoamSourcePopulationPreset
    {
        Custom,
        ShoreContactTest,
        RiverBodyTest,
        ObstacleContactTest,
        LeeWakeTest,
        BalancedMixedTest,
        Off
    }

    public enum StylizedRiverFoamShorePattern
    {
        Mixed,
        ShoreRibbons,
        InwardWash
    }

    public enum StylizedRiverFoamObjectPattern
    {
        Mixed = 0,
        ContactArcs = 1,
        ContactSemiArcs = 3,
        ContactFlecks = 2
    }

    public enum StylizedRiverFoamFreeWaterPattern
    {
        Mixed = 0,
        LaceConnectors = 1,
        CrossLaceConnectors = 3,
        TornFragments = 2
    }

    public enum StylizedRiverFinalFoamVisibilityMode
    {
        [InspectorName("Concentration + Lifetime")]
        ConcentrationAndLifetime = 0,
        [InspectorName("Lifecycle-Faithful")]
        LifecycleFaithful = 1
    }

    public enum StylizedRiverFoamPresenceFootprintMode
    {
        [InspectorName("Coverage-Only")]
        Current = 0,
        [InspectorName("Presence-Amplitude")]
        PresenceAmplitude = 1
    }


    public enum StylizedRiverFoamTransportScheme
    {
        [InspectorName("Donor Cell")]
        DonorCell = 0,
        [InspectorName("TVD Superbee")]
        TvdSuperbee = 1
    }

    public enum StylizedRiverFoamBirthShapeMode
    {
        Ellipse,
        Stroke,
        Compound
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

    public enum StylizedRiverFoamDebugView
    {
        // Zero is the exact normal rendered result and debug-off state.
        Final = 0,
        FoamAndAgingTopology = 1,
        AutomaticBirthSources = 2,
        MaterialPresence = 3,
        MaterialRemainingLife = 4,
        FoamMotionField = 5,
        FoamMotionFieldCellGrid = 6,
        FoamEvaluatedShape = 7,
        FoamShapeDifference = 8,
        FoamChipAndStrandProbe = 9,
        FoamChipAndStrandDifference = 10,
        FoamFilmSource = 11,
        FoamFilmSupport = 12,
        FoamFilmTarget = 13,
        FoamTemporalOccupancy = 14,
        FoamTemporalDifference = 15,
        // Numeric values remain explicit for serialized compatibility.
        FoamEvaluatedFinalPreview = 17,
        ChipCandidateField = 18,
        ProductionChipMask = 25,
        ChipEligibilityComposite = 26,
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
        RippleBoundary = 21,
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

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SplineContainer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class StylizedRiver : MonoBehaviour
    {
        private enum RiverRegenerationRequestOrigin
        {
            OnEnable,
            ExplicitRegenerateAll,
            RebuildSurfaceOnly,
            GroundCorridorChanged,
            InspectorStructural,
            SplineChanged,
            ChannelPreset,
            Count
        }

#if UNITY_EDITOR
        private enum RiverEditorRegenerationPassKind
        {
            Full,
            SurfaceOnly,
            CorridorOnly,
            Count
        }

        private sealed class RiverEditorRegenerationBatch
        {
            public int Id;
            public double StartedAt;
            public int StartFrame;
            public int EndFrame;
            public int RequestCount;
            public readonly int[] RequestOrigins =
                new int[(int)RiverRegenerationRequestOrigin.Count];
            public int CoalescedRequestCount;
            public int PassCount;
            public readonly int[] PassKinds =
                new int[(int)RiverEditorRegenerationPassKind.Count];
            public double TotalPassMilliseconds;
            public int DomainBuilds;
            public int DomainFirstObservations;
            public int DomainContentChanges;
            public int DomainUnchangedRebuilds;
            public int DomainVersionIncrements;
            public int DomainPublications;
            public bool HasDomainFingerprint;
            public GeneratedGeometryStableFingerprint DomainPreviousFingerprint;
            public GeneratedGeometryStableFingerprint DomainLatestFingerprint;
            public int SurfaceBuilds;
            public int CorridorBuilds;
            public int CorridorColliderAssignments;
            public int GroundSnapshotBuilds;
            public int GroundSnapshotFirstObservations;
            public int GroundSnapshotContentChanges;
            public int GroundSnapshotUnchangedBuilds;
            public bool HasGroundSnapshotFingerprint;
            public GeneratedGeometryStableFingerprint GroundSnapshotPreviousFingerprint;
            public GeneratedGeometryStableFingerprint GroundSnapshotLatestFingerprint;
            public int GroundNotifications;
            public int GroundNotificationMisses;
            public int GroundDeferredNotifications;
            public int FoamNotifications;
            public int ReflectionRequests;
            public readonly List<string> Timeline = new List<string>(32);
            public int DroppedTimelineEvents;
        }
#endif
        public const string CompatibleShaderName =
            "PS3D/Stylized River Water";

        public const float MinimumStaticPressureFrontReachMetres = 0.02f;
        public const float DefaultStaticPressureFrontReachMetres = 0.12f;
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

        private const int CurrentFoamMaterialLifecycleTuningVersion = 1;
        private const int CurrentFoamVelocityTuningVersion = 1;
        private const float MinimumFoamNeutralLifetime = 1f;
        private const float MaximumFoamNeutralLifetime = 20f;
        private const float DefaultFoamNeutralLifetime = 4f;
        private const float MinimumFoamSupportedAgingRate = 0.05f;
        private const float MaximumFoamSupportedAgingRate = 1f;
        private const float DefaultFoamSupportedAgingRate = 0.2f;
        private const float MinimumFoamFullSupportedAgingAt = 0.15f;
        private const float MaximumFoamFullSupportedAgingAt = 1f;
        private const float DefaultFoamFullSupportedAgingAt = 0.92f;
        private const float MinimumFoamNegativeAgingRate = 1f;
        private const float MaximumFoamNegativeAgingRate = 20f;
        private const float DefaultFoamNegativeAgingRate = 4f;
        private const float MinimumFoamDownstreamSpeedRatio = 0f;
        private const float MaximumFoamDownstreamSpeedRatio = 2f;
        private const float DefaultFoamDownstreamSpeedRatio = 1f;
        private const float MinimumShoreFoamFormationSpeedMetresPerSecond = 0.15f;
        private const float MaximumShoreFoamFormationSpeedMetresPerSecond = 2.5f;
        private const float DefaultShoreFoamFormationSpeedMetresPerSecond = 0.75f;
        private const float MinimumFoamPacketGapMetres = 0f;
        private const float MaximumFoamPacketGapMetres = 10f;
        private const float DefaultShoreFoamPacketGapMetres = 0.75f;
        private const float DefaultObjectContactPacketGapMetres = 1.00f;
        private const int MinimumObjectContactStrokeCount = 1;
        private const int MaximumObjectContactStrokeCount = 3;
        private const int DefaultObjectContactStrokeCount = 2;
        private const float DefaultFreeWaterFoamPacketGapMetres = 1.00f;
        private const float MinimumFoamMaximumLateralSpeedRatio = 0f;
        private const float MaximumFoamMaximumLateralSpeedRatio = 1f;
        private const float DefaultFoamMaximumLateralSpeedRatio = 0.22f;
        private const float LegacyDefaultFoamMotionFieldStrength = 1f;
        private const float MinimumFoamLaneAdvectionRatio = 0f;
        private const float MaximumFoamLaneAdvectionRatio = 1f;
        private const float DefaultFoamLaneAdvectionRatio = 0.60f;
        private const float LegacyDefaultFoamMotionFieldScrollHz = 0.01f;
        private const float MinimumFoamLowLateralMotionCoverage = 0f;
        private const float MaximumFoamLowLateralMotionCoverage = 0.30f;
        private const float DefaultFoamLowLateralMotionCoverage = 0.10f;
        private const float MinimumFoamDirectionChangeFrequency = 0.25f;
        private const float MaximumFoamDirectionChangeFrequency = 4f;
        private const float DefaultFoamDirectionChangeFrequency = 1f;
        private const float MinimumFoamAcrossRiverCoherence = 0.5f;
        private const float MaximumFoamAcrossRiverCoherence = 4f;
        private const float DefaultFoamAcrossRiverCoherence = 1f;
        private const float MinimumFoamObstacleSlowdownStrength = 0f;
        private const float MaximumFoamObstacleSlowdownStrength = 1f;
        private const float DefaultFoamObstacleSlowdownStrength = 0.85f;
        private const float MinimumFoamObstacleMinimumDownstreamFactor = 0f;
        private const float MaximumFoamObstacleMinimumDownstreamFactor = 1f;
        private const float DefaultFoamObstacleMinimumDownstreamFactor = 0.12f;
        private const float MinimumFoamVisualOccupancyBuildTime = 0.02f;
        private const float MaximumFoamVisualOccupancyBuildTime = 2f;
        private const float DefaultFoamVisualOccupancyBuildTime = 0.20f;
        private const float MinimumFoamVisualOccupancyReleaseTime = 0.05f;
        private const float MaximumFoamVisualOccupancyReleaseTime = 4f;
        private const float DefaultFoamVisualOccupancyReleaseTime = 0.80f;
        private const float DefaultFoamStrandScale = 0.55f;
        private const float DefaultFoamStrandDensity = 0.50f;
        private const float DefaultFoamStrandReach = 0.55f;
        private const float MinimumFoamChipCandidateSpacing = 0.10f;
        private const float MaximumFoamChipCandidateSpacing = 3.00f;
        private const float DefaultFoamChipCandidateSpacing = 1.15f;
        // Chip Size is authored as one bounded 0-1 control. Internally it
        // maps to a radius-to-spacing ratio so the adaptive candidate search
        // retains its proven 3x3 through 5x11 cost ceiling.
        private const float DefaultFoamChipSize = 0.3152174f;
        private const float DefaultFoamChipIrregularity = 1f;
        private const float MinimumFoamChipStableScreenRadiusPixels = 0f;
        private const float MaximumFoamChipStableScreenRadiusPixels = 16f;
        private const float DefaultFoamChipStableScreenRadiusPixels = 2f;
        private const float MinimumFoamChipMaximumViewScale = 1f;
        private const float MaximumFoamChipMaximumViewScale = 2.5f;
        private const float DefaultFoamChipMaximumViewScale = 1.75f;
        private const float DefaultFoamChipEdgeWidthPixels = 4f;
        private const float MinimumFoamChipSoftEdgeStart = 0f;
        private const float MaximumFoamChipSoftEdgeStart = 0.25f;
        private const float DefaultFoamChipSoftEdgeStart = 0.06f;
        private const float DefaultFoamChipInteriorAccess = 0f;
        private const float MinimumFoamChipFieldSpeed = 0f;
        private const float MaximumFoamChipFieldSpeed = 12f;
        private const float DefaultFoamChipFieldSpeed = 0f;
        private const float MinimumFoamChipLifecycleTime = 0.25f;
        private const float MaximumFoamChipLifecycleTime = 30f;
        private const float DefaultFoamChipFormationTime = 2.5f;
        private const float DefaultFoamChipStableTime = 5f;
        private const float DefaultFoamChipDissolveTime = 2.5f;
        private const float DefaultFoamChipDormantTime = 4f;
        private const float MinimumFoamChipLateralMotionAmount = 0f;
        private const float MaximumFoamChipLateralMotionAmount = 2.5f;
        private const float DefaultFoamChipLateralMotionAmount = 0f;
        private const float MinimumFoamChipMotionSpeed = 0f;
        private const float MaximumFoamChipMotionSpeed = 1f;
        private const float DefaultFoamChipLateralMotionSpeed = 0.04f;
        private const float MinimumFoamChipRotationAmountDegrees = 0f;
        private const float MaximumFoamChipRotationAmountDegrees = 180f;
        private const float DefaultFoamChipRotationAmountDegrees = 0f;
        private const float DefaultFoamChipRotationSpeed = 0.04f;
        private const float MinimumFoamChipSizePulseAmount = 0f;
        private const float MaximumFoamChipSizePulseAmount = 0.45f;
        private const float DefaultFoamChipSizePulseAmount = 0f;
        private const float DefaultFoamChipSizePulseSpeed = 0.06f;
        private const float DefaultFoamChipShapeChangeAmount = 0f;
        private const float DefaultFoamChipShapeChangeSpeed = 0.04f;
        private const float MinimumFoamChipShapeTransitionTime = 0.10f;
        private const float MaximumFoamChipShapeTransitionTime = 30f;
        private const float DefaultFoamChipShapeTransitionTime = 4f;
        private const float MinimumFoamProgressiveRibbonDuration = 0.5f;
        private const float MaximumFoamProgressiveRibbonDuration = 5f;
        private const float DefaultFoamProgressiveRibbonDuration = 2.4f;
        private const float MinimumFoamProgressiveRibbonTravelDistance = 0.5f;
        private const float MaximumFoamProgressiveRibbonTravelDistance = 8f;
        private const float DefaultFoamProgressiveRibbonTravelDistance = 3f;
        private const float MinimumFoamProgressiveRibbonAcrossDrift = -1f;
        private const float MaximumFoamProgressiveRibbonAcrossDrift = 1f;
        private const float DefaultFoamProgressiveRibbonAcrossDrift = 0.25f;
        private const float MinimumFoamProgressiveRibbonPathWander = 0f;
        private const float MaximumFoamProgressiveRibbonPathWander = 1f;
        private const float DefaultFoamProgressiveRibbonPathWander = 0.35f;
        private const float FoamSpawnMaximumBendAcross = 0.35f;
        private const float MinimumFoamSpawnScale = 0.03f;
        private const float MaximumFoamSpawnScale = 1.25f;
        private const float DefaultFoamSpawnScale = 0.18f;


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

        [Tooltip("Vertical shore-wave amplitude relative to the centre-river macro wave. A value of 1 preserves the existing wave height.")]
        [Range(0f, 2.5f)]
        [SerializeField] private float shoreWaveHeightScale = 1f;

        [Tooltip("Longitudinal shore-wave length relative to the centre-river macro wave. A value of 1 preserves the existing wavelength.")]
        [Range(0.25f, 4f)]
        [SerializeField] private float shoreWaveLengthScale = 1f;

        [Tooltip("Maximum fraction of the hidden shoreline allowance that shore waves may reach. A value of 1 preserves the complete generated allowance.")]
        [Range(0f, 1f)]
        [SerializeField] private float shoreWaveReach = 1f;

        [Tooltip("World-space smoothing distance for the shore-wave profile and for transitions between neighbouring waves with different sizes. Larger values produce broader, rounder shoreline transitions.")]
        [Range(0.25f, 3f)]
        [SerializeField] private float shoreWaveTransitionLength = 1f;

        [Tooltip("Stable deterministic size differences between successive shore waves. Zero keeps every wave at the same overall size; higher values make some waves taller and farther-reaching than others without live reseeding.")]
        [Range(0f, 1f)]
        [SerializeField] private float shoreWaveSizeVariation;

        [Tooltip("How independently the same shore wave varies on the left and right banks. This affects both Size Variation and Profile Variation.")]
        [Range(0f, 1f)]
        [SerializeField] private float shoreWaveSideAsymmetry;

        [Tooltip("Per-wave start, middle, and end variation for shore-wave height and lateral reach. Zero preserves the former uniform repeating wave.")]
        [Range(0f, 1f)]
        [SerializeField] private float shoreWaveProfileVariation;

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
        [Tooltip("Master switch for Stage 5 Pressure, Wake, and Impact Ripples. When disabled, no disturbance textures are allocated or simulated and the river renders with its Stage 4 behavior.")]
        [SerializeField] private bool runtimeDisturbances;

        // Retained for serialized/API compatibility with the earlier combined
        // disturbance preset. It is intentionally hidden because Stage 5 is now
        // authored per feature.
        [HideInInspector]
        [SerializeField]
        private StylizedRiverDisturbancePreset disturbancePreset =
            StylizedRiverDisturbancePreset.Custom;

        [Tooltip("Selects a diagnostic visualization for Stage 5 source fields, persistent fields, or final disturbance geometry. This changes only the debug display, not the simulation.")]
        [SerializeField]
        private StylizedRiverDisturbanceDebugView disturbanceDebugView =
            StylizedRiverDisturbanceDebugView.Final;

        [Header("Pressure")]
        [Tooltip("Selects how much of each source's computed safe Pressure-height range is used. Zero removes attached buildup; one uses the source's maximum geometry-, support-, and flow-safe height without bypassing rear protection.")]
        [Range(0f, 1f)]
        [SerializeField] private float staticPressureStrength = 0.65f;

        [Tooltip("Requested open-water Pressure distance upstream from the physical obstacle contact, in metres. The runtime reports the quality-dependent resolved distance after applying the minimum raster floor.")]
        [Min(MinimumStaticPressureFrontReachMetres)]
        [SerializeField]
        private float staticPressureFrontReachMetres =
            DefaultStaticPressureFrontReachMetres;

        [Tooltip("Shapes the open-water falloff inside Front Reach. Lower values make a softer ridge; higher values keep it steeper. This does not change total reach or the computed crest-height ceiling.")]
        [Range(0.5f, 4f)]
        [SerializeField] private float staticPressureContactSharpness = 2.8f;

        [Tooltip("Controls deterministic lateral reshaping of the Pressure ridge. Zero keeps the cached geometry-derived profile fixed; one gives the normal variation range; two permits the strongest bounded redistribution. This is independent from Stage 3 waves.")]
        [Range(0f, 2f)]
        [SerializeField] private float staticPressureWaveResponse = 1f;

        [Tooltip("Shortest randomized delay, in seconds, before a stationary Pressure source chooses a new lateral profile target. The profile morphs smoothly rather than switching instantly.")]
        [Range(
            MinimumStaticPressureProfileChangeInterval,
            MaximumStaticPressureProfileChangeInterval)]
        [SerializeField]
        private float staticPressureProfileChangeIntervalMin =
            DefaultStaticPressureProfileChangeIntervalMin;

        [Tooltip("Longest randomized delay, in seconds, before a stationary Pressure source chooses a new lateral profile target. Each source selects independently, and the smooth morph completes before the next target.")]
        [Range(
            MinimumStaticPressureProfileChangeInterval,
            MaximumStaticPressureProfileChangeInterval)]
        [SerializeField]
        private float staticPressureProfileChangeIntervalMax =
            DefaultStaticPressureProfileChangeIntervalMax;

        [Header("Wake")]
        [Tooltip("Controls the shared Wake response after source preparation. Zero removes the authored lee/release response; higher values deepen the attached lee and inject more transported wake energy. Stationary geometry and dynamic emitters use the same river-level value.")]
        [Range(0f, 3f)]
        [SerializeField] private float obstructionWakeStrength = 1.50f;

        [Tooltip("Controls how far the prepared Wake source and its retained energy are allowed to influence downstream water. Higher values extend source persistence and active range; this does not change river flow speed.")]
        [Range(0.25f, 3f)]
        [SerializeField] private float obstructionWakeReach = 1f;

        [Tooltip("Controls the initial across-river width of the Wake source. Stationary geometry uses it for the attached lee and rear releases; dynamic emitters use it for their swept footprint. It does not control downstream diffusion—that is Widening.")]
        [Range(0.5f, 2f)]
        [SerializeField] private float obstructionWakeSpread = 1f;

        [Tooltip("Controls the allowed spatial change in Wake source shape. Zero keeps stationary lee/release geometry stable; one permits the full bounded variation range. It does not pulse or globally brighten the persistent field.")]
        [Range(0f, 1f)]
        [SerializeField] private float obstructionWakeVariation = 0.35f;

        [Tooltip("Shortest randomized delay, in seconds, before a stationary Wake source chooses new lee and left/right release targets.")]
        [Range(
            MinimumStaticWakeVariationInterval,
            MaximumStaticWakeVariationInterval)]
        [SerializeField]
        private float obstructionWakeVariationIntervalMin =
            DefaultStaticWakeVariationIntervalMin;

        [Tooltip("Longest randomized delay, in seconds, before a stationary Wake source chooses new lee and left/right release targets. Smooth transitions occupy about 85% of the chosen interval.")]
        [Range(
            MinimumStaticWakeVariationInterval,
            MaximumStaticWakeVariationInterval)]
        [SerializeField]
        private float obstructionWakeVariationIntervalMax =
            DefaultStaticWakeVariationIntervalMax;

        [Tooltip("Controls lateral diffusion after Wake energy enters the shared persistent field. Lower values keep trails narrow for longer; higher values broaden and merge them sooner. This does not change the initial source width.")]
        [Range(0.35f, 1.25f)]
        [SerializeField] private float obstructionWakeWidening = 0.65f;

        [Tooltip("Maximum positive water-surface displacement, in metres, extracted from the compact core of transported Wake energy. Zero preserves Wake transport, normals, and intensity but adds no positive transported Wake height.")]
        [Range(0f, 0.40f)]
        [SerializeField] private float obstructionWakeSurfaceHeight = 0.08f;

        [Tooltip("Controls which part of the broad transported Wake field becomes positive geometry. Lower values turn more of the field into a broad visible rise; higher values restrict height to the strongest core. Transport, normals, intensity, and future foam data are unchanged.")]
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
        [Tooltip("Master multiplier for Impact Ripple height, velocity, initial elevation, and normal detail. The nonlinear response maps 0.5 to about the former 1.4, 1 to about 2.6, and 3 to about 5.4. Values from 0–1.5 are the normal authoring range, 2–3 are exaggerated stress settings, and 4 is an intentional override level for exceptional impacts.")]
        [Range(0f, 4f)]
        [SerializeField] private float impactRippleStrength = 1f;

        [Tooltip("Emphasizes only the raised ripple ridge: its positive height, outward velocity, and normal-detail edge. It does not deepen the centre, change radius, propagation speed, decay, reflections, or initial elevation. Values above 1 make the ring slightly sharper and more pronounced.")]
        [Range(0.75f, 1.50f)]
        [SerializeField] private float impactRippleRidgeEmphasis = 1.15f;

        [Tooltip("Approximate world-space wavefront expansion speed in metres per second. This controls radial spreading through the local river metrics; river Flow Speed separately advects the ripple downstream.")]
        [Range(0.2f, 2.5f)]
        [SerializeField] private float impactRipplePropagation = 1.05f;

        [Tooltip("Base exponential Impact Ripple loss per second. Effective Decay = Decay + abs(Flow Speed) × Flow Dissipation. Higher values shorten visible ripple lifetime even in still water.")]
        [Range(0.1f, 3f)]
        [SerializeField] private float impactRippleDecay = 0.85f;

        [Tooltip("Adds decay in direct proportion to river speed: abs(Flow Speed in m/s) × this value. Example: Decay 0.85, Flow Speed 2 m/s, and Flow Dissipation 0.15 produce Effective Decay 1.15/s. Set to zero when fast flow should only advect, not additionally suppress, ripples.")]
        [Range(0f, 1.5f)]
        [SerializeField] private float impactRippleFlowDissipation = 0.15f;

        [Tooltip("CPU-side reservation threshold, not a direct visual clamp. When a predicted event envelope falls below this value, its future chunk reservation may end. Lower values keep simulation coverage longer; higher values save work but can retire very faint tails sooner.")]
        [Range(0.01f, 0.20f)]
        [SerializeField] private float impactRippleMinimumVisibleEnergy = 0.04f;

        [Tooltip("Hard safety cap, in seconds, on how long one event may reserve future chunks. After this time the reservation ends even if the analytic envelope remains above Minimum Visible Energy, so values that are too low can clip extreme low-decay ripples.")]
        [Range(1f, 15f)]
        [SerializeField] private float impactRippleMaximumLifetime = 8f;

        [Tooltip("Controls shoreline boundary hardness after the shallow absorption band. Zero uses the most absorbing outgoing-wave response; higher values move toward a harder no-flux reflection and make the broad return wave clearer. This is not a literal returned-energy percentage because shoreline absorption and normal ripple decay still apply.")]
        [Range(0f, 0.60f)]
        [SerializeField] private float impactRippleShoreReflection = 0.25f;

        [Tooltip("Controls registered-solid boundary hardness. Zero uses the most absorbing outgoing-wave response; higher values move toward a hard no-flux reflection. This is not a literal returned-energy percentage because obstacle-edge absorption and normal ripple decay still apply.")]
        [Range(0f, 0.85f)]
        [SerializeField] private float impactRippleObstacleReflection = 0.50f;

        [HideInInspector, SerializeField, Range(0f, 1f)]
        private float impactRippleTestDistanceNormalized = 0.5f;

        [HideInInspector, SerializeField, Range(-1f, 1f)]
        private float impactRippleTestAcrossNormalized;

        [HideInInspector, SerializeField]
        private ImpactRippleEventSettings impactRippleTestEvent =
            ImpactRippleEventSettings.CreateEntryDefaults();

        [Header("Foam and Surface Tracing")]
        [Tooltip("Master switch for the Stage 6 shared persistent Foam field. When disabled, no Foam textures are allocated or simulated and the water shader receives a neutral Foam input.")]
        [SerializeField] private bool foamEnabled;

        [Tooltip("Selects the active Foam coordinate contract. Fixed Metric is the P12 test default. Legacy Normalized Across remains available for direct A/B comparison and rollback.")]
        [SerializeField]
        private StylizedRiverFoamGridMode foamGridMode =
            StylizedRiverFoamGridMode.FixedMetric;

        [Tooltip("Selects the requested fixed-metric cell size. Quality Default resolves Low to 0.25 m, Medium to 0.15 m, and High to 0.10 m. Changing this or Grid Mode invalidates the current Foam resources when the resolved descriptor changes and then requires a matching topology-cache rebuild.")]
        [SerializeField]
        private StylizedRiverFoamFixedMetricCellSize
            foamFixedMetricCellSize =
                StylizedRiverFoamFixedMetricCellSize.QualityDefault;

        [Tooltip("Selects how Layer C transports geometric Coverage. Donor Cell is the conservative first-order baseline and is more numerically diffuse. TVD Superbee reconstructs bounded Coverage at faces to retain sharper footprints while transporting one coherent material state. Neither scheme is permitted to alter decoded intrinsic Presence or Remaining Life merely because material moved. This changes no allocation or topology contract and may be switched during Play Mode.")]
        [SerializeField]
        private StylizedRiverFoamTransportScheme foamTransportScheme =
            StylizedRiverFoamTransportScheme.DonorCell;

        [Tooltip("Persistent prepared-topology cache associated with this authored river. Exact caches load directly. Stale-compatible caches may be used for one Play session without replacement; missing or incompatible caches require explicit Edit Mode preparation and are never generated or saved automatically during Play.")]
        [SerializeField]
        private StylizedRiverFoamTopologyCacheAsset foamTopologyCacheAsset;

        [Tooltip("Controls the nested deterministic population of whole-river Major Support. Higher values activate later-ranked opportunities without changing the identity or transform of earlier accepted regions. It does not alter the separate local candidate preview.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamMajorSupportAmount = 0.56f;

        [Tooltip("Controls the physical scale envelope of the same stable whole-river Major opportunities. It preserves opportunity identity and does not enlarge the separate local candidate preview or change candidate field resolution.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamMajorSupportSize = 0.46f;

        [Tooltip("Controls the relative size spread between stable Major opportunities without changing their identity. Zero makes their scale multipliers uniform, 0.5 preserves the Patch 2 distribution, and one strongly separates the smallest and largest regions while river-width and placement limits remain authoritative.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamMajorSupportSizeVariation = 0.5f;

        [Tooltip("Controls how far a Major may respawn longitudinally from its original accepted river position when its occurrence recycles. The value is a percentage of valid river length applied in both directions. Near-egress originals retain an upstream runway safeguard so they cannot become trapped at the chunk end.")]
        [Range(0f, 10f)]
        [SerializeField]
        private float foamMajorRecycleTerritoryDeviationPercent = 3f;

        [Tooltip("Average combined lifetime budget for one evolving Major occurrence. Approximately one normal dwell-plus-move cycle consumes one unit through both elapsed time and completed hops. Higher values keep an occurrence alive longer before it recycles inside its local territory.")]
        [Range(1f, 20f)]
        [SerializeField] private float foamMajorLifetimeUnits = 6f;

        [Tooltip("Deterministic plus-or-minus variation applied to Major Lifetime Units for each recycled occurrence. A base of 6 and deviation of 2 allocates approximately 4–8 units, with an enforced minimum of one unit.")]
        [Range(0f, 10f)]
        [SerializeField] private float foamMajorLifetimeUnitDeviation = 2f;

        [Tooltip("Deterministic seed for both the field-first candidate proof and stable whole-river opportunity identity, candidate assignment, transforms, and future evolution metadata. The same inputs reproduce the same static Major topology.")]
        [Min(0)]
        [SerializeField] private int foamMajorSupportSeed = 1;

        [Tooltip("Controls how many eligible Major-to-Major relationships become Connector Support. Zero keeps only the strongest sparse relationships, 0.5 preserves the Patch 3 result, and one permits more secondary connections and bounded overlap without becoming an all-to-all web.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamConnectorAmount = 0.5f;

        [Tooltip("Controls how directly Connector Support joins Major regions. One preserves near-facing endpoints and the shortest valid route. Lower values deliberately broaden endpoint choice and force one stable broad lateral bend when valid, without adding random wiggle.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamConnectorDirectness = 1f;

        [Tooltip("Controls which Connector lengths are favoured within one fixed safe relationship envelope. Zero strongly favours short connections, 0.5 applies no short-versus-long preference, and one strongly favours long connections. It does not remove obstacle, valid-water, path-length, or connection-count limits.")]
        [FormerlySerializedAs("foamConnectorReach")]
        [Range(0f, 1f)]
        [SerializeField] private float foamConnectorLengthPreference = 0.5f;

        [Tooltip("Maximum live Connector stretch relative to the length captured when its current relationship or recycle variant becomes active. A value of 1.45 permits 45% growth before the Connector breaks and attempts an immediate prepared relationship turnover.")]
        [Range(1.1f, 2f)]
        [SerializeField] private float foamConnectorBreakStretchRatio = 1.45f;

        [Tooltip("Controls the nested deterministic population of closed Interior Pocket negative regions hosted inside broad Major Support. Zero disables Interior Pockets, 0.5 preserves approximately the accepted Patch 4 population, and one activates additional bounded opportunities without reshuffling earlier identities.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamInteriorPocketAmount = 0.5f;

        [Tooltip("Controls the nested deterministic population of Edge Cavity negative regions. Cavities are hosted by broad Major Support but deliberately breach one selected side while preserving a useful positive remainder. Zero disables them; 0.5 is the normal baseline; one permits the maximum bounded population.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamEdgeCavityAmount = 0.5f;

        [Tooltip("Controls the nested deterministic population of Connector Weak Span negative regions. Weak Spans remain bound to accepted Connector identities, stay away from endpoint gates, and locally weaken a short path section without deleting or regenerating the Connector. Zero disables them; 0.5 is the normal baseline; one permits the maximum bounded population.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamConnectorWeakSpanAmount = 0.5f;

        [Tooltip("Controls the nested deterministic population of sparse Free-Water Negative Events. Events require valid water but no Major or Connector host, prefer neutral or weakly supported areas, and retain stable future drift, fade, span, and recycle metadata. Zero disables them; 0.5 is the normal sparse baseline; one permits the maximum bounded population.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamFreeWaterEventAmount = 0.5f;

        [Tooltip("Enables conservative automatic Layer C material birth from source candidates. This creates real persistent FoamState material through the existing birth pipeline; support topology then decides how long it survives. Disabled by default so validation can compare against manual sources honestly.")]
        [SerializeField] private bool foamAutomaticBirthEnabled;

        [Tooltip("Selects which automatic source-population strategy is active. Shore Contact Test and Obstacle Contact Test are implemented Layer C source-spawning classes. Free-water source entries remain documented placeholders.")]
        [SerializeField]
        private StylizedRiverFoamSourcePopulationPreset foamSourcePopulationPreset =
            StylizedRiverFoamSourcePopulationPreset.ShoreContactTest;

        [Tooltip("Enables the shore/contact source class when the source population preset allows shore birth. Shore foam creates real persistent material near the bank; support topology then decides how long it survives.")]
        [SerializeField] private bool foamAutomaticShoreBirthEnabled = true;

        [Tooltip("How much of the shoreline can participate in deterministic shore source events over time. This controls slot eligibility, not opacity or patch size.")]
        [FormerlySerializedAs("foamAutomaticShoreBirthAmount")]
        [FormerlySerializedAs("foamShoreBirthDensity")]
        [Range(0f, 1f)]
        [SerializeField] private float foamShoreFoamCoverage = 0.45f;

        [Tooltip("How promptly an eligible Shore source slot starts a new finite packet. Zero disables new starts. One fires immediately when packet clearance permits; Activity cannot bypass Minimum Packet Gap.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamShoreFoamActivity = 0.45f;

        [Tooltip("Minimum downstream clearance in metres required after one Shore packet completes before the same deterministic source slot may emit again.")]
        [Range(MinimumFoamPacketGapMetres, MaximumFoamPacketGapMetres)]
        [SerializeField] private float foamShoreMinimumPacketGapMetres =
            DefaultShoreFoamPacketGapMetres;

        [Tooltip("How large each deterministic shore source event is. Higher values lengthen/widen the spawned ribbon or inward-wash tongue.")]
        [FormerlySerializedAs("foamShoreFoamSize")]
        [Range(0f, 1f)]
        [SerializeField] private float foamShoreFoamPatchSize = 0.35f;

        [Tooltip("Base reveal speed in metres per second for automatic Shore Foam source paths. This controls one event's progressive source-head advance and is independent of Activity and later Foam transport.")]
        [Range(
            MinimumShoreFoamFormationSpeedMetresPerSecond,
            MaximumShoreFoamFormationSpeedMetresPerSecond)]
        [SerializeField]
        private float foamShoreFoamFormationSpeedMetresPerSecond =
            DefaultShoreFoamFormationSpeedMetresPerSecond;

        [Tooltip("Chooses the deterministic shore source recipe. Mixed uses the normalized Shore Ribbon / Inward Wash pattern weights below; the pure modes force one pattern for debugging.")]
        [SerializeField] private StylizedRiverFoamShorePattern foamShoreFoamPattern =
            StylizedRiverFoamShorePattern.Mixed;

        [Tooltip("Normalized Shore Foam mix share for bank-parallel ribbon sources when Pattern is Mixed. The editor keeps this and Inward Wash Weight summing to one.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamShoreRibbonPatternWeight = 0.88f;

        [Tooltip("Normalized Shore Foam mix share for inward wash sources when Pattern is Mixed. The editor keeps this and Shore Ribbon Weight summing to one.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamInwardWashPatternWeight = 0.12f;

        [Tooltip("Reveal Speed multiplier for Shore Ribbon events. One uses the Shore Foam Base Reveal Speed.")]
        [Range(0.10f, 3.00f)]
        [SerializeField] private float foamShoreRibbonFormationSpeedMultiplier = 1.00f;

        [Tooltip("Minimum authored Shore Ribbon length in metres before global Patch Size and deterministic variation are applied.")]
        [Min(0.05f)]
        [SerializeField] private float foamShoreRibbonLengthMinMetres = 2.20f;

        [Tooltip("Maximum authored Shore Ribbon length in metres before global Patch Size and deterministic variation are applied.")]
        [Min(0.05f)]
        [SerializeField] private float foamShoreRibbonLengthMaxMetres = 7.00f;

        [Tooltip("Compatibility Shore Ribbon thickness. LegacyNormalizedAcross interprets this in source-local cross-river Foam cells. FixedMetricLattice resolves the same authored value to source-local metres before rasterization.")]
        [Range(0.5f, 4f)]
        [SerializeField] private float foamShoreRibbonThicknessCells = 1f;

        [Tooltip("Base inward offset from the live shore edge for Shore Ribbon sources, in metres. Keep this close to zero for contact-attached ribbons.")]
        [Min(0f)]
        [SerializeField] private float foamShoreRibbonOffsetMetres = 0.030f;

        [Tooltip("Compatibility deterministic offset variation. LegacyNormalizedAcross interprets this in source-local cross-river Foam cells. FixedMetricLattice resolves it to source-local metres when the event is prepared.")]
        [Range(0f, 0.5f)]
        [SerializeField] private float foamShoreRibbonOffsetVariationCells = 0.25f;

        [Tooltip("Minimum intrinsic Presence written exactly to newly occupied Shore Ribbon material. Source shape, progressive reveal, subcell width, and valid-fluid clipping affect geometric Coverage only and do not attenuate this value.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamShoreRibbonInitialPresenceMin = 0.90f;

        [Tooltip("Maximum intrinsic Presence written exactly to newly occupied Shore Ribbon material. Source shape, progressive reveal, subcell width, and valid-fluid clipping affect geometric Coverage only and do not attenuate this value.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamShoreRibbonInitialPresenceMax = 1.00f;

        [Tooltip("Minimum initial normalized Remaining Life assigned to spawned Shore Ribbon material. One writes the full normalized life budget exactly; only explicit Layer C aging changes it afterward.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamShoreRibbonInitialLifeMin = 0.80f;

        [Tooltip("Maximum initial normalized Remaining Life assigned to spawned Shore Ribbon material. One writes the full normalized life budget exactly; only explicit Layer C aging changes it afterward.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamShoreRibbonInitialLifeMax = 1.00f;



        [Tooltip("Reveal Speed multiplier for Inward Wash events. One uses the Shore Foam Base Reveal Speed.")]
        [Range(0.10f, 3.00f)]
        [SerializeField] private float foamInwardWashFormationSpeedMultiplier = 1.00f;

        [Tooltip("Minimum authored Inward Wash length in metres before global Patch Size and deterministic variation are applied.")]
        [Min(0.05f)]
        [SerializeField] private float foamInwardWashLengthMinMetres = 0.70f;

        [Tooltip("Maximum authored Inward Wash length in metres before global Patch Size and deterministic variation are applied.")]
        [Min(0.05f)]
        [SerializeField] private float foamInwardWashLengthMaxMetres = 1.90f;

        [Tooltip("Minimum authored Inward Wash stroke width in metres before global Patch Size and deterministic variation are applied.")]
        [Min(0.005f)]
        [SerializeField] private float foamInwardWashWidthMinMetres = 0.030f;

        [Tooltip("Maximum authored Inward Wash stroke width in metres before global Patch Size and deterministic variation are applied.")]
        [Min(0.005f)]
        [SerializeField] private float foamInwardWashWidthMaxMetres = 0.085f;

        [Tooltip("Minimum authored inward reach in metres for Inward Wash sources before global Patch Size and deterministic variation are applied.")]
        [Min(0.005f)]
        [SerializeField] private float foamInwardWashReachMinMetres = 0.18f;

        [Tooltip("Maximum authored inward reach in metres for Inward Wash sources before global Patch Size and deterministic variation are applied.")]
        [Min(0.005f)]
        [SerializeField] private float foamInwardWashReachMaxMetres = 0.75f;

        [Tooltip("Minimum starting offset from the live shore edge for Inward Wash sources.")]
        [Min(0f)]
        [SerializeField] private float foamInwardWashOffsetMinMetres = 0.006f;

        [Tooltip("Maximum starting offset from the live shore edge for Inward Wash sources.")]
        [Min(0f)]
        [SerializeField] private float foamInwardWashOffsetMaxMetres = 0.040f;

        [Tooltip("Minimum intrinsic Presence written exactly to newly occupied Inward Wash material. Source shape, progressive reveal, subcell width, and valid-fluid clipping affect geometric Coverage only and do not attenuate this value.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamInwardWashInitialPresenceMin = 0.84f;

        [Tooltip("Maximum intrinsic Presence written exactly to newly occupied Inward Wash material. Source shape, progressive reveal, subcell width, and valid-fluid clipping affect geometric Coverage only and do not attenuate this value.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamInwardWashInitialPresenceMax = 0.98f;

        [Tooltip("Minimum initial normalized Remaining Life assigned to spawned Inward Wash material. One writes the full normalized life budget exactly; only explicit Layer C aging changes it afterward.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamInwardWashInitialLifeMin = 0.60f;

        [Tooltip("Maximum initial normalized Remaining Life assigned to spawned Inward Wash material. One writes the full normalized life budget exactly; only explicit Layer C aging changes it afterward.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamInwardWashInitialLifeMax = 1.00f;




        [Tooltip("Enables deterministic static object/contact Layer C material birth when the selected source preset allows object sources.")]
        [SerializeField] private bool foamAutomaticObjectBirthEnabled = true;

        [Tooltip("How much of the registered static object/contact population can participate in supplemental Contact Fleck events. Arc and Semi-Arc contact-cycle participation is controlled separately below.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamObjectFoamCoverage = 0.45f;

        [Tooltip("Stable share of registered static object anchors that can emit a finite Contact Arc or Contact Semi-Arc reinforcement burst. One includes every eligible object anchor.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamObjectContactCycleCoverage = 1.00f;

        [Tooltip("How promptly an eligible Object Contact Fleck attempts to start. Zero disables new Flecks. Activity cannot bypass the shared per-object packet-clearance gate, and a successful Fleck yields the next eligible opportunity to Arc/Semi-Arc when contact cycles are enabled.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamObjectFoamActivity = 0.35f;

        [Tooltip("Minimum downstream clearance in metres required after any Object Arc, Semi-Arc, or Fleck finishes before the same object may emit another packet. Rearm also includes conservative clearance through the authored object-contact slowdown halo.")]
        [Range(MinimumFoamPacketGapMetres, MaximumFoamPacketGapMetres)]
        [SerializeField] private float foamObjectContactMinimumPacketGapMetres =
            DefaultObjectContactPacketGapMetres;

        [Tooltip("Finite number of strokes in each Object Arc or Semi-Arc burst. Stroke one emits the complete contact packet and finite wake arm or arms. Later strokes reinforce only the immediate object-contact profile. The emitter ends after the final stroke and cannot bypass the shared packet-clearance gate.")]
        [Range(MinimumObjectContactStrokeCount, MaximumObjectContactStrokeCount)]
        [SerializeField] private int foamObjectContactStrokeCount =
            DefaultObjectContactStrokeCount;

        [Tooltip("Base reveal speed in metres per second for each finite Object Arc, Semi-Arc, and Fleck stroke. Per-pattern Reveal Speed multipliers remain available. This does not change later Layer C transport.")]
        [Range(
            MinimumShoreFoamFormationSpeedMetresPerSecond,
            MaximumShoreFoamFormationSpeedMetresPerSecond)]
        [SerializeField]
        private float foamObjectFoamFormationSpeedMetresPerSecond =
            DefaultShoreFoamFormationSpeedMetresPerSecond;

        [Tooltip("Chooses the deterministic object-contact source recipe. Mixed uses Arc and Semi-Arc weights for per-object contact cycles and enables supplemental Flecks through the independent Fleck Coverage and Activity controls. Pure modes force one pattern for debugging.")]
        [SerializeField] private StylizedRiverFoamObjectPattern foamObjectFoamPattern =
            StylizedRiverFoamObjectPattern.Mixed;

        [Tooltip("Normalized Object Foam mix share for contact-arc sources when Pattern is Mixed. The editor keeps object pattern weights summing to one.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamObjectContactArcPatternWeight = 0.45f;

        [Tooltip("Normalized Object Foam mix share for single-arm contact semi-arc sources when Pattern is Mixed. The editor keeps object pattern weights summing to one.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamObjectContactSemiArcPatternWeight = 0.35f;

        [Tooltip("Reveal Speed multiplier for each Object Contact Arc stroke. Stroke one traverses the upstream contact bridge and two straight downstream wake arms; later finite strokes traverse only the immediate contact bridge.")]
        [Range(0.10f, 3.00f)]
        [SerializeField] private float foamObjectContactArcFormationSpeedMultiplier = 1.00f;

        [Tooltip("Minimum straight downstream wake-arm length in metres for Object Contact Arcs. The source follows the physical obstacle only across its upstream face, then continues downstream from each side shoulder as a thin ribbon.")]
        [Min(0.05f)]
        [SerializeField] private float foamObjectContactArcLengthMinMetres = 0.45f;

        [Tooltip("Maximum straight downstream wake-arm length in metres for Object Contact Arcs. Increasing this extends the two side wakes downstream and never allows the source to wrap around the rear of the obstacle.")]
        [Min(0.05f)]
        [SerializeField] private float foamObjectContactArcLengthMaxMetres = 1.80f;

        [Tooltip("Signed visual-fit offset in metres applied to the Arc upstream contact radius. Negative values pull the connector closer to or beneath the object silhouette; positive values detach it farther upstream. Zero follows the prepared physical waterline profile. This does not sample or account for support zones.")]
        [SerializeField] private float foamObjectContactArcAlongFlowContactOffsetMetres = 0f;

        [Tooltip("Signed visual-fit offset in metres applied symmetrically to the Arc side-shoulder radius. Negative values pull both arms closer to or beneath the object sides; positive values detach them farther across-river. Zero follows the prepared physical waterline profile. This does not sample or account for support zones.")]
        [SerializeField] private float foamObjectContactArcAcrossRiverContactOffsetMetres = 0f;

        [HideInInspector]
        [Tooltip("Minimum Object Contact Arc profile scale in metres before deterministic variation. This shapes early tangential reveal, feather/profile gating, and local allowance inside the fixed immediate contact shell; it does not control shell thickness.")]
        [Min(0.005f)]
        [SerializeField] private float foamObjectContactArcWidthMinMetres = 0.035f;

        [HideInInspector]
        [Tooltip("Maximum Object Contact Arc profile scale in metres before deterministic variation. This shapes early tangential reveal, feather/profile gating, and local allowance inside the fixed immediate contact shell; it does not control shell thickness.")]
        [Min(0.005f)]
        [SerializeField] private float foamObjectContactArcWidthMaxMetres = 0.120f;

        [HideInInspector]
        [Tooltip("Minimum profile offset from the physical obstacle contact shell for Object Contact Arc sources. This biases profile placement but cannot widen the fixed shell.")]
        [Min(0f)]
        [SerializeField] private float foamObjectContactArcOffsetMinMetres = 0.015f;

        [HideInInspector]
        [Tooltip("Maximum profile offset from the physical obstacle contact shell for Object Contact Arc sources. This biases profile placement but cannot widen the fixed shell.")]
        [Min(0f)]
        [SerializeField] private float foamObjectContactArcOffsetMaxMetres = 0.120f;

        [Tooltip("Minimum intrinsic Presence written exactly to newly occupied Object Contact Arc material. Source shape, progressive reveal, subcell width, and valid-fluid clipping affect geometric Coverage only and do not attenuate this value.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamObjectContactArcInitialPresenceMin = 0.88f;

        [Tooltip("Maximum intrinsic Presence written exactly to newly occupied Object Contact Arc material. Source shape, progressive reveal, subcell width, and valid-fluid clipping affect geometric Coverage only and do not attenuate this value.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamObjectContactArcInitialPresenceMax = 1.00f;

        [Tooltip("Minimum initial normalized Remaining Life written exactly to newly occupied Object Contact Arc material. Only explicit Layer C aging changes it afterward.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamObjectContactArcInitialLifeMin = 0.75f;

        [Tooltip("Maximum initial normalized Remaining Life written exactly to newly occupied Object Contact Arc material. Only explicit Layer C aging changes it afterward.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamObjectContactArcInitialLifeMax = 1.00f;



        [Tooltip("Reveal Speed multiplier for each Object Contact Semi-Arc stroke. Stroke one traverses the selected upstream contact half and one straight downstream wake arm; later finite strokes traverse only that contact half.")]
        [Range(0.10f, 3.00f)]
        [SerializeField] private float foamObjectContactSemiArcFormationSpeedMultiplier = 1.00f;

        [Tooltip("Minimum straight downstream wake-arm length in metres for Object Contact Semi-Arcs. The selected side receives this arm; the opposite side stops at the face shoulder while the complete upstream connector remains present.")]
        [Min(0.05f)]
        [SerializeField] private float foamObjectContactSemiArcLengthMinMetres = 0.35f;

        [Tooltip("Maximum straight downstream wake-arm length in metres for Object Contact Semi-Arcs. Increasing this extends the single selected-side wake downstream and never creates an opposite-side arm or rear wrap.")]
        [Min(0.05f)]
        [SerializeField] private float foamObjectContactSemiArcLengthMaxMetres = 1.35f;

        [Tooltip("Signed visual-fit offset in metres applied to the Semi-Arc upstream contact radius. Negative values pull the connector closer to or beneath the object silhouette; positive values detach it farther upstream. Zero follows the prepared physical waterline profile. This does not sample or account for support zones.")]
        [SerializeField] private float foamObjectContactSemiArcAlongFlowContactOffsetMetres = 0f;

        [Tooltip("Signed visual-fit offset in metres applied symmetrically to the Semi-Arc side-shoulder radius. Negative values pull the face endpoint and single arm closer to or beneath the object sides; positive values detach them farther across-river. Zero follows the prepared physical waterline profile. This does not sample or account for support zones.")]
        [SerializeField] private float foamObjectContactSemiArcAcrossRiverContactOffsetMetres = 0f;

        [HideInInspector]
        [Tooltip("Minimum Object Contact Semi-Arc profile scale in metres before deterministic variation. This shapes one-sided reveal, feather/profile gating, and local allowance inside the fixed immediate contact shell; it does not control shell thickness.")]
        [Min(0.005f)]
        [SerializeField] private float foamObjectContactSemiArcWidthMinMetres = 0.030f;

        [HideInInspector]
        [Tooltip("Maximum Object Contact Semi-Arc profile scale in metres before deterministic variation. This shapes one-sided reveal, feather/profile gating, and local allowance inside the fixed immediate contact shell; it does not control shell thickness.")]
        [Min(0.005f)]
        [SerializeField] private float foamObjectContactSemiArcWidthMaxMetres = 0.105f;

        [HideInInspector]
        [Tooltip("Minimum profile offset from the physical obstacle contact shell for Object Contact Semi-Arc sources. This biases profile placement but cannot widen the fixed shell.")]
        [Min(0f)]
        [SerializeField] private float foamObjectContactSemiArcOffsetMinMetres = 0.020f;

        [HideInInspector]
        [Tooltip("Maximum profile offset from the physical obstacle contact shell for Object Contact Semi-Arc sources. This biases profile placement but cannot widen the fixed shell.")]
        [Min(0f)]
        [SerializeField] private float foamObjectContactSemiArcOffsetMaxMetres = 0.140f;

        [Tooltip("Minimum intrinsic Presence written exactly to newly occupied Object Contact Semi-Arc material. Source shape, progressive reveal, subcell width, and valid-fluid clipping affect geometric Coverage only and do not attenuate this value.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamObjectContactSemiArcInitialPresenceMin = 0.84f;

        [Tooltip("Maximum intrinsic Presence written exactly to newly occupied Object Contact Semi-Arc material. Source shape, progressive reveal, subcell width, and valid-fluid clipping affect geometric Coverage only and do not attenuate this value.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamObjectContactSemiArcInitialPresenceMax = 0.98f;

        [Tooltip("Minimum initial normalized Remaining Life written exactly to newly occupied Object Contact Semi-Arc material. Only explicit Layer C aging changes it afterward.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamObjectContactSemiArcInitialLifeMin = 0.65f;

        [Tooltip("Maximum initial normalized Remaining Life written exactly to newly occupied Object Contact Semi-Arc material. Only explicit Layer C aging changes it afterward.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamObjectContactSemiArcInitialLifeMax = 1.00f;



        [Tooltip("Reveal Speed multiplier for Object Contact Fleck reveal.")]
        [Range(0.10f, 3.00f)]
        [SerializeField] private float foamObjectContactFleckFormationSpeedMultiplier = 1.00f;

        [Tooltip("Minimum authored Object Contact Fleck length in metres before deterministic variation is applied.")]
        [Min(0.05f)]
        [SerializeField] private float foamObjectContactFleckLengthMinMetres = 0.12f;

        [Tooltip("Maximum authored Object Contact Fleck length in metres before deterministic variation is applied.")]
        [Min(0.05f)]
        [SerializeField] private float foamObjectContactFleckLengthMaxMetres = 0.55f;

        [Tooltip("Minimum Object Contact Fleck capsule size in metres before deterministic variation. This controls the fleck shape inside the fixed immediate contact shell; it does not control shell thickness.")]
        [Min(0.005f)]
        [SerializeField] private float foamObjectContactFleckWidthMinMetres = 0.025f;

        [Tooltip("Maximum Object Contact Fleck capsule size in metres before deterministic variation. This controls the fleck shape inside the fixed immediate contact shell; it does not control shell thickness.")]
        [Min(0.005f)]
        [SerializeField] private float foamObjectContactFleckWidthMaxMetres = 0.080f;

        [Tooltip("Minimum shape offset from the physical obstacle contact shell for Object Contact Fleck sources. This biases fleck placement but cannot widen the fixed shell.")]
        [Min(0f)]
        [SerializeField] private float foamObjectContactFleckOffsetMinMetres = 0.020f;

        [Tooltip("Maximum shape offset from the physical obstacle contact shell for Object Contact Fleck sources. This biases fleck placement but cannot widen the fixed shell.")]
        [Min(0f)]
        [SerializeField] private float foamObjectContactFleckOffsetMaxMetres = 0.160f;

        [Tooltip("Minimum intrinsic Presence written exactly to newly occupied Object Contact Fleck material. Source shape, progressive reveal, subcell width, and valid-fluid clipping affect geometric Coverage only and do not attenuate this value.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamObjectContactFleckInitialPresenceMin = 0.82f;

        [Tooltip("Maximum intrinsic Presence written exactly to newly occupied Object Contact Fleck material. Source shape, progressive reveal, subcell width, and valid-fluid clipping affect geometric Coverage only and do not attenuate this value.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamObjectContactFleckInitialPresenceMax = 0.97f;

        [Tooltip("Minimum initial normalized Remaining Life written exactly to newly occupied Object Contact Fleck material. Only explicit Layer C aging changes it afterward.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamObjectContactFleckInitialLifeMin = 0.55f;

        [Tooltip("Maximum initial normalized Remaining Life written exactly to newly occupied Object Contact Fleck material. Only explicit Layer C aging changes it afterward.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamObjectContactFleckInitialLifeMax = 0.90f;



        [Tooltip("Enables deterministic open-water Layer C material birth when Automatic Foam Birth is on and Spawn Preset is not Off.")]
        [SerializeField] private bool foamAutomaticFreeWaterBirthEnabled = true;

        [Tooltip("How much of the open-water deterministic source lattice can participate in Free Water Foam source events over time. This controls slot eligibility, not opacity or patch size.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamFreeWaterFoamCoverage = 0.25f;

        [Tooltip("How promptly an eligible Free Water source slot starts a new finite packet. Zero disables new starts. One fires immediately when packet clearance permits; Activity cannot bypass Minimum Packet Gap.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamFreeWaterFoamActivity = 0.25f;

        [Tooltip("Minimum downstream clearance in metres required after one Free Water packet completes before the same deterministic source slot may emit again.")]
        [Range(MinimumFoamPacketGapMetres, MaximumFoamPacketGapMetres)]
        [SerializeField] private float foamFreeWaterMinimumPacketGapMetres =
            DefaultFreeWaterFoamPacketGapMetres;

        [Tooltip("Base reveal speed in metres per second for Free Water Foam. Per-pattern Reveal Speed multipliers control Lace, Cross-Lace, and Torn Fragment source progression.")]
        [Range(
            MinimumShoreFoamFormationSpeedMetresPerSecond,
            MaximumShoreFoamFormationSpeedMetresPerSecond)]
        [SerializeField]
        private float foamFreeWaterFoamFormationSpeedMetresPerSecond =
            DefaultShoreFoamFormationSpeedMetresPerSecond;

        [Tooltip("Chooses the deterministic open-water source recipe. Mixed uses the normalized Lace Connector / Cross-Lace Connector / Torn Fragment pattern weights below; the pure modes force one pattern for debugging.")]
        [SerializeField] private StylizedRiverFoamFreeWaterPattern foamFreeWaterFoamPattern =
            StylizedRiverFoamFreeWaterPattern.Mixed;

        [Tooltip("Normalized Free Water Foam mix share for with-flow lace connector sources when Pattern is Mixed. The editor keeps free-water pattern weights summing to one.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamFreeWaterLaceConnectorPatternWeight = 0.30f;

        [Tooltip("Normalized Free Water Foam mix share for cross-current lace connector sources when Pattern is Mixed. The editor keeps free-water pattern weights summing to one.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamFreeWaterCrossLaceConnectorPatternWeight = 0.45f;

        [Tooltip("Normalized Free Water Foam mix share for progressive torn fragment sources when Pattern is Mixed. The editor keeps free-water pattern weights summing to one.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamFreeWaterTornFragmentPatternWeight = 0.25f;

        [Tooltip("Reveal Speed multiplier for Free Water Lace Connector source-head progression.")]
        [Range(0.10f, 3.00f)]
        [SerializeField] private float foamFreeWaterLaceFormationSpeedMultiplier = 1.00f;

        [Tooltip("Minimum authored Free Water Lace Connector length in metres before deterministic variation is applied.")]
        [Min(0.05f)]
        [SerializeField] private float foamFreeWaterLaceLengthMinMetres = 1.40f;

        [Tooltip("Maximum authored Free Water Lace Connector length in metres before deterministic variation is applied.")]
        [Min(0.05f)]
        [SerializeField] private float foamFreeWaterLaceLengthMaxMetres = 5.80f;

        [Tooltip("Minimum authored Free Water Lace Connector width in metres before deterministic variation is applied.")]
        [Min(0.005f)]
        [SerializeField] private float foamFreeWaterLaceWidthMinMetres = 0.025f;

        [Tooltip("Maximum authored Free Water Lace Connector width in metres before deterministic variation is applied.")]
        [Min(0.005f)]
        [SerializeField] private float foamFreeWaterLaceWidthMaxMetres = 0.115f;

        [Tooltip("Minimum intrinsic Presence written exactly to newly occupied Free Water Lace Connector material. Source shape, progressive reveal, subcell width, and valid-fluid clipping affect geometric Coverage only and do not attenuate this value.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamFreeWaterLaceInitialPresenceMin = 0.78f;

        [Tooltip("Maximum intrinsic Presence written exactly to newly occupied Free Water Lace Connector material. Source shape, progressive reveal, subcell width, and valid-fluid clipping affect geometric Coverage only and do not attenuate this value.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamFreeWaterLaceInitialPresenceMax = 0.96f;

        [Tooltip("Minimum initial normalized Remaining Life written exactly to newly occupied Free Water Lace Connector material. Only explicit Layer C aging changes it afterward.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamFreeWaterLaceInitialLifeMin = 0.35f;

        [Tooltip("Maximum initial normalized Remaining Life written exactly to newly occupied Free Water Lace Connector material. Only explicit Layer C aging changes it afterward.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamFreeWaterLaceInitialLifeMax = 0.80f;



        [Tooltip("Minimum signed curvature magnitude for Free Water Lace Connector sources. The sign is chosen deterministically per event.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamFreeWaterLaceCurvatureMin = 0.00f;

        [Tooltip("Maximum signed curvature magnitude for Free Water Lace Connector sources. The sign is chosen deterministically per event.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamFreeWaterLaceCurvatureMax = 1.00f;

        [Tooltip("Reveal Speed multiplier for Free Water Cross-Lace Connector source-head progression.")]
        [Range(0.10f, 3.00f)]
        [SerializeField] private float foamFreeWaterCrossLaceFormationSpeedMultiplier = 1.00f;

        [Tooltip("Minimum authored Free Water Cross-Lace Connector lateral length in metres before deterministic variation is applied.")]
        [Min(0.05f)]
        [SerializeField] private float foamFreeWaterCrossLaceLengthMinMetres = 0.70f;

        [Tooltip("Maximum authored Free Water Cross-Lace Connector lateral length in metres before deterministic variation is applied.")]
        [Min(0.05f)]
        [SerializeField] private float foamFreeWaterCrossLaceLengthMaxMetres = 2.40f;

        [Tooltip("Minimum authored Free Water Cross-Lace Connector width in metres before deterministic variation is applied.")]
        [Min(0.005f)]
        [SerializeField] private float foamFreeWaterCrossLaceWidthMinMetres = 0.030f;

        [Tooltip("Maximum authored Free Water Cross-Lace Connector width in metres before deterministic variation is applied.")]
        [Min(0.005f)]
        [SerializeField] private float foamFreeWaterCrossLaceWidthMaxMetres = 0.120f;

        [Tooltip("Minimum intrinsic Presence written exactly to newly occupied Free Water Cross-Lace Connector material. Source shape, progressive reveal, subcell width, and valid-fluid clipping affect geometric Coverage only and do not attenuate this value.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamFreeWaterCrossLaceInitialPresenceMin = 0.78f;

        [Tooltip("Maximum intrinsic Presence written exactly to newly occupied Free Water Cross-Lace Connector material. Source shape, progressive reveal, subcell width, and valid-fluid clipping affect geometric Coverage only and do not attenuate this value.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamFreeWaterCrossLaceInitialPresenceMax = 0.96f;

        [Tooltip("Minimum initial normalized Remaining Life written exactly to newly occupied Free Water Cross-Lace Connector material. Only explicit Layer C aging changes it afterward.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamFreeWaterCrossLaceInitialLifeMin = 0.45f;

        [Tooltip("Maximum initial normalized Remaining Life written exactly to newly occupied Free Water Cross-Lace Connector material. Only explicit Layer C aging changes it afterward.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamFreeWaterCrossLaceInitialLifeMax = 0.90f;



        [Tooltip("Reveal Speed multiplier for the complete Free Water Torn Fragment local sweep.")]
        [Range(0.10f, 3.00f)]
        [SerializeField] private float foamFreeWaterFragmentFormationSpeedMultiplier = 1.00f;

        [Tooltip("Minimum authored Free Water Torn Fragment length in metres before deterministic variation is applied.")]
        [Min(0.05f)]
        [SerializeField] private float foamFreeWaterFragmentLengthMinMetres = 0.35f;

        [Tooltip("Maximum authored Free Water Torn Fragment length in metres before deterministic variation is applied.")]
        [Min(0.05f)]
        [SerializeField] private float foamFreeWaterFragmentLengthMaxMetres = 1.35f;

        [Tooltip("Minimum authored Free Water Torn Fragment width in metres before deterministic variation is applied.")]
        [Min(0.005f)]
        [SerializeField] private float foamFreeWaterFragmentWidthMinMetres = 0.055f;

        [Tooltip("Maximum authored Free Water Torn Fragment width in metres before deterministic variation is applied.")]
        [Min(0.005f)]
        [SerializeField] private float foamFreeWaterFragmentWidthMaxMetres = 0.280f;

        [Tooltip("Minimum intrinsic Presence written exactly to newly occupied Free Water Torn Fragment material. Source shape, progressive reveal, subcell width, and valid-fluid clipping affect geometric Coverage only and do not attenuate this value.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamFreeWaterFragmentInitialPresenceMin = 0.76f;

        [Tooltip("Maximum intrinsic Presence written exactly to newly occupied Free Water Torn Fragment material. Source shape, progressive reveal, subcell width, and valid-fluid clipping affect geometric Coverage only and do not attenuate this value.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamFreeWaterFragmentInitialPresenceMax = 0.94f;

        [Tooltip("Minimum initial normalized Remaining Life written exactly to newly occupied Free Water Torn Fragment material. Only explicit Layer C aging changes it afterward.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamFreeWaterFragmentInitialLifeMin = 0.25f;

        [Tooltip("Maximum initial normalized Remaining Life written exactly to newly occupied Free Water Torn Fragment material. Only explicit Layer C aging changes it afterward.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamFreeWaterFragmentInitialLifeMax = 0.65f;



        [SerializeField, HideInInspector] private float foamShoreFoamStrength = 0.35f;
        [SerializeField, HideInInspector] private float foamShoreFoamPersistence = 0.30f;

        [Tooltip("Lifetime in seconds for unsupported Foam in neutral water. Positive topology slows local aging, while Negative Aging Pressure suppresses that preservation and then accelerates Remaining Life loss. This controls persistent material life, not topology lifetime.")]
        [Range(MinimumFoamNeutralLifetime, MaximumFoamNeutralLifetime)]
        [SerializeField]
        private float foamNeutralLifetime = DefaultFoamNeutralLifetime;

        [Tooltip("Aging-rate multiplier at full positive support. Values below one extend Remaining Life. The default 0.20 gives fully supported material five times the neutral lifetime; the minimum 0.05 requests twenty times the neutral lifetime before negative overlap is considered.")]
        [Range(
            MinimumFoamSupportedAgingRate,
            MaximumFoamSupportedAgingRate)]
        [SerializeField]
        private float foamSupportedAgingRate = DefaultFoamSupportedAgingRate;

        [Tooltip("Raw combined positive-support value at which Supported Aging Rate is applied fully. Lower values let ordinary support preserve Foam more strongly. The default 0.92 reproduces the previous fixed support-authority curve.")]
        [Range(
            MinimumFoamFullSupportedAgingAt,
            MaximumFoamFullSupportedAgingAt)]
        [SerializeField]
        private float foamFullSupportedAgingAt =
            DefaultFoamFullSupportedAgingAt;

        [Tooltip("Selects how Final Foam converts transported Coverage and Remaining Life into a visible shape. Concentration + Lifetime deliberately lets diffuse Coverage and Remaining Life both reduce visibility. Lifecycle-Faithful uses meaningful Coverage as the footprint and leaves ordinary lifetime authority to explicit Layer C aging, so numerical dilution cannot counterfeit early death. This is render-only and does not change stored material or lifecycle.")]
        [SerializeField]
        private StylizedRiverFinalFoamVisibilityMode foamFinalVisibilityMode =
            StylizedRiverFinalFoamVisibilityMode.ConcentrationAndLifetime;

        [Tooltip("Selects whether decoded intrinsic Presence scales Final Foam. Coverage-Only resolves the shape from Coverage, Life, Pattern, Chipping, and Strands without using Presence as visual amplitude. Presence-Amplitude carries exact Presence through identical Presence-independent shape and surface-coupling weights, so uniform 0.75 produces 75% of the equivalent Presence 1.00 resolved mask before other explicit global rendering controls. This is render-only and may be switched during Play Mode.")]
        [SerializeField]
        private StylizedRiverFoamPresenceFootprintMode
            foamPresenceFootprintMode =
                StylizedRiverFoamPresenceFootprintMode.Current;

        [Tooltip("Aging-rate multiplier at full Negative Aging Pressure. Values above one consume Remaining Life faster. Negative pressure also suppresses positive support preservation before this multiplier is applied, so hostile overlap kills rather than merely weakens support.")]
        [Range(
            MinimumFoamNegativeAgingRate,
            MaximumFoamNegativeAgingRate)]
        [SerializeField]
        private float foamNegativeAgingRate = DefaultFoamNegativeAgingRate;

        [Tooltip("Downstream speed ratio for persistent Foam relative to the authored river Flow Speed. One follows the liquid surface speed, zero removes ordinary downstream Foam travel, and values above one make Foam respond faster than the visible water. This is the base speed input for the unified Foam velocity contract.")]
        [Range(
            MinimumFoamDownstreamSpeedRatio,
            MaximumFoamDownstreamSpeedRatio)]
        [SerializeField]
        private float foamMaterialFlowSpeedMultiplier =
            DefaultFoamDownstreamSpeedRatio;

        // Legacy serialized fields retained only to avoid scene/prefab
        // serialization churn. Persistent stored-state surface morphing was
        // removed after 4.11C.5.9n; these values are no longer read, migrated,
        // clamped, exposed, or bound to runtime/shader systems.
        [SerializeField, HideInInspector]
        private float foamSurfaceMorphStrength;

        [SerializeField, HideInInspector]
        private int foamSurfaceMorphCalibrationVersion;

        // The legacy serialized field names below are retained to avoid scene
        // and prefab churn. Patch 4.11C.5.16A changes their authored meaning
        // from arbitrary lateral cells / field wraps into physical velocity
        // ratios used by one canonical Foam velocity contract.
        [Tooltip("Maximum signed lateral Foam speed as a ratio of the base downstream Foam speed. A value of 0.22 permits left/right motion up to 22% of the base downstream speed. This contract is validated by the Motion Field view but does not move stored material until conservative 2D transport is implemented.")]
        [Range(
            MinimumFoamMaximumLateralSpeedRatio,
            MaximumFoamMaximumLateralSpeedRatio)]
        [SerializeField]
        private float foamMotionFieldStrength =
            LegacyDefaultFoamMotionFieldStrength;

        [Tooltip("Downstream advection speed of the generated lateral route pattern relative to the base downstream Foam speed. Zero keeps routes fixed in river space; one moves the lane pattern downstream at the base Foam speed. This is a sample-coordinate phase motion, not a field rebuild and not stored-material transport. Slow values around 0.03-0.08 are recommended when routes should evolve without visibly travelling with the river.")]
        [Range(
            MinimumFoamLaneAdvectionRatio,
            MaximumFoamLaneAdvectionRatio)]
        [SerializeField]
        private float foamMotionFieldScrollHz =
            LegacyDefaultFoamMotionFieldScrollHz;

        [Tooltip("Approximate fraction of the generated lateral route field compressed toward very low lateral intent. These regions still move downstream and are not true stagnant water. Changing this regenerates the lane texture only.")]
        [Range(
            MinimumFoamLowLateralMotionCoverage,
            MaximumFoamLowLateralMotionCoverage)]
        [SerializeField]
        private float foamMotionFieldNeutralCoverage =
            DefaultFoamLowLateralMotionCoverage;

        [Tooltip("Controls how often generated left/right route intent changes sign downstream. Higher values create more frequent but still irregular downstream route changes; lower values create longer persistent route regions. This does not directly change across-river coherence. Changing this regenerates the lane texture only.")]
        [Range(
            MinimumFoamDirectionChangeFrequency,
            MaximumFoamDirectionChangeFrequency)]
        [SerializeField]
        private float foamMotionFieldLaneScale =
            DefaultFoamDirectionChangeFrequency;

        [Tooltip("Controls how broadly lateral route intent is shared across the river width. Higher values keep neighbouring rows coherent over larger areas; lower values permit finer across-river variation. This does not directly change how often routes switch downstream. Changing this regenerates the lane texture only.")]
        [Range(
            MinimumFoamAcrossRiverCoherence,
            MaximumFoamAcrossRiverCoherence)]
        [SerializeField]
        private float foamMotionFieldAcrossRiverCoherence =
            DefaultFoamAcrossRiverCoherence;

        [Tooltip("Controls how quickly the object-contact slowdown halo approaches its full authority. Zero disables contact slowdown. Any positive value reaches the exact Minimum Speed Factor at full contact influence.")]
        [Range(
            MinimumFoamObstacleSlowdownStrength,
            MaximumFoamObstacleSlowdownStrength)]
        [SerializeField]
        private float foamObstacleSlowdownStrength =
            DefaultFoamObstacleSlowdownStrength;

        [Tooltip("Exact speed factor applied to the complete routed Foam velocity vector at full object-contact slowdown influence. Zero permits local stagnation and prevents automatic object-source rearm while slowdown is enabled; one disables speed reduction.")]
        [Range(
            MinimumFoamObstacleMinimumDownstreamFactor,
            MaximumFoamObstacleMinimumDownstreamFactor)]
        [SerializeField]
        private float foamObstacleMinimumDownstreamFactor =
            DefaultFoamObstacleMinimumDownstreamFactor;

        [Tooltip("Distance in metres from the obstacle surface over which the contact slowdown field remains at full influence.")]
        [Min(0f)]
        [SerializeField] private float foamObjectContactFullSlowdownReachMetres =
            0.10f;

        [Tooltip("Outer distance in metres from the obstacle surface where the contact slowdown field reaches zero. This value is clamped to at least Full Slowdown Reach.")]
        [Min(0f)]
        [SerializeField] private float foamObjectContactSlowdownOuterReachMetres =
            0.45f;

        [Tooltip("Seconds for the Layer D temporal visual sheet to acquire newly supported film. This changes visual occupancy only; it never creates persistent material or changes Remaining Life.")]
        [Range(
            MinimumFoamVisualOccupancyBuildTime,
            MaximumFoamVisualOccupancyBuildTime)]
        [SerializeField]
        private float foamVisualOccupancyBuildTime =
            DefaultFoamVisualOccupancyBuildTime;

        [Tooltip("Seconds for unsupported Layer D temporal visual occupancy to release toward the current instantaneous film target. This is visual-only persistence and does not extend material lifetime.")]
        [Range(
            MinimumFoamVisualOccupancyReleaseTime,
            MaximumFoamVisualOccupancyReleaseTime)]
        [SerializeField]
        private float foamVisualOccupancyReleaseTime =
            DefaultFoamVisualOccupancyReleaseTime;

        [SerializeField, HideInInspector]
        private int foamMaterialLifecycleTuningVersion;

        [SerializeField, HideInInspector]
        private int foamVelocityTuningVersion;

        [Tooltip("Lit, non-emissive Foam base tint. RGB is resolved before water bleed-through; alpha sets the base Foam opacity before the established-interior floor is applied.")]
        [SerializeField] private Color foamColour =
            new Color(0.94f, 0.97f, 0.94f, 0.72f);

        [Tooltip("Sets an absolute minimum rendered opacity for established Foam interiors. This may exceed Foam Colour alpha, but it does not affect weak fringe or create Foam outside the incoming silhouette. Zero preserves the accepted pre-5.17A composition.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamInteriorOpacityFloor;

        [Tooltip("Controls the existing edge-versus-interior lighting contrast. Negative values suppress the bright rim toward interior lighting, zero preserves the accepted pre-5.17A lighting, and positive values intensify the existing edge. This never expands the Foam silhouette.")]
        [Range(-1f, 1f)]
        [SerializeField] private float foamEdgeContrast;

        [Tooltip("Fraction of analytical Chip candidates selected for production removal. Zero disables production Chipping; one selects every available candidate before edge and transported-material gating.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamChipActivation;

        [Tooltip("World-space metres between analytical Chip candidate centres. Lower values create more candidates; higher values create fewer, more isolated candidates. Chip Size is resolved relative to this spacing so the adaptive search remains bounded and predictable.")]
        [Range(MinimumFoamChipCandidateSpacing, MaximumFoamChipCandidateSpacing)]
        [SerializeField] private float foamChipCandidateSpacing =
            DefaultFoamChipCandidateSpacing;

        [Tooltip("Relative mean Chip size within the authored spacing. Zero maps to a radius of 5% of Chip Spacing; one maps to 65%. The Inspector reports the effective world-space radius and diameter. This bounded relationship prevents hidden candidate-loop expansion.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamChipSize =
            DefaultFoamChipSize;

        [Tooltip("One static variation control for Chip placement, size, and connected contour shape. Zero gives a regular lattice of equal circles. One enables the accepted maximum centre jitter, approximately 0.80×-1.40× radius variation, and strongly asymmetric connected contours. The raised minimum removes the former tiny-size tail.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamChipIrregularity =
            DefaultFoamChipIrregularity;

        [Tooltip("Target readable radius in screen pixels for each fully formed Chip. Zero preserves pure world-space sizing. Positive values first use the bounded enlargement, then fade candidates that still cannot approach the target instead of leaving distant pixel dirt. Formation, dissolution, dormancy, Chip Irregularity, and pulse remain authoritative.")]
        [Range(MinimumFoamChipStableScreenRadiusPixels, MaximumFoamChipStableScreenRadiusPixels)]
        [SerializeField] private float foamChipStableScreenRadiusPixels =
            DefaultFoamChipStableScreenRadiusPixels;

        [Tooltip("Maximum multiplier permitted for view-readability enlargement. One disables enlargement even when Minimum Stable Radius is positive; larger values preserve distant readability while bounding world-space growth and overlap.")]
        [Range(MinimumFoamChipMaximumViewScale, MaximumFoamChipMaximumViewScale)]
        [SerializeField] private float foamChipMaximumViewScale =
            DefaultFoamChipMaximumViewScale;


        [Tooltip("Approximate inward width, in rendered pixels, of the canonical pre-Chip Foam edge territory. Zero disables edge permission exactly. The Inspector slider covers 0–256 px, while direct numeric entry accepts any non-negative value for deliberately extreme tests. This is a derivative-normalized local screen-space estimate, not a global geometric distance field.")]
        [Min(0f)]
        [SerializeField] private float foamChipEdgeWidthPixels =
            DefaultFoamChipEdgeWidthPixels;

        [Tooltip("Soft-visibility value treated as the exterior start of the Presence-Amplitude Eligibility coordinate. The default 0.06 matches the accepted historical Coverage-Only route. Higher values move the detected band inward; lower values include fainter fringe. This control affects Presence-Amplitude only.")]
        [Range(
            MinimumFoamChipSoftEdgeStart,
            MaximumFoamChipSoftEdgeStart)]
        [SerializeField] private float foamChipSoftEdgeStart =
            DefaultFoamChipSoftEdgeStart;

        [Tooltip("Fraction of activated analytical candidate cells granted permission in the established visible body complementary to Chip Edge Width. Zero keeps every candidate edge-only; one grants every activated candidate full visible-body access. Admission is deterministic per candidate, so connected Chip contours remain intact.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamChipInteriorAccess =
            DefaultFoamChipInteriorAccess;

        [Tooltip("Downstream speed in metres per second of the complete analytical Chip candidate field. This is rigid translation in River space and cannot stretch an individual candidate.")]
        [Range(MinimumFoamChipFieldSpeed, MaximumFoamChipFieldSpeed)]
        [SerializeField] private float foamChipFieldSpeed =
            DefaultFoamChipFieldSpeed;

        [Tooltip("Seconds for an individual participating Chip to grow monotonically from zero radius to its authored living radius.")]
        [Range(MinimumFoamChipLifecycleTime, MaximumFoamChipLifecycleTime)]
        [SerializeField] private float foamChipFormationTime =
            DefaultFoamChipFormationTime;

        [Tooltip("Seconds an individual Chip remains fully formed before dissolution begins. Size pulse and shape change ease in and out inside this stage.")]
        [Range(MinimumFoamChipLifecycleTime, MaximumFoamChipLifecycleTime)]
        [SerializeField] private float foamChipStableTime =
            DefaultFoamChipStableTime;

        [Tooltip("Seconds for an individual Chip to shrink monotonically from its authored living radius to zero.")]
        [Range(MinimumFoamChipLifecycleTime, MaximumFoamChipLifecycleTime)]
        [SerializeField] private float foamChipDissolveTime =
            DefaultFoamChipDissolveTime;

        [Tooltip("Seconds the same deterministic Chip identity remains completely absent after dissolution before its next formation cycle.")]
        [Range(MinimumFoamChipLifecycleTime, MaximumFoamChipLifecycleTime)]
        [SerializeField] private float foamChipDormantTime =
            DefaultFoamChipDormantTime;

        [Tooltip("Maximum rigid lateral excursion of each Chip centre as a fraction of Chip Spacing. Zero disables lateral movement; 1 moves plus or minus one full spacing, and 2.5 moves plus or minus two and a half spacings. The shader expands its lateral candidate search to keep the translated contours complete.")]
        [Range(MinimumFoamChipLateralMotionAmount, MaximumFoamChipLateralMotionAmount)]
        [SerializeField] private float foamChipLateralMotionAmount =
            DefaultFoamChipLateralMotionAmount;

        [Tooltip("Lateral oscillation cycles per second. Zero returns every candidate to its static lateral centre even when Lateral Motion Amount is nonzero.")]
        [Range(MinimumFoamChipMotionSpeed, MaximumFoamChipMotionSpeed)]
        [SerializeField] private float foamChipLateralMotionSpeed =
            DefaultFoamChipLateralMotionSpeed;

        [Tooltip("Maximum rigid angular excursion in degrees around each candidate's static orientation. Rotation preserves area and aspect ratio; circles are visually unchanged.")]
        [Range(MinimumFoamChipRotationAmountDegrees, MaximumFoamChipRotationAmountDegrees)]
        [SerializeField] private float foamChipRotationAmountDegrees =
            DefaultFoamChipRotationAmountDegrees;

        [Tooltip("Rotation oscillation cycles per second. Zero restores the static orientation even when Rotation Amount is nonzero.")]
        [Range(MinimumFoamChipMotionSpeed, MaximumFoamChipMotionSpeed)]
        [SerializeField] private float foamChipRotationSpeed =
            DefaultFoamChipRotationSpeed;

        [Tooltip("Fractional radius excursion while a Chip is established. For example, 0.20 pulses between 80% and 120% of its living radius. This never controls birth, death, or Dormant Time.")]
        [Range(MinimumFoamChipSizePulseAmount, MaximumFoamChipSizePulseAmount)]
        [SerializeField] private float foamChipSizePulseAmount =
            DefaultFoamChipSizePulseAmount;

        [Tooltip("Size-pulse cycles per second during the established lifecycle stage. Zero keeps the living radius at its authored value without disabling lifecycle turnover.")]
        [Range(MinimumFoamChipMotionSpeed, MaximumFoamChipMotionSpeed)]
        [SerializeField] private float foamChipSizePulseSpeed =
            DefaultFoamChipSizePulseSpeed;

        [Tooltip("Authority of multi-axis temporal silhouette morphing while a Chip is established. Zero preserves the static contour; one blends toward a strong candidate-specific sine-harmonic geometry trajectory with exact temporal radial-area preservation. This is independent of static Chip Irregularity and does not change the authored Chip Size, Size Pulse, or lifecycle scale; redistributed lobes are covered by the adaptive search.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamChipShapeChangeAmount =
            DefaultFoamChipShapeChangeAmount;

        [Tooltip("How often each deterministic Chip selects its next contour target, in changes per second. This controls cadence only. Candidate phases stagger target changes so the field does not switch in unison. Zero preserves the current static contour even when Shape Change Amount is nonzero.")]
        [Range(MinimumFoamChipMotionSpeed, MaximumFoamChipMotionSpeed)]
        [SerializeField] private float foamChipShapeChangeSpeed =
            DefaultFoamChipShapeChangeSpeed;

        [Tooltip("Seconds spent smoothly morphing from the previous contour target to the next one. This controls the actual geometric transition speed independently of Shape Change Cadence. If it exceeds the cadence interval, the transition uses the full interval and remains continuously in motion rather than popping.")]
        [Range(MinimumFoamChipShapeTransitionTime, MaximumFoamChipShapeTransitionTime)]
        [SerializeField] private float foamChipShapeTransitionTime =
            DefaultFoamChipShapeTransitionTime;

        [Tooltip("Strength of the render-only structural Foam lineification. Zero produces the coherent Foam body; higher values create elongated anisotropic cuts and remnants.")]
        [Range(0f, 1f)]
        [SerializeField] private float foamStrandStrength;

        [Tooltip("Controls the broad-to-medium size hierarchy used by structural Strands. Zero retains medium subdivisions; one keeps broader, simpler structures.")]
        [FormerlySerializedAs("foamStrandSpacing")]
        [Range(0f, 1f)]
        [SerializeField] private float foamStrandScale =
            DefaultFoamStrandScale;

        [Tooltip("Controls how much of the candidate Strand field survives. Zero keeps sparse selected groups; one keeps denser groups. This does not change cut depth.")]
        [FormerlySerializedAs("foamStrandWidth")]
        [Range(0f, 1f)]
        [SerializeField] private float foamStrandDensity =
            DefaultFoamStrandDensity;

        [Tooltip("Controls how deeply selected Strand regions penetrate weak-to-medium Foam. Zero stays shallow near weak edges; one permits deeper channels. This does not change candidate density.")]
        [FormerlySerializedAs("foamStrandCurvature")]
        [Range(0f, 1f)]
        [SerializeField] private float foamStrandReach =
            DefaultFoamStrandReach;

        [Tooltip("Selects one of the retained Stage 6 diagnostics. Final is the normal rendered result; all obsolete Foam debug modes have been removed.")]
        [SerializeField] private StylizedRiverFoamDebugView foamDebugView =
            StylizedRiverFoamDebugView.Final;


        [FormerlySerializedAs("foamTestDistanceNormalized")]
        [Tooltip("Compatibility normalized longitudinal position. The source command resolves it to global river distance when submitted.")]
        [HideInInspector, SerializeField, Range(0f, 1f)]
        private float foamSpawnDistanceNormalized = 0.5f;

        [FormerlySerializedAs("foamTestAcrossNormalized")]
        [Tooltip("Compatibility normalized lateral position. The source command resolves it against the local left/right river half-width when submitted.")]
        [HideInInspector, SerializeField, Range(-1f, 1f)]
        private float foamSpawnAcrossNormalized;

        [Tooltip("World-space half-width of the canonical manual source. This is source material footprint size, not final Foam fracture or renderer detail.")]
        [FormerlySerializedAs("foamSpawnRibbonHalfWidth")]
        [FormerlySerializedAs("foamSpawnPatchRadius")]
        [FormerlySerializedAs("foamTestProgressiveRibbonHalfWidth")]
        [FormerlySerializedAs("foamTestRadius")]
        [HideInInspector, SerializeField]
        [Range(MinimumFoamSpawnScale, MaximumFoamSpawnScale)]
        private float foamSpawnScale = DefaultFoamSpawnScale;

        [Tooltip("Source-only coefficient controlling how much of the candidate birth shape becomes occupied Foam. It does not modify Initial Remaining Life or durability.")]
        [FormerlySerializedAs("foamTestAmount")]
        [HideInInspector, SerializeField, Range(0f, 1f)]
        private float foamSpawnAmount = 0.85f;

        [FormerlySerializedAs("foamTestRemainingLife")]
        [HideInInspector, SerializeField, Range(0f, 1f)]
        private float foamSpawnRemainingLife = 1f;


        [Tooltip("Duration of the budgeted moving-head manual source event.")]
        [FormerlySerializedAs("foamTestProgressiveRibbonDuration")]
        [HideInInspector, SerializeField]
        [Range(
            MinimumFoamProgressiveRibbonDuration,
            MaximumFoamProgressiveRibbonDuration)]
        private float foamSpawnRibbonDuration =
            DefaultFoamProgressiveRibbonDuration;

        [Tooltip("Net downstream distance travelled by the manual source head while the event is active.")]
        [FormerlySerializedAs("foamTestProgressiveRibbonTravelDistance")]
        [HideInInspector, SerializeField]
        [Range(
            MinimumFoamProgressiveRibbonTravelDistance,
            MaximumFoamProgressiveRibbonTravelDistance)]
        private float foamSpawnRibbonTravelDistance =
            DefaultFoamProgressiveRibbonTravelDistance;

        [Tooltip("Compatibility normalized lateral drift from event start to event end. Metric command callers provide drift directly in metres.")]
        [FormerlySerializedAs("foamTestProgressiveRibbonAcrossDrift")]
        [HideInInspector, SerializeField]
        [Range(
            MinimumFoamProgressiveRibbonAcrossDrift,
            MaximumFoamProgressiveRibbonAcrossDrift)]
        private float foamSpawnRibbonAcrossDrift =
            DefaultFoamProgressiveRibbonAcrossDrift;

        [Tooltip("Compatibility normalized bend strength. Zero follows only downstream travel and Across Drift; metric command callers provide the maximum bend directly in metres.")]
        [FormerlySerializedAs("foamTestProgressiveRibbonPathWander")]
        [HideInInspector, SerializeField]
        [Range(
            MinimumFoamProgressiveRibbonPathWander,
            MaximumFoamProgressiveRibbonPathWander)]
        private float foamSpawnRibbonPathWander =
            DefaultFoamProgressiveRibbonPathWander;

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
        private static readonly int ShoreWaveHeightScaleId =
            Shader.PropertyToID("_ShoreWaveHeightScale");
        private static readonly int ShoreWaveLengthScaleId =
            Shader.PropertyToID("_ShoreWaveLengthScale");
        private static readonly int ShoreWaveReachId =
            Shader.PropertyToID("_ShoreWaveReach");
        private static readonly int ShoreWaveTransitionLengthId =
            Shader.PropertyToID("_ShoreWaveTransitionLength");
        private static readonly int ShoreWaveSizeVariationId =
            Shader.PropertyToID("_ShoreWaveSizeVariation");
        private static readonly int ShoreWaveSideAsymmetryId =
            Shader.PropertyToID("_ShoreWaveSideAsymmetry");
        private static readonly int ShoreWaveProfileVariationId =
            Shader.PropertyToID("_ShoreWaveProfileVariation");
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

        private static readonly int FoamEnabledStage6Id =
            Shader.PropertyToID("_FoamEnabled");

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

#if UNITY_EDITOR
        private RiverEditorRegenerationBatch activeEditorRegenerationBatch;
        private string lastEditorRegenerationAccountingReport =
            "No River regeneration-accounting batch has completed yet.";
        private int nextEditorRegenerationBatchId = 1;
        private int editorRegenerationActivityRevision;
        private int scheduledEditorRegenerationActivityRevision;
        private bool editorRegenerationCompletionScheduled;
        private bool logNextEditorRegenerationBatch;
        private bool hasEditorDomainFingerprint;
        private GeneratedGeometryStableFingerprint lastEditorDomainFingerprint;
        private bool hasEditorGroundSnapshotFingerprint;
        private GeneratedGeometryStableFingerprint lastEditorGroundSnapshotFingerprint;
#endif
        private bool subscribedToSplineChanges;
        private StylizedRiverDisturbanceRuntime disturbanceRuntime;
        private StylizedRiverFoamRuntime foamRuntime;
        [NonSerialized] private bool foamStateHeld;

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
        public float ShoreMotionWidth => shoreMotionWidth;
        public float ShoreWaveHeightScale => shoreWaveHeightScale;
        public float ShoreWaveLengthScale => shoreWaveLengthScale;
        public float ShoreWaveReach => shoreWaveReach;
        public float ShoreWaveTransitionLength => shoreWaveTransitionLength;
        public float ShoreWaveSizeVariation => shoreWaveSizeVariation;
        public float ShoreWaveSideAsymmetry => shoreWaveSideAsymmetry;
        public float ShoreWaveProfileVariation => shoreWaveProfileVariation;
        public bool RuntimeDisturbancesEnabled => runtimeDisturbances;
        public StylizedRiverDisturbancePreset DisturbancePreset =>
            disturbancePreset;
        public StylizedRiverDisturbanceDebugView DisturbanceDebugView =>
            ResolveDisturbanceDebugView(disturbanceDebugView);

        // Canonical Stage 5 response settings are source-agnostic. The
        // stationary registry path and the dynamic-emitter path prepare
        // different source data, but both consume these Pressure/Wake rules.
        public float PressureStrength => staticPressureStrength;
        public float PressureFrontReachMetres => Mathf.Max(
            MinimumStaticPressureFrontReachMetres,
            staticPressureFrontReachMetres);
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
        public float ResolvedImpactRippleStrength =>
            ResolveImpactRippleStrength();
        public float ImpactRippleRidgeEmphasis =>
            impactRippleRidgeEmphasis;
        public float ImpactRipplePropagation => impactRipplePropagation;
        public float ImpactRippleDecay => impactRippleDecay;
        public float ImpactRippleFlowDissipation =>
            impactRippleFlowDissipation;
        public float ImpactRippleMinimumVisibleEnergy =>
            impactRippleMinimumVisibleEnergy;
        public float ImpactRippleMaximumLifetime =>
            impactRippleMaximumLifetime;
        public float ImpactRippleShoreReflection =>
            impactRippleShoreReflection;
        public float ImpactRippleObstacleReflection =>
            impactRippleObstacleReflection;
        public float ResolvedImpactRippleDecay =>
            ResolveImpactRippleDecay();
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

        public bool FoamEnabled => foamEnabled;
        public StylizedRiverFoamGridMode FoamGridMode => foamGridMode;
        public StylizedRiverFoamFixedMetricCellSize
            FoamFixedMetricCellSize => foamFixedMetricCellSize;
        public StylizedRiverFoamTransportScheme FoamTransportScheme =>
            foamTransportScheme == StylizedRiverFoamTransportScheme.TvdSuperbee
                ? StylizedRiverFoamTransportScheme.TvdSuperbee
                : StylizedRiverFoamTransportScheme.DonorCell;
        public float FoamFixedMetricRequestedCellSizeMetres =>
            ResolveFoamFixedMetricRequestedCellSizeMetres();
        public bool FoamStateHeld =>
            Application.isPlaying && foamEnabled && foamStateHeld;
        public StylizedRiverFoamTopologyCacheAsset FoamTopologyCacheAsset =>
            foamTopologyCacheAsset;
        public float FoamMajorSupportAmount =>
            Mathf.Clamp01(foamMajorSupportAmount);
        public float FoamMajorSupportSize =>
            Mathf.Clamp01(foamMajorSupportSize);
        public float FoamMajorSupportSizeVariation =>
            Mathf.Clamp01(foamMajorSupportSizeVariation);
        public float FoamMajorRecycleTerritoryDeviationPercent =>
            Mathf.Clamp(
                foamMajorRecycleTerritoryDeviationPercent,
                0f,
                10f);
        public float FoamMajorLifetimeUnits =>
            Mathf.Clamp(foamMajorLifetimeUnits, 1f, 20f);
        public float FoamMajorLifetimeUnitDeviation =>
            Mathf.Clamp(foamMajorLifetimeUnitDeviation, 0f, 10f);
        public int FoamMajorSupportSeed =>
            Mathf.Max(0, foamMajorSupportSeed);
        public float FoamConnectorAmount =>
            Mathf.Clamp01(foamConnectorAmount);
        public float FoamConnectorDirectness =>
            Mathf.Clamp01(foamConnectorDirectness);
        public float FoamConnectorLengthPreference =>
            Mathf.Clamp01(foamConnectorLengthPreference);
        public float FoamConnectorBreakStretchRatio =>
            Mathf.Clamp(foamConnectorBreakStretchRatio, 1.1f, 2f);
        public float FoamInteriorPocketAmount =>
            Mathf.Clamp01(foamInteriorPocketAmount);
        public float FoamEdgeCavityAmount =>
            Mathf.Clamp01(foamEdgeCavityAmount);
        public float FoamConnectorWeakSpanAmount =>
            Mathf.Clamp01(foamConnectorWeakSpanAmount);
        public float FoamFreeWaterEventAmount =>
            Mathf.Clamp01(foamFreeWaterEventAmount);
        public bool FoamAutomaticBirthEnabled => foamAutomaticBirthEnabled;
        public StylizedRiverFoamSourcePopulationPreset FoamSourcePopulationPreset =>
            foamSourcePopulationPreset;
        public bool FoamAutomaticShoreBirthEnabled =>
            foamAutomaticShoreBirthEnabled;
        public bool FoamAutomaticShoreBirthActive =>
            FoamAutomaticBirthEnabled &&
            FoamAutomaticShoreBirthEnabled &&
            FoamSourcePopulationPreset != StylizedRiverFoamSourcePopulationPreset.Off;
        public bool FoamAutomaticObjectBirthEnabled =>
            foamAutomaticObjectBirthEnabled;
        public bool FoamAutomaticObjectBirthActive =>
            FoamAutomaticBirthEnabled &&
            FoamAutomaticObjectBirthEnabled &&
            FoamSourcePopulationPreset != StylizedRiverFoamSourcePopulationPreset.Off;
        public bool FoamAutomaticFreeWaterBirthEnabled =>
            foamAutomaticFreeWaterBirthEnabled;
        public bool FoamAutomaticFreeWaterBirthActive =>
            FoamAutomaticBirthEnabled &&
            FoamAutomaticFreeWaterBirthEnabled &&
            FoamSourcePopulationPreset != StylizedRiverFoamSourcePopulationPreset.Off;
        public bool FoamSourcePopulationPresetImplemented =>
            FoamSourcePopulationPreset ==
                StylizedRiverFoamSourcePopulationPreset.ShoreContactTest ||
            FoamSourcePopulationPreset ==
                StylizedRiverFoamSourcePopulationPreset.RiverBodyTest ||
            FoamSourcePopulationPreset ==
                StylizedRiverFoamSourcePopulationPreset.ObstacleContactTest ||
            FoamSourcePopulationPreset ==
                StylizedRiverFoamSourcePopulationPreset.Custom ||
            FoamSourcePopulationPreset ==
                StylizedRiverFoamSourcePopulationPreset.BalancedMixedTest ||
            FoamSourcePopulationPreset ==
                StylizedRiverFoamSourcePopulationPreset.Off;
        public float FoamShoreFoamCoverage =>
            Mathf.Clamp01(foamShoreFoamCoverage);
        public float FoamShoreFoamActivity =>
            Mathf.Clamp01(foamShoreFoamActivity);
        public float FoamShoreMinimumPacketGapMetres =>
            Mathf.Clamp(
                foamShoreMinimumPacketGapMetres,
                MinimumFoamPacketGapMetres,
                MaximumFoamPacketGapMetres);
        public float FoamShoreFoamPatchSize =>
            Mathf.Clamp01(foamShoreFoamPatchSize);
        public float FoamShoreFoamFormationSpeedMetresPerSecond =>
            Mathf.Clamp(
                foamShoreFoamFormationSpeedMetresPerSecond,
                MinimumShoreFoamFormationSpeedMetresPerSecond,
                MaximumShoreFoamFormationSpeedMetresPerSecond);
        public StylizedRiverFoamShorePattern FoamShoreFoamPattern =>
            foamShoreFoamPattern;
        public float FoamShoreFoamSize =>
            FoamShoreFoamPatchSize;
        public float FoamShoreRibbonPatternWeight =>
            Mathf.Clamp01(foamShoreRibbonPatternWeight);
        public float FoamInwardWashPatternWeight =>
            Mathf.Clamp01(foamInwardWashPatternWeight);
        public float FoamShoreRibbonFormationSpeedMultiplier =>
            Mathf.Clamp(foamShoreRibbonFormationSpeedMultiplier, 0.10f, 3.00f);
        public float FoamShoreRibbonLengthMinMetres =>
            Mathf.Max(0.05f, Mathf.Min(
                foamShoreRibbonLengthMinMetres,
                foamShoreRibbonLengthMaxMetres));
        public float FoamShoreRibbonLengthMaxMetres =>
            Mathf.Max(FoamShoreRibbonLengthMinMetres, foamShoreRibbonLengthMaxMetres);
        public float FoamShoreRibbonThicknessCells =>
            Mathf.Clamp(foamShoreRibbonThicknessCells, 0.5f, 4f);
        public float FoamShoreRibbonOffsetMetres =>
            Mathf.Max(0f, foamShoreRibbonOffsetMetres);
        public float FoamShoreRibbonOffsetVariationCells =>
            Mathf.Clamp(foamShoreRibbonOffsetVariationCells, 0f, 0.5f);
        public float FoamShoreRibbonInitialPresenceMin =>
            Mathf.Clamp01(Mathf.Min(
                foamShoreRibbonInitialPresenceMin,
                foamShoreRibbonInitialPresenceMax));
        public float FoamShoreRibbonInitialPresenceMax =>
            Mathf.Clamp01(Mathf.Max(
                foamShoreRibbonInitialPresenceMin,
                foamShoreRibbonInitialPresenceMax));
        public float FoamShoreRibbonInitialLifeMin =>
            Mathf.Clamp01(Mathf.Min(
                foamShoreRibbonInitialLifeMin,
                foamShoreRibbonInitialLifeMax));
        public float FoamShoreRibbonInitialLifeMax =>
            Mathf.Clamp01(Mathf.Max(
                foamShoreRibbonInitialLifeMin,
                foamShoreRibbonInitialLifeMax));
        public float FoamInwardWashFormationSpeedMultiplier =>
            Mathf.Clamp(foamInwardWashFormationSpeedMultiplier, 0.10f, 3.00f);
        public float FoamInwardWashLengthMinMetres =>
            Mathf.Max(0.05f, Mathf.Min(
                foamInwardWashLengthMinMetres,
                foamInwardWashLengthMaxMetres));
        public float FoamInwardWashLengthMaxMetres =>
            Mathf.Max(FoamInwardWashLengthMinMetres, foamInwardWashLengthMaxMetres);
        public float FoamInwardWashWidthMinMetres =>
            Mathf.Max(0.005f, Mathf.Min(
                foamInwardWashWidthMinMetres,
                foamInwardWashWidthMaxMetres));
        public float FoamInwardWashWidthMaxMetres =>
            Mathf.Max(FoamInwardWashWidthMinMetres, foamInwardWashWidthMaxMetres);
        public float FoamInwardWashReachMinMetres =>
            Mathf.Max(0.005f, Mathf.Min(
                foamInwardWashReachMinMetres,
                foamInwardWashReachMaxMetres));
        public float FoamInwardWashReachMaxMetres =>
            Mathf.Max(FoamInwardWashReachMinMetres, foamInwardWashReachMaxMetres);
        public float FoamInwardWashOffsetMinMetres =>
            Mathf.Max(0f, Mathf.Min(
                foamInwardWashOffsetMinMetres,
                foamInwardWashOffsetMaxMetres));
        public float FoamInwardWashOffsetMaxMetres =>
            Mathf.Max(FoamInwardWashOffsetMinMetres, foamInwardWashOffsetMaxMetres);
        public float FoamInwardWashInitialPresenceMin =>
            Mathf.Clamp01(Mathf.Min(
                foamInwardWashInitialPresenceMin,
                foamInwardWashInitialPresenceMax));
        public float FoamInwardWashInitialPresenceMax =>
            Mathf.Clamp01(Mathf.Max(
                foamInwardWashInitialPresenceMin,
                foamInwardWashInitialPresenceMax));
        public float FoamInwardWashInitialLifeMin =>
            Mathf.Clamp01(Mathf.Min(
                foamInwardWashInitialLifeMin,
                foamInwardWashInitialLifeMax));
        public float FoamInwardWashInitialLifeMax =>
            Mathf.Clamp01(Mathf.Max(
                foamInwardWashInitialLifeMin,
                foamInwardWashInitialLifeMax));

        public float FoamObjectFoamCoverage =>
            Mathf.Clamp01(foamObjectFoamCoverage);
        public float FoamObjectContactCycleCoverage =>
            Mathf.Clamp01(foamObjectContactCycleCoverage);
        public float FoamObjectFoamActivity =>
            Mathf.Clamp01(foamObjectFoamActivity);
        public float FoamObjectContactMinimumPacketGapMetres =>
            Mathf.Clamp(
                foamObjectContactMinimumPacketGapMetres,
                MinimumFoamPacketGapMetres,
                MaximumFoamPacketGapMetres);
        public int FoamObjectContactStrokeCount =>
            Mathf.Clamp(
                foamObjectContactStrokeCount,
                MinimumObjectContactStrokeCount,
                MaximumObjectContactStrokeCount);
        public float FoamObjectFoamFormationSpeedMetresPerSecond =>
            Mathf.Clamp(
                foamObjectFoamFormationSpeedMetresPerSecond,
                MinimumShoreFoamFormationSpeedMetresPerSecond,
                MaximumShoreFoamFormationSpeedMetresPerSecond);
        public StylizedRiverFoamObjectPattern FoamObjectFoamPattern =>
            foamObjectFoamPattern;
        public bool FoamObjectContactCyclesEnabled =>
            foamObjectFoamPattern != StylizedRiverFoamObjectPattern.ContactFlecks &&
            (foamObjectFoamPattern != StylizedRiverFoamObjectPattern.Mixed ||
             FoamObjectContactArcPatternWeight +
             FoamObjectContactSemiArcPatternWeight > 0.0001f);
        public float FoamObjectContactArcPatternWeight =>
            Mathf.Clamp01(foamObjectContactArcPatternWeight);
        public float FoamObjectContactArcFormationSpeedMultiplier =>
            Mathf.Clamp(foamObjectContactArcFormationSpeedMultiplier, 0.10f, 3.00f);
        public float FoamObjectContactArcLengthMinMetres =>
            Mathf.Max(0.05f, Mathf.Min(
                foamObjectContactArcLengthMinMetres,
                foamObjectContactArcLengthMaxMetres));
        public float FoamObjectContactArcLengthMaxMetres =>
            Mathf.Max(FoamObjectContactArcLengthMinMetres, foamObjectContactArcLengthMaxMetres);
        public float FoamObjectContactArcWakeArmLengthMinMetres =>
            FoamObjectContactArcLengthMinMetres;
        public float FoamObjectContactArcWakeArmLengthMaxMetres =>
            FoamObjectContactArcLengthMaxMetres;
        public float FoamObjectContactArcAlongFlowContactOffsetMetres =>
            ResolveFiniteContactOffset(foamObjectContactArcAlongFlowContactOffsetMetres);
        public float FoamObjectContactArcAcrossRiverContactOffsetMetres =>
            ResolveFiniteContactOffset(foamObjectContactArcAcrossRiverContactOffsetMetres);
        public float FoamObjectContactArcWidthMinMetres =>
            Mathf.Max(0.005f, Mathf.Min(
                foamObjectContactArcWidthMinMetres,
                foamObjectContactArcWidthMaxMetres));
        public float FoamObjectContactArcWidthMaxMetres =>
            Mathf.Max(FoamObjectContactArcWidthMinMetres, foamObjectContactArcWidthMaxMetres);
        public float FoamObjectContactArcOffsetMinMetres =>
            Mathf.Max(0f, Mathf.Min(
                foamObjectContactArcOffsetMinMetres,
                foamObjectContactArcOffsetMaxMetres));
        public float FoamObjectContactArcOffsetMaxMetres =>
            Mathf.Max(FoamObjectContactArcOffsetMinMetres, foamObjectContactArcOffsetMaxMetres);
        public float FoamObjectContactArcInitialPresenceMin =>
            Mathf.Clamp01(Mathf.Min(
                foamObjectContactArcInitialPresenceMin,
                foamObjectContactArcInitialPresenceMax));
        public float FoamObjectContactArcInitialPresenceMax =>
            Mathf.Clamp01(Mathf.Max(
                foamObjectContactArcInitialPresenceMin,
                foamObjectContactArcInitialPresenceMax));
        public float FoamObjectContactArcInitialLifeMin =>
            Mathf.Clamp01(Mathf.Min(
                foamObjectContactArcInitialLifeMin,
                foamObjectContactArcInitialLifeMax));
        public float FoamObjectContactArcInitialLifeMax =>
            Mathf.Clamp01(Mathf.Max(
                foamObjectContactArcInitialLifeMin,
                foamObjectContactArcInitialLifeMax));
        public float FoamObjectContactSemiArcPatternWeight =>
            Mathf.Clamp01(foamObjectContactSemiArcPatternWeight);
        public float FoamObjectContactSemiArcFormationSpeedMultiplier =>
            Mathf.Clamp(foamObjectContactSemiArcFormationSpeedMultiplier, 0.10f, 3.00f);
        public float FoamObjectContactSemiArcLengthMinMetres =>
            Mathf.Max(0.05f, Mathf.Min(
                foamObjectContactSemiArcLengthMinMetres,
                foamObjectContactSemiArcLengthMaxMetres));
        public float FoamObjectContactSemiArcLengthMaxMetres =>
            Mathf.Max(FoamObjectContactSemiArcLengthMinMetres, foamObjectContactSemiArcLengthMaxMetres);
        public float FoamObjectContactSemiArcWakeArmLengthMinMetres =>
            FoamObjectContactSemiArcLengthMinMetres;
        public float FoamObjectContactSemiArcWakeArmLengthMaxMetres =>
            FoamObjectContactSemiArcLengthMaxMetres;
        public float FoamObjectContactSemiArcAlongFlowContactOffsetMetres =>
            ResolveFiniteContactOffset(foamObjectContactSemiArcAlongFlowContactOffsetMetres);
        public float FoamObjectContactSemiArcAcrossRiverContactOffsetMetres =>
            ResolveFiniteContactOffset(foamObjectContactSemiArcAcrossRiverContactOffsetMetres);
        public float FoamObjectContactSemiArcWidthMinMetres =>
            Mathf.Max(0.005f, Mathf.Min(
                foamObjectContactSemiArcWidthMinMetres,
                foamObjectContactSemiArcWidthMaxMetres));
        public float FoamObjectContactSemiArcWidthMaxMetres =>
            Mathf.Max(FoamObjectContactSemiArcWidthMinMetres, foamObjectContactSemiArcWidthMaxMetres);
        public float FoamObjectContactSemiArcOffsetMinMetres =>
            Mathf.Max(0f, Mathf.Min(
                foamObjectContactSemiArcOffsetMinMetres,
                foamObjectContactSemiArcOffsetMaxMetres));
        public float FoamObjectContactSemiArcOffsetMaxMetres =>
            Mathf.Max(FoamObjectContactSemiArcOffsetMinMetres, foamObjectContactSemiArcOffsetMaxMetres);
        public float FoamObjectContactSemiArcInitialPresenceMin =>
            Mathf.Clamp01(Mathf.Min(
                foamObjectContactSemiArcInitialPresenceMin,
                foamObjectContactSemiArcInitialPresenceMax));
        public float FoamObjectContactSemiArcInitialPresenceMax =>
            Mathf.Clamp01(Mathf.Max(
                foamObjectContactSemiArcInitialPresenceMin,
                foamObjectContactSemiArcInitialPresenceMax));
        public float FoamObjectContactSemiArcInitialLifeMin =>
            Mathf.Clamp01(Mathf.Min(
                foamObjectContactSemiArcInitialLifeMin,
                foamObjectContactSemiArcInitialLifeMax));
        public float FoamObjectContactSemiArcInitialLifeMax =>
            Mathf.Clamp01(Mathf.Max(
                foamObjectContactSemiArcInitialLifeMin,
                foamObjectContactSemiArcInitialLifeMax));
        public float FoamObjectContactFleckFormationSpeedMultiplier =>
            Mathf.Clamp(foamObjectContactFleckFormationSpeedMultiplier, 0.10f, 3.00f);
        public float FoamObjectContactFleckLengthMinMetres =>
            Mathf.Max(0.05f, Mathf.Min(
                foamObjectContactFleckLengthMinMetres,
                foamObjectContactFleckLengthMaxMetres));
        public float FoamObjectContactFleckLengthMaxMetres =>
            Mathf.Max(FoamObjectContactFleckLengthMinMetres, foamObjectContactFleckLengthMaxMetres);
        public float FoamObjectContactFleckWidthMinMetres =>
            Mathf.Max(0.005f, Mathf.Min(
                foamObjectContactFleckWidthMinMetres,
                foamObjectContactFleckWidthMaxMetres));
        public float FoamObjectContactFleckWidthMaxMetres =>
            Mathf.Max(FoamObjectContactFleckWidthMinMetres, foamObjectContactFleckWidthMaxMetres);
        public float FoamObjectContactFleckOffsetMinMetres =>
            Mathf.Max(0f, Mathf.Min(
                foamObjectContactFleckOffsetMinMetres,
                foamObjectContactFleckOffsetMaxMetres));
        public float FoamObjectContactFleckOffsetMaxMetres =>
            Mathf.Max(FoamObjectContactFleckOffsetMinMetres, foamObjectContactFleckOffsetMaxMetres);
        public float FoamObjectContactFleckInitialPresenceMin =>
            Mathf.Clamp01(Mathf.Min(
                foamObjectContactFleckInitialPresenceMin,
                foamObjectContactFleckInitialPresenceMax));
        public float FoamObjectContactFleckInitialPresenceMax =>
            Mathf.Clamp01(Mathf.Max(
                foamObjectContactFleckInitialPresenceMin,
                foamObjectContactFleckInitialPresenceMax));
        public float FoamObjectContactFleckInitialLifeMin =>
            Mathf.Clamp01(Mathf.Min(
                foamObjectContactFleckInitialLifeMin,
                foamObjectContactFleckInitialLifeMax));
        public float FoamObjectContactFleckInitialLifeMax =>
            Mathf.Clamp01(Mathf.Max(
                foamObjectContactFleckInitialLifeMin,
                foamObjectContactFleckInitialLifeMax));

        public float FoamFreeWaterFoamCoverage =>
            Mathf.Clamp01(foamFreeWaterFoamCoverage);
        public float FoamFreeWaterFoamActivity =>
            Mathf.Clamp01(foamFreeWaterFoamActivity);
        public float FoamFreeWaterMinimumPacketGapMetres =>
            Mathf.Clamp(
                foamFreeWaterMinimumPacketGapMetres,
                MinimumFoamPacketGapMetres,
                MaximumFoamPacketGapMetres);
        public float FoamFreeWaterFoamFormationSpeedMetresPerSecond =>
            Mathf.Clamp(
                foamFreeWaterFoamFormationSpeedMetresPerSecond,
                MinimumShoreFoamFormationSpeedMetresPerSecond,
                MaximumShoreFoamFormationSpeedMetresPerSecond);
        public StylizedRiverFoamFreeWaterPattern FoamFreeWaterFoamPattern =>
            foamFreeWaterFoamPattern;
        public float FoamFreeWaterLaceConnectorPatternWeight =>
            Mathf.Clamp01(foamFreeWaterLaceConnectorPatternWeight);
        public float FoamFreeWaterCrossLaceConnectorPatternWeight =>
            Mathf.Clamp01(foamFreeWaterCrossLaceConnectorPatternWeight);
        public float FoamFreeWaterTornFragmentPatternWeight =>
            Mathf.Clamp01(foamFreeWaterTornFragmentPatternWeight);
        public float FoamFreeWaterLaceFormationSpeedMultiplier =>
            Mathf.Clamp(foamFreeWaterLaceFormationSpeedMultiplier, 0.10f, 3.00f);
        public float FoamFreeWaterLaceLengthMinMetres =>
            Mathf.Max(0.05f, Mathf.Min(
                foamFreeWaterLaceLengthMinMetres,
                foamFreeWaterLaceLengthMaxMetres));
        public float FoamFreeWaterLaceLengthMaxMetres =>
            Mathf.Max(FoamFreeWaterLaceLengthMinMetres, foamFreeWaterLaceLengthMaxMetres);
        public float FoamFreeWaterLaceWidthMinMetres =>
            Mathf.Max(0.005f, Mathf.Min(
                foamFreeWaterLaceWidthMinMetres,
                foamFreeWaterLaceWidthMaxMetres));
        public float FoamFreeWaterLaceWidthMaxMetres =>
            Mathf.Max(FoamFreeWaterLaceWidthMinMetres, foamFreeWaterLaceWidthMaxMetres);
        public float FoamFreeWaterLaceInitialPresenceMin =>
            Mathf.Clamp01(Mathf.Min(
                foamFreeWaterLaceInitialPresenceMin,
                foamFreeWaterLaceInitialPresenceMax));
        public float FoamFreeWaterLaceInitialPresenceMax =>
            Mathf.Clamp01(Mathf.Max(
                foamFreeWaterLaceInitialPresenceMin,
                foamFreeWaterLaceInitialPresenceMax));
        public float FoamFreeWaterLaceInitialLifeMin =>
            Mathf.Clamp01(Mathf.Min(
                foamFreeWaterLaceInitialLifeMin,
                foamFreeWaterLaceInitialLifeMax));
        public float FoamFreeWaterLaceInitialLifeMax =>
            Mathf.Clamp01(Mathf.Max(
                foamFreeWaterLaceInitialLifeMin,
                foamFreeWaterLaceInitialLifeMax));
        public float FoamFreeWaterLaceCurvatureMin =>
            Mathf.Clamp01(Mathf.Min(
                foamFreeWaterLaceCurvatureMin,
                foamFreeWaterLaceCurvatureMax));
        public float FoamFreeWaterLaceCurvatureMax =>
            Mathf.Clamp01(Mathf.Max(
                foamFreeWaterLaceCurvatureMin,
                foamFreeWaterLaceCurvatureMax));
        public float FoamFreeWaterCrossLaceFormationSpeedMultiplier =>
            Mathf.Clamp(foamFreeWaterCrossLaceFormationSpeedMultiplier, 0.10f, 3.00f);
        public float FoamFreeWaterCrossLaceLengthMinMetres =>
            Mathf.Max(0.05f, Mathf.Min(
                foamFreeWaterCrossLaceLengthMinMetres,
                foamFreeWaterCrossLaceLengthMaxMetres));
        public float FoamFreeWaterCrossLaceLengthMaxMetres =>
            Mathf.Max(FoamFreeWaterCrossLaceLengthMinMetres, foamFreeWaterCrossLaceLengthMaxMetres);
        public float FoamFreeWaterCrossLaceWidthMinMetres =>
            Mathf.Max(0.005f, Mathf.Min(
                foamFreeWaterCrossLaceWidthMinMetres,
                foamFreeWaterCrossLaceWidthMaxMetres));
        public float FoamFreeWaterCrossLaceWidthMaxMetres =>
            Mathf.Max(FoamFreeWaterCrossLaceWidthMinMetres, foamFreeWaterCrossLaceWidthMaxMetres);
        public float FoamFreeWaterCrossLaceInitialPresenceMin =>
            Mathf.Clamp01(Mathf.Min(
                foamFreeWaterCrossLaceInitialPresenceMin,
                foamFreeWaterCrossLaceInitialPresenceMax));
        public float FoamFreeWaterCrossLaceInitialPresenceMax =>
            Mathf.Clamp01(Mathf.Max(
                foamFreeWaterCrossLaceInitialPresenceMin,
                foamFreeWaterCrossLaceInitialPresenceMax));
        public float FoamFreeWaterCrossLaceInitialLifeMin =>
            Mathf.Clamp01(Mathf.Min(
                foamFreeWaterCrossLaceInitialLifeMin,
                foamFreeWaterCrossLaceInitialLifeMax));
        public float FoamFreeWaterCrossLaceInitialLifeMax =>
            Mathf.Clamp01(Mathf.Max(
                foamFreeWaterCrossLaceInitialLifeMin,
                foamFreeWaterCrossLaceInitialLifeMax));
        public float FoamFreeWaterFragmentFormationSpeedMultiplier =>
            Mathf.Clamp(foamFreeWaterFragmentFormationSpeedMultiplier, 0.10f, 3.00f);
        public float FoamFreeWaterFragmentLengthMinMetres =>
            Mathf.Max(0.05f, Mathf.Min(
                foamFreeWaterFragmentLengthMinMetres,
                foamFreeWaterFragmentLengthMaxMetres));
        public float FoamFreeWaterFragmentLengthMaxMetres =>
            Mathf.Max(FoamFreeWaterFragmentLengthMinMetres, foamFreeWaterFragmentLengthMaxMetres);
        public float FoamFreeWaterFragmentWidthMinMetres =>
            Mathf.Max(0.005f, Mathf.Min(
                foamFreeWaterFragmentWidthMinMetres,
                foamFreeWaterFragmentWidthMaxMetres));
        public float FoamFreeWaterFragmentWidthMaxMetres =>
            Mathf.Max(FoamFreeWaterFragmentWidthMinMetres, foamFreeWaterFragmentWidthMaxMetres);
        public float FoamFreeWaterFragmentInitialPresenceMin =>
            Mathf.Clamp01(Mathf.Min(
                foamFreeWaterFragmentInitialPresenceMin,
                foamFreeWaterFragmentInitialPresenceMax));
        public float FoamFreeWaterFragmentInitialPresenceMax =>
            Mathf.Clamp01(Mathf.Max(
                foamFreeWaterFragmentInitialPresenceMin,
                foamFreeWaterFragmentInitialPresenceMax));
        public float FoamFreeWaterFragmentInitialLifeMin =>
            Mathf.Clamp01(Mathf.Min(
                foamFreeWaterFragmentInitialLifeMin,
                foamFreeWaterFragmentInitialLifeMax));
        public float FoamFreeWaterFragmentInitialLifeMax =>
            Mathf.Clamp01(Mathf.Max(
                foamFreeWaterFragmentInitialLifeMin,
                foamFreeWaterFragmentInitialLifeMax));
        public float FoamShoreFoamStrength =>
            Mathf.Clamp01(foamShoreFoamStrength);
        public float FoamShoreFoamPersistence =>
            Mathf.Clamp01(foamShoreFoamPersistence);
        public float FoamNeutralLifetime =>
            Mathf.Clamp(
                foamNeutralLifetime,
                MinimumFoamNeutralLifetime,
                MaximumFoamNeutralLifetime);
        public float FoamSupportedAgingRate =>
            Mathf.Clamp(
                foamSupportedAgingRate,
                MinimumFoamSupportedAgingRate,
                MaximumFoamSupportedAgingRate);
        public float FoamFullSupportedAgingAt =>
            Mathf.Clamp(
                foamFullSupportedAgingAt,
                MinimumFoamFullSupportedAgingAt,
                MaximumFoamFullSupportedAgingAt);
        public StylizedRiverFinalFoamVisibilityMode FoamFinalVisibilityMode =>
            foamFinalVisibilityMode ==
                StylizedRiverFinalFoamVisibilityMode.LifecycleFaithful
                ? StylizedRiverFinalFoamVisibilityMode.LifecycleFaithful
                : StylizedRiverFinalFoamVisibilityMode.ConcentrationAndLifetime;
        public StylizedRiverFoamPresenceFootprintMode
            FoamPresenceFootprintMode =>
                foamPresenceFootprintMode ==
                    StylizedRiverFoamPresenceFootprintMode.PresenceAmplitude
                    ? StylizedRiverFoamPresenceFootprintMode.PresenceAmplitude
                    : StylizedRiverFoamPresenceFootprintMode.Current;
        public float FoamNegativeAgingRate =>
            Mathf.Clamp(
                foamNegativeAgingRate,
                MinimumFoamNegativeAgingRate,
                MaximumFoamNegativeAgingRate);
        public float FoamDownstreamSpeedRatio =>
            Mathf.Clamp(
                foamMaterialFlowSpeedMultiplier,
                MinimumFoamDownstreamSpeedRatio,
                MaximumFoamDownstreamSpeedRatio);
        public float FoamMaximumLateralSpeedRatio =>
            Mathf.Clamp(
                foamMotionFieldStrength,
                MinimumFoamMaximumLateralSpeedRatio,
                MaximumFoamMaximumLateralSpeedRatio);
        public float FoamLaneAdvectionRatio =>
            Mathf.Clamp(
                foamMotionFieldScrollHz,
                MinimumFoamLaneAdvectionRatio,
                MaximumFoamLaneAdvectionRatio);
        public float FoamLowLateralMotionCoverage =>
            Mathf.Clamp(
                foamMotionFieldNeutralCoverage,
                MinimumFoamLowLateralMotionCoverage,
                MaximumFoamLowLateralMotionCoverage);
        public float FoamDirectionChangeFrequency =>
            Mathf.Clamp(
                foamMotionFieldLaneScale,
                MinimumFoamDirectionChangeFrequency,
                MaximumFoamDirectionChangeFrequency);
        public float FoamAcrossRiverCoherence =>
            Mathf.Clamp(
                foamMotionFieldAcrossRiverCoherence,
                MinimumFoamAcrossRiverCoherence,
                MaximumFoamAcrossRiverCoherence);
        public float FoamObstacleSlowdownStrength =>
            Mathf.Clamp(
                foamObstacleSlowdownStrength,
                MinimumFoamObstacleSlowdownStrength,
                MaximumFoamObstacleSlowdownStrength);
        public float FoamObstacleMinimumDownstreamFactor =>
            Mathf.Clamp(
                foamObstacleMinimumDownstreamFactor,
                MinimumFoamObstacleMinimumDownstreamFactor,
                MaximumFoamObstacleMinimumDownstreamFactor);
        public float FoamObjectContactFullSlowdownReachMetres =>
            Mathf.Max(0f, foamObjectContactFullSlowdownReachMetres);
        public float FoamObjectContactSlowdownOuterReachMetres =>
            Mathf.Max(
                FoamObjectContactFullSlowdownReachMetres,
                foamObjectContactSlowdownOuterReachMetres);
        public float FoamVisualOccupancyBuildTime =>
            Mathf.Clamp(
                foamVisualOccupancyBuildTime,
                MinimumFoamVisualOccupancyBuildTime,
                MaximumFoamVisualOccupancyBuildTime);
        public float FoamVisualOccupancyReleaseTime =>
            Mathf.Clamp(
                foamVisualOccupancyReleaseTime,
                MinimumFoamVisualOccupancyReleaseTime,
                MaximumFoamVisualOccupancyReleaseTime);
        public Color FoamColour => foamColour;
        public float FoamInteriorOpacityFloor =>
            Mathf.Clamp01(foamInteriorOpacityFloor);
        public float FoamEdgeContrast =>
            Mathf.Clamp(foamEdgeContrast, -1f, 1f);
        public float FoamChipActivation =>
            Mathf.Clamp01(foamChipActivation);
        public float FoamChipCandidateSpacing =>
            Mathf.Clamp(
                foamChipCandidateSpacing,
                MinimumFoamChipCandidateSpacing,
                MaximumFoamChipCandidateSpacing);
        public float FoamChipSize =>
            Mathf.Clamp01(foamChipSize);
        public float FoamChipIrregularity =>
            Mathf.Clamp01(foamChipIrregularity);
        public float FoamChipStableScreenRadiusPixels =>
            Mathf.Clamp(
                foamChipStableScreenRadiusPixels,
                MinimumFoamChipStableScreenRadiusPixels,
                MaximumFoamChipStableScreenRadiusPixels);
        public float FoamChipMaximumViewScale =>
            Mathf.Clamp(
                foamChipMaximumViewScale,
                MinimumFoamChipMaximumViewScale,
                MaximumFoamChipMaximumViewScale);
        public float FoamChipEdgeWidthPixels =>
            Mathf.Max(0f, foamChipEdgeWidthPixels);
        public float FoamChipSoftEdgeStart =>
            Mathf.Clamp(
                foamChipSoftEdgeStart,
                MinimumFoamChipSoftEdgeStart,
                MaximumFoamChipSoftEdgeStart);
        public float FoamChipInteriorAccess =>
            Mathf.Clamp01(foamChipInteriorAccess);
        public float FoamChipFieldSpeed =>
            Mathf.Clamp(
                foamChipFieldSpeed,
                MinimumFoamChipFieldSpeed,
                MaximumFoamChipFieldSpeed);
        public float FoamChipFormationTime =>
            Mathf.Clamp(
                foamChipFormationTime,
                MinimumFoamChipLifecycleTime,
                MaximumFoamChipLifecycleTime);
        public float FoamChipStableTime =>
            Mathf.Clamp(
                foamChipStableTime,
                MinimumFoamChipLifecycleTime,
                MaximumFoamChipLifecycleTime);
        public float FoamChipDissolveTime =>
            Mathf.Clamp(
                foamChipDissolveTime,
                MinimumFoamChipLifecycleTime,
                MaximumFoamChipLifecycleTime);
        public float FoamChipDormantTime =>
            Mathf.Clamp(
                foamChipDormantTime,
                MinimumFoamChipLifecycleTime,
                MaximumFoamChipLifecycleTime);
        public float FoamChipLateralMotionAmount =>
            Mathf.Clamp(
                foamChipLateralMotionAmount,
                MinimumFoamChipLateralMotionAmount,
                MaximumFoamChipLateralMotionAmount);
        public float FoamChipLateralMotionSpeed =>
            Mathf.Clamp(
                foamChipLateralMotionSpeed,
                MinimumFoamChipMotionSpeed,
                MaximumFoamChipMotionSpeed);
        public float FoamChipRotationAmountDegrees =>
            Mathf.Clamp(
                foamChipRotationAmountDegrees,
                MinimumFoamChipRotationAmountDegrees,
                MaximumFoamChipRotationAmountDegrees);
        public float FoamChipRotationSpeed =>
            Mathf.Clamp(
                foamChipRotationSpeed,
                MinimumFoamChipMotionSpeed,
                MaximumFoamChipMotionSpeed);
        public float FoamChipSizePulseAmount =>
            Mathf.Clamp(
                foamChipSizePulseAmount,
                MinimumFoamChipSizePulseAmount,
                MaximumFoamChipSizePulseAmount);
        public float FoamChipSizePulseSpeed =>
            Mathf.Clamp(
                foamChipSizePulseSpeed,
                MinimumFoamChipMotionSpeed,
                MaximumFoamChipMotionSpeed);
        public float FoamChipShapeChangeAmount =>
            Mathf.Clamp01(foamChipShapeChangeAmount);
        public float FoamChipShapeChangeSpeed =>
            Mathf.Clamp(
                foamChipShapeChangeSpeed,
                MinimumFoamChipMotionSpeed,
                MaximumFoamChipMotionSpeed);
        public float FoamChipShapeTransitionTime =>
            Mathf.Clamp(
                foamChipShapeTransitionTime,
                MinimumFoamChipShapeTransitionTime,
                MaximumFoamChipShapeTransitionTime);
        public float FoamStrandStrength =>
            Mathf.Clamp01(foamStrandStrength);
        public float FoamStrandScale =>
            Mathf.Clamp01(foamStrandScale);
        public float FoamStrandDensity =>
            Mathf.Clamp01(foamStrandDensity);
        public float FoamStrandReach =>
            Mathf.Clamp01(foamStrandReach);
        public StylizedRiverFoamDebugView FoamDebugView => foamDebugView;
        public float FoamSpawnDistanceNormalized =>
            foamSpawnDistanceNormalized;
        public float FoamSpawnAcrossNormalized =>
            foamSpawnAcrossNormalized;
        public float FoamSpawnScale =>
            Mathf.Clamp(
                foamSpawnScale,
                MinimumFoamSpawnScale,
                MaximumFoamSpawnScale);
        public float FoamSpawnAmount => foamSpawnAmount;
        public float FoamSpawnRemainingLife => foamSpawnRemainingLife;
        public float FoamSpawnRibbonDuration =>
            Mathf.Clamp(
                foamSpawnRibbonDuration,
                MinimumFoamProgressiveRibbonDuration,
                MaximumFoamProgressiveRibbonDuration);
        public float FoamSpawnRibbonTravelDistance =>
            Mathf.Clamp(
                foamSpawnRibbonTravelDistance,
                MinimumFoamProgressiveRibbonTravelDistance,
                MaximumFoamProgressiveRibbonTravelDistance);
        public float FoamSpawnRibbonAcrossDrift =>
            Mathf.Clamp(
                foamSpawnRibbonAcrossDrift,
                MinimumFoamProgressiveRibbonAcrossDrift,
                MaximumFoamProgressiveRibbonAcrossDrift);
        public float FoamSpawnRibbonPathWander =>
            Mathf.Clamp(
                foamSpawnRibbonPathWander,
                MinimumFoamProgressiveRibbonPathWander,
                MaximumFoamProgressiveRibbonPathWander);

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

        private void NormalizeShorePatternWeights()
        {
            foamShoreRibbonPatternWeight = Mathf.Clamp01(
                foamShoreRibbonPatternWeight);
            foamInwardWashPatternWeight = Mathf.Clamp01(
                foamInwardWashPatternWeight);
            float total = foamShoreRibbonPatternWeight +
                foamInwardWashPatternWeight;
            if (total <= 0.0001f)
            {
                foamShoreRibbonPatternWeight = 0.88f;
                foamInwardWashPatternWeight = 0.12f;
                return;
            }

            foamShoreRibbonPatternWeight /= total;
            foamInwardWashPatternWeight /= total;
        }

        private void NormalizeObjectPatternWeights()
        {
            foamObjectContactArcPatternWeight = Mathf.Clamp01(
                foamObjectContactArcPatternWeight);
            foamObjectContactSemiArcPatternWeight = Mathf.Clamp01(
                foamObjectContactSemiArcPatternWeight);
            float total = foamObjectContactArcPatternWeight +
                foamObjectContactSemiArcPatternWeight;
            if (total <= 0.0001f)
            {
                foamObjectContactArcPatternWeight = 0.56f;
                foamObjectContactSemiArcPatternWeight = 0.44f;
                return;
            }

            foamObjectContactArcPatternWeight /= total;
            foamObjectContactSemiArcPatternWeight /= total;
        }

        private void NormalizeFreeWaterPatternWeights()
        {
            foamFreeWaterLaceConnectorPatternWeight = Mathf.Clamp01(
                foamFreeWaterLaceConnectorPatternWeight);
            foamFreeWaterCrossLaceConnectorPatternWeight = Mathf.Clamp01(
                foamFreeWaterCrossLaceConnectorPatternWeight);
            foamFreeWaterTornFragmentPatternWeight = Mathf.Clamp01(
                foamFreeWaterTornFragmentPatternWeight);
            float total = foamFreeWaterLaceConnectorPatternWeight +
                foamFreeWaterCrossLaceConnectorPatternWeight +
                foamFreeWaterTornFragmentPatternWeight;
            if (total <= 0.0001f)
            {
                foamFreeWaterLaceConnectorPatternWeight = 0.30f;
                foamFreeWaterCrossLaceConnectorPatternWeight = 0.45f;
                foamFreeWaterTornFragmentPatternWeight = 0.25f;
                return;
            }

            foamFreeWaterLaceConnectorPatternWeight /= total;
            foamFreeWaterCrossLaceConnectorPatternWeight /= total;
            foamFreeWaterTornFragmentPatternWeight /= total;
        }

        private void SanitizeShoreFoamPatternControls()
        {
            foamShoreRibbonFormationSpeedMultiplier = Mathf.Clamp(
                foamShoreRibbonFormationSpeedMultiplier,
                0.10f,
                3.00f);
            SanitizePositiveRange(
                ref foamShoreRibbonLengthMinMetres,
                ref foamShoreRibbonLengthMaxMetres,
                0.05f);
            foamShoreRibbonThicknessCells = Mathf.Clamp(
                foamShoreRibbonThicknessCells,
                0.5f,
                4f);
            foamShoreRibbonOffsetMetres = Mathf.Max(
                0f,
                foamShoreRibbonOffsetMetres);
            foamShoreRibbonOffsetVariationCells = Mathf.Clamp(
                foamShoreRibbonOffsetVariationCells,
                0f,
                0.5f);
            SanitizeUnitRange(
                ref foamShoreRibbonInitialPresenceMin,
                ref foamShoreRibbonInitialPresenceMax);
            SanitizeUnitRange(
                ref foamShoreRibbonInitialLifeMin,
                ref foamShoreRibbonInitialLifeMax);

            foamInwardWashFormationSpeedMultiplier = Mathf.Clamp(
                foamInwardWashFormationSpeedMultiplier,
                0.10f,
                3.00f);
            SanitizePositiveRange(
                ref foamInwardWashLengthMinMetres,
                ref foamInwardWashLengthMaxMetres,
                0.05f);
            SanitizePositiveRange(
                ref foamInwardWashWidthMinMetres,
                ref foamInwardWashWidthMaxMetres,
                0.005f);
            SanitizePositiveRange(
                ref foamInwardWashReachMinMetres,
                ref foamInwardWashReachMaxMetres,
                0.005f);
            SanitizePositiveRange(
                ref foamInwardWashOffsetMinMetres,
                ref foamInwardWashOffsetMaxMetres,
                0f);
            SanitizeUnitRange(
                ref foamInwardWashInitialPresenceMin,
                ref foamInwardWashInitialPresenceMax);
            SanitizeUnitRange(
                ref foamInwardWashInitialLifeMin,
                ref foamInwardWashInitialLifeMax);
        }

        private void SanitizeObjectFoamPatternControls()
        {
            foamObjectFoamCoverage = Mathf.Clamp01(foamObjectFoamCoverage);
            foamObjectContactCycleCoverage = Mathf.Clamp01(
                foamObjectContactCycleCoverage);
            foamObjectFoamActivity = Mathf.Clamp01(foamObjectFoamActivity);
            foamObjectContactMinimumPacketGapMetres = Mathf.Clamp(
                foamObjectContactMinimumPacketGapMetres,
                MinimumFoamPacketGapMetres,
                MaximumFoamPacketGapMetres);
            foamObjectContactStrokeCount = Mathf.Clamp(
                foamObjectContactStrokeCount,
                MinimumObjectContactStrokeCount,
                MaximumObjectContactStrokeCount);
            foamObjectFoamFormationSpeedMetresPerSecond = Mathf.Clamp(
                foamObjectFoamFormationSpeedMetresPerSecond,
                MinimumShoreFoamFormationSpeedMetresPerSecond,
                MaximumShoreFoamFormationSpeedMetresPerSecond);
            NormalizeObjectPatternWeights();
            foamObjectContactArcFormationSpeedMultiplier = Mathf.Clamp(
                foamObjectContactArcFormationSpeedMultiplier,
                0.10f,
                3.00f);
            SanitizePositiveRange(
                ref foamObjectContactArcLengthMinMetres,
                ref foamObjectContactArcLengthMaxMetres,
                0.05f);
            SanitizeFiniteContactOffset(
                ref foamObjectContactArcAlongFlowContactOffsetMetres);
            SanitizeFiniteContactOffset(
                ref foamObjectContactArcAcrossRiverContactOffsetMetres);
            SanitizePositiveRange(
                ref foamObjectContactArcWidthMinMetres,
                ref foamObjectContactArcWidthMaxMetres,
                0.005f);
            SanitizePositiveRange(
                ref foamObjectContactArcOffsetMinMetres,
                ref foamObjectContactArcOffsetMaxMetres,
                0f);
            SanitizeUnitRange(
                ref foamObjectContactArcInitialPresenceMin,
                ref foamObjectContactArcInitialPresenceMax);
            SanitizeUnitRange(
                ref foamObjectContactArcInitialLifeMin,
                ref foamObjectContactArcInitialLifeMax);

            foamObjectContactSemiArcFormationSpeedMultiplier = Mathf.Clamp(
                foamObjectContactSemiArcFormationSpeedMultiplier,
                0.10f,
                3.00f);
            SanitizePositiveRange(
                ref foamObjectContactSemiArcLengthMinMetres,
                ref foamObjectContactSemiArcLengthMaxMetres,
                0.05f);
            SanitizeFiniteContactOffset(
                ref foamObjectContactSemiArcAlongFlowContactOffsetMetres);
            SanitizeFiniteContactOffset(
                ref foamObjectContactSemiArcAcrossRiverContactOffsetMetres);
            SanitizePositiveRange(
                ref foamObjectContactSemiArcWidthMinMetres,
                ref foamObjectContactSemiArcWidthMaxMetres,
                0.005f);
            SanitizePositiveRange(
                ref foamObjectContactSemiArcOffsetMinMetres,
                ref foamObjectContactSemiArcOffsetMaxMetres,
                0f);
            SanitizeUnitRange(
                ref foamObjectContactSemiArcInitialPresenceMin,
                ref foamObjectContactSemiArcInitialPresenceMax);
            SanitizeUnitRange(
                ref foamObjectContactSemiArcInitialLifeMin,
                ref foamObjectContactSemiArcInitialLifeMax);

            foamObjectContactFleckFormationSpeedMultiplier = Mathf.Clamp(
                foamObjectContactFleckFormationSpeedMultiplier,
                0.10f,
                3.00f);
            SanitizePositiveRange(
                ref foamObjectContactFleckLengthMinMetres,
                ref foamObjectContactFleckLengthMaxMetres,
                0.05f);
            SanitizePositiveRange(
                ref foamObjectContactFleckWidthMinMetres,
                ref foamObjectContactFleckWidthMaxMetres,
                0.005f);
            SanitizePositiveRange(
                ref foamObjectContactFleckOffsetMinMetres,
                ref foamObjectContactFleckOffsetMaxMetres,
                0f);
            SanitizeUnitRange(
                ref foamObjectContactFleckInitialPresenceMin,
                ref foamObjectContactFleckInitialPresenceMax);
            SanitizeUnitRange(
                ref foamObjectContactFleckInitialLifeMin,
                ref foamObjectContactFleckInitialLifeMax);
        }

        private void SanitizeFreeWaterFoamPatternControls()
        {
            foamFreeWaterFoamCoverage = Mathf.Clamp01(foamFreeWaterFoamCoverage);
            foamFreeWaterFoamActivity = Mathf.Clamp01(foamFreeWaterFoamActivity);
            foamFreeWaterMinimumPacketGapMetres = Mathf.Clamp(
                foamFreeWaterMinimumPacketGapMetres,
                MinimumFoamPacketGapMetres,
                MaximumFoamPacketGapMetres);
            foamFreeWaterFoamFormationSpeedMetresPerSecond = Mathf.Clamp(
                foamFreeWaterFoamFormationSpeedMetresPerSecond,
                MinimumShoreFoamFormationSpeedMetresPerSecond,
                MaximumShoreFoamFormationSpeedMetresPerSecond);
            NormalizeFreeWaterPatternWeights();

            foamFreeWaterLaceFormationSpeedMultiplier = Mathf.Clamp(
                foamFreeWaterLaceFormationSpeedMultiplier,
                0.10f,
                3.00f);
            SanitizePositiveRange(
                ref foamFreeWaterLaceLengthMinMetres,
                ref foamFreeWaterLaceLengthMaxMetres,
                0.05f);
            SanitizePositiveRange(
                ref foamFreeWaterLaceWidthMinMetres,
                ref foamFreeWaterLaceWidthMaxMetres,
                0.005f);
            SanitizeUnitRange(
                ref foamFreeWaterLaceInitialPresenceMin,
                ref foamFreeWaterLaceInitialPresenceMax);
            SanitizeUnitRange(
                ref foamFreeWaterLaceInitialLifeMin,
                ref foamFreeWaterLaceInitialLifeMax);
            SanitizeUnitRange(
                ref foamFreeWaterLaceCurvatureMin,
                ref foamFreeWaterLaceCurvatureMax);

            foamFreeWaterCrossLaceFormationSpeedMultiplier = Mathf.Clamp(
                foamFreeWaterCrossLaceFormationSpeedMultiplier,
                0.10f,
                3.00f);
            SanitizePositiveRange(
                ref foamFreeWaterCrossLaceLengthMinMetres,
                ref foamFreeWaterCrossLaceLengthMaxMetres,
                0.05f);
            SanitizePositiveRange(
                ref foamFreeWaterCrossLaceWidthMinMetres,
                ref foamFreeWaterCrossLaceWidthMaxMetres,
                0.005f);
            SanitizeUnitRange(
                ref foamFreeWaterCrossLaceInitialPresenceMin,
                ref foamFreeWaterCrossLaceInitialPresenceMax);
            SanitizeUnitRange(
                ref foamFreeWaterCrossLaceInitialLifeMin,
                ref foamFreeWaterCrossLaceInitialLifeMax);

            foamFreeWaterFragmentFormationSpeedMultiplier = Mathf.Clamp(
                foamFreeWaterFragmentFormationSpeedMultiplier,
                0.10f,
                3.00f);
            SanitizePositiveRange(
                ref foamFreeWaterFragmentLengthMinMetres,
                ref foamFreeWaterFragmentLengthMaxMetres,
                0.05f);
            SanitizePositiveRange(
                ref foamFreeWaterFragmentWidthMinMetres,
                ref foamFreeWaterFragmentWidthMaxMetres,
                0.005f);
            SanitizeUnitRange(
                ref foamFreeWaterFragmentInitialPresenceMin,
                ref foamFreeWaterFragmentInitialPresenceMax);
            SanitizeUnitRange(
                ref foamFreeWaterFragmentInitialLifeMin,
                ref foamFreeWaterFragmentInitialLifeMax);
        }

        private static void SanitizePositiveRange(
            ref float minimum,
            ref float maximum,
            float floor)
        {
            minimum = Mathf.Max(floor, minimum);
            maximum = Mathf.Max(floor, maximum);
            if (maximum < minimum)
            {
                float previousMinimum = minimum;
                minimum = maximum;
                maximum = previousMinimum;
            }
        }

        private static float ResolveFiniteContactOffset(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value)
                ? 0f
                : value;
        }

        private static void SanitizeFiniteContactOffset(ref float value)
        {
            value = ResolveFiniteContactOffset(value);
        }

        private static void SanitizeUnitRange(
            ref float minimum,
            ref float maximum)
        {
            minimum = Mathf.Clamp01(minimum);
            maximum = Mathf.Clamp01(maximum);
            if (maximum < minimum)
            {
                float previousMinimum = minimum;
                minimum = maximum;
                maximum = previousMinimum;
            }
        }

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

#if UNITY_EDITOR
        public string LastEditorRegenerationAccountingReport =>
            lastEditorRegenerationAccountingReport;

        public void ClearEditorRegenerationAccounting()
        {
            activeEditorRegenerationBatch = null;
            lastEditorRegenerationAccountingReport =
                "No River regeneration-accounting batch has completed yet.";
            editorRegenerationActivityRevision++;
        }

        public void LogNextEditorRegenerationBatchOnce()
        {
            logNextEditorRegenerationBatch = true;
        }
#endif

        private void Reset()
        {
            splineContainer = GetComponent<SplineContainer>();
            AssignWaterLayer();
        }

        private void OnEnable()
        {
            foamStateHeld = false;
            MigrateFoamMaterialLifecycleTuningIfRequired();
            MigrateFoamVelocityTuningIfRequired();
            foamDebugView = ResolveFoamDebugView(foamDebugView);
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
            EnsureFoamRuntime();
            SetRendererEnabled(true);
            RegenerateAll(
                RiverRegenerationRequestOrigin.OnEnable,
                true);
            lastEditorTime = Time.realtimeSinceStartupAsDouble;
        }

        private void OnDisable()
        {
            foamStateHeld = false;
            UnsubscribeFromSplineChanges();

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            foamRuntime ??=
                GetComponent<StylizedRiverFoamRuntime>();

            if (disturbanceRuntime != null)
            {
                disturbanceRuntime.enabled = false;
            }

            if (foamRuntime != null)
            {
                foamRuntime.enabled = false;
            }

            SetRendererEnabled(false);
        }

        private static StylizedRiverDisturbanceDebugView
            ResolveDisturbanceDebugView(
                StylizedRiverDisturbanceDebugView value)
        {
            int rawValue = (int)value;
            if ((rawValue >= 0 && rawValue <= 8) ||
                rawValue == 19 || rawValue == 21)
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


        private static StylizedRiverFoamDebugView ResolveFoamDebugView(
            StylizedRiverFoamDebugView value)
        {
            switch ((int)value)
            {
                case (int)StylizedRiverFoamDebugView.FoamAndAgingTopology:
                    return StylizedRiverFoamDebugView.FoamAndAgingTopology;
                case (int)StylizedRiverFoamDebugView.AutomaticBirthSources:
                    return StylizedRiverFoamDebugView.AutomaticBirthSources;
                case (int)StylizedRiverFoamDebugView.MaterialPresence:
                    return StylizedRiverFoamDebugView.MaterialPresence;
                case (int)StylizedRiverFoamDebugView.MaterialRemainingLife:
                    return StylizedRiverFoamDebugView.MaterialRemainingLife;
                case (int)StylizedRiverFoamDebugView.FoamMotionField:
                    return StylizedRiverFoamDebugView.FoamMotionField;
                case (int)StylizedRiverFoamDebugView.FoamMotionFieldCellGrid:
                    return StylizedRiverFoamDebugView.FoamMotionFieldCellGrid;
                case (int)StylizedRiverFoamDebugView.FoamEvaluatedShape:
                    return StylizedRiverFoamDebugView.FoamEvaluatedShape;
                case (int)StylizedRiverFoamDebugView.FoamShapeDifference:
                    return StylizedRiverFoamDebugView.FoamShapeDifference;
                case (int)StylizedRiverFoamDebugView.FoamChipAndStrandProbe:
                    return StylizedRiverFoamDebugView.FoamChipAndStrandProbe;
                case (int)StylizedRiverFoamDebugView.FoamChipAndStrandDifference:
                    return StylizedRiverFoamDebugView.FoamChipAndStrandDifference;
                case (int)StylizedRiverFoamDebugView.FoamFilmSource:
                    return StylizedRiverFoamDebugView.FoamFilmSource;
                case (int)StylizedRiverFoamDebugView.FoamFilmSupport:
                    return StylizedRiverFoamDebugView.FoamFilmSupport;
                case (int)StylizedRiverFoamDebugView.FoamFilmTarget:
                    return StylizedRiverFoamDebugView.FoamFilmTarget;
                case (int)StylizedRiverFoamDebugView.FoamTemporalOccupancy:
                    return StylizedRiverFoamDebugView.FoamTemporalOccupancy;
                case (int)StylizedRiverFoamDebugView.FoamTemporalDifference:
                    return StylizedRiverFoamDebugView.FoamTemporalDifference;
                case (int)StylizedRiverFoamDebugView.FoamEvaluatedFinalPreview:
                    return StylizedRiverFoamDebugView.FoamEvaluatedFinalPreview;
                case (int)StylizedRiverFoamDebugView.ChipCandidateField:
                    return StylizedRiverFoamDebugView.ChipCandidateField;
                case (int)StylizedRiverFoamDebugView.ProductionChipMask:
                    return StylizedRiverFoamDebugView.ProductionChipMask;
                case (int)StylizedRiverFoamDebugView.ChipEligibilityComposite:
                    return StylizedRiverFoamDebugView.ChipEligibilityComposite;
                default:
                    return StylizedRiverFoamDebugView.Final;
            }
        }

        private void PreserveLegacyFoamSurfaceMorphSerializationOnly()
        {
            _ = foamSurfaceMorphStrength;
            _ = foamSurfaceMorphCalibrationVersion;
        }

        private void MigrateFoamMaterialLifecycleTuningIfRequired()
        {
            if (foamMaterialLifecycleTuningVersion >=
                CurrentFoamMaterialLifecycleTuningVersion)
            {
                return;
            }

            foamNeutralLifetime = DefaultFoamNeutralLifetime;
            foamSupportedAgingRate = DefaultFoamSupportedAgingRate;
            foamNegativeAgingRate = DefaultFoamNegativeAgingRate;
            foamMaterialFlowSpeedMultiplier =
                DefaultFoamDownstreamSpeedRatio;
            foamMaterialLifecycleTuningVersion =
                CurrentFoamMaterialLifecycleTuningVersion;
        }

        private void MigrateFoamVelocityTuningIfRequired()
        {
            if (foamVelocityTuningVersion >= CurrentFoamVelocityTuningVersion)
            {
                return;
            }

            // Legacy Motion Field Strength was an arbitrary cell displacement
            // scalar whose accepted baseline was one. Preserve authored relative
            // strength while converting that baseline to a physical speed ratio.
            float legacyStrength = Mathf.Max(0f, foamMotionFieldStrength);
            foamMotionFieldStrength = Mathf.Clamp(
                legacyStrength *
                (DefaultFoamMaximumLateralSpeedRatio /
                 LegacyDefaultFoamMotionFieldStrength),
                MinimumFoamMaximumLateralSpeedRatio,
                MaximumFoamMaximumLateralSpeedRatio);

            // Legacy scroll was expressed in full-field wraps per second and
            // therefore changed physical speed with river length. Preserve the
            // accepted 0.01 baseline as a 0.60 advection ratio; all future
            // scrolling is resolved from physical Foam speed and cell spacing.
            float legacyScrollHz = Mathf.Max(0f, foamMotionFieldScrollHz);
            foamMotionFieldScrollHz = Mathf.Clamp(
                legacyScrollHz *
                (DefaultFoamLaneAdvectionRatio /
                 LegacyDefaultFoamMotionFieldScrollHz),
                MinimumFoamLaneAdvectionRatio,
                MaximumFoamLaneAdvectionRatio);

            foamObstacleSlowdownStrength =
                DefaultFoamObstacleSlowdownStrength;
            foamObstacleMinimumDownstreamFactor =
                DefaultFoamObstacleMinimumDownstreamFactor;
            foamVelocityTuningVersion = CurrentFoamVelocityTuningVersion;
        }

        private void OnValidate()
        {
            MigrateFoamMaterialLifecycleTuningIfRequired();
            MigrateFoamVelocityTuningIfRequired();
            foamDebugView = ResolveFoamDebugView(foamDebugView);
            disturbanceDebugView =
                ResolveDisturbanceDebugView(disturbanceDebugView);
            ValidateSettings();
            if (!foamEnabled)
            {
                foamStateHeld = false;
            }
            CacheComponents();
            ResolveSplineContainer();
            AssignWaterLayer();
            if (!Application.isPlaying)
            {
                RemoveLegacyGeneratedObjects();
                EnsureSurfaceOutput();
                EnsureCorridorOutput();
            }
            EnsureDisturbanceRuntime();
            EnsureFoamRuntime();
            ApplyVisualSettings();

            // OnValidate is shared by structural, simulation, rendering, and
            // diagnostic controls. Full regeneration is therefore routed only
            // by the custom Inspector's structural sections and spline-change
            // callback. Render and runtime tuning must preserve live Foam state.
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
                RegenerateAll(
                    RiverRegenerationRequestOrigin.InspectorStructural,
                    false);
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
            RegenerateAll(
                RiverRegenerationRequestOrigin.ExplicitRegenerateAll,
                true);
        }

        private void RegenerateAll(
            RiverRegenerationRequestOrigin origin,
            bool recordRequest)
        {
#if UNITY_EDITOR
            if (recordRequest)
            {
                BeginEditorRegenerationRequest(origin, false);
            }
            long editorPassStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
#endif
            ValidateSettings();
            CacheComponents();
            ResolveSplineContainer();
            AssignWaterLayer();
            RemoveLegacyGeneratedObjects();
            EnsureSurfaceOutput();
            EnsureCorridorOutput();
            BuildRiverDomain();
            BuildSurface();

            bool parentGroundCommitted =
                NotifyParentGround(
                    origin == RiverRegenerationRequestOrigin.OnEnable);
            if (!parentGroundCommitted)
            {
                // A deferred Play-startup Ground transaction has not yet
                // committed its final height field. Build a cheap immediate
                // corridor against retained Ground data when available, or the
                // established fallback sampler otherwise, so River output stays
                // available until Ground commits once.
                BuildCorridor();
            }

            ApplyVisualSettings();
            NotifyReflectionSurfaceChanged();
            NotifyFoamRuntimeChanged();
#if UNITY_EDITOR
            RecordEditorRegenerationPass(
                RiverEditorRegenerationPassKind.Full,
                ResolveEditorElapsedMilliseconds(editorPassStartedAt));
#endif
        }

        [ContextMenu("Rebuild Surface Only")]
        public void RebuildSurfaceOnly()
        {
#if UNITY_EDITOR
            BeginEditorRegenerationRequest(
                RiverRegenerationRequestOrigin.RebuildSurfaceOnly,
                false);
            long editorPassStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
#endif
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
            NotifyFoamRuntimeChanged();
#if UNITY_EDITOR
            RecordEditorRegenerationPass(
                RiverEditorRegenerationPassKind.SurfaceOnly,
                ResolveEditorElapsedMilliseconds(editorPassStartedAt));
#endif
        }

        /// <summary>
        /// Rebuilds only the dedicated visible river corridor after the parent
        /// generated ground has refreshed its base-height field and concealed
        /// broad ground mesh.
        /// </summary>
        public void RebuildCorridorFromGround()
        {
#if UNITY_EDITOR
            BeginEditorRegenerationRequest(
                RiverRegenerationRequestOrigin.GroundCorridorChanged,
                false);
#endif
            if (!isActiveAndEnabled)
            {
                return;
            }

#if UNITY_EDITOR
            long editorPassStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
#endif
            ValidateSettings();
            CacheComponents();
            EnsureRiverDomain();
            EnsureCorridorOutput();
            BuildCorridor();
#if UNITY_EDITOR
            RecordEditorRegenerationPass(
                RiverEditorRegenerationPassKind.CorridorOnly,
                ResolveEditorElapsedMilliseconds(editorPassStartedAt));
#endif
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
            NotifyFoamRuntimeChanged();
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
            RequestRegeneration(
                RiverRegenerationRequestOrigin.ChannelPreset);
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
                    ResetShoreWaveProfileControls();
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
                    ResetShoreWaveProfileControls();
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
                    ResetShoreWaveProfileControls();
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
                    ResetShoreWaveProfileControls();
                    break;

                case StylizedRiverMotionPreset.Custom:
                    break;
            }

            ValidateSettings();
            RebuildSurfaceOnly();
        }

        private void ResetShoreWaveProfileControls()
        {
            shoreWaveHeightScale = 1f;
            shoreWaveLengthScale = 1f;
            shoreWaveReach = 1f;
            shoreWaveTransitionLength = 1f;
            shoreWaveSizeVariation = 0f;
            shoreWaveSideAsymmetry = 0f;
            shoreWaveProfileVariation = 0f;
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
                    staticPressureFrontReachMetres =
                        DefaultStaticPressureFrontReachMetres;
                    obstructionWakeStrength = 0f;
                    obstructionWakeVariation = 0f;
                    obstructionWakeVariationIntervalMin =
                        DefaultStaticWakeVariationIntervalMin;
                    obstructionWakeVariationIntervalMax =
                        DefaultStaticWakeVariationIntervalMax;
                    obstructionWakeWidening = 0.65f;
                    impactRippleStrength = 0f;
                    impactRippleRidgeEmphasis = 1.15f;
                    impactRippleFlowDissipation = 0.15f;
                    impactRippleMinimumVisibleEnergy = 0.04f;
                    impactRippleMaximumLifetime = 8f;
                    impactRippleShoreReflection = 0.25f;
                    impactRippleObstacleReflection = 0.50f;
                    break;

                case StylizedRiverDisturbancePreset.Subtle:
                    runtimeDisturbances = true;
                    staticPressureStrength = 0f;
                    staticPressureFrontReachMetres =
                        DefaultStaticPressureFrontReachMetres;
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
                    impactRippleStrength = 0.50f;
                    impactRippleRidgeEmphasis = 1.05f;
                    impactRipplePropagation = 0.82f;
                    impactRippleDecay = 1.25f;
                    impactRippleFlowDissipation = 0.25f;
                    impactRippleMinimumVisibleEnergy = 0.06f;
                    impactRippleMaximumLifetime = 5f;
                    impactRippleShoreReflection = 0.18f;
                    impactRippleObstacleReflection = 0.42f;
                    break;

                case StylizedRiverDisturbancePreset.Balanced:
                    runtimeDisturbances = true;
                    staticPressureStrength = 0.65f;
                    staticPressureFrontReachMetres =
                        DefaultStaticPressureFrontReachMetres;
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
                    impactRippleStrength = 1f;
                    impactRippleRidgeEmphasis = 1.15f;
                    impactRipplePropagation = 1.05f;
                    impactRippleDecay = 0.85f;
                    impactRippleFlowDissipation = 0.15f;
                    impactRippleMinimumVisibleEnergy = 0.04f;
                    impactRippleMaximumLifetime = 8f;
                    impactRippleShoreReflection = 0.25f;
                    impactRippleObstacleReflection = 0.50f;
                    break;

                case StylizedRiverDisturbancePreset.Reactive:
                    runtimeDisturbances = true;
                    staticPressureStrength = 0.90f;
                    staticPressureFrontReachMetres = 0.16f;
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
                    impactRippleStrength = 1.50f;
                    impactRippleRidgeEmphasis = 1.25f;
                    impactRipplePropagation = 1.3f;
                    impactRippleDecay = 0.55f;
                    impactRippleFlowDissipation = 0.08f;
                    impactRippleMinimumVisibleEnergy = 0.025f;
                    impactRippleMaximumLifetime = 10f;
                    impactRippleShoreReflection = 0.32f;
                    impactRippleObstacleReflection = 0.58f;
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

        public StylizedRiverFoamRuntime GetOrCreateFoamRuntime()
        {
            foamEnabled = true;
            EnsureFoamRuntime();
            return foamRuntime;
        }

        public bool StartFoamSpawn()
        {
            StylizedRiverFoamRuntime runtime = GetOrCreateFoamRuntime();
            if (runtime == null)
            {
                return false;
            }

            return runtime.StartFoamCompositionNormalized(
                foamSpawnDistanceNormalized,
                foamSpawnAcrossNormalized,
                FoamSpawnScale,
                foamSpawnAmount,
                foamSpawnRemainingLife,
                foamSpawnRibbonDuration,
                foamSpawnRibbonTravelDistance,
                foamSpawnRibbonAcrossDrift,
                foamSpawnRibbonPathWander);
        }

        public bool TryResolveFoamSpawnMetricPlacement(
            out float globalDistance,
            out float lateralMetres,
            out float driftMetres,
            out float maximumBendMetres)
        {
            globalDistance = 0f;
            lateralMetres = 0f;
            driftMetres = 0f;
            maximumBendMetres = 0f;
            if (!Domain.IsValid)
            {
                return false;
            }

            globalDistance = Mathf.Lerp(
                Domain.GlobalDistanceMinimum,
                Domain.GlobalDistanceMaximum,
                Mathf.Clamp01(foamSpawnDistanceNormalized));
            StylizedRiverSplineSample sample =
                Domain.SampleAtGlobalDistance(globalDistance);
            float startAcross = Mathf.Clamp(
                foamSpawnAcrossNormalized,
                -1f,
                1f);
            lateralMetres = startAcross < 0f
                ? startAcross * sample.LeftHalfWidth
                : startAcross * sample.RightHalfWidth;
            float targetAcross = Mathf.Clamp(
                startAcross + FoamSpawnRibbonAcrossDrift,
                -1f,
                1f);
            float targetLateral = targetAcross < 0f
                ? targetAcross * sample.LeftHalfWidth
                : targetAcross * sample.RightHalfWidth;
            driftMetres = targetLateral - lateralMetres;
            maximumBendMetres = FoamSpawnRibbonPathWander *
                FoamSpawnMaximumBendAcross * Mathf.Max(
                    sample.LeftHalfWidth,
                    sample.RightHalfWidth);
            return true;
        }

        public bool StartFoamSpawnMetric(
            float globalDistance,
            float lateralMetres,
            float scale,
            float amount,
            float remainingLife,
            float duration,
            float travelDistanceMetres,
            float lateralDriftMetres,
            float maximumBendMetres)
        {
            StylizedRiverFoamRuntime runtime = GetOrCreateFoamRuntime();
            return runtime != null && runtime.StartFoamCompositionMetric(
                globalDistance,
                lateralMetres,
                scale,
                amount,
                remainingLife,
                duration,
                travelDistanceMetres,
                lateralDriftMetres,
                maximumBendMetres);
        }

        public bool EmitFoamMetric(
            float globalDistance,
            float lateralMetres,
            float radiusMetres,
            float amount,
            float initialRemainingLife,
            float elongation)
        {
            StylizedRiverFoamRuntime runtime = GetOrCreateFoamRuntime();
            return runtime != null && runtime.EmitMetric(
                globalDistance,
                lateralMetres,
                radiusMetres,
                amount,
                initialRemainingLife,
                elongation);
        }

        public bool ClearAndEmitFoamIsolatedLifeProbe(
            bool absoluteAging = false)
        {
            StylizedRiverFoamRuntime runtime = GetOrCreateFoamRuntime();
            return runtime != null && runtime.EmitIsolatedLifeProbe(
                foamSpawnDistanceNormalized,
                foamSpawnAcrossNormalized,
                absoluteAging);
        }

        public bool ClearAndEmitFoamAbsoluteLifeProbe()
        {
            return ClearAndEmitFoamIsolatedLifeProbe(true);
        }

        public void ClearFoam()
        {
            foamRuntime?.ClearFoam();
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
            NotifyFoamRuntimeChanged();
        }

        public void SetCustomFreezeAmount(float amount)
        {
            customFreezeAmount = Mathf.Clamp01(amount);
            surfaceState = StylizedRiverSurfaceState.Custom;
            ApplyVisualSettings();
            NotifyReflectionSurfaceChanged();
            NotifyFoamRuntimeChanged();
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

            float resolvedGroundGridSpacing = ResolveGroundGridSpacing();
            StylizedRiverGroundSnapshot snapshot =
                new StylizedRiverGroundSnapshot(
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
                    resolvedGroundGridSpacing,
                    shorelineWetClearance,
                    shorelineBankCover,
                    reservedDownwardSurfaceDisplacement);
#if UNITY_EDITOR
            RecordEditorGroundSnapshotFingerprint(
                CalculateEditorGroundSnapshotFingerprint(
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
                    resolvedGroundGridSpacing,
                    shorelineWetClearance,
                    shorelineBankCover,
                    reservedDownwardSurfaceDisplacement));
#endif
            return snapshot;
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

        private float ResolveFoamFixedMetricRequestedCellSizeMetres()
        {
            return foamFixedMetricCellSize switch
            {
                StylizedRiverFoamFixedMetricCellSize.Metres0_25 =>
                    StylizedRiverFoamGridDescriptor
                        .ConservativeCandidateCellSizeMetres,
                StylizedRiverFoamFixedMetricCellSize.Metres0_20 =>
                    StylizedRiverFoamGridDescriptor
                        .IntermediateCandidateCellSizeMetres,
                StylizedRiverFoamFixedMetricCellSize.Metres0_15 =>
                    StylizedRiverFoamGridDescriptor
                        .TargetCandidateCellSizeMetres,
                StylizedRiverFoamFixedMetricCellSize.Metres0_10 =>
                    StylizedRiverFoamGridDescriptor
                        .StressCandidateCellSizeMetres,
                _ => StylizedRiverFoamGridDescriptor
                    .ResolveProvisionalRequestedCellSizeMetres(quality)
            };
        }

        private void ValidateSettings()
        {
            if (!Enum.IsDefined(typeof(StylizedRiverFoamGridMode), foamGridMode))
            {
                foamGridMode = StylizedRiverFoamGridMode.FixedMetric;
            }
            if (!Enum.IsDefined(
                    typeof(StylizedRiverFoamFixedMetricCellSize),
                    foamFixedMetricCellSize))
            {
                foamFixedMetricCellSize =
                    StylizedRiverFoamFixedMetricCellSize.QualityDefault;
            }
            if (!Enum.IsDefined(
                    typeof(StylizedRiverFoamTransportScheme),
                    foamTransportScheme))
            {
                foamTransportScheme =
                    StylizedRiverFoamTransportScheme.DonorCell;
            }

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
            shoreWaveHeightScale = Mathf.Clamp(
                shoreWaveHeightScale,
                0f,
                2.5f);
            shoreWaveLengthScale = Mathf.Clamp(
                shoreWaveLengthScale,
                0.25f,
                4f);
            shoreWaveReach = Mathf.Clamp01(shoreWaveReach);
            shoreWaveTransitionLength = Mathf.Clamp(
                shoreWaveTransitionLength,
                0.25f,
                3f);
            shoreWaveSizeVariation = Mathf.Clamp01(
                shoreWaveSizeVariation);
            shoreWaveSideAsymmetry = Mathf.Clamp01(
                shoreWaveSideAsymmetry);
            shoreWaveProfileVariation = Mathf.Clamp01(
                shoreWaveProfileVariation);

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
            staticPressureFrontReachMetres = Mathf.Max(
                MinimumStaticPressureFrontReachMetres,
                staticPressureFrontReachMetres);
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
                4f);
            impactRippleRidgeEmphasis = Mathf.Clamp(
                impactRippleRidgeEmphasis,
                0.75f,
                1.50f);
            impactRipplePropagation = Mathf.Clamp(
                impactRipplePropagation,
                0.2f,
                2.5f);
            impactRippleDecay = Mathf.Clamp(
                impactRippleDecay,
                0.1f,
                3f);
            impactRippleFlowDissipation = Mathf.Clamp(
                impactRippleFlowDissipation,
                0f,
                1.5f);
            impactRippleMinimumVisibleEnergy = Mathf.Clamp(
                impactRippleMinimumVisibleEnergy,
                0.01f,
                0.20f);
            impactRippleMaximumLifetime = Mathf.Clamp(
                impactRippleMaximumLifetime,
                1f,
                15f);
            impactRippleShoreReflection = Mathf.Clamp(
                impactRippleShoreReflection,
                0f,
                0.60f);
            impactRippleObstacleReflection = Mathf.Clamp(
                impactRippleObstacleReflection,
                0f,
                0.85f);
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
            foamMajorSupportAmount = Mathf.Clamp01(foamMajorSupportAmount);
            foamMajorSupportSize = Mathf.Clamp01(foamMajorSupportSize);
            foamMajorSupportSizeVariation = Mathf.Clamp01(
                foamMajorSupportSizeVariation);
            foamMajorRecycleTerritoryDeviationPercent = Mathf.Clamp(
                foamMajorRecycleTerritoryDeviationPercent,
                0f,
                10f);
            foamMajorLifetimeUnits = Mathf.Clamp(
                foamMajorLifetimeUnits,
                1f,
                20f);
            foamMajorLifetimeUnitDeviation = Mathf.Clamp(
                foamMajorLifetimeUnitDeviation,
                0f,
                10f);
            foamMajorSupportSeed = Mathf.Max(0, foamMajorSupportSeed);
            foamConnectorAmount = Mathf.Clamp01(foamConnectorAmount);
            foamConnectorDirectness = Mathf.Clamp01(
                foamConnectorDirectness);
            foamConnectorLengthPreference = Mathf.Clamp01(
                foamConnectorLengthPreference);
            foamConnectorBreakStretchRatio = Mathf.Clamp(
                foamConnectorBreakStretchRatio,
                1.1f,
                2f);
            foamInteriorPocketAmount = Mathf.Clamp01(
                foamInteriorPocketAmount);
            foamEdgeCavityAmount = Mathf.Clamp01(
                foamEdgeCavityAmount);
            foamConnectorWeakSpanAmount = Mathf.Clamp01(
                foamConnectorWeakSpanAmount);
            foamFreeWaterEventAmount = Mathf.Clamp01(
                foamFreeWaterEventAmount);
            foamShoreFoamCoverage = Mathf.Clamp01(
                foamShoreFoamCoverage);
            foamShoreFoamActivity = Mathf.Clamp01(
                foamShoreFoamActivity);
            foamShoreMinimumPacketGapMetres = Mathf.Clamp(
                foamShoreMinimumPacketGapMetres,
                MinimumFoamPacketGapMetres,
                MaximumFoamPacketGapMetres);
            foamShoreFoamPatchSize = Mathf.Clamp01(
                foamShoreFoamPatchSize);
            foamShoreFoamFormationSpeedMetresPerSecond = Mathf.Clamp(
                foamShoreFoamFormationSpeedMetresPerSecond,
                MinimumShoreFoamFormationSpeedMetresPerSecond,
                MaximumShoreFoamFormationSpeedMetresPerSecond);
            NormalizeShorePatternWeights();
            SanitizeShoreFoamPatternControls();
            SanitizeObjectFoamPatternControls();
            SanitizeFreeWaterFoamPatternControls();
            foamShoreFoamStrength = Mathf.Clamp01(
                foamShoreFoamStrength);
            foamShoreFoamPersistence = Mathf.Clamp01(
                foamShoreFoamPersistence);
            foamNeutralLifetime = Mathf.Clamp(
                foamNeutralLifetime,
                MinimumFoamNeutralLifetime,
                MaximumFoamNeutralLifetime);
            foamSupportedAgingRate = Mathf.Clamp(
                foamSupportedAgingRate,
                MinimumFoamSupportedAgingRate,
                MaximumFoamSupportedAgingRate);
            foamFullSupportedAgingAt = Mathf.Clamp(
                foamFullSupportedAgingAt,
                MinimumFoamFullSupportedAgingAt,
                MaximumFoamFullSupportedAgingAt);
            foamFinalVisibilityMode = FoamFinalVisibilityMode;
            foamPresenceFootprintMode = FoamPresenceFootprintMode;
            foamNegativeAgingRate = Mathf.Clamp(
                foamNegativeAgingRate,
                MinimumFoamNegativeAgingRate,
                MaximumFoamNegativeAgingRate);
            foamMaterialFlowSpeedMultiplier = Mathf.Clamp(
                foamMaterialFlowSpeedMultiplier,
                MinimumFoamDownstreamSpeedRatio,
                MaximumFoamDownstreamSpeedRatio);
            foamMotionFieldStrength = Mathf.Clamp(
                foamMotionFieldStrength,
                MinimumFoamMaximumLateralSpeedRatio,
                MaximumFoamMaximumLateralSpeedRatio);
            foamMotionFieldScrollHz = Mathf.Clamp(
                foamMotionFieldScrollHz,
                MinimumFoamLaneAdvectionRatio,
                MaximumFoamLaneAdvectionRatio);
            foamMotionFieldNeutralCoverage = Mathf.Clamp(
                foamMotionFieldNeutralCoverage,
                MinimumFoamLowLateralMotionCoverage,
                MaximumFoamLowLateralMotionCoverage);
            foamMotionFieldLaneScale = Mathf.Clamp(
                foamMotionFieldLaneScale,
                MinimumFoamDirectionChangeFrequency,
                MaximumFoamDirectionChangeFrequency);
            foamMotionFieldAcrossRiverCoherence = Mathf.Clamp(
                foamMotionFieldAcrossRiverCoherence,
                MinimumFoamAcrossRiverCoherence,
                MaximumFoamAcrossRiverCoherence);
            foamObstacleSlowdownStrength = Mathf.Clamp(
                foamObstacleSlowdownStrength,
                MinimumFoamObstacleSlowdownStrength,
                MaximumFoamObstacleSlowdownStrength);
            foamObstacleMinimumDownstreamFactor = Mathf.Clamp(
                foamObstacleMinimumDownstreamFactor,
                MinimumFoamObstacleMinimumDownstreamFactor,
                MaximumFoamObstacleMinimumDownstreamFactor);
            foamObjectContactFullSlowdownReachMetres = Mathf.Max(
                0f,
                foamObjectContactFullSlowdownReachMetres);
            foamObjectContactSlowdownOuterReachMetres = Mathf.Max(
                foamObjectContactFullSlowdownReachMetres,
                foamObjectContactSlowdownOuterReachMetres);
            foamVisualOccupancyBuildTime = Mathf.Clamp(
                foamVisualOccupancyBuildTime,
                MinimumFoamVisualOccupancyBuildTime,
                MaximumFoamVisualOccupancyBuildTime);
            foamVisualOccupancyReleaseTime = Mathf.Clamp(
                foamVisualOccupancyReleaseTime,
                MinimumFoamVisualOccupancyReleaseTime,
                MaximumFoamVisualOccupancyReleaseTime);
            foamColour.a = Mathf.Clamp01(foamColour.a);
            foamInteriorOpacityFloor = Mathf.Clamp01(
                foamInteriorOpacityFloor);
            foamEdgeContrast = Mathf.Clamp(
                foamEdgeContrast,
                -1f,
                1f);
            foamChipActivation = Mathf.Clamp01(foamChipActivation);
            foamChipCandidateSpacing = Mathf.Clamp(
                foamChipCandidateSpacing,
                MinimumFoamChipCandidateSpacing,
                MaximumFoamChipCandidateSpacing);
            foamChipSize = Mathf.Clamp01(foamChipSize);
            foamChipIrregularity = Mathf.Clamp01(
                foamChipIrregularity);
            foamChipStableScreenRadiusPixels = Mathf.Clamp(
                foamChipStableScreenRadiusPixels,
                MinimumFoamChipStableScreenRadiusPixels,
                MaximumFoamChipStableScreenRadiusPixels);
            foamChipMaximumViewScale = Mathf.Clamp(
                foamChipMaximumViewScale,
                MinimumFoamChipMaximumViewScale,
                MaximumFoamChipMaximumViewScale);
            foamChipEdgeWidthPixels = Mathf.Max(
                0f,
                foamChipEdgeWidthPixels);
            foamChipSoftEdgeStart = Mathf.Clamp(
                foamChipSoftEdgeStart,
                MinimumFoamChipSoftEdgeStart,
                MaximumFoamChipSoftEdgeStart);
            foamChipInteriorAccess = Mathf.Clamp01(
                foamChipInteriorAccess);
            foamChipFieldSpeed = Mathf.Clamp(
                foamChipFieldSpeed,
                MinimumFoamChipFieldSpeed,
                MaximumFoamChipFieldSpeed);
            foamChipFormationTime = Mathf.Clamp(
                foamChipFormationTime,
                MinimumFoamChipLifecycleTime,
                MaximumFoamChipLifecycleTime);
            foamChipStableTime = Mathf.Clamp(
                foamChipStableTime,
                MinimumFoamChipLifecycleTime,
                MaximumFoamChipLifecycleTime);
            foamChipDissolveTime = Mathf.Clamp(
                foamChipDissolveTime,
                MinimumFoamChipLifecycleTime,
                MaximumFoamChipLifecycleTime);
            foamChipDormantTime = Mathf.Clamp(
                foamChipDormantTime,
                MinimumFoamChipLifecycleTime,
                MaximumFoamChipLifecycleTime);
            foamChipLateralMotionAmount = Mathf.Clamp(
                foamChipLateralMotionAmount,
                MinimumFoamChipLateralMotionAmount,
                MaximumFoamChipLateralMotionAmount);
            foamChipLateralMotionSpeed = Mathf.Clamp(
                foamChipLateralMotionSpeed,
                MinimumFoamChipMotionSpeed,
                MaximumFoamChipMotionSpeed);
            foamChipRotationAmountDegrees = Mathf.Clamp(
                foamChipRotationAmountDegrees,
                MinimumFoamChipRotationAmountDegrees,
                MaximumFoamChipRotationAmountDegrees);
            foamChipRotationSpeed = Mathf.Clamp(
                foamChipRotationSpeed,
                MinimumFoamChipMotionSpeed,
                MaximumFoamChipMotionSpeed);
            foamChipSizePulseAmount = Mathf.Clamp(
                foamChipSizePulseAmount,
                MinimumFoamChipSizePulseAmount,
                MaximumFoamChipSizePulseAmount);
            foamChipSizePulseSpeed = Mathf.Clamp(
                foamChipSizePulseSpeed,
                MinimumFoamChipMotionSpeed,
                MaximumFoamChipMotionSpeed);
            foamChipShapeChangeAmount = Mathf.Clamp01(
                foamChipShapeChangeAmount);
            foamChipShapeChangeSpeed = Mathf.Clamp(
                foamChipShapeChangeSpeed,
                MinimumFoamChipMotionSpeed,
                MaximumFoamChipMotionSpeed);
            foamChipShapeTransitionTime = Mathf.Clamp(
                foamChipShapeTransitionTime,
                MinimumFoamChipShapeTransitionTime,
                MaximumFoamChipShapeTransitionTime);
            foamStrandStrength = Mathf.Clamp01(foamStrandStrength);
            foamStrandScale = Mathf.Clamp01(foamStrandScale);
            foamStrandDensity = Mathf.Clamp01(foamStrandDensity);
            foamStrandReach = Mathf.Clamp01(foamStrandReach);
            foamSpawnDistanceNormalized = Mathf.Clamp01(
                foamSpawnDistanceNormalized);
            foamSpawnAcrossNormalized = Mathf.Clamp(
                foamSpawnAcrossNormalized,
                -1f,
                1f);
            foamSpawnScale = Mathf.Clamp(
                foamSpawnScale,
                MinimumFoamSpawnScale,
                MaximumFoamSpawnScale);
            foamSpawnAmount = Mathf.Clamp01(foamSpawnAmount);
            foamSpawnRemainingLife = Mathf.Clamp01(
                foamSpawnRemainingLife);
            foamSpawnRibbonDuration = Mathf.Clamp(
                foamSpawnRibbonDuration,
                MinimumFoamProgressiveRibbonDuration,
                MaximumFoamProgressiveRibbonDuration);
            foamSpawnRibbonTravelDistance = Mathf.Clamp(
                foamSpawnRibbonTravelDistance,
                MinimumFoamProgressiveRibbonTravelDistance,
                MaximumFoamProgressiveRibbonTravelDistance);
            foamSpawnRibbonAcrossDrift = Mathf.Clamp(
                foamSpawnRibbonAcrossDrift,
                MinimumFoamProgressiveRibbonAcrossDrift,
                MaximumFoamProgressiveRibbonAcrossDrift);
            foamSpawnRibbonPathWander = Mathf.Clamp(
                foamSpawnRibbonPathWander,
                MinimumFoamProgressiveRibbonPathWander,
                MaximumFoamProgressiveRibbonPathWander);

            visualSeed = Mathf.Clamp(visualSeed, 1, 9999);
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
                bool shouldRun =
                    runtimeDisturbances && isActiveAndEnabled;
                disturbanceRuntime.enabled = shouldRun;
                if (shouldRun)
                {
                    disturbanceRuntime.NotifyRiverChanged();
                }
            }
        }

        private void EnsureFoamRuntime()
        {
            if (foamRuntime == null)
            {
                foamRuntime = GetComponent<StylizedRiverFoamRuntime>();
            }

            if (foamEnabled && foamRuntime == null)
            {
                foamRuntime =
                    gameObject.AddComponent<StylizedRiverFoamRuntime>();
            }

            if (foamRuntime != null)
            {
                foamRuntime.hideFlags = HideFlags.HideInInspector;
                bool shouldRun = foamEnabled && isActiveAndEnabled;
                foamRuntime.enabled = shouldRun;
                if (shouldRun)
                {
                    foamRuntime.NotifyRiverChanged();
                }
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
                ground.ApplySurfaceProfileMaterialProperties(
                    corridorMeshRenderer,
                    GroundSurfaceRenderRole.RiverCorridor);
            }
            else
            {
                corridorObject.layer = 0;
                corridorMeshRenderer.SetPropertyBlock(null);
            }

            corridorMeshRenderer.shadowCastingMode =
                ShadowCastingMode.On;
            corridorMeshRenderer.receiveShadows = true;
        }

        public void RefreshCorridorMaterialProperties()
        {
            if (corridorObject == null)
            {
                Transform existing = transform.Find(CorridorObjectName);

                if (existing != null)
                {
                    corridorObject = existing.gameObject;
                }
            }

            if (corridorObject == null)
            {
                return;
            }

            if (corridorMeshRenderer == null)
            {
                corridorMeshRenderer =
                    corridorObject.GetComponent<MeshRenderer>();
            }

            if (corridorMeshRenderer == null)
            {
                return;
            }

            GeneratedGround ground =
                GetComponentInParent<GeneratedGround>();

            if (ground != null)
            {
                corridorObject.layer = ground.gameObject.layer;
                corridorMeshRenderer.sharedMaterial = ground.SharedMaterial;
                ground.ApplySurfaceProfileMaterialProperties(
                    corridorMeshRenderer,
                    GroundSurfaceRenderRole.RiverCorridor);
            }
            else
            {
                corridorMeshRenderer.SetPropertyBlock(null);
            }
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
#if UNITY_EDITOR
            int previousDomainVersion = riverDomainVersion;
#endif
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
#if UNITY_EDITOR
            RecordEditorDomainBuild(
                CalculateEditorDomainFingerprint(Domain),
                riverDomainVersion != previousDomainVersion);
#endif

            riverLength = Domain.LocalLength;

            if (!Domain.IsValid)
            {
                averageSurfaceHeight = transform.position.y + surfaceOffset;
                DomainChanged?.Invoke(Domain);
#if UNITY_EDITOR
                RecordEditorDomainPublication();
#endif
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
#if UNITY_EDITOR
            RecordEditorDomainPublication();
#endif
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
#if UNITY_EDITOR
            RecordEditorSurfaceBuild();
#endif
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
#if UNITY_EDITOR
            RecordEditorCorridorBuild();
#endif

            if (corridorMeshCollider != null)
            {
                corridorMeshCollider.sharedMesh = null;

                if (corridorBuildResult.IsValid)
                {
                    corridorMeshCollider.sharedMesh = corridorColliderMesh;
#if UNITY_EDITOR
                    RecordEditorCorridorColliderAssignment();
#endif
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
                    ground.ApplySurfaceProfileMaterialProperties(
                        corridorMeshRenderer,
                        GroundSurfaceRenderRole.RiverCorridor);
                }
                else
                {
                    corridorMeshRenderer.SetPropertyBlock(null);
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
            bodyProperties.SetFloat(
                ShoreWaveHeightScaleId,
                shoreWaveHeightScale);
            bodyProperties.SetFloat(
                ShoreWaveLengthScaleId,
                shoreWaveLengthScale);
            bodyProperties.SetFloat(ShoreWaveReachId, shoreWaveReach);
            bodyProperties.SetFloat(
                ShoreWaveTransitionLengthId,
                shoreWaveTransitionLength);
            bodyProperties.SetFloat(
                ShoreWaveSizeVariationId,
                shoreWaveSizeVariation);
            bodyProperties.SetFloat(
                ShoreWaveSideAsymmetryId,
                shoreWaveSideAsymmetry);
            bodyProperties.SetFloat(
                ShoreWaveProfileVariationId,
                shoreWaveProfileVariation);
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

            // The hidden Stage 6 runtime binds the shared Foam field in
            // LateUpdate. Keeping this neutral here preserves exact Stage 1–5
            // output while Foam is disabled, sleeping, unsupported, or frozen.
            bodyProperties.SetFloat(FoamEnabledStage6Id, 0f);

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

        private void NotifyFoamRuntimeChanged()
        {
            EnsureFoamRuntime();
            foamRuntime?.NotifyRiverChanged();
#if UNITY_EDITOR
            RecordEditorFoamNotification();
#endif
        }

        public void SetFoamStateHeld(bool held)
        {
            foamStateHeld = Application.isPlaying && foamEnabled && held;
        }

#if UNITY_EDITOR
        public void RequestStructuralRegenerationFromInspector()
        {
            if (!liveRegeneration)
            {
                return;
            }

            RequestRegeneration(
                RiverRegenerationRequestOrigin.InspectorStructural);
        }
#endif

        private void RequestRegeneration(
            RiverRegenerationRequestOrigin origin)
        {
#if UNITY_EDITOR
            BeginEditorRegenerationRequest(origin, pendingRegeneration);
#endif
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

            RequestRegeneration(
                RiverRegenerationRequestOrigin.SplineChanged);
        }

        private bool NotifyParentGround(
            bool allowPlayStartupCoalescing)
        {
            GeneratedGround ground = GetComponentInParent<GeneratedGround>();

            if (ground == null)
            {
#if UNITY_EDITOR
                RecordEditorGroundNotification(false, false);
#endif
                return false;
            }

            bool committedBeforeReturn =
                ground.NotifyRiverChanged(
                    this,
                    allowPlayStartupCoalescing);
#if UNITY_EDITOR
            RecordEditorGroundNotification(
                true,
                !committedBeforeReturn);
#endif
            return committedBeforeReturn;
        }

        private void NotifyReflectionSurfaceChanged()
        {
            StylizedRiverPlanarReflection reflection =
                GetComponent<StylizedRiverPlanarReflection>();

            if (reflection != null)
            {
                reflection.RequestRender();
#if UNITY_EDITOR
                RecordEditorReflectionRequest();
#endif
            }
        }

#if UNITY_EDITOR
        private void BeginEditorRegenerationRequest(
            RiverRegenerationRequestOrigin origin,
            bool coalesced)
        {
            EnsureEditorRegenerationBatch();
            activeEditorRegenerationBatch.RequestCount++;
            activeEditorRegenerationBatch.RequestOrigins[(int)origin]++;
            if (coalesced)
            {
                activeEditorRegenerationBatch.CoalescedRequestCount++;
            }
            AppendEditorRegenerationTimeline(
                "Request " + origin + (coalesced ? " (coalesced)" : ""));
            MarkEditorRegenerationActivity();
        }

        private void RecordEditorRegenerationPass(
            RiverEditorRegenerationPassKind kind,
            double elapsedMilliseconds)
        {
            EnsureEditorRegenerationBatch();
            activeEditorRegenerationBatch.PassCount++;
            activeEditorRegenerationBatch.PassKinds[(int)kind]++;
            activeEditorRegenerationBatch.TotalPassMilliseconds +=
                Math.Max(0.0, elapsedMilliseconds);
            AppendEditorRegenerationTimeline(
                "Pass " + kind + " " +
                Math.Max(0.0, elapsedMilliseconds).ToString("F3") + " ms");
            MarkEditorRegenerationActivity();
        }

        private void RecordEditorDomainBuild(
            GeneratedGeometryStableFingerprint fingerprint,
            bool versionIncremented)
        {
            EnsureEditorRegenerationBatch();
            RiverEditorRegenerationBatch batch =
                activeEditorRegenerationBatch;
            batch.DomainBuilds++;
            batch.HasDomainFingerprint = true;
            batch.DomainPreviousFingerprint =
                hasEditorDomainFingerprint
                    ? lastEditorDomainFingerprint
                    : default(GeneratedGeometryStableFingerprint);
            batch.DomainLatestFingerprint = fingerprint;
            string contentState;
            if (!hasEditorDomainFingerprint)
            {
                batch.DomainFirstObservations++;
                contentState = "first observation";
            }
            else if (!lastEditorDomainFingerprint.Equals(fingerprint))
            {
                batch.DomainContentChanges++;
                contentState = "changed";
            }
            else
            {
                batch.DomainUnchangedRebuilds++;
                contentState = "unchanged";
            }
            if (versionIncremented)
            {
                batch.DomainVersionIncrements++;
            }
            hasEditorDomainFingerprint = true;
            lastEditorDomainFingerprint = fingerprint;
            AppendEditorRegenerationTimeline(
                "Domain build " + contentState +
                (versionIncremented ? " / version incremented" : "") +
                " / " + fingerprint);
            MarkEditorRegenerationActivity();
        }

        private void RecordEditorDomainPublication()
        {
            EnsureEditorRegenerationBatch();
            activeEditorRegenerationBatch.DomainPublications++;
            AppendEditorRegenerationTimeline("Domain published");
            MarkEditorRegenerationActivity();
        }

        private void RecordEditorSurfaceBuild()
        {
            EnsureEditorRegenerationBatch();
            activeEditorRegenerationBatch.SurfaceBuilds++;
            AppendEditorRegenerationTimeline("Surface built");
            MarkEditorRegenerationActivity();
        }

        private void RecordEditorCorridorBuild()
        {
            EnsureEditorRegenerationBatch();
            activeEditorRegenerationBatch.CorridorBuilds++;
            AppendEditorRegenerationTimeline("Corridor built");
            MarkEditorRegenerationActivity();
        }

        private void RecordEditorCorridorColliderAssignment()
        {
            EnsureEditorRegenerationBatch();
            activeEditorRegenerationBatch.CorridorColliderAssignments++;
            AppendEditorRegenerationTimeline("Corridor collider assigned");
            MarkEditorRegenerationActivity();
        }

        private void RecordEditorGroundSnapshotFingerprint(
            GeneratedGeometryStableFingerprint fingerprint)
        {
            EnsureEditorRegenerationBatch();
            RiverEditorRegenerationBatch batch =
                activeEditorRegenerationBatch;
            batch.GroundSnapshotBuilds++;
            batch.HasGroundSnapshotFingerprint = true;
            batch.GroundSnapshotPreviousFingerprint =
                hasEditorGroundSnapshotFingerprint
                    ? lastEditorGroundSnapshotFingerprint
                    : default(GeneratedGeometryStableFingerprint);
            batch.GroundSnapshotLatestFingerprint = fingerprint;
            string contentState;
            if (!hasEditorGroundSnapshotFingerprint)
            {
                batch.GroundSnapshotFirstObservations++;
                contentState = "first observation";
            }
            else if (!lastEditorGroundSnapshotFingerprint.Equals(fingerprint))
            {
                batch.GroundSnapshotContentChanges++;
                contentState = "changed";
            }
            else
            {
                batch.GroundSnapshotUnchangedBuilds++;
                contentState = "unchanged";
            }
            hasEditorGroundSnapshotFingerprint = true;
            lastEditorGroundSnapshotFingerprint = fingerprint;
            AppendEditorRegenerationTimeline(
                "Ground snapshot " + contentState +
                " / " + fingerprint);
            MarkEditorRegenerationActivity();
        }

        private void RecordEditorGroundNotification(
            bool delivered,
            bool deferred)
        {
            EnsureEditorRegenerationBatch();
            if (delivered)
            {
                activeEditorRegenerationBatch.GroundNotifications++;
                if (deferred)
                {
                    activeEditorRegenerationBatch
                        .GroundDeferredNotifications++;
                }
            }
            else
            {
                activeEditorRegenerationBatch.GroundNotificationMisses++;
            }
            AppendEditorRegenerationTimeline(
                delivered
                    ? deferred
                        ? "Ground notified / startup transaction deferred"
                        : "Ground notified / committed before return"
                    : "Ground notification missed");
            MarkEditorRegenerationActivity();
        }

        private void RecordEditorFoamNotification()
        {
            EnsureEditorRegenerationBatch();
            activeEditorRegenerationBatch.FoamNotifications++;
            AppendEditorRegenerationTimeline("Foam notified");
            MarkEditorRegenerationActivity();
        }

        private void RecordEditorReflectionRequest()
        {
            EnsureEditorRegenerationBatch();
            activeEditorRegenerationBatch.ReflectionRequests++;
            AppendEditorRegenerationTimeline("Reflection requested");
            MarkEditorRegenerationActivity();
        }

        private void AppendEditorRegenerationTimeline(string entry)
        {
            const int MaximumTimelineEvents = 48;
            EnsureEditorRegenerationBatch();
            if (activeEditorRegenerationBatch.Timeline.Count <
                MaximumTimelineEvents)
            {
                activeEditorRegenerationBatch.Timeline.Add(
                    EditorApplication.timeSinceStartup.ToString("F6") +
                    "s " + entry);
            }
            else
            {
                activeEditorRegenerationBatch.DroppedTimelineEvents++;
            }
        }

        private void EnsureEditorRegenerationBatch()
        {
            if (activeEditorRegenerationBatch != null)
            {
                return;
            }

            activeEditorRegenerationBatch =
                new RiverEditorRegenerationBatch
                {
                    Id = nextEditorRegenerationBatchId++,
                    StartedAt = EditorApplication.timeSinceStartup,
                    StartFrame = Time.frameCount
                };
        }

        private void MarkEditorRegenerationActivity()
        {
            editorRegenerationActivityRevision++;
            ScheduleEditorRegenerationBatchCompletion();
        }

        private void ScheduleEditorRegenerationBatchCompletion()
        {
            if (editorRegenerationCompletionScheduled)
            {
                return;
            }

            editorRegenerationCompletionScheduled = true;
            scheduledEditorRegenerationActivityRevision =
                editorRegenerationActivityRevision;
            EditorApplication.delayCall +=
                TryCompleteEditorRegenerationBatch;
        }

        private void TryCompleteEditorRegenerationBatch()
        {
            editorRegenerationCompletionScheduled = false;
            if (activeEditorRegenerationBatch == null)
            {
                return;
            }

            if ((pendingRegeneration && isActiveAndEnabled) ||
                scheduledEditorRegenerationActivityRevision !=
                editorRegenerationActivityRevision)
            {
                ScheduleEditorRegenerationBatchCompletion();
                return;
            }

            RiverEditorRegenerationBatch completed =
                activeEditorRegenerationBatch;
            activeEditorRegenerationBatch = null;
            completed.EndFrame = Time.frameCount;
            lastEditorRegenerationAccountingReport =
                BuildEditorRegenerationAccountingReport(completed);

            if (logNextEditorRegenerationBatch)
            {
                logNextEditorRegenerationBatch = false;
                Debug.Log(
                    lastEditorRegenerationAccountingReport,
                    this);
            }
        }

        private static string BuildEditorRegenerationAccountingReport(
            RiverEditorRegenerationBatch batch)
        {
            double wallMilliseconds =
                Math.Max(
                    0.0,
                    (EditorApplication.timeSinceStartup - batch.StartedAt) *
                    1000.0);
            StringBuilder builder = new StringBuilder(1024);
            builder.Append("StylizedRiver regeneration accounting\n");
            builder.Append("Batch ").Append(batch.Id)
                .Append(" | frames ").Append(batch.StartFrame)
                .Append('–').Append(batch.EndFrame)
                .Append(" | wall ").Append(wallMilliseconds.ToString("F3"))
                .Append(" ms | measured passes ")
                .Append(batch.TotalPassMilliseconds.ToString("F3"))
                .Append(" ms\n");
            builder.Append("Requests ").Append(batch.RequestCount)
                .Append(" | coalesced ").Append(batch.CoalescedRequestCount)
                .Append(" | passes ").Append(batch.PassCount).Append('\n');
            builder.Append("Origins: ");
            AppendEditorOriginCounts(builder, batch.RequestOrigins);
            builder.Append("\nPasses: Full×")
                .Append(batch.PassKinds[(int)RiverEditorRegenerationPassKind.Full])
                .Append(" SurfaceOnly×")
                .Append(batch.PassKinds[(int)RiverEditorRegenerationPassKind.SurfaceOnly])
                .Append(" CorridorOnly×")
                .Append(batch.PassKinds[(int)RiverEditorRegenerationPassKind.CorridorOnly])
                .Append('\n');
            builder.Append("Outputs: Domain×").Append(batch.DomainBuilds)
                .Append(" Surface×").Append(batch.SurfaceBuilds)
                .Append(" Corridor×").Append(batch.CorridorBuilds)
                .Append(" ColliderAssign×")
                .Append(batch.CorridorColliderAssignments).Append('\n');
            builder.Append("Domain: first observations ")
                .Append(batch.DomainFirstObservations)
                .Append(" | content changes ")
                .Append(batch.DomainContentChanges)
                .Append(" | unchanged rebuilds ")
                .Append(batch.DomainUnchangedRebuilds)
                .Append(" | version increments ")
                .Append(batch.DomainVersionIncrements)
                .Append(" | publications ")
                .Append(batch.DomainPublications);
            AppendEditorFingerprintPair(
                builder,
                batch.HasDomainFingerprint,
                batch.DomainPreviousFingerprint,
                batch.DomainLatestFingerprint);
            builder.Append("\nGround snapshots: builds ")
                .Append(batch.GroundSnapshotBuilds)
                .Append(" | first observations ")
                .Append(batch.GroundSnapshotFirstObservations)
                .Append(" | content changes ")
                .Append(batch.GroundSnapshotContentChanges)
                .Append(" | unchanged builds ")
                .Append(batch.GroundSnapshotUnchangedBuilds);
            AppendEditorFingerprintPair(
                builder,
                batch.HasGroundSnapshotFingerprint,
                batch.GroundSnapshotPreviousFingerprint,
                batch.GroundSnapshotLatestFingerprint);
            builder.Append("\nNotifications: Ground ")
                .Append(batch.GroundNotifications)
                .Append(" (deferred ")
                .Append(batch.GroundDeferredNotifications)
                .Append(", misses ")
                .Append(batch.GroundNotificationMisses)
                .Append(") | Foam ").Append(batch.FoamNotifications)
                .Append(" | Reflection ").Append(batch.ReflectionRequests);
            AppendEditorRegenerationTimelineReport(builder, batch);
            return builder.ToString();
        }

        private static void AppendEditorRegenerationTimelineReport(
            StringBuilder builder,
            RiverEditorRegenerationBatch batch)
        {
            builder.Append("\nTimeline:");
            for (int index = 0; index < batch.Timeline.Count; index++)
            {
                builder.Append("\n  ").Append(index + 1).Append(". ")
                    .Append(batch.Timeline[index]);
            }
            if (batch.DroppedTimelineEvents > 0)
            {
                builder.Append("\n  … ")
                    .Append(batch.DroppedTimelineEvents)
                    .Append(" additional event(s) omitted");
            }
        }

        private static void AppendEditorOriginCounts(
            StringBuilder builder,
            int[] counts)
        {
            bool wroteAny = false;
            for (int index = 0;
                 index < (int)RiverRegenerationRequestOrigin.Count;
                 index++)
            {
                int count = counts[index];
                if (count <= 0)
                {
                    continue;
                }

                if (wroteAny)
                {
                    builder.Append(", ");
                }

                builder.Append((RiverRegenerationRequestOrigin)index)
                    .Append('×').Append(count);
                wroteAny = true;
            }

            if (!wroteAny)
            {
                builder.Append("none");
            }
        }

        private static void AppendEditorFingerprintPair(
            StringBuilder builder,
            bool hasFingerprint,
            GeneratedGeometryStableFingerprint previous,
            GeneratedGeometryStableFingerprint latest)
        {
            if (!hasFingerprint)
            {
                builder.Append(" | fingerprints unavailable");
                return;
            }

            builder.Append(" | fingerprints ")
                .Append(previous.Equals(default) ? "none" : previous.ToString())
                .Append(" → ").Append(latest);
        }

        private static double ResolveEditorElapsedMilliseconds(long startedAt)
        {
            long elapsed =
                System.Diagnostics.Stopwatch.GetTimestamp() - startedAt;
            return elapsed * 1000.0 /
                System.Diagnostics.Stopwatch.Frequency;
        }

        private static GeneratedGeometryStableFingerprint
            CalculateEditorDomainFingerprint(RiverDomainSnapshot snapshot)
        {
            GeneratedGeometryStableHashBuilder builder =
                GeneratedGeometryStableHashBuilder.Create(
                    "PS3D.River.EditorDomain");
            builder.AddBoolean(snapshot != null && snapshot.IsValid);
            if (snapshot == null)
            {
                return builder.Finish();
            }

            builder.AddInt32(snapshot.SampleCount);
            builder.AddSingle(snapshot.LocalLength);
            builder.AddSingle(snapshot.RequestedSampleSpacing);
            builder.AddSingle(snapshot.ConnectedDistanceOffset);
            builder.AddBoolean(snapshot.ReverseFlow);
            for (int index = 0; index < snapshot.SampleCount; index++)
            {
                StylizedRiverSplineSample sample = snapshot.Samples[index];
                builder.AddVector3(sample.Centre);
                builder.AddVector3(sample.SurfacePoint);
                builder.AddVector3(sample.Tangent);
                builder.AddVector3(sample.Side);
                builder.AddVector3(sample.Up);
                builder.AddSingle(sample.Distance);
                builder.AddSingle(sample.OrientedDistance);
                builder.AddSingle(sample.GlobalDistance);
                builder.AddSingle(sample.LeftHalfWidth);
                builder.AddSingle(sample.RightHalfWidth);
                builder.AddSingle(sample.LeftSurfaceHalfWidth);
                builder.AddSingle(sample.RightSurfaceHalfWidth);
                builder.AddSingle(sample.NormalizedDistance);
                builder.AddSingle(sample.NormalizedTime);
            }
            return builder.Finish();
        }

        private static GeneratedGeometryStableFingerprint
            CalculateEditorGroundSnapshotFingerprint(
                Vector3[] points,
                Vector3[] sides,
                float[] leftVisibleHalfWidths,
                float[] rightVisibleHalfWidths,
                float[] leftSurfaceHalfWidths,
                float[] rightSurfaceHalfWidths,
                float snapshotBankBlend,
                float snapshotDepth,
                float snapshotBedFlatness,
                StylizedRiverBankProfile snapshotBankProfile,
                float snapshotTerrainConformity,
                float snapshotGroundGridSpacing,
                float snapshotWetClearance,
                float snapshotBankCover,
                float snapshotReservedDownwardDisplacement)
        {
            GeneratedGeometryStableHashBuilder builder =
                GeneratedGeometryStableHashBuilder.Create(
                    "PS3D.River.EditorGroundSnapshot");
            AddEditorFingerprintValues(ref builder, points);
            AddEditorFingerprintValues(ref builder, sides);
            AddEditorFingerprintValues(ref builder, leftVisibleHalfWidths);
            AddEditorFingerprintValues(ref builder, rightVisibleHalfWidths);
            AddEditorFingerprintValues(ref builder, leftSurfaceHalfWidths);
            AddEditorFingerprintValues(ref builder, rightSurfaceHalfWidths);
            builder.AddSingle(snapshotBankBlend);
            builder.AddSingle(snapshotDepth);
            builder.AddSingle(snapshotBedFlatness);
            builder.AddInt32((int)snapshotBankProfile);
            builder.AddSingle(snapshotTerrainConformity);
            builder.AddSingle(snapshotGroundGridSpacing);
            builder.AddSingle(snapshotWetClearance);
            builder.AddSingle(snapshotBankCover);
            builder.AddSingle(snapshotReservedDownwardDisplacement);
            return builder.Finish();
        }

        private static void AddEditorFingerprintValues(
            ref GeneratedGeometryStableHashBuilder builder,
            Vector3[] values)
        {
            builder.AddInt32(values != null ? values.Length : -1);
            if (values == null)
            {
                return;
            }
            for (int index = 0; index < values.Length; index++)
            {
                builder.AddVector3(values[index]);
            }
        }

        private static void AddEditorFingerprintValues(
            ref GeneratedGeometryStableHashBuilder builder,
            float[] values)
        {
            builder.AddInt32(values != null ? values.Length : -1);
            if (values == null)
            {
                return;
            }
            for (int index = 0; index < values.Length; index++)
            {
                builder.AddSingle(values[index]);
            }
        }
#endif

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

        private float ResolveImpactRippleStrength()
        {
            float strength = Mathf.Clamp(impactRippleStrength, 0f, 4f);
            if (strength <= 3f)
            {
                return strength * (3f - 0.4f * strength);
            }

            return 5.4f + (strength - 3f) * 3.6f;
        }

        private float ResolveImpactRippleDecay()
        {
            return Mathf.Max(
                0.01f,
                impactRippleDecay +
                Mathf.Abs(FlowSpeedMetresPerSecond) *
                impactRippleFlowDissipation);
        }

        private float ResolveImpactRippleMaximumHeight()
        {
            float waveAllowance = motionWaveHeight * 0.20f;
            float strengthAllowance =
                ResolveImpactRippleStrength() * 0.035f;
            float overload01 = Mathf.InverseLerp(
                3f,
                4f,
                Mathf.Clamp(impactRippleStrength, 3f, 4f));
            float maximumHeight = Mathf.Lerp(0.28f, 0.45f, overload01);
            return Mathf.Clamp(
                0.06f + waveAllowance + strengthAllowance,
                0.06f,
                maximumHeight);
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
