using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear types

        private static readonly bool EnableVerboseChamferDiagnostics = false;
        private static readonly Dictionary<int, string>
            LastChamferCompactSummaryByGeometry =
                new Dictionary<int, string>();
        private static readonly Dictionary<int, long>
            LastChamferCompactSummaryTicksByGeometry =
                new Dictionary<int, long>();
        private static readonly Dictionary<int, string>
            LastChamferCompactSummaryOriginByGeometry =
                new Dictionary<int, string>();

        private enum PolygonSurfaceTriangulationMode
        {
            None,
            BoundaryFan,
            GeneralTriangulation,
            CollinearReinsertion
        }

        private enum OneSurfaceTriangleCandidateFailure
        {
            None,
            NonFinite,
            Area,
            NormalAgreement
        }

        private readonly struct OneSurfaceTriangleIndex
        {
            public readonly int A;
            public readonly int B;
            public readonly int C;

            public OneSurfaceTriangleIndex(int a, int b, int c)
            {
                A = a;
                B = b;
                C = c;
            }
        }

        private readonly struct OneSurfaceTriangleCandidate
        {
            public readonly bool Valid;
            public readonly float Area;
            public readonly float NormalDot;
            public readonly float NormalDeviationDegrees;
            public readonly OneSurfaceTriangleCandidateFailure Failure;

            public OneSurfaceTriangleCandidate(
                bool valid,
                float area,
                float normalDot,
                float normalDeviationDegrees,
                OneSurfaceTriangleCandidateFailure failure)
            {
                Valid = valid;
                Area = area;
                NormalDot = normalDot;
                NormalDeviationDegrees = normalDeviationDegrees;
                Failure = failure;
            }
        }

        private struct OneSurfaceTriangulationState
        {
            public bool Evaluated;
            public bool Succeeded;
            public float MinimumArea;
            public float MinimumNormalDot;
            public int SplitIndex;
        }

        private struct OneSurfaceBoundaryFanAudit
        {
            public int AnchorsTested;
            public int StableAnchors;
            public int BestAnchorIndex;
            public int BestCertifiedTriangleCount;
            public float BestMinimumArea;
            public float BestMinimumNormalDot;
            public int RejectedTriangleA;
            public int RejectedTriangleB;
            public int RejectedTriangleC;
            public OneSurfaceTriangleCandidateFailure Rejection;
        }

        private struct OneSurfaceGeneralTriangulationAudit
        {
            public int ProjectionSucceeded;
            public int ProjectionSelfIntersection;
            public int StatesEvaluated;
            public int ValidTriangleCandidateCount;
            public int RejectedNonFiniteCount;
            public int RejectedAreaCount;
            public int RejectedNormalAgreementCount;
            public int RejectedDiagonalCount;
            public int CompleteSolutionFound;
            public float SelectedMinimumArea;
            public float SelectedMinimumNormalDot;
            public string SelectedTriangleEvidence;
            public string FailureReason;
        }

        private readonly struct OneSurfaceBoundaryRemoval
        {
            public readonly int PreviousIndex;
            public readonly int RemovedIndex;
            public readonly int NextIndex;

            public OneSurfaceBoundaryRemoval(
                int previousIndex,
                int removedIndex,
                int nextIndex)
            {
                PreviousIndex = previousIndex;
                RemovedIndex = removedIndex;
                NextIndex = nextIndex;
            }
        }

        private struct OneSurfaceCollinearCandidateAudit
        {
            public int PreviousIndex;
            public int CurrentIndex;
            public int NextIndex;
            public float LocalArea;
            public float LocalNormalDot;
            public OneSurfaceTriangleCandidateFailure LocalFailure;
            public float SegmentParameter;
            public float ProjectedDistance;
            public float ProjectedCross;
            public int Eligible;
            public string Blocker;
        }

        private struct OneSurfaceCollinearReinsertionAudit
        {
            public int Attempted;
            public int Succeeded;
            public int ProjectionSucceeded;
            public int ProjectionSelfIntersection;
            public int SimplificationAttempts;
            public int RemovedVertexCount;
            public int ReinsertionCount;
            public float SignedAreaBefore;
            public float SignedAreaAfter;
            public string BoundaryVertexEvidence;
            public string CandidateEvidence;
            public string RemovedIndexEvidence;
            public string RetainedIndexEvidence;
            public string SimplifiedTriangleEvidence;
            public string ReinsertionEvidence;
            public string FailureReason;
        }

        private sealed class EdgeWearEdgeAggregate
        {
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly List<int> FaceIndices = new List<int>(2);
            public bool CoincidentBoundarySeamReconciled;

            public EdgeWearEdgeAggregate(Vector3 start, Vector3 end)
            {
                Start = start;
                End = end;
            }

            public void AddFace(int faceIndex)
            {
                if (!FaceIndices.Contains(faceIndex))
                {
                    FaceIndices.Add(faceIndex);
                }
            }
        }

