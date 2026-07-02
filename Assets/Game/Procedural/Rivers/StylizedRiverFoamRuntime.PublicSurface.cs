using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
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
        public bool MajorEvolutionAvailable => majorEvolutionReady;
        public int MajorEvolutionSlotCount => majorEvolutionSlots.Length;
        public int MajorEvolutionMovingCount => CountMovingMajorSlots();
        public int MajorEvolutionDwellingCount =>
            Mathf.Max(0, majorEvolutionSlots.Length - CountMovingMajorSlots());
        public float MajorEvolutionMinimumDwell =>
            float.IsPositiveInfinity(majorEvolutionObservedMinimumDwell)
                ? 0f
                : majorEvolutionObservedMinimumDwell;
        public float MajorEvolutionMaximumDwell =>
            majorEvolutionObservedMaximumDwell;
        public float MajorEvolutionMinimumMove =>
            float.IsPositiveInfinity(majorEvolutionObservedMinimumMove)
                ? 0f
                : majorEvolutionObservedMinimumMove;
        public float MajorEvolutionMaximumMove =>
            majorEvolutionObservedMaximumMove;
        public int MajorEvolutionReconstructionTicks =>
            majorEvolutionReconstructionTicks;
        public int MajorEvolutionRecycleCount => majorEvolutionRecycleCount;
        public int MajorEvolutionCrowdedRecycleFallbackCount =>
            majorEvolutionCrowdedRecycleFallbackCount;
        public int MajorEvolutionUpstreamViolations =>
            majorEvolutionUpstreamViolations;
        public int MajorPreparedRecycleAnchorCount => majorTopology != null
            ? majorTopology.PreparedRecycleAnchorCount
            : 0;
        public int MajorRecycleFallbackCount => majorTopology != null
            ? majorTopology.RecycleFallbackCount
            : 0;
        public double MajorEvolutionLastCpuMilliseconds =>
            majorEvolutionLastCpuMilliseconds;
        public long MajorEvolutionLastAllocatedBytes =>
            majorEvolutionLastAllocatedBytes;
        public bool HostedNegativeEvolutionAvailable =>
            hostedNegativeEvolutionReady;
        public int HostedNegativeEvolutionSlotCount =>
            hostedNegativeEvolutionSlots.Length;
        public int HostedNegativeInteriorCount =>
            CountHostedNegativeSlots(
                StylizedRiverFoamNegativeRegionClass.InteriorPocket);
        public int HostedNegativeCavityCount =>
            CountHostedNegativeSlots(
                StylizedRiverFoamNegativeRegionClass.EdgeCavity);
        public int HostedNegativeLocalChangeCount =>
            hostedNegativeLocalChangeCount;
        public int HostedNegativeAcceptedCount => pocketTopology != null
            ? pocketTopology.AcceptedHostedRegionCount
            : 0;
        public int HostedNegativePreparedCount => pocketTopology != null
            ? pocketTopology.PreparedHostedRegionCount
            : 0;
        public int HostedNegativeFallbackCount => pocketTopology != null
            ? pocketTopology.HostedFallbackRegionCount
            : 0;
        public bool FreeWaterEvolutionAvailable => freeWaterEvolutionReady;
        public int FreeWaterEvolutionSlotCount =>
            freeWaterEvolutionSlots.Length;
        public int FreeWaterPreparedCount => pocketTopology != null
            ? pocketTopology.PreparedFreeWaterRegionCount
            : 0;
        public int FreeWaterPreparedRecycleAnchorCount =>
            pocketTopology != null
                ? pocketTopology.PreparedFreeWaterRecycleAnchorCount
                : 0;
        public int FreeWaterRecycleFallbackCount => pocketTopology != null
            ? pocketTopology.FreeWaterRecycleFallbackCount
            : 0;
        public int FreeWaterEvolutionMovingCount =>
            CountMovingFreeWaterSlots();
        public int FreeWaterEvolutionDwellingCount =>
            Mathf.Max(0, freeWaterEvolutionSlots.Length -
                CountMovingFreeWaterSlots());
        public float FreeWaterEvolutionMinimumDwell =>
            float.IsPositiveInfinity(freeWaterObservedMinimumDwell)
                ? 0f
                : freeWaterObservedMinimumDwell;
        public float FreeWaterEvolutionMaximumDwell =>
            freeWaterObservedMaximumDwell;
        public float FreeWaterEvolutionMinimumMove =>
            float.IsPositiveInfinity(freeWaterObservedMinimumMove)
                ? 0f
                : freeWaterObservedMinimumMove;
        public float FreeWaterEvolutionMaximumMove =>
            freeWaterObservedMaximumMove;
        public int FreeWaterMoveCount => freeWaterMoveCount;
        public int FreeWaterRecycleCount => freeWaterRecycleCount;
        public int FreeWaterUpstreamViolationCount =>
            freeWaterUpstreamViolationCount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public bool HostedNegativeInitialParityAvailable =>
            hostedNegativeInitialParityAvailable;
        public bool HostedNegativeInitialParityPending =>
            hostedNegativeInitialParityPending ||
            hostedNegativeInitialParityReadbackPending;
        public float HostedNegativeInitialParityMeanDifference =>
            hostedNegativeInitialParityMeanDifference;
        public float HostedNegativeInitialParityMaximumDifference =>
            hostedNegativeInitialParityMaximumDifference;
