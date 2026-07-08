using System;
using System.Collections.Generic;
using UnityEngine;
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
        [SerializeField]
        private GroundRecipe recipe = new GroundRecipe();

        [Tooltip("Surface/material identity used to generate deterministic ground masks. When empty, built-in defaults are used so old scenes do not require a profile asset.")]
        [SerializeField]
        private GroundSurfaceProfile surfaceProfile;

        [Tooltip("Regenerate when recipe values change in the Inspector.")]
        [SerializeField]
        private bool regenerateOnValidate = true;

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
        private MaterialPropertyBlock materialProperties;

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

        public GroundRecipe Recipe => recipe;
        public GroundSurfaceProfile SurfaceProfile => surfaceProfile;
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
            RefreshModifiers();
            Regenerate();
        }

        private void OnValidate()
        {
            CacheComponents();

            if (!regenerateOnValidate)
            {
                ApplySurfaceProfileMaterialProperties();
                return;
            }

            RefreshModifiers();
            Regenerate();
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

            MeshData meshData = GroundGenerator.Generate(
                recipe,
                surfaceProfile,
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

            float patchContrast =
                GroundSurfaceProfile.ResolvePatchContrast(surfaceProfile);

            float exposureBias =
                GroundSurfaceProfile.ResolveExposureBias(surfaceProfile);

            float dampBias =
                GroundSurfaceProfile.ResolveDampDepositBias(surfaceProfile);

            float vegetationSuitability =
                GroundSurfaceProfile.ResolveVegetationSuitability(surfaceProfile);

            float rockyDrySuitability =
                GroundSurfaceProfile.ResolveRockyDrySuitability(surfaceProfile);

            float snowEligibility =
                GroundSurfaceProfile.ResolveSnowEligibility(surfaceProfile);

            float rainAbsorption =
                GroundSurfaceProfile.ResolveRainAbsorption(surfaceProfile);

            materialProperties ??= new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(materialProperties);

            // Surface Contract: 0 = generated mass/rock, 1 = generated ground.
            // The material may be shared with stones and corridor geometry, so
            // ground-specific response is selected per-renderer through a
            // property block rather than by mutating or duplicating the material
            // asset.
            materialProperties.SetFloat(SurfaceContractId, 1f);
            materialProperties.SetFloat(
                ProfileContrastId,
                Mathf.Lerp(0.85f, 1.35f, patchContrast));
            materialProperties.SetFloat(
                ProfilePixelContrastId,
                Mathf.Lerp(0.80f, 1.35f, patchContrast));
            materialProperties.SetFloat(
                GroundSnowResponseId,
                Mathf.Lerp(0.25f, 1.35f, snowEligibility) *
                Mathf.Lerp(0.90f, 1.15f, exposureBias));
            materialProperties.SetFloat(
                GroundDampResponseId,
                Mathf.Lerp(0.45f, 1.35f, dampBias) *
                Mathf.Lerp(0.90f, 1.20f, rainAbsorption));
            materialProperties.SetFloat(
                GroundVegetationResponseId,
                Mathf.Lerp(0.05f, 0.55f, vegetationSuitability));
            materialProperties.SetFloat(
                GroundRockyDryResponseId,
                Mathf.Lerp(0.15f, 0.95f, rockyDrySuitability));
            materialProperties.SetFloat(
                GroundShoreDampStrengthId,
                Mathf.Lerp(0.75f, 1.35f, rainAbsorption));

            targetRenderer.SetPropertyBlock(materialProperties);
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