private readonly struct EdgeWearTopologyStats
        {
            public static readonly EdgeWearTopologyStats Empty =
                new EdgeWearTopologyStats(0, 0, 0);

            public readonly int OpenEdgeCount;
            public readonly int NonManifoldEdgeCount;
            public readonly int TJunctionCount;

            public EdgeWearTopologyStats(
                int openEdgeCount,
                int nonManifoldEdgeCount,
                int tJunctionCount)
            {
                OpenEdgeCount = openEdgeCount;
                NonManifoldEdgeCount = nonManifoldEdgeCount;
                TJunctionCount = tJunctionCount;
            }
        }

        private readonly struct TopologyEdgeKey : IEquatable<TopologyEdgeKey>
        {
            private readonly VertexKey first;
            private readonly VertexKey second;

            public VertexKey First => first;
            public VertexKey Second => second;

            public TopologyEdgeKey(VertexKey start, VertexKey end)
            {
                if (start.CompareTo(end) <= 0)
                {
                    first = start;
                    second = end;
                }
                else
                {
                    first = end;
                    second = start;
                }
            }

            public bool Equals(TopologyEdgeKey other)
            {
                return first.Equals(other.first) &&
                    second.Equals(other.second);
            }

            public override bool Equals(object obj)
            {
                return obj is TopologyEdgeKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (first.GetHashCode() * 397) ^
                        second.GetHashCode();
                }
            }
        }

        private readonly struct TopologyEdgeSegment
        {
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly VertexKey StartKey;
            public readonly VertexKey EndKey;

            public TopologyEdgeSegment(
                Vector3 start,
                Vector3 end,
                VertexKey startKey,
                VertexKey endKey)
            {
                Start = start;
                End = end;
                StartKey = startKey;
                EndKey = endKey;
            }
        }
        private struct ChamferCornerStats
        {
            public int SourceFaceCount;
            public int ExpectedCornerCount;
            public int SelectedEdgeCount;
            public int ActiveSelectedEdgeCount;
            public int DeferredSelectedEdgeCount;
            public float MinimumSolvedWidth;
            public float MaximumSolvedWidth;
            public int WidthSolveFailures;
            public int CornerSolveFailures;
            public float MinimumCornerWidthScale;
            public float InitialMaximumCornerDisplacement;
            public float FinalMaximumCornerDisplacement;
            public float MinimumSharedEdgeWidthScale;
            public int ConflictSearchStatesEvaluated;
            public int ConflictSearchCommittedExclusionCount;
            public int ConflictSearchTimeBudgetExceeded;
            public int ConflictSearchCancelled;
            public double ConflictSearchElapsedMilliseconds;
            public int ReplacementFaceAreaFailureCount;
            public int ReplacementFaceWindingFailureCount;
            public int ReplacementEdgeCollapseFailureCount;

        }

        private sealed class ChamferHalfEdge
        {
            public int Index;
            public int OriginVertex;
            public int DestinationVertex;
            public int FaceIndex;
            public int SourceEdgeIndex;
            public int Next = -1;
            public int Previous = -1;
            public int Opposite = -1;
            public bool IsSelected;
        }

        private sealed class ChamferTopologyContext
        {
            public readonly EdgeWearTopologyGraph Graph;
            public readonly List<EdgeWearSelectedGraphEdge> SelectedEdges;
            public readonly List<ChamferHalfEdge> HalfEdges;
            public readonly HashSet<int> SelectedSourceEdges;

            public ChamferTopologyContext(
                EdgeWearTopologyGraph graph,
                List<EdgeWearSelectedGraphEdge> selectedEdges,
                List<ChamferHalfEdge> halfEdges)
            {
                Graph = graph;
                SelectedEdges = selectedEdges;
                HalfEdges = halfEdges;
                SelectedSourceEdges = new HashSet<int>();
                for (int i = 0; i < selectedEdges.Count; i++)
                {
                    SelectedSourceEdges.Add(selectedEdges[i].GraphEdgeIndex);
                }
            }
        }

        private readonly struct ChamferFaceCornerKey :
            IEquatable<ChamferFaceCornerKey>
        {
            public readonly int FaceIndex;
            public readonly int SourceVertexIndex;

            public ChamferFaceCornerKey(int faceIndex, int sourceVertexIndex)
            {
                FaceIndex = faceIndex;
                SourceVertexIndex = sourceVertexIndex;
            }

            public bool Equals(ChamferFaceCornerKey other)
            {
                return FaceIndex == other.FaceIndex &&
                    SourceVertexIndex == other.SourceVertexIndex;
            }

            public override bool Equals(object obj)
            {
                return obj is ChamferFaceCornerKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (FaceIndex * 397) ^ SourceVertexIndex;
                }
            }
        }

        private sealed class ChamferSolvedCorner
        {
            public Vector3 Position;
            public readonly int FaceIndex;
            public readonly int SourceVertexIndex;
            public readonly int PreviousSourceEdgeIndex;
            public readonly int NextSourceEdgeIndex;
            public readonly bool PreviousSelected;
            public readonly bool NextSelected;

            public ChamferSolvedCorner(
                Vector3 position,
                int faceIndex,
                int sourceVertexIndex,
                int previousSourceEdgeIndex,
                int nextSourceEdgeIndex,
                bool previousSelected,
                bool nextSelected)
            {
                Position = position;
                FaceIndex = faceIndex;
                SourceVertexIndex = sourceVertexIndex;
                PreviousSourceEdgeIndex = previousSourceEdgeIndex;
                NextSourceEdgeIndex = nextSourceEdgeIndex;
                PreviousSelected = previousSelected;
                NextSelected = nextSelected;
            }
        }

        private sealed class ChamferSharedEdgeSpan
        {
            public readonly int SourceEdgeIndex;
            public readonly int FaceA;
            public readonly int FaceB;
            public readonly int VertexA;
            public readonly int VertexB;
            public readonly Vector3 SharedAtVertexA;
            public readonly Vector3 SharedAtVertexB;

            public ChamferSharedEdgeSpan(
                int sourceEdgeIndex,
                int faceA,
                int faceB,
                int vertexA,
                int vertexB,
                Vector3 sharedAtVertexA,
                Vector3 sharedAtVertexB)
            {
                SourceEdgeIndex = sourceEdgeIndex;
                FaceA = faceA;
                FaceB = faceB;
                VertexA = vertexA;
                VertexB = vertexB;
                SharedAtVertexA = sharedAtVertexA;
                SharedAtVertexB = sharedAtVertexB;
            }
        }

        private enum ChamferVertexBoundaryKind
        {
            BevelStripEndpoint,
            UnselectedEdgeTail
        }

        private readonly struct ChamferExpectedVertexBoundary
        {
            public readonly int SourceVertexIndex;
            public readonly int SourceEdgeIndex;
            public readonly int FaceIndex;
            public readonly ChamferVertexBoundaryKind Kind;
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly TopologyEdgeKey Key;

            public ChamferExpectedVertexBoundary(
                int sourceVertexIndex,
                int sourceEdgeIndex,
                int faceIndex,
                ChamferVertexBoundaryKind kind,
                Vector3 start,
                Vector3 end,
                TopologyEdgeKey key)
            {
                SourceVertexIndex = sourceVertexIndex;
                SourceEdgeIndex = sourceEdgeIndex;
                FaceIndex = faceIndex;
                Kind = kind;
                Start = start;
                End = end;
                Key = key;
            }
        }

        private enum ChamferVertexPatchClosure
        {
            Unclassified,
            ClosedLoop,
            OpenChainSourceBoundaryResolved,
            OpenChainSourceBoundaryCompletionResolved,
            OpenChainSourceBoundaryMultiCycleResolved,
            OpenChainSourceBoundaryResidualPatchResolved,
            OpenChainClosedSourceResolved,
            OpenChainClosedSourceClusterResolved,
            OpenChainUnresolved
        }

        private readonly struct ChamferOrientedVertexBoundary
        {
            public readonly ChamferExpectedVertexBoundary Boundary;
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly VertexKey StartKey;
            public readonly VertexKey EndKey;

            public ChamferOrientedVertexBoundary(
                ChamferExpectedVertexBoundary boundary,
                Vector3 start,
                Vector3 end,
                VertexKey startKey,
                VertexKey endKey)
            {
                Boundary = boundary;
                Start = start;
                End = end;
                StartKey = startKey;
                EndKey = endKey;
            }
        }

        private enum ChamferPatchIntersectionType
        {
            None,
            Proper,
            EndpointTouch,
            CollinearOverlap
        }

        private readonly struct ChamferPatchIntersectionEvidence
        {
            public readonly ChamferPatchIntersectionType Type;
            public readonly int FirstEdgeIndex;
            public readonly int SecondEdgeIndex;
            public readonly Vector2 FirstStart;
            public readonly Vector2 FirstEnd;
            public readonly Vector2 SecondStart;
            public readonly Vector2 SecondEnd;
            public readonly float FirstOrientation;
            public readonly float SecondOrientation;
            public readonly float ThirdOrientation;
            public readonly float FourthOrientation;

            public ChamferPatchIntersectionEvidence(
                ChamferPatchIntersectionType type,
                int firstEdgeIndex,
                int secondEdgeIndex,
                Vector2 firstStart,
                Vector2 firstEnd,
                Vector2 secondStart,
                Vector2 secondEnd,
                float firstOrientation,
                float secondOrientation,
                float thirdOrientation,
                float fourthOrientation)
            {
                Type = type;
                FirstEdgeIndex = firstEdgeIndex;
                SecondEdgeIndex = secondEdgeIndex;
                FirstStart = firstStart;
                FirstEnd = firstEnd;
                SecondStart = secondStart;
                SecondEnd = secondEnd;
                FirstOrientation = firstOrientation;
                SecondOrientation = secondOrientation;
                ThirdOrientation = thirdOrientation;
                FourthOrientation = fourthOrientation;
            }
        }

        private sealed class ChamferPatchTriangulationCandidate
        {
            public readonly float MinimumArea;
            public readonly float MinimumQuality;
            public readonly float MinimumAlignment;
            public readonly List<int> DiagonalCodes;

            public ChamferPatchTriangulationCandidate(
                float minimumArea,
                float minimumQuality,
                float minimumAlignment,
                List<int> diagonalCodes)
            {
                MinimumArea = minimumArea;
                MinimumQuality = minimumQuality;
                MinimumAlignment = minimumAlignment;
                DiagonalCodes = diagonalCodes;
            }
        }

        private readonly struct ChamferDirectedTriangleIndex
        {
            public readonly int First;
            public readonly int Second;
            public readonly int Third;

            public ChamferDirectedTriangleIndex(
                int first,
                int second,
                int third)
            {
                First = first;
                Second = second;
                Third = third;
            }
        }

        private readonly struct ChamferDirectedBoundaryEdge
        {
            public readonly TopologyEdgeKey Key;
            public readonly Vector3 ExistingStart;
            public readonly Vector3 ExistingEnd;
            public readonly VertexKey ExistingStartKey;
            public readonly VertexKey ExistingEndKey;
            public readonly Vector3 PatchStart;
            public readonly Vector3 PatchEnd;
            public readonly VertexKey PatchStartKey;
            public readonly VertexKey PatchEndKey;
            public readonly int OwningFaceIndex;
            public readonly Vector3 OwningFaceNormal;

            public ChamferDirectedBoundaryEdge(
                TopologyEdgeKey key,
                Vector3 existingStart,
                Vector3 existingEnd,
                int owningFaceIndex,
                Vector3 owningFaceNormal)
            {
                Key = key;
                ExistingStart = existingStart;
                ExistingEnd = existingEnd;
                ExistingStartKey = new VertexKey(existingStart);
                ExistingEndKey = new VertexKey(existingEnd);
                PatchStart = Vector3.zero;
                PatchEnd = Vector3.zero;
                PatchStartKey = default;
                PatchEndKey = default;
                OwningFaceIndex = owningFaceIndex;
                OwningFaceNormal = owningFaceNormal;
            }

            private ChamferDirectedBoundaryEdge(
                TopologyEdgeKey key,
                Vector3 existingStart,
                Vector3 existingEnd,
                Vector3 patchStart,
                Vector3 patchEnd,
                int owningFaceIndex,
                Vector3 owningFaceNormal)
            {
                Key = key;
                ExistingStart = existingStart;
                ExistingEnd = existingEnd;
                ExistingStartKey = new VertexKey(existingStart);
                ExistingEndKey = new VertexKey(existingEnd);
                PatchStart = patchStart;
                PatchEnd = patchEnd;
                PatchStartKey = new VertexKey(patchStart);
                PatchEndKey = new VertexKey(patchEnd);
                OwningFaceIndex = owningFaceIndex;
                OwningFaceNormal = owningFaceNormal;
            }

            public ChamferDirectedBoundaryEdge WithPatchDirection(
                Vector3 patchStart,
                Vector3 patchEnd)
            {
                return new ChamferDirectedBoundaryEdge(
                    Key,
                    ExistingStart,
                    ExistingEnd,
                    patchStart,
                    patchEnd,
                    OwningFaceIndex,
                    OwningFaceNormal);
            }
        }

        private sealed class ChamferProvisionalHalfEdge
        {
            public readonly int Index;
            public readonly int FaceRecordIndex;
            public readonly int FaceCornerIndex;
            public readonly VertexKey StartKey;
            public readonly VertexKey EndKey;
            public readonly TopologyEdgeKey UndirectedKey;
            public readonly Vector3 StartPosition;
            public readonly Vector3 EndPosition;
            public readonly int NextInFace;
            public readonly int PreviousInFace;
            public readonly ChamferProvisionalFaceKind FaceKind;
            public readonly int SourceFaceIndex;
            public readonly int SourceEdgeIndex;
            public int TwinHalfEdgeIndex;
            public int UndirectedUseCount;
            public bool IsBoundaryHalfEdge;

            public ChamferProvisionalHalfEdge(
                int index,
                int faceRecordIndex,
                int faceCornerIndex,
                VertexKey startKey,
                VertexKey endKey,
                Vector3 startPosition,
                Vector3 endPosition,
                int nextInFace,
                int previousInFace,
                ChamferProvisionalFaceKind faceKind,
                int sourceFaceIndex,
                int sourceEdgeIndex)
            {
                Index = index;
                FaceRecordIndex = faceRecordIndex;
                FaceCornerIndex = faceCornerIndex;
                StartKey = startKey;
                EndKey = endKey;
                UndirectedKey = new TopologyEdgeKey(startKey, endKey);
                StartPosition = startPosition;
                EndPosition = endPosition;
                NextInFace = nextInFace;
                PreviousInFace = previousInFace;
                FaceKind = faceKind;
                SourceFaceIndex = sourceFaceIndex;
                SourceEdgeIndex = sourceEdgeIndex;
                TwinHalfEdgeIndex = -1;
                UndirectedUseCount = 0;
                IsBoundaryHalfEdge = false;
            }
        }

        private enum ChamferHalfEdgeBoundaryComponentKind
        {
            Loop,
            OpenChain,
            PinchedOrAmbiguous
        }

        private sealed class ChamferCoDirectedHalfEdgePair
        {
            public readonly int FirstHalfEdgeIndex;
            public readonly int SecondHalfEdgeIndex;
            public ChamferCoDirectedHalfEdgePair(
                int firstHalfEdgeIndex,
                int secondHalfEdgeIndex)
            {
                FirstHalfEdgeIndex = firstHalfEdgeIndex;
                SecondHalfEdgeIndex = secondHalfEdgeIndex;
            }
        }

        private readonly struct ChamferDirectedEdgeKey :
            IEquatable<ChamferDirectedEdgeKey>
        {
            public readonly VertexKey Start;
            public readonly VertexKey End;

            public ChamferDirectedEdgeKey(
                VertexKey start,
                VertexKey end)
            {
                Start = start;
                End = end;
            }

            public bool Equals(ChamferDirectedEdgeKey other)
            {
                return Start.Equals(other.Start) &&
                    End.Equals(other.End);
            }

            public override bool Equals(object obj)
            {
                return obj is ChamferDirectedEdgeKey other &&
                    Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Start.GetHashCode() * 397) ^
                        End.GetHashCode();
                }
            }
        }

        private sealed class ChamferSectorPlanAssignment
        {
            public readonly int PlanListIndex;
            public readonly int ComponentListIndex;
            public readonly int OccurrenceOverlapCount;
            public readonly int KeyOverlapCount;
            public readonly bool Exact;

            public ChamferSectorPlanAssignment(
                int planListIndex,
                int componentListIndex,
                int occurrenceOverlapCount,
                int keyOverlapCount,
                bool exact)
            {
                PlanListIndex = planListIndex;
                ComponentListIndex = componentListIndex;
                OccurrenceOverlapCount = occurrenceOverlapCount;
                KeyOverlapCount = keyOverlapCount;
                Exact = exact;
            }
        }

        private sealed class ChamferHalfEdgeBoundaryComponent
        {
            public readonly int ComponentIndex;
            public readonly ChamferHalfEdgeBoundaryComponentKind Kind;
            public readonly List<int> OrderedHalfEdgeIndices;

            public ChamferHalfEdgeBoundaryComponent(
                int componentIndex,
                ChamferHalfEdgeBoundaryComponentKind kind,
                List<int> orderedHalfEdgeIndices)
            {
                ComponentIndex = componentIndex;
                Kind = kind;
                OrderedHalfEdgeIndices = orderedHalfEdgeIndices;
            }
        }

        private sealed class ChamferHalfEdgeDecomposition
        {
            public readonly List<ChamferProvisionalHalfEdge> HalfEdges =
                new List<ChamferProvisionalHalfEdge>();
            public readonly List<int> BoundaryHalfEdges =
                new List<int>();
            public readonly Dictionary<TopologyEdgeKey, List<int>>
                BoundaryHalfEdgesByKey =
                    new Dictionary<TopologyEdgeKey, List<int>>();
            public readonly Dictionary<int, int>
                SuccessorByBoundaryHalfEdge =
                    new Dictionary<int, int>();
            public readonly List<ChamferHalfEdgeBoundaryComponent>
                Components =
                    new List<ChamferHalfEdgeBoundaryComponent>();
            public readonly Dictionary<int, int> ComponentByHalfEdge =
                new Dictionary<int, int>();
            public readonly HashSet<VertexKey> PinchVertices =
                new HashSet<VertexKey>();
            public readonly List<ChamferCoDirectedHalfEdgePair>
                CoDirectedPairs =
                    new List<ChamferCoDirectedHalfEdgePair>();
            public int LoopCount;
            public int OpenChainCount;
            public int SuccessorFailureCount;
            public int UnassignedBoundaryEdgeCount;
            public int MultiAssignedBoundaryEdgeCount;
            public int InternalDirectionFailureCount;
        }

        private readonly struct ChamferSliverEdgeOwner
        {
            public readonly int FaceRecordIndex;
            public readonly int FaceCornerIndex;
            public readonly ChamferProvisionalFaceKind FaceKind;
            public readonly int SourceFaceIndex;
            public readonly int SourceEdgeIndex;

            public ChamferSliverEdgeOwner(
                int faceRecordIndex,
                int faceCornerIndex,
                ChamferProvisionalFaceKind faceKind,
                int sourceFaceIndex,
                int sourceEdgeIndex)
            {
                FaceRecordIndex = faceRecordIndex;
                FaceCornerIndex = faceCornerIndex;
                FaceKind = faceKind;
                SourceFaceIndex = sourceFaceIndex;
                SourceEdgeIndex = sourceEdgeIndex;
            }
        }

        private sealed class ChamferSliverEdgeCandidate
        {
            public readonly int BoundaryIndex;
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly TopologyEdgeKey Key;
            public readonly float Length;
            public readonly List<ChamferSliverEdgeOwner> Owners;

            public ChamferSliverEdgeCandidate(
                int boundaryIndex,
                Vector3 start,
                Vector3 end,
                TopologyEdgeKey key,
                float length,
                List<ChamferSliverEdgeOwner> owners)
            {
                BoundaryIndex = boundaryIndex;
                Start = start;
                End = end;
                Key = key;
                Length = length;
                Owners = owners;
            }
        }

        private readonly struct ChamferTrackedSanitizeVertex
        {
            public readonly Vector3 Position;
            public readonly VertexKey OriginalKey;
            public readonly int OriginalIndex;

            public ChamferTrackedSanitizeVertex(
                Vector3 position,
                VertexKey originalKey,
                int originalIndex)
            {
                Position = position;
                OriginalKey = originalKey;
                OriginalIndex = originalIndex;
            }
        }

        private enum ChamferSliverResolution
        {
            Unresolved,
            ResolvedToTriangle,
            ResolvedByElimination
        }

        private sealed class ChamferSliverNormalizationResult
        {
            public bool RepresentativeConflict;
            public bool Collapsed;
            public VertexKey RepresentativeKey;
            public Vector3 RepresentativePosition;
            public VertexKey RemovedKey;
            public List<int> AffectedFaceIndices = new List<int>();
            public int RebuiltSegmentCount;
            public List<Vector3> PostCollapseBoundaryPositions =
                new List<Vector3>();
            public List<TopologyEdgeKey> PostCollapseBoundaryKeys =
                new List<TopologyEdgeKey>();
            public int PostCollapseNonManifoldEdges;
            public int PostCollapseTJunctions;
            public int PostSegmentationSplitCount;
            public int PostSegmentationPassCount;
            public int PostSegmentationIncompatibleTJunctions;
            public int FaceFailureCount;
            public int TopologyFailureCount;
            public int IntersectionFailureCount;
            public ChamferDirectedTriangulationEvaluation TriangleEvaluation;
            public ChamferSliverResolution Resolution =
                ChamferSliverResolution.Unresolved;
            public string Failure = string.Empty;
        }


        private readonly struct ChamferDirectedEdgeUse
        {
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly VertexKey StartKey;
            public readonly VertexKey EndKey;
            public readonly int TriangleIndex;

            public ChamferDirectedEdgeUse(
                Vector3 start,
                Vector3 end,
                int triangleIndex)
            {
                Start = start;
                End = end;
                StartKey = new VertexKey(start);
                EndKey = new VertexKey(end);
                TriangleIndex = triangleIndex;
            }
        }



        private readonly struct ChamferBoundarySegment
        {
            public readonly Vector3 Start;
            public readonly Vector3 End;

            public ChamferBoundarySegment(Vector3 start, Vector3 end)
            {
                Start = start;
                End = end;
            }
        }



        private sealed class ChamferDirectedTriangleGeometry
        {
            public readonly Vector3 First;
            public readonly Vector3 Second;
            public readonly Vector3 Third;
            public readonly VertexKey FirstKey;
            public readonly VertexKey SecondKey;
            public readonly VertexKey ThirdKey;
            public readonly Vector3 Normal;
            public readonly float Area;
            public readonly float Quality;
            public readonly int TriangleIndex;

            public ChamferDirectedTriangleGeometry(
                Vector3 first,
                Vector3 second,
                Vector3 third,
                Vector3 normal,
                float area,
                float quality,
                int triangleIndex)
            {
                First = first;
                Second = second;
                Third = third;
                FirstKey = new VertexKey(first);
                SecondKey = new VertexKey(second);
                ThirdKey = new VertexKey(third);
                Normal = normal;
                Area = area;
                Quality = quality;
                TriangleIndex = triangleIndex;
            }

            public Vector3 GetPosition(int index)
            {
                return index == 0
                    ? First
                    : index == 1
                        ? Second
                        : Third;
            }

            public VertexKey GetKey(int index)
            {
                return index == 0
                    ? FirstKey
                    : index == 1
                        ? SecondKey
                        : ThirdKey;
            }

            public bool ContainsKey(VertexKey key)
            {
                return FirstKey.Equals(key) ||
                    SecondKey.Equals(key) ||
                    ThirdKey.Equals(key);
            }

            public Vector3 GetPosition(VertexKey key)
            {
                if (FirstKey.Equals(key))
                {
                    return First;
                }
                return SecondKey.Equals(key) ? Second : Third;
            }
        }

        private sealed class ChamferDirectedTriangulationEvaluation
        {
            public bool PassesIncidence;
            public bool PassesTriangleIntersection;
            public bool PassesExistingFaceIntersection;
            public bool TJunctionFailure;
            public bool NonManifoldFailure;
            public bool Feasible;
            public int StageScore;
            public float MinimumArea;
            public float MinimumQuality;
            public float MaximumInternalDihedral;
            public float MaximumBoundaryDihedral;
            public List<int> DiagonalCodes = new List<int>();
            public string Failure = string.Empty;
        }


        private enum ChamferVertexPatchLoopKind
        {
            LocalClosedComponent,
            ClosedSourceSpokeLoop,
            ClosedSourceCluster,
            ResidualSourceBoundaryCycle
        }

        private sealed class ChamferVertexPatchLoop
        {
            public readonly int LoopIndex;
            public readonly ChamferVertexPatchLoopKind Kind;
            public readonly List<Vector3> OrderedPositions;
            public readonly List<TopologyEdgeKey> OrderedBoundaryKeys;
            public readonly List<ChamferVertexPatchComponent> Components;
            public readonly List<Vector3> ComponentExpectedNormals;
            public readonly Vector3 ExpectedNormal;
            public readonly float FeatureStrength;

            public ChamferVertexPatchLoop(
                int loopIndex,
                ChamferVertexPatchLoopKind kind,
                List<Vector3> orderedPositions,
                List<TopologyEdgeKey> orderedBoundaryKeys,
                List<ChamferVertexPatchComponent> components,
                List<Vector3> componentExpectedNormals,
                Vector3 expectedNormal,
                float featureStrength)
            {
                LoopIndex = loopIndex;
                Kind = kind;
                OrderedPositions = orderedPositions;
                OrderedBoundaryKeys = orderedBoundaryKeys;
                Components = components;
                ComponentExpectedNormals = componentExpectedNormals;
                ExpectedNormal = expectedNormal;
                FeatureStrength = featureStrength;
            }
        }

        private sealed class ChamferVertexPatchPlan
        {
            public readonly List<ChamferVertexPatchLoop> PatchLoops;
            public readonly HashSet<TopologyEdgeKey>
                FinalSourceBoundaryEdges;
            public readonly HashSet<TopologyEdgeKey>
                RemainingVertexPatchBoundaryEdges;
            public readonly bool Ready;
            public readonly string Failure;

            public ChamferVertexPatchPlan(
                List<ChamferVertexPatchLoop> patchLoops,
                HashSet<TopologyEdgeKey> finalSourceBoundaryEdges,
                HashSet<TopologyEdgeKey> remainingVertexPatchBoundaryEdges,
                bool ready,
                string failure)
            {
                PatchLoops = patchLoops;
                FinalSourceBoundaryEdges =
                    new HashSet<TopologyEdgeKey>(finalSourceBoundaryEdges);
                RemainingVertexPatchBoundaryEdges =
                    new HashSet<TopologyEdgeKey>(
                        remainingVertexPatchBoundaryEdges);
                Ready = ready;
                Failure = failure;
            }
        }

        private sealed class ChamferVertexPatchSourceContext
        {
            public readonly int SourceVertexIndex;
            public readonly Vector3 SourcePosition;
            public readonly bool SourceFanOpen;
            public readonly bool IsValid;
            public readonly int ActiveRunCount;
            public readonly List<int> ActiveSourceEdges;
            public readonly List<ChamferSourceBoundaryRecord>
                SourceBoundaryRecords;

            public ChamferVertexPatchSourceContext(
                int sourceVertexIndex,
                Vector3 sourcePosition,
                bool sourceFanOpen,
                bool isValid,
                int activeRunCount,
                List<int> activeSourceEdges,
                List<ChamferSourceBoundaryRecord> sourceBoundaryRecords)
            {
                SourceVertexIndex = sourceVertexIndex;
                SourcePosition = sourcePosition;
                SourceFanOpen = sourceFanOpen;
                IsValid = isValid;
                ActiveRunCount = activeRunCount;
                ActiveSourceEdges = activeSourceEdges;
                SourceBoundaryRecords = sourceBoundaryRecords;
            }
        }

        private sealed class ChamferVertexPatchComponent
        {
            public readonly int SourceVertexIndex;
            public readonly List<ChamferOrientedVertexBoundary>
                OrderedBoundaries;
            public readonly List<Vector3> OrderedPositions;
            public readonly bool IsClosed;
            public readonly ChamferVertexPatchSourceContext SourceContext;
            public readonly bool SourceFanOpen;
            public readonly int ActiveRunCount;
            public readonly bool HasEndpointSpokes;
            public readonly VertexKey StartEndpointKey;
            public readonly VertexKey EndEndpointKey;
            public readonly TopologyEdgeKey StartSpokeKey;
            public readonly TopologyEdgeKey EndSpokeKey;
            public bool ProvenanceValid;
            public ChamferVertexPatchClosure Closure;
            public ChamferVertexPatchCluster Cluster;
            public ChamferFinalSourceBoundaryLoop FinalSourceBoundaryLoop;
            public int StartSourceBoundaryOwnerCount;
            public int EndSourceBoundaryOwnerCount;
            public int StartExistingSpokeUseCount;
            public int EndExistingSpokeUseCount;
            public int StartPlannedSpokeUseCount;
            public int EndPlannedSpokeUseCount;

            public ChamferVertexPatchComponent(
                int sourceVertexIndex,
                List<ChamferOrientedVertexBoundary> orderedBoundaries,
                List<Vector3> orderedPositions,
                bool isClosed,
                ChamferVertexPatchSourceContext sourceContext)
            {
                SourceVertexIndex = sourceVertexIndex;
                OrderedBoundaries = orderedBoundaries;
                OrderedPositions = orderedPositions;
                IsClosed = isClosed;
                SourceContext = sourceContext;
                SourceFanOpen = sourceContext.SourceFanOpen;
                ActiveRunCount = sourceContext.ActiveRunCount;
                ProvenanceValid = false;
                Closure = ChamferVertexPatchClosure.Unclassified;
                Cluster = null;
                FinalSourceBoundaryLoop = null;
                StartSourceBoundaryOwnerCount = 0;
                EndSourceBoundaryOwnerCount = 0;
                StartExistingSpokeUseCount = 0;
                EndExistingSpokeUseCount = 0;
                StartPlannedSpokeUseCount = 0;
                EndPlannedSpokeUseCount = 0;

                if (!isClosed && orderedBoundaries.Count > 0)
                {
                    StartEndpointKey = orderedBoundaries[0].StartKey;
                    EndEndpointKey =
                        orderedBoundaries[orderedBoundaries.Count - 1].EndKey;
                    VertexKey sourceKey = new VertexKey(
                        sourceContext.SourcePosition);
                    HasEndpointSpokes =
                        !StartEndpointKey.Equals(sourceKey) &&
                        !EndEndpointKey.Equals(sourceKey);
                    if (HasEndpointSpokes)
                    {
                        StartSpokeKey = new TopologyEdgeKey(
                            sourceKey,
                            StartEndpointKey);
                        EndSpokeKey = new TopologyEdgeKey(
                            sourceKey,
                            EndEndpointKey);
                    }
                    else
                    {
                        StartSpokeKey = default;
                        EndSpokeKey = default;
                    }
                }
                else
                {
                    HasEndpointSpokes = false;
                    StartEndpointKey = default;
                    EndEndpointKey = default;
                    StartSpokeKey = default;
                    EndSpokeKey = default;
                }
            }
        }

        private sealed class ChamferVertexPatchCluster
        {
            public readonly List<ChamferVertexPatchComponent> Components;
            public readonly List<ChamferOrientedVertexBoundary>
                OrderedBoundaries;
            public readonly List<Vector3> OrderedPositions;
            public readonly HashSet<int> SourceVertexIndices;
            public readonly List<ChamferFinalSourceBoundaryEdge>
                OrderedCompletionEdges;
            public readonly bool IsClosed;

            public ChamferVertexPatchCluster(
                List<ChamferVertexPatchComponent> components,
                List<ChamferOrientedVertexBoundary> orderedBoundaries,
                List<Vector3> orderedPositions,
                HashSet<int> sourceVertexIndices)
            {
                Components = components;
                OrderedBoundaries = orderedBoundaries;
                OrderedPositions = orderedPositions;
                SourceVertexIndices = sourceVertexIndices;
                OrderedCompletionEdges =
                    new List<ChamferFinalSourceBoundaryEdge>();
                IsClosed = true;
            }

            public ChamferVertexPatchCluster(
                List<ChamferVertexPatchComponent> components,
                List<ChamferFinalSourceBoundaryEdge> orderedCompletionEdges)
            {
                Components = components;
                OrderedBoundaries =
                    new List<ChamferOrientedVertexBoundary>();
                OrderedCompletionEdges = orderedCompletionEdges;
                OrderedPositions = new List<Vector3>();
                SourceVertexIndices = new HashSet<int>();
                for (int i = 0; i < orderedCompletionEdges.Count; i++)
                {
                    OrderedPositions.Add(orderedCompletionEdges[i].Start);
                }
                for (int i = 0; i < components.Count; i++)
                {
                    SourceVertexIndices.Add(
                        components[i].SourceVertexIndex);
                }
                IsClosed = true;
            }
        }

        private readonly struct ChamferBoundaryCompletionEdge
        {
            public readonly TopologyEdgeKey Key;
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly VertexKey StartKey;
            public readonly VertexKey EndKey;
            public readonly bool IsCandidate;
            public readonly int BoundaryLoopIndex;
            public readonly int BoundaryOrder;
            public readonly int SourceEdgeIndex;
            public readonly int LocalIndex;
            public readonly int SourceVertexIndex;
            public readonly ChamferVertexPatchComponent Component;

            public ChamferBoundaryCompletionEdge(
                TopologyEdgeKey key,
                Vector3 start,
                Vector3 end,
                bool isCandidate,
                int boundaryLoopIndex,
                int boundaryOrder,
                int sourceEdgeIndex,
                int localIndex,
                int sourceVertexIndex,
                ChamferVertexPatchComponent component)
            {
                Key = key;
                Start = start;
                End = end;
                StartKey = new VertexKey(start);
                EndKey = new VertexKey(end);
                IsCandidate = isCandidate;
                BoundaryLoopIndex = boundaryLoopIndex;
                BoundaryOrder = boundaryOrder;
                SourceEdgeIndex = sourceEdgeIndex;
                LocalIndex = localIndex;
                SourceVertexIndex = sourceVertexIndex;
                Component = component;
            }
        }

        private readonly struct ChamferFinalSourceBoundaryEdge
        {
            public readonly TopologyEdgeKey Key;
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly VertexKey StartKey;
            public readonly VertexKey EndKey;
            public readonly bool IsPromoted;
            public readonly int BoundaryOrder;
            public readonly int SourceEdgeIndex;
            public readonly int SourceVertexIndex;

            public ChamferFinalSourceBoundaryEdge(
                ChamferBoundaryCompletionEdge edge,
                bool forward)
            {
                Key = edge.Key;
                Start = forward ? edge.Start : edge.End;
                End = forward ? edge.End : edge.Start;
                StartKey = forward ? edge.StartKey : edge.EndKey;
                EndKey = forward ? edge.EndKey : edge.StartKey;
                IsPromoted = edge.IsCandidate;
                BoundaryOrder = edge.BoundaryOrder;
                SourceEdgeIndex = edge.SourceEdgeIndex;
                SourceVertexIndex = edge.SourceVertexIndex;
            }
        }

        private sealed class ChamferBoundaryCompletionCycle
        {
            public readonly List<ChamferFinalSourceBoundaryEdge>
                OrderedEdges;
            public readonly HashSet<VertexKey> Vertices;
            public readonly HashSet<ChamferVertexPatchComponent>
                CandidateComponents;
            public readonly HashSet<int> SourceBoundaryOrders;
            public readonly HashSet<int> SourceEdgeIndices;
            public readonly HashSet<int> CandidateSourceVertices;
            public readonly VertexKey MinimumVertex;

            public ChamferBoundaryCompletionCycle(
                List<ChamferFinalSourceBoundaryEdge> orderedEdges,
                HashSet<VertexKey> vertices,
                HashSet<ChamferVertexPatchComponent> candidateComponents,
                HashSet<int> sourceBoundaryOrders,
                HashSet<int> sourceEdgeIndices,
                HashSet<int> candidateSourceVertices,
                VertexKey minimumVertex)
            {
                OrderedEdges = orderedEdges;
                Vertices = vertices;
                CandidateComponents = candidateComponents;
                SourceBoundaryOrders = sourceBoundaryOrders;
                SourceEdgeIndices = sourceEdgeIndices;
                CandidateSourceVertices = candidateSourceVertices;
                MinimumVertex = minimumVertex;
            }
        }

        private sealed class ChamferFinalSourceBoundaryLoop
        {
            public readonly int SourceBoundaryLoopIndex;
            public readonly List<ChamferFinalSourceBoundaryEdge> OrderedEdges;
            public readonly List<ChamferVertexPatchComponent>
                PromotedComponents;
            public readonly int PromotedEdgeCount;

            public ChamferFinalSourceBoundaryLoop(
                int sourceBoundaryLoopIndex,
                List<ChamferFinalSourceBoundaryEdge> orderedEdges,
                List<ChamferVertexPatchComponent> promotedComponents,
                int promotedEdgeCount)
            {
                SourceBoundaryLoopIndex = sourceBoundaryLoopIndex;
                OrderedEdges = orderedEdges;
                PromotedComponents = promotedComponents;
                PromotedEdgeCount = promotedEdgeCount;
            }
        }



        private readonly struct ChamferSourceBoundaryEndpointOwnership
        {
            public readonly ChamferSourceBoundaryRecord Record;
            public readonly int ChildIndex;

            public ChamferSourceBoundaryEndpointOwnership(
                ChamferSourceBoundaryRecord record,
                int childIndex)
            {
                Record = record;
                ChildIndex = childIndex;
            }
        }

        private readonly struct ChamferSourceBoundaryChildLocation
        {
            public readonly ChamferSourceBoundaryRecord Record;
            public readonly int ChildIndex;
            public readonly ChamferSourceBoundaryChild Child;

            public ChamferSourceBoundaryChildLocation(
                ChamferSourceBoundaryRecord record,
                int childIndex,
                ChamferSourceBoundaryChild child)
            {
                Record = record;
                ChildIndex = childIndex;
                Child = child;
            }
        }

        private readonly struct ChamferSourceBoundaryChildOccurrence
        {
            public readonly ChamferSourceBoundaryRecord Record;
            public readonly int ChildIndex;
            public readonly ChamferSourceBoundaryChild Child;
            public readonly int FlattenedLoopIndex;
            public readonly int LoopChildCount;
            public readonly int RecordChildCount;
            public readonly bool TerminalTransition;

            public ChamferSourceBoundaryChildOccurrence(
                ChamferSourceBoundaryRecord record,
                int childIndex,
                ChamferSourceBoundaryChild child,
                int flattenedLoopIndex,
                int loopChildCount,
                int recordChildCount,
                bool terminalTransition)
            {
                Record = record;
                ChildIndex = childIndex;
                Child = child;
                FlattenedLoopIndex = flattenedLoopIndex;
                LoopChildCount = loopChildCount;
                RecordChildCount = recordChildCount;
                TerminalTransition = terminalTransition;
            }
        }

        private readonly struct ChamferSourceBoundaryChildRemoval
        {
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly string Stage;
            public readonly int PartnerSourceEdgeIndex;
            public readonly int PartnerBoundaryOrder;

            public ChamferSourceBoundaryChildRemoval(
                Vector3 start,
                Vector3 end,
                string stage,
                int partnerSourceEdgeIndex,
                int partnerBoundaryOrder)
            {
                Start = start;
                End = end;
                Stage = stage;
                PartnerSourceEdgeIndex = partnerSourceEdgeIndex;
                PartnerBoundaryOrder = partnerBoundaryOrder;
            }
        }

        private sealed class ChamferSourceBoundaryRecord
        {
            public readonly int SourceEdgeIndex;
            public readonly int BoundaryLoopIndex;
            public readonly int BoundaryOrder;
            public readonly int SourceVertexStart;
            public readonly int SourceVertexEnd;
            public readonly Vector3 ParentStart;
            public readonly Vector3 ParentEnd;
            public readonly List<ChamferSourceBoundaryChild> Children =
                new List<ChamferSourceBoundaryChild>();
            public readonly List<ChamferSourceBoundaryChildRemoval>
                RemovedChildren =
                    new List<ChamferSourceBoundaryChildRemoval>();
            public int RawChildCount;
            public int PostLoopNormalizationChildCount;
            public int PostTerminalAliasChildCount;

            public ChamferSourceBoundaryRecord(
                int sourceEdgeIndex,
                int boundaryLoopIndex,
                int boundaryOrder,
                int sourceVertexStart,
                int sourceVertexEnd,
                Vector3 parentStart,
                Vector3 parentEnd)
            {
                SourceEdgeIndex = sourceEdgeIndex;
                BoundaryLoopIndex = boundaryLoopIndex;
                BoundaryOrder = boundaryOrder;
                SourceVertexStart = sourceVertexStart;
                SourceVertexEnd = sourceVertexEnd;
                ParentStart = parentStart;
                ParentEnd = parentEnd;
                RawChildCount = 0;
                PostLoopNormalizationChildCount = 0;
                PostTerminalAliasChildCount = 0;
                Children.Add(new ChamferSourceBoundaryChild(
                    parentStart,
                    parentEnd,
                    true,
                    true));
            }
        }

        private readonly struct ChamferSourceBoundaryChild
        {
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly TopologyEdgeKey Key;
            public readonly bool TouchesParentStart;
            public readonly bool TouchesParentEnd;

            public ChamferSourceBoundaryChild(
                Vector3 start,
                Vector3 end,
                bool touchesParentStart,
                bool touchesParentEnd)
            {
                Start = start;
                End = end;
                Key = new TopologyEdgeKey(
                    new VertexKey(start),
                    new VertexKey(end));
                TouchesParentStart = touchesParentStart;
                TouchesParentEnd = touchesParentEnd;
            }
        }

        private enum ChamferProvisionalFaceKind
        {
            ReplacementBase,
            BevelStrip,
            VertexPatch
        }

        private enum ChamferSegmentRole
        {
            ReplacementSharedSpan,
            ReplacementVertexTail,
            ReplacementOrdinaryEdge,
            PreservedSourceBoundary,
            BevelRail,
            BevelEndpoint,
            VertexPatchBoundary,
            VertexPatchDiagonal
        }

        private sealed class ChamferProvisionalFaceRecord
        {
            public PolygonFace Face;
            public readonly ChamferProvisionalFaceKind Kind;
            public readonly int SourceFaceIndex;
            public readonly int SourceEdgeIndex;
            public readonly int PatchLoopIndex;

            public ChamferProvisionalFaceRecord(
                PolygonFace face,
                ChamferProvisionalFaceKind kind,
                int sourceFaceIndex,
                int sourceEdgeIndex,
                int patchLoopIndex = -1)
            {
                Face = face;
                Kind = kind;
                SourceFaceIndex = sourceFaceIndex;
                SourceEdgeIndex = sourceEdgeIndex;
                PatchLoopIndex = patchLoopIndex;
            }
        }

        private readonly struct ChamferProvisionalSegmentRecord
        {
            public readonly int FaceRecordIndex;
            public readonly int LocalEdgeIndex;
            public readonly TopologyEdgeKey Key;
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly ChamferProvisionalFaceKind FaceKind;
            public readonly ChamferSegmentRole Role;
            public readonly int SourceFaceIndex;
            public readonly int SourceEdgeIndex;
            public readonly int PatchLoopIndex;

            public ChamferProvisionalSegmentRecord(
                int faceRecordIndex,
                int localEdgeIndex,
                TopologyEdgeKey key,
                Vector3 start,
                Vector3 end,
                ChamferProvisionalFaceKind faceKind,
                ChamferSegmentRole role,
                int sourceFaceIndex,
                int sourceEdgeIndex,
                int patchLoopIndex = -1)
            {
                FaceRecordIndex = faceRecordIndex;
                LocalEdgeIndex = localEdgeIndex;
                Key = key;
                Start = start;
                End = end;
                FaceKind = faceKind;
                Role = role;
                SourceFaceIndex = sourceFaceIndex;
                SourceEdgeIndex = sourceEdgeIndex;
                PatchLoopIndex = patchLoopIndex;
            }
        }

        private sealed class ChamferBoundaryPointRecord
        {
            public readonly VertexKey Key;
            public readonly Vector3 Position;
            public readonly HashSet<int> SourceVertexIndices =
                new HashSet<int>();

            public ChamferBoundaryPointRecord(
                VertexKey key,
                Vector3 position)
            {
                Key = key;
                Position = position;
            }
        }

        private readonly struct ChamferSplitPoint
        {
            public readonly VertexKey Key;
            public readonly Vector3 Position;
            public readonly float Parameter;
            public readonly int CompatibleFaceRecordIndex;
            public readonly int CompatibleLocalEdgeIndex;

            public ChamferSplitPoint(
                VertexKey key,
                Vector3 position,
                float parameter,
                int compatibleFaceRecordIndex,
                int compatibleLocalEdgeIndex)
            {
                Key = key;
                Position = position;
                Parameter = parameter;
                CompatibleFaceRecordIndex = compatibleFaceRecordIndex;
                CompatibleLocalEdgeIndex = compatibleLocalEdgeIndex;
            }
        }

        private readonly struct ChamferTJunctionRecordKey :
            IEquatable<ChamferTJunctionRecordKey>
        {
            private readonly VertexKey pointKey;
            private readonly TopologyEdgeKey containingEdgeKey;
            private readonly int faceRecordIndex;
            private readonly ChamferSegmentRole segmentRole;

            public ChamferTJunctionRecordKey(
                VertexKey pointKey,
                TopologyEdgeKey containingEdgeKey,
                int faceRecordIndex,
                ChamferSegmentRole segmentRole)
            {
                this.pointKey = pointKey;
                this.containingEdgeKey = containingEdgeKey;
                this.faceRecordIndex = faceRecordIndex;
                this.segmentRole = segmentRole;
            }

            public bool Equals(ChamferTJunctionRecordKey other)
            {
                return pointKey.Equals(other.pointKey) &&
                    containingEdgeKey.Equals(other.containingEdgeKey) &&
                    faceRecordIndex == other.faceRecordIndex &&
                    segmentRole == other.segmentRole;
            }

            public override bool Equals(object obj)
            {
                return obj is ChamferTJunctionRecordKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = pointKey.GetHashCode();
                    hash = (hash * 397) ^ containingEdgeKey.GetHashCode();
                    hash = (hash * 397) ^ faceRecordIndex;
                    hash = (hash * 397) ^ (int)segmentRole;
                    return hash;
                }
            }
        }

        private enum ChamferCornerZeroingStage
        {
            None,
            SharedEdgeUniformScale,
            SharedEdgeForcedDeferral
        }

        private sealed class ChamferCornerConflictRecord
        {
            public int UnselectedSourceEdgeIndex = -1;
            public float UniformScale;
            public ChamferCornerZeroingStage ZeroingStage;
            public readonly List<int> ParticipatingSelectedEdges =
                new List<int>();
            public readonly List<int> ZeroedSelectedEdges =
                new List<int>();
            public readonly Dictionary<int, float>
                ParticipantWidthBeforeScale =
                    new Dictionary<int, float>();
        }

        private sealed class ChamferCornerSolution
        {
            public readonly Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> Corners;
            public readonly Dictionary<int, float> WidthByEdge;
            public readonly Dictionary<int, ChamferSharedEdgeSpan> SharedSpans;
            public readonly List<ChamferCornerConflictRecord> Conflicts;
            public readonly HashSet<int> ForcedDeferredEdges;

            public ChamferCornerSolution(
                Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> corners,
                Dictionary<int, float> widthByEdge,
                Dictionary<int, ChamferSharedEdgeSpan> sharedSpans,
                List<ChamferCornerConflictRecord> conflicts,
                ICollection<int> forcedDeferredEdges)
            {
                Corners = corners;
                WidthByEdge = widthByEdge;
                SharedSpans = sharedSpans;
                Conflicts = conflicts ??
                    new List<ChamferCornerConflictRecord>();
                ForcedDeferredEdges = forcedDeferredEdges == null
                    ? new HashSet<int>()
                    : new HashSet<int>(forcedDeferredEdges);
            }
        }

        private readonly struct ChamferFaceLine
        {
            public readonly Vector3 Point;
            public readonly Vector3 Direction;
            public readonly int SourceEdgeIndex;
            public readonly float Offset;

            public ChamferFaceLine(
                Vector3 point,
                Vector3 direction,
                int sourceEdgeIndex,
                float offset)
            {
                Point = point;
                Direction = direction;
                SourceEdgeIndex = sourceEdgeIndex;
                Offset = offset;
            }
        }

        private struct ChamferEmissionStats
        {
            public int SourceFaceCount;
            public int ReplacementFacesAttempted;
            public int ReplacementFacesBuilt;
            public int CandidateSelectedEdgeCount;
            public int ActiveSelectedEdgeCount;
            public int DeferredSelectedEdgeCount;
            public int BevelStripsAttempted;
            public int BevelStripsBuilt;
            public int SourceBoundaryLoopNormalizationFailureCount;
            public int SourceBoundaryTerminalAliasNormalizationFailureCount;
            public int SourceBoundaryTerminalTransferFailureCount;
            public int SourceBoundaryChildIncidenceFailureCount;
            public int SourceBoundaryDuplicateChildKeyFailureCount;
            public int ExpectedSourceBoundaryEdgeCount;
            public int MatchedSourceBoundaryEdgeCount;
            public int ProvisionalOpenEdgeCount;
            public int UnexpectedProvisionalOpenEdgeCount;
            public int MissingExpectedVertexBoundaryEdgeCount;
            public int ProvisionalNonManifoldEdgeCount;
            public int ProvisionalTJunctionCount;
            public int VertexBoundarySameOwnerDuplicateFailureCount;
            public int VertexBoundaryMultiOwnerFailureCount;
            public int FaceLocalNormalizationFailureCount;
            public int FaceLocalDuplicateEdgeFailureCount;
            public int StaleBoundaryRegistrationFailureCount;
            public int ProvenanceCompatibleTJunctionSplits;
            public int TJunctionSegmentationPasses;
            public int TJunctionRecordsIncompatible;
            public int VertexBoundaryBranchFailureCount;
            public int VertexBoundaryDuplicateFailureCount;
            public int PatchUnresolvedOpenChainCount;
            public int PatchComponentOrderingFailureCount;
            public int PatchComponentProvenanceFailureCount;
            public int PatchLoopsAttempted;
            public int PatchLoopsBuilt;
            public int PatchLoopsFailed;
            public int PatchMaximumBoundaryCount;
            public int PatchTrianglesAttempted;
            public int PatchTrianglesBuilt;
            public int PatchLoopConstructionFailureCount;
            public int PatchNonFiniteFailureCount;
            public int PatchAreaFailureCount;
            public int PatchWindingFailureCount;
            public int PatchDuplicateEdgeFailureCount;
            public int PatchProjectionFailureCount;
            public int PatchSelfIntersectionFailureCount;
            public long PatchTriangulationCandidatesTested;
            public long PatchFeasibleTriangulationCount;
            public int PatchSectorExistingPlanLoops;
            public int PatchSectorAuthoritativeLoops;
            public int PatchSectorBoundaryHalfEdges;
            public int PatchSectorBoundaryHalfEdgesAssigned;
            public int PatchCorrectedLoopsAttempted;
            public int PatchCorrectedLoopsBuilt;
            public int PatchCorrectedReservedSliverLoops;
            public int PatchCorrectedReservedSliverTriangles;
            public int PatchCorrectedBaselineLoopsRejected;
            public int PatchOverlapLoopsClassified;
            public int PatchOverlapPatchContainedInReplacement;
            public int PatchOverlapReplacementContainedInPatch;
            public int PatchOverlapPartialCoplanarArea;
            public int PatchOverlapNonCoplanarPenetration;
            public int PatchOverlapBevelStripPenetration;
            public int PatchOverlapUnclassified;
            public int PatchOverlapBoundaryOwner;
            public int PatchOverlapNonBoundaryOwner;
            public long PatchOverlapProjectedAreaNanounits;
            public int PatchContainedOwnershipCandidates;
            public int PatchContainedOwnershipResolved;
            public int PatchContainedOwnershipStillRequired;
            public int PatchContainedOwnershipOwnerAmbiguous;
            public int PatchContainedOwnershipBoundaryTransferFailures;
            public int PatchContainedOwnershipTopologyFailures;
            public int PatchContainedRepartitionCandidates;
            public int PatchContainedRepartitionResolved;
            public int PatchContainedRepartitionArrangementFailures;
            public int PatchContainedRepartitionTriangulationFailures;
            public int PatchContainedRepartitionAreaFailures;
            public int PatchContainedRepartitionBoundaryFailures;
            public int PatchContainedRepartitionTopologyFailures;
            public int PatchContainedRepartitionOverlapRemaining;
            public int PatchContainedRepairCandidates;
            public int PatchContainedRepairGuidedResiduals;
            public int PatchContainedRepairGenericFallbacks;
            public int PatchContainedRepairEndpointAligned;
            public int PatchContainedRepairResolved;
            public int PatchContainedRepairBuildFailures;
            public int PatchContainedRepairBoundaryFailures;
            public int PatchContainedRepairTopologyFailures;
            public int PatchContainedRepairOverlapRemaining;
            public int PatchContainedBoundaryCandidates;
            public int PatchContainedBoundaryExactValid;
            public int PatchContainedBoundarySplitEquivalent;
            public int PatchContainedBoundaryResidualMissing;
            public int PatchContainedBoundaryExternalUnsplit;
            public int PatchContainedBoundaryUnderused;
            public int PatchContainedBoundaryOverused;
            public int PatchContainedBoundaryAmbiguous;
            public int PatchContainedBoundarySegments;
            public int PatchContainedBoundarySegmentExactValid;
            public int PatchContainedBoundarySegmentSplitEquivalent;
            public int PatchContainedBoundarySegmentResidualMissing;
            public int PatchContainedBoundarySegmentExternalUnsplit;
            public int PatchContainedBoundarySegmentUnderused;
            public int PatchContainedBoundarySegmentOverused;
            public int PatchContainedBoundarySegmentAmbiguous;
            public int PatchContainedShadowTested;
            public int PatchContainedShadowOverlapRemoved;
            public int PatchContainedShadowTopologyClean;
            public int PatchContainedShadowTJunctionIncrease;
            public int PatchContainedShadowUnexpectedOpenEdgeIncrease;
            public int PatchContainedShadowSourceBoundaryIncrease;
            public int PatchContainedShadowNonManifoldIncrease;
            public int PatchContainedCombinedAttempted;
            public int PatchContainedCombinedApplied;
            public int PatchContainedCombinedOwnerConflicts;
            public int PatchContainedCombinedTopologyFailures;
            public int PatchContainedCombinedRemainingOverlaps;
            public int PatchCorrectedBoundaryMissingOpposite;
            public int PatchCorrectedBoundaryDuplicateOpposite;
            public int PatchCorrectedBoundaryDirectionMismatch;
            public int PatchCorrectedBoundaryExtraPatchEdge;
            public string PatchCorrectedBoundaryOccurrenceDiagnostic;
            public int PatchSliverDeltaPreCollapseComponents;
            public int PatchSliverDeltaPostCollapseComponents;
            public int PatchSliverDeltaReservedPreComponents;
            public int PatchSliverDeltaExactComponentMatches;
            public int PatchSliverDeltaDisappearedComponents;
            public int PatchSliverDeltaMergedPostComponents;
            public int PatchSliverDeltaSplitPreComponents;
            public int PatchSliverDeltaMissingLoopCount;
            public string PatchSliverDeltaDiagnostic;
            public int PatchCorrectedFinalUnexpectedOpenEdges;
            public int PatchCorrectedFinalNonManifoldEdges;
            public int PatchCorrectedFinalTJunctions;
            public int ReadyForCorrectedChamferPatchTopology;
            public int DiagnosticGeometrySignature;
            public int PatchEarSelectionFailureCount;
            public int PatchDiagonalIntersectionFailureCount;
            public int PatchBoundaryUseFailureCount;
            public int PatchDiagonalUseFailureCount;
            public int FinalSourceBoundaryUseFailureCount;
            public int FinalUnexpectedOpenEdgeCount;
            public int FinalPatchNonManifoldEdgeCount;
            public int FinalPatchTJunctionCount;
            public int ReadyForChamferPatchTopology;

        }

        private struct ChamferReadinessStats
        {
            public int SourceFaceCount;
            public int SourceNonManifoldEdgeCount;
            public int SourceTJunctionCount;
            public int BoundaryTraceFailureCount;
            public int SelectedGraphEdgeCount;
            public int SelectedBoundaryEdgeCount;
            public int SelectedNonManifoldEdgeCount;
            public int MissingSelectedGraphEdgeCount;
            public int MismatchedSelectedGraphFaceCount;
            public int DuplicateSelectedGraphEdgeCount;
            public int AffectedVertexCount;
            public int DisconnectedVertexFanCount;
            public int Ready;

            public ChamferReadinessStats(int candidateCount, int selectedCount)
            {
                this = default;
            }

            public void ApplyGraphStats(EdgeWearGraphBuildStats stats)
            {
                SourceFaceCount = stats.GraphFaceCount;
                SourceNonManifoldEdgeCount = stats.GraphNonManifoldEdgeCount;
                SelectedGraphEdgeCount = stats.SelectedGraphEdgeCount;
                MissingSelectedGraphEdgeCount = stats.MissingSelectedGraphEdgeCount;
                MismatchedSelectedGraphFaceCount = stats.MismatchedSelectedGraphFaceCount;
                DuplicateSelectedGraphEdgeCount = stats.DuplicateSelectedGraphEdgeCount;
            }

        }

        private sealed class EdgeWearTopologyGraph
        {
            public readonly List<EdgeWearGraphVertex> Vertices =
                new List<EdgeWearGraphVertex>();
            public readonly List<EdgeWearGraphEdge> Edges =
                new List<EdgeWearGraphEdge>();
            public readonly List<EdgeWearGraphFace> Faces =
                new List<EdgeWearGraphFace>();
            public readonly Dictionary<VertexKey, int> VertexByKey =
                new Dictionary<VertexKey, int>();
            public readonly Dictionary<EdgeKey, int> EdgeByKey =
                new Dictionary<EdgeKey, int>();
            public int CoincidentVertexReconciliationCount;
            public int CoincidentBoundarySeamPairCount;
        }
        private sealed class EdgeWearGraphVertex
        {
            public readonly Vector3 Position;
            public readonly VertexKey Key;
            public readonly List<int> EdgeIndices = new List<int>();
            public readonly List<int> FaceIndices = new List<int>();

            public EdgeWearGraphVertex(Vector3 position, VertexKey key)
            {
                Position = position;
                Key = key;
            }

            public void AddEdge(int edgeIndex)
            {
                if (!EdgeIndices.Contains(edgeIndex))
                {
                    EdgeIndices.Add(edgeIndex);
                }
            }

            public void AddFace(int faceIndex)
            {
                if (!FaceIndices.Contains(faceIndex))
                {
                    FaceIndices.Add(faceIndex);
                }
            }
        }

        private sealed class EdgeWearGraphEdge
        {
            public readonly int VertexA;
            public readonly int VertexB;
            public int FaceA = -1;
            public int FaceB = -1;
            public int ExtraFaceCount;
            public int CandidateIndex = -1;
            public bool Selected;

            public EdgeWearGraphEdge(int vertexA, int vertexB)
            {
                VertexA = vertexA;
                VertexB = vertexB;
            }

            public bool TryAddFace(int faceIndex)
            {
                if (FaceA == faceIndex || FaceB == faceIndex)
                {
                    return true;
                }

                if (FaceA < 0)
                {
                    FaceA = faceIndex;
                    return true;
                }

                if (FaceB < 0)
                {
                    FaceB = faceIndex;
                    return true;
                }

                ExtraFaceCount++;
                return false;
            }
        }

        private sealed class EdgeWearGraphFace
        {
            public readonly int SourceFaceIndex;
            public readonly PolygonFace SourceFace;
            public readonly List<int> VertexIndices;
            public readonly List<int> EdgeIndices;

            public EdgeWearGraphFace(
                int sourceFaceIndex,
                PolygonFace sourceFace,
                List<int> vertexIndices,
                List<int> edgeIndices)
            {
                SourceFaceIndex = sourceFaceIndex;
                SourceFace = sourceFace;
                VertexIndices = vertexIndices;
                EdgeIndices = edgeIndices;
            }
        }

        private sealed class EdgeWearMicroTopologySuppressedEdge
        {
            public int OriginalSourceEdgeIndex = -1;
            public Vector3 Start;
            public Vector3 End;
            public float Length;
        }

        private sealed class EdgeWearMicroTopologyCollapseAttemptRecord
        {
            public int CanonicalGraphVertexIndex = -1;
            public Vector3 CanonicalPosition;
            public bool Succeeded;
            public double SquaredDisplacement;
            public double NormalizedVolume;
            public double VolumeLoss;
            public string Blocker = string.Empty;
        }

        private sealed class EdgeWearMicroTopologyComponentRecord
        {
            public readonly List<int> EdgeIndices = new List<int>();
            public readonly List<int> SeedEdgeIndices = new List<int>();
            public readonly List<int> VertexIndices = new List<int>();
            public readonly List<EdgeWearMicroTopologyCollapseAttemptRecord>
                Attempts =
                    new List<EdgeWearMicroTopologyCollapseAttemptRecord>();
            public float Diameter;
            public bool CandidateEligible;
            public bool Applied;
            public int SelectedCanonicalGraphVertexIndex = -1;
            public double SelectedSquaredDisplacement;
            public double SelectedVolumeLoss;
            public string Blocker = string.Empty;
        }

        private sealed class CornerDamageCandidateRecord
        {
            public int GraphVertexIndex = -1;
            public Vector3 Position;
            public bool Eligible;
            public string Blocker = string.Empty;
            public int IncidentFaceCount;
            public int IncidentEdgeCount;
            public int ConvexIncidentEdgeCount;
            public float MaximumIncidentDihedral;
            public float MinimumIncidentEdgeLength;
            public float SharpnessScore;
            public float SizeScore;
            public float UpwardExposureScore;
            public float RandomScore;
            public float Score;
            public readonly List<int> IncidentFaceIndices =
                new List<int>();
            public readonly List<int> IncidentGraphEdgeIndices =
                new List<int>();
            public readonly List<int> IncidentOriginalEdgeIndices =
                new List<int>();
        }

        private sealed class CornerDamageEdgeIdentityRecord
        {
            public string Kind = string.Empty;
            public int OutputGraphEdgeIndex = -1;
            public int ParentOriginalEdgeA = -1;
            public int ParentOriginalEdgeB = -1;
            public int GeneratedIdentity = -1;
            public Vector3 Start;
            public Vector3 End;
        }

        private sealed class CornerDamageTrialRecord
        {
            public int TrialIndex = -1;
            public float DepthFactor;
            public float Depth;
            public Vector3 PlanePoint;
            public float PlaneDistance;
            public bool Succeeded;
            public string Blocker = string.Empty;
            public int FaceCount;
            public int CapFaceCount;
            public int CapVertexCount;
            public float CapArea;
            public float MaximumCapPlaneResidual;
            public int OpenEdgeCount;
            public int NonManifoldEdgeCount;
            public int TJunctionCount;
            public int InvalidFaceCount;
            public int NonPlanarFaceCount;
            public int NonConvexFaceCount;
            public int WindingFailureCount;
            public int BoundsValid;
            public int OutputVertexCount;
            public int OutputTriangleCount;
            public int BudgetValid;
            public double SourceVolume;
            public double ResultVolume;
            public double VolumeLoss;
            public double VolumeLossFraction;
            public int ExactConstructionFailureCount;
            public string ExactConstructionFailure = string.Empty;
            public int UntouchedOriginalEdgeCount;
            public int ShortenedDescendantEdgeCount;
            public int CapRingEdgeCount;
            public int MissingOriginalEdgeCount;
            public int AmbiguousIdentityCount;
            public int GeneratedIdentityCollisionCount;
            public readonly List<CornerDamageEdgeIdentityRecord>
                IdentityRecords =
                    new List<CornerDamageEdgeIdentityRecord>();
        }

        private sealed class CornerDamageTransactionAuditResult
        {
            public bool Attempted;
            public bool GraphAvailable;
            public bool CandidateFound;
            public bool Succeeded;
            public int ShapeSeed;
            public int NormalizedVertexCount;
            public int NormalizedEdgeCount;
            public int NormalizedFaceCount;
            public int EligibleCandidateCount;
            public int SelectedGraphVertexIndex = -1;
            public Vector3 SelectedPosition;
            public Vector3 OutwardNormal;
            public float BaseDepth;
            public float MinimumStableEdgeLength;
            public float MinimumStableFaceArea;
            public float MaximumDimension;
            public double SourceVolume;
            public int AcceptedTrialIndex = -1;
            public float AcceptedDepth;
            public float ShortestCapEdgeLength;
            public List<PolygonFace> AcceptedFaces;
            public PolygonFace AcceptedCapFace;
            public string Diagnostic = string.Empty;
            public readonly Dictionary<EdgeKey, int>
                StableIdentityByOutputKey =
                    new Dictionary<EdgeKey, int>();
            public readonly HashSet<EdgeKey> CapRingKeys =
                new HashSet<EdgeKey>();
            public readonly HashSet<int> CapRingGeneratedIdentities =
                new HashSet<int>();
            public readonly HashSet<int> AffectedOriginalEdgeIndices =
                new HashSet<int>();
            public readonly List<CornerDamageCandidateRecord> Candidates =
                new List<CornerDamageCandidateRecord>();
            public readonly List<CornerDamageTrialRecord> Trials =
                new List<CornerDamageTrialRecord>();
        }

        private sealed class CornerDamagePreviewConstructionRecord
        {
            public CornerDamageTransactionAuditResult Transaction;
            public float CapRingRequestedWidth;
            public int ExpectedMandatoryCount;
            public int MandatoryCandidateCount;
            public int MandatorySelectedCount;
            public UnifiedEdgeWearPreviewStatus CornerPreviewStatus;
            public string Blocker = string.Empty;
        }

        private sealed class EdgeWearMicroTopologyNormalizationResult
        {
            public List<PolygonFace> Faces;
            public EdgeWearTopologyGraph OriginalGraph;
            public bool Attempted;
            public bool Applied;
            public float Threshold;
            public float ComponentThreshold;
            public int OriginalVertexCount;
            public int OriginalEdgeCount;
            public int OriginalFaceCount;
            public int NormalizedVertexCount;
            public int NormalizedEdgeCount;
            public int NormalizedFaceCount;
            public int EligibleComponentCount;
            public int AppliedComponentCount;
            public int CandidateCollapseCount;
            public double OriginalVolume;
            public double NormalizedVolume;
            public double VolumeLoss;
            public double VolumeLossFraction;
            public double ElapsedMilliseconds;
            public string Diagnostic = string.Empty;
            public readonly Dictionary<EdgeKey, int>
                OriginalSourceEdgeIndexByNormalizedKey =
                    new Dictionary<EdgeKey, int>();
            public readonly Dictionary<int, int>
                OriginalSourceEdgeIndexByNormalizedGraphEdge =
                    new Dictionary<int, int>();
            public readonly HashSet<EdgeKey> GeneratedTransitionKeys =
                new HashSet<EdgeKey>();
            public readonly List<EdgeWearMicroTopologySuppressedEdge>
                SuppressedEdges =
                    new List<EdgeWearMicroTopologySuppressedEdge>();

            public readonly List<EdgeWearMicroTopologyComponentRecord>
                Components =
                    new List<EdgeWearMicroTopologyComponentRecord>();

            public int ResolveOriginalSourceEdgeIndex(
                EdgeKey key,
                int normalizedGraphEdgeIndex)
            {
                return OriginalSourceEdgeIndexByNormalizedKey.TryGetValue(
                        key,
                        out int originalIndex)
                    ? originalIndex
                    : normalizedGraphEdgeIndex;
            }
        }

