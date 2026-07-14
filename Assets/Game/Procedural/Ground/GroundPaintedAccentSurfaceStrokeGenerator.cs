using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Rivers;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    public enum GroundPaintedAccentCompositionRegionMode
    {
        Quiet = 0,
        Supporting = 1,
        Accent = 2
    }

    public enum GroundPaintedAccentCompositionRole
    {
        Support = 0,
        Standard = 1,
        Dominant = 2
    }

    public enum GroundPaintedAccentGlyphFamily
    {
        CompleteMound = 0,
        AsymmetricMound = 1,
        SingleShoulder = 2,
        ShallowCrest = 3
    }

    internal enum GroundPaintedAccentCompanionMemberRole
    {
        None = 0,
        Primary = 1,
        Secondary = 2,
        Tertiary = 3
    }

    internal enum GroundPaintedAccentCompanionPairLayout
    {
        None = 0,
        ShallowOffset = 1,
        SteppedContinuation = 2,
        ShoulderContact = 3,
        OffsetEcho = 4
    }

    internal enum GroundPaintedAccentCompanionTripletLayout
    {
        None = 0,
        SteppedRun = 1,
        CrownRun = 2,
        BrokenTerrace = 3,
        ShallowRun = 4
    }

    internal readonly struct GroundPaintedAccentSurfaceStrokeVariant
    {
        public GroundPaintedAccentSurfaceStrokeVariant(
            Vector3[] localPoints,
            Vector3[] localNormals,
            float width,
            float bodyWidth,
            float strength,
            float authoredLength,
            GroundPaintedAccentGlyphFamily family,
            int seed)
        {
            LocalPoints = localPoints ?? Array.Empty<Vector3>();
            LocalNormals = localNormals ?? Array.Empty<Vector3>();
            Width = Mathf.Max(0.001f, width);
            BodyWidth = Mathf.Max(Width, bodyWidth);
            Strength = Mathf.Clamp01(strength);
            AuthoredLength = Mathf.Max(0f, authoredLength);
            Family = family;
            Seed = seed;
        }

        public Vector3[] LocalPoints { get; }
        public Vector3[] LocalNormals { get; }
        public float Width { get; }
        public float BodyWidth { get; }
        public float Strength { get; }
        public float AuthoredLength { get; }
        public GroundPaintedAccentGlyphFamily Family { get; }
        public int Seed { get; }

        public bool IsValid =>
            LocalPoints != null &&
            LocalNormals != null &&
            LocalPoints.Length >= 2 &&
            LocalNormals.Length == LocalPoints.Length;
    }

    internal readonly struct GroundPaintedAccentSurfaceStroke
    {
        public GroundPaintedAccentSurfaceStroke(
            Vector3[] localPoints,
            Vector3[] localNormals,
            float width,
            float bodyWidth,
            float strength,
            float authoredLength,
            GroundPaintedAccentGlyphFamily family,
            int seed)
            : this(
                localPoints,
                localNormals,
                width,
                bodyWidth,
                strength,
                authoredLength,
                family,
                seed,
                -1,
                GroundPaintedAccentCompanionMemberRole.None,
                0,
                GroundPaintedAccentCompanionPairLayout.None,
                default,
                0,
                GroundPaintedAccentCompositionRegionMode.Supporting)
        {
        }

        private GroundPaintedAccentSurfaceStroke(
            Vector3[] localPoints,
            Vector3[] localNormals,
            float width,
            float bodyWidth,
            float strength,
            float authoredLength,
            GroundPaintedAccentGlyphFamily family,
            int seed,
            int companionClusterIndex,
            GroundPaintedAccentCompanionMemberRole companionMemberRole,
            int intendedCompanionClusterSize,
            GroundPaintedAccentCompanionPairLayout companionPairLayout,
            GroundPaintedAccentSurfaceStrokeVariant independentFallback,
            int proposalRankQuartile,
            GroundPaintedAccentCompositionRegionMode compositionRegionMode)
        {
            LocalPoints = localPoints ?? Array.Empty<Vector3>();
            LocalNormals = localNormals ?? Array.Empty<Vector3>();
            Width = Mathf.Max(0.001f, width);
            BodyWidth = Mathf.Max(Width, bodyWidth);
            Strength = Mathf.Clamp01(strength);
            AuthoredLength = Mathf.Max(0f, authoredLength);
            Family = family;
            Seed = seed;
            CompanionClusterIndex = companionClusterIndex;
            CompanionMemberRole = companionMemberRole;
            IntendedCompanionClusterSize =
                companionClusterIndex >= 0
                    ? Mathf.Clamp(intendedCompanionClusterSize, 2, 3)
                    : 0;
            CompanionPairLayout =
                companionClusterIndex >= 0 &&
                IntendedCompanionClusterSize == 2
                    ? companionPairLayout
                    : GroundPaintedAccentCompanionPairLayout.None;
            IndependentFallback = independentFallback;
            ProposalRankQuartile = Mathf.Clamp(proposalRankQuartile, 0, 3);
            CompositionRegionMode = compositionRegionMode;
        }

        public Vector3[] LocalPoints { get; }
        public Vector3[] LocalNormals { get; }
        public float Width { get; }
        public float BodyWidth { get; }
        public float Strength { get; }
        public float AuthoredLength { get; }
        public GroundPaintedAccentGlyphFamily Family { get; }
        public int Seed { get; }
        public int CompanionClusterIndex { get; }
        public GroundPaintedAccentCompanionMemberRole CompanionMemberRole { get; }
        public int IntendedCompanionClusterSize { get; }
        public GroundPaintedAccentCompanionPairLayout CompanionPairLayout { get; }
        public GroundPaintedAccentSurfaceStrokeVariant IndependentFallback { get; }
        public int ProposalRankQuartile { get; }
        public GroundPaintedAccentCompositionRegionMode CompositionRegionMode
        {
            get;
        }
        public bool IsCompanionMember => CompanionClusterIndex >= 0;
        public bool HasIndependentFallback => IndependentFallback.IsValid;

        public GroundPaintedAccentSurfaceStroke WithCompanionMetadata(
            int companionClusterIndex,
            GroundPaintedAccentCompanionMemberRole companionMemberRole,
            int intendedCompanionClusterSize,
            GroundPaintedAccentCompanionPairLayout companionPairLayout,
            GroundPaintedAccentSurfaceStrokeVariant independentFallback)
        {
            return new GroundPaintedAccentSurfaceStroke(
                LocalPoints,
                LocalNormals,
                Width,
                BodyWidth,
                Strength,
                AuthoredLength,
                Family,
                Seed,
                companionClusterIndex,
                companionMemberRole,
                intendedCompanionClusterSize,
                companionPairLayout,
                independentFallback,
                ProposalRankQuartile,
                CompositionRegionMode);
        }

        public GroundPaintedAccentSurfaceStroke WithPlacementMetadata(
            int proposalRankQuartile,
            GroundPaintedAccentCompositionRegionMode compositionRegionMode)
        {
            return new GroundPaintedAccentSurfaceStroke(
                LocalPoints,
                LocalNormals,
                Width,
                BodyWidth,
                Strength,
                AuthoredLength,
                Family,
                Seed,
                CompanionClusterIndex,
                CompanionMemberRole,
                IntendedCompanionClusterSize,
                CompanionPairLayout,
                IndependentFallback,
                proposalRankQuartile,
                compositionRegionMode);
        }

        public GroundPaintedAccentSurfaceStroke CreateIndependentFallback()
        {
            if (!IndependentFallback.IsValid)
            {
                return default;
            }

            return new GroundPaintedAccentSurfaceStroke(
                IndependentFallback.LocalPoints,
                IndependentFallback.LocalNormals,
                IndependentFallback.Width,
                IndependentFallback.BodyWidth,
                IndependentFallback.Strength,
                IndependentFallback.AuthoredLength,
                IndependentFallback.Family,
                IndependentFallback.Seed)
                .WithPlacementMetadata(
                    ProposalRankQuartile,
                    CompositionRegionMode);
        }

        public GroundPaintedAccentSurfaceStrokeVariant ToVariant()
        {
            return new GroundPaintedAccentSurfaceStrokeVariant(
                LocalPoints,
                LocalNormals,
                Width,
                BodyWidth,
                Strength,
                AuthoredLength,
                Family,
                Seed);
        }

        public bool IsValid =>
            LocalPoints != null &&
            LocalNormals != null &&
            LocalPoints.Length >= 2 &&
            LocalNormals.Length == LocalPoints.Length;
    }

    internal readonly struct GroundPaintedAccentProposalRankDiagnostics
    {
        public GroundPaintedAccentProposalRankDiagnostics(
            Vector4 selected,
            Vector4 accepted)
        {
            Selected = MaxZero(selected);
            Accepted = MaxZero(accepted);
        }

        public Vector4 Selected { get; }
        public Vector4 Accepted { get; }

        private static Vector4 MaxZero(Vector4 value)
        {
            return new Vector4(
                Mathf.Max(0f, value.x),
                Mathf.Max(0f, value.y),
                Mathf.Max(0f, value.z),
                Mathf.Max(0f, value.w));
        }
    }

    internal readonly struct GroundPaintedAccentSurfaceStrokeBuildTimings
    {
        public GroundPaintedAccentSurfaceStrokeBuildTimings(
            double candidateBuildMilliseconds,
            double regionalWeightingMilliseconds,
            double candidateOrderingMilliseconds,
            double compositionSetupMilliseconds,
            double strokeSetupMilliseconds,
            double surfaceConstructionValidationMilliseconds,
            double diagnosticsMilliseconds)
        {
            CandidateBuildMilliseconds = Math.Max(0d, candidateBuildMilliseconds);
            RegionalWeightingMilliseconds = Math.Max(0d, regionalWeightingMilliseconds);
            CandidateOrderingMilliseconds = Math.Max(0d, candidateOrderingMilliseconds);
            CompositionSetupMilliseconds = Math.Max(0d, compositionSetupMilliseconds);
            StrokeSetupMilliseconds = Math.Max(0d, strokeSetupMilliseconds);
            SurfaceConstructionValidationMilliseconds =
                Math.Max(0d, surfaceConstructionValidationMilliseconds);
            DiagnosticsMilliseconds = Math.Max(0d, diagnosticsMilliseconds);
        }

        public double CandidateBuildMilliseconds { get; }
        public double RegionalWeightingMilliseconds { get; }
        public double CandidateOrderingMilliseconds { get; }
        public double CompositionSetupMilliseconds { get; }
        public double StrokeSetupMilliseconds { get; }
        public double SurfaceConstructionValidationMilliseconds { get; }
        public double DiagnosticsMilliseconds { get; }

        public static GroundPaintedAccentSurfaceStrokeBuildTimings Empty =>
            default;
    }

    internal readonly struct GroundPaintedAccentPlacementDiagnostics
    {
        public GroundPaintedAccentPlacementDiagnostics(
            int targetProposals,
            int candidatePool,
            int proposed,
            int physicallyEvaluated,
            int accepted,
            float distributionPatchScale,
            float distributionPatchiness,
            float distributionSparseFloor,
            float proposalPatchWeightMin,
            float proposalPatchWeightMean,
            float proposalPatchWeightMax,
            int rejectedSampling,
            int rejectedRiver,
            int rejectedModifierExclusion,
            int rejectedBroadSlope,
            int rejectedLocalGrade,
            int horizontalCompanionClusterCount,
            int horizontalCompanionPairClusterCount,
            int horizontalCompanionTripletClusterCount,
            int horizontalCompanionPairShallowLayoutCount,
            int horizontalCompanionPairSteppedLayoutCount,
            int horizontalCompanionPairShoulderLayoutCount,
            int horizontalCompanionPairOffsetLayoutCount,
            int horizontalCompanionParticipantCount,
            int horizontalCompanionAcceptedClusterCount,
            int horizontalCompanionAcceptedParticipantCount,
            int horizontalCompanionTargetParticipantCount,
            int horizontalCompanionFormedParticipantCount,
            int horizontalCompanionPrimaryClusterAttempts,
            int horizontalCompanionTargetPlanRejected,
            int horizontalCompanionNoSecondaryCandidate,
            int horizontalCompanionNoTertiaryCandidate,
            int horizontalCompanionTripletToPairDowngrade,
            int horizontalCompanionCrossRegionCandidateSelection,
            int horizontalCompanionPairStructureRepair,
            int horizontalCompanionTripletStructureRepair,
            int horizontalCompanionEnvelopeCollision,
            int horizontalCompanionOccupiedCell,
            int horizontalCompanionPairStructureRejected,
            int horizontalCompanionTripletStructureRejected,
            int horizontalCompanionSourceMemberPhysicalFallback,
            int horizontalCompanionSourceClusterIncomplete,
            int compositionRegionCount,
            int quietRegionCount,
            int supportingRegionCount,
            int accentRegionCount,
            int quietProposalCount,
            int supportingProposalCount,
            int accentProposalCount,
            int quietAcceptedCount,
            int supportingAcceptedCount,
            int accentAcceptedCount,
            int dominantAcceptedCount,
            int standardAcceptedCount,
            int supportAcceptedCount,
            int completeMoundSelectedCount,
            int asymmetricMoundSelectedCount,
            int singleShoulderSelectedCount,
            int shallowCrestSelectedCount,
            int completeMoundAcceptedCount,
            int asymmetricMoundAcceptedCount,
            int singleShoulderAcceptedCount,
            int shallowCrestAcceptedCount,
            float compositionRegionScale,
            float acceptedLengthMin,
            float acceptedLengthMean,
            float acceptedLengthMax,
            float acceptedAngleOffsetMin,
            float acceptedAngleOffsetMean,
            float acceptedAngleOffsetMax,
            float acceptedCompanionAngleOffsetMin,
            float acceptedCompanionAngleOffsetMean,
            float acceptedCompanionAngleOffsetMax,
            GroundPaintedAccentProposalRankDiagnostics proposalRankDiagnostics,
            GroundPaintedAccentSurfaceStrokeBuildTimings buildTimings)
        {
            TargetProposals = Mathf.Max(0, targetProposals);
            CandidatePool = Mathf.Max(0, candidatePool);
            Proposed = Mathf.Max(0, proposed);
            PhysicallyEvaluated = Mathf.Max(0, physicallyEvaluated);
            Accepted = Mathf.Max(0, accepted);
            DistributionPatchScale = Mathf.Max(0f, distributionPatchScale);
            DistributionPatchiness = Mathf.Clamp01(distributionPatchiness);
            DistributionSparseFloor = Mathf.Clamp01(distributionSparseFloor);
            ProposalPatchWeightMin = Mathf.Clamp01(proposalPatchWeightMin);
            ProposalPatchWeightMean = Mathf.Clamp01(proposalPatchWeightMean);
            ProposalPatchWeightMax = Mathf.Clamp01(proposalPatchWeightMax);
            RejectedSampling = Mathf.Max(0, rejectedSampling);
            RejectedRiver = Mathf.Max(0, rejectedRiver);
            RejectedModifierExclusion = Mathf.Max(0, rejectedModifierExclusion);
            RejectedBroadSlope = Mathf.Max(0, rejectedBroadSlope);
            RejectedLocalGrade = Mathf.Max(0, rejectedLocalGrade);
            HorizontalCompanionClusterCount =
                Mathf.Max(0, horizontalCompanionClusterCount);
            HorizontalCompanionPairClusterCount =
                Mathf.Max(0, horizontalCompanionPairClusterCount);
            HorizontalCompanionTripletClusterCount =
                Mathf.Max(0, horizontalCompanionTripletClusterCount);
            HorizontalCompanionPairShallowLayoutCount =
                Mathf.Max(0, horizontalCompanionPairShallowLayoutCount);
            HorizontalCompanionPairSteppedLayoutCount =
                Mathf.Max(0, horizontalCompanionPairSteppedLayoutCount);
            HorizontalCompanionPairShoulderLayoutCount =
                Mathf.Max(0, horizontalCompanionPairShoulderLayoutCount);
            HorizontalCompanionPairOffsetLayoutCount =
                Mathf.Max(0, horizontalCompanionPairOffsetLayoutCount);
            HorizontalCompanionParticipantCount =
                Mathf.Max(0, horizontalCompanionParticipantCount);
            HorizontalCompanionAcceptedClusterCount =
                Mathf.Max(0, horizontalCompanionAcceptedClusterCount);
            HorizontalCompanionAcceptedParticipantCount =
                Mathf.Max(0, horizontalCompanionAcceptedParticipantCount);
            HorizontalCompanionTargetParticipantCount =
                Mathf.Max(0, horizontalCompanionTargetParticipantCount);
            HorizontalCompanionFormedParticipantCount =
                Mathf.Max(0, horizontalCompanionFormedParticipantCount);
            HorizontalCompanionPrimaryClusterAttempts =
                Mathf.Max(0, horizontalCompanionPrimaryClusterAttempts);
            HorizontalCompanionTargetPlanRejected =
                Mathf.Max(0, horizontalCompanionTargetPlanRejected);
            HorizontalCompanionNoSecondaryCandidate =
                Mathf.Max(0, horizontalCompanionNoSecondaryCandidate);
            HorizontalCompanionNoTertiaryCandidate =
                Mathf.Max(0, horizontalCompanionNoTertiaryCandidate);
            HorizontalCompanionTripletToPairDowngrade =
                Mathf.Max(0, horizontalCompanionTripletToPairDowngrade);
            HorizontalCompanionCrossRegionCandidateSelection =
                Mathf.Max(0, horizontalCompanionCrossRegionCandidateSelection);
            HorizontalCompanionPairStructureRepair =
                Mathf.Max(0, horizontalCompanionPairStructureRepair);
            HorizontalCompanionTripletStructureRepair =
                Mathf.Max(0, horizontalCompanionTripletStructureRepair);
            HorizontalCompanionEnvelopeCollision =
                Mathf.Max(0, horizontalCompanionEnvelopeCollision);
            HorizontalCompanionOccupiedCell =
                Mathf.Max(0, horizontalCompanionOccupiedCell);
            HorizontalCompanionPairStructureRejected =
                Mathf.Max(0, horizontalCompanionPairStructureRejected);
            HorizontalCompanionTripletStructureRejected =
                Mathf.Max(0, horizontalCompanionTripletStructureRejected);
            HorizontalCompanionSourceMemberPhysicalFallback =
                Mathf.Max(0, horizontalCompanionSourceMemberPhysicalFallback);
            HorizontalCompanionSourceClusterIncomplete =
                Mathf.Max(0, horizontalCompanionSourceClusterIncomplete);
            CompositionRegionCount = Mathf.Max(0, compositionRegionCount);
            QuietRegionCount = Mathf.Max(0, quietRegionCount);
            SupportingRegionCount = Mathf.Max(0, supportingRegionCount);
            AccentRegionCount = Mathf.Max(0, accentRegionCount);
            QuietProposalCount = Mathf.Max(0, quietProposalCount);
            SupportingProposalCount = Mathf.Max(0, supportingProposalCount);
            AccentProposalCount = Mathf.Max(0, accentProposalCount);
            QuietAcceptedCount = Mathf.Max(0, quietAcceptedCount);
            SupportingAcceptedCount = Mathf.Max(0, supportingAcceptedCount);
            AccentAcceptedCount = Mathf.Max(0, accentAcceptedCount);
            DominantAcceptedCount = Mathf.Max(0, dominantAcceptedCount);
            StandardAcceptedCount = Mathf.Max(0, standardAcceptedCount);
            SupportAcceptedCount = Mathf.Max(0, supportAcceptedCount);
            CompleteMoundSelectedCount = Mathf.Max(0, completeMoundSelectedCount);
            AsymmetricMoundSelectedCount = Mathf.Max(0, asymmetricMoundSelectedCount);
            SingleShoulderSelectedCount = Mathf.Max(0, singleShoulderSelectedCount);
            ShallowCrestSelectedCount = Mathf.Max(0, shallowCrestSelectedCount);
            CompleteMoundAcceptedCount = Mathf.Max(0, completeMoundAcceptedCount);
            AsymmetricMoundAcceptedCount = Mathf.Max(0, asymmetricMoundAcceptedCount);
            SingleShoulderAcceptedCount = Mathf.Max(0, singleShoulderAcceptedCount);
            ShallowCrestAcceptedCount = Mathf.Max(0, shallowCrestAcceptedCount);
            CompositionRegionScale = Mathf.Max(0f, compositionRegionScale);
            AcceptedLengthMin = Mathf.Max(0f, acceptedLengthMin);
            AcceptedLengthMean = Mathf.Max(0f, acceptedLengthMean);
            AcceptedLengthMax = Mathf.Max(0f, acceptedLengthMax);
            AcceptedAngleOffsetMin = acceptedAngleOffsetMin;
            AcceptedAngleOffsetMean = acceptedAngleOffsetMean;
            AcceptedAngleOffsetMax = acceptedAngleOffsetMax;
            AcceptedCompanionAngleOffsetMin =
                acceptedCompanionAngleOffsetMin;
            AcceptedCompanionAngleOffsetMean =
                acceptedCompanionAngleOffsetMean;
            AcceptedCompanionAngleOffsetMax =
                acceptedCompanionAngleOffsetMax;
            ProposalRankDiagnostics = proposalRankDiagnostics;
            BuildTimings = buildTimings;
        }

        public int TargetProposals { get; }
        public int CandidatePool { get; }
        public int Proposed { get; }
        public int PhysicallyEvaluated { get; }
        public int Accepted { get; }
        public float DistributionPatchScale { get; }
        public float DistributionPatchiness { get; }
        public float DistributionSparseFloor { get; }
        public float ProposalPatchWeightMin { get; }
        public float ProposalPatchWeightMean { get; }
        public float ProposalPatchWeightMax { get; }
        public int RejectedSampling { get; }
        public int RejectedRiver { get; }
        public int RejectedModifierExclusion { get; }
        public int RejectedBroadSlope { get; }
        public int RejectedLocalGrade { get; }
        public int HorizontalCompanionClusterCount { get; }
        public int HorizontalCompanionPairClusterCount { get; }
        public int HorizontalCompanionTripletClusterCount { get; }
        public int HorizontalCompanionPairShallowLayoutCount { get; }
        public int HorizontalCompanionPairSteppedLayoutCount { get; }
        public int HorizontalCompanionPairShoulderLayoutCount { get; }
        public int HorizontalCompanionPairOffsetLayoutCount { get; }
        public int HorizontalCompanionParticipantCount { get; }
        public int HorizontalCompanionAcceptedClusterCount { get; }
        public int HorizontalCompanionAcceptedParticipantCount { get; }
        public int HorizontalCompanionTargetParticipantCount { get; }
        public int HorizontalCompanionFormedParticipantCount { get; }
        public int HorizontalCompanionPrimaryClusterAttempts { get; }
        public int HorizontalCompanionTargetPlanRejected { get; }
        public int HorizontalCompanionNoSecondaryCandidate { get; }
        public int HorizontalCompanionNoTertiaryCandidate { get; }
        public int HorizontalCompanionTripletToPairDowngrade { get; }
        public int HorizontalCompanionCrossRegionCandidateSelection { get; }
        public int HorizontalCompanionPairStructureRepair { get; }
        public int HorizontalCompanionTripletStructureRepair { get; }
        public int HorizontalCompanionEnvelopeCollision { get; }
        public int HorizontalCompanionOccupiedCell { get; }
        public int HorizontalCompanionPairStructureRejected { get; }
        public int HorizontalCompanionTripletStructureRejected { get; }
        public int HorizontalCompanionSourceMemberPhysicalFallback { get; }
        public int HorizontalCompanionSourceClusterIncomplete { get; }
        public int CompositionRegionCount { get; }
        public int QuietRegionCount { get; }
        public int SupportingRegionCount { get; }
        public int AccentRegionCount { get; }
        public int QuietProposalCount { get; }
        public int SupportingProposalCount { get; }
        public int AccentProposalCount { get; }
        public int QuietAcceptedCount { get; }
        public int SupportingAcceptedCount { get; }
        public int AccentAcceptedCount { get; }
        public int DominantAcceptedCount { get; }
        public int StandardAcceptedCount { get; }
        public int SupportAcceptedCount { get; }
        public int CompleteMoundSelectedCount { get; }
        public int AsymmetricMoundSelectedCount { get; }
        public int SingleShoulderSelectedCount { get; }
        public int ShallowCrestSelectedCount { get; }
        public int CompleteMoundAcceptedCount { get; }
        public int AsymmetricMoundAcceptedCount { get; }
        public int SingleShoulderAcceptedCount { get; }
        public int ShallowCrestAcceptedCount { get; }
        public float CompositionRegionScale { get; }
        public float AcceptedLengthMin { get; }
        public float AcceptedLengthMean { get; }
        public float AcceptedLengthMax { get; }
        public float AcceptedAngleOffsetMin { get; }
        public float AcceptedAngleOffsetMean { get; }
        public float AcceptedAngleOffsetMax { get; }
        public float AcceptedCompanionAngleOffsetMin { get; }
        public float AcceptedCompanionAngleOffsetMean { get; }
        public float AcceptedCompanionAngleOffsetMax { get; }
        public GroundPaintedAccentProposalRankDiagnostics ProposalRankDiagnostics
        {
            get;
        }
        public GroundPaintedAccentSurfaceStrokeBuildTimings BuildTimings
        {
            get;
        }

        public int RejectedPhysicalValidation =>
            RejectedSampling +
            RejectedRiver +
            RejectedModifierExclusion +
            RejectedBroadSlope +
            RejectedLocalGrade;

        public static GroundPaintedAccentPlacementDiagnostics Empty => default;
    }

    public readonly struct GroundPaintedAccentCompositionProposalDebugPoint
    {
        public GroundPaintedAccentCompositionProposalDebugPoint(
            Vector3 localPosition,
            GroundPaintedAccentCompositionRegionMode regionMode)
        {
            LocalPosition = localPosition;
            RegionMode = regionMode;
        }

        public Vector3 LocalPosition { get; }
        public GroundPaintedAccentCompositionRegionMode RegionMode { get; }
    }

    public readonly struct GroundPaintedAccentCompositionRegionDebug
    {
        public GroundPaintedAccentCompositionRegionDebug(
            Vector3 localPosition,
            Vector2 localDirection,
            GroundPaintedAccentCompositionRegionMode regionMode,
            float selectionMultiplier,
            bool isOccupied)
        {
            LocalPosition = localPosition;
            LocalDirection =
                localDirection.sqrMagnitude > 0.000001f
                    ? localDirection.normalized
                    : Vector2.right;
            RegionMode = regionMode;
            SelectionMultiplier = Mathf.Max(0.001f, selectionMultiplier);
            IsOccupied = isOccupied;
        }

        public Vector3 LocalPosition { get; }
        public Vector2 LocalDirection { get; }
        public GroundPaintedAccentCompositionRegionMode RegionMode { get; }
        public float SelectionMultiplier { get; }
        public bool IsOccupied { get; }
    }

    public readonly struct GroundPaintedAccentCompositionMarkDebugPoint
    {
        public GroundPaintedAccentCompositionMarkDebugPoint(
            Vector3 localPosition,
            Vector2 localDirection,
            GroundPaintedAccentCompositionRegionMode regionMode,
            GroundPaintedAccentCompositionRole role,
            GroundPaintedAccentGlyphFamily family,
            float length)
        {
            LocalPosition = localPosition;
            LocalDirection =
                localDirection.sqrMagnitude > 0.000001f
                    ? localDirection.normalized
                    : Vector2.right;
            RegionMode = regionMode;
            Role = role;
            Family = family;
            Length = Mathf.Max(0f, length);
        }

        public Vector3 LocalPosition { get; }
        public Vector2 LocalDirection { get; }
        public GroundPaintedAccentCompositionRegionMode RegionMode { get; }
        public GroundPaintedAccentCompositionRole Role { get; }
        public GroundPaintedAccentGlyphFamily Family { get; }
        public float Length { get; }
    }

    public readonly struct GroundPaintedAccentCompositionDebugSnapshot
    {
        public GroundPaintedAccentCompositionDebugSnapshot(
            GroundPaintedAccentCompositionProposalDebugPoint[] proposals,
            GroundPaintedAccentCompositionRegionDebug[] regions,
            GroundPaintedAccentCompositionMarkDebugPoint[] acceptedMarks)
        {
            Proposals =
                proposals ??
                Array.Empty<GroundPaintedAccentCompositionProposalDebugPoint>();
            Regions =
                regions ??
                Array.Empty<GroundPaintedAccentCompositionRegionDebug>();
            AcceptedMarks =
                acceptedMarks ??
                Array.Empty<GroundPaintedAccentCompositionMarkDebugPoint>();
            isValid = true;
        }

        public GroundPaintedAccentCompositionProposalDebugPoint[] Proposals
        {
            get;
        }
        public GroundPaintedAccentCompositionRegionDebug[] Regions { get; }
        public GroundPaintedAccentCompositionMarkDebugPoint[] AcceptedMarks
        {
            get;
        }

        private readonly bool isValid;

        public bool IsValid =>
            isValid &&
            Proposals != null &&
            Regions != null &&
            AcceptedMarks != null;

        public static GroundPaintedAccentCompositionDebugSnapshot Empty =>
            default;
    }

    public readonly struct GroundPaintedAccentDistributionDebugSample
    {
        public GroundPaintedAccentDistributionDebugSample(
            Vector3 localPosition,
            float patchWeight,
            float effectiveProposalWeight,
            bool isValid)
        {
            LocalPosition = localPosition;
            PatchWeight = Mathf.Clamp01(patchWeight);
            EffectiveProposalWeight =
                Mathf.Clamp01(effectiveProposalWeight);
            IsValid = isValid;
        }

        public Vector3 LocalPosition { get; }
        public float PatchWeight { get; }
        public float EffectiveProposalWeight { get; }
        public bool IsValid { get; }
    }

    public readonly struct GroundPaintedAccentProposalDebugPoint
    {
        public GroundPaintedAccentProposalDebugPoint(
            Vector3 localPosition,
            float patchWeight,
            float effectiveProposalWeight)
        {
            LocalPosition = localPosition;
            PatchWeight = Mathf.Clamp01(patchWeight);
            EffectiveProposalWeight =
                Mathf.Clamp01(effectiveProposalWeight);
        }

        public Vector3 LocalPosition { get; }
        public float PatchWeight { get; }
        public float EffectiveProposalWeight { get; }
    }

    public readonly struct GroundPaintedAccentPlacementDebugSnapshot
    {
        public GroundPaintedAccentPlacementDebugSnapshot(
            GroundPaintedAccentDistributionDebugSample[] distributionSamples,
            int distributionSampleResolution,
            GroundPaintedAccentProposalDebugPoint[] proposedPoints,
            int targetProposals,
            int candidatePool)
        {
            DistributionSamples =
                distributionSamples ??
                Array.Empty<GroundPaintedAccentDistributionDebugSample>();
            DistributionSampleResolution =
                Mathf.Max(0, distributionSampleResolution);
            ProposedPoints =
                proposedPoints ??
                Array.Empty<GroundPaintedAccentProposalDebugPoint>();
            TargetProposals = Mathf.Max(0, targetProposals);
            CandidatePool = Mathf.Max(0, candidatePool);
        }

        public GroundPaintedAccentDistributionDebugSample[] DistributionSamples
        {
            get;
        }

        public int DistributionSampleResolution { get; }

        public GroundPaintedAccentProposalDebugPoint[] ProposedPoints
        {
            get;
        }

        public int TargetProposals { get; }
        public int CandidatePool { get; }

        public bool IsValid =>
            DistributionSamples != null &&
            DistributionSampleResolution >= 2 &&
            DistributionSamples.Length ==
                DistributionSampleResolution * DistributionSampleResolution &&
            ProposedPoints != null;

        public static GroundPaintedAccentPlacementDebugSnapshot Empty =>
            new GroundPaintedAccentPlacementDebugSnapshot(
                Array.Empty<GroundPaintedAccentDistributionDebugSample>(),
                0,
                Array.Empty<GroundPaintedAccentProposalDebugPoint>(),
                0,
                0);
    }

    internal static class GroundPaintedAccentSurfaceStrokeGenerator
    {
        public const int Revision = 23;

        private const float MinimumFieldSize = 0.0001f;
        private const int MinimumStrokePointCount = 13;
        private const int MaximumStrokePointCount = 33;
        private const float TargetStoredStrokePointSpacing = 0.03f;
        private const float MaximumWiggleFractionOfLength = 0.075f;
        private const int CandidatePoolMultiplier = 16;
        private const int MaximumTargetStrokeCount = 2000;
        private const int DebugDistributionSampleResolution = 21;
        private const float DebugSurfaceOffset = 0.025f;
        private const int MinimumValidationSampleCount = 13;
        private const float MaximumValidationSpacing = 0.25f;
        private const float RiverSafetyClearance = 0.15f;
        private const float MaximumBroadSlopeDegrees = 45f;
        private const float MaximumLocalGradeDegrees = 40f;
        private const float CompositionRegionJitterFraction = 0.28f;
        private const float QuietRegionProbability = 0.30f;
        private const float SupportingRegionProbability = 0.50f;
        private const float QuietConcentratedWeight = 0.15f;
        private const float SupportingConcentratedWeight = 0.65f;
        private const float AccentConcentratedWeight = 1.45f;
        private const float MaximumRegionalAngleOffsetDegrees = 10f;
        private const float RegionalAngleJitterFraction = 0.25f;
        private const float MinimumBalancedPerMarkJitterFraction = 0.35f;
        private const float MinimumRegionScaleBias = 0.92f;
        private const float MaximumRegionScaleBias = 1.08f;
        private const float MaximumHorizontalCompanionParticipantFraction = 0.94f;
        private const float HorizontalCompanionStrengthExponent = 1.05f;
        private const float MaximumHorizontalCompanionTripletProbability = 0.56f;
        private const float MaximumTripletProbabilityVerticalityBoost = 0.16f;
        private const float MaximumStructuredPairProbability = 0.94f;
        private const float MinimumCompanionLengthRatio = 0.78f;
        private const float MaximumCompanionLengthRatio = 1.00f;
        private const float MinimumTertiaryLengthRatio = 0.68f;
        private const float MaximumTertiaryLengthRatio = 0.92f;
        private const float MinimumCompanionStrengthScale = 0.94f;
        private const float MaximumCompanionStrengthScale = 1.00f;
        private const float MinimumTertiaryStrengthScale = 0.90f;
        private const float MaximumTertiaryStrengthScale = 0.98f;
        private const float MinimumCompanionGap = -0.14f;
        private const float MaximumCompanionGap = 0.22f;
        private const float CompanionEndTaperContactFraction = 0.25f;
        private const float MaximumCompanionAngleDifferenceDegrees = 6f;
        private const float MinimumHorizontalTripletFlatProbability = 0.015f;
        private const float MaximumHorizontalTripletFlatProbability = 0.12f;
        private const float MinimumHorizontalTripletShapeAngleDegrees = 7f;
        private const float MaximumHorizontalTripletMemberJitterDegrees = 1.5f;
        private const float MaximumPairCompanionAngleAllowanceDegrees = 10f;
        private const float MaximumTripletCompanionAngleAllowanceDegrees = 15f;
        private const float AbsoluteCompanionMemberAngleDegrees = 42f;
        private const float MaximumPairShapeAngleDegrees = 10f;
        private const float MaximumTripletShapeAngleDegrees = 15f;
        private const float MaximumPairStepWidthMultiplier = 4.25f;
        private const float MinimumStructuredPairCenterStepLengthFraction =
            0.15f;
        private const float MinimumShallowPairCenterStepLengthFraction =
            0.055f;
        private const float MinimumShallowPairSpanLengthFraction = 0.10f;
        private const float MaximumShallowPairStepWidthMultiplier = 1.75f;
        private const float ShallowPairTranslationLengthFraction = 0.07f;
        private const float StructuredPairTranslationLengthFraction = 0.24f;
        private const float OffsetEchoPairTranslationScale = 0.90f;
        private const float HorizontalPairStructureRepairMargin = 1.08f;
        private const float FirstTripletStructureRepairScale = 1.35f;
        private const float SecondTripletStructureRepairScale = 2.00f;
        private const float MinimumTripletNonLinearityWidthMultiplier = 0f;
        private const float MaximumTripletNonLinearityWidthMultiplier = 2.50f;
        private const float MinimumTripletVerticalSpanWidthMultiplier = 0f;
        private const float MaximumTripletVerticalSpanWidthMultiplier = 7.00f;
        private const float MinimumTripletStepWidthMultiplier = 0f;
        private const float MaximumTripletStepWidthMultiplier = 2.75f;
        private const float MaximumEndpointContactPenetrationWidthFraction = 0.35f;
        private const float MaximumInteriorContactPenetrationWidthFraction = 0.06f;
        private const float MinimumCompanionCellSize = 0.72f;
        private const float CompanionCellSizeLengthMultiplier = 1.65f;
        private const float MinimumCompanionEndClearance = 0.10f;
        private const float MinimumCompanionCorridorPadding = 0.06f;
        private const float MaximumCompanionRelocationLengthMultiplier = 2.00f;
        private const float DebugCompositionSurfaceOffset = 0.055f;

        private enum StrokeRejectionReason
        {
            None,
            Sampling,
            River,
            ModifierExclusion,
            BroadSlope,
            LocalGrade
        }

        private enum HorizontalCompanionTargetFailureReason
        {
            None,
            InvalidPlan,
            PairStructure,
            TripletStructure
        }

        private enum HorizontalCompanionReservationFailureReason
        {
            None,
            EnvelopeCollision,
            OccupiedCell
        }

        private sealed class HorizontalCompanionCompositionAudit
        {
            public int TargetParticipantCount;
            public int FormedParticipantCount;
            public int PrimaryClusterAttempts;
            public int TargetPlanRejected;
            public int NoSecondaryCandidate;
            public int NoTertiaryCandidate;
            public int TripletToPairDowngrade;
            public int CrossRegionCandidateSelection;
            public int PairStructureRepair;
            public int TripletStructureRepair;
            public int EnvelopeCollision;
            public int OccupiedCell;
            public int PairStructureRejected;
            public int TripletStructureRejected;
        }

        private enum HorizontalCompanionArrangement
        {
            Continuation,
            StaggeredEcho,
            OffsetShoulder
        }

        private enum HorizontalCompanionTripletLayout
        {
            SteppedRun,
            CrownRun,
            BrokenTerrace,
            FlatContinuation
        }

        private readonly struct HorizontalCompanionTarget
        {
            public HorizontalCompanionTarget(
                HorizontalCompanionArrangement arrangement,
                Vector2 centerXZ,
                float length,
                float strengthScale,
                float angleOffsetDegrees)
            {
                Arrangement = arrangement;
                CenterXZ = centerXZ;
                Length = length;
                StrengthScale = strengthScale;
                AngleOffsetDegrees = angleOffsetDegrees;
            }

            public HorizontalCompanionArrangement Arrangement { get; }
            public Vector2 CenterXZ { get; }
            public float Length { get; }
            public float StrengthScale { get; }
            public float AngleOffsetDegrees { get; }
        }

        private readonly struct HorizontalCompanionEnvelope
        {
            public HorizontalCompanionEnvelope(
                float horizontalMin,
                float horizontalMax,
                float verticalMin,
                float verticalMax)
            {
                HorizontalMin = horizontalMin;
                HorizontalMax = horizontalMax;
                VerticalMin = verticalMin;
                VerticalMax = verticalMax;
            }

            public float HorizontalMin { get; }
            public float HorizontalMax { get; }
            public float VerticalMin { get; }
            public float VerticalMax { get; }

            public bool Intersects(HorizontalCompanionEnvelope other)
            {
                return HorizontalMin <= other.HorizontalMax &&
                       HorizontalMax >= other.HorizontalMin &&
                       VerticalMin <= other.VerticalMax &&
                       VerticalMax >= other.VerticalMin;
            }
        }

        private readonly struct StrokeCandidate
        {
            public StrokeCandidate(
                int column,
                int row,
                int seed,
                Vector2 centerXZ,
                float priority,
                float patchWeight,
                float semanticSupport,
                float selectionWeight,
                CompositionRegion region)
            {
                Column = column;
                Row = row;
                Seed = seed;
                CenterXZ = centerXZ;
                Priority = priority;
                PatchWeight = patchWeight;
                SemanticSupport = semanticSupport;
                SelectionWeight = selectionWeight;
                Region = region;
            }

            public int Column { get; }
            public int Row { get; }
            public int Seed { get; }
            public Vector2 CenterXZ { get; }
            public float Priority { get; }
            public float PatchWeight { get; }
            public float SemanticSupport { get; }
            public float SelectionWeight { get; }
            public CompositionRegion Region { get; }
        }

        private readonly struct CompositionRegion
        {
            public CompositionRegion(
                long coordinateKey,
                int seed,
                GroundPaintedAccentCompositionRegionMode mode,
                float selectionMultiplier,
                float angleOffsetDegrees,
                float scaleBias)
            {
                CoordinateKey = coordinateKey;
                Seed = seed;
                Mode = mode;
                SelectionMultiplier = selectionMultiplier;
                AngleOffsetDegrees = angleOffsetDegrees;
                ScaleBias = scaleBias;
            }

            public long CoordinateKey { get; }
            public int Seed { get; }
            public GroundPaintedAccentCompositionRegionMode Mode { get; }
            public float SelectionMultiplier { get; }
            public float AngleOffsetDegrees { get; }
            public float ScaleBias { get; }
        }

        private sealed class CompositionCandidate
        {
            public CompositionCandidate(
                StrokeCandidate source,
                CompositionRegion region,
                Vector3 debugLocalPosition,
                bool hasDebugLocalPosition)
            {
                Source = source;
                Region = region;
                DebugLocalPosition = debugLocalPosition;
                HasDebugLocalPosition = hasDebugLocalPosition;
                CenterXZ = source.CenterXZ;
                Role = GroundPaintedAccentCompositionRole.Standard;
                CompanionClusterIndex = -1;
                CompanionMemberRole =
                    GroundPaintedAccentCompanionMemberRole.None;
                IntendedCompanionClusterSize = 0;
                CompanionPairLayout =
                    GroundPaintedAccentCompanionPairLayout.None;
                ProfileSeed = source.Seed;
                StrengthScale = 1f;
                IndependentCenterXZ = source.CenterXZ;
                IndependentProfileSeed = source.Seed;
                IndependentStrengthScale = 1f;
            }

            public StrokeCandidate Source { get; }
            public CompositionRegion Region { get; }
            public Vector3 DebugLocalPosition { get; }
            public bool HasDebugLocalPosition { get; }
            public int ProposalRankQuartile { get; set; }
            public Vector2 CenterXZ { get; set; }
            public GroundPaintedAccentCompositionRole Role { get; set; }
            public GroundPaintedAccentGlyphFamily Family { get; set; }
            public int CompanionClusterIndex { get; set; }
            public GroundPaintedAccentCompanionMemberRole CompanionMemberRole
            {
                get;
                set;
            }
            public int IntendedCompanionClusterSize { get; set; }
            public GroundPaintedAccentCompanionPairLayout CompanionPairLayout
            {
                get;
                set;
            }
            public bool HasCompanionOrientation { get; set; }
            public float CompanionAngleOffsetDegrees { get; set; }
            public int ProfileSeed { get; set; }
            public float StrengthScale { get; set; }
            public float Length { get; set; }
            public float Width { get; set; }
            public float BodyWidth { get; set; }
            public float TotalAngleOffsetDegrees { get; set; }
            public Vector2 Axis { get; set; }
            public Vector2 IndependentCenterXZ { get; set; }
            public GroundPaintedAccentGlyphFamily IndependentFamily { get; set; }
            public int IndependentProfileSeed { get; set; }
            public float IndependentStrengthScale { get; set; }
            public float IndependentLength { get; set; }
            public float IndependentWidth { get; set; }
            public float IndependentBodyWidth { get; set; }
        }

        private sealed class CompositionRegionRuntime
        {
            public CompositionRegionRuntime(CompositionRegion region)
            {
                Region = region;
                Members = new List<CompositionCandidate>();
            }

            public CompositionRegion Region { get; }
            public List<CompositionCandidate> Members { get; }
            public bool IsOccupied { get; set; }
            public Vector3 AcceptedPositionTotal { get; set; }
            public int AcceptedPositionCount { get; set; }
            public int AcceptedNegativeAngleCount { get; set; }
            public int AcceptedPositiveAngleCount { get; set; }
        }

        public static GroundPaintedAccentSurfaceStroke[] GenerateSurfaceStrokes(
            Bounds localBounds,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            int shapeSeed,
            Vector2Int patchCoordinate,
            Vector2 visualHorizontalLocalXZ,
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            out Vector4 originSize,
            out GroundPaintedAccentPlacementDiagnostics diagnostics,
            out GroundPaintedAccentCompositionDebugSnapshot compositionDebugSnapshot)
        {
            originSize = ResolveOriginSize(localBounds);
            FieldSettings settings =
                FieldSettings.Create(
                    feature,
                    shapeSeed,
                    originSize,
                    visualHorizontalLocalXZ);

            return GenerateSurfaceStrokesInternal(
                originSize,
                baseSurface,
                feature,
                settings,
                patchCoordinate,
                rivers,
                modifiers,
                out diagnostics,
                out compositionDebugSnapshot);
        }

        public static GroundPaintedAccentPlacementDebugSnapshot
            BuildPlacementDebugSnapshot(
                Bounds localBounds,
                GroundHeightFieldSnapshot baseSurface,
                GroundSurfaceFeatureRecipe feature,
                int shapeSeed,
                Vector2Int patchCoordinate)
        {
            if (baseSurface == null || !baseSurface.IsValid)
            {
                return GroundPaintedAccentPlacementDebugSnapshot.Empty;
            }

            Vector4 originSize = ResolveOriginSize(localBounds);
            FieldSettings settings =
                FieldSettings.Create(
                    feature,
                    shapeSeed,
                    originSize,
                    Vector2.right);
            List<StrokeCandidate> candidates =
                settings.TargetStrokeCount > 0
                    ? BuildStrokeCandidates(
                        originSize,
                        baseSurface,
                        feature,
                        settings,
                        patchCoordinate,
                        out _,
                        out _)
                    : new List<StrokeCandidate>();

            candidates.Sort(CompareStrokeCandidates);

            int proposedCount =
                Mathf.Min(settings.TargetStrokeCount, candidates.Count);
            List<GroundPaintedAccentProposalDebugPoint> proposedPoints =
                new List<GroundPaintedAccentProposalDebugPoint>(proposedCount);

            for (int index = 0; index < proposedCount; index++)
            {
                StrokeCandidate candidate = candidates[index];
                if (!TryResolveDebugLocalPosition(
                        baseSurface,
                        candidate.CenterXZ,
                        out Vector3 localPosition))
                {
                    continue;
                }

                proposedPoints.Add(
                    new GroundPaintedAccentProposalDebugPoint(
                        localPosition,
                        candidate.PatchWeight,
                        candidate.SelectionWeight));
            }

            float compositionRegionScale =
                ResolveCompositionRegionScale(settings);
            int sampleResolution = DebugDistributionSampleResolution;
            List<GroundPaintedAccentDistributionDebugSample>
                distributionSamples =
                    new List<GroundPaintedAccentDistributionDebugSample>(
                        sampleResolution * sampleResolution);

            for (int z = 0; z < sampleResolution; z++)
            {
                float v =
                    sampleResolution > 1
                        ? z / (float)(sampleResolution - 1)
                        : 0.5f;

                for (int x = 0; x < sampleResolution; x++)
                {
                    float u =
                        sampleResolution > 1
                            ? x / (float)(sampleResolution - 1)
                            : 0.5f;
                    Vector2 localXZ =
                        new Vector2(
                            originSize.x + u * originSize.z,
                            originSize.y + v * originSize.w);

                    bool hasSurfacePosition =
                        TryResolveDebugLocalPosition(
                            baseSurface,
                            localXZ,
                            out Vector3 localPosition);

                    if (!hasSurfacePosition)
                    {
                        distributionSamples.Add(
                            new GroundPaintedAccentDistributionDebugSample(
                                localPosition,
                                0f,
                                0f,
                                false));
                        continue;
                    }

                    Vector2 distributionSamplePosition =
                        ResolveDistributionSamplePosition(
                            localXZ,
                            patchCoordinate,
                            originSize);
                    float patchWeight =
                        ResolveDistributionPatchWeight(
                            distributionSamplePosition,
                            settings.DistributionPatchScale,
                            settings.DistributionPatchiness,
                            settings.DistributionSparseFloor,
                            settings.Seed);
                    float semanticSupport =
                        ResolveSemanticSupport(
                            baseSurface,
                            localXZ,
                            feature != null ? feature.MaskInfluence : 0f);
                    float semanticWeight =
                        Mathf.Lerp(0.45f, 1f, semanticSupport);
                    CompositionRegion region =
                        ResolveCompositionRegion(
                            distributionSamplePosition,
                            settings,
                            compositionRegionScale);
                    float effectiveProposalWeight =
                        Mathf.Clamp01(
                            patchWeight *
                            semanticWeight *
                            region.SelectionMultiplier);

                    distributionSamples.Add(
                        new GroundPaintedAccentDistributionDebugSample(
                            localPosition,
                            patchWeight,
                            effectiveProposalWeight,
                            true));
                }
            }

            return new GroundPaintedAccentPlacementDebugSnapshot(
                distributionSamples.ToArray(),
                sampleResolution,
                proposedPoints.ToArray(),
                settings.TargetStrokeCount,
                candidates.Count);
        }

        private static Vector4 ResolveOriginSize(Bounds localBounds)
        {
            return new Vector4(
                localBounds.min.x,
                localBounds.min.z,
                Mathf.Max(MinimumFieldSize, localBounds.size.x),
                Mathf.Max(MinimumFieldSize, localBounds.size.z));
        }

        private static GroundPaintedAccentSurfaceStroke[] GenerateSurfaceStrokesInternal(
            Vector4 originSize,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            FieldSettings settings,
            Vector2Int patchCoordinate,
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            out GroundPaintedAccentPlacementDiagnostics diagnostics,
            out GroundPaintedAccentCompositionDebugSnapshot compositionDebugSnapshot)
        {
            diagnostics = GroundPaintedAccentPlacementDiagnostics.Empty;
            compositionDebugSnapshot =
                GroundPaintedAccentCompositionDebugSnapshot.Empty;

            if (baseSurface == null ||
                !baseSurface.IsValid ||
                settings.TargetStrokeCount <= 0)
            {
                return Array.Empty<GroundPaintedAccentSurfaceStroke>();
            }

            List<StrokeCandidate> candidates =
                BuildStrokeCandidates(
                    originSize,
                    baseSurface,
                    feature,
                    settings,
                    patchCoordinate,
                    out double candidateBuildMilliseconds,
                    out double regionalWeightingMilliseconds);
            long candidateOrderingStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
            candidates.Sort(CompareStrokeCandidates);
            double candidateOrderingMilliseconds =
                ResolveElapsedMilliseconds(candidateOrderingStartedAt);

            int proposed =
                Mathf.Min(settings.TargetStrokeCount, candidates.Count);
            float compositionRegionScale =
                ResolveCompositionRegionScale(settings);
            Dictionary<long, CompositionRegionRuntime> regions =
                new Dictionary<long, CompositionRegionRuntime>();
            List<CompositionCandidate> compositionCandidates =
                new List<CompositionCandidate>(proposed);

            float minimumProposalPatchWeight = float.PositiveInfinity;
            float maximumProposalPatchWeight = 0f;
            double totalProposalPatchWeight = 0d;
            int quietProposalCount = 0;
            int supportingProposalCount = 0;
            int accentProposalCount = 0;
            int[] selectedByRankQuartile = new int[4];
            int[] acceptedByRankQuartile = new int[4];
            List<int> horizontalCompanionClusterSizes = new List<int>();
            long compositionSetupStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();

            for (int proposalIndex = 0;
                 proposalIndex < proposed;
                 proposalIndex++)
            {
                StrokeCandidate candidate = candidates[proposalIndex];
                minimumProposalPatchWeight =
                    Mathf.Min(minimumProposalPatchWeight, candidate.PatchWeight);
                maximumProposalPatchWeight =
                    Mathf.Max(maximumProposalPatchWeight, candidate.PatchWeight);
                totalProposalPatchWeight += candidate.PatchWeight;

                CompositionRegion region = candidate.Region;

                if (!regions.TryGetValue(
                        region.CoordinateKey,
                        out CompositionRegionRuntime regionRuntime))
                {
                    regionRuntime = new CompositionRegionRuntime(region);
                    regions.Add(region.CoordinateKey, regionRuntime);
                }

                switch (region.Mode)
                {
                    case GroundPaintedAccentCompositionRegionMode.Quiet:
                        quietProposalCount++;
                        break;
                    case GroundPaintedAccentCompositionRegionMode.Accent:
                        accentProposalCount++;
                        break;
                    case GroundPaintedAccentCompositionRegionMode.Supporting:
                    default:
                        supportingProposalCount++;
                        break;
                }

                bool hasDebugLocalPosition =
                    TryResolveDebugLocalPosition(
                        baseSurface,
                        candidate.CenterXZ,
                        out Vector3 debugLocalPosition);
                if (hasDebugLocalPosition)
                {
                    debugLocalPosition.y +=
                        DebugCompositionSurfaceOffset - DebugSurfaceOffset;
                }

                CompositionCandidate compositionCandidate =
                    new CompositionCandidate(
                        candidate,
                        region,
                        debugLocalPosition,
                        hasDebugLocalPosition);
                int rankQuartile =
                    proposed > 0
                        ? Mathf.Min(3, proposalIndex * 4 / proposed)
                        : 0;
                compositionCandidate.ProposalRankQuartile = rankQuartile;
                selectedByRankQuartile[rankQuartile]++;
                compositionCandidates.Add(compositionCandidate);
                regionRuntime.Members.Add(compositionCandidate);
            }

            List<CompositionCandidate> selectedCandidates =
                new List<CompositionCandidate>(proposed);
            foreach (KeyValuePair<long, CompositionRegionRuntime> pair in regions)
            {
                CompositionRegionRuntime regionRuntime = pair.Value;
                regionRuntime.Members.Sort(
                    CompareCompositionCandidatesBySource);
                for (int rank = 0; rank < regionRuntime.Members.Count; rank++)
                {
                    CompositionCandidate candidate =
                        regionRuntime.Members[rank];
                    candidate.Role = ResolveCompositionRole(candidate, rank);
                    ResolveCompositionCandidateGeometry(candidate, settings);
                    selectedCandidates.Add(candidate);
                }
            }

            double compositionSetupMilliseconds =
                ResolveElapsedMilliseconds(compositionSetupStartedAt);

            // V3J.4F3: companion ownership moved to the final valid projected
            // prototype pool. Surface generation now remains purely independent
            // so authoring quotas are resolved against marks that have already
            // passed ordinary surface and projected validation.
            int horizontalCompanionPairShallowLayoutCount = 0;
            int horizontalCompanionPairSteppedLayoutCount = 0;
            int horizontalCompanionPairShoulderLayoutCount = 0;
            int horizontalCompanionPairOffsetLayoutCount = 0;
            HorizontalCompanionCompositionAudit
                horizontalCompanionCompositionAudit =
                    new HorizontalCompanionCompositionAudit();
            int horizontalCompanionClusterCount =
                horizontalCompanionClusterSizes.Count;
            int horizontalCompanionPairClusterCount = 0;
            int horizontalCompanionTripletClusterCount = 0;
            int horizontalCompanionParticipantCount = 0;
            for (int clusterIndex = 0;
                 clusterIndex < horizontalCompanionClusterSizes.Count;
                 clusterIndex++)
            {
                int clusterSize =
                    horizontalCompanionClusterSizes[clusterIndex];
                horizontalCompanionParticipantCount += clusterSize;
                if (clusterSize >= 3)
                {
                    horizontalCompanionTripletClusterCount++;
                }
                else
                {
                    horizontalCompanionPairClusterCount++;
                }
            }
            selectedCandidates.Sort(CompareCompositionCandidatesBySource);
            List<GroundPaintedAccentSurfaceStroke> strokes =
                new List<GroundPaintedAccentSurfaceStroke>(
                    selectedCandidates.Count);
            List<GroundPaintedAccentCompositionMarkDebugPoint>
                acceptedMarkDebugPoints =
                    new List<GroundPaintedAccentCompositionMarkDebugPoint>(
                        selectedCandidates.Count);
            int rejectedSampling = 0;
            int rejectedRiver = 0;
            int rejectedModifierExclusion = 0;
            int rejectedBroadSlope = 0;
            int rejectedLocalGrade = 0;
            int quietAcceptedCount = 0;
            int supportingAcceptedCount = 0;
            int accentAcceptedCount = 0;
            int dominantAcceptedCount = 0;
            int standardAcceptedCount = 0;
            int supportAcceptedCount = 0;
            int completeMoundSelectedCount = 0;
            int asymmetricMoundSelectedCount = 0;
            int singleShoulderSelectedCount = 0;
            int shallowCrestSelectedCount = 0;
            int completeMoundAcceptedCount = 0;
            int asymmetricMoundAcceptedCount = 0;
            int singleShoulderAcceptedCount = 0;
            int shallowCrestAcceptedCount = 0;
            int horizontalCompanionAcceptedParticipantCount = 0;
            int horizontalCompanionSourceMemberPhysicalFallback = 0;
            int[] horizontalCompanionAcceptedMembers =
                horizontalCompanionClusterCount > 0
                    ? new int[horizontalCompanionClusterCount]
                    : Array.Empty<int>();
            float acceptedLengthMin = float.PositiveInfinity;
            float acceptedLengthMax = 0f;
            double acceptedLengthTotal = 0d;
            float acceptedAngleOffsetMin = float.PositiveInfinity;
            float acceptedAngleOffsetMax = float.NegativeInfinity;
            double acceptedAngleOffsetTotal = 0d;
            int acceptedCompanionAngleOffsetCount = 0;
            float acceptedCompanionAngleOffsetMin = float.PositiveInfinity;
            float acceptedCompanionAngleOffsetMax = float.NegativeInfinity;
            double acceptedCompanionAngleOffsetTotal = 0d;
            long strokeSetupTicks = 0L;
            long surfaceConstructionValidationTicks = 0L;

            for (int index = 0;
                 index < selectedCandidates.Count;
                 index++)
            {
                CompositionCandidate candidate = selectedCandidates[index];
                long strokeSetupStartedAt =
                    System.Diagnostics.Stopwatch.GetTimestamp();
                IncrementFamilyCount(
                    candidate.Family,
                    ref completeMoundSelectedCount,
                    ref asymmetricMoundSelectedCount,
                    ref singleShoulderSelectedCount,
                    ref shallowCrestSelectedCount);
                CompositionRegionRuntime regionRuntime =
                    regions[candidate.Region.CoordinateKey];
                ResolveCompositionCandidateOrientation(
                    candidate,
                    settings,
                    regionRuntime);
                uint strokeHash = (uint)candidate.Source.Seed;
                float curvature =
                    candidate.Length *
                    MaximumWiggleFractionOfLength *
                    settings.StrokePathWiggle *
                    (Hash01(strokeHash, 67u) * 2f - 1f);
                float strength =
                    settings.Strength *
                    Mathf.Lerp(
                        0.78f,
                        1.0f,
                        candidate.Source.SemanticSupport) *
                    Mathf.Lerp(0.80f, 1.0f, Hash01(strokeHash, 71u)) *
                    candidate.StrengthScale;
                strokeSetupTicks +=
                    System.Diagnostics.Stopwatch.GetTimestamp() -
                    strokeSetupStartedAt;

                GroundPaintedAccentSurfaceStroke independentFallback =
                    default;
                if (candidate.CompanionClusterIndex >= 0)
                {
                    TryCreateIndependentFallbackStroke(
                        candidate,
                        settings,
                        baseSurface,
                        rivers,
                        modifiers,
                        out independentFallback);
                }

                long surfaceValidationStartedAt =
                    System.Diagnostics.Stopwatch.GetTimestamp();
                bool composedStrokeCreated =
                    TryCreateSurfaceStroke(
                        candidate.CenterXZ,
                        candidate.Axis,
                        candidate.Length,
                        candidate.Width,
                        candidate.BodyWidth,
                        curvature,
                        strength,
                        candidate.Length,
                        candidate.Family,
                        candidate.Source.Seed,
                        candidate.ProfileSeed,
                        baseSurface,
                        rivers,
                        modifiers,
                        out GroundPaintedAccentSurfaceStroke stroke,
                        out StrokeRejectionReason rejectionReason);
                surfaceConstructionValidationTicks +=
                    System.Diagnostics.Stopwatch.GetTimestamp() -
                    surfaceValidationStartedAt;
                if (!composedStrokeCreated && independentFallback.IsValid)
                {
                    horizontalCompanionSourceMemberPhysicalFallback++;
                    stroke = independentFallback;
                    rejectionReason = StrokeRejectionReason.None;
                }

                if (!stroke.IsValid)
                {
                    switch (rejectionReason)
                    {
                        case StrokeRejectionReason.River:
                            rejectedRiver++;
                            break;
                        case StrokeRejectionReason.ModifierExclusion:
                            rejectedModifierExclusion++;
                            break;
                        case StrokeRejectionReason.BroadSlope:
                            rejectedBroadSlope++;
                            break;
                        case StrokeRejectionReason.LocalGrade:
                            rejectedLocalGrade++;
                            break;
                        case StrokeRejectionReason.Sampling:
                        default:
                            rejectedSampling++;
                            break;
                    }

                    continue;
                }

                stroke = stroke.WithPlacementMetadata(
                    candidate.ProposalRankQuartile,
                    candidate.Region.Mode);

                if (composedStrokeCreated &&
                    candidate.CompanionClusterIndex >= 0)
                {
                    stroke = stroke.WithCompanionMetadata(
                        candidate.CompanionClusterIndex,
                        candidate.CompanionMemberRole,
                        candidate.IntendedCompanionClusterSize,
                        candidate.CompanionPairLayout,
                        independentFallback.ToVariant());
                }

                strokes.Add(stroke);
                acceptedByRankQuartile[
                    Mathf.Clamp(candidate.ProposalRankQuartile, 0, 3)]++;
                if (stroke.IsCompanionMember &&
                    stroke.CompanionClusterIndex <
                        horizontalCompanionAcceptedMembers.Length)
                {
                    horizontalCompanionAcceptedParticipantCount++;
                    horizontalCompanionAcceptedMembers[
                        stroke.CompanionClusterIndex]++;
                }

                regionRuntime.IsOccupied = true;
                Vector3 acceptedPosition =
                    stroke.LocalPoints[stroke.LocalPoints.Length / 2];
                regionRuntime.AcceptedPositionTotal += acceptedPosition;
                regionRuntime.AcceptedPositionCount++;
                if (candidate.TotalAngleOffsetDegrees < -0.0001f)
                {
                    regionRuntime.AcceptedNegativeAngleCount++;
                }
                else if (candidate.TotalAngleOffsetDegrees > 0.0001f)
                {
                    regionRuntime.AcceptedPositiveAngleCount++;
                }

                acceptedPosition.y += DebugCompositionSurfaceOffset;
                acceptedMarkDebugPoints.Add(
                    new GroundPaintedAccentCompositionMarkDebugPoint(
                        acceptedPosition,
                        candidate.Axis,
                        candidate.Region.Mode,
                        candidate.Role,
                        stroke.Family,
                        stroke.AuthoredLength));

                switch (candidate.Region.Mode)
                {
                    case GroundPaintedAccentCompositionRegionMode.Quiet:
                        quietAcceptedCount++;
                        break;
                    case GroundPaintedAccentCompositionRegionMode.Accent:
                        accentAcceptedCount++;
                        break;
                    case GroundPaintedAccentCompositionRegionMode.Supporting:
                    default:
                        supportingAcceptedCount++;
                        break;
                }

                switch (candidate.Role)
                {
                    case GroundPaintedAccentCompositionRole.Dominant:
                        dominantAcceptedCount++;
                        break;
                    case GroundPaintedAccentCompositionRole.Support:
                        supportAcceptedCount++;
                        break;
                    case GroundPaintedAccentCompositionRole.Standard:
                    default:
                        standardAcceptedCount++;
                        break;
                }

                IncrementFamilyCount(
                    stroke.Family,
                    ref completeMoundAcceptedCount,
                    ref asymmetricMoundAcceptedCount,
                    ref singleShoulderAcceptedCount,
                    ref shallowCrestAcceptedCount);

                acceptedLengthMin =
                    Mathf.Min(acceptedLengthMin, stroke.AuthoredLength);
                acceptedLengthMax =
                    Mathf.Max(acceptedLengthMax, stroke.AuthoredLength);
                acceptedLengthTotal += stroke.AuthoredLength;
                acceptedAngleOffsetMin =
                    Mathf.Min(
                        acceptedAngleOffsetMin,
                        candidate.TotalAngleOffsetDegrees);
                acceptedAngleOffsetMax =
                    Mathf.Max(
                        acceptedAngleOffsetMax,
                        candidate.TotalAngleOffsetDegrees);
                acceptedAngleOffsetTotal +=
                    candidate.TotalAngleOffsetDegrees;
                if (stroke.IsCompanionMember)
                {
                    acceptedCompanionAngleOffsetCount++;
                    acceptedCompanionAngleOffsetMin =
                        Mathf.Min(
                            acceptedCompanionAngleOffsetMin,
                            candidate.TotalAngleOffsetDegrees);
                    acceptedCompanionAngleOffsetMax =
                        Mathf.Max(
                            acceptedCompanionAngleOffsetMax,
                            candidate.TotalAngleOffsetDegrees);
                    acceptedCompanionAngleOffsetTotal +=
                        candidate.TotalAngleOffsetDegrees;
                }
            }

            long diagnosticsStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
            int quietRegionCount = 0;
            int supportingRegionCount = 0;
            int accentRegionCount = 0;
            List<GroundPaintedAccentCompositionRegionDebug> regionDebugPoints =
                new List<GroundPaintedAccentCompositionRegionDebug>(
                    regions.Count);

            foreach (KeyValuePair<long, CompositionRegionRuntime> pair in regions)
            {
                CompositionRegionRuntime regionRuntime = pair.Value;
                switch (regionRuntime.Region.Mode)
                {
                    case GroundPaintedAccentCompositionRegionMode.Quiet:
                        quietRegionCount++;
                        break;
                    case GroundPaintedAccentCompositionRegionMode.Accent:
                        accentRegionCount++;
                        break;
                    case GroundPaintedAccentCompositionRegionMode.Supporting:
                    default:
                        supportingRegionCount++;
                        break;
                }

                if (!regionRuntime.IsOccupied ||
                    regionRuntime.AcceptedPositionCount <= 0)
                {
                    continue;
                }

                Vector3 regionPosition =
                    regionRuntime.AcceptedPositionTotal /
                    regionRuntime.AcceptedPositionCount;
                regionPosition.y += DebugCompositionSurfaceOffset;
                regionDebugPoints.Add(
                    new GroundPaintedAccentCompositionRegionDebug(
                        regionPosition,
                        ResolveStrokeAxis(
                            settings,
                            regionRuntime.Region.AngleOffsetDegrees),
                        regionRuntime.Region.Mode,
                        regionRuntime.Region.SelectionMultiplier,
                        true));
            }

            List<GroundPaintedAccentCompositionProposalDebugPoint>
                proposalDebugPoints =
                    new List<GroundPaintedAccentCompositionProposalDebugPoint>(
                        compositionCandidates.Count);
            for (int index = 0;
                 index < compositionCandidates.Count;
                 index++)
            {
                CompositionCandidate candidate = compositionCandidates[index];
                if (!candidate.HasDebugLocalPosition)
                {
                    continue;
                }

                proposalDebugPoints.Add(
                    new GroundPaintedAccentCompositionProposalDebugPoint(
                        candidate.DebugLocalPosition,
                        candidate.Region.Mode));
            }

            compositionDebugSnapshot =
                new GroundPaintedAccentCompositionDebugSnapshot(
                    proposalDebugPoints.ToArray(),
                    regionDebugPoints.ToArray(),
                    acceptedMarkDebugPoints.ToArray());

            int horizontalCompanionAcceptedClusterCount = 0;
            int horizontalCompanionSourceClusterIncomplete = 0;
            for (int clusterIndex = 0;
                 clusterIndex < horizontalCompanionAcceptedMembers.Length;
                 clusterIndex++)
            {
                if (horizontalCompanionAcceptedMembers[clusterIndex] >=
                    horizontalCompanionClusterSizes[clusterIndex])
                {
                    horizontalCompanionAcceptedClusterCount++;
                }
                else
                {
                    horizontalCompanionSourceClusterIncomplete++;
                }
            }

            int acceptedCount = strokes.Count;
            float proposalPatchWeightMin =
                proposed > 0 ? minimumProposalPatchWeight : 0f;
            float proposalPatchWeightMean =
                proposed > 0
                    ? (float)(totalProposalPatchWeight / proposed)
                    : 0f;
            float acceptedLengthMean =
                acceptedCount > 0
                    ? (float)(acceptedLengthTotal / acceptedCount)
                    : 0f;
            float acceptedAngleOffsetMean =
                acceptedCount > 0
                    ? (float)(acceptedAngleOffsetTotal / acceptedCount)
                    : 0f;
            float acceptedCompanionAngleOffsetMean =
                acceptedCompanionAngleOffsetCount > 0
                    ? (float)(
                        acceptedCompanionAngleOffsetTotal /
                        acceptedCompanionAngleOffsetCount)
                    : 0f;

            diagnostics =
                new GroundPaintedAccentPlacementDiagnostics(
                    settings.TargetStrokeCount,
                    candidates.Count,
                    proposed,
                    selectedCandidates.Count,
                    acceptedCount,
                    settings.DistributionPatchScale,
                    settings.DistributionPatchiness,
                    settings.DistributionSparseFloor,
                    proposalPatchWeightMin,
                    proposalPatchWeightMean,
                    proposed > 0 ? maximumProposalPatchWeight : 0f,
                    rejectedSampling,
                    rejectedRiver,
                    rejectedModifierExclusion,
                    rejectedBroadSlope,
                    rejectedLocalGrade,
                    horizontalCompanionClusterCount,
                    horizontalCompanionPairClusterCount,
                    horizontalCompanionTripletClusterCount,
                    horizontalCompanionPairShallowLayoutCount,
                    horizontalCompanionPairSteppedLayoutCount,
                    horizontalCompanionPairShoulderLayoutCount,
                    horizontalCompanionPairOffsetLayoutCount,
                    horizontalCompanionParticipantCount,
                    horizontalCompanionAcceptedClusterCount,
                    horizontalCompanionAcceptedParticipantCount,
                    horizontalCompanionCompositionAudit.TargetParticipantCount,
                    horizontalCompanionCompositionAudit.FormedParticipantCount,
                    horizontalCompanionCompositionAudit.PrimaryClusterAttempts,
                    horizontalCompanionCompositionAudit.TargetPlanRejected,
                    horizontalCompanionCompositionAudit.NoSecondaryCandidate,
                    horizontalCompanionCompositionAudit.NoTertiaryCandidate,
                    horizontalCompanionCompositionAudit.TripletToPairDowngrade,
                    horizontalCompanionCompositionAudit.CrossRegionCandidateSelection,
                    horizontalCompanionCompositionAudit.PairStructureRepair,
                    horizontalCompanionCompositionAudit.TripletStructureRepair,
                    horizontalCompanionCompositionAudit.EnvelopeCollision,
                    horizontalCompanionCompositionAudit.OccupiedCell,
                    horizontalCompanionCompositionAudit.PairStructureRejected,
                    horizontalCompanionCompositionAudit.TripletStructureRejected,
                    horizontalCompanionSourceMemberPhysicalFallback,
                    horizontalCompanionSourceClusterIncomplete,
                    regions.Count,
                    quietRegionCount,
                    supportingRegionCount,
                    accentRegionCount,
                    quietProposalCount,
                    supportingProposalCount,
                    accentProposalCount,
                    quietAcceptedCount,
                    supportingAcceptedCount,
                    accentAcceptedCount,
                    dominantAcceptedCount,
                    standardAcceptedCount,
                    supportAcceptedCount,
                    completeMoundSelectedCount,
                    asymmetricMoundSelectedCount,
                    singleShoulderSelectedCount,
                    shallowCrestSelectedCount,
                    completeMoundAcceptedCount,
                    asymmetricMoundAcceptedCount,
                    singleShoulderAcceptedCount,
                    shallowCrestAcceptedCount,
                    compositionRegionScale,
                    acceptedCount > 0 ? acceptedLengthMin : 0f,
                    acceptedLengthMean,
                    acceptedCount > 0 ? acceptedLengthMax : 0f,
                    acceptedCount > 0 ? acceptedAngleOffsetMin : 0f,
                    acceptedAngleOffsetMean,
                    acceptedCount > 0 ? acceptedAngleOffsetMax : 0f,
                    acceptedCompanionAngleOffsetCount > 0
                        ? acceptedCompanionAngleOffsetMin
                        : 0f,
                    acceptedCompanionAngleOffsetMean,
                    acceptedCompanionAngleOffsetCount > 0
                        ? acceptedCompanionAngleOffsetMax
                        : 0f,
                    new GroundPaintedAccentProposalRankDiagnostics(
                        new Vector4(
                            selectedByRankQuartile[0],
                            selectedByRankQuartile[1],
                            selectedByRankQuartile[2],
                            selectedByRankQuartile[3]),
                        new Vector4(
                            acceptedByRankQuartile[0],
                            acceptedByRankQuartile[1],
                            acceptedByRankQuartile[2],
                            acceptedByRankQuartile[3])),
                    new GroundPaintedAccentSurfaceStrokeBuildTimings(
                        candidateBuildMilliseconds,
                        regionalWeightingMilliseconds,
                        candidateOrderingMilliseconds,
                        compositionSetupMilliseconds,
                        strokeSetupTicks * 1000d /
                            System.Diagnostics.Stopwatch.Frequency,
                        surfaceConstructionValidationTicks * 1000d /
                            System.Diagnostics.Stopwatch.Frequency,
                        ResolveElapsedMilliseconds(diagnosticsStartedAt)));

            return strokes.ToArray();
        }

        private static float ResolveCompositionRegionScale(
            FieldSettings settings)
        {
            return settings.CompositionRegionScale;
        }

        private static CompositionRegion ResolveCompositionRegion(
            Vector2 distributionPosition,
            FieldSettings settings,
            float regionScale)
        {
            float safeScale = Mathf.Max(0.001f, regionScale);
            int baseCellX = Mathf.FloorToInt(distributionPosition.x / safeScale);
            int baseCellZ = Mathf.FloorToInt(distributionPosition.y / safeScale);
            int bestCellX = baseCellX;
            int bestCellZ = baseCellZ;
            float bestDistanceSquared = float.PositiveInfinity;

            for (int offsetZ = -1; offsetZ <= 1; offsetZ++)
            {
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    int cellX = baseCellX + offsetX;
                    int cellZ = baseCellZ + offsetZ;
                    int centerSeed =
                        Hash(cellX, cellZ, settings.Seed ^ 0x45C7);
                    float jitterX =
                        (Hash01((uint)centerSeed, 191u) * 2f - 1f) *
                        CompositionRegionJitterFraction;
                    float jitterZ =
                        (Hash01((uint)centerSeed, 193u) * 2f - 1f) *
                        CompositionRegionJitterFraction;
                    Vector2 center =
                        new Vector2(
                            (cellX + 0.5f + jitterX) * safeScale,
                            (cellZ + 0.5f + jitterZ) * safeScale);
                    float distanceSquared =
                        (distributionPosition - center).sqrMagnitude;

                    if (distanceSquared < bestDistanceSquared)
                    {
                        bestDistanceSquared = distanceSquared;
                        bestCellX = cellX;
                        bestCellZ = cellZ;
                    }
                }
            }

            int regionSeed =
                Hash(bestCellX, bestCellZ, settings.Seed ^ 0x73A1);
            float modeRoll = Hash01((uint)regionSeed, 211u);
            GroundPaintedAccentCompositionRegionMode mode;

            if (modeRoll < QuietRegionProbability)
            {
                mode = GroundPaintedAccentCompositionRegionMode.Quiet;
            }
            else if (modeRoll <
                     QuietRegionProbability + SupportingRegionProbability)
            {
                mode = GroundPaintedAccentCompositionRegionMode.Supporting;
            }
            else
            {
                mode = GroundPaintedAccentCompositionRegionMode.Accent;
            }

            float selectionMultiplier =
                ResolveRegionalSelectionMultiplier(
                    mode,
                    settings.CompositionDensityContrast);
            float regionalAngleMaximum =
                Mathf.Min(
                    MaximumRegionalAngleOffsetDegrees,
                    Mathf.Max(
                        0f,
                        settings.AngleJitterDegrees *
                        RegionalAngleJitterFraction));
            float angleOffsetDegrees =
                (Hash01((uint)regionSeed, 227u) * 2f - 1f) *
                regionalAngleMaximum;
            float scaleBias =
                Mathf.Lerp(
                    MinimumRegionScaleBias,
                    MaximumRegionScaleBias,
                    Hash01((uint)regionSeed, 229u));
            long coordinateKey =
                ((long)(uint)bestCellX << 32) |
                (uint)bestCellZ;
            return new CompositionRegion(
                coordinateKey,
                regionSeed,
                mode,
                selectionMultiplier,
                angleOffsetDegrees,
                scaleBias);
        }

        private static float ResolveRegionalSelectionMultiplier(
            GroundPaintedAccentCompositionRegionMode mode,
            float densityContrast)
        {
            float contrast = Mathf.Clamp01(densityContrast);
            float quietWeight =
                Mathf.Lerp(1f, QuietConcentratedWeight, contrast);
            float supportingWeight =
                Mathf.Lerp(1f, SupportingConcentratedWeight, contrast);
            float accentWeight =
                Mathf.Lerp(1f, AccentConcentratedWeight, contrast);
            float accentProbability =
                Mathf.Max(
                    0f,
                    1f - QuietRegionProbability -
                    SupportingRegionProbability);
            float expectedWeight =
                QuietRegionProbability * quietWeight +
                SupportingRegionProbability * supportingWeight +
                accentProbability * accentWeight;
            float selectedWeight;

            switch (mode)
            {
                case GroundPaintedAccentCompositionRegionMode.Quiet:
                    selectedWeight = quietWeight;
                    break;
                case GroundPaintedAccentCompositionRegionMode.Accent:
                    selectedWeight = accentWeight;
                    break;
                case GroundPaintedAccentCompositionRegionMode.Supporting:
                default:
                    selectedWeight = supportingWeight;
                    break;
            }

            return
                selectedWeight /
                Mathf.Max(0.0001f, expectedWeight);
        }

        private static GroundPaintedAccentCompositionRole
            ResolveCompositionRole(
                CompositionCandidate candidate,
                int rankInRegion)
        {
            if (candidate.Region.Mode ==
                    GroundPaintedAccentCompositionRegionMode.Accent &&
                rankInRegion == 0)
            {
                return GroundPaintedAccentCompositionRole.Dominant;
            }

            uint roleHash =
                (uint)Hash(
                    candidate.Source.Seed,
                    candidate.Region.Seed,
                    313 + rankInRegion);
            float supportRoll = Hash01(roleHash, 317u);
            return supportRoll < 0.42f
                ? GroundPaintedAccentCompositionRole.Support
                : GroundPaintedAccentCompositionRole.Standard;
        }

        private static void ResolveCompositionCandidateGeometry(
            CompositionCandidate candidate,
            FieldSettings settings)
        {
            uint strokeHash = (uint)candidate.Source.Seed;
            float normalizedLength;
            float widthScale;

            switch (candidate.Role)
            {
                case GroundPaintedAccentCompositionRole.Dominant:
                    normalizedLength =
                        Mathf.Lerp(0.72f, 1.00f, Hash01(strokeHash, 53u));
                    widthScale =
                        Mathf.Lerp(0.90f, 1.05f, Hash01(strokeHash, 319u));
                    break;
                case GroundPaintedAccentCompositionRole.Support:
                    normalizedLength =
                        Mathf.Lerp(0.00f, 0.38f, Hash01(strokeHash, 53u));
                    widthScale =
                        Mathf.Lerp(0.80f, 0.95f, Hash01(strokeHash, 319u));
                    break;
                case GroundPaintedAccentCompositionRole.Standard:
                default:
                    normalizedLength =
                        Mathf.Lerp(0.28f, 0.78f, Hash01(strokeHash, 53u));
                    widthScale =
                        Mathf.Lerp(0.90f, 1.00f, Hash01(strokeHash, 319u));
                    break;
            }

            candidate.Family =
                ResolveGlyphFamily(
                    settings.GlyphFamilyWeights,
                    strokeHash);

            float regionalLengthOffset =
                (Mathf.InverseLerp(
                     MinimumRegionScaleBias,
                     MaximumRegionScaleBias,
                     candidate.Region.ScaleBias) - 0.5f) *
                0.14f;
            normalizedLength =
                Mathf.Clamp01(normalizedLength + regionalLengthOffset);
            candidate.Length =
                Mathf.Lerp(
                    settings.StrokeLengthMin,
                    settings.StrokeLengthMax,
                    normalizedLength);
            candidate.Width =
                settings.StrokeWidthWorld *
                Mathf.Lerp(0.84f, 1.18f, Hash01(strokeHash, 59u)) *
                widthScale;
            float bodyScale =
                Mathf.Lerp(0.82f, 1.08f, normalizedLength);
            candidate.BodyWidth =
                Mathf.Max(
                    candidate.Width * 2.75f,
                    settings.BodyWidthWorld *
                    Mathf.Lerp(0.82f, 1.18f, Hash01(strokeHash, 61u)) *
                    bodyScale);

            candidate.IndependentCenterXZ = candidate.CenterXZ;
            candidate.IndependentFamily = candidate.Family;
            candidate.IndependentProfileSeed = candidate.ProfileSeed;
            candidate.IndependentStrengthScale = candidate.StrengthScale;
            candidate.IndependentLength = candidate.Length;
            candidate.IndependentWidth = candidate.Width;
            candidate.IndependentBodyWidth = candidate.BodyWidth;
        }

        private static void ComposeHorizontalCompanions(
            List<CompositionCandidate> candidates,
            FieldSettings settings,
            List<int> clusterSizes,
            out int pairShallowLayoutCount,
            out int pairSteppedLayoutCount,
            out int pairShoulderLayoutCount,
            out int pairOffsetLayoutCount,
            out HorizontalCompanionCompositionAudit audit)
        {
            pairShallowLayoutCount = 0;
            pairSteppedLayoutCount = 0;
            pairShoulderLayoutCount = 0;
            pairOffsetLayoutCount = 0;
            audit = new HorizontalCompanionCompositionAudit();
            if (candidates == null ||
                clusterSizes == null ||
                candidates.Count < 2 ||
                settings.HorizontalCompanionStrength <= 0.0001f)
            {
                return;
            }

            float effectiveStrength =
                Mathf.Pow(
                    Mathf.Clamp01(settings.HorizontalCompanionStrength),
                    HorizontalCompanionStrengthExponent);
            float desiredParticipantCount =
                candidates.Count *
                MaximumHorizontalCompanionParticipantFraction *
                effectiveStrength;
            int targetParticipantCount =
                Mathf.FloorToInt(desiredParticipantCount);
            float fractionalParticipant =
                desiredParticipantCount - targetParticipantCount;
            uint globalHash =
                (uint)Hash(settings.Seed, candidates.Count, 367);
            if (Hash01(globalHash, 373u) < fractionalParticipant)
            {
                targetParticipantCount++;
            }

            int maximumParticipantCount =
                Mathf.FloorToInt(
                    candidates.Count *
                    MaximumHorizontalCompanionParticipantFraction);
            targetParticipantCount =
                Mathf.Clamp(
                    targetParticipantCount,
                    0,
                    maximumParticipantCount);
            audit.TargetParticipantCount = targetParticipantCount;
            if (targetParticipantCount < 2)
            {
                return;
            }

            List<CompositionCandidate> available =
                new List<CompositionCandidate>(candidates);
            available.Sort(CompareCompanionCandidatesByPriority);
            List<HorizontalCompanionEnvelope> occupiedEnvelopes =
                new List<HorizontalCompanionEnvelope>(
                    Mathf.Max(1, targetParticipantCount / 2));
            HashSet<long> occupiedCells = new HashSet<long>();

            int formedParticipantCount = 0;
            for (int index = 0;
                 index < available.Count &&
                 formedParticipantCount + 2 <= targetParticipantCount;
                 index++)
            {
                CompositionCandidate primary = available[index];
                if (primary.CompanionClusterIndex >= 0)
                {
                    continue;
                }

                int remainingParticipantBudget =
                    targetParticipantCount - formedParticipantCount;
                int requestedClusterSize =
                    ResolveHorizontalCompanionClusterSize(
                        primary,
                        effectiveStrength,
                        remainingParticipantBudget,
                        settings);
                int clusterIndex = clusterSizes.Count;
                audit.PrimaryClusterAttempts++;
                if (!TryComposeHorizontalCompanionCluster(
                        available,
                        primary,
                        requestedClusterSize,
                        occupiedEnvelopes,
                        occupiedCells,
                        settings,
                        clusterIndex,
                        audit,
                        out HorizontalCompanionEnvelope envelope,
                        out long cellKey,
                        out int composedClusterSize,
                        out GroundPaintedAccentCompanionPairLayout composedPairLayout))
                {
                    continue;
                }

                occupiedEnvelopes.Add(envelope);
                occupiedCells.Add(cellKey);
                clusterSizes.Add(composedClusterSize);
                if (composedClusterSize == 2)
                {
                    switch (composedPairLayout)
                    {
                        case GroundPaintedAccentCompanionPairLayout.SteppedContinuation:
                            pairSteppedLayoutCount++;
                            break;
                        case GroundPaintedAccentCompanionPairLayout.ShoulderContact:
                            pairShoulderLayoutCount++;
                            break;
                        case GroundPaintedAccentCompanionPairLayout.OffsetEcho:
                            pairOffsetLayoutCount++;
                            break;
                        case GroundPaintedAccentCompanionPairLayout.ShallowOffset:
                        default:
                            pairShallowLayoutCount++;
                            break;
                    }
                }
                formedParticipantCount += composedClusterSize;
            }

            audit.FormedParticipantCount = formedParticipantCount;
        }

        private static int CompareCompanionCandidatesByPriority(
            CompositionCandidate left,
            CompositionCandidate right)
        {
            uint leftPriority =
                (uint)Hash(
                    left.Source.Seed,
                    left.Region.Seed,
                    379);
            uint rightPriority =
                (uint)Hash(
                    right.Source.Seed,
                    right.Region.Seed,
                    379);
            int order = leftPriority.CompareTo(rightPriority);
            if (order != 0)
            {
                return order;
            }

            return CompareCompositionCandidatesBySource(left, right);
        }

        private static int ResolveHorizontalCompanionClusterSize(
            CompositionCandidate primary,
            float effectiveStrength,
            int remainingParticipantBudget,
            FieldSettings settings)
        {
            if (remainingParticipantBudget < 3)
            {
                return 2;
            }

            float verticality =
                Smooth01(settings.CompanionTripletVerticality);
            float tripletProbability =
                Mathf.Lerp(
                    0.08f,
                    MaximumHorizontalCompanionTripletProbability,
                    Mathf.Clamp01(effectiveStrength)) +
                MaximumTripletProbabilityVerticalityBoost *
                verticality *
                Mathf.Clamp01(effectiveStrength);
            tripletProbability = Mathf.Clamp01(tripletProbability);
            uint clusterHash =
                (uint)Hash(
                    primary.Source.Seed,
                    primary.Region.Seed,
                    457);
            return Hash01(clusterHash, 461u) < tripletProbability
                ? 3
                : 2;
        }

        private static bool TryComposeHorizontalCompanionCluster(
            List<CompositionCandidate> available,
            CompositionCandidate primary,
            int requestedClusterSize,
            List<HorizontalCompanionEnvelope> occupiedEnvelopes,
            HashSet<long> occupiedCells,
            FieldSettings settings,
            int clusterIndex,
            HorizontalCompanionCompositionAudit audit,
            out HorizontalCompanionEnvelope envelope,
            out long cellKey,
            out int composedClusterSize,
            out GroundPaintedAccentCompanionPairLayout composedPairLayout)
        {
            envelope = default;
            cellKey = 0L;
            composedClusterSize = 0;
            composedPairLayout =
                GroundPaintedAccentCompanionPairLayout.ShallowOffset;
            if (primary == null || primary.CompanionClusterIndex >= 0)
            {
                return false;
            }

            bool downgradeRecorded = false;
            if (!TryResolveHorizontalCompanionTargets(
                    primary,
                    requestedClusterSize,
                    settings,
                    audit,
                    out float primaryAngleOffsetDegrees,
                    out HorizontalCompanionTarget secondaryTarget,
                    out HorizontalCompanionTarget tertiaryTarget,
                    out HorizontalCompanionTargetFailureReason targetFailure))
            {
                RecordHorizontalCompanionTargetFailure(audit, targetFailure);
                if (requestedClusterSize < 3)
                {
                    return false;
                }

                audit.TripletToPairDowngrade++;
                downgradeRecorded = true;
                requestedClusterSize = 2;
                if (!TryResolveHorizontalCompanionTargets(
                        primary,
                        requestedClusterSize,
                        settings,
                        audit,
                        out primaryAngleOffsetDegrees,
                        out secondaryTarget,
                        out tertiaryTarget,
                        out targetFailure))
                {
                    RecordHorizontalCompanionTargetFailure(
                        audit,
                        targetFailure);
                    return false;
                }
            }

            CompositionCandidate secondary =
                FindTargetedAvailableCompanion(
                    available,
                    primary,
                    null,
                    secondaryTarget,
                    settings,
                    out bool secondaryUsedCrossRegion);
            if (secondaryUsedCrossRegion)
            {
                audit.CrossRegionCandidateSelection++;
            }
            if (secondary == null)
            {
                audit.NoSecondaryCandidate++;
                if (requestedClusterSize < 3)
                {
                    return false;
                }
            }

            if (requestedClusterSize >= 3)
            {
                CompositionCandidate tertiary = null;
                if (secondary != null)
                {
                    tertiary =
                        FindTargetedAvailableCompanion(
                            available,
                            primary,
                            secondary,
                            tertiaryTarget,
                            settings,
                            out bool tertiaryUsedCrossRegion);
                    if (tertiaryUsedCrossRegion)
                    {
                        audit.CrossRegionCandidateSelection++;
                    }
                    if (tertiary == null)
                    {
                        audit.NoTertiaryCandidate++;
                    }
                }

                if (secondary != null && tertiary != null)
                {
                    if (TryReserveHorizontalCompanionCluster(
                            primary,
                            primaryAngleOffsetDegrees,
                            secondary,
                            secondaryTarget,
                            tertiary,
                            tertiaryTarget,
                            occupiedEnvelopes,
                            occupiedCells,
                            settings,
                            out envelope,
                            out cellKey,
                            out HorizontalCompanionReservationFailureReason
                                reservationFailure))
                    {
                        ApplyHorizontalCompanionCluster(
                            primary,
                            primaryAngleOffsetDegrees,
                            secondary,
                            secondaryTarget,
                            tertiary,
                            tertiaryTarget,
                            GroundPaintedAccentCompanionPairLayout.None,
                            settings,
                            clusterIndex);
                        composedClusterSize = 3;
                        return true;
                    }

                    RecordHorizontalCompanionReservationFailure(
                        audit,
                        reservationFailure);
                }

                if (!downgradeRecorded)
                {
                    audit.TripletToPairDowngrade++;
                    downgradeRecorded = true;
                }

                requestedClusterSize = 2;
                if (!TryResolveHorizontalCompanionTargets(
                        primary,
                        requestedClusterSize,
                        settings,
                        audit,
                        out primaryAngleOffsetDegrees,
                        out secondaryTarget,
                        out tertiaryTarget,
                        out targetFailure))
                {
                    RecordHorizontalCompanionTargetFailure(
                        audit,
                        targetFailure);
                    return false;
                }

                secondary =
                    FindTargetedAvailableCompanion(
                        available,
                        primary,
                        null,
                        secondaryTarget,
                        settings,
                        out bool downgradedSecondaryUsedCrossRegion);
                if (downgradedSecondaryUsedCrossRegion)
                {
                    audit.CrossRegionCandidateSelection++;
                }
                if (secondary == null)
                {
                    audit.NoSecondaryCandidate++;
                    return false;
                }
            }

            if (!TryReserveHorizontalCompanionCluster(
                    primary,
                    primaryAngleOffsetDegrees,
                    secondary,
                    secondaryTarget,
                    null,
                    default,
                    occupiedEnvelopes,
                    occupiedCells,
                    settings,
                    out envelope,
                    out cellKey,
                    out HorizontalCompanionReservationFailureReason
                        pairReservationFailure))
            {
                RecordHorizontalCompanionReservationFailure(
                    audit,
                    pairReservationFailure);
                return false;
            }

            uint pairLayoutHash =
                (uint)Hash(
                    primary.Source.Seed,
                    primary.Region.Seed,
                    449);
            composedPairLayout =
                ResolveHorizontalCompanionPairLayout(
                    pairLayoutHash,
                    Smooth01(settings.CompanionTripletVerticality));
            ApplyHorizontalCompanionCluster(
                primary,
                primaryAngleOffsetDegrees,
                secondary,
                secondaryTarget,
                null,
                default,
                composedPairLayout,
                settings,
                clusterIndex);
            composedClusterSize = 2;
            return true;
        }

        private static bool TryResolveHorizontalCompanionTargets(
            CompositionCandidate primary,
            int requestedClusterSize,
            FieldSettings settings,
            HorizontalCompanionCompositionAudit audit,
            out float primaryAngleOffsetDegrees,
            out HorizontalCompanionTarget secondaryTarget,
            out HorizontalCompanionTarget tertiaryTarget,
            out HorizontalCompanionTargetFailureReason failureReason)
        {
            primaryAngleOffsetDegrees = 0f;
            secondaryTarget = default;
            tertiaryTarget = default;
            failureReason = HorizontalCompanionTargetFailureReason.None;
            if (primary == null)
            {
                failureReason =
                    HorizontalCompanionTargetFailureReason.InvalidPlan;
                return false;
            }

            uint clusterHash =
                (uint)Hash(
                    primary.Source.Seed,
                    primary.Region.Seed,
                    449);
            float verticality =
                Smooth01(settings.CompanionTripletVerticality);
            HorizontalCompanionArrangement arrangement =
                ResolveHorizontalCompanionArrangement(clusterHash);
            GroundPaintedAccentCompanionPairLayout pairLayout =
                requestedClusterSize == 2
                    ? ResolveHorizontalCompanionPairLayout(
                        clusterHash,
                        verticality)
                    : GroundPaintedAccentCompanionPairLayout.ShallowOffset;
            HorizontalCompanionTripletLayout tripletLayout =
                requestedClusterSize >= 3
                    ? ResolveHorizontalCompanionTripletLayout(
                        clusterHash,
                        verticality)
                    : HorizontalCompanionTripletLayout.FlatContinuation;
            ResolveHorizontalCompanionAngles(
                primary,
                arrangement,
                pairLayout,
                tripletLayout,
                requestedClusterSize,
                clusterHash,
                settings,
                out primaryAngleOffsetDegrees,
                out float secondaryAngleOffsetDegrees,
                out float tertiaryAngleOffsetDegrees);

            float secondaryLength =
                Mathf.Clamp(
                    primary.Length *
                    Mathf.Lerp(
                        MinimumCompanionLengthRatio,
                        MaximumCompanionLengthRatio,
                        Hash01(clusterHash, 383u)),
                    settings.StrokeLengthMin,
                    settings.StrokeLengthMax);
            float tertiaryLength =
                Mathf.Clamp(
                    primary.Length *
                    Mathf.Lerp(
                        MinimumTertiaryLengthRatio,
                        MaximumTertiaryLengthRatio,
                        Hash01(clusterHash, 389u)),
                    settings.StrokeLengthMin,
                    settings.StrokeLengthMax);
            float secondaryStrengthScale =
                Mathf.Lerp(
                    MinimumCompanionStrengthScale,
                    MaximumCompanionStrengthScale,
                    Hash01(clusterHash, 397u));
            float tertiaryStrengthScale =
                Mathf.Lerp(
                    MinimumTertiaryStrengthScale,
                    MaximumTertiaryStrengthScale,
                    Hash01(clusterHash, 401u));
            float tightness = Smooth01(settings.CompanionTightness);
            float horizontalSign =
                Hash01(clusterHash, 421u) < 0.5f ? -1f : 1f;
            bool structuredTriplet =
                requestedClusterSize >= 3 &&
                tripletLayout !=
                    HorizontalCompanionTripletLayout.FlatContinuation;
            bool structuredPair =
                requestedClusterSize == 2 &&
                pairLayout !=
                    GroundPaintedAccentCompanionPairLayout.ShallowOffset;

            if (structuredPair)
            {
                ResolveStructuredPairContactPlan(
                    pairLayout,
                    horizontalSign,
                    verticality,
                    clusterHash,
                    out float pairAnchorFraction,
                    out float pairTargetFraction);
                float structuredPairGap =
                    ResolveStructuredCompanionContactGap(
                        primary.Length,
                        secondaryLength,
                        pairAnchorFraction,
                        pairTargetFraction,
                        tightness,
                        verticality,
                        clusterHash,
                        409u,
                        settings);
                Vector2 structuredPairCenter =
                    ResolveCompanionContactCenter(
                        primary.CenterXZ,
                        primary.Length,
                        primaryAngleOffsetDegrees,
                        pairAnchorFraction,
                        secondaryLength,
                        secondaryAngleOffsetDegrees,
                        pairTargetFraction,
                        horizontalSign,
                        structuredPairGap,
                        settings) +
                    ResolveStructuredPairTranslation(
                        pairLayout,
                        primaryAngleOffsetDegrees,
                        secondaryAngleOffsetDegrees,
                        Mathf.Min(primary.Length, secondaryLength),
                        verticality,
                        clusterHash,
                        settings);
                if (!TryRepairHorizontalPairStructure(
                        primary.CenterXZ,
                        primary.Length,
                        primaryAngleOffsetDegrees,
                        secondaryLength,
                        secondaryAngleOffsetDegrees,
                        pairLayout,
                        clusterHash,
                        settings,
                        ref structuredPairCenter,
                        out bool pairStructureRepaired))
                {
                    failureReason =
                        HorizontalCompanionTargetFailureReason.PairStructure;
                    return false;
                }
                if (pairStructureRepaired && audit != null)
                {
                    audit.PairStructureRepair++;
                }

                secondaryTarget =
                    new HorizontalCompanionTarget(
                        arrangement,
                        structuredPairCenter,
                        secondaryLength,
                        secondaryStrengthScale,
                        secondaryAngleOffsetDegrees);
                return true;
            }

            if (!structuredTriplet)
            {
                float secondaryGap =
                    ResolveHorizontalCompanionGap(
                        primary.Length,
                        secondaryLength,
                        tightness,
                        clusterHash,
                        409u,
                        settings);
                float secondaryVerticalOffset =
                    ResolveHorizontalCompanionVerticalOffset(
                        arrangement,
                        1,
                        tightness,
                        clusterHash,
                        431u,
                        settings);
                Vector2 secondaryCenter =
                    ResolveAdjacentCompanionCenter(
                        primary.CenterXZ,
                        primary.Length,
                        primaryAngleOffsetDegrees,
                        secondaryLength,
                        secondaryAngleOffsetDegrees,
                        horizontalSign,
                        secondaryGap,
                        secondaryVerticalOffset,
                        settings);
                if (requestedClusterSize == 2 &&
                    pairLayout ==
                        GroundPaintedAccentCompanionPairLayout.ShallowOffset)
                {
                    secondaryCenter +=
                        ResolveShallowPairTranslation(
                            primaryAngleOffsetDegrees,
                            secondaryAngleOffsetDegrees,
                            Mathf.Min(primary.Length, secondaryLength),
                            verticality,
                            clusterHash,
                            settings);
                    if (!TryRepairHorizontalPairStructure(
                            primary.CenterXZ,
                            primary.Length,
                            primaryAngleOffsetDegrees,
                            secondaryLength,
                            secondaryAngleOffsetDegrees,
                            pairLayout,
                            clusterHash,
                            settings,
                            ref secondaryCenter,
                            out bool pairStructureRepaired))
                    {
                        failureReason =
                            HorizontalCompanionTargetFailureReason.PairStructure;
                        return false;
                    }
                    if (pairStructureRepaired && audit != null)
                    {
                        audit.PairStructureRepair++;
                    }
                }
                secondaryTarget =
                    new HorizontalCompanionTarget(
                        arrangement,
                        secondaryCenter,
                        secondaryLength,
                        secondaryStrengthScale,
                        secondaryAngleOffsetDegrees);

                if (requestedClusterSize < 3)
                {
                    return true;
                }

                float tertiaryGap =
                    ResolveHorizontalCompanionGap(
                        arrangement ==
                            HorizontalCompanionArrangement.StaggeredEcho
                            ? secondaryLength
                            : primary.Length,
                        tertiaryLength,
                        tightness,
                        clusterHash,
                        419u,
                        settings);
                float tertiaryVerticalOffset =
                    ResolveHorizontalCompanionVerticalOffset(
                        arrangement,
                        2,
                        tightness,
                        clusterHash,
                        433u,
                        settings);
                Vector2 tertiaryCenter;
                switch (arrangement)
                {
                    case HorizontalCompanionArrangement.StaggeredEcho:
                        tertiaryCenter =
                            ResolveAdjacentCompanionCenter(
                                secondaryCenter,
                                secondaryLength,
                                secondaryAngleOffsetDegrees,
                                tertiaryLength,
                                tertiaryAngleOffsetDegrees,
                                horizontalSign,
                                tertiaryGap,
                                tertiaryVerticalOffset,
                                settings);
                        break;
                    case HorizontalCompanionArrangement.OffsetShoulder:
                    case HorizontalCompanionArrangement.Continuation:
                    default:
                        tertiaryCenter =
                            ResolveAdjacentCompanionCenter(
                                primary.CenterXZ,
                                primary.Length,
                                primaryAngleOffsetDegrees,
                                tertiaryLength,
                                tertiaryAngleOffsetDegrees,
                                -horizontalSign,
                                tertiaryGap,
                                tertiaryVerticalOffset,
                                settings);
                        break;
                }

                tertiaryTarget =
                    new HorizontalCompanionTarget(
                        arrangement,
                        tertiaryCenter,
                        tertiaryLength,
                        tertiaryStrengthScale,
                        tertiaryAngleOffsetDegrees);
                return true;
            }

            ResolveStructuredTripletContactPlan(
                tripletLayout,
                horizontalSign,
                verticality,
                clusterHash,
                out float secondaryAnchorFraction,
                out float secondaryTargetFraction,
                out bool tertiaryAnchorsToPrimary,
                out float tertiaryAnchorFraction,
                out float tertiaryTargetFraction,
                out float tertiaryDirectionSign);
            float structuredSecondaryGap =
                ResolveStructuredCompanionContactGap(
                    primary.Length,
                    secondaryLength,
                    secondaryAnchorFraction,
                    secondaryTargetFraction,
                    tightness,
                    verticality,
                    clusterHash,
                    409u,
                    settings);
            ResolveStructuredTripletTranslations(
                tripletLayout,
                Mathf.Min(
                    primary.Length,
                    Mathf.Min(secondaryLength, tertiaryLength)),
                verticality,
                clusterHash,
                settings,
                out Vector2 secondaryTranslation,
                out Vector2 tertiaryTranslation);

            float tertiaryAnchorLength =
                tertiaryAnchorsToPrimary
                    ? primary.Length
                    : secondaryLength;
            float tertiaryAnchorAngle =
                tertiaryAnchorsToPrimary
                    ? primaryAngleOffsetDegrees
                    : secondaryAngleOffsetDegrees;
            float structuredTertiaryGap =
                ResolveStructuredCompanionContactGap(
                    tertiaryAnchorLength,
                    tertiaryLength,
                    tertiaryAnchorFraction,
                    tertiaryTargetFraction,
                    tightness,
                    verticality,
                    clusterHash,
                    419u,
                    settings);

            Vector2 structuredSecondaryCenter = default;
            Vector2 structuredTertiaryCenter = default;
            bool tripletStructureAccepted = false;
            int acceptedRepairPass = -1;
            for (int repairPass = 0; repairPass < 3; repairPass++)
            {
                float repairScale =
                    repairPass == 0
                        ? 1f
                        : repairPass == 1
                            ? FirstTripletStructureRepairScale
                            : SecondTripletStructureRepairScale;
                structuredSecondaryCenter =
                    ResolveCompanionContactCenter(
                        primary.CenterXZ,
                        primary.Length,
                        primaryAngleOffsetDegrees,
                        secondaryAnchorFraction,
                        secondaryLength,
                        secondaryAngleOffsetDegrees,
                        secondaryTargetFraction,
                        horizontalSign,
                        structuredSecondaryGap,
                        settings) +
                    secondaryTranslation * repairScale;
                Vector2 tertiaryAnchorCenter =
                    tertiaryAnchorsToPrimary
                        ? primary.CenterXZ
                        : structuredSecondaryCenter;
                structuredTertiaryCenter =
                    ResolveCompanionContactCenter(
                        tertiaryAnchorCenter,
                        tertiaryAnchorLength,
                        tertiaryAnchorAngle,
                        tertiaryAnchorFraction,
                        tertiaryLength,
                        tertiaryAngleOffsetDegrees,
                        tertiaryTargetFraction,
                        tertiaryDirectionSign,
                        structuredTertiaryGap,
                        settings) +
                    tertiaryTranslation * repairScale;
                if (!HasRequiredHorizontalTripletStructure(
                        primary.CenterXZ,
                        primary.Length,
                        primaryAngleOffsetDegrees,
                        structuredSecondaryCenter,
                        secondaryLength,
                        secondaryAngleOffsetDegrees,
                        structuredTertiaryCenter,
                        tertiaryLength,
                        tertiaryAngleOffsetDegrees,
                        tripletLayout,
                        settings))
                {
                    continue;
                }

                tripletStructureAccepted = true;
                acceptedRepairPass = repairPass;
                break;
            }

            if (!tripletStructureAccepted)
            {
                failureReason =
                    HorizontalCompanionTargetFailureReason.TripletStructure;
                return false;
            }
            if (acceptedRepairPass > 0 && audit != null)
            {
                audit.TripletStructureRepair++;
            }

            secondaryTarget =
                new HorizontalCompanionTarget(
                    arrangement,
                    structuredSecondaryCenter,
                    secondaryLength,
                    secondaryStrengthScale,
                    secondaryAngleOffsetDegrees);
            tertiaryTarget =
                new HorizontalCompanionTarget(
                    arrangement,
                    structuredTertiaryCenter,
                    tertiaryLength,
                    tertiaryStrengthScale,
                    tertiaryAngleOffsetDegrees);
            return true;
        }

        private static void ResolveStructuredPairContactPlan(
            GroundPaintedAccentCompanionPairLayout pairLayout,
            float horizontalSign,
            float verticality,
            uint clusterHash,
            out float anchorContactFraction,
            out float targetContactFraction)
        {
            float directionSign = horizontalSign < 0f ? -1f : 1f;
            switch (pairLayout)
            {
                case GroundPaintedAccentCompanionPairLayout.ShoulderContact:
                    anchorContactFraction =
                        Mathf.Lerp(0.34f, 0.04f, verticality) *
                        directionSign;
                    break;
                case GroundPaintedAccentCompanionPairLayout.OffsetEcho:
                    anchorContactFraction =
                        Mathf.Lerp(0.48f, 0.26f, verticality) *
                        directionSign;
                    break;
                case GroundPaintedAccentCompanionPairLayout.SteppedContinuation:
                default:
                    anchorContactFraction =
                        Mathf.Lerp(0.44f, 0.16f, verticality) *
                        directionSign;
                    break;
            }

            float anchorJitter =
                Mathf.Lerp(0.02f, 0.07f, verticality) *
                (Hash01(clusterHash, 571u) * 2f - 1f);
            anchorContactFraction =
                Mathf.Clamp(
                    anchorContactFraction + anchorJitter,
                    -0.48f,
                    0.48f);
            targetContactFraction =
                ResolveEndpointContactFraction(
                    -0.5f * directionSign,
                    clusterHash,
                    577u,
                    Mathf.Lerp(0.015f, 0.05f, verticality));
        }

        private static bool TryRepairHorizontalPairStructure(
            Vector2 primaryCenter,
            float primaryLength,
            float primaryAngleOffsetDegrees,
            float secondaryLength,
            float secondaryAngleOffsetDegrees,
            GroundPaintedAccentCompanionPairLayout pairLayout,
            uint clusterHash,
            FieldSettings settings,
            ref Vector2 secondaryCenter,
            out bool repaired)
        {
            repaired = false;
            if (HasRequiredHorizontalPairStructure(
                    primaryCenter,
                    primaryLength,
                    primaryAngleOffsetDegrees,
                    secondaryCenter,
                    secondaryLength,
                    secondaryAngleOffsetDegrees,
                    pairLayout,
                    settings))
            {
                return true;
            }

            Vector2 pairNormal =
                ResolveCompanionPairSharedNormal(
                    primaryAngleOffsetDegrees,
                    secondaryAngleOffsetDegrees,
                    settings);
            ResolveHorizontalPairStructureMinimums(
                Mathf.Min(primaryLength, secondaryLength),
                pairLayout,
                settings,
                out float minimumCenterStep,
                out float minimumPairNormalSpan);
            float currentSignedStep =
                Vector2.Dot(secondaryCenter - primaryCenter, pairNormal);
            float repairSign =
                Mathf.Abs(currentSignedStep) > 0.0001f
                    ? Mathf.Sign(currentSignedStep)
                    : (Hash01(clusterHash, 659u) < 0.5f ? -1f : 1f);
            float requiredStep =
                Mathf.Max(minimumCenterStep, minimumPairNormalSpan) *
                HorizontalPairStructureRepairMargin;
            secondaryCenter +=
                pairNormal *
                (repairSign * requiredStep - currentSignedStep);
            repaired = true;
            return HasRequiredHorizontalPairStructure(
                primaryCenter,
                primaryLength,
                primaryAngleOffsetDegrees,
                secondaryCenter,
                secondaryLength,
                secondaryAngleOffsetDegrees,
                pairLayout,
                settings);
        }

        private static void ResolveHorizontalPairStructureMinimums(
            float minimumLength,
            GroundPaintedAccentCompanionPairLayout pairLayout,
            FieldSettings settings,
            out float minimumCenterStep,
            out float minimumPairNormalSpan)
        {
            float verticality =
                Smooth01(settings.CompanionTripletVerticality);
            bool shallowOffset =
                pairLayout ==
                GroundPaintedAccentCompanionPairLayout.ShallowOffset;
            minimumCenterStep =
                shallowOffset
                    ? Mathf.Max(
                        settings.StrokeWidthWorld *
                        MaximumShallowPairStepWidthMultiplier *
                        verticality,
                        minimumLength *
                        MinimumShallowPairCenterStepLengthFraction *
                        verticality)
                    : Mathf.Max(
                        settings.StrokeWidthWorld *
                        Mathf.Lerp(
                            0f,
                            MaximumPairStepWidthMultiplier,
                            verticality),
                        minimumLength *
                        MinimumStructuredPairCenterStepLengthFraction *
                        verticality);
            minimumPairNormalSpan =
                shallowOffset
                    ? minimumLength *
                      MinimumShallowPairSpanLengthFraction *
                      verticality
                    : Mathf.Max(
                        settings.StrokeWidthWorld *
                        Mathf.Lerp(0f, 4.5f, verticality),
                        minimumLength * 0.22f * verticality);
        }

        private static bool HasRequiredHorizontalPairStructure(
            Vector2 primaryCenter,
            float primaryLength,
            float primaryAngleOffsetDegrees,
            Vector2 secondaryCenter,
            float secondaryLength,
            float secondaryAngleOffsetDegrees,
            GroundPaintedAccentCompanionPairLayout pairLayout,
            FieldSettings settings)
        {
            float verticality =
                Smooth01(settings.CompanionTripletVerticality);
            if (verticality <= 0.0001f)
            {
                return true;
            }

            Vector2 pairNormal =
                ResolveCompanionPairSharedNormal(
                    primaryAngleOffsetDegrees,
                    secondaryAngleOffsetDegrees,
                    settings);
            ResolveCompanionSegmentEndpoints(
                primaryCenter,
                primaryLength,
                primaryAngleOffsetDegrees,
                settings,
                out Vector2 primaryStart,
                out Vector2 primaryEnd);
            ResolveCompanionSegmentEndpoints(
                secondaryCenter,
                secondaryLength,
                secondaryAngleOffsetDegrees,
                settings,
                out Vector2 secondaryStart,
                out Vector2 secondaryEnd);

            float pairNormalMin = float.PositiveInfinity;
            float pairNormalMax = float.NegativeInfinity;
            AccumulateProjectedAxisExtent(
                primaryStart,
                pairNormal,
                ref pairNormalMin,
                ref pairNormalMax);
            AccumulateProjectedAxisExtent(
                primaryEnd,
                pairNormal,
                ref pairNormalMin,
                ref pairNormalMax);
            AccumulateProjectedAxisExtent(
                secondaryStart,
                pairNormal,
                ref pairNormalMin,
                ref pairNormalMax);
            AccumulateProjectedAxisExtent(
                secondaryEnd,
                pairNormal,
                ref pairNormalMin,
                ref pairNormalMax);

            float minimumLength = Mathf.Min(primaryLength, secondaryLength);
            ResolveHorizontalPairStructureMinimums(
                minimumLength,
                pairLayout,
                settings,
                out float minimumCenterStep,
                out float minimumPairNormalSpan);
            float pairLocalCenterStep =
                Mathf.Abs(
                    Vector2.Dot(
                        secondaryCenter - primaryCenter,
                        pairNormal));
            return pairLocalCenterStep >= minimumCenterStep &&
                   pairNormalMax - pairNormalMin >=
                   minimumPairNormalSpan;
        }

        private static bool HasRequiredHorizontalTripletStructure(
            Vector2 primaryCenter,
            float primaryLength,
            float primaryAngleOffsetDegrees,
            Vector2 secondaryCenter,
            float secondaryLength,
            float secondaryAngleOffsetDegrees,
            Vector2 tertiaryCenter,
            float tertiaryLength,
            float tertiaryAngleOffsetDegrees,
            HorizontalCompanionTripletLayout tripletLayout,
            FieldSettings settings)
        {
            float verticality =
                Smooth01(settings.CompanionTripletVerticality);
            if (verticality <= 0.0001f)
            {
                return true;
            }

            float minimumLength =
                Mathf.Min(
                    primaryLength,
                    Mathf.Min(secondaryLength, tertiaryLength));
            float minimumNonLinearity =
                Mathf.Max(
                    settings.StrokeWidthWorld *
                    Mathf.Lerp(
                        MinimumTripletNonLinearityWidthMultiplier,
                        MaximumTripletNonLinearityWidthMultiplier,
                        verticality),
                    minimumLength * 0.08f * verticality);
            float minimumVerticalSpan =
                Mathf.Max(
                    settings.StrokeWidthWorld *
                    Mathf.Lerp(
                        MinimumTripletVerticalSpanWidthMultiplier,
                        MaximumTripletVerticalSpanWidthMultiplier,
                        verticality),
                    minimumLength * 0.30f * verticality);
            float minimumStep =
                Mathf.Max(
                    settings.StrokeWidthWorld *
                    Mathf.Lerp(
                        MinimumTripletStepWidthMultiplier,
                        MaximumTripletStepWidthMultiplier,
                        verticality),
                    minimumLength * 0.12f * verticality);
            Vector2 visualHorizontalAxis =
                ResolveSafeAxis(settings.VisualHorizontalAxis);
            Vector2 visualVerticalAxis =
                new Vector2(
                    -visualHorizontalAxis.y,
                    visualHorizontalAxis.x);
            ResolveCompanionSegmentEndpoints(
                primaryCenter,
                primaryLength,
                primaryAngleOffsetDegrees,
                settings,
                out Vector2 primaryStart,
                out Vector2 primaryEnd);
            ResolveCompanionSegmentEndpoints(
                secondaryCenter,
                secondaryLength,
                secondaryAngleOffsetDegrees,
                settings,
                out Vector2 secondaryStart,
                out Vector2 secondaryEnd);
            ResolveCompanionSegmentEndpoints(
                tertiaryCenter,
                tertiaryLength,
                tertiaryAngleOffsetDegrees,
                settings,
                out Vector2 tertiaryStart,
                out Vector2 tertiaryEnd);

            float verticalMin = float.PositiveInfinity;
            float verticalMax = float.NegativeInfinity;
            AccumulateProjectedAxisExtent(
                primaryStart,
                visualVerticalAxis,
                ref verticalMin,
                ref verticalMax);
            AccumulateProjectedAxisExtent(
                primaryEnd,
                visualVerticalAxis,
                ref verticalMin,
                ref verticalMax);
            AccumulateProjectedAxisExtent(
                secondaryStart,
                visualVerticalAxis,
                ref verticalMin,
                ref verticalMax);
            AccumulateProjectedAxisExtent(
                secondaryEnd,
                visualVerticalAxis,
                ref verticalMin,
                ref verticalMax);
            AccumulateProjectedAxisExtent(
                tertiaryStart,
                visualVerticalAxis,
                ref verticalMin,
                ref verticalMax);
            AccumulateProjectedAxisExtent(
                tertiaryEnd,
                visualVerticalAxis,
                ref verticalMin,
                ref verticalMax);
            float verticalSpan = verticalMax - verticalMin;

            Vector2 outer = tertiaryCenter - primaryCenter;
            float outerLength = outer.magnitude;
            if (outerLength <= 0.0001f)
            {
                return false;
            }

            float perpendicularDistance =
                Mathf.Abs(
                    outer.x * (secondaryCenter.y - primaryCenter.y) -
                    outer.y * (secondaryCenter.x - primaryCenter.x)) /
                outerLength;
            float primaryVertical =
                Vector2.Dot(primaryCenter, visualVerticalAxis);
            float secondaryVertical =
                Vector2.Dot(secondaryCenter, visualVerticalAxis);
            float tertiaryVertical =
                Vector2.Dot(tertiaryCenter, visualVerticalAxis);
            float firstStep =
                Mathf.Abs(secondaryVertical - primaryVertical);
            float secondStep =
                tripletLayout ==
                    HorizontalCompanionTripletLayout.CrownRun ||
                tripletLayout ==
                    HorizontalCompanionTripletLayout.BrokenTerrace
                    ? Mathf.Abs(tertiaryVertical - primaryVertical)
                    : Mathf.Abs(tertiaryVertical - secondaryVertical);

            bool hasRequiredShape =
                perpendicularDistance >= minimumNonLinearity;

            return hasRequiredShape &&
                   verticalSpan >= minimumVerticalSpan &&
                   firstStep >= minimumStep &&
                   secondStep >= minimumStep;
        }

        private static void ResolveStructuredTripletContactPlan(
            HorizontalCompanionTripletLayout tripletLayout,
            float horizontalSign,
            float verticality,
            uint clusterHash,
            out float secondaryAnchorFraction,
            out float secondaryTargetFraction,
            out bool tertiaryAnchorsToPrimary,
            out float tertiaryAnchorFraction,
            out float tertiaryTargetFraction,
            out float tertiaryDirectionSign)
        {
            float directionSign = horizontalSign < 0f ? -1f : 1f;
            float end = 0.5f * directionSign;
            float oppositeEnd = -end;
            float primaryShoulder =
                Mathf.Lerp(0.46f, 0.16f, verticality) * directionSign;
            float secondaryShoulder =
                Mathf.Lerp(0.46f, 0.28f, verticality) * directionSign;

            switch (tripletLayout)
            {
                case HorizontalCompanionTripletLayout.CrownRun:
                    secondaryAnchorFraction = primaryShoulder;
                    secondaryTargetFraction = oppositeEnd;
                    tertiaryAnchorsToPrimary = true;
                    tertiaryAnchorFraction = -primaryShoulder;
                    tertiaryDirectionSign = -directionSign;
                    tertiaryTargetFraction =
                        -0.5f * tertiaryDirectionSign;
                    break;
                case HorizontalCompanionTripletLayout.BrokenTerrace:
                    secondaryAnchorFraction = primaryShoulder;
                    secondaryTargetFraction = oppositeEnd;
                    tertiaryAnchorsToPrimary = true;
                    tertiaryAnchorFraction = -primaryShoulder * 0.55f;
                    tertiaryDirectionSign = -directionSign;
                    tertiaryTargetFraction =
                        -0.5f * tertiaryDirectionSign;
                    break;
                case HorizontalCompanionTripletLayout.SteppedRun:
                default:
                    secondaryAnchorFraction = primaryShoulder;
                    secondaryTargetFraction = oppositeEnd;
                    tertiaryAnchorsToPrimary = false;
                    tertiaryAnchorFraction = secondaryShoulder;
                    tertiaryTargetFraction = oppositeEnd;
                    tertiaryDirectionSign = directionSign;
                    break;
            }

            float fractionJitter =
                Mathf.Lerp(0.025f, 0.075f, verticality);
            secondaryAnchorFraction =
                Mathf.Clamp(
                    secondaryAnchorFraction +
                    (Hash01(clusterHash, 547u) * 2f - 1f) *
                    fractionJitter,
                    -0.5f,
                    0.5f);
            secondaryTargetFraction =
                ResolveEndpointContactFraction(
                    secondaryTargetFraction,
                    clusterHash,
                    551u,
                    fractionJitter);
            tertiaryAnchorFraction =
                Mathf.Clamp(
                    tertiaryAnchorFraction +
                    (Hash01(clusterHash, 557u) * 2f - 1f) *
                    fractionJitter,
                    -0.5f,
                    0.5f);
            tertiaryTargetFraction =
                ResolveEndpointContactFraction(
                    tertiaryTargetFraction,
                    clusterHash,
                    563u,
                    fractionJitter);
        }

        private static float ResolveEndpointContactFraction(
            float endpointFraction,
            uint clusterHash,
            uint salt,
            float jitter)
        {
            float sign = endpointFraction < 0f ? -1f : 1f;
            float magnitude =
                Mathf.Clamp(
                    Mathf.Abs(endpointFraction) -
                    Hash01(clusterHash, salt) * jitter,
                    0.485f,
                    0.5f);
            return magnitude * sign;
        }

        private static float ResolveStructuredCompanionContactGap(
            float anchorLength,
            float targetLength,
            float anchorContactFraction,
            float targetContactFraction,
            float tightness,
            float verticality,
            uint clusterHash,
            uint salt,
            FieldSettings settings)
        {
            float looseGap =
                ResolveHorizontalCompanionGap(
                    anchorLength,
                    targetLength,
                    0f,
                    clusterHash,
                    salt,
                    settings);
            float endTaperFraction =
                Mathf.Lerp(0.025f, 0.35f, settings.EndTaper);
            bool endpointToEndpoint =
                Mathf.Abs(anchorContactFraction) >= 0.42f &&
                Mathf.Abs(targetContactFraction) >= 0.42f;
            float taperPenetration =
                endpointToEndpoint
                    ? Mathf.Min(anchorLength, targetLength) *
                      endTaperFraction *
                      Mathf.Lerp(0.02f, 0.055f, verticality)
                    : 0f;
            float penetrationFraction =
                endpointToEndpoint
                    ? MaximumEndpointContactPenetrationWidthFraction
                    : MaximumInteriorContactPenetrationWidthFraction;
            float widthPenetration =
                settings.StrokeWidthWorld *
                penetrationFraction *
                Mathf.Lerp(0.45f, 1f, Hash01(clusterHash, salt + 2u));
            float tightGap =
                Mathf.Clamp(
                    -(taperPenetration + widthPenetration),
                    MinimumCompanionGap,
                    0f);
            return Mathf.Lerp(looseGap, tightGap, tightness);
        }

        private static Vector2 ResolveStructuredPairTranslation(
            GroundPaintedAccentCompanionPairLayout pairLayout,
            float primaryAngleOffsetDegrees,
            float secondaryAngleOffsetDegrees,
            float minimumLength,
            float verticality,
            uint clusterHash,
            FieldSettings settings)
        {
            Vector2 pairNormal =
                ResolveCompanionPairSharedNormal(
                    primaryAngleOffsetDegrees,
                    secondaryAngleOffsetDegrees,
                    settings);
            float layoutScale;
            switch (pairLayout)
            {
                case GroundPaintedAccentCompanionPairLayout.ShoulderContact:
                    layoutScale = 1.12f;
                    break;
                case GroundPaintedAccentCompanionPairLayout.OffsetEcho:
                    layoutScale = OffsetEchoPairTranslationScale;
                    break;
                case GroundPaintedAccentCompanionPairLayout.SteppedContinuation:
                default:
                    layoutScale = 1f;
                    break;
            }

            float widthStep =
                settings.StrokeWidthWorld *
                MaximumPairStepWidthMultiplier *
                verticality;
            float lengthStep =
                Mathf.Max(0f, minimumLength) *
                StructuredPairTranslationLengthFraction *
                verticality;
            float magnitude =
                Mathf.Max(widthStep, lengthStep) *
                layoutScale *
                Mathf.Lerp(0.84f, 1.16f, Hash01(clusterHash, 607u));
            float sign = Hash01(clusterHash, 613u) < 0.5f ? -1f : 1f;
            return pairNormal * (magnitude * sign);
        }

        private static Vector2 ResolveShallowPairTranslation(
            float primaryAngleOffsetDegrees,
            float secondaryAngleOffsetDegrees,
            float minimumLength,
            float verticality,
            uint clusterHash,
            FieldSettings settings)
        {
            Vector2 pairNormal =
                ResolveCompanionPairSharedNormal(
                    primaryAngleOffsetDegrees,
                    secondaryAngleOffsetDegrees,
                    settings);
            float widthStep =
                settings.StrokeWidthWorld *
                MaximumShallowPairStepWidthMultiplier *
                verticality;
            float lengthStep =
                Mathf.Max(0f, minimumLength) *
                ShallowPairTranslationLengthFraction *
                verticality;
            float magnitude =
                Mathf.Max(widthStep, lengthStep) *
                Mathf.Lerp(0.88f, 1.12f, Hash01(clusterHash, 617u));
            float sign = Hash01(clusterHash, 619u) < 0.5f ? -1f : 1f;
            return pairNormal * (magnitude * sign);
        }

        private static Vector2 ResolveCompanionPairSharedNormal(
            float primaryAngleOffsetDegrees,
            float secondaryAngleOffsetDegrees,
            FieldSettings settings)
        {
            Vector2 visualHorizontalAxis =
                ResolveSafeAxis(settings.VisualHorizontalAxis);
            Vector2 primaryAxis =
                ResolveStrokeAxis(settings, primaryAngleOffsetDegrees);
            Vector2 secondaryAxis =
                ResolveStrokeAxis(settings, secondaryAngleOffsetDegrees);
            if (Vector2.Dot(primaryAxis, visualHorizontalAxis) < 0f)
            {
                primaryAxis = -primaryAxis;
            }
            if (Vector2.Dot(secondaryAxis, visualHorizontalAxis) < 0f)
            {
                secondaryAxis = -secondaryAxis;
            }

            Vector2 sharedAxis = ResolveSafeAxis(primaryAxis + secondaryAxis);
            return new Vector2(-sharedAxis.y, sharedAxis.x);
        }

        private static void ResolveStructuredTripletTranslations(
            HorizontalCompanionTripletLayout tripletLayout,
            float minimumLength,
            float verticality,
            uint clusterHash,
            FieldSettings settings,
            out Vector2 secondaryTranslation,
            out Vector2 tertiaryTranslation)
        {
            Vector2 horizontalAxis =
                ResolveSafeAxis(settings.VisualHorizontalAxis);
            Vector2 verticalAxis =
                new Vector2(-horizontalAxis.y, horizontalAxis.x);
            float widthStep =
                settings.StrokeWidthWorld *
                3.50f *
                verticality;
            float lengthStep =
                Mathf.Max(0f, minimumLength) *
                0.18f *
                verticality;
            float baseStep =
                Mathf.Max(widthStep, lengthStep) *
                Mathf.Lerp(0.88f, 1.18f, Hash01(clusterHash, 617u));
            float sign = Hash01(clusterHash, 619u) < 0.5f ? -1f : 1f;
            float secondaryStep = baseStep * sign;
            float tertiaryStep;
            switch (tripletLayout)
            {
                case HorizontalCompanionTripletLayout.CrownRun:
                    tertiaryStep =
                        baseStep *
                        Mathf.Lerp(0.72f, 1.02f, Hash01(clusterHash, 631u)) *
                        sign;
                    break;
                case HorizontalCompanionTripletLayout.BrokenTerrace:
                    tertiaryStep =
                        baseStep *
                        Mathf.Lerp(0.58f, 0.82f, Hash01(clusterHash, 641u)) *
                        -sign;
                    break;
                case HorizontalCompanionTripletLayout.SteppedRun:
                default:
                    tertiaryStep =
                        baseStep *
                        Mathf.Lerp(0.72f, 1.08f, Hash01(clusterHash, 643u)) *
                        sign;
                    break;
            }

            secondaryTranslation = verticalAxis * secondaryStep;
            tertiaryTranslation = verticalAxis * tertiaryStep;
        }

        private static Vector2 ResolveCompanionContactCenter(
            Vector2 anchorCenter,
            float anchorLength,
            float anchorAngleOffsetDegrees,
            float anchorContactFraction,
            float targetLength,
            float targetAngleOffsetDegrees,
            float targetContactFraction,
            float horizontalSign,
            float gap,
            FieldSettings settings)
        {
            float directionSign = horizontalSign < 0f ? -1f : 1f;
            Vector2 visualHorizontalAxis =
                ResolveSafeAxis(settings.VisualHorizontalAxis);
            Vector2 anchorAxis =
                ResolveStrokeAxis(settings, anchorAngleOffsetDegrees);
            if (Vector2.Dot(anchorAxis, visualHorizontalAxis) < 0f)
            {
                anchorAxis = -anchorAxis;
            }

            Vector2 targetAxis =
                ResolveStrokeAxis(settings, targetAngleOffsetDegrees);
            if (Vector2.Dot(targetAxis, visualHorizontalAxis) < 0f)
            {
                targetAxis = -targetAxis;
            }

            Vector2 anchorContact =
                anchorCenter +
                anchorAxis *
                (anchorLength *
                 Mathf.Clamp(anchorContactFraction, -0.5f, 0.5f));
            Vector2 targetContactOffset =
                targetAxis *
                (targetLength *
                 Mathf.Clamp(targetContactFraction, -0.5f, 0.5f));
            float targetContactSign =
                Mathf.Abs(targetContactFraction) > 0.0001f
                    ? Mathf.Sign(targetContactFraction)
                    : -directionSign;
            Vector2 targetOutwardAxis =
                targetAxis * -targetContactSign;
            return anchorContact +
                   targetOutwardAxis * gap -
                   targetContactOffset;
        }

        private static void ResolveCompanionSegmentEndpoints(
            Vector2 center,
            float length,
            float angleOffsetDegrees,
            FieldSettings settings,
            out Vector2 start,
            out Vector2 end)
        {
            Vector2 visualHorizontalAxis =
                ResolveSafeAxis(settings.VisualHorizontalAxis);
            Vector2 axis = ResolveStrokeAxis(settings, angleOffsetDegrees);
            if (Vector2.Dot(axis, visualHorizontalAxis) < 0f)
            {
                axis = -axis;
            }

            Vector2 half = axis * (length * 0.5f);
            start = center - half;
            end = center + half;
        }

        private static void AccumulateProjectedAxisExtent(
            Vector2 point,
            Vector2 axis,
            ref float minimum,
            ref float maximum)
        {
            float projected = Vector2.Dot(point, axis);
            minimum = Mathf.Min(minimum, projected);
            maximum = Mathf.Max(maximum, projected);
        }

        private static float ResolveHorizontalCompanionGap(
            float anchorLength,
            float targetLength,
            float tightness,
            uint clusterHash,
            uint salt,
            FieldSettings settings)
        {
            float looseGap =
                Mathf.Clamp(
                    (anchorLength + targetLength) *
                    Mathf.Lerp(0.12f, 0.20f, Hash01(clusterHash, salt)),
                    settings.StrokeWidthWorld * 1.25f,
                    MaximumCompanionGap);
            float endTaperFraction =
                Mathf.Lerp(0.025f, 0.35f, settings.EndTaper);
            float endTaperContactAllowance =
                (anchorLength + targetLength) *
                endTaperFraction *
                CompanionEndTaperContactFraction;
            float desiredVisibleGap =
                settings.StrokeWidthWorld *
                Mathf.Lerp(-0.20f, 0.70f, Hash01(clusterHash, salt + 2u));
            float tightGap =
                Mathf.Clamp(
                    desiredVisibleGap - endTaperContactAllowance,
                    MinimumCompanionGap,
                    settings.StrokeWidthWorld * 0.70f);
            return Mathf.Lerp(looseGap, tightGap, tightness);
        }

        private static float ResolveHorizontalCompanionVerticalOffset(
            HorizontalCompanionArrangement arrangement,
            int memberIndex,
            float tightness,
            uint clusterHash,
            uint salt,
            FieldSettings settings)
        {
            float tightMinimum;
            float tightMaximum;
            float looseMinimum;
            float looseMaximum;
            switch (arrangement)
            {
                case HorizontalCompanionArrangement.StaggeredEcho:
                    tightMinimum = 0.002f;
                    tightMaximum = 0.009f;
                    looseMinimum = 0.045f;
                    looseMaximum = 0.125f;
                    break;
                case HorizontalCompanionArrangement.OffsetShoulder:
                    tightMinimum = 0.004f;
                    tightMaximum = 0.014f;
                    looseMinimum = 0.060f;
                    looseMaximum = 0.145f;
                    break;
                case HorizontalCompanionArrangement.Continuation:
                default:
                    tightMinimum = 0.000f;
                    tightMaximum = 0.006f;
                    looseMinimum = 0.025f;
                    looseMaximum = 0.075f;
                    break;
            }

            tightMinimum =
                Mathf.Max(tightMinimum, settings.StrokeWidthWorld * 0.05f);
            tightMaximum =
                Mathf.Max(tightMaximum, settings.StrokeWidthWorld * 0.35f);
            float minimum = Mathf.Lerp(looseMinimum, tightMinimum, tightness);
            float maximum = Mathf.Lerp(looseMaximum, tightMaximum, tightness);
            float magnitude =
                Mathf.Lerp(
                    minimum,
                    maximum,
                    Hash01(clusterHash, salt + (uint)(memberIndex * 7)));
            float sign =
                Hash01(clusterHash, salt + (uint)(memberIndex * 11 + 3)) < 0.5f
                    ? -1f
                    : 1f;
            return magnitude * sign;
        }



        private static Vector2 ResolveAdjacentCompanionCenter(
            Vector2 anchorCenter,
            float anchorLength,
            float anchorAngleOffsetDegrees,
            float targetLength,
            float targetAngleOffsetDegrees,
            float horizontalSign,
            float gap,
            float verticalOffset,
            FieldSettings settings)
        {
            float directionSign = horizontalSign < 0f ? -1f : 1f;
            Vector2 visualHorizontalAxis =
                ResolveSafeAxis(settings.VisualHorizontalAxis);
            Vector2 anchorAxis =
                ResolveStrokeAxis(settings, anchorAngleOffsetDegrees);
            if (Vector2.Dot(anchorAxis, visualHorizontalAxis) < 0f)
            {
                anchorAxis = -anchorAxis;
            }

            Vector2 targetAxis =
                ResolveStrokeAxis(settings, targetAngleOffsetDegrees);
            if (Vector2.Dot(targetAxis, visualHorizontalAxis) < 0f)
            {
                targetAxis = -targetAxis;
            }

            Vector2 linkAxis = ResolveSafeAxis(anchorAxis + targetAxis);
            Vector2 visualVerticalAxis =
                new Vector2(
                    -visualHorizontalAxis.y,
                    visualHorizontalAxis.x);
            Vector2 anchorEndpoint =
                anchorCenter +
                anchorAxis * (anchorLength * 0.5f * directionSign);
            return anchorEndpoint +
                   linkAxis * (gap * directionSign) +
                   visualVerticalAxis * verticalOffset +
                   targetAxis * (targetLength * 0.5f * directionSign);
        }

        private static CompositionCandidate FindTargetedAvailableCompanion(
            List<CompositionCandidate> available,
            CompositionCandidate primary,
            CompositionCandidate excludedCandidate,
            HorizontalCompanionTarget target,
            FieldSettings settings,
            out bool usedCrossRegion)
        {
            usedCrossRegion = false;
            float maximumRelocation =
                Mathf.Max(
                    settings.StrokeLengthMax *
                    MaximumCompanionRelocationLengthMultiplier,
                    Vector2.Distance(primary.CenterXZ, target.CenterXZ) * 1.10f);
            float maximumRelocationSquared =
                maximumRelocation * maximumRelocation;

            for (int searchPass = 0; searchPass < 2; searchPass++)
            {
                bool sameRegionOnly = searchPass == 0;
                CompositionCandidate selected = null;
                float closestDistanceSquared = float.PositiveInfinity;
                for (int index = 0; index < available.Count; index++)
                {
                    CompositionCandidate candidate = available[index];
                    if (object.ReferenceEquals(candidate, primary) ||
                        object.ReferenceEquals(candidate, excludedCandidate) ||
                        candidate.CompanionClusterIndex >= 0)
                    {
                        continue;
                    }

                    bool sameRegion =
                        candidate.Region.CoordinateKey ==
                        primary.Region.CoordinateKey;
                    if ((sameRegionOnly && !sameRegion) ||
                        (!sameRegionOnly && sameRegion))
                    {
                        continue;
                    }

                    float relocationDistanceSquared =
                        (candidate.CenterXZ - target.CenterXZ).sqrMagnitude;
                    if (relocationDistanceSquared > maximumRelocationSquared)
                    {
                        continue;
                    }

                    if (relocationDistanceSquared <
                            closestDistanceSquared - 0.000001f ||
                        (Mathf.Abs(
                             relocationDistanceSquared -
                             closestDistanceSquared) <= 0.000001f &&
                         selected != null &&
                         CompareCompositionCandidatesBySource(
                             candidate,
                             selected) < 0))
                    {
                        selected = candidate;
                        closestDistanceSquared = relocationDistanceSquared;
                    }
                }

                if (selected != null)
                {
                    usedCrossRegion = !sameRegionOnly;
                    return selected;
                }
            }

            return null;
        }

        private static bool TryReserveHorizontalCompanionCluster(
            CompositionCandidate primary,
            float primaryAngleOffsetDegrees,
            CompositionCandidate secondary,
            HorizontalCompanionTarget secondaryTarget,
            CompositionCandidate tertiary,
            HorizontalCompanionTarget tertiaryTarget,
            List<HorizontalCompanionEnvelope> occupiedEnvelopes,
            HashSet<long> occupiedCells,
            FieldSettings settings,
            out HorizontalCompanionEnvelope envelope,
            out long cellKey,
            out HorizontalCompanionReservationFailureReason failureReason)
        {
            failureReason = HorizontalCompanionReservationFailureReason.None;
            envelope =
                BuildHorizontalCompanionEnvelope(
                    primary,
                    primaryAngleOffsetDegrees,
                    secondary,
                    secondaryTarget,
                    tertiary,
                    tertiaryTarget,
                    settings);
            for (int index = 0;
                 index < occupiedEnvelopes.Count;
                 index++)
            {
                if (envelope.Intersects(occupiedEnvelopes[index]))
                {
                    cellKey = 0L;
                    failureReason =
                        HorizontalCompanionReservationFailureReason
                            .EnvelopeCollision;
                    return false;
                }
            }

            Vector2 motifCenter =
                tertiary != null
                    ? (primary.CenterXZ +
                       secondaryTarget.CenterXZ +
                       tertiaryTarget.CenterXZ) / 3f
                    : (primary.CenterXZ + secondaryTarget.CenterXZ) * 0.5f;
            float cellSize =
                Mathf.Max(
                    MinimumCompanionCellSize,
                    settings.StrokeLengthMax *
                    CompanionCellSizeLengthMultiplier);
            Vector2 horizontalAxis =
                ResolveSafeAxis(settings.VisualHorizontalAxis);
            Vector2 verticalAxis =
                new Vector2(-horizontalAxis.y, horizontalAxis.x);
            cellKey =
                ResolveHorizontalCompanionCellKey(
                    motifCenter,
                    horizontalAxis,
                    verticalAxis,
                    cellSize);
            if (occupiedCells.Contains(cellKey))
            {
                failureReason =
                    HorizontalCompanionReservationFailureReason.OccupiedCell;
                return false;
            }

            return true;
        }

        private static void RecordHorizontalCompanionTargetFailure(
            HorizontalCompanionCompositionAudit audit,
            HorizontalCompanionTargetFailureReason failureReason)
        {
            if (audit == null ||
                failureReason == HorizontalCompanionTargetFailureReason.None)
            {
                return;
            }

            audit.TargetPlanRejected++;
            switch (failureReason)
            {
                case HorizontalCompanionTargetFailureReason.PairStructure:
                    audit.PairStructureRejected++;
                    break;
                case HorizontalCompanionTargetFailureReason.TripletStructure:
                    audit.TripletStructureRejected++;
                    break;
            }
        }

        private static void RecordHorizontalCompanionReservationFailure(
            HorizontalCompanionCompositionAudit audit,
            HorizontalCompanionReservationFailureReason failureReason)
        {
            if (audit == null)
            {
                return;
            }

            switch (failureReason)
            {
                case HorizontalCompanionReservationFailureReason
                    .EnvelopeCollision:
                    audit.EnvelopeCollision++;
                    break;
                case HorizontalCompanionReservationFailureReason.OccupiedCell:
                    audit.OccupiedCell++;
                    break;
            }
        }

        private static void ResolveHorizontalCompanionAngles(
            CompositionCandidate primary,
            HorizontalCompanionArrangement arrangement,
            GroundPaintedAccentCompanionPairLayout pairLayout,
            HorizontalCompanionTripletLayout tripletLayout,
            int requestedClusterSize,
            uint clusterHash,
            FieldSettings settings,
            out float primaryAngleOffsetDegrees,
            out float secondaryAngleOffsetDegrees,
            out float tertiaryAngleOffsetDegrees)
        {
            float authoredAngleJitter =
                Mathf.Max(0f, settings.AngleJitterDegrees);
            primaryAngleOffsetDegrees = 0f;
            secondaryAngleOffsetDegrees = 0f;
            tertiaryAngleOffsetDegrees = 0f;
            ResolveIndependentCandidateOrientation(
                primary,
                settings,
                out float authoredPrimaryAngleOffsetDegrees,
                out _);
            if (requestedClusterSize == 2 &&
                pairLayout !=
                    GroundPaintedAccentCompanionPairLayout.ShallowOffset)
            {
                ResolveStructuredHorizontalCompanionPairAngles(
                    primary,
                    pairLayout,
                    clusterHash,
                    authoredAngleJitter,
                    authoredPrimaryAngleOffsetDegrees,
                    settings.CompanionTripletVerticality,
                    out primaryAngleOffsetDegrees,
                    out secondaryAngleOffsetDegrees);
                return;
            }

            if (requestedClusterSize >= 3 &&
                tripletLayout !=
                    HorizontalCompanionTripletLayout.FlatContinuation)
            {
                ResolveStructuredHorizontalCompanionTripletAngles(
                    primary,
                    tripletLayout,
                    clusterHash,
                    authoredAngleJitter,
                    authoredPrimaryAngleOffsetDegrees,
                    settings.CompanionTripletVerticality,
                    out primaryAngleOffsetDegrees,
                    out secondaryAngleOffsetDegrees,
                    out tertiaryAngleOffsetDegrees);
                return;
            }

            if (authoredAngleJitter <= 0.0001f)
            {
                return;
            }

            float sharedTrend =
                Mathf.Clamp(
                    primary.Region.AngleOffsetDegrees +
                    (Hash01(clusterHash, 463u) * 2f - 1f) *
                    authoredAngleJitter * 0.30f,
                    -authoredAngleJitter,
                    authoredAngleJitter);
            float arrangementScale =
                arrangement == HorizontalCompanionArrangement.Continuation
                    ? 0.90f
                    : arrangement ==
                          HorizontalCompanionArrangement.StaggeredEcho
                        ? 1.00f
                        : 0.80f;
            float maximumDrift =
                Mathf.Min(
                    MaximumCompanionAngleDifferenceDegrees,
                    authoredAngleJitter * 0.36f) *
                arrangementScale;
            float minimumDifference =
                Mathf.Min(1.35f, maximumDrift * 0.45f);
            primaryAngleOffsetDegrees =
                ResolveHorizontalCompanionMemberAngle(
                    sharedTrend,
                    maximumDrift * 0.55f,
                    clusterHash,
                    467u,
                    -authoredAngleJitter,
                    authoredAngleJitter);
            secondaryAngleOffsetDegrees =
                ResolveHorizontalCompanionMemberAngle(
                    sharedTrend,
                    maximumDrift,
                    clusterHash,
                    479u,
                    -authoredAngleJitter,
                    authoredAngleJitter);
            secondaryAngleOffsetDegrees =
                EnsureHorizontalCompanionAngleSeparation(
                    primaryAngleOffsetDegrees,
                    secondaryAngleOffsetDegrees,
                    minimumDifference,
                    clusterHash,
                    487u,
                    authoredAngleJitter);

            if (requestedClusterSize < 3)
            {
                return;
            }

            tertiaryAngleOffsetDegrees =
                ResolveHorizontalCompanionMemberAngle(
                    sharedTrend,
                    maximumDrift,
                    clusterHash,
                    491u,
                    -authoredAngleJitter,
                    authoredAngleJitter);
            tertiaryAngleOffsetDegrees =
                EnsureHorizontalCompanionAngleSeparation(
                    secondaryAngleOffsetDegrees,
                    tertiaryAngleOffsetDegrees,
                    minimumDifference,
                    clusterHash,
                    499u,
                    authoredAngleJitter);
        }

        private static void ResolveStructuredHorizontalCompanionPairAngles(
            CompositionCandidate primary,
            GroundPaintedAccentCompanionPairLayout pairLayout,
            uint clusterHash,
            float authoredAngleJitter,
            float authoredPrimaryAngleOffsetDegrees,
            float pairVerticality,
            out float primaryAngleOffsetDegrees,
            out float secondaryAngleOffsetDegrees)
        {
            float verticality = Smooth01(pairVerticality);
            float memberLimit =
                ResolveCompanionMemberAngleLimit(
                    authoredAngleJitter,
                    MaximumPairCompanionAngleAllowanceDegrees,
                    verticality);
            primaryAngleOffsetDegrees =
                Mathf.Clamp(
                    authoredPrimaryAngleOffsetDegrees,
                    -memberLimit,
                    memberLimit);
            float legacyShapeAngle =
                Mathf.Min(
                    authoredAngleJitter * 0.55f,
                    MaximumCompanionAngleDifferenceDegrees * 1.25f);
            float maximumShapeAngle =
                Mathf.Lerp(
                    Mathf.Max(2f, legacyShapeAngle),
                    MaximumPairShapeAngleDegrees,
                    verticality);
            float minimumShapeAngle =
                Mathf.Lerp(
                    Mathf.Min(2.5f, maximumShapeAngle),
                    Mathf.Min(5f, maximumShapeAngle),
                    verticality);
            float shapeAngle =
                Mathf.Lerp(
                    minimumShapeAngle,
                    maximumShapeAngle,
                    Hash01(clusterHash, 587u));
            float direction =
                Hash01(clusterHash, 593u) < 0.5f ? -1f : 1f;
            float layoutScale;
            switch (pairLayout)
            {
                case GroundPaintedAccentCompanionPairLayout.ShoulderContact:
                    layoutScale = 1f;
                    break;
                case GroundPaintedAccentCompanionPairLayout.OffsetEcho:
                    layoutScale = 0.62f;
                    break;
                case GroundPaintedAccentCompanionPairLayout.SteppedContinuation:
                default:
                    layoutScale = 0.78f;
                    break;
            }

            float secondaryAngle =
                primaryAngleOffsetDegrees +
                direction * shapeAngle * layoutScale;
            secondaryAngleOffsetDegrees =
                ClampHorizontalTripletMemberAngle(
                    secondaryAngle,
                    Mathf.Lerp(0.35f, 1.5f, verticality),
                    clusterHash,
                    601u,
                    memberLimit);
        }

        private static void ResolveStructuredHorizontalCompanionTripletAngles(
            CompositionCandidate primary,
            HorizontalCompanionTripletLayout tripletLayout,
            uint clusterHash,
            float authoredAngleJitter,
            float authoredPrimaryAngleOffsetDegrees,
            float tripletVerticality,
            out float primaryAngleOffsetDegrees,
            out float secondaryAngleOffsetDegrees,
            out float tertiaryAngleOffsetDegrees)
        {
            float verticality = Smooth01(tripletVerticality);
            float memberAngleLimit =
                ResolveCompanionMemberAngleLimit(
                    authoredAngleJitter,
                    MaximumTripletCompanionAngleAllowanceDegrees,
                    verticality);
            primaryAngleOffsetDegrees =
                Mathf.Clamp(
                    authoredPrimaryAngleOffsetDegrees,
                    -memberAngleLimit,
                    memberAngleLimit);
            float legacyMinimumShapeAngle =
                Mathf.Min(
                    MinimumHorizontalTripletShapeAngleDegrees,
                    authoredAngleJitter * 0.45f);
            float legacyMaximumShapeAngle =
                Mathf.Max(
                    legacyMinimumShapeAngle,
                    Mathf.Min(
                        authoredAngleJitter * 0.72f,
                        MaximumTripletShapeAngleDegrees));
            float minimumShapeAngle =
                Mathf.Lerp(
                    legacyMinimumShapeAngle,
                    Mathf.Min(6f, MaximumTripletShapeAngleDegrees),
                    verticality);
            float maximumShapeAngle =
                Mathf.Lerp(
                    legacyMaximumShapeAngle,
                    MaximumTripletShapeAngleDegrees,
                    verticality);
            maximumShapeAngle =
                Mathf.Max(minimumShapeAngle, maximumShapeAngle);
            float shapeAngle =
                Mathf.Lerp(
                    minimumShapeAngle,
                    maximumShapeAngle,
                    Hash01(clusterHash, 509u));
            float direction =
                Hash01(clusterHash, 511u) < 0.5f ? -1f : 1f;
            float memberJitter =
                Mathf.Lerp(
                    Mathf.Min(
                        MaximumHorizontalTripletMemberJitterDegrees,
                        authoredAngleJitter * 0.08f),
                    1.75f,
                    verticality);

            switch (tripletLayout)
            {
                case HorizontalCompanionTripletLayout.CrownRun:
                    secondaryAngleOffsetDegrees =
                        primaryAngleOffsetDegrees +
                        direction * shapeAngle * 0.78f;
                    tertiaryAngleOffsetDegrees =
                        primaryAngleOffsetDegrees -
                        direction * shapeAngle * 0.68f;
                    break;
                case HorizontalCompanionTripletLayout.BrokenTerrace:
                    secondaryAngleOffsetDegrees =
                        primaryAngleOffsetDegrees +
                        direction * shapeAngle;
                    tertiaryAngleOffsetDegrees =
                        primaryAngleOffsetDegrees -
                        direction * shapeAngle * 0.48f;
                    break;
                case HorizontalCompanionTripletLayout.SteppedRun:
                default:
                    secondaryAngleOffsetDegrees =
                        primaryAngleOffsetDegrees +
                        direction * shapeAngle * 0.62f;
                    tertiaryAngleOffsetDegrees =
                        primaryAngleOffsetDegrees +
                        direction * shapeAngle * 0.28f;
                    break;
            }

            secondaryAngleOffsetDegrees =
                ClampHorizontalTripletMemberAngle(
                    secondaryAngleOffsetDegrees,
                    memberJitter,
                    clusterHash,
                    523u,
                    memberAngleLimit);
            tertiaryAngleOffsetDegrees =
                ClampHorizontalTripletMemberAngle(
                    tertiaryAngleOffsetDegrees,
                    memberJitter,
                    clusterHash,
                    527u,
                    memberAngleLimit);
        }

        private static float ResolveCompanionMemberAngleLimit(
            float authoredAngleJitter,
            float maximumCompanionAllowance,
            float verticality)
        {
            return Mathf.Min(
                AbsoluteCompanionMemberAngleDegrees,
                Mathf.Max(0f, authoredAngleJitter) +
                Mathf.Max(0f, maximumCompanionAllowance) *
                Smooth01(verticality));
        }

        private static float ClampHorizontalTripletMemberAngle(
            float angle,
            float memberJitter,
            uint clusterHash,
            uint salt,
            float memberAngleLimit)
        {
            float jitter =
                (Hash01(clusterHash, salt) * 2f - 1f) * memberJitter;
            return Mathf.Clamp(
                angle + jitter,
                -memberAngleLimit,
                memberAngleLimit);
        }

        private static float ResolveHorizontalCompanionMemberAngle(
            float sharedTrend,
            float maximumDrift,
            uint clusterHash,
            uint salt,
            float minimumAngle,
            float maximumAngle)
        {
            float signedDrift =
                (Hash01(clusterHash, salt) * 2f - 1f) *
                maximumDrift *
                Mathf.Lerp(0.45f, 1f, Hash01(clusterHash, salt + 2u));
            return Mathf.Clamp(
                sharedTrend + signedDrift,
                minimumAngle,
                maximumAngle);
        }

        private static float EnsureHorizontalCompanionAngleSeparation(
            float referenceAngle,
            float candidateAngle,
            float minimumDifference,
            uint clusterHash,
            uint salt,
            float authoredAngleJitter)
        {
            if (minimumDifference <= 0.0001f ||
                Mathf.Abs(candidateAngle - referenceAngle) >=
                    minimumDifference)
            {
                return candidateAngle;
            }

            float direction =
                Hash01(clusterHash, salt) < 0.5f ? -1f : 1f;
            return Mathf.Clamp(
                referenceAngle + minimumDifference * direction,
                -authoredAngleJitter,
                authoredAngleJitter);
        }

        private static HorizontalCompanionEnvelope
            BuildHorizontalCompanionEnvelope(
                CompositionCandidate primary,
                float primaryAngleOffsetDegrees,
                CompositionCandidate secondary,
                HorizontalCompanionTarget secondaryTarget,
                CompositionCandidate tertiary,
                HorizontalCompanionTarget tertiaryTarget,
                FieldSettings settings)
        {
            Vector2 horizontalAxis =
                ResolveSafeAxis(settings.VisualHorizontalAxis);
            Vector2 verticalAxis =
                new Vector2(-horizontalAxis.y, horizontalAxis.x);
            float horizontalMin = float.PositiveInfinity;
            float horizontalMax = float.NegativeInfinity;
            float verticalMin = float.PositiveInfinity;
            float verticalMax = float.NegativeInfinity;

            AccumulateHorizontalCompanionMemberBounds(
                primary.CenterXZ,
                primary.Length,
                primary.Width,
                primary.BodyWidth,
                primaryAngleOffsetDegrees,
                horizontalAxis,
                verticalAxis,
                settings,
                ref horizontalMin,
                ref horizontalMax,
                ref verticalMin,
                ref verticalMax);
            AccumulateHorizontalCompanionMemberBounds(
                secondaryTarget.CenterXZ,
                secondaryTarget.Length,
                secondary.Width,
                secondary.BodyWidth,
                secondaryTarget.AngleOffsetDegrees,
                horizontalAxis,
                verticalAxis,
                settings,
                ref horizontalMin,
                ref horizontalMax,
                ref verticalMin,
                ref verticalMax);
            if (tertiary != null)
            {
                AccumulateHorizontalCompanionMemberBounds(
                    tertiaryTarget.CenterXZ,
                    tertiaryTarget.Length,
                    tertiary.Width,
                    tertiary.BodyWidth,
                    tertiaryTarget.AngleOffsetDegrees,
                    horizontalAxis,
                    verticalAxis,
                    settings,
                    ref horizontalMin,
                    ref horizontalMax,
                    ref verticalMin,
                    ref verticalMax);
            }

            float endClearance =
                Mathf.Max(
                    MinimumCompanionEndClearance,
                    settings.StrokeLengthMin * 0.45f,
                    settings.StrokeWidthWorld * 3f);
            float corridorPadding =
                Mathf.Max(
                    MinimumCompanionCorridorPadding,
                    settings.StrokeWidthWorld * 3f,
                    settings.BodyWidthWorld * 0.25f);
            return new HorizontalCompanionEnvelope(
                horizontalMin - endClearance,
                horizontalMax + endClearance,
                verticalMin - corridorPadding,
                verticalMax + corridorPadding);
        }

        private static void AccumulateHorizontalCompanionMemberBounds(
            Vector2 centerXZ,
            float length,
            float width,
            float bodyWidth,
            float angleOffsetDegrees,
            Vector2 horizontalAxis,
            Vector2 verticalAxis,
            FieldSettings settings,
            ref float horizontalMin,
            ref float horizontalMax,
            ref float verticalMin,
            ref float verticalMax)
        {
            Vector2 axis =
                ResolveStrokeAxis(settings, angleOffsetDegrees);
            Vector2 start = centerXZ - axis * (length * 0.5f);
            Vector2 end = centerXZ + axis * (length * 0.5f);
            float pathPadding =
                Mathf.Max(width * 1.5f, bodyWidth * 0.30f) +
                length *
                MaximumWiggleFractionOfLength *
                settings.StrokePathWiggle;
            float startHorizontal = Vector2.Dot(start, horizontalAxis);
            float endHorizontal = Vector2.Dot(end, horizontalAxis);
            float startVertical = Vector2.Dot(start, verticalAxis);
            float endVertical = Vector2.Dot(end, verticalAxis);
            horizontalMin =
                Mathf.Min(
                    horizontalMin,
                    Mathf.Min(startHorizontal, endHorizontal) - pathPadding);
            horizontalMax =
                Mathf.Max(
                    horizontalMax,
                    Mathf.Max(startHorizontal, endHorizontal) + pathPadding);
            verticalMin =
                Mathf.Min(
                    verticalMin,
                    Mathf.Min(startVertical, endVertical) - pathPadding);
            verticalMax =
                Mathf.Max(
                    verticalMax,
                    Mathf.Max(startVertical, endVertical) + pathPadding);
        }

        private static long ResolveHorizontalCompanionCellKey(
            Vector2 centerXZ,
            Vector2 horizontalAxis,
            Vector2 verticalAxis,
            float cellSize)
        {
            float safeCellSize = Mathf.Max(0.0001f, cellSize);
            int horizontalCell =
                Mathf.FloorToInt(
                    Vector2.Dot(centerXZ, horizontalAxis) /
                    safeCellSize);
            int verticalCell =
                Mathf.FloorToInt(
                    Vector2.Dot(centerXZ, verticalAxis) /
                    safeCellSize);
            return ((long)horizontalCell << 32) ^
                   (uint)verticalCell;
        }

        private static void ApplyHorizontalCompanionCluster(
            CompositionCandidate primary,
            float primaryAngleOffsetDegrees,
            CompositionCandidate secondary,
            HorizontalCompanionTarget secondaryTarget,
            CompositionCandidate tertiary,
            HorizontalCompanionTarget tertiaryTarget,
            GroundPaintedAccentCompanionPairLayout pairLayout,
            FieldSettings settings,
            int clusterIndex)
        {
            uint secondaryHash =
                (uint)Hash(
                    primary.Source.Seed,
                    secondary.Source.Seed,
                    primary.Region.Seed ^ 0x4E41);
            GroundPaintedAccentGlyphFamily secondaryFamily =
                ResolveCompatibleCompanionFamily(
                    primary.Family,
                    settings.GlyphFamilyWeights,
                    secondaryHash);
            int desiredFacing =
                ResolveGlyphFacingSign(
                    primary.Family,
                    primary.ProfileSeed);
            if (desiredFacing == 0)
            {
                desiredFacing =
                    Hash01(secondaryHash, 503u) < 0.5f ? -1 : 1;
            }

            int secondaryProfileSeed =
                ResolveCompanionProfileSeed(
                    secondary.Source.Seed,
                    secondaryFamily,
                    desiredFacing,
                    secondaryHash);
            int intendedClusterSize = tertiary != null ? 3 : 2;
            ApplyHorizontalCompanionMember(
                secondary,
                secondaryTarget,
                secondaryFamily,
                secondaryProfileSeed,
                clusterIndex,
                GroundPaintedAccentCompanionMemberRole.Secondary,
                intendedClusterSize,
                pairLayout);

            if (tertiary != null)
            {
                uint tertiaryHash =
                    (uint)Hash(
                        primary.Source.Seed,
                        tertiary.Source.Seed,
                        primary.Region.Seed ^ 0x4E43);
                GroundPaintedAccentGlyphFamily tertiaryFamily =
                    ResolveCompatibleCompanionFamily(
                        secondaryFamily,
                        settings.GlyphFamilyWeights,
                        tertiaryHash);
                int tertiaryProfileSeed =
                    ResolveCompanionProfileSeed(
                        tertiary.Source.Seed,
                        tertiaryFamily,
                        desiredFacing,
                        tertiaryHash);
                ApplyHorizontalCompanionMember(
                    tertiary,
                    tertiaryTarget,
                    tertiaryFamily,
                    tertiaryProfileSeed,
                    clusterIndex,
                    GroundPaintedAccentCompanionMemberRole.Tertiary,
                    intendedClusterSize,
                    GroundPaintedAccentCompanionPairLayout.None);
            }

            primary.CompanionClusterIndex = clusterIndex;
            primary.CompanionMemberRole =
                GroundPaintedAccentCompanionMemberRole.Primary;
            primary.IntendedCompanionClusterSize = intendedClusterSize;
            primary.CompanionPairLayout =
                intendedClusterSize == 2
                    ? pairLayout
                    : GroundPaintedAccentCompanionPairLayout.None;
            primary.HasCompanionOrientation = true;
            primary.CompanionAngleOffsetDegrees =
                primaryAngleOffsetDegrees;
        }

        private static void ApplyHorizontalCompanionMember(
            CompositionCandidate candidate,
            HorizontalCompanionTarget target,
            GroundPaintedAccentGlyphFamily family,
            int profileSeed,
            int clusterIndex,
            GroundPaintedAccentCompanionMemberRole companionMemberRole,
            int intendedClusterSize,
            GroundPaintedAccentCompanionPairLayout pairLayout)
        {
            candidate.CenterXZ = target.CenterXZ;
            candidate.Length = target.Length;
            candidate.StrengthScale = target.StrengthScale;
            candidate.Family = family;
            candidate.ProfileSeed = profileSeed;
            candidate.CompanionClusterIndex = clusterIndex;
            candidate.CompanionMemberRole = companionMemberRole;
            candidate.IntendedCompanionClusterSize = intendedClusterSize;
            candidate.CompanionPairLayout =
                intendedClusterSize == 2
                    ? pairLayout
                    : GroundPaintedAccentCompanionPairLayout.None;
            candidate.HasCompanionOrientation = true;
            candidate.CompanionAngleOffsetDegrees =
                target.AngleOffsetDegrees;
        }

        private static HorizontalCompanionTripletLayout
            ResolveHorizontalCompanionTripletLayout(
                uint clusterHash,
                float verticality)
        {
            float flatProbability =
                Mathf.Lerp(
                    MaximumHorizontalTripletFlatProbability,
                    MinimumHorizontalTripletFlatProbability,
                    Smooth01(verticality));
            float roll = Hash01(clusterHash, 541u);
            if (roll >= 1f - flatProbability)
            {
                return HorizontalCompanionTripletLayout.FlatContinuation;
            }

            float structuredRoll =
                roll / Mathf.Max(0.0001f, 1f - flatProbability);
            if (structuredRoll < 0.38f)
            {
                return HorizontalCompanionTripletLayout.SteppedRun;
            }

            if (structuredRoll < 0.70f)
            {
                return HorizontalCompanionTripletLayout.CrownRun;
            }

            return HorizontalCompanionTripletLayout.BrokenTerrace;
        }

        private static GroundPaintedAccentCompanionPairLayout
            ResolveHorizontalCompanionPairLayout(
                uint clusterHash,
                float verticality)
        {
            float structuredProbability =
                MaximumStructuredPairProbability *
                Smooth01(verticality);
            float roll = Hash01(clusterHash, 607u);
            if (roll >= structuredProbability ||
                structuredProbability <= 0.0001f)
            {
                return GroundPaintedAccentCompanionPairLayout.ShallowOffset;
            }

            float structuredRoll =
                roll / Mathf.Max(0.0001f, structuredProbability);
            if (structuredRoll < 0.46f)
            {
                return GroundPaintedAccentCompanionPairLayout.SteppedContinuation;
            }

            if (structuredRoll < 0.79f)
            {
                return GroundPaintedAccentCompanionPairLayout.ShoulderContact;
            }

            return GroundPaintedAccentCompanionPairLayout.OffsetEcho;
        }

        private static HorizontalCompanionArrangement
            ResolveHorizontalCompanionArrangement(uint clusterHash)
        {
            float roll = Hash01(clusterHash, 443u);
            if (roll < 0.45f)
            {
                return HorizontalCompanionArrangement.Continuation;
            }

            if (roll < 0.80f)
            {
                return HorizontalCompanionArrangement.StaggeredEcho;
            }

            return HorizontalCompanionArrangement.OffsetShoulder;
        }

        private static int ResolveCompanionProfileSeed(
            int sourceSeed,
            GroundPaintedAccentGlyphFamily family,
            int desiredFacing,
            uint pairHash)
        {
            if (desiredFacing == 0 ||
                family == GroundPaintedAccentGlyphFamily.CompleteMound ||
                ResolveGlyphFacingSign(family, sourceSeed) == desiredFacing)
            {
                return sourceSeed;
            }

            for (int attempt = 0; attempt < 8; attempt++)
            {
                int candidateSeed =
                    Hash(
                        sourceSeed,
                        (int)pairHash,
                        449 + attempt * 17);
                if (ResolveGlyphFacingSign(family, candidateSeed) ==
                    desiredFacing)
                {
                    return candidateSeed;
                }
            }

            return sourceSeed;
        }

        private static int ResolveGlyphFacingSign(
            GroundPaintedAccentGlyphFamily family,
            int seed)
        {
            uint salt;
            switch (family)
            {
                case GroundPaintedAccentGlyphFamily.AsymmetricMound:
                    salt = 811u;
                    break;
                case GroundPaintedAccentGlyphFamily.SingleShoulder:
                    salt = 829u;
                    break;
                case GroundPaintedAccentGlyphFamily.ShallowCrest:
                    salt = 859u;
                    break;
                case GroundPaintedAccentGlyphFamily.CompleteMound:
                default:
                    return 0;
            }

            return Hash01((uint)seed, salt) < 0.5f ? -1 : 1;
        }

        private static GroundPaintedAccentGlyphFamily
            ResolveCompatibleCompanionFamily(
                GroundPaintedAccentGlyphFamily primaryFamily,
                Vector4 authoredWeights,
                uint pairHash)
        {
            float completeMultiplier;
            float asymmetricMultiplier;
            float shoulderMultiplier;
            float shallowMultiplier;

            switch (primaryFamily)
            {
                case GroundPaintedAccentGlyphFamily.AsymmetricMound:
                    completeMultiplier = 0.20f;
                    asymmetricMultiplier = 0.35f;
                    shoulderMultiplier = 1.00f;
                    shallowMultiplier = 0.65f;
                    break;
                case GroundPaintedAccentGlyphFamily.SingleShoulder:
                    completeMultiplier = 0.55f;
                    asymmetricMultiplier = 0.75f;
                    shoulderMultiplier = 0.35f;
                    shallowMultiplier = 1.00f;
                    break;
                case GroundPaintedAccentGlyphFamily.ShallowCrest:
                    completeMultiplier = 0.20f;
                    asymmetricMultiplier = 0.45f;
                    shoulderMultiplier = 1.00f;
                    shallowMultiplier = 0.75f;
                    break;
                case GroundPaintedAccentGlyphFamily.CompleteMound:
                default:
                    completeMultiplier = 0.15f;
                    asymmetricMultiplier = 0.35f;
                    shoulderMultiplier = 1.00f;
                    shallowMultiplier = 0.65f;
                    break;
            }

            float complete =
                Mathf.Max(0f, authoredWeights.x) * completeMultiplier;
            float asymmetric =
                Mathf.Max(0f, authoredWeights.y) * asymmetricMultiplier;
            float shoulder =
                Mathf.Max(0f, authoredWeights.z) * shoulderMultiplier;
            float shallow =
                Mathf.Max(0f, authoredWeights.w) * shallowMultiplier;
            float total = complete + asymmetric + shoulder + shallow;
            if (total <= 0.0001f)
            {
                return primaryFamily;
            }

            float roll =
                Mathf.Min(Hash01(pairHash, 431u), 0.99999994f) * total;
            if (roll < complete)
            {
                return GroundPaintedAccentGlyphFamily.CompleteMound;
            }

            roll -= complete;
            if (roll < asymmetric)
            {
                return GroundPaintedAccentGlyphFamily.AsymmetricMound;
            }

            roll -= asymmetric;
            if (roll < shoulder)
            {
                return GroundPaintedAccentGlyphFamily.SingleShoulder;
            }

            return GroundPaintedAccentGlyphFamily.ShallowCrest;
        }

        private static bool TryCreateIndependentFallbackStroke(
            CompositionCandidate candidate,
            FieldSettings settings,
            GroundHeightFieldSnapshot baseSurface,
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            out GroundPaintedAccentSurfaceStroke stroke)
        {
            stroke = default;
            if (candidate == null)
            {
                return false;
            }

            ResolveIndependentCandidateOrientation(
                candidate,
                settings,
                out _,
                out Vector2 independentAxis);
            uint strokeHash = (uint)candidate.Source.Seed;
            float curvature =
                candidate.IndependentLength *
                MaximumWiggleFractionOfLength *
                settings.StrokePathWiggle *
                (Hash01(strokeHash, 67u) * 2f - 1f);
            float strength =
                settings.Strength *
                Mathf.Lerp(
                    0.78f,
                    1.0f,
                    candidate.Source.SemanticSupport) *
                Mathf.Lerp(0.80f, 1.0f, Hash01(strokeHash, 71u)) *
                candidate.IndependentStrengthScale;

            return TryCreateSurfaceStroke(
                candidate.IndependentCenterXZ,
                independentAxis,
                candidate.IndependentLength,
                candidate.IndependentWidth,
                candidate.IndependentBodyWidth,
                curvature,
                strength,
                candidate.IndependentLength,
                candidate.IndependentFamily,
                candidate.Source.Seed,
                candidate.IndependentProfileSeed,
                baseSurface,
                rivers,
                modifiers,
                out stroke,
                out _);
        }

        private static void ResolveIndependentCandidateOrientation(
            CompositionCandidate candidate,
            FieldSettings settings,
            out float angleOffsetDegrees,
            out Vector2 axis)
        {
            float authoredAngleJitter =
                Mathf.Max(0f, settings.AngleJitterDegrees);
            if (authoredAngleJitter <= 0.0001f)
            {
                angleOffsetDegrees = 0f;
                axis = ResolveStrokeAxis(settings, 0f);
                return;
            }

            float sign =
                Hash01((uint)candidate.Source.Seed, 331u) < 0.5f
                    ? -1f
                    : 1f;
            float magnitude =
                authoredAngleJitter *
                Mathf.Lerp(
                    MinimumBalancedPerMarkJitterFraction,
                    1f,
                    Hash01((uint)candidate.Source.Seed, 73u));
            angleOffsetDegrees =
                Mathf.Clamp(
                    candidate.Region.AngleOffsetDegrees + sign * magnitude,
                    -authoredAngleJitter,
                    authoredAngleJitter);
            axis = ResolveStrokeAxis(settings, angleOffsetDegrees);
        }

        private static void ResolveCompositionCandidateOrientation(
            CompositionCandidate candidate,
            FieldSettings settings,
            CompositionRegionRuntime regionRuntime)
        {
            if (candidate.HasCompanionOrientation)
            {
                float maximumCompanionAllowance =
                    candidate.IntendedCompanionClusterSize >= 3
                        ? MaximumTripletCompanionAngleAllowanceDegrees
                        : MaximumPairCompanionAngleAllowanceDegrees;
                float companionAngleLimit =
                    ResolveCompanionMemberAngleLimit(
                        settings.AngleJitterDegrees,
                        maximumCompanionAllowance,
                        settings.CompanionTripletVerticality);
                candidate.TotalAngleOffsetDegrees =
                    Mathf.Clamp(
                        candidate.CompanionAngleOffsetDegrees,
                        -companionAngleLimit,
                        companionAngleLimit);
                candidate.Axis =
                    ResolveStrokeAxis(
                        settings,
                        candidate.TotalAngleOffsetDegrees);
                return;
            }

            float authoredAngleJitter =
                Mathf.Max(0f, settings.AngleJitterDegrees);
            if (authoredAngleJitter <= 0.0001f)
            {
                candidate.TotalAngleOffsetDegrees = 0f;
                candidate.Axis = ResolveStrokeAxis(settings, 0f);
                return;
            }

            float perMarkSign;
            if (regionRuntime.AcceptedPositiveAngleCount <
                regionRuntime.AcceptedNegativeAngleCount)
            {
                perMarkSign = 1f;
            }
            else if (regionRuntime.AcceptedNegativeAngleCount <
                     regionRuntime.AcceptedPositiveAngleCount)
            {
                perMarkSign = -1f;
            }
            else
            {
                perMarkSign =
                    Hash01((uint)candidate.Source.Seed, 331u) < 0.5f
                        ? -1f
                        : 1f;
            }

            float perMarkMagnitude =
                authoredAngleJitter *
                Mathf.Lerp(
                    MinimumBalancedPerMarkJitterFraction,
                    1f,
                    Hash01((uint)candidate.Source.Seed, 73u));
            candidate.TotalAngleOffsetDegrees =
                Mathf.Clamp(
                    candidate.Region.AngleOffsetDegrees +
                    perMarkSign * perMarkMagnitude,
                    -authoredAngleJitter,
                    authoredAngleJitter);
            candidate.Axis =
                ResolveStrokeAxis(
                    settings,
                    candidate.TotalAngleOffsetDegrees);
        }

        private static int CompareCompositionCandidatesBySource(
            CompositionCandidate left,
            CompositionCandidate right)
        {
            int order = CompareStrokeCandidates(left.Source, right.Source);
            if (order != 0)
            {
                return order;
            }

            return left.Source.Seed.CompareTo(right.Source.Seed);
        }

        private static GroundPaintedAccentGlyphFamily ResolveGlyphFamily(
            Vector4 weights,
            uint seed)
        {
            float complete = Mathf.Max(0f, weights.x);
            float asymmetric = Mathf.Max(0f, weights.y);
            float shoulder = Mathf.Max(0f, weights.z);
            float shallow = Mathf.Max(0f, weights.w);
            float total = complete + asymmetric + shoulder + shallow;
            if (total <= 0.0001f)
            {
                return GroundPaintedAccentGlyphFamily.CompleteMound;
            }

            float roll =
                Mathf.Min(Hash01(seed, 347u), 0.99999994f) * total;
            if (roll < complete)
            {
                return GroundPaintedAccentGlyphFamily.CompleteMound;
            }

            roll -= complete;
            if (roll < asymmetric)
            {
                return GroundPaintedAccentGlyphFamily.AsymmetricMound;
            }

            roll -= asymmetric;
            if (roll < shoulder)
            {
                return GroundPaintedAccentGlyphFamily.SingleShoulder;
            }

            if (shallow > 0f)
            {
                return GroundPaintedAccentGlyphFamily.ShallowCrest;
            }

            // Defensive floating-point fallback for isolated-family mixes.
            if (shoulder > 0f)
            {
                return GroundPaintedAccentGlyphFamily.SingleShoulder;
            }

            if (asymmetric > 0f)
            {
                return GroundPaintedAccentGlyphFamily.AsymmetricMound;
            }

            return GroundPaintedAccentGlyphFamily.CompleteMound;
        }

        private static void IncrementFamilyCount(
            GroundPaintedAccentGlyphFamily family,
            ref int completeMound,
            ref int asymmetricMound,
            ref int singleShoulder,
            ref int shallowCrest)
        {
            switch (family)
            {
                case GroundPaintedAccentGlyphFamily.AsymmetricMound:
                    asymmetricMound++;
                    break;
                case GroundPaintedAccentGlyphFamily.SingleShoulder:
                    singleShoulder++;
                    break;
                case GroundPaintedAccentGlyphFamily.ShallowCrest:
                    shallowCrest++;
                    break;
                case GroundPaintedAccentGlyphFamily.CompleteMound:
                default:
                    completeMound++;
                    break;
            }
        }

        private static List<StrokeCandidate> BuildStrokeCandidates(
            Vector4 originSize,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            FieldSettings settings,
            Vector2Int patchCoordinate,
            out double candidateBuildMilliseconds,
            out double regionalWeightingMilliseconds)
        {
            long candidateBuildStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
            long regionalWeightingTicks = 0L;
            float compositionRegionScale =
                ResolveCompositionRegionScale(settings);
            int desiredCandidateCount =
                Mathf.Max(
                    settings.TargetStrokeCount,
                    settings.TargetStrokeCount * CandidatePoolMultiplier);
            float aspect =
                Mathf.Max(0.25f, originSize.z / Mathf.Max(0.0001f, originSize.w));
            int columns =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(Mathf.Sqrt(desiredCandidateCount * aspect)));
            int rows =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(desiredCandidateCount / (float)columns));
            float cellWidth = originSize.z / columns;
            float cellHeight = originSize.w / rows;
            List<StrokeCandidate> candidates =
                new List<StrokeCandidate>(columns * rows);

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    int globalColumn = column + patchCoordinate.x * columns;
                    int globalRow = row + patchCoordinate.y * rows;
                    int cellHash = Hash(globalColumn, globalRow, settings.Seed);
                    float jitterX = Hash01((uint)cellHash, 19u);
                    float jitterZ = Hash01((uint)cellHash, 31u);
                    float localX =
                        originSize.x +
                        (column + Mathf.Lerp(0.08f, 0.92f, jitterX)) * cellWidth;
                    float localZ =
                        originSize.y +
                        (row + Mathf.Lerp(0.08f, 0.92f, jitterZ)) * cellHeight;
                    Vector2 centerXZ = new Vector2(localX, localZ);
                    float semanticSupport =
                        ResolveSemanticSupport(
                            baseSurface,
                            centerXZ,
                            feature != null ? feature.MaskInfluence : 0f);
                    Vector2 distributionSamplePosition =
                        ResolveDistributionSamplePosition(
                            centerXZ,
                            patchCoordinate,
                            originSize);
                    float patchWeight =
                        ResolveDistributionPatchWeight(
                            distributionSamplePosition,
                            settings.DistributionPatchScale,
                            settings.DistributionPatchiness,
                            settings.DistributionSparseFloor,
                            settings.Seed);
                    float semanticWeight =
                        Mathf.Lerp(0.45f, 1f, semanticSupport);
                    long regionalWeightingStartedAt =
                        System.Diagnostics.Stopwatch.GetTimestamp();
                    CompositionRegion region =
                        ResolveCompositionRegion(
                            distributionSamplePosition,
                            settings,
                            compositionRegionScale);
                    float regionalSelectionMultiplier =
                        region.SelectionMultiplier;
                    regionalWeightingTicks +=
                        System.Diagnostics.Stopwatch.GetTimestamp() -
                        regionalWeightingStartedAt;
                    float selectionWeight =
                        Mathf.Max(
                            0.001f,
                            patchWeight *
                            semanticWeight *
                            regionalSelectionMultiplier);
                    float selectionRoll =
                        Mathf.Max(0.000001f, Hash01((uint)cellHash, 43u));
                    float priority =
                        -Mathf.Log(selectionRoll) / selectionWeight;

                    candidates.Add(
                        new StrokeCandidate(
                            column,
                            row,
                            cellHash,
                            centerXZ,
                            priority,
                            patchWeight,
                            semanticSupport,
                            selectionWeight,
                            region));
                }
            }

            regionalWeightingMilliseconds =
                regionalWeightingTicks * 1000d /
                System.Diagnostics.Stopwatch.Frequency;
            candidateBuildMilliseconds =
                ResolveElapsedMilliseconds(candidateBuildStartedAt);
            return candidates;
        }

        private static Vector2 ResolveDistributionSamplePosition(
            Vector2 localXZ,
            Vector2Int patchCoordinate,
            Vector4 originSize)
        {
            return
                localXZ +
                new Vector2(
                    patchCoordinate.x * originSize.z,
                    patchCoordinate.y * originSize.w);
        }

        private static float ResolveDistributionPatchWeight(
            Vector2 distributionSamplePosition,
            float patchScale,
            float patchiness,
            float sparseFloor,
            int seed)
        {
            float patchNoise =
                ResolveDistributionPatchNoise(
                    distributionSamplePosition,
                    patchScale,
                    seed);

            return Mathf.Lerp(
                1f,
                Mathf.Lerp(
                    Mathf.Clamp(sparseFloor, 0.02f, 0.40f),
                    1f,
                    Smooth01(patchNoise)),
                Mathf.Clamp01(patchiness));
        }

        private static bool TryResolveDebugLocalPosition(
            GroundHeightFieldSnapshot baseSurface,
            Vector2 localXZ,
            out Vector3 localPosition)
        {
            localPosition = new Vector3(localXZ.x, 0f, localXZ.y);
            if (baseSurface == null ||
                !baseSurface.TrySample(localXZ, out GroundSurfaceSample sample))
            {
                return false;
            }

            localPosition.y = sample.Height + DebugSurfaceOffset;
            return true;
        }

        private static bool TryCreateSurfaceStroke(
            Vector2 centerXZ,
            Vector2 axis,
            float length,
            float width,
            float bodyWidth,
            float curvature,
            float strength,
            float authoredLength,
            GroundPaintedAccentGlyphFamily family,
            int geometrySeed,
            int profileSeed,
            GroundHeightFieldSnapshot baseSurface,
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            out GroundPaintedAccentSurfaceStroke stroke,
            out StrokeRejectionReason rejectionReason)
        {
            stroke = default;
            rejectionReason = StrokeRejectionReason.None;

            axis = ResolveSafeAxis(axis);
            Vector2 fixedCrossAxis = new Vector2(-axis.y, axis.x);
            float phase = Hash01((uint)geometrySeed, 83u) * Mathf.PI * 2f;
            float secondaryPhase = Hash01((uint)geometrySeed, 89u) * Mathf.PI * 2f;
            float estimatedValidationLength =
                length + Mathf.Abs(curvature) * 9f;
            int validationSampleCount =
                Mathf.Max(
                    MinimumValidationSampleCount,
                    Mathf.CeilToInt(
                        estimatedValidationLength /
                        MaximumValidationSpacing) + 1);
            float validationStep =
                validationSampleCount > 1
                    ? 1f / (validationSampleCount - 1f)
                    : 1f;
            bool hasPreviousCenter = false;
            Vector2 previousCenterXZ = Vector2.zero;
            float previousCenterHeight = 0f;

            for (int sampleIndex = 0;
                 sampleIndex < validationSampleCount;
                 sampleIndex++)
            {
                float t =
                    validationSampleCount <= 1
                        ? 0f
                        : sampleIndex / (float)(validationSampleCount - 1);
                Vector2 sampleCenterXZ =
                    EvaluateStrokeCenterline(
                        centerXZ,
                        axis,
                        fixedCrossAxis,
                        length,
                        curvature,
                        phase,
                        secondaryPhase,
                        t);
                Vector2 tangent =
                    ResolveStrokeTangent(
                        centerXZ,
                        axis,
                        fixedCrossAxis,
                        length,
                        curvature,
                        phase,
                        secondaryPhase,
                        t,
                        validationStep);
                Vector2 sideAxis = new Vector2(-tangent.y, tangent.x);
                float halfWidth = Mathf.Max(0.0005f, width * 0.5f);
                Vector2 leftXZ = sampleCenterXZ - sideAxis * halfWidth;
                Vector2 rightXZ = sampleCenterXZ + sideAxis * halfWidth;

                if (!baseSurface.TrySample(
                        sampleCenterXZ,
                        out GroundSurfaceSample centerSample) ||
                    !baseSurface.TrySample(
                        leftXZ,
                        out GroundSurfaceSample leftSample) ||
                    !baseSurface.TrySample(
                        rightXZ,
                        out GroundSurfaceSample rightSample))
                {
                    rejectionReason = StrokeRejectionReason.Sampling;
                    return false;
                }

                if (ExceedsBroadSlope(centerSample) ||
                    ExceedsBroadSlope(leftSample) ||
                    ExceedsBroadSlope(rightSample))
                {
                    rejectionReason = StrokeRejectionReason.BroadSlope;
                    return false;
                }

                if (IsExcludedByRiver(
                        sampleCenterXZ,
                        rivers,
                        halfWidth + RiverSafetyClearance) ||
                    IsExcludedByRiver(
                        leftXZ,
                        rivers,
                        RiverSafetyClearance) ||
                    IsExcludedByRiver(
                        rightXZ,
                        rivers,
                        RiverSafetyClearance))
                {
                    rejectionReason = StrokeRejectionReason.River;
                    return false;
                }

                if (IsExcludedByModifier(sampleCenterXZ, modifiers) ||
                    IsExcludedByModifier(leftXZ, modifiers) ||
                    IsExcludedByModifier(rightXZ, modifiers))
                {
                    rejectionReason = StrokeRejectionReason.ModifierExclusion;
                    return false;
                }

                if (ExceedsGrade(
                        leftSample.Height,
                        centerSample.Height,
                        Vector2.Distance(leftXZ, sampleCenterXZ)) ||
                    ExceedsGrade(
                        centerSample.Height,
                        rightSample.Height,
                        Vector2.Distance(sampleCenterXZ, rightXZ)))
                {
                    rejectionReason = StrokeRejectionReason.LocalGrade;
                    return false;
                }

                if (hasPreviousCenter &&
                    ExceedsGrade(
                        previousCenterHeight,
                        centerSample.Height,
                        Vector2.Distance(previousCenterXZ, sampleCenterXZ)))
                {
                    rejectionReason = StrokeRejectionReason.LocalGrade;
                    return false;
                }

                hasPreviousCenter = true;
                previousCenterXZ = sampleCenterXZ;
                previousCenterHeight = centerSample.Height;
            }

            float estimatedStoredPathLength =
                length + Mathf.Abs(curvature) * 4f;
            int pointCount =
                Mathf.Clamp(
                    Mathf.CeilToInt(
                        estimatedStoredPathLength /
                        TargetStoredStrokePointSpacing) + 1,
                    MinimumStrokePointCount,
                    MaximumStrokePointCount);
            Vector3[] localPoints = new Vector3[pointCount];
            Vector3[] localNormals = new Vector3[pointCount];

            for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
            {
                float t =
                    pointCount <= 1
                        ? 0f
                        : pointIndex / (float)(pointCount - 1);
                Vector2 localXZ =
                    EvaluateStrokeCenterline(
                        centerXZ,
                        axis,
                        fixedCrossAxis,
                        length,
                        curvature,
                        phase,
                        secondaryPhase,
                        t);

                if (!baseSurface.TrySample(
                        localXZ,
                        out GroundSurfaceSample sample))
                {
                    rejectionReason = StrokeRejectionReason.Sampling;
                    return false;
                }

                localPoints[pointIndex] =
                    new Vector3(localXZ.x, sample.Height, localXZ.y);
                localNormals[pointIndex] = sample.RenderNormal;
            }

            stroke =
                new GroundPaintedAccentSurfaceStroke(
                    localPoints,
                    localNormals,
                    width,
                    bodyWidth,
                    strength,
                    authoredLength,
                    family,
                    profileSeed);
            return stroke.IsValid;
        }

        private static Vector2 EvaluateStrokeCenterline(
            Vector2 centerXZ,
            Vector2 axis,
            Vector2 crossAxis,
            float length,
            float curvature,
            float phase,
            float secondaryPhase,
            float t)
        {
            float clampedT = Mathf.Clamp01(t);
            float signedT = clampedT * 2f - 1f;
            float endFade = Mathf.Sin(clampedT * Mathf.PI);
            float wobble =
                (Mathf.Sin(clampedT * Mathf.PI * 1.35f + phase) * 0.72f +
                 Mathf.Sin(clampedT * Mathf.PI * 2.10f + secondaryPhase) * 0.28f) *
                curvature *
                endFade;

            return
                centerXZ +
                axis * (signedT * length * 0.5f) +
                crossAxis * wobble;
        }

        private static Vector2 ResolveStrokeTangent(
            Vector2 centerXZ,
            Vector2 axis,
            Vector2 crossAxis,
            float length,
            float curvature,
            float phase,
            float secondaryPhase,
            float t,
            float sampleStep)
        {
            float halfStep = Mathf.Max(0.001f, sampleStep * 0.5f);
            float beforeT = Mathf.Clamp01(t - halfStep);
            float afterT = Mathf.Clamp01(t + halfStep);
            Vector2 before =
                EvaluateStrokeCenterline(
                    centerXZ,
                    axis,
                    crossAxis,
                    length,
                    curvature,
                    phase,
                    secondaryPhase,
                    beforeT);
            Vector2 after =
                EvaluateStrokeCenterline(
                    centerXZ,
                    axis,
                    crossAxis,
                    length,
                    curvature,
                    phase,
                    secondaryPhase,
                    afterT);
            Vector2 tangent = after - before;
            return tangent.sqrMagnitude > 0.000001f
                ? tangent.normalized
                : axis;
        }

        private static bool ExceedsBroadSlope(GroundSurfaceSample sample)
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
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers,
            float clearance)
        {
            if (rivers == null)
            {
                return false;
            }

            for (int riverIndex = 0;
                 riverIndex < rivers.Count;
                 riverIndex++)
            {
                StylizedRiverGroundSnapshot river = rivers[riverIndex];
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
            IReadOnlyList<GroundModifierSnapshot> modifiers)
        {
            if (modifiers == null)
            {
                return false;
            }

            for (int modifierIndex = 0;
                 modifierIndex < modifiers.Count;
                 modifierIndex++)
            {
                GroundModifierSnapshot modifier = modifiers[modifierIndex];
                if (!modifier.Excludes(
                        GroundSurfaceFeatureExclusionFlags.PaintedAccentLines))
                {
                    continue;
                }

                if (modifier.EvaluateWeight(point) > 0.0001f)
                {
                    return true;
                }
            }

            return false;
        }

        private static float ResolveSemanticSupport(
            GroundHeightFieldSnapshot baseSurface,
            Vector2 localXZ,
            float maskInfluence)
        {
            if (baseSurface == null ||
                !baseSurface.IsValid ||
                !baseSurface.TrySample(localXZ, out GroundSurfaceSample sample))
            {
                return 1f;
            }

            float semanticSupport =
                0.22f +
                sample.DampDeposit * 0.22f +
                sample.VegetationSuitability * 0.24f +
                sample.Compaction * 0.24f +
                sample.RockyDry * 0.08f;

            return Mathf.Lerp(
                1f,
                Mathf.Clamp01(semanticSupport),
                Mathf.Clamp01(maskInfluence));
        }

        private static Vector2 ResolveStrokeAxis(
            FieldSettings settings,
            float compositionAngleOffsetDegrees)
        {
            float facingDirectionDegrees = settings.StrokeFacingDirectionDegrees;
            float perpendicularStrokeAngleDegrees = facingDirectionDegrees + 90f;
            float finalAngle =
                (perpendicularStrokeAngleDegrees +
                 compositionAngleOffsetDegrees) *
                Mathf.Deg2Rad;

            return new Vector2(
                Mathf.Cos(finalAngle),
                Mathf.Sin(finalAngle));
        }

        private static int CompareStrokeCandidates(
            StrokeCandidate left,
            StrokeCandidate right)
        {
            int order = left.Priority.CompareTo(right.Priority);
            if (order != 0)
            {
                return order;
            }

            order = left.Row.CompareTo(right.Row);
            if (order != 0)
            {
                return order;
            }

            return left.Column.CompareTo(right.Column);
        }

        private static float ResolveDistributionPatchNoise(
            Vector2 point,
            float patchScale,
            int seed)
        {
            float broadScale = Mathf.Max(0.01f, patchScale);
            float detailScale = Mathf.Max(0.01f, patchScale * 0.43f);
            float broad =
                ValueNoise2D(point / broadScale, seed ^ 0x2A71);
            float detail =
                ValueNoise2D(
                    point / detailScale + new Vector2(13.7f, -9.2f),
                    seed ^ 0x61C5);
            return Mathf.Clamp01(broad * 0.72f + detail * 0.28f);
        }

        private static float ValueNoise2D(Vector2 point, int seed)
        {
            int x0 = Mathf.FloorToInt(point.x);
            int y0 = Mathf.FloorToInt(point.y);
            int x1 = x0 + 1;
            int y1 = y0 + 1;
            float tx = Smooth01(point.x - x0);
            float ty = Smooth01(point.y - y0);
            float a = Hash01((uint)Hash(x0, y0, seed), 101u);
            float b = Hash01((uint)Hash(x1, y0, seed), 103u);
            float c = Hash01((uint)Hash(x0, y1, seed), 107u);
            float d = Hash01((uint)Hash(x1, y1, seed), 109u);
            float lower = Mathf.Lerp(a, b, tx);
            float upper = Mathf.Lerp(c, d, tx);
            return Mathf.Lerp(lower, upper, ty);
        }

        private static Vector2 ResolveSafeAxis(Vector2 axis)
        {
            if (axis.sqrMagnitude < 0.0001f)
            {
                return Vector2.right;
            }

            return axis.normalized;
        }

        private static float SmoothStep(
            float edge0,
            float edge1,
            float value)
        {
            if (Mathf.Abs(edge1 - edge0) <= 0.00001f)
            {
                return value >= edge1 ? 1f : 0f;
            }

            return Smooth01((value - edge0) / (edge1 - edge0));
        }

        private static float Smooth01(float value)
        {
            value = Mathf.Clamp01(value);
            return value * value * (3f - 2f * value);
        }

        private static int Hash(
            int a,
            int b,
            int salt)
        {
            unchecked
            {
                uint h = 2166136261u;
                h = (h ^ (uint)a) * 16777619u;
                h = (h ^ (uint)b) * 16777619u;
                h = (h ^ (uint)salt) * 16777619u;
                h ^= h >> 13;
                h *= 1274126177u;
                h ^= h >> 16;
                return (int)h;
            }
        }

        private static float Hash01(
            uint hash,
            uint salt)
        {
            uint value = hash ^ (salt * 0x9E3779B9u);
            value ^= value >> 16;
            value *= 0x7FEB352Du;
            value ^= value >> 15;
            value *= 0x846CA68Bu;
            value ^= value >> 16;
            return (value & 0x00FFFFFFu) / 16777215f;
        }

        private static double ResolveElapsedMilliseconds(long startedAt)
        {
            long elapsedTicks =
                System.Diagnostics.Stopwatch.GetTimestamp() - startedAt;
            return elapsedTicks * 1000d /
                   System.Diagnostics.Stopwatch.Frequency;
        }

        private readonly struct FieldSettings
        {
            private FieldSettings(
                int seed,
                float strength,
                float contrast,
                int targetStrokeCount,
                float strokeLengthMin,
                float strokeLengthMax,
                float strokeWidthWorld,
                float bodyWidthWorld,
                float strokeFacingDirectionDegrees,
                float angleJitterDegrees,
                float strokePathWiggle,
                float endTaper,
                float distributionPatchScale,
                float distributionPatchiness,
                float distributionSparseFloor,
                float compositionRegionScale,
                float compositionDensityContrast,
                float horizontalCompanionStrength,
                float companionTightness,
                float companionTripletVerticality,
                Vector2 visualHorizontalAxis,
                Vector4 glyphFamilyWeights)
            {
                Seed = seed;
                Strength = strength;
                Contrast = contrast;
                TargetStrokeCount = targetStrokeCount;
                StrokeLengthMin = strokeLengthMin;
                StrokeLengthMax = Mathf.Max(strokeLengthMin + 0.05f, strokeLengthMax);
                StrokeWidthWorld = strokeWidthWorld;
                BodyWidthWorld = bodyWidthWorld;
                StrokeFacingDirectionDegrees = strokeFacingDirectionDegrees;
                AngleJitterDegrees = angleJitterDegrees;
                StrokePathWiggle = Mathf.Clamp01(strokePathWiggle);
                EndTaper = Mathf.Clamp01(endTaper);
                DistributionPatchScale = distributionPatchScale;
                DistributionPatchiness = distributionPatchiness;
                DistributionSparseFloor = distributionSparseFloor;
                CompositionRegionScale =
                    Mathf.Clamp(compositionRegionScale, 1f, 16f);
                CompositionDensityContrast =
                    Mathf.Clamp01(compositionDensityContrast);
                HorizontalCompanionStrength =
                    Mathf.Clamp01(horizontalCompanionStrength);
                CompanionTightness = Mathf.Clamp01(companionTightness);
                CompanionTripletVerticality =
                    Mathf.Clamp01(companionTripletVerticality);
                VisualHorizontalAxis = ResolveSafeAxis(visualHorizontalAxis);
                GlyphFamilyWeights = glyphFamilyWeights;
            }

            public int Seed { get; }
            public float Strength { get; }
            public float Contrast { get; }
            public int TargetStrokeCount { get; }
            public float StrokeLengthMin { get; }
            public float StrokeLengthMax { get; }
            public float StrokeWidthWorld { get; }
            public float BodyWidthWorld { get; }
            public float StrokeFacingDirectionDegrees { get; }
            public float AngleJitterDegrees { get; }
            public float StrokePathWiggle { get; }
            public float EndTaper { get; }
            public float DistributionPatchScale { get; }
            public float DistributionPatchiness { get; }
            public float DistributionSparseFloor { get; }
            public float CompositionRegionScale { get; }
            public float CompositionDensityContrast { get; }
            public float HorizontalCompanionStrength { get; }
            public float CompanionTightness { get; }
            public float CompanionTripletVerticality { get; }
            public Vector2 VisualHorizontalAxis { get; }
            public Vector4 GlyphFamilyWeights { get; }

            public static FieldSettings Create(
                GroundSurfaceFeatureRecipe feature,
                int shapeSeed,
                Vector4 originSize,
                Vector2 visualHorizontalAxis)
            {
                float strength =
                    feature != null
                        ? Mathf.Clamp01(feature.Strength)
                        : 0f;
                float contrast =
                    feature != null
                        ? Mathf.Clamp01(feature.Contrast)
                        : 0.5f;
                int seed =
                    Hash(
                        shapeSeed,
                        feature != null ? feature.SeedOffset : 0,
                        0x5A3D);
                float area = Mathf.Max(1f, originSize.z * originSize.w);
                float areaFactor = Mathf.Max(0.05f, area / 1600f);
                float strokeDensity =
                    feature != null
                        ? feature.PaintedAccentStrokeDensity
                        : 34f;
                int targetStrokeCount =
                    Mathf.Clamp(
                        Mathf.RoundToInt(strokeDensity * areaFactor),
                        0,
                        MaximumTargetStrokeCount);
                float strokeLengthMin =
                    feature != null
                        ? feature.PaintedAccentStrokeLengthMin
                        : 0.55f;
                float strokeLengthMax =
                    feature != null
                        ? feature.PaintedAccentStrokeLengthMax
                        : 1.55f;
                strokeLengthMax = Mathf.Max(strokeLengthMin + 0.05f, strokeLengthMax);
                float strokeWidthWorld =
                    feature != null
                        ? feature.PaintedAccentStrokeWidth
                        : 0.12f;
                float bodyWidthWorld =
                    Mathf.Max(
                        strokeWidthWorld * 3.25f,
                        strokeLengthMax * 0.12f);
                float strokeFacingDirectionDegrees =
                    feature != null
                        ? feature.PaintedAccentStrokeFacingDirectionDegrees
                        : 90f;
                float angleJitterDegrees =
                    feature != null
                        ? feature.PaintedAccentStrokeAngleJitterDegrees
                        : 18f;
                float strokePathWiggle =
                    feature != null
                        ? feature.PaintedAccentStrokePathWiggle
                        : 0.35f;
                float endTaper =
                    feature != null
                        ? feature.PaintedAccentFoldEndTaper
                        : 0.65f;
                float distributionPatchScale =
                    feature != null
                        ? feature.PaintedAccentDistributionScale
                        : 9f;
                float distributionPatchiness =
                    feature != null
                        ? feature.PaintedAccentDistributionContrast
                        : 0.70f;
                float distributionSparseFloor =
                    feature != null
                        ? feature.PaintedAccentDistributionSparseFloor
                        : 0.19f;
                float compositionRegionScale =
                    feature != null
                        ? feature.PaintedAccentCompositionRegionScale
                        : 5f;
                float compositionDensityContrast =
                    feature != null
                        ? feature.PaintedAccentCompositionDensityContrast
                        : 0.70f;
                float horizontalCompanionStrength =
                    feature != null
                        ? feature.PaintedAccentHorizontalCompanionStrength
                        : 0f;
                float companionTightness =
                    feature != null
                        ? feature.PaintedAccentCompanionTightness
                        : 0.65f;
                float companionTripletVerticality =
                    feature != null
                        ? feature.PaintedAccentCompanionTripletVerticality
                        : 1f;
                Vector4 glyphFamilyWeights =
                    feature != null
                        ? feature.PaintedAccentGlyphFamilyWeights
                        : new Vector4(0.20f, 0.30f, 0.30f, 0.20f);

                return new FieldSettings(
                    seed,
                    Mathf.Max(0.05f, strength),
                    contrast,
                    targetStrokeCount,
                    strokeLengthMin,
                    strokeLengthMax,
                    strokeWidthWorld,
                    bodyWidthWorld,
                    strokeFacingDirectionDegrees,
                    angleJitterDegrees,
                    strokePathWiggle,
                    endTaper,
                    distributionPatchScale,
                    distributionPatchiness,
                    distributionSparseFloor,
                    compositionRegionScale,
                    compositionDensityContrast,
                    horizontalCompanionStrength,
                    companionTightness,
                    companionTripletVerticality,
                    visualHorizontalAxis,
                    glyphFamilyWeights);
            }
        }
    }
}
