using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear convex plane-cut kernel

        private readonly struct PlaneCutBevelCandidate
        {
            public readonly int SourceEdgeIndex;
            public readonly int VertexA;
            public readonly int VertexB;
            public readonly float Width;
            public readonly CutPlane Plane;
            public readonly float Strength;
            public readonly float SelectionScore;
            public readonly float PlaneTolerance;
            public readonly float ClipEpsilon;
            public readonly float MinimumSourceRemoval;
            public readonly bool WasLocalized;

            public PlaneCutBevelCandidate(
                int sourceEdgeIndex,
                int vertexA,
                int vertexB,
                float width,
                CutPlane plane,
                float strength,
                float selectionScore,
                float planeTolerance,
                float clipEpsilon,
                float minimumSourceRemoval,
                bool wasLocalized)
            {
                SourceEdgeIndex = sourceEdgeIndex;
                VertexA = vertexA;
                VertexB = vertexB;
                Width = width;
                Plane = plane;
                Strength = strength;
                SelectionScore = selectionScore;
                PlaneTolerance = planeTolerance;
                ClipEpsilon = clipEpsilon;
                MinimumSourceRemoval = minimumSourceRemoval;
                WasLocalized = wasLocalized;
            }
        }

        private sealed class PlaneCutConflictWidthReductionRecord
        {
            public int PassIndex;
            public int VictimEdgeIndex = -1;
            public int ForeignEdgeIndex = -1;
            public int VertexIndex = -1;
            public readonly List<int> ClusterEdgeIndices =
                new List<int>();
            public string TriggerCategory = string.Empty;
            public int BandValid;
            public int TopologyValid;
            public int OpenEdgeCount;
            public int NonManifoldEdgeCount;
            public int TJunctionCount;
            public int InvalidFaceCount;
            public int NonPlanarFaceCount;
            public int TopologyRollbackApplied;
            public float PreviousMinimumScale;
            public float RequestedScale;
            public float AppliedMinimumScale;
            public float ClusterFloorScale;
            public float VictimCoverageRatio;
            public float ForeignAxialParameter;
            public float ForeignSharedSpanRatio;
            public string ClusterReasonEvidence = string.Empty;
            public string PreviousScaleEvidence = string.Empty;
            public string RollbackScaleEvidence = string.Empty;
            public string AppliedScaleEvidence = string.Empty;
            public string Result = string.Empty;
        }

        private sealed class PlaneCutTJunctionFailureRecord
        {
            public int RecordIndex;
            public string Stage = string.Empty;
            public Vector3 JunctionVertex;
            public int VertexOwnerFaceCount;
            public string VertexOwnerFaces = string.Empty;
            public int HostFaceIndex = -1;
            public PolygonFaceProvenanceKind HostProvenanceKind;
            public int HostProvenanceIndex = -1;
            public int HostSegmentIndex = -1;
            public Vector3 HostStart;
            public Vector3 HostEnd;
            public Vector3 ClosestPoint;
            public float HostLength;
            public float SegmentParameter;
            public float Distance;
            public float Tolerance;
            public int MatchingHostSegmentCount;
            public string ProvenanceBevelEdges = string.Empty;
            public string CandidatePlaneMatches = string.Empty;
            public string AssociatedEdgeScales = string.Empty;
            public int LastConflictPass = -1;
            public string LastConflictCluster = string.Empty;
            public readonly List<int> LinkedEdgeIndices =
                new List<int>();
            public string Cause = string.Empty;
        }

        private sealed class PlaneCutLocalityDeferralRecord
        {
            public int SourceEdgeIndex = -1;
            public int VertexA = -1;
            public int VertexB = -1;
            public int FaceA = -1;
            public int FaceB = -1;
            public Vector3 SourceA;
            public Vector3 SourceB;
            public Vector3 BevelNormal;
            public float SolvedWidth;
            public float SolvedPlaneDistance;
            public float LocalizedPlaneDistance;
            public float LocalizationDelta;
            public float LocalGuardMargin;
            public int LimitingUnrelatedVertex = -1;
            public Vector3 LimitingUnrelatedPosition;
            public float LimitingUnrelatedProjection;
            public float SolvedSourceRemovalA;
            public float SolvedSourceRemovalB;
            public float LocalizedSourceRemovalA;
            public float LocalizedSourceRemovalB;
            public float MinimumRequiredRemoval;
            public string Cause = string.Empty;
        }

        private sealed class PlaneCutSolverTransactionState
        {
            public string Name = string.Empty;
            public int PassIndex = -1;
            public int BandClean;
            public int GeometryClean;
            public readonly List<PlaneCutBevelCandidate> Candidates =
                new List<PlaneCutBevelCandidate>();
            public List<PolygonFace> Faces = new List<PolygonFace>();
            public Dictionary<int, float> ScaleByEdge =
                new Dictionary<int, float>();
            public PlaneCutStageSnapshot Stage;
        }

        private sealed class PlaneCutRetryFailureDossier
        {
            public int PassIndex = -1;
            public string Stage = string.Empty;
            public int AttemptedBuiltCount;
            public int OpenEdgeCount;
            public int NonManifoldEdgeCount;
            public int TJunctionCount;
            public int InvalidFaceCount;
            public int NonPlanarFaceCount;
            public string CandidateEdgeEvidence = string.Empty;
            public string ScaleEvidence = string.Empty;
            public string NonManifoldEvidence = string.Empty;
            public string InvalidFaceEvidence = string.Empty;
            public string GeneralizedClusterEvidence = string.Empty;
            public string GeneralizedClusterReasonEvidence = string.Empty;
            public string Cause = string.Empty;
            public readonly List<PlaneCutOpenEdgeFailureRecord>
                OpenEdgeFailures =
                    new List<PlaneCutOpenEdgeFailureRecord>();
            public readonly List<PlaneCutFaceQualityFailureRecord>
                NonPlanarFaceFailures =
                    new List<PlaneCutFaceQualityFailureRecord>();
            public readonly List<PlaneCutTJunctionFailureRecord>
                TJunctionFailures =
                    new List<PlaneCutTJunctionFailureRecord>();
            public readonly List<int> LinkedEdgeIndices = new List<int>();
        }

        private enum PlaneCutCoexistenceFailureCategory
        {
            None,
            PlaneBand,
            StrictIntersection,
            MissingJunction,
            TJunction,
            OpenBoundary,
            FaceQuality,
            CandidateConservation,
            RetainedVolume,
            SourceBounds,
            SurfaceTriangulation,
            General
        }

        private sealed class PlaneCutCoexistenceFailureDossier
        {
            public PlaneCutCoexistenceFailureCategory Category;
            public string Stage = string.Empty;
            public int SourceVertex = -1;
            public int VictimEdge = -1;
            public int ForeignEdge = -1;
            public readonly List<int> LinkedEdges = new List<int>();
            public readonly List<int> IncidentStarEdges = new List<int>();
            public int OpenEdgeCount;
            public int TJunctionCount;
            public int NonPlanarCount;
            public string Diagnostic = string.Empty;
        }

        private sealed class PlaneCutCoexistenceTrialOutcome
        {
            public string CacheKey = string.Empty;
            public readonly List<int> ExcludedEdgeIndices = new List<int>();
            public string ExclusionReason = string.Empty;
            public bool FullyValid;
            public bool CandidateConservationValid = true;
            public int ExpectedCandidateCount;
            public int ActualCandidateCount;
            public int CertifiedCandidateCount;
            public readonly List<int> ExpectedCandidateEdgeIndices =
                new List<int>();
            public readonly List<int> ActualCandidateEdgeIndices =
                new List<int>();
            public readonly List<int> MissingCandidateEdgeIndices =
                new List<int>();
            public readonly List<int> UnexpectedCandidateEdgeIndices =
                new List<int>();
            public string FailureSignature = string.Empty;
            public Dictionary<int, float> ScaleByEdge =
                new Dictionary<int, float>();
            public List<PlaneCutBevelCandidate> Candidates =
                new List<PlaneCutBevelCandidate>();
            public List<PolygonFace> Faces = new List<PolygonFace>();
            public PlaneCutTopologyScaleTrialRecord Trial;
            public PlaneCutBevelAuditResult Audit;
            public PlaneCutCoexistenceFailureDossier DirectFailureDossier;
            public string Blocker = string.Empty;
        }

        private sealed class PlaneCutCoexistenceSearchNode
        {
            public PlaneCutCoexistenceTrialOutcome Outcome;
            public PlaneCutCoexistenceFailureDossier EffectiveFailureDossier;
            public Dictionary<int, string> ReasonByExcludedEdge =
                new Dictionary<int, string>();
        }

        private sealed class PlaneCutCoexistenceSearchStateRecord
        {
            public int StateIndex;
            public int Depth;
            public string ExclusionEvidence = string.Empty;
            public string FailureCategory = string.Empty;
            public string FailureStage = string.Empty;
            public int SourceVertex = -1;
            public int VictimEdge = -1;
            public int ForeignEdge = -1;
            public string LinkedEdgeEvidence = string.Empty;
            public string IncidentStarEvidence = string.Empty;
            public string ImplicatedEdgeEvidence = string.Empty;
            public int ExpectedCandidateCount;
            public int ActualCandidateCount;
            public int CertifiedCandidateCount;
            public string ExpectedCandidateEvidence = string.Empty;
            public string ActualCandidateEvidence = string.Empty;
            public string MissingCandidateEvidence = string.Empty;
            public string UnexpectedCandidateEvidence = string.Empty;
            public int CandidateConservationValid;
            public float MinimumWidthScale;
            public int FullyValid;
            public string FailureSignature = string.Empty;
        }

        private sealed class PlaneCutTopologyScaleTrialRecord
        {
            public int TrialIndex;
            public int BasePassIndex = -1;
            public float Factor;
            public string SearchMode = string.Empty;
            public string ProtectedEdgeEvidence = string.Empty;
            public int BandVictimEdgeIndex = -1;
            public int BandForeignEdgeIndex = -1;
            public float BandForeignAxialParameter;
            public float BandForeignSharedSpanRatio;
            public readonly List<int> ClusterEdgeIndices = new List<int>();
            public string BaseScaleEvidence = string.Empty;
            public string RequestedScaleEvidence = string.Empty;
            public string EffectiveScaleEvidence = string.Empty;
            public string FloorHitEvidence = string.Empty;
            public string CollateralChangedEvidence = string.Empty;
            public int AttemptedBuiltCount;
            public int BandEvaluated;
            public int TopologyEvaluated;
            public int FaceQualityEvaluated;
            public int BandValid = -1;
            public int TopologyValid = -1;
            public int FaceQualityValid = -1;
            public int SurfaceValid = -1;
            public int MeshValid = -1;
            public int FullyValid = -1;
            public int OpenEdgeCount;
            public int NonManifoldEdgeCount;
            public int TJunctionCount;
            public int InvalidFaceCount;
            public int NonPlanarFaceCount;
            public float MaximumPlaneDeviation;
            public float MaximumNormalSpreadDegrees;
            public string FailureStage = string.Empty;
            public string FailureCause = string.Empty;
            public string Result = string.Empty;
            public readonly List<PlaneCutOpenEdgeFailureRecord>
                OpenEdgeFailures =
                    new List<PlaneCutOpenEdgeFailureRecord>();
            public readonly List<PlaneCutFaceQualityFailureRecord>
                NonPlanarFaceFailures =
                    new List<PlaneCutFaceQualityFailureRecord>();
            public readonly List<PlaneCutTJunctionFailureRecord>
                TJunctionFailures =
                    new List<PlaneCutTJunctionFailureRecord>();
        }

        private readonly struct PlaneCutDiagnosticSegment
        {
            public readonly int FaceIndex;
            public readonly PolygonFaceProvenanceKind ProvenanceKind;
            public readonly int ProvenanceIndex;
            public readonly int SegmentIndex;
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly VertexKey StartKey;
            public readonly VertexKey EndKey;

            public PlaneCutDiagnosticSegment(
                int faceIndex,
                PolygonFaceProvenanceKind provenanceKind,
                int provenanceIndex,
                int segmentIndex,
                Vector3 start,
                Vector3 end)
            {
                FaceIndex = faceIndex;
                ProvenanceKind = provenanceKind;
                ProvenanceIndex = provenanceIndex;
                SegmentIndex = segmentIndex;
                Start = start;
                End = end;
                StartKey = new VertexKey(start);
                EndKey = new VertexKey(end);
            }
        }

        private readonly struct PlaneCutBoundarySplit
        {
            public readonly float Parameter;
            public readonly Vector3 Position;

            public PlaneCutBoundarySplit(float parameter, Vector3 position)
            {
                Parameter = parameter;
                Position = position;
            }
        }

        private readonly struct PlaneCutOpenEdgeRecord
        {
            public readonly int FaceIndex;
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly VertexKey StartKey;
            public readonly VertexKey EndKey;
            public readonly EdgeKey EdgeKey;

            public PlaneCutOpenEdgeRecord(
                int faceIndex,
                Vector3 start,
                Vector3 end)
            {
                FaceIndex = faceIndex;
                Start = start;
                End = end;
                StartKey = new VertexKey(start);
                EndKey = new VertexKey(end);
                EdgeKey = new EdgeKey(start, end);
            }
        }

        private readonly struct PlaneCutStageSnapshot
        {
            public readonly string Stage;
            public readonly int FaceCount;
            public readonly int VertexCount;
            public readonly int UniqueVertexCount;
            public readonly int BevelFaceCount;
            public readonly int JunctionFaceCount;
            public readonly int OpenEdgeCount;
            public readonly int NonManifoldEdgeCount;
            public readonly int TJunctionCount;
            public readonly int InvalidFaceCount;
            public readonly int NonPlanarFaceCount;
            public readonly float MaximumPlaneDeviation;
            public readonly float MaximumNormalSpreadDegrees;

            public PlaneCutStageSnapshot(
                string stage,
                int faceCount,
                int vertexCount,
                int uniqueVertexCount,
                int bevelFaceCount,
                int junctionFaceCount,
                int openEdgeCount,
                int nonManifoldEdgeCount,
                int tJunctionCount,
                int invalidFaceCount,
                int nonPlanarFaceCount,
                float maximumPlaneDeviation,
                float maximumNormalSpreadDegrees)
            {
                Stage = stage;
                FaceCount = faceCount;
                VertexCount = vertexCount;
                UniqueVertexCount = uniqueVertexCount;
                BevelFaceCount = bevelFaceCount;
                JunctionFaceCount = junctionFaceCount;
                OpenEdgeCount = openEdgeCount;
                NonManifoldEdgeCount = nonManifoldEdgeCount;
                TJunctionCount = tJunctionCount;
                InvalidFaceCount = invalidFaceCount;
                NonPlanarFaceCount = nonPlanarFaceCount;
                MaximumPlaneDeviation = maximumPlaneDeviation;
                MaximumNormalSpreadDegrees =
                    maximumNormalSpreadDegrees;
            }
        }

        private readonly struct PlaneCutFaceQualityFailureRecord
        {
            public readonly int FaceIndex;
            public readonly PolygonFaceProvenanceKind ProvenanceKind;
            public readonly int ProvenanceIndex;
            public readonly int VertexCount;
            public readonly Vector3 AuthoredNormal;
            public readonly Vector3 MeasuredNormal;
            public readonly float PlaneDistance;
            public readonly float MaximumPlaneDeviation;
            public readonly float PlanarityTolerance;
            public readonly int OffendingVertexIndex;
            public readonly Vector3 OffendingVertexPosition;
            public readonly float OffendingSignedResidual;
            public readonly float MaximumNormalSpreadDegrees;
            public readonly float NormalSpreadToleranceDegrees;
            public readonly int OffendingSegmentIndex;
            public readonly Vector3 OffendingTriangleNormal;
            public readonly float Area;
            public readonly float MinimumEdgeLength;
            public readonly string FirstFailureStage;
            public readonly int BoundaryConformityTouched;
            public readonly int SeamRepairTouched;
            public readonly float SeamRepairMaximumMovement;
            public readonly string VertexResidualEvidence;
            public readonly string Cause;

            public PlaneCutFaceQualityFailureRecord(
                int faceIndex,
                PolygonFaceProvenanceKind provenanceKind,
                int provenanceIndex,
                int vertexCount,
                Vector3 authoredNormal,
                Vector3 measuredNormal,
                float planeDistance,
                float maximumPlaneDeviation,
                float planarityTolerance,
                int offendingVertexIndex,
                Vector3 offendingVertexPosition,
                float offendingSignedResidual,
                float maximumNormalSpreadDegrees,
                float normalSpreadToleranceDegrees,
                int offendingSegmentIndex,
                Vector3 offendingTriangleNormal,
                float area,
                float minimumEdgeLength,
                string firstFailureStage,
                int boundaryConformityTouched,
                int seamRepairTouched,
                float seamRepairMaximumMovement,
                string vertexResidualEvidence,
                string cause)
            {
                FaceIndex = faceIndex;
                ProvenanceKind = provenanceKind;
                ProvenanceIndex = provenanceIndex;
                VertexCount = vertexCount;
                AuthoredNormal = authoredNormal;
                MeasuredNormal = measuredNormal;
                PlaneDistance = planeDistance;
                MaximumPlaneDeviation = maximumPlaneDeviation;
                PlanarityTolerance = planarityTolerance;
                OffendingVertexIndex = offendingVertexIndex;
                OffendingVertexPosition = offendingVertexPosition;
                OffendingSignedResidual = offendingSignedResidual;
                MaximumNormalSpreadDegrees =
                    maximumNormalSpreadDegrees;
                NormalSpreadToleranceDegrees =
                    normalSpreadToleranceDegrees;
                OffendingSegmentIndex = offendingSegmentIndex;
                OffendingTriangleNormal = offendingTriangleNormal;
                Area = area;
                MinimumEdgeLength = minimumEdgeLength;
                FirstFailureStage = firstFailureStage;
                BoundaryConformityTouched =
                    boundaryConformityTouched;
                SeamRepairTouched = seamRepairTouched;
                SeamRepairMaximumMovement =
                    seamRepairMaximumMovement;
                VertexResidualEvidence = vertexResidualEvidence;
                Cause = cause;
            }
        }

        private readonly struct PlaneCutOpenEdgeFailureRecord
        {
            public readonly int RecordIndex;
            public readonly int FaceIndex;
            public readonly PolygonFaceProvenanceKind FaceProvenanceKind;
            public readonly int FaceProvenanceIndex;
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly float Length;
            public readonly int AssociatedSourceVertex;
            public readonly Vector3 AssociatedSourcePosition;
            public readonly float SourceVertexDistance;
            public readonly string IncidentBuiltEdges;
            public readonly int JunctionExpected;
            public readonly int JunctionFaceCount;
            public readonly string ExpectedNeighbour;
            public readonly int NearestFaceIndex;
            public readonly PolygonFaceProvenanceKind
                NearestFaceProvenanceKind;
            public readonly int NearestFaceProvenanceIndex;
            public readonly Vector3 NearestSegmentStart;
            public readonly Vector3 NearestSegmentEnd;
            public readonly float NearestReversedEndpointDistance;
            public readonly string FirstFailureStage;
            public readonly string Cause;

            public PlaneCutOpenEdgeFailureRecord(
                int recordIndex,
                int faceIndex,
                PolygonFaceProvenanceKind faceProvenanceKind,
                int faceProvenanceIndex,
                Vector3 start,
                Vector3 end,
                float length,
                int associatedSourceVertex,
                Vector3 associatedSourcePosition,
                float sourceVertexDistance,
                string incidentBuiltEdges,
                int junctionExpected,
                int junctionFaceCount,
                string expectedNeighbour,
                int nearestFaceIndex,
                PolygonFaceProvenanceKind nearestFaceProvenanceKind,
                int nearestFaceProvenanceIndex,
                Vector3 nearestSegmentStart,
                Vector3 nearestSegmentEnd,
                float nearestReversedEndpointDistance,
                string firstFailureStage,
                string cause)
            {
                RecordIndex = recordIndex;
                FaceIndex = faceIndex;
                FaceProvenanceKind = faceProvenanceKind;
                FaceProvenanceIndex = faceProvenanceIndex;
                Start = start;
                End = end;
                Length = length;
                AssociatedSourceVertex = associatedSourceVertex;
                AssociatedSourcePosition = associatedSourcePosition;
                SourceVertexDistance = sourceVertexDistance;
                IncidentBuiltEdges = incidentBuiltEdges;
                JunctionExpected = junctionExpected;
                JunctionFaceCount = junctionFaceCount;
                ExpectedNeighbour = expectedNeighbour;
                NearestFaceIndex = nearestFaceIndex;
                NearestFaceProvenanceKind =
                    nearestFaceProvenanceKind;
                NearestFaceProvenanceIndex =
                    nearestFaceProvenanceIndex;
                NearestSegmentStart = nearestSegmentStart;
                NearestSegmentEnd = nearestSegmentEnd;
                NearestReversedEndpointDistance =
                    nearestReversedEndpointDistance;
                FirstFailureStage = firstFailureStage;
                Cause = cause;
            }
        }

        private sealed class PlaneCutNumericalRepairTelemetry
        {
            public int IntersectionRequestCount;
            public int StrictCrossingIntersectionCount;
            public int ProjectedFallbackIntersectionCount;
            public int CachedIntersectionReuseCount;
            public int CachedIntersectionInvalidationCount;
            public int CachedIntersectionRecomputeSuccessCount;
            public int IntersectionProjectionCount;
            public float MaximumIntersectionProjectionDistance;
            public int StrictInsideClassificationCount;
            public int StrictOnPlaneClassificationCount;
            public int StrictOutsideClassificationCount;
            public int OnPlaneSnapCount;
            public float MaximumOnPlaneSnapDistance;
            public int SameSideFallbackAttemptCount;
            public int ExactConstructionFailureCount;
            public float MaximumCutPlaneResidualBeforeCorrection;
            public float MaximumCutPlaneResidualAfterCorrection;
            public float MaximumOwnerPlaneResidualBeforeCorrection;
            public float MaximumOwnerPlaneResidualAfterCorrection;
            public int CapVertexProjectionCount;
            public int CapVertexValidationCount;
            public float MaximumCapResidualBeforeProjection;
            public float MaximumCapResidualAfterProjection;
            public int CapResidualRejectCount;
            public int DistanceWeldComparisonCount;
            public int DistanceWeldMatchCount;
            public int DistanceWeldMovedCount;
            public float MaximumDistanceWeldMovement;
            public int FirstExactFailureRecorded;
            public PolygonFaceProvenanceKind
                FirstExactFailureOwnerProvenanceKind;
            public int FirstExactFailureOwnerProvenanceIndex;
            public PolygonFaceProvenanceKind
                FirstExactFailureCutProvenanceKind;
            public int FirstExactFailureCutProvenanceIndex;
            public float FirstExactFailureStartDistance;
            public float FirstExactFailureEndDistance;
            public string FirstExactFailureStartClassification;
            public string FirstExactFailureEndClassification;
            public float FirstExactFailureCutResidualBefore;
            public float FirstExactFailureCutResidualAfter;
            public float FirstExactFailureOwnerResidualBefore;
            public float FirstExactFailureOwnerResidualAfter;
            public string FirstExactFailureReason;
        }

        private struct PlaneCutBevelAuditResult
        {
            public int SelectedEdgeCount;
            public int ActiveEdgeCount;
            public int PlanesBuilt;
            public int AttemptedPlanesBuilt;
            public int CertifiedPlanesBuilt;
            public int TrialRejectedPlanes;
            public int PlanesLocalized;
            public int PlanesDeferred;
            public int PlanesRejected;
            public int VertexJunctionCandidateCount;
            public int VertexJunctionDirectBuiltCount;
            public int VertexJunctionAdaptiveBuiltCount;
            public int VertexJunctionBacktrackBuiltCount;
            public int VertexJunctionCleanSharpCount;
            public int VertexJunctionUnresolvedCount;
            public int VertexJunctionTriangleCapCount;
            public int VertexJunctionQuadCapCount;
            public int VertexJunctionLargerCapCount;
            public int VertexJunctionEdgesDeferredCount;
            public int VertexJunctionRebuildPassCount;
            public int CapsBuilt;
            public int CapsMissing;
            public int CapsRedundant;
            public int ConformalSplitCount;
            public int SeamPairCount;
            public int OpenEdgeCount;
            public int NonManifoldEdgeCount;
            public int TJunctionCount;
            public int InvalidFaceCount;
            public int PreviewTriangleCount;
            public int PreviewDegenerateTriangleCount;
            public int PreviewOpenEdgeCount;
            public int PreviewNonManifoldEdgeCount;
            public int PreviewWindingFailureCount;
            public int PreviewBoundsFailureCount;
            public int PreviewVolumeFailureCount;
            public int PreviewGeometryValid;
            public int SolveStatesEvaluated;
            public int SolveJunctionsVisited;
            public int SolveCandidateTrials;
            public int SolveSystemRebuilds;
            public int SolvePolygonAudits;
            public int SolveTriangleAudits;
            public int SolveEdgesDeferred;
            public long SolveElapsedMilliseconds;
            public int SolveTimedOut;
            public int FaceQualityFaceCount;
            public int FaceQualitySeamTouchedFaceCount;
            public int FaceQualityNonPlanarCount;
            public int FaceQualityElongatedJunctionCount;
            public float FaceQualityMaxPlaneDeviation;
            public float FaceQualityMaxNormalSpreadDegrees;
            public float FaceQualityMinimumJunctionCompactness;
            public float FaceQualityMaximumJunctionAspectRatio;
            public int FaceQualityWorstVertexCount;
            public int BandRetainedEdgeCount;
            public int BandSingleFaceCount;
            public int BandSplitCount;
            public int BandInterruptedCount;
            public int BandForeignCutCount;
            public int BandOverlongJunctionCount;
            public int BandCollapsedCount;
            public float BandMinimumCoverageRatio;
            public float BandMaximumJunctionInfluenceRatio;
            public float BandMaximumSharedAxisSpanRatio;
            public int EdgeConflictPassCount;
            public int EdgeConflictEdgesDeferredCount;
            public int EdgeConflictResolvedCount;
            public int EdgeConflictBudgetExhausted;
            public int EdgeConflictWidthReductionCount;
            public int EdgeConflictClusterCount;
            public int EdgeConflictUnresolvedCount;
            public int CoexistenceStarEvaluationCount;
            public int CoexistenceStarCacheUseCount;
            public int CoexistencePairEvaluationCount;
            public int CoexistencePairCacheUseCount;
            public int CoexistenceTrialCount;
            public int CoexistenceTrialCacheUseCount;
            public int CoexistenceExclusionCount;
            public int CoexistenceSearchStatesEvaluated;
            public int CoexistenceSearchStatesDeduplicated;
            public int CoexistenceSearchMaximumDepth;
            public int CoexistenceSearchFrontierRemaining;
            public int CoexistenceSearchWinningDepth;
            public int CoexistenceCandidateConservationFailureCount;
            public string CoexistenceCandidateExpectedEvidence;
            public string CoexistenceCandidateActualEvidence;
            public string CoexistenceCandidateMissingEvidence;
            public string CoexistenceCandidateUnexpectedEvidence;
            public List<PlaneCutCoexistenceSearchStateRecord>
                CoexistenceSearchStates;
            public float CoexistenceMinimumCommittedWidthScale;
            public string CoexistenceExcludedEdgeEvidence;
            public string CoexistenceExclusionReasonEvidence;
            public int EdgeConflictTopologyRejectedPassCount;
            public int EdgeConflictTopologyExpandedClusterCount;
            public int EdgeConflictTopologyRollbackCount;
            public float EdgeConflictMinimumWidthScale;
            public List<PlaneCutConflictWidthReductionRecord>
                EdgeConflictWidthReductions;
            public int EdgeConflictVictimEdgeIndex;
            public int EdgeConflictForeignEdgeIndex;
            public int EdgeConflictVertexIndex;
            public int EdgeConflictDeferredEdgeIndex;
            public float EdgeConflictVictimCoverageRatio;
            public float EdgeConflictForeignAxialParameter;
            public float EdgeConflictForeignSharedSpanRatio;
            public int LocalJunctionCandidateCount;
            public int LocalJunctionStarsExtractedCount;
            public int LocalJunctionClosedLoopCount;
            public int LocalJunctionBranchedCount;
            public int LocalJunctionSelfIntersectingCount;
            public int LocalJunctionForeignFaceCount;
            public int LocalJunctionMissingIncidentBevelCount;
            public int LocalJunctionDuplicateIncidentBevelCount;
            public int LocalJunctionMinimumLoopVertexCount;
            public int LocalJunctionMaximumLoopVertexCount;
            public float LocalJunctionMaximumExtentRatio;
            public int BevelRegionFaceCount;
            public int BevelRegionBoundaryVertexCount;
            public int BevelRegionTriangleCount;
            public int BevelRegionAuthoredNormalTriangleCount;
            public int BevelRegionAuthoredSurfaceGroupTriangleCount;
            public int BevelRegionInternalFanVertexCount;
            public float BevelRegionMaximumPlaneResidual;
            public float BevelRegionMaximumNormalDeviationDegrees;
            public int BevelRegionRenderValid;
            public int MaterializedEdgeCoverageValid;
            public string ActiveEdgeEvidence;
            public string AttemptedEdgeEvidence;
            public string BuiltEdgeEvidence;
            public string TrialRejectedEdgeEvidence;
            public string DeferredEdgeEvidence;
            public PlaneCutSolverTransactionState LatestAttemptedState;
            public PlaneCutSolverTransactionState LatestBandCleanState;
            public PlaneCutSolverTransactionState LatestTopologyCleanState;
            public PlaneCutSolverTransactionState LatestCertifiedState;
            public List<PlaneCutRetryFailureDossier> RetryFailureDossiers;
            public List<PlaneCutTopologyScaleTrialRecord>
                TopologyScaleTrials;
            public int TopologyScaleSearchBasePass;
            public string TopologyScaleSearchMode;
            public string TopologyScaleSearchTriggerEvidence;
            public string TopologyScaleSearchTopologyLinkedEvidence;
            public string TopologyScaleSearchClusterEvidence;
            public string TopologyScaleSearchProtectedEvidence;
            public string ActiveSearchFailureStage;
            public string ActiveSearchFailureCause;
            public string ActiveSearchFailureEvidence;
            public List<int> DebugFocusEdgeIndices;
            public int TopologyScaleSearchTrialCount;
            public int TopologyScaleSearchBandFailureCount;
            public int TopologyScaleSearchTopologyFailureCount;
            public int TopologyScaleSearchFaceQualityFailureCount;
            public int TopologyScaleSearchCollateralFailureCount;
            public float TopologyScaleSearchCommittedFactor;
            public float TopologyScaleSearchHighestValidFactor;
            public string TopologyScaleSearchCollateralChangedEvidence;
            public int TopologyScaleSearchFailedStateScalesReused;
            public int TopologyScaleSearchUnresolved;
            public PlaneCutStageSnapshot StagePlaneConstruction;
            public PlaneCutStageSnapshot StageSanitized;
            public PlaneCutStageSnapshot StageWelded;
            public PlaneCutStageSnapshot StageConformed;
            public PlaneCutStageSnapshot StageSeamRepaired;
            public PlaneCutStageSnapshot StageFinalCertification;
            public string FirstOpenEdgeStage;
            public string FirstTJunctionStage;
            public string FirstNonPlanarStage;
            public float FaceQualityPlanarityTolerance;
            public float FaceQualityNormalSpreadToleranceDegrees;
            public List<PlaneCutFaceQualityFailureRecord>
                FaceQualityFailures;
            public List<PlaneCutOpenEdgeFailureRecord> OpenEdgeFailures;
            public List<PlaneCutTJunctionFailureRecord> TJunctionFailures;
            public List<PlaneCutLocalityDeferralRecord>
                LocalityDeferrals;
            public Dictionary<string, string>
                FaceFirstNonPlanarStageByIdentity;
            public HashSet<string> BoundaryConformityTouchedFaces;
            public HashSet<string> SeamRepairTouchedFaces;
            public Dictionary<string, float>
                SeamRepairMaximumMovementByIdentity;
            public PlaneCutNumericalRepairTelemetry NumericalRepairs;
            public EdgeWearCoverageAudit CoverageAudit;
            public GeneratedGeometryStableFingerprint
                ExclusionReasonFingerprint;
            public GeneratedGeometryStableFingerprint SelectedEdgeFingerprint;
            public GeneratedGeometryStableFingerprint CertifiedEdgeFingerprint;
            public GeneratedGeometryStableFingerprint GeometryTopologyFingerprint;
            public int StableFingerprintPrepared;
            public int GeometryValid;
            public string Diagnostic;
        }

        private static PlaneCutBevelAuditResult AuditPlaneCutBevelKernel(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            ChamferCornerSolution solution,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            EdgeWearCoverageAudit coverageAudit,
            out TriangleSoup previewSoup)
        {
            previewSoup = null;
            PlaneCutBevelAuditResult result =
                new PlaneCutBevelAuditResult
                {
                    SelectedEdgeCount = context.SelectedSourceEdges.Count,
                    CoverageAudit = coverageAudit,
                    EdgeConflictVictimEdgeIndex = -1,
                    EdgeConflictForeignEdgeIndex = -1,
                    EdgeConflictVertexIndex = -1,
                    EdgeConflictDeferredEdgeIndex = -1,
                    EdgeConflictMinimumWidthScale = 1f,
                    CoexistenceMinimumCommittedWidthScale = 1f,
                    CoexistenceSearchWinningDepth = -1,
                    CoexistenceCandidateExpectedEvidence = "none",
                    CoexistenceCandidateActualEvidence = "none",
                    CoexistenceCandidateMissingEvidence = "none",
                    CoexistenceCandidateUnexpectedEvidence = "none",
                    CoexistenceSearchStates =
                        new List<PlaneCutCoexistenceSearchStateRecord>(),
                    CoexistenceExcludedEdgeEvidence = "none",
                    CoexistenceExclusionReasonEvidence = "none",
                    EdgeConflictWidthReductions =
                        new List<PlaneCutConflictWidthReductionRecord>(),
                    FaceQualityFailures =
                        new List<PlaneCutFaceQualityFailureRecord>(),
                    OpenEdgeFailures =
                        new List<PlaneCutOpenEdgeFailureRecord>(),
                    TJunctionFailures =
                        new List<PlaneCutTJunctionFailureRecord>(),
                    LocalityDeferrals =
                        new List<PlaneCutLocalityDeferralRecord>(),
                    RetryFailureDossiers =
                        new List<PlaneCutRetryFailureDossier>(),
                    TopologyScaleTrials =
                        new List<PlaneCutTopologyScaleTrialRecord>(),
                    TopologyScaleSearchBasePass = -1,
                    TopologyScaleSearchCommittedFactor = -1f,
                    TopologyScaleSearchHighestValidFactor = -1f,
                    TopologyScaleSearchMode = "none",
                    TopologyScaleSearchTriggerEvidence = "none",
                    TopologyScaleSearchTopologyLinkedEvidence = "none",
                    TopologyScaleSearchClusterEvidence = "none",
                    TopologyScaleSearchProtectedEvidence = "none",
                    ActiveSearchFailureStage = "none",
                    ActiveSearchFailureCause = "none",
                    ActiveSearchFailureEvidence = "none",
                    DebugFocusEdgeIndices = new List<int>(),
                    TopologyScaleSearchCollateralChangedEvidence = "none",
                    FaceFirstNonPlanarStageByIdentity =
                        new Dictionary<string, string>(),
                    BoundaryConformityTouchedFaces =
                        new HashSet<string>(),
                    SeamRepairTouchedFaces =
                        new HashSet<string>(),
                    SeamRepairMaximumMovementByIdentity =
                        new Dictionary<string, float>(),
                    NumericalRepairs =
                        new PlaneCutNumericalRepairTelemetry()
                };

            List<EdgeWearSelectedGraphEdge> orderedSelected =
                new List<EdgeWearSelectedGraphEdge>(context.SelectedEdges);
            orderedSelected.Sort((left, right) =>
                left.GraphEdgeIndex.CompareTo(right.GraphEdgeIndex));

            List<PlaneCutBevelCandidate> planeCandidates =
                new List<PlaneCutBevelCandidate>(orderedSelected.Count);
            List<int> activeEdgeIndices =
                new List<int>(orderedSelected.Count);
            int localityDeferredCount = 0;
            for (int selectedIndex = 0;
                 selectedIndex < orderedSelected.Count;
                 selectedIndex++)
            {
                EdgeWearSelectedGraphEdge selected =
                    orderedSelected[selectedIndex];
                TryGetEdgeWearCoverageRecord(
                    coverageAudit,
                    selected.GraphEdgeIndex,
                    out EdgeWearEdgeLifecycleRecord lifecycle);
                if (!solution.WidthByEdge.TryGetValue(
                        selected.GraphEdgeIndex,
                        out float width) ||
                    width <= PointMergeDistance)
                {
                    if (lifecycle != null)
                    {
                        bool hasInactiveWidth =
                            solution.WidthByEdge.TryGetValue(
                                selected.GraphEdgeIndex,
                                out float inactiveWidth);
                        SetEdgeWearCornerWidthCoexistenceIneligibility(
                            lifecycle,
                            hasInactiveWidth ? inactiveWidth : 0f,
                            hasInactiveWidth
                                ? "corner-width-inactive"
                                : "corner-width-missing");
                    }
                    continue;
                }

                result.ActiveEdgeCount++;
                activeEdgeIndices.Add(selected.GraphEdgeIndex);
                if (lifecycle != null)
                {
                    lifecycle.SolvedWidth = width;
                    lifecycle.WidthInactive = false;
                    lifecycle.Active = true;
                    lifecycle.FinalReason = "active";
                }

                EdgeWearEdgeViabilityRecord cachedViability =
                    lifecycle != null ? lifecycle.Viability : null;
                if (coverageAudit != null)
                {
                    if (cachedViability != null &&
                        cachedViability.Evaluated &&
                        cachedViability.LocalityValid)
                    {
                        coverageAudit.ViabilityLocalityCacheUseCount++;
                    }
                    else
                    {
                        coverageAudit.ViabilityLocalityCacheMissCount++;
                    }
                }
                if (!TryBuildPlaneCutBevelCandidate(
                        context,
                        solution,
                        selected,
                        cachedViability,
                        minimumStableEdgeLength,
                        out PlaneCutBevelCandidate candidate,
                        out bool localityDeferred,
                        out PlaneCutLocalityDeferralRecord localityEvidence,
                        out string blocker))
                {
                    if (localityDeferred)
                    {
                        localityDeferredCount++;
                        if (localityEvidence != null)
                        {
                            result.LocalityDeferrals.Add(localityEvidence);
                        }
                        if (lifecycle != null)
                        {
                            lifecycle.Deferred = true;
                            lifecycle.FinalReason = string.IsNullOrEmpty(blocker)
                                ? "plane-locality-deferred"
                                : "plane-locality-deferred:" + blocker;
                        }
                    }
                    else
                    {
                        result.PlanesRejected++;
                        if (lifecycle != null)
                        {
                            lifecycle.Rejected = true;
                            lifecycle.FinalReason = string.IsNullOrEmpty(blocker)
                                ? "plane-rejected"
                                : "plane-rejected:" + blocker;
                        }
                        SetPlaneCutBevelDiagnostic(
                            ref result.Diagnostic,
                            blocker);
                    }
                    continue;
                }

                planeCandidates.Add(candidate);
            }

            RecalculateEdgeWearCoverageAudit(coverageAudit);
            if (coverageAudit != null)
            {
                result.SelectedEdgeCount = coverageAudit.SelectedCount;
                result.ActiveEdgeCount = coverageAudit.ActiveCount;
                SynchronizePlaneCutCoexistenceExclusionEvidence(
                    coverageAudit,
                    ref result);
            }

            if (result.ActiveEdgeCount == 0)
            {
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    "no active positive-width selected edge");
                return result;
            }

            if (planeCandidates.Count == 0)
            {
                result.PlanesDeferred = localityDeferredCount;
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    localityDeferredCount == result.ActiveEdgeCount
                        ? "all active bevel planes were safely deferred"
                        : "no valid bevel cut plane");
                return result;
            }

            Bounds sourceBounds = CalculateFaceBounds(sourceFaces);
            double sourceVolume = CalculatePlaneCutPolyhedronVolume(sourceFaces);
            List<PlaneCutVertexJunctionCandidate> noJunctions =
                new List<PlaneCutVertexJunctionCandidate>();

            if (!TryBuildCleanPlaneCutEdgeOnlyShell(
                    sourceFaces,
                    context,
                    planeCandidates,
                    noJunctions,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    localityDeferredCount,
                    coverageAudit != null &&
                        coverageAudit.MaximumCoverageMode,
                    ref result,
                    out List<PlaneCutBevelCandidate> retainedCandidates,
                    out List<PolygonFace> edgeOnlyFaces,
                    out string edgeOnlyBlocker))
            {
                FinalizeEdgeWearCoverageAfterFailedPlaneShell(
                    coverageAudit,
                    retainedCandidates,
                    string.IsNullOrEmpty(edgeOnlyBlocker)
                        ? "edge-only-shell-build-failed"
                        : edgeOnlyBlocker);
                result.ActiveEdgeEvidence =
                    FormatPlaneCutEdgeIndexEvidence(activeEdgeIndices);
                result.AttemptedEdgeEvidence =
                    FormatPlaneCutCandidateEdgeEvidence(
                        retainedCandidates);
                result.BuiltEdgeEvidence = "none";
                result.TrialRejectedEdgeEvidence =
                    result.AttemptedEdgeEvidence;
                result.DeferredEdgeEvidence =
                    FormatPlaneCutDeferredEdgeEvidence(
                        activeEdgeIndices,
                        retainedCandidates);
                result.MaterializedEdgeCoverageValid = 0;
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    string.IsNullOrEmpty(edgeOnlyBlocker)
                        ? "the deterministic clean-band edge-only shell could not be built"
                        : edgeOnlyBlocker);
                return result;
            }

            planeCandidates = retainedCandidates;
            result.PlanesBuilt = planeCandidates.Count;
            result.AttemptedPlanesBuilt = planeCandidates.Count;
            result.CertifiedPlanesBuilt = 0;
            result.TrialRejectedPlanes = planeCandidates.Count;
            result.ActiveEdgeEvidence =
                FormatPlaneCutCandidateEdgeEvidence(planeCandidates);
            result.AttemptedEdgeEvidence =
                FormatPlaneCutCandidateEdgeEvidence(planeCandidates);
            result.BuiltEdgeEvidence = "none";
            result.TrialRejectedEdgeEvidence =
                result.AttemptedEdgeEvidence;
            result.DeferredEdgeEvidence =
                result.CoexistenceExclusionCount > 0
                    ? "none"
                    : FormatPlaneCutDeferredEdgeEvidence(
                        activeEdgeIndices,
                        planeCandidates);
            result.MaterializedEdgeCoverageValid = 0;

            for (int candidateIndex = 0;
                 candidateIndex < planeCandidates.Count;
                 candidateIndex++)
            {
                PlaneCutBevelCandidate candidate =
                    planeCandidates[candidateIndex];
                int capCount = CountMatchingPlaneCutCaps(
                    edgeOnlyFaces,
                    candidate);
                if (capCount == 1)
                {
                    continue;
                }

                if (capCount == 0 &&
                    IsPlaneCutCandidateRedundant(
                        edgeOnlyFaces,
                        candidate))
                {
                    result.CapsRedundant++;
                    continue;
                }

                result.CapsMissing++;
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    capCount == 0
                        ? "an edge-only bevel plane has no surviving cap and is not redundant"
                        : "an edge-only bevel plane retains duplicate caps");
            }

            AuditPlaneCutFaceQuality(
                edgeOnlyFaces,
                noJunctions,
                minimumStableEdgeLength,
                ref result);
            AuditPlaneCutLocalJunctionStars(
                edgeOnlyFaces,
                context,
                planeCandidates,
                minimumStableEdgeLength,
                ref result,
                out _);
            if (result.FaceQualityNonPlanarCount > 0)
            {
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    "an edge-only bevel face exceeds certified planarity");
            }

            EdgeWearTopologyStats topology = AuditEdgeWearTopology(
                edgeOnlyFaces,
                minimumStableEdgeLength);
            result.OpenEdgeCount = topology.OpenEdgeCount;
            result.NonManifoldEdgeCount = topology.NonManifoldEdgeCount;
            result.TJunctionCount = topology.TJunctionCount;
            result.InvalidFaceCount += CountInvalidPlaneCutFaces(
                edgeOnlyFaces,
                minimumStableFaceArea);
            CapturePlaneCutStageSnapshot(
                edgeOnlyFaces,
                "FinalCertification",
                minimumStableEdgeLength,
                minimumStableFaceArea,
                planeCandidates,
                ref result,
                out result.StageFinalCertification);
            AuditPlaneCutOpenEdgeFailures(
                edgeOnlyFaces,
                context,
                planeCandidates,
                minimumStableEdgeLength,
                ref result);
            double resultVolume =
                CalculatePlaneCutPolyhedronVolume(edgeOnlyFaces);
            bool volumeValid = sourceVolume > 0.000000001 &&
                resultVolume > sourceVolume * 0.75 &&
                resultVolume <= sourceVolume * 1.0001;
            bool boundsValid = ArePlaneCutBoundsContained(
                sourceBounds,
                CalculateFaceBounds(edgeOnlyFaces),
                Mathf.Max(
                    PlaneEpsilon * 1.25f,
                    Mathf.Max(
                        PointMergeDistance * 8f,
                        minimumStableEdgeLength * 0.02f)));

            if (!volumeValid)
            {
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    "the edge-only plane-cut clone has an invalid retained volume");
            }
            if (!boundsValid)
            {
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    "the edge-only plane-cut clone exceeds the source bounds");
            }
            if (result.OpenEdgeCount > 0)
            {
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    "the edge-only plane-cut clone has open edges");
            }
            if (result.NonManifoldEdgeCount > 0)
            {
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    "the edge-only plane-cut clone has non-manifold edges");
            }
            if (result.TJunctionCount > 0)
            {
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    "the edge-only plane-cut clone has T-junctions");
            }
            if (result.InvalidFaceCount > 0)
            {
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    "the edge-only plane-cut clone has invalid faces");
            }

            bool polygonGeometryValid =
                result.PlanesRejected == 0 &&
                result.PlanesBuilt > 0 &&
                result.PlanesBuilt + result.PlanesDeferred ==
                    result.ActiveEdgeCount &&
                result.BandRetainedEdgeCount == result.PlanesBuilt &&
                result.BandSingleFaceCount == result.PlanesBuilt &&
                result.CapsMissing == 0 &&
                result.OpenEdgeCount == 0 &&
                result.NonManifoldEdgeCount == 0 &&
                result.TJunctionCount == 0 &&
                result.InvalidFaceCount == 0 &&
                result.FaceQualityNonPlanarCount == 0 &&
                result.BandSplitCount == 0 &&
                result.BandInterruptedCount == 0 &&
                result.BandForeignCutCount == 0 &&
                result.BandOverlongJunctionCount == 0 &&
                result.BandCollapsedCount == 0 &&
                result.EdgeConflictBudgetExhausted == 0 &&
                volumeValid &&
                boundsValid &&
                edgeOnlyFaces.Count >= 4;

            TriangleSoup auditedSoup = null;
            bool surfaceTriangulationValid = false;
            if (polygonGeometryValid)
            {
                BoundedSingleEdgeAuditResult surfaceAudit =
                    new BoundedSingleEdgeAuditResult
                    {
                        TriangulationFailureFace = -1,
                        TriangulationFailureProvenanceIndex = -1,
                        BevelRegionFailureFace = -1,
                        BevelRegionFailureProvenanceIndex = -1
                    };
                surfaceTriangulationValid =
                    TryTriangulateBoundedPreviewFaces(
                        edgeOnlyFaces,
                        minimumStableFaceArea,
                        ref surfaceAudit,
                        out auditedSoup,
                        out string surfaceBlocker);
                result.BevelRegionFaceCount =
                    surfaceAudit.BevelRegionFaceCount;
                result.BevelRegionBoundaryVertexCount =
                    surfaceAudit.BevelRegionBoundaryVertexCount;
                result.BevelRegionTriangleCount =
                    surfaceAudit.BevelRegionTriangleCount;
                result.BevelRegionAuthoredNormalTriangleCount =
                    surfaceAudit.BevelRegionAuthoredNormalTriangleCount;
                result.BevelRegionAuthoredSurfaceGroupTriangleCount =
                    surfaceAudit.BevelRegionAuthoredSurfaceGroupTriangleCount;
                result.BevelRegionInternalFanVertexCount =
                    surfaceAudit.BevelRegionInternalFanVertexCount;
                result.BevelRegionMaximumPlaneResidual =
                    surfaceAudit.BevelRegionMaximumPlaneResidual;
                result.BevelRegionMaximumNormalDeviationDegrees =
                    surfaceAudit.BevelRegionMaximumNormalDeviationDegrees;
                result.BevelRegionRenderValid =
                    surfaceAudit.BevelRegionRenderValid;
                if (!surfaceTriangulationValid)
                {
                    SetPlaneCutBevelDiagnostic(
                        ref result.Diagnostic,
                        string.IsNullOrEmpty(surfaceBlocker)
                            ? "the all-edge bevel shell failed the one-surface render contract"
                            : surfaceBlocker);
                }
            }
            if (auditedSoup != null)
            {
                AuditPlaneCutPreviewTriangleSoup(
                    auditedSoup,
                    edgeOnlyFaces,
                    minimumStableEdgeLength,
                    ref result);
            }

            bool geometryValid = polygonGeometryValid &&
                surfaceTriangulationValid &&
                result.BevelRegionFaceCount == result.PlanesBuilt &&
                result.BevelRegionRenderValid == 1 &&
                result.PreviewGeometryValid == 1;
            result.GeometryValid = geometryValid ? 1 : 0;
            if (geometryValid)
            {
                FinalizeEdgeWearCoverageAfterPlaneShell(
                    coverageAudit,
                    planeCandidates);
                result.CertifiedPlanesBuilt = planeCandidates.Count;
                result.TrialRejectedPlanes = 0;
                result.BuiltEdgeEvidence =
                    result.AttemptedEdgeEvidence;
                result.TrialRejectedEdgeEvidence = "none";
                if (result.LatestCertifiedState != null)
                {
                    result.LatestCertifiedState.Name =
                        "fully-certified";
                    result.LatestCertifiedState.Stage =
                        result.StageFinalCertification;
                }
            }
            else
            {
                FinalizeEdgeWearCoverageAfterFailedPlaneShell(
                    coverageAudit,
                    planeCandidates,
                    "final-certification-failed");
                result.CertifiedPlanesBuilt = 0;
                result.TrialRejectedPlanes =
                    result.AttemptedPlanesBuilt;
                result.PlanesBuilt = 0;
                result.BuiltEdgeEvidence = "none";
                result.TrialRejectedEdgeEvidence =
                    result.AttemptedEdgeEvidence;
                result.LatestCertifiedState = null;
            }
            result.MaterializedEdgeCoverageValid =
                geometryValid &&
                IsEdgeWearCoverageMaterialized(coverageAudit, result)
                    ? 1
                    : 0;
            bool previewContractValid = geometryValid &&
                result.MaterializedEdgeCoverageValid == 1;
            if (previewContractValid)
            {
                previewSoup = auditedSoup;
            }
            PrepareEdgeWearStableEvaluationFingerprints(
                edgeOnlyFaces,
                ref result);
            return result;
        }

        private static void PrepareEdgeWearStableEvaluationFingerprints(
            List<PolygonFace> finalFaces,
            ref PlaneCutBevelAuditResult result)
        {
            EdgeWearCoverageAudit coverage = result.CoverageAudit;

            GeneratedGeometryStableHashBuilder exclusions =
                GeneratedGeometryStableHashBuilder.Create(
                    "PS3D.GeneratedMass.EdgeWear.Exclusions.v1");
            GeneratedGeometryStableHashBuilder selected =
                GeneratedGeometryStableHashBuilder.Create(
                    "PS3D.GeneratedMass.EdgeWear.SelectedEdges.v1");
            GeneratedGeometryStableHashBuilder certified =
                GeneratedGeometryStableHashBuilder.Create(
                    "PS3D.GeneratedMass.EdgeWear.CertifiedEdges.v1");

            List<EdgeWearEdgeLifecycleRecord> ordered =
                coverage == null
                    ? new List<EdgeWearEdgeLifecycleRecord>()
                    : new List<EdgeWearEdgeLifecycleRecord>(coverage.Records);
            ordered.Sort((left, right) =>
                left.SourceEdgeIndex.CompareTo(right.SourceEdgeIndex));

            int exclusionCount = 0;
            int selectedCount = 0;
            int certifiedCount = 0;
            for (int recordIndex = 0;
                 recordIndex < ordered.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord record = ordered[recordIndex];
                if (record.ViabilityState ==
                        EdgeWearViabilityState.StructuralIneligible ||
                    record.ViabilityState ==
                        EdgeWearViabilityState.GeometricIneligible ||
                    record.ViabilityState ==
                        EdgeWearViabilityState.CoexistenceIneligible)
                {
                    exclusionCount++;
                    exclusions.AddInt32(record.SourceEdgeIndex);
                    exclusions.AddInt32((int)record.ViabilityState);
                    exclusions.AddString(record.FinalReason ?? string.Empty);
                }
                if (record.Selected)
                {
                    selectedCount++;
                    selected.AddInt32(record.SourceEdgeIndex);
                }
                if (record.Built)
                {
                    certifiedCount++;
                    certified.AddInt32(record.SourceEdgeIndex);
                }
            }
            exclusions.AddInt32(exclusionCount);
            selected.AddInt32(selectedCount);
            certified.AddInt32(certifiedCount);

            GeneratedGeometryStableHashBuilder topology =
                GeneratedGeometryStableHashBuilder.Create(
                    "PS3D.GeneratedMass.EdgeWear.GeometryTopology.v1");
            int faceCount = finalFaces == null ? 0 : finalFaces.Count;
            topology.AddInt32(faceCount);
            for (int faceIndex = 0; faceIndex < faceCount; faceIndex++)
            {
                PolygonFace face = finalFaces[faceIndex];
                if (face == null)
                {
                    topology.AddBoolean(false);
                    continue;
                }
                topology.AddBoolean(true);
                topology.AddInt32((int)face.ProvenanceKind);
                topology.AddInt32(face.ProvenanceIndex);
                topology.AddInt32((int)face.Feature);
                topology.AddSingle(face.FeatureStrength);
                topology.AddVector3(face.Normal);
                int vertexCount = face.Vertices == null
                    ? 0
                    : face.Vertices.Count;
                topology.AddInt32(vertexCount);
                for (int vertexIndex = 0;
                     vertexIndex < vertexCount;
                     vertexIndex++)
                {
                    topology.AddVector3(face.Vertices[vertexIndex]);
                }
            }
            topology.AddInt32(result.OpenEdgeCount);
            topology.AddInt32(result.NonManifoldEdgeCount);
            topology.AddInt32(result.TJunctionCount);
            topology.AddInt32(result.InvalidFaceCount);
            topology.AddInt32(result.PreviewTriangleCount);

            result.ExclusionReasonFingerprint = exclusions.Finish();
            result.SelectedEdgeFingerprint = selected.Finish();
            result.CertifiedEdgeFingerprint = certified.Finish();
            result.GeometryTopologyFingerprint = topology.Finish();
            result.StableFingerprintPrepared = 1;
        }

        private static void FinalizeEdgeWearCoverageAfterPlaneShell(
            EdgeWearCoverageAudit audit,
            List<PlaneCutBevelCandidate> retainedCandidates)
        {
            if (audit == null)
            {
                return;
            }

            Dictionary<int, PlaneCutBevelCandidate> builtByEdge =
                new Dictionary<int, PlaneCutBevelCandidate>();
            if (retainedCandidates != null)
            {
                for (int candidateIndex = 0;
                     candidateIndex < retainedCandidates.Count;
                     candidateIndex++)
                {
                    PlaneCutBevelCandidate candidate =
                        retainedCandidates[candidateIndex];
                    builtByEdge[candidate.SourceEdgeIndex] = candidate;
                }
            }

            for (int recordIndex = 0;
                 recordIndex < audit.Records.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord record =
                    audit.Records[recordIndex];
                if (!record.Selected)
                {
                    continue;
                }

                if (builtByEdge.TryGetValue(
                        record.SourceEdgeIndex,
                        out PlaneCutBevelCandidate builtCandidate))
                {
                    record.AttemptedBuilt = true;
                    record.Built = true;
                    record.TrialRejected = false;
                    record.Deferred = false;
                    record.Rejected = false;
                    record.MaterializedWidth = builtCandidate.Width;
                    record.MaterializedWidthScale = record.SolvedWidth >
                            PointMergeDistance
                        ? Mathf.Clamp01(
                            builtCandidate.Width / record.SolvedWidth)
                        : 1f;
                    record.WidthReduced =
                        record.MaterializedWidthScale < 0.9999f;
                    record.FinalReason = record.WidthReduced
                        ? "built-width-reduced"
                        : "built";
                }
                else if (record.Active &&
                    !record.Deferred &&
                    !record.Rejected)
                {
                    record.AttemptedBuilt = false;
                    record.TrialRejected = false;
                    record.MaterializedWidth = 0f;
                    record.MaterializedWidthScale = 0f;
                    record.WidthReduced = false;
                    record.Deferred = true;
                    record.FinalReason = "shell-conflict-deferred";
                }
            }

            RecalculateEdgeWearCoverageAudit(audit);
        }

        private static void FinalizeEdgeWearCoverageAfterFailedPlaneShell(
            EdgeWearCoverageAudit audit,
            List<PlaneCutBevelCandidate> attemptedCandidates,
            string reason)
        {
            if (audit == null)
            {
                return;
            }

            Dictionary<int, PlaneCutBevelCandidate> attemptedByEdge =
                new Dictionary<int, PlaneCutBevelCandidate>();
            if (attemptedCandidates != null)
            {
                for (int candidateIndex = 0;
                     candidateIndex < attemptedCandidates.Count;
                     candidateIndex++)
                {
                    PlaneCutBevelCandidate candidate =
                        attemptedCandidates[candidateIndex];
                    attemptedByEdge[candidate.SourceEdgeIndex] =
                        candidate;
                }
            }

            for (int recordIndex = 0;
                 recordIndex < audit.Records.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord record =
                    audit.Records[recordIndex];
                if (!record.Selected)
                {
                    continue;
                }

                if (attemptedByEdge.TryGetValue(
                        record.SourceEdgeIndex,
                        out PlaneCutBevelCandidate attempted))
                {
                    record.AttemptedBuilt = true;
                    record.Built = false;
                    record.TrialRejected = true;
                    record.Deferred = false;
                    record.Rejected = false;
                    record.MaterializedWidth = attempted.Width;
                    record.MaterializedWidthScale = record.SolvedWidth >
                            PointMergeDistance
                        ? Mathf.Clamp01(
                            attempted.Width / record.SolvedWidth)
                        : 1f;
                    record.WidthReduced =
                        record.MaterializedWidthScale < 0.9999f;
                    record.FinalReason =
                        "solver-trial-rejected:" +
                        (string.IsNullOrEmpty(reason)
                            ? "unknown"
                            : reason);
                }
                else if (record.Active && !record.Deferred &&
                    !record.Rejected)
                {
                    record.AttemptedBuilt = false;
                    record.Built = false;
                    record.TrialRejected = false;
                    record.MaterializedWidth = 0f;
                    record.MaterializedWidthScale = 0f;
                    record.WidthReduced = false;
                    record.Deferred = true;
                    record.FinalReason =
                        "pre-shell-locality-or-construction-deferred";
                }
            }

            RecalculateEdgeWearCoverageAudit(audit);
        }

        private static bool IsEdgeWearCoverageMaterialized(
            EdgeWearCoverageAudit audit,
            PlaneCutBevelAuditResult result)
        {
            if (audit == null)
            {
                return result.PlanesBuilt == result.ActiveEdgeCount;
            }

            RecalculateEdgeWearCoverageAudit(audit);
            bool exhaustiveDenominatorValid =
                !audit.RequireAllGeometricCandidates ||
                audit.CoexistenceEligibleCount == audit.SelectedCount;
            return exhaustiveDenominatorValid &&
                audit.SelectedCount == audit.ActiveCount &&
                audit.ActiveCount == audit.AttemptedBuiltCount &&
                audit.AttemptedBuiltCount == audit.BuiltCount &&
                audit.UnresolvedWidthInactiveCount == 0 &&
                audit.TrialRejectedCount == 0 &&
                audit.DeferredCount == 0 &&
                audit.RejectedCount == 0 &&
                audit.UnmappedCount == 0;
        }

        private static string FormatPlaneCutEdgeIndexEvidence(
            List<int> edgeIndices)
        {
            if (edgeIndices == null || edgeIndices.Count == 0)
            {
                return "none";
            }

            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < edgeIndices.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append('/');
                }
                builder.Append(edgeIndices[index]);
            }
            return builder.ToString();
        }

        private static string FormatPlaneCutCandidateEdgeEvidence(
            List<PlaneCutBevelCandidate> candidates)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return "none";
            }

            List<int> indices = new List<int>(candidates.Count);
            for (int index = 0; index < candidates.Count; index++)
            {
                indices.Add(candidates[index].SourceEdgeIndex);
            }
            return FormatPlaneCutEdgeIndexEvidence(indices);
        }

        private static string FormatPlaneCutCandidateDifferenceEvidence(
            List<PlaneCutBevelCandidate> attemptedCandidates,
            List<PlaneCutBevelCandidate> certifiedCandidates)
        {
            if (attemptedCandidates == null ||
                attemptedCandidates.Count == 0)
            {
                return "none";
            }

            HashSet<int> certified = new HashSet<int>();
            if (certifiedCandidates != null)
            {
                for (int index = 0;
                     index < certifiedCandidates.Count;
                     index++)
                {
                    certified.Add(
                        certifiedCandidates[index].SourceEdgeIndex);
                }
            }

            List<int> rejectedTrialEdges = new List<int>();
            for (int index = 0;
                 index < attemptedCandidates.Count;
                 index++)
            {
                int edgeIndex =
                    attemptedCandidates[index].SourceEdgeIndex;
                if (!certified.Contains(edgeIndex))
                {
                    rejectedTrialEdges.Add(edgeIndex);
                }
            }
            rejectedTrialEdges.Sort();
            return FormatPlaneCutEdgeIndexEvidence(rejectedTrialEdges);
        }

        private static string FormatPlaneCutDeferredEdgeEvidence(
            List<int> activeEdgeIndices,
            List<PlaneCutBevelCandidate> retainedCandidates)
        {
            if (activeEdgeIndices == null || activeEdgeIndices.Count == 0)
            {
                return "none";
            }

            HashSet<int> retained = new HashSet<int>();
            if (retainedCandidates != null)
            {
                for (int index = 0;
                     index < retainedCandidates.Count;
                     index++)
                {
                    retained.Add(
                        retainedCandidates[index].SourceEdgeIndex);
                }
            }

            List<int> deferred = new List<int>();
            for (int index = 0; index < activeEdgeIndices.Count; index++)
            {
                int edgeIndex = activeEdgeIndices[index];
                if (!retained.Contains(edgeIndex))
                {
                    deferred.Add(edgeIndex);
                }
            }
            return FormatPlaneCutEdgeIndexEvidence(deferred);
        }

        private static bool TryBuildCleanPlaneCutEdgeOnlyShell(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            List<PlaneCutBevelCandidate> allCandidates,
            List<PlaneCutVertexJunctionCandidate> noJunctions,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            int localityDeferredCount,
            bool maximumCoverageMode,
            ref PlaneCutBevelAuditResult result,
            out List<PlaneCutBevelCandidate> retainedCandidates,
            out List<PolygonFace> preparedFaces,
            out string blocker)
        {
            if (maximumCoverageMode)
            {
                return TryBuildCleanPlaneCutEdgeOnlyShellWithWidthReduction(
                    sourceFaces,
                    context,
                    allCandidates,
                    noJunctions,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    localityDeferredCount,
                    ref result,
                    out retainedCandidates,
                    out preparedFaces,
                    out blocker);
            }

            return TryBuildCleanPlaneCutEdgeOnlyShellWithDeferral(
                sourceFaces,
                context,
                allCandidates,
                noJunctions,
                minimumStableEdgeLength,
                minimumStableFaceArea,
                localityDeferredCount,
                ref result,
                out retainedCandidates,
                out preparedFaces,
                out blocker);
        }

        private static bool
            TryBuildCleanPlaneCutEdgeOnlyShellWithWidthReduction(
                List<PolygonFace> sourceFaces,
                ChamferTopologyContext context,
                List<PlaneCutBevelCandidate> allCandidates,
                List<PlaneCutVertexJunctionCandidate> noJunctions,
                float minimumStableEdgeLength,
                float minimumStableFaceArea,
                int localityDeferredCount,
                ref PlaneCutBevelAuditResult result,
                out List<PlaneCutBevelCandidate> retainedCandidates,
                out List<PolygonFace> preparedFaces,
                out string blocker)
        {
            const int maximumConflictPasses = 32;
            const float reductionFactor = 0.75f;
            Dictionary<int, float> scaleByEdge =
                new Dictionary<int, float>();
            Dictionary<int, float> minimumScaleByEdge =
                new Dictionary<int, float>();
            for (int candidateIndex = 0;
                 candidateIndex < allCandidates.Count;
                 candidateIndex++)
            {
                PlaneCutBevelCandidate candidate =
                    allCandidates[candidateIndex];
                scaleByEdge[candidate.SourceEdgeIndex] = 1f;
                minimumScaleByEdge[candidate.SourceEdgeIndex] =
                    ResolvePlaneCutCandidateMinimumScale(
                        candidate,
                        minimumStableEdgeLength);
            }

            Dictionary<int, float> lastTopologyCleanScaleByEdge = null;
            PlaneCutSolverTransactionState latestAttemptedState = null;
            PlaneCutSolverTransactionState latestBandCleanState = null;
            PlaneCutSolverTransactionState latestTopologyCleanState = null;
            PlaneCutSolverTransactionState latestCertifiedState = null;
            PlaneCutRetryFailureDossier latestRetryFailure = null;
            retainedCandidates = new List<PlaneCutBevelCandidate>();
            preparedFaces = null;
            blocker = string.Empty;
            PlaneCutBevelAuditResult lastBandAudit =
                CreatePlaneCutBandAuditScratch();

            for (int passIndex = 0;
                 passIndex < maximumConflictPasses;
                 passIndex++)
            {
                retainedCandidates = BuildScaledPlaneCutCandidates(
                    allCandidates,
                    context,
                    scaleByEdge,
                    minimumStableEdgeLength);
                ResetPlaneCutStageTelemetry(ref result);
                PlaneCutNumericalRepairTelemetry numericalRepairs =
                    new PlaneCutNumericalRepairTelemetry();
                result.NumericalRepairs = numericalRepairs;
                result.EdgeConflictPassCount = passIndex + 1;
                if (!TryBuildPlaneCutSystemFaces(
                        sourceFaces,
                        retainedCandidates,
                        noJunctions,
                        out List<PolygonFace> rawFaces,
                        out int edgeCapsBuilt,
                        out string buildBlocker,
                        numericalRepairs))
                {
                    blocker = string.IsNullOrEmpty(buildBlocker)
                        ? "the deterministic width-reduced edge shell could not be built"
                        : buildBlocker;
                    result.EdgeConflictUnresolvedCount++;
                    result.AttemptedPlanesBuilt =
                        retainedCandidates.Count;
                    result.CertifiedPlanesBuilt = 0;
                    result.TrialRejectedPlanes =
                        retainedCandidates.Count;
                    result.PlanesBuilt = 0;
                    result.AttemptedEdgeEvidence =
                        FormatPlaneCutCandidateEdgeEvidence(
                            retainedCandidates);
                    result.BuiltEdgeEvidence = "none";
                    result.TrialRejectedEdgeEvidence =
                        result.AttemptedEdgeEvidence;
                    result.PlanesDeferred = localityDeferredCount;
                    if (TryResolvePlaneCutCoexistenceByExclusion(
                            sourceFaces,
                            context,
                            allCandidates,
                            noJunctions,
                            minimumStableEdgeLength,
                            minimumStableFaceArea,
                            localityDeferredCount,
                            scaleByEdge,
                            minimumScaleByEdge,
                            passIndex + 1,
                            blocker,
                            ref result,
                            out retainedCandidates,
                            out preparedFaces,
                            out string buildCoexistenceBlocker))
                    {
                        blocker = string.Empty;
                        return true;
                    }
                    blocker = string.IsNullOrEmpty(buildCoexistenceBlocker)
                        ? blocker
                        : buildCoexistenceBlocker;
                    return false;
                }

                CapturePlaneCutStageSnapshot(
                    rawFaces,
                    "AfterPlaneConstruction",
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    retainedCandidates,
                    ref result,
                    out result.StagePlaneConstruction);

                if (!TryPreparePlaneCutPreviewFaces(
                        rawFaces,
                        retainedCandidates,
                        minimumStableEdgeLength,
                        minimumStableFaceArea,
                        ref result,
                        out List<PolygonFace> auditedFaces,
                        out int conformalSplitCount,
                        out int seamPairCount,
                        out int seamTouchedFaceCount,
                        out string preparationBlocker,
                        numericalRepairs))
                {
                    blocker = string.IsNullOrEmpty(preparationBlocker)
                        ? "the width-reduced edge shell failed preview preparation"
                        : preparationBlocker;
                    result.EdgeConflictUnresolvedCount++;
                    latestAttemptedState =
                        CapturePlaneCutSolverTransactionState(
                            "attempted-preparation-failed",
                            passIndex + 1,
                            retainedCandidates,
                            rawFaces,
                            scaleByEdge,
                            false,
                            false,
                            result.StagePlaneConstruction);
                    result.LatestAttemptedState =
                        latestAttemptedState;
                    result.AttemptedPlanesBuilt =
                        retainedCandidates.Count;
                    result.CertifiedPlanesBuilt = 0;
                    result.TrialRejectedPlanes =
                        retainedCandidates.Count;
                    result.PlanesBuilt = 0;
                    result.AttemptedEdgeEvidence =
                        FormatPlaneCutCandidateEdgeEvidence(
                            retainedCandidates);
                    result.BuiltEdgeEvidence = "none";
                    result.TrialRejectedEdgeEvidence =
                        result.AttemptedEdgeEvidence;
                    result.PlanesDeferred = localityDeferredCount;
                    if (TryResolvePlaneCutCoexistenceByExclusion(
                            sourceFaces,
                            context,
                            allCandidates,
                            noJunctions,
                            minimumStableEdgeLength,
                            minimumStableFaceArea,
                            localityDeferredCount,
                            scaleByEdge,
                            minimumScaleByEdge,
                            passIndex + 1,
                            blocker,
                            ref result,
                            out retainedCandidates,
                            out preparedFaces,
                            out string preparationCoexistenceBlocker))
                    {
                        blocker = string.Empty;
                        return true;
                    }
                    blocker = string.IsNullOrEmpty(preparationCoexistenceBlocker)
                        ? blocker
                        : preparationCoexistenceBlocker;
                    return false;
                }

                PlaneCutBevelAuditResult bandAudit =
                    CreatePlaneCutBandAuditScratch();
                AuditPlaneCutBandIntegrity(
                    auditedFaces,
                    context,
                    retainedCandidates,
                    noJunctions,
                    minimumStableEdgeLength,
                    ref bandAudit,
                    out int offendingVertex,
                    out string bandBlocker);
                lastBandAudit = bandAudit;
                CapturePlaneCutEdgeConflict(
                    ref result,
                    bandAudit,
                    offendingVertex);

                bool bandClean = IsPlaneCutBandAuditClean(bandAudit);
                bool retryGeometryClean =
                    IsPlaneCutRetryGeometryClean(
                        result.StageSeamRepaired);
                latestAttemptedState =
                    CapturePlaneCutSolverTransactionState(
                        "attempted",
                        passIndex + 1,
                        retainedCandidates,
                        auditedFaces,
                        scaleByEdge,
                        bandClean,
                        retryGeometryClean,
                        result.StageSeamRepaired);
                result.LatestAttemptedState = latestAttemptedState;
                if (bandClean)
                {
                    latestBandCleanState =
                        CapturePlaneCutSolverTransactionState(
                            "band-clean",
                            passIndex + 1,
                            retainedCandidates,
                            auditedFaces,
                            scaleByEdge,
                            bandClean,
                            retryGeometryClean,
                            result.StageSeamRepaired);
                    result.LatestBandCleanState =
                        latestBandCleanState;
                }
                if (retryGeometryClean)
                {
                    lastTopologyCleanScaleByEdge =
                        ClonePlaneCutScaleMap(scaleByEdge);
                    latestTopologyCleanState =
                        CapturePlaneCutSolverTransactionState(
                            "topology-clean",
                            passIndex + 1,
                            retainedCandidates,
                            auditedFaces,
                            scaleByEdge,
                            bandClean,
                            retryGeometryClean,
                            result.StageSeamRepaired);
                    result.LatestTopologyCleanState =
                        latestTopologyCleanState;
                }

                if (!retryGeometryClean)
                {
                    string retryFailureStage =
                        ResolvePlaneCutFirstRetryFailureStage(result);
                    List<PolygonFace> retryFailureFaces =
                        retryFailureStage == "AfterPlaneConstruction"
                            ? rawFaces
                            : auditedFaces;
                    latestRetryFailure =
                        CapturePlaneCutRetryFailureDossier(
                            passIndex + 1,
                            retryFailureStage,
                            retryFailureFaces,
                            context,
                            retainedCandidates,
                            noJunctions,
                            minimumStableEdgeLength,
                            minimumStableFaceArea,
                            scaleByEdge,
                            ref result);
                    result.RetryFailureDossiers.Add(
                        latestRetryFailure);
                }

                if (bandClean && retryGeometryClean)
                {
                    latestCertifiedState =
                        CapturePlaneCutSolverTransactionState(
                            "solver-clean",
                            passIndex + 1,
                            retainedCandidates,
                            auditedFaces,
                            scaleByEdge,
                            bandClean,
                            retryGeometryClean,
                            result.StageSeamRepaired);
                    result.LatestCertifiedState =
                        latestCertifiedState;
                    preparedFaces = auditedFaces;
                    result.AttemptedPlanesBuilt =
                        retainedCandidates.Count;
                    result.CertifiedPlanesBuilt = 0;
                    result.TrialRejectedPlanes =
                        retainedCandidates.Count;
                    result.PlanesBuilt =
                        retainedCandidates.Count;
                    result.AttemptedEdgeEvidence =
                        FormatPlaneCutCandidateEdgeEvidence(
                            retainedCandidates);
                    result.BuiltEdgeEvidence = "none";
                    result.TrialRejectedEdgeEvidence =
                        result.AttemptedEdgeEvidence;
                    result.PlanesDeferred = localityDeferredCount;
                    result.PlanesLocalized =
                        CountLocalizedPlaneCutCandidates(
                            retainedCandidates);
                    result.CapsBuilt = edgeCapsBuilt;
                    result.ConformalSplitCount = conformalSplitCount;
                    result.SeamPairCount = seamPairCount;
                    result.FaceQualitySeamTouchedFaceCount =
                        seamTouchedFaceCount;
                    result.EdgeConflictResolvedCount =
                        result.EdgeConflictWidthReductionCount > 0
                            ? 1
                            : 0;
                    result.EdgeConflictMinimumWidthScale =
                        ResolveMinimumPlaneCutScale(scaleByEdge);
                    result.CoexistenceMinimumCommittedWidthScale =
                        result.EdgeConflictMinimumWidthScale;
                    CopyPlaneCutBandAudit(bandAudit, ref result);
                    return true;
                }

                bool topologyTriggered = !retryGeometryClean;
                if (topologyTriggered)
                {
                    result.EdgeConflictTopologyRejectedPassCount++;
                }

                if (passIndex + 1 >= maximumConflictPasses)
                {
                    result.EdgeConflictBudgetExhausted = 1;
                    result.EdgeConflictUnresolvedCount++;
                    blocker = topologyTriggered
                        ? "topology-aware conflict reduction exhausted its bounded pass budget"
                        : string.IsNullOrEmpty(bandBlocker)
                            ? "conflict-cluster width reduction exhausted its bounded pass budget"
                            : bandBlocker;
                    break;
                }

                List<int> clusterEdgeIndices;
                string clusterReasonEvidence;
                PlaneCutRetryFailureDossier topologyFailure = null;
                PlaneCutTJunctionFailureRecord tJunctionFailure = null;
                if (topologyTriggered)
                {
                    topologyFailure = latestRetryFailure;
                    if (topologyFailure != null &&
                        topologyFailure.TJunctionCount > 0 &&
                        latestTopologyCleanState != null)
                    {
                        if (!TryBuildMinimalPlaneCutTopologyCluster(
                                retainedCandidates,
                                topologyFailure.TJunctionFailures,
                                out clusterEdgeIndices,
                                out clusterReasonEvidence,
                                out tJunctionFailure))
                        {
                            result.EdgeConflictBudgetExhausted = 1;
                            result.EdgeConflictUnresolvedCount++;
                            blocker =
                                "the T-junction could not be mapped to its exact linked bevel set";
                            break;
                        }

                        topologyFailure.GeneralizedClusterEvidence =
                            FormatPlaneCutEdgeIndexEvidence(
                                clusterEdgeIndices);
                        topologyFailure.GeneralizedClusterReasonEvidence =
                            clusterReasonEvidence;
                        if (!TryResolvePlaneCutForeignBandRetreatTarget(
                                retainedCandidates,
                                result.EdgeConflictWidthReductions,
                                clusterEdgeIndices,
                                out int retreatVictimEdgeIndex,
                                out int retreatForeignEdgeIndex,
                                out int retreatSourcePassIndex,
                                out string retreatTriggerEvidence))
                        {
                            result.EdgeConflictBudgetExhausted = 1;
                            result.EdgeConflictUnresolvedCount++;
                            blocker =
                                "the T-junction could not be traced to a directly evidenced foreign band plane";
                            break;
                        }

                        List<int> retreatEdgeIndices = new List<int>
                        {
                            retreatForeignEdgeIndex
                        };
                        List<int> protectedEdgeIndices =
                            BuildPlaneCutProtectedEdgeSet(
                                clusterEdgeIndices,
                                retreatEdgeIndices);
                        result.TopologyScaleSearchBasePass =
                            latestTopologyCleanState.PassIndex;
                        result.TopologyScaleSearchMode =
                            "direct-foreign-band-plane-retreat";
                        result.TopologyScaleSearchTriggerEvidence =
                            retreatTriggerEvidence;
                        result.TopologyScaleSearchTopologyLinkedEvidence =
                            topologyFailure.GeneralizedClusterEvidence;
                        result.TopologyScaleSearchClusterEvidence =
                            FormatPlaneCutEdgeIndexEvidence(
                                retreatEdgeIndices);
                        result.TopologyScaleSearchProtectedEvidence =
                            FormatPlaneCutEdgeIndexEvidence(
                                protectedEdgeIndices);
                        SetPlaneCutDebugFocusEdges(
                            ref result,
                            clusterEdgeIndices,
                            retreatEdgeIndices);
                        result.EdgeConflictTopologyRollbackCount++;

                        bool searchSucceeded =
                            TrySearchPlaneCutRetreatScales(
                                sourceFaces,
                                context,
                                allCandidates,
                                noJunctions,
                                minimumStableEdgeLength,
                                minimumStableFaceArea,
                                minimumScaleByEdge,
                                latestTopologyCleanState,
                                retreatEdgeIndices,
                                retreatVictimEdgeIndex,
                                retreatForeignEdgeIndex,
                                retreatSourcePassIndex,
                                "direct-foreign-band-plane-retreat",
                                "foreign-band-retreat-trial-",
                                "direct-foreign-band-plane-retreat",
                                "direct-foreign-band-plane-relative-factor",
                                result.TopologyScaleSearchProtectedEvidence,
                                "no tested foreign-plane retreat factor produced a band-clean, topology-clean, face-quality-clean, one-surface, mesh-valid shell",
                                ref result,
                                out Dictionary<int, float>
                                    searchedScaleByEdge,
                                out List<PlaneCutBevelCandidate>
                                    searchedCandidates,
                                out List<PolygonFace> searchedFaces,
                                out string searchBlocker);

                        if (!searchSucceeded &&
                            TryResolvePlaneCutOpposingBandRetreatTarget(
                                result.TopologyScaleTrials,
                                retainedCandidates,
                                retreatVictimEdgeIndex,
                                retreatForeignEdgeIndex,
                                out int opposingForeignEdgeIndex,
                                out string dualTriggerEvidence))
                        {
                            retreatEdgeIndices = new List<int>
                            {
                                retreatForeignEdgeIndex,
                                opposingForeignEdgeIndex
                            };
                            retreatEdgeIndices.Sort();
                            protectedEdgeIndices =
                                BuildPlaneCutProtectedEdgeSet(
                                    clusterEdgeIndices,
                                    retreatEdgeIndices);
                            result.TopologyScaleSearchMode =
                                "dual-endpoint-foreign-plane-retreat";
                            result.TopologyScaleSearchTriggerEvidence =
                                dualTriggerEvidence;
                            result.TopologyScaleSearchClusterEvidence =
                                FormatPlaneCutEdgeIndexEvidence(
                                    retreatEdgeIndices);
                            result.TopologyScaleSearchProtectedEvidence =
                                FormatPlaneCutEdgeIndexEvidence(
                                    protectedEdgeIndices);
                            SetPlaneCutDebugFocusEdges(
                                ref result,
                                clusterEdgeIndices,
                                retreatEdgeIndices);

                            searchSucceeded =
                                TrySearchPlaneCutRetreatScales(
                                    sourceFaces,
                                    context,
                                    allCandidates,
                                    noJunctions,
                                    minimumStableEdgeLength,
                                    minimumStableFaceArea,
                                    minimumScaleByEdge,
                                    latestTopologyCleanState,
                                    retreatEdgeIndices,
                                    retreatVictimEdgeIndex,
                                    opposingForeignEdgeIndex,
                                    retreatSourcePassIndex,
                                    "dual-endpoint-foreign-plane-retreat",
                                    "dual-endpoint-retreat-trial-",
                                    "dual-endpoint-foreign-plane-retreat",
                                    "dual-endpoint-relative-factor",
                                    result.TopologyScaleSearchProtectedEvidence,
                                    "no tested dual-endpoint retreat factor produced a band-clean, topology-clean, face-quality-clean, one-surface, mesh-valid shell",
                                    ref result,
                                    out searchedScaleByEdge,
                                    out searchedCandidates,
                                    out searchedFaces,
                                    out searchBlocker);
                        }

                        latestAttemptedState =
                            result.LatestAttemptedState;
                        latestBandCleanState =
                            result.LatestBandCleanState;
                        latestTopologyCleanState =
                            result.LatestTopologyCleanState;
                        latestCertifiedState =
                            result.LatestCertifiedState;
                        if (result.RetryFailureDossiers.Count > 0)
                        {
                            latestRetryFailure =
                                result.RetryFailureDossiers[
                                    result.RetryFailureDossiers.Count - 1];
                        }

                        if (searchSucceeded)
                        {
                            scaleByEdge = searchedScaleByEdge;
                            retainedCandidates = searchedCandidates;
                            preparedFaces = searchedFaces;
                            result.AttemptedPlanesBuilt =
                                retainedCandidates.Count;
                            result.CertifiedPlanesBuilt = 0;
                            result.TrialRejectedPlanes =
                                retainedCandidates.Count;
                            result.PlanesBuilt =
                                retainedCandidates.Count;
                            result.AttemptedEdgeEvidence =
                                FormatPlaneCutCandidateEdgeEvidence(
                                    retainedCandidates);
                            result.BuiltEdgeEvidence = "none";
                            result.TrialRejectedEdgeEvidence =
                                result.AttemptedEdgeEvidence;
                            result.PlanesDeferred = localityDeferredCount;
                            result.PlanesLocalized =
                                CountLocalizedPlaneCutCandidates(
                                    retainedCandidates);
                            result.EdgeConflictResolvedCount = 1;
                            result.EdgeConflictMinimumWidthScale =
                                ResolveMinimumPlaneCutScale(scaleByEdge);
                            result.CoexistenceMinimumCommittedWidthScale =
                                result.EdgeConflictMinimumWidthScale;
                            return true;
                        }

                        result.EdgeConflictBudgetExhausted = 1;
                        result.EdgeConflictUnresolvedCount++;
                        blocker = string.IsNullOrEmpty(searchBlocker)
                            ? "the bounded endpoint retreat searches found no fully valid factor"
                            : searchBlocker;
                        break;
                    }

                    bool mapped =
                        TryBuildPlaneCutGeneralizedFailureCluster(
                            retainedCandidates,
                            topologyFailure,
                            result.EdgeConflictWidthReductions,
                            out clusterEdgeIndices,
                            out clusterReasonEvidence);
                    if (!mapped)
                    {
                        result.EdgeConflictBudgetExhausted = 1;
                        result.EdgeConflictUnresolvedCount++;
                        blocker =
                            "the retry defect could not be mapped to a complete local bevel interaction cluster";
                        break;
                    }
                    topologyFailure.GeneralizedClusterEvidence =
                        FormatPlaneCutEdgeIndexEvidence(
                            clusterEdgeIndices);
                    topologyFailure.GeneralizedClusterReasonEvidence =
                        clusterReasonEvidence;

                    PlaneCutConflictWidthReductionRecord captured =
                        new PlaneCutConflictWidthReductionRecord
                        {
                            PassIndex = passIndex + 1,
                            VictimEdgeIndex =
                                topologyFailure.LinkedEdgeIndices.Count > 0
                                    ? topologyFailure.LinkedEdgeIndices[0]
                                    : -1,
                            ForeignEdgeIndex =
                                topologyFailure.LinkedEdgeIndices.Count > 1
                                    ? topologyFailure.LinkedEdgeIndices[1]
                                    : -1,
                            VertexIndex = -1,
                            TriggerCategory = "topology-diagnostic",
                            BandValid = bandClean ? 1 : 0,
                            TopologyValid = 0,
                            OpenEdgeCount = topologyFailure.OpenEdgeCount,
                            NonManifoldEdgeCount =
                                topologyFailure.NonManifoldEdgeCount,
                            TJunctionCount = topologyFailure.TJunctionCount,
                            InvalidFaceCount =
                                topologyFailure.InvalidFaceCount,
                            NonPlanarFaceCount =
                                topologyFailure.NonPlanarFaceCount,
                            ClusterReasonEvidence =
                                clusterReasonEvidence,
                            PreviousScaleEvidence =
                                FormatPlaneCutScaleEvidence(
                                    scaleByEdge,
                                    clusterEdgeIndices),
                            Result =
                                "generalized-failure-dossier-captured-no-geometry-retry"
                        };
                    captured.ClusterEdgeIndices.AddRange(
                        clusterEdgeIndices);
                    result.EdgeConflictWidthReductions.Add(captured);
                    result.EdgeConflictClusterCount =
                        result.EdgeConflictWidthReductions.Count;
                    result.EdgeConflictBudgetExhausted = 1;
                    result.EdgeConflictUnresolvedCount++;
                    blocker =
                        "the generalized retry failure was captured; width-reduction geometry remains unchanged";
                    break;
                }
                else if (!TryBuildPlaneCutConflictCluster(
                        retainedCandidates,
                        bandAudit,
                        context,
                        out clusterEdgeIndices))
                {
                    result.EdgeConflictBudgetExhausted = 1;
                    result.EdgeConflictUnresolvedCount++;
                    blocker = string.IsNullOrEmpty(bandBlocker)
                        ? "the bevel-band conflict could not be mapped to a local edge cluster"
                        : bandBlocker;
                    break;
                }
                else
                {
                    clusterReasonEvidence =
                        "band-conflict-and-incident-source-vertex-star";
                }

                int victimEdgeIndex = tJunctionFailure != null &&
                        tJunctionFailure.LinkedEdgeIndices.Count > 0
                    ? tJunctionFailure.LinkedEdgeIndices[0]
                    : topologyFailure != null &&
                        topologyFailure.LinkedEdgeIndices.Count > 0
                        ? topologyFailure.LinkedEdgeIndices[0]
                        : bandAudit.EdgeConflictVictimEdgeIndex;
                int foreignEdgeIndex = tJunctionFailure != null &&
                        tJunctionFailure.LinkedEdgeIndices.Count > 1
                    ? tJunctionFailure.LinkedEdgeIndices[1]
                    : topologyFailure != null &&
                        topologyFailure.LinkedEdgeIndices.Count > 1
                        ? topologyFailure.LinkedEdgeIndices[1]
                        : bandAudit.EdgeConflictForeignEdgeIndex;
                PlaneCutConflictWidthReductionRecord reduction =
                    new PlaneCutConflictWidthReductionRecord
                    {
                        PassIndex = passIndex + 1,
                        VictimEdgeIndex = victimEdgeIndex,
                        ForeignEdgeIndex = foreignEdgeIndex,
                        VertexIndex = topologyTriggered
                            ? -1
                            : bandAudit.EdgeConflictVertexIndex >= 0
                                ? bandAudit.EdgeConflictVertexIndex
                                : offendingVertex,
                        TriggerCategory = topologyTriggered
                            ? "topology"
                            : "band-integrity",
                        BandValid = bandClean ? 1 : 0,
                        TopologyValid = retryGeometryClean ? 1 : 0,
                        OpenEdgeCount =
                            result.StageSeamRepaired.OpenEdgeCount,
                        NonManifoldEdgeCount =
                            result.StageSeamRepaired
                                .NonManifoldEdgeCount,
                        TJunctionCount =
                            result.StageSeamRepaired.TJunctionCount,
                        InvalidFaceCount =
                            result.StageSeamRepaired.InvalidFaceCount,
                        NonPlanarFaceCount =
                            result.StageSeamRepaired
                                .NonPlanarFaceCount,
                        VictimCoverageRatio =
                            bandAudit.EdgeConflictVictimCoverageRatio,
                        ForeignAxialParameter =
                            bandAudit.EdgeConflictForeignAxialParameter,
                        ForeignSharedSpanRatio =
                            bandAudit.EdgeConflictForeignSharedSpanRatio,
                        ClusterReasonEvidence =
                            clusterReasonEvidence
                    };
                reduction.ClusterEdgeIndices.AddRange(clusterEdgeIndices);
                reduction.PreviousMinimumScale =
                    ResolveMinimumPlaneCutScale(
                        scaleByEdge,
                        clusterEdgeIndices);
                reduction.PreviousScaleEvidence =
                    FormatPlaneCutScaleEvidence(
                        scaleByEdge,
                        clusterEdgeIndices);
                reduction.RequestedScale =
                    reduction.PreviousMinimumScale * reductionFactor;
                reduction.ClusterFloorScale =
                    ResolveMaximumPlaneCutScale(
                        minimumScaleByEdge,
                        clusterEdgeIndices);

                if (topologyTriggered &&
                    lastTopologyCleanScaleByEdge != null)
                {
                    scaleByEdge = ClonePlaneCutScaleMap(
                        lastTopologyCleanScaleByEdge);
                    reduction.TopologyRollbackApplied = 1;
                    reduction.RollbackScaleEvidence =
                        FormatPlaneCutScaleEvidence(
                            scaleByEdge,
                            clusterEdgeIndices);
                    result.EdgeConflictTopologyRollbackCount++;
                }

                bool reduced = false;
                float appliedMinimumScale = 1f;
                for (int clusterIndex = 0;
                     clusterIndex < clusterEdgeIndices.Count;
                     clusterIndex++)
                {
                    int edgeIndex = clusterEdgeIndices[clusterIndex];
                    float currentScale = scaleByEdge[edgeIndex];
                    float minimumScale = minimumScaleByEdge[edgeIndex];
                    float nextScale = Mathf.Max(
                        minimumScale,
                        Mathf.Min(
                            currentScale,
                            reduction.RequestedScale));
                    if (nextScale < currentScale - 0.000001f)
                    {
                        scaleByEdge[edgeIndex] = nextScale;
                        reduced = true;
                    }
                    appliedMinimumScale = Mathf.Min(
                        appliedMinimumScale,
                        scaleByEdge[edgeIndex]);
                }
                reduction.AppliedMinimumScale = appliedMinimumScale;
                reduction.AppliedScaleEvidence =
                    FormatPlaneCutScaleEvidence(
                        scaleByEdge,
                        clusterEdgeIndices);

                if (!reduced)
                {
                    reduction.Result = topologyTriggered
                        ? "topology-unresolved-geometric-floor"
                        : "unresolved-geometric-floor";
                    result.EdgeConflictWidthReductions.Add(reduction);
                    result.EdgeConflictClusterCount =
                        result.EdgeConflictWidthReductions.Count;
                    result.EdgeConflictBudgetExhausted = 1;
                    result.EdgeConflictUnresolvedCount++;
                    blocker = topologyTriggered
                        ? "the topology conflict cluster reached its geometric minimum width"
                        : string.IsNullOrEmpty(bandBlocker)
                            ? "the conflicting bevel cluster reached its geometric minimum width"
                            : bandBlocker +
                                "; conflict cluster reached its geometric minimum width";
                    break;
                }

                reduction.Result = topologyTriggered
                    ? "topology-rejected-expanded-reduced-and-retry"
                    : "band-reduced-and-retry";
                result.EdgeConflictWidthReductions.Add(reduction);
                result.EdgeConflictClusterCount =
                    result.EdgeConflictWidthReductions.Count;
                result.EdgeConflictWidthReductionCount++;
                result.EdgeConflictMinimumWidthScale = Mathf.Min(
                    result.EdgeConflictMinimumWidthScale,
                    appliedMinimumScale);
            }

            CopyPlaneCutBandAudit(lastBandAudit, ref result);
            result.LatestAttemptedState = latestAttemptedState;
            result.LatestBandCleanState = latestBandCleanState;
            result.LatestTopologyCleanState = latestTopologyCleanState;
            result.LatestCertifiedState = latestCertifiedState;
            result.AttemptedPlanesBuilt = latestAttemptedState == null
                ? retainedCandidates.Count
                : latestAttemptedState.Candidates.Count;
            result.CertifiedPlanesBuilt = latestCertifiedState == null
                ? 0
                : latestCertifiedState.Candidates.Count;
            result.TrialRejectedPlanes = Mathf.Max(
                0,
                result.AttemptedPlanesBuilt -
                    result.CertifiedPlanesBuilt);
            result.PlanesBuilt = result.CertifiedPlanesBuilt;
            result.AttemptedEdgeEvidence = latestAttemptedState == null
                ? FormatPlaneCutCandidateEdgeEvidence(
                    retainedCandidates)
                : FormatPlaneCutCandidateEdgeEvidence(
                    latestAttemptedState.Candidates);
            result.BuiltEdgeEvidence = latestCertifiedState == null
                ? "none"
                : FormatPlaneCutCandidateEdgeEvidence(
                    latestCertifiedState.Candidates);
            result.TrialRejectedEdgeEvidence =
                FormatPlaneCutCandidateDifferenceEvidence(
                    latestAttemptedState == null
                        ? retainedCandidates
                        : latestAttemptedState.Candidates,
                    latestCertifiedState == null
                        ? null
                        : latestCertifiedState.Candidates);
            result.PlanesDeferred = localityDeferredCount;
            result.PlanesLocalized = CountLocalizedPlaneCutCandidates(
                retainedCandidates);
            if (latestRetryFailure != null)
            {
                ApplyPlaneCutRetryFailureToResult(
                    latestRetryFailure,
                    minimumStableEdgeLength,
                    ref result);
            }
            if (TryResolvePlaneCutCoexistenceByExclusion(
                    sourceFaces,
                    context,
                    allCandidates,
                    noJunctions,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    localityDeferredCount,
                    scaleByEdge,
                    minimumScaleByEdge,
                    result.EdgeConflictPassCount,
                    blocker,
                    ref result,
                    out retainedCandidates,
                    out preparedFaces,
                    out string coexistenceBlocker))
            {
                blocker = string.Empty;
                return true;
            }
            blocker = string.IsNullOrEmpty(coexistenceBlocker)
                ? blocker
                : coexistenceBlocker;
            return false;
        }

        private static bool TryResolvePlaneCutCoexistenceByExclusion(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            List<PlaneCutBevelCandidate> allCandidates,
            List<PlaneCutVertexJunctionCandidate> noJunctions,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            int localityDeferredCount,
            Dictionary<int, float> startingScaleByEdge,
            Dictionary<int, float> minimumScaleByEdge,
            int basePassIndex,
            string initialBlocker,
            ref PlaneCutBevelAuditResult result,
            out List<PlaneCutBevelCandidate> retainedCandidates,
            out List<PolygonFace> preparedFaces,
            out string blocker)
        {
            const int maximumExclusions = 12;
            const int maximumEvaluatedStates = 128;
            const int maximumConflictCandidates = 10;

            retainedCandidates = new List<PlaneCutBevelCandidate>();
            preparedFaces = null;
            blocker = string.Empty;
            if (allCandidates == null || allCandidates.Count <= 1)
            {
                blocker = string.IsNullOrEmpty(initialBlocker)
                    ? "coexistence closure requires at least two candidates"
                    : initialBlocker;
                return false;
            }

            Dictionary<int, float> rootScaleByEdge =
                ClonePlaneCutScaleMap(startingScaleByEdge);
            for (int candidateIndex = 0;
                 candidateIndex < allCandidates.Count;
                 candidateIndex++)
            {
                int edgeIndex = allCandidates[candidateIndex].SourceEdgeIndex;
                float stored = rootScaleByEdge.TryGetValue(
                        edgeIndex,
                        out float value)
                    ? value
                    : 1f;
                rootScaleByEdge[edgeIndex] = Mathf.Clamp(
                    stored,
                    EdgeWearMinimumFeasibleWidthFraction,
                    1f);
            }

            List<int> rootExpectedEdgeIndices =
                BuildPlaneCutExpectedCoexistenceEdgeSet(
                    result.CoverageAudit,
                    allCandidates);
            Dictionary<string, PlaneCutCoexistenceTrialOutcome> trialCache =
                new Dictionary<string, PlaneCutCoexistenceTrialOutcome>();
            HashSet<string> enqueuedStateKeys = new HashSet<string>();
            List<PlaneCutCoexistenceSearchNode> frontier =
                new List<PlaneCutCoexistenceSearchNode>();
            PlaneCutCoexistenceTrialOutcome rootOutcome =
                new PlaneCutCoexistenceTrialOutcome
                {
                    CacheKey = BuildPlaneCutCoexistenceSearchStateKey(
                        new List<int>(),
                        rootScaleByEdge),
                    ScaleByEdge = rootScaleByEdge,
                    Candidates = new List<PlaneCutBevelCandidate>(
                        allCandidates),
                    Audit = result,
                    Blocker = initialBlocker ?? string.Empty,
                    FailureSignature =
                        BuildPlaneCutCoexistenceFailureSignature(
                            result,
                            initialBlocker)
                };
            rootOutcome.DirectFailureDossier =
                BuildPlaneCutCoexistenceFailureDossier(
                    result,
                    null,
                    initialBlocker,
                    allCandidates);
            PopulatePlaneCutCandidateConservation(
                rootExpectedEdgeIndices,
                rootOutcome.ExcludedEdgeIndices,
                rootOutcome.Candidates,
                rootOutcome.Candidates.Count,
                false,
                ref rootOutcome);
            frontier.Add(new PlaneCutCoexistenceSearchNode
            {
                Outcome = rootOutcome,
                EffectiveFailureDossier =
                    rootOutcome.DirectFailureDossier
            });
            enqueuedStateKeys.Add(
                BuildPlaneCutCoexistenceSearchNodeKey(
                    rootOutcome.CacheKey,
                    rootOutcome.DirectFailureDossier));

            int trialOrdinal = 0;
            int processedStates = 0;
            int deduplicatedStates = 0;
            int maximumDepth = 0;
            string lastFailure = string.IsNullOrEmpty(initialBlocker)
                ? "coexistence closure found no certified state"
                : initialBlocker;
            Bounds sourceBounds = CalculateFaceBounds(sourceFaces);
            double sourceVolume = CalculatePlaneCutPolyhedronVolume(
                sourceFaces);

            while (frontier.Count > 0 &&
                processedStates < maximumEvaluatedStates)
            {
                frontier.Sort((left, right) =>
                    ComparePlaneCutCoexistenceSearchNodes(
                        left,
                        right,
                        allCandidates));
                PlaneCutCoexistenceSearchNode node = frontier[0];
                frontier.RemoveAt(0);
                PlaneCutCoexistenceTrialOutcome outcome = node.Outcome;
                processedStates++;
                maximumDepth = Mathf.Max(
                    maximumDepth,
                    outcome.ExcludedEdgeIndices.Count);

                if (outcome.FullyValid)
                {
                    RecordPlaneCutCoexistenceSearchState(
                        processedStates,
                        "none",
                        new List<int>(),
                        node.EffectiveFailureDossier,
                        outcome,
                        ref result);
                    SortedSet<int> winningExclusions =
                        new SortedSet<int>(
                            outcome.ExcludedEdgeIndices);
                    result.CoexistenceSearchStatesEvaluated =
                        processedStates;
                    result.CoexistenceSearchStatesDeduplicated =
                        deduplicatedStates;
                    result.CoexistenceSearchMaximumDepth = maximumDepth;
                    result.CoexistenceSearchFrontierRemaining =
                        frontier.Count;
                    result.CoexistenceSearchWinningDepth =
                        outcome.ExcludedEdgeIndices.Count;
                    result.CoexistenceCandidateExpectedEvidence =
                        FormatPlaneCutEdgeIndexEvidence(
                            outcome.ExpectedCandidateEdgeIndices);
                    result.CoexistenceCandidateActualEvidence =
                        FormatPlaneCutEdgeIndexEvidence(
                            outcome.ActualCandidateEdgeIndices);
                    result.CoexistenceCandidateMissingEvidence = "none";
                    result.CoexistenceCandidateUnexpectedEvidence = "none";
                    if (!ApplyPlaneCutCoexistenceSuccess(
                            outcome,
                            winningExclusions,
                            node.ReasonByExcludedEdge,
                            localityDeferredCount,
                            basePassIndex,
                            ref result,
                            out retainedCandidates,
                            out preparedFaces,
                            out string finalizationBlocker))
                    {
                        blocker = finalizationBlocker;
                        return false;
                    }
                    return true;
                }

                string exclusionReason =
                    ResolvePlaneCutCoexistenceExclusionReason(
                        node.EffectiveFailureDossier,
                        outcome);
                List<int> conflictEdges =
                    BuildPlaneCutCoexistenceConflictEdgeSet(
                        node.EffectiveFailureDossier,
                        allCandidates,
                        outcome.ExcludedEdgeIndices,
                        context,
                        maximumConflictCandidates,
                        outcome);
                RecordPlaneCutCoexistenceSearchState(
                    processedStates,
                    exclusionReason,
                    conflictEdges,
                    node.EffectiveFailureDossier,
                    outcome,
                    ref result);

                if (string.Equals(
                        exclusionReason,
                        "coexistence-incompatible",
                        StringComparison.Ordinal) ||
                    conflictEdges.Count == 0 ||
                    outcome.ExcludedEdgeIndices.Count >= maximumExclusions)
                {
                    if (!string.IsNullOrEmpty(outcome.Blocker))
                    {
                        lastFailure = outcome.Blocker;
                    }
                    continue;
                }

                bool starConflict = string.Equals(
                    exclusionReason,
                    "source-vertex-star-incompatible",
                    StringComparison.Ordinal);
                bool pairConflict = string.Equals(
                        exclusionReason,
                        "plane-pair-incompatible",
                        StringComparison.Ordinal) ||
                    string.Equals(
                        exclusionReason,
                        "plane-band-incompatible",
                        StringComparison.Ordinal);
                HashSet<int> alreadyExcluded = new HashSet<int>(
                    outcome.ExcludedEdgeIndices);
                for (int edgeOrdinal = 0;
                     edgeOrdinal < conflictEdges.Count &&
                     trialOrdinal < maximumEvaluatedStates;
                     edgeOrdinal++)
                {
                    int edgeToExclude = conflictEdges[edgeOrdinal];
                    if (alreadyExcluded.Contains(edgeToExclude))
                    {
                        continue;
                    }

                    List<int> childExclusions =
                        new List<int>(outcome.ExcludedEdgeIndices)
                        {
                            edgeToExclude
                        };
                    childExclusions.Sort();
                    if (childExclusions.Count > maximumExclusions)
                    {
                        continue;
                    }
                    Dictionary<int, string> childReasons =
                        ClonePlaneCutCoexistenceReasonMap(
                            node.ReasonByExcludedEdge);
                    childReasons[edgeToExclude] = exclusionReason;
                    PlaneCutCoexistenceTrialOutcome childOutcome =
                        EvaluatePlaneCutCoexistenceExclusionTrial(
                            sourceFaces,
                            sourceBounds,
                            sourceVolume,
                            context,
                            allCandidates,
                            noJunctions,
                            minimumStableEdgeLength,
                            minimumStableFaceArea,
                            minimumScaleByEdge,
                            outcome.ScaleByEdge,
                            rootExpectedEdgeIndices,
                            childExclusions,
                            exclusionReason,
                            basePassIndex,
                            trialCache,
                            ref trialOrdinal,
                            ref result,
                            starConflict,
                            pairConflict);
                    PlaneCutCoexistenceFailureDossier
                        childEffectiveDossier =
                            ResolvePlaneCutEffectiveFailureDossier(
                                childOutcome.DirectFailureDossier,
                                node.EffectiveFailureDossier,
                                childExclusions,
                                allCandidates);
                    string childStateKey =
                        BuildPlaneCutCoexistenceSearchNodeKey(
                            childOutcome.CacheKey,
                            childEffectiveDossier);
                    if (!enqueuedStateKeys.Add(childStateKey))
                    {
                        deduplicatedStates++;
                        continue;
                    }
                    frontier.Add(new PlaneCutCoexistenceSearchNode
                    {
                        Outcome = childOutcome,
                        EffectiveFailureDossier =
                            childEffectiveDossier,
                        ReasonByExcludedEdge = childReasons
                    });
                }
            }

            result.CoexistenceTrialCount = trialOrdinal;
            result.CoexistenceSearchStatesEvaluated = processedStates;
            result.CoexistenceSearchStatesDeduplicated = deduplicatedStates;
            result.CoexistenceSearchMaximumDepth = maximumDepth;
            result.CoexistenceSearchFrontierRemaining = frontier.Count;
            result.CoexistenceSearchWinningDepth = -1;
            blocker = frontier.Count > 0
                ? "coexistence closure exhausted its bounded state budget"
                : string.IsNullOrEmpty(lastFailure)
                    ? "coexistence closure exhausted its conflict-directed frontier"
                    : lastFailure;
            return false;
        }

        private static PlaneCutCoexistenceTrialOutcome
            EvaluatePlaneCutCoexistenceExclusionTrial(
                List<PolygonFace> sourceFaces,
                Bounds sourceBounds,
                double sourceVolume,
                ChamferTopologyContext context,
                List<PlaneCutBevelCandidate> allCandidates,
                List<PlaneCutVertexJunctionCandidate> noJunctions,
                float minimumStableEdgeLength,
                float minimumStableFaceArea,
                Dictionary<int, float> minimumScaleByEdge,
                Dictionary<int, float> scaleByEdge,
                List<int> rootExpectedEdgeIndices,
                List<int> excludedEdges,
                string exclusionReason,
                int basePassIndex,
                Dictionary<string, PlaneCutCoexistenceTrialOutcome> cache,
                ref int trialOrdinal,
                ref PlaneCutBevelAuditResult result,
                bool starConflict,
                bool pairConflict)
        {
            excludedEdges.Sort();
            HashSet<int> excluded = new HashSet<int>(excludedEdges);
            List<PlaneCutBevelCandidate> subset =
                new List<PlaneCutBevelCandidate>();
            Dictionary<int, float> subsetScales =
                new Dictionary<int, float>();
            for (int candidateIndex = 0;
                 candidateIndex < allCandidates.Count;
                 candidateIndex++)
            {
                PlaneCutBevelCandidate candidate = allCandidates[candidateIndex];
                if (excluded.Contains(candidate.SourceEdgeIndex))
                {
                    continue;
                }
                subset.Add(candidate);
                subsetScales[candidate.SourceEdgeIndex] =
                    scaleByEdge.TryGetValue(
                            candidate.SourceEdgeIndex,
                            out float scale)
                        ? Mathf.Clamp(
                            scale,
                            EdgeWearMinimumFeasibleWidthFraction,
                            1f)
                        : 1f;
            }

            string cacheKey = BuildPlaneCutCoexistenceTrialCacheKey(
                excludedEdges,
                subset,
                subsetScales);
            if (cache.TryGetValue(
                    cacheKey,
                    out PlaneCutCoexistenceTrialOutcome cached))
            {
                result.CoexistenceTrialCacheUseCount++;
                if (starConflict)
                {
                    result.CoexistenceStarCacheUseCount++;
                }
                if (pairConflict)
                {
                    result.CoexistencePairCacheUseCount++;
                }
                return cached;
            }

            trialOrdinal++;
            result.CoexistenceTrialCount = trialOrdinal;
            if (starConflict)
            {
                result.CoexistenceStarEvaluationCount++;
            }
            if (pairConflict)
            {
                result.CoexistencePairEvaluationCount++;
            }

            PlaneCutCoexistenceTrialOutcome outcome =
                new PlaneCutCoexistenceTrialOutcome
                {
                    CacheKey = cacheKey,
                    ExclusionReason = exclusionReason,
                    ScaleByEdge = subsetScales,
                    Candidates = subset
                };
            outcome.ExcludedEdgeIndices.AddRange(excludedEdges);
            if (subset.Count == 0)
            {
                outcome.FullyValid = false;
                outcome.Blocker = "coexistence trial excluded every candidate";
                outcome.FailureSignature = outcome.Blocker;
                outcome.DirectFailureDossier =
                    BuildPlaneCutCoexistenceFailureDossier(
                        outcome.Audit,
                        outcome.Trial,
                        outcome.Blocker,
                        allCandidates);
                cache.Add(cacheKey, outcome);
                return outcome;
            }

            PlaneCutSolverTransactionState baseState =
                new PlaneCutSolverTransactionState
                {
                    Name = "coexistence-base",
                    PassIndex = Mathf.Max(1, basePassIndex),
                    ScaleByEdge = subsetScales
                };
            baseState.Candidates.AddRange(subset);
            bool fullyValid = TryEvaluatePlaneCutRetreatTrial(
                sourceFaces,
                sourceBounds,
                sourceVolume,
                context,
                subset,
                noJunctions,
                minimumStableEdgeLength,
                minimumStableFaceArea,
                minimumScaleByEdge,
                baseState,
                new List<int>(),
                trialOrdinal,
                1f,
                "coexistence-exclusion",
                "none",
                out PlaneCutTopologyScaleTrialRecord trial,
                out Dictionary<int, float> trialScaleByEdge,
                out List<PlaneCutBevelCandidate> trialCandidates,
                out List<PolygonFace> trialFaces,
                out PlaneCutBevelAuditResult trialAudit,
                out string trialBlocker);
            outcome.ScaleByEdge = trialScaleByEdge;
            outcome.Candidates = trialCandidates;
            outcome.Faces = trialFaces;
            outcome.Trial = trial;
            outcome.Audit = trialAudit;
            outcome.Blocker = trialBlocker ?? string.Empty;
            PopulatePlaneCutCandidateConservation(
                rootExpectedEdgeIndices,
                outcome.ExcludedEdgeIndices,
                trialCandidates,
                trial == null ? 0 : trial.AttemptedBuiltCount,
                fullyValid,
                ref outcome);
            outcome.FullyValid = fullyValid &&
                outcome.CandidateConservationValid &&
                outcome.CertifiedCandidateCount ==
                    outcome.ExpectedCandidateCount;
            if (fullyValid && !outcome.FullyValid)
            {
                RecordPlaneCutCandidateConservationFailure(
                    outcome,
                    ref result);
                outcome.Blocker = "candidate-conservation-failed";
            }
            outcome.FailureSignature =
                BuildPlaneCutCoexistenceFailureSignature(
                    trialAudit,
                    outcome.Blocker);
            outcome.DirectFailureDossier =
                BuildPlaneCutCoexistenceFailureDossier(
                    trialAudit,
                    trial,
                    outcome.Blocker,
                    allCandidates);
            cache.Add(cacheKey, outcome);
            return outcome;
        }

        private static bool ApplyPlaneCutCoexistenceSuccess(
            PlaneCutCoexistenceTrialOutcome outcome,
            SortedSet<int> excludedEdges,
            Dictionary<int, string> reasonByExcludedEdge,
            int localityDeferredCount,
            int basePassIndex,
            ref PlaneCutBevelAuditResult result,
            out List<PlaneCutBevelCandidate> retainedCandidates,
            out List<PolygonFace> preparedFaces,
            out string blocker)
        {
            blocker = string.Empty;
            retainedCandidates = outcome.Candidates;
            preparedFaces = outcome.Faces;
            ApplyPlaneCutTopologyTrialAuditToResult(
                outcome.Audit,
                ref result);
            result.Diagnostic = string.Empty;
            result.RetryFailureDossiers.Clear();
            result.FirstOpenEdgeStage = "none";
            result.FirstTJunctionStage = "none";
            result.FirstNonPlanarStage = "none";
            result.EdgeConflictBudgetExhausted = 0;
            result.EdgeConflictUnresolvedCount = 0;
            result.EdgeConflictResolvedCount = 1;
            result.EdgeConflictVictimEdgeIndex = -1;
            result.EdgeConflictForeignEdgeIndex = -1;
            result.EdgeConflictVertexIndex = -1;
            result.EdgeConflictDeferredEdgeIndex = -1;
            result.EdgeConflictVictimCoverageRatio = 0f;
            result.EdgeConflictForeignAxialParameter = 0f;
            result.EdgeConflictForeignSharedSpanRatio = 0f;
            result.ActiveSearchFailureStage = "none";
            result.ActiveSearchFailureCause = "none";
            result.ActiveSearchFailureEvidence = "none";
            result.CoexistenceExclusionCount = excludedEdges.Count;
            result.CoexistenceExcludedEdgeEvidence =
                FormatPlaneCutEdgeIndexEvidence(
                    new List<int>(excludedEdges));
            result.CoexistenceExclusionReasonEvidence =
                FormatPlaneCutCoexistenceReasonEvidence(
                    excludedEdges,
                    reasonByExcludedEdge);
            result.CoexistenceMinimumCommittedWidthScale =
                ResolveMinimumPlaneCutScale(outcome.ScaleByEdge);
            result.EdgeConflictMinimumWidthScale =
                result.CoexistenceMinimumCommittedWidthScale;
            result.PlanesBuilt = retainedCandidates.Count;
            result.AttemptedPlanesBuilt = retainedCandidates.Count;
            result.CertifiedPlanesBuilt = retainedCandidates.Count;
            result.TrialRejectedPlanes = 0;
            result.PlanesDeferred = localityDeferredCount;
            result.PlanesLocalized = CountLocalizedPlaneCutCandidates(
                retainedCandidates);
            result.AttemptedEdgeEvidence =
                FormatPlaneCutCandidateEdgeEvidence(retainedCandidates);
            result.BuiltEdgeEvidence =
                result.AttemptedEdgeEvidence;
            result.TrialRejectedEdgeEvidence = "none";
            result.DeferredEdgeEvidence = "none";
            result.LatestAttemptedState =
                CapturePlaneCutSolverTransactionState(
                    "coexistence-attempted",
                    Mathf.Max(1, basePassIndex),
                    retainedCandidates,
                    preparedFaces,
                    outcome.ScaleByEdge,
                    true,
                    true,
                    outcome.Audit.StageFinalCertification);
            result.LatestBandCleanState =
                CapturePlaneCutSolverTransactionState(
                    "coexistence-band-clean",
                    Mathf.Max(1, basePassIndex),
                    retainedCandidates,
                    preparedFaces,
                    outcome.ScaleByEdge,
                    true,
                    true,
                    outcome.Audit.StageFinalCertification);
            result.LatestTopologyCleanState =
                CapturePlaneCutSolverTransactionState(
                    "coexistence-topology-clean",
                    Mathf.Max(1, basePassIndex),
                    retainedCandidates,
                    preparedFaces,
                    outcome.ScaleByEdge,
                    true,
                    true,
                    outcome.Audit.StageFinalCertification);
            result.LatestCertifiedState =
                CapturePlaneCutSolverTransactionState(
                    "solver-clean",
                    Mathf.Max(1, basePassIndex),
                    retainedCandidates,
                    preparedFaces,
                    outcome.ScaleByEdge,
                    true,
                    true,
                    outcome.Audit.StageFinalCertification);

            EdgeWearCoverageAudit coverage = result.CoverageAudit;
            if (coverage != null)
            {
                coverage.CoexistenceStarEvaluationCount =
                    result.CoexistenceStarEvaluationCount;
                coverage.CoexistenceStarCacheUseCount =
                    result.CoexistenceStarCacheUseCount;
                coverage.CoexistencePairEvaluationCount =
                    result.CoexistencePairEvaluationCount;
                coverage.CoexistencePairCacheUseCount =
                    result.CoexistencePairCacheUseCount;
                coverage.CoexistenceTrialCount =
                    result.CoexistenceTrialCount;
                coverage.CoexistenceSearchExclusionCount =
                    excludedEdges.Count;
                for (int recordIndex = 0;
                     recordIndex < coverage.Records.Count;
                     recordIndex++)
                {
                    EdgeWearEdgeLifecycleRecord record =
                        coverage.Records[recordIndex];
                    if (!excludedEdges.Contains(record.SourceEdgeIndex))
                    {
                        continue;
                    }
                    string reason = reasonByExcludedEdge.TryGetValue(
                            record.SourceEdgeIndex,
                            out string storedReason)
                        ? storedReason
                        : "coexistence-incompatible";
                    record.CoexistenceEligible = false;
                    record.CoexistenceFailureReason = reason;
                    record.ViabilityState =
                        EdgeWearViabilityState.CoexistenceIneligible;
                    record.Candidate = false;
                    record.CandidateIndex = -1;
                    record.CandidateReason = reason;
                    record.Selected = false;
                    record.Active = false;
                    record.AttemptedBuilt = false;
                    record.Built = false;
                    record.TrialRejected = false;
                    record.Deferred = false;
                    record.Rejected = false;
                    record.MaterializedWidth = 0f;
                    record.MaterializedWidthScale = 0f;
                    record.WidthReduced = false;
                    record.FinalReason = reason;
                }
                RecalculateEdgeWearCoverageAudit(coverage);
                SynchronizePlaneCutCoexistenceExclusionEvidence(
                    coverage,
                    ref result);
                result.SelectedEdgeCount = coverage.SelectedCount;
                result.ActiveEdgeCount = coverage.ActiveCount;
            }
            else
            {
                result.SelectedEdgeCount = retainedCandidates.Count;
                result.ActiveEdgeCount = retainedCandidates.Count;
            }

            FinalizeEdgeWearCoverageAfterPlaneShell(
                result.CoverageAudit,
                retainedCandidates);
            result.CertifiedPlanesBuilt = retainedCandidates.Count;
            result.TrialRejectedPlanes = 0;
            result.PlanesBuilt = retainedCandidates.Count;
            result.BuiltEdgeEvidence = result.AttemptedEdgeEvidence;
            result.TrialRejectedEdgeEvidence = "none";
            result.MaterializedEdgeCoverageValid =
                IsEdgeWearCoverageMaterialized(
                    result.CoverageAudit,
                    result)
                    ? 1
                    : 0;
            if (!IsPlaneCutWinningStateFinalized(
                    retainedCandidates,
                    localityDeferredCount,
                    ref result,
                    out blocker))
            {
                result.Diagnostic = blocker;
                return false;
            }
            return true;
        }

        private static List<int> BuildPlaneCutCoexistenceConflictEdgeSet(
            PlaneCutCoexistenceFailureDossier dossier,
            List<PlaneCutBevelCandidate> allCandidates,
            List<int> excludedEdges,
            ChamferTopologyContext context,
            int maximumCount,
            PlaneCutCoexistenceTrialOutcome outcome)
        {
            HashSet<int> excluded = new HashSet<int>(
                excludedEdges ?? new List<int>());
            HashSet<int> active = new HashSet<int>();
            for (int candidateIndex = 0;
                 candidateIndex < allCandidates.Count;
                 candidateIndex++)
            {
                int edge = allCandidates[candidateIndex].SourceEdgeIndex;
                if (!excluded.Contains(edge))
                {
                    active.Add(edge);
                }
            }

            SortedSet<int> implicated = new SortedSet<int>();
            if (outcome != null)
            {
                for (int index = 0;
                     index < outcome.MissingCandidateEdgeIndices.Count;
                     index++)
                {
                    implicated.Add(
                        outcome.MissingCandidateEdgeIndices[index]);
                }
                for (int index = 0;
                     index < outcome.UnexpectedCandidateEdgeIndices.Count;
                     index++)
                {
                    implicated.Add(
                        outcome.UnexpectedCandidateEdgeIndices[index]);
                }
            }
            if (dossier != null)
            {
                AddPlaneCutActiveDossierEdge(
                    dossier.VictimEdge,
                    active,
                    implicated);
                AddPlaneCutActiveDossierEdge(
                    dossier.ForeignEdge,
                    active,
                    implicated);
                AddPlaneCutActiveDossierEdges(
                    dossier.LinkedEdges,
                    active,
                    implicated);
                AddPlaneCutActiveDossierEdges(
                    dossier.IncidentStarEdges,
                    active,
                    implicated);
            }

            List<PlaneCutBevelCandidate> ordered =
                new List<PlaneCutBevelCandidate>();
            for (int candidateIndex = 0;
                 candidateIndex < allCandidates.Count;
                 candidateIndex++)
            {
                PlaneCutBevelCandidate candidate =
                    allCandidates[candidateIndex];
                if (active.Contains(candidate.SourceEdgeIndex) &&
                    implicated.Contains(candidate.SourceEdgeIndex))
                {
                    ordered.Add(candidate);
                }
            }
            ordered.Sort((left, right) =>
                ComparePlaneCutBacktrackCandidates(
                    left,
                    right,
                    context));
            List<int> result = new List<int>();
            for (int candidateIndex = 0;
                 candidateIndex < ordered.Count &&
                 result.Count < maximumCount;
                 candidateIndex++)
            {
                result.Add(ordered[candidateIndex].SourceEdgeIndex);
            }
            return result;
        }

        private static void AddPlaneCutActiveDossierEdge(
            int edge,
            HashSet<int> active,
            SortedSet<int> destination)
        {
            if (edge >= 0 && active.Contains(edge))
            {
                destination.Add(edge);
            }
        }

        private static void AddPlaneCutActiveDossierEdges(
            List<int> edges,
            HashSet<int> active,
            SortedSet<int> destination)
        {
            if (edges == null)
            {
                return;
            }
            for (int index = 0; index < edges.Count; index++)
            {
                AddPlaneCutActiveDossierEdge(
                    edges[index],
                    active,
                    destination);
            }
        }

        private static void AddPlaneCutIncidentCandidateEdges(
            List<PlaneCutBevelCandidate> candidates,
            int vertexIndex,
            SortedSet<int> destination)
        {
            for (int candidateIndex = 0;
                 candidateIndex < candidates.Count;
                 candidateIndex++)
            {
                PlaneCutBevelCandidate candidate = candidates[candidateIndex];
                if (candidate.VertexA == vertexIndex ||
                    candidate.VertexB == vertexIndex)
                {
                    destination.Add(candidate.SourceEdgeIndex);
                }
            }
        }

        private static string ResolvePlaneCutCoexistenceExclusionReason(
            PlaneCutCoexistenceFailureDossier dossier,
            PlaneCutCoexistenceTrialOutcome outcome)
        {
            if (outcome != null &&
                !outcome.CandidateConservationValid)
            {
                return "candidate-conservation-incompatible";
            }
            if (dossier == null)
            {
                return "coexistence-incompatible";
            }
            switch (dossier.Category)
            {
                case PlaneCutCoexistenceFailureCategory.PlaneBand:
                    return "plane-band-incompatible";
                case PlaneCutCoexistenceFailureCategory.StrictIntersection:
                case PlaneCutCoexistenceFailureCategory.TJunction:
                    return "plane-pair-incompatible";
                case PlaneCutCoexistenceFailureCategory.MissingJunction:
                    return "source-vertex-star-incompatible";
                case PlaneCutCoexistenceFailureCategory.CandidateConservation:
                    return "candidate-conservation-incompatible";
                default:
                    return "coexistence-incompatible";
            }
        }

        private static PlaneCutCoexistenceFailureDossier
            BuildPlaneCutCoexistenceFailureDossier(
                PlaneCutBevelAuditResult audit,
                PlaneCutTopologyScaleTrialRecord trial,
                string blocker,
                List<PlaneCutBevelCandidate> originalCandidates)
        {
            PlaneCutCoexistenceFailureDossier dossier =
                new PlaneCutCoexistenceFailureDossier
                {
                    Stage = trial == null ||
                            string.IsNullOrEmpty(trial.FailureStage)
                        ? "none"
                        : trial.FailureStage,
                    Diagnostic = blocker ?? string.Empty
                };
            if (trial != null && trial.FullyValid == 1)
            {
                return dossier;
            }
            if (string.Equals(
                    blocker,
                    "candidate-conservation-failed",
                    StringComparison.Ordinal))
            {
                dossier.Category =
                    PlaneCutCoexistenceFailureCategory
                        .CandidateConservation;
                return dossier;
            }
            PlaneCutNumericalRepairTelemetry numerical =
                audit.NumericalRepairs;
            if (numerical != null &&
                numerical.FirstExactFailureRecorded == 1)
            {
                dossier.Category =
                    PlaneCutCoexistenceFailureCategory
                        .StrictIntersection;
                if (numerical.FirstExactFailureOwnerProvenanceKind ==
                    PolygonFaceProvenanceKind.EdgeBevelPlane)
                {
                    dossier.VictimEdge =
                        numerical.FirstExactFailureOwnerProvenanceIndex;
                    AddPlaneCutUniqueDossierEdge(
                        dossier.LinkedEdges,
                        dossier.VictimEdge);
                }
                if (numerical.FirstExactFailureCutProvenanceKind ==
                    PolygonFaceProvenanceKind.EdgeBevelPlane)
                {
                    dossier.ForeignEdge =
                        numerical.FirstExactFailureCutProvenanceIndex;
                    AddPlaneCutUniqueDossierEdge(
                        dossier.LinkedEdges,
                        dossier.ForeignEdge);
                }
                return dossier;
            }

            PlaneCutOpenEdgeFailureRecord? openFailure =
                FindPlaneCutSourceVertexOpenFailure(audit);
            if (openFailure.HasValue)
            {
                PlaneCutOpenEdgeFailureRecord failure =
                    openFailure.Value;
                dossier.Category =
                    PlaneCutCoexistenceFailureCategory.MissingJunction;
                dossier.Stage = string.IsNullOrEmpty(
                        failure.FirstFailureStage)
                    ? dossier.Stage
                    : failure.FirstFailureStage;
                dossier.SourceVertex =
                    failure.AssociatedSourceVertex;
                BuildPlaneCutImmutableIncidentStar(
                    originalCandidates,
                    dossier.SourceVertex,
                    dossier.IncidentStarEdges);
                dossier.LinkedEdges.AddRange(
                    dossier.IncidentStarEdges);
                dossier.OpenEdgeCount = Mathf.Max(
                    1,
                    audit.OpenEdgeCount);
                return dossier;
            }

            PlaneCutTJunctionFailureRecord tJunction =
                FindPlaneCutCoexistenceTJunctionFailure(audit);
            if (tJunction != null)
            {
                dossier.Category =
                    PlaneCutCoexistenceFailureCategory.TJunction;
                dossier.Stage = string.IsNullOrEmpty(tJunction.Stage)
                    ? dossier.Stage
                    : tJunction.Stage;
                AddPlaneCutUniqueDossierEdges(
                    dossier.LinkedEdges,
                    tJunction.LinkedEdgeIndices);
                dossier.TJunctionCount = Mathf.Max(
                    1,
                    audit.TJunctionCount);
                return dossier;
            }
            PlaneCutRetryFailureDossier retryFailure =
                ResolveLatestPlaneCutRetryFailureDossier(audit);
            if (retryFailure != null &&
                retryFailure.TJunctionCount > 0 &&
                retryFailure.LinkedEdgeIndices.Count > 0)
            {
                dossier.Category =
                    PlaneCutCoexistenceFailureCategory.TJunction;
                dossier.Stage = string.IsNullOrEmpty(retryFailure.Stage)
                    ? dossier.Stage
                    : retryFailure.Stage;
                AddPlaneCutUniqueDossierEdges(
                    dossier.LinkedEdges,
                    retryFailure.LinkedEdgeIndices);
                dossier.TJunctionCount = retryFailure.TJunctionCount;
                return dossier;
            }

            if (audit.EdgeConflictVictimEdgeIndex >= 0 &&
                audit.EdgeConflictForeignEdgeIndex >= 0)
            {
                dossier.Category =
                    PlaneCutCoexistenceFailureCategory.PlaneBand;
                dossier.Stage = "BandIntegrity";
                dossier.VictimEdge =
                    audit.EdgeConflictVictimEdgeIndex;
                dossier.ForeignEdge =
                    audit.EdgeConflictForeignEdgeIndex;
                dossier.SourceVertex = audit.EdgeConflictVertexIndex;
                if (dossier.SourceVertex >= 0)
                {
                    BuildPlaneCutImmutableIncidentStar(
                        originalCandidates,
                        dossier.SourceVertex,
                        dossier.IncidentStarEdges);
                }
                AddPlaneCutUniqueDossierEdge(
                    dossier.LinkedEdges,
                    dossier.VictimEdge);
                AddPlaneCutUniqueDossierEdge(
                    dossier.LinkedEdges,
                    dossier.ForeignEdge);
                return dossier;
            }
            if (trial != null &&
                trial.BandVictimEdgeIndex >= 0 &&
                trial.BandForeignEdgeIndex >= 0)
            {
                dossier.Category =
                    PlaneCutCoexistenceFailureCategory.PlaneBand;
                dossier.Stage = "BandIntegrity";
                dossier.VictimEdge = trial.BandVictimEdgeIndex;
                dossier.ForeignEdge = trial.BandForeignEdgeIndex;
                AddPlaneCutUniqueDossierEdge(
                    dossier.LinkedEdges,
                    dossier.VictimEdge);
                AddPlaneCutUniqueDossierEdge(
                    dossier.LinkedEdges,
                    dossier.ForeignEdge);
                return dossier;
            }

            dossier.OpenEdgeCount = audit.OpenEdgeCount;
            dossier.TJunctionCount = audit.TJunctionCount;
            dossier.NonPlanarCount = audit.FaceQualityNonPlanarCount;
            if (audit.FaceQualityNonPlanarCount > 0 ||
                audit.InvalidFaceCount > 0)
            {
                dossier.Category =
                    PlaneCutCoexistenceFailureCategory.FaceQuality;
                return dossier;
            }
            if (trial != null)
            {
                if (string.Equals(
                        trial.FailureCause,
                        "retained-volume",
                        StringComparison.Ordinal))
                {
                    dossier.Category =
                        PlaneCutCoexistenceFailureCategory
                            .RetainedVolume;
                    return dossier;
                }
                if (string.Equals(
                        trial.FailureCause,
                        "source-bounds",
                        StringComparison.Ordinal))
                {
                    dossier.Category =
                        PlaneCutCoexistenceFailureCategory.SourceBounds;
                    return dossier;
                }
                if (string.Equals(
                        trial.FailureCause,
                        "one-surface-render",
                        StringComparison.Ordinal) ||
                    string.Equals(
                        trial.FailureCause,
                        "preview-mesh",
                        StringComparison.Ordinal))
                {
                    dossier.Category =
                        PlaneCutCoexistenceFailureCategory
                            .SurfaceTriangulation;
                    return dossier;
                }
            }
            dossier.Category = audit.OpenEdgeCount > 0 ||
                    audit.NonManifoldEdgeCount > 0
                ? PlaneCutCoexistenceFailureCategory.OpenBoundary
                : PlaneCutCoexistenceFailureCategory.General;
            return dossier;
        }

        private static PlaneCutCoexistenceFailureDossier
            ResolvePlaneCutEffectiveFailureDossier(
                PlaneCutCoexistenceFailureDossier direct,
                PlaneCutCoexistenceFailureDossier parent,
                List<int> excludedEdges,
                List<PlaneCutBevelCandidate> originalCandidates)
        {
            if (HasPlaneCutAuthoritativeFailureDossier(direct))
            {
                return ClonePlaneCutCoexistenceFailureDossier(direct);
            }
            if (!HasPlaneCutExpandableFailureDossier(parent))
            {
                return direct == null
                    ? new PlaneCutCoexistenceFailureDossier
                    {
                        Category =
                            PlaneCutCoexistenceFailureCategory.General
                    }
                    : ClonePlaneCutCoexistenceFailureDossier(direct);
            }

            PlaneCutCoexistenceFailureDossier inherited =
                ClonePlaneCutCoexistenceFailureDossier(parent);
            inherited.Stage = "inherited:" +
                (string.IsNullOrEmpty(parent.Stage)
                    ? "unknown"
                    : parent.Stage);
            if (direct != null &&
                !string.IsNullOrEmpty(direct.Diagnostic))
            {
                inherited.Diagnostic = direct.Diagnostic;
            }
            if (inherited.Category ==
                    PlaneCutCoexistenceFailureCategory.MissingJunction &&
                inherited.IncidentStarEdges.Count == 0 &&
                inherited.SourceVertex >= 0)
            {
                BuildPlaneCutImmutableIncidentStar(
                    originalCandidates,
                    inherited.SourceVertex,
                    inherited.IncidentStarEdges);
            }
            return inherited;
        }

        private static bool HasPlaneCutAuthoritativeFailureDossier(
            PlaneCutCoexistenceFailureDossier dossier)
        {
            if (dossier == null)
            {
                return false;
            }
            return dossier.Category !=
                    PlaneCutCoexistenceFailureCategory.None &&
                dossier.Category !=
                    PlaneCutCoexistenceFailureCategory.General &&
                dossier.Category !=
                    PlaneCutCoexistenceFailureCategory.OpenBoundary;
        }

        private static bool HasPlaneCutExpandableFailureDossier(
            PlaneCutCoexistenceFailureDossier dossier)
        {
            if (dossier == null)
            {
                return false;
            }
            return dossier.Category ==
                    PlaneCutCoexistenceFailureCategory.PlaneBand ||
                dossier.Category ==
                    PlaneCutCoexistenceFailureCategory
                        .StrictIntersection ||
                dossier.Category ==
                    PlaneCutCoexistenceFailureCategory.MissingJunction ||
                dossier.Category ==
                    PlaneCutCoexistenceFailureCategory.TJunction;
        }

        private static PlaneCutCoexistenceFailureDossier
            ClonePlaneCutCoexistenceFailureDossier(
                PlaneCutCoexistenceFailureDossier source)
        {
            if (source == null)
            {
                return null;
            }
            PlaneCutCoexistenceFailureDossier clone =
                new PlaneCutCoexistenceFailureDossier
                {
                    Category = source.Category,
                    Stage = source.Stage,
                    SourceVertex = source.SourceVertex,
                    VictimEdge = source.VictimEdge,
                    ForeignEdge = source.ForeignEdge,
                    OpenEdgeCount = source.OpenEdgeCount,
                    TJunctionCount = source.TJunctionCount,
                    NonPlanarCount = source.NonPlanarCount,
                    Diagnostic = source.Diagnostic
                };
            clone.LinkedEdges.AddRange(source.LinkedEdges);
            clone.IncidentStarEdges.AddRange(source.IncidentStarEdges);
            return clone;
        }

        private static PlaneCutOpenEdgeFailureRecord?
            FindPlaneCutSourceVertexOpenFailure(
                PlaneCutBevelAuditResult audit)
        {
            if (audit.OpenEdgeFailures != null)
            {
                for (int index = 0;
                     index < audit.OpenEdgeFailures.Count;
                     index++)
                {
                    PlaneCutOpenEdgeFailureRecord failure =
                        audit.OpenEdgeFailures[index];
                    if (failure.AssociatedSourceVertex >= 0)
                    {
                        return failure;
                    }
                }
            }
            PlaneCutRetryFailureDossier retry =
                ResolveLatestPlaneCutRetryFailureDossier(audit);
            if (retry != null)
            {
                for (int index = 0;
                     index < retry.OpenEdgeFailures.Count;
                     index++)
                {
                    PlaneCutOpenEdgeFailureRecord failure =
                        retry.OpenEdgeFailures[index];
                    if (failure.AssociatedSourceVertex >= 0)
                    {
                        return failure;
                    }
                }
            }
            return null;
        }

        private static PlaneCutTJunctionFailureRecord
            FindPlaneCutCoexistenceTJunctionFailure(
                PlaneCutBevelAuditResult audit)
        {
            if (audit.TJunctionFailures != null &&
                audit.TJunctionFailures.Count > 0)
            {
                return audit.TJunctionFailures[0];
            }
            PlaneCutRetryFailureDossier retry =
                ResolveLatestPlaneCutRetryFailureDossier(audit);
            if (retry != null &&
                retry.TJunctionFailures.Count > 0)
            {
                return retry.TJunctionFailures[0];
            }
            return null;
        }

        private static PlaneCutRetryFailureDossier
            ResolveLatestPlaneCutRetryFailureDossier(
                PlaneCutBevelAuditResult audit)
        {
            return audit.RetryFailureDossiers == null ||
                    audit.RetryFailureDossiers.Count == 0
                ? null
                : audit.RetryFailureDossiers[
                    audit.RetryFailureDossiers.Count - 1];
        }

        private static void BuildPlaneCutImmutableIncidentStar(
            List<PlaneCutBevelCandidate> originalCandidates,
            int sourceVertex,
            List<int> destination)
        {
            destination.Clear();
            if (originalCandidates == null || sourceVertex < 0)
            {
                return;
            }
            SortedSet<int> star = new SortedSet<int>();
            for (int candidateIndex = 0;
                 candidateIndex < originalCandidates.Count;
                 candidateIndex++)
            {
                PlaneCutBevelCandidate candidate =
                    originalCandidates[candidateIndex];
                if (candidate.VertexA == sourceVertex ||
                    candidate.VertexB == sourceVertex)
                {
                    star.Add(candidate.SourceEdgeIndex);
                }
            }
            destination.AddRange(star);
        }

        private static void AddPlaneCutUniqueDossierEdges(
            List<int> destination,
            List<int> source)
        {
            if (source == null)
            {
                return;
            }
            for (int index = 0; index < source.Count; index++)
            {
                AddPlaneCutUniqueDossierEdge(
                    destination,
                    source[index]);
            }
        }

        private static void AddPlaneCutUniqueDossierEdge(
            List<int> destination,
            int edge)
        {
            if (edge >= 0 && !destination.Contains(edge))
            {
                destination.Add(edge);
                destination.Sort();
            }
        }

        private static bool IsPlaneCutWinningStateFinalized(
            List<PlaneCutBevelCandidate> retainedCandidates,
            int localityDeferredCount,
            ref PlaneCutBevelAuditResult result,
            out string blocker)
        {
            EdgeWearCoverageAudit coverage = result.CoverageAudit;
            int retainedCount = retainedCandidates == null
                ? 0
                : retainedCandidates.Count;
            bool exhaustiveDenominatorValid = coverage != null &&
                (!coverage.RequireAllGeometricCandidates ||
                 coverage.CoexistenceEligibleCount ==
                    coverage.SelectedCount);
            bool valid = coverage != null &&
                exhaustiveDenominatorValid &&
                coverage.SelectedCount ==
                    coverage.AttemptedBuiltCount &&
                coverage.AttemptedBuiltCount == coverage.BuiltCount &&
                coverage.BuiltCount == retainedCount &&
                coverage.UnresolvedWidthInactiveCount == 0 &&
                coverage.TrialRejectedCount == 0 &&
                coverage.DeferredCount == 0 &&
                coverage.RejectedCount == 0 &&
                coverage.UnmappedCount == 0 &&
                result.AttemptedPlanesBuilt == retainedCount &&
                result.CertifiedPlanesBuilt == retainedCount &&
                result.TrialRejectedPlanes == 0 &&
                localityDeferredCount == 0 &&
                result.MaterializedEdgeCoverageValid == 1;
            if (valid)
            {
                blocker = string.Empty;
                return true;
            }
            blocker = "winning-state-finalization-failed:" +
                "coexistence/selected/attempted/built/retained=" +
                (coverage == null
                    ? "missing"
                    : coverage.CoexistenceEligibleCount + "/" +
                        coverage.SelectedCount + "/" +
                        coverage.AttemptedBuiltCount + "/" +
                        coverage.BuiltCount) +
                "/" + retainedCount +
                ",requireAllGeometricCandidates=" +
                (coverage != null &&
                 coverage.RequireAllGeometricCandidates ? "1" : "0") +
                ",widthInactive/unresolvedWidthInactive/" +
                    "trialRejected/deferred/rejected/unmapped=" +
                (coverage == null
                    ? "missing"
                    : coverage.WidthInactiveCount + "/" +
                        coverage.UnresolvedWidthInactiveCount + "/" +
                        coverage.TrialRejectedCount + "/" +
                        coverage.DeferredCount + "/" +
                        coverage.RejectedCount + "/" +
                        coverage.UnmappedCount) +
                ",planes=" + result.AttemptedPlanesBuilt + "/" +
                    result.CertifiedPlanesBuilt + "/" +
                    result.TrialRejectedPlanes +
                ",localityDeferred=" + localityDeferredCount +
                ",materialized=" +
                    result.MaterializedEdgeCoverageValid;
            return false;
        }

        private static string BuildPlaneCutCoexistenceFailureSignature(
            PlaneCutBevelAuditResult audit,
            string blocker)
        {
            if (string.Equals(
                    blocker,
                    "candidate-conservation-failed",
                    StringComparison.Ordinal))
            {
                return "candidate-conservation";
            }
            PlaneCutNumericalRepairTelemetry numerical =
                audit.NumericalRepairs;
            if (numerical != null &&
                numerical.FirstExactFailureRecorded == 1)
            {
                return "strict:" +
                    numerical.FirstExactFailureOwnerProvenanceKind + ":" +
                    numerical.FirstExactFailureOwnerProvenanceIndex + "/" +
                    numerical.FirstExactFailureCutProvenanceKind + ":" +
                    numerical.FirstExactFailureCutProvenanceIndex;
            }
            if (audit.OpenEdgeFailures != null &&
                audit.OpenEdgeFailures.Count > 0)
            {
                PlaneCutOpenEdgeFailureRecord failure =
                    audit.OpenEdgeFailures[0];
                return "open:" + failure.FirstFailureStage + ":" +
                    failure.AssociatedSourceVertex + ":" + failure.Cause;
            }
            if (audit.TJunctionFailures != null &&
                audit.TJunctionFailures.Count > 0)
            {
                PlaneCutTJunctionFailureRecord failure =
                    audit.TJunctionFailures[0];
                return "tjunction:" + failure.Stage + ":" +
                    FormatPlaneCutEdgeIndexEvidence(
                        failure.LinkedEdgeIndices);
            }
            if (audit.EdgeConflictVictimEdgeIndex >= 0 ||
                audit.EdgeConflictForeignEdgeIndex >= 0)
            {
                return "band:" + audit.EdgeConflictVictimEdgeIndex + "/" +
                    audit.EdgeConflictForeignEdgeIndex;
            }
            return string.IsNullOrEmpty(blocker)
                ? "unknown"
                : blocker;
        }

        private static string BuildPlaneCutCoexistenceTrialCacheKey(
            List<int> excludedEdges,
            List<PlaneCutBevelCandidate> candidates,
            Dictionary<int, float> scaleByEdge)
        {
            List<int> edges = new List<int>();
            for (int candidateIndex = 0;
                 candidateIndex < candidates.Count;
                 candidateIndex++)
            {
                edges.Add(candidates[candidateIndex].SourceEdgeIndex);
            }
            edges.Sort();
            StringBuilder builder = new StringBuilder();
            builder.Append("excluded:");
            builder.Append(FormatPlaneCutEdgeIndexEvidence(
                excludedEdges));
            builder.Append('|');
            for (int edgeOrdinal = 0;
                 edgeOrdinal < edges.Count;
                 edgeOrdinal++)
            {
                if (edgeOrdinal > 0)
                {
                    builder.Append('/');
                }
                int edge = edges[edgeOrdinal];
                builder.Append(edge);
                builder.Append('=');
                builder.Append(scaleByEdge.TryGetValue(edge, out float scale)
                    ? scale.ToString("R")
                    : "1");
            }
            return builder.ToString();
        }

        private static void
            SynchronizePlaneCutCoexistenceExclusionEvidence(
                EdgeWearCoverageAudit coverage,
                ref PlaneCutBevelAuditResult result)
        {
            if (coverage == null || coverage.Records == null)
            {
                return;
            }

            SortedSet<int> excludedEdges = new SortedSet<int>();
            Dictionary<int, string> reasonByExcludedEdge =
                new Dictionary<int, string>();
            for (int recordIndex = 0;
                 recordIndex < coverage.Records.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord record =
                    coverage.Records[recordIndex];
                if (record.ViabilityState !=
                        EdgeWearViabilityState.CoexistenceIneligible ||
                    record.SourceEdgeIndex < 0)
                {
                    continue;
                }

                excludedEdges.Add(record.SourceEdgeIndex);
                string reason = string.IsNullOrEmpty(
                        record.CoexistenceFailureReason)
                    ? record.FinalReason
                    : record.CoexistenceFailureReason;
                reasonByExcludedEdge[record.SourceEdgeIndex] =
                    string.IsNullOrEmpty(reason)
                        ? "coexistence-incompatible"
                        : reason;
            }

            result.CoexistenceExclusionCount = excludedEdges.Count;
            result.CoexistenceExcludedEdgeEvidence =
                FormatPlaneCutEdgeIndexEvidence(
                    new List<int>(excludedEdges));
            result.CoexistenceExclusionReasonEvidence =
                FormatPlaneCutCoexistenceReasonEvidence(
                    excludedEdges,
                    reasonByExcludedEdge);
        }

        private static List<int> BuildPlaneCutExpectedCoexistenceEdgeSet(
            EdgeWearCoverageAudit coverage,
            List<PlaneCutBevelCandidate> candidates)
        {
            SortedSet<int> expected = new SortedSet<int>();
            if (coverage != null)
            {
                for (int recordIndex = 0;
                     recordIndex < coverage.Records.Count;
                     recordIndex++)
                {
                    EdgeWearEdgeLifecycleRecord record =
                        coverage.Records[recordIndex];
                    if (record.Selected &&
                        record.Active &&
                        !record.WidthInactive &&
                        record.CoexistenceEligible &&
                        !record.Deferred &&
                        !record.Rejected &&
                        record.ViabilityState !=
                            EdgeWearViabilityState.CoexistenceIneligible)
                    {
                        expected.Add(record.SourceEdgeIndex);
                    }
                }
            }
            if (coverage == null && candidates != null)
            {
                for (int candidateIndex = 0;
                     candidateIndex < candidates.Count;
                     candidateIndex++)
                {
                    expected.Add(
                        candidates[candidateIndex].SourceEdgeIndex);
                }
            }
            return new List<int>(expected);
        }

        private static void PopulatePlaneCutCandidateConservation(
            List<int> rootExpectedEdgeIndices,
            List<int> excludedEdgeIndices,
            List<PlaneCutBevelCandidate> actualCandidates,
            int attemptedBuiltCount,
            bool geometryFullyValid,
            ref PlaneCutCoexistenceTrialOutcome outcome)
        {
            HashSet<int> excluded = new HashSet<int>(
                excludedEdgeIndices ?? new List<int>());
            SortedSet<int> expected = new SortedSet<int>();
            for (int index = 0;
                 index < rootExpectedEdgeIndices.Count;
                 index++)
            {
                int edge = rootExpectedEdgeIndices[index];
                if (!excluded.Contains(edge))
                {
                    expected.Add(edge);
                }
            }
            SortedSet<int> actual = new SortedSet<int>();
            if (actualCandidates != null)
            {
                for (int candidateIndex = 0;
                     candidateIndex < actualCandidates.Count;
                     candidateIndex++)
                {
                    actual.Add(
                        actualCandidates[candidateIndex].SourceEdgeIndex);
                }
            }

            outcome.ExpectedCandidateEdgeIndices.Clear();
            outcome.ActualCandidateEdgeIndices.Clear();
            outcome.MissingCandidateEdgeIndices.Clear();
            outcome.UnexpectedCandidateEdgeIndices.Clear();
            outcome.ExpectedCandidateEdgeIndices.AddRange(expected);
            outcome.ActualCandidateEdgeIndices.AddRange(actual);
            foreach (int edge in expected)
            {
                if (!actual.Contains(edge))
                {
                    outcome.MissingCandidateEdgeIndices.Add(edge);
                }
            }
            foreach (int edge in actual)
            {
                if (!expected.Contains(edge))
                {
                    outcome.UnexpectedCandidateEdgeIndices.Add(edge);
                }
            }
            outcome.ExpectedCandidateCount = expected.Count;
            outcome.ActualCandidateCount = actual.Count;
            outcome.CertifiedCandidateCount = geometryFullyValid
                ? actual.Count
                : 0;
            outcome.CandidateConservationValid =
                outcome.MissingCandidateEdgeIndices.Count == 0 &&
                outcome.UnexpectedCandidateEdgeIndices.Count == 0 &&
                attemptedBuiltCount == actual.Count;
        }

        private static void RecordPlaneCutCandidateConservationFailure(
            PlaneCutCoexistenceTrialOutcome outcome,
            ref PlaneCutBevelAuditResult result)
        {
            result.CoexistenceCandidateConservationFailureCount++;
            result.CoexistenceCandidateExpectedEvidence =
                FormatPlaneCutEdgeIndexEvidence(
                    outcome.ExpectedCandidateEdgeIndices);
            result.CoexistenceCandidateActualEvidence =
                FormatPlaneCutEdgeIndexEvidence(
                    outcome.ActualCandidateEdgeIndices);
            result.CoexistenceCandidateMissingEvidence =
                FormatPlaneCutEdgeIndexEvidence(
                    outcome.MissingCandidateEdgeIndices);
            result.CoexistenceCandidateUnexpectedEvidence =
                FormatPlaneCutEdgeIndexEvidence(
                    outcome.UnexpectedCandidateEdgeIndices);
        }

        private static string BuildPlaneCutCoexistenceSearchNodeKey(
            string outcomeKey,
            PlaneCutCoexistenceFailureDossier dossier)
        {
            if (dossier == null)
            {
                return outcomeKey + "|dossier:none";
            }
            return outcomeKey + "|dossier:" +
                dossier.Category + ":" +
                dossier.SourceVertex + ":" +
                dossier.VictimEdge + ":" +
                dossier.ForeignEdge + ":" +
                FormatPlaneCutEdgeIndexEvidence(dossier.LinkedEdges) +
                ":" +
                FormatPlaneCutEdgeIndexEvidence(
                    dossier.IncidentStarEdges);
        }

        private static string BuildPlaneCutCoexistenceSearchStateKey(
            List<int> excludedEdges,
            Dictionary<int, float> scaleByEdge)
        {
            List<int> orderedEdges = new List<int>(
                scaleByEdge.Keys);
            orderedEdges.Sort();
            StringBuilder builder = new StringBuilder();
            builder.Append("excluded:");
            builder.Append(FormatPlaneCutEdgeIndexEvidence(
                excludedEdges));
            builder.Append('|');
            for (int index = 0; index < orderedEdges.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append('/');
                }
                int edge = orderedEdges[index];
                builder.Append(edge);
                builder.Append('=');
                builder.Append(scaleByEdge[edge].ToString("R"));
            }
            return builder.ToString();
        }

        private static Dictionary<int, string>
            ClonePlaneCutCoexistenceReasonMap(
                Dictionary<int, string> source)
        {
            return source == null
                ? new Dictionary<int, string>()
                : new Dictionary<int, string>(source);
        }

        private static int ComparePlaneCutCoexistenceSearchNodes(
            PlaneCutCoexistenceSearchNode left,
            PlaneCutCoexistenceSearchNode right,
            List<PlaneCutBevelCandidate> allCandidates)
        {
            int objective = ComparePlaneCutCoexistenceOutcomes(
                left.Outcome,
                right.Outcome,
                allCandidates);
            if (objective != 0)
            {
                return objective;
            }
            float leftScale = ResolveMinimumPlaneCutScale(
                left.Outcome.ScaleByEdge);
            float rightScale = ResolveMinimumPlaneCutScale(
                right.Outcome.ScaleByEdge);
            int scale = rightScale.CompareTo(leftScale);
            if (scale != 0)
            {
                return scale;
            }
            return string.CompareOrdinal(
                left.Outcome.CacheKey,
                right.Outcome.CacheKey);
        }

        private static void RecordPlaneCutCoexistenceSearchState(
            int stateIndex,
            string failureCategory,
            List<int> implicatedEdges,
            PlaneCutCoexistenceFailureDossier dossier,
            PlaneCutCoexistenceTrialOutcome outcome,
            ref PlaneCutBevelAuditResult result)
        {
            if (result.CoexistenceSearchStates == null)
            {
                result.CoexistenceSearchStates =
                    new List<PlaneCutCoexistenceSearchStateRecord>();
            }
            if (!outcome.CandidateConservationValid)
            {
                result.CoexistenceCandidateExpectedEvidence =
                    FormatPlaneCutEdgeIndexEvidence(
                        outcome.ExpectedCandidateEdgeIndices);
                result.CoexistenceCandidateActualEvidence =
                    FormatPlaneCutEdgeIndexEvidence(
                        outcome.ActualCandidateEdgeIndices);
                result.CoexistenceCandidateMissingEvidence =
                    FormatPlaneCutEdgeIndexEvidence(
                        outcome.MissingCandidateEdgeIndices);
                result.CoexistenceCandidateUnexpectedEvidence =
                    FormatPlaneCutEdgeIndexEvidence(
                        outcome.UnexpectedCandidateEdgeIndices);
            }
            result.CoexistenceSearchStates.Add(
                new PlaneCutCoexistenceSearchStateRecord
                {
                    StateIndex = stateIndex,
                    Depth = outcome.ExcludedEdgeIndices.Count,
                    ExclusionEvidence =
                        FormatPlaneCutEdgeIndexEvidence(
                            outcome.ExcludedEdgeIndices),
                    FailureCategory = string.IsNullOrEmpty(failureCategory)
                        ? "unknown"
                        : failureCategory,
                    FailureStage = dossier == null ||
                            string.IsNullOrEmpty(dossier.Stage)
                        ? "none"
                        : dossier.Stage,
                    SourceVertex = dossier == null
                        ? -1
                        : dossier.SourceVertex,
                    VictimEdge = dossier == null
                        ? -1
                        : dossier.VictimEdge,
                    ForeignEdge = dossier == null
                        ? -1
                        : dossier.ForeignEdge,
                    LinkedEdgeEvidence = dossier == null
                        ? "none"
                        : FormatPlaneCutEdgeIndexEvidence(
                            dossier.LinkedEdges),
                    IncidentStarEvidence = dossier == null
                        ? "none"
                        : FormatPlaneCutEdgeIndexEvidence(
                            dossier.IncidentStarEdges),
                    ImplicatedEdgeEvidence =
                        FormatPlaneCutEdgeIndexEvidence(
                            implicatedEdges),
                    ExpectedCandidateCount =
                        outcome.ExpectedCandidateCount,
                    ActualCandidateCount =
                        outcome.ActualCandidateCount,
                    CertifiedCandidateCount =
                        outcome.CertifiedCandidateCount,
                    ExpectedCandidateEvidence =
                        FormatPlaneCutEdgeIndexEvidence(
                            outcome.ExpectedCandidateEdgeIndices),
                    ActualCandidateEvidence =
                        FormatPlaneCutEdgeIndexEvidence(
                            outcome.ActualCandidateEdgeIndices),
                    MissingCandidateEvidence =
                        FormatPlaneCutEdgeIndexEvidence(
                            outcome.MissingCandidateEdgeIndices),
                    UnexpectedCandidateEvidence =
                        FormatPlaneCutEdgeIndexEvidence(
                            outcome.UnexpectedCandidateEdgeIndices),
                    CandidateConservationValid =
                        outcome.CandidateConservationValid ? 1 : 0,
                    MinimumWidthScale = ResolveMinimumPlaneCutScale(
                        outcome.ScaleByEdge),
                    FullyValid = outcome.FullyValid ? 1 : 0,
                    FailureSignature = outcome.FailureSignature ??
                        string.Empty
                });
        }

        private static int ComparePlaneCutCoexistenceOutcomes(
            PlaneCutCoexistenceTrialOutcome left,
            PlaneCutCoexistenceTrialOutcome right,
            List<PlaneCutBevelCandidate> allCandidates)
        {
            int count = left.ExcludedEdgeIndices.Count.CompareTo(
                right.ExcludedEdgeIndices.Count);
            if (count != 0)
            {
                return count;
            }
            float leftWidth = CalculatePlaneCutExcludedWidth(
                left.ExcludedEdgeIndices,
                allCandidates);
            float rightWidth = CalculatePlaneCutExcludedWidth(
                right.ExcludedEdgeIndices,
                allCandidates);
            int width = leftWidth.CompareTo(rightWidth);
            if (width != 0)
            {
                return width;
            }
            float leftScore = CalculatePlaneCutExcludedSelectionScore(
                left.ExcludedEdgeIndices,
                allCandidates);
            float rightScore = CalculatePlaneCutExcludedSelectionScore(
                right.ExcludedEdgeIndices,
                allCandidates);
            int score = leftScore.CompareTo(rightScore);
            if (score != 0)
            {
                return score;
            }
            return ComparePlaneCutEdgeIndexLists(
                left.ExcludedEdgeIndices,
                right.ExcludedEdgeIndices);
        }

        private static float CalculatePlaneCutExcludedWidth(
            List<int> excludedEdges,
            List<PlaneCutBevelCandidate> candidates)
        {
            HashSet<int> excluded = new HashSet<int>(excludedEdges);
            float sum = 0f;
            for (int candidateIndex = 0;
                 candidateIndex < candidates.Count;
                 candidateIndex++)
            {
                if (excluded.Contains(
                        candidates[candidateIndex].SourceEdgeIndex))
                {
                    sum += candidates[candidateIndex].Width;
                }
            }
            return sum;
        }

        private static float CalculatePlaneCutExcludedSelectionScore(
            List<int> excludedEdges,
            List<PlaneCutBevelCandidate> candidates)
        {
            HashSet<int> excluded = new HashSet<int>(excludedEdges);
            float sum = 0f;
            for (int candidateIndex = 0;
                 candidateIndex < candidates.Count;
                 candidateIndex++)
            {
                if (excluded.Contains(
                        candidates[candidateIndex].SourceEdgeIndex))
                {
                    sum += candidates[candidateIndex].SelectionScore;
                }
            }
            return sum;
        }

        private static int ComparePlaneCutEdgeIndexLists(
            List<int> left,
            List<int> right)
        {
            int count = Mathf.Min(left.Count, right.Count);
            for (int index = 0; index < count; index++)
            {
                int comparison = left[index].CompareTo(right[index]);
                if (comparison != 0)
                {
                    return comparison;
                }
            }
            return left.Count.CompareTo(right.Count);
        }

        private static string FormatPlaneCutCoexistenceReasonEvidence(
            SortedSet<int> excludedEdges,
            Dictionary<int, string> reasons)
        {
            if (excludedEdges == null || excludedEdges.Count == 0)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            foreach (int edge in excludedEdges)
            {
                if (builder.Length > 0)
                {
                    builder.Append('/');
                }
                builder.Append(edge);
                builder.Append('=');
                builder.Append(reasons.TryGetValue(edge, out string reason)
                    ? reason
                    : "coexistence-incompatible");
            }
            return builder.ToString();
        }

        private static bool IsPlaneCutRetryGeometryClean(
            PlaneCutStageSnapshot snapshot)
        {
            return snapshot.OpenEdgeCount == 0 &&
                snapshot.NonManifoldEdgeCount == 0 &&
                snapshot.TJunctionCount == 0 &&
                snapshot.InvalidFaceCount == 0 &&
                snapshot.NonPlanarFaceCount == 0;
        }

        private static Dictionary<int, float> ClonePlaneCutScaleMap(
            Dictionary<int, float> source)
        {
            return source == null
                ? new Dictionary<int, float>()
                : new Dictionary<int, float>(source);
        }

        private static string ResolvePlaneCutFirstRetryFailureStage(
            PlaneCutBevelAuditResult result)
        {
            string earliest = string.Empty;
            int earliestRank = int.MaxValue;
            ConsiderPlaneCutFailureStage(
                result.FirstOpenEdgeStage,
                ref earliest,
                ref earliestRank);
            ConsiderPlaneCutFailureStage(
                result.FirstTJunctionStage,
                ref earliest,
                ref earliestRank);
            ConsiderPlaneCutFailureStage(
                result.FirstNonPlanarStage,
                ref earliest,
                ref earliestRank);
            return string.IsNullOrEmpty(earliest)
                ? "AfterSeamRepair"
                : earliest;
        }

        private static void ConsiderPlaneCutFailureStage(
            string stage,
            ref string earliest,
            ref int earliestRank)
        {
            if (string.IsNullOrEmpty(stage))
            {
                return;
            }
            int rank = ResolvePlaneCutStageRank(stage);
            if (rank >= earliestRank)
            {
                return;
            }
            earliest = stage;
            earliestRank = rank;
        }

        private static int ResolvePlaneCutStageRank(string stage)
        {
            return stage switch
            {
                "AfterPlaneConstruction" => 0,
                "AfterSanitation" => 1,
                "AfterWeld" => 2,
                "AfterBoundaryConformity" => 3,
                "AfterSeamRepair" => 4,
                "FinalCertification" => 5,
                _ => 6
            };
        }

        private static PlaneCutSolverTransactionState
            CapturePlaneCutSolverTransactionState(
                string name,
                int passIndex,
                List<PlaneCutBevelCandidate> candidates,
                List<PolygonFace> faces,
                Dictionary<int, float> scaleByEdge,
                bool bandClean,
                bool geometryClean,
                PlaneCutStageSnapshot stage)
        {
            PlaneCutSolverTransactionState state =
                new PlaneCutSolverTransactionState
                {
                    Name = name,
                    PassIndex = passIndex,
                    BandClean = bandClean ? 1 : 0,
                    GeometryClean = geometryClean ? 1 : 0,
                    Faces = faces == null
                        ? new List<PolygonFace>()
                        : ClonePolygonFacesForPlaneCutAudit(faces),
                    ScaleByEdge = ClonePlaneCutScaleMap(scaleByEdge),
                    Stage = stage
                };
            if (candidates != null)
            {
                state.Candidates.AddRange(candidates);
            }
            return state;
        }

        private static PlaneCutRetryFailureDossier
            CapturePlaneCutRetryFailureDossier(
                int passIndex,
                string stage,
                List<PolygonFace> faces,
                ChamferTopologyContext context,
                List<PlaneCutBevelCandidate> candidates,
                List<PlaneCutVertexJunctionCandidate> junctions,
                float minimumStableEdgeLength,
                float minimumStableFaceArea,
                Dictionary<int, float> scaleByEdge,
                ref PlaneCutBevelAuditResult result)
        {
            PlaneCutRetryFailureDossier dossier =
                new PlaneCutRetryFailureDossier
                {
                    PassIndex = passIndex,
                    Stage = stage,
                    AttemptedBuiltCount = candidates == null
                        ? 0
                        : candidates.Count,
                    CandidateEdgeEvidence =
                        FormatPlaneCutCandidateEdgeEvidence(candidates),
                    ScaleEvidence = FormatPlaneCutScaleEvidence(
                        scaleByEdge,
                        CollectPlaneCutCandidateEdgeIndices(candidates))
                };

            PlaneCutBevelAuditResult scratch =
                new PlaneCutBevelAuditResult
                {
                    CoverageAudit = result.CoverageAudit,
                    EdgeConflictWidthReductions =
                        result.EdgeConflictWidthReductions,
                    FaceQualityFailures =
                        new List<PlaneCutFaceQualityFailureRecord>(),
                    OpenEdgeFailures =
                        new List<PlaneCutOpenEdgeFailureRecord>(),
                    TJunctionFailures =
                        new List<PlaneCutTJunctionFailureRecord>(),
                    FaceFirstNonPlanarStageByIdentity =
                        new Dictionary<string, string>(),
                    BoundaryConformityTouchedFaces =
                        new HashSet<string>(),
                    SeamRepairTouchedFaces =
                        new HashSet<string>(),
                    SeamRepairMaximumMovementByIdentity =
                        new Dictionary<string, float>(),
                    TopologyScaleSearchBasePass = -1,
                    TopologyScaleSearchCommittedFactor = -1f,
                    TopologyScaleSearchHighestValidFactor = -1f,
                    TopologyScaleSearchMode = "none",
                    TopologyScaleSearchTriggerEvidence = "none",
                    TopologyScaleSearchTopologyLinkedEvidence = "none",
                    FirstOpenEdgeStage = stage,
                    FirstTJunctionStage = stage,
                    FirstNonPlanarStage = stage
                };

            EdgeWearTopologyStats topology = faces == null
                ? default
                : AuditEdgeWearTopology(
                    faces,
                    minimumStableEdgeLength);
            dossier.OpenEdgeCount = topology.OpenEdgeCount;
            dossier.NonManifoldEdgeCount = topology.NonManifoldEdgeCount;
            dossier.TJunctionCount = topology.TJunctionCount;
            dossier.InvalidFaceCount = faces == null
                ? 0
                : CountInvalidPlaneCutFaces(
                    faces,
                    minimumStableFaceArea);

            if (faces != null)
            {
                for (int faceIndex = 0;
                     faceIndex < faces.Count;
                     faceIndex++)
                {
                    PolygonFace face = faces[faceIndex];
                    if (face == null ||
                        face.Feature !=
                            PolygonFaceFeature.ConvexEdgeWear)
                    {
                        continue;
                    }
                    string identity = BuildPlaneCutFaceIdentity(
                        face,
                        faceIndex);
                    if (!scratch.FaceFirstNonPlanarStageByIdentity
                            .ContainsKey(identity))
                    {
                        scratch.FaceFirstNonPlanarStageByIdentity.Add(
                            identity,
                            stage);
                    }
                }
                AuditPlaneCutFaceQuality(
                    faces,
                    junctions ??
                        new List<PlaneCutVertexJunctionCandidate>(),
                    minimumStableEdgeLength,
                    ref scratch);
                dossier.NonPlanarFaceCount =
                    scratch.FaceQualityNonPlanarCount;
                dossier.NonPlanarFaceFailures.AddRange(
                    scratch.FaceQualityFailures);

                AuditPlaneCutOpenEdgeFailures(
                    faces,
                    context,
                    candidates,
                    minimumStableEdgeLength,
                    ref scratch);
                dossier.OpenEdgeFailures.AddRange(
                    scratch.OpenEdgeFailures);

                if (topology.TJunctionCount > 0)
                {
                    CapturePlaneCutTJunctionFailures(
                        faces,
                        stage,
                        minimumStableEdgeLength,
                        candidates,
                        ref scratch);
                    dossier.TJunctionFailures.AddRange(
                        scratch.TJunctionFailures);
                }

                dossier.NonManifoldEvidence =
                    BuildPlaneCutNonManifoldFailureEvidence(
                        faces,
                        dossier.LinkedEdgeIndices);
                dossier.InvalidFaceEvidence =
                    BuildPlaneCutInvalidFaceFailureEvidence(
                        faces,
                        minimumStableFaceArea,
                        dossier.LinkedEdgeIndices);
            }

            CollectPlaneCutRetryLinkedEdges(
                dossier,
                candidates,
                minimumStableEdgeLength);
            dossier.Cause = ResolvePlaneCutRetryFailureCause(dossier);
            return dossier;
        }

        private static List<int> CollectPlaneCutCandidateEdgeIndices(
            List<PlaneCutBevelCandidate> candidates)
        {
            List<int> indices = new List<int>();
            if (candidates == null)
            {
                return indices;
            }
            for (int index = 0; index < candidates.Count; index++)
            {
                indices.Add(candidates[index].SourceEdgeIndex);
            }
            indices.Sort();
            return indices;
        }

        private static void CollectPlaneCutRetryLinkedEdges(
            PlaneCutRetryFailureDossier dossier,
            List<PlaneCutBevelCandidate> candidates,
            float minimumStableEdgeLength)
        {
            SortedSet<int> linked = new SortedSet<int>(
                dossier.LinkedEdgeIndices);
            for (int index = 0;
                 index < dossier.TJunctionFailures.Count;
                 index++)
            {
                linked.UnionWith(
                    dossier.TJunctionFailures[index].LinkedEdgeIndices);
            }
            for (int index = 0;
                 index < dossier.OpenEdgeFailures.Count;
                 index++)
            {
                PlaneCutOpenEdgeFailureRecord failure =
                    dossier.OpenEdgeFailures[index];
                AddPlaneCutFailureProvenanceEdge(
                    failure.FaceProvenanceKind,
                    failure.FaceProvenanceIndex,
                    linked);
                AddPlaneCutFailureProvenanceEdge(
                    failure.NearestFaceProvenanceKind,
                    failure.NearestFaceProvenanceIndex,
                    linked);
                AddPlaneCutCandidatePlaneMatches(
                    candidates,
                    failure.Start,
                    minimumStableEdgeLength,
                    linked);
                AddPlaneCutCandidatePlaneMatches(
                    candidates,
                    failure.End,
                    minimumStableEdgeLength,
                    linked);
            }
            for (int index = 0;
                 index < dossier.NonPlanarFaceFailures.Count;
                 index++)
            {
                PlaneCutFaceQualityFailureRecord failure =
                    dossier.NonPlanarFaceFailures[index];
                AddPlaneCutFailureProvenanceEdge(
                    failure.ProvenanceKind,
                    failure.ProvenanceIndex,
                    linked);
                AddPlaneCutCandidatePlaneMatches(
                    candidates,
                    failure.OffendingVertexPosition,
                    minimumStableEdgeLength,
                    linked);
            }
            dossier.LinkedEdgeIndices.Clear();
            dossier.LinkedEdgeIndices.AddRange(linked);
        }

        private static void AddPlaneCutFailureProvenanceEdge(
            PolygonFaceProvenanceKind provenanceKind,
            int provenanceIndex,
            SortedSet<int> linked)
        {
            if (provenanceKind ==
                    PolygonFaceProvenanceKind.EdgeBevelPlane &&
                provenanceIndex >= 0)
            {
                linked.Add(provenanceIndex);
            }
        }

        private static void AddPlaneCutCandidatePlaneMatches(
            List<PlaneCutBevelCandidate> candidates,
            Vector3 position,
            float minimumStableEdgeLength,
            SortedSet<int> linked)
        {
            if (candidates == null || !IsFinite(position))
            {
                return;
            }
            float baseTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                minimumStableEdgeLength * 0.002f);
            for (int index = 0; index < candidates.Count; index++)
            {
                PlaneCutBevelCandidate candidate = candidates[index];
                float tolerance = Mathf.Max(
                    baseTolerance,
                    candidate.PlaneTolerance);
                if (Mathf.Abs(
                        candidate.Plane.SignedDistance(position)) <=
                    tolerance)
                {
                    linked.Add(candidate.SourceEdgeIndex);
                }
            }
        }

        private static string ResolvePlaneCutRetryFailureCause(
            PlaneCutRetryFailureDossier dossier)
        {
            List<string> causes = new List<string>();
            if (dossier.OpenEdgeCount > 0)
            {
                causes.Add("open-edges:" + dossier.OpenEdgeCount);
            }
            if (dossier.NonManifoldEdgeCount > 0)
            {
                causes.Add(
                    "non-manifold-edges:" +
                    dossier.NonManifoldEdgeCount);
            }
            if (dossier.TJunctionCount > 0)
            {
                causes.Add("t-junctions:" + dossier.TJunctionCount);
            }
            if (dossier.InvalidFaceCount > 0)
            {
                causes.Add("invalid-faces:" + dossier.InvalidFaceCount);
            }
            if (dossier.NonPlanarFaceCount > 0)
            {
                causes.Add(
                    "non-planar-faces:" +
                    dossier.NonPlanarFaceCount);
            }
            return causes.Count == 0
                ? "none"
                : string.Join("+", causes);
        }

        private static string BuildPlaneCutNonManifoldFailureEvidence(
            List<PolygonFace> faces,
            List<int> linkedEdgeIndices)
        {
            if (faces == null)
            {
                return "none";
            }
            Dictionary<EdgeKey, List<int>> incidentByEdge =
                new Dictionary<EdgeKey, List<int>>();
            Dictionary<EdgeKey, Vector3> startByEdge =
                new Dictionary<EdgeKey, Vector3>();
            Dictionary<EdgeKey, Vector3> endByEdge =
                new Dictionary<EdgeKey, Vector3>();
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face == null || face.Vertices == null ||
                    face.Vertices.Count < 2)
                {
                    continue;
                }
                for (int vertexIndex = 0;
                     vertexIndex < face.Vertices.Count;
                     vertexIndex++)
                {
                    Vector3 start = face.Vertices[vertexIndex];
                    Vector3 end = face.Vertices[
                        (vertexIndex + 1) % face.Vertices.Count];
                    EdgeKey key = new EdgeKey(start, end);
                    if (!incidentByEdge.TryGetValue(
                            key,
                            out List<int> incidents))
                    {
                        incidents = new List<int>();
                        incidentByEdge.Add(key, incidents);
                        startByEdge.Add(key, start);
                        endByEdge.Add(key, end);
                    }
                    incidents.Add(faceIndex);
                }
            }

            StringBuilder builder = new StringBuilder();
            SortedSet<int> linked = new SortedSet<int>(linkedEdgeIndices);
            List<EdgeKey> orderedKeys =
                new List<EdgeKey>(incidentByEdge.Keys);
            orderedKeys.Sort((left, right) =>
                string.CompareOrdinal(
                    FormatPlaneCutVector(startByEdge[left]) + "->" +
                        FormatPlaneCutVector(endByEdge[left]),
                    FormatPlaneCutVector(startByEdge[right]) + "->" +
                        FormatPlaneCutVector(endByEdge[right])));
            for (int keyIndex = 0;
                 keyIndex < orderedKeys.Count;
                 keyIndex++)
            {
                EdgeKey key = orderedKeys[keyIndex];
                List<int> incidents = incidentByEdge[key];
                if (incidents.Count <= 2)
                {
                    continue;
                }
                incidents.Sort();
                if (builder.Length > 0)
                {
                    builder.Append('|');
                }
                builder.Append("edge=");
                builder.Append(FormatPlaneCutVector(startByEdge[key]));
                builder.Append("->");
                builder.Append(FormatPlaneCutVector(endByEdge[key]));
                builder.Append(",faces={");
                for (int index = 0; index < incidents.Count; index++)
                {
                    int faceIndex = incidents[index];
                    if (index > 0)
                    {
                        builder.Append('/');
                    }
                    PolygonFace face = faces[faceIndex];
                    builder.Append(BuildPlaneCutFaceIdentity(
                        face,
                        faceIndex));
                    builder.Append('#');
                    builder.Append(faceIndex);
                    AddPlaneCutFailureProvenanceEdge(
                        face.ProvenanceKind,
                        face.ProvenanceIndex,
                        linked);
                }
                builder.Append('}');
            }
            linkedEdgeIndices.Clear();
            linkedEdgeIndices.AddRange(linked);
            return builder.Length == 0 ? "none" : builder.ToString();
        }

        private static string BuildPlaneCutInvalidFaceFailureEvidence(
            List<PolygonFace> faces,
            float minimumStableFaceArea,
            List<int> linkedEdgeIndices)
        {
            if (faces == null)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            SortedSet<int> linked = new SortedSet<int>(linkedEdgeIndices);
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                string reason = ResolvePlaneCutInvalidFaceReason(
                    face,
                    minimumStableFaceArea);
                if (string.IsNullOrEmpty(reason))
                {
                    continue;
                }
                if (builder.Length > 0)
                {
                    builder.Append('|');
                }
                builder.Append("face=");
                builder.Append(BuildPlaneCutFaceIdentity(face, faceIndex));
                builder.Append('#');
                builder.Append(faceIndex);
                builder.Append(",reason=");
                builder.Append(reason);
                if (face != null)
                {
                    AddPlaneCutFailureProvenanceEdge(
                        face.ProvenanceKind,
                        face.ProvenanceIndex,
                        linked);
                }
            }
            linkedEdgeIndices.Clear();
            linkedEdgeIndices.AddRange(linked);
            return builder.Length == 0 ? "none" : builder.ToString();
        }

        private static string ResolvePlaneCutInvalidFaceReason(
            PolygonFace face,
            float minimumStableFaceArea)
        {
            if (face == null)
            {
                return "null-face";
            }
            if (face.Vertices == null || face.Vertices.Count < 3)
            {
                return "fewer-than-three-vertices";
            }
            if (!IsFinite(face.Normal) ||
                face.Normal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return "invalid-authored-normal";
            }
            for (int index = 0; index < face.Vertices.Count; index++)
            {
                if (!IsFinite(face.Vertices[index]))
                {
                    return "non-finite-vertex:" + index;
                }
            }
            float area = CalculatePolygonArea(face.Vertices);
            if (area <= minimumStableFaceArea)
            {
                return "area:" + area.ToString("G9") +
                    "<=" + minimumStableFaceArea.ToString("G9");
            }
            Vector3 measuredNormal =
                CalculatePolygonNormal(face.Vertices);
            if (!IsFinite(measuredNormal) ||
                measuredNormal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return "invalid-measured-normal";
            }
            if (Vector3.Dot(measuredNormal, face.Normal) <= 0f)
            {
                return "winding-opposes-authored-normal";
            }
            return string.Empty;
        }

        private static bool TryBuildPlaneCutGeneralizedFailureCluster(
            List<PlaneCutBevelCandidate> activeCandidates,
            PlaneCutRetryFailureDossier failure,
            List<PlaneCutConflictWidthReductionRecord> reductions,
            out List<int> clusterEdgeIndices,
            out string reasonEvidence)
        {
            clusterEdgeIndices = new List<int>();
            reasonEvidence = string.Empty;
            if (failure == null ||
                failure.LinkedEdgeIndices.Count == 0)
            {
                return false;
            }

            Dictionary<int, SortedSet<string>> reasons =
                new Dictionary<int, SortedSet<string>>();
            HashSet<int> clusterEdges = new HashSet<int>();
            HashSet<int> clusterVertices = new HashSet<int>();
            for (int edgeIndex = 0;
                 edgeIndex < failure.LinkedEdgeIndices.Count;
                 edgeIndex++)
            {
                AddPlaneCutTopologyClusterEdge(
                    activeCandidates,
                    failure.LinkedEdgeIndices[edgeIndex],
                    "retry-failure-" + failure.Cause,
                    clusterEdges,
                    clusterVertices,
                    reasons);
            }

            PlaneCutConflictWidthReductionRecord previousConflict =
                FindLatestPlaneCutConflictIntersectingEdges(
                    reductions,
                    clusterEdges);
            if (previousConflict != null)
            {
                for (int edgeIndex = 0;
                     edgeIndex <
                         previousConflict.ClusterEdgeIndices.Count;
                     edgeIndex++)
                {
                    AddPlaneCutTopologyClusterEdge(
                        activeCandidates,
                        previousConflict.ClusterEdgeIndices[edgeIndex],
                        "previous-conflict-pass-" +
                            previousConflict.PassIndex,
                        clusterEdges,
                        clusterVertices,
                        reasons);
                }
            }

            HashSet<int> interactionVertices =
                new HashSet<int>(clusterVertices);
            for (int candidateIndex = 0;
                 candidateIndex < activeCandidates.Count;
                 candidateIndex++)
            {
                PlaneCutBevelCandidate candidate =
                    activeCandidates[candidateIndex];
                if (!interactionVertices.Contains(candidate.VertexA) &&
                    !interactionVertices.Contains(candidate.VertexB))
                {
                    continue;
                }
                AddPlaneCutTopologyClusterEdge(
                    activeCandidates,
                    candidate.SourceEdgeIndex,
                    "incident-source-vertex-star",
                    clusterEdges,
                    clusterVertices,
                    reasons);
            }

            clusterEdgeIndices.AddRange(clusterEdges);
            clusterEdgeIndices.Sort();
            reasonEvidence = FormatPlaneCutClusterReasonEvidence(
                clusterEdgeIndices,
                reasons);
            return clusterEdgeIndices.Count > 0;
        }

        private static void ApplyPlaneCutRetryFailureToResult(
            PlaneCutRetryFailureDossier failure,
            float minimumStableEdgeLength,
            ref PlaneCutBevelAuditResult result)
        {
            if (failure == null)
            {
                return;
            }
            result.OpenEdgeCount = failure.OpenEdgeCount;
            result.NonManifoldEdgeCount =
                failure.NonManifoldEdgeCount;
            result.TJunctionCount = failure.TJunctionCount;
            result.InvalidFaceCount = failure.InvalidFaceCount;
            result.FaceQualityNonPlanarCount =
                failure.NonPlanarFaceCount;
            result.FaceQualityPlanarityTolerance =
                ResolvePlaneCutPlanarityTolerance(
                    minimumStableEdgeLength);
            result.FaceQualityNormalSpreadToleranceDegrees =
                PlaneCutMaximumNormalSpreadDegrees;
            result.OpenEdgeFailures.Clear();
            result.OpenEdgeFailures.AddRange(
                failure.OpenEdgeFailures);
            result.FaceQualityFailures.Clear();
            result.FaceQualityFailures.AddRange(
                failure.NonPlanarFaceFailures);
            result.TJunctionFailures.Clear();
            result.TJunctionFailures.AddRange(
                failure.TJunctionFailures);
            result.FirstOpenEdgeStage = failure.OpenEdgeCount > 0
                ? failure.Stage
                : string.Empty;
            result.FirstTJunctionStage = failure.TJunctionCount > 0
                ? failure.Stage
                : string.Empty;
            result.FirstNonPlanarStage =
                failure.NonPlanarFaceCount > 0
                    ? failure.Stage
                    : string.Empty;
        }

        private static bool TryBuildMinimalPlaneCutTopologyCluster(
            List<PlaneCutBevelCandidate> activeCandidates,
            List<PlaneCutTJunctionFailureRecord> tJunctionFailures,
            out List<int> clusterEdgeIndices,
            out string reasonEvidence,
            out PlaneCutTJunctionFailureRecord failure)
        {
            clusterEdgeIndices = new List<int>();
            reasonEvidence = string.Empty;
            failure = null;
            if (activeCandidates == null ||
                tJunctionFailures == null ||
                tJunctionFailures.Count == 0)
            {
                return false;
            }

            failure = tJunctionFailures[0];
            SortedSet<int> exactEdges = new SortedSet<int>();
            for (int failureIndex = 0;
                 failureIndex < tJunctionFailures.Count;
                 failureIndex++)
            {
                PlaneCutTJunctionFailureRecord exactFailure =
                    tJunctionFailures[failureIndex];
                for (int edgeIndex = 0;
                     edgeIndex < exactFailure.LinkedEdgeIndices.Count;
                     edgeIndex++)
                {
                    int sourceEdgeIndex =
                        exactFailure.LinkedEdgeIndices[edgeIndex];
                    if (TryFindPlaneCutCandidateBySourceEdge(
                            activeCandidates,
                            sourceEdgeIndex,
                            out PlaneCutBevelCandidate _))
                    {
                        exactEdges.Add(sourceEdgeIndex);
                    }
                }
            }

            clusterEdgeIndices.AddRange(exactEdges);
            if (clusterEdgeIndices.Count == 0)
            {
                return false;
            }

            StringBuilder reasons = new StringBuilder();
            for (int edgeIndex = 0;
                 edgeIndex < clusterEdgeIndices.Count;
                 edgeIndex++)
            {
                if (edgeIndex > 0)
                {
                    reasons.Append('/');
                }
                reasons.Append(clusterEdgeIndices[edgeIndex]);
                reasons.Append("=exact-tjunction-link");
            }
            reasonEvidence = reasons.ToString();
            return true;
        }

        private static bool TryResolvePlaneCutForeignBandRetreatTarget(
            List<PlaneCutBevelCandidate> activeCandidates,
            List<PlaneCutConflictWidthReductionRecord> reductions,
            List<int> topologyLinkedEdgeIndices,
            out int victimEdgeIndex,
            out int foreignEdgeIndex,
            out int sourcePassIndex,
            out string triggerEvidence)
        {
            victimEdgeIndex = -1;
            foreignEdgeIndex = -1;
            sourcePassIndex = -1;
            triggerEvidence = string.Empty;
            if (activeCandidates == null ||
                reductions == null ||
                topologyLinkedEdgeIndices == null ||
                topologyLinkedEdgeIndices.Count == 0)
            {
                return false;
            }

            HashSet<int> topologyLinkedEdges = new HashSet<int>(
                topologyLinkedEdgeIndices);
            for (int recordIndex = reductions.Count - 1;
                 recordIndex >= 0;
                 recordIndex--)
            {
                PlaneCutConflictWidthReductionRecord reduction =
                    reductions[recordIndex];
                if (!string.Equals(
                        reduction.TriggerCategory,
                        "band-integrity",
                        StringComparison.Ordinal) ||
                    !topologyLinkedEdges.Contains(
                        reduction.VictimEdgeIndex) ||
                    reduction.ForeignEdgeIndex < 0 ||
                    reduction.ForeignEdgeIndex ==
                        reduction.VictimEdgeIndex ||
                    !TryFindPlaneCutCandidateBySourceEdge(
                        activeCandidates,
                        reduction.ForeignEdgeIndex,
                        out PlaneCutBevelCandidate _))
                {
                    continue;
                }

                victimEdgeIndex = reduction.VictimEdgeIndex;
                foreignEdgeIndex = reduction.ForeignEdgeIndex;
                sourcePassIndex = reduction.PassIndex;
                triggerEvidence =
                    "bandPass:" + reduction.PassIndex +
                    ",victim:" + reduction.VictimEdgeIndex +
                    ",foreign:" + reduction.ForeignEdgeIndex +
                    ",foreignAxial:" +
                        reduction.ForeignAxialParameter.ToString("G9") +
                    ",foreignSpan:" +
                        reduction.ForeignSharedSpanRatio.ToString("G9") +
                    ",topologyLinked:{" +
                        FormatPlaneCutEdgeIndexEvidence(
                            topologyLinkedEdgeIndices) + "}";
                return true;
            }

            return false;
        }

        private static bool TryResolvePlaneCutOpposingBandRetreatTarget(
            List<PlaneCutTopologyScaleTrialRecord> trials,
            List<PlaneCutBevelCandidate> activeCandidates,
            int victimEdgeIndex,
            int initialForeignEdgeIndex,
            out int opposingForeignEdgeIndex,
            out string triggerEvidence)
        {
            opposingForeignEdgeIndex = -1;
            triggerEvidence = string.Empty;
            if (trials == null || activeCandidates == null)
            {
                return false;
            }

            for (int trialIndex = 0;
                 trialIndex < trials.Count;
                 trialIndex++)
            {
                PlaneCutTopologyScaleTrialRecord trial = trials[trialIndex];
                if (!string.Equals(
                        trial.SearchMode,
                        "direct-foreign-band-plane-retreat",
                        StringComparison.Ordinal) ||
                    trial.BandEvaluated != 1 ||
                    trial.BandValid != 0 ||
                    trial.TopologyValid != 1 ||
                    trial.BandVictimEdgeIndex != victimEdgeIndex ||
                    trial.BandForeignEdgeIndex < 0 ||
                    trial.BandForeignEdgeIndex == victimEdgeIndex ||
                    trial.BandForeignEdgeIndex == initialForeignEdgeIndex ||
                    !TryFindPlaneCutCandidateBySourceEdge(
                        activeCandidates,
                        trial.BandForeignEdgeIndex,
                        out PlaneCutBevelCandidate _))
                {
                    continue;
                }

                opposingForeignEdgeIndex = trial.BandForeignEdgeIndex;
                triggerEvidence =
                    "directTrial:" + trial.TrialIndex +
                    ",victim:" + victimEdgeIndex +
                    ",foreignA:" + initialForeignEdgeIndex +
                    ",foreignB:" + opposingForeignEdgeIndex +
                    ",foreignBAxial:" +
                        trial.BandForeignAxialParameter.ToString("G9") +
                    ",foreignBSpan:" +
                        trial.BandForeignSharedSpanRatio.ToString("G9");
                return true;
            }

            return false;
        }

        private static List<int> BuildPlaneCutProtectedEdgeSet(
            List<int> topologyLinkedEdgeIndices,
            List<int> retreatEdgeIndices)
        {
            HashSet<int> retreatEdges = retreatEdgeIndices == null
                ? new HashSet<int>()
                : new HashSet<int>(retreatEdgeIndices);
            SortedSet<int> protectedEdges = new SortedSet<int>();
            if (topologyLinkedEdgeIndices != null)
            {
                for (int edgeIndex = 0;
                     edgeIndex < topologyLinkedEdgeIndices.Count;
                     edgeIndex++)
                {
                    int sourceEdgeIndex =
                        topologyLinkedEdgeIndices[edgeIndex];
                    if (!retreatEdges.Contains(sourceEdgeIndex))
                    {
                        protectedEdges.Add(sourceEdgeIndex);
                    }
                }
            }
            return new List<int>(protectedEdges);
        }

        private static void SetPlaneCutDebugFocusEdges(
            ref PlaneCutBevelAuditResult result,
            List<int> topologyLinkedEdgeIndices,
            List<int> retreatEdgeIndices)
        {
            if (result.DebugFocusEdgeIndices == null)
            {
                result.DebugFocusEdgeIndices = new List<int>();
            }
            result.DebugFocusEdgeIndices.Clear();
            SortedSet<int> focusEdges = new SortedSet<int>();
            if (topologyLinkedEdgeIndices != null)
            {
                focusEdges.UnionWith(topologyLinkedEdgeIndices);
            }
            if (retreatEdgeIndices != null)
            {
                focusEdges.UnionWith(retreatEdgeIndices);
            }
            result.DebugFocusEdgeIndices.AddRange(focusEdges);
        }

        private static bool TrySearchPlaneCutRetreatScales(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            List<PlaneCutBevelCandidate> allCandidates,
            List<PlaneCutVertexJunctionCandidate> noJunctions,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            Dictionary<int, float> minimumScaleByEdge,
            PlaneCutSolverTransactionState topologyCleanBaseState,
            List<int> clusterEdgeIndices,
            int retreatVictimEdgeIndex,
            int retreatForeignEdgeIndex,
            int retreatSourcePassIndex,
            string searchMode,
            string trialStatePrefix,
            string committedTriggerCategory,
            string clusterReasonEvidence,
            string protectedEdgeEvidence,
            string failureSummary,
            ref PlaneCutBevelAuditResult result,
            out Dictionary<int, float> committedScaleByEdge,
            out List<PlaneCutBevelCandidate> committedCandidates,
            out List<PolygonFace> committedFaces,
            out string blocker)
        {
            committedScaleByEdge = null;
            committedCandidates = null;
            committedFaces = null;
            blocker = string.Empty;
            if (topologyCleanBaseState == null ||
                topologyCleanBaseState.ScaleByEdge == null ||
                clusterEdgeIndices == null ||
                clusterEdgeIndices.Count == 0)
            {
                blocker =
                    "the bounded retreat search has no immutable topology-clean base state";
                result.TopologyScaleSearchUnresolved = 1;
                return false;
            }

            float[] factors = { 0.95f, 0.90f, 0.85f, 0.80f, 0.75f };
            result.TopologyScaleSearchBasePass =
                topologyCleanBaseState.PassIndex;
            result.TopologyScaleSearchMode = searchMode;
            if (string.IsNullOrEmpty(
                    result.TopologyScaleSearchTriggerEvidence) ||
                string.Equals(
                    result.TopologyScaleSearchTriggerEvidence,
                    "none",
                    StringComparison.Ordinal))
            {
                result.TopologyScaleSearchTriggerEvidence =
                    "bandPass:" + retreatSourcePassIndex +
                    ",victim:" + retreatVictimEdgeIndex +
                    ",foreign:" + retreatForeignEdgeIndex;
            }
            result.TopologyScaleSearchClusterEvidence =
                FormatPlaneCutEdgeIndexEvidence(clusterEdgeIndices);
            result.TopologyScaleSearchCommittedFactor = -1f;
            result.TopologyScaleSearchHighestValidFactor = -1f;
            result.TopologyScaleSearchCollateralChangedEvidence = "none";
            result.TopologyScaleSearchFailedStateScalesReused = 0;
            result.TopologyScaleSearchUnresolved = 1;

            Bounds sourceBounds = CalculateFaceBounds(sourceFaces);
            double sourceVolume =
                CalculatePlaneCutPolyhedronVolume(sourceFaces);
            for (int trialIndex = 0;
                 trialIndex < factors.Length;
                 trialIndex++)
            {
                float factor = factors[trialIndex];
                bool fullyValid = TryEvaluatePlaneCutRetreatTrial(
                    sourceFaces,
                    sourceBounds,
                    sourceVolume,
                    context,
                    allCandidates,
                    noJunctions,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    minimumScaleByEdge,
                    topologyCleanBaseState,
                    clusterEdgeIndices,
                    trialIndex + 1,
                    factor,
                    searchMode,
                    protectedEdgeEvidence,
                    out PlaneCutTopologyScaleTrialRecord trial,
                    out Dictionary<int, float> trialScaleByEdge,
                    out List<PlaneCutBevelCandidate> trialCandidates,
                    out List<PolygonFace> trialFaces,
                    out PlaneCutBevelAuditResult trialAudit,
                    out string trialBlocker);
                result.TopologyScaleTrials.Add(trial);
                result.TopologyScaleSearchTrialCount++;
                ApplyPlaneCutActiveSearchFailure(
                    trial,
                    ref result);
                if (trial.BandEvaluated == 1 &&
                    trial.BandValid == 0)
                {
                    result.TopologyScaleSearchBandFailureCount++;
                }
                if (trial.TopologyEvaluated == 1 &&
                    trial.TopologyValid == 0)
                {
                    result.TopologyScaleSearchTopologyFailureCount++;
                }
                if (trial.FaceQualityEvaluated == 1 &&
                    trial.FaceQualityValid == 0)
                {
                    result.TopologyScaleSearchFaceQualityFailureCount++;
                }
                if (!string.Equals(
                        trial.CollateralChangedEvidence,
                        "none",
                        StringComparison.Ordinal))
                {
                    result.TopologyScaleSearchCollateralFailureCount++;
                    result.TopologyScaleSearchCollateralChangedEvidence =
                        trial.CollateralChangedEvidence;
                }

                PlaneCutSolverTransactionState attemptedState =
                    CapturePlaneCutSolverTransactionState(
                        trialStatePrefix + (trialIndex + 1),
                        topologyCleanBaseState.PassIndex,
                        trialCandidates,
                        trialFaces,
                        trialScaleByEdge,
                        trial.BandValid == 1,
                        trial.TopologyValid == 1 &&
                            trial.FaceQualityValid == 1,
                        trialAudit.StageFinalCertification.Stage ==
                                "FinalCertification"
                            ? trialAudit.StageFinalCertification
                            : trialAudit.StageSeamRepaired);
                result.LatestAttemptedState = attemptedState;

                if (fullyValid)
                {
                    result.ActiveSearchFailureStage = "none";
                    result.ActiveSearchFailureCause = "none";
                    result.ActiveSearchFailureEvidence = "none";
                    result.TopologyScaleSearchCommittedFactor = factor;
                    result.TopologyScaleSearchHighestValidFactor = factor;
                    result.TopologyScaleSearchUnresolved = 0;
                    result.LatestBandCleanState =
                        CapturePlaneCutSolverTransactionState(
                            "band-clean-" + searchMode,
                            topologyCleanBaseState.PassIndex,
                            trialCandidates,
                            trialFaces,
                            trialScaleByEdge,
                            true,
                            true,
                            trialAudit.StageFinalCertification);
                    result.LatestTopologyCleanState =
                        CapturePlaneCutSolverTransactionState(
                            "topology-clean-" + searchMode,
                            topologyCleanBaseState.PassIndex,
                            trialCandidates,
                            trialFaces,
                            trialScaleByEdge,
                            true,
                            true,
                            trialAudit.StageFinalCertification);
                    result.LatestCertifiedState =
                        CapturePlaneCutSolverTransactionState(
                            "solver-clean",
                            topologyCleanBaseState.PassIndex,
                            trialCandidates,
                            trialFaces,
                            trialScaleByEdge,
                            true,
                            true,
                            trialAudit.StageFinalCertification);
                    ApplyPlaneCutTopologyTrialAuditToResult(
                        trialAudit,
                        ref result);
                    PlaneCutConflictWidthReductionRecord committedReduction =
                        new PlaneCutConflictWidthReductionRecord
                        {
                            PassIndex = topologyCleanBaseState.PassIndex,
                            VictimEdgeIndex =
                                retreatVictimEdgeIndex,
                            ForeignEdgeIndex =
                                retreatForeignEdgeIndex,
                            VertexIndex = -1,
                            TriggerCategory =
                                committedTriggerCategory,
                            BandValid = 1,
                            TopologyValid = 1,
                            PreviousMinimumScale =
                                ResolveMinimumPlaneCutScale(
                                    topologyCleanBaseState.ScaleByEdge,
                                    clusterEdgeIndices),
                            RequestedScale = factor,
                            AppliedMinimumScale =
                                ResolveMinimumPlaneCutScale(
                                    trialScaleByEdge,
                                    clusterEdgeIndices),
                            ClusterFloorScale =
                                ResolveMaximumPlaneCutScale(
                                    minimumScaleByEdge,
                                    clusterEdgeIndices),
                            ClusterReasonEvidence =
                                clusterReasonEvidence,
                            PreviousScaleEvidence =
                                trial.BaseScaleEvidence,
                            RollbackScaleEvidence =
                                trial.BaseScaleEvidence,
                            AppliedScaleEvidence =
                                trial.EffectiveScaleEvidence,
                            Result =
                                "committed-" + searchMode + "-factor-" +
                                    factor.ToString("G9")
                        };
                    committedReduction.ClusterEdgeIndices.AddRange(
                        clusterEdgeIndices);
                    result.EdgeConflictWidthReductions.Add(
                        committedReduction);
                    result.EdgeConflictWidthReductionCount++;
                    result.EdgeConflictClusterCount =
                        result.EdgeConflictWidthReductions.Count;
                    committedScaleByEdge = trialScaleByEdge;
                    committedCandidates = trialCandidates;
                    committedFaces = trialFaces;
                    blocker = string.Empty;
                    return true;
                }

                blocker = string.IsNullOrEmpty(trialBlocker)
                    ? "the bounded retreat trial failed certification"
                    : trialBlocker;
            }

            result.LatestTopologyCleanState = topologyCleanBaseState;
            result.LatestCertifiedState = null;
            result.TopologyScaleSearchUnresolved = 1;
            blocker = failureSummary;
            return false;
        }

        private static bool TryEvaluatePlaneCutRetreatTrial(
            List<PolygonFace> sourceFaces,
            Bounds sourceBounds,
            double sourceVolume,
            ChamferTopologyContext context,
            List<PlaneCutBevelCandidate> allCandidates,
            List<PlaneCutVertexJunctionCandidate> noJunctions,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            Dictionary<int, float> minimumScaleByEdge,
            PlaneCutSolverTransactionState topologyCleanBaseState,
            List<int> clusterEdgeIndices,
            int trialIndex,
            float factor,
            string searchMode,
            string protectedEdgeEvidence,
            out PlaneCutTopologyScaleTrialRecord trial,
            out Dictionary<int, float> trialScaleByEdge,
            out List<PlaneCutBevelCandidate> trialCandidates,
            out List<PolygonFace> trialFaces,
            out PlaneCutBevelAuditResult trialAudit,
            out string blocker)
        {
            trial = new PlaneCutTopologyScaleTrialRecord
            {
                TrialIndex = trialIndex,
                BasePassIndex = topologyCleanBaseState.PassIndex,
                Factor = factor,
                SearchMode = searchMode,
                ProtectedEdgeEvidence = protectedEdgeEvidence,
                Result = "discarded"
            };
            trial.ClusterEdgeIndices.AddRange(clusterEdgeIndices);
            trialScaleByEdge = ClonePlaneCutScaleMap(
                topologyCleanBaseState.ScaleByEdge);
            trialCandidates = new List<PlaneCutBevelCandidate>();
            trialFaces = new List<PolygonFace>();
            trialAudit = CreatePlaneCutTopologyTrialAuditScratch();
            blocker = string.Empty;

            Dictionary<int, float> requestedScaleByEdge =
                ClonePlaneCutScaleMap(trialScaleByEdge);
            SortedSet<int> floorHitEdges = new SortedSet<int>();
            for (int clusterIndex = 0;
                 clusterIndex < clusterEdgeIndices.Count;
                 clusterIndex++)
            {
                int edgeIndex = clusterEdgeIndices[clusterIndex];
                if (!trialScaleByEdge.TryGetValue(
                        edgeIndex,
                        out float baseScale) ||
                    !minimumScaleByEdge.TryGetValue(
                        edgeIndex,
                        out float minimumScale))
                {
                    trial.FailureCause =
                        "retreat edge is absent from the rollback scale map";
                    blocker = trial.FailureCause;
                    return false;
                }

                float requestedScale = baseScale * factor;
                float effectiveScale = Mathf.Max(
                    minimumScale,
                    requestedScale);
                requestedScaleByEdge[edgeIndex] = requestedScale;
                trialScaleByEdge[edgeIndex] = effectiveScale;
                if (effectiveScale > requestedScale + 0.000001f)
                {
                    floorHitEdges.Add(edgeIndex);
                }
            }

            trial.BaseScaleEvidence = FormatPlaneCutScaleEvidence(
                topologyCleanBaseState.ScaleByEdge,
                clusterEdgeIndices);
            trial.RequestedScaleEvidence = FormatPlaneCutScaleEvidence(
                requestedScaleByEdge,
                clusterEdgeIndices);
            trial.EffectiveScaleEvidence = FormatPlaneCutScaleEvidence(
                trialScaleByEdge,
                clusterEdgeIndices);
            trial.FloorHitEvidence =
                FormatPlaneCutEdgeIndexEvidence(
                    new List<int>(floorHitEdges));
            trial.CollateralChangedEvidence =
                FormatPlaneCutScaleDifferencesOutsideCluster(
                    topologyCleanBaseState.ScaleByEdge,
                    trialScaleByEdge,
                    clusterEdgeIndices);
            if (!string.Equals(
                    trial.CollateralChangedEvidence,
                    "none",
                    StringComparison.Ordinal))
            {
                trial.FailureCause =
                    "a bounded retreat trial changed an edge outside the exact retreat set";
                blocker = trial.FailureCause;
                return false;
            }

            trialCandidates = BuildScaledPlaneCutCandidates(
                allCandidates,
                context,
                trialScaleByEdge,
                minimumStableEdgeLength);
            trial.AttemptedBuiltCount = trialCandidates.Count;
            ResetPlaneCutStageTelemetry(ref trialAudit);
            PlaneCutNumericalRepairTelemetry numericalRepairs =
                new PlaneCutNumericalRepairTelemetry();
            trialAudit.NumericalRepairs = numericalRepairs;
            if (!TryBuildPlaneCutSystemFaces(
                    sourceFaces,
                    trialCandidates,
                    noJunctions,
                    out List<PolygonFace> rawFaces,
                    out int edgeCapsBuilt,
                    out string buildBlocker,
                    numericalRepairs))
            {
                trial.FailureStage = "PlaneConstruction";
                trial.FailureCause = string.IsNullOrEmpty(buildBlocker)
                    ? "trial plane construction failed"
                    : buildBlocker;
                blocker = trial.FailureCause;
                return false;
            }

            CapturePlaneCutStageSnapshot(
                rawFaces,
                "AfterPlaneConstruction",
                minimumStableEdgeLength,
                minimumStableFaceArea,
                trialCandidates,
                ref trialAudit,
                out trialAudit.StagePlaneConstruction);
            if (!TryPreparePlaneCutPreviewFaces(
                    rawFaces,
                    trialCandidates,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    ref trialAudit,
                    out trialFaces,
                    out int conformalSplitCount,
                    out int seamPairCount,
                    out int seamTouchedFaceCount,
                    out string preparationBlocker,
                    numericalRepairs))
            {
                trial.FailureStage =
                    ResolvePlaneCutFirstRetryFailureStage(trialAudit);
                trial.FailureCause = string.IsNullOrEmpty(
                        preparationBlocker)
                    ? "trial preview preparation failed"
                    : preparationBlocker;
                blocker = trial.FailureCause;
                CopyPlaneCutTopologyTrialFailures(
                    trialAudit,
                    trial);
                return false;
            }

            trialAudit.PlanesBuilt = trialCandidates.Count;
            trialAudit.AttemptedPlanesBuilt = trialCandidates.Count;
            trialAudit.CapsBuilt = edgeCapsBuilt;
            trialAudit.ConformalSplitCount = conformalSplitCount;
            trialAudit.SeamPairCount = seamPairCount;
            trialAudit.FaceQualitySeamTouchedFaceCount =
                seamTouchedFaceCount;

            PlaneCutBevelAuditResult bandAudit =
                CreatePlaneCutBandAuditScratch();
            AuditPlaneCutBandIntegrity(
                trialFaces,
                context,
                trialCandidates,
                noJunctions,
                minimumStableEdgeLength,
                ref bandAudit,
                out int _,
                out string bandBlocker);
            CopyPlaneCutBandAudit(bandAudit, ref trialAudit);
            trial.BandEvaluated = 1;
            trial.BandValid = IsPlaneCutBandAuditClean(bandAudit)
                ? 1
                : 0;
            trial.BandVictimEdgeIndex =
                bandAudit.EdgeConflictVictimEdgeIndex;
            trial.BandForeignEdgeIndex =
                bandAudit.EdgeConflictForeignEdgeIndex;
            trial.BandForeignAxialParameter =
                bandAudit.EdgeConflictForeignAxialParameter;
            trial.BandForeignSharedSpanRatio =
                bandAudit.EdgeConflictForeignSharedSpanRatio;

            for (int candidateIndex = 0;
                 candidateIndex < trialCandidates.Count;
                 candidateIndex++)
            {
                PlaneCutBevelCandidate candidate =
                    trialCandidates[candidateIndex];
                int capCount = CountMatchingPlaneCutCaps(
                    trialFaces,
                    candidate);
                if (capCount == 1)
                {
                    continue;
                }
                if (capCount == 0 &&
                    IsPlaneCutCandidateRedundant(
                        trialFaces,
                        candidate))
                {
                    trialAudit.CapsRedundant++;
                    continue;
                }
                trialAudit.CapsMissing++;
            }

            AuditPlaneCutFaceQuality(
                trialFaces,
                noJunctions,
                minimumStableEdgeLength,
                ref trialAudit);
            EdgeWearTopologyStats topology = AuditEdgeWearTopology(
                trialFaces,
                minimumStableEdgeLength);
            trialAudit.OpenEdgeCount = topology.OpenEdgeCount;
            trialAudit.NonManifoldEdgeCount =
                topology.NonManifoldEdgeCount;
            trialAudit.TJunctionCount = topology.TJunctionCount;
            trialAudit.InvalidFaceCount += CountInvalidPlaneCutFaces(
                trialFaces,
                minimumStableFaceArea);
            CapturePlaneCutStageSnapshot(
                trialFaces,
                "FinalCertification",
                minimumStableEdgeLength,
                minimumStableFaceArea,
                trialCandidates,
                ref trialAudit,
                out trialAudit.StageFinalCertification);
            AuditPlaneCutOpenEdgeFailures(
                trialFaces,
                context,
                trialCandidates,
                minimumStableEdgeLength,
                ref trialAudit);

            trial.OpenEdgeCount = trialAudit.OpenEdgeCount;
            trial.NonManifoldEdgeCount =
                trialAudit.NonManifoldEdgeCount;
            trial.TJunctionCount = trialAudit.TJunctionCount;
            trial.InvalidFaceCount = trialAudit.InvalidFaceCount;
            trial.NonPlanarFaceCount =
                trialAudit.FaceQualityNonPlanarCount;
            trial.MaximumPlaneDeviation =
                trialAudit.FaceQualityMaxPlaneDeviation;
            trial.MaximumNormalSpreadDegrees =
                trialAudit.FaceQualityMaxNormalSpreadDegrees;
            trial.TopologyEvaluated = 1;
            trial.FaceQualityEvaluated = 1;
            trial.TopologyValid =
                trial.OpenEdgeCount == 0 &&
                trial.NonManifoldEdgeCount == 0 &&
                trial.TJunctionCount == 0 &&
                trial.InvalidFaceCount == 0
                    ? 1
                    : 0;
            trial.FaceQualityValid =
                trial.NonPlanarFaceCount == 0 ? 1 : 0;

            double resultVolume =
                CalculatePlaneCutPolyhedronVolume(trialFaces);
            bool volumeValid = sourceVolume > 0.000000001 &&
                resultVolume > sourceVolume * 0.75 &&
                resultVolume <= sourceVolume * 1.0001;
            bool boundsValid = ArePlaneCutBoundsContained(
                sourceBounds,
                CalculateFaceBounds(trialFaces),
                Mathf.Max(
                    PlaneEpsilon * 1.25f,
                    Mathf.Max(
                        PointMergeDistance * 8f,
                        minimumStableEdgeLength * 0.02f)));
            bool polygonValid =
                trial.BandValid == 1 &&
                trial.TopologyValid == 1 &&
                trial.FaceQualityValid == 1 &&
                trialAudit.BandRetainedEdgeCount ==
                    trialCandidates.Count &&
                trialAudit.BandSingleFaceCount ==
                    trialCandidates.Count &&
                trialAudit.CapsMissing == 0 &&
                volumeValid &&
                boundsValid &&
                trialFaces.Count >= 4;

            bool surfaceTriangulationValid = false;
            TriangleSoup auditedSoup = null;
            if (polygonValid)
            {
                BoundedSingleEdgeAuditResult surfaceAudit =
                    new BoundedSingleEdgeAuditResult
                    {
                        TriangulationFailureFace = -1,
                        TriangulationFailureProvenanceIndex = -1,
                        BevelRegionFailureFace = -1,
                        BevelRegionFailureProvenanceIndex = -1
                    };
                surfaceTriangulationValid =
                    TryTriangulateBoundedPreviewFaces(
                        trialFaces,
                        minimumStableFaceArea,
                        ref surfaceAudit,
                        out auditedSoup,
                        out string surfaceBlocker);
                trialAudit.BevelRegionFaceCount =
                    surfaceAudit.BevelRegionFaceCount;
                trialAudit.BevelRegionBoundaryVertexCount =
                    surfaceAudit.BevelRegionBoundaryVertexCount;
                trialAudit.BevelRegionTriangleCount =
                    surfaceAudit.BevelRegionTriangleCount;
                trialAudit.BevelRegionAuthoredNormalTriangleCount =
                    surfaceAudit.BevelRegionAuthoredNormalTriangleCount;
                trialAudit.BevelRegionAuthoredSurfaceGroupTriangleCount =
                    surfaceAudit.BevelRegionAuthoredSurfaceGroupTriangleCount;
                trialAudit.BevelRegionInternalFanVertexCount =
                    surfaceAudit.BevelRegionInternalFanVertexCount;
                trialAudit.BevelRegionMaximumPlaneResidual =
                    surfaceAudit.BevelRegionMaximumPlaneResidual;
                trialAudit.BevelRegionMaximumNormalDeviationDegrees =
                    surfaceAudit.BevelRegionMaximumNormalDeviationDegrees;
                trialAudit.BevelRegionRenderValid =
                    surfaceAudit.BevelRegionRenderValid;
                if (!surfaceTriangulationValid &&
                    string.IsNullOrEmpty(blocker))
                {
                    blocker = surfaceBlocker;
                }
            }
            if (auditedSoup != null)
            {
                AuditPlaneCutPreviewTriangleSoup(
                    auditedSoup,
                    trialFaces,
                    minimumStableEdgeLength,
                    ref trialAudit);
            }

            trial.SurfaceValid =
                surfaceTriangulationValid &&
                trialAudit.BevelRegionFaceCount ==
                    trialCandidates.Count &&
                trialAudit.BevelRegionRenderValid == 1
                    ? 1
                    : 0;
            trial.MeshValid =
                trialAudit.PreviewGeometryValid == 1 ? 1 : 0;
            trial.FullyValid =
                polygonValid &&
                trial.SurfaceValid == 1 &&
                trial.MeshValid == 1
                    ? 1
                    : 0;
            CopyPlaneCutTopologyTrialFailures(trialAudit, trial);
            if (trial.FullyValid == 1)
            {
                trial.Result = "committed";
                trial.FailureStage = "none";
                trial.FailureCause = "none";
                return true;
            }

            trial.FailureStage = trial.BandValid == 0
                ? "BandIntegrity"
                : ResolvePlaneCutFirstRetryFailureStage(trialAudit);
            if (trial.BandValid == 0)
            {
                trial.FailureCause = string.IsNullOrEmpty(bandBlocker)
                    ? "band-integrity"
                    : bandBlocker;
            }
            else if (trial.TopologyValid == 0)
            {
                trial.FailureCause = "topology";
            }
            else if (trial.FaceQualityValid == 0)
            {
                trial.FailureCause = "face-quality";
            }
            else if (!volumeValid)
            {
                trial.FailureCause = "retained-volume";
            }
            else if (!boundsValid)
            {
                trial.FailureCause = "source-bounds";
            }
            else if (trial.SurfaceValid == 0)
            {
                trial.FailureCause = "one-surface-render";
            }
            else
            {
                trial.FailureCause = "preview-mesh";
            }
            blocker = trial.FailureCause;
            return false;
        }

        private static void ApplyPlaneCutActiveSearchFailure(
            PlaneCutTopologyScaleTrialRecord trial,
            ref PlaneCutBevelAuditResult result)
        {
            if (trial == null || trial.FullyValid == 1)
            {
                return;
            }
            result.ActiveSearchFailureStage =
                string.IsNullOrEmpty(trial.FailureStage)
                    ? "unknown"
                    : trial.FailureStage;
            result.ActiveSearchFailureCause =
                string.IsNullOrEmpty(trial.FailureCause)
                    ? "unknown"
                    : trial.FailureCause;
            result.ActiveSearchFailureEvidence =
                "mode:" +
                    (string.IsNullOrEmpty(trial.SearchMode)
                        ? "unknown"
                        : trial.SearchMode) +
                ",trial:" + trial.TrialIndex +
                ",factor:" + trial.Factor.ToString("G9") +
                ",victim:" + trial.BandVictimEdgeIndex +
                ",foreign:" + trial.BandForeignEdgeIndex +
                ",foreignAxial:" +
                    trial.BandForeignAxialParameter.ToString("G9") +
                ",foreignSpan:" +
                    trial.BandForeignSharedSpanRatio.ToString("G9");
        }

        private static PlaneCutBevelAuditResult
            CreatePlaneCutTopologyTrialAuditScratch()
        {
            return new PlaneCutBevelAuditResult
            {
                EdgeConflictVictimEdgeIndex = -1,
                EdgeConflictForeignEdgeIndex = -1,
                EdgeConflictVertexIndex = -1,
                EdgeConflictDeferredEdgeIndex = -1,
                EdgeConflictMinimumWidthScale = 1f,
                EdgeConflictWidthReductions =
                    new List<PlaneCutConflictWidthReductionRecord>(),
                FaceQualityFailures =
                    new List<PlaneCutFaceQualityFailureRecord>(),
                OpenEdgeFailures =
                    new List<PlaneCutOpenEdgeFailureRecord>(),
                TJunctionFailures =
                    new List<PlaneCutTJunctionFailureRecord>(),
                LocalityDeferrals =
                    new List<PlaneCutLocalityDeferralRecord>(),
                RetryFailureDossiers =
                    new List<PlaneCutRetryFailureDossier>(),
                TopologyScaleTrials =
                    new List<PlaneCutTopologyScaleTrialRecord>(),
                TopologyScaleSearchBasePass = -1,
                TopologyScaleSearchCommittedFactor = -1f,
                TopologyScaleSearchHighestValidFactor = -1f,
                TopologyScaleSearchMode = "none",
                TopologyScaleSearchTriggerEvidence = "none",
                TopologyScaleSearchTopologyLinkedEvidence = "none",
                TopologyScaleSearchClusterEvidence = "none",
                TopologyScaleSearchCollateralChangedEvidence = "none",
                FaceFirstNonPlanarStageByIdentity =
                    new Dictionary<string, string>(),
                BoundaryConformityTouchedFaces =
                    new HashSet<string>(),
                SeamRepairTouchedFaces =
                    new HashSet<string>(),
                SeamRepairMaximumMovementByIdentity =
                    new Dictionary<string, float>(),
                NumericalRepairs =
                    new PlaneCutNumericalRepairTelemetry()
            };
        }

        private static string FormatPlaneCutScaleDifferencesOutsideCluster(
            Dictionary<int, float> baseline,
            Dictionary<int, float> trial,
            List<int> clusterEdgeIndices)
        {
            if (baseline == null || trial == null)
            {
                return "missing-scale-map";
            }
            HashSet<int> cluster = clusterEdgeIndices == null
                ? new HashSet<int>()
                : new HashSet<int>(clusterEdgeIndices);
            SortedSet<int> changed = new SortedSet<int>();
            foreach (KeyValuePair<int, float> pair in baseline)
            {
                if (cluster.Contains(pair.Key))
                {
                    continue;
                }
                if (!trial.TryGetValue(pair.Key, out float trialScale) ||
                    Mathf.Abs(trialScale - pair.Value) > 0.000001f)
                {
                    changed.Add(pair.Key);
                }
            }
            foreach (KeyValuePair<int, float> pair in trial)
            {
                if (!cluster.Contains(pair.Key) &&
                    !baseline.ContainsKey(pair.Key))
                {
                    changed.Add(pair.Key);
                }
            }
            return FormatPlaneCutEdgeIndexEvidence(
                new List<int>(changed));
        }

        private static void CopyPlaneCutTopologyTrialFailures(
            PlaneCutBevelAuditResult audit,
            PlaneCutTopologyScaleTrialRecord trial)
        {
            if (audit.OpenEdgeFailures != null)
            {
                trial.OpenEdgeFailures.AddRange(
                    audit.OpenEdgeFailures);
            }
            if (audit.FaceQualityFailures != null)
            {
                trial.NonPlanarFaceFailures.AddRange(
                    audit.FaceQualityFailures);
            }
            if (audit.TJunctionFailures != null)
            {
                trial.TJunctionFailures.AddRange(
                    audit.TJunctionFailures);
            }
        }

        private static void ApplyPlaneCutTopologyTrialAuditToResult(
            PlaneCutBevelAuditResult trial,
            ref PlaneCutBevelAuditResult result)
        {
            result.StagePlaneConstruction =
                trial.StagePlaneConstruction;
            result.StageSanitized = trial.StageSanitized;
            result.StageWelded = trial.StageWelded;
            result.StageConformed = trial.StageConformed;
            result.StageSeamRepaired = trial.StageSeamRepaired;
            result.StageFinalCertification =
                trial.StageFinalCertification;
            result.FirstOpenEdgeStage = trial.FirstOpenEdgeStage;
            result.FirstTJunctionStage = trial.FirstTJunctionStage;
            result.FirstNonPlanarStage = trial.FirstNonPlanarStage;
            result.OpenEdgeCount = trial.OpenEdgeCount;
            result.NonManifoldEdgeCount = trial.NonManifoldEdgeCount;
            result.TJunctionCount = trial.TJunctionCount;
            result.InvalidFaceCount = trial.InvalidFaceCount;
            result.FaceQualityFaceCount = trial.FaceQualityFaceCount;
            result.FaceQualityNonPlanarCount =
                trial.FaceQualityNonPlanarCount;
            result.FaceQualityMaxPlaneDeviation =
                trial.FaceQualityMaxPlaneDeviation;
            result.FaceQualityMaxNormalSpreadDegrees =
                trial.FaceQualityMaxNormalSpreadDegrees;
            result.FaceQualityPlanarityTolerance =
                trial.FaceQualityPlanarityTolerance;
            result.FaceQualityNormalSpreadToleranceDegrees =
                trial.FaceQualityNormalSpreadToleranceDegrees;
            result.CapsBuilt = trial.CapsBuilt;
            result.CapsMissing = trial.CapsMissing;
            result.CapsRedundant = trial.CapsRedundant;
            result.ConformalSplitCount = trial.ConformalSplitCount;
            result.SeamPairCount = trial.SeamPairCount;
            result.FaceQualitySeamTouchedFaceCount =
                trial.FaceQualitySeamTouchedFaceCount;
            CopyPlaneCutBandAudit(trial, ref result);
            result.BevelRegionFaceCount = trial.BevelRegionFaceCount;
            result.BevelRegionBoundaryVertexCount =
                trial.BevelRegionBoundaryVertexCount;
            result.BevelRegionTriangleCount =
                trial.BevelRegionTriangleCount;
            result.BevelRegionAuthoredNormalTriangleCount =
                trial.BevelRegionAuthoredNormalTriangleCount;
            result.BevelRegionAuthoredSurfaceGroupTriangleCount =
                trial.BevelRegionAuthoredSurfaceGroupTriangleCount;
            result.BevelRegionInternalFanVertexCount =
                trial.BevelRegionInternalFanVertexCount;
            result.BevelRegionMaximumPlaneResidual =
                trial.BevelRegionMaximumPlaneResidual;
            result.BevelRegionMaximumNormalDeviationDegrees =
                trial.BevelRegionMaximumNormalDeviationDegrees;
            result.BevelRegionRenderValid =
                trial.BevelRegionRenderValid;
            result.PreviewTriangleCount = trial.PreviewTriangleCount;
            result.PreviewDegenerateTriangleCount =
                trial.PreviewDegenerateTriangleCount;
            result.PreviewOpenEdgeCount = trial.PreviewOpenEdgeCount;
            result.PreviewNonManifoldEdgeCount =
                trial.PreviewNonManifoldEdgeCount;
            result.PreviewWindingFailureCount =
                trial.PreviewWindingFailureCount;
            result.PreviewBoundsFailureCount =
                trial.PreviewBoundsFailureCount;
            result.PreviewVolumeFailureCount =
                trial.PreviewVolumeFailureCount;
            result.PreviewGeometryValid = trial.PreviewGeometryValid;
            result.NumericalRepairs = trial.NumericalRepairs;
            result.FaceQualityFailures.Clear();
            result.FaceQualityFailures.AddRange(
                trial.FaceQualityFailures);
            result.OpenEdgeFailures.Clear();
            result.OpenEdgeFailures.AddRange(
                trial.OpenEdgeFailures);
            result.TJunctionFailures.Clear();
            result.TJunctionFailures.AddRange(
                trial.TJunctionFailures);
        }

        private static bool TryBuildPlaneCutTopologyConflictCluster(
            List<PlaneCutBevelCandidate> activeCandidates,
            List<PlaneCutTJunctionFailureRecord> tJunctionFailures,
            List<PlaneCutConflictWidthReductionRecord> reductions,
            out List<int> clusterEdgeIndices,
            out string reasonEvidence,
            out PlaneCutTJunctionFailureRecord failure)
        {
            clusterEdgeIndices = new List<int>();
            reasonEvidence = string.Empty;
            failure = null;
            if (tJunctionFailures == null ||
                tJunctionFailures.Count == 0)
            {
                return false;
            }

            failure = tJunctionFailures[0];
            Dictionary<int, SortedSet<string>> reasons =
                new Dictionary<int, SortedSet<string>>();
            HashSet<int> clusterEdges = new HashSet<int>();
            HashSet<int> clusterVertices = new HashSet<int>();
            for (int edgeIndex = 0;
                 edgeIndex < failure.LinkedEdgeIndices.Count;
                 edgeIndex++)
            {
                int sourceEdgeIndex =
                    failure.LinkedEdgeIndices[edgeIndex];
                AddPlaneCutTopologyClusterEdge(
                    activeCandidates,
                    sourceEdgeIndex,
                    "tjunction-linked",
                    clusterEdges,
                    clusterVertices,
                    reasons);
            }

            PlaneCutConflictWidthReductionRecord previousConflict =
                FindLatestPlaneCutConflictIntersectingEdges(
                    reductions,
                    clusterEdges);
            if (previousConflict != null)
            {
                for (int edgeIndex = 0;
                     edgeIndex <
                         previousConflict.ClusterEdgeIndices.Count;
                     edgeIndex++)
                {
                    AddPlaneCutTopologyClusterEdge(
                        activeCandidates,
                        previousConflict.ClusterEdgeIndices[edgeIndex],
                        "previous-conflict-pass-" +
                            previousConflict.PassIndex,
                        clusterEdges,
                        clusterVertices,
                        reasons);
                }
            }

            if (clusterEdges.Count == 0)
            {
                return false;
            }

            HashSet<int> interactionVertices =
                new HashSet<int>(clusterVertices);
            for (int candidateIndex = 0;
                 candidateIndex < activeCandidates.Count;
                 candidateIndex++)
            {
                PlaneCutBevelCandidate candidate =
                    activeCandidates[candidateIndex];
                if (!interactionVertices.Contains(candidate.VertexA) &&
                    !interactionVertices.Contains(candidate.VertexB))
                {
                    continue;
                }
                AddPlaneCutTopologyClusterEdge(
                    activeCandidates,
                    candidate.SourceEdgeIndex,
                    "incident-source-vertex-star",
                    clusterEdges,
                    clusterVertices,
                    reasons);
            }

            clusterEdgeIndices.AddRange(clusterEdges);
            clusterEdgeIndices.Sort();
            reasonEvidence = FormatPlaneCutClusterReasonEvidence(
                clusterEdgeIndices,
                reasons);
            return clusterEdgeIndices.Count > 0;
        }

        private static void AddPlaneCutTopologyClusterEdge(
            List<PlaneCutBevelCandidate> activeCandidates,
            int sourceEdgeIndex,
            string reason,
            HashSet<int> clusterEdges,
            HashSet<int> clusterVertices,
            Dictionary<int, SortedSet<string>> reasons)
        {
            if (!TryFindPlaneCutCandidateBySourceEdge(
                    activeCandidates,
                    sourceEdgeIndex,
                    out PlaneCutBevelCandidate candidate))
            {
                return;
            }
            clusterEdges.Add(candidate.SourceEdgeIndex);
            clusterVertices.Add(candidate.VertexA);
            clusterVertices.Add(candidate.VertexB);
            if (!reasons.TryGetValue(
                    candidate.SourceEdgeIndex,
                    out SortedSet<string> edgeReasons))
            {
                edgeReasons = new SortedSet<string>();
                reasons.Add(candidate.SourceEdgeIndex, edgeReasons);
            }
            edgeReasons.Add(reason);
        }

        private static PlaneCutConflictWidthReductionRecord
            FindLatestPlaneCutConflictIntersectingEdges(
                List<PlaneCutConflictWidthReductionRecord> reductions,
                HashSet<int> sourceEdges)
        {
            if (reductions == null || sourceEdges == null ||
                sourceEdges.Count == 0)
            {
                return null;
            }
            for (int recordIndex = reductions.Count - 1;
                 recordIndex >= 0;
                 recordIndex--)
            {
                PlaneCutConflictWidthReductionRecord reduction =
                    reductions[recordIndex];
                for (int edgeIndex = 0;
                     edgeIndex < reduction.ClusterEdgeIndices.Count;
                     edgeIndex++)
                {
                    if (sourceEdges.Contains(
                            reduction.ClusterEdgeIndices[edgeIndex]))
                    {
                        return reduction;
                    }
                }
            }
            return null;
        }

        private static string FormatPlaneCutClusterReasonEvidence(
            List<int> clusterEdgeIndices,
            Dictionary<int, SortedSet<string>> reasons)
        {
            if (clusterEdgeIndices == null ||
                clusterEdgeIndices.Count == 0)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            for (int edgeIndex = 0;
                 edgeIndex < clusterEdgeIndices.Count;
                 edgeIndex++)
            {
                int sourceEdgeIndex = clusterEdgeIndices[edgeIndex];
                if (builder.Length > 0)
                {
                    builder.Append('/');
                }
                builder.Append(sourceEdgeIndex);
                builder.Append('=');
                if (!reasons.TryGetValue(
                        sourceEdgeIndex,
                        out SortedSet<string> edgeReasons) ||
                    edgeReasons.Count == 0)
                {
                    builder.Append("cluster");
                    continue;
                }
                bool first = true;
                foreach (string reason in edgeReasons)
                {
                    if (!first)
                    {
                        builder.Append('+');
                    }
                    builder.Append(reason);
                    first = false;
                }
            }
            return builder.ToString();
        }

        private static List<PlaneCutBevelCandidate>
            BuildScaledPlaneCutCandidates(
                List<PlaneCutBevelCandidate> originalCandidates,
                ChamferTopologyContext context,
                Dictionary<int, float> scaleByEdge,
                float minimumStableEdgeLength)
        {
            List<PlaneCutBevelCandidate> scaled =
                new List<PlaneCutBevelCandidate>(
                    originalCandidates.Count);
            for (int candidateIndex = 0;
                 candidateIndex < originalCandidates.Count;
                 candidateIndex++)
            {
                PlaneCutBevelCandidate original =
                    originalCandidates[candidateIndex];
                float scale = scaleByEdge.TryGetValue(
                        original.SourceEdgeIndex,
                        out float storedScale)
                    ? storedScale
                    : 1f;
                scaled.Add(ScalePlaneCutBevelCandidate(
                    original,
                    context,
                    scale,
                    minimumStableEdgeLength));
            }
            return scaled;
        }

        private static PlaneCutBevelCandidate ScalePlaneCutBevelCandidate(
            PlaneCutBevelCandidate original,
            ChamferTopologyContext context,
            float requestedScale,
            float minimumStableEdgeLength)
        {
            float minimumScale = ResolvePlaneCutCandidateMinimumScale(
                original,
                minimumStableEdgeLength);
            float scale = Mathf.Clamp(
                requestedScale,
                minimumScale,
                1f);
            EdgeWearGraphEdge edge =
                context.Graph.Edges[original.SourceEdgeIndex];
            Vector3 sourceA =
                context.Graph.Vertices[edge.VertexA].Position;
            Vector3 sourceB =
                context.Graph.Vertices[edge.VertexB].Position;
            float minimumRemovalFloor = Mathf.Max(
                PointMergeDistance * 2f,
                minimumStableEdgeLength * 0.02f);
            float scaledMinimumRemoval = Mathf.Max(
                minimumRemovalFloor,
                original.MinimumSourceRemoval * scale);
            float minimumSourceProjection = Mathf.Min(
                Vector3.Dot(original.Plane.Normal, sourceA),
                Vector3.Dot(original.Plane.Normal, sourceB));
            CutPlane scaledPlane = new CutPlane(
                original.Plane.Normal,
                minimumSourceProjection - scaledMinimumRemoval);
            float actualMinimumRemoval = Mathf.Min(
                scaledPlane.SignedDistance(sourceA),
                scaledPlane.SignedDistance(sourceB));
            float clipEpsilon = Mathf.Min(
                PlaneEpsilon,
                Mathf.Max(
                    PointMergeDistance * 0.25f,
                    actualMinimumRemoval * 0.25f));
            return new PlaneCutBevelCandidate(
                original.SourceEdgeIndex,
                original.VertexA,
                original.VertexB,
                original.Width * scale,
                scaledPlane,
                original.Strength,
                original.SelectionScore,
                original.PlaneTolerance,
                clipEpsilon,
                actualMinimumRemoval,
                original.WasLocalized);
        }

        private static float ResolvePlaneCutCandidateMinimumScale(
            PlaneCutBevelCandidate candidate,
            float minimumStableEdgeLength)
        {
            float minimumRemoval = Mathf.Max(
                PointMergeDistance * 2f,
                minimumStableEdgeLength * 0.02f);
            float removalScale = candidate.MinimumSourceRemoval > 0f
                ? minimumRemoval / candidate.MinimumSourceRemoval
                : 1f;
            float widthScale = candidate.Width > 0f
                ? PointMergeDistance * 2f / candidate.Width
                : 1f;
            return Mathf.Clamp01(Mathf.Max(
                EdgeWearMinimumFeasibleWidthFraction,
                Mathf.Max(removalScale, widthScale)));
        }

        private static bool TryBuildPlaneCutConflictCluster(
            List<PlaneCutBevelCandidate> activeCandidates,
            PlaneCutBevelAuditResult bandAudit,
            ChamferTopologyContext context,
            out List<int> clusterEdgeIndices)
        {
            clusterEdgeIndices = new List<int>();
            HashSet<int> clusterEdges = new HashSet<int>();
            HashSet<int> clusterVertices = new HashSet<int>();
            AddPlaneCutConflictSeed(
                activeCandidates,
                bandAudit.EdgeConflictVictimEdgeIndex,
                clusterEdges,
                clusterVertices);
            AddPlaneCutConflictSeed(
                activeCandidates,
                bandAudit.EdgeConflictForeignEdgeIndex,
                clusterEdges,
                clusterVertices);
            if (bandAudit.EdgeConflictVertexIndex >= 0 &&
                bandAudit.EdgeConflictVertexIndex <
                    context.Graph.Vertices.Count)
            {
                clusterVertices.Add(bandAudit.EdgeConflictVertexIndex);
            }

            if (clusterEdges.Count == 0 && clusterVertices.Count == 0)
            {
                return false;
            }

            for (int candidateIndex = 0;
                 candidateIndex < activeCandidates.Count;
                 candidateIndex++)
            {
                PlaneCutBevelCandidate candidate =
                    activeCandidates[candidateIndex];
                if (clusterVertices.Contains(candidate.VertexA) ||
                    clusterVertices.Contains(candidate.VertexB))
                {
                    clusterEdges.Add(candidate.SourceEdgeIndex);
                }
            }

            clusterEdgeIndices.AddRange(clusterEdges);
            clusterEdgeIndices.Sort();
            return clusterEdgeIndices.Count > 0;
        }

        private static void AddPlaneCutConflictSeed(
            List<PlaneCutBevelCandidate> activeCandidates,
            int sourceEdgeIndex,
            HashSet<int> clusterEdges,
            HashSet<int> clusterVertices)
        {
            if (!TryFindPlaneCutCandidateBySourceEdge(
                    activeCandidates,
                    sourceEdgeIndex,
                    out PlaneCutBevelCandidate candidate))
            {
                return;
            }
            clusterEdges.Add(candidate.SourceEdgeIndex);
            clusterVertices.Add(candidate.VertexA);
            clusterVertices.Add(candidate.VertexB);
        }

        private static float ResolveMinimumPlaneCutScale(
            Dictionary<int, float> scaleByEdge)
        {
            if (scaleByEdge == null || scaleByEdge.Count == 0)
            {
                return 1f;
            }
            float minimum = 1f;
            foreach (KeyValuePair<int, float> pair in scaleByEdge)
            {
                minimum = Mathf.Min(minimum, pair.Value);
            }
            return minimum;
        }

        private static float ResolveMinimumPlaneCutScale(
            Dictionary<int, float> scaleByEdge,
            List<int> edgeIndices)
        {
            float minimum = 1f;
            for (int edgeIndex = 0;
                 edgeIndex < edgeIndices.Count;
                 edgeIndex++)
            {
                if (scaleByEdge.TryGetValue(
                        edgeIndices[edgeIndex],
                        out float scale))
                {
                    minimum = Mathf.Min(minimum, scale);
                }
            }
            return minimum;
        }

        private static float ResolveMaximumPlaneCutScale(
            Dictionary<int, float> scaleByEdge,
            List<int> edgeIndices)
        {
            float maximum = 0f;
            for (int edgeIndex = 0;
                 edgeIndex < edgeIndices.Count;
                 edgeIndex++)
            {
                if (scaleByEdge.TryGetValue(
                        edgeIndices[edgeIndex],
                        out float scale))
                {
                    maximum = Mathf.Max(maximum, scale);
                }
            }
            return maximum;
        }

        private static string FormatPlaneCutScaleEvidence(
            Dictionary<int, float> scaleByEdge,
            List<int> edgeIndices)
        {
            if (scaleByEdge == null || edgeIndices == null ||
                edgeIndices.Count == 0)
            {
                return "none";
            }

            StringBuilder builder = new StringBuilder();
            for (int edgeIndex = 0;
                 edgeIndex < edgeIndices.Count;
                 edgeIndex++)
            {
                int sourceEdgeIndex = edgeIndices[edgeIndex];
                if (!scaleByEdge.TryGetValue(
                        sourceEdgeIndex,
                        out float scale))
                {
                    continue;
                }
                if (builder.Length > 0)
                {
                    builder.Append('/');
                }
                builder.Append(sourceEdgeIndex);
                builder.Append('=');
                builder.Append(scale.ToString("G9"));
            }
            return builder.Length == 0 ? "none" : builder.ToString();
        }

        private static bool TryBuildCleanPlaneCutEdgeOnlyShellWithDeferral(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            List<PlaneCutBevelCandidate> allCandidates,
            List<PlaneCutVertexJunctionCandidate> noJunctions,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            int localityDeferredCount,
            ref PlaneCutBevelAuditResult result,
            out List<PlaneCutBevelCandidate> retainedCandidates,
            out List<PolygonFace> preparedFaces,
            out string blocker)
        {
            const int maximumConflictPasses = 12;
            retainedCandidates = new List<PlaneCutBevelCandidate>(
                allCandidates);
            preparedFaces = null;
            blocker = string.Empty;
            PlaneCutBevelAuditResult lastBandAudit =
                CreatePlaneCutBandAuditScratch();

            for (int passIndex = 0;
                 passIndex < maximumConflictPasses &&
                 retainedCandidates.Count > 0;
                 passIndex++)
            {
                ResetPlaneCutStageTelemetry(ref result);
                PlaneCutNumericalRepairTelemetry numericalRepairs =
                    new PlaneCutNumericalRepairTelemetry();
                result.NumericalRepairs = numericalRepairs;
                result.EdgeConflictPassCount = passIndex + 1;
                if (!TryBuildPlaneCutSystemFaces(
                        sourceFaces,
                        retainedCandidates,
                        noJunctions,
                        out List<PolygonFace> rawFaces,
                        out int edgeCapsBuilt,
                        out string buildBlocker,
                        numericalRepairs))
                {
                    blocker = string.IsNullOrEmpty(buildBlocker)
                        ? "the deterministic edge-only shell could not be built"
                        : buildBlocker;
                    return false;
                }

                CapturePlaneCutStageSnapshot(
                    rawFaces,
                    "AfterPlaneConstruction",
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    retainedCandidates,
                    ref result,
                    out result.StagePlaneConstruction);

                if (!TryPreparePlaneCutPreviewFaces(
                        rawFaces,
                        retainedCandidates,
                        minimumStableEdgeLength,
                        minimumStableFaceArea,
                        ref result,
                        out List<PolygonFace> auditedFaces,
                        out int conformalSplitCount,
                        out int seamPairCount,
                        out int seamTouchedFaceCount,
                        out string preparationBlocker,
                        numericalRepairs))
                {
                    blocker = string.IsNullOrEmpty(preparationBlocker)
                        ? "the edge-only shell failed preview preparation"
                        : preparationBlocker;
                    return false;
                }

                PlaneCutBevelAuditResult bandAudit =
                    CreatePlaneCutBandAuditScratch();
                AuditPlaneCutBandIntegrity(
                    auditedFaces,
                    context,
                    retainedCandidates,
                    noJunctions,
                    minimumStableEdgeLength,
                    ref bandAudit,
                    out int offendingVertex,
                    out string bandBlocker);
                lastBandAudit = bandAudit;
                CapturePlaneCutEdgeConflict(
                    ref result,
                    bandAudit,
                    offendingVertex);

                bool bandClean = IsPlaneCutBandAuditClean(bandAudit);
                if (bandClean)
                {
                    preparedFaces = auditedFaces;
                    result.PlanesBuilt = retainedCandidates.Count;
                    result.PlanesDeferred = localityDeferredCount +
                        result.EdgeConflictEdgesDeferredCount;
                    result.PlanesLocalized =
                        CountLocalizedPlaneCutCandidates(
                            retainedCandidates);
                    result.CapsBuilt = edgeCapsBuilt;
                    result.ConformalSplitCount = conformalSplitCount;
                    result.SeamPairCount = seamPairCount;
                    result.FaceQualitySeamTouchedFaceCount =
                        seamTouchedFaceCount;
                    CopyPlaneCutBandAudit(
                        bandAudit,
                        ref result);
                    if (result.EdgeConflictEdgesDeferredCount > 0)
                    {
                        result.EdgeConflictResolvedCount = 1;
                    }
                    return true;
                }

                if (passIndex + 1 >= maximumConflictPasses)
                {
                    result.EdgeConflictBudgetExhausted = 1;
                    blocker = string.IsNullOrEmpty(bandBlocker)
                        ? "edge-plane conflict resolution exhausted its bounded pass budget"
                        : bandBlocker;
                    break;
                }

                if (!TrySelectPlaneCutEdgeConflictDeferral(
                        retainedCandidates,
                        bandAudit,
                        context,
                        out PlaneCutBevelCandidate deferredCandidate))
                {
                    result.EdgeConflictBudgetExhausted = 1;
                    blocker = string.IsNullOrEmpty(bandBlocker)
                        ? "an edge-plane conflict could not be attributed to a deterministic source edge"
                        : bandBlocker;
                    break;
                }

                if (result.EdgeConflictDeferredEdgeIndex < 0)
                {
                    result.EdgeConflictDeferredEdgeIndex =
                        deferredCandidate.SourceEdgeIndex;
                }
                retainedCandidates.RemoveAll(candidate =>
                    candidate.SourceEdgeIndex ==
                        deferredCandidate.SourceEdgeIndex);
                result.EdgeConflictEdgesDeferredCount++;
            }

            CopyPlaneCutBandAudit(lastBandAudit, ref result);
            result.PlanesBuilt = retainedCandidates.Count;
            result.PlanesDeferred = localityDeferredCount +
                result.EdgeConflictEdgesDeferredCount;
            result.PlanesLocalized = CountLocalizedPlaneCutCandidates(
                retainedCandidates);
            if (retainedCandidates.Count == 0 &&
                string.IsNullOrEmpty(blocker))
            {
                blocker =
                    "edge-plane conflict resolution deferred every active bevel edge";
            }
            return false;
        }

        private static PlaneCutBevelAuditResult
            CreatePlaneCutBandAuditScratch()
        {
            return new PlaneCutBevelAuditResult
            {
                EdgeConflictVictimEdgeIndex = -1,
                EdgeConflictForeignEdgeIndex = -1,
                EdgeConflictVertexIndex = -1,
                EdgeConflictDeferredEdgeIndex = -1,
                EdgeConflictMinimumWidthScale = 1f,
                EdgeConflictWidthReductions =
                    new List<PlaneCutConflictWidthReductionRecord>(),
                TopologyScaleSearchBasePass = -1,
                TopologyScaleSearchCommittedFactor = -1f,
                TopologyScaleSearchHighestValidFactor = -1f
            };
        }

        private static bool IsPlaneCutBandAuditClean(
            PlaneCutBevelAuditResult audit)
        {
            return audit.BandSplitCount == 0 &&
                audit.BandInterruptedCount == 0 &&
                audit.BandForeignCutCount == 0 &&
                audit.BandOverlongJunctionCount == 0 &&
                audit.BandCollapsedCount == 0;
        }

        private static void CopyPlaneCutBandAudit(
            PlaneCutBevelAuditResult source,
            ref PlaneCutBevelAuditResult destination)
        {
            destination.BandRetainedEdgeCount =
                source.BandRetainedEdgeCount;
            destination.BandSingleFaceCount =
                source.BandSingleFaceCount;
            destination.BandSplitCount = source.BandSplitCount;
            destination.BandInterruptedCount =
                source.BandInterruptedCount;
            destination.BandForeignCutCount =
                source.BandForeignCutCount;
            destination.BandOverlongJunctionCount =
                source.BandOverlongJunctionCount;
            destination.BandCollapsedCount = source.BandCollapsedCount;
            destination.BandMinimumCoverageRatio =
                source.BandMinimumCoverageRatio;
            destination.BandMaximumJunctionInfluenceRatio =
                source.BandMaximumJunctionInfluenceRatio;
            destination.BandMaximumSharedAxisSpanRatio =
                source.BandMaximumSharedAxisSpanRatio;
        }

        private static void CapturePlaneCutEdgeConflict(
            ref PlaneCutBevelAuditResult destination,
            PlaneCutBevelAuditResult source,
            int offendingVertex)
        {
            if (destination.EdgeConflictVictimEdgeIndex >= 0)
            {
                return;
            }
            destination.EdgeConflictVictimEdgeIndex =
                source.EdgeConflictVictimEdgeIndex;
            destination.EdgeConflictForeignEdgeIndex =
                source.EdgeConflictForeignEdgeIndex;
            destination.EdgeConflictVertexIndex =
                source.EdgeConflictVertexIndex >= 0
                    ? source.EdgeConflictVertexIndex
                    : offendingVertex;
            destination.EdgeConflictVictimCoverageRatio =
                source.EdgeConflictVictimCoverageRatio;
            destination.EdgeConflictForeignAxialParameter =
                source.EdgeConflictForeignAxialParameter;
            destination.EdgeConflictForeignSharedSpanRatio =
                source.EdgeConflictForeignSharedSpanRatio;
        }

        private static bool TrySelectPlaneCutEdgeConflictDeferral(
            List<PlaneCutBevelCandidate> activeCandidates,
            PlaneCutBevelAuditResult bandAudit,
            ChamferTopologyContext context,
            out PlaneCutBevelCandidate deferredCandidate)
        {
            deferredCandidate = default;
            bool hasVictim = TryFindPlaneCutCandidateBySourceEdge(
                activeCandidates,
                bandAudit.EdgeConflictVictimEdgeIndex,
                out PlaneCutBevelCandidate victim);
            bool hasForeign = TryFindPlaneCutCandidateBySourceEdge(
                activeCandidates,
                bandAudit.EdgeConflictForeignEdgeIndex,
                out PlaneCutBevelCandidate foreign);
            if (!hasVictim && !hasForeign)
            {
                return false;
            }
            if (!hasVictim)
            {
                deferredCandidate = foreign;
                return true;
            }
            if (!hasForeign)
            {
                deferredCandidate = victim;
                return true;
            }

            deferredCandidate = ComparePlaneCutBacktrackCandidates(
                    victim,
                    foreign,
                    context) <= 0
                ? victim
                : foreign;
            return true;
        }

        private static bool TryFindPlaneCutCandidateBySourceEdge(
            List<PlaneCutBevelCandidate> candidates,
            int sourceEdgeIndex,
            out PlaneCutBevelCandidate match)
        {
            match = default;
            if (sourceEdgeIndex < 0)
            {
                return false;
            }
            for (int candidateIndex = 0;
                 candidateIndex < candidates.Count;
                 candidateIndex++)
            {
                if (candidates[candidateIndex].SourceEdgeIndex !=
                    sourceEdgeIndex)
                {
                    continue;
                }
                match = candidates[candidateIndex];
                return true;
            }
            return false;
        }

        private static bool TryBuildPlaneCutBevelCandidate(
            ChamferTopologyContext context,
            ChamferCornerSolution solution,
            EdgeWearSelectedGraphEdge selected,
            EdgeWearEdgeViabilityRecord viability,
            float minimumStableEdgeLength,
            out PlaneCutBevelCandidate candidate,
            out bool localityDeferred,
            out PlaneCutLocalityDeferralRecord localityEvidence,
            out string blocker)
        {
            candidate = default;
            localityDeferred = false;
            localityEvidence = null;
            blocker = string.Empty;
            int edgeIndex = selected.GraphEdgeIndex;
            if (edgeIndex < 0 || edgeIndex >= context.Graph.Edges.Count)
            {
                blocker = "a selected edge has invalid graph provenance";
                return false;
            }

            EdgeWearGraphEdge edge = context.Graph.Edges[edgeIndex];
            if (edge.FaceA < 0 || edge.FaceB < 0)
            {
                blocker = "a selected edge is not an internal manifold edge";
                return false;
            }

            Vector3 normal = selected.Candidate.BevelNormal;
            if (!IsFinite(normal) ||
                normal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                blocker = "a selected edge has an invalid bevel normal";
                return false;
            }
            normal.Normalize();

            Vector3 a0 = solution.Corners[
                new ChamferFaceCornerKey(
                    edge.FaceA,
                    edge.VertexA)].Position;
            Vector3 b0 = solution.Corners[
                new ChamferFaceCornerKey(
                    edge.FaceA,
                    edge.VertexB)].Position;
            Vector3 a1 = solution.Corners[
                new ChamferFaceCornerKey(
                    edge.FaceB,
                    edge.VertexA)].Position;
            Vector3 b1 = solution.Corners[
                new ChamferFaceCornerKey(
                    edge.FaceB,
                    edge.VertexB)].Position;

            if (!IsFinite(a0) || !IsFinite(b0) ||
                !IsFinite(a1) || !IsFinite(b1))
            {
                blocker = "a selected edge has non-finite solved bevel points";
                return false;
            }

            float d0 = Vector3.Dot(normal, a0);
            float d1 = Vector3.Dot(normal, b0);
            float d2 = Vector3.Dot(normal, a1);
            float d3 = Vector3.Dot(normal, b1);
            float minimumDistance = Mathf.Min(
                Mathf.Min(d0, d1),
                Mathf.Min(d2, d3));
            float maximumDistance = Mathf.Max(
                Mathf.Max(d0, d1),
                Mathf.Max(d2, d3));
            float planeTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                minimumStableEdgeLength * 0.02f);
            if (maximumDistance - minimumDistance > planeTolerance)
            {
                blocker = "the four solved bevel points are not coplanar";
                return false;
            }

            float solvedDistance = (d0 + d1 + d2 + d3) * 0.25f;
            if (viability == null || !viability.Evaluated)
            {
                blocker =
                    "a selected edge lacks its cached viability preflight";
                return false;
            }
            if (!viability.LocalityValid)
            {
                blocker = "a selected edge failed cached independent-plane locality viability";
                return false;
            }

            float localizedDistance = Mathf.Max(
                solvedDistance,
                viability.LocalityRetainPlaneFloor);
            float localGuardMargin =
                viability.LocalityGuardMargin;
            int limitingUnrelatedVertex =
                viability.LocalityLimitingVertex;
            Vector3 limitingUnrelatedPosition =
                viability.LocalityLimitingPosition;
            float limitingUnrelatedProjection =
                viability.LocalityLimitingProjection;
            bool wasLocalized = localizedDistance >
                solvedDistance + PointMergeDistance * 0.25f;
            Vector3 sourceA =
                context.Graph.Vertices[edge.VertexA].Position;
            Vector3 sourceB =
                context.Graph.Vertices[edge.VertexB].Position;
            float minimumRemoval =
                viability.LocalityMinimumRemoval;
            if (localizedDistance >
                viability.LocalityRemovalPlaneCeiling +
                    PointMergeDistance * 0.25f)
            {
                blocker =
                    "the globally solved bevel plane lies outside the cached locality-feasible interval";
                return false;
            }

            CutPlane plane = new CutPlane(normal, localizedDistance);
            float sourceRemovalA = plane.SignedDistance(sourceA);
            float sourceRemovalB = plane.SignedDistance(sourceB);
            if (sourceRemovalA <= minimumRemoval ||
                sourceRemovalB <= minimumRemoval)
            {
                blocker =
                    "the globally solved bevel plane does not remove its source edge inside the cached locality interval";
                localityEvidence = new PlaneCutLocalityDeferralRecord
                {
                    SourceEdgeIndex = edgeIndex,
                    VertexA = edge.VertexA,
                    VertexB = edge.VertexB,
                    FaceA = edge.FaceA,
                    FaceB = edge.FaceB,
                    SourceA = sourceA,
                    SourceB = sourceB,
                    BevelNormal = normal,
                    SolvedWidth = solution.WidthByEdge[edgeIndex],
                    SolvedPlaneDistance = solvedDistance,
                    LocalizedPlaneDistance = localizedDistance,
                    LocalizationDelta = localizedDistance - solvedDistance,
                    LocalGuardMargin = localGuardMargin,
                    LimitingUnrelatedVertex = limitingUnrelatedVertex,
                    LimitingUnrelatedPosition =
                        limitingUnrelatedPosition,
                    LimitingUnrelatedProjection =
                        limitingUnrelatedProjection,
                    SolvedSourceRemovalA =
                        Vector3.Dot(normal, sourceA) - solvedDistance,
                    SolvedSourceRemovalB =
                        Vector3.Dot(normal, sourceB) - solvedDistance,
                    LocalizedSourceRemovalA = sourceRemovalA,
                    LocalizedSourceRemovalB = sourceRemovalB,
                    MinimumRequiredRemoval = minimumRemoval,
                    Cause = blocker
                };
                return false;
            }

            float minimumSourceRemoval = Mathf.Min(
                sourceRemovalA,
                sourceRemovalB);
            float clipEpsilon = Mathf.Min(
                PlaneEpsilon,
                Mathf.Max(
                    PointMergeDistance * 0.25f,
                    minimumSourceRemoval * 0.25f));

            candidate = new PlaneCutBevelCandidate(
                edgeIndex,
                edge.VertexA,
                edge.VertexB,
                solution.WidthByEdge[edgeIndex],
                plane,
                selected.Candidate.Strength,
                selected.Candidate.Score,
                planeTolerance,
                clipEpsilon,
                minimumSourceRemoval,
                wasLocalized);
            return true;
        }

        private const float PlaneCutMaximumNormalSpreadDegrees = 0.75f;

        private static float ResolvePlaneCutPlanarityTolerance(
            float minimumStableEdgeLength)
        {
            return Mathf.Max(
                PointMergeDistance * 2f,
                minimumStableEdgeLength * 0.00005f);
        }

        private static void ResetPlaneCutStageTelemetry(
            ref PlaneCutBevelAuditResult result)
        {
            result.StagePlaneConstruction = default;
            result.StageSanitized = default;
            result.StageWelded = default;
            result.StageConformed = default;
            result.StageSeamRepaired = default;
            result.StageFinalCertification = default;
            result.FirstOpenEdgeStage = string.Empty;
            result.FirstTJunctionStage = string.Empty;
            result.FirstNonPlanarStage = string.Empty;
            result.OpenEdgeCount = 0;
            result.NonManifoldEdgeCount = 0;
            result.TJunctionCount = 0;
            result.InvalidFaceCount = 0;
            result.FaceQualityFaceCount = 0;
            result.FaceQualityNonPlanarCount = 0;
            result.FaceQualityMaxPlaneDeviation = 0f;
            result.FaceQualityMaxNormalSpreadDegrees = 0f;
            result.FaceQualityFailures?.Clear();
            result.OpenEdgeFailures?.Clear();
            result.TJunctionFailures?.Clear();
            result.FaceFirstNonPlanarStageByIdentity?.Clear();
            result.BoundaryConformityTouchedFaces?.Clear();
            result.SeamRepairTouchedFaces?.Clear();
            result.SeamRepairMaximumMovementByIdentity?.Clear();
        }

        private static string BuildPlaneCutFaceIdentity(
            PolygonFace face,
            int faceIndex)
        {
            if (face == null)
            {
                return "NullFace:" + faceIndex;
            }
            if (face.ProvenanceKind != PolygonFaceProvenanceKind.None &&
                face.ProvenanceIndex >= 0)
            {
                return face.ProvenanceKind + ":" +
                    face.ProvenanceIndex;
            }
            return "FaceIndex:" + faceIndex;
        }

        private static void CapturePlaneCutModifiedFaceIdentities(
            List<PolygonFace> before,
            List<PolygonFace> after,
            HashSet<string> touchedIdentities,
            Dictionary<string, float> maximumMovementByIdentity)
        {
            if (before == null || after == null ||
                touchedIdentities == null)
            {
                return;
            }

            int count = Mathf.Min(before.Count, after.Count);
            for (int faceIndex = 0; faceIndex < count; faceIndex++)
            {
                PolygonFace left = before[faceIndex];
                PolygonFace right = after[faceIndex];
                if (left == null || right == null ||
                    left.Vertices == null || right.Vertices == null)
                {
                    continue;
                }

                bool touched = left.Vertices.Count != right.Vertices.Count;
                float maximumMovement = 0f;
                int vertexCount = Mathf.Min(
                    left.Vertices.Count,
                    right.Vertices.Count);
                for (int vertexIndex = 0;
                     vertexIndex < vertexCount;
                     vertexIndex++)
                {
                    float movement = Vector3.Distance(
                        left.Vertices[vertexIndex],
                        right.Vertices[vertexIndex]);
                    maximumMovement = Mathf.Max(
                        maximumMovement,
                        movement);
                    if (movement > Mathf.Sqrt(MinimumEdgeLengthSqr))
                    {
                        touched = true;
                    }
                }

                if (!touched)
                {
                    continue;
                }

                string identity = BuildPlaneCutFaceIdentity(
                    right,
                    faceIndex);
                touchedIdentities.Add(identity);
                if (maximumMovementByIdentity != null)
                {
                    maximumMovementByIdentity[identity] =
                        maximumMovement;
                }
            }
        }

        private static void CapturePlaneCutStageSnapshot(
            List<PolygonFace> faces,
            string stage,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            List<PlaneCutBevelCandidate> candidates,
            ref PlaneCutBevelAuditResult result,
            out PlaneCutStageSnapshot snapshot)
        {
            int faceCount = faces == null ? 0 : faces.Count;
            int vertexCount = 0;
            int bevelFaceCount = 0;
            int junctionFaceCount = 0;
            int nonPlanarFaceCount = 0;
            float maximumDeviation = 0f;
            float maximumSpread = 0f;
            Dictionary<VertexKey, Vector3> unique =
                new Dictionary<VertexKey, Vector3>();
            float planarityTolerance =
                ResolvePlaneCutPlanarityTolerance(
                    minimumStableEdgeLength);

            if (faces != null)
            {
                for (int faceIndex = 0;
                     faceIndex < faces.Count;
                     faceIndex++)
                {
                    PolygonFace face = faces[faceIndex];
                    if (face == null || face.Vertices == null)
                    {
                        continue;
                    }
                    vertexCount += face.Vertices.Count;
                    for (int vertexIndex = 0;
                         vertexIndex < face.Vertices.Count;
                         vertexIndex++)
                    {
                        Vector3 vertex = face.Vertices[vertexIndex];
                        VertexKey key = new VertexKey(vertex);
                        if (!unique.ContainsKey(key))
                        {
                            unique.Add(key, vertex);
                        }
                    }

                    if (face.ProvenanceKind ==
                        PolygonFaceProvenanceKind.EdgeBevelPlane)
                    {
                        bevelFaceCount++;
                    }
                    else if (face.ProvenanceKind ==
                        PolygonFaceProvenanceKind.VertexJunctionPlane)
                    {
                        junctionFaceCount++;
                    }

                    if (face.Feature !=
                            PolygonFaceFeature.ConvexEdgeWear ||
                        face.Vertices.Count < 3)
                    {
                        continue;
                    }

                    MeasurePlaneCutFacePlanarityDetailed(
                        face,
                        out float deviation,
                        out _,
                        out _,
                        out _,
                        out float spread,
                        out _,
                        out _,
                        out _,
                        out _,
                        out _);
                    maximumDeviation = Mathf.Max(
                        maximumDeviation,
                        deviation);
                    maximumSpread = Mathf.Max(
                        maximumSpread,
                        spread);
                    if (deviation > planarityTolerance ||
                        spread > PlaneCutMaximumNormalSpreadDegrees)
                    {
                        nonPlanarFaceCount++;
                        if (result.FaceFirstNonPlanarStageByIdentity != null)
                        {
                            string identity = BuildPlaneCutFaceIdentity(
                                face,
                                faceIndex);
                            if (!result.FaceFirstNonPlanarStageByIdentity
                                    .ContainsKey(identity))
                            {
                                result.FaceFirstNonPlanarStageByIdentity.Add(
                                    identity,
                                    stage);
                            }
                        }
                    }
                }
            }

            EdgeWearTopologyStats topology = faces == null
                ? default
                : AuditEdgeWearTopology(
                    faces,
                    minimumStableEdgeLength);
            int invalidFaceCount = faces == null
                ? 0
                : CountInvalidPlaneCutFaces(
                    faces,
                    minimumStableFaceArea);
            snapshot = new PlaneCutStageSnapshot(
                stage,
                faceCount,
                vertexCount,
                unique.Count,
                bevelFaceCount,
                junctionFaceCount,
                topology.OpenEdgeCount,
                topology.NonManifoldEdgeCount,
                topology.TJunctionCount,
                invalidFaceCount,
                nonPlanarFaceCount,
                maximumDeviation,
                maximumSpread);

            if (topology.OpenEdgeCount > 0 &&
                string.IsNullOrEmpty(result.FirstOpenEdgeStage))
            {
                result.FirstOpenEdgeStage = stage;
            }
            if (topology.TJunctionCount > 0)
            {
                if (string.IsNullOrEmpty(result.FirstTJunctionStage))
                {
                    result.FirstTJunctionStage = stage;
                }
                CapturePlaneCutTJunctionFailures(
                    faces,
                    stage,
                    minimumStableEdgeLength,
                    candidates,
                    ref result);
            }
            if (nonPlanarFaceCount > 0 &&
                string.IsNullOrEmpty(result.FirstNonPlanarStage))
            {
                result.FirstNonPlanarStage = stage;
            }
        }

        private static void CapturePlaneCutTJunctionFailures(
            List<PolygonFace> faces,
            string stage,
            float minimumStableEdgeLength,
            List<PlaneCutBevelCandidate> candidates,
            ref PlaneCutBevelAuditResult result)
        {
            if (faces == null || faces.Count == 0)
            {
                return;
            }
            if (result.TJunctionFailures == null)
            {
                result.TJunctionFailures =
                    new List<PlaneCutTJunctionFailureRecord>();
            }

            Dictionary<VertexKey, Vector3> uniqueVertices =
                new Dictionary<VertexKey, Vector3>();
            List<VertexKey> vertexOrder = new List<VertexKey>();
            Dictionary<VertexKey, List<string>> ownerFacesByVertex =
                new Dictionary<VertexKey, List<string>>();
            Dictionary<VertexKey, SortedSet<int>>
                ownerBevelEdgesByVertex =
                    new Dictionary<VertexKey, SortedSet<int>>();
            List<PlaneCutDiagnosticSegment> segments =
                new List<PlaneCutDiagnosticSegment>();

            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face == null || face.Vertices == null ||
                    face.Vertices.Count < 2)
                {
                    continue;
                }
                for (int vertexIndex = 0;
                     vertexIndex < face.Vertices.Count;
                     vertexIndex++)
                {
                    Vector3 vertex = face.Vertices[vertexIndex];
                    VertexKey key = new VertexKey(vertex);
                    if (!uniqueVertices.ContainsKey(key))
                    {
                        uniqueVertices.Add(key, vertex);
                        vertexOrder.Add(key);
                    }
                    if (!ownerFacesByVertex.TryGetValue(
                            key,
                            out List<string> owners))
                    {
                        owners = new List<string>();
                        ownerFacesByVertex.Add(key, owners);
                    }
                    owners.Add(
                        BuildPlaneCutFaceIdentity(face, faceIndex) +
                        "#" + faceIndex + "[v" + vertexIndex + "]");
                    if (face.ProvenanceKind ==
                            PolygonFaceProvenanceKind.EdgeBevelPlane &&
                        face.ProvenanceIndex >= 0)
                    {
                        if (!ownerBevelEdgesByVertex.TryGetValue(
                                key,
                                out SortedSet<int> ownerEdges))
                        {
                            ownerEdges = new SortedSet<int>();
                            ownerBevelEdgesByVertex.Add(key, ownerEdges);
                        }
                        ownerEdges.Add(face.ProvenanceIndex);
                    }

                    Vector3 end = face.Vertices[
                        (vertexIndex + 1) % face.Vertices.Count];
                    if (!AreSamePoint(vertex, end))
                    {
                        segments.Add(new PlaneCutDiagnosticSegment(
                            faceIndex,
                            face.ProvenanceKind,
                            face.ProvenanceIndex,
                            vertexIndex,
                            vertex,
                            end));
                    }
                }
            }

            float tolerance = CalculateTopologyTJunctionTolerance(
                minimumStableEdgeLength);
            float toleranceSqr = tolerance * tolerance;
            for (int vertexOrderIndex = 0;
                 vertexOrderIndex < vertexOrder.Count;
                 vertexOrderIndex++)
            {
                VertexKey vertexKey = vertexOrder[vertexOrderIndex];
                Vector3 vertex = uniqueVertices[vertexKey];
                int bestSegmentIndex = -1;
                float bestDistance = float.PositiveInfinity;
                float bestParameter = 0f;
                Vector3 bestClosest = default;
                int matchingSegments = 0;

                for (int segmentIndex = 0;
                     segmentIndex < segments.Count;
                     segmentIndex++)
                {
                    PlaneCutDiagnosticSegment segment =
                        segments[segmentIndex];
                    if (vertexKey.Equals(segment.StartKey) ||
                        vertexKey.Equals(segment.EndKey))
                    {
                        continue;
                    }
                    if (!TryMeasurePlaneCutTJunction(
                            vertex,
                            segment.Start,
                            segment.End,
                            toleranceSqr,
                            out float parameter,
                            out Vector3 closest,
                            out float distance))
                    {
                        continue;
                    }
                    matchingSegments++;
                    if (distance < bestDistance - 0.000000001f ||
                        (Mathf.Abs(distance - bestDistance) <=
                            0.000000001f &&
                         (bestSegmentIndex < 0 ||
                          segment.FaceIndex <
                              segments[bestSegmentIndex].FaceIndex ||
                          (segment.FaceIndex ==
                               segments[bestSegmentIndex].FaceIndex &&
                           segment.SegmentIndex <
                               segments[bestSegmentIndex].SegmentIndex))))
                    {
                        bestSegmentIndex = segmentIndex;
                        bestDistance = distance;
                        bestParameter = parameter;
                        bestClosest = closest;
                    }
                }

                if (bestSegmentIndex < 0)
                {
                    continue;
                }

                PlaneCutDiagnosticSegment host =
                    segments[bestSegmentIndex];
                SortedSet<int> provenanceEdges =
                    ownerBevelEdgesByVertex.TryGetValue(
                            vertexKey,
                            out SortedSet<int> storedOwnerEdges)
                        ? new SortedSet<int>(storedOwnerEdges)
                        : new SortedSet<int>();
                if (host.ProvenanceKind ==
                        PolygonFaceProvenanceKind.EdgeBevelPlane &&
                    host.ProvenanceIndex >= 0)
                {
                    provenanceEdges.Add(host.ProvenanceIndex);
                }

                SortedSet<int> candidatePlaneMatches =
                    new SortedSet<int>();
                if (candidates != null)
                {
                    for (int candidateIndex = 0;
                         candidateIndex < candidates.Count;
                         candidateIndex++)
                    {
                        PlaneCutBevelCandidate candidate =
                            candidates[candidateIndex];
                        float matchTolerance = Mathf.Max(
                            tolerance,
                            candidate.PlaneTolerance);
                        if (Mathf.Abs(candidate.Plane.SignedDistance(vertex)) <=
                            matchTolerance)
                        {
                            candidatePlaneMatches.Add(
                                candidate.SourceEdgeIndex);
                        }
                    }
                }

                SortedSet<int> linkedEdges =
                    new SortedSet<int>(provenanceEdges);
                linkedEdges.UnionWith(candidatePlaneMatches);
                ResolvePlaneCutLastConflictForEdges(
                    result.EdgeConflictWidthReductions,
                    linkedEdges,
                    out int lastConflictPass,
                    out string lastConflictCluster);

                PlaneCutTJunctionFailureRecord failure =
                    new PlaneCutTJunctionFailureRecord
                    {
                        RecordIndex = result.TJunctionFailures.Count,
                        Stage = stage,
                        JunctionVertex = vertex,
                        VertexOwnerFaceCount =
                            ownerFacesByVertex[vertexKey].Count,
                        VertexOwnerFaces = string.Join(
                            "/",
                            ownerFacesByVertex[vertexKey]),
                        HostFaceIndex = host.FaceIndex,
                        HostProvenanceKind = host.ProvenanceKind,
                        HostProvenanceIndex = host.ProvenanceIndex,
                        HostSegmentIndex = host.SegmentIndex,
                        HostStart = host.Start,
                        HostEnd = host.End,
                        ClosestPoint = bestClosest,
                        HostLength = Vector3.Distance(
                            host.Start,
                            host.End),
                        SegmentParameter = bestParameter,
                        Distance = bestDistance,
                        Tolerance = tolerance,
                        MatchingHostSegmentCount = matchingSegments,
                        ProvenanceBevelEdges =
                            FormatPlaneCutSortedEdgeSet(provenanceEdges),
                        CandidatePlaneMatches =
                            FormatPlaneCutSortedEdgeSet(
                                candidatePlaneMatches),
                        AssociatedEdgeScales =
                            FormatPlaneCutCandidateScaleEvidence(
                                candidates,
                                result.CoverageAudit,
                                linkedEdges),
                        LastConflictPass = lastConflictPass,
                        LastConflictCluster = lastConflictCluster,
                        Cause = "vertex-on-unsplit-host-segment"
                    };
                failure.LinkedEdgeIndices.AddRange(linkedEdges);
                result.TJunctionFailures.Add(failure);
            }
        }

        private static bool TryMeasurePlaneCutTJunction(
            Vector3 point,
            Vector3 start,
            Vector3 end,
            float toleranceSqr,
            out float parameter,
            out Vector3 closest,
            out float distance)
        {
            parameter = 0f;
            closest = default;
            distance = float.PositiveInfinity;
            Vector3 segment = end - start;
            float segmentLengthSqr = segment.sqrMagnitude;
            if (segmentLengthSqr <= MinimumEdgeLengthSqr ||
                (point - start).sqrMagnitude <= toleranceSqr ||
                (point - end).sqrMagnitude <= toleranceSqr)
            {
                return false;
            }
            parameter = Vector3.Dot(point - start, segment) /
                segmentLengthSqr;
            if (parameter <= 0f || parameter >= 1f)
            {
                return false;
            }
            closest = start + segment * parameter;
            distance = Vector3.Distance(point, closest);
            return distance * distance <= toleranceSqr;
        }

        private static void ResolvePlaneCutLastConflictForEdges(
            List<PlaneCutConflictWidthReductionRecord> reductions,
            SortedSet<int> sourceEdges,
            out int lastPass,
            out string clusterEvidence)
        {
            lastPass = -1;
            clusterEvidence = "none";
            if (reductions == null || sourceEdges == null ||
                sourceEdges.Count == 0)
            {
                return;
            }
            for (int recordIndex = reductions.Count - 1;
                 recordIndex >= 0;
                 recordIndex--)
            {
                PlaneCutConflictWidthReductionRecord reduction =
                    reductions[recordIndex];
                bool intersects = false;
                for (int edgeIndex = 0;
                     edgeIndex < reduction.ClusterEdgeIndices.Count;
                     edgeIndex++)
                {
                    if (sourceEdges.Contains(
                            reduction.ClusterEdgeIndices[edgeIndex]))
                    {
                        intersects = true;
                        break;
                    }
                }
                if (!intersects)
                {
                    continue;
                }
                lastPass = reduction.PassIndex;
                clusterEvidence = FormatPlaneCutEdgeIndexEvidence(
                    reduction.ClusterEdgeIndices) +
                    "@" + reduction.AppliedScaleEvidence;
                return;
            }
        }

        private static string FormatPlaneCutSortedEdgeSet(
            SortedSet<int> sourceEdges)
        {
            if (sourceEdges == null || sourceEdges.Count == 0)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            foreach (int sourceEdge in sourceEdges)
            {
                if (builder.Length > 0)
                {
                    builder.Append('/');
                }
                builder.Append(sourceEdge);
            }
            return builder.ToString();
        }

        private static string FormatPlaneCutCandidateScaleEvidence(
            List<PlaneCutBevelCandidate> candidates,
            EdgeWearCoverageAudit coverageAudit,
            SortedSet<int> sourceEdges)
        {
            if (candidates == null || sourceEdges == null ||
                sourceEdges.Count == 0)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            for (int candidateIndex = 0;
                 candidateIndex < candidates.Count;
                 candidateIndex++)
            {
                PlaneCutBevelCandidate candidate = candidates[candidateIndex];
                if (!sourceEdges.Contains(candidate.SourceEdgeIndex))
                {
                    continue;
                }
                float solvedWidth = candidate.Width;
                if (TryGetEdgeWearCoverageRecord(
                        coverageAudit,
                        candidate.SourceEdgeIndex,
                        out EdgeWearEdgeLifecycleRecord lifecycle) &&
                    lifecycle.SolvedWidth > PointMergeDistance)
                {
                    solvedWidth = lifecycle.SolvedWidth;
                }
                float scale = solvedWidth > PointMergeDistance
                    ? candidate.Width / solvedWidth
                    : 1f;
                if (builder.Length > 0)
                {
                    builder.Append('/');
                }
                builder.Append(candidate.SourceEdgeIndex);
                builder.Append('=');
                builder.Append(scale.ToString("G9"));
                builder.Append('(');
                builder.Append(candidate.Width.ToString("G9"));
                builder.Append(')');
            }
            return builder.Length == 0 ? "none" : builder.ToString();
        }

        private static bool TryPreparePlaneCutPreviewFaces(
            List<PolygonFace> sourceFaces,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            ref PlaneCutBevelAuditResult result,
            out List<PolygonFace> auditedFaces,
            out int conformalSplitCount,
            out int seamPairCount,
            out int seamTouchedFaceCount,
            out string blocker,
            PlaneCutNumericalRepairTelemetry numericalRepairs = null)
        {
            return TryPreparePlaneCutPreviewFaces(
                sourceFaces,
                null,
                minimumStableEdgeLength,
                minimumStableFaceArea,
                ref result,
                out auditedFaces,
                out conformalSplitCount,
                out seamPairCount,
                out seamTouchedFaceCount,
                out blocker,
                numericalRepairs);
        }

        private static bool TryPreparePlaneCutPreviewFaces(
            List<PolygonFace> sourceFaces,
            List<PlaneCutBevelCandidate> candidates,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            ref PlaneCutBevelAuditResult result,
            out List<PolygonFace> auditedFaces,
            out int conformalSplitCount,
            out int seamPairCount,
            out int seamTouchedFaceCount,
            out string blocker,
            PlaneCutNumericalRepairTelemetry numericalRepairs = null)
        {
            auditedFaces = new List<PolygonFace>(sourceFaces.Count);
            conformalSplitCount = 0;
            seamPairCount = 0;
            seamTouchedFaceCount = 0;
            blocker = string.Empty;

            for (int faceIndex = 0;
                 faceIndex < sourceFaces.Count;
                 faceIndex++)
            {
                PolygonFace sourceFace = sourceFaces[faceIndex];
                List<Vector3> sanitized = SanitizePolygon(
                    sourceFace.Vertices,
                    sourceFace.Normal);
                if (sanitized.Count < 3 ||
                    CalculatePolygonArea(sanitized) <= minimumStableFaceArea)
                {
                    blocker =
                        "a plane-cut face collapses during preview sanitation";
                    return false;
                }

                Vector3 measuredNormal =
                    CalculatePolygonNormal(sanitized);
                if (!IsFinite(measuredNormal) ||
                    measuredNormal.sqrMagnitude <= MinimumEdgeLengthSqr ||
                    Vector3.Dot(measuredNormal, sourceFace.Normal) <= 0f)
                {
                    blocker =
                        "a plane-cut face changes winding during preview sanitation";
                    return false;
                }

                auditedFaces.Add(new PolygonFace(
                    sanitized,
                    sourceFace.Normal,
                    sourceFace.Feature,
                    sourceFace.FeatureStrength,
                    sourceFace.ProvenanceKind,
                    sourceFace.ProvenanceIndex));
            }

            CapturePlaneCutStageSnapshot(
                auditedFaces,
                "AfterSanitation",
                minimumStableEdgeLength,
                minimumStableFaceArea,
                candidates,
                ref result,
                out result.StageSanitized);

            if (numericalRepairs != null)
            {
                WeldSharedVerticesByDistance(
                    auditedFaces,
                    PointMergeDistance,
                    numericalRepairs);
            }
            else
            {
                WeldSharedVertices(auditedFaces);
            }
            CapturePlaneCutStageSnapshot(
                auditedFaces,
                "AfterWeld",
                minimumStableEdgeLength,
                minimumStableFaceArea,
                candidates,
                ref result,
                out result.StageWelded);

            List<PolygonFace> beforeConformity =
                ClonePolygonFacesForPlaneCutAudit(auditedFaces);
            conformalSplitCount = ConformPlaneCutFaceBoundaries(
                auditedFaces,
                minimumStableEdgeLength);
            CapturePlaneCutModifiedFaceIdentities(
                beforeConformity,
                auditedFaces,
                result.BoundaryConformityTouchedFaces,
                null);
            CapturePlaneCutStageSnapshot(
                auditedFaces,
                "AfterBoundaryConformity",
                minimumStableEdgeLength,
                minimumStableFaceArea,
                candidates,
                ref result,
                out result.StageConformed);

            List<PolygonFace> beforeSeamRepair =
                ClonePolygonFacesForPlaneCutAudit(auditedFaces);
            seamPairCount = RepairPlaneCutNumericalSeams(
                auditedFaces,
                minimumStableEdgeLength,
                out seamTouchedFaceCount);
            CapturePlaneCutModifiedFaceIdentities(
                beforeSeamRepair,
                auditedFaces,
                result.SeamRepairTouchedFaces,
                result.SeamRepairMaximumMovementByIdentity);
            CapturePlaneCutStageSnapshot(
                auditedFaces,
                "AfterSeamRepair",
                minimumStableEdgeLength,
                minimumStableFaceArea,
                candidates,
                ref result,
                out result.StageSeamRepaired);
            return auditedFaces.Count >= 4;
        }

        private static TriangleSoup TriangulatePlaneCutPreviewFaces(
            List<PolygonFace> faces)
        {
            BoundedSingleEdgeAuditResult surfaceAudit =
                new BoundedSingleEdgeAuditResult
                {
                    TriangulationFailureFace = -1,
                    TriangulationFailureProvenanceIndex = -1,
                    BevelRegionFailureFace = -1,
                    BevelRegionFailureProvenanceIndex = -1
                };
            return TryTriangulateBoundedPreviewFaces(
                    faces,
                    TinyFaceAreaEpsilon,
                    ref surfaceAudit,
                    out TriangleSoup soup,
                    out _)
                ? soup
                : null;
        }

        private static void AuditPlaneCutPreviewTriangleSoup(
            TriangleSoup soup,
            List<PolygonFace> auditedFaces,
            float minimumStableEdgeLength,
            ref PlaneCutBevelAuditResult result)
        {
            if (soup == null || soup.Positions.Count < 3 ||
                soup.Positions.Count % 3 != 0)
            {
                result.PreviewDegenerateTriangleCount++;
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    "the plane-cut preview triangle soup is empty or malformed");
                return;
            }

            result.PreviewTriangleCount = soup.Positions.Count / 3;
            Vector3 solidCentre = CalculatePlaneCutFaceVertexCentre(
                auditedFaces);
            Dictionary<EdgeKey, int> edgeUses =
                new Dictionary<EdgeKey, int>();
            double signedVolume = 0.0;

            for (int triangleIndex = 0;
                 triangleIndex < soup.Positions.Count;
                 triangleIndex += 3)
            {
                Vector3 a = soup.Positions[triangleIndex];
                Vector3 b = soup.Positions[triangleIndex + 1];
                Vector3 c = soup.Positions[triangleIndex + 2];
                if (!IsFinite(a) || !IsFinite(b) || !IsFinite(c))
                {
                    result.PreviewDegenerateTriangleCount++;
                    continue;
                }

                Vector3 ab = b - a;
                Vector3 ac = c - a;
                Vector3 bc = c - b;
                float maximumEdgeLengthSqr = Mathf.Max(
                    ab.sqrMagnitude,
                    Mathf.Max(ac.sqrMagnitude, bc.sqrMagnitude));
                Vector3 normal = Vector3.Cross(ab, ac);
                float relativeAreaThreshold =
                    maximumEdgeLengthSqr *
                    maximumEdgeLengthSqr *
                    RelativeTriangleAreaEpsilon;
                if (maximumEdgeLengthSqr <= MinimumEdgeLengthSqr ||
                    normal.sqrMagnitude <= relativeAreaThreshold)
                {
                    result.PreviewDegenerateTriangleCount++;
                    continue;
                }

                Vector3 faceCentre = (a + b + c) / 3f;
                if (Vector3.Dot(normal, faceCentre - solidCentre) <= 0f)
                {
                    result.PreviewWindingFailureCount++;
                }

                CountPlaneCutTriangleEdge(edgeUses, a, b);
                CountPlaneCutTriangleEdge(edgeUses, b, c);
                CountPlaneCutTriangleEdge(edgeUses, c, a);
                signedVolume += Vector3.Dot(
                    a,
                    Vector3.Cross(b, c)) / 6.0;
            }

            foreach (KeyValuePair<EdgeKey, int> entry in edgeUses)
            {
                if (entry.Value == 1)
                {
                    result.PreviewOpenEdgeCount++;
                }
                else if (entry.Value > 2)
                {
                    result.PreviewNonManifoldEdgeCount++;
                }
            }

            Bounds polygonBounds = CalculateFaceBounds(auditedFaces);
            Bounds soupBounds = CalculateBounds(soup.Positions);
            float boundsTolerance = Mathf.Max(
                PlaneEpsilon * 1.25f,
                minimumStableEdgeLength * 0.002f);
            if (!ArePlaneCutBoundsEquivalent(
                    polygonBounds,
                    soupBounds,
                    boundsTolerance))
            {
                result.PreviewBoundsFailureCount++;
            }

            double polygonVolume =
                CalculatePlaneCutPolyhedronVolume(auditedFaces);
            double soupVolume = Math.Abs(signedVolume);
            double volumeTolerance = Math.Max(
                0.000000001,
                polygonVolume * 0.001);
            if (polygonVolume <= 0.000000001 ||
                Math.Abs(polygonVolume - soupVolume) > volumeTolerance)
            {
                result.PreviewVolumeFailureCount++;
            }

            bool valid = result.PreviewTriangleCount > 0 &&
                result.PreviewDegenerateTriangleCount == 0 &&
                result.PreviewOpenEdgeCount == 0 &&
                result.PreviewNonManifoldEdgeCount == 0 &&
                result.PreviewWindingFailureCount == 0 &&
                result.PreviewBoundsFailureCount == 0 &&
                result.PreviewVolumeFailureCount == 0;
            result.PreviewGeometryValid = valid ? 1 : 0;
            if (!valid)
            {
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    "the exact plane-cut preview triangle soup failed validation");
            }
        }

        private static Vector3 CalculatePlaneCutFaceVertexCentre(
            List<PolygonFace> faces)
        {
            Dictionary<VertexKey, Vector3> unique =
                new Dictionary<VertexKey, Vector3>();
            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                List<Vector3> vertices = faces[faceIndex].Vertices;
                for (int vertexIndex = 0;
                     vertexIndex < vertices.Count;
                     vertexIndex++)
                {
                    VertexKey key = new VertexKey(vertices[vertexIndex]);
                    if (!unique.ContainsKey(key))
                    {
                        unique.Add(key, vertices[vertexIndex]);
                    }
                }
            }

            Vector3 centre = Vector3.zero;
            foreach (Vector3 position in unique.Values)
            {
                centre += position;
            }
            return unique.Count > 0
                ? centre / unique.Count
                : Vector3.zero;
        }

        private static void CountPlaneCutTriangleEdge(
            Dictionary<EdgeKey, int> edgeUses,
            Vector3 start,
            Vector3 end)
        {
            EdgeKey key = new EdgeKey(start, end);
            edgeUses.TryGetValue(key, out int useCount);
            edgeUses[key] = useCount + 1;
        }

        private static bool ArePlaneCutBoundsEquivalent(
            Bounds left,
            Bounds right,
            float tolerance)
        {
            return Mathf.Abs(left.min.x - right.min.x) <= tolerance &&
                Mathf.Abs(left.min.y - right.min.y) <= tolerance &&
                Mathf.Abs(left.min.z - right.min.z) <= tolerance &&
                Mathf.Abs(left.max.x - right.max.x) <= tolerance &&
                Mathf.Abs(left.max.y - right.max.y) <= tolerance &&
                Mathf.Abs(left.max.z - right.max.z) <= tolerance;
        }

        private static List<PolygonFace> ClonePolygonFacesForPlaneCutAudit(
            List<PolygonFace> sourceFaces,
            bool assignSourceFaceProvenance = false)
        {
            List<PolygonFace> cloned =
                new List<PolygonFace>(sourceFaces.Count);
            for (int faceIndex = 0;
                 faceIndex < sourceFaces.Count;
                 faceIndex++)
            {
                PolygonFace source = sourceFaces[faceIndex];
                PolygonFaceProvenanceKind provenanceKind =
                    source.ProvenanceKind;
                int provenanceIndex = source.ProvenanceIndex;
                if (assignSourceFaceProvenance &&
                    provenanceKind == PolygonFaceProvenanceKind.None)
                {
                    provenanceKind =
                        PolygonFaceProvenanceKind.SourceFace;
                    provenanceIndex = faceIndex;
                }
                cloned.Add(new PolygonFace(
                    new List<Vector3>(source.Vertices),
                    source.Normal,
                    source.Feature,
                    source.FeatureStrength,
                    provenanceKind,
                    provenanceIndex));
            }
            return cloned;
        }

        private static int ConformPlaneCutFaceBoundaries(
            List<PolygonFace> faces,
            float minimumStableEdgeLength)
        {
            if (faces == null || faces.Count == 0)
            {
                return 0;
            }

            WeldSharedVertices(faces);
            Dictionary<VertexKey, Vector3> uniqueVertices =
                new Dictionary<VertexKey, Vector3>();
            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                List<Vector3> vertices = faces[faceIndex].Vertices;
                for (int vertexIndex = 0;
                     vertexIndex < vertices.Count;
                     vertexIndex++)
                {
                    VertexKey key = new VertexKey(vertices[vertexIndex]);
                    if (!uniqueVertices.ContainsKey(key))
                    {
                        uniqueVertices.Add(key, vertices[vertexIndex]);
                    }
                }
            }

            float tolerance = Mathf.Max(
                PointMergeDistance * 8f,
                minimumStableEdgeLength * 0.002f);
            float toleranceSqr = tolerance * tolerance;
            int inserted = 0;

            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                List<Vector3> source = faces[faceIndex].Vertices;
                if (source == null || source.Count < 3)
                {
                    continue;
                }

                List<Vector3> conformed =
                    new List<Vector3>(source.Count + 4);
                for (int edgeIndex = 0;
                     edgeIndex < source.Count;
                     edgeIndex++)
                {
                    Vector3 start = source[edgeIndex];
                    Vector3 end = source[(edgeIndex + 1) % source.Count];
                    AddPointIfDifferent(conformed, start);

                    Vector3 segment = end - start;
                    float lengthSqr = segment.sqrMagnitude;
                    if (lengthSqr <= MinimumEdgeLengthSqr)
                    {
                        continue;
                    }

                    float length = Mathf.Sqrt(lengthSqr);
                    float endpointParameter = Mathf.Min(
                        0.25f,
                        tolerance / length);
                    VertexKey startKey = new VertexKey(start);
                    VertexKey endKey = new VertexKey(end);
                    List<PlaneCutBoundarySplit> splits =
                        new List<PlaneCutBoundarySplit>();

                    foreach (KeyValuePair<VertexKey, Vector3> entry
                        in uniqueVertices)
                    {
                        if (entry.Key.Equals(startKey) ||
                            entry.Key.Equals(endKey))
                        {
                            continue;
                        }

                        float parameter = Vector3.Dot(
                            entry.Value - start,
                            segment) / lengthSqr;
                        if (parameter <= endpointParameter ||
                            parameter >= 1f - endpointParameter)
                        {
                            continue;
                        }

                        Vector3 closest =
                            start + segment * parameter;
                        if ((entry.Value - closest).sqrMagnitude >
                            toleranceSqr)
                        {
                            continue;
                        }

                        splits.Add(new PlaneCutBoundarySplit(
                            parameter,
                            entry.Value));
                    }

                    splits.Sort((left, right) =>
                        left.Parameter.CompareTo(right.Parameter));
                    for (int splitIndex = 0;
                         splitIndex < splits.Count;
                         splitIndex++)
                    {
                        int beforeCount = conformed.Count;
                        AddPointIfDifferent(
                            conformed,
                            splits[splitIndex].Position);
                        if (conformed.Count > beforeCount)
                        {
                            inserted++;
                        }
                    }
                }

                RemoveClosingDuplicate(conformed);
                if (conformed.Count >= 3)
                {
                    source.Clear();
                    source.AddRange(conformed);
                }
            }

            WeldSharedVertices(faces);
            return inserted;
        }

        private static int RepairPlaneCutNumericalSeams(
            List<PolygonFace> faces,
            float minimumStableEdgeLength,
            out int touchedFaceCount)
        {
            touchedFaceCount = 0;
            EdgeWearTopologyStats before = AuditEdgeWearTopology(
                faces,
                minimumStableEdgeLength);
            if (before.OpenEdgeCount == 0)
            {
                return 0;
            }

            List<PlaneCutOpenEdgeRecord> openEdges =
                CollectPlaneCutOpenEdges(faces);
            if (openEdges.Count != before.OpenEdgeCount)
            {
                return 0;
            }

            float tolerance = Mathf.Clamp(
                minimumStableEdgeLength * 0.0001f,
                PointMergeDistance * 4f,
                PlaneEpsilon * 0.5f);
            float toleranceSqr = tolerance * tolerance;
            List<int>[] counterparts =
                new List<int>[openEdges.Count];
            for (int edgeIndex = 0;
                 edgeIndex < openEdges.Count;
                 edgeIndex++)
            {
                counterparts[edgeIndex] = new List<int>();
            }

            for (int leftIndex = 0;
                 leftIndex < openEdges.Count;
                 leftIndex++)
            {
                for (int rightIndex = leftIndex + 1;
                     rightIndex < openEdges.Count;
                     rightIndex++)
                {
                    if (!ArePlaneCutOpenEdgesCounterparts(
                            openEdges[leftIndex],
                            openEdges[rightIndex],
                            toleranceSqr))
                    {
                        continue;
                    }

                    counterparts[leftIndex].Add(rightIndex);
                    counterparts[rightIndex].Add(leftIndex);
                }
            }

            List<(int Left, int Right)> pairs =
                new List<(int Left, int Right)>();
            for (int edgeIndex = 0;
                 edgeIndex < openEdges.Count;
                 edgeIndex++)
            {
                if (counterparts[edgeIndex].Count != 1)
                {
                    continue;
                }

                int counterpartIndex = counterparts[edgeIndex][0];
                if (counterpartIndex <= edgeIndex ||
                    counterparts[counterpartIndex].Count != 1 ||
                    counterparts[counterpartIndex][0] != edgeIndex)
                {
                    continue;
                }

                pairs.Add((edgeIndex, counterpartIndex));
            }

            if (pairs.Count == 0)
            {
                return 0;
            }

            Dictionary<VertexKey, Vector3> snapTargets =
                new Dictionary<VertexKey, Vector3>();
            for (int pairIndex = 0; pairIndex < pairs.Count; pairIndex++)
            {
                PlaneCutOpenEdgeRecord left =
                    openEdges[pairs[pairIndex].Left];
                PlaneCutOpenEdgeRecord right =
                    openEdges[pairs[pairIndex].Right];
                PolygonFace leftFace = faces[left.FaceIndex];
                PolygonFace rightFace = faces[right.FaceIndex];

                Vector3 averageStart =
                    (left.Start + right.End) * 0.5f;
                Vector3 averageEnd =
                    (left.End + right.Start) * 0.5f;
                if (!TryProjectPlaneCutSeamPoint(
                        averageStart,
                        leftFace,
                        rightFace,
                        out Vector3 canonicalStart) ||
                    !TryProjectPlaneCutSeamPoint(
                        averageEnd,
                        leftFace,
                        rightFace,
                        out Vector3 canonicalEnd) ||
                    (canonicalStart - left.Start).sqrMagnitude >
                        toleranceSqr * 4f ||
                    (canonicalStart - right.End).sqrMagnitude >
                        toleranceSqr * 4f ||
                    (canonicalEnd - left.End).sqrMagnitude >
                        toleranceSqr * 4f ||
                    (canonicalEnd - right.Start).sqrMagnitude >
                        toleranceSqr * 4f ||
                    (canonicalEnd - canonicalStart).sqrMagnitude <=
                        MinimumEdgeLengthSqr ||
                    !TryAddPlaneCutSnapTarget(
                        snapTargets,
                        left.StartKey,
                        canonicalStart,
                        toleranceSqr) ||
                    !TryAddPlaneCutSnapTarget(
                        snapTargets,
                        right.EndKey,
                        canonicalStart,
                        toleranceSqr) ||
                    !TryAddPlaneCutSnapTarget(
                        snapTargets,
                        left.EndKey,
                        canonicalEnd,
                        toleranceSqr) ||
                    !TryAddPlaneCutSnapTarget(
                        snapTargets,
                        right.StartKey,
                        canonicalEnd,
                        toleranceSqr))
                {
                    return 0;
                }
            }

            List<PolygonFace> backup =
                ClonePolygonFacesForPlaneCutAudit(faces);
            HashSet<int> touchedFaces = new HashSet<int>();
            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                List<Vector3> vertices = faces[faceIndex].Vertices;
                for (int vertexIndex = 0;
                     vertexIndex < vertices.Count;
                     vertexIndex++)
                {
                    VertexKey key = new VertexKey(vertices[vertexIndex]);
                    if (snapTargets.TryGetValue(key, out Vector3 target))
                    {
                        if ((vertices[vertexIndex] - target).sqrMagnitude >
                            MinimumEdgeLengthSqr)
                        {
                            touchedFaces.Add(faceIndex);
                        }
                        vertices[vertexIndex] = target;
                    }
                }
            }

            WeldSharedVertices(faces);
            if (!ArePlaneCutFacesPlanarAfterRepair(
                    faces,
                    backup,
                    tolerance))
            {
                faces.Clear();
                faces.AddRange(backup);
                return 0;
            }

            EdgeWearTopologyStats after = AuditEdgeWearTopology(
                faces,
                minimumStableEdgeLength);
            int expectedOpenEdges =
                before.OpenEdgeCount - pairs.Count * 2;
            if (after.OpenEdgeCount != expectedOpenEdges ||
                after.NonManifoldEdgeCount >
                    before.NonManifoldEdgeCount ||
                after.TJunctionCount > before.TJunctionCount)
            {
                faces.Clear();
                faces.AddRange(backup);
                return 0;
            }

            touchedFaceCount = touchedFaces.Count;
            return pairs.Count;
        }

        private static bool TryProjectPlaneCutSeamPoint(
            Vector3 target,
            PolygonFace leftFace,
            PolygonFace rightFace,
            out Vector3 projected)
        {
            projected = target;
            if (leftFace == null ||
                rightFace == null ||
                leftFace.Vertices.Count == 0 ||
                rightFace.Vertices.Count == 0)
            {
                return false;
            }

            Vector3 leftNormal = leftFace.Normal;
            Vector3 rightNormal = rightFace.Normal;
            float leftDistance = Vector3.Dot(
                leftNormal,
                leftFace.Vertices[0]);
            float rightDistance = Vector3.Dot(
                rightNormal,
                rightFace.Vertices[0]);
            float normalDot = Vector3.Dot(
                leftNormal,
                rightNormal);
            float determinant = 1f - normalDot * normalDot;

            if (determinant <= 0.000001f)
            {
                projected = target -
                    leftNormal *
                    (Vector3.Dot(leftNormal, target) - leftDistance);
                return IsFinite(projected) &&
                    Mathf.Abs(
                        Vector3.Dot(rightNormal, projected) -
                        rightDistance) <= PlaneEpsilon;
            }

            float leftError =
                Vector3.Dot(leftNormal, target) - leftDistance;
            float rightError =
                Vector3.Dot(rightNormal, target) - rightDistance;
            float leftLambda =
                (leftError - normalDot * rightError) /
                determinant;
            float rightLambda =
                (rightError - normalDot * leftError) /
                determinant;
            projected = target -
                leftNormal * leftLambda -
                rightNormal * rightLambda;
            return IsFinite(projected);
        }

        private static bool ArePlaneCutFacesPlanarAfterRepair(
            List<PolygonFace> faces,
            List<PolygonFace> originalFaces,
            float tolerance)
        {
            if (faces.Count != originalFaces.Count)
            {
                return false;
            }

            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                if (originalFaces[faceIndex].Vertices.Count == 0)
                {
                    return false;
                }

                Vector3 normal = originalFaces[faceIndex].Normal;
                float distance = Vector3.Dot(
                    normal,
                    originalFaces[faceIndex].Vertices[0]);
                List<Vector3> vertices = faces[faceIndex].Vertices;
                for (int vertexIndex = 0;
                     vertexIndex < vertices.Count;
                     vertexIndex++)
                {
                    if (Mathf.Abs(
                            Vector3.Dot(normal, vertices[vertexIndex]) -
                            distance) > tolerance)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static List<PlaneCutOpenEdgeRecord>
            CollectPlaneCutOpenEdges(List<PolygonFace> faces)
        {
            Dictionary<EdgeKey, int> uses =
                new Dictionary<EdgeKey, int>();
            List<PlaneCutOpenEdgeRecord> records =
                new List<PlaneCutOpenEdgeRecord>();
            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                List<Vector3> vertices = faces[faceIndex].Vertices;
                for (int startIndex = 0;
                     startIndex < vertices.Count;
                     startIndex++)
                {
                    int endIndex = (startIndex + 1) % vertices.Count;
                    Vector3 start = vertices[startIndex];
                    Vector3 end = vertices[endIndex];
                    if (AreSamePoint(start, end))
                    {
                        continue;
                    }

                    PlaneCutOpenEdgeRecord record =
                        new PlaneCutOpenEdgeRecord(
                            faceIndex,
                            start,
                            end);
                    uses.TryGetValue(record.EdgeKey, out int useCount);
                    uses[record.EdgeKey] = useCount + 1;
                    records.Add(record);
                }
            }

            List<PlaneCutOpenEdgeRecord> openEdges =
                new List<PlaneCutOpenEdgeRecord>();
            for (int recordIndex = 0;
                 recordIndex < records.Count;
                 recordIndex++)
            {
                PlaneCutOpenEdgeRecord record = records[recordIndex];
                if (uses[record.EdgeKey] == 1)
                {
                    openEdges.Add(record);
                }
            }

            return openEdges;
        }

        private static bool ArePlaneCutOpenEdgesCounterparts(
            PlaneCutOpenEdgeRecord left,
            PlaneCutOpenEdgeRecord right,
            float toleranceSqr)
        {
            if (left.FaceIndex == right.FaceIndex ||
                (left.Start - right.End).sqrMagnitude > toleranceSqr ||
                (left.End - right.Start).sqrMagnitude > toleranceSqr)
            {
                return false;
            }

            Vector3 leftDirection = left.End - left.Start;
            Vector3 rightDirection = right.End - right.Start;
            float leftLength = leftDirection.magnitude;
            float rightLength = rightDirection.magnitude;
            if (leftLength <= PointMergeDistance ||
                rightLength <= PointMergeDistance ||
                Mathf.Abs(leftLength - rightLength) >
                    Mathf.Sqrt(toleranceSqr) * 2f)
            {
                return false;
            }

            return Vector3.Dot(
                    leftDirection / leftLength,
                    rightDirection / rightLength) <= -0.999f;
        }

        private static bool TryAddPlaneCutSnapTarget(
            Dictionary<VertexKey, Vector3> snapTargets,
            VertexKey key,
            Vector3 target,
            float toleranceSqr)
        {
            if (snapTargets.TryGetValue(key, out Vector3 existing))
            {
                return (existing - target).sqrMagnitude <= toleranceSqr;
            }

            snapTargets.Add(key, target);
            return true;
        }

        private static bool IsPlaneCutCandidateAlreadySatisfied(
            List<PolygonFace> faces,
            PlaneCutBevelCandidate candidate)
        {
            return IsPlaneCutHalfSpaceSatisfied(
                faces,
                candidate);
        }

        private static bool IsPlaneCutCandidateRedundant(
            List<PolygonFace> faces,
            PlaneCutBevelCandidate candidate)
        {
            return IsPlaneCutHalfSpaceSatisfied(
                faces,
                candidate);
        }

        private static bool IsPlaneCutHalfSpaceSatisfied(
            List<PolygonFace> faces,
            PlaneCutBevelCandidate candidate)
        {
            float planeTolerance =
                ResolvePlaneCutRedundancyTolerance(candidate);
            bool foundVertex = false;
            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                List<Vector3> vertices = faces[faceIndex].Vertices;
                for (int vertexIndex = 0;
                     vertexIndex < vertices.Count;
                     vertexIndex++)
                {
                    foundVertex = true;
                    if (candidate.Plane.SignedDistance(
                            vertices[vertexIndex]) > planeTolerance)
                    {
                        return false;
                    }
                }
            }

            return foundVertex;
        }

        private static float ResolvePlaneCutRedundancyTolerance(
            PlaneCutBevelCandidate candidate)
        {
            float numericalTolerance = Mathf.Max(
                PointMergeDistance * 0.25f,
                candidate.ClipEpsilon * 1.25f);
            return Mathf.Min(
                numericalTolerance,
                candidate.MinimumSourceRemoval * 0.5f);
        }

        private static int CountMatchingPlaneCutCaps(
            List<PolygonFace> faces,
            PlaneCutBevelCandidate candidate)
        {
            int count = 0;
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face.Feature != PolygonFaceFeature.ConvexEdgeWear ||
                    Vector3.Dot(face.Normal, candidate.Plane.Normal) < 0.999f)
                {
                    continue;
                }

                bool onPlane = true;
                for (int vertexIndex = 0;
                     vertexIndex < face.Vertices.Count;
                     vertexIndex++)
                {
                    if (Mathf.Abs(candidate.Plane.SignedDistance(
                            face.Vertices[vertexIndex])) >
                        candidate.PlaneTolerance)
                    {
                        onPlane = false;
                        break;
                    }
                }
                if (onPlane)
                {
                    count++;
                }
            }
            return count;
        }

        private static void AuditPlaneCutOpenEdgeFailures(
            List<PolygonFace> faces,
            ChamferTopologyContext context,
            List<PlaneCutBevelCandidate> candidates,
            float minimumStableEdgeLength,
            ref PlaneCutBevelAuditResult result)
        {
            if (result.OpenEdgeFailures == null)
            {
                result.OpenEdgeFailures =
                    new List<PlaneCutOpenEdgeFailureRecord>();
            }
            else
            {
                result.OpenEdgeFailures.Clear();
            }

            if (faces == null || context == null)
            {
                return;
            }

            List<PlaneCutOpenEdgeRecord> openEdges =
                CollectPlaneCutOpenEdges(faces);
            float nearMatchTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                minimumStableEdgeLength * 0.002f);
            for (int recordIndex = 0;
                 recordIndex < openEdges.Count;
                 recordIndex++)
            {
                PlaneCutOpenEdgeRecord open = openEdges[recordIndex];
                PolygonFace owner = open.FaceIndex >= 0 &&
                        open.FaceIndex < faces.Count
                    ? faces[open.FaceIndex]
                    : null;
                FindNearestPlaneCutSourceVertex(
                    context,
                    open.Start,
                    open.End,
                    out int sourceVertex,
                    out Vector3 sourcePosition,
                    out float sourceDistance);
                List<int> incidentEdges =
                    CollectPlaneCutIncidentCandidateEdges(
                        candidates,
                        sourceVertex);
                int junctionExpected = incidentEdges.Count >= 2 ? 1 : 0;
                int junctionFaceCount = CountPlaneCutJunctionFaces(
                    faces,
                    sourceVertex);
                FindNearestPlaneCutBoundarySegment(
                    faces,
                    open,
                    out int nearestFaceIndex,
                    out PolygonFaceProvenanceKind nearestKind,
                    out int nearestProvenanceIndex,
                    out Vector3 nearestStart,
                    out Vector3 nearestEnd,
                    out float nearestDistance);

                string expectedNeighbour = junctionExpected == 1
                    ? "Junction(vertex:" + sourceVertex + ")"
                    : owner != null &&
                        owner.ProvenanceKind ==
                            PolygonFaceProvenanceKind.SourceFace &&
                        incidentEdges.Count > 0
                        ? "Bevel(edge:" + incidentEdges[0] + ")"
                        : "BoundaryMate";
                string cause;
                if (junctionExpected == 1 && junctionFaceCount == 0)
                {
                    cause = "missing-junction";
                }
                else if (nearestDistance <= nearMatchTolerance * 4f)
                {
                    cause = "near-miss-boundary";
                }
                else
                {
                    cause = "unmatched-boundary";
                }

                result.OpenEdgeFailures.Add(
                    new PlaneCutOpenEdgeFailureRecord(
                        recordIndex,
                        open.FaceIndex,
                        owner == null
                            ? PolygonFaceProvenanceKind.None
                            : owner.ProvenanceKind,
                        owner == null ? -1 : owner.ProvenanceIndex,
                        open.Start,
                        open.End,
                        Vector3.Distance(open.Start, open.End),
                        sourceVertex,
                        sourcePosition,
                        sourceDistance,
                        FormatPlaneCutEdgeIndexEvidence(incidentEdges),
                        junctionExpected,
                        junctionFaceCount,
                        expectedNeighbour,
                        nearestFaceIndex,
                        nearestKind,
                        nearestProvenanceIndex,
                        nearestStart,
                        nearestEnd,
                        nearestDistance,
                        string.IsNullOrEmpty(result.FirstOpenEdgeStage)
                            ? "FinalCertification"
                            : result.FirstOpenEdgeStage,
                        cause));
            }
        }

        private static List<int> CollectPlaneCutIncidentCandidateEdges(
            List<PlaneCutBevelCandidate> candidates,
            int vertexIndex)
        {
            List<int> edges = new List<int>();
            if (candidates == null || vertexIndex < 0)
            {
                return edges;
            }
            for (int candidateIndex = 0;
                 candidateIndex < candidates.Count;
                 candidateIndex++)
            {
                PlaneCutBevelCandidate candidate =
                    candidates[candidateIndex];
                if (candidate.VertexA == vertexIndex ||
                    candidate.VertexB == vertexIndex)
                {
                    edges.Add(candidate.SourceEdgeIndex);
                }
            }
            edges.Sort();
            return edges;
        }

        private static int CountPlaneCutJunctionFaces(
            List<PolygonFace> faces,
            int vertexIndex)
        {
            if (faces == null || vertexIndex < 0)
            {
                return 0;
            }
            int count = 0;
            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face != null &&
                    face.ProvenanceKind ==
                        PolygonFaceProvenanceKind.VertexJunctionPlane &&
                    face.ProvenanceIndex == vertexIndex)
                {
                    count++;
                }
            }
            return count;
        }

        private static void FindNearestPlaneCutSourceVertex(
            ChamferTopologyContext context,
            Vector3 start,
            Vector3 end,
            out int vertexIndex,
            out Vector3 sourcePosition,
            out float distance)
        {
            vertexIndex = -1;
            sourcePosition = Vector3.zero;
            distance = float.PositiveInfinity;
            if (context == null || context.Graph == null)
            {
                return;
            }
            for (int index = 0;
                 index < context.Graph.Vertices.Count;
                 index++)
            {
                Vector3 candidate =
                    context.Graph.Vertices[index].Position;
                float candidateDistance = Mathf.Min(
                    Vector3.Distance(start, candidate),
                    Vector3.Distance(end, candidate));
                if (candidateDistance >= distance)
                {
                    continue;
                }
                vertexIndex = index;
                sourcePosition = candidate;
                distance = candidateDistance;
            }
            if (float.IsPositiveInfinity(distance))
            {
                distance = 0f;
            }
        }

        private static void FindNearestPlaneCutBoundarySegment(
            List<PolygonFace> faces,
            PlaneCutOpenEdgeRecord open,
            out int nearestFaceIndex,
            out PolygonFaceProvenanceKind nearestKind,
            out int nearestProvenanceIndex,
            out Vector3 nearestStart,
            out Vector3 nearestEnd,
            out float nearestDistance)
        {
            nearestFaceIndex = -1;
            nearestKind = PolygonFaceProvenanceKind.None;
            nearestProvenanceIndex = -1;
            nearestStart = Vector3.zero;
            nearestEnd = Vector3.zero;
            nearestDistance = float.PositiveInfinity;
            if (faces == null)
            {
                return;
            }

            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face == null || face.Vertices == null ||
                    face.Vertices.Count < 2)
                {
                    continue;
                }
                for (int edgeIndex = 0;
                     edgeIndex < face.Vertices.Count;
                     edgeIndex++)
                {
                    Vector3 start = face.Vertices[edgeIndex];
                    Vector3 end = face.Vertices[
                        (edgeIndex + 1) % face.Vertices.Count];
                    EdgeKey key = new EdgeKey(start, end);
                    if (faceIndex == open.FaceIndex &&
                        key.Equals(open.EdgeKey))
                    {
                        continue;
                    }
                    float reversedDistance = Mathf.Max(
                        Vector3.Distance(open.Start, end),
                        Vector3.Distance(open.End, start));
                    if (reversedDistance >= nearestDistance)
                    {
                        continue;
                    }
                    nearestDistance = reversedDistance;
                    nearestFaceIndex = faceIndex;
                    nearestKind = face.ProvenanceKind;
                    nearestProvenanceIndex = face.ProvenanceIndex;
                    nearestStart = start;
                    nearestEnd = end;
                }
            }
            if (float.IsPositiveInfinity(nearestDistance))
            {
                nearestDistance = 0f;
            }
        }

        private static string BuildPlaneCutFaceVertexResidualEvidence(
            PolygonFace face)
        {
            if (face == null || face.Vertices == null ||
                face.Vertices.Count == 0)
            {
                return "none";
            }
            float planeDistance = Vector3.Dot(
                face.Normal,
                face.Vertices[0]);
            StringBuilder builder = new StringBuilder();
            for (int vertexIndex = 0;
                 vertexIndex < face.Vertices.Count;
                 vertexIndex++)
            {
                if (vertexIndex > 0)
                {
                    builder.Append('|');
                }
                Vector3 vertex = face.Vertices[vertexIndex];
                builder.Append(vertexIndex);
                builder.Append('@');
                builder.Append('(');
                builder.Append(vertex.x.ToString("G9"));
                builder.Append('/');
                builder.Append(vertex.y.ToString("G9"));
                builder.Append('/');
                builder.Append(vertex.z.ToString("G9"));
                builder.Append(")[r=");
                builder.Append((Vector3.Dot(face.Normal, vertex) -
                    planeDistance).ToString("G9"));
                builder.Append(']');
            }
            return builder.ToString();
        }

        private static void AuditPlaneCutFaceQuality(
            List<PolygonFace> faces,
            List<PlaneCutVertexJunctionCandidate> junctions,
            float minimumStableEdgeLength,
            ref PlaneCutBevelAuditResult result)
        {
            const float minimumJunctionCompactness = 0.06f;
            const float maximumJunctionAspectRatio = 12f;
            float planarityTolerance =
                ResolvePlaneCutPlanarityTolerance(
                    minimumStableEdgeLength);
            result.FaceQualityPlanarityTolerance = planarityTolerance;
            result.FaceQualityNormalSpreadToleranceDegrees =
                PlaneCutMaximumNormalSpreadDegrees;
            if (result.FaceQualityFailures == null)
            {
                result.FaceQualityFailures =
                    new List<PlaneCutFaceQualityFailureRecord>();
            }
            else
            {
                result.FaceQualityFailures.Clear();
            }

            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face == null ||
                    face.Feature != PolygonFaceFeature.ConvexEdgeWear ||
                    face.Vertices == null ||
                    face.Vertices.Count < 3)
                {
                    continue;
                }

                result.FaceQualityFaceCount++;
                MeasurePlaneCutFacePlanarityDetailed(
                    face,
                    out float maximumDeviation,
                    out int offendingVertexIndex,
                    out Vector3 offendingVertexPosition,
                    out float offendingSignedResidual,
                    out float maximumNormalSpread,
                    out int offendingSegmentIndex,
                    out Vector3 offendingTriangleNormal,
                    out Vector3 measuredNormal,
                    out float area,
                    out float minimumEdgeLength);
                result.FaceQualityMaxPlaneDeviation = Mathf.Max(
                    result.FaceQualityMaxPlaneDeviation,
                    maximumDeviation);
                result.FaceQualityMaxNormalSpreadDegrees = Mathf.Max(
                    result.FaceQualityMaxNormalSpreadDegrees,
                    maximumNormalSpread);
                bool deviationFailure =
                    maximumDeviation > planarityTolerance;
                bool spreadFailure =
                    maximumNormalSpread >
                        PlaneCutMaximumNormalSpreadDegrees;
                if (!deviationFailure && !spreadFailure)
                {
                    continue;
                }

                result.FaceQualityNonPlanarCount++;
                string identity = BuildPlaneCutFaceIdentity(
                    face,
                    faceIndex);
                string firstFailureStage = string.Empty;
                result.FaceFirstNonPlanarStageByIdentity?.TryGetValue(
                    identity,
                    out firstFailureStage);
                bool conformityTouched =
                    result.BoundaryConformityTouchedFaces != null &&
                    result.BoundaryConformityTouchedFaces.Contains(identity);
                bool seamTouched =
                    result.SeamRepairTouchedFaces != null &&
                    result.SeamRepairTouchedFaces.Contains(identity);
                float seamMovement = 0f;
                result.SeamRepairMaximumMovementByIdentity?.TryGetValue(
                    identity,
                    out seamMovement);
                string cause = deviationFailure && spreadFailure
                    ? "plane-residual+normal-spread"
                    : deviationFailure
                        ? "plane-residual"
                        : "normal-spread";
                result.FaceQualityFailures.Add(
                    new PlaneCutFaceQualityFailureRecord(
                        faceIndex,
                        face.ProvenanceKind,
                        face.ProvenanceIndex,
                        face.Vertices.Count,
                        face.Normal,
                        measuredNormal,
                        Vector3.Dot(face.Normal, face.Vertices[0]),
                        maximumDeviation,
                        planarityTolerance,
                        offendingVertexIndex,
                        offendingVertexPosition,
                        offendingSignedResidual,
                        maximumNormalSpread,
                        PlaneCutMaximumNormalSpreadDegrees,
                        offendingSegmentIndex,
                        offendingTriangleNormal,
                        area,
                        minimumEdgeLength,
                        string.IsNullOrEmpty(firstFailureStage)
                            ? "FinalCertification"
                            : firstFailureStage,
                        conformityTouched ? 1 : 0,
                        seamTouched ? 1 : 0,
                        seamMovement,
                        BuildPlaneCutFaceVertexResidualEvidence(face),
                        cause));
            }

            result.FaceQualityMinimumJunctionCompactness =
                junctions.Count > 0
                    ? float.PositiveInfinity
                    : 0f;
            for (int junctionIndex = 0;
                 junctionIndex < junctions.Count;
                 junctionIndex++)
            {
                PlaneCutVertexJunctionCandidate junction =
                    junctions[junctionIndex];
                if (!TryFindSinglePlaneCutCap(
                        faces,
                        junction.Plane,
                        junction.PlaneTolerance,
                        out PolygonFace cap))
                {
                    continue;
                }

                float area = CalculatePolygonArea(cap.Vertices);
                float perimeter = 0f;
                for (int vertexIndex = 0;
                     vertexIndex < cap.Vertices.Count;
                     vertexIndex++)
                {
                    perimeter += Vector3.Distance(
                        cap.Vertices[vertexIndex],
                        cap.Vertices[
                            (vertexIndex + 1) %
                            cap.Vertices.Count]);
                }
                float compactness =
                    4f * Mathf.PI * area /
                    Mathf.Max(
                        perimeter * perimeter,
                        0.0000000001f);
                float aspectRatio =
                    CalculatePlaneCutPolygonAspectRatio(
                        cap.Vertices,
                        area);
                result.FaceQualityMinimumJunctionCompactness =
                    Mathf.Min(
                        result.FaceQualityMinimumJunctionCompactness,
                        compactness);
                result.FaceQualityMaximumJunctionAspectRatio =
                    Mathf.Max(
                        result.FaceQualityMaximumJunctionAspectRatio,
                        aspectRatio);
                result.FaceQualityWorstVertexCount = Mathf.Max(
                    result.FaceQualityWorstVertexCount,
                    cap.Vertices.Count);
                if (compactness < minimumJunctionCompactness ||
                    aspectRatio > maximumJunctionAspectRatio)
                {
                    result.FaceQualityElongatedJunctionCount++;
                }
            }

            if (float.IsPositiveInfinity(
                    result.FaceQualityMinimumJunctionCompactness))
            {
                result.FaceQualityMinimumJunctionCompactness = 0f;
            }
        }

        private static void MeasurePlaneCutFacePlanarity(
            PolygonFace face,
            out float maximumDeviation,
            out float maximumNormalSpread)
        {
            MeasurePlaneCutFacePlanarityDetailed(
                face,
                out maximumDeviation,
                out _,
                out _,
                out _,
                out maximumNormalSpread,
                out _,
                out _,
                out _,
                out _,
                out _);
        }

        private static void MeasurePlaneCutFacePlanarityDetailed(
            PolygonFace face,
            out float maximumDeviation,
            out int offendingVertexIndex,
            out Vector3 offendingVertexPosition,
            out float offendingSignedResidual,
            out float maximumNormalSpread,
            out int offendingSegmentIndex,
            out Vector3 offendingTriangleNormal,
            out Vector3 measuredNormal,
            out float area,
            out float minimumEdgeLength)
        {
            maximumDeviation = 0f;
            offendingVertexIndex = -1;
            offendingVertexPosition = Vector3.zero;
            offendingSignedResidual = 0f;
            maximumNormalSpread = 0f;
            offendingSegmentIndex = -1;
            offendingTriangleNormal = Vector3.zero;
            measuredNormal = Vector3.zero;
            area = 0f;
            minimumEdgeLength = 0f;
            if (face == null || face.Vertices == null ||
                face.Vertices.Count < 3)
            {
                return;
            }

            measuredNormal = CalculatePolygonNormal(face.Vertices);
            area = CalculatePolygonArea(face.Vertices);
            minimumEdgeLength = float.PositiveInfinity;
            float planeDistance = Vector3.Dot(
                face.Normal,
                face.Vertices[0]);
            Vector3 centre = CalculateAverage(face.Vertices);
            for (int vertexIndex = 0;
                 vertexIndex < face.Vertices.Count;
                 vertexIndex++)
            {
                Vector3 start = face.Vertices[vertexIndex];
                Vector3 end = face.Vertices[
                    (vertexIndex + 1) % face.Vertices.Count];
                float signedResidual =
                    Vector3.Dot(face.Normal, start) - planeDistance;
                float deviation = Mathf.Abs(signedResidual);
                if (deviation > maximumDeviation)
                {
                    maximumDeviation = deviation;
                    offendingVertexIndex = vertexIndex;
                    offendingVertexPosition = start;
                    offendingSignedResidual = signedResidual;
                }

                float edgeLength = Vector3.Distance(start, end);
                minimumEdgeLength = Mathf.Min(
                    minimumEdgeLength,
                    edgeLength);
                Vector3 triangleNormal = Vector3.Cross(
                    start - centre,
                    end - centre);
                if (triangleNormal.sqrMagnitude <=
                    MinimumEdgeLengthSqr)
                {
                    continue;
                }
                triangleNormal.Normalize();
                if (Vector3.Dot(triangleNormal, face.Normal) < 0f)
                {
                    triangleNormal = -triangleNormal;
                }
                float spread = Vector3.Angle(
                    triangleNormal,
                    face.Normal);
                if (spread > maximumNormalSpread)
                {
                    maximumNormalSpread = spread;
                    offendingSegmentIndex = vertexIndex;
                    offendingTriangleNormal = triangleNormal;
                }
            }

            if (float.IsPositiveInfinity(minimumEdgeLength))
            {
                minimumEdgeLength = 0f;
            }
        }

        private static int CountInvalidPlaneCutFaces(
            List<PolygonFace> faces,
            float minimumStableFaceArea)
        {
            int invalid = 0;
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face == null || face.Vertices == null ||
                    face.Vertices.Count < 3 || !IsFinite(face.Normal) ||
                    face.Normal.sqrMagnitude <= MinimumEdgeLengthSqr)
                {
                    invalid++;
                    continue;
                }

                bool finite = true;
                for (int vertexIndex = 0;
                     vertexIndex < face.Vertices.Count;
                     vertexIndex++)
                {
                    if (!IsFinite(face.Vertices[vertexIndex]))
                    {
                        finite = false;
                        break;
                    }
                }
                if (!finite ||
                    CalculatePolygonArea(face.Vertices) <=
                        minimumStableFaceArea)
                {
                    invalid++;
                    continue;
                }

                Vector3 measuredNormal =
                    CalculatePolygonNormal(face.Vertices);
                if (!IsFinite(measuredNormal) ||
                    measuredNormal.sqrMagnitude <= MinimumEdgeLengthSqr ||
                    Vector3.Dot(measuredNormal, face.Normal) <= 0f)
                {
                    invalid++;
                }
            }
            return invalid;
        }

        private static double CalculatePlaneCutPolyhedronVolume(
            List<PolygonFace> faces)
        {
            double signedVolume = 0.0;
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                List<Vector3> vertices = faces[faceIndex].Vertices;
                if (vertices == null || vertices.Count < 3)
                {
                    continue;
                }
                Vector3 anchor = vertices[0];
                for (int vertexIndex = 1;
                     vertexIndex < vertices.Count - 1;
                     vertexIndex++)
                {
                    Vector3 b = vertices[vertexIndex];
                    Vector3 c = vertices[vertexIndex + 1];
                    signedVolume += Vector3.Dot(
                        anchor,
                        Vector3.Cross(b, c)) / 6.0;
                }
            }
            return Math.Abs(signedVolume);
        }

        private static bool ArePlaneCutBoundsContained(
            Bounds source,
            Bounds result,
            float tolerance)
        {
            return result.min.x >= source.min.x - tolerance &&
                result.min.y >= source.min.y - tolerance &&
                result.min.z >= source.min.z - tolerance &&
                result.max.x <= source.max.x + tolerance &&
                result.max.y <= source.max.y + tolerance &&
                result.max.z <= source.max.z + tolerance;
        }

        private static void SetPlaneCutBevelDiagnostic(
            ref string target,
            string value)
        {
            if (string.IsNullOrEmpty(target) &&
                !string.IsNullOrEmpty(value))
            {
                target = value;
            }
        }

        #endregion
    }
}
