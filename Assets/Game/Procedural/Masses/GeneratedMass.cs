using System;
using UnityEngine;
using UnityEngine.Serialization;
using Unity.Profiling;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public enum MassArchetype
    {
        TerrainBoulder,
        SquatBoulder,
        StandingStone,
        FlatSlab,
        BrokenChunk,
        PolishedStone,
        LayeredStone,
        CarvedMarkerStone,
        FracturedPillar
    }

    public enum MassScaleStep
    {
        XS,
        S,
        M,
        L,
        XL,
        XXL,
        Monumental
    }


    public enum FormComplexity
    {
        Primitive,
        Simple,
        Moderate,
        Complex,
        HighlyComplex
    }

    public enum SurfaceFacetDensity
    {
        Sparse,
        Low,
        Medium,
        High,
        VeryHigh
    }

    public enum GeneratedMassGenerationBudget
    {
        Compact,
        Standard,
        Detailed,
        Hero,
        Custom
    }

    public enum EdgeCharacter
    {
        Sharp,
        Chipped,
        Natural,
        Worn,
        Polished
    }

    public enum StoneSurfaceProfile
    {
        RendererMaterial,
        ColdGreyStone,
        DarkWetRiverStone,
        PaleFrostStone,
        BlackSacredStone
    }

    public enum GeneratedMassFeatureRecipe
    {
        GenericTestMass,
        ColdGreyStone,
        WetRiverStone,
        PaleFrostStone,
        BlackSacredStone,
        Custom
    }

    public enum StoneSurfaceMaskDebug
    {
        None = 0,
        SurfaceVariation = 1,
        Exposure = 2,
        CreviceBase = 3,
        ConvexEdgeWear = 4,
        ConcaveCrease = 5,
        DirtDeposit = 6,

        // FeatureAtlas0 diagnostics. Values intentionally match
        // PixelSurfaceMaskDebugMode and avoid the shared ground-debug range.
        ConvexBoundaryProximity = 15,
        ConcaveBoundaryProximity = 16,
        ConvexBoundarySalienceComposite = 17,
        BoundarySalience = 18,
        BoundaryIdentity = 19,
        ConcaveBoundarySalienceComposite = 20,
        BoundaryFieldDiagnostic = 21,
        BoundaryModulationDiagnostic = 22,
        BoundaryAlongCoordinate = 23,
        BoundaryCrossCoordinate = 24,
        BoundaryCoarseModulation = 25,
        BoundaryFineModulation = 26
    }

    public enum ShapeDiversity
    {
        Restrained,
        Broad,
        Wild
    }

    public enum GroundingStyle
    {
        Light,
        Stable,
        Embedded
    }

    public enum LeanStyle
    {
        None,
        Subtle,
        Pronounced
    }

    [Serializable]
    public sealed class MassRecipe
    {
        public const int MinimumSeed = 1;
        public const int MaximumSeed = 9999;

        [Header("Identity")]

        [SerializeField]
        private MassArchetype archetype = MassArchetype.TerrainBoulder;

        [Tooltip("Controls proportions, major planes, cuts, lean and silhouette.")]
        [Range(MinimumSeed, MaximumSeed)]
        [SerializeField]
        private int shapeSeed = 1234;

        [Tooltip("Controls surface triangulation, subtle facet relief and vertex-colour variation.")]
        [Range(MinimumSeed, MaximumSeed)]
        [SerializeField]
        private int surfaceSeed = 5678;

        [Header("Primary Shape")]

        [SerializeField]
        private MassScaleStep size = MassScaleStep.M;

        [Tooltip("Controls the number of major cuts and dominant planes.")]
        [SerializeField]
        private FormComplexity formComplexity = FormComplexity.Moderate;

        [Tooltip("Controls triangulation across the major planes.")]
        [SerializeField]
        private SurfaceFacetDensity surfaceFacetDensity = SurfaceFacetDensity.Medium;

        [Tooltip("Controls how strongly major edges are preserved or worn down.")]
        [SerializeField]
        private EdgeCharacter edgeCharacter = EdgeCharacter.Natural;

        [Tooltip("Controls how far different shape seeds may depart from the archetype's base form.")]
        [SerializeField]
        private ShapeDiversity shapeDiversity = ShapeDiversity.Broad;

        [SerializeField]
        private GroundingStyle grounding = GroundingStyle.Stable;

        [SerializeField]
        private LeanStyle lean = LeanStyle.Subtle;

        [Header("Advanced Proportions")]

        [Tooltip("Fine adjustment within the selected size step.")]
        [Range(0.85f, 1.15f)]
        [SerializeField]
        private float fineScale = 1f;

        [Range(0.7f, 1.3f)]
        [SerializeField]
        private float widthBias = 1f;

        [Range(0.7f, 1.3f)]
        [SerializeField]
        private float heightBias = 1f;

        [Range(0.7f, 1.3f)]
        [SerializeField]
        private float depthBias = 1f;

        [Header("Surface Data")]

        [Tooltip("Amount of deterministic variation written into vertex colour red.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float surfaceVariation = 0.35f;

        public MassArchetype Archetype => archetype;
        public int ShapeSeed => shapeSeed;
        public int SurfaceSeed => surfaceSeed;
        public MassScaleStep Size => size;
        public FormComplexity FormComplexity => formComplexity;
        public SurfaceFacetDensity SurfaceFacetDensity => surfaceFacetDensity;
        public EdgeCharacter EdgeCharacter => edgeCharacter;
        public ShapeDiversity ShapeDiversity => shapeDiversity;
        public GroundingStyle Grounding => grounding;
        public LeanStyle Lean => lean;
        public float FineScale => fineScale;
        public float WidthBias => widthBias;
        public float HeightBias => heightBias;
        public float DepthBias => depthBias;
        public float SurfaceVariation => surfaceVariation;

        public void SetShapeSeed(int value)
        {
            shapeSeed = Mathf.Clamp(value, MinimumSeed, MaximumSeed);
        }

        public void SetSurfaceSeed(int value)
        {
            surfaceSeed = Mathf.Clamp(value, MinimumSeed, MaximumSeed);
        }

        public void ApplyArchetypeDefaults()
        {
            switch (archetype)
            {
                case MassArchetype.TerrainBoulder:
                    size = MassScaleStep.M;
                    formComplexity = FormComplexity.Moderate;
                    surfaceFacetDensity = SurfaceFacetDensity.Medium;
                    edgeCharacter = EdgeCharacter.Natural;
                    shapeDiversity = ShapeDiversity.Broad;
                    grounding = GroundingStyle.Stable;
                    lean = LeanStyle.Subtle;
                    break;

                case MassArchetype.SquatBoulder:
                    size = MassScaleStep.M;
                    formComplexity = FormComplexity.Moderate;
                    surfaceFacetDensity = SurfaceFacetDensity.Medium;
                    edgeCharacter = EdgeCharacter.Natural;
                    shapeDiversity = ShapeDiversity.Broad;
                    grounding = GroundingStyle.Embedded;
                    lean = LeanStyle.None;
                    break;

                case MassArchetype.StandingStone:
                    size = MassScaleStep.M;
                    formComplexity = FormComplexity.Simple;
                    surfaceFacetDensity = SurfaceFacetDensity.Low;
                    edgeCharacter = EdgeCharacter.Sharp;
                    shapeDiversity = ShapeDiversity.Broad;
                    grounding = GroundingStyle.Stable;
                    lean = LeanStyle.Subtle;
                    break;

                case MassArchetype.FlatSlab:
                    size = MassScaleStep.M;
                    formComplexity = FormComplexity.Simple;
                    surfaceFacetDensity = SurfaceFacetDensity.Low;
                    edgeCharacter = EdgeCharacter.Sharp;
                    shapeDiversity = ShapeDiversity.Restrained;
                    grounding = GroundingStyle.Embedded;
                    lean = LeanStyle.None;
                    break;

                case MassArchetype.BrokenChunk:
                    size = MassScaleStep.M;
                    formComplexity = FormComplexity.Complex;
                    surfaceFacetDensity = SurfaceFacetDensity.Medium;
                    edgeCharacter = EdgeCharacter.Chipped;
                    shapeDiversity = ShapeDiversity.Wild;
                    grounding = GroundingStyle.Stable;
                    lean = LeanStyle.Subtle;
                    break;

                case MassArchetype.PolishedStone:
                    size = MassScaleStep.M;
                    formComplexity = FormComplexity.Simple;
                    surfaceFacetDensity = SurfaceFacetDensity.Medium;
                    edgeCharacter = EdgeCharacter.Polished;
                    shapeDiversity = ShapeDiversity.Broad;
                    grounding = GroundingStyle.Stable;
                    lean = LeanStyle.None;
                    break;

                case MassArchetype.LayeredStone:
                    size = MassScaleStep.M;
                    formComplexity = FormComplexity.Simple;
                    surfaceFacetDensity = SurfaceFacetDensity.Low;
                    edgeCharacter = EdgeCharacter.Chipped;
                    shapeDiversity = ShapeDiversity.Broad;
                    grounding = GroundingStyle.Embedded;
                    lean = LeanStyle.None;
                    break;

                case MassArchetype.CarvedMarkerStone:
                    size = MassScaleStep.M;
                    formComplexity = FormComplexity.Complex;
                    surfaceFacetDensity = SurfaceFacetDensity.Low;
                    edgeCharacter = EdgeCharacter.Sharp;
                    shapeDiversity = ShapeDiversity.Wild;
                    grounding = GroundingStyle.Stable;
                    lean = LeanStyle.None;
                    break;

                case MassArchetype.FracturedPillar:
                    size = MassScaleStep.M;
                    formComplexity = FormComplexity.Moderate;
                    surfaceFacetDensity = SurfaceFacetDensity.Low;
                    edgeCharacter = EdgeCharacter.Chipped;
                    shapeDiversity = ShapeDiversity.Wild;
                    grounding = GroundingStyle.Stable;
                    lean = LeanStyle.Pronounced;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            fineScale = 1f;
            widthBias = 1f;
            heightBias = 1f;
            depthBias = 1f;
            surfaceVariation = 0.35f;
        }
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public sealed class GeneratedMass : MonoBehaviour,
        IGeneratedGeometrySource,
        IGeneratedRiverInteractionSource,
        IGeneratedGeometryStableFingerprintSource
    {
        private const string LegacyRiverFoamProxyObjectName =
            "RiverFoamProxy";
        private const string LegacySurfaceFeatureRootObjectName =
            "GeneratedMass_SurfaceFeatures";
        private const string LegacyEdgeWearFeatureObjectName =
            "GeneratedMass_EdgeWearFeatures";
        private const string LegacyCreaseFeatureObjectName =
            "GeneratedMass_CreaseFeatures";
        private const int CompactFeatureAtlasResolution = 128;
        private const int StandardFeatureAtlasResolution = 256;
        private const int DetailedFeatureAtlasResolution = 256;
        private const int HeroFeatureAtlasResolution = 512;
        private const int ProductionGenerationContractVersion = 1;
        private const int FeatureAtlasGenerationContractVersion = 1;
        private const string GeneratedMeshNamePrefix = "GeneratedMass_";
        private const string PlaneCutPreviewMeshNameSuffix =
            "_PlaneCutBevelPreview";
        private const string BoundedEdgePreviewMeshNameSuffix =
            "_BoundedEdgeBevelPreview";
        private const string UnifiedEdgeWearPreviewMeshNameSuffix =
            "_UnifiedEdgeWearBevelPreview";

        private enum PreviewGenerationMode
        {
            Production,
            PlaneCut,
            BoundedSingleEdge,
            UnifiedEdgeWear
        }
        private static readonly ProfilerMarker SynchronizeProfilerMarker =
            new ProfilerMarker("GeneratedMass.Synchronize");
        private static readonly ProfilerMarker GenerateProductionProfilerMarker =
            new ProfilerMarker("GeneratedMass.GenerateProduction");
        private static readonly ProfilerMarker BindColliderProfilerMarker =
            new ProfilerMarker("GeneratedMass.BindCollider");
        private static readonly ProfilerMarker ComputeFingerprintProfilerMarker =
            new ProfilerMarker("GeneratedMass.ComputeFingerprint");
        private static readonly ProfilerMarker NotifyConsumersProfilerMarker =
            new ProfilerMarker("GeneratedMass.NotifyConsumers");
        private static readonly int BaseColorId =
            Shader.PropertyToID("_BaseColor");
        private static readonly int LegacyBaseColorId =
            Shader.PropertyToID("_Base_Color");
        private static readonly int MaskDebugModeId =
            Shader.PropertyToID("_MaskDebugMode");
        private static readonly int SurfaceContractId =
            Shader.PropertyToID("_SurfaceContract");
        private static readonly int GeneratedMassLocalMinYId =
            Shader.PropertyToID("_GeneratedMassLocalMinY");
        private static readonly int GeneratedMassLocalHeightId =
            Shader.PropertyToID("_GeneratedMassLocalHeight");
        private static readonly int GeneratedMassMaskSeedId =
            Shader.PropertyToID("_GeneratedMassMaskSeed");
        private static readonly int GeneratedMassLocalXZScaleId =
            Shader.PropertyToID("_GeneratedMassLocalXZScale");
        private static readonly int GeneratedMassMaskBaseLiftId =
            Shader.PropertyToID("_GeneratedMassMaskBaseLift");
        private static readonly int GeneratedMassCreviceReachId =
            Shader.PropertyToID("_GeneratedMassCreviceReach");
        private static readonly int GeneratedMassCreviceSmoothnessId =
            Shader.PropertyToID("_GeneratedMassCreviceSmoothness");
        private static readonly int GeneratedMassCreviceBreakupId =
            Shader.PropertyToID("_GeneratedMassCreviceBreakup");
        private static readonly int GeneratedMassDirtCrawlReachId =
            Shader.PropertyToID("_GeneratedMassDirtCrawlReach");
        private static readonly int GeneratedMassDirtCoverageId =
            Shader.PropertyToID("_GeneratedMassDirtCoverage");
        private static readonly int GeneratedMassExposureResponseId =
            Shader.PropertyToID("_GeneratedMassExposureResponse");
        private static readonly int GeneratedMassCreviceResponseId =
            Shader.PropertyToID("_GeneratedMassCreviceResponse");
        private static readonly int GeneratedMassBaseResponseId =
            Shader.PropertyToID("_GeneratedMassBaseResponse");
        private static readonly int GeneratedMassDirtDepositResponseId =
            Shader.PropertyToID("_GeneratedMassDirtDepositResponse");
        private static readonly int GeneratedMassExposureTintId =
            Shader.PropertyToID("_GeneratedMassExposureTint");
        private static readonly int GeneratedMassExposureTintStrengthId =
            Shader.PropertyToID("_GeneratedMassExposureTintStrength");
        private static readonly int GeneratedMassCreviceTintId =
            Shader.PropertyToID("_GeneratedMassCreviceTint");
        private static readonly int GeneratedMassCreviceTintStrengthId =
            Shader.PropertyToID("_GeneratedMassCreviceTintStrength");
        private static readonly int GeneratedMassBaseTintId =
            Shader.PropertyToID("_GeneratedMassBaseTint");
        private static readonly int GeneratedMassBaseTintStrengthId =
            Shader.PropertyToID("_GeneratedMassBaseTintStrength");
        private static readonly int GeneratedMassDirtDepositTintId =
            Shader.PropertyToID("_GeneratedMassDirtDepositTint");
        private static readonly int GeneratedMassDirtDepositTintStrengthId =
            Shader.PropertyToID("_GeneratedMassDirtDepositTintStrength");
        private static readonly int GeneratedMassOverallRockTintId =
            Shader.PropertyToID("_GeneratedMassOverallRockTint");
        private static readonly int GeneratedMassOverallRockTintStrengthId =
            Shader.PropertyToID("_GeneratedMassOverallRockTintStrength");
        private static readonly int GeneratedMassLightingTintInfluenceId =
            Shader.PropertyToID("_GeneratedMassLightingTintInfluence");
        private static readonly int GeneratedMassFeatureAtlas0Id =
            Shader.PropertyToID("_GeneratedMassFeatureAtlas0");
        private static readonly int GeneratedMassFeatureAtlas0EnabledId =
            Shader.PropertyToID("_GeneratedMassFeatureAtlas0Enabled");
        private static readonly int GeneratedMassFeatureAtlas1Id =
            Shader.PropertyToID("_GeneratedMassFeatureAtlas1");
        private static readonly int GeneratedMassFeatureAtlas1EnabledId =
            Shader.PropertyToID("_GeneratedMassFeatureAtlas1Enabled");
        private static readonly int GeneratedMassFeatureAtlasQualityId =
            Shader.PropertyToID("_GeneratedMassFeatureAtlasQuality");
        private static readonly int GeneratedMassGeometryEdgeWearEnabledId =
            Shader.PropertyToID("_GeneratedMassGeometryEdgeWearEnabled");
        private static readonly int GeneratedMassEdgeWearCoverageId =
            Shader.PropertyToID("_GeneratedMassEdgeWearCoverage");
        private static readonly int GeneratedMassEdgeWearSoftnessId =
            Shader.PropertyToID("_GeneratedMassEdgeWearSoftness");
        private static readonly int GeneratedMassEdgeWearResponseStrengthId =
            Shader.PropertyToID("_GeneratedMassEdgeWearResponseStrength");
        private static readonly int GeneratedMassEdgeWearBrightnessLiftId =
            Shader.PropertyToID("_GeneratedMassEdgeWearBrightnessLift");
        private static readonly int GeneratedMassEdgeWearTintId =
            Shader.PropertyToID("_GeneratedMassEdgeWearTint");
        private static readonly int GeneratedMassEdgeWearTintStrengthId =
            Shader.PropertyToID("_GeneratedMassEdgeWearTintStrength");
        private static readonly int GeneratedMassEdgeWearMacroVariationId =
            Shader.PropertyToID("_GeneratedMassEdgeWearMacroVariation");
        private static readonly int GeneratedMassEdgeWearMicroVariationId =
            Shader.PropertyToID("_GeneratedMassEdgeWearMicroVariation");
        private static readonly int GeneratedMassCreaseLengthId =
            Shader.PropertyToID("_GeneratedMassCreaseLength");
        private static readonly int GeneratedMassCreaseBranchingId =
            Shader.PropertyToID("_GeneratedMassCreaseBranching");
        private static readonly int GeneratedMassCreaseSoftnessId =
            Shader.PropertyToID("_GeneratedMassCreaseSoftness");

        private struct FeatureRecipeValues
        {
            public StoneSurfaceProfile StoneSurfaceProfile;
            public Color BaseColor;
            public StoneSurfaceMaskDebug SurfaceMaskDebug;
            public float SurfaceMaskBaseLift;
            public float CreviceReach;
            public float CreviceSmoothness;
            public float CreviceBreakup;
            public float DirtCrawlReach;
            public float DirtCoverage;
            public float ExposureResponse;
            public float CreviceResponse;
            public float BaseResponse;
            public float DirtDepositResponse;
            public Color ExposureTint;
            public float ExposureTintStrength;
            public Color CreviceTint;
            public float CreviceTintStrength;
            public Color BaseTint;
            public float BaseTintStrength;
            public Color DirtDepositTint;
            public float DirtDepositTintStrength;
            public Color OverallRockTint;
            public float OverallRockTintStrength;
            public float LightingTintInfluence;
            public float EdgeWearAmount;
            public float EdgeWearWidth;
            public float EdgeWearCoverage;
            public float EdgeWearSoftness;
            public float EdgeWearResponseStrength;
            public float EdgeWearBrightnessLift;
            public Color EdgeWearTint;
            public float EdgeWearTintStrength;
            public float EdgeWearMacroVariation;
            public float EdgeWearMicroVariation;
            public float CreaseAmount;
            public float CreaseWidth;
            public float CreaseLength;
            public float CreaseBranching;
            public float CreaseSoftness;
        }

        [Serializable]
        private struct ProductionGenerationState
        {
            public int ContractVersion;
            public MassArchetype Archetype;
            public int ShapeSeed;
            public int SurfaceSeed;
            public MassScaleStep Size;
            public FormComplexity FormComplexity;
            public SurfaceFacetDensity SurfaceFacetDensity;
            public EdgeCharacter EdgeCharacter;
            public ShapeDiversity ShapeDiversity;
            public GroundingStyle Grounding;
            public LeanStyle Lean;
            public float FineScale;
            public float WidthBias;
            public float HeightBias;
            public float DepthBias;
            public float SurfaceVariation;

            public bool Matches(ProductionGenerationState other)
            {
                return ContractVersion == other.ContractVersion &&
                       Archetype == other.Archetype &&
                       ShapeSeed == other.ShapeSeed &&
                       SurfaceSeed == other.SurfaceSeed &&
                       Size == other.Size &&
                       FormComplexity == other.FormComplexity &&
                       SurfaceFacetDensity == other.SurfaceFacetDensity &&
                       EdgeCharacter == other.EdgeCharacter &&
                       ShapeDiversity == other.ShapeDiversity &&
                       Grounding == other.Grounding &&
                       Lean == other.Lean &&
                       FineScale == other.FineScale &&
                       WidthBias == other.WidthBias &&
                       HeightBias == other.HeightBias &&
                       DepthBias == other.DepthBias &&
                       SurfaceVariation == other.SurfaceVariation;
            }
        }

        [Serializable]
        private struct FeatureAtlasGenerationState
        {
            public int ContractVersion;
            public ProductionGenerationState ProductionState;
            public GeneratedMassFeatureAtlasRequest Request;
            public int Resolution;
            public float EdgeWearAmount;
            public float EdgeWearWidth;
            public float EdgeWearCoverage;
            public float EdgeWearSoftness;
            public float CreaseAmount;
            public float CreaseWidth;
            public float CreaseLength;
            public float CreaseBranching;

            public bool Matches(FeatureAtlasGenerationState other)
            {
                return ContractVersion == other.ContractVersion &&
                       ProductionState.Matches(other.ProductionState) &&
                       Request == other.Request &&
                       Resolution == other.Resolution &&
                       EdgeWearAmount == other.EdgeWearAmount &&
                       EdgeWearWidth == other.EdgeWearWidth &&
                       EdgeWearCoverage == other.EdgeWearCoverage &&
                       EdgeWearSoftness == other.EdgeWearSoftness &&
                       CreaseAmount == other.CreaseAmount &&
                       CreaseWidth == other.CreaseWidth &&
                       CreaseLength == other.CreaseLength &&
                       CreaseBranching == other.CreaseBranching;
            }
        }

        [Serializable]
        private struct RiverInteractionState
        {
            public GeneratedRiverInteractionParticipation Participation;
            public GeneratedRiverFeatureMode PressureMode;
            public float PressureStrength;
            public float PressureContactSharpness;
            public float PressureProfileVariation;
            public float PressureProfileChangeIntervalMin;
            public float PressureProfileChangeIntervalMax;
            public GeneratedRiverFeatureMode WakeMode;
            public float WakeStrength;
            public float WakeReach;
            public float WakeSpread;
            public float WakeVariation;
            public GeneratedRiverRippleCollisionMode ImpactRippleCollisionMode;
            public float FootprintPadding;

            public bool Matches(RiverInteractionState other)
            {
                return Participation == other.Participation &&
                       PressureMode == other.PressureMode &&
                       PressureStrength == other.PressureStrength &&
                       PressureContactSharpness ==
                           other.PressureContactSharpness &&
                       PressureProfileVariation ==
                           other.PressureProfileVariation &&
                       PressureProfileChangeIntervalMin ==
                           other.PressureProfileChangeIntervalMin &&
                       PressureProfileChangeIntervalMax ==
                           other.PressureProfileChangeIntervalMax &&
                       WakeMode == other.WakeMode &&
                       WakeStrength == other.WakeStrength &&
                       WakeReach == other.WakeReach &&
                       WakeSpread == other.WakeSpread &&
                       WakeVariation == other.WakeVariation &&
                       ImpactRippleCollisionMode ==
                           other.ImpactRippleCollisionMode &&
                       FootprintPadding == other.FootprintPadding;
            }
        }

        [SerializeField]
        private MassRecipe recipe = new MassRecipe();

        [SerializeField]
        private bool regenerateOnValidate = true;

        [Header("Generation Budget")]
        [Tooltip("Caps generated-mass feature data cost. Feature use decides whether atlases are generated; this budget caps atlas resolution and future mesh/detail cost.")]
        [SerializeField]
        private GeneratedMassGenerationBudget generationBudget =
            GeneratedMassGenerationBudget.Standard;

        [Tooltip("Atlas resolution used only when Generation Budget is Custom. Values are quantized to 128, 256 or 512.")]
        [Range(128, 512)]
        [SerializeField]
        private int customFeatureAtlasResolution = 256;

        [Tooltip("Editable feature recipe used by the Generated Mass framework. Changing this field does not overwrite current controls; use Apply Selected Feature Recipe or Reset Feature Controls to Recipe in the inspector.")]
        [SerializeField]
        private GeneratedMassFeatureRecipe featureRecipe =
            GeneratedMassFeatureRecipe.GenericTestMass;

        [Tooltip("Chooses a named HLSL stone material profile for this generated mass. Renderer Material leaves the current renderer material untouched.")]
        [SerializeField]
        private StoneSurfaceProfile stoneSurfaceProfile =
            StoneSurfaceProfile.RendererMaterial;

        [Tooltip("Per-object starting stone colour. The shared material can still layer pixel, exposure, crevice, or biome effects over this base.")]
        [SerializeField]
        private Color baseColor =
            new Color(0.33423817f, 0.3410176f, 0.3490566f, 1f);

        [Tooltip("Temporarily visualizes generated material masks on this object. Leave disabled for normal rendering.")]
        [SerializeField]
        private StoneSurfaceMaskDebug surfaceMaskDebug =
            StoneSurfaceMaskDebug.None;

        [Tooltip("Moves the generated lower/contact mask origin upward in normalized local height. Useful for flat slabs or rocks that are intentionally embedded below ground.")]
        [Range(0f, 0.2f)]
        [SerializeField]
        private float surfaceMaskBaseLift;

        [Tooltip("Scales how far the CreviceBase accumulation may crawl upward from the generated base/contact area.")]
        [Range(0.25f, 2f)]
        [SerializeField]
        private float creviceReach = 1f;

        [Tooltip("Controls how gradually the CreviceBase accumulation fades out upward, independently from reach. Higher values create a longer, softer transition.")]
        [Range(0.25f, 2f)]
        [SerializeField]
        private float creviceSmoothness = 1f;

        [Tooltip("Controls how interrupted the CreviceBase accumulation field is. Lower values are smoother; higher values break the field more aggressively.")]
        [Range(0.25f, 2f)]
        [SerializeField]
        private float creviceBreakup = 1f;

        [Tooltip("Scales how far DirtDeposit crawl paths may rise from the generated base/contact area.")]
        [Range(0.25f, 2f)]
        [SerializeField]
        private float dirtCrawlReach = 1f;

        [Tooltip("Controls how much of the DirtDeposit crawl field becomes visible. Lower values are sparse; higher values are fuller.")]
        [Range(0.25f, 2f)]
        [SerializeField]
        private float dirtCoverage = 1f;

        [Tooltip("Scales how strongly the Exposure mask affects the final normal render. 0 disables exposed-plane response; 1 is the current default; 2 exaggerates it for tuning.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float exposureResponse = 1f;

        [Tooltip("Scales how strongly the CreviceBase mask affects the final normal render. 0 disables crevice-specific response; 1 is the current default; 2 exaggerates it for tuning.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float creviceResponse = 1f;

        [Tooltip("Scales how strongly the Base mask affects the final normal render. 0 disables base-contact response; 1 is the current default; 2 exaggerates it for tuning.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float baseResponse = 1f;

        [Tooltip("Scales how strongly the DirtDeposit mask affects the final normal render. 0 disables deposit-specific response; 1 is the current default; 2 exaggerates it for tuning.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float dirtDepositResponse = 1f;

        [Tooltip("Optional hue tint for exposed/generated upper surfaces. Tint Strength at 0 keeps neutral greys neutral.")]
        [SerializeField]
        private Color exposureTint = new Color(0.72f, 0.76f, 0.78f, 1f);

        [Tooltip("How much Exposure Tint affects the final render. Default 0 means no hue shift.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float exposureTintStrength;

        [Tooltip("Optional hue tint for CreviceBase. Tint Strength at 0 keeps crevice as neutral depth/shadow.")]
        [SerializeField]
        private Color creviceTint = new Color(0.5f, 0.5f, 0.5f, 1f);

        [Tooltip("How much Crevice Tint affects the final render. Default 0 means neutral darkening only.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float creviceTintStrength;

        [Tooltip("Optional hue tint for Base/contact grounding. Tint Strength at 0 keeps base response neutral.")]
        [SerializeField]
        private Color baseTint = new Color(0.5f, 0.5f, 0.5f, 1f);

        [Tooltip("How much Base Tint affects the final render. Default 0 means neutral grounding only.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float baseTintStrength;

        [Tooltip("Optional hue tint for DirtDeposit. Tint Strength at 0 keeps dirt/deposit response neutral.")]
        [SerializeField]
        private Color dirtDepositTint = new Color(0.42f, 0.40f, 0.36f, 1f);

        [Tooltip("How much Dirt Deposit Tint affects the final render. Default 0 means no brown/red/green hue shift.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float dirtDepositTintStrength;

        [Tooltip("Optional object-level colour identity for the rock. Tint Strength at 0 keeps the chosen Base Color authoritative.")]
        [SerializeField]
        private Color overallRockTint = new Color(0.5f, 0.5f, 0.5f, 1f);

        [Tooltip("How much Overall Rock Tint affects the post-mask albedo before lighting. Default 0 preserves neutral Base Color.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float overallRockTintStrength;

        [Tooltip("How much RGB colour from URP/PBR lighting is allowed to tint the rock. 0 keeps only light brightness/shadow value, 1 keeps full PBR light colour. Default allows moderate light tint without letting it dominate neutral grey.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float lightingTintInfluence = 0.35f;

        [Tooltip("Convex edge-wear amount. For EW-4 plane-cut masses, this enables generated bevel/chamfer edge wear and controls worn-face material strength, not physical bevel width.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float edgeWearAmount = 1f;

        [Tooltip("Authoritative convex edge-wear bevel/chamfer width for both production candidates and the plane-cut preview. Values below 0.25 unlock a thinner preview range; existing values from 0.25 to 2 keep their previous physical mapping.")]
        [Range(0.05f, 2f)]
        [SerializeField]
        private float edgeWearWidth = 1f;

        [Tooltip("How many eligible convex edges receive generated bevel/chamfer wear.")]
        [Range(0.1f, 2f)]
        [SerializeField]
        private float edgeWearCoverage = 1f;

        [Tooltip("Softens the visible worn-edge material response. EW-4A.1 no longer changes physical bevel width from this control.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float edgeWearSoftness = 0.45f;

        [Tooltip("Master visible intensity for UV2.z-marked generated bevel/chamfer edge-wear faces. Set above zero to see worn-edge response in normal rendering.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float edgeWearResponseStrength;

        [Tooltip("How much the worn convex ridge response brightens the base stone value.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float edgeWearBrightnessLift = 0.25f;

        [Tooltip("Optional material tint target for worn convex ridges. Tint Influence controls how much of this hue is applied.")]
        [SerializeField]
        private Color edgeWearTint = new Color(0.70f, 0.69f, 0.62f, 1f);

        [Tooltip("How strongly Worn Edge Tint affects the edge-wear response. Zero keeps the response value-only.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float edgeWearTintStrength;

        [Tooltip("Reserved for richer inter-edge variation on generated bevel/chamfer wear. The first EW-4 pass uses deterministic edge scoring only.")]
        [Range(0f, 1f)]
        [SerializeField]
        [FormerlySerializedAs("edgeWearBreakup")]
        private float edgeWearMacroVariation;

        [Tooltip("Reserved for future along-edge chipping/segmentation on generated bevel/chamfer wear. The first EW-4 pass does not segment bevel faces.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float edgeWearMicroVariation;

        [Tooltip("Reserved ConcaveCrease amount for the future atlas-based crack/seam feature. Current Patch 14C does not render secondary crease meshes.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float creaseAmount = 1f;

        [Tooltip("Reserved ConcaveCrease width for the future atlas-based crack/seam feature.")]
        [Range(0.25f, 2f)]
        [SerializeField]
        private float creaseWidth = 1f;

        [Tooltip("Scales generated crack path length. Higher values make cracks more connected and less scratch-like.")]
        [Range(0.25f, 2f)]
        [SerializeField]
        private float creaseLength = 1f;

        [Tooltip("Controls how often generated crack paths add short branches.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float creaseBranching = 1f;

        [Tooltip("Reserved ConcaveCrease softness for the future atlas-based crack/seam feature.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float creaseSoftness = 0.35f;

        [SerializeField, HideInInspector]
        private Material coldGreyStoneMaterial;

        [SerializeField, HideInInspector]
        private Material darkWetRiverStoneMaterial;

        [SerializeField, HideInInspector]
        private Material paleFrostStoneMaterial;

        [SerializeField, HideInInspector]
        private Material blackSacredStoneMaterial;

        [Header("River Interaction")]
        [SerializeField]
        private GeneratedRiverInteractionSettings riverInteraction =
            new GeneratedRiverInteractionSettings();

        [SerializeField, HideInInspector]
        private bool recipeInitialized;

        [SerializeField, HideInInspector]
        private MassArchetype lastAppliedArchetype;

        [SerializeField, HideInInspector]
        private GeneratedMassFeatureRecipe lastAppliedFeatureRecipe =
            GeneratedMassFeatureRecipe.GenericTestMass;

        [SerializeField, HideInInspector]
        private bool acceptedProductionStateValid;

        [SerializeField, HideInInspector]
        private ProductionGenerationState acceptedProductionState;

        [SerializeField, HideInInspector]
        private bool acceptedFeatureAtlasStateValid;

        [SerializeField, HideInInspector]
        private FeatureAtlasGenerationState acceptedFeatureAtlasState;

        [SerializeField, HideInInspector]
        private bool acceptedRiverInteractionStateValid;

        [SerializeField, HideInInspector]
        private RiverInteractionState acceptedRiverInteractionState;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;
        private MaterialPropertyBlock materialProperties;
        private Mesh generatedMesh;
        private Texture2D generatedFeatureAtlas0;
        private Texture2D generatedFeatureAtlas1;
        private bool stableWorldGeometryFingerprintValid;
        private GeneratedGeometryStableFingerprint
            stableWorldGeometryFingerprint;
        private Mesh stableWorldGeometryFingerprintMesh;
        private Matrix4x4 stableWorldGeometryFingerprintMatrix;
        private bool regenerationInProgress;
#if UNITY_EDITOR
        [NonSerialized]
        private bool planeCutBevelPreviewEnabled;
        [NonSerialized]
        private bool planeCutBevelPreviewStale;
        [NonSerialized]
        private bool planeCutBevelPreviewApplied;
        [NonSerialized]
        private int planeCutBevelPreviewActiveEdges;
        [NonSerialized]
        private int planeCutBevelPreviewBuiltEdges;
        [NonSerialized]
        private int planeCutBevelPreviewDeferredEdges;
        [NonSerialized]
        private int planeCutBevelPreviewRejectedEdges;
        [NonSerialized]
        private string planeCutBevelPreviewDiagnostic;
        [NonSerialized]
        private bool boundedEdgePreviewEnabled;
        [NonSerialized]
        private bool boundedEdgePreviewStale;
        [NonSerialized]
        private bool boundedEdgePreviewApplied;
        [NonSerialized]
        private int boundedEdgePreviewOrdinal;
        [NonSerialized]
        private int boundedEdgePreviewCandidateCount;
        [NonSerialized]
        private int boundedEdgePreviewSourceEdgeIndex = -1;
        [NonSerialized]
        private int boundedEdgePreviewBevelFaceCount;
        [NonSerialized]
        private int boundedEdgePreviewEndpointCapCount;
        [NonSerialized]
        private int boundedEdgePreviewModifiedSourceFaceCount;
        [NonSerialized]
        private int boundedEdgePreviewForeignSourceFaceModifiedCount;
        [NonSerialized]
        private float boundedEdgePreviewRailDeviation;
        [NonSerialized]
        private float boundedEdgePreviewMaximumExtentBeyondRails;
        [NonSerialized]
        private string boundedEdgePreviewDiagnostic;
        [NonSerialized]
        private bool unifiedEdgeWearPreviewEnabled;
        [NonSerialized]
        private bool unifiedEdgeWearPreviewStale;
        [NonSerialized]
        private bool unifiedEdgeWearPreviewApplied;
        [NonSerialized]
        private int unifiedEdgeWearPreviewCandidateCount;
        [NonSerialized]
        private int unifiedEdgeWearPreviewRailSolvedEdgeCount;
        [NonSerialized]
        private int unifiedEdgeWearPreviewActiveEdgeCount;
        [NonSerialized]
        private int unifiedEdgeWearPreviewDeferredEdgeCount;
        [NonSerialized]
        private int unifiedEdgeWearPreviewRejectedEdgeCount;
        [NonSerialized]
        private int unifiedEdgeWearPreviewBevelFaceCount;
        [NonSerialized]
        private int unifiedEdgeWearPreviewVertexJunctionFaceCount;
        [NonSerialized]
        private int unifiedEdgeWearPreviewTriangleCount;
        [NonSerialized]
        private string unifiedEdgeWearPreviewDiagnostic;
        [NonSerialized]
        private MassGenerator.EdgeWearDebugEdgeRecord[]
            unifiedEdgeWearPreviewDebugEdges =
                Array.Empty<MassGenerator.EdgeWearDebugEdgeRecord>();
#endif

        public event Action GeometryChanged;

        public MassRecipe Recipe => recipe;
        public GeneratedRiverInteractionSettings RiverInteractionSettings
        {
            get
            {
                riverInteraction ??= new GeneratedRiverInteractionSettings();
                return riverInteraction;
            }
        }
        public bool IsSolidGeometry => true;
        public bool IsStaticGeometry => true;
        public GeneratedMassFeatureRecipe FeatureRecipe => featureRecipe;
        public GeneratedMassFeatureRecipe LastAppliedFeatureRecipe =>
            lastAppliedFeatureRecipe;
        public GeneratedMassGenerationBudget GenerationBudget => generationBudget;
        public int CustomFeatureAtlasResolution => customFeatureAtlasResolution;
        public StoneSurfaceProfile StoneSurfaceProfile => stoneSurfaceProfile;
        public StoneSurfaceMaskDebug SurfaceMaskDebug => surfaceMaskDebug;
        public Color BaseColor => baseColor;
        public float SurfaceMaskBaseLift => surfaceMaskBaseLift;
        public float CreviceReach => creviceReach;
        public float CreviceSmoothness => creviceSmoothness;
        public float CreviceBreakup => creviceBreakup;
        public float DirtCrawlReach => dirtCrawlReach;
        public float DirtCoverage => dirtCoverage;
        public float EdgeWearAmount => edgeWearAmount;
        public float EdgeWearWidth => edgeWearWidth;
        public float EdgeWearCoverage => edgeWearCoverage;
        public float EdgeWearSoftness => edgeWearSoftness;
        public float EdgeWearResponseStrength => edgeWearResponseStrength;
        public float EdgeWearBrightnessLift => edgeWearBrightnessLift;
        public Color EdgeWearTint => edgeWearTint;
        public float EdgeWearTintStrength => edgeWearTintStrength;
        public float EdgeWearMacroVariation => edgeWearMacroVariation;
        public float EdgeWearMicroVariation => edgeWearMicroVariation;
        public float CreaseAmount => creaseAmount;
        public float CreaseWidth => creaseWidth;
        public float CreaseLength => creaseLength;
        public float CreaseBranching => creaseBranching;
        public float CreaseSoftness => creaseSoftness;
#if UNITY_EDITOR
        public bool PlaneCutBevelPreviewEnabled =>
            planeCutBevelPreviewEnabled;
        public bool PlaneCutBevelPreviewStale =>
            planeCutBevelPreviewStale;
        public bool PlaneCutBevelPreviewApplied =>
            planeCutBevelPreviewApplied;
        public int PlaneCutBevelPreviewActiveEdges =>
            planeCutBevelPreviewActiveEdges;
        public int PlaneCutBevelPreviewBuiltEdges =>
            planeCutBevelPreviewBuiltEdges;
        public int PlaneCutBevelPreviewDeferredEdges =>
            planeCutBevelPreviewDeferredEdges;
        public int PlaneCutBevelPreviewRejectedEdges =>
            planeCutBevelPreviewRejectedEdges;
        public string PlaneCutBevelPreviewDiagnostic =>
            planeCutBevelPreviewDiagnostic ?? string.Empty;
        public bool BoundedEdgePreviewEnabled =>
            boundedEdgePreviewEnabled;
        public bool BoundedEdgePreviewStale =>
            boundedEdgePreviewStale;
        public bool BoundedEdgePreviewApplied =>
            boundedEdgePreviewApplied;
        public int BoundedEdgePreviewOrdinal =>
            boundedEdgePreviewOrdinal;
        public int BoundedEdgePreviewCandidateCount =>
            boundedEdgePreviewCandidateCount;
        public int BoundedEdgePreviewSourceEdgeIndex =>
            boundedEdgePreviewSourceEdgeIndex;
        public int BoundedEdgePreviewBevelFaceCount =>
            boundedEdgePreviewBevelFaceCount;
        public int BoundedEdgePreviewEndpointCapCount =>
            boundedEdgePreviewEndpointCapCount;
        public int BoundedEdgePreviewModifiedSourceFaceCount =>
            boundedEdgePreviewModifiedSourceFaceCount;
        public int BoundedEdgePreviewForeignSourceFaceModifiedCount =>
            boundedEdgePreviewForeignSourceFaceModifiedCount;
        public float BoundedEdgePreviewRailDeviation =>
            boundedEdgePreviewRailDeviation;
        public float BoundedEdgePreviewMaximumExtentBeyondRails =>
            boundedEdgePreviewMaximumExtentBeyondRails;
        public string BoundedEdgePreviewDiagnostic =>
            boundedEdgePreviewDiagnostic ?? string.Empty;

        public bool UnifiedEdgeWearPreviewEnabled =>
            unifiedEdgeWearPreviewEnabled;
        public bool UnifiedEdgeWearPreviewStale =>
            unifiedEdgeWearPreviewStale;
        public bool UnifiedEdgeWearPreviewApplied =>
            unifiedEdgeWearPreviewApplied;
        public int UnifiedEdgeWearPreviewCandidateCount =>
            unifiedEdgeWearPreviewCandidateCount;
        public int UnifiedEdgeWearPreviewRailSolvedEdgeCount =>
            unifiedEdgeWearPreviewRailSolvedEdgeCount;
        public int UnifiedEdgeWearPreviewActiveEdgeCount =>
            unifiedEdgeWearPreviewActiveEdgeCount;
        public int UnifiedEdgeWearPreviewDeferredEdgeCount =>
            unifiedEdgeWearPreviewDeferredEdgeCount;
        public int UnifiedEdgeWearPreviewRejectedEdgeCount =>
            unifiedEdgeWearPreviewRejectedEdgeCount;
        public int UnifiedEdgeWearPreviewBevelFaceCount =>
            unifiedEdgeWearPreviewBevelFaceCount;
        public int UnifiedEdgeWearPreviewVertexJunctionFaceCount =>
            unifiedEdgeWearPreviewVertexJunctionFaceCount;
        public int UnifiedEdgeWearPreviewTriangleCount =>
            unifiedEdgeWearPreviewTriangleCount;
        public string UnifiedEdgeWearPreviewDiagnostic =>
            unifiedEdgeWearPreviewDiagnostic ?? string.Empty;
        public MassGenerator.EdgeWearDebugEdgeRecord[]
            UnifiedEdgeWearPreviewDebugEdges =>
                unifiedEdgeWearPreviewDebugEdges ??
                    Array.Empty<MassGenerator.EdgeWearDebugEdgeRecord>();

        public void EvaluateUnifiedEdgeWearPreview()
        {
            if (Application.isPlaying || regenerationInProgress)
            {
                return;
            }

            regenerationInProgress = true;
            planeCutBevelPreviewEnabled = false;
            planeCutBevelPreviewStale = false;
            ClearPlaneCutBevelPreviewStatus();
            boundedEdgePreviewEnabled = false;
            boundedEdgePreviewStale = false;
            ClearBoundedEdgePreviewStatus();
            unifiedEdgeWearPreviewEnabled = true;
            unifiedEdgeWearPreviewStale = false;
            ClearUnifiedEdgeWearPreviewStatus();
            try
            {
                RegenerateInternal(PreviewGenerationMode.UnifiedEdgeWear);
            }
            catch
            {
                unifiedEdgeWearPreviewStale = true;
                throw;
            }
            finally
            {
                regenerationInProgress = false;
            }
        }

        public void EvaluatePlaneCutBevelPreview()
        {
            if (Application.isPlaying || regenerationInProgress)
            {
                return;
            }

            regenerationInProgress = true;
            boundedEdgePreviewEnabled = false;
            boundedEdgePreviewStale = false;
            ClearBoundedEdgePreviewStatus();
            unifiedEdgeWearPreviewEnabled = false;
            unifiedEdgeWearPreviewStale = false;
            ClearUnifiedEdgeWearPreviewStatus();
            planeCutBevelPreviewEnabled = true;
            planeCutBevelPreviewStale = false;
            ClearPlaneCutBevelPreviewStatus();
            try
            {
                RegenerateInternal(PreviewGenerationMode.PlaneCut);
            }
            catch
            {
                planeCutBevelPreviewStale = true;
                throw;
            }
            finally
            {
                regenerationInProgress = false;
            }
        }

        public void RefreshPlaneCutBevelPreview()
        {
            EvaluatePlaneCutBevelPreview();
        }

        public void EvaluateBoundedEdgePreview()
        {
            EvaluateBoundedEdgePreviewAtOrdinal(
                boundedEdgePreviewOrdinal);
        }

        public void PreviousBoundedEdgePreview()
        {
            int count = Mathf.Max(1, boundedEdgePreviewCandidateCount);
            int next = boundedEdgePreviewOrdinal - 1;
            if (next < 0)
            {
                next = count - 1;
            }
            EvaluateBoundedEdgePreviewAtOrdinal(next);
        }

        public void NextBoundedEdgePreview()
        {
            int count = Mathf.Max(1, boundedEdgePreviewCandidateCount);
            int next = (boundedEdgePreviewOrdinal + 1) % count;
            EvaluateBoundedEdgePreviewAtOrdinal(next);
        }

        private void EvaluateBoundedEdgePreviewAtOrdinal(int ordinal)
        {
            if (Application.isPlaying || regenerationInProgress)
            {
                return;
            }

            regenerationInProgress = true;
            planeCutBevelPreviewEnabled = false;
            planeCutBevelPreviewStale = false;
            ClearPlaneCutBevelPreviewStatus();
            unifiedEdgeWearPreviewEnabled = false;
            unifiedEdgeWearPreviewStale = false;
            ClearUnifiedEdgeWearPreviewStatus();
            boundedEdgePreviewEnabled = true;
            boundedEdgePreviewStale = false;
            boundedEdgePreviewOrdinal = Mathf.Max(0, ordinal);
            ClearBoundedEdgePreviewStatus(preserveOrdinal: true);
            try
            {
                RegenerateInternal(
                    PreviewGenerationMode.BoundedSingleEdge);
            }
            catch
            {
                boundedEdgePreviewStale = true;
                throw;
            }
            finally
            {
                regenerationInProgress = false;
            }
        }

        public void RunLegacyEdgeWearDiagnosticAudit()
        {
            if (Application.isPlaying || regenerationInProgress)
            {
                return;
            }

            regenerationInProgress = true;
            try
            {
                EnsureRecipeState();
                MassGenerator.RunLegacyEdgeWearDiagnosticAudit(
                    recipe,
                    CreateSurfaceFeatureSettings());
            }
            finally
            {
                regenerationInProgress = false;
            }
        }

        public void ShowProductionGeometry()
        {
            if (regenerationInProgress)
            {
                return;
            }

            planeCutBevelPreviewEnabled = false;
            planeCutBevelPreviewStale = false;
            ClearPlaneCutBevelPreviewStatus();
            boundedEdgePreviewEnabled = false;
            boundedEdgePreviewStale = false;
            ClearBoundedEdgePreviewStatus();
            unifiedEdgeWearPreviewEnabled = false;
            unifiedEdgeWearPreviewStale = false;
            ClearUnifiedEdgeWearPreviewStatus();
            Regenerate();
        }

        private void ClearPlaneCutBevelPreviewStatus()
        {
            planeCutBevelPreviewApplied = false;
            planeCutBevelPreviewActiveEdges = 0;
            planeCutBevelPreviewBuiltEdges = 0;
            planeCutBevelPreviewDeferredEdges = 0;
            planeCutBevelPreviewRejectedEdges = 0;
            planeCutBevelPreviewDiagnostic = string.Empty;
        }

        private void ClearUnifiedEdgeWearPreviewStatus()
        {
            unifiedEdgeWearPreviewApplied = false;
            unifiedEdgeWearPreviewCandidateCount = 0;
            unifiedEdgeWearPreviewRailSolvedEdgeCount = 0;
            unifiedEdgeWearPreviewActiveEdgeCount = 0;
            unifiedEdgeWearPreviewDeferredEdgeCount = 0;
            unifiedEdgeWearPreviewRejectedEdgeCount = 0;
            unifiedEdgeWearPreviewBevelFaceCount = 0;
            unifiedEdgeWearPreviewVertexJunctionFaceCount = 0;
            unifiedEdgeWearPreviewTriangleCount = 0;
            unifiedEdgeWearPreviewDiagnostic = string.Empty;
            unifiedEdgeWearPreviewDebugEdges =
                Array.Empty<MassGenerator.EdgeWearDebugEdgeRecord>();
        }

        private void ClearBoundedEdgePreviewStatus(
            bool preserveOrdinal = false)
        {
            boundedEdgePreviewApplied = false;
            if (!preserveOrdinal)
            {
                boundedEdgePreviewOrdinal = 0;
            }
            boundedEdgePreviewCandidateCount = 0;
            boundedEdgePreviewSourceEdgeIndex = -1;
            boundedEdgePreviewBevelFaceCount = 0;
            boundedEdgePreviewEndpointCapCount = 0;
            boundedEdgePreviewModifiedSourceFaceCount = 0;
            boundedEdgePreviewForeignSourceFaceModifiedCount = 0;
            boundedEdgePreviewRailDeviation = 0f;
            boundedEdgePreviewMaximumExtentBeyondRails = 0f;
            boundedEdgePreviewDiagnostic = string.Empty;
        }
#endif

        public MeshFilter GeometryMeshFilter
        {
            get
            {
                CacheComponents();
                return meshFilter;
            }
        }

        public bool TryGetStableWorldGeometryFingerprint(
            out GeneratedGeometryStableFingerprint fingerprint,
            out string status)
        {
            CacheComponents();
            Mesh currentMesh = meshFilter != null
                ? meshFilter.sharedMesh
                : null;
            Matrix4x4 currentMatrix = meshFilter != null
                ? meshFilter.transform.localToWorldMatrix
                : Matrix4x4.identity;

            if (!stableWorldGeometryFingerprintValid ||
                stableWorldGeometryFingerprintMesh != currentMesh ||
                !stableWorldGeometryFingerprintMatrix.Equals(currentMatrix))
            {
                RefreshStableWorldGeometryFingerprint();
            }

            fingerprint = stableWorldGeometryFingerprint;
            status = stableWorldGeometryFingerprintValid
                ? "Using the generated owner's exact world-geometry fingerprint."
                : "The generated owner could not prepare an exact " +
                  "world-geometry fingerprint.";
            return stableWorldGeometryFingerprintValid;
        }

        private void OnEnable()
        {
            EnsureRecipeState();
            CacheComponents();
            SynchronizeGeneratedState(
                forceRegeneration: false,
                allowAutomaticRegeneration: true);
            GeneratedGeometryRegistry.Register(this);
        }

        private void OnDisable()
        {
            GeneratedGeometryRegistry.Unregister(this);
        }

        private void OnValidate()
        {
            EnsureRecipeState();
            RemoveLegacyRiverFoamProxy();
            RemoveLegacySurfaceFeatureObjects();
#if UNITY_EDITOR
            if (!regenerationInProgress)
            {
                if (planeCutBevelPreviewEnabled)
                {
                    planeCutBevelPreviewStale = true;
                }
                if (boundedEdgePreviewEnabled)
                {
                    boundedEdgePreviewStale = true;
                }
                if (unifiedEdgeWearPreviewEnabled)
                {
                    unifiedEdgeWearPreviewStale = true;
                }
            }
#endif

            CacheComponents();
            SynchronizeGeneratedState(
                forceRegeneration: false,
                allowAutomaticRegeneration: regenerateOnValidate);
        }

        private void EnsureRecipeState()
        {
            if (recipe == null)
            {
                recipe = new MassRecipe();
            }

            riverInteraction ??= new GeneratedRiverInteractionSettings();
            riverInteraction.Validate();
            customFeatureAtlasResolution = QuantizeFeatureAtlasResolution(
                customFeatureAtlasResolution);

            bool archetypeChanged =
                recipeInitialized &&
                recipe.Archetype != lastAppliedArchetype;

            if (!recipeInitialized || archetypeChanged)
            {
                recipe.ApplyArchetypeDefaults();
                lastAppliedArchetype = recipe.Archetype;
                recipeInitialized = true;
            }
        }

        [ContextMenu("Regenerate Mass")]
        public void Regenerate()
        {
            SynchronizeGeneratedState(
                forceRegeneration: true,
                allowAutomaticRegeneration: true);
        }

        private void SynchronizeGeneratedState(
            bool forceRegeneration,
            bool allowAutomaticRegeneration)
        {
            if (regenerationInProgress)
            {
                return;
            }

            using (SynchronizeProfilerMarker.Auto())
            {
                regenerationInProgress = true;
                try
                {
                    CacheComponents();
                    RemoveLegacySurfaceFeatureObjects();

                    ProductionGenerationState productionState =
                        CaptureProductionGenerationState();
                    FeatureAtlasGenerationState featureAtlasState =
                        CaptureFeatureAtlasGenerationState(productionState);
                    RiverInteractionState riverInteractionState =
                        CaptureRiverInteractionState();

                    bool riverInteractionChanged =
                        acceptedRiverInteractionStateValid &&
                        !acceptedRiverInteractionState.Matches(
                            riverInteractionState);

                    bool reusableProductionMesh =
                        !forceRegeneration &&
                        acceptedProductionStateValid &&
                        acceptedProductionState.Matches(productionState) &&
                        TryAdoptReusableProductionMesh();

                    bool productionRegenerated = false;
                    if (forceRegeneration ||
                        (!reusableProductionMesh &&
                         allowAutomaticRegeneration))
                    {
                        PrepareForProductionRegeneration();
                        using (GenerateProductionProfilerMarker.Auto())
                        {
                            RegenerateInternal(
                                PreviewGenerationMode.Production);
                        }

                        acceptedProductionState = productionState;
                        acceptedProductionStateValid = true;
                        acceptedFeatureAtlasState = featureAtlasState;
                        acceptedFeatureAtlasStateValid =
                            IsFeatureAtlasStateSatisfied(featureAtlasState);
                        productionRegenerated = true;
                    }
                    else
                    {
                        if (reusableProductionMesh &&
                            allowAutomaticRegeneration)
                        {
                            SynchronizeFeatureAtlas(featureAtlasState);
                        }

                        ApplyMaterialProperties();
                        RemoveLegacyRiverFoamProxy();
                    }

                    if (!productionRegenerated &&
                        riverInteractionChanged)
                    {
                        NotifyGeometryChanged();
                    }

                    acceptedRiverInteractionState =
                        riverInteractionState;
                    acceptedRiverInteractionStateValid = true;
                }
                finally
                {
                    regenerationInProgress = false;
                }
            }
        }

        private void PrepareForProductionRegeneration()
        {
#if UNITY_EDITOR
            if (planeCutBevelPreviewEnabled)
            {
                planeCutBevelPreviewStale = true;
                planeCutBevelPreviewApplied = false;
            }
            else
            {
                ClearPlaneCutBevelPreviewStatus();
            }

            if (boundedEdgePreviewEnabled)
            {
                boundedEdgePreviewStale = true;
                boundedEdgePreviewApplied = false;
            }
            else
            {
                ClearBoundedEdgePreviewStatus();
            }

            if (unifiedEdgeWearPreviewEnabled)
            {
                unifiedEdgeWearPreviewStale = true;
                unifiedEdgeWearPreviewApplied = false;
            }
            else
            {
                ClearUnifiedEdgeWearPreviewStatus();
            }
#endif
        }

        private ProductionGenerationState CaptureProductionGenerationState()
        {
            if (recipe == null)
            {
                return default;
            }

            return new ProductionGenerationState
            {
                ContractVersion = ProductionGenerationContractVersion,
                Archetype = recipe.Archetype,
                ShapeSeed = recipe.ShapeSeed,
                SurfaceSeed = recipe.SurfaceSeed,
                Size = recipe.Size,
                FormComplexity = recipe.FormComplexity,
                SurfaceFacetDensity = recipe.SurfaceFacetDensity,
                EdgeCharacter = recipe.EdgeCharacter,
                ShapeDiversity = recipe.ShapeDiversity,
                Grounding = recipe.Grounding,
                Lean = recipe.Lean,
                FineScale = recipe.FineScale,
                WidthBias = recipe.WidthBias,
                HeightBias = recipe.HeightBias,
                DepthBias = recipe.DepthBias,
                SurfaceVariation = recipe.SurfaceVariation
            };
        }

        private FeatureAtlasGenerationState
            CaptureFeatureAtlasGenerationState(
                ProductionGenerationState productionState)
        {
            MassSurfaceFeatureSettings featureSettings =
                CreateSurfaceFeatureSettings();
            GeneratedMassFeatureAtlasRequest request =
                ResolveFeatureAtlasRequest();

            return new FeatureAtlasGenerationState
            {
                ContractVersion = FeatureAtlasGenerationContractVersion,
                ProductionState = productionState,
                Request = request,
                Resolution = ResolveFeatureAtlasResolution(request),
                EdgeWearAmount = featureSettings.EdgeWearAmount,
                EdgeWearWidth = featureSettings.EdgeWearWidth,
                EdgeWearCoverage = featureSettings.EdgeWearCoverage,
                EdgeWearSoftness = featureSettings.EdgeWearSoftness,
                CreaseAmount = featureSettings.CreaseAmount,
                CreaseWidth = featureSettings.CreaseWidth,
                CreaseLength = featureSettings.CreaseLength,
                CreaseBranching = featureSettings.CreaseBranching
            };
        }

        private RiverInteractionState CaptureRiverInteractionState()
        {
            GeneratedRiverInteractionSettings settings =
                RiverInteractionSettings;
            return new RiverInteractionState
            {
                Participation = settings.Participation,
                PressureMode = settings.PressureMode,
                PressureStrength = settings.PressureStrength,
                PressureContactSharpness =
                    settings.PressureContactSharpness,
                PressureProfileVariation =
                    settings.PressureProfileVariation,
                PressureProfileChangeIntervalMin =
                    settings.PressureProfileChangeIntervalMin,
                PressureProfileChangeIntervalMax =
                    settings.PressureProfileChangeIntervalMax,
                WakeMode = settings.WakeMode,
                WakeStrength = settings.WakeStrength,
                WakeReach = settings.WakeReach,
                WakeSpread = settings.WakeSpread,
                WakeVariation = settings.WakeVariation,
                ImpactRippleCollisionMode =
                    settings.ImpactRippleCollisionMode,
                FootprintPadding = settings.FootprintPadding
            };
        }

        private bool TryAdoptReusableProductionMesh()
        {
            Mesh candidate = meshFilter != null
                ? meshFilter.sharedMesh
                : null;
            if (candidate == null)
            {
                candidate = generatedMesh;
            }

            if (!IsReusableProductionMesh(candidate))
            {
                return false;
            }

            generatedMesh = candidate;
            generatedMesh.hideFlags = HideFlags.DontSave;
            BindGeneratedMeshToComponents(forceColliderRecook: false);
            return true;
        }

        private bool IsReusableProductionMesh(Mesh candidate)
        {
            if (candidate == null ||
                recipe == null ||
                !string.Equals(
                    candidate.name,
                    BuildProductionMeshName(),
                    StringComparison.Ordinal) ||
                candidate.vertexCount < 3 ||
                candidate.subMeshCount < 1 ||
                candidate.GetTopology(0) != MeshTopology.Triangles ||
                candidate.GetIndexCount(0) < 3)
            {
                return false;
            }

            return true;
        }

        private string BuildProductionMeshName()
        {
            return recipe == null
                ? "GeneratedMass_Temporary"
                : $"GeneratedMass_{recipe.Archetype}_" +
                  $"Shape{recipe.ShapeSeed}_Surface{recipe.SurfaceSeed}";
        }

        private void SynchronizeFeatureAtlas(
            FeatureAtlasGenerationState currentState)
        {
            bool stateMatches =
                acceptedFeatureAtlasStateValid &&
                acceptedFeatureAtlasState.Matches(currentState);
            if (stateMatches &&
                IsFeatureAtlasStateSatisfied(currentState))
            {
                return;
            }

            if (currentState.Request ==
                GeneratedMassFeatureAtlasRequest.None)
            {
                ReleaseGeneratedFeatureAtlas();
                acceptedFeatureAtlasState = currentState;
                acceptedFeatureAtlasStateValid = true;
                return;
            }

            if (!IsReusableProductionMesh(generatedMesh))
            {
                acceptedFeatureAtlasStateValid = false;
                return;
            }

            MassSurfaceFeatureSettings featureSettings =
                CreateSurfaceFeatureSettings();
            MeshData sourceMeshData = MassGenerator.Generate(
                recipe,
                featureSettings);
            GeneratedMassFeatureAtlasBaker.Result featureAtlas =
                GeneratedMassFeatureAtlasBaker.Bake(
                    sourceMeshData,
                    featureSettings,
                    currentState.Resolution,
                    currentState.Request);

            if (TryApplyFeatureAtlasResult(featureAtlas))
            {
                acceptedFeatureAtlasState = currentState;
                acceptedFeatureAtlasStateValid =
                    IsFeatureAtlasStateSatisfied(currentState);
            }
            else
            {
                acceptedFeatureAtlasStateValid = false;
            }
        }

        private bool TryApplyFeatureAtlasResult(
            GeneratedMassFeatureAtlasBaker.Result featureAtlas)
        {
            if (featureAtlas == null ||
                featureAtlas.FeatureAtlasUV == null ||
                generatedMesh == null ||
                featureAtlas.FeatureAtlasUV.Count !=
                    generatedMesh.vertexCount)
            {
                if (featureAtlas != null)
                {
                    DestroyGeneratedTexture(featureAtlas.Atlas0);
                    DestroyGeneratedTexture(featureAtlas.Atlas1);
                }

                return false;
            }

            ReleaseGeneratedFeatureAtlas();
            generatedFeatureAtlas0 = featureAtlas.Atlas0;
            generatedFeatureAtlas1 = featureAtlas.Atlas1;
            generatedMesh.SetUVs(3, featureAtlas.FeatureAtlasUV);
            return true;
        }

        private bool IsFeatureAtlasStateSatisfied(
            FeatureAtlasGenerationState state)
        {
            bool requiresAtlas0 =
                (state.Request &
                 GeneratedMassFeatureAtlasRequest.FeatureAtlas0) != 0;
            bool requiresAtlas1 =
                (state.Request &
                 GeneratedMassFeatureAtlasRequest.FeatureAtlas1) != 0;

            if (!requiresAtlas0 && generatedFeatureAtlas0 != null)
            {
                return false;
            }

            if (!requiresAtlas1 && generatedFeatureAtlas1 != null)
            {
                return false;
            }

            if (requiresAtlas0 &&
                (generatedFeatureAtlas0 == null ||
                 generatedFeatureAtlas0.width != state.Resolution ||
                 generatedFeatureAtlas0.height != state.Resolution))
            {
                return false;
            }

            if (requiresAtlas1 &&
                (generatedFeatureAtlas1 == null ||
                 generatedFeatureAtlas1.width != state.Resolution ||
                 generatedFeatureAtlas1.height != state.Resolution))
            {
                return false;
            }

            return true;
        }

        private void BindGeneratedMeshToComponents(
            bool forceColliderRecook)
        {
            if (meshFilter != null &&
                meshFilter.sharedMesh != generatedMesh)
            {
                meshFilter.sharedMesh = generatedMesh;
            }

            if (meshCollider == null)
            {
                return;
            }

            using (BindColliderProfilerMarker.Auto())
            {
                if (forceColliderRecook)
                {
                    meshCollider.sharedMesh = null;
                    meshCollider.sharedMesh = generatedMesh;
                }
                else if (meshCollider.sharedMesh != generatedMesh)
                {
                    meshCollider.sharedMesh = generatedMesh;
                }

                meshCollider.convex = false;
            }
        }

        private void RegenerateInternal(PreviewGenerationMode previewMode)
        {
            CacheComponents();
            RemoveLegacySurfaceFeatureObjects();

            if (recipe == null)
            {
                ReleaseGeneratedFeatureAtlas();
                ClearGeneratedAssignments();
                acceptedProductionStateValid = false;
                acceptedFeatureAtlasStateValid = false;
                ApplyMaterialProperties();
                InvalidateStableWorldGeometryFingerprint();
                NotifyGeometryChanged();
                return;
            }

            EnsureGeneratedMesh();
            ReleaseGeneratedFeatureAtlas();

            MassSurfaceFeatureSettings featureSettings =
                CreateSurfaceFeatureSettings();
#if UNITY_EDITOR
            MassGenerator.PlaneCutBevelPreviewStatus previewStatus = default;
            MassGenerator.BoundedEdgePreviewStatus boundedStatus = default;
            MassGenerator.UnifiedEdgeWearPreviewStatus unifiedStatus = default;
            MeshData sourceMeshData;
            if (previewMode == PreviewGenerationMode.PlaneCut)
            {
                sourceMeshData = MassGenerator.GeneratePlaneCutBevelPreview(
                    recipe,
                    featureSettings,
                    out previewStatus);
                planeCutBevelPreviewApplied =
                    previewStatus.PreviewApplied;
                planeCutBevelPreviewActiveEdges =
                    previewStatus.ActiveEdgeCount;
                planeCutBevelPreviewBuiltEdges =
                    previewStatus.BuiltEdgeCount;
                planeCutBevelPreviewDeferredEdges =
                    previewStatus.DeferredEdgeCount;
                planeCutBevelPreviewRejectedEdges =
                    previewStatus.RejectedEdgeCount;
                planeCutBevelPreviewDiagnostic = previewStatus.Diagnostic;
                planeCutBevelPreviewStale = false;
            }
            else if (previewMode ==
                PreviewGenerationMode.BoundedSingleEdge)
            {
                sourceMeshData =
                    MassGenerator.GenerateBoundedSingleEdgeBevelPreview(
                        recipe,
                        featureSettings,
                        boundedEdgePreviewOrdinal,
                        out boundedStatus);
                boundedEdgePreviewApplied = boundedStatus.PreviewApplied;
                boundedEdgePreviewCandidateCount =
                    boundedStatus.CandidateCount;
                boundedEdgePreviewOrdinal = Mathf.Max(
                    0,
                    boundedStatus.SelectedOrdinal);
                boundedEdgePreviewSourceEdgeIndex =
                    boundedStatus.SourceEdgeIndex;
                boundedEdgePreviewBevelFaceCount =
                    boundedStatus.BevelFaceCount;
                boundedEdgePreviewEndpointCapCount =
                    boundedStatus.EndpointCapCount;
                boundedEdgePreviewModifiedSourceFaceCount =
                    boundedStatus.ModifiedSourceFaceCount;
                boundedEdgePreviewForeignSourceFaceModifiedCount =
                    boundedStatus.ForeignSourceFaceModifiedCount;
                boundedEdgePreviewRailDeviation =
                    boundedStatus.RailDeviation;
                boundedEdgePreviewMaximumExtentBeyondRails =
                    boundedStatus.MaximumExtentBeyondRails;
                boundedEdgePreviewDiagnostic = boundedStatus.Diagnostic;
                boundedEdgePreviewStale = false;
            }
            else if (previewMode == PreviewGenerationMode.UnifiedEdgeWear)
            {
                sourceMeshData =
                    MassGenerator.GenerateUnifiedEdgeWearPreview(
                        recipe,
                        featureSettings,
                        out unifiedStatus);
                unifiedEdgeWearPreviewApplied =
                    unifiedStatus.PreviewApplied;
                unifiedEdgeWearPreviewCandidateCount =
                    unifiedStatus.CandidateCount;
                unifiedEdgeWearPreviewRailSolvedEdgeCount =
                    unifiedStatus.RailSolvedEdgeCount;
                unifiedEdgeWearPreviewActiveEdgeCount =
                    unifiedStatus.ActiveEdgeCount;
                unifiedEdgeWearPreviewDeferredEdgeCount =
                    unifiedStatus.DeferredEdgeCount;
                unifiedEdgeWearPreviewRejectedEdgeCount =
                    unifiedStatus.RejectedEdgeCount;
                unifiedEdgeWearPreviewBevelFaceCount =
                    unifiedStatus.BevelFaceCount;
                unifiedEdgeWearPreviewVertexJunctionFaceCount =
                    unifiedStatus.VertexJunctionFaceCount;
                unifiedEdgeWearPreviewTriangleCount =
                    unifiedStatus.TriangleCount;
                unifiedEdgeWearPreviewDiagnostic =
                    unifiedStatus.Diagnostic;
                unifiedEdgeWearPreviewDebugEdges =
                    unifiedStatus.DebugEdges ??
                        Array.Empty<
                            MassGenerator.EdgeWearDebugEdgeRecord>();
                unifiedEdgeWearPreviewStale = false;
            }
            else
            {
                sourceMeshData = MassGenerator.Generate(
                    recipe,
                    featureSettings);
            }
#else
            MeshData sourceMeshData = MassGenerator.Generate(
                recipe,
                featureSettings);
#endif
            GeneratedMassFeatureAtlasRequest atlasRequest =
                ResolveFeatureAtlasRequest();
            int featureAtlasResolution =
                ResolveFeatureAtlasResolution(atlasRequest);
            GeneratedMassFeatureAtlasBaker.Result featureAtlas =
                atlasRequest != GeneratedMassFeatureAtlasRequest.None
                    ? GeneratedMassFeatureAtlasBaker.Bake(
                        sourceMeshData,
                        featureSettings,
                        featureAtlasResolution,
                        atlasRequest)
                    : null;

            string meshName = BuildProductionMeshName();
#if UNITY_EDITOR
            if (previewMode == PreviewGenerationMode.PlaneCut &&
                planeCutBevelPreviewApplied)
            {
                meshName += PlaneCutPreviewMeshNameSuffix;
            }
            else if (previewMode ==
                    PreviewGenerationMode.BoundedSingleEdge &&
                boundedEdgePreviewApplied)
            {
                meshName += BoundedEdgePreviewMeshNameSuffix;
            }
            else if (previewMode ==
                    PreviewGenerationMode.UnifiedEdgeWear &&
                unifiedEdgeWearPreviewApplied)
            {
                meshName += UnifiedEdgeWearPreviewMeshNameSuffix;
            }
#endif

            MeshBuilder.ApplyToMesh(
                sourceMeshData,
                generatedMesh,
                meshName);

            if (featureAtlas != null)
            {
                TryApplyFeatureAtlasResult(featureAtlas);
            }

            BindGeneratedMeshToComponents(forceColliderRecook: true);

            ApplyMaterialProperties();
            InvalidateStableWorldGeometryFingerprint();
            RemoveLegacyRiverFoamProxy();
            NotifyGeometryChanged();
        }

        private void RefreshStableWorldGeometryFingerprint()
        {
            using (ComputeFingerprintProfilerMarker.Auto())
            {
                stableWorldGeometryFingerprintValid =
                    GeneratedGeometryStableFingerprintUtility
                        .TryComputeExactWorldTriangleFingerprint(
                            meshFilter,
                            out stableWorldGeometryFingerprint,
                            out _);
                stableWorldGeometryFingerprintMesh = meshFilter != null
                    ? meshFilter.sharedMesh
                    : null;
                stableWorldGeometryFingerprintMatrix = meshFilter != null
                    ? meshFilter.transform.localToWorldMatrix
                    : Matrix4x4.identity;
            }
        }

        private void InvalidateStableWorldGeometryFingerprint()
        {
            stableWorldGeometryFingerprintValid = false;
            stableWorldGeometryFingerprint = default;
            stableWorldGeometryFingerprintMesh = null;
            stableWorldGeometryFingerprintMatrix = Matrix4x4.identity;
        }

        [ContextMenu("New Shape")]
        public void CreateNewShape()
        {
            recipe.SetShapeSeed(
                GenerateDifferentSeed(recipe.ShapeSeed));

            Regenerate();
        }

        [ContextMenu("New Surface")]
        public void CreateNewSurface()
        {
            recipe.SetSurfaceSeed(
                GenerateDifferentSeed(recipe.SurfaceSeed));

            Regenerate();
        }

        [ContextMenu("New Variant")]
        public void CreateNewVariant()
        {
            recipe.SetShapeSeed(
                GenerateDifferentSeed(recipe.ShapeSeed));

            recipe.SetSurfaceSeed(
                GenerateDifferentSeed(recipe.SurfaceSeed));

            Regenerate();
        }

        [ContextMenu("Reset Recipe to Archetype")]
        public void ResetRecipeToArchetype()
        {
            recipe.ApplyArchetypeDefaults();
            lastAppliedArchetype = recipe.Archetype;
            recipeInitialized = true;
            Regenerate();
        }

        [ContextMenu("Apply Selected Feature Recipe")]
        public void ApplySelectedFeatureRecipe()
        {
            ApplyFeatureRecipe(featureRecipe);
        }

        [ContextMenu("Reset Feature Controls to Selected Recipe")]
        public void ResetFeatureControlsToSelectedRecipe()
        {
            ApplyFeatureRecipe(featureRecipe);
        }

        public bool CurrentFeatureControlsMatchSelectedRecipe()
        {
            return CurrentFeatureControlsMatchRecipe(featureRecipe);
        }

        public bool CurrentFeatureControlsMatchRecipe(
            GeneratedMassFeatureRecipe selectedRecipe)
        {
            if (selectedRecipe == GeneratedMassFeatureRecipe.Custom)
            {
                return false;
            }

            FeatureRecipeValues values =
                ResolveFeatureRecipeValues(selectedRecipe);
            return FeatureControlsMatch(values);
        }

        private void ApplyFeatureRecipe(
            GeneratedMassFeatureRecipe selectedRecipe)
        {
            featureRecipe = selectedRecipe;

            if (selectedRecipe == GeneratedMassFeatureRecipe.Custom)
            {
                ApplyMaterialProperties();
                return;
            }

            FeatureRecipeValues values =
                ResolveFeatureRecipeValues(selectedRecipe);
            ApplyFeatureRecipeValues(values);
            lastAppliedFeatureRecipe = selectedRecipe;
            SynchronizeGeneratedState(
                forceRegeneration: false,
                allowAutomaticRegeneration: true);
        }

        private void ApplyFeatureRecipeValues(FeatureRecipeValues values)
        {
            stoneSurfaceProfile = values.StoneSurfaceProfile;
            baseColor = values.BaseColor;
            surfaceMaskDebug = values.SurfaceMaskDebug;
            surfaceMaskBaseLift = values.SurfaceMaskBaseLift;
            creviceReach = values.CreviceReach;
            creviceSmoothness = values.CreviceSmoothness;
            creviceBreakup = values.CreviceBreakup;
            dirtCrawlReach = values.DirtCrawlReach;
            dirtCoverage = values.DirtCoverage;
            exposureResponse = values.ExposureResponse;
            creviceResponse = values.CreviceResponse;
            baseResponse = values.BaseResponse;
            dirtDepositResponse = values.DirtDepositResponse;
            exposureTint = values.ExposureTint;
            exposureTintStrength = values.ExposureTintStrength;
            creviceTint = values.CreviceTint;
            creviceTintStrength = values.CreviceTintStrength;
            baseTint = values.BaseTint;
            baseTintStrength = values.BaseTintStrength;
            dirtDepositTint = values.DirtDepositTint;
            dirtDepositTintStrength = values.DirtDepositTintStrength;
            overallRockTint = values.OverallRockTint;
            overallRockTintStrength = values.OverallRockTintStrength;
            lightingTintInfluence = values.LightingTintInfluence;
            edgeWearAmount = values.EdgeWearAmount;
            edgeWearWidth = values.EdgeWearWidth;
            edgeWearCoverage = values.EdgeWearCoverage;
            edgeWearSoftness = values.EdgeWearSoftness;
            edgeWearResponseStrength = values.EdgeWearResponseStrength;
            edgeWearBrightnessLift = values.EdgeWearBrightnessLift;
            edgeWearTint = values.EdgeWearTint;
            edgeWearTintStrength = values.EdgeWearTintStrength;
            edgeWearMacroVariation = values.EdgeWearMacroVariation;
            edgeWearMicroVariation = values.EdgeWearMicroVariation;
            creaseAmount = values.CreaseAmount;
            creaseWidth = values.CreaseWidth;
            creaseLength = values.CreaseLength;
            creaseBranching = values.CreaseBranching;
            creaseSoftness = values.CreaseSoftness;
        }

        private bool FeatureControlsMatch(FeatureRecipeValues values)
        {
            return stoneSurfaceProfile == values.StoneSurfaceProfile &&
                   ColorApproximately(baseColor, values.BaseColor) &&
                   surfaceMaskDebug == values.SurfaceMaskDebug &&
                   FloatApproximately(
                       surfaceMaskBaseLift,
                       values.SurfaceMaskBaseLift) &&
                   FloatApproximately(creviceReach, values.CreviceReach) &&
                   FloatApproximately(
                       creviceSmoothness,
                       values.CreviceSmoothness) &&
                   FloatApproximately(creviceBreakup, values.CreviceBreakup) &&
                   FloatApproximately(dirtCrawlReach, values.DirtCrawlReach) &&
                   FloatApproximately(dirtCoverage, values.DirtCoverage) &&
                   FloatApproximately(
                       exposureResponse,
                       values.ExposureResponse) &&
                   FloatApproximately(creviceResponse, values.CreviceResponse) &&
                   FloatApproximately(baseResponse, values.BaseResponse) &&
                   FloatApproximately(
                       dirtDepositResponse,
                       values.DirtDepositResponse) &&
                   ColorApproximately(exposureTint, values.ExposureTint) &&
                   FloatApproximately(
                       exposureTintStrength,
                       values.ExposureTintStrength) &&
                   ColorApproximately(creviceTint, values.CreviceTint) &&
                   FloatApproximately(
                       creviceTintStrength,
                       values.CreviceTintStrength) &&
                   ColorApproximately(baseTint, values.BaseTint) &&
                   FloatApproximately(
                       baseTintStrength,
                       values.BaseTintStrength) &&
                   ColorApproximately(
                       dirtDepositTint,
                       values.DirtDepositTint) &&
                   FloatApproximately(
                       dirtDepositTintStrength,
                       values.DirtDepositTintStrength) &&
                   ColorApproximately(
                       overallRockTint,
                       values.OverallRockTint) &&
                   FloatApproximately(
                       overallRockTintStrength,
                       values.OverallRockTintStrength) &&
                   FloatApproximately(
                       lightingTintInfluence,
                       values.LightingTintInfluence) &&
                   FloatApproximately(edgeWearAmount, values.EdgeWearAmount) &&
                   FloatApproximately(edgeWearWidth, values.EdgeWearWidth) &&
                   FloatApproximately(
                       edgeWearCoverage,
                       values.EdgeWearCoverage) &&
                   FloatApproximately(
                       edgeWearSoftness,
                       values.EdgeWearSoftness) &&
                   FloatApproximately(
                       edgeWearResponseStrength,
                       values.EdgeWearResponseStrength) &&
                   FloatApproximately(
                       edgeWearBrightnessLift,
                       values.EdgeWearBrightnessLift) &&
                   ColorApproximately(edgeWearTint, values.EdgeWearTint) &&
                   FloatApproximately(
                       edgeWearTintStrength,
                       values.EdgeWearTintStrength) &&
                   FloatApproximately(
                       edgeWearMacroVariation,
                       values.EdgeWearMacroVariation) &&
                   FloatApproximately(
                       edgeWearMicroVariation,
                       values.EdgeWearMicroVariation) &&
                   FloatApproximately(creaseAmount, values.CreaseAmount) &&
                   FloatApproximately(creaseWidth, values.CreaseWidth) &&
                   FloatApproximately(creaseLength, values.CreaseLength) &&
                   FloatApproximately(
                       creaseBranching,
                       values.CreaseBranching) &&
                   FloatApproximately(creaseSoftness, values.CreaseSoftness);
        }

        private static FeatureRecipeValues ResolveFeatureRecipeValues(
            GeneratedMassFeatureRecipe selectedRecipe)
        {
            FeatureRecipeValues values = CreateGenericTestMassValues();

            switch (selectedRecipe)
            {
                case GeneratedMassFeatureRecipe.GenericTestMass:
                    return values;

                case GeneratedMassFeatureRecipe.ColdGreyStone:
                    values.StoneSurfaceProfile =
                        StoneSurfaceProfile.ColdGreyStone;
                    values.EdgeWearResponseStrength = 0.45f;
                    values.EdgeWearBrightnessLift = 0.24f;
                    values.EdgeWearTint = new Color(0.68f, 0.69f, 0.65f, 1f);
                    values.EdgeWearTintStrength = 0.12f;
                    values.EdgeWearMacroVariation = 0.28f;
                    values.EdgeWearMicroVariation = 0.22f;
                    return values;

                case GeneratedMassFeatureRecipe.WetRiverStone:
                    values.StoneSurfaceProfile =
                        StoneSurfaceProfile.DarkWetRiverStone;
                    values.BaseColor =
                        new Color(0.22f, 0.23f, 0.24f, 1f);
                    values.CreviceReach = 1.15f;
                    values.CreviceSmoothness = 1.15f;
                    values.DirtCrawlReach = 1.2f;
                    values.DirtCoverage = 1.15f;
                    values.ExposureResponse = 0.85f;
                    values.CreviceResponse = 1.2f;
                    values.BaseResponse = 1.15f;
                    values.DirtDepositResponse = 1.1f;
                    values.EdgeWearAmount = 1.2f;
                    values.EdgeWearSoftness = 0.65f;
                    values.EdgeWearResponseStrength = 0f;
                    values.EdgeWearTintStrength = 0f;
                    values.EdgeWearMacroVariation = 0f;
                    values.EdgeWearMicroVariation = 0f;
                    return values;

                case GeneratedMassFeatureRecipe.PaleFrostStone:
                    values.StoneSurfaceProfile =
                        StoneSurfaceProfile.PaleFrostStone;
                    values.BaseColor =
                        new Color(0.58f, 0.60f, 0.60f, 1f);
                    values.ExposureResponse = 1.15f;
                    values.CreviceResponse = 0.85f;
                    values.BaseResponse = 0.85f;
                    values.DirtDepositResponse = 0.45f;
                    values.DirtCoverage = 0.55f;
                    values.CreaseAmount = 1.25f;
                    values.CreaseSoftness = 0.45f;
                    values.EdgeWearResponseStrength = 0f;
                    values.EdgeWearTintStrength = 0f;
                    values.EdgeWearMacroVariation = 0f;
                    values.EdgeWearMicroVariation = 0f;
                    return values;

                case GeneratedMassFeatureRecipe.BlackSacredStone:
                    values.StoneSurfaceProfile =
                        StoneSurfaceProfile.BlackSacredStone;
                    values.BaseColor =
                        new Color(0.055f, 0.057f, 0.06f, 1f);
                    values.ExposureResponse = 0.55f;
                    values.CreviceResponse = 0.75f;
                    values.BaseResponse = 0.65f;
                    values.DirtDepositResponse = 0.2f;
                    values.DirtCoverage = 0.4f;
                    values.EdgeWearAmount = 0.55f;
                    values.CreaseAmount = 0.35f;
                    values.EdgeWearResponseStrength = 0f;
                    values.EdgeWearTintStrength = 0f;
                    values.EdgeWearMacroVariation = 0f;
                    values.EdgeWearMicroVariation = 0f;
                    values.LightingTintInfluence = 0.2f;
                    return values;

                case GeneratedMassFeatureRecipe.Custom:
                default:
                    return values;
            }
        }

        private static FeatureRecipeValues CreateGenericTestMassValues()
        {
            return new FeatureRecipeValues
            {
                StoneSurfaceProfile = StoneSurfaceProfile.ColdGreyStone,
                BaseColor = new Color(0.33423817f, 0.3410176f, 0.3490566f, 1f),
                SurfaceMaskDebug = StoneSurfaceMaskDebug.None,
                SurfaceMaskBaseLift = 0f,
                CreviceReach = 1f,
                CreviceSmoothness = 1f,
                CreviceBreakup = 1f,
                DirtCrawlReach = 1f,
                DirtCoverage = 1f,
                ExposureResponse = 1f,
                CreviceResponse = 1f,
                BaseResponse = 1f,
                DirtDepositResponse = 1f,
                ExposureTint = new Color(0.72f, 0.76f, 0.78f, 1f),
                ExposureTintStrength = 0f,
                CreviceTint = new Color(0.5f, 0.5f, 0.5f, 1f),
                CreviceTintStrength = 0f,
                BaseTint = new Color(0.5f, 0.5f, 0.5f, 1f),
                BaseTintStrength = 0f,
                DirtDepositTint = new Color(0.42f, 0.40f, 0.36f, 1f),
                DirtDepositTintStrength = 0f,
                OverallRockTint = new Color(0.5f, 0.5f, 0.5f, 1f),
                OverallRockTintStrength = 0f,
                LightingTintInfluence = 0.35f,
                EdgeWearAmount = 1f,
                EdgeWearWidth = 1f,
                EdgeWearCoverage = 1f,
                EdgeWearSoftness = 0.45f,
                EdgeWearResponseStrength = 0.65f,
                EdgeWearBrightnessLift = 0.38f,
                EdgeWearTint = new Color(0.72f, 0.70f, 0.60f, 1f),
                EdgeWearTintStrength = 0.18f,
                EdgeWearMacroVariation = 0.32f,
                EdgeWearMicroVariation = 0.28f,
                CreaseAmount = 1f,
                CreaseWidth = 1f,
                CreaseLength = 1f,
                CreaseBranching = 1f,
                CreaseSoftness = 0.35f
            };
        }

        private static bool FloatApproximately(float a, float b)
        {
            return Mathf.Abs(a - b) <= 0.0001f;
        }

        private static bool ColorApproximately(Color a, Color b)
        {
            return FloatApproximately(a.r, b.r) &&
                   FloatApproximately(a.g, b.g) &&
                   FloatApproximately(a.b, b.b) &&
                   FloatApproximately(a.a, b.a);
        }

        private static int GenerateDifferentSeed(int current)
        {
            int candidate =
                1 +
                (int)((uint)Guid.NewGuid().GetHashCode() %
                      MassRecipe.MaximumSeed);

            if (candidate == current)
            {
                candidate = current >= MassRecipe.MaximumSeed
                    ? MassRecipe.MinimumSeed
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

        private void ApplyMaterialProperties()
        {
            if (meshRenderer == null)
            {
                return;
            }

            ApplyStoneSurfaceProfileMaterial();

            materialProperties ??= new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(materialProperties);
            materialProperties.SetColor(BaseColorId, baseColor);
            materialProperties.SetColor(LegacyBaseColorId, baseColor);
            materialProperties.SetFloat(
                MaskDebugModeId,
                (float)surfaceMaskDebug);
            materialProperties.SetFloat(
                SurfaceContractId,
                0f);
            materialProperties.SetFloat(
                GeneratedMassFeatureAtlas0EnabledId,
                generatedFeatureAtlas0 != null ? 1f : 0f);
            materialProperties.SetTexture(
                GeneratedMassFeatureAtlas0Id,
                generatedFeatureAtlas0 != null
                    ? generatedFeatureAtlas0
                    : Texture2D.blackTexture);
            materialProperties.SetFloat(
                GeneratedMassFeatureAtlas1EnabledId,
                generatedFeatureAtlas1 != null ? 1f : 0f);
            materialProperties.SetTexture(
                GeneratedMassFeatureAtlas1Id,
                generatedFeatureAtlas1 != null
                    ? generatedFeatureAtlas1
                    : Texture2D.blackTexture);
            materialProperties.SetFloat(
                GeneratedMassFeatureAtlasQualityId,
                ResolveCurrentFeatureAtlasQuality());
            materialProperties.SetFloat(
                GeneratedMassGeometryEdgeWearEnabledId,
                1f);
            materialProperties.SetFloat(
                GeneratedMassEdgeWearCoverageId,
                Mathf.Clamp(edgeWearCoverage, 0.1f, 2f));
            materialProperties.SetFloat(
                GeneratedMassEdgeWearSoftnessId,
                Mathf.Clamp01(edgeWearSoftness));
            materialProperties.SetFloat(
                GeneratedMassEdgeWearResponseStrengthId,
                Mathf.Clamp01(edgeWearResponseStrength));
            materialProperties.SetFloat(
                GeneratedMassEdgeWearBrightnessLiftId,
                Mathf.Clamp01(edgeWearBrightnessLift));
            materialProperties.SetColor(
                GeneratedMassEdgeWearTintId,
                edgeWearTint);
            materialProperties.SetFloat(
                GeneratedMassEdgeWearTintStrengthId,
                Mathf.Clamp01(edgeWearTintStrength));
            materialProperties.SetFloat(
                GeneratedMassEdgeWearMacroVariationId,
                Mathf.Clamp01(edgeWearMacroVariation));
            materialProperties.SetFloat(
                GeneratedMassEdgeWearMicroVariationId,
                Mathf.Clamp01(edgeWearMicroVariation));
            materialProperties.SetFloat(
                GeneratedMassCreaseLengthId,
                Mathf.Clamp(creaseLength, 0.25f, 2f));
            materialProperties.SetFloat(
                GeneratedMassCreaseBranchingId,
                Mathf.Clamp(creaseBranching, 0f, 2f));
            materialProperties.SetFloat(
                GeneratedMassCreaseSoftnessId,
                Mathf.Clamp01(creaseSoftness));

            Bounds meshBounds = meshFilter != null &&
                meshFilter.sharedMesh != null
                    ? meshFilter.sharedMesh.bounds
                    : new Bounds(Vector3.up * 0.5f, Vector3.one);
            float localHeight = Mathf.Max(
                0.0001f,
                meshBounds.size.y);
            float localXZScale = Mathf.Max(
                0.0001f,
                Mathf.Max(meshBounds.size.x, meshBounds.size.z));
            materialProperties.SetFloat(
                GeneratedMassLocalMinYId,
                meshBounds.min.y);
            materialProperties.SetFloat(
                GeneratedMassLocalHeightId,
                localHeight);
            materialProperties.SetFloat(
                GeneratedMassMaskSeedId,
                recipe != null
                    ? recipe.SurfaceSeed
                    : 0f);
            materialProperties.SetFloat(
                GeneratedMassLocalXZScaleId,
                localXZScale);
            materialProperties.SetFloat(
                GeneratedMassMaskBaseLiftId,
                Mathf.Clamp(surfaceMaskBaseLift, 0f, 0.2f));
            materialProperties.SetFloat(
                GeneratedMassCreviceReachId,
                Mathf.Clamp(creviceReach, 0.25f, 2f));
            materialProperties.SetFloat(
                GeneratedMassCreviceSmoothnessId,
                Mathf.Clamp(creviceSmoothness, 0.25f, 2f));
            materialProperties.SetFloat(
                GeneratedMassCreviceBreakupId,
                Mathf.Clamp(creviceBreakup, 0.25f, 2f));
            materialProperties.SetFloat(
                GeneratedMassDirtCrawlReachId,
                Mathf.Clamp(dirtCrawlReach, 0.25f, 2f));
            materialProperties.SetFloat(
                GeneratedMassDirtCoverageId,
                Mathf.Clamp(dirtCoverage, 0.25f, 2f));
            materialProperties.SetFloat(
                GeneratedMassExposureResponseId,
                Mathf.Clamp(exposureResponse, 0f, 2f));
            materialProperties.SetFloat(
                GeneratedMassCreviceResponseId,
                Mathf.Clamp(creviceResponse, 0f, 2f));
            materialProperties.SetFloat(
                GeneratedMassBaseResponseId,
                Mathf.Clamp(baseResponse, 0f, 2f));
            materialProperties.SetFloat(
                GeneratedMassDirtDepositResponseId,
                Mathf.Clamp(dirtDepositResponse, 0f, 2f));
            materialProperties.SetColor(
                GeneratedMassExposureTintId,
                exposureTint);
            materialProperties.SetFloat(
                GeneratedMassExposureTintStrengthId,
                Mathf.Clamp01(exposureTintStrength));
            materialProperties.SetColor(
                GeneratedMassCreviceTintId,
                creviceTint);
            materialProperties.SetFloat(
                GeneratedMassCreviceTintStrengthId,
                Mathf.Clamp01(creviceTintStrength));
            materialProperties.SetColor(
                GeneratedMassBaseTintId,
                baseTint);
            materialProperties.SetFloat(
                GeneratedMassBaseTintStrengthId,
                Mathf.Clamp01(baseTintStrength));
            materialProperties.SetColor(
                GeneratedMassDirtDepositTintId,
                dirtDepositTint);
            materialProperties.SetFloat(
                GeneratedMassDirtDepositTintStrengthId,
                Mathf.Clamp01(dirtDepositTintStrength));
            materialProperties.SetColor(
                GeneratedMassOverallRockTintId,
                overallRockTint);
            materialProperties.SetFloat(
                GeneratedMassOverallRockTintStrengthId,
                Mathf.Clamp01(overallRockTintStrength));
            materialProperties.SetFloat(
                GeneratedMassLightingTintInfluenceId,
                Mathf.Clamp01(lightingTintInfluence));

            meshRenderer.SetPropertyBlock(materialProperties);
        }

        private void ApplyStoneSurfaceProfileMaterial()
        {
            Material selectedMaterial =
                stoneSurfaceProfile switch
                {
                    StoneSurfaceProfile.ColdGreyStone =>
                        coldGreyStoneMaterial,
                    StoneSurfaceProfile.DarkWetRiverStone =>
                        darkWetRiverStoneMaterial,
                    StoneSurfaceProfile.PaleFrostStone =>
                        paleFrostStoneMaterial,
                    StoneSurfaceProfile.BlackSacredStone =>
                        blackSacredStoneMaterial,
                    _ => null
                };

            if (selectedMaterial == null ||
                meshRenderer.sharedMaterial == selectedMaterial)
            {
                return;
            }

            meshRenderer.sharedMaterial = selectedMaterial;
        }

        private float ResolveCurrentFeatureAtlasQuality()
        {
            Texture2D atlas = generatedFeatureAtlas0 != null
                ? generatedFeatureAtlas0
                : generatedFeatureAtlas1;

            if (atlas == null)
            {
                return 0f;
            }

            int resolution = Mathf.Max(atlas.width, atlas.height);
            if (resolution >= 512)
            {
                return 1f;
            }

            if (resolution >= 256)
            {
                return 0.45f;
            }

            return 0f;
        }

        public GeneratedMassFeatureAtlasRequest ResolveFeatureAtlasRequest()
        {
            GeneratedMassFeatureAtlasRequest request =
                GeneratedMassFeatureAtlasRequest.None;

            // Normal-render convex edge wear is geometry-first. It uses generated
            // bevel/chamfer faces and mesh-carried UV2.z material markers, not
            // FeatureAtlas0/1. Atlases remain temporary boundary diagnostics only.
            if (DebugModeRequiresFeatureAtlas0(surfaceMaskDebug))
            {
                request |= GeneratedMassFeatureAtlasRequest.FeatureAtlas0;
            }

            if (DebugModeRequiresFeatureAtlas1(surfaceMaskDebug))
            {
                request |= GeneratedMassFeatureAtlasRequest.FeatureAtlas0 |
                    GeneratedMassFeatureAtlasRequest.FeatureAtlas1;
            }

            return request;
        }

        public int ResolveFeatureAtlasResolutionForRequest(
            GeneratedMassFeatureAtlasRequest atlasRequest)
        {
            return ResolveFeatureAtlasResolution(atlasRequest);
        }

        public static bool DebugModeRequiresFeatureAtlas0(
            StoneSurfaceMaskDebug debugMode)
        {
            switch (debugMode)
            {
                case StoneSurfaceMaskDebug.ConcaveCrease:
                case StoneSurfaceMaskDebug.ConvexBoundaryProximity:
                case StoneSurfaceMaskDebug.ConcaveBoundaryProximity:
                case StoneSurfaceMaskDebug.ConvexBoundarySalienceComposite:
                case StoneSurfaceMaskDebug.BoundarySalience:
                case StoneSurfaceMaskDebug.BoundaryIdentity:
                case StoneSurfaceMaskDebug.ConcaveBoundarySalienceComposite:
                case StoneSurfaceMaskDebug.BoundaryFieldDiagnostic:
                    return true;

                default:
                    return false;
            }
        }

        public static bool DebugModeRequiresFeatureAtlas1(
            StoneSurfaceMaskDebug debugMode)
        {
            switch (debugMode)
            {
                case StoneSurfaceMaskDebug.BoundaryModulationDiagnostic:
                case StoneSurfaceMaskDebug.BoundaryAlongCoordinate:
                case StoneSurfaceMaskDebug.BoundaryCrossCoordinate:
                case StoneSurfaceMaskDebug.BoundaryCoarseModulation:
                case StoneSurfaceMaskDebug.BoundaryFineModulation:
                    return true;

                default:
                    return false;
            }
        }

        private int ResolveFeatureAtlasResolution(
            GeneratedMassFeatureAtlasRequest atlasRequest)
        {
            if (atlasRequest == GeneratedMassFeatureAtlasRequest.None)
            {
                return 0;
            }

            return generationBudget switch
            {
                GeneratedMassGenerationBudget.Compact =>
                    CompactFeatureAtlasResolution,
                GeneratedMassGenerationBudget.Standard =>
                    StandardFeatureAtlasResolution,
                GeneratedMassGenerationBudget.Detailed =>
                    DetailedFeatureAtlasResolution,
                GeneratedMassGenerationBudget.Hero =>
                    HeroFeatureAtlasResolution,
                GeneratedMassGenerationBudget.Custom =>
                    QuantizeFeatureAtlasResolution(
                        customFeatureAtlasResolution),
                _ => StandardFeatureAtlasResolution
            };
        }

        private static int QuantizeFeatureAtlasResolution(int requestedResolution)
        {
            if (requestedResolution <= 128)
            {
                return 128;
            }

            if (requestedResolution <= 256)
            {
                return 256;
            }

            return 512;
        }

        private MassSurfaceFeatureSettings CreateSurfaceFeatureSettings()
        {
            return new MassSurfaceFeatureSettings(
                recipe != null
                    ? recipe.Archetype
                    : MassArchetype.TerrainBoulder,
                recipe != null
                    ? recipe.SurfaceSeed
                    : 0,
                edgeWearAmount,
                edgeWearWidth,
                edgeWearCoverage,
                edgeWearSoftness,
                creaseAmount,
                creaseWidth,
                creaseLength,
                creaseBranching);
        }

        private void EnsureGeneratedMesh()
        {
            if (generatedMesh != null)
            {
                return;
            }

            Mesh existingMesh = meshFilter != null
                ? meshFilter.sharedMesh
                : null;
            if (existingMesh != null &&
                existingMesh.name.StartsWith(
                    GeneratedMeshNamePrefix,
                    StringComparison.Ordinal))
            {
                generatedMesh = existingMesh;
                generatedMesh.hideFlags = HideFlags.DontSave;
                return;
            }

            generatedMesh = new Mesh
            {
                name = "GeneratedMass_Temporary",
                hideFlags = HideFlags.DontSave
            };
        }

        private void ClearGeneratedAssignments()
        {
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

            RemoveLegacyRiverFoamProxy();
            RemoveLegacySurfaceFeatureObjects();
        }

        private void NotifyGeometryChanged()
        {
            using (NotifyConsumersProfilerMarker.Auto())
            {
                GeometryChanged?.Invoke();
            }
        }

        private void ReleaseGeneratedFeatureAtlas()
        {
            DestroyGeneratedTexture(generatedFeatureAtlas0);
            DestroyGeneratedTexture(generatedFeatureAtlas1);
            generatedFeatureAtlas0 = null;
            generatedFeatureAtlas1 = null;
        }

        private void DestroyGeneratedTexture(Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(texture);
            }
            else
            {
                DestroyImmediate(texture);
            }
        }

        private void RemoveLegacyRiverFoamProxy()
        {
            RemoveLegacyChildObject(LegacyRiverFoamProxyObjectName);
        }

        private void RemoveLegacySurfaceFeatureObjects()
        {
            RemoveLegacyChildObject(LegacySurfaceFeatureRootObjectName);
            RemoveLegacyChildObject(LegacyEdgeWearFeatureObjectName);
            RemoveLegacyChildObject(LegacyCreaseFeatureObjectName);
        }

        private void RemoveLegacyChildObject(string childName)
        {
            Transform existing = transform.Find(childName);

            if (existing == null)
            {
                return;
            }

            GameObject legacyObject = existing.gameObject;

            // OnValidate can be invoked while Unity is inside validation, rendering,
            // animation, or physics callbacks. DestroyImmediate is not permitted
            // for GameObjects in those contexts, so legacy scene children are
            // removed through Unity's delayed destruction path instead.
            Destroy(legacyObject);
        }

        private void OnDestroy()
        {
            GeneratedGeometryRegistry.Unregister(this);
            ClearGeneratedAssignments();
            InvalidateStableWorldGeometryFingerprint();

            ReleaseGeneratedFeatureAtlas();

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
