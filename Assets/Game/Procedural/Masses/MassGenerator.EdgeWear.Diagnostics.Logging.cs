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
                    planeCutAudit.PlanesRejected + "/" +
                    planeCutAudit.CapsBuilt + "/" +
                    planeCutAudit.CapsMissing + "/" +
                    planeCutAudit.CapsRedundant + "/" +
                    planeCutAudit.ConformalSplitCount + "/" +
                    planeCutAudit.OpenEdgeCount + "/" +
                    planeCutAudit.NonManifoldEdgeCount + "/" +
                    planeCutAudit.TJunctionCount + "/" +
                    planeCutAudit.InvalidFaceCount + "/" +
                    planeCutAudit.GeometryValid +
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
