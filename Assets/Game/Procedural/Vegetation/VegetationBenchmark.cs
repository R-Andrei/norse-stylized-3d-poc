using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using ProgrammaticStylized3D.Geometry.Ground;
using ProgrammaticStylized3D.Weather;

namespace ProgrammaticStylized3D.Vegetation
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class VegetationBenchmark : MonoBehaviour
    {
        private const string BenchmarkShaderName =
            "PS3D/Vegetation/Stylized Vegetation Benchmark";

        private static readonly int InstancesId =
            Shader.PropertyToID("_VegetationInstances");
        private static readonly int LocalToWorldId =
            Shader.PropertyToID("_VegetationLocalToWorld");
        private static readonly int GeometryCandidateId =
            Shader.PropertyToID("_GeometryCandidate");
        private static readonly int BaseColorId =
            Shader.PropertyToID("_BaseColor");
        private static readonly int RootColorId =
            Shader.PropertyToID("_RootColor");
        private static readonly int TipColorId =
            Shader.PropertyToID("_TipColor");
        private static readonly int GrassPatchDarkeningId =
            Shader.PropertyToID("_GrassPatchDarkening");
        private static readonly int GrassPatchBrighteningId =
            Shader.PropertyToID("_GrassPatchBrightening");
        private static readonly int AmbientResponseId =
            Shader.PropertyToID("_AmbientResponse");
        private static readonly int SunResponseId =
            Shader.PropertyToID("_SunResponse");
        private static readonly int LocalLightResponseId =
            Shader.PropertyToID("_LocalLightResponse");
        private static readonly int MinimumNightVisibilityId =
            Shader.PropertyToID("_MinimumNightVisibility");
        private static readonly int DiffuseWrapId =
            Shader.PropertyToID("_DiffuseWrap");
        private static readonly int NormalUpBiasId =
            Shader.PropertyToID("_NormalUpBias");
        private static readonly int WindNormalResponseId =
            Shader.PropertyToID("_WindNormalResponse");
        private static readonly int WindBendShadingResponseId =
            Shader.PropertyToID("_WindBendShadingResponse");
        private static readonly int LightColourInfluenceId =
            Shader.PropertyToID("_LightColourInfluence");
        private static readonly int StylizedEdgeAccentId =
            Shader.PropertyToID("_StylizedEdgeAccent");
        private static readonly int EdgeAccentWidthId =
            Shader.PropertyToID("_EdgeAccentWidth");
        private static readonly int MinimumStableAccentPixelsId =
            Shader.PropertyToID("_MinimumStableAccentPixels");
        private static readonly int EdgeHighlightWhitenessId =
            Shader.PropertyToID("_EdgeHighlightWhiteness");
        private static readonly int LocalEdgeFalloffPowerId =
            Shader.PropertyToID("_LocalEdgeFalloffPower");
        private static readonly int LocalEdgeActivationThresholdId =
            Shader.PropertyToID("_LocalEdgeActivationThreshold");
        private static readonly int TipWidthRatioId =
            Shader.PropertyToID("_TipWidthRatio");
        private static readonly int TaperStartId =
            Shader.PropertyToID("_TaperStart");
        private static readonly int WidthStabilizationEnabledId =
            Shader.PropertyToID("_WidthStabilizationEnabled");
        private static readonly int WidthStabilizationStartDistanceId =
            Shader.PropertyToID("_WidthStabilizationStartDistance");
        private static readonly int WidthStabilizationMaximumMultiplierId =
            Shader.PropertyToID("_WidthStabilizationMaximumMultiplier");

        [Header("Benchmark Domain")]
        [SerializeField]
        private Vector2 fieldSize = new Vector2(40f, 30f);

        [SerializeField]
        private int densityPerSquareMetre = 16;

        [SerializeField]
        private int seed = 7319;

        [Header("Ground Coverage Integration")]
        [SerializeField]
        private GeneratedGround coverageGround;

        [SerializeField]
        private bool useGroundCoverage = true;

        [SerializeField, Range(0f, 1f)]
        private float minimumCoverage = 0.02f;

        [Header("Cluster Geometry")]
        [SerializeField]
        private VegetationBenchmarkGeometry geometry =
            VegetationBenchmarkGeometry.OpaqueStrips;

        [SerializeField, Min(0.01f)]
        private float clusterDiameter = 0.24f;

        [SerializeField, Min(0.01f)]
        private float grassHeight = 0.8f;

        [SerializeField, Min(0.002f)]
        [Tooltip("Authoritative reference blade width in metres. Card-based candidates scale their card footprint proportionally from this value.")]
        private float masterBladeWidth = 0.021f;

        [SerializeField, Range(0f, 0.5f)]
        private float tipWidthRatio = 0.12f;

        [SerializeField, Range(0f, 0.95f)]
        private float taperStart = 0.68f;

        [Header("Silhouette Stability")]
        [SerializeField]
        private bool enableWidthStabilization = true;

        [SerializeField, Min(0f)]
        private float widthStabilizationStartDistance = 18f;

        [SerializeField, Range(1f, 1.5f)]
        private float widthStabilizationMaximumMultiplier = 1.2f;

        [Header("Instance Variation")]
        [SerializeField]
        private Vector2 heightScaleRange = new Vector2(0.8f, 1.2f);

        [SerializeField]
        private Vector2 widthScaleRange = new Vector2(0.85f, 1.15f);

        [SerializeField]
        private Vector2 stiffnessRange = new Vector2(0.12f, 0.48f);

        [Header("Grass Macro Patch Composition")]
        [SerializeField, Range(0.5f, 12f)]
        [Tooltip("Metre scale of broad deterministic grass colour patches. This changes the baked per-cluster pattern and rebuilds the benchmark.")]
        private float grassPatchScale = 4.5f;

        [SerializeField]
        [Tooltip("Deterministic integer seed for grass macro-patch positions and silhouettes. Adjacent vegetation fields remain continuous when they use the same values.")]
        private int grassPatchPatternSeed;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Softness of the transition between neutral grass and dark or light macro patches. This changes the baked field and rebuilds the benchmark.")]
        private float grassPatchTransitionSoftness = 0.75f;

        [SerializeField, Min(0f)]
        [Tooltip("Average amount of neutral grass between dark and light macro patches. Higher values make tonal patches increasingly sparse.")]
        private float averageGrassPatchSeparation = 1f;

        [SerializeField, Range(0f, 0.5f)]
        [Tooltip("Maximum body-colour darkening inside a full dark grass patch. This is material-only and does not rebuild instances.")]
        private float darkPatchStrength = 0.12f;

        [SerializeField, Range(0f, 0.5f)]
        [Tooltip("Maximum body-colour brightening inside a full light grass patch. This is material-only and does not rebuild instances.")]
        private float lightPatchStrength = 0.08f;

        [Header("Stylized Lighting")]
        [SerializeField]
        private Color rootColor = new Color(0.08f, 0.12f, 0.045f, 1f);

        [SerializeField]
        private Color baseColor = new Color(0.22f, 0.34f, 0.12f, 1f);

        [SerializeField]
        private Color tipColor = new Color(0.38f, 0.52f, 0.20f, 1f);

        [SerializeField, Range(0f, 2f)]
        private float ambientResponse = 1f;

        [SerializeField, Range(0f, 2f)]
        private float sunResponse = 1f;

        [SerializeField, Range(0f, 2f)]
        private float localLightResponse = 1f;

        [SerializeField, Range(0f, 0.5f)]
        private float minimumNightVisibility = 0.04f;

        [SerializeField, Range(0f, 1f)]
        private float diffuseWrap = 0.35f;

        [SerializeField, Range(0f, 1f)]
        private float normalUpBias = 0.42f;

        [SerializeField, Range(0f, 4f)]
        [Tooltip("Controls how strongly Weather-driven blade bending rotates the lighting normal. Zero preserves static normals; one applies the full analytical bend normal; values above one provide bounded stylized exaggeration.")]
        private float windNormalResponse = 0.70f;

        [SerializeField, Range(0f, 2f)]
        [Tooltip("Controls explicit bend-side body shading independently from normal tilt. One provides up to 30% concave darkening and 12% convex brightening; two doubles that stylized response.")]
        private float windBendShadingResponse = 1f;

        [SerializeField, Range(0f, 1f)]
        private float lightColourInfluence = 1f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Master strength for the stylized punctual-light edge line. Zero preserves accepted VEG-V1C body lighting; low and medium values use a restrained nonlinear response, while one retains the established maximum accent.")]
        private float stylizedEdgeAccent = 0.22f;

        [SerializeField, Range(0.01f, 0.50f)]
        [Tooltip("Normalized width of the highlighted light-facing blade edge. The shader follows clipped card silhouettes and geometry strip edges; values up to 0.50 permit broader coherent accents.")]
        private float edgeAccentWidth = 0.10f;

        [SerializeField, Range(0.5f, 2f)]
        [Tooltip("Minimum per-light half-strength accent-band width in screen pixels. The shader measures the linear pre-clip blade footprint, accounts for light-facing side narrowing, and restores full strength 0.2 pixels above this threshold.")]
        private float minimumStableAccentPixels = 1f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Moves the direct-light edge colour toward white while preserving the source light's HDR intensity. Zero keeps the light chroma; one produces a near-white accent.")]
        private float edgeHighlightWhiteness = 0.75f;

        [SerializeField, Range(1f, 8f)]
        [Tooltip("Shapes only the punctual-light edge accent falloff. One follows normalized URP attenuation; higher values make the graphic edge weaken faster while close-range distance attenuation remains bounded.")]
        private float localEdgeFalloffPower = 3f;

        [SerializeField, Range(0f, 2f)]
        [Tooltip("Minimum attenuated punctual-light energy required before a stylized edge accent can appear. Sun and directional lights never create this accent.")]
        private float localEdgeActivationThreshold = 0.35f;

        [Header("Automated Comparison Suite")]
        [SerializeField, Min(0.1f)]
        private float suiteWarmupSeconds = 0.75f;

        [SerializeField, Min(0.25f)]
        private float suiteMeasurementSeconds = 2f;

        [SerializeField, Range(1, 10)]
        private int suitePassesPerCase = 3;

        [SerializeField]
        private bool suiteCaptureScreenshots = true;

        [SerializeField]
        private bool suiteInterleaveDisabledBaseline = true;

        [Header("Runtime")]
        [SerializeField]
        private Camera targetCamera;

        [SerializeField, HideInInspector]
        private bool renderBenchmark = true;

        [Header("Editor Preview")]
        [SerializeField]
        private bool sceneViewPreview = true;

        private static readonly List<VegetationBenchmark> ActiveBenchmarksInternal =
            new List<VegetationBenchmark>();

        private GraphicsBuffer instanceBuffer;
        private GraphicsBuffer indirectArgsBuffer;
        private Mesh clusterMesh;
        private Material runtimeMaterial;
        private VegetationClusterMeshStats meshStats;
        private Bounds localBounds;
        private int instanceCount;
        private int placementCandidateCount;
        private int coverageRejectedCount;
        private int groundSampleRejectedCount;
        private int darkPatchInstanceCount;
        private int neutralPatchInstanceCount;
        private int lightPatchInstanceCount;
        private float minimumGrassPatchValue;
        private float maximumGrassPatchValue;
        private float averageGrassPatchValue;
        private bool suiteForceFullCoverage;
        private ulong deterministicHash;
        private bool resourcesReady;
        private string lastBuildError = string.Empty;
        private Coroutine suiteCoroutine;
        private Coroutine groundStartupRetryCoroutine;
        private bool editModeGroundSyncPending;
        private GeneratedGround editModeGroundSyncAttemptedGround;
        private int editModeGroundSyncAttemptedRevision = int.MinValue;
        private bool suiteRunning;
        private int suiteCurrentCase;
        private int suiteTotalCases;
        private string suiteStatus = "Not run";
        private string lastTimedSuiteReport = string.Empty;

        public Vector2 FieldSize => fieldSize;
        public int DensityPerSquareMetre => densityPerSquareMetre;
        public int Seed => seed;
        public GeneratedGround CoverageGround => coverageGround;
        public bool UseGroundCoverage => useGroundCoverage;
        public VegetationBenchmarkGeometry Geometry => geometry;
        public int InstanceCount => instanceCount;
        public int ClusterVertexCount => meshStats.VertexCount;
        public int ClusterTriangleCount => meshStats.TriangleCount;
        public long InstanceBufferBytes =>
            (long)instanceCount * VegetationInstanceData.Stride;
        public ulong DeterministicHash => deterministicHash;
        public bool ResourcesReady => resourcesReady;
        public string LastBuildError => lastBuildError;
        public bool SuiteRunning => suiteRunning;
        public int SuiteCurrentCase => suiteCurrentCase;
        public int SuiteTotalCases => suiteTotalCases;
        public string SuiteStatus => suiteStatus;
        public bool HasTimedSuiteReport => !string.IsNullOrEmpty(lastTimedSuiteReport);
        public string LastTimedSuiteReport => lastTimedSuiteReport;
        public bool RenderBenchmarkEnabled => renderBenchmark;
        public bool SceneViewPreviewEnabled => sceneViewPreview;
        public bool UsesAllCameras => targetCamera == null;
        public static IReadOnlyList<VegetationBenchmark> ActiveBenchmarks =>
            ActiveBenchmarksInternal;

        private void OnEnable()
        {
            if (!ActiveBenchmarksInternal.Contains(this))
            {
                ActiveBenchmarksInternal.Add(this);
            }

            editModeGroundSyncAttemptedGround = null;
            editModeGroundSyncAttemptedRevision = int.MinValue;
            RebuildBenchmark();
            ScheduleGroundStartupRetryIfNeeded();
        }


        private void ScheduleGroundStartupRetryIfNeeded()
        {
            if (!Application.isPlaying ||
                suiteRunning ||
                !useGroundCoverage ||
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
                    suiteRunning ||
                    !useGroundCoverage ||
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
                    RebuildBenchmark();
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

        private void OnDisable()
        {
            if (suiteCoroutine != null)
            {
                StopCoroutine(suiteCoroutine);
                suiteCoroutine = null;
            }

            if (groundStartupRetryCoroutine != null)
            {
                StopCoroutine(groundStartupRetryCoroutine);
                groundStartupRetryCoroutine = null;
            }

            suiteRunning = false;
            editModeGroundSyncPending = false;
            editModeGroundSyncAttemptedGround = null;
            editModeGroundSyncAttemptedRevision = int.MinValue;
            ActiveBenchmarksInternal.Remove(this);
            ReleaseResources();
        }

        private void OnDestroy()
        {
            ActiveBenchmarksInternal.Remove(this);
            ReleaseResources();
        }

        private void OnValidate()
        {
            fieldSize.x = Mathf.Max(0.5f, fieldSize.x);
            fieldSize.y = Mathf.Max(0.5f, fieldSize.y);
            densityPerSquareMetre = NormalizeDensity(densityPerSquareMetre);
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
            suiteWarmupSeconds = Mathf.Max(0.1f, suiteWarmupSeconds);
            suiteMeasurementSeconds = Mathf.Max(0.25f, suiteMeasurementSeconds);
            suitePassesPerCase = Mathf.Clamp(suitePassesPerCase, 1, 10);
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
            if (Application.isPlaying || suiteRunning || editorCamera == null)
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

        public void SetDensityPreset(int density)
        {
            densityPerSquareMetre = NormalizeDensity(density);
        }

        public void SetGeometryCandidate(VegetationBenchmarkGeometry candidate)
        {
            geometry = candidate;
        }

        public void SetRenderBenchmark(bool enabled)
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
                suiteRunning ||
                !renderBenchmark ||
                !editModeGroundSyncPending ||
                !useGroundCoverage ||
                coverageGround == null)
            {
                return;
            }

            int surfaceRevision =
                coverageGround.VegetationCoverageSurfaceRevision;
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
            RebuildBenchmark();

            // A valid Ground centre sample proves the startup ordering race is over.
            // Do not repeatedly rebuild if the configured vegetation field itself
            // legitimately extends beyond or contains no accepted Ground samples.
            editModeGroundSyncPending = false;
        }

        public int ComputeRebuildConfigurationHash()
        {
            unchecked
            {
                int hash = 17;
                hash = CombineHash(hash, fieldSize.GetHashCode());
                hash = CombineHash(hash, densityPerSquareMetre);
                hash = CombineHash(hash, seed);
                hash = CombineHash(
                    hash,
                    coverageGround != null
                        ? coverageGround.GetEntityId().GetHashCode()
                        : 0);
                hash = CombineHash(hash, useGroundCoverage ? 1 : 0);
                hash = CombineHash(hash, minimumCoverage.GetHashCode());
                hash = CombineHash(hash, (int)geometry);
                hash = CombineHash(hash, clusterDiameter.GetHashCode());
                hash = CombineHash(hash, grassHeight.GetHashCode());
                hash = CombineHash(hash, masterBladeWidth.GetHashCode());
                hash = CombineHash(hash, tipWidthRatio.GetHashCode());
                hash = CombineHash(hash, taperStart.GetHashCode());
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
        }

        public bool BeginTimedComparisonSuite()
        {
            if (!Application.isPlaying)
            {
                suiteStatus = "Timed suite requires Play Mode.";
                return false;
            }

            if (suiteRunning)
            {
                return false;
            }

            suiteCoroutine = StartCoroutine(RunTimedComparisonSuite());
            return true;
        }

        private sealed class TimingResult
        {
            public readonly List<double> CpuSamples = new List<double>(512);
            public readonly List<double> GpuSamples = new List<double>(512);
        }

        private struct SuiteOutcome
        {
            public string Label;
            public double CpuDelta;
            public double CpuNoise;
            public double GpuDelta;
            public double GpuNoise;
        }

        private enum SilhouetteSuiteProfile
        {
            CurrentCompatible = 0,
            StableSilhouette = 1,
            BroadStress = 2
        }

        private struct SuiteCase
        {
            public VegetationBenchmarkGeometry Geometry;
            public int Density;
            public SilhouetteSuiteProfile Profile;
            public bool ForceFullCoverage;

            public SuiteCase(
                VegetationBenchmarkGeometry geometry,
                int density,
                SilhouetteSuiteProfile profile,
                bool forceFullCoverage)
            {
                Geometry = geometry;
                Density = density;
                Profile = profile;
                ForceFullCoverage = forceFullCoverage;
            }

            public string CoverageLabel =>
                ForceFullCoverage ? "FullCoverage" : "AuthoredCoverage";
        }

        private IEnumerator RunTimedComparisonSuite()
        {
            VegetationBenchmarkGeometry originalGeometry = geometry;
            int originalDensity = densityPerSquareMetre;
            bool originalRenderState = renderBenchmark;
            float originalMasterBladeWidth = masterBladeWidth;
            float originalTipWidthRatio = tipWidthRatio;
            float originalTaperStart = taperStart;
            bool originalWidthStabilization = enableWidthStabilization;
            float originalStabilizationStart = widthStabilizationStartDistance;
            float originalStabilizationMaximum = widthStabilizationMaximumMultiplier;
            var builder = new StringBuilder(65536);
            var cases = BuildSuiteCases();
            int successfulCases = 0;
            int failedCases = 0;
            var outcomes = new List<SuiteOutcome>(cases.Count);

            suiteRunning = true;
            suiteCurrentCase = 0;
            suiteTotalCases = cases.Count;
            suiteStatus = "Preparing complete benchmark suite";
            lastTimedSuiteReport = string.Empty;

            builder.AppendLine("[Vegetation V1B Silhouette Stability Automated Benchmark Suite]");
            builder.Append("Cases: ").AppendLine(cases.Count.ToString());
            builder.AppendLine("Scope: 3 geometry candidates × 3 silhouette profiles × 2 densities");
            builder.AppendLine("Coverage scenario: forced full coverage stress");
            builder.Append("Passes per case: ").AppendLine(suitePassesPerCase.ToString());
            builder.Append("Warm-up per measurement: ")
                .Append(suiteWarmupSeconds.ToString("0.###"))
                .AppendLine(" s");
            builder.Append("Measurement per pass: ")
                .Append(suiteMeasurementSeconds.ToString("0.###"))
                .AppendLine(" s");
            builder.Append("Disabled-render baseline: ")
                .AppendLine(suiteInterleaveDisabledBaseline ? "Interleaved" : "Disabled");
            builder.Append("Automatic screenshots: ")
                .AppendLine(suiteCaptureScreenshots ? "Enabled" : "Disabled");
            AppendEnvironmentSummary(builder);
            builder.AppendLine(
                "CPU and GPU values are whole-frame measurements. Vegetation deltas " +
                "subtract an adjacent render-disabled baseline and remain estimates.");
            builder.AppendLine();

            try
            {
                for (int caseIndex = 0; caseIndex < cases.Count; caseIndex++)
                {
                    SuiteCase suiteCase = cases[caseIndex];
                    suiteCurrentCase = caseIndex + 1;
                    geometry = suiteCase.Geometry;
                    densityPerSquareMetre = suiteCase.Density;
                    suiteForceFullCoverage = suiteCase.ForceFullCoverage;
                    ApplySilhouetteSuiteProfile(suiteCase.Profile);
                    renderBenchmark = true;
                    suiteStatus = BuildSuiteStatus(caseIndex, cases.Count, suiteCase, "rebuilding");
                    RebuildBenchmark();

                    builder.AppendLine("============================================================");
                    builder.Append("Case ").Append(caseIndex + 1).Append(" / ")
                        .Append(cases.Count).Append(": ").Append(suiteCase.Geometry)
                        .Append(" @ ").Append(suiteCase.Density).Append(" clusters/m² — ")
                        .Append(suiteCase.Profile).Append(" — ")
                        .AppendLine(suiteCase.CoverageLabel);
                    builder.AppendLine("============================================================");

                    if (!resourcesReady)
                    {
                        failedCases++;
                        builder.AppendLine(BuildComprehensiveReport());
                        builder.AppendLine("Timed measurement: SKIPPED — build failed");
                        builder.AppendLine();
                        yield return null;
                        continue;
                    }

                    var enabledAggregate = new TimingResult();
                    var baselineAggregate = new TimingResult();
                    string screenshotPath = string.Empty;

                    for (int pass = 0; pass < suitePassesPerCase; pass++)
                    {
                        bool baselineFirst = (pass & 1) == 0;
                        if (suiteInterleaveDisabledBaseline && baselineFirst)
                        {
                            yield return MeasureSuiteWindow(
                                false, caseIndex, cases.Count, suiteCase, pass,
                                "baseline", baselineAggregate);
                        }

                        yield return MeasureSuiteWindow(
                            true, caseIndex, cases.Count, suiteCase, pass,
                            "vegetation", enabledAggregate);

                        if (suiteCaptureScreenshots && pass == 0)
                        {
                            screenshotPath = CaptureSuiteScreenshot(suiteCase);
                            yield return new WaitForEndOfFrame();
                            float screenshotDeadline = Time.realtimeSinceStartup + 2f;
                            while (!IsScreenshotReady(screenshotPath) &&
                                   Time.realtimeSinceStartup < screenshotDeadline)
                            {
                                yield return null;
                            }
                        }

                        if (suiteInterleaveDisabledBaseline && !baselineFirst)
                        {
                            yield return MeasureSuiteWindow(
                                false, caseIndex, cases.Count, suiteCase, pass,
                                "baseline", baselineAggregate);
                        }
                    }

                    renderBenchmark = true;
                    successfulCases++;
                    builder.AppendLine(BuildComprehensiveReport());
                    AppendComparisonTimingSummary(
                        builder,
                        enabledAggregate,
                        suiteInterleaveDisabledBaseline ? baselineAggregate : null);
                    if (suiteInterleaveDisabledBaseline)
                    {
                        outcomes.Add(BuildSuiteOutcome(
                            suiteCase,
                            enabledAggregate,
                            baselineAggregate));
                    }
                    if (!string.IsNullOrEmpty(screenshotPath))
                    {
                        builder.Append("Screenshot: ")
                            .AppendLine(DescribeScreenshot(screenshotPath));
                    }
                    builder.AppendLine();
                }
            }
            finally
            {
                geometry = originalGeometry;
                densityPerSquareMetre = originalDensity;
                renderBenchmark = originalRenderState;
                masterBladeWidth = originalMasterBladeWidth;
                tipWidthRatio = originalTipWidthRatio;
                taperStart = originalTaperStart;
                enableWidthStabilization = originalWidthStabilization;
                widthStabilizationStartDistance = originalStabilizationStart;
                widthStabilizationMaximumMultiplier = originalStabilizationMaximum;
                suiteForceFullCoverage = false;
                RebuildBenchmark();

                builder.AppendLine("============================================================");
                AppendSuiteRanking(builder, outcomes);
                builder.AppendLine("Suite summary");
                builder.Append("Successful timed cases: ").Append(successfulCases)
                    .Append(" / ").AppendLine(suiteTotalCases.ToString());
                builder.Append("Failed cases: ").Append(failedCases)
                    .Append(" / ").AppendLine(suiteTotalCases.ToString());
                builder.Append("Restored geometry: ").AppendLine(originalGeometry.ToString());
                builder.Append("Restored density: ").Append(originalDensity)
                    .AppendLine(" clusters/m²");
                builder.Append("Restored render enabled: ")
                    .AppendLine(originalRenderState ? "Yes" : "No");
                builder.Append("Restored master blade width: ")
                    .Append(originalMasterBladeWidth.ToString("0.####")).AppendLine(" m");
                builder.Append("Restored tip width ratio: ")
                    .AppendLine(originalTipWidthRatio.ToString("0.###"));
                builder.Append("Restored taper start: ")
                    .AppendLine(originalTaperStart.ToString("0.###"));
                builder.Append("Restored width stabilization: ")
                    .AppendLine(originalWidthStabilization ? "Enabled" : "Disabled");

                lastTimedSuiteReport = builder.ToString();
                suiteStatus = failedCases == 0
                    ? "Complete — report ready to copy"
                    : "Complete with failures — report ready to copy";
                suiteRunning = false;
                suiteCoroutine = null;
            }
        }

        private IEnumerator MeasureSuiteWindow(
            bool vegetationEnabled,
            int caseIndex,
            int caseCount,
            SuiteCase suiteCase,
            int pass,
            string phase,
            TimingResult aggregate)
        {
            renderBenchmark = vegetationEnabled;
            suiteStatus = BuildSuiteStatus(
                caseIndex,
                caseCount,
                suiteCase,
                $"pass {pass + 1}/{suitePassesPerCase} {phase} warm-up");

            float warmupEnd = Time.realtimeSinceStartup + suiteWarmupSeconds;
            while (Time.realtimeSinceStartup < warmupEnd)
            {
                FrameTimingManager.CaptureFrameTimings();
                yield return null;
            }

            suiteStatus = BuildSuiteStatus(
                caseIndex,
                caseCount,
                suiteCase,
                $"pass {pass + 1}/{suitePassesPerCase} {phase} measurement");
            var frameTimings = new FrameTiming[1];
            float measurementEnd = Time.realtimeSinceStartup + suiteMeasurementSeconds;
            while (Time.realtimeSinceStartup < measurementEnd)
            {
                FrameTimingManager.CaptureFrameTimings();
                yield return null;

                AddFinitePositive(aggregate.CpuSamples, Time.unscaledDeltaTime * 1000.0);
                uint timingCount = FrameTimingManager.GetLatestTimings(1, frameTimings);
                if (timingCount > 0)
                {
                    AddFinitePositive(aggregate.GpuSamples, frameTimings[0].gpuFrameTime);
                }
            }
        }

        private static string BuildSuiteStatus(
            int caseIndex,
            int caseCount,
            SuiteCase suiteCase,
            string phase)
        {
            return $"Case {caseIndex + 1}/{caseCount}: {suiteCase.Geometry} @ " +
                   $"{suiteCase.Density}/m² {suiteCase.Profile} — {phase}";
        }

        private List<SuiteCase> BuildSuiteCases()
        {
            int[] densities = { 35, 50 };
            Array geometries = Enum.GetValues(typeof(VegetationBenchmarkGeometry));
            Array profiles = Enum.GetValues(typeof(SilhouetteSuiteProfile));
            var cases = new List<SuiteCase>(
                geometries.Length * profiles.Length * densities.Length);
            foreach (object geometryValue in geometries)
            {
                var candidate = (VegetationBenchmarkGeometry)geometryValue;
                foreach (object profileValue in profiles)
                {
                    var profile = (SilhouetteSuiteProfile)profileValue;
                    for (int index = 0; index < densities.Length; index++)
                    {
                        cases.Add(new SuiteCase(
                            candidate, densities[index], profile, true));
                    }
                }
            }
            return cases;
        }

        private void ApplySilhouetteSuiteProfile(SilhouetteSuiteProfile profile)
        {
            switch (profile)
            {
                case SilhouetteSuiteProfile.CurrentCompatible:
                    masterBladeWidth = 0.0168f;
                    tipWidthRatio = 0.10f;
                    taperStart = 0f;
                    enableWidthStabilization = false;
                    widthStabilizationMaximumMultiplier = 1f;
                    break;

                case SilhouetteSuiteProfile.StableSilhouette:
                    masterBladeWidth = 0.021f;
                    tipWidthRatio = 0.12f;
                    taperStart = 0.68f;
                    enableWidthStabilization = true;
                    widthStabilizationMaximumMultiplier = 1.20f;
                    break;

                case SilhouetteSuiteProfile.BroadStress:
                    masterBladeWidth = 0.028f;
                    tipWidthRatio = 0.22f;
                    taperStart = 0.78f;
                    enableWidthStabilization = true;
                    widthStabilizationMaximumMultiplier = 1.35f;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(profile), profile, null);
            }
        }

        private static void AddFinitePositive(List<double> samples, double value)
        {
            if (value > 0.0 && !double.IsNaN(value) && !double.IsInfinity(value))
            {
                samples.Add(value);
            }
        }

        private string CaptureSuiteScreenshot(SuiteCase suiteCase)
        {
            try
            {
                string directory = Path.GetFullPath(Path.Combine(
                    Application.dataPath,
                    "../Library/VegetationBenchmarkCaptures"));
                Directory.CreateDirectory(directory);
                string fileName = $"VegetationV1B_{suiteCase.Geometry}_{suiteCase.Density}pm2_{suiteCase.Profile}_{BuildAaFileLabel()}.png";
                string path = Path.Combine(directory, fileName);
                ScreenCapture.CaptureScreenshot(path, 1);
                return path;
            }
            catch (Exception exception)
            {
                return "FAILED: " + exception.Message;
            }
        }

        private void AppendEnvironmentSummary(StringBuilder builder)
        {
            builder.Append("Resolution: ").Append(Screen.width).Append(" × ")
                .Append(Screen.height).AppendLine();
            builder.Append("Graphics API: ").AppendLine(SystemInfo.graphicsDeviceType.ToString());
            builder.Append("GPU: ").AppendLine(SystemInfo.graphicsDeviceName);
            builder.Append("VSync count: ").AppendLine(QualitySettings.vSyncCount.ToString());
            builder.Append("Target frame rate: ").AppendLine(Application.targetFrameRate.ToString());
            builder.Append("Runtime context: ")
                .AppendLine(Application.isEditor
                    ? (Application.isPlaying ? "Unity Editor Play Mode" : "Unity Editor Edit Mode")
                    : "Player build");
            builder.Append("QualitySettings MSAA: ")
                .Append(QualitySettings.antiAliasing).AppendLine("×");
            builder.Append("URP pipeline MSAA: ")
                .AppendLine(ReadPipelineProperty("msaaSampleCount", "Unavailable"));
            builder.Append("Camera post-process AA: ")
                .AppendLine(ReadCameraAntialiasing());
            builder.Append("Render scale: ")
                .AppendLine(ReadPipelineProperty("renderScale", "Unavailable"));
        }

        private static SuiteOutcome BuildSuiteOutcome(
            SuiteCase suiteCase,
            TimingResult enabled,
            TimingResult baseline)
        {
            CalculateDelta(enabled.CpuSamples, baseline.CpuSamples,
                out double cpuDelta, out double cpuNoise);
            CalculateDelta(enabled.GpuSamples, baseline.GpuSamples,
                out double gpuDelta, out double gpuNoise);
            return new SuiteOutcome
            {
                Label = $"{suiteCase.Geometry} @ {suiteCase.Density}/m² {suiteCase.Profile}",
                CpuDelta = cpuDelta,
                CpuNoise = cpuNoise,
                GpuDelta = gpuDelta,
                GpuNoise = gpuNoise
            };
        }

        private static void AppendSuiteRanking(
            StringBuilder builder,
            List<SuiteOutcome> outcomes)
        {
            builder.AppendLine("Confidence-aware ranking");
            if (outcomes.Count == 0)
            {
                builder.AppendLine("Ranking unavailable: no paired baseline outcomes.");
                return;
            }

            outcomes.Sort((left, right) => left.GpuDelta.CompareTo(right.GpuDelta));
            bool anySeparated = false;
            for (int index = 0; index < outcomes.Count; index++)
            {
                SuiteOutcome outcome = outcomes[index];
                bool gpuSeparated = !double.IsNaN(outcome.GpuDelta) &&
                                    outcome.GpuNoise > 0.0 &&
                                    Math.Abs(outcome.GpuDelta) >= outcome.GpuNoise;
                bool cpuSeparated = !double.IsNaN(outcome.CpuDelta) &&
                                    outcome.CpuNoise > 0.0 &&
                                    Math.Abs(outcome.CpuDelta) >= outcome.CpuNoise;
                anySeparated |= gpuSeparated || cpuSeparated;
                builder.Append(index + 1).Append(". ").Append(outcome.Label)
                    .Append(" | GPU Δ ").Append(FormatDelta(outcome.GpuDelta))
                    .Append(" ms, noise ").Append(FormatValue(outcome.GpuNoise))
                    .Append(" | CPU Δ ").Append(FormatDelta(outcome.CpuDelta))
                    .Append(" ms, noise ").Append(FormatValue(outcome.CpuNoise))
                    .Append(" | ")
                    .AppendLine(gpuSeparated || cpuSeparated
                        ? "SEPARATED IN AT LEAST ONE METRIC"
                        : "WITHIN OBSERVED NOISE");
            }

            if (!anySeparated)
            {
                builder.AppendLine(
                    "Verdict: no configuration is separated from observed timing " +
                    "noise; do not select a winner from this run.");
            }
            else
            {
                builder.AppendLine(
                    "Verdict: ranking is ordered by estimated GPU median delta, but " +
                    "only rows marked separated should influence a performance decision.");
            }
            builder.AppendLine();
        }

        private static void CalculateDelta(
            List<double> enabled,
            List<double> baseline,
            out double delta,
            out double noiseFloor)
        {
            if (enabled.Count == 0 || baseline.Count == 0)
            {
                delta = double.NaN;
                noiseFloor = double.NaN;
                return;
            }

            CalculateRobustStatistics(enabled, out _, out double enabledMedian,
                out _, out _, out _, out double enabledStdDev);
            CalculateRobustStatistics(baseline, out _, out double baselineMedian,
                out _, out _, out _, out double baselineStdDev);
            delta = enabledMedian - baselineMedian;
            noiseFloor = Math.Sqrt(
                enabledStdDev * enabledStdDev + baselineStdDev * baselineStdDev);
        }

        private static string FormatDelta(double value)
        {
            return double.IsNaN(value) ? "N/A" : value.ToString("+0.###;-0.###;0");
        }

        private static string FormatValue(double value)
        {
            return double.IsNaN(value) ? "N/A" : value.ToString("0.###");
        }

        private static void AppendComparisonTimingSummary(
            StringBuilder builder,
            TimingResult enabled,
            TimingResult baseline)
        {
            builder.AppendLine("[Timed Measurement]");
            AppendMetricStatistics(builder, "Vegetation-enabled CPU", enabled.CpuSamples);
            AppendMetricStatistics(builder, "Vegetation-enabled GPU", enabled.GpuSamples);

            if (baseline == null)
            {
                builder.AppendLine("Disabled-render baseline: NOT MEASURED");
                return;
            }

            AppendMetricStatistics(builder, "Render-disabled CPU baseline", baseline.CpuSamples);
            AppendMetricStatistics(builder, "Render-disabled GPU baseline", baseline.GpuSamples);
            AppendDelta(builder, "Estimated vegetation CPU delta", enabled.CpuSamples, baseline.CpuSamples);
            AppendDelta(builder, "Estimated vegetation GPU delta", enabled.GpuSamples, baseline.GpuSamples);
        }

        private static void AppendMetricStatistics(
            StringBuilder builder,
            string label,
            List<double> samples)
        {
            builder.Append(label).Append(" samples: ").AppendLine(samples.Count.ToString("N0"));
            if (samples.Count == 0)
            {
                builder.Append(label).AppendLine(": UNAVAILABLE");
                return;
            }

            CalculateRobustStatistics(
                samples,
                out double average,
                out double median,
                out double percentile95,
                out double minimum,
                out double maximum,
                out double standardDeviation);
            builder.Append(label).Append(" average: ").Append(average.ToString("0.###"))
                .AppendLine(" ms");
            builder.Append(label).Append(" median: ").Append(median.ToString("0.###"))
                .AppendLine(" ms");
            builder.Append(label).Append(" p95: ").Append(percentile95.ToString("0.###"))
                .AppendLine(" ms");
            builder.Append(label).Append(" minimum: ").Append(minimum.ToString("0.###"))
                .AppendLine(" ms");
            builder.Append(label).Append(" maximum: ").Append(maximum.ToString("0.###"))
                .AppendLine(" ms");
            builder.Append(label).Append(" standard deviation: ")
                .Append(standardDeviation.ToString("0.###")).AppendLine(" ms");
        }

        private static void AppendDelta(
            StringBuilder builder,
            string label,
            List<double> enabled,
            List<double> baseline)
        {
            if (enabled.Count == 0 || baseline.Count == 0)
            {
                builder.Append(label).AppendLine(": UNAVAILABLE");
                return;
            }

            CalculateDelta(enabled, baseline, out double delta, out double noiseFloor);
            builder.Append(label).Append(": ").Append(delta.ToString("+0.###;-0.###;0"))
                .AppendLine(" ms (median difference)");
            builder.Append(label).Append(" confidence: ")
                .AppendLine(Math.Abs(delta) >= noiseFloor && noiseFloor > 0.0
                    ? "SEPARATED FROM OBSERVED NOISE"
                    : "BELOW / WITHIN OBSERVED NOISE");
            builder.Append(label).Append(" combined noise estimate: ")
                .Append(noiseFloor.ToString("0.###")).AppendLine(" ms");
        }

        private static void CalculateRobustStatistics(
            List<double> samples,
            out double average,
            out double median,
            out double percentile95,
            out double minimum,
            out double maximum,
            out double standardDeviation)
        {
            var sorted = new List<double>(samples);
            sorted.Sort();
            double sum = 0.0;
            for (int index = 0; index < sorted.Count; index++)
            {
                sum += sorted[index];
            }

            average = sum / sorted.Count;
            minimum = sorted[0];
            maximum = sorted[sorted.Count - 1];
            median = Percentile(sorted, 0.5);
            percentile95 = Percentile(sorted, 0.95);

            double squaredDifferenceSum = 0.0;
            for (int index = 0; index < sorted.Count; index++)
            {
                double difference = sorted[index] - average;
                squaredDifferenceSum += difference * difference;
            }
            standardDeviation = Math.Sqrt(squaredDifferenceSum / sorted.Count);
        }

        private static double Percentile(List<double> sortedSamples, double percentile)
        {
            if (sortedSamples.Count == 1)
            {
                return sortedSamples[0];
            }

            double position = (sortedSamples.Count - 1) * percentile;
            int lower = (int)Math.Floor(position);
            int upper = (int)Math.Ceiling(position);
            if (lower == upper)
            {
                return sortedSamples[lower];
            }

            double fraction = position - lower;
            return sortedSamples[lower] +
                   (sortedSamples[upper] - sortedSamples[lower]) * fraction;
        }

        public string BuildAllConfigurationComparisonsReport()
        {
            VegetationBenchmarkGeometry originalGeometry = geometry;
            int originalDensity = densityPerSquareMetre;
            var builder = new StringBuilder(12288);
            int completedCases = 0;
            int failedCases = 0;

            builder.AppendLine("[Vegetation V1A Complete Configuration Comparison]");
            builder.AppendLine("Scope: 3 geometry candidates × 3 density presets");
            builder.AppendLine("Cases: 9");
            builder.AppendLine(
                "Note: this matrix validates generated resources and structural cost. " +
                "It does not measure GPU frame time.");
            builder.AppendLine();

            try
            {
                Array geometryValues = Enum.GetValues(
                    typeof(VegetationBenchmarkGeometry));
                int[] densityPresets = { 20, 35, 50 };

                foreach (object geometryValue in geometryValues)
                {
                    var candidate =
                        (VegetationBenchmarkGeometry)geometryValue;
                    foreach (int density in densityPresets)
                    {
                        geometry = candidate;
                        densityPerSquareMetre = density;
                        RebuildBenchmark();

                        builder.AppendLine(
                            "============================================================");
                        builder.Append("Case ")
                            .Append(completedCases + failedCases + 1)
                            .Append(" / 9: ")
                            .Append(candidate)
                            .Append(" @ ")
                            .Append(density)
                            .AppendLine(" clusters/m²");
                        builder.AppendLine(
                            "============================================================");
                        builder.AppendLine(BuildComprehensiveReport());

                        if (resourcesReady)
                        {
                            completedCases++;
                        }
                        else
                        {
                            failedCases++;
                        }
                    }
                }
            }
            finally
            {
                geometry = originalGeometry;
                densityPerSquareMetre = originalDensity;
                RebuildBenchmark();
            }

            builder.AppendLine(
                "============================================================");
            builder.AppendLine("Comparison summary");
            builder.Append("Successful cases: ")
                .Append(completedCases)
                .AppendLine(" / 9");
            builder.Append("Failed cases: ")
                .Append(failedCases)
                .AppendLine(" / 9");
            builder.Append("Restored geometry: ")
                .AppendLine(originalGeometry.ToString());
            builder.Append("Restored density: ")
                .Append(originalDensity)
                .AppendLine(" clusters/m²");

            return builder.ToString();
        }

        public void RebuildBenchmark()
        {
            ReleaseResources();
            lastBuildError = string.Empty;
            editModeGroundSyncPending = false;

            if (!SystemInfo.supportsComputeShaders)
            {
                lastBuildError =
                    "Graphics.RenderMeshIndirect requires compute-shader support on the current platform.";
                return;
            }

            Shader shader = Shader.Find(BenchmarkShaderName);
            if (shader == null)
            {
                lastBuildError =
                    $"Required shader was not found: {BenchmarkShaderName}";
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
                    return;
                }

                clusterMesh = VegetationClusterMeshBuilder.Build(
                    geometry,
                    clusterDiameter,
                    grassHeight,
                    masterBladeWidth,
                    tipWidthRatio,
                    taperStart,
                    out meshStats);

                VegetationInstanceData[] instances = BuildInstances(
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
                    name = "Vegetation Benchmark Runtime Material",
                    hideFlags = HideFlags.HideAndDontSave,
                    enableInstancing = true
                };
                runtimeMaterial.SetBuffer(InstancesId, instanceBuffer);
                runtimeMaterial.SetFloat(GeometryCandidateId, (float)geometry);
                RefreshLightingMaterialProperties();
                runtimeMaterial.SetFloat(TipWidthRatioId, tipWidthRatio);
                runtimeMaterial.SetFloat(TaperStartId, taperStart);
                runtimeMaterial.SetFloat(WidthStabilizationEnabledId,
                    enableWidthStabilization ? 1f : 0f);
                runtimeMaterial.SetFloat(WidthStabilizationStartDistanceId,
                    widthStabilizationStartDistance);
                runtimeMaterial.SetFloat(WidthStabilizationMaximumMultiplierId,
                    widthStabilizationMaximumMultiplier);

                float maximumHeight = grassHeight * heightScaleRange.y;
                float maximumHorizontalBend = maximumHeight * 1.1f +
                    clusterDiameter * widthScaleRange.y *
                    widthStabilizationMaximumMultiplier;
                localBounds = new Bounds(
                    new Vector3(0f, maximumHeight * 0.5f, 0f),
                    new Vector3(
                        fieldSize.x + maximumHorizontalBend * 2f,
                        maximumHeight * 1.5f,
                        fieldSize.y + maximumHorizontalBend * 2f));

                resourcesReady = true;
                editModeGroundSyncPending =
                    !Application.isPlaying &&
                    useGroundCoverage &&
                    coverageGround != null &&
                    instanceCount == 0 &&
                    groundSampleRejectedCount > 0;
            }
            catch (Exception exception)
            {
                lastBuildError = exception.ToString();
                ReleaseResources();
            }
        }

        public string BuildComprehensiveReport()
        {
            float area = fieldSize.x * fieldSize.y;
            long totalTriangles =
                (long)meshStats.TriangleCount * instanceCount;
            int windDomainCount = WeatherWindDomain.ActiveDomainCount;
            WeatherWindDomain publishedWindDomain = WeatherWindDomain.PublishedDomain;

            var builder = new StringBuilder(4096);
            builder.AppendLine("[Vegetation V1F Benchmark Report]");
            builder.Append("Status: ")
                .AppendLine(resourcesReady ? "READY" : "NOT READY");
            builder.Append("Render enabled: ")
                .AppendLine(renderBenchmark ? "Yes" : "No");
            builder.Append("Target camera: ")
                .AppendLine(targetCamera != null
                    ? targetCamera.name
                    : "All cameras");
            builder.Append("Scene view preview: ")
                .AppendLine(sceneViewPreview ? "Enabled" : "Disabled");
            builder.Append("Scene view preview suppressed by suite: ")
                .AppendLine(suiteRunning ? "Yes" : "No");
            builder.Append("Coverage source: ")
                .AppendLine(useGroundCoverage && coverageGround != null
                    ? coverageGround.name
                    : "Full benchmark field");
            builder.Append("Coverage scenario: ")
                .AppendLine(suiteForceFullCoverage ? "Forced full coverage" : "Authored / configured");
            if (coverageGround != null)
            {
                builder.Append("Ground coverage initialized: ")
                    .AppendLine(coverageGround.VegetationCoverageInitialized ? "Yes" : "No");
                builder.Append("Ground coverage average: ")
                    .Append((coverageGround.CalculateVegetationCoverageFraction() * 100f).ToString("0.###"))
                    .AppendLine("%");
                builder.Append("Ground coverage revision: ")
                    .AppendLine(coverageGround.VegetationCoverageRevision.ToString());
            }
            builder.Append("Placement candidates: ")
                .AppendLine(placementCandidateCount.ToString("N0"));
            builder.Append("Coverage rejected: ")
                .AppendLine(coverageRejectedCount.ToString("N0"));
            builder.Append("Ground sample rejected: ")
                .AppendLine(groundSampleRejectedCount.ToString("N0"));
            builder.Append("Geometry candidate: ")
                .AppendLine(geometry.ToString());
            builder.Append("Master blade width: ")
                .Append(masterBladeWidth.ToString("0.####")).AppendLine(" m");
            builder.Append("Tip width ratio: ")
                .AppendLine(tipWidthRatio.ToString("0.###"));
            builder.Append("Taper start: ")
                .AppendLine(taperStart.ToString("0.###"));
            builder.Append("Width stabilization: ")
                .AppendLine(enableWidthStabilization ? "Enabled" : "Disabled");
            builder.Append("Width stabilization start: ")
                .Append(widthStabilizationStartDistance.ToString("0.###")).AppendLine(" m");
            builder.Append("Width stabilization maximum multiplier: ")
                .AppendLine(widthStabilizationMaximumMultiplier.ToString("0.###"));
            builder.AppendLine("Grass macro patch composition:");
            builder.Append("  Scale / pattern seed: ")
                .Append(grassPatchScale.ToString("0.###")).Append(" m / ")
                .AppendLine(grassPatchPatternSeed.ToString());
            builder.Append("  Transition softness / average separation: ")
                .Append(grassPatchTransitionSoftness.ToString("0.###")).Append(" / ")
                .AppendLine(averageGrassPatchSeparation.ToString("0.###"));
            builder.Append("  Dark / light strengths: ")
                .Append(darkPatchStrength.ToString("0.###")).Append(" / ")
                .AppendLine(lightPatchStrength.ToString("0.###"));
            builder.Append("  Dark / neutral / light instances: ")
                .Append(darkPatchInstanceCount.ToString("N0")).Append(" / ")
                .Append(neutralPatchInstanceCount.ToString("N0")).Append(" / ")
                .AppendLine(lightPatchInstanceCount.ToString("N0"));
            builder.Append("  Signed patch min / mean / max: ")
                .Append(minimumGrassPatchValue.ToString("0.###")).Append(" / ")
                .Append(averageGrassPatchValue.ToString("0.###")).Append(" / ")
                .AppendLine(maximumGrassPatchValue.ToString("0.###"));
            builder.AppendLine("  Storage: VariationPhase.w in existing 48-byte instance record");
            builder.AppendLine("Stylized lighting:");
            builder.Append("  Root / base / tip colours: ")
                .Append(rootColor).Append(" / ")
                .Append(baseColor).Append(" / ")
                .AppendLine(tipColor.ToString());
            builder.Append("  Ambient / sun / local responses: ")
                .Append(ambientResponse.ToString("0.###")).Append(" / ")
                .Append(sunResponse.ToString("0.###")).Append(" / ")
                .AppendLine(localLightResponse.ToString("0.###"));
            builder.Append("  Night floor / wrap / normal-up bias: ")
                .Append(minimumNightVisibility.ToString("0.###")).Append(" / ")
                .Append(diffuseWrap.ToString("0.###")).Append(" / ")
                .AppendLine(normalUpBias.ToString("0.###"));
            builder.Append("  Wind normal response: ")
                .AppendLine(windNormalResponse.ToString("0.###"));
            builder.Append("  Wind bend shading response: ")
                .AppendLine(windBendShadingResponse.ToString("0.###"));
            builder.Append("  Light colour influence: ")
                .AppendLine(lightColourInfluence.ToString("0.###"));
            builder.Append("  Stylized edge accent / width: ")
                .Append(stylizedEdgeAccent.ToString("0.###")).Append(" / ")
                .AppendLine(edgeAccentWidth.ToString("0.###"));
            builder.Append("  Minimum stable accent pixels: ")
                .AppendLine(minimumStableAccentPixels.ToString("0.###"));
            builder.Append("  Edge whiteness / local falloff power: ")
                .Append(edgeHighlightWhiteness.ToString("0.###")).Append(" / ")
                .AppendLine(localEdgeFalloffPower.ToString("0.###"));
            builder.Append("  Local edge activation threshold: ")
                .AppendLine(localEdgeActivationThreshold.ToString("0.###"));
            builder.Append("  RenderSettings ambient mode / colour / intensity: ")
                .Append(RenderSettings.ambientMode).Append(" / ")
                .Append(RenderSettings.ambientLight).Append(" / ")
                .AppendLine(RenderSettings.ambientIntensity.ToString("0.###"));
            Light reportSun = RenderSettings.sun;
            builder.Append("  RenderSettings sun: ")
                .AppendLine(reportSun != null ? reportSun.name : "None");
            if (reportSun != null)
            {
                builder.Append("  Sun colour / intensity / direction: ")
                    .Append(reportSun.color).Append(" / ")
                    .Append(reportSun.intensity.ToString("0.###")).Append(" / ")
                    .AppendLine((-reportSun.transform.forward).ToString("F3"));
            }
            AppendEnvironmentSummary(builder);
            builder.Append("Field size: ")
                .Append(fieldSize.x.ToString("0.###"))
                .Append(" × ")
                .Append(fieldSize.y.ToString("0.###"))
                .AppendLine(" m");
            builder.Append("Field area: ")
                .Append(area.ToString("0.###"))
                .AppendLine(" m²");
            builder.Append("Density: ")
                .Append(densityPerSquareMetre)
                .AppendLine(" clusters/m²");
            builder.Append("Seed: ").AppendLine(seed.ToString());
            builder.Append("Instances: ")
                .AppendLine(instanceCount.ToString("N0"));
            builder.Append("Cluster vertices: ")
                .AppendLine(meshStats.VertexCount.ToString("N0"));
            builder.Append("Cluster triangles: ")
                .AppendLine(meshStats.TriangleCount.ToString("N0"));
            builder.Append("Total submitted triangles: ")
                .AppendLine(totalTriangles.ToString("N0"));
            builder.Append("Draw calls: ")
                .AppendLine(resourcesReady ? "1" : "0");
            builder.Append("Instance stride: ")
                .Append(VegetationInstanceData.Stride)
                .AppendLine(" bytes");
            builder.Append("Instance buffer: ")
                .Append(InstanceBufferBytes.ToString("N0"))
                .AppendLine(" bytes");
            builder.Append("Deterministic hash: 0x")
                .AppendLine(deterministicHash.ToString("X16"));
            builder.Append("Local bounds center: ")
                .AppendLine(localBounds.center.ToString("F3"));
            builder.Append("Local bounds size: ")
                .AppendLine(localBounds.size.ToString("F3"));
            builder.Append("Compute shaders supported: ")
                .AppendLine(SystemInfo.supportsComputeShaders ? "Yes" : "No");
            builder.Append("Active Weather XZ wind domains: ")
                .AppendLine(windDomainCount.ToString());
            builder.Append("Weather wind source state: ")
                .AppendLine(publishedWindDomain != null
                    ? (publishedWindDomain.ResourcesReady
                        ? "SHARED WEATHER XZ FIELD ACTIVE"
                        : "DOMAIN ACTIVE — RESOURCES NOT READY")
                    : "NONE — vegetation remains static");
            if (publishedWindDomain != null)
            {
                builder.AppendLine("Weather domain detail:");
                string weatherReport = publishedWindDomain.BuildComprehensiveReport();
                string[] weatherLines = weatherReport.Split('\n');
                for (int lineIndex = 0; lineIndex < weatherLines.Length; lineIndex++)
                {
                    if (!string.IsNullOrEmpty(weatherLines[lineIndex]))
                    {
                        builder.Append("  ").AppendLine(weatherLines[lineIndex].TrimEnd('\r'));
                    }
                }
            }
            builder.AppendLine("Grass shadow casting: Off");
            builder.AppendLine("Grass real-time shadow receiving: Off");
            builder.AppendLine(
                "Lighting path: URP ambient SH + directional body lighting + additional point/spot body lighting + punctual-local-light-only blade-edge accent");
            builder.Append("Ground integration: ")
                .AppendLine(useGroundCoverage && coverageGround != null
                    ? "Ground height + authored coverage active; grass macro patches remain independently owned"
                    : "Disabled / unassigned; grass macro patches remain independently owned");
            builder.Append("Actor interaction/trails: Not present in V1F")
                .AppendLine();

            if (!string.IsNullOrEmpty(lastBuildError))
            {
                builder.AppendLine("Build error:");
                builder.AppendLine(lastBuildError);
            }

            return builder.ToString();
        }

        private VegetationInstanceData[] BuildInstances(out ulong hash)
        {
            float area = fieldSize.x * fieldSize.y;
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

            float halfX = fieldSize.x * 0.5f;
            float halfZ = fieldSize.y * 0.5f;
            for (int index = 0; index < placementCandidateCount; index++)
            {
                float x = Mathf.Lerp(-halfX, halfX, NextFloat(random));
                float z = Mathf.Lerp(-halfZ, halfZ, NextFloat(random));
                float acceptanceRoll = NextFloat(random);
                Vector3 localPosition = new Vector3(x, 0f, z);
                Vector3 worldPosition = transform.TransformPoint(localPosition);

                if (useGroundCoverage && coverageGround != null)
                {
                    float coverage = 1f;
                    if (!suiteForceFullCoverage &&
                        coverageGround.VegetationCoverageInitialized)
                    {
                        if (!coverageGround.TrySampleVegetationCoverage(
                                worldPosition, out coverage) ||
                            coverage < minimumCoverage ||
                            acceptanceRoll > coverage)
                        {
                            coverageRejectedCount++;
                            continue;
                        }
                    }

                    if (!coverageGround.TrySampleBaseSurface(
                            worldPosition, out float worldHeight, out Vector3 ignoredNormal))
                    {
                        groundSampleRejectedCount++;
                        continue;
                    }
                    worldPosition.y = worldHeight;
                    localPosition = transform.InverseTransformPoint(worldPosition);
                }

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

        private static string ReadPipelineProperty(string propertyName, string fallback)
        {
            RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
            if (pipeline == null)
            {
                return fallback;
            }

            var property = pipeline.GetType().GetProperty(propertyName);
            object value = property != null ? property.GetValue(pipeline, null) : null;
            return value != null ? value.ToString() : fallback;
        }

        private string ReadCameraAntialiasing()
        {
            if (targetCamera == null)
            {
                return "Unavailable — no target camera";
            }

            Component additionalData = targetCamera.GetComponent(
                "UniversalAdditionalCameraData");
            if (additionalData == null)
            {
                return "None / unavailable";
            }

            var property = additionalData.GetType().GetProperty("antialiasing");
            object value = property != null
                ? property.GetValue(additionalData, null)
                : null;
            return value != null ? value.ToString() : "Unavailable";
        }

        private string BuildAaFileLabel()
        {
            string postAa = ReadCameraAntialiasing()
                .Replace(' ', '_')
                .Replace('/', '-');
            return $"MSAA{QualitySettings.antiAliasing}_{postAa}";
        }

        private static bool IsScreenshotReady(string screenshotPath)
        {
            if (string.IsNullOrEmpty(screenshotPath) ||
                screenshotPath.StartsWith("FAILED:", StringComparison.Ordinal))
            {
                return true;
            }

            var file = new FileInfo(screenshotPath);
            return file.Exists && file.Length > 0;
        }

        private static string DescribeScreenshot(string screenshotPath)
        {
            if (string.IsNullOrEmpty(screenshotPath))
            {
                return "Not requested";
            }

            if (screenshotPath.StartsWith("FAILED:", StringComparison.Ordinal))
            {
                return screenshotPath;
            }

            var file = new FileInfo(screenshotPath);
            return file.Exists && file.Length > 0
                ? $"VERIFIED — {file.Length:N0} bytes — {screenshotPath}"
                : $"NOT VERIFIED — file missing or empty — {screenshotPath}";
        }

        private void ReleaseResources()
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

        private static int NormalizeDensity(int density)
        {
            if (density <= 14) return 12;
            if (density <= 18) return 16;
            if (density <= 27) return 20;
            if (density <= 42) return 35;
            return 50;
        }

        private static int CombineHash(int current, int value)
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
