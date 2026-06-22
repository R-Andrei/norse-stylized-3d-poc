using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines;
using ProgrammaticStylized3D.Geometry.Ground;

namespace ProgrammaticStylized3D.Rivers
{
    public enum StylizedRiverQuality
    {
        Low,
        Medium,
        High
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SplineContainer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class StylizedRiver : MonoBehaviour
    {
        private const string CurrentObjectName = "__PS3D_RiverCurrentAccents";

        private const string BodyShaderResourcePath =
            "PS3DRiver/Shaders/SH_CleanStylizedRiver";

        private const string CurrentShaderResourcePath =
            "PS3DRiver/Shaders/SH_CleanStylizedRiverCurrent";

        private const string FlowTextureResourcePath =
            "PS3DRiver/Textures/T_RiverFlowBands";

        private const string DetailTextureResourcePath =
            "PS3DRiver/Textures/T_RiverDetail";

        [Header("Setup")]
        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private bool liveRegeneration = true;

        [Header("Channel")]
        [Range(0.5f, 20f)]
        [SerializeField] private float width = 4f;

        [Range(0.1f, 12f)]
        [SerializeField] private float bankBlend = 2.5f;

        [Range(0.1f, 6f)]
        [SerializeField] private float depth = 1.1f;

        [Range(0f, 1f)]
        [SerializeField] private float bedFlatness = 0.62f;

        [SerializeField] private StylizedRiverBankProfile bankProfile = StylizedRiverBankProfile.Natural;

        [Range(0f, 0.8f)]
        [SerializeField] private float bankOverlap = 0.22f;

        [Range(0f, 1f)]
        [SerializeField] private float carvingStrength = 1f;

        [Header("Surface Mesh")]
        [SerializeField] private StylizedRiverQuality quality = StylizedRiverQuality.Medium;

        [Tooltip("Raises water above the carved bed to avoid depth fighting.")]
        [Range(0f, 0.25f)]
        [SerializeField] private float surfaceOffset = 0.035f;

        [Header("Water Body")]
        [SerializeField] private Color shallowColor = new Color(0.42f, 0.73f, 0.73f, 1f);
        [SerializeField] private Color deepColor = new Color(0.12f, 0.42f, 0.48f, 1f);
        [SerializeField] private Color flowTint = new Color(0.72f, 0.92f, 0.88f, 1f);

        [Range(0.15f, 1f)]
        [SerializeField] private float opacity = 0.72f;

        // Provisional local-slice Body Flow controls only. This tiled implementation is deferred and must not
        // be replaced with one independently generated mask per map chunk. Its future replacement depends on
        // global connected-river distance supplied by the procedural map assembler. Body Detail and the
        // separate white Current Accents remain approved and active.
        [Range(0f, 4f)]
        [SerializeField] private float flowSpeed = 0.75f;

        [Range(0.5f, 12f)]
        [SerializeField] private float flowScale = 4.5f;

        [Range(0f, 1f)]
        [SerializeField] private float flowStrength = 0.32f;

        [Range(0.15f, 4f)]
        [SerializeField] private float detailScale = 0.85f;

        [Range(0f, 1f)]
        [SerializeField] private float detailStrength = 0.48f;

        [Range(0f, 0.18f)]
        [SerializeField] private float waveHeight = 0.05f;

        [Range(0f, 1f)]
        [SerializeField] private float bankLight = 0.35f;

        [Range(1f, 6f)]
        [SerializeField] private float lightingSteps = 3f;

        [Header("Current Accents")]
        [SerializeField] private bool enableCurrentAccents = true;
        [SerializeField] private Color currentColor = Color.white;

        [Range(0f, 2f)]
        [SerializeField] private float currentIntensity = 1f;

        [Range(0f, 1f)]
        [SerializeField] private float currentOpacity = 0.92f;

        [Range(0f, 4f)]
        [SerializeField] private float currentSpeed = 0.9f;

        [Tooltip("How many accent ribbons are generated per metre of river length.")]
        [Range(0.05f, 4f)]
        [SerializeField] private float currentDensity = 1.1f;

        [Range(0.2f, 8f)]
        [SerializeField] private float currentLength = 2.2f;

        [Range(0.02f, 1f)]
        [SerializeField] private float currentWidth = 0.18f;

        [Range(0f, 1f)]
        [SerializeField] private float currentCurvature = 0.18f;

        [Range(0f, 1f)]
        [SerializeField] private float currentSoftness = 0.25f;

        [Header("Advanced")]
        [SerializeField] private Material bodyMaterial;
        [SerializeField] private Material currentMaterial;
        // Retained only for the provisional local-slice Body Flow path until procedural sampling can use
        // global connected-river distance. Do not swap this for one unrelated generated mask per chunk.
        [SerializeField] private Texture2D flowTexture;
        [SerializeField] private Texture2D detailTexture;

        [Range(0.001f, 0.08f)]
        [SerializeField] private float currentVerticalOffset = 0.018f;

        [Range(1, 9999)]
        [SerializeField] private int visualSeed = 1731;

        private static readonly int ShallowColorId = Shader.PropertyToID("_ShallowColor");
        private static readonly int DeepColorId = Shader.PropertyToID("_DeepColor");
        private static readonly int FlowTintId = Shader.PropertyToID("_FlowTint");
        private static readonly int OpacityId = Shader.PropertyToID("_Opacity");
        private static readonly int FlowScaleId = Shader.PropertyToID("_FlowScale");
        private static readonly int FlowStrengthId = Shader.PropertyToID("_FlowStrength");
        private static readonly int DetailScaleId = Shader.PropertyToID("_DetailScale");
        private static readonly int DetailStrengthId = Shader.PropertyToID("_DetailStrength");
        private static readonly int WaveHeightId = Shader.PropertyToID("_WaveHeight");
        private static readonly int BankLightId = Shader.PropertyToID("_BankLight");
        private static readonly int LightingStepsId = Shader.PropertyToID("_LightingSteps");
        private static readonly int FlowDistanceId = Shader.PropertyToID("_FlowDistance");
        private static readonly int RiverTimeId = Shader.PropertyToID("_RiverTime");
        private static readonly int FlowTextureId = Shader.PropertyToID("_FlowTex");
        private static readonly int DetailTextureId = Shader.PropertyToID("_DetailTex");
        private static readonly int VisualSeedId = Shader.PropertyToID("_VisualSeed");

        private static readonly int AccentColorId = Shader.PropertyToID("_AccentColor");
        private static readonly int AccentIntensityId = Shader.PropertyToID("_AccentIntensity");
        private static readonly int AccentOpacityId = Shader.PropertyToID("_AccentOpacity");
        private static readonly int EdgeSoftnessId = Shader.PropertyToID("_EdgeSoftness");

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh surfaceMesh;

        private MeshFilter currentFilter;
        private MeshRenderer currentRenderer;
        private Mesh currentMesh;

        private Material temporaryBodyMaterial;
        private Material temporaryCurrentMaterial;
        private MaterialPropertyBlock bodyProperties;
        private MaterialPropertyBlock currentProperties;

        private readonly List<StylizedRiverSplineSample> splineSamples = new List<StylizedRiverSplineSample>();

        private float riverLength;
        private float flowDistance;
        private float currentTravelDistance;
        private float riverTime;
        private double lastEditorTime;
        private double pendingRegenerationTime;
        private bool pendingRegeneration;
        private bool subscribedToSplineChanges;
        private int generatedAccentCount;

        public SplineContainer SplineContainer => ResolveSplineContainer();
        public float Width => width;
        public float BankBlend => bankBlend;
        public float Depth => depth;
        public float RiverLength => riverLength;
        public int CurrentAccentCount => generatedAccentCount;

        public int SurfaceTriangleCount =>
            surfaceMesh != null && surfaceMesh.subMeshCount > 0
                ? (int)surfaceMesh.GetIndexCount(0) / 3
                : 0;

        public int CurrentTriangleCount =>
            currentMesh != null && currentMesh.subMeshCount > 0
                ? (int)currentMesh.GetIndexCount(0) / 3
                : 0;

        private void Reset()
        {
            splineContainer = GetComponent<SplineContainer>();
        }

        private void OnEnable()
        {
            CacheComponents();
            ResolveSplineContainer();
            SubscribeToSplineChanges();
            EnsureGeneratedObjects();
            SetRenderersEnabled(true);
            RegenerateAll();
            lastEditorTime = Time.realtimeSinceStartupAsDouble;
        }

        private void OnDisable()
        {
            UnsubscribeFromSplineChanges();
            SetRenderersEnabled(false);
        }

        private void OnValidate()
        {
            ValidateSettings();
            CacheComponents();
            ResolveSplineContainer();
            EnsureGeneratedObjects();
            ApplyVisualSettings();

            if (liveRegeneration)
            {
                RequestRegeneration();
            }
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
                RegenerateAll();
            }

            AdvanceAnimation(deltaTime);
        }