#endif
        public bool ConnectorIdentityReconstructionAvailable =>
            connectorIdentityReconstructionReady;
        public bool WeakSpanIdentityReconstructionAvailable =>
            weakSpanIdentityReconstructionReady;
        public bool ConnectorEvolutionAvailable =>
            majorEvolutionReady &&
            connectorIdentityReconstructionReady &&
            connectorEvolutionSlots.Length == connectorIdentityGpuData.Length &&
            connectorRelationshipCandidates.Length >=
                connectorEvolutionSlots.Length;
        public int ConnectorEvolutionActiveCount =>
            connectorEvolutionActiveCount;
        public int ConnectorEvolutionTemporaryAbsenceCount =>
            connectorEvolutionTemporaryAbsenceCount;
        public int ConnectorEvolutionIdentityPathCount =>
            connectorEvolutionIdentityPathCount;
        public int ConnectorEvolutionRecycleVariantCount =>
            connectorEvolutionRecycleVariantCount;
        public int ConnectorEvolutionOriginalRelationshipCount =>
            connectorEvolutionOriginalRelationshipCount;
        public int ConnectorEvolutionReplacementRelationshipCount =>
            connectorEvolutionReplacementRelationshipCount;
        public int ConnectorEvolutionRelationshipRebindCount =>
            connectorEvolutionRelationshipRebindCount;
        public int ConnectorEvolutionVariantSwitchCount =>
            connectorEvolutionVariantSwitchCount;
        public int ConnectorEvolutionStretchBreakCount =>
            connectorEvolutionStretchBreakCount;
        public int ConnectorEvolutionRetainDecisionCount =>
            connectorEvolutionRetainDecisionCount;
        public int ConnectorEvolutionTurnoverRequestCount =>
            connectorEvolutionTurnoverRequestCount;
        public int ConnectorEvolutionSuccessfulTurnoverCount =>
            connectorEvolutionSuccessfulTurnoverCount;
        public int ConnectorEvolutionNoAlternativeFallbackCount =>
            connectorEvolutionNoAlternativeFallbackCount;
        public int ConnectorEvolutionCrowdingBoostedTurnoverCount =>
            connectorEvolutionCrowdingBoostedTurnoverCount;
        public int ConnectorEvolutionMajorDegreeZeroCount =>
            connectorEvolutionMajorDegreeZeroCount;
        public int ConnectorEvolutionMajorDegreeOneCount =>
            connectorEvolutionMajorDegreeOneCount;
        public int ConnectorEvolutionMajorDegreeTwoCount =>
            connectorEvolutionMajorDegreeTwoCount;
        public int ConnectorEvolutionMajorDegreeThreePlusCount =>
            connectorEvolutionMajorDegreeThreePlusCount;
        public int ConnectorEvolutionMaximumMajorDegree =>
            connectorEvolutionMaximumMajorDegree;
        public int ConnectorEvolutionAbsenceEventCount =>
            connectorEvolutionAbsenceEventCount;
        public int ConnectorEvolutionReappearanceCount =>
            connectorEvolutionReappearanceCount;
        public int WeakSpanEvolutionActiveCount =>
            weakSpanEvolutionActiveCount;
        public int WeakSpanEvolutionTemporaryAbsenceCount => Mathf.Max(
            0,
            weakSpanIdentityGpuData.Length - weakSpanEvolutionActiveCount);
        public int ConnectorIdentityRecordCount => connectorIdentityGpuData.Length;
        public int ConnectorIdentityPathPointCount =>
            connectorPathPointGpuData.Length;
        public int WeakSpanIdentityRecordCount => weakSpanIdentityGpuData.Length;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public bool ConnectorIdentityParityAvailable =>
            connectorIdentityParityAvailable;
        public bool ConnectorIdentityParityPending =>
            connectorIdentityParityPending ||
            connectorIdentityParityReadbackPending;
        public float ConnectorIdentityParityMeanDifference =>
            connectorIdentityParityMeanDifference;
        public float ConnectorIdentityParityMaximumDifference =>
            connectorIdentityParityMaximumDifference;
        public bool WeakSpanIdentityParityAvailable =>
            weakSpanIdentityParityAvailable;
        public bool WeakSpanIdentityParityPending =>
            weakSpanIdentityParityPending ||
            weakSpanIdentityParityReadbackPending;
        public float WeakSpanIdentityParityMeanDifference =>
            weakSpanIdentityParityMeanDifference;
        public float WeakSpanIdentityParityMaximumDifference =>
            weakSpanIdentityParityMaximumDifference;
