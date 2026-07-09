using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ProgrammaticStylized3D.Geometry;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private StylizedRiver river;
        private MeshRenderer surfaceRenderer;
        private MaterialPropertyBlock propertyBlock;
        private ComputeShader computeShader;
        private RenderTexture stateA;
        private RenderTexture stateB;
        private RenderTexture progressiveBirthDebugTexture;
        private RenderTexture previousState;
        private RenderTexture currentState;
        private RenderTexture writeState;
        private RenderTexture shapeMaskTexture;
        private RenderTexture filmSourceTexture;
        private RenderTexture filmSupportTexture;
        private RenderTexture topologyTexture;
        private RenderTexture topologySourcesTexture;
        private RenderTexture topologyGeneratedTexture;
        private RenderTexture evolvingMajorTexture;
        private RenderTexture evolvingHostedNegativeTexture;
        private RenderTexture evolvingFreeWaterNegativeTexture;
        private RenderTexture evolvingConnectorTexture;
        private RenderTexture evolvingWeakSpanNegativeTexture;
        private RenderTexture currentShoreEdgesTexture;
        private RenderTexture obstacleExclusionTexture;
        private Texture2D motionLaneTexture;
        private Texture2D obstacleRoutingTexture;
        private RenderTexture neutralDisturbanceTexture;
        private Texture2D boundaryTexture;
        private Texture2D obstacleExclusionReadbackTexture;
        private Texture2D obstacleExclusionUploadTexture;
        private ushort[] motionLaneHalfData = Array.Empty<ushort>();
        private ushort[] obstacleRoutingHalfData = Array.Empty<ushort>();
        private float[] motionLaneRawValues = Array.Empty<float>();
        private bool[] obstacleRoutingOccupied = Array.Empty<bool>();
        private bool[] obstacleRoutingVisited = Array.Empty<bool>();
        private int[] obstacleRoutingComponentIds = Array.Empty<int>();
        private int[] obstacleRoutingQueue = Array.Empty<int>();
        private readonly List<FoamObstacleRoutingComponent> obstacleRoutingComponents = new();
        private int motionLaneFieldSignature = int.MinValue;
        private int obstacleRoutingFieldSignature = int.MinValue;
        private float motionLaneScrollCells;
        private Texture2D topologyGeneratedUploadTexture;
        private Texture2DArray majorMaskTextureArray;
        private Texture2DArray hostedNegativeMaskTextureArray;
        private Texture2DArray freeWaterNegativeMaskTextureArray;
        private ComputeBuffer metricBuffer;
        private ComputeBuffer obstacleExclusionCellBuffer;
        private ComputeBuffer obstacleExclusionSampleBuffer;
        private ComputeBuffer topologyMetricsBuffer;
        private ComputeBuffer progressiveBirthDebugCounterBuffer;
        private ComputeBuffer automaticFoamSourceEventBuffer;
        private ComputeBuffer majorEvolutionBuffer;
        private ComputeBuffer hostedNegativeEvolutionBuffer;
        private ComputeBuffer freeWaterEvolutionBuffer;
        private ComputeBuffer connectorIdentityBuffer;
        private ComputeBuffer connectorPathPointBuffer;
        private ComputeBuffer weakSpanIdentityBuffer;
        private StylizedRiverDisturbanceRuntime disturbanceRuntime;
        private FoamMetricRow[] metricRows = Array.Empty<FoamMetricRow>();
        private readonly uint[] latestTopologyMetrics =
            new uint[TopologyMetricCount];
        private bool topologyMetricsReadbackPending;
        private bool topologyMetricsAvailable;
        private double topologyMetricsLastCompletedAt = -1.0;
        private double topologyMetricsReadbackRequestedAt = -1.0;
        private int topologyMetricsGeneration;
        private float integratedPresenceArea;
        private float visiblePresenceCoreArea;
        private float manualProofReferenceArea;
        private bool manualProofReferencePending;
        // Patch 4.11C.5.4b: Remaining Life in the material texture is
        // the only foam survival authority. This flag keeps lifecycle
        // simulation running while material may still be alive, independent
        // of reservation/chunk scheduling windows.
        private bool materialLifetimeAuthorityActive;
        private int materialLifetimeEmptyMetricReadbacks;
        private string lifetimeAuthorityStatus =
            "Remaining Life owns survival";
        private bool materialClockSessionActive;
        private double materialClockSessionStartedAt = -1.0;
        private float materialSimulatedSecondsSinceSession;
        private int materialStepCountSinceSession;
        private int birthCommandsThisFrame;
        private double lastBirthCommandAt = -1.0;
        private float automaticShoreBirthAccumulator;
        private int automaticShoreBirthCursor;
        private int automaticShoreBirthSubmittedLastUpdate;
        private int automaticShoreBirthRejectedLastUpdate;
        private int automaticShoreBirthSubmittedTotal;
        private int activeAutomaticFoamSourceEventCount;
        private int automaticSourceEventsRasterizedLastUpdate;
        private string automaticShoreBirthStatus =
            "Automatic source population disabled";
        private StylizedRiverFoamMajorTopology majorTopology;
        private StylizedRiverFoamConnectorTopology connectorTopology;
        private StylizedRiverFoamPocketTopology pocketTopology;
        private int majorTopologyInputSignature = int.MinValue;
        private int connectorTopologyInputSignature = int.MinValue;
        private int pocketTopologyInputSignature = int.MinValue;
        private int majorEvolutionLifetimeInputSignature = int.MinValue;

        // Patch 4.9A keeps its proof telemetry entirely separate from the
        // active topology lifecycle. The explicit Inspector action serializes
        // immutable prepared data only and never replaces renderer bindings.
        private string topologyCacheRoundTripState = "Not Run";
        private string topologyCacheRoundTripSummary =
            "Run the explicit cache round-trip proof after initialization.";
        private int topologyCacheRoundTripPayloadBytes;
        private ulong topologyCacheRoundTripPayloadHash;
        private double topologyCacheRoundTripSerializationMilliseconds;
        private double topologyCacheRoundTripLoadMilliseconds;
        private double topologyCacheRoundTripVerificationMilliseconds;
        private int topologyCacheRoundTripRunCount;
        private int topologyCacheRoundTripPassCount;

        // Patch 4.9B keeps cache creation explicit. Patch 4.9C reuses the
        // stable-input diagnostics during staged startup, while renderer
        // ownership remains with the existing Foam runtime.
        private string topologyCacheBuildState = "Not Built";
        private string topologyCacheBuildSummary =
            "Build a cache explicitly after the prepared topology is Ready.";
        private int topologyCacheBuildCount;
        private int topologyCacheBuildSuccessCount;
        private int topologyCacheBuildPayloadBytes;
        private string topologyCacheBuildPayloadHash = "—";
        private double topologyCacheBuildMilliseconds;
        private string topologyCacheValidationState = "Not Evaluated";
        private string topologyCacheValidationSummary =
            "Assigned cache has not been checked against current stable inputs.";
        private int topologyCacheValidationCount;
        private int topologyCacheValidationHitCount;
        private string topologyCacheDomainFingerprint = "—";
        private string topologyCacheObstacleFingerprint = "—";
        private string topologyCacheGenerationFingerprint = "—";
        private string topologyCacheCombinedFingerprint = "—";
        private int topologyCacheObstacleSourceCount;

        // Patch 4.9C changes ordinary startup from generation-first to
        // cache-first. Patch 4.9C.1 keeps the strict production miss policy,
        // while Editor and Development builds automatically generate a missing
        // or stale topology and the Editor coordinator persists the completed,
        // round-trip-validated payload.
        private string topologyCacheStartupState = "Not Evaluated";
        private string topologyCacheStartupSummary =
            "Ordinary startup has not evaluated the assigned cache.";
        private int topologyCacheStartupAttemptCount;
        private int topologyCacheStartupHitCount;
        private int topologyCacheStartupMissCount;
        private int topologyCacheStartupPayloadBytes;
        private string topologyCacheStartupPayloadHash = "—";
        private double topologyCacheStartupLoadMilliseconds;
        private bool topologyCacheLoadedForActiveResources;
        private bool obstacleExclusionUsesCachedScalar;
        private bool activeTopologyObstacleStale;
        private bool explicitTopologyGenerationRequested;
        private bool explicitTopologyGenerationInProgress;
        private bool automaticTopologyGenerationInProgress;
        private double automaticDevelopmentRebuildNotBefore;
        private int automaticDevelopmentObservedSignature = int.MinValue;
        private AutomaticDevelopmentRebuildReason
            automaticDevelopmentRebuildReason;
        private bool pendingAutomaticDevelopmentRebuildAfterStartup;
        private bool pendingStartupCacheStaleObstacles;
        private bool pendingStartupCacheStaleSettings;
        private bool automaticTopologyCacheWritePending;
        private string automaticTopologyCachePersistenceState = "Idle";
        private string automaticTopologyCachePersistenceSummary =
            "No automatic development cache write is pending.";
        private int automaticTopologyCacheWriteCount;
        private int automaticTopologyCacheWriteSuccessCount;
        private string topologyCacheSessionPersistentInputKey = string.Empty;
        private bool topologyCacheSessionPersistentInputCaptured;
        private StylizedRiverFoamTopologyCachePackage
            pendingStartupCachePackage;

        // Patch 4.9D records decisive cold/warm startup evidence without
        // changing scheduling. Each AdvanceInitializationPhase call remains one
        // bounded staged step; the counters prove whether expensive preparation
        // executed and identify the slowest individual step.
        private bool topologyStartupValidationActive;
        private bool topologyStartupValidationComplete;
        private double topologyStartupValidationStartedAt;
        private double topologyStartupValidationTotalMilliseconds;
        private double topologyStartupValidationSlowestStepMilliseconds;
        private string topologyStartupValidationSlowestStep = "—";
        private int topologyStartupValidationStepCount;
        private int topologyStartupValidationCacheInstallCount;
        private int topologyStartupValidationObstacleBuildCount;
        private int topologyStartupValidationMajorBuildCount;
        private int topologyStartupValidationConnectorBuildCount;
        private int topologyStartupValidationPocketBuildCount;

        private readonly List<PendingInjection> pendingInjections = new();
        private readonly List<PendingInjection> pendingMaterialBirths = new();
        private readonly FoamCompositionEvent[] foamCompositionEvents =
            new FoamCompositionEvent[FoamCompositionEventCapacity];
        private readonly AutomaticFoamSourceEvent[] automaticFoamSourceEvents =
            new AutomaticFoamSourceEvent[AutomaticFoamSourceEventCapacity];
        private readonly FoamSourceEventGpuData[] automaticFoamSourceEventGpuData =
            new FoamSourceEventGpuData[AutomaticFoamSourceEventCapacity];
        private readonly uint[] progressiveBirthDebugCounterReadback =
            new uint[ProgressiveBirthDebugCounterCount];
        private readonly List<MeshFilter> obstacleExclusionMeshFilters = new();
        private readonly List<MeshFilter> topologyCacheFingerprintMeshFilters =
            new();
        private readonly List<GeneratedGeometryStableFingerprint>
            topologyCachePreparedObstacleFingerprints = new();
        private readonly List<RiverObstacleExclusionCell>
            obstacleExclusionCells = new();
        private readonly List<RiverObstacleExclusionSample>
            obstacleExclusionSamples = new();
        private FoamObstacleIntervalCellData[] obstacleExclusionGpuCells =
            Array.Empty<FoamObstacleIntervalCellData>();
        private float[] obstacleExclusionScalar = Array.Empty<float>();
        private Color[] topologyGeneratedUploadPixels = Array.Empty<Color>();
        private Color[] majorMaskUploadPixels = Array.Empty<Color>();
        private Color[] hostedNegativeMaskUploadPixels = Array.Empty<Color>();
        private Color[] freeWaterNegativeMaskUploadPixels = Array.Empty<Color>();
        private MajorEvolutionSlot[] majorEvolutionSlots =
            Array.Empty<MajorEvolutionSlot>();
        private FoamMajorEvolutionData[] majorEvolutionGpuData =
            Array.Empty<FoamMajorEvolutionData>();
        private HostedNegativeEvolutionSlot[] hostedNegativeEvolutionSlots =
            Array.Empty<HostedNegativeEvolutionSlot>();
        private FoamHostedNegativeEvolutionData[]
            hostedNegativeEvolutionGpuData =
                Array.Empty<FoamHostedNegativeEvolutionData>();
        private FreeWaterEvolutionSlot[] freeWaterEvolutionSlots =
            Array.Empty<FreeWaterEvolutionSlot>();
        private FoamFreeWaterEvolutionData[] freeWaterEvolutionGpuData =
            Array.Empty<FoamFreeWaterEvolutionData>();
        private FoamConnectorIdentityData[] connectorIdentityGpuData =
            Array.Empty<FoamConnectorIdentityData>();
        private Vector4[] connectorPathPointGpuData = Array.Empty<Vector4>();
        private FoamWeakSpanIdentityData[] weakSpanIdentityGpuData =
            Array.Empty<FoamWeakSpanIdentityData>();
        private ConnectorEvolutionSlot[] connectorEvolutionSlots =
            Array.Empty<ConnectorEvolutionSlot>();
        private ConnectorRelationshipCandidate[] connectorRelationshipCandidates =
            Array.Empty<ConnectorRelationshipCandidate>();
        private bool[] connectorCandidateClaimed = Array.Empty<bool>();
        private bool[] connectorMajorPairClaimed = Array.Empty<bool>();
        private int[] connectorMajorDegree = Array.Empty<int>();
        private int[] connectorPreviousMajorDegree = Array.Empty<int>();
        private int connectorEvolutionActiveCount;
        private int connectorEvolutionTemporaryAbsenceCount;
        private int connectorEvolutionIdentityPathCount;
        private int connectorEvolutionRecycleVariantCount;
        private int connectorEvolutionOriginalRelationshipCount;
        private int connectorEvolutionReplacementRelationshipCount;
        private int connectorEvolutionRelationshipRebindCount;
        private int connectorEvolutionVariantSwitchCount;
        private int connectorEvolutionStretchBreakCount;
        private int connectorEvolutionRetainDecisionCount;
        private int connectorEvolutionTurnoverRequestCount;
        private int connectorEvolutionSuccessfulTurnoverCount;
        private int connectorEvolutionNoAlternativeFallbackCount;
        private int connectorEvolutionCrowdingBoostedTurnoverCount;
        private int connectorEvolutionMajorDegreeZeroCount;
        private int connectorEvolutionMajorDegreeOneCount;
        private int connectorEvolutionMajorDegreeTwoCount;
        private int connectorEvolutionMajorDegreeThreePlusCount;
        private int connectorEvolutionMaximumMajorDegree;
        private int connectorEvolutionAbsenceEventCount;
        private int connectorEvolutionReappearanceCount;
        private int weakSpanEvolutionActiveCount;
        private float majorEvolutionAccumulator;
        private float freeWaterEvolutionAccumulator;
        private bool majorEvolutionReady;
        private bool hostedNegativeEvolutionReady;
        private bool freeWaterEvolutionReady;
        private bool connectorIdentityReconstructionReady;
        private bool weakSpanIdentityReconstructionReady;
        private int majorEvolutionReconstructionTicks;
        private int hostedNegativeLocalChangeCount;
        private int freeWaterMoveCount;
        private int freeWaterRecycleCount;
        private int freeWaterUpstreamViolationCount;
        private float freeWaterObservedMinimumDwell = float.PositiveInfinity;
        private float freeWaterObservedMaximumDwell;
        private float freeWaterObservedMinimumMove = float.PositiveInfinity;
        private float freeWaterObservedMaximumMove;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private bool hostedNegativeInitialParityPending;
        private bool hostedNegativeInitialParityReadbackPending;
        private bool hostedNegativeInitialParityAvailable;
        private float hostedNegativeInitialParityMeanDifference;
        private float hostedNegativeInitialParityMaximumDifference;
        private int hostedNegativeInitialParityGeneration;
        private bool connectorIdentityParityPending;
        private bool connectorIdentityParityReadbackPending;
        private bool connectorIdentityParityAvailable;
        private float connectorIdentityParityMeanDifference;
        private float connectorIdentityParityMaximumDifference;
        private int connectorIdentityParityGeneration;
        private bool weakSpanIdentityParityPending;
        private bool weakSpanIdentityParityReadbackPending;
        private bool weakSpanIdentityParityAvailable;
        private float weakSpanIdentityParityMeanDifference;
        private float weakSpanIdentityParityMaximumDifference;
        private int weakSpanIdentityParityGeneration;
