using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using ProgrammaticStylized3D.Geometry.Ground;

namespace ProgrammaticStylized3D.Vegetation
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public abstract class VegetationRendererBase : MonoBehaviour
    {
        protected const string BenchmarkShaderName =
            "PS3D/Vegetation/Stylized Vegetation Benchmark";

        protected static readonly int InstancesId =
            Shader.PropertyToID("_VegetationInstances");
        protected static readonly int LocalToWorldId =
            Shader.PropertyToID("_VegetationLocalToWorld");
        protected static readonly int BaseColorId =
            Shader.PropertyToID("_BaseColor");
        protected static readonly int RootColorId =
            Shader.PropertyToID("_RootColor");
        protected static readonly int TipColorId =
            Shader.PropertyToID("_TipColor");
        protected static readonly int GrassPatchDarkeningId =
            Shader.PropertyToID("_GrassPatchDarkening");
        protected static readonly int GrassPatchBrighteningId =
            Shader.PropertyToID("_GrassPatchBrightening");
        protected static readonly int AmbientResponseId =
            Shader.PropertyToID("_AmbientResponse");
        protected static readonly int SunResponseId =
            Shader.PropertyToID("_SunResponse");
        protected static readonly int LocalLightResponseId =
            Shader.PropertyToID("_LocalLightResponse");
        protected static readonly int MinimumNightVisibilityId =
            Shader.PropertyToID("_MinimumNightVisibility");
        protected static readonly int DiffuseWrapId =
            Shader.PropertyToID("_DiffuseWrap");
        protected static readonly int NormalUpBiasId =
            Shader.PropertyToID("_NormalUpBias");
        protected static readonly int WindNormalResponseId =
            Shader.PropertyToID("_WindNormalResponse");
        protected static readonly int WindBendShadingResponseId =
            Shader.PropertyToID("_WindBendShadingResponse");
        protected static readonly int LightColourInfluenceId =
            Shader.PropertyToID("_LightColourInfluence");
        protected static readonly int StylizedEdgeAccentId =
            Shader.PropertyToID("_StylizedEdgeAccent");
        protected static readonly int EdgeAccentWidthId =
            Shader.PropertyToID("_EdgeAccentWidth");
        protected static readonly int MinimumStableAccentPixelsId =
            Shader.PropertyToID("_MinimumStableAccentPixels");
        protected static readonly int EdgeHighlightWhitenessId =
            Shader.PropertyToID("_EdgeHighlightWhiteness");
        protected static readonly int LocalEdgeFalloffPowerId =
            Shader.PropertyToID("_LocalEdgeFalloffPower");
        protected static readonly int LocalEdgeActivationThresholdId =
            Shader.PropertyToID("_LocalEdgeActivationThreshold");
        protected static readonly int TipWidthRatioId =
            Shader.PropertyToID("_TipWidthRatio");
        protected static readonly int TaperStartId =
            Shader.PropertyToID("_TaperStart");
        protected static readonly int WidthStabilizationEnabledId =
            Shader.PropertyToID("_WidthStabilizationEnabled");
        protected static readonly int WidthStabilizationStartDistanceId =
            Shader.PropertyToID("_WidthStabilizationStartDistance");
        protected static readonly int WidthStabilizationMaximumMultiplierId =
            Shader.PropertyToID("_WidthStabilizationMaximumMultiplier");
        protected static readonly int InteractionBendResponseId =
            Shader.PropertyToID("_InteractionBendResponse");
        protected static readonly int InteractionFlattenResponseId =
            Shader.PropertyToID("_InteractionFlattenResponse");
        protected static readonly int InteractionHeightExponentId =
            Shader.PropertyToID("_InteractionHeightExponent");
        protected static readonly int InteractionMaximumBendId =
            Shader.PropertyToID("_InteractionMaximumBend");
        protected static readonly int InteractionNormalResponseId =
            Shader.PropertyToID("_InteractionNormalResponse");

        [Header("Placement")]
        [SerializeField, Min(1)]
        [Tooltip("Authored production cluster density. Any positive integer is valid; benchmark tiers are owned by VegetationBenchmarkRunner and do not constrain this recipe value.")]
        protected int densityPerSquareMetre = 16;

        [SerializeField]
        protected int seed = 7319;

        [Header("Ground Surface Integration")]
        [SerializeField, HideInInspector]
        protected GeneratedGround coverageGround;

        [SerializeField, Range(0f, 1f)]
        protected float minimumCoverage = 0.02f;

        [Header("Cluster Geometry")]
        [SerializeField, Min(0.01f)]
        protected float clusterDiameter = 0.24f;

        [SerializeField, Min(0.01f)]
        protected float grassHeight = 0.8f;

        [SerializeField, Min(0.002f)]
        [Tooltip("Authoritative reference blade width in metres. The production crossed-card footprint scales proportionally from this value.")]
        protected float masterBladeWidth = 0.021f;

        [SerializeField, Range(0f, 0.5f)]
        protected float tipWidthRatio = 0.12f;

        [SerializeField, Range(0f, 0.95f)]
        protected float taperStart = 0.68f;

        [Header("Silhouette Stability")]
        [SerializeField]
        protected bool enableWidthStabilization = true;

        [SerializeField, Min(0f)]
        protected float widthStabilizationStartDistance = 18f;

        [SerializeField, Range(1f, 1.5f)]
        protected float widthStabilizationMaximumMultiplier = 1.2f;

        [Header("Instance Variation")]
        [SerializeField]
        protected Vector2 heightScaleRange = new Vector2(0.8f, 1.2f);

        [SerializeField]
        protected Vector2 widthScaleRange = new Vector2(0.85f, 1.15f);

        [SerializeField]
        protected Vector2 stiffnessRange = new Vector2(0.12f, 0.48f);

        [Header("Grass Macro Patch Composition")]
        [SerializeField, Range(0.5f, 12f)]
        [Tooltip("Metre scale of broad deterministic grass colour patches. This changes the baked per-cluster pattern and rebuilds the benchmark.")]
        protected float grassPatchScale = 4.5f;

        [SerializeField]
        [Tooltip("Deterministic integer seed for grass macro-patch positions and silhouettes. Adjacent vegetation fields remain continuous when they use the same values.")]
        protected int grassPatchPatternSeed;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Softness of the transition between neutral grass and dark or light macro patches. This changes the baked field and rebuilds the benchmark.")]
        protected float grassPatchTransitionSoftness = 0.75f;

        [SerializeField, Min(0f)]
        [Tooltip("Average amount of neutral grass between dark and light macro patches. Higher values make tonal patches increasingly sparse.")]
        protected float averageGrassPatchSeparation = 1f;

        [SerializeField, Range(0f, 0.5f)]
        [Tooltip("Maximum body-colour darkening inside a full dark grass patch. This is material-only and does not rebuild instances.")]
        protected float darkPatchStrength = 0.12f;

        [SerializeField, Range(0f, 0.5f)]
        [Tooltip("Maximum body-colour brightening inside a full light grass patch. This is material-only and does not rebuild instances.")]
        protected float lightPatchStrength = 0.08f;

        [Header("Immediate Interaction Response")]
        [SerializeField, Range(0f, 2f)]
        [Tooltip("Per-recipe multiplier for horizontal displacement from the scene interaction field.")]
        protected float interactionBendResponse = 1f;

        [SerializeField, Range(0f, 2f)]
        [Tooltip("Per-recipe multiplier for temporary flattening from the scene interaction field.")]
        protected float interactionFlattenResponse = 1f;

        [SerializeField, Range(0.25f, 4f)]
        [Tooltip("Shapes interaction influence from planted roots to responsive tips. Higher values keep more of the lower blade rigid.")]
        protected float interactionHeightExponent = 1.5f;

        [SerializeField, Range(0f, 3f)]
        [Tooltip("Maximum world-space horizontal bend contributed by immediate interaction. This expands conservative render bounds and therefore rebuilds the layer when changed.")]
        protected float maximumInteractionBendMetres = 0.65f;

        [SerializeField, Range(0f, 4f)]
        [Tooltip("Controls how strongly immediate interaction rotates the lighting normal independently from Weather normal response.")]
        protected float interactionNormalResponse = 1f;

        [Header("Stylized Lighting")]
        [SerializeField]
        protected Color rootColor = new Color(0.08f, 0.12f, 0.045f, 1f);

        [SerializeField]
        protected Color baseColor = new Color(0.22f, 0.34f, 0.12f, 1f);

        [SerializeField]
        protected Color tipColor = new Color(0.38f, 0.52f, 0.20f, 1f);

        [SerializeField, Range(0f, 2f)]
        protected float ambientResponse = 1f;

        [SerializeField, Range(0f, 2f)]
        protected float sunResponse = 1f;

        [SerializeField, Range(0f, 2f)]
        protected float localLightResponse = 1f;

        [SerializeField, Range(0f, 0.5f)]
        protected float minimumNightVisibility = 0.04f;

        [SerializeField, Range(0f, 1f)]
        protected float diffuseWrap = 0.35f;

        [SerializeField, Range(0f, 1f)]
        protected float normalUpBias = 0.42f;

        [SerializeField, Range(0f, 4f)]
        [Tooltip("Controls how strongly Weather-driven blade bending rotates the lighting normal. Zero preserves static normals; one applies the full analytical bend normal; values above one provide bounded stylized exaggeration.")]
        protected float windNormalResponse = 0.70f;

        [SerializeField, Range(0f, 2f)]
        [Tooltip("Controls explicit bend-side body shading independently from normal tilt. One provides up to 30% concave darkening and 12% convex brightening; two doubles that stylized response.")]
        protected float windBendShadingResponse = 1f;

        [SerializeField, Range(0f, 1f)]
        protected float lightColourInfluence = 1f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Master strength for the stylized punctual-light edge line. Zero preserves accepted VEG-V1C body lighting; low and medium values use a restrained nonlinear response, while one retains the established maximum accent.")]
        protected float stylizedEdgeAccent = 0.22f;

        [SerializeField, Range(0.01f, 0.50f)]
        [Tooltip("Normalized width of the highlighted light-facing blade edge. The shader follows the clipped crossed-card silhouette; values up to 0.50 permit broader coherent accents.")]
        protected float edgeAccentWidth = 0.10f;

        [SerializeField, Range(0.5f, 2f)]
        [Tooltip("Minimum per-light half-strength accent-band width in screen pixels. The shader measures the linear pre-clip blade footprint, accounts for light-facing side narrowing, and restores full strength 0.2 pixels above this threshold.")]
        protected float minimumStableAccentPixels = 1f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Moves the direct-light edge colour toward white while preserving the source light's HDR intensity. Zero keeps the light chroma; one produces a near-white accent.")]
        protected float edgeHighlightWhiteness = 0.75f;

        [SerializeField, Range(1f, 8f)]
        [Tooltip("Shapes only the punctual-light edge accent falloff. One follows normalized URP attenuation; higher values make the graphic edge weaken faster while close-range distance attenuation remains bounded.")]
        protected float localEdgeFalloffPower = 3f;

        [SerializeField, Range(0f, 2f)]
        [Tooltip("Minimum attenuated punctual-light energy required before a stylized edge accent can appear. Sun and directional lights never create this accent.")]
        protected float localEdgeActivationThreshold = 0.35f;

        [Header("Runtime")]
        [SerializeField]
        protected Camera targetCamera;

        [SerializeField, HideInInspector]
        protected bool renderBenchmark = true;

        [Header("Editor Preview")]
        [SerializeField]
        protected bool sceneViewPreview = true;

        protected GraphicsBuffer instanceBuffer;
        protected GraphicsBuffer indirectArgsBuffer;
        protected Mesh clusterMesh;
        protected Material runtimeMaterial;
        protected VegetationClusterMeshStats meshStats;
        protected Bounds localBounds;
        protected int instanceCount;
        protected int placementCandidateCount;
        protected int coverageRejectedCount;
        protected int groundSampleRejectedCount;
        protected int darkPatchInstanceCount;
        protected int neutralPatchInstanceCount;
        protected int lightPatchInstanceCount;
        protected float minimumGrassPatchValue;
        protected float maximumGrassPatchValue;
        protected float averageGrassPatchValue;
        protected ulong deterministicHash;
        protected bool resourcesReady;
        protected string lastBuildError = string.Empty;
        protected Coroutine groundStartupRetryCoroutine;
        protected bool editModeGroundSyncPending;
        protected GeneratedGround editModeGroundSyncAttemptedGround;
        protected int editModeGroundSyncAttemptedRevision = int.MinValue;
        protected double lastBuildDurationMilliseconds;

        protected readonly struct PlacementDomain
        {
            public PlacementDomain(
                Vector2 localSize,
                float worldArea,
                Matrix4x4 localToWorld,
                string ownerLabel)
            {
                LocalSize = localSize;
                WorldArea = worldArea;
                LocalToWorld = localToWorld;
                OwnerLabel = ownerLabel;
            }

            public Vector2 LocalSize { get; }
            public float WorldArea { get; }
            public Matrix4x4 LocalToWorld { get; }
            public string OwnerLabel { get; }
        }

        private static readonly List<VegetationRendererBase> ActiveRenderersInternal =
            new List<VegetationRendererBase>();

        public int DensityPerSquareMetre => densityPerSquareMetre;
        public int Seed => seed;
        public GeneratedGround SurfaceGround => coverageGround;
        public int InstanceCount => instanceCount;
        public int PlacementCandidateCount => placementCandidateCount;
        public int CoverageRejectedCount => coverageRejectedCount;
        public int GroundSampleRejectedCount => groundSampleRejectedCount;
        public int ClusterVertexCount => meshStats.VertexCount;
        public int ClusterTriangleCount => meshStats.TriangleCount;
        public long InstanceBufferBytes =>
            (long)instanceCount * VegetationInstanceData.Stride;
        public ulong DeterministicHash => deterministicHash;
        public bool ResourcesReady => resourcesReady;
        public string LastBuildError => lastBuildError;
        public double LastBuildDurationMilliseconds =>
            lastBuildDurationMilliseconds;
        public bool RenderingEnabled => renderBenchmark;
        public bool SceneViewPreviewEnabled => sceneViewPreview;
        public float InteractionBendResponse => interactionBendResponse;
        public float InteractionFlattenResponse => interactionFlattenResponse;
        public float InteractionHeightExponent => interactionHeightExponent;
        public float MaximumInteractionBendMetres => maximumInteractionBendMetres;
        public float InteractionNormalResponse => interactionNormalResponse;
        public bool UsesAllCameras => targetCamera == null;
        public string PlacementDomainSummary =>
            coverageGround != null
                ? BuildPlacementDomainSummary(ResolvePlacementDomain())
                : "Invalid: no GeneratedGround ancestor";
        public static IReadOnlyList<VegetationRendererBase> ActiveRenderers =>
            ActiveRenderersInternal;

        protected abstract int AuthoredCoverageRevision { get; }

        protected abstract bool TrySampleAuthoredCoverage(
            Vector3 worldPosition,
            out float coverage);

        protected abstract bool TryValidateBuildSource(out string error);

        protected virtual void OnEnable()
        {
            if (!ActiveRenderersInternal.Contains(this))
            {
                ActiveRenderersInternal.Add(this);
            }

            editModeGroundSyncAttemptedGround = null;
            editModeGroundSyncAttemptedRevision = int.MinValue;
            RebuildVegetation();
            ScheduleGroundStartupRetryIfNeeded();
        }


        private void ScheduleGroundStartupRetryIfNeeded()
        {
            if (!Application.isPlaying ||
                coverageGround == null ||
                instanceCount > 0 ||
                groundSampleRejectedCount <= 0 ||
                groundStartupRetryCoroutine != null)
            {
                return;
            }

            groundStartupRetryCoroutine =
                StartCoroutine(RetryAfterGroundStartupRegeneration());
        }

        private IEnumerator RetryAfterGroundStartupRegeneration()
        {
            const int MaximumWaitFrames = 120;

            for (int frame = 0; frame < MaximumWaitFrames; frame++)
            {
                if (!isActiveAndEnabled ||
                    coverageGround == null)
                {
                    groundStartupRetryCoroutine = null;
                    yield break;
                }

                if (coverageGround.TrySampleBaseSurface(
                        coverageGround.transform.position,
                        out float ignoredHeight,
                        out Vector3 ignoredNormal))
                {
                    groundStartupRetryCoroutine = null;
                    RebuildVegetation();
                    yield break;
                }

                yield return null;
            }

            groundStartupRetryCoroutine = null;
            lastBuildError =
                "Generated Ground did not expose a valid base-surface sample " +
                "within 120 Play Mode frames. Vegetation remains unbuilt; " +
                "rebuild after Ground regeneration completes.";
        }

        protected virtual void OnDisable()
        {
            if (groundStartupRetryCoroutine != null)
            {
                StopCoroutine(groundStartupRetryCoroutine);
                groundStartupRetryCoroutine = null;
            }

            editModeGroundSyncPending = false;
            editModeGroundSyncAttemptedGround = null;
            editModeGroundSyncAttemptedRevision = int.MinValue;
            ActiveRenderersInternal.Remove(this);
            ReleaseResources();
        }

        protected virtual void OnDestroy()
        {
            ActiveRenderersInternal.Remove(this);
            ReleaseResources();
        }

        protected virtual void OnValidate()
        {
            densityPerSquareMetre = ClampDensity(densityPerSquareMetre);
            clusterDiameter = Mathf.Max(0.01f, clusterDiameter);
            grassHeight = Mathf.Max(0.01f, grassHeight);
            masterBladeWidth = Mathf.Clamp(masterBladeWidth, 0.002f, 0.1f);
            tipWidthRatio = Mathf.Clamp(tipWidthRatio, 0f, 0.5f);
            taperStart = Mathf.Clamp(taperStart, 0f, 0.95f);
            widthStabilizationStartDistance = Mathf.Max(0f, widthStabilizationStartDistance);
            widthStabilizationMaximumMultiplier = Mathf.Clamp(
                widthStabilizationMaximumMultiplier, 1f, 1.5f);
            NormalizeRange(ref heightScaleRange, 0.05f, 4f);
            NormalizeRange(ref widthScaleRange, 0.05f, 4f);
            NormalizeRange(ref stiffnessRange, 0f, 1f);
            grassPatchScale = Mathf.Clamp(grassPatchScale, 0.5f, 12f);
            grassPatchTransitionSoftness = Mathf.Clamp01(
                grassPatchTransitionSoftness);
            averageGrassPatchSeparation = Mathf.Max(
                0f,
                averageGrassPatchSeparation);
            darkPatchStrength = Mathf.Clamp(darkPatchStrength, 0f, 0.5f);
            lightPatchStrength = Mathf.Clamp(lightPatchStrength, 0f, 0.5f);
            interactionBendResponse = Mathf.Clamp(
                interactionBendResponse, 0f, 2f);
            interactionFlattenResponse = Mathf.Clamp(
                interactionFlattenResponse, 0f, 2f);
            interactionHeightExponent = Mathf.Clamp(
                interactionHeightExponent, 0.25f, 4f);
            maximumInteractionBendMetres = Mathf.Clamp(
                maximumInteractionBendMetres, 0f, 3f);
            interactionNormalResponse = Mathf.Clamp(
                interactionNormalResponse, 0f, 4f);
            ambientResponse = Mathf.Clamp(ambientResponse, 0f, 2f);
            sunResponse = Mathf.Clamp(sunResponse, 0f, 2f);
            localLightResponse = Mathf.Clamp(localLightResponse, 0f, 2f);
            minimumNightVisibility = Mathf.Clamp(
                minimumNightVisibility,
                0f,
                0.5f);
            diffuseWrap = Mathf.Clamp01(diffuseWrap);
            normalUpBias = Mathf.Clamp01(normalUpBias);
            windNormalResponse = Mathf.Clamp(windNormalResponse, 0f, 4f);
            windBendShadingResponse = Mathf.Clamp(
                windBendShadingResponse,
                0f,
                2f);
            lightColourInfluence = Mathf.Clamp01(lightColourInfluence);
            stylizedEdgeAccent = Mathf.Clamp01(stylizedEdgeAccent);
            edgeAccentWidth = Mathf.Clamp(edgeAccentWidth, 0.01f, 0.50f);
            minimumStableAccentPixels = Mathf.Clamp(
                minimumStableAccentPixels,
                0.5f,
                2f);
            edgeHighlightWhiteness = Mathf.Clamp01(edgeHighlightWhiteness);
            localEdgeFalloffPower = Mathf.Clamp(
                localEdgeFalloffPower,
                1f,
                8f);
            localEdgeActivationThreshold = Mathf.Clamp(
                localEdgeActivationThreshold,
                0f,
                2f);
            RefreshLightingMaterialProperties();
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            SubmitIndirectRender(targetCamera);
        }

        public void RenderEditorPreview(Camera editorCamera)
        {
            if (Application.isPlaying || editorCamera == null)
            {
                return;
            }

            if (editorCamera.cameraType == CameraType.SceneView)
            {
                if (!sceneViewPreview)
                {
                    return;
                }
            }
            else if (editorCamera.cameraType == CameraType.Game)
            {
                if (targetCamera != null && editorCamera != targetCamera)
                {
                    return;
                }
            }
            else
            {
                return;
            }

            TrySynchronizeEditModeGround();
            SubmitIndirectRender(editorCamera);
        }

        private void SubmitIndirectRender(Camera camera)
        {
            if (!renderBenchmark || !resourcesReady ||
                clusterMesh == null || indirectArgsBuffer == null ||
                runtimeMaterial == null)
            {
                return;
            }

            runtimeMaterial.SetMatrix(LocalToWorldId, transform.localToWorldMatrix);
            RenderParams renderParams = new RenderParams(runtimeMaterial)
            {
                worldBounds = TransformBounds(localBounds, transform.localToWorldMatrix),
                shadowCastingMode = ShadowCastingMode.Off,
                receiveShadows = false,
                layer = gameObject.layer,
                entityId = gameObject.GetEntityId(),
                camera = camera
            };

            Graphics.RenderMeshIndirect(
                renderParams,
                clusterMesh,
                indirectArgsBuffer,
                1,
                0);
        }

        public void SetDensityPerSquareMetre(int density)
        {
            densityPerSquareMetre = ClampDensity(density);
        }


        public void SetRenderingEnabled(bool enabled)
        {
            renderBenchmark = enabled;
            if (enabled && !Application.isPlaying)
            {
                TrySynchronizeEditModeGround();
            }
        }

        private void TrySynchronizeEditModeGround()
        {
            if (Application.isPlaying ||
                !isActiveAndEnabled ||
                !renderBenchmark ||
                !editModeGroundSyncPending ||
                coverageGround == null)
            {
                return;
            }

            int surfaceRevision =
                coverageGround.SurfaceGeometryRevision;
            if (coverageGround == editModeGroundSyncAttemptedGround &&
                surfaceRevision == editModeGroundSyncAttemptedRevision)
            {
                return;
            }

            if (!coverageGround.TrySampleBaseSurface(
                    coverageGround.transform.position,
                    out float ignoredHeight,
                    out Vector3 ignoredNormal))
            {
                return;
            }

            editModeGroundSyncAttemptedGround = coverageGround;
            editModeGroundSyncAttemptedRevision = surfaceRevision;
            editModeGroundSyncPending = false;
            RebuildVegetation();

            // A valid Ground centre sample proves the startup ordering race is over.
            // Do not repeatedly rebuild if the configured vegetation field itself
            // legitimately extends beyond or contains no accepted Ground samples.
            editModeGroundSyncPending = false;
        }

        protected PlacementDomain ResolvePlacementDomain()
        {
            float patchSize = Mathf.Max(0.5f, coverageGround.PatchSize);
            Matrix4x4 groundLocalToWorld =
                coverageGround.transform.localToWorldMatrix;
            Vector3 worldLocalX = groundLocalToWorld.MultiplyVector(
                Vector3.right);
            Vector3 worldLocalZ = groundLocalToWorld.MultiplyVector(
                Vector3.forward);
            float worldAreaScale = Vector3.Cross(
                worldLocalX,
                worldLocalZ).magnitude;
            float worldArea = Mathf.Max(
                0.0001f,
                patchSize * patchSize * worldAreaScale);
            return new PlacementDomain(
                new Vector2(patchSize, patchSize),
                worldArea,
                groundLocalToWorld,
                coverageGround.name);
        }

        protected static string BuildPlacementDomainSummary(
            PlacementDomain domain)
        {
            return $"{domain.OwnerLabel}: " +
                   $"{domain.LocalSize.x:0.###} × {domain.LocalSize.y:0.###} local m, " +
                   $"{domain.WorldArea:0.###} world m²";
        }

        private Bounds BuildLocalBounds(
            VegetationInstanceData[] instances,
            PlacementDomain domain,
            float maximumHeight,
            float maximumHorizontalBend)
        {
            Bounds bounds;
            if (instances.Length > 0)
            {
                Vector3 firstBase = ReadInstanceLocalPosition(instances[0]);
                bounds = new Bounds(firstBase, Vector3.zero);
                for (int index = 0; index < instances.Length; index++)
                {
                    Vector3 basePosition =
                        ReadInstanceLocalPosition(instances[index]);
                    bounds.Encapsulate(basePosition);
                    bounds.Encapsulate(
                        basePosition + Vector3.up * maximumHeight);
                }
            }
            else
            {
                bounds = BuildPlacementDomainFallbackBounds(domain);
                Vector3 baseMinimum = bounds.min;
                Vector3 baseMaximum = bounds.max;
                bounds.Encapsulate(
                    baseMinimum + Vector3.up * maximumHeight);
                bounds.Encapsulate(
                    baseMaximum + Vector3.up * maximumHeight);
            }

            bounds.Expand(new Vector3(
                maximumHorizontalBend * 2f,
                maximumHeight * 0.5f,
                maximumHorizontalBend * 2f));
            return bounds;
        }

        private Bounds BuildPlacementDomainFallbackBounds(
            PlacementDomain domain)
        {
            float halfX = domain.LocalSize.x * 0.5f;
            float halfZ = domain.LocalSize.y * 0.5f;
            Vector3 firstWorld = domain.LocalToWorld.MultiplyPoint3x4(
                new Vector3(-halfX, 0f, -halfZ));
            Bounds bounds = new Bounds(
                transform.InverseTransformPoint(firstWorld),
                Vector3.zero);
            EncapsulateDomainCorner(ref bounds, domain, halfX, -halfZ);
            EncapsulateDomainCorner(ref bounds, domain, -halfX, halfZ);
            EncapsulateDomainCorner(ref bounds, domain, halfX, halfZ);
            return bounds;
        }

        private void EncapsulateDomainCorner(
            ref Bounds bounds,
            PlacementDomain domain,
            float x,
            float z)
        {
            Vector3 world = domain.LocalToWorld.MultiplyPoint3x4(
                new Vector3(x, 0f, z));
            bounds.Encapsulate(transform.InverseTransformPoint(world));
        }

        private static Vector3 ReadInstanceLocalPosition(
            VegetationInstanceData instance)
        {
            return new Vector3(
                instance.PositionYaw.x,
                instance.PositionYaw.y,
                instance.PositionYaw.z);
        }

        public int ComputeRebuildConfigurationHash()
        {
            unchecked
            {
                int hash = 17;
                if (coverageGround != null)
                {
                    hash = CombineHash(
                        hash,
                        coverageGround.PatchSize.GetHashCode());
                    Transform groundTransform = coverageGround.transform;
                    hash = CombineHash(
                        hash,
                        groundTransform.position.GetHashCode());
                    hash = CombineHash(
                        hash,
                        groundTransform.rotation.GetHashCode());
                    hash = CombineHash(
                        hash,
                        groundTransform.lossyScale.GetHashCode());
                }
                hash = CombineHash(hash, densityPerSquareMetre);
                hash = CombineHash(hash, seed);
                hash = CombineHash(
                    hash,
                    coverageGround != null
                        ? coverageGround.GetEntityId().GetHashCode()
                        : 0);
                hash = CombineHash(hash, AuthoredCoverageRevision);
                hash = CombineHash(hash, minimumCoverage.GetHashCode());
                hash = CombineHash(hash, clusterDiameter.GetHashCode());
                hash = CombineHash(hash, grassHeight.GetHashCode());
                hash = CombineHash(hash, masterBladeWidth.GetHashCode());
                hash = CombineHash(hash, enableWidthStabilization ? 1 : 0);
                hash = CombineHash(
                    hash,
                    widthStabilizationStartDistance.GetHashCode());
                hash = CombineHash(
                    hash,
                    widthStabilizationMaximumMultiplier.GetHashCode());
                hash = CombineHash(hash, heightScaleRange.GetHashCode());
                hash = CombineHash(hash, widthScaleRange.GetHashCode());
                hash = CombineHash(hash, stiffnessRange.GetHashCode());
                hash = CombineHash(hash, grassPatchScale.GetHashCode());
                hash = CombineHash(hash, grassPatchPatternSeed);
                hash = CombineHash(
                    hash,
                    grassPatchTransitionSoftness.GetHashCode());
                hash = CombineHash(
                    hash,
                    averageGrassPatchSeparation.GetHashCode());
                hash = CombineHash(
                    hash,
                    maximumInteractionBendMetres.GetHashCode());
                return hash;
            }
        }

        public int ComputeLightingConfigurationHash()
        {
            unchecked
            {
                int hash = 17;
                hash = CombineHash(hash, rootColor.GetHashCode());
                hash = CombineHash(hash, baseColor.GetHashCode());
                hash = CombineHash(hash, tipColor.GetHashCode());
                hash = CombineHash(hash, darkPatchStrength.GetHashCode());
                hash = CombineHash(hash, lightPatchStrength.GetHashCode());
                hash = CombineHash(
                    hash,
                    interactionBendResponse.GetHashCode());
                hash = CombineHash(
                    hash,
                    interactionFlattenResponse.GetHashCode());
                hash = CombineHash(
                    hash,
                    interactionHeightExponent.GetHashCode());
                hash = CombineHash(
                    hash,
                    interactionNormalResponse.GetHashCode());
                hash = CombineHash(hash, ambientResponse.GetHashCode());
                hash = CombineHash(hash, sunResponse.GetHashCode());
                hash = CombineHash(hash, localLightResponse.GetHashCode());
                hash = CombineHash(
                    hash,
                    minimumNightVisibility.GetHashCode());
                hash = CombineHash(hash, diffuseWrap.GetHashCode());
                hash = CombineHash(hash, normalUpBias.GetHashCode());
                hash = CombineHash(hash, windNormalResponse.GetHashCode());
                hash = CombineHash(
                    hash,
                    windBendShadingResponse.GetHashCode());
                hash = CombineHash(hash, lightColourInfluence.GetHashCode());
                hash = CombineHash(hash, stylizedEdgeAccent.GetHashCode());
                hash = CombineHash(hash, edgeAccentWidth.GetHashCode());
                hash = CombineHash(
                    hash,
                    minimumStableAccentPixels.GetHashCode());
                hash = CombineHash(
                    hash,
                    edgeHighlightWhiteness.GetHashCode());
                hash = CombineHash(
                    hash,
                    localEdgeFalloffPower.GetHashCode());
                hash = CombineHash(
                    hash,
                    localEdgeActivationThreshold.GetHashCode());
                hash = CombineHash(hash, tipWidthRatio.GetHashCode());
                hash = CombineHash(hash, taperStart.GetHashCode());
                return hash;
            }
        }

        public void RefreshLightingMaterialProperties()
        {
            if (runtimeMaterial == null)
            {
                return;
            }

            runtimeMaterial.SetColor(BaseColorId, baseColor);
            runtimeMaterial.SetColor(RootColorId, rootColor);
            runtimeMaterial.SetColor(TipColorId, tipColor);
            runtimeMaterial.SetFloat(
                GrassPatchDarkeningId,
                darkPatchStrength);
            runtimeMaterial.SetFloat(
                GrassPatchBrighteningId,
                lightPatchStrength);
            runtimeMaterial.SetFloat(
                InteractionBendResponseId,
                interactionBendResponse);
            runtimeMaterial.SetFloat(
                InteractionFlattenResponseId,
                interactionFlattenResponse);
            runtimeMaterial.SetFloat(
                InteractionHeightExponentId,
                interactionHeightExponent);
            runtimeMaterial.SetFloat(
                InteractionMaximumBendId,
                maximumInteractionBendMetres);
            runtimeMaterial.SetFloat(
                InteractionNormalResponseId,
                interactionNormalResponse);
            runtimeMaterial.SetFloat(AmbientResponseId, ambientResponse);
            runtimeMaterial.SetFloat(SunResponseId, sunResponse);
            runtimeMaterial.SetFloat(
                LocalLightResponseId,
                localLightResponse);
            runtimeMaterial.SetFloat(
                MinimumNightVisibilityId,
                minimumNightVisibility);
            runtimeMaterial.SetFloat(DiffuseWrapId, diffuseWrap);
            runtimeMaterial.SetFloat(NormalUpBiasId, normalUpBias);
            runtimeMaterial.SetFloat(
                WindNormalResponseId,
                windNormalResponse);
            runtimeMaterial.SetFloat(
                WindBendShadingResponseId,
                windBendShadingResponse);
            runtimeMaterial.SetFloat(
                LightColourInfluenceId,
                lightColourInfluence);
            runtimeMaterial.SetFloat(
                StylizedEdgeAccentId,
                stylizedEdgeAccent);
            runtimeMaterial.SetFloat(
                EdgeAccentWidthId,
                edgeAccentWidth);
            runtimeMaterial.SetFloat(
                MinimumStableAccentPixelsId,
                minimumStableAccentPixels);
            runtimeMaterial.SetFloat(
                EdgeHighlightWhitenessId,
                edgeHighlightWhiteness);
            runtimeMaterial.SetFloat(
                LocalEdgeFalloffPowerId,
                localEdgeFalloffPower);
            runtimeMaterial.SetFloat(
                LocalEdgeActivationThresholdId,
                localEdgeActivationThreshold);
            runtimeMaterial.SetFloat(TipWidthRatioId, tipWidthRatio);
            runtimeMaterial.SetFloat(TaperStartId, taperStart);
        }


        public void RebuildVegetation()
        {
            double buildStartTime = Time.realtimeSinceStartupAsDouble;
            ReleaseResources();
            lastBuildError = string.Empty;
            editModeGroundSyncPending = false;

            if (!TryValidateBuildSource(out string sourceError))
            {
                lastBuildError = sourceError;
                RecordBuildDuration(buildStartTime);
                return;
            }

            if (!SystemInfo.supportsComputeShaders)
            {
                lastBuildError =
                    "Graphics.RenderMeshIndirect requires compute-shader support on the current platform.";
                RecordBuildDuration(buildStartTime);
                return;
            }

            Shader shader = Shader.Find(BenchmarkShaderName);
            if (shader == null)
            {
                lastBuildError =
                    $"Required shader was not found: {BenchmarkShaderName}";
                RecordBuildDuration(buildStartTime);
                return;
            }

            try
            {
                if (VegetationInstanceData.RuntimeStride !=
                    VegetationInstanceData.Stride)
                {
                    lastBuildError =
                        $"Vegetation instance stride mismatch. Declared " +
                        $"{VegetationInstanceData.Stride}, runtime " +
                        $"{VegetationInstanceData.RuntimeStride}.";
                    RecordBuildDuration(buildStartTime);
                    return;
                }

                clusterMesh = VegetationClusterMeshBuilder.Build(
                    clusterDiameter,
                    grassHeight,
                    masterBladeWidth,
                    out meshStats);

                PlacementDomain placementDomain = ResolvePlacementDomain();
                VegetationInstanceData[] instances = BuildInstances(
                    placementDomain,
                    out deterministicHash);
                instanceCount = instances.Length;
                VegetationInstanceData[] uploadInstances = instances;
                if (instanceCount == 0)
                {
                    uploadInstances = new[]
                    {
                        new VegetationInstanceData(
                            Vector3.zero,
                            0f,
                            Vector2.one,
                            0f,
                            0f,
                            0f,
                            0f)
                    };
                }

                instanceBuffer = new GraphicsBuffer(
                    GraphicsBuffer.Target.Structured,
                    uploadInstances.Length,
                    VegetationInstanceData.Stride);
                instanceBuffer.SetData(uploadInstances);

                indirectArgsBuffer = new GraphicsBuffer(
                    GraphicsBuffer.Target.IndirectArguments,
                    1,
                    GraphicsBuffer.IndirectDrawIndexedArgs.size);
                var arguments = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
                arguments[0].indexCountPerInstance =
                    clusterMesh.GetIndexCount(0);
                arguments[0].instanceCount = (uint)instanceCount;
                arguments[0].startIndex = clusterMesh.GetIndexStart(0);
                arguments[0].baseVertexIndex = clusterMesh.GetBaseVertex(0);
                arguments[0].startInstance = 0;
                indirectArgsBuffer.SetData(arguments);

                runtimeMaterial = new Material(shader)
                {
                    name = "Vegetation Layer Runtime Material",
                    hideFlags = HideFlags.HideAndDontSave,
                    enableInstancing = true
                };
                runtimeMaterial.SetBuffer(InstancesId, instanceBuffer);
                RefreshLightingMaterialProperties();
                runtimeMaterial.SetFloat(WidthStabilizationEnabledId,
                    enableWidthStabilization ? 1f : 0f);
                runtimeMaterial.SetFloat(WidthStabilizationStartDistanceId,
                    widthStabilizationStartDistance);
                runtimeMaterial.SetFloat(WidthStabilizationMaximumMultiplierId,
                    widthStabilizationMaximumMultiplier);

                float maximumHeight = grassHeight * heightScaleRange.y;
                float maximumHorizontalBend = maximumHeight * 1.1f +
                    maximumInteractionBendMetres +
                    clusterDiameter * widthScaleRange.y *
                    widthStabilizationMaximumMultiplier;
                localBounds = BuildLocalBounds(
                    instances,
                    placementDomain,
                    maximumHeight,
                    maximumHorizontalBend);

                resourcesReady = true;
                editModeGroundSyncPending =
                    !Application.isPlaying &&
                    coverageGround != null &&
                    instanceCount == 0 &&
                    groundSampleRejectedCount > 0;
            }
            catch (Exception exception)
            {
                lastBuildError = exception.ToString();
                ReleaseResources();
            }

            RecordBuildDuration(buildStartTime);
        }

        private void RecordBuildDuration(double buildStartTime)
        {
            lastBuildDurationMilliseconds = Math.Max(
                0.0,
                (Time.realtimeSinceStartupAsDouble - buildStartTime) * 1000.0);
        }


        private VegetationInstanceData[] BuildInstances(
            PlacementDomain placementDomain,
            out ulong hash)
        {
            float area = placementDomain.WorldArea;
            placementCandidateCount = Mathf.Max(
                1,
                Mathf.RoundToInt(area * densityPerSquareMetre));
            var instances = new List<VegetationInstanceData>(placementCandidateCount);
            var random = new System.Random(seed);
            hash = 1469598103934665603ul;
            coverageRejectedCount = 0;
            groundSampleRejectedCount = 0;
            darkPatchInstanceCount = 0;
            neutralPatchInstanceCount = 0;
            lightPatchInstanceCount = 0;
            minimumGrassPatchValue = 1f;
            maximumGrassPatchValue = -1f;
            averageGrassPatchValue = 0f;
            double grassPatchValueSum = 0.0;

            float halfX = placementDomain.LocalSize.x * 0.5f;
            float halfZ = placementDomain.LocalSize.y * 0.5f;
            for (int index = 0; index < placementCandidateCount; index++)
            {
                float x = Mathf.Lerp(-halfX, halfX, NextFloat(random));
                float z = Mathf.Lerp(-halfZ, halfZ, NextFloat(random));
                float acceptanceRoll = NextFloat(random);
                Vector3 domainLocalPosition = new Vector3(x, 0f, z);
                Vector3 worldPosition =
                    placementDomain.LocalToWorld.MultiplyPoint3x4(
                        domainLocalPosition);
                Vector3 localPosition =
                    transform.InverseTransformPoint(worldPosition);

                if (!TrySampleAuthoredCoverage(
                        worldPosition, out float coverage) ||
                    coverage < minimumCoverage ||
                    acceptanceRoll > coverage)
                {
                    coverageRejectedCount++;
                    continue;
                }

                if (!coverageGround.TrySampleBaseSurface(
                        worldPosition,
                        out float worldHeight,
                        out Vector3 ignoredNormal))
                {
                    groundSampleRejectedCount++;
                    continue;
                }
                worldPosition.y = worldHeight;
                localPosition = transform.InverseTransformPoint(worldPosition);

                float yaw = NextFloat(random) * Mathf.PI * 2f;
                float widthScale = Mathf.Lerp(
                    widthScaleRange.x, widthScaleRange.y, NextFloat(random));
                float heightScale = Mathf.Lerp(
                    heightScaleRange.x, heightScaleRange.y, NextFloat(random));
                float stiffness = Mathf.Lerp(
                    stiffnessRange.x, stiffnessRange.y, NextFloat(random));
                float phase = NextFloat(random);
                float colorVariation = NextFloat(random);
                float bladeVariation = NextFloat(random);
                float grassMacroPatch = EvaluateGrassMacroPatch(worldPosition);

                var instance = new VegetationInstanceData(
                    localPosition, yaw,
                    new Vector2(widthScale, heightScale),
                    stiffness, phase, colorVariation, bladeVariation);
                instance.VariationPhase.w = grassMacroPatch;
                instances.Add(instance);

                if (grassMacroPatch < -0.0001f)
                {
                    darkPatchInstanceCount++;
                }
                else if (grassMacroPatch > 0.0001f)
                {
                    lightPatchInstanceCount++;
                }
                else
                {
                    neutralPatchInstanceCount++;
                }

                minimumGrassPatchValue = Mathf.Min(
                    minimumGrassPatchValue,
                    grassMacroPatch);
                maximumGrassPatchValue = Mathf.Max(
                    maximumGrassPatchValue,
                    grassMacroPatch);
                grassPatchValueSum += grassMacroPatch;

                HashFloat(ref hash, localPosition.x);
                HashFloat(ref hash, localPosition.y);
                HashFloat(ref hash, localPosition.z);
                HashFloat(ref hash, yaw);
                HashFloat(ref hash, widthScale);
                HashFloat(ref hash, heightScale);
                HashFloat(ref hash, stiffness);
                HashFloat(ref hash, phase);
                HashFloat(ref hash, grassMacroPatch);
            }

            if (instances.Count > 0)
            {
                averageGrassPatchValue =
                    (float)(grassPatchValueSum / instances.Count);
            }
            else
            {
                minimumGrassPatchValue = 0f;
                maximumGrassPatchValue = 0f;
                averageGrassPatchValue = 0f;
            }

            return instances.ToArray();
        }

        private float EvaluateGrassMacroPatch(Vector3 worldPosition)
        {
            float macroScale = Mathf.Max(grassPatchScale, 0.0001f);
            Vector2 patternCoordinate = new Vector2(
                worldPosition.x / macroScale,
                worldPosition.z / macroScale);

            Vector2 warpCoordinate = patternCoordinate * 0.43f;
            Vector2 warp = new Vector2(
                EvaluateGrassValueNoise(
                    warpCoordinate + new Vector2(11.17f, 29.31f),
                    CombineGrassPatchSeed(grassPatchPatternSeed, 7)),
                EvaluateGrassValueNoise(
                    warpCoordinate + new Vector2(23.31f, 17.73f),
                    CombineGrassPatchSeed(grassPatchPatternSeed, 19))) *
                2f - Vector2.one;

            Vector2 regionCoordinate = patternCoordinate + warp * 0.52f;
            float secondaryRegion = EvaluateGrassValueNoise(
                regionCoordinate * 1.65f,
                CombineGrassPatchSeed(grassPatchPatternSeed, 53));
            float secondaryCentered = secondaryRegion - 0.5f;
            Vector2 contourDirection = new Vector2(
                0.34f + warp.y * 0.12f,
                -0.26f + warp.x * 0.12f);
            Vector2 primaryCoordinate =
                regionCoordinate + contourDirection * secondaryCentered;
            float regionalSource = EvaluateGrassValueNoise(
                primaryCoordinate,
                CombineGrassPatchSeed(grassPatchPatternSeed, 37));

            float localSeparationVariation = warp.x * warp.y * 0.08f;
            float localGap = Mathf.Clamp(
                averageGrassPatchSeparation * 0.28f +
                localSeparationVariation,
                0f,
                0.98f);
            float halfGap = localGap * 0.5f;
            float darkNeutralBoundary = 0.5f - halfGap;
            float lightNeutralBoundary = 0.5f + halfGap;
            float transitionWidth = Mathf.Lerp(
                0.06f,
                0.35f,
                Mathf.Clamp01(grassPatchTransitionSoftness));
            float darkPlateauBoundary =
                darkNeutralBoundary - transitionWidth;
            float lightPlateauBoundary =
                lightNeutralBoundary + transitionWidth;

            float darkRegion = 1f - EvaluateGrassSmoothStep(
                darkPlateauBoundary,
                darkNeutralBoundary,
                regionalSource);
            float lightRegion = EvaluateGrassSmoothStep(
                lightNeutralBoundary,
                lightPlateauBoundary,
                regionalSource);
            return Mathf.Clamp(lightRegion - darkRegion, -1f, 1f);
        }

        private static float EvaluateGrassValueNoise(
            Vector2 coordinate,
            int seedValue)
        {
            int x0 = Mathf.FloorToInt(coordinate.x);
            int z0 = Mathf.FloorToInt(coordinate.y);
            int x1 = x0 + 1;
            int z1 = z0 + 1;
            float tx = SmoothGrassNoiseCoordinate(coordinate.x - x0);
            float tz = SmoothGrassNoiseCoordinate(coordinate.y - z0);

            float lower = Mathf.Lerp(
                HashGrassLattice01(x0, z0, seedValue),
                HashGrassLattice01(x1, z0, seedValue),
                tx);
            float upper = Mathf.Lerp(
                HashGrassLattice01(x0, z1, seedValue),
                HashGrassLattice01(x1, z1, seedValue),
                tx);
            return Mathf.Lerp(lower, upper, tz);
        }

        private static float SmoothGrassNoiseCoordinate(float value)
        {
            value = Mathf.Clamp01(value);
            return value * value * (3f - 2f * value);
        }

        private static float EvaluateGrassSmoothStep(
            float edge0,
            float edge1,
            float value)
        {
            float width = Mathf.Max(edge1 - edge0, 0.000001f);
            float t = Mathf.Clamp01((value - edge0) / width);
            return t * t * (3f - 2f * t);
        }

        private static int CombineGrassPatchSeed(int baseSeed, int salt)
        {
            unchecked
            {
                return baseSeed * 486187739 + salt * 16777619;
            }
        }

        private static float HashGrassLattice01(
            int x,
            int z,
            int seedValue)
        {
            unchecked
            {
                uint hashValue = (uint)seedValue;
                hashValue ^= (uint)x * 0x9E3779B9u;
                hashValue = (hashValue << 13) | (hashValue >> 19);
                hashValue ^= (uint)z * 0x85EBCA6Bu;
                hashValue ^= hashValue >> 16;
                hashValue *= 0x7FEB352Du;
                hashValue ^= hashValue >> 15;
                hashValue *= 0x846CA68Bu;
                hashValue ^= hashValue >> 16;
                return (hashValue & 0x00FFFFFFu) *
                    (1f / 16777215f);
            }
        }


        protected void ReleaseResources()
        {
            resourcesReady = false;
            instanceCount = 0;

            instanceBuffer?.Release();
            instanceBuffer = null;

            indirectArgsBuffer?.Release();
            indirectArgsBuffer = null;

            if (runtimeMaterial != null)
            {
                DestroyRuntimeObject(runtimeMaterial);
                runtimeMaterial = null;
            }

            if (clusterMesh != null)
            {
                DestroyRuntimeObject(clusterMesh);
                clusterMesh = null;
            }
        }

        private static void DestroyRuntimeObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(target);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        protected static int ClampDensity(int density)
        {
            return Mathf.Max(1, density);
        }

        protected static int CombineHash(int current, int value)
        {
            unchecked
            {
                return current * 31 + value;
            }
        }

        private static void NormalizeRange(
            ref Vector2 range,
            float minimum,
            float maximum)
        {
            range.x = Mathf.Clamp(range.x, minimum, maximum);
            range.y = Mathf.Clamp(range.y, minimum, maximum);
            if (range.y < range.x)
            {
                range.y = range.x;
            }
        }

        private static float NextFloat(System.Random random)
        {
            return (float)random.NextDouble();
        }

        private static void HashFloat(ref ulong hash, float value)
        {
            uint bits = unchecked((uint)BitConverter.SingleToInt32Bits(value));
            hash ^= bits;
            hash *= 1099511628211ul;
        }

        private static Bounds TransformBounds(
            Bounds local,
            Matrix4x4 localToWorld)
        {
            Vector3 center = localToWorld.MultiplyPoint3x4(local.center);
            Vector3 extents = local.extents;
            Vector3 axisX = localToWorld.MultiplyVector(
                new Vector3(extents.x, 0f, 0f));
            Vector3 axisY = localToWorld.MultiplyVector(
                new Vector3(0f, extents.y, 0f));
            Vector3 axisZ = localToWorld.MultiplyVector(
                new Vector3(0f, 0f, extents.z));
            extents = new Vector3(
                Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x),
                Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y),
                Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z));
            return new Bounds(center, extents * 2f);
        }
    }
}
