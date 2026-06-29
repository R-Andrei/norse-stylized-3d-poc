using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ProgrammaticStylized3D.Geometry;
using UnityEngine;
using UnityEngine.Rendering;

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
        private const int TopologyMetricCount = 16;
        private const float TopologyMetricQuantisation = 1023f;
        // Canonical Stage 6 shore-capture band measured inward from the
        // instantaneous Stage 3 visible water edge. These are deliberately
        // fixed while the capture contract is being validated.
        private const float ShoreCaptureCoreWidthMetres = 0.24f;
        private const float ShoreCaptureFadeWidthMetres = 0.03f;

        private static readonly int FoamEnabledId =
            Shader.PropertyToID("_FoamEnabled");
        private static readonly int FoamPreviousId =
            Shader.PropertyToID("_FoamPrevious");
        private static readonly int FoamCurrentId =
            Shader.PropertyToID("_FoamCurrent");
        private static readonly int FoamGuidanceId =
            Shader.PropertyToID("_FoamGuidance");
        private static readonly int FoamTopologyId =
            Shader.PropertyToID("_FoamTopology");
        private static readonly int FoamTopologySourcesId =
            Shader.PropertyToID("_FoamTopologySources");
        private static readonly int FoamFractureId =
            Shader.PropertyToID("_FoamFracture");
        private static readonly int FoamBoundaryId =
            Shader.PropertyToID("_FoamBoundary");
        private static readonly int FoamObstacleExclusionId =
            Shader.PropertyToID("_FoamObstacleExclusion");
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
            public Vector4 TopologyData;
            public Vector4 ShoreData;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FoamObstacleIntervalCell
        {
            // x/y = full-resolution Foam texel coordinate,
            // z = first of nine consecutive exact-mesh interval samples,
            // w = reserved.
            public Vector4 CoordinateAndOffset;
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
        private RenderTexture topologyTexture;
        private RenderTexture topologySourcesTexture;
        private RenderTexture topologyMajorTexture;
        private RenderTexture topologyPocketTexture;
        private RenderTexture topologyConnectorTexture;
        private RenderTexture currentShoreEdgesTexture;
        private RenderTexture obstacleExclusionTexture;
        private RenderTexture fractureA;
        private RenderTexture fractureB;
        private RenderTexture currentFracture;
        private RenderTexture writeFracture;
        private RenderTexture neutralDisturbanceTexture;
        private Texture2D boundaryTexture;
        private ComputeBuffer metricBuffer;
        private ComputeBuffer obstacleSampleBuffer;
        private ComputeBuffer obstacleIntervalCellBuffer;
        private ComputeBuffer populationMetricsBuffer;
        private ComputeBuffer topologyMetricsBuffer;
        private StylizedRiverDisturbanceRuntime disturbanceRuntime;
        private FoamMetricRow[] metricRows = Array.Empty<FoamMetricRow>();
        private readonly uint[] latestTopologyMetrics =
            new uint[TopologyMetricCount];
        private bool topologyMetricsReadbackPending;
        private bool topologyMetricsAvailable;
        private int topologyMetricsGeneration;

        private readonly List<PendingInjection> pendingInjections = new();
        private readonly List<FoamReservation> reservations = new();
        private readonly List<MeshFilter> obstacleExclusionMeshes = new();
        private readonly List<RiverObstacleExclusionCell>
            obstacleBakedCells = new();
        private readonly List<RiverObstacleExclusionSample> obstacleSamples = new();
        private readonly List<FoamObstacleIntervalCell> obstacleIntervalCells =
            new();

        private bool[] chunkActive = Array.Empty<bool>();
        private double[] chunkActiveUntil = Array.Empty<double>();
        private bool resourcesDirty = true;
        private bool boundaryDirty = true;
        private bool supportWarningReported;
        private bool allocationWarningReported;
        private bool obstacleBakeWarningReported;
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
        private float validFieldLength;
        private float simulationFieldLength;
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
        private int buildCurrentShoreEdgesKernel = -1;
        private int buildTopologyMajorKernel = -1;
        private int buildTopologyPocketsKernel = -1;
        private int buildTopologyConnectorsKernel = -1;
        private int composeTopologyKernel = -1;
        private int clearObstacleExclusionKernel = -1;
        private int updateObstacleExclusionKernel = -1;
        private int resetTopologyMetricsKernel = -1;
        private int measureTopologyMetricsKernel = -1;
        private int resetPopulationKernel = -1;
        private int measurePopulationKernel = -1;
        private int updateFractureKernel = -1;
        private int clearFractureKernel = -1;
        private int advectForwardKernel = -1;
        private int advectReverseKernel = -1;
        private int simulateKernel = -1;
        private int applyBoundaryKernel = -1;
        private int obstacleIntervalCellCount;
        private int obstacleGeometryVersion = -1;
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
        public int TopologyWidth => topologyTexture != null ? topologyTexture.width : 0;
        public int TopologyHeight => topologyTexture != null ? topologyTexture.height : 0;
        public int DynamicShoreRowCount => currentShoreEdgesTexture != null
            ? currentShoreEdgesTexture.width
            : 0;
        public bool TopologyMetricsAvailable => topologyMetricsAvailable;
        public float MajorCapacityCoverage => TopologyCoverageRatio(1);
        public float ConnectorCapacityCoverage => TopologyCoverageRatio(2);
        public float ProtectedPocketCoverage => TopologyCoverageRatio(3);
        public float ProtectedPocketViolation => TopologyRegionRatio(4, 3);
        public float VisibleMaterialCoverage => TopologyCoverageRatio(5);
        public float MaterialDeficitInsideCapacity =>
            topologyMetricsAvailable && latestTopologyMetrics[6] > 0u
                ? latestTopologyMetrics[7] /
                  (TopologyMetricQuantisation * latestTopologyMetrics[6])
                : 0f;
        public float ShoreOccupancy => TopologyRegionRatio(9, 8);
        public float PressureLeeOccupancy => TopologyRegionRatio(13, 12);
        public float PerimeterRatio => TopologyRegionRatio(14, 5);
        public float ComposedTopologyCoverage => TopologyCoverageRatio(6);
        public float OpenSpanCoverage => topologyMetricsAvailable
            ? Mathf.Clamp01(1f - ComposedTopologyCoverage)
            : 0f;
        public float ConnectorInMajorOverlap => TopologyRegionRatio(15, 2);
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
            !IsTopologyDebugActive &&
            pendingInjections.Count == 0 &&
            reservations.Count == 0 &&
            CountActiveChunks() == 0;
        public long EstimatedMemoryBytes =>
            EstimateTextureBytes(stateA) +
            EstimateTextureBytes(stateB) +
            EstimateTextureBytes(advectedState) +
            EstimateTextureBytes(reverseState) +
            EstimateTextureBytes(guidanceTexture) +
            EstimateTextureBytes(topologyTexture) +
            EstimateTextureBytes(topologySourcesTexture) +
            EstimateTextureBytes(topologyMajorTexture) +
            EstimateTextureBytes(topologyPocketTexture) +
            EstimateTextureBytes(topologyConnectorTexture) +
            EstimateTextureBytes(currentShoreEdgesTexture) +
            EstimateTextureBytes(obstacleExclusionTexture) +
            EstimateTextureBytes(fractureA) +
            EstimateTextureBytes(fractureB) +
            EstimateTextureBytes(neutralDisturbanceTexture) +
            EstimateTextureBytes(boundaryTexture) +
            (metricBuffer != null
                ? (long)metricBuffer.count * metricBuffer.stride
                : 0L) +
            (obstacleSampleBuffer != null
                ? (long)obstacleSampleBuffer.count *
                  obstacleSampleBuffer.stride
                : 0L) +
            (obstacleIntervalCellBuffer != null
                ? (long)obstacleIntervalCellBuffer.count *
                  obstacleIntervalCellBuffer.stride
                : 0L) +
            (populationMetricsBuffer != null
                ? (long)populationMetricsBuffer.count * populationMetricsBuffer.stride
                : 0L) +
            (topologyMetricsBuffer != null
                ? (long)topologyMetricsBuffer.count * topologyMetricsBuffer.stride
                : 0L);

        private bool IsAutonomousPopulationActive =>
            river != null && river.FoamAmount > 0.0001f;

        private bool IsTopologyDebugActive
        {
            get
            {
                if (river == null)
                {
                    return false;
                }

                StylizedRiverFoamDebugView view = river.FoamDebugView;
                return view == StylizedRiverFoamDebugView.CaptureZones ||
                    view == StylizedRiverFoamDebugView.PositiveNegativeZones ||
                    view == StylizedRiverFoamDebugView.PositiveZoneClasses ||
                    view == StylizedRiverFoamDebugView.NegativeZoneClasses;
            }
        }

        private bool IsSupported =>
            SystemInfo.supportsComputeShaders &&
            SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf) &&
            SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGHalf) &&
            SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RHalf) &&
            SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf);

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

            if (river == null ||
                !river.isActiveAndEnabled ||
                !river.FoamEnabled)
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
                        $"Stage 6 Foam on '{name}' is disabled because compute shaders or ARGBHalf/RGHalf/RHalf random-write textures are unavailable.",
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
            bool topologyDebugActive = IsTopologyDebugActive;
            bool materialWork =
                autonomousPopulationActive ||
                autonomousDecayActive ||
                pendingInjections.Count > 0 ||
                reservations.Count > 0 ||
                CountActiveChunks() > 0;
            bool hasWork = materialWork || topologyDebugActive;

            if (!hasWork && currentState == null)
            {
                BindDisabled();
                return;
            }

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();

            if (!EnsureResources())
            {
                BindDisabled();
                return;
            }

            int currentObstacleGeometryVersion =
                disturbanceRuntime != null
                    ? disturbanceRuntime.ObstacleGeometryVersion
                    : -1;
            if (currentObstacleGeometryVersion != obstacleGeometryVersion)
            {
                RebuildObstacleExclusionCache();
                if (topologyDebugActive)
                {
                    BuildTopologyField(0f);
                }
            }

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
                    bool materialStepActive =
                        autonomousPopulationActive ||
                        autonomousDecayActive ||
                        reservations.Count > 0 ||
                        CountActiveChunks() > 0;

                    if (materialStepActive)
                    {
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
                        populationAccumulator += stepDuration;
                        fractureAccumulator += stepDuration;
                    }

                    guidanceAccumulator += stepDuration;
                    float guidanceInterval = 1f /
                        Mathf.Max(1f, ResolveGuidanceUpdateRate());
                    bool topologyStructuresRebuilt = false;
                    if (guidanceAccumulator >= guidanceInterval)
                    {
                        if (materialStepActive)
                        {
                            BuildGuidanceField(guidanceAccumulator);
                        }

                        if (topologyDebugActive)
                        {
                            BuildTopologyField(guidanceAccumulator);
                            topologyStructuresRebuilt = true;
                        }

                        guidanceAccumulator %= guidanceInterval;
                    }

                    // The expensive free-water topology remains low-rate, but
                    // the current Stage 3 shoreline edge and its capture band
                    // refresh at the normal Foam update cadence so the band
                    // follows visible shore-wave motion without stepping at the
                    // 4/6/8 Hz topology cadence.
                    if (topologyDebugActive && !topologyStructuresRebuilt)
                    {
                        RefreshDynamicTopologySources(false);
                    }

                    if (materialStepActive)
                    {
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
                topologyTexture != null &&
                topologySourcesTexture != null &&
                topologyMajorTexture != null &&
                topologyPocketTexture != null &&
                topologyConnectorTexture != null &&
                currentShoreEdgesTexture != null &&
                obstacleExclusionTexture != null &&
                obstacleExclusionTexture.IsCreated() &&
                currentFracture != null &&
                writeFracture != null &&
                neutralDisturbanceTexture != null &&
                neutralDisturbanceTexture.IsCreated() &&
                boundaryTexture != null &&
                metricBuffer != null &&
                populationMetricsBuffer != null &&
                topologyMetricsBuffer != null &&
                domainVersion == river.Domain.Version &&
                allocatedQuality == river.Quality)
            {
                if (boundaryDirty)
                {
                    RebuildBoundaryTexture();
                    BuildTopologyField(0f);
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
            buildCurrentShoreEdgesKernel =
                computeShader.FindKernel("BuildCurrentShoreEdges");
            buildTopologyMajorKernel =
                computeShader.FindKernel("BuildTopologyMajor");
            buildTopologyPocketsKernel =
                computeShader.FindKernel("BuildTopologyPockets");
            buildTopologyConnectorsKernel =
                computeShader.FindKernel("BuildTopologyConnectors");
            composeTopologyKernel =
                computeShader.FindKernel("ComposeTopology");
            clearObstacleExclusionKernel =
                computeShader.FindKernel("ClearObstacleExclusion");
            updateObstacleExclusionKernel =
                computeShader.FindKernel("UpdateObstacleExclusion");
            resetTopologyMetricsKernel =
                computeShader.FindKernel("ResetTopologyMetrics");
            measureTopologyMetricsKernel =
                computeShader.FindKernel("MeasureTopologyMetrics");
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

            int maximumTextureSize = SystemInfo.maxTextureSize;
            if (!TryResolveChunkedTextureWidth(
                    chunkCount,
                    resolutionPerChunk,
                    16,
                    maximumTextureSize,
                    out resolutionPerChunk,
                    out fieldWidth))
            {
                ReportAllocationFailure(maximumTextureSize);
                return false;
            }

            fieldHeight = river.Quality switch
            {
                StylizedRiverQuality.Low => 32,
                StylizedRiverQuality.Medium => 48,
                StylizedRiverQuality.High => 64,
                _ => 48
            };
            int guidanceResolutionPerChunk = river.Quality switch
            {
                StylizedRiverQuality.Low => 12,
                StylizedRiverQuality.Medium => 18,
                StylizedRiverQuality.High => 24,
                _ => 18
            };
            long desiredGuidanceWidth =
                (long)chunkCount * guidanceResolutionPerChunk;
            guidanceWidth = (int)Math.Min(
                maximumTextureSize,
                Math.Max(24L, desiredGuidanceWidth));
            guidanceHeight = river.Quality switch
            {
                StylizedRiverQuality.Low => 32,
                StylizedRiverQuality.Medium => 44,
                StylizedRiverQuality.High => 56,
                _ => 44
            };
            fractureWidth = Mathf.Max(16, Mathf.CeilToInt(fieldWidth * 0.5f));
            fractureHeight = Mathf.Max(16, Mathf.CeilToInt(fieldHeight * 0.5f));

            if (maximumTextureSize < 24 ||
                fieldHeight > maximumTextureSize ||
                guidanceHeight > maximumTextureSize ||
                fractureWidth > maximumTextureSize ||
                fractureHeight > maximumTextureSize)
            {
                ReportAllocationFailure(maximumTextureSize);
                return false;
            }

            fieldLength = chunkCount * ChunkLengthMetres;
            validFieldLength = domain.LocalLength;
            float longitudinalSpacing =
                fieldLength / Mathf.Max(1, fieldWidth - 1);
            // Keep one inert outflow sample beyond the exact endpoint so
            // bilinear rendering reaches the visible river end without
            // reintroducing a padded population domain.
            simulationFieldLength = Mathf.Min(
                fieldLength,
                validFieldLength + longitudinalSpacing);
            allocationWarningReported = false;
            domainVersion = domain.Version;
            allocatedQuality = river.Quality;

            stateA = CreateFieldTexture("PS3D_RiverFoam_A");
            stateB = CreateFieldTexture("PS3D_RiverFoam_B");
            advectedState = CreateFieldTexture("PS3D_RiverFoam_Advected");
            reverseState = CreateFieldTexture("PS3D_RiverFoam_Reverse");
            guidanceTexture = CreateGuidanceTexture(
                "PS3D_RiverFoam_Guidance");
            topologyTexture = CreateGuidanceTexture(
                "PS3D_RiverFoam_Topology");
            topologySourcesTexture = CreateGuidanceTexture(
                "PS3D_RiverFoam_TopologySources");
            topologyMajorTexture = CreateGuidanceTexture(
                "PS3D_RiverFoam_TopologyMajorWork");
            topologyPocketTexture = CreateGuidanceTexture(
                "PS3D_RiverFoam_TopologyPocketWork");
            topologyConnectorTexture = CreateGuidanceTexture(
                "PS3D_RiverFoam_TopologyConnectorWork");
            currentShoreEdgesTexture = CreateShoreEdgesTexture(
                "PS3D_RiverFoam_CurrentShoreEdges");
            obstacleExclusionTexture = CreateObstacleExclusionTexture(
                "PS3D_RiverFoam_ObstacleExclusion");
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
            topologyMetricsBuffer = new ComputeBuffer(
                TopologyMetricCount,
                sizeof(uint),
                ComputeBufferType.Raw);

            chunkActive = new bool[chunkCount];
            chunkActiveUntil = new double[chunkCount];

            BuildMetricBuffer();
            RebuildBoundaryTexture(false);
            RebuildObstacleExclusionCache();

            DispatchClear(stateA, 0, fieldWidth);
            DispatchClear(stateB, 0, fieldWidth);
            DispatchClear(advectedState, 0, fieldWidth);
            DispatchClear(reverseState, 0, fieldWidth);
            DispatchClearFracture(fractureA, 0, fractureWidth);
            DispatchClearFracture(fractureB, 0, fractureWidth);
            BuildGuidanceField(0f);
            BuildTopologyField(0f);
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

        private static bool TryResolveChunkedTextureWidth(
            int chunks,
            int desiredResolutionPerChunk,
            int minimumResolutionPerChunk,
            int maximumTextureSize,
            out int resolvedResolutionPerChunk,
            out int resolvedWidth)
        {
            resolvedResolutionPerChunk = 0;
            resolvedWidth = 0;
            if (chunks < 1 ||
                maximumTextureSize < minimumResolutionPerChunk ||
                (long)chunks * minimumResolutionPerChunk > maximumTextureSize)
            {
                return false;
            }

            resolvedResolutionPerChunk = Math.Min(
                desiredResolutionPerChunk,
                maximumTextureSize / chunks);
            if (resolvedResolutionPerChunk < minimumResolutionPerChunk)
            {
                return false;
            }

            long width = (long)resolvedResolutionPerChunk * chunks;
            if (width < 1 || width > maximumTextureSize)
            {
                return false;
            }

            resolvedWidth = (int)width;
            return true;
        }

        private void ReportAllocationFailure(int maximumTextureSize)
        {
            if (allocationWarningReported)
            {
                return;
            }

            Debug.LogWarning(
                $"Stage 6 Foam on '{name}' is disabled because the required " +
                $"textures for {chunkCount} chunks cannot fit within the " +
                $"hardware texture limit of {maximumTextureSize} pixels. " +
                "The field requires at least 16 columns per chunk.",
                this);
            allocationWarningReported = true;
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

        private RenderTexture CreateShoreEdgesTexture(string textureName)
        {
            RenderTexture texture = new RenderTexture(
                guidanceWidth,
                1,
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

        private RenderTexture CreateObstacleExclusionTexture(
            string textureName)
        {
            RenderTexture texture = new RenderTexture(
                fieldWidth,
                fieldHeight,
                0,
                RenderTextureFormat.RHalf,
                RenderTextureReadWrite.Linear)
            {
                name = textureName,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Point,
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
            float curvatureSampleDistance = Mathf.Max(
                0.5f,
                longitudinalSpacing * 2f);
            float flowSign = river.Domain.ReverseFlow ? -1f : 1f;

            for (int x = 0; x < fieldWidth; x++)
            {
                float localDistance =
                    x / (float)Mathf.Max(1, fieldWidth - 1) * fieldLength;
                float clampedLocalDistance = Mathf.Min(
                    localDistance,
                    validFieldLength);
                float globalDistance =
                    river.Domain.GlobalDistanceMinimum + clampedLocalDistance;
                StylizedRiverSplineSample sample =
                    river.Domain.SampleAtGlobalDistance(globalDistance);
                float left = Mathf.Max(0.05f, sample.LeftSurfaceHalfWidth);
                float right = Mathf.Max(0.05f, sample.RightSurfaceHalfWidth);
                float minimumLateralSpacing =
                    (left + right) / Mathf.Max(1, fieldHeight - 1);

                float previousGlobal = Mathf.Max(
                    river.Domain.GlobalDistanceMinimum,
                    globalDistance - curvatureSampleDistance);
                float nextGlobal = Mathf.Min(
                    river.Domain.GlobalDistanceMaximum,
                    globalDistance + curvatureSampleDistance);
                StylizedRiverSplineSample previousSample =
                    river.Domain.SampleAtGlobalDistance(previousGlobal);
                StylizedRiverSplineSample nextSample =
                    river.Domain.SampleAtGlobalDistance(nextGlobal);
                Vector3 previousTangent =
                    previousSample.Tangent * flowSign;
                Vector3 nextTangent =
                    nextSample.Tangent * flowSign;
                // Topology lateral coordinates retain the domain's stored
                // Side basis under reverse flow, so curvature sign must be
                // expressed in that same basis rather than a flipped flow-side.
                Vector3 topologySide = sample.Side;
                float curvatureDistance = Mathf.Max(
                    0.01f,
                    nextGlobal - previousGlobal);
                float signedCurvature = Vector3.Dot(
                    (nextTangent - previousTangent) / curvatureDistance,
                    topologySide);
                float previousWidth =
                    previousSample.LeftSurfaceHalfWidth +
                    previousSample.RightSurfaceHalfWidth;
                float nextWidth =
                    nextSample.LeftSurfaceHalfWidth +
                    nextSample.RightSurfaceHalfWidth;
                float widthDerivative =
                    (nextWidth - previousWidth) / curvatureDistance;
                float asymmetry =
                    (right - left) / Mathf.Max(0.05f, left + right);

                metricRows[x] = new FoamMetricRow
                {
                    WidthsAndSpacing = new Vector4(
                        left,
                        right,
                        longitudinalSpacing,
                        minimumLateralSpacing),
                    TopologyData = new Vector4(
                        signedCurvature,
                        widthDerivative,
                        asymmetry,
                        localDistance <= validFieldLength + 0.0001f
                            ? 1f
                            : 0f),
                    ShoreData = new Vector4(
                        Mathf.Max(0.01f, sample.LeftHalfWidth),
                        Mathf.Max(0.01f, sample.RightHalfWidth),
                        sample.SurfaceHeight,
                        0f)
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
                float localDistance =
                    x / (float)Mathf.Max(1, fieldWidth - 1) * fieldLength;
                if (localDistance > simulationFieldLength + 0.0001f)
                {
                    continue;
                }

                float globalDistance =
                    river.Domain.GlobalDistanceMinimum +
                    Mathf.Min(localDistance, validFieldLength);
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
                    // R/G remain the legacy material-simulation fluid and
                    // attraction contract. B/A are reserved zero: registered
                    // solids now use the one-time exact-mesh interval bake
                    // and full-resolution Obstacle Exclusion mask. Canonical
                    // Shore Capture comes from the instantaneous Stage 3 edge.
                    pixels[y * fieldWidth + x] = new Color(
                        coverage,
                        attraction,
                        0f,
                        0f);
                }
            }

            // Registered solid geometry is no longer rasterised into this
            // static boundary texture. Obstacle Exclusion is reconstructed
            // from cached exact transformed-mesh solid intervals and the current
            // Stage 3 water height in a dedicated point-sampled mask.

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
                    TextureFormat.RGBAHalf,
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

        private void RebuildObstacleExclusionCache()
        {
            ReleaseObstacleExclusionBuffers();
            obstacleExclusionMeshes.Clear();
            obstacleBakedCells.Clear();
            obstacleSamples.Clear();
            obstacleIntervalCells.Clear();
            obstacleIntervalCellCount = 0;

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            obstacleGeometryVersion = disturbanceRuntime != null
                ? disturbanceRuntime.ObstacleGeometryVersion
                : -1;

            if (disturbanceRuntime == null ||
                fieldWidth < 1 || fieldHeight < 1 ||
                fieldLength <= 0.0001f ||
                river == null || !river.Domain.IsValid)
            {
                ClearObstacleExclusionMask();
                return;
            }

            disturbanceRuntime.CopyObstacleExclusionMeshFiltersTo(
                obstacleExclusionMeshes);
            if (obstacleExclusionMeshes.Count == 0)
            {
                obstacleBakeWarningReported = false;
                ClearObstacleExclusionMask();
                return;
            }

            int successfulMeshCount = 0;
            for (int meshIndex = 0;
                 meshIndex < obstacleExclusionMeshes.Count;
                 meshIndex++)
            {
                MeshFilter meshFilter = obstacleExclusionMeshes[meshIndex];
                if (RiverObstacleExclusionResolver.TryBake(
                        river,
                        meshFilter,
                        fieldWidth,
                        fieldHeight,
                        fieldLength,
                        obstacleBakedCells,
                        obstacleSamples,
                        out string bakeStatus))
                {
                    successfulMeshCount++;
                    continue;
                }

                if (!obstacleBakeWarningReported)
                {
                    Debug.LogWarning(
                        $"{name}: exact Foam Obstacle Exclusion could not be " +
                        $"baked for '{meshFilter.name}'. {bakeStatus} " +
                        "No bounds, hull, or rectangle fallback was created.",
                        this);
                    obstacleBakeWarningReported = true;
                }
            }

            if (successfulMeshCount > 0)
            {
                obstacleBakeWarningReported = false;
            }

            for (int index = 0; index < obstacleBakedCells.Count; index++)
            {
                RiverObstacleExclusionCell cell = obstacleBakedCells[index];
                obstacleIntervalCells.Add(
                    new FoamObstacleIntervalCell
                    {
                        CoordinateAndOffset = new Vector4(
                            cell.Coordinate.x,
                            cell.Coordinate.y,
                            cell.IntervalOffset,
                            0f)
                    });
            }

            obstacleIntervalCellCount = obstacleIntervalCells.Count;
            if (obstacleIntervalCellCount <= 0 ||
                obstacleSamples.Count <
                    obstacleIntervalCellCount *
                    RiverObstacleExclusionResolver.SamplesPerCell)
            {
                ClearObstacleExclusionMask();
                return;
            }

            obstacleSampleBuffer = new ComputeBuffer(
                obstacleSamples.Count,
                Marshal.SizeOf<RiverObstacleExclusionSample>(),
                ComputeBufferType.Structured);
            obstacleSampleBuffer.SetData(obstacleSamples.ToArray());

            obstacleIntervalCellBuffer = new ComputeBuffer(
                obstacleIntervalCellCount,
                Marshal.SizeOf<FoamObstacleIntervalCell>(),
                ComputeBufferType.Structured);
            obstacleIntervalCellBuffer.SetData(
                obstacleIntervalCells.ToArray());

            ClearObstacleExclusionMask();
        }

        private void ReleaseObstacleExclusionBuffers()
        {
            obstacleSampleBuffer?.Release();
            obstacleSampleBuffer = null;
            obstacleIntervalCellBuffer?.Release();
            obstacleIntervalCellBuffer = null;
        }

        private void ClearObstacleExclusionMask()
        {
            if (computeShader == null ||
                obstacleExclusionTexture == null ||
                clearObstacleExclusionKernel < 0)
            {
                return;
            }

            computeShader.SetInts(
                "_FoamDimensions",
                fieldWidth,
                fieldHeight);
            computeShader.SetTexture(
                clearObstacleExclusionKernel,
                "_FoamObstacleExclusionWrite",
                obstacleExclusionTexture);
            Dispatch(
                clearObstacleExclusionKernel,
                fieldWidth,
                fieldHeight);
        }

        private void UpdateObstacleExclusionMask()
        {
            if (computeShader == null ||
                obstacleExclusionTexture == null ||
                updateObstacleExclusionKernel < 0)
            {
                return;
            }

            // Multiple static meshes may cover the same texel. Clear first,
            // then each thread writes only accepted solid cells; no source ever
            // writes zero over a cell accepted by another source.
            ClearObstacleExclusionMask();

            if (obstacleSampleBuffer == null ||
                obstacleIntervalCellBuffer == null ||
                obstacleIntervalCellCount <= 0)
            {
                return;
            }

            computeShader.SetInt(
                "_FoamObstacleCellCount",
                obstacleIntervalCellCount);
            computeShader.SetBuffer(
                updateObstacleExclusionKernel,
                "_FoamObstacleSamples",
                obstacleSampleBuffer);
            computeShader.SetBuffer(
                updateObstacleExclusionKernel,
                "_FoamObstacleCells",
                obstacleIntervalCellBuffer);
            computeShader.SetTexture(
                updateObstacleExclusionKernel,
                "_FoamObstacleExclusionWrite",
                obstacleExclusionTexture);
            DispatchOneDimensional(
                updateObstacleExclusionKernel,
                obstacleIntervalCellCount,
                64);
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
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
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
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
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

        private void BuildTopologyField(float deltaTime)
        {
            if (computeShader == null || topologyTexture == null ||
                topologySourcesTexture == null ||
                topologyMajorTexture == null ||
                topologyPocketTexture == null ||
                topologyConnectorTexture == null ||
                currentShoreEdgesTexture == null ||
                obstacleExclusionTexture == null ||
                boundaryTexture == null || metricBuffer == null ||
                buildCurrentShoreEdgesKernel < 0 ||
                buildTopologyMajorKernel < 0 ||
                buildTopologyPocketsKernel < 0 ||
                buildTopologyConnectorsKernel < 0 ||
                composeTopologyKernel < 0)
            {
                return;
            }

            ConfigureTopologyParameters(deltaTime);

            computeShader.SetBuffer(
                buildTopologyMajorKernel,
                "_FoamMetricRows",
                metricBuffer);
            computeShader.SetTexture(
                buildTopologyMajorKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetTexture(
                buildTopologyMajorKernel,
                "_FoamTopologyMajorWrite",
                topologyMajorTexture);
            Dispatch(
                buildTopologyMajorKernel,
                guidanceWidth,
                guidanceHeight);

            computeShader.SetBuffer(
                buildTopologyPocketsKernel,
                "_FoamMetricRows",
                metricBuffer);
            computeShader.SetTexture(
                buildTopologyPocketsKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetTexture(
                buildTopologyPocketsKernel,
                "_FoamTopologyMajorRead",
                topologyMajorTexture);
            computeShader.SetTexture(
                buildTopologyPocketsKernel,
                "_FoamTopologyPocketWrite",
                topologyPocketTexture);
            Dispatch(
                buildTopologyPocketsKernel,
                guidanceWidth,
                guidanceHeight);

            computeShader.SetBuffer(
                buildTopologyConnectorsKernel,
                "_FoamMetricRows",
                metricBuffer);
            computeShader.SetTexture(
                buildTopologyConnectorsKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetTexture(
                buildTopologyConnectorsKernel,
                "_FoamTopologyMajorRead",
                topologyMajorTexture);
            computeShader.SetTexture(
                buildTopologyConnectorsKernel,
                "_FoamTopologyPocketRead",
                topologyPocketTexture);
            computeShader.SetTexture(
                buildTopologyConnectorsKernel,
                "_FoamTopologyConnectorWrite",
                topologyConnectorTexture);
            Dispatch(
                buildTopologyConnectorsKernel,
                guidanceWidth,
                guidanceHeight);

            RefreshDynamicTopologySources(true);
        }

        private void ConfigureTopologyParameters(float deltaTime)
        {
            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetInts(
                "_FoamTopologyDimensions",
                guidanceWidth,
                guidanceHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamGlobalStart",
                river.Domain.GlobalDistanceMinimum);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat("_FoamDeltaTime", deltaTime);
            computeShader.SetFloat("_FoamTime", river.MotionTime);
            computeShader.SetFloat("_FoamSeed", river.VisualSeed);
            computeShader.SetFloat(
                "_FoamWebGranularity",
                river.FoamWebGranularity);
            computeShader.SetFloat(
                "_FoamNetworkEvolution",
                river.FoamNetworkEvolution);
            computeShader.SetFloat(
                "_FoamMotionFlowSpeed",
                river.FlowSpeedMetresPerSecond);
            computeShader.SetFloat(
                "_FoamMotionWaveHeight",
                river.MotionWaveHeight);
            computeShader.SetFloat(
                "_FoamMotionWaveLength",
                river.MotionWaveLength);
            computeShader.SetFloat(
                "_FoamMotionWaveSteepness",
                river.MotionWaveSteepness);
            computeShader.SetFloat(
                "_FoamMotionTurbulence",
                river.MotionTurbulence);
            computeShader.SetFloat(
                "_FoamShoreMotion",
                river.ShoreMotion);
            computeShader.SetFloat(
                "_FoamShoreMotionWidth",
                river.ShoreMotionWidth);
            computeShader.SetFloat(
                "_FoamShoreWaveHeightScale",
                river.ShoreWaveHeightScale);
            computeShader.SetFloat(
                "_FoamShoreWaveLengthScale",
                river.ShoreWaveLengthScale);
            computeShader.SetFloat(
                "_FoamShoreWaveReach",
                river.ShoreWaveReach);
            computeShader.SetFloat(
                "_FoamShoreWaveTransitionLength",
                river.ShoreWaveTransitionLength);
            computeShader.SetFloat(
                "_FoamShoreWaveSizeVariation",
                river.ShoreWaveSizeVariation);
            computeShader.SetFloat(
                "_FoamShoreWaveSideAsymmetry",
                river.ShoreWaveSideAsymmetry);
            computeShader.SetFloat(
                "_FoamShoreWaveProfileVariation",
                river.ShoreWaveProfileVariation);
            computeShader.SetFloat(
                "_FoamShoreBankCover",
                river.ShorelineBankCover);
            computeShader.SetFloat(
                "_FoamFreezeAmount",
                river.FreezeAmount);
            computeShader.SetFloat(
                "_FoamShoreCaptureCoreWidth",
                ShoreCaptureCoreWidthMetres);
            computeShader.SetFloat(
                "_FoamShoreCaptureFadeWidth",
                ShoreCaptureFadeWidthMetres);
        }

        private void RefreshDynamicTopologySources(bool measureMetrics)
        {
            if (computeShader == null || topologyTexture == null ||
                topologySourcesTexture == null ||
                topologyMajorTexture == null ||
                topologyPocketTexture == null ||
                topologyConnectorTexture == null ||
                currentShoreEdgesTexture == null ||
                obstacleExclusionTexture == null ||
                boundaryTexture == null || metricBuffer == null ||
                buildCurrentShoreEdgesKernel < 0 ||
                composeTopologyKernel < 0)
            {
                return;
            }

            ConfigureTopologyParameters(0f);

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            bool disturbanceAvailable =
                disturbanceRuntime != null &&
                disturbanceRuntime.IsAllocated;
            RenderTexture staticWakeSource = disturbanceAvailable
                ? disturbanceRuntime.StaticWakeSourceTexture
                : null;
            RenderTexture staticPressureSource = disturbanceAvailable
                ? disturbanceRuntime.StaticPressureTexture
                : null;
            bool staticWakeAvailable = IsCreatedTexture(staticWakeSource);
            bool staticPressureAvailable = IsCreatedTexture(staticPressureSource);
            Texture staticWakeTexture = staticWakeAvailable
                ? staticWakeSource
                : neutralDisturbanceTexture;
            Texture staticPressureTexture = staticPressureAvailable
                ? staticPressureSource
                : neutralDisturbanceTexture;
            Vector2Int staticWakeDimensions = staticWakeAvailable
                ? new Vector2Int(staticWakeSource.width, staticWakeSource.height)
                : Vector2Int.one;
            Vector2Int staticPressureDimensions = staticPressureAvailable
                ? new Vector2Int(
                    staticPressureSource.width,
                    staticPressureSource.height)
                : Vector2Int.one;

            computeShader.SetInts(
                "_FoamStaticWakeDimensions",
                staticWakeDimensions.x,
                staticWakeDimensions.y);
            computeShader.SetInts(
                "_FoamStaticPressureDimensions",
                staticPressureDimensions.x,
                staticPressureDimensions.y);
            computeShader.SetFloat(
                "_FoamDisturbanceEnabled",
                staticWakeAvailable || staticPressureAvailable ? 1f : 0f);

            computeShader.SetBuffer(
                buildCurrentShoreEdgesKernel,
                "_FoamMetricRows",
                metricBuffer);
            computeShader.SetTexture(
                buildCurrentShoreEdgesKernel,
                "_FoamCurrentShoreEdgesWrite",
                currentShoreEdgesTexture);
            DispatchOneDimensional(
                buildCurrentShoreEdgesKernel,
                guidanceWidth,
                64);

            UpdateObstacleExclusionMask();

            computeShader.SetBuffer(
                composeTopologyKernel,
                "_FoamMetricRows",
                metricBuffer);
            computeShader.SetTexture(
                composeTopologyKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetTexture(
                composeTopologyKernel,
                "_FoamObstacleExclusionRead",
                obstacleExclusionTexture);
            computeShader.SetTexture(
                composeTopologyKernel,
                "_FoamTopologyMajorRead",
                topologyMajorTexture);
            computeShader.SetTexture(
                composeTopologyKernel,
                "_FoamTopologyPocketRead",
                topologyPocketTexture);
            computeShader.SetTexture(
                composeTopologyKernel,
                "_FoamTopologyConnectorRead",
                topologyConnectorTexture);
            computeShader.SetTexture(
                composeTopologyKernel,
                "_FoamStaticWakeField",
                staticWakeTexture);
            computeShader.SetTexture(
                composeTopologyKernel,
                "_FoamStaticPressureField",
                staticPressureTexture);
            computeShader.SetTexture(
                composeTopologyKernel,
                "_FoamCurrentShoreEdgesRead",
                currentShoreEdgesTexture);
            computeShader.SetTexture(
                composeTopologyKernel,
                "_FoamTopologyWrite",
                topologyTexture);
            computeShader.SetTexture(
                composeTopologyKernel,
                "_FoamTopologySourcesWrite",
                topologySourcesTexture);
            Dispatch(
                composeTopologyKernel,
                guidanceWidth,
                guidanceHeight);

            if (measureMetrics)
            {
                MeasureTopologyMetrics();
            }
        }

        private void MeasureTopologyMetrics()
        {
            if (computeShader == null || currentState == null ||
                topologyTexture == null || topologySourcesTexture == null ||
                topologyMetricsBuffer == null ||
                resetTopologyMetricsKernel < 0 ||
                measureTopologyMetricsKernel < 0)
            {
                return;
            }

            computeShader.SetBuffer(
                resetTopologyMetricsKernel,
                "_FoamTopologyMetrics",
                topologyMetricsBuffer);
            DispatchOneDimensional(
                resetTopologyMetricsKernel,
                TopologyMetricCount,
                64);

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetInts(
                "_FoamTopologyDimensions",
                guidanceWidth,
                guidanceHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat(
                "_FoamVisibleThreshold",
                river.FoamPopulationVisibleThreshold);
            computeShader.SetTexture(
                measureTopologyMetricsKernel,
                "_FoamTopologyRead",
                topologyTexture);
            computeShader.SetTexture(
                measureTopologyMetricsKernel,
                "_FoamTopologySourcesRead",
                topologySourcesTexture);
            computeShader.SetTexture(
                measureTopologyMetricsKernel,
                "_FoamStateRead",
                currentState);
            computeShader.SetTexture(
                measureTopologyMetricsKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetBuffer(
                measureTopologyMetricsKernel,
                "_FoamTopologyMetrics",
                topologyMetricsBuffer);
            Dispatch(
                measureTopologyMetricsKernel,
                guidanceWidth,
                guidanceHeight);
            RequestTopologyMetricsReadback();
        }

        private void RequestTopologyMetricsReadback()
        {
            if (!SystemInfo.supportsAsyncGPUReadback ||
                topologyMetricsReadbackPending ||
                topologyMetricsBuffer == null)
            {
                return;
            }

            topologyMetricsReadbackPending = true;
            int generation = topologyMetricsGeneration;
            ComputeBuffer requestedBuffer = topologyMetricsBuffer;
            AsyncGPUReadback.Request(
                requestedBuffer,
                request =>
                {
                    if (this == null || generation != topologyMetricsGeneration)
                    {
                        requestedBuffer?.Release();
                        return;
                    }

                    topologyMetricsReadbackPending = false;
                    if (request.hasError)
                    {
                        topologyMetricsAvailable = false;
                        return;
                    }

                    var data = request.GetData<uint>();
                    int count = Mathf.Min(
                        data.Length,
                        latestTopologyMetrics.Length);
                    for (int index = 0; index < count; index++)
                    {
                        latestTopologyMetrics[index] = data[index];
                    }

                    topologyMetricsAvailable = count == TopologyMetricCount;
                });
        }

        private float TopologyCoverageRatio(int numeratorIndex)
        {
            return TopologyRegionRatio(numeratorIndex, 0);
        }

        private float TopologyRegionRatio(
            int numeratorIndex,
            int denominatorIndex)
        {
            if (!topologyMetricsAvailable ||
                numeratorIndex < 0 || numeratorIndex >= TopologyMetricCount ||
                denominatorIndex < 0 || denominatorIndex >= TopologyMetricCount)
            {
                return 0f;
            }

            uint denominator = latestTopologyMetrics[denominatorIndex];
            return denominator > 0u
                ? latestTopologyMetrics[numeratorIndex] / (float)denominator
                : 0f;
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
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
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
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
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
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
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
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
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
            propertyBlock.SetTexture(FoamTopologyId, topologyTexture);
            propertyBlock.SetTexture(
                FoamTopologySourcesId,
                topologySourcesTexture);
            propertyBlock.SetTexture(FoamFractureId, currentFracture);
            propertyBlock.SetTexture(FoamBoundaryId, boundaryTexture);
            propertyBlock.SetTexture(
                FoamObstacleExclusionId,
                obstacleExclusionTexture);
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
            propertyBlock.SetTexture(FoamTopologyId, Texture2D.blackTexture);
            propertyBlock.SetTexture(
                FoamTopologySourcesId,
                Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamFractureId, Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamBoundaryId, Texture2D.blackTexture);
            propertyBlock.SetTexture(
                FoamObstacleExclusionId,
                Texture2D.blackTexture);
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
            ReleaseTexture(ref topologyTexture);
            ReleaseTexture(ref topologySourcesTexture);
            ReleaseTexture(ref topologyMajorTexture);
            ReleaseTexture(ref topologyPocketTexture);
            ReleaseTexture(ref topologyConnectorTexture);
            ReleaseTexture(ref currentShoreEdgesTexture);
            ReleaseTexture(ref obstacleExclusionTexture);
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
            ReleaseObstacleExclusionBuffers();
            obstacleIntervalCellCount = 0;
            obstacleExclusionMeshes.Clear();
            obstacleBakedCells.Clear();
            obstacleSamples.Clear();
            obstacleIntervalCells.Clear();
            obstacleGeometryVersion = -1;
            populationMetricsBuffer?.Release();
            populationMetricsBuffer = null;
            if (topologyMetricsReadbackPending)
            {
                // The request callback owns this retired buffer until the GPU
                // copy completes. Releasing it here can invalidate the request.
                topologyMetricsBuffer = null;
            }
            else
            {
                topologyMetricsBuffer?.Release();
                topologyMetricsBuffer = null;
            }

            topologyMetricsGeneration++;
            topologyMetricsReadbackPending = false;
            topologyMetricsAvailable = false;
            Array.Clear(
                latestTopologyMetrics,
                0,
                latestTopologyMetrics.Length);
            metricRows = Array.Empty<FoamMetricRow>();
            computeShader = null;
            clearKernel = -1;
            injectKernel = -1;
            buildGuidanceKernel = -1;
            buildCurrentShoreEdgesKernel = -1;
            buildTopologyMajorKernel = -1;
            buildTopologyPocketsKernel = -1;
            buildTopologyConnectorsKernel = -1;
            composeTopologyKernel = -1;
            clearObstacleExclusionKernel = -1;
            updateObstacleExclusionKernel = -1;
            resetTopologyMetricsKernel = -1;
            measureTopologyMetricsKernel = -1;
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
            validFieldLength = 0f;
            simulationFieldLength = 0f;
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
                    RenderTextureFormat.RHalf => 2L,
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

        private static float AcrossMetresTo01(
            float acrossMetres,
            float leftHalfWidth,
            float rightHalfWidth)
        {
            if (acrossMetres <= 0f)
            {
                return Mathf.Clamp01(
                    0.5f + acrossMetres /
                    Mathf.Max(0.001f, leftHalfWidth * 2f));
            }

            return Mathf.Clamp01(
                0.5f + acrossMetres /
                Mathf.Max(0.001f, rightHalfWidth * 2f));
        }

    }
}
