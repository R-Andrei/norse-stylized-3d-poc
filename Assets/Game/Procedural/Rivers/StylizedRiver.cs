using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
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

    public enum StylizedRiverWaterBodyPreset
    {
        ClearStream,
        BalancedRiver,
        DeepWater,
        Custom
    }

    public enum StylizedRiverBodyDebugView
    {
        Final = 0,
        VerticalDepth = 1,
        DepthBlend = 2,
        Transmission = 3,
        BodyCoverage = 4,
        SceneColour = 5,
        DepthValidity = 6,
        SurfaceCoverage = 7
    }

    public enum StylizedRiverDebugView
    {
        Final = 0,
        Depth = 1,
        Normals = 2,
        FoamState = 3,
        FinalFoam = 4,
        Refraction = 5,
        PlanarReflection = 6,
        WaveEdgeMask = 7
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SplineContainer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class StylizedRiver : MonoBehaviour
    {
        public const string CompatibleShaderName =
            "PS3D/Stylized River Water";

        private const string LegacyCurrentObjectName =
            "__PS3D_RiverCurrentAccents";

        private const string LegacyStaticFoamObjectName =
            "__PS3D_RiverStaticFoam";

        private const string BodyShaderResourcePath =
            "PS3DRiver/Shaders/SH_CleanStylizedRiver";

        private const string NormalTextureResourcePath =
            "PS3DRiver/Textures/T_RiverNormal";


        [Header("Setup")]
        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private bool liveRegeneration = true;
        [SerializeField] private bool reverseFlow;

        [Header("River Domain")]
        [Tooltip("World-space metres between authoritative river-domain samples.")]
        [Min(0.05f)]
        [SerializeField] private float domainSampleSpacing = 0.5f;

        [Tooltip("Cumulative distance assigned by a future connected-river assembler.")]
        [SerializeField] private float connectedRiverDistanceOffset;

        [Header("Channel")]
        [Range(0.5f, 20f)]
        [SerializeField] private float width = 4f;

        [Range(0.1f, 12f)]
        [SerializeField] private float bankBlend = 2.5f;

        [Range(0.1f, 6f)]
        [SerializeField] private float depth = 1.1f;

        [Range(0f, 1f)]
        [SerializeField] private float bedFlatness = 0.62f;

        [SerializeField]
        private StylizedRiverBankProfile bankProfile =
            StylizedRiverBankProfile.Natural;

        [Range(0f, 0.8f)]
        [SerializeField] private float bankOverlap = 0.22f;

        [Range(0f, 1f)]
        [SerializeField] private float carvingStrength = 1f;

        [Header("Surface Mesh")]
        [SerializeField]
        private StylizedRiverQuality quality =
            StylizedRiverQuality.Medium;

        [Tooltip("Raises water above the carved bed to avoid depth fighting.")]
        [Range(0f, 0.25f)]
        [SerializeField] private float surfaceOffset = 0.035f;

        [Header("Water Body")]
        [SerializeField]
        private StylizedRiverWaterBodyPreset bodyPreset =
            StylizedRiverWaterBodyPreset.BalancedRiver;

        [SerializeField]
        private Color shallowColor =
            new Color(0.458f, 0.802f, 0.798f, 1f);

        [SerializeField]
        private Color deepColor =
            new Color(0f, 0.310f, 0.594f, 1f);

        [Tooltip("How strongly the riverbed remains visible through the water.")]
        [Range(0f, 1f)]
        [SerializeField] private float clarity = 0.62f;

        [Tooltip("World-space vertical depth at which the body reaches its deep-water appearance.")]
        [Range(0.1f, 8f)]
        [SerializeField] private float bodyDepthRange = 1.4f;

        [Tooltip("Controls whether the shallow-to-deep transition is soft or pronounced.")]
        [Range(0f, 1f)]
        [SerializeField] private float bodyDepthContrast = 0.5f;

        [Tooltip("Controls how strongly the water volume colours the scene beneath it.")]
        [Range(0f, 1f)]
        [FormerlySerializedAs("bodyStrength")]
        [SerializeField] private float waterTintStrength = 0.72f;

        [Tooltip("Controls how clearly the air-water boundary remains visible, even in shallow clear water.")]
        [Range(0f, 1f)]
        [SerializeField] private float surfacePresence = 0.46f;

        [Header("Water Body Validation")]
        [SerializeField]
        private StylizedRiverBodyDebugView bodyDebugView =
            StylizedRiverBodyDebugView.Final;

        // Deferred-stage settings are retained for serialized compatibility.
        // The Stage 2 shader intentionally does not consume them yet.
        [SerializeField]
        private Color horizonColor =
            new Color(0.58f, 0.91f, 0.94f, 0.35f);

        [FormerlySerializedAs("waterHighlightColor")]
        [SerializeField]
        private Color specularColor =
            new Color(1f, 1f, 1f, 0.35f);

        [Range(0f, 1f)]
        [SerializeField] private float opacity = 0.82f;

        [Range(0f, 1f)]
        [SerializeField] private float shallowOpacity = 0.42f;

        [Range(0f, 1f)]
        [SerializeField] private float deepOpacity = 0.82f;

        [Min(0.01f)]
        [SerializeField] private float depthFadeDistance = 0.4f;

        [Tooltip("Zero or one keeps a continuous gradient. Two or more posterizes the depth colour.")]
        [Range(0f, 12f)]
        [SerializeField] private float depthBands;

        [SerializeField] private bool useHsvColorBlend = true;

        [Range(0.25f, 20f)]
        [SerializeField] private float horizonPower = 5.1f;

        [Header("Refraction")]
        [Min(0.0001f)]
        [SerializeField] private float refractionScale = 0.01f;

        [Range(0f, 2f)]
        [SerializeField] private float refractionSpeed = 0.053f;

        [Range(0f, 0.05f)]
        [SerializeField] private float refractionStrength = 0.0065f;

        [Header("Surface Normals")]
        [SerializeField] private Texture2D normalTexture;

        [Min(0.0001f)]
        [SerializeField] private float normalScale = 0.012f;

        [Range(0f, 2f)]
        [SerializeField] private float normalSpeed = 0.073f;

        [Range(0f, 2f)]
        [SerializeField] private float normalStrength = 0.277f;

        [Header("Gerstner Waves")]
        [Range(0.15f, 12f)]
        [SerializeField] private float waveScale = 2.8f;

        [Range(0f, 4f)]
        [SerializeField] private float waveSpeed = 0.57f;

        [Range(0f, 0.5f)]
        [SerializeField] private float waveHeight = 0.088f;

        [SerializeField]
        private Vector4 waveDirections =
            new Vector4(0f, 0.5f, 1f, 0.2f);

        [Tooltip("Normalized distance from the centre where wave damping begins. One is the outer bank edge.")]
        [Range(0f, 0.99f)]
        [SerializeField] private float waveEdgeDampingStart = 0.65f;

        [Range(0f, 1f)]
        [SerializeField] private float waveHeightColorStrength = 0.12f;

        [Header("Lighting")]
        [Range(0f, 1f)]
        [SerializeField] private float lightingSmoothness = 0.587f;

        [Range(0f, 1f)]
        [SerializeField] private float lightingHardness = 1f;

        [Range(0f, 4f)]
        [SerializeField] private float specularStrength = 0.65f;

        [Range(1f, 8f)]
        [SerializeField] private float lightingSteps = 4f;

        [Header("Advanced")]
        [SerializeField] private Material bodyMaterial;

        [Range(1, 9999)]
        [SerializeField] private int visualSeed = 1731;

        [SerializeField]
        private StylizedRiverDebugView debugView =
            StylizedRiverDebugView.Final;

        private Texture statefulFoamTexture;
        private Color statefulFoamColor = new Color(0.94f, 0.985f, 1f, 0.78f);
        private float statefulFoamStrength;
        private float statefulFoamThreshold = 0.16f;
        private float statefulFoamSoftness = 0.025f;
        private float statefulFoamBandWidth = 0.14f;
        private float statefulFoamContactStrength = 0.16f;
        private float statefulFoamContactDepth = 0.20f;

        private static readonly int ShallowColorId = Shader.PropertyToID("_ShallowColor");
        private static readonly int DeepColorId = Shader.PropertyToID("_DeepColor");
        private static readonly int ClarityId = Shader.PropertyToID("_Clarity");
        private static readonly int BodyDepthRangeId = Shader.PropertyToID("_BodyDepthRange");
        private static readonly int BodyDepthContrastId = Shader.PropertyToID("_BodyDepthContrast");
        private static readonly int WaterTintStrengthId = Shader.PropertyToID("_WaterTintStrength");
        private static readonly int SurfacePresenceId = Shader.PropertyToID("_SurfacePresence");
        private static readonly int DomainFallbackDepthId = Shader.PropertyToID("_DomainFallbackDepth");
        private static readonly int BodyDebugViewId = Shader.PropertyToID("_BodyDebugView");
        private static readonly int HorizonColorId = Shader.PropertyToID("_HorizonColor");
        private static readonly int SpecularColorId = Shader.PropertyToID("_SpecularColor");
        private static readonly int OpacityId = Shader.PropertyToID("_Opacity");
        private static readonly int ShallowOpacityId = Shader.PropertyToID("_ShallowOpacity");
        private static readonly int DeepOpacityId = Shader.PropertyToID("_DeepOpacity");
        private static readonly int DepthFadeDistanceId = Shader.PropertyToID("_DepthFadeDistance");
        private static readonly int DepthBandsId = Shader.PropertyToID("_DepthBands");
        private static readonly int UseHsvBlendId = Shader.PropertyToID("_UseHSVBlend");
        private static readonly int HorizonPowerId = Shader.PropertyToID("_HorizonPower");

        private static readonly int RefractionScaleId = Shader.PropertyToID("_RefractionScale");
        private static readonly int RefractionSpeedId = Shader.PropertyToID("_RefractionSpeed");
        private static readonly int RefractionStrengthId = Shader.PropertyToID("_RefractionStrength");

        private static readonly int NormalTextureId = Shader.PropertyToID("_NormalTexture");
        private static readonly int NormalScaleId = Shader.PropertyToID("_NormalScale");
        private static readonly int NormalSpeedId = Shader.PropertyToID("_NormalSpeed");
        private static readonly int NormalStrengthId = Shader.PropertyToID("_NormalStrength");

        private static readonly int SurfaceFoamColorId = Shader.PropertyToID("_SurfaceFoamColor");
        private static readonly int SurfaceFoamColorBlendId = Shader.PropertyToID("_SurfaceFoamColorBlend");

        private static readonly int WaveLengthId = Shader.PropertyToID("_WaveLength");
        private static readonly int WaveSpeedId = Shader.PropertyToID("_WaveSpeed");
        private static readonly int WaveSteepnessId = Shader.PropertyToID("_WaveSteepness");
        private static readonly int WaveDirectionsId = Shader.PropertyToID("_WaveDirections");
        private static readonly int WaveEdgeDampingStartId = Shader.PropertyToID("_WaveEdgeDampingStart");
        private static readonly int WaveHeightColorStrengthId = Shader.PropertyToID("_WaveHeightColorStrength");

        private static readonly int LightingSmoothnessId = Shader.PropertyToID("_LightingSmoothness");
        private static readonly int LightingHardnessId = Shader.PropertyToID("_LightingHardness");
        private static readonly int SpecularStrengthId = Shader.PropertyToID("_SpecularStrength");
        private static readonly int LightingStepsId = Shader.PropertyToID("_LightingSteps");

        private static readonly int RiverWidthId = Shader.PropertyToID("_RiverWidth");
        private static readonly int RiverLengthId = Shader.PropertyToID("_RiverLength");
        private static readonly int FlowDirectionId = Shader.PropertyToID("_FlowDirection");
        private static readonly int RiverTimeId = Shader.PropertyToID("_RiverTime");
        private static readonly int VisualSeedId = Shader.PropertyToID("_VisualSeed");
        private static readonly int DebugViewId = Shader.PropertyToID("_DebugView");

        private static readonly int ExternalFoamFieldId = Shader.PropertyToID("_ExternalFoamField");
        private static readonly int ExternalFoamStrengthId = Shader.PropertyToID("_ExternalFoamStrength");
        private static readonly int ExternalFoamThresholdId = Shader.PropertyToID("_ExternalFoamThreshold");
        private static readonly int ExternalFoamSoftnessId = Shader.PropertyToID("_ExternalFoamSoftness");
        private static readonly int ExternalFoamBandWidthId = Shader.PropertyToID("_ExternalFoamBandWidth");
        private static readonly int ExternalFoamContactStrengthId = Shader.PropertyToID("_ExternalFoamContactStrength");
        private static readonly int ExternalFoamContactDepthId = Shader.PropertyToID("_ExternalFoamContactDepth");

        private static readonly int PlanarReflectionTextureId = Shader.PropertyToID("_PlanarReflectionTexture");
        private static readonly int PlanarReflectionVpId = Shader.PropertyToID("_PlanarReflectionVP");
        private static readonly int PlanarReflectionStrengthId = Shader.PropertyToID("_PlanarReflectionStrength");
        private static readonly int PlanarReflectionDistortionId = Shader.PropertyToID("_PlanarReflectionDistortion");
        private static readonly int PlanarReflectionAvailableId = Shader.PropertyToID("_PlanarReflectionAvailable");

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh surfaceMesh;
        private Material temporaryBodyMaterial;
        private MaterialPropertyBlock bodyProperties;
        private bool incompatibleMaterialWarningReported;
        private bool missingWaterLayerWarningReported;

        private Texture2D defaultNormalTexture;

        private Texture planarReflectionTexture;
        private Matrix4x4 planarReflectionVp = Matrix4x4.identity;
        private float planarReflectionStrength;
        private float planarReflectionDistortion;
        private bool planarReflectionAvailable;

        private RiverDomainSnapshot riverDomain = RiverDomainSnapshot.Empty;
        private int riverDomainVersion;

        private float riverLength;
        private float averageSurfaceHeight;
        private float riverTime;
        private double lastEditorTime;
        private double pendingRegenerationTime;
        private bool pendingRegeneration;
        private bool subscribedToSplineChanges;

        public event Action<RiverDomainSnapshot> DomainChanged;

        public SplineContainer SplineContainer => ResolveSplineContainer();
        public StylizedRiverQuality Quality => quality;
        public float Width => width;
        public float BankBlend => bankBlend;
        public float Depth => depth;
        public float SurfaceOffset => surfaceOffset;
        public StylizedRiverWaterBodyPreset BodyPreset => bodyPreset;
        public float Clarity => clarity;
        public float BodyDepthRange => bodyDepthRange;
        public float BodyDepthContrast => bodyDepthContrast;
        public float WaterTintStrength => waterTintStrength;
        public float SurfacePresence => surfacePresence;
        public float VisibleHalfWidth => width * 0.5f + bankOverlap;
        public float VisibleWidth => VisibleHalfWidth * 2f;
        public RiverDomainSnapshot Domain => riverDomain ?? RiverDomainSnapshot.Empty;
        public float DomainSampleSpacing => domainSampleSpacing;
        public float ConnectedRiverDistanceOffset => connectedRiverDistanceOffset;
        public float RiverLength => riverLength;
        public float GlobalDistanceMinimum => Domain.GlobalDistanceMinimum;
        public float GlobalDistanceMaximum => Domain.GlobalDistanceMaximum;
        public float AverageSurfaceHeight => averageSurfaceHeight;
        public int VisualSeed => visualSeed;
        public float FlowDirection => reverseFlow ? -1f : 1f;
        public MeshRenderer SurfaceRenderer => meshRenderer != null ? meshRenderer : GetComponent<MeshRenderer>();

        public int SurfaceTriangleCount =>
            surfaceMesh != null && surfaceMesh.subMeshCount > 0
                ? (int)surfaceMesh.GetIndexCount(0) / 3
                : 0;

        private void Reset()
        {
            splineContainer = GetComponent<SplineContainer>();
            AssignWaterLayer();
        }

        private void OnEnable()
        {
            CacheComponents();
            ResolveSplineContainer();
            AssignWaterLayer();
            SubscribeToSplineChanges();
            RemoveLegacyGeneratedObjects();
            EnsureSurfaceOutput();
            SetRendererEnabled(true);
            RegenerateAll();
            lastEditorTime = Time.realtimeSinceStartupAsDouble;
        }

        private void OnDisable()
        {
            UnsubscribeFromSplineChanges();
            SetRendererEnabled(false);
        }

        private void OnValidate()
        {
            ValidateSettings();
            CacheComponents();
            ResolveSplineContainer();
            AssignWaterLayer();
            RemoveLegacyGeneratedObjects();
            EnsureSurfaceOutput();
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

            if (deltaTime <= 0f)
            {
                return;
            }

            riverTime = Mathf.Repeat(riverTime + deltaTime, 4096f);
            ApplyAnimationClock();
        }

        [ContextMenu("Regenerate River and Ground")]
        public void RegenerateAll()
        {
            ValidateSettings();
            CacheComponents();
            ResolveSplineContainer();
            AssignWaterLayer();
            RemoveLegacyGeneratedObjects();
            EnsureSurfaceOutput();
            BuildRiverDomain();
            BuildSurface();
            NotifyParentGround();
            ApplyVisualSettings();
            NotifyReflectionSurfaceChanged();
            NotifyFoamSimulationChanged();
        }

        [ContextMenu("Rebuild Surface Only")]
        public void RebuildSurfaceOnly()
        {
            ValidateSettings();
            CacheComponents();
            ResolveSplineContainer();
            AssignWaterLayer();
            RemoveLegacyGeneratedObjects();
            EnsureSurfaceOutput();
            BuildRiverDomain();
            BuildSurface();
            ApplyVisualSettings();
            NotifyReflectionSurfaceChanged();
            NotifyFoamSimulationChanged();
        }

        [ContextMenu("Clear Generated River")]
        public void ClearGenerated()
        {
            if (surfaceMesh != null)
            {
                surfaceMesh.Clear();
            }

            riverDomainVersion++;
            riverDomain = new RiverDomainSnapshot(
                Array.Empty<StylizedRiverSplineSample>(),
                0f,
                domainSampleSpacing,
                connectedRiverDistanceOffset,
                reverseFlow,
                riverDomainVersion);
            riverLength = 0f;
            averageSurfaceHeight = transform.position.y + surfaceOffset;
            DomainChanged?.Invoke(Domain);
            ApplyVisualSettings();
            NotifyReflectionSurfaceChanged();
            NotifyFoamSimulationChanged();
        }


        public void ApplyWaterBodyPreset()
        {
            ApplyWaterBodyPreset(bodyPreset);
        }

        public void ApplyWaterBodyPreset(
            StylizedRiverWaterBodyPreset preset)
        {
            bodyPreset = preset;

            switch (preset)
            {
                case StylizedRiverWaterBodyPreset.ClearStream:
                    shallowColor = new Color(0.62f, 0.88f, 0.82f, 1f);
                    deepColor = new Color(0.18f, 0.55f, 0.62f, 1f);
                    clarity = 0.90f;
                    bodyDepthRange = 0.90f;
                    bodyDepthContrast = 0.24f;
                    waterTintStrength = 0.42f;
                    surfacePresence = 0.32f;
                    break;

                case StylizedRiverWaterBodyPreset.DeepWater:
                    shallowColor = new Color(0.28f, 0.62f, 0.68f, 1f);
                    deepColor = new Color(0.015f, 0.12f, 0.25f, 1f);
                    clarity = 0.28f;
                    bodyDepthRange = 2.40f;
                    bodyDepthContrast = 0.68f;
                    waterTintStrength = 0.90f;
                    surfacePresence = 0.58f;
                    break;

                case StylizedRiverWaterBodyPreset.BalancedRiver:
                    shallowColor = new Color(0.458f, 0.802f, 0.798f, 1f);
                    deepColor = new Color(0f, 0.310f, 0.594f, 1f);
                    clarity = 0.62f;
                    bodyDepthRange = 1.40f;
                    bodyDepthContrast = 0.50f;
                    waterTintStrength = 0.72f;
                    surfacePresence = 0.46f;
                    break;

                case StylizedRiverWaterBodyPreset.Custom:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(preset),
                        preset,
                        "Unsupported water-body preset.");
            }

            ValidateSettings();
            ApplyVisualSettings();
        }

        public void MarkWaterBodyCustom()
        {
            bodyPreset = StylizedRiverWaterBodyPreset.Custom;
        }

        public void ConfigureConnectedDomain(
            float distanceOffset,
            bool isReverseFlow)
        {
            bool changed =
                !Mathf.Approximately(
                    connectedRiverDistanceOffset,
                    distanceOffset) ||
                reverseFlow != isReverseFlow;

            if (!changed)
            {
                return;
            }

            connectedRiverDistanceOffset = distanceOffset;
            reverseFlow = isReverseFlow;
            RebuildSurfaceOnly();
        }

        public float BuildSharedSplineSamples(
            List<StylizedRiverSplineSample> targetSamples)
        {
            if (targetSamples == null)
            {
                throw new ArgumentNullException(nameof(targetSamples));
            }

            EnsureRiverDomain();
            targetSamples.Clear();

            for (int index = 0; index < Domain.SampleCount; index++)
            {
                targetSamples.Add(Domain.Samples[index]);
            }

            return Domain.LocalLength;
        }

        public bool TryProjectWorldPoint(
            Vector3 worldPoint,
            out StylizedRiverProjection projection)
        {
            EnsureRiverDomain();
            return Domain.TryProjectWorldPoint(worldPoint, out projection);
        }

        public StylizedRiverSplineSample SampleAtLocalDistance(
            float localDistance)
        {
            EnsureRiverDomain();
            return Domain.SampleAtLocalDistance(localDistance);
        }

        public StylizedRiverSplineSample SampleAtOrientedDistance(
            float orientedDistance)
        {
            EnsureRiverDomain();
            return Domain.SampleAtOrientedDistance(orientedDistance);
        }

        public StylizedRiverSplineSample SampleAtGlobalDistance(
            float globalDistance)
        {
            EnsureRiverDomain();
            return Domain.SampleAtGlobalDistance(globalDistance);
        }

        public Vector3 RiverToWorld(
            float localDistance,
            float acrossMetres,
            float heightOffset = 0f)
        {
            EnsureRiverDomain();
            return Domain.RiverToWorld(
                localDistance,
                acrossMetres,
                heightOffset);
        }

        [ContextMenu("Validate River Domain Contract")]
        public void ValidateRiverDomainContract()
        {
            EnsureRiverDomain();

            bool valid = Domain.ValidateContract(out string report);

            if (valid)
            {
                Debug.Log(report, this);
            }
            else
            {
                Debug.LogError(report, this);
            }
        }

        public StylizedRiverGroundSnapshot CreateGroundSnapshot(
            Transform groundTransform)
        {
            if (groundTransform == null)
            {
                throw new ArgumentNullException(nameof(groundTransform));
            }

            EnsureRiverDomain();

            if (!Domain.IsValid)
            {
                return default;
            }

            Vector3[] localPoints = new Vector3[Domain.SampleCount];
            float[] halfWidths = new float[Domain.SampleCount];

            for (int index = 0; index < Domain.SampleCount; index++)
            {
                StylizedRiverSplineSample sample = Domain.Samples[index];

                localPoints[index] =
                    groundTransform.InverseTransformPoint(
                        sample.Centre);

                halfWidths[index] = sample.HalfWidth;
            }

            return new StylizedRiverGroundSnapshot(
                localPoints,
                halfWidths,
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

        public bool TryGetSurfaceBounds(out Bounds bounds)
        {
            MeshRenderer renderer = SurfaceRenderer;

            if (renderer == null || surfaceMesh == null || surfaceMesh.vertexCount == 0)
            {
                bounds = default;
                return false;
            }

            bounds = renderer.bounds;
            return true;
        }

        public void SetExternalFoamTexture(Texture texture, float strength)
        {
            SetStatefulFoamTexture(
                texture,
                statefulFoamColor,
                strength,
                0.13f,
                0.025f,
                0.14f,
                0.16f,
                0.20f);
        }

        public void SetStatefulFoamTexture(
            Texture texture,
            Color color,
            float strength,
            float threshold,
            float softness,
            float bandWidth,
            float contactStrength,
            float contactDepth)
        {
            statefulFoamTexture = texture;
            statefulFoamStrength = Mathf.Clamp01(strength);
            statefulFoamColor = color;
            statefulFoamThreshold = Mathf.Clamp01(threshold);
            statefulFoamSoftness = Mathf.Clamp(softness, 0.001f, 0.25f);
            statefulFoamBandWidth = Mathf.Clamp(bandWidth, 0.01f, 0.5f);
            statefulFoamContactStrength = Mathf.Clamp01(contactStrength);
            statefulFoamContactDepth = Mathf.Max(0.001f, contactDepth);

            ApplyVisualSettings();
        }

        // Compatibility bridge for older callers.
        [Obsolete("Use SetExternalFoamTexture(Texture, float) instead.")]
        public void SetDynamicFoamTexture(Texture texture)
        {
            SetExternalFoamTexture(texture, texture != null ? 1f : 0f);
        }

        [Obsolete("Use RegenerateAll or SetExternalFoamTexture instead.")]
        public void RefreshFoamTextureBinding()
        {
            ApplyVisualSettings();
        }

        public void ClearExternalFoamTexture()
        {
            statefulFoamTexture = null;
            statefulFoamStrength = 0f;
            ApplyVisualSettings();
        }

        public void SetPlanarReflectionData(
            Texture texture,
            Matrix4x4 viewProjection,
            float strength,
            float distortion,
            bool available)
        {
            planarReflectionTexture = texture;
            planarReflectionVp = viewProjection;
            planarReflectionStrength = Mathf.Clamp01(strength);
            planarReflectionDistortion = Mathf.Clamp(distortion, 0f, 0.1f);
            planarReflectionAvailable = available && texture != null;
            ApplyBodyProperties();
        }

        public void ClearPlanarReflectionData()
        {
            planarReflectionTexture = null;
            planarReflectionVp = Matrix4x4.identity;
            planarReflectionStrength = 0f;
            planarReflectionDistortion = 0f;
            planarReflectionAvailable = false;
            ApplyBodyProperties();
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
            domainSampleSpacing = Mathf.Max(0.05f, domainSampleSpacing);

            clarity = Mathf.Clamp01(clarity);
            bodyDepthRange = Mathf.Clamp(bodyDepthRange, 0.1f, 8f);
            bodyDepthContrast = Mathf.Clamp01(bodyDepthContrast);
            waterTintStrength = Mathf.Clamp01(waterTintStrength);
            surfacePresence = Mathf.Clamp01(surfacePresence);

            opacity = Mathf.Clamp01(opacity);
            shallowOpacity = Mathf.Clamp01(shallowOpacity);
            deepOpacity = Mathf.Clamp01(deepOpacity);
            depthFadeDistance = Mathf.Max(0.01f, depthFadeDistance);
            depthBands = Mathf.Clamp(depthBands, 0f, 12f);
            horizonPower = Mathf.Clamp(horizonPower, 0.25f, 20f);

            refractionScale = Mathf.Max(0.0001f, refractionScale);
            refractionSpeed = Mathf.Clamp(refractionSpeed, 0f, 2f);
            refractionStrength = Mathf.Clamp(refractionStrength, 0f, 0.05f);

            normalScale = Mathf.Max(0.0001f, normalScale);
            normalSpeed = Mathf.Clamp(normalSpeed, 0f, 2f);
            normalStrength = Mathf.Clamp(normalStrength, 0f, 2f);

            waveScale = Mathf.Clamp(waveScale, 0.15f, 12f);
            waveSpeed = Mathf.Clamp(waveSpeed, 0f, 4f);
            waveHeight = Mathf.Clamp(waveHeight, 0f, 0.5f);
            waveEdgeDampingStart = Mathf.Clamp(waveEdgeDampingStart, 0f, 0.99f);
            waveHeightColorStrength = Mathf.Clamp01(waveHeightColorStrength);

            lightingSmoothness = Mathf.Clamp01(lightingSmoothness);
            lightingHardness = Mathf.Clamp01(lightingHardness);
            specularStrength = Mathf.Clamp(specularStrength, 0f, 4f);
            lightingSteps = Mathf.Clamp(lightingSteps, 1f, 8f);
            visualSeed = Mathf.Clamp(visualSeed, 1, 9999);
            statefulFoamStrength = Mathf.Clamp01(statefulFoamStrength);
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

        private void AssignWaterLayer()
        {
            int waterLayer = LayerMask.NameToLayer("Water");

            if (waterLayer < 0)
            {
                if (!missingWaterLayerWarningReported)
                {
                    Debug.LogWarning(
                        $"StylizedRiver on '{name}' could not find the required 'Water' layer.",
                        this);
                    missingWaterLayerWarningReported = true;
                }

                return;
            }

            missingWaterLayerWarningReported = false;
            gameObject.layer = waterLayer;
        }

        private void SetRendererEnabled(bool enabled)
        {
            if (meshRenderer != null)
            {
                meshRenderer.enabled = enabled;
            }
        }

        private void EnsureSurfaceOutput()
        {
            CacheComponents();

            if (surfaceMesh == null)
            {
                surfaceMesh = new Mesh
                {
                    name = "PS3D_StylizedRiverSurface",
                    hideFlags = HideFlags.DontSave
                };

                surfaceMesh.MarkDynamic();
            }

            if (meshFilter != null)
            {
                meshFilter.sharedMesh = surfaceMesh;
            }

            if (meshRenderer != null)
            {
                meshRenderer.sharedMaterial = ResolveBodyMaterial();
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                meshRenderer.receiveShadows = true;
                meshRenderer.sortingOrder = 0;
            }
        }

        private Material ResolveBodyMaterial()
        {
            if (bodyMaterial != null &&
                bodyMaterial.shader != null &&
                bodyMaterial.shader.name == CompatibleShaderName)
            {
                incompatibleMaterialWarningReported = false;
                DestroyTemporaryMaterial(ref temporaryBodyMaterial);
                return bodyMaterial;
            }

            if (bodyMaterial != null && !incompatibleMaterialWarningReported)
            {
                Debug.LogWarning(
                    $"StylizedRiver on '{name}' ignored Body Material Override '{bodyMaterial.name}' because it does not use shader '{CompatibleShaderName}'.",
                    this);

                incompatibleMaterialWarningReported = true;
            }

            if (temporaryBodyMaterial != null)
            {
                Shader temporaryShader = temporaryBodyMaterial.shader;

                if (temporaryShader != null &&
                    temporaryShader.name == CompatibleShaderName)
                {
                    return temporaryBodyMaterial;
                }

                // Unity hot reload can preserve private serializable fields. Reject a
                // cached temporary material left over from the deleted river shader.
                DestroyTemporaryMaterial(ref temporaryBodyMaterial);
            }

            Shader shader = Resources.Load<Shader>(BodyShaderResourcePath);

            if (shader == null)
            {
                shader = Shader.Find(CompatibleShaderName);
            }

            if (shader == null)
            {
                Debug.LogError(
                    $"StylizedRiver on '{name}' could not load shader '{CompatibleShaderName}'.",
                    this);
                return null;
            }

            temporaryBodyMaterial = new Material(shader)
            {
                name = "M_PS3D_StylizedRiver_Temporary",
                hideFlags = HideFlags.DontSave
            };

            return temporaryBodyMaterial;
        }

        private void ResolveDefaultTextures()
        {
            if (defaultNormalTexture == null)
            {
                defaultNormalTexture = Resources.Load<Texture2D>(NormalTextureResourcePath);
            }

        }

        private void EnsureRiverDomain()
        {
            if (riverDomain == null || !riverDomain.IsValid)
            {
                BuildRiverDomain();
            }
        }

        private void BuildRiverDomain()
        {
            riverDomainVersion++;

            riverDomain =
                StylizedRiverGeometry.BuildDomain(
                    ResolveSplineContainer(),
                    domainSampleSpacing,
                    width,
                    bankOverlap,
                    surfaceOffset,
                    connectedRiverDistanceOffset,
                    reverseFlow,
                    riverDomainVersion);

            riverLength = Domain.LocalLength;

            if (!Domain.IsValid)
            {
                averageSurfaceHeight = transform.position.y + surfaceOffset;
                DomainChanged?.Invoke(Domain);
                return;
            }

            double heightSum = 0.0;

            for (int index = 0; index < Domain.SampleCount; index++)
            {
                heightSum += Domain.Samples[index].SurfaceHeight;
            }

            averageSurfaceHeight =
                (float)(heightSum / Domain.SampleCount);

            DomainChanged?.Invoke(Domain);
        }

        private void BuildSurface()
        {
            StylizedRiverGeometry.BuildSurfaceMesh(
                transform,
                Domain,
                ResolveCrossSegments(),
                surfaceMesh);
        }

        private void ApplyVisualSettings()
        {
            EnsureSurfaceOutput();
            ApplyBodyProperties();
        }

        private void ApplyBodyProperties()
        {
            if (meshRenderer == null)
            {
                return;
            }

            ResolveDefaultTextures();

            bodyProperties ??= new MaterialPropertyBlock();
            bodyProperties.Clear();

            bodyProperties.SetColor(ShallowColorId, shallowColor);
            bodyProperties.SetColor(DeepColorId, deepColor);
            bodyProperties.SetFloat(ClarityId, clarity);
            bodyProperties.SetFloat(BodyDepthRangeId, bodyDepthRange);
            bodyProperties.SetFloat(BodyDepthContrastId, bodyDepthContrast);
            bodyProperties.SetFloat(WaterTintStrengthId, waterTintStrength);
            bodyProperties.SetFloat(SurfacePresenceId, surfacePresence);
            bodyProperties.SetFloat(DomainFallbackDepthId, Mathf.Max(0.01f, depth));
            bodyProperties.SetFloat(BodyDebugViewId, (float)bodyDebugView);
            bodyProperties.SetColor(HorizonColorId, horizonColor);
            bodyProperties.SetColor(SpecularColorId, specularColor);
            bodyProperties.SetFloat(OpacityId, opacity);
            bodyProperties.SetFloat(ShallowOpacityId, shallowOpacity);
            bodyProperties.SetFloat(DeepOpacityId, deepOpacity);
            bodyProperties.SetFloat(DepthFadeDistanceId, depthFadeDistance);
            bodyProperties.SetFloat(DepthBandsId, depthBands);
            bodyProperties.SetFloat(UseHsvBlendId, useHsvColorBlend ? 1f : 0f);
            bodyProperties.SetFloat(HorizonPowerId, horizonPower);

            bodyProperties.SetFloat(RefractionScaleId, refractionScale);
            bodyProperties.SetFloat(RefractionSpeedId, refractionSpeed);
            bodyProperties.SetFloat(RefractionStrengthId, refractionStrength);

            bodyProperties.SetTexture(
                NormalTextureId,
                normalTexture != null ? normalTexture : defaultNormalTexture);
            bodyProperties.SetFloat(NormalScaleId, normalScale);
            bodyProperties.SetFloat(NormalSpeedId, normalSpeed);
            bodyProperties.SetFloat(NormalStrengthId, normalStrength);

            bodyProperties.SetColor(SurfaceFoamColorId, statefulFoamColor);
            bodyProperties.SetFloat(SurfaceFoamColorBlendId, 1f);

            bodyProperties.SetFloat(WaveLengthId, waveScale);
            bodyProperties.SetFloat(WaveSpeedId, waveSpeed);
            bodyProperties.SetFloat(WaveSteepnessId, waveHeight);
            bodyProperties.SetVector(WaveDirectionsId, waveDirections);
            bodyProperties.SetFloat(WaveEdgeDampingStartId, waveEdgeDampingStart);
            bodyProperties.SetFloat(WaveHeightColorStrengthId, waveHeightColorStrength);

            bodyProperties.SetFloat(LightingSmoothnessId, lightingSmoothness);
            bodyProperties.SetFloat(LightingHardnessId, lightingHardness);
            bodyProperties.SetFloat(SpecularStrengthId, specularStrength);
            bodyProperties.SetFloat(LightingStepsId, lightingSteps);

            bodyProperties.SetFloat(RiverWidthId, Mathf.Max(0.01f, VisibleWidth));
            bodyProperties.SetFloat(RiverLengthId, Mathf.Max(0.01f, riverLength));
            bodyProperties.SetFloat(FlowDirectionId, FlowDirection);
            bodyProperties.SetFloat(RiverTimeId, riverTime);
            bodyProperties.SetFloat(VisualSeedId, visualSeed);
            bodyProperties.SetFloat(DebugViewId, (float)debugView);

            bodyProperties.SetTexture(
                ExternalFoamFieldId,
                statefulFoamTexture != null
                    ? statefulFoamTexture
                    : Texture2D.blackTexture);
            bodyProperties.SetFloat(ExternalFoamStrengthId, statefulFoamStrength);
            bodyProperties.SetFloat(ExternalFoamThresholdId, statefulFoamThreshold);
            bodyProperties.SetFloat(ExternalFoamSoftnessId, statefulFoamSoftness);
            bodyProperties.SetFloat(ExternalFoamBandWidthId, statefulFoamBandWidth);
            bodyProperties.SetFloat(ExternalFoamContactStrengthId, statefulFoamContactStrength);
            bodyProperties.SetFloat(ExternalFoamContactDepthId, statefulFoamContactDepth);

            bodyProperties.SetTexture(
                PlanarReflectionTextureId,
                planarReflectionTexture != null
                    ? planarReflectionTexture
                    : Texture2D.blackTexture);
            bodyProperties.SetMatrix(PlanarReflectionVpId, planarReflectionVp);
            bodyProperties.SetFloat(
                PlanarReflectionStrengthId,
                planarReflectionStrength);
            bodyProperties.SetFloat(
                PlanarReflectionDistortionId,
                planarReflectionDistortion);
            bodyProperties.SetFloat(
                PlanarReflectionAvailableId,
                planarReflectionAvailable ? 1f : 0f);

            meshRenderer.SetPropertyBlock(bodyProperties);
        }

        private void ApplyAnimationClock()
        {
            if (meshRenderer == null)
            {
                return;
            }

            bodyProperties ??= new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(bodyProperties);
            bodyProperties.SetFloat(RiverTimeId, riverTime);
            meshRenderer.SetPropertyBlock(bodyProperties);
        }

        private void NotifyFoamSimulationChanged()
        {
            StylizedRiverFoamSimulation simulation =
                GetComponent<StylizedRiverFoamSimulation>();

            if (simulation != null)
            {
                simulation.NotifyRiverChanged();
            }
        }

        private void RequestRegeneration()
        {
            pendingRegeneration = true;
            pendingRegenerationTime =
                Time.realtimeSinceStartupAsDouble + 0.08;
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

        private void OnSplineChanged(
            Spline spline,
            int knotIndex,
            SplineModification modification)
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

        private void NotifyReflectionSurfaceChanged()
        {
            StylizedRiverPlanarReflection reflection =
                GetComponent<StylizedRiverPlanarReflection>();

            if (reflection != null)
            {
                reflection.RequestRender();
            }
        }

        private void RemoveLegacyGeneratedObjects()
        {
            RemoveGeneratedChild(LegacyCurrentObjectName);
            RemoveGeneratedChild(LegacyStaticFoamObjectName);
        }

        private void RemoveGeneratedChild(string childName)
        {
            Transform child = transform.Find(childName);

            if (child == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }

        private int ResolveCrossSegments()
        {
            return quality switch
            {
                StylizedRiverQuality.Low => 6,
                StylizedRiverQuality.Medium => 12,
                StylizedRiverQuality.High => 20,
                _ => 12
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

            if (surfaceMesh != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(surfaceMesh);
                }
                else
                {
                    DestroyImmediate(surfaceMesh);
                }

                surfaceMesh = null;
            }

            DestroyTemporaryMaterial(ref temporaryBodyMaterial);
            RemoveLegacyGeneratedObjects();
        }
    }
}