private readonly struct EdgeWearSelectedGraphEdge
        {
            public readonly int GraphEdgeIndex;
            public readonly int CandidateIndex;
            public readonly EdgeWearBevelCandidate Candidate;

            public EdgeWearSelectedGraphEdge(
                int graphEdgeIndex,
                int candidateIndex,
                EdgeWearBevelCandidate candidate)
            {
                GraphEdgeIndex = graphEdgeIndex;
                CandidateIndex = candidateIndex;
                Candidate = candidate;
            }
        }
private struct EdgeWearGraphBuildStats
        {
            public int GraphVertexCount;
            public int GraphEdgeCount;
            public int GraphFaceCount;
            public int GraphBoundaryEdgeCount;
            public int GraphNonManifoldEdgeCount;
            public int CoincidentVertexReconciliationCount;
            public int CoincidentBoundarySeamPairCount;
            public int SelectedGraphEdgeCount;
            public int MissingSelectedGraphEdgeCount;
            public int MismatchedSelectedGraphFaceCount;
            public int DuplicateSelectedGraphEdgeCount;
            public int InvalidFaceCount;
            public int InvalidEdgeCount;
        }

        private enum EdgeWearViabilityState
        {
            Unassessed,
            StructuralIneligible,
            GeometricIneligible,
            CoexistenceIneligible,
            ViableUnselected,
            ViableSelected
        }

        private sealed class EdgeWearIsolatedWidthAttemptRecord
        {
            public int AttemptIndex;
            public float Width;
            public bool RailSolved;
            public string RailFailure = string.Empty;
            public bool ConstructionAttempted;
            public bool ConstructionSucceeded;
            public bool SinglePlaneAttempted;
            public bool SinglePlaneSucceeded;
            public string SinglePlaneFailure = string.Empty;
            public bool RetainedHullAttempted;
            public bool RetainedHullSucceeded;
            public string RetainedHullFailure = string.Empty;
            public bool Certified;
            public string FinalFailure = string.Empty;
        }

        private sealed class EdgeWearEdgeViabilityRecord
        {
            public EdgeKey Key;
            public int SourceEdgeIndex = -1;
            public bool Evaluated;
            public bool DihedralValid;
            public bool FootprintValid;
            public bool LocalityValid;
            public bool IsolatedConstructionValid;
            public bool FeasibleWidthFractionValid;
            public bool WidthRecoveryProvisional;
            public bool MaterialWidthRecoveryEligible;
            public float MaterialWidthRecoveryRequiredLength;
            public bool MultiSupportHullRecovery;
            public bool EndpointSpanValid;
            public bool Viable;
            public float MinimumDihedralDegrees;
            public float BaseRequestedWidth;
            public float MacroVariationCoverage;
            public float MacroVariation;
            public float MacroParticipationIdentity01;
            public bool MacroVariationParticipates;
            public float MacroIdentity01;
            public float MacroSampledMultiplier = 1f;
            public float MacroEffectiveMultiplier = 1f;
            public bool MacroMinimumStyleClamped;
            public float RequestedWidth;
            public float RequiredFootprintLength;
            public float LengthToWidthRatio;
            public float LocalityRetainPlaneFloor;
            public float LocalityRemovalPlaneCeiling;
            public float LocalityFeasibleMargin;
            public float LocalityGuardMargin;
            public float LocalityMinimumRemoval;
            public int LocalityLimitingVertex = -1;
            public Vector3 LocalityLimitingPosition;
            public float LocalityLimitingProjection;
            // Internal viability values retained for unchanged geometry decisions.
            public float MaximumLocallyFeasibleWidth;
            public float FeasibleWidthFraction;
            public float MinimumStyleWidth;
            public float MinimumRequiredCertifiedWidth;
            // Truthful audit semantics: failed attempts are not certified widths.
            public bool IsolatedSucceeded;
            public int IsolatedWidthAttemptCount;
            public float IsolatedLastAttemptedWidth;
            public bool IsolatedAttemptScheduleComplete;
            public bool IsolatedTerminalConstructionAtMinimum;
            public string IsolatedAttemptScheduleResolution =
                string.Empty;
            public string IsolatedWidthAttemptEvidence = string.Empty;
            public float IsolatedMaximumCertifiedWidth;
            public float IsolatedMaximumCertifiedWidthFraction;
            public int IsolatedAlternateBoundaryRailCount;
            public int IsolatedMaximumBoundaryCandidateCount;
            public float IsolatedMaximumBoundarySnapDistance;
            public float IsolatedMaximumBoundaryPointTolerance;
            public int IsolatedMaximumBoundaryDiagnosticRailIndex;
            public int IsolatedMaximumBoundaryOriginalAdjacentEdgeIndex;
            public int IsolatedMaximumBoundaryResolvedEdgeIndex;
            public float IsolatedMaximumBoundaryOriginalRawParameter;
            public float IsolatedMaximumBoundaryOriginalSegmentDistance;
            public float IsolatedMinimumBoundaryEndpointDistance;
            public float EndpointConsumptionA;
            public float EndpointConsumptionB;
            public float RemainingCentralSpan;
            public float MinimumCentralSpan;
            public int IsolatedOpenEdgeCount;
            public int IsolatedNonManifoldEdgeCount;
            public int IsolatedTJunctionCount;
            public int IsolatedInvalidFaceCount;
            public string IsolatedDiagnostic = string.Empty;
            public string FailureReason = string.Empty;
        }

        private sealed class EdgeWearCollateralBaselineRecord
        {
            public readonly EdgeKey Key;
            public readonly int SourceEdgeIndex;
            public readonly int OriginalSourceEdgeIndex;
            public readonly int FaceA;
            public readonly int FaceB;
            public readonly BoundedEdgeClassification Classification;
            public readonly float Length;
            public readonly float DihedralDegrees;
            public readonly float MaximumLocallyFeasibleWidth;
            public readonly float FeasibleWidthFraction;
            public readonly bool GeometricEligible;

            public EdgeWearCollateralBaselineRecord(
                EdgeWearEdgeLifecycleRecord record)
            {
                Key = record.Key;
                SourceEdgeIndex = record.SourceEdgeIndex;
                OriginalSourceEdgeIndex =
                    record.OriginalSourceEdgeIndex >= 0
                        ? record.OriginalSourceEdgeIndex
                        : record.SourceEdgeIndex;
                FaceA = Mathf.Min(record.FaceA, record.FaceB);
                FaceB = Mathf.Max(record.FaceA, record.FaceB);
                Classification = record.Classification;
                Length = record.Length;
                DihedralDegrees = record.DihedralDegrees;
                MaximumLocallyFeasibleWidth = record.Viability == null
                    ? 0f
                    : record.Viability.MaximumLocallyFeasibleWidth;
                FeasibleWidthFraction = record.Viability == null
                    ? 0f
                    : record.Viability.FeasibleWidthFraction;
                GeometricEligible = record.GeometricEligible;
            }
        }

        private sealed class EdgeWearCoverageAudit
        {
            public readonly bool MaximumCoverageMode;
            public readonly bool RequireAllGeometricCandidates;
            public readonly List<EdgeWearEdgeLifecycleRecord> Records =
                new List<EdgeWearEdgeLifecycleRecord>();
            public readonly Dictionary<EdgeKey, EdgeWearEdgeLifecycleRecord>
                RecordByKey =
                    new Dictionary<EdgeKey, EdgeWearEdgeLifecycleRecord>();
            public readonly Dictionary<int, EdgeWearEdgeLifecycleRecord>
                RecordByGraphEdge =
                    new Dictionary<int, EdgeWearEdgeLifecycleRecord>();
            public readonly Dictionary<EdgeKey, EdgeWearEdgeViabilityRecord>
                ViabilityByKey =
                    new Dictionary<EdgeKey, EdgeWearEdgeViabilityRecord>();
            public readonly Dictionary<int, EdgeWearEdgeViabilityRecord>
                ViabilityByGraphEdge =
                    new Dictionary<int, EdgeWearEdgeViabilityRecord>();
            public EdgeWearMicroTopologyNormalizationResult
                MicroTopologyNormalization;
            public float MacroVariationCoverage;
            public float MacroVariation;
            public float MacroBaseRequestedWidth;
            public int RawSourceEdgeCount;
            public int SourceEdgeCount;
            public int CoincidentBoundarySeamPairCount;
            public int CoincidentGraphVertexReconciliationCount;
            public int CoincidentGraphBoundarySeamPairCount;
            public bool CollateralBaselineCaptured;
            public readonly Dictionary<EdgeKey,
                EdgeWearCollateralBaselineRecord> CollateralBaselineByKey =
                    new Dictionary<EdgeKey,
                        EdgeWearCollateralBaselineRecord>();
            public int BaselineGeometricEligibleCount;
            public int RecoveredGeometricEdgeCount;
            public int CollateralLostEdgeCount;
            public int CollateralChangedEdgeCount;
            public bool CollateralPreservationValid = true;
            public readonly List<int> RecoveredGeometricEdgeIndices =
                new List<int>();
            public readonly List<int> CollateralLostEdgeIndices =
                new List<int>();
            public readonly List<int> CollateralChangedEdgeIndices =
                new List<int>();
            public int ViabilityLocalityEvaluationCount;
            public int ViabilityIsolatedEvaluationCount;
            public int ViabilityLocalityCacheUseCount;
            public int ViabilityLocalityCacheMissCount;
            public int ViabilityLocalityRecomputationCount;
            public double ViabilityPreflightMilliseconds;
            public int StructuralEligibleCount;
            public int GeometricEligibleCount;
            public int GeometricIneligibleCount;
            public int CoexistenceEligibleCount;
            public int CoexistenceIneligibleCount;
            public int CoexistenceStarEvaluationCount;
            public int CoexistenceStarCacheUseCount;
            public int CoexistencePairEvaluationCount;
            public int CoexistencePairCacheUseCount;
            public int CoexistenceTrialCount;
            public int CoexistenceExclusionCount;
            public int CoexistencePreShellExclusionCount;
            public int CoexistenceSearchExclusionCount;
            public int CornerWidthMissingExclusionCount;
            public int CornerWidthInactiveExclusionCount;
            public int ArtisticEligibleCount;
            public int ArtisticFilteredCount;
            public bool ArtisticAuditCaptured;
            public int ArtisticSelectionTargetCount;
            public float ArtisticSelectionThreshold;
            public int CandidateCount;
            public int SelectedCount;
            public int WidthInactiveCount;
            public int UnresolvedWidthInactiveCount;
            public int WidthReducedCount;
            public int ActiveCount;
            public int AttemptedBuiltCount;
            public int BuiltCount;
            public int TrialRejectedCount;
            public int DeferredCount;
            public int RejectedCount;
            public int UnmappedCount;

            public EdgeWearCoverageAudit(
                bool maximumCoverageMode,
                bool requireAllGeometricCandidates)
            {
                MaximumCoverageMode = maximumCoverageMode;
                RequireAllGeometricCandidates =
                    requireAllGeometricCandidates;
            }

            public EdgeWearCoverageAudit CloneForTrial()
            {
                EdgeWearCoverageAudit clone = new EdgeWearCoverageAudit(
                    MaximumCoverageMode,
                    RequireAllGeometricCandidates)
                {
                    MicroTopologyNormalization =
                        MicroTopologyNormalization,
                    MacroVariationCoverage =
                        MacroVariationCoverage,
                    MacroVariation = MacroVariation,
                    MacroBaseRequestedWidth = MacroBaseRequestedWidth,
                    RawSourceEdgeCount = RawSourceEdgeCount,
                    SourceEdgeCount = SourceEdgeCount,
                    CoincidentBoundarySeamPairCount =
                        CoincidentBoundarySeamPairCount,
                    CoincidentGraphVertexReconciliationCount =
                        CoincidentGraphVertexReconciliationCount,
                    CoincidentGraphBoundarySeamPairCount =
                        CoincidentGraphBoundarySeamPairCount,
                    CollateralBaselineCaptured =
                        CollateralBaselineCaptured,
                    BaselineGeometricEligibleCount =
                        BaselineGeometricEligibleCount,
                    RecoveredGeometricEdgeCount =
                        RecoveredGeometricEdgeCount,
                    CollateralLostEdgeCount = CollateralLostEdgeCount,
                    CollateralChangedEdgeCount =
                        CollateralChangedEdgeCount,
                    CollateralPreservationValid =
                        CollateralPreservationValid,
                    ViabilityLocalityEvaluationCount =
                        ViabilityLocalityEvaluationCount,
                    ViabilityIsolatedEvaluationCount =
                        ViabilityIsolatedEvaluationCount,
                    ViabilityLocalityCacheUseCount =
                        ViabilityLocalityCacheUseCount,
                    ViabilityLocalityCacheMissCount =
                        ViabilityLocalityCacheMissCount,
                    ViabilityLocalityRecomputationCount =
                        ViabilityLocalityRecomputationCount,
                    ViabilityPreflightMilliseconds =
                        ViabilityPreflightMilliseconds,
                    StructuralEligibleCount = StructuralEligibleCount,
                    GeometricEligibleCount = GeometricEligibleCount,
                    GeometricIneligibleCount = GeometricIneligibleCount,
                    CoexistenceEligibleCount = CoexistenceEligibleCount,
                    CoexistenceIneligibleCount =
                        CoexistenceIneligibleCount,
                    CoexistenceStarEvaluationCount =
                        CoexistenceStarEvaluationCount,
                    CoexistenceStarCacheUseCount =
                        CoexistenceStarCacheUseCount,
                    CoexistencePairEvaluationCount =
                        CoexistencePairEvaluationCount,
                    CoexistencePairCacheUseCount =
                        CoexistencePairCacheUseCount,
                    CoexistenceTrialCount = CoexistenceTrialCount,
                    CoexistenceExclusionCount =
                        CoexistenceExclusionCount,
                    CoexistencePreShellExclusionCount =
                        CoexistencePreShellExclusionCount,
                    CoexistenceSearchExclusionCount =
                        CoexistenceSearchExclusionCount,
                    CornerWidthMissingExclusionCount =
                        CornerWidthMissingExclusionCount,
                    CornerWidthInactiveExclusionCount =
                        CornerWidthInactiveExclusionCount,
                    ArtisticEligibleCount = ArtisticEligibleCount,
                    ArtisticFilteredCount = ArtisticFilteredCount,
                    ArtisticAuditCaptured = ArtisticAuditCaptured,
                    ArtisticSelectionTargetCount =
                        ArtisticSelectionTargetCount,
                    ArtisticSelectionThreshold =
                        ArtisticSelectionThreshold,
                    CandidateCount = CandidateCount,
                    SelectedCount = SelectedCount,
                    WidthInactiveCount = WidthInactiveCount,
                    UnresolvedWidthInactiveCount =
                        UnresolvedWidthInactiveCount,
                    WidthReducedCount = WidthReducedCount,
                    ActiveCount = ActiveCount,
                    AttemptedBuiltCount = AttemptedBuiltCount,
                    BuiltCount = BuiltCount,
                    TrialRejectedCount = TrialRejectedCount,
                    DeferredCount = DeferredCount,
                    RejectedCount = RejectedCount,
                    UnmappedCount = UnmappedCount
                };

                for (int recordIndex = 0;
                     recordIndex < Records.Count;
                     recordIndex++)
                {
                    EdgeWearEdgeLifecycleRecord copiedRecord =
                        Records[recordIndex].CloneForTrial();
                    clone.Records.Add(copiedRecord);
                    clone.RecordByKey[copiedRecord.Key] = copiedRecord;
                    if (copiedRecord.SourceEdgeIndex >= 0)
                    {
                        clone.RecordByGraphEdge[
                            copiedRecord.SourceEdgeIndex] = copiedRecord;
                    }
                    if (copiedRecord.Viability != null)
                    {
                        clone.ViabilityByKey[copiedRecord.Key] =
                            copiedRecord.Viability;
                        if (copiedRecord.SourceEdgeIndex >= 0)
                        {
                            clone.ViabilityByGraphEdge[
                                copiedRecord.SourceEdgeIndex] =
                                    copiedRecord.Viability;
                        }
                    }
                }

                foreach (KeyValuePair<EdgeKey,
                    EdgeWearCollateralBaselineRecord> pair in
                    CollateralBaselineByKey)
                {
                    clone.CollateralBaselineByKey[pair.Key] = pair.Value;
                }
                clone.RecoveredGeometricEdgeIndices.AddRange(
                    RecoveredGeometricEdgeIndices);
                clone.CollateralLostEdgeIndices.AddRange(
                    CollateralLostEdgeIndices);
                clone.CollateralChangedEdgeIndices.AddRange(
                    CollateralChangedEdgeIndices);
                return clone;
            }
        }

        private sealed class EdgeWearEdgeLifecycleRecord
        {
            public EdgeKey Key;
            public int SourceEdgeIndex = -1;
            public int OriginalSourceEdgeIndex = -1;
            public EdgeWearCandidateClass CandidateClass =
                EdgeWearCandidateClass.Ordinary;
            public bool Mandatory;
            public bool MicroTopologySuppressed;
            public bool MicroTopologyGeneratedTransition;
            public int CandidateIndex = -1;
            public Vector3 Start;
            public Vector3 End;
            public Vector3 Midpoint;
            public Vector3 OwnerNormalA;
            public Vector3 OwnerNormalB;
            public Vector3 BevelNormal;
            public int FaceA = -1;
            public int FaceB = -1;
            public int FaceCount;
            public float Length;
            public float DihedralDegrees;
            public float Vertical01;
            public float Score;
            public float ArtisticMinimumLength;
            public float ArtisticLengthScore;
            public float ArtisticAngleScore;
            public float ArtisticBaseSuppression;
            public float ArtisticUpwardEdgeBoost = 1f;
            public float ArtisticCharacterBoost = 1f;
            public float ArtisticRandomScore;
            public float ArtisticEdgeAxisVertical01;
            public float ArtisticEdgeAxisAbsX;
            public float ArtisticEdgeAxisAbsY;
            public float ArtisticEdgeAxisAbsZ;
            public float ArtisticSilhouettePotential;
            public float ArtisticFeasibleWidthFraction;
            public float ArtisticSolvedWidthFraction;
            public float ArtisticLocalDensity01;
            public int ArtisticSharedVertexDegreeA;
            public int ArtisticSharedVertexDegreeB;
            public int ArtisticSelectionRank = -1;
            public float ArtisticSelectionThreshold;
            public float ArtisticSelectionDelta;
            public bool ArtisticLengthEligible;
            public bool ArtisticAngleEligible;
            public bool ArtisticBaseEligible;
            public string ArtisticFilterReason = string.Empty;
            public float ArtisticDeterministicVariation;
            public float ArtisticStrength;
            public float ArtisticDepthMultiplier;
            public float SolvedWidth;
            public float MaterializedWidth;
            public float MaterializedWidthScale = 1f;
            public bool WidthReduced;
            public BoundedEdgeClassification Classification =
                BoundedEdgeClassification.None;
            public bool CoincidentBoundarySeamReconciled;
            public bool StructuralEligible;
            public bool GeometricEligible;
            public bool CoexistenceEligible;
            public bool ArtisticEligible;
            public EdgeWearViabilityState ViabilityState =
                EdgeWearViabilityState.Unassessed;
            public EdgeWearEdgeViabilityRecord Viability;
            public bool Candidate;
            public bool Selected;
            public bool WidthInactive;
            public bool MaterialWidthRecoveryTarget;
            public bool MaterialWidthRecoveryBaselineDeferred;
            public bool RecoveryBaselineDeferred;
            public bool MaterialWidthRecoveryAttempted;
            public bool MaterialWidthRecoveryTrialCompleted;
            public bool MaterialWidthRecoveryTrialSucceeded;
            public bool MaterialWidthRecoveryCertified;
            public string MaterialWidthRecoveryFailure = string.Empty;
            public string WidthRecoveryResolution = string.Empty;
            public string WidthRecoveryEvidence = string.Empty;
            public bool CornerRecoveryProvisional;
            public int CornerRecoveryCollapsedSourceEdgeIndex = -1;
            public float CornerRecoveryLastPositiveWidth;
            public float CornerRecoveryUniformScale;
            public ChamferCornerZeroingStage CornerRecoveryZeroingStage;
            public string CornerRecoveryParticipants = string.Empty;
            public string CornerRecoveryZeroedParticipants = string.Empty;
            public string CornerRecoveryResolution = string.Empty;
            public bool Active;
            public bool AttemptedBuilt;
            public bool Built;
            public bool TrialRejected;
            public bool Deferred;
            public bool Rejected;
            public string CandidateReason = string.Empty;
            public string CoexistenceFailureReason = string.Empty;
            public string FinalReason = string.Empty;

            public EdgeWearEdgeLifecycleRecord CloneForTrial()
            {
                return (EdgeWearEdgeLifecycleRecord)MemberwiseClone();
            }
        }

        private enum EdgeWearCandidateClass
        {
            Ordinary,
            CornerDamageCapRing
        }

        private readonly struct EdgeWearBevelCandidate
        {
            public readonly int CandidateIndex;
            public readonly int StableIdentity;
            public readonly EdgeWearCandidateClass CandidateClass;
            public readonly bool Mandatory;
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly int FaceA;
            public readonly int FaceB;
            public readonly Vector3 NormalA;
            public readonly Vector3 NormalB;
            public readonly Vector3 Midpoint;
            public readonly Vector3 BevelNormal;
            public readonly float Score;
            public readonly float Strength;
            public readonly float DepthMultiplier;

            public EdgeWearBevelCandidate(
                int candidateIndex,
                int stableIdentity,
                EdgeWearCandidateClass candidateClass,
                bool mandatory,
                Vector3 start,
                Vector3 end,
                int faceA,
                int faceB,
                Vector3 normalA,
                Vector3 normalB,
                Vector3 midpoint,
                Vector3 bevelNormal,
                float score,
                float strength,
                float depthMultiplier)
            {
                CandidateIndex = candidateIndex;
                StableIdentity = stableIdentity;
                CandidateClass = candidateClass;
                Mandatory = mandatory;
                Start = start;
                End = end;
                FaceA = faceA;
                FaceB = faceB;
                NormalA = normalA;
                NormalB = normalB;
                Midpoint = midpoint;
                BevelNormal = bevelNormal;
                Score = score;
                Strength = strength;
                DepthMultiplier = depthMultiplier;
            }
        }

        #endregion
    }
}
