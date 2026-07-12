using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        public int FieldWidth => currentState != null ? currentState.width : 0;
        public int FieldHeight => currentState != null ? currentState.height : 0;
        public int StructuralWidth => structuralWidth;
        public int StructuralHeight => structuralHeight;
        public int TopologyWidth => topologyTexture != null ? topologyTexture.width : 0;
        public int TopologyHeight => topologyTexture != null ? topologyTexture.height : 0;
        public bool MajorTopologyAvailable => majorTopology != null;
        public float FoamMotionLaneScrollCells => lastMotionLaneScrollCells;
        public float FoamMotionLaneScrollMetres =>
            lastMotionLaneScrollCells *
            Mathf.Max(0f, minimumTransportLongitudinalSpacing);
        public float FoamBaseDownstreamSpeedMetresPerSecond =>
            ResolveBaseFoamDownstreamSpeedMetresPerSecond();
        public float FoamMaximumLateralSpeedMetresPerSecond =>
            ResolveBaseFoamDownstreamSpeedMetresPerSecond() *
            (river != null ? river.FoamMaximumLateralSpeedRatio : 0f);
        public int FoamMotionLaneSignature => lastMotionLaneSignature;
        public int FoamObstacleRoutingSignature => lastObstacleRoutingSignature;
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
        public float TopologyMetricsAgeSeconds =>
            topologyMetricsAvailable && topologyMetricsLastCompletedAt >= 0.0
                ? Mathf.Max(
                    0f,
                    (float)(Time.realtimeSinceStartupAsDouble -
                        topologyMetricsLastCompletedAt))
                : -1f;
        public bool TopologyMetricsFresh =>
            TopologyMetricsAgeSeconds >= 0f &&
            TopologyMetricsAgeSeconds <= 0.75f;
        public float MajorSupportCoverage => TopologyCoverageRatio(TopologyMetricMajorSupport);
        public float ConnectorSupportCoverage => TopologyCoverageRatio(TopologyMetricConnectorSupport);
        public float NegativeAgingPressureCoverage => TopologyCoverageRatio(TopologyMetricNegativeAgingPressure);
        public float FoamWithinNegativeAgingPressure =>
            TopologyRegionRatio(
                TopologyMetricFoamInNegativeAgingPressure,
                TopologyMetricNegativeAgingPressure);
        public float VisibleMaterialCoverage => TopologyCoverageRatio(TopologyMetricVisibleMaterial);
        public float IntegratedPresenceArea => integratedPresenceArea;
        public float VisiblePresenceCoreArea => visiblePresenceCoreArea;
        public bool ManualProofReferenceAvailable => manualProofReferenceArea > 0.0001f;
        public float ManualProofReferenceArea => manualProofReferenceArea;
        public float ManualProofPresenceRatio => manualProofReferenceArea > 0.0001f
            ? integratedPresenceArea / manualProofReferenceArea
            : 0f;
        public float FoamWithinShoreSupport => TopologyRegionRatio(
            TopologyMetricFoamInShoreSupport,
            TopologyMetricShoreSupport);
        public float FoamWithinPressureLeeSupport => TopologyRegionRatio(
            TopologyMetricFoamInPressureLeeSupport,
            TopologyMetricPressureLeeSupport);
        public float PerimeterRatio => TopologyRegionRatio(
            TopologyMetricPerimeterVisible,
            TopologyMetricVisibleMaterial);
        public float ConnectorMajorOverlap => TopologyRegionRatio(
            TopologyMetricConnectorMajorOverlap,
            TopologyMetricConnectorSupport);
        public float VisibleFoamPresenceArea => TopologyAreaMetric(
            TopologyMetricVisiblePresenceArea);
        public float AverageVisibleRemainingLife =>
            TopologyWeightedMetricAverage(
                TopologyMetricVisibleLifeArea,
                TopologyMetricVisiblePresenceArea);
        public float AveragePositiveSupportUnderVisibleFoam =>
            TopologyWeightedMetricAverage(
                TopologyMetricVisiblePositiveSupportArea,
                TopologyMetricVisiblePresenceArea);
        public float AverageNegativeAgingUnderVisibleFoam =>
            TopologyWeightedMetricAverage(
                TopologyMetricVisibleNegativeAgingArea,
                TopologyMetricVisiblePresenceArea);
        public float AverageLocalAgingRateUnderVisibleFoam =>
            TopologyWeightedMetricAverage(
                TopologyMetricVisibleLocalAgingRateArea,
                TopologyMetricVisiblePresenceArea);
        public float FoamSupportNegativeOverlapRatio =>
            TopologyRegionRatio(
                TopologyMetricFoamSupportNegativeOverlap,
                TopologyMetricVisibleMaterial);
        public float StrongestPositiveSupportUnderFoam =>
            TopologyFixedTenThousandMetric(
                TopologyMetricMaxPositiveSupportUnderFoam);
        public float StrongestNegativeAgingUnderFoam =>
            TopologyFixedTenThousandMetric(
                TopologyMetricMaxNegativeAgingUnderFoam);
        public bool VisibleLifeRangeAvailable =>
            topologyMetricsAvailable &&
            latestTopologyMetrics[TopologyMetricVisibleMaterial] > 0u;
        public float MinimumVisibleRemainingLife => VisibleLifeRangeAvailable
            ? Mathf.Clamp01(
                latestTopologyMetrics[TopologyMetricVisibleLifeMinimumFixed] /
                10000f)
            : 0f;
        public float MaximumVisibleRemainingLife => VisibleLifeRangeAvailable
            ? Mathf.Clamp01(
                latestTopologyMetrics[TopologyMetricVisibleLifeMaximumFixed] /
                10000f)
            : 0f;
        public string MaterialClockStatus => ResolveMaterialClockStatus();
        public string RuntimeAgingParameterStatus =>
            ResolveRuntimeAgingParameterStatus();
        public string ProbeDecayCheckStatus => ResolveProbeDecayCheckStatus();
        public string VisibleLifeRangeStatus => ResolveVisibleLifeRangeStatus();
        public string BirthActivityStatus => ResolveBirthActivityStatus();
        public string IsolatedLifeProbeStatus => isolatedLifeProbeStatus;
        public bool ShouldRepaintInspectorForFoamDebug =>
            Application.isPlaying &&
            (currentState != null ||
             materialLifetimeAuthorityActive ||
             topologyMetricsReadbackPending ||
             pendingInjections.Count > 0 ||
             pendingMaterialBirths.Count > 0 ||
             pendingIsolatedLifeProbe ||
             activeFoamCompositionEventCount > 0 ||
             IsTopologyDebugActive ||
             IsProgressiveBirthSourceDebugActive ||
             (topologyMetricsLastCompletedAt >= 0.0 &&
              Time.realtimeSinceStartupAsDouble - topologyMetricsLastCompletedAt < 2.0));
        public string LifetimeAuthorityStatus => lifetimeAuthorityStatus;
        public bool MaterialLifetimeAuthorityActive =>
            currentState != null || materialLifetimeAuthorityActive;
        public string LifetimeProofStatus => ResolveLifetimeProofStatus();
        public string TopologyAgingProofStatus => ResolveTopologyAgingProofStatus();
        public int ActiveChunkCount => 0;
        public int PendingInjectionCount => pendingInjections.Count;
        public int ActiveReservationCount => 0;
        public int FoamCompositionPoolCapacity =>
            FoamCompositionEventCapacity;
        public int ActiveFoamCompositionEventCount =>
            activeFoamCompositionEventCount;
        public int FoamCompositionBirthBudgetPerStep =>
            ResolveFoamCompositionBirthBudgetPerStep();
        public int FoamCompositionStartedCount =>
            foamCompositionStartedCount;
        public int FoamCompositionCompletedCount =>
            foamCompositionCompletedCount;
        public int FoamCompositionRejectedCount =>
            foamCompositionRejectedCount;
        public int LatestFoamCompositionEventId =>
            latestFoamCompositionEventId;
        public float LatestFoamCompositionProgress =>
            latestFoamCompositionProgress;
        public float LatestFoamCompositionHeadDistanceNormalized =>
            latestFoamCompositionHeadDistanceNormalized;
        public float LatestFoamCompositionHeadAcrossNormalized =>
            latestFoamCompositionHeadAcrossNormalized;
        public float LatestFoamCompositionPreviousDistanceNormalized =>
            latestFoamCompositionPreviousDistanceNormalized;
        public float LatestFoamCompositionPreviousAcrossNormalized =>
            latestFoamCompositionPreviousAcrossNormalized;
        public float LastFoamCompositionSegmentLength =>
            lastFoamCompositionSegmentLength;
        public int FoamCompositionEventUpdateCount =>
            foamCompositionEventUpdateCount;
        public int FoamCompositionSegmentDispatchAttemptCount =>
            foamCompositionSegmentDispatchAttemptCount;
        public int FoamCompositionSegmentDispatchSubmittedCount =>
            foamCompositionSegmentDispatchSubmittedCount;
        public float FoamCompositionCumulativeCentrelineDistance =>
            foamCompositionCumulativeCentrelineDistance;

        public bool ProgressiveBirthDebugReadbackAvailable =>
            progressiveBirthDebugReadbackAvailable;
        public bool ProgressiveBirthDebugReadbackPending =>
            progressiveBirthDebugReadbackPending;
        public uint ProgressiveBirthDebugLatestAffectedTexels =>
            progressiveBirthDebugLatestAffectedTexels;
        public uint ProgressiveBirthDebugCumulativeAffectedTexels =>
            progressiveBirthDebugCumulativeAffectedTexels;
        public bool ProgressiveBirthSourceDebugActive =>
            IsProgressiveBirthSourceDebugActive;
        public string AutomaticShoreBirthStatus => automaticShoreBirthStatus;
        public int AutomaticShoreBirthSubmittedLastUpdate =>
            automaticShoreBirthSubmittedLastUpdate;
        public int AutomaticShoreBirthRejectedLastUpdate =>
            automaticShoreBirthRejectedLastUpdate;
        public int AutomaticShoreBirthSubmittedTotal =>
            automaticShoreBirthSubmittedTotal;
        public int AutomaticShoreBirthBudgetPerTick =>
            ResolveAutomaticShoreBirthBudgetPerTick();

        public string AutomaticObjectBirthStatus => automaticObjectBirthStatus;
        public int AutomaticObjectBirthSubmittedLastUpdate =>
            automaticObjectBirthSubmittedLastUpdate;
        public int AutomaticObjectBirthRejectedLastUpdate =>
            automaticObjectBirthRejectedLastUpdate;
        public int AutomaticObjectBirthSubmittedTotal =>
            automaticObjectBirthSubmittedTotal;
        public int AutomaticObjectBirthAnchorCountLastUpdate =>
            automaticObjectBirthAnchorCountLastUpdate;
        public int AutomaticObjectBirthBudgetPerTick =>
            AutomaticObjectSourceMaximumStartsPerUpdate;

        public string AutomaticFreeWaterBirthStatus => automaticFreeWaterBirthStatus;
        public int AutomaticFreeWaterBirthSubmittedLastUpdate =>
            automaticFreeWaterBirthSubmittedLastUpdate;
        public int AutomaticFreeWaterBirthRejectedLastUpdate =>
            automaticFreeWaterBirthRejectedLastUpdate;
        public int AutomaticFreeWaterBirthSubmittedTotal =>
            automaticFreeWaterBirthSubmittedTotal;
        public int AutomaticFreeWaterBirthBudgetPerTick =>
            AutomaticFreeWaterSourceMaximumStartsPerUpdate;
        public int InjectedLastUpdate => injectedLastUpdate;
        public float LastInjectionBoundaryCoverage => lastInjectionBoundaryCoverage;
        public bool LastInjectionStateSynchronized =>
            lastInjectionStateSynchronized;
        public int LastUpdateDispatches => lastUpdateDispatches;
        public int RecentPeakDispatches => recentPeakDispatches;
        public long LastUpdateCellIterations => lastUpdateCellIterations;
        public long RecentPeakCellIterations => recentPeakCellIterations;
        public float UpdateRate => ResolveUpdateRate();
        public float MaterialStepDuration => lastMaterialStepDuration;
        public int MaterialStepsLastFrame => lastMaterialStepsThisFrame;
        public float RenderInterpolationAlpha => lastRenderInterpolationAlpha;
        public float EstimatedTransportCellsPerStep =>
            lastEstimatedTransportCellsPerStep;
        public float EstimatedLateralTransportCellsPerStep =>
            lastEstimatedLateralTransportCellsPerStep;
        public float TransportStepCfl => lastMaximumTransportCfl;
        public float MaximumTransportCfl =>
            lastMaximumTransportCfl /
            Mathf.Max(1, lastUsedTransportSubsteps);
        public int TransportSubstepsRequired =>
            lastRequiredTransportSubsteps;
        public int TransportSubstepsUsed => lastUsedTransportSubsteps;
        public int TransportSubstepLimit => MaximumTransportSubsteps;
        public float TransportCflTarget => TransportTargetCfl;
        public float TransportConservationErrorGateRatio =>
            TransportConservationErrorGate;
        public float TransportClampLossGateRatio =>
            TransportClampLossGate;
        public bool TransportSafetyLimitExceeded =>
            transportSafetyLimitExceeded;
        public string TransportSafetyStatus => transportSafetyStatus;
        public bool TransportMetricsSupported =>
            SystemInfo.supportsAsyncGPUReadback;
        public bool TransportMetricsAvailable => transportMetricsAvailable;
        public float TransportPresenceBefore => transportPresenceBefore;
        public float TransportPresenceAfter => transportPresenceAfter;
        public float TransportLifeMomentBefore => transportLifeMomentBefore;
        public float TransportLifeMomentAfter => transportLifeMomentAfter;
        public float TransportPatternMomentBefore =>
            transportPatternMomentBefore;
        public float TransportPatternMomentAfter =>
            transportPatternMomentAfter;
        public float TransportPresenceBoundaryOutflow =>
            transportPresenceBoundaryOutflow;
        public float TransportLifeBoundaryOutflow =>
            transportLifeBoundaryOutflow;
        public float TransportPatternBoundaryOutflow =>
            transportPatternBoundaryOutflow;
        public float TransportPresenceUnaccountedError =>
            transportPresenceUnaccountedError;
        public float TransportLifeUnaccountedError =>
            transportLifeUnaccountedError;
        public float TransportPatternUnaccountedError =>
            transportPatternUnaccountedError;
        public float TransportPresenceUnaccountedErrorRatio =>
            transportPresenceUnaccountedErrorRatio;
        public float TransportLifeUnaccountedErrorRatio =>
            transportLifeUnaccountedErrorRatio;
        public float TransportPatternUnaccountedErrorRatio =>
            transportPatternUnaccountedErrorRatio;
        public float TransportPresenceClampLoss =>
            transportPresenceClampLoss;
        public float TransportLifeClampLoss => transportLifeClampLoss;
        public float TransportPatternClampLoss => transportPatternClampLoss;
        public float TransportPresenceClampLossRatio =>
            transportPresenceClampLossRatio;
        public float TransportPresenceUnitCapacityLoss =>
            transportPresenceUnitCapacityLoss;
        public float TransportPresenceBoundaryCapacityLoss =>
            transportPresenceBoundaryCapacityLoss;
        public float TransportPresenceObstacleCapacityLoss =>
            transportPresenceObstacleCapacityLoss;
        public float TransportPresenceStateValidityLoss =>
            transportPresenceStateValidityLoss;
        public float TransportPresenceMinimumCutoffLoss =>
            transportPresenceMinimumCutoffLoss;
        public float TransportPresenceAttributionResidual =>
            transportPresenceAttributionResidual;
        public float TransportMaximumRawPresence =>
            transportMaximumRawPresence;
        public float TransportMaximumLocalCapacityExcess =>
            transportMaximumLocalCapacityExcess;
        public uint TransportTotalCapacityHitCount =>
            transportTotalCapacityHitCount;
        public uint TransportUnitCapacityHitCount =>
            transportUnitCapacityHitCount;
        public uint TransportBoundaryCapacityHitCount =>
            transportBoundaryCapacityHitCount;
        public uint TransportObstacleCapacityHitCount =>
            transportObstacleCapacityHitCount;
        public bool InitializationComplete =>
            initializationPhase == InitializationPhase.Ready;
        public bool ResourcesAllocated =>
            InitializationComplete && currentState != null &&
            shapeMaskTexture != null && currentVisualOccupancy != null;
        public bool VisualOccupancyAvailable =>
            ResourcesAllocated && visualOccupancyA != null &&
            visualOccupancyB != null && advanceVisualOccupancyKernel >= 0;
        public bool ConservativeTransportActive =>
            ResourcesAllocated && simulateKernel >= 0 &&
            resetTransportMetricsKernel >= 0 &&
            transportMetricsBuffer != null;
        public bool IsSleeping =>
            currentState == null &&
            !TopologyReplacementInProgress &&
            !TopologyTransitionActive &&
            !IsTopologyDebugActive &&
            !IsProgressiveBirthSourceDebugActive &&
            !IsMotionFieldDebugActive &&
            !IsShapeProductDebugActive &&
            !materialLifetimeAuthorityActive &&
            pendingInjections.Count == 0 &&
            !pendingIsolatedLifeProbe &&
            activeFoamCompositionEventCount == 0 &&
            pendingMaterialBirths.Count == 0 &&
            !IsAutomaticSourcePopulationActive;
        public long EstimatedMemoryBytes =>
            EstimateTextureBytes(stateA) +
            EstimateTextureBytes(stateB) +
            EstimateTextureBytes(shapeMaskTexture) +
            EstimateTextureBytes(filmSourceTexture) +
            EstimateTextureBytes(filmSupportTexture) +
            EstimateTextureBytes(visualOccupancyA) +
            EstimateTextureBytes(visualOccupancyB) +
            EstimateTextureBytes(progressiveBirthDebugTexture) +
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
            EstimateTextureBytes(objectContactFieldTexture) +
            EstimateTextureBytes(motionLaneTexture) +
            EstimateTextureBytes(obstacleRoutingTexture) +
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
            (topologyMetricsBuffer != null
                ? (long)topologyMetricsBuffer.count * topologyMetricsBuffer.stride
                : 0L) +
            (transportMetricsBuffer != null
                ? (long)transportMetricsBuffer.count * transportMetricsBuffer.stride
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

        private bool IsTopologyDebugActive
        {
            get
            {
                if (river == null)
                {
                    return false;
                }

                return river.FoamDebugView ==
                    StylizedRiverFoamDebugView.FoamAndAgingTopology;
            }
        }

        private bool IsMotionFieldDebugActive
        {
            get
            {
                if (river == null)
                {
                    return false;
                }

                return river.FoamDebugView ==
                        StylizedRiverFoamDebugView.FoamMotionField ||
                    river.FoamDebugView ==
                        StylizedRiverFoamDebugView.FoamMotionFieldCellGrid;
            }
        }

        private bool IsShapeProductDebugActive
        {
            get
            {
                if (river == null)
                {
                    return false;
                }

                return river.FoamDebugView ==
                        StylizedRiverFoamDebugView.FoamEvaluatedShape ||
                    river.FoamDebugView ==
                        StylizedRiverFoamDebugView.FoamShapeDifference ||
                    river.FoamDebugView ==
                        StylizedRiverFoamDebugView.FoamShaderDetailProbe ||
                    river.FoamDebugView ==
                        StylizedRiverFoamDebugView.FoamShaderDetailDifference ||
                    river.FoamDebugView ==
                        StylizedRiverFoamDebugView.FoamFilmSource ||
                    river.FoamDebugView ==
                        StylizedRiverFoamDebugView.FoamFilmSupport ||
                    river.FoamDebugView ==
                        StylizedRiverFoamDebugView.FoamFilmTarget ||
                    river.FoamDebugView ==
                        StylizedRiverFoamDebugView.FoamTemporalOccupancy ||
                    river.FoamDebugView ==
                        StylizedRiverFoamDebugView.FoamTemporalDifference ||
                    river.FoamDebugView ==
                        StylizedRiverFoamDebugView.FoamEvaluatedFinalPreview;
            }
        }

        private bool IsSupported =>
            SystemInfo.supportsComputeShaders &&
            SystemInfo.supports2DArrayTextures &&
            SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf) &&
            SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGHalf) &&
            SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RHalf) &&
            SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf) &&
            SystemInfo.SupportsTextureFormat(TextureFormat.RHalf) &&
            SystemInfo.SupportsTextureFormat(TextureFormat.RGHalf);
    }
}
