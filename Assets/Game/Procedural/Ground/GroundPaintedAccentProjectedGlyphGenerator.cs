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
            float maximumCrossAxisDrift)
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
            new GroundPaintedAccentProjectedGlyphBuildTimings(
                0d, 0d, 0d, 0d, 0d, 0d, 0d,
                0d, 0d, 0d, 0d, 0d, 0d, 0d);
    }

    internal sealed class GroundPaintedAccentProjectedGlyphTimingAccumulator
    {
        public long ProfileBuildTicks;
        public long FamilyValidationTicks;
        public long PointConstructionTicks;
        public long TopologyValidationTicks;
        public long ClusterCompositionTicks;
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

    internal sealed class GroundPaintedAccentProjectedGlyphScratch
    {
        public Vector2[] ProjectedPoints = Array.Empty<Vector2>();
        public float[] HalfWidths = Array.Empty<float>();
        public Vector2[] LeftPoints = Array.Empty<Vector2>();
        public Vector2[] RightPoints = Array.Empty<Vector2>();
        public Vector3[] SurfacePoints = Array.Empty<Vector3>();
        public int[] RiverIndices = Array.Empty<int>();
        public int[] ModifierIndices = Array.Empty<int>();

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
        public const int Revision = 7;

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
        private const float ProjectedUnintendedOverlapWidthFraction = 0.48f;
        private const float ProjectedExternalConflictWidthFraction = 0.42f;
        private const float MinimumProjectedExternalClearance = 0.003f;
        private const float MinimumProjectedStructuredStep = 0.020f;
        private const float MinimumProjectedStepRetentionFraction = 0.42f;
        private const float ProjectedStepErrorScoreWeight = 8f;

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
                return new ProjectedGlyphPrototype(
                    Stroke,
                    (Vector2[])ProjectedPoints.Clone(),
                    (float[])HalfWidths.Clone(),
                    Profile,
                    CrestT,
                    MaximumProjectedTurnDegrees,
                    MaximumNorthDisplacementError,
                    MaximumCrossAxisDrift);
            }
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
            Dictionary<int, List<GroundPaintedAccentSurfaceStroke>> clusters =
                new Dictionary<int, List<GroundPaintedAccentSurfaceStroke>>();
            HashSet<int> acceptedClusterKeys = new HashSet<int>();
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
            int companionRejectedIncomplete = 0;
            int companionRejectedPrototype = 0;
            int companionRejectedContact = 0;
            int companionRejectedSurface = 0;
            int companionRejectedExternalConflict = 0;

            for (int strokeIndex = 0;
                 strokeIndex < baseStrokes.Count;
                 strokeIndex++)
            {
                GroundPaintedAccentSurfaceStroke stroke = baseStrokes[strokeIndex];
                if (stroke.IsCompanionMember)
                {
                    if (!clusters.TryGetValue(
                            stroke.CompanionClusterIndex,
                            out List<GroundPaintedAccentSurfaceStroke> members))
                    {
                        members =
                            new List<GroundPaintedAccentSurfaceStroke>(
                                stroke.IntendedCompanionClusterSize);
                        clusters.Add(stroke.CompanionClusterIndex, members);
                    }

                    members.Add(stroke);
                    continue;
                }

                ProcessProjectedStroke(
                    stroke,
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

            List<int> clusterKeys = new List<int>(clusters.Keys);
            clusterKeys.Sort();
            for (int clusterKeyIndex = 0;
                 clusterKeyIndex < clusterKeys.Count;
                 clusterKeyIndex++)
            {
                int clusterKey = clusterKeys[clusterKeyIndex];
                List<GroundPaintedAccentSurfaceStroke> members =
                    clusters[clusterKey];
                members.Sort(CompareCompanionSurfaceStrokeMembers);
                companionClustersRequested++;

                if (TryBuildAtomicProjectedCluster(
                        members,
                        baseSurface,
                        feature,
                        rivers,
                        modifiers,
                        localNorth,
                        scratch,
                        timing,
                        glyphs,
                        out List<GroundPaintedAccentProjectedGlyph> clusterGlyphs,
                        out ProjectedCompanionFallbackReason fallbackReason))
                {
                    companionClustersAccepted++;
                    acceptedClusterKeys.Add(clusterKey);
                    if (clusterGlyphs.Count >= 3)
                    {
                        companionTripletsAccepted++;
                    }
                    else
                    {
                        companionPairsAccepted++;
                    }

                    glyphs.AddRange(clusterGlyphs);
                    continue;
                }

                companionClustersFallback++;
                switch (fallbackReason)
                {
                    case ProjectedCompanionFallbackReason.Incomplete:
                        companionRejectedIncomplete++;
                        break;
                    case ProjectedCompanionFallbackReason.Prototype:
                        companionRejectedPrototype++;
                        break;
                    case ProjectedCompanionFallbackReason.Contact:
                        companionRejectedContact++;
                        break;
                    case ProjectedCompanionFallbackReason.Surface:
                        companionRejectedSurface++;
                        break;
                    case ProjectedCompanionFallbackReason.ExternalConflict:
                        companionRejectedExternalConflict++;
                        break;
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
                            GroundPaintedAccentProjectedGlyphRejectionReason.Sampling,
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
            }

            ReconcileAcceptedProjectedClustersAgainstFinalIndependents(
                clusters,
                acceptedClusterKeys,
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
                ref rejectedSharpTurn,
                ref companionClustersAccepted,
                ref companionClustersFallback,
                ref companionPairsAccepted,
                ref companionTripletsAccepted,
                ref companionRejectedExternalConflict);

            long diagnosticsStartedAt =
                System.Diagnostics.Stopwatch.GetTimestamp();
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
                    maximumCrossDrift);

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
                ref int companionRejectedExternalConflict)
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
            int clusterKey,
            IReadOnlyList<GroundPaintedAccentProjectedGlyph> glyphs)
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

                    if (HasUnintendedProjectedOverlap(
                            member.LocalProjectedPoints,
                            member.HalfWidths,
                            other.LocalProjectedPoints,
                            other.HalfWidths,
                            -1,
                            -1,
                            ProjectedExternalConflictWidthFraction))
                    {
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
            out ProjectedCompanionFallbackReason fallbackReason)
        {
            clusterGlyphs = new List<GroundPaintedAccentProjectedGlyph>(3);
            fallbackReason = ProjectedCompanionFallbackReason.None;
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
                    GroundPaintedAccentCompanionMemberRole.Primary)
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
                if (member.IntendedCompanionClusterSize != intendedSize)
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
            if (!TryComposeProjectedCluster(
                    prototypes,
                    feature,
                    localNorth,
                    out List<ProjectedClusterContact> contacts))
            {
                timing.ClusterCompositionTicks +=
                    System.Diagnostics.Stopwatch.GetTimestamp() -
                    compositionStartedAt;
                fallbackReason = ProjectedCompanionFallbackReason.Contact;
                return false;
            }

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
            out List<ProjectedClusterContact> contacts)
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
                    tightness,
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
                        tightness,
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
            float tightness,
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
                    BuildProjectedAnchorContactIndices(anchor);
                for (int anchorContactIndex = 0;
                     anchorContactIndex < anchorContactIndices.Count;
                     anchorContactIndex++)
                {
                    int anchorPointIndex =
                        anchorContactIndices[anchorContactIndex];
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
                        float looseGap =
                            Mathf.Max(
                                0.018f,
                                (anchorHalfWidth + movingHalfWidth) * 1.60f);
                        float tightOverlap =
                            -Mathf.Min(anchorHalfWidth, movingHalfWidth) * 0.12f;
                        float contactGap =
                            Mathf.Lerp(looseGap, tightOverlap, tightness);
                        Vector2 targetContact =
                            anchor.ProjectedPoints[anchorPointIndex] +
                            outward * contactGap;
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
                        if (HasUnintendedProjectedOverlap(
                                anchor.ProjectedPoints,
                                anchor.HalfWidths,
                                candidatePoints,
                                candidateHalfWidths,
                                anchorPointIndex,
                                movingPointIndex,
                                ProjectedUnintendedOverlapWidthFraction))
                        {
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
                            if (HasUnintendedProjectedOverlap(
                                    other.ProjectedPoints,
                                    other.HalfWidths,
                                    candidatePoints,
                                    candidateHalfWidths,
                                    -1,
                                    -1,
                                    ProjectedUnintendedOverlapWidthFraction))
                            {
                                conflictsWithOtherMember = true;
                                break;
                            }
                        }

                        if (conflictsWithOtherMember)
                        {
                            continue;
                        }

                        Vector2 movingCentroid =
                            ResolveProjectedCentroid(candidatePoints);
                        Vector2 originalCentroid =
                            originalCentroids[movingMemberIndex];
                        Vector2 anchorCentroid =
                            ResolveProjectedCentroid(anchor.ProjectedPoints);
                        float desiredVerticalStep =
                            Vector2.Dot(
                                originalCentroid -
                                originalCentroids[anchorMemberIndex],
                                localNorth);
                        float candidateVerticalStep =
                            Vector2.Dot(
                                movingCentroid - anchorCentroid,
                                localNorth);
                        float desiredStepMagnitude =
                            Mathf.Abs(desiredVerticalStep);
                        if (desiredStepMagnitude >=
                            MinimumProjectedStructuredStep)
                        {
                            float minimumRetainedStep =
                                Mathf.Max(
                                    MinimumProjectedStructuredStep,
                                    desiredStepMagnitude *
                                    MinimumProjectedStepRetentionFraction);
                            if (Mathf.Sign(candidateVerticalStep) !=
                                    Mathf.Sign(desiredVerticalStep) ||
                                Mathf.Abs(candidateVerticalStep) <
                                    minimumRetainedStep)
                            {
                                continue;
                            }
                        }

                        float verticalStepError =
                            candidateVerticalStep - desiredVerticalStep;
                        float score =
                            translation.sqrMagnitude +
                            (movingCentroid - originalCentroid).sqrMagnitude * 0.15f +
                            verticalStepError * verticalStepError *
                            ProjectedStepErrorScoreWeight +
                            Mathf.Abs(contactGap) * 0.02f;
                        if (score >= bestScore)
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
            ProjectedGlyphPrototype prototype)
        {
            int pointCount = prototype.ProjectedPoints.Length;
            int start = ResolveSafeProjectedContactIndex(prototype, true);
            int end = ResolveSafeProjectedContactIndex(prototype, false);
            int[] candidates =
            {
                start,
                Mathf.RoundToInt((pointCount - 1) * 0.25f),
                Mathf.RoundToInt((pointCount - 1) * 0.50f),
                Mathf.RoundToInt((pointCount - 1) * 0.75f),
                end
            };
            List<int> indices = new List<int>(candidates.Length);
            for (int candidateIndex = 0;
                 candidateIndex < candidates.Length;
                 candidateIndex++)
            {
                int index = Mathf.Clamp(candidates[candidateIndex], 0, pointCount - 1);
                if (!indices.Contains(index))
                {
                    indices.Add(index);
                }
            }

            return indices;
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

        private static bool HasValidProjectedClusterSilhouette(
            IReadOnlyList<ProjectedGlyphPrototype> members,
            IReadOnlyList<ProjectedClusterContact> contacts)
        {
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
                    if (HasUnintendedProjectedOverlap(
                            members[leftIndex].ProjectedPoints,
                            members[leftIndex].HalfWidths,
                            members[rightIndex].ProjectedPoints,
                            members[rightIndex].HalfWidths,
                            leftContactIndex,
                            rightContactIndex,
                            ProjectedUnintendedOverlapWidthFraction))
                    {
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

        private static bool HasProjectedClusterExternalConflict(
            IReadOnlyList<ProjectedGlyphPrototype> members,
            IReadOnlyList<GroundPaintedAccentProjectedGlyph> acceptedGlyphs)
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
                    if (HasUnintendedProjectedOverlap(
                            member.ProjectedPoints,
                            member.HalfWidths,
                            glyph.LocalProjectedPoints,
                            glyph.HalfWidths,
                            -1,
                            -1,
                            ProjectedExternalConflictWidthFraction))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool HasUnintendedProjectedOverlap(
            IReadOnlyList<Vector2> leftPoints,
            IReadOnlyList<float> leftHalfWidths,
            IReadOnlyList<Vector2> rightPoints,
            IReadOnlyList<float> rightHalfWidths,
            int leftContactIndex,
            int rightContactIndex,
            float widthFraction)
        {
            if (!ProjectedBoundsOverlap(
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
                    if (IsProjectedContactNeighborhood(
                            leftSegmentIndex,
                            leftContactIndex,
                            leftNeighborhood) &&
                        IsProjectedContactNeighborhood(
                            rightSegmentIndex,
                            rightContactIndex,
                            rightNeighborhood))
                    {
                        continue;
                    }

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

        private static bool IsProjectedContactNeighborhood(
            int segmentIndex,
            int contactIndex,
            int neighborhood)
        {
            return contactIndex >= 0 &&
                   (Mathf.Abs(segmentIndex - contactIndex) <= neighborhood ||
                    Mathf.Abs(segmentIndex + 1 - contactIndex) <= neighborhood);
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
