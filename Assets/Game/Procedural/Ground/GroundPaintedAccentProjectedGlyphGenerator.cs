using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Rivers;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    public enum GroundPaintedAccentProjectedGlyphRejectionReason
    {
        None = 0,
        Sampling = 1,
        River = 2,
        ModifierExclusion = 3,
        BroadSlope = 4,
        LocalGrade = 5,
        FamilyShape = 6,
        SharpTurn = 7
    }

    public readonly struct GroundPaintedAccentMetricRange
    {
        public GroundPaintedAccentMetricRange(
            float minimum,
            float mean,
            float maximum)
        {
            Minimum = Mathf.Max(0f, minimum);
            Mean = Mathf.Max(0f, mean);
            Maximum = Mathf.Max(0f, maximum);
        }

        public float Minimum { get; }
        public float Mean { get; }
        public float Maximum { get; }
    }

    public readonly struct GroundPaintedAccentGlyphFamilyShapeMetrics
    {
        public GroundPaintedAccentGlyphFamilyShapeMetrics(
            float crestPosition,
            float legSpanRatio,
            float legSlopeRatio,
            float upperRunFraction,
            float upperEndpointDropFraction,
            float descendingDropFraction,
            float plateauFraction,
            float verticalRangeFraction,
            float endpointDifferenceFraction,
            float endpointDifferenceWorld,
            bool isNearStraightVariant)
        {
            CrestPosition = Mathf.Clamp01(crestPosition);
            LegSpanRatio = Mathf.Max(0f, legSpanRatio);
            LegSlopeRatio = Mathf.Max(0f, legSlopeRatio);
            UpperRunFraction = Mathf.Clamp01(upperRunFraction);
            UpperEndpointDropFraction =
                Mathf.Clamp01(upperEndpointDropFraction);
            DescendingDropFraction = Mathf.Clamp01(descendingDropFraction);
            PlateauFraction = Mathf.Clamp01(plateauFraction);
            VerticalRangeFraction = Mathf.Clamp01(verticalRangeFraction);
            EndpointDifferenceFraction =
                Mathf.Clamp01(endpointDifferenceFraction);
            EndpointDifferenceWorld =
                Mathf.Max(0f, endpointDifferenceWorld);
            IsNearStraightVariant = isNearStraightVariant;
        }

        public float CrestPosition { get; }
        public float LegSpanRatio { get; }
        public float LegSlopeRatio { get; }
        public float UpperRunFraction { get; }
        public float UpperEndpointDropFraction { get; }
        public float DescendingDropFraction { get; }
        public float PlateauFraction { get; }
        public float VerticalRangeFraction { get; }
        public float EndpointDifferenceFraction { get; }
        public float EndpointDifferenceWorld { get; }
        public bool IsNearStraightVariant { get; }
    }

    public readonly struct GroundPaintedAccentGlyphFamilyShapeStatistics
    {
        public GroundPaintedAccentGlyphFamilyShapeStatistics(
            GroundPaintedAccentMetricRange crestPosition,
            GroundPaintedAccentMetricRange legSpanRatio,
            GroundPaintedAccentMetricRange legSlopeRatio,
            GroundPaintedAccentMetricRange upperRunFraction,
            GroundPaintedAccentMetricRange upperEndpointDropFraction,
            GroundPaintedAccentMetricRange descendingDropFraction,
            GroundPaintedAccentMetricRange plateauFraction,
            GroundPaintedAccentMetricRange verticalRangeFraction,
            GroundPaintedAccentMetricRange endpointDifferenceFraction,
            GroundPaintedAccentMetricRange endpointDifferenceWorld,
            int nearStraightCount)
        {
            CrestPosition = crestPosition;
            LegSpanRatio = legSpanRatio;
            LegSlopeRatio = legSlopeRatio;
            UpperRunFraction = upperRunFraction;
            UpperEndpointDropFraction = upperEndpointDropFraction;
            DescendingDropFraction = descendingDropFraction;
            PlateauFraction = plateauFraction;
            VerticalRangeFraction = verticalRangeFraction;
            EndpointDifferenceFraction = endpointDifferenceFraction;
            EndpointDifferenceWorld = endpointDifferenceWorld;
            NearStraightCount = Mathf.Max(0, nearStraightCount);
        }

        public GroundPaintedAccentMetricRange CrestPosition { get; }
        public GroundPaintedAccentMetricRange LegSpanRatio { get; }
        public GroundPaintedAccentMetricRange LegSlopeRatio { get; }
        public GroundPaintedAccentMetricRange UpperRunFraction { get; }
        public GroundPaintedAccentMetricRange UpperEndpointDropFraction { get; }
        public GroundPaintedAccentMetricRange DescendingDropFraction { get; }
        public GroundPaintedAccentMetricRange PlateauFraction { get; }
        public GroundPaintedAccentMetricRange VerticalRangeFraction { get; }
        public GroundPaintedAccentMetricRange EndpointDifferenceFraction { get; }
        public GroundPaintedAccentMetricRange EndpointDifferenceWorld { get; }
        public int NearStraightCount { get; }
    }

    public readonly struct GroundPaintedAccentGlyphFamilyStatistics
    {
        public GroundPaintedAccentGlyphFamilyStatistics(
            int attempted,
            int accepted,
            int rejected,
            float authoredLengthMin,
            float authoredLengthMean,
            float authoredLengthMax,
            float peakHeightMin,
            float peakHeightMean,
            float peakHeightMax,
            GroundPaintedAccentGlyphFamilyShapeStatistics shapeStatistics)
        {
            Attempted = Mathf.Max(0, attempted);
            Accepted = Mathf.Max(0, accepted);
            Rejected = Mathf.Max(0, rejected);
            AuthoredLengthMin = Mathf.Max(0f, authoredLengthMin);
            AuthoredLengthMean = Mathf.Max(0f, authoredLengthMean);
            AuthoredLengthMax = Mathf.Max(0f, authoredLengthMax);
            PeakHeightMin = Mathf.Max(0f, peakHeightMin);
            PeakHeightMean = Mathf.Max(0f, peakHeightMean);
            PeakHeightMax = Mathf.Max(0f, peakHeightMax);
            ShapeStatistics = shapeStatistics;
        }

        public int Attempted { get; }
        public int Accepted { get; }
        public int Rejected { get; }
        public float AuthoredLengthMin { get; }
        public float AuthoredLengthMean { get; }
        public float AuthoredLengthMax { get; }
        public float PeakHeightMin { get; }
        public float PeakHeightMean { get; }
        public float PeakHeightMax { get; }
        public GroundPaintedAccentGlyphFamilyShapeStatistics ShapeStatistics
        {
            get;
        }
    }

    public readonly struct GroundPaintedAccentProjectedGlyph
    {
        internal GroundPaintedAccentProjectedGlyph(
            Vector3[] localSurfacePoints,
            Vector2[] localProjectedPoints,
            float[] halfWidths,
            GroundPaintedAccentGlyphFamily family,
            float authoredLength,
            int seed,
            GroundPaintedAccentGlyphFamilyShapeMetrics shapeMetrics,
            float crestT,
            float crestPeakHeight,
            float crownPeakHeight,
            float combinedPeakHeight,
            float maximumProjectedTurnDegrees,
            float maximumNorthDisplacementError,
            float maximumCrossAxisDrift,
            int companionClusterIndex,
            GroundPaintedAccentCompanionMemberRole companionMemberRole,
            int intendedCompanionClusterSize)
        {
            LocalSurfacePoints =
                localSurfacePoints ?? Array.Empty<Vector3>();
            LocalProjectedPoints =
                localProjectedPoints ?? Array.Empty<Vector2>();
            HalfWidths = halfWidths ?? Array.Empty<float>();
            Family = family;
            AuthoredLength = Mathf.Max(0f, authoredLength);
            Seed = seed;
            ShapeMetrics = shapeMetrics;
            CrestT = Mathf.Clamp01(crestT);
            CrestPeakHeight = Mathf.Max(0f, crestPeakHeight);
            CrownPeakHeight = Mathf.Max(0f, crownPeakHeight);
            CombinedPeakHeight = Mathf.Max(0f, combinedPeakHeight);
            MaximumProjectedTurnDegrees =
                Mathf.Max(0f, maximumProjectedTurnDegrees);
            MaximumNorthDisplacementError =
                Mathf.Max(0f, maximumNorthDisplacementError);
            MaximumCrossAxisDrift = Mathf.Max(0f, maximumCrossAxisDrift);
            CompanionClusterIndex = companionClusterIndex;
            CompanionMemberRole = companionMemberRole;
            IntendedCompanionClusterSize =
                companionClusterIndex >= 0
                    ? Mathf.Clamp(intendedCompanionClusterSize, 2, 3)
                    : 0;
        }

        public Vector3[] LocalSurfacePoints { get; }
        internal Vector2[] LocalProjectedPoints { get; }
        public float[] HalfWidths { get; }
        public GroundPaintedAccentGlyphFamily Family { get; }
        public float AuthoredLength { get; }
        public int Seed { get; }
        public GroundPaintedAccentGlyphFamilyShapeMetrics ShapeMetrics { get; }
        public float CrestT { get; }
        public float CrestPeakHeight { get; }
        public float CrownPeakHeight { get; }
        public float CombinedPeakHeight { get; }
        public float MaximumProjectedTurnDegrees { get; }
        public float MaximumNorthDisplacementError { get; }
        public float MaximumCrossAxisDrift { get; }
        internal int CompanionClusterIndex { get; }
        internal GroundPaintedAccentCompanionMemberRole CompanionMemberRole { get; }
        internal int IntendedCompanionClusterSize { get; }
        internal bool IsCompanionMember => CompanionClusterIndex >= 0;

        public bool IsValid =>
            LocalSurfacePoints != null &&
            LocalProjectedPoints != null &&
            HalfWidths != null &&
            LocalSurfacePoints.Length >= 2 &&
            LocalProjectedPoints.Length == LocalSurfacePoints.Length &&
            HalfWidths.Length == LocalSurfacePoints.Length;
    }

    public readonly struct GroundPaintedAccentProjectedGlyphRejectionDebugPoint
    {
        public GroundPaintedAccentProjectedGlyphRejectionDebugPoint(
            Vector3 localPosition,
            GroundPaintedAccentGlyphFamily family,
            GroundPaintedAccentProjectedGlyphRejectionReason reason)
        {
            LocalPosition = localPosition;
            Family = family;
            Reason = reason;
        }

        public Vector3 LocalPosition { get; }
        public GroundPaintedAccentGlyphFamily Family { get; }
        public GroundPaintedAccentProjectedGlyphRejectionReason Reason { get; }
    }

    public readonly struct GroundPaintedAccentCompanionQuotaDiagnostics
    {
        public GroundPaintedAccentCompanionQuotaDiagnostics(
            int validProjectedMarks,
            float requestedParticipationFraction,
            float requestedTripletShare,
            int requestedParticipants,
            int requestedPairClusters,
            int requestedTripletClusters,
            int achievedPairClusters,
            int achievedTripletClusters,
            int pairShortfall,
            int tripletShortfall,
            int buildAttempts,
            Vector4 requestedPairLayouts,
            Vector4 achievedPairLayouts,
            Vector4 requestedTripletLayouts,
            Vector4 achievedTripletLayouts)
        {
            ValidProjectedMarks = Mathf.Max(0, validProjectedMarks);
            RequestedParticipationFraction =
                Mathf.Clamp01(requestedParticipationFraction);
            RequestedTripletShare = Mathf.Clamp01(requestedTripletShare);
            RequestedParticipants = Mathf.Max(0, requestedParticipants);
            RequestedPairClusters = Mathf.Max(0, requestedPairClusters);
            RequestedTripletClusters = Mathf.Max(0, requestedTripletClusters);
            AchievedPairClusters = Mathf.Max(0, achievedPairClusters);
            AchievedTripletClusters = Mathf.Max(0, achievedTripletClusters);
            PairShortfall = Mathf.Max(0, pairShortfall);
            TripletShortfall = Mathf.Max(0, tripletShortfall);
            BuildAttempts = Mathf.Max(0, buildAttempts);
            RequestedPairLayouts = ClampCountVector(requestedPairLayouts);
            AchievedPairLayouts = ClampCountVector(achievedPairLayouts);
            RequestedTripletLayouts = ClampCountVector(requestedTripletLayouts);
            AchievedTripletLayouts = ClampCountVector(achievedTripletLayouts);
        }

        public int ValidProjectedMarks { get; }
        public float RequestedParticipationFraction { get; }
        public float RequestedTripletShare { get; }
        public int RequestedParticipants { get; }
        public int RequestedPairClusters { get; }
        public int RequestedTripletClusters { get; }
        public int AchievedPairClusters { get; }
        public int AchievedTripletClusters { get; }
        public int PairShortfall { get; }
        public int TripletShortfall { get; }
        public int BuildAttempts { get; }
        public Vector4 RequestedPairLayouts { get; }
        public Vector4 AchievedPairLayouts { get; }
        public Vector4 RequestedTripletLayouts { get; }
        public Vector4 AchievedTripletLayouts { get; }

        private static Vector4 ClampCountVector(Vector4 value)
        {
            value.x = Mathf.Max(0f, value.x);
            value.y = Mathf.Max(0f, value.y);
            value.z = Mathf.Max(0f, value.z);
            value.w = Mathf.Max(0f, value.w);
            return value;
        }
    }


    public readonly struct GroundPaintedAccentClusterBuildAuditDiagnostics
    {
        public GroundPaintedAccentClusterBuildAuditDiagnostics(
            int requestedClusters,
            int builtClusters,
            int failedClusters,
            int totalBuildAttempts,
            int successfulAttemptsMin,
            float successfulAttemptsMean,
            int successfulAttemptsMax,
            int succeededOnAttemptOne,
            int succeededOnAttemptsTwoToFour,
            int succeededOnAttemptsFiveToEight,
            int succeededOnAttemptsNineToSixteen,
            int succeededOnAttemptsSeventeenToThirtyTwo,
            int succeededOnAttemptsThirtyThreeToSeventyTwo,
            int failedClustersExhaustingAttemptBudget,
            long externalGlyphCandidatesExamined,
            long externalBoundsTests,
            long externalBoundsOverlapPasses,
            long externalDetailedOverlapTests,
            int externalConflictRejections,
            long externalSpatialQueries,
            long externalGridCellsVisited,
            long externalUniqueCandidatesReturned,
            long externalFullListComparisonsAvoided,
            long reconciliationClustersExamined,
            long reconciliationPreviouslyValidatedRelationshipsSkipped,
            long reconciliationNewIndependentRelationshipsTested,
            long reconciliationLegacyFullListComparisonsAvoided,
            long reconciliationBoundsTests,
            long reconciliationBoundsOverlapPasses,
            long reconciliationDetailedOverlapTests)
        {
            RequestedClusters = Mathf.Max(0, requestedClusters);
            BuiltClusters = Mathf.Max(0, builtClusters);
            FailedClusters = Mathf.Max(0, failedClusters);
            TotalBuildAttempts = Mathf.Max(0, totalBuildAttempts);
            SuccessfulAttemptsMin = Mathf.Max(0, successfulAttemptsMin);
            SuccessfulAttemptsMean = Mathf.Max(0f, successfulAttemptsMean);
            SuccessfulAttemptsMax = Mathf.Max(0, successfulAttemptsMax);
            SucceededOnAttemptOne = Mathf.Max(0, succeededOnAttemptOne);
            SucceededOnAttemptsTwoToFour =
                Mathf.Max(0, succeededOnAttemptsTwoToFour);
            SucceededOnAttemptsFiveToEight =
                Mathf.Max(0, succeededOnAttemptsFiveToEight);
            SucceededOnAttemptsNineToSixteen =
                Mathf.Max(0, succeededOnAttemptsNineToSixteen);
            SucceededOnAttemptsSeventeenToThirtyTwo =
                Mathf.Max(0, succeededOnAttemptsSeventeenToThirtyTwo);
            SucceededOnAttemptsThirtyThreeToSeventyTwo =
                Mathf.Max(0, succeededOnAttemptsThirtyThreeToSeventyTwo);
            FailedClustersExhaustingAttemptBudget =
                Mathf.Max(0, failedClustersExhaustingAttemptBudget);
            ExternalGlyphCandidatesExamined =
                Math.Max(0L, externalGlyphCandidatesExamined);
            ExternalBoundsTests = Math.Max(0L, externalBoundsTests);
            ExternalBoundsOverlapPasses =
                Math.Max(0L, externalBoundsOverlapPasses);
            ExternalDetailedOverlapTests =
                Math.Max(0L, externalDetailedOverlapTests);
            ExternalConflictRejections =
                Mathf.Max(0, externalConflictRejections);
            ExternalSpatialQueries = Math.Max(0L, externalSpatialQueries);
            ExternalGridCellsVisited =
                Math.Max(0L, externalGridCellsVisited);
            ExternalUniqueCandidatesReturned =
                Math.Max(0L, externalUniqueCandidatesReturned);
            ExternalFullListComparisonsAvoided =
                Math.Max(0L, externalFullListComparisonsAvoided);
            ReconciliationClustersExamined =
                Math.Max(0L, reconciliationClustersExamined);
            ReconciliationPreviouslyValidatedRelationshipsSkipped =
                Math.Max(
                    0L,
                    reconciliationPreviouslyValidatedRelationshipsSkipped);
            ReconciliationNewIndependentRelationshipsTested =
                Math.Max(
                    0L,
                    reconciliationNewIndependentRelationshipsTested);
            ReconciliationLegacyFullListComparisonsAvoided =
                Math.Max(
                    0L,
                    reconciliationLegacyFullListComparisonsAvoided);
            ReconciliationBoundsTests =
                Math.Max(0L, reconciliationBoundsTests);
            ReconciliationBoundsOverlapPasses =
                Math.Max(0L, reconciliationBoundsOverlapPasses);
            ReconciliationDetailedOverlapTests =
                Math.Max(0L, reconciliationDetailedOverlapTests);
        }

        public int RequestedClusters { get; }
        public int BuiltClusters { get; }
        public int FailedClusters { get; }
        public int TotalBuildAttempts { get; }
        public int SuccessfulAttemptsMin { get; }
        public float SuccessfulAttemptsMean { get; }
        public int SuccessfulAttemptsMax { get; }
        public int SucceededOnAttemptOne { get; }
        public int SucceededOnAttemptsTwoToFour { get; }
        public int SucceededOnAttemptsFiveToEight { get; }
        public int SucceededOnAttemptsNineToSixteen { get; }
        public int SucceededOnAttemptsSeventeenToThirtyTwo { get; }
        public int SucceededOnAttemptsThirtyThreeToSeventyTwo { get; }
        public int FailedClustersExhaustingAttemptBudget { get; }
        public long ExternalGlyphCandidatesExamined { get; }
        public long ExternalBoundsTests { get; }
        public long ExternalBoundsOverlapPasses { get; }
        public long ExternalDetailedOverlapTests { get; }
        public int ExternalConflictRejections { get; }
        public long ExternalSpatialQueries { get; }
        public long ExternalGridCellsVisited { get; }
        public long ExternalUniqueCandidatesReturned { get; }
        public long ExternalFullListComparisonsAvoided { get; }
        public long ReconciliationClustersExamined { get; }
        public long ReconciliationPreviouslyValidatedRelationshipsSkipped
        {
            get;
        }
        public long ReconciliationNewIndependentRelationshipsTested { get; }
        public long ReconciliationLegacyFullListComparisonsAvoided { get; }
        public long ReconciliationBoundsTests { get; }
        public long ReconciliationBoundsOverlapPasses { get; }
        public long ReconciliationDetailedOverlapTests { get; }

        public static GroundPaintedAccentClusterBuildAuditDiagnostics Empty =>
            default;
    }

    public readonly struct GroundPaintedAccentInternalOverlapAuditDiagnostics
    {
        public GroundPaintedAccentInternalOverlapAuditDiagnostics(
            long methodCalls,
            long finalSilhouetteCalls,
            long segmentPairsConsidered,
            long segmentPairsRejectedByBroadPhase,
            long segmentPairsSentToExactNarrowPhase,
            long exactSegmentIntersectionsFound,
            long exactSweptClearanceRejections)
        {
            MethodCalls = Math.Max(0L, methodCalls);
            FinalSilhouetteCalls = Math.Max(0L, finalSilhouetteCalls);
            SegmentPairsConsidered = Math.Max(0L, segmentPairsConsidered);
            SegmentPairsRejectedByBroadPhase =
                Math.Max(0L, segmentPairsRejectedByBroadPhase);
            SegmentPairsSentToExactNarrowPhase =
                Math.Max(0L, segmentPairsSentToExactNarrowPhase);
            ExactSegmentIntersectionsFound =
                Math.Max(0L, exactSegmentIntersectionsFound);
            ExactSweptClearanceRejections =
                Math.Max(0L, exactSweptClearanceRejections);
        }

        public long MethodCalls { get; }
        public long FinalSilhouetteCalls { get; }
        public long SegmentPairsConsidered { get; }
        public long SegmentPairsRejectedByBroadPhase { get; }
        public long SegmentPairsSentToExactNarrowPhase { get; }
        public long ExactSegmentIntersectionsFound { get; }
        public long ExactSweptClearanceRejections { get; }

        public static GroundPaintedAccentInternalOverlapAuditDiagnostics Empty =>
            default;
    }

    public readonly struct GroundPaintedAccentNearParallelAuditDiagnostics
    {
        public GroundPaintedAccentNearParallelAuditDiagnostics(
            long methodCalls,
            long rightSegmentMetadataPreparations,
            long rightSegmentsPrepared,
            long segmentPairsConsidered,
            long segmentPairsRejectedByAxisGap,
            long segmentPairsRejectedByAlignment,
            long segmentPairsSentToExactDistance,
            long exactDistancePasses,
            long exactIntervalOverlapEvaluations,
            long blendsDetected)
        {
            MethodCalls = Math.Max(0L, methodCalls);
            RightSegmentMetadataPreparations =
                Math.Max(0L, rightSegmentMetadataPreparations);
            RightSegmentsPrepared = Math.Max(0L, rightSegmentsPrepared);
            SegmentPairsConsidered = Math.Max(0L, segmentPairsConsidered);
            SegmentPairsRejectedByAxisGap =
                Math.Max(0L, segmentPairsRejectedByAxisGap);
            SegmentPairsRejectedByAlignment =
                Math.Max(0L, segmentPairsRejectedByAlignment);
            SegmentPairsSentToExactDistance =
                Math.Max(0L, segmentPairsSentToExactDistance);
            ExactDistancePasses = Math.Max(0L, exactDistancePasses);
            ExactIntervalOverlapEvaluations =
                Math.Max(0L, exactIntervalOverlapEvaluations);
            BlendsDetected = Math.Max(0L, blendsDetected);
        }

        public long MethodCalls { get; }
        public long RightSegmentMetadataPreparations { get; }
        public long RightSegmentsPrepared { get; }
        public long SegmentPairsConsidered { get; }
        public long SegmentPairsRejectedByAxisGap { get; }
        public long SegmentPairsRejectedByAlignment { get; }
        public long SegmentPairsSentToExactDistance { get; }
        public long ExactDistancePasses { get; }
        public long ExactIntervalOverlapEvaluations { get; }
        public long BlendsDetected { get; }

        public static GroundPaintedAccentNearParallelAuditDiagnostics Empty =>
            default;
    }

    public readonly struct GroundPaintedAccentProjectedFunnelDiagnostics
    {
        public GroundPaintedAccentProjectedFunnelDiagnostics(
            Vector4 proposalRankProjectedValid,
            int quietProjectedValid,
            int supportingProjectedValid,
            int accentProjectedValid)
        {
            ProposalRankProjectedValid = MaxZero(proposalRankProjectedValid);
            QuietProjectedValid = Mathf.Max(0, quietProjectedValid);
            SupportingProjectedValid = Mathf.Max(0, supportingProjectedValid);
            AccentProjectedValid = Mathf.Max(0, accentProjectedValid);
        }

        public Vector4 ProposalRankProjectedValid { get; }
        public int QuietProjectedValid { get; }
        public int SupportingProjectedValid { get; }
        public int AccentProjectedValid { get; }

        private static Vector4 MaxZero(Vector4 value)
        {
            value.x = Mathf.Max(0f, value.x);
            value.y = Mathf.Max(0f, value.y);
            value.z = Mathf.Max(0f, value.z);
            value.w = Mathf.Max(0f, value.w);
            return value;
        }
    }

    public readonly struct GroundPaintedAccentProjectedGlyphDiagnostics
    {
        public GroundPaintedAccentProjectedGlyphDiagnostics(
            int acceptedBaseDescriptors,
            int projectedGlyphsAccepted,
            int projectedGlyphsRejectedSampling,
            int projectedGlyphsRejectedRiver,
            int projectedGlyphsRejectedModifier,
            int projectedGlyphsRejectedBroadSlope,
            int projectedGlyphsRejectedLocalGrade,
            int projectedGlyphsRejectedFamilyShape,
            int projectedGlyphsRejectedSharpTurn,
            int companionClustersRequested,
            int companionClustersAccepted,
            int companionClustersFallback,
            int companionPairsAccepted,
            int companionTripletsAccepted,
            GroundPaintedAccentCompanionQuotaDiagnostics quotaDiagnostics,
            GroundPaintedAccentClusterBuildAuditDiagnostics clusterBuildAuditDiagnostics,
            GroundPaintedAccentInternalOverlapAuditDiagnostics internalOverlapAuditDiagnostics,
            GroundPaintedAccentNearParallelAuditDiagnostics nearParallelAuditDiagnostics,
            int finalCompanionParticipantCount,
            float finalCompanionParticipantFraction,
            int companionClustersRemovedDuringReconciliation,
            int companionPairStepRetentionRejectedCandidates,
            int companionPairNearCollinearRejectedCandidates,
            int companionPreGeometryStepRetentionRejectedCandidates,
            int companionPreGeometryNonCompetitiveScoreRejectedCandidates,
            int companionCandidatesSentToGeometryValidation,
            int companionNearParallelBodyRejectedCandidates,
            int companionOccupiedAttachmentSlotRejectedCandidates,
            int companionSharedContactLocusRejectedCandidates,
            int companionCrowdedTripletJunctionRejectedCandidates,
            int companionTripletFreeEndPseudoContactRejectedCandidates,
            int companionSeverelyCompressedTripletRejectedCandidates,
            int companionWrongSideTerminalRejectedCandidates,
            int companionSweptWidthInternalOverlapRejectedCandidates,
            int companionPairRejectedIncomplete,
            int companionPairRejectedPrototype,
            int companionPairRejectedContact,
            int companionPairRejectedSurface,
            int companionPairRejectedExternalConflict,
            float companionPairVerticalStepMin,
            float companionPairVerticalStepMean,
            float companionPairVerticalStepMax,
            float companionPairVerticalStepFractionMin,
            float companionPairVerticalStepFractionMean,
            float companionPairVerticalStepFractionMax,
            int companionRejectedIncomplete,
            int companionRejectedPrototype,
            int companionRejectedContact,
            int companionRejectedSurface,
            int companionRejectedExternalConflict,
            GroundPaintedAccentGlyphFamilyStatistics completeMoundStatistics,
            GroundPaintedAccentGlyphFamilyStatistics asymmetricMoundStatistics,
            GroundPaintedAccentGlyphFamilyStatistics singleShoulderStatistics,
            GroundPaintedAccentGlyphFamilyStatistics shallowCrestStatistics,
            Vector2 projectionLocalDirection,
            int projectedPointCountMin,
            float projectedPointCountMean,
            int projectedPointCountMax,
            float projectedCrestTMin,
            float projectedCrestTMean,
            float projectedCrestTMax,
            float crestPeakHeightMin,
            float crestPeakHeightMean,
            float crestPeakHeightMax,
            float crownPeakHeightMin,
            float crownPeakHeightMean,
            float crownPeakHeightMax,
            float combinedPeakHeightMin,
            float combinedPeakHeightMean,
            float combinedPeakHeightMax,
            float projectedTurnDegreesMin,
            float projectedTurnDegreesMean,
            float projectedTurnDegreesMax,
            float maximumNorthDisplacementError,
            float maximumCrossAxisDrift,
            GroundPaintedAccentProjectedFunnelDiagnostics funnelDiagnostics)
        {
            AcceptedBaseDescriptors = Mathf.Max(0, acceptedBaseDescriptors);
            ProjectedGlyphsAccepted = Mathf.Max(0, projectedGlyphsAccepted);
            ProjectedGlyphsRejectedSampling =
                Mathf.Max(0, projectedGlyphsRejectedSampling);
            ProjectedGlyphsRejectedRiver =
                Mathf.Max(0, projectedGlyphsRejectedRiver);
            ProjectedGlyphsRejectedModifier =
                Mathf.Max(0, projectedGlyphsRejectedModifier);
            ProjectedGlyphsRejectedBroadSlope =
                Mathf.Max(0, projectedGlyphsRejectedBroadSlope);
            ProjectedGlyphsRejectedLocalGrade =
                Mathf.Max(0, projectedGlyphsRejectedLocalGrade);
            ProjectedGlyphsRejectedFamilyShape =
                Mathf.Max(0, projectedGlyphsRejectedFamilyShape);
            ProjectedGlyphsRejectedSharpTurn =
                Mathf.Max(0, projectedGlyphsRejectedSharpTurn);
            CompanionClustersRequested =
                Mathf.Max(0, companionClustersRequested);
            CompanionClustersAccepted =
                Mathf.Max(0, companionClustersAccepted);
            CompanionClustersFallback =
                Mathf.Max(0, companionClustersFallback);
            CompanionPairsAccepted = Mathf.Max(0, companionPairsAccepted);
            CompanionTripletsAccepted =
                Mathf.Max(0, companionTripletsAccepted);
            QuotaDiagnostics = quotaDiagnostics;
            ClusterBuildAuditDiagnostics = clusterBuildAuditDiagnostics;
            InternalOverlapAuditDiagnostics = internalOverlapAuditDiagnostics;
            NearParallelAuditDiagnostics = nearParallelAuditDiagnostics;
            FinalCompanionParticipantCount =
                Mathf.Max(0, finalCompanionParticipantCount);
            FinalCompanionParticipantFraction =
                Mathf.Clamp01(finalCompanionParticipantFraction);
            CompanionClustersRemovedDuringReconciliation =
                Mathf.Max(0, companionClustersRemovedDuringReconciliation);
            CompanionPairStepRetentionRejectedCandidates =
                Mathf.Max(0, companionPairStepRetentionRejectedCandidates);
            CompanionPairNearCollinearRejectedCandidates =
                Mathf.Max(0, companionPairNearCollinearRejectedCandidates);
            CompanionPreGeometryStepRetentionRejectedCandidates =
                Mathf.Max(
                    0,
                    companionPreGeometryStepRetentionRejectedCandidates);
            CompanionPreGeometryNonCompetitiveScoreRejectedCandidates =
                Mathf.Max(
                    0,
                    companionPreGeometryNonCompetitiveScoreRejectedCandidates);
            CompanionCandidatesSentToGeometryValidation =
                Mathf.Max(0, companionCandidatesSentToGeometryValidation);
            CompanionNearParallelBodyRejectedCandidates =
                Mathf.Max(0, companionNearParallelBodyRejectedCandidates);
            CompanionOccupiedAttachmentSlotRejectedCandidates =
                Mathf.Max(
                    0,
                    companionOccupiedAttachmentSlotRejectedCandidates);
            CompanionSharedContactLocusRejectedCandidates =
                Mathf.Max(0, companionSharedContactLocusRejectedCandidates);
            CompanionCrowdedTripletJunctionRejectedCandidates =
                Mathf.Max(
                    0,
                    companionCrowdedTripletJunctionRejectedCandidates);
            CompanionTripletFreeEndPseudoContactRejectedCandidates =
                Mathf.Max(
                    0,
                    companionTripletFreeEndPseudoContactRejectedCandidates);
            CompanionSeverelyCompressedTripletRejectedCandidates =
                Mathf.Max(
                    0,
                    companionSeverelyCompressedTripletRejectedCandidates);
            CompanionWrongSideTerminalRejectedCandidates =
                Mathf.Max(0, companionWrongSideTerminalRejectedCandidates);
            CompanionSweptWidthInternalOverlapRejectedCandidates =
                Mathf.Max(
                    0,
                    companionSweptWidthInternalOverlapRejectedCandidates);
            CompanionPairRejectedIncomplete =
                Mathf.Max(0, companionPairRejectedIncomplete);
            CompanionPairRejectedPrototype =
                Mathf.Max(0, companionPairRejectedPrototype);
            CompanionPairRejectedContact =
                Mathf.Max(0, companionPairRejectedContact);
            CompanionPairRejectedSurface =
                Mathf.Max(0, companionPairRejectedSurface);
            CompanionPairRejectedExternalConflict =
                Mathf.Max(0, companionPairRejectedExternalConflict);
            CompanionPairVerticalStepMin =
                Mathf.Max(0f, companionPairVerticalStepMin);
            CompanionPairVerticalStepMean =
                Mathf.Max(0f, companionPairVerticalStepMean);
            CompanionPairVerticalStepMax =
                Mathf.Max(0f, companionPairVerticalStepMax);
            CompanionPairVerticalStepFractionMin =
                Mathf.Max(0f, companionPairVerticalStepFractionMin);
            CompanionPairVerticalStepFractionMean =
                Mathf.Max(0f, companionPairVerticalStepFractionMean);
            CompanionPairVerticalStepFractionMax =
                Mathf.Max(0f, companionPairVerticalStepFractionMax);
            CompanionRejectedIncomplete =
                Mathf.Max(0, companionRejectedIncomplete);
            CompanionRejectedPrototype =
                Mathf.Max(0, companionRejectedPrototype);
            CompanionRejectedContact =
                Mathf.Max(0, companionRejectedContact);
            CompanionRejectedSurface =
                Mathf.Max(0, companionRejectedSurface);
            CompanionRejectedExternalConflict =
                Mathf.Max(0, companionRejectedExternalConflict);
            CompleteMoundStatistics = completeMoundStatistics;
            AsymmetricMoundStatistics = asymmetricMoundStatistics;
            SingleShoulderStatistics = singleShoulderStatistics;
            ShallowCrestStatistics = shallowCrestStatistics;
            ProjectionLocalDirection =
                projectionLocalDirection.sqrMagnitude > 0.000001f
                    ? projectionLocalDirection.normalized
                    : Vector2.up;
            ProjectedPointCountMin = Mathf.Max(0, projectedPointCountMin);
            ProjectedPointCountMean = Mathf.Max(0f, projectedPointCountMean);
            ProjectedPointCountMax = Mathf.Max(0, projectedPointCountMax);
            ProjectedCrestTMin = Mathf.Clamp01(projectedCrestTMin);
            ProjectedCrestTMean = Mathf.Clamp01(projectedCrestTMean);
            ProjectedCrestTMax = Mathf.Clamp01(projectedCrestTMax);
            CrestPeakHeightMin = Mathf.Max(0f, crestPeakHeightMin);
            CrestPeakHeightMean = Mathf.Max(0f, crestPeakHeightMean);
            CrestPeakHeightMax = Mathf.Max(0f, crestPeakHeightMax);
            CrownPeakHeightMin = Mathf.Max(0f, crownPeakHeightMin);
            CrownPeakHeightMean = Mathf.Max(0f, crownPeakHeightMean);
            CrownPeakHeightMax = Mathf.Max(0f, crownPeakHeightMax);
            CombinedPeakHeightMin = Mathf.Max(0f, combinedPeakHeightMin);
            CombinedPeakHeightMean = Mathf.Max(0f, combinedPeakHeightMean);
            CombinedPeakHeightMax = Mathf.Max(0f, combinedPeakHeightMax);
            ProjectedTurnDegreesMin = Mathf.Max(0f, projectedTurnDegreesMin);
            ProjectedTurnDegreesMean = Mathf.Max(0f, projectedTurnDegreesMean);
            ProjectedTurnDegreesMax = Mathf.Max(0f, projectedTurnDegreesMax);
            MaximumNorthDisplacementError =
                Mathf.Max(0f, maximumNorthDisplacementError);
            MaximumCrossAxisDrift = Mathf.Max(0f, maximumCrossAxisDrift);
            FunnelDiagnostics = funnelDiagnostics;
        }

        public int AcceptedBaseDescriptors { get; }
        public int ProjectedGlyphsAccepted { get; }
        public int ProjectedGlyphsRejectedSampling { get; }
        public int ProjectedGlyphsRejectedRiver { get; }
        public int ProjectedGlyphsRejectedModifier { get; }
        public int ProjectedGlyphsRejectedBroadSlope { get; }
        public int ProjectedGlyphsRejectedLocalGrade { get; }
        public int ProjectedGlyphsRejectedFamilyShape { get; }
        public int ProjectedGlyphsRejectedSharpTurn { get; }
        public int CompanionClustersRequested { get; }
        public int CompanionClustersAccepted { get; }
        public int CompanionClustersFallback { get; }
        public int CompanionPairsAccepted { get; }
        public int CompanionTripletsAccepted { get; }
        public GroundPaintedAccentCompanionQuotaDiagnostics QuotaDiagnostics
        {
            get;
        }
        public GroundPaintedAccentClusterBuildAuditDiagnostics
            ClusterBuildAuditDiagnostics
        {
            get;
        }
        public GroundPaintedAccentInternalOverlapAuditDiagnostics
            InternalOverlapAuditDiagnostics
        {
            get;
        }
        public GroundPaintedAccentNearParallelAuditDiagnostics
            NearParallelAuditDiagnostics
        {
            get;
        }
        public int FinalCompanionParticipantCount { get; }
        public float FinalCompanionParticipantFraction { get; }
        public int CompanionClustersRemovedDuringReconciliation { get; }
        public int CompanionPairStepRetentionRejectedCandidates { get; }
        public int CompanionPairNearCollinearRejectedCandidates { get; }
        public int CompanionPreGeometryStepRetentionRejectedCandidates
        {
            get;
        }
        public int CompanionPreGeometryNonCompetitiveScoreRejectedCandidates
        {
            get;
        }
        public int CompanionCandidatesSentToGeometryValidation { get; }
        public int CompanionNearParallelBodyRejectedCandidates { get; }
        public int CompanionOccupiedAttachmentSlotRejectedCandidates { get; }
        public int CompanionSharedContactLocusRejectedCandidates { get; }
        public int CompanionCrowdedTripletJunctionRejectedCandidates { get; }
        public int CompanionTripletFreeEndPseudoContactRejectedCandidates
        {
            get;
        }
        public int CompanionSeverelyCompressedTripletRejectedCandidates
        {
            get;
        }
        public int CompanionWrongSideTerminalRejectedCandidates { get; }
        public int CompanionSweptWidthInternalOverlapRejectedCandidates
        {
            get;
        }
        public int CompanionPairRejectedIncomplete { get; }
        public int CompanionPairRejectedPrototype { get; }
        public int CompanionPairRejectedContact { get; }
        public int CompanionPairRejectedSurface { get; }
        public int CompanionPairRejectedExternalConflict { get; }
        public float CompanionPairVerticalStepMin { get; }
        public float CompanionPairVerticalStepMean { get; }
        public float CompanionPairVerticalStepMax { get; }
        public float CompanionPairVerticalStepFractionMin { get; }
        public float CompanionPairVerticalStepFractionMean { get; }
        public float CompanionPairVerticalStepFractionMax { get; }
        public int CompanionRejectedIncomplete { get; }
        public int CompanionRejectedPrototype { get; }
        public int CompanionRejectedContact { get; }
        public int CompanionRejectedSurface { get; }
        public int CompanionRejectedExternalConflict { get; }
        public GroundPaintedAccentGlyphFamilyStatistics CompleteMoundStatistics { get; }
        public GroundPaintedAccentGlyphFamilyStatistics AsymmetricMoundStatistics { get; }
        public GroundPaintedAccentGlyphFamilyStatistics SingleShoulderStatistics { get; }
        public GroundPaintedAccentGlyphFamilyStatistics ShallowCrestStatistics { get; }
        public Vector2 ProjectionLocalDirection { get; }
        public int ProjectedPointCountMin { get; }
        public float ProjectedPointCountMean { get; }
        public int ProjectedPointCountMax { get; }
        public float ProjectedCrestTMin { get; }
        public float ProjectedCrestTMean { get; }
        public float ProjectedCrestTMax { get; }
        public float CrestPeakHeightMin { get; }
        public float CrestPeakHeightMean { get; }
        public float CrestPeakHeightMax { get; }
        public float CrownPeakHeightMin { get; }
        public float CrownPeakHeightMean { get; }
        public float CrownPeakHeightMax { get; }
        public float CombinedPeakHeightMin { get; }
        public float CombinedPeakHeightMean { get; }
        public float CombinedPeakHeightMax { get; }
        public float ProjectedTurnDegreesMin { get; }
        public float ProjectedTurnDegreesMean { get; }
        public float ProjectedTurnDegreesMax { get; }
        public float MaximumNorthDisplacementError { get; }
        public float MaximumCrossAxisDrift { get; }
        public GroundPaintedAccentProjectedFunnelDiagnostics FunnelDiagnostics
        {
            get;
        }

        public int ProjectedGlyphsRejectedTotal =>
            ProjectedGlyphsRejectedSampling +
            ProjectedGlyphsRejectedRiver +
            ProjectedGlyphsRejectedModifier +
            ProjectedGlyphsRejectedBroadSlope +
            ProjectedGlyphsRejectedLocalGrade +
            ProjectedGlyphsRejectedFamilyShape +
            ProjectedGlyphsRejectedSharpTurn;

        public static GroundPaintedAccentProjectedGlyphDiagnostics Empty =>
            default;
    }

    public readonly struct GroundPaintedAccentProjectedGlyphBuildTimings
    {
        public GroundPaintedAccentProjectedGlyphBuildTimings(
            double profileBuildMilliseconds,
            double familyValidationMilliseconds,
            double pointConstructionMilliseconds,
            double topologyValidationMilliseconds,
            double clusterCompositionMilliseconds,
            double clusterAuditTotalMilliseconds,
            double clusterQuotaPreparationMilliseconds,
            double clusterParticipantSelectionMilliseconds,
            double clusterDonorSelectionMilliseconds,
            double clusterPrototypePreparationMilliseconds,
            double clusterContactSolvingMilliseconds,
            double clusterNearParallelValidationMilliseconds,
            double clusterCandidateInternalOverlapValidationMilliseconds,
            double clusterFinalSilhouetteOverlapValidationMilliseconds,
            double clusterInternalValidationMilliseconds,
            double clusterSurfaceValidationMilliseconds,
            double clusterExternalConflictMilliseconds,
            double clusterCommitMilliseconds,
            double clusterReconciliationMilliseconds,
            double surfaceValidationMilliseconds,
            double footprintPreparationMilliseconds,
            double groundSamplingMilliseconds,
            double broadSlopeMilliseconds,
            double riverExclusionMilliseconds,
            double modifierExclusionMilliseconds,
            double transverseGradeMilliseconds,
            double longitudinalGradeMilliseconds,
            double diagnosticsMilliseconds)
        {
            ProfileBuildMilliseconds = Math.Max(0d, profileBuildMilliseconds);
            FamilyValidationMilliseconds = Math.Max(0d, familyValidationMilliseconds);
            PointConstructionMilliseconds = Math.Max(0d, pointConstructionMilliseconds);
            TopologyValidationMilliseconds = Math.Max(0d, topologyValidationMilliseconds);
            ClusterCompositionMilliseconds = Math.Max(0d, clusterCompositionMilliseconds);
            ClusterAuditTotalMilliseconds = Math.Max(0d, clusterAuditTotalMilliseconds);
            ClusterQuotaPreparationMilliseconds =
                Math.Max(0d, clusterQuotaPreparationMilliseconds);
            ClusterParticipantSelectionMilliseconds =
                Math.Max(0d, clusterParticipantSelectionMilliseconds);
            ClusterDonorSelectionMilliseconds =
                Math.Max(0d, clusterDonorSelectionMilliseconds);
            ClusterPrototypePreparationMilliseconds =
                Math.Max(0d, clusterPrototypePreparationMilliseconds);
            ClusterContactSolvingMilliseconds =
                Math.Max(0d, clusterContactSolvingMilliseconds);
            ClusterNearParallelValidationMilliseconds =
                Math.Max(0d, clusterNearParallelValidationMilliseconds);
            ClusterCandidateInternalOverlapValidationMilliseconds =
                Math.Max(
                    0d,
                    clusterCandidateInternalOverlapValidationMilliseconds);
            ClusterFinalSilhouetteOverlapValidationMilliseconds =
                Math.Max(
                    0d,
                    clusterFinalSilhouetteOverlapValidationMilliseconds);
            ClusterContactPlacementMilliseconds =
                Math.Max(
                    0d,
                    ClusterContactSolvingMilliseconds -
                    ClusterNearParallelValidationMilliseconds -
                    ClusterCandidateInternalOverlapValidationMilliseconds);
            ClusterInternalValidationMilliseconds =
                Math.Max(0d, clusterInternalValidationMilliseconds);
            ClusterSurfaceValidationMilliseconds =
                Math.Max(0d, clusterSurfaceValidationMilliseconds);
            ClusterExternalConflictMilliseconds =
                Math.Max(0d, clusterExternalConflictMilliseconds);
            ClusterCommitMilliseconds = Math.Max(0d, clusterCommitMilliseconds);
            ClusterReconciliationMilliseconds =
                Math.Max(0d, clusterReconciliationMilliseconds);
            SurfaceValidationMilliseconds = Math.Max(0d, surfaceValidationMilliseconds);
            FootprintPreparationMilliseconds = Math.Max(0d, footprintPreparationMilliseconds);
            GroundSamplingMilliseconds = Math.Max(0d, groundSamplingMilliseconds);
            BroadSlopeMilliseconds = Math.Max(0d, broadSlopeMilliseconds);
            RiverExclusionMilliseconds = Math.Max(0d, riverExclusionMilliseconds);
            ModifierExclusionMilliseconds = Math.Max(0d, modifierExclusionMilliseconds);
            TransverseGradeMilliseconds = Math.Max(0d, transverseGradeMilliseconds);
            LongitudinalGradeMilliseconds = Math.Max(0d, longitudinalGradeMilliseconds);
            DiagnosticsMilliseconds = Math.Max(0d, diagnosticsMilliseconds);
        }

        public double ProfileBuildMilliseconds { get; }
        public double FamilyValidationMilliseconds { get; }
        public double PointConstructionMilliseconds { get; }
        public double TopologyValidationMilliseconds { get; }
        public double ClusterCompositionMilliseconds { get; }
        public double ClusterAuditTotalMilliseconds { get; }
        public double ClusterQuotaPreparationMilliseconds { get; }
        public double ClusterParticipantSelectionMilliseconds { get; }
        public double ClusterDonorSelectionMilliseconds { get; }
        public double ClusterPrototypePreparationMilliseconds { get; }
        public double ClusterContactSolvingMilliseconds { get; }
        public double ClusterContactPlacementMilliseconds { get; }
        public double ClusterNearParallelValidationMilliseconds { get; }
        public double ClusterCandidateInternalOverlapValidationMilliseconds
        {
            get;
        }
        public double ClusterFinalSilhouetteOverlapValidationMilliseconds
        {
            get;
        }
        public double ClusterInternalValidationMilliseconds { get; }
        public double ClusterSurfaceValidationMilliseconds { get; }
        public double ClusterExternalConflictMilliseconds { get; }
        public double ClusterCommitMilliseconds { get; }
        public double ClusterReconciliationMilliseconds { get; }
        public double SurfaceValidationMilliseconds { get; }
        public double FootprintPreparationMilliseconds { get; }
        public double GroundSamplingMilliseconds { get; }
        public double BroadSlopeMilliseconds { get; }
        public double RiverExclusionMilliseconds { get; }
        public double ModifierExclusionMilliseconds { get; }
        public double TransverseGradeMilliseconds { get; }
        public double LongitudinalGradeMilliseconds { get; }
        public double DiagnosticsMilliseconds { get; }

        public static GroundPaintedAccentProjectedGlyphBuildTimings Empty =>
            default;
    }

    internal sealed class GroundPaintedAccentProjectedGlyphTimingAccumulator
    {
        public long ProfileBuildTicks;
        public long FamilyValidationTicks;
        public long PointConstructionTicks;
        public long TopologyValidationTicks;
        public long ClusterCompositionTicks;
        public long ClusterAuditTotalTicks;
        public long ClusterQuotaPreparationTicks;
        public long ClusterParticipantSelectionTicks;
        public long ClusterDonorSelectionTicks;
        public long ClusterPrototypePreparationTicks;
        public long ClusterContactSolvingTicks;
        public long ClusterNearParallelValidationTicks;
        public long ClusterCandidateInternalOverlapValidationTicks;
        public long ClusterFinalSilhouetteOverlapValidationTicks;
        public long ClusterInternalValidationTicks;
        public long ClusterSurfaceValidationTicks;
        public long ClusterExternalConflictTicks;
        public long ClusterCommitTicks;
        public long ClusterReconciliationTicks;
        public long SurfaceValidationTicks;
        public long FootprintPreparationTicks;
        public long GroundSamplingTicks;
        public long BroadSlopeTicks;
        public long RiverExclusionTicks;
        public long ModifierExclusionTicks;
        public long TransverseGradeTicks;
        public long LongitudinalGradeTicks;
        public long DiagnosticsTicks;

        public GroundPaintedAccentProjectedGlyphBuildTimings Resolve()
        {
            double tickToMilliseconds =
                1000d / System.Diagnostics.Stopwatch.Frequency;
            return new GroundPaintedAccentProjectedGlyphBuildTimings(
                ProfileBuildTicks * tickToMilliseconds,
                FamilyValidationTicks * tickToMilliseconds,
                PointConstructionTicks * tickToMilliseconds,
                TopologyValidationTicks * tickToMilliseconds,
                ClusterCompositionTicks * tickToMilliseconds,
                ClusterAuditTotalTicks * tickToMilliseconds,
                ClusterQuotaPreparationTicks * tickToMilliseconds,
                ClusterParticipantSelectionTicks * tickToMilliseconds,
                ClusterDonorSelectionTicks * tickToMilliseconds,
                ClusterPrototypePreparationTicks * tickToMilliseconds,
                ClusterContactSolvingTicks * tickToMilliseconds,
                ClusterNearParallelValidationTicks * tickToMilliseconds,
                ClusterCandidateInternalOverlapValidationTicks *
                    tickToMilliseconds,
                ClusterFinalSilhouetteOverlapValidationTicks *
                    tickToMilliseconds,
                ClusterInternalValidationTicks * tickToMilliseconds,
                ClusterSurfaceValidationTicks * tickToMilliseconds,
                ClusterExternalConflictTicks * tickToMilliseconds,
                ClusterCommitTicks * tickToMilliseconds,
                ClusterReconciliationTicks * tickToMilliseconds,
                SurfaceValidationTicks * tickToMilliseconds,
                FootprintPreparationTicks * tickToMilliseconds,
                GroundSamplingTicks * tickToMilliseconds,
                BroadSlopeTicks * tickToMilliseconds,
                RiverExclusionTicks * tickToMilliseconds,
                ModifierExclusionTicks * tickToMilliseconds,
                TransverseGradeTicks * tickToMilliseconds,
                LongitudinalGradeTicks * tickToMilliseconds,
                DiagnosticsTicks * tickToMilliseconds);
        }
    }

    internal sealed class GroundPaintedAccentClusterBuildAuditAccumulator
    {
        private int successfulClusterCount;
        private int successfulAttemptMinimum = int.MaxValue;
        private int successfulAttemptMaximum;
        private long successfulAttemptTotal;
        private int succeededOnAttemptOne;
        private int succeededOnAttemptsTwoToFour;
        private int succeededOnAttemptsFiveToEight;
        private int succeededOnAttemptsNineToSixteen;
        private int succeededOnAttemptsSeventeenToThirtyTwo;
        private int succeededOnAttemptsThirtyThreeToSeventyTwo;
        private int failedClustersExhaustingAttemptBudget;

        public long ExternalGlyphCandidatesExamined;
        public long ExternalBoundsTests;
        public long ExternalBoundsOverlapPasses;
        public long ExternalDetailedOverlapTests;
        public int ExternalConflictRejections;
        public long ExternalSpatialQueries;
        public long ExternalGridCellsVisited;
        public long ExternalUniqueCandidatesReturned;
        public long ExternalFullListComparisonsAvoided;
        public long ReconciliationClustersExamined;
        public long ReconciliationPreviouslyValidatedRelationshipsSkipped;
        public long ReconciliationNewIndependentRelationshipsTested;
        public long ReconciliationLegacyFullListComparisonsAvoided;
        public long ReconciliationBoundsTests;
        public long ReconciliationBoundsOverlapPasses;
        public long ReconciliationDetailedOverlapTests;

        public void RecordBuildResult(
            bool built,
            int attempts,
            bool exhaustedAttemptBudget)
        {
            int safeAttempts = Mathf.Max(0, attempts);
            if (!built)
            {
                if (exhaustedAttemptBudget)
                {
                    failedClustersExhaustingAttemptBudget++;
                }
                return;
            }

            successfulClusterCount++;
            successfulAttemptMinimum =
                Mathf.Min(successfulAttemptMinimum, safeAttempts);
            successfulAttemptMaximum =
                Mathf.Max(successfulAttemptMaximum, safeAttempts);
            successfulAttemptTotal += safeAttempts;
            if (safeAttempts <= 1)
            {
                succeededOnAttemptOne++;
            }
            else if (safeAttempts <= 4)
            {
                succeededOnAttemptsTwoToFour++;
            }
            else if (safeAttempts <= 8)
            {
                succeededOnAttemptsFiveToEight++;
            }
            else if (safeAttempts <= 16)
            {
                succeededOnAttemptsNineToSixteen++;
            }
            else if (safeAttempts <= 32)
            {
                succeededOnAttemptsSeventeenToThirtyTwo++;
            }
            else
            {
                succeededOnAttemptsThirtyThreeToSeventyTwo++;
            }
        }

        public GroundPaintedAccentClusterBuildAuditDiagnostics Resolve(
            int requestedClusters,
            int totalBuildAttempts)
        {
            int resolvedSuccessfulMinimum =
                successfulClusterCount > 0
                    ? successfulAttemptMinimum
                    : 0;
            float successfulAttemptMean =
                successfulClusterCount > 0
                    ? successfulAttemptTotal / (float)successfulClusterCount
                    : 0f;
            return new GroundPaintedAccentClusterBuildAuditDiagnostics(
                requestedClusters,
                successfulClusterCount,
                Mathf.Max(0, requestedClusters - successfulClusterCount),
                totalBuildAttempts,
                resolvedSuccessfulMinimum,
                successfulAttemptMean,
                successfulAttemptMaximum,
                succeededOnAttemptOne,
                succeededOnAttemptsTwoToFour,
                succeededOnAttemptsFiveToEight,
                succeededOnAttemptsNineToSixteen,
                succeededOnAttemptsSeventeenToThirtyTwo,
                succeededOnAttemptsThirtyThreeToSeventyTwo,
                failedClustersExhaustingAttemptBudget,
                ExternalGlyphCandidatesExamined,
                ExternalBoundsTests,
                ExternalBoundsOverlapPasses,
                ExternalDetailedOverlapTests,
                ExternalConflictRejections,
                ExternalSpatialQueries,
                ExternalGridCellsVisited,
                ExternalUniqueCandidatesReturned,
                ExternalFullListComparisonsAvoided,
                ReconciliationClustersExamined,
                ReconciliationPreviouslyValidatedRelationshipsSkipped,
                ReconciliationNewIndependentRelationshipsTested,
                ReconciliationLegacyFullListComparisonsAvoided,
                ReconciliationBoundsTests,
                ReconciliationBoundsOverlapPasses,
                ReconciliationDetailedOverlapTests);
        }
    }

    internal sealed class GroundPaintedAccentInternalOverlapAuditAccumulator
    {
        public long MethodCalls;
        public long FinalSilhouetteCalls;
        public long SegmentPairsConsidered;
        public long SegmentPairsRejectedByBroadPhase;
        public long SegmentPairsSentToExactNarrowPhase;
        public long ExactSegmentIntersectionsFound;
        public long ExactSweptClearanceRejections;

        public void Accumulate(
            bool finalSilhouetteCall,
            long segmentPairsConsidered,
            long segmentPairsRejectedByBroadPhase,
            long segmentPairsSentToExactNarrowPhase,
            long exactSegmentIntersectionsFound,
            long exactSweptClearanceRejections)
        {
            MethodCalls++;
            if (finalSilhouetteCall)
            {
                FinalSilhouetteCalls++;
            }

            SegmentPairsConsidered +=
                Math.Max(0L, segmentPairsConsidered);
            SegmentPairsRejectedByBroadPhase +=
                Math.Max(0L, segmentPairsRejectedByBroadPhase);
            SegmentPairsSentToExactNarrowPhase +=
                Math.Max(0L, segmentPairsSentToExactNarrowPhase);
            ExactSegmentIntersectionsFound +=
                Math.Max(0L, exactSegmentIntersectionsFound);
            ExactSweptClearanceRejections +=
                Math.Max(0L, exactSweptClearanceRejections);
        }

        public GroundPaintedAccentInternalOverlapAuditDiagnostics Resolve()
        {
            return new GroundPaintedAccentInternalOverlapAuditDiagnostics(
                MethodCalls,
                FinalSilhouetteCalls,
                SegmentPairsConsidered,
                SegmentPairsRejectedByBroadPhase,
                SegmentPairsSentToExactNarrowPhase,
                ExactSegmentIntersectionsFound,
                ExactSweptClearanceRejections);
        }
    }

    internal sealed class GroundPaintedAccentNearParallelAuditAccumulator
    {
        public long MethodCalls;
        public long RightSegmentMetadataPreparations;
        public long RightSegmentsPrepared;
        public long SegmentPairsConsidered;
        public long SegmentPairsRejectedByAxisGap;
        public long SegmentPairsRejectedByAlignment;
        public long SegmentPairsSentToExactDistance;
        public long ExactDistancePasses;
        public long ExactIntervalOverlapEvaluations;
        public long BlendsDetected;

        public void Accumulate(
            long rightSegmentMetadataPreparations,
            long rightSegmentsPrepared,
            long segmentPairsConsidered,
            long segmentPairsRejectedByAxisGap,
            long segmentPairsRejectedByAlignment,
            long segmentPairsSentToExactDistance,
            long exactDistancePasses,
            long exactIntervalOverlapEvaluations,
            long blendsDetected)
        {
            MethodCalls++;
            RightSegmentMetadataPreparations +=
                Math.Max(0L, rightSegmentMetadataPreparations);
            RightSegmentsPrepared += Math.Max(0L, rightSegmentsPrepared);
            SegmentPairsConsidered += Math.Max(0L, segmentPairsConsidered);
            SegmentPairsRejectedByAxisGap +=
                Math.Max(0L, segmentPairsRejectedByAxisGap);
            SegmentPairsRejectedByAlignment +=
                Math.Max(0L, segmentPairsRejectedByAlignment);
            SegmentPairsSentToExactDistance +=
                Math.Max(0L, segmentPairsSentToExactDistance);
            ExactDistancePasses += Math.Max(0L, exactDistancePasses);
            ExactIntervalOverlapEvaluations +=
                Math.Max(0L, exactIntervalOverlapEvaluations);
            BlendsDetected += Math.Max(0L, blendsDetected);
        }

        public GroundPaintedAccentNearParallelAuditDiagnostics Resolve()
        {
            return new GroundPaintedAccentNearParallelAuditDiagnostics(
                MethodCalls,
                RightSegmentMetadataPreparations,
                RightSegmentsPrepared,
                SegmentPairsConsidered,
                SegmentPairsRejectedByAxisGap,
                SegmentPairsRejectedByAlignment,
                SegmentPairsSentToExactDistance,
                ExactDistancePasses,
                ExactIntervalOverlapEvaluations,
                BlendsDetected);
        }
    }

    internal readonly struct GroundPaintedAccentRiverExclusionSnapshot
    {
        public GroundPaintedAccentRiverExclusionSnapshot(
            StylizedRiverGroundSnapshot snapshot,
            Vector2 boundsMinimum,
            Vector2 boundsMaximum,
            bool hasBounds)
        {
            Snapshot = snapshot;
            BoundsMinimum = boundsMinimum;
            BoundsMaximum = boundsMaximum;
            HasBounds = hasBounds;
        }

        public StylizedRiverGroundSnapshot Snapshot { get; }
        public Vector2 BoundsMinimum { get; }
        public Vector2 BoundsMaximum { get; }
        public bool HasBounds { get; }

        public bool Intersects(
            Vector2 minimum,
            Vector2 maximum,
            float expansion)
        {
            if (!Snapshot.IsValid)
            {
                return false;
            }

            if (!HasBounds)
            {
                return true;
            }

            float safeExpansion = Mathf.Max(0f, expansion);
            return maximum.x >= BoundsMinimum.x - safeExpansion &&
                   minimum.x <= BoundsMaximum.x + safeExpansion &&
                   maximum.y >= BoundsMinimum.y - safeExpansion &&
                   minimum.y <= BoundsMaximum.y + safeExpansion;
        }
    }

    internal readonly struct GroundPaintedAccentNearParallelSegmentMetadata
    {
        public GroundPaintedAccentNearParallelSegmentMetadata(
            Vector2 start,
            Vector2 end,
            float startHalfWidth,
            float endHalfWidth)
        {
            Start = start;
            End = end;
            Vector2 delta = end - start;
            Length = delta.magnitude;
            IsValid = Length > 0.000001f;
            Direction = IsValid ? delta / Length : Vector2.zero;
            AverageHalfWidth =
                (startHalfWidth + endHalfWidth) * 0.5f;
            MinimumX = Mathf.Min(start.x, end.x);
            MaximumX = Mathf.Max(start.x, end.x);
            MinimumY = Mathf.Min(start.y, end.y);
            MaximumY = Mathf.Max(start.y, end.y);
        }

        public Vector2 Start { get; }
        public Vector2 End { get; }
        public Vector2 Direction { get; }
        public float Length { get; }
        public float AverageHalfWidth { get; }
        public float MinimumX { get; }
        public float MaximumX { get; }
        public float MinimumY { get; }
        public float MaximumY { get; }
        public bool IsValid { get; }
    }

    internal readonly struct GroundPaintedAccentInternalOverlapSegmentMetadata
    {
        public GroundPaintedAccentInternalOverlapSegmentMetadata(
            Vector2 start,
            Vector2 end,
            float startHalfWidth,
            float endHalfWidth)
        {
            Start = start;
            End = end;
            StartHalfWidth = startHalfWidth;
            EndHalfWidth = endHalfWidth;
            MaximumHalfWidth =
                Mathf.Max(0f, Mathf.Max(startHalfWidth, endHalfWidth));
            MinimumX = Mathf.Min(start.x, end.x);
            MaximumX = Mathf.Max(start.x, end.x);
            MinimumY = Mathf.Min(start.y, end.y);
            MaximumY = Mathf.Max(start.y, end.y);
        }

        public Vector2 Start { get; }
        public Vector2 End { get; }
        public float StartHalfWidth { get; }
        public float EndHalfWidth { get; }
        public float MaximumHalfWidth { get; }
        public float MinimumX { get; }
        public float MaximumX { get; }
        public float MinimumY { get; }
        public float MaximumY { get; }
    }

    internal sealed class GroundPaintedAccentProjectedGlyphScratch
    {
        public Vector2[] ProjectedPoints = Array.Empty<Vector2>();
        public float[] HalfWidths = Array.Empty<float>();
        public Vector2[] LeftPoints = Array.Empty<Vector2>();
        public Vector2[] RightPoints = Array.Empty<Vector2>();
        public Vector3[] SurfacePoints = Array.Empty<Vector3>();
        public int[] RiverIndices = Array.Empty<int>();
        public int[] ModifierIndices = Array.Empty<int>();
        public GroundPaintedAccentNearParallelSegmentMetadata[]
            NearParallelRightSegments =
                Array.Empty<GroundPaintedAccentNearParallelSegmentMetadata>();
        public GroundPaintedAccentInternalOverlapSegmentMetadata[]
            InternalOverlapLeftSegments =
                Array.Empty<GroundPaintedAccentInternalOverlapSegmentMetadata>();
        public GroundPaintedAccentInternalOverlapSegmentMetadata[]
            InternalOverlapRightSegments =
                Array.Empty<GroundPaintedAccentInternalOverlapSegmentMetadata>();

        public void EnsurePointCapacity(int required)
        {
            if (ProjectedPoints.Length >= required)
            {
                return;
            }

            int capacity = Mathf.NextPowerOfTwo(Mathf.Max(2, required));
            ProjectedPoints = new Vector2[capacity];
            HalfWidths = new float[capacity];
            LeftPoints = new Vector2[capacity];
            RightPoints = new Vector2[capacity];
            SurfacePoints = new Vector3[capacity];
        }

        public void EnsureNearParallelSegmentCapacity(int required)
        {
            if (NearParallelRightSegments.Length >= required)
            {
                return;
            }

            int capacity = Mathf.NextPowerOfTwo(Mathf.Max(2, required));
            NearParallelRightSegments =
                new GroundPaintedAccentNearParallelSegmentMetadata[capacity];
        }

        public void EnsureInternalOverlapSegmentCapacity(
            int requiredLeft,
            int requiredRight)
        {
            if (InternalOverlapLeftSegments.Length < requiredLeft)
            {
                int capacity =
                    Mathf.NextPowerOfTwo(Mathf.Max(2, requiredLeft));
                InternalOverlapLeftSegments =
                    new GroundPaintedAccentInternalOverlapSegmentMetadata[
                        capacity];
            }

            if (InternalOverlapRightSegments.Length < requiredRight)
            {
                int capacity =
                    Mathf.NextPowerOfTwo(Mathf.Max(2, requiredRight));
                InternalOverlapRightSegments =
                    new GroundPaintedAccentInternalOverlapSegmentMetadata[
                        capacity];
            }
        }

        public void EnsureRiverCapacity(int required)
        {
            if (RiverIndices.Length < required)
            {
                RiverIndices = new int[Mathf.NextPowerOfTwo(Mathf.Max(1, required))];
            }
        }

        public void EnsureModifierCapacity(int required)
        {
            if (ModifierIndices.Length < required)
            {
                ModifierIndices = new int[Mathf.NextPowerOfTwo(Mathf.Max(1, required))];
            }
        }
    }

    public readonly struct GroundPaintedAccentProjectedGlyphDebugSnapshot
    {
        public GroundPaintedAccentProjectedGlyphDebugSnapshot(
            GroundPaintedAccentProjectedGlyph[] glyphs,
            GroundPaintedAccentProjectedGlyphRejectionDebugPoint[] rejections,
            GroundPaintedAccentProjectedGlyphDiagnostics diagnostics)
        {
            Glyphs = glyphs ?? Array.Empty<GroundPaintedAccentProjectedGlyph>();
            Rejections =
                rejections ??
                Array.Empty<GroundPaintedAccentProjectedGlyphRejectionDebugPoint>();
            Diagnostics = diagnostics;
            isValid = true;
        }

        public GroundPaintedAccentProjectedGlyph[] Glyphs { get; }
        public GroundPaintedAccentProjectedGlyphRejectionDebugPoint[] Rejections
        {
            get;
        }
        public GroundPaintedAccentProjectedGlyphDiagnostics Diagnostics { get; }

        private readonly bool isValid;

        public bool IsValid =>
            isValid && Glyphs != null && Rejections != null;

        public static GroundPaintedAccentProjectedGlyphDebugSnapshot Empty =>
            default;
    }

    internal static class GroundPaintedAccentProjectedGlyphGenerator
    {
        public const int Revision = 19;

        private const int ShapeMetricCount = 10;
        private const int CrestPositionMetricIndex = 0;
        private const int LegSpanRatioMetricIndex = 1;
        private const int LegSlopeRatioMetricIndex = 2;
        private const int UpperRunFractionMetricIndex = 3;
        private const int UpperEndpointDropMetricIndex = 4;
        private const int DescendingDropMetricIndex = 5;
        private const int PlateauFractionMetricIndex = 6;
        private const int VerticalRangeMetricIndex = 7;
        private const int EndpointDifferenceMetricIndex = 8;
        private const int EndpointDifferenceWorldMetricIndex = 9;

        private const float RiverSafetyClearance = 0.15f;
        private const float MaximumBroadSlopeDegrees = 45f;
        private const float MaximumLocalGradeDegrees = 40f;
        private const float MaximumCompleteMoundTurnDegrees = 32f;
        private const float MaximumAsymmetricMoundTurnDegrees = 30f;
        private const float MaximumSingleShoulderTurnDegrees = 27f;
        private const float MaximumShallowCrestTurnDegrees = 25f;
        private const float RejectionMarkerSurfaceOffset = 0.04f;
        private const float MinimumProjectedContactHalfWidth = 0.0035f;
        private const float MinimumProjectedContactWidthFraction = 0.32f;
        private const float MaximumProjectedContactSearchFraction = 0.24f;
        private const float MaximumProjectedClusterRelocationLengthMultiplier =
            2.25f;
        private const float ProjectedContactNeighborhoodFraction = 0.08f;
        private const float MinimumProjectedInteriorContactNormalComponent = 0.22f;
        private const float ProjectedLooseContactExtraWidthMultiplier = 0.60f;
        private const float ProjectedInternalSweptClearanceFraction = 0.98f;
        private const float ProjectedContactSweptClearanceFraction = 0.94f;
        private const float ProjectedExternalConflictWidthFraction = 0.42f;
        private const float ProjectedExternalConflictGridCellSize = 0.50f;
        private const float MinimumProjectedExternalClearance = 0.003f;
        private const float ProjectedSweptClearanceTolerance = 0.0005f;
        private const float MinimumProjectedContactSideProjection = 0.0001f;
        private const float MinimumProjectedStructuredStep = 0.020f;
        private const float MinimumProjectedPairStepRetentionFraction = 0.65f;
        private const float MinimumProjectedTripletStepRetentionFraction = 0.42f;
        private const float MaximumNearCollinearPairJunctionDegrees = 16f;
        private const float MaximumNearCollinearPairStepLengthFraction = 0.12f;
        private const float MinimumShallowPairContactGap = 0.010f;
        private const float MaximumNearParallelBodyAngleDegrees = 22f;
        private const float NearParallelBodyClearanceFraction = 0.88f;
        private const float MinimumNearParallelBlendLengthFraction = 0.14f;
        private const float MinimumNearParallelBlendWorldLength = 0.025f;
        private const float MinimumTripletAttachmentIndexSeparationFraction =
            0.14f;
        private const float MinimumTripletContactSeparationLengthFraction =
            0.10f;
        private const float MinimumTripletContactSeparationWidthMultiplier =
            2.25f;
        private const float MaximumSeverelyCompressedTripletCentroidSpanFraction =
            0.42f;
        private const float MinimumCrowdedTripletJunctionLengthFraction =
            0.055f;
        private const float MinimumCrowdedTripletJunctionWidthMultiplier =
            1.35f;
        private const float MinimumTripletPseudoContactLengthFraction =
            0.055f;
        private const float MinimumTripletPseudoContactWidthMultiplier =
            1.35f;
        private const float MinimumTripletPseudoContactBodyLengthFraction =
            0.035f;
        private const float MinimumTripletPseudoContactBodyWidthMultiplier =
            1.10f;
        private const float MinimumTripletPseudoContactApproachDot = 0.35f;
        private const float ProjectedStepErrorScoreWeight = 8f;
        private const int MaximumAuthoritativeClusterBuildAttempts = 72;
        private const float AuthoritativeDensityRadiusLengthMultiplier = 3.25f;
        private const float MinimumAuthoritativeDensityRadius = 1.50f;
        private const float PairSteppedStepLengthFraction = 0.24f;
        private const float PairShoulderStepLengthFraction = 0.21f;
        private const float PairOffsetStepLengthFraction = 0.18f;
        private const float PairShallowStepLengthFraction = 0.075f;
        private const float TripletStepLengthFraction = 0.22f;
        private const float AuthoritativeAlongSpacingLengthFraction = 0.58f;

        private sealed class ProjectedGlyphPrototype
        {
            public ProjectedGlyphPrototype(
                GroundPaintedAccentSurfaceStroke stroke,
                Vector2[] projectedPoints,
                float[] halfWidths,
                GroundPaintedAccentLongitudinalProfile profile,
                float crestT,
                float maximumProjectedTurnDegrees,
                float maximumNorthDisplacementError,
                float maximumCrossAxisDrift)
            {
                Stroke = stroke;
                ProjectedPoints = projectedPoints ?? Array.Empty<Vector2>();
                HalfWidths = halfWidths ?? Array.Empty<float>();
                Profile = profile;
                CrestT = crestT;
                MaximumProjectedTurnDegrees = maximumProjectedTurnDegrees;
                MaximumNorthDisplacementError =
                    maximumNorthDisplacementError;
                MaximumCrossAxisDrift = maximumCrossAxisDrift;
            }

            public GroundPaintedAccentSurfaceStroke Stroke { get; }
            public Vector2[] ProjectedPoints { get; set; }
            public float[] HalfWidths { get; set; }
            public GroundPaintedAccentLongitudinalProfile Profile { get; }
            public float CrestT { get; }
            public float MaximumProjectedTurnDegrees { get; }
            public float MaximumNorthDisplacementError { get; }
            public float MaximumCrossAxisDrift { get; }

            public ProjectedGlyphPrototype Clone()
            {
                return CloneWithStroke(Stroke);
            }

            public ProjectedGlyphPrototype CloneWithStroke(
                GroundPaintedAccentSurfaceStroke stroke)
            {
                return new ProjectedGlyphPrototype(
                    stroke,
                    (Vector2[])ProjectedPoints.Clone(),
                    (float[])HalfWidths.Clone(),
                    Profile,
                    CrestT,
                    MaximumProjectedTurnDegrees,
                    MaximumNorthDisplacementError,
                    MaximumCrossAxisDrift);
            }
        }

        private sealed class AuthoritativeProjectedMark
        {
            public AuthoritativeProjectedMark(
                int sourceIndex,
                GroundPaintedAccentSurfaceStroke stroke,
                ProjectedGlyphPrototype prototype,
                GroundPaintedAccentProjectedGlyph independentGlyph)
            {
                SourceIndex = sourceIndex;
                Stroke = stroke;
                Prototype = prototype;
                IndependentGlyph = independentGlyph;
                Centroid = ResolveProjectedCentroid(prototype.ProjectedPoints);
            }

            public int SourceIndex { get; }
            public GroundPaintedAccentSurfaceStroke Stroke { get; }
            public ProjectedGlyphPrototype Prototype { get; }
            public GroundPaintedAccentProjectedGlyph IndependentGlyph { get; }
            public Vector2 Centroid { get; }
            public float DensityScore { get; set; }
            public float SelectionScore { get; set; }
        }

        private readonly struct AuthoritativeCompanionQuota
        {
            public AuthoritativeCompanionQuota(
                int participantCount,
                int pairClusterCount,
                int tripletClusterCount)
            {
                ParticipantCount = Mathf.Max(0, participantCount);
                PairClusterCount = Mathf.Max(0, pairClusterCount);
                TripletClusterCount = Mathf.Max(0, tripletClusterCount);
            }

            public int ParticipantCount { get; }
            public int PairClusterCount { get; }
            public int TripletClusterCount { get; }
        }

        private readonly struct AuthoritativeClusterSpec
        {
            public AuthoritativeClusterSpec(
                int size,
                GroundPaintedAccentCompanionPairLayout pairLayout,
                GroundPaintedAccentCompanionTripletLayout tripletLayout,
                int ordinal)
            {
                Size = Mathf.Clamp(size, 2, 3);
                PairLayout = pairLayout;
                TripletLayout = tripletLayout;
                Ordinal = Mathf.Max(0, ordinal);
            }

            public int Size { get; }
            public GroundPaintedAccentCompanionPairLayout PairLayout { get; }
            public GroundPaintedAccentCompanionTripletLayout TripletLayout { get; }
            public int Ordinal { get; }
        }

        private sealed class AuthoritativeClusterRecord
        {
            public int ClusterIndex;
            public AuthoritativeClusterSpec Spec;
            public int[] MarkIndices;
            public GroundPaintedAccentProjectedGlyph[] ClusterGlyphs;
            public int ReconciliationIndependentCountChecked;
        }

        private sealed class ProjectedGlyphSpatialIndex
        {
            private readonly Dictionary<long, List<int>> cells =
                new Dictionary<long, List<int>>();
            private readonly List<int> queryResults = new List<int>();
            private int[] queryVisitStamps = Array.Empty<int>();
            private int queryStamp;
            private int indexedGlyphCount;

            public void Add(
                int glyphIndex,
                GroundPaintedAccentProjectedGlyph glyph)
            {
                ResolveProjectedBounds(
                    glyph.LocalProjectedPoints,
                    glyph.HalfWidths,
                    ProjectedExternalConflictWidthFraction,
                    out Vector2 minimum,
                    out Vector2 maximum);
                Add(glyphIndex, minimum, maximum);
            }

            public IReadOnlyList<int> Query(
                IReadOnlyList<Vector2> points,
                IReadOnlyList<float> halfWidths,
                GroundPaintedAccentClusterBuildAuditAccumulator clusterAudit)
            {
                queryResults.Clear();
                if (indexedGlyphCount <= 0)
                {
                    if (clusterAudit != null)
                    {
                        clusterAudit.ExternalSpatialQueries++;
                    }
                    return queryResults;
                }

                ResolveProjectedBounds(
                    points,
                    halfWidths,
                    ProjectedExternalConflictWidthFraction,
                    out Vector2 minimum,
                    out Vector2 maximum);
                int minimumCellX = ResolveCellCoordinate(minimum.x);
                int maximumCellX = ResolveCellCoordinate(maximum.x);
                int minimumCellY = ResolveCellCoordinate(minimum.y);
                int maximumCellY = ResolveCellCoordinate(maximum.y);
                EnsureQueryStampCapacity(indexedGlyphCount);
                AdvanceQueryStamp();

                long visitedCells = 0L;
                for (int cellX = minimumCellX;
                     cellX <= maximumCellX;
                     cellX++)
                {
                    for (int cellY = minimumCellY;
                         cellY <= maximumCellY;
                         cellY++)
                    {
                        visitedCells++;
                        if (!cells.TryGetValue(
                                ResolveCellKey(cellX, cellY),
                                out List<int> glyphIndices))
                        {
                            continue;
                        }

                        for (int index = 0;
                             index < glyphIndices.Count;
                             index++)
                        {
                            int glyphIndex = glyphIndices[index];
                            if (glyphIndex < 0 ||
                                glyphIndex >= indexedGlyphCount ||
                                queryVisitStamps[glyphIndex] == queryStamp)
                            {
                                continue;
                            }

                            queryVisitStamps[glyphIndex] = queryStamp;
                            queryResults.Add(glyphIndex);
                        }
                    }
                }

                queryResults.Sort();
                if (clusterAudit != null)
                {
                    clusterAudit.ExternalSpatialQueries++;
                    clusterAudit.ExternalGridCellsVisited += visitedCells;
                    clusterAudit.ExternalUniqueCandidatesReturned +=
                        queryResults.Count;
                    clusterAudit.ExternalFullListComparisonsAvoided +=
                        Mathf.Max(0, indexedGlyphCount - queryResults.Count);
                }
                return queryResults;
            }

            private void Add(
                int glyphIndex,
                Vector2 minimum,
                Vector2 maximum)
            {
                if (glyphIndex < 0)
                {
                    return;
                }

                int minimumCellX = ResolveCellCoordinate(minimum.x);
                int maximumCellX = ResolveCellCoordinate(maximum.x);
                int minimumCellY = ResolveCellCoordinate(minimum.y);
                int maximumCellY = ResolveCellCoordinate(maximum.y);
                for (int cellX = minimumCellX;
                     cellX <= maximumCellX;
                     cellX++)
                {
                    for (int cellY = minimumCellY;
                         cellY <= maximumCellY;
                         cellY++)
                    {
                        long key = ResolveCellKey(cellX, cellY);
                        if (!cells.TryGetValue(key, out List<int> indices))
                        {
                            indices = new List<int>();
                            cells.Add(key, indices);
                        }
                        indices.Add(glyphIndex);
                    }
                }

                indexedGlyphCount = Mathf.Max(indexedGlyphCount, glyphIndex + 1);
                EnsureQueryStampCapacity(indexedGlyphCount);
            }

            private void EnsureQueryStampCapacity(int required)
            {
                if (queryVisitStamps.Length >= required)
                {
                    return;
                }

                int capacity = Mathf.NextPowerOfTwo(Mathf.Max(2, required));
                Array.Resize(ref queryVisitStamps, capacity);
            }

            private void AdvanceQueryStamp()
            {
                if (queryStamp == int.MaxValue)
                {
                    Array.Clear(queryVisitStamps, 0, queryVisitStamps.Length);
                    queryStamp = 1;
                    return;
                }

                queryStamp++;
                if (queryStamp <= 0)
                {
                    queryStamp = 1;
                }
            }

            private static int ResolveCellCoordinate(float value)
            {
                return Mathf.FloorToInt(
                    value / ProjectedExternalConflictGridCellSize);
            }

            private static long ResolveCellKey(int cellX, int cellY)
            {
                return ((long)cellX << 32) ^ (uint)cellY;
            }
        }

        private readonly struct ProjectedTripletFreeTerminal
        {
            public ProjectedTripletFreeTerminal(
                int memberIndex,
                Vector2 position,
                Vector2 outward,
                float halfWidth,
                float authoredLength)
            {
                MemberIndex = memberIndex;
                Position = position;
                Outward = outward;
                HalfWidth = Mathf.Max(0f, halfWidth);
                AuthoredLength = Mathf.Max(0f, authoredLength);
            }

            public int MemberIndex { get; }
            public Vector2 Position { get; }
            public Vector2 Outward { get; }
            public float HalfWidth { get; }
            public float AuthoredLength { get; }
        }

        private readonly struct ProjectedClusterContact
        {
            public ProjectedClusterContact(
                int anchorMemberIndex,
                int anchorPointIndex,
                int movingMemberIndex,
                int movingPointIndex)
            {
                AnchorMemberIndex = anchorMemberIndex;
                AnchorPointIndex = anchorPointIndex;
                MovingMemberIndex = movingMemberIndex;
                MovingPointIndex = movingPointIndex;
            }

            public int AnchorMemberIndex { get; }
            public int AnchorPointIndex { get; }
            public int MovingMemberIndex { get; }
            public int MovingPointIndex { get; }
        }

        private struct ProjectedClusterQualityRejections
        {
            public int PairStepRetention;
            public int PairNearCollinear;
            public int PreGeometryStepRetention;
            public int PreGeometryNonCompetitiveScore;
            public int CandidatesSentToGeometry;
            public int NearParallelBodyBlend;
            public int OccupiedAttachmentSlot;
            public int SharedContactLocus;
            public int CrowdedTripletJunction;
            public int TripletFreeEndPseudoContact;
            public int SeverelyCompressedTriplet;
            public int WrongSideTerminal;
            public int SweptWidthInternalOverlap;

            public void Accumulate(ProjectedClusterQualityRejections other)
            {
                PairStepRetention += other.PairStepRetention;
                PairNearCollinear += other.PairNearCollinear;
                PreGeometryStepRetention += other.PreGeometryStepRetention;
                PreGeometryNonCompetitiveScore +=
                    other.PreGeometryNonCompetitiveScore;
                CandidatesSentToGeometry += other.CandidatesSentToGeometry;
                NearParallelBodyBlend += other.NearParallelBodyBlend;
                OccupiedAttachmentSlot += other.OccupiedAttachmentSlot;
                SharedContactLocus += other.SharedContactLocus;
                CrowdedTripletJunction += other.CrowdedTripletJunction;
                TripletFreeEndPseudoContact +=
                    other.TripletFreeEndPseudoContact;
                SeverelyCompressedTriplet += other.SeverelyCompressedTriplet;
                WrongSideTerminal += other.WrongSideTerminal;
                SweptWidthInternalOverlap +=
                    other.SweptWidthInternalOverlap;
            }
        }

        private enum ProjectedCompanionFallbackReason
        {
            None = 0,
            Incomplete = 1,
            Prototype = 2,
            Contact = 3,
            Surface = 4,
            ExternalConflict = 5
        }

        public static GroundPaintedAccentProjectedGlyphDebugSnapshot Build(
            IReadOnlyList<GroundPaintedAccentSurfaceStroke> baseStrokes,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            IReadOnlyList<GroundPaintedAccentRiverExclusionSnapshot> rivers,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            Vector2 projectionLocalDirection,
            out GroundPaintedAccentProjectedGlyphBuildTimings timings)
        {
            GroundPaintedAccentProjectedGlyphTimingAccumulator timing =
                new GroundPaintedAccentProjectedGlyphTimingAccumulator();
            timings = GroundPaintedAccentProjectedGlyphBuildTimings.Empty;

            if (baseStrokes == null ||
                baseSurface == null ||
                !baseSurface.IsValid ||
                feature == null)
            {
                return GroundPaintedAccentProjectedGlyphDebugSnapshot.Empty;
            }

            Vector2 localNorth =
                projectionLocalDirection.sqrMagnitude > 0.000001f
                    ? projectionLocalDirection.normalized
                    : Vector2.up;
            List<GroundPaintedAccentProjectedGlyph> glyphs =
                new List<GroundPaintedAccentProjectedGlyph>(baseStrokes.Count);
            List<GroundPaintedAccentProjectedGlyphRejectionDebugPoint> rejections =
                new List<GroundPaintedAccentProjectedGlyphRejectionDebugPoint>();
            GroundPaintedAccentProjectedGlyphScratch scratch =
                new GroundPaintedAccentProjectedGlyphScratch();

            int rejectedSampling = 0;
            int rejectedRiver = 0;
            int rejectedModifier = 0;
            int rejectedBroadSlope = 0;
            int rejectedLocalGrade = 0;
            int rejectedFamilyShape = 0;
            int rejectedSharpTurn = 0;
            int companionClustersRequested = 0;
            int companionClustersAccepted = 0;
            int companionClustersFallback = 0;
            int companionPairsAccepted = 0;
            int companionTripletsAccepted = 0;
            int companionClustersRemovedDuringReconciliation = 0;
            int companionPairStepRetentionRejectedCandidates = 0;
            int companionPairNearCollinearRejectedCandidates = 0;
            int companionPreGeometryStepRetentionRejectedCandidates = 0;
            int companionPreGeometryNonCompetitiveScoreRejectedCandidates = 0;
            int companionCandidatesSentToGeometryValidation = 0;
            int companionNearParallelBodyRejectedCandidates = 0;
            int companionOccupiedAttachmentSlotRejectedCandidates = 0;
            int companionSharedContactLocusRejectedCandidates = 0;
            int companionCrowdedTripletJunctionRejectedCandidates = 0;
            int companionTripletFreeEndPseudoContactRejectedCandidates = 0;
            int companionSeverelyCompressedTripletRejectedCandidates = 0;
            int companionWrongSideTerminalRejectedCandidates = 0;
            int companionSweptWidthInternalOverlapRejectedCandidates = 0;
            int companionPairRejectedIncomplete = 0;
            int companionPairRejectedPrototype = 0;
            int companionPairRejectedContact = 0;
            int companionPairRejectedSurface = 0;
            int companionPairRejectedExternalConflict = 0;
            int companionRejectedIncomplete = 0;
            int companionRejectedPrototype = 0;
            int companionRejectedContact = 0;
            int companionRejectedSurface = 0;
            int companionRejectedExternalConflict = 0;

            int[] projectedValidByRankQuartile = new int[4];
            int quietProjectedValid = 0;
            int supportingProjectedValid = 0;
            int accentProjectedValid = 0;
            List<AuthoritativeProjectedMark> eligibleMarks =
                new List<AuthoritativeProjectedMark>(baseStrokes.Count);
            for (int strokeIndex = 0;
                 strokeIndex < baseStrokes.Count;
                 strokeIndex++)
            {
                GroundPaintedAccentSurfaceStroke stroke = baseStrokes[strokeIndex];
                if (stroke.IsCompanionMember && stroke.HasIndependentFallback)
                {
                    stroke = stroke.CreateIndependentFallback();
                }

                if (!TryBuildProjectedGlyphPrototype(
                        stroke,
                        feature,
                        localNorth,
                        timing,
                        out ProjectedGlyphPrototype prototype,
                        out GroundPaintedAccentProjectedGlyphRejectionReason
                            prototypeReason))
                {
                    RecordProjectionRejection(
                        stroke,
                        prototypeReason,
                        rejections,
                        ref rejectedSampling,
                        ref rejectedRiver,
                        ref rejectedModifier,
                        ref rejectedBroadSlope,
                        ref rejectedLocalGrade,
                        ref rejectedFamilyShape,
                        ref rejectedSharpTurn);
                    continue;
                }

                if (!TryFinalizeProjectedGlyph(
                        prototype,
                        baseSurface,
                        rivers,
                        modifiers,
                        scratch,
                        timing,
                        out GroundPaintedAccentProjectedGlyph independentGlyph,
                        out GroundPaintedAccentProjectedGlyphRejectionReason
                            finalizeReason))
                {
                    RecordProjectionRejection(
                        stroke,
                        finalizeReason,
                        rejections,
                        ref rejectedSampling,
                        ref rejectedRiver,
                        ref rejectedModifier,
                        ref rejectedBroadSlope,
                        ref rejectedLocalGrade,
                        ref rejectedFamilyShape,
                        ref rejectedSharpTurn);
                    continue;
                }

                projectedValidByRankQuartile[
                    Mathf.Clamp(stroke.ProposalRankQuartile, 0, 3)]++;
                switch (stroke.CompositionRegionMode)
                {
                    case GroundPaintedAccentCompositionRegionMode.Quiet:
                        quietProjectedValid++;
                        break;
                    case GroundPaintedAccentCompositionRegionMode.Accent:
                        accentProjectedValid++;
                        break;
                    case GroundPaintedAccentCompositionRegionMode.Supporting:
                    default:
                        supportingProjectedValid++;
                        break;
                }

                eligibleMarks.Add(
                    new AuthoritativeProjectedMark(
                        strokeIndex,
                        stroke,
                        prototype,
                        independentGlyph));
            }

            GroundPaintedAccentClusterBuildAuditAccumulator clusterAudit =
                new GroundPaintedAccentClusterBuildAuditAccumulator();
            GroundPaintedAccentInternalOverlapAuditAccumulator
                internalOverlapAudit =
                    new GroundPaintedAccentInternalOverlapAuditAccumulator();
            GroundPaintedAccentNearParallelAuditAccumulator
                nearParallelAudit =
                    new GroundPaintedAccentNearParallelAuditAccumulator();
            long clusterAuditStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
            long quotaPreparationStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
            float companionParticipation =
                Mathf.Clamp01(feature.PaintedAccentCompanionParticipation);
            float companionTripletShare =
                Mathf.Clamp01(feature.PaintedAccentCompanionTripletShare);
            AuthoritativeCompanionQuota authoritativeQuota =
                ResolveAuthoritativeCompanionQuota(
                    eligibleMarks.Count,
                    companionParticipation,
                    companionTripletShare);
            companionClustersRequested =
                authoritativeQuota.PairClusterCount +
                authoritativeQuota.TripletClusterCount;
            int companionParticipantsRequested =
                authoritativeQuota.ParticipantCount;
            int companionPairClustersRequested =
                authoritativeQuota.PairClusterCount;
            int companionTripletClustersRequested =
                authoritativeQuota.TripletClusterCount;

            int[] requestedPairLayouts =
                ResolveAuthoritativeLayoutCounts(
                    companionPairClustersRequested,
                    feature.PaintedAccentCompanionPairLayoutWeights);
            int[] requestedTripletLayouts =
                ResolveAuthoritativeLayoutCounts(
                    companionTripletClustersRequested,
                    feature.PaintedAccentCompanionTripletLayoutWeights);
            int[] acceptedPairLayouts = new int[4];
            int[] acceptedTripletLayouts = new int[4];
            List<AuthoritativeClusterSpec> clusterSpecs =
                BuildAuthoritativeClusterSpecs(
                    requestedPairLayouts,
                    requestedTripletLayouts,
                    feature.SeedOffset);
            timing.ClusterQuotaPreparationTicks +=
                System.Diagnostics.Stopwatch.GetTimestamp() -
                quotaPreparationStartedAt;

            long participantSelectionStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
            if (companionParticipantsRequested > 0)
            {
                ResolveAuthoritativeMarkSelectionScores(
                    eligibleMarks,
                    feature.PaintedAccentClusterRegionBias);
                eligibleMarks.Sort(CompareAuthoritativeProjectedMarks);
            }

            int resolvedParticipantCount =
                Mathf.Min(
                    companionParticipantsRequested,
                    eligibleMarks.Count);
            List<int> participantMarkIndices =
                new List<int>(resolvedParticipantCount);
            List<int> independentMarkIndices =
                new List<int>(eligibleMarks.Count - resolvedParticipantCount);
            for (int markIndex = 0;
                 markIndex < eligibleMarks.Count;
                 markIndex++)
            {
                if (markIndex < resolvedParticipantCount)
                {
                    participantMarkIndices.Add(markIndex);
                }
                else
                {
                    independentMarkIndices.Add(markIndex);
                }
            }

            for (int independentIndex = 0;
                 independentIndex < independentMarkIndices.Count;
                 independentIndex++)
            {
                glyphs.Add(
                    eligibleMarks[independentMarkIndices[independentIndex]]
                        .IndependentGlyph);
            }

            ProjectedGlyphSpatialIndex acceptedGlyphSpatialIndex =
                new ProjectedGlyphSpatialIndex();
            for (int glyphIndex = 0;
                 glyphIndex < glyphs.Count;
                 glyphIndex++)
            {
                acceptedGlyphSpatialIndex.Add(glyphIndex, glyphs[glyphIndex]);
            }

            timing.ClusterParticipantSelectionTicks +=
                System.Diagnostics.Stopwatch.GetTimestamp() -
                participantSelectionStartedAt;

            List<AuthoritativeClusterRecord> acceptedClusterRecords =
                new List<AuthoritativeClusterRecord>(clusterSpecs.Count);
            List<int> availableParticipantIndices =
                new List<int>(participantMarkIndices);
            int authoritativeClusterBuildAttempts = 0;
            int authoritativePairShortfall = 0;
            int authoritativeTripletShortfall = 0;

            for (int specIndex = 0;
                 specIndex < clusterSpecs.Count;
                 specIndex++)
            {
                AuthoritativeClusterSpec spec = clusterSpecs[specIndex];
                int clusterIndex = acceptedClusterRecords.Count;
                bool clusterBuilt =
                    TryBuildAuthoritativeProjectedCluster(
                        spec,
                        clusterIndex,
                        eligibleMarks,
                        availableParticipantIndices,
                        baseSurface,
                        feature,
                        rivers,
                        modifiers,
                        localNorth,
                        scratch,
                        timing,
                        clusterAudit,
                        nearParallelAudit,
                        internalOverlapAudit,
                        glyphs,
                        acceptedGlyphSpatialIndex,
                        out int[] selectedMarkIndices,
                        out List<GroundPaintedAccentProjectedGlyph>
                            clusterGlyphs,
                        out ProjectedCompanionFallbackReason fallbackReason,
                        out int buildAttempts,
                        out bool exhaustedAttemptBudget,
                        out ProjectedClusterQualityRejections
                            qualityRejections);
                authoritativeClusterBuildAttempts += buildAttempts;
                clusterAudit.RecordBuildResult(
                    clusterBuilt,
                    buildAttempts,
                    exhaustedAttemptBudget);
                long clusterCommitStartedAt =
                    System.Diagnostics.Stopwatch.GetTimestamp();
                companionPairStepRetentionRejectedCandidates +=
                    qualityRejections.PairStepRetention;
                companionPairNearCollinearRejectedCandidates +=
                    qualityRejections.PairNearCollinear;
                companionPreGeometryStepRetentionRejectedCandidates +=
                    qualityRejections.PreGeometryStepRetention;
                companionPreGeometryNonCompetitiveScoreRejectedCandidates +=
                    qualityRejections.PreGeometryNonCompetitiveScore;
                companionCandidatesSentToGeometryValidation +=
                    qualityRejections.CandidatesSentToGeometry;
                companionNearParallelBodyRejectedCandidates +=
                    qualityRejections.NearParallelBodyBlend;
                companionOccupiedAttachmentSlotRejectedCandidates +=
                    qualityRejections.OccupiedAttachmentSlot;
                companionSharedContactLocusRejectedCandidates +=
                    qualityRejections.SharedContactLocus;
                companionCrowdedTripletJunctionRejectedCandidates +=
                    qualityRejections.CrowdedTripletJunction;
                companionTripletFreeEndPseudoContactRejectedCandidates +=
                    qualityRejections.TripletFreeEndPseudoContact;
                companionSeverelyCompressedTripletRejectedCandidates +=
                    qualityRejections.SeverelyCompressedTriplet;
                companionWrongSideTerminalRejectedCandidates +=
                    qualityRejections.WrongSideTerminal;
                companionSweptWidthInternalOverlapRejectedCandidates +=
                    qualityRejections.SweptWidthInternalOverlap;

                if (clusterBuilt)
                {
                    companionClustersAccepted++;
                    if (spec.Size >= 3)
                    {
                        companionTripletsAccepted++;
                        acceptedTripletLayouts[
                            ResolveAuthoritativeTripletLayoutIndex(
                                spec.TripletLayout)]++;
                    }
                    else
                    {
                        companionPairsAccepted++;
                        acceptedPairLayouts[
                            ResolveAuthoritativePairLayoutIndex(
                                spec.PairLayout)]++;
                    }

                    for (int selectedIndex = 0;
                         selectedIndex < selectedMarkIndices.Length;
                         selectedIndex++)
                    {
                        availableParticipantIndices.Remove(
                            selectedMarkIndices[selectedIndex]);
                    }
                    GroundPaintedAccentProjectedGlyph[]
                        committedClusterGlyphs = clusterGlyphs.ToArray();
                    for (int clusterGlyphIndex = 0;
                         clusterGlyphIndex < committedClusterGlyphs.Length;
                         clusterGlyphIndex++)
                    {
                        int glyphIndex = glyphs.Count;
                        GroundPaintedAccentProjectedGlyph committedGlyph =
                            committedClusterGlyphs[clusterGlyphIndex];
                        glyphs.Add(committedGlyph);
                        acceptedGlyphSpatialIndex.Add(
                            glyphIndex,
                            committedGlyph);
                    }
                    acceptedClusterRecords.Add(
                        new AuthoritativeClusterRecord
                        {
                            ClusterIndex = clusterIndex,
                            Spec = spec,
                            MarkIndices = selectedMarkIndices,
                            ClusterGlyphs = committedClusterGlyphs,
                            ReconciliationIndependentCountChecked = 0
                        });
                    timing.ClusterCommitTicks +=
                        System.Diagnostics.Stopwatch.GetTimestamp() -
                        clusterCommitStartedAt;
                    continue;
                }

                companionClustersFallback++;
                if (spec.Size >= 3)
                {
                    authoritativeTripletShortfall++;
                }
                else
                {
                    authoritativePairShortfall++;
                }

                bool pairFallback = spec.Size == 2;
                switch (fallbackReason)
                {
                    case ProjectedCompanionFallbackReason.Incomplete:
                        companionRejectedIncomplete++;
                        if (pairFallback)
                        {
                            companionPairRejectedIncomplete++;
                        }
                        break;
                    case ProjectedCompanionFallbackReason.Prototype:
                        companionRejectedPrototype++;
                        if (pairFallback)
                        {
                            companionPairRejectedPrototype++;
                        }
                        break;
                    case ProjectedCompanionFallbackReason.Surface:
                        companionRejectedSurface++;
                        if (pairFallback)
                        {
                            companionPairRejectedSurface++;
                        }
                        break;
                    case ProjectedCompanionFallbackReason.ExternalConflict:
                        companionRejectedExternalConflict++;
                        if (pairFallback)
                        {
                            companionPairRejectedExternalConflict++;
                        }
                        break;
                    case ProjectedCompanionFallbackReason.Contact:
                    default:
                        companionRejectedContact++;
                        if (pairFallback)
                        {
                            companionPairRejectedContact++;
                        }
                        break;
                }
                timing.ClusterCommitTicks +=
                    System.Diagnostics.Stopwatch.GetTimestamp() -
                    clusterCommitStartedAt;
            }

            long remainingIndependentCommitStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
            // Initial independents and cluster-to-cluster relationships were
            // already checked during authoritative construction. Only glyphs
            // committed after allocation can introduce reconciliation work.
            List<GroundPaintedAccentProjectedGlyph>
                reconciliationIndependentGlyphs =
                    new List<GroundPaintedAccentProjectedGlyph>(
                        availableParticipantIndices.Count);
            for (int availableIndex = 0;
                 availableIndex < availableParticipantIndices.Count;
                 availableIndex++)
            {
                GroundPaintedAccentProjectedGlyph independentGlyph =
                    eligibleMarks[availableParticipantIndices[availableIndex]]
                        .IndependentGlyph;
                glyphs.Add(independentGlyph);
                reconciliationIndependentGlyphs.Add(independentGlyph);
            }

            timing.ClusterCommitTicks +=
                System.Diagnostics.Stopwatch.GetTimestamp() -
                remainingIndependentCommitStartedAt;

            long reconciliationStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
            ReconcileAuthoritativeClustersAgainstFinalIndependents(
                eligibleMarks,
                acceptedClusterRecords,
                glyphs,
                reconciliationIndependentGlyphs,
                acceptedPairLayouts,
                acceptedTripletLayouts,
                ref companionClustersAccepted,
                ref companionClustersFallback,
                ref companionPairsAccepted,
                ref companionTripletsAccepted,
                ref authoritativePairShortfall,
                ref authoritativeTripletShortfall,
                ref companionRejectedExternalConflict,
                ref companionPairRejectedExternalConflict,
                ref companionClustersRemovedDuringReconciliation,
                clusterAudit);
            timing.ClusterReconciliationTicks +=
                System.Diagnostics.Stopwatch.GetTimestamp() -
                reconciliationStartedAt;
            timing.ClusterAuditTotalTicks +=
                System.Diagnostics.Stopwatch.GetTimestamp() -
                clusterAuditStartedAt;

            int finalCompanionParticipantCount = 0;
            for (int glyphIndex = 0; glyphIndex < glyphs.Count; glyphIndex++)
            {
                if (glyphs[glyphIndex].IsCompanionMember)
                {
                    finalCompanionParticipantCount++;
                }
            }
            float finalCompanionParticipantFraction =
                glyphs.Count > 0
                    ? finalCompanionParticipantCount / (float)glyphs.Count
                    : 0f;

            long diagnosticsStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
            ResolveAcceptedProjectedPairStepStatistics(
                glyphs,
                out float companionPairVerticalStepMin,
                out float companionPairVerticalStepMean,
                out float companionPairVerticalStepMax,
                out float companionPairVerticalStepFractionMin,
                out float companionPairVerticalStepFractionMean,
                out float companionPairVerticalStepFractionMax);
            int[] familyAttempted = new int[4];
            int[] familyAccepted = new int[4];
            int[] familyRejected = new int[4];
            float[] familyLengthMin =
                { float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity };
            float[] familyLengthMax = new float[4];
            double[] familyLengthTotal = new double[4];
            float[] familyPeakMin =
                { float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity };
            float[] familyPeakMax = new float[4];
            double[] familyPeakTotal = new double[4];
            float[,] familyShapeMin = new float[4, ShapeMetricCount];
            float[,] familyShapeMax = new float[4, ShapeMetricCount];
            double[,] familyShapeTotal = new double[4, ShapeMetricCount];
            int[] familyNearStraightCount = new int[4];
            for (int familyIndex = 0; familyIndex < 4; familyIndex++)
            {
                for (int metricIndex = 0;
                     metricIndex < ShapeMetricCount;
                     metricIndex++)
                {
                    familyShapeMin[familyIndex, metricIndex] =
                        float.PositiveInfinity;
                }
            }

            int pointCountMinimum = int.MaxValue;
            int pointCountMaximum = 0;
            double pointCountTotal = 0.0;
            float crestMinimum = float.PositiveInfinity;
            float crestMaximum = 0f;
            double crestTotal = 0.0;
            float crestHeightMinimum = float.PositiveInfinity;
            float crestHeightMaximum = 0f;
            double crestHeightTotal = 0.0;
            float crownHeightMinimum = float.PositiveInfinity;
            float crownHeightMaximum = 0f;
            double crownHeightTotal = 0.0;
            float combinedHeightMinimum = float.PositiveInfinity;
            float combinedHeightMaximum = 0f;
            double combinedHeightTotal = 0.0;
            float projectedTurnMinimum = float.PositiveInfinity;
            float projectedTurnMaximum = 0f;
            double projectedTurnTotal = 0.0;
            float maximumNorthError = 0f;
            float maximumCrossDrift = 0f;

            for (int glyphIndex = 0;
                 glyphIndex < glyphs.Count;
                 glyphIndex++)
            {
                GroundPaintedAccentProjectedGlyph glyph = glyphs[glyphIndex];
                int familyIndex = Mathf.Clamp((int)glyph.Family, 0, 3);
                familyAttempted[familyIndex]++;
                familyAccepted[familyIndex]++;
                familyLengthMin[familyIndex] =
                    Mathf.Min(familyLengthMin[familyIndex], glyph.AuthoredLength);
                familyLengthMax[familyIndex] =
                    Mathf.Max(familyLengthMax[familyIndex], glyph.AuthoredLength);
                familyLengthTotal[familyIndex] += glyph.AuthoredLength;
                familyPeakMin[familyIndex] =
                    Mathf.Min(familyPeakMin[familyIndex], glyph.CombinedPeakHeight);
                familyPeakMax[familyIndex] =
                    Mathf.Max(familyPeakMax[familyIndex], glyph.CombinedPeakHeight);
                familyPeakTotal[familyIndex] += glyph.CombinedPeakHeight;
                AccumulateFamilyShapeMetrics(
                    familyIndex,
                    glyph.ShapeMetrics,
                    familyShapeMin,
                    familyShapeTotal,
                    familyShapeMax,
                    familyNearStraightCount);

                int pointCount = glyph.LocalSurfacePoints.Length;
                pointCountMinimum = Mathf.Min(pointCountMinimum, pointCount);
                pointCountMaximum = Mathf.Max(pointCountMaximum, pointCount);
                pointCountTotal += pointCount;
                crestMinimum = Mathf.Min(crestMinimum, glyph.CrestT);
                crestMaximum = Mathf.Max(crestMaximum, glyph.CrestT);
                crestTotal += glyph.CrestT;
                crestHeightMinimum =
                    Mathf.Min(crestHeightMinimum, glyph.CrestPeakHeight);
                crestHeightMaximum =
                    Mathf.Max(crestHeightMaximum, glyph.CrestPeakHeight);
                crestHeightTotal += glyph.CrestPeakHeight;
                crownHeightMinimum =
                    Mathf.Min(crownHeightMinimum, glyph.CrownPeakHeight);
                crownHeightMaximum =
                    Mathf.Max(crownHeightMaximum, glyph.CrownPeakHeight);
                crownHeightTotal += glyph.CrownPeakHeight;
                combinedHeightMinimum =
                    Mathf.Min(combinedHeightMinimum, glyph.CombinedPeakHeight);
                combinedHeightMaximum =
                    Mathf.Max(combinedHeightMaximum, glyph.CombinedPeakHeight);
                combinedHeightTotal += glyph.CombinedPeakHeight;
                projectedTurnMinimum =
                    Mathf.Min(
                        projectedTurnMinimum,
                        glyph.MaximumProjectedTurnDegrees);
                projectedTurnMaximum =
                    Mathf.Max(
                        projectedTurnMaximum,
                        glyph.MaximumProjectedTurnDegrees);
                projectedTurnTotal += glyph.MaximumProjectedTurnDegrees;
                maximumNorthError =
                    Mathf.Max(
                        maximumNorthError,
                        glyph.MaximumNorthDisplacementError);
                maximumCrossDrift =
                    Mathf.Max(maximumCrossDrift, glyph.MaximumCrossAxisDrift);
            }

            for (int rejectionIndex = 0;
                 rejectionIndex < rejections.Count;
                 rejectionIndex++)
            {
                int familyIndex =
                    Mathf.Clamp((int)rejections[rejectionIndex].Family, 0, 3);
                familyAttempted[familyIndex]++;
                familyRejected[familyIndex]++;

            }

            int acceptedCount = glyphs.Count;
            float divisor = Mathf.Max(1, acceptedCount);
            GroundPaintedAccentProjectedGlyphDiagnostics diagnostics =
                new GroundPaintedAccentProjectedGlyphDiagnostics(
                    baseStrokes.Count,
                    acceptedCount,
                    rejectedSampling,
                    rejectedRiver,
                    rejectedModifier,
                    rejectedBroadSlope,
                    rejectedLocalGrade,
                    rejectedFamilyShape,
                    rejectedSharpTurn,
                    companionClustersRequested,
                    companionClustersAccepted,
                    companionClustersFallback,
                    companionPairsAccepted,
                    companionTripletsAccepted,
                    new GroundPaintedAccentCompanionQuotaDiagnostics(
                        eligibleMarks.Count,
                        companionParticipation,
                        companionTripletShare,
                        companionParticipantsRequested,
                        companionPairClustersRequested,
                        companionTripletClustersRequested,
                        companionPairsAccepted,
                        companionTripletsAccepted,
                        authoritativePairShortfall,
                        authoritativeTripletShortfall,
                        authoritativeClusterBuildAttempts,
                        new Vector4(
                            requestedPairLayouts[0],
                            requestedPairLayouts[1],
                            requestedPairLayouts[2],
                            requestedPairLayouts[3]),
                        new Vector4(
                            acceptedPairLayouts[0],
                            acceptedPairLayouts[1],
                            acceptedPairLayouts[2],
                            acceptedPairLayouts[3]),
                        new Vector4(
                            requestedTripletLayouts[0],
                            requestedTripletLayouts[1],
                            requestedTripletLayouts[2],
                            requestedTripletLayouts[3]),
                        new Vector4(
                            acceptedTripletLayouts[0],
                            acceptedTripletLayouts[1],
                            acceptedTripletLayouts[2],
                            acceptedTripletLayouts[3])),
                    clusterAudit.Resolve(
                        companionClustersRequested,
                        authoritativeClusterBuildAttempts),
                    internalOverlapAudit.Resolve(),
                    nearParallelAudit.Resolve(),
                    finalCompanionParticipantCount,
                    finalCompanionParticipantFraction,
                    companionClustersRemovedDuringReconciliation,
                    companionPairStepRetentionRejectedCandidates,
                    companionPairNearCollinearRejectedCandidates,
                    companionPreGeometryStepRetentionRejectedCandidates,
                    companionPreGeometryNonCompetitiveScoreRejectedCandidates,
                    companionCandidatesSentToGeometryValidation,
                    companionNearParallelBodyRejectedCandidates,
                    companionOccupiedAttachmentSlotRejectedCandidates,
                    companionSharedContactLocusRejectedCandidates,
                    companionCrowdedTripletJunctionRejectedCandidates,
                    companionTripletFreeEndPseudoContactRejectedCandidates,
                    companionSeverelyCompressedTripletRejectedCandidates,
                    companionWrongSideTerminalRejectedCandidates,
                    companionSweptWidthInternalOverlapRejectedCandidates,
                    companionPairRejectedIncomplete,
                    companionPairRejectedPrototype,
                    companionPairRejectedContact,
                    companionPairRejectedSurface,
                    companionPairRejectedExternalConflict,
                    companionPairVerticalStepMin,
                    companionPairVerticalStepMean,
                    companionPairVerticalStepMax,
                    companionPairVerticalStepFractionMin,
                    companionPairVerticalStepFractionMean,
                    companionPairVerticalStepFractionMax,
                    companionRejectedIncomplete,
                    companionRejectedPrototype,
                    companionRejectedContact,
                    companionRejectedSurface,
                    companionRejectedExternalConflict,
                    BuildFamilyStatistics(
                        0,
                        familyAttempted,
                        familyAccepted,
                        familyRejected,
                        familyLengthMin,
                        familyLengthTotal,
                        familyLengthMax,
                        familyPeakMin,
                        familyPeakTotal,
                        familyPeakMax,
                        familyShapeMin,
                        familyShapeTotal,
                        familyShapeMax,
                        familyNearStraightCount),
                    BuildFamilyStatistics(
                        1,
                        familyAttempted,
                        familyAccepted,
                        familyRejected,
                        familyLengthMin,
                        familyLengthTotal,
                        familyLengthMax,
                        familyPeakMin,
                        familyPeakTotal,
                        familyPeakMax,
                        familyShapeMin,
                        familyShapeTotal,
                        familyShapeMax,
                        familyNearStraightCount),
                    BuildFamilyStatistics(
                        2,
                        familyAttempted,
                        familyAccepted,
                        familyRejected,
                        familyLengthMin,
                        familyLengthTotal,
                        familyLengthMax,
                        familyPeakMin,
                        familyPeakTotal,
                        familyPeakMax,
                        familyShapeMin,
                        familyShapeTotal,
                        familyShapeMax,
                        familyNearStraightCount),
                    BuildFamilyStatistics(
                        3,
                        familyAttempted,
                        familyAccepted,
                        familyRejected,
                        familyLengthMin,
                        familyLengthTotal,
                        familyLengthMax,
                        familyPeakMin,
                        familyPeakTotal,
                        familyPeakMax,
                        familyShapeMin,
                        familyShapeTotal,
                        familyShapeMax,
                        familyNearStraightCount),
                    localNorth,
                    acceptedCount > 0 ? pointCountMinimum : 0,
                    (float)(pointCountTotal / divisor),
                    pointCountMaximum,
                    acceptedCount > 0 ? crestMinimum : 0f,
                    (float)(crestTotal / divisor),
                    acceptedCount > 0 ? crestMaximum : 0f,
                    acceptedCount > 0 ? crestHeightMinimum : 0f,
                    (float)(crestHeightTotal / divisor),
                    acceptedCount > 0 ? crestHeightMaximum : 0f,
                    acceptedCount > 0 ? crownHeightMinimum : 0f,
                    (float)(crownHeightTotal / divisor),
                    acceptedCount > 0 ? crownHeightMaximum : 0f,
                    acceptedCount > 0 ? combinedHeightMinimum : 0f,
                    (float)(combinedHeightTotal / divisor),
                    acceptedCount > 0 ? combinedHeightMaximum : 0f,
                    acceptedCount > 0 ? projectedTurnMinimum : 0f,
                    (float)(projectedTurnTotal / divisor),
                    acceptedCount > 0 ? projectedTurnMaximum : 0f,
                    maximumNorthError,
                    maximumCrossDrift,
                    new GroundPaintedAccentProjectedFunnelDiagnostics(
                        new Vector4(
                            projectedValidByRankQuartile[0],
                            projectedValidByRankQuartile[1],
                            projectedValidByRankQuartile[2],
                            projectedValidByRankQuartile[3]),
                        quietProjectedValid,
                        supportingProjectedValid,
                        accentProjectedValid));

            GroundPaintedAccentProjectedGlyphDebugSnapshot snapshot =
                new GroundPaintedAccentProjectedGlyphDebugSnapshot(
                    glyphs.ToArray(),
                    rejections.ToArray(),
                    diagnostics);
            timing.DiagnosticsTicks +=
                System.Diagnostics.Stopwatch.GetTimestamp() -
                diagnosticsStartedAt;
            timings = timing.Resolve();
            return snapshot;
        }

        private static AuthoritativeCompanionQuota
            ResolveAuthoritativeCompanionQuota(
                int totalMarkCount,
                float participation,
                float tripletShare)
        {
            int total = Mathf.Max(0, totalMarkCount);
            if (total < 2 || participation <= 0.0001f)
            {
                return new AuthoritativeCompanionQuota(0, 0, 0);
            }

            float targetParticipants = total * Mathf.Clamp01(participation);
            float requestedTripletShare = Mathf.Clamp01(tripletShare);
            int bestParticipants = 0;
            int bestPairs = 0;
            int bestTriplets = 0;
            float bestScore = Mathf.Abs(targetParticipants) * 10000f;

            for (int triplets = 0; triplets <= total / 3; triplets++)
            {
                int maximumPairs = (total - triplets * 3) / 2;
                for (int pairs = 0; pairs <= maximumPairs; pairs++)
                {
                    int participants = pairs * 2 + triplets * 3;
                    if (participants <= 0)
                    {
                        continue;
                    }

                    float participationError =
                        Mathf.Abs(participants - targetParticipants);
                    float resolvedTripletShare =
                        triplets * 3f / participants;
                    float tripletShareError =
                        Mathf.Abs(
                            resolvedTripletShare - requestedTripletShare);
                    float score =
                        participationError * 10000f +
                        tripletShareError * 100f +
                        Mathf.Abs(triplets - pairs) * 0.0001f;
                    if (score < bestScore - 0.0001f)
                    {
                        bestScore = score;
                        bestParticipants = participants;
                        bestPairs = pairs;
                        bestTriplets = triplets;
                    }
                }
            }

            return new AuthoritativeCompanionQuota(
                bestParticipants,
                bestPairs,
                bestTriplets);
        }

        private static int[] ResolveAuthoritativeLayoutCounts(
            int totalCount,
            Vector4 authoredWeights)
        {
            int[] counts = new int[4];
            int total = Mathf.Max(0, totalCount);
            if (total <= 0)
            {
                return counts;
            }

            float[] weights =
            {
                Mathf.Max(0f, authoredWeights.x),
                Mathf.Max(0f, authoredWeights.y),
                Mathf.Max(0f, authoredWeights.z),
                Mathf.Max(0f, authoredWeights.w)
            };
            float weightTotal =
                weights[0] + weights[1] + weights[2] + weights[3];
            if (weightTotal <= 0.0001f)
            {
                weights[0] = 1f;
                weightTotal = 1f;
            }

            float[] remainders = new float[4];
            int assigned = 0;
            for (int index = 0; index < 4; index++)
            {
                float exact = total * weights[index] / weightTotal;
                counts[index] = Mathf.FloorToInt(exact);
                remainders[index] = exact - counts[index];
                assigned += counts[index];
            }

            while (assigned < total)
            {
                int bestIndex = 0;
                for (int index = 1; index < 4; index++)
                {
                    if (remainders[index] > remainders[bestIndex] + 0.000001f)
                    {
                        bestIndex = index;
                    }
                }

                counts[bestIndex]++;
                remainders[bestIndex] = -1f;
                assigned++;
            }

            return counts;
        }

        private static List<AuthoritativeClusterSpec>
            BuildAuthoritativeClusterSpecs(
                IReadOnlyList<int> pairLayoutCounts,
                IReadOnlyList<int> tripletLayoutCounts,
                int seedOffset)
        {
            List<AuthoritativeClusterSpec> specs =
                new List<AuthoritativeClusterSpec>();
            int ordinal = 0;
            GroundPaintedAccentCompanionTripletLayout[] tripletLayouts =
            {
                GroundPaintedAccentCompanionTripletLayout.SteppedRun,
                GroundPaintedAccentCompanionTripletLayout.CrownRun,
                GroundPaintedAccentCompanionTripletLayout.BrokenTerrace,
                GroundPaintedAccentCompanionTripletLayout.ShallowRun
            };
            GroundPaintedAccentCompanionPairLayout[] pairLayouts =
            {
                GroundPaintedAccentCompanionPairLayout.SteppedContinuation,
                GroundPaintedAccentCompanionPairLayout.ShoulderContact,
                GroundPaintedAccentCompanionPairLayout.OffsetEcho,
                GroundPaintedAccentCompanionPairLayout.ShallowOffset
            };

            for (int layoutIndex = 0; layoutIndex < 4; layoutIndex++)
            {
                int count =
                    tripletLayoutCounts != null &&
                    layoutIndex < tripletLayoutCounts.Count
                        ? Mathf.Max(0, tripletLayoutCounts[layoutIndex])
                        : 0;
                for (int index = 0; index < count; index++)
                {
                    specs.Add(
                        new AuthoritativeClusterSpec(
                            3,
                            GroundPaintedAccentCompanionPairLayout.None,
                            tripletLayouts[layoutIndex],
                            ordinal++));
                }
            }

            for (int layoutIndex = 0; layoutIndex < 4; layoutIndex++)
            {
                int count =
                    pairLayoutCounts != null &&
                    layoutIndex < pairLayoutCounts.Count
                        ? Mathf.Max(0, pairLayoutCounts[layoutIndex])
                        : 0;
                for (int index = 0; index < count; index++)
                {
                    specs.Add(
                        new AuthoritativeClusterSpec(
                            2,
                            pairLayouts[layoutIndex],
                            GroundPaintedAccentCompanionTripletLayout.None,
                            ordinal++));
                }
            }

            specs.Sort(
                (left, right) =>
                {
                    uint leftHash =
                        HashAuthoritative(
                            (uint)(left.Ordinal + 1),
                            (uint)(seedOffset + 0x4F31));
                    uint rightHash =
                        HashAuthoritative(
                            (uint)(right.Ordinal + 1),
                            (uint)(seedOffset + 0x4F31));
                    int sizeComparison = right.Size.CompareTo(left.Size);
                    if (sizeComparison != 0)
                    {
                        return sizeComparison;
                    }

                    return leftHash.CompareTo(rightHash);
                });
            return specs;
        }

        private static void ResolveAuthoritativeMarkSelectionScores(
            List<AuthoritativeProjectedMark> marks,
            float accentBias)
        {
            if (marks == null || marks.Count == 0)
            {
                return;
            }

            float maximumLength = 0f;
            for (int index = 0; index < marks.Count; index++)
            {
                maximumLength =
                    Mathf.Max(maximumLength, marks[index].Stroke.AuthoredLength);
            }
            float densityRadius =
                Mathf.Max(
                    MinimumAuthoritativeDensityRadius,
                    maximumLength * AuthoritativeDensityRadiusLengthMultiplier);
            float densityRadiusSquared = densityRadius * densityRadius;
            float densityMinimum = float.PositiveInfinity;
            float densityMaximum = float.NegativeInfinity;

            for (int leftIndex = 0; leftIndex < marks.Count; leftIndex++)
            {
                float density = 0f;
                Vector2 left = marks[leftIndex].Centroid;
                for (int rightIndex = 0; rightIndex < marks.Count; rightIndex++)
                {
                    if (rightIndex == leftIndex)
                    {
                        continue;
                    }

                    float distanceSquared =
                        (marks[rightIndex].Centroid - left).sqrMagnitude;
                    if (distanceSquared > densityRadiusSquared)
                    {
                        continue;
                    }

                    density +=
                        1f - Mathf.Sqrt(distanceSquared) / densityRadius;
                }

                marks[leftIndex].DensityScore = density;
                densityMinimum = Mathf.Min(densityMinimum, density);
                densityMaximum = Mathf.Max(densityMaximum, density);
            }

            float densityRange = Mathf.Max(0.0001f, densityMaximum - densityMinimum);
            float bias = Mathf.Clamp01(accentBias);
            for (int index = 0; index < marks.Count; index++)
            {
                float normalizedDensity =
                    (marks[index].DensityScore - densityMinimum) / densityRange;
                float deterministicOrder =
                    HashAuthoritative01(
                        (uint)marks[index].Stroke.Seed,
                        0xB5297A4Du);
                marks[index].SelectionScore =
                    Mathf.Lerp(deterministicOrder, normalizedDensity, bias);
            }
        }

        private static int CompareAuthoritativeProjectedMarks(
            AuthoritativeProjectedMark left,
            AuthoritativeProjectedMark right)
        {
            int scoreComparison =
                right.SelectionScore.CompareTo(left.SelectionScore);
            if (scoreComparison != 0)
            {
                return scoreComparison;
            }

            int seedComparison = left.Stroke.Seed.CompareTo(right.Stroke.Seed);
            if (seedComparison != 0)
            {
                return seedComparison;
            }

            return left.SourceIndex.CompareTo(right.SourceIndex);
        }

        private static bool TryBuildAuthoritativeProjectedCluster(
            AuthoritativeClusterSpec spec,
            int clusterIndex,
            IReadOnlyList<AuthoritativeProjectedMark> marks,
            IReadOnlyList<int> availableMarkIndices,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            IReadOnlyList<GroundPaintedAccentRiverExclusionSnapshot> rivers,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            Vector2 localNorth,
            GroundPaintedAccentProjectedGlyphScratch scratch,
            GroundPaintedAccentProjectedGlyphTimingAccumulator timing,
            GroundPaintedAccentClusterBuildAuditAccumulator clusterAudit,
            GroundPaintedAccentNearParallelAuditAccumulator
                nearParallelAudit,
            GroundPaintedAccentInternalOverlapAuditAccumulator
                internalOverlapAudit,
            IReadOnlyList<GroundPaintedAccentProjectedGlyph> acceptedGlyphs,
            ProjectedGlyphSpatialIndex acceptedGlyphSpatialIndex,
            out int[] selectedMarkIndices,
            out List<GroundPaintedAccentProjectedGlyph> clusterGlyphs,
            out ProjectedCompanionFallbackReason fallbackReason,
            out int buildAttempts,
            out bool exhaustedAttemptBudget,
            out ProjectedClusterQualityRejections qualityRejections)
        {
            selectedMarkIndices = Array.Empty<int>();
            clusterGlyphs = new List<GroundPaintedAccentProjectedGlyph>(spec.Size);
            fallbackReason = ProjectedCompanionFallbackReason.Incomplete;
            buildAttempts = 0;
            exhaustedAttemptBudget = false;
            qualityRejections = default;
            if (marks == null ||
                availableMarkIndices == null ||
                availableMarkIndices.Count < spec.Size)
            {
                return false;
            }

            int availableCount = availableMarkIndices.Count;
            uint specHash =
                HashAuthoritative(
                    (uint)(spec.Ordinal + 1),
                    (uint)(feature.SeedOffset + 0x792D));
            int start = (int)(specHash % (uint)availableCount);
            ProjectedCompanionFallbackReason lastReason =
                ProjectedCompanionFallbackReason.Contact;

            int maximumAttempts =
                Mathf.Min(
                    MaximumAuthoritativeClusterBuildAttempts,
                    Mathf.Max(12, availableCount * 2));
            for (int attempt = 0; attempt < maximumAttempts; attempt++)
            {
                long donorSelectionStartedAt =
                    System.Diagnostics.Stopwatch.GetTimestamp();
                buildAttempts++;
                int primaryPosition =
                    (start + attempt * 17) % availableCount;
                int secondaryPosition =
                    (start + 1 + attempt * 29) % availableCount;
                if (secondaryPosition == primaryPosition)
                {
                    secondaryPosition = (secondaryPosition + 1) % availableCount;
                }

                int tertiaryPosition = -1;
                if (spec.Size >= 3)
                {
                    tertiaryPosition =
                        (start + 2 + attempt * 43) % availableCount;
                    int guard = 0;
                    while ((tertiaryPosition == primaryPosition ||
                            tertiaryPosition == secondaryPosition) &&
                           guard < availableCount)
                    {
                        tertiaryPosition =
                            (tertiaryPosition + 1) % availableCount;
                        guard++;
                    }
                    if (tertiaryPosition == primaryPosition ||
                        tertiaryPosition == secondaryPosition)
                    {
                        timing.ClusterDonorSelectionTicks +=
                            System.Diagnostics.Stopwatch.GetTimestamp() -
                            donorSelectionStartedAt;
                        continue;
                    }
                }

                int[] candidateIndices =
                    spec.Size >= 3
                        ? new[]
                        {
                            availableMarkIndices[primaryPosition],
                            availableMarkIndices[secondaryPosition],
                            availableMarkIndices[tertiaryPosition]
                        }
                        : new[]
                        {
                            availableMarkIndices[primaryPosition],
                            availableMarkIndices[secondaryPosition]
                        };
                timing.ClusterDonorSelectionTicks +=
                    System.Diagnostics.Stopwatch.GetTimestamp() -
                    donorSelectionStartedAt;
                if (!TryBuildAuthoritativeProjectedClusterCandidate(
                        spec,
                        clusterIndex,
                        marks,
                        candidateIndices,
                        baseSurface,
                        feature,
                        rivers,
                        modifiers,
                        localNorth,
                        scratch,
                        timing,
                        clusterAudit,
                        nearParallelAudit,
                        internalOverlapAudit,
                        acceptedGlyphs,
                        acceptedGlyphSpatialIndex,
                        out List<GroundPaintedAccentProjectedGlyph>
                            candidateGlyphs,
                        out lastReason,
                        out ProjectedClusterQualityRejections
                            candidateQualityRejections))
                {
                    qualityRejections.Accumulate(
                        candidateQualityRejections);
                    continue;
                }

                qualityRejections.Accumulate(candidateQualityRejections);
                selectedMarkIndices = candidateIndices;
                clusterGlyphs = candidateGlyphs;
                fallbackReason = ProjectedCompanionFallbackReason.None;
                return true;
            }

            exhaustedAttemptBudget =
                maximumAttempts > 0 && buildAttempts >= maximumAttempts;
            fallbackReason = lastReason;
            return false;
        }

        private static bool TryBuildAuthoritativeProjectedClusterCandidate(
            AuthoritativeClusterSpec spec,
            int clusterIndex,
            IReadOnlyList<AuthoritativeProjectedMark> marks,
            IReadOnlyList<int> selectedMarkIndices,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            IReadOnlyList<GroundPaintedAccentRiverExclusionSnapshot> rivers,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            Vector2 localNorth,
            GroundPaintedAccentProjectedGlyphScratch scratch,
            GroundPaintedAccentProjectedGlyphTimingAccumulator timing,
            GroundPaintedAccentClusterBuildAuditAccumulator clusterAudit,
            GroundPaintedAccentNearParallelAuditAccumulator
                nearParallelAudit,
            GroundPaintedAccentInternalOverlapAuditAccumulator
                internalOverlapAudit,
            IReadOnlyList<GroundPaintedAccentProjectedGlyph> acceptedGlyphs,
            ProjectedGlyphSpatialIndex acceptedGlyphSpatialIndex,
            out List<GroundPaintedAccentProjectedGlyph> clusterGlyphs,
            out ProjectedCompanionFallbackReason fallbackReason,
            out ProjectedClusterQualityRejections qualityRejections)
        {
            clusterGlyphs =
                new List<GroundPaintedAccentProjectedGlyph>(spec.Size);
            fallbackReason = ProjectedCompanionFallbackReason.None;
            qualityRejections = default;
            if (selectedMarkIndices == null ||
                selectedMarkIndices.Count != spec.Size)
            {
                fallbackReason = ProjectedCompanionFallbackReason.Incomplete;
                return false;
            }

            long prototypePreparationStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
            List<ProjectedGlyphPrototype> prototypes =
                new List<ProjectedGlyphPrototype>(spec.Size);
            for (int memberIndex = 0;
                 memberIndex < selectedMarkIndices.Count;
                 memberIndex++)
            {
                int markIndex = selectedMarkIndices[memberIndex];
                if (markIndex < 0 || markIndex >= marks.Count)
                {
                    timing.ClusterPrototypePreparationTicks +=
                        System.Diagnostics.Stopwatch.GetTimestamp() -
                        prototypePreparationStartedAt;
                    fallbackReason =
                        ProjectedCompanionFallbackReason.Incomplete;
                    return false;
                }

                AuthoritativeProjectedMark mark = marks[markIndex];
                GroundPaintedAccentCompanionMemberRole role =
                    memberIndex == 0
                        ? GroundPaintedAccentCompanionMemberRole.Primary
                        : memberIndex == 1
                            ? GroundPaintedAccentCompanionMemberRole.Secondary
                            : GroundPaintedAccentCompanionMemberRole.Tertiary;
                GroundPaintedAccentSurfaceStroke clusteredStroke =
                    mark.Stroke.WithCompanionMetadata(
                        clusterIndex,
                        role,
                        spec.Size,
                        spec.Size == 2
                            ? spec.PairLayout
                            : GroundPaintedAccentCompanionPairLayout.None,
                        mark.Stroke.ToVariant());
                prototypes.Add(
                    mark.Prototype.CloneWithStroke(clusteredStroke));
            }

            PrepareAuthoritativeClusterPrototypePositions(
                prototypes,
                spec,
                feature,
                localNorth);
            timing.ClusterPrototypePreparationTicks +=
                System.Diagnostics.Stopwatch.GetTimestamp() -
                prototypePreparationStartedAt;

            long contactSolvingStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
            if (!TryComposeProjectedCluster(
                    prototypes,
                    feature,
                    localNorth,
                    scratch,
                    timing,
                    nearParallelAudit,
                    internalOverlapAudit,
                    out List<ProjectedClusterContact> contacts,
                    ref qualityRejections))
            {
                long contactSolvingTicks =
                    System.Diagnostics.Stopwatch.GetTimestamp() -
                    contactSolvingStartedAt;
                timing.ClusterContactSolvingTicks += contactSolvingTicks;
                timing.ClusterCompositionTicks += contactSolvingTicks;
                fallbackReason = ProjectedCompanionFallbackReason.Contact;
                return false;
            }
            long resolvedContactSolvingTicks =
                System.Diagnostics.Stopwatch.GetTimestamp() -
                contactSolvingStartedAt;
            timing.ClusterContactSolvingTicks += resolvedContactSolvingTicks;
            timing.ClusterCompositionTicks += resolvedContactSolvingTicks;

            long internalValidationStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
            long finalSilhouetteOverlapStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
            bool validFinalSilhouette =
                HasValidProjectedClusterSilhouette(
                    prototypes,
                    contacts,
                    out bool finalSweptWidthOverlap,
                    internalOverlapAudit,
                    scratch);
            timing.ClusterFinalSilhouetteOverlapValidationTicks +=
                System.Diagnostics.Stopwatch.GetTimestamp() -
                finalSilhouetteOverlapStartedAt;
            if (!validFinalSilhouette)
            {
                if (finalSweptWidthOverlap)
                {
                    qualityRejections.SweptWidthInternalOverlap++;
                }
                long internalValidationTicks =
                    System.Diagnostics.Stopwatch.GetTimestamp() -
                    internalValidationStartedAt;
                timing.ClusterInternalValidationTicks +=
                    internalValidationTicks;
                timing.ClusterCompositionTicks += internalValidationTicks;
                fallbackReason = ProjectedCompanionFallbackReason.Contact;
                return false;
            }

            if (prototypes.Count == 3 &&
                HasCrowdedProjectedTripletJunctions(prototypes, contacts))
            {
                qualityRejections.CrowdedTripletJunction++;
                long internalValidationTicks =
                    System.Diagnostics.Stopwatch.GetTimestamp() -
                    internalValidationStartedAt;
                timing.ClusterInternalValidationTicks +=
                    internalValidationTicks;
                timing.ClusterCompositionTicks += internalValidationTicks;
                fallbackReason = ProjectedCompanionFallbackReason.Contact;
                return false;
            }

            if (prototypes.Count == 3 &&
                HasProjectedTripletFreeEndPseudoContact(prototypes, contacts))
            {
                qualityRejections.TripletFreeEndPseudoContact++;
                long internalValidationTicks =
                    System.Diagnostics.Stopwatch.GetTimestamp() -
                    internalValidationStartedAt;
                timing.ClusterInternalValidationTicks +=
                    internalValidationTicks;
                timing.ClusterCompositionTicks += internalValidationTicks;
                fallbackReason = ProjectedCompanionFallbackReason.Contact;
                return false;
            }

            if (prototypes.Count == 3 &&
                HasSeverelyCompressedProjectedTriplet(prototypes))
            {
                qualityRejections.SeverelyCompressedTriplet++;
                long internalValidationTicks =
                    System.Diagnostics.Stopwatch.GetTimestamp() -
                    internalValidationStartedAt;
                timing.ClusterInternalValidationTicks +=
                    internalValidationTicks;
                timing.ClusterCompositionTicks += internalValidationTicks;
                fallbackReason = ProjectedCompanionFallbackReason.Contact;
                return false;
            }

            long internalValidationTicksResolved =
                System.Diagnostics.Stopwatch.GetTimestamp() -
                internalValidationStartedAt;
            timing.ClusterInternalValidationTicks +=
                internalValidationTicksResolved;
            timing.ClusterCompositionTicks += internalValidationTicksResolved;

            long externalConflictStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
            if (HasProjectedClusterExternalConflict(
                    prototypes,
                    acceptedGlyphs,
                    acceptedGlyphSpatialIndex,
                    clusterAudit))
            {
                long externalConflictTicks =
                    System.Diagnostics.Stopwatch.GetTimestamp() -
                    externalConflictStartedAt;
                timing.ClusterExternalConflictTicks += externalConflictTicks;
                timing.ClusterCompositionTicks += externalConflictTicks;
                fallbackReason =
                    ProjectedCompanionFallbackReason.ExternalConflict;
                return false;
            }
            long externalConflictTicksResolved =
                System.Diagnostics.Stopwatch.GetTimestamp() -
                externalConflictStartedAt;
            timing.ClusterExternalConflictTicks +=
                externalConflictTicksResolved;
            timing.ClusterCompositionTicks += externalConflictTicksResolved;

            long clusterSurfaceValidationStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
            for (int memberIndex = 0;
                 memberIndex < prototypes.Count;
                 memberIndex++)
            {
                if (!TryFinalizeProjectedGlyph(
                        prototypes[memberIndex],
                        baseSurface,
                        rivers,
                        modifiers,
                        scratch,
                        timing,
                        out GroundPaintedAccentProjectedGlyph glyph,
                        out _))
                {
                    timing.ClusterSurfaceValidationTicks +=
                        System.Diagnostics.Stopwatch.GetTimestamp() -
                        clusterSurfaceValidationStartedAt;
                    clusterGlyphs.Clear();
                    fallbackReason = ProjectedCompanionFallbackReason.Surface;
                    return false;
                }

                clusterGlyphs.Add(glyph);
            }

            timing.ClusterSurfaceValidationTicks +=
                System.Diagnostics.Stopwatch.GetTimestamp() -
                clusterSurfaceValidationStartedAt;
            return clusterGlyphs.Count == spec.Size;
        }

        private static void PrepareAuthoritativeClusterPrototypePositions(
            List<ProjectedGlyphPrototype> prototypes,
            AuthoritativeClusterSpec spec,
            GroundSurfaceFeatureRecipe feature,
            Vector2 localNorth)
        {
            if (prototypes == null || prototypes.Count < 2)
            {
                return;
            }

            Vector2 primaryCentroid =
                ResolveProjectedCentroid(prototypes[0].ProjectedPoints);
            Vector2 primaryAxis =
                ResolveProjectedPrototypeAxis(prototypes[0]);
            uint clusterHash =
                HashAuthoritative(
                    (uint)prototypes[0].Stroke.Seed,
                    (uint)(spec.Ordinal + 0x6D2B));
            float alongSign =
                (clusterHash & 1u) == 0u ? 1f : -1f;
            float verticalSign =
                (clusterHash & 2u) == 0u ? 1f : -1f;
            Vector2 alongAxis = primaryAxis * alongSign;
            Vector2 resolvedNorth =
                localNorth.sqrMagnitude > 0.000001f
                    ? localNorth.normalized
                    : Vector2.up;
            Vector2 pairNormal =
                new Vector2(-primaryAxis.y, primaryAxis.x);
            if (Vector2.Dot(pairNormal, resolvedNorth) < 0f)
            {
                pairNormal = -pairNormal;
            }
            pairNormal *= verticalSign;
            resolvedNorth *= verticalSign;

            float minimumLength = float.PositiveInfinity;
            float averageLength = 0f;
            for (int index = 0; index < prototypes.Count; index++)
            {
                float length = prototypes[index].Stroke.AuthoredLength;
                minimumLength = Mathf.Min(minimumLength, length);
                averageLength += length;
            }
            minimumLength =
                float.IsInfinity(minimumLength) ? 0.5f : minimumLength;
            averageLength /= prototypes.Count;
            float verticality =
                SmoothAuthoritative01(feature.PaintedAccentClusterVerticality);
            float alongSpacing =
                averageLength * AuthoritativeAlongSpacingLengthFraction;

            if (spec.Size == 2)
            {
                float stepFraction;
                switch (spec.PairLayout)
                {
                    case GroundPaintedAccentCompanionPairLayout.ShoulderContact:
                        stepFraction = PairShoulderStepLengthFraction;
                        break;
                    case GroundPaintedAccentCompanionPairLayout.OffsetEcho:
                        stepFraction = PairOffsetStepLengthFraction;
                        break;
                    case GroundPaintedAccentCompanionPairLayout.ShallowOffset:
                        stepFraction = PairShallowStepLengthFraction;
                        break;
                    case GroundPaintedAccentCompanionPairLayout.SteppedContinuation:
                    default:
                        stepFraction = PairSteppedStepLengthFraction;
                        break;
                }

                float stepScale =
                    spec.PairLayout ==
                        GroundPaintedAccentCompanionPairLayout.ShallowOffset
                        ? Mathf.Lerp(0.55f, 1f, verticality)
                        : Mathf.Lerp(0.45f, 1f, verticality);
                Vector2 targetCentroid =
                    primaryCentroid +
                    alongAxis * alongSpacing +
                    pairNormal * (minimumLength * stepFraction * stepScale);
                TranslatePrototypeToCentroid(prototypes[1], targetCentroid);
                return;
            }

            float tripletStep =
                minimumLength *
                TripletStepLengthFraction *
                Mathf.Lerp(0.40f, 1f, verticality);
            Vector2 secondaryOffset;
            Vector2 tertiaryOffset;
            switch (spec.TripletLayout)
            {
                case GroundPaintedAccentCompanionTripletLayout.CrownRun:
                    secondaryOffset =
                        alongAxis * alongSpacing +
                        resolvedNorth * (tripletStep * 1.35f);
                    tertiaryOffset =
                        alongAxis * (alongSpacing * 2f) +
                        resolvedNorth * (tripletStep * 0.10f);
                    break;
                case GroundPaintedAccentCompanionTripletLayout.BrokenTerrace:
                    secondaryOffset =
                        alongAxis * alongSpacing +
                        resolvedNorth * (tripletStep * 1.10f);
                    tertiaryOffset =
                        alongAxis * (alongSpacing * 2f) -
                        resolvedNorth * (tripletStep * 0.45f);
                    break;
                case GroundPaintedAccentCompanionTripletLayout.ShallowRun:
                    secondaryOffset =
                        alongAxis * alongSpacing +
                        resolvedNorth * (tripletStep * 0.45f);
                    tertiaryOffset =
                        alongAxis * (alongSpacing * 2f) +
                        resolvedNorth * (tripletStep * 0.15f);
                    break;
                case GroundPaintedAccentCompanionTripletLayout.SteppedRun:
                default:
                    secondaryOffset =
                        alongAxis * alongSpacing +
                        resolvedNorth * tripletStep;
                    tertiaryOffset =
                        alongAxis * (alongSpacing * 2f) +
                        resolvedNorth * (tripletStep * 2f);
                    break;
            }

            TranslatePrototypeToCentroid(
                prototypes[1],
                primaryCentroid + secondaryOffset);
            TranslatePrototypeToCentroid(
                prototypes[2],
                primaryCentroid + tertiaryOffset);
        }

        private static Vector2 ResolveProjectedPrototypeAxis(
            ProjectedGlyphPrototype prototype)
        {
            if (prototype == null ||
                prototype.ProjectedPoints == null ||
                prototype.ProjectedPoints.Length < 2)
            {
                return Vector2.right;
            }

            Vector2 axis =
                prototype.ProjectedPoints[
                    prototype.ProjectedPoints.Length - 1] -
                prototype.ProjectedPoints[0];
            return axis.sqrMagnitude > 0.000001f
                ? axis.normalized
                : Vector2.right;
        }

        private static void TranslatePrototypeToCentroid(
            ProjectedGlyphPrototype prototype,
            Vector2 targetCentroid)
        {
            if (prototype == null || prototype.ProjectedPoints == null)
            {
                return;
            }

            Vector2 translation =
                targetCentroid -
                ResolveProjectedCentroid(prototype.ProjectedPoints);
            TranslateProjectedPoints(prototype.ProjectedPoints, translation);
        }

        private static void ReconcileAuthoritativeClustersAgainstFinalIndependents(
            IReadOnlyList<AuthoritativeProjectedMark> marks,
            List<AuthoritativeClusterRecord> records,
            List<GroundPaintedAccentProjectedGlyph> glyphs,
            List<GroundPaintedAccentProjectedGlyph>
                reconciliationIndependentGlyphs,
            int[] acceptedPairLayouts,
            int[] acceptedTripletLayouts,
            ref int companionClustersAccepted,
            ref int companionClustersFallback,
            ref int companionPairsAccepted,
            ref int companionTripletsAccepted,
            ref int pairShortfall,
            ref int tripletShortfall,
            ref int companionRejectedExternalConflict,
            ref int companionPairRejectedExternalConflict,
            ref int clustersRemovedDuringReconciliation,
            GroundPaintedAccentClusterBuildAuditAccumulator clusterAudit)
        {
            if (records == null ||
                records.Count == 0 ||
                reconciliationIndependentGlyphs == null ||
                reconciliationIndependentGlyphs.Count == 0)
            {
                return;
            }

            // Each record remembers the late-independent prefix already
            // validated. A removal appends new fallback independents and the
            // next pass checks only those newly introduced relationships.
            int maximumPasses = records.Count;
            for (int passIndex = 0;
                 passIndex < maximumPasses && records.Count > 0;
                 passIndex++)
            {
                bool changed = false;
                for (int recordIndex = records.Count - 1;
                     recordIndex >= 0;
                     recordIndex--)
                {
                    AuthoritativeClusterRecord record = records[recordIndex];
                    GroundPaintedAccentProjectedGlyph[] clusterGlyphs =
                        record.ClusterGlyphs ??
                        Array.Empty<GroundPaintedAccentProjectedGlyph>();
                    int independentCount =
                        reconciliationIndependentGlyphs.Count;
                    int checkedIndependentCount =
                        Mathf.Clamp(
                            record.ReconciliationIndependentCountChecked,
                            0,
                            independentCount);
                    if (clusterAudit != null)
                    {
                        clusterAudit.ReconciliationClustersExamined++;
                        long memberCount = clusterGlyphs.Length;
                        long newRelationships =
                            memberCount *
                            (independentCount - checkedIndependentCount);
                        long legacyRelationships =
                            memberCount *
                            Mathf.Max(0, glyphs.Count - clusterGlyphs.Length);
                        clusterAudit
                            .ReconciliationPreviouslyValidatedRelationshipsSkipped +=
                                memberCount * checkedIndependentCount;
                        clusterAudit
                            .ReconciliationLegacyFullListComparisonsAvoided +=
                                Math.Max(
                                    0L,
                                    legacyRelationships - newRelationships);
                    }

                    if (!HasFinalProjectedClusterExternalConflict(
                            clusterGlyphs,
                            reconciliationIndependentGlyphs,
                            checkedIndependentCount,
                            clusterAudit))
                    {
                        record.ReconciliationIndependentCountChecked =
                            independentCount;
                        continue;
                    }

                    for (int glyphIndex = glyphs.Count - 1;
                         glyphIndex >= 0;
                         glyphIndex--)
                    {
                        if (glyphs[glyphIndex].CompanionClusterIndex ==
                            record.ClusterIndex)
                        {
                            glyphs.RemoveAt(glyphIndex);
                        }
                    }

                    for (int memberIndex = 0;
                         memberIndex < record.MarkIndices.Length;
                         memberIndex++)
                    {
                        GroundPaintedAccentProjectedGlyph independentGlyph =
                            marks[record.MarkIndices[memberIndex]]
                                .IndependentGlyph;
                        glyphs.Add(independentGlyph);
                        reconciliationIndependentGlyphs.Add(independentGlyph);
                    }

                    companionClustersAccepted =
                        Mathf.Max(0, companionClustersAccepted - 1);
                    companionClustersFallback++;
                    companionRejectedExternalConflict++;
                    clustersRemovedDuringReconciliation++;
                    if (record.Spec.Size >= 3)
                    {
                        companionTripletsAccepted =
                            Mathf.Max(0, companionTripletsAccepted - 1);
                        tripletShortfall++;
                        int layoutIndex =
                            ResolveAuthoritativeTripletLayoutIndex(
                                record.Spec.TripletLayout);
                        acceptedTripletLayouts[layoutIndex] =
                            Mathf.Max(
                                0,
                                acceptedTripletLayouts[layoutIndex] - 1);
                    }
                    else
                    {
                        companionPairsAccepted =
                            Mathf.Max(0, companionPairsAccepted - 1);
                        companionPairRejectedExternalConflict++;
                        pairShortfall++;
                        int layoutIndex =
                            ResolveAuthoritativePairLayoutIndex(
                                record.Spec.PairLayout);
                        acceptedPairLayouts[layoutIndex] =
                            Mathf.Max(0, acceptedPairLayouts[layoutIndex] - 1);
                    }

                    records.RemoveAt(recordIndex);
                    changed = true;
                    break;
                }

                if (!changed)
                {
                    break;
                }
            }
        }

        private static int ResolveAuthoritativePairLayoutIndex(
            GroundPaintedAccentCompanionPairLayout layout)
        {
            switch (layout)
            {
                case GroundPaintedAccentCompanionPairLayout.ShoulderContact:
                    return 1;
                case GroundPaintedAccentCompanionPairLayout.OffsetEcho:
                    return 2;
                case GroundPaintedAccentCompanionPairLayout.ShallowOffset:
                    return 3;
                case GroundPaintedAccentCompanionPairLayout.SteppedContinuation:
                default:
                    return 0;
            }
        }

        private static int ResolveAuthoritativeTripletLayoutIndex(
            GroundPaintedAccentCompanionTripletLayout layout)
        {
            switch (layout)
            {
                case GroundPaintedAccentCompanionTripletLayout.CrownRun:
                    return 1;
                case GroundPaintedAccentCompanionTripletLayout.BrokenTerrace:
                    return 2;
                case GroundPaintedAccentCompanionTripletLayout.ShallowRun:
                    return 3;
                case GroundPaintedAccentCompanionTripletLayout.SteppedRun:
                default:
                    return 0;
            }
        }

        private static uint HashAuthoritative(uint value, uint salt)
        {
            uint hash = value ^ (salt * 0x9E3779B9u);
            hash ^= hash >> 16;
            hash *= 0x7FEB352Du;
            hash ^= hash >> 15;
            hash *= 0x846CA68Bu;
            hash ^= hash >> 16;
            return hash;
        }

        private static float HashAuthoritative01(uint value, uint salt)
        {
            return (HashAuthoritative(value, salt) & 0x00FFFFFFu) /
                   16777215f;
        }

        private static float SmoothAuthoritative01(float value)
        {
            value = Mathf.Clamp01(value);
            return value * value * (3f - 2f * value);
        }

        private static void ProcessProjectedStroke(
            GroundPaintedAccentSurfaceStroke stroke,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            IReadOnlyList<GroundPaintedAccentRiverExclusionSnapshot> rivers,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            Vector2 localNorth,
            GroundPaintedAccentProjectedGlyphScratch scratch,
            GroundPaintedAccentProjectedGlyphTimingAccumulator timing,
            List<GroundPaintedAccentProjectedGlyph> glyphs,
            List<GroundPaintedAccentProjectedGlyphRejectionDebugPoint> rejections,
            ref int rejectedSampling,
            ref int rejectedRiver,
            ref int rejectedModifier,
            ref int rejectedBroadSlope,
            ref int rejectedLocalGrade,
            ref int rejectedFamilyShape,
            ref int rejectedSharpTurn)
        {
            if (TryCreateProjectedGlyph(
                    stroke,
                    baseSurface,
                    feature,
                    rivers,
                    modifiers,
                    localNorth,
                    scratch,
                    timing,
                    out GroundPaintedAccentProjectedGlyph glyph,
                    out GroundPaintedAccentProjectedGlyphRejectionReason reason))
            {
                glyphs.Add(glyph);
                return;
            }

            RecordProjectionRejection(
                stroke,
                reason,
                rejections,
                ref rejectedSampling,
                ref rejectedRiver,
                ref rejectedModifier,
                ref rejectedBroadSlope,
                ref rejectedLocalGrade,
                ref rejectedFamilyShape,
                ref rejectedSharpTurn);
        }

        private static void RecordProjectionRejection(
            GroundPaintedAccentSurfaceStroke stroke,
            GroundPaintedAccentProjectedGlyphRejectionReason reason,
            List<GroundPaintedAccentProjectedGlyphRejectionDebugPoint> rejections,
            ref int rejectedSampling,
            ref int rejectedRiver,
            ref int rejectedModifier,
            ref int rejectedBroadSlope,
            ref int rejectedLocalGrade,
            ref int rejectedFamilyShape,
            ref int rejectedSharpTurn)
        {
            switch (reason)
            {
                case GroundPaintedAccentProjectedGlyphRejectionReason.River:
                    rejectedRiver++;
                    break;
                case GroundPaintedAccentProjectedGlyphRejectionReason.ModifierExclusion:
                    rejectedModifier++;
                    break;
                case GroundPaintedAccentProjectedGlyphRejectionReason.BroadSlope:
                    rejectedBroadSlope++;
                    break;
                case GroundPaintedAccentProjectedGlyphRejectionReason.LocalGrade:
                    rejectedLocalGrade++;
                    break;
                case GroundPaintedAccentProjectedGlyphRejectionReason.FamilyShape:
                    rejectedFamilyShape++;
                    break;
                case GroundPaintedAccentProjectedGlyphRejectionReason.SharpTurn:
                    rejectedSharpTurn++;
                    break;
                case GroundPaintedAccentProjectedGlyphRejectionReason.Sampling:
                default:
                    rejectedSampling++;
                    break;
            }

            Vector3 markerPosition =
                stroke.LocalPoints != null && stroke.LocalPoints.Length > 0
                    ? stroke.LocalPoints[stroke.LocalPoints.Length / 2]
                    : Vector3.zero;
            markerPosition.y += RejectionMarkerSurfaceOffset;
            rejections.Add(
                new GroundPaintedAccentProjectedGlyphRejectionDebugPoint(
                    markerPosition,
                    stroke.Family,
                    reason));
        }

        private static int CompareCompanionSurfaceStrokeMembers(
            GroundPaintedAccentSurfaceStroke left,
            GroundPaintedAccentSurfaceStroke right)
        {
            int roleComparison =
                ((int)left.CompanionMemberRole).CompareTo(
                    (int)right.CompanionMemberRole);
            if (roleComparison != 0)
            {
                return roleComparison;
            }

            return left.Seed.CompareTo(right.Seed);
        }

        private static void
            ReconcileAcceptedProjectedClustersAgainstFinalIndependents(
                IReadOnlyDictionary<int, List<GroundPaintedAccentSurfaceStroke>>
                    clusters,
                HashSet<int> acceptedClusterKeys,
                GroundHeightFieldSnapshot baseSurface,
                GroundSurfaceFeatureRecipe feature,
                IReadOnlyList<GroundPaintedAccentRiverExclusionSnapshot> rivers,
                IReadOnlyList<GroundModifierSnapshot> modifiers,
                Vector2 localNorth,
                GroundPaintedAccentProjectedGlyphScratch scratch,
                GroundPaintedAccentProjectedGlyphTimingAccumulator timing,
                List<GroundPaintedAccentProjectedGlyph> glyphs,
                List<GroundPaintedAccentProjectedGlyphRejectionDebugPoint>
                    rejections,
                ref int rejectedSampling,
                ref int rejectedRiver,
                ref int rejectedModifier,
                ref int rejectedBroadSlope,
                ref int rejectedLocalGrade,
                ref int rejectedFamilyShape,
                ref int rejectedSharpTurn,
                ref int companionClustersAccepted,
                ref int companionClustersFallback,
                ref int companionPairsAccepted,
                ref int companionTripletsAccepted,
                ref int companionRejectedExternalConflict,
                ref int companionPairRejectedExternalConflict,
                ref int companionClustersRemovedDuringReconciliation)
        {
            if (acceptedClusterKeys == null ||
                acceptedClusterKeys.Count == 0)
            {
                return;
            }

            int maximumPasses = acceptedClusterKeys.Count;
            for (int passIndex = 0;
                 passIndex < maximumPasses && acceptedClusterKeys.Count > 0;
                 passIndex++)
            {
                bool changed = false;
                List<int> clusterKeys =
                    new List<int>(acceptedClusterKeys);
                clusterKeys.Sort();
                for (int clusterIndex = 0;
                     clusterIndex < clusterKeys.Count;
                     clusterIndex++)
                {
                    int clusterKey = clusterKeys[clusterIndex];
                    long conflictStartedAt =
                        System.Diagnostics.Stopwatch.GetTimestamp();
                    bool conflicts =
                        HasFinalProjectedClusterExternalConflict(
                            clusterKey,
                            glyphs);
                    timing.ClusterCompositionTicks +=
                        System.Diagnostics.Stopwatch.GetTimestamp() -
                        conflictStartedAt;
                    if (!conflicts)
                    {
                        continue;
                    }

                    if (!clusters.TryGetValue(
                            clusterKey,
                            out List<GroundPaintedAccentSurfaceStroke> members))
                    {
                        acceptedClusterKeys.Remove(clusterKey);
                        changed = true;
                        break;
                    }

                    int intendedSize =
                        members.Count > 0
                            ? members[0].IntendedCompanionClusterSize
                            : 0;
                    for (int glyphIndex = glyphs.Count - 1;
                         glyphIndex >= 0;
                         glyphIndex--)
                    {
                        if (glyphs[glyphIndex].CompanionClusterIndex ==
                            clusterKey)
                        {
                            glyphs.RemoveAt(glyphIndex);
                        }
                    }

                    companionClustersAccepted =
                        Mathf.Max(0, companionClustersAccepted - 1);
                    companionClustersFallback++;
                    companionClustersRemovedDuringReconciliation++;
                    companionRejectedExternalConflict++;
                    if (intendedSize >= 3)
                    {
                        companionTripletsAccepted =
                            Mathf.Max(0, companionTripletsAccepted - 1);
                    }
                    else
                    {
                        companionPairsAccepted =
                            Mathf.Max(0, companionPairsAccepted - 1);
                        companionPairRejectedExternalConflict++;
                    }

                    for (int memberIndex = 0;
                         memberIndex < members.Count;
                         memberIndex++)
                    {
                        GroundPaintedAccentSurfaceStroke fallbackStroke =
                            members[memberIndex].CreateIndependentFallback();
                        if (!fallbackStroke.IsValid)
                        {
                            RecordProjectionRejection(
                                members[memberIndex],
                                GroundPaintedAccentProjectedGlyphRejectionReason
                                    .Sampling,
                                rejections,
                                ref rejectedSampling,
                                ref rejectedRiver,
                                ref rejectedModifier,
                                ref rejectedBroadSlope,
                                ref rejectedLocalGrade,
                                ref rejectedFamilyShape,
                                ref rejectedSharpTurn);
                            continue;
                        }

                        ProcessProjectedStroke(
                            fallbackStroke,
                            baseSurface,
                            feature,
                            rivers,
                            modifiers,
                            localNorth,
                            scratch,
                            timing,
                            glyphs,
                            rejections,
                            ref rejectedSampling,
                            ref rejectedRiver,
                            ref rejectedModifier,
                            ref rejectedBroadSlope,
                            ref rejectedLocalGrade,
                            ref rejectedFamilyShape,
                            ref rejectedSharpTurn);
                    }

                    acceptedClusterKeys.Remove(clusterKey);
                    changed = true;
                    break;
                }

                if (!changed)
                {
                    break;
                }
            }
        }

        private static bool HasFinalProjectedClusterExternalConflict(
            IReadOnlyList<GroundPaintedAccentProjectedGlyph> clusterGlyphs,
            IReadOnlyList<GroundPaintedAccentProjectedGlyph>
                independentGlyphs,
            int independentStartIndex,
            GroundPaintedAccentClusterBuildAuditAccumulator clusterAudit)
        {
            if (clusterGlyphs == null ||
                independentGlyphs == null ||
                clusterGlyphs.Count == 0)
            {
                return false;
            }

            int startIndex =
                Mathf.Clamp(
                    independentStartIndex,
                    0,
                    independentGlyphs.Count);
            for (int memberIndex = 0;
                 memberIndex < clusterGlyphs.Count;
                 memberIndex++)
            {
                GroundPaintedAccentProjectedGlyph member =
                    clusterGlyphs[memberIndex];
                for (int independentIndex = startIndex;
                     independentIndex < independentGlyphs.Count;
                     independentIndex++)
                {
                    GroundPaintedAccentProjectedGlyph independent =
                        independentGlyphs[independentIndex];
                    if (HasInstrumentedProjectedReconciliationOverlap(
                            member.LocalProjectedPoints,
                            member.HalfWidths,
                            independent.LocalProjectedPoints,
                            independent.HalfWidths,
                            clusterAudit))
                    {
                        if (clusterAudit != null)
                        {
                            clusterAudit.ExternalConflictRejections++;
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool HasFinalProjectedClusterExternalConflict(
            int clusterKey,
            IReadOnlyList<GroundPaintedAccentProjectedGlyph> glyphs,
            GroundPaintedAccentClusterBuildAuditAccumulator clusterAudit = null)
        {
            for (int memberIndex = 0;
                 memberIndex < glyphs.Count;
                 memberIndex++)
            {
                GroundPaintedAccentProjectedGlyph member = glyphs[memberIndex];
                if (member.CompanionClusterIndex != clusterKey)
                {
                    continue;
                }

                for (int otherIndex = 0;
                     otherIndex < glyphs.Count;
                     otherIndex++)
                {
                    if (otherIndex == memberIndex)
                    {
                        continue;
                    }

                    GroundPaintedAccentProjectedGlyph other =
                        glyphs[otherIndex];
                    if (other.CompanionClusterIndex == clusterKey)
                    {
                        continue;
                    }

                    if (HasInstrumentedProjectedExternalOverlap(
                            member.LocalProjectedPoints,
                            member.HalfWidths,
                            other.LocalProjectedPoints,
                            other.HalfWidths,
                            clusterAudit))
                    {
                        if (clusterAudit != null)
                        {
                            clusterAudit.ExternalConflictRejections++;
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TryBuildAtomicProjectedCluster(
            List<GroundPaintedAccentSurfaceStroke> members,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            IReadOnlyList<GroundPaintedAccentRiverExclusionSnapshot> rivers,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            Vector2 localNorth,
            GroundPaintedAccentProjectedGlyphScratch scratch,
            GroundPaintedAccentProjectedGlyphTimingAccumulator timing,
            IReadOnlyList<GroundPaintedAccentProjectedGlyph> acceptedGlyphs,
            out List<GroundPaintedAccentProjectedGlyph> clusterGlyphs,
            out ProjectedCompanionFallbackReason fallbackReason,
            out int pairStepRetentionRejectedCandidates,
            out int pairNearCollinearRejectedCandidates)
        {
            clusterGlyphs = new List<GroundPaintedAccentProjectedGlyph>(3);
            fallbackReason = ProjectedCompanionFallbackReason.None;
            pairStepRetentionRejectedCandidates = 0;
            pairNearCollinearRejectedCandidates = 0;
            if (members == null || members.Count < 2)
            {
                fallbackReason = ProjectedCompanionFallbackReason.Incomplete;
                return false;
            }

            int intendedSize = members[0].IntendedCompanionClusterSize;
            if (intendedSize < 2 ||
                intendedSize > 3 ||
                members.Count != intendedSize ||
                members[0].CompanionMemberRole !=
                    GroundPaintedAccentCompanionMemberRole.Primary ||
                (intendedSize == 2 &&
                 members[0].CompanionPairLayout ==
                     GroundPaintedAccentCompanionPairLayout.None))
            {
                fallbackReason = ProjectedCompanionFallbackReason.Incomplete;
                return false;
            }

            bool hasSecondary = false;
            bool hasTertiary = intendedSize < 3;
            for (int memberIndex = 0;
                 memberIndex < members.Count;
                 memberIndex++)
            {
                GroundPaintedAccentSurfaceStroke member = members[memberIndex];
                if (member.IntendedCompanionClusterSize != intendedSize ||
                    (intendedSize == 2 &&
                     member.CompanionPairLayout !=
                         members[0].CompanionPairLayout))
                {
                    fallbackReason =
                        ProjectedCompanionFallbackReason.Incomplete;
                    return false;
                }

                hasSecondary |=
                    member.CompanionMemberRole ==
                    GroundPaintedAccentCompanionMemberRole.Secondary;
                hasTertiary |=
                    member.CompanionMemberRole ==
                    GroundPaintedAccentCompanionMemberRole.Tertiary;
            }

            if (!hasSecondary || !hasTertiary)
            {
                fallbackReason = ProjectedCompanionFallbackReason.Incomplete;
                return false;
            }

            List<ProjectedGlyphPrototype> prototypes =
                new List<ProjectedGlyphPrototype>(members.Count);
            for (int memberIndex = 0;
                 memberIndex < members.Count;
                 memberIndex++)
            {
                if (!TryBuildProjectedGlyphPrototype(
                        members[memberIndex],
                        feature,
                        localNorth,
                        timing,
                        out ProjectedGlyphPrototype prototype,
                        out _))
                {
                    fallbackReason =
                        ProjectedCompanionFallbackReason.Prototype;
                    return false;
                }

                prototypes.Add(prototype.Clone());
            }

            long compositionStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
            ProjectedClusterQualityRejections qualityRejections = default;
            if (!TryComposeProjectedCluster(
                    prototypes,
                    feature,
                    localNorth,
                    scratch,
                    timing,
                    null,
                    null,
                    out List<ProjectedClusterContact> contacts,
                    ref qualityRejections))
            {
                pairStepRetentionRejectedCandidates =
                    qualityRejections.PairStepRetention;
                pairNearCollinearRejectedCandidates =
                    qualityRejections.PairNearCollinear;
                timing.ClusterCompositionTicks +=
                    System.Diagnostics.Stopwatch.GetTimestamp() -
                    compositionStartedAt;
                fallbackReason = ProjectedCompanionFallbackReason.Contact;
                return false;
            }

            pairStepRetentionRejectedCandidates =
                qualityRejections.PairStepRetention;
            pairNearCollinearRejectedCandidates =
                qualityRejections.PairNearCollinear;

            if (HasProjectedClusterExternalConflict(
                    prototypes,
                    acceptedGlyphs))
            {
                timing.ClusterCompositionTicks +=
                    System.Diagnostics.Stopwatch.GetTimestamp() -
                    compositionStartedAt;
                fallbackReason =
                    ProjectedCompanionFallbackReason.ExternalConflict;
                return false;
            }

            if (!HasValidProjectedClusterSilhouette(prototypes, contacts))
            {
                timing.ClusterCompositionTicks +=
                    System.Diagnostics.Stopwatch.GetTimestamp() -
                    compositionStartedAt;
                fallbackReason = ProjectedCompanionFallbackReason.Contact;
                return false;
            }
            timing.ClusterCompositionTicks +=
                System.Diagnostics.Stopwatch.GetTimestamp() -
                compositionStartedAt;

            for (int memberIndex = 0;
                 memberIndex < prototypes.Count;
                 memberIndex++)
            {
                if (!TryFinalizeProjectedGlyph(
                        prototypes[memberIndex],
                        baseSurface,
                        rivers,
                        modifiers,
                        scratch,
                        timing,
                        out GroundPaintedAccentProjectedGlyph glyph,
                        out _))
                {
                    fallbackReason = ProjectedCompanionFallbackReason.Surface;
                    clusterGlyphs.Clear();
                    return false;
                }

                clusterGlyphs.Add(glyph);
            }

            return clusterGlyphs.Count == intendedSize;
        }

        private static bool TryComposeProjectedCluster(
            List<ProjectedGlyphPrototype> members,
            GroundSurfaceFeatureRecipe feature,
            Vector2 localNorth,
            GroundPaintedAccentProjectedGlyphScratch scratch,
            GroundPaintedAccentProjectedGlyphTimingAccumulator timing,
            GroundPaintedAccentNearParallelAuditAccumulator
                nearParallelAudit,
            GroundPaintedAccentInternalOverlapAuditAccumulator
                internalOverlapAudit,
            out List<ProjectedClusterContact> contacts,
            ref ProjectedClusterQualityRejections qualityRejections)
        {
            contacts = new List<ProjectedClusterContact>(2);
            if (members == null || members.Count < 2 || members.Count > 3)
            {
                return false;
            }

            float tightness =
                feature != null
                    ? Mathf.Clamp01(feature.PaintedAccentCompanionTightness)
                    : 0f;
            Vector2 resolvedLocalNorth =
                localNorth.sqrMagnitude > 0.000001f
                    ? localNorth.normalized
                    : Vector2.up;
            GroundPaintedAccentCompanionPairLayout pairLayout =
                members.Count == 2
                    ? members[0].Stroke.CompanionPairLayout
                    : GroundPaintedAccentCompanionPairLayout.None;
            Vector2[] originalCentroids =
                new Vector2[members.Count];
            for (int memberIndex = 0;
                 memberIndex < members.Count;
                 memberIndex++)
            {
                originalCentroids[memberIndex] =
                    ResolveProjectedCentroid(
                        members[memberIndex].ProjectedPoints);
            }

            if (!TryPlaceProjectedClusterMember(
                    members,
                    originalCentroids,
                    resolvedLocalNorth,
                    1,
                    new[] { 0 },
                    contacts,
                    tightness,
                    members.Count == 2
                        ? MinimumProjectedPairStepRetentionFraction
                        : MinimumProjectedTripletStepRetentionFraction,
                    pairLayout,
                    members.Count == 2,
                    scratch,
                    timing,
                    nearParallelAudit,
                    internalOverlapAudit,
                    ref qualityRejections,
                    out ProjectedClusterContact secondaryContact))
            {
                return false;
            }
            contacts.Add(secondaryContact);

            if (members.Count >= 3)
            {
                if (!TryPlaceProjectedClusterMember(
                        members,
                        originalCentroids,
                        resolvedLocalNorth,
                        2,
                        new[] { 0, 1 },
                        contacts,
                        tightness,
                        MinimumProjectedTripletStepRetentionFraction,
                        GroundPaintedAccentCompanionPairLayout.None,
                        false,
                        scratch,
                        timing,
                        nearParallelAudit,
                        internalOverlapAudit,
                        ref qualityRejections,
                        out ProjectedClusterContact tertiaryContact))
                {
                    return false;
                }
                contacts.Add(tertiaryContact);
            }

            return true;
        }

        private static bool TryPlaceProjectedClusterMember(
            List<ProjectedGlyphPrototype> members,
            IReadOnlyList<Vector2> originalCentroids,
            Vector2 localNorth,
            int movingMemberIndex,
            IReadOnlyList<int> anchorMemberIndices,
            IReadOnlyList<ProjectedClusterContact> existingContacts,
            float tightness,
            float minimumStepRetentionFraction,
            GroundPaintedAccentCompanionPairLayout pairLayout,
            bool countPairStepRetentionRejections,
            GroundPaintedAccentProjectedGlyphScratch scratch,
            GroundPaintedAccentProjectedGlyphTimingAccumulator timing,
            GroundPaintedAccentNearParallelAuditAccumulator
                nearParallelAudit,
            GroundPaintedAccentInternalOverlapAuditAccumulator
                internalOverlapAudit,
            ref ProjectedClusterQualityRejections qualityRejections,
            out ProjectedClusterContact contact)
        {
            contact = default;
            if (members == null ||
                movingMemberIndex <= 0 ||
                movingMemberIndex >= members.Count ||
                originalCentroids == null ||
                originalCentroids.Count != members.Count ||
                anchorMemberIndices == null ||
                anchorMemberIndices.Count == 0)
            {
                return false;
            }

            ProjectedGlyphPrototype moving = members[movingMemberIndex];
            int movingStart = ResolveSafeProjectedContactIndex(moving, true);
            int movingEnd = ResolveSafeProjectedContactIndex(moving, false);
            int[] movingContactIndices = { movingStart, movingEnd };
            float bestScore = float.PositiveInfinity;
            Vector2[] bestPoints = null;
            float[] bestHalfWidths = null;
            ProjectedClusterContact bestContact = default;

            for (int anchorListIndex = 0;
                 anchorListIndex < anchorMemberIndices.Count;
                 anchorListIndex++)
            {
                int anchorMemberIndex = anchorMemberIndices[anchorListIndex];
                if (anchorMemberIndex < 0 ||
                    anchorMemberIndex >= movingMemberIndex)
                {
                    continue;
                }

                ProjectedGlyphPrototype anchor = members[anchorMemberIndex];
                List<int> anchorContactIndices =
                    BuildProjectedAnchorContactIndices(
                        anchor,
                        pairLayout);
                for (int anchorContactIndex = 0;
                     anchorContactIndex < anchorContactIndices.Count;
                     anchorContactIndex++)
                {
                    int anchorPointIndex =
                        anchorContactIndices[anchorContactIndex];
                    if (IsProjectedTripletAttachmentSlotOccupied(
                            members,
                            existingContacts,
                            anchorMemberIndex,
                            anchorPointIndex))
                    {
                        qualityRejections.OccupiedAttachmentSlot++;
                        continue;
                    }

                    for (int movingContactIndex = 0;
                         movingContactIndex < movingContactIndices.Length;
                         movingContactIndex++)
                    {
                        int originalMovingPointIndex =
                            movingContactIndices[movingContactIndex];
                        CreateTrimmedProjectedContactVariant(
                            moving,
                            originalMovingPointIndex,
                            out Vector2[] candidatePoints,
                            out float[] candidateHalfWidths,
                            out int movingPointIndex);
                        Vector2 outward =
                            ResolveProjectedEndpointOutwardDirection(
                                candidatePoints,
                                movingPointIndex);
                        float anchorHalfWidth =
                            anchor.HalfWidths[anchorPointIndex];
                        float movingHalfWidth =
                            candidateHalfWidths[movingPointIndex];
                        if (!TryResolveProjectedEndpointCentreSeparation(
                                anchor.ProjectedPoints,
                                anchorPointIndex,
                                outward,
                                anchorHalfWidth,
                                movingHalfWidth,
                                tightness,
                                pairLayout,
                                out float endpointCentreSeparation))
                        {
                            continue;
                        }
                        Vector2 anchorContactPoint =
                            anchor.ProjectedPoints[anchorPointIndex];
                        Vector2 targetContact =
                            anchorContactPoint -
                            outward * endpointCentreSeparation;
                        if (IsProjectedTripletContactLocusTooClose(
                                members,
                                existingContacts,
                                anchor.ProjectedPoints[anchorPointIndex],
                                Mathf.Min(
                                    anchor.Stroke.AuthoredLength,
                                    moving.Stroke.AuthoredLength),
                                anchorHalfWidth + movingHalfWidth))
                        {
                            qualityRejections.SharedContactLocus++;
                            continue;
                        }

                        Vector2 translation =
                            targetContact - candidatePoints[movingPointIndex];
                        float maximumRelocation =
                            Mathf.Max(
                                0.25f,
                                (anchor.Stroke.AuthoredLength +
                                 moving.Stroke.AuthoredLength) *
                                MaximumProjectedClusterRelocationLengthMultiplier);
                        if (translation.sqrMagnitude >
                            maximumRelocation * maximumRelocation)
                        {
                            continue;
                        }

                        TranslateProjectedPoints(candidatePoints, translation);
                        if (!HasValidProjectedEndpointContactSide(
                                anchorContactPoint,
                                candidatePoints,
                                movingPointIndex,
                                outward))
                        {
                            qualityRejections.WrongSideTerminal++;
                            continue;
                        }

                        Vector2 movingCentroid =
                            ResolveProjectedCentroid(candidatePoints);
                        Vector2 originalCentroid =
                            originalCentroids[movingMemberIndex];
                        Vector2 anchorCentroid =
                            ResolveProjectedCentroid(anchor.ProjectedPoints);
                        bool isPair =
                            pairLayout !=
                            GroundPaintedAccentCompanionPairLayout.None;
                        Vector2 stepAxis =
                            isPair
                                ? ResolveProjectedPairSharedNormal(
                                    anchor.ProjectedPoints,
                                    moving.ProjectedPoints)
                                : localNorth;
                        float desiredStep =
                            Vector2.Dot(
                                originalCentroid -
                                originalCentroids[anchorMemberIndex],
                                stepAxis);
                        float candidateStep =
                            Vector2.Dot(
                                movingCentroid - anchorCentroid,
                                stepAxis);
                        float desiredStepMagnitude =
                            Mathf.Abs(desiredStep);
                        if (desiredStepMagnitude >=
                            MinimumProjectedStructuredStep)
                        {
                            float minimumRetainedStep =
                                Mathf.Max(
                                    MinimumProjectedStructuredStep,
                                    desiredStepMagnitude *
                                    Mathf.Clamp01(
                                        minimumStepRetentionFraction));
                            if (Mathf.Sign(candidateStep) !=
                                    Mathf.Sign(desiredStep) ||
                                Mathf.Abs(candidateStep) <
                                    minimumRetainedStep)
                            {
                                qualityRejections.PreGeometryStepRetention++;
                                if (countPairStepRetentionRejections)
                                {
                                    qualityRejections.PairStepRetention++;
                                }
                                continue;
                            }
                        }

                        if (isPair &&
                            endpointCentreSeparation <= 0f &&
                            IsProjectedEndpointContact(
                                anchor,
                                anchorPointIndex) &&
                            IsNearCollinearProjectedPairContact(
                                anchor.ProjectedPoints,
                                anchorPointIndex,
                                candidatePoints,
                                movingPointIndex,
                                Mathf.Abs(candidateStep),
                                Mathf.Min(
                                    anchor.Stroke.AuthoredLength,
                                    moving.Stroke.AuthoredLength)))
                        {
                            qualityRejections.PairNearCollinear++;
                            continue;
                        }

                        float stepError = candidateStep - desiredStep;
                        float score =
                            translation.sqrMagnitude +
                            (movingCentroid - originalCentroid).sqrMagnitude * 0.15f +
                            stepError * stepError *
                            ProjectedStepErrorScoreWeight +
                            endpointCentreSeparation * 0.02f;
                        if (score >= bestScore)
                        {
                            qualityRejections
                                .PreGeometryNonCompetitiveScore++;
                            continue;
                        }

                        qualityRejections.CandidatesSentToGeometry++;
                        long nearParallelStartedAt =
                            System.Diagnostics.Stopwatch.GetTimestamp();
                        bool hasAnchorNearParallelBlend =
                            HasNearParallelProjectedBodyBlend(
                                anchor.ProjectedPoints,
                                anchor.HalfWidths,
                                candidatePoints,
                                candidateHalfWidths,
                                Mathf.Min(
                                    anchor.Stroke.AuthoredLength,
                                    moving.Stroke.AuthoredLength),
                                scratch,
                                nearParallelAudit);
                        if (timing != null)
                        {
                            timing.ClusterNearParallelValidationTicks +=
                                System.Diagnostics.Stopwatch.GetTimestamp() -
                                nearParallelStartedAt;
                        }
                        if (hasAnchorNearParallelBlend)
                        {
                            qualityRejections.NearParallelBodyBlend++;
                            continue;
                        }

                        long internalOverlapStartedAt =
                            System.Diagnostics.Stopwatch.GetTimestamp();
                        bool hasAnchorInternalOverlap =
                            HasUnintendedProjectedInternalOverlap(
                                anchor.ProjectedPoints,
                                anchor.HalfWidths,
                                candidatePoints,
                                candidateHalfWidths,
                                anchorPointIndex,
                                movingPointIndex,
                                out bool anchorSweptWidthOverlap,
                                internalOverlapAudit,
                                false,
                                scratch);
                        if (timing != null)
                        {
                            timing
                                .ClusterCandidateInternalOverlapValidationTicks +=
                                System.Diagnostics.Stopwatch.GetTimestamp() -
                                internalOverlapStartedAt;
                        }
                        if (hasAnchorInternalOverlap)
                        {
                            if (anchorSweptWidthOverlap)
                            {
                                qualityRejections.SweptWidthInternalOverlap++;
                            }
                            continue;
                        }

                        bool conflictsWithOtherMember = false;
                        for (int otherMemberIndex = 0;
                             otherMemberIndex < movingMemberIndex;
                             otherMemberIndex++)
                        {
                            if (otherMemberIndex == anchorMemberIndex)
                            {
                                continue;
                            }

                            ProjectedGlyphPrototype other =
                                members[otherMemberIndex];
                            nearParallelStartedAt =
                                System.Diagnostics.Stopwatch.GetTimestamp();
                            bool hasOtherNearParallelBlend =
                                HasNearParallelProjectedBodyBlend(
                                    other.ProjectedPoints,
                                    other.HalfWidths,
                                    candidatePoints,
                                    candidateHalfWidths,
                                    Mathf.Min(
                                        other.Stroke.AuthoredLength,
                                        moving.Stroke.AuthoredLength),
                                    scratch,
                                    nearParallelAudit);
                            if (timing != null)
                            {
                                timing.ClusterNearParallelValidationTicks +=
                                    System.Diagnostics.Stopwatch.GetTimestamp() -
                                    nearParallelStartedAt;
                            }
                            if (hasOtherNearParallelBlend)
                            {
                                qualityRejections.NearParallelBodyBlend++;
                                conflictsWithOtherMember = true;
                                break;
                            }

                            internalOverlapStartedAt =
                                System.Diagnostics.Stopwatch.GetTimestamp();
                            bool hasOtherInternalOverlap =
                                HasUnintendedProjectedInternalOverlap(
                                    other.ProjectedPoints,
                                    other.HalfWidths,
                                    candidatePoints,
                                    candidateHalfWidths,
                                    -1,
                                    -1,
                                    out bool otherSweptWidthOverlap,
                                    internalOverlapAudit,
                                    false,
                                    scratch);
                            if (timing != null)
                            {
                                timing
                                    .ClusterCandidateInternalOverlapValidationTicks +=
                                    System.Diagnostics.Stopwatch.GetTimestamp() -
                                    internalOverlapStartedAt;
                            }
                            if (hasOtherInternalOverlap)
                            {
                                if (otherSweptWidthOverlap)
                                {
                                    qualityRejections
                                        .SweptWidthInternalOverlap++;
                                }
                                conflictsWithOtherMember = true;
                                break;
                            }
                        }

                        if (conflictsWithOtherMember)
                        {
                            continue;
                        }

                        bestScore = score;
                        bestPoints = candidatePoints;
                        bestHalfWidths = candidateHalfWidths;
                        bestContact =
                            new ProjectedClusterContact(
                                anchorMemberIndex,
                                anchorPointIndex,
                                movingMemberIndex,
                                movingPointIndex);
                    }
                }
            }

            if (bestPoints == null)
            {
                return false;
            }

            moving.ProjectedPoints = bestPoints;
            moving.HalfWidths = bestHalfWidths;
            members[movingMemberIndex] = moving;
            contact = bestContact;
            return true;
        }

        private static void CreateTrimmedProjectedContactVariant(
            ProjectedGlyphPrototype prototype,
            int originalContactIndex,
            out Vector2[] points,
            out float[] halfWidths,
            out int contactIndex)
        {
            bool contactAtStart =
                originalContactIndex <= prototype.ProjectedPoints.Length / 2;
            int sourceStart = contactAtStart ? originalContactIndex : 0;
            int sourceEnd =
                contactAtStart
                    ? prototype.ProjectedPoints.Length - 1
                    : originalContactIndex;
            if (contactAtStart && sourceStart >= sourceEnd)
            {
                sourceStart = Mathf.Max(0, sourceEnd - 1);
            }
            else if (!contactAtStart && sourceEnd <= sourceStart)
            {
                sourceEnd = Mathf.Min(
                    prototype.ProjectedPoints.Length - 1,
                    sourceStart + 1);
            }
            int count = sourceEnd - sourceStart + 1;
            points = new Vector2[count];
            halfWidths = new float[count];
            Array.Copy(
                prototype.ProjectedPoints,
                sourceStart,
                points,
                0,
                count);
            Array.Copy(
                prototype.HalfWidths,
                sourceStart,
                halfWidths,
                0,
                count);
            contactIndex = contactAtStart ? 0 : count - 1;
        }

        private static int ResolveSafeProjectedContactIndex(
            ProjectedGlyphPrototype prototype,
            bool fromStart)
        {
            int pointCount = prototype.ProjectedPoints.Length;
            int maximumSearchIndex =
                Mathf.Clamp(
                    Mathf.CeilToInt(
                        (pointCount - 1) *
                        MaximumProjectedContactSearchFraction),
                    1,
                    Mathf.Max(1, pointCount - 2));
            float maximumHalfWidth = 0f;
            for (int index = 0; index < prototype.HalfWidths.Length; index++)
            {
                maximumHalfWidth =
                    Mathf.Max(maximumHalfWidth, prototype.HalfWidths[index]);
            }
            float minimumHalfWidth =
                Mathf.Max(
                    MinimumProjectedContactHalfWidth,
                    maximumHalfWidth * MinimumProjectedContactWidthFraction);

            for (int offset = 1; offset <= maximumSearchIndex; offset++)
            {
                int index = fromStart ? offset : pointCount - 1 - offset;
                if (prototype.HalfWidths[index] >= minimumHalfWidth)
                {
                    return index;
                }
            }

            return fromStart ? maximumSearchIndex : pointCount - 1 - maximumSearchIndex;
        }

        private static List<int> BuildProjectedAnchorContactIndices(
            ProjectedGlyphPrototype prototype,
            GroundPaintedAccentCompanionPairLayout pairLayout)
        {
            int pointCount = prototype.ProjectedPoints.Length;
            int start = ResolveSafeProjectedContactIndex(prototype, true);
            int quarter = Mathf.RoundToInt((pointCount - 1) * 0.25f);
            int middle = Mathf.RoundToInt((pointCount - 1) * 0.50f);
            int threeQuarter = Mathf.RoundToInt((pointCount - 1) * 0.75f);
            int end = ResolveSafeProjectedContactIndex(prototype, false);
            int[] candidates;
            switch (pairLayout)
            {
                case GroundPaintedAccentCompanionPairLayout.SteppedContinuation:
                    candidates = new[] { quarter, threeQuarter, middle };
                    break;
                case GroundPaintedAccentCompanionPairLayout.ShoulderContact:
                    candidates = new[] { middle, quarter, threeQuarter };
                    break;
                case GroundPaintedAccentCompanionPairLayout.OffsetEcho:
                    candidates = new[] { quarter, threeQuarter, middle };
                    break;
                case GroundPaintedAccentCompanionPairLayout.ShallowOffset:
                    candidates = new[] { start, quarter, threeQuarter, end };
                    break;
                case GroundPaintedAccentCompanionPairLayout.None:
                default:
                    candidates =
                        new[] { start, quarter, middle, threeQuarter, end };
                    break;
            }

            List<int> indices = new List<int>(candidates.Length);
            for (int candidateIndex = 0;
                 candidateIndex < candidates.Length;
                 candidateIndex++)
            {
                int index =
                    Mathf.Clamp(candidates[candidateIndex], 0, pointCount - 1);
                if (!indices.Contains(index))
                {
                    indices.Add(index);
                }
            }

            return indices;
        }

        private static bool TryResolveProjectedEndpointCentreSeparation(
            IReadOnlyList<Vector2> anchorPoints,
            int anchorContactIndex,
            Vector2 movingOutward,
            float anchorHalfWidth,
            float movingHalfWidth,
            float tightness,
            GroundPaintedAccentCompanionPairLayout pairLayout,
            out float endpointCentreSeparation)
        {
            endpointCentreSeparation = 0f;
            float totalHalfWidth =
                Mathf.Max(0f, anchorHalfWidth) +
                Mathf.Max(0f, movingHalfWidth);
            float looseExtra =
                Mathf.Max(
                    0.018f,
                    totalHalfWidth *
                    ProjectedLooseContactExtraWidthMultiplier);

            bool anchorEndpoint =
                anchorContactIndex <= 0 ||
                anchorContactIndex >= anchorPoints.Count - 1;
            float anchorBoundaryDistance;
            if (anchorEndpoint)
            {
                anchorBoundaryDistance = Mathf.Max(0f, anchorHalfWidth);
            }
            else
            {
                Vector2 anchorTangent =
                    ResolveProjectedContactTangent(
                        anchorPoints,
                        anchorContactIndex);
                Vector2 anchorNormal =
                    new Vector2(-anchorTangent.y, anchorTangent.x);
                float normalComponent =
                    Mathf.Abs(Vector2.Dot(movingOutward, anchorNormal));
                if (normalComponent <
                    MinimumProjectedInteriorContactNormalComponent)
                {
                    return false;
                }

                anchorBoundaryDistance =
                    Mathf.Max(0f, anchorHalfWidth) /
                    normalComponent;
            }

            float exactEdgeStop =
                anchorBoundaryDistance +
                Mathf.Max(0f, movingHalfWidth);
            if (pairLayout ==
                GroundPaintedAccentCompanionPairLayout.ShallowOffset)
            {
                float shallowVisibleGap =
                    Mathf.Max(
                        MinimumShallowPairContactGap,
                        totalHalfWidth * 0.10f);
                endpointCentreSeparation =
                    exactEdgeStop +
                    Mathf.Lerp(
                        looseExtra,
                        shallowVisibleGap,
                        tightness);
                return true;
            }

            endpointCentreSeparation =
                exactEdgeStop +
                Mathf.Lerp(looseExtra, 0f, tightness);
            return true;
        }

        private static bool IsProjectedEndpointContact(
            ProjectedGlyphPrototype prototype,
            int contactIndex)
        {
            int start = ResolveSafeProjectedContactIndex(prototype, true);
            int end = ResolveSafeProjectedContactIndex(prototype, false);
            return contactIndex == start || contactIndex == end;
        }

        private static bool IsNearCollinearProjectedPairContact(
            IReadOnlyList<Vector2> anchorPoints,
            int anchorContactIndex,
            IReadOnlyList<Vector2> movingPoints,
            int movingContactIndex,
            float pairLocalStep,
            float minimumAuthoredLength)
        {
            float maximumStep =
                Mathf.Max(0f, minimumAuthoredLength) *
                MaximumNearCollinearPairStepLengthFraction;
            if (pairLocalStep > maximumStep)
            {
                return false;
            }

            Vector2 anchorTangent =
                ResolveProjectedContactTangent(
                    anchorPoints,
                    anchorContactIndex);
            Vector2 movingTangent =
                ResolveProjectedContactTangent(
                    movingPoints,
                    movingContactIndex);
            float alignment =
                Mathf.Clamp01(
                    Mathf.Abs(Vector2.Dot(anchorTangent, movingTangent)));
            float junctionAngle = Mathf.Acos(alignment) * Mathf.Rad2Deg;
            return junctionAngle <=
                   MaximumNearCollinearPairJunctionDegrees;
        }

        private static Vector2 ResolveProjectedContactTangent(
            IReadOnlyList<Vector2> points,
            int contactIndex)
        {
            if (points == null || points.Count < 2)
            {
                return Vector2.right;
            }

            int previousIndex = Mathf.Max(0, contactIndex - 1);
            int nextIndex = Mathf.Min(points.Count - 1, contactIndex + 1);
            Vector2 tangent = points[nextIndex] - points[previousIndex];
            return tangent.sqrMagnitude > 0.000001f
                ? tangent.normalized
                : Vector2.right;
        }

        private static Vector2 ResolveProjectedPairSharedNormal(
            IReadOnlyList<Vector2> primaryPoints,
            IReadOnlyList<Vector2> secondaryPoints)
        {
            Vector2 primaryAxis = ResolveProjectedPathAxis(primaryPoints);
            Vector2 secondaryAxis = ResolveProjectedPathAxis(secondaryPoints);
            if (Vector2.Dot(primaryAxis, secondaryAxis) < 0f)
            {
                secondaryAxis = -secondaryAxis;
            }

            Vector2 sharedAxis = primaryAxis + secondaryAxis;
            if (sharedAxis.sqrMagnitude <= 0.000001f)
            {
                sharedAxis = primaryAxis;
            }
            sharedAxis.Normalize();
            return new Vector2(-sharedAxis.y, sharedAxis.x);
        }

        private static Vector2 ResolveProjectedPathAxis(
            IReadOnlyList<Vector2> points)
        {
            if (points == null || points.Count < 2)
            {
                return Vector2.right;
            }

            Vector2 axis = points[points.Count - 1] - points[0];
            return axis.sqrMagnitude > 0.000001f
                ? axis.normalized
                : Vector2.right;
        }

        private static Vector2 ResolveProjectedEndpointOutwardDirection(
            IReadOnlyList<Vector2> points,
            int contactIndex)
        {
            int pointCount = points.Count;
            int inwardIndex =
                contactIndex <= pointCount / 2
                    ? Mathf.Min(pointCount - 1, contactIndex + 1)
                    : Mathf.Max(0, contactIndex - 1);
            Vector2 outward = points[contactIndex] - points[inwardIndex];
            return outward.sqrMagnitude > 0.000001f
                ? outward.normalized
                : Vector2.right;
        }

        private static bool HasValidProjectedEndpointContactSide(
            Vector2 anchorContactPoint,
            IReadOnlyList<Vector2> movingPoints,
            int movingContactIndex,
            Vector2 movingOutward)
        {
            if (movingPoints == null ||
                movingPoints.Count < 2 ||
                movingContactIndex < 0 ||
                movingContactIndex >= movingPoints.Count ||
                movingOutward.sqrMagnitude <= 0.000001f)
            {
                return false;
            }

            int inwardIndex =
                movingContactIndex <= movingPoints.Count / 2
                    ? Mathf.Min(
                        movingPoints.Count - 1,
                        movingContactIndex + 1)
                    : Mathf.Max(0, movingContactIndex - 1);
            if (inwardIndex == movingContactIndex)
            {
                return false;
            }

            Vector2 outward = movingOutward.normalized;
            Vector2 endpoint = movingPoints[movingContactIndex];
            float anchorProjection =
                Vector2.Dot(anchorContactPoint - endpoint, outward);
            float bodyProjection =
                Vector2.Dot(movingPoints[inwardIndex] - endpoint, outward);
            return anchorProjection >= MinimumProjectedContactSideProjection &&
                   bodyProjection <= -MinimumProjectedContactSideProjection;
        }

        private static void TranslateProjectedPoints(
            Vector2[] points,
            Vector2 translation)
        {
            for (int pointIndex = 0;
                 pointIndex < points.Length;
                 pointIndex++)
            {
                points[pointIndex] += translation;
            }
        }

        private static Vector2 ResolveProjectedCentroid(
            IReadOnlyList<Vector2> points)
        {
            Vector2 total = Vector2.zero;
            for (int pointIndex = 0;
                 pointIndex < points.Count;
                 pointIndex++)
            {
                total += points[pointIndex];
            }

            return points.Count > 0 ? total / points.Count : Vector2.zero;
        }

        private static void ResolveAcceptedProjectedPairStepStatistics(
            IReadOnlyList<GroundPaintedAccentProjectedGlyph> glyphs,
            out float stepMinimum,
            out float stepMean,
            out float stepMaximum,
            out float stepFractionMinimum,
            out float stepFractionMean,
            out float stepFractionMaximum)
        {
            stepMinimum = 0f;
            stepMean = 0f;
            stepMaximum = 0f;
            stepFractionMinimum = 0f;
            stepFractionMean = 0f;
            stepFractionMaximum = 0f;
            if (glyphs == null || glyphs.Count < 2)
            {
                return;
            }

            Dictionary<int, GroundPaintedAccentProjectedGlyph> primaries =
                new Dictionary<int, GroundPaintedAccentProjectedGlyph>();
            Dictionary<int, GroundPaintedAccentProjectedGlyph> secondaries =
                new Dictionary<int, GroundPaintedAccentProjectedGlyph>();
            for (int glyphIndex = 0;
                 glyphIndex < glyphs.Count;
                 glyphIndex++)
            {
                GroundPaintedAccentProjectedGlyph glyph = glyphs[glyphIndex];
                if (glyph.CompanionClusterIndex < 0 ||
                    glyph.IntendedCompanionClusterSize != 2)
                {
                    continue;
                }

                if (glyph.CompanionMemberRole ==
                    GroundPaintedAccentCompanionMemberRole.Primary)
                {
                    primaries[glyph.CompanionClusterIndex] = glyph;
                }
                else if (glyph.CompanionMemberRole ==
                    GroundPaintedAccentCompanionMemberRole.Secondary)
                {
                    secondaries[glyph.CompanionClusterIndex] = glyph;
                }
            }

            float minimum = float.PositiveInfinity;
            float maximum = 0f;
            double total = 0d;
            float fractionMinimum = float.PositiveInfinity;
            float fractionMaximum = 0f;
            double fractionTotal = 0d;
            int pairCount = 0;
            List<int> pairKeys = new List<int>(primaries.Keys);
            pairKeys.Sort();
            for (int pairIndex = 0;
                 pairIndex < pairKeys.Count;
                 pairIndex++)
            {
                int pairKey = pairKeys[pairIndex];
                if (!secondaries.TryGetValue(
                        pairKey,
                        out GroundPaintedAccentProjectedGlyph secondary))
                {
                    continue;
                }

                GroundPaintedAccentProjectedGlyph primary =
                    primaries[pairKey];
                Vector2 pairNormal =
                    ResolveProjectedPairSharedNormal(
                        primary.LocalProjectedPoints,
                        secondary.LocalProjectedPoints);
                float step =
                    Mathf.Abs(
                        Vector2.Dot(
                            ResolveProjectedCentroid(
                                secondary.LocalProjectedPoints) -
                            ResolveProjectedCentroid(
                                primary.LocalProjectedPoints),
                            pairNormal));
                float minimumLength =
                    Mathf.Max(
                        0.0001f,
                        Mathf.Min(
                            primary.AuthoredLength,
                            secondary.AuthoredLength));
                float stepFraction = step / minimumLength;
                minimum = Mathf.Min(minimum, step);
                maximum = Mathf.Max(maximum, step);
                total += step;
                fractionMinimum =
                    Mathf.Min(fractionMinimum, stepFraction);
                fractionMaximum =
                    Mathf.Max(fractionMaximum, stepFraction);
                fractionTotal += stepFraction;
                pairCount++;
            }

            if (pairCount <= 0)
            {
                return;
            }

            stepMinimum = minimum;
            stepMean = (float)(total / pairCount);
            stepMaximum = maximum;
            stepFractionMinimum = fractionMinimum;
            stepFractionMean = (float)(fractionTotal / pairCount);
            stepFractionMaximum = fractionMaximum;
        }

        private static bool IsProjectedTripletAttachmentSlotOccupied(
            IReadOnlyList<ProjectedGlyphPrototype> members,
            IReadOnlyList<ProjectedClusterContact> existingContacts,
            int candidateMemberIndex,
            int candidatePointIndex)
        {
            if (members == null ||
                existingContacts == null ||
                existingContacts.Count == 0)
            {
                return false;
            }

            for (int contactIndex = 0;
                 contactIndex < existingContacts.Count;
                 contactIndex++)
            {
                ProjectedClusterContact contact =
                    existingContacts[contactIndex];
                if (IsProjectedAttachmentSlotTooClose(
                        members,
                        candidateMemberIndex,
                        candidatePointIndex,
                        contact.AnchorMemberIndex,
                        contact.AnchorPointIndex) ||
                    IsProjectedAttachmentSlotTooClose(
                        members,
                        candidateMemberIndex,
                        candidatePointIndex,
                        contact.MovingMemberIndex,
                        contact.MovingPointIndex))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsProjectedAttachmentSlotTooClose(
            IReadOnlyList<ProjectedGlyphPrototype> members,
            int candidateMemberIndex,
            int candidatePointIndex,
            int occupiedMemberIndex,
            int occupiedPointIndex)
        {
            if (candidateMemberIndex != occupiedMemberIndex ||
                candidateMemberIndex < 0 ||
                candidateMemberIndex >= members.Count)
            {
                return false;
            }

            int pointCount =
                members[candidateMemberIndex].ProjectedPoints.Length;
            int minimumIndexSeparation =
                Mathf.Max(
                    2,
                    Mathf.CeilToInt(
                        Mathf.Max(1, pointCount - 1) *
                        MinimumTripletAttachmentIndexSeparationFraction));
            return Mathf.Abs(candidatePointIndex - occupiedPointIndex) <
                   minimumIndexSeparation;
        }

        private static bool IsProjectedTripletContactLocusTooClose(
            IReadOnlyList<ProjectedGlyphPrototype> members,
            IReadOnlyList<ProjectedClusterContact> existingContacts,
            Vector2 candidateAnchorPoint,
            float minimumAuthoredLength,
            float candidateCombinedHalfWidth)
        {
            if (members == null ||
                existingContacts == null ||
                existingContacts.Count == 0)
            {
                return false;
            }

            float minimumSeparation =
                Mathf.Max(
                    Mathf.Max(0f, minimumAuthoredLength) *
                    MinimumTripletContactSeparationLengthFraction,
                    Mathf.Max(0f, candidateCombinedHalfWidth) *
                    MinimumTripletContactSeparationWidthMultiplier);
            float minimumSeparationSquared =
                minimumSeparation * minimumSeparation;
            for (int contactIndex = 0;
                 contactIndex < existingContacts.Count;
                 contactIndex++)
            {
                ProjectedClusterContact contact =
                    existingContacts[contactIndex];
                if (IsProjectedPointNearContactSide(
                        members,
                        candidateAnchorPoint,
                        contact.AnchorMemberIndex,
                        contact.AnchorPointIndex,
                        minimumSeparationSquared) ||
                    IsProjectedPointNearContactSide(
                        members,
                        candidateAnchorPoint,
                        contact.MovingMemberIndex,
                        contact.MovingPointIndex,
                        minimumSeparationSquared))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsProjectedPointNearContactSide(
            IReadOnlyList<ProjectedGlyphPrototype> members,
            Vector2 candidatePoint,
            int memberIndex,
            int pointIndex,
            float minimumSeparationSquared)
        {
            if (memberIndex < 0 || memberIndex >= members.Count)
            {
                return false;
            }

            IReadOnlyList<Vector2> points =
                members[memberIndex].ProjectedPoints;
            if (pointIndex < 0 || pointIndex >= points.Count)
            {
                return false;
            }

            return (candidatePoint - points[pointIndex]).sqrMagnitude <
                   minimumSeparationSquared;
        }

        private static bool HasNearParallelProjectedBodyBlend(
            IReadOnlyList<Vector2> leftPoints,
            IReadOnlyList<float> leftHalfWidths,
            IReadOnlyList<Vector2> rightPoints,
            IReadOnlyList<float> rightHalfWidths,
            float minimumAuthoredLength,
            GroundPaintedAccentProjectedGlyphScratch scratch,
            GroundPaintedAccentNearParallelAuditAccumulator nearParallelAudit)
        {
            long rightSegmentMetadataPreparations = 0;
            long rightSegmentsPrepared = 0;
            long segmentPairsConsidered = 0;
            long segmentPairsRejectedByAxisGap = 0;
            long segmentPairsRejectedByAlignment = 0;
            long segmentPairsSentToExactDistance = 0;
            long exactDistancePasses = 0;
            long exactIntervalOverlapEvaluations = 0;
            long blendsDetected = 0;

            try
            {
                if (leftPoints == null ||
                    rightPoints == null ||
                    leftHalfWidths == null ||
                    rightHalfWidths == null ||
                    leftPoints.Count < 2 ||
                    rightPoints.Count < 2 ||
                    leftHalfWidths.Count != leftPoints.Count ||
                    rightHalfWidths.Count != rightPoints.Count ||
                    !ProjectedBoundsOverlap(
                        leftPoints,
                        leftHalfWidths,
                        rightPoints,
                        rightHalfWidths,
                        1f))
                {
                    return false;
                }

                GroundPaintedAccentProjectedGlyphScratch resolvedScratch =
                    scratch ?? new GroundPaintedAccentProjectedGlyphScratch();
                int rightSegmentCount = rightPoints.Count - 1;
                resolvedScratch.EnsureNearParallelSegmentCapacity(
                    rightSegmentCount);
                rightSegmentMetadataPreparations = 1;
                rightSegmentsPrepared = rightSegmentCount;
                GroundPaintedAccentNearParallelSegmentMetadata[]
                    rightSegments =
                        resolvedScratch.NearParallelRightSegments;
                for (int rightSegmentIndex = 0;
                     rightSegmentIndex < rightSegmentCount;
                     rightSegmentIndex++)
                {
                    rightSegments[rightSegmentIndex] =
                        new GroundPaintedAccentNearParallelSegmentMetadata(
                            rightPoints[rightSegmentIndex],
                            rightPoints[rightSegmentIndex + 1],
                            rightHalfWidths[rightSegmentIndex],
                            rightHalfWidths[rightSegmentIndex + 1]);
                }

                float minimumBlendLength =
                    Mathf.Max(
                        MinimumNearParallelBlendWorldLength,
                        Mathf.Max(0f, minimumAuthoredLength) *
                        MinimumNearParallelBlendLengthFraction);
                float minimumAlignment =
                    Mathf.Cos(
                        MaximumNearParallelBodyAngleDegrees * Mathf.Deg2Rad);
                float accumulatedBlendLength = 0f;

                for (int leftSegmentIndex = 0;
                     leftSegmentIndex < leftPoints.Count - 1;
                     leftSegmentIndex++)
                {
                    Vector2 leftStart = leftPoints[leftSegmentIndex];
                    Vector2 leftEnd = leftPoints[leftSegmentIndex + 1];
                    Vector2 leftDelta = leftEnd - leftStart;
                    float leftLength = leftDelta.magnitude;
                    if (leftLength <= 0.000001f)
                    {
                        continue;
                    }

                    Vector2 leftDirection = leftDelta / leftLength;
                    float leftWidth =
                        (leftHalfWidths[leftSegmentIndex] +
                         leftHalfWidths[leftSegmentIndex + 1]) * 0.5f;
                    float leftMinimumX = Mathf.Min(leftStart.x, leftEnd.x);
                    float leftMaximumX = Mathf.Max(leftStart.x, leftEnd.x);
                    float leftMinimumY = Mathf.Min(leftStart.y, leftEnd.y);
                    float leftMaximumY = Mathf.Max(leftStart.y, leftEnd.y);
                    float bestSegmentBlend = 0f;
                    for (int rightSegmentIndex = 0;
                         rightSegmentIndex < rightSegmentCount;
                         rightSegmentIndex++)
                    {
                        GroundPaintedAccentNearParallelSegmentMetadata right =
                            rightSegments[rightSegmentIndex];
                        if (!right.IsValid)
                        {
                            continue;
                        }

                        segmentPairsConsidered++;
                        float requiredClearance =
                            Mathf.Max(
                                MinimumProjectedExternalClearance,
                                (leftWidth + right.AverageHalfWidth) *
                                NearParallelBodyClearanceFraction);
                        float xGap =
                            Mathf.Max(
                                0f,
                                Mathf.Max(
                                    leftMinimumX - right.MaximumX,
                                    right.MinimumX - leftMaximumX));
                        if (xGap > requiredClearance)
                        {
                            segmentPairsRejectedByAxisGap++;
                            continue;
                        }

                        float yGap =
                            Mathf.Max(
                                0f,
                                Mathf.Max(
                                    leftMinimumY - right.MaximumY,
                                    right.MinimumY - leftMaximumY));
                        if (yGap > requiredClearance)
                        {
                            segmentPairsRejectedByAxisGap++;
                            continue;
                        }

                        float signedAlignment =
                            Vector2.Dot(leftDirection, right.Direction);
                        float alignment = Mathf.Abs(signedAlignment);
                        if (alignment < minimumAlignment)
                        {
                            segmentPairsRejectedByAlignment++;
                            continue;
                        }

                        segmentPairsSentToExactDistance++;
                        if (SqrDistanceBetweenProjectedSegments(
                                leftStart,
                                leftEnd,
                                right.Start,
                                right.End) >=
                            requiredClearance * requiredClearance)
                        {
                            continue;
                        }

                        exactDistancePasses++;
                        Vector2 alignedRightDirection =
                            signedAlignment >= 0f
                                ? right.Direction
                                : -right.Direction;
                        Vector2 sharedDirection =
                            leftDirection + alignedRightDirection;
                        if (sharedDirection.sqrMagnitude <= 0.000001f)
                        {
                            sharedDirection = leftDirection;
                        }
                        else
                        {
                            sharedDirection.Normalize();
                        }

                        exactIntervalOverlapEvaluations++;
                        float overlapLength =
                            ResolveProjectedSegmentIntervalOverlap(
                                leftStart,
                                leftEnd,
                                right.Start,
                                right.End,
                                sharedDirection);
                        bestSegmentBlend =
                            Mathf.Max(bestSegmentBlend, overlapLength);
                    }

                    accumulatedBlendLength +=
                        Mathf.Min(leftLength, bestSegmentBlend);
                    if (accumulatedBlendLength >= minimumBlendLength)
                    {
                        blendsDetected++;
                        return true;
                    }
                }

                return false;
            }
            finally
            {
                if (nearParallelAudit != null)
                {
                    nearParallelAudit.Accumulate(
                        rightSegmentMetadataPreparations,
                        rightSegmentsPrepared,
                        segmentPairsConsidered,
                        segmentPairsRejectedByAxisGap,
                        segmentPairsRejectedByAlignment,
                        segmentPairsSentToExactDistance,
                        exactDistancePasses,
                        exactIntervalOverlapEvaluations,
                        blendsDetected);
                }
            }
        }

        private static float ResolveProjectedSegmentIntervalOverlap(
            Vector2 leftStart,
            Vector2 leftEnd,
            Vector2 rightStart,
            Vector2 rightEnd,
            Vector2 axis)
        {
            float leftA = Vector2.Dot(leftStart, axis);
            float leftB = Vector2.Dot(leftEnd, axis);
            float rightA = Vector2.Dot(rightStart, axis);
            float rightB = Vector2.Dot(rightEnd, axis);
            float leftMinimum = Mathf.Min(leftA, leftB);
            float leftMaximum = Mathf.Max(leftA, leftB);
            float rightMinimum = Mathf.Min(rightA, rightB);
            float rightMaximum = Mathf.Max(rightA, rightB);
            return Mathf.Max(
                0f,
                Mathf.Min(leftMaximum, rightMaximum) -
                Mathf.Max(leftMinimum, rightMinimum));
        }

        private static float SqrDistanceBetweenProjectedSegments(
            Vector2 firstStart,
            Vector2 firstEnd,
            Vector2 secondStart,
            Vector2 secondEnd)
        {
            if (SegmentsIntersect(
                    firstStart,
                    firstEnd,
                    secondStart,
                    secondEnd))
            {
                return 0f;
            }

            return Mathf.Min(
                Mathf.Min(
                    SqrDistanceFromProjectedPointToSegment(
                        firstStart,
                        secondStart,
                        secondEnd),
                    SqrDistanceFromProjectedPointToSegment(
                        firstEnd,
                        secondStart,
                        secondEnd)),
                Mathf.Min(
                    SqrDistanceFromProjectedPointToSegment(
                        secondStart,
                        firstStart,
                        firstEnd),
                    SqrDistanceFromProjectedPointToSegment(
                        secondEnd,
                        firstStart,
                        firstEnd)));
        }

        private static float SqrDistanceBetweenProjectedSegments(
            Vector2 firstStart,
            Vector2 firstEnd,
            Vector2 secondStart,
            Vector2 secondEnd,
            out float firstT,
            out float secondT)
        {
            Vector2 firstDirection = firstEnd - firstStart;
            Vector2 secondDirection = secondEnd - secondStart;
            Vector2 startDelta = firstStart - secondStart;
            float firstLengthSquared = firstDirection.sqrMagnitude;
            float secondLengthSquared = secondDirection.sqrMagnitude;
            const float epsilon = 0.000001f;

            if (firstLengthSquared <= epsilon &&
                secondLengthSquared <= epsilon)
            {
                firstT = 0f;
                secondT = 0f;
                return startDelta.sqrMagnitude;
            }

            if (firstLengthSquared <= epsilon)
            {
                firstT = 0f;
                secondT =
                    Mathf.Clamp01(
                        Vector2.Dot(secondDirection, startDelta) /
                        secondLengthSquared);
            }
            else
            {
                float firstProjection =
                    Vector2.Dot(firstDirection, startDelta);
                if (secondLengthSquared <= epsilon)
                {
                    secondT = 0f;
                    firstT =
                        Mathf.Clamp01(
                            -firstProjection / firstLengthSquared);
                }
                else
                {
                    float directionDot =
                        Vector2.Dot(firstDirection, secondDirection);
                    float secondProjection =
                        Vector2.Dot(secondDirection, startDelta);
                    float denominator =
                        firstLengthSquared * secondLengthSquared -
                        directionDot * directionDot;
                    firstT =
                        denominator > epsilon
                            ? Mathf.Clamp01(
                                (directionDot * secondProjection -
                                 firstProjection * secondLengthSquared) /
                                denominator)
                            : 0f;
                    secondT =
                        (directionDot * firstT + secondProjection) /
                        secondLengthSquared;

                    if (secondT < 0f)
                    {
                        secondT = 0f;
                        firstT =
                            Mathf.Clamp01(
                                -firstProjection / firstLengthSquared);
                    }
                    else if (secondT > 1f)
                    {
                        secondT = 1f;
                        firstT =
                            Mathf.Clamp01(
                                (directionDot - firstProjection) /
                                firstLengthSquared);
                    }
                }
            }

            Vector2 firstClosest =
                firstStart + firstDirection * firstT;
            Vector2 secondClosest =
                secondStart + secondDirection * secondT;
            return (firstClosest - secondClosest).sqrMagnitude;
        }

        private static float SqrDistanceFromProjectedPointToSegment(
            Vector2 point,
            Vector2 segmentStart,
            Vector2 segmentEnd)
        {
            Vector2 segment = segmentEnd - segmentStart;
            float segmentLengthSquared = segment.sqrMagnitude;
            if (segmentLengthSquared <= 0.000001f)
            {
                return (point - segmentStart).sqrMagnitude;
            }

            float t =
                Mathf.Clamp01(
                    Vector2.Dot(point - segmentStart, segment) /
                    segmentLengthSquared);
            Vector2 closest = segmentStart + segment * t;
            return (point - closest).sqrMagnitude;
        }

        private static bool HasCrowdedProjectedTripletJunctions(
            IReadOnlyList<ProjectedGlyphPrototype> members,
            IReadOnlyList<ProjectedClusterContact> contacts)
        {
            if (members == null ||
                members.Count != 3 ||
                contacts == null ||
                contacts.Count != 2 ||
                !ProjectedContactsShareMember(contacts[0], contacts[1]) ||
                !TryResolveProjectedContactLocus(
                    members,
                    contacts[0],
                    out Vector2 firstLocus,
                    out float firstCombinedHalfWidth) ||
                !TryResolveProjectedContactLocus(
                    members,
                    contacts[1],
                    out Vector2 secondLocus,
                    out float secondCombinedHalfWidth))
            {
                return false;
            }

            float minimumAuthoredLength = float.PositiveInfinity;
            for (int memberIndex = 0;
                 memberIndex < members.Count;
                 memberIndex++)
            {
                minimumAuthoredLength =
                    Mathf.Min(
                        minimumAuthoredLength,
                        members[memberIndex].Stroke.AuthoredLength);
            }

            if (float.IsInfinity(minimumAuthoredLength) ||
                minimumAuthoredLength <= 0f)
            {
                return false;
            }

            float minimumSeparation =
                Mathf.Max(
                    minimumAuthoredLength *
                    MinimumCrowdedTripletJunctionLengthFraction,
                    Mathf.Min(
                        firstCombinedHalfWidth,
                        secondCombinedHalfWidth) *
                    MinimumCrowdedTripletJunctionWidthMultiplier);
            return (firstLocus - secondLocus).sqrMagnitude <
                   minimumSeparation * minimumSeparation;
        }

        private static bool ProjectedContactsShareMember(
            ProjectedClusterContact first,
            ProjectedClusterContact second)
        {
            return first.AnchorMemberIndex == second.AnchorMemberIndex ||
                   first.AnchorMemberIndex == second.MovingMemberIndex ||
                   first.MovingMemberIndex == second.AnchorMemberIndex ||
                   first.MovingMemberIndex == second.MovingMemberIndex;
        }

        private static bool TryResolveProjectedContactLocus(
            IReadOnlyList<ProjectedGlyphPrototype> members,
            ProjectedClusterContact contact,
            out Vector2 locus,
            out float combinedHalfWidth)
        {
            locus = Vector2.zero;
            combinedHalfWidth = 0f;
            if (members == null ||
                contact.AnchorMemberIndex < 0 ||
                contact.AnchorMemberIndex >= members.Count ||
                contact.MovingMemberIndex < 0 ||
                contact.MovingMemberIndex >= members.Count)
            {
                return false;
            }

            ProjectedGlyphPrototype anchor =
                members[contact.AnchorMemberIndex];
            ProjectedGlyphPrototype moving =
                members[contact.MovingMemberIndex];
            if (contact.AnchorPointIndex < 0 ||
                contact.AnchorPointIndex >= anchor.ProjectedPoints.Length ||
                contact.MovingPointIndex < 0 ||
                contact.MovingPointIndex >= moving.ProjectedPoints.Length)
            {
                return false;
            }

            Vector2 anchorPoint =
                anchor.ProjectedPoints[contact.AnchorPointIndex];
            Vector2 movingPoint =
                moving.ProjectedPoints[contact.MovingPointIndex];
            locus = (anchorPoint + movingPoint) * 0.5f;
            combinedHalfWidth =
                Mathf.Max(0f, anchor.HalfWidths[contact.AnchorPointIndex]) +
                Mathf.Max(0f, moving.HalfWidths[contact.MovingPointIndex]);
            return true;
        }

        private static bool HasProjectedTripletFreeEndPseudoContact(
            IReadOnlyList<ProjectedGlyphPrototype> members,
            IReadOnlyList<ProjectedClusterContact> contacts)
        {
            if (members == null || members.Count != 3 || contacts == null)
            {
                return false;
            }

            List<ProjectedTripletFreeTerminal> freeTerminals =
                new List<ProjectedTripletFreeTerminal>(6);
            for (int memberIndex = 0;
                 memberIndex < members.Count;
                 memberIndex++)
            {
                ProjectedGlyphPrototype member = members[memberIndex];
                if (member.ProjectedPoints == null ||
                    member.ProjectedPoints.Length < 2 ||
                    member.HalfWidths == null ||
                    member.HalfWidths.Length != member.ProjectedPoints.Length)
                {
                    continue;
                }

                int lastIndex = member.ProjectedPoints.Length - 1;
                AddProjectedTripletFreeTerminal(
                    member,
                    memberIndex,
                    0,
                    contacts,
                    freeTerminals);
                AddProjectedTripletFreeTerminal(
                    member,
                    memberIndex,
                    lastIndex,
                    contacts,
                    freeTerminals);
            }

            for (int leftIndex = 0;
                 leftIndex < freeTerminals.Count;
                 leftIndex++)
            {
                ProjectedTripletFreeTerminal left =
                    freeTerminals[leftIndex];
                for (int rightIndex = leftIndex + 1;
                     rightIndex < freeTerminals.Count;
                     rightIndex++)
                {
                    ProjectedTripletFreeTerminal right =
                        freeTerminals[rightIndex];
                    if (left.MemberIndex == right.MemberIndex)
                    {
                        continue;
                    }

                    Vector2 separation = right.Position - left.Position;
                    float separationLength = separation.magnitude;
                    float minimumAuthoredLength =
                        Mathf.Min(left.AuthoredLength, right.AuthoredLength);
                    float minimumSeparation =
                        Mathf.Max(
                            minimumAuthoredLength *
                            MinimumTripletPseudoContactLengthFraction,
                            (left.HalfWidth + right.HalfWidth) *
                            MinimumTripletPseudoContactWidthMultiplier);
                    if (separationLength <= 0.000001f ||
                        separationLength >= minimumSeparation)
                    {
                        continue;
                    }

                    Vector2 direction = separation / separationLength;
                    if (Vector2.Dot(left.Outward, direction) >=
                            MinimumTripletPseudoContactApproachDot &&
                        Vector2.Dot(right.Outward, -direction) >=
                            MinimumTripletPseudoContactApproachDot)
                    {
                        return true;
                    }
                }
            }

            for (int terminalIndex = 0;
                 terminalIndex < freeTerminals.Count;
                 terminalIndex++)
            {
                ProjectedTripletFreeTerminal terminal =
                    freeTerminals[terminalIndex];
                for (int memberIndex = 0;
                     memberIndex < members.Count;
                     memberIndex++)
                {
                    if (memberIndex == terminal.MemberIndex ||
                        !TryResolveClosestProjectedBodyPoint(
                            terminal.Position,
                            members[memberIndex],
                            out Vector2 closestPoint,
                            out float closestHalfWidth))
                    {
                        continue;
                    }

                    Vector2 approach = closestPoint - terminal.Position;
                    float approachLength = approach.magnitude;
                    float minimumAuthoredLength =
                        Mathf.Min(
                            terminal.AuthoredLength,
                            members[memberIndex].Stroke.AuthoredLength);
                    float minimumSeparation =
                        Mathf.Max(
                            minimumAuthoredLength *
                            MinimumTripletPseudoContactBodyLengthFraction,
                            (terminal.HalfWidth + closestHalfWidth) *
                            MinimumTripletPseudoContactBodyWidthMultiplier);
                    if (approachLength <= 0.000001f ||
                        approachLength >= minimumSeparation)
                    {
                        continue;
                    }

                    if (Vector2.Dot(
                            terminal.Outward,
                            approach / approachLength) >=
                        MinimumTripletPseudoContactApproachDot)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void AddProjectedTripletFreeTerminal(
            ProjectedGlyphPrototype member,
            int memberIndex,
            int pointIndex,
            IReadOnlyList<ProjectedClusterContact> contacts,
            List<ProjectedTripletFreeTerminal> terminals)
        {
            if (IsProjectedTerminalUsedByContact(
                    contacts,
                    memberIndex,
                    pointIndex,
                    member.ProjectedPoints.Length))
            {
                return;
            }

            Vector2 outward =
                ResolveProjectedTerminalOutwardDirection(
                    member.ProjectedPoints,
                    pointIndex);
            if (outward.sqrMagnitude <= 0.000001f)
            {
                return;
            }

            terminals.Add(
                new ProjectedTripletFreeTerminal(
                    memberIndex,
                    member.ProjectedPoints[pointIndex],
                    outward.normalized,
                    member.HalfWidths[pointIndex],
                    member.Stroke.AuthoredLength));
        }

        private static bool IsProjectedTerminalUsedByContact(
            IReadOnlyList<ProjectedClusterContact> contacts,
            int memberIndex,
            int terminalPointIndex,
            int pointCount)
        {
            int contactNeighborhood =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        Mathf.Max(1, pointCount - 1) *
                        ProjectedContactNeighborhoodFraction));
            for (int contactIndex = 0;
                 contactIndex < contacts.Count;
                 contactIndex++)
            {
                ProjectedClusterContact contact = contacts[contactIndex];
                if (contact.AnchorMemberIndex == memberIndex &&
                    Mathf.Abs(
                        contact.AnchorPointIndex - terminalPointIndex) <=
                    contactNeighborhood)
                {
                    return true;
                }

                if (contact.MovingMemberIndex == memberIndex &&
                    Mathf.Abs(
                        contact.MovingPointIndex - terminalPointIndex) <=
                    contactNeighborhood)
                {
                    return true;
                }
            }

            return false;
        }

        private static Vector2 ResolveProjectedTerminalOutwardDirection(
            IReadOnlyList<Vector2> points,
            int terminalPointIndex)
        {
            if (points == null || points.Count < 2)
            {
                return Vector2.zero;
            }

            if (terminalPointIndex <= 0)
            {
                return points[0] - points[1];
            }

            int lastIndex = points.Count - 1;
            return points[lastIndex] - points[lastIndex - 1];
        }

        private static bool TryResolveClosestProjectedBodyPoint(
            Vector2 point,
            ProjectedGlyphPrototype member,
            out Vector2 closestPoint,
            out float closestHalfWidth)
        {
            closestPoint = Vector2.zero;
            closestHalfWidth = 0f;
            if (member == null ||
                member.ProjectedPoints == null ||
                member.ProjectedPoints.Length < 2 ||
                member.HalfWidths == null ||
                member.HalfWidths.Length != member.ProjectedPoints.Length)
            {
                return false;
            }

            float bestDistanceSquared = float.PositiveInfinity;
            for (int segmentIndex = 0;
                 segmentIndex < member.ProjectedPoints.Length - 1;
                 segmentIndex++)
            {
                Vector2 segmentStart = member.ProjectedPoints[segmentIndex];
                Vector2 segmentEnd = member.ProjectedPoints[segmentIndex + 1];
                Vector2 segment = segmentEnd - segmentStart;
                float segmentLengthSquared = segment.sqrMagnitude;
                float t =
                    segmentLengthSquared > 0.000001f
                        ? Mathf.Clamp01(
                            Vector2.Dot(point - segmentStart, segment) /
                            segmentLengthSquared)
                        : 0f;
                Vector2 candidatePoint = segmentStart + segment * t;
                float distanceSquared =
                    (point - candidatePoint).sqrMagnitude;
                if (distanceSquared >= bestDistanceSquared)
                {
                    continue;
                }

                bestDistanceSquared = distanceSquared;
                closestPoint = candidatePoint;
                closestHalfWidth =
                    Mathf.Lerp(
                        member.HalfWidths[segmentIndex],
                        member.HalfWidths[segmentIndex + 1],
                        t);
            }

            return !float.IsInfinity(bestDistanceSquared);
        }

        private static bool HasSeverelyCompressedProjectedTriplet(
            IReadOnlyList<ProjectedGlyphPrototype> members)
        {
            if (members == null || members.Count != 3)
            {
                return false;
            }

            float minimumAuthoredLength = float.PositiveInfinity;
            Vector2[] centroids = new Vector2[3];
            for (int memberIndex = 0; memberIndex < members.Count; memberIndex++)
            {
                minimumAuthoredLength =
                    Mathf.Min(
                        minimumAuthoredLength,
                        members[memberIndex].Stroke.AuthoredLength);
                centroids[memberIndex] =
                    ResolveProjectedCentroid(
                        members[memberIndex].ProjectedPoints);
            }

            if (float.IsInfinity(minimumAuthoredLength) ||
                minimumAuthoredLength <= 0f)
            {
                return false;
            }

            float maximumCentroidSpan =
                Mathf.Max(
                    Vector2.Distance(centroids[0], centroids[1]),
                    Mathf.Max(
                        Vector2.Distance(centroids[0], centroids[2]),
                        Vector2.Distance(centroids[1], centroids[2])));
            return maximumCentroidSpan <
                   minimumAuthoredLength *
                   MaximumSeverelyCompressedTripletCentroidSpanFraction;
        }

        private static bool HasValidProjectedClusterSilhouette(
            IReadOnlyList<ProjectedGlyphPrototype> members,
            IReadOnlyList<ProjectedClusterContact> contacts)
        {
            return HasValidProjectedClusterSilhouette(
                members,
                contacts,
                out _,
                null);
        }

        private static bool HasValidProjectedClusterSilhouette(
            IReadOnlyList<ProjectedGlyphPrototype> members,
            IReadOnlyList<ProjectedClusterContact> contacts,
            out bool sweptWidthOverlap)
        {
            return HasValidProjectedClusterSilhouette(
                members,
                contacts,
                out sweptWidthOverlap,
                null);
        }

        private static bool HasValidProjectedClusterSilhouette(
            IReadOnlyList<ProjectedGlyphPrototype> members,
            IReadOnlyList<ProjectedClusterContact> contacts,
            out bool sweptWidthOverlap,
            GroundPaintedAccentInternalOverlapAuditAccumulator
                internalOverlapAudit)
        {
            return HasValidProjectedClusterSilhouette(
                members,
                contacts,
                out sweptWidthOverlap,
                internalOverlapAudit,
                null);
        }

        private static bool HasValidProjectedClusterSilhouette(
            IReadOnlyList<ProjectedGlyphPrototype> members,
            IReadOnlyList<ProjectedClusterContact> contacts,
            out bool sweptWidthOverlap,
            GroundPaintedAccentInternalOverlapAuditAccumulator
                internalOverlapAudit,
            GroundPaintedAccentProjectedGlyphScratch scratch)
        {
            sweptWidthOverlap = false;
            for (int leftIndex = 0;
                 leftIndex < members.Count;
                 leftIndex++)
            {
                for (int rightIndex = leftIndex + 1;
                     rightIndex < members.Count;
                     rightIndex++)
                {
                    int leftContactIndex = -1;
                    int rightContactIndex = -1;
                    TryResolveProjectedContactIndices(
                        contacts,
                        leftIndex,
                        rightIndex,
                        out leftContactIndex,
                        out rightContactIndex);
                    if (HasUnintendedProjectedInternalOverlap(
                            members[leftIndex].ProjectedPoints,
                            members[leftIndex].HalfWidths,
                            members[rightIndex].ProjectedPoints,
                            members[rightIndex].HalfWidths,
                            leftContactIndex,
                            rightContactIndex,
                            out bool pairSweptWidthOverlap,
                            internalOverlapAudit,
                            true,
                            scratch))
                    {
                        sweptWidthOverlap |= pairSweptWidthOverlap;
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool TryResolveProjectedContactIndices(
            IReadOnlyList<ProjectedClusterContact> contacts,
            int leftMemberIndex,
            int rightMemberIndex,
            out int leftContactIndex,
            out int rightContactIndex)
        {
            leftContactIndex = -1;
            rightContactIndex = -1;
            for (int contactIndex = 0;
                 contactIndex < contacts.Count;
                 contactIndex++)
            {
                ProjectedClusterContact contact = contacts[contactIndex];
                if (contact.AnchorMemberIndex == leftMemberIndex &&
                    contact.MovingMemberIndex == rightMemberIndex)
                {
                    leftContactIndex = contact.AnchorPointIndex;
                    rightContactIndex = contact.MovingPointIndex;
                    return true;
                }

                if (contact.AnchorMemberIndex == rightMemberIndex &&
                    contact.MovingMemberIndex == leftMemberIndex)
                {
                    leftContactIndex = contact.MovingPointIndex;
                    rightContactIndex = contact.AnchorPointIndex;
                    return true;
                }
            }

            return false;
        }

        private static bool HasUnintendedProjectedInternalOverlap(
            IReadOnlyList<Vector2> leftPoints,
            IReadOnlyList<float> leftHalfWidths,
            IReadOnlyList<Vector2> rightPoints,
            IReadOnlyList<float> rightHalfWidths,
            int leftContactIndex,
            int rightContactIndex,
            out bool sweptWidthOverlap)
        {
            return HasUnintendedProjectedInternalOverlap(
                leftPoints,
                leftHalfWidths,
                rightPoints,
                rightHalfWidths,
                leftContactIndex,
                rightContactIndex,
                out sweptWidthOverlap,
                null,
                false,
                null);
        }

        private static bool HasUnintendedProjectedInternalOverlap(
            IReadOnlyList<Vector2> leftPoints,
            IReadOnlyList<float> leftHalfWidths,
            IReadOnlyList<Vector2> rightPoints,
            IReadOnlyList<float> rightHalfWidths,
            int leftContactIndex,
            int rightContactIndex,
            out bool sweptWidthOverlap,
            GroundPaintedAccentInternalOverlapAuditAccumulator
                internalOverlapAudit,
            bool finalSilhouetteCall,
            GroundPaintedAccentProjectedGlyphScratch scratch)
        {
            sweptWidthOverlap = false;
            long segmentPairsConsidered = 0L;
            long segmentPairsRejectedByBroadPhase = 0L;
            long segmentPairsSentToExactNarrowPhase = 0L;
            long exactSegmentIntersectionsFound = 0L;
            long exactSweptClearanceRejections = 0L;
            try
            {
                if (leftPoints == null ||
                    rightPoints == null ||
                    leftHalfWidths == null ||
                    rightHalfWidths == null ||
                    leftPoints.Count < 2 ||
                    rightPoints.Count < 2 ||
                    leftHalfWidths.Count != leftPoints.Count ||
                    rightHalfWidths.Count != rightPoints.Count ||
                    !ProjectedBoundsOverlap(
                        leftPoints,
                        leftHalfWidths,
                        rightPoints,
                        rightHalfWidths,
                        1f))
                {
                    return false;
                }

                int leftSegmentCount = leftPoints.Count - 1;
                int rightSegmentCount = rightPoints.Count - 1;
                GroundPaintedAccentProjectedGlyphScratch resolvedScratch =
                    scratch ?? new GroundPaintedAccentProjectedGlyphScratch();
                resolvedScratch.EnsureInternalOverlapSegmentCapacity(
                    leftSegmentCount,
                    rightSegmentCount);
                GroundPaintedAccentInternalOverlapSegmentMetadata[]
                    leftSegments = resolvedScratch.InternalOverlapLeftSegments;
                GroundPaintedAccentInternalOverlapSegmentMetadata[]
                    rightSegments = resolvedScratch.InternalOverlapRightSegments;
                for (int leftSegmentIndex = 0;
                     leftSegmentIndex < leftSegmentCount;
                     leftSegmentIndex++)
                {
                    leftSegments[leftSegmentIndex] =
                        new GroundPaintedAccentInternalOverlapSegmentMetadata(
                            leftPoints[leftSegmentIndex],
                            leftPoints[leftSegmentIndex + 1],
                            leftHalfWidths[leftSegmentIndex],
                            leftHalfWidths[leftSegmentIndex + 1]);
                }
                for (int rightSegmentIndex = 0;
                     rightSegmentIndex < rightSegmentCount;
                     rightSegmentIndex++)
                {
                    rightSegments[rightSegmentIndex] =
                        new GroundPaintedAccentInternalOverlapSegmentMetadata(
                            rightPoints[rightSegmentIndex],
                            rightPoints[rightSegmentIndex + 1],
                            rightHalfWidths[rightSegmentIndex],
                            rightHalfWidths[rightSegmentIndex + 1]);
                }

                for (int leftSegmentIndex = 0;
                     leftSegmentIndex < leftSegmentCount;
                     leftSegmentIndex++)
                {
                    GroundPaintedAccentInternalOverlapSegmentMetadata left =
                        leftSegments[leftSegmentIndex];
                    for (int rightSegmentIndex = 0;
                         rightSegmentIndex < rightSegmentCount;
                         rightSegmentIndex++)
                    {
                        segmentPairsConsidered++;
                        GroundPaintedAccentInternalOverlapSegmentMetadata right =
                            rightSegments[rightSegmentIndex];
                        bool exactContactSegmentPair =
                            IsExactProjectedContactSegmentPair(
                                leftSegmentIndex,
                                leftPoints.Count,
                                leftContactIndex,
                                rightSegmentIndex,
                                rightPoints.Count,
                                rightContactIndex);
                        float clearanceFraction =
                            exactContactSegmentPair
                                ? ProjectedContactSweptClearanceFraction
                                : ProjectedInternalSweptClearanceFraction;
                        if (!CouldProjectedSegmentsReachSweptClearance(
                                left,
                                right,
                                clearanceFraction))
                        {
                            segmentPairsRejectedByBroadPhase++;
                            continue;
                        }

                        segmentPairsSentToExactNarrowPhase++;
                        if (SegmentsIntersect(
                                left.Start,
                                left.End,
                                right.Start,
                                right.End))
                        {
                            exactSegmentIntersectionsFound++;
                            sweptWidthOverlap = true;
                            return true;
                        }

                        float distanceSquared =
                            SqrDistanceBetweenProjectedSegments(
                                left.Start,
                                left.End,
                                right.Start,
                                right.End,
                                out float leftT,
                                out float rightT);
                        float leftHalfWidth =
                            Mathf.Lerp(
                                left.StartHalfWidth,
                                left.EndHalfWidth,
                                leftT);
                        float rightHalfWidth =
                            Mathf.Lerp(
                                right.StartHalfWidth,
                                right.EndHalfWidth,
                                rightT);
                        float requiredClearance =
                            Mathf.Max(
                                MinimumProjectedExternalClearance,
                                (leftHalfWidth + rightHalfWidth) *
                                clearanceFraction);
                        float toleratedClearance =
                            Mathf.Max(
                                0f,
                                requiredClearance -
                                ProjectedSweptClearanceTolerance);
                        if (distanceSquared <
                            toleratedClearance * toleratedClearance)
                        {
                            exactSweptClearanceRejections++;
                            sweptWidthOverlap = true;
                            return true;
                        }
                    }
                }

                return false;
            }
            finally
            {
                if (internalOverlapAudit != null)
                {
                    internalOverlapAudit.Accumulate(
                        finalSilhouetteCall,
                        segmentPairsConsidered,
                        segmentPairsRejectedByBroadPhase,
                        segmentPairsSentToExactNarrowPhase,
                        exactSegmentIntersectionsFound,
                        exactSweptClearanceRejections);
                }
            }
        }

        private static bool CouldProjectedSegmentsReachSweptClearance(
            GroundPaintedAccentInternalOverlapSegmentMetadata left,
            GroundPaintedAccentInternalOverlapSegmentMetadata right,
            float clearanceFraction)
        {
            float maximumRequiredClearance =
                Mathf.Max(
                    MinimumProjectedExternalClearance,
                    (left.MaximumHalfWidth + right.MaximumHalfWidth) *
                    Mathf.Max(0f, clearanceFraction));
            float xGap =
                Mathf.Max(
                    0f,
                    Mathf.Max(
                        left.MinimumX - right.MaximumX,
                        right.MinimumX - left.MaximumX));
            if (xGap > maximumRequiredClearance)
            {
                return false;
            }

            float yGap =
                Mathf.Max(
                    0f,
                    Mathf.Max(
                        left.MinimumY - right.MaximumY,
                        right.MinimumY - left.MaximumY));
            return yGap <= maximumRequiredClearance;
        }

        private static bool IsExactProjectedContactSegmentPair(
            int leftSegmentIndex,
            int leftPointCount,
            int leftContactIndex,
            int rightSegmentIndex,
            int rightPointCount,
            int rightContactIndex)
        {
            if (leftContactIndex < 0 || rightContactIndex < 0)
            {
                return false;
            }

            bool leftIsMovingTerminal =
                IsProjectedTerminalSegmentForContact(
                    leftSegmentIndex,
                    leftPointCount,
                    leftContactIndex);
            bool rightIsMovingTerminal =
                IsProjectedTerminalSegmentForContact(
                    rightSegmentIndex,
                    rightPointCount,
                    rightContactIndex);
            bool leftContainsContact =
                ProjectedSegmentContainsPointIndex(
                    leftSegmentIndex,
                    leftContactIndex);
            bool rightContainsContact =
                ProjectedSegmentContainsPointIndex(
                    rightSegmentIndex,
                    rightContactIndex);
            return (leftIsMovingTerminal && rightContainsContact) ||
                   (rightIsMovingTerminal && leftContainsContact);
        }

        private static bool IsProjectedTerminalSegmentForContact(
            int segmentIndex,
            int pointCount,
            int contactIndex)
        {
            if (pointCount < 2)
            {
                return false;
            }

            return (contactIndex == 0 && segmentIndex == 0) ||
                   (contactIndex == pointCount - 1 &&
                    segmentIndex == pointCount - 2);
        }

        private static bool ProjectedSegmentContainsPointIndex(
            int segmentIndex,
            int pointIndex)
        {
            return pointIndex == segmentIndex ||
                   pointIndex == segmentIndex + 1;
        }

        private static bool HasProjectedClusterExternalConflict(
            IReadOnlyList<ProjectedGlyphPrototype> members,
            IReadOnlyList<GroundPaintedAccentProjectedGlyph> acceptedGlyphs,
            ProjectedGlyphSpatialIndex acceptedGlyphSpatialIndex,
            GroundPaintedAccentClusterBuildAuditAccumulator clusterAudit)
        {
            if (acceptedGlyphSpatialIndex == null)
            {
                return HasProjectedClusterExternalConflict(
                    members,
                    acceptedGlyphs,
                    clusterAudit);
            }

            for (int memberIndex = 0;
                 memberIndex < members.Count;
                 memberIndex++)
            {
                ProjectedGlyphPrototype member = members[memberIndex];
                IReadOnlyList<int> candidateIndices =
                    acceptedGlyphSpatialIndex.Query(
                        member.ProjectedPoints,
                        member.HalfWidths,
                        clusterAudit);
                for (int candidateIndex = 0;
                     candidateIndex < candidateIndices.Count;
                     candidateIndex++)
                {
                    int glyphIndex = candidateIndices[candidateIndex];
                    if (glyphIndex < 0 || glyphIndex >= acceptedGlyphs.Count)
                    {
                        continue;
                    }

                    GroundPaintedAccentProjectedGlyph glyph =
                        acceptedGlyphs[glyphIndex];
                    if (HasInstrumentedProjectedExternalOverlap(
                            member.ProjectedPoints,
                            member.HalfWidths,
                            glyph.LocalProjectedPoints,
                            glyph.HalfWidths,
                            clusterAudit))
                    {
                        if (clusterAudit != null)
                        {
                            clusterAudit.ExternalConflictRejections++;
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool HasProjectedClusterExternalConflict(
            IReadOnlyList<ProjectedGlyphPrototype> members,
            IReadOnlyList<GroundPaintedAccentProjectedGlyph> acceptedGlyphs,
            GroundPaintedAccentClusterBuildAuditAccumulator clusterAudit = null)
        {
            for (int memberIndex = 0;
                 memberIndex < members.Count;
                 memberIndex++)
            {
                ProjectedGlyphPrototype member = members[memberIndex];
                for (int glyphIndex = 0;
                     glyphIndex < acceptedGlyphs.Count;
                     glyphIndex++)
                {
                    GroundPaintedAccentProjectedGlyph glyph =
                        acceptedGlyphs[glyphIndex];
                    if (HasInstrumentedProjectedExternalOverlap(
                            member.ProjectedPoints,
                            member.HalfWidths,
                            glyph.LocalProjectedPoints,
                            glyph.HalfWidths,
                            clusterAudit))
                    {
                        if (clusterAudit != null)
                        {
                            clusterAudit.ExternalConflictRejections++;
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool HasInstrumentedProjectedReconciliationOverlap(
            IReadOnlyList<Vector2> leftPoints,
            IReadOnlyList<float> leftHalfWidths,
            IReadOnlyList<Vector2> rightPoints,
            IReadOnlyList<float> rightHalfWidths,
            GroundPaintedAccentClusterBuildAuditAccumulator clusterAudit)
        {
            if (clusterAudit != null)
            {
                clusterAudit.ExternalGlyphCandidatesExamined++;
                clusterAudit.ExternalBoundsTests++;
                clusterAudit.ReconciliationNewIndependentRelationshipsTested++;
                clusterAudit.ReconciliationBoundsTests++;
            }

            if (!ProjectedBoundsOverlap(
                    leftPoints,
                    leftHalfWidths,
                    rightPoints,
                    rightHalfWidths,
                    ProjectedExternalConflictWidthFraction))
            {
                return false;
            }

            if (clusterAudit != null)
            {
                clusterAudit.ExternalBoundsOverlapPasses++;
                clusterAudit.ExternalDetailedOverlapTests++;
                clusterAudit.ReconciliationBoundsOverlapPasses++;
                clusterAudit.ReconciliationDetailedOverlapTests++;
            }

            return HasUnintendedProjectedOverlap(
                leftPoints,
                leftHalfWidths,
                rightPoints,
                rightHalfWidths,
                -1,
                -1,
                ProjectedExternalConflictWidthFraction,
                true);
        }

        private static bool HasInstrumentedProjectedExternalOverlap(
            IReadOnlyList<Vector2> leftPoints,
            IReadOnlyList<float> leftHalfWidths,
            IReadOnlyList<Vector2> rightPoints,
            IReadOnlyList<float> rightHalfWidths,
            GroundPaintedAccentClusterBuildAuditAccumulator clusterAudit)
        {
            if (clusterAudit != null)
            {
                clusterAudit.ExternalGlyphCandidatesExamined++;
                clusterAudit.ExternalBoundsTests++;
            }

            if (!ProjectedBoundsOverlap(
                    leftPoints,
                    leftHalfWidths,
                    rightPoints,
                    rightHalfWidths,
                    ProjectedExternalConflictWidthFraction))
            {
                return false;
            }

            if (clusterAudit != null)
            {
                clusterAudit.ExternalBoundsOverlapPasses++;
                clusterAudit.ExternalDetailedOverlapTests++;
            }

            return HasUnintendedProjectedOverlap(
                leftPoints,
                leftHalfWidths,
                rightPoints,
                rightHalfWidths,
                -1,
                -1,
                ProjectedExternalConflictWidthFraction,
                true);
        }

        private static bool HasUnintendedProjectedOverlap(
            IReadOnlyList<Vector2> leftPoints,
            IReadOnlyList<float> leftHalfWidths,
            IReadOnlyList<Vector2> rightPoints,
            IReadOnlyList<float> rightHalfWidths,
            int leftContactIndex,
            int rightContactIndex,
            float widthFraction,
            bool boundsAlreadyOverlap = false)
        {
            if (!boundsAlreadyOverlap &&
                !ProjectedBoundsOverlap(
                    leftPoints,
                    leftHalfWidths,
                    rightPoints,
                    rightHalfWidths,
                    widthFraction))
            {
                return false;
            }

            int leftNeighborhood =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        leftPoints.Count * ProjectedContactNeighborhoodFraction));
            int rightNeighborhood =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        rightPoints.Count * ProjectedContactNeighborhoodFraction));

            for (int leftSegmentIndex = 0;
                 leftSegmentIndex < leftPoints.Count - 1;
                 leftSegmentIndex++)
            {
                for (int rightSegmentIndex = 0;
                     rightSegmentIndex < rightPoints.Count - 1;
                     rightSegmentIndex++)
                {
                    if (SegmentsIntersect(
                            leftPoints[leftSegmentIndex],
                            leftPoints[leftSegmentIndex + 1],
                            rightPoints[rightSegmentIndex],
                            rightPoints[rightSegmentIndex + 1]))
                    {
                        return true;
                    }
                }
            }

            for (int leftIndex = 0;
                 leftIndex < leftPoints.Count;
                 leftIndex++)
            {
                bool leftNearContact =
                    leftContactIndex >= 0 &&
                    Mathf.Abs(leftIndex - leftContactIndex) <= leftNeighborhood;
                for (int rightIndex = 0;
                     rightIndex < rightPoints.Count;
                     rightIndex++)
                {
                    bool rightNearContact =
                        rightContactIndex >= 0 &&
                        Mathf.Abs(rightIndex - rightContactIndex) <=
                        rightNeighborhood;
                    if (leftNearContact && rightNearContact)
                    {
                        continue;
                    }

                    float requiredClearance =
                        Mathf.Max(
                            MinimumProjectedExternalClearance,
                            (leftHalfWidths[leftIndex] +
                             rightHalfWidths[rightIndex]) *
                            widthFraction);
                    if ((leftPoints[leftIndex] - rightPoints[rightIndex])
                            .sqrMagnitude <
                        requiredClearance * requiredClearance)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool ProjectedBoundsOverlap(
            IReadOnlyList<Vector2> leftPoints,
            IReadOnlyList<float> leftHalfWidths,
            IReadOnlyList<Vector2> rightPoints,
            IReadOnlyList<float> rightHalfWidths,
            float widthFraction)
        {
            ResolveProjectedBounds(
                leftPoints,
                leftHalfWidths,
                widthFraction,
                out Vector2 leftMin,
                out Vector2 leftMax);
            ResolveProjectedBounds(
                rightPoints,
                rightHalfWidths,
                widthFraction,
                out Vector2 rightMin,
                out Vector2 rightMax);
            return leftMin.x <= rightMax.x &&
                   leftMax.x >= rightMin.x &&
                   leftMin.y <= rightMax.y &&
                   leftMax.y >= rightMin.y;
        }

        private static void ResolveProjectedBounds(
            IReadOnlyList<Vector2> points,
            IReadOnlyList<float> halfWidths,
            float widthFraction,
            out Vector2 minimum,
            out Vector2 maximum)
        {
            minimum = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            maximum = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            for (int pointIndex = 0;
                 pointIndex < points.Count;
                 pointIndex++)
            {
                float padding =
                    Mathf.Max(
                        MinimumProjectedExternalClearance,
                        halfWidths[pointIndex] * widthFraction);
                Vector2 point = points[pointIndex];
                minimum.x = Mathf.Min(minimum.x, point.x - padding);
                minimum.y = Mathf.Min(minimum.y, point.y - padding);
                maximum.x = Mathf.Max(maximum.x, point.x + padding);
                maximum.y = Mathf.Max(maximum.y, point.y + padding);
            }
        }

        private static GroundPaintedAccentGlyphFamilyStatistics
            BuildFamilyStatistics(
                int index,
                int[] attempted,
                int[] accepted,
                int[] rejected,
                float[] lengthMin,
                double[] lengthTotal,
                float[] lengthMax,
                float[] peakMin,
                double[] peakTotal,
                float[] peakMax,
                float[,] shapeMin,
                double[,] shapeTotal,
                float[,] shapeMax,
                int[] nearStraightCount)
        {
            int acceptedCount = accepted[index];
            float divisor = Mathf.Max(1, acceptedCount);
            return new GroundPaintedAccentGlyphFamilyStatistics(
                attempted[index],
                acceptedCount,
                rejected[index],
                acceptedCount > 0 ? lengthMin[index] : 0f,
                (float)(lengthTotal[index] / divisor),
                acceptedCount > 0 ? lengthMax[index] : 0f,
                acceptedCount > 0 ? peakMin[index] : 0f,
                (float)(peakTotal[index] / divisor),
                acceptedCount > 0 ? peakMax[index] : 0f,
                BuildFamilyShapeStatistics(
                    index,
                    acceptedCount,
                    shapeMin,
                    shapeTotal,
                    shapeMax,
                    nearStraightCount));
        }

        private static void AccumulateFamilyShapeMetrics(
            int familyIndex,
            GroundPaintedAccentGlyphFamilyShapeMetrics metrics,
            float[,] minimum,
            double[,] total,
            float[,] maximum,
            int[] nearStraightCount)
        {
            float[] values =
            {
                metrics.CrestPosition,
                metrics.LegSpanRatio,
                metrics.LegSlopeRatio,
                metrics.UpperRunFraction,
                metrics.UpperEndpointDropFraction,
                metrics.DescendingDropFraction,
                metrics.PlateauFraction,
                metrics.VerticalRangeFraction,
                metrics.EndpointDifferenceFraction,
                metrics.EndpointDifferenceWorld
            };

            for (int metricIndex = 0;
                 metricIndex < ShapeMetricCount;
                 metricIndex++)
            {
                float value = values[metricIndex];
                minimum[familyIndex, metricIndex] =
                    Mathf.Min(minimum[familyIndex, metricIndex], value);
                total[familyIndex, metricIndex] += value;
                maximum[familyIndex, metricIndex] =
                    Mathf.Max(maximum[familyIndex, metricIndex], value);
            }

            if (metrics.IsNearStraightVariant)
            {
                nearStraightCount[familyIndex]++;
            }
        }

        private static GroundPaintedAccentGlyphFamilyShapeStatistics
            BuildFamilyShapeStatistics(
                int familyIndex,
                int acceptedCount,
                float[,] minimum,
                double[,] total,
                float[,] maximum,
                int[] nearStraightCount)
        {
            return new GroundPaintedAccentGlyphFamilyShapeStatistics(
                BuildMetricRange(
                    familyIndex,
                    CrestPositionMetricIndex,
                    acceptedCount,
                    minimum,
                    total,
                    maximum),
                BuildMetricRange(
                    familyIndex,
                    LegSpanRatioMetricIndex,
                    acceptedCount,
                    minimum,
                    total,
                    maximum),
                BuildMetricRange(
                    familyIndex,
                    LegSlopeRatioMetricIndex,
                    acceptedCount,
                    minimum,
                    total,
                    maximum),
                BuildMetricRange(
                    familyIndex,
                    UpperRunFractionMetricIndex,
                    acceptedCount,
                    minimum,
                    total,
                    maximum),
                BuildMetricRange(
                    familyIndex,
                    UpperEndpointDropMetricIndex,
                    acceptedCount,
                    minimum,
                    total,
                    maximum),
                BuildMetricRange(
                    familyIndex,
                    DescendingDropMetricIndex,
                    acceptedCount,
                    minimum,
                    total,
                    maximum),
                BuildMetricRange(
                    familyIndex,
                    PlateauFractionMetricIndex,
                    acceptedCount,
                    minimum,
                    total,
                    maximum),
                BuildMetricRange(
                    familyIndex,
                    VerticalRangeMetricIndex,
                    acceptedCount,
                    minimum,
                    total,
                    maximum),
                BuildMetricRange(
                    familyIndex,
                    EndpointDifferenceMetricIndex,
                    acceptedCount,
                    minimum,
                    total,
                    maximum),
                BuildMetricRange(
                    familyIndex,
                    EndpointDifferenceWorldMetricIndex,
                    acceptedCount,
                    minimum,
                    total,
                    maximum),
                nearStraightCount[familyIndex]);
        }

        private static GroundPaintedAccentMetricRange BuildMetricRange(
            int familyIndex,
            int metricIndex,
            int acceptedCount,
            float[,] minimum,
            double[,] total,
            float[,] maximum)
        {
            float divisor = Mathf.Max(1, acceptedCount);
            return new GroundPaintedAccentMetricRange(
                acceptedCount > 0 ? minimum[familyIndex, metricIndex] : 0f,
                (float)(total[familyIndex, metricIndex] / divisor),
                acceptedCount > 0 ? maximum[familyIndex, metricIndex] : 0f);
        }

        private static bool HasInvalidSegmentOrSelfIntersection(
            Vector2[] points,
            int pointCount)
        {
            if (points == null || pointCount < 2 || points.Length < pointCount)
            {
                return true;
            }

            for (int index = 0; index < pointCount; index++)
            {
                Vector2 point = points[index];
                if (float.IsNaN(point.x) ||
                    float.IsNaN(point.y) ||
                    float.IsInfinity(point.x) ||
                    float.IsInfinity(point.y))
                {
                    return true;
                }

                if (index > 0 &&
                    (point - points[index - 1]).sqrMagnitude <= 0.00000001f)
                {
                    return true;
                }
            }

            for (int first = 0; first < pointCount - 1; first++)
            {
                Vector2 firstA = points[first];
                Vector2 firstB = points[first + 1];
                float firstMinX = Mathf.Min(firstA.x, firstB.x);
                float firstMaxX = Mathf.Max(firstA.x, firstB.x);
                float firstMinY = Mathf.Min(firstA.y, firstB.y);
                float firstMaxY = Mathf.Max(firstA.y, firstB.y);

                for (int second = first + 2; second < pointCount - 1; second++)
                {
                    Vector2 secondA = points[second];
                    Vector2 secondB = points[second + 1];
                    if (firstMaxX < Mathf.Min(secondA.x, secondB.x) ||
                        firstMinX > Mathf.Max(secondA.x, secondB.x) ||
                        firstMaxY < Mathf.Min(secondA.y, secondB.y) ||
                        firstMinY > Mathf.Max(secondA.y, secondB.y))
                    {
                        continue;
                    }

                    if (SegmentsIntersect(
                            firstA,
                            firstB,
                            secondA,
                            secondB))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool SegmentsIntersect(
            Vector2 a,
            Vector2 b,
            Vector2 c,
            Vector2 d)
        {
            float abC = Cross(b - a, c - a);
            float abD = Cross(b - a, d - a);
            float cdA = Cross(d - c, a - c);
            float cdB = Cross(d - c, b - c);
            const float epsilon = 0.000001f;
            return abC * abD < -epsilon && cdA * cdB < -epsilon;
        }

        private static float Cross(Vector2 left, Vector2 right)
        {
            return left.x * right.y - left.y * right.x;
        }

        private static bool TryCreateProjectedGlyph(
            GroundPaintedAccentSurfaceStroke stroke,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            IReadOnlyList<GroundPaintedAccentRiverExclusionSnapshot> rivers,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            Vector2 localNorth,
            GroundPaintedAccentProjectedGlyphScratch scratch,
            GroundPaintedAccentProjectedGlyphTimingAccumulator timing,
            out GroundPaintedAccentProjectedGlyph glyph,
            out GroundPaintedAccentProjectedGlyphRejectionReason rejectionReason)
        {
            glyph = default;
            if (!TryBuildProjectedGlyphPrototype(
                    stroke,
                    feature,
                    localNorth,
                    timing,
                    out ProjectedGlyphPrototype prototype,
                    out rejectionReason))
            {
                return false;
            }

            return TryFinalizeProjectedGlyph(
                prototype,
                baseSurface,
                rivers,
                modifiers,
                scratch,
                timing,
                out glyph,
                out rejectionReason);
        }

        private static bool TryBuildProjectedGlyphPrototype(
            GroundPaintedAccentSurfaceStroke stroke,
            GroundSurfaceFeatureRecipe feature,
            Vector2 localNorth,
            GroundPaintedAccentProjectedGlyphTimingAccumulator timing,
            out ProjectedGlyphPrototype prototype,
            out GroundPaintedAccentProjectedGlyphRejectionReason rejectionReason)
        {
            prototype = null;
            rejectionReason =
                GroundPaintedAccentProjectedGlyphRejectionReason.None;

            if (!stroke.IsValid)
            {
                rejectionReason =
                    GroundPaintedAccentProjectedGlyphRejectionReason.Sampling;
                return false;
            }

            long stageStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
            bool profileBuilt =
                GroundPaintedAccentLongitudinalProfileGenerator.TryBuild(
                    stroke,
                    feature,
                    out GroundPaintedAccentLongitudinalProfile profile);
            timing.ProfileBuildTicks +=
                System.Diagnostics.Stopwatch.GetTimestamp() - stageStartedAt;
            if (!profileBuilt)
            {
                rejectionReason =
                    GroundPaintedAccentProjectedGlyphRejectionReason.Sampling;
                return false;
            }

            stageStartedAt = System.Diagnostics.Stopwatch.GetTimestamp();
            bool familyValid =
                GroundPaintedAccentLongitudinalProfileGenerator
                    .ValidateFamilyProfile(stroke.Family, profile);
            timing.FamilyValidationTicks +=
                System.Diagnostics.Stopwatch.GetTimestamp() - stageStartedAt;
            if (!familyValid)
            {
                rejectionReason =
                    GroundPaintedAccentProjectedGlyphRejectionReason.FamilyShape;
                return false;
            }

            stageStartedAt = System.Diagnostics.Stopwatch.GetTimestamp();
            GroundPaintedAccentLongitudinalProfileSample[] samples =
                profile.Samples;
            int pointCount = samples.Length;
            Vector2[] projectedPoints = new Vector2[pointCount];
            float[] halfWidths = new float[pointCount];
            Vector2 localCross = new Vector2(-localNorth.y, localNorth.x);
            float maximumNorthError = 0f;
            float maximumCrossDrift = 0f;

            for (int pointIndex = 0;
                 pointIndex < samples.Length;
                 pointIndex++)
            {
                GroundPaintedAccentLongitudinalProfileSample sample =
                    samples[pointIndex];
                Vector2 baseline =
                    new Vector2(
                        sample.LocalBaselinePoint.x,
                        sample.LocalBaselinePoint.z);
                Vector2 projected =
                    baseline + localNorth * sample.CombinedHeight;
                Vector2 displacement = projected - baseline;
                float northDistance = Vector2.Dot(displacement, localNorth);
                float crossDistance = Vector2.Dot(displacement, localCross);

                maximumNorthError =
                    Mathf.Max(
                        maximumNorthError,
                        Mathf.Abs(northDistance - sample.CombinedHeight));
                maximumCrossDrift =
                    Mathf.Max(maximumCrossDrift, Mathf.Abs(crossDistance));
                projectedPoints[pointIndex] = projected;
                halfWidths[pointIndex] = sample.HalfWidth;
            }
            timing.PointConstructionTicks +=
                System.Diagnostics.Stopwatch.GetTimestamp() - stageStartedAt;

            stageStartedAt = System.Diagnostics.Stopwatch.GetTimestamp();
            if (HasInvalidSegmentOrSelfIntersection(projectedPoints, pointCount))
            {
                rejectionReason =
                    GroundPaintedAccentProjectedGlyphRejectionReason.FamilyShape;
                timing.TopologyValidationTicks +=
                    System.Diagnostics.Stopwatch.GetTimestamp() - stageStartedAt;
                return false;
            }

            float maximumProjectedTurnDegrees =
                ResolveMaximumProjectedTurnDegrees(projectedPoints, pointCount);
            if (maximumProjectedTurnDegrees >
                ResolveMaximumAllowedProjectedTurnDegrees(stroke.Family))
            {
                rejectionReason =
                    GroundPaintedAccentProjectedGlyphRejectionReason.SharpTurn;
                timing.TopologyValidationTicks +=
                    System.Diagnostics.Stopwatch.GetTimestamp() - stageStartedAt;
                return false;
            }
            timing.TopologyValidationTicks +=
                System.Diagnostics.Stopwatch.GetTimestamp() - stageStartedAt;

            float crestT =
                samples[Mathf.Clamp(
                    profile.PeakSampleIndex,
                    0,
                    samples.Length - 1)].T;
            prototype =
                new ProjectedGlyphPrototype(
                    stroke,
                    projectedPoints,
                    halfWidths,
                    profile,
                    crestT,
                    maximumProjectedTurnDegrees,
                    maximumNorthError,
                    maximumCrossDrift);
            return true;
        }

        private static bool TryFinalizeProjectedGlyph(
            ProjectedGlyphPrototype prototype,
            GroundHeightFieldSnapshot baseSurface,
            IReadOnlyList<GroundPaintedAccentRiverExclusionSnapshot> rivers,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            GroundPaintedAccentProjectedGlyphScratch scratch,
            GroundPaintedAccentProjectedGlyphTimingAccumulator timing,
            out GroundPaintedAccentProjectedGlyph glyph,
            out GroundPaintedAccentProjectedGlyphRejectionReason rejectionReason)
        {
            glyph = default;
            rejectionReason =
                GroundPaintedAccentProjectedGlyphRejectionReason.None;
            if (prototype == null ||
                prototype.ProjectedPoints == null ||
                prototype.ProjectedPoints.Length < 2 ||
                prototype.HalfWidths == null ||
                prototype.HalfWidths.Length != prototype.ProjectedPoints.Length)
            {
                rejectionReason =
                    GroundPaintedAccentProjectedGlyphRejectionReason.Sampling;
                return false;
            }

            long stageStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
            int pointCount = prototype.ProjectedPoints.Length;
            bool surfaceValid = TryValidateAndSampleProjectedGlyph(
                prototype.ProjectedPoints,
                prototype.HalfWidths,
                pointCount,
                baseSurface,
                rivers,
                modifiers,
                scratch,
                timing,
                out rejectionReason);
            timing.SurfaceValidationTicks +=
                System.Diagnostics.Stopwatch.GetTimestamp() - stageStartedAt;
            if (!surfaceValid)
            {
                return false;
            }

            Vector3[] localSurfacePoints = new Vector3[pointCount];
            Array.Copy(scratch.SurfacePoints, localSurfacePoints, pointCount);

            GroundPaintedAccentSurfaceStroke stroke = prototype.Stroke;
            GroundPaintedAccentLongitudinalProfile profile = prototype.Profile;
            glyph =
                new GroundPaintedAccentProjectedGlyph(
                    localSurfacePoints,
                    prototype.ProjectedPoints,
                    prototype.HalfWidths,
                    stroke.Family,
                    stroke.AuthoredLength,
                    stroke.Seed,
                    profile.ShapeMetrics,
                    prototype.CrestT,
                    profile.CrestPeakHeight,
                    profile.CrownPeakHeight,
                    profile.CombinedPeakHeight,
                    prototype.MaximumProjectedTurnDegrees,
                    prototype.MaximumNorthDisplacementError,
                    prototype.MaximumCrossAxisDrift,
                    stroke.CompanionClusterIndex,
                    stroke.CompanionMemberRole,
                    stroke.IntendedCompanionClusterSize);
            return glyph.IsValid;
        }

        private static float ResolveMaximumAllowedProjectedTurnDegrees(
            GroundPaintedAccentGlyphFamily family)
        {
            switch (family)
            {
                case GroundPaintedAccentGlyphFamily.AsymmetricMound:
                    return MaximumAsymmetricMoundTurnDegrees;
                case GroundPaintedAccentGlyphFamily.SingleShoulder:
                    return MaximumSingleShoulderTurnDegrees;
                case GroundPaintedAccentGlyphFamily.ShallowCrest:
                    return MaximumShallowCrestTurnDegrees;
                case GroundPaintedAccentGlyphFamily.CompleteMound:
                default:
                    return MaximumCompleteMoundTurnDegrees;
            }
        }

        private static float ResolveMaximumProjectedTurnDegrees(
            Vector2[] points,
            int pointCount)
        {
            float maximumTurn = 0f;
            if (points == null || pointCount < 3 || points.Length < pointCount)
            {
                return maximumTurn;
            }

            for (int index = 1; index < pointCount - 1; index++)
            {
                Vector2 incoming = points[index] - points[index - 1];
                Vector2 outgoing = points[index + 1] - points[index];
                if (incoming.sqrMagnitude <= 0.00000001f ||
                    outgoing.sqrMagnitude <= 0.00000001f)
                {
                    continue;
                }

                maximumTurn =
                    Mathf.Max(
                        maximumTurn,
                        Vector2.Angle(incoming, outgoing));
            }

            return maximumTurn;
        }

        private static bool TryValidateAndSampleProjectedGlyph(
            Vector2[] projectedPoints,
            float[] halfWidths,
            int pointCount,
            GroundHeightFieldSnapshot baseSurface,
            IReadOnlyList<GroundPaintedAccentRiverExclusionSnapshot> rivers,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            GroundPaintedAccentProjectedGlyphScratch scratch,
            GroundPaintedAccentProjectedGlyphTimingAccumulator timing,
            out GroundPaintedAccentProjectedGlyphRejectionReason rejectionReason)
        {
            rejectionReason =
                GroundPaintedAccentProjectedGlyphRejectionReason.None;

            if (projectedPoints == null ||
                halfWidths == null ||
                scratch == null ||
                pointCount < 2 ||
                projectedPoints.Length < pointCount ||
                halfWidths.Length < pointCount)
            {
                rejectionReason =
                    GroundPaintedAccentProjectedGlyphRejectionReason.Sampling;
                return false;
            }

            scratch.EnsurePointCapacity(pointCount);
            long stageStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
            Vector2 boundsMinimum =
                new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 boundsMaximum =
                new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            float maximumHalfWidth = 0f;

            for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
            {
                Vector2 centerXZ = projectedPoints[pointIndex];
                Vector2 tangent =
                    ResolveTangent(projectedPoints, pointCount, pointIndex);
                Vector2 sideAxis = new Vector2(-tangent.y, tangent.x);
                float halfWidth = Mathf.Max(0.0005f, halfWidths[pointIndex]);
                scratch.LeftPoints[pointIndex] =
                    centerXZ - sideAxis * halfWidth;
                scratch.RightPoints[pointIndex] =
                    centerXZ + sideAxis * halfWidth;
                maximumHalfWidth = Mathf.Max(maximumHalfWidth, halfWidth);
                boundsMinimum = Vector2.Min(boundsMinimum, centerXZ);
                boundsMaximum = Vector2.Max(boundsMaximum, centerXZ);
            }

            Vector2 radialExpansion = Vector2.one * maximumHalfWidth;
            boundsMinimum -= radialExpansion;
            boundsMaximum += radialExpansion;
            int riverCount = BuildRiverCandidateIndices(
                rivers,
                boundsMinimum,
                boundsMaximum,
                scratch);
            int modifierCount = BuildModifierCandidateIndices(
                modifiers,
                boundsMinimum,
                boundsMaximum,
                scratch);
            timing.FootprintPreparationTicks +=
                System.Diagnostics.Stopwatch.GetTimestamp() - stageStartedAt;

            bool hasPreviousCenter = false;
            Vector2 previousCenterXZ = Vector2.zero;
            float previousCenterHeight = 0f;

            for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
            {
                Vector2 centerXZ = projectedPoints[pointIndex];
                Vector2 leftXZ = scratch.LeftPoints[pointIndex];
                Vector2 rightXZ = scratch.RightPoints[pointIndex];
                float halfWidth = Mathf.Max(0.0005f, halfWidths[pointIndex]);

                stageStartedAt = System.Diagnostics.Stopwatch.GetTimestamp();
                bool sampled = baseSurface.TrySampleProjectedFootprint(
                    centerXZ,
                    leftXZ,
                    rightXZ,
                    out GroundProjectedFootprintSample centerSample,
                    out GroundProjectedFootprintSample leftSample,
                    out GroundProjectedFootprintSample rightSample);
                timing.GroundSamplingTicks +=
                    System.Diagnostics.Stopwatch.GetTimestamp() - stageStartedAt;
                if (!sampled)
                {
                    rejectionReason =
                        GroundPaintedAccentProjectedGlyphRejectionReason.Sampling;
                    return false;
                }

                stageStartedAt = System.Diagnostics.Stopwatch.GetTimestamp();
                bool broadSlopeExceeded =
                    ExceedsBroadSlope(centerSample) ||
                    ExceedsBroadSlope(leftSample) ||
                    ExceedsBroadSlope(rightSample);
                timing.BroadSlopeTicks +=
                    System.Diagnostics.Stopwatch.GetTimestamp() - stageStartedAt;
                if (broadSlopeExceeded)
                {
                    rejectionReason =
                        GroundPaintedAccentProjectedGlyphRejectionReason.BroadSlope;
                    return false;
                }

                stageStartedAt = System.Diagnostics.Stopwatch.GetTimestamp();
                bool riverExcluded =
                    IsExcludedByRiver(
                        centerXZ,
                        rivers,
                        scratch.RiverIndices,
                        riverCount,
                        halfWidth + RiverSafetyClearance) ||
                    IsExcludedByRiver(
                        leftXZ,
                        rivers,
                        scratch.RiverIndices,
                        riverCount,
                        RiverSafetyClearance) ||
                    IsExcludedByRiver(
                        rightXZ,
                        rivers,
                        scratch.RiverIndices,
                        riverCount,
                        RiverSafetyClearance);
                timing.RiverExclusionTicks +=
                    System.Diagnostics.Stopwatch.GetTimestamp() - stageStartedAt;
                if (riverExcluded)
                {
                    rejectionReason =
                        GroundPaintedAccentProjectedGlyphRejectionReason.River;
                    return false;
                }

                stageStartedAt = System.Diagnostics.Stopwatch.GetTimestamp();
                bool modifierExcluded =
                    IsExcludedByModifier(
                        centerXZ,
                        modifiers,
                        scratch.ModifierIndices,
                        modifierCount) ||
                    IsExcludedByModifier(
                        leftXZ,
                        modifiers,
                        scratch.ModifierIndices,
                        modifierCount) ||
                    IsExcludedByModifier(
                        rightXZ,
                        modifiers,
                        scratch.ModifierIndices,
                        modifierCount);
                timing.ModifierExclusionTicks +=
                    System.Diagnostics.Stopwatch.GetTimestamp() - stageStartedAt;
                if (modifierExcluded)
                {
                    rejectionReason =
                        GroundPaintedAccentProjectedGlyphRejectionReason.ModifierExclusion;
                    return false;
                }

                stageStartedAt = System.Diagnostics.Stopwatch.GetTimestamp();
                bool transverseGradeExceeded =
                    ExceedsGrade(
                        leftSample.Height,
                        centerSample.Height,
                        Vector2.Distance(leftXZ, centerXZ)) ||
                    ExceedsGrade(
                        centerSample.Height,
                        rightSample.Height,
                        Vector2.Distance(centerXZ, rightXZ));
                timing.TransverseGradeTicks +=
                    System.Diagnostics.Stopwatch.GetTimestamp() - stageStartedAt;
                if (transverseGradeExceeded)
                {
                    rejectionReason =
                        GroundPaintedAccentProjectedGlyphRejectionReason.LocalGrade;
                    return false;
                }

                stageStartedAt = System.Diagnostics.Stopwatch.GetTimestamp();
                bool longitudinalGradeExceeded =
                    hasPreviousCenter &&
                    ExceedsGrade(
                        previousCenterHeight,
                        centerSample.Height,
                        Vector2.Distance(previousCenterXZ, centerXZ));
                timing.LongitudinalGradeTicks +=
                    System.Diagnostics.Stopwatch.GetTimestamp() - stageStartedAt;
                if (longitudinalGradeExceeded)
                {
                    rejectionReason =
                        GroundPaintedAccentProjectedGlyphRejectionReason.LocalGrade;
                    return false;
                }

                scratch.SurfacePoints[pointIndex] =
                    new Vector3(centerXZ.x, centerSample.Height, centerXZ.y);
                hasPreviousCenter = true;
                previousCenterXZ = centerXZ;
                previousCenterHeight = centerSample.Height;
            }

            return true;
        }

        private static int BuildRiverCandidateIndices(
            IReadOnlyList<GroundPaintedAccentRiverExclusionSnapshot> rivers,
            Vector2 boundsMinimum,
            Vector2 boundsMaximum,
            GroundPaintedAccentProjectedGlyphScratch scratch)
        {
            if (rivers == null || rivers.Count == 0)
            {
                return 0;
            }

            scratch.EnsureRiverCapacity(rivers.Count);
            int count = 0;
            for (int riverIndex = 0; riverIndex < rivers.Count; riverIndex++)
            {
                if (!rivers[riverIndex].Intersects(
                        boundsMinimum,
                        boundsMaximum,
                        RiverSafetyClearance))
                {
                    continue;
                }

                scratch.RiverIndices[count++] = riverIndex;
            }

            return count;
        }

        private static int BuildModifierCandidateIndices(
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            Vector2 boundsMinimum,
            Vector2 boundsMaximum,
            GroundPaintedAccentProjectedGlyphScratch scratch)
        {
            if (modifiers == null || modifiers.Count == 0)
            {
                return 0;
            }

            scratch.EnsureModifierCapacity(modifiers.Count);
            int count = 0;
            for (int modifierIndex = 0;
                 modifierIndex < modifiers.Count;
                 modifierIndex++)
            {
                GroundModifierSnapshot modifier = modifiers[modifierIndex];
                if (!modifier.Excludes(
                        GroundSurfaceFeatureExclusionFlags.PaintedAccentLines) ||
                    !ModifierIntersectsBounds(
                        modifier,
                        boundsMinimum,
                        boundsMaximum))
                {
                    continue;
                }

                scratch.ModifierIndices[count++] = modifierIndex;
            }

            return count;
        }

        private static bool ModifierIntersectsBounds(
            GroundModifierSnapshot modifier,
            Vector2 boundsMinimum,
            Vector2 boundsMaximum)
        {
            Vector2 extents;
            if (modifier.Shape == GroundModifierShape.Circle)
            {
                float radius = modifier.CircleRadius + modifier.BlendDistance;
                extents = Vector2.one * radius;
            }
            else
            {
                Vector2 halfSize = modifier.BoxSize * 0.5f;
                extents =
                    new Vector2(
                        Mathf.Abs(modifier.Right.x) * halfSize.x +
                        Mathf.Abs(modifier.Forward.x) * halfSize.y,
                        Mathf.Abs(modifier.Right.y) * halfSize.x +
                        Mathf.Abs(modifier.Forward.y) * halfSize.y) +
                    Vector2.one * modifier.BlendDistance;
            }

            Vector2 modifierMinimum = modifier.Centre - extents;
            Vector2 modifierMaximum = modifier.Centre + extents;
            return boundsMaximum.x >= modifierMinimum.x &&
                   boundsMinimum.x <= modifierMaximum.x &&
                   boundsMaximum.y >= modifierMinimum.y &&
                   boundsMinimum.y <= modifierMaximum.y;
        }

        private static Vector2 ResolveTangent(
            Vector2[] points,
            int pointCount,
            int pointIndex)
        {
            int beforeIndex = Mathf.Max(0, pointIndex - 1);
            int afterIndex = Mathf.Min(pointCount - 1, pointIndex + 1);
            Vector2 tangent = points[afterIndex] - points[beforeIndex];

            if (tangent.sqrMagnitude > 0.000001f)
            {
                return tangent.normalized;
            }

            return Vector2.right;
        }

        private static bool ExceedsBroadSlope(GroundProjectedFootprintSample sample)
        {
            Vector3 normal =
                sample.RenderNormal.sqrMagnitude > 0.000001f
                    ? sample.RenderNormal.normalized
                    : Vector3.up;
            return Vector3.Angle(normal, Vector3.up) > MaximumBroadSlopeDegrees;
        }

        private static bool ExceedsGrade(
            float heightA,
            float heightB,
            float horizontalDistance)
        {
            if (horizontalDistance <= 0.00001f)
            {
                return Mathf.Abs(heightB - heightA) > 0.00001f;
            }

            float gradeDegrees =
                Mathf.Atan2(
                    Mathf.Abs(heightB - heightA),
                    horizontalDistance) *
                Mathf.Rad2Deg;
            return gradeDegrees > MaximumLocalGradeDegrees;
        }

        private static bool IsExcludedByRiver(
            Vector2 point,
            IReadOnlyList<GroundPaintedAccentRiverExclusionSnapshot> rivers,
            int[] candidateIndices,
            int candidateCount,
            float clearance)
        {
            if (rivers == null || candidateIndices == null)
            {
                return false;
            }

            for (int candidateIndex = 0;
                 candidateIndex < candidateCount;
                 candidateIndex++)
            {
                GroundPaintedAccentRiverExclusionSnapshot candidate =
                    rivers[candidateIndices[candidateIndex]];
                StylizedRiverGroundSnapshot river = candidate.Snapshot;
                if (!river.IsValid ||
                    !river.TryEvaluate(
                        point,
                        out float distance,
                        out _,
                        out _,
                        out float surfaceHalfWidth))
                {
                    continue;
                }

                if (distance <=
                    river.ResolveHandoffHalfWidth(surfaceHalfWidth) +
                    Mathf.Max(0f, clearance))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsExcludedByModifier(
            Vector2 point,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            int[] candidateIndices,
            int candidateCount)
        {
            if (modifiers == null || candidateIndices == null)
            {
                return false;
            }

            for (int candidateIndex = 0;
                 candidateIndex < candidateCount;
                 candidateIndex++)
            {
                GroundModifierSnapshot modifier =
                    modifiers[candidateIndices[candidateIndex]];
                if (modifier.EvaluateWeight(point) > 0.0001f)
                {
                    return true;
                }
            }

            return false;
        }


    }
}
