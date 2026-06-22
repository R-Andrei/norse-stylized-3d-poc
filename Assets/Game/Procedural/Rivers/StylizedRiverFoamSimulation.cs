using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public enum StylizedRiverFoamStyle
    {
        Subtle,
        Flowing,
        Lively,
        Custom
    }

    public enum StylizedRiverFoamQuality
    {
        Low,
        Medium,
        High
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(StylizedRiver))]
    public sealed class StylizedRiverFoamSimulation : MonoBehaviour
    {
        private const string ComputeResourcePath =
            "PS3DRiver/Compute/CS_StylizedRiverFoam";

        [Header("Foam Look")]
        [SerializeField] private StylizedRiverFoamStyle style =
            StylizedRiverFoamStyle.Subtle;

        [Range(0f, 1f)]
        [SerializeField] private float foamAmount = 0.22f;

        [Min(0.25f)]
        [SerializeField] private float patternScale = 1.35f;

        [Range(0f, 2f)]
        [SerializeField] private float flowSpeed = 0.58f;

        [Range(0f, 1f)]
        [SerializeField] private float breakup = 0.28f;

        [SerializeField]
        private Color foamColor = new Color(0.94f, 0.985f, 1f, 0.78f);

        [Header("Simulation")]
        [SerializeField] private StylizedRiverFoamQuality quality =
            StylizedRiverFoamQuality.Medium;

        [Range(10, 60)]
        [SerializeField] private int updatesPerSecond = 30;

        [Range(0, 512)]
        [SerializeField] private int warmupSteps = 240;

        [SerializeField] private bool simulateInSceneView;
        [SerializeField] private bool resetWhenEnabled = true;

        [Header("Custom Style")]
        [Range(0.01f, 0.09f)]
        [SerializeField] private float customFeed = 0.0367f;

        [Range(0.04f, 0.08f)]
        [SerializeField] private float customKill = 0.0649f;

        [Range(0f, 1f)]
        [SerializeField] private float customCohesion = 0.42f;

        [Range(0f, 1f)]
        [SerializeField] private float customBankSource = 0.22f;

        [Range(0f, 0.2f)]
        [SerializeField] private float customAmbientSource = 0.035f;

        [Range(1f, 20f)]
        [SerializeField] private float customLifetime = 7f;

        [Range(0f, 1f)]
        [SerializeField] private float customContactStrength = 0.24f;

        [Range(0.02f, 1.5f)]
        [SerializeField] private float customContactDepth = 0.22f;

        [Range(0.05f, 0.8f)]
        [SerializeField] private float customThreshold = 0.34f;

        [Range(0.005f, 0.25f)]
        [SerializeField] private float customSoftness = 0.075f;

        private static readonly int StateReadId = Shader.PropertyToID("_StateRead");
        private static readonly int StateWriteId = Shader.PropertyToID("_StateWrite");
        private static readonly int TextureSizeId = Shader.PropertyToID("_TextureSize");
        private static readonly int DeltaTimeId = Shader.PropertyToID("_DeltaTime");
        private static readonly int TimeValueId = Shader.PropertyToID("_TimeValue");
        private static readonly int FlowSpeedId = Shader.PropertyToID("_FlowSpeed");
        private static readonly int FlowDirectionId = Shader.PropertyToID("_FlowDirection");
        private static readonly int RiverLengthId = Shader.PropertyToID("_RiverLength");
        private static readonly int RiverWidthId = Shader.PropertyToID("_RiverWidth");
        private static readonly int PatternScaleId = Shader.PropertyToID("_PatternScale");
        private static readonly int BreakupId = Shader.PropertyToID("_Breakup");
        private static readonly int LifetimeId = Shader.PropertyToID("_Lifetime");
        private static readonly int CohesionId = Shader.PropertyToID("_Cohesion");
        private static readonly int BankSourceId = Shader.PropertyToID("_BankSource");
        private static readonly int AmbientSourceId = Shader.PropertyToID("_AmbientSource");
        private static readonly int FeedId = Shader.PropertyToID("_Feed");
        private static readonly int KillId = Shader.PropertyToID("_Kill");
        private static readonly int SeedId = Shader.PropertyToID("_Seed");

        private StylizedRiver river;
        private ComputeShader computeShader;
        private RenderTexture stateA;
        private RenderTexture stateB;
        private RenderTexture currentState;
        private RenderTexture nextState;
        private int initializeKernel = -1;
        private int stepKernel = -1;
        private int textureWidth;
        private int textureHeight;
        private float simulatedTime;
        private float accumulator;
        private double lastEditorTime;
        private bool resetRequested = true;
        private bool unsupportedWarningReported;
        private bool missingComputeWarningReported;

        public StylizedRiverFoamStyle Style => style;
        public StylizedRiverFoamQuality Quality => quality;
        public bool HasStateTexture => currentState != null && currentState.IsCreated();
        public Vector2Int StateTextureSize => new Vector2Int(textureWidth, textureHeight);
        public RenderTexture StateTexture => currentState;

        private void Reset()
        {
            river = GetComponent<StylizedRiver>();
            resetRequested = true;
        }

        private void OnEnable()
        {
            river = GetComponent<StylizedRiver>();
            lastEditorTime = Time.realtimeSinceStartupAsDouble;
            resetRequested = resetWhenEnabled || currentState == null;
            BindCurrentState();
        }

        private void OnDisable()
        {
            if (river != null)
            {
                river.ClearExternalFoamTexture();
            }

            ReleaseTextures();
        }

        private void OnDestroy()
        {
            ReleaseTextures();
        }

        private void OnValidate()
        {
            foamAmount = Mathf.Clamp01(foamAmount);
            patternScale = Mathf.Max(0.25f, patternScale);
            flowSpeed = Mathf.Clamp(flowSpeed, 0f, 2f);
            breakup = Mathf.Clamp01(breakup);
            updatesPerSecond = Mathf.Clamp(updatesPerSecond, 10, 60);
            warmupSteps = Mathf.Clamp(warmupSteps, 0, 512);

            if (river == null)
            {
                river = GetComponent<StylizedRiver>();
            }

            BindCurrentState();
        }

        private void Update()
        {
            if (!Application.isPlaying && !simulateInSceneView)
            {
                if (resetRequested)
                {
                    EnsureReady();
                }
                else
                {
                    BindCurrentState();
                }

                lastEditorTime = Time.realtimeSinceStartupAsDouble;
                return;
            }

            if (!EnsureReady())
            {
                return;
            }

            double now = Time.realtimeSinceStartupAsDouble;
            float frameDelta = Application.isPlaying
                ? Time.deltaTime
                : Mathf.Clamp((float)(now - lastEditorTime), 0f, 0.1f);
            lastEditorTime = now;

            if (frameDelta <= 0f)
            {
                return;
            }

            float stepDelta = 1f / Mathf.Max(10, updatesPerSecond);
            accumulator = Mathf.Min(accumulator + frameDelta, stepDelta * 3f);
            int performedSteps = 0;

            while (accumulator >= stepDelta && performedSteps < 2)
            {
                DispatchStep(stepDelta);
                accumulator -= stepDelta;
                performedSteps++;
            }
        }

        [ContextMenu("Reset Stateful Foam")]
        public void ResetSimulation()
        {
            resetRequested = true;
            EnsureReady();
        }

        public void NotifyRiverChanged()
        {
            resetRequested = true;
        }

        public void RefreshBinding()
        {
            BindCurrentState();
        }

        public void ApplyStyleDefaults()
        {
            switch (style)
            {
                case StylizedRiverFoamStyle.Subtle:
                    foamAmount = 0.22f;
                    patternScale = 1.35f;
                    flowSpeed = 0.58f;
                    breakup = 0.24f;
                    foamColor = new Color(0.94f, 0.985f, 1f, 0.72f);
                    break;

                case StylizedRiverFoamStyle.Flowing:
                    foamAmount = 0.32f;
                    patternScale = 1.10f;
                    flowSpeed = 0.74f;
                    breakup = 0.42f;
                    foamColor = new Color(0.96f, 0.99f, 1f, 0.82f);
                    break;

                case StylizedRiverFoamStyle.Lively:
                    foamAmount = 0.46f;
                    patternScale = 0.82f;
                    flowSpeed = 0.92f;
                    breakup = 0.64f;
                    foamColor = new Color(0.98f, 1f, 1f, 0.90f);
                    break;

                case StylizedRiverFoamStyle.Custom:
                    break;
            }

            resetRequested = true;
            BindCurrentState();
        }

        private bool EnsureReady()
        {
            if (!SystemInfo.supportsComputeShaders)
            {
                if (!unsupportedWarningReported)
                {
                    Debug.LogWarning(
                        $"Stateful river foam on '{name}' is disabled because this platform does not support compute shaders.",
                        this);
                    unsupportedWarningReported = true;
                }

                if (river != null)
                {
                    river.ClearExternalFoamTexture();
                }

                return false;
            }

            unsupportedWarningReported = false;

            if (river == null)
            {
                river = GetComponent<StylizedRiver>();
            }

            if (river == null || river.RiverLength <= 0.01f)
            {
                return false;
            }

            if (!LoadComputeShader())
            {
                return false;
            }

            Vector2Int requiredSize = ResolveTextureSize();
            bool sizeChanged = requiredSize.x != textureWidth || requiredSize.y != textureHeight;

            if (resetRequested || sizeChanged || currentState == null || !currentState.IsCreated())
            {
                AllocateTextures(requiredSize.x, requiredSize.y);
                DispatchInitialize();
                Warmup();
                resetRequested = false;
                accumulator = 0f;
                BindCurrentState();
            }

            return currentState != null && currentState.IsCreated();
        }

        private bool LoadComputeShader()
        {
            if (computeShader != null && initializeKernel >= 0 && stepKernel >= 0)
            {
                return true;
            }

            computeShader = Resources.Load<ComputeShader>(ComputeResourcePath);

            if (computeShader == null)
            {
                if (!missingComputeWarningReported)
                {
                    Debug.LogError(
                        $"Stateful river foam on '{name}' could not load Resources/{ComputeResourcePath}.compute.",
                        this);
                    missingComputeWarningReported = true;
                }

                return false;
            }

            missingComputeWarningReported = false;
            initializeKernel = computeShader.FindKernel("InitializeFoam");
            stepKernel = computeShader.FindKernel("StepFoam");
            return true;
        }

        private Vector2Int ResolveTextureSize()
        {
            int acrossResolution;

            switch (quality)
            {
                case StylizedRiverFoamQuality.Low:
                    acrossResolution = 64;
                    break;

                case StylizedRiverFoamQuality.High:
                    acrossResolution = 128;
                    break;

                default:
                    acrossResolution = 96;
                    break;
            }

            float aspect = river.RiverLength / Mathf.Max(0.5f, river.VisibleWidth);
            int alongResolution = Mathf.RoundToInt(acrossResolution * aspect);
            alongResolution = Mathf.Clamp(alongResolution, acrossResolution, 1024);
            alongResolution = Mathf.Max(8, Mathf.CeilToInt(alongResolution / 8f) * 8);
            return new Vector2Int(acrossResolution, alongResolution);
        }

        private void AllocateTextures(int width, int height)
        {
            ReleaseTextures();
            textureWidth = width;
            textureHeight = height;
            stateA = CreateStateTexture("A");
            stateB = CreateStateTexture("B");
            currentState = stateA;
            nextState = stateB;
        }

        private RenderTexture CreateStateTexture(string suffix)
        {
            RenderTexture texture = new RenderTexture(
                textureWidth,
                textureHeight,
                0,
                RenderTextureFormat.ARGBHalf,
                RenderTextureReadWrite.Linear)
            {
                name = $"RT_PS3D_StatefulFoam_{suffix}_{GetEntityId()}",
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            texture.Create();
            return texture;
        }

        private void DispatchInitialize()
        {
            if (currentState == null)
            {
                return;
            }

            ApplyCommonParameters(1f / Mathf.Max(10, updatesPerSecond));
            computeShader.SetTexture(initializeKernel, StateWriteId, currentState);
            Dispatch(initializeKernel);
            simulatedTime = 0f;
        }

        private void Warmup()
        {
            float deltaTime = 1f / Mathf.Max(10, updatesPerSecond);

            for (int index = 0; index < warmupSteps; index++)
            {
                DispatchStep(deltaTime, false);
            }
        }

        private void DispatchStep(float deltaTime, bool bindAfterStep = true)
        {
            if (currentState == null || nextState == null)
            {
                return;
            }

            simulatedTime += deltaTime;
            ApplyCommonParameters(deltaTime);
            computeShader.SetTexture(stepKernel, StateReadId, currentState);
            computeShader.SetTexture(stepKernel, StateWriteId, nextState);
            Dispatch(stepKernel);

            RenderTexture previous = currentState;
            currentState = nextState;
            nextState = previous;

            if (bindAfterStep)
            {
                BindCurrentState();
            }
        }

        private void ApplyCommonParameters(float deltaTime)
        {
            ResolveStyleTuning(
                out float feed,
                out float kill,
                out float cohesion,
                out float bankSource,
                out float ambientSource,
                out float lifetime,
                out _,
                out _,
                out _,
                out _,
                out _);

            computeShader.SetInts(TextureSizeId, textureWidth, textureHeight);
            computeShader.SetFloat(DeltaTimeId, deltaTime);
            computeShader.SetFloat(TimeValueId, simulatedTime);
            computeShader.SetFloat(FlowSpeedId, flowSpeed);
            computeShader.SetFloat(FlowDirectionId, river.FlowDirection);
            computeShader.SetFloat(RiverLengthId, Mathf.Max(0.01f, river.RiverLength));
            computeShader.SetFloat(RiverWidthId, Mathf.Max(0.01f, river.VisibleWidth));
            computeShader.SetFloat(PatternScaleId, patternScale);
            computeShader.SetFloat(BreakupId, breakup);
            computeShader.SetFloat(LifetimeId, lifetime);
            computeShader.SetFloat(CohesionId, cohesion);
            computeShader.SetFloat(BankSourceId, bankSource);
            computeShader.SetFloat(AmbientSourceId, ambientSource);
            computeShader.SetFloat(FeedId, feed);
            computeShader.SetFloat(KillId, kill);
            computeShader.SetFloat(SeedId, river.VisualSeed);
        }

        private void BindCurrentState()
        {
            if (river == null)
            {
                river = GetComponent<StylizedRiver>();
            }

            if (river == null)
            {
                return;
            }

            ResolveStyleTuning(
                out _,
                out _,
                out _,
                out _,
                out _,
                out _,
                out float contactStrength,
                out float contactDepth,
                out float threshold,
                out float softness,
                out float bandWidth);

            river.SetStatefulFoamTexture(
                currentState,
                foamColor,
                foamAmount,
                threshold,
                softness,
                bandWidth,
                contactStrength,
                contactDepth);
        }

        private void ResolveStyleTuning(
            out float feed,
            out float kill,
            out float cohesion,
            out float bankSource,
            out float ambientSource,
            out float lifetime,
            out float contactStrength,
            out float contactDepth,
            out float threshold,
            out float softness,
            out float bandWidth)
        {
            switch (style)
            {
                case StylizedRiverFoamStyle.Flowing:
                    feed = 0.022f;
                    kill = 0.051f;
                    cohesion = 0.38f;
                    bankSource = 0.13f;
                    ambientSource = 0.018f;
                    lifetime = 8f;
                    contactStrength = 0.20f;
                    contactDepth = 0.22f;
                    threshold = 0.10f;
                    softness = 0.030f;
                    bandWidth = 0.15f;
                    break;

                case StylizedRiverFoamStyle.Lively:
                    feed = 0.014f;
                    kill = 0.054f;
                    cohesion = 0.28f;
                    bankSource = 0.18f;
                    ambientSource = 0.030f;
                    lifetime = 6f;
                    contactStrength = 0.28f;
                    contactDepth = 0.26f;
                    threshold = 0.09f;
                    softness = 0.035f;
                    bandWidth = 0.18f;
                    break;

                case StylizedRiverFoamStyle.Custom:
                    feed = customFeed;
                    kill = customKill;
                    cohesion = customCohesion;
                    bankSource = customBankSource;
                    ambientSource = customAmbientSource;
                    lifetime = customLifetime;
                    contactStrength = customContactStrength;
                    contactDepth = customContactDepth;
                    threshold = customThreshold;
                    softness = customSoftness;
                    bandWidth = 0.14f;
                    break;

                default:
                    feed = 0.0367f;
                    kill = 0.0649f;
                    cohesion = 0.42f;
                    bankSource = 0.10f;
                    ambientSource = 0.012f;
                    lifetime = 9f;
                    contactStrength = 0.16f;
                    contactDepth = 0.20f;
                    threshold = 0.13f;
                    softness = 0.025f;
                    bandWidth = 0.14f;
                    break;
            }
        }

        private void Dispatch(int kernel)
        {
            int groupsX = Mathf.CeilToInt(textureWidth / 8f);
            int groupsY = Mathf.CeilToInt(textureHeight / 8f);
            computeShader.Dispatch(kernel, groupsX, groupsY, 1);
        }

        private void ReleaseTextures()
        {
            ReleaseTexture(ref stateA);
            ReleaseTexture(ref stateB);
            currentState = null;
            nextState = null;
            textureWidth = 0;
            textureHeight = 0;
        }

        private static void ReleaseTexture(ref RenderTexture texture)
        {
            if (texture == null)
            {
                return;
            }

            if (texture.IsCreated())
            {
                texture.Release();
            }

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
    }
}
