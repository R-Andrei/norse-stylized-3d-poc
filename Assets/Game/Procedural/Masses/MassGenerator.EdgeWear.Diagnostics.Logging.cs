using System;
using System.Collections.Generic;
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
            return "passes:" + audit.EdgeConflictPassCount +
                ",deferred:" + audit.EdgeConflictEdgesDeferredCount +
                ",resolved:" + audit.EdgeConflictResolvedCount +
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

        private static void LogPlaneCutBevelAudit(
            PlaneCutBevelAuditResult planeCutAudit)
        {
#if UNITY_EDITOR
            string message =
                "GeneratedMass plane-cut bevel compact audit. " +
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
                ", geometryCommit=disabled";
            LogChamferNoStackTrace(
                message,
                planeCutAudit.GeometryValid != 1);
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
                    ",foreignSourceFacesModified:" +
                        audit.ForeignSourceFaceModifiedCount +
                    ",foreignBoundarySubdivided:" +
                        audit.ForeignBoundarySubdividedCount +
                    ",railDeviation:" +
                        audit.RailDeviation.ToString("G6") +
                    ",maxExtentBeyondRails:" +
                        audit.MaximumExtentBeyondRails.ToString("G6") +
                    ",valid:" + audit.GeometryValid +
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
                ", boundedPrepare=" +
                    FormatBoundedPreparationAudit(
                        audit.ResultPreparation) +
                    ",failedCanonicalSubdivision:" +
                        audit.PrepareFailedCanonicalSubdivision +
                ", boundedSourcePrepare=" +
                    FormatBoundedPreparationAudit(
                        audit.SourcePreparation) +
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
                ", boundedCertification=" +
                    "attempted:" + audit.CertificationAttempted +
                    ",facesReoriented:" +
                        audit.FacesReoriented +
                    ",outwardWindingFailures:" +
                        audit.OutwardWindingFailureCount +
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
