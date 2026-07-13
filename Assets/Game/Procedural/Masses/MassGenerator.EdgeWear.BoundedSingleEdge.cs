using System;
using System.Collections.Generic;
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
            public int CanonicalRailCount;
            public float MaximumBoundarySnapDistance;
            public int OwnerClipAttemptedCount;
            public int OwnerClipCount;
            public int OwnerIntersectionFailureCount;
            public int OwnerDegenerateCount;
            public int OwnerNonPlanarCount;
            public int OwnerNonSimpleCount;
            public int OwnerNonConvexCount;
            public int OwnerWindingFailureCount;
            public int BoundarySubdivisionCount;
            public int BevelFaceCount;
            public int EndpointCapCount;
            public int ModifiedSourceFaceCount;
            public int ForeignSourceFaceModifiedCount;
            public int ForeignBoundarySubdividedCount;
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
            public int TriangulationFailureFace;
            public BoundedPolygonFailure TriangulationFailureKind;
            public PolygonFaceProvenanceKind
                TriangulationFailureProvenanceKind;
            public int TriangulationFailureProvenanceIndex;
            public string TriangulationFailureReason;
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

        private readonly struct BoundedIsolatedRailPoint
        {
            public readonly Vector3 Position;
            public readonly int RailIndex;
            public readonly int OwnerGraphFaceIndex;
            public readonly int OwnerSourceFaceIndex;
            public readonly int SourceVertexIndex;
            public readonly int AdjacentGraphEdgeIndex;
            public readonly int TargetGraphFaceIndex;
            public readonly int TargetSourceFaceIndex;
            public readonly float BoundarySnapDistance;

            public BoundedIsolatedRailPoint(
                Vector3 position,
                int railIndex,
                int ownerGraphFaceIndex,
                int ownerSourceFaceIndex,
                int sourceVertexIndex,
                int adjacentGraphEdgeIndex,
                int targetGraphFaceIndex,
                int targetSourceFaceIndex,
                float boundarySnapDistance)
            {
                Position = position;
                RailIndex = railIndex;
                OwnerGraphFaceIndex = ownerGraphFaceIndex;
                OwnerSourceFaceIndex = ownerSourceFaceIndex;
                SourceVertexIndex = sourceVertexIndex;
                AdjacentGraphEdgeIndex = adjacentGraphEdgeIndex;
                TargetGraphFaceIndex = targetGraphFaceIndex;
                TargetSourceFaceIndex = targetSourceFaceIndex;
                BoundarySnapDistance = boundarySnapDistance;
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
                out TriangleSoup previewSoup)
        {
            previewSoup = null;
            BoundedSingleEdgeAuditResult result =
                new BoundedSingleEdgeAuditResult
                {
                    SelectedOrdinal = -1,
                    SourceEdgeIndex = -1,
                    PrepareFailedFace = -1,
                    PrepareFailedProvenanceIndex = -1,
                    ResultPreparation = CreateBoundedPreparationAudit(),
                    SourcePreparation = CreateBoundedPreparationAudit(),
                    TriangulationFailureFace = -1,
                    TriangulationFailureProvenanceIndex = -1
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
            result.TargetBoundaryCount = isolatedRails.Length;
            for (int railIndex = 0;
                 railIndex < isolatedRails.Length;
                 railIndex++)
            {
                if (!float.IsNaN(isolatedRails[railIndex].BoundarySnapDistance) &&
                    !float.IsInfinity(isolatedRails[railIndex].BoundarySnapDistance))
                {
                    result.CanonicalRailCount++;
                    result.MaximumBoundarySnapDistance = Mathf.Max(
                        result.MaximumBoundarySnapDistance,
                        isolatedRails[railIndex].BoundarySnapDistance);
                }
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
                        " on a canonical rail-subdivided source face";
                }
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    string.IsNullOrEmpty(preparationBlocker)
                        ? "the bounded single-edge shell failed preview preparation"
                        : preparationBlocker);
                return result;
            }

            Vector3 boundedSolidCentre =
                CalculatePlaneCutFaceVertexCentre(sourceFaces);
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

            bool sourcePreparationValid = TryPrepareBoundedFaces(
                sourceFaces,
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

            CountBoundedSingleEdgeFaces(
                auditedFaces,
                selected.GraphEdgeIndex,
                ref result);
            AuditBoundedSourceFaceChanges(
                sourceFaces,
                auditedFaces,
                context,
                selected.GraphEdgeIndex,
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
            const double maximumRetainedVolumeRatio = 1.0001;
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

            float railTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                minimumStableEdgeLength * 0.02f);
            bool polygonValid =
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
                result.BevelFaceCount == 1 &&
                result.EndpointCapCount == 2 &&
                result.ModifiedSourceFaceCount == 2 &&
                result.ForeignSourceFaceModifiedCount == 0 &&
                result.RailDeviation <= railTolerance &&
                result.MaximumExtentBeyondRails <= railTolerance &&
                result.OpenEdgeCount == 0 &&
                result.NonManifoldEdgeCount == 0 &&
                result.TJunctionCount == 0 &&
                result.InvalidFaceCount == 0 &&
                boundsValid &&
                volumeValid;

            if (result.IsolatedRailSolved != 1)
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
            else if (result.BevelFaceCount != 1)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the bounded edge does not retain exactly one bevel polygon");
            }
            else if (result.EndpointCapCount != 2)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the bounded edge does not retain exactly two endpoint caps");
            }
            else if (result.ModifiedSourceFaceCount != 2 ||
                result.ForeignSourceFaceModifiedCount != 0)
            {
                SetBoundedSingleEdgeDiagnostic(
                    ref result.Diagnostic,
                    "the bounded edge changes source-surface geometry outside its two owners");
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
            if (polygonValid &&
                !TryTriangulateBoundedPreviewFaces(
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
                if (triangleAudit.PreviewGeometryValid != 1)
                {
                    SetBoundedSingleEdgeDiagnostic(
                        ref result.Diagnostic,
                        "the exact bounded preview triangle soup failed certification");
                }
                else
                {
                    result.GeometryValid = 1;
                    previewSoup = soup;
                }
            }

            return result;
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
                blocker = "the isolated rail solution collapses a rail or endpoint cap";
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

            int adjacentEdgeIndex = previousIsSelected
                ? nextEdgeIndex
                : previousEdgeIndex;
            PolygonFace ownerSourceFace = sourceFaces[ownerSourceFaceIndex];
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

            EdgeWearGraphEdge adjacentEdge =
                context.Graph.Edges[adjacentEdgeIndex];
            int targetGraphFaceIndex = adjacentEdge.FaceA ==
                ownerGraphFaceIndex
                ? adjacentEdge.FaceB
                : adjacentEdge.FaceB == ownerGraphFaceIndex
                    ? adjacentEdge.FaceA
                    : -1;
            if (targetGraphFaceIndex < 0 ||
                targetGraphFaceIndex >= context.Graph.Faces.Count ||
                adjacentEdge.ExtraFaceCount > 0)
            {
                blocker =
                    "the isolated rail adjacent boundary is not manifold";
                return false;
            }

            EdgeWearGraphFace targetGraphFace =
                context.Graph.Faces[targetGraphFaceIndex];
            int targetSourceFaceIndex = targetGraphFace.SourceFaceIndex;
            if (targetSourceFaceIndex < 0 ||
                targetSourceFaceIndex >= sourceFaces.Count ||
                targetSourceFaceIndex == ownerSourceFaceIndex)
            {
                blocker =
                    "the isolated rail cannot resolve its exact endpoint-adjacent source face";
                return false;
            }

            int targetLocalEdge = -1;
            int targetOccurrenceCount = 0;
            for (int localEdgeIndex = 0;
                 localEdgeIndex < targetGraphFace.EdgeIndices.Count;
                 localEdgeIndex++)
            {
                if (targetGraphFace.EdgeIndices[localEdgeIndex] !=
                    adjacentEdgeIndex)
                {
                    continue;
                }
                targetLocalEdge = localEdgeIndex;
                targetOccurrenceCount++;
            }
            PolygonFace targetSourceFace = sourceFaces[targetSourceFaceIndex];
            if (targetOccurrenceCount != 1 ||
                targetSourceFace == null ||
                targetSourceFace.Vertices == null ||
                targetSourceFace.Vertices.Count !=
                    targetGraphFace.VertexIndices.Count)
            {
                blocker =
                    "the isolated rail exact target boundary is inconsistent";
                return false;
            }

            Vector3 boundaryStart =
                targetSourceFace.Vertices[targetLocalEdge];
            Vector3 boundaryEnd = targetSourceFace.Vertices[
                (targetLocalEdge + 1) % targetSourceFace.Vertices.Count];
            Vector3 boundarySegment = boundaryEnd - boundaryStart;
            float boundaryLengthSqr = boundarySegment.sqrMagnitude;
            if (boundaryLengthSqr <= MinimumEdgeLengthSqr)
            {
                blocker = "the isolated rail adjacent source edge is degenerate";
                return false;
            }

            float parameter = Vector3.Dot(
                solved - boundaryStart,
                boundarySegment) / boundaryLengthSqr;
            Vector3 canonical =
                boundaryStart + boundarySegment * parameter;
            float boundarySnapDistance = (solved - canonical).magnitude;
            float pointTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                minimumStableEdgeLength * 0.002f);
            float minimumEndpointDistance = Mathf.Max(
                PointMergeDistance * 4f,
                minimumStableEdgeLength * 0.01f);
            float boundaryLength = Mathf.Sqrt(boundaryLengthSqr);
            float endpointParameter = Mathf.Min(
                0.25f,
                minimumEndpointDistance / boundaryLength);
            if (parameter <= endpointParameter ||
                parameter >= 1f - endpointParameter ||
                boundarySnapDistance > pointTolerance)
            {
                blocker =
                    "the isolated rail endpoint lies outside its exact adjacent source-edge segment";
                return false;
            }

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
                    "the canonical isolated rail leaves one of its exact boundary planes";
                return false;
            }

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
                adjacentEdgeIndex,
                targetGraphFaceIndex,
                targetSourceFaceIndex,
                boundarySnapDistance);
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
            if (!TryInsertBoundedRailSubdivisions(
                    boundedFaces,
                    context,
                    sourceFaceA,
                    sourceFaceB,
                    isolatedRails,
                    minimumStableEdgeLength,
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
                    out PolygonFace bevelFace) ||
                !TryCreateBoundedFace(
                    new List<Vector3> { sourceA, a1, a0 },
                    Vector3.zero,
                    solidCentre,
                    PolygonFaceProvenanceKind.BoundedEndpointCap,
                    edge.VertexA,
                    selected.Candidate.Strength,
                    minimumStableFaceArea,
                    out PolygonFace capA) ||
                !TryCreateBoundedFace(
                    new List<Vector3> { sourceB, b0, b1 },
                    Vector3.zero,
                    solidCentre,
                    PolygonFaceProvenanceKind.BoundedEndpointCap,
                    edge.VertexB,
                    selected.Candidate.Strength,
                    minimumStableFaceArea,
                    out PolygonFace capB))
            {
                blocker = "the selected edge produces a collapsed bounded face";
                boundedFaces = null;
                return false;
            }

            boundedFaces.Add(bevelFace);
            boundedFaces.Add(capA);
            boundedFaces.Add(capB);
            return true;
        }

        private static bool TryInsertBoundedRailSubdivisions(
            List<PolygonFace> faces,
            ChamferTopologyContext context,
            int ownerFaceA,
            int ownerFaceB,
            BoundedIsolatedRailPoint[] rails,
            float minimumStableEdgeLength,
            out int subdivisionCount,
            out int targetBoundaryCount,
            out string blocker)
        {
            subdivisionCount = 0;
            targetBoundaryCount = 0;
            blocker = string.Empty;
            if (rails == null || rails.Length != 4)
            {
                blocker = "the isolated rail set does not contain four exact boundary targets";
                return false;
            }

            float tolerance = Mathf.Max(
                PointMergeDistance * 8f,
                minimumStableEdgeLength * 0.002f);
            float toleranceSqr = tolerance * tolerance;
            Dictionary<int, List<BoundedExactRailSplit>> splitsBySourceFace =
                new Dictionary<int, List<BoundedExactRailSplit>>();
            HashSet<int> targetedGraphEdges = new HashSet<int>();

            for (int railIndex = 0; railIndex < rails.Length; railIndex++)
            {
                BoundedIsolatedRailPoint rail = rails[railIndex];
                if (rail.RailIndex != railIndex ||
                    rail.TargetSourceFaceIndex < 0 ||
                    rail.TargetSourceFaceIndex >= faces.Count ||
                    rail.TargetSourceFaceIndex == ownerFaceA ||
                    rail.TargetSourceFaceIndex == ownerFaceB ||
                    rail.TargetGraphFaceIndex < 0 ||
                    rail.TargetGraphFaceIndex >= context.Graph.Faces.Count ||
                    rail.AdjacentGraphEdgeIndex < 0 ||
                    rail.AdjacentGraphEdgeIndex >= context.Graph.Edges.Count)
                {
                    blocker =
                        "isolated rail r" + railIndex +
                        " has invalid exact boundary ownership";
                    return false;
                }

                EdgeWearGraphFace targetGraphFace =
                    context.Graph.Faces[rail.TargetGraphFaceIndex];
                if (targetGraphFace.SourceFaceIndex !=
                    rail.TargetSourceFaceIndex)
                {
                    blocker =
                        "isolated rail r" + railIndex +
                        " target face provenance is inconsistent";
                    return false;
                }

                int targetLocalEdge = -1;
                int targetOccurrenceCount = 0;
                for (int localEdgeIndex = 0;
                     localEdgeIndex < targetGraphFace.EdgeIndices.Count;
                     localEdgeIndex++)
                {
                    if (targetGraphFace.EdgeIndices[localEdgeIndex] !=
                        rail.AdjacentGraphEdgeIndex)
                    {
                        continue;
                    }
                    targetLocalEdge = localEdgeIndex;
                    targetOccurrenceCount++;
                }
                if (targetOccurrenceCount != 1)
                {
                    blocker =
                        "isolated rail r" + railIndex +
                        " owns " + targetOccurrenceCount +
                        " matching segments on its exact target face";
                    return false;
                }

                PolygonFace targetFace = faces[rail.TargetSourceFaceIndex];
                if (targetFace == null ||
                    targetFace.Vertices == null ||
                    targetFace.Vertices.Count !=
                        targetGraphFace.VertexIndices.Count)
                {
                    blocker =
                        "isolated rail r" + railIndex +
                        " target source-face boundary is inconsistent";
                    return false;
                }

                Vector3 start = targetFace.Vertices[targetLocalEdge];
                Vector3 end = targetFace.Vertices[
                    (targetLocalEdge + 1) % targetFace.Vertices.Count];
                Vector3 segment = end - start;
                float segmentLengthSqr = segment.sqrMagnitude;
                if (segmentLengthSqr <= MinimumEdgeLengthSqr)
                {
                    blocker =
                        "isolated rail r" + railIndex +
                        " exact target segment is degenerate";
                    return false;
                }

                float parameter = Vector3.Dot(
                    rail.Position - start,
                    segment) / segmentLengthSqr;
                float segmentLength = Mathf.Sqrt(segmentLengthSqr);
                float endpointParameter = Mathf.Min(
                    0.25f,
                    tolerance / segmentLength);
                Vector3 closest = start + segment * parameter;
                if (parameter <= endpointParameter ||
                    parameter >= 1f - endpointParameter ||
                    (rail.Position - closest).sqrMagnitude > toleranceSqr)
                {
                    blocker =
                        "isolated rail r" + railIndex +
                        " does not lie inside its exact graph-owned boundary segment";
                    return false;
                }

                if (!splitsBySourceFace.TryGetValue(
                        rail.TargetSourceFaceIndex,
                        out List<BoundedExactRailSplit> faceSplits))
                {
                    faceSplits = new List<BoundedExactRailSplit>();
                    splitsBySourceFace.Add(
                        rail.TargetSourceFaceIndex,
                        faceSplits);
                }
                faceSplits.Add(new BoundedExactRailSplit(
                    targetLocalEdge,
                    parameter,
                    railIndex,
                    rail.Position));
                targetedGraphEdges.Add(rail.AdjacentGraphEdgeIndex);
                targetBoundaryCount++;
            }

            if (targetBoundaryCount != rails.Length ||
                targetedGraphEdges.Count != rails.Length)
            {
                blocker =
                    "the isolated rails do not resolve four distinct exact boundary targets";
                return false;
            }

            int[] railInsertions = new int[rails.Length];
            foreach (KeyValuePair<int, List<BoundedExactRailSplit>> pair
                     in splitsBySourceFace)
            {
                int sourceFaceIndex = pair.Key;
                PolygonFace face = faces[sourceFaceIndex];
                List<Vector3> source = face.Vertices;
                List<BoundedExactRailSplit> faceSplits = pair.Value;
                faceSplits.Sort((left, right) =>
                {
                    int edgeCompare =
                        left.LocalEdgeIndex.CompareTo(right.LocalEdgeIndex);
                    return edgeCompare != 0
                        ? edgeCompare
                        : left.Parameter.CompareTo(right.Parameter);
                });

                List<Vector3> rebuilt = new List<Vector3>(
                    source.Count + faceSplits.Count);
                int splitCursor = 0;
                for (int localEdgeIndex = 0;
                     localEdgeIndex < source.Count;
                     localEdgeIndex++)
                {
                    AddBoundedVertex(rebuilt, source[localEdgeIndex]);
                    while (splitCursor < faceSplits.Count &&
                           faceSplits[splitCursor].LocalEdgeIndex ==
                               localEdgeIndex)
                    {
                        BoundedExactRailSplit split =
                            faceSplits[splitCursor++];
                        int before = rebuilt.Count;
                        AddBoundedVertex(rebuilt, split.Position);
                        if (rebuilt.Count <= before)
                        {
                            blocker =
                                "isolated rail r" + split.RailIndex +
                                " collapsed while splitting its exact boundary";
                            return false;
                        }
                        railInsertions[split.RailIndex]++;
                        subdivisionCount++;
                    }
                }
                if (splitCursor != faceSplits.Count)
                {
                    blocker =
                        "one or more isolated rail targets were not consumed by their source face";
                    return false;
                }

                RemoveClosingDuplicate(rebuilt);
                faces[sourceFaceIndex] = new PolygonFace(
                    rebuilt,
                    face.Normal,
                    face.Feature,
                    face.FeatureStrength,
                    face.ProvenanceKind,
                    face.ProvenanceIndex);
            }

            for (int railIndex = 0;
                 railIndex < railInsertions.Length;
                 railIndex++)
            {
                if (railInsertions[railIndex] != 1)
                {
                    blocker =
                        "isolated rail r" + railIndex +
                        " split its exact boundary " +
                        railInsertions[railIndex] + " times";
                    return false;
                }
            }
            return subdivisionCount == rails.Length;
        }

        private readonly struct BoundedExactRailSplit
        {
            public readonly int LocalEdgeIndex;
            public readonly float Parameter;
            public readonly int RailIndex;
            public readonly Vector3 Position;

            public BoundedExactRailSplit(
                int localEdgeIndex,
                float parameter,
                int railIndex,
                Vector3 position)
            {
                LocalEdgeIndex = localEdgeIndex;
                Parameter = parameter;
                RailIndex = railIndex;
                Position = position;
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

        private static void AuditBoundedSourceFaceChanges(
            List<PolygonFace> sourceFaces,
            List<PolygonFace> auditedFaces,
            ChamferTopologyContext context,
            int sourceEdgeIndex,
            ref BoundedSingleEdgeAuditResult result)
        {
            EdgeWearGraphEdge edge = context.Graph.Edges[sourceEdgeIndex];
            int ownerA = context.Graph.Faces[edge.FaceA].SourceFaceIndex;
            int ownerB = context.Graph.Faces[edge.FaceB].SourceFaceIndex;

            for (int sourceFaceIndex = 0;
                 sourceFaceIndex < sourceFaces.Count;
                 sourceFaceIndex++)
            {
                PolygonFace source = sourceFaces[sourceFaceIndex];
                PolygonFace audited = FindBoundedSourceFace(
                    auditedFaces,
                    sourceFaceIndex);
                bool boundaryOnlyDifference = false;
                bool equivalent = audited != null &&
                    AreBoundedPolygonsGeometricallyEquivalent(
                        source.Vertices,
                        audited.Vertices,
                        source.Normal,
                        out boundaryOnlyDifference);
                bool owner = sourceFaceIndex == ownerA ||
                    sourceFaceIndex == ownerB;
                if (owner)
                {
                    if (!equivalent)
                    {
                        result.ModifiedSourceFaceCount++;
                    }
                }
                else if (!equivalent)
                {
                    result.ForeignSourceFaceModifiedCount++;
                }
                else if (boundaryOnlyDifference)
                {
                    result.ForeignBoundarySubdividedCount++;
                }
            }
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
