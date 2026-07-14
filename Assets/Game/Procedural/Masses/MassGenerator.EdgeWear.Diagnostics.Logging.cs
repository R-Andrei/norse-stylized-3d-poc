using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear diagnostic logging

        private static int BuildChamferDiagnosticGeometrySignature(
            List<ChamferProvisionalFaceRecord> records)
        {
            unchecked
            {
                int hash = 17;
                if (records == null)
                {
                    return hash;
                }
                hash = hash * 31 + records.Count;
                for (int recordIndex = 0;
                     recordIndex < records.Count;
                     recordIndex++)
                {
                    ChamferProvisionalFaceRecord record = records[recordIndex];
                    hash = hash * 31 + (int)record.Kind;
                    PolygonFace face = record.Face;
                    if (face == null || face.Vertices == null)
                    {
                        continue;
                    }
                    hash = hash * 31 + face.Vertices.Count;
                    for (int vertexIndex = 0;
                         vertexIndex < face.Vertices.Count;
                         vertexIndex++)
                    {
                        hash = hash * 31 +
                            new VertexKey(
                                face.Vertices[vertexIndex]).GetHashCode();
                    }
                }
                return hash;
            }
        }

        private static void AppendChamferCompactDiagnostic(
            ref string target,
            string value,
            int maximumEntries)
        {
            if (string.IsNullOrEmpty(value) || maximumEntries <= 0)
            {
                return;
            }
            int existingEntries = string.IsNullOrEmpty(target)
                ? 0
                : target.Split(';').Length;
            if (existingEntries >= maximumEntries)
            {
                return;
            }
            target = string.IsNullOrEmpty(target)
                ? value
                : target + ";" + value;
        }

        private static void LogChamferNoStackTrace(
            string message,
            bool warning)
        {
#if UNITY_EDITOR
            Debug.LogFormat(
                warning ? LogType.Warning : LogType.Log,
                LogOption.NoStacktrace,
                null,
                "{0}",
                message);
#endif
        }

        private static string GetChamferGenerationCaller()
        {
#if UNITY_EDITOR
            System.Diagnostics.StackTrace stack =
                new System.Diagnostics.StackTrace(1, false);
            System.Diagnostics.StackFrame[] frames = stack.GetFrames();
            if (frames == null)
            {
                return string.Empty;
            }
            for (int i = 0; i < frames.Length; i++)
            {
                System.Reflection.MethodBase method =
                    frames[i].GetMethod();
                if (method == null || method.DeclaringType == null ||
                    method.DeclaringType.Name != "GeneratedMass")
                {
                    continue;
                }
                if (method.Name == "OnValidate" ||
                    method.Name == "OnEnable")
                {
                    return method.Name;
                }
            }
#endif
            return string.Empty;
        }

        private static bool ShouldSuppressChamferCompactSummary(
            int geometrySignature,
            string message)
        {
            if (geometrySignature == 0)
            {
                return false;
            }
            string origin = GetChamferGenerationCaller();
            long now = DateTime.UtcNow.Ticks;
            if (!string.IsNullOrEmpty(origin) &&
                LastChamferCompactSummaryByGeometry.TryGetValue(
                    geometrySignature,
                    out string previous) &&
                previous == message &&
                LastChamferCompactSummaryTicksByGeometry.TryGetValue(
                    geometrySignature,
                    out long previousTicks) &&
                LastChamferCompactSummaryOriginByGeometry.TryGetValue(
                    geometrySignature,
                    out string previousOrigin) &&
                !string.IsNullOrEmpty(previousOrigin) &&
                previousOrigin != origin &&
                ((previousOrigin == "OnValidate" && origin == "OnEnable") ||
                 (previousOrigin == "OnEnable" && origin == "OnValidate")) &&
                now - previousTicks <= TimeSpan.TicksPerSecond * 2)
            {
                return true;
            }
            if (LastChamferCompactSummaryByGeometry.Count >= 512)
            {
                LastChamferCompactSummaryByGeometry.Clear();
                LastChamferCompactSummaryTicksByGeometry.Clear();
                LastChamferCompactSummaryOriginByGeometry.Clear();
            }
            LastChamferCompactSummaryByGeometry[geometrySignature] = message;
            LastChamferCompactSummaryTicksByGeometry[geometrySignature] = now;
            LastChamferCompactSummaryOriginByGeometry[geometrySignature] =
                origin;
            return false;
        }

        private static string FormatPlaneCutBandAudit(
            PlaneCutBevelAuditResult audit)
        {
            return "retained:" + audit.BandRetainedEdgeCount +
                ",single:" + audit.BandSingleFaceCount +
                ",split:" + audit.BandSplitCount +
                ",interrupted:" + audit.BandInterruptedCount +
                ",foreignCut:" + audit.BandForeignCutCount +
                ",overlongJunction:" +
                    audit.BandOverlongJunctionCount +
                ",collapsed:" + audit.BandCollapsedCount +
                ",minCoverage:" +
                    audit.BandMinimumCoverageRatio.ToString("G6") +
                ",maxJunctionInfluence:" +
                    audit.BandMaximumJunctionInfluenceRatio
                        .ToString("G6") +
                ",maxSharedAxisSpan:" +
                    audit.BandMaximumSharedAxisSpanRatio
                        .ToString("G6");
        }

        private static string FormatPlaneCutEdgeConflictAudit(
            PlaneCutBevelAuditResult audit)
        {
            bool evaluated = audit.EdgeConflictPassCount > 0;
            bool widthReduction = audit.CoverageAudit != null &&
                audit.CoverageAudit.MaximumCoverageMode;
            return "mode:" +
                    (widthReduction
                        ? "clusterWidthReduction"
                        : "candidateDeferral") +
                ",passes:" + audit.EdgeConflictPassCount +
                ",clusters:" + audit.EdgeConflictClusterCount +
                ",reductions:" +
                    audit.EdgeConflictWidthReductionCount +
                ",minimumWidthScale:" +
                    audit.EdgeConflictMinimumWidthScale.ToString("G6") +
                ",unresolved:" + audit.EdgeConflictUnresolvedCount +
                ",deferred:" + audit.EdgeConflictEdgesDeferredCount +
                ",resolved:" + audit.EdgeConflictResolvedCount +
                ",topologyRejected:" +
                    audit.EdgeConflictTopologyRejectedPassCount +
                ",topologyExpanded:" +
                    audit.EdgeConflictTopologyExpandedClusterCount +
                ",topologyRollbacks:" +
                    audit.EdgeConflictTopologyRollbackCount +
                ",budgetExhausted:" +
                    audit.EdgeConflictBudgetExhausted +
                ",victim:" +
                    (evaluated
                        ? audit.EdgeConflictVictimEdgeIndex
                        : -1) +
                ",foreign:" +
                    (evaluated
                        ? audit.EdgeConflictForeignEdgeIndex
                        : -1) +
                ",vertex:" +
                    (evaluated
                        ? audit.EdgeConflictVertexIndex
                        : -1) +
                ",deferredEdge:" +
                    (evaluated
                        ? audit.EdgeConflictDeferredEdgeIndex
                        : -1) +
                ",victimCoverage:" +
                    audit.EdgeConflictVictimCoverageRatio
                        .ToString("G6") +
                ",foreignAxial:" +
                    audit.EdgeConflictForeignAxialParameter
                        .ToString("G6") +
                ",foreignSpan:" +
                    audit.EdgeConflictForeignSharedSpanRatio
                        .ToString("G6");
        }

        private static string FormatPlaneCutTopologyScaleSearchAudit(
            PlaneCutBevelAuditResult audit)
        {
            return "mode:" +
                    (string.IsNullOrEmpty(
                            audit.TopologyScaleSearchMode)
                        ? "none"
                        : audit.TopologyScaleSearchMode) +
                ",trigger:{" +
                    (string.IsNullOrEmpty(
                            audit.TopologyScaleSearchTriggerEvidence)
                        ? "none"
                        : audit.TopologyScaleSearchTriggerEvidence) + "}" +
                ",topologyLinked:{" +
                    (string.IsNullOrEmpty(
                            audit.TopologyScaleSearchTopologyLinkedEvidence)
                        ? "none"
                        : audit.TopologyScaleSearchTopologyLinkedEvidence) +
                    "}" +
                ",baseState:" +
                    (audit.TopologyScaleSearchBasePass >= 0
                        ? "topologyClean:" +
                            audit.TopologyScaleSearchBasePass.ToString()
                        : "none") +
                ",retreatEdges:{" +
                    (string.IsNullOrEmpty(
                            audit.TopologyScaleSearchClusterEvidence)
                        ? "none"
                        : audit.TopologyScaleSearchClusterEvidence) + "}" +
                ",protectedEdges:{" +
                    (string.IsNullOrEmpty(
                            audit.TopologyScaleSearchProtectedEvidence)
                        ? "none"
                        : audit.TopologyScaleSearchProtectedEvidence) + "}" +
                ",activeSearchFailure:{stage:" +
                    (string.IsNullOrEmpty(
                            audit.ActiveSearchFailureStage)
                        ? "none"
                        : audit.ActiveSearchFailureStage) +
                    ",cause:" +
                    (string.IsNullOrEmpty(
                            audit.ActiveSearchFailureCause)
                        ? "none"
                        : audit.ActiveSearchFailureCause) +
                    ",evidence:{" +
                    (string.IsNullOrEmpty(
                            audit.ActiveSearchFailureEvidence)
                        ? "none"
                        : audit.ActiveSearchFailureEvidence) + "}}" +
                ",trials:" + audit.TopologyScaleSearchTrialCount +
                ",committedFactor:" +
                    (audit.TopologyScaleSearchCommittedFactor >= 0f
                        ? audit.TopologyScaleSearchCommittedFactor
                            .ToString("G6")
                        : "none") +
                ",highestValidFactor:" +
                    (audit.TopologyScaleSearchHighestValidFactor >= 0f
                        ? audit.TopologyScaleSearchHighestValidFactor
                            .ToString("G6")
                        : "none") +
                ",bandFailures:" +
                    audit.TopologyScaleSearchBandFailureCount +
                ",topologyFailures:" +
                    audit.TopologyScaleSearchTopologyFailureCount +
                ",faceQualityFailures:" +
                    audit.TopologyScaleSearchFaceQualityFailureCount +
                ",collateralFailures:" +
                    audit.TopologyScaleSearchCollateralFailureCount +
                ",collateralChanged:{" +
                    (string.IsNullOrEmpty(
                            audit.TopologyScaleSearchCollateralChangedEvidence)
                        ? "none"
                        : audit.TopologyScaleSearchCollateralChangedEvidence) +
                    "}" +
                ",failedStateScalesReused:" +
                    audit.TopologyScaleSearchFailedStateScalesReused +
                ",fallbackState:" +
                    (audit.TopologyScaleSearchUnresolved == 1 &&
                     audit.TopologyScaleSearchBasePass >= 0
                        ? "topologyClean:" +
                            audit.TopologyScaleSearchBasePass.ToString()
                        : "none") +
                ",unresolved:" +
                    audit.TopologyScaleSearchUnresolved;
        }

        private static string FormatPlaneCutLocalJunctionAudit(
            PlaneCutBevelAuditResult audit)
        {
            return "candidates:" +
                    audit.LocalJunctionCandidateCount +
                ",extracted:" +
                    audit.LocalJunctionStarsExtractedCount +
                ",closed:" + audit.LocalJunctionClosedLoopCount +
                ",branched:" + audit.LocalJunctionBranchedCount +
                ",selfX:" +
                    audit.LocalJunctionSelfIntersectingCount +
                ",foreign:" +
                    audit.LocalJunctionForeignFaceCount +
                ",missing:" +
                    audit.LocalJunctionMissingIncidentBevelCount +
                ",duplicate:" +
                    audit.LocalJunctionDuplicateIncidentBevelCount +
                ",loopVertices:" +
                    audit.LocalJunctionMinimumLoopVertexCount + "-" +
                    audit.LocalJunctionMaximumLoopVertexCount +
                ",maxExtent:" +
                    audit.LocalJunctionMaximumExtentRatio
                        .ToString("G6");
        }

        private static string FormatPlaneCutBevelAuditFields(
            PlaneCutBevelAuditResult planeCutAudit)
        {
            return
                "planeBevel=" +
                    planeCutAudit.SelectedEdgeCount + "/" +
                    planeCutAudit.ActiveEdgeCount + "/" +
                    planeCutAudit.PlanesBuilt + "/" +
                    planeCutAudit.PlanesLocalized + "/" +
                    planeCutAudit.PlanesDeferred + "/" +
                    planeCutAudit.PlanesRejected + "/" +
                    planeCutAudit.CapsBuilt + "/" +
                    planeCutAudit.CapsMissing + "/" +
                    planeCutAudit.CapsRedundant + "/" +
                    planeCutAudit.ConformalSplitCount + "/" +
                    planeCutAudit.SeamPairCount + "/" +
                    planeCutAudit.OpenEdgeCount + "/" +
                    planeCutAudit.NonManifoldEdgeCount + "/" +
                    planeCutAudit.TJunctionCount + "/" +
                    planeCutAudit.InvalidFaceCount + "/" +
                    planeCutAudit.GeometryValid +
                ",planeTransaction=" +
                    "attempted:" +
                        planeCutAudit.AttemptedPlanesBuilt +
                    ",certified:" +
                        planeCutAudit.CertifiedPlanesBuilt +
                    ",trialRejected:" +
                        planeCutAudit.TrialRejectedPlanes +
                ",planeVertexJunction=" +
                    planeCutAudit.VertexJunctionCandidateCount + "/" +
                    planeCutAudit.VertexJunctionDirectBuiltCount + "/" +
                    planeCutAudit.VertexJunctionAdaptiveBuiltCount + "/" +
                    planeCutAudit.VertexJunctionBacktrackBuiltCount + "/" +
                    planeCutAudit.VertexJunctionCleanSharpCount + "/" +
                    planeCutAudit.VertexJunctionUnresolvedCount + "/" +
                    planeCutAudit.VertexJunctionTriangleCapCount + "/" +
                    planeCutAudit.VertexJunctionQuadCapCount + "/" +
                    planeCutAudit.VertexJunctionLargerCapCount + "/" +
                    planeCutAudit.VertexJunctionEdgesDeferredCount + "/" +
                    planeCutAudit.VertexJunctionRebuildPassCount +
                ",planeSolve=" +
                    planeCutAudit.SolveStatesEvaluated + "/" +
                    planeCutAudit.SolveJunctionsVisited + "/" +
                    planeCutAudit.SolveCandidateTrials + "/" +
                    planeCutAudit.SolveSystemRebuilds + "/" +
                    planeCutAudit.SolvePolygonAudits + "/" +
                    planeCutAudit.SolveTriangleAudits + "/" +
                    planeCutAudit.SolveEdgesDeferred + "/" +
                    planeCutAudit.SolveElapsedMilliseconds + "/" +
                    planeCutAudit.SolveTimedOut +
                ",planeFaceQuality=" +
                    planeCutAudit.FaceQualityFaceCount + "/" +
                    planeCutAudit.FaceQualitySeamTouchedFaceCount + "/" +
                    planeCutAudit.FaceQualityNonPlanarCount + "/" +
                    planeCutAudit.FaceQualityElongatedJunctionCount + "/" +
                    planeCutAudit.FaceQualityMaxPlaneDeviation
                        .ToString("G6") + "/" +
                    planeCutAudit.FaceQualityMaxNormalSpreadDegrees
                        .ToString("G6") + "/" +
                    planeCutAudit.FaceQualityMinimumJunctionCompactness
                        .ToString("G6") + "/" +
                    planeCutAudit.FaceQualityMaximumJunctionAspectRatio
                        .ToString("G6") + "/" +
                    planeCutAudit.FaceQualityWorstVertexCount +
                ",planeBand=" +
                    FormatPlaneCutBandAudit(planeCutAudit) +
                ",edgeConflict=" +
                    FormatPlaneCutEdgeConflictAudit(planeCutAudit) +
                ",topologyScaleSearch={" +
                    FormatPlaneCutTopologyScaleSearchAudit(
                        planeCutAudit) + "}" +
                ",localJunction=" +
                    FormatPlaneCutLocalJunctionAudit(planeCutAudit) +
                ",planeSurface=" +
                    "faces:" + planeCutAudit.BevelRegionFaceCount +
                    ",boundaryVertices:" +
                        planeCutAudit.BevelRegionBoundaryVertexCount +
                    ",triangles:" +
                        planeCutAudit.BevelRegionTriangleCount +
                    ",authoredNormalTriangles:" +
                        planeCutAudit.BevelRegionAuthoredNormalTriangleCount +
                    ",authoredSurfaceGroupTriangles:" +
                        planeCutAudit.BevelRegionAuthoredSurfaceGroupTriangleCount +
                    ",internalFanVertices:" +
                        planeCutAudit.BevelRegionInternalFanVertexCount +
                    ",maxPlaneResidual:" +
                        planeCutAudit.BevelRegionMaximumPlaneResidual.ToString("G9") +
                    ",maxNormalDeviationDegrees:" +
                        planeCutAudit.BevelRegionMaximumNormalDeviationDegrees.ToString("G9") +
                    ",renderValid:" +
                        planeCutAudit.BevelRegionRenderValid +
                    ",materializedCoverage:" +
                        planeCutAudit.MaterializedEdgeCoverageValid +
                ",planeEdges=" +
                    "active:{" +
                        (string.IsNullOrEmpty(planeCutAudit.ActiveEdgeEvidence)
                            ? "none"
                            : planeCutAudit.ActiveEdgeEvidence) + "}" +
                    ",attempted:{" +
                        (string.IsNullOrEmpty(
                                planeCutAudit.AttemptedEdgeEvidence)
                            ? "none"
                            : planeCutAudit.AttemptedEdgeEvidence) + "}" +
                    ",certified:{" +
                        (string.IsNullOrEmpty(planeCutAudit.BuiltEdgeEvidence)
                            ? "none"
                            : planeCutAudit.BuiltEdgeEvidence) + "}" +
                    ",trialRejected:{" +
                        (string.IsNullOrEmpty(
                                planeCutAudit.TrialRejectedEdgeEvidence)
                            ? "none"
                            : planeCutAudit.TrialRejectedEdgeEvidence) + "}" +
                    ",deferred:{" +
                        (string.IsNullOrEmpty(planeCutAudit.DeferredEdgeEvidence)
                            ? "none"
                            : planeCutAudit.DeferredEdgeEvidence) + "}" +
                ",planeMesh=" +
                    planeCutAudit.PreviewTriangleCount + "/" +
                    planeCutAudit.PreviewDegenerateTriangleCount + "/" +
                    planeCutAudit.PreviewOpenEdgeCount + "/" +
                    planeCutAudit.PreviewNonManifoldEdgeCount + "/" +
                    planeCutAudit.PreviewWindingFailureCount + "/" +
                    planeCutAudit.PreviewBoundsFailureCount + "/" +
                    planeCutAudit.PreviewVolumeFailureCount + "/" +
                    planeCutAudit.PreviewGeometryValid +
                (string.IsNullOrEmpty(planeCutAudit.Diagnostic)
                    ? string.Empty
                    : ",planeTrace=" + planeCutAudit.Diagnostic);
        }

        private static void LogPlaneCutBevelAudit(
            PlaneCutBevelAuditResult planeCutAudit)
        {
#if UNITY_EDITOR
            string message =
                "GeneratedMass plane-cut bevel compact audit. " +
                FormatPlaneCutBevelAuditFields(planeCutAudit) +
                ", geometryCommit=disabled";
            LogChamferNoStackTrace(
                message,
                planeCutAudit.GeometryValid != 1);
#endif
        }

        private static string FormatPlaneCutVector(Vector3 value)
        {
            return "(" + value.x.ToString("G9") + "/" +
                value.y.ToString("G9") + "/" +
                value.z.ToString("G9") + ")";
        }

        private static string FormatPlaneCutStageSnapshot(
            PlaneCutStageSnapshot snapshot)
        {
            if (string.IsNullOrEmpty(snapshot.Stage))
            {
                return "notCaptured";
            }
            return snapshot.Stage +
                "[faces=" + snapshot.FaceCount +
                ",vertices=" + snapshot.VertexCount +
                ",unique=" + snapshot.UniqueVertexCount +
                ",bevel=" + snapshot.BevelFaceCount +
                ",junction=" + snapshot.JunctionFaceCount +
                ",open=" + snapshot.OpenEdgeCount +
                ",nonManifold=" + snapshot.NonManifoldEdgeCount +
                ",tJunction=" + snapshot.TJunctionCount +
                ",invalid=" + snapshot.InvalidFaceCount +
                ",nonPlanar=" + snapshot.NonPlanarFaceCount +
                ",maxDeviation=" +
                    snapshot.MaximumPlaneDeviation.ToString("G9") +
                ",maxSpread=" +
                    snapshot.MaximumNormalSpreadDegrees.ToString("G9") +
                "]";
        }

        private static string FormatPlaneCutStageTimeline(
            PlaneCutBevelAuditResult audit)
        {
            return FormatPlaneCutStageSnapshot(
                    audit.StagePlaneConstruction) + ";" +
                FormatPlaneCutStageSnapshot(audit.StageSanitized) + ";" +
                FormatPlaneCutStageSnapshot(audit.StageWelded) + ";" +
                FormatPlaneCutStageSnapshot(audit.StageConformed) + ";" +
                FormatPlaneCutStageSnapshot(audit.StageSeamRepaired) + ";" +
                FormatPlaneCutStageSnapshot(
                    audit.StageFinalCertification);
        }

        private static string FormatPlaneCutFaceFailure(
            PlaneCutFaceQualityFailureRecord failure,
            bool includeVertices)
        {
            string result =
                "face=" + failure.FaceIndex +
                ",id=" + failure.ProvenanceKind + ":" +
                    failure.ProvenanceIndex +
                ",cause=" + failure.Cause +
                ",firstStage=" + failure.FirstFailureStage +
                ",vertices=" + failure.VertexCount +
                ",deviation=" +
                    failure.MaximumPlaneDeviation.ToString("G9") +
                "/" + failure.PlanarityTolerance.ToString("G9") +
                ",offendingVertex=" + failure.OffendingVertexIndex +
                ",signedResidual=" +
                    failure.OffendingSignedResidual.ToString("G9") +
                ",spread=" +
                    failure.MaximumNormalSpreadDegrees.ToString("G9") +
                "/" +
                    failure.NormalSpreadToleranceDegrees.ToString("G9") +
                ",offendingSegment=" +
                    failure.OffendingSegmentIndex +
                ",area=" + failure.Area.ToString("G9") +
                ",minEdge=" +
                    failure.MinimumEdgeLength.ToString("G9") +
                ",conformTouched=" +
                    failure.BoundaryConformityTouched +
                ",seamTouched=" + failure.SeamRepairTouched +
                ",seamMove=" +
                    failure.SeamRepairMaximumMovement.ToString("G9");
            if (!includeVertices)
            {
                return result;
            }
            return result +
                ",authoredNormal=" +
                    FormatPlaneCutVector(failure.AuthoredNormal) +
                ",measuredNormal=" +
                    FormatPlaneCutVector(failure.MeasuredNormal) +
                ",planeDistance=" +
                    failure.PlaneDistance.ToString("G9") +
                ",offendingPosition=" +
                    FormatPlaneCutVector(
                        failure.OffendingVertexPosition) +
                ",offendingTriangleNormal=" +
                    FormatPlaneCutVector(
                        failure.OffendingTriangleNormal) +
                ",vertexResiduals={" +
                    failure.VertexResidualEvidence + "}";
        }

        private static string FormatPlaneCutOpenEdgeFailure(
            PlaneCutOpenEdgeFailureRecord failure,
            bool includeNearestSegment)
        {
            string result =
                "open=" + failure.RecordIndex +
                ",owner=" + failure.FaceProvenanceKind + ":" +
                    failure.FaceProvenanceIndex +
                "#" + failure.FaceIndex +
                ",cause=" + failure.Cause +
                ",firstStage=" + failure.FirstFailureStage +
                ",length=" + failure.Length.ToString("G9") +
                ",sourceVertex=" + failure.AssociatedSourceVertex +
                ",sourceDistance=" +
                    failure.SourceVertexDistance.ToString("G9") +
                ",incidentEdges={" + failure.IncidentBuiltEdges + "}" +
                ",junctionExpected=" + failure.JunctionExpected +
                ",junctionFaces=" + failure.JunctionFaceCount +
                ",expected=" + failure.ExpectedNeighbour +
                ",nearest=" +
                    failure.NearestFaceProvenanceKind + ":" +
                    failure.NearestFaceProvenanceIndex +
                    "#" + failure.NearestFaceIndex +
                ",nearestDistance=" +
                    failure.NearestReversedEndpointDistance.ToString("G9") +
                ",edge=" + FormatPlaneCutVector(failure.Start) + "->" +
                    FormatPlaneCutVector(failure.End);
            if (!includeNearestSegment)
            {
                return result;
            }
            return result +
                ",sourcePosition=" +
                    FormatPlaneCutVector(
                        failure.AssociatedSourcePosition) +
                ",nearestSegment=" +
                    FormatPlaneCutVector(failure.NearestSegmentStart) +
                    "->" +
                    FormatPlaneCutVector(failure.NearestSegmentEnd);
        }

        private static string FormatPlaneCutJunctionCoverage(
            PlaneCutJunctionCoverageRecord coverage)
        {
            return "vertex=" + coverage.VertexIndex +
                ",position=" +
                    FormatPlaneCutVector(coverage.SourcePosition) +
                ",incidentEdges={" + coverage.IncidentBuiltEdges + "}" +
                ",incidentCount=" + coverage.IncidentBuiltEdgeCount +
                ",expected=" + coverage.JunctionExpected +
                ",faces=" + coverage.JunctionFaceCount +
                ",openEdges=" + coverage.AssignedOpenEdgeCount +
                ",reason=" + coverage.FailureReason;
        }

        private static string FormatPlaneCutTJunctionFailure(
            PlaneCutTJunctionFailureRecord failure,
            bool includeOwners)
        {
            string result =
                "record=" + failure.RecordIndex +
                ",stage=" + failure.Stage +
                ",cause=" + failure.Cause +
                ",vertex=" +
                    FormatPlaneCutVector(failure.JunctionVertex) +
                ",host=" + failure.HostProvenanceKind + ":" +
                    failure.HostProvenanceIndex + "#" +
                    failure.HostFaceIndex +
                ",hostSegment=" + failure.HostSegmentIndex +
                ",t=" + failure.SegmentParameter.ToString("G9") +
                ",distance=" + failure.Distance.ToString("G9") +
                    "/" + failure.Tolerance.ToString("G9") +
                ",hostLength=" + failure.HostLength.ToString("G9") +
                ",hostMatches=" + failure.MatchingHostSegmentCount +
                ",provenanceBevels={" +
                    failure.ProvenanceBevelEdges + "}" +
                ",candidatePlaneMatches={" +
                    failure.CandidatePlaneMatches + "}" +
                ",edgeScales={" +
                    failure.AssociatedEdgeScales + "}" +
                ",lastConflictPass=" + failure.LastConflictPass +
                ",lastConflictCluster={" +
                    failure.LastConflictCluster + "}";
            if (!includeOwners)
            {
                return result;
            }
            return result +
                ",closest=" +
                    FormatPlaneCutVector(failure.ClosestPoint) +
                ",hostEdge=" +
                    FormatPlaneCutVector(failure.HostStart) + "->" +
                    FormatPlaneCutVector(failure.HostEnd) +
                ",vertexOwnerCount=" + failure.VertexOwnerFaceCount +
                ",vertexOwners={" + failure.VertexOwnerFaces + "}";
        }

        private static string FormatPlaneCutLocalityDeferral(
            PlaneCutLocalityDeferralRecord record,
            bool includePositions)
        {
            string result =
                "edge=" + record.SourceEdgeIndex +
                ",vertices=" + record.VertexA + "/" + record.VertexB +
                ",faces=" + record.FaceA + "/" + record.FaceB +
                ",width=" + record.SolvedWidth.ToString("G9") +
                ",solvedDistance=" +
                    record.SolvedPlaneDistance.ToString("G9") +
                ",localizedDistance=" +
                    record.LocalizedPlaneDistance.ToString("G9") +
                ",localizationDelta=" +
                    record.LocalizationDelta.ToString("G9") +
                ",guardMargin=" +
                    record.LocalGuardMargin.ToString("G9") +
                ",limitingVertex=" +
                    record.LimitingUnrelatedVertex +
                ",solvedRemoval=" +
                    record.SolvedSourceRemovalA.ToString("G9") + "/" +
                    record.SolvedSourceRemovalB.ToString("G9") +
                ",localizedRemoval=" +
                    record.LocalizedSourceRemovalA.ToString("G9") + "/" +
                    record.LocalizedSourceRemovalB.ToString("G9") +
                ",minimumRemoval=" +
                    record.MinimumRequiredRemoval.ToString("G9") +
                ",cause=" + record.Cause;
            if (!includePositions)
            {
                return result;
            }
            return result +
                ",normal=" +
                    FormatPlaneCutVector(record.BevelNormal) +
                ",sourceA=" +
                    FormatPlaneCutVector(record.SourceA) +
                ",sourceB=" +
                    FormatPlaneCutVector(record.SourceB) +
                ",limitingPosition=" +
                    FormatPlaneCutVector(
                        record.LimitingUnrelatedPosition) +
                ",limitingProjection=" +
                    record.LimitingUnrelatedProjection.ToString("G9");
        }

        private static string FormatCappedPlaneCutTJunctions(
            PlaneCutBevelAuditResult audit,
            int cap)
        {
            if (audit.TJunctionFailures == null ||
                audit.TJunctionFailures.Count == 0)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            int count = Mathf.Min(cap, audit.TJunctionFailures.Count);
            for (int index = 0; index < count; index++)
            {
                if (index > 0)
                {
                    builder.Append('|');
                }
                builder.Append(FormatPlaneCutTJunctionFailure(
                    audit.TJunctionFailures[index],
                    false));
            }
            if (audit.TJunctionFailures.Count > count)
            {
                builder.Append("|omitted=");
                builder.Append(audit.TJunctionFailures.Count - count);
            }
            return builder.ToString();
        }

        private static string FormatCappedPlaneCutLocalityDeferrals(
            PlaneCutBevelAuditResult audit,
            int cap)
        {
            if (audit.LocalityDeferrals == null ||
                audit.LocalityDeferrals.Count == 0)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            int count = Mathf.Min(cap, audit.LocalityDeferrals.Count);
            for (int index = 0; index < count; index++)
            {
                if (index > 0)
                {
                    builder.Append('|');
                }
                builder.Append(FormatPlaneCutLocalityDeferral(
                    audit.LocalityDeferrals[index],
                    false));
            }
            if (audit.LocalityDeferrals.Count > count)
            {
                builder.Append("|omitted=");
                builder.Append(audit.LocalityDeferrals.Count - count);
            }
            return builder.ToString();
        }

        private static string FormatPlaneCutSolverTransactionState(
            PlaneCutSolverTransactionState state)
        {
            if (state == null)
            {
                return "none";
            }
            return "name=" + state.Name +
                ",pass=" + state.PassIndex +
                ",candidates=" + state.Candidates.Count +
                ",bandClean=" + state.BandClean +
                ",geometryClean=" + state.GeometryClean +
                ",edges={" +
                    FormatPlaneCutCandidateEdgeEvidence(
                        state.Candidates) + "}" +
                ",scales={" +
                    FormatPlaneCutScaleEvidence(
                        state.ScaleByEdge,
                        CollectPlaneCutCandidateEdgeIndices(
                            state.Candidates)) + "}" +
                ",stage={" +
                    FormatPlaneCutStageSnapshot(state.Stage) + "}";
        }

        private static string FormatPlaneCutRetryFailureDossier(
            PlaneCutRetryFailureDossier dossier,
            bool complete)
        {
            if (dossier == null)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("pass=");
            builder.Append(dossier.PassIndex);
            builder.Append(",stage=");
            builder.Append(dossier.Stage);
            builder.Append(",cause=");
            builder.Append(dossier.Cause);
            builder.Append(",attemptedBuilt=");
            builder.Append(dossier.AttemptedBuiltCount);
            builder.Append(",topology=");
            builder.Append(dossier.OpenEdgeCount);
            builder.Append('/');
            builder.Append(dossier.NonManifoldEdgeCount);
            builder.Append('/');
            builder.Append(dossier.TJunctionCount);
            builder.Append('/');
            builder.Append(dossier.InvalidFaceCount);
            builder.Append(",nonPlanar=");
            builder.Append(dossier.NonPlanarFaceCount);
            builder.Append(",linked={");
            builder.Append(FormatPlaneCutEdgeIndexEvidence(
                dossier.LinkedEdgeIndices));
            builder.Append("},cluster={");
            builder.Append(string.IsNullOrEmpty(
                    dossier.GeneralizedClusterEvidence)
                ? "none"
                : dossier.GeneralizedClusterEvidence);
            builder.Append("},clusterReasons={");
            builder.Append(string.IsNullOrEmpty(
                    dossier.GeneralizedClusterReasonEvidence)
                ? "none"
                : dossier.GeneralizedClusterReasonEvidence);
            builder.Append('}');
            if (!complete)
            {
                if (dossier.NonPlanarFaceFailures.Count > 0)
                {
                    builder.Append(",face={");
                    builder.Append(FormatPlaneCutFaceFailure(
                        dossier.NonPlanarFaceFailures[0],
                        false));
                    builder.Append('}');
                }
                if (dossier.OpenEdgeFailures.Count > 0)
                {
                    builder.Append(",open={");
                    builder.Append(FormatPlaneCutOpenEdgeFailure(
                        dossier.OpenEdgeFailures[0],
                        false));
                    builder.Append('}');
                }
                if (dossier.TJunctionFailures.Count > 0)
                {
                    builder.Append(",tJunction={");
                    builder.Append(FormatPlaneCutTJunctionFailure(
                        dossier.TJunctionFailures[0],
                        false));
                    builder.Append('}');
                }
                return builder.ToString();
            }

            builder.Append(",candidateEdges={");
            builder.Append(string.IsNullOrEmpty(
                    dossier.CandidateEdgeEvidence)
                ? "none"
                : dossier.CandidateEdgeEvidence);
            builder.Append("},scales={");
            builder.Append(string.IsNullOrEmpty(dossier.ScaleEvidence)
                ? "none"
                : dossier.ScaleEvidence);
            builder.Append("},nonManifold={");
            builder.Append(string.IsNullOrEmpty(
                    dossier.NonManifoldEvidence)
                ? "none"
                : dossier.NonManifoldEvidence);
            builder.Append("},invalidFaces={");
            builder.Append(string.IsNullOrEmpty(
                    dossier.InvalidFaceEvidence)
                ? "none"
                : dossier.InvalidFaceEvidence);
            builder.Append('}');
            return builder.ToString();
        }

        private static string FormatCappedPlaneCutRetryFailures(
            PlaneCutBevelAuditResult audit,
            int cap)
        {
            if (audit.RetryFailureDossiers == null ||
                audit.RetryFailureDossiers.Count == 0)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            int count = Mathf.Min(
                cap,
                audit.RetryFailureDossiers.Count);
            int startIndex =
                audit.RetryFailureDossiers.Count - count;
            for (int offset = 0; offset < count; offset++)
            {
                if (offset > 0)
                {
                    builder.Append('|');
                }
                builder.Append(FormatPlaneCutRetryFailureDossier(
                    audit.RetryFailureDossiers[startIndex + offset],
                    false));
            }
            int omitted = audit.RetryFailureDossiers.Count - count;
            if (omitted > 0)
            {
                builder.Append("|omitted=");
                builder.Append(omitted);
            }
            return builder.ToString();
        }

        private static void AppendPlaneCutRetryFailureDossiers(
            StringBuilder builder,
            PlaneCutBevelAuditResult audit)
        {
            int count = audit.RetryFailureDossiers == null
                ? 0
                : audit.RetryFailureDossiers.Count;
            builder.Append("count=");
            builder.AppendLine(count.ToString());
            if (audit.RetryFailureDossiers == null)
            {
                return;
            }
            for (int index = 0; index < count; index++)
            {
                PlaneCutRetryFailureDossier dossier =
                    audit.RetryFailureDossiers[index];
                builder.Append(index);
                builder.Append(':');
                builder.AppendLine(FormatPlaneCutRetryFailureDossier(
                    dossier,
                    true));
                for (int faceIndex = 0;
                     faceIndex < dossier.NonPlanarFaceFailures.Count;
                     faceIndex++)
                {
                    builder.Append("  face[");
                    builder.Append(faceIndex);
                    builder.Append("]=");
                    builder.AppendLine(FormatPlaneCutFaceFailure(
                        dossier.NonPlanarFaceFailures[faceIndex],
                        true));
                }
                for (int openIndex = 0;
                     openIndex < dossier.OpenEdgeFailures.Count;
                     openIndex++)
                {
                    builder.Append("  open[");
                    builder.Append(openIndex);
                    builder.Append("]=");
                    builder.AppendLine(FormatPlaneCutOpenEdgeFailure(
                        dossier.OpenEdgeFailures[openIndex],
                        true));
                }
                for (int tIndex = 0;
                     tIndex < dossier.TJunctionFailures.Count;
                     tIndex++)
                {
                    builder.Append("  tJunction[");
                    builder.Append(tIndex);
                    builder.Append("]=");
                    builder.AppendLine(FormatPlaneCutTJunctionFailure(
                        dossier.TJunctionFailures[tIndex],
                        true));
                }
            }
        }

        private static string FormatPlaneCutPrimaryFailure(
            PlaneCutBevelAuditResult audit)
        {
            if (audit.FaceQualityFailures != null &&
                audit.FaceQualityFailures.Count > 0)
            {
                PlaneCutFaceQualityFailureRecord failure =
                    audit.FaceQualityFailures[0];
                return "stage=" + failure.FirstFailureStage +
                    ",category=FaceQuality" +
                    ",face=" + failure.FaceIndex +
                    ",id=" + failure.ProvenanceKind + ":" +
                        failure.ProvenanceIndex +
                    ",cause=" + failure.Cause +
                    ",deviation=" +
                        failure.MaximumPlaneDeviation.ToString("G9") +
                        "/" +
                        failure.PlanarityTolerance.ToString("G9") +
                    ",spread=" +
                        failure.MaximumNormalSpreadDegrees.ToString("G9") +
                        "/" +
                        failure.NormalSpreadToleranceDegrees.ToString("G9");
            }
            if (audit.OpenEdgeFailures != null &&
                audit.OpenEdgeFailures.Count > 0)
            {
                PlaneCutOpenEdgeFailureRecord failure =
                    audit.OpenEdgeFailures[0];
                return "stage=" + failure.FirstFailureStage +
                    ",category=OpenEdge" +
                    ",owner=" + failure.FaceProvenanceKind + ":" +
                        failure.FaceProvenanceIndex +
                    ",cause=" + failure.Cause +
                    ",sourceVertex=" +
                        failure.AssociatedSourceVertex +
                    ",expected=" + failure.ExpectedNeighbour;
            }
            if (audit.TJunctionFailures != null &&
                audit.TJunctionFailures.Count > 0)
            {
                PlaneCutTJunctionFailureRecord failure =
                    audit.TJunctionFailures[0];
                return "stage=" + failure.Stage +
                    ",category=TJunction" +
                    ",vertex=" +
                        FormatPlaneCutVector(failure.JunctionVertex) +
                    ",host=" + failure.HostProvenanceKind + ":" +
                        failure.HostProvenanceIndex +
                    ",segment=" + failure.HostSegmentIndex +
                    ",t=" + failure.SegmentParameter.ToString("G9") +
                    ",distance=" + failure.Distance.ToString("G9") +
                        "/" + failure.Tolerance.ToString("G9") +
                    ",bevels={" +
                        failure.ProvenanceBevelEdges + "}" +
                    ",lastConflictPass=" +
                        failure.LastConflictPass;
            }
            if (audit.RetryFailureDossiers != null &&
                audit.RetryFailureDossiers.Count > 0)
            {
                PlaneCutRetryFailureDossier failure =
                    audit.RetryFailureDossiers[
                        audit.RetryFailureDossiers.Count - 1];
                return "stage=" + failure.Stage +
                    ",category=RetryFailure" +
                    ",cause=" + failure.Cause +
                    ",topology=" + failure.OpenEdgeCount + "/" +
                        failure.NonManifoldEdgeCount + "/" +
                        failure.TJunctionCount + "/" +
                        failure.InvalidFaceCount +
                    ",nonPlanar=" + failure.NonPlanarFaceCount +
                    ",linked={" +
                        FormatPlaneCutEdgeIndexEvidence(
                            failure.LinkedEdgeIndices) + "}" +
                    ",cluster={" +
                        (string.IsNullOrEmpty(
                                failure.GeneralizedClusterEvidence)
                            ? "none"
                            : failure.GeneralizedClusterEvidence) + "}";
            }
            if (audit.NumericalRepairs != null &&
                audit.NumericalRepairs.ExactConstructionFailureCount > 0)
            {
                return "stage=PlaneConstruction" +
                    ",category=StrictIntersection" +
                    ",cause={" +
                    FormatPlaneCutFirstExactFailure(
                        audit.NumericalRepairs) + "}";
            }
            if (audit.GeometryValid == 1 &&
                audit.MaterializedEdgeCoverageValid == 0)
            {
                return "category=Coverage,cause=selected bevels did not all materialize" +
                    ",built=" + audit.PlanesBuilt +
                    ",active=" + audit.ActiveEdgeCount +
                    ",deferred=" + audit.PlanesDeferred +
                    ",unresolvedConflicts=" +
                        audit.EdgeConflictUnresolvedCount;
            }
            if (audit.GeometryValid == 1 &&
                audit.MaterializedEdgeCoverageValid == 1)
            {
                return "none";
            }
            return string.IsNullOrEmpty(audit.Diagnostic)
                ? "none"
                : "category=General,cause=" + audit.Diagnostic;
        }

        private static string FormatCappedPlaneCutFaceFailures(
            PlaneCutBevelAuditResult audit,
            int cap)
        {
            if (audit.FaceQualityFailures == null ||
                audit.FaceQualityFailures.Count == 0)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            int count = Mathf.Min(cap, audit.FaceQualityFailures.Count);
            for (int index = 0; index < count; index++)
            {
                if (index > 0)
                {
                    builder.Append('|');
                }
                builder.Append(FormatPlaneCutFaceFailure(
                    audit.FaceQualityFailures[index],
                    false));
            }
            if (audit.FaceQualityFailures.Count > count)
            {
                builder.Append("|omitted=");
                builder.Append(audit.FaceQualityFailures.Count - count);
            }
            return builder.ToString();
        }

        private static string FormatCappedPlaneCutOpenEdges(
            PlaneCutBevelAuditResult audit,
            int cap)
        {
            if (audit.OpenEdgeFailures == null ||
                audit.OpenEdgeFailures.Count == 0)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            int count = Mathf.Min(cap, audit.OpenEdgeFailures.Count);
            for (int index = 0; index < count; index++)
            {
                if (index > 0)
                {
                    builder.Append('|');
                }
                builder.Append(FormatPlaneCutOpenEdgeFailure(
                    audit.OpenEdgeFailures[index],
                    false));
            }
            if (audit.OpenEdgeFailures.Count > count)
            {
                builder.Append("|omitted=");
                builder.Append(audit.OpenEdgeFailures.Count - count);
            }
            return builder.ToString();
        }

        private static string FormatCappedPlaneCutJunctionFailures(
            PlaneCutBevelAuditResult audit,
            int cap)
        {
            if (audit.JunctionCoverage == null ||
                audit.JunctionCoverage.Count == 0)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            int written = 0;
            int failureCount = 0;
            for (int index = 0;
                 index < audit.JunctionCoverage.Count;
                 index++)
            {
                PlaneCutJunctionCoverageRecord coverage =
                    audit.JunctionCoverage[index];
                if (coverage.FailureReason == "none")
                {
                    continue;
                }
                failureCount++;
                if (written >= cap)
                {
                    continue;
                }
                if (written > 0)
                {
                    builder.Append('|');
                }
                builder.Append(FormatPlaneCutJunctionCoverage(coverage));
                written++;
            }
            if (failureCount == 0)
            {
                return "none";
            }
            if (failureCount > written)
            {
                builder.Append("|omitted=");
                builder.Append(failureCount - written);
            }
            return builder.ToString();
        }

        private static string FormatPlaneCutNumericalRepairs(
            PlaneCutNumericalRepairTelemetry repairs)
        {
            if (repairs == null)
            {
                return "none";
            }
            return "intersections:" +
                    repairs.IntersectionRequestCount +
                ",strict:" +
                    repairs.StrictCrossingIntersectionCount +
                ",fallbackProjected:" +
                    repairs.ProjectedFallbackIntersectionCount +
                ",sameSideFallbackAttempts:" +
                    repairs.SameSideFallbackAttemptCount +
                ",classifications:" +
                    repairs.StrictInsideClassificationCount + "/" +
                    repairs.StrictOnPlaneClassificationCount + "/" +
                    repairs.StrictOutsideClassificationCount +
                ",onPlaneSnaps:" + repairs.OnPlaneSnapCount +
                ",maxOnPlaneSnap:" +
                    repairs.MaximumOnPlaneSnapDistance.ToString("G9") +
                ",cacheReuse:" +
                    repairs.CachedIntersectionReuseCount +
                ",twoPlaneCorrections:" +
                    repairs.IntersectionProjectionCount +
                ",maxCorrection:" +
                    repairs.MaximumIntersectionProjectionDistance
                        .ToString("G9") +
                ",cutResidual:" +
                    repairs.MaximumCutPlaneResidualBeforeCorrection
                        .ToString("G9") + "/" +
                    repairs.MaximumCutPlaneResidualAfterCorrection
                        .ToString("G9") +
                ",ownerResidual:" +
                    repairs.MaximumOwnerPlaneResidualBeforeCorrection
                        .ToString("G9") + "/" +
                    repairs.MaximumOwnerPlaneResidualAfterCorrection
                        .ToString("G9") +
                ",exactFailures:" +
                    repairs.ExactConstructionFailureCount +
                ",capProjected:" +
                    repairs.CapVertexProjectionCount +
                ",capValidated:" +
                    repairs.CapVertexValidationCount +
                ",capResidualBefore:" +
                    repairs.MaximumCapResidualBeforeProjection
                        .ToString("G9") +
                ",capResidualAfter:" +
                    repairs.MaximumCapResidualAfterProjection
                        .ToString("G9") +
                ",capRejected:" +
                    repairs.CapResidualRejectCount +
                ",weldComparisons:" +
                    repairs.DistanceWeldComparisonCount +
                ",weldMatches:" +
                    repairs.DistanceWeldMatchCount +
                ",weldMoved:" +
                    repairs.DistanceWeldMovedCount +
                ",maxWeldMove:" +
                    repairs.MaximumDistanceWeldMovement
                        .ToString("G9") +
                ",firstExactFailure:{" +
                    FormatPlaneCutFirstExactFailure(repairs) + "}";
        }

        private static string FormatPlaneCutFirstExactFailure(
            PlaneCutNumericalRepairTelemetry repairs)
        {
            if (repairs == null ||
                repairs.FirstExactFailureRecorded == 0)
            {
                return "none";
            }

            return "owner=" +
                    repairs.FirstExactFailureOwnerProvenanceKind + ":" +
                    repairs.FirstExactFailureOwnerProvenanceIndex +
                ",cut=" +
                    repairs.FirstExactFailureCutProvenanceKind + ":" +
                    repairs.FirstExactFailureCutProvenanceIndex +
                ",distance=" +
                    repairs.FirstExactFailureStartDistance
                        .ToString("G9") + "/" +
                    repairs.FirstExactFailureEndDistance
                        .ToString("G9") +
                ",classification=" +
                    repairs.FirstExactFailureStartClassification + "/" +
                    repairs.FirstExactFailureEndClassification +
                ",cutResidual=" +
                    repairs.FirstExactFailureCutResidualBefore
                        .ToString("G9") + "/" +
                    repairs.FirstExactFailureCutResidualAfter
                        .ToString("G9") +
                ",ownerResidual=" +
                    repairs.FirstExactFailureOwnerResidualBefore
                        .ToString("G9") + "/" +
                    repairs.FirstExactFailureOwnerResidualAfter
                        .ToString("G9") +
                ",reason=" +
                    (string.IsNullOrEmpty(
                        repairs.FirstExactFailureReason)
                            ? "none"
                            : repairs.FirstExactFailureReason);
        }

        private static void AppendPlaneCutNumericalRepairDossier(
            StringBuilder builder,
            PlaneCutNumericalRepairTelemetry repairs)
        {
            if (repairs == null)
            {
                builder.AppendLine("none");
                return;
            }

            builder.Append("strictClassificationTolerance=");
            builder.AppendLine(
                (PointMergeDistance * 0.25f).ToString("G9"));
            builder.Append("classificationsInsideOnOutside=");
            builder.Append(repairs.StrictInsideClassificationCount);
            builder.Append('/');
            builder.Append(repairs.StrictOnPlaneClassificationCount);
            builder.Append('/');
            builder.AppendLine(
                repairs.StrictOutsideClassificationCount.ToString());
            builder.Append("intersectionRequests=");
            builder.AppendLine(repairs.IntersectionRequestCount.ToString());
            builder.Append("strictCrossings=");
            builder.AppendLine(
                repairs.StrictCrossingIntersectionCount.ToString());
            builder.Append("sameSideFallbackAttempts=");
            builder.AppendLine(
                repairs.SameSideFallbackAttemptCount.ToString());
            builder.Append("legacyProjectedFallbacks=");
            builder.AppendLine(
                repairs.ProjectedFallbackIntersectionCount.ToString());
            builder.Append("onPlaneSnaps=");
            builder.Append(repairs.OnPlaneSnapCount);
            builder.Append(",maximumMovement=");
            builder.AppendLine(
                repairs.MaximumOnPlaneSnapDistance.ToString("G9"));
            builder.Append("twoPlaneCorrections=");
            builder.Append(repairs.IntersectionProjectionCount);
            builder.Append(",maximumMovement=");
            builder.AppendLine(
                repairs.MaximumIntersectionProjectionDistance
                    .ToString("G9"));
            builder.Append("cutPlaneResidualBeforeAfter=");
            builder.Append(
                repairs.MaximumCutPlaneResidualBeforeCorrection
                    .ToString("G9"));
            builder.Append('/');
            builder.AppendLine(
                repairs.MaximumCutPlaneResidualAfterCorrection
                    .ToString("G9"));
            builder.Append("ownerPlaneResidualBeforeAfter=");
            builder.Append(
                repairs.MaximumOwnerPlaneResidualBeforeCorrection
                    .ToString("G9"));
            builder.Append('/');
            builder.AppendLine(
                repairs.MaximumOwnerPlaneResidualAfterCorrection
                    .ToString("G9"));
            builder.Append("capValidatedProjectedRejected=");
            builder.Append(repairs.CapVertexValidationCount);
            builder.Append('/');
            builder.Append(repairs.CapVertexProjectionCount);
            builder.Append('/');
            builder.AppendLine(repairs.CapResidualRejectCount.ToString());
            builder.Append("capResidualBeforeAfter=");
            builder.Append(
                repairs.MaximumCapResidualBeforeProjection
                    .ToString("G9"));
            builder.Append('/');
            builder.AppendLine(
                repairs.MaximumCapResidualAfterProjection
                    .ToString("G9"));
            builder.Append("distanceWeldComparisonsMatchesMoved=");
            builder.Append(repairs.DistanceWeldComparisonCount);
            builder.Append('/');
            builder.Append(repairs.DistanceWeldMatchCount);
            builder.Append('/');
            builder.AppendLine(repairs.DistanceWeldMovedCount.ToString());
            builder.Append("maximumDistanceWeldMovement=");
            builder.AppendLine(
                repairs.MaximumDistanceWeldMovement.ToString("G9"));
            builder.Append("exactConstructionFailures=");
            builder.AppendLine(
                repairs.ExactConstructionFailureCount.ToString());
            builder.Append("firstExactFailure=");
            builder.AppendLine(FormatPlaneCutFirstExactFailure(repairs));
        }

        private static void AppendPlaneCutConflictWidthReductions(
            StringBuilder builder,
            PlaneCutBevelAuditResult audit)
        {
            if (builder == null)
            {
                return;
            }
            builder.AppendLine(FormatPlaneCutEdgeConflictAudit(audit));
            if (audit.EdgeConflictWidthReductions == null ||
                audit.EdgeConflictWidthReductions.Count == 0)
            {
                builder.AppendLine("records=none");
                return;
            }

            builder.Append("records=");
            builder.AppendLine(
                audit.EdgeConflictWidthReductions.Count.ToString());
            for (int recordIndex = 0;
                 recordIndex < audit.EdgeConflictWidthReductions.Count;
                 recordIndex++)
            {
                PlaneCutConflictWidthReductionRecord record =
                    audit.EdgeConflictWidthReductions[recordIndex];
                builder.Append("pass=");
                builder.Append(record.PassIndex);
                builder.Append(",victim=");
                builder.Append(record.VictimEdgeIndex);
                builder.Append(",foreign=");
                builder.Append(record.ForeignEdgeIndex);
                builder.Append(",vertex=");
                builder.Append(record.VertexIndex);
                builder.Append(",trigger=");
                builder.Append(string.IsNullOrEmpty(
                        record.TriggerCategory)
                    ? "none"
                    : record.TriggerCategory);
                builder.Append(",bandValid=");
                builder.Append(record.BandValid);
                builder.Append(",topologyValid=");
                builder.Append(record.TopologyValid);
                builder.Append(",topology=");
                builder.Append(record.OpenEdgeCount);
                builder.Append('/');
                builder.Append(record.NonManifoldEdgeCount);
                builder.Append('/');
                builder.Append(record.TJunctionCount);
                builder.Append('/');
                builder.Append(record.InvalidFaceCount);
                builder.Append(",nonPlanar=");
                builder.Append(record.NonPlanarFaceCount);
                builder.Append(",rollback=");
                builder.Append(record.TopologyRollbackApplied);
                builder.Append(",cluster={");
                for (int edgeIndex = 0;
                     edgeIndex < record.ClusterEdgeIndices.Count;
                     edgeIndex++)
                {
                    if (edgeIndex > 0)
                    {
                        builder.Append('/');
                    }
                    builder.Append(
                        record.ClusterEdgeIndices[edgeIndex]);
                }
                builder.Append("},clusterReasons={");
                builder.Append(string.IsNullOrEmpty(
                        record.ClusterReasonEvidence)
                    ? "none"
                    : record.ClusterReasonEvidence);
                builder.Append("},previousMinimumScale=");
                builder.Append(
                    record.PreviousMinimumScale.ToString("G9"));
                builder.Append(",requestedScale=");
                builder.Append(record.RequestedScale.ToString("G9"));
                builder.Append(",appliedMinimumScale=");
                builder.Append(
                    record.AppliedMinimumScale.ToString("G9"));
                builder.Append(",clusterFloorScale=");
                builder.Append(
                    record.ClusterFloorScale.ToString("G9"));
                builder.Append(",previousScales={");
                builder.Append(string.IsNullOrEmpty(
                        record.PreviousScaleEvidence)
                    ? "none"
                    : record.PreviousScaleEvidence);
                builder.Append("},rollbackScales={");
                builder.Append(string.IsNullOrEmpty(
                        record.RollbackScaleEvidence)
                    ? "none"
                    : record.RollbackScaleEvidence);
                builder.Append("},appliedScales={");
                builder.Append(string.IsNullOrEmpty(
                        record.AppliedScaleEvidence)
                    ? "none"
                    : record.AppliedScaleEvidence);
                builder.Append("},victimCoverage=");
                builder.Append(
                    record.VictimCoverageRatio.ToString("G9"));
                builder.Append(",foreignAxial=");
                builder.Append(
                    record.ForeignAxialParameter.ToString("G9"));
                builder.Append(",foreignSpan=");
                builder.Append(
                    record.ForeignSharedSpanRatio.ToString("G9"));
                builder.Append(",result=");
                builder.AppendLine(string.IsNullOrEmpty(record.Result)
                    ? "none"
                    : record.Result);
            }
        }

        private static string FormatPlaneCutTopologyTrialValidity(
            int evaluated,
            int valid)
        {
            return evaluated == 1
                ? valid.ToString()
                : "not-evaluated";
        }

        private static string FormatPlaneCutTopologyScaleTrial(
            PlaneCutTopologyScaleTrialRecord trial,
            bool complete)
        {
            if (trial == null)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("trial=");
            builder.Append(trial.TrialIndex);
            builder.Append(",searchMode=");
            builder.Append(string.IsNullOrEmpty(trial.SearchMode)
                ? "none"
                : trial.SearchMode);
            builder.Append(",basePass=");
            builder.Append(trial.BasePassIndex);
            builder.Append(",retreatEdges={");
            for (int edgeIndex = 0;
                 edgeIndex < trial.ClusterEdgeIndices.Count;
                 edgeIndex++)
            {
                if (edgeIndex > 0)
                {
                    builder.Append('/');
                }
                builder.Append(trial.ClusterEdgeIndices[edgeIndex]);
            }
            builder.Append("},protectedEdges={");
            builder.Append(string.IsNullOrEmpty(
                    trial.ProtectedEdgeEvidence)
                ? "none"
                : trial.ProtectedEdgeEvidence);
            builder.Append("},factor=");
            builder.Append(trial.Factor.ToString("G9"));
            builder.Append(",baseScales={");
            builder.Append(string.IsNullOrEmpty(trial.BaseScaleEvidence)
                ? "none"
                : trial.BaseScaleEvidence);
            builder.Append("},requestedScales={");
            builder.Append(string.IsNullOrEmpty(
                    trial.RequestedScaleEvidence)
                ? "none"
                : trial.RequestedScaleEvidence);
            builder.Append("},effectiveScales={");
            builder.Append(string.IsNullOrEmpty(
                    trial.EffectiveScaleEvidence)
                ? "none"
                : trial.EffectiveScaleEvidence);
            builder.Append("},floorHits={");
            builder.Append(string.IsNullOrEmpty(trial.FloorHitEvidence)
                ? "none"
                : trial.FloorHitEvidence);
            builder.Append("},collateralChanged={");
            builder.Append(string.IsNullOrEmpty(
                    trial.CollateralChangedEvidence)
                ? "none"
                : trial.CollateralChangedEvidence);
            builder.Append("},attemptedBuilt=");
            builder.Append(trial.AttemptedBuiltCount);
            builder.Append(",bandValid=");
            builder.Append(FormatPlaneCutTopologyTrialValidity(
                trial.BandEvaluated,
                trial.BandValid));
            builder.Append(",bandVictim=");
            builder.Append(trial.BandVictimEdgeIndex);
            builder.Append(",bandForeign=");
            builder.Append(trial.BandForeignEdgeIndex);
            builder.Append(",bandForeignAxial=");
            builder.Append(trial.BandForeignAxialParameter.ToString("G9"));
            builder.Append(",bandForeignSpan=");
            builder.Append(trial.BandForeignSharedSpanRatio.ToString("G9"));
            builder.Append(",topologyValid=");
            builder.Append(FormatPlaneCutTopologyTrialValidity(
                trial.TopologyEvaluated,
                trial.TopologyValid));
            builder.Append(",faceQualityValid=");
            builder.Append(FormatPlaneCutTopologyTrialValidity(
                trial.FaceQualityEvaluated,
                trial.FaceQualityValid));
            builder.Append(",surfaceValid=");
            builder.Append(trial.SurfaceValid);
            builder.Append(",meshValid=");
            builder.Append(trial.MeshValid);
            builder.Append(",fullyValid=");
            builder.Append(trial.FullyValid);
            builder.Append(",topology=");
            builder.Append(trial.OpenEdgeCount);
            builder.Append('/');
            builder.Append(trial.NonManifoldEdgeCount);
            builder.Append('/');
            builder.Append(trial.TJunctionCount);
            builder.Append('/');
            builder.Append(trial.InvalidFaceCount);
            builder.Append(",nonPlanar=");
            builder.Append(trial.NonPlanarFaceCount);
            builder.Append(",maxDeviation=");
            builder.Append(trial.MaximumPlaneDeviation.ToString("G9"));
            builder.Append(",maxSpread=");
            builder.Append(
                trial.MaximumNormalSpreadDegrees.ToString("G9"));
            builder.Append(",failureStage=");
            builder.Append(string.IsNullOrEmpty(trial.FailureStage)
                ? "none"
                : trial.FailureStage);
            builder.Append(",failureCause=");
            builder.Append(string.IsNullOrEmpty(trial.FailureCause)
                ? "none"
                : trial.FailureCause);
            builder.Append(",result=");
            builder.Append(string.IsNullOrEmpty(trial.Result)
                ? "none"
                : trial.Result);
            if (!complete)
            {
                return builder.ToString();
            }
            if (trial.NonPlanarFaceFailures.Count > 0)
            {
                builder.Append(",faces={");
                for (int failureIndex = 0;
                     failureIndex < trial.NonPlanarFaceFailures.Count;
                     failureIndex++)
                {
                    if (failureIndex > 0)
                    {
                        builder.Append('|');
                    }
                    builder.Append(FormatPlaneCutFaceFailure(
                        trial.NonPlanarFaceFailures[failureIndex],
                        true));
                }
                builder.Append('}');
            }
            if (trial.OpenEdgeFailures.Count > 0)
            {
                builder.Append(",opens={");
                for (int failureIndex = 0;
                     failureIndex < trial.OpenEdgeFailures.Count;
                     failureIndex++)
                {
                    if (failureIndex > 0)
                    {
                        builder.Append('|');
                    }
                    builder.Append(FormatPlaneCutOpenEdgeFailure(
                        trial.OpenEdgeFailures[failureIndex],
                        true));
                }
                builder.Append('}');
            }
            if (trial.TJunctionFailures.Count > 0)
            {
                builder.Append(",tJunctions={");
                for (int failureIndex = 0;
                     failureIndex < trial.TJunctionFailures.Count;
                     failureIndex++)
                {
                    if (failureIndex > 0)
                    {
                        builder.Append('|');
                    }
                    builder.Append(FormatPlaneCutTJunctionFailure(
                        trial.TJunctionFailures[failureIndex],
                        true));
                }
                builder.Append('}');
            }
            return builder.ToString();
        }

        private static void AppendPlaneCutTopologyScaleTrials(
            StringBuilder builder,
            PlaneCutBevelAuditResult audit,
            string searchModeFilter)
        {
            builder.Append("sectionMode=");
            builder.AppendLine(string.IsNullOrEmpty(searchModeFilter)
                ? "all"
                : searchModeFilter);
            builder.Append("finalSearchMode=");
            builder.AppendLine(string.IsNullOrEmpty(
                    audit.TopologyScaleSearchMode)
                ? "none"
                : audit.TopologyScaleSearchMode);
            builder.Append("finalTrigger={");
            builder.Append(string.IsNullOrEmpty(
                    audit.TopologyScaleSearchTriggerEvidence)
                ? "none"
                : audit.TopologyScaleSearchTriggerEvidence);
            builder.AppendLine("}");
            builder.Append("topologyLinked={");
            builder.Append(string.IsNullOrEmpty(
                    audit.TopologyScaleSearchTopologyLinkedEvidence)
                ? "none"
                : audit.TopologyScaleSearchTopologyLinkedEvidence);
            builder.AppendLine("}");
            builder.Append("trialBaseState=");
            builder.AppendLine(audit.TopologyScaleSearchBasePass >= 0
                ? "topologyClean:" +
                    audit.TopologyScaleSearchBasePass.ToString()
                : "none");
            builder.Append("failedStateScalesReused=");
            builder.AppendLine(
                audit.TopologyScaleSearchFailedStateScalesReused
                    .ToString());
            builder.Append("finalRetreatEdges={");
            builder.Append(string.IsNullOrEmpty(
                    audit.TopologyScaleSearchClusterEvidence)
                ? "none"
                : audit.TopologyScaleSearchClusterEvidence);
            builder.AppendLine("}");
            builder.Append("finalProtectedEdges={");
            builder.Append(string.IsNullOrEmpty(
                    audit.TopologyScaleSearchProtectedEvidence)
                ? "none"
                : audit.TopologyScaleSearchProtectedEvidence);
            builder.AppendLine("}");
            builder.Append("finalActiveSearchFailure={stage:");
            builder.Append(string.IsNullOrEmpty(
                    audit.ActiveSearchFailureStage)
                ? "none"
                : audit.ActiveSearchFailureStage);
            builder.Append(",cause:");
            builder.Append(string.IsNullOrEmpty(
                    audit.ActiveSearchFailureCause)
                ? "none"
                : audit.ActiveSearchFailureCause);
            builder.Append(",evidence:{");
            builder.Append(string.IsNullOrEmpty(
                    audit.ActiveSearchFailureEvidence)
                ? "none"
                : audit.ActiveSearchFailureEvidence);
            builder.AppendLine("}}");
            builder.Append("committedFactor=");
            builder.AppendLine(
                audit.TopologyScaleSearchCommittedFactor >= 0f
                    ? audit.TopologyScaleSearchCommittedFactor
                        .ToString("G9")
                    : "none");
            builder.Append("highestFullyValidFactor=");
            builder.AppendLine(
                audit.TopologyScaleSearchHighestValidFactor >= 0f
                    ? audit.TopologyScaleSearchHighestValidFactor
                        .ToString("G9")
                    : "none");
            builder.Append("collateralChanged={");
            builder.Append(string.IsNullOrEmpty(
                    audit.TopologyScaleSearchCollateralChangedEvidence)
                ? "none"
                : audit.TopologyScaleSearchCollateralChangedEvidence);
            builder.AppendLine("}");
            builder.Append("fallbackState=");
            builder.AppendLine(
                audit.TopologyScaleSearchUnresolved == 1 &&
                audit.TopologyScaleSearchBasePass >= 0
                    ? "topologyClean:" +
                        audit.TopologyScaleSearchBasePass.ToString()
                    : "none");
            builder.Append("unresolved=");
            builder.AppendLine(
                audit.TopologyScaleSearchUnresolved.ToString());
            if (audit.TopologyScaleTrials == null ||
                audit.TopologyScaleTrials.Count == 0)
            {
                builder.AppendLine("trials=none");
                return;
            }
            int matchingTrialCount = 0;
            for (int trialIndex = 0;
                 trialIndex < audit.TopologyScaleTrials.Count;
                 trialIndex++)
            {
                PlaneCutTopologyScaleTrialRecord trial =
                    audit.TopologyScaleTrials[trialIndex];
                if (string.IsNullOrEmpty(searchModeFilter) ||
                    string.Equals(
                        trial.SearchMode,
                        searchModeFilter,
                        StringComparison.Ordinal))
                {
                    matchingTrialCount++;
                }
            }
            builder.Append("trials=");
            builder.AppendLine(matchingTrialCount.ToString());
            for (int trialIndex = 0;
                 trialIndex < audit.TopologyScaleTrials.Count;
                 trialIndex++)
            {
                PlaneCutTopologyScaleTrialRecord trial =
                    audit.TopologyScaleTrials[trialIndex];
                if (!string.IsNullOrEmpty(searchModeFilter) &&
                    !string.Equals(
                        trial.SearchMode,
                        searchModeFilter,
                        StringComparison.Ordinal))
                {
                    continue;
                }
                builder.AppendLine(FormatPlaneCutTopologyScaleTrial(
                    trial,
                    true));
            }
        }

        private static string BuildPlaneCutDetailedTelemetry(
            PlaneCutBevelAuditResult audit,
            bool cornerSolutionValid,
            string cornerBlocker)
        {
            StringBuilder builder = new StringBuilder(8192);
            builder.AppendLine(
                "GeneratedMass all-edge bevel rebuild telemetry");
            builder.AppendLine("mode=edge-plane-shell");
            builder.Append("cornerSolutionValid=");
            builder.AppendLine(cornerSolutionValid ? "1" : "0");
            builder.Append("cornerTrace=");
            builder.AppendLine(string.IsNullOrEmpty(cornerBlocker)
                ? "none"
                : cornerBlocker);
            builder.Append("primaryFailure=");
            builder.AppendLine(FormatPlaneCutPrimaryFailure(audit));
            builder.AppendLine();
            builder.AppendLine("[Evaluation Summary]");
            builder.AppendLine(FormatPlaneCutBevelAuditFields(audit));
            builder.Append("legacyLocalJunctionDiagnostic=");
            builder.AppendLine(string.IsNullOrEmpty(
                    audit.LocalJunctionDiagnostic)
                ? "none"
                : audit.LocalJunctionDiagnostic);
            builder.AppendLine();
            builder.AppendLine("[Edge Coverage Summary]");
            builder.AppendLine(FormatEdgeWearCoverageSummary(
                audit.CoverageAudit));
            builder.AppendLine();
            builder.AppendLine("[Edge Lifecycle]");
            AppendEdgeWearCoverageLifecycle(
                builder,
                audit.CoverageAudit);
            builder.AppendLine();
            builder.AppendLine("[Transactional Solver States]");
            builder.Append("latestAttempted=");
            builder.AppendLine(FormatPlaneCutSolverTransactionState(
                audit.LatestAttemptedState));
            builder.Append("latestBandClean=");
            builder.AppendLine(FormatPlaneCutSolverTransactionState(
                audit.LatestBandCleanState));
            builder.Append("latestTopologyClean=");
            builder.AppendLine(FormatPlaneCutSolverTransactionState(
                audit.LatestTopologyCleanState));
            builder.Append("latestCertified=");
            builder.AppendLine(FormatPlaneCutSolverTransactionState(
                audit.LatestCertifiedState));
            builder.AppendLine();
            builder.AppendLine("[Retry Failure Dossiers]");
            AppendPlaneCutRetryFailureDossiers(builder, audit);
            builder.AppendLine();
            builder.AppendLine("[Conflict Width Reduction]");
            AppendPlaneCutConflictWidthReductions(builder, audit);
            builder.AppendLine();
            builder.AppendLine("[Direct Foreign Band-Plane Retreat Search]");
            AppendPlaneCutTopologyScaleTrials(
                builder,
                audit,
                "direct-foreign-band-plane-retreat");
            builder.AppendLine();
            builder.AppendLine("[Dual-Endpoint Foreign-Plane Retreat Search]");
            AppendPlaneCutTopologyScaleTrials(
                builder,
                audit,
                "dual-endpoint-foreign-plane-retreat");
            builder.AppendLine();
            builder.AppendLine("[T-Junction Failures]");
            builder.Append("firstStage=");
            builder.AppendLine(string.IsNullOrEmpty(
                    audit.FirstTJunctionStage)
                ? "none"
                : audit.FirstTJunctionStage);
            builder.Append("count=");
            builder.AppendLine((audit.TJunctionFailures == null
                ? 0
                : audit.TJunctionFailures.Count).ToString());
            if (audit.TJunctionFailures != null)
            {
                for (int index = 0;
                     index < audit.TJunctionFailures.Count;
                     index++)
                {
                    builder.Append(index);
                    builder.Append(':');
                    builder.AppendLine(FormatPlaneCutTJunctionFailure(
                        audit.TJunctionFailures[index],
                        true));
                }
            }
            builder.AppendLine();
            builder.AppendLine("[Locality Deferrals]");
            builder.Append("count=");
            builder.AppendLine((audit.LocalityDeferrals == null
                ? 0
                : audit.LocalityDeferrals.Count).ToString());
            if (audit.LocalityDeferrals != null)
            {
                for (int index = 0;
                     index < audit.LocalityDeferrals.Count;
                     index++)
                {
                    builder.Append(index);
                    builder.Append(':');
                    builder.AppendLine(FormatPlaneCutLocalityDeferral(
                        audit.LocalityDeferrals[index],
                        true));
                }
            }
            builder.AppendLine();
            builder.AppendLine("[Numerical Repairs]");
            builder.AppendLine(FormatPlaneCutNumericalRepairs(
                audit.NumericalRepairs));
            builder.AppendLine();
            builder.AppendLine("[Strict Intersection Contract]");
            AppendPlaneCutNumericalRepairDossier(
                builder,
                audit.NumericalRepairs);
            builder.AppendLine();
            builder.AppendLine("[Stage Timeline]");
            builder.AppendLine(FormatPlaneCutStageSnapshot(
                audit.StagePlaneConstruction));
            builder.AppendLine(FormatPlaneCutStageSnapshot(
                audit.StageSanitized));
            builder.AppendLine(FormatPlaneCutStageSnapshot(
                audit.StageWelded));
            builder.AppendLine(FormatPlaneCutStageSnapshot(
                audit.StageConformed));
            builder.AppendLine(FormatPlaneCutStageSnapshot(
                audit.StageSeamRepaired));
            builder.AppendLine(FormatPlaneCutStageSnapshot(
                audit.StageFinalCertification));
            builder.Append("firstOpenEdgeStage=");
            builder.AppendLine(string.IsNullOrEmpty(audit.FirstOpenEdgeStage)
                ? "none"
                : audit.FirstOpenEdgeStage);
            builder.Append("firstTJunctionStage=");
            builder.AppendLine(string.IsNullOrEmpty(
                    audit.FirstTJunctionStage)
                ? "none"
                : audit.FirstTJunctionStage);
            builder.Append("firstNonPlanarStage=");
            builder.AppendLine(string.IsNullOrEmpty(
                    audit.FirstNonPlanarStage)
                ? "none"
                : audit.FirstNonPlanarStage);
            builder.AppendLine();
            builder.AppendLine("[Face Quality Failures]");
            builder.Append("count=");
            builder.AppendLine((audit.FaceQualityFailures == null
                ? 0
                : audit.FaceQualityFailures.Count).ToString());
            if (audit.FaceQualityFailures != null)
            {
                for (int index = 0;
                     index < audit.FaceQualityFailures.Count;
                     index++)
                {
                    builder.Append(index);
                    builder.Append(':');
                    builder.AppendLine(FormatPlaneCutFaceFailure(
                        audit.FaceQualityFailures[index],
                        true));
                }
            }
            builder.AppendLine();
            builder.AppendLine("[Open Edge Failures]");
            builder.Append("count=");
            builder.AppendLine((audit.OpenEdgeFailures == null
                ? 0
                : audit.OpenEdgeFailures.Count).ToString());
            if (audit.OpenEdgeFailures != null)
            {
                for (int index = 0;
                     index < audit.OpenEdgeFailures.Count;
                     index++)
                {
                    builder.Append(index);
                    builder.Append(':');
                    builder.AppendLine(FormatPlaneCutOpenEdgeFailure(
                        audit.OpenEdgeFailures[index],
                        true));
                }
            }
            builder.AppendLine();
            builder.AppendLine("[Legacy Junction Heuristic - Non-Authoritative]");
            builder.Append("touched=");
            builder.Append(audit.JunctionCoverageTouchedVertexCount);
            builder.Append(",expected=");
            builder.Append(audit.JunctionCoverageExpectedCount);
            builder.Append(",built=");
            builder.Append(audit.JunctionCoverageBuiltCount);
            builder.Append(",missing=");
            builder.AppendLine(audit.JunctionCoverageMissingCount.ToString());
            if (audit.JunctionCoverage != null)
            {
                for (int index = 0;
                     index < audit.JunctionCoverage.Count;
                     index++)
                {
                    builder.Append(index);
                    builder.Append(':');
                    builder.AppendLine(FormatPlaneCutJunctionCoverage(
                        audit.JunctionCoverage[index]));
                }
            }
            builder.AppendLine();
            builder.AppendLine("[Preparation Movement]");
            builder.Append("boundaryConformityTouched=");
            builder.AppendLine(FormatPlaneCutStringSet(
                audit.BoundaryConformityTouchedFaces));
            builder.Append("seamRepairTouched=");
            builder.AppendLine(FormatPlaneCutStringSet(
                audit.SeamRepairTouchedFaces));
            builder.Append("seamRepairMovement=");
            builder.AppendLine(FormatPlaneCutFloatDictionary(
                audit.SeamRepairMaximumMovementByIdentity));
            builder.AppendLine();
            builder.AppendLine("[Geometry Commit]");
            builder.AppendLine("geometryCommit=disabled");
            return builder.ToString();
        }

        private static string FormatPlaneCutStringSet(
            HashSet<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return "none";
            }
            List<string> ordered = new List<string>(values);
            ordered.Sort(StringComparer.Ordinal);
            return string.Join("/", ordered);
        }

        private static string FormatPlaneCutFloatDictionary(
            Dictionary<string, float> values)
        {
            if (values == null || values.Count == 0)
            {
                return "none";
            }
            List<string> keys = new List<string>(values.Keys);
            keys.Sort(StringComparer.Ordinal);
            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < keys.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append('/');
                }
                string key = keys[index];
                builder.Append(key);
                builder.Append('=');
                builder.Append(values[key].ToString("G9"));
            }
            return builder.ToString();
        }

        private static string FormatEdgeWearCoverageSummary(
            EdgeWearCoverageAudit audit)
        {
            if (audit == null)
            {
                return "notCaptured";
            }

            RecalculateEdgeWearCoverageAudit(audit);
            return "max=" + (audit.MaximumCoverageMode ? "1" : "0") +
                ",source=" + audit.SourceEdgeCount +
                ",structural=" + audit.StructuralEligibleCount +
                ",artistic=" + audit.ArtisticEligibleCount +
                ",wouldBeArtisticallyFiltered=" +
                    audit.ArtisticFilteredCount +
                ",candidates=" + audit.CandidateCount +
                ",selected=" + audit.SelectedCount +
                ",widthInactive=" + audit.WidthInactiveCount +
                ",widthReduced=" + audit.WidthReducedCount +
                ",active=" + audit.ActiveCount +
                ",attemptedBuilt=" + audit.AttemptedBuiltCount +
                ",certifiedBuilt=" + audit.BuiltCount +
                ",trialRejected=" + audit.TrialRejectedCount +
                ",deferred=" + audit.DeferredCount +
                ",rejected=" + audit.RejectedCount +
                ",unmapped=" + audit.UnmappedCount;
        }

        private static string FormatEdgeWearCoverageIdSummary(
            EdgeWearCoverageAudit audit)
        {
            if (audit == null)
            {
                return "notCaptured";
            }

            return "structuralIneligible={" +
                    FormatEdgeWearCoverageIds(
                        audit,
                        "structural-ineligible") + "}" +
                ",wouldBeArtisticallyFiltered={" +
                    FormatEdgeWearCoverageIds(
                        audit,
                        "artistic-filtered") + "}" +
                ",widthInactive={" +
                    FormatEdgeWearCoverageIds(
                        audit,
                        "width-inactive") + "}" +
                ",trialRejected={" +
                    FormatEdgeWearCoverageIds(
                        audit,
                        "trial-rejected") + "}" +
                ",deferred={" +
                    FormatEdgeWearCoverageIds(
                        audit,
                        "deferred") + "}" +
                ",rejected={" +
                    FormatEdgeWearCoverageIds(
                        audit,
                        "rejected") + "}";
        }

        private static string FormatEdgeWearCoverageIds(
            EdgeWearCoverageAudit audit,
            string category)
        {
            if (audit == null || audit.Records == null)
            {
                return "none";
            }

            List<int> indices = new List<int>();
            for (int recordIndex = 0;
                 recordIndex < audit.Records.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord record =
                    audit.Records[recordIndex];
                bool include = category switch
                {
                    "structural-ineligible" =>
                        !record.StructuralEligible,
                    "artistic-filtered" =>
                        record.StructuralEligible &&
                        !record.ArtisticEligible,
                    "width-inactive" => record.WidthInactive,
                    "trial-rejected" => record.TrialRejected,
                    "deferred" => record.Deferred,
                    "rejected" => record.Rejected,
                    _ => false
                };
                if (include)
                {
                    indices.Add(record.SourceEdgeIndex);
                }
            }

            if (indices.Count == 0)
            {
                return "none";
            }

            indices.Sort();
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

        private static void AppendEdgeWearCoverageLifecycle(
            StringBuilder builder,
            EdgeWearCoverageAudit audit)
        {
            if (builder == null)
            {
                return;
            }
            if (audit == null || audit.Records == null)
            {
                builder.AppendLine("notCaptured");
                return;
            }

            List<EdgeWearEdgeLifecycleRecord> ordered =
                new List<EdgeWearEdgeLifecycleRecord>(audit.Records);
            ordered.Sort((left, right) =>
                left.SourceEdgeIndex.CompareTo(right.SourceEdgeIndex));
            builder.Append("count=");
            builder.AppendLine(ordered.Count.ToString());
            for (int recordIndex = 0;
                 recordIndex < ordered.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord record = ordered[recordIndex];
                builder.Append("edge=");
                builder.Append(record.SourceEdgeIndex);
                builder.Append(",segment=");
                builder.Append(FormatPlaneCutVector(record.Start));
                builder.Append("->");
                builder.Append(FormatPlaneCutVector(record.End));
                builder.Append(",faces=");
                builder.Append(record.FaceA);
                builder.Append('/');
                builder.Append(record.FaceB);
                builder.Append('/');
                builder.Append(record.FaceCount);
                builder.Append(",length=");
                builder.Append(record.Length.ToString("G9"));
                builder.Append(",dihedral=");
                builder.Append(record.DihedralDegrees.ToString("G9"));
                builder.Append(",vertical01=");
                builder.Append(record.Vertical01.ToString("G9"));
                builder.Append(",classification=");
                builder.Append(record.Classification);
                builder.Append(",structural=");
                builder.Append(record.StructuralEligible ? '1' : '0');
                builder.Append(",artistic=");
                builder.Append(record.ArtisticEligible ? '1' : '0');
                builder.Append(",candidate=");
                builder.Append(record.Candidate ? '1' : '0');
                builder.Append(",candidateIndex=");
                builder.Append(record.CandidateIndex);
                builder.Append(",candidateReason=");
                builder.Append(string.IsNullOrEmpty(record.CandidateReason)
                    ? "none"
                    : record.CandidateReason);
                builder.Append(",score=");
                builder.Append(record.Score.ToString("G9"));
                builder.Append(",selected=");
                builder.Append(record.Selected ? '1' : '0');
                builder.Append(",solvedWidth=");
                builder.Append(record.SolvedWidth.ToString("G9"));
                builder.Append(",materializedWidth=");
                builder.Append(record.MaterializedWidth.ToString("G9"));
                builder.Append(",materializedWidthScale=");
                builder.Append(
                    record.MaterializedWidthScale.ToString("G9"));
                builder.Append(",widthReduced=");
                builder.Append(record.WidthReduced ? '1' : '0');
                builder.Append(",widthInactive=");
                builder.Append(record.WidthInactive ? '1' : '0');
                builder.Append(",active=");
                builder.Append(record.Active ? '1' : '0');
                builder.Append(",attemptedBuilt=");
                builder.Append(record.AttemptedBuilt ? '1' : '0');
                builder.Append(",certifiedBuilt=");
                builder.Append(record.Built ? '1' : '0');
                builder.Append(",trialRejected=");
                builder.Append(record.TrialRejected ? '1' : '0');
                builder.Append(",deferred=");
                builder.Append(record.Deferred ? '1' : '0');
                builder.Append(",rejected=");
                builder.Append(record.Rejected ? '1' : '0');
                builder.Append(",finalReason=");
                builder.AppendLine(string.IsNullOrEmpty(record.FinalReason)
                    ? "none"
                    : record.FinalReason);
            }
        }

        private static void LogUnifiedAllEdgeBevelAudit(
            PlaneCutBevelAuditResult audit,
            bool cornerSolutionValid,
            string cornerBlocker)
        {
#if UNITY_EDITOR
            const string relativePath =
                "Library/GeneratedMassEdgeWearTelemetry.txt";
            int writeSucceeded = 0;
            string writeFailure = string.Empty;
            string detailed = BuildPlaneCutDetailedTelemetry(
                audit,
                cornerSolutionValid,
                cornerBlocker);
            try
            {
                string projectRoot = Path.GetFullPath(
                    Path.Combine(Application.dataPath, ".."));
                string fullPath = Path.Combine(
                    projectRoot,
                    "Library",
                    "GeneratedMassEdgeWearTelemetry.txt");
                string directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(
                    fullPath,
                    detailed,
                    new UTF8Encoding(
                        encoderShouldEmitUTF8Identifier: false));
                writeSucceeded = 1;
            }
            catch (Exception exception)
            {
                writeFailure =
                    exception.GetType().Name + ":" + exception.Message;
            }

            string message =
                "GeneratedMass all-edge bevel rebuild audit. " +
                "mode=edgePlaneShell" +
                ",primaryFailure:{" +
                    FormatPlaneCutPrimaryFailure(audit) + "}" +
                ",cornerValid:" +
                    (cornerSolutionValid ? "1" : "0") +
                ",valid:" + audit.GeometryValid +
                ",geometryValid:" + audit.GeometryValid +
                ",coverageValid:" +
                    audit.MaterializedEdgeCoverageValid +
                ",selected:" + audit.SelectedEdgeCount +
                ",active:" + audit.ActiveEdgeCount +
                ",attemptedBuilt:" + audit.AttemptedPlanesBuilt +
                ",certifiedBuilt:" + audit.CertifiedPlanesBuilt +
                ",trialRejected:" + audit.TrialRejectedPlanes +
                ",built:" + audit.PlanesBuilt +
                ",deferred:" + audit.PlanesDeferred +
                ",rejected:" + audit.PlanesRejected +
                ",materializedCoverage:" +
                    audit.MaterializedEdgeCoverageValid +
                ",coverage:{" +
                    FormatEdgeWearCoverageSummary(
                        audit.CoverageAudit) + "}" +
                ",coverageIds:{" +
                    FormatEdgeWearCoverageIdSummary(
                        audit.CoverageAudit) + "}" +
                ",conflictSolve:{" +
                    FormatPlaneCutEdgeConflictAudit(audit) + "}" +
                ",solverStates:{attempted=" +
                    (audit.LatestAttemptedState == null
                        ? -1
                        : audit.LatestAttemptedState.PassIndex) +
                    ",bandClean=" +
                    (audit.LatestBandCleanState == null
                        ? -1
                        : audit.LatestBandCleanState.PassIndex) +
                    ",topologyClean=" +
                    (audit.LatestTopologyCleanState == null
                        ? -1
                        : audit.LatestTopologyCleanState.PassIndex) +
                    ",certified=" +
                    (audit.LatestCertifiedState == null
                        ? -1
                        : audit.LatestCertifiedState.PassIndex) + "}" +
                ",retryFailures:{" +
                    FormatCappedPlaneCutRetryFailures(audit, 2) + "}" +
                ",surfaceFaces:" + audit.BevelRegionFaceCount +
                ",surfaceTriangles:" +
                    audit.BevelRegionTriangleCount +
                ",surfaceRenderValid:" +
                    audit.BevelRegionRenderValid +
                ",internalFanVertices:" +
                    audit.BevelRegionInternalFanVertexCount +
                ",topology:" +
                    audit.OpenEdgeCount + "/" +
                    audit.NonManifoldEdgeCount + "/" +
                    audit.TJunctionCount + "/" +
                    audit.InvalidFaceCount +
                ",faceQuality=count:" +
                    audit.FaceQualityNonPlanarCount +
                    ",planarityLimit:" +
                    audit.FaceQualityPlanarityTolerance.ToString("G9") +
                    ",spreadLimit:" +
                    audit.FaceQualityNormalSpreadToleranceDegrees
                        .ToString("G9") +
                    ",examples:{" +
                    FormatCappedPlaneCutFaceFailures(audit, 3) + "}" +
                ",numerics:{" +
                    FormatPlaneCutNumericalRepairs(
                        audit.NumericalRepairs) + "}" +
                ",openEdges=count:" + audit.OpenEdgeCount +
                    ",firstStage:" +
                    (string.IsNullOrEmpty(audit.FirstOpenEdgeStage)
                        ? "none"
                        : audit.FirstOpenEdgeStage) +
                    ",examples:{" +
                    FormatCappedPlaneCutOpenEdges(audit, 4) + "}" +
                ",tJunctions=count:" + audit.TJunctionCount +
                    ",firstStage:" +
                    (string.IsNullOrEmpty(audit.FirstTJunctionStage)
                        ? "none"
                        : audit.FirstTJunctionStage) +
                    ",examples:{" +
                    FormatCappedPlaneCutTJunctions(audit, 1) + "}" +
                ",localityDeferrals=count:" +
                    (audit.LocalityDeferrals == null
                        ? 0
                        : audit.LocalityDeferrals.Count) +
                    ",examples:{" +
                    FormatCappedPlaneCutLocalityDeferrals(audit, 2) + "}" +
                ",legacyJunctionHeuristic=nonAuthoritative:1" +
                    ",touched:" +
                    audit.JunctionCoverageTouchedVertexCount +
                    ",expected:" +
                    audit.JunctionCoverageExpectedCount +
                    ",built:" + audit.JunctionCoverageBuiltCount +
                    ",missing:" + audit.JunctionCoverageMissingCount +
                ",stageTimeline:{" +
                    FormatPlaneCutStageTimeline(audit) + "}" +
                ",meshTriangles:" + audit.PreviewTriangleCount +
                ",meshValid:" + audit.PreviewGeometryValid +
                ",edges=active:{" +
                    (string.IsNullOrEmpty(audit.ActiveEdgeEvidence)
                        ? "none"
                        : audit.ActiveEdgeEvidence) + "}" +
                    ",attempted:{" +
                    (string.IsNullOrEmpty(audit.AttemptedEdgeEvidence)
                        ? "none"
                        : audit.AttemptedEdgeEvidence) + "}" +
                    ",certified:{" +
                    (string.IsNullOrEmpty(audit.BuiltEdgeEvidence)
                        ? "none"
                        : audit.BuiltEdgeEvidence) + "}" +
                    ",trialRejected:{" +
                    (string.IsNullOrEmpty(
                            audit.TrialRejectedEdgeEvidence)
                        ? "none"
                        : audit.TrialRejectedEdgeEvidence) + "}" +
                    ",deferred:{" +
                    (string.IsNullOrEmpty(audit.DeferredEdgeEvidence)
                        ? "none"
                        : audit.DeferredEdgeEvidence) + "}" +
                ",telemetry=path:" + relativePath +
                    ",write:" + writeSucceeded +
                    (string.IsNullOrEmpty(writeFailure)
                        ? string.Empty
                        : ",writeFailure:" + writeFailure) +
                (string.IsNullOrEmpty(audit.Diagnostic)
                    ? string.Empty
                    : ",trace:" + audit.Diagnostic) +
                ",geometryCommit=disabled";
            LogChamferNoStackTrace(
                message,
                audit.GeometryValid != 1 ||
                (audit.CoverageAudit != null &&
                 audit.CoverageAudit.MaximumCoverageMode &&
                 audit.MaterializedEdgeCoverageValid != 1));
#endif
        }

        private static void LogBoundedSingleEdgeAudit(
            BoundedSingleEdgeAuditResult audit)
        {
#if UNITY_EDITOR
            string message =
                "GeneratedMass bounded edge compact audit. " +
                "boundedEdge=" +
                    "candidateCount:" + audit.CandidateCount +
                    ",selectedOrdinal:" + audit.SelectedOrdinal +
                    ",sourceEdge:" + audit.SourceEdgeIndex +
                    ",isolatedRailSolved:" +
                        audit.IsolatedRailSolved +
                    ",widthAttempts:" + audit.WidthAttemptCount +
                    ",solvedWidth:" +
                        audit.SolvedWidth.ToString("G6") +
                    ",canonicalRails:" +
                        audit.CanonicalRailCount +
                    ",maxBoundarySnap:" +
                        audit.MaximumBoundarySnapDistance.ToString("G6") +
                    ",targetBoundaries:" +
                        audit.TargetBoundaryCount +
                    ",ownerClips:" + audit.OwnerClipCount +
                    ",boundarySubdivisions:" +
                        audit.BoundarySubdivisionCount +
                    ",bevelFaces:" + audit.BevelFaceCount +
                    ",endpointCaps:" + audit.EndpointCapCount +
                    ",modifiedSourceFaces:" +
                        audit.ModifiedSourceFaceCount +
                    ",ownerSourceFacesModified:" +
                        audit.OwnerSourceFaceModifiedCount +
                    ",endpointSupportFacesModified:" +
                        audit.EndpointSupportSourceFaceModifiedCount +
                    ",unexpectedSourceFacesModified:" +
                        audit.UnexpectedSourceFaceModifiedCount +
                    ",boundaryOnlyUnexpectedSourceFaces:" +
                        audit.BoundaryOnlyUnexpectedSourceFaceCount +
                    ",foreignSourceFacesModified:" +
                        audit.ForeignSourceFaceModifiedCount +
                    ",foreignBoundarySubdivided:" +
                        audit.ForeignBoundarySubdividedCount +
                    ",preparedSourceChangeComparisonAttempted:" +
                        audit.PreparedSourceChangeComparisonAttempted +
                    ",railDeviation:" +
                        audit.RailDeviation.ToString("G6") +
                    ",maxExtentBeyondRails:" +
                        audit.MaximumExtentBeyondRails.ToString("G6") +
                    ",valid:" + audit.GeometryValid +
                ", boundedEdgeClass=" +
                    "attempted:" + audit.EdgeClassificationAttempted +
                    ",classification:" + audit.EdgeClassification +
                    ",sourceFaceA:" + audit.EdgeSourceFaceA +
                    ",sourceFaceB:" + audit.EdgeSourceFaceB +
                    ",normalA:" + FormatBoundedAuditVector(
                        audit.EdgeNormalA) +
                    ",normalB:" + FormatBoundedAuditVector(
                        audit.EdgeNormalB) +
                    ",normalDot:" +
                        audit.EdgeNormalDot.ToString("G9") +
                    ",dihedralDegrees:" +
                        audit.EdgeDihedralDegrees.ToString("G9") +
                    ",faceAInteriorAgainstFaceB:" +
                        audit.EdgeFaceAInteriorAgainstFaceB.ToString("G9") +
                    ",faceBInteriorAgainstFaceA:" +
                        audit.EdgeFaceBInteriorAgainstFaceA.ToString("G9") +
                    ",solidCentreAgainstFaceA:" +
                        audit.EdgeSolidCentreAgainstFaceA.ToString("G9") +
                    ",solidCentreAgainstFaceB:" +
                        audit.EdgeSolidCentreAgainstFaceB.ToString("G9") +
                    ",tolerance:" +
                        audit.EdgeClassificationTolerance.ToString("G9") +
                    ",poolConvex:" + audit.ConvexCandidateCount +
                    ",poolConcave:" + audit.ConcaveCandidateCount +
                    ",poolCoplanar:" + audit.CoplanarCandidateCount +
                    ",poolAmbiguous:" + audit.AmbiguousCandidateCount +
                    ",poolInvalidOrientation:" +
                        audit.InvalidOrientationCandidateCount +
                ", boundedOwner=" +
                    "attempted:" + audit.OwnerClipAttemptedCount +
                    ",clipped:" + audit.OwnerClipCount +
                    ",intersectionFailure:" +
                        audit.OwnerIntersectionFailureCount +
                    ",degenerate:" + audit.OwnerDegenerateCount +
                    ",nonPlanar:" + audit.OwnerNonPlanarCount +
                    ",nonSimple:" + audit.OwnerNonSimpleCount +
                    ",nonConvex:" + audit.OwnerNonConvexCount +
                    ",windingFailure:" +
                        audit.OwnerWindingFailureCount +
                ", boundedEndpointSupport=" +
                    "attempted:" +
                        audit.EndpointSupportClipAttemptedCount +
                    ",clipped:" + audit.EndpointSupportClipCount +
                    ",faceA:" + audit.EndpointSupportFaceA +
                    ",faceB:" + audit.EndpointSupportFaceB +
                    ",graphFaceA:" +
                        audit.EndpointSupportGraphFaceA +
                    ",graphFaceB:" +
                        audit.EndpointSupportGraphFaceB +
                    ",vertexA:" + audit.EndpointSupportVertexA +
                    ",vertexB:" + audit.EndpointSupportVertexB +
                    ",previousEdgeA:" +
                        audit.EndpointSupportPreviousEdgeA +
                    ",previousEdgeB:" +
                        audit.EndpointSupportPreviousEdgeB +
                    ",nextEdgeA:" + audit.EndpointSupportNextEdgeA +
                    ",nextEdgeB:" + audit.EndpointSupportNextEdgeB +
                    ",previousRailA:" +
                        audit.EndpointSupportPreviousRailA +
                    ",previousRailB:" +
                        audit.EndpointSupportPreviousRailB +
                    ",nextRailA:" + audit.EndpointSupportNextRailA +
                    ",nextRailB:" + audit.EndpointSupportNextRailB +
                    ",sourcePositionA:" + FormatBoundedAuditVector(
                        audit.EndpointSupportSourcePositionA) +
                    ",sourcePositionB:" + FormatBoundedAuditVector(
                        audit.EndpointSupportSourcePositionB) +
                    ",previousRailPositionA:" +
                        FormatBoundedAuditVector(
                            audit.EndpointSupportPreviousRailPositionA) +
                    ",previousRailPositionB:" +
                        FormatBoundedAuditVector(
                            audit.EndpointSupportPreviousRailPositionB) +
                    ",nextRailPositionA:" + FormatBoundedAuditVector(
                        audit.EndpointSupportNextRailPositionA) +
                    ",nextRailPositionB:" + FormatBoundedAuditVector(
                        audit.EndpointSupportNextRailPositionB) +
                    ",normalA:" + FormatBoundedAuditVector(
                        audit.EndpointSupportNormalA) +
                    ",normalB:" + FormatBoundedAuditVector(
                        audit.EndpointSupportNormalB) +
                    ",previousParameterA:" +
                        audit.EndpointSupportPreviousParameterA
                            .ToString("G9") +
                    ",previousParameterB:" +
                        audit.EndpointSupportPreviousParameterB
                            .ToString("G9") +
                    ",nextParameterA:" +
                        audit.EndpointSupportNextParameterA.ToString("G9") +
                    ",nextParameterB:" +
                        audit.EndpointSupportNextParameterB.ToString("G9") +
                    ",previousEdgeResidualA:" +
                        audit.EndpointSupportPreviousEdgeResidualA
                            .ToString("G9") +
                    ",previousEdgeResidualB:" +
                        audit.EndpointSupportPreviousEdgeResidualB
                            .ToString("G9") +
                    ",nextEdgeResidualA:" +
                        audit.EndpointSupportNextEdgeResidualA
                            .ToString("G9") +
                    ",nextEdgeResidualB:" +
                        audit.EndpointSupportNextEdgeResidualB
                            .ToString("G9") +
                    ",previousPlaneResidualA:" +
                        audit.EndpointSupportPreviousPlaneResidualA
                            .ToString("G9") +
                    ",previousPlaneResidualB:" +
                        audit.EndpointSupportPreviousPlaneResidualB
                            .ToString("G9") +
                    ",nextPlaneResidualA:" +
                        audit.EndpointSupportNextPlaneResidualA
                            .ToString("G9") +
                    ",nextPlaneResidualB:" +
                        audit.EndpointSupportNextPlaneResidualB
                            .ToString("G9") +
                    ",sharedFaceFailure:" +
                        audit.EndpointSupportSharedFaceFailureCount +
                    ",incidenceFailure:" +
                        audit.EndpointSupportIncidenceFailureCount +
                    ",degenerate:" +
                        audit.EndpointSupportDegenerateCount +
                    ",nonPlanar:" +
                        audit.EndpointSupportNonPlanarCount +
                    ",nonSimple:" +
                        audit.EndpointSupportNonSimpleCount +
                    ",nonConvex:" +
                        audit.EndpointSupportNonConvexCount +
                    ",windingFailure:" +
                        audit.EndpointSupportWindingFailureCount +
                    ",removedVertices:" +
                        audit.EndpointSupportRemovedVertexCount +
                    ",railInsertions:" +
                        audit.EndpointSupportRailInsertionCount +
                ", boundedPrepare=" +
                    FormatBoundedPreparationAudit(
                        audit.ResultPreparation) +
                    ",failedCanonicalSubdivision:" +
                        audit.PrepareFailedCanonicalSubdivision +
                ", boundedSourcePrepare=" +
                    FormatBoundedPreparationAudit(
                        audit.SourcePreparation) +
                ", boundedSourceProvenance=" +
                    "certified:" +
                        audit.SourceProvenanceCertificationValid +
                    ",raw:{" +
                        FormatBoundedSourceProvenanceAudit(
                            audit.RawSourceProvenance) + "}" +
                    ",prepared:{" +
                        FormatBoundedSourceProvenanceAudit(
                            audit.PreparedSourceProvenance) + "}" +
                    ",result:{" +
                        FormatBoundedSourceProvenanceAudit(
                            audit.ResultSourceProvenance) + "}" +
                ", boundedSourceChanges=" +
                    "baseline:prepared" +
                    ",preparedAttempted:" +
                        audit.PreparedSourceChangeComparisonAttempted +
                    ",preparedModified:" +
                        audit.ModifiedSourceFaceCount +
                    ",preparedOwnerModified:" +
                        audit.OwnerSourceFaceModifiedCount +
                    ",preparedSupportModified:" +
                        audit.EndpointSupportSourceFaceModifiedCount +
                    ",preparedUnexpectedModified:" +
                        audit.UnexpectedSourceFaceModifiedCount +
                    ",preparedBoundaryOnlyUnexpected:" +
                        audit.BoundaryOnlyUnexpectedSourceFaceCount +
                    ",preparedForeignModified:" +
                        audit.ForeignSourceFaceModifiedCount +
                    ",preparedForeignBoundarySubdivided:" +
                        audit.ForeignBoundarySubdividedCount +
                    ",rawModified:" +
                        audit.RawModifiedSourceFaceCount +
                    ",rawOwnerModified:" +
                        audit.RawOwnerSourceFaceModifiedCount +
                    ",rawSupportModified:" +
                        audit.RawEndpointSupportSourceFaceModifiedCount +
                    ",rawUnexpectedModified:" +
                        audit.RawUnexpectedSourceFaceModifiedCount +
                    ",rawBoundaryOnlyUnexpected:" +
                        audit.RawBoundaryOnlyUnexpectedSourceFaceCount +
                    ",rawForeignModified:" +
                        audit.RawForeignSourceFaceModifiedCount +
                    ",rawForeignBoundarySubdivided:" +
                        audit.RawForeignBoundarySubdividedCount +
                ", boundedTopology=" +
                    "open:" + audit.OpenEdgeCount +
                    ",nonManifold:" + audit.NonManifoldEdgeCount +
                    ",tJunction:" + audit.TJunctionCount +
                    ",invalidFaces:" + audit.InvalidFaceCount +
                ", boundedBounds=" +
                    "attempted:" + audit.CertificationAttempted +
                    ",rawValid:" + audit.BoundsValid +
                    ",preparedValid:" +
                        audit.PreparedBoundsValid +
                    ",tolerance:" +
                        audit.BoundsTolerance.ToString("G9") +
                    ",rawMin:" + FormatBoundedAuditVector(
                        audit.RawSourceBoundsMinimum) +
                    ",rawMax:" + FormatBoundedAuditVector(
                        audit.RawSourceBoundsMaximum) +
                    ",preparedMin:" + FormatBoundedAuditVector(
                        audit.PreparedSourceBoundsMinimum) +
                    ",preparedMax:" + FormatBoundedAuditVector(
                        audit.PreparedSourceBoundsMaximum) +
                    ",resultMin:" + FormatBoundedAuditVector(
                        audit.ResultBoundsMinimum) +
                    ",resultMax:" + FormatBoundedAuditVector(
                        audit.ResultBoundsMaximum) +
                    ",rawMinMargin:" + FormatBoundedAuditVector(
                        audit.RawBoundsMinimumMargin) +
                    ",rawMaxMargin:" + FormatBoundedAuditVector(
                        audit.RawBoundsMaximumMargin) +
                    ",preparedMinMargin:" +
                        FormatBoundedAuditVector(
                            audit.PreparedBoundsMinimumMargin) +
                    ",preparedMaxMargin:" +
                        FormatBoundedAuditVector(
                            audit.PreparedBoundsMaximumMargin) +
                ", boundedSolid=" +
                    "sourceConvexityAttempted:" +
                        audit.SourceConvexityAttempted +
                    ",sourceConvexityViolations:" +
                        audit.SourceConvexityViolationCount +
                    ",sourceMaximumPlaneViolation:" +
                        audit.SourceMaximumPlaneViolation.ToString("G9") +
                    ",sourceViolatingPlaneFace:" +
                        audit.SourceViolatingPlaneFace +
                    ",sourceViolatingVertex:" +
                        audit.SourceViolatingVertexFace + ":" +
                        audit.SourceViolatingVertexIndex +
                    ",resultContainmentAttempted:" +
                        audit.ResultContainmentAttempted +
                    ",resultContainmentViolations:" +
                        audit.ResultContainmentViolationCount +
                    ",resultMaximumOutwardDistance:" +
                        audit.ResultMaximumOutwardDistance.ToString("G9") +
                    ",resultViolatingFace:" +
                        audit.ResultViolatingFace +
                    ",resultViolatingProvenance:" +
                        audit.ResultViolatingProvenanceKind + ":" +
                        audit.ResultViolatingProvenanceIndex +
                    ",resultViolatingVertex:" +
                        audit.ResultViolatingVertexIndex +
                    ",violatedSourcePlane:" +
                        audit.ResultViolatedSourcePlane +
                    ",tolerance:" +
                        audit.SolidContainmentTolerance.ToString("G9") +
                ", boundedResultConvexity=" +
                    "attempted:" + audit.ResultConvexityAttempted +
                    ",violations:" +
                        audit.ResultConvexityViolationCount +
                    ",maximumViolation:" +
                        audit.ResultMaximumConvexityViolation
                            .ToString("G9") +
                    ",planeFace:" + audit.ResultConvexityPlaneFace +
                    ",planeProvenance:" +
                        audit.ResultConvexityPlaneProvenanceKind + ":" +
                        audit.ResultConvexityPlaneProvenanceIndex +
                    ",vertexFace:" + audit.ResultConvexityVertexFace +
                    ",vertexProvenance:" +
                        audit.ResultConvexityVertexProvenanceKind + ":" +
                        audit.ResultConvexityVertexProvenanceIndex +
                    ",vertexIndex:" +
                        audit.ResultConvexityVertexIndex +
                ", boundedFaceIntersections=" +
                    "sourceAttempted:" +
                        audit.SourceFaceIntersectionAttempted +
                    ",sourcePairs:" +
                        audit.SourceFaceIntersectionPairCount +
                    ",sourceCoplanar:" +
                        audit.SourceCoplanarOverlapPairCount +
                    ",sourceNonCoplanar:" +
                        audit.SourceNonCoplanarIntersectionPairCount +
                    ",sourceBoundaryContacts:" +
                        audit.SourceBoundaryContactPairCount +
                    ",sourceImproperInterior:" +
                        audit.SourceImproperInteriorPairCount +
                    ",resultAttempted:" +
                        audit.FaceIntersectionAttempted +
                    ",resultPairs:" +
                        audit.FaceIntersectionPairCount +
                    ",resultCoplanar:" +
                        audit.CoplanarOverlapPairCount +
                    ",resultNonCoplanar:" +
                        audit.NonCoplanarIntersectionPairCount +
                    ",resultBoundaryContacts:" +
                        audit.ResultBoundaryContactPairCount +
                    ",resultImproperInterior:" +
                        audit.ResultImproperInteriorPairCount +
                    ",unchanged:" +
                        audit.UnchangedIntersectionPairCount +
                    ",changed:" +
                        audit.ChangedIntersectionPairCount +
                    ",new:" + audit.NewIntersectionPairCount +
                    ",newBoundaryContacts:" +
                        audit.NewBoundaryContactPairCount +
                    ",newInterior:" +
                        audit.NewImproperInteriorIntersectionPairCount +
                    ",changedInterior:" +
                        audit.ChangedImproperInteriorIntersectionPairCount +
                    ",introducedInterior:" +
                        audit.IntroducedImproperInteriorIntersectionPairCount +
                    ",resolved:" +
                        audit.ResolvedIntersectionPairCount +
                    ",firstResultA:" + audit.FirstIntersectionFaceA +
                    ",firstResultAProvenance:" +
                        audit.FirstIntersectionFaceAProvenanceKind + ":" +
                        audit.FirstIntersectionFaceAProvenanceIndex +
                    ",firstResultB:" + audit.FirstIntersectionFaceB +
                    ",firstResultBProvenance:" +
                        audit.FirstIntersectionFaceBProvenanceKind + ":" +
                        audit.FirstIntersectionFaceBProvenanceIndex +
                    ",sourceEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.SourceIntersectionPairEvidence) + "}" +
                    ",resultEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.ResultIntersectionPairEvidence) + "}" +
                    ",unchangedEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.UnchangedIntersectionPairEvidence) + "}" +
                    ",changedEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.ChangedIntersectionPairEvidence) + "}" +
                    ",newEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.NewIntersectionPairEvidence) + "}" +
                    ",resolvedEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.ResolvedIntersectionPairEvidence) + "}" +
                ", boundedVolume=" +
                    "rawSource:" +
                        audit.SourceVolume.ToString("G9") +
                    ",preparedSource:" +
                        audit.PreparedSourceVolume.ToString("G9") +
                    ",result:" +
                        audit.ResultVolume.ToString("G9") +
                    ",rawRatio:" +
                        audit.RawVolumeRatio.ToString("G9") +
                    ",preparedRatio:" +
                        audit.VolumeRatio.ToString("G9") +
                    ",sourcePreparationRatio:" +
                        audit.SourcePreparationVolumeRatio.ToString("G9") +
                    ",rawDelta:" +
                        audit.RawVolumeDelta.ToString("G9") +
                    ",preparedDelta:" +
                        audit.PreparedVolumeDelta.ToString("G9") +
                    ",minimumRatio:" +
                        audit.VolumeMinimumRatio.ToString("G9") +
                    ",maximumRatio:" +
                        audit.VolumeMaximumRatio.ToString("G9") +
                    ",lowerMargin:" +
                        audit.VolumeLowerMargin.ToString("G9") +
                    ",upperMargin:" +
                        audit.VolumeUpperMargin.ToString("G9") +
                    ",valid:" + audit.VolumeValid +
                ", boundedLocalVolume=" +
                    "attempted:" + audit.LocalVolumeAttempted +
                    ",sourceSigned:" +
                        audit.SourceSignedVolume.ToString("G12") +
                    ",preparedSourceSigned:" +
                        audit.PreparedSourceSignedVolume.ToString("G12") +
                    ",resultSigned:" +
                        audit.ResultSignedVolume.ToString("G12") +
                    ",resultAbsolute:" +
                        audit.ResultAbsoluteVolume.ToString("G12") +
                    ",originalOwnerA:" +
                        audit.OriginalOwnerAContribution.ToString("G12") +
                    ",originalOwnerB:" +
                        audit.OriginalOwnerBContribution.ToString("G12") +
                    ",originalOwnerTotal:" +
                        audit.OriginalOwnerContribution.ToString("G12") +
                    ",replacementOwnerA:" +
                        audit.ReplacementOwnerAContribution.ToString("G12") +
                    ",replacementOwnerB:" +
                        audit.ReplacementOwnerBContribution.ToString("G12") +
                    ",replacementOwnerTotal:" +
                        audit.ReplacementOwnerContribution.ToString("G12") +
                    ",originalSupportA:" +
                        audit.OriginalSupportAContribution.ToString("G12") +
                    ",originalSupportB:" +
                        audit.OriginalSupportBContribution.ToString("G12") +
                    ",originalSupportTotal:" +
                        audit.OriginalSupportContribution.ToString("G12") +
                    ",replacementSupportA:" +
                        audit.ReplacementSupportAContribution.ToString("G12") +
                    ",replacementSupportB:" +
                        audit.ReplacementSupportBContribution.ToString("G12") +
                    ",replacementSupportTotal:" +
                        audit.ReplacementSupportContribution.ToString("G12") +
                    ",bevel:" +
                        audit.BevelContribution.ToString("G12") +
                    ",capA:" +
                        audit.CapAContribution.ToString("G12") +
                    ",capB:" +
                        audit.CapBContribution.ToString("G12") +
                    ",originalForeign:" +
                        audit.OriginalForeignContribution.ToString("G12") +
                    ",resultForeign:" +
                        audit.ResultForeignContribution.ToString("G12") +
                    ",foreignDelta:" +
                        audit.ForeignContributionDelta.ToString("G12") +
                    ",localReplacementDelta:" +
                        audit.LocalReplacementDelta.ToString("G12") +
                    ",globalSignedDelta:" +
                        audit.GlobalSignedVolumeDelta.ToString("G12") +
                    ",localGlobalResidual:" +
                        audit.LocalGlobalResidual.ToString("G12") +
                ", boundedCertification=" +
                    "attempted:" + audit.CertificationAttempted +
                    ",facesReoriented:" +
                        audit.FacesReoriented +
                    ",outwardWindingFailures:" +
                        audit.OutwardWindingFailureCount +
                ", boundedBevelPlane=" +
                    "attempted:" + audit.BevelPlaneAttempted +
                    ",planeNormal:" + FormatBoundedAuditVector(
                        audit.BevelPlaneNormal) +
                    ",faceNormal:" + FormatBoundedAuditVector(
                        audit.BevelFaceNormal) +
                    ",normalAgreement:" +
                        audit.BevelPlaneNormalAgreement.ToString("G9") +
                    ",distance:" +
                        audit.BevelPlaneDistance.ToString("G9") +
                    ",solidCentreSide:" +
                        audit.BevelSolidCentreSide.ToString("G9") +
                    ",sourceEdgeASide:" +
                        audit.BevelSourceEdgeASide.ToString("G9") +
                    ",sourceEdgeBSide:" +
                        audit.BevelSourceEdgeBSide.ToString("G9") +
                    ",railMaxResidual:" +
                        audit.BevelRailMaximumPlaneResidual.ToString("G9") +
                ", boundedVolumeCrossCheck=" +
                    "triangulationAttempted:" +
                        audit.DiagnosticTriangulationAttempted +
                    ",triangleSoupValid:" +
                        audit.DiagnosticTriangleSoupValid +
                    ",triangleSigned:" +
                        audit.DiagnosticTriangleSignedVolume.ToString("G12") +
                    ",triangleAbsolute:" +
                        audit.DiagnosticTriangleVolume.ToString("G12") +
                    ",polygonTriangleDelta:" +
                        audit.PolygonTriangleVolumeDelta.ToString("G12") +
                    ",polygonTriangleSignedDelta:" +
                        audit.PolygonTriangleSignedVolumeDelta.ToString("G12") +
                ", boundedBevelRegion=" +
                    "polygonFaces:" + audit.BevelRegionFaceCount +
                    ",boundaryVertices:" +
                        audit.BevelRegionBoundaryVertexCount +
                    ",triangles:" + audit.BevelRegionTriangleCount +
                    ",authoredNormalTriangles:" +
                        audit.BevelRegionAuthoredNormalTriangleCount +
                    ",authoredSurfaceGroupTriangles:" +
                        audit.BevelRegionAuthoredSurfaceGroupTriangleCount +
                    ",internalFanVertices:" +
                        audit.BevelRegionInternalFanVertexCount +
                    ",maxPlaneResidual:" +
                        audit.BevelRegionMaximumPlaneResidual
                            .ToString("G9") +
                    ",maxGeometricNormalDeviationDegrees:" +
                        audit.BevelRegionMaximumNormalDeviationDegrees
                            .ToString("G9") +
                    ",renderValid:" +
                        audit.BevelRegionRenderValid +
                    ",failureFace:" +
                        audit.BevelRegionFailureFace +
                    ",failureProvenance:" +
                        audit.BevelRegionFailureProvenanceIndex +
                    ",failureReason:" +
                        (string.IsNullOrEmpty(
                            audit.BevelRegionFailureReason)
                            ? "none"
                            : audit.BevelRegionFailureReason) +
                ", boundedMesh=" +
                    "triangles:" + audit.PreviewTriangleCount +
                    ",triangulatedFaces:" +
                        audit.TriangulatedFaceCount +
                    ",degenerate:" +
                        audit.PreviewDegenerateTriangleCount +
                    ",open:" + audit.PreviewOpenEdgeCount +
                    ",nonManifold:" +
                        audit.PreviewNonManifoldEdgeCount +
                    ",winding:" +
                        audit.PreviewWindingFailureCount +
                    ",bounds:" + audit.PreviewBoundsFailureCount +
                    ",volume:" + audit.PreviewVolumeFailureCount +
                    ",failureFace:" +
                        audit.TriangulationFailureFace +
                    ",failureKind:" +
                        audit.TriangulationFailureKind +
                    ",failureProvenance:" +
                        audit.TriangulationFailureProvenanceKind + ":" +
                        audit.TriangulationFailureProvenanceIndex +
                    ",failureReason:" +
                        (string.IsNullOrEmpty(
                            audit.TriangulationFailureReason)
                            ? "none"
                            : audit.TriangulationFailureReason) +
                (string.IsNullOrEmpty(audit.Diagnostic)
                    ? string.Empty
                    : ", boundedTrace=" + audit.Diagnostic) +
                ", geometryCommit=disabled";
            LogChamferNoStackTrace(message, audit.GeometryValid != 1);
