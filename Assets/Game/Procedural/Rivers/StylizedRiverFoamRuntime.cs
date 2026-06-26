using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ProgrammaticStylized3D.Geometry;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    /// <summary>
    /// Hidden Stage 6 runtime that owns the complete shared Foam network.
    /// Amount, Freshness, Integrity, and material phase are transported in one
    /// persistent state while a low-resolution guidance field, GPU-only
    /// population controller, boundaries, Wake, and Impact activity organise
    /// that material into an evolving web-like tracer network.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(StylizedRiver))]
    [AddComponentMenu("")]
    public sealed class StylizedRiverFoamRuntime : MonoBehaviour
    {
        private const string ComputeResourcePath =
            "PS3DRiver/Compute/CS_RiverFoam";
        private const float ChunkLengthMetres = 32f;
        private const float ResourceReleaseDelaySeconds = 2f;
        private const float MaximumManualReservationSeconds = 90f;
        private const float DecayToFivePercent = 2.995732f;
        private const int ThreadGroupSize = 8;

        private static readonly int FoamEnabledId =
            Shader.PropertyToID("_FoamEnabled");
        private static readonly int FoamPreviousId =
            Shader.PropertyToID("_FoamPrevious");
        private static readonly int FoamCurrentId =
            Shader.PropertyToID("_FoamCurrent");
        private static readonly int FoamGuidanceId =
            Shader.PropertyToID("_FoamGuidance");
        private static readonly int FoamFractureId =
            Shader.PropertyToID("_FoamFracture");
        private static readonly int FoamBoundaryId =
            Shader.PropertyToID("_FoamBoundary");
        private static readonly int FoamInterpolationId =
            Shader.PropertyToID("_FoamInterpolation");
        private static readonly int FoamGlobalStartId =
            Shader.PropertyToID("_FoamGlobalStart");
        private static readonly int FoamFieldLengthId =
            Shader.PropertyToID("_FoamFieldLength");
        private static readonly int FoamColourId =
            Shader.PropertyToID("_FoamColour");
        private static readonly int FoamStrengthId =
            Shader.PropertyToID("_FoamStrength");
        private static readonly int FoamCoverageId =
            Shader.PropertyToID("_FoamCoverage");
        private static readonly int FoamSharpnessId =
            Shader.PropertyToID("_FoamSharpness");
        private static readonly int FoamDetailScaleId =
            Shader.PropertyToID("_FoamDetailScale");
        private static readonly int FoamDetailStrengthId =
            Shader.PropertyToID("_FoamDetailStrength");
        private static readonly int FoamDebugViewId =
            Shader.PropertyToID("_FoamDebugView");
        private static readonly int FoamSeedId =
            Shader.PropertyToID("_FoamSeed");

        private readonly struct PendingInjection
        {
            public PendingInjection(
                float globalDistance,
                float acrossNormalized,
                float radius,
                float amount,
                float freshness,
                float integrity,
                float phase,
                float elongation,
                bool isManual,
                float shapeSeed = 0f,
                float shapeVariety = 0f,
                bool compoundShape = false)
            {
                GlobalDistance = globalDistance;
                AcrossNormalized = acrossNormalized;
                Radius = radius;
                Amount = amount;
                Freshness = freshness;
                Integrity = integrity;
                Phase = phase;
                Elongation = elongation;
                IsManual = isManual;
                ShapeSeed = shapeSeed;
                ShapeVariety = shapeVariety;
                CompoundShape = compoundShape;
            }

            public float GlobalDistance { get; }
            public float AcrossNormalized { get; }
            public float Radius { get; }
            public float Amount { get; }
            public float Freshness { get; }
            public float Integrity { get; }
            public float Phase { get; }
            public float Elongation { get; }
            public bool IsManual { get; }
            public float ShapeSeed { get; }
            public float ShapeVariety { get; }
            public bool CompoundShape { get; }
        }

        private sealed class FoamReservation
        {
            public float CentreGlobalDistance;
            public float AlongRadius;
            public float RemainingAmount;
            public float Elapsed;
            public float MaximumLifetime;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FoamMetricRow
        {
            public Vector4 WidthsAndSpacing;
        }

        private StylizedRiver river;
        private MeshRenderer surfaceRenderer;
        private MaterialPropertyBlock propertyBlock;
        private ComputeShader computeShader;
        private RenderTexture stateA;
        private RenderTexture stateB;
        private RenderTexture advectedState;
        private RenderTexture reverseState;
        private RenderTexture previousState;
        private RenderTexture currentState;
        private RenderTexture writeState;
        private RenderTexture guidanceTexture;
        private RenderTexture fractureA;
        private RenderTexture fractureB;
        private RenderTexture currentFracture;
        private RenderTexture writeFracture;
        private RenderTexture neutralDisturbanceTexture;
        private Texture2D boundaryTexture;
        private ComputeBuffer metricBuffer;
        private ComputeBuffer populationMetricsBuffer;
        private StylizedRiverDisturbanceRuntime disturbanceRuntime;
        private FoamMetricRow[] metricRows = Array.Empty<FoamMetricRow>();

        private readonly List<PendingInjection> pendingInjections = new();
        private readonly List<FoamReservation> reservations = new();
        private readonly List<IGeneratedGeometrySource> generatedSources = new();

        private bool[] chunkActive = Array.Empty<bool>();
        private double[] chunkActiveUntil = Array.Empty<double>();
        private bool resourcesDirty = true;
        private bool boundaryDirty = true;
        private bool supportWarningReported;
        private bool fullyFrozenLastUpdate;
        private int domainVersion = -1;
        private int fieldWidth;
        private int fieldHeight;
        private int chunkCount;
        private int resolutionPerChunk;
        private int guidanceWidth;
        private int guidanceHeight;
        private int fractureWidth;
        private int fractureHeight;
        private float fieldLength;
        private float simulationAccumulator;
        private float guidanceAccumulator;
        private float populationAccumulator;
        private float fractureAccumulator;
        private float simulationInterpolation = 1f;
        private float lastRuntimeTime;
        private double idleSince;
        private int clearKernel = -1;
        private int injectKernel = -1;
        private int buildGuidanceKernel = -1;
        private int resetPopulationKernel = -1;
        private int measurePopulationKernel = -1;
        private int updateFractureKernel = -1;
        private int clearFractureKernel = -1;
        private int advectForwardKernel = -1;
        private int advectReverseKernel = -1;
        private int simulateKernel = -1;
        private int applyBoundaryKernel = -1;
        private double autonomousDecayUntil;
        private bool autonomousPopulationWasEnabled;
        private StylizedRiverQuality allocatedQuality;

        private int lastUpdateDispatches;
        private int recentPeakDispatches;
        private long lastUpdateCellIterations;
        private long recentPeakCellIterations;
        private double recentPeakWindowEnd;
        private int injectedLastUpdate;
        private float lastInjectionBoundaryCoverage = -1f;
        private bool lastInjectionStateSynchronized;
        private int manualInjectionSequence;

        public int FieldWidth => currentState != null ? currentState.width : 0;
        public int FieldHeight => currentState != null ? currentState.height : 0;
        public int GuidanceWidth => guidanceTexture != null ? guidanceTexture.width : 0;
        public int GuidanceHeight => guidanceTexture != null ? guidanceTexture.height : 0;
        public int FractureWidth => currentFracture != null ? currentFracture.width : 0;
        public int FractureHeight => currentFracture != null ? currentFracture.height : 0;
        public float GuidanceUpdateRate => ResolveGuidanceUpdateRate();
        public float PopulationUpdateRate => ResolvePopulationUpdateRate();
        public float FractureUpdateRate => ResolveFractureUpdateRate();
        public int ActiveChunkCount => CountActiveChunks();
        public int PendingInjectionCount => pendingInjections.Count;
        public int ActiveReservationCount => reservations.Count;
        public int InjectedLastUpdate => injectedLastUpdate;
        public float LastInjectionBoundaryCoverage => lastInjectionBoundaryCoverage;
        public bool LastInjectionStateSynchronized =>
            lastInjectionStateSynchronized;
        public int LastUpdateDispatches => lastUpdateDispatches;
        public int RecentPeakDispatches => recentPeakDispatches;
        public long LastUpdateCellIterations => lastUpdateCellIterations;
        public long RecentPeakCellIterations => recentPeakCellIterations;
        public float UpdateRate => ResolveUpdateRate();
        public bool ResourcesAllocated => currentState != null;
        public bool CorrectedAdvectionActive =>
            currentState != null &&
            advectedState != null &&
            reverseState != null;
        public bool IsSleeping =>
            !IsAutonomousPopulationActive &&
            pendingInjections.Count == 0 &&
            reservations.Count == 0 &&
            CountActiveChunks() == 0;
        public long EstimatedMemoryBytes =>
            EstimateTextureBytes(stateA) +
            EstimateTextureBytes(stateB) +
            EstimateTextureBytes(advectedState) +
            EstimateTextureBytes(reverseState) +
            EstimateTextureBytes(guidanceTexture) +
            EstimateTextureBytes(fractureA) +
            EstimateTextureBytes(fractureB) +
            EstimateTextureBytes(neutralDisturbanceTexture) +
            EstimateTextureBytes(boundaryTexture) +
            (metricBuffer != null
                ? (long)metricBuffer.count * metricBuffer.stride
                : 0L) +
            (populationMetricsBuffer != null
                ? (long)populationMetricsBuffer.count * populationMetricsBuffer.stride
                : 0L);

        private bool IsAutonomousPopulationActive =>
            river != null && river.FoamAmount > 0.0001f;

        private bool IsSupported =>
            SystemInfo.supportsComputeShaders &&
            SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf) &&
            SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGHalf);

        private void OnEnable()
        {
            hideFlags = HideFlags.HideInInspector;
            river = GetComponent<StylizedRiver>();
            surfaceRenderer = river != null ? river.SurfaceRenderer : null;

            if (river != null)
            {
                river.DomainChanged += HandleDomainChanged;
            }

            GeneratedGeometryRegistry.SourceAdded += HandleGeneratedSourceChanged;
            GeneratedGeometryRegistry.SourceRemoved += HandleGeneratedSourceChanged;
            GeneratedGeometryRegistry.SourceChanged += HandleGeneratedSourceChanged;

            lastRuntimeTime = Time.realtimeSinceStartup;
            resourcesDirty = true;
            boundaryDirty = true;
            BindDisabled();
        }

        private void OnDisable()
        {
            if (river != null)
            {
                river.DomainChanged -= HandleDomainChanged;
            }

            GeneratedGeometryRegistry.SourceAdded -= HandleGeneratedSourceChanged;
            GeneratedGeometryRegistry.SourceRemoved -= HandleGeneratedSourceChanged;
            GeneratedGeometryRegistry.SourceChanged -= HandleGeneratedSourceChanged;

            BindDisabled();
            ReleaseResources();
            pendingInjections.Clear();
            reservations.Clear();
        }

        private void OnDestroy()
        {
            ReleaseResources();
        }

        private void LateUpdate()
        {
            ResetLastUpdateDiagnostics();

            if (river == null)
            {
                river = GetComponent<StylizedRiver>();
            }

            if (river == null || !river.FoamEnabled)
            {
                BindDisabled();
                ReleaseResources();
                pendingInjections.Clear();
                reservations.Clear();
                ResetManualInjectionSequence();
                return;
            }

            surfaceRenderer = river.SurfaceRenderer;

            if (!Application.isPlaying)
            {
                BindDisabled();
                return;
            }

            if (!IsSupported)
            {
                if (!supportWarningReported)
                {
                    Debug.LogWarning(
                        $"Stage 6 Foam on '{name}' is disabled because compute shaders or ARGBHalf/RGHalf random-write textures are unavailable.",
                        this);
                    supportWarningReported = true;
                }

                BindDisabled();
                return;
            }

            supportWarningReported = false;

            bool fullyFrozen = river.FreezeAmount >= 0.999f;
            if (fullyFrozen)
            {
                pendingInjections.Clear();
                reservations.Clear();
                ResetManualInjectionSequence();

                if (!fullyFrozenLastUpdate)
                {
                    ClearFoam();
                }

                fullyFrozenLastUpdate = true;
                BindDisabled();
                ReleaseResources();
                return;
            }

            fullyFrozenLastUpdate = false;

            double nowAsDouble = Time.realtimeSinceStartupAsDouble;
            bool autonomousPopulationActive = IsAutonomousPopulationActive;
            if (autonomousPopulationActive)
            {
                autonomousPopulationWasEnabled = true;
                autonomousDecayUntil = 0.0;
            }
            else if (autonomousPopulationWasEnabled)
            {
                // Amount zero stops all new supply immediately, but the complete
                // field remains simulated long enough for existing material to
                // travel, fragment, decay, and then return to the normal sleep
                // and delayed-release path.
                autonomousPopulationWasEnabled = false;
                autonomousDecayUntil = nowAsDouble +
                    Mathf.Max(3f, river.FoamLifetime * 1.25f);
            }

            bool autonomousDecayActive =
                currentState != null && nowAsDouble < autonomousDecayUntil;
            bool hasWork =
                autonomousPopulationActive ||
                autonomousDecayActive ||
                pendingInjections.Count > 0 ||
                reservations.Count > 0 ||
                CountActiveChunks() > 0;

            if (!hasWork && currentState == null)
            {
                BindDisabled();
                return;
            }

            if (!EnsureResources())
            {
                BindDisabled();
                return;
            }

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();

            float now = Time.realtimeSinceStartup;
            float deltaTime = Mathf.Clamp(now - lastRuntimeTime, 0f, 0.1f);
            lastRuntimeTime = now;

            bool manualInjectedThisUpdate = ProcessPendingInjections(now);

            float updateRate = ResolveUpdateRate();
            float stepDuration = 1f / Mathf.Max(1f, updateRate);

            if (manualInjectedThisUpdate)
            {
                // Manual diagnostics remain immediately visible, but they now
                // enter the same autonomous guidance, population, boundary,
                // Wake, Impact, merging, and structural-failure solver on the
                // following simulation step.
                simulationAccumulator = 0f;
                simulationInterpolation = 1f;
            }
            else
            {
                simulationAccumulator = Mathf.Min(
                    simulationAccumulator + deltaTime,
                    stepDuration * 2f);

                while (simulationAccumulator >= stepDuration)
                {
                    simulationAccumulator -= stepDuration;
                    UpdateReservations(stepDuration, now);

                    if (autonomousPopulationActive || autonomousDecayActive)
                    {
                        ActivateAllChunks(now + stepDuration * 3f);
                    }
                    else
                    {
                        UpdateActiveChunks(now);
                    }

                    ConfigureSharedComputeParameters(stepDuration);
                    guidanceAccumulator += stepDuration;
                    populationAccumulator += stepDuration;
                    fractureAccumulator += stepDuration;

                    float guidanceInterval = 1f /
                        Mathf.Max(1f, ResolveGuidanceUpdateRate());
                    if (guidanceAccumulator >= guidanceInterval)
                    {
                        BuildGuidanceField(guidanceAccumulator);
                        guidanceAccumulator %= guidanceInterval;
                    }

                    float populationInterval = 1f /
                        Mathf.Max(1f, ResolvePopulationUpdateRate());
                    if (populationAccumulator >= populationInterval)
                    {
                        MeasurePopulation();
                        populationAccumulator %= populationInterval;
                    }

                    float fractureInterval = 1f /
                        Mathf.Max(1f, ResolveFractureUpdateRate());
                    if (fractureAccumulator >= fractureInterval)
                    {
                        UpdateFractureField(fractureAccumulator);
                        fractureAccumulator %= fractureInterval;
                    }

                    SimulateActiveChunks(stepDuration);
                }

                simulationInterpolation = Mathf.Clamp01(
                    simulationAccumulator / Mathf.Max(0.0001f, stepDuration));
            }

            if (IsSleeping)
            {
                if (idleSince <= 0.0)
                {
                    idleSince = Time.realtimeSinceStartupAsDouble;
                }
                else if (Time.realtimeSinceStartupAsDouble - idleSince >=
                         ResourceReleaseDelaySeconds)
                {
                    BindDisabled();
                    ReleaseResources();
                    return;
                }
            }
            else
            {
                idleSince = 0.0;
            }

            BindField();
            UpdateRecentPeaks();
        }

        public void NotifyRiverChanged()
        {
            if (river == null)
            {
                river = GetComponent<StylizedRiver>();
            }

            if (river == null || !river.Domain.IsValid)
            {
                resourcesDirty = true;
                boundaryDirty = true;
                return;
            }

            bool domainChanged = domainVersion != river.Domain.Version;
            bool qualityChanged =
                currentState != null && allocatedQuality != river.Quality;
            resourcesDirty |= domainChanged || qualityChanged;
            boundaryDirty |= domainChanged;
        }

        public bool EmitNormalized(
            float distanceNormalized,
            float acrossNormalized,
            float radius,
            float amount,
            float freshness,
            float elongation)
        {
            if (river == null)
            {
                river = GetComponent<StylizedRiver>();
            }

            if (river == null || !river.FoamEnabled ||
                river.FreezeAmount >= 0.999f ||
                !river.Domain.IsValid)
            {
                return false;
            }

            float globalDistance = Mathf.Lerp(
                river.Domain.GlobalDistanceMinimum,
                river.Domain.GlobalDistanceMaximum,
                Mathf.Clamp01(distanceNormalized));

            float resolvedAmount = Mathf.Clamp01(amount);
            if (resolvedAmount <= 0.0001f)
            {
                return false;
            }

            int injectionIndex = ++manualInjectionSequence;
            float resolvedFreshness = Mathf.Clamp01(freshness);
            float shapeSeed = river.VisualSeed + injectionIndex * 17.371f;
            float phase = Mathf.Repeat(
                river.VisualSeed * 0.000173f + injectionIndex * 0.6180339f,
                1f);
            float integrity = Mathf.Clamp01(
                Mathf.Lerp(0.78f, 1f, resolvedFreshness));

            pendingInjections.Add(
                new PendingInjection(
                    globalDistance,
                    Mathf.Clamp(acrossNormalized, -1f, 1f),
                    Mathf.Clamp(radius, 0.05f, 8f),
                    resolvedAmount,
                    resolvedFreshness,
                    integrity,
                    phase,
                    Mathf.Clamp(elongation, 0.25f, 8f),
                    true,
                    shapeSeed,
                    river.FoamShapeVariety,
                    true));
            idleSince = 0.0;
            return true;
        }

        public void ClearFoam()
        {
            pendingInjections.Clear();
            reservations.Clear();
            ResetManualInjectionSequence();
            lastInjectionBoundaryCoverage = -1f;
            lastInjectionStateSynchronized = false;
            simulationAccumulator = 0f;
            guidanceAccumulator = 0f;
            populationAccumulator = 0f;
            fractureAccumulator = 0f;
            simulationInterpolation = 1f;
            idleSince = Time.realtimeSinceStartupAsDouble;

            if (stateA != null)
            {
                DispatchClear(stateA, 0, fieldWidth);
            }

            if (stateB != null)
            {
                DispatchClear(stateB, 0, fieldWidth);
            }

            if (advectedState != null)
            {
                DispatchClear(advectedState, 0, fieldWidth);
            }

            if (reverseState != null)
            {
                DispatchClear(reverseState, 0, fieldWidth);
            }

            DispatchClearFracture(fractureA, 0, fractureWidth);
            DispatchClearFracture(fractureB, 0, fractureWidth);
            guidanceAccumulator = 0f;
            populationAccumulator = 0f;
            fractureAccumulator = 0f;

            Array.Clear(chunkActive, 0, chunkActive.Length);
            Array.Clear(chunkActiveUntil, 0, chunkActiveUntil.Length);
        }

        public void ResetRecentPeaks()
        {
            recentPeakDispatches = lastUpdateDispatches;
            recentPeakCellIterations = lastUpdateCellIterations;
            recentPeakWindowEnd = Time.realtimeSinceStartupAsDouble + 5.0;
        }

        private bool EnsureResources()
        {
            if (!resourcesDirty &&
                currentState != null &&
                advectedState != null &&
                reverseState != null &&
                guidanceTexture != null &&
                currentFracture != null &&
                writeFracture != null &&
                neutralDisturbanceTexture != null &&
                neutralDisturbanceTexture.IsCreated() &&
                boundaryTexture != null &&
                metricBuffer != null &&
                populationMetricsBuffer != null &&
                domainVersion == river.Domain.Version &&
                allocatedQuality == river.Quality)
            {
                if (boundaryDirty)
                {
                    RebuildBoundaryTexture();
                }

                return true;
            }

            ReleaseResources();

            computeShader = Resources.Load<ComputeShader>(ComputeResourcePath);
            if (computeShader == null)
            {
                Debug.LogError(
                    $"Stage 6 Foam on '{name}' could not load Resources/{ComputeResourcePath}.compute.",
                    this);
                return false;
            }

            clearKernel = computeShader.FindKernel("ClearRange");
            injectKernel = computeShader.FindKernel("InjectFoam");
            buildGuidanceKernel = computeShader.FindKernel("BuildGuidance");
            resetPopulationKernel = computeShader.FindKernel("ResetPopulation");
            measurePopulationKernel = computeShader.FindKernel("MeasurePopulation");
            updateFractureKernel = computeShader.FindKernel("UpdateFracture");
            clearFractureKernel = computeShader.FindKernel("ClearFractureRange");
            advectForwardKernel = computeShader.FindKernel("AdvectForward");
            advectReverseKernel = computeShader.FindKernel("AdvectReverse");
            simulateKernel = computeShader.FindKernel("SimulateFoam");
            applyBoundaryKernel = computeShader.FindKernel("ApplyBoundary");

            RiverDomainSnapshot domain = river.Domain;
            if (!domain.IsValid)
            {
                return false;
            }

            chunkCount = Mathf.Max(
                1,
                Mathf.CeilToInt(domain.LocalLength / ChunkLengthMetres));
            resolutionPerChunk = river.Quality switch
            {
                StylizedRiverQuality.Low => 32,
                StylizedRiverQuality.Medium => 48,
                StylizedRiverQuality.High => 64,
                _ => 48
            };

            int maximumWidth = Mathf.Max(256, SystemInfo.maxTextureSize);
            resolutionPerChunk = Mathf.Max(
                16,
                Mathf.Min(resolutionPerChunk, maximumWidth / chunkCount));
            fieldWidth = Mathf.Max(16, resolutionPerChunk * chunkCount);
            fieldHeight = river.Quality switch
            {
                StylizedRiverQuality.Low => 32,
                StylizedRiverQuality.Medium => 48,
                StylizedRiverQuality.High => 64,
                _ => 48
            };
            guidanceWidth = Mathf.Max(
                24,
                chunkCount * (river.Quality switch
                {
                    StylizedRiverQuality.Low => 12,
                    StylizedRiverQuality.Medium => 18,
                    StylizedRiverQuality.High => 24,
                    _ => 18
                }));
            guidanceHeight = river.Quality switch
            {
                StylizedRiverQuality.Low => 32,
                StylizedRiverQuality.Medium => 44,
                StylizedRiverQuality.High => 56,
                _ => 44
            };
            fractureWidth = Mathf.Max(16, Mathf.CeilToInt(fieldWidth * 0.5f));
            fractureHeight = Mathf.Max(16, Mathf.CeilToInt(fieldHeight * 0.5f));
            fieldLength = chunkCount * ChunkLengthMetres;
            domainVersion = domain.Version;
            allocatedQuality = river.Quality;

            stateA = CreateFieldTexture("PS3D_RiverFoam_A");
            stateB = CreateFieldTexture("PS3D_RiverFoam_B");
            advectedState = CreateFieldTexture("PS3D_RiverFoam_Advected");
            reverseState = CreateFieldTexture("PS3D_RiverFoam_Reverse");
            guidanceTexture = CreateGuidanceTexture(
                "PS3D_RiverFoam_Guidance");
            fractureA = CreateFractureTexture("PS3D_RiverFoam_FractureA");
            fractureB = CreateFractureTexture("PS3D_RiverFoam_FractureB");
            currentFracture = fractureA;
            writeFracture = fractureB;
            neutralDisturbanceTexture = CreateNeutralDisturbanceTexture();
            previousState = stateA;
            currentState = stateA;
            writeState = stateB;

            populationMetricsBuffer = new ComputeBuffer(
                chunkCount * 8,
                sizeof(uint),
                ComputeBufferType.Raw);

            chunkActive = new bool[chunkCount];
            chunkActiveUntil = new double[chunkCount];

            BuildMetricBuffer();
            RebuildBoundaryTexture(false);

            DispatchClear(stateA, 0, fieldWidth);
            DispatchClear(stateB, 0, fieldWidth);
            DispatchClear(advectedState, 0, fieldWidth);
            DispatchClear(reverseState, 0, fieldWidth);
            DispatchClearFracture(fractureA, 0, fractureWidth);
            DispatchClearFracture(fractureB, 0, fractureWidth);
            BuildGuidanceField(0f);
            MeasurePopulation();

            resourcesDirty = false;
            boundaryDirty = false;
            simulationAccumulator = 0f;
            guidanceAccumulator = 0f;
            populationAccumulator = 0f;
            fractureAccumulator = 0f;
            simulationInterpolation = 1f;
            return true;
        }

        private RenderTexture CreateFieldTexture(string textureName)
        {
            RenderTexture texture = new RenderTexture(
                fieldWidth,
                fieldHeight,
                0,
                RenderTextureFormat.ARGBHalf,
                RenderTextureReadWrite.Linear)
            {
                name = textureName,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            texture.Create();
            return texture;
        }


        private RenderTexture CreateGuidanceTexture(string textureName)
        {
            RenderTexture texture = new RenderTexture(
                guidanceWidth,
                guidanceHeight,
                0,
                RenderTextureFormat.ARGBHalf,
                RenderTextureReadWrite.Linear)
            {
                name = textureName,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            texture.Create();
            return texture;
        }

        private RenderTexture CreateFractureTexture(string textureName)
        {
            RenderTexture texture = new RenderTexture(
                fractureWidth,
                fractureHeight,
                0,
                RenderTextureFormat.RGHalf,
                RenderTextureReadWrite.Linear)
            {
                name = textureName,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            texture.Create();
            return texture;
        }

        private static RenderTexture CreateNeutralDisturbanceTexture()
        {
            RenderTexture texture = new RenderTexture(
                1,
                1,
                0,
                RenderTextureFormat.ARGBHalf,
                RenderTextureReadWrite.Linear)
            {
                name = "PS3D_RiverFoam_NeutralDisturbance",
                enableRandomWrite = false,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            texture.Create();

            RenderTexture previous = RenderTexture.active;
            try
            {
                RenderTexture.active = texture;
                GL.Clear(false, true, Color.clear);
            }
            finally
            {
                RenderTexture.active = previous;
            }

            return texture;
        }

        private void BuildMetricBuffer()
        {
            metricRows = new FoamMetricRow[fieldWidth];
            float longitudinalSpacing =
                fieldLength / Mathf.Max(1, fieldWidth - 1);

            for (int x = 0; x < fieldWidth; x++)
            {
                float globalDistance =
                    river.Domain.GlobalDistanceMinimum +
                    x / (float)Mathf.Max(1, fieldWidth - 1) * fieldLength;
                StylizedRiverSplineSample sample =
                    river.Domain.SampleAtGlobalDistance(globalDistance);
                float left = Mathf.Max(0.05f, sample.LeftSurfaceHalfWidth);
                float right = Mathf.Max(0.05f, sample.RightSurfaceHalfWidth);
                float minimumLateralSpacing =
                    (left + right) / Mathf.Max(1, fieldHeight - 1);

                metricRows[x] = new FoamMetricRow
                {
                    WidthsAndSpacing = new Vector4(
                        left,
                        right,
                        longitudinalSpacing,
                        minimumLateralSpacing)
                };
            }

            metricBuffer?.Release();
            metricBuffer = new ComputeBuffer(
                fieldWidth,
                Marshal.SizeOf<FoamMetricRow>(),
                ComputeBufferType.Structured);
            metricBuffer.SetData(metricRows);
        }

        private void RebuildBoundaryTexture(bool applyToExistingState = true)
        {
            if (fieldWidth <= 0 || fieldHeight <= 0 || metricRows.Length != fieldWidth)
            {
                return;
            }

            Color[] pixels = new Color[fieldWidth * fieldHeight];
            float edgeCells = river.Quality switch
            {
                StylizedRiverQuality.Low => 1.5f,
                StylizedRiverQuality.Medium => 2.0f,
                StylizedRiverQuality.High => 2.5f,
                _ => 2.0f
            };

            for (int x = 0; x < fieldWidth; x++)
            {
                float globalDistance =
                    river.Domain.GlobalDistanceMinimum +
                    x / (float)Mathf.Max(1, fieldWidth - 1) * fieldLength;
                StylizedRiverSplineSample sample =
                    river.Domain.SampleAtGlobalDistance(globalDistance);
                float leftSurface = Mathf.Max(0.05f, sample.LeftSurfaceHalfWidth);
                float rightSurface = Mathf.Max(0.05f, sample.RightSurfaceHalfWidth);
                float leftVisible = Mathf.Max(0.01f, sample.LeftHalfWidth);
                float rightVisible = Mathf.Max(0.01f, sample.RightHalfWidth);
                float animatedEnvelope = Mathf.Lerp(
                    0.25f,
                    0.90f,
                    Mathf.Clamp01(river.ShoreMotion));
                float leftFoamReach = Mathf.Lerp(
                    leftVisible,
                    leftSurface,
                    animatedEnvelope);
                float rightFoamReach = Mathf.Lerp(
                    rightVisible,
                    rightSurface,
                    animatedEnvelope);
                float edgeWidth = Mathf.Max(
                    0.05f,
                    (leftSurface + rightSurface) /
                    Mathf.Max(1, fieldHeight - 1) * edgeCells);

                for (int y = 0; y < fieldHeight; y++)
                {
                    float across01 = y / (float)Mathf.Max(1, fieldHeight - 1);
                    float lateral = Across01ToMetres(
                        across01,
                        leftSurface,
                        rightSurface);
                    float foamReach = lateral < 0f
                        ? leftFoamReach
                        : rightFoamReach;
                    float distanceInsideReach = foamReach - Mathf.Abs(lateral);
                    float coverage = Mathf.Clamp01(
                        distanceInsideReach / edgeWidth);
                    float attraction = coverage *
                        (1f - Mathf.SmoothStep(
                            0f,
                            1f,
                            Mathf.InverseLerp(0.10f, 0.95f, coverage)));
                    pixels[y * fieldWidth + x] = new Color(
                        coverage,
                        attraction,
                        0f,
                        1f);
                }
            }

            GeneratedGeometryRegistry.CopySourcesTo(generatedSources);
            for (int index = 0; index < generatedSources.Count; index++)
            {
                IGeneratedGeometrySource source = generatedSources[index];
                if (!IsUsableStaticSource(source))
                {
                    continue;
                }

                if (!RiverDisturbanceFootprintResolver.TryResolve(
                        river,
                        source.GeometryMeshFilter,
                        0f,
                        out RiverDisturbanceFootprint footprint,
                        out _))
                {
                    continue;
                }

                RasterizeObstacle(footprint, pixels);
            }

            if (boundaryTexture == null ||
                boundaryTexture.width != fieldWidth ||
                boundaryTexture.height != fieldHeight)
            {
                if (boundaryTexture != null)
                {
                    DestroyUnityObject(boundaryTexture);
                }

                boundaryTexture = new Texture2D(
                    fieldWidth,
                    fieldHeight,
                    TextureFormat.RGHalf,
                    false,
                    true)
                {
                    name = "T_PS3D_RiverFoamBoundary_Runtime",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.DontSave
                };
            }

            boundaryTexture.SetPixels(pixels);
            boundaryTexture.Apply(false, false);
            boundaryDirty = false;

            if (applyToExistingState)
            {
                ApplyBoundaryToState(stateA);
                ApplyBoundaryToState(stateB);
            }
        }

        private void RasterizeObstacle(
            RiverDisturbanceFootprint footprint,
            Color[] pixels)
        {
            if (footprint.Contour == null || footprint.Contour.Length < 3 ||
                footprint.WorldDownstream.sqrMagnitude < 0.0001f ||
                footprint.WorldAcross.sqrMagnitude < 0.0001f)
            {
                return;
            }

            // The cached contour is source-local, not river-local. Reconstruct
            // every actual world-space vertex and project it independently so
            // object rotation, irregular outlines, and changing river frames
            // survive the boundary bake.
            Vector2[] projectedContour =
                new Vector2[footprint.Contour.Length];
            float minimumGlobal = float.PositiveInfinity;
            float maximumGlobal = float.NegativeInfinity;
            float minimumAcross = float.PositiveInfinity;
            float maximumAcross = float.NegativeInfinity;

            for (int index = 0; index < footprint.Contour.Length; index++)
            {
                Vector2 localVertex = footprint.Contour[index];
                Vector3 worldVertex =
                    footprint.WorldPosition +
                    footprint.WorldDownstream * localVertex.x +
                    footprint.WorldAcross * localVertex.y;

                if (!river.TryProjectWorldPoint(
                        worldVertex,
                        out StylizedRiverProjection projection))
                {
                    return;
                }

                Vector2 projectedVertex = new Vector2(
                    projection.GlobalDistance,
                    projection.AcrossMetres);
                projectedContour[index] = projectedVertex;
                minimumGlobal = Mathf.Min(minimumGlobal, projectedVertex.x);
                maximumGlobal = Mathf.Max(maximumGlobal, projectedVertex.x);
                minimumAcross = Mathf.Min(minimumAcross, projectedVertex.y);
                maximumAcross = Mathf.Max(maximumAcross, projectedVertex.y);
            }

            if (float.IsInfinity(minimumGlobal) ||
                float.IsInfinity(minimumAcross))
            {
                return;
            }

            int minimumX = GlobalDistanceToX(minimumGlobal) - 2;
            int maximumX = GlobalDistanceToX(maximumGlobal) + 2;
            minimumX = Mathf.Clamp(minimumX, 0, fieldWidth - 1);
            maximumX = Mathf.Clamp(maximumX, 0, fieldWidth - 1);

            float projectedAcrossExtent = Mathf.Max(
                0.04f,
                maximumAcross - minimumAcross);
            float edgeWidth = Mathf.Max(
                0.04f,
                projectedAcrossExtent / Mathf.Max(2f, fieldHeight * 0.50f));

            for (int x = minimumX; x <= maximumX; x++)
            {
                float globalDistance = XToGlobalDistance(x);
                Vector4 metrics = metricRows[x].WidthsAndSpacing;
                int minimumY = AcrossMetresToY(
                    minimumAcross - edgeWidth * 2f,
                    metrics.x,
                    metrics.y) - 1;
                int maximumY = AcrossMetresToY(
                    maximumAcross + edgeWidth * 2f,
                    metrics.x,
                    metrics.y) + 1;
                minimumY = Mathf.Clamp(minimumY, 0, fieldHeight - 1);
                maximumY = Mathf.Clamp(maximumY, 0, fieldHeight - 1);

                for (int y = minimumY; y <= maximumY; y++)
                {
                    float across01 = y / (float)Mathf.Max(1, fieldHeight - 1);
                    float lateral = Across01ToMetres(
                        across01,
                        metrics.x,
                        metrics.y);
                    Vector2 riverPoint = new Vector2(
                        globalDistance,
                        lateral);

                    bool inside = PointInPolygon(
                        riverPoint,
                        projectedContour);
                    float edgeDistance = DistanceToPolygon(
                        riverPoint,
                        projectedContour);
                    float obstacleCoverage = inside
                        ? 0f
                        : Mathf.Clamp01(edgeDistance / edgeWidth);
                    int pixelIndex = y * fieldWidth + x;
                    Color previous = pixels[pixelIndex];
                    previous.r = Mathf.Min(previous.r, obstacleCoverage);
                    if (!inside)
                    {
                        float obstacleAttraction = Mathf.Clamp01(
                            1f - edgeDistance /
                            Mathf.Max(0.001f, edgeWidth * 5.5f));
                        previous.g = Mathf.Max(
                            previous.g,
                            obstacleAttraction * obstacleCoverage);
                    }
                    else
                    {
                        previous.g = 0f;
                    }
                    pixels[pixelIndex] = previous;
                }
            }
        }

        private void ResetManualInjectionSequence()
        {
            manualInjectionSequence = 0;
        }

        private bool ProcessPendingInjections(float now)
        {
            if (pendingInjections.Count == 0)
            {
                return false;
            }

            bool manualInjected = false;
            for (int index = 0; index < pendingInjections.Count; index++)
            {
                PendingInjection injection = pendingInjections[index];
                ActivateInjectionRange(injection, now);
                DispatchInjection(injection);
                reservations.Add(CreateReservation(injection));

                injectedLastUpdate++;
                manualInjected = true;
            }

            pendingInjections.Clear();
            return manualInjected;
        }

        private FoamReservation CreateReservation(PendingInjection injection)
        {
            return new FoamReservation
            {
                CentreGlobalDistance = injection.GlobalDistance,
                AlongRadius = injection.Radius * injection.Elongation,
                RemainingAmount = injection.Amount,
                Elapsed = 0f,
                MaximumLifetime = MaximumManualReservationSeconds
            };
        }

        private void UpdateReservations(float deltaTime, float now)
        {
            float liquid = river.LiquidFactor;
            float speed =
                river.FlowSpeedMetresPerSecond *
                river.FoamFlowFollow * liquid;
            float amountDecay =
                DecayToFivePercent / Mathf.Max(0.05f, river.FoamLifetime);
            float spread = river.FoamLateralSpread * river.FoamEvolution;

            for (int index = reservations.Count - 1; index >= 0; index--)
            {
                FoamReservation reservation = reservations[index];
                reservation.Elapsed += deltaTime;
                reservation.CentreGlobalDistance += speed * deltaTime;
                reservation.AlongRadius +=
                    (0.15f + spread * 0.35f) * deltaTime;
                reservation.RemainingAmount *=
                    Mathf.Exp(-amountDecay * deltaTime);

                if (reservation.Elapsed >= reservation.MaximumLifetime ||
                    reservation.RemainingAmount < 0.015f)
                {
                    reservations.RemoveAt(index);
                    continue;
                }

                ActivateReservationRange(reservation, now);
            }
        }

        private void ActivateInjectionRange(PendingInjection injection, float now)
        {
            float padding = Mathf.Max(0.5f, injection.Radius * injection.Elongation);
            ActivateGlobalRange(
                injection.GlobalDistance - padding,
                injection.GlobalDistance + padding,
                now + Mathf.Max(river.FoamLifetime, river.FoamFreshnessLifetime));
        }

        private void ActivateReservationRange(FoamReservation reservation, float now)
        {
            float margin = Mathf.Max(
                0.5f,
                Mathf.Abs(river.FlowSpeedMetresPerSecond * river.FoamFlowFollow) /
                Mathf.Max(1f, ResolveUpdateRate()) * 2f);
            ActivateGlobalRange(
                reservation.CentreGlobalDistance - reservation.AlongRadius - margin,
                reservation.CentreGlobalDistance + reservation.AlongRadius + margin,
                now + 1.5f / Mathf.Max(1f, ResolveUpdateRate()));
        }

        private void ActivateGlobalRange(
            float minimumGlobal,
            float maximumGlobal,
            double activeUntil)
        {
            int minimumChunk = GlobalDistanceToChunk(minimumGlobal);
            int maximumChunk = GlobalDistanceToChunk(maximumGlobal);

            for (int chunk = minimumChunk; chunk <= maximumChunk; chunk++)
            {
                if (!chunkActive[chunk])
                {
                    // Inactive chunks are already cleared by allocation or
                    // the existing sleep path; reactivation needs no extra
                    // clear dispatch.
                    chunkActive[chunk] = true;
                }

                chunkActiveUntil[chunk] = Math.Max(
                    chunkActiveUntil[chunk],
                    activeUntil);
            }
        }

        private void ActivateAllChunks(double activeUntil)
        {
            for (int chunk = 0; chunk < chunkCount; chunk++)
            {
                chunkActive[chunk] = true;
                chunkActiveUntil[chunk] = Math.Max(
                    chunkActiveUntil[chunk],
                    activeUntil);
            }
        }

        private void UpdateActiveChunks(float now)
        {
            for (int chunk = 0; chunk < chunkCount; chunk++)
            {
                if (!chunkActive[chunk] || now <= chunkActiveUntil[chunk])
                {
                    continue;
                }

                chunkActive[chunk] = false;
                chunkActiveUntil[chunk] = 0.0;
                ClearChunk(chunk);
            }
        }

        private void SimulateActiveChunks(float deltaTime)
        {
            if (CountActiveChunks() == 0)
            {
                return;
            }

            previousState = currentState;

            int chunk = 0;
            while (chunk < chunkCount)
            {
                while (chunk < chunkCount && !chunkActive[chunk])
                {
                    chunk++;
                }

                if (chunk >= chunkCount)
                {
                    break;
                }

                int startChunk = chunk;
                while (chunk < chunkCount && chunkActive[chunk])
                {
                    chunk++;
                }

                int endChunkExclusive = chunk;
                int startX = startChunk * resolutionPerChunk;
                int countX = Mathf.Min(
                    fieldWidth - startX,
                    (endChunkExclusive - startChunk) * resolutionPerChunk);
                DispatchSimulation(startX, countX);
            }

            (currentState, writeState) = (writeState, currentState);
        }

        private void DispatchInjection(PendingInjection injection)
        {
            int centreX = GlobalDistanceToX(injection.GlobalDistance);
            float alongRadius = injection.Radius * injection.Elongation *
                (injection.CompoundShape ? 1.25f : 1f);
            float dx = fieldLength / Mathf.Max(1, fieldWidth - 1);
            int radiusPixels = Mathf.CeilToInt(alongRadius / Mathf.Max(0.001f, dx)) + 2;
            int startX = Mathf.Clamp(centreX - radiusPixels, 0, fieldWidth - 1);
            int endX = Mathf.Clamp(centreX + radiusPixels, 0, fieldWidth - 1);
            int countX = endX - startX + 1;

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetInt("_FoamRangeStart", startX);
            computeShader.SetInt("_FoamRangeCount", countX);
            computeShader.SetFloat("_FoamGlobalStart", river.Domain.GlobalDistanceMinimum);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat("_FoamInjectionGlobalDistance", injection.GlobalDistance);
            computeShader.SetFloat("_FoamInjectionAcrossNormalized", injection.AcrossNormalized);
            computeShader.SetFloat("_FoamInjectionRadius", injection.Radius);
            computeShader.SetFloat("_FoamInjectionAmount", injection.Amount);
            computeShader.SetFloat("_FoamInjectionFreshness", injection.Freshness);
            computeShader.SetFloat("_FoamInjectionIntegrity", injection.Integrity);
            computeShader.SetFloat("_FoamInjectionPhase", injection.Phase);
            computeShader.SetFloat("_FoamInjectionElongation", injection.Elongation);
            computeShader.SetFloat("_FoamInjectionShapeSeed", injection.ShapeSeed);
            computeShader.SetFloat("_FoamInjectionShapeVariety", injection.ShapeVariety);
            computeShader.SetFloat(
                "_FoamInjectionCompound",
                injection.CompoundShape ? 1f : 0f);
            computeShader.SetBuffer(injectKernel, "_FoamMetricRows", metricBuffer);
            computeShader.SetTexture(injectKernel, "_FoamBoundary", boundaryTexture);

            DispatchInjectionToState(currentState, countX);

            if (injection.IsManual)
            {
                // Manual diagnostics exist in both temporal states so
                // interpolation cannot hide a fresh source behind an empty
                // previous field. They then evolve through the complete solver.
                if (stateA != null && stateA != currentState)
                {
                    DispatchInjectionToState(stateA, countX);
                }

                if (stateB != null && stateB != currentState && stateB != stateA)
                {
                    DispatchInjectionToState(stateB, countX);
                }

                lastInjectionStateSynchronized = stateA != null && stateB != null;
                lastInjectionBoundaryCoverage = SampleInjectionBoundaryCoverage(injection);
                simulationInterpolation = 1f;
            }
        }

        private void DispatchInjectionToState(RenderTexture target, int countX)
        {
            if (target == null)
            {
                return;
            }

            computeShader.SetTexture(injectKernel, "_FoamStateWrite", target);
            Dispatch(injectKernel, countX, fieldHeight);
        }

        private float SampleInjectionBoundaryCoverage(PendingInjection injection)
        {
            if (boundaryTexture == null || fieldLength <= 0.0001f)
            {
                return -1f;
            }

            float u = Mathf.Clamp01(
                (injection.GlobalDistance - river.Domain.GlobalDistanceMinimum) /
                fieldLength);
            float v = Mathf.Clamp01(injection.AcrossNormalized * 0.5f + 0.5f);
            return Mathf.Clamp01(boundaryTexture.GetPixelBilinear(u, v).r);
        }

        private void BuildGuidanceField(float deltaTime)
        {
            if (computeShader == null || guidanceTexture == null ||
                buildGuidanceKernel < 0)
            {
                return;
            }

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetInts(
                "_FoamGuidanceDimensions",
                guidanceWidth,
                guidanceHeight);
            computeShader.SetInts(
                "_FoamFractureDimensions",
                fractureWidth,
                fractureHeight);
            computeShader.SetInt("_FoamChunkCount", chunkCount);
            computeShader.SetFloat(
                "_FoamGlobalStart",
                river.Domain.GlobalDistanceMinimum);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat("_FoamDeltaTime", deltaTime);
            computeShader.SetFloat("_FoamTime", river.MotionTime);
            computeShader.SetFloat("_FoamSeed", river.VisualSeed);
            computeShader.SetFloat("_FoamEvolution", river.FoamEvolution);
            computeShader.SetBuffer(
                buildGuidanceKernel,
                "_FoamMetricRows",
                metricBuffer);
            computeShader.SetTexture(
                buildGuidanceKernel,
                "_FoamGuidanceWrite",
                guidanceTexture);
            Dispatch(buildGuidanceKernel, guidanceWidth, guidanceHeight);
        }

        private void MeasurePopulation()
        {
            if (computeShader == null || currentState == null ||
                boundaryTexture == null || populationMetricsBuffer == null ||
                resetPopulationKernel < 0 || measurePopulationKernel < 0)
            {
                return;
            }

            computeShader.SetInt("_FoamChunkCount", chunkCount);
            computeShader.SetBuffer(
                resetPopulationKernel,
                "_FoamPopulationMetrics",
                populationMetricsBuffer);
            DispatchOneDimensional(
                resetPopulationKernel,
                chunkCount,
                64);

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetInt(
                "_FoamResolutionPerChunk",
                resolutionPerChunk);
            computeShader.SetFloat(
                "_FoamVisibleThreshold",
                river.FoamPopulationVisibleThreshold);
            computeShader.SetTexture(
                measurePopulationKernel,
                "_FoamStateRead",
                currentState);
            computeShader.SetTexture(
                measurePopulationKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetInts(
                "_FoamGuidanceDimensions",
                guidanceWidth,
                guidanceHeight);
            computeShader.SetTexture(
                measurePopulationKernel,
                "_FoamGuidanceRead",
                guidanceTexture);
            computeShader.SetBuffer(
                measurePopulationKernel,
                "_FoamPopulationMetrics",
                populationMetricsBuffer);
            Dispatch(measurePopulationKernel, fieldWidth, fieldHeight);
        }

        private void UpdateFractureField(float deltaTime)
        {
            if (computeShader == null || currentState == null ||
                currentFracture == null || writeFracture == null ||
                updateFractureKernel < 0)
            {
                return;
            }

            computeShader.SetFloat(
                "_FoamFractureDeltaTime",
                Mathf.Max(0.0001f, deltaTime));
            computeShader.SetTexture(
                updateFractureKernel,
                "_FoamStateRead",
                currentState);
            computeShader.SetTexture(
                updateFractureKernel,
                "_FoamFractureRead",
                currentFracture);
            computeShader.SetTexture(
                updateFractureKernel,
                "_FoamFractureWrite",
                writeFracture);
            Dispatch(updateFractureKernel, fractureWidth, fractureHeight);
            (currentFracture, writeFracture) =
                (writeFracture, currentFracture);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamFractureRead",
                currentFracture);
        }

        private void ConfigureSharedComputeParameters(float deltaTime)
        {
            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetInts(
                "_FoamGuidanceDimensions",
                guidanceWidth,
                guidanceHeight);
            computeShader.SetInts(
                "_FoamFractureDimensions",
                fractureWidth,
                fractureHeight);
            computeShader.SetInt(
                "_FoamResolutionPerChunk",
                resolutionPerChunk);
            computeShader.SetInt("_FoamChunkCount", chunkCount);
            computeShader.SetFloat("_FoamDeltaTime", deltaTime);
            computeShader.SetFloat(
                "_FoamGlobalStart",
                river.Domain.GlobalDistanceMinimum);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat(
                "_FoamFlowSpeed",
                river.FlowSpeedMetresPerSecond *
                river.FoamFlowFollow *
                river.LiquidFactor);
            computeShader.SetFloat("_FoamEvolution", river.FoamEvolution);
            computeShader.SetFloat("_FoamBreakup", river.FoamBreakup);
            computeShader.SetFloat("_FoamSpread", river.FoamLateralSpread);
            computeShader.SetFloat("_FoamCohesion", river.FoamCohesion);
            computeShader.SetFloat(
                "_FoamConnectivity",
                river.FoamConnectivity);
            computeShader.SetFloat(
                "_FoamAmountDecay",
                DecayToFivePercent /
                Mathf.Max(0.05f, river.FoamLifetime));
            computeShader.SetFloat(
                "_FoamFreshnessDecay",
                DecayToFivePercent /
                Mathf.Max(0.05f, river.FoamFreshnessLifetime));
            computeShader.SetFloat(
                "_FoamIntegrityDamage",
                Mathf.Clamp01(river.FoamFragmentation));
            computeShader.SetFloat(
                "_FoamShoreRetention",
                river.FoamShoreRetention);
            computeShader.SetFloat("_FoamTime", river.MotionTime);
            computeShader.SetFloat("_FoamSeed", river.VisualSeed);
            computeShader.SetFloat(
                "_FoamTargetCoverage",
                IsAutonomousPopulationActive
                    ? river.FoamTargetCoverage
                    : 0f);
            computeShader.SetFloat(
                "_FoamSupplyRate",
                IsAutonomousPopulationActive
                    ? river.FoamSupplyRate
                    : 0f);
            computeShader.SetFloat(
                "_FoamVisibleThreshold",
                river.FoamPopulationVisibleThreshold);
            computeShader.SetFloat(
                "_FoamGuidanceStrength",
                river.FoamGuidanceStrength);
            computeShader.SetFloat(
                "_FoamBoundaryAttraction",
                river.FoamBoundaryAttraction);
            computeShader.SetFloat(
                "_FoamWakeReinforcement",
                river.FoamWakeReinforcement);
            computeShader.SetFloat(
                "_FoamImpactReinforcement",
                river.FoamImpactReinforcement);

            bool disturbanceAvailable =
                disturbanceRuntime != null &&
                disturbanceRuntime.IsAllocated;

            RenderTexture wakeSource = disturbanceAvailable
                ? disturbanceRuntime.CurrentWakeTexture
                : null;
            RenderTexture rippleSource = disturbanceAvailable
                ? disturbanceRuntime.CurrentRippleTexture
                : null;
            RenderTexture staticWakeSource = disturbanceAvailable
                ? disturbanceRuntime.StaticWakeSourceTexture
                : null;
            RenderTexture staticPressureSource = disturbanceAvailable
                ? disturbanceRuntime.StaticPressureTexture
                : null;

            bool wakeAvailable = IsCreatedTexture(wakeSource);
            bool rippleAvailable = IsCreatedTexture(rippleSource);
            bool staticWakeAvailable = IsCreatedTexture(staticWakeSource);
            bool staticPressureAvailable =
                IsCreatedTexture(staticPressureSource);

            // Every texture declared by a compute kernel must be bound for
            // every dispatch. Stage 5 allocates its optional fields
            // independently, so an allocated disturbance runtime does not
            // guarantee that each individual texture is already created.
            // Bind one explicit zero-valued RenderTexture for every missing
            // input rather than relying on a built-in Texture2D fallback.
            Texture wakeTexture = wakeAvailable
                ? wakeSource
                : neutralDisturbanceTexture;
            Texture rippleTexture = rippleAvailable
                ? rippleSource
                : neutralDisturbanceTexture;
            Texture staticWakeTexture = staticWakeAvailable
                ? staticWakeSource
                : neutralDisturbanceTexture;
            Texture staticPressureTexture = staticPressureAvailable
                ? staticPressureSource
                : neutralDisturbanceTexture;

            Vector2Int wakeDimensions = wakeAvailable
                ? new Vector2Int(wakeSource.width, wakeSource.height)
                : Vector2Int.one;
            Vector2Int rippleDimensions = rippleAvailable
                ? new Vector2Int(rippleSource.width, rippleSource.height)
                : Vector2Int.one;
            Vector2Int staticWakeDimensions = staticWakeAvailable
                ? new Vector2Int(staticWakeSource.width, staticWakeSource.height)
                : Vector2Int.one;
            Vector2Int staticPressureDimensions = staticPressureAvailable
                ? new Vector2Int(
                    staticPressureSource.width,
                    staticPressureSource.height)
                : Vector2Int.one;

            computeShader.SetFloat(
                "_FoamDisturbanceEnabled",
                disturbanceAvailable ? 1f : 0f);
            computeShader.SetInts(
                "_FoamWakeDimensions",
                wakeDimensions.x,
                wakeDimensions.y);
            computeShader.SetInts(
                "_FoamRippleDimensions",
                rippleDimensions.x,
                rippleDimensions.y);
            computeShader.SetInts(
                "_FoamStaticWakeDimensions",
                staticWakeDimensions.x,
                staticWakeDimensions.y);
            computeShader.SetInts(
                "_FoamStaticPressureDimensions",
                staticPressureDimensions.x,
                staticPressureDimensions.y);

            BindMotionKernel(
                advectForwardKernel,
                wakeTexture,
                rippleTexture,
                staticWakeTexture,
                staticPressureTexture);
            BindMotionKernel(
                advectReverseKernel,
                wakeTexture,
                rippleTexture,
                staticWakeTexture,
                staticPressureTexture);
            BindMotionKernel(
                simulateKernel,
                wakeTexture,
                rippleTexture,
                staticWakeTexture,
                staticPressureTexture);
            BindMotionKernel(
                updateFractureKernel,
                wakeTexture,
                rippleTexture,
                staticWakeTexture,
                staticPressureTexture);

            computeShader.SetTexture(
                advectForwardKernel,
                "_FoamStateRead",
                currentState);
            computeShader.SetTexture(
                advectForwardKernel,
                "_FoamAdvectionWrite",
                advectedState);

            computeShader.SetTexture(
                advectReverseKernel,
                "_FoamAdvectedRead",
                advectedState);
            computeShader.SetTexture(
                advectReverseKernel,
                "_FoamAdvectionWrite",
                reverseState);

            computeShader.SetBuffer(
                simulateKernel,
                "_FoamPopulationMetrics",
                populationMetricsBuffer);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamStateRead",
                currentState);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamAdvectedRead",
                advectedState);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamReverseRead",
                reverseState);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamFractureRead",
                currentFracture);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamStateWrite",
                writeState);
        }

        private void BindMotionKernel(
            int kernel,
            Texture wakeTexture,
            Texture rippleTexture,
            Texture staticWakeTexture,
            Texture staticPressureTexture)
        {
            computeShader.SetBuffer(
                kernel,
                "_FoamMetricRows",
                metricBuffer);
            computeShader.SetTexture(
                kernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamGuidanceRead",
                guidanceTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamWakeField",
                wakeTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamRippleField",
                rippleTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamStaticWakeField",
                staticWakeTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamStaticPressureField",
                staticPressureTexture);
        }

        private void DispatchSimulation(int startX, int countX)
        {
            computeShader.SetInt("_FoamRangeStart", startX);
            computeShader.SetInt("_FoamRangeCount", countX);

            // Corrected transport is intentionally a three-dispatch sequence:
            // forward advection, reverse error estimate, then bounded correction
            // plus population, topology, tearing, capture, and reinforcement.
            Dispatch(advectForwardKernel, countX, fieldHeight);
            Dispatch(advectReverseKernel, countX, fieldHeight);
            Dispatch(simulateKernel, countX, fieldHeight);
        }

        private void DispatchClearFracture(
            RenderTexture target,
            int startX,
            int countX)
        {
            if (computeShader == null || target == null ||
                clearFractureKernel < 0 || countX <= 0)
            {
                return;
            }

            computeShader.SetInts(
                "_FoamFractureDimensions",
                fractureWidth,
                fractureHeight);
            computeShader.SetInt("_FoamFractureRangeStart", startX);
            computeShader.SetInt("_FoamFractureRangeCount", countX);
            computeShader.SetTexture(
                clearFractureKernel,
                "_FoamFractureWrite",
                target);
            Dispatch(clearFractureKernel, countX, fractureHeight);
        }

        private void DispatchClear(RenderTexture target, int startX, int countX)
        {
            if (computeShader == null || target == null || clearKernel < 0 || countX <= 0)
            {
                return;
            }

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetInt("_FoamRangeStart", startX);
            computeShader.SetInt("_FoamRangeCount", countX);
            computeShader.SetTexture(clearKernel, "_FoamStateWrite", target);
            Dispatch(clearKernel, countX, fieldHeight);
        }


        private void ApplyBoundaryToState(RenderTexture target)
        {
            if (computeShader == null || target == null ||
                boundaryTexture == null || applyBoundaryKernel < 0 ||
                fieldWidth <= 0 || fieldHeight <= 0)
            {
                return;
            }

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetInt("_FoamRangeStart", 0);
            computeShader.SetInt("_FoamRangeCount", fieldWidth);
            computeShader.SetTexture(
                applyBoundaryKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetTexture(
                applyBoundaryKernel,
                "_FoamStateWrite",
                target);
            Dispatch(applyBoundaryKernel, fieldWidth, fieldHeight);
        }

        private void ClearChunk(int chunk)
        {
            if (stateA == null || stateB == null || chunk < 0 || chunk >= chunkCount)
            {
                return;
            }

            int startX = chunk * resolutionPerChunk;
            int countX = Mathf.Min(resolutionPerChunk, fieldWidth - startX);
            DispatchClear(stateA, startX, countX);
            DispatchClear(stateB, startX, countX);
            DispatchClear(advectedState, startX, countX);
            DispatchClear(reverseState, startX, countX);

            int fractureStart = Mathf.Clamp(
                Mathf.FloorToInt(startX / (float)Mathf.Max(1, fieldWidth) * fractureWidth),
                0,
                Mathf.Max(0, fractureWidth - 1));
            int fractureEnd = Mathf.Clamp(
                Mathf.CeilToInt((startX + countX) / (float)Mathf.Max(1, fieldWidth) * fractureWidth),
                fractureStart + 1,
                fractureWidth);
            int fractureCount = fractureEnd - fractureStart;
            DispatchClearFracture(fractureA, fractureStart, fractureCount);
            DispatchClearFracture(fractureB, fractureStart, fractureCount);
        }

        private void Dispatch(int kernel, int width, int height)
        {
            int groupsX = Mathf.CeilToInt(width / (float)ThreadGroupSize);
            int groupsY = Mathf.CeilToInt(height / (float)ThreadGroupSize);
            computeShader.Dispatch(kernel, groupsX, groupsY, 1);
            lastUpdateDispatches++;
            lastUpdateCellIterations += (long)width * height;
        }

        private void DispatchOneDimensional(
            int kernel,
            int count,
            int threadsPerGroup)
        {
            if (count <= 0)
            {
                return;
            }

            int groups = Mathf.CeilToInt(
                count / (float)Mathf.Max(1, threadsPerGroup));
            computeShader.Dispatch(kernel, groups, 1, 1);
            lastUpdateDispatches++;
            lastUpdateCellIterations += count;
        }

        private void BindField()
        {
            if (surfaceRenderer == null || currentState == null || previousState == null)
            {
                BindDisabled();
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            surfaceRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(FoamEnabledId, 1f);
            propertyBlock.SetTexture(FoamPreviousId, previousState);
            propertyBlock.SetTexture(FoamCurrentId, currentState);
            propertyBlock.SetTexture(FoamGuidanceId, guidanceTexture);
            propertyBlock.SetTexture(FoamFractureId, currentFracture);
            propertyBlock.SetTexture(FoamBoundaryId, boundaryTexture);
            propertyBlock.SetFloat(FoamInterpolationId, simulationInterpolation);
            propertyBlock.SetFloat(FoamGlobalStartId, river.Domain.GlobalDistanceMinimum);
            propertyBlock.SetFloat(FoamFieldLengthId, Mathf.Max(0.001f, fieldLength));
            propertyBlock.SetColor(FoamColourId, river.FoamColour);
            propertyBlock.SetFloat(FoamStrengthId, river.FoamStrength);
            propertyBlock.SetFloat(FoamCoverageId, river.FoamCoverage);
            propertyBlock.SetFloat(FoamSharpnessId, river.FoamSharpness);
            propertyBlock.SetFloat(FoamDetailScaleId, river.FoamDetailScale);
            propertyBlock.SetFloat(FoamDetailStrengthId, river.FoamDetailStrength);
            propertyBlock.SetFloat(FoamDebugViewId, (float)river.FoamDebugView);
            propertyBlock.SetFloat(FoamSeedId, river.VisualSeed);
            surfaceRenderer.SetPropertyBlock(propertyBlock);
        }

        private void BindDisabled()
        {
            if (surfaceRenderer == null && river != null)
            {
                surfaceRenderer = river.SurfaceRenderer;
            }

            if (surfaceRenderer == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            surfaceRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(FoamEnabledId, 0f);
            propertyBlock.SetTexture(FoamGuidanceId, Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamFractureId, Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamBoundaryId, Texture2D.blackTexture);
            propertyBlock.SetFloat(FoamInterpolationId, 1f);
            propertyBlock.SetFloat(
                FoamDebugViewId,
                river != null && river.FoamEnabled
                    ? (float)river.FoamDebugView
                    : 0f);
            surfaceRenderer.SetPropertyBlock(propertyBlock);
        }

        private void ReleaseResources()
        {
            ReleaseTexture(ref stateA);
            ReleaseTexture(ref stateB);
            ReleaseTexture(ref advectedState);
            ReleaseTexture(ref reverseState);
            ReleaseTexture(ref guidanceTexture);
            ReleaseTexture(ref fractureA);
            ReleaseTexture(ref fractureB);
            ReleaseTexture(ref neutralDisturbanceTexture);
            currentFracture = null;
            writeFracture = null;
            previousState = null;
            currentState = null;
            writeState = null;

            if (boundaryTexture != null)
            {
                DestroyUnityObject(boundaryTexture);
                boundaryTexture = null;
            }

            metricBuffer?.Release();
            metricBuffer = null;
            populationMetricsBuffer?.Release();
            populationMetricsBuffer = null;
            metricRows = Array.Empty<FoamMetricRow>();
            computeShader = null;
            clearKernel = -1;
            injectKernel = -1;
            buildGuidanceKernel = -1;
            resetPopulationKernel = -1;
            measurePopulationKernel = -1;
            updateFractureKernel = -1;
            clearFractureKernel = -1;
            advectForwardKernel = -1;
            advectReverseKernel = -1;
            simulateKernel = -1;
            applyBoundaryKernel = -1;
            fieldWidth = 0;
            fieldHeight = 0;
            guidanceWidth = 0;
            guidanceHeight = 0;
            fractureWidth = 0;
            fractureHeight = 0;
            chunkCount = 0;
            resolutionPerChunk = 0;
            fieldLength = 0f;
            chunkActive = Array.Empty<bool>();
            chunkActiveUntil = Array.Empty<double>();
            resourcesDirty = true;
            boundaryDirty = true;
            domainVersion = -1;
            autonomousDecayUntil = 0.0;
            autonomousPopulationWasEnabled = false;
            guidanceAccumulator = 0f;
            populationAccumulator = 0f;
            fractureAccumulator = 0f;
        }

        private void HandleDomainChanged(RiverDomainSnapshot _)
        {
            resourcesDirty = true;
            boundaryDirty = true;
        }

        private void HandleGeneratedSourceChanged(IGeneratedGeometrySource _)
        {
            boundaryDirty = true;
        }

        private static bool IsUsableStaticSource(IGeneratedGeometrySource source)
        {
            if (source == null || !source.IsSolidGeometry || !source.IsStaticGeometry)
            {
                return false;
            }

            if (source is UnityEngine.Object unityObject && unityObject == null)
            {
                return false;
            }

            MeshFilter meshFilter = source.GeometryMeshFilter;
            return meshFilter != null && meshFilter.sharedMesh != null;
        }

        private float ResolveGuidanceUpdateRate()
        {
            if (river == null)
            {
                return 6f;
            }

            return river.Quality switch
            {
                StylizedRiverQuality.Low => 4f,
                StylizedRiverQuality.Medium => 6f,
                StylizedRiverQuality.High => 8f,
                _ => 6f
            };
        }

        private float ResolvePopulationUpdateRate()
        {
            if (river == null)
            {
                return 6f;
            }

            return river.Quality switch
            {
                StylizedRiverQuality.Low => 4f,
                StylizedRiverQuality.Medium => 6f,
                StylizedRiverQuality.High => 8f,
                _ => 6f
            };
        }

        private float ResolveFractureUpdateRate()
        {
            if (river == null)
            {
                return 10f;
            }

            return river.Quality switch
            {
                StylizedRiverQuality.Low => 8f,
                StylizedRiverQuality.Medium => 10f,
                StylizedRiverQuality.High => 12f,
                _ => 10f
            };
        }

        private float ResolveUpdateRate()
        {
            if (river == null)
            {
                return 20f;
            }

            return river.Quality switch
            {
                StylizedRiverQuality.Low => 12f,
                StylizedRiverQuality.Medium => 20f,
                StylizedRiverQuality.High => 30f,
                _ => 20f
            };
        }

        private int GlobalDistanceToX(float globalDistance)
        {
            float normalized =
                (globalDistance - river.Domain.GlobalDistanceMinimum) /
                Mathf.Max(0.001f, fieldLength);
            return Mathf.Clamp(
                Mathf.RoundToInt(normalized * Mathf.Max(1, fieldWidth - 1)),
                0,
                fieldWidth - 1);
        }

        private float XToGlobalDistance(int x)
        {
            return river.Domain.GlobalDistanceMinimum +
                   x / (float)Mathf.Max(1, fieldWidth - 1) * fieldLength;
        }

        private int GlobalDistanceToChunk(float globalDistance)
        {
            float local = globalDistance - river.Domain.GlobalDistanceMinimum;
            return Mathf.Clamp(
                Mathf.FloorToInt(local / ChunkLengthMetres),
                0,
                Mathf.Max(0, chunkCount - 1));
        }

        private int CountActiveChunks()
        {
            int count = 0;
            for (int index = 0; index < chunkActive.Length; index++)
            {
                if (chunkActive[index])
                {
                    count++;
                }
            }

            return count;
        }

        private void ResetLastUpdateDiagnostics()
        {
            lastUpdateDispatches = 0;
            lastUpdateCellIterations = 0;
            injectedLastUpdate = 0;
        }

        private void UpdateRecentPeaks()
        {
            double now = Time.realtimeSinceStartupAsDouble;
            if (now > recentPeakWindowEnd)
            {
                recentPeakDispatches = lastUpdateDispatches;
                recentPeakCellIterations = lastUpdateCellIterations;
                recentPeakWindowEnd = now + 5.0;
                return;
            }

            recentPeakDispatches = Mathf.Max(
                recentPeakDispatches,
                lastUpdateDispatches);
            recentPeakCellIterations = Math.Max(
                recentPeakCellIterations,
                lastUpdateCellIterations);
        }

        private static bool IsCreatedTexture(RenderTexture texture)
        {
            return texture != null && texture.IsCreated();
        }

        private static long EstimateTextureBytes(Texture texture)
        {
            if (texture == null)
            {
                return 0L;
            }

            long bytesPerPixel = 4L;
            if (texture is RenderTexture renderTexture)
            {
                bytesPerPixel = renderTexture.format switch
                {
                    RenderTextureFormat.ARGBHalf => 8L,
                    RenderTextureFormat.RGHalf => 4L,
                    _ => 4L
                };
            }
            else if (texture is Texture2D texture2D)
            {
                bytesPerPixel = texture2D.format switch
                {
                    TextureFormat.RGBAHalf => 8L,
                    TextureFormat.RGHalf => 4L,
                    _ => 4L
                };
            }

            return (long)texture.width * texture.height * bytesPerPixel;
        }

        private static void ReleaseTexture(ref RenderTexture texture)
        {
            if (texture == null)
            {
                return;
            }

            texture.Release();
            DestroyUnityObject(texture);
            texture = null;
        }

        private static void DestroyUnityObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private static float Across01ToMetres(
            float across01,
            float leftHalfWidth,
            float rightHalfWidth)
        {
            if (across01 <= 0.5f)
            {
                return -leftHalfWidth * (1f - across01 * 2f);
            }

            return rightHalfWidth * (across01 * 2f - 1f);
        }

        private int AcrossMetresToY(
            float acrossMetres,
            float leftHalfWidth,
            float rightHalfWidth)
        {
            float across01;
            if (acrossMetres <= 0f)
            {
                across01 = 0.5f *
                    (1f + acrossMetres / Mathf.Max(0.001f, leftHalfWidth));
            }
            else
            {
                across01 = 0.5f + 0.5f *
                    acrossMetres / Mathf.Max(0.001f, rightHalfWidth);
            }

            return Mathf.RoundToInt(
                Mathf.Clamp01(across01) * Mathf.Max(1, fieldHeight - 1));
        }

        private static bool PointInPolygon(Vector2 point, Vector2[] polygon)
        {
            bool inside = false;
            int previous = polygon.Length - 1;

            for (int current = 0; current < polygon.Length; current++)
            {
                Vector2 a = polygon[current];
                Vector2 b = polygon[previous];
                float denominator = b.y - a.y;
                bool crosses =
                    (a.y > point.y) != (b.y > point.y) &&
                    Mathf.Abs(denominator) > 0.000001f &&
                    point.x <
                    (b.x - a.x) * (point.y - a.y) /
                    denominator + a.x;
                if (crosses)
                {
                    inside = !inside;
                }

                previous = current;
            }

            return inside;
        }

        private static float DistanceToPolygon(Vector2 point, Vector2[] polygon)
        {
            float minimum = float.PositiveInfinity;
            int previous = polygon.Length - 1;

            for (int current = 0; current < polygon.Length; current++)
            {
                minimum = Mathf.Min(
                    minimum,
                    DistanceToSegment(point, polygon[previous], polygon[current]));
                previous = current;
            }

            return minimum;
        }

        private static float DistanceToSegment(
            Vector2 point,
            Vector2 a,
            Vector2 b)
        {
            Vector2 ab = b - a;
            float denominator = Vector2.Dot(ab, ab);
            float t = denominator > 0.000001f
                ? Mathf.Clamp01(Vector2.Dot(point - a, ab) / denominator)
                : 0f;
            return Vector2.Distance(point, a + ab * t);
        }
    }
}
