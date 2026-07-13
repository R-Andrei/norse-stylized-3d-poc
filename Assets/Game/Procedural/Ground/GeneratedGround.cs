using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Unity.Profiling;
#if UNITY_EDITOR
using UnityEditor;
#endif
using ProgrammaticStylized3D.Geometry;
using ProgrammaticStylized3D.Rivers;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    public enum GroundPatchSize
    {
        [InspectorName("Compact")]
        Compact20,

        [InspectorName("Standard")]
        Standard40,

        [InspectorName("Large")]
        Large60,

        [InspectorName("Huge")]
        Huge80
    }


    public enum GroundResolution
    {
        [InspectorName("Low")]
        Low17,

        [InspectorName("Medium")]
        Medium33,

        [InspectorName("High")]
        High65,

        [InspectorName("Very High")]
        VeryHigh129
    }

    public enum GroundProfile
    {
        Flat,
        Rolling,
        Basin,
        Ridge,
        Uneven
    }

    public enum GroundTransitionDirection
    {
        None,
        North,
        South,
        East,
        West,
        NorthEast,
        NorthWest,
        SouthEast,
        SouthWest
    }

    public enum GroundEdgeBlend
    {
        Narrow,
        Medium,
        Wide
    }

    public enum GeneratedGroundDebugView
    {
        None = 0,

        [InspectorName("Ground Tonal")]
        GroundTonal = 7,

        [InspectorName("Ground Exposure")]
        GroundExposure = 8,

        [InspectorName("Ground Damp Deposit")]
        GroundDampDeposit = 9,

        [InspectorName("Ground Vegetation")]
        GroundVegetation = 10,

        [InspectorName("Ground Compaction Path")]
        GroundCompaction = 11,

        [InspectorName("Ground Shore")]
        GroundShore = 12,

        [InspectorName("Ground Rocky Dry")]
        GroundRockyDry = 13,

        [InspectorName("Ground Combined")]
        GroundCombined = 14,

        [InspectorName("Ground Standing Water Potential")]
        GroundStandingWaterPotential = 27,

        [InspectorName("Ground Painted Accent Lines")]
        GroundPaintedAccentLines = 28,
    }

    public enum PaintedAccentGlyphFamilyPreview
    {
        [InspectorName("All Families")]
        All = 0,

        [InspectorName("Complete Mound")]
        CompleteMound = 1,

        [InspectorName("Asymmetric Mound")]
        AsymmetricMound = 2,

        [InspectorName("Single Shoulder")]
        SingleShoulder = 3,

        [InspectorName("Shallow Crest")]
        ShallowCrest = 4
    }

    public enum PaintedAccentPlacementOverlayWeightMode
    {
        [InspectorName("Patch Preference")]
        PatchPreference = 0,

        [InspectorName("Effective Proposal Weight")]
        EffectiveProposalWeight = 1
    }

    public enum GroundSurfaceType
    {
        [InspectorName("Snowfield")]
        Snowfield = 0
    }

    public enum GroundSnowfieldVariant
    {
        Custom = 0,

        [InspectorName("Clean")]
        Clean = 1,

        [InspectorName("Patchy")]
        Patchy = 2,

        [InspectorName("Dirty Thawing")]
        DirtyThawing = 3,

        [InspectorName("Wind-Scoured")]
        WindScoured = 4
    }

    [Serializable]
    public sealed class GroundRecipe
    {
        public const int MinimumSeed = 1;
        public const int MaximumSeed = 9999;

        [Tooltip("Controls the deterministic broad form and surface detail.")]
        [Range(MinimumSeed, MaximumSeed)]
        [SerializeField]
        private int shapeSeed = 1234;

        [SerializeField]
        private GroundPatchSize patchSize = GroundPatchSize.Standard40;

        [SerializeField]
        private GroundResolution resolution = GroundResolution.Medium33;

        [Tooltip("Used to vary stable noise between neighbouring generated patches.")]
        [SerializeField]
        private Vector2Int patchCoordinate = Vector2Int.zero;

        [SerializeField]
        private GroundTransitionDirection transitionDirection =
            GroundTransitionDirection.None;

        [Tooltip("Height difference, in metres, from the low side to the high side.")]
        [Range(-12f, 12f)]
        [SerializeField]
        private float transitionHeight = 0f;

        [SerializeField]
        private GroundProfile profile = GroundProfile.Rolling;

        [Tooltip("Height, in metres, contributed by the selected broad profile.")]
        [Range(0f, 6f)]
        [SerializeField]
        private float broadForm = 1.4f;

        [Tooltip("Controls how busy the broad and detail noise becomes.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float roughness = 0.35f;

        [Tooltip("Small-scale height variation. Kept deliberately restrained.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float surfaceDetail = 0.25f;

        [Tooltip("How far terrain variation fades before reaching patch borders.")]
        [SerializeField]
        private GroundEdgeBlend edgeBlend = GroundEdgeBlend.Medium;

        [Tooltip("Amount of deterministic broad variation written to vertex colour red.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float surfaceVariation = 0.35f;

        [SerializeField]
        private bool useModifiers = true;

        public int ShapeSeed => shapeSeed;
        public GroundPatchSize PatchSize => patchSize;
        public GroundResolution Resolution => resolution;
        public Vector2Int PatchCoordinate => patchCoordinate;
        public GroundTransitionDirection TransitionDirection => transitionDirection;
        public float TransitionHeight => transitionHeight;
        public GroundProfile Profile => profile;
        public float BroadForm => broadForm;
        public float Roughness => roughness;
        public float SurfaceDetail => surfaceDetail;
        public GroundEdgeBlend EdgeBlend => edgeBlend;
        public float SurfaceVariation => surfaceVariation;
        public bool UseModifiers => useModifiers;

        public void SetShapeSeed(int value)
        {
            shapeSeed = Mathf.Clamp(value, MinimumSeed, MaximumSeed);
        }
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public sealed class GeneratedGround : MonoBehaviour
    {
        [Flags]
        private enum GroundRegenerationStage
        {
            None = 0,
            Snapshots = 1 << 0,
            Geometry = 1 << 1,
            Mesh = 1 << 2,
            Collider = 1 << 3,
            SurfaceStrokes = 1 << 4,
            ProjectedGlyphs = 1 << 5,
            Coverage = 1 << 6,
            Material = 1 << 7,
            RiverCorridor = 1 << 8
        }
        public const string SnowfieldCleanVariantId = "snowfield.clean";
        public const string SnowfieldPatchyVariantId = "snowfield.patchy";
        public const string SnowfieldDirtyThawingVariantId =
            "snowfield.dirty_thawing";
        public const string SnowfieldWindScouredVariantId =
            "snowfield.wind_scoured";

        private const int CurrentSurfaceStyleMigrationVersion = 1;

        [SerializeField]
        private GroundRecipe recipe = new GroundRecipe();

        [Tooltip("Visual surface family that owns variant recipes. Example: Snowfield.")]
        [SerializeField]
        private GroundSurfaceStyleProfile surfaceStyleProfile;

        [Tooltip("Stable variant id inside the selected Surface Style Profile.")]
        [SerializeField]
        private string surfaceVariantId = SnowfieldCleanVariantId;

        [Tooltip("Use a local semantic/mask profile instead of the style profile's default.")]
        [SerializeField]
        private bool overrideSurfaceProfile;

        [Tooltip("Optional local semantic/mask profile. When Override Surface Profile is disabled, the style profile default is used instead.")]
        [SerializeField]
        private GroundSurfaceProfile surfaceProfile;

        [Tooltip("Use local material controls instead of the selected style variant recipe.")]
        [SerializeField]
        private bool overrideMaterialControls;

        [SerializeField]
        private GroundMaterialControls groundMaterialControls =
            new GroundMaterialControls();

        [SerializeField, HideInInspector]
        private GroundSurfaceType groundSurfaceType =
            GroundSurfaceType.Snowfield;

        [FormerlySerializedAs("groundVisualPreset")]
        [SerializeField, HideInInspector]
        private GroundSnowfieldVariant snowfieldVariant =
            GroundSnowfieldVariant.Clean;

        [SerializeField, HideInInspector]
        private int surfaceStyleMigrationVersion;

        [Tooltip("Regenerate when recipe values change in the Inspector.")]
        [SerializeField]
        private bool regenerateOnValidate = true;

        [Tooltip("Renderer-local ground debug view. This is written through the GeneratedGround material property block so validation does not require editing shared material assets.")]
        [SerializeField]
        private GeneratedGroundDebugView debugView =
            GeneratedGroundDebugView.None;

        [SerializeField, HideInInspector]
        private bool showPaintedAccentDistributionOverlay;

        [SerializeField, HideInInspector]
        private bool showPaintedAccentWeightedProposals;

        [SerializeField, HideInInspector]
        private bool showPaintedAccentLastAcceptedPositions;

        [SerializeField, HideInInspector]
        private bool showPaintedAccentCompositionDebug;

        [SerializeField, HideInInspector]
        private bool showPaintedAccentProjectedGlyphDebug;

        [SerializeField, HideInInspector]
        private PaintedAccentGlyphFamilyPreview paintedAccentGlyphFamilyPreview =
            PaintedAccentGlyphFamilyPreview.All;

        [SerializeField, HideInInspector]
        private PaintedAccentPlacementOverlayWeightMode
            paintedAccentPlacementOverlayWeight =
                PaintedAccentPlacementOverlayWeightMode.PatchPreference;

        [SerializeField, HideInInspector]
        private GroundModifier[] modifiers = Array.Empty<GroundModifier>();

        [SerializeField, HideInInspector]
        private StylizedRiver[] rivers = Array.Empty<StylizedRiver>();

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;
        private Mesh generatedMesh;
        private GroundHeightFieldSnapshot baseSurface =
            GroundHeightFieldSnapshot.Empty;
        [SerializeField, HideInInspector]
        private string lastSurfaceMaskDiagnostics =
            "Surface masks have not been generated yet.";
        [SerializeField, HideInInspector]
        private int lastValidatedGenerationSignature;
        private int appliedGroundGeometrySignature;
        private bool groundGeometryInitialized;
        private int groundGeometryRevision;
        private int runtimeRiverNotificationRevision;
        private int currentSnapshotSignature;
        private int currentPaintedAccentDomainSignature;
        private GroundRegenerationStage lastExecutedRegenerationStages;
        private MaterialPropertyBlock materialProperties;
        private int paintedAccentSurfaceStrokeSignature;
        private bool paintedAccentSurfaceStrokesInitialized;
        private GroundPaintedAccentSurfaceStroke[] paintedAccentSurfaceStrokes =
            Array.Empty<GroundPaintedAccentSurfaceStroke>();
        private int paintedAccentSurfaceStrokeRevision;
        private GroundPaintedAccentPlacementDiagnostics
            paintedAccentPlacementDiagnostics =
                GroundPaintedAccentPlacementDiagnostics.Empty;
        private GroundPaintedAccentCompositionDebugSnapshot
            paintedAccentCompositionDebugSnapshot =
                GroundPaintedAccentCompositionDebugSnapshot.Empty;
        private GroundModifierSnapshot[] paintedAccentModifierSnapshots =
            Array.Empty<GroundModifierSnapshot>();
        private StylizedRiverGroundSnapshot[] paintedAccentRiverSnapshots =
            Array.Empty<StylizedRiverGroundSnapshot>();
        private GroundPaintedAccentRiverExclusionSnapshot[]
            paintedAccentRiverExclusionSnapshots =
                Array.Empty<GroundPaintedAccentRiverExclusionSnapshot>();
        private int paintedAccentProjectedGlyphSignature;
        private GroundPaintedAccentProjectedGlyphDebugSnapshot
            paintedAccentProjectedGlyphDebugSnapshot =
                GroundPaintedAccentProjectedGlyphDebugSnapshot.Empty;
        private GroundPaintedAccentProjectedGlyphBuildTimings
            paintedAccentProjectedGlyphBuildTimings =
                GroundPaintedAccentProjectedGlyphBuildTimings.Empty;
        private Texture2D paintedAccentCoverageTexture;
        private byte[] paintedAccentCoveragePixels = Array.Empty<byte>();
        private bool paintedAccentCoverageEnabled;
        private Vector4 paintedAccentCoverageOriginSize =
            new Vector4(0f, 0f, 1f, 1f);
        private int paintedAccentCoverageSignature;
        private GroundPaintedAccentCoverageDiagnostics
            paintedAccentCoverageDiagnostics =
                GroundPaintedAccentCoverageDiagnostics.Empty;
        private string lastRegenerationTimingDiagnostics =
            "Ground regeneration has not been measured yet.";
        private double lastSnapshotsMilliseconds;
        private double lastGeometryMilliseconds;
        private double lastMeshApplyMilliseconds;
        private double lastColliderMilliseconds;
        private double lastSurfaceStrokeMilliseconds;
        private double lastProjectedGlyphMilliseconds;
        private double lastCoverageRasterMilliseconds;
        private double lastCoverageUploadMilliseconds;
        private double lastMaterialMilliseconds;
        private double lastRiverCorridorMilliseconds;
        private double lastTotalRegenerationMilliseconds;

        private static readonly ProfilerMarker RegenerateProfilerMarker =
            new ProfilerMarker("GeneratedGround.Regenerate.Total");
        private static readonly ProfilerMarker SnapshotsProfilerMarker =
            new ProfilerMarker("GeneratedGround.Snapshots");
        private static readonly ProfilerMarker GeometryProfilerMarker =
            new ProfilerMarker("GeneratedGround.Geometry.Generate");
        private static readonly ProfilerMarker MeshApplyProfilerMarker =
            new ProfilerMarker("GeneratedGround.Mesh.Apply");
        private static readonly ProfilerMarker ColliderProfilerMarker =
            new ProfilerMarker("GeneratedGround.Collider.Cook");
        private static readonly ProfilerMarker SurfaceStrokeProfilerMarker =
            new ProfilerMarker("GeneratedGround.PaintedAccent.SurfaceStrokes");
        private static readonly ProfilerMarker ProjectedGlyphProfilerMarker =
            new ProfilerMarker("GeneratedGround.PaintedAccent.ProjectedGlyphs");
        private static readonly ProfilerMarker CoverageProfilerMarker =
            new ProfilerMarker("GeneratedGround.PaintedAccent.Coverage");
        private static readonly ProfilerMarker MaterialProfilerMarker =
            new ProfilerMarker("GeneratedGround.Material.Apply");
        private static readonly ProfilerMarker RiverCorridorProfilerMarker =
            new ProfilerMarker("GeneratedGround.River.CorridorNotification");

        private static readonly int BaseColorId =
            Shader.PropertyToID("_BaseColor");
        private static readonly int SurfaceContractId =
            Shader.PropertyToID("_SurfaceContract");
        private static readonly int ProfileContrastId =
            Shader.PropertyToID("_ProfileContrast");
        private static readonly int ProfilePixelContrastId =
            Shader.PropertyToID("_ProfilePixelContrast");
        private static readonly int GroundSnowResponseId =
            Shader.PropertyToID("_GroundSnowResponse");
        private static readonly int GroundDampResponseId =
            Shader.PropertyToID("_GroundDampResponse");
        private static readonly int GroundVegetationResponseId =
            Shader.PropertyToID("_GroundVegetationResponse");
        private static readonly int GroundRockyDryResponseId =
            Shader.PropertyToID("_GroundRockyDryResponse");
        private static readonly int GroundShoreDampStrengthId =
            Shader.PropertyToID("_GroundShoreDampStrength");
        private static readonly int PixelCellSizeId =
            Shader.PropertyToID("_PixelCellSize");
        private static readonly int PixelToneCountId =
            Shader.PropertyToID("_PixelToneCount");
        private static readonly int PixelClusterStrengthId =
            Shader.PropertyToID("_PixelClusterStrength");
        private static readonly int PixelVariationId =
            Shader.PropertyToID("_PixelVariation");
        private static readonly int PixelBroadVariationId =
            Shader.PropertyToID("_PixelBroadVariation");
        private static readonly int PixelVertexVariationId =
            Shader.PropertyToID("_PixelVertexVariation");
        private static readonly int PixelEffectStrengthId =
            Shader.PropertyToID("_PixelEffectStrength");
        private static readonly int PixelWarpStrengthId =
            Shader.PropertyToID("_PixelWarpStrength");
        private static readonly int GroundPatchBlendStrengthId =
            Shader.PropertyToID("_GroundPatchBlendStrength");
        private static readonly int GroundMacroPatchScaleId =
            Shader.PropertyToID("_GroundMacroPatchScale");
        private static readonly int GroundSnowTintStrengthId =
            Shader.PropertyToID("_GroundSnowTintStrength");
        private static readonly int GroundSnowBrightnessId =
            Shader.PropertyToID("_GroundSnowBrightness");
        private static readonly int GroundDampDarkenStrengthId =
            Shader.PropertyToID("_GroundDampDarkenStrength");
        private static readonly int GroundDampTintId =
            Shader.PropertyToID("_GroundDampTint");
        private static readonly int GroundDampTintStrengthId =
            Shader.PropertyToID("_GroundDampTintStrength");
        private static readonly int GroundRockyDryTintId =
            Shader.PropertyToID("_GroundRockyDryTint");
        private static readonly int GroundRockyDryTintStrengthId =
            Shader.PropertyToID("_GroundRockyDryTintStrength");
        private static readonly int GroundVegetationTintId =
            Shader.PropertyToID("_GroundVegetationTint");
        private static readonly int GroundVegetationTintStrengthId =
            Shader.PropertyToID("_GroundVegetationTintStrength");
        private static readonly int WetnessId =
            Shader.PropertyToID("_Wetness");
        private static readonly int WetDarkenStrengthId =
            Shader.PropertyToID("_WetDarkenStrength");
        private static readonly int WetPixelSofteningId =
            Shader.PropertyToID("_WetPixelSoftening");
        private static readonly int WetSmoothnessBoostId =
            Shader.PropertyToID("_WetSmoothnessBoost");
        private static readonly int FrostStrengthId =
            Shader.PropertyToID("_FrostStrength");
        private static readonly int FrostContrastId =
            Shader.PropertyToID("_FrostContrast");
        private static readonly int FrostColorId =
            Shader.PropertyToID("_FrostColor");
        private static readonly int MonolithicFlattenId =
            Shader.PropertyToID("_MonolithicFlatten");
        private static readonly int MonolithicSmoothnessBoostId =
            Shader.PropertyToID("_MonolithicSmoothnessBoost");
        private static readonly int SmoothnessId =
            Shader.PropertyToID("_Smoothness");
        private static readonly int SpecularStrengthId =
            Shader.PropertyToID("_SpecularStrength");
        private static readonly int MaskDebugModeId =
            Shader.PropertyToID("_MaskDebugMode");
        private static readonly int GroundPaintedAccentCoverageId =
            Shader.PropertyToID("_GroundPaintedAccentCoverage");
        private static readonly int GroundPaintedAccentCoverageEnabledId =
            Shader.PropertyToID("_GroundPaintedAccentCoverageEnabled");
        private static readonly int GroundPaintedAccentCoverageOriginSizeId =
            Shader.PropertyToID("_GroundPaintedAccentCoverageOriginSize");
        private static readonly int GroundPaintedAccentCoverageWorldToLocalId =
            Shader.PropertyToID("_GroundPaintedAccentCoverageWorldToLocal");
        private static readonly int GroundPaintedAccentInkColorId =
            Shader.PropertyToID("_GroundPaintedAccentInkColor");
        private static readonly int GroundFeatureModeId =
            Shader.PropertyToID("_GroundFeatureMode");
        private static readonly int GroundFeatureStrengthId =
            Shader.PropertyToID("_GroundFeatureStrength");
        private static readonly int GroundFeatureScaleId =
            Shader.PropertyToID("_GroundFeatureScale");
        private static readonly int GroundFeatureContrastId =
            Shader.PropertyToID("_GroundFeatureContrast");
        private static readonly int GroundFeatureMaskInfluenceId =
            Shader.PropertyToID("_GroundFeatureMaskInfluence");
        private static readonly int GroundFeatureDirectionId =
            Shader.PropertyToID("_GroundFeatureDirection");
        private static readonly int GroundFeatureSeedId =
            Shader.PropertyToID("_GroundFeatureSeed");

        private static readonly GroundShaderFeaturePropertyIds
            GroundDirectionalStreakFeatureIds =
                new GroundShaderFeaturePropertyIds("_GroundDirectionalStreak");
        private static readonly GroundShaderFeaturePropertyIds
            GroundPooledWetnessFeatureIds =
                new GroundShaderFeaturePropertyIds("_GroundPooledWetness");
        private static readonly GroundShaderFeaturePropertyIds
            GroundTrampledWearFeatureIds =
                new GroundShaderFeaturePropertyIds("_GroundTrampledWear");
        private static readonly int GroundPaintedAccentLineStrengthId =
            Shader.PropertyToID("_GroundPaintedAccentLineStrength");

        private struct GroundShaderFeaturePropertyIds
        {
            public readonly int StrengthId;
            public readonly int ScaleId;
            public readonly int ContrastId;
            public readonly int MaskInfluenceId;
            public readonly int DirectionId;
            public readonly int SeedId;

            public GroundShaderFeaturePropertyIds(string prefix)
            {
                StrengthId = Shader.PropertyToID(prefix + "Strength");
                ScaleId = Shader.PropertyToID(prefix + "Scale");
                ContrastId = Shader.PropertyToID(prefix + "Contrast");
                MaskInfluenceId = Shader.PropertyToID(prefix + "MaskInfluence");
                DirectionId = Shader.PropertyToID(prefix + "Direction");
                SeedId = Shader.PropertyToID(prefix + "Seed");
            }
        }

        public GroundRecipe Recipe => recipe;
        public GroundSurfaceStyleProfile SurfaceStyleProfile =>
            surfaceStyleProfile;
        public string SurfaceVariantId => surfaceVariantId;
        public bool OverrideSurfaceProfile => overrideSurfaceProfile;
        public bool OverrideMaterialControls => overrideMaterialControls;
        public GeneratedGroundDebugView DebugView => debugView;
        public GroundSurfaceProfile SurfaceProfile => ResolveSurfaceProfile();
        public GroundSurfaceType SurfaceType => groundSurfaceType;
        public GroundSnowfieldVariant SnowfieldVariant => snowfieldVariant;
        public int ModifierCount => modifiers != null ? modifiers.Length : 0;
        public int RiverCount => rivers != null ? rivers.Length : 0;
        public Material SharedMaterial =>
            meshRenderer != null
                ? meshRenderer.sharedMaterial
                : null;
        public string LastSurfaceMaskDiagnostics =>
            string.IsNullOrWhiteSpace(lastSurfaceMaskDiagnostics)
                ? "Surface masks have not been generated yet."
                : lastSurfaceMaskDiagnostics;

        public string LastRegenerationTimingDiagnostics =>
            string.IsNullOrWhiteSpace(lastRegenerationTimingDiagnostics)
                ? "Ground regeneration has not been measured yet."
                : lastRegenerationTimingDiagnostics;
        public string ResolvedSurfaceFeatureSummary =>
            ResolveSurfaceVariant() != null
                ? ResolveSurfaceVariant().BuildFeatureSummary()
                : "Features: no resolved variant";
        public float PatchSize =>
            recipe != null
                ? GroundGenerator.ResolvePatchSize(recipe.PatchSize)
                : 40f;

        public float GridSpacing
        {
            get
            {
                if (recipe == null)
                {
                    return 0.5f;
                }

                float patchSize =
                    GroundGenerator.ResolvePatchSize(recipe.PatchSize);

                int resolution =
                    GroundGenerator.ResolveResolution(recipe.Resolution);

                return patchSize / Mathf.Max(1, resolution - 1);
            }
        }

        public float GridCellDiagonal => GridSpacing * 1.41421356237f;

        private void OnEnable()
        {
            CacheComponents();
            NormalizeSurfaceStyleSelection();
            RefreshModifiers();
            Regenerate();
        }

        private void OnValidate()
        {
            CacheComponents();
            NormalizeSurfaceStyleSelection();

            int generationSignature = CalculateGenerationSignature();

            if (!regenerateOnValidate)
            {
                RefreshSurfaceMaterialProperties();
                return;
            }

            if (lastValidatedGenerationSignature != generationSignature)
            {
                RefreshModifiers();
                Regenerate();
                return;
            }

            RefreshSurfaceMaterialProperties();
        }

        [ContextMenu("Regenerate Ground")]
        public void Regenerate()
        {
            ResetRegenerationTiming();
            long totalStartedAt = System.Diagnostics.Stopwatch.GetTimestamp();
            GroundRegenerationStage executedStages =
                GroundRegenerationStage.None;

            using (RegenerateProfilerMarker.Auto())
            {
                CacheComponents();

                if (recipe == null)
                {
                    ClearGeneratedAssignments();
                    lastTotalRegenerationMilliseconds =
                        ResolveElapsedMilliseconds(totalStartedAt);
                    lastExecutedRegenerationStages = executedStages;
                    UpdateRegenerationTimingDiagnostics();
                    return;
                }

                EnsureGeneratedMesh();

                List<GroundModifierSnapshot> allModifierSnapshots;
                IReadOnlyList<GroundModifierSnapshot> heightModifierSnapshots;
                List<StylizedRiverGroundSnapshot> riverSnapshots;
                long stageStartedAt = System.Diagnostics.Stopwatch.GetTimestamp();
                using (SnapshotsProfilerMarker.Auto())
                {
                    allModifierSnapshots = BuildModifierSnapshots();
                    heightModifierSnapshots = allModifierSnapshots;
                    if (!recipe.UseModifiers)
                    {
                        heightModifierSnapshots =
                            Array.Empty<GroundModifierSnapshot>();
                    }

                    riverSnapshots = BuildRiverSnapshots(
                        out paintedAccentRiverExclusionSnapshots);
                    paintedAccentModifierSnapshots =
                        allModifierSnapshots.ToArray();
                    paintedAccentRiverSnapshots =
                        riverSnapshots.ToArray();
                    currentSnapshotSignature =
                        CalculateGroundSnapshotSignature(
                            allModifierSnapshots,
                            riverSnapshots,
                            false);
                    currentPaintedAccentDomainSignature =
                        CalculateGroundSnapshotSignature(
                            allModifierSnapshots,
                            riverSnapshots,
                            true);
                }
                lastSnapshotsMilliseconds =
                    ResolveElapsedMilliseconds(stageStartedAt);
                executedStages |= GroundRegenerationStage.Snapshots;

                int geometrySignature =
                    CalculateGroundGeometrySignature(currentSnapshotSignature);
                bool geometryOutputMissing =
                    generatedMesh == null ||
                    meshFilter == null ||
                    meshFilter.sharedMesh != generatedMesh ||
                    baseSurface == null ||
                    !baseSurface.IsValid;
                bool geometryChanged =
                    !groundGeometryInitialized ||
                    geometryOutputMissing ||
                    appliedGroundGeometrySignature != geometrySignature;

                if (geometryChanged)
                {
                    GroundSurfaceProfile resolvedSurfaceProfile =
                        ResolveSurfaceProfile();
                    MeshData meshData;
                    stageStartedAt =
                        System.Diagnostics.Stopwatch.GetTimestamp();
                    using (GeometryProfilerMarker.Auto())
                    {
                        meshData = GroundGenerator.Generate(
                            recipe,
                            resolvedSurfaceProfile,
                            heightModifierSnapshots,
                            riverSnapshots,
                            out baseSurface,
                            out lastSurfaceMaskDiagnostics);
                    }
                    lastGeometryMilliseconds =
                        ResolveElapsedMilliseconds(stageStartedAt);
                    executedStages |= GroundRegenerationStage.Geometry;

                    string meshName =
                        $"GeneratedGround_{recipe.PatchSize}_{recipe.Resolution}_Seed{recipe.ShapeSeed}";
                    stageStartedAt =
                        System.Diagnostics.Stopwatch.GetTimestamp();
                    using (MeshApplyProfilerMarker.Auto())
                    {
                        MeshBuilder.ApplyToMesh(
                            meshData,
                            generatedMesh,
                            meshName);
                        meshFilter.sharedMesh = generatedMesh;
                    }
                    lastMeshApplyMilliseconds =
                        ResolveElapsedMilliseconds(stageStartedAt);
                    executedStages |= GroundRegenerationStage.Mesh;

                    stageStartedAt =
                        System.Diagnostics.Stopwatch.GetTimestamp();
                    using (ColliderProfilerMarker.Auto())
                    {
                        meshCollider.sharedMesh = null;
                        meshCollider.sharedMesh = generatedMesh;
                        meshCollider.convex = false;
                    }
                    lastColliderMilliseconds =
                        ResolveElapsedMilliseconds(stageStartedAt);
                    executedStages |= GroundRegenerationStage.Collider;

                    appliedGroundGeometrySignature = geometrySignature;
                    groundGeometryInitialized = true;
                    groundGeometryRevision++;
                }

                lastValidatedGenerationSignature =
                    CalculateGenerationSignature();

                int previousSurfaceRevision =
                    paintedAccentSurfaceStrokeRevision;
                EnsurePaintedAccentSurfaceStrokesCurrent();
                if (paintedAccentSurfaceStrokeRevision !=
                    previousSurfaceRevision)
                {
                    executedStages |= GroundRegenerationStage.SurfaceStrokes;
                }

                int previousProjectedSignature =
                    paintedAccentProjectedGlyphSignature;
                int previousCoverageSignature =
                    paintedAccentCoverageSignature;
                EnsurePaintedAccentCoverageCurrent();
                if (paintedAccentProjectedGlyphSignature !=
                    previousProjectedSignature)
                {
                    executedStages |= GroundRegenerationStage.ProjectedGlyphs;
                }
                if (paintedAccentCoverageSignature !=
                    previousCoverageSignature)
                {
                    executedStages |= GroundRegenerationStage.Coverage;
                }

                stageStartedAt = System.Diagnostics.Stopwatch.GetTimestamp();
                using (MaterialProfilerMarker.Auto())
                {
                    ApplySurfaceProfileMaterialProperties();
                }
                lastMaterialMilliseconds =
                    ResolveElapsedMilliseconds(stageStartedAt);
                executedStages |= GroundRegenerationStage.Material;

                if (geometryChanged)
                {
                    stageStartedAt =
                        System.Diagnostics.Stopwatch.GetTimestamp();
                    using (RiverCorridorProfilerMarker.Auto())
                    {
                        NotifyRiverCorridorsChanged();
                    }
                    lastRiverCorridorMilliseconds =
                        ResolveElapsedMilliseconds(stageStartedAt);
                    executedStages |= GroundRegenerationStage.RiverCorridor;
                }
            }

            lastTotalRegenerationMilliseconds =
                ResolveElapsedMilliseconds(totalStartedAt);
            lastExecutedRegenerationStages = executedStages;
            UpdateRegenerationTimingDiagnostics();
        }

        [ContextMenu("New Ground Shape")]
        public void CreateNewShape()
        {
            recipe.SetShapeSeed(
                GenerateDifferentSeed(recipe.ShapeSeed));

            Regenerate();
        }

        [ContextMenu("Find Ground Modifiers")]
        public void RefreshModifiers()
        {
            modifiers = GetComponentsInChildren<GroundModifier>(true);

            Array.Sort(
                modifiers,
                (left, right) =>
                    left.PriorityValue.CompareTo(right.PriorityValue));

            rivers = GetComponentsInChildren<StylizedRiver>(true);
        }

        public void NotifyModifierChanged(GroundModifier modifier)
        {
            if (modifier == null ||
                !modifier.transform.IsChildOf(transform))
            {
                return;
            }

            RefreshModifiers();
            Regenerate();
        }

        public void NotifyRiverChanged(StylizedRiver river)
        {
            if (river == null ||
                (river.transform != transform &&
                 !river.transform.IsChildOf(transform)))
            {
                return;
            }

            RefreshModifiers();
#if !UNITY_EDITOR
            runtimeRiverNotificationRevision++;
#endif
            Regenerate();
        }

        public void ApplySnowfieldVariant(GroundSnowfieldVariant variant)
        {
            NormalizeSurfaceStyleSelection();

            groundSurfaceType = GroundSurfaceType.Snowfield;
            snowfieldVariant = variant;
            surfaceVariantId = MapSnowfieldVariantToId(variant);
            overrideMaterialControls = variant == GroundSnowfieldVariant.Custom;

            if (overrideMaterialControls)
            {
                groundMaterialControls ??= new GroundMaterialControls();
                groundMaterialControls.ApplySnowfieldVariant(variant);
            }

            RefreshSurfaceMaterialProperties();
        }

        public void SetSurfaceStyleProfile(
            GroundSurfaceStyleProfile profile)
        {
            surfaceStyleProfile = profile;
            overrideMaterialControls = false;
            NormalizeSurfaceStyleSelection();
            RefreshSurfaceStyleState();
        }

        public void SetSurfaceVariant(string variantId)
        {
            if (string.IsNullOrWhiteSpace(variantId))
            {
                return;
            }

            surfaceVariantId = variantId;

            if (variantId.StartsWith(
                    "snowfield.",
                    StringComparison.Ordinal))
            {
                snowfieldVariant = MapSnowfieldVariantIdToLegacy(
                    variantId);
                groundSurfaceType = GroundSurfaceType.Snowfield;
            }

            overrideMaterialControls = false;
            NormalizeSurfaceStyleSelection();
            RefreshSurfaceMaterialProperties();
        }

        public void RefreshSurfaceStyleState()
        {
            NormalizeSurfaceStyleSelection();

            if (regenerateOnValidate &&
                lastValidatedGenerationSignature !=
                CalculateGenerationSignature())
            {
                RefreshModifiers();
                Regenerate();
                return;
            }

            RefreshSurfaceMaterialProperties();
        }

        public void EnableMaterialControlOverrideFromResolved()
        {
            groundMaterialControls ??= new GroundMaterialControls();
            groundMaterialControls.CopyFrom(ResolveMaterialControls());
            overrideMaterialControls = true;
            snowfieldVariant = GroundSnowfieldVariant.Custom;
            RefreshSurfaceMaterialProperties();
        }

        public void DisableMaterialControlOverride()
        {
            overrideMaterialControls = false;
            NormalizeSurfaceStyleSelection();
            RefreshSurfaceMaterialProperties();
        }

        public void MarkGroundVisualControlsCustom()
        {
            overrideMaterialControls = true;
            snowfieldVariant = GroundSnowfieldVariant.Custom;
            RefreshSurfaceMaterialProperties();
        }

        public void SetDebugView(
            GeneratedGroundDebugView value)
        {
            debugView = value;
            RefreshSurfaceMaterialProperties();
        }

        public void ClearDebugView()
        {
            SetDebugView(GeneratedGroundDebugView.None);
        }

        public bool ShowPaintedAccentDistributionOverlay =>
            showPaintedAccentDistributionOverlay;

        public bool ShowPaintedAccentWeightedProposals =>
            showPaintedAccentWeightedProposals;

        public bool ShowPaintedAccentLastAcceptedPositions =>
            showPaintedAccentLastAcceptedPositions;

        public bool ShowPaintedAccentCompositionDebug =>
            showPaintedAccentCompositionDebug;

        public bool ShowPaintedAccentProjectedGlyphDebug =>
            showPaintedAccentProjectedGlyphDebug;

        public PaintedAccentGlyphFamilyPreview PaintedAccentGlyphFamilyFilter =>
            paintedAccentGlyphFamilyPreview;

        public PaintedAccentPlacementOverlayWeightMode
            PaintedAccentPlacementOverlayWeight =>
                paintedAccentPlacementOverlayWeight;

        public int CalculatePaintedAccentPlacementDebugSignature()
        {
            GroundSurfaceFeatureRecipe feature =
                ResolveShaderFeature(
                    GroundSurfaceFeatureKind.PaintedAccentLines);
            return CalculatePaintedAccentSurfaceStrokeSignature(feature);
        }

        public bool TryBuildPaintedAccentPlacementDebugSnapshot(
            out GroundPaintedAccentPlacementDebugSnapshot snapshot)
        {
            snapshot = GroundPaintedAccentPlacementDebugSnapshot.Empty;
            CacheComponents();

            GroundSurfaceFeatureRecipe feature =
                ResolveShaderFeature(
                    GroundSurfaceFeatureKind.PaintedAccentLines);

            if (!CanGeneratePaintedAccentSurfaceStrokes(feature))
            {
                return false;
            }

            snapshot =
                GroundPaintedAccentSurfaceStrokeGenerator
                    .BuildPlacementDebugSnapshot(
                        generatedMesh.bounds,
                        baseSurface,
                        feature,
                        recipe != null ? recipe.ShapeSeed : 0,
                        recipe != null
                            ? recipe.PatchCoordinate
                            : Vector2Int.zero);
            return snapshot.IsValid;
        }

        public GroundPaintedAccentCompositionDebugSnapshot
            GetLastPaintedAccentCompositionDebugSnapshot()
        {
            return paintedAccentCompositionDebugSnapshot;
        }

        public Vector3[] GetLastPaintedAccentAcceptedLocalPositions()
        {
            if (paintedAccentSurfaceStrokes == null ||
                paintedAccentSurfaceStrokes.Length == 0)
            {
                return Array.Empty<Vector3>();
            }

            List<Vector3> positions =
                new List<Vector3>(paintedAccentSurfaceStrokes.Length);

            for (int index = 0;
                 index < paintedAccentSurfaceStrokes.Length;
                 index++)
            {
                GroundPaintedAccentSurfaceStroke stroke =
                    paintedAccentSurfaceStrokes[index];

                if (!stroke.IsValid)
                {
                    continue;
                }

                int middleIndex = stroke.LocalPoints.Length / 2;
                Vector3 position = stroke.LocalPoints[middleIndex];
                position.y += 0.035f;
                positions.Add(position);
            }

            return positions.ToArray();
        }

        public string GetLastPaintedAccentPlacementStatistics()
        {
            if (!paintedAccentSurfaceStrokesInitialized)
            {
                return "No Painted Accent placement has been generated yet.";
            }

            GroundPaintedAccentPlacementDiagnostics diagnostics =
                paintedAccentPlacementDiagnostics;
            GroundSurfaceFeatureRecipe feature =
                ResolveShaderFeature(
                    GroundSurfaceFeatureKind.PaintedAccentLines);
            float densityContrast =
                feature != null
                    ? feature.PaintedAccentCompositionDensityContrast
                    : 0f;
            float pathWiggle =
                feature != null
                    ? feature.PaintedAccentStrokePathWiggle
                    : 0f;
            float horizontalCompanionStrength =
                feature != null
                    ? feature.PaintedAccentHorizontalCompanionStrength
                    : 0f;
            float companionTightness =
                feature != null
                    ? feature.PaintedAccentCompanionTightness
                    : 0f;
            float companionTripletVerticality =
                feature != null
                    ? feature.PaintedAccentCompanionTripletVerticality
                    : 1f;
            Vector4 familyWeights =
                feature != null
                    ? feature.PaintedAccentGlyphFamilyWeights
                    : Vector4.zero;

            return
                $"Target proposals / candidate pool: " +
                $"{diagnostics.TargetProposals} / {diagnostics.CandidatePool}\n" +
                $"Proposed / final accepted: " +
                $"{diagnostics.Proposed} / {diagnostics.Accepted}\n" +
                $"Rejected regional thinning: " +
                $"{diagnostics.RejectedRegionalThinning}\n" +
                $"Rejected physical validation: " +
                $"{diagnostics.RejectedPhysicalValidation}\n\n" +
                $"Proposal-bearing regions: " +
                $"{diagnostics.CompositionRegionCount}\n" +
                $"Quiet / supporting / accent regions: " +
                $"{diagnostics.QuietRegionCount} / " +
                $"{diagnostics.SupportingRegionCount} / " +
                $"{diagnostics.AccentRegionCount}\n" +
                $"Region scale: {diagnostics.CompositionRegionScale:F3} m\n" +
                $"Regional density contrast: {densityContrast:F2}\n" +
                $"Stroke path wiggle: {pathWiggle:F2}\n" +
                $"Horizontal companion strength / tightness / triplet verticality: " +
                $"{horizontalCompanionStrength:F2} / " +
                $"{companionTightness:F2} / " +
                $"{companionTripletVerticality:F2}\n" +
                $"Companion clusters composed (pairs / triplets): " +
                $"{diagnostics.HorizontalCompanionClusterCount} (" +
                $"{diagnostics.HorizontalCompanionPairClusterCount} / " +
                $"{diagnostics.HorizontalCompanionTripletClusterCount})\n" +
                $"Companion participants composed / accepted: " +
                $"{diagnostics.HorizontalCompanionParticipantCount} / " +
                $"{diagnostics.HorizontalCompanionAcceptedParticipantCount}\n" +
                $"Companion full clusters accepted: " +
                $"{diagnostics.HorizontalCompanionAcceptedClusterCount}\n" +
                $"Proposals quiet / supporting / accent: " +
                $"{diagnostics.QuietProposalCount} / " +
                $"{diagnostics.SupportingProposalCount} / " +
                $"{diagnostics.AccentProposalCount}\n" +
                $"Accepted quiet / supporting / accent: " +
                $"{diagnostics.QuietAcceptedCount} / " +
                $"{diagnostics.SupportingAcceptedCount} / " +
                $"{diagnostics.AccentAcceptedCount}\n" +
                $"Accepted dominant / standard / support: " +
                $"{diagnostics.DominantAcceptedCount} / " +
                $"{diagnostics.StandardAcceptedCount} / " +
                $"{diagnostics.SupportAcceptedCount}\n\n" +
                $"Family weights complete / asymmetric / shoulder / shallow: " +
                $"{familyWeights.x:F2} / {familyWeights.y:F2} / " +
                $"{familyWeights.z:F2} / {familyWeights.w:F2}\n" +
                $"Family selected complete / asymmetric / shoulder / shallow: " +
                $"{diagnostics.CompleteMoundSelectedCount} / " +
                $"{diagnostics.AsymmetricMoundSelectedCount} / " +
                $"{diagnostics.SingleShoulderSelectedCount} / " +
                $"{diagnostics.ShallowCrestSelectedCount}\n" +
                $"Surface descriptors accepted by family: " +
                $"{diagnostics.CompleteMoundAcceptedCount} / " +
                $"{diagnostics.AsymmetricMoundAcceptedCount} / " +
                $"{diagnostics.SingleShoulderAcceptedCount} / " +
                $"{diagnostics.ShallowCrestAcceptedCount}\n\n" +
                $"Accepted length min/mean/max: " +
                $"{diagnostics.AcceptedLengthMin:F3} / " +
                $"{diagnostics.AcceptedLengthMean:F3} / " +
                $"{diagnostics.AcceptedLengthMax:F3} m\n" +
                $"Accepted angle offset min/mean/max: " +
                $"{diagnostics.AcceptedAngleOffsetMin:F1} / " +
                $"{diagnostics.AcceptedAngleOffsetMean:F1} / " +
                $"{diagnostics.AcceptedAngleOffsetMax:F1} deg\n" +
                $"Accepted companion angle min/mean/max: " +
                $"{diagnostics.AcceptedCompanionAngleOffsetMin:F1} / " +
                $"{diagnostics.AcceptedCompanionAngleOffsetMean:F1} / " +
                $"{diagnostics.AcceptedCompanionAngleOffsetMax:F1} deg\n\n" +
                $"Rejected sampling / river / modifier: " +
                $"{diagnostics.RejectedSampling} / " +
                $"{diagnostics.RejectedRiver} / " +
                $"{diagnostics.RejectedModifierExclusion}\n" +
                $"Rejected broad slope / local grade: " +
                $"{diagnostics.RejectedBroadSlope} / " +
                $"{diagnostics.RejectedLocalGrade}";
        }

        public int CalculatePaintedAccentProjectedGlyphDebugSignature()
        {
            GroundSurfaceFeatureRecipe feature =
                ResolveShaderFeature(
                    GroundSurfaceFeatureKind.PaintedAccentLines);
            return CalculatePaintedAccentProjectedGlyphSignature(feature);
        }

        public bool TryBuildPaintedAccentProjectedGlyphDebugSnapshot(
            out GroundPaintedAccentProjectedGlyphDebugSnapshot snapshot)
        {
            snapshot = GroundPaintedAccentProjectedGlyphDebugSnapshot.Empty;
            CacheComponents();

            GroundSurfaceFeatureRecipe feature =
                ResolveShaderFeature(
                    GroundSurfaceFeatureKind.PaintedAccentLines);

            if (!CanGeneratePaintedAccentSurfaceStrokes(feature))
            {
                return false;
            }

            EnsurePaintedAccentSurfaceStrokesCurrent();
            EnsurePaintedAccentProjectedGlyphsCurrent(feature);
            snapshot = paintedAccentProjectedGlyphDebugSnapshot;
            return snapshot.IsValid;
        }

        public string GetLastPaintedAccentProjectedGlyphStatistics()
        {
            CacheComponents();
            GroundSurfaceFeatureRecipe feature =
                ResolveShaderFeature(
                    GroundSurfaceFeatureKind.PaintedAccentLines);

            if (!CanGeneratePaintedAccentSurfaceStrokes(feature))
            {
                return "No Painted Accent projected glyphs can be generated from the current ground and feature settings.";
            }

            EnsurePaintedAccentSurfaceStrokesCurrent();
            EnsurePaintedAccentProjectedGlyphsCurrent(feature);

            GroundPaintedAccentProjectedGlyphDiagnostics diagnostics =
                paintedAccentProjectedGlyphDebugSnapshot.Diagnostics;

            return
                $"Base descriptors: {diagnostics.AcceptedBaseDescriptors}\n" +
                $"Projected accepted: {diagnostics.ProjectedGlyphsAccepted}\n" +
                $"Projected rejected: {diagnostics.ProjectedGlyphsRejectedTotal}\n\n" +
                $"Rejected sampling: {diagnostics.ProjectedGlyphsRejectedSampling}\n" +
                $"Rejected river: {diagnostics.ProjectedGlyphsRejectedRiver}\n" +
                $"Rejected modifier: {diagnostics.ProjectedGlyphsRejectedModifier}\n" +
                $"Rejected broad slope: {diagnostics.ProjectedGlyphsRejectedBroadSlope}\n" +
                $"Rejected local grade: {diagnostics.ProjectedGlyphsRejectedLocalGrade}\n" +
                $"Rejected family shape: {diagnostics.ProjectedGlyphsRejectedFamilyShape}\n" +
                $"Rejected sharp projected turn: {diagnostics.ProjectedGlyphsRejectedSharpTurn}\n\n" +
                $"Projected companion clusters requested / accepted / fallback: " +
                $"{diagnostics.CompanionClustersRequested} / " +
                $"{diagnostics.CompanionClustersAccepted} / " +
                $"{diagnostics.CompanionClustersFallback}\n" +
                $"Projected complete pairs / triplets: " +
                $"{diagnostics.CompanionPairsAccepted} / " +
                $"{diagnostics.CompanionTripletsAccepted}\n" +
                $"Companion fallback incomplete / prototype / contact / surface / external: " +
                $"{diagnostics.CompanionRejectedIncomplete} / " +
                $"{diagnostics.CompanionRejectedPrototype} / " +
                $"{diagnostics.CompanionRejectedContact} / " +
                $"{diagnostics.CompanionRejectedSurface} / " +
                $"{diagnostics.CompanionRejectedExternalConflict}\n\n" +
                FormatPaintedAccentFamilyStatistics(
                    GroundPaintedAccentGlyphFamily.CompleteMound,
                    "Complete",
                    diagnostics.CompleteMoundStatistics) +
                FormatPaintedAccentFamilyStatistics(
                    GroundPaintedAccentGlyphFamily.AsymmetricMound,
                    "Asymmetric",
                    diagnostics.AsymmetricMoundStatistics) +
                FormatPaintedAccentFamilyStatistics(
                    GroundPaintedAccentGlyphFamily.SingleShoulder,
                    "Shoulder",
                    diagnostics.SingleShoulderStatistics) +
                FormatPaintedAccentFamilyStatistics(
                    GroundPaintedAccentGlyphFamily.ShallowCrest,
                    "Shallow",
                    diagnostics.ShallowCrestStatistics) +
                "\n" +
                $"Projection world direction: +Z\n" +
                $"Projection local X/Z: " +
                $"({diagnostics.ProjectionLocalDirection.x:F4}, " +
                $"{diagnostics.ProjectionLocalDirection.y:F4})\n" +
                $"Point count min/mean/max: " +
                $"{diagnostics.ProjectedPointCountMin} / " +
                $"{diagnostics.ProjectedPointCountMean:F1} / " +
                $"{diagnostics.ProjectedPointCountMax}\n" +
                $"Crest T min/mean/max: " +
                $"{diagnostics.ProjectedCrestTMin:F3} / " +
                $"{diagnostics.ProjectedCrestTMean:F3} / " +
                $"{diagnostics.ProjectedCrestTMax:F3}\n" +
                $"Crest peak min/mean/max: " +
                $"{diagnostics.CrestPeakHeightMin:F4} / " +
                $"{diagnostics.CrestPeakHeightMean:F4} / " +
                $"{diagnostics.CrestPeakHeightMax:F4} m\n" +
                $"Crown peak min/mean/max: " +
                $"{diagnostics.CrownPeakHeightMin:F4} / " +
                $"{diagnostics.CrownPeakHeightMean:F4} / " +
                $"{diagnostics.CrownPeakHeightMax:F4} m\n" +
                $"Combined peak min/mean/max: " +
                $"{diagnostics.CombinedPeakHeightMin:F4} / " +
                $"{diagnostics.CombinedPeakHeightMean:F4} / " +
                $"{diagnostics.CombinedPeakHeightMax:F4} m\n" +
                $"Projected turn min/mean/max: " +
                $"{diagnostics.ProjectedTurnDegreesMin:F2} / " +
                $"{diagnostics.ProjectedTurnDegreesMean:F2} / " +
                $"{diagnostics.ProjectedTurnDegreesMax:F2} deg\n" +
                $"Maximum north displacement error: " +
                $"{diagnostics.MaximumNorthDisplacementError:F7} m\n" +
                $"Maximum cross-axis drift: " +
                $"{diagnostics.MaximumCrossAxisDrift:F7} m";
        }

        private static string FormatPaintedAccentFamilyStatistics(
            GroundPaintedAccentGlyphFamily family,
            string label,
            GroundPaintedAccentGlyphFamilyStatistics statistics)
        {
            string common =
                $"{label} attempted / accepted / rejected: " +
                $"{statistics.Attempted} / {statistics.Accepted} / " +
                $"{statistics.Rejected}\n" +
                $"{label} length min/mean/max: " +
                $"{statistics.AuthoredLengthMin:F3} / " +
                $"{statistics.AuthoredLengthMean:F3} / " +
                $"{statistics.AuthoredLengthMax:F3} m\n" +
                $"{label} peak min/mean/max: " +
                $"{statistics.PeakHeightMin:F4} / " +
                $"{statistics.PeakHeightMean:F4} / " +
                $"{statistics.PeakHeightMax:F4} m\n";

            GroundPaintedAccentGlyphFamilyShapeStatistics shape =
                statistics.ShapeStatistics;
            switch (family)
            {
                case GroundPaintedAccentGlyphFamily.AsymmetricMound:
                    return
                        common +
                        FormatPaintedAccentMetricRange(
                            "Asymmetric crest position",
                            shape.CrestPosition) +
                        FormatPaintedAccentMetricRange(
                            "Asymmetric leg-span ratio",
                            shape.LegSpanRatio) +
                        FormatPaintedAccentMetricRange(
                            "Asymmetric leg-slope ratio",
                            shape.LegSlopeRatio);
                case GroundPaintedAccentGlyphFamily.SingleShoulder:
                    return
                        common +
                        FormatPaintedAccentMetricRange(
                            "Shoulder upper-run fraction",
                            shape.UpperRunFraction) +
                        FormatPaintedAccentMetricRange(
                            "Shoulder upper-end drop fraction",
                            shape.UpperEndpointDropFraction) +
                        FormatPaintedAccentMetricRange(
                            "Shoulder descending-drop fraction",
                            shape.DescendingDropFraction);
                case GroundPaintedAccentGlyphFamily.ShallowCrest:
                    return
                        common +
                        FormatPaintedAccentMetricRange(
                            "Shallow plateau fraction",
                            shape.PlateauFraction) +
                        FormatPaintedAccentMetricRange(
                            "Shallow vertical-range fraction",
                            shape.VerticalRangeFraction) +
                        FormatPaintedAccentMetricRange(
                            "Shallow endpoint-difference fraction",
                            shape.EndpointDifferenceFraction) +
                        $"Shallow endpoint-difference world min/mean/max: " +
                        $"{shape.EndpointDifferenceWorld.Minimum:F4} / " +
                        $"{shape.EndpointDifferenceWorld.Mean:F4} / " +
                        $"{shape.EndpointDifferenceWorld.Maximum:F4} m\n" +
                        $"Shallow near-straight accepted: " +
                        $"{shape.NearStraightCount}\n";
                case GroundPaintedAccentGlyphFamily.CompleteMound:
                default:
                    return common;
            }
        }

        private static string FormatPaintedAccentMetricRange(
            string label,
            GroundPaintedAccentMetricRange range)
        {
            return
                $"{label} min/mean/max: " +
                $"{range.Minimum:F3} / {range.Mean:F3} / " +
                $"{range.Maximum:F3}\n";
        }

        public string GetLastPaintedAccentCoverageStatistics()
        {
            GroundPaintedAccentCoverageDiagnostics diagnostics =
                paintedAccentCoverageDiagnostics;

            if (!diagnostics.IsValid)
            {
                return "Projected coverage has not been generated yet.";
            }

            return
                $"Texture: {diagnostics.TextureWidth} x " +
                $"{diagnostics.TextureHeight} R8\n" +
                $"Projected glyphs baked: {diagnostics.GlyphCount}\n" +
                $"Polyline segments baked: {diagnostics.SegmentCount}\n" +
                $"Covered texels: {diagnostics.CoveredTexelCount} " +
                $"({diagnostics.CoveredTexelFraction * 100f:F3}%)\n" +
                $"World texel X/Z: " +
                $"{diagnostics.TexelWorldSizeX:F5} / " +
                $"{diagnostics.TexelWorldSizeZ:F5} m\n" +
                $"Minimum authored full width: " +
                $"{diagnostics.MinimumAuthoredFullWidth:F5} m\n" +
                $"Minimum effective raster core width: " +
                $"{diagnostics.MinimumEffectiveCoreFullWidth:F5} m\n" +
                $"Minimum edge feather width: " +
                $"{diagnostics.MinimumEdgeFeatherWidth:F5} m per side\n" +
                $"Minimum estimated visible full width: " +
                $"{diagnostics.MinimumEstimatedVisibleFullWidth:F5} m";
        }

        public void RefreshSurfaceMaterialProperties()
        {
            ResetRegenerationTiming();
            long totalStartedAt = System.Diagnostics.Stopwatch.GetTimestamp();
            GroundRegenerationStage executedStages =
                GroundRegenerationStage.None;

            CacheComponents();
            int previousSurfaceRevision =
                paintedAccentSurfaceStrokeRevision;
            EnsurePaintedAccentSurfaceStrokesCurrent();
            if (paintedAccentSurfaceStrokeRevision != previousSurfaceRevision)
            {
                executedStages |= GroundRegenerationStage.SurfaceStrokes;
            }

            int previousProjectedSignature =
                paintedAccentProjectedGlyphSignature;
            int previousCoverageSignature =
                paintedAccentCoverageSignature;
            EnsurePaintedAccentCoverageCurrent();
            if (paintedAccentProjectedGlyphSignature !=
                previousProjectedSignature)
            {
                executedStages |= GroundRegenerationStage.ProjectedGlyphs;
            }
            if (paintedAccentCoverageSignature !=
                previousCoverageSignature)
            {
                executedStages |= GroundRegenerationStage.Coverage;
            }

            long materialStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
            using (MaterialProfilerMarker.Auto())
            {
                ApplySurfaceProfileMaterialProperties();
                RefreshRiverCorridorMaterialProperties();
            }
            lastMaterialMilliseconds =
                ResolveElapsedMilliseconds(materialStartedAt);
            executedStages |= GroundRegenerationStage.Material;

            lastTotalRegenerationMilliseconds =
                ResolveElapsedMilliseconds(totalStartedAt);
            lastExecutedRegenerationStages = executedStages;
            UpdateRegenerationTimingDiagnostics();
        }

        public bool TrySampleBaseSurface(
            Vector3 worldPosition,
            out float height,
            out Vector3 normal)
        {
            bool succeeded =
                TrySampleBaseSurface(
                    worldPosition,
                    out GroundSurfaceSample sample);

            height = sample.Height;
            normal = sample.Normal;
            return succeeded;
        }

        public bool TrySampleBaseSurface(
            Vector3 worldPosition,
            out GroundSurfaceSample sample)
        {
            sample =
                new GroundSurfaceSample(
                    0f,
                    Vector3.up,
                    0.5f,
                    0f);

            if (baseSurface == null || !baseSurface.IsValid)
            {
                return false;
            }

            Vector3 localPoint =
                transform.InverseTransformPoint(worldPosition);

            if (!baseSurface.TrySample(
                    new Vector2(localPoint.x, localPoint.z),
                    out GroundSurfaceSample localSample))
            {
                return false;
            }

            Vector3 worldPoint =
                transform.TransformPoint(
                    new Vector3(
                        localPoint.x,
                        localSample.Height,
                        localPoint.z));

            sample =
                new GroundSurfaceSample(
                    worldPoint.y,
                    transform
                        .TransformDirection(localSample.Normal)
                        .normalized,
                    transform
                        .TransformDirection(localSample.RenderNormal)
                        .normalized,
                    localSample.SurfaceVariation,
                    localSample.Exposure,
                    localSample.DampDeposit,
                    localSample.VegetationSuitability,
                    new Vector4(
                        localSample.Compaction,
                        localSample.ShoreInfluence,
                        localSample.RockyDry,
                        localSample.ReservedSurfaceMask),
                    localSample.MaterialClassification);

            return true;
        }

        public bool TrySampleSurface(
            Vector3 worldPosition,
            out float height,
            out Vector3 normal)
        {
            CacheComponents();

            if (meshCollider == null ||
                meshCollider.sharedMesh == null)
            {
                height = 0f;
                normal = Vector3.up;
                return false;
            }

            Bounds bounds = meshCollider.bounds;
            float rayStartHeight =
                Mathf.Max(worldPosition.y, bounds.max.y + 5f);

            Ray ray = new Ray(
                new Vector3(
                    worldPosition.x,
                    rayStartHeight,
                    worldPosition.z),
                Vector3.down);

            float maximumDistance =
                Mathf.Max(10f, rayStartHeight - bounds.min.y + 10f);

            if (meshCollider.Raycast(
                    ray,
                    out RaycastHit hit,
                    maximumDistance))
            {
                height = hit.point.y;
                normal = hit.normal;
                return true;
            }

            height = 0f;
            normal = Vector3.up;
            return false;
        }

        private List<GroundModifierSnapshot> BuildModifierSnapshots()
        {
            List<GroundModifierSnapshot> snapshots =
                new List<GroundModifierSnapshot>();

            if (modifiers == null)
            {
                return snapshots;
            }

            for (int i = 0; i < modifiers.Length; i++)
            {
                GroundModifier modifier = modifiers[i];

                if (modifier == null ||
                    !modifier.isActiveAndEnabled)
                {
                    continue;
                }

                snapshots.Add(
                    modifier.CreateSnapshot(transform));
            }

            snapshots.Sort(
                (left, right) =>
                    left.Priority.CompareTo(right.Priority));

            return snapshots;
        }

        private List<StylizedRiverGroundSnapshot> BuildRiverSnapshots(
            out GroundPaintedAccentRiverExclusionSnapshot[] exclusionSnapshots)
        {
            List<StylizedRiverGroundSnapshot> snapshots =
                new List<StylizedRiverGroundSnapshot>();
            List<GroundPaintedAccentRiverExclusionSnapshot> exclusions =
                new List<GroundPaintedAccentRiverExclusionSnapshot>();
            List<StylizedRiverSplineSample> splineSamples =
                new List<StylizedRiverSplineSample>();

            if (rivers == null)
            {
                exclusionSnapshots =
                    Array.Empty<GroundPaintedAccentRiverExclusionSnapshot>();
                return snapshots;
            }

            for (int index = 0; index < rivers.Length; index++)
            {
                StylizedRiver river = rivers[index];

                if (river == null ||
                    !river.isActiveAndEnabled)
                {
                    continue;
                }

                StylizedRiverGroundSnapshot snapshot =
                    river.CreateGroundSnapshot(transform);

                if (!snapshot.IsValid)
                {
                    continue;
                }

                snapshots.Add(snapshot);
                exclusions.Add(
                    BuildPaintedAccentRiverExclusionSnapshot(
                        river,
                        snapshot,
                        splineSamples));
            }

            exclusionSnapshots = exclusions.ToArray();
            return snapshots;
        }

        private GroundPaintedAccentRiverExclusionSnapshot
            BuildPaintedAccentRiverExclusionSnapshot(
                StylizedRiver river,
                StylizedRiverGroundSnapshot snapshot,
                List<StylizedRiverSplineSample> splineSamples)
        {
            if (river == null || splineSamples == null)
            {
                return new GroundPaintedAccentRiverExclusionSnapshot(
                    snapshot,
                    Vector2.zero,
                    Vector2.zero,
                    false);
            }

            river.BuildSharedSplineSamples(splineSamples);
            if (splineSamples.Count < 2)
            {
                return new GroundPaintedAccentRiverExclusionSnapshot(
                    snapshot,
                    Vector2.zero,
                    Vector2.zero,
                    false);
            }

            Vector2 minimum =
                new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 maximum =
                new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            for (int sampleIndex = 0;
                 sampleIndex < splineSamples.Count;
                 sampleIndex++)
            {
                Vector3 localPoint =
                    transform.InverseTransformPoint(
                        splineSamples[sampleIndex].SurfacePoint);
                Vector2 localXZ = new Vector2(localPoint.x, localPoint.z);
                minimum = Vector2.Min(minimum, localXZ);
                maximum = Vector2.Max(maximum, localXZ);
            }

            Vector2 expansion =
                Vector2.one * snapshot.MaximumInfluenceDistance;
            return new GroundPaintedAccentRiverExclusionSnapshot(
                snapshot,
                minimum - expansion,
                maximum + expansion,
                true);
        }

        private void NotifyRiverCorridorsChanged()
        {
            if (rivers == null)
            {
                return;
            }

            for (int index = 0; index < rivers.Length; index++)
            {
                StylizedRiver river = rivers[index];

                if (river == null ||
                    !river.isActiveAndEnabled)
                {
                    continue;
                }

                river.RebuildCorridorFromGround();
            }
        }

        private void RefreshRiverCorridorMaterialProperties()
        {
            if (rivers == null)
            {
                return;
            }

            for (int index = 0; index < rivers.Length; index++)
            {
                StylizedRiver river = rivers[index];

                if (river == null ||
                    !river.isActiveAndEnabled)
                {
                    continue;
                }

                river.RefreshCorridorMaterialProperties();
            }
        }

        private void NormalizeSurfaceStyleSelection()
        {
#if UNITY_EDITOR
            if (surfaceStyleProfile == null)
            {
                surfaceStyleProfile = LoadDefaultSnowfieldStyleProfile();
            }
#endif

            if (surfaceStyleMigrationVersion <
                CurrentSurfaceStyleMigrationVersion)
            {
                surfaceVariantId = MapSnowfieldVariantToId(
                    snowfieldVariant);

                if (snowfieldVariant == GroundSnowfieldVariant.Custom)
                {
                    overrideMaterialControls = true;
                }

                surfaceStyleMigrationVersion =
                    CurrentSurfaceStyleMigrationVersion;
            }

            if (string.IsNullOrWhiteSpace(surfaceVariantId))
            {
                surfaceVariantId = MapSnowfieldVariantToId(
                    snowfieldVariant);
            }

            if (surfaceStyleProfile != null &&
                !surfaceStyleProfile.TryGetVariant(
                    surfaceVariantId,
                    out _) &&
                surfaceStyleProfile.TryGetFirstVariant(
                    out GroundSurfaceVariantRecipe firstVariant))
            {
                surfaceVariantId = firstVariant.Id;
            }
        }

        private GroundSurfaceProfile ResolveSurfaceProfile()
        {
            if (overrideSurfaceProfile)
            {
                return surfaceProfile;
            }

            if (surfaceStyleProfile != null &&
                surfaceStyleProfile.DefaultSurfaceProfile != null)
            {
                return surfaceStyleProfile.DefaultSurfaceProfile;
            }

            return surfaceProfile;
        }

        private GroundSurfaceVariantRecipe ResolveSurfaceVariant()
        {
            if (surfaceStyleProfile != null &&
                surfaceStyleProfile.TryGetVariant(
                    surfaceVariantId,
                    out GroundSurfaceVariantRecipe variant))
            {
                return variant;
            }

            return null;
        }

        private GroundMaterialControls ResolveMaterialControls()
        {
            if (overrideMaterialControls)
            {
                groundMaterialControls ??= new GroundMaterialControls();
                return groundMaterialControls;
            }

            GroundSurfaceVariantRecipe variant = ResolveSurfaceVariant();

            if (variant != null && variant.MaterialControls != null)
            {
                return variant.MaterialControls;
            }

            groundMaterialControls ??= new GroundMaterialControls();
            return groundMaterialControls;
        }

        private GroundSurfaceFeatureRecipe ResolveShaderFeature(
            GroundSurfaceFeatureKind kind)
        {
            GroundSurfaceVariantRecipe variant = ResolveSurfaceVariant();

            if (variant != null &&
                variant.TryGetFirstShaderFeature(
                    kind,
                    out GroundSurfaceFeatureRecipe feature))
            {
                return feature;
            }

            return null;
        }

        public static string MapSnowfieldVariantToId(
            GroundSnowfieldVariant variant)
        {
            switch (variant)
            {
                case GroundSnowfieldVariant.Patchy:
                    return SnowfieldPatchyVariantId;

                case GroundSnowfieldVariant.DirtyThawing:
                    return SnowfieldDirtyThawingVariantId;

                case GroundSnowfieldVariant.WindScoured:
                    return SnowfieldWindScouredVariantId;

                case GroundSnowfieldVariant.Clean:
                case GroundSnowfieldVariant.Custom:
                default:
                    return SnowfieldCleanVariantId;
            }
        }

        private static GroundSnowfieldVariant MapSnowfieldVariantIdToLegacy(
            string variantId)
        {
            switch (variantId)
            {
                case SnowfieldPatchyVariantId:
                    return GroundSnowfieldVariant.Patchy;

                case SnowfieldDirtyThawingVariantId:
                    return GroundSnowfieldVariant.DirtyThawing;

                case SnowfieldWindScouredVariantId:
                    return GroundSnowfieldVariant.WindScoured;

                case SnowfieldCleanVariantId:
                default:
                    return GroundSnowfieldVariant.Clean;
            }
        }

#if UNITY_EDITOR
        private static GroundSurfaceStyleProfile LoadDefaultSnowfieldStyleProfile()
        {
            string[] guids = AssetDatabase.FindAssets(
                "GSSP_Snowfield t:GroundSurfaceStyleProfile");

            if (guids == null || guids.Length == 0)
            {
                return null;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<GroundSurfaceStyleProfile>(
                assetPath);
        }
#endif

        private int CalculateGenerationSignature()
        {
            if (recipe == null)
            {
                return 0;
            }

            GroundSurfaceProfile resolvedSurfaceProfile =
                ResolveSurfaceProfile();

            unchecked
            {
                int hash = 17;
                hash = hash * 31 + recipe.ShapeSeed;
                hash = hash * 31 + (int)recipe.PatchSize;
                hash = hash * 31 + (int)recipe.Resolution;
                hash = hash * 31 + recipe.PatchCoordinate.x;
                hash = hash * 31 + recipe.PatchCoordinate.y;
                hash = hash * 31 + (int)recipe.TransitionDirection;
                hash = hash * 31 + Quantize(recipe.TransitionHeight);
                hash = hash * 31 + (int)recipe.Profile;
                hash = hash * 31 + Quantize(recipe.BroadForm);
                hash = hash * 31 + Quantize(recipe.Roughness);
                hash = hash * 31 + Quantize(recipe.SurfaceDetail);
                hash = hash * 31 + (int)recipe.EdgeBlend;
                hash = hash * 31 + Quantize(recipe.SurfaceVariation);
                hash = hash * 31 + (recipe.UseModifiers ? 1 : 0);
                hash = hash * 31 + (resolvedSurfaceProfile != null ? 1 : 0);
                hash = hash * 31 + Quantize(
                    GroundSurfaceProfile.ResolvePatchScale(resolvedSurfaceProfile));
                hash = hash * 31 + Quantize(
                    GroundSurfaceProfile.ResolvePatchContrast(resolvedSurfaceProfile));
                hash = hash * 31 + Quantize(
                    GroundSurfaceProfile.ResolvePatchEdgeSoftness(resolvedSurfaceProfile));
                hash = hash * 31 + Quantize(
                    GroundSurfaceProfile.ResolveExposureBias(resolvedSurfaceProfile));
                hash = hash * 31 + Quantize(
                    GroundSurfaceProfile.ResolveDampDepositBias(resolvedSurfaceProfile));
                hash = hash * 31 + Quantize(
                    GroundSurfaceProfile.ResolveVegetationSuitability(resolvedSurfaceProfile));
                hash = hash * 31 + Quantize(
                    GroundSurfaceProfile.ResolveRockyDrySuitability(resolvedSurfaceProfile));
                hash = hash * 31 + Quantize(
                    GroundSurfaceProfile.ResolveSnowEligibility(resolvedSurfaceProfile));
                hash = hash * 31 + Quantize(
                    GroundSurfaceProfile.ResolveRainAbsorption(resolvedSurfaceProfile));
                return hash;
            }
        }

        private int CalculateGroundGeometrySignature(
            int snapshotSignature)
        {
            unchecked
            {
                int hash = CalculateGenerationSignature();
                hash = hash * 31 + snapshotSignature;
                return hash;
            }
        }

        private int CalculateGroundSnapshotSignature(
            IReadOnlyList<GroundModifierSnapshot> modifierSnapshots,
            IReadOnlyList<StylizedRiverGroundSnapshot> riverSnapshots,
            bool includeFeatureExclusions)
        {
            unchecked
            {
                int hash = 17;
                int modifierCount =
                    modifierSnapshots != null ? modifierSnapshots.Count : 0;
                hash = hash * 31 + modifierCount;
                for (int index = 0; index < modifierCount; index++)
                {
                    GroundModifierSnapshot modifier = modifierSnapshots[index];
                    hash = hash * 31 + (int)modifier.Mode;
                    hash = hash * 31 + (int)modifier.Shape;
                    hash = hash * 31 + modifier.Priority;
                    hash = hash * 31 + (int)modifier.SurfaceEffectMode;
                    if (includeFeatureExclusions)
                    {
                        hash = hash * 31 + (int)modifier.FeatureExclusions;
                    }
                    hash = hash * 31 + Quantize(modifier.SurfaceCompactionStrength);
                    hash = hash * 31 + Quantize(modifier.SurfaceDampDepositStrength);
                    hash = hash * 31 + Quantize(modifier.SurfaceStandingWaterStrength);
                    hash = hash * 31 + Quantize(modifier.Strength);
                    hash = hash * 31 + Quantize(modifier.BlendDistance);
                    hash = hash * 31 + Quantize(modifier.Centre.x);
                    hash = hash * 31 + Quantize(modifier.Centre.y);
                    hash = hash * 31 + Quantize(modifier.TargetHeight);
                    hash = hash * 31 + Quantize(modifier.Right.x);
                    hash = hash * 31 + Quantize(modifier.Right.y);
                    hash = hash * 31 + Quantize(modifier.Forward.x);
                    hash = hash * 31 + Quantize(modifier.Forward.y);
                    hash = hash * 31 + Quantize(modifier.CircleRadius);
                    hash = hash * 31 + Quantize(modifier.BoxSize.x);
                    hash = hash * 31 + Quantize(modifier.BoxSize.y);
                    hash = hash * 31 + Quantize(modifier.HeightAmount);
                    hash = hash * 31 + Quantize(modifier.PreserveDetail);
                }

                int riverCount = rivers != null ? rivers.Length : 0;
                hash = hash * 31 + riverCount;
                for (int index = 0; index < riverCount; index++)
                {
                    StylizedRiver river = rivers[index];
                    if (river == null)
                    {
                        hash = hash * 31;
                        continue;
                    }

                    hash = hash * 31 +
                        HashDeterministicString(river.GetEntityId().ToString());
                    Matrix4x4 matrix = river.transform.localToWorldMatrix;
                    for (int element = 0; element < 16; element++)
                    {
                        hash = hash * 31 + Quantize(matrix[element]);
                    }
                    hash = hash * 31 +
                        (river.isActiveAndEnabled ? 1 : 0);
#if UNITY_EDITOR
                    hash = hash * 31 + HashDeterministicString(
                        EditorJsonUtility.ToJson(river, false));
#else
                    hash = hash * 31 + runtimeRiverNotificationRevision;
#endif
                }

                int validRiverSnapshotCount =
                    riverSnapshots != null ? riverSnapshots.Count : 0;
                hash = hash * 31 + validRiverSnapshotCount;
                return hash;
            }
        }

        private static int HashDeterministicString(string value)
        {
            unchecked
            {
                int hash = 17;
                if (string.IsNullOrEmpty(value))
                {
                    return hash;
                }

                for (int index = 0; index < value.Length; index++)
                {
                    hash = hash * 31 + value[index];
                }

                return hash;
            }
        }

        private static int Quantize(float value)
        {
            return Mathf.RoundToInt(value * 10000f);
        }

        private static int GenerateDifferentSeed(int current)
        {
            int candidate =
                1 +
                (int)((uint)Guid.NewGuid().GetHashCode() %
                      GroundRecipe.MaximumSeed);

            if (candidate == current)
            {
                candidate = current >= GroundRecipe.MaximumSeed
                    ? GroundRecipe.MinimumSeed
                    : current + 1;
            }

            return candidate;
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

            if (meshCollider == null)
            {
                meshCollider = GetComponent<MeshCollider>();
            }
        }

        private void ApplySurfaceProfileMaterialProperties()
        {
            ApplySurfaceProfileMaterialProperties(meshRenderer);
        }

        public void ApplySurfaceProfileMaterialProperties(Renderer targetRenderer)
        {
            if (targetRenderer == null)
            {
                return;
            }

            GroundSurfaceProfile resolvedSurfaceProfile =
                ResolveSurfaceProfile();

            float patchContrast =
                GroundSurfaceProfile.ResolvePatchContrast(resolvedSurfaceProfile);

            float exposureBias =
                GroundSurfaceProfile.ResolveExposureBias(resolvedSurfaceProfile);

            float dampBias =
                GroundSurfaceProfile.ResolveDampDepositBias(resolvedSurfaceProfile);

            float vegetationSuitability =
                GroundSurfaceProfile.ResolveVegetationSuitability(resolvedSurfaceProfile);

            float rockyDrySuitability =
                GroundSurfaceProfile.ResolveRockyDrySuitability(resolvedSurfaceProfile);

            float snowEligibility =
                GroundSurfaceProfile.ResolveSnowEligibility(resolvedSurfaceProfile);

            float rainAbsorption =
                GroundSurfaceProfile.ResolveRainAbsorption(resolvedSurfaceProfile);

            GroundMaterialControls resolvedMaterialControls =
                ResolveMaterialControls();

            materialProperties ??= new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(materialProperties);

            // Surface Contract: 0 = generated mass/rock, 1 = generated ground.
            // The material may be shared with stones and corridor geometry, so
            // ground-specific response is selected per-renderer through a
            // property block rather than by mutating or duplicating the material
            // asset.
            materialProperties.SetColor(
                BaseColorId,
                resolvedMaterialControls.BaseColor);
            materialProperties.SetFloat(SurfaceContractId, 1f);
            materialProperties.SetFloat(
                ProfileContrastId,
                Mathf.Lerp(0.85f, 1.35f, patchContrast) *
                resolvedMaterialControls.ProfileContrastScale);
            materialProperties.SetFloat(
                ProfilePixelContrastId,
                Mathf.Lerp(0.80f, 1.35f, patchContrast) *
                resolvedMaterialControls.ProfilePixelContrastScale);
            materialProperties.SetFloat(
                GroundSnowResponseId,
                Mathf.Lerp(0.25f, 1.35f, snowEligibility) *
                Mathf.Lerp(0.90f, 1.15f, exposureBias) *
                resolvedMaterialControls.GroundSnowResponseScale);
            materialProperties.SetFloat(
                GroundDampResponseId,
                Mathf.Lerp(0.45f, 1.35f, dampBias) *
                Mathf.Lerp(0.90f, 1.20f, rainAbsorption) *
                resolvedMaterialControls.GroundDampResponseScale);
            materialProperties.SetFloat(
                GroundVegetationResponseId,
                Mathf.Lerp(0.05f, 0.55f, vegetationSuitability) *
                resolvedMaterialControls.GroundVegetationResponseScale);
            materialProperties.SetFloat(
                GroundRockyDryResponseId,
                Mathf.Lerp(0.15f, 0.95f, rockyDrySuitability) *
                resolvedMaterialControls.GroundRockyDryResponseScale);
            materialProperties.SetFloat(
                GroundShoreDampStrengthId,
                Mathf.Lerp(0.75f, 1.35f, rainAbsorption) *
                resolvedMaterialControls.GroundShoreDampStrengthScale);
            materialProperties.SetFloat(
                PixelCellSizeId,
                resolvedMaterialControls.PixelCellSize);
            materialProperties.SetFloat(
                PixelToneCountId,
                resolvedMaterialControls.PixelToneCount);
            materialProperties.SetFloat(
                PixelClusterStrengthId,
                resolvedMaterialControls.PixelClusterStrength);
            materialProperties.SetFloat(
                PixelVariationId,
                resolvedMaterialControls.PixelVariation);
            materialProperties.SetFloat(
                PixelBroadVariationId,
                resolvedMaterialControls.BroadVariation);
            materialProperties.SetFloat(
                PixelVertexVariationId,
                resolvedMaterialControls.VertexVariation);
            materialProperties.SetFloat(
                PixelEffectStrengthId,
                resolvedMaterialControls.PixelEffectStrength);
            materialProperties.SetFloat(
                PixelWarpStrengthId,
                resolvedMaterialControls.CellWarpStrength);
            materialProperties.SetFloat(
                GroundPatchBlendStrengthId,
                resolvedMaterialControls.GroundPatchBlendStrength);
            materialProperties.SetFloat(
                GroundMacroPatchScaleId,
                resolvedMaterialControls.GroundMacroPatchScale);
            materialProperties.SetFloat(
                GroundSnowTintStrengthId,
                resolvedMaterialControls.GroundSnowTintStrength);
            materialProperties.SetFloat(
                GroundSnowBrightnessId,
                resolvedMaterialControls.GroundSnowBrightness);
            materialProperties.SetFloat(
                GroundDampDarkenStrengthId,
                resolvedMaterialControls.GroundDampDarkenStrength);
            materialProperties.SetColor(
                GroundDampTintId,
                resolvedMaterialControls.DampTint);
            materialProperties.SetFloat(
                GroundDampTintStrengthId,
                resolvedMaterialControls.DampTintStrength);
            materialProperties.SetColor(
                GroundRockyDryTintId,
                resolvedMaterialControls.RockyDryTint);
            materialProperties.SetFloat(
                GroundRockyDryTintStrengthId,
                resolvedMaterialControls.RockyDryTintStrength);
            materialProperties.SetColor(
                GroundVegetationTintId,
                resolvedMaterialControls.VegetationTint);
            materialProperties.SetFloat(
                GroundVegetationTintStrengthId,
                resolvedMaterialControls.VegetationTintStrength);
            materialProperties.SetFloat(
                WetnessId,
                resolvedMaterialControls.Wetness);
            materialProperties.SetFloat(
                WetDarkenStrengthId,
                resolvedMaterialControls.WetDarkenStrength);
            materialProperties.SetFloat(
                WetPixelSofteningId,
                resolvedMaterialControls.WetPixelSoftening);
            materialProperties.SetFloat(
                WetSmoothnessBoostId,
                resolvedMaterialControls.WetSmoothnessBoost);
            materialProperties.SetFloat(
                FrostStrengthId,
                resolvedMaterialControls.FrostStrength);
            materialProperties.SetFloat(
                FrostContrastId,
                resolvedMaterialControls.FrostContrast);
            materialProperties.SetColor(
                FrostColorId,
                resolvedMaterialControls.FrostColor);
            materialProperties.SetFloat(
                MonolithicFlattenId,
                resolvedMaterialControls.MonolithicFlatten);
            materialProperties.SetFloat(
                MonolithicSmoothnessBoostId,
                resolvedMaterialControls.MonolithicSmoothnessBoost);
            materialProperties.SetFloat(
                SmoothnessId,
                resolvedMaterialControls.Smoothness);
            materialProperties.SetFloat(
                SpecularStrengthId,
                resolvedMaterialControls.SpecularStrength);
            materialProperties.SetFloat(
                MaskDebugModeId,
                (float)debugView);

            ApplyResolvedFeatureMaterialProperties(materialProperties);
            ApplyPaintedAccentCoverageProperties(materialProperties);

            targetRenderer.SetPropertyBlock(materialProperties);
        }

        private void ApplyResolvedFeatureMaterialProperties(
            MaterialPropertyBlock properties)
        {
            // Retire the old single-feature slot for active rendering while
            // keeping the hidden material properties written to safe values for
            // serialized-material compatibility.
            properties.SetFloat(GroundFeatureModeId, 0f);
            properties.SetFloat(GroundFeatureStrengthId, 0f);
            properties.SetFloat(GroundFeatureScaleId, 1f);
            properties.SetFloat(GroundFeatureContrastId, 0f);
            properties.SetFloat(GroundFeatureMaskInfluenceId, 0f);
            properties.SetVector(
                GroundFeatureDirectionId,
                new Vector4(1f, 0f, 0f, 0f));
            properties.SetFloat(GroundFeatureSeedId, 0f);

            ApplyFeatureRecipe(
                properties,
                ResolveShaderFeature(GroundSurfaceFeatureKind.DirectionalStreaks),
                GroundDirectionalStreakFeatureIds);
            ApplyFeatureRecipe(
                properties,
                ResolveShaderFeature(GroundSurfaceFeatureKind.PooledWetness),
                GroundPooledWetnessFeatureIds);
            ApplyFeatureRecipe(
                properties,
                ResolveShaderFeature(GroundSurfaceFeatureKind.TrampledWear),
                GroundTrampledWearFeatureIds);
            GroundSurfaceFeatureRecipe paintedAccentFeature =
                ResolveShaderFeature(
                    GroundSurfaceFeatureKind.PaintedAccentLines);
            properties.SetFloat(
                GroundPaintedAccentLineStrengthId,
                paintedAccentFeature != null &&
                paintedAccentFeature.CanApplyAsShaderOnly
                    ? paintedAccentFeature.Strength
                    : 0f);
        }

        private void ApplyPaintedAccentCoverageProperties(
            MaterialPropertyBlock properties)
        {
            GroundSurfaceFeatureRecipe feature =
                ResolveShaderFeature(
                    GroundSurfaceFeatureKind.PaintedAccentLines);
            Color inkColor =
                feature != null
                    ? feature.PaintedAccentInkColor
                    : new Color(0.12f, 0.10f, 0.08f, 1f);

            properties.SetTexture(
                GroundPaintedAccentCoverageId,
                ResolvePaintedAccentCoverageTexture());
            properties.SetFloat(
                GroundPaintedAccentCoverageEnabledId,
                paintedAccentCoverageEnabled ? 1f : 0f);
            properties.SetVector(
                GroundPaintedAccentCoverageOriginSizeId,
                paintedAccentCoverageOriginSize);
            properties.SetMatrix(
                GroundPaintedAccentCoverageWorldToLocalId,
                transform.worldToLocalMatrix);
            properties.SetColor(
                GroundPaintedAccentInkColorId,
                inkColor);
        }

        private Texture2D ResolvePaintedAccentCoverageTexture()
        {
            if (paintedAccentCoverageTexture == null)
            {
                paintedAccentCoverageTexture =
                    GroundPaintedAccentCoverageBaker.CreateNeutralTexture();
            }

            return paintedAccentCoverageTexture;
        }

        private void EnsurePaintedAccentSurfaceStrokesCurrent()
        {
            GroundSurfaceFeatureRecipe feature =
                ResolveShaderFeature(
                    GroundSurfaceFeatureKind.PaintedAccentLines);
            int signature =
                CalculatePaintedAccentSurfaceStrokeSignature(feature);

            if (paintedAccentSurfaceStrokesInitialized &&
                paintedAccentSurfaceStrokeSignature == signature)
            {
                lastSurfaceStrokeMilliseconds = 0d;
                return;
            }

            paintedAccentSurfaceStrokeSignature = signature;
            paintedAccentSurfaceStrokesInitialized = true;

            if (!CanGeneratePaintedAccentSurfaceStrokes(feature))
            {
                ApplyNeutralPaintedAccentSurfaceStrokes();
                paintedAccentSurfaceStrokeSignature = signature;
                return;
            }

            long startedAt = System.Diagnostics.Stopwatch.GetTimestamp();
            using (SurfaceStrokeProfilerMarker.Auto())
            {
                GroundPaintedAccentSurfaceStroke[] surfaceStrokes =
                    GroundPaintedAccentSurfaceStrokeGenerator.GenerateSurfaceStrokes(
                        generatedMesh.bounds,
                        baseSurface,
                        feature,
                        recipe != null ? recipe.ShapeSeed : 0,
                        recipe != null
                            ? recipe.PatchCoordinate
                            : Vector2Int.zero,
                        ResolvePaintedAccentVisualHorizontalLocalXZ(),
                        paintedAccentRiverSnapshots,
                        paintedAccentModifierSnapshots,
                        out _,
                        out GroundPaintedAccentPlacementDiagnostics diagnostics,
                        out GroundPaintedAccentCompositionDebugSnapshot
                            compositionDebugSnapshot);

                paintedAccentSurfaceStrokes =
                    surfaceStrokes ??
                    Array.Empty<GroundPaintedAccentSurfaceStroke>();
                paintedAccentPlacementDiagnostics = diagnostics;
                paintedAccentCompositionDebugSnapshot =
                    compositionDebugSnapshot;
                paintedAccentSurfaceStrokeRevision++;
                ClearPaintedAccentProjectedGlyphDebugCache();
            }

            lastSurfaceStrokeMilliseconds =
                ResolveElapsedMilliseconds(startedAt);
        }

        private bool CanGeneratePaintedAccentSurfaceStrokes(
            GroundSurfaceFeatureRecipe feature)
        {
            return feature != null &&
                   feature.CanApplyAsShaderOnly &&
                   feature.Strength > 0.0001f &&
                   generatedMesh != null &&
                   baseSurface != null &&
                   baseSurface.IsValid;
        }

        private void ApplyNeutralPaintedAccentSurfaceStrokes()
        {
            paintedAccentSurfaceStrokes =
                Array.Empty<GroundPaintedAccentSurfaceStroke>();
            paintedAccentPlacementDiagnostics =
                GroundPaintedAccentPlacementDiagnostics.Empty;
            paintedAccentCompositionDebugSnapshot =
                GroundPaintedAccentCompositionDebugSnapshot.Empty;
            paintedAccentSurfaceStrokeRevision++;
            lastSurfaceStrokeMilliseconds = 0d;
            ClearPaintedAccentProjectedGlyphDebugCache();
        }

        private void EnsurePaintedAccentCoverageCurrent()
        {
            GroundSurfaceFeatureRecipe feature =
                ResolveShaderFeature(
                    GroundSurfaceFeatureKind.PaintedAccentLines);
            int signature =
                CalculatePaintedAccentCoverageSignature(feature);

            if (paintedAccentCoverageSignature == signature &&
                paintedAccentCoverageTexture != null)
            {
                lastCoverageRasterMilliseconds = 0d;
                lastCoverageUploadMilliseconds = 0d;
                return;
            }

            paintedAccentCoverageSignature = signature;
            EnsurePaintedAccentProjectedGlyphsCurrent(feature);

            if (!CanGeneratePaintedAccentSurfaceStrokes(feature) ||
                !paintedAccentProjectedGlyphDebugSnapshot.IsValid ||
                paintedAccentProjectedGlyphDebugSnapshot.Glyphs == null ||
                paintedAccentProjectedGlyphDebugSnapshot.Glyphs.Length == 0)
            {
                ApplyNeutralPaintedAccentCoverage();
                paintedAccentCoverageSignature = signature;
                return;
            }

            Texture2D generatedTexture;
            Vector4 originSize;
            GroundPaintedAccentCoverageDiagnostics diagnostics;
            double rasterMilliseconds;
            double uploadMilliseconds;
            using (CoverageProfilerMarker.Auto())
            {
                generatedTexture =
                    GroundPaintedAccentCoverageBaker.Bake(
                        generatedMesh.bounds,
                        paintedAccentProjectedGlyphDebugSnapshot.Glyphs,
                        paintedAccentCoverageTexture,
                        ref paintedAccentCoveragePixels,
                        out originSize,
                        out diagnostics,
                        out rasterMilliseconds,
                        out uploadMilliseconds);
            }

            ReplacePaintedAccentCoverageTexture(generatedTexture);
            paintedAccentCoverageEnabled = diagnostics.IsValid;
            paintedAccentCoverageOriginSize = originSize;
            paintedAccentCoverageDiagnostics = diagnostics;
            lastCoverageRasterMilliseconds = rasterMilliseconds;
            lastCoverageUploadMilliseconds = uploadMilliseconds;
        }

        private int CalculatePaintedAccentCoverageSignature(
            GroundSurfaceFeatureRecipe feature)
        {
            unchecked
            {
                int hash =
                    CalculatePaintedAccentProjectedGlyphSignature(feature);
                hash = hash * 31 +
                    GroundPaintedAccentCoverageBaker.Revision;
                return hash;
            }
        }

        private void ApplyNeutralPaintedAccentCoverage()
        {
            paintedAccentCoverageEnabled = false;
            paintedAccentCoverageDiagnostics =
                GroundPaintedAccentCoverageDiagnostics.Empty;
            ReplacePaintedAccentCoverageTexture(
                GroundPaintedAccentCoverageBaker.CreateNeutralTexture());

            Bounds localBounds =
                generatedMesh != null
                    ? generatedMesh.bounds
                    : new Bounds(Vector3.zero, Vector3.one);
            paintedAccentCoverageOriginSize =
                new Vector4(
                    localBounds.min.x,
                    localBounds.min.z,
                    Mathf.Max(0.0001f, localBounds.size.x),
                    Mathf.Max(0.0001f, localBounds.size.z));
        }

        private void ReplacePaintedAccentCoverageTexture(
            Texture2D texture)
        {
            if (paintedAccentCoverageTexture != null &&
                paintedAccentCoverageTexture != texture)
            {
                if (Application.isPlaying)
                {
                    Destroy(paintedAccentCoverageTexture);
                }
                else
                {
                    DestroyImmediate(paintedAccentCoverageTexture);
                }
            }

            paintedAccentCoverageTexture = texture;
        }

        private void EnsurePaintedAccentProjectedGlyphsCurrent(
            GroundSurfaceFeatureRecipe feature)
        {
            int signature =
                CalculatePaintedAccentProjectedGlyphSignature(feature);

            if (paintedAccentProjectedGlyphSignature == signature &&
                paintedAccentProjectedGlyphDebugSnapshot.IsValid)
            {
                paintedAccentProjectedGlyphBuildTimings =
                    GroundPaintedAccentProjectedGlyphBuildTimings.Empty;
                lastProjectedGlyphMilliseconds = 0d;
                return;
            }

            paintedAccentProjectedGlyphSignature = signature;

            if (!CanGeneratePaintedAccentSurfaceStrokes(feature) ||
                paintedAccentSurfaceStrokes == null)
            {
                paintedAccentProjectedGlyphDebugSnapshot =
                    GroundPaintedAccentProjectedGlyphDebugSnapshot.Empty;
                paintedAccentProjectedGlyphBuildTimings =
                    GroundPaintedAccentProjectedGlyphBuildTimings.Empty;
                lastProjectedGlyphMilliseconds = 0d;
                return;
            }

            long startedAt = System.Diagnostics.Stopwatch.GetTimestamp();
            using (ProjectedGlyphProfilerMarker.Auto())
            {
                paintedAccentProjectedGlyphDebugSnapshot =
                    GroundPaintedAccentProjectedGlyphGenerator.Build(
                        paintedAccentSurfaceStrokes,
                        baseSurface,
                        feature,
                        paintedAccentRiverExclusionSnapshots,
                        paintedAccentModifierSnapshots,
                        ResolvePaintedAccentProjectedNorthLocalXZ(),
                        out paintedAccentProjectedGlyphBuildTimings);
            }

            lastProjectedGlyphMilliseconds =
                ResolveElapsedMilliseconds(startedAt);
        }

        private int CalculatePaintedAccentProjectedGlyphSignature(
            GroundSurfaceFeatureRecipe feature)
        {
            unchecked
            {
                int hash = CalculatePaintedAccentSurfaceStrokeSignature(feature);
                hash = hash * 31 +
                    GroundPaintedAccentProjectedGlyphGenerator.Revision;
                hash = hash * 31 + paintedAccentSurfaceStrokeRevision;
                hash = hash * 31 + Quantize(
                    feature != null
                        ? feature.PaintedAccentCrestCrownHeight
                        : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.PaintedAccentFoldHeight : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.PaintedAccentFoldIrregularity : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.PaintedAccentFoldEndTaper : 0f);
                Vector2 localNorth = ResolvePaintedAccentProjectedNorthLocalXZ();
                hash = hash * 31 + Quantize(localNorth.x);
                hash = hash * 31 + Quantize(localNorth.y);
                return hash;
            }
        }

        private Vector2 ResolvePaintedAccentVisualHorizontalLocalXZ()
        {
            Vector3 localHorizontal3 =
                transform.InverseTransformDirection(Vector3.right);
            Vector2 localHorizontal =
                new Vector2(localHorizontal3.x, localHorizontal3.z);
            return localHorizontal.sqrMagnitude > 0.000001f
                ? localHorizontal.normalized
                : Vector2.right;
        }

        private Vector2 ResolvePaintedAccentProjectedNorthLocalXZ()
        {
            Vector3 localNorth3 =
                transform.InverseTransformDirection(Vector3.forward);
            Vector2 localNorth =
                new Vector2(localNorth3.x, localNorth3.z);
            return localNorth.sqrMagnitude > 0.000001f
                ? localNorth.normalized
                : Vector2.up;
        }

        private void ClearPaintedAccentProjectedGlyphDebugCache()
        {
            paintedAccentProjectedGlyphSignature = 0;
            paintedAccentProjectedGlyphDebugSnapshot =
                GroundPaintedAccentProjectedGlyphDebugSnapshot.Empty;
            paintedAccentCoverageSignature = 0;
        }

        private int CalculatePaintedAccentSurfaceStrokeSignature(
            GroundSurfaceFeatureRecipe feature)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + appliedGroundGeometrySignature;
                hash = hash * 31 + currentPaintedAccentDomainSignature;
                hash = hash * 31 + groundGeometryRevision;
                hash = hash * 31 +
                    GroundPaintedAccentSurfaceStrokeGenerator.Revision;
                hash = hash * 31 +
                    (feature != null && feature.CanApplyAsShaderOnly ? 1 : 0);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.Strength : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.Scale : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.Contrast : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.MaskInfluence : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.PaintedAccentStrokeWidth : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.PaintedAccentStrokeDensity : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.PaintedAccentDistributionPatchScale : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.PaintedAccentDistributionPatchiness : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.PaintedAccentDistributionSparseFloor : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.PaintedAccentCompositionRegionScale : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.PaintedAccentCompositionDensityContrast : 0f);
                float horizontalCompanionStrength =
                    feature != null
                        ? feature.PaintedAccentHorizontalCompanionStrength
                        : 0f;
                hash = hash * 31 + Quantize(horizontalCompanionStrength);
                if (horizontalCompanionStrength > 0.0001f)
                {
                    hash = hash * 31 + Quantize(
                        feature != null
                            ? feature.PaintedAccentCompanionTightness
                            : 0f);
                    hash = hash * 31 + Quantize(
                        feature != null
                            ? feature.PaintedAccentCompanionTripletVerticality
                            : 1f);
                    Vector2 visualHorizontal =
                        ResolvePaintedAccentVisualHorizontalLocalXZ();
                    hash = hash * 31 + Quantize(visualHorizontal.x);
                    hash = hash * 31 + Quantize(visualHorizontal.y);
                }
                Vector4 familyWeights =
                    feature != null
                        ? feature.PaintedAccentGlyphFamilyWeights
                        : Vector4.zero;
                hash = hash * 31 + Quantize(familyWeights.x);
                hash = hash * 31 + Quantize(familyWeights.y);
                hash = hash * 31 + Quantize(familyWeights.z);
                hash = hash * 31 + Quantize(familyWeights.w);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.PaintedAccentStrokeLengthMin : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.PaintedAccentStrokeLengthMax : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.PaintedAccentStrokeFacingDirectionDegrees : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.PaintedAccentStrokeAngleJitterDegrees : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.PaintedAccentStrokePathWiggle : 0f);

                hash = hash * 31 +
                    (feature != null ? feature.SeedOffset : 0);

                if (generatedMesh != null)
                {
                    Bounds localBounds = generatedMesh.bounds;
                    hash = hash * 31 + Quantize(localBounds.min.x);
                    hash = hash * 31 + Quantize(localBounds.min.z);
                    hash = hash * 31 + Quantize(localBounds.size.x);
                    hash = hash * 31 + Quantize(localBounds.size.z);
                }

                return hash;
            }
        }

        private static void ApplyFeatureRecipe(
            MaterialPropertyBlock properties,
            GroundSurfaceFeatureRecipe feature,
            GroundShaderFeaturePropertyIds propertyIds)
        {
            if (feature == null || !feature.CanApplyAsShaderOnly)
            {
                properties.SetFloat(propertyIds.StrengthId, 0f);
                properties.SetFloat(propertyIds.ScaleId, 5f);
                properties.SetFloat(propertyIds.ContrastId, 0.5f);
                properties.SetFloat(propertyIds.MaskInfluenceId, 0.5f);
                properties.SetVector(
                    propertyIds.DirectionId,
                    new Vector4(1f, 0f, 0f, 0f));
                properties.SetFloat(propertyIds.SeedId, 0f);
                return;
            }

            Vector2 direction = feature.Direction;

            properties.SetFloat(
                propertyIds.StrengthId,
                feature.Strength);
            properties.SetFloat(
                propertyIds.ScaleId,
                feature.Scale);
            properties.SetFloat(
                propertyIds.ContrastId,
                feature.Contrast);
            properties.SetFloat(
                propertyIds.MaskInfluenceId,
                feature.MaskInfluence);
            properties.SetVector(
                propertyIds.DirectionId,
                new Vector4(direction.x, direction.y, 0f, 0f));
            properties.SetFloat(
                propertyIds.SeedId,
                feature.SeedOffset);
        }

        private static double ResolveElapsedMilliseconds(long startedAt)
        {
            long elapsedTicks =
                System.Diagnostics.Stopwatch.GetTimestamp() - startedAt;
            return elapsedTicks * 1000d /
                   System.Diagnostics.Stopwatch.Frequency;
        }

        private void ResetRegenerationTiming()
        {
            lastSnapshotsMilliseconds = 0d;
            lastGeometryMilliseconds = 0d;
            lastMeshApplyMilliseconds = 0d;
            lastColliderMilliseconds = 0d;
            lastSurfaceStrokeMilliseconds = 0d;
            lastProjectedGlyphMilliseconds = 0d;
            paintedAccentProjectedGlyphBuildTimings =
                GroundPaintedAccentProjectedGlyphBuildTimings.Empty;
            lastCoverageRasterMilliseconds = 0d;
            lastCoverageUploadMilliseconds = 0d;
            lastMaterialMilliseconds = 0d;
            lastRiverCorridorMilliseconds = 0d;
            lastTotalRegenerationMilliseconds = 0d;
        }

        private void UpdateRegenerationTimingDiagnostics()
        {
            lastRegenerationTimingDiagnostics =
                $"Executed stages: {lastExecutedRegenerationStages}\n" +
                $"Total: {lastTotalRegenerationMilliseconds:F2} ms\n" +
                $"Snapshots: {lastSnapshotsMilliseconds:F2} ms\n" +
                $"Ground generation: {lastGeometryMilliseconds:F2} ms\n" +
                $"Mesh apply: {lastMeshApplyMilliseconds:F2} ms\n" +
                $"Collider cook: {lastColliderMilliseconds:F2} ms\n" +
                $"Painted Accent strokes: {lastSurfaceStrokeMilliseconds:F2} ms\n" +
                $"Painted Accent projection: {lastProjectedGlyphMilliseconds:F2} ms\n" +
                $"  profile build: {paintedAccentProjectedGlyphBuildTimings.ProfileBuildMilliseconds:F2} ms\n" +
                $"  family validation: {paintedAccentProjectedGlyphBuildTimings.FamilyValidationMilliseconds:F2} ms\n" +
                $"  point construction: {paintedAccentProjectedGlyphBuildTimings.PointConstructionMilliseconds:F2} ms\n" +
                $"  topology + turn: {paintedAccentProjectedGlyphBuildTimings.TopologyValidationMilliseconds:F2} ms\n" +
                $"  projected cluster composition: {paintedAccentProjectedGlyphBuildTimings.ClusterCompositionMilliseconds:F2} ms\n" +
                $"  surface/domain validation: {paintedAccentProjectedGlyphBuildTimings.SurfaceValidationMilliseconds:F2} ms\n" +
                $"    footprint preparation: {paintedAccentProjectedGlyphBuildTimings.FootprintPreparationMilliseconds:F2} ms\n" +
                $"    ground sampling: {paintedAccentProjectedGlyphBuildTimings.GroundSamplingMilliseconds:F2} ms\n" +
                $"    broad slope: {paintedAccentProjectedGlyphBuildTimings.BroadSlopeMilliseconds:F2} ms\n" +
                $"    river exclusion: {paintedAccentProjectedGlyphBuildTimings.RiverExclusionMilliseconds:F2} ms\n" +
                $"    modifier exclusion: {paintedAccentProjectedGlyphBuildTimings.ModifierExclusionMilliseconds:F2} ms\n" +
                $"    transverse grade: {paintedAccentProjectedGlyphBuildTimings.TransverseGradeMilliseconds:F2} ms\n" +
                $"    longitudinal grade: {paintedAccentProjectedGlyphBuildTimings.LongitudinalGradeMilliseconds:F2} ms\n" +
                $"  diagnostics: {paintedAccentProjectedGlyphBuildTimings.DiagnosticsMilliseconds:F2} ms\n" +
                $"Coverage raster: {lastCoverageRasterMilliseconds:F2} ms\n" +
                $"Coverage upload: {lastCoverageUploadMilliseconds:F2} ms\n" +
                $"Material apply: {lastMaterialMilliseconds:F2} ms\n" +
                $"River corridor notification: {lastRiverCorridorMilliseconds:F2} ms";
        }

        private void EnsureGeneratedMesh()
        {
            if (generatedMesh != null)
            {
                return;
            }

            generatedMesh = new Mesh
            {
                name = "GeneratedGround_Temporary",
                hideFlags = HideFlags.DontSave
            };
        }

        private void ClearGeneratedAssignments()
        {
            baseSurface = GroundHeightFieldSnapshot.Empty;
            lastSurfaceMaskDiagnostics =
                "Surface masks have not been generated yet.";
            appliedGroundGeometrySignature = 0;
            groundGeometryInitialized = false;
            groundGeometryRevision = 0;
            currentSnapshotSignature = 0;
            currentPaintedAccentDomainSignature = 0;
            paintedAccentSurfaceStrokeSignature = 0;
            paintedAccentSurfaceStrokesInitialized = false;
            paintedAccentModifierSnapshots = Array.Empty<GroundModifierSnapshot>();
            paintedAccentRiverSnapshots = Array.Empty<StylizedRiverGroundSnapshot>();
            paintedAccentRiverExclusionSnapshots =
                Array.Empty<GroundPaintedAccentRiverExclusionSnapshot>();
            ApplyNeutralPaintedAccentSurfaceStrokes();
            ApplyNeutralPaintedAccentCoverage();

            if (meshFilter != null &&
                meshFilter.sharedMesh == generatedMesh)
            {
                meshFilter.sharedMesh = null;
            }

            if (meshCollider != null &&
                meshCollider.sharedMesh == generatedMesh)
            {
                meshCollider.sharedMesh = null;
            }
        }

        private void OnDestroy()
        {
            ClearGeneratedAssignments();

            if (paintedAccentCoverageTexture != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(paintedAccentCoverageTexture);
                }
                else
                {
                    DestroyImmediate(paintedAccentCoverageTexture);
                }

                paintedAccentCoverageTexture = null;
            }

            if (generatedMesh == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(generatedMesh);
            }
            else
            {
                DestroyImmediate(generatedMesh);
            }

            generatedMesh = null;
        }
    }
}