#endif
        }

        private static string FormatBoundedPreparationAudit(
            BoundedPreparationAudit audit)
        {
            return "attempted:" + audit.Attempted +
                ",succeeded:" + audit.Succeeded +
                ",inputFaces:" + audit.InputFaceCount +
                ",inputVertices:" + audit.InputVertexCount +
                ",inputUniqueVertices:" +
                    audit.InputUniqueVertexCount +
                ",outputFaces:" + audit.OutputFaceCount +
                ",outputVertices:" + audit.OutputVertexCount +
                ",outputUniqueVertices:" +
                    audit.OutputUniqueVertexCount +
                ",welded:" + audit.Welded +
                ",conformed:" + audit.ConformedCount +
                ",seamPairs:" + audit.SeamRepairCount +
                ",seamTouchedFaces:" +
                    audit.SeamTouchedFaceCount +
                ",inputOpen:" + audit.InputOpenEdgeCount +
                ",inputNonManifold:" +
                    audit.InputNonManifoldEdgeCount +
                ",inputTJunction:" + audit.InputTJunctionCount +
                ",inputInvalidFaces:" +
                    audit.InputInvalidFaceCount +
                ",outputOpen:" + audit.OutputOpenEdgeCount +
                ",outputNonManifold:" +
                    audit.OutputNonManifoldEdgeCount +
                ",outputTJunction:" + audit.OutputTJunctionCount +
                ",outputInvalidFaces:" +
                    audit.OutputInvalidFaceCount +
                ",inputVolume:" +
                    audit.InputVolume.ToString("G9") +
                ",outputVolume:" +
                    audit.OutputVolume.ToString("G9") +
                ",volumeDelta:" +
                    audit.VolumeDelta.ToString("G9") +
                ",volumeRatio:" +
                    audit.VolumeRatio.ToString("G9") +
                ",failedStage:" +
                    (string.IsNullOrEmpty(audit.FailedStage)
                        ? "none"
                        : audit.FailedStage) +
                ",failedFace:" + audit.FailedFace +
                ",failedKind:" + audit.FailedKind +
                ",failedProvenance:" +
                    audit.FailedProvenanceKind + ":" +
                    audit.FailedProvenanceIndex +
                ",degenerate:" + audit.DegenerateCount +
                ",nonPlanar:" + audit.NonPlanarCount +
                ",nonSimple:" + audit.NonSimpleCount +
                ",nonConvex:" + audit.NonConvexCount +
                ",windingFailure:" +
                    audit.WindingFailureCount;
        }

        private static string FormatBoundedSourceProvenanceAudit(
            BoundedSourceProvenanceAudit audit)
        {
            return "attempted:" + audit.Attempted +
                ",valid:" + audit.Valid +
                ",expected:" + audit.ExpectedSourceFaceCount +
                ",totalFaces:" + audit.TotalFaceCount +
                ",sourceFaces:" +
                    audit.SourceProvenanceFaceCount +
                ",uniqueValid:" +
                    audit.ValidUniqueSourceFaceCount +
                ",missing:" + audit.MissingSourceFaceCount +
                ",duplicates:" +
                    audit.DuplicateSourceFaceCount +
                ",outOfRange:" +
                    audit.OutOfRangeSourceFaceCount +
                ",nonSource:" + audit.NonSourceFaceCount +
                ",nullFaces:" + audit.NullFaceCount +
                ",firstMissing:" + audit.FirstMissingSourceFace +
                ",firstDuplicate:" +
                    audit.FirstDuplicateSourceFace +
                ",firstOutOfRange:" +
                    audit.FirstOutOfRangeSourceFace;
        }

        private static string FormatBoundedAuditEvidence(
            string value)
        {
            return string.IsNullOrEmpty(value)
                ? "none"
                : value;
        }

        private static string FormatBoundedAuditVector(
            Vector3 value)
        {
            return "(" + value.x.ToString("G9") + "/" +
                value.y.ToString("G9") + "/" +
                value.z.ToString("G9") + ")";
        }

        private static void LogChamferEmissionAudit(
            ChamferEmissionStats stats,
            bool ready,
            string blocker,
            PlaneCutBevelAuditResult planeCutAudit)
        {
#if UNITY_EDITOR
            string message =
                "GeneratedMass edge wear compact audit. " +
                "selected=" + stats.ActiveSelectedEdgeCount + "/" +
                    stats.CandidateSelectedEdgeCount +
                ", replacement=" + stats.ReplacementFacesBuilt + "/" +
                    stats.ReplacementFacesAttempted +
                ", bevel=" + stats.BevelStripsBuilt + "/" +
                    stats.BevelStripsAttempted +
                ", livePatch=" + stats.PatchLoopsBuilt + "/" +
                    stats.PatchLoopsAttempted +
                ", correctedPatch=" + stats.PatchCorrectedLoopsBuilt +
                    "/" + stats.PatchCorrectedLoopsAttempted +
                ", baselineRejected=" +
                    stats.PatchCorrectedBaselineLoopsRejected +
                ", overlap=" + stats.PatchOverlapLoopsClassified + ":" +
                    stats.PatchOverlapPatchContainedInReplacement + "/" +
                    stats.PatchOverlapReplacementContainedInPatch + "/" +
                    stats.PatchOverlapPartialCoplanarArea + "/" +
                    stats.PatchOverlapNonCoplanarPenetration + "/" +
                    stats.PatchOverlapBevelStripPenetration + "/" +
                    stats.PatchOverlapUnclassified +
                ", overlapOwner=" + stats.PatchOverlapBoundaryOwner +
                    "/" + stats.PatchOverlapNonBoundaryOwner +
                ", overlapArea=" +
                    (stats.PatchOverlapProjectedAreaNanounits /
                        1000000000.0).ToString("G6") +
                ", contained=" +
                    stats.PatchContainedOwnershipCandidates + "/" +
                    stats.PatchContainedOwnershipResolved + "/" +
                    stats.PatchContainedOwnershipStillRequired + "/" +
                    stats.PatchContainedOwnershipOwnerAmbiguous + "/" +
                    stats.PatchContainedOwnershipBoundaryTransferFailures +
                    "/" +
                    stats.PatchContainedOwnershipTopologyFailures +
                ", containedRepartition=" +
                    stats.PatchContainedRepartitionCandidates + "/" +
                    stats.PatchContainedRepartitionResolved + "/" +
                    stats.PatchContainedRepartitionArrangementFailures + "/" +
                    stats.PatchContainedRepartitionTriangulationFailures +
                    "/" + stats.PatchContainedRepartitionAreaFailures +
                    "/" + stats.PatchContainedRepartitionBoundaryFailures +
                    "/" + stats.PatchContainedRepartitionTopologyFailures +
                    "/" + stats.PatchContainedRepartitionOverlapRemaining +
                ", containedRepair=" +
                    stats.PatchContainedRepairCandidates + "/" +
                    stats.PatchContainedRepairGuidedResiduals + "/" +
                    stats.PatchContainedRepairGenericFallbacks + "/" +
                    stats.PatchContainedRepairEndpointAligned + "/" +
                    stats.PatchContainedRepairResolved + "/" +
                    stats.PatchContainedRepairBuildFailures + "/" +
                    stats.PatchContainedRepairBoundaryFailures + "/" +
                    stats.PatchContainedRepairTopologyFailures + "/" +
                    stats.PatchContainedRepairOverlapRemaining +
                ", containedBoundary=" +
                    stats.PatchContainedBoundaryCandidates + "/" +
                    stats.PatchContainedBoundaryExactValid + "/" +
                    stats.PatchContainedBoundarySplitEquivalent + "/" +
                    stats.PatchContainedBoundaryResidualMissing + "/" +
                    stats.PatchContainedBoundaryExternalUnsplit + "/" +
                    stats.PatchContainedBoundaryUnderused + "/" +
                    stats.PatchContainedBoundaryOverused + "/" +
                    stats.PatchContainedBoundaryAmbiguous +
                ", containedBoundarySegments=" +
                    stats.PatchContainedBoundarySegments + "/" +
                    stats.PatchContainedBoundarySegmentExactValid + "/" +
                    stats.PatchContainedBoundarySegmentSplitEquivalent + "/" +
                    stats.PatchContainedBoundarySegmentResidualMissing + "/" +
                    stats.PatchContainedBoundarySegmentExternalUnsplit + "/" +
                    stats.PatchContainedBoundarySegmentUnderused + "/" +
                    stats.PatchContainedBoundarySegmentOverused + "/" +
                    stats.PatchContainedBoundarySegmentAmbiguous +
                ", containedShadow=" +
                    stats.PatchContainedShadowTested + "/" +
                    stats.PatchContainedShadowOverlapRemoved + "/" +
                    stats.PatchContainedShadowTopologyClean + "/" +
                    stats.PatchContainedShadowTJunctionIncrease + "/" +
                    stats.PatchContainedShadowUnexpectedOpenEdgeIncrease +
                    "/" + stats.PatchContainedShadowSourceBoundaryIncrease +
                    "/" + stats.PatchContainedShadowNonManifoldIncrease +
                ", containedCombined=" +
                    stats.PatchContainedCombinedAttempted + "/" +
                    stats.PatchContainedCombinedApplied + "/" +
                    stats.PatchContainedCombinedOwnerConflicts + "/" +
                    stats.PatchContainedCombinedTopologyFailures + "/" +
                    stats.PatchContainedCombinedRemainingOverlaps +
                ", planeBevel=" +
                    planeCutAudit.SelectedEdgeCount + "/" +
                    planeCutAudit.ActiveEdgeCount + "/" +
                    planeCutAudit.PlanesBuilt + "/" +
                    planeCutAudit.PlanesLocalized + "/" +
                    planeCutAudit.PlanesDeferred + "/" +
                    planeCutAudit.PlanesRejected + "/" +
                    planeCutAudit.CapsBuilt + "/" +
                    planeCutAudit.CapsMissing + "/" +
                    planeCutAudit.CapsRedundant + "/" +
                    planeCutAudit.ConformalSplitCount + "/" +
                    planeCutAudit.SeamPairCount + "/" +
                    planeCutAudit.OpenEdgeCount + "/" +
                    planeCutAudit.NonManifoldEdgeCount + "/" +
                    planeCutAudit.TJunctionCount + "/" +
                    planeCutAudit.InvalidFaceCount + "/" +
                    planeCutAudit.GeometryValid +
                ", planeVertexJunction=" +
                    planeCutAudit.VertexJunctionCandidateCount + "/" +
                    planeCutAudit.VertexJunctionDirectBuiltCount + "/" +
                    planeCutAudit.VertexJunctionAdaptiveBuiltCount + "/" +
                    planeCutAudit.VertexJunctionBacktrackBuiltCount + "/" +
                    planeCutAudit.VertexJunctionCleanSharpCount + "/" +
                    planeCutAudit.VertexJunctionUnresolvedCount + "/" +
                    planeCutAudit.VertexJunctionTriangleCapCount + "/" +
                    planeCutAudit.VertexJunctionQuadCapCount + "/" +
                    planeCutAudit.VertexJunctionLargerCapCount + "/" +
                    planeCutAudit.VertexJunctionEdgesDeferredCount + "/" +
                    planeCutAudit.VertexJunctionRebuildPassCount +
                ", planeSolve=" +
                    planeCutAudit.SolveStatesEvaluated + "/" +
                    planeCutAudit.SolveJunctionsVisited + "/" +
                    planeCutAudit.SolveCandidateTrials + "/" +
                    planeCutAudit.SolveSystemRebuilds + "/" +
                    planeCutAudit.SolvePolygonAudits + "/" +
                    planeCutAudit.SolveTriangleAudits + "/" +
                    planeCutAudit.SolveEdgesDeferred + "/" +
                    planeCutAudit.SolveElapsedMilliseconds + "/" +
                    planeCutAudit.SolveTimedOut +
                ", planeFaceQuality=" +
                    planeCutAudit.FaceQualityFaceCount + "/" +
                    planeCutAudit.FaceQualitySeamTouchedFaceCount + "/" +
                    planeCutAudit.FaceQualityNonPlanarCount + "/" +
                    planeCutAudit.FaceQualityElongatedJunctionCount + "/" +
                    planeCutAudit.FaceQualityMaxPlaneDeviation
                        .ToString("G6") + "/" +
                    planeCutAudit.FaceQualityMaxNormalSpreadDegrees
                        .ToString("G6") + "/" +
                    planeCutAudit.FaceQualityMinimumJunctionCompactness
                        .ToString("G6") + "/" +
                    planeCutAudit.FaceQualityMaximumJunctionAspectRatio
                        .ToString("G6") + "/" +
                    planeCutAudit.FaceQualityWorstVertexCount +
                ", planeBand=" +
                    FormatPlaneCutBandAudit(planeCutAudit) +
                ", edgeConflict=" +
                    FormatPlaneCutEdgeConflictAudit(planeCutAudit) +
                ", localJunction=" +
                    FormatPlaneCutLocalJunctionAudit(planeCutAudit) +
                ", planeMesh=" +
                    planeCutAudit.PreviewTriangleCount + "/" +
                    planeCutAudit.PreviewDegenerateTriangleCount + "/" +
                    planeCutAudit.PreviewOpenEdgeCount + "/" +
                    planeCutAudit.PreviewNonManifoldEdgeCount + "/" +
                    planeCutAudit.PreviewWindingFailureCount + "/" +
                    planeCutAudit.PreviewBoundsFailureCount + "/" +
                    planeCutAudit.PreviewVolumeFailureCount + "/" +
                    planeCutAudit.PreviewGeometryValid +
                (string.IsNullOrEmpty(planeCutAudit.Diagnostic)
                    ? string.Empty
                    : ", planeTrace=" + planeCutAudit.Diagnostic) +
                ", sector=" + stats.PatchSectorAuthoritativeLoops +
                    "/" + stats.PatchSectorExistingPlanLoops +
                ", sectorOwned=" +
                    stats.PatchSectorBoundaryHalfEdgesAssigned + "/" +
                    stats.PatchSectorBoundaryHalfEdges +
                ", sliver=" +
                    stats.PatchCorrectedReservedSliverTriangles +
                    "/" + stats.PatchCorrectedReservedSliverLoops +
                ", sliverDelta=" +
                    stats.PatchSliverDeltaPreCollapseComponents + "/" +
                    stats.PatchSliverDeltaPostCollapseComponents + "/" +
                    stats.PatchSliverDeltaReservedPreComponents + "/" +
                    stats.PatchSliverDeltaExactComponentMatches + "/" +
                    stats.PatchSliverDeltaDisappearedComponents + "/" +
                    stats.PatchSliverDeltaMergedPostComponents + "/" +
                    stats.PatchSliverDeltaSplitPreComponents + "/" +
                    stats.PatchSliverDeltaMissingLoopCount +
                (string.IsNullOrEmpty(stats.PatchSliverDeltaDiagnostic)
                    ? string.Empty
                    : ", sliverTrace=" +
                        stats.PatchSliverDeltaDiagnostic) +
                ", boundaryOccurrence=" +
                    stats.PatchCorrectedBoundaryMissingOpposite + "/" +
                    stats.PatchCorrectedBoundaryDuplicateOpposite + "/" +
                    stats.PatchCorrectedBoundaryDirectionMismatch + "/" +
                    stats.PatchCorrectedBoundaryExtraPatchEdge +
                (string.IsNullOrEmpty(
                        stats.PatchCorrectedBoundaryOccurrenceDiagnostic)
                    ? string.Empty
                    : ", boundaryTrace=" +
                        stats.PatchCorrectedBoundaryOccurrenceDiagnostic) +
                ", final=" +
                    stats.PatchCorrectedFinalUnexpectedOpenEdges + "/" +
                    stats.PatchCorrectedFinalNonManifoldEdges + "/" +
                    stats.PatchCorrectedFinalTJunctions +
                ", readyLive=" +
                    stats.ReadyForChamferPatchTopology +
                ", readyCorrected=" +
                    stats.ReadyForCorrectedChamferPatchTopology +
                ", geometryCommit=disabled";
            if (!string.IsNullOrEmpty(blocker))
            {
                message += ", blocker=" + blocker;
            }
            if (!ShouldSuppressChamferCompactSummary(
                    stats.DiagnosticGeometrySignature,
                    message))
            {
                LogChamferNoStackTrace(message, !ready);
            }
#endif
        }

        private static void LogBoundedAllEdgesAudit(
            BoundedAllEdgesAuditResult audit)
        {
#if UNITY_EDITOR
            string detailed =
                BuildBoundedAllEdgesDetailedTelemetry(audit);
            const string relativePath =
                "Library/GeneratedMassEdgeWearTelemetry.txt";
            audit.TelemetryRelativePath = relativePath;
            try
            {
                string projectRoot = Path.GetFullPath(
                    Path.Combine(Application.dataPath, ".."));
                string fullPath = Path.Combine(
                    projectRoot,
                    "Library",
                    "GeneratedMassEdgeWearTelemetry.txt");
                string directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(
                    fullPath,
                    detailed,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                audit.TelemetryWriteSucceeded = 1;
                audit.TelemetryWriteFailure = string.Empty;
            }
            catch (Exception exception)
            {
                audit.TelemetryWriteSucceeded = 0;
                audit.TelemetryWriteFailure =
                    exception.GetType().Name + ":" + exception.Message;
            }

            string message =
                "GeneratedMass unified bounded edge-wear audit. " +
                "stage=" + audit.Stage +
                ",failureStage:" + audit.FailureStage +
                ",valid:" + audit.GeometryValid +
                ",trace:" +
                    (string.IsNullOrEmpty(audit.Diagnostic)
                        ? "none"
                        : audit.Diagnostic) +
                ", allBounded=" +
                    "candidates:" + audit.CandidateCount +
                    ",convex:" + audit.ConvexCandidateCount +
                    ",railSolved:" + audit.RailSolvedEdgeCount +
                    ",railRejected:" + audit.RailRejectedEdgeCount +
                    ",hullSuppressed:" + audit.HullSuppressedEdgeCount +
                    ",active:" + audit.ActiveEdgeCount +
                ", pointCloud=" +
                    "points:" + audit.PointCount +
                    ",rank:" + audit.PointCloudRank +
                    ",min:" +
                        FormatBoundedAllEdgeVector(
                            audit.PointCloudBoundsMinimum) +
                    ",max:" +
                        FormatBoundedAllEdgeVector(
                            audit.PointCloudBoundsMaximum) +
                ", planeExtraction=" +
                    "triples:" + audit.HullTriplesTested +
                    ",degenerate:" + audit.HullDegenerateTriples +
                    ",nearDegenerate:" +
                        audit.HullNearDegenerateTriples +
                    ",normalizationRejected:" +
                        audit.HullNormalizationRejectedTriples +
                    ",postNormalizationInvalid:" +
                        audit.HullPostNormalizationInvalidTriples +
                    ",minimumCross:" +
                        audit.HullPlaneMinimumCrossMagnitude
                            .ToString("G9") +
                    ",rejectedCrossRange:" +
                        audit.HullMinimumRejectedCrossMagnitude
                            .ToString("G9") + "-" +
                        audit.HullMaximumRejectedCrossMagnitude
                            .ToString("G9") +
                    ",minimumAcceptedCross:" +
                        audit.HullMinimumAcceptedCrossMagnitude
                            .ToString("G9") +
                    ",supporting:" + audit.HullSupportingTriples +
                    ",straddling:" + audit.HullStraddlingTriples +
                    ",created:" + audit.HullPlanesCreated +
                    ",merged:" + audit.HullPlanesMerged +
                    ",beforePrune:" + audit.HullPlanesBeforePrune +
                    ",pruned:" +
                        audit.HullPlanesRemovedUnderThreePoints +
                    ",invalidRemoved:" +
                        audit.HullInvalidPlanesRemoved +
                    ",firstInvalid:" +
                        audit.HullFirstInvalidPlaneIndex +
                    ",firstInvalidSeed:" +
                        audit.HullFirstInvalidSeedA + "/" +
                        audit.HullFirstInvalidSeedB + "/" +
                        audit.HullFirstInvalidSeedC +
                    ",firstInvalidCross:" +
                        audit.HullFirstInvalidSeedCrossMagnitude
                            .ToString("G9") +
                    ",firstInvalidReason:" +
                        (string.IsNullOrEmpty(
                            audit.HullFirstInvalidPlaneReason)
                            ? "none"
                            : audit.HullFirstInvalidPlaneReason) +
                    ",final:" + audit.HullPlaneCount +
                ", facetBuild=" +
                    "attempted:" + audit.HullPlanesAttempted +
                    ",completed:" + audit.HullFacesCompleted +
                    ",failurePlane:" + audit.HullFailurePlaneIndex +
                    ",normal:" +
                        FormatBoundedAllEdgeVector(
                            audit.HullFailurePlaneNormal) +
                    ",distance:" +
                        audit.HullFailurePlaneDistance.ToString("G9") +
                    ",planePoints:" +
                        audit.HullFailurePlanePointCount +
                    ",ordered:" +
                        audit.HullFailureOrderedVertexCount +
                    ",sanitized:" +
                        audit.HullFailureSanitizedVertexCount +
                    ",area:" +
                        audit.HullFailureFacetArea.ToString("G9") +
                    ",convex:" +
                        audit.HullFailureConvexityValid +
                    ",reason:" +
                        (string.IsNullOrEmpty(audit.HullFailureReason)
                            ? "none"
                            : audit.HullFailureReason) +
                ", boundedHull=" +
                    "iterations:" + audit.HullIterationCount +
                    ",faces:" + audit.OutputFaceCount +
                    ",sourceFaces:" + audit.SourceFaceCount +
                    ",bevelFaces:" + audit.BevelFaceCount +
                    ",junctionFaces:" +
                        audit.VertexJunctionFaceCount +
                    ",missingBevelFaces:" +
                        audit.MissingBevelFaceCount +
                    ",duplicateBevelFaces:" +
                        audit.DuplicateBevelFaceCount +
                ", boundedPrepare=" +
                    FormatBoundedPreparationAudit(audit.Preparation) +
                ", boundedTopology=" +
                    "open:" + audit.OpenEdgeCount +
                    ",nonManifold:" + audit.NonManifoldEdgeCount +
                    ",tJunction:" + audit.TJunctionCount +
                    ",invalidFaces:" + audit.InvalidFaceCount +
                ", boundedVolume=" +
                    "source:" + audit.SourceVolume.ToString("G12") +
                    ",result:" + audit.ResultVolume.ToString("G12") +
                    ",ratio:" + audit.VolumeRatio.ToString("G12") +
                    ",delta:" + audit.VolumeDelta.ToString("G12") +
                    ",valid:" + audit.VolumeValid +
                ", boundedBevelRegion=" +
                    "polygonFaces:" +
                        audit.CertificationAudit.BevelRegionFaceCount +
                    ",boundaryVertices:" +
                        audit.CertificationAudit
                            .BevelRegionBoundaryVertexCount +
                    ",triangles:" +
                        audit.CertificationAudit.BevelRegionTriangleCount +
                    ",authoredNormalTriangles:" +
                        audit.CertificationAudit
                            .BevelRegionAuthoredNormalTriangleCount +
                    ",authoredSurfaceGroupTriangles:" +
                        audit.CertificationAudit
                            .BevelRegionAuthoredSurfaceGroupTriangleCount +
                    ",internalFanVertices:" +
                        audit.CertificationAudit
                            .BevelRegionInternalFanVertexCount +
                    ",maxPlaneResidual:" +
                        audit.CertificationAudit
                            .BevelRegionMaximumPlaneResidual
                            .ToString("G9") +
                    ",maxGeometricNormalDeviationDegrees:" +
                        audit.CertificationAudit
                            .BevelRegionMaximumNormalDeviationDegrees
                            .ToString("G9") +
                    ",renderValid:" +
                        audit.CertificationAudit.BevelRegionRenderValid +
                    ",failureFace:" +
                        audit.CertificationAudit.BevelRegionFailureFace +
                    ",failureProvenance:" +
                        audit.CertificationAudit
                            .BevelRegionFailureProvenanceIndex +
                    ",failureReason:" +
                        (string.IsNullOrEmpty(
                            audit.CertificationAudit
                                .BevelRegionFailureReason)
                            ? "none"
                            : audit.CertificationAudit
                                .BevelRegionFailureReason) +
                ", boundedMesh=" +
                    "triangulationAttempted:" +
                        audit.TriangulationAttempted +
                    ",triangulatedFaces:" +
                        audit.TriangulatedFaceCount +
                    ",triangles:" + audit.TriangleCount +
                    ",triangleSoupValid:" + audit.TriangleSoupValid +
                ", diagnostics=" +
                    "corner:" + audit.CornerDiagnosticValid +
                    ",plane:" + audit.PlaneDiagnosticValid +
                    ",planeActive:" +
                        audit.PlaneDiagnosticActiveEdges +
                    ",planeBuilt:" +
                        audit.PlaneDiagnosticBuiltEdges +
                    ",planeDeferred:" +
                        audit.PlaneDiagnosticDeferredEdges +
                    ",planeRejected:" +
                        audit.PlaneDiagnosticRejectedEdges +
                ", telemetry=" +
                    "path:" + audit.TelemetryRelativePath +
                    ",write:" + audit.TelemetryWriteSucceeded +
                    (string.IsNullOrEmpty(audit.TelemetryWriteFailure)
                        ? string.Empty
                        : ",error:" + audit.TelemetryWriteFailure) +
                ", geometryCommit=disabled";
            LogChamferNoStackTrace(
                message,
                audit.GeometryValid != 1);
#endif
        }

#if UNITY_EDITOR
        private static string BuildBoundedAllEdgesDetailedTelemetry(
            BoundedAllEdgesAuditResult audit)
        {
            string detailed =
                "GeneratedMass unified bounded edge-wear detailed telemetry." +
                Environment.NewLine +
                "timestampUtc:" + DateTime.UtcNow.ToString("O") +
                Environment.NewLine +
                "stage:" + audit.Stage +
                ",failureStage:" + audit.FailureStage +
                ",valid:" + audit.GeometryValid +
                ",trace:" +
                    (string.IsNullOrEmpty(audit.Diagnostic)
                        ? "none"
                        : audit.Diagnostic) +
                Environment.NewLine +
                "allBounded=" +
                    "candidates:" + audit.CandidateCount +
                    ",convex:" + audit.ConvexCandidateCount +
                    ",railSolved:" + audit.RailSolvedEdgeCount +
                    ",railRejected:" + audit.RailRejectedEdgeCount +
                    ",hullSuppressed:" + audit.HullSuppressedEdgeCount +
                    ",active:" + audit.ActiveEdgeCount +
                    ",valid:" + audit.GeometryValid +
                Environment.NewLine + "pointCloud=" +
                    "points:" + audit.PointCount +
                    ",rank:" + audit.PointCloudRank +
                    ",min:" +
                        FormatBoundedAllEdgeVector(
                            audit.PointCloudBoundsMinimum) +
                    ",max:" +
                        FormatBoundedAllEdgeVector(
                            audit.PointCloudBoundsMaximum) +
                Environment.NewLine + "planeExtraction=" +
                    "triples:" + audit.HullTriplesTested +
                    ",degenerate:" + audit.HullDegenerateTriples +
                    ",nearDegenerate:" +
                        audit.HullNearDegenerateTriples +
                    ",normalizationRejected:" +
                        audit.HullNormalizationRejectedTriples +
                    ",postNormalizationInvalid:" +
                        audit.HullPostNormalizationInvalidTriples +
                    ",minimumCross:" +
                        audit.HullPlaneMinimumCrossMagnitude
                            .ToString("G9") +
                    ",rejectedCrossRange:" +
                        audit.HullMinimumRejectedCrossMagnitude
                            .ToString("G9") + "-" +
                        audit.HullMaximumRejectedCrossMagnitude
                            .ToString("G9") +
                    ",minimumAcceptedCross:" +
                        audit.HullMinimumAcceptedCrossMagnitude
                            .ToString("G9") +
                    ",supporting:" + audit.HullSupportingTriples +
                    ",straddling:" + audit.HullStraddlingTriples +
                    ",created:" + audit.HullPlanesCreated +
                    ",merged:" + audit.HullPlanesMerged +
                    ",beforePrune:" + audit.HullPlanesBeforePrune +
                    ",pruned:" +
                        audit.HullPlanesRemovedUnderThreePoints +
                    ",invalidRemoved:" +
                        audit.HullInvalidPlanesRemoved +
                    ",firstInvalid:" +
                        audit.HullFirstInvalidPlaneIndex +
                    ",firstInvalidSeed:" +
                        audit.HullFirstInvalidSeedA + "/" +
                        audit.HullFirstInvalidSeedB + "/" +
                        audit.HullFirstInvalidSeedC +
                    ",firstInvalidCross:" +
                        audit.HullFirstInvalidSeedCrossMagnitude
                            .ToString("G9") +
                    ",firstInvalidReason:" +
                        (string.IsNullOrEmpty(
                            audit.HullFirstInvalidPlaneReason)
                            ? "none"
                            : audit.HullFirstInvalidPlaneReason) +
                    ",final:" + audit.HullPlaneCount +
                Environment.NewLine + "facetBuild=" +
                    "attempted:" + audit.HullPlanesAttempted +
                    ",completed:" + audit.HullFacesCompleted +
                    ",failurePlane:" + audit.HullFailurePlaneIndex +
                    ",normal:" +
                        FormatBoundedAllEdgeVector(
                            audit.HullFailurePlaneNormal) +
                    ",distance:" +
                        audit.HullFailurePlaneDistance.ToString("G9") +
                    ",planePoints:" +
                        audit.HullFailurePlanePointCount +
                    ",ordered:" +
                        audit.HullFailureOrderedVertexCount +
                    ",sanitized:" +
                        audit.HullFailureSanitizedVertexCount +
                    ",area:" +
                        audit.HullFailureFacetArea.ToString("G9") +
                    ",convex:" +
                        audit.HullFailureConvexityValid +
                    ",reason:" +
                        (string.IsNullOrEmpty(audit.HullFailureReason)
                            ? "none"
                            : audit.HullFailureReason) +
                Environment.NewLine + "cornerDiagnostic=" +
                    "attempted:" + audit.CornerDiagnosticAttempted +
                    ",valid:" + audit.CornerDiagnosticValid +
                    (string.IsNullOrEmpty(audit.CornerDiagnostic)
                        ? string.Empty
                        : ",trace:" + audit.CornerDiagnostic) +
                Environment.NewLine + "planeDiagnostic=" +
                    "attempted:" + audit.PlaneDiagnosticAttempted +
                    ",valid:" + audit.PlaneDiagnosticValid +
                    ",active:" + audit.PlaneDiagnosticActiveEdges +
                    ",built:" + audit.PlaneDiagnosticBuiltEdges +
                    ",deferred:" + audit.PlaneDiagnosticDeferredEdges +
                    ",rejected:" + audit.PlaneDiagnosticRejectedEdges +
                    ",detail:{" + audit.PlaneDiagnosticEvidence + "}" +
                Environment.NewLine + "boundedHull=" +
                    "iterations:" + audit.HullIterationCount +
                    ",points:" + audit.PointCount +
                    ",planes:" + audit.HullPlaneCount +
                    ",faces:" + audit.OutputFaceCount +
                    ",sourceFaces:" + audit.SourceFaceCount +
                    ",bevelFaces:" + audit.BevelFaceCount +
                    ",vertexJunctionFaces:" +
                        audit.VertexJunctionFaceCount +
                    ",missingBevelFaces:" +
                        audit.MissingBevelFaceCount +
                    ",duplicateBevelFaces:" +
                        audit.DuplicateBevelFaceCount +
                Environment.NewLine + "boundedPrepare=" +
                    FormatBoundedPreparationAudit(audit.Preparation) +
                Environment.NewLine + "boundedTopology=" +
                    "open:" + audit.OpenEdgeCount +
                    ",nonManifold:" + audit.NonManifoldEdgeCount +
                    ",tJunction:" + audit.TJunctionCount +
                    ",invalidFaces:" + audit.InvalidFaceCount +
                Environment.NewLine + "boundedBounds=" +
                    "valid:" + audit.BoundsValid +
                    ",tolerance:" + audit.BoundsTolerance.ToString("G9") +
                    ",sourceMin:" +
                        FormatBoundedAllEdgeVector(
                            audit.SourceBoundsMinimum) +
                    ",sourceMax:" +
                        FormatBoundedAllEdgeVector(
                            audit.SourceBoundsMaximum) +
                    ",resultMin:" +
                        FormatBoundedAllEdgeVector(
                            audit.ResultBoundsMinimum) +
                    ",resultMax:" +
                        FormatBoundedAllEdgeVector(
                            audit.ResultBoundsMaximum) +
                    ",minMargin:" +
                        FormatBoundedAllEdgeVector(
                            audit.BoundsMinimumMargin) +
                    ",maxMargin:" +
                        FormatBoundedAllEdgeVector(
                            audit.BoundsMaximumMargin) +
                Environment.NewLine + "boundedContainment=" +
                    "sourceAttempted:" +
                        audit.CertificationAudit.SourceConvexityAttempted +
                    ",sourceViolations:" +
                        audit.CertificationAudit
                            .SourceConvexityViolationCount +
                    ",sourceMaximumViolation:" +
                        audit.CertificationAudit
                            .SourceMaximumPlaneViolation.ToString("G9") +
                    ",sourcePlaneFace:" +
                        audit.CertificationAudit.SourceViolatingPlaneFace +
                    ",sourceVertexFace:" +
                        audit.CertificationAudit.SourceViolatingVertexFace +
                    ",sourceVertexIndex:" +
                        audit.CertificationAudit.SourceViolatingVertexIndex +
                    ",resultAttempted:" +
                        audit.CertificationAudit
                            .ResultContainmentAttempted +
                    ",resultViolations:" +
                        audit.SourceContainmentViolations +
                    ",resultMaximumOutwardDistance:" +
                        audit.MaximumSourceContainmentViolation
                            .ToString("G9") +
                    ",resultFace:" +
                        audit.CertificationAudit.ResultViolatingFace +
                    ",resultProvenance:" +
                        audit.CertificationAudit
                            .ResultViolatingProvenanceKind + ":" +
                        audit.CertificationAudit
                            .ResultViolatingProvenanceIndex +
                    ",resultVertex:" +
                        audit.CertificationAudit
                            .ResultViolatingVertexIndex +
                    ",sourcePlane:" +
                        audit.CertificationAudit
                            .ResultViolatedSourcePlane +
                Environment.NewLine + "boundedConvexity=" +
                    "attempted:" +
                        audit.CertificationAudit
                            .ResultConvexityAttempted +
                    ",violations:" + audit.ResultConvexityViolations +
                    ",maximumViolation:" +
                        audit.MaximumResultConvexityViolation
                            .ToString("G9") +
                    ",planeFace:" +
                        audit.CertificationAudit
                            .ResultConvexityPlaneFace +
                    ",planeProvenance:" +
                        audit.CertificationAudit
                            .ResultConvexityPlaneProvenanceKind + ":" +
                        audit.CertificationAudit
                            .ResultConvexityPlaneProvenanceIndex +
                    ",vertexFace:" +
                        audit.CertificationAudit
                            .ResultConvexityVertexFace +
                    ",vertexProvenance:" +
                        audit.CertificationAudit
                            .ResultConvexityVertexProvenanceKind + ":" +
                        audit.CertificationAudit
                            .ResultConvexityVertexProvenanceIndex +
                    ",vertexIndex:" +
                        audit.CertificationAudit
                            .ResultConvexityVertexIndex +
                Environment.NewLine + "boundedIntersections=" +
                    "sourceAttempted:" +
                        audit.CertificationAudit
                            .SourceFaceIntersectionAttempted +
                    ",sourcePairs:" +
                        audit.CertificationAudit
                            .SourceFaceIntersectionPairCount +
                    ",sourceBoundary:" +
                        audit.CertificationAudit
                            .SourceBoundaryContactPairCount +
                    ",sourceInterior:" +
                        audit.CertificationAudit
                            .SourceImproperInteriorPairCount +
                    ",resultAttempted:" +
                        audit.CertificationAudit.FaceIntersectionAttempted +
                    ",resultPairs:" +
                        audit.CertificationAudit.FaceIntersectionPairCount +
                    ",resultBoundary:" +
                        audit.CertificationAudit
                            .ResultBoundaryContactPairCount +
                    ",resultInterior:" +
                        audit.CertificationAudit
                            .ResultImproperInteriorPairCount +
                    ",unchanged:" +
                        audit.CertificationAudit
                            .UnchangedIntersectionPairCount +
                    ",changed:" +
                        audit.CertificationAudit
                            .ChangedIntersectionPairCount +
                    ",new:" +
                        audit.CertificationAudit.NewIntersectionPairCount +
                    ",introducedInterior:" +
                        audit.IntroducedInteriorIntersections +
                    ",resolved:" +
                        audit.CertificationAudit
                            .ResolvedIntersectionPairCount +
                    ",sourceEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.CertificationAudit
                                .SourceIntersectionPairEvidence) + "}" +
                    ",resultEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.CertificationAudit
                                .ResultIntersectionPairEvidence) + "}" +
                    ",unchangedEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.CertificationAudit
                                .UnchangedIntersectionPairEvidence) + "}" +
                    ",changedEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.CertificationAudit
                                .ChangedIntersectionPairEvidence) + "}" +
                    ",newEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.CertificationAudit
                                .NewIntersectionPairEvidence) + "}" +
                    ",resolvedEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.CertificationAudit
                                .ResolvedIntersectionPairEvidence) + "}" +
                Environment.NewLine + "boundedVolume=" +
                    "source:" + audit.SourceVolume.ToString("G12") +
                    ",result:" + audit.ResultVolume.ToString("G12") +
                    ",ratio:" + audit.VolumeRatio.ToString("G12") +
                    ",delta:" + audit.VolumeDelta.ToString("G12") +
                    ",lowerMargin:" +
                        audit.VolumeLowerMargin.ToString("G12") +
                    ",upperMargin:" +
                        audit.VolumeUpperMargin.ToString("G12") +
                    ",valid:" + audit.VolumeValid +
                Environment.NewLine + "boundedBevelRegion=" +
                    "polygonFaces:" +
                        audit.CertificationAudit.BevelRegionFaceCount +
                    ",boundaryVertices:" +
                        audit.CertificationAudit
                            .BevelRegionBoundaryVertexCount +
                    ",triangles:" +
                        audit.CertificationAudit.BevelRegionTriangleCount +
                    ",authoredNormalTriangles:" +
                        audit.CertificationAudit
                            .BevelRegionAuthoredNormalTriangleCount +
                    ",authoredSurfaceGroupTriangles:" +
                        audit.CertificationAudit
                            .BevelRegionAuthoredSurfaceGroupTriangleCount +
                    ",internalFanVertices:" +
                        audit.CertificationAudit
                            .BevelRegionInternalFanVertexCount +
                    ",maxPlaneResidual:" +
                        audit.CertificationAudit
                            .BevelRegionMaximumPlaneResidual
                            .ToString("G9") +
                    ",maxGeometricNormalDeviationDegrees:" +
                        audit.CertificationAudit
                            .BevelRegionMaximumNormalDeviationDegrees
                            .ToString("G9") +
                    ",renderValid:" +
                        audit.CertificationAudit.BevelRegionRenderValid +
                    ",failureFace:" +
                        audit.CertificationAudit.BevelRegionFailureFace +
                    ",failureProvenance:" +
                        audit.CertificationAudit
                            .BevelRegionFailureProvenanceIndex +
                    ",failureReason:" +
                        (string.IsNullOrEmpty(
                            audit.CertificationAudit
                                .BevelRegionFailureReason)
                            ? "none"
                            : audit.CertificationAudit
                                .BevelRegionFailureReason) +
                Environment.NewLine + "boundedMesh=" +
                    "triangulationAttempted:" +
                        audit.TriangulationAttempted +
                    ",triangulatedFaces:" +
                        audit.TriangulatedFaceCount +
                    ",triangulationFailureFace:" +
                        audit.CertificationAudit
                            .TriangulationFailureFace +
                    ",triangulationFailureKind:" +
                        audit.CertificationAudit
                            .TriangulationFailureKind +
                    ",triangulationFailureProvenance:" +
                        audit.CertificationAudit
                            .TriangulationFailureProvenanceKind + ":" +
                        audit.CertificationAudit
                            .TriangulationFailureProvenanceIndex +
                    ",triangulationFailureReason:" +
                        (string.IsNullOrEmpty(
                            audit.CertificationAudit
                                .TriangulationFailureReason)
                            ? "none"
                            : audit.CertificationAudit
                                .TriangulationFailureReason) +
                    ",triangles:" + audit.TriangleCount +
                    ",degenerate:" +
                        audit.TriangleAudit
                            .PreviewDegenerateTriangleCount +
                    ",open:" +
                        audit.TriangleAudit.PreviewOpenEdgeCount +
                    ",nonManifold:" +
                        audit.TriangleAudit
                            .PreviewNonManifoldEdgeCount +
                    ",winding:" +
                        audit.TriangleAudit
                            .PreviewWindingFailureCount +
                    ",bounds:" +
                        audit.TriangleAudit
                            .PreviewBoundsFailureCount +
                    ",volume:" +
                        audit.TriangleAudit
                            .PreviewVolumeFailureCount +
                    ",triangleSoupValid:" + audit.TriangleSoupValid +
                Environment.NewLine + "hullPoints=" + audit.HullPointEvidence +
                Environment.NewLine + "hullPlanes=" + audit.HullPlaneEvidence +
                Environment.NewLine + "hullFaces=" + audit.HullFaceEvidence +
                Environment.NewLine + "edgeResults=" + audit.EdgeEvidence +
                (string.IsNullOrEmpty(audit.Diagnostic)
                    ? string.Empty
                    : Environment.NewLine + "boundedTrace=" + audit.Diagnostic) +
                Environment.NewLine + "geometryCommit=disabled";

            return detailed;
        }
