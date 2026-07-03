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
        private static readonly int BaseColorId =
            Shader.PropertyToID("_BaseColor");
        private static readonly int LegacyBaseColorId =
            Shader.PropertyToID("_Base_Color");

        [SerializeField]
        private MassRecipe recipe = new MassRecipe();

        [SerializeField]
        private bool regenerateOnValidate = true;

        [Header("Rendering")]
        [Tooltip("Per-object starting stone colour. The shared material can still layer pixel, exposure, crevice, or biome effects over this base.")]
        [SerializeField]
        private Color baseColor =
            new Color(0.33423817f, 0.3410176f, 0.3490566f, 1f);

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
        private MaterialPropertyBlock materialProperties;
        private Mesh generatedMesh;
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
        public Color BaseColor => baseColor;
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
        }

        private void ApplyMaterialProperties()
        {
            if (meshRenderer == null)
            {
                return;
            }

            materialProperties ??= new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(materialProperties);
            materialProperties.SetColor(BaseColorId, baseColor);
            materialProperties.SetColor(LegacyBaseColorId, baseColor);
            meshRenderer.SetPropertyBlock(materialProperties);
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
