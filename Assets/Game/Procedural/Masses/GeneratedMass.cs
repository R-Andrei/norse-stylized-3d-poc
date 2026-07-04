using System;
using UnityEngine;
using UnityEngine.Rendering;
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

    public enum StoneSurfaceMaskDebug
    {
        None,
        SurfaceVariation,
        Exposure,
        CreviceBase,
        ConvexEdgeWear,
        ConcaveCrease,
        DirtDeposit
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
        private const string SurfaceFeatureObjectName =
            "GeneratedMass_SurfaceFeatures";
        private static readonly int BaseColorId =
            Shader.PropertyToID("_BaseColor");
        private static readonly int LegacyBaseColorId =
            Shader.PropertyToID("_Base_Color");
        private static readonly int MaskDebugModeId =
            Shader.PropertyToID("_MaskDebugMode");
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
        private static readonly int GeneratedMassCreviceBreakupId =
            Shader.PropertyToID("_GeneratedMassCreviceBreakup");
        private static readonly int GeneratedMassDirtCrawlReachId =
            Shader.PropertyToID("_GeneratedMassDirtCrawlReach");
        private static readonly int GeneratedMassDirtCoverageId =
            Shader.PropertyToID("_GeneratedMassDirtCoverage");

        [SerializeField]
        private MassRecipe recipe = new MassRecipe();

        [SerializeField]
        private bool regenerateOnValidate = true;

        [Header("Rendering")]
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

        [Header("Surface Mask Tuning")]
        [Tooltip("Moves the generated lower/contact mask origin upward in normalized local height. Useful for flat slabs or rocks that are intentionally embedded below ground.")]
        [Range(0f, 0.2f)]
        [SerializeField]
        private float surfaceMaskBaseLift;

        [Tooltip("Scales how far the CreviceBase mask may climb from the generated base/contact area.")]
        [Range(0.25f, 2f)]
        [SerializeField]
        private float creviceReach = 1f;

        [Tooltip("Controls how interrupted the CreviceBase lower belt is. Lower values are smoother; higher values break the belt more aggressively.")]
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

        [Header("Surface Feature Lines")]
        [Tooltip("Controls how many selected convex ridge/corner strips are generated for ConvexEdgeWear debug. Zero disables generated edge-wear strips.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float edgeWearAmount = 1f;

        [Tooltip("Scales the width of generated ConvexEdgeWear debug strips.")]
        [Range(0.25f, 2f)]
        [SerializeField]
        private float edgeWearWidth = 1f;

        [Tooltip("Controls how many selected seam/crack strips are generated for ConcaveCrease debug. Zero disables generated crease/crack strips.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float creaseAmount = 1f;

        [Tooltip("Scales the width of generated ConcaveCrease debug strips.")]
        [Range(0.25f, 2f)]
        [SerializeField]
        private float creaseWidth = 1f;

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

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;
        private MeshFilter surfaceFeatureMeshFilter;
        private MeshRenderer surfaceFeatureMeshRenderer;
        private MaterialPropertyBlock materialProperties;
        private MaterialPropertyBlock surfaceFeatureMaterialProperties;
        private Mesh generatedMesh;
        private Mesh surfaceFeatureMesh;
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
        public StoneSurfaceProfile StoneSurfaceProfile => stoneSurfaceProfile;
        public StoneSurfaceMaskDebug SurfaceMaskDebug => surfaceMaskDebug;
        public Color BaseColor => baseColor;
        public float SurfaceMaskBaseLift => surfaceMaskBaseLift;
        public float CreviceReach => creviceReach;
        public float CreviceBreakup => creviceBreakup;
        public float DirtCrawlReach => dirtCrawlReach;
        public float DirtCoverage => dirtCoverage;
        public float EdgeWearAmount => edgeWearAmount;
        public float EdgeWearWidth => edgeWearWidth;
        public float CreaseAmount => creaseAmount;
        public float CreaseWidth => creaseWidth;
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

            if (recipe == null)
            {
                ClearGeneratedAssignments();
                ApplyMaterialProperties();
                InvalidateStableWorldGeometryFingerprint();
                NotifyGeometryChanged();
                return;
            }

            EnsureGeneratedMesh();

            MeshData meshData = MassGenerator.Generate(recipe);

            string meshName =
                $"GeneratedMass_{recipe.Archetype}_Shape{recipe.ShapeSeed}_Surface{recipe.SurfaceSeed}";

            MeshBuilder.ApplyToMesh(
                meshData,
                generatedMesh,
                meshName);

            meshFilter.sharedMesh = generatedMesh;

            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = generatedMesh;
            meshCollider.convex = false;

            RegenerateSurfaceFeatureOverlay();
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

            if (surfaceFeatureMeshFilter == null ||
                surfaceFeatureMeshRenderer == null)
            {
                Transform featureTransform = transform.Find(SurfaceFeatureObjectName);
                if (featureTransform != null)
                {
                    surfaceFeatureMeshFilter = featureTransform.GetComponent<MeshFilter>();
                    surfaceFeatureMeshRenderer = featureTransform.GetComponent<MeshRenderer>();
                }
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
                GeneratedMassCreviceBreakupId,
                Mathf.Clamp(creviceBreakup, 0.25f, 2f));
            materialProperties.SetFloat(
                GeneratedMassDirtCrawlReachId,
                Mathf.Clamp(dirtCrawlReach, 0.25f, 2f));
            materialProperties.SetFloat(
                GeneratedMassDirtCoverageId,
                Mathf.Clamp(dirtCoverage, 0.25f, 2f));

            meshRenderer.SetPropertyBlock(materialProperties);

            ApplySurfaceFeatureOverlayMaterialProperties(
                meshBounds,
                localHeight,
                localXZScale);
        }

        private void ApplySurfaceFeatureOverlayMaterialProperties(
            Bounds meshBounds,
            float localHeight,
            float localXZScale)
        {
            if (surfaceFeatureMeshRenderer == null)
            {
                return;
            }

            surfaceFeatureMeshRenderer.sharedMaterial = meshRenderer.sharedMaterial;
            surfaceFeatureMeshRenderer.enabled = IsSurfaceFeatureDebugMode();
            surfaceFeatureMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            surfaceFeatureMeshRenderer.receiveShadows = false;
            surfaceFeatureMeshRenderer.lightProbeUsage = LightProbeUsage.Off;
            surfaceFeatureMeshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            surfaceFeatureMeshRenderer.motionVectorGenerationMode =
                MotionVectorGenerationMode.ForceNoMotion;

            surfaceFeatureMaterialProperties ??= new MaterialPropertyBlock();
            surfaceFeatureMaterialProperties.Clear();
            surfaceFeatureMaterialProperties.SetColor(BaseColorId, baseColor);
            surfaceFeatureMaterialProperties.SetColor(LegacyBaseColorId, baseColor);
            surfaceFeatureMaterialProperties.SetFloat(
                MaskDebugModeId,
                (float)surfaceMaskDebug);
            surfaceFeatureMaterialProperties.SetFloat(
                GeneratedMassLocalMinYId,
                meshBounds.min.y);
            surfaceFeatureMaterialProperties.SetFloat(
                GeneratedMassLocalHeightId,
                localHeight);
            surfaceFeatureMaterialProperties.SetFloat(
                GeneratedMassMaskSeedId,
                recipe != null
                    ? recipe.SurfaceSeed
                    : 0f);
            surfaceFeatureMaterialProperties.SetFloat(
                GeneratedMassLocalXZScaleId,
                localXZScale);
            surfaceFeatureMaterialProperties.SetFloat(
                GeneratedMassMaskBaseLiftId,
                Mathf.Clamp(surfaceMaskBaseLift, 0f, 0.2f));
            surfaceFeatureMaterialProperties.SetFloat(
                GeneratedMassCreviceReachId,
                Mathf.Clamp(creviceReach, 0.25f, 2f));
            surfaceFeatureMaterialProperties.SetFloat(
                GeneratedMassCreviceBreakupId,
                Mathf.Clamp(creviceBreakup, 0.25f, 2f));
            surfaceFeatureMaterialProperties.SetFloat(
                GeneratedMassDirtCrawlReachId,
                Mathf.Clamp(dirtCrawlReach, 0.25f, 2f));
            surfaceFeatureMaterialProperties.SetFloat(
                GeneratedMassDirtCoverageId,
                Mathf.Clamp(dirtCoverage, 0.25f, 2f));
            surfaceFeatureMeshRenderer.SetPropertyBlock(
                surfaceFeatureMaterialProperties);
        }

        private bool IsSurfaceFeatureDebugMode()
        {
            return surfaceMaskDebug == StoneSurfaceMaskDebug.ConvexEdgeWear ||
                   surfaceMaskDebug == StoneSurfaceMaskDebug.ConcaveCrease;
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

        private void RegenerateSurfaceFeatureOverlay()
        {
            EnsureSurfaceFeatureObject();

            if (surfaceFeatureMesh == null ||
                surfaceFeatureMeshFilter == null ||
                generatedMesh == null)
            {
                return;
            }

            MassSurfaceFeatureSettings settings =
                new MassSurfaceFeatureSettings(
                    recipe != null
                        ? recipe.Archetype
                        : MassArchetype.TerrainBoulder,
                    recipe != null
                        ? recipe.SurfaceSeed
                        : 0,
                    edgeWearAmount,
                    edgeWearWidth,
                    creaseAmount,
                    creaseWidth);

            MassSurfaceFeatureGenerator.Generate(
                generatedMesh,
                surfaceFeatureMesh,
                settings);
            surfaceFeatureMeshFilter.sharedMesh = surfaceFeatureMesh;
        }

        private void EnsureSurfaceFeatureObject()
        {
            if (surfaceFeatureMeshFilter == null ||
                surfaceFeatureMeshRenderer == null)
            {
                Transform featureTransform = transform.Find(SurfaceFeatureObjectName);
                if (featureTransform == null)
                {
                    GameObject featureObject =
                        new GameObject(SurfaceFeatureObjectName);
                    featureTransform = featureObject.transform;
                    featureTransform.SetParent(transform, false);
                }

                surfaceFeatureMeshFilter =
                    featureTransform.GetComponent<MeshFilter>();
                if (surfaceFeatureMeshFilter == null)
                {
                    surfaceFeatureMeshFilter =
                        featureTransform.gameObject.AddComponent<MeshFilter>();
                }

                surfaceFeatureMeshRenderer =
                    featureTransform.GetComponent<MeshRenderer>();
                if (surfaceFeatureMeshRenderer == null)
                {
                    surfaceFeatureMeshRenderer =
                        featureTransform.gameObject.AddComponent<MeshRenderer>();
                }

                surfaceFeatureMeshRenderer.shadowCastingMode =
                    ShadowCastingMode.Off;
                surfaceFeatureMeshRenderer.receiveShadows = false;
                surfaceFeatureMeshRenderer.lightProbeUsage = LightProbeUsage.Off;
                surfaceFeatureMeshRenderer.reflectionProbeUsage =
                    ReflectionProbeUsage.Off;
                surfaceFeatureMeshRenderer.motionVectorGenerationMode =
                    MotionVectorGenerationMode.ForceNoMotion;
                surfaceFeatureMeshRenderer.enabled = IsSurfaceFeatureDebugMode();
            }

            if (surfaceFeatureMesh != null)
            {
                return;
            }

            surfaceFeatureMesh = new Mesh
            {
                name = "GeneratedMass_SurfaceFeatures_Temporary",
                hideFlags = HideFlags.DontSave
            };
            surfaceFeatureMeshFilter.sharedMesh = surfaceFeatureMesh;
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

            if (surfaceFeatureMeshFilter != null &&
                surfaceFeatureMeshFilter.sharedMesh == surfaceFeatureMesh)
            {
                surfaceFeatureMeshFilter.sharedMesh = null;
            }

            if (surfaceFeatureMeshRenderer != null)
            {
                surfaceFeatureMeshRenderer.enabled = false;
            }

            RemoveLegacyRiverFoamProxy();
        }

        private void NotifyGeometryChanged()
        {
            GeometryChanged?.Invoke();
        }

        private void RemoveLegacyRiverFoamProxy()
        {
            Transform existing =
                transform.Find(
                    LegacyRiverFoamProxyObjectName);

            if (existing == null)
            {
                return;
            }

            GameObject proxy = existing.gameObject;

            if (Application.isPlaying)
            {
                Destroy(proxy);
            }
            else
            {
                DestroyImmediate(proxy);
            }
        }

        private void OnDestroy()
        {
            GeneratedGeometryRegistry.Unregister(this);
            ClearGeneratedAssignments();
            InvalidateStableWorldGeometryFingerprint();

            if (surfaceFeatureMesh != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(surfaceFeatureMesh);
                }
                else
                {
                    DestroyImmediate(surfaceFeatureMesh);
                }

                surfaceFeatureMesh = null;
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