#endif

        private static void LogChamferCornerAudit(
            ChamferCornerStats stats,
            bool ready,
            string blocker)
        {
#if UNITY_EDITOR
            if (ready && !EnableVerboseChamferDiagnostics)
            {
                return;
            }
            string message =
                "GeneratedMass edge wear corner audit. " +
                "selected=" + stats.ActiveSelectedEdgeCount + "/" +
                    stats.SelectedEdgeCount +
                ", replacementFailures=" +
                    stats.ReplacementFaceAreaFailureCount + "/" +
                    stats.ReplacementFaceWindingFailureCount + "/" +
                    stats.ReplacementEdgeCollapseFailureCount +
                ", solveFailures=" + stats.WidthSolveFailures + "/" +
                    stats.CornerSolveFailures +
                ", ready=" + (ready ? 1 : 0);
            if (!string.IsNullOrEmpty(blocker))
            {
                message += ", blocker=" + blocker;
            }
            LogChamferNoStackTrace(message, !ready);
#endif
        }

        private static void LogChamferReadiness(
            ChamferReadinessStats stats,
            bool ready,
            string blocker)
        {
#if UNITY_EDITOR
            if (ready && !EnableVerboseChamferDiagnostics)
            {
                return;
            }
            string message =
                "GeneratedMass edge wear readiness audit. " +
                "selected=" + stats.SelectedGraphEdgeCount +
                ", affectedVertices=" + stats.AffectedVertexCount +
                ", sourceNonManifold=" +
                    stats.SourceNonManifoldEdgeCount +
                ", sourceTJunctions=" + stats.SourceTJunctionCount +
                ", ready=" + (ready ? 1 : 0);
            if (!string.IsNullOrEmpty(blocker))
            {
                message += ", blocker=" + blocker;
            }
            LogChamferNoStackTrace(message, !ready);
#endif
        }

        #endregion
    }
}
