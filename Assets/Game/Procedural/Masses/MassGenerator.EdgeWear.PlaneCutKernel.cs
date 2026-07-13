using System;
using System.Collections.Generic;
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

        private struct PlaneCutBevelAuditResult
        {
            public int SelectedEdgeCount;
            public int ActiveEdgeCount;
            public int PlanesBuilt;
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
            public int GeometryValid;
            public string Diagnostic;
        }

        private static PlaneCutBevelAuditResult AuditPlaneCutBevelKernel(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            ChamferCornerSolution solution,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            out TriangleSoup previewSoup)
        {
            previewSoup = null;
            PlaneCutBevelAuditResult result =
                new PlaneCutBevelAuditResult
                {
                    SelectedEdgeCount = context.SelectedSourceEdges.Count,
                    EdgeConflictVictimEdgeIndex = -1,
                    EdgeConflictForeignEdgeIndex = -1,
                    EdgeConflictVertexIndex = -1,
                    EdgeConflictDeferredEdgeIndex = -1
                };

            List<EdgeWearSelectedGraphEdge> orderedSelected =
                new List<EdgeWearSelectedGraphEdge>(context.SelectedEdges);
            orderedSelected.Sort((left, right) =>
                left.GraphEdgeIndex.CompareTo(right.GraphEdgeIndex));

            List<PlaneCutBevelCandidate> planeCandidates =
                new List<PlaneCutBevelCandidate>(orderedSelected.Count);
            int localityDeferredCount = 0;
            for (int selectedIndex = 0;
                 selectedIndex < orderedSelected.Count;
                 selectedIndex++)
            {
                EdgeWearSelectedGraphEdge selected =
                    orderedSelected[selectedIndex];
                if (!solution.WidthByEdge.TryGetValue(
                        selected.GraphEdgeIndex,
                        out float width) ||
                    width <= PointMergeDistance)
                {
                    continue;
                }

                result.ActiveEdgeCount++;

                if (!TryBuildPlaneCutBevelCandidate(
                        context,
                        solution,
                        selected,
                        minimumStableEdgeLength,
                        out PlaneCutBevelCandidate candidate,
                        out bool localityDeferred,
                        out string blocker))
                {
                    if (localityDeferred)
                    {
                        localityDeferredCount++;
                    }
                    else
                    {
                        result.PlanesRejected++;
                        SetPlaneCutBevelDiagnostic(
                            ref result.Diagnostic,
                            blocker);
                    }
                    continue;
                }

                planeCandidates.Add(candidate);
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
                    ref result,
                    out List<PlaneCutBevelCandidate> retainedCandidates,
                    out List<PolygonFace> edgeOnlyFaces,
                    out string edgeOnlyBlocker))
            {
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    string.IsNullOrEmpty(edgeOnlyBlocker)
                        ? "the deterministic clean-band edge-only shell could not be built"
                        : edgeOnlyBlocker);
                return result;
            }

            planeCandidates = retainedCandidates;

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
                out string localJunctionBlocker);
            if (!string.IsNullOrEmpty(localJunctionBlocker))
            {
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    localJunctionBlocker);
            }
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

            TriangleSoup auditedSoup = polygonGeometryValid
                ? TriangulatePlaneCutPreviewFaces(edgeOnlyFaces)
                : null;
            if (auditedSoup != null)
            {
                AuditPlaneCutPreviewTriangleSoup(
                    auditedSoup,
                    edgeOnlyFaces,
                    minimumStableEdgeLength,
                    ref result);
            }

            bool geometryValid = polygonGeometryValid &&
                result.PreviewGeometryValid == 1;
            result.GeometryValid = geometryValid ? 1 : 0;
            if (geometryValid)
            {
                previewSoup = auditedSoup;
            }
            return result;
        }

        private static bool TryBuildCleanPlaneCutEdgeOnlyShell(
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
                result.EdgeConflictPassCount = passIndex + 1;
                if (!TryBuildPlaneCutSystemFaces(
                        sourceFaces,
                        retainedCandidates,
                        noJunctions,
                        out List<PolygonFace> rawFaces,
                        out int edgeCapsBuilt,
                        out string buildBlocker))
                {
                    blocker = string.IsNullOrEmpty(buildBlocker)
                        ? "the deterministic edge-only shell could not be built"
                        : buildBlocker;
                    return false;
                }

                if (!TryPreparePlaneCutPreviewFaces(
                        rawFaces,
                        minimumStableEdgeLength,
                        minimumStableFaceArea,
                        out List<PolygonFace> auditedFaces,
                        out int conformalSplitCount,
                        out int seamPairCount,
                        out int seamTouchedFaceCount,
                        out string preparationBlocker))
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
                EdgeConflictDeferredEdgeIndex = -1
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
            float minimumStableEdgeLength,
            out PlaneCutBevelCandidate candidate,
            out bool localityDeferred,
            out string blocker)
        {
            candidate = default;
            localityDeferred = false;
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
            float localizedDistance = solvedDistance;
            float localGuardMargin = Mathf.Max(
                PointMergeDistance * 2f,
                minimumStableEdgeLength * 0.001f);
            for (int vertexIndex = 0;
                 vertexIndex < context.Graph.Vertices.Count;
                 vertexIndex++)
            {
                if (vertexIndex == edge.VertexA ||
                    vertexIndex == edge.VertexB)
                {
                    continue;
                }

                Vector3 unrelated =
                    context.Graph.Vertices[vertexIndex].Position;
                localizedDistance = Mathf.Max(
                    localizedDistance,
                    Vector3.Dot(normal, unrelated) + localGuardMargin);
            }

            bool wasLocalized = localizedDistance >
                solvedDistance + PointMergeDistance * 0.25f;
            CutPlane plane = new CutPlane(normal, localizedDistance);
            Vector3 sourceA =
                context.Graph.Vertices[edge.VertexA].Position;
            Vector3 sourceB =
                context.Graph.Vertices[edge.VertexB].Position;
            float minimumRemoval = Mathf.Max(
                PointMergeDistance * 2f,
                minimumStableEdgeLength * 0.02f);
            float sourceRemovalA = plane.SignedDistance(sourceA);
            float sourceRemovalB = plane.SignedDistance(sourceB);
            if (sourceRemovalA <= minimumRemoval ||
                sourceRemovalB <= minimumRemoval)
            {
                localityDeferred = wasLocalized;
                blocker = wasLocalized
                    ? "a localized bevel plane cannot retain unrelated vertices and still remove its source edge"
                    : "the solved bevel plane does not remove its source edge";
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

        private static bool TryPreparePlaneCutPreviewFaces(
            List<PolygonFace> sourceFaces,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            out List<PolygonFace> auditedFaces,
            out int conformalSplitCount,
            out int seamPairCount,
            out int seamTouchedFaceCount,
            out string blocker)
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

            WeldSharedVertices(auditedFaces);
            conformalSplitCount = ConformPlaneCutFaceBoundaries(
                auditedFaces,
                minimumStableEdgeLength);
            seamPairCount = RepairPlaneCutNumericalSeams(
                auditedFaces,
                minimumStableEdgeLength,
                out seamTouchedFaceCount);
            return auditedFaces.Count >= 4;
        }

        private static TriangleSoup TriangulatePlaneCutPreviewFaces(
            List<PolygonFace> faces)
        {
            TriangleSoup soup = new TriangleSoup();
            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                Vector3 centre = CalculateAverage(face.Vertices);
                for (int vertexIndex = 0;
                     vertexIndex < face.Vertices.Count;
                     vertexIndex++)
                {
                    AddOrientedTriangle(
                        soup,
                        centre,
                        face.Vertices[vertexIndex],
                        face.Vertices[
                            (vertexIndex + 1) % face.Vertices.Count],
                        face.Normal,
                        face.Feature,
                        face.FeatureStrength);
                }
            }
            return soup;
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

        private static void AuditPlaneCutFaceQuality(
            List<PolygonFace> faces,
            List<PlaneCutVertexJunctionCandidate> junctions,
            float minimumStableEdgeLength,
            ref PlaneCutBevelAuditResult result)
        {
            const float maximumTriangleNormalSpreadDegrees = 0.75f;
            const float minimumJunctionCompactness = 0.06f;
            const float maximumJunctionAspectRatio = 12f;
            float planarityTolerance = Mathf.Max(
                PointMergeDistance * 2f,
                minimumStableEdgeLength * 0.00005f);

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
                MeasurePlaneCutFacePlanarity(
                    face,
                    out float maximumDeviation,
                    out float maximumNormalSpread);
                result.FaceQualityMaxPlaneDeviation = Mathf.Max(
                    result.FaceQualityMaxPlaneDeviation,
                    maximumDeviation);
                result.FaceQualityMaxNormalSpreadDegrees = Mathf.Max(
                    result.FaceQualityMaxNormalSpreadDegrees,
                    maximumNormalSpread);
                if (maximumDeviation > planarityTolerance ||
                    maximumNormalSpread >
                        maximumTriangleNormalSpreadDegrees)
                {
                    result.FaceQualityNonPlanarCount++;
                }
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
            maximumDeviation = 0f;
            maximumNormalSpread = 0f;
            if (face.Vertices.Count < 3)
            {
                return;
            }

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
                maximumDeviation = Mathf.Max(
                    maximumDeviation,
                    Mathf.Abs(
                        Vector3.Dot(face.Normal, start) -
                        planeDistance));

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
                maximumNormalSpread = Mathf.Max(
                    maximumNormalSpread,
                    Vector3.Angle(
                        triangleNormal,
                        face.Normal));
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