#endif
        private int majorEvolutionRecycleCount;
        private int majorEvolutionCrowdedRecycleFallbackCount;
        private int majorEvolutionUpstreamViolations;
        private float majorEvolutionObservedMinimumDwell = float.PositiveInfinity;
        private float majorEvolutionObservedMaximumDwell;
        private float majorEvolutionObservedMinimumMove = float.PositiveInfinity;
        private float majorEvolutionObservedMaximumMove;
        private double majorEvolutionLastCpuMilliseconds;
        private long majorEvolutionLastAllocatedBytes;

        private bool resourcesDirty = true;
        private bool boundaryDirty = true;
        private InitializationPhase initializationPhase =
            InitializationPhase.NotStarted;
        private RebuildPhase rebuildPhase = RebuildPhase.Idle;
        private bool pendingBoundaryRebuild;
        private bool pendingObstacleRebuild;
        private bool pendingTopologyRefresh;
        private bool pendingTopologyReplacementAfterMaintenance;
        private int pendingObstacleObservedVersion = int.MinValue;
        private int pendingObstacleStableFrameCount;
        private int initializationObstacleObservedVersion = int.MinValue;
        private int initializationObstacleStableFrameCount;
        private TopologyReplacementBuild topologyReplacementBuild;
        private TopologyReplacementPhase topologyReplacementPhase =
            TopologyReplacementPhase.Idle;
        private int topologyReplacementRequestSequence;
        private int topologyReplacementRequestCount;
        private int topologyReplacementCoalescedCount;
        private int topologyReplacementCancelledCount;
        private int topologyReplacementActivatedCount;
        private int topologyReplacementIdenticalPreparedCount;
        private string topologyReplacementLastReason = "None";
        private TopologyTransitionSnapshot topologyTransitionSnapshot;
        private RenderTexture retiredTopologyTransitionTexture;
        private ComputeBuffer retiredTopologyTransitionMetricBuffer;
        private int retiredTopologyTransitionReleaseFrame = -1;
        private RenderTexture retiredGeneratedTopologyTexture;
        private int retiredGeneratedTopologyReleaseFrame = -1;
        private float topologyTransitionElapsed;
        private int topologyTransitionStartedCount;
        private int topologyTransitionCompletedCount;
        private int topologyTransitionRemappedCount;
        private int topologyTransitionFlattenedCount;
        private bool supportWarningReported;
        private bool allocationWarningReported;
        private bool fullyFrozenLastUpdate;
        private int domainVersion = -1;
        private int fieldWidth;
        private int fieldHeight;
        private int filmFieldWidth;
        private int filmFieldHeight;
        private int chunkCount;
        private int resolutionPerChunk;
        private int structuralWidth;
        private int structuralHeight;
        private float fieldLength;
        private float validFieldLength;
        private float simulationFieldLength;
        private float minimumTransportLongitudinalSpacing;
        private float allocatedGlobalStart;
        private float simulationAccumulator;
        private float topologyMetricsAccumulator;
        private float simulationInterpolation = 1f;
        private float foamPhaseTransportMetres;
        private float foamRenderTravelMetres;
        private float lastMaterialStepDuration;
        private int lastMaterialStepsThisFrame;
        private float lastRenderInterpolationAlpha = 1f;
        private float lastFoamPhaseTransportMetres;
        private float lastFoamPhaseCellFraction;
        private float lastMotionLaneScrollCells;
        private int lastMotionLaneSignature = int.MinValue;
        private int lastObstacleRoutingSignature = int.MinValue;
        private int lastPhaseCommitCellsThisFrame;
        private int lastPhaseCommitCellsThisSecond;
        private int phaseCommitCellsInCurrentSecond;
        private float phaseCommitCounterAccumulator;
        private float lastFoamRenderTravelMetres;
        private float lastEstimatedTransportCellsPerStep;
        private float initializationMotionTime;
        private float lastRuntimeTime;
        private float lastConfiguredFoamDeltaTime;
        private float lastConfiguredFoamNeutralLifetime = 4f;
        private float lastConfiguredFoamPositiveAgeMultiplier = 1f;
        private float lastConfiguredFoamNegativeAgeMultiplier = 1f;
        private bool lastConfiguredAbsoluteLifeProbeActive;
        private double idleSince;
        private int clearKernel = -1;
        private int injectKernel = -1;
        private int rasterizeFoamSourceEventKernel = -1;
        private int writeIsolatedLifeProbeKernel = -1;
        private int clearProgressiveBirthDebugAllKernel = -1;
        private int clearProgressiveBirthDebugTransientKernel = -1;
        private int paintProgressiveBirthDebugSegmentKernel = -1;
        private int buildCurrentShoreEdgesKernel = -1;
        private int composeTopologyKernel = -1;
        private int captureGeneratedTopologyKernel = -1;
        private int buildEvolvingMajorSupportKernel = -1;
        private int clearObstacleExclusionKernel = -1;
        private int updateObstacleExclusionKernel = -1;
        private int resetTopologyMetricsKernel = -1;
        private int measureTopologyMetricsKernel = -1;
        private int phaseCommitKernel = -1;
        private int simulateKernel = -1;
        private int buildFilmSourceKernel = -1;
        private int buildFilmSupportKernel = -1;
        private int evaluateShapeKernel = -1;
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
        private bool pendingIsolatedLifeProbe;
        private bool pendingIsolatedLifeProbeAbsoluteAging;
        private bool isolatedLifeProbeAbsoluteAgingActive;
        private double isolatedLifeProbeWrittenAt = -1.0;
        private float pendingIsolatedLifeProbeDistanceNormalized = 0.5f;
        private float pendingIsolatedLifeProbeAcrossNormalized;
        private string isolatedLifeProbeStatus =
            "Not emitted this session";
        private int foamCompositionSequence;
        private int activeFoamCompositionEventCount;
        private int foamCompositionScanCursor;
        private int foamCompositionStartedCount;
        private int foamCompositionCompletedCount;
        private int foamCompositionRejectedCount;
        private float latestFoamCompositionProgress;
        private float latestFoamCompositionHeadDistanceNormalized;
        private float latestFoamCompositionHeadAcrossNormalized;
        private float latestFoamCompositionPreviousDistanceNormalized;
        private float latestFoamCompositionPreviousAcrossNormalized;
        private float lastFoamCompositionSegmentLength;
        private int latestFoamCompositionEventId;
        private int foamCompositionEventUpdateCount;
        private int foamCompositionSegmentDispatchAttemptCount;
        private int foamCompositionSegmentDispatchSubmittedCount;
        private float foamCompositionCumulativeCentrelineDistance;
        private bool progressiveBirthDebugResetPending;
        private bool progressiveBirthDebugReadbackPending;
        private bool progressiveBirthDebugReadbackAvailable;
        private int progressiveBirthDebugResourceGeneration;
        private int progressiveBirthDebugSessionGeneration;
        private uint progressiveBirthDebugLatestAffectedTexels;
        private uint progressiveBirthDebugCumulativeAffectedTexels;

        private bool HasQueuedRebuildWork =>
            rebuildPhase != RebuildPhase.Idle ||
            pendingBoundaryRebuild ||
            pendingObstacleRebuild ||
            pendingTopologyRefresh;

        private bool HasTopologyTransitionVisibleHold =>
            topologyTransitionSnapshot != null &&
            topologyTransitionSnapshot.HoldsVisibleResources;
    }
}