        [ContextMenu("Regenerate River and Ground")]
        public void RegenerateAll()
        {
            ValidateSettings();
            CacheComponents();
            ResolveSplineContainer();
            EnsureGeneratedObjects();
            ResolveDefaultTextures();
            BuildSplineSamples();
            BuildSurface();
            BuildCurrentAccents();
            NotifyParentGround();
            ApplyVisualSettings();
        }

        [ContextMenu("Rebuild Surface Only")]
        public void RebuildSurfaceOnly()
        {
            ValidateSettings();
            CacheComponents();
            ResolveSplineContainer();
            EnsureGeneratedObjects();
            BuildSplineSamples();
            BuildSurface();
            ApplyVisualSettings();
        }

        [ContextMenu("Rebuild Current Accents Only")]
        public void RebuildCurrentAccentsOnly()
        {
            ValidateSettings();
            CacheComponents();
            ResolveSplineContainer();
            EnsureGeneratedObjects();

            if (splineSamples.Count < 2)
            {
                BuildSplineSamples();
            }

            BuildCurrentAccents();
            ApplyCurrentProperties();
        }

        [ContextMenu("Clear Generated River")]
        public void ClearGenerated()
        {
            if (surfaceMesh != null)
            {
                surfaceMesh.Clear();
            }

            if (currentMesh != null)
            {
                currentMesh.Clear();
            }

            splineSamples.Clear();
            riverLength = 0f;
            generatedAccentCount = 0;
        }

