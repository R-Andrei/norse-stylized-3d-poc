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
        private RenderTexture presentationPreviousState;
        private RenderTexture automaticBirthDebugTexture;
        private RenderTexture previousState;
        private RenderTexture currentState;
        private RenderTexture writeState;
        private RenderTexture shapeMaskTexture;
        private RenderTexture filmSourceTexture;
        private RenderTexture filmSupportTexture;
        private RenderTexture visualOccupancyA;
        private RenderTexture visualOccupancyB;
        private RenderTexture currentVisualOccupancy;
        private RenderTexture writeVisualOccupancy;
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
        private RenderTexture objectContactFieldTexture;
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
        private ComputeBuffer transportMetricsBuffer;
        private ComputeBuffer automaticBirthDebugCounterBuffer;
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
        private readonly uint[] latestTransportMetrics =
            new uint[TransportMetricCount];
        private bool topologyMetricsReadbackPending;
        private bool topologyMetricsAvailable;
        private double topologyMetricsLastCompletedAt = -1.0;
        private double topologyMetricsReadbackRequestedAt = -1.0;
        private int topologyMetricsGeneration;
        private bool transportMetricsReadbackPending;
        private bool transportMetricsAvailable;
        private double transportMetricsReadbackRequestedAt = -1.0;
        private double transportMetricsLastCompletedAt = -1.0;
        private int transportMetricsGeneration;
        private float transportMetricsAccumulator;
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
        private int automaticShoreBirthCursor;
        private int automaticShoreBirthSubmittedLastUpdate;
        private int automaticShoreBirthRejectedLastUpdate;
        private int automaticShoreBirthSubmittedTotal;
        private float automaticObjectBirthAccumulator;
        private float automaticObjectContactCycleTime;
        private int automaticObjectPatternAuthoritySignature = int.MinValue;
        private int automaticObjectClearanceAuthoritySignature = int.MinValue;
        private int automaticObjectReinforcementAuthoritySignature = int.MinValue;
        private int automaticObjectBirthCursor;
        private int automaticObjectBirthSubmittedLastUpdate;
        private int automaticObjectBirthRejectedLastUpdate;
        private int automaticObjectBirthSubmittedTotal;
        private int automaticObjectBirthAnchorCountLastUpdate;
        private int automaticObjectContactBuildCount;
        private int automaticObjectContactReinforcementCount;
        private int automaticObjectContactFleckCount;
        private int automaticObjectWaitingClearanceCount;
        private float automaticFreeWaterBirthAccumulator;
        private int automaticFreeWaterBirthCursor;
        private int automaticFreeWaterBirthSubmittedLastUpdate;
        private int automaticFreeWaterBirthRejectedLastUpdate;
        private int automaticFreeWaterBirthSubmittedTotal;
        private int automaticPacketEnvelopeRejectedLastUpdate;
        private int automaticPacketEnvelopeRejectedTotal;
        private int automaticPacketReservationActiveCount;
        private int activeAutomaticFoamSourceEventCount;
        private int activeAutomaticShoreSourceEventCount;
        private float automaticShorePopulationMeanHeadCount;
        private int automaticShorePopulationMinimumHeadCount;
        private int automaticShorePopulationMaximumHeadCount;
        private int automaticShorePopulationTargetHeadCount;
        private float automaticShorePopulationActiveBankLengthMetres;
        private int automaticShorePopulationEpochIndex;
        private float automaticShorePopulationNextBoundaryTime = -1f;
        private int automaticShorePopulationAuthoritySignature = int.MinValue;
        private bool automaticShorePopulationTargetRefreshPending = true;
        private int automaticSourceEventsRasterizedLastUpdate;
        private string automaticShoreBirthStatus =
            "Automatic source population disabled";
        private string automaticObjectBirthStatus =
            "Object source population disabled";
        private string automaticFreeWaterBirthStatus =
            "Free Water source population disabled";
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
        private int topologyCacheLastBuildSerializationCount;
        private int topologyCachePreparationGeneratedUploadCount;
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

        // P1 makes ordinary startup cache-only. Exact and stale-compatible
        // payloads may install for the session, while missing or incompatible
        // payloads stop in a stable preparation-required state. Topology
        // generation and cache persistence are explicit Edit Mode work.
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
        private bool explicitTopologyGenerationInProgress;
        private bool pendingStartupCacheStaleObstacles;
        private bool pendingStartupCacheStaleSettings;
        private bool automaticTopologyCacheWritePending;
        private bool editorTopologyPreparationInProgress;
        private bool activeTopologyRequiresExplicitPreparation;
        private bool generatedSourceNotificationBurstPending;
        private TopologyCacheStartupOutcome topologyCacheStartupOutcome;
        private TopologyCacheStartupReason topologyCacheStartupReasons;
        private string automaticTopologyCachePersistenceState = "Disabled";
        private string automaticTopologyCachePersistenceSummary =
            "Play Mode cache persistence is disabled. Prepare the cache explicitly in Edit Mode.";
        private int automaticTopologyCacheWriteCount;
        private int automaticTopologyCacheWriteSuccessCount;
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
        private int[] topologyStartupPhaseCallCounts =
            Array.Empty<int>();
        private double[] topologyStartupPhaseAccumulatedMilliseconds =
            Array.Empty<double>();
        private int topologyStartupSourceAddedCount;
        private int topologyStartupSourceRemovedCount;
        private int topologyStartupSourceChangedCount;
        private int topologyStartupDirtyCycleCount;
        private int topologyStartupRestartCount;
        private int topologyStartupCacheBuildAttemptCount;
        private int topologyStartupReplacementAttemptCount;
        private int topologyStartupCacheWriteAttemptCount;
        private int topologyStartupCacheWriteSuccessCount;
        private TopologyStartupDirtyReason topologyStartupDirtyReasons;
        private readonly HashSet<IGeneratedGeometrySource>
            topologyStartupDistinctGeneratedSources = new();
        private bool topologyStartupSummaryLogged;

        private int automaticFoamSourceEventCapacity;
        private int automaticFoamPacketReservationCapacity;
        private AutomaticFoamSourceEvent[] automaticFoamSourceEvents =
            Array.Empty<AutomaticFoamSourceEvent>();
        private AutomaticFoamPacketReservation[] automaticFoamPacketReservations =
            Array.Empty<AutomaticFoamPacketReservation>();
        private FoamSourceEventGpuData[] automaticFoamSourceEventGpuData =
            Array.Empty<FoamSourceEventGpuData>();
        private readonly AutomaticRevealTimingTelemetry[]
            automaticRevealTimingByType =
                new AutomaticRevealTimingTelemetry[9];
        private readonly uint[] automaticBirthDebugCounterReadback =
            new uint[AutomaticBirthDebugCounterCount];
        private readonly List<RiverFoamStaticObjectSource>
            automaticObjectFoamSources = new();
        private readonly Dictionary<EntityId, AutomaticObjectSourceState>
            automaticObjectSourceStates = new();
        private readonly Dictionary<int, AutomaticShoreSlotScheduleState>
            automaticShoreSlotSchedules = new();
        private readonly Dictionary<int, float>
            automaticFreeWaterSlotNextStartTimes = new();
        private readonly HashSet<EntityId> automaticObjectContactLiveSourceIds = new();
        private readonly List<EntityId> automaticObjectContactStaleSourceIds = new();
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
        private int persistentStateReplacementExactCount;
        private int persistentStateReplacementClearCount;
        private PersistentStateReplacementPolicy lastPersistentStateReplacementPolicy;
        private string lastPersistentStateReplacementDetail = "None";
        private bool supportWarningReported;
        private bool allocationWarningReported;
        private bool fullyFrozenLastUpdate;
        private bool shapeProductDebugActiveLastUpdate;
        private int domainVersion = -1;
        private int fieldWidth;
        private int fieldHeight;
        private int filmFieldWidth;
        private int filmFieldHeight;
        private int chunkCount;
        private int resolutionPerChunk;
        private StylizedRiverFoamGridDescriptor gridDescriptor;
        private StylizedRiverFoamGridGpuData gridDescriptorGpuData;
        private StylizedRiverFoamGridDescriptor fixedMetricCandidateDescriptor;
        private string fixedMetricCandidateFailureReason = "Not resolved";
        private int structuralWidth;
        private int structuralHeight;
        private float fieldLength;
        private float validFieldLength;
        private float simulationFieldLength;
        private float minimumTransportLongitudinalSpacing;
        private float minimumTransportLateralSpacing;
        private float minimumTransportCurvatureJacobian = 1f;
        private float minimumTransportRawJacobian = 1f;
        private float maximumAbsoluteTransportCurvatureLateralProduct;
        private float allocatedGlobalStart;
        private float simulationAccumulator;
        private float topologyMetricsAccumulator;
        private float simulationInterpolation = 1f;
        private float lastMaterialStepDuration;
        private int lastMaterialStepsThisFrame;
        private float lastRenderInterpolationAlpha = 1f;
        private float lastMotionLaneScrollCells;
        private float lastMotionLaneMinimumIntent;
        private float lastMotionLaneMaximumIntent;
        private float lastMotionLaneMeanAbsoluteIntent;
        private float lastMotionLaneRootMeanSquareIntent;
        private float lastMotionLanePositiveFraction;
        private float lastMotionLaneNegativeFraction;
        private float lastMotionLaneNearNeutralFraction;
        private float lastMotionLaneMeanAbsoluteFaceIntent;
        private float lastMotionLaneRootMeanSquareFaceIntent;
        private float lastMotionLaneOpposingFaceFraction;
        private float lastMotionLaneFaceCancellationRatio;
        private int lastMotionLaneSignature = int.MinValue;
        private int lastObstacleRoutingSignature = int.MinValue;
        private float lastEstimatedTransportCellsPerStep;
        private float lastEstimatedLateralTransportCellsPerStep;
        private float lastDownstreamTransportCfl;
        private float lastLateralTransportCfl;
        private float lastMaximumTransportCfl;
        private int lastRequiredTransportSubsteps = 1;
        private int lastUsedTransportSubsteps = 1;
        private bool transportSafetyLimitExceeded;
        private string transportSafetyStatus = "CFL transport ready";
        private float transportPresenceBefore;
        private float transportPresenceAfter;
        private float transportLifeMomentBefore;
        private float transportLifeMomentAfter;
        private float transportPatternMomentBefore;
        private float transportPatternMomentAfter;
        private float transportPresenceBoundaryOutflow;
        private float transportLifeBoundaryOutflow;
        private float transportPatternBoundaryOutflow;
        private float transportPresenceClampLoss;
        private float transportLifeClampLoss;
        private float transportPatternClampLoss;
        private float transportPresenceUnitCapacityLoss;
        private float transportPresenceBoundaryCapacityLoss;
        private float transportPresenceObstacleCapacityLoss;
        private float transportPresenceStateValidityLoss;
        private float transportPresenceMinimumCutoffLoss;
        private float transportPresenceAttributionResidual;
        private float transportMaximumRawPresence;
        private float transportMaximumLocalCapacityExcess;
        private uint transportTotalCapacityHitCount;
        private uint transportUnitCapacityHitCount;
        private uint transportBoundaryCapacityHitCount;
        private uint transportObstacleCapacityHitCount;
        private float transportLateralWeightedSpeedNumerator;
        private float transportLateralWeightedSpeedWeight;
        private float transportLateralMaterialWeightedSpeed;
        private float transportLateralPositiveMovement;
        private float transportLateralNegativeMovement;
        private float transportPresenceUnaccountedError;
        private float transportLifeUnaccountedError;
        private float transportPatternUnaccountedError;
        private float transportPresenceUnaccountedErrorRatio;
        private float transportLifeUnaccountedErrorRatio;
        private float transportPatternUnaccountedErrorRatio;
        private float transportPresenceClampLossRatio;
        private float initializationMotionTime;
        private float lastRuntimeTime;
        private float lastConfiguredFoamDeltaTime;
        private float lastConfiguredFoamNeutralLifetime = 4f;
        private float lastConfiguredFoamPositiveAgeMultiplier = 1f;
        private float lastConfiguredFoamNegativeAgeMultiplier = 1f;
        private bool lastConfiguredAbsoluteLifeProbeActive;
        private double idleSince;
        private int clearKernel = -1;
        private int rasterizeFoamSourceEventKernel = -1;
        private int rasterizeFoamSourceEventDebugKernel = -1;
        private int writeIsolatedLifeProbeKernel = -1;
        private int clearAutomaticBirthDebugAllKernel = -1;
        private int buildCurrentShoreEdgesKernel = -1;
        private int composeTopologyKernel = -1;
        private int captureGeneratedTopologyKernel = -1;
        private int buildEvolvingMajorSupportKernel = -1;
        private int clearObstacleExclusionKernel = -1;
        private int updateObstacleExclusionKernel = -1;
        private int buildObjectContactFieldKernel = -1;
        private int resetTopologyMetricsKernel = -1;
        private int measureTopologyMetricsKernel = -1;
        private int resetTransportMetricsKernel = -1;
        private int remapPersistentStateKernel = -1;
        private int simulateKernel = -1;
        private StylizedRiverFoamMaterialContract activeMaterialContract =
            StylizedRiverFoamMaterialContract.CoveragePresenceLifeBaseline;
        private bool materialContractInitialized;
        private float bulkTransportPhaseCells;
        private float previousBulkTransportPhaseCells;
        private int bulkTransportIntegerShift;
        private int buildFilmSourceKernel = -1;
        private int buildFilmSupportKernel = -1;
        private int advanceVisualOccupancyKernel = -1;
        private int evaluateShapeKernel = -1;
        private int applyBoundaryKernel = -1;
        private int obstacleGeometryVersion = -1;
        private StylizedRiverQuality allocatedQuality;
        private StylizedRiverFoamGridMode allocatedFoamGridMode;
        private float allocatedFoamRequestedCellSizeMetres;

        private int lastUpdateDispatches;
        private int recentPeakDispatches;
        private long lastUpdateCellIterations;
        private long recentPeakCellIterations;
        private double recentPeakWindowEnd;

        // P4 records steady-state River Foam work without changing scheduling.
        // The accounting window is reset explicitly or when Play startup begins;
        // it never gates simulation, topology evolution, or diagnostics.
        private bool steadyStateWorkAccountingActive;
        private int steadyStateWorkAccountingGeneration;
        private int steadyStateWorkTopologyMetricGeneration;
        private int steadyStateWorkTransportMetricGeneration;
        private double steadyStateWorkAccountingStartedAt;
        private long steadyStateWorkFrameCount;
        private long steadyStateWorkVisibleFrameCount;
        private long steadyStateWorkOffscreenFrameCount;
        private long steadyStateWorkHeldFrameCount;
        private long steadyStateWorkMaterialFrameCount;
        private long steadyStateWorkTotalDispatchCount;
        private long steadyStateWorkTotalCellIterations;
        private long steadyStateWorkMaterialStepCount;
        private long steadyStateWorkTransportSubstepCount;
        private int steadyStateWorkMaximumTransportSubsteps;
        private float steadyStateWorkMaximumTransportCfl;
        private long steadyStateWorkRenderInterpolationSampleCount;
        private float steadyStateWorkRenderInterpolationMinimum = 1f;
        private float steadyStateWorkRenderInterpolationMaximum;
        private long steadyStateWorkMaterialDispatchCount;
        private long steadyStateWorkMaterialCellIterations;
        private double steadyStateWorkMaterialCpuMilliseconds;
        private long steadyStateWorkEmptyMaterialStepCount;
        private long steadyStateWorkTopologyDirtyEvaluationCount;
        private long steadyStateWorkTopologyDirtyPositiveCount;
        private long steadyStateWorkTopologyMaintenanceCount;
        private long steadyStateWorkTopologyEvolutionCount;
        private double steadyStateWorkTopologyEvolutionCpuMilliseconds;
        private long steadyStateWorkTopologyRefreshCount;
        private long steadyStateWorkTopologyDispatchCount;
        private long steadyStateWorkTopologyCellIterations;
        private double steadyStateWorkTopologyCpuMilliseconds;
        private long steadyStateWorkShapeEvaluationCount;
        private long steadyStateWorkShapeDispatchCount;
        private long steadyStateWorkShapeCellIterations;
        private double steadyStateWorkShapeCpuMilliseconds;
        private long steadyStateWorkTopologyMetricRequestCount;
        private long steadyStateWorkTopologyMetricCompletionCount;
        private long steadyStateWorkTopologyMetricErrorCount;
        private long steadyStateWorkTopologyMetricTimeoutCount;
        private long steadyStateWorkTransportMetricRequestCount;
        private long steadyStateWorkTransportMetricCompletionCount;
        private long steadyStateWorkTransportMetricErrorCount;

        private int injectedLastUpdate;
        private float lastInjectionBoundaryCoverage = -1f;
        private bool lastInjectionStateSynchronized;
        private bool pendingIsolatedLifeProbe;
        private bool pendingIsolatedLifeProbeAbsoluteAging;
        private bool isolatedLifeProbeAbsoluteAgingActive;
        private double isolatedLifeProbeWrittenAt = -1.0;
        private float pendingIsolatedLifeProbeDistanceNormalized = 0.5f;
        private float pendingIsolatedLifeProbeAcrossNormalized;
        private string isolatedLifeProbeStatus =
            "Not emitted this session";
        private float latestFoamCompositionProgress;
        private float latestFoamCompositionHeadDistanceNormalized;
        private float latestFoamCompositionHeadAcrossNormalized;
        private float latestFoamCompositionPreviousDistanceNormalized;
        private float latestFoamCompositionPreviousAcrossNormalized;
        private float lastFoamCompositionSegmentLength;
        private int latestFoamCompositionEventId;
        private int foamCompositionSequence;
        private int foamCompositionStartedCount;
        private int foamCompositionRejectedCount;
        private int foamCompositionCompletedCount;
        private bool automaticBirthDebugActiveLastUpdate;
        private bool automaticBirthDebugReadbackPending;
        private bool automaticBirthDebugReadbackAvailable;
        private int automaticBirthDebugResourceGeneration;
        private int automaticBirthDebugSessionGeneration;
        private uint automaticBirthDebugLiveAffectedTexels;

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
