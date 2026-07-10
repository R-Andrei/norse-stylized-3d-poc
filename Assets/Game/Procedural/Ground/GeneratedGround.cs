using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
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

        [InspectorName("Ground Painted Accent Relief")]
        GroundPaintedAccentRelief = 29,

        [InspectorName("Ground Painted Accent Signed Relief")]
        GroundPaintedAccentSignedRelief = 30,

        [InspectorName("Ground Painted Accent Final Prototype")]
        GroundPaintedAccentFinalPrototype = 31
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
        public const string SnowfieldCleanVariantId = "snowfield.clean";
        public const string SnowfieldPatchyVariantId = "snowfield.patchy";
        public const string SnowfieldDirtyThawingVariantId =
            "snowfield.dirty_thawing";
        public const string SnowfieldWindScouredVariantId =
            "snowfield.wind_scoured";

        private const int CurrentSurfaceStyleMigrationVersion = 1;
        private const string PaintedAccentFoldSurfacePreviewName =
            "__PaintedAccentRidgePreview_Debug";
        private const string LegacyPaintedAccentFoldSurfacePreviewName =
            "__PaintedAccentFoldSurfacePreview_Debug";
        private const string LegacyPaintedAccentFoldLinePreviewName =
            "__FoldFieldLinePreview_Debug";
        private const float PaintedAccentRidgeBoundaryEmbedDepth = 0.002f;
        private const float PaintedAccentRidgeMinimumEndWidthScale = 0.12f;
        private const int PaintedAccentFoldCrossSectionSampleCount = 7;

        // Position (12) + normal (12) + UV0 (8). The proof mesh has no
        // tangents, vertex colours, collider data, or secondary streams.
        private const int PaintedAccentRidgeVertexStrideBytes = 32;

        private static Material paintedAccentRidgePreviewMaterial;

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
        private MaterialPropertyBlock materialProperties;
        private Texture2D paintedAccentFoldFieldTexture;
        private bool paintedAccentFoldFieldEnabled;
        private Vector4 paintedAccentFoldFieldOriginSize =
            new Vector4(0f, 0f, 1f, 1f);
        private Vector4 paintedAccentFoldFieldTexelSize =
            new Vector4(1f, 1f, 1f, 1f);
        private int paintedAccentFoldFieldSignature;
        private GroundPaintedAccentSurfaceStroke[] paintedAccentSurfaceStrokes =
            Array.Empty<GroundPaintedAccentSurfaceStroke>();

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
        private static readonly int GroundPaintedAccentFoldFieldId =
            Shader.PropertyToID("_GroundPaintedAccentFoldField");
        private static readonly int GroundPaintedAccentFoldFieldEnabledId =
            Shader.PropertyToID("_GroundPaintedAccentFoldFieldEnabled");
        private static readonly int GroundPaintedAccentFoldFieldOriginSizeId =
            Shader.PropertyToID("_GroundPaintedAccentFoldFieldOriginSize");
        private static readonly int GroundPaintedAccentFoldFieldTexelSizeId =
            Shader.PropertyToID("_GroundPaintedAccentFoldFieldTexelSize");
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
        private static readonly GroundShaderFeaturePropertyIds
            GroundPaintedAccentLineFeatureIds =
                new GroundShaderFeaturePropertyIds("_GroundPaintedAccentLine");

        private readonly struct PaintedAccentFoldProfileBasis
        {
            public PaintedAccentFoldProfileBasis(
                float center,
                float width,
                float amplitude,
                float centerDrift,
                float widthVariation,
                float amplitudeVariation,
                float phase,
                float frequency)
            {
                Center = center;
                Width = Mathf.Max(0.04f, width);
                Amplitude =
                    Mathf.Abs(amplitude) <= 0.001f
                        ? (amplitude < 0f ? -0.001f : 0.001f)
                        : amplitude;
                CenterDrift = centerDrift;
                WidthVariation = Mathf.Max(0f, widthVariation);
                AmplitudeVariation = Mathf.Max(0f, amplitudeVariation);
                Phase = phase;
                Frequency = Mathf.Max(0.05f, frequency);
            }

            public float Center { get; }
            public float Width { get; }
            public float Amplitude { get; }
            public float CenterDrift { get; }
            public float WidthVariation { get; }
            public float AmplitudeVariation { get; }
            public float Phase { get; }
            public float Frequency { get; }
        }

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
            CacheComponents();

            if (recipe == null)
            {
                ClearGeneratedAssignments();
                return;
            }

            EnsureGeneratedMesh();

            List<GroundModifierSnapshot> snapshots =
                BuildModifierSnapshots();

            List<StylizedRiverGroundSnapshot> riverSnapshots =
                BuildRiverSnapshots();

            GroundSurfaceProfile resolvedSurfaceProfile =
                ResolveSurfaceProfile();

            MeshData meshData = GroundGenerator.Generate(
                recipe,
                resolvedSurfaceProfile,
                snapshots,
                riverSnapshots,
                out baseSurface,
                out lastSurfaceMaskDiagnostics);

            string meshName =
                $"GeneratedGround_{recipe.PatchSize}_{recipe.Resolution}_Seed{recipe.ShapeSeed}";

            MeshBuilder.ApplyToMesh(
                meshData,
                generatedMesh,
                meshName);

            meshFilter.sharedMesh = generatedMesh;

            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = generatedMesh;
            meshCollider.convex = false;

            lastValidatedGenerationSignature =
                CalculateGenerationSignature();

            paintedAccentFoldFieldSignature = 0;
            EnsurePaintedAccentFoldFieldCurrent();

            ApplySurfaceProfileMaterialProperties();
            NotifyRiverCorridorsChanged();
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

        public void RefreshSurfaceMaterialProperties()
        {
            CacheComponents();
            EnsurePaintedAccentFoldFieldCurrent();
            ApplySurfaceProfileMaterialProperties();
            RefreshRiverCorridorMaterialProperties();
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

            if (!recipe.UseModifiers ||
                modifiers == null)
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

        private List<StylizedRiverGroundSnapshot> BuildRiverSnapshots()
        {
            List<StylizedRiverGroundSnapshot> snapshots =
                new List<StylizedRiverGroundSnapshot>();

            if (rivers == null)
            {
                return snapshots;
            }

            for (int index = 0;
                 index < rivers.Length;
                 index++)
            {
                StylizedRiver river = rivers[index];

                if (river == null ||
                    !river.isActiveAndEnabled)
                {
                    continue;
                }

                StylizedRiverGroundSnapshot snapshot =
                    river.CreateGroundSnapshot(transform);

                if (snapshot.IsValid)
                {
                    snapshots.Add(snapshot);
                }
            }

            return snapshots;
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
            ApplyPaintedAccentFoldFieldProperties(materialProperties);

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
            ApplyFeatureRecipe(
                properties,
                ResolveShaderFeature(GroundSurfaceFeatureKind.PaintedAccentLines),
                GroundPaintedAccentLineFeatureIds);
        }

        private void ApplyPaintedAccentFoldFieldProperties(
            MaterialPropertyBlock properties)
        {
            properties.SetTexture(
                GroundPaintedAccentFoldFieldId,
                ResolvePaintedAccentFoldFieldTexture());

            properties.SetFloat(
                GroundPaintedAccentFoldFieldEnabledId,
                paintedAccentFoldFieldEnabled ? 1f : 0f);

            properties.SetVector(
                GroundPaintedAccentFoldFieldOriginSizeId,
                paintedAccentFoldFieldOriginSize);

            properties.SetVector(
                GroundPaintedAccentFoldFieldTexelSizeId,
                paintedAccentFoldFieldTexelSize);
        }

        private Texture2D ResolvePaintedAccentFoldFieldTexture()
        {
            if (paintedAccentFoldFieldTexture == null)
            {
                paintedAccentFoldFieldTexture =
                    CreateNeutralPaintedAccentFoldFieldTexture();
            }

            return paintedAccentFoldFieldTexture;
        }

        private static Texture2D CreateNeutralPaintedAccentFoldFieldTexture()
        {
            Texture2D texture =
                new Texture2D(
                    1,
                    1,
                    TextureFormat.RGBA32,
                    false,
                    true)
                {
                    name = "GeneratedGround_NeutralPaintedAccentFoldField",
                    hideFlags = HideFlags.DontSave,
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };

            texture.SetPixel(0, 0, new Color32(0, 0, 128, 0));
            texture.Apply(false, true);
            return texture;
        }

        private void EnsurePaintedAccentFoldFieldCurrent()
        {
            GroundSurfaceFeatureRecipe feature =
                ResolveShaderFeature(GroundSurfaceFeatureKind.PaintedAccentLines);
            int signature =
                CalculatePaintedAccentFoldFieldSignature(feature);

            if (paintedAccentFoldFieldTexture != null &&
                paintedAccentFoldFieldSignature == signature)
            {
                return;
            }

            paintedAccentFoldFieldSignature = signature;

            if (!CanGeneratePaintedAccentFoldField(feature))
            {
                ApplyNeutralPaintedAccentFoldField();
                return;
            }

            Texture2D generatedTexture =
                GroundPaintedAccentFoldFieldGenerator.Generate(
                    generatedMesh.bounds,
                    baseSurface,
                    feature,
                    recipe != null ? recipe.ShapeSeed : 0,
                    out Vector4 originSize,
                    out Vector4 texelSize,
                    out float[] bodyValues,
                    out GroundPaintedAccentSurfaceStroke[] surfaceStrokes);

            paintedAccentSurfaceStrokes =
                surfaceStrokes ?? Array.Empty<GroundPaintedAccentSurfaceStroke>();
            ReplacePaintedAccentFoldFieldTexture(generatedTexture);
            paintedAccentFoldFieldEnabled = true;
            paintedAccentFoldFieldOriginSize = originSize;
            paintedAccentFoldFieldTexelSize = texelSize;
        }

        private bool CanGeneratePaintedAccentFoldField(
            GroundSurfaceFeatureRecipe feature)
        {
            return feature != null &&
                   feature.CanApplyAsShaderOnly &&
                   feature.Strength > 0.0001f &&
                   generatedMesh != null &&
                   baseSurface != null &&
                   baseSurface.IsValid;
        }

        private void ApplyNeutralPaintedAccentFoldField()
        {
            paintedAccentFoldFieldEnabled = false;
            paintedAccentSurfaceStrokes = Array.Empty<GroundPaintedAccentSurfaceStroke>();
            ReplacePaintedAccentFoldFieldTexture(
                CreateNeutralPaintedAccentFoldFieldTexture());

            Bounds localBounds =
                generatedMesh != null
                    ? generatedMesh.bounds
                    : new Bounds(Vector3.zero, Vector3.one);

            paintedAccentFoldFieldOriginSize =
                new Vector4(
                    localBounds.min.x,
                    localBounds.min.z,
                    Mathf.Max(0.0001f, localBounds.size.x),
                    Mathf.Max(0.0001f, localBounds.size.z));

            paintedAccentFoldFieldTexelSize =
                new Vector4(1f, 1f, 1f, 1f);
        }

        private void ReplacePaintedAccentFoldFieldTexture(
            Texture2D texture)
        {
            if (paintedAccentFoldFieldTexture != null &&
                paintedAccentFoldFieldTexture != texture)
            {
                if (Application.isPlaying)
                {
                    Destroy(paintedAccentFoldFieldTexture);
                }
                else
                {
                    DestroyImmediate(paintedAccentFoldFieldTexture);
                }
            }

            paintedAccentFoldFieldTexture = texture;
        }

        private int CalculatePaintedAccentFoldFieldSignature(
            GroundSurfaceFeatureRecipe feature)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + CalculateGenerationSignature();
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
                    feature != null ? feature.PaintedAccentStrokeLengthMin : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.PaintedAccentStrokeLengthMax : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.PaintedAccentStrokeFacingDirectionDegrees : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.PaintedAccentStrokeAngleJitterDegrees : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.PaintedAccentFoldHeight : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.PaintedAccentFoldIrregularity : 0f);
                hash = hash * 31 + Quantize(
                    feature != null ? feature.PaintedAccentFoldEndTaper : 0f);

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

        [ContextMenu("Build Painted Accent 3D Ridge Preview")]
        public void BuildPaintedAccentFoldSurfacePreview()
        {
            CacheComponents();

            if (generatedMesh == null ||
                baseSurface == null ||
                !baseSurface.IsValid)
            {
                Regenerate();
            }

            paintedAccentFoldFieldSignature = 0;
            EnsurePaintedAccentFoldFieldCurrent();

            if (!CanBuildPaintedAccentFoldSurfacePreview())
            {
                ClearPaintedAccentFoldSurfacePreview();
                return;
            }

            System.Diagnostics.Stopwatch buildStopwatch =
                System.Diagnostics.Stopwatch.StartNew();

            GroundSurfaceFeatureRecipe feature =
                ResolveShaderFeature(GroundSurfaceFeatureKind.PaintedAccentLines);
            float foldHeight =
                feature != null
                    ? feature.PaintedAccentFoldHeight
                    : 0.018f;
            float foldIrregularity =
                feature != null
                    ? feature.PaintedAccentFoldIrregularity
                    : 0.55f;
            float foldEndTaper =
                feature != null
                    ? feature.PaintedAccentFoldEndTaper
                    : 0.65f;

            ClearPaintedAccentFoldSurfacePreview();

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();
            int builtStrokeCount = 0;

            for (int strokeIndex = 0;
                 strokeIndex < paintedAccentSurfaceStrokes.Length;
                 strokeIndex++)
            {
                GroundPaintedAccentSurfaceStroke stroke =
                    paintedAccentSurfaceStrokes[strokeIndex];
                if (!stroke.IsValid)
                {
                    continue;
                }

                Vector3[] points = stroke.LocalPoints;
                Vector3[] normals = stroke.LocalNormals;
                PaintedAccentFoldProfileBasis[] profileBases =
                    BuildPaintedAccentFoldProfileBases(
                        stroke.Seed,
                        foldIrregularity,
                        out float profileNormalization);
                int baseVertex = vertices.Count;
                float halfWidth = stroke.Width * 0.5f;
                float strokeHeight =
                    foldHeight *
                    Mathf.Lerp(0.82f, 1f, stroke.Strength);

                for (int pointIndex = 0; pointIndex < points.Length; pointIndex++)
                {
                    float t =
                        points.Length <= 1
                            ? 0f
                            : pointIndex / (float)(points.Length - 1);
                    float endEnvelope =
                        ResolvePaintedAccentFoldEndEnvelope(
                            t,
                            stroke.Seed,
                            foldEndTaper);
                    float widthScale =
                        Mathf.Lerp(
                            PaintedAccentRidgeMinimumEndWidthScale,
                            1f,
                            endEnvelope);
                    float effectiveHalfWidth = halfWidth * widthScale;
                    bool isEndBoundary =
                        pointIndex == 0 ||
                        pointIndex == points.Length - 1;
                    Vector3 normal =
                        pointIndex < normals.Length &&
                        normals[pointIndex].sqrMagnitude > 0.000001f
                            ? normals[pointIndex].normalized
                            : Vector3.up;
                    Vector3 tangent =
                        ResolveStrokePreviewTangent(
                            points,
                            pointIndex,
                            normal);
                    Vector3 across =
                        Vector3.Cross(normal, tangent);
                    if (across.sqrMagnitude <= 0.000001f)
                    {
                        across = Vector3.Cross(Vector3.up, tangent);
                    }

                    if (across.sqrMagnitude <= 0.000001f)
                    {
                        across = Vector3.right;
                    }

                    across.Normalize();

                    for (int crossIndex = 0;
                         crossIndex < PaintedAccentFoldCrossSectionSampleCount;
                         crossIndex++)
                    {
                        float cross01 =
                            crossIndex /
                            (float)(PaintedAccentFoldCrossSectionSampleCount - 1);
                        float u = cross01 * 2f - 1f;
                        float normalizedHeight =
                            ResolvePaintedAccentFoldProfileHeight(
                                t,
                                u,
                                stroke.Seed,
                                profileBases,
                                profileNormalization,
                                foldIrregularity,
                                endEnvelope);
                        float height = strokeHeight * normalizedHeight;
                        Vector3 lateralPosition =
                            points[pointIndex] +
                            across * (u * effectiveHalfWidth);
                        Vector3 groundPosition = lateralPosition;
                        Vector3 liftNormal = normal;
                        Vector2 lateralXZ =
                            new Vector2(
                                lateralPosition.x,
                                lateralPosition.z);
                        if (baseSurface.TrySample(
                                lateralXZ,
                                out GroundSurfaceSample lateralSample))
                        {
                            groundPosition =
                                new Vector3(
                                    lateralXZ.x,
                                    lateralSample.Height,
                                    lateralXZ.y);
                            if (lateralSample.RenderNormal.sqrMagnitude > 0.000001f)
                            {
                                liftNormal = lateralSample.RenderNormal.normalized;
                            }
                        }

                        bool isSideBoundary =
                            crossIndex == 0 ||
                            crossIndex ==
                                PaintedAccentFoldCrossSectionSampleCount - 1;
                        float boundaryEmbed =
                            isEndBoundary || isSideBoundary
                                ? PaintedAccentRidgeBoundaryEmbedDepth
                                : 0f;
                        Vector3 surfacePosition =
                            groundPosition +
                            liftNormal * (height - boundaryEmbed);

                        vertices.Add(surfacePosition);
                        uvs.Add(new Vector2(t, cross01));
                    }
                }

                for (int pointIndex = 0;
                     pointIndex < points.Length - 1;
                     pointIndex++)
                {
                    for (int crossIndex = 0;
                         crossIndex < PaintedAccentFoldCrossSectionSampleCount - 1;
                         crossIndex++)
                    {
                        int a =
                            baseVertex +
                            pointIndex * PaintedAccentFoldCrossSectionSampleCount +
                            crossIndex;
                        int b = a + 1;
                        int c = a + PaintedAccentFoldCrossSectionSampleCount;
                        int d = c + 1;

                        triangles.Add(a);
                        triangles.Add(c);
                        triangles.Add(b);
                        triangles.Add(b);
                        triangles.Add(c);
                        triangles.Add(d);
                    }
                }

                builtStrokeCount++;
            }

            if (vertices.Count < PaintedAccentFoldCrossSectionSampleCount * 2 ||
                triangles.Count < 6)
            {
                return;
            }

            bool use32BitIndices = vertices.Count > ushort.MaxValue;
            Mesh previewMesh =
                new Mesh
                {
                    name = "GeneratedGround_PaintedAccentRidgePreview",
                    hideFlags = HideFlags.DontSave,
                    indexFormat =
                        use32BitIndices
                            ? IndexFormat.UInt32
                            : IndexFormat.UInt16
                };
            previewMesh.SetVertices(vertices);
            previewMesh.SetUVs(0, uvs);
            previewMesh.SetTriangles(triangles, 0);
            previewMesh.RecalculateNormals();
            previewMesh.RecalculateBounds();

            GameObject previewObject =
                new GameObject(PaintedAccentFoldSurfacePreviewName)
                {
                    hideFlags = HideFlags.DontSave
                };
            previewObject.transform.SetParent(transform, false);
            previewObject.transform.localPosition = Vector3.zero;
            previewObject.transform.localRotation = Quaternion.identity;
            previewObject.transform.localScale = Vector3.one;

            MeshFilter previewFilter =
                previewObject.AddComponent<MeshFilter>();
            MeshRenderer previewRenderer =
                previewObject.AddComponent<MeshRenderer>();
            previewFilter.sharedMesh = previewMesh;
            previewRenderer.shadowCastingMode = ShadowCastingMode.Off;
            previewRenderer.receiveShadows = true;
            previewRenderer.motionVectorGenerationMode =
                MotionVectorGenerationMode.Camera;
            previewRenderer.sharedMaterial =
                ResolvePaintedAccentRidgePreviewMaterial();

            buildStopwatch.Stop();
            int triangleCount = triangles.Count / 3;
            int indexStrideBytes = use32BitIndices ? 4 : 2;
            long estimatedVertexBufferBytes =
                (long)vertices.Count * PaintedAccentRidgeVertexStrideBytes;
            long estimatedIndexBufferBytes =
                (long)triangles.Count * indexStrideBytes;
            long estimatedRawMeshBytes =
                estimatedVertexBufferBytes + estimatedIndexBufferBytes;

            Debug.Log(
                $"GeneratedGround Painted Accent ridge built: " +
                $"strokes={builtStrokeCount}, " +
                $"vertices={vertices.Count}, " +
                $"triangles={triangleCount}, " +
                $"estimatedVertexBufferBytes={estimatedVertexBufferBytes}, " +
                $"estimatedIndexBufferBytes={estimatedIndexBufferBytes}, " +
                $"estimatedRawMeshBytes={estimatedRawMeshBytes}, " +
                $"buildMs={buildStopwatch.Elapsed.TotalMilliseconds:F2}.",
                this);

#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(
                previewObject,
                "Build Painted Accent 3D Ridge Preview");