#endif
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
        public int ConnectorPreparedCount => connectorTopology != null
            ? connectorTopology.PreparedConnectorCount
            : 0;
        public int ConnectorUnavailableCount => connectorTopology != null
            ? connectorTopology.UnavailableConnectorCount
            : 0;
        public int ConnectorPreparedEndpointCount => connectorTopology != null
            ? connectorTopology.PreparedEndpointCount
            : 0;
        public int ConnectorUnresolvedEndpointCount =>
            connectorTopology != null
                ? connectorTopology.UnresolvedEndpointCount
                : 0;
        public int ConnectorPreparedPathPointCount =>
            connectorTopology != null
                ? connectorTopology.PreparedPathPointCount
                : 0;
        public int ConnectorPreparedPathVariantCount =>
            connectorTopology != null
                ? connectorTopology.PreparedPathVariantCount
                : 0;
        public int ConnectorUnavailablePathVariantCount =>
            connectorTopology != null
                ? connectorTopology.UnavailablePathVariantCount
                : 0;
        public int ConnectorPreparedRelationshipCatalogueCount =>
            connectorTopology != null
                ? connectorTopology.PreparedRelationshipCatalogueCount
                : 0;
        public int ConnectorPreparedReplacementRelationshipCount =>
            connectorTopology != null
                ? connectorTopology.PreparedReplacementRelationshipCount
                : 0;
        public int ConnectorPreparedRelationshipCataloguePathPointCount =>
            connectorTopology != null
                ? connectorTopology.PreparedRelationshipCataloguePathPointCount
                : 0;
        public int ConnectorPreparedReplacementPathVariantCount =>
            connectorTopology != null
                ? connectorTopology.PreparedReplacementPathVariantCount
                : 0;
        public int ConnectorUnavailableReplacementPathVariantCount =>
            connectorTopology != null
                ? connectorTopology.UnavailableReplacementPathVariantCount
                : 0;
        public string ConnectorTopRejectionReason => connectorTopology != null
            ? connectorTopology.GetTopRejectionSummary()
            : "None";
        public double ConnectorGenerationMilliseconds =>
            connectorTopology != null
                ? connectorTopology.GenerationMilliseconds
                : 0.0;
        public bool PocketTopologyAvailable => pocketTopology != null;
        public int PocketEligibleHostCount => pocketTopology != null
            ? pocketTopology.EligibleHostCount
            : 0;
        public int PocketCandidateCentreCount => pocketTopology != null
            ? pocketTopology.CandidateCentreCount
            : 0;
        public int PocketAcceptedCount => pocketTopology != null
            ? pocketTopology.AcceptedPocketCount
            : 0;
        public int InteriorPocketEligibleHostCount => pocketTopology != null
            ? pocketTopology.InteriorEligibleHostCount
            : 0;
        public int InteriorPocketCandidateCount => pocketTopology != null
            ? pocketTopology.InteriorCandidateCount
            : 0;
        public int InteriorPocketAcceptedCount => pocketTopology != null
            ? pocketTopology.AcceptedInteriorPocketCount
            : 0;
        public int EdgeCavityEligibleHostCount => pocketTopology != null
            ? pocketTopology.CavityEligibleHostCount
            : 0;
        public int EdgeCavityCandidateCount => pocketTopology != null
            ? pocketTopology.CavityCandidateCount
            : 0;
        public int EdgeCavityAcceptedCount => pocketTopology != null
            ? pocketTopology.AcceptedEdgeCavityCount
            : 0;
        public int ConnectorWeakSpanEligibleConnectorCount =>
            pocketTopology != null
                ? pocketTopology.WeakSpanEligibleConnectorCount
                : 0;
        public int ConnectorWeakSpanCandidateCount => pocketTopology != null
            ? pocketTopology.WeakSpanCandidateCount
            : 0;
        public int ConnectorWeakSpanAcceptedCount => pocketTopology != null
            ? pocketTopology.AcceptedConnectorWeakSpanCount
            : 0;
        public int ConnectorWeakSpanPreparedCount => pocketTopology != null
            ? pocketTopology.PreparedWeakSpanRegionCount
            : 0;
        public int ConnectorWeakSpanUnavailableCount => pocketTopology != null
            ? pocketTopology.UnavailableWeakSpanRegionCount
            : 0;
        public int FreeWaterEventOpportunityCount => pocketTopology != null
            ? pocketTopology.FreeWaterOpportunityCount
            : 0;
        public int FreeWaterEventCandidateCount => pocketTopology != null
            ? pocketTopology.FreeWaterCandidateCount
            : 0;
        public int FreeWaterEventAcceptedCount => pocketTopology != null
            ? pocketTopology.AcceptedFreeWaterEventCount
            : 0;
        public string PocketTopRejectionReason => pocketTopology != null
            ? pocketTopology.GetTopRejectionSummary()
            : "None";
        public double PocketGenerationMilliseconds => pocketTopology != null
            ? pocketTopology.GenerationMilliseconds
            : 0.0;
        public bool TopologyReplacementInProgress =>
            topologyReplacementPhase != TopologyReplacementPhase.Idle;
        public bool TopologyReplacementReady =>
            topologyReplacementPhase ==
                TopologyReplacementPhase.ReadyToActivate;
        public string TopologyReplacementState =>
            topologyReplacementPhase.ToString();
        public string TopologyReplacementLastReason =>
            topologyReplacementLastReason;
        public int TopologyReplacementRequestCount =>
            topologyReplacementRequestCount;
        public int TopologyReplacementCoalescedCount =>
            topologyReplacementCoalescedCount;
        public int TopologyReplacementCancelledCount =>
            topologyReplacementCancelledCount;
        public int TopologyReplacementActivatedCount =>
            topologyReplacementActivatedCount;
        public int TopologyReplacementIdenticalPreparedCount =>
            topologyReplacementIdenticalPreparedCount;
        public bool TopologyTransitionActive =>
            topologyTransitionSnapshot != null;
        public string TopologyTransitionState =>
            topologyTransitionSnapshot == null
                ? "Idle"
                : topologyTransitionSnapshot.HoldsVisibleResources
                    ? "Holding Previous Mapping"
                    : "Crossfading";
        public float TopologyTransitionProgress =>
            topologyTransitionSnapshot == null ||
            topologyTransitionSnapshot.HoldsVisibleResources
                ? 0f
                : Mathf.Clamp01(
                    topologyTransitionElapsed /
                    TopologyReplacementTransitionSeconds);
        public float TopologyTransitionDuration =>
            TopologyReplacementTransitionSeconds;
        public int TopologyTransitionStartedCount =>
            topologyTransitionStartedCount;
        public int TopologyTransitionCompletedCount =>
            topologyTransitionCompletedCount;
        public int TopologyTransitionRemappedCount =>
            topologyTransitionRemappedCount;
        public int TopologyTransitionFlattenedCount =>
            topologyTransitionFlattenedCount;
        public string TopologyCacheRoundTripState =>
            topologyCacheRoundTripState;
        public string TopologyCacheRoundTripSummary =>
            topologyCacheRoundTripSummary;
        public int TopologyCacheRoundTripPayloadBytes =>
            topologyCacheRoundTripPayloadBytes;
        public string TopologyCacheRoundTripPayloadHash =>
            topologyCacheRoundTripPayloadHash > 0ul
                ? topologyCacheRoundTripPayloadHash.ToString("X16")
                : "—";
        public double TopologyCacheRoundTripSerializationMilliseconds =>
            topologyCacheRoundTripSerializationMilliseconds;
        public double TopologyCacheRoundTripLoadMilliseconds =>
            topologyCacheRoundTripLoadMilliseconds;
        public double TopologyCacheRoundTripVerificationMilliseconds =>
            topologyCacheRoundTripVerificationMilliseconds;
        public int TopologyCacheRoundTripRunCount =>
            topologyCacheRoundTripRunCount;
        public int TopologyCacheRoundTripPassCount =>
            topologyCacheRoundTripPassCount;
        public bool TopologyCacheRoundTripReady =>
            initializationPhase == InitializationPhase.Ready &&
            AreResourcesCompleteAndCurrent() &&
            majorTopology != null &&
            connectorTopology != null &&
            pocketTopology != null &&
            obstacleExclusionScalar.Length == fieldWidth * fieldHeight;
        public bool TopologyCacheBuildReady =>
            AreTopologyCacheInputsCurrent();
        public int TopologyCacheFormatVersion =>
            StylizedRiverFoamTopologyCacheCodec.FormatVersion;
        public int TopologyCacheGeneratorContractVersion =>
            StylizedRiverFoamTopologyCacheCodec.GeneratorContractVersion;
        public string TopologyCacheBuildState => topologyCacheBuildState;
        public string TopologyCacheBuildSummary => topologyCacheBuildSummary;
        public int TopologyCacheBuildCount => topologyCacheBuildCount;
        public int TopologyCacheBuildSuccessCount =>
            topologyCacheBuildSuccessCount;
        public int TopologyCacheBuildPayloadBytes =>
            topologyCacheBuildPayloadBytes;
        public string TopologyCacheBuildPayloadHash =>
            topologyCacheBuildPayloadHash;
        public double TopologyCacheBuildMilliseconds =>
            topologyCacheBuildMilliseconds;
        public string TopologyCacheValidationState =>
            topologyCacheValidationState;
        public string TopologyCacheValidationSummary =>
            topologyCacheValidationSummary;
        public int TopologyCacheValidationCount =>
            topologyCacheValidationCount;
        public int TopologyCacheValidationHitCount =>
            topologyCacheValidationHitCount;
        public string TopologyCacheDomainFingerprint =>
            topologyCacheDomainFingerprint;
        public string TopologyCacheObstacleFingerprint =>
            topologyCacheObstacleFingerprint;
        public string TopologyCacheGenerationFingerprint =>
            topologyCacheGenerationFingerprint;
        public string TopologyCacheCombinedFingerprint =>
            topologyCacheCombinedFingerprint;
        public int TopologyCacheObstacleSourceCount =>
            topologyCacheObstacleSourceCount;
        public bool TopologyCacheObstacleRegistryReady
        {
            get
            {
                disturbanceRuntime ??=
                    GetComponent<StylizedRiverDisturbanceRuntime>();
                return disturbanceRuntime == null ||
                    disturbanceRuntime.GeneratedObstacleRegistryReady;
            }
        }
        public int TopologyCacheObstacleRegistryProcessedCount
        {
            get
            {
                disturbanceRuntime ??=
                    GetComponent<StylizedRiverDisturbanceRuntime>();
                return disturbanceRuntime != null
                    ? disturbanceRuntime
                        .GeneratedObstacleRegistryProcessedCount
                    : 0;
            }
        }
        public int TopologyCacheObstacleRegistryTotalCount
        {
            get
            {
                disturbanceRuntime ??=
                    GetComponent<StylizedRiverDisturbanceRuntime>();
                return disturbanceRuntime != null
                    ? disturbanceRuntime.GeneratedObstacleRegistryTotalCount
                    : 0;
            }
        }
        public string TopologyCacheStartupState => topologyCacheStartupState;
        public string TopologyCacheStartupSummary =>
            topologyCacheStartupSummary;
        public int TopologyCacheStartupAttemptCount =>
            topologyCacheStartupAttemptCount;
        public int TopologyCacheStartupHitCount =>
            topologyCacheStartupHitCount;
        public int TopologyCacheStartupMissCount =>
            topologyCacheStartupMissCount;
        public int TopologyCacheStartupPayloadBytes =>
            topologyCacheStartupPayloadBytes;
        public string TopologyCacheStartupPayloadHash =>
            topologyCacheStartupPayloadHash;
        public double TopologyCacheStartupLoadMilliseconds =>
            topologyCacheStartupLoadMilliseconds;
        public bool TopologyCacheLoadedForActiveResources =>
            topologyCacheLoadedForActiveResources;
        public bool AutomaticDevelopmentCacheEnabled =>
            IsAutomaticDevelopmentCacheEnabled;
        public bool AutomaticTopologyCacheWritePending =>
            automaticTopologyCacheWritePending;
        public string AutomaticTopologyCachePersistenceState =>
            automaticTopologyCachePersistenceState;
        public string AutomaticTopologyCachePersistenceSummary =>
            automaticTopologyCachePersistenceSummary;
        public int AutomaticTopologyCacheWriteCount =>
            automaticTopologyCacheWriteCount;
        public int AutomaticTopologyCacheWriteSuccessCount =>
            automaticTopologyCacheWriteSuccessCount;
        public bool TopologyStartupValidationComplete =>
            topologyStartupValidationComplete;
        public double TopologyStartupTotalMilliseconds =>
            topologyStartupValidationTotalMilliseconds;
        public double TopologyStartupSlowestStepMilliseconds =>
            topologyStartupValidationSlowestStepMilliseconds;
        public string TopologyStartupSlowestStep =>
            topologyStartupValidationSlowestStep;
        public int TopologyStartupStepCount =>
            topologyStartupValidationStepCount;
        public int TopologyStartupCacheInstallCount =>
            topologyStartupValidationCacheInstallCount;
        public int TopologyStartupObstacleBuildCount =>
            topologyStartupValidationObstacleBuildCount;
        public int TopologyStartupMajorBuildCount =>
            topologyStartupValidationMajorBuildCount;
        public int TopologyStartupConnectorBuildCount =>
            topologyStartupValidationConnectorBuildCount;
        public int TopologyStartupPocketBuildCount =>
            topologyStartupValidationPocketBuildCount;
        public int TopologyStartupExpensivePreparationCount =>
            topologyStartupValidationObstacleBuildCount +
            topologyStartupValidationMajorBuildCount +
            topologyStartupValidationConnectorBuildCount +
            topologyStartupValidationPocketBuildCount;
        public bool ExplicitTopologyGenerationAvailable =>
            Application.isPlaying &&
            !DevelopmentTopologyGenerationInProgress &&
            (initializationPhase ==
                InitializationPhase.AwaitExplicitTopologyGeneration ||
             (initializationPhase == InitializationPhase.Ready &&
              (ResolveRequestedTopologySignature() !=
                   ResolveActiveTopologySignature() ||
               ResolveCurrentObstacleGeometryVersion() !=
                   obstacleGeometryVersion)));
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
            !TopologyReplacementInProgress &&
            !TopologyTransitionActive &&
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
            EstimateTextureBytes(
                topologyReplacementBuild != null
                    ? topologyReplacementBuild.GeneratedTexture
                    : null) +
            EstimateTopologyTransitionBytes() +
            EstimateTextureBytes(retiredTopologyTransitionTexture) +
            EstimateTextureBytes(retiredGeneratedTopologyTexture) +
            EstimateTextureBytes(evolvingMajorTexture) +
            EstimateTextureBytes(evolvingHostedNegativeTexture) +
            EstimateTextureBytes(evolvingFreeWaterNegativeTexture) +
            EstimateTextureBytes(evolvingConnectorTexture) +
            EstimateTextureBytes(evolvingWeakSpanNegativeTexture) +
            EstimateTextureBytes(topologyGeneratedUploadTexture) +
            EstimateTextureBytes(majorMaskTextureArray) +
            EstimateTextureBytes(hostedNegativeMaskTextureArray) +
            EstimateTextureBytes(freeWaterNegativeMaskTextureArray) +
            EstimateTextureBytes(currentShoreEdgesTexture) +
            EstimateTextureBytes(obstacleExclusionTexture) +
            EstimateTextureBytes(fractureA) +
            EstimateTextureBytes(fractureB) +
            EstimateTextureBytes(neutralDisturbanceTexture) +
            EstimateTextureBytes(boundaryTexture) +
            EstimateTextureBytes(obstacleExclusionReadbackTexture) +
            EstimateTextureBytes(obstacleExclusionUploadTexture) +
            (obstacleExclusionCellBuffer != null
                ? (long)obstacleExclusionCellBuffer.count *
                    obstacleExclusionCellBuffer.stride
                : 0L) +
            (obstacleExclusionSampleBuffer != null
                ? (long)obstacleExclusionSampleBuffer.count *
                    obstacleExclusionSampleBuffer.stride
                : 0L) +
            (metricBuffer != null
                ? (long)metricBuffer.count * metricBuffer.stride
                : 0L) +
            (populationMetricsBuffer != null
                ? (long)populationMetricsBuffer.count * populationMetricsBuffer.stride
                : 0L) +
            (topologyMetricsBuffer != null
                ? (long)topologyMetricsBuffer.count * topologyMetricsBuffer.stride
                : 0L) +
            (majorEvolutionBuffer != null
                ? (long)majorEvolutionBuffer.count * majorEvolutionBuffer.stride
                : 0L) +
            (hostedNegativeEvolutionBuffer != null
                ? (long)hostedNegativeEvolutionBuffer.count *
                    hostedNegativeEvolutionBuffer.stride
                : 0L) +
            (freeWaterEvolutionBuffer != null
                ? (long)freeWaterEvolutionBuffer.count *
                    freeWaterEvolutionBuffer.stride
                : 0L) +
            (connectorIdentityBuffer != null
                ? (long)connectorIdentityBuffer.count *
                    connectorIdentityBuffer.stride
                : 0L) +
            (connectorPathPointBuffer != null
                ? (long)connectorPathPointBuffer.count *
                    connectorPathPointBuffer.stride
                : 0L) +
            (weakSpanIdentityBuffer != null
                ? (long)weakSpanIdentityBuffer.count *
                    weakSpanIdentityBuffer.stride
                : 0L);

        private static bool IsAutomaticDevelopmentCacheEnabled
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return true;
#else
                return false;
#endif
            }
        }

        private bool DevelopmentTopologyGenerationInProgress =>
            explicitTopologyGenerationInProgress ||
            automaticTopologyGenerationInProgress;

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
            SystemInfo.supports2DArrayTextures &&
            SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf) &&
            SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGHalf) &&
            SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RHalf) &&
            SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf);
    }
}
