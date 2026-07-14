using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        private enum BoundedAllEdgesStage
        {
            NotStarted,
            CandidateEvaluation,
            PointCloud,
            PlaneExtraction,
            FacetOrdering,
            FacetSanitation,
            FacetClassification,
            Preparation,
            TopologyCertification,
            Triangulation,
            Complete
        }

        private sealed class BoundedAllEdgesAuditResult
        {
            public BoundedAllEdgesStage Stage;
            public BoundedAllEdgesStage FailureStage;
            public int CandidateCount;
            public int ConvexCandidateCount;
            public int RailSolvedEdgeCount;
            public int RailRejectedEdgeCount;
            public int HullSuppressedEdgeCount;
            public int ActiveEdgeCount;
            public int PointCount;
            public int PointCloudRank;
            public Vector3 PointCloudBoundsMinimum;
            public Vector3 PointCloudBoundsMaximum;
            public int HullIterationCount;
            public int HullTriplesTested;
            public int HullDegenerateTriples;
            public int HullNearDegenerateTriples;
            public int HullNormalizationRejectedTriples;
            public int HullPostNormalizationInvalidTriples;
            public float HullPlaneMinimumCrossMagnitude;
            public float HullMinimumRejectedCrossMagnitude;
            public float HullMaximumRejectedCrossMagnitude;
            public float HullMinimumAcceptedCrossMagnitude;
            public int HullSupportingTriples;
            public int HullStraddlingTriples;
            public int HullPlanesCreated;
            public int HullPlanesMerged;
            public int HullPlanesBeforePrune;
            public int HullPlanesRemovedUnderThreePoints;
            public int HullInvalidPlanesRemoved;
            public int HullFirstInvalidPlaneIndex = -1;
            public int HullFirstInvalidSeedA = -1;
            public int HullFirstInvalidSeedB = -1;
            public int HullFirstInvalidSeedC = -1;
            public float HullFirstInvalidSeedCrossMagnitude;
            public string HullFirstInvalidPlaneReason = string.Empty;
            public int HullPlaneCount;
            public int HullPlanesAttempted;
            public int HullFacesCompleted;
            public int HullFailurePlaneIndex = -1;
            public Vector3 HullFailurePlaneNormal;
            public float HullFailurePlaneDistance;
            public int HullFailurePlanePointCount;
            public int HullFailureOrderedVertexCount;
            public int HullFailureSanitizedVertexCount;
            public float HullFailureFacetArea;
            public int HullFailureConvexityValid;
            public string HullFailureReason = string.Empty;
            public int OutputFaceCount;
            public int SourceFaceCount;
            public int BevelFaceCount;
            public int VertexJunctionFaceCount;
            public int MissingBevelFaceCount;
            public int DuplicateBevelFaceCount;
            public int OpenEdgeCount;
            public int NonManifoldEdgeCount;
            public int TJunctionCount;
            public int InvalidFaceCount;
            public int BoundsValid;
            public float BoundsTolerance;
            public Vector3 SourceBoundsMinimum;
            public Vector3 SourceBoundsMaximum;
            public Vector3 ResultBoundsMinimum;
            public Vector3 ResultBoundsMaximum;
            public Vector3 BoundsMinimumMargin;
            public Vector3 BoundsMaximumMargin;
            public int SourceContainmentViolations;
            public float MaximumSourceContainmentViolation;
            public int ResultConvexityViolations;
            public float MaximumResultConvexityViolation;
            public int IntroducedInteriorIntersections;
            public double SourceVolume;
            public double ResultVolume;
            public double VolumeRatio;
            public double VolumeDelta;
            public double VolumeLowerMargin;
            public double VolumeUpperMargin;
            public int VolumeValid;
            public int TriangulationAttempted;
            public int TriangulatedFaceCount;
            public int TriangleCount;
            public int TriangleSoupValid;
            public PlaneCutBevelAuditResult TriangleAudit;
            public BoundedSingleEdgeAuditResult CertificationAudit;
            public int GeometryValid;
            public int CornerDiagnosticAttempted;
            public int CornerDiagnosticValid;
            public string CornerDiagnostic = string.Empty;
            public int PlaneDiagnosticAttempted;
            public int PlaneDiagnosticValid;
            public int PlaneDiagnosticActiveEdges;
            public int PlaneDiagnosticBuiltEdges;
            public int PlaneDiagnosticDeferredEdges;
            public int PlaneDiagnosticRejectedEdges;
            public string PlaneDiagnosticEvidence = string.Empty;
            public BoundedPreparationAudit Preparation;
            public string EdgeEvidence = string.Empty;
            public string HullPointEvidence = string.Empty;
            public string HullPlaneEvidence = string.Empty;
            public string HullFaceEvidence = string.Empty;
            public string Diagnostic = string.Empty;
            public string TelemetryRelativePath = string.Empty;
            public int TelemetryWriteSucceeded;
            public string TelemetryWriteFailure = string.Empty;
        }

        private sealed class BoundedAllEdgePlan
        {
            public int Ordinal;
            public EdgeWearSelectedGraphEdge Selected;
            public BoundedEdgeClassification Classification;
            public BoundedEdgeClassificationEvidence ClassificationEvidence;
            public int EdgeVertexA;
            public int EdgeVertexB;
            public Vector3 SourcePositionA;
            public Vector3 SourcePositionB;
            public BoundedIsolatedRailPoint[] Rails;
            public float SolvedWidth;
            public int WidthAttempts;
            public float MaximumBoundarySnap;
            public Vector3 PlaneNormal;
            public float PlaneDistance;
            public float MaximumPlaneResidual;
            public float SolidCentreSide;
            public float SourceEdgeSideA;
            public float SourceEdgeSideB;
            public bool Active;
            public bool HullSuppressed;
            public int EmittedFaceCount;
            public string Failure = string.Empty;
        }

        private sealed class BoundedHullPlane
        {
            public Vector3 Normal;
            public float Distance;
            public int SeedPointA = -1;
            public int SeedPointB = -1;
            public int SeedPointC = -1;
            public float SeedCrossMagnitude;
            public float MinimumMergedSeedCrossMagnitude;
            public float MaximumMergedSeedCrossMagnitude;
            public readonly HashSet<int> PointIndices = new HashSet<int>();
        }

        private readonly struct BoundedHullProjectedPoint
        {
            public readonly int PointIndex;
            public readonly Vector2 Position;

            public BoundedHullProjectedPoint(int pointIndex, Vector2 position)
            {
                PointIndex = pointIndex;
                Position = position;
            }
        }

        private static void SetBoundedAllEdgesFailure(
            BoundedAllEdgesAuditResult audit,
            BoundedAllEdgesStage stage,
            string diagnostic)
        {
            audit.Stage = stage;
            audit.FailureStage = stage;
            audit.Diagnostic = string.IsNullOrEmpty(diagnostic)
                ? "unspecified unified bounded failure"
                : diagnostic;
        }

        private static void RefreshBoundedAllEdgePlanCounts(
            BoundedAllEdgesAuditResult audit,
            List<BoundedAllEdgePlan> plans,
            bool includeEmissionCounts)
        {
            audit.ActiveEdgeCount = 0;
            audit.MissingBevelFaceCount = 0;
            audit.DuplicateBevelFaceCount = 0;
            if (plans == null)
            {
                return;
            }

            for (int planIndex = 0; planIndex < plans.Count; planIndex++)
            {
                BoundedAllEdgePlan plan = plans[planIndex];
                if (!plan.Active)
                {
                    continue;
                }

                audit.ActiveEdgeCount++;
                if (!includeEmissionCounts)
                {
                    continue;
                }
                if (plan.EmittedFaceCount == 0)
                {
                    audit.MissingBevelFaceCount++;
                }
                else if (plan.EmittedFaceCount > 1)
                {
                    audit.DuplicateBevelFaceCount +=
                        plan.EmittedFaceCount - 1;
                }
            }
        }

        private static void ResetBoundedHullFacetAudit(
            BoundedAllEdgesAuditResult audit)
        {
            audit.HullPlanesAttempted = 0;
            audit.HullFacesCompleted = 0;
            audit.HullFailurePlaneIndex = -1;
            audit.HullFailurePlaneNormal = Vector3.zero;
            audit.HullFailurePlaneDistance = 0f;
            audit.HullFailurePlanePointCount = 0;
            audit.HullFailureOrderedVertexCount = 0;
            audit.HullFailureSanitizedVertexCount = 0;
            audit.HullFailureFacetArea = 0f;
            audit.HullFailureConvexityValid = 0;
            audit.HullFailureReason = string.Empty;
            audit.OutputFaceCount = 0;
            audit.SourceFaceCount = 0;
            audit.BevelFaceCount = 0;
            audit.VertexJunctionFaceCount = 0;
            audit.HullFaceEvidence = string.Empty;
        }

        private static void AuditBoundedPointCloudShape(
            List<Vector3> points,
            float tolerance,
            BoundedAllEdgesAuditResult audit)
        {
            audit.PointCloudRank = 0;
            audit.PointCloudBoundsMinimum = Vector3.zero;
            audit.PointCloudBoundsMaximum = Vector3.zero;
            if (points == null || points.Count == 0)
            {
                return;
            }

            Bounds bounds = new Bounds(points[0], Vector3.zero);
            for (int pointIndex = 1; pointIndex < points.Count; pointIndex++)
            {
                bounds.Encapsulate(points[pointIndex]);
            }
            audit.PointCloudBoundsMinimum = bounds.min;
            audit.PointCloudBoundsMaximum = bounds.max;

            float linearTolerance = Mathf.Max(
                tolerance,
                PointMergeDistance * 8f);
            float linearToleranceSqr =
                linearTolerance * linearTolerance;
            Vector3 origin = points[0];

            int axisPoint = -1;
            for (int pointIndex = 1; pointIndex < points.Count; pointIndex++)
            {
                if ((points[pointIndex] - origin).sqrMagnitude >
                    linearToleranceSqr)
                {
                    axisPoint = pointIndex;
                    break;
                }
            }
            if (axisPoint < 0)
            {
                return;
            }
            audit.PointCloudRank = 1;

            Vector3 axis = points[axisPoint] - origin;
            float axisLength = axis.magnitude;
            int planePoint = -1;
            Vector3 planeNormal = Vector3.zero;
            for (int pointIndex = 1; pointIndex < points.Count; pointIndex++)
            {
                if (pointIndex == axisPoint)
                {
                    continue;
                }
                Vector3 cross = Vector3.Cross(
                    axis,
                    points[pointIndex] - origin);
                float distanceFromAxis = axisLength > 0f
                    ? cross.magnitude / axisLength
                    : 0f;
                if (distanceFromAxis > linearTolerance)
                {
                    planePoint = pointIndex;
                    planeNormal = cross.normalized;
                    break;
                }
            }
            if (planePoint < 0)
            {
                return;
            }
            audit.PointCloudRank = 2;

            for (int pointIndex = 1; pointIndex < points.Count; pointIndex++)
            {
                if (pointIndex == axisPoint ||
                    pointIndex == planePoint)
                {
                    continue;
                }
                float distanceFromPlane = Mathf.Abs(
                    Vector3.Dot(
                        planeNormal,
                        points[pointIndex] - origin));
                if (distanceFromPlane > linearTolerance)
                {
                    audit.PointCloudRank = 3;
                    return;
                }
            }
        }

        private static BoundedAllEdgesAuditResult AuditBoundedAllEdgesBevel(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            float requestedWidth,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            out TriangleSoup previewSoup)
        {
            previewSoup = null;
            BoundedAllEdgesAuditResult audit =
                new BoundedAllEdgesAuditResult
                {
                    Stage = BoundedAllEdgesStage.CandidateEvaluation,
                    FailureStage = BoundedAllEdgesStage.NotStarted,
                    Preparation = CreateBoundedPreparationAudit()
                };

            List<EdgeWearSelectedGraphEdge> eligible =
                BuildBoundedSingleEdgeEligibleList(context);
            audit.CandidateCount = eligible.Count;
            if (eligible.Count == 0)
            {
                SetBoundedAllEdgesFailure(
                    audit,
                    BoundedAllEdgesStage.CandidateEvaluation,
                    "no selected manifold edge is available for unified bounded evaluation");
                return audit;
            }

            Vector3 sourceCentre =
                CalculatePlaneCutFaceVertexCentre(sourceFaces);
            float classificationTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                minimumStableEdgeLength * 0.02f);
            List<BoundedAllEdgePlan> plans =
                new List<BoundedAllEdgePlan>(eligible.Count);

            for (int ordinal = 0; ordinal < eligible.Count; ordinal++)
            {
                EdgeWearSelectedGraphEdge selected = eligible[ordinal];
                EdgeWearGraphEdge graphEdge =
                    context.Graph.Edges[selected.GraphEdgeIndex];
                BoundedAllEdgePlan plan = new BoundedAllEdgePlan
                {
                    Ordinal = ordinal,
                    Selected = selected,
                    EdgeVertexA = graphEdge.VertexA,
                    EdgeVertexB = graphEdge.VertexB,
                    SourcePositionA =
                        context.Graph.Vertices[graphEdge.VertexA].Position,
                    SourcePositionB =
                        context.Graph.Vertices[graphEdge.VertexB].Position
                };
                plans.Add(plan);

                bool classificationMeasured = TryClassifyBoundedEdge(
                    sourceFaces,
                    context,
                    selected,
                    sourceCentre,
                    classificationTolerance,
                    out BoundedEdgeClassificationEvidence edgeEvidence);
                BoundedEdgeClassification classification =
                    classificationMeasured
                        ? edgeEvidence.Classification
                        : BoundedEdgeClassification.Ambiguous;
                plan.Classification = classification;
                plan.ClassificationEvidence = edgeEvidence;
                if (classification != BoundedEdgeClassification.Convex)
                {
                    plan.Failure = "classification:" + classification;
                    audit.RailRejectedEdgeCount++;
                    continue;
                }
                audit.ConvexCandidateCount++;

                if (!TrySolveBoundedIsolatedSingleEdgeRails(
                        sourceFaces,
                        context,
                        selected,
                        requestedWidth,
                        minimumStableEdgeLength,
                        out BoundedIsolatedRailPoint[] rails,
                        out int widthAttempts,
                        out float solvedWidth,
                        out string blocker))
                {
                    plan.WidthAttempts = widthAttempts;
                    plan.SolvedWidth = solvedWidth;
                    plan.Failure = string.IsNullOrEmpty(blocker)
                        ? "rail solve failed"
                        : blocker;
                    audit.RailRejectedEdgeCount++;
                    continue;
                }

                if (rails == null || rails.Length != 4)
                {
                    plan.WidthAttempts = widthAttempts;
                    plan.SolvedWidth = solvedWidth;
                    plan.Failure =
                        "isolated rail solve did not return exactly four rails";
                    audit.RailRejectedEdgeCount++;
                    continue;
                }
                bool railsFinite = true;
                for (int railIndex = 0; railIndex < rails.Length; railIndex++)
                {
                    if (!IsFinite(rails[railIndex].Position))
                    {
                        railsFinite = false;
                        break;
                    }
                }
                if (!railsFinite)
                {
                    plan.WidthAttempts = widthAttempts;
                    plan.SolvedWidth = solvedWidth;
                    plan.Failure = "isolated rail solve returned a non-finite point";
                    audit.RailRejectedEdgeCount++;
                    continue;
                }

                plan.Rails = rails;
                plan.WidthAttempts = widthAttempts;
                plan.SolvedWidth = solvedWidth;
                plan.MaximumBoundarySnap = 0f;
                for (int railIndex = 0; railIndex < rails.Length; railIndex++)
                {
                    plan.MaximumBoundarySnap = Mathf.Max(
                        plan.MaximumBoundarySnap,
                        rails[railIndex].BoundarySnapDistance);
                }

                Vector3 normal = selected.Candidate.BevelNormal;
                if (!IsFinite(normal) ||
                    normal.sqrMagnitude <= MinimumEdgeLengthSqr)
                {
                    plan.Failure = "invalid bevel plane normal";
                    audit.RailRejectedEdgeCount++;
                    continue;
                }
                normal.Normalize();
                float distance = 0f;
                for (int railIndex = 0; railIndex < rails.Length; railIndex++)
                {
                    distance += Vector3.Dot(normal, rails[railIndex].Position);
                }
                distance /= rails.Length;

                float maximumResidual = 0f;
                for (int railIndex = 0; railIndex < rails.Length; railIndex++)
                {
                    maximumResidual = Mathf.Max(
                        maximumResidual,
                        Mathf.Abs(
                            Vector3.Dot(normal, rails[railIndex].Position) -
                            distance));
                }
                plan.MaximumPlaneResidual = maximumResidual;
                plan.SolidCentreSide =
                    Vector3.Dot(normal, sourceCentre) - distance;
                plan.SourceEdgeSideA =
                    Vector3.Dot(normal, plan.SourcePositionA) - distance;
                plan.SourceEdgeSideB =
                    Vector3.Dot(normal, plan.SourcePositionB) - distance;
                if (maximumResidual > classificationTolerance)
                {
                    plan.Failure = "non-coplanar isolated rails";
                    audit.RailRejectedEdgeCount++;
                    continue;
                }
                if (plan.SolidCentreSide >= -classificationTolerance ||
                    plan.SourceEdgeSideA <= classificationTolerance ||
                    plan.SourceEdgeSideB <= classificationTolerance)
                {
                    plan.Failure =
                        "isolated bevel plane failed subtractive sidedness certification";
                    audit.RailRejectedEdgeCount++;
                    continue;
                }

                plan.PlaneNormal = normal;
                plan.PlaneDistance = distance;
                plan.Active = true;
                audit.RailSolvedEdgeCount++;
            }

            RefreshBoundedAllEdgePlanCounts(
                audit,
                plans,
                includeEmissionCounts: false);
            audit.EdgeEvidence = FormatBoundedAllEdgeEvidence(plans);
            if (audit.RailSolvedEdgeCount == 0)
            {
                SetBoundedAllEdgesFailure(
                    audit,
                    BoundedAllEdgesStage.CandidateEvaluation,
                    "no selected edge produced a stable isolated bounded rail plane");
                return audit;
            }

            List<Vector3> hullPoints = null;
            List<BoundedHullPlane> hullPlanes = null;
            List<PolygonFace> hullFaces = null;
            int maximumIterations = Mathf.Max(1, plans.Count + 1);
            for (int iteration = 0; iteration < maximumIterations; iteration++)
            {
                audit.HullIterationCount = iteration + 1;
                ResetBoundedHullFacetAudit(audit);
                audit.Stage = BoundedAllEdgesStage.PointCloud;
                hullPoints = BuildBoundedAllEdgePointCloud(
                    context,
                    plans);
                audit.PointCount = hullPoints.Count;
                AuditBoundedPointCloudShape(
                    hullPoints,
                    classificationTolerance,
                    audit);
                audit.HullPointEvidence =
                    FormatBoundedHullPointEvidence(hullPoints);
                RefreshBoundedAllEdgePlanCounts(
                    audit,
                    plans,
                    includeEmissionCounts: false);
                if (audit.PointCloudRank < 3)
                {
                    audit.EdgeEvidence =
                        FormatBoundedAllEdgeEvidence(plans);
                    SetBoundedAllEdgesFailure(
                        audit,
                        BoundedAllEdgesStage.PointCloud,
                        "combined bounded point cloud is not three-dimensional");
                    return audit;
                }

                audit.Stage = BoundedAllEdgesStage.PlaneExtraction;
                if (!TryBuildBoundedConvexHullPlanes(
                        hullPoints,
                        sourceCentre,
                        classificationTolerance,
                        audit,
                        out hullPlanes,
                        out string hullBlocker))
                {
                    audit.EdgeEvidence = FormatBoundedAllEdgeEvidence(plans);
                    SetBoundedAllEdgesFailure(
                        audit,
                        BoundedAllEdgesStage.PlaneExtraction,
                        hullBlocker);
                    return audit;
                }

                audit.Stage = BoundedAllEdgesStage.FacetOrdering;
                if (!TryBuildBoundedHullFaces(
                        hullPoints,
                        hullPlanes,
                        sourceFaces,
                        plans,
                        classificationTolerance,
                        minimumStableFaceArea,
                        audit,
                        out hullFaces,
                        out int sourceFaceCount,
                        out int bevelFaceCount,
                        out int vertexJunctionFaceCount,
                        out string faceBlocker))
                {
                    audit.EdgeEvidence = FormatBoundedAllEdgeEvidence(plans);
                    SetBoundedAllEdgesFailure(
                        audit,
                        audit.FailureStage == BoundedAllEdgesStage.NotStarted
                            ? BoundedAllEdgesStage.FacetOrdering
                            : audit.FailureStage,
                        faceBlocker);
                    return audit;
                }

                audit.HullPlaneCount = hullPlanes.Count;
                audit.OutputFaceCount = hullFaces.Count;
                audit.Stage = BoundedAllEdgesStage.FacetClassification;
                audit.SourceFaceCount = sourceFaceCount;
                audit.BevelFaceCount = bevelFaceCount;
                audit.VertexJunctionFaceCount = vertexJunctionFaceCount;
                audit.HullFaceEvidence =
                    FormatBoundedHullFaceEvidence(hullFaces);

                int newlySuppressed = 0;
                for (int planIndex = 0; planIndex < plans.Count; planIndex++)
                {
                    BoundedAllEdgePlan plan = plans[planIndex];
                    plan.EmittedFaceCount = CountBoundedHullEdgeFaces(
                        hullFaces,
                        plan.Selected.GraphEdgeIndex);
                    if (plan.Active && plan.EmittedFaceCount == 0)
                    {
                        plan.Active = false;
                        plan.HullSuppressed = true;
                        plan.Failure =
                            "combined hull suppressed the edge plane";
                        newlySuppressed++;
                        audit.HullSuppressedEdgeCount++;
                    }
                }

                if (newlySuppressed == 0)
                {
                    break;
                }

                if (iteration == maximumIterations - 1)
                {
                    audit.EdgeEvidence = FormatBoundedAllEdgeEvidence(plans);
                    SetBoundedAllEdgesFailure(
                        audit,
                        BoundedAllEdgesStage.FacetClassification,
                        "combined bounded hull did not stabilize after edge-plane suppression");
                    return audit;
                }
            }

            RefreshBoundedAllEdgePlanCounts(
                audit,
                plans,
                includeEmissionCounts: true);
            audit.EdgeEvidence = FormatBoundedAllEdgeEvidence(plans);

            if (audit.ActiveEdgeCount == 0)
            {
                SetBoundedAllEdgesFailure(
                    audit,
                    BoundedAllEdgesStage.FacetClassification,
                    "all rail-solved edge planes were suppressed by the combined bounded hull");
                return audit;
            }

            audit.Stage = BoundedAllEdgesStage.Preparation;
            if (!TryPrepareBoundedFaces(
                    hullFaces,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    out List<PolygonFace> preparedFaces,
                    out BoundedPreparationAudit preparation,
                    out string preparationBlocker))
            {
                audit.Preparation = preparation;
                SetBoundedAllEdgesFailure(
                    audit,
                    BoundedAllEdgesStage.Preparation,
                    string.IsNullOrEmpty(preparationBlocker)
                        ? "combined bounded hull preparation failed"
                        : preparationBlocker);
                return audit;
            }
            audit.Preparation = preparation;
            audit.Stage = BoundedAllEdgesStage.TopologyCertification;

            EdgeWearTopologyStats topology = AuditEdgeWearTopology(
                preparedFaces,
                minimumStableEdgeLength);
            audit.OpenEdgeCount = topology.OpenEdgeCount;
            audit.NonManifoldEdgeCount = topology.NonManifoldEdgeCount;
            audit.TJunctionCount = topology.TJunctionCount;
            audit.InvalidFaceCount = CountInvalidPlaneCutFaces(
                preparedFaces,
                minimumStableFaceArea);

            Bounds sourceBounds = CalculateFaceBounds(sourceFaces);
            Bounds resultBounds = CalculateFaceBounds(preparedFaces);
            audit.BoundsTolerance = classificationTolerance;
            audit.SourceBoundsMinimum = sourceBounds.min;
            audit.SourceBoundsMaximum = sourceBounds.max;
            audit.ResultBoundsMinimum = resultBounds.min;
            audit.ResultBoundsMaximum = resultBounds.max;
            audit.BoundsMinimumMargin =
                resultBounds.min - sourceBounds.min;
            audit.BoundsMaximumMargin =
                sourceBounds.max - resultBounds.max;
            audit.BoundsValid = ArePlaneCutBoundsContained(
                sourceBounds,
                resultBounds,
                classificationTolerance)
                ? 1
                : 0;

            BoundedSingleEdgeAuditResult globalAudit =
                new BoundedSingleEdgeAuditResult
                {
                    SourceViolatingPlaneFace = -1,
                    SourceViolatingVertexFace = -1,
                    SourceViolatingVertexIndex = -1,
                    ResultViolatingFace = -1,
                    ResultViolatingProvenanceIndex = -1,
                    ResultViolatingVertexIndex = -1,
                    ResultViolatedSourcePlane = -1,
                    ResultConvexityPlaneFace = -1,
                    ResultConvexityPlaneProvenanceIndex = -1,
                    ResultConvexityVertexFace = -1,
                    ResultConvexityVertexProvenanceIndex = -1,
                    ResultConvexityVertexIndex = -1,
                    FirstIntersectionFaceA = -1,
                    FirstIntersectionFaceAProvenanceIndex = -1,
                    FirstIntersectionFaceB = -1,
                    FirstIntersectionFaceBProvenanceIndex = -1,
                    TriangulationFailureFace = -1,
                    TriangulationFailureProvenanceIndex = -1
                };
            AuditBoundedSourceSolidContainment(
                sourceFaces,
                preparedFaces,
                classificationTolerance,
                ref globalAudit);
            AuditBoundedResultConvexity(
                preparedFaces,
                classificationTolerance,
                ref globalAudit);

            List<PolygonFace> attributedSource =
                ClonePolygonFacesForPlaneCutAudit(
                    sourceFaces,
                    assignSourceFaceProvenance: true);
            if (!TryPrepareBoundedFaces(
                    attributedSource,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    out List<PolygonFace> preparedSource,
                    out _,
                    out _))
            {
                SetBoundedAllEdgesFailure(
                    audit,
                    BoundedAllEdgesStage.TopologyCertification,
                    "combined bounded preview could not prepare its source intersection baseline");
                return audit;
            }
            BoundedFaceIntersectionAudit sourceIntersections =
                AuditBoundedFaceIntersections(
                    preparedSource,
                    context,
                    minimumStableFaceArea,
                    classificationTolerance);
            BoundedFaceIntersectionAudit resultIntersections =
                AuditBoundedFaceIntersections(
                    preparedFaces,
                    context,
                    minimumStableFaceArea,
                    classificationTolerance);
            ApplyBoundedFaceIntersectionDelta(
                sourceIntersections,
                resultIntersections,
                ref globalAudit);

            audit.SourceContainmentViolations =
                globalAudit.ResultContainmentViolationCount;
            audit.MaximumSourceContainmentViolation =
                globalAudit.ResultMaximumOutwardDistance;
            audit.ResultConvexityViolations =
                globalAudit.ResultConvexityViolationCount;
            audit.MaximumResultConvexityViolation =
                globalAudit.ResultMaximumConvexityViolation;
            audit.IntroducedInteriorIntersections =
                globalAudit.IntroducedImproperInteriorIntersectionPairCount;

            audit.SourceVolume =
                CalculatePlaneCutPolyhedronVolume(preparedSource);
            audit.ResultVolume =
                CalculatePlaneCutPolyhedronVolume(preparedFaces);
            audit.VolumeRatio = audit.SourceVolume > 0.000000001
                ? audit.ResultVolume / audit.SourceVolume
                : 0.0;
            audit.VolumeDelta =
                audit.ResultVolume - audit.SourceVolume;
            audit.VolumeLowerMargin = audit.VolumeRatio - 0.75;
            audit.VolumeUpperMargin = 1.0 - audit.VolumeRatio;
            audit.VolumeValid =
                audit.SourceVolume > 0.000000001 &&
                audit.ResultVolume > audit.SourceVolume * 0.75 &&
                audit.ResultVolume <= audit.SourceVolume
                    ? 1
                    : 0;

            bool polygonValid =
                audit.MissingBevelFaceCount == 0 &&
                audit.DuplicateBevelFaceCount == 0 &&
                audit.OpenEdgeCount == 0 &&
                audit.NonManifoldEdgeCount == 0 &&
                audit.TJunctionCount == 0 &&
                audit.InvalidFaceCount == 0 &&
                audit.BoundsValid == 1 &&
                audit.SourceContainmentViolations == 0 &&
                audit.ResultConvexityViolations == 0 &&
                audit.IntroducedInteriorIntersections == 0 &&
                audit.VolumeValid == 1;

            audit.Stage = BoundedAllEdgesStage.Triangulation;
            audit.TriangulationAttempted = 1;
            if (!TryTriangulateBoundedPreviewFaces(
                    preparedFaces,
                    minimumStableFaceArea,
                    ref globalAudit,
                    out TriangleSoup soup,
                    out string triangulationBlocker))
            {
                audit.CertificationAudit = globalAudit;
                SetBoundedAllEdgesFailure(
                    audit,
                    BoundedAllEdgesStage.Triangulation,
                    string.IsNullOrEmpty(triangulationBlocker)
                        ? "combined bounded hull triangulation failed"
                        : triangulationBlocker);
                return audit;
            }
            audit.TriangulatedFaceCount =
                globalAudit.TriangulatedFaceCount;

            PlaneCutBevelAuditResult triangleAudit = default;
            AuditPlaneCutPreviewTriangleSoup(
                soup,
                preparedFaces,
                minimumStableEdgeLength,
                ref triangleAudit);
            audit.TriangleAudit = triangleAudit;
            audit.CertificationAudit = globalAudit;
            audit.TriangleCount = triangleAudit.PreviewTriangleCount;
            audit.TriangleSoupValid =
                triangleAudit.PreviewGeometryValid == 1 ? 1 : 0;
            if (!polygonValid ||
                audit.TriangleSoupValid != 1 ||
                globalAudit.BevelRegionRenderValid != 1)
            {
                SetBoundedAllEdgesFailure(
                    audit,
                    BoundedAllEdgesStage.Triangulation,
                    BuildBoundedAllEdgesFailure(audit));
                return audit;
            }

            audit.GeometryValid = 1;
            audit.Stage = BoundedAllEdgesStage.Complete;
            audit.FailureStage = BoundedAllEdgesStage.NotStarted;
            previewSoup = soup;
            return audit;
        }

        private static List<Vector3> BuildBoundedAllEdgePointCloud(
            ChamferTopologyContext context,
            List<BoundedAllEdgePlan> plans)
        {
            HashSet<int> cutVertices = new HashSet<int>();
            for (int planIndex = 0; planIndex < plans.Count; planIndex++)
            {
                BoundedAllEdgePlan plan = plans[planIndex];
                if (!plan.Active)
                {
                    continue;
                }
                EdgeWearGraphEdge edge =
                    context.Graph.Edges[plan.Selected.GraphEdgeIndex];
                cutVertices.Add(edge.VertexA);
                cutVertices.Add(edge.VertexB);
            }

            List<Vector3> points = new List<Vector3>(
                context.Graph.Vertices.Count + plans.Count * 4);
            Dictionary<VertexKey, int> unique =
                new Dictionary<VertexKey, int>();
            for (int vertexIndex = 0;
                 vertexIndex < context.Graph.Vertices.Count;
                 vertexIndex++)
            {
                if (!cutVertices.Contains(vertexIndex))
                {
                    AddBoundedHullPoint(
                        context.Graph.Vertices[vertexIndex].Position,
                        points,
                        unique);
                }
            }

            for (int planIndex = 0; planIndex < plans.Count; planIndex++)
            {
                BoundedAllEdgePlan plan = plans[planIndex];
                if (!plan.Active || plan.Rails == null)
                {
                    continue;
                }
                for (int railIndex = 0;
                     railIndex < plan.Rails.Length;
                     railIndex++)
                {
                    AddBoundedHullPoint(
                        plan.Rails[railIndex].Position,
                        points,
                        unique);
                }
            }
            return points;
        }

        private static void AddBoundedHullPoint(
            Vector3 point,
            List<Vector3> points,
            Dictionary<VertexKey, int> unique)
        {
            VertexKey key = new VertexKey(point);
            if (unique.ContainsKey(key))
            {
                return;
            }
            unique.Add(key, points.Count);
            points.Add(point);
        }

        private static bool TryBuildBoundedConvexHullPlanes(
            List<Vector3> points,
            Vector3 solidCentre,
            float tolerance,
            BoundedAllEdgesAuditResult audit,
            out List<BoundedHullPlane> planes,
            out string blocker)
        {
            planes = new List<BoundedHullPlane>();
            blocker = string.Empty;
            audit.HullTriplesTested = 0;
            audit.HullDegenerateTriples = 0;
            audit.HullNearDegenerateTriples = 0;
            audit.HullNormalizationRejectedTriples = 0;
            audit.HullPostNormalizationInvalidTriples = 0;
            audit.HullPlaneMinimumCrossMagnitude = 0f;
            audit.HullMinimumRejectedCrossMagnitude = 0f;
            audit.HullMaximumRejectedCrossMagnitude = 0f;
            audit.HullMinimumAcceptedCrossMagnitude = 0f;
            audit.HullSupportingTriples = 0;
            audit.HullStraddlingTriples = 0;
            audit.HullPlanesCreated = 0;
            audit.HullPlanesMerged = 0;
            audit.HullPlanesBeforePrune = 0;
            audit.HullPlanesRemovedUnderThreePoints = 0;
            audit.HullInvalidPlanesRemoved = 0;
            audit.HullFirstInvalidPlaneIndex = -1;
            audit.HullFirstInvalidSeedA = -1;
            audit.HullFirstInvalidSeedB = -1;
            audit.HullFirstInvalidSeedC = -1;
            audit.HullFirstInvalidSeedCrossMagnitude = 0f;
            audit.HullFirstInvalidPlaneReason = string.Empty;
            audit.HullPlaneCount = 0;
            audit.HullPlaneEvidence = string.Empty;
            if (points == null || points.Count < 4)
            {
                blocker =
                    "combined bounded point cloud contains fewer than four unique points";
                return false;
            }

            Bounds cloudBounds = new Bounds(points[0], Vector3.zero);
            for (int pointIndex = 1;
                 pointIndex < points.Count;
                 pointIndex++)
            {
                cloudBounds.Encapsulate(points[pointIndex]);
            }
            float cloudExtent = cloudBounds.size.magnitude;
            float minimumCrossMagnitude = Mathf.Max(
                PointMergeDistance,
                PointMergeDistance * Mathf.Max(1f, cloudExtent));
            audit.HullPlaneMinimumCrossMagnitude =
                minimumCrossMagnitude;

            float legacyMinimumCrossMagnitude =
                Mathf.Sqrt(MinimumEdgeLengthSqr);
            float sideTolerance = Mathf.Max(
                tolerance,
                PointMergeDistance * 8f);
            float normalTolerance = 1f - 0.00002f;
            float distanceTolerance = sideTolerance * 2f;
            const float unitNormalTolerance = 0.0001f;
            for (int a = 0; a < points.Count - 2; a++)
            {
                for (int b = a + 1; b < points.Count - 1; b++)
                {
                    for (int c = b + 1; c < points.Count; c++)
                    {
                        audit.HullTriplesTested++;
                        Vector3 rawNormal = Vector3.Cross(
                            points[b] - points[a],
                            points[c] - points[a]);
                        float crossMagnitude = rawNormal.magnitude;
                        if (!IsFinite(rawNormal) ||
                            !IsFiniteFloat(crossMagnitude) ||
                            crossMagnitude <= legacyMinimumCrossMagnitude)
                        {
                            audit.HullDegenerateTriples++;
                            audit.HullNormalizationRejectedTriples++;
                            RecordBoundedHullRejectedCrossMagnitude(
                                audit,
                                crossMagnitude);
                            continue;
                        }
                        if (crossMagnitude <= minimumCrossMagnitude)
                        {
                            audit.HullNearDegenerateTriples++;
                            audit.HullNormalizationRejectedTriples++;
                            RecordBoundedHullRejectedCrossMagnitude(
                                audit,
                                crossMagnitude);
                            continue;
                        }

                        Vector3 normal = rawNormal / crossMagnitude;
                        float normalizedMagnitude = normal.magnitude;
                        if (!IsFinite(normal) ||
                            !IsFiniteFloat(normalizedMagnitude) ||
                            Mathf.Abs(normalizedMagnitude - 1f) >
                                unitNormalTolerance)
                        {
                            audit.HullPostNormalizationInvalidTriples++;
                            RecordBoundedHullRejectedCrossMagnitude(
                                audit,
                                crossMagnitude);
                            continue;
                        }
                        if (audit.HullMinimumAcceptedCrossMagnitude <= 0f ||
                            crossMagnitude <
                                audit.HullMinimumAcceptedCrossMagnitude)
                        {
                            audit.HullMinimumAcceptedCrossMagnitude =
                                crossMagnitude;
                        }

                        float distance = Vector3.Dot(normal, points[a]);
                        if (!IsFiniteFloat(distance))
                        {
                            audit.HullPostNormalizationInvalidTriples++;
                            RecordBoundedHullRejectedCrossMagnitude(
                                audit,
                                crossMagnitude);
                            continue;
                        }

                        float maximum = float.NegativeInfinity;
                        float minimum = float.PositiveInfinity;
                        for (int pointIndex = 0;
                             pointIndex < points.Count;
                             pointIndex++)
                        {
                            float side =
                                Vector3.Dot(normal, points[pointIndex]) -
                                distance;
                            maximum = Mathf.Max(maximum, side);
                            minimum = Mathf.Min(minimum, side);
                        }

                        if (maximum > sideTolerance &&
                            minimum < -sideTolerance)
                        {
                            audit.HullStraddlingTriples++;
                            continue;
                        }
                        audit.HullSupportingTriples++;

                        if (minimum >= -sideTolerance)
                        {
                            normal = -normal;
                            distance = -distance;
                        }
                        if (Vector3.Dot(normal, solidCentre) - distance >
                            sideTolerance)
                        {
                            normal = -normal;
                            distance = -distance;
                        }

                        HashSet<int> supportingPoints =
                            new HashSet<int>();
                        for (int pointIndex = 0;
                             pointIndex < points.Count;
                             pointIndex++)
                        {
                            if (Mathf.Abs(
                                    Vector3.Dot(
                                        normal,
                                        points[pointIndex]) -
                                    distance) <= distanceTolerance)
                            {
                                supportingPoints.Add(pointIndex);
                            }
                        }
                        if (supportingPoints.Count < 3)
                        {
                            audit.HullPostNormalizationInvalidTriples++;
                            RecordBoundedHullRejectedCrossMagnitude(
                                audit,
                                crossMagnitude);
                            continue;
                        }

                        int existing = -1;
                        for (int planeIndex = 0;
                             planeIndex < planes.Count;
                             planeIndex++)
                        {
                            BoundedHullPlane candidate = planes[planeIndex];
                            if (Vector3.Dot(candidate.Normal, normal) >=
                                    normalTolerance &&
                                Mathf.Abs(candidate.Distance - distance) <=
                                    distanceTolerance)
                            {
                                existing = planeIndex;
                                break;
                            }
                        }

                        BoundedHullPlane plane;
                        if (existing >= 0)
                        {
                            plane = planes[existing];
                            plane.MinimumMergedSeedCrossMagnitude =
                                Mathf.Min(
                                    plane.MinimumMergedSeedCrossMagnitude,
                                    crossMagnitude);
                            plane.MaximumMergedSeedCrossMagnitude =
                                Mathf.Max(
                                    plane.MaximumMergedSeedCrossMagnitude,
                                    crossMagnitude);
                            audit.HullPlanesMerged++;
                        }
                        else
                        {
                            plane = new BoundedHullPlane
                            {
                                Normal = normal,
                                Distance = distance,
                                SeedPointA = a,
                                SeedPointB = b,
                                SeedPointC = c,
                                SeedCrossMagnitude = crossMagnitude,
                                MinimumMergedSeedCrossMagnitude =
                                    crossMagnitude,
                                MaximumMergedSeedCrossMagnitude =
                                    crossMagnitude
                            };
                            planes.Add(plane);
                            audit.HullPlanesCreated++;
                        }

                        foreach (int pointIndex in supportingPoints)
                        {
                            plane.PointIndices.Add(pointIndex);
                        }
                    }
                }
            }

            audit.HullPlanesBeforePrune = planes.Count;
            for (int planeIndex = planes.Count - 1;
                 planeIndex >= 0;
                 planeIndex--)
            {
                if (planes[planeIndex].PointIndices.Count < 3)
                {
                    planes.RemoveAt(planeIndex);
                    audit.HullPlanesRemovedUnderThreePoints++;
                }
            }

            List<int> invalidPlaneIndices = new List<int>();
            for (int planeIndex = 0;
                 planeIndex < planes.Count;
                 planeIndex++)
            {
                BoundedHullPlane plane = planes[planeIndex];
                if (TryValidateBoundedHullPlaneInvariant(
                        points,
                        plane,
                        minimumCrossMagnitude,
                        distanceTolerance,
                        unitNormalTolerance,
                        out string invalidReason))
                {
                    continue;
                }

                invalidPlaneIndices.Add(planeIndex);
                if (audit.HullFirstInvalidPlaneIndex < 0)
                {
                    audit.HullFirstInvalidPlaneIndex = planeIndex;
                    audit.HullFirstInvalidSeedA = plane.SeedPointA;
                    audit.HullFirstInvalidSeedB = plane.SeedPointB;
                    audit.HullFirstInvalidSeedC = plane.SeedPointC;
                    audit.HullFirstInvalidSeedCrossMagnitude =
                        plane.SeedCrossMagnitude;
                    audit.HullFirstInvalidPlaneReason = invalidReason;
                }
            }
            for (int invalidIndex = invalidPlaneIndices.Count - 1;
                 invalidIndex >= 0;
                 invalidIndex--)
            {
                planes.RemoveAt(invalidPlaneIndices[invalidIndex]);
                audit.HullInvalidPlanesRemoved++;
            }

            audit.HullPlaneCount = planes.Count;
            audit.HullPlaneEvidence =
                FormatBoundedHullPlaneEvidence(points, planes);
            if (audit.HullInvalidPlanesRemoved > 0)
            {
                blocker =
                    "combined bounded hull plane invariants failed: " +
                    audit.HullFirstInvalidPlaneReason;
                return false;
            }
            if (planes.Count < 4)
            {
                blocker =
                    "combined bounded point cloud did not produce a closed convex hull plane set";
                return false;
            }
            return true;
        }

        private static void RecordBoundedHullRejectedCrossMagnitude(
            BoundedAllEdgesAuditResult audit,
            float crossMagnitude)
        {
            if (!IsFiniteFloat(crossMagnitude) || crossMagnitude < 0f)
            {
                return;
            }
            if (audit.HullMinimumRejectedCrossMagnitude <= 0f ||
                crossMagnitude < audit.HullMinimumRejectedCrossMagnitude)
            {
                audit.HullMinimumRejectedCrossMagnitude = crossMagnitude;
            }
            audit.HullMaximumRejectedCrossMagnitude = Mathf.Max(
                audit.HullMaximumRejectedCrossMagnitude,
                crossMagnitude);
        }

        private static bool TryValidateBoundedHullPlaneInvariant(
            List<Vector3> points,
            BoundedHullPlane plane,
            float minimumCrossMagnitude,
            float distanceTolerance,
            float unitNormalTolerance,
            out string reason)
        {
            reason = string.Empty;
            if (plane == null)
            {
                reason = "plane record is null";
                return false;
            }
            if (!IsFinite(plane.Normal))
            {
                reason = "plane normal is non-finite";
                return false;
            }
            float normalMagnitude = plane.Normal.magnitude;
            if (!IsFiniteFloat(normalMagnitude) ||
                Mathf.Abs(normalMagnitude - 1f) > unitNormalTolerance)
            {
                reason =
                    "plane normal is not finite unit length (magnitude=" +
                    normalMagnitude.ToString("G9") + ")";
                return false;
            }
            if (!IsFiniteFloat(plane.Distance))
            {
                reason = "plane distance is non-finite";
                return false;
            }
            if (plane.PointIndices == null ||
                plane.PointIndices.Count < 3)
            {
                reason = "plane has fewer than three supporting points";
                return false;
            }

            foreach (int pointIndex in plane.PointIndices)
            {
                if (pointIndex < 0 || pointIndex >= points.Count)
                {
                    reason = "plane references an out-of-range support point";
                    return false;
                }
                float residual = Mathf.Abs(
                    Vector3.Dot(plane.Normal, points[pointIndex]) -
                    plane.Distance);
                if (!IsFiniteFloat(residual) ||
                    residual > distanceTolerance * 1.25f)
                {
                    reason =
                        "plane support point exceeds residual tolerance";
                    return false;
                }
            }

            int[] support = new int[plane.PointIndices.Count];
            plane.PointIndices.CopyTo(support);
            float maximumCrossMagnitude = 0f;
            for (int a = 0; a < support.Length - 2; a++)
            {
                for (int b = a + 1; b < support.Length - 1; b++)
                {
                    for (int c = b + 1; c < support.Length; c++)
                    {
                        float crossMagnitude = Vector3.Cross(
                            points[support[b]] - points[support[a]],
                            points[support[c]] - points[support[a]])
                            .magnitude;
                        maximumCrossMagnitude = Mathf.Max(
                            maximumCrossMagnitude,
                            crossMagnitude);
                        if (crossMagnitude > minimumCrossMagnitude)
                        {
                            return true;
                        }
                    }
                }
            }

            reason =
                "plane supporting points have insufficient projected rank " +
                "(maximumCross=" +
                maximumCrossMagnitude.ToString("G9") + ")";
            return false;
        }

        private static bool TryBuildBoundedHullFaces(
            List<Vector3> points,
            List<BoundedHullPlane> planes,
            List<PolygonFace> sourceFaces,
            List<BoundedAllEdgePlan> plans,
            float tolerance,
            float minimumStableFaceArea,
            BoundedAllEdgesAuditResult audit,
            out List<PolygonFace> faces,
            out int sourceFaceCount,
            out int bevelFaceCount,
            out int vertexJunctionFaceCount,
            out string blocker)
        {
            faces = new List<PolygonFace>(planes.Count);
            sourceFaceCount = 0;
            bevelFaceCount = 0;
            vertexJunctionFaceCount = 0;
            blocker = string.Empty;
            for (int planeIndex = 0;
                 planeIndex < planes.Count;
                 planeIndex++)
            {
                BoundedHullPlane plane = planes[planeIndex];
                audit.HullPlanesAttempted++;
                if (!TryOrderBoundedHullFacet(
                        points,
                        plane,
                        out List<Vector3> vertices))
                {
                    blocker =
                        "a combined bounded hull plane could not produce a stable convex boundary";
                    audit.FailureStage =
                        BoundedAllEdgesStage.FacetOrdering;
                    audit.HullFailurePlaneIndex = planeIndex;
                    audit.HullFailurePlaneNormal = plane.Normal;
                    audit.HullFailurePlaneDistance = plane.Distance;
                    audit.HullFailurePlanePointCount =
                        plane.PointIndices.Count;
                    audit.HullFailureReason = blocker;
                    return false;
                }

                int orderedVertexCount = vertices.Count;
                audit.Stage = BoundedAllEdgesStage.FacetSanitation;
                List<Vector3> sanitized = SanitizePolygon(
                    vertices,
                    plane.Normal);
                float facetArea = sanitized.Count >= 3
                    ? CalculatePolygonArea(sanitized)
                    : 0f;
                if (sanitized.Count < 3 ||
                    facetArea <= minimumStableFaceArea)
                {
                    blocker =
                        "a combined bounded hull facet collapsed during polygon sanitation";
                    audit.FailureStage =
                        BoundedAllEdgesStage.FacetSanitation;
                    audit.HullFailurePlaneIndex = planeIndex;
                    audit.HullFailurePlaneNormal = plane.Normal;
                    audit.HullFailurePlaneDistance = plane.Distance;
                    audit.HullFailurePlanePointCount =
                        plane.PointIndices.Count;
                    audit.HullFailureOrderedVertexCount =
                        orderedVertexCount;
                    audit.HullFailureSanitizedVertexCount =
                        sanitized.Count;
                    audit.HullFailureFacetArea = facetArea;
                    audit.HullFailureReason = blocker;
                    return false;
                }

                Vector3 measuredNormal =
                    CalculatePolygonNormal(sanitized);
                if (Vector3.Dot(measuredNormal, plane.Normal) < 0f)
                {
                    sanitized.Reverse();
                    measuredNormal = -measuredNormal;
                }

                audit.Stage = BoundedAllEdgesStage.FacetClassification;
                bool convexityValid =
                    IsFinite(measuredNormal) &&
                    IsBoundedPolygonConvex(
                        BuildBoundedConvexityCheckLoop(sanitized),
                        plane.Normal);
                if (!convexityValid)
                {
                    blocker =
                        "a combined bounded hull facet failed convex polygon certification";
                    audit.FailureStage =
                        BoundedAllEdgesStage.FacetClassification;
                    audit.HullFailurePlaneIndex = planeIndex;
                    audit.HullFailurePlaneNormal = plane.Normal;
                    audit.HullFailurePlaneDistance = plane.Distance;
                    audit.HullFailurePlanePointCount =
                        plane.PointIndices.Count;
                    audit.HullFailureOrderedVertexCount =
                        orderedVertexCount;
                    audit.HullFailureSanitizedVertexCount =
                        sanitized.Count;
                    audit.HullFailureFacetArea = facetArea;
                    audit.HullFailureConvexityValid = 0;
                    audit.HullFailureReason = blocker;
                    return false;
                }

                PolygonFaceFeature feature = PolygonFaceFeature.Base;
                float featureStrength = 0f;
                PolygonFaceProvenanceKind provenanceKind =
                    PolygonFaceProvenanceKind.VertexJunctionPlane;
                int provenanceIndex = planeIndex;

                int sourceFaceIndex = FindBoundedHullSourcePlaneMatch(
                    sourceFaces,
                    plane.Normal,
                    plane.Distance,
                    tolerance);
                if (sourceFaceIndex >= 0)
                {
                    PolygonFace source = sourceFaces[sourceFaceIndex];
                    feature = source.Feature;
                    featureStrength = source.FeatureStrength;
                    provenanceKind =
                        PolygonFaceProvenanceKind.SourceFace;
                    provenanceIndex = sourceFaceIndex;
                    sourceFaceCount++;
                }
                else
                {
                    int edgePlanIndex = FindBoundedHullEdgePlaneMatch(
                        plans,
                        plane.Normal,
                        plane.Distance,
                        tolerance);
                    if (edgePlanIndex >= 0)
                    {
                        BoundedAllEdgePlan plan = plans[edgePlanIndex];
                        feature =
                            PolygonFaceFeature.ConvexEdgeWear;
                        featureStrength =
                            plan.Selected.Candidate.Strength;
                        provenanceKind =
                            PolygonFaceProvenanceKind.BoundedEdgeBevel;
                        provenanceIndex =
                            plan.Selected.GraphEdgeIndex;
                        bevelFaceCount++;
                    }
                    else
                    {
                        feature =
                            PolygonFaceFeature.ConvexEdgeWear;
                        featureStrength =
                            CalculateBoundedHullJunctionStrength(
                                plans,
                                vertices,
                                tolerance);
                        vertexJunctionFaceCount++;
                    }
                }

                PolygonFace face = new PolygonFace(
                    sanitized,
                    plane.Normal,
                    feature,
                    featureStrength,
                    provenanceKind,
                    provenanceIndex);
                faces.Add(face);
                audit.HullFacesCompleted++;
            }

            if (faces.Count < 4)
            {
                blocker =
                    "combined bounded hull emitted fewer than four faces";
                audit.FailureStage =
                    BoundedAllEdgesStage.FacetClassification;
                audit.HullFailureReason = blocker;
                return false;
            }

            audit.HullFailurePlaneIndex = -1;
            audit.HullFailureReason = string.Empty;
            audit.HullFailureConvexityValid = 1;
            return true;
        }

        private static bool TryOrderBoundedHullFacet(
            List<Vector3> points,
            BoundedHullPlane plane,
            out List<Vector3> vertices)
        {
            vertices = null;
            Vector3 reference = Mathf.Abs(plane.Normal.y) < 0.9f
                ? Vector3.up
                : Vector3.right;
            Vector3 axisU = Vector3.Cross(reference, plane.Normal);
            if (axisU.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                reference = Vector3.forward;
                axisU = Vector3.Cross(reference, plane.Normal);
            }
            if (axisU.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return false;
            }
            axisU.Normalize();
            Vector3 axisV = Vector3.Cross(plane.Normal, axisU).normalized;

            List<BoundedHullProjectedPoint> projected =
                new List<BoundedHullProjectedPoint>(
                    plane.PointIndices.Count);
            foreach (int pointIndex in plane.PointIndices)
            {
                Vector3 point = points[pointIndex];
                projected.Add(new BoundedHullProjectedPoint(
                    pointIndex,
                    new Vector2(
                        Vector3.Dot(point, axisU),
                        Vector3.Dot(point, axisV))));
            }
            projected.Sort((left, right) =>
            {
                int x = left.Position.x.CompareTo(right.Position.x);
                return x != 0
                    ? x
                    : left.Position.y.CompareTo(right.Position.y);
            });

            List<BoundedHullProjectedPoint> lower =
                new List<BoundedHullProjectedPoint>();
            for (int i = 0; i < projected.Count; i++)
            {
                while (lower.Count >= 2 &&
                    BoundedHullCross(
                        lower[lower.Count - 2].Position,
                        lower[lower.Count - 1].Position,
                        projected[i].Position) <= 0f)
                {
                    lower.RemoveAt(lower.Count - 1);
                }
                lower.Add(projected[i]);
            }

            List<BoundedHullProjectedPoint> upper =
                new List<BoundedHullProjectedPoint>();
            for (int i = projected.Count - 1; i >= 0; i--)
            {
                while (upper.Count >= 2 &&
                    BoundedHullCross(
                        upper[upper.Count - 2].Position,
                        upper[upper.Count - 1].Position,
                        projected[i].Position) <= 0f)
                {
                    upper.RemoveAt(upper.Count - 1);
                }
                upper.Add(projected[i]);
            }

            if (lower.Count < 2 || upper.Count < 2)
            {
                return false;
            }
            lower.RemoveAt(lower.Count - 1);
            upper.RemoveAt(upper.Count - 1);
            lower.AddRange(upper);
            if (lower.Count < 3)
            {
                return false;
            }

            vertices = new List<Vector3>(lower.Count);
            for (int i = 0; i < lower.Count; i++)
            {
                vertices.Add(points[lower[i].PointIndex]);
            }
            Vector3 measured = CalculatePolygonNormal(vertices);
            if (Vector3.Dot(measured, plane.Normal) < 0f)
            {
                vertices.Reverse();
            }
            return true;
        }

        private static float BoundedHullCross(
            Vector2 a,
            Vector2 b,
            Vector2 c)
        {
            Vector2 ab = b - a;
            Vector2 ac = c - a;
            return ab.x * ac.y - ab.y * ac.x;
        }

        private static int FindBoundedHullSourcePlaneMatch(
            List<PolygonFace> sourceFaces,
            Vector3 normal,
            float distance,
            float tolerance)
        {
            int best = -1;
            float bestError = float.PositiveInfinity;
            float distanceTolerance = tolerance * 2f;
            for (int faceIndex = 0;
                 faceIndex < sourceFaces.Count;
                 faceIndex++)
            {
                PolygonFace face = sourceFaces[faceIndex];
                if (face == null || face.Vertices.Count < 3)
                {
                    continue;
                }
                Vector3 sourceNormal = face.Normal.normalized;
                float normalError = 1f - Vector3.Dot(sourceNormal, normal);
                float planeError = Mathf.Abs(
                    Vector3.Dot(sourceNormal, face.Vertices[0]) -
                    distance);
                float error = normalError + planeError;
                if (normalError <= 0.00005f &&
                    planeError <= distanceTolerance &&
                    error < bestError)
                {
                    best = faceIndex;
                    bestError = error;
                }
            }
            return best;
        }

        private static int FindBoundedHullEdgePlaneMatch(
            List<BoundedAllEdgePlan> plans,
            Vector3 normal,
            float distance,
            float tolerance)
        {
            int best = -1;
            float bestError = float.PositiveInfinity;
            float distanceTolerance = tolerance * 2f;
            for (int planIndex = 0;
                 planIndex < plans.Count;
                 planIndex++)
            {
                BoundedAllEdgePlan plan = plans[planIndex];
                if (!plan.Active)
                {
                    continue;
                }
                float normalError =
                    1f - Vector3.Dot(plan.PlaneNormal, normal);
                float planeError = Mathf.Abs(
                    plan.PlaneDistance - distance);
                float error = normalError + planeError;
                if (normalError <= 0.00005f &&
                    planeError <= distanceTolerance &&
                    error < bestError)
                {
                    best = planIndex;
                    bestError = error;
                }
            }
            return best;
        }

        private static float CalculateBoundedHullJunctionStrength(
            List<BoundedAllEdgePlan> plans,
            List<Vector3> vertices,
            float tolerance)
        {
            float total = 0f;
            int count = 0;
            float toleranceSqr = tolerance * tolerance * 16f;
            for (int planIndex = 0;
                 planIndex < plans.Count;
                 planIndex++)
            {
                BoundedAllEdgePlan plan = plans[planIndex];
                if (!plan.Active || plan.Rails == null)
                {
                    continue;
                }
                bool touches = false;
                for (int vertexIndex = 0;
                     vertexIndex < vertices.Count && !touches;
                     vertexIndex++)
                {
                    for (int railIndex = 0;
                         railIndex < plan.Rails.Length;
                         railIndex++)
                    {
                        if ((vertices[vertexIndex] -
                                plan.Rails[railIndex].Position).sqrMagnitude <=
                            toleranceSqr)
                        {
                            touches = true;
                            break;
                        }
                    }
                }
                if (touches)
                {
                    total += plan.Selected.Candidate.Strength;
                    count++;
                }
            }
            return count > 0 ? Mathf.Clamp01(total / count) : 0f;
        }

        private static int CountBoundedHullEdgeFaces(
            List<PolygonFace> faces,
            int graphEdgeIndex)
        {
            int count = 0;
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face.ProvenanceKind ==
                        PolygonFaceProvenanceKind.BoundedEdgeBevel &&
                    face.ProvenanceIndex == graphEdgeIndex)
                {
                    count++;
                }
            }
            return count;
        }

        private static string FormatBoundedAllEdgeEvidence(
            List<BoundedAllEdgePlan> plans)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('{');
            for (int planIndex = 0;
                 planIndex < plans.Count;
                 planIndex++)
            {
                if (planIndex > 0)
                {
                    builder.Append('|');
                }
                BoundedAllEdgePlan plan = plans[planIndex];
                builder.Append("ordinal=");
                builder.Append(plan.Ordinal);
                builder.Append(",edge=");
                builder.Append(plan.Selected.GraphEdgeIndex);
                builder.Append(",candidate=");
                builder.Append(plan.Selected.CandidateIndex);
                builder.Append(",state=");
                if (plan.Active)
                {
                    builder.Append("active");
                }
                else if (plan.HullSuppressed)
                {
                    builder.Append("suppressed");
                }
                else
                {
                    builder.Append("rejected");
                }
                builder.Append(",classification=");
                builder.Append(plan.Classification);
                builder.Append(",faces=");
                builder.Append(plan.ClassificationEvidence.SourceFaceA);
                builder.Append('/');
                builder.Append(plan.ClassificationEvidence.SourceFaceB);
                builder.Append(",vertices=");
                builder.Append(plan.EdgeVertexA);
                builder.Append('/');
                builder.Append(plan.EdgeVertexB);
                builder.Append(",sourceA=");
                builder.Append(FormatBoundedAllEdgeVector(plan.SourcePositionA));
                builder.Append(",sourceB=");
                builder.Append(FormatBoundedAllEdgeVector(plan.SourcePositionB));
                builder.Append(",normalDot=");
                builder.Append(
                    plan.ClassificationEvidence.NormalDot.ToString("G9"));
                builder.Append(",dihedral=");
                builder.Append(
                    plan.ClassificationEvidence.DihedralDegrees
                        .ToString("G9"));
                builder.Append(",interiorAB=");
                builder.Append(
                    plan.ClassificationEvidence.FaceAInteriorAgainstFaceB
                        .ToString("G9"));
                builder.Append('/');
                builder.Append(
                    plan.ClassificationEvidence.FaceBInteriorAgainstFaceA
                        .ToString("G9"));
                builder.Append(",solidOwnerSides=");
                builder.Append(
                    plan.ClassificationEvidence.SolidCentreAgainstFaceA
                        .ToString("G9"));
                builder.Append('/');
                builder.Append(
                    plan.ClassificationEvidence.SolidCentreAgainstFaceB
                        .ToString("G9"));
                builder.Append(",width=");
                builder.Append(plan.SolvedWidth.ToString("G9"));
                builder.Append(",attempts=");
                builder.Append(plan.WidthAttempts);
                builder.Append(",snap=");
                builder.Append(plan.MaximumBoundarySnap.ToString("G9"));
                builder.Append(",bevelPlaneNormal=");
                builder.Append(FormatBoundedAllEdgeVector(plan.PlaneNormal));
                builder.Append(",bevelPlaneDistance=");
                builder.Append(plan.PlaneDistance.ToString("G9"));
                builder.Append(",planeResidual=");
                builder.Append(plan.MaximumPlaneResidual.ToString("G9"));
                builder.Append(",planeSides=");
                builder.Append(plan.SolidCentreSide.ToString("G9"));
                builder.Append('/');
                builder.Append(plan.SourceEdgeSideA.ToString("G9"));
                builder.Append('/');
                builder.Append(plan.SourceEdgeSideB.ToString("G9"));
                builder.Append(",rails=");
                if (plan.Rails == null)
                {
                    builder.Append("none");
                }
                else
                {
                    for (int railIndex = 0;
                         railIndex < plan.Rails.Length;
                         railIndex++)
                    {
                        if (railIndex > 0)
                        {
                            builder.Append(';');
                        }
                        BoundedIsolatedRailPoint rail =
                            plan.Rails[railIndex];
                        builder.Append(railIndex);
                        builder.Append('@');
                        builder.Append(
                            FormatBoundedAllEdgeVector(rail.Position));
                        builder.Append("[vertex=");
                        builder.Append(rail.SourceVertexIndex);
                        builder.Append(",adjacentEdge=");
                        builder.Append(rail.AdjacentGraphEdgeIndex);
                        builder.Append(",targetGraphFace=");
                        builder.Append(rail.TargetGraphFaceIndex);
                        builder.Append(",targetSourceFace=");
                        builder.Append(rail.TargetSourceFaceIndex);
                        builder.Append(",snap=");
                        builder.Append(
                            rail.BoundarySnapDistance.ToString("G9"));
                        builder.Append(']');
                    }
                }
                builder.Append(",emittedFaces=");
                builder.Append(plan.EmittedFaceCount);
                if (!string.IsNullOrEmpty(plan.Failure))
                {
                    builder.Append(",reason=");
                    builder.Append(plan.Failure.Replace('|', '/'));
                }
            }
            builder.Append('}');
            return builder.ToString();
        }

        private static string FormatBoundedHullPlaneEvidence(
            List<Vector3> points,
            List<BoundedHullPlane> planes)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('{');
            if (planes != null)
            {
                for (int planeIndex = 0;
                     planeIndex < planes.Count;
                     planeIndex++)
                {
                    if (planeIndex > 0)
                    {
                        builder.Append('|');
                    }
                    BoundedHullPlane plane = planes[planeIndex];
                    builder.Append(planeIndex);
                    builder.Append(":normal=");
                    builder.Append(
                        FormatBoundedAllEdgeVector(plane.Normal));
                    builder.Append(",normalMagnitude=");
                    builder.Append(plane.Normal.magnitude.ToString("G9"));
                    builder.Append(",distance=");
                    builder.Append(plane.Distance.ToString("G9"));
                    builder.Append(",points=");
                    builder.Append(plane.PointIndices.Count);
                    builder.Append(",seed=");
                    builder.Append(plane.SeedPointA);
                    builder.Append('/');
                    builder.Append(plane.SeedPointB);
                    builder.Append('/');
                    builder.Append(plane.SeedPointC);
                    builder.Append(",seedCross=");
                    builder.Append(
                        plane.SeedCrossMagnitude.ToString("G9"));
                    builder.Append(",mergedCrossRange=");
                    builder.Append(
                        plane.MinimumMergedSeedCrossMagnitude
                            .ToString("G9"));
                    builder.Append('-');
                    builder.Append(
                        plane.MaximumMergedSeedCrossMagnitude
                            .ToString("G9"));
                    builder.Append(",support={");
                    List<int> supportIndices =
                        new List<int>(plane.PointIndices);
                    supportIndices.Sort();
                    bool first = true;
                    for (int supportIndex = 0;
                         supportIndex < supportIndices.Count;
                         supportIndex++)
                    {
                        int pointIndex = supportIndices[supportIndex];
                        if (!first)
                        {
                            builder.Append('/');
                        }
                        first = false;
                        builder.Append(pointIndex);
                        if (points != null &&
                            pointIndex >= 0 &&
                            pointIndex < points.Count)
                        {
                            builder.Append('@');
                            builder.Append(
                                FormatBoundedAllEdgeVector(
                                    points[pointIndex]));
                        }
                    }
                    builder.Append("}");
                }
            }
            builder.Append('}');
            return builder.ToString();
        }

        private static string FormatBoundedHullPointEvidence(
            List<Vector3> points)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('{');
            for (int pointIndex = 0;
                 pointIndex < points.Count;
                 pointIndex++)
            {
                if (pointIndex > 0)
                {
                    builder.Append('|');
                }
                builder.Append(pointIndex);
                builder.Append(':');
                builder.Append(FormatBoundedAllEdgeVector(points[pointIndex]));
            }
            builder.Append('}');
            return builder.ToString();
        }

        private static string FormatBoundedHullFaceEvidence(
            List<PolygonFace> faces)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('{');
            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                if (faceIndex > 0)
                {
                    builder.Append('|');
                }
                PolygonFace face = faces[faceIndex];
                builder.Append(faceIndex);
                builder.Append(':');
                builder.Append(face.ProvenanceKind);
                builder.Append(':');
                builder.Append(face.ProvenanceIndex);
                builder.Append("[vertices=");
                builder.Append(face.Vertices.Count);
                builder.Append(",normal=");
                builder.Append(FormatBoundedAllEdgeVector(face.Normal));
                builder.Append(",distance=");
                builder.Append(
                    Vector3.Dot(face.Normal, face.Vertices[0])
                        .ToString("G9"));
                builder.Append(",area=");
                builder.Append(CalculatePolygonArea(face.Vertices)
                    .ToString("G9"));
                builder.Append(']');
            }
            builder.Append('}');
            return builder.ToString();
        }

        private static string FormatBoundedAllEdgeVector(Vector3 value)
        {
            return "(" + value.x.ToString("G9") + "/" +
                value.y.ToString("G9") + "/" +
                value.z.ToString("G9") + ")";
        }

        private static string BuildBoundedAllEdgesFailure(
            BoundedAllEdgesAuditResult audit)
        {
            if (audit.MissingBevelFaceCount > 0 ||
                audit.DuplicateBevelFaceCount > 0)
            {
                return "combined bounded hull did not retain exactly one face for every active edge plane";
            }
            if (audit.OpenEdgeCount > 0 ||
                audit.NonManifoldEdgeCount > 0 ||
                audit.TJunctionCount > 0 ||
                audit.InvalidFaceCount > 0)
            {
                return "combined bounded hull failed polygon topology certification";
            }
            if (audit.BoundsValid != 1 ||
                audit.SourceContainmentViolations > 0)
            {
                return "combined bounded hull escaped the original source solid";
            }
            if (audit.ResultConvexityViolations > 0)
            {
                return "combined bounded hull failed global convexity certification";
            }
            if (audit.IntroducedInteriorIntersections > 0)
            {
                return "combined bounded hull introduced an improper interior face intersection";
            }
            if (audit.VolumeValid != 1)
            {
                return "combined bounded hull failed subtractive retained-volume certification";
            }
            if (audit.TriangleSoupValid != 1)
            {
                return "combined bounded hull triangle soup failed certification";
            }
            return "combined bounded hull failed an unspecified certification gate";
        }
    }
}
