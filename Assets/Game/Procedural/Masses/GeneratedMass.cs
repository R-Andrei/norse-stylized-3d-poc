using System;
using UnityEngine;
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
        ConvexRidgeProximity = 15,
        ConvexRidgeWeight = 16,
        ConvexRidgeComposite = 17,
        ConcaveCreaseProximity = 18,
        ConcaveCreaseWeight = 19,
        ConcaveCreaseComposite = 20,
        BoundaryFieldDiagnostic = 21
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
        private const int FeatureAtlasResolution =
            GeneratedMassFeatureAtlasBaker.DefaultResolution;
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
        private static readonly int GeneratedMassEdgeWearBreakupId =
            Shader.PropertyToID("_GeneratedMassEdgeWearBreakup");
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
            public float EdgeWearBreakup;
            public float CreaseAmount;
            public float CreaseWidth;
            public float CreaseLength;
            public float CreaseBranching;
            public float CreaseSoftness;
        }

        [SerializeField]
        private MassRecipe recipe = new MassRecipe();

        [SerializeField]
        private bool regenerateOnValidate = true;

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

        [Tooltip("Controls the generated ConvexEdgeWear semantic field strength. Zero disables the atlas edge-wear mask.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float edgeWearAmount = 1f;

        [Tooltip("Scales the width of the generated ConvexEdgeWear atlas mask.")]
        [Range(0.25f, 2f)]
        [SerializeField]
        private float edgeWearWidth = 1f;

        [Tooltip("Controls how broadly convex ridge boundaries are eligible for the semantic edge-wear field. Lower values keep only strongest ridges; higher values include more ridge boundaries.")]
        [Range(0.1f, 2f)]
        [SerializeField]
        private float edgeWearCoverage = 1f;

        [Tooltip("Controls falloff for generated ConvexEdgeWear atlas data. Lower values are sharper; higher values are softer.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float edgeWearSoftness = 0.45f;

        [Tooltip("Master intensity for the normal-render convex edge-wear material response. Zero keeps the atlas data debug-only and leaves normal rendering unchanged.")]
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

        [Tooltip("How much existing generated surface noise modulates the visible edge-wear response. This does not change the semantic atlas data.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float edgeWearBreakup;

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

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;
        private MaterialPropertyBlock materialProperties;
        private Mesh generatedMesh;
        private Texture2D generatedFeatureAtlas0;
        private bool stableWorldGeometryFingerprintValid;
        private GeneratedGeometryStableFingerprint
            stableWorldGeometryFingerprint;
        private Mesh stableWorldGeometryFingerprintMesh;
        private Matrix4x4 stableWorldGeometryFingerprintMatrix;

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
        public float EdgeWearBreakup => edgeWearBreakup;
        public float CreaseAmount => creaseAmount;
        public float CreaseWidth => creaseWidth;
        public float CreaseLength => creaseLength;
        public float CreaseBranching => creaseBranching;
        public float CreaseSoftness => creaseSoftness;
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
            Regenerate();
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

            if (!regenerateOnValidate)
            {
                CacheComponents();
                ApplyMaterialProperties();
                GeneratedGeometryRegistry.NotifyChanged(this);
                return;
            }

            CacheComponents();
            Regenerate();
        }

        private void EnsureRecipeState()
        {
            if (recipe == null)
            {
                recipe = new MassRecipe();
            }

            riverInteraction ??= new GeneratedRiverInteractionSettings();
            riverInteraction.Validate();

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
            CacheComponents();
            RemoveLegacySurfaceFeatureObjects();

            if (recipe == null)
            {
                ReleaseGeneratedFeatureAtlas();
                ClearGeneratedAssignments();
                ApplyMaterialProperties();
                InvalidateStableWorldGeometryFingerprint();
                NotifyGeometryChanged();
                return;
            }

            EnsureGeneratedMesh();
            ReleaseGeneratedFeatureAtlas();

            MeshData sourceMeshData = MassGenerator.Generate(recipe);
            MassSurfaceFeatureSettings featureSettings =
                CreateSurfaceFeatureSettings();
            GeneratedMassFeatureAtlasBaker.Result featureAtlas =
                GeneratedMassFeatureAtlasBaker.Bake(
                    sourceMeshData,
                    featureSettings,
                    FeatureAtlasResolution);

            string meshName =
                $"GeneratedMass_{recipe.Archetype}_Shape{recipe.ShapeSeed}_Surface{recipe.SurfaceSeed}";

            MeshBuilder.ApplyToMesh(
                sourceMeshData,
                generatedMesh,
                meshName);

            if (featureAtlas != null &&
                featureAtlas.FeatureAtlasUV != null &&
                featureAtlas.FeatureAtlasUV.Count == generatedMesh.vertexCount)
            {
                generatedFeatureAtlas0 = featureAtlas.Atlas0;
                generatedMesh.SetUVs(3, featureAtlas.FeatureAtlasUV);
            }
            else if (featureAtlas != null)
            {
                DestroyGeneratedTexture(featureAtlas.Atlas0);
            }

            meshFilter.sharedMesh = generatedMesh;

            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = generatedMesh;
            meshCollider.convex = false;

            ApplyMaterialProperties();
            RefreshStableWorldGeometryFingerprint();
            RemoveLegacyRiverFoamProxy();
            NotifyGeometryChanged();
        }

        private void RefreshStableWorldGeometryFingerprint()
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
            Regenerate();
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
            edgeWearBreakup = values.EdgeWearBreakup;
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
                   FloatApproximately(edgeWearBreakup, values.EdgeWearBreakup) &&
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
                    values.EdgeWearBreakup = 0.16f;
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
                    values.EdgeWearBreakup = 0f;
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
                    values.EdgeWearBreakup = 0f;
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
                    values.EdgeWearBreakup = 0f;
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
                EdgeWearBreakup = 0.22f,
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
                GeneratedMassEdgeWearBreakupId,
                Mathf.Clamp01(edgeWearBreakup));
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
            GeometryChanged?.Invoke();
        }

        private void ReleaseGeneratedFeatureAtlas()
        {
            DestroyGeneratedTexture(generatedFeatureAtlas0);
            generatedFeatureAtlas0 = null;
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
