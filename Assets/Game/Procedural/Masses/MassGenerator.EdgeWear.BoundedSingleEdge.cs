using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Bounded single-edge bevel prototype

        private struct BoundedSingleEdgeAuditResult
        {
            public int CandidateCount;
            public int SelectedOrdinal;
            public int SourceEdgeIndex;
            public int IsolatedRailSolved;
            public int WidthAttemptCount;
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
            BoundedSingleEdgeAuditResult result =
                new BoundedSingleEdgeAuditResult
                {
                    SelectedOrdinal = -1,
                    SourceEdgeIndex = -1,
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
                return result;
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
                    out int widthAttemptCount,
                    out float solvedWidth,
                    out string railBlocker))
            {
                result.WidthAttemptCount = widthAttemptCount;
                result.SolvedWidth = solvedWidth;
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    railBlocker);
                return result;
            }
            result.IsolatedRailSolved = 1;
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

            if (!TryBuildBoundedSingleEdgeFaces(
                    sourceFaces,
                    context,
                    selected,
                    isolatedRails,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    ref result,
                    out List<PolygonFace> boundedFaces,
                    out int boundarySubdivisionCount,
                    out string buildBlocker))
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    buildBlocker);
                return result;
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
                return result;
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
                return result;
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
                return result;
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
                return result;
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
                return result;
            }

            CountBoundedSingleEdgeFaces(
                auditedFaces,
                selected.GraphEdgeIndex,
                ref result);
            BoundedSourceFaceChangeAudit rawSourceChanges =
                AuditBoundedSourceFaceChanges(
                    attributedSourceFaces,
                    auditedFaces,
                    context,
                    selected.GraphEdgeIndex,
                    result.EndpointSupportFaceA,
                    result.EndpointSupportFaceB);
            BoundedSourceFaceChangeAudit preparedSourceChanges =
                AuditBoundedSourceFaceChanges(
                    preparedSourceFaces,
                    auditedFaces,
                    context,
                    selected.GraphEdgeIndex,
                    result.EndpointSupportFaceA,
                    result.EndpointSupportFaceB);
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
                result.EndpointSupportRemovedVertexCount == 2 &&
                result.EndpointSupportRailInsertionCount == 4 &&
                result.BevelFaceCount == 1 &&
                result.EndpointCapCount == 0 &&
                result.ModifiedSourceFaceCount == 4 &&
                result.OwnerSourceFaceModifiedCount == 2 &&
                result.EndpointSupportSourceFaceModifiedCount == 2 &&
                result.UnexpectedSourceFaceModifiedCount == 0 &&
                result.BoundaryOnlyUnexpectedSourceFaceCount == 0 &&
                result.ForeignSourceFaceModifiedCount == 0 &&
                result.ForeignBoundarySubdividedCount == 0 &&
                result.PreparedSourceChangeComparisonAttempted == 1 &&
                result.SourceProvenanceCertificationValid == 1 &&
                result.RailDeviation <= railTolerance &&
                result.MaximumExtentBeyondRails <= railTolerance &&
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
            else if (result.EndpointSupportClipCount != 2 ||
                result.EndpointSupportRemovedVertexCount != 2 ||
                result.EndpointSupportRailInsertionCount != 4)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the bounded edge did not replace both endpoint source corners with exact support-face rail boundaries");
            }
            else if (result.BevelFaceCount != 1)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the bounded edge does not retain exactly one bevel polygon");
            }
            else if (result.EndpointCapCount != 0)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the bounded edge still contains obsolete endpoint cap polygons");
            }
            else if (result.SourceProvenanceCertificationValid != 1)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the bounded source baselines do not preserve one complete unique source-face provenance set");
            }
            else if (result.ModifiedSourceFaceCount != 4 ||
                result.OwnerSourceFaceModifiedCount != 2 ||
                result.EndpointSupportSourceFaceModifiedCount != 2 ||
                result.UnexpectedSourceFaceModifiedCount != 0 ||
                result.BoundaryOnlyUnexpectedSourceFaceCount != 0 ||
                result.ForeignSourceFaceModifiedCount != 0 ||
                result.ForeignBoundarySubdividedCount != 0)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the bounded edge did not modify exactly its two owner faces and two endpoint support faces relative to the prepared source baseline");
            }
            else if (result.RailDeviation > railTolerance ||
                result.MaximumExtentBeyondRails > railTolerance)
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

            return result;
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
            const int maximumWidthAttempts = 12;
            const float widthBackoff = 0.75f;

            rails = null;
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

            float minimumWidth = Mathf.Max(
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
                 attemptIndex < maximumWidthAttempts;
                 attemptIndex++)
            {
                widthAttemptCount++;
                solvedWidth = attemptWidth;
                if (TrySolveBoundedIsolatedRailsAtWidth(
                        sourceFaces,
                        context,
                        selected,
                        requestedWidth,
                        attemptWidth,
                        minimumStableEdgeLength,
                        out BoundedIsolatedRailPoint[] candidateRails,
                        out lastBlocker))
                {
                    rails = candidateRails;
                    return true;
                }

                if (attemptWidth <= minimumWidth + PointMergeDistance)
                {
                    break;
                }

                float nextWidth = Mathf.Max(
                    minimumWidth,
                    attemptWidth * widthBackoff);
                if (nextWidth >= attemptWidth - PointMergeDistance)
                {
                    break;
                }
                attemptWidth = nextWidth;
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

            Vector3 solidCentre = CalculatePlaneCutFaceVertexCentre(
                sourceFaces);
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
                int supportB)
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
                    provenanceIndex == supportB;
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
                    if (!equivalent)
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
            PolygonFace bevelFace = null;
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face.ProvenanceKind ==
                        PolygonFaceProvenanceKind.BoundedEdgeBevel &&
                    face.ProvenanceIndex == sourceEdgeIndex)
                {
                    bevelFace = face;
                    break;
                }
            }
            if (bevelFace == null)
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
                for (int vertexIndex = 0;
                     vertexIndex < bevelFace.Vertices.Count;
                     vertexIndex++)
                {
                    minimumDistanceSqr = Mathf.Min(
                        minimumDistanceSqr,
                        (bevelFace.Vertices[vertexIndex] -
                         expected[expectedIndex]).sqrMagnitude);
                }
                maximumDeviationSqr = Mathf.Max(
                    maximumDeviationSqr,
                    minimumDistanceSqr);
            }
            result.RailDeviation = Mathf.Sqrt(maximumDeviationSqr);

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
                 vertexIndex < bevelFace.Vertices.Count;
                 vertexIndex++)
            {
                float edgeValue = Vector3.Dot(
                    bevelFace.Vertices[vertexIndex],
                    edgeAxis);
                float widthValue = Vector3.Dot(
                    bevelFace.Vertices[vertexIndex],
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

                if (IsOneSurfaceBevelFace(face))
                {
                    if (!TryTriangulateBoundedBevelRegionFace(
                            face,
                            faceIndex,
                            minimumTriangleArea,
                            ref audit,
                            soup,
                            out blocker))
                    {
                        soup = null;
                        return false;
                    }
                    audit.TriangulatedFaceCount++;
                    continue;
                }

                Vector3 centre = CalculateAverage(convexityLoop);
                if (!IsFinite(centre))
                {
                    blocker = RecordBoundedTriangulationFailure(
                        ref audit,
                        faceIndex,
                        face,
                        BoundedPolygonFailure.NonFinite,
                        "the convex region has no finite fan centre");
                    soup = null;
                    return false;
                }

                for (int vertexIndex = 0;
                     vertexIndex < face.Vertices.Count;
                     vertexIndex++)
                {
                    Vector3 start = face.Vertices[vertexIndex];
                    Vector3 end = face.Vertices[
                        (vertexIndex + 1) % face.Vertices.Count];
                    List<Vector3> triangle = new List<Vector3>
                    {
                        centre,
                        start,
                        end
                    };
                    if (CalculatePolygonArea(triangle) <=
                        minimumTriangleArea)
                    {
                        blocker = RecordBoundedTriangulationFailure(
                            ref audit,
                            faceIndex,
                            face,
                            BoundedPolygonFailure.Degenerate,
                            "a boundary segment collapses against the fan centre");
                        soup = null;
                        return false;
                    }

                    int previousPositionCount = soup.Positions.Count;
                    AddOrientedTriangle(
                        soup,
                        centre,
                        start,
                        end,
                        face.Normal,
                        face.Feature,
                        face.FeatureStrength);
                    if (soup.Positions.Count !=
                        previousPositionCount + 3)
                    {
                        blocker = RecordBoundedTriangulationFailure(
                            ref audit,
                            faceIndex,
                            face,
                            BoundedPolygonFailure.Degenerate,
                            "triangle emission rejected a real boundary segment");
                        soup = null;
                        return false;
                    }

                    Vector3 emittedNormal = Vector3.Cross(
                        soup.Positions[previousPositionCount + 1] -
                            soup.Positions[previousPositionCount],
                        soup.Positions[previousPositionCount + 2] -
                            soup.Positions[previousPositionCount]);
                    if (!IsFinite(emittedNormal) ||
                        Vector3.Dot(emittedNormal, face.Normal) <= 0f)
                    {
                        blocker = RecordBoundedTriangulationFailure(
                            ref audit,
                            faceIndex,
                            face,
                            BoundedPolygonFailure.Winding,
                            "an emitted boundary triangle changes parent-face winding");
                        soup = null;
                        return false;
                    }
                }

                audit.TriangulatedFaceCount++;
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
                blocker = "the bounded bevel region did not render as one planar authored-normal surface";
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

        private static bool TryTriangulateBoundedBevelRegionFace(
            PolygonFace face,
            int faceIndex,
            float minimumTriangleArea,
            ref BoundedSingleEdgeAuditResult audit,
            TriangleSoup soup,
            out string blocker)
        {
            blocker = string.Empty;
            audit.BevelRegionFaceCount++;
            audit.BevelRegionBoundaryVertexCount += face.Vertices.Count;

            Vector3 authoredNormal = face.Normal;
            if (!IsFinite(authoredNormal) ||
                authoredNormal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                audit.BevelRegionFailureFace = faceIndex;
                audit.BevelRegionFailureProvenanceIndex =
                    face.ProvenanceIndex;
                audit.BevelRegionFailureReason =
                    "the bevel region has no finite authoritative plane normal";
                blocker = RecordBoundedTriangulationFailure(
                    ref audit,
                    faceIndex,
                    face,
                    BoundedPolygonFailure.NonFinite,
                    audit.BevelRegionFailureReason);
                return false;
            }
            authoredNormal.Normalize();

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
            audit.BevelRegionMaximumPlaneResidual = Mathf.Max(
                audit.BevelRegionMaximumPlaneResidual,
                maximumPlaneResidual);
            float planeTolerance = PointMergeDistance * 8f;
            if (maximumPlaneResidual > planeTolerance)
            {
                audit.BevelRegionFailureFace = faceIndex;
                audit.BevelRegionFailureProvenanceIndex =
                    face.ProvenanceIndex;
                audit.BevelRegionFailureReason =
                    "the complete bevel boundary is not on one authoritative plane";
                blocker = RecordBoundedTriangulationFailure(
                    ref audit,
                    faceIndex,
                    face,
                    BoundedPolygonFailure.NonPlanar,
                    audit.BevelRegionFailureReason);
                return false;
            }

            if (!TryFindStableOneSurfaceFanAnchor(
                    face.Vertices,
                    minimumTriangleArea,
                    out int anchorIndex))
            {
                audit.BevelRegionFailureFace = faceIndex;
                audit.BevelRegionFailureProvenanceIndex =
                    face.ProvenanceIndex;
                audit.BevelRegionFailureReason =
                    "the one-surface bevel boundary has no stable direct triangulation anchor";
                blocker = RecordBoundedTriangulationFailure(
                    ref audit,
                    faceIndex,
                    face,
                    BoundedPolygonFailure.Degenerate,
                    audit.BevelRegionFailureReason);
                return false;
            }

            Vector3 anchor = face.Vertices[anchorIndex];
            int expectedTriangleCount = face.Vertices.Count - 2;
            int emittedTriangleCount = 0;
            for (int offset = 1;
                 offset < face.Vertices.Count - 1;
                 offset++)
            {
                Vector3 b = face.Vertices[
                    (anchorIndex + offset) % face.Vertices.Count];
                Vector3 c = face.Vertices[
                    (anchorIndex + offset + 1) % face.Vertices.Count];
                Vector3 geometricNormal = Vector3.Cross(
                    b - anchor,
                    c - anchor);
                List<Vector3> triangle = new List<Vector3>
                {
                    anchor,
                    b,
                    c
                };
                if (!IsFinite(geometricNormal) ||
                    geometricNormal.sqrMagnitude <= MinimumEdgeLengthSqr ||
                    CalculatePolygonArea(triangle) <= minimumTriangleArea)
                {
                    audit.BevelRegionFailureFace = faceIndex;
                    audit.BevelRegionFailureProvenanceIndex =
                        face.ProvenanceIndex;
                    audit.BevelRegionFailureReason =
                        "the one-surface bevel boundary produces a degenerate direct triangle";
                    blocker = RecordBoundedTriangulationFailure(
                        ref audit,
                        faceIndex,
                        face,
                        BoundedPolygonFailure.Degenerate,
                        audit.BevelRegionFailureReason);
                    return false;
                }

                if (Vector3.Dot(geometricNormal, authoredNormal) < 0f)
                {
                    Vector3 temporary = b;
                    b = c;
                    c = temporary;
                    geometricNormal = -geometricNormal;
                }

                float normalDeviation = Vector3.Angle(
                    geometricNormal.normalized,
                    authoredNormal);
                audit.BevelRegionMaximumNormalDeviationDegrees =
                    Mathf.Max(
                        audit.BevelRegionMaximumNormalDeviationDegrees,
                        normalDeviation);

                int authoredSurfaceGroup = unchecked(
                    0x4B1D0000 ^
                    ((int)face.ProvenanceKind << 20) ^
                    face.ProvenanceIndex);
                soup.AddTriangle(
                    anchor,
                    b,
                    c,
                    face.Feature,
                    face.FeatureStrength,
                    authoredNormal,
                    authoredSurfaceGroup);
                emittedTriangleCount++;
                audit.BevelRegionTriangleCount++;
                audit.BevelRegionAuthoredNormalTriangleCount++;
                audit.BevelRegionAuthoredSurfaceGroupTriangleCount++;
            }

            if (emittedTriangleCount != expectedTriangleCount)
            {
                audit.BevelRegionFailureFace = faceIndex;
                audit.BevelRegionFailureProvenanceIndex =
                    face.ProvenanceIndex;
                audit.BevelRegionFailureReason =
                    "the one-surface bevel did not emit exactly boundaryVertices-minus-two triangles";
                blocker = RecordBoundedTriangulationFailure(
                    ref audit,
                    faceIndex,
                    face,
                    BoundedPolygonFailure.Degenerate,
                    audit.BevelRegionFailureReason);
                return false;
            }

            return true;
        }


        private static bool IsOneSurfaceBevelFace(PolygonFace face)
        {
            return face != null &&
                (face.ProvenanceKind ==
                    PolygonFaceProvenanceKind.BoundedEdgeBevel ||
                 face.ProvenanceKind ==
                    PolygonFaceProvenanceKind.EdgeBevelPlane);
        }

        private static bool TryFindStableOneSurfaceFanAnchor(
            List<Vector3> vertices,
            float minimumTriangleArea,
            out int anchorIndex)
        {
            anchorIndex = -1;
            if (vertices == null || vertices.Count < 3)
            {
                return false;
            }

            float bestMinimumArea = -1f;
            for (int candidate = 0;
                 candidate < vertices.Count;
                 candidate++)
            {
                Vector3 anchor = vertices[candidate];
                float minimumArea = float.PositiveInfinity;
                bool stable = true;
                for (int offset = 1;
                     offset < vertices.Count - 1;
                     offset++)
                {
                    Vector3 b = vertices[
                        (candidate + offset) % vertices.Count];
                    Vector3 c = vertices[
                        (candidate + offset + 1) % vertices.Count];
                    float area = CalculatePolygonArea(
                        new List<Vector3> { anchor, b, c });
                    if (!IsFiniteFloat(area) ||
                        area <= minimumTriangleArea)
                    {
                        stable = false;
                        break;
                    }
                    minimumArea = Mathf.Min(minimumArea, area);
                }

                if (!stable || minimumArea <= bestMinimumArea)
                {
                    continue;
                }

                bestMinimumArea = minimumArea;
                anchorIndex = candidate;
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
