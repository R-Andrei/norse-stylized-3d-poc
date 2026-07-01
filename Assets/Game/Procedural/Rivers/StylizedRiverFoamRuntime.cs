using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ProgrammaticStylized3D.Geometry;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    /// <summary>
    /// Hidden Stage 6 runtime that owns the complete shared Foam network.
    /// Amount, Freshness, Integrity, and material phase are transported in one
    /// persistent state while a shared structural-resolution guidance field,
    /// GPU-only population controller, boundaries, Wake, and Impact activity organise
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
        // Stage 6 structural resolution is shared by persistent material,
        // topology, guidance, and the authoritative obstacle mask. Medium is
        // the standard project tier; Low and High preserve the same physical
        // topology scale while changing only spatial precision and workload.
        private const int LowStructuralResolution = 64;
        private const int MediumStructuralResolution = 96;
        private const int HighStructuralResolution = 128;
        private const float ResourceReleaseDelaySeconds = 2f;
        private const float MaximumManualReservationSeconds = 90f;
        private const float DecayToFivePercent = 2.995732f;
        private const float TopologyMetricsUpdateRate = 2f;
        private const int ThreadGroupSize = 8;
        private const int TopologyMetricCount = 16;
        private const float TopologyMetricQuantisation = 1023f;
        // Canonical Stage 6 Shore Support band measured inward from the
        // instantaneous Stage 3 visible water edge. These metric widths remain
        // fixed while the accepted anchored-support contract is retained.
        private const float ShoreSupportCoreWidthMetres = 0.228127f;
        private const float ShoreSupportFadeWidthMetres = 0.03f;
        private const int ObstacleRebuildStableFrameCount = 2;

        // The old broad Foam authoring controls were removed in Patch 3.4.
        // Persistent material behaviour remains on one fixed provisional
        // baseline matching the supplied project state until the dedicated
        // lifecycle pass proves and exposes a new canonical control set. These
        // are implementation values, not public
        // topology authoring controls.
        private const float ProvisionalMaterialStrength = 1.342259f;
        private const float ProvisionalMaterialCoverage = 0.394613f;
        private const float ProvisionalMaterialSharpness = 0.78f;
        private const float ProvisionalMaterialDetailScale = 0.5796f;
        private const float ProvisionalMaterialDetailStrength = 0.637799f;
        private const float ProvisionalMaterialShapeVariety = 0.763854f;
        private const float ProvisionalMaterialFlowFollow = 1.02818f;
        private const float ProvisionalMaterialEvolution = 1.26135f;
        private const float ProvisionalMaterialBreakup = 0.703747f;
        private const float ProvisionalMaterialSpread = 0.42858f;
        private const float ProvisionalMaterialCohesion = 0.056401f;
        private const float ProvisionalMaterialConnectivity = 0.228127f;
        private const float ProvisionalMaterialLifetime = 29.810668f;
        private const float ProvisionalMaterialFreshnessLifetime = 2.307567f;
        private const float ProvisionalMaterialIntegrityDamage = 0.703747f;
        private const float ProvisionalMaterialShoreRetention = 1.35f;
        private const float ProvisionalMaterialTargetCoverage = 0.203078f;
        private const float ProvisionalMaterialSupplyRate = 1.1152f;
        private const float ProvisionalMaterialVisibleThreshold = 0.16432f;
        private const float ProvisionalMaterialGuidanceStrength = 1.15646f;
        private const float ProvisionalMaterialBoundaryAttraction = 1.951f;
        private const float ProvisionalMaterialWakeReinforcement = 1.047333f;
        private const float ProvisionalMaterialImpactReinforcement = 0f;

        private enum InitializationPhase
        {
            NotStarted,
            ReleaseOldResources,
            LoadCompute,
            ResolveKernels,
            ResolveDimensions,
            AllocateMaterialTextures,
            AllocateTopologyTextures,
            AllocateAuxiliaryTextures,
            ClearTopology,
            AllocateBuffers,
            BuildMetricBuffer,
            BuildBoundary,
            WaitForObstacleStability,
            BuildObstacleExclusion,
            BuildMajorTopology,
            BuildConnectorTopology,
            ClearMaterial,
            ClearFracture,
            BuildGuidance,
            RefreshInitialTopologySources,
            MeasureInitialPopulation,
            Finalize,
            Ready,
            Failed
        }

        private enum RebuildPhase
        {
            Idle,
            BuildBoundary,
            ApplyBoundary,
            WaitForObstacleStability,
            BuildObstacleExclusion,
            BuildMajorTopology,
            BuildConnectorTopology,
            RefreshTopologySources
        }

        // The accepted profiler instrumentation remains active throughout the
        // topology proof. Initialization advances through an explicit per-river
        // phase machine, but each
        // existing operation keeps its own marker so cold-start work remains
        // independently measurable while scheduling changes are validated.
        private static readonly ProfilerMarker LateUpdateProfilerMarker =
            new ProfilerMarker("RiverFoam.LateUpdate");
        private static readonly ProfilerMarker EnsureResourcesProfilerMarker =
            new ProfilerMarker("RiverFoam.EnsureResources.Total");
        private static readonly ProfilerMarker InitReleaseOldResourcesProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.ReleaseOldResources");
        private static readonly ProfilerMarker InitLoadComputeProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.LoadCompute");
        private static readonly ProfilerMarker InitResolveKernelsProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.ResolveKernels");
        private static readonly ProfilerMarker InitResolveDimensionsProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.ResolveDimensions");
        private static readonly ProfilerMarker InitAllocateMaterialTexturesProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.AllocateMaterialTextures");
        private static readonly ProfilerMarker InitAllocateTopologyTexturesProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.AllocateTopologyTextures");
        private static readonly ProfilerMarker InitAllocateAuxiliaryTexturesProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.AllocateAuxiliaryTextures");
        private static readonly ProfilerMarker InitClearTopologyProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.ClearTopology");
        private static readonly ProfilerMarker InitAllocateBuffersProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.AllocateBuffers");
        private static readonly ProfilerMarker InitBuildMetricBufferProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.BuildMetricBuffer");
        private static readonly ProfilerMarker InitBuildBoundaryProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.BuildBoundary");
        private static readonly ProfilerMarker InitWaitObstacleProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.WaitObstacleStability");
        private static readonly ProfilerMarker InitBuildObstacleExclusionProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.BuildObstacleExclusion");
        private static readonly ProfilerMarker InitClearMaterialProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.ClearMaterial");
        private static readonly ProfilerMarker InitClearFractureProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.ClearFracture");
        private static readonly ProfilerMarker InitBuildGuidanceProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.BuildGuidance");
        private static readonly ProfilerMarker TopologyBuildMajorProfilerMarker =
            new ProfilerMarker("RiverFoam.Topology.BuildMajor");
        private static readonly ProfilerMarker TopologyBuildConnectorProfilerMarker =
            new ProfilerMarker("RiverFoam.Topology.BuildConnector");
        private static readonly ProfilerMarker TopologyRefreshSourcesProfilerMarker =
            new ProfilerMarker("RiverFoam.Topology.RefreshSources");
        private static readonly ProfilerMarker TopologyComposeProfilerMarker =
            new ProfilerMarker("RiverFoam.Topology.Compose");
        private static readonly ProfilerMarker DiagnosticsMeasurePopulationProfilerMarker =
            new ProfilerMarker("RiverFoam.Diagnostics.MeasurePopulation");
        private static readonly ProfilerMarker DiagnosticsMeasureTopologyProfilerMarker =
            new ProfilerMarker("RiverFoam.Diagnostics.MeasureTopology");
        private static readonly ProfilerMarker RebuildBuildBoundaryProfilerMarker =
            new ProfilerMarker("RiverFoam.Rebuild.BuildBoundary");
        private static readonly ProfilerMarker RebuildApplyBoundaryProfilerMarker =
            new ProfilerMarker("RiverFoam.Rebuild.ApplyBoundary");
        private static readonly ProfilerMarker RebuildWaitObstacleProfilerMarker =
            new ProfilerMarker("RiverFoam.Rebuild.WaitObstacleStability");
        private static readonly ProfilerMarker RebuildBuildObstacleProfilerMarker =
            new ProfilerMarker("RiverFoam.Rebuild.BuildObstacleExclusion");
        private static readonly ProfilerMarker RebuildBuildMajorProfilerMarker =
            new ProfilerMarker("RiverFoam.Rebuild.BuildMajor");
        private static readonly ProfilerMarker RebuildBuildConnectorProfilerMarker =
            new ProfilerMarker("RiverFoam.Rebuild.BuildConnector");
        private static readonly ProfilerMarker RebuildRefreshSourcesProfilerMarker =
            new ProfilerMarker("RiverFoam.Rebuild.RefreshTopologySources");

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
        private RenderTexture topologyGeneratedTexture;
        private RenderTexture currentShoreEdgesTexture;
        private RenderTexture obstacleExclusionTexture;
        private RenderTexture fractureA;
        private RenderTexture fractureB;
        private RenderTexture currentFracture;
        private RenderTexture writeFracture;
        private RenderTexture neutralDisturbanceTexture;
        private Texture2D boundaryTexture;
        private Texture2D obstacleExclusionUploadTexture;
        private Texture2D topologyGeneratedUploadTexture;
        private ComputeBuffer metricBuffer;
        private ComputeBuffer populationMetricsBuffer;
        private ComputeBuffer topologyMetricsBuffer;
        private StylizedRiverDisturbanceRuntime disturbanceRuntime;
        private FoamMetricRow[] metricRows = Array.Empty<FoamMetricRow>();
        private readonly uint[] latestTopologyMetrics =
            new uint[TopologyMetricCount];
        private bool topologyMetricsReadbackPending;
        private bool topologyMetricsAvailable;
        private int topologyMetricsGeneration;
        private StylizedRiverFoamMajorTopology majorTopology;
        private StylizedRiverFoamConnectorTopology connectorTopology;
        private int majorTopologyInputSignature = int.MinValue;
        private int connectorTopologyInputSignature = int.MinValue;

        private readonly List<PendingInjection> pendingInjections = new();
        private readonly List<FoamReservation> reservations = new();
        private readonly List<RiverObstacleExclusionFootprint>
            obstacleExclusionFootprints =
            new();
        private Color[] obstacleExclusionPixels = Array.Empty<Color>();
        private float[] obstacleExclusionScalar = Array.Empty<float>();
        private Color[] topologyGeneratedUploadPixels = Array.Empty<Color>();

        private bool[] chunkActive = Array.Empty<bool>();
        private double[] chunkActiveUntil = Array.Empty<double>();
        private bool resourcesDirty = true;
        private bool boundaryDirty = true;
        private InitializationPhase initializationPhase =
            InitializationPhase.NotStarted;
        private RebuildPhase rebuildPhase = RebuildPhase.Idle;
        private bool pendingBoundaryRebuild;
        private bool pendingObstacleRebuild;
        private bool pendingMajorRebuild;
        private bool pendingConnectorRebuild;
        private bool pendingTopologyRefresh;
        private int pendingObstacleObservedVersion = int.MinValue;
        private int pendingObstacleStableFrameCount;
        private int initializationObstacleObservedVersion = int.MinValue;
        private int initializationObstacleStableFrameCount;
        private bool supportWarningReported;
        private bool allocationWarningReported;
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
        private float topologyMetricsAccumulator;
        private float populationAccumulator;
        private float fractureAccumulator;
        private float simulationInterpolation = 1f;
        private float initializationMotionTime;
        private float lastRuntimeTime;
        private double idleSince;
        private int clearKernel = -1;
        private int injectKernel = -1;
        private int buildGuidanceKernel = -1;
        private int buildCurrentShoreEdgesKernel = -1;
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
        private int obstacleGeometryVersion = -1;
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
        public bool MajorTopologyAvailable => majorTopology != null;
        public int MajorOpportunityCount => majorTopology != null
            ? majorTopology.AttemptedOpportunityCount
            : 0;
        public int MajorAcceptedRegionCount => majorTopology != null
            ? majorTopology.AcceptedRegionCount
            : 0;
        public int MajorRejectedRegionCount => majorTopology != null
            ? majorTopology.RejectedRegionCount
            : 0;
        public int MajorRegionMetadataCount => majorTopology != null
            ? majorTopology.Regions.Count
            : 0;
        public float MajorGeneratedCoverage => majorTopology != null
            ? majorTopology.Coverage
            : 0f;
        public double MajorGenerationMilliseconds => majorTopology != null
            ? majorTopology.GenerationMilliseconds
            : 0.0;
        public string MajorTopRejectionReasons => majorTopology != null
            ? majorTopology.GetTopRejectionSummary()
            : "None";
        public bool ConnectorTopologyAvailable => connectorTopology != null;
        public int ConnectorEligibleEndpointCount => connectorTopology != null
            ? connectorTopology.EligibleEndpointCount
            : 0;
        public int ConnectorPathAttemptCount => connectorTopology != null
            ? connectorTopology.PathAttemptCount
            : 0;
        public int ConnectorAcceptedCount => connectorTopology != null
            ? connectorTopology.AcceptedConnectorCount
            : 0;
        public string ConnectorTopRejectionReason => connectorTopology != null
            ? connectorTopology.GetTopRejectionSummary()
            : "None";
        public double ConnectorGenerationMilliseconds =>
            connectorTopology != null
                ? connectorTopology.GenerationMilliseconds
                : 0.0;
        public int DynamicShoreRowCount => currentShoreEdgesTexture != null
            ? currentShoreEdgesTexture.width
            : 0;
        public bool TopologyMetricsAvailable => topologyMetricsAvailable;
        public float MajorSupportCoverage => TopologyCoverageRatio(1);
        public float ConnectorSupportCoverage => TopologyCoverageRatio(2);
        public float PocketPressureCoverage => TopologyCoverageRatio(3);
        public float FoamWithinPocketPressure => TopologyRegionRatio(4, 3);
        public float VisibleMaterialCoverage => TopologyCoverageRatio(5);
        public float MaterialDeficitInsideNetSupport =>
            topologyMetricsAvailable && latestTopologyMetrics[6] > 0u
                ? latestTopologyMetrics[7] /
                  (TopologyMetricQuantisation * latestTopologyMetrics[6])
                : 0f;
        public float FoamWithinShoreSupport => TopologyRegionRatio(9, 8);
        public float FoamWithinPressureLeeSupport => TopologyRegionRatio(13, 12);
        public float PerimeterRatio => TopologyRegionRatio(14, 5);
        public float LegacyNetSupportCoverage => TopologyCoverageRatio(6);
        public float OpenSpanCoverage => topologyMetricsAvailable
            ? Mathf.Clamp01(1f - LegacyNetSupportCoverage)
            : 0f;
        public float ConnectorMajorOverlap => TopologyRegionRatio(15, 2);
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
        public bool InitializationComplete =>
            initializationPhase == InitializationPhase.Ready;
        public bool ResourcesAllocated =>
            InitializationComplete && currentState != null;
        public bool CorrectedAdvectionActive =>
            InitializationComplete &&
            currentState != null &&
            advectedState != null &&
            reverseState != null;
        public bool IsSleeping =>
            !IsAutomaticMaterialSupplyActive &&
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
            EstimateTextureBytes(topologyGeneratedTexture) +
            EstimateTextureBytes(topologyGeneratedUploadTexture) +
            EstimateTextureBytes(currentShoreEdgesTexture) +
            EstimateTextureBytes(obstacleExclusionTexture) +
            EstimateTextureBytes(fractureA) +
            EstimateTextureBytes(fractureB) +
            EstimateTextureBytes(neutralDisturbanceTexture) +
            EstimateTextureBytes(boundaryTexture) +
            EstimateTextureBytes(obstacleExclusionUploadTexture) +
            (metricBuffer != null
                ? (long)metricBuffer.count * metricBuffer.stride
                : 0L) +
            (populationMetricsBuffer != null
                ? (long)populationMetricsBuffer.count * populationMetricsBuffer.stride
                : 0L) +
            (topologyMetricsBuffer != null
                ? (long)topologyMetricsBuffer.count * topologyMetricsBuffer.stride
                : 0L);

        private bool IsAutomaticMaterialSupplyActive =>
            river != null && river.FoamEnabled;

        private bool IsTopologyDebugActive
        {
            get
            {
                if (river == null)
                {
                    return false;
                }

                StylizedRiverFoamDebugView view = river.FoamDebugView;
                return view == StylizedRiverFoamDebugView.AnchoredSupport ||
                    view == StylizedRiverFoamDebugView.SupportAndNegativeInfluence ||
                    view == StylizedRiverFoamDebugView.SupportClasses ||
                    view == StylizedRiverFoamDebugView.NegativeInfluenceClasses;
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
            rebuildPhase = RebuildPhase.Idle;
            pendingBoundaryRebuild = false;
            pendingObstacleRebuild = false;
            pendingMajorRebuild = false;
            pendingConnectorRebuild = false;
            pendingTopologyRefresh = false;
            pendingObstacleObservedVersion = int.MinValue;
            pendingObstacleStableFrameCount = 0;
            initializationObstacleObservedVersion = int.MinValue;
            initializationObstacleStableFrameCount = 0;
            initializationPhase = InitializationPhase.NotStarted;
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
            using var profilerScope = LateUpdateProfilerMarker.Auto();
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

            bool automaticMaterialSupplyActive =
                IsAutomaticMaterialSupplyActive;
            // TODO: Audit topology/debug gating so Final Foam does not force
            // diagnostic-grade topology composition or metric refreshes.
            bool topologyDebugActive = IsTopologyDebugActive;
            bool materialWork =
                automaticMaterialSupplyActive ||
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

            QueueObstacleRebuildIfNeeded();
            QueueMajorTopologyRebuildIfNeeded();
            bool rebuildPhaseAdvanced = AdvanceQueuedRebuild();
            bool topologyMaintenanceBlocked =
                rebuildPhaseAdvanced || HasQueuedRebuildWork;

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
                        automaticMaterialSupplyActive ||
                        reservations.Count > 0 ||
                        CountActiveChunks() > 0;

                    if (materialStepActive)
                    {
                        UpdateReservations(stepDuration, now);

                        if (automaticMaterialSupplyActive)
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
                    if (guidanceAccumulator >= guidanceInterval)
                    {
                        if (materialStepActive)
                        {
                            BuildGuidanceField(guidanceAccumulator);
                        }

                        guidanceAccumulator %= guidanceInterval;
                    }

                    if (topologyDebugActive &&
                        !topologyMaintenanceBlocked)
                    {
                        // Patch 3 composes the accepted static Major and
                        // Connector fields with the live anchored sources.
                        // Pocket remains empty until its gated step.
                        topologyMetricsAccumulator += stepDuration;
                        float topologyMetricsInterval = 1f /
                            TopologyMetricsUpdateRate;
                        bool measureTopology =
                            topologyMetricsAccumulator >=
                            topologyMetricsInterval;
                        RefreshDynamicTopologySources(measureTopology);
                        if (measureTopology)
                        {
                            topologyMetricsAccumulator %=
                                topologyMetricsInterval;
                        }
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
            if (initializationPhase == InitializationPhase.Failed &&
                (domainChanged || qualityChanged))
            {
                initializationPhase = InitializationPhase.NotStarted;
            }
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
                    ProvisionalMaterialShapeVariety,
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
            topologyMetricsAccumulator = 0f;
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
            using var profilerScope = EnsureResourcesProfilerMarker.Auto();

            if (initializationPhase == InitializationPhase.Ready &&
                AreResourcesCompleteAndCurrent())
            {
                if (boundaryDirty)
                {
                    RequestBoundaryRebuild();
                }

                return true;
            }

            if (initializationPhase == InitializationPhase.Failed)
            {
                return false;
            }

            if (initializationPhase == InitializationPhase.NotStarted ||
                initializationPhase == InitializationPhase.Ready)
            {
                initializationPhase = InitializationPhase.ReleaseOldResources;
            }
            else if (InitializationInputsChanged())
            {
                // A domain or quality change invalidates every later phase.
                // Restart from release rather than allowing partially built
                // resources to become authoritative.
                initializationPhase = InitializationPhase.ReleaseOldResources;
            }

            AdvanceInitializationPhase();
            return initializationPhase == InitializationPhase.Ready;
        }

        private bool HasQueuedRebuildWork =>
            rebuildPhase != RebuildPhase.Idle ||
            pendingBoundaryRebuild ||
            pendingObstacleRebuild ||
            pendingMajorRebuild ||
            pendingConnectorRebuild ||
            pendingTopologyRefresh;

        private void RequestBoundaryRebuild()
        {
            pendingBoundaryRebuild = true;
            pendingMajorRebuild = true;
            pendingTopologyRefresh = true;
        }

        private void RequestObstacleRebuild(int currentObstacleVersion)
        {
            pendingObstacleRebuild = true;
            pendingMajorRebuild = true;
            pendingTopologyRefresh = true;

            if (pendingObstacleObservedVersion != currentObstacleVersion)
            {
                pendingObstacleObservedVersion = currentObstacleVersion;
                pendingObstacleStableFrameCount = 0;
            }
        }

        private void QueueObstacleRebuildIfNeeded()
        {
            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            int currentObstacleVersion = disturbanceRuntime != null
                ? disturbanceRuntime.ObstacleGeometryVersion
                : -1;

            if (currentObstacleVersion != obstacleGeometryVersion)
            {
                RequestObstacleRebuild(currentObstacleVersion);
            }
        }

        private void QueueMajorTopologyRebuildIfNeeded()
        {
            if (majorTopologyInputSignature !=
                ResolveMajorTopologyInputSignature())
            {
                pendingMajorRebuild = true;
                pendingConnectorRebuild = true;
                pendingTopologyRefresh = true;
                return;
            }

            if (connectorTopologyInputSignature ==
                ResolveConnectorTopologyInputSignature())
            {
                return;
            }

            pendingConnectorRebuild = true;
            pendingTopologyRefresh = true;
        }

        private bool AdvanceQueuedRebuild()
        {
            PromotePendingRebuildPhase();
            if (rebuildPhase == RebuildPhase.Idle)
            {
                return false;
            }

            switch (rebuildPhase)
            {
                case RebuildPhase.BuildBoundary:
                    using (RebuildBuildBoundaryProfilerMarker.Auto())
                    {
                        RebuildBoundaryTexture(false);
                    }

                    pendingBoundaryRebuild = false;
                    rebuildPhase = RebuildPhase.ApplyBoundary;
                    break;

                case RebuildPhase.ApplyBoundary:
                    using (RebuildApplyBoundaryProfilerMarker.Auto())
                    {
                        ApplyBoundaryToState(stateA);
                        ApplyBoundaryToState(stateB);
                    }

                    rebuildPhase = pendingObstacleRebuild
                        ? RebuildPhase.WaitForObstacleStability
                        : pendingMajorRebuild
                            ? RebuildPhase.BuildMajorTopology
                            : pendingConnectorRebuild
                                ? RebuildPhase.BuildConnectorTopology
                                : RebuildPhase.RefreshTopologySources;
                    break;

                case RebuildPhase.WaitForObstacleStability:
                    using (RebuildWaitObstacleProfilerMarker.Auto())
                    {
                        disturbanceRuntime ??=
                            GetComponent<StylizedRiverDisturbanceRuntime>();
                        int currentObstacleVersion = disturbanceRuntime != null
                            ? disturbanceRuntime.ObstacleGeometryVersion
                            : -1;

                        if (currentObstacleVersion !=
                            pendingObstacleObservedVersion)
                        {
                            pendingObstacleObservedVersion =
                                currentObstacleVersion;
                            pendingObstacleStableFrameCount = 0;
                        }
                        else
                        {
                            pendingObstacleStableFrameCount++;
                            if (pendingObstacleStableFrameCount >=
                                ObstacleRebuildStableFrameCount)
                            {
                                rebuildPhase =
                                    RebuildPhase.BuildObstacleExclusion;
                            }
                        }
                    }
                    break;

                case RebuildPhase.BuildObstacleExclusion:
                    using (RebuildBuildObstacleProfilerMarker.Auto())
                    {
                        RebuildObstacleExclusionCache();
                    }

                    pendingObstacleRebuild = false;
                    pendingObstacleObservedVersion = int.MinValue;
                    pendingObstacleStableFrameCount = 0;
                    pendingMajorRebuild = true;
                    rebuildPhase = RebuildPhase.BuildMajorTopology;
                    break;

                case RebuildPhase.BuildMajorTopology:
                    using (RebuildBuildMajorProfilerMarker.Auto())
                    {
                        BuildMajorTopology();
                    }

                    pendingMajorRebuild = false;
                    pendingConnectorRebuild = true;
                    rebuildPhase = RebuildPhase.BuildConnectorTopology;
                    break;

                case RebuildPhase.BuildConnectorTopology:
                    using (RebuildBuildConnectorProfilerMarker.Auto())
                    {
                        BuildConnectorTopology();
                    }

                    pendingConnectorRebuild = false;
                    rebuildPhase = pendingTopologyRefresh
                        ? RebuildPhase.RefreshTopologySources
                        : RebuildPhase.Idle;
                    break;

                case RebuildPhase.RefreshTopologySources:
                    using (RebuildRefreshSourcesProfilerMarker.Auto())
                    {
                        RefreshDynamicTopologySources(true);
                    }

                    pendingTopologyRefresh = false;
                    rebuildPhase = RebuildPhase.Idle;
                    break;
            }

            return true;
        }

        private void PromotePendingRebuildPhase()
        {
            if (pendingBoundaryRebuild &&
                (rebuildPhase == RebuildPhase.Idle ||
                 (int)rebuildPhase > (int)RebuildPhase.BuildBoundary))
            {
                rebuildPhase = RebuildPhase.BuildBoundary;
                return;
            }

            if (pendingObstacleRebuild &&
                (rebuildPhase == RebuildPhase.Idle ||
                 (int)rebuildPhase >
                    (int)RebuildPhase.BuildObstacleExclusion))
            {
                rebuildPhase = RebuildPhase.WaitForObstacleStability;
                return;
            }

            if (pendingMajorRebuild &&
                (rebuildPhase == RebuildPhase.Idle ||
                 (int)rebuildPhase >
                    (int)RebuildPhase.BuildMajorTopology))
            {
                rebuildPhase = RebuildPhase.BuildMajorTopology;
                return;
            }

            if (pendingConnectorRebuild &&
                (rebuildPhase == RebuildPhase.Idle ||
                 (int)rebuildPhase >
                    (int)RebuildPhase.BuildConnectorTopology))
            {
                rebuildPhase = RebuildPhase.BuildConnectorTopology;
                return;
            }

            if (pendingTopologyRefresh &&
                rebuildPhase == RebuildPhase.Idle)
            {
                rebuildPhase = RebuildPhase.RefreshTopologySources;
            }
        }

        private bool AreResourcesCompleteAndCurrent()
        {
            return !resourcesDirty &&
                currentState != null &&
                advectedState != null &&
                reverseState != null &&
                guidanceTexture != null &&
                topologyTexture != null &&
                topologySourcesTexture != null &&
                topologyGeneratedTexture != null &&
                majorTopology != null &&
                connectorTopology != null &&
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
                allocatedQuality == river.Quality;
        }

        private bool InitializationInputsChanged()
        {
            if ((int)initializationPhase <=
                    (int)InitializationPhase.ResolveDimensions ||
                (int)initializationPhase >=
                    (int)InitializationPhase.Ready)
            {
                return false;
            }

            return river == null ||
                !river.Domain.IsValid ||
                domainVersion != river.Domain.Version ||
                allocatedQuality != river.Quality;
        }

        private void AdvanceInitializationPhase()
        {
            switch (initializationPhase)
            {
                case InitializationPhase.ReleaseOldResources:
                    using (InitReleaseOldResourcesProfilerMarker.Auto())
                    {
                        ReleaseResources();
                    }

                    initializationPhase = InitializationPhase.LoadCompute;
                    break;

                case InitializationPhase.LoadCompute:
                    using (InitLoadComputeProfilerMarker.Auto())
                    {
                        computeShader = Resources.Load<ComputeShader>(
                            ComputeResourcePath);
                    }

                    if (computeShader == null)
                    {
                        Debug.LogError(
                            $"Stage 6 Foam on '{name}' could not load Resources/{ComputeResourcePath}.compute.",
                            this);
                        initializationPhase = InitializationPhase.Failed;
                        break;
                    }

                    initializationPhase = InitializationPhase.ResolveKernels;
                    break;

                case InitializationPhase.ResolveKernels:
                    using (InitResolveKernelsProfilerMarker.Auto())
                    {
                        ResolveComputeKernels();
                    }

                    initializationPhase = InitializationPhase.ResolveDimensions;
                    break;

                case InitializationPhase.ResolveDimensions:
                    using (InitResolveDimensionsProfilerMarker.Auto())
                    {
                        if (!ResolveInitializationDimensions())
                        {
                            break;
                        }
                    }

                    initializationPhase =
                        InitializationPhase.AllocateMaterialTextures;
                    break;

                case InitializationPhase.AllocateMaterialTextures:
                    using (InitAllocateMaterialTexturesProfilerMarker.Auto())
                    {
                        stateA = CreateFieldTexture("PS3D_RiverFoam_A");
                        stateB = CreateFieldTexture("PS3D_RiverFoam_B");
                        advectedState = CreateFieldTexture(
                            "PS3D_RiverFoam_Advected");
                        reverseState = CreateFieldTexture(
                            "PS3D_RiverFoam_Reverse");
                    }

                    initializationPhase =
                        InitializationPhase.AllocateTopologyTextures;
                    break;

                case InitializationPhase.AllocateTopologyTextures:
                    using (InitAllocateTopologyTexturesProfilerMarker.Auto())
                    {
                        guidanceTexture = CreateGuidanceTexture(
                            "PS3D_RiverFoam_Guidance");
                        topologyTexture = CreateGuidanceTexture(
                            "PS3D_RiverFoam_Topology");
                        topologySourcesTexture = CreateGuidanceTexture(
                            "PS3D_RiverFoam_TopologySources");
                        // Patch 3 uploads deterministic prepared Major and
                        // Connector fields through separate channels of this
                        // generated-topology input. Pocket remains absent.
                        topologyGeneratedTexture = CreateGuidanceTexture(
                            "PS3D_RiverFoam_TopologyGeneratedInput");
                        currentShoreEdgesTexture = CreateShoreEdgesTexture(
                            "PS3D_RiverFoam_CurrentShoreEdges");
                        obstacleExclusionTexture =
                            CreateObstacleExclusionTexture(
                                "PS3D_RiverFoam_ObstacleExclusion");
                    }

                    initializationPhase =
                        InitializationPhase.AllocateAuxiliaryTextures;
                    break;

                case InitializationPhase.AllocateAuxiliaryTextures:
                    using (InitAllocateAuxiliaryTexturesProfilerMarker.Auto())
                    {
                        fractureA = CreateFractureTexture(
                            "PS3D_RiverFoam_FractureA");
                        fractureB = CreateFractureTexture(
                            "PS3D_RiverFoam_FractureB");
                        currentFracture = fractureA;
                        writeFracture = fractureB;
                        neutralDisturbanceTexture =
                            CreateNeutralDisturbanceTexture();
                        previousState = stateA;
                        currentState = stateA;
                        writeState = stateB;
                    }

                    initializationPhase = InitializationPhase.ClearTopology;
                    break;

                case InitializationPhase.ClearTopology:
                    // These textures are sampled before their first authored
                    // write, so clear them explicitly rather than relying on
                    // allocation contents.
                    using (InitClearTopologyProfilerMarker.Auto())
                    {
                        ClearRenderTexture(topologyTexture);
                        ClearRenderTexture(topologySourcesTexture);
                        ClearRenderTexture(topologyGeneratedTexture);
                    }

                    initializationPhase = InitializationPhase.AllocateBuffers;
                    break;

                case InitializationPhase.AllocateBuffers:
                    using (InitAllocateBuffersProfilerMarker.Auto())
                    {
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
                    }

                    initializationPhase =
                        InitializationPhase.BuildMetricBuffer;
                    break;

                case InitializationPhase.BuildMetricBuffer:
                    BuildMetricBuffer();
                    initializationPhase = InitializationPhase.BuildBoundary;
                    break;

                case InitializationPhase.BuildBoundary:
                    RebuildBoundaryTexture(false);
                    initializationObstacleObservedVersion = int.MinValue;
                    initializationObstacleStableFrameCount = 0;
                    initializationPhase =
                        InitializationPhase.WaitForObstacleStability;
                    break;

                case InitializationPhase.WaitForObstacleStability:
                    using (InitWaitObstacleProfilerMarker.Auto())
                    {
                        disturbanceRuntime ??=
                            GetComponent<StylizedRiverDisturbanceRuntime>();
                        int currentObstacleVersion = disturbanceRuntime != null
                            ? disturbanceRuntime.ObstacleGeometryVersion
                            : -1;

                        if (currentObstacleVersion !=
                            initializationObstacleObservedVersion)
                        {
                            initializationObstacleObservedVersion =
                                currentObstacleVersion;
                            initializationObstacleStableFrameCount = 0;
                        }
                        else
                        {
                            initializationObstacleStableFrameCount++;
                            if (initializationObstacleStableFrameCount >=
                                ObstacleRebuildStableFrameCount)
                            {
                                initializationPhase =
                                    InitializationPhase.BuildObstacleExclusion;
                            }
                        }
                    }
                    break;

                case InitializationPhase.BuildObstacleExclusion:
                    RebuildObstacleExclusionCache();
                    initializationObstacleObservedVersion = int.MinValue;
                    initializationObstacleStableFrameCount = 0;
                    initializationPhase = InitializationPhase.BuildMajorTopology;
                    break;

                case InitializationPhase.BuildMajorTopology:
                    BuildMajorTopology();
                    initializationPhase =
                        InitializationPhase.BuildConnectorTopology;
                    break;

                case InitializationPhase.BuildConnectorTopology:
                    BuildConnectorTopology();
                    initializationPhase = InitializationPhase.ClearMaterial;
                    break;

                case InitializationPhase.ClearMaterial:
                    using (InitClearMaterialProfilerMarker.Auto())
                    {
                        DispatchClear(stateA, 0, fieldWidth);
                        DispatchClear(stateB, 0, fieldWidth);
                        DispatchClear(advectedState, 0, fieldWidth);
                        DispatchClear(reverseState, 0, fieldWidth);
                    }

                    initializationPhase = InitializationPhase.ClearFracture;
                    break;

                case InitializationPhase.ClearFracture:
                    using (InitClearFractureProfilerMarker.Auto())
                    {
                        DispatchClearFracture(fractureA, 0, fractureWidth);
                        DispatchClearFracture(fractureB, 0, fractureWidth);
                    }

                    initializationPhase = InitializationPhase.BuildGuidance;
                    break;

                case InitializationPhase.BuildGuidance:
                    BuildGuidanceField(0f);
                    initializationPhase =
                        InitializationPhase.RefreshInitialTopologySources;
                    break;

                case InitializationPhase.RefreshInitialTopologySources:
                    RefreshDynamicTopologySources(true);
                    initializationPhase =
                        InitializationPhase.MeasureInitialPopulation;
                    break;

                case InitializationPhase.MeasureInitialPopulation:
                    MeasurePopulation();
                    initializationPhase = InitializationPhase.Finalize;
                    break;

                case InitializationPhase.Finalize:
                    FinalizeInitialization();
                    break;
            }
        }

        private void ResolveComputeKernels()
        {
            clearKernel = computeShader.FindKernel("ClearRange");
            injectKernel = computeShader.FindKernel("InjectFoam");
            buildGuidanceKernel = computeShader.FindKernel("BuildGuidance");
            buildCurrentShoreEdgesKernel =
                computeShader.FindKernel("BuildCurrentShoreEdges");
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
            resetPopulationKernel =
                computeShader.FindKernel("ResetPopulation");
            measurePopulationKernel =
                computeShader.FindKernel("MeasurePopulation");
            updateFractureKernel =
                computeShader.FindKernel("UpdateFracture");
            clearFractureKernel =
                computeShader.FindKernel("ClearFractureRange");
            advectForwardKernel =
                computeShader.FindKernel("AdvectForward");
            advectReverseKernel =
                computeShader.FindKernel("AdvectReverse");
            simulateKernel = computeShader.FindKernel("SimulateFoam");
            applyBoundaryKernel = computeShader.FindKernel("ApplyBoundary");
        }

        private bool ResolveInitializationDimensions()
        {
            RiverDomainSnapshot domain = river.Domain;
            if (!domain.IsValid)
            {
                return false;
            }

            chunkCount = Mathf.Max(
                1,
                Mathf.CeilToInt(domain.LocalLength / ChunkLengthMetres));
            int desiredStructuralResolution =
                ResolveStructuralResolution(river.Quality);
            resolutionPerChunk = desiredStructuralResolution;

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
                initializationPhase = InitializationPhase.Failed;
                return false;
            }

            fieldHeight = desiredStructuralResolution;

            // Topology and guidance use the same structural grid as persistent
            // material so narrow structures are not authored on a coarser
            // hidden lattice and then upsampled.
            guidanceWidth = fieldWidth;
            guidanceHeight = fieldHeight;
            fractureWidth = Mathf.Max(16, Mathf.CeilToInt(fieldWidth * 0.5f));
            fractureHeight = Mathf.Max(
                16,
                Mathf.CeilToInt(fieldHeight * 0.5f));

            if (maximumTextureSize < 24 ||
                fieldHeight > maximumTextureSize ||
                guidanceHeight > maximumTextureSize ||
                fractureWidth > maximumTextureSize ||
                fractureHeight > maximumTextureSize)
            {
                ReportAllocationFailure(maximumTextureSize);
                initializationPhase = InitializationPhase.Failed;
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
            initializationMotionTime = river.MotionTime;
            return true;
        }

        private float ResolveInitializationMotionTime()
        {
            return (int)initializationPhase >
                    (int)InitializationPhase.ResolveDimensions &&
                (int)initializationPhase <
                    (int)InitializationPhase.Ready
                    ? initializationMotionTime
                    : river.MotionTime;
        }

        private void FinalizeInitialization()
        {
            if (boundaryDirty)
            {
                // A source changed after the boundary phase. Re-run the
                // boundary and accepted topology-source composition while
                // Foam remains disabled.
                initializationPhase = InitializationPhase.BuildBoundary;
                return;
            }

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            int currentObstacleGeometryVersion =
                disturbanceRuntime != null
                    ? disturbanceRuntime.ObstacleGeometryVersion
                    : -1;
            if (currentObstacleGeometryVersion != obstacleGeometryVersion)
            {
                // The disturbance runtime can settle generated sources over
                // several frames. Keep the partially initialized river hidden
                // and rebuild the obstacle-dependent tail once it stabilises.
                initializationObstacleObservedVersion = int.MinValue;
                initializationObstacleStableFrameCount = 0;
                initializationPhase =
                    InitializationPhase.WaitForObstacleStability;
                return;
            }

            if (majorTopologyInputSignature !=
                ResolveMajorTopologyInputSignature())
            {
                initializationPhase = InitializationPhase.BuildMajorTopology;
                return;
            }

            if (connectorTopologyInputSignature !=
                ResolveConnectorTopologyInputSignature())
            {
                initializationPhase =
                    InitializationPhase.BuildConnectorTopology;
                return;
            }

            resourcesDirty = false;
            simulationAccumulator = 0f;
            guidanceAccumulator = 0f;
            topologyMetricsAccumulator = 0f;
            populationAccumulator = 0f;
            fractureAccumulator = 0f;
            simulationInterpolation = 1f;
            lastRuntimeTime = Time.realtimeSinceStartup;
            rebuildPhase = RebuildPhase.Idle;
            pendingBoundaryRebuild = false;
            pendingObstacleRebuild = false;
            pendingMajorRebuild = false;
            pendingConnectorRebuild = false;
            pendingTopologyRefresh = false;
            pendingObstacleObservedVersion = int.MinValue;
            pendingObstacleStableFrameCount = 0;
            initializationObstacleObservedVersion = int.MinValue;
            initializationObstacleStableFrameCount = 0;
            initializationPhase = InitializationPhase.Ready;
        }

        private static int ResolveStructuralResolution(
            StylizedRiverQuality quality)
        {
            return quality switch
            {
                StylizedRiverQuality.Low => LowStructuralResolution,
                StylizedRiverQuality.Medium => MediumStructuralResolution,
                StylizedRiverQuality.High => HighStructuralResolution,
                _ => MediumStructuralResolution
            };
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

        private static void ClearRenderTexture(RenderTexture texture)
        {
            if (texture == null || !texture.IsCreated())
            {
                return;
            }

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = texture;
            GL.Clear(false, true, Color.clear);
            RenderTexture.active = previous;
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
            using var profilerScope = InitBuildMetricBufferProfilerMarker.Auto();
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
            using var profilerScope = InitBuildBoundaryProfilerMarker.Auto();
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
                    // solids now use the full-resolution Obstacle Footprint
                    // mask. Canonical Shore Support comes from the
                    // instantaneous Stage 3 edge.
                    pixels[y * fieldWidth + x] = new Color(
                        coverage,
                        attraction,
                        0f,
                        0f);
                }
            }

            // Registered solid geometry is no longer rasterised into this
            // static boundary texture. Obstacle Footprint is reconstructed
            // from mesh-derived waterline contours in a dedicated
            // point-sampled mask.

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
            using var profilerScope = InitBuildObstacleExclusionProfilerMarker.Auto();
            ReleaseObstacleExclusionBuffers();
            obstacleExclusionFootprints.Clear();

            int pixelCount = Mathf.Max(0, fieldWidth * fieldHeight);
            if (obstacleExclusionPixels.Length != pixelCount)
            {
                obstacleExclusionPixels = new Color[pixelCount];
            }
            else
            {
                Array.Clear(
                    obstacleExclusionPixels,
                    0,
                    obstacleExclusionPixels.Length);
            }

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

            disturbanceRuntime.CopyObstacleExclusionFootprintsTo(
                obstacleExclusionFootprints);
            if (obstacleExclusionFootprints.Count == 0)
            {
                ClearObstacleExclusionMask();
                return;
            }

            for (int index = 0; index < obstacleExclusionFootprints.Count; index++)
            {
                RasterizeObstacleFootprint(
                    obstacleExclusionFootprints[index],
                    obstacleExclusionPixels);
            }

            UploadObstacleExclusionPixels(obstacleExclusionPixels);
        }

        private void RasterizeObstacleFootprint(
            RiverObstacleExclusionFootprint footprint,
            Color[] pixels)
        {
            if (pixels == null ||
                footprint.Contour == null ||
                footprint.Contour.Length < 3)
            {
                return;
            }

            float globalStart = river.Domain.GlobalDistanceMinimum;
            int minimumX = Mathf.Clamp(
                Mathf.FloorToInt(
                    (footprint.GlobalDistance -
                     footprint.AlongHalfLength -
                     globalStart) /
                    fieldLength * fieldWidth) - 1,
                0,
                fieldWidth - 1);
            int maximumX = Mathf.Clamp(
                Mathf.CeilToInt(
                    (footprint.GlobalDistance +
                     footprint.AlongHalfLength -
                     globalStart) /
                    fieldLength * fieldWidth) + 1,
                0,
                fieldWidth - 1);

            for (int x = minimumX; x <= maximumX; x++)
            {
                float centreU = (x + 0.5f) / Mathf.Max(1f, fieldWidth);
                float centreGlobalDistance = globalStart + centreU * fieldLength;
                if (centreGlobalDistance <
                        river.Domain.GlobalDistanceMinimum - 0.0001f ||
                    centreGlobalDistance >
                        river.Domain.GlobalDistanceMaximum + 0.0001f)
                {
                    continue;
                }

                StylizedRiverSplineSample centreSample =
                    river.Domain.SampleAtGlobalDistance(
                        Mathf.Clamp(
                            centreGlobalDistance,
                            river.Domain.GlobalDistanceMinimum,
                            river.Domain.GlobalDistanceMaximum));
                float minimumAcross = footprint.AcrossMetres -
                    footprint.AcrossHalfWidth;
                float maximumAcross = footprint.AcrossMetres +
                    footprint.AcrossHalfWidth;
                float minimumAcross01 = AcrossMetresTo01(
                    minimumAcross,
                    centreSample.LeftSurfaceHalfWidth,
                    centreSample.RightSurfaceHalfWidth);
                float maximumAcross01 = AcrossMetresTo01(
                    maximumAcross,
                    centreSample.LeftSurfaceHalfWidth,
                    centreSample.RightSurfaceHalfWidth);
                int minimumY = Mathf.Clamp(
                    Mathf.FloorToInt(
                        Mathf.Min(minimumAcross01, maximumAcross01) *
                        fieldHeight) - 1,
                    0,
                    fieldHeight - 1);
                int maximumY = Mathf.Clamp(
                    Mathf.CeilToInt(
                        Mathf.Max(minimumAcross01, maximumAcross01) *
                        fieldHeight) + 1,
                    0,
                    fieldHeight - 1);

                for (int y = minimumY; y <= maximumY; y++)
                {
                    float v = (y + 0.5f) / Mathf.Max(1f, fieldHeight);
                    float acrossMetres = Across01ToMetres(
                        v,
                        centreSample.LeftSurfaceHalfWidth,
                        centreSample.RightSurfaceHalfWidth);
                    Vector2 localPoint = new Vector2(
                        centreGlobalDistance - footprint.GlobalDistance,
                        acrossMetres - footprint.AcrossMetres);
                    if (IsPointInsidePolygon(localPoint, footprint.Contour))
                    {
                        pixels[y * fieldWidth + x] = Color.white;
                    }
                }
            }
        }

        private static bool IsPointInsidePolygon(
            Vector2 point,
            IReadOnlyList<Vector2> polygon)
        {
            if (polygon == null || polygon.Count < 3)
            {
                return false;
            }

            bool inside = false;
            int previous = polygon.Count - 1;
            for (int current = 0; current < polygon.Count; current++)
            {
                Vector2 a = polygon[current];
                Vector2 b = polygon[previous];
                bool crosses =
                    (a.y > point.y) != (b.y > point.y);
                if (crosses)
                {
                    float deltaY = b.y - a.y;
                    if (Mathf.Abs(deltaY) <= 0.000001f)
                    {
                        previous = current;
                        continue;
                    }

                    float xAtY = (b.x - a.x) *
                        (point.y - a.y) /
                        deltaY +
                        a.x;
                    if (point.x < xAtY)
                    {
                        inside = !inside;
                    }
                }

                previous = current;
            }

            return inside;
        }

        private void UploadObstacleExclusionPixels(Color[] pixels)
        {
            if (obstacleExclusionTexture == null ||
                pixels == null ||
                pixels.Length != fieldWidth * fieldHeight)
            {
                ClearObstacleExclusionMask();
                return;
            }

            if (obstacleExclusionUploadTexture == null ||
                obstacleExclusionUploadTexture.width != fieldWidth ||
                obstacleExclusionUploadTexture.height != fieldHeight)
            {
                if (obstacleExclusionUploadTexture != null)
                {
                    DestroyUnityObject(obstacleExclusionUploadTexture);
                }

                obstacleExclusionUploadTexture = new Texture2D(
                    fieldWidth,
                    fieldHeight,
                    TextureFormat.RGBAHalf,
                    false,
                    true)
                {
                    name = "T_PS3D_RiverFoamObstacleFootprint_Upload",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.DontSave
                };
            }

            obstacleExclusionUploadTexture.SetPixels(pixels);
            obstacleExclusionUploadTexture.Apply(false, false);
            Graphics.Blit(
                obstacleExclusionUploadTexture,
                obstacleExclusionTexture);
        }

        private void ReleaseObstacleExclusionBuffers()
        {
            if (obstacleExclusionUploadTexture != null)
            {
                DestroyUnityObject(obstacleExclusionUploadTexture);
                obstacleExclusionUploadTexture = null;
            }
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
            // Obstacle Footprint is now a high-resolution waterline-contour
            // raster uploaded when the generated-geometry version changes.
            // This per-topology refresh hook is retained so call sites do not
            // accidentally clear the cached mask.
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
                ProvisionalMaterialFlowFollow * liquid;
            float amountDecay =
                DecayToFivePercent /
                Mathf.Max(0.05f, ProvisionalMaterialLifetime);
            float spread =
                ProvisionalMaterialSpread *
                ProvisionalMaterialEvolution;

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
                now + Mathf.Max(
                    ProvisionalMaterialLifetime,
                    ProvisionalMaterialFreshnessLifetime));
        }

        private void ActivateReservationRange(FoamReservation reservation, float now)
        {
            float margin = Mathf.Max(
                0.5f,
                Mathf.Abs(
                    river.FlowSpeedMetresPerSecond *
                    ProvisionalMaterialFlowFollow) /
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
            using var profilerScope = InitBuildGuidanceProfilerMarker.Auto();
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
            computeShader.SetFloat(
                "_FoamTime",
                ResolveInitializationMotionTime());
            computeShader.SetFloat("_FoamSeed", river.VisualSeed);
            computeShader.SetFloat(
                "_FoamEvolution",
                ProvisionalMaterialEvolution);
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

        private void BuildMajorTopology()
        {
            using var profilerScope = TopologyBuildMajorProfilerMarker.Auto();
            if (river == null || !river.Domain.IsValid ||
                topologyGeneratedTexture == null ||
                fieldWidth < 2 || fieldHeight < 2 ||
                fieldLength <= 0.0001f || validFieldLength <= 0.0001f)
            {
                majorTopology = null;
                connectorTopology = null;
                majorTopologyInputSignature = int.MinValue;
                connectorTopologyInputSignature = int.MinValue;
                ClearRenderTexture(topologyGeneratedTexture);
                return;
            }

            int cellCount = fieldWidth * fieldHeight;
            if (obstacleExclusionScalar.Length != cellCount)
            {
                obstacleExclusionScalar = new float[cellCount];
            }

            for (int index = 0; index < cellCount; index++)
            {
                obstacleExclusionScalar[index] =
                    index < obstacleExclusionPixels.Length
                        ? Mathf.Clamp01(obstacleExclusionPixels[index].r)
                        : 0f;
            }

            majorTopology =
                StylizedRiverFoamMajorTopologyGenerator.Generate(
                    river.Domain,
                    fieldWidth,
                    fieldHeight,
                    fieldLength,
                    validFieldLength,
                    river.Quality,
                    river.ShoreMotion,
                    river.FoamMajorSupportAmount,
                    river.FoamMajorSupportSize,
                    river.FoamMajorSupportSizeVariation,
                    river.FoamMajorSupportSeed,
                    obstacleExclusionScalar);
            connectorTopology = null;
            connectorTopologyInputSignature = int.MinValue;
            UploadGeneratedTopology();
            majorTopologyInputSignature = ResolveMajorTopologyInputSignature();
        }

        private void BuildConnectorTopology()
        {
            using var profilerScope =
                TopologyBuildConnectorProfilerMarker.Auto();
            if (river == null || !river.Domain.IsValid ||
                topologyGeneratedTexture == null ||
                majorTopology == null ||
                fieldWidth < 2 || fieldHeight < 2 ||
                fieldLength <= 0.0001f || validFieldLength <= 0.0001f)
            {
                connectorTopology = null;
                connectorTopologyInputSignature = int.MinValue;
                UploadGeneratedTopology();
                return;
            }

            connectorTopology =
                StylizedRiverFoamConnectorTopologyGenerator.Generate(
                    river.Domain,
                    fieldWidth,
                    fieldHeight,
                    fieldLength,
                    validFieldLength,
                    river.Quality,
                    river.ShoreMotion,
                    river.FoamMajorSupportSeed,
                    river.FoamConnectorAmount,
                    river.FoamConnectorDirectness,
                    river.FoamConnectorLengthPreference,
                    obstacleExclusionScalar,
                    majorTopology);
            connectorTopologyInputSignature =
                ResolveConnectorTopologyInputSignature();
            UploadGeneratedTopology();
        }

        private void UploadGeneratedTopology()
        {
            if (topologyGeneratedTexture == null ||
                fieldWidth < 1 || fieldHeight < 1)
            {
                return;
            }

            int cellCount = fieldWidth * fieldHeight;
            if (topologyGeneratedUploadPixels.Length != cellCount)
            {
                topologyGeneratedUploadPixels = new Color[cellCount];
            }
            else
            {
                Array.Clear(
                    topologyGeneratedUploadPixels,
                    0,
                    topologyGeneratedUploadPixels.Length);
            }

            majorTopology?.FillUploadPixels(topologyGeneratedUploadPixels);
            connectorTopology?.AddToUploadPixels(
                topologyGeneratedUploadPixels);

            if (topologyGeneratedUploadTexture == null ||
                topologyGeneratedUploadTexture.width != fieldWidth ||
                topologyGeneratedUploadTexture.height != fieldHeight)
            {
                if (topologyGeneratedUploadTexture != null)
                {
                    DestroyUnityObject(topologyGeneratedUploadTexture);
                }

                topologyGeneratedUploadTexture = new Texture2D(
                    fieldWidth,
                    fieldHeight,
                    TextureFormat.RGBAHalf,
                    false,
                    true)
                {
                    name = "T_PS3D_RiverFoamGeneratedTopology_Upload",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.DontSave
                };
            }

            topologyGeneratedUploadTexture.SetPixels(
                topologyGeneratedUploadPixels);
            topologyGeneratedUploadTexture.Apply(false, false);
            Graphics.Blit(
                topologyGeneratedUploadTexture,
                topologyGeneratedTexture);
        }

        private int ResolveMajorTopologyInputSignature()
        {
            if (river == null || !river.Domain.IsValid ||
                fieldWidth < 2 || fieldHeight < 2)
            {
                return int.MinValue + 1;
            }

            unchecked
            {
                int hash = 17;
                hash = hash * 31 + river.Domain.Version;
                hash = hash * 31 + obstacleGeometryVersion;
                hash = hash * 31 + (int)river.Quality;
                hash = hash * 31 + fieldWidth;
                hash = hash * 31 + fieldHeight;
                hash = hash * 31 + river.FoamMajorSupportSeed;
                hash = hash * 31 + Mathf.RoundToInt(
                    river.FoamMajorSupportAmount * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(
                    river.FoamMajorSupportSize * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(
                    river.FoamMajorSupportSizeVariation * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(
                    river.ShoreMotion * 10000f);
                return hash;
            }
        }

        private int ResolveConnectorTopologyInputSignature()
        {
            if (river == null || !river.Domain.IsValid ||
                fieldWidth < 2 || fieldHeight < 2 ||
                majorTopology == null)
            {
                return int.MinValue + 2;
            }

            unchecked
            {
                int hash = 23;
                hash = hash * 31 + majorTopologyInputSignature;
                hash = hash * 31 + Mathf.RoundToInt(
                    river.FoamConnectorAmount * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(
                    river.FoamConnectorDirectness * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(
                    river.FoamConnectorLengthPreference * 10000f);
                return hash;
            }
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
            computeShader.SetFloat(
                "_FoamFlowSpeed",
                river.FlowSpeedMetresPerSecond * river.LiquidFactor);
            computeShader.SetFloat(
                "_FoamFlowDirection",
                river.FlowDirection);
            computeShader.SetFloat(
                "_FoamTime",
                ResolveInitializationMotionTime());
            computeShader.SetFloat("_FoamSeed", river.VisualSeed);
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
                ShoreSupportCoreWidthMetres);
            computeShader.SetFloat(
                "_FoamShoreCaptureFadeWidth",
                ShoreSupportFadeWidthMetres);
        }

        private void RefreshDynamicTopologySources(bool measureMetrics)
        {
            using var profilerScope = TopologyRefreshSourcesProfilerMarker.Auto();
            if (computeShader == null || topologyTexture == null ||
                topologySourcesTexture == null ||
                topologyGeneratedTexture == null ||
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

            using (TopologyComposeProfilerMarker.Auto())
            {
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
                    "_FoamTopologyGeneratedRead",
                    topologyGeneratedTexture);
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
            }

            if (measureMetrics)
            {
                MeasureTopologyMetrics();
            }
        }

        private void MeasureTopologyMetrics()
        {
            using var profilerScope = DiagnosticsMeasureTopologyProfilerMarker.Auto();
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
                ProvisionalMaterialVisibleThreshold);
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
            using var profilerScope = DiagnosticsMeasurePopulationProfilerMarker.Auto();
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
                ProvisionalMaterialVisibleThreshold);
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
                ProvisionalMaterialFlowFollow *
                river.LiquidFactor);
            // Pressure Support builds its fail-closed upstream support
            // envelope from the exact obstacle mask. Flow direction selects
            // which side of each solid cell is the object-facing front on both
            // normal-flow and reverse-flow rivers.
            computeShader.SetFloat(
                "_FoamFlowDirection",
                river.FlowDirection);
            computeShader.SetFloat(
                "_FoamEvolution",
                ProvisionalMaterialEvolution);
            computeShader.SetFloat("_FoamBreakup", ProvisionalMaterialBreakup);
            computeShader.SetFloat("_FoamSpread", ProvisionalMaterialSpread);
            computeShader.SetFloat("_FoamCohesion", ProvisionalMaterialCohesion);
            computeShader.SetFloat(
                "_FoamConnectivity",
                ProvisionalMaterialConnectivity);
            computeShader.SetFloat(
                "_FoamAmountDecay",
                DecayToFivePercent /
                Mathf.Max(0.05f, ProvisionalMaterialLifetime));
            computeShader.SetFloat(
                "_FoamFreshnessDecay",
                DecayToFivePercent /
                Mathf.Max(0.05f, ProvisionalMaterialFreshnessLifetime));
            computeShader.SetFloat(
                "_FoamIntegrityDamage",
                Mathf.Clamp01(ProvisionalMaterialIntegrityDamage));
            computeShader.SetFloat(
                "_FoamShoreRetention",
                ProvisionalMaterialShoreRetention);
            computeShader.SetFloat("_FoamTime", river.MotionTime);
            computeShader.SetFloat("_FoamSeed", river.VisualSeed);
            computeShader.SetFloat(
                "_FoamTargetCoverage",
                IsAutomaticMaterialSupplyActive
                    ? ProvisionalMaterialTargetCoverage
                    : 0f);
            computeShader.SetFloat(
                "_FoamSupplyRate",
                IsAutomaticMaterialSupplyActive
                    ? ProvisionalMaterialSupplyRate
                    : 0f);
            computeShader.SetFloat(
                "_FoamVisibleThreshold",
                ProvisionalMaterialVisibleThreshold);
            computeShader.SetFloat(
                "_FoamGuidanceStrength",
                ProvisionalMaterialGuidanceStrength);
            computeShader.SetFloat(
                "_FoamBoundaryAttraction",
                ProvisionalMaterialBoundaryAttraction);
            computeShader.SetFloat(
                "_FoamWakeReinforcement",
                ProvisionalMaterialWakeReinforcement);
            computeShader.SetFloat(
                "_FoamImpactReinforcement",
                ProvisionalMaterialImpactReinforcement);

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
            propertyBlock.SetFloat(
                FoamStrengthId,
                ProvisionalMaterialStrength);
            propertyBlock.SetFloat(
                FoamCoverageId,
                ProvisionalMaterialCoverage);
            propertyBlock.SetFloat(
                FoamSharpnessId,
                ProvisionalMaterialSharpness);
            propertyBlock.SetFloat(
                FoamDetailScaleId,
                ProvisionalMaterialDetailScale);
            propertyBlock.SetFloat(
                FoamDetailStrengthId,
                ProvisionalMaterialDetailStrength);
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
            ReleaseTexture(ref topologyGeneratedTexture);
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

            if (topologyGeneratedUploadTexture != null)
            {
                DestroyUnityObject(topologyGeneratedUploadTexture);
                topologyGeneratedUploadTexture = null;
            }

            metricBuffer?.Release();
            metricBuffer = null;
            ReleaseObstacleExclusionBuffers();
            obstacleExclusionFootprints.Clear();
            obstacleExclusionPixels = Array.Empty<Color>();
            obstacleExclusionScalar = Array.Empty<float>();
            topologyGeneratedUploadPixels = Array.Empty<Color>();
            majorTopology = null;
            connectorTopology = null;
            majorTopologyInputSignature = int.MinValue;
            connectorTopologyInputSignature = int.MinValue;
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
            initializationMotionTime = 0f;
            chunkActive = Array.Empty<bool>();
            chunkActiveUntil = Array.Empty<double>();
            resourcesDirty = true;
            boundaryDirty = true;
            rebuildPhase = RebuildPhase.Idle;
            pendingBoundaryRebuild = false;
            pendingObstacleRebuild = false;
            pendingMajorRebuild = false;
            pendingConnectorRebuild = false;
            pendingTopologyRefresh = false;
            pendingObstacleObservedVersion = int.MinValue;
            pendingObstacleStableFrameCount = 0;
            initializationObstacleObservedVersion = int.MinValue;
            initializationObstacleStableFrameCount = 0;
            initializationPhase = InitializationPhase.NotStarted;
            domainVersion = -1;
            guidanceAccumulator = 0f;
            topologyMetricsAccumulator = 0f;
            populationAccumulator = 0f;
            fractureAccumulator = 0f;
        }

        private void HandleDomainChanged(RiverDomainSnapshot _)
        {
            resourcesDirty = true;
            boundaryDirty = true;
            if (initializationPhase == InitializationPhase.Failed)
            {
                initializationPhase = InitializationPhase.NotStarted;
            }
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