        public StylizedRiverGroundSnapshot CreateGroundSnapshot(Transform groundTransform)
        {
            if (groundTransform == null)
            {
                throw new ArgumentNullException(nameof(groundTransform));
            }

            SplineContainer container = ResolveSplineContainer();

            if (container == null || container.Splines.Count == 0)
            {
                return default;
            }

            List<StylizedRiverSplineSample> groundSamples = new List<StylizedRiverSplineSample>();

            StylizedRiverGeometry.BuildSplineSamples(
                container,
                ResolveGroundSampleSpacing(),
                groundSamples);

            Vector3[] localPoints = new Vector3[groundSamples.Count];

            for (int index = 0; index < groundSamples.Count; index++)
            {
                localPoints[index] = groundTransform.InverseTransformPoint(groundSamples[index].Centre);
            }

            return new StylizedRiverGroundSnapshot(
                localPoints,
                width,
                bankBlend,
                depth,
                bedFlatness,
                bankProfile,
                carvingStrength);
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

        private void ValidateSettings()
        {
            width = Mathf.Max(0.5f, width);
            bankBlend = Mathf.Max(0.1f, bankBlend);
            depth = Mathf.Max(0.1f, depth);
            bedFlatness = Mathf.Clamp01(bedFlatness);
            bankOverlap = Mathf.Clamp(bankOverlap, 0f, 0.8f);
            carvingStrength = Mathf.Clamp01(carvingStrength);
            surfaceOffset = Mathf.Clamp(surfaceOffset, 0f, 0.25f);
            opacity = Mathf.Clamp(opacity, 0.15f, 1f);
            flowSpeed = Mathf.Clamp(flowSpeed, 0f, 4f);
            flowScale = Mathf.Clamp(flowScale, 0.5f, 12f);
            flowStrength = Mathf.Clamp01(flowStrength);
            detailScale = Mathf.Clamp(detailScale, 0.15f, 4f);
            detailStrength = Mathf.Clamp01(detailStrength);
            waveHeight = Mathf.Clamp(waveHeight, 0f, 0.18f);
            bankLight = Mathf.Clamp01(bankLight);
            lightingSteps = Mathf.Clamp(lightingSteps, 1f, 6f);
            currentIntensity = Mathf.Clamp(currentIntensity, 0f, 2f);
            currentOpacity = Mathf.Clamp01(currentOpacity);
            currentSpeed = Mathf.Clamp(currentSpeed, 0f, 4f);
            currentDensity = Mathf.Clamp(currentDensity, 0.05f, 4f);
            currentLength = Mathf.Clamp(currentLength, 0.2f, 8f);
            currentWidth = Mathf.Clamp(currentWidth, 0.02f, 1f);
            currentCurvature = Mathf.Clamp01(currentCurvature);
            currentSoftness = Mathf.Clamp01(currentSoftness);
            currentVerticalOffset = Mathf.Clamp(currentVerticalOffset, 0.001f, 0.08f);
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

        private void ResolveDefaultTextures()
        {
            if (flowTexture == null)
            {
                flowTexture = Resources.Load<Texture2D>(FlowTextureResourcePath);
            }

            if (detailTexture == null)
            {
                detailTexture = Resources.Load<Texture2D>(DetailTextureResourcePath);
            }
        }

        private void SetRenderersEnabled(bool enabled)
        {
            if (meshRenderer != null)
            {
                meshRenderer.enabled = enabled;
            }

            if (currentRenderer != null)
            {
                currentRenderer.enabled = enabled;
            }
        }

        private void EnsureGeneratedObjects()
        {
            CacheComponents();
            EnsureSurfaceMesh();
            EnsureCurrentOutput();

            Material resolvedBody = ResolveBodyMaterial();
            Material resolvedCurrent = ResolveCurrentMaterial();

            if (meshRenderer != null)
            {
                meshRenderer.sharedMaterial = resolvedBody;
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                meshRenderer.receiveShadows = false;
                meshRenderer.sortingOrder = 0;
            }

            if (currentRenderer != null)
            {
                currentRenderer.sharedMaterial = resolvedCurrent;
                currentRenderer.shadowCastingMode = ShadowCastingMode.Off;
                currentRenderer.receiveShadows = false;
                currentRenderer.sortingOrder = 1;
            }
        }

        private void EnsureSurfaceMesh()
        {
            if (surfaceMesh == null)
            {
                surfaceMesh = new Mesh
                {
                    name = "PS3D_CleanRiverSurface",
                    hideFlags = HideFlags.DontSave
                };

                surfaceMesh.MarkDynamic();
            }

            if (meshFilter != null)
            {
                meshFilter.sharedMesh = surfaceMesh;
            }
        }

        private void EnsureCurrentOutput()
        {
            Transform child = transform.Find(CurrentObjectName);
            GameObject output;

            if (child == null)
            {
                output = new GameObject(CurrentObjectName);
                output.transform.SetParent(transform, false);
                output.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
            }
            else
            {
                output = child.gameObject;
            }

            output.layer = gameObject.layer;
            currentFilter = output.GetComponent<MeshFilter>();
            currentRenderer = output.GetComponent<MeshRenderer>();

            if (currentFilter == null)
            {
                currentFilter = output.AddComponent<MeshFilter>();
            }

            if (currentRenderer == null)
            {
                currentRenderer = output.AddComponent<MeshRenderer>();
            }

            if (currentMesh == null)
            {
                currentMesh = new Mesh
                {
                    name = "PS3D_CleanRiverCurrentAccents",
                    hideFlags = HideFlags.DontSave
                };

                currentMesh.MarkDynamic();
            }

            currentFilter.sharedMesh = currentMesh;
        }

        private Material ResolveBodyMaterial()
        {
            if (bodyMaterial != null)
            {
                DestroyTemporaryMaterial(ref temporaryBodyMaterial);
                return bodyMaterial;
            }

            if (temporaryBodyMaterial != null)
            {
                return temporaryBodyMaterial;
            }

            Shader shader = Resources.Load<Shader>(BodyShaderResourcePath);

            if (shader == null)
            {
                shader = Shader.Find("PS3D/Clean Stylized River");
            }

            if (shader == null)
            {
                return null;
            }

            temporaryBodyMaterial = new Material(shader)
            {
                name = "M_PS3D_CleanRiver_Temporary",
                hideFlags = HideFlags.DontSave
            };

            return temporaryBodyMaterial;
        }

        private Material ResolveCurrentMaterial()
        {
            if (currentMaterial != null)
            {
                DestroyTemporaryMaterial(ref temporaryCurrentMaterial);
                return currentMaterial;
            }

            if (temporaryCurrentMaterial != null)
            {
                return temporaryCurrentMaterial;
            }

            Shader shader = Resources.Load<Shader>(CurrentShaderResourcePath);

            if (shader == null)
            {
                shader = Shader.Find("PS3D/Clean Stylized River Current");
            }

            if (shader == null)
            {
                return null;
            }

            temporaryCurrentMaterial = new Material(shader)
            {
                name = "M_PS3D_CleanRiverCurrent_Temporary",
                hideFlags = HideFlags.DontSave
            };

            return temporaryCurrentMaterial;
        }

        private void BuildSplineSamples()
        {
            riverLength = StylizedRiverGeometry.BuildSplineSamples(
                ResolveSplineContainer(),
                ResolveSurfaceSampleSpacing(),
                splineSamples);
        }

        private void BuildSurface()
        {
            StylizedRiverGeometry.BuildSurfaceMesh(
                transform,
                splineSamples,
                width,
                bankOverlap,
                ResolveCrossSegments(),
                surfaceOffset,
                surfaceMesh);
        }

        private void BuildCurrentAccents()
        {
            if (!enableCurrentAccents)
            {
                generatedAccentCount = 0;

                if (currentMesh != null)
                {
                    currentMesh.Clear();
                }

                return;
            }

            StylizedRiverGeometry.BuildCurrentAccentMesh(
                transform,
                splineSamples,
                riverLength,
                width,
                surfaceOffset + currentVerticalOffset,
                currentDensity,
                currentLength,
                currentWidth,
                currentCurvature,
                1f,
                currentTravelDistance,
                visualSeed,
                ResolveCurrentRows(),
                currentMesh);

            generatedAccentCount =
                currentMesh != null
                    ? Mathf.Max(0, currentMesh.vertexCount / (ResolveCurrentRows() + 1) / 2)
                    : 0;
        }

        private void AdvanceAnimation(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            riverTime = Mathf.Repeat(riverTime + deltaTime, 4096f);
            flowDistance = Mathf.Repeat(flowDistance + flowSpeed * deltaTime, Mathf.Max(64f, flowScale * 256f));
            currentTravelDistance = Mathf.Repeat(currentTravelDistance + currentSpeed * deltaTime, Mathf.Max(64f, riverLength + currentLength * 4f));

            if (enableCurrentAccents && splineSamples.Count >= 2)
            {
                BuildCurrentAccents();
            }

            ApplyAnimationClock();
        }

        private void ApplyVisualSettings()
        {
            ResolveDefaultTextures();
            EnsureGeneratedObjects();
            ApplyBodyProperties();
            ApplyCurrentProperties();
        }

        private void ApplyBodyProperties()
        {
            if (meshRenderer == null)
            {
                return;
            }

            bodyProperties ??= new MaterialPropertyBlock();
            bodyProperties.Clear();
            bodyProperties.SetColor(ShallowColorId, shallowColor);
            bodyProperties.SetColor(DeepColorId, deepColor);
            bodyProperties.SetColor(FlowTintId, flowTint);
            bodyProperties.SetFloat(OpacityId, opacity);
            bodyProperties.SetFloat(FlowScaleId, flowScale);
            bodyProperties.SetFloat(FlowStrengthId, flowStrength);
            bodyProperties.SetFloat(DetailScaleId, detailScale);
            bodyProperties.SetFloat(DetailStrengthId, detailStrength);
            bodyProperties.SetFloat(WaveHeightId, waveHeight);
            bodyProperties.SetFloat(BankLightId, bankLight);
            bodyProperties.SetFloat(LightingStepsId, lightingSteps);
            // Retain the current Body Flow shader inputs only as a provisional local-slice implementation.
            // The future replacement must be driven by global connected-river distance from the procedural
            // map assembler, while Body Detail and Current Accents remain part of the accepted baseline.
            bodyProperties.SetFloat(FlowDistanceId, flowDistance);
            bodyProperties.SetFloat(RiverTimeId, riverTime);
            bodyProperties.SetFloat(VisualSeedId, visualSeed);

            if (flowTexture != null)
            {
                bodyProperties.SetTexture(FlowTextureId, flowTexture);
            }

            if (detailTexture != null)
            {
                bodyProperties.SetTexture(DetailTextureId, detailTexture);
            }

            meshRenderer.SetPropertyBlock(bodyProperties);
        }

        private void ApplyCurrentProperties()
        {
            if (currentRenderer == null)
            {
                return;
            }

            currentProperties ??= new MaterialPropertyBlock();
            currentProperties.Clear();
            currentProperties.SetColor(AccentColorId, currentColor);
            currentProperties.SetFloat(AccentIntensityId, currentIntensity);
            currentProperties.SetFloat(AccentOpacityId, currentOpacity);
            currentProperties.SetFloat(EdgeSoftnessId, currentSoftness);
            currentProperties.SetFloat(FlowDistanceId, flowDistance);
            currentProperties.SetFloat(RiverTimeId, riverTime);
            currentProperties.SetFloat(VisualSeedId, visualSeed);
            currentRenderer.SetPropertyBlock(currentProperties);
        }

        private void ApplyAnimationClock()
        {
            if (meshRenderer != null)
            {
                bodyProperties ??= new MaterialPropertyBlock();
                meshRenderer.GetPropertyBlock(bodyProperties);
                bodyProperties.SetFloat(FlowDistanceId, flowDistance);
                bodyProperties.SetFloat(RiverTimeId, riverTime);
                meshRenderer.SetPropertyBlock(bodyProperties);
            }

            if (currentRenderer != null)
            {
                currentProperties ??= new MaterialPropertyBlock();
                currentRenderer.GetPropertyBlock(currentProperties);
                currentProperties.SetFloat(FlowDistanceId, flowDistance);
                currentProperties.SetFloat(RiverTimeId, riverTime);
                currentRenderer.SetPropertyBlock(currentProperties);
            }
        }

        private void RequestRegeneration()
        {
            pendingRegeneration = true;
            pendingRegenerationTime = Time.realtimeSinceStartupAsDouble + 0.08;
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

        private void OnSplineChanged(Spline spline, int knotIndex, SplineModification modification)
        {
            if (!liveRegeneration || !UsesSpline(spline))
            {
                return;
            }

            RequestRegeneration();
        }

        private void NotifyParentGround()
        {
            GeneratedGround ground = GetComponentInParent<GeneratedGround>();

            if (ground != null)
            {
                ground.NotifyRiverChanged(this);
            }
        }

        private float ResolveSurfaceSampleSpacing()
        {
            return quality switch
            {
                StylizedRiverQuality.Low => 1.2f,
                StylizedRiverQuality.Medium => 0.6f,
                StylizedRiverQuality.High => 0.3f,
                _ => 0.6f
            };
        }

        private float ResolveGroundSampleSpacing()
        {
            return quality switch
            {
                StylizedRiverQuality.Low => 1.5f,
                StylizedRiverQuality.Medium => 0.75f,
                StylizedRiverQuality.High => 0.4f,
                _ => 0.75f
            };
        }

        private int ResolveCrossSegments()
        {
            return quality switch
            {
                StylizedRiverQuality.Low => 4,
                StylizedRiverQuality.Medium => 8,
                StylizedRiverQuality.High => 12,
                _ => 8
            };
        }

        private int ResolveCurrentRows()
        {
            return quality switch
            {
                StylizedRiverQuality.Low => 4,
                StylizedRiverQuality.Medium => 7,
                StylizedRiverQuality.High => 10,
                _ => 7
            };
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

            DestroyGeneratedMesh(ref surfaceMesh);
            DestroyGeneratedMesh(ref currentMesh);
            DestroyTemporaryMaterial(ref temporaryBodyMaterial);
            DestroyTemporaryMaterial(ref temporaryCurrentMaterial);

            Transform child = transform.Find(CurrentObjectName);

            if (child != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private static void DestroyGeneratedMesh(ref Mesh mesh)
        {
            if (mesh == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(mesh);
            }
            else
            {
                DestroyImmediate(mesh);
            }

            mesh = null;
        }
    }
}
