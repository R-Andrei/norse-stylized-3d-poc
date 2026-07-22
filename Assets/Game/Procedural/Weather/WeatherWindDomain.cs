using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ProgrammaticStylized3D.Weather
{
    public enum WeatherWindDebugView
    {
        Off = 0,
        WindField = 1,
        ResponseError = 2
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("PS3D/Weather/Weather Wind Domain")]
    public class WeatherWindDomain : MonoBehaviour
    {
        private const string ComputeResourcePath =
            "PS3DWeather/Compute/CS_WeatherWindField";
        private const int ThreadGroupSize = 8;

        private static readonly int TargetFieldId =
            Shader.PropertyToID("_WeatherWindTargetField");
        private static readonly int ResponseFieldId =
            Shader.PropertyToID("_WeatherWindResponseField");
        private static readonly int FieldOriginCellSizeId =
            Shader.PropertyToID("_WeatherWindFieldOriginCellSize");
        private static readonly int FieldResolutionOffsetId =
            Shader.PropertyToID("_WeatherWindFieldResolutionOffset");
        private static readonly int FieldTimingId =
            Shader.PropertyToID("_WeatherWindFieldTiming");

        private static readonly List<WeatherWindDomain> ActiveDomainsInternal =
            new List<WeatherWindDomain>();

        [Header("Domain Anchor")]
        [SerializeField]
        [Tooltip("Preferred XZ anchor for the moving wind field, normally the player or camera follow target. This avoids centring an isometric field on the camera's horizontally offset transform.")]
        private Transform fieldAnchor;

        [SerializeField]
        [Tooltip("Fallback camera used when no field anchor is assigned. Its forward ray is projected onto the XZ field plane. When unassigned, Camera.main is resolved once.")]
        private Camera targetCamera;

        [SerializeField]
        [Tooltip("World-space Y height of the horizontal plane used when projecting the fallback isometric camera to an XZ field centre.")]
        private float fieldPlaneY;

        [Header("Field Resolution")]
        [SerializeField, Range(32, 256)]
        private int fieldResolution = 128;

        [SerializeField, Min(0.1f)]
        private float cellSizeMetres = 0.5f;

        [SerializeField, Range(5f, 60f)]
        private float updateRateHz = 10f;

        [SerializeField, Range(1, 8)]
        private int maximumStepsPerFrame = 4;

        [Header("Prevailing Wind")]
        [SerializeField]
        private Vector2 prevailingDirection = new Vector2(1f, 0.25f);

        [SerializeField, Min(0f)]
        private float baseStrength = 0.07f;

        [Header("Broad XZ Variation")]
        [SerializeField, Min(1f)]
        private float broadNoiseScaleMetres = 12f;

        [SerializeField, Min(0f)]
        private float broadNoiseTravelSpeed = 1.1f;

        [SerializeField, Min(0f)]
        private float turbulenceStrength = 0.17f;

        [Header("Irregular Gust Regions")]
        [SerializeField, Min(2f)]
        private float gustNoiseScaleMetres = 18f;

        [SerializeField, Min(0f)]
        private float gustTravelSpeed = 3.2f;

        [SerializeField, Min(0f)]
        private float gustStrength = 0.62f;

        [SerializeField, Range(0f, 1f)]
        private float gustThreshold = 0.58f;

        [SerializeField, Range(0.01f, 0.45f)]
        private float gustSoftness = 0.16f;

        [SerializeField]
        private int seed = 8429;

        [Header("Elastic Visual Response")]
        [SerializeField, Range(0.1f, 4f)]
        private float responseFrequencyHz = 1.35f;

        [SerializeField, Range(0.1f, 2f)]
        private float responseDampingRatio = 0.56f;

        [SerializeField, Range(0f, 0.75f)]
        private float responseVariation = 0.38f;

        [Header("Wind / Visual Mapping")]
        [SerializeField, Min(0.05f)]
        [Tooltip("Maximum magnitude of the authoritative CPU/GPU target-wind vector. Gameplay and wind-line consumers read these dimensionless Weather strength units.")]
        private float maximumWindStrength = 1f;

        [SerializeField, Min(0.05f)]
        [Tooltip("Maximum displacement stored by the visual spring-response field. Vegetation samples this bend in world metres.")]
        private float maximumVisualBendMetres = 0.82f;

        [Header("Debug Visualization")]
        [SerializeField]
        private WeatherWindDebugView debugView = WeatherWindDebugView.Off;

        [SerializeField, Range(1, 32)]
        private int debugSampleStepCells = 8;

        [SerializeField, Min(0f)]
        private float debugHeightOffset = 0.15f;

        [SerializeField, Min(0.05f)]
        private float debugArrowScale = 1.15f;

        private ComputeShader computeShader;
        private RenderTexture targetField;
        private RenderTexture responseA;
        private RenderTexture responseB;
        private RenderTexture currentResponse;
        private RenderTexture writeResponse;
        private Camera resolvedCamera;
        private int initializeKernel = -1;
        private int recenterKernel = -1;
        private int simulateKernel = -1;
        private Vector2Int originCell;
        private Vector2Int ringOffset;
        private bool originInitialized;
        private bool resourcesDirty = true;
        private bool resourcesReady;
        private string lastError = string.Empty;
        private double lastRealtime;
        private float simulationAccumulator;
        private float simulationTime;
        private int lastFrameStepCount;
        private int lastFrameDispatchCount;
        private int totalSimulationDispatchCount;
        private int totalRecenterDispatchCount;
        private int lastValidatedSimulationConfigurationHash;
        private bool simulationConfigurationHashInitialized;

        public static int ActiveDomainCount => ActiveDomainsInternal.Count;
        public static WeatherWindDomain PublishedDomain { get; private set; }

        public Transform FieldAnchor => fieldAnchor;
        public Camera TargetCamera => targetCamera != null
            ? targetCamera
            : resolvedCamera;
        public int FieldResolution => fieldResolution;
        public float CellSizeMetres => cellSizeMetres;
        public float FieldWorldSizeMetres => fieldResolution * cellSizeMetres;
        public float UpdateRateHz => updateRateHz;
        public bool ResourcesReady => resourcesReady;
        public string LastError => lastError;
        public Vector2 FieldOriginXZ => new Vector2(
            originCell.x * cellSizeMetres,
            originCell.y * cellSizeMetres);
        public Vector2Int RingOffset => ringOffset;
        public RenderTexture TargetWindTexture => targetField;
        public RenderTexture ResponseTexture => currentResponse;
        public long EstimatedTextureBytes =>
            (long)fieldResolution * fieldResolution * (4L + 8L + 8L);
        public int LastFrameStepCount => lastFrameStepCount;
        public int LastFrameDispatchCount => lastFrameDispatchCount;
        public int TotalSimulationDispatchCount => totalSimulationDispatchCount;
        public int TotalRecenterDispatchCount => totalRecenterDispatchCount;
        public WeatherWindDebugView DebugView => debugView;
        public int DebugSampleStepCells => debugSampleStepCells;
        public float DebugHeightOffset => debugHeightOffset;
        public float DebugArrowScale => debugArrowScale;
        public float MaximumWindStrength => maximumWindStrength;
        public float MaximumVisualBendMetres => maximumVisualBendMetres;
        public float SimulationTime => simulationTime;
        public int SimulationConfigurationHash =>
            ComputeSimulationConfigurationHash();

        protected virtual void OnEnable()
        {
            if (!ActiveDomainsInternal.Contains(this))
            {
                ActiveDomainsInternal.Add(this);
            }

            PublishedDomain = this;
            ResolveCameraOnce();
            lastValidatedSimulationConfigurationHash =
                ComputeSimulationConfigurationHash();
            simulationConfigurationHashInitialized = true;
            resourcesDirty = true;
            lastRealtime = Time.realtimeSinceStartupAsDouble;
            EnsureResources();
            PublishShaderGlobals();
        }

        protected virtual void OnDisable()
        {
            ActiveDomainsInternal.Remove(this);
            bool wasPublished = PublishedDomain == this;
            ReleaseResources();

            if (!wasPublished)
            {
                return;
            }

            PublishedDomain = ActiveDomainsInternal.Count > 0
                ? ActiveDomainsInternal[ActiveDomainsInternal.Count - 1]
                : null;
            if (PublishedDomain != null)
            {
                PublishedDomain.resourcesDirty = true;
                PublishedDomain.EnsureResources();
                PublishedDomain.PublishShaderGlobals();
            }
            else
            {
                ClearShaderGlobals();
            }
        }

        protected virtual void OnDestroy()
        {
            ActiveDomainsInternal.Remove(this);
            bool wasPublished = PublishedDomain == this;
            ReleaseResources();

            if (!wasPublished)
            {
                return;
            }

            PublishedDomain = ActiveDomainsInternal.Count > 0
                ? ActiveDomainsInternal[ActiveDomainsInternal.Count - 1]
                : null;
            if (PublishedDomain != null)
            {
                PublishedDomain.resourcesDirty = true;
                PublishedDomain.EnsureResources();
                PublishedDomain.PublishShaderGlobals();
            }
            else
            {
                ClearShaderGlobals();
            }
        }

        protected virtual void OnValidate()
        {
            fieldResolution = Mathf.Clamp(
                Mathf.ClosestPowerOfTwo(fieldResolution),
                32,
                256);
            cellSizeMetres = Mathf.Clamp(cellSizeMetres, 0.1f, 4f);
            updateRateHz = Mathf.Clamp(updateRateHz, 5f, 60f);
            maximumStepsPerFrame = Mathf.Clamp(maximumStepsPerFrame, 1, 8);
            baseStrength = Mathf.Max(0f, baseStrength);
            broadNoiseScaleMetres = Mathf.Max(1f, broadNoiseScaleMetres);
            broadNoiseTravelSpeed = Mathf.Max(0f, broadNoiseTravelSpeed);
            turbulenceStrength = Mathf.Max(0f, turbulenceStrength);
            gustNoiseScaleMetres = Mathf.Max(2f, gustNoiseScaleMetres);
            gustTravelSpeed = Mathf.Max(0f, gustTravelSpeed);
            gustStrength = Mathf.Max(0f, gustStrength);
            gustThreshold = Mathf.Clamp01(gustThreshold);
            gustSoftness = Mathf.Clamp(gustSoftness, 0.01f, 0.45f);
            responseFrequencyHz = Mathf.Clamp(responseFrequencyHz, 0.1f, 4f);
            responseDampingRatio = Mathf.Clamp(responseDampingRatio, 0.1f, 2f);
            responseVariation = Mathf.Clamp(responseVariation, 0f, 0.75f);
            maximumWindStrength = Mathf.Clamp(maximumWindStrength, 0.05f, 4f);
            maximumVisualBendMetres = Mathf.Clamp(
                maximumVisualBendMetres,
                0.05f,
                3f);
            debugSampleStepCells = Mathf.Clamp(debugSampleStepCells, 1, 32);
            debugHeightOffset = Mathf.Max(0f, debugHeightOffset);
            debugArrowScale = Mathf.Clamp(debugArrowScale, 0.05f, 8f);
            resolvedCamera = targetCamera;

            int simulationConfigurationHash =
                ComputeSimulationConfigurationHash();
            if (!simulationConfigurationHashInitialized)
            {
                lastValidatedSimulationConfigurationHash =
                    simulationConfigurationHash;
                simulationConfigurationHashInitialized = true;
            }
            else if (simulationConfigurationHash !=
                     lastValidatedSimulationConfigurationHash)
            {
                lastValidatedSimulationConfigurationHash =
                    simulationConfigurationHash;
                resourcesDirty = true;
            }
        }

        protected virtual void Update()
        {
            if (PublishedDomain != this)
            {
                return;
            }

            ResolveCameraOnce();
            lastFrameStepCount = 0;
            lastFrameDispatchCount = 0;

            if (!EnsureResources())
            {
                PublishShaderGlobals();
                return;
            }

            RecenterIfNeeded();

            double now = Time.realtimeSinceStartupAsDouble;
            float elapsed = (float)Math.Max(0.0, Math.Min(0.25, now - lastRealtime));
            lastRealtime = now;
            simulationAccumulator += elapsed;

            float fixedStep = 1f / Mathf.Max(1f, updateRateHz);
            int stepCount = 0;
            while (simulationAccumulator >= fixedStep &&
                   stepCount < maximumStepsPerFrame)
            {
                simulationTime += fixedStep;
                DispatchSimulation(fixedStep);
                simulationAccumulator -= fixedStep;
                stepCount++;
            }

            if (stepCount == maximumStepsPerFrame &&
                simulationAccumulator > fixedStep)
            {
                simulationAccumulator = fixedStep;
            }

            lastFrameStepCount = stepCount;
            PublishShaderGlobals();
        }

        public void RequestRebuild()
        {
            resourcesDirty = true;
        }

        public void ResetField()
        {
            if (PublishedDomain != this)
            {
                PublishedDomain = this;
            }

            resourcesDirty = true;
            EnsureResources();
            PublishShaderGlobals();
        }

        public Vector2 SampleWindXZ(Vector3 worldPosition)
        {
            return EvaluateTargetWind(new Vector2(worldPosition.x, worldPosition.z), simulationTime);
        }

        public static bool TrySampleWindXZ(Vector3 worldPosition, out Vector2 wind)
        {
            if (PublishedDomain == null || !PublishedDomain.isActiveAndEnabled)
            {
                wind = Vector2.zero;
                return false;
            }

            wind = PublishedDomain.SampleWindXZ(worldPosition);
            return true;
        }

        public string BuildComprehensiveReport()
        {
            var builder = new StringBuilder(2048);
            builder.AppendLine("[Weather Wind V0 XZ Domain Report]");
            builder.Append("Status: ")
                .AppendLine(resourcesReady ? "READY" : "NOT READY");
            builder.Append("Published domain: ")
                .AppendLine(PublishedDomain == this ? "Yes" : "No");
            builder.Append("Active Weather domains: ")
                .AppendLine(ActiveDomainCount.ToString());
            builder.Append("Field anchor: ")
                .AppendLine(fieldAnchor != null ? fieldAnchor.name : "Camera ground projection");
            builder.Append("Fallback camera: ")
                .AppendLine(TargetCamera != null ? TargetCamera.name : "Component transform");
            builder.Append("Field plane Y: ")
                .AppendLine(fieldPlaneY.ToString("0.###"));
            builder.Append("Resolved anchor position: ")
                .AppendLine(ResolveAnchorPosition().ToString("F3"));
            builder.Append("XZ field resolution: ")
                .Append(fieldResolution).Append(" × ").AppendLine(fieldResolution.ToString());
            builder.Append("Cell size: ")
                .Append(cellSizeMetres.ToString("0.###")).AppendLine(" m");
            builder.Append("World coverage: ")
                .Append(FieldWorldSizeMetres.ToString("0.###")).Append(" × ")
                .Append(FieldWorldSizeMetres.ToString("0.###")).AppendLine(" m");
            builder.Append("Update rate: ")
                .Append(updateRateHz.ToString("0.###")).AppendLine(" Hz");
            builder.Append("Field origin XZ: ")
                .AppendLine(FieldOriginXZ.ToString("F3"));
            builder.Append("Toroidal offset: ")
                .AppendLine(ringOffset.ToString());
            builder.Append("Estimated texture memory: ")
                .Append(EstimatedTextureBytes.ToString("N0")).AppendLine(" bytes");
            builder.Append("Prevailing direction: ")
                .AppendLine(NormalizedDirection().ToString("F3"));
            builder.Append("Base strength: ")
                .AppendLine(baseStrength.ToString("0.###"));
            builder.Append("Broad noise scale: ")
                .Append(broadNoiseScaleMetres.ToString("0.###")).AppendLine(" m");
            builder.Append("Broad noise travel speed: ")
                .Append(broadNoiseTravelSpeed.ToString("0.###")).AppendLine(" m/s");
            builder.Append("Turbulence strength: ")
                .AppendLine(turbulenceStrength.ToString("0.###"));
            builder.Append("Gust noise scale: ")
                .Append(gustNoiseScaleMetres.ToString("0.###")).AppendLine(" m");
            builder.Append("Gust travel speed: ")
                .Append(gustTravelSpeed.ToString("0.###")).AppendLine(" m/s");
            builder.Append("Gust strength: ")
                .AppendLine(gustStrength.ToString("0.###"));
            builder.Append("Gust threshold / softness: ")
                .Append(gustThreshold.ToString("0.###")).Append(" / ")
                .AppendLine(gustSoftness.ToString("0.###"));
            builder.Append("Response frequency: ")
                .Append(responseFrequencyHz.ToString("0.###")).AppendLine(" Hz");
            builder.Append("Response damping ratio: ")
                .AppendLine(responseDampingRatio.ToString("0.###"));
            builder.Append("Response variation: ")
                .AppendLine(responseVariation.ToString("0.###"));
            builder.Append("Maximum authoritative wind strength: ")
                .AppendLine(maximumWindStrength.ToString("0.###"));
            builder.Append("Maximum visual bend: ")
                .Append(maximumVisualBendMetres.ToString("0.###")).AppendLine(" m");
            builder.Append("Last frame simulation steps: ")
                .AppendLine(lastFrameStepCount.ToString());
            builder.Append("Last frame compute dispatches: ")
                .AppendLine(lastFrameDispatchCount.ToString());
            builder.Append("Total simulation dispatches: ")
                .AppendLine(totalSimulationDispatchCount.ToString("N0"));
            builder.Append("Total recenter dispatches: ")
                .AppendLine(totalRecenterDispatchCount.ToString("N0"));
            builder.Append("CPU gameplay query: ")
                .AppendLine("SampleWindXZ / TrySampleWindXZ available");
            builder.Append("Target-field consumer contract: ")
                .AppendLine("Available for future stylized wind-line advection");
            builder.Append("Debug view: ")
                .AppendLine(debugView.ToString());
            builder.Append("Debug step / height / scale: ")
                .Append(debugSampleStepCells.ToString()).Append(" cells / ")
                .Append(debugHeightOffset.ToString("0.###")).Append(" m / ")
                .Append(debugArrowScale.ToString("0.###")).AppendLine("×");

            if (!string.IsNullOrEmpty(lastError))
            {
                builder.AppendLine("Error:");
                builder.AppendLine(lastError);
            }

            return builder.ToString();
        }

        private bool EnsureResources()
        {
            if (!resourcesDirty && ResourcesAreValid())
            {
                return true;
            }

            ReleaseResources();
            lastError = string.Empty;

            if (!SystemInfo.supportsComputeShaders)
            {
                lastError = "Weather XZ wind field requires compute-shader support.";
                return false;
            }

            if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGHalf) ||
                !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
            {
                lastError =
                    "Weather XZ wind field requires RGHalf and ARGBHalf random-write textures.";
                return false;
            }

            computeShader = Resources.Load<ComputeShader>(ComputeResourcePath);
            if (computeShader == null)
            {
                lastError =
                    $"Could not load compute shader Resources/{ComputeResourcePath}.";
                return false;
            }

            try
            {
                initializeKernel = computeShader.FindKernel("InitializeField");
                recenterKernel = computeShader.FindKernel("RecenterField");
                simulateKernel = computeShader.FindKernel("SimulateField");

                targetField = CreateTexture(
                    "PS3D_WeatherWind_TargetXZ",
                    RenderTextureFormat.RGHalf);
                responseA = CreateTexture(
                    "PS3D_WeatherWind_ResponseA",
                    RenderTextureFormat.ARGBHalf);
                responseB = CreateTexture(
                    "PS3D_WeatherWind_ResponseB",
                    RenderTextureFormat.ARGBHalf);
                currentResponse = responseA;
                writeResponse = responseB;

                originCell = ComputeDesiredOriginCell();
                ringOffset = Vector2Int.zero;
                originInitialized = true;
                simulationAccumulator = 0f;
                simulationTime = 0f;
                DispatchInitialize();
                resourcesDirty = false;
                resourcesReady = true;
                return true;
            }
            catch (Exception exception)
            {
                lastError = exception.ToString();
                ReleaseResources();
                return false;
            }
        }

        private bool ResourcesAreValid()
        {
            return resourcesReady &&
                   targetField != null && targetField.IsCreated() &&
                   responseA != null && responseA.IsCreated() &&
                   responseB != null && responseB.IsCreated() &&
                   computeShader != null;
        }

        private RenderTexture CreateTexture(
            string textureName,
            RenderTextureFormat format)
        {
            var texture = new RenderTexture(
                fieldResolution,
                fieldResolution,
                0,
                format,
                RenderTextureReadWrite.Linear)
            {
                name = textureName,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Repeat,
                hideFlags = HideFlags.DontSave
            };
            if (!texture.Create())
            {
                if (Application.isPlaying)
                {
                    Destroy(texture);
                }
                else
                {
                    DestroyImmediate(texture);
                }

                throw new InvalidOperationException(
                    $"Could not create random-write texture {textureName} ({format}).");
            }

            return texture;
        }

        private void DispatchInitialize()
        {
            SetCommonComputeParameters(0f);
            computeShader.SetTexture(initializeKernel, "_TargetWindWrite", targetField);
            computeShader.SetTexture(initializeKernel, "_StateAWrite", responseA);
            computeShader.SetTexture(initializeKernel, "_StateBWrite", responseB);
            Dispatch(initializeKernel);
        }

        private void RecenterIfNeeded()
        {
            Vector2Int desiredOrigin = ComputeDesiredOriginCell();
            if (!originInitialized)
            {
                originCell = desiredOrigin;
                ringOffset = Vector2Int.zero;
                originInitialized = true;
                DispatchInitialize();
                return;
            }

            Vector2Int delta = desiredOrigin - originCell;
            if (delta == Vector2Int.zero)
            {
                return;
            }

            bool resetAll = Mathf.Abs(delta.x) >= fieldResolution ||
                            Mathf.Abs(delta.y) >= fieldResolution;
            originCell = desiredOrigin;
            if (resetAll)
            {
                ringOffset = Vector2Int.zero;
            }
            else
            {
                ringOffset = new Vector2Int(
                    PositiveMod(ringOffset.x + delta.x, fieldResolution),
                    PositiveMod(ringOffset.y + delta.y, fieldResolution));
            }

            SetCommonComputeParameters(0f);
            computeShader.SetInts("_RecenterDelta", delta.x, delta.y);
            computeShader.SetInt("_ResetAll", resetAll ? 1 : 0);
            computeShader.SetTexture(recenterKernel, "_TargetWindWrite", targetField);
            computeShader.SetTexture(recenterKernel, "_StateAWrite", responseA);
            computeShader.SetTexture(recenterKernel, "_StateBWrite", responseB);
            Dispatch(recenterKernel);
            totalRecenterDispatchCount++;
        }

        private void DispatchSimulation(float deltaTime)
        {
            SetCommonComputeParameters(deltaTime);
            computeShader.SetTexture(simulateKernel, "_TargetWindWrite", targetField);
            computeShader.SetTexture(simulateKernel, "_StateRead", currentResponse);
            computeShader.SetTexture(simulateKernel, "_StateWrite", writeResponse);
            Dispatch(simulateKernel);

            RenderTexture previous = currentResponse;
            currentResponse = writeResponse;
            writeResponse = previous;
            totalSimulationDispatchCount++;
        }

        private void SetCommonComputeParameters(float deltaTime)
        {
            Vector2 direction = NormalizedDirection();
            computeShader.SetInts("_FieldResolution", fieldResolution, fieldResolution);
            computeShader.SetInts("_FieldOffset", ringOffset.x, ringOffset.y);
            computeShader.SetVector(
                "_FieldOriginCellSize",
                new Vector4(
                    originCell.x * cellSizeMetres,
                    originCell.y * cellSizeMetres,
                    cellSizeMetres,
                    0f));
            computeShader.SetFloat("_DeltaTime", deltaTime);
            computeShader.SetFloat("_WindTime", simulationTime);
            computeShader.SetInt("_WindSeed", seed);
            computeShader.SetVector(
                "_PrevailingParameters",
                new Vector4(
                    direction.x,
                    direction.y,
                    baseStrength,
                    turbulenceStrength));
            computeShader.SetVector(
                "_BroadNoiseParameters",
                new Vector4(
                    broadNoiseScaleMetres,
                    broadNoiseTravelSpeed,
                    gustNoiseScaleMetres,
                    gustTravelSpeed));
            computeShader.SetVector(
                "_GustParameters",
                new Vector4(
                    gustStrength,
                    gustThreshold,
                    gustSoftness,
                    maximumWindStrength));
            computeShader.SetVector(
                "_ResponseParameters",
                new Vector4(
                    responseFrequencyHz,
                    responseDampingRatio,
                    responseVariation,
                    maximumVisualBendMetres));
        }

        private void Dispatch(int kernel)
        {
            int groupCount = Mathf.CeilToInt(fieldResolution / (float)ThreadGroupSize);
            computeShader.Dispatch(kernel, groupCount, groupCount, 1);
            lastFrameDispatchCount++;
        }

        private void PublishShaderGlobals()
        {
            if (PublishedDomain != this)
            {
                return;
            }

            if (!resourcesReady || currentResponse == null)
            {
                ClearShaderGlobals();
                return;
            }

            float fixedStep = 1f / Mathf.Max(1f, updateRateHz);
            Shader.SetGlobalTexture(TargetFieldId, targetField);
            Shader.SetGlobalTexture(ResponseFieldId, currentResponse);
            Shader.SetGlobalVector(
                FieldOriginCellSizeId,
                new Vector4(
                    originCell.x * cellSizeMetres,
                    originCell.y * cellSizeMetres,
                    cellSizeMetres,
                    1f));
            Shader.SetGlobalVector(
                FieldResolutionOffsetId,
                new Vector4(
                    fieldResolution,
                    fieldResolution,
                    ringOffset.x,
                    ringOffset.y));
            Shader.SetGlobalVector(
                FieldTimingId,
                new Vector4(
                    simulationTime,
                    Mathf.Min(simulationAccumulator, fixedStep),
                    fixedStep,
                    maximumVisualBendMetres));
        }

        private static void ClearShaderGlobals()
        {
            Shader.SetGlobalTexture(TargetFieldId, Texture2D.blackTexture);
            Shader.SetGlobalTexture(ResponseFieldId, Texture2D.blackTexture);
            Shader.SetGlobalVector(FieldOriginCellSizeId, Vector4.zero);
            Shader.SetGlobalVector(FieldResolutionOffsetId, Vector4.zero);
            Shader.SetGlobalVector(FieldTimingId, Vector4.zero);
        }

        private Vector2Int ComputeDesiredOriginCell()
        {
            Vector3 anchor = ResolveAnchorPosition();
            int centreX = Mathf.FloorToInt(anchor.x / cellSizeMetres);
            int centreZ = Mathf.FloorToInt(anchor.z / cellSizeMetres);
            int halfResolution = fieldResolution / 2;
            return new Vector2Int(
                centreX - halfResolution,
                centreZ - halfResolution);
        }

        private Vector3 ResolveAnchorPosition()
        {
            if (fieldAnchor != null)
            {
                return fieldAnchor.position;
            }

            Camera camera = TargetCamera;
            if (camera == null)
            {
                return transform.position;
            }

            Vector3 cameraPosition = camera.transform.position;
            Vector3 cameraForward = camera.transform.forward;
            if (Mathf.Abs(cameraForward.y) > 0.0001f)
            {
                float projectionDistance =
                    (fieldPlaneY - cameraPosition.y) / cameraForward.y;
                if (projectionDistance >= 0f)
                {
                    return cameraPosition + cameraForward * projectionDistance;
                }
            }

            return new Vector3(cameraPosition.x, fieldPlaneY, cameraPosition.z);
        }

        private void ResolveCameraOnce()
        {
            if (targetCamera != null)
            {
                resolvedCamera = targetCamera;
                return;
            }

            if (resolvedCamera == null)
            {
                resolvedCamera = Camera.main;
            }
        }

        private Vector2 NormalizedDirection()
        {
            return prevailingDirection.sqrMagnitude > 0.000001f
                ? prevailingDirection.normalized
                : Vector2.right;
        }

        private Vector2 EvaluateTargetWind(Vector2 worldXZ, float time)
        {
            Vector2 direction = NormalizedDirection();
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);
            Vector2 broadTravel = direction * (time * broadNoiseTravelSpeed);
            Vector2 broadCoordinate =
                (worldXZ - broadTravel) / Mathf.Max(1f, broadNoiseScaleMetres);
            float noiseX = ValueNoise(broadCoordinate, unchecked((uint)seed + 11u)) * 2f - 1f;
            float noiseY = ValueNoise(
                broadCoordinate * 0.83f + new Vector2(19.17f, -7.31f),
                unchecked((uint)seed + 37u)) * 2f - 1f;
            Vector2 turbulentWind = new Vector2(noiseX, noiseY) * turbulenceStrength;

            Vector2 gustTravel = direction * (time * gustTravelSpeed);
            Vector2 gustCoordinate =
                (worldXZ - gustTravel) / Mathf.Max(2f, gustNoiseScaleMetres);
            float gustA = ValueNoise(gustCoordinate, unchecked((uint)seed + 101u));
            float gustB = ValueNoise(
                gustCoordinate * 2.03f + new Vector2(-13.7f, 8.9f),
                unchecked((uint)seed + 211u));
            float gustNoise = gustA * 0.68f + gustB * 0.32f;
            float gustMask = SmoothStep(
                gustThreshold - gustSoftness,
                gustThreshold + gustSoftness,
                gustNoise);
            gustMask *= gustMask;

            Vector2 gustDirection = direction + perpendicular * (noiseX * 0.22f);
            if (gustDirection.sqrMagnitude > 0.000001f)
            {
                gustDirection.Normalize();
            }
            else
            {
                gustDirection = direction;
            }

            Vector2 target = direction * baseStrength +
                             turbulentWind +
                             gustDirection * (gustStrength * gustMask);
            return ClampMagnitude(target, maximumWindStrength);
        }

        private static float ValueNoise(Vector2 coordinate, uint salt)
        {
            int x0 = Mathf.FloorToInt(coordinate.x);
            int y0 = Mathf.FloorToInt(coordinate.y);
            float tx = coordinate.x - x0;
            float ty = coordinate.y - y0;
            tx = tx * tx * (3f - 2f * tx);
            ty = ty * ty * (3f - 2f * ty);

            float h00 = Hash01(x0, y0, salt);
            float h10 = Hash01(x0 + 1, y0, salt);
            float h01 = Hash01(x0, y0 + 1, salt);
            float h11 = Hash01(x0 + 1, y0 + 1, salt);
            float lower = Mathf.Lerp(h00, h10, tx);
            float upper = Mathf.Lerp(h01, h11, tx);
            return Mathf.Lerp(lower, upper, ty);
        }

        private static float Hash01(int x, int y, uint salt)
        {
            unchecked
            {
                uint state = (uint)x * 0x8da6b343u;
                state ^= (uint)y * 0xd8163841u;
                state ^= salt * 0xcb1ab31fu;
                state ^= state >> 16;
                state *= 0x7feb352du;
                state ^= state >> 15;
                state *= 0x846ca68bu;
                state ^= state >> 16;
                return (state & 0x00ffffffu) / 16777215f;
            }
        }

        private static float SmoothStep(float edge0, float edge1, float value)
        {
            float t = Mathf.Clamp01((value - edge0) / Mathf.Max(0.000001f, edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        private static Vector2 ClampMagnitude(Vector2 value, float maximum)
        {
            float squareMagnitude = value.sqrMagnitude;
            float maximumSquared = maximum * maximum;
            if (squareMagnitude <= maximumSquared || squareMagnitude <= 0.0000001f)
            {
                return value;
            }

            return value * (maximum / Mathf.Sqrt(squareMagnitude));
        }

        public Rect GetFieldWorldRectXZ()
        {
            float size = FieldWorldSizeMetres;
            Vector2 origin = FieldOriginXZ;
            return new Rect(origin.x, origin.y, size, size);
        }

        public Vector3 GetDebugAnchorPosition()
        {
            return ResolveAnchorPosition();
        }

        public Vector2 SampleTargetWindXZ(Vector2 worldXZ)
        {
            return SampleTargetWindXZ(worldXZ, simulationTime);
        }

        public Vector2 SampleTargetWindXZ(Vector2 worldXZ, float sampleTime)
        {
            return EvaluateTargetWind(worldXZ, sampleTime);
        }

        public Vector2 ConvertTargetWindToVisualBend(Vector2 targetWind)
        {
            return targetWind *
                (maximumVisualBendMetres /
                 Mathf.Max(0.05f, maximumWindStrength));
        }

        public bool TrySampleResponseDebug(int logicalX, int logicalY, Color[] cachedPixels, out Vector2 bend)
        {
            bend = Vector2.zero;
            if (cachedPixels == null ||
                logicalX < 0 || logicalY < 0 ||
                logicalX >= fieldResolution || logicalY >= fieldResolution)
            {
                return false;
            }

            int physicalX = PositiveMod(logicalX + ringOffset.x, fieldResolution);
            int physicalY = PositiveMod(logicalY + ringOffset.y, fieldResolution);
            int index = physicalY * fieldResolution + physicalX;
            if (index < 0 || index >= cachedPixels.Length)
            {
                return false;
            }

            Color sample = cachedPixels[index];
            bend = new Vector2(sample.r, sample.g);
            return true;
        }

        public bool TrySampleResponseErrorDebug(
            int logicalX,
            int logicalY,
            Vector2 worldXZ,
            float responseSampleTime,
            Color[] cachedPixels,
            out Vector2 responseError)
        {
            responseError = Vector2.zero;
            if (!TrySampleResponseDebug(
                    logicalX,
                    logicalY,
                    cachedPixels,
                    out Vector2 actualResponse))
            {
                return false;
            }

            Vector2 expectedResponse = ConvertTargetWindToVisualBend(
                SampleTargetWindXZ(worldXZ, responseSampleTime));
            responseError = actualResponse - expectedResponse;
            return true;
        }

        private int ComputeSimulationConfigurationHash()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 +
                    (fieldAnchor != null ? fieldAnchor.GetEntityId().GetHashCode() : 0);
                hash = hash * 31 +
                    (targetCamera != null ? targetCamera.GetEntityId().GetHashCode() : 0);
                hash = hash * 31 + fieldPlaneY.GetHashCode();
                hash = hash * 31 + fieldResolution;
                hash = hash * 31 + cellSizeMetres.GetHashCode();
                hash = hash * 31 + updateRateHz.GetHashCode();
                hash = hash * 31 + maximumStepsPerFrame;
                hash = hash * 31 + prevailingDirection.GetHashCode();
                hash = hash * 31 + baseStrength.GetHashCode();
                hash = hash * 31 + broadNoiseScaleMetres.GetHashCode();
                hash = hash * 31 + broadNoiseTravelSpeed.GetHashCode();
                hash = hash * 31 + turbulenceStrength.GetHashCode();
                hash = hash * 31 + gustNoiseScaleMetres.GetHashCode();
                hash = hash * 31 + gustTravelSpeed.GetHashCode();
                hash = hash * 31 + gustStrength.GetHashCode();
                hash = hash * 31 + gustThreshold.GetHashCode();
                hash = hash * 31 + gustSoftness.GetHashCode();
                hash = hash * 31 + seed;
                hash = hash * 31 + responseFrequencyHz.GetHashCode();
                hash = hash * 31 + responseDampingRatio.GetHashCode();
                hash = hash * 31 + responseVariation.GetHashCode();
                hash = hash * 31 + maximumWindStrength.GetHashCode();
                hash = hash * 31 + maximumVisualBendMetres.GetHashCode();
                return hash;
            }
        }

        private void ReleaseResources()
        {
            resourcesReady = false;
            ReleaseTexture(ref targetField);
            ReleaseTexture(ref responseA);
            ReleaseTexture(ref responseB);
            currentResponse = null;
            writeResponse = null;
            computeShader = null;
            initializeKernel = -1;
            recenterKernel = -1;
            simulateKernel = -1;
            originInitialized = false;
        }

        private static void ReleaseTexture(ref RenderTexture texture)
        {
            if (texture == null)
            {
                return;
            }

            texture.Release();
            if (Application.isPlaying)
            {
                Destroy(texture);
            }
            else
            {
                DestroyImmediate(texture);
            }
            texture = null;
        }

        private static int PositiveMod(int value, int modulus)
        {
            int remainder = value % modulus;
            return remainder < 0 ? remainder + modulus : remainder;
        }
    }
}