#endif
        }

        [ContextMenu("Clear Painted Accent 3D Ridge Preview")]
        public void ClearPaintedAccentFoldSurfacePreview()
        {
            List<GameObject> previewObjects =
                new List<GameObject>();

            for (int childIndex = 0; childIndex < transform.childCount; childIndex++)
            {
                Transform child =
                    transform.GetChild(childIndex);

                if (child != null &&
                    IsPaintedAccentFoldPreviewObjectName(child.name))
                {
                    previewObjects.Add(child.gameObject);
                }
            }

            for (int index = 0; index < previewObjects.Count; index++)
            {
                DestroyPaintedAccentFoldSurfacePreviewObject(
                    previewObjects[index]);
            }
        }

        private bool CanBuildPaintedAccentFoldSurfacePreview()
        {
            return paintedAccentFoldFieldEnabled &&
                   paintedAccentSurfaceStrokes != null &&
                   paintedAccentSurfaceStrokes.Length > 0 &&
                   baseSurface != null &&
                   baseSurface.IsValid;
        }

        private static bool IsPaintedAccentFoldPreviewObjectName(
            string objectName)
        {
            return !string.IsNullOrEmpty(objectName) &&
                   (objectName.StartsWith(
                        PaintedAccentFoldSurfacePreviewName,
                        StringComparison.Ordinal) ||
                    objectName.StartsWith(
                        LegacyPaintedAccentFoldSurfacePreviewName,
                        StringComparison.Ordinal) ||
                    objectName.StartsWith(
                        LegacyPaintedAccentFoldLinePreviewName,
                        StringComparison.Ordinal));
        }

        private static Vector3 ResolveStrokePreviewTangent(
            Vector3[] points,
            int pointIndex,
            Vector3 surfaceNormal)
        {
            if (points == null || points.Length < 2)
            {
                return ResolvePreviewFallbackTangent(surfaceNormal);
            }

            Vector3 previous =
                points[Mathf.Max(0, pointIndex - 1)];
            Vector3 next =
                points[Mathf.Min(points.Length - 1, pointIndex + 1)];
            Vector3 tangent =
                Vector3.ProjectOnPlane(
                    next - previous,
                    surfaceNormal);

            if (tangent.sqrMagnitude <= 0.000001f)
            {
                return ResolvePreviewFallbackTangent(surfaceNormal);
            }

            return tangent.normalized;
        }

        private static Vector3 ResolvePreviewFallbackTangent(
            Vector3 surfaceNormal)
        {
            Vector3 tangent =
                Vector3.ProjectOnPlane(
                    Vector3.forward,
                    surfaceNormal);
            if (tangent.sqrMagnitude <= 0.000001f)
            {
                tangent =
                    Vector3.ProjectOnPlane(
                        Vector3.right,
                        surfaceNormal);
            }

            if (tangent.sqrMagnitude <= 0.000001f)
            {
                tangent = Vector3.forward;
            }

            return tangent.normalized;
        }

        private static PaintedAccentFoldProfileBasis[]
            BuildPaintedAccentFoldProfileBases(
                int strokeSeed,
                float irregularity,
                out float normalization)
        {
            irregularity = Mathf.Clamp01(irregularity);
            int maximumAdditionalBases =
                irregularity <= 0.001f
                    ? 0
                    : Mathf.Clamp(
                          Mathf.CeilToInt(irregularity * 3f),
                          1,
                          3);
            int additionalBases = 0;
            if (maximumAdditionalBases > 0)
            {
                additionalBases =
                    1 +
                    Mathf.Min(
                        maximumAdditionalBases - 1,
                        Mathf.FloorToInt(
                            ResolvePaintedAccentPreviewHash01(
                                strokeSeed,
                                101u) *
                            maximumAdditionalBases));
            }

            int basisCount = 1 + additionalBases;
            PaintedAccentFoldProfileBasis[] bases =
                new PaintedAccentFoldProfileBasis[basisCount];
            normalization = 1f;
            float primaryWidth =
                Mathf.Lerp(
                    0.34f,
                    0.50f,
                    ResolvePaintedAccentPreviewHash01(
                        strokeSeed,
                        109u));

            for (int basisIndex = 0; basisIndex < basisCount; basisIndex++)
            {
                uint salt = (uint)(basisIndex * 47 + 137);
                bool isPrimary = basisIndex == 0;
                float center =
                    isPrimary
                        ? ResolvePaintedAccentPreviewSignedHash(
                              strokeSeed,
                              salt + 1u) *
                          0.20f * irregularity
                        : ResolvePaintedAccentPreviewSignedHash(
                              strokeSeed,
                              salt + 1u) *
                          Mathf.Lerp(
                              0.22f,
                              0.62f,
                              ResolvePaintedAccentPreviewHash01(
                                  strokeSeed,
                                  salt + 3u));
                float width =
                    primaryWidth *
                    (isPrimary
                        ? Mathf.Lerp(
                              0.84f,
                              1.14f,
                              ResolvePaintedAccentPreviewHash01(
                                  strokeSeed,
                                  salt + 5u))
                        : Mathf.Lerp(
                              0.38f,
                              0.82f,
                              ResolvePaintedAccentPreviewHash01(
                                  strokeSeed,
                                  salt + 5u)));
                float amplitude = 1f;
                if (!isPrimary)
                {
                    float amplitudeMagnitude =
                        Mathf.Lerp(
                            0.14f,
                            0.56f,
                            ResolvePaintedAccentPreviewHash01(
                                strokeSeed,
                                salt + 7u)) *
                        irregularity;
                    float amplitudeSign =
                        ResolvePaintedAccentPreviewHash01(
                            strokeSeed,
                            salt + 9u) < 0.33f
                            ? -1f
                            : 1f;
                    amplitude = amplitudeMagnitude * amplitudeSign;
                }
                float centerDrift =
                    ResolvePaintedAccentPreviewSignedHash(
                        strokeSeed,
                        salt + 11u) *
                    0.18f * irregularity;
                float widthVariation =
                    Mathf.Lerp(
                        0.04f,
                        0.24f,
                        ResolvePaintedAccentPreviewHash01(
                            strokeSeed,
                            salt + 13u)) *
                    irregularity;
                float amplitudeVariation =
                    Mathf.Lerp(
                        0.07f,
                        0.32f,
                        ResolvePaintedAccentPreviewHash01(
                            strokeSeed,
                            salt + 17u)) *
                    irregularity;
                float phase =
                    ResolvePaintedAccentPreviewHash01(
                        strokeSeed,
                        salt + 19u) *
                    Mathf.PI * 2f;
                float frequency =
                    Mathf.Lerp(
                        0.55f,
                        1.35f,
                        ResolvePaintedAccentPreviewHash01(
                            strokeSeed,
                            salt + 23u));

                bases[basisIndex] =
                    new PaintedAccentFoldProfileBasis(
                        center,
                        width,
                        amplitude,
                        centerDrift,
                        widthVariation,
                        amplitudeVariation,
                        phase,
                        frequency);
                if (!isPrimary && amplitude > 0f)
                {
                    normalization += amplitude * 0.35f;
                }
            }

            normalization = Mathf.Max(0.001f, normalization);
            return bases;
        }

        private static float ResolvePaintedAccentFoldProfileHeight(
            float t,
            float u,
            int strokeSeed,
            PaintedAccentFoldProfileBasis[] profileBases,
            float profileNormalization,
            float irregularity,
            float endEnvelope)
        {
            float edgePower =
                Mathf.Lerp(
                    1.15f,
                    1.65f,
                    ResolvePaintedAccentPreviewHash01(
                        strokeSeed,
                        307u));
            float edgeEnvelope =
                Mathf.Pow(
                    Mathf.Max(0f, 1f - u * u),
                    edgePower);
            if (edgeEnvelope <= 0f)
            {
                return 0f;
            }

            float profile = 0f;
            for (int basisIndex = 0;
                 basisIndex < profileBases.Length;
                 basisIndex++)
            {
                PaintedAccentFoldProfileBasis basis =
                    profileBases[basisIndex];
                float angle =
                    t * Mathf.PI * 2f * basis.Frequency +
                    basis.Phase;
                float center =
                    basis.Center +
                    basis.CenterDrift * Mathf.Sin(angle);
                float width =
                    basis.Width *
                    (1f +
                     basis.WidthVariation *
                     Mathf.Sin(angle * 0.83f + basis.Phase * 0.47f));
                width = Mathf.Max(0.04f, width);
                float amplitude =
                    basis.Amplitude *
                    (1f +
                     basis.AmplitudeVariation *
                     Mathf.Sin(angle * 1.11f + basis.Phase * 0.71f));
                float normalizedDistance =
                    (u - center) /
                    width;
                float gaussian =
                    Mathf.Exp(
                        -0.5f *
                        normalizedDistance *
                        normalizedDistance);
                profile += amplitude * gaussian;
            }

            profile /= Mathf.Max(0.001f, profileNormalization);

            float crossPhaseA =
                ResolvePaintedAccentPreviewHash01(
                    strokeSeed,
                    313u) *
                Mathf.PI * 2f;
            float crossPhaseB =
                ResolvePaintedAccentPreviewHash01(
                    strokeSeed,
                    317u) *
                Mathf.PI * 2f;
            float crossVariation =
                1f +
                irregularity *
                (Mathf.Sin(
                     u * Mathf.PI * 1.25f +
                     t * Mathf.PI * 0.55f +
                     crossPhaseA) * 0.24f +
                 Mathf.Sin(
                     u * Mathf.PI * 2.35f -
                     t * Mathf.PI * 0.70f +
                     crossPhaseB) * 0.14f);
            profile =
                Mathf.Max(
                    0f,
                    profile * Mathf.Max(0.40f, crossVariation));

            float phaseA =
                ResolvePaintedAccentPreviewHash01(
                    strokeSeed,
                    271u) *
                Mathf.PI * 2f;
            float phaseB =
                ResolvePaintedAccentPreviewHash01(
                    strokeSeed,
                    277u) *
                Mathf.PI * 2f;
            float alongVariation =
                1f +
                irregularity *
                (Mathf.Sin(t * Mathf.PI * 1.55f + phaseA) * 0.30f +
                 Mathf.Sin(t * Mathf.PI * 3.10f + phaseB) * 0.18f);
            alongVariation = Mathf.Clamp(alongVariation, 0.45f, 1.50f);

            return Mathf.Clamp(
                profile *
                edgeEnvelope *
                alongVariation *
                endEnvelope,
                0f,
                1.55f);
        }

        private static float ResolvePaintedAccentFoldEndEnvelope(
            float t,
            int strokeSeed,
            float endTaper)
        {
            float taperFraction =
                Mathf.Lerp(0.025f, 0.35f, Mathf.Clamp01(endTaper));
            float startTaper =
                taperFraction *
                Mathf.Lerp(
                    0.84f,
                    1.16f,
                    ResolvePaintedAccentPreviewHash01(
                        strokeSeed,
                        283u));
            float finishTaper =
                taperFraction *
                Mathf.Lerp(
                    0.84f,
                    1.16f,
                    ResolvePaintedAccentPreviewHash01(
                        strokeSeed,
                        293u));

            return
                Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(t / Mathf.Max(0.001f, startTaper))) *
                Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01((1f - t) / Mathf.Max(0.001f, finishTaper)));
        }

        private static float ResolvePaintedAccentPreviewSignedHash(
            int seed,
            uint salt)
        {
            return ResolvePaintedAccentPreviewHash01(seed, salt) * 2f - 1f;
        }

        private static float ResolvePaintedAccentPreviewHash01(
            int seed,
            uint salt)
        {
            unchecked
            {
                uint value = (uint)seed ^ (salt * 0x9E3779B9u);
                value ^= value >> 16;
                value *= 0x7FEB352Du;
                value ^= value >> 15;
                value *= 0x846CA68Bu;
                value ^= value >> 16;
                return (value & 0x00FFFFFFu) / 16777215f;
            }
        }

        private static Material ResolvePaintedAccentRidgePreviewMaterial()
        {
            if (paintedAccentRidgePreviewMaterial != null)
            {
                return paintedAccentRidgePreviewMaterial;
            }

            Shader shader =
                Shader.Find("Universal Render Pipeline/Lit");

            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader == null)
            {
                return null;
            }

            paintedAccentRidgePreviewMaterial =
                new Material(shader)
                {
                    name = "GeneratedGround_PaintedAccentRidgePreview_SharedMaterial",
                    hideFlags = HideFlags.DontSave
                };

            Color previewColor = new Color(0.22f, 0.18f, 0.14f, 1f);
            if (paintedAccentRidgePreviewMaterial.HasProperty("_BaseColor"))
            {
                paintedAccentRidgePreviewMaterial.SetColor(
                    "_BaseColor",
                    previewColor);
            }

            if (paintedAccentRidgePreviewMaterial.HasProperty("_Color"))
            {
                paintedAccentRidgePreviewMaterial.SetColor(
                    "_Color",
                    previewColor);
            }

            if (paintedAccentRidgePreviewMaterial.HasProperty("_Metallic"))
            {
                paintedAccentRidgePreviewMaterial.SetFloat("_Metallic", 0f);
            }

            if (paintedAccentRidgePreviewMaterial.HasProperty("_Smoothness"))
            {
                paintedAccentRidgePreviewMaterial.SetFloat("_Smoothness", 0.08f);
            }

            if (paintedAccentRidgePreviewMaterial.HasProperty("_SpecularHighlights"))
            {
                paintedAccentRidgePreviewMaterial.SetFloat(
                    "_SpecularHighlights",
                    0f);
            }

            return paintedAccentRidgePreviewMaterial;
        }

        private static void DestroyPaintedAccentFoldSurfacePreviewObject(
            GameObject previewObject)
        {
            MeshFilter meshFilter =
                previewObject.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer =
                previewObject.GetComponent<MeshRenderer>();

            if (meshFilter != null &&
                meshFilter.sharedMesh != null)
            {
                DestroyGeneratedObject(meshFilter.sharedMesh);
            }

            if (meshRenderer != null &&
                meshRenderer.sharedMaterial != null &&
                meshRenderer.sharedMaterial != paintedAccentRidgePreviewMaterial)
            {
                // Legacy V3J.3B previews owned one material per child. Destroy
                // those stale materials, but never destroy the shared ridge material.
                DestroyGeneratedObject(meshRenderer.sharedMaterial);
            }

            DestroyGeneratedObject(previewObject);
        }

        private static void DestroyGeneratedObject(
            UnityEngine.Object generatedObject)
        {
            if (generatedObject == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(generatedObject);
            }
            else
            {
                DestroyImmediate(generatedObject);
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
            paintedAccentFoldFieldSignature = 0;
            ApplyNeutralPaintedAccentFoldField();

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
            ClearPaintedAccentFoldSurfacePreview();
            ClearGeneratedAssignments();

            if (paintedAccentFoldFieldTexture != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(paintedAccentFoldFieldTexture);
                }
                else
                {
                    DestroyImmediate(paintedAccentFoldFieldTexture);
                }

                paintedAccentFoldFieldTexture = null;
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
