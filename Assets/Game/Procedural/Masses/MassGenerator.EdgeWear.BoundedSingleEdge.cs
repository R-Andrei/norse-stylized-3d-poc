using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Bounded single-edge bevel prototype

        private const int BoundedIsolatedMaximumWidthAttempts = 12;
        private const float BoundedIsolatedWidthBackoff = 0.75f;
        private const float OneSurfaceMinimumRenderNormalDot = 0.5f;

        private struct BoundedSingleEdgeAuditResult
        {
            public int CandidateCount;
            public int SelectedOrdinal;
            public int SourceEdgeIndex;
            public int IsolatedRailSolved;
            public int WidthAttemptCount;
            public float IsolatedMinimumWidth;
            public int IsolatedAttemptScheduleComplete;
            public int IsolatedTerminalConstructionAtMinimum;
            public string IsolatedAttemptScheduleResolution;
            public List<EdgeWearIsolatedWidthAttemptRecord>
                IsolatedWidthAttempts;
            public int TargetBoundaryCount;
            public float SolvedWidth;
            public float EndpointConsumptionA;
            public float EndpointConsumptionB;
            public float RemainingCentralSpan;
            public float MinimumCentralSpan;
            public int CanonicalRailCount;
            public int AlternateBoundaryRailCount;
            public int MaximumBoundaryCandidateCount;
            public float MaximumBoundarySnapDistance;
            public float MaximumBoundaryPointTolerance;
            public int MaximumBoundaryDiagnosticRailIndex;
            public int MaximumBoundaryOriginalAdjacentEdgeIndex;
            public int MaximumBoundaryResolvedEdgeIndex;
            public float MaximumBoundaryOriginalRawParameter;
            public float MaximumBoundaryOriginalSegmentDistance;
            public float MinimumBoundaryEndpointDistance;
            public int OwnerClipAttemptedCount;
            public int OwnerClipCount;
            public int OwnerIntersectionFailureCount;
            public int OwnerDegenerateCount;
            public int OwnerNonPlanarCount;
            public int OwnerNonSimpleCount;
            public int OwnerNonConvexCount;
            public int OwnerWindingFailureCount;
            public int EndpointSupportClipAttemptedCount;
            public int EndpointSupportClipCount;
            public int EndpointSupportFaceA;
            public int EndpointSupportFaceB;
            public int EndpointSupportAdditionalFaceA;
            public int EndpointSupportAdditionalFaceB;
            public int EndpointSupportMultiFaceEndpointCount;
            public int EndpointSupportModifiedFaceExpectedCount;
            public int EndpointSupportCapFaceCount;
            public int EndpointSupportBoundaryPathVertexCount;
            public int MultiSupportPlaneCut;
            public int MultiSupportSinglePlaneAttempted;
            public int MultiSupportSinglePlaneSucceeded;
            public string MultiSupportSinglePlaneFailure;
            public int MultiSupportRetainedHullAttempted;
            public int MultiSupportRetainedHullSucceeded;
            public string MultiSupportRetainedHullFailure;
            public int MultiSupportPlaneCount;
            public int MultiSupportPlaneChainCandidateCount;
            public int MultiSupportPlaneForeignVertexRejectCount;
            public int MultiSupportPlaneFirstForeignVertex;
            public float MultiSupportPlaneMaximumForeignDistance;
            public float MultiSupportPlaneSplitA;
            public float MultiSupportPlaneSplitB;
            public int MultiSupportPlaneCapAdjacencyCount;
            public Vector3 MultiSupportPlaneNormal;
            public float MultiSupportPlaneDistance;
            public Vector3 MultiSupportPlaneNormalB;
            public float MultiSupportPlaneDistanceB;
            public int MultiSupportHullPointCount;
            public int MultiSupportHullPlaneCount;
            public int MultiSupportHullBevelFaceCount;
            public int MultiSupportHullTriplesTested;
            public int MultiSupportHullSupportingTriples;
            public string MultiSupportHullEvidence;
            public int EndpointSupportGraphFaceA;
            public int EndpointSupportGraphFaceB;
            public int EndpointSupportVertexA;
            public int EndpointSupportVertexB;
            public int EndpointSupportPreviousEdgeA;
            public int EndpointSupportPreviousEdgeB;
            public int EndpointSupportNextEdgeA;
            public int EndpointSupportNextEdgeB;
            public int EndpointSupportPreviousRailA;
            public int EndpointSupportPreviousRailB;
            public int EndpointSupportNextRailA;
            public int EndpointSupportNextRailB;
            public Vector3 EndpointSupportSourcePositionA;
            public Vector3 EndpointSupportSourcePositionB;
            public Vector3 EndpointSupportPreviousRailPositionA;
            public Vector3 EndpointSupportPreviousRailPositionB;
            public Vector3 EndpointSupportNextRailPositionA;
            public Vector3 EndpointSupportNextRailPositionB;
            public Vector3 EndpointSupportNormalA;
            public Vector3 EndpointSupportNormalB;
            public float EndpointSupportPreviousParameterA;
            public float EndpointSupportPreviousParameterB;
            public float EndpointSupportNextParameterA;
            public float EndpointSupportNextParameterB;
            public float EndpointSupportPreviousEdgeResidualA;
            public float EndpointSupportPreviousEdgeResidualB;
            public float EndpointSupportNextEdgeResidualA;
            public float EndpointSupportNextEdgeResidualB;
            public float EndpointSupportPreviousPlaneResidualA;
            public float EndpointSupportPreviousPlaneResidualB;
            public float EndpointSupportNextPlaneResidualA;
            public float EndpointSupportNextPlaneResidualB;
            public int EndpointSupportSharedFaceFailureCount;
            public int EndpointSupportIncidenceFailureCount;
            public int EndpointSupportDegenerateCount;
            public int EndpointSupportNonPlanarCount;
            public int EndpointSupportNonSimpleCount;
            public int EndpointSupportNonConvexCount;
            public int EndpointSupportWindingFailureCount;
            public int EndpointSupportRemovedVertexCount;
            public int EndpointSupportRailInsertionCount;
            public int BoundarySubdivisionCount;
            public int BevelFaceCount;
            public int EndpointCapCount;
            public int ModifiedSourceFaceCount;
            public int OwnerSourceFaceModifiedCount;
            public int EndpointSupportSourceFaceModifiedCount;
            public int UnexpectedSourceFaceModifiedCount;
            public int BoundaryOnlyUnexpectedSourceFaceCount;
            public int ForeignSourceFaceModifiedCount;
            public int ForeignBoundarySubdividedCount;
            public int PreparedSourceChangeComparisonAttempted;
            public int RawModifiedSourceFaceCount;
            public int RawOwnerSourceFaceModifiedCount;
            public int RawEndpointSupportSourceFaceModifiedCount;
            public int RawUnexpectedSourceFaceModifiedCount;
            public int RawBoundaryOnlyUnexpectedSourceFaceCount;
            public int RawForeignSourceFaceModifiedCount;
            public int RawForeignBoundarySubdividedCount;
            public BoundedSourceProvenanceAudit RawSourceProvenance;
            public BoundedSourceProvenanceAudit PreparedSourceProvenance;
            public BoundedSourceProvenanceAudit ResultSourceProvenance;
            public int SourceProvenanceCertificationValid;
            public float RailDeviation;
            public float MaximumExtentBeyondRails;
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
            public int PrepareInputFaceCount;
            public int PrepareWelded;
            public int PrepareConformedCount;
            public int PrepareSeamRepairCount;
            public string PrepareFailedStage;
            public int PrepareFailedFace;
            public BoundedPolygonFailure PrepareFailedKind;
            public PolygonFaceProvenanceKind PrepareFailedProvenanceKind;
            public int PrepareFailedProvenanceIndex;
            public int PrepareDegenerateCount;
            public int PrepareNonPlanarCount;
            public int PrepareNonSimpleCount;
            public int PrepareNonConvexCount;
            public int PrepareWindingFailureCount;
            public int PrepareFailedCanonicalSubdivision;
            public BoundedPreparationAudit ResultPreparation;
            public BoundedPreparationAudit SourcePreparation;
            public int CertificationAttempted;
            public int FacesReoriented;
            public int OutwardWindingFailureCount;
            public int BoundsValid;
            public int PreparedBoundsValid;
            public float BoundsTolerance;
            public Vector3 RawSourceBoundsMinimum;
            public Vector3 RawSourceBoundsMaximum;
            public Vector3 PreparedSourceBoundsMinimum;
            public Vector3 PreparedSourceBoundsMaximum;
            public Vector3 ResultBoundsMinimum;
            public Vector3 ResultBoundsMaximum;
            public Vector3 RawBoundsMinimumMargin;
            public Vector3 RawBoundsMaximumMargin;
            public Vector3 PreparedBoundsMinimumMargin;
            public Vector3 PreparedBoundsMaximumMargin;
            public int VolumeValid;
            public double SourceVolume;
            public double PreparedSourceVolume;
            public double ResultVolume;
            public double RawVolumeRatio;
            public double VolumeRatio;
            public double SourcePreparationVolumeRatio;
            public double RawVolumeDelta;
            public double PreparedVolumeDelta;
            public double VolumeMinimumRatio;
            public double VolumeMaximumRatio;
            public double VolumeLowerMargin;
            public double VolumeUpperMargin;
            public int TriangulatedFaceCount;
            public int PolygonSurfaceFaceCount;
            public int PolygonSurfaceBoundaryVertexCount;
            public int PolygonSurfaceExpectedTriangleCount;
            public int PolygonSurfaceTriangleCount;
            public int PolygonSurfaceAuthoredNormalTriangleCount;
            public int PolygonSurfaceAuthoredSurfaceGroupTriangleCount;
            public int PolygonSurfaceInternalFanVertexCount;
            public int PolygonSurfaceGroupCollisionCount;
            public int PolygonSurfaceGroupCollisionSurfaceGroup;
            public int PolygonSurfaceGroupCollisionFirstFace;
            public int PolygonSurfaceGroupCollisionSecondFace;
            public float PolygonSurfaceMaximumPlaneResidual;
            public float PolygonSurfaceMaximumNormalDeviationDegrees;
            public int PolygonSurfaceRenderValid;
            public int PolygonSurfaceFailureFace;
            public int PolygonSurfaceFailureProvenanceIndex;
            public string PolygonSurfaceFailureReason;
            public int BevelRegionFaceCount;
            public int BevelRegionBoundaryVertexCount;
            public int BevelRegionTriangleCount;
            public int BevelRegionAuthoredNormalTriangleCount;
            public int BevelRegionAuthoredSurfaceGroupTriangleCount;
            public int BevelRegionInternalFanVertexCount;
            public float BevelRegionMaximumPlaneResidual;
            public float BevelRegionMaximumNormalDeviationDegrees;
            public int BevelRegionRenderValid;
            public int BevelRegionFailureFace;
            public int BevelRegionFailureProvenanceIndex;
            public string BevelRegionFailureReason;
            public int TriangulationFailureFace;
            public BoundedPolygonFailure TriangulationFailureKind;
            public PolygonFaceProvenanceKind
                TriangulationFailureProvenanceKind;
            public int TriangulationFailureProvenanceIndex;
            public string TriangulationFailureReason;
            public int EdgeClassificationAttempted;
            public BoundedEdgeClassification EdgeClassification;
            public int EdgeSourceFaceA;
            public int EdgeSourceFaceB;
            public Vector3 EdgeNormalA;
            public Vector3 EdgeNormalB;
            public float EdgeNormalDot;
            public float EdgeDihedralDegrees;
            public float EdgeFaceAInteriorAgainstFaceB;
            public float EdgeFaceBInteriorAgainstFaceA;
            public float EdgeSolidCentreAgainstFaceA;
            public float EdgeSolidCentreAgainstFaceB;
            public float EdgeClassificationTolerance;
            public int ConvexCandidateCount;
            public int ConcaveCandidateCount;
            public int CoplanarCandidateCount;
            public int AmbiguousCandidateCount;
            public int InvalidOrientationCandidateCount;
            public int SourceConvexityAttempted;
            public int SourceConvexityViolationCount;
            public float SourceMaximumPlaneViolation;
            public int SourceViolatingPlaneFace;
            public int SourceViolatingVertexFace;
            public int SourceViolatingVertexIndex;
            public int ResultContainmentAttempted;
            public int ResultContainmentViolationCount;
            public float ResultMaximumOutwardDistance;
            public int ResultViolatingFace;
            public PolygonFaceProvenanceKind ResultViolatingProvenanceKind;
            public int ResultViolatingProvenanceIndex;
            public int ResultViolatingVertexIndex;
            public int ResultViolatedSourcePlane;
            public float SolidContainmentTolerance;
            public int ResultConvexityAttempted;
            public int ResultConvexityViolationCount;
            public float ResultMaximumConvexityViolation;
            public int ResultConvexityPlaneFace;
            public PolygonFaceProvenanceKind
                ResultConvexityPlaneProvenanceKind;
            public int ResultConvexityPlaneProvenanceIndex;
            public int ResultConvexityVertexFace;
            public PolygonFaceProvenanceKind
                ResultConvexityVertexProvenanceKind;
            public int ResultConvexityVertexProvenanceIndex;
            public int ResultConvexityVertexIndex;
            public int FaceIntersectionAttempted;
            public int FaceIntersectionPairCount;
            public int CoplanarOverlapPairCount;
            public int NonCoplanarIntersectionPairCount;
            public int FirstIntersectionFaceA;
            public PolygonFaceProvenanceKind
                FirstIntersectionFaceAProvenanceKind;
            public int FirstIntersectionFaceAProvenanceIndex;
            public int FirstIntersectionFaceB;
            public PolygonFaceProvenanceKind
                FirstIntersectionFaceBProvenanceKind;
            public int FirstIntersectionFaceBProvenanceIndex;
            public int SourceFaceIntersectionAttempted;
            public int SourceFaceIntersectionPairCount;
            public int SourceCoplanarOverlapPairCount;
            public int SourceNonCoplanarIntersectionPairCount;
            public int SourceBoundaryContactPairCount;
            public int SourceImproperInteriorPairCount;
            public int ResultBoundaryContactPairCount;
            public int ResultImproperInteriorPairCount;
            public int UnchangedIntersectionPairCount;
            public int ChangedIntersectionPairCount;
            public int NewIntersectionPairCount;
            public int NewBoundaryContactPairCount;
            public int NewImproperInteriorIntersectionPairCount;
            public int ChangedImproperInteriorIntersectionPairCount;
            public int IntroducedImproperInteriorIntersectionPairCount;
            public int ResolvedIntersectionPairCount;
            public string SourceIntersectionPairEvidence;
            public string ResultIntersectionPairEvidence;
            public string UnchangedIntersectionPairEvidence;
            public string ChangedIntersectionPairEvidence;
            public string NewIntersectionPairEvidence;
            public string ResolvedIntersectionPairEvidence;
            public int LocalVolumeAttempted;
            public double SourceSignedVolume;
            public double PreparedSourceSignedVolume;
            public double ResultSignedVolume;
            public double ResultAbsoluteVolume;
            public double OriginalOwnerAContribution;
            public double OriginalOwnerBContribution;
            public double OriginalOwnerContribution;
            public double ReplacementOwnerAContribution;
            public double ReplacementOwnerBContribution;
            public double ReplacementOwnerContribution;
            public double OriginalSupportAContribution;
            public double OriginalSupportBContribution;
            public double OriginalSupportContribution;
            public double ReplacementSupportAContribution;
            public double ReplacementSupportBContribution;
            public double ReplacementSupportContribution;
            public double BevelContribution;
            public double CapAContribution;
            public double CapBContribution;
            public double OriginalForeignContribution;
            public double ResultForeignContribution;
            public double ForeignContributionDelta;
            public double LocalReplacementDelta;
            public double GlobalSignedVolumeDelta;
            public double LocalGlobalResidual;
            public int BevelPlaneAttempted;
            public Vector3 BevelPlaneNormal;
            public Vector3 BevelFaceNormal;
            public float BevelPlaneNormalAgreement;
            public float BevelPlaneDistance;
            public float BevelSolidCentreSide;
            public float BevelSourceEdgeASide;
            public float BevelSourceEdgeBSide;
            public float BevelRailMaximumPlaneResidual;
            public int DiagnosticTriangulationAttempted;
            public int DiagnosticTriangleSoupValid;
            public double DiagnosticTriangleSignedVolume;
            public double DiagnosticTriangleVolume;
            public double PolygonTriangleVolumeDelta;
            public double PolygonTriangleSignedVolumeDelta;
            public int GeometryValid;
            public string Diagnostic;
        }

        private struct BoundedPreparationAudit
        {
            public int Attempted;
            public int Succeeded;
            public int InputFaceCount;
            public int InputVertexCount;
            public int InputUniqueVertexCount;
            public int OutputFaceCount;
            public int OutputVertexCount;
            public int OutputUniqueVertexCount;
            public int Welded;
            public int ConformedCount;
            public int SeamRepairCount;
            public int SeamTouchedFaceCount;
            public int InputOpenEdgeCount;
            public int InputNonManifoldEdgeCount;
            public int InputTJunctionCount;
            public int InputInvalidFaceCount;
            public int OutputOpenEdgeCount;
            public int OutputNonManifoldEdgeCount;
            public int OutputTJunctionCount;
            public int OutputInvalidFaceCount;
            public double InputVolume;
            public double OutputVolume;
            public double VolumeDelta;
            public double VolumeRatio;
            public string FailedStage;
            public int FailedFace;
            public BoundedPolygonFailure FailedKind;
            public PolygonFaceProvenanceKind FailedProvenanceKind;
            public int FailedProvenanceIndex;
            public int DegenerateCount;
            public int NonPlanarCount;
            public int NonSimpleCount;
            public int NonConvexCount;
            public int WindingFailureCount;
        }

        private struct BoundedSourceFaceChangeAudit
        {
            public int Attempted;
            public int ModifiedSourceFaceCount;
            public int OwnerSourceFaceModifiedCount;
            public int EndpointSupportSourceFaceModifiedCount;
            public int UnexpectedSourceFaceModifiedCount;
            public int BoundaryOnlyUnexpectedSourceFaceCount;
            public int ForeignSourceFaceModifiedCount;
            public int ForeignBoundarySubdividedCount;
        }

        private struct BoundedSourceProvenanceAudit
        {
            public int Attempted;
            public int Valid;
            public int ExpectedSourceFaceCount;
            public int TotalFaceCount;
            public int SourceProvenanceFaceCount;
            public int ValidUniqueSourceFaceCount;
            public int MissingSourceFaceCount;
            public int DuplicateSourceFaceCount;
            public int OutOfRangeSourceFaceCount;
            public int NonSourceFaceCount;
            public int NullFaceCount;
            public int FirstMissingSourceFace;
            public int FirstDuplicateSourceFace;
            public int FirstOutOfRangeSourceFace;
        }

        private readonly struct BoundedIntersectionFaceKey :
            IEquatable<BoundedIntersectionFaceKey>
        {
            public readonly PolygonFaceProvenanceKind Kind;
            public readonly int Index;

            public BoundedIntersectionFaceKey(
                PolygonFaceProvenanceKind kind,
                int index)
            {
                Kind = kind;
                Index = index;
            }

            public bool Equals(BoundedIntersectionFaceKey other)
            {
                return Kind == other.Kind && Index == other.Index;
            }

            public override bool Equals(object obj)
            {
                return obj is BoundedIntersectionFaceKey other &&
                    Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((int)Kind * 397) ^ Index;
                }
            }
        }

        private readonly struct BoundedFaceIntersectionPairEvidence
        {
            public readonly BoundedIntersectionFaceKey First;
            public readonly BoundedIntersectionFaceKey Second;
            public readonly int FirstFaceIndex;
            public readonly int SecondFaceIndex;
            public readonly int Coplanar;
            public readonly int SharedVertexCount;
            public readonly int SharedBoundaryEdgeCount;
            public readonly int GraphAdjacent;
            public readonly int BoundaryContact;
            public readonly int ImproperInterior;

            public BoundedFaceIntersectionPairEvidence(
                BoundedIntersectionFaceKey first,
                BoundedIntersectionFaceKey second,
                int firstFaceIndex,
                int secondFaceIndex,
                int coplanar,
                int sharedVertexCount,
                int sharedBoundaryEdgeCount,
                int graphAdjacent,
                int boundaryContact,
                int improperInterior)
            {
                First = first;
                Second = second;
                FirstFaceIndex = firstFaceIndex;
                SecondFaceIndex = secondFaceIndex;
                Coplanar = coplanar;
                SharedVertexCount = sharedVertexCount;
                SharedBoundaryEdgeCount = sharedBoundaryEdgeCount;
                GraphAdjacent = graphAdjacent;
                BoundaryContact = boundaryContact;
                ImproperInterior = improperInterior;
            }
        }

        private struct BoundedFaceIntersectionAudit
        {
            public int Attempted;
            public int PairCount;
            public int CoplanarPairCount;
            public int NonCoplanarPairCount;
            public int BoundaryContactPairCount;
            public int ImproperInteriorPairCount;
            public int FirstFaceA;
            public PolygonFaceProvenanceKind FirstFaceAProvenanceKind;
            public int FirstFaceAProvenanceIndex;
            public int FirstFaceB;
            public PolygonFaceProvenanceKind FirstFaceBProvenanceKind;
            public int FirstFaceBProvenanceIndex;
            public List<BoundedFaceIntersectionPairEvidence> Pairs;
            public string PairEvidence;
        }

        private enum BoundedOwnerClipFailure
        {
            None,
            Intersection,
            Degenerate,
            NonPlanar,
            NonSimple,
            NonConvex,
            Winding
        }

        private enum BoundedPolygonFailure
        {
            None,
            Degenerate,
            NonFinite,
            NonPlanar,
            NonSimple,
            NonConvex,
            Winding
        }

        private enum BoundedEdgeClassification
        {
            None,
            Convex,
            Concave,
            Coplanar,
            Ambiguous,
            InvalidOrientation
        }

        private struct BoundedEdgeClassificationEvidence
        {
            public int SourceFaceA;
            public int SourceFaceB;
            public Vector3 NormalA;
            public Vector3 NormalB;
            public float NormalDot;
            public float DihedralDegrees;
            public float FaceAInteriorAgainstFaceB;
            public float FaceBInteriorAgainstFaceA;
            public float SolidCentreAgainstFaceA;
            public float SolidCentreAgainstFaceB;
            public BoundedEdgeClassification Classification;
        }

        private readonly struct BoundedOwnerBoundaryHit
        {
            public readonly Vector3 Position;
            public readonly int LocalEdgeIndex;
            public readonly int GraphEdgeIndex;
            public readonly int TargetGraphFaceIndex;
            public readonly int TargetSourceFaceIndex;
            public readonly float RayDistance;
            public readonly float SegmentParameter;
            public readonly float SnapDistance;
            public readonly float NearestEndpointDistance;

            public BoundedOwnerBoundaryHit(
                Vector3 position,
                int localEdgeIndex,
                int graphEdgeIndex,
                int targetGraphFaceIndex,
                int targetSourceFaceIndex,
                float rayDistance,
                float segmentParameter,
                float snapDistance,
                float nearestEndpointDistance)
            {
                Position = position;
                LocalEdgeIndex = localEdgeIndex;
                GraphEdgeIndex = graphEdgeIndex;
                TargetGraphFaceIndex = targetGraphFaceIndex;
                TargetSourceFaceIndex = targetSourceFaceIndex;
                RayDistance = rayDistance;
                SegmentParameter = segmentParameter;
                SnapDistance = snapDistance;
                NearestEndpointDistance = nearestEndpointDistance;
            }
        }

        private readonly struct BoundedIsolatedRailPoint
        {
            public readonly Vector3 Position;
            public readonly int RailIndex;
            public readonly int OwnerGraphFaceIndex;
            public readonly int OwnerSourceFaceIndex;
            public readonly int SourceVertexIndex;
            public readonly int AdjacentGraphEdgeIndex;
            public readonly int OriginalAdjacentGraphEdgeIndex;
            public readonly int TargetGraphFaceIndex;
            public readonly int TargetSourceFaceIndex;
            public readonly int BoundaryCandidateCount;
            public readonly bool UsedAlternateBoundarySegment;
            public readonly float BoundarySnapDistance;
            public readonly float OriginalBoundaryRawParameter;
            public readonly float OriginalBoundarySegmentDistance;
            public readonly float ResolvedBoundaryParameter;
            public readonly float BoundaryPointTolerance;
            public readonly float NearestBoundaryEndpointDistance;

            public BoundedIsolatedRailPoint(
                Vector3 position,
                int railIndex,
                int ownerGraphFaceIndex,
                int ownerSourceFaceIndex,
                int sourceVertexIndex,
                int adjacentGraphEdgeIndex,
                int originalAdjacentGraphEdgeIndex,
                int targetGraphFaceIndex,
                int targetSourceFaceIndex,
                int boundaryCandidateCount,
                bool usedAlternateBoundarySegment,
                float boundarySnapDistance,
                float originalBoundaryRawParameter,
                float originalBoundarySegmentDistance,
                float resolvedBoundaryParameter,
                float boundaryPointTolerance,
                float nearestBoundaryEndpointDistance)
            {
                Position = position;
                RailIndex = railIndex;
                OwnerGraphFaceIndex = ownerGraphFaceIndex;
                OwnerSourceFaceIndex = ownerSourceFaceIndex;
                SourceVertexIndex = sourceVertexIndex;
                AdjacentGraphEdgeIndex = adjacentGraphEdgeIndex;
                OriginalAdjacentGraphEdgeIndex =
                    originalAdjacentGraphEdgeIndex;
                TargetGraphFaceIndex = targetGraphFaceIndex;
                TargetSourceFaceIndex = targetSourceFaceIndex;
                BoundaryCandidateCount = boundaryCandidateCount;
                UsedAlternateBoundarySegment =
                    usedAlternateBoundarySegment;
                BoundarySnapDistance = boundarySnapDistance;
                OriginalBoundaryRawParameter =
                    originalBoundaryRawParameter;
                OriginalBoundarySegmentDistance =
                    originalBoundarySegmentDistance;
                ResolvedBoundaryParameter = resolvedBoundaryParameter;
                BoundaryPointTolerance = boundaryPointTolerance;
                NearestBoundaryEndpointDistance =
                    nearestBoundaryEndpointDistance;
            }
        }

        private static bool IsBoundedCanonicalSubdivisionSourceFace(
            BoundedIsolatedRailPoint[] rails,
            PolygonFaceProvenanceKind provenanceKind,
            int provenanceIndex)
        {
            if (rails == null ||
                provenanceKind != PolygonFaceProvenanceKind.SourceFace ||
                provenanceIndex < 0)
            {
                return false;
            }

            for (int railIndex = 0; railIndex < rails.Length; railIndex++)
            {
                if (rails[railIndex].TargetSourceFaceIndex ==
                    provenanceIndex)
                {
                    return true;
                }
            }
            return false;
        }

        private static BoundedSingleEdgeAuditResult
            AuditBoundedSingleEdgeBevel(
                List<PolygonFace> sourceFaces,
                ChamferTopologyContext context,
                int requestedOrdinal,
                float requestedWidth,
                float minimumStableEdgeLength,
                float minimumStableFaceArea,
                bool auditCandidatePool,
                out TriangleSoup previewSoup)
        {
            previewSoup = null;
            List<EdgeWearIsolatedWidthAttemptRecord> aggregateAttempts =
                new List<EdgeWearIsolatedWidthAttemptRecord>(
                    BoundedIsolatedMaximumWidthAttempts);
            float nextRequestedWidth = requestedWidth;
            int remainingAttempts = BoundedIsolatedMaximumWidthAttempts;
            BoundedSingleEdgeAuditResult result = default;

            while (remainingAttempts > 0)
            {
                result = AuditBoundedSingleEdgeBevelSingleSchedule(
                    sourceFaces,
                    context,
                    requestedOrdinal,
                    nextRequestedWidth,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    auditCandidatePool,
                    remainingAttempts,
                    out TriangleSoup attemptSoup);
                AppendBoundedWidthAttempts(
                    aggregateAttempts,
                    result.IsolatedWidthAttempts);
                remainingAttempts = Mathf.Max(
                    0,
                    BoundedIsolatedMaximumWidthAttempts -
                        aggregateAttempts.Count);

                if (result.GeometryValid == 1)
                {
                    previewSoup = attemptSoup;
                    return FinalizeBoundedAggregateWidthSchedule(
                        result,
                        aggregateAttempts,
                        remainingAttempts == 0);
                }

                if (!ShouldRetryBoundedOwnerSupportAtLowerWidth(result) ||
                    remainingAttempts == 0)
                {
                    return FinalizeBoundedAggregateWidthSchedule(
                        result,
                        aggregateAttempts,
                        remainingAttempts == 0);
                }

                float minimumWidth = Mathf.Max(
                    result.IsolatedMinimumWidth,
                    Mathf.Max(
                        minimumStableEdgeLength,
                        PointMergeDistance * 4f));
                float failedWidth = result.SolvedWidth > 0f
                    ? result.SolvedWidth
                    : nextRequestedWidth;
                if (failedWidth <= minimumWidth + PointMergeDistance)
                {
                    result.IsolatedAttemptScheduleComplete = 1;
                    return FinalizeBoundedAggregateWidthSchedule(
                        result,
                        aggregateAttempts,
                        true);
                }

                float reducedWidth = Mathf.Max(
                    minimumWidth,
                    failedWidth * BoundedIsolatedWidthBackoff);
                if (reducedWidth >= failedWidth - PointMergeDistance)
                {
                    return FinalizeBoundedAggregateWidthSchedule(
                        result,
                        aggregateAttempts,
                        false);
                }
                nextRequestedWidth = reducedWidth;
            }

            return FinalizeBoundedAggregateWidthSchedule(
                result,
                aggregateAttempts,
                true);
        }

        private static void AppendBoundedWidthAttempts(
            List<EdgeWearIsolatedWidthAttemptRecord> aggregate,
            List<EdgeWearIsolatedWidthAttemptRecord> attempts)
        {
            if (aggregate == null || attempts == null)
            {
                return;
            }

            for (int attemptIndex = 0;
                 attemptIndex < attempts.Count &&
                 aggregate.Count < BoundedIsolatedMaximumWidthAttempts;
                 attemptIndex++)
            {
                EdgeWearIsolatedWidthAttemptRecord attempt =
                    attempts[attemptIndex];
                attempt.AttemptIndex = aggregate.Count + 1;
                aggregate.Add(attempt);
            }
        }

        private static bool ShouldRetryBoundedOwnerSupportAtLowerWidth(
            BoundedSingleEdgeAuditResult result)
        {
            return result.GeometryValid != 1 &&
                result.IsolatedRailSolved == 1 &&
                (result.OwnerClipCount != 2 ||
                 result.EndpointSupportClipCount != 2 ||
                 result.EndpointSupportRemovedVertexCount != 2 ||
                 result.EndpointSupportRailInsertionCount != 4);
        }

        private static BoundedSingleEdgeAuditResult
            FinalizeBoundedAggregateWidthSchedule(
                BoundedSingleEdgeAuditResult result,
                List<EdgeWearIsolatedWidthAttemptRecord> aggregateAttempts,
                bool scheduleComplete)
        {
            result.IsolatedWidthAttempts = aggregateAttempts ??
                new List<EdgeWearIsolatedWidthAttemptRecord>();
            result.WidthAttemptCount = result.IsolatedWidthAttempts.Count;
            if (scheduleComplete ||
                result.WidthAttemptCount >=
                    BoundedIsolatedMaximumWidthAttempts)
            {
                result.IsolatedAttemptScheduleComplete = 1;
            }
            if (result.IsolatedWidthAttempts.Count > 0)
            {
                EdgeWearIsolatedWidthAttemptRecord terminal =
                    result.IsolatedWidthAttempts[
                        result.IsolatedWidthAttempts.Count - 1];
                result.IsolatedTerminalConstructionAtMinimum =
                    terminal.ConstructionAttempted &&
                    terminal.Width <=
                        result.IsolatedMinimumWidth + PointMergeDistance
                        ? 1
                        : 0;
            }
            return FinalizeBoundedSingleEdgeAuditResult(result);
        }

        private static BoundedSingleEdgeAuditResult
            AuditBoundedSingleEdgeBevelSingleSchedule(
                List<PolygonFace> sourceFaces,
                ChamferTopologyContext context,
                int requestedOrdinal,
                float requestedWidth,
                float minimumStableEdgeLength,
                float minimumStableFaceArea,
                bool auditCandidatePool,
                int maximumWidthAttempts,
                out TriangleSoup previewSoup)
        {
            previewSoup = null;
            BoundedSingleEdgeAuditResult result =
                new BoundedSingleEdgeAuditResult
                {
                    SelectedOrdinal = -1,
                    SourceEdgeIndex = -1,
                    IsolatedAttemptScheduleResolution = string.Empty,
                    IsolatedWidthAttempts =
                        new List<EdgeWearIsolatedWidthAttemptRecord>(),
                    MultiSupportSinglePlaneFailure = string.Empty,
                    MultiSupportRetainedHullFailure = string.Empty,
                    MaximumBoundaryDiagnosticRailIndex = -1,
                    MaximumBoundaryOriginalAdjacentEdgeIndex = -1,
                    MaximumBoundaryResolvedEdgeIndex = -1,
                    PrepareFailedFace = -1,
                    PrepareFailedProvenanceIndex = -1,
                    ResultPreparation = CreateBoundedPreparationAudit(),
                    SourcePreparation = CreateBoundedPreparationAudit(),
                    RawSourceProvenance =
                        CreateBoundedSourceProvenanceAudit(),
                    PreparedSourceProvenance =
                        CreateBoundedSourceProvenanceAudit(),
                    ResultSourceProvenance =
                        CreateBoundedSourceProvenanceAudit(),
                    BevelRegionFailureFace = -1,
                    BevelRegionFailureProvenanceIndex = -1,
                    TriangulationFailureFace = -1,
                    TriangulationFailureProvenanceIndex = -1,
                    EdgeSourceFaceA = -1,
                    EdgeSourceFaceB = -1,
                    SourceViolatingPlaneFace = -1,
                    SourceViolatingVertexFace = -1,
                    SourceViolatingVertexIndex = -1,
                    ResultViolatingFace = -1,
                    ResultViolatingProvenanceIndex = -1,
                    ResultViolatingVertexIndex = -1,
                    ResultViolatedSourcePlane = -1,
                    EndpointSupportFaceA = -1,
                    EndpointSupportFaceB = -1,
                    EndpointSupportAdditionalFaceA = -1,
                    EndpointSupportAdditionalFaceB = -1,
                    EndpointSupportGraphFaceA = -1,
                    EndpointSupportGraphFaceB = -1,
                    EndpointSupportVertexA = -1,
                    EndpointSupportVertexB = -1,
                    EndpointSupportPreviousEdgeA = -1,
                    EndpointSupportPreviousEdgeB = -1,
                    EndpointSupportNextEdgeA = -1,
                    EndpointSupportNextEdgeB = -1,
                    EndpointSupportPreviousRailA = -1,
                    EndpointSupportPreviousRailB = -1,
                    EndpointSupportNextRailA = -1,
                    EndpointSupportNextRailB = -1,
                    ResultConvexityPlaneFace = -1,
                    ResultConvexityPlaneProvenanceIndex = -1,
                    ResultConvexityVertexFace = -1,
                    ResultConvexityVertexProvenanceIndex = -1,
                    ResultConvexityVertexIndex = -1,
                    FirstIntersectionFaceA = -1,
                    FirstIntersectionFaceAProvenanceIndex = -1,
                    FirstIntersectionFaceB = -1,
                    FirstIntersectionFaceBProvenanceIndex = -1
                };

            List<EdgeWearSelectedGraphEdge> eligible =
                BuildBoundedSingleEdgeEligibleList(context);
            result.CandidateCount = eligible.Count;
            if (eligible.Count == 0)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "no selected manifold edge is available for bounded evaluation");
                return FinalizeBoundedSingleEdgeAuditResult(result);
            }

            int selectedOrdinal = Mathf.Clamp(
                requestedOrdinal,
                0,
                eligible.Count - 1);
            result.SelectedOrdinal = selectedOrdinal;
            EdgeWearSelectedGraphEdge selected = eligible[selectedOrdinal];
            result.SourceEdgeIndex = selected.GraphEdgeIndex;
            Vector3 sourceSolidCentre =
                CalculatePlaneCutFaceVertexCentre(sourceFaces);
            float edgeClassificationTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                minimumStableEdgeLength * 0.02f);
            if (auditCandidatePool)
            {
                AuditBoundedEdgeClassificationPool(
                    sourceFaces,
                    context,
                    eligible,
                    sourceSolidCentre,
                    edgeClassificationTolerance,
                    ref result);
            }
            AuditBoundedSelectedEdgeClassification(
                sourceFaces,
                context,
                selected,
                sourceSolidCentre,
                edgeClassificationTolerance,
                ref result);

            if (!TrySolveBoundedIsolatedSingleEdgeRails(
                    sourceFaces,
                    context,
                    selected,
                    requestedWidth,
                    minimumStableEdgeLength,
                    out BoundedIsolatedRailPoint[] isolatedRails,
                    out List<EdgeWearIsolatedWidthAttemptRecord>
                        widthAttempts,
                    out float minimumIsolatedWidth,
                    out bool railScheduleComplete,
                    out int widthAttemptCount,
                    out float solvedWidth,
                    out string railBlocker,
                    maximumWidthAttempts))
            {
                result.IsolatedWidthAttempts = widthAttempts;
                result.IsolatedMinimumWidth = minimumIsolatedWidth;
                result.IsolatedAttemptScheduleComplete =
                    railScheduleComplete ? 1 : 0;
                result.WidthAttemptCount = widthAttemptCount;
                result.SolvedWidth = solvedWidth;
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    railBlocker);
                return FinalizeBoundedSingleEdgeAuditResult(result);
            }
            result.IsolatedRailSolved = 1;
            result.IsolatedWidthAttempts = widthAttempts;
            result.IsolatedMinimumWidth = minimumIsolatedWidth;
            result.IsolatedAttemptScheduleComplete =
                railScheduleComplete ? 1 : 0;
            result.WidthAttemptCount = widthAttemptCount;
            result.SolvedWidth = solvedWidth;
            EdgeWearGraphEdge isolatedSourceEdge =
                context.Graph.Edges[selected.GraphEdgeIndex];
            Vector3 isolatedSourceA = context.Graph.Vertices[
                isolatedSourceEdge.VertexA].Position;
            Vector3 isolatedSourceB = context.Graph.Vertices[
                isolatedSourceEdge.VertexB].Position;
            Vector3 isolatedAxis = isolatedSourceB - isolatedSourceA;
            float isolatedSourceLength = isolatedAxis.magnitude;
            if (isolatedSourceLength > PointMergeDistance)
            {
                isolatedAxis /= isolatedSourceLength;
                Vector3 endpointRailA =
                    (isolatedRails[0].Position +
                     isolatedRails[2].Position) * 0.5f;
                Vector3 endpointRailB =
                    (isolatedRails[1].Position +
                     isolatedRails[3].Position) * 0.5f;
                result.EndpointConsumptionA = Mathf.Max(
                    0f,
                    Vector3.Dot(
                        endpointRailA - isolatedSourceA,
                        isolatedAxis));
                result.EndpointConsumptionB = Mathf.Max(
                    0f,
                    Vector3.Dot(
                        isolatedSourceB - endpointRailB,
                        isolatedAxis));
                result.RemainingCentralSpan = Mathf.Max(
                    0f,
                    isolatedSourceLength -
                    result.EndpointConsumptionA -
                    result.EndpointConsumptionB);
            }
            result.MinimumCentralSpan = Mathf.Max(
                minimumStableEdgeLength,
                requestedWidth * 0.5f);
            result.TargetBoundaryCount = isolatedRails.Length;
            result.MinimumBoundaryEndpointDistance = float.PositiveInfinity;
            for (int railIndex = 0;
                 railIndex < isolatedRails.Length;
                 railIndex++)
            {
                BoundedIsolatedRailPoint rail = isolatedRails[railIndex];
                if (!float.IsNaN(rail.BoundarySnapDistance) &&
                    !float.IsInfinity(rail.BoundarySnapDistance))
                {
                    result.CanonicalRailCount++;
                    result.MaximumBoundarySnapDistance = Mathf.Max(
                        result.MaximumBoundarySnapDistance,
                        rail.BoundarySnapDistance);
                    result.MaximumBoundaryPointTolerance = Mathf.Max(
                        result.MaximumBoundaryPointTolerance,
                        rail.BoundaryPointTolerance);
                    result.MaximumBoundaryCandidateCount = Mathf.Max(
                        result.MaximumBoundaryCandidateCount,
                        rail.BoundaryCandidateCount);
                    result.MinimumBoundaryEndpointDistance = Mathf.Min(
                        result.MinimumBoundaryEndpointDistance,
                        rail.NearestBoundaryEndpointDistance);
                    if (rail.UsedAlternateBoundarySegment)
                    {
                        result.AlternateBoundaryRailCount++;
                    }
                    if (rail.OriginalBoundarySegmentDistance >
                        result.MaximumBoundaryOriginalSegmentDistance)
                    {
                        result.MaximumBoundaryDiagnosticRailIndex =
                            rail.RailIndex;
                        result.MaximumBoundaryOriginalAdjacentEdgeIndex =
                            rail.OriginalAdjacentGraphEdgeIndex;
                        result.MaximumBoundaryResolvedEdgeIndex =
                            rail.AdjacentGraphEdgeIndex;
                        result.MaximumBoundaryOriginalRawParameter =
                            rail.OriginalBoundaryRawParameter;
                        result.MaximumBoundaryOriginalSegmentDistance =
                            rail.OriginalBoundarySegmentDistance;
                    }
                }
            }
            if (float.IsPositiveInfinity(
                    result.MinimumBoundaryEndpointDistance))
            {
                result.MinimumBoundaryEndpointDistance = 0f;
            }

            bool boundedFacesBuilt = TryBuildBoundedSingleEdgeFaces(
                sourceFaces,
                context,
                selected,
                isolatedRails,
                minimumStableEdgeLength,
                minimumStableFaceArea,
                ref result,
                out List<PolygonFace> boundedFaces,
                out int boundarySubdivisionCount,
                out string buildBlocker);
            UpdateBoundedTerminalWidthAttemptConstruction(
                ref result,
                boundedFacesBuilt,
                buildBlocker);
            if (!boundedFacesBuilt)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    buildBlocker);
                return FinalizeBoundedSingleEdgeAuditResult(result);
            }
            result.BoundarySubdivisionCount = boundarySubdivisionCount;

            if (!TryPrepareBoundedPreviewFaces(
                    boundedFaces,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    ref result,
                    out List<PolygonFace> auditedFaces,
                    out string preparationBlocker))
            {
                result.PrepareFailedCanonicalSubdivision =
                    IsBoundedCanonicalSubdivisionSourceFace(
                        isolatedRails,
                        result.PrepareFailedProvenanceKind,
                        result.PrepareFailedProvenanceIndex)
                        ? 1
                        : 0;
                if (result.PrepareFailedCanonicalSubdivision == 1)
                {
                    preparationBlocker =
                        (string.IsNullOrEmpty(preparationBlocker)
                            ? "the bounded single-edge shell failed preview preparation"
                            : preparationBlocker) +
                        " on a canonical rail-clipped endpoint support face";
                }
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    string.IsNullOrEmpty(preparationBlocker)
                        ? "the bounded single-edge shell failed preview preparation"
                        : preparationBlocker);
                return FinalizeBoundedSingleEdgeAuditResult(result);
            }

            Vector3 boundedSolidCentre = sourceSolidCentre;
            if (!TryOrientBoundedGeneratedFacesOutward(
                    auditedFaces,
                    boundedSolidCentre,
                    ref result,
                    out List<PolygonFace> outwardFaces,
                    out string windingBlocker))
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    windingBlocker);
                return FinalizeBoundedSingleEdgeAuditResult(result);
            }
            auditedFaces = outwardFaces;

            int expectedSourceFaceCount =
                sourceFaces == null ? 0 : sourceFaces.Count;
            List<PolygonFace> attributedSourceFaces =
                ClonePolygonFacesForPlaneCutAudit(
                    sourceFaces,
                    assignSourceFaceProvenance: true);
            result.RawSourceProvenance =
                AuditBoundedSourceFaceProvenance(
                    attributedSourceFaces,
                    expectedSourceFaceCount);
            if (result.RawSourceProvenance.Valid != 1)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the attributed raw source baseline failed complete unique source-face provenance certification");
                return FinalizeBoundedSingleEdgeAuditResult(result);
            }

            bool sourcePreparationValid = TryPrepareBoundedFaces(
                attributedSourceFaces,
                minimumStableEdgeLength,
                minimumStableFaceArea,
                out List<PolygonFace> preparedSourceFaces,
                out BoundedPreparationAudit sourcePreparation,
                out string sourcePreparationBlocker);
            result.SourcePreparation = sourcePreparation;
            if (!sourcePreparationValid)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "source-baseline " +
                    (string.IsNullOrEmpty(sourcePreparationBlocker)
                        ? "preparation failed"
                        : sourcePreparationBlocker));
                return FinalizeBoundedSingleEdgeAuditResult(result);
            }

            result.PreparedSourceProvenance =
                AuditBoundedSourceFaceProvenance(
                    preparedSourceFaces,
                    expectedSourceFaceCount);
            result.ResultSourceProvenance =
                AuditBoundedSourceFaceProvenance(
                    auditedFaces,
                    expectedSourceFaceCount);
            result.SourceProvenanceCertificationValid =
                result.RawSourceProvenance.Valid == 1 &&
                result.PreparedSourceProvenance.Valid == 1 &&
                result.ResultSourceProvenance.Valid == 1
                    ? 1
                    : 0;
            if (result.SourceProvenanceCertificationValid != 1)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the prepared source or bounded result failed complete unique source-face provenance certification");
                return FinalizeBoundedSingleEdgeAuditResult(result);
            }

            CountBoundedSingleEdgeFaces(
                auditedFaces,
                selected.GraphEdgeIndex,
                ref result);
            ICollection<int> multiSupportAllowedFaces = null;
            if (result.MultiSupportPlaneCut == 1)
            {
                multiSupportAllowedFaces =
                    CollectBoundedEndpointSupportSourceFaces(
                        context,
                        context.Graph.Edges[selected.GraphEdgeIndex]);
            }
            BoundedSourceFaceChangeAudit rawSourceChanges =
                AuditBoundedSourceFaceChanges(
                    attributedSourceFaces,
                    auditedFaces,
                    context,
                    selected.GraphEdgeIndex,
                    result.EndpointSupportFaceA,
                    result.EndpointSupportAdditionalFaceA,
                    result.EndpointSupportFaceB,
                    result.EndpointSupportAdditionalFaceB,
                    result.MultiSupportPlaneCut == 1,
                    result.MultiSupportPlaneNormal,
                    result.MultiSupportPlaneDistance,
                    multiSupportAllowedFaces);
            BoundedSourceFaceChangeAudit preparedSourceChanges =
                AuditBoundedSourceFaceChanges(
                    preparedSourceFaces,
                    auditedFaces,
                    context,
                    selected.GraphEdgeIndex,
                    result.EndpointSupportFaceA,
                    result.EndpointSupportAdditionalFaceA,
                    result.EndpointSupportFaceB,
                    result.EndpointSupportAdditionalFaceB,
                    result.MultiSupportPlaneCut == 1,
                    result.MultiSupportPlaneNormal,
                    result.MultiSupportPlaneDistance,
                    multiSupportAllowedFaces);
            ApplyBoundedSourceFaceChangeAudits(
                rawSourceChanges,
                preparedSourceChanges,
                ref result);
            AuditBoundedRailFidelity(
                auditedFaces,
                selected.GraphEdgeIndex,
                isolatedRails[0].Position,
                isolatedRails[1].Position,
                isolatedRails[2].Position,
                isolatedRails[3].Position,
                ref result);

            EdgeWearTopologyStats topology = AuditEdgeWearTopology(
                auditedFaces,
                minimumStableEdgeLength);
            result.OpenEdgeCount = topology.OpenEdgeCount;
            result.NonManifoldEdgeCount = topology.NonManifoldEdgeCount;
            result.TJunctionCount = topology.TJunctionCount;
            result.InvalidFaceCount = CountInvalidPlaneCutFaces(
                auditedFaces,
                minimumStableFaceArea);

            result.CertificationAttempted = 1;
            Bounds sourceBounds = CalculateFaceBounds(sourceFaces);
            Bounds preparedSourceBounds =
                CalculateFaceBounds(preparedSourceFaces);
            Bounds resultBounds = CalculateFaceBounds(auditedFaces);
            float boundsTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                minimumStableEdgeLength * 0.02f);
            bool boundsValid = ArePlaneCutBoundsContained(
                sourceBounds,
                resultBounds,
                boundsTolerance);
            bool preparedBoundsValid = ArePlaneCutBoundsContained(
                preparedSourceBounds,
                resultBounds,
                boundsTolerance);
            result.BoundsValid = boundsValid ? 1 : 0;
            result.PreparedBoundsValid = preparedBoundsValid ? 1 : 0;
            result.BoundsTolerance = boundsTolerance;
            result.RawSourceBoundsMinimum = sourceBounds.min;
            result.RawSourceBoundsMaximum = sourceBounds.max;
            result.PreparedSourceBoundsMinimum = preparedSourceBounds.min;
            result.PreparedSourceBoundsMaximum = preparedSourceBounds.max;
            result.ResultBoundsMinimum = resultBounds.min;
            result.ResultBoundsMaximum = resultBounds.max;
            result.RawBoundsMinimumMargin =
                resultBounds.min - sourceBounds.min;
            result.RawBoundsMaximumMargin =
                sourceBounds.max - resultBounds.max;
            result.PreparedBoundsMinimumMargin =
                resultBounds.min - preparedSourceBounds.min;
            result.PreparedBoundsMaximumMargin =
                preparedSourceBounds.max - resultBounds.max;

            double sourceVolume =
                CalculatePlaneCutPolyhedronVolume(sourceFaces);
            double preparedSourceVolume =
                CalculatePlaneCutPolyhedronVolume(preparedSourceFaces);
            double resultVolume =
                CalculatePlaneCutPolyhedronVolume(auditedFaces);
            const double minimumRetainedVolumeRatio = 0.75;
            const double maximumRetainedVolumeRatio = 1.0;
            result.SourceVolume = sourceVolume;
            result.PreparedSourceVolume = preparedSourceVolume;
            result.ResultVolume = resultVolume;
            result.RawVolumeRatio = sourceVolume > 0.000000001
                ? resultVolume / sourceVolume
                : 0.0;
            result.VolumeRatio = preparedSourceVolume > 0.000000001
                ? resultVolume / preparedSourceVolume
                : 0.0;
            result.SourcePreparationVolumeRatio =
                sourceVolume > 0.000000001
                    ? preparedSourceVolume / sourceVolume
                    : 0.0;
            result.RawVolumeDelta = resultVolume - sourceVolume;
            result.PreparedVolumeDelta =
                resultVolume - preparedSourceVolume;
            result.VolumeMinimumRatio = minimumRetainedVolumeRatio;
            result.VolumeMaximumRatio = maximumRetainedVolumeRatio;
            result.VolumeLowerMargin =
                result.VolumeRatio - minimumRetainedVolumeRatio;
            result.VolumeUpperMargin =
                maximumRetainedVolumeRatio - result.VolumeRatio;
            bool volumeValid = preparedSourceVolume > 0.000000001 &&
                resultVolume >
                    preparedSourceVolume * minimumRetainedVolumeRatio &&
                resultVolume <=
                    preparedSourceVolume * maximumRetainedVolumeRatio;
            result.VolumeValid = volumeValid ? 1 : 0;

            AuditBoundedSourceSolidContainment(
                sourceFaces,
                auditedFaces,
                boundsTolerance,
                ref result);
            AuditBoundedResultConvexity(
                auditedFaces,
                boundsTolerance,
                ref result);
            BoundedFaceIntersectionAudit sourceIntersectionAudit =
                AuditBoundedFaceIntersections(
                    preparedSourceFaces,
                    context,
                    minimumStableFaceArea,
                    boundsTolerance);
            BoundedFaceIntersectionAudit resultIntersectionAudit =
                AuditBoundedFaceIntersections(
                    auditedFaces,
                    context,
                    minimumStableFaceArea,
                    boundsTolerance);
            ApplyBoundedFaceIntersectionDelta(
                sourceIntersectionAudit,
                resultIntersectionAudit,
                ref result);
            AuditBoundedLocalVolumeAttribution(
                sourceFaces,
                preparedSourceFaces,
                auditedFaces,
                context,
                selected,
                boundedSolidCentre,
                ref result);
            AuditBoundedBevelPlaneSidedness(
                auditedFaces,
                context,
                selected,
                isolatedRails,
                boundedSolidCentre,
                ref result);

            float railTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                minimumStableEdgeLength * 0.02f);
            int expectedModifiedSourceFaceCount =
                2 + result.EndpointSupportModifiedFaceExpectedCount;
            bool endpointSupportTopologyValid =
                result.EndpointSupportClipAttemptedCount == 2 &&
                result.EndpointSupportClipCount == 2 &&
                result.EndpointSupportRemovedVertexCount == 2 &&
                result.EndpointSupportRailInsertionCount == 4 &&
                result.EndpointCapCount ==
                    result.EndpointSupportCapFaceCount;
            int expectedBevelFaceCount = result.MultiSupportPlaneCut == 1
                ? result.MultiSupportPlaneCount
                : 1;
            bool railExtentValid =
                result.MultiSupportPlaneCut == 1 ||
                result.MaximumExtentBeyondRails <= railTolerance;
            bool shellReadyForTriangulation =
                result.EdgeClassification ==
                    BoundedEdgeClassification.Convex &&
                result.IsolatedRailSolved == 1 &&
                result.WidthAttemptCount > 0 &&
                result.TargetBoundaryCount == 4 &&
                result.CanonicalRailCount == 4 &&
                result.MaximumBoundarySnapDistance <= railTolerance &&
                result.OwnerClipAttemptedCount == 2 &&
                result.OwnerClipCount == 2 &&
                result.OwnerIntersectionFailureCount == 0 &&
                result.OwnerDegenerateCount == 0 &&
                result.OwnerNonPlanarCount == 0 &&
                result.OwnerNonSimpleCount == 0 &&
                result.OwnerNonConvexCount == 0 &&
                result.OwnerWindingFailureCount == 0 &&
                result.OutwardWindingFailureCount == 0 &&
                result.EndpointSupportClipAttemptedCount == 2 &&
                result.EndpointSupportClipCount == 2 &&
                result.EndpointSupportSharedFaceFailureCount == 0 &&
                result.EndpointSupportIncidenceFailureCount == 0 &&
                result.EndpointSupportDegenerateCount == 0 &&
                result.EndpointSupportNonPlanarCount == 0 &&
                result.EndpointSupportNonSimpleCount == 0 &&
                result.EndpointSupportNonConvexCount == 0 &&
                result.EndpointSupportWindingFailureCount == 0 &&
                endpointSupportTopologyValid &&
                expectedBevelFaceCount > 0 &&
                result.BevelFaceCount == expectedBevelFaceCount &&
                result.ModifiedSourceFaceCount ==
                    expectedModifiedSourceFaceCount &&
                result.OwnerSourceFaceModifiedCount == 2 &&
                result.EndpointSupportSourceFaceModifiedCount ==
                    result.EndpointSupportModifiedFaceExpectedCount &&
                result.UnexpectedSourceFaceModifiedCount == 0 &&
                result.BoundaryOnlyUnexpectedSourceFaceCount == 0 &&
                result.ForeignSourceFaceModifiedCount == 0 &&
                result.ForeignBoundarySubdividedCount == 0 &&
                result.PreparedSourceChangeComparisonAttempted == 1 &&
                result.SourceProvenanceCertificationValid == 1 &&
                result.RailDeviation <= railTolerance &&
                railExtentValid &&
                result.OpenEdgeCount == 0 &&
                result.NonManifoldEdgeCount == 0 &&
                result.TJunctionCount == 0 &&
                result.InvalidFaceCount == 0 &&
                result.SourceConvexityViolationCount == 0 &&
                result.ResultContainmentViolationCount == 0 &&
                result.ResultConvexityViolationCount == 0 &&
                result.IntroducedImproperInteriorIntersectionPairCount == 0;
            bool polygonValid = shellReadyForTriangulation &&
                boundsValid &&
                volumeValid;

            if (result.EdgeClassification !=
                BoundedEdgeClassification.Convex)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the selected edge is not certified convex for bounded single-edge beveling");
            }
            else if (result.IsolatedRailSolved != 1)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the selected edge has no stable isolated rail solution");
            }
            else if (result.TargetBoundaryCount != 4 ||
                result.CanonicalRailCount != 4 ||
                result.MaximumBoundarySnapDistance > railTolerance)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the isolated rail solution did not canonicalize four exact boundary points");
            }
            else if (result.OwnerClipCount != 2)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the bounded edge did not produce two certified owner-face clips");
            }
            else if (!endpointSupportTopologyValid)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the bounded edge did not replace both endpoint source corners with four exact rail boundaries");
            }
            else if (expectedBevelFaceCount <= 0 ||
                result.BevelFaceCount != expectedBevelFaceCount)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the bounded edge does not retain its complete certified bevel-facet chain");
            }
            else if (result.SourceProvenanceCertificationValid != 1)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the bounded source baselines do not preserve one complete unique source-face provenance set");
            }
            else if (result.ModifiedSourceFaceCount !=
                    expectedModifiedSourceFaceCount ||
                result.OwnerSourceFaceModifiedCount != 2 ||
                result.EndpointSupportSourceFaceModifiedCount !=
                    result.EndpointSupportModifiedFaceExpectedCount ||
                result.UnexpectedSourceFaceModifiedCount != 0 ||
                result.BoundaryOnlyUnexpectedSourceFaceCount != 0 ||
                result.ForeignSourceFaceModifiedCount != 0 ||
                result.ForeignBoundarySubdividedCount != 0)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the bounded edge did not modify exactly its two owner faces and certified endpoint-support interval relative to the prepared source baseline");
            }
            else if (result.RailDeviation > railTolerance ||
                !railExtentValid)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the bounded bevel polygon escaped its solved rail quadrilateral");
            }
            else if (result.OpenEdgeCount > 0 ||
                result.NonManifoldEdgeCount > 0 ||
                result.TJunctionCount > 0 ||
                result.InvalidFaceCount > 0)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the bounded single-edge shell failed polygon topology certification");
            }
            else if (result.SourceConvexityViolationCount > 0)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the source shell is not convex enough for this bounded single-edge prototype");
            }
            else if (result.ResultContainmentViolationCount > 0)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the bounded single-edge shell escapes the original source solid");
            }
            else if (result.ResultConvexityViolationCount > 0)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the bounded single-edge shell is not globally convex");
            }
            else if (result.IntroducedImproperInteriorIntersectionPairCount > 0)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the bounded single-edge shell introduces a new improper interior face intersection beyond the prepared source baseline");
            }
            else if (!boundsValid)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the bounded single-edge shell exceeds source bounds");
            }
            else if (!volumeValid)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the bounded single-edge shell failed preparation-equivalent retained-volume certification");
            }

            TriangleSoup soup = null;
            if (shellReadyForTriangulation)
            {
                result.DiagnosticTriangulationAttempted = 1;
                if (!TryTriangulateBoundedPreviewFaces(
                        auditedFaces,
                        minimumStableFaceArea,
                        ref result,
                        out soup,
                        out string triangulationBlocker))
                {
                    SetBoundedSingleEdgeDiagnostic(
                        ref result.Diagnostic,
                        triangulationBlocker);
                    polygonValid = false;
                }
            }
            if (soup != null)
            {
                PlaneCutBevelAuditResult triangleAudit = default;
                AuditPlaneCutPreviewTriangleSoup(
                    soup,
                    auditedFaces,
                    minimumStableEdgeLength,
                    ref triangleAudit);
                result.PreviewTriangleCount =
                    triangleAudit.PreviewTriangleCount;
                result.PreviewDegenerateTriangleCount =
                    triangleAudit.PreviewDegenerateTriangleCount;
                result.PreviewOpenEdgeCount =
                    triangleAudit.PreviewOpenEdgeCount;
                result.PreviewNonManifoldEdgeCount =
                    triangleAudit.PreviewNonManifoldEdgeCount;
                result.PreviewWindingFailureCount =
                    triangleAudit.PreviewWindingFailureCount;
                result.PreviewBoundsFailureCount =
                    triangleAudit.PreviewBoundsFailureCount;
                result.PreviewVolumeFailureCount =
                    triangleAudit.PreviewVolumeFailureCount;
                result.DiagnosticTriangleSignedVolume =
                    CalculateBoundedTriangleSoupSignedVolume(soup);
                result.DiagnosticTriangleVolume = Math.Abs(
                    result.DiagnosticTriangleSignedVolume);
                result.PolygonTriangleVolumeDelta =
                    result.DiagnosticTriangleVolume -
                    result.ResultAbsoluteVolume;
                result.PolygonTriangleSignedVolumeDelta =
                    result.DiagnosticTriangleSignedVolume -
                    result.ResultSignedVolume;
                result.DiagnosticTriangleSoupValid =
                    triangleAudit.PreviewGeometryValid == 1 ? 1 : 0;
                if (triangleAudit.PreviewGeometryValid != 1)
                {
                    SetBoundedSingleEdgeDiagnostic(
                        ref result.Diagnostic,
                        "the exact bounded preview triangle soup failed certification");
                    polygonValid = false;
                }
                else if (polygonValid)
                {
                    result.GeometryValid = 1;
                    previewSoup = soup;
                }
            }

            return FinalizeBoundedSingleEdgeAuditResult(result);
        }

        private static void UpdateBoundedTerminalWidthAttemptConstruction(
            ref BoundedSingleEdgeAuditResult result,
            bool constructionSucceeded,
            string blocker)
        {
            if (result.IsolatedWidthAttempts == null ||
                result.IsolatedWidthAttempts.Count == 0)
            {
                return;
            }

            EdgeWearIsolatedWidthAttemptRecord attempt =
                result.IsolatedWidthAttempts[
                    result.IsolatedWidthAttempts.Count - 1];
            attempt.ConstructionAttempted = true;
            attempt.ConstructionSucceeded = constructionSucceeded;
            attempt.SinglePlaneAttempted =
                result.MultiSupportSinglePlaneAttempted == 1;
            attempt.SinglePlaneSucceeded =
                result.MultiSupportSinglePlaneSucceeded == 1;
            attempt.SinglePlaneFailure =
                result.MultiSupportSinglePlaneFailure ?? string.Empty;
            attempt.RetainedHullAttempted =
                result.MultiSupportRetainedHullAttempted == 1;
            attempt.RetainedHullSucceeded =
                result.MultiSupportRetainedHullSucceeded == 1;
            attempt.RetainedHullFailure =
                result.MultiSupportRetainedHullFailure ?? string.Empty;
            attempt.FinalFailure = constructionSucceeded
                ? string.Empty
                : blocker ?? string.Empty;
            result.IsolatedTerminalConstructionAtMinimum =
                attempt.Width <=
                    result.IsolatedMinimumWidth + PointMergeDistance
                    ? 1
                    : 0;
        }

        private static BoundedSingleEdgeAuditResult
            FinalizeBoundedSingleEdgeAuditResult(
                BoundedSingleEdgeAuditResult result)
        {
            if (result.IsolatedWidthAttempts == null ||
                result.IsolatedWidthAttempts.Count == 0)
            {
                return result;
            }

            EdgeWearIsolatedWidthAttemptRecord attempt =
                result.IsolatedWidthAttempts[
                    result.IsolatedWidthAttempts.Count - 1];
            if (attempt.ConstructionAttempted)
            {
                attempt.Certified = result.GeometryValid == 1;
                if (!attempt.Certified &&
                    string.IsNullOrEmpty(attempt.FinalFailure))
                {
                    attempt.FinalFailure =
                        result.Diagnostic ?? string.Empty;
                }
            }

            if (result.GeometryValid == 1)
            {
                result.IsolatedAttemptScheduleResolution = "certified";
            }
            else if (result.IsolatedAttemptScheduleComplete == 1 &&
                result.IsolatedRailSolved != 1)
            {
                result.IsolatedAttemptScheduleResolution =
                    "complete-rail-infeasible";
            }
            else if (result.IsolatedAttemptScheduleComplete == 1 &&
                (result.IsolatedTerminalConstructionAtMinimum == 1 ||
                 result.WidthAttemptCount >= 12) &&
                attempt.ConstructionAttempted)
            {
                result.IsolatedAttemptScheduleResolution =
                    "complete-infeasible";
            }
            else
            {
                result.IsolatedAttemptScheduleResolution = "unresolved";
            }
            return result;
        }

        private static string FormatBoundedIsolatedWidthAttemptEvidence(
            BoundedSingleEdgeAuditResult result)
        {
            StringBuilder builder = new StringBuilder(512);
            builder.Append("scheduleResolution:");
            builder.Append(string.IsNullOrEmpty(
                    result.IsolatedAttemptScheduleResolution)
                ? "unresolved"
                : result.IsolatedAttemptScheduleResolution);
            builder.Append(",scheduleComplete:");
            builder.Append(result.IsolatedAttemptScheduleComplete);
            builder.Append(",minimumWidth:");
            builder.Append(result.IsolatedMinimumWidth.ToString(
                "G9", CultureInfo.InvariantCulture));
            builder.Append(",terminalAtMinimum:");
            builder.Append(result.IsolatedTerminalConstructionAtMinimum);
            builder.Append(",attempts:[");
            if (result.IsolatedWidthAttempts != null)
            {
                for (int attemptIndex = 0;
                     attemptIndex < result.IsolatedWidthAttempts.Count;
                     attemptIndex++)
                {
                    if (attemptIndex > 0)
                    {
                        builder.Append('|');
                    }
                    EdgeWearIsolatedWidthAttemptRecord attempt =
                        result.IsolatedWidthAttempts[attemptIndex];
                    builder.Append(attempt.AttemptIndex);
                    builder.Append('@');
                    builder.Append(attempt.Width.ToString(
                        "G9", CultureInfo.InvariantCulture));
                    builder.Append(":rail=");
                    builder.Append(attempt.RailSolved ? '1' : '0');
                    builder.Append(":construction=");
                    builder.Append(attempt.ConstructionAttempted
                        ? (attempt.ConstructionSucceeded ? '1' : '0')
                        : '-');
                    builder.Append(":single=");
                    builder.Append(attempt.SinglePlaneAttempted
                        ? (attempt.SinglePlaneSucceeded ? '1' : '0')
                        : '-');
                    builder.Append(":hull=");
                    builder.Append(attempt.RetainedHullAttempted
                        ? (attempt.RetainedHullSucceeded ? '1' : '0')
                        : '-');
                    builder.Append(":certified=");
                    builder.Append(attempt.Certified ? '1' : '0');
                    if (!string.IsNullOrEmpty(attempt.RailFailure))
                    {
                        builder.Append(":railFailure={");
                        builder.Append(attempt.RailFailure);
                        builder.Append('}');
                    }
                    if (!string.IsNullOrEmpty(
                            attempt.SinglePlaneFailure))
                    {
                        builder.Append(":singleFailure={");
                        builder.Append(attempt.SinglePlaneFailure);
                        builder.Append('}');
                    }
                    if (!string.IsNullOrEmpty(
                            attempt.RetainedHullFailure))
                    {
                        builder.Append(":hullFailure={");
                        builder.Append(attempt.RetainedHullFailure);
                        builder.Append('}');
                    }
                    if (!string.IsNullOrEmpty(attempt.FinalFailure))
                    {
                        builder.Append(":finalFailure={");
                        builder.Append(attempt.FinalFailure);
                        builder.Append('}');
                    }
                }
            }
            builder.Append(']');
            return builder.ToString();
        }

        private static void AuditBoundedEdgeClassificationPool(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            List<EdgeWearSelectedGraphEdge> eligible,
            Vector3 solidCentre,
            float tolerance,
            ref BoundedSingleEdgeAuditResult audit)
        {
            if (eligible == null)
            {
                return;
            }

            for (int edgeIndex = 0; edgeIndex < eligible.Count; edgeIndex++)
            {
                if (!TryClassifyBoundedEdge(
                        sourceFaces,
                        context,
                        eligible[edgeIndex],
                        solidCentre,
                        tolerance,
                        out BoundedEdgeClassificationEvidence evidence))
                {
                    audit.AmbiguousCandidateCount++;
                    continue;
                }

                switch (evidence.Classification)
                {
                    case BoundedEdgeClassification.Convex:
                        audit.ConvexCandidateCount++;
                        break;
                    case BoundedEdgeClassification.Concave:
                        audit.ConcaveCandidateCount++;
                        break;
                    case BoundedEdgeClassification.Coplanar:
                        audit.CoplanarCandidateCount++;
                        break;
                    case BoundedEdgeClassification.InvalidOrientation:
                        audit.InvalidOrientationCandidateCount++;
                        break;
                    default:
                        audit.AmbiguousCandidateCount++;
                        break;
                }
            }
        }

        private static void AuditBoundedSelectedEdgeClassification(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            EdgeWearSelectedGraphEdge selected,
            Vector3 solidCentre,
            float tolerance,
            ref BoundedSingleEdgeAuditResult audit)
        {
            audit.EdgeClassificationAttempted = 1;
            audit.EdgeClassificationTolerance = tolerance;
            if (!TryClassifyBoundedEdge(
                    sourceFaces,
                    context,
                    selected,
                    solidCentre,
                    tolerance,
                    out BoundedEdgeClassificationEvidence evidence))
            {
                audit.EdgeClassification =
                    BoundedEdgeClassification.Ambiguous;
                return;
            }

            audit.EdgeSourceFaceA = evidence.SourceFaceA;
            audit.EdgeSourceFaceB = evidence.SourceFaceB;
            audit.EdgeNormalA = evidence.NormalA;
            audit.EdgeNormalB = evidence.NormalB;
            audit.EdgeNormalDot = evidence.NormalDot;
            audit.EdgeDihedralDegrees = evidence.DihedralDegrees;
            audit.EdgeFaceAInteriorAgainstFaceB =
                evidence.FaceAInteriorAgainstFaceB;
            audit.EdgeFaceBInteriorAgainstFaceA =
                evidence.FaceBInteriorAgainstFaceA;
            audit.EdgeSolidCentreAgainstFaceA =
                evidence.SolidCentreAgainstFaceA;
            audit.EdgeSolidCentreAgainstFaceB =
                evidence.SolidCentreAgainstFaceB;
            audit.EdgeClassification = evidence.Classification;
        }

        private static bool TryClassifyBoundedEdge(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            EdgeWearSelectedGraphEdge selected,
            Vector3 solidCentre,
            float tolerance,
            out BoundedEdgeClassificationEvidence evidence)
        {
            evidence = new BoundedEdgeClassificationEvidence
            {
                SourceFaceA = -1,
                SourceFaceB = -1,
                Classification = BoundedEdgeClassification.Ambiguous
            };
            if (sourceFaces == null || context == null ||
                selected.GraphEdgeIndex < 0 ||
                selected.GraphEdgeIndex >= context.Graph.Edges.Count ||
                !IsFinite(solidCentre))
            {
                return false;
            }

            EdgeWearGraphEdge edge =
                context.Graph.Edges[selected.GraphEdgeIndex];
            if (edge.FaceA < 0 || edge.FaceB < 0 ||
                edge.FaceA >= context.Graph.Faces.Count ||
                edge.FaceB >= context.Graph.Faces.Count ||
                edge.VertexA < 0 || edge.VertexB < 0 ||
                edge.VertexA >= context.Graph.Vertices.Count ||
                edge.VertexB >= context.Graph.Vertices.Count)
            {
                return false;
            }

            int sourceFaceA =
                context.Graph.Faces[edge.FaceA].SourceFaceIndex;
            int sourceFaceB =
                context.Graph.Faces[edge.FaceB].SourceFaceIndex;
            if (sourceFaceA < 0 || sourceFaceB < 0 ||
                sourceFaceA >= sourceFaces.Count ||
                sourceFaceB >= sourceFaces.Count ||
                sourceFaceA == sourceFaceB)
            {
                return false;
            }

            PolygonFace faceA = sourceFaces[sourceFaceA];
            PolygonFace faceB = sourceFaces[sourceFaceB];
            if (faceA == null || faceB == null ||
                !IsFinite(faceA.Normal) || !IsFinite(faceB.Normal) ||
                faceA.Normal.sqrMagnitude <= MinimumEdgeLengthSqr ||
                faceB.Normal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return false;
            }

            Vector3 normalA = faceA.Normal.normalized;
            Vector3 normalB = faceB.Normal.normalized;
            float planeA = CalculateBoundedFacePlaneDistance(faceA, normalA);
            float planeB = CalculateBoundedFacePlaneDistance(faceB, normalB);
            Vector3 edgeA = context.Graph.Vertices[edge.VertexA].Position;
            Vector3 edgeB = context.Graph.Vertices[edge.VertexB].Position;
            bool measuredA = TryMeasureBoundedFaceInteriorAgainstPlane(
                faceA,
                edgeA,
                edgeB,
                normalB,
                planeB,
                tolerance,
                out float faceAAgainstB);
            bool measuredB = TryMeasureBoundedFaceInteriorAgainstPlane(
                faceB,
                edgeA,
                edgeB,
                normalA,
                planeA,
                tolerance,
                out float faceBAgainstA);

            float normalDot = Mathf.Clamp(
                Vector3.Dot(normalA, normalB),
                -1f,
                1f);
            float solidAgainstA =
                Vector3.Dot(normalA, solidCentre) - planeA;
            float solidAgainstB =
                Vector3.Dot(normalB, solidCentre) - planeB;
            BoundedEdgeClassification classification;
            if (solidAgainstA > tolerance || solidAgainstB > tolerance)
            {
                classification =
                    BoundedEdgeClassification.InvalidOrientation;
            }
            else if (!measuredA || !measuredB)
            {
                classification = BoundedEdgeClassification.Ambiguous;
            }
            else if (normalDot >= 0.9999f &&
                Mathf.Abs(faceAAgainstB) <= tolerance &&
                Mathf.Abs(faceBAgainstA) <= tolerance)
            {
                classification = BoundedEdgeClassification.Coplanar;
            }
            else if (faceAAgainstB <= tolerance &&
                faceBAgainstA <= tolerance)
            {
                classification = BoundedEdgeClassification.Convex;
            }
            else if (faceAAgainstB > tolerance ||
                faceBAgainstA > tolerance)
            {
                classification = BoundedEdgeClassification.Concave;
            }
            else
            {
                classification = BoundedEdgeClassification.Ambiguous;
            }

            evidence.SourceFaceA = sourceFaceA;
            evidence.SourceFaceB = sourceFaceB;
            evidence.NormalA = normalA;
            evidence.NormalB = normalB;
            evidence.NormalDot = normalDot;
            evidence.DihedralDegrees =
                Mathf.Acos(normalDot) * Mathf.Rad2Deg;
            evidence.FaceAInteriorAgainstFaceB = faceAAgainstB;
            evidence.FaceBInteriorAgainstFaceA = faceBAgainstA;
            evidence.SolidCentreAgainstFaceA = solidAgainstA;
            evidence.SolidCentreAgainstFaceB = solidAgainstB;
            evidence.Classification = classification;
            return true;
        }

        private static float CalculateBoundedFacePlaneDistance(
            PolygonFace face,
            Vector3 normal)
        {
            if (face == null || face.Vertices == null ||
                face.Vertices.Count == 0)
            {
                return 0f;
            }

            double sum = 0.0;
            int count = 0;
            for (int vertexIndex = 0;
                 vertexIndex < face.Vertices.Count;
                 vertexIndex++)
            {
                Vector3 vertex = face.Vertices[vertexIndex];
                if (!IsFinite(vertex))
                {
                    continue;
                }
                sum += Vector3.Dot(normal, vertex);
                count++;
            }
            return count > 0 ? (float)(sum / count) : 0f;
        }

        private static bool TryMeasureBoundedFaceInteriorAgainstPlane(
            PolygonFace face,
            Vector3 sharedEdgeA,
            Vector3 sharedEdgeB,
            Vector3 planeNormal,
            float planeDistance,
            float tolerance,
            out float maximumSignedDistance)
        {
            maximumSignedDistance = float.NegativeInfinity;
            if (face == null || face.Vertices == null)
            {
                return false;
            }

            float endpointToleranceSqr = tolerance * tolerance;
            bool measured = false;
            for (int vertexIndex = 0;
                 vertexIndex < face.Vertices.Count;
                 vertexIndex++)
            {
                Vector3 vertex = face.Vertices[vertexIndex];
                if (!IsFinite(vertex) ||
                    (vertex - sharedEdgeA).sqrMagnitude <=
                        endpointToleranceSqr ||
                    (vertex - sharedEdgeB).sqrMagnitude <=
                        endpointToleranceSqr)
                {
                    continue;
                }

                maximumSignedDistance = Mathf.Max(
                    maximumSignedDistance,
                    Vector3.Dot(planeNormal, vertex) - planeDistance);
                measured = true;
            }

            if (!measured)
            {
                maximumSignedDistance = 0f;
            }
            return measured;
        }

        private static void AuditBoundedSourceSolidContainment(
            List<PolygonFace> sourceFaces,
            List<PolygonFace> resultFaces,
            float tolerance,
            ref BoundedSingleEdgeAuditResult audit)
        {
            audit.SourceConvexityAttempted = 1;
            audit.ResultContainmentAttempted = 1;
            audit.SolidContainmentTolerance = tolerance;
            if (sourceFaces == null || resultFaces == null)
            {
                return;
            }

            for (int planeFaceIndex = 0;
                 planeFaceIndex < sourceFaces.Count;
                 planeFaceIndex++)
            {
                PolygonFace planeFace = sourceFaces[planeFaceIndex];
                if (planeFace == null || planeFace.Vertices == null ||
                    !IsFinite(planeFace.Normal) ||
                    planeFace.Normal.sqrMagnitude <= MinimumEdgeLengthSqr)
                {
                    continue;
                }

                Vector3 planeNormal = planeFace.Normal.normalized;
                float planeDistance = CalculateBoundedFacePlaneDistance(
                    planeFace,
                    planeNormal);
                for (int vertexFaceIndex = 0;
                     vertexFaceIndex < sourceFaces.Count;
                     vertexFaceIndex++)
                {
                    PolygonFace vertexFace = sourceFaces[vertexFaceIndex];
                    if (vertexFace == null || vertexFace.Vertices == null)
                    {
                        continue;
                    }
                    for (int vertexIndex = 0;
                         vertexIndex < vertexFace.Vertices.Count;
                         vertexIndex++)
                    {
                        float distance = Vector3.Dot(
                            planeNormal,
                            vertexFace.Vertices[vertexIndex]) -
                            planeDistance;
                        if (distance <= tolerance)
                        {
                            continue;
                        }

                        audit.SourceConvexityViolationCount++;
                        if (distance > audit.SourceMaximumPlaneViolation)
                        {
                            audit.SourceMaximumPlaneViolation = distance;
                            audit.SourceViolatingPlaneFace = planeFaceIndex;
                            audit.SourceViolatingVertexFace = vertexFaceIndex;
                            audit.SourceViolatingVertexIndex = vertexIndex;
                        }
                    }
                }

                for (int resultFaceIndex = 0;
                     resultFaceIndex < resultFaces.Count;
                     resultFaceIndex++)
                {
                    PolygonFace resultFace = resultFaces[resultFaceIndex];
                    if (resultFace == null || resultFace.Vertices == null)
                    {
                        continue;
                    }
                    for (int vertexIndex = 0;
                         vertexIndex < resultFace.Vertices.Count;
                         vertexIndex++)
                    {
                        float distance = Vector3.Dot(
                            planeNormal,
                            resultFace.Vertices[vertexIndex]) -
                            planeDistance;
                        if (distance <= tolerance)
                        {
                            continue;
                        }

                        audit.ResultContainmentViolationCount++;
                        if (distance > audit.ResultMaximumOutwardDistance)
                        {
                            audit.ResultMaximumOutwardDistance = distance;
                            audit.ResultViolatingFace = resultFaceIndex;
                            audit.ResultViolatingProvenanceKind =
                                resultFace.ProvenanceKind;
                            audit.ResultViolatingProvenanceIndex =
                                resultFace.ProvenanceIndex;
                            audit.ResultViolatingVertexIndex = vertexIndex;
                            audit.ResultViolatedSourcePlane = planeFaceIndex;
                        }
                    }
                }
            }
        }

        private static void AuditBoundedResultConvexity(
            List<PolygonFace> resultFaces,
            float tolerance,
            ref BoundedSingleEdgeAuditResult audit)
        {
            audit.ResultConvexityAttempted = 1;
            if (resultFaces == null)
            {
                return;
            }

            for (int planeFaceIndex = 0;
                 planeFaceIndex < resultFaces.Count;
                 planeFaceIndex++)
            {
                PolygonFace planeFace = resultFaces[planeFaceIndex];
                if (planeFace == null || planeFace.Vertices == null ||
                    planeFace.Vertices.Count < 3 ||
                    !IsFinite(planeFace.Normal) ||
                    planeFace.Normal.sqrMagnitude <= MinimumEdgeLengthSqr)
                {
                    continue;
                }

                Vector3 planeNormal = planeFace.Normal.normalized;
                float planeDistance = CalculateBoundedFacePlaneDistance(
                    planeFace,
                    planeNormal);
                for (int vertexFaceIndex = 0;
                     vertexFaceIndex < resultFaces.Count;
                     vertexFaceIndex++)
                {
                    PolygonFace vertexFace = resultFaces[vertexFaceIndex];
                    if (vertexFace == null || vertexFace.Vertices == null)
                    {
                        continue;
                    }

                    for (int vertexIndex = 0;
                         vertexIndex < vertexFace.Vertices.Count;
                         vertexIndex++)
                    {
                        float distance = Vector3.Dot(
                            planeNormal,
                            vertexFace.Vertices[vertexIndex]) -
                            planeDistance;
                        if (distance <= tolerance)
                        {
                            continue;
                        }

                        audit.ResultConvexityViolationCount++;
                        if (distance <=
                            audit.ResultMaximumConvexityViolation)
                        {
                            continue;
                        }

                        audit.ResultMaximumConvexityViolation = distance;
                        audit.ResultConvexityPlaneFace = planeFaceIndex;
                        audit.ResultConvexityPlaneProvenanceKind =
                            planeFace.ProvenanceKind;
                        audit.ResultConvexityPlaneProvenanceIndex =
                            planeFace.ProvenanceIndex;
                        audit.ResultConvexityVertexFace = vertexFaceIndex;
                        audit.ResultConvexityVertexProvenanceKind =
                            vertexFace.ProvenanceKind;
                        audit.ResultConvexityVertexProvenanceIndex =
                            vertexFace.ProvenanceIndex;
                        audit.ResultConvexityVertexIndex = vertexIndex;
                    }
                }
            }
        }

        private static BoundedFaceIntersectionAudit
            AuditBoundedFaceIntersections(
                List<PolygonFace> faces,
                ChamferTopologyContext context,
                float minimumStableFaceArea,
                float tolerance)
        {
            BoundedFaceIntersectionAudit audit =
                new BoundedFaceIntersectionAudit
                {
                    Attempted = 1,
                    FirstFaceA = -1,
                    FirstFaceAProvenanceIndex = -1,
                    FirstFaceB = -1,
                    FirstFaceBProvenanceIndex = -1,
                    Pairs = new List<BoundedFaceIntersectionPairEvidence>(),
                    PairEvidence = "none"
                };
            if (faces == null)
            {
                return audit;
            }

            float triangleArea = Mathf.Max(
                minimumStableFaceArea * 0.0001f,
                0.0000000001f);
            float epsilon = Mathf.Max(
                tolerance,
                PointMergeDistance * 8f);
            List<List<ChamferDirectedTriangleGeometry>> trianglesByFace =
                new List<List<ChamferDirectedTriangleGeometry>>(
                    faces.Count);
            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                trianglesByFace.Add(BuildChamferDirectedFaceTriangles(
                    faces[faceIndex],
                    faceIndex,
                    triangleArea));
            }

            for (int firstFaceIndex = 0;
                 firstFaceIndex < faces.Count;
                 firstFaceIndex++)
            {
                PolygonFace firstFace = faces[firstFaceIndex];
                if (firstFace == null || firstFace.Vertices == null)
                {
                    continue;
                }

                for (int secondFaceIndex = firstFaceIndex + 1;
                     secondFaceIndex < faces.Count;
                     secondFaceIndex++)
                {
                    PolygonFace secondFace = faces[secondFaceIndex];
                    if (secondFace == null || secondFace.Vertices == null)
                    {
                        continue;
                    }

                    bool improperIntersection = false;
                    List<ChamferDirectedTriangleGeometry> firstTriangles =
                        trianglesByFace[firstFaceIndex];
                    List<ChamferDirectedTriangleGeometry> secondTriangles =
                        trianglesByFace[secondFaceIndex];
                    for (int firstTriangleIndex = 0;
                         firstTriangleIndex < firstTriangles.Count &&
                             !improperIntersection;
                         firstTriangleIndex++)
                    {
                        for (int secondTriangleIndex = 0;
                             secondTriangleIndex < secondTriangles.Count;
                             secondTriangleIndex++)
                        {
                            if (!ChamferDirectedTrianglesIntersectImproperly(
                                    firstTriangles[firstTriangleIndex],
                                    secondTriangles[secondTriangleIndex],
                                    epsilon))
                            {
                                continue;
                            }

                            improperIntersection = true;
                            break;
                        }
                    }

                    if (!improperIntersection)
                    {
                        continue;
                    }

                    float normalAlignment = Mathf.Abs(Vector3.Dot(
                        firstFace.Normal,
                        secondFace.Normal));
                    float planeDistance = Mathf.Abs(Vector3.Dot(
                        secondFace.Vertices[0] - firstFace.Vertices[0],
                        firstFace.Normal));
                    int coplanar = normalAlignment >= 0.9999f &&
                        planeDistance <= epsilon
                            ? 1
                            : 0;
                    int sharedVertexCount = CountBoundedSharedVertices(
                        firstFace.Vertices,
                        secondFace.Vertices,
                        epsilon);
                    int sharedBoundaryEdgeCount =
                        CountBoundedSharedBoundaryEdges(
                            firstFace.Vertices,
                            secondFace.Vertices,
                            epsilon);
                    int graphAdjacent = GetBoundedSourceGraphAdjacency(
                        firstFace,
                        secondFace,
                        context);
                    int boundaryContact = sharedVertexCount > 0 ||
                        sharedBoundaryEdgeCount > 0
                            ? 1
                            : 0;
                    int improperInterior = coplanar == 1 ||
                        boundaryContact == 0
                            ? 1
                            : 0;

                    BoundedIntersectionFaceKey firstKey =
                        new BoundedIntersectionFaceKey(
                            firstFace.ProvenanceKind,
                            firstFace.ProvenanceIndex);
                    BoundedIntersectionFaceKey secondKey =
                        new BoundedIntersectionFaceKey(
                            secondFace.ProvenanceKind,
                            secondFace.ProvenanceIndex);
                    int storedFirstFaceIndex = firstFaceIndex;
                    int storedSecondFaceIndex = secondFaceIndex;
                    if (CompareBoundedIntersectionFaceKeys(
                            firstKey,
                            secondKey) > 0)
                    {
                        BoundedIntersectionFaceKey swapKey = firstKey;
                        firstKey = secondKey;
                        secondKey = swapKey;
                        int swapIndex = storedFirstFaceIndex;
                        storedFirstFaceIndex = storedSecondFaceIndex;
                        storedSecondFaceIndex = swapIndex;
                    }

                    BoundedFaceIntersectionPairEvidence pair =
                        new BoundedFaceIntersectionPairEvidence(
                            firstKey,
                            secondKey,
                            storedFirstFaceIndex,
                            storedSecondFaceIndex,
                            coplanar,
                            sharedVertexCount,
                            sharedBoundaryEdgeCount,
                            graphAdjacent,
                            boundaryContact,
                            improperInterior);
                    audit.Pairs.Add(pair);
                    audit.PairCount++;
                    if (coplanar == 1)
                    {
                        audit.CoplanarPairCount++;
                    }
                    else
                    {
                        audit.NonCoplanarPairCount++;
                    }
                    if (boundaryContact == 1)
                    {
                        audit.BoundaryContactPairCount++;
                    }
                    if (improperInterior == 1)
                    {
                        audit.ImproperInteriorPairCount++;
                    }

                    if (audit.FirstFaceA >= 0)
                    {
                        continue;
                    }

                    audit.FirstFaceA = storedFirstFaceIndex;
                    audit.FirstFaceAProvenanceKind = firstKey.Kind;
                    audit.FirstFaceAProvenanceIndex = firstKey.Index;
                    audit.FirstFaceB = storedSecondFaceIndex;
                    audit.FirstFaceBProvenanceKind = secondKey.Kind;
                    audit.FirstFaceBProvenanceIndex = secondKey.Index;
                }
            }

            audit.PairEvidence = FormatBoundedIntersectionPairEvidence(
                audit.Pairs);
            return audit;
        }

        private static void ApplyBoundedFaceIntersectionDelta(
            BoundedFaceIntersectionAudit sourceAudit,
            BoundedFaceIntersectionAudit resultAudit,
            ref BoundedSingleEdgeAuditResult result)
        {
            result.SourceFaceIntersectionAttempted = sourceAudit.Attempted;
            result.SourceFaceIntersectionPairCount = sourceAudit.PairCount;
            result.SourceCoplanarOverlapPairCount =
                sourceAudit.CoplanarPairCount;
            result.SourceNonCoplanarIntersectionPairCount =
                sourceAudit.NonCoplanarPairCount;
            result.SourceBoundaryContactPairCount =
                sourceAudit.BoundaryContactPairCount;
            result.SourceImproperInteriorPairCount =
                sourceAudit.ImproperInteriorPairCount;
            result.SourceIntersectionPairEvidence =
                sourceAudit.PairEvidence;

            result.FaceIntersectionAttempted = resultAudit.Attempted;
            result.FaceIntersectionPairCount = resultAudit.PairCount;
            result.CoplanarOverlapPairCount =
                resultAudit.CoplanarPairCount;
            result.NonCoplanarIntersectionPairCount =
                resultAudit.NonCoplanarPairCount;
            result.ResultBoundaryContactPairCount =
                resultAudit.BoundaryContactPairCount;
            result.ResultImproperInteriorPairCount =
                resultAudit.ImproperInteriorPairCount;
            result.ResultIntersectionPairEvidence =
                resultAudit.PairEvidence;
            result.FirstIntersectionFaceA = resultAudit.FirstFaceA;
            result.FirstIntersectionFaceAProvenanceKind =
                resultAudit.FirstFaceAProvenanceKind;
            result.FirstIntersectionFaceAProvenanceIndex =
                resultAudit.FirstFaceAProvenanceIndex;
            result.FirstIntersectionFaceB = resultAudit.FirstFaceB;
            result.FirstIntersectionFaceBProvenanceKind =
                resultAudit.FirstFaceBProvenanceKind;
            result.FirstIntersectionFaceBProvenanceIndex =
                resultAudit.FirstFaceBProvenanceIndex;

            List<BoundedFaceIntersectionPairEvidence> unchanged =
                new List<BoundedFaceIntersectionPairEvidence>();
            List<BoundedFaceIntersectionPairEvidence> changed =
                new List<BoundedFaceIntersectionPairEvidence>();
            List<BoundedFaceIntersectionPairEvidence> added =
                new List<BoundedFaceIntersectionPairEvidence>();
            List<BoundedFaceIntersectionPairEvidence> resolved =
                new List<BoundedFaceIntersectionPairEvidence>();

            for (int resultIndex = 0;
                 resultIndex < resultAudit.Pairs.Count;
                 resultIndex++)
            {
                BoundedFaceIntersectionPairEvidence resultPair =
                    resultAudit.Pairs[resultIndex];
                int sourceIndex = FindBoundedIntersectionPair(
                    sourceAudit.Pairs,
                    resultPair);
                if (sourceIndex < 0)
                {
                    added.Add(resultPair);
                    result.NewIntersectionPairCount++;
                    if (resultPair.ImproperInterior == 0)
                    {
                        result.NewBoundaryContactPairCount++;
                    }
                    else
                    {
                        result.NewImproperInteriorIntersectionPairCount++;
                    }
                    continue;
                }

                BoundedFaceIntersectionPairEvidence sourcePair =
                    sourceAudit.Pairs[sourceIndex];
                if (AreBoundedIntersectionPairEvidenceEquivalent(
                        sourcePair,
                        resultPair))
                {
                    unchanged.Add(resultPair);
                    result.UnchangedIntersectionPairCount++;
                }
                else
                {
                    changed.Add(resultPair);
                    result.ChangedIntersectionPairCount++;
                    if (resultPair.ImproperInterior == 1)
                    {
                        result.ChangedImproperInteriorIntersectionPairCount++;
                    }
                }
            }

            for (int sourceIndex = 0;
                 sourceIndex < sourceAudit.Pairs.Count;
                 sourceIndex++)
            {
                BoundedFaceIntersectionPairEvidence sourcePair =
                    sourceAudit.Pairs[sourceIndex];
                if (FindBoundedIntersectionPair(
                        resultAudit.Pairs,
                        sourcePair) >= 0)
                {
                    continue;
                }

                resolved.Add(sourcePair);
                result.ResolvedIntersectionPairCount++;
            }

            result.IntroducedImproperInteriorIntersectionPairCount =
                result.NewImproperInteriorIntersectionPairCount +
                result.ChangedImproperInteriorIntersectionPairCount;
            result.UnchangedIntersectionPairEvidence =
                FormatBoundedIntersectionPairEvidence(unchanged);
            result.ChangedIntersectionPairEvidence =
                FormatBoundedIntersectionPairEvidence(changed);
            result.NewIntersectionPairEvidence =
                FormatBoundedIntersectionPairEvidence(added);
            result.ResolvedIntersectionPairEvidence =
                FormatBoundedIntersectionPairEvidence(resolved);
        }

        private static int FindBoundedIntersectionPair(
            List<BoundedFaceIntersectionPairEvidence> pairs,
            BoundedFaceIntersectionPairEvidence target)
        {
            if (pairs == null)
            {
                return -1;
            }

            for (int pairIndex = 0;
                 pairIndex < pairs.Count;
                 pairIndex++)
            {
                BoundedFaceIntersectionPairEvidence pair = pairs[pairIndex];
                if (pair.First.Equals(target.First) &&
                    pair.Second.Equals(target.Second))
                {
                    return pairIndex;
                }
            }
            return -1;
        }

        private static bool AreBoundedIntersectionPairEvidenceEquivalent(
            BoundedFaceIntersectionPairEvidence first,
            BoundedFaceIntersectionPairEvidence second)
        {
            return first.Coplanar == second.Coplanar &&
                first.SharedVertexCount == second.SharedVertexCount &&
                first.SharedBoundaryEdgeCount ==
                    second.SharedBoundaryEdgeCount &&
                first.GraphAdjacent == second.GraphAdjacent &&
                first.BoundaryContact == second.BoundaryContact &&
                first.ImproperInterior == second.ImproperInterior;
        }

        private static int CompareBoundedIntersectionFaceKeys(
            BoundedIntersectionFaceKey first,
            BoundedIntersectionFaceKey second)
        {
            int kindComparison = ((int)first.Kind).CompareTo(
                (int)second.Kind);
            return kindComparison != 0
                ? kindComparison
                : first.Index.CompareTo(second.Index);
        }

        private static int CountBoundedSharedVertices(
            List<Vector3> first,
            List<Vector3> second,
            float tolerance)
        {
            if (first == null || second == null)
            {
                return 0;
            }

            float toleranceSquared = tolerance * tolerance;
            int count = 0;
            for (int firstIndex = 0;
                 firstIndex < first.Count;
                 firstIndex++)
            {
                for (int secondIndex = 0;
                     secondIndex < second.Count;
                     secondIndex++)
                {
                    if ((first[firstIndex] - second[secondIndex])
                            .sqrMagnitude > toleranceSquared)
                    {
                        continue;
                    }

                    count++;
                    break;
                }
            }
            return count;
        }

        private static int CountBoundedSharedBoundaryEdges(
            List<Vector3> first,
            List<Vector3> second,
            float tolerance)
        {
            if (first == null || second == null ||
                first.Count < 2 || second.Count < 2)
            {
                return 0;
            }

            float toleranceSquared = tolerance * tolerance;
            int count = 0;
            for (int firstIndex = 0;
                 firstIndex < first.Count;
                 firstIndex++)
            {
                Vector3 firstStart = first[firstIndex];
                Vector3 firstEnd = first[(firstIndex + 1) % first.Count];
                for (int secondIndex = 0;
                     secondIndex < second.Count;
                     secondIndex++)
                {
                    Vector3 secondStart = second[secondIndex];
                    Vector3 secondEnd =
                        second[(secondIndex + 1) % second.Count];
                    bool sameDirection =
                        (firstStart - secondStart).sqrMagnitude <=
                            toleranceSquared &&
                        (firstEnd - secondEnd).sqrMagnitude <=
                            toleranceSquared;
                    bool oppositeDirection =
                        (firstStart - secondEnd).sqrMagnitude <=
                            toleranceSquared &&
                        (firstEnd - secondStart).sqrMagnitude <=
                            toleranceSquared;
                    if (!sameDirection && !oppositeDirection)
                    {
                        continue;
                    }

                    count++;
                    break;
                }
            }
            return count;
        }

        private static int GetBoundedSourceGraphAdjacency(
            PolygonFace first,
            PolygonFace second,
            ChamferTopologyContext context)
        {
            if (first == null || second == null || context == null ||
                first.ProvenanceKind !=
                    PolygonFaceProvenanceKind.SourceFace ||
                second.ProvenanceKind !=
                    PolygonFaceProvenanceKind.SourceFace)
            {
                return -1;
            }

            for (int edgeIndex = 0;
                 edgeIndex < context.Graph.Edges.Count;
                 edgeIndex++)
            {
                EdgeWearGraphEdge edge = context.Graph.Edges[edgeIndex];
                if (edge.FaceA < 0 || edge.FaceB < 0 ||
                    edge.FaceA >= context.Graph.Faces.Count ||
                    edge.FaceB >= context.Graph.Faces.Count)
                {
                    continue;
                }

                int sourceFaceA =
                    context.Graph.Faces[edge.FaceA].SourceFaceIndex;
                int sourceFaceB =
                    context.Graph.Faces[edge.FaceB].SourceFaceIndex;
                bool matches =
                    sourceFaceA == first.ProvenanceIndex &&
                    sourceFaceB == second.ProvenanceIndex ||
                    sourceFaceA == second.ProvenanceIndex &&
                    sourceFaceB == first.ProvenanceIndex;
                if (matches)
                {
                    return 1;
                }
            }
            return 0;
        }

        private static string FormatBoundedIntersectionPairEvidence(
            List<BoundedFaceIntersectionPairEvidence> pairs)
        {
            if (pairs == null || pairs.Count == 0)
            {
                return "none";
            }

            List<string> entries = new List<string>(pairs.Count);
            for (int pairIndex = 0;
                 pairIndex < pairs.Count;
                 pairIndex++)
            {
                BoundedFaceIntersectionPairEvidence pair = pairs[pairIndex];
                entries.Add(
                    pair.First.Kind + ":" + pair.First.Index + "~" +
                    pair.Second.Kind + ":" + pair.Second.Index +
                    "[faces=" + pair.FirstFaceIndex + ":" +
                        pair.SecondFaceIndex +
                    ";type=" +
                        (pair.Coplanar == 1
                            ? "coplanar"
                            : "nonCoplanar") +
                    ";sharedVertices=" + pair.SharedVertexCount +
                    ";sharedEdges=" + pair.SharedBoundaryEdgeCount +
                    ";graphAdjacent=" + pair.GraphAdjacent +
                    ";boundaryContact=" + pair.BoundaryContact +
                    ";improperInterior=" + pair.ImproperInterior + "]");
            }
            return string.Join("|", entries);
        }

        private static void AuditBoundedLocalVolumeAttribution(
            List<PolygonFace> sourceFaces,
            List<PolygonFace> preparedSourceFaces,
            List<PolygonFace> resultFaces,
            ChamferTopologyContext context,
            EdgeWearSelectedGraphEdge selected,
            Vector3 referencePoint,
            ref BoundedSingleEdgeAuditResult audit)
        {
            audit.LocalVolumeAttempted = 1;
            if (sourceFaces == null || preparedSourceFaces == null ||
                resultFaces == null || context == null ||
                selected.GraphEdgeIndex < 0 ||
                selected.GraphEdgeIndex >= context.Graph.Edges.Count)
            {
                return;
            }

            EdgeWearGraphEdge edge =
                context.Graph.Edges[selected.GraphEdgeIndex];
            if (edge.FaceA < 0 || edge.FaceB < 0 ||
                edge.FaceA >= context.Graph.Faces.Count ||
                edge.FaceB >= context.Graph.Faces.Count)
            {
                return;
            }
            int sourceFaceA =
                context.Graph.Faces[edge.FaceA].SourceFaceIndex;
            int sourceFaceB =
                context.Graph.Faces[edge.FaceB].SourceFaceIndex;
            int supportFaceA = audit.EndpointSupportFaceA;
            int supportFaceB = audit.EndpointSupportFaceB;
            if (sourceFaceA < 0 || sourceFaceB < 0 ||
                sourceFaceA >= sourceFaces.Count ||
                sourceFaceB >= sourceFaces.Count ||
                supportFaceA < 0 || supportFaceB < 0 ||
                supportFaceA >= sourceFaces.Count ||
                supportFaceB >= sourceFaces.Count ||
                supportFaceA == supportFaceB ||
                supportFaceA == sourceFaceA ||
                supportFaceA == sourceFaceB ||
                supportFaceB == sourceFaceA ||
                supportFaceB == sourceFaceB)
            {
                return;
            }

            double sourceSigned =
                CalculateBoundedSignedPolyhedronVolume(sourceFaces);
            double preparedSigned =
                CalculateBoundedSignedPolyhedronVolume(preparedSourceFaces);
            double resultSigned =
                CalculateBoundedSignedPolyhedronVolume(resultFaces);
            double orientationSign = sourceSigned < 0.0 ? -1.0 : 1.0;
            audit.SourceSignedVolume = sourceSigned;
            audit.PreparedSourceSignedVolume = preparedSigned;
            audit.ResultSignedVolume = resultSigned;
            audit.ResultAbsoluteVolume = Math.Abs(resultSigned);

            audit.OriginalOwnerAContribution = orientationSign *
                CalculateBoundedFaceSignedVolumeRelativeToReference(
                    sourceFaces[sourceFaceA],
                    referencePoint);
            audit.OriginalOwnerBContribution = orientationSign *
                CalculateBoundedFaceSignedVolumeRelativeToReference(
                    sourceFaces[sourceFaceB],
                    referencePoint);
            audit.OriginalOwnerContribution =
                audit.OriginalOwnerAContribution +
                audit.OriginalOwnerBContribution;
            audit.OriginalSupportAContribution = orientationSign *
                CalculateBoundedFaceSignedVolumeRelativeToReference(
                    sourceFaces[supportFaceA],
                    referencePoint);
            audit.OriginalSupportBContribution = orientationSign *
                CalculateBoundedFaceSignedVolumeRelativeToReference(
                    sourceFaces[supportFaceB],
                    referencePoint);
            audit.OriginalSupportContribution =
                audit.OriginalSupportAContribution +
                audit.OriginalSupportBContribution;

            double originalForeign = 0.0;
            for (int faceIndex = 0;
                 faceIndex < sourceFaces.Count;
                 faceIndex++)
            {
                if (faceIndex == sourceFaceA ||
                    faceIndex == sourceFaceB ||
                    faceIndex == supportFaceA ||
                    faceIndex == supportFaceB)
                {
                    continue;
                }
                originalForeign += orientationSign *
                    CalculateBoundedFaceSignedVolumeRelativeToReference(
                        sourceFaces[faceIndex],
                        referencePoint);
            }
            audit.OriginalForeignContribution = originalForeign;

            double resultForeign = 0.0;
            for (int faceIndex = 0;
                 faceIndex < resultFaces.Count;
                 faceIndex++)
            {
                PolygonFace face = resultFaces[faceIndex];
                double contribution = orientationSign *
                    CalculateBoundedFaceSignedVolumeRelativeToReference(
                        face,
                        referencePoint);
                if (face.ProvenanceKind ==
                        PolygonFaceProvenanceKind.SourceFace &&
                    face.ProvenanceIndex == sourceFaceA)
                {
                    audit.ReplacementOwnerAContribution += contribution;
                }
                else if (face.ProvenanceKind ==
                             PolygonFaceProvenanceKind.SourceFace &&
                    face.ProvenanceIndex == sourceFaceB)
                {
                    audit.ReplacementOwnerBContribution += contribution;
                }
                else if (face.ProvenanceKind ==
                             PolygonFaceProvenanceKind.SourceFace &&
                    face.ProvenanceIndex == supportFaceA)
                {
                    audit.ReplacementSupportAContribution += contribution;
                }
                else if (face.ProvenanceKind ==
                             PolygonFaceProvenanceKind.SourceFace &&
                    face.ProvenanceIndex == supportFaceB)
                {
                    audit.ReplacementSupportBContribution += contribution;
                }
                else if (face.ProvenanceKind ==
                             PolygonFaceProvenanceKind.BoundedEdgeBevel &&
                    face.ProvenanceIndex == selected.GraphEdgeIndex)
                {
                    audit.BevelContribution += contribution;
                }
                else if (face.ProvenanceKind ==
                             PolygonFaceProvenanceKind.BoundedEndpointCap &&
                    face.ProvenanceIndex == edge.VertexA)
                {
                    audit.CapAContribution += contribution;
                }
                else if (face.ProvenanceKind ==
                             PolygonFaceProvenanceKind.BoundedEndpointCap &&
                    face.ProvenanceIndex == edge.VertexB)
                {
                    audit.CapBContribution += contribution;
                }
                else if (face.ProvenanceKind ==
                    PolygonFaceProvenanceKind.SourceFace)
                {
                    resultForeign += contribution;
                }
            }

            audit.ReplacementOwnerContribution =
                audit.ReplacementOwnerAContribution +
                audit.ReplacementOwnerBContribution;
            audit.ReplacementSupportContribution =
                audit.ReplacementSupportAContribution +
                audit.ReplacementSupportBContribution;
            audit.ResultForeignContribution = resultForeign;
            audit.ForeignContributionDelta =
                resultForeign - originalForeign;
            audit.LocalReplacementDelta =
                audit.ReplacementOwnerContribution +
                audit.ReplacementSupportContribution +
                audit.BevelContribution +
                audit.CapAContribution +
                audit.CapBContribution -
                audit.OriginalOwnerContribution -
                audit.OriginalSupportContribution;
            audit.GlobalSignedVolumeDelta = orientationSign *
                (resultSigned - sourceSigned);
            audit.LocalGlobalResidual =
                audit.LocalReplacementDelta +
                audit.ForeignContributionDelta -
                audit.GlobalSignedVolumeDelta;
        }

        private static void AuditBoundedBevelPlaneSidedness(
            List<PolygonFace> resultFaces,
            ChamferTopologyContext context,
            EdgeWearSelectedGraphEdge selected,
            BoundedIsolatedRailPoint[] rails,
            Vector3 solidCentre,
            ref BoundedSingleEdgeAuditResult audit)
        {
            audit.BevelPlaneAttempted = 1;
            if (resultFaces == null || context == null ||
                rails == null || rails.Length != 4 ||
                selected.GraphEdgeIndex < 0 ||
                selected.GraphEdgeIndex >= context.Graph.Edges.Count)
            {
                return;
            }

            if (audit.MultiSupportPlaneCut == 1 &&
                audit.MultiSupportPlaneCount > 0)
            {
                float maximumRailResidual = 0f;
                float minimumAgreement = 1f;
                float maximumSolidCentreSide =
                    float.NegativeInfinity;
                Vector3 selectedBevelNormal;
                if (!TryNormalizeMassVector(
                        selected.Candidate.BevelNormal,
                        out selectedBevelNormal))
                {
                    selectedBevelNormal = Vector3.zero;
                }
                int bevelFaceCount = 0;
                for (int faceIndex = 0;
                     faceIndex < resultFaces.Count;
                     faceIndex++)
                {
                    PolygonFace face = resultFaces[faceIndex];
                    if (face.ProvenanceKind !=
                            PolygonFaceProvenanceKind.BoundedEdgeBevel ||
                        face.ProvenanceIndex != selected.GraphEdgeIndex ||
                        face.Vertices.Count == 0)
                    {
                        continue;
                    }
                    bevelFaceCount++;
                    float faceDistance = Vector3.Dot(
                        face.Normal,
                        face.Vertices[0]);
                    minimumAgreement = Mathf.Min(
                        minimumAgreement,
                        Mathf.Abs(Vector3.Dot(
                            selectedBevelNormal,
                            face.Normal)));
                    for (int railIndex = 0;
                         railIndex < rails.Length;
                         railIndex++)
                    {
                        float minimumResidual = float.PositiveInfinity;
                        for (int candidateFaceIndex = 0;
                             candidateFaceIndex < resultFaces.Count;
                             candidateFaceIndex++)
                        {
                            PolygonFace candidateFace =
                                resultFaces[candidateFaceIndex];
                            if (candidateFace.ProvenanceKind !=
                                    PolygonFaceProvenanceKind.BoundedEdgeBevel ||
                                candidateFace.ProvenanceIndex !=
                                    selected.GraphEdgeIndex ||
                                candidateFace.Vertices.Count == 0)
                            {
                                continue;
                            }
                            float candidateDistance = Vector3.Dot(
                                candidateFace.Normal,
                                candidateFace.Vertices[0]);
                            minimumResidual = Mathf.Min(
                                minimumResidual,
                                Mathf.Abs(Vector3.Dot(
                                    candidateFace.Normal,
                                    rails[railIndex].Position) -
                                    candidateDistance));
                        }
                        maximumRailResidual = Mathf.Max(
                            maximumRailResidual,
                            minimumResidual);
                    }
                    audit.BevelFaceNormal = face.Normal;
                    maximumSolidCentreSide = Mathf.Max(
                        maximumSolidCentreSide,
                        Vector3.Dot(face.Normal, solidCentre) -
                        faceDistance);
                }
                audit.BevelPlaneNormal =
                    audit.MultiSupportPlaneNormal;
                audit.BevelPlaneDistance =
                    audit.MultiSupportPlaneDistance;
                audit.BevelPlaneNormalAgreement = bevelFaceCount > 0
                    ? minimumAgreement
                    : 0f;
                audit.BevelSolidCentreSide = bevelFaceCount > 0
                    ? maximumSolidCentreSide
                    : 0f;
                audit.BevelRailMaximumPlaneResidual =
                    maximumRailResidual;
                return;
            }

            Vector3 normal = selected.Candidate.BevelNormal;
            if (!IsFinite(normal) ||
                normal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return;
            }
            normal.Normalize();
            float distance = 0f;
            float maximumResidual = 0f;
            for (int railIndex = 0; railIndex < rails.Length; railIndex++)
            {
                distance += Vector3.Dot(normal, rails[railIndex].Position);
            }
            distance /= rails.Length;
            for (int railIndex = 0; railIndex < rails.Length; railIndex++)
            {
                maximumResidual = Mathf.Max(
                    maximumResidual,
                    Mathf.Abs(Vector3.Dot(
                        normal,
                        rails[railIndex].Position) - distance));
            }

            EdgeWearGraphEdge edge =
                context.Graph.Edges[selected.GraphEdgeIndex];
            Vector3 sourceA =
                context.Graph.Vertices[edge.VertexA].Position;
            Vector3 sourceB =
                context.Graph.Vertices[edge.VertexB].Position;
            audit.BevelPlaneNormal = normal;
            audit.BevelPlaneDistance = distance;
            audit.BevelSolidCentreSide =
                Vector3.Dot(normal, solidCentre) - distance;
            audit.BevelSourceEdgeASide =
                Vector3.Dot(normal, sourceA) - distance;
            audit.BevelSourceEdgeBSide =
                Vector3.Dot(normal, sourceB) - distance;
            audit.BevelRailMaximumPlaneResidual = maximumResidual;

            for (int faceIndex = 0;
                 faceIndex < resultFaces.Count;
                 faceIndex++)
            {
                PolygonFace face = resultFaces[faceIndex];
                if (face.ProvenanceKind !=
                        PolygonFaceProvenanceKind.BoundedEdgeBevel ||
                    face.ProvenanceIndex != selected.GraphEdgeIndex)
                {
                    continue;
                }
                audit.BevelFaceNormal = face.Normal;
                audit.BevelPlaneNormalAgreement =
                    Vector3.Dot(normal, face.Normal);
                break;
            }
        }

        private static double CalculateBoundedSignedPolyhedronVolume(
            List<PolygonFace> faces)
        {
            double signedVolume = 0.0;
            if (faces == null)
            {
                return signedVolume;
            }
            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face == null || face.Vertices == null ||
                    face.Vertices.Count < 3)
                {
                    continue;
                }
                Vector3 anchor = face.Vertices[0];
                for (int vertexIndex = 1;
                     vertexIndex < face.Vertices.Count - 1;
                     vertexIndex++)
                {
                    signedVolume += Vector3.Dot(
                        anchor,
                        Vector3.Cross(
                            face.Vertices[vertexIndex],
                            face.Vertices[vertexIndex + 1])) / 6.0;
                }
            }
            return signedVolume;
        }

        private static double CalculateBoundedFaceSignedVolumeRelativeToReference(
            PolygonFace face,
            Vector3 referencePoint)
        {
            if (face == null || face.Vertices == null ||
                face.Vertices.Count < 3)
            {
                return 0.0;
            }

            double signedVolume = 0.0;
            Vector3 anchor = face.Vertices[0] - referencePoint;
            for (int vertexIndex = 1;
                 vertexIndex < face.Vertices.Count - 1;
                 vertexIndex++)
            {
                Vector3 b = face.Vertices[vertexIndex] - referencePoint;
                Vector3 c =
                    face.Vertices[vertexIndex + 1] - referencePoint;
                signedVolume += Vector3.Dot(
                    anchor,
                    Vector3.Cross(b, c)) / 6.0;
            }
            return signedVolume;
        }

        private static double CalculateBoundedTriangleSoupSignedVolume(
            TriangleSoup soup)
        {
            double signedVolume = 0.0;
            if (soup == null)
            {
                return signedVolume;
            }
            for (int triangleIndex = 0;
                 triangleIndex + 2 < soup.Positions.Count;
                 triangleIndex += 3)
            {
                Vector3 a = soup.Positions[triangleIndex];
                Vector3 b = soup.Positions[triangleIndex + 1];
                Vector3 c = soup.Positions[triangleIndex + 2];
                signedVolume += Vector3.Dot(
                    a,
                    Vector3.Cross(b, c)) / 6.0;
            }
            return signedVolume;
        }

        private static List<EdgeWearSelectedGraphEdge>
            BuildBoundedSingleEdgeEligibleList(
                ChamferTopologyContext context)
        {
            List<EdgeWearSelectedGraphEdge> eligible =
                new List<EdgeWearSelectedGraphEdge>();
            for (int selectedIndex = 0;
                 selectedIndex < context.SelectedEdges.Count;
                 selectedIndex++)
            {
                EdgeWearSelectedGraphEdge selected =
                    context.SelectedEdges[selectedIndex];
                if (selected.GraphEdgeIndex < 0 ||
                    selected.GraphEdgeIndex >= context.Graph.Edges.Count)
                {
                    continue;
                }

                EdgeWearGraphEdge edge =
                    context.Graph.Edges[selected.GraphEdgeIndex];
                if (edge.FaceA < 0 || edge.FaceB < 0 ||
                    edge.FaceA >= context.Graph.Faces.Count ||
                    edge.FaceB >= context.Graph.Faces.Count)
                {
                    continue;
                }

                eligible.Add(selected);
            }

            eligible.Sort((left, right) =>
                left.GraphEdgeIndex.CompareTo(right.GraphEdgeIndex));
            return eligible;
        }

        private static bool TrySolveBoundedIsolatedSingleEdgeRails(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            EdgeWearSelectedGraphEdge selected,
            float requestedWidth,
            float minimumStableEdgeLength,
            out BoundedIsolatedRailPoint[] rails,
            out int widthAttemptCount,
            out float solvedWidth,
            out string blocker)
        {
            return TrySolveBoundedIsolatedSingleEdgeRails(
                sourceFaces,
                context,
                selected,
                requestedWidth,
                minimumStableEdgeLength,
                out rails,
                out _,
                out _,
                out _,
                out widthAttemptCount,
                out solvedWidth,
                out blocker);
        }

        private static bool TrySolveBoundedIsolatedSingleEdgeRails(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            EdgeWearSelectedGraphEdge selected,
            float requestedWidth,
            float minimumStableEdgeLength,
            out BoundedIsolatedRailPoint[] rails,
            out List<EdgeWearIsolatedWidthAttemptRecord> widthAttempts,
            out float minimumWidth,
            out bool scheduleComplete,
            out int widthAttemptCount,
            out float solvedWidth,
            out string blocker)
        {
            return TrySolveBoundedIsolatedSingleEdgeRails(
                sourceFaces,
                context,
                selected,
                requestedWidth,
                minimumStableEdgeLength,
                out rails,
                out widthAttempts,
                out minimumWidth,
                out scheduleComplete,
                out widthAttemptCount,
                out solvedWidth,
                out blocker,
                BoundedIsolatedMaximumWidthAttempts);
        }

        private static bool TrySolveBoundedIsolatedSingleEdgeRails(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            EdgeWearSelectedGraphEdge selected,
            float requestedWidth,
            float minimumStableEdgeLength,
            out BoundedIsolatedRailPoint[] rails,
            out List<EdgeWearIsolatedWidthAttemptRecord> widthAttempts,
            out float minimumWidth,
            out bool scheduleComplete,
            out int widthAttemptCount,
            out float solvedWidth,
            out string blocker,
            int maximumWidthAttempts)
        {
            int boundedMaximumWidthAttempts = Mathf.Clamp(
                maximumWidthAttempts,
                1,
                BoundedIsolatedMaximumWidthAttempts);

            rails = null;
            widthAttempts =
                new List<EdgeWearIsolatedWidthAttemptRecord>(
                    boundedMaximumWidthAttempts);
            minimumWidth = 0f;
            scheduleComplete = false;
            widthAttemptCount = 0;
            solvedWidth = 0f;
            blocker = string.Empty;

            int selectedEdgeIndex = selected.GraphEdgeIndex;
            if (selectedEdgeIndex < 0 ||
                selectedEdgeIndex >= context.Graph.Edges.Count)
            {
                blocker = "the selected edge is absent from the topology graph";
                return false;
            }

            minimumWidth = Mathf.Max(
                minimumStableEdgeLength,
                PointMergeDistance * 4f);
            float initialWidth = CalculateChamferEdgeWidth(
                context.Graph,
                selectedEdgeIndex,
                requestedWidth,
                minimumStableEdgeLength,
                out _);
            if (float.IsNaN(initialWidth) ||
                float.IsInfinity(initialWidth) ||
                initialWidth < minimumWidth - PointMergeDistance)
            {
                blocker = "the selected edge has no stable isolated starting width";
                return false;
            }

            float attemptWidth = Mathf.Max(minimumWidth, initialWidth);
            string lastBlocker = string.Empty;
            for (int attemptIndex = 0;
                 attemptIndex < boundedMaximumWidthAttempts;
                 attemptIndex++)
            {
                widthAttemptCount++;
                solvedWidth = attemptWidth;
                EdgeWearIsolatedWidthAttemptRecord attempt =
                    new EdgeWearIsolatedWidthAttemptRecord
                    {
                        AttemptIndex = widthAttemptCount,
                        Width = attemptWidth
                    };
                bool railSolved = TrySolveBoundedIsolatedRailsAtWidth(
                    sourceFaces,
                    context,
                    selected,
                    requestedWidth,
                    attemptWidth,
                    minimumStableEdgeLength,
                    out BoundedIsolatedRailPoint[] candidateRails,
                    out lastBlocker);
                attempt.RailSolved = railSolved;
                attempt.RailFailure = railSolved
                    ? string.Empty
                    : lastBlocker ?? string.Empty;
                widthAttempts.Add(attempt);
                if (railSolved)
                {
                    rails = candidateRails;
                    scheduleComplete =
                        attemptWidth <=
                            minimumWidth + PointMergeDistance ||
                        widthAttemptCount >= boundedMaximumWidthAttempts;
                    return true;
                }

                if (attemptWidth <= minimumWidth + PointMergeDistance)
                {
                    scheduleComplete = true;
                    break;
                }

                float nextWidth = Mathf.Max(
                    minimumWidth,
                    attemptWidth * BoundedIsolatedWidthBackoff);
                if (nextWidth >= attemptWidth - PointMergeDistance)
                {
                    break;
                }
                attemptWidth = nextWidth;
            }

            if (widthAttemptCount >= boundedMaximumWidthAttempts)
            {
                scheduleComplete = true;
            }
            blocker = string.IsNullOrEmpty(lastBlocker)
                ? "the selected edge has no stable isolated rail solution"
                : lastBlocker;
            return false;
        }

        private static bool TrySolveBoundedIsolatedRailsAtWidth(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            EdgeWearSelectedGraphEdge selected,
            float requestedWidth,
            float isolatedWidth,
            float minimumStableEdgeLength,
            out BoundedIsolatedRailPoint[] rails,
            out string blocker)
        {
            rails = new BoundedIsolatedRailPoint[4];
            blocker = string.Empty;

            int selectedEdgeIndex = selected.GraphEdgeIndex;
            EdgeWearGraphEdge selectedEdge =
                context.Graph.Edges[selectedEdgeIndex];
            if (!TrySolveBoundedIsolatedRailPoint(
                    sourceFaces,
                    context,
                    selectedEdgeIndex,
                    selectedEdge.FaceA,
                    selectedEdge.VertexA,
                    0,
                    requestedWidth,
                    isolatedWidth,
                    minimumStableEdgeLength,
                    out rails[0],
                    out blocker) ||
                !TrySolveBoundedIsolatedRailPoint(
                    sourceFaces,
                    context,
                    selectedEdgeIndex,
                    selectedEdge.FaceA,
                    selectedEdge.VertexB,
                    1,
                    requestedWidth,
                    isolatedWidth,
                    minimumStableEdgeLength,
                    out rails[1],
                    out blocker) ||
                !TrySolveBoundedIsolatedRailPoint(
                    sourceFaces,
                    context,
                    selectedEdgeIndex,
                    selectedEdge.FaceB,
                    selectedEdge.VertexA,
                    2,
                    requestedWidth,
                    isolatedWidth,
                    minimumStableEdgeLength,
                    out rails[2],
                    out blocker) ||
                !TrySolveBoundedIsolatedRailPoint(
                    sourceFaces,
                    context,
                    selectedEdgeIndex,
                    selectedEdge.FaceB,
                    selectedEdge.VertexB,
                    3,
                    requestedWidth,
                    isolatedWidth,
                    minimumStableEdgeLength,
                    out rails[3],
                    out blocker))
            {
                rails = null;
                return false;
            }

            HashSet<int> targetEdges = new HashSet<int>();
            for (int railIndex = 0; railIndex < rails.Length; railIndex++)
            {
                targetEdges.Add(rails[railIndex].AdjacentGraphEdgeIndex);
            }
            if (targetEdges.Count != rails.Length)
            {
                blocker =
                    "the isolated rail solution does not own four distinct adjacent source edges";
                rails = null;
                return false;
            }

            float minimumLengthSqr = Mathf.Max(
                MinimumEdgeLengthSqr,
                minimumStableEdgeLength * minimumStableEdgeLength *
                    0.0001f);
            if ((rails[1].Position - rails[0].Position).sqrMagnitude <=
                    minimumLengthSqr ||
                (rails[3].Position - rails[2].Position).sqrMagnitude <=
                    minimumLengthSqr ||
                (rails[2].Position - rails[0].Position).sqrMagnitude <=
                    minimumLengthSqr ||
                (rails[3].Position - rails[1].Position).sqrMagnitude <=
                    minimumLengthSqr)
            {
                blocker = "the isolated rail solution collapses a rail or endpoint support boundary";
                rails = null;
                return false;
            }
            return true;
        }

        private static bool TrySolveBoundedIsolatedRailPoint(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            int selectedEdgeIndex,
            int ownerGraphFaceIndex,
            int sourceVertexIndex,
            int railIndex,
            float requestedWidth,
            float isolatedWidth,
            float minimumStableEdgeLength,
            out BoundedIsolatedRailPoint rail,
            out string blocker)
        {
            rail = default;
            blocker = string.Empty;
            if (ownerGraphFaceIndex < 0 ||
                ownerGraphFaceIndex >= context.Graph.Faces.Count)
            {
                blocker = "the isolated rail has invalid owner-face provenance";
                return false;
            }

            EdgeWearGraphFace ownerGraphFace =
                context.Graph.Faces[ownerGraphFaceIndex];
            int ownerSourceFaceIndex = ownerGraphFace.SourceFaceIndex;
            if (ownerSourceFaceIndex < 0 ||
                ownerSourceFaceIndex >= sourceFaces.Count)
            {
                blocker = "the isolated rail owner source face is unavailable";
                return false;
            }

            int localVertexIndex =
                ownerGraphFace.VertexIndices.IndexOf(sourceVertexIndex);
            if (localVertexIndex < 0)
            {
                blocker = "the isolated rail endpoint is absent from its owner face";
                return false;
            }

            int vertexCount = ownerGraphFace.VertexIndices.Count;
            int previousEdgeIndex = ownerGraphFace.EdgeIndices[
                (localVertexIndex + vertexCount - 1) % vertexCount];
            int nextEdgeIndex =
                ownerGraphFace.EdgeIndices[localVertexIndex];
            bool previousIsSelected =
                previousEdgeIndex == selectedEdgeIndex;
            bool nextIsSelected = nextEdgeIndex == selectedEdgeIndex;
            if (previousIsSelected == nextIsSelected)
            {
                blocker =
                    "the selected edge does not own exactly one side of an isolated owner corner";
                return false;
            }

            int originalAdjacentEdgeIndex = previousIsSelected
                ? nextEdgeIndex
                : previousEdgeIndex;
            PolygonFace ownerSourceFace = sourceFaces[ownerSourceFaceIndex];
            if (ownerSourceFace == null ||
                ownerSourceFace.Vertices == null ||
                ownerSourceFace.Vertices.Count != vertexCount)
            {
                blocker =
                    "the isolated rail owner boundary does not match its topology face";
                return false;
            }

            Vector3 faceCentre = CalculateAverage(ownerSourceFace.Vertices);
            if (!TryBuildChamferFaceLine(
                    context.Graph,
                    previousEdgeIndex,
                    ownerSourceFace.Normal,
                    faceCentre,
                    previousIsSelected ? isolatedWidth : 0f,
                    out ChamferFaceLine previousLine) ||
                !TryBuildChamferFaceLine(
                    context.Graph,
                    nextEdgeIndex,
                    ownerSourceFace.Normal,
                    faceCentre,
                    nextIsSelected ? isolatedWidth : 0f,
                    out ChamferFaceLine nextLine))
            {
                blocker = "the isolated rail could not build stable owner-face support lines";
                return false;
            }

            Vector3 sourceVertex =
                context.Graph.Vertices[sourceVertexIndex].Position;
            if (!TrySolveChamferFaceCorner(
                    sourceVertex,
                    previousLine,
                    nextLine,
                    ownerSourceFace.Normal,
                    minimumStableEdgeLength * 0.001f,
                    out Vector3 solved) ||
                !IsFinite(solved))
            {
                blocker = "the isolated rail corner has parallel or unstable support lines";
                return false;
            }

            float pointTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                minimumStableEdgeLength * 0.002f);
            MeasureBoundedPointAgainstGraphEdge(
                context.Graph,
                originalAdjacentEdgeIndex,
                solved,
                out float originalRawParameter,
                out float originalSegmentDistance);

            ChamferFaceLine selectedLine = previousIsSelected
                ? previousLine
                : nextLine;
            EdgeWearGraphEdge selectedEdge =
                context.Graph.Edges[selectedEdgeIndex];
            Vector3 rayDirection = selectedLine.Direction;
            if (sourceVertexIndex == selectedEdge.VertexB)
            {
                rayDirection = -rayDirection;
            }
            Vector3 rayOrigin = selectedLine.Point +
                selectedLine.Direction * Vector3.Dot(
                    sourceVertex - selectedLine.Point,
                    selectedLine.Direction);
            if (Vector3.Dot(solved - rayOrigin, rayDirection) < 0f)
            {
                rayDirection = -rayDirection;
            }

            if (!TryResolveBoundedOwnerBoundaryHit(
                    sourceFaces,
                    context,
                    ownerGraphFaceIndex,
                    selectedEdgeIndex,
                    originalAdjacentEdgeIndex,
                    rayOrigin,
                    rayDirection,
                    pointTolerance,
                    out BoundedOwnerBoundaryHit boundaryHit,
                    out int boundaryCandidateCount,
                    out blocker))
            {
                blocker = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} (rail={1},originalAdjacentEdge={2}," +
                    "rawParameter={3:G9},segmentDistance={4:G9}," +
                    "pointTolerance={5:G9})",
                    string.IsNullOrEmpty(blocker)
                        ? "the isolated rail has no unique forward owner-boundary terminal"
                        : blocker,
                    railIndex,
                    originalAdjacentEdgeIndex,
                    originalRawParameter,
                    originalSegmentDistance,
                    pointTolerance);
                return false;
            }

            Vector3 canonical = boundaryHit.Position;
            PolygonFace targetSourceFace =
                sourceFaces[boundaryHit.TargetSourceFaceIndex];
            Vector3 ownerNormal = ownerSourceFace.Normal;
            Vector3 targetNormal = targetSourceFace.Normal;
            if (!IsFinite(ownerNormal) || !IsFinite(targetNormal) ||
                ownerNormal.sqrMagnitude <= MinimumEdgeLengthSqr ||
                targetNormal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                blocker =
                    "the isolated rail boundary has an invalid analytical face plane";
                return false;
            }
            ownerNormal.Normalize();
            targetNormal.Normalize();
            float ownerPlaneDistance = Vector3.Dot(
                ownerNormal,
                ownerSourceFace.Vertices[0]);
            float targetPlaneDistance = Vector3.Dot(
                targetNormal,
                targetSourceFace.Vertices[0]);
            if (Mathf.Abs(Vector3.Dot(ownerNormal, canonical) -
                    ownerPlaneDistance) > pointTolerance ||
                Mathf.Abs(Vector3.Dot(targetNormal, canonical) -
                    targetPlaneDistance) > pointTolerance)
            {
                blocker =
                    "the resolved isolated rail terminal leaves one of its exact boundary planes";
                return false;
            }

            EdgeWearGraphEdge boundaryEdge =
                context.Graph.Edges[boundaryHit.GraphEdgeIndex];
            float boundaryLength = Vector3.Distance(
                context.Graph.Vertices[boundaryEdge.VertexA].Position,
                context.Graph.Vertices[boundaryEdge.VertexB].Position);
            float selectedLength = GetGraphEdgeLength(
                context.Graph,
                selectedEdgeIndex);
            float displacementLimit =
                CalculateChamferCornerDisplacementLimit(
                    requestedWidth,
                    minimumStableEdgeLength,
                    selectedLength,
                    boundaryLength);
            if ((canonical - sourceVertex).magnitude >
                displacementLimit + pointTolerance)
            {
                blocker =
                    "the isolated rail endpoint exceeds the conservative local displacement limit";
                return false;
            }

            rail = new BoundedIsolatedRailPoint(
                canonical,
                railIndex,
                ownerGraphFaceIndex,
                ownerSourceFaceIndex,
                sourceVertexIndex,
                boundaryHit.GraphEdgeIndex,
                originalAdjacentEdgeIndex,
                boundaryHit.TargetGraphFaceIndex,
                boundaryHit.TargetSourceFaceIndex,
                boundaryCandidateCount,
                boundaryHit.GraphEdgeIndex != originalAdjacentEdgeIndex,
                boundaryHit.SnapDistance,
                originalRawParameter,
                originalSegmentDistance,
                boundaryHit.SegmentParameter,
                pointTolerance,
                boundaryHit.NearestEndpointDistance);
            return true;
        }

        private static void MeasureBoundedPointAgainstGraphEdge(
            EdgeWearTopologyGraph graph,
            int graphEdgeIndex,
            Vector3 point,
            out float rawParameter,
            out float segmentDistance)
        {
            rawParameter = 0f;
            segmentDistance = float.PositiveInfinity;
            if (graphEdgeIndex < 0 ||
                graphEdgeIndex >= graph.Edges.Count ||
                !IsFinite(point))
            {
                return;
            }

            EdgeWearGraphEdge edge = graph.Edges[graphEdgeIndex];
            Vector3 start = graph.Vertices[edge.VertexA].Position;
            Vector3 end = graph.Vertices[edge.VertexB].Position;
            Vector3 segment = end - start;
            double x = segment.x;
            double y = segment.y;
            double z = segment.z;
            double lengthSqr = x * x + y * y + z * z;
            if (!(lengthSqr > 0.0) ||
                double.IsNaN(lengthSqr) ||
                double.IsInfinity(lengthSqr))
            {
                return;
            }

            Vector3 delta = point - start;
            double parameter =
                (delta.x * x + delta.y * y + delta.z * z) /
                lengthSqr;
            if (double.IsNaN(parameter) ||
                double.IsInfinity(parameter))
            {
                return;
            }

            rawParameter = (float)parameter;
            Vector3 closest = start + segment * Mathf.Clamp01(rawParameter);
            segmentDistance = (point - closest).magnitude;
        }

        private static bool TryResolveBoundedOwnerBoundaryHit(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            int ownerGraphFaceIndex,
            int selectedEdgeIndex,
            int originalAdjacentEdgeIndex,
            Vector3 rayOrigin,
            Vector3 rayDirection,
            float pointTolerance,
            out BoundedOwnerBoundaryHit resolved,
            out int candidateCount,
            out string blocker)
        {
            resolved = default;
            candidateCount = 0;
            blocker = string.Empty;
            if (!IsFinite(rayOrigin) || !IsFinite(rayDirection) ||
                rayDirection.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                blocker = "the isolated rail owner-boundary ray is invalid";
                return false;
            }

            EdgeWearGraphFace ownerGraphFace =
                context.Graph.Faces[ownerGraphFaceIndex];
            PolygonFace ownerSourceFace =
                sourceFaces[ownerGraphFace.SourceFaceIndex];
            Vector3 faceNormal = ownerSourceFace.Normal;
            if (!IsFinite(faceNormal) ||
                faceNormal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                blocker =
                    "the isolated rail owner face has an invalid normal";
                return false;
            }
            faceNormal.Normalize();
            rayDirection.Normalize();
            Vector3 lateral = Vector3.Cross(faceNormal, rayDirection);
            if (!IsFinite(lateral) ||
                lateral.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                blocker =
                    "the isolated rail owner-boundary basis is unstable";
                return false;
            }
            lateral.Normalize();

            List<BoundedOwnerBoundaryHit> hits =
                new List<BoundedOwnerBoundaryHit>();
            int edgeCount = ownerGraphFace.EdgeIndices.Count;
            for (int localEdgeIndex = 0;
                 localEdgeIndex < edgeCount;
                 localEdgeIndex++)
            {
                int graphEdgeIndex =
                    ownerGraphFace.EdgeIndices[localEdgeIndex];
                if (graphEdgeIndex == selectedEdgeIndex)
                {
                    continue;
                }

                EdgeWearGraphEdge graphEdge =
                    context.Graph.Edges[graphEdgeIndex];
                int targetGraphFaceIndex = graphEdge.FaceA ==
                    ownerGraphFaceIndex
                    ? graphEdge.FaceB
                    : graphEdge.FaceB == ownerGraphFaceIndex
                        ? graphEdge.FaceA
                        : -1;
                if (targetGraphFaceIndex < 0 ||
                    targetGraphFaceIndex >= context.Graph.Faces.Count ||
                    graphEdge.ExtraFaceCount > 0)
                {
                    continue;
                }
                int targetSourceFaceIndex = context.Graph.Faces[
                    targetGraphFaceIndex].SourceFaceIndex;
                if (targetSourceFaceIndex < 0 ||
                    targetSourceFaceIndex >= sourceFaces.Count ||
                    targetSourceFaceIndex == ownerGraphFace.SourceFaceIndex)
                {
                    continue;
                }

                Vector3 segmentStart =
                    ownerSourceFace.Vertices[localEdgeIndex];
                Vector3 segmentEnd = ownerSourceFace.Vertices[
                    (localEdgeIndex + 1) % edgeCount];
                Vector3 segment = segmentEnd - segmentStart;
                float segmentLength = segment.magnitude;
                if (segmentLength <= PointMergeDistance ||
                    !IsFinite(segment))
                {
                    continue;
                }

                Vector3 startDelta = segmentStart - rayOrigin;
                Vector3 endDelta = segmentEnd - rayOrigin;
                double x0 = Vector3.Dot(startDelta, rayDirection);
                double y0 = Vector3.Dot(startDelta, lateral);
                double x1 = Vector3.Dot(endDelta, rayDirection);
                double y1 = Vector3.Dot(endDelta, lateral);
                double denominator = y1 - y0;
                double lateralTolerance = pointTolerance;
                if (Math.Abs(denominator) <=
                    Math.Max(1e-12, lateralTolerance * 1e-4))
                {
                    continue;
                }

                double segmentParameter = -y0 / denominator;
                double parameterTolerance = pointTolerance / segmentLength;
                if (segmentParameter < -parameterTolerance ||
                    segmentParameter > 1.0 + parameterTolerance)
                {
                    continue;
                }

                double rayDistance = x0 +
                    (x1 - x0) * segmentParameter;
                if (rayDistance < -pointTolerance)
                {
                    continue;
                }

                float canonicalParameter = Mathf.Clamp01(
                    (float)segmentParameter);
                Vector3 segmentPoint = segmentStart +
                    segment * canonicalParameter;
                Vector3 rayPoint = rayOrigin +
                    rayDirection * Mathf.Max(0f, (float)rayDistance);
                float snapDistance = (segmentPoint - rayPoint).magnitude;
                if (snapDistance > pointTolerance)
                {
                    continue;
                }

                float nearestEndpointDistance = Mathf.Min(
                    (segmentPoint - segmentStart).magnitude,
                    (segmentPoint - segmentEnd).magnitude);
                hits.Add(new BoundedOwnerBoundaryHit(
                    segmentPoint,
                    localEdgeIndex,
                    graphEdgeIndex,
                    targetGraphFaceIndex,
                    targetSourceFaceIndex,
                    Mathf.Max(0f, (float)rayDistance),
                    canonicalParameter,
                    snapDistance,
                    nearestEndpointDistance));
            }

            if (hits.Count == 0)
            {
                blocker =
                    "the isolated rail support line does not reach a manifold owner-face boundary";
                return false;
            }

            hits.Sort((left, right) =>
            {
                int distanceOrder = left.RayDistance.CompareTo(
                    right.RayDistance);
                if (distanceOrder != 0)
                {
                    return distanceOrder;
                }
                bool leftOriginal =
                    left.GraphEdgeIndex == originalAdjacentEdgeIndex;
                bool rightOriginal =
                    right.GraphEdgeIndex == originalAdjacentEdgeIndex;
                if (leftOriginal != rightOriginal)
                {
                    return leftOriginal ? -1 : 1;
                }
                float leftInterior = Mathf.Min(
                    left.SegmentParameter,
                    1f - left.SegmentParameter);
                float rightInterior = Mathf.Min(
                    right.SegmentParameter,
                    1f - right.SegmentParameter);
                int interiorOrder = rightInterior.CompareTo(leftInterior);
                return interiorOrder != 0
                    ? interiorOrder
                    : left.GraphEdgeIndex.CompareTo(right.GraphEdgeIndex);
            });

            BoundedOwnerBoundaryHit nearest = hits[0];
            List<BoundedOwnerBoundaryHit> distinct =
                new List<BoundedOwnerBoundaryHit>();
            for (int hitIndex = 0; hitIndex < hits.Count; hitIndex++)
            {
                BoundedOwnerBoundaryHit hit = hits[hitIndex];
                bool duplicate = false;
                for (int distinctIndex = 0;
                     distinctIndex < distinct.Count;
                     distinctIndex++)
                {
                    if ((hit.Position - distinct[distinctIndex].Position)
                            .magnitude <= pointTolerance)
                    {
                        duplicate = true;
                        break;
                    }
                }
                if (!duplicate)
                {
                    distinct.Add(hit);
                }
            }
            candidateCount = distinct.Count;

            for (int hitIndex = 1; hitIndex < distinct.Count; hitIndex++)
            {
                if (Mathf.Abs(
                        distinct[hitIndex].RayDistance -
                        nearest.RayDistance) <= pointTolerance &&
                    (distinct[hitIndex].Position - nearest.Position)
                        .magnitude > pointTolerance)
                {
                    blocker =
                        "the isolated rail has multiple distinct nearest forward owner-boundary terminals";
                    return false;
                }
            }

            resolved = nearest;
            return true;
        }



        private static bool TryBuildBoundedSingleEdgeFaces(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            EdgeWearSelectedGraphEdge selected,
            BoundedIsolatedRailPoint[] isolatedRails,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            ref BoundedSingleEdgeAuditResult audit,
            out List<PolygonFace> boundedFaces,
            out int boundarySubdivisionCount,
            out string blocker)
        {
            boundedFaces = null;
            boundarySubdivisionCount = 0;
            blocker = string.Empty;

            if (isolatedRails == null || isolatedRails.Length != 4)
            {
                blocker = "the bounded edge does not contain four isolated rail points";
                return false;
            }

            Vector3 a0 = isolatedRails[0].Position;
            Vector3 b0 = isolatedRails[1].Position;
            Vector3 a1 = isolatedRails[2].Position;
            Vector3 b1 = isolatedRails[3].Position;

            int edgeIndex = selected.GraphEdgeIndex;
            EdgeWearGraphEdge edge = context.Graph.Edges[edgeIndex];
            EdgeWearGraphFace graphFaceA = context.Graph.Faces[edge.FaceA];
            EdgeWearGraphFace graphFaceB = context.Graph.Faces[edge.FaceB];
            int sourceFaceA = graphFaceA.SourceFaceIndex;
            int sourceFaceB = graphFaceB.SourceFaceIndex;
            if (sourceFaceA < 0 || sourceFaceA >= sourceFaces.Count ||
                sourceFaceB < 0 || sourceFaceB >= sourceFaces.Count ||
                sourceFaceA == sourceFaceB)
            {
                blocker = "the selected edge has invalid source-face ownership";
                return false;
            }

            Vector3 bevelNormal = selected.Candidate.BevelNormal;
            if (!IsFinite(bevelNormal) ||
                bevelNormal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                blocker = "the selected edge has an invalid bounded bevel normal";
                return false;
            }
            bevelNormal.Normalize();
            float railPlaneDistance =
                (Vector3.Dot(bevelNormal, a0) +
                 Vector3.Dot(bevelNormal, b0) +
                 Vector3.Dot(bevelNormal, a1) +
                 Vector3.Dot(bevelNormal, b1)) * 0.25f;
            float railPlaneTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                minimumStableEdgeLength * 0.02f);
            if (Mathf.Abs(Vector3.Dot(bevelNormal, a0) -
                    railPlaneDistance) > railPlaneTolerance ||
                Mathf.Abs(Vector3.Dot(bevelNormal, b0) -
                    railPlaneDistance) > railPlaneTolerance ||
                Mathf.Abs(Vector3.Dot(bevelNormal, a1) -
                    railPlaneDistance) > railPlaneTolerance ||
                Mathf.Abs(Vector3.Dot(bevelNormal, b1) -
                    railPlaneDistance) > railPlaneTolerance)
            {
                blocker = "the four solved rails do not define one bounded bevel plane";
                return false;
            }

            Vector3 sourceA =
                context.Graph.Vertices[edge.VertexA].Position;
            Vector3 sourceB =
                context.Graph.Vertices[edge.VertexB].Position;
            float minimumLengthSqr = Mathf.Max(
                MinimumEdgeLengthSqr,
                minimumStableEdgeLength * minimumStableEdgeLength *
                    0.0001f);
            if ((sourceB - sourceA).sqrMagnitude <= minimumLengthSqr ||
                (b0 - a0).sqrMagnitude <= minimumLengthSqr ||
                (b1 - a1).sqrMagnitude <= minimumLengthSqr ||
                (a1 - a0).sqrMagnitude <= minimumLengthSqr ||
                (b1 - b0).sqrMagnitude <= minimumLengthSqr)
            {
                blocker = "the selected edge has a collapsed bounded rail";
                return false;
            }

            Vector3 solidCentre = CalculatePlaneCutFaceVertexCentre(
                sourceFaces);
            bool requiresMultiSupportPlaneCut =
                isolatedRails[0].TargetSourceFaceIndex !=
                    isolatedRails[2].TargetSourceFaceIndex ||
                isolatedRails[1].TargetSourceFaceIndex !=
                    isolatedRails[3].TargetSourceFaceIndex;
            if (requiresMultiSupportPlaneCut)
            {
                return TryBuildBoundedMultiSupportPlaneCutFaces(
                    sourceFaces,
                    context,
                    selected,
                    isolatedRails,
                    bevelNormal,
                    railPlaneDistance,
                    solidCentre,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    ref audit,
                    out boundedFaces,
                    out boundarySubdivisionCount,
                    out blocker);
            }

            boundedFaces = ClonePolygonFacesForPlaneCutAudit(
                sourceFaces,
                assignSourceFaceProvenance: true);
            if (!TryClipBoundedEndpointSupportFaces(
                    sourceFaces,
                    boundedFaces,
                    context,
                    sourceFaceA,
                    sourceFaceB,
                    edge.VertexA,
                    edge.VertexB,
                    isolatedRails,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    ref audit,
                    out boundarySubdivisionCount,
                    out int targetBoundaryCount,
                    out blocker))
            {
                boundedFaces = null;
                return false;
            }
            audit.TargetBoundaryCount = targetBoundaryCount;
            audit.EndpointSupportModifiedFaceExpectedCount = 2;

            if (!TryClipBoundedOwnerSourceFace(
                    sourceFaces[sourceFaceA],
                    graphFaceA,
                    edge.VertexA,
                    edge.VertexB,
                    a0,
                    b0,
                    sourceFaceA,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    ref audit,
                    out PolygonFace replacementA,
                    out blocker) ||
                !TryClipBoundedOwnerSourceFace(
                    sourceFaces[sourceFaceB],
                    graphFaceB,
                    edge.VertexA,
                    edge.VertexB,
                    a1,
                    b1,
                    sourceFaceB,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    ref audit,
                    out PolygonFace replacementB,
                    out blocker))
            {
                boundedFaces = null;
                return false;
            }

            boundedFaces[sourceFaceA] = replacementA;
            boundedFaces[sourceFaceB] = replacementB;

            if (!TryCreateBoundedFace(
                    new List<Vector3> { a0, b0, b1, a1 },
                    bevelNormal,
                    solidCentre,
                    PolygonFaceProvenanceKind.BoundedEdgeBevel,
                    edgeIndex,
                    selected.Candidate.Strength,
                    minimumStableFaceArea,
                    out PolygonFace bevelFace))
            {
                blocker = "the selected edge produces a collapsed bounded bevel face";
                boundedFaces = null;
                return false;
            }

            boundedFaces.Add(bevelFace);
            return true;
        }

        private static bool TryBuildBoundedMultiSupportPlaneCutFaces(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            EdgeWearSelectedGraphEdge selected,
            BoundedIsolatedRailPoint[] isolatedRails,
            Vector3 bevelNormal,
            float railPlaneDistance,
            Vector3 solidCentre,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            ref BoundedSingleEdgeAuditResult audit,
            out List<PolygonFace> boundedFaces,
            out int boundarySubdivisionCount,
            out string blocker)
        {
            audit.MultiSupportSinglePlaneAttempted = 1;
            BoundedSingleEdgeAuditResult singlePlaneAudit = audit;
            if (TryBuildBoundedMultiSupportSinglePlaneCutFaces(
                    sourceFaces,
                    context,
                    selected,
                    isolatedRails,
                    bevelNormal,
                    railPlaneDistance,
                    solidCentre,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    ref singlePlaneAudit,
                    out boundedFaces,
                    out boundarySubdivisionCount,
                    out blocker))
            {
                singlePlaneAudit.MultiSupportSinglePlaneSucceeded = 1;
                singlePlaneAudit.MultiSupportSinglePlaneFailure =
                    string.Empty;
                singlePlaneAudit.MultiSupportPlaneCount = 1;
                audit = singlePlaneAudit;
                return true;
            }

            string singlePlaneBlocker = blocker ?? string.Empty;
            audit.MultiSupportSinglePlaneFailure = singlePlaneBlocker;
            audit.MultiSupportRetainedHullAttempted = 1;
            BoundedSingleEdgeAuditResult retainedHullAudit = audit;
            if (TryBuildBoundedRetainedPointHullFaces(
                    sourceFaces,
                    context,
                    selected,
                    isolatedRails,
                    bevelNormal,
                    railPlaneDistance,
                    solidCentre,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    ref retainedHullAudit,
                    out boundedFaces,
                    out boundarySubdivisionCount,
                    out blocker))
            {
                retainedHullAudit.MultiSupportRetainedHullSucceeded = 1;
                retainedHullAudit.MultiSupportRetainedHullFailure =
                    string.Empty;
                retainedHullAudit.MultiSupportHullEvidence =
                    "singlePlaneFailure:{" + singlePlaneBlocker + "};" +
                    (retainedHullAudit.MultiSupportHullEvidence ??
                        string.Empty);
                audit = retainedHullAudit;
                return true;
            }

            retainedHullAudit.MultiSupportRetainedHullFailure =
                blocker ?? string.Empty;
            retainedHullAudit.MultiSupportHullEvidence =
                "singlePlaneFailure:{" + singlePlaneBlocker + "};" +
                (retainedHullAudit.MultiSupportHullEvidence ??
                    string.Empty);
            audit = retainedHullAudit;
            blocker =
                "single-plane multi-support construction failed: " +
                singlePlaneBlocker +
                "; retained-point hull recovery failed: " +
                (blocker ?? string.Empty);
            boundedFaces = null;
            boundarySubdivisionCount = 0;
            return false;
        }

        private static bool TryBuildBoundedMultiSupportSinglePlaneCutFaces(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            EdgeWearSelectedGraphEdge selected,
            BoundedIsolatedRailPoint[] isolatedRails,
            Vector3 bevelNormal,
            float railPlaneDistance,
            Vector3 solidCentre,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            ref BoundedSingleEdgeAuditResult audit,
            out List<PolygonFace> boundedFaces,
            out int boundarySubdivisionCount,
            out string blocker)
        {
            boundedFaces = null;
            boundarySubdivisionCount = 0;
            blocker = string.Empty;
            if (sourceFaces == null || context == null ||
                isolatedRails == null || isolatedRails.Length != 4 ||
                selected.GraphEdgeIndex < 0 ||
                selected.GraphEdgeIndex >= context.Graph.Edges.Count)
            {
                blocker =
                    "the multi-support bounded plane cut lacks exact source geometry";
                return false;
            }

            EdgeWearGraphEdge sourceEdge =
                context.Graph.Edges[selected.GraphEdgeIndex];
            Vector3 sourceA =
                context.Graph.Vertices[sourceEdge.VertexA].Position;
            Vector3 sourceB =
                context.Graph.Vertices[sourceEdge.VertexB].Position;
            CutPlane plane = new CutPlane(bevelNormal, railPlaneDistance);
            if (plane.SignedDistance(solidCentre) > 0f)
            {
                plane = new CutPlane(-bevelNormal, -railPlaneDistance);
            }

            float pointTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                minimumStableEdgeLength * 0.002f);
            float minimumRemoval = Mathf.Max(
                PointMergeDistance * 0.25f,
                minimumStableEdgeLength * 0.0001f);
            float sourceRemovalA = plane.SignedDistance(sourceA);
            float sourceRemovalB = plane.SignedDistance(sourceB);
            if (sourceRemovalA <= minimumRemoval ||
                sourceRemovalB <= minimumRemoval ||
                plane.SignedDistance(solidCentre) >= -pointTolerance)
            {
                blocker =
                    "the multi-support bounded plane does not remove only the selected source edge side";
                return false;
            }
            for (int vertexIndex = 0;
                 vertexIndex < context.Graph.Vertices.Count;
                 vertexIndex++)
            {
                if (vertexIndex == sourceEdge.VertexA ||
                    vertexIndex == sourceEdge.VertexB)
                {
                    continue;
                }
                if (plane.SignedDistance(
                        context.Graph.Vertices[vertexIndex].Position) >
                    pointTolerance)
                {
                    blocker =
                        "the multi-support bounded plane would remove a foreign source vertex";
                    return false;
                }
            }

            boundedFaces = ClonePolygonFacesForPlaneCutAudit(
                sourceFaces,
                assignSourceFaceProvenance: true);
            float clipEpsilon = Mathf.Min(
                PlaneEpsilon,
                Mathf.Max(
                    PointMergeDistance * 0.25f,
                    Mathf.Min(sourceRemovalA, sourceRemovalB) * 0.25f));
            PlaneCutNumericalRepairTelemetry numericalRepairs =
                new PlaneCutNumericalRepairTelemetry();
            ClipPolyhedron(
                boundedFaces,
                plane,
                PolygonFaceFeature.ConvexEdgeWear,
                selected.Candidate.Strength,
                true,
                clipEpsilon,
                true,
                PolygonFaceProvenanceKind.BoundedEdgeBevel,
                selected.GraphEdgeIndex,
                true,
                true,
                numericalRepairs);
            if (numericalRepairs.ExactConstructionFailureCount > 0)
            {
                blocker =
                    "the exact multi-support bounded plane cut failed strict intersection construction";
                boundedFaces = null;
                return false;
            }

            int capFaceIndex = -1;
            int capFaceCount = 0;
            for (int faceIndex = 0;
                 faceIndex < boundedFaces.Count;
                 faceIndex++)
            {
                PolygonFace face = boundedFaces[faceIndex];
                if (face.ProvenanceKind ==
                        PolygonFaceProvenanceKind.BoundedEdgeBevel &&
                    face.ProvenanceIndex == selected.GraphEdgeIndex)
                {
                    capFaceCount++;
                    capFaceIndex = faceIndex;
                }
            }
            if (capFaceCount != 1 || capFaceIndex < 0)
            {
                blocker =
                    "the multi-support bounded plane cut did not emit one unique bevel cap";
                boundedFaces = null;
                return false;
            }

            PolygonFace capFace = boundedFaces[capFaceIndex];
            List<Vector3> capVertices =
                CopyBoundedPolygonPreservingCollinear(capFace.Vertices);
            for (int railIndex = 0;
                 railIndex < isolatedRails.Length;
                 railIndex++)
            {
                if (!TryInsertBoundedPointIntoPolygonBoundary(
                        capVertices,
                        isolatedRails[railIndex].Position,
                        pointTolerance,
                        out _))
                {
                    blocker =
                        "a solved multi-support rail is absent from the exact bevel-cap boundary";
                    boundedFaces = null;
                    return false;
                }
            }
            if (!ValidateBoundedPolygon(
                    capVertices,
                    capFace.Normal,
                    minimumStableFaceArea,
                    requireConvex: true,
                    out string capBlocker,
                    out _))
            {
                blocker = "multi-support bevel cap failed: " + capBlocker;
                boundedFaces = null;
                return false;
            }
            boundedFaces[capFaceIndex] = new PolygonFace(
                capVertices,
                capFace.Normal,
                capFace.Feature,
                capFace.FeatureStrength,
                capFace.ProvenanceKind,
                capFace.ProvenanceIndex);

            int ownerSourceFaceA =
                context.Graph.Faces[sourceEdge.FaceA].SourceFaceIndex;
            int ownerSourceFaceB =
                context.Graph.Faces[sourceEdge.FaceB].SourceFaceIndex;
            int ownerModifiedCount = 0;
            int supportModifiedCount = 0;
            for (int sourceFaceIndex = 0;
                 sourceFaceIndex < sourceFaces.Count;
                 sourceFaceIndex++)
            {
                PolygonFace resultFace = FindBoundedSourceFace(
                    boundedFaces,
                    sourceFaceIndex);
                if (resultFace == null)
                {
                    blocker =
                        "the multi-support bounded plane cut consumed a complete source face";
                    boundedFaces = null;
                    return false;
                }
                bool equivalent =
                    AreBoundedPolygonsGeometricallyEquivalent(
                        sourceFaces[sourceFaceIndex].Vertices,
                        resultFace.Vertices,
                        sourceFaces[sourceFaceIndex].Normal,
                        out bool boundaryOnlyDifference);
                if (equivalent && !boundaryOnlyDifference)
                {
                    continue;
                }
                if (sourceFaceIndex == ownerSourceFaceA ||
                    sourceFaceIndex == ownerSourceFaceB)
                {
                    ownerModifiedCount++;
                }
                else
                {
                    supportModifiedCount++;
                }
            }
            if (ownerModifiedCount != 2 || supportModifiedCount < 2)
            {
                blocker =
                    "the multi-support bounded plane cut did not modify both owners and its exact endpoint-support interval";
                boundedFaces = null;
                return false;
            }

            audit.MultiSupportPlaneCut = 1;
            audit.MultiSupportPlaneNormal = plane.Normal;
            audit.MultiSupportPlaneDistance = plane.Distance;
            audit.OwnerClipAttemptedCount = 2;
            audit.OwnerClipCount = 2;
            audit.EndpointSupportClipAttemptedCount = 2;
            audit.EndpointSupportClipCount = 2;
            audit.EndpointSupportRemovedVertexCount = 2;
            audit.EndpointSupportRailInsertionCount = 4;
            audit.EndpointSupportModifiedFaceExpectedCount =
                supportModifiedCount;
            audit.EndpointSupportBoundaryPathVertexCount =
                capVertices.Count;
            audit.EndpointSupportFaceA =
                isolatedRails[0].TargetSourceFaceIndex;
            audit.EndpointSupportAdditionalFaceA =
                isolatedRails[2].TargetSourceFaceIndex ==
                    audit.EndpointSupportFaceA
                    ? -1
                    : isolatedRails[2].TargetSourceFaceIndex;
            audit.EndpointSupportFaceB =
                isolatedRails[1].TargetSourceFaceIndex;
            audit.EndpointSupportAdditionalFaceB =
                isolatedRails[3].TargetSourceFaceIndex ==
                    audit.EndpointSupportFaceB
                    ? -1
                    : isolatedRails[3].TargetSourceFaceIndex;
            audit.EndpointSupportGraphFaceA =
                isolatedRails[0].TargetGraphFaceIndex;
            audit.EndpointSupportGraphFaceB =
                isolatedRails[1].TargetGraphFaceIndex;
            audit.EndpointSupportVertexA = sourceEdge.VertexA;
            audit.EndpointSupportVertexB = sourceEdge.VertexB;
            audit.EndpointSupportMultiFaceEndpointCount =
                (isolatedRails[0].TargetSourceFaceIndex !=
                    isolatedRails[2].TargetSourceFaceIndex ? 1 : 0) +
                (isolatedRails[1].TargetSourceFaceIndex !=
                    isolatedRails[3].TargetSourceFaceIndex ? 1 : 0);
            audit.TargetBoundaryCount = 4;
            boundarySubdivisionCount = 4;
            return true;
        }

        private static bool TryBuildBoundedRetainedPointHullFaces(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            EdgeWearSelectedGraphEdge selected,
            BoundedIsolatedRailPoint[] isolatedRails,
            Vector3 bevelNormal,
            float railPlaneDistance,
            Vector3 solidCentre,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            ref BoundedSingleEdgeAuditResult audit,
            out List<PolygonFace> boundedFaces,
            out int boundarySubdivisionCount,
            out string blocker)
        {
            boundedFaces = null;
            boundarySubdivisionCount = 0;
            blocker = string.Empty;
            audit.MultiSupportPlaneFirstForeignVertex = -1;
            if (sourceFaces == null || context == null ||
                isolatedRails == null || isolatedRails.Length != 4 ||
                selected.GraphEdgeIndex < 0 ||
                selected.GraphEdgeIndex >= context.Graph.Edges.Count)
            {
                blocker =
                    "the retained-point multi-support hull lacks exact source geometry";
                return false;
            }

            EdgeWearGraphEdge sourceEdge =
                context.Graph.Edges[selected.GraphEdgeIndex];
            Vector3 sourceA =
                context.Graph.Vertices[sourceEdge.VertexA].Position;
            Vector3 sourceB =
                context.Graph.Vertices[sourceEdge.VertexB].Position;
            float pointTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                minimumStableEdgeLength * 0.002f);
            float endpointToleranceSqr =
                pointTolerance * pointTolerance;

            List<Vector3> retainedPoints = new List<Vector3>();
            Dictionary<VertexKey, int> unique =
                new Dictionary<VertexKey, int>();
            for (int railIndex = 0;
                 railIndex < isolatedRails.Length;
                 railIndex++)
            {
                AddBoundedHullPoint(
                    isolatedRails[railIndex].Position,
                    retainedPoints,
                    unique);
            }
            for (int faceIndex = 0;
                 faceIndex < sourceFaces.Count;
                 faceIndex++)
            {
                PolygonFace sourceFace = sourceFaces[faceIndex];
                if (sourceFace == null || sourceFace.Vertices == null)
                {
                    blocker =
                        "the retained-point multi-support hull contains a null source face";
                    return false;
                }
                for (int vertexIndex = 0;
                     vertexIndex < sourceFace.Vertices.Count;
                     vertexIndex++)
                {
                    Vector3 point = sourceFace.Vertices[vertexIndex];
                    if ((point - sourceA).sqrMagnitude <=
                            endpointToleranceSqr ||
                        (point - sourceB).sqrMagnitude <=
                            endpointToleranceSqr)
                    {
                        continue;
                    }
                    AddBoundedHullPoint(point, retainedPoints, unique);
                }
            }
            audit.MultiSupportHullPointCount = retainedPoints.Count;
            if (retainedPoints.Count < 4)
            {
                blocker =
                    "the retained-point multi-support hull contains fewer than four unique points";
                return false;
            }

            BoundedAllEdgesAuditResult hullAudit =
                new BoundedAllEdgesAuditResult();
            if (!TryBuildBoundedConvexHullPlanes(
                    retainedPoints,
                    solidCentre,
                    pointTolerance,
                    hullAudit,
                    out List<BoundedHullPlane> hullPlanes,
                    out blocker))
            {
                blocker =
                    "retained-point multi-support hull plane extraction failed: " +
                    blocker;
                audit.MultiSupportHullTriplesTested =
                    hullAudit.HullTriplesTested;
                audit.MultiSupportHullSupportingTriples =
                    hullAudit.HullSupportingTriples;
                audit.MultiSupportHullEvidence =
                    hullAudit.HullPlaneEvidence ?? string.Empty;
                return false;
            }
            audit.MultiSupportHullTriplesTested =
                hullAudit.HullTriplesTested;
            audit.MultiSupportHullSupportingTriples =
                hullAudit.HullSupportingTriples;
            audit.MultiSupportHullPlaneCount = hullPlanes.Count;
            audit.MultiSupportHullEvidence =
                hullAudit.HullPlaneEvidence ?? string.Empty;

            boundedFaces = new List<PolygonFace>(hullPlanes.Count);
            List<int> bevelFaceIndices = new List<int>();
            for (int planeIndex = 0;
                 planeIndex < hullPlanes.Count;
                 planeIndex++)
            {
                BoundedHullPlane plane = hullPlanes[planeIndex];
                if (!TryOrderBoundedHullFacet(
                        retainedPoints,
                        plane,
                        out List<Vector3> ordered))
                {
                    blocker =
                        "a retained-point multi-support hull facet could not be ordered";
                    boundedFaces = null;
                    return false;
                }
                List<Vector3> sanitized =
                    SanitizePolygon(ordered, plane.Normal);
                if (sanitized.Count < 3 ||
                    CalculatePolygonArea(sanitized) <=
                        minimumStableFaceArea)
                {
                    blocker =
                        "a retained-point multi-support hull facet collapsed during sanitation";
                    boundedFaces = null;
                    return false;
                }
                Vector3 measuredNormal =
                    CalculatePolygonNormal(sanitized);
                if (!IsFinite(measuredNormal) ||
                    measuredNormal.sqrMagnitude <= MinimumEdgeLengthSqr)
                {
                    blocker =
                        "a retained-point multi-support hull facet has an invalid normal";
                    boundedFaces = null;
                    return false;
                }
                if (Vector3.Dot(measuredNormal, plane.Normal) < 0f)
                {
                    sanitized.Reverse();
                    measuredNormal = -measuredNormal;
                }
                if (!ValidateBoundedPolygon(
                        sanitized,
                        measuredNormal,
                        minimumStableFaceArea,
                        requireConvex: true,
                        out string facetBlocker,
                        out _))
                {
                    blocker =
                        "retained-point multi-support hull facet failed: " +
                        facetBlocker;
                    boundedFaces = null;
                    return false;
                }

                int sourceFaceIndex =
                    FindBoundedRetainedHullSourceFace(
                        sourceFaces,
                        plane.Normal,
                        plane.Distance,
                        pointTolerance);
                if (sourceFaceIndex >= 0)
                {
                    PolygonFace sourceFace = sourceFaces[sourceFaceIndex];
                    boundedFaces.Add(new PolygonFace(
                        sanitized,
                        measuredNormal,
                        sourceFace.Feature,
                        sourceFace.FeatureStrength,
                        PolygonFaceProvenanceKind.SourceFace,
                        sourceFaceIndex));
                }
                else
                {
                    bevelFaceIndices.Add(boundedFaces.Count);
                    boundedFaces.Add(new PolygonFace(
                        sanitized,
                        measuredNormal,
                        PolygonFaceFeature.ConvexEdgeWear,
                        selected.Candidate.Strength,
                        PolygonFaceProvenanceKind.BoundedEdgeBevel,
                        selected.GraphEdgeIndex));
                }
            }

            if (bevelFaceIndices.Count == 0)
            {
                blocker =
                    "the retained-point multi-support hull emitted no bevel facets";
                boundedFaces = null;
                return false;
            }
            for (int sourceFaceIndex = 0;
                 sourceFaceIndex < sourceFaces.Count;
                 sourceFaceIndex++)
            {
                if (FindBoundedSourceFace(
                        boundedFaces,
                        sourceFaceIndex) == null)
                {
                    blocker =
                        "the retained-point multi-support hull consumed a complete source face";
                    boundedFaces = null;
                    return false;
                }
            }

            if (ContainsBoundedHullVertex(
                    boundedFaces,
                    sourceA,
                    pointTolerance) ||
                ContainsBoundedHullVertex(
                    boundedFaces,
                    sourceB,
                    pointTolerance))
            {
                blocker =
                    "the retained-point multi-support hull did not remove both selected source-edge endpoints";
                boundedFaces = null;
                return false;
            }

            for (int railIndex = 0;
                 railIndex < isolatedRails.Length;
                 railIndex++)
            {
                if (!TryCanonicalizeBoundedHullRail(
                        boundedFaces,
                        bevelFaceIndices,
                        isolatedRails[railIndex].Position,
                        pointTolerance,
                        minimumStableFaceArea))
                {
                    blocker =
                        "an exact solved rail is absent from the retained-point bevel-band boundary";
                    boundedFaces = null;
                    return false;
                }
            }

            if (!TryAuditConnectedBoundedBevelBand(
                    boundedFaces,
                    bevelFaceIndices,
                    out int bevelAdjacencyCount))
            {
                blocker =
                    "the retained-point multi-support bevel facets do not form one connected band";
                boundedFaces = null;
                return false;
            }

            int ownerSourceFaceA =
                context.Graph.Faces[sourceEdge.FaceA].SourceFaceIndex;
            int ownerSourceFaceB =
                context.Graph.Faces[sourceEdge.FaceB].SourceFaceIndex;
            if (!DoesBoundedBevelBandShareEdgeWithSourceFace(
                    boundedFaces,
                    bevelFaceIndices,
                    ownerSourceFaceA) ||
                !DoesBoundedBevelBandShareEdgeWithSourceFace(
                    boundedFaces,
                    bevelFaceIndices,
                    ownerSourceFaceB))
            {
                blocker =
                    "the retained-point multi-support bevel band does not connect both owner faces";
                boundedFaces = null;
                return false;
            }
            HashSet<int> allowedSupportFaces =
                CollectBoundedEndpointSupportSourceFaces(
                    context,
                    sourceEdge);
            int ownerModifiedCount = 0;
            int supportModifiedCount = 0;
            for (int sourceFaceIndex = 0;
                 sourceFaceIndex < sourceFaces.Count;
                 sourceFaceIndex++)
            {
                PolygonFace resultFace = FindBoundedSourceFace(
                    boundedFaces,
                    sourceFaceIndex);
                bool equivalent =
                    AreBoundedPolygonsGeometricallyEquivalent(
                        sourceFaces[sourceFaceIndex].Vertices,
                        resultFace.Vertices,
                        sourceFaces[sourceFaceIndex].Normal,
                        out bool boundaryOnlyDifference);
                if (equivalent && !boundaryOnlyDifference)
                {
                    continue;
                }
                if (sourceFaceIndex == ownerSourceFaceA ||
                    sourceFaceIndex == ownerSourceFaceB)
                {
                    ownerModifiedCount++;
                }
                else if (allowedSupportFaces.Contains(sourceFaceIndex))
                {
                    supportModifiedCount++;
                }
                else
                {
                    blocker =
                        "the retained-point multi-support hull modified a source face outside the selected endpoint stars";
                    boundedFaces = null;
                    return false;
                }
            }
            if (ownerModifiedCount != 2 || supportModifiedCount < 1)
            {
                blocker =
                    "the retained-point multi-support hull did not modify both owners and a bounded endpoint-star interval";
                boundedFaces = null;
                return false;
            }

            audit.MultiSupportPlaneCut = 1;
            audit.MultiSupportPlaneCount = bevelFaceIndices.Count;
            audit.MultiSupportHullBevelFaceCount =
                bevelFaceIndices.Count;
            audit.MultiSupportPlaneCapAdjacencyCount =
                bevelAdjacencyCount;
            PolygonFace firstBevel =
                boundedFaces[bevelFaceIndices[0]];
            audit.MultiSupportPlaneNormal = firstBevel.Normal;
            audit.MultiSupportPlaneDistance = Vector3.Dot(
                firstBevel.Normal,
                firstBevel.Vertices[0]);
            if (bevelFaceIndices.Count > 1)
            {
                PolygonFace secondBevel =
                    boundedFaces[bevelFaceIndices[1]];
                audit.MultiSupportPlaneNormalB = secondBevel.Normal;
                audit.MultiSupportPlaneDistanceB = Vector3.Dot(
                    secondBevel.Normal,
                    secondBevel.Vertices[0]);
            }
            audit.MultiSupportPlaneChainCandidateCount =
                hullPlanes.Count;
            audit.OwnerClipAttemptedCount = 2;
            audit.OwnerClipCount = 2;
            audit.EndpointSupportClipAttemptedCount = 2;
            audit.EndpointSupportClipCount = 2;
            audit.EndpointSupportRemovedVertexCount = 2;
            audit.EndpointSupportRailInsertionCount = 4;
            audit.EndpointSupportModifiedFaceExpectedCount =
                supportModifiedCount;
            audit.EndpointSupportBoundaryPathVertexCount =
                CountBoundedBevelBandBoundaryVertices(
                    boundedFaces,
                    bevelFaceIndices);
            audit.EndpointSupportFaceA =
                isolatedRails[0].TargetSourceFaceIndex;
            audit.EndpointSupportAdditionalFaceA =
                isolatedRails[2].TargetSourceFaceIndex ==
                    audit.EndpointSupportFaceA
                    ? -1
                    : isolatedRails[2].TargetSourceFaceIndex;
            audit.EndpointSupportFaceB =
                isolatedRails[1].TargetSourceFaceIndex;
            audit.EndpointSupportAdditionalFaceB =
                isolatedRails[3].TargetSourceFaceIndex ==
                    audit.EndpointSupportFaceB
                    ? -1
                    : isolatedRails[3].TargetSourceFaceIndex;
            audit.EndpointSupportGraphFaceA =
                isolatedRails[0].TargetGraphFaceIndex;
            audit.EndpointSupportGraphFaceB =
                isolatedRails[1].TargetGraphFaceIndex;
            audit.EndpointSupportVertexA = sourceEdge.VertexA;
            audit.EndpointSupportVertexB = sourceEdge.VertexB;
            audit.EndpointSupportMultiFaceEndpointCount =
                (isolatedRails[0].TargetSourceFaceIndex !=
                    isolatedRails[2].TargetSourceFaceIndex ? 1 : 0) +
                (isolatedRails[1].TargetSourceFaceIndex !=
                    isolatedRails[3].TargetSourceFaceIndex ? 1 : 0);
            audit.TargetBoundaryCount = 4;
            boundarySubdivisionCount = 4;
            return true;
        }

        private static int FindBoundedRetainedHullSourceFace(
            List<PolygonFace> sourceFaces,
            Vector3 normal,
            float distance,
            float tolerance)
        {
            const float NormalAgreement = 0.99998f;
            float distanceTolerance = Mathf.Max(
                tolerance * 2f,
                PointMergeDistance * 8f);
            for (int faceIndex = 0;
                 faceIndex < sourceFaces.Count;
                 faceIndex++)
            {
                PolygonFace face = sourceFaces[faceIndex];
                if (face == null || face.Vertices.Count == 0 ||
                    Vector3.Dot(face.Normal, normal) < NormalAgreement)
                {
                    continue;
                }
                float sourceDistance = Vector3.Dot(
                    face.Normal,
                    face.Vertices[0]);
                if (Mathf.Abs(sourceDistance - distance) <=
                    distanceTolerance)
                {
                    return faceIndex;
                }
            }
            return -1;
        }

        private static bool ContainsBoundedHullVertex(
            List<PolygonFace> faces,
            Vector3 point,
            float tolerance)
        {
            float toleranceSqr = tolerance * tolerance;
            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                for (int vertexIndex = 0;
                     vertexIndex < faces[faceIndex].Vertices.Count;
                     vertexIndex++)
                {
                    if ((faces[faceIndex].Vertices[vertexIndex] - point)
                            .sqrMagnitude <= toleranceSqr)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool TryCanonicalizeBoundedHullRail(
            List<PolygonFace> faces,
            List<int> bevelFaceIndices,
            Vector3 rail,
            float pointTolerance,
            float minimumStableFaceArea)
        {
            for (int bevelIndex = 0;
                 bevelIndex < bevelFaceIndices.Count;
                 bevelIndex++)
            {
                int faceIndex = bevelFaceIndices[bevelIndex];
                PolygonFace face = faces[faceIndex];
                List<Vector3> vertices =
                    CopyBoundedPolygonPreservingCollinear(
                        face.Vertices);
                if (!TryInsertBoundedPointIntoPolygonBoundary(
                        vertices,
                        rail,
                        pointTolerance,
                        out _))
                {
                    continue;
                }
                if (!ValidateBoundedPolygon(
                        vertices,
                        face.Normal,
                        minimumStableFaceArea,
                        requireConvex: true,
                        out _,
                        out _))
                {
                    continue;
                }
                faces[faceIndex] = new PolygonFace(
                    vertices,
                    face.Normal,
                    face.Feature,
                    face.FeatureStrength,
                    face.ProvenanceKind,
                    face.ProvenanceIndex);
                return true;
            }
            return false;
        }

        private static bool TryAuditConnectedBoundedBevelBand(
            List<PolygonFace> faces,
            List<int> bevelFaceIndices,
            out int adjacencyCount)
        {
            adjacencyCount = 0;
            if (bevelFaceIndices == null ||
                bevelFaceIndices.Count == 0)
            {
                return false;
            }
            bool[] visited = new bool[bevelFaceIndices.Count];
            Queue<int> queue = new Queue<int>();
            visited[0] = true;
            queue.Enqueue(0);
            int visitedCount = 0;
            while (queue.Count > 0)
            {
                int localIndex = queue.Dequeue();
                visitedCount++;
                PolygonFace current =
                    faces[bevelFaceIndices[localIndex]];
                for (int otherIndex = 0;
                     otherIndex < bevelFaceIndices.Count;
                     otherIndex++)
                {
                    if (otherIndex == localIndex)
                    {
                        continue;
                    }
                    PolygonFace other =
                        faces[bevelFaceIndices[otherIndex]];
                    if (CountBoundedPolygonSharedVertices(
                            current,
                            other) < 2)
                    {
                        continue;
                    }
                    if (localIndex < otherIndex)
                    {
                        adjacencyCount++;
                    }
                    if (!visited[otherIndex])
                    {
                        visited[otherIndex] = true;
                        queue.Enqueue(otherIndex);
                    }
                }
            }
            return visitedCount == bevelFaceIndices.Count;
        }

        private static bool
            DoesBoundedBevelBandShareEdgeWithSourceFace(
                List<PolygonFace> faces,
                List<int> bevelFaceIndices,
                int sourceFaceIndex)
        {
            PolygonFace sourceFace = FindBoundedSourceFace(
                faces,
                sourceFaceIndex);
            if (sourceFace == null)
            {
                return false;
            }
            for (int bevelIndex = 0;
                 bevelIndex < bevelFaceIndices.Count;
                 bevelIndex++)
            {
                if (CountBoundedPolygonSharedVertices(
                        sourceFace,
                        faces[bevelFaceIndices[bevelIndex]]) >= 2)
                {
                    return true;
                }
            }
            return false;
        }

        private static int CountBoundedPolygonSharedVertices(
            PolygonFace first,
            PolygonFace second)
        {
            HashSet<VertexKey> firstKeys = new HashSet<VertexKey>();
            for (int vertexIndex = 0;
                 vertexIndex < first.Vertices.Count;
                 vertexIndex++)
            {
                firstKeys.Add(new VertexKey(first.Vertices[vertexIndex]));
            }
            int count = 0;
            HashSet<VertexKey> counted = new HashSet<VertexKey>();
            for (int vertexIndex = 0;
                 vertexIndex < second.Vertices.Count;
                 vertexIndex++)
            {
                VertexKey key = new VertexKey(
                    second.Vertices[vertexIndex]);
                if (firstKeys.Contains(key) && counted.Add(key))
                {
                    count++;
                }
            }
            return count;
        }

        private static int CountBoundedBevelBandBoundaryVertices(
            List<PolygonFace> faces,
            List<int> bevelFaceIndices)
        {
            HashSet<VertexKey> vertices = new HashSet<VertexKey>();
            for (int bevelIndex = 0;
                 bevelIndex < bevelFaceIndices.Count;
                 bevelIndex++)
            {
                PolygonFace face = faces[bevelFaceIndices[bevelIndex]];
                for (int vertexIndex = 0;
                     vertexIndex < face.Vertices.Count;
                     vertexIndex++)
                {
                    vertices.Add(new VertexKey(
                        face.Vertices[vertexIndex]));
                }
            }
            return vertices.Count;
        }

        private static HashSet<int> CollectBoundedEndpointSupportSourceFaces(
            ChamferTopologyContext context,
            EdgeWearGraphEdge sourceEdge)
        {
            HashSet<int> result = new HashSet<int>();
            int[] endpointVertices =
            {
                sourceEdge.VertexA,
                sourceEdge.VertexB
            };
            for (int endpointIndex = 0;
                 endpointIndex < endpointVertices.Length;
                 endpointIndex++)
            {
                EdgeWearGraphVertex vertex =
                    context.Graph.Vertices[
                        endpointVertices[endpointIndex]];
                for (int faceIndex = 0;
                     faceIndex < vertex.FaceIndices.Count;
                     faceIndex++)
                {
                    int graphFaceIndex =
                        vertex.FaceIndices[faceIndex];
                    if (graphFaceIndex < 0 ||
                        graphFaceIndex >= context.Graph.Faces.Count)
                    {
                        continue;
                    }
                    result.Add(
                        context.Graph.Faces[graphFaceIndex]
                            .SourceFaceIndex);
                }
            }
            return result;
        }

        private static bool TryInsertBoundedPointIntoPolygonBoundary(
            List<Vector3> vertices,
            Vector3 point,
            float tolerance,
            out bool inserted)
        {
            inserted = false;
            if (vertices == null || vertices.Count < 3 ||
                !IsFinite(point) || tolerance <= 0f)
            {
                return false;
            }
            float toleranceSqr = tolerance * tolerance;
            for (int vertexIndex = 0;
                 vertexIndex < vertices.Count;
                 vertexIndex++)
            {
                if ((vertices[vertexIndex] - point).sqrMagnitude <=
                    toleranceSqr)
                {
                    vertices[vertexIndex] = point;
                    return true;
                }
            }

            int insertionIndex = -1;
            float bestResidual = float.PositiveInfinity;
            for (int edgeIndex = 0;
                 edgeIndex < vertices.Count;
                 edgeIndex++)
            {
                Vector3 start = vertices[edgeIndex];
                Vector3 end = vertices[(edgeIndex + 1) % vertices.Count];
                Vector3 segment = end - start;
                float lengthSqr = segment.sqrMagnitude;
                if (lengthSqr <= MinimumEdgeLengthSqr)
                {
                    continue;
                }
                float parameter = Vector3.Dot(
                    point - start,
                    segment) / lengthSqr;
                float length = Mathf.Sqrt(lengthSqr);
                float parameterTolerance = Mathf.Min(
                    0.25f,
                    tolerance / length);
                if (parameter <= parameterTolerance ||
                    parameter >= 1f - parameterTolerance)
                {
                    continue;
                }
                Vector3 closest = start + segment * parameter;
                float residual = Vector3.Distance(point, closest);
                if (residual <= tolerance && residual < bestResidual)
                {
                    bestResidual = residual;
                    insertionIndex = edgeIndex + 1;
                }
            }
            if (insertionIndex < 0)
            {
                return false;
            }
            vertices.Insert(insertionIndex, point);
            inserted = true;
            return true;
        }

        private static bool TryClipBoundedEndpointSupportFaces(
            List<PolygonFace> sourceFaces,
            List<PolygonFace> boundedFaces,
            ChamferTopologyContext context,
            int ownerFaceA,
            int ownerFaceB,
            int sourceVertexA,
            int sourceVertexB,
            BoundedIsolatedRailPoint[] rails,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            ref BoundedSingleEdgeAuditResult audit,
            out int railInsertionCount,
            out int targetBoundaryCount,
            out string blocker)
        {
            railInsertionCount = 0;
            targetBoundaryCount = 0;
            blocker = string.Empty;
            if (rails == null || rails.Length != 4)
            {
                blocker = "the endpoint support clip does not contain four solved rails";
                return false;
            }

            if (!TryClipBoundedEndpointSupportFace(
                    sourceFaces,
                    context,
                    ownerFaceA,
                    ownerFaceB,
                    0,
                    sourceVertexA,
                    rails[0],
                    rails[2],
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    ref audit,
                    out int supportFaceA,
                    out PolygonFace replacementA,
                    out blocker) ||
                !TryClipBoundedEndpointSupportFace(
                    sourceFaces,
                    context,
                    ownerFaceA,
                    ownerFaceB,
                    1,
                    sourceVertexB,
                    rails[1],
                    rails[3],
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    ref audit,
                    out int supportFaceB,
                    out PolygonFace replacementB,
                    out blocker))
            {
                return false;
            }

            if (supportFaceA == supportFaceB)
            {
                audit.EndpointSupportSharedFaceFailureCount++;
                blocker =
                    "the two bounded endpoints unexpectedly resolve to the same support face";
                return false;
            }

            audit.EndpointSupportFaceA = supportFaceA;
            audit.EndpointSupportFaceB = supportFaceB;
            boundedFaces[supportFaceA] = replacementA;
            boundedFaces[supportFaceB] = replacementB;
            railInsertionCount = 4;
            targetBoundaryCount = 4;
            audit.EndpointSupportRailInsertionCount = 4;
            return true;
        }

        private static bool TryClipBoundedEndpointSupportFace(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            int ownerFaceA,
            int ownerFaceB,
            int endpointOrdinal,
            int sourceVertexIndex,
            BoundedIsolatedRailPoint firstRail,
            BoundedIsolatedRailPoint secondRail,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            ref BoundedSingleEdgeAuditResult audit,
            out int supportSourceFaceIndex,
            out PolygonFace replacement,
            out string blocker)
        {
            supportSourceFaceIndex = -1;
            replacement = null;
            blocker = string.Empty;
            audit.EndpointSupportClipAttemptedCount++;

            if (firstRail.SourceVertexIndex != sourceVertexIndex ||
                secondRail.SourceVertexIndex != sourceVertexIndex ||
                firstRail.TargetSourceFaceIndex < 0 ||
                firstRail.TargetSourceFaceIndex >= sourceFaces.Count ||
                firstRail.TargetSourceFaceIndex !=
                    secondRail.TargetSourceFaceIndex ||
                firstRail.TargetGraphFaceIndex < 0 ||
                firstRail.TargetGraphFaceIndex >= context.Graph.Faces.Count ||
                firstRail.TargetGraphFaceIndex !=
                    secondRail.TargetGraphFaceIndex)
            {
                audit.EndpointSupportSharedFaceFailureCount++;
                blocker =
                    "the two rails at one bounded endpoint do not share one exact support face";
                return false;
            }

            supportSourceFaceIndex = firstRail.TargetSourceFaceIndex;
            if (endpointOrdinal == 0)
            {
                audit.EndpointSupportFaceA = supportSourceFaceIndex;
                audit.EndpointSupportGraphFaceA =
                    firstRail.TargetGraphFaceIndex;
                audit.EndpointSupportVertexA = sourceVertexIndex;
            }
            else
            {
                audit.EndpointSupportFaceB = supportSourceFaceIndex;
                audit.EndpointSupportGraphFaceB =
                    firstRail.TargetGraphFaceIndex;
                audit.EndpointSupportVertexB = sourceVertexIndex;
            }
            if (supportSourceFaceIndex == ownerFaceA ||
                supportSourceFaceIndex == ownerFaceB)
            {
                audit.EndpointSupportSharedFaceFailureCount++;
                blocker =
                    "a bounded endpoint support face aliases one of the selected edge owners";
                return false;
            }

            EdgeWearGraphFace supportGraphFace =
                context.Graph.Faces[firstRail.TargetGraphFaceIndex];
            if (supportGraphFace.SourceFaceIndex !=
                    supportSourceFaceIndex ||
                supportGraphFace.VertexIndices == null ||
                supportGraphFace.EdgeIndices == null)
            {
                audit.EndpointSupportIncidenceFailureCount++;
                blocker =
                    "the bounded endpoint support face has inconsistent graph provenance";
                return false;
            }

            PolygonFace sourceFace = sourceFaces[supportSourceFaceIndex];
            if (sourceFace == null || sourceFace.Vertices == null ||
                sourceFace.Vertices.Count !=
                    supportGraphFace.VertexIndices.Count ||
                supportGraphFace.EdgeIndices.Count !=
                    supportGraphFace.VertexIndices.Count)
            {
                audit.EndpointSupportIncidenceFailureCount++;
                blocker =
                    "the bounded endpoint support face has inconsistent source geometry";
                return false;
            }

            int localVertexIndex =
                supportGraphFace.VertexIndices.IndexOf(sourceVertexIndex);
            if (localVertexIndex < 0)
            {
                audit.EndpointSupportIncidenceFailureCount++;
                blocker =
                    "the bounded endpoint source vertex is absent from its support face";
                return false;
            }

            int vertexCount = supportGraphFace.VertexIndices.Count;
            int previousEdgeIndex = supportGraphFace.EdgeIndices[
                (localVertexIndex + vertexCount - 1) % vertexCount];
            int nextEdgeIndex =
                supportGraphFace.EdgeIndices[localVertexIndex];
            BoundedIsolatedRailPoint previousRail;
            BoundedIsolatedRailPoint nextRail;
            if (firstRail.AdjacentGraphEdgeIndex == previousEdgeIndex &&
                secondRail.AdjacentGraphEdgeIndex == nextEdgeIndex)
            {
                previousRail = firstRail;
                nextRail = secondRail;
            }
            else if (secondRail.AdjacentGraphEdgeIndex == previousEdgeIndex &&
                firstRail.AdjacentGraphEdgeIndex == nextEdgeIndex)
            {
                previousRail = secondRail;
                nextRail = firstRail;
            }
            else
            {
                audit.EndpointSupportIncidenceFailureCount++;
                blocker =
                    "the bounded endpoint rails do not own the two support-face edges incident to the source vertex";
                return false;
            }

            float pointTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                minimumStableEdgeLength * 0.002f);
            float pointToleranceSqr = pointTolerance * pointTolerance;
            Vector3 sourceVertex =
                context.Graph.Vertices[sourceVertexIndex].Position;
            bool previousRailInside =
                TryGetBoundedPointGraphEdgeEvidence(
                    context,
                    previousEdgeIndex,
                    previousRail.Position,
                    pointTolerance,
                    out float previousParameter,
                    out float previousEdgeResidual);
            bool nextRailInside =
                TryGetBoundedPointGraphEdgeEvidence(
                    context,
                    nextEdgeIndex,
                    nextRail.Position,
                    pointTolerance,
                    out float nextParameter,
                    out float nextEdgeResidual);

            if (endpointOrdinal == 0)
            {
                audit.EndpointSupportPreviousEdgeA = previousEdgeIndex;
                audit.EndpointSupportNextEdgeA = nextEdgeIndex;
                audit.EndpointSupportPreviousRailA = previousRail.RailIndex;
                audit.EndpointSupportNextRailA = nextRail.RailIndex;
                audit.EndpointSupportSourcePositionA = sourceVertex;
                audit.EndpointSupportPreviousRailPositionA =
                    previousRail.Position;
                audit.EndpointSupportNextRailPositionA = nextRail.Position;
                audit.EndpointSupportPreviousParameterA = previousParameter;
                audit.EndpointSupportNextParameterA = nextParameter;
                audit.EndpointSupportPreviousEdgeResidualA =
                    previousEdgeResidual;
                audit.EndpointSupportNextEdgeResidualA = nextEdgeResidual;
            }
            else
            {
                audit.EndpointSupportPreviousEdgeB = previousEdgeIndex;
                audit.EndpointSupportNextEdgeB = nextEdgeIndex;
                audit.EndpointSupportPreviousRailB = previousRail.RailIndex;
                audit.EndpointSupportNextRailB = nextRail.RailIndex;
                audit.EndpointSupportSourcePositionB = sourceVertex;
                audit.EndpointSupportPreviousRailPositionB =
                    previousRail.Position;
                audit.EndpointSupportNextRailPositionB = nextRail.Position;
                audit.EndpointSupportPreviousParameterB = previousParameter;
                audit.EndpointSupportNextParameterB = nextParameter;
                audit.EndpointSupportPreviousEdgeResidualB =
                    previousEdgeResidual;
                audit.EndpointSupportNextEdgeResidualB = nextEdgeResidual;
            }

            if ((sourceFace.Vertices[localVertexIndex] - sourceVertex)
                    .sqrMagnitude > pointToleranceSqr ||
                !previousRailInside ||
                !nextRailInside)
            {
                audit.EndpointSupportIncidenceFailureCount++;
                blocker =
                    "the bounded endpoint rails do not lie inside their exact support-face boundary edges";
                return false;
            }

            Vector3 normal = sourceFace.Normal;
            if (!IsFinite(normal) ||
                normal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                audit.EndpointSupportNonPlanarCount++;
                blocker =
                    "the bounded endpoint support face has an invalid analytical plane";
                return false;
            }
            normal.Normalize();
            float planeDistance = Vector3.Dot(
                normal,
                sourceFace.Vertices[0]);
            float previousPlaneResidual = Mathf.Abs(Vector3.Dot(
                normal,
                previousRail.Position) - planeDistance);
            float nextPlaneResidual = Mathf.Abs(Vector3.Dot(
                normal,
                nextRail.Position) - planeDistance);
            if (endpointOrdinal == 0)
            {
                audit.EndpointSupportNormalA = normal;
                audit.EndpointSupportPreviousPlaneResidualA =
                    previousPlaneResidual;
                audit.EndpointSupportNextPlaneResidualA = nextPlaneResidual;
            }
            else
            {
                audit.EndpointSupportNormalB = normal;
                audit.EndpointSupportPreviousPlaneResidualB =
                    previousPlaneResidual;
                audit.EndpointSupportNextPlaneResidualB = nextPlaneResidual;
            }
            if (previousPlaneResidual > pointTolerance ||
                nextPlaneResidual > pointTolerance)
            {
                audit.EndpointSupportNonPlanarCount++;
                blocker =
                    "a bounded endpoint rail leaves its support-face plane";
                return false;
            }

            List<Vector3> clipped = new List<Vector3>(
                sourceFace.Vertices.Count + 1);
            for (int vertexIndex = 0;
                 vertexIndex < sourceFace.Vertices.Count;
                 vertexIndex++)
            {
                if (vertexIndex == localVertexIndex)
                {
                    AddBoundedVertex(clipped, previousRail.Position);
                    AddBoundedVertex(clipped, nextRail.Position);
                    continue;
                }
                AddBoundedVertex(
                    clipped,
                    sourceFace.Vertices[vertexIndex]);
            }
            RemoveClosingDuplicate(clipped);

            if (CountBoundedPointMatches(
                    clipped,
                    sourceVertex,
                    pointToleranceSqr) != 0 ||
                CountBoundedPointMatches(
                    clipped,
                    previousRail.Position,
                    pointToleranceSqr) != 1 ||
                CountBoundedPointMatches(
                    clipped,
                    nextRail.Position,
                    pointToleranceSqr) != 1 ||
                !AreBoundedPointsAdjacent(
                    clipped,
                    previousRail.Position,
                    nextRail.Position,
                    pointToleranceSqr))
            {
                audit.EndpointSupportIncidenceFailureCount++;
                blocker =
                    "the bounded endpoint support clip did not replace exactly one source corner with one rail-to-rail boundary";
                return false;
            }

            if (!ValidateBoundedPolygon(
                    clipped,
                    sourceFace.Normal,
                    minimumStableFaceArea,
                    requireConvex: true,
                    out string validationBlocker,
                    out BoundedPolygonFailure failure))
            {
                RecordBoundedEndpointSupportFailure(
                    ref audit,
                    failure);
                blocker =
                    "endpoint support face " + supportSourceFaceIndex +
                    " clip failed: " + validationBlocker;
                return false;
            }

            replacement = new PolygonFace(
                clipped,
                sourceFace.Normal,
                sourceFace.Feature,
                sourceFace.FeatureStrength,
                PolygonFaceProvenanceKind.SourceFace,
                supportSourceFaceIndex);
            audit.EndpointSupportClipCount++;
            audit.EndpointSupportRemovedVertexCount++;
            return true;
        }

        private static bool TryGetBoundedPointGraphEdgeEvidence(
            ChamferTopologyContext context,
            int graphEdgeIndex,
            Vector3 point,
            float tolerance,
            out float parameter,
            out float residual)
        {
            parameter = 0f;
            residual = 0f;
            if (context == null || graphEdgeIndex < 0 ||
                graphEdgeIndex >= context.Graph.Edges.Count)
            {
                return false;
            }

            EdgeWearGraphEdge edge = context.Graph.Edges[graphEdgeIndex];
            Vector3 start =
                context.Graph.Vertices[edge.VertexA].Position;
            Vector3 end =
                context.Graph.Vertices[edge.VertexB].Position;
            Vector3 segment = end - start;
            float lengthSqr = segment.sqrMagnitude;
            if (lengthSqr <= MinimumEdgeLengthSqr)
            {
                return false;
            }

            parameter = Vector3.Dot(
                point - start,
                segment) / lengthSqr;
            float length = Mathf.Sqrt(lengthSqr);
            float parameterTolerance = Mathf.Min(
                0.25f,
                tolerance / length);
            Vector3 closest = start + segment * parameter;
            residual = Vector3.Distance(point, closest);
            return parameter > parameterTolerance &&
                parameter < 1f - parameterTolerance &&
                residual <= tolerance;
        }

        private static void RecordBoundedEndpointSupportFailure(
            ref BoundedSingleEdgeAuditResult audit,
            BoundedPolygonFailure failure)
        {
            switch (failure)
            {
                case BoundedPolygonFailure.Degenerate:
                case BoundedPolygonFailure.NonFinite:
                    audit.EndpointSupportDegenerateCount++;
                    break;
                case BoundedPolygonFailure.NonPlanar:
                    audit.EndpointSupportNonPlanarCount++;
                    break;
                case BoundedPolygonFailure.NonSimple:
                    audit.EndpointSupportNonSimpleCount++;
                    break;
                case BoundedPolygonFailure.NonConvex:
                    audit.EndpointSupportNonConvexCount++;
                    break;
                case BoundedPolygonFailure.Winding:
                    audit.EndpointSupportWindingFailureCount++;
                    break;
                default:
                    audit.EndpointSupportIncidenceFailureCount++;
                    break;
            }
        }

        private static bool TryClipBoundedOwnerSourceFace(
            PolygonFace sourceFace,
            EdgeWearGraphFace graphFace,
            int vertexA,
            int vertexB,
            Vector3 railAtA,
            Vector3 railAtB,
            int sourceFaceIndex,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            ref BoundedSingleEdgeAuditResult audit,
            out PolygonFace replacement,
            out string blocker)
        {
            replacement = null;
            blocker = string.Empty;
            audit.OwnerClipAttemptedCount++;

            if (sourceFace == null ||
                sourceFace.Vertices == null ||
                graphFace.VertexIndices.Count != sourceFace.Vertices.Count)
            {
                RecordBoundedOwnerClipFailure(
                    ref audit,
                    BoundedOwnerClipFailure.Intersection);
                blocker =
                    "owner face " + sourceFaceIndex +
                    " has inconsistent graph provenance";
                return false;
            }

            Vector3 normal = sourceFace.Normal;
            if (!IsFinite(normal) ||
                normal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                RecordBoundedOwnerClipFailure(
                    ref audit,
                    BoundedOwnerClipFailure.NonPlanar);
                blocker =
                    "owner face " + sourceFaceIndex +
                    " has an invalid analytical plane";
                return false;
            }
            normal.Normalize();

            float pointTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                minimumStableEdgeLength * 0.002f);
            float pointToleranceSqr = pointTolerance * pointTolerance;
            float planeDistance = Vector3.Dot(
                normal,
                sourceFace.Vertices[0]);
            if (Mathf.Abs(Vector3.Dot(normal, railAtA) -
                    planeDistance) > pointTolerance ||
                Mathf.Abs(Vector3.Dot(normal, railAtB) -
                    planeDistance) > pointTolerance)
            {
                RecordBoundedOwnerClipFailure(
                    ref audit,
                    BoundedOwnerClipFailure.NonPlanar);
                blocker =
                    "owner face " + sourceFaceIndex +
                    " solved rail leaves the source-face plane";
                return false;
            }

            Vector3 reference = Mathf.Abs(normal.y) < 0.9f
                ? Vector3.up
                : Vector3.right;
            Vector3 tangent = Vector3.Cross(reference, normal);
            if (tangent.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                reference = Vector3.forward;
                tangent = Vector3.Cross(reference, normal);
            }
            if (tangent.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                RecordBoundedOwnerClipFailure(
                    ref audit,
                    BoundedOwnerClipFailure.NonPlanar);
                blocker =
                    "owner face " + sourceFaceIndex +
                    " cannot form a stable projection basis";
                return false;
            }
            tangent.Normalize();
            Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;
            Vector3 projectionOrigin = sourceFace.Vertices[0];

            Vector2 railA2 = ProjectBoundedOwnerPoint(
                railAtA,
                projectionOrigin,
                tangent,
                bitangent);
            Vector2 railB2 = ProjectBoundedOwnerPoint(
                railAtB,
                projectionOrigin,
                tangent,
                bitangent);
            float railLength = Vector2.Distance(railA2, railB2);
            if (railLength <= pointTolerance)
            {
                RecordBoundedOwnerClipFailure(
                    ref audit,
                    BoundedOwnerClipFailure.Degenerate);
                blocker =
                    "owner face " + sourceFaceIndex +
                    " rail is too short for local clipping";
                return false;
            }

            Vector3 retainedReference = Vector3.zero;
            int retainedReferenceCount = 0;
            for (int vertexIndex = 0;
                 vertexIndex < graphFace.VertexIndices.Count;
                 vertexIndex++)
            {
                int graphVertex = graphFace.VertexIndices[vertexIndex];
                if (graphVertex == vertexA || graphVertex == vertexB)
                {
                    continue;
                }
                retainedReference += sourceFace.Vertices[vertexIndex];
                retainedReferenceCount++;
            }
            if (retainedReferenceCount == 0)
            {
                retainedReference = CalculateAverage(sourceFace.Vertices);
            }
            else
            {
                retainedReference /= retainedReferenceCount;
            }

            Vector2 retainedReference2 = ProjectBoundedOwnerPoint(
                retainedReference,
                projectionOrigin,
                tangent,
                bitangent);
            float keepSide = BoundedOwnerSide(
                railA2,
                railB2,
                retainedReference2);
            float sideTolerance = pointTolerance * railLength;
            if (Mathf.Abs(keepSide) <= sideTolerance)
            {
                Vector2 centroid2 = ProjectBoundedOwnerPoint(
                    CalculateAverage(sourceFace.Vertices),
                    projectionOrigin,
                    tangent,
                    bitangent);
                keepSide = BoundedOwnerSide(
                    railA2,
                    railB2,
                    centroid2);
            }
            if (Mathf.Abs(keepSide) <= sideTolerance)
            {
                RecordBoundedOwnerClipFailure(
                    ref audit,
                    BoundedOwnerClipFailure.Intersection);
                blocker =
                    "owner face " + sourceFaceIndex +
                    " cannot determine the retained side of its rail";
                return false;
            }
            float keepSign = Mathf.Sign(keepSide);

            List<Vector3> clipped = new List<Vector3>(
                sourceFace.Vertices.Count + 2);
            for (int vertexIndex = 0;
                 vertexIndex < sourceFace.Vertices.Count;
                 vertexIndex++)
            {
                Vector3 current = sourceFace.Vertices[vertexIndex];
                Vector3 next = sourceFace.Vertices[
                    (vertexIndex + 1) % sourceFace.Vertices.Count];
                Vector2 current2 = ProjectBoundedOwnerPoint(
                    current,
                    projectionOrigin,
                    tangent,
                    bitangent);
                Vector2 next2 = ProjectBoundedOwnerPoint(
                    next,
                    projectionOrigin,
                    tangent,
                    bitangent);
                float currentSide = BoundedOwnerSide(
                    railA2,
                    railB2,
                    current2);
                float nextSide = BoundedOwnerSide(
                    railA2,
                    railB2,
                    next2);
                bool currentInside =
                    currentSide * keepSign >= -sideTolerance;
                bool nextInside =
                    nextSide * keepSign >= -sideTolerance;

                if (currentInside)
                {
                    AddBoundedVertex(
                        clipped,
                        SnapBoundedOwnerRailPoint(
                            current,
                            railAtA,
                            railAtB,
                            pointToleranceSqr));
                }

                if (currentInside == nextInside)
                {
                    continue;
                }

                float denominator = currentSide - nextSide;
                if (Mathf.Abs(denominator) <= sideTolerance)
                {
                    RecordBoundedOwnerClipFailure(
                        ref audit,
                        BoundedOwnerClipFailure.Intersection);
                    blocker =
                        "owner face " + sourceFaceIndex +
                        " rail crossing is numerically unstable";
                    return false;
                }

                float parameter = Mathf.Clamp01(
                    currentSide / denominator);
                Vector3 intersection = Vector3.Lerp(
                    current,
                    next,
                    parameter);
                intersection = SnapBoundedOwnerRailPoint(
                    intersection,
                    railAtA,
                    railAtB,
                    pointToleranceSqr);
                if ((intersection - railAtA).sqrMagnitude >
                        pointToleranceSqr &&
                    (intersection - railAtB).sqrMagnitude >
                        pointToleranceSqr)
                {
                    RecordBoundedOwnerClipFailure(
                        ref audit,
                        BoundedOwnerClipFailure.Intersection);
                    blocker =
                        "owner face " + sourceFaceIndex +
                        " rail intersection does not match a solved endpoint";
                    return false;
                }

                AddBoundedVertex(clipped, intersection);
            }
            RemoveClosingDuplicate(clipped);

            int railAMatches = CountBoundedPointMatches(
                clipped,
                railAtA,
                pointToleranceSqr);
            int railBMatches = CountBoundedPointMatches(
                clipped,
                railAtB,
                pointToleranceSqr);
            int railBoundaryVertexCount =
                CountBoundedOwnerRailBoundaryVertices(
                    clipped,
                    projectionOrigin,
                    tangent,
                    bitangent,
                    railA2,
                    railB2,
                    sideTolerance);
            if (railBoundaryVertexCount != 2 ||
                railAMatches != 1 || railBMatches != 1 ||
                !AreBoundedPointsAdjacent(
                    clipped,
                    railAtA,
                    railAtB,
                    pointToleranceSqr))
            {
                RecordBoundedOwnerClipFailure(
                    ref audit,
                    BoundedOwnerClipFailure.Intersection);
                blocker =
                    "owner face " + sourceFaceIndex +
                    " rail did not produce exactly two matching boundary intersections";
                return false;
            }

            if (!ValidateBoundedPolygon(
                    clipped,
                    sourceFace.Normal,
                    minimumStableFaceArea,
                    requireConvex: true,
                    out string validationBlocker))
            {
                BoundedOwnerClipFailure failure =
                    ClassifyBoundedOwnerClipFailure(validationBlocker);
                RecordBoundedOwnerClipFailure(ref audit, failure);
                blocker =
                    "owner face " + sourceFaceIndex +
                    " clip failed: " + validationBlocker;
                return false;
            }

            replacement = new PolygonFace(
                clipped,
                sourceFace.Normal,
                sourceFace.Feature,
                sourceFace.FeatureStrength,
                PolygonFaceProvenanceKind.SourceFace,
                sourceFaceIndex);
            audit.OwnerClipCount++;
            return true;
        }

        private static Vector2 ProjectBoundedOwnerPoint(
            Vector3 point,
            Vector3 origin,
            Vector3 tangent,
            Vector3 bitangent)
        {
            Vector3 relative = point - origin;
            return new Vector2(
                Vector3.Dot(relative, tangent),
                Vector3.Dot(relative, bitangent));
        }

        private static float BoundedOwnerSide(
            Vector2 lineStart,
            Vector2 lineEnd,
            Vector2 point)
        {
            Vector2 line = lineEnd - lineStart;
            Vector2 relative = point - lineStart;
            return line.x * relative.y - line.y * relative.x;
        }

        private static Vector3 SnapBoundedOwnerRailPoint(
            Vector3 point,
            Vector3 railA,
            Vector3 railB,
            float toleranceSqr)
        {
            if ((point - railA).sqrMagnitude <= toleranceSqr)
            {
                return railA;
            }
            if ((point - railB).sqrMagnitude <= toleranceSqr)
            {
                return railB;
            }
            return point;
        }

        private static int CountBoundedOwnerRailBoundaryVertices(
            List<Vector3> points,
            Vector3 origin,
            Vector3 tangent,
            Vector3 bitangent,
            Vector2 railStart,
            Vector2 railEnd,
            float sideTolerance)
        {
            int count = 0;
            for (int index = 0; index < points.Count; index++)
            {
                Vector2 projected = ProjectBoundedOwnerPoint(
                    points[index],
                    origin,
                    tangent,
                    bitangent);
                if (Mathf.Abs(BoundedOwnerSide(
                        railStart,
                        railEnd,
                        projected)) <= sideTolerance)
                {
                    count++;
                }
            }
            return count;
        }

        private static int CountBoundedPointMatches(
            List<Vector3> points,
            Vector3 point,
            float toleranceSqr)
        {
            int count = 0;
            for (int index = 0; index < points.Count; index++)
            {
                if ((points[index] - point).sqrMagnitude <= toleranceSqr)
                {
                    count++;
                }
            }
            return count;
        }

        private static bool AreBoundedPointsAdjacent(
            List<Vector3> points,
            Vector3 first,
            Vector3 second,
            float toleranceSqr)
        {
            int firstIndex = -1;
            int secondIndex = -1;
            for (int index = 0; index < points.Count; index++)
            {
                if ((points[index] - first).sqrMagnitude <= toleranceSqr)
                {
                    firstIndex = index;
                }
                if ((points[index] - second).sqrMagnitude <= toleranceSqr)
                {
                    secondIndex = index;
                }
            }
            if (firstIndex < 0 || secondIndex < 0)
            {
                return false;
            }
            int distance = Mathf.Abs(firstIndex - secondIndex);
            return distance == 1 || distance == points.Count - 1;
        }

        private static BoundedOwnerClipFailure
            ClassifyBoundedOwnerClipFailure(string blocker)
        {
            if (string.IsNullOrEmpty(blocker))
            {
                return BoundedOwnerClipFailure.Intersection;
            }
            if (blocker.Contains("insufficient area") ||
                blocker.Contains("fewer than three"))
            {
                return BoundedOwnerClipFailure.Degenerate;
            }
            if (blocker.Contains("analytical plane") ||
                blocker.Contains("non-finite"))
            {
                return BoundedOwnerClipFailure.NonPlanar;
            }
            if (blocker.Contains("simple planar loop"))
            {
                return BoundedOwnerClipFailure.NonSimple;
            }
            if (blocker.Contains("not convex"))
            {
                return BoundedOwnerClipFailure.NonConvex;
            }
            if (blocker.Contains("winding"))
            {
                return BoundedOwnerClipFailure.Winding;
            }
            return BoundedOwnerClipFailure.Intersection;
        }

        private static void RecordBoundedOwnerClipFailure(
            ref BoundedSingleEdgeAuditResult audit,
            BoundedOwnerClipFailure failure)
        {
            switch (failure)
            {
                case BoundedOwnerClipFailure.Degenerate:
                    audit.OwnerDegenerateCount++;
                    break;
                case BoundedOwnerClipFailure.NonPlanar:
                    audit.OwnerNonPlanarCount++;
                    break;
                case BoundedOwnerClipFailure.NonSimple:
                    audit.OwnerNonSimpleCount++;
                    break;
                case BoundedOwnerClipFailure.NonConvex:
                    audit.OwnerNonConvexCount++;
                    break;
                case BoundedOwnerClipFailure.Winding:
                    audit.OwnerWindingFailureCount++;
                    break;
                default:
                    audit.OwnerIntersectionFailureCount++;
                    break;
            }
        }

        private static bool TryPrepareBoundedPreviewFaces(
            List<PolygonFace> sourceFaces,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            ref BoundedSingleEdgeAuditResult audit,
            out List<PolygonFace> auditedFaces,
            out string blocker)
        {
            bool succeeded = TryPrepareBoundedFaces(
                sourceFaces,
                minimumStableEdgeLength,
                minimumStableFaceArea,
                out auditedFaces,
                out BoundedPreparationAudit preparation,
                out blocker);
            audit.ResultPreparation = preparation;
            CopyBoundedPreparationToLegacyAudit(
                preparation,
                ref audit);
            return succeeded;
        }

        private static bool TryPrepareBoundedFaces(
            List<PolygonFace> sourceFaces,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            out List<PolygonFace> auditedFaces,
            out BoundedPreparationAudit audit,
            out string blocker)
        {
            audit = CreateBoundedPreparationAudit();
            audit.Attempted = 1;
            auditedFaces = new List<PolygonFace>(
                sourceFaces == null ? 0 : sourceFaces.Count);
            blocker = string.Empty;

            CaptureBoundedPreparationInput(
                sourceFaces,
                minimumStableEdgeLength,
                minimumStableFaceArea,
                ref audit);
            if (sourceFaces == null || sourceFaces.Count == 0)
            {
                RecordBoundedPreparationFailure(
                    ref audit,
                    "input",
                    -1,
                    null,
                    BoundedPolygonFailure.Degenerate);
                blocker = BuildBoundedPreparationBlocker(
                    audit,
                    "the source face collection is empty");
                return false;
            }

            for (int faceIndex = 0;
                 faceIndex < sourceFaces.Count;
                 faceIndex++)
            {
                PolygonFace sourceFace = sourceFaces[faceIndex];
                List<Vector3> preserved =
                    CopyBoundedPolygonPreservingCollinear(
                        sourceFace == null
                            ? null
                            : sourceFace.Vertices);
                BoundedPolygonFailure failure =
                    BoundedPolygonFailure.None;
                if (sourceFace == null ||
                    !ValidateBoundedPolygon(
                        preserved,
                        sourceFace.Normal,
                        minimumStableFaceArea,
                        requireConvex: true,
                        out blocker,
                        out failure))
                {
                    RecordBoundedPreparationFailure(
                        ref audit,
                        "input",
                        faceIndex,
                        sourceFace,
                        sourceFace == null
                            ? BoundedPolygonFailure.Degenerate
                            : failure);
                    CaptureBoundedPreparationOutput(
                        auditedFaces,
                        minimumStableEdgeLength,
                        minimumStableFaceArea,
                        ref audit);
                    blocker = BuildBoundedPreparationBlocker(
                        audit,
                        blocker);
                    return false;
                }

                auditedFaces.Add(new PolygonFace(
                    preserved,
                    sourceFace.Normal,
                    sourceFace.Feature,
                    sourceFace.FeatureStrength,
                    sourceFace.ProvenanceKind,
                    sourceFace.ProvenanceIndex));
            }

            WeldSharedVertices(auditedFaces);
            audit.Welded = 1;
            audit.ConformedCount = ConformPlaneCutFaceBoundaries(
                auditedFaces,
                minimumStableEdgeLength);
            audit.SeamRepairCount = RepairPlaneCutNumericalSeams(
                auditedFaces,
                minimumStableEdgeLength,
                out int seamTouchedFaceCount);
            audit.SeamTouchedFaceCount = seamTouchedFaceCount;

            for (int faceIndex = 0;
                 faceIndex < auditedFaces.Count;
                 faceIndex++)
            {
                PolygonFace face = auditedFaces[faceIndex];
                if (!ValidateBoundedPolygon(
                        face.Vertices,
                        face.Normal,
                        minimumStableFaceArea,
                        requireConvex: true,
                        out blocker,
                        out BoundedPolygonFailure failure))
                {
                    RecordBoundedPreparationFailure(
                        ref audit,
                        "final",
                        faceIndex,
                        face,
                        failure);
                    CaptureBoundedPreparationOutput(
                        auditedFaces,
                        minimumStableEdgeLength,
                        minimumStableFaceArea,
                        ref audit);
                    blocker = BuildBoundedPreparationBlocker(
                        audit,
                        blocker);
                    return false;
                }
            }

            CaptureBoundedPreparationOutput(
                auditedFaces,
                minimumStableEdgeLength,
                minimumStableFaceArea,
                ref audit);
            audit.Succeeded = auditedFaces.Count >= 4 ? 1 : 0;
            if (audit.Succeeded == 1)
            {
                return true;
            }

            RecordBoundedPreparationFailure(
                ref audit,
                "final",
                -1,
                null,
                BoundedPolygonFailure.Degenerate);
            blocker = BuildBoundedPreparationBlocker(
                audit,
                "the prepared shell has fewer than four faces");
            return false;
        }

        private static BoundedPreparationAudit
            CreateBoundedPreparationAudit()
        {
            return new BoundedPreparationAudit
            {
                FailedFace = -1,
                FailedProvenanceIndex = -1
            };
        }

        private static void CaptureBoundedPreparationInput(
            List<PolygonFace> faces,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            ref BoundedPreparationAudit audit)
        {
            audit.InputFaceCount = faces == null ? 0 : faces.Count;
            audit.InputVertexCount = CountBoundedFaceVertices(faces);
            audit.InputUniqueVertexCount =
                CountBoundedUniqueVertices(faces);
            audit.InputVolume = faces == null
                ? 0.0
                : CalculatePlaneCutPolyhedronVolume(faces);
            CaptureBoundedPreparationTopology(
                faces,
                minimumStableEdgeLength,
                minimumStableFaceArea,
                out audit.InputOpenEdgeCount,
                out audit.InputNonManifoldEdgeCount,
                out audit.InputTJunctionCount,
                out audit.InputInvalidFaceCount);
        }

        private static void CaptureBoundedPreparationOutput(
            List<PolygonFace> faces,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            ref BoundedPreparationAudit audit)
        {
            audit.OutputFaceCount = faces == null ? 0 : faces.Count;
            audit.OutputVertexCount = CountBoundedFaceVertices(faces);
            audit.OutputUniqueVertexCount =
                CountBoundedUniqueVertices(faces);
            audit.OutputVolume = faces == null
                ? 0.0
                : CalculatePlaneCutPolyhedronVolume(faces);
            audit.VolumeDelta = audit.OutputVolume - audit.InputVolume;
            audit.VolumeRatio = audit.InputVolume > 0.000000001
                ? audit.OutputVolume / audit.InputVolume
                : 0.0;
            CaptureBoundedPreparationTopology(
                faces,
                minimumStableEdgeLength,
                minimumStableFaceArea,
                out audit.OutputOpenEdgeCount,
                out audit.OutputNonManifoldEdgeCount,
                out audit.OutputTJunctionCount,
                out audit.OutputInvalidFaceCount);
        }

        private static void CaptureBoundedPreparationTopology(
            List<PolygonFace> faces,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            out int openEdgeCount,
            out int nonManifoldEdgeCount,
            out int tJunctionCount,
            out int invalidFaceCount)
        {
            if (faces == null || faces.Count == 0)
            {
                openEdgeCount = 0;
                nonManifoldEdgeCount = 0;
                tJunctionCount = 0;
                invalidFaceCount = 0;
                return;
            }

            EdgeWearTopologyStats topology = AuditEdgeWearTopology(
                faces,
                minimumStableEdgeLength);
            openEdgeCount = topology.OpenEdgeCount;
            nonManifoldEdgeCount = topology.NonManifoldEdgeCount;
            tJunctionCount = topology.TJunctionCount;
            invalidFaceCount = CountInvalidPlaneCutFaces(
                faces,
                minimumStableFaceArea);
        }

        private static int CountBoundedFaceVertices(
            List<PolygonFace> faces)
        {
            int count = 0;
            if (faces == null)
            {
                return count;
            }

            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                count += face == null || face.Vertices == null
                    ? 0
                    : face.Vertices.Count;
            }
            return count;
        }

        private static int CountBoundedUniqueVertices(
            List<PolygonFace> faces)
        {
            HashSet<VertexKey> keys = new HashSet<VertexKey>();
            if (faces == null)
            {
                return 0;
            }

            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face == null || face.Vertices == null)
                {
                    continue;
                }

                for (int vertexIndex = 0;
                     vertexIndex < face.Vertices.Count;
                     vertexIndex++)
                {
                    Vector3 vertex = face.Vertices[vertexIndex];
                    if (IsFinite(vertex))
                    {
                        keys.Add(new VertexKey(vertex));
                    }
                }
            }
            return keys.Count;
        }

        private static void CopyBoundedPreparationToLegacyAudit(
            BoundedPreparationAudit preparation,
            ref BoundedSingleEdgeAuditResult audit)
        {
            audit.PrepareInputFaceCount = preparation.InputFaceCount;
            audit.PrepareWelded = preparation.Welded;
            audit.PrepareConformedCount = preparation.ConformedCount;
            audit.PrepareSeamRepairCount = preparation.SeamRepairCount;
            audit.PrepareFailedStage = preparation.FailedStage;
            audit.PrepareFailedFace = preparation.FailedFace;
            audit.PrepareFailedKind = preparation.FailedKind;
            audit.PrepareFailedProvenanceKind =
                preparation.FailedProvenanceKind;
            audit.PrepareFailedProvenanceIndex =
                preparation.FailedProvenanceIndex;
            audit.PrepareDegenerateCount = preparation.DegenerateCount;
            audit.PrepareNonPlanarCount = preparation.NonPlanarCount;
            audit.PrepareNonSimpleCount = preparation.NonSimpleCount;
            audit.PrepareNonConvexCount = preparation.NonConvexCount;
            audit.PrepareWindingFailureCount =
                preparation.WindingFailureCount;
        }

        private static bool TryOrientBoundedGeneratedFacesOutward(
            List<PolygonFace> faces,
            Vector3 solidCentre,
            ref BoundedSingleEdgeAuditResult audit,
            out List<PolygonFace> orientedFaces,
            out string blocker)
        {
            orientedFaces = faces == null
                ? null
                : new List<PolygonFace>(faces);
            blocker = string.Empty;
            if (orientedFaces == null || !IsFinite(solidCentre))
            {
                audit.OutwardWindingFailureCount++;
                blocker =
                    "the bounded shell has no stable solid centre for outward winding certification";
                return false;
            }

            for (int faceIndex = 0;
                 faceIndex < orientedFaces.Count;
                 faceIndex++)
            {
                PolygonFace face = orientedFaces[faceIndex];
                if (!IsBoundedGeneratedFace(face))
                {
                    continue;
                }

                if (!TryMeasureBoundedFaceOutwardness(
                        face,
                        solidCentre,
                        out Vector3 measuredNormal,
                        out float outwardDot))
                {
                    audit.OutwardWindingFailureCount++;
                    blocker = BuildBoundedWindingBlocker(
                        faceIndex,
                        face,
                        "the face has no stable outward direction");
                    return false;
                }

                if (outwardDot < 0f)
                {
                    List<Vector3> reversed =
                        new List<Vector3>(face.Vertices);
                    reversed.Reverse();
                    measuredNormal = CalculatePolygonNormal(reversed);
                    orientedFaces[faceIndex] = new PolygonFace(
                        reversed,
                        measuredNormal,
                        face.Feature,
                        face.FeatureStrength,
                        face.ProvenanceKind,
                        face.ProvenanceIndex);
                    audit.FacesReoriented++;
                }
            }

            for (int faceIndex = 0;
                 faceIndex < orientedFaces.Count;
                 faceIndex++)
            {
                PolygonFace face = orientedFaces[faceIndex];
                if (!IsBoundedGeneratedFace(face))
                {
                    continue;
                }

                if (!TryMeasureBoundedFaceOutwardness(
                        face,
                        solidCentre,
                        out Vector3 measuredNormal,
                        out float outwardDot) ||
                    Vector3.Dot(measuredNormal, face.Normal) <= 0f ||
                    outwardDot <= 0f)
                {
                    audit.OutwardWindingFailureCount++;
                    blocker = BuildBoundedWindingBlocker(
                        faceIndex,
                        face,
                        "the face remains inward after certification");
                    return false;
                }
            }

            return true;
        }

        private static bool IsBoundedGeneratedFace(PolygonFace face)
        {
            return face != null &&
                (face.ProvenanceKind ==
                    PolygonFaceProvenanceKind.BoundedEdgeBevel ||
                 face.ProvenanceKind ==
                    PolygonFaceProvenanceKind.BoundedEndpointCap);
        }

        private static bool TryMeasureBoundedFaceOutwardness(
            PolygonFace face,
            Vector3 solidCentre,
            out Vector3 measuredNormal,
            out float outwardDot)
        {
            measuredNormal = Vector3.zero;
            outwardDot = 0f;
            if (face == null || face.Vertices == null ||
                face.Vertices.Count < 3)
            {
                return false;
            }

            measuredNormal = CalculatePolygonNormal(face.Vertices);
            Vector3 faceCentre = CalculateAverage(face.Vertices);
            Vector3 outwardDirection = faceCentre - solidCentre;
            if (!IsFinite(measuredNormal) ||
                measuredNormal.sqrMagnitude <= MinimumEdgeLengthSqr ||
                !IsFinite(faceCentre) ||
                outwardDirection.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return false;
            }

            outwardDot = Vector3.Dot(
                measuredNormal,
                outwardDirection);
            return !float.IsNaN(outwardDot) &&
                !float.IsInfinity(outwardDot);
        }

        private static string BuildBoundedWindingBlocker(
            int faceIndex,
            PolygonFace face,
            string reason)
        {
            return "outward winding certification failed on " +
                (face == null
                    ? PolygonFaceProvenanceKind.None
                    : face.ProvenanceKind) + ":" +
                (face == null ? -1 : face.ProvenanceIndex) +
                " (face " + faceIndex + ") because " + reason;
        }

        private static void RecordBoundedPreparationFailure(
            ref BoundedPreparationAudit audit,
            string stage,
            int faceIndex,
            PolygonFace face,
            BoundedPolygonFailure failure)
        {
            audit.FailedStage = stage;
            audit.FailedFace = faceIndex;
            audit.FailedKind = failure;
            audit.FailedProvenanceKind = face == null
                ? PolygonFaceProvenanceKind.None
                : face.ProvenanceKind;
            audit.FailedProvenanceIndex = face == null
                ? -1
                : face.ProvenanceIndex;
            switch (failure)
            {
                case BoundedPolygonFailure.NonPlanar:
                    audit.NonPlanarCount++;
                    break;
                case BoundedPolygonFailure.NonSimple:
                    audit.NonSimpleCount++;
                    break;
                case BoundedPolygonFailure.NonConvex:
                    audit.NonConvexCount++;
                    break;
                case BoundedPolygonFailure.Winding:
                    audit.WindingFailureCount++;
                    break;
                default:
                    audit.DegenerateCount++;
                    break;
            }
        }

        private static string BuildBoundedPreparationBlocker(
            BoundedPreparationAudit audit,
            string reason)
        {
            string stage = string.IsNullOrEmpty(audit.FailedStage)
                ? "unknown"
                : audit.FailedStage;
            return "prepare/" + stage + " failed on " +
                audit.FailedProvenanceKind + ":" +
                audit.FailedProvenanceIndex +
                " (face " + audit.FailedFace + ") because " +
                (string.IsNullOrEmpty(reason)
                    ? audit.FailedKind.ToString()
                    : reason);
        }

        private static List<Vector3> CopyBoundedPolygonPreservingCollinear(
            List<Vector3> source)
        {
            List<Vector3> result = new List<Vector3>(
                source == null ? 0 : source.Count);
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                if (!IsFinite(source[index]))
                {
                    continue;
                }
                AddBoundedVertex(result, source[index]);
            }
            RemoveClosingDuplicate(result);
            return result;
        }

        private static bool ValidateBoundedPolygon(
            List<Vector3> vertices,
            Vector3 normal,
            float minimumStableFaceArea,
            bool requireConvex,
            out string blocker)
        {
            return ValidateBoundedPolygon(
                vertices,
                normal,
                minimumStableFaceArea,
                requireConvex,
                out blocker,
                out _);
        }

        private static bool ValidateBoundedPolygon(
            List<Vector3> vertices,
            Vector3 normal,
            float minimumStableFaceArea,
            bool requireConvex,
            out string blocker,
            out BoundedPolygonFailure failure)
        {
            blocker = string.Empty;
            failure = BoundedPolygonFailure.None;
            if (vertices == null || vertices.Count < 3)
            {
                blocker = "a bounded polygon has fewer than three vertices";
                failure = BoundedPolygonFailure.Degenerate;
                return false;
            }
            for (int vertexIndex = 0;
                 vertexIndex < vertices.Count;
                 vertexIndex++)
            {
                if (!IsFinite(vertices[vertexIndex]))
                {
                    blocker = "a bounded polygon contains a non-finite vertex";
                    failure = BoundedPolygonFailure.NonFinite;
                    return false;
                }
            }

            float area = CalculatePolygonArea(vertices);
            if (area <= minimumStableFaceArea)
            {
                blocker = "a bounded polygon has insufficient area";
                failure = BoundedPolygonFailure.Degenerate;
                return false;
            }

            Vector3 measuredNormal = CalculatePolygonNormal(vertices);
            if (!IsFinite(measuredNormal) ||
                measuredNormal.sqrMagnitude <= MinimumEdgeLengthSqr ||
                Vector3.Dot(measuredNormal, normal) <= 0f)
            {
                blocker = "a bounded polygon changes winding";
                failure = BoundedPolygonFailure.Winding;
                return false;
            }

            float planeDistance = Vector3.Dot(normal, vertices[0]);
            float planeTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                Mathf.Sqrt(Mathf.Max(minimumStableFaceArea, 0f)) *
                    0.0005f);
            for (int vertexIndex = 1;
                 vertexIndex < vertices.Count;
                 vertexIndex++)
            {
                if (Mathf.Abs(Vector3.Dot(normal, vertices[vertexIndex]) -
                        planeDistance) > planeTolerance)
                {
                    blocker = "a bounded polygon leaves its analytical plane";
                    failure = BoundedPolygonFailure.NonPlanar;
                    return false;
                }
            }

            if (!TryProjectChamferPatchLoop(
                    vertices,
                    normal,
                    out List<Vector2> projected,
                    out float signedArea,
                    out float projectionEpsilon) ||
                Mathf.Abs(signedArea) <=
                    projectionEpsilon * projectionEpsilon ||
                ChamferPatchPolygonSelfIntersects(
                    projected,
                    projectionEpsilon,
                    out _))
            {
                blocker = "a bounded polygon is not a simple planar loop";
                failure = BoundedPolygonFailure.NonSimple;
                return false;
            }

            if (requireConvex)
            {
                List<Vector3> convexityLoop =
                    BuildBoundedConvexityCheckLoop(vertices);
                if (convexityLoop.Count < 3 ||
                    !IsBoundedPolygonConvex(convexityLoop, normal))
                {
                    blocker = "a bounded polygon is not convex";
                    failure = BoundedPolygonFailure.NonConvex;
                    return false;
                }
            }
            return true;
        }

        private static List<Vector3> BuildBoundedConvexityCheckLoop(
            List<Vector3> vertices)
        {
            List<Vector3> simplified =
                CopyBoundedPolygonPreservingCollinear(vertices);
            if (simplified.Count <= 3)
            {
                return simplified;
            }

            float maximumEdgeLength = 0f;
            for (int vertexIndex = 0;
                 vertexIndex < simplified.Count;
                 vertexIndex++)
            {
                maximumEdgeLength = Mathf.Max(
                    maximumEdgeLength,
                    Vector3.Distance(
                        simplified[vertexIndex],
                        simplified[(vertexIndex + 1) %
                            simplified.Count]));
            }
            float collinearTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                maximumEdgeLength * 0.000001f);
            float collinearToleranceSqr =
                collinearTolerance * collinearTolerance;

            int guard = Mathf.Max(8, simplified.Count * 2);
            bool changed = true;
            while (changed && simplified.Count > 3 && guard-- > 0)
            {
                changed = false;
                for (int vertexIndex = 0;
                     vertexIndex < simplified.Count;
                     vertexIndex++)
                {
                    Vector3 previous = simplified[
                        (vertexIndex - 1 + simplified.Count) %
                        simplified.Count];
                    Vector3 current = simplified[vertexIndex];
                    Vector3 next = simplified[
                        (vertexIndex + 1) % simplified.Count];
                    Vector3 span = next - previous;
                    float spanLengthSqr = span.sqrMagnitude;
                    if (spanLengthSqr <= MinimumEdgeLengthSqr)
                    {
                        simplified.RemoveAt(vertexIndex);
                        changed = true;
                        break;
                    }

                    float parameter = Vector3.Dot(
                        current - previous,
                        span) / spanLengthSqr;
                    Vector3 closest = previous + span * parameter;
                    float spanLength = Mathf.Sqrt(spanLengthSqr);
                    float parameterTolerance = Mathf.Min(
                        0.25f,
                        collinearTolerance / spanLength);
                    if (parameter >= -parameterTolerance &&
                        parameter <= 1f + parameterTolerance &&
                        (current - closest).sqrMagnitude <=
                            collinearToleranceSqr)
                    {
                        simplified.RemoveAt(vertexIndex);
                        changed = true;
                        break;
                    }
                }
            }
            return simplified;
        }

        private static bool IsBoundedPolygonConvex(
            List<Vector3> vertices,
            Vector3 normal)
        {
            if (vertices == null || vertices.Count < 3 ||
                !TryProjectChamferPatchLoop(
                    vertices,
                    normal,
                    out List<Vector2> projected,
                    out _,
                    out float epsilon))
            {
                return false;
            }

            float expectedSign = 0f;
            float crossTolerance = epsilon * epsilon;
            for (int vertexIndex = 0;
                 vertexIndex < projected.Count;
                 vertexIndex++)
            {
                Vector2 previous = projected[
                    (vertexIndex - 1 + projected.Count) %
                    projected.Count];
                Vector2 current = projected[vertexIndex];
                Vector2 next = projected[
                    (vertexIndex + 1) % projected.Count];
                float cross = ChamferPatchCross2D(
                    previous,
                    current,
                    next);
                if (Mathf.Abs(cross) <= crossTolerance)
                {
                    continue;
                }

                float sign = Mathf.Sign(cross);
                if (expectedSign == 0f)
                {
                    expectedSign = sign;
                }
                else if (sign != expectedSign)
                {
                    return false;
                }
            }
            return expectedSign != 0f;
        }

        private static void AddBoundedVertex(
            List<Vector3> vertices,
            Vector3 position)
        {
            if (vertices.Count == 0 ||
                (vertices[vertices.Count - 1] - position).sqrMagnitude >
                    PointMergeDistanceSqr)
            {
                vertices.Add(position);
            }
        }

        private static bool TryCreateBoundedFace(
            List<Vector3> vertices,
            Vector3 preferredNormal,
            Vector3 solidCentre,
            PolygonFaceProvenanceKind provenanceKind,
            int provenanceIndex,
            float strength,
            float minimumStableFaceArea,
            out PolygonFace face)
        {
            face = null;
            if (vertices == null || vertices.Count < 3)
            {
                return false;
            }

            Vector3 normal = CalculatePolygonNormal(vertices);
            if (!IsFinite(normal) ||
                normal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return false;
            }

            Vector3 centre = CalculateAverage(vertices);
            bool reverse = preferredNormal.sqrMagnitude >
                    MinimumEdgeLengthSqr
                ? Vector3.Dot(normal, preferredNormal) < 0f
                : Vector3.Dot(normal, centre - solidCentre) < 0f;
            if (reverse)
            {
                vertices.Reverse();
                normal = -normal;
            }

            List<Vector3> sanitized = SanitizePolygon(vertices, normal);
            if (sanitized.Count < 3 ||
                CalculatePolygonArea(sanitized) <= minimumStableFaceArea)
            {
                return false;
            }

            Vector3 measured = CalculatePolygonNormal(sanitized);
            if (Vector3.Dot(measured, normal) <= 0f ||
                !IsBoundedPolygonConvex(sanitized, normal))
            {
                return false;
            }

            face = new PolygonFace(
                sanitized,
                normal,
                PolygonFaceFeature.ConvexEdgeWear,
                strength,
                provenanceKind,
                provenanceIndex);
            return true;
        }

        private static void CountBoundedSingleEdgeFaces(
            List<PolygonFace> faces,
            int sourceEdgeIndex,
            ref BoundedSingleEdgeAuditResult result)
        {
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face.ProvenanceKind ==
                        PolygonFaceProvenanceKind.BoundedEdgeBevel &&
                    face.ProvenanceIndex == sourceEdgeIndex)
                {
                    result.BevelFaceCount++;
                }
                else if (face.ProvenanceKind ==
                    PolygonFaceProvenanceKind.BoundedEndpointCap)
                {
                    result.EndpointCapCount++;
                }
            }
        }

        private static BoundedSourceProvenanceAudit
            CreateBoundedSourceProvenanceAudit()
        {
            return new BoundedSourceProvenanceAudit
            {
                FirstMissingSourceFace = -1,
                FirstDuplicateSourceFace = -1,
                FirstOutOfRangeSourceFace = -1
            };
        }

        private static BoundedSourceProvenanceAudit
            AuditBoundedSourceFaceProvenance(
                List<PolygonFace> faces,
                int expectedSourceFaceCount)
        {
            BoundedSourceProvenanceAudit audit =
                CreateBoundedSourceProvenanceAudit();
            audit.Attempted = 1;
            audit.ExpectedSourceFaceCount =
                Mathf.Max(0, expectedSourceFaceCount);
            audit.TotalFaceCount = faces == null ? 0 : faces.Count;
            bool[] seen = new bool[audit.ExpectedSourceFaceCount];

            if (faces != null)
            {
                for (int faceIndex = 0;
                     faceIndex < faces.Count;
                     faceIndex++)
                {
                    PolygonFace face = faces[faceIndex];
                    if (face == null)
                    {
                        audit.NullFaceCount++;
                        continue;
                    }
                    if (face.ProvenanceKind !=
                        PolygonFaceProvenanceKind.SourceFace)
                    {
                        audit.NonSourceFaceCount++;
                        continue;
                    }

                    audit.SourceProvenanceFaceCount++;
                    int sourceFaceIndex = face.ProvenanceIndex;
                    if (sourceFaceIndex < 0 ||
                        sourceFaceIndex >= audit.ExpectedSourceFaceCount)
                    {
                        audit.OutOfRangeSourceFaceCount++;
                        if (audit.FirstOutOfRangeSourceFace < 0)
                        {
                            audit.FirstOutOfRangeSourceFace =
                                sourceFaceIndex;
                        }
                        continue;
                    }
                    if (seen[sourceFaceIndex])
                    {
                        audit.DuplicateSourceFaceCount++;
                        if (audit.FirstDuplicateSourceFace < 0)
                        {
                            audit.FirstDuplicateSourceFace =
                                sourceFaceIndex;
                        }
                        continue;
                    }

                    seen[sourceFaceIndex] = true;
                    audit.ValidUniqueSourceFaceCount++;
                }
            }

            for (int sourceFaceIndex = 0;
                 sourceFaceIndex < seen.Length;
                 sourceFaceIndex++)
            {
                if (seen[sourceFaceIndex])
                {
                    continue;
                }

                audit.MissingSourceFaceCount++;
                if (audit.FirstMissingSourceFace < 0)
                {
                    audit.FirstMissingSourceFace = sourceFaceIndex;
                }
            }

            audit.Valid =
                audit.ExpectedSourceFaceCount > 0 &&
                audit.ValidUniqueSourceFaceCount ==
                    audit.ExpectedSourceFaceCount &&
                audit.MissingSourceFaceCount == 0 &&
                audit.DuplicateSourceFaceCount == 0 &&
                audit.OutOfRangeSourceFaceCount == 0 &&
                audit.NullFaceCount == 0
                    ? 1
                    : 0;
            return audit;
        }

        private static BoundedSourceFaceChangeAudit
            AuditBoundedSourceFaceChanges(
                List<PolygonFace> baselineFaces,
                List<PolygonFace> auditedFaces,
                ChamferTopologyContext context,
                int sourceEdgeIndex,
                int supportA,
                int additionalSupportA,
                int supportB,
                int additionalSupportB,
                bool classifyExactPlaneSupport,
                Vector3 supportPlaneNormal,
                float supportPlaneDistance,
                ICollection<int> allowedEndpointSupportFaces)
        {
            BoundedSourceFaceChangeAudit audit =
                new BoundedSourceFaceChangeAudit
                {
                    Attempted = 1
                };
            if (baselineFaces == null || auditedFaces == null ||
                context == null || sourceEdgeIndex < 0 ||
                sourceEdgeIndex >= context.Graph.Edges.Count)
            {
                return audit;
            }

            EdgeWearGraphEdge edge = context.Graph.Edges[sourceEdgeIndex];
            int ownerA = context.Graph.Faces[edge.FaceA].SourceFaceIndex;
            int ownerB = context.Graph.Faces[edge.FaceB].SourceFaceIndex;

            for (int sourceFaceIndex = 0;
                 sourceFaceIndex < baselineFaces.Count;
                 sourceFaceIndex++)
            {
                PolygonFace baseline = baselineFaces[sourceFaceIndex];
                if (baseline == null ||
                    baseline.ProvenanceKind !=
                        PolygonFaceProvenanceKind.SourceFace ||
                    baseline.ProvenanceIndex < 0)
                {
                    continue;
                }

                int provenanceIndex = baseline.ProvenanceIndex;
                PolygonFace audited = FindBoundedSourceFace(
                    auditedFaces,
                    provenanceIndex);
                bool boundaryOnlyDifference = false;
                bool equivalent = audited != null &&
                    AreBoundedPolygonsGeometricallyEquivalent(
                        baseline.Vertices,
                        audited.Vertices,
                        baseline.Normal,
                        out boundaryOnlyDifference);
                bool owner = provenanceIndex == ownerA ||
                    provenanceIndex == ownerB;
                bool endpointSupport = provenanceIndex == supportA ||
                    provenanceIndex == additionalSupportA ||
                    provenanceIndex == supportB ||
                    provenanceIndex == additionalSupportB ||
                    (allowedEndpointSupportFaces != null &&
                     allowedEndpointSupportFaces.Contains(
                         provenanceIndex)) ||
                    (classifyExactPlaneSupport &&
                     IsBoundedSourceFaceIntersectedByPlane(
                         baseline,
                         supportPlaneNormal,
                         supportPlaneDistance));
                if (owner)
                {
                    if (!equivalent)
                    {
                        audit.ModifiedSourceFaceCount++;
                        audit.OwnerSourceFaceModifiedCount++;
                    }
                }
                else if (endpointSupport)
                {
                    if (!equivalent || boundaryOnlyDifference)
                    {
                        audit.ModifiedSourceFaceCount++;
                        audit.EndpointSupportSourceFaceModifiedCount++;
                    }
                }
                else if (!equivalent)
                {
                    audit.UnexpectedSourceFaceModifiedCount++;
                    audit.ForeignSourceFaceModifiedCount++;
                }
                else if (boundaryOnlyDifference)
                {
                    audit.BoundaryOnlyUnexpectedSourceFaceCount++;
                    audit.ForeignBoundarySubdividedCount++;
                }
            }
            return audit;
        }

        private static bool IsBoundedSourceFaceIntersectedByPlane(
            PolygonFace face,
            Vector3 planeNormal,
            float planeDistance)
        {
            if (face == null || face.Vertices == null ||
                face.Vertices.Count < 3 || !IsFinite(planeNormal) ||
                planeNormal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return false;
            }
            planeNormal.Normalize();
            float tolerance = PointMergeDistance * 8f;
            bool hasRetained = false;
            bool hasRemoved = false;
            for (int vertexIndex = 0;
                 vertexIndex < face.Vertices.Count;
                 vertexIndex++)
            {
                float distance = Vector3.Dot(
                    planeNormal,
                    face.Vertices[vertexIndex]) - planeDistance;
                hasRetained |= distance <= tolerance;
                hasRemoved |= distance > tolerance;
            }
            return hasRetained && hasRemoved;
        }

        private static void ApplyBoundedSourceFaceChangeAudits(
            BoundedSourceFaceChangeAudit rawAudit,
            BoundedSourceFaceChangeAudit preparedAudit,
            ref BoundedSingleEdgeAuditResult result)
        {
            result.PreparedSourceChangeComparisonAttempted =
                preparedAudit.Attempted;
            result.ModifiedSourceFaceCount =
                preparedAudit.ModifiedSourceFaceCount;
            result.OwnerSourceFaceModifiedCount =
                preparedAudit.OwnerSourceFaceModifiedCount;
            result.EndpointSupportSourceFaceModifiedCount =
                preparedAudit.EndpointSupportSourceFaceModifiedCount;
            result.UnexpectedSourceFaceModifiedCount =
                preparedAudit.UnexpectedSourceFaceModifiedCount;
            result.BoundaryOnlyUnexpectedSourceFaceCount =
                preparedAudit.BoundaryOnlyUnexpectedSourceFaceCount;
            result.ForeignSourceFaceModifiedCount =
                preparedAudit.ForeignSourceFaceModifiedCount;
            result.ForeignBoundarySubdividedCount =
                preparedAudit.ForeignBoundarySubdividedCount;

            result.RawModifiedSourceFaceCount =
                rawAudit.ModifiedSourceFaceCount;
            result.RawOwnerSourceFaceModifiedCount =
                rawAudit.OwnerSourceFaceModifiedCount;
            result.RawEndpointSupportSourceFaceModifiedCount =
                rawAudit.EndpointSupportSourceFaceModifiedCount;
            result.RawUnexpectedSourceFaceModifiedCount =
                rawAudit.UnexpectedSourceFaceModifiedCount;
            result.RawBoundaryOnlyUnexpectedSourceFaceCount =
                rawAudit.BoundaryOnlyUnexpectedSourceFaceCount;
            result.RawForeignSourceFaceModifiedCount =
                rawAudit.ForeignSourceFaceModifiedCount;
            result.RawForeignBoundarySubdividedCount =
                rawAudit.ForeignBoundarySubdividedCount;
        }

        private static PolygonFace FindBoundedSourceFace(
            List<PolygonFace> faces,
            int sourceFaceIndex)
        {
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face.ProvenanceKind ==
                        PolygonFaceProvenanceKind.SourceFace &&
                    face.ProvenanceIndex == sourceFaceIndex)
                {
                    return face;
                }
            }
            return null;
        }

        private static bool AreBoundedPolygonsGeometricallyEquivalent(
            List<Vector3> left,
            List<Vector3> right,
            Vector3 normal,
            out bool boundaryOnlyDifference)
        {
            boundaryOnlyDifference = false;
            List<Vector3> normalizedLeft = SanitizePolygon(
                left,
                normal);
            List<Vector3> normalizedRight = SanitizePolygon(
                right,
                normal);
            if (AreBoundedPolygonsCyclicallyEquivalent(
                    normalizedLeft,
                    normalizedRight))
            {
                return true;
            }

            if (!TryProjectBoundedRegionPair(
                    normalizedLeft,
                    normalizedRight,
                    normal,
                    out List<Vector2> projectedLeft,
                    out List<Vector2> projectedRight,
                    out float pointTolerance,
                    out float areaTolerance))
            {
                return false;
            }

            float leftArea = Mathf.Abs(
                CalculateBoundedSignedArea(projectedLeft));
            float rightArea = Mathf.Abs(
                CalculateBoundedSignedArea(projectedRight));
            if (Mathf.Abs(leftArea - rightArea) > areaTolerance)
            {
                return false;
            }

            for (int index = 0; index < projectedLeft.Count; index++)
            {
                if (!IsBoundedPointInsideOrOnPolygon(
                        projectedLeft[index],
                        projectedRight,
                        pointTolerance))
                {
                    return false;
                }
            }
            for (int index = 0; index < projectedRight.Count; index++)
            {
                if (!IsBoundedPointInsideOrOnPolygon(
                        projectedRight[index],
                        projectedLeft,
                        pointTolerance))
                {
                    return false;
                }
            }

            boundaryOnlyDifference = true;
            return true;
        }

        private static bool TryProjectBoundedRegionPair(
            List<Vector3> left,
            List<Vector3> right,
            Vector3 expectedNormal,
            out List<Vector2> projectedLeft,
            out List<Vector2> projectedRight,
            out float pointTolerance,
            out float areaTolerance)
        {
            projectedLeft = new List<Vector2>();
            projectedRight = new List<Vector2>();
            pointTolerance = 0f;
            areaTolerance = 0f;
            if (left == null || right == null ||
                left.Count < 3 || right.Count < 3 ||
                !IsFinite(expectedNormal) ||
                expectedNormal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return false;
            }

            Vector3 normal = expectedNormal.normalized;
            Vector3 reference;
            float absX = Mathf.Abs(normal.x);
            float absY = Mathf.Abs(normal.y);
            float absZ = Mathf.Abs(normal.z);
            if (absX <= absY && absX <= absZ)
            {
                reference = Vector3.right;
            }
            else if (absY <= absZ)
            {
                reference = Vector3.up;
            }
            else
            {
                reference = Vector3.forward;
            }

            Vector3 tangent = Vector3.Cross(reference, normal);
            if (!IsFinite(tangent) ||
                tangent.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return false;
            }
            tangent.Normalize();
            Vector3 bitangent = Vector3.Cross(normal, tangent);
            if (!IsFinite(bitangent) ||
                bitangent.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return false;
            }
            bitangent.Normalize();

            Vector3 origin = left[0];
            float planeDistance = Vector3.Dot(normal, origin);
            float minimumX = float.PositiveInfinity;
            float minimumY = float.PositiveInfinity;
            float maximumX = float.NegativeInfinity;
            float maximumY = float.NegativeInfinity;
            float planeTolerance = PointMergeDistance * 16f;

            if (!ProjectBoundedRegionVertices(
                    left,
                    origin,
                    normal,
                    planeDistance,
                    planeTolerance,
                    tangent,
                    bitangent,
                    projectedLeft,
                    ref minimumX,
                    ref minimumY,
                    ref maximumX,
                    ref maximumY) ||
                !ProjectBoundedRegionVertices(
                    right,
                    origin,
                    normal,
                    planeDistance,
                    planeTolerance,
                    tangent,
                    bitangent,
                    projectedRight,
                    ref minimumX,
                    ref minimumY,
                    ref maximumX,
                    ref maximumY))
            {
                return false;
            }

            float extent = Mathf.Max(
                maximumX - minimumX,
                maximumY - minimumY);
            if (!IsFiniteFloat(extent) || extent <= 0f)
            {
                return false;
            }

            pointTolerance = Mathf.Max(
                PointMergeDistance * 16f,
                extent * 0.00001f);
            float maximumArea = Mathf.Max(
                Mathf.Abs(CalculateBoundedSignedArea(projectedLeft)),
                Mathf.Abs(CalculateBoundedSignedArea(projectedRight)));
            areaTolerance = Mathf.Max(
                pointTolerance * extent * 4f,
                maximumArea * 0.00005f);
            return true;
        }

        private static bool ProjectBoundedRegionVertices(
            List<Vector3> vertices,
            Vector3 origin,
            Vector3 normal,
            float planeDistance,
            float planeTolerance,
            Vector3 tangent,
            Vector3 bitangent,
            List<Vector2> projected,
            ref float minimumX,
            ref float minimumY,
            ref float maximumX,
            ref float maximumY)
        {
            for (int index = 0; index < vertices.Count; index++)
            {
                Vector3 vertex = vertices[index];
                if (!IsFinite(vertex) ||
                    Mathf.Abs(Vector3.Dot(normal, vertex) -
                        planeDistance) > planeTolerance)
                {
                    return false;
                }

                Vector3 offset = vertex - origin;
                Vector2 point = new Vector2(
                    Vector3.Dot(offset, tangent),
                    Vector3.Dot(offset, bitangent));
                if (!IsFiniteFloat(point.x) ||
                    !IsFiniteFloat(point.y))
                {
                    return false;
                }
                projected.Add(point);
                minimumX = Mathf.Min(minimumX, point.x);
                minimumY = Mathf.Min(minimumY, point.y);
                maximumX = Mathf.Max(maximumX, point.x);
                maximumY = Mathf.Max(maximumY, point.y);
            }
            return true;
        }

        private static float CalculateBoundedSignedArea(
            List<Vector2> polygon)
        {
            float twiceArea = 0f;
            for (int index = 0; index < polygon.Count; index++)
            {
                Vector2 current = polygon[index];
                Vector2 next = polygon[(index + 1) % polygon.Count];
                twiceArea += current.x * next.y - next.x * current.y;
            }
            return twiceArea * 0.5f;
        }

        private static bool IsBoundedPointInsideOrOnPolygon(
            Vector2 point,
            List<Vector2> polygon,
            float tolerance)
        {
            float toleranceSqr = tolerance * tolerance;
            bool inside = false;
            for (int index = 0, previous = polygon.Count - 1;
                 index < polygon.Count;
                 previous = index++)
            {
                Vector2 start = polygon[previous];
                Vector2 end = polygon[index];
                Vector2 segment = end - start;
                float segmentLengthSqr = segment.sqrMagnitude;
                if (segmentLengthSqr > 0f)
                {
                    float parameter = Mathf.Clamp01(
                        Vector2.Dot(point - start, segment) /
                        segmentLengthSqr);
                    Vector2 closest = start + segment * parameter;
                    if ((point - closest).sqrMagnitude <= toleranceSqr)
                    {
                        return true;
                    }
                }

                bool crosses = (start.y > point.y) !=
                    (end.y > point.y);
                if (!crosses)
                {
                    continue;
                }
                float denominator = end.y - start.y;
                if (Mathf.Abs(denominator) <= Mathf.Epsilon)
                {
                    continue;
                }
                float intersectionX = start.x +
                    (point.y - start.y) *
                    (end.x - start.x) / denominator;
                if (point.x < intersectionX)
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        private static bool AreBoundedPolygonsCyclicallyEquivalent(
            List<Vector3> left,
            List<Vector3> right)
        {
            if (left == null || right == null ||
                left.Count != right.Count || left.Count == 0)
            {
                return false;
            }

            VertexKey first = new VertexKey(left[0]);
            for (int start = 0; start < right.Count; start++)
            {
                if (!first.Equals(new VertexKey(right[start])))
                {
                    continue;
                }

                bool forward = true;
                bool reverse = true;
                for (int index = 0; index < left.Count; index++)
                {
                    forward &= new VertexKey(left[index]).Equals(
                        new VertexKey(right[(start + index) % right.Count]));
                    int reverseIndex =
                        (start - index + right.Count) % right.Count;
                    reverse &= new VertexKey(left[index]).Equals(
                        new VertexKey(right[reverseIndex]));
                }
                if (forward || reverse)
                {
                    return true;
                }
            }
            return false;
        }

        private static void AuditBoundedRailFidelity(
            List<PolygonFace> faces,
            int sourceEdgeIndex,
            Vector3 a0,
            Vector3 b0,
            Vector3 a1,
            Vector3 b1,
            ref BoundedSingleEdgeAuditResult result)
        {
            List<PolygonFace> bevelFaces = new List<PolygonFace>();
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face.ProvenanceKind ==
                        PolygonFaceProvenanceKind.BoundedEdgeBevel &&
                    face.ProvenanceIndex == sourceEdgeIndex)
                {
                    bevelFaces.Add(face);
                }
            }
            if (bevelFaces.Count == 0)
            {
                result.RailDeviation = float.PositiveInfinity;
                result.MaximumExtentBeyondRails = float.PositiveInfinity;
                return;
            }

            Vector3[] expected = { a0, b0, b1, a1 };
            float maximumDeviationSqr = 0f;
            for (int expectedIndex = 0;
                 expectedIndex < expected.Length;
                 expectedIndex++)
            {
                float minimumDistanceSqr = float.PositiveInfinity;
                for (int faceIndex = 0;
                     faceIndex < bevelFaces.Count;
                     faceIndex++)
                {
                    PolygonFace bevelFace = bevelFaces[faceIndex];
                    for (int vertexIndex = 0;
                         vertexIndex < bevelFace.Vertices.Count;
                         vertexIndex++)
                    {
                        minimumDistanceSqr = Mathf.Min(
                            minimumDistanceSqr,
                            (bevelFace.Vertices[vertexIndex] -
                             expected[expectedIndex]).sqrMagnitude);
                    }
                }
                maximumDeviationSqr = Mathf.Max(
                    maximumDeviationSqr,
                    minimumDistanceSqr);
            }
            result.RailDeviation = Mathf.Sqrt(maximumDeviationSqr);
            if (result.MultiSupportPlaneCut == 1)
            {
                result.MaximumExtentBeyondRails = 0f;
                return;
            }

            PolygonFace ordinaryBevelFace = bevelFaces[0];
            Vector3 edgeAxis =
                ((b0 + b1) - (a0 + a1)) * 0.5f;
            Vector3 widthAxis =
                ((a1 + b1) - (a0 + b0)) * 0.5f;
            if (edgeAxis.sqrMagnitude <= MinimumEdgeLengthSqr ||
                widthAxis.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                result.MaximumExtentBeyondRails = float.PositiveInfinity;
                return;
            }
            edgeAxis.Normalize();
            widthAxis = Vector3.ProjectOnPlane(widthAxis, edgeAxis);
            if (widthAxis.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                result.MaximumExtentBeyondRails = float.PositiveInfinity;
                return;
            }
            widthAxis.Normalize();

            float minEdge = float.PositiveInfinity;
            float maxEdge = float.NegativeInfinity;
            float minWidth = float.PositiveInfinity;
            float maxWidth = float.NegativeInfinity;
            for (int expectedIndex = 0;
                 expectedIndex < expected.Length;
                 expectedIndex++)
            {
                minEdge = Mathf.Min(
                    minEdge,
                    Vector3.Dot(expected[expectedIndex], edgeAxis));
                maxEdge = Mathf.Max(
                    maxEdge,
                    Vector3.Dot(expected[expectedIndex], edgeAxis));
                minWidth = Mathf.Min(
                    minWidth,
                    Vector3.Dot(expected[expectedIndex], widthAxis));
                maxWidth = Mathf.Max(
                    maxWidth,
                    Vector3.Dot(expected[expectedIndex], widthAxis));
            }

            float maximumExtent = 0f;
            for (int vertexIndex = 0;
                 vertexIndex < ordinaryBevelFace.Vertices.Count;
                 vertexIndex++)
            {
                float edgeValue = Vector3.Dot(
                    ordinaryBevelFace.Vertices[vertexIndex],
                    edgeAxis);
                float widthValue = Vector3.Dot(
                    ordinaryBevelFace.Vertices[vertexIndex],
                    widthAxis);
                maximumExtent = Mathf.Max(
                    maximumExtent,
                    Mathf.Max(
                        Mathf.Max(minEdge - edgeValue, edgeValue - maxEdge),
                        Mathf.Max(
                            minWidth - widthValue,
                            widthValue - maxWidth)));
            }
            result.MaximumExtentBeyondRails = Mathf.Max(0f, maximumExtent);
        }

        private static bool TryTriangulateBoundedPreviewFaces(
            List<PolygonFace> faces,
            float minimumStableFaceArea,
            ref BoundedSingleEdgeAuditResult audit,
            out TriangleSoup soup,
            out string blocker)
        {
            soup = new TriangleSoup();
            blocker = string.Empty;
            float minimumTriangleArea = Mathf.Max(
                TinyFaceAreaEpsilon,
                minimumStableFaceArea * 0.001f);
            Dictionary<int, int> surfaceGroupOwnerById =
                new Dictionary<int, int>();

            audit.PolygonSurfaceFaceCount = 0;
            audit.PolygonSurfaceBoundaryVertexCount = 0;
            audit.PolygonSurfaceExpectedTriangleCount = 0;
            audit.PolygonSurfaceTriangleCount = 0;
            audit.PolygonSurfaceAuthoredNormalTriangleCount = 0;
            audit.PolygonSurfaceAuthoredSurfaceGroupTriangleCount = 0;
            audit.PolygonSurfaceInternalFanVertexCount = 0;
            audit.PolygonSurfaceGroupCollisionCount = 0;
            audit.PolygonSurfaceGroupCollisionSurfaceGroup = -1;
            audit.PolygonSurfaceGroupCollisionFirstFace = -1;
            audit.PolygonSurfaceGroupCollisionSecondFace = -1;
            audit.PolygonSurfaceMaximumPlaneResidual = 0f;
            audit.PolygonSurfaceMaximumNormalDeviationDegrees = 0f;
            audit.PolygonSurfaceRenderValid = 0;
            audit.PolygonSurfaceFailureFace = -1;
            audit.PolygonSurfaceFailureProvenanceIndex = -1;
            audit.PolygonSurfaceFailureReason = string.Empty;

            audit.BevelRegionFaceCount = 0;
            audit.BevelRegionBoundaryVertexCount = 0;
            audit.BevelRegionTriangleCount = 0;
            audit.BevelRegionAuthoredNormalTriangleCount = 0;
            audit.BevelRegionAuthoredSurfaceGroupTriangleCount = 0;
            audit.BevelRegionInternalFanVertexCount = 0;
            audit.BevelRegionMaximumPlaneResidual = 0f;
            audit.BevelRegionMaximumNormalDeviationDegrees = 0f;
            audit.BevelRegionRenderValid = 0;
            audit.BevelRegionFailureFace = -1;
            audit.BevelRegionFailureProvenanceIndex = -1;
            audit.BevelRegionFailureReason = string.Empty;

            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face == null || face.Vertices == null ||
                    face.Vertices.Count < 3)
                {
                    blocker = RecordBoundedTriangulationFailure(
                        ref audit,
                        faceIndex,
                        face,
                        BoundedPolygonFailure.Degenerate,
                        "the face has fewer than three boundary vertices");
                    soup = null;
                    return false;
                }

                List<Vector3> convexityLoop =
                    BuildBoundedConvexityCheckLoop(face.Vertices);
                if (convexityLoop.Count < 3 ||
                    !IsBoundedPolygonConvex(
                        convexityLoop,
                        face.Normal))
                {
                    blocker = RecordBoundedTriangulationFailure(
                        ref audit,
                        faceIndex,
                        face,
                        BoundedPolygonFailure.NonConvex,
                        "the subdivision-safe planar region is not convex");
                    soup = null;
                    return false;
                }

                if (!TryTriangulateBoundedOneSurfaceFace(
                        face,
                        faceIndex,
                        minimumTriangleArea,
                        surfaceGroupOwnerById,
                        ref audit,
                        soup,
                        out blocker))
                {
                    soup = null;
                    return false;
                }
                audit.TriangulatedFaceCount++;
            }

            audit.PolygonSurfaceRenderValid =
                audit.PolygonSurfaceFaceCount > 0 &&
                audit.PolygonSurfaceTriangleCount > 0 &&
                audit.PolygonSurfaceTriangleCount ==
                    audit.PolygonSurfaceExpectedTriangleCount &&
                audit.PolygonSurfaceTriangleCount ==
                    audit.PolygonSurfaceAuthoredNormalTriangleCount &&
                audit.PolygonSurfaceTriangleCount ==
                    audit.PolygonSurfaceAuthoredSurfaceGroupTriangleCount &&
                audit.PolygonSurfaceInternalFanVertexCount == 0 &&
                audit.PolygonSurfaceGroupCollisionCount == 0 &&
                audit.PolygonSurfaceFailureFace < 0
                    ? 1
                    : 0;
            if (audit.PolygonSurfaceRenderValid != 1)
            {
                blocker =
                    "the bounded shell did not render each polygon as one authored surface";
                soup = null;
                return false;
            }

            audit.BevelRegionRenderValid =
                audit.BevelRegionFaceCount > 0 &&
                audit.BevelRegionTriangleCount > 0 &&
                audit.BevelRegionTriangleCount ==
                    audit.BevelRegionAuthoredNormalTriangleCount &&
                audit.BevelRegionTriangleCount ==
                    audit.BevelRegionAuthoredSurfaceGroupTriangleCount &&
                audit.BevelRegionInternalFanVertexCount == 0 &&
                audit.BevelRegionFailureFace < 0
                    ? 1
                    : 0;
            if (audit.BevelRegionFaceCount > 0 &&
                audit.BevelRegionRenderValid != 1)
            {
                blocker =
                    "the bounded bevel region did not render as one planar authored-normal surface";
                soup = null;
                return false;
            }

            if (soup.Positions.Count >= 3)
            {
                return true;
            }

            blocker = RecordBoundedTriangulationFailure(
                ref audit,
                -1,
                null,
                BoundedPolygonFailure.Degenerate,
                "the bounded shell emitted no triangles");
            soup = null;
            return false;
        }

        private static bool TryTriangulateBoundedOneSurfaceFace(
            PolygonFace face,
            int faceIndex,
            float minimumTriangleArea,
            Dictionary<int, int> surfaceGroupOwnerById,
            ref BoundedSingleEdgeAuditResult audit,
            TriangleSoup soup,
            out string blocker)
        {
            blocker = string.Empty;
            bool isBevelFace = IsOneSurfaceBevelFace(face);
            audit.PolygonSurfaceFaceCount++;
            audit.PolygonSurfaceBoundaryVertexCount +=
                face.Vertices.Count;
            if (isBevelFace)
            {
                audit.BevelRegionFaceCount++;
                audit.BevelRegionBoundaryVertexCount +=
                    face.Vertices.Count;
            }

            if (!TryResolveOneSurfaceAuthoredNormal(
                    face,
                    isBevelFace,
                    out Vector3 authoredNormal))
            {
                blocker = RecordBoundedTriangulationFailure(
                    ref audit,
                    faceIndex,
                    face,
                    BoundedPolygonFailure.NonFinite,
                    "the polygon has no finite ordered-boundary render normal");
                return false;
            }

            float planeDistance = 0f;
            for (int vertexIndex = 0;
                 vertexIndex < face.Vertices.Count;
                 vertexIndex++)
            {
                planeDistance += Vector3.Dot(
                    authoredNormal,
                    face.Vertices[vertexIndex]);
            }
            planeDistance /= face.Vertices.Count;

            float maximumPlaneResidual = 0f;
            for (int vertexIndex = 0;
                 vertexIndex < face.Vertices.Count;
                 vertexIndex++)
            {
                maximumPlaneResidual = Mathf.Max(
                    maximumPlaneResidual,
                    Mathf.Abs(
                        Vector3.Dot(
                            authoredNormal,
                            face.Vertices[vertexIndex]) -
                        planeDistance));
            }
            audit.PolygonSurfaceMaximumPlaneResidual = Mathf.Max(
                audit.PolygonSurfaceMaximumPlaneResidual,
                maximumPlaneResidual);
            if (isBevelFace)
            {
                audit.BevelRegionMaximumPlaneResidual = Mathf.Max(
                    audit.BevelRegionMaximumPlaneResidual,
                    maximumPlaneResidual);
            }
            float planeTolerance = PointMergeDistance * 8f;
            if (maximumPlaneResidual > planeTolerance)
            {
                blocker = RecordBoundedTriangulationFailure(
                    ref audit,
                    faceIndex,
                    face,
                    BoundedPolygonFailure.NonPlanar,
                    "the complete polygon boundary is not on one authoritative plane");
                return false;
            }

            int authoredSurfaceGroup = ResolvePolygonSurfaceGroup(
                face,
                faceIndex);
            if (authoredSurfaceGroup < 0)
            {
                blocker = RecordBoundedTriangulationFailure(
                    ref audit,
                    faceIndex,
                    face,
                    BoundedPolygonFailure.NonFinite,
                    "the polygon has no valid authored surface-group identity");
                return false;
            }
            if (surfaceGroupOwnerById.TryGetValue(
                    authoredSurfaceGroup,
                    out int previousOwnerFace) &&
                previousOwnerFace != faceIndex)
            {
                audit.PolygonSurfaceGroupCollisionCount++;
                audit.PolygonSurfaceGroupCollisionSurfaceGroup =
                    authoredSurfaceGroup;
                audit.PolygonSurfaceGroupCollisionFirstFace =
                    previousOwnerFace;
                audit.PolygonSurfaceGroupCollisionSecondFace =
                    faceIndex;
                blocker = RecordBoundedTriangulationFailure(
                    ref audit,
                    faceIndex,
                    face,
                    BoundedPolygonFailure.Degenerate,
                    "the polygon authored surface-group collides with face " +
                        previousOwnerFace);
                return false;
            }
            surfaceGroupOwnerById[authoredSurfaceGroup] = faceIndex;

            PolygonSurfaceTriangulationMode triangulationMode =
                PolygonSurfaceTriangulationMode.None;
            int anchorIndex = -1;
            OneSurfaceBoundaryFanAudit boundaryFanAudit;
            List<OneSurfaceTriangleIndex> generalTriangles = null;
            OneSurfaceGeneralTriangulationAudit generalAudit = default;
            OneSurfaceCollinearReinsertionAudit collinearAudit = default;
            bool hasStableFan = TryFindStableOneSurfaceFanAnchor(
                face.Vertices,
                authoredNormal,
                minimumTriangleArea,
                out anchorIndex,
                out boundaryFanAudit);
            List<OneSurfaceTriangleIndex> fanTriangles = hasStableFan
                ? BuildOneSurfaceFanTriangles(
                    face.Vertices.Count,
                    anchorIndex)
                : null;
            bool hasGeneral = TryResolveGeneralOneSurfaceTriangulation(
                face.Vertices,
                authoredNormal,
                minimumTriangleArea,
                out generalTriangles,
                out generalAudit);

            if (hasStableFan && hasGeneral)
            {
                OneSurfaceTriangulationQuality fanQuality =
                    EvaluateOneSurfaceTriangulationQuality(
                        face.Vertices,
                        authoredNormal,
                        minimumTriangleArea,
                        fanTriangles);
                OneSurfaceTriangulationQuality generalQuality =
                    EvaluateOneSurfaceTriangulationQuality(
                        face.Vertices,
                        authoredNormal,
                        minimumTriangleArea,
                        generalTriangles);
                if (IsOneSurfaceTriangulationQualityBetter(
                        generalQuality,
                        fanQuality))
                {
                    triangulationMode =
                        PolygonSurfaceTriangulationMode.GeneralTriangulation;
                }
                else
                {
                    triangulationMode =
                        PolygonSurfaceTriangulationMode.BoundaryFan;
                }
            }
            else if (hasStableFan)
            {
                triangulationMode =
                    PolygonSurfaceTriangulationMode.BoundaryFan;
            }
            else if (hasGeneral)
            {
                triangulationMode =
                    PolygonSurfaceTriangulationMode.GeneralTriangulation;
            }
            else if (HasOneSurfaceNormalAgreementFailure(
                         boundaryFanAudit,
                         generalAudit) &&
                TryResolveToleranceCollinearOneSurfaceTriangulation(
                    face.Vertices,
                    authoredNormal,
                    planeDistance,
                    planeTolerance,
                    minimumTriangleArea,
                    out generalTriangles,
                    out collinearAudit))
            {
                triangulationMode =
                    PolygonSurfaceTriangulationMode.CollinearReinsertion;
            }

            if (triangulationMode ==
                PolygonSurfaceTriangulationMode.None)
            {
                string failureEvidence =
                    FormatOneSurfaceTriangulationFailureEvidence(
                        face,
                        faceIndex,
                        authoredNormal,
                        maximumPlaneResidual,
                        minimumTriangleArea,
                        boundaryFanAudit,
                        generalAudit,
                        collinearAudit);
                blocker = RecordBoundedTriangulationFailure(
                    ref audit,
                    faceIndex,
                    face,
                    BoundedPolygonFailure.Degenerate,
                    failureEvidence);
                return false;
            }

            int expectedTriangleCount = face.Vertices.Count - 2;
            audit.PolygonSurfaceExpectedTriangleCount +=
                expectedTriangleCount;

            int emittedTriangleCount = 0;
            if (triangulationMode ==
                PolygonSurfaceTriangulationMode.BoundaryFan)
            {
                for (int triangleIndex = 0;
                     triangleIndex < fanTriangles.Count;
                     triangleIndex++)
                {
                    OneSurfaceTriangleIndex triangle =
                        fanTriangles[triangleIndex];
                    if (!TryEmitOneSurfaceTriangle(
                            face.Vertices[triangle.A],
                            face.Vertices[triangle.B],
                            face.Vertices[triangle.C],
                            face,
                            faceIndex,
                            authoredNormal,
                            authoredSurfaceGroup,
                            minimumTriangleArea,
                            isBevelFace,
                            ref audit,
                            soup,
                            out blocker))
                    {
                        return false;
                    }
                    emittedTriangleCount++;
                }
            }
            else
            {
                for (int triangleIndex = 0;
                     triangleIndex < generalTriangles.Count;
                     triangleIndex++)
                {
                    OneSurfaceTriangleIndex triangle =
                        generalTriangles[triangleIndex];
                    if (!TryEmitOneSurfaceTriangle(
                            face.Vertices[triangle.A],
                            face.Vertices[triangle.B],
                            face.Vertices[triangle.C],
                            face,
                            faceIndex,
                            authoredNormal,
                            authoredSurfaceGroup,
                            minimumTriangleArea,
                            isBevelFace,
                            ref audit,
                            soup,
                            out blocker))
                    {
                        return false;
                    }
                    emittedTriangleCount++;
                }
            }

            if (emittedTriangleCount != expectedTriangleCount)
            {
                blocker = RecordBoundedTriangulationFailure(
                    ref audit,
                    faceIndex,
                    face,
                    BoundedPolygonFailure.Degenerate,
                    "the one-surface polygon emitted an unexpected triangle count for its selected triangulation mode");
                return false;
            }

            return true;
        }

        private static List<OneSurfaceTriangleIndex>
            BuildOneSurfaceFanTriangles(
                int vertexCount,
                int anchorIndex)
        {
            List<OneSurfaceTriangleIndex> triangles =
                new List<OneSurfaceTriangleIndex>(
                    Mathf.Max(0, vertexCount - 2));
            for (int offset = 1; offset < vertexCount - 1; offset++)
            {
                triangles.Add(new OneSurfaceTriangleIndex(
                    anchorIndex,
                    (anchorIndex + offset) % vertexCount,
                    (anchorIndex + offset + 1) % vertexCount));
            }
            return triangles;
        }

        private static OneSurfaceTriangulationQuality
            EvaluateOneSurfaceTriangulationQuality(
                List<Vector3> vertices,
                Vector3 authoredNormal,
                float minimumTriangleArea,
                List<OneSurfaceTriangleIndex> triangles)
        {
            if (vertices == null || triangles == null ||
                triangles.Count != vertices.Count - 2)
            {
                return new OneSurfaceTriangulationQuality(
                    false, 0f, 0f, float.PositiveInfinity, 0f);
            }

            float minimumArea = float.PositiveInfinity;
            float minimumNormalDot = 1f;
            float maximumAspectRatio = 0f;
            float minimumAngleDegrees = 180f;
            float polygonScale = ResolveOneSurfacePolygonScale(vertices);
            float finalCoincidenceTolerance = Mathf.Max(
                0.000001f,
                polygonScale * 0.000001f);
            float finalCoincidenceToleranceSqr =
                finalCoincidenceTolerance * finalCoincidenceTolerance;
            for (int triangleIndex = 0;
                 triangleIndex < triangles.Count;
                 triangleIndex++)
            {
                OneSurfaceTriangleIndex triangle = triangles[triangleIndex];
                if (triangle.A < 0 || triangle.A >= vertices.Count ||
                    triangle.B < 0 || triangle.B >= vertices.Count ||
                    triangle.C < 0 || triangle.C >= vertices.Count ||
                    triangle.A == triangle.B ||
                    triangle.B == triangle.C ||
                    triangle.C == triangle.A)
                {
                    return new OneSurfaceTriangulationQuality(
                        false, 0f, 0f, float.PositiveInfinity, 0f);
                }

                Vector3 a = vertices[triangle.A];
                Vector3 b = vertices[triangle.B];
                Vector3 c = vertices[triangle.C];
                if ((a - b).sqrMagnitude <= finalCoincidenceToleranceSqr ||
                    (b - c).sqrMagnitude <= finalCoincidenceToleranceSqr ||
                    (c - a).sqrMagnitude <= finalCoincidenceToleranceSqr ||
                    !PassesOneSurfaceFinalEmissionAreaGate(
                        a,
                        b,
                        c,
                        minimumTriangleArea,
                        polygonScale))
                {
                    return new OneSurfaceTriangulationQuality(
                        false, 0f, 0f, float.PositiveInfinity, 0f);
                }

                OneSurfaceTriangleCandidate candidate =
                    EvaluateOneSurfaceTriangleCandidate(
                        a,
                        b,
                        c,
                        authoredNormal,
                        minimumTriangleArea);
                if (!candidate.Valid)
                {
                    return new OneSurfaceTriangulationQuality(
                        false, 0f, 0f, float.PositiveInfinity, 0f);
                }

                minimumArea = Mathf.Min(minimumArea, candidate.Area);
                minimumNormalDot = Mathf.Min(
                    minimumNormalDot,
                    candidate.NormalDot);
                maximumAspectRatio = Mathf.Max(
                    maximumAspectRatio,
                    candidate.AspectRatio);
                minimumAngleDegrees = Mathf.Min(
                    minimumAngleDegrees,
                    candidate.MinimumAngleDegrees);
            }

            return new OneSurfaceTriangulationQuality(
                true,
                minimumArea,
                minimumNormalDot,
                maximumAspectRatio,
                minimumAngleDegrees);
        }

        private static bool IsOneSurfaceTriangulationQualityBetter(
            OneSurfaceTriangulationQuality candidate,
            OneSurfaceTriangulationQuality current)
        {
            if (!candidate.Valid)
            {
                return false;
            }
            if (!current.Valid)
            {
                return true;
            }

            const float epsilon = 0.000001f;
            if (candidate.MinimumNormalDot >
                current.MinimumNormalDot + epsilon)
            {
                return true;
            }
            if (Mathf.Abs(
                    candidate.MinimumNormalDot -
                    current.MinimumNormalDot) > epsilon)
            {
                return false;
            }
            if (candidate.MaximumAspectRatio <
                current.MaximumAspectRatio - epsilon)
            {
                return true;
            }
            if (Mathf.Abs(
                    candidate.MaximumAspectRatio -
                    current.MaximumAspectRatio) > epsilon)
            {
                return false;
            }
            if (candidate.MinimumAngleDegrees >
                current.MinimumAngleDegrees + epsilon)
            {
                return true;
            }
            if (Mathf.Abs(
                    candidate.MinimumAngleDegrees -
                    current.MinimumAngleDegrees) > epsilon)
            {
                return false;
            }
            if (candidate.MinimumArea > current.MinimumArea + epsilon)
            {
                return true;
            }
            if (Mathf.Abs(
                    candidate.MinimumArea - current.MinimumArea) > epsilon)
            {
                return false;
            }
            return false;
        }

        private static float ResolveOneSurfacePolygonScale(
            List<Vector3> vertices)
        {
            if (vertices == null || vertices.Count == 0)
            {
                return 0f;
            }

            Vector3 minimum = vertices[0];
            Vector3 maximum = vertices[0];
            for (int index = 1; index < vertices.Count; index++)
            {
                minimum = Vector3.Min(minimum, vertices[index]);
                maximum = Vector3.Max(maximum, vertices[index]);
            }
            return (maximum - minimum).magnitude;
        }

        private static bool PassesOneSurfaceFinalEmissionAreaGate(
            Vector3 a,
            Vector3 b,
            Vector3 c,
            float minimumTriangleArea,
            float polygonScale)
        {
            double abx = (double)b.x - a.x;
            double aby = (double)b.y - a.y;
            double abz = (double)b.z - a.z;
            double acx = (double)c.x - a.x;
            double acy = (double)c.y - a.y;
            double acz = (double)c.z - a.z;
            double crossX = aby * acz - abz * acy;
            double crossY = abz * acx - abx * acz;
            double crossZ = abx * acy - aby * acx;
            double crossLengthSquared =
                crossX * crossX + crossY * crossY + crossZ * crossZ;
            if (double.IsNaN(crossLengthSquared) ||
                double.IsInfinity(crossLengthSquared))
            {
                return false;
            }

            double area = 0.5d * Math.Sqrt(crossLengthSquared);
            double scaleAreaFloor =
                Math.Max(1e-12d, (double)polygonScale * polygonScale * 1e-10d);
            double requiredArea = Math.Max(minimumTriangleArea, scaleAreaFloor);
            return area > requiredArea;
        }

        private static bool TryResolveGeneralOneSurfaceTriangulation(
            List<Vector3> vertices,
            Vector3 authoredNormal,
            float minimumTriangleArea,
            out List<OneSurfaceTriangleIndex> triangles,
            out OneSurfaceGeneralTriangulationAudit audit)
        {
            triangles = new List<OneSurfaceTriangleIndex>();
            audit = new OneSurfaceGeneralTriangulationAudit
            {
                SelectedMinimumArea = 0f,
                SelectedMinimumNormalDot = 0f,
                SelectedMaximumAspectRatio = float.PositiveInfinity,
                SelectedMinimumAngleDegrees = 0f,
                SelectedTriangleEvidence = "none",
                FailureReason = string.Empty
            };
            if (vertices == null || vertices.Count < 3 ||
                !IsFinite(authoredNormal) ||
                authoredNormal.sqrMagnitude <= MinimumEdgeLengthSqr ||
                !IsFiniteFloat(minimumTriangleArea) ||
                minimumTriangleArea < 0f)
            {
                audit.FailureReason =
                    "general triangulation received invalid polygon inputs";
                return false;
            }

            if (!TryProjectChamferPatchLoop(
                    vertices,
                    authoredNormal,
                    out List<Vector2> projected,
                    out float signedArea,
                    out float projectionEpsilon))
            {
                audit.FailureReason =
                    "the ordered polygon could not be projected to its authoritative plane";
                return false;
            }
            audit.ProjectionSucceeded = 1;
            float projectedAreaTolerance =
                projectionEpsilon * projectionEpsilon;
            if (!IsFiniteFloat(signedArea) ||
                Mathf.Abs(signedArea) <= projectedAreaTolerance)
            {
                audit.FailureReason =
                    "the projected polygon has no stable signed area";
                return false;
            }
            if (ChamferPatchPolygonSelfIntersects(
                    projected,
                    projectionEpsilon,
                    out _))
            {
                audit.ProjectionSelfIntersection = 1;
                audit.FailureReason =
                    "the projected polygon self-intersects";
                return false;
            }

            int vertexCount = vertices.Count;
            List<int> fullBoundary = new List<int>(vertexCount);
            for (int vertexIndex = 0;
                 vertexIndex < vertexCount;
                 vertexIndex++)
            {
                fullBoundary.Add(vertexIndex);
            }

            bool[,] diagonalValid = new bool[vertexCount, vertexCount];
            for (int first = 0; first < vertexCount; first++)
            {
                diagonalValid[first, first] = true;
                for (int second = first + 1;
                     second < vertexCount;
                     second++)
                {
                    bool boundarySegment = second == first + 1 ||
                        (first == 0 && second == vertexCount - 1);
                    bool valid = boundarySegment ||
                        IsOneSurfaceProjectedDiagonalValid(
                            first,
                            second,
                            fullBoundary,
                            projected,
                            projectionEpsilon);
                    diagonalValid[first, second] = valid;
                    diagonalValid[second, first] = valid;
                    if (!valid)
                    {
                        audit.RejectedDiagonalCount++;
                    }
                }
            }

            OneSurfaceTriangulationState[,] states =
                new OneSurfaceTriangulationState[
                    vertexCount,
                    vertexCount];
            for (int index = 0;
                 index < vertexCount - 1;
                 index++)
            {
                states[index, index + 1] =
                    new OneSurfaceTriangulationState
                    {
                        Evaluated = true,
                        Succeeded = true,
                        MinimumArea = float.PositiveInfinity,
                        MinimumNormalDot = 1f,
                        MaximumAspectRatio = 0f,
                        MinimumAngleDegrees = 180f,
                        SplitIndex = -1
                    };
            }

            const float rankingEpsilon = 0.000001f;
            for (int span = 2;
                 span < vertexCount;
                 span++)
            {
                for (int first = 0;
                     first + span < vertexCount;
                     first++)
                {
                    int last = first + span;
                    audit.StatesEvaluated++;
                    OneSurfaceTriangulationState best =
                        new OneSurfaceTriangulationState
                        {
                            Evaluated = true,
                            Succeeded = false,
                            MinimumArea = -1f,
                            MinimumNormalDot = -1f,
                            MaximumAspectRatio = float.PositiveInfinity,
                            MinimumAngleDegrees = -1f,
                            SplitIndex = -1
                        };
                    if (!diagonalValid[first, last])
                    {
                        states[first, last] = best;
                        continue;
                    }

                    for (int middle = first + 1;
                         middle < last;
                         middle++)
                    {
                        OneSurfaceTriangulationState left =
                            states[first, middle];
                        OneSurfaceTriangulationState right =
                            states[middle, last];
                        if (!left.Succeeded || !right.Succeeded ||
                            !diagonalValid[first, middle] ||
                            !diagonalValid[middle, last])
                        {
                            continue;
                        }

                        OneSurfaceTriangleCandidate candidate =
                            EvaluateOneSurfaceTriangleCandidate(
                                vertices[first],
                                vertices[middle],
                                vertices[last],
                                authoredNormal,
                                minimumTriangleArea);
                        if (candidate.Valid)
                        {
                            audit.ValidTriangleCandidateCount++;
                        }
                        else if (candidate.Failure ==
                            OneSurfaceTriangleCandidateFailure.NonFinite)
                        {
                            audit.RejectedNonFiniteCount++;
                            continue;
                        }
                        else if (candidate.Failure ==
                            OneSurfaceTriangleCandidateFailure.Area)
                        {
                            audit.RejectedAreaCount++;
                            continue;
                        }
                        else
                        {
                            audit.RejectedNormalAgreementCount++;
                            continue;
                        }

                        float minimumArea = Mathf.Min(
                            candidate.Area,
                            Mathf.Min(
                                left.MinimumArea,
                                right.MinimumArea));
                        float minimumNormalDot = Mathf.Min(
                            candidate.NormalDot,
                            Mathf.Min(
                                left.MinimumNormalDot,
                                right.MinimumNormalDot));
                        float maximumAspectRatio = Mathf.Max(
                            candidate.AspectRatio,
                            Mathf.Max(
                                left.MaximumAspectRatio,
                                right.MaximumAspectRatio));
                        float minimumAngleDegrees = Mathf.Min(
                            candidate.MinimumAngleDegrees,
                            Mathf.Min(
                                left.MinimumAngleDegrees,
                                right.MinimumAngleDegrees));
                        bool betterAspect =
                            maximumAspectRatio <
                                best.MaximumAspectRatio - rankingEpsilon;
                        bool equalAspect = Mathf.Abs(
                            maximumAspectRatio -
                            best.MaximumAspectRatio) <= rankingEpsilon;
                        bool betterAngle =
                            minimumAngleDegrees >
                                best.MinimumAngleDegrees + rankingEpsilon;
                        bool equalAngle = Mathf.Abs(
                            minimumAngleDegrees -
                            best.MinimumAngleDegrees) <= rankingEpsilon;
                        bool betterArea =
                            minimumArea >
                                best.MinimumArea + rankingEpsilon;
                        bool equalArea = Mathf.Abs(
                            minimumArea - best.MinimumArea) <=
                                rankingEpsilon;
                        bool betterNormal =
                            minimumNormalDot >
                                best.MinimumNormalDot + rankingEpsilon;
                        bool equalNormal = Mathf.Abs(
                            minimumNormalDot -
                            best.MinimumNormalDot) <= rankingEpsilon;
                        bool betterSplit = best.SplitIndex < 0 ||
                            middle < best.SplitIndex;
                        if (!best.Succeeded || betterAspect ||
                            (equalAspect && betterAngle) ||
                            (equalAspect && equalAngle && betterArea) ||
                            (equalAspect && equalAngle && equalArea &&
                                betterNormal) ||
                            (equalAspect && equalAngle && equalArea &&
                                equalNormal && betterSplit))
                        {
                            best.Succeeded = true;
                            best.MinimumArea = minimumArea;
                            best.MinimumNormalDot = minimumNormalDot;
                            best.MaximumAspectRatio = maximumAspectRatio;
                            best.MinimumAngleDegrees = minimumAngleDegrees;
                            best.SplitIndex = middle;
                        }
                    }
                    states[first, last] = best;
                }
            }

            OneSurfaceTriangulationState complete =
                states[0, vertexCount - 1];
            if (!complete.Succeeded)
            {
                audit.FailureReason =
                    "the projected interval solver found no complete certified triangulation";
                return false;
            }

            if (!CollectOneSurfaceTriangulationTriangles(
                    states,
                    0,
                    vertexCount - 1,
                    triangles) ||
                triangles.Count != vertexCount - 2)
            {
                triangles.Clear();
                audit.FailureReason =
                    "the projected interval solver could not reconstruct exactly n-2 triangles";
                return false;
            }

            audit.CompleteSolutionFound = 1;
            audit.SelectedMinimumArea = complete.MinimumArea;
            audit.SelectedMinimumNormalDot =
                complete.MinimumNormalDot;
            audit.SelectedMaximumAspectRatio =
                complete.MaximumAspectRatio;
            audit.SelectedMinimumAngleDegrees =
                complete.MinimumAngleDegrees;
            StringBuilder selectedEvidence = new StringBuilder();
            for (int triangleIndex = 0;
                 triangleIndex < triangles.Count;
                 triangleIndex++)
            {
                if (triangleIndex > 0)
                {
                    selectedEvidence.Append('|');
                }
                OneSurfaceTriangleIndex triangle =
                    triangles[triangleIndex];
                selectedEvidence.Append(triangle.A);
                selectedEvidence.Append('/');
                selectedEvidence.Append(triangle.B);
                selectedEvidence.Append('/');
                selectedEvidence.Append(triangle.C);
            }
            audit.SelectedTriangleEvidence =
                selectedEvidence.Length > 0
                    ? selectedEvidence.ToString()
                    : "none";
            audit.FailureReason = string.Empty;
            return true;
        }

        private static bool HasOneSurfaceNormalAgreementFailure(
            OneSurfaceBoundaryFanAudit boundaryFanAudit,
            OneSurfaceGeneralTriangulationAudit generalAudit)
        {
            return boundaryFanAudit.Rejection ==
                    OneSurfaceTriangleCandidateFailure.NormalAgreement ||
                generalAudit.RejectedNormalAgreementCount > 0;
        }

        private static bool TryResolveToleranceCollinearOneSurfaceTriangulation(
            List<Vector3> vertices,
            Vector3 authoredNormal,
            float planeDistance,
            float planeTolerance,
            float minimumTriangleArea,
            out List<OneSurfaceTriangleIndex> triangles,
            out OneSurfaceCollinearReinsertionAudit audit)
        {
            triangles = new List<OneSurfaceTriangleIndex>();
            audit = new OneSurfaceCollinearReinsertionAudit
            {
                Attempted = 1,
                BoundaryVertexEvidence = "none",
                CandidateEvidence = "none",
                RemovedIndexEvidence = "none",
                RetainedIndexEvidence = "none",
                SimplifiedTriangleEvidence = "none",
                ReinsertionEvidence = "none",
                FailureReason = string.Empty
            };
            if (vertices == null || vertices.Count < 4 ||
                !IsFinite(authoredNormal) ||
                authoredNormal.sqrMagnitude <= MinimumEdgeLengthSqr ||
                !IsFiniteFloat(planeDistance) ||
                !IsFiniteFloat(planeTolerance) || planeTolerance < 0f ||
                !IsFiniteFloat(minimumTriangleArea) ||
                minimumTriangleArea < 0f)
            {
                audit.FailureReason =
                    "collinear reinsertion received invalid polygon inputs";
                return false;
            }

            if (!TryProjectChamferPatchLoop(
                    vertices,
                    authoredNormal,
                    out List<Vector2> projected,
                    out float signedArea,
                    out float projectionEpsilon))
            {
                audit.FailureReason =
                    "the retained boundary could not be projected for collinear reinsertion";
                return false;
            }
            audit.ProjectionSucceeded = 1;
            audit.SignedAreaBefore = signedArea;
            audit.SignedAreaAfter = signedArea;
            audit.BoundaryVertexEvidence =
                FormatOneSurfaceBoundaryVertexEvidence(
                    vertices,
                    projected,
                    authoredNormal,
                    planeDistance);
            if (ChamferPatchPolygonSelfIntersects(
                    projected,
                    projectionEpsilon,
                    out _))
            {
                audit.ProjectionSelfIntersection = 1;
                audit.FailureReason =
                    "the retained projected boundary self-intersects";
                return false;
            }

            List<int> workingIndices = new List<int>(vertices.Count);
            for (int vertexIndex = 0;
                 vertexIndex < vertices.Count;
                 vertexIndex++)
            {
                workingIndices.Add(vertexIndex);
            }
            List<OneSurfaceBoundaryRemoval> removals =
                new List<OneSurfaceBoundaryRemoval>();
            StringBuilder candidateEvidence = new StringBuilder();
            StringBuilder removedEvidence = new StringBuilder();
            int guard = Mathf.Max(8, vertices.Count * 2);
            while (workingIndices.Count >= 3 && guard-- > 0)
            {
                if (removals.Count > 0)
                {
                    audit.SimplificationAttempts++;
                    List<List<OneSurfaceTriangleIndex>> solutions =
                        ResolveOneSurfaceWorkingBoundarySolutions(
                            vertices,
                            workingIndices,
                            authoredNormal,
                            minimumTriangleArea);
                    for (int solutionIndex = 0;
                         solutionIndex < solutions.Count;
                         solutionIndex++)
                    {
                        audit.RetainedIndexEvidence =
                            FormatOneSurfaceIndexList(workingIndices);
                        audit.SimplifiedTriangleEvidence =
                            FormatOneSurfaceTriangleIndices(
                                solutions[solutionIndex]);
                        audit.RemovedIndexEvidence =
                            removedEvidence.Length > 0
                                ? removedEvidence.ToString()
                                : "none";
                        List<OneSurfaceTriangleIndex> candidateTriangles =
                            new List<OneSurfaceTriangleIndex>(
                                solutions[solutionIndex]);
                        if (!TryReinsertOneSurfaceBoundaryVertices(
                                vertices,
                                authoredNormal,
                                minimumTriangleArea,
                                removals,
                                candidateTriangles,
                                out string reinsertionEvidence,
                                out string reinsertionBlocker))
                        {
                            audit.ReinsertionEvidence =
                                reinsertionEvidence;
                            audit.FailureReason = reinsertionBlocker;
                            continue;
                        }
                        if (!TryCertifyOneSurfaceIndexedTriangulation(
                                vertices,
                                authoredNormal,
                                minimumTriangleArea,
                                candidateTriangles,
                                out string certificationBlocker))
                        {
                            audit.ReinsertionEvidence =
                                reinsertionEvidence;
                            audit.FailureReason = certificationBlocker;
                            continue;
                        }

                        triangles = candidateTriangles;
                        audit.Succeeded = 1;
                        audit.ReinsertionCount = removals.Count;
                        audit.RemovedVertexCount = removals.Count;
                        audit.CandidateEvidence =
                            candidateEvidence.Length > 0
                                ? candidateEvidence.ToString()
                                : "none";
                        audit.RemovedIndexEvidence =
                            removedEvidence.Length > 0
                                ? removedEvidence.ToString()
                                : "none";
                        audit.RetainedIndexEvidence =
                            FormatOneSurfaceIndexList(workingIndices);
                        audit.SimplifiedTriangleEvidence =
                            FormatOneSurfaceTriangleIndices(
                                solutions[solutionIndex]);
                        audit.ReinsertionEvidence = reinsertionEvidence;
                        audit.FailureReason = string.Empty;
                        return true;
                    }
                }

                if (workingIndices.Count <= 3)
                {
                    break;
                }

                bool foundCandidate = false;
                int selectedWorkingIndex = -1;
                OneSurfaceCollinearCandidateAudit selectedCandidate =
                    default;
                List<int> selectedReducedIndices = null;
                float selectedDistance = float.PositiveInfinity;
                float selectedCross = float.PositiveInfinity;
                const float rankingEpsilon = 0.0000001f;
                for (int workingIndex = 0;
                     workingIndex < workingIndices.Count;
                     workingIndex++)
                {
                    OneSurfaceCollinearCandidateAudit candidate =
                        EvaluateOneSurfaceCollinearCandidate(
                            vertices,
                            projected,
                            workingIndices,
                            workingIndex,
                            authoredNormal,
                            planeDistance,
                            planeTolerance,
                            minimumTriangleArea,
                            projectionEpsilon,
                            signedArea,
                            out List<int> reducedIndices,
                            out float reducedSignedArea);
                    AppendOneSurfaceCollinearCandidateEvidence(
                        candidateEvidence,
                        audit.SimplificationAttempts,
                        candidate,
                        reducedSignedArea);
                    if (candidate.Eligible != 1)
                    {
                        continue;
                    }

                    float absoluteCross = Mathf.Abs(
                        candidate.ProjectedCross);
                    bool betterDistance =
                        candidate.ProjectedDistance <
                            selectedDistance - rankingEpsilon;
                    bool equalDistance = Mathf.Abs(
                        candidate.ProjectedDistance -
                        selectedDistance) <= rankingEpsilon;
                    bool betterCross =
                        absoluteCross < selectedCross - rankingEpsilon;
                    bool equalCross = Mathf.Abs(
                        absoluteCross - selectedCross) <=
                            rankingEpsilon;
                    bool lowerIndex = !foundCandidate ||
                        candidate.CurrentIndex <
                            selectedCandidate.CurrentIndex;
                    if (!foundCandidate || betterDistance ||
                        (equalDistance && betterCross) ||
                        (equalDistance && equalCross && lowerIndex))
                    {
                        foundCandidate = true;
                        selectedWorkingIndex = workingIndex;
                        selectedCandidate = candidate;
                        selectedReducedIndices = reducedIndices;
                        selectedDistance = candidate.ProjectedDistance;
                        selectedCross = absoluteCross;
                        audit.SignedAreaAfter = reducedSignedArea;
                    }
                }

                if (!foundCandidate || selectedReducedIndices == null)
                {
                    audit.CandidateEvidence =
                        candidateEvidence.Length > 0
                            ? candidateEvidence.ToString()
                            : "none";
                    audit.RemovedVertexCount = removals.Count;
                    audit.RemovedIndexEvidence =
                        removedEvidence.Length > 0
                            ? removedEvidence.ToString()
                            : "none";
                    audit.RetainedIndexEvidence =
                        FormatOneSurfaceIndexList(workingIndices);
                    if (string.IsNullOrEmpty(audit.FailureReason))
                    {
                        audit.FailureReason =
                            "no eligible tolerance-collinear retained boundary vertex remains";
                    }
                    return false;
                }

                OneSurfaceBoundaryRemoval removal =
                    new OneSurfaceBoundaryRemoval(
                        selectedCandidate.PreviousIndex,
                        selectedCandidate.CurrentIndex,
                        selectedCandidate.NextIndex);
                removals.Add(removal);
                if (removedEvidence.Length > 0)
                {
                    removedEvidence.Append('|');
                }
                removedEvidence.Append(removal.PreviousIndex);
                removedEvidence.Append('/');
                removedEvidence.Append(removal.RemovedIndex);
                removedEvidence.Append('/');
                removedEvidence.Append(removal.NextIndex);
                workingIndices = selectedReducedIndices;
                audit.RemovedVertexCount = removals.Count;
                audit.RetainedIndexEvidence =
                    FormatOneSurfaceIndexList(workingIndices);
                audit.CandidateEvidence =
                    candidateEvidence.Length > 0
                        ? candidateEvidence.ToString()
                        : "none";
                audit.RemovedIndexEvidence =
                    removedEvidence.ToString();

                if (selectedWorkingIndex < 0)
                {
                    audit.FailureReason =
                        "the selected collinear boundary index is invalid";
                    return false;
                }
            }

            audit.CandidateEvidence = candidateEvidence.Length > 0
                ? candidateEvidence.ToString()
                : "none";
            audit.RemovedVertexCount = removals.Count;
            audit.RemovedIndexEvidence = removedEvidence.Length > 0
                ? removedEvidence.ToString()
                : "none";
            audit.RetainedIndexEvidence =
                FormatOneSurfaceIndexList(workingIndices);
            if (string.IsNullOrEmpty(audit.FailureReason))
            {
                audit.FailureReason =
                    "the tolerance-collinear simplification produced no reinsertable complete triangulation";
            }
            return false;
        }

        private static OneSurfaceCollinearCandidateAudit
            EvaluateOneSurfaceCollinearCandidate(
                List<Vector3> vertices,
                List<Vector2> projected,
                List<int> workingIndices,
                int workingIndex,
                Vector3 authoredNormal,
                float planeDistance,
                float planeTolerance,
                float minimumTriangleArea,
                float projectionEpsilon,
                float originalSignedArea,
                out List<int> reducedIndices,
                out float reducedSignedArea)
        {
            reducedIndices = null;
            reducedSignedArea = 0f;
            OneSurfaceCollinearCandidateAudit audit =
                new OneSurfaceCollinearCandidateAudit
                {
                    PreviousIndex = -1,
                    CurrentIndex = -1,
                    NextIndex = -1,
                    LocalFailure =
                        OneSurfaceTriangleCandidateFailure.None,
                    SegmentParameter = 0f,
                    ProjectedDistance = float.PositiveInfinity,
                    ProjectedCross = 0f,
                    Eligible = 0,
                    Blocker = string.Empty
                };
            if (vertices == null || projected == null ||
                workingIndices == null || workingIndices.Count < 4 ||
                workingIndex < 0 ||
                workingIndex >= workingIndices.Count)
            {
                audit.Blocker = "invalid-candidate-input";
                return audit;
            }

            int previousIndex = workingIndices[
                (workingIndex - 1 + workingIndices.Count) %
                    workingIndices.Count];
            int currentIndex = workingIndices[workingIndex];
            int nextIndex = workingIndices[
                (workingIndex + 1) % workingIndices.Count];
            audit.PreviousIndex = previousIndex;
            audit.CurrentIndex = currentIndex;
            audit.NextIndex = nextIndex;
            if (previousIndex < 0 || currentIndex < 0 ||
                nextIndex < 0 || previousIndex >= vertices.Count ||
                currentIndex >= vertices.Count ||
                nextIndex >= vertices.Count ||
                previousIndex >= projected.Count ||
                currentIndex >= projected.Count ||
                nextIndex >= projected.Count ||
                !IsFinite(vertices[previousIndex]) ||
                !IsFinite(vertices[currentIndex]) ||
                !IsFinite(vertices[nextIndex]))
            {
                audit.Blocker = "non-finite-or-invalid-boundary-index";
                return audit;
            }

            OneSurfaceTriangleCandidate localCandidate =
                EvaluateOneSurfaceTriangleCandidate(
                    vertices[previousIndex],
                    vertices[currentIndex],
                    vertices[nextIndex],
                    authoredNormal,
                    minimumTriangleArea);
            audit.LocalArea = localCandidate.Area;
            audit.LocalNormalDot = localCandidate.NormalDot;
            audit.LocalFailure = localCandidate.Failure;
            if (localCandidate.Valid)
            {
                audit.Blocker = "local-triangle-is-already-stable";
                return audit;
            }
            if (localCandidate.Failure !=
                    OneSurfaceTriangleCandidateFailure.NormalAgreement &&
                localCandidate.Failure !=
                    OneSurfaceTriangleCandidateFailure.Area &&
                localCandidate.Failure !=
                    OneSurfaceTriangleCandidateFailure.NonFinite)
            {
                audit.Blocker = "local-failure-is-not-collinearity-compatible";
                return audit;
            }

            float previousResidual = Mathf.Abs(
                Vector3.Dot(authoredNormal, vertices[previousIndex]) -
                planeDistance);
            float currentResidual = Mathf.Abs(
                Vector3.Dot(authoredNormal, vertices[currentIndex]) -
                planeDistance);
            float nextResidual = Mathf.Abs(
                Vector3.Dot(authoredNormal, vertices[nextIndex]) -
                planeDistance);
            if (!IsFiniteFloat(previousResidual) ||
                !IsFiniteFloat(currentResidual) ||
                !IsFiniteFloat(nextResidual) ||
                previousResidual > planeTolerance ||
                currentResidual > planeTolerance ||
                nextResidual > planeTolerance)
            {
                audit.Blocker = "local-vertices-leave-authoritative-plane-tolerance";
                return audit;
            }

            Vector2 previous = projected[previousIndex];
            Vector2 current = projected[currentIndex];
            Vector2 next = projected[nextIndex];
            Vector2 span = next - previous;
            float spanLengthSqr = span.sqrMagnitude;
            if (!IsFiniteFloat(spanLengthSqr) ||
                spanLengthSqr <= projectionEpsilon * projectionEpsilon)
            {
                audit.Blocker = "neighbour-span-is-degenerate";
                return audit;
            }

            float parameter = Vector2.Dot(
                current - previous,
                span) / spanLengthSqr;
            Vector2 closest = previous + span * parameter;
            float projectedDistance = Vector2.Distance(
                current,
                closest);
            float spanLength = Mathf.Sqrt(spanLengthSqr);
            float collinearTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                projectionEpsilon * 8f);
            float parameterTolerance = Mathf.Min(
                0.25f,
                collinearTolerance / Mathf.Max(spanLength, 0.0000001f));
            audit.SegmentParameter = parameter;
            audit.ProjectedDistance = projectedDistance;
            audit.ProjectedCross = ChamferPatchCross2D(
                previous,
                current,
                next);
            if (!IsFiniteFloat(parameter) ||
                !IsFiniteFloat(projectedDistance) ||
                parameter < -parameterTolerance ||
                parameter > 1f + parameterTolerance)
            {
                audit.Blocker = "projection-does-not-land-on-neighbour-segment";
                return audit;
            }
            if (projectedDistance > collinearTolerance)
            {
                audit.Blocker = "projected-distance-exceeds-collinear-tolerance";
                return audit;
            }
            if (!IsOneSurfaceProjectedDiagonalValid(
                    previousIndex,
                    nextIndex,
                    workingIndices,
                    projected,
                    projectionEpsilon))
            {
                audit.Blocker = "replacement-boundary-segment-is-not-contained";
                return audit;
            }

            reducedIndices = new List<int>(workingIndices);
            reducedIndices.RemoveAt(workingIndex);
            List<Vector2> reducedProjected =
                BuildOneSurfaceProjectedIndexLoop(
                    reducedIndices,
                    projected);
            if (reducedProjected == null ||
                reducedProjected.Count < 3)
            {
                reducedIndices = null;
                audit.Blocker = "simplified-loop-is-incomplete";
                return audit;
            }
            reducedSignedArea =
                CalculateChamferPatchSignedArea(reducedProjected);
            float areaTolerance =
                projectionEpsilon * projectionEpsilon;
            if (!IsFiniteFloat(reducedSignedArea) ||
                Mathf.Abs(reducedSignedArea) <= areaTolerance ||
                Mathf.Sign(reducedSignedArea) !=
                    Mathf.Sign(originalSignedArea))
            {
                reducedIndices = null;
                audit.Blocker = "simplified-loop-changes-winding-or-area";
                return audit;
            }
            if (ChamferPatchPolygonSelfIntersects(
                    reducedProjected,
                    projectionEpsilon,
                    out _))
            {
                reducedIndices = null;
                audit.Blocker = "simplified-loop-self-intersects";
                return audit;
            }

            audit.Eligible = 1;
            audit.Blocker = "none";
            return audit;
        }

        private static List<List<OneSurfaceTriangleIndex>>
            ResolveOneSurfaceWorkingBoundarySolutions(
                List<Vector3> vertices,
                List<int> workingIndices,
                Vector3 authoredNormal,
                float minimumTriangleArea)
        {
            List<List<OneSurfaceTriangleIndex>> solutions =
                new List<List<OneSurfaceTriangleIndex>>();
            if (vertices == null || workingIndices == null ||
                workingIndices.Count < 3)
            {
                return solutions;
            }

            List<Vector3> workingVertices =
                new List<Vector3>(workingIndices.Count);
            for (int index = 0;
                 index < workingIndices.Count;
                 index++)
            {
                int originalIndex = workingIndices[index];
                if (originalIndex < 0 ||
                    originalIndex >= vertices.Count)
                {
                    return solutions;
                }
                workingVertices.Add(vertices[originalIndex]);
            }

            if (TryFindStableOneSurfaceFanAnchor(
                    workingVertices,
                    authoredNormal,
                    minimumTriangleArea,
                    out int anchorIndex,
                    out _))
            {
                List<OneSurfaceTriangleIndex> fanTriangles =
                    new List<OneSurfaceTriangleIndex>();
                for (int offset = 1;
                     offset < workingIndices.Count - 1;
                     offset++)
                {
                    fanTriangles.Add(new OneSurfaceTriangleIndex(
                        workingIndices[anchorIndex],
                        workingIndices[
                            (anchorIndex + offset) %
                                workingIndices.Count],
                        workingIndices[
                            (anchorIndex + offset + 1) %
                                workingIndices.Count]));
                }
                solutions.Add(fanTriangles);
            }

            if (TryResolveGeneralOneSurfaceTriangulation(
                    workingVertices,
                    authoredNormal,
                    minimumTriangleArea,
                    out List<OneSurfaceTriangleIndex> localTriangles,
                    out _))
            {
                List<OneSurfaceTriangleIndex> mappedTriangles =
                    new List<OneSurfaceTriangleIndex>(
                        localTriangles.Count);
                for (int triangleIndex = 0;
                     triangleIndex < localTriangles.Count;
                     triangleIndex++)
                {
                    OneSurfaceTriangleIndex triangle =
                        localTriangles[triangleIndex];
                    mappedTriangles.Add(new OneSurfaceTriangleIndex(
                        workingIndices[triangle.A],
                        workingIndices[triangle.B],
                        workingIndices[triangle.C]));
                }
                if (solutions.Count == 0 ||
                    FormatOneSurfaceTriangleIndices(solutions[0]) !=
                    FormatOneSurfaceTriangleIndices(mappedTriangles))
                {
                    solutions.Add(mappedTriangles);
                }
            }
            return solutions;
        }

        private static bool TryReinsertOneSurfaceBoundaryVertices(
            List<Vector3> vertices,
            Vector3 authoredNormal,
            float minimumTriangleArea,
            List<OneSurfaceBoundaryRemoval> removals,
            List<OneSurfaceTriangleIndex> triangles,
            out string evidence,
            out string blocker)
        {
            StringBuilder builder = new StringBuilder();
            blocker = string.Empty;
            for (int removalIndex = removals.Count - 1;
                 removalIndex >= 0;
                 removalIndex--)
            {
                OneSurfaceBoundaryRemoval removal =
                    removals[removalIndex];
                int parentTriangleIndex = -1;
                int parentCount = 0;
                int thirdIndex = -1;
                for (int triangleIndex = 0;
                     triangleIndex < triangles.Count;
                     triangleIndex++)
                {
                    OneSurfaceTriangleIndex triangle =
                        triangles[triangleIndex];
                    if (!OneSurfaceTriangleContainsIndex(
                            triangle,
                            removal.PreviousIndex) ||
                        !OneSurfaceTriangleContainsIndex(
                            triangle,
                            removal.NextIndex))
                    {
                        continue;
                    }
                    parentCount++;
                    parentTriangleIndex = triangleIndex;
                    thirdIndex = ResolveOneSurfaceTriangleThirdIndex(
                        triangle,
                        removal.PreviousIndex,
                        removal.NextIndex);
                }
                if (parentCount != 1 || thirdIndex < 0)
                {
                    blocker =
                        "reinsertion parent boundary edge does not belong to exactly one selected triangle";
                    evidence = builder.Length > 0
                        ? builder.ToString()
                        : "none";
                    return false;
                }

                OneSurfaceTriangleIndex firstReplacement =
                    new OneSurfaceTriangleIndex(
                        removal.PreviousIndex,
                        removal.RemovedIndex,
                        thirdIndex);
                OneSurfaceTriangleIndex secondReplacement =
                    new OneSurfaceTriangleIndex(
                        removal.RemovedIndex,
                        removal.NextIndex,
                        thirdIndex);
                OneSurfaceTriangleCandidate firstCandidate =
                    EvaluateOneSurfaceTriangleCandidate(
                        vertices[firstReplacement.A],
                        vertices[firstReplacement.B],
                        vertices[firstReplacement.C],
                        authoredNormal,
                        minimumTriangleArea);
                OneSurfaceTriangleCandidate secondCandidate =
                    EvaluateOneSurfaceTriangleCandidate(
                        vertices[secondReplacement.A],
                        vertices[secondReplacement.B],
                        vertices[secondReplacement.C],
                        authoredNormal,
                        minimumTriangleArea);
                if (builder.Length > 0)
                {
                    builder.Append('|');
                }
                builder.Append(removal.RemovedIndex);
                builder.Append('@');
                builder.Append(removal.PreviousIndex);
                builder.Append('-');
                builder.Append(removal.NextIndex);
                builder.Append(":parent=");
                builder.Append(FormatOneSurfaceTriangleIndex(
                    triangles[parentTriangleIndex]));
                builder.Append(",replacement=");
                builder.Append(FormatOneSurfaceTriangleIndex(
                    firstReplacement));
                builder.Append('+');
                builder.Append(FormatOneSurfaceTriangleIndex(
                    secondReplacement));
                builder.Append(",areas=");
                builder.Append(firstCandidate.Area.ToString(
                    "G9",
                    CultureInfo.InvariantCulture));
                builder.Append('/');
                builder.Append(secondCandidate.Area.ToString(
                    "G9",
                    CultureInfo.InvariantCulture));
                builder.Append(",normalDots=");
                builder.Append(firstCandidate.NormalDot.ToString(
                    "G9",
                    CultureInfo.InvariantCulture));
                builder.Append('/');
                builder.Append(secondCandidate.NormalDot.ToString(
                    "G9",
                    CultureInfo.InvariantCulture));
                if (!firstCandidate.Valid || !secondCandidate.Valid)
                {
                    builder.Append(",blocker=");
                    builder.Append(!firstCandidate.Valid
                        ? firstCandidate.Failure.ToString()
                        : secondCandidate.Failure.ToString());
                    blocker =
                        "a reinserted boundary subdivision triangle failed final certification";
                    evidence = builder.ToString();
                    return false;
                }
                builder.Append(",blocker=none");
                triangles[parentTriangleIndex] = firstReplacement;
                triangles.Insert(
                    parentTriangleIndex + 1,
                    secondReplacement);
            }

            evidence = builder.Length > 0
                ? builder.ToString()
                : "none";
            return true;
        }

        private static bool TryCertifyOneSurfaceIndexedTriangulation(
            List<Vector3> vertices,
            Vector3 authoredNormal,
            float minimumTriangleArea,
            List<OneSurfaceTriangleIndex> triangles,
            out string blocker)
        {
            blocker = string.Empty;
            if (vertices == null || vertices.Count < 3 ||
                triangles == null ||
                triangles.Count != vertices.Count - 2)
            {
                blocker =
                    "the reinserted triangulation does not contain exactly n-2 triangles";
                return false;
            }

            for (int triangleIndex = 0;
                 triangleIndex < triangles.Count;
                 triangleIndex++)
            {
                OneSurfaceTriangleIndex triangle =
                    triangles[triangleIndex];
                if (triangle.A < 0 || triangle.B < 0 ||
                    triangle.C < 0 || triangle.A >= vertices.Count ||
                    triangle.B >= vertices.Count ||
                    triangle.C >= vertices.Count ||
                    triangle.A == triangle.B ||
                    triangle.B == triangle.C ||
                    triangle.C == triangle.A ||
                    !EvaluateOneSurfaceTriangleCandidate(
                        vertices[triangle.A],
                        vertices[triangle.B],
                        vertices[triangle.C],
                        authoredNormal,
                        minimumTriangleArea).Valid)
                {
                    blocker =
                        "the reinserted triangulation contains an invalid final triangle";
                    return false;
                }
            }

            for (int boundaryIndex = 0;
                 boundaryIndex < vertices.Count;
                 boundaryIndex++)
            {
                int nextIndex =
                    (boundaryIndex + 1) % vertices.Count;
                int ownerCount = 0;
                for (int triangleIndex = 0;
                     triangleIndex < triangles.Count;
                     triangleIndex++)
                {
                    OneSurfaceTriangleIndex triangle =
                        triangles[triangleIndex];
                    if (OneSurfaceTriangleContainsIndex(
                            triangle,
                            boundaryIndex) &&
                        OneSurfaceTriangleContainsIndex(
                            triangle,
                            nextIndex))
                    {
                        ownerCount++;
                    }
                }
                if (ownerCount != 1)
                {
                    blocker =
                        "the reinserted triangulation does not preserve every retained boundary segment exactly once";
                    return false;
                }
            }
            return true;
        }

        private static List<Vector2> BuildOneSurfaceProjectedIndexLoop(
            List<int> indices,
            List<Vector2> projected)
        {
            if (indices == null || projected == null)
            {
                return null;
            }
            List<Vector2> loop = new List<Vector2>(indices.Count);
            for (int index = 0; index < indices.Count; index++)
            {
                int projectedIndex = indices[index];
                if (projectedIndex < 0 ||
                    projectedIndex >= projected.Count)
                {
                    return null;
                }
                loop.Add(projected[projectedIndex]);
            }
            return loop;
        }

        private static bool OneSurfaceTriangleContainsIndex(
            OneSurfaceTriangleIndex triangle,
            int index)
        {
            return triangle.A == index || triangle.B == index ||
                triangle.C == index;
        }

        private static int ResolveOneSurfaceTriangleThirdIndex(
            OneSurfaceTriangleIndex triangle,
            int first,
            int second)
        {
            if (triangle.A != first && triangle.A != second)
            {
                return triangle.A;
            }
            if (triangle.B != first && triangle.B != second)
            {
                return triangle.B;
            }
            if (triangle.C != first && triangle.C != second)
            {
                return triangle.C;
            }
            return -1;
        }

        private static string FormatOneSurfaceBoundaryVertexEvidence(
            List<Vector3> vertices,
            List<Vector2> projected,
            Vector3 authoredNormal,
            float planeDistance)
        {
            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < vertices.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append('|');
                }
                Vector3 position = vertices[index];
                Vector2 point = projected[index];
                float residual = Mathf.Abs(
                    Vector3.Dot(authoredNormal, position) -
                    planeDistance);
                builder.Append(index);
                builder.Append(":p(");
                builder.Append(position.x.ToString(
                    "G9",
                    CultureInfo.InvariantCulture));
                builder.Append('/');
                builder.Append(position.y.ToString(
                    "G9",
                    CultureInfo.InvariantCulture));
                builder.Append('/');
                builder.Append(position.z.ToString(
                    "G9",
                    CultureInfo.InvariantCulture));
                builder.Append("),q(");
                builder.Append(point.x.ToString(
                    "G9",
                    CultureInfo.InvariantCulture));
                builder.Append('/');
                builder.Append(point.y.ToString(
                    "G9",
                    CultureInfo.InvariantCulture));
                builder.Append("),r=");
                builder.Append(residual.ToString(
                    "G9",
                    CultureInfo.InvariantCulture));
            }
            return builder.Length > 0
                ? builder.ToString()
                : "none";
        }

        private static void AppendOneSurfaceCollinearCandidateEvidence(
            StringBuilder builder,
            int iteration,
            OneSurfaceCollinearCandidateAudit candidate,
            float reducedSignedArea)
        {
            if (builder.Length > 0)
            {
                builder.Append('|');
            }
            builder.Append("i");
            builder.Append(iteration);
            builder.Append(':');
            builder.Append(candidate.PreviousIndex);
            builder.Append('/');
            builder.Append(candidate.CurrentIndex);
            builder.Append('/');
            builder.Append(candidate.NextIndex);
            builder.Append(",area=");
            builder.Append(candidate.LocalArea.ToString(
                "G9",
                CultureInfo.InvariantCulture));
            builder.Append(",normalDot=");
            builder.Append(candidate.LocalNormalDot.ToString(
                "G9",
                CultureInfo.InvariantCulture));
            builder.Append(",failure=");
            builder.Append(candidate.LocalFailure);
            builder.Append(",parameter=");
            builder.Append(candidate.SegmentParameter.ToString(
                "G9",
                CultureInfo.InvariantCulture));
            builder.Append(",distance=");
            builder.Append(candidate.ProjectedDistance.ToString(
                "G9",
                CultureInfo.InvariantCulture));
            builder.Append(",cross=");
            builder.Append(candidate.ProjectedCross.ToString(
                "G9",
                CultureInfo.InvariantCulture));
            builder.Append(",reducedArea=");
            builder.Append(reducedSignedArea.ToString(
                "G9",
                CultureInfo.InvariantCulture));
            builder.Append(",eligible=");
            builder.Append(candidate.Eligible);
            builder.Append(",blocker=");
            builder.Append(string.IsNullOrEmpty(candidate.Blocker)
                ? "none"
                : candidate.Blocker);
        }

        private static string FormatOneSurfaceIndexList(
            List<int> indices)
        {
            if (indices == null || indices.Count == 0)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < indices.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append('/');
                }
                builder.Append(indices[index]);
            }
            return builder.ToString();
        }

        private static string FormatOneSurfaceTriangleIndices(
            List<OneSurfaceTriangleIndex> triangles)
        {
            if (triangles == null || triangles.Count == 0)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < triangles.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append('|');
                }
                builder.Append(FormatOneSurfaceTriangleIndex(
                    triangles[index]));
            }
            return builder.ToString();
        }

        private static string FormatOneSurfaceTriangleIndex(
            OneSurfaceTriangleIndex triangle)
        {
            return triangle.A + "/" + triangle.B + "/" + triangle.C;
        }

        private static string ResolveOneSurfaceEvidence(string evidence)
        {
            return string.IsNullOrEmpty(evidence)
                ? "none"
                : evidence;
        }

        private static bool IsOneSurfaceProjectedDiagonalValid(
            int firstIndex,
            int secondIndex,
            List<int> fullBoundary,
            List<Vector2> projected,
            float epsilon)
        {
            if (firstIndex < 0 || secondIndex < 0 ||
                firstIndex >= projected.Count ||
                secondIndex >= projected.Count ||
                firstIndex == secondIndex ||
                !IsFiniteFloat(epsilon) || epsilon < 0f)
            {
                return false;
            }
            if (ChamferPatchDiagonalIntersectsRemainingBoundary(
                    firstIndex,
                    secondIndex,
                    fullBoundary,
                    projected,
                    epsilon))
            {
                return false;
            }

            Vector2 midpoint =
                (projected[firstIndex] + projected[secondIndex]) * 0.5f;
            return IsFiniteFloat(midpoint.x) &&
                IsFiniteFloat(midpoint.y) &&
                IsBoundedPointInsideOrOnPolygon(
                    midpoint,
                    projected,
                    epsilon);
        }

        private static bool CollectOneSurfaceTriangulationTriangles(
            OneSurfaceTriangulationState[,] states,
            int first,
            int last,
            List<OneSurfaceTriangleIndex> triangles)
        {
            if (last <= first + 1)
            {
                return true;
            }
            OneSurfaceTriangulationState state = states[first, last];
            if (!state.Succeeded ||
                state.SplitIndex <= first ||
                state.SplitIndex >= last)
            {
                return false;
            }

            int middle = state.SplitIndex;
            triangles.Add(new OneSurfaceTriangleIndex(
                first,
                middle,
                last));
            return CollectOneSurfaceTriangulationTriangles(
                    states,
                    first,
                    middle,
                    triangles) &&
                CollectOneSurfaceTriangulationTriangles(
                    states,
                    middle,
                    last,
                    triangles);
        }

        private static string FormatOneSurfaceTriangulationFailureEvidence(
            PolygonFace face,
            int faceIndex,
            Vector3 authoredNormal,
            float maximumPlaneResidual,
            float minimumTriangleArea,
            OneSurfaceBoundaryFanAudit boundaryFanAudit,
            OneSurfaceGeneralTriangulationAudit generalAudit,
            OneSurfaceCollinearReinsertionAudit collinearAudit)
        {
            string rejectionTriangle =
                boundaryFanAudit.RejectedTriangleA >= 0
                    ? boundaryFanAudit.RejectedTriangleA + "/" +
                        boundaryFanAudit.RejectedTriangleB + "/" +
                        boundaryFanAudit.RejectedTriangleC
                    : "none";
            return "the one-surface polygon has no complete certified triangulation; " +
                "face=" + faceIndex +
                ",provenance=" +
                    (face == null
                        ? PolygonFaceProvenanceKind.None
                        : face.ProvenanceKind) + ":" +
                    (face == null ? -1 : face.ProvenanceIndex) +
                ",boundaryVertices=" +
                    (face == null || face.Vertices == null
                        ? 0
                        : face.Vertices.Count) +
                ",authoredNormal=(" +
                    authoredNormal.x.ToString(
                        "G9",
                        CultureInfo.InvariantCulture) + "/" +
                    authoredNormal.y.ToString(
                        "G9",
                        CultureInfo.InvariantCulture) + "/" +
                    authoredNormal.z.ToString(
                        "G9",
                        CultureInfo.InvariantCulture) + ")" +
                ",planeResidual=" +
                    maximumPlaneResidual.ToString(
                        "G9",
                        CultureInfo.InvariantCulture) +
                ",minimumTriangleArea=" +
                    minimumTriangleArea.ToString(
                        "G9",
                        CultureInfo.InvariantCulture) +
                ",boundaryFan={anchorsTested:" +
                    boundaryFanAudit.AnchorsTested +
                    ",stableAnchors:" +
                    boundaryFanAudit.StableAnchors +
                    ",bestAnchor:" +
                    boundaryFanAudit.BestAnchorIndex +
                    ",bestCertifiedTriangles:" +
                    boundaryFanAudit.BestCertifiedTriangleCount +
                    ",bestMinimumArea:" +
                    boundaryFanAudit.BestMinimumArea.ToString(
                        "G9",
                        CultureInfo.InvariantCulture) +
                    ",bestMinimumNormalDot:" +
                    boundaryFanAudit.BestMinimumNormalDot.ToString(
                        "G9",
                        CultureInfo.InvariantCulture) +
                    ",bestMaximumAspectRatio:" +
                    boundaryFanAudit.BestMaximumAspectRatio.ToString(
                        "G9",
                        CultureInfo.InvariantCulture) +
                    ",bestMinimumAngleDegrees:" +
                    boundaryFanAudit.BestMinimumAngleDegrees.ToString(
                        "G9",
                        CultureInfo.InvariantCulture) +
                    ",rejectedTriangle:" + rejectionTriangle +
                    ",rejection:" +
                    boundaryFanAudit.Rejection + "}" +
                ",generalTriangulation={projectionSucceeded:" +
                    generalAudit.ProjectionSucceeded +
                    ",projectionSelfIntersection:" +
                    generalAudit.ProjectionSelfIntersection +
                    ",statesEvaluated:" +
                    generalAudit.StatesEvaluated +
                    ",validTriangleCandidates:" +
                    generalAudit.ValidTriangleCandidateCount +
                    ",rejectedNonFinite:" +
                    generalAudit.RejectedNonFiniteCount +
                    ",rejectedArea:" +
                    generalAudit.RejectedAreaCount +
                    ",rejectedNormalAgreement:" +
                    generalAudit.RejectedNormalAgreementCount +
                    ",rejectedDiagonal:" +
                    generalAudit.RejectedDiagonalCount +
                    ",completeSolutionFound:" +
                    generalAudit.CompleteSolutionFound +
                    ",selectedMinimumArea:" +
                    generalAudit.SelectedMinimumArea.ToString(
                        "G9",
                        CultureInfo.InvariantCulture) +
                    ",selectedMinimumNormalDot:" +
                    generalAudit.SelectedMinimumNormalDot.ToString(
                        "G9",
                        CultureInfo.InvariantCulture) +
                    ",selectedMaximumAspectRatio:" +
                    generalAudit.SelectedMaximumAspectRatio.ToString(
                        "G9",
                        CultureInfo.InvariantCulture) +
                    ",selectedMinimumAngleDegrees:" +
                    generalAudit.SelectedMinimumAngleDegrees.ToString(
                        "G9",
                        CultureInfo.InvariantCulture) +
                    ",selectedTriangles:" +
                    (string.IsNullOrEmpty(
                        generalAudit.SelectedTriangleEvidence)
                        ? "none"
                        : generalAudit.SelectedTriangleEvidence) +
                    ",reason:" +
                    (string.IsNullOrEmpty(generalAudit.FailureReason)
                        ? "none"
                        : generalAudit.FailureReason) + "}" +
                ",collinearReinsertion={attempted:" +
                    collinearAudit.Attempted +
                    ",succeeded:" + collinearAudit.Succeeded +
                    ",projectionSucceeded:" +
                    collinearAudit.ProjectionSucceeded +
                    ",projectionSelfIntersection:" +
                    collinearAudit.ProjectionSelfIntersection +
                    ",simplificationAttempts:" +
                    collinearAudit.SimplificationAttempts +
                    ",removedVertexCount:" +
                    collinearAudit.RemovedVertexCount +
                    ",reinsertionCount:" +
                    collinearAudit.ReinsertionCount +
                    ",signedAreaBefore:" +
                    collinearAudit.SignedAreaBefore.ToString(
                        "G9",
                        CultureInfo.InvariantCulture) +
                    ",signedAreaAfter:" +
                    collinearAudit.SignedAreaAfter.ToString(
                        "G9",
                        CultureInfo.InvariantCulture) +
                    ",boundaryVertices:" +
                    ResolveOneSurfaceEvidence(
                        collinearAudit.BoundaryVertexEvidence) +
                    ",unstableBoundaryCandidates:" +
                    ResolveOneSurfaceEvidence(
                        collinearAudit.CandidateEvidence) +
                    ",removedIndices:" +
                    ResolveOneSurfaceEvidence(
                        collinearAudit.RemovedIndexEvidence) +
                    ",retainedIndices:" +
                    ResolveOneSurfaceEvidence(
                        collinearAudit.RetainedIndexEvidence) +
                    ",simplifiedTriangles:" +
                    ResolveOneSurfaceEvidence(
                        collinearAudit.SimplifiedTriangleEvidence) +
                    ",reinsertion:" +
                    ResolveOneSurfaceEvidence(
                        collinearAudit.ReinsertionEvidence) +
                    ",reason:" +
                    ResolveOneSurfaceEvidence(
                        collinearAudit.FailureReason) + "}";
        }

        private static bool TryEmitOneSurfaceTriangle(
            Vector3 a,
            Vector3 b,
            Vector3 c,
            PolygonFace face,
            int faceIndex,
            Vector3 authoredNormal,
            int authoredSurfaceGroup,
            float minimumTriangleArea,
            bool isBevelFace,
            ref BoundedSingleEdgeAuditResult audit,
            TriangleSoup soup,
            out string blocker)
        {
            blocker = string.Empty;
            if (!TryResolveOneSurfaceTriangle(
                    a,
                    b,
                    c,
                    authoredNormal,
                    minimumTriangleArea,
                    out Vector3 orientedB,
                    out Vector3 orientedC,
                    out _,
                    out float normalDeviation))
            {
                blocker = RecordBoundedTriangulationFailure(
                    ref audit,
                    faceIndex,
                    face,
                    BoundedPolygonFailure.Winding,
                    "a polygon triangle failed area or final render-normal agreement certification");
                return false;
            }

            audit.PolygonSurfaceMaximumNormalDeviationDegrees =
                Mathf.Max(
                    audit.PolygonSurfaceMaximumNormalDeviationDegrees,
                    normalDeviation);
            if (isBevelFace)
            {
                audit.BevelRegionMaximumNormalDeviationDegrees =
                    Mathf.Max(
                        audit.BevelRegionMaximumNormalDeviationDegrees,
                        normalDeviation);
            }

            soup.AddTriangle(
                a,
                orientedB,
                orientedC,
                face.Feature,
                face.FeatureStrength,
                authoredNormal,
                authoredSurfaceGroup,
                face.ProvenanceKind,
                face.ProvenanceIndex);
            audit.PolygonSurfaceTriangleCount++;
            audit.PolygonSurfaceAuthoredNormalTriangleCount++;
            audit.PolygonSurfaceAuthoredSurfaceGroupTriangleCount++;
            if (isBevelFace)
            {
                audit.BevelRegionTriangleCount++;
                audit.BevelRegionAuthoredNormalTriangleCount++;
                audit.BevelRegionAuthoredSurfaceGroupTriangleCount++;
            }
            return true;
        }

        private static OneSurfaceTriangleCandidate
            EvaluateOneSurfaceTriangleCandidate(
                Vector3 a,
                Vector3 b,
                Vector3 c,
                Vector3 authoredNormal,
                float minimumTriangleArea)
        {
            if (!IsFinite(a) || !IsFinite(b) || !IsFinite(c) ||
                !IsFinite(authoredNormal) ||
                authoredNormal.sqrMagnitude <= MinimumEdgeLengthSqr ||
                !IsFiniteFloat(minimumTriangleArea) ||
                minimumTriangleArea < 0f)
            {
                return new OneSurfaceTriangleCandidate(
                    false,
                    0f,
                    0f,
                    0f,
                    OneSurfaceTriangleCandidateFailure.NonFinite);
            }

            Vector3 geometricNormal = Vector3.Cross(b - a, c - a);
            if (!IsFinite(geometricNormal) ||
                geometricNormal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return new OneSurfaceTriangleCandidate(
                    false,
                    0f,
                    0f,
                    0f,
                    OneSurfaceTriangleCandidateFailure.NonFinite);
            }

            float area = geometricNormal.magnitude * 0.5f;
            if (!IsFiniteFloat(area) || area <= minimumTriangleArea)
            {
                return new OneSurfaceTriangleCandidate(
                    false,
                    IsFiniteFloat(area) ? area : 0f,
                    0f,
                    0f,
                    OneSurfaceTriangleCandidateFailure.Area);
            }

            if (!TryNormalizeMassVector(
                    geometricNormal,
                    out Vector3 normalizedGeometricNormal) ||
                !TryNormalizeMassVector(
                    authoredNormal,
                    out Vector3 normalizedAuthoredNormal))
            {
                return new OneSurfaceTriangleCandidate(
                    false,
                    area,
                    0f,
                    0f,
                    OneSurfaceTriangleCandidateFailure.NonFinite);
            }

            float normalDot = Mathf.Abs(Vector3.Dot(
                normalizedGeometricNormal,
                normalizedAuthoredNormal));
            if (!IsFiniteFloat(normalDot) ||
                normalDot < OneSurfaceMinimumRenderNormalDot)
            {
                return new OneSurfaceTriangleCandidate(
                    false,
                    area,
                    IsFiniteFloat(normalDot) ? normalDot : 0f,
                    0f,
                    OneSurfaceTriangleCandidateFailure.NormalAgreement);
            }

            float normalDeviation = Mathf.Acos(
                Mathf.Clamp(normalDot, -1f, 1f)) * Mathf.Rad2Deg;
            if (!IsFiniteFloat(normalDeviation))
            {
                return new OneSurfaceTriangleCandidate(
                    false,
                    area,
                    normalDot,
                    0f,
                    OneSurfaceTriangleCandidateFailure.NonFinite);
            }

            float ab = Vector3.Distance(a, b);
            float bc = Vector3.Distance(b, c);
            float ca = Vector3.Distance(c, a);
            float longestEdge = Mathf.Max(ab, Mathf.Max(bc, ca));
            float aspectRatio = area > 0f
                ? longestEdge * longestEdge / (4f * area)
                : float.PositiveInfinity;
            float minimumAngleDegrees = CalculateOneSurfaceMinimumAngleDegrees(
                ab,
                bc,
                ca);
            if (!IsFiniteFloat(aspectRatio) ||
                !IsFiniteFloat(minimumAngleDegrees))
            {
                return new OneSurfaceTriangleCandidate(
                    false,
                    area,
                    normalDot,
                    normalDeviation,
                    OneSurfaceTriangleCandidateFailure.NonFinite);
            }

            return new OneSurfaceTriangleCandidate(
                true,
                area,
                normalDot,
                normalDeviation,
                aspectRatio,
                minimumAngleDegrees,
                OneSurfaceTriangleCandidateFailure.None);
        }

        private static float CalculateOneSurfaceMinimumAngleDegrees(
            float ab,
            float bc,
            float ca)
        {
            if (!IsFiniteFloat(ab) || !IsFiniteFloat(bc) ||
                !IsFiniteFloat(ca) || ab <= 0f || bc <= 0f || ca <= 0f)
            {
                return 0f;
            }

            float angleA = Mathf.Acos(Mathf.Clamp(
                (ab * ab + ca * ca - bc * bc) / (2f * ab * ca),
                -1f,
                1f)) * Mathf.Rad2Deg;
            float angleB = Mathf.Acos(Mathf.Clamp(
                (ab * ab + bc * bc - ca * ca) / (2f * ab * bc),
                -1f,
                1f)) * Mathf.Rad2Deg;
            float angleC = 180f - angleA - angleB;
            return Mathf.Min(angleA, Mathf.Min(angleB, angleC));
        }

        private static bool TryResolveOneSurfaceTriangle(
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 authoredNormal,
            float minimumTriangleArea,
            out Vector3 orientedB,
            out Vector3 orientedC,
            out Vector3 normalizedGeometricNormal,
            out float normalDeviation)
        {
            orientedB = b;
            orientedC = c;
            normalizedGeometricNormal = Vector3.zero;
            normalDeviation = 0f;
            OneSurfaceTriangleCandidate candidate =
                EvaluateOneSurfaceTriangleCandidate(
                    a,
                    b,
                    c,
                    authoredNormal,
                    minimumTriangleArea);
            if (!candidate.Valid)
            {
                return false;
            }

            Vector3 geometricNormal = Vector3.Cross(b - a, c - a);
            if (Vector3.Dot(geometricNormal, authoredNormal) < 0f)
            {
                orientedB = c;
                orientedC = b;
                geometricNormal = -geometricNormal;
            }

            if (!TryNormalizeMassVector(
                    geometricNormal,
                    out normalizedGeometricNormal))
            {
                return false;
            }

            normalDeviation = candidate.NormalDeviationDegrees;
            return IsFinite(normalizedGeometricNormal) &&
                IsFiniteFloat(normalDeviation);
        }

        private static int ResolvePolygonSurfaceGroup(
            PolygonFace face,
            int faceIndex)
        {
            if (face == null || faceIndex < 0)
            {
                return -1;
            }

            int identity = face.ProvenanceIndex >= 0
                ? face.ProvenanceIndex
                : faceIndex;
            int prefix = IsOneSurfaceBevelFace(face)
                ? 0x4B1D0000
                : 0x3A710000;
            return unchecked(
                (prefix ^
                 ((int)face.ProvenanceKind << 20) ^
                 identity) &
                0x7FFFFFFF);
        }

        private static bool IsOneSurfaceBevelFace(PolygonFace face)
        {
            return face != null &&
                (face.ProvenanceKind ==
                    PolygonFaceProvenanceKind.BoundedEdgeBevel ||
                 face.ProvenanceKind ==
                    PolygonFaceProvenanceKind.EdgeBevelPlane);
        }

        private static bool TryResolveOneSurfaceAuthoredNormal(
            PolygonFace face,
            bool isBevelFace,
            out Vector3 authoredNormal)
        {
            authoredNormal = Vector3.zero;
            if (face == null || face.Vertices == null ||
                face.Vertices.Count < 3 ||
                !IsFinite(face.Normal) ||
                face.Normal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return false;
            }

            Vector3 analyticalNormal = face.Normal.normalized;
            if (isBevelFace)
            {
                authoredNormal = analyticalNormal;
                return true;
            }

            Vector3 orderedBoundaryNormal = Vector3.zero;
            for (int vertexIndex = 0;
                 vertexIndex < face.Vertices.Count;
                 vertexIndex++)
            {
                Vector3 current = face.Vertices[vertexIndex];
                Vector3 next = face.Vertices[
                    (vertexIndex + 1) % face.Vertices.Count];
                if (!IsFinite(current) || !IsFinite(next))
                {
                    return false;
                }

                orderedBoundaryNormal.x +=
                    (current.y - next.y) *
                    (current.z + next.z);
                orderedBoundaryNormal.y +=
                    (current.z - next.z) *
                    (current.x + next.x);
                orderedBoundaryNormal.z +=
                    (current.x - next.x) *
                    (current.y + next.y);
            }

            if (!IsFinite(orderedBoundaryNormal) ||
                orderedBoundaryNormal.sqrMagnitude <=
                    MinimumEdgeLengthSqr)
            {
                return false;
            }

            orderedBoundaryNormal.Normalize();
            if (Vector3.Dot(
                    orderedBoundaryNormal,
                    analyticalNormal) < 0f)
            {
                orderedBoundaryNormal = -orderedBoundaryNormal;
            }

            authoredNormal = orderedBoundaryNormal;
            return true;
        }

        private static bool TryFindStableOneSurfaceFanAnchor(
            List<Vector3> vertices,
            Vector3 authoredNormal,
            float minimumTriangleArea,
            out int anchorIndex,
            out OneSurfaceBoundaryFanAudit audit)
        {
            anchorIndex = -1;
            audit = new OneSurfaceBoundaryFanAudit
            {
                BestAnchorIndex = -1,
                BestCertifiedTriangleCount = -1,
                BestMinimumArea = 0f,
                BestMinimumNormalDot = 0f,
                BestMaximumAspectRatio = float.PositiveInfinity,
                BestMinimumAngleDegrees = 0f,
                RejectedTriangleA = -1,
                RejectedTriangleB = -1,
                RejectedTriangleC = -1,
                Rejection = OneSurfaceTriangleCandidateFailure.None
            };
            if (vertices == null || vertices.Count < 3 ||
                !IsFinite(authoredNormal) ||
                authoredNormal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return false;
            }

            authoredNormal.Normalize();
            float bestMinimumNormalDot = -1f;
            float bestMinimumArea = -1f;
            float bestMaximumAspectRatio = float.PositiveInfinity;
            float bestMinimumAngleDegrees = -1f;
            const float rankingEpsilon = 0.000001f;
            for (int candidate = 0;
                 candidate < vertices.Count;
                 candidate++)
            {
                audit.AnchorsTested++;
                Vector3 anchor = vertices[candidate];
                float minimumNormalDot = 1f;
                float minimumArea = float.PositiveInfinity;
                float maximumAspectRatio = 0f;
                float minimumAngleDegrees = 180f;
                int certifiedTriangleCount = 0;
                int rejectedA = -1;
                int rejectedB = -1;
                int rejectedC = -1;
                OneSurfaceTriangleCandidateFailure rejection =
                    OneSurfaceTriangleCandidateFailure.None;
                bool stable = true;
                for (int offset = 1;
                     offset < vertices.Count - 1;
                     offset++)
                {
                    int bIndex =
                        (candidate + offset) % vertices.Count;
                    int cIndex =
                        (candidate + offset + 1) % vertices.Count;
                    OneSurfaceTriangleCandidate triangle =
                        EvaluateOneSurfaceTriangleCandidate(
                            anchor,
                            vertices[bIndex],
                            vertices[cIndex],
                            authoredNormal,
                            minimumTriangleArea);
                    if (!triangle.Valid)
                    {
                        stable = false;
                        rejectedA = candidate;
                        rejectedB = bIndex;
                        rejectedC = cIndex;
                        rejection = triangle.Failure;
                        break;
                    }

                    certifiedTriangleCount++;
                    minimumNormalDot = Mathf.Min(
                        minimumNormalDot,
                        triangle.NormalDot);
                    minimumArea = Mathf.Min(
                        minimumArea,
                        triangle.Area);
                    maximumAspectRatio = Mathf.Max(
                        maximumAspectRatio,
                        triangle.AspectRatio);
                    minimumAngleDegrees = Mathf.Min(
                        minimumAngleDegrees,
                        triangle.MinimumAngleDegrees);
                }

                float comparableMinimumArea =
                    certifiedTriangleCount > 0
                        ? minimumArea
                        : 0f;
                float comparableMinimumNormalDot =
                    certifiedTriangleCount > 0
                        ? minimumNormalDot
                        : 0f;
                bool betterPartial =
                    certifiedTriangleCount >
                        audit.BestCertifiedTriangleCount;
                bool equalPartial =
                    certifiedTriangleCount ==
                        audit.BestCertifiedTriangleCount;
                bool betterPartialNormal =
                    comparableMinimumNormalDot >
                        audit.BestMinimumNormalDot + rankingEpsilon;
                bool equalPartialNormal = Mathf.Abs(
                    comparableMinimumNormalDot -
                    audit.BestMinimumNormalDot) <= rankingEpsilon;
                bool betterPartialArea =
                    comparableMinimumArea >
                        audit.BestMinimumArea + rankingEpsilon;
                bool equalPartialArea = Mathf.Abs(
                    comparableMinimumArea -
                    audit.BestMinimumArea) <= rankingEpsilon;
                bool lowerPartialAnchor =
                    audit.BestAnchorIndex < 0 ||
                    candidate < audit.BestAnchorIndex;
                if (betterPartial ||
                    (equalPartial && betterPartialNormal) ||
                    (equalPartial && equalPartialNormal &&
                        betterPartialArea) ||
                    (equalPartial && equalPartialNormal &&
                        equalPartialArea && lowerPartialAnchor))
                {
                    audit.BestAnchorIndex = candidate;
                    audit.BestCertifiedTriangleCount =
                        certifiedTriangleCount;
                    audit.BestMinimumArea = comparableMinimumArea;
                    audit.BestMinimumNormalDot =
                        comparableMinimumNormalDot;
                    audit.RejectedTriangleA = rejectedA;
                    audit.RejectedTriangleB = rejectedB;
                    audit.RejectedTriangleC = rejectedC;
                    audit.Rejection = rejection;
                }

                if (!stable)
                {
                    continue;
                }

                audit.StableAnchors++;
                bool betterAspect =
                    maximumAspectRatio <
                        bestMaximumAspectRatio - rankingEpsilon;
                bool equalAspect = Mathf.Abs(
                    maximumAspectRatio -
                    bestMaximumAspectRatio) <= rankingEpsilon;
                bool betterAngle =
                    minimumAngleDegrees >
                        bestMinimumAngleDegrees + rankingEpsilon;
                bool equalAngle = Mathf.Abs(
                    minimumAngleDegrees -
                    bestMinimumAngleDegrees) <= rankingEpsilon;
                bool betterArea =
                    minimumArea > bestMinimumArea + rankingEpsilon;
                bool equalArea = Mathf.Abs(
                    minimumArea - bestMinimumArea) <= rankingEpsilon;
                bool betterNormalAgreement =
                    minimumNormalDot >
                        bestMinimumNormalDot + rankingEpsilon;
                bool equalNormalAgreement = Mathf.Abs(
                    minimumNormalDot - bestMinimumNormalDot) <=
                        rankingEpsilon;
                bool lowerAnchor = anchorIndex < 0 || candidate < anchorIndex;
                if (anchorIndex >= 0 && !betterAspect &&
                    !(equalAspect && betterAngle) &&
                    !(equalAspect && equalAngle && betterArea) &&
                    !(equalAspect && equalAngle && equalArea &&
                        betterNormalAgreement) &&
                    !(equalAspect && equalAngle && equalArea &&
                        equalNormalAgreement && lowerAnchor))
                {
                    continue;
                }

                bestMaximumAspectRatio = maximumAspectRatio;
                bestMinimumAngleDegrees = minimumAngleDegrees;
                bestMinimumNormalDot = minimumNormalDot;
                bestMinimumArea = minimumArea;
                anchorIndex = candidate;
            }

            if (anchorIndex >= 0)
            {
                audit.BestAnchorIndex = anchorIndex;
                audit.BestCertifiedTriangleCount =
                    vertices.Count - 2;
                audit.BestMinimumArea = bestMinimumArea;
                audit.BestMinimumNormalDot =
                    bestMinimumNormalDot;
                audit.BestMaximumAspectRatio =
                    bestMaximumAspectRatio;
                audit.BestMinimumAngleDegrees =
                    bestMinimumAngleDegrees;
                audit.RejectedTriangleA = -1;
                audit.RejectedTriangleB = -1;
                audit.RejectedTriangleC = -1;
                audit.Rejection =
                    OneSurfaceTriangleCandidateFailure.None;
            }
            return anchorIndex >= 0;
        }

        private static string RecordBoundedTriangulationFailure(
            ref BoundedSingleEdgeAuditResult audit,
            int faceIndex,
            PolygonFace face,
            BoundedPolygonFailure failure,
            string reason)
        {
            audit.TriangulationFailureFace = faceIndex;
            audit.TriangulationFailureKind = failure;
            audit.TriangulationFailureProvenanceKind = face == null
                ? PolygonFaceProvenanceKind.None
                : face.ProvenanceKind;
            audit.TriangulationFailureProvenanceIndex = face == null
                ? -1
                : face.ProvenanceIndex;
            audit.TriangulationFailureReason = reason;
            audit.PolygonSurfaceRenderValid = 0;
            audit.PolygonSurfaceFailureFace = faceIndex;
            audit.PolygonSurfaceFailureProvenanceIndex = face == null
                ? -1
                : face.ProvenanceIndex;
            audit.PolygonSurfaceFailureReason = reason;
            if (IsOneSurfaceBevelFace(face))
            {
                audit.BevelRegionRenderValid = 0;
                audit.BevelRegionFailureFace = faceIndex;
                audit.BevelRegionFailureProvenanceIndex =
                    face.ProvenanceIndex;
                audit.BevelRegionFailureReason = reason;
            }
            return "triangulation failed on " +
                audit.TriangulationFailureProvenanceKind + ":" +
                audit.TriangulationFailureProvenanceIndex +
                " (face " + faceIndex + ") because " + reason;
        }

        private static void SetBoundedSingleEdgeDiagnostic(
            ref string diagnostic,
            string value)
        {
            if (string.IsNullOrEmpty(diagnostic) &&
                !string.IsNullOrEmpty(value))
            {
                diagnostic = value;
            }
        }

        #endregion
    }
}
