using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear global plane-cut junction solver

        private readonly struct PlaneCutVertexJunctionCandidate
        {
            public readonly int VertexIndex;
            public readonly CutPlane Plane;
            public readonly float Strength;
            public readonly float PlaneTolerance;
            public readonly float ClipEpsilon;
            public readonly float LocalRadius;
            public readonly float CutDepth;
            public readonly float Compactness;
            public readonly float AspectRatio;
            public readonly int CapVertexCount;
            public readonly int NormalRank;
            public readonly bool IsDirect;

            public PlaneCutVertexJunctionCandidate(
                int vertexIndex,
                CutPlane plane,
                float strength,
                float planeTolerance,
                float clipEpsilon,
                float localRadius,
                float cutDepth,
                float compactness,
                float aspectRatio,
                int capVertexCount,
                int normalRank,
                bool isDirect)
            {
                VertexIndex = vertexIndex;
                Plane = plane;
                Strength = strength;
                PlaneTolerance = planeTolerance;
                ClipEpsilon = clipEpsilon;
                LocalRadius = localRadius;
                CutDepth = cutDepth;
                Compactness = compactness;
                AspectRatio = aspectRatio;
                CapVertexCount = capVertexCount;
                NormalRank = normalRank;
                IsDirect = isDirect;
            }
        }

        private readonly struct PlaneCutJunctionNormalOption
        {
            public readonly Vector3 Normal;
            public readonly int Rank;
            public readonly bool IsDirect;

            public PlaneCutJunctionNormalOption(
                Vector3 normal,
                int rank,
                bool isDirect)
            {
                Normal = normal;
                Rank = rank;
                IsDirect = isDirect;
            }
        }

        private struct PlaneCutSolveMetrics
        {
            public int StatesEvaluated;
            public int JunctionsVisited;
            public int CandidateTrials;
            public int SystemRebuilds;
            public int PolygonAudits;
            public int TriangleAudits;
            public int EdgesDeferred;
            public long ElapsedMilliseconds;
            public int TimedOut;
        }

        private sealed class PlaneCutJunctionSearchNode
        {
            public readonly List<int> DeferredSourceEdges;

            public int Depth => DeferredSourceEdges.Count;

            public PlaneCutJunctionSearchNode(List<int> deferredSourceEdges)
            {
                DeferredSourceEdges = deferredSourceEdges;
            }
        }

        private sealed class PlaneCutJunctionSolveOutcome
        {
            public List<PolygonFace> Faces;
            public List<PlaneCutBevelCandidate> ActiveEdges;
            public readonly List<PlaneCutVertexJunctionCandidate> Junctions =
                new List<PlaneCutVertexJunctionCandidate>();
            public readonly List<int> UnresolvedVertices = new List<int>();
            public readonly HashSet<int> DeferredSourceEdges =
                new HashSet<int>();
            public int EdgeCapsBuilt;
            public float QualityScore;
            public bool HardFailure;
            public string Diagnostic;
        }

        private static PlaneCutJunctionSolveOutcome
            SolvePlaneCutGlobalJunctionSystem(
                List<PolygonFace> sourceFaces,
                ChamferTopologyContext context,
                List<PlaneCutBevelCandidate> allEdges,
                float minimumStableEdgeLength,
                float minimumStableFaceArea,
                out PlaneCutSolveMetrics metrics)
        {
            const int maximumSearchStates = 48;
            const long timeBudgetMilliseconds = 3000;
            metrics = new PlaneCutSolveMetrics();
            System.Diagnostics.Stopwatch stopwatch =
                System.Diagnostics.Stopwatch.StartNew();

            Queue<PlaneCutJunctionSearchNode> queue =
                new Queue<PlaneCutJunctionSearchNode>();
            HashSet<string> visited = new HashSet<string>();
            PlaneCutJunctionSearchNode initial =
                new PlaneCutJunctionSearchNode(new List<int>());
            queue.Enqueue(initial);
            visited.Add(string.Empty);

            PlaneCutJunctionSolveOutcome bestClean = null;
            int bestCleanDepth = int.MaxValue;
            PlaneCutJunctionSolveOutcome bestPartial = null;

            while (queue.Count > 0 &&
                   metrics.StatesEvaluated < maximumSearchStates)
            {
                if (HasPlaneCutSolveTimedOut(
                        stopwatch,
                        timeBudgetMilliseconds,
                        ref metrics))
                {
                    break;
                }

                PlaneCutJunctionSearchNode node = queue.Dequeue();
                if (node.Depth > bestCleanDepth)
                {
                    break;
                }

                PlaneCutJunctionSolveOutcome outcome =
                    EvaluatePlaneCutJunctionState(
                        sourceFaces,
                        context,
                        allEdges,
                        node.DeferredSourceEdges,
                        minimumStableEdgeLength,
                        minimumStableFaceArea,
                        stopwatch,
                        timeBudgetMilliseconds,
                        ref metrics);
                metrics.StatesEvaluated++;

                if (IsBetterPlaneCutPartialOutcome(
                        outcome,
                        bestPartial))
                {
                    bestPartial = outcome;
                }

                if (!outcome.HardFailure &&
                    outcome.UnresolvedVertices.Count == 0)
                {
                    if (bestClean == null ||
                        node.Depth < bestCleanDepth ||
                        (node.Depth == bestCleanDepth &&
                         outcome.QualityScore > bestClean.QualityScore))
                    {
                        bestClean = outcome;
                        bestCleanDepth = node.Depth;
                    }
                    continue;
                }

                if (outcome.HardFailure ||
                    outcome.UnresolvedVertices.Count == 0 ||
                    metrics.TimedOut != 0)
                {
                    continue;
                }

                int unresolvedVertex = outcome.UnresolvedVertices[0];
                List<PlaneCutBevelCandidate> branchEdges =
                    GetActivePlaneCutIncidentCandidates(
                        outcome.ActiveEdges,
                        unresolvedVertex);
                branchEdges.Sort((left, right) =>
                    ComparePlaneCutBacktrackCandidates(
                        left,
                        right,
                        context));

                for (int branchIndex = 0;
                     branchIndex < branchEdges.Count;
                     branchIndex++)
                {
                    List<int> deferred =
                        AddPlaneCutDeferredSourceEdge(
                            node.DeferredSourceEdges,
                            branchEdges[branchIndex].SourceEdgeIndex);
                    string key = BuildPlaneCutDeferredSetKey(deferred);
                    if (!visited.Add(key))
                    {
                        continue;
                    }
                    queue.Enqueue(new PlaneCutJunctionSearchNode(deferred));
                }
            }

            if (bestClean != null)
            {
                return CompletePlaneCutSolveMetrics(
                    bestClean,
                    stopwatch,
                    ref metrics);
            }

            PlaneCutJunctionSolveOutcome greedy = bestPartial;
            int greedyGuard = allEdges.Count;
            while (greedy != null &&
                   !greedy.HardFailure &&
                   greedy.UnresolvedVertices.Count > 0 &&
                   greedyGuard-- > 0 &&
                   metrics.StatesEvaluated < maximumSearchStates &&
                   metrics.TimedOut == 0)
            {
                if (HasPlaneCutSolveTimedOut(
                        stopwatch,
                        timeBudgetMilliseconds,
                        ref metrics))
                {
                    break;
                }

                int unresolvedVertex = greedy.UnresolvedVertices[0];
                List<PlaneCutBevelCandidate> incident =
                    GetActivePlaneCutIncidentCandidates(
                        greedy.ActiveEdges,
                        unresolvedVertex);
                if (incident.Count == 0)
                {
                    break;
                }
                incident.Sort((left, right) =>
                    ComparePlaneCutBacktrackCandidates(
                        left,
                        right,
                        context));

                List<int> deferred = new List<int>(
                    greedy.DeferredSourceEdges);
                deferred.Sort();
                deferred = AddPlaneCutDeferredSourceEdge(
                    deferred,
                    incident[0].SourceEdgeIndex);
                greedy = EvaluatePlaneCutJunctionState(
                    sourceFaces,
                    context,
                    allEdges,
                    deferred,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    stopwatch,
                    timeBudgetMilliseconds,
                    ref metrics);
                metrics.StatesEvaluated++;
            }

            if (greedy != null &&
                greedy.UnresolvedVertices.Count == 0 &&
                !greedy.HardFailure)
            {
                return CompletePlaneCutSolveMetrics(
                    greedy,
                    stopwatch,
                    ref metrics);
            }

            if (bestPartial == null)
            {
                bestPartial = new PlaneCutJunctionSolveOutcome
                {
                    HardFailure = true,
                    Diagnostic = metrics.TimedOut != 0
                        ? "the bounded global junction solve reached its editor time budget"
                        : "the bounded global junction search produced no state"
                };
            }
            else if (string.IsNullOrEmpty(bestPartial.Diagnostic))
            {
                bestPartial.Diagnostic = metrics.TimedOut != 0
                    ? "the bounded global junction solve reached its editor time budget"
                    : "the bounded global junction search found no clean compatible edge set";
            }

            return CompletePlaneCutSolveMetrics(
                bestPartial,
                stopwatch,
                ref metrics);
        }

        private static PlaneCutJunctionSolveOutcome
            CompletePlaneCutSolveMetrics(
                PlaneCutJunctionSolveOutcome outcome,
                System.Diagnostics.Stopwatch stopwatch,
                ref PlaneCutSolveMetrics metrics)
        {
            stopwatch.Stop();
            metrics.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            metrics.EdgesDeferred = outcome == null
                ? 0
                : outcome.DeferredSourceEdges.Count;
            return outcome;
        }

        private static bool HasPlaneCutSolveTimedOut(
            System.Diagnostics.Stopwatch stopwatch,
            long timeBudgetMilliseconds,
            ref PlaneCutSolveMetrics metrics)
        {
            if (stopwatch.ElapsedMilliseconds < timeBudgetMilliseconds)
            {
                return false;
            }
            metrics.TimedOut = 1;
            return true;
        }

        private static PlaneCutJunctionSolveOutcome
            EvaluatePlaneCutJunctionState(
                List<PolygonFace> sourceFaces,
                ChamferTopologyContext context,
                List<PlaneCutBevelCandidate> allEdges,
                List<int> deferredSourceEdges,
                float minimumStableEdgeLength,
                float minimumStableFaceArea,
                System.Diagnostics.Stopwatch stopwatch,
                long timeBudgetMilliseconds,
                ref PlaneCutSolveMetrics metrics)
        {
            PlaneCutJunctionSolveOutcome outcome =
                new PlaneCutJunctionSolveOutcome();
            for (int deferredIndex = 0;
                 deferredIndex < deferredSourceEdges.Count;
                 deferredIndex++)
            {
                outcome.DeferredSourceEdges.Add(
                    deferredSourceEdges[deferredIndex]);
            }

            outcome.ActiveEdges = new List<PlaneCutBevelCandidate>();
            for (int edgeIndex = 0; edgeIndex < allEdges.Count; edgeIndex++)
            {
                PlaneCutBevelCandidate candidate = allEdges[edgeIndex];
                if (!outcome.DeferredSourceEdges.Contains(
                        candidate.SourceEdgeIndex))
                {
                    outcome.ActiveEdges.Add(candidate);
                }
            }
            outcome.ActiveEdges.Sort((left, right) =>
                left.SourceEdgeIndex.CompareTo(right.SourceEdgeIndex));

            if (outcome.ActiveEdges.Count == 0)
            {
                outcome.HardFailure = true;
                outcome.Diagnostic =
                    "global junction backtracking removed every bevel edge";
                return outcome;
            }

            List<PlaneCutVertexJunctionCandidate> acceptedJunctions =
                new List<PlaneCutVertexJunctionCandidate>();
            metrics.SystemRebuilds++;
            if (!TryBuildPlaneCutSystemFaces(
                    sourceFaces,
                    outcome.ActiveEdges,
                    acceptedJunctions,
                    out List<PolygonFace> currentFaces,
                    out int edgeCapsBuilt,
                    out string blocker))
            {
                outcome.HardFailure = true;
                outcome.Diagnostic = blocker;
                return outcome;
            }
            outcome.EdgeCapsBuilt = edgeCapsBuilt;

            Dictionary<int, List<PlaneCutBevelCandidate>> incidentByVertex =
                BuildPlaneCutIncidentMap(outcome.ActiveEdges);
            for (int vertexIndex = 0;
                 vertexIndex < context.Graph.Vertices.Count;
                 vertexIndex++)
            {
                if (!incidentByVertex.TryGetValue(
                        vertexIndex,
                        out List<PlaneCutBevelCandidate> incident) ||
                    incident.Count < 2)
                {
                    continue;
                }

                if (HasPlaneCutSolveTimedOut(
                        stopwatch,
                        timeBudgetMilliseconds,
                        ref metrics))
                {
                    outcome.UnresolvedVertices.Add(vertexIndex);
                    outcome.Diagnostic =
                        "the bounded global junction solve reached its editor time budget";
                    break;
                }

                metrics.JunctionsVisited++;
                if (!TryFindBestPlaneCutVertexJunction(
                        currentFaces,
                        context,
                        acceptedJunctions,
                        vertexIndex,
                        incident,
                        minimumStableEdgeLength,
                        minimumStableFaceArea,
                        stopwatch,
                        timeBudgetMilliseconds,
                        ref metrics,
                        out PlaneCutVertexJunctionCandidate junction,
                        out List<PolygonFace> transaction))
                {
                    outcome.UnresolvedVertices.Add(vertexIndex);
                    continue;
                }

                acceptedJunctions.Add(junction);
                currentFaces = transaction;
                outcome.QualityScore +=
                    junction.Compactness -
                    Mathf.Max(0f, junction.AspectRatio - 2f) * 0.02f -
                    junction.CutDepth * 0.01f -
                    junction.CapVertexCount * 0.0005f;
            }

            outcome.Junctions.AddRange(acceptedJunctions);
            if (outcome.UnresolvedVertices.Count == 0 &&
                HasPlaneCutSolveTimedOut(
                    stopwatch,
                    timeBudgetMilliseconds,
                    ref metrics))
            {
                for (int vertexIndex = 0;
                     vertexIndex < context.Graph.Vertices.Count;
                     vertexIndex++)
                {
                    if (incidentByVertex.TryGetValue(
                            vertexIndex,
                            out List<PlaneCutBevelCandidate> incident) &&
                        incident.Count >= 2)
                    {
                        outcome.UnresolvedVertices.Add(vertexIndex);
                        break;
                    }
                }
                outcome.Diagnostic =
                    "the bounded global junction solve reached its editor time budget";
            }

            if (outcome.UnresolvedVertices.Count == 0)
            {
                metrics.SystemRebuilds++;
                if (!TryBuildPlaneCutSystemFaces(
                        sourceFaces,
                        outcome.ActiveEdges,
                        acceptedJunctions,
                        out List<PolygonFace> rebuiltFaces,
                        out edgeCapsBuilt,
                        out blocker))
                {
                    outcome.HardFailure = true;
                    outcome.Diagnostic = blocker;
                    currentFaces = null;
                }
                else
                {
                    currentFaces = rebuiltFaces;
                    outcome.EdgeCapsBuilt = edgeCapsBuilt;
                    if (!IsPlaneCutJunctionTrialGeometryValid(
                            currentFaces,
                            context,
                            outcome.ActiveEdges,
                            acceptedJunctions,
                            minimumStableEdgeLength,
                            minimumStableFaceArea,
                            ref metrics,
                            out int bandFailureVertex,
                            out string exactBlocker))
                    {
                        if (bandFailureVertex >= 0)
                        {
                            outcome.UnresolvedVertices.Add(
                                bandFailureVertex);
                        }
                        for (int vertexIndex = 0;
                             vertexIndex < context.Graph.Vertices.Count;
                             vertexIndex++)
                        {
                            if (outcome.UnresolvedVertices.Count == 0 &&
                                incidentByVertex.TryGetValue(
                                    vertexIndex,
                                    out List<PlaneCutBevelCandidate> incident) &&
                                incident.Count >= 2)
                            {
                                outcome.UnresolvedVertices.Add(vertexIndex);
                                break;
                            }
                        }
                        if (outcome.UnresolvedVertices.Count == 0)
                        {
                            outcome.HardFailure = true;
                        }
                        outcome.Diagnostic = string.IsNullOrEmpty(exactBlocker)
                            ? "the complete junction state failed the exact polygon or triangle audit"
                            : exactBlocker;
                    }
                }
            }

            outcome.Faces = currentFaces;
            return outcome;
        }

        private static bool TryFindBestPlaneCutVertexJunction(
            List<PolygonFace> currentFaces,
            ChamferTopologyContext context,
            List<PlaneCutVertexJunctionCandidate> acceptedJunctions,
            int vertexIndex,
            List<PlaneCutBevelCandidate> incident,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            System.Diagnostics.Stopwatch stopwatch,
            long timeBudgetMilliseconds,
            ref PlaneCutSolveMetrics metrics,
            out PlaneCutVertexJunctionCandidate bestJunction,
            out List<PolygonFace> bestFaces)
        {
            bestJunction = default;
            bestFaces = null;

            List<PlaneCutJunctionNormalOption> normalOptions =
                BuildPlaneCutJunctionNormalOptions(
                    context,
                    vertexIndex,
                    incident);
            if (normalOptions.Count == 0)
            {
                return false;
            }

            float[] adaptiveDepthFactors =
            {
                0.35f,
                0.50f,
                0.75f,
                1.00f,
                1.25f
            };
            bool found = false;
            for (int normalIndex = 0;
                 normalIndex < normalOptions.Count;
                 normalIndex++)
            {
                PlaneCutJunctionNormalOption option =
                    normalOptions[normalIndex];
                int trialCount = option.IsDirect
                    ? adaptiveDepthFactors.Length + 1
                    : adaptiveDepthFactors.Length;
                for (int trialIndex = 0;
                     trialIndex < trialCount;
                     trialIndex++)
                {
                    if (HasPlaneCutSolveTimedOut(
                            stopwatch,
                            timeBudgetMilliseconds,
                            ref metrics))
                    {
                        return found;
                    }

                    metrics.CandidateTrials++;
                    float depthFactor = option.IsDirect && trialIndex == 0
                        ? 0.60f
                        : adaptiveDepthFactors[
                            option.IsDirect
                                ? trialIndex - 1
                                : trialIndex];
                    bool isDirect = option.IsDirect && trialIndex == 0;
                    if (!TryBuildPlaneCutVertexJunctionCandidate(
                            currentFaces,
                            context,
                            vertexIndex,
                            incident,
                            option.Normal,
                            option.Rank,
                            isDirect,
                            depthFactor,
                            minimumStableEdgeLength,
                            out PlaneCutVertexJunctionCandidate junction))
                    {
                        continue;
                    }

                    List<PolygonFace> transaction =
                        ClonePolygonFacesForPlaneCutAudit(currentFaces);
                    ClipPolyhedron(
                        transaction,
                        junction.Plane,
                        PolygonFaceFeature.ConvexEdgeWear,
                        junction.Strength,
                        true,
                        junction.ClipEpsilon,
                        true,
                        PolygonFaceProvenanceKind.VertexJunctionPlane,
                        junction.VertexIndex);

                    if (!TryFindSinglePlaneCutCap(
                            transaction,
                            junction.Plane,
                            junction.PlaneTolerance,
                            out PolygonFace cap) ||
                        !IsStablePlaneCutVertexJunctionCap(
                            cap,
                            context.Graph.Vertices[vertexIndex].Position,
                            junction.LocalRadius,
                            minimumStableEdgeLength,
                            minimumStableFaceArea,
                            out float compactness,
                            out float aspectRatio) ||
                        !DoesPlaneCutJunctionJoinIncidentBevels(
                            transaction,
                            junction,
                            incident) ||
                        !IsPlaneCutJunctionInfluenceLocal(
                            transaction,
                            context,
                            junction,
                            incident,
                            minimumStableEdgeLength,
                            out _,
                            out _) ||
                        !DoAcceptedPlaneCutJunctionCapsSurvive(
                            transaction,
                            acceptedJunctions))
                    {
                        continue;
                    }

                    PlaneCutVertexJunctionCandidate scored =
                        new PlaneCutVertexJunctionCandidate(
                            junction.VertexIndex,
                            junction.Plane,
                            junction.Strength,
                            junction.PlaneTolerance,
                            junction.ClipEpsilon,
                            junction.LocalRadius,
                            junction.CutDepth,
                            compactness,
                            aspectRatio,
                            cap.Vertices.Count,
                            junction.NormalRank,
                            junction.IsDirect);
                    if (!found ||
                        IsBetterPlaneCutVertexJunction(
                            scored,
                            bestJunction))
                    {
                        found = true;
                        bestJunction = scored;
                        bestFaces = transaction;
                    }
                }
            }

            return found;
        }

        private static bool IsPlaneCutJunctionInfluenceLocal(
            List<PolygonFace> faces,
            ChamferTopologyContext context,
            PlaneCutVertexJunctionCandidate junction,
            List<PlaneCutBevelCandidate> incident,
            float minimumStableEdgeLength,
            out float maximumInfluenceRatio,
            out float maximumSharedAxisSpanRatio)
        {
            maximumInfluenceRatio = 0f;
            maximumSharedAxisSpanRatio = 0f;
            if (!TryFindSinglePlaneCutProvenanceFace(
                    faces,
                    PolygonFaceProvenanceKind.VertexJunctionPlane,
                    junction.VertexIndex,
                    out PolygonFace junctionFace))
            {
                return false;
            }

            for (int incidentIndex = 0;
                 incidentIndex < incident.Count;
                 incidentIndex++)
            {
                PlaneCutBevelCandidate edge = incident[incidentIndex];
                if (!TryMeasurePlaneCutJunctionInfluence(
                        junctionFace,
                        context,
                        junction,
                        edge,
                        minimumStableEdgeLength,
                        out float influenceRatio,
                        out float sharedAxisSpanRatio,
                        out float allowedInfluenceRatio))
                {
                    return false;
                }

                maximumInfluenceRatio = Mathf.Max(
                    maximumInfluenceRatio,
                    influenceRatio);
                maximumSharedAxisSpanRatio = Mathf.Max(
                    maximumSharedAxisSpanRatio,
                    sharedAxisSpanRatio);
                float ratioTolerance = Mathf.Max(
                    0.0025f,
                    edge.PlaneTolerance /
                        Mathf.Max(
                            Vector3.Distance(
                                context.Graph.Vertices[edge.VertexA].Position,
                                context.Graph.Vertices[edge.VertexB].Position),
                            PointMergeDistance));
                if (influenceRatio >
                        allowedInfluenceRatio + ratioTolerance ||
                    sharedAxisSpanRatio >
                        allowedInfluenceRatio * 1.25f + ratioTolerance)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool TryMeasurePlaneCutJunctionInfluence(
            PolygonFace junctionFace,
            ChamferTopologyContext context,
            PlaneCutVertexJunctionCandidate junction,
            PlaneCutBevelCandidate edge,
            float minimumStableEdgeLength,
            out float influenceRatio,
            out float sharedAxisSpanRatio,
            out float allowedInfluenceRatio)
        {
            influenceRatio = 0f;
            sharedAxisSpanRatio = 0f;
            allowedInfluenceRatio = 0f;
            if (junctionFace == null ||
                (junction.VertexIndex != edge.VertexA &&
                 junction.VertexIndex != edge.VertexB))
            {
                return false;
            }

            Vector3 sourceA =
                context.Graph.Vertices[edge.VertexA].Position;
            Vector3 sourceB =
                context.Graph.Vertices[edge.VertexB].Position;
            Vector3 edgeVector = sourceB - sourceA;
            float edgeLength = edgeVector.magnitude;
            if (edgeLength <= PointMergeDistance)
            {
                return false;
            }
            Vector3 edgeDirection = edgeVector / edgeLength;
            float tolerance = Mathf.Max(
                edge.PlaneTolerance,
                junction.PlaneTolerance) * 1.5f;
            float minimumParameter = float.PositiveInfinity;
            float maximumParameter = float.NegativeInfinity;
            int intersectionVertexCount = 0;
            for (int vertexIndex = 0;
                 vertexIndex < junctionFace.Vertices.Count;
                 vertexIndex++)
            {
                Vector3 vertex = junctionFace.Vertices[vertexIndex];
                if (Mathf.Abs(edge.Plane.SignedDistance(vertex)) >
                    tolerance)
                {
                    continue;
                }
                float parameter = Vector3.Dot(
                    vertex - sourceA,
                    edgeDirection) / edgeLength;
                minimumParameter = Mathf.Min(
                    minimumParameter,
                    parameter);
                maximumParameter = Mathf.Max(
                    maximumParameter,
                    parameter);
                intersectionVertexCount++;
            }
            if (intersectionVertexCount < 2 ||
                float.IsInfinity(minimumParameter) ||
                float.IsInfinity(maximumParameter))
            {
                return false;
            }

            sharedAxisSpanRatio = Mathf.Max(
                0f,
                maximumParameter - minimumParameter);
            influenceRatio = junction.VertexIndex == edge.VertexA
                ? Mathf.Max(0f, maximumParameter)
                : Mathf.Max(0f, 1f - minimumParameter);
            float allowedDistance = Mathf.Max(
                edge.Width * 4f,
                Mathf.Max(
                    junction.CutDepth * 3f,
                    minimumStableEdgeLength * 0.5f));
            allowedInfluenceRatio = Mathf.Clamp(
                allowedDistance / edgeLength,
                0.03f,
                0.25f);
            return true;
        }

        private static void AuditPlaneCutBandIntegrity(
            List<PolygonFace> faces,
            ChamferTopologyContext context,
            List<PlaneCutBevelCandidate> activeEdges,
            List<PlaneCutVertexJunctionCandidate> junctions,
            float minimumStableEdgeLength,
            ref PlaneCutBevelAuditResult result,
            out int offendingVertex,
            out string blocker)
        {
            offendingVertex = -1;
            blocker = string.Empty;
            result.EdgeConflictVictimEdgeIndex = -1;
            result.EdgeConflictForeignEdgeIndex = -1;
            result.EdgeConflictVertexIndex = -1;
            result.EdgeConflictDeferredEdgeIndex = -1;
            result.BandMinimumCoverageRatio = activeEdges.Count > 0
                ? 1f
                : 0f;
            Dictionary<int, PlaneCutVertexJunctionCandidate>
                junctionByVertex =
                    new Dictionary<int, PlaneCutVertexJunctionCandidate>();
            for (int junctionIndex = 0;
                 junctionIndex < junctions.Count;
                 junctionIndex++)
            {
                junctionByVertex[junctions[junctionIndex].VertexIndex] =
                    junctions[junctionIndex];
            }

            for (int edgeIndex = 0;
                 edgeIndex < activeEdges.Count;
                 edgeIndex++)
            {
                PlaneCutBevelCandidate edge = activeEdges[edgeIndex];
                result.BandRetainedEdgeCount++;
                List<PolygonFace> edgeFaces =
                    FindPlaneCutProvenanceFaces(
                        faces,
                        PolygonFaceProvenanceKind.EdgeBevelPlane,
                        edge.SourceEdgeIndex);
                if (edgeFaces.Count != 1)
                {
                    if (edgeFaces.Count == 0)
                    {
                        result.BandCollapsedCount++;
                        result.BandMinimumCoverageRatio = 0f;
                    }
                    else
                    {
                        result.BandSplitCount++;
                    }
                    RecordPlaneCutBandConflict(
                        ref result,
                        edge.SourceEdgeIndex,
                        -1,
                        edge.VertexA,
                        0f,
                        0f,
                        0f);
                    if (offendingVertex < 0)
                    {
                        offendingVertex = edge.VertexA;
                        blocker = edgeFaces.Count == 0
                            ? "bevel-band edge " +
                                edge.SourceEdgeIndex +
                                " has no surviving owned face"
                            : "bevel-band edge " +
                                edge.SourceEdgeIndex +
                                " split into " + edgeFaces.Count +
                                " owned faces";
                    }
                    continue;
                }
                result.BandSingleFaceCount++;

                PolygonFace edgeFace = edgeFaces[0];
                Vector3 sourceA =
                    context.Graph.Vertices[edge.VertexA].Position;
                Vector3 sourceB =
                    context.Graph.Vertices[edge.VertexB].Position;
                Vector3 edgeVector = sourceB - sourceA;
                float edgeLength = edgeVector.magnitude;
                if (edgeLength <= PointMergeDistance)
                {
                    result.BandCollapsedCount++;
                    result.BandMinimumCoverageRatio = 0f;
                    RecordPlaneCutBandConflict(
                        ref result,
                        edge.SourceEdgeIndex,
                        -1,
                        edge.VertexA,
                        0f,
                        0f,
                        0f);
                    if (offendingVertex < 0)
                    {
                        offendingVertex = edge.VertexA;
                        blocker = "bevel-band edge " +
                            edge.SourceEdgeIndex +
                            " has a collapsed source axis";
                    }
                    continue;
                }
                Vector3 edgeDirection = edgeVector / edgeLength;
                float minimumParameter = float.PositiveInfinity;
                float maximumParameter = float.NegativeInfinity;
                for (int vertexIndex = 0;
                     vertexIndex < edgeFace.Vertices.Count;
                     vertexIndex++)
                {
                    float parameter = Vector3.Dot(
                        edgeFace.Vertices[vertexIndex] - sourceA,
                        edgeDirection) / edgeLength;
                    minimumParameter = Mathf.Min(
                        minimumParameter,
                        parameter);
                    maximumParameter = Mathf.Max(
                        maximumParameter,
                        parameter);
                }
                float coverageRatio = Mathf.Max(
                    0f,
                    Mathf.Min(1f, maximumParameter) -
                    Mathf.Max(0f, minimumParameter));
                result.BandMinimumCoverageRatio = Mathf.Min(
                    result.BandMinimumCoverageRatio,
                    coverageRatio);
                if (coverageRatio < 0.35f)
                {
                    result.BandCollapsedCount++;
                    RecordPlaneCutBandConflict(
                        ref result,
                        edge.SourceEdgeIndex,
                        -1,
                        edge.VertexA,
                        coverageRatio,
                        0f,
                        0f);
                    if (offendingVertex < 0)
                    {
                        offendingVertex = edge.VertexA;
                        blocker = "bevel-band edge " +
                            edge.SourceEdgeIndex +
                            " collapsed to axial coverage " +
                            coverageRatio.ToString("G4");
                    }
                }

                bool edgeInterrupted = false;
                bool edgeForeignCut = false;
                bool edgeOverlong = false;
                int[] endpointVertices =
                {
                    edge.VertexA,
                    edge.VertexB
                };
                for (int endpointIndex = 0;
                     endpointIndex < endpointVertices.Length;
                     endpointIndex++)
                {
                    int endpointVertex = endpointVertices[endpointIndex];
                    if (!junctionByVertex.TryGetValue(
                            endpointVertex,
                            out PlaneCutVertexJunctionCandidate junction) ||
                        !TryFindSinglePlaneCutProvenanceFace(
                            faces,
                            PolygonFaceProvenanceKind.VertexJunctionPlane,
                            endpointVertex,
                            out PolygonFace junctionFace))
                    {
                        continue;
                    }
                    if (!TryMeasurePlaneCutJunctionInfluence(
                            junctionFace,
                            context,
                            junction,
                            edge,
                            minimumStableEdgeLength,
                            out float influenceRatio,
                            out float sharedAxisSpanRatio,
                            out float allowedInfluenceRatio))
                    {
                        edgeInterrupted = true;
                        RecordPlaneCutBandConflict(
                            ref result,
                            edge.SourceEdgeIndex,
                            -1,
                            endpointVertex,
                            coverageRatio,
                            0f,
                            0f);
                        if (offendingVertex < 0)
                        {
                            offendingVertex = endpointVertex;
                            blocker = "junction " + endpointVertex +
                                " does not expose a measurable local intersection with bevel-band edge " +
                                edge.SourceEdgeIndex;
                        }
                        continue;
                    }
                    result.BandMaximumJunctionInfluenceRatio = Mathf.Max(
                        result.BandMaximumJunctionInfluenceRatio,
                        influenceRatio);
                    result.BandMaximumSharedAxisSpanRatio = Mathf.Max(
                        result.BandMaximumSharedAxisSpanRatio,
                        sharedAxisSpanRatio);
                    float ratioTolerance = Mathf.Max(
                        0.0025f,
                        edge.PlaneTolerance / edgeLength);
                    if (influenceRatio >
                            allowedInfluenceRatio + ratioTolerance ||
                        sharedAxisSpanRatio >
                            allowedInfluenceRatio * 1.25f +
                                ratioTolerance)
                    {
                        edgeOverlong = true;
                        edgeInterrupted = true;
                        RecordPlaneCutBandConflict(
                            ref result,
                            edge.SourceEdgeIndex,
                            -1,
                            endpointVertex,
                            coverageRatio,
                            influenceRatio,
                            sharedAxisSpanRatio);
                        if (offendingVertex < 0)
                        {
                            offendingVertex = endpointVertex;
                            blocker = "junction " + endpointVertex +
                                " cuts bevel-band edge " +
                                edge.SourceEdgeIndex +
                                " for influence/span " +
                                influenceRatio.ToString("G4") + "/" +
                                sharedAxisSpanRatio.ToString("G4") +
                                " above local limit " +
                                allowedInfluenceRatio.ToString("G4");
                        }
                    }
                }

                for (int boundaryIndex = 0;
                     boundaryIndex < edgeFace.Vertices.Count;
                     boundaryIndex++)
                {
                    Vector3 start = edgeFace.Vertices[boundaryIndex];
                    Vector3 end = edgeFace.Vertices[
                        (boundaryIndex + 1) % edgeFace.Vertices.Count];
                    if (!TryFindPlaneCutAdjacentFace(
                            faces,
                            edgeFace,
                            start,
                            end,
                            out PolygonFace adjacent) ||
                        adjacent.Feature !=
                            PolygonFaceFeature.ConvexEdgeWear)
                    {
                        continue;
                    }
                    if (adjacent.ProvenanceKind ==
                            PolygonFaceProvenanceKind.VertexJunctionPlane &&
                        (adjacent.ProvenanceIndex == edge.VertexA ||
                         adjacent.ProvenanceIndex == edge.VertexB))
                    {
                        continue;
                    }

                    float startParameter = Vector3.Dot(
                        start - sourceA,
                        edgeDirection) / edgeLength;
                    float endParameter = Vector3.Dot(
                        end - sourceA,
                        edgeDirection) / edgeLength;
                    float midpointParameter =
                        (startParameter + endParameter) * 0.5f;
                    float endpointAllowance = Mathf.Clamp(
                        Mathf.Max(
                            edge.Width * 4f,
                            minimumStableEdgeLength * 0.5f) /
                            edgeLength,
                        0.03f,
                        0.25f);
                    if (midpointParameter > endpointAllowance &&
                        midpointParameter < 1f - endpointAllowance)
                    {
                        edgeForeignCut = true;
                        edgeInterrupted = true;
                        int foreignEdgeIndex =
                            adjacent.ProvenanceKind ==
                                PolygonFaceProvenanceKind.EdgeBevelPlane
                                ? adjacent.ProvenanceIndex
                                : -1;
                        int conflictVertex = midpointParameter < 0.5f
                            ? edge.VertexA
                            : edge.VertexB;
                        RecordPlaneCutBandConflict(
                            ref result,
                            edge.SourceEdgeIndex,
                            foreignEdgeIndex,
                            conflictVertex,
                            coverageRatio,
                            midpointParameter,
                            Mathf.Abs(endParameter - startParameter));
                        if (offendingVertex < 0)
                        {
                            offendingVertex = conflictVertex;
                            blocker = "foreign generated plane " +
                                adjacent.ProvenanceKind + ":" +
                                adjacent.ProvenanceIndex +
                                " splits bevel-band edge " +
                                edge.SourceEdgeIndex +
                                " at axial parameter " +
                                midpointParameter.ToString("G4");
                        }
                    }
                }

                if (edgeInterrupted)
                {
                    RecordPlaneCutBandConflict(
                        ref result,
                        edge.SourceEdgeIndex,
                        -1,
                        edge.VertexA,
                        coverageRatio,
                        0f,
                        0f);
                    result.BandInterruptedCount++;
                }
                if (edgeForeignCut)
                {
                    result.BandForeignCutCount++;
                }
                if (edgeOverlong)
                {
                    result.BandOverlongJunctionCount++;
                }
            }
        }

        private static void RecordPlaneCutBandConflict(
            ref PlaneCutBevelAuditResult result,
            int victimEdgeIndex,
            int foreignEdgeIndex,
            int vertexIndex,
            float victimCoverageRatio,
            float foreignAxialParameter,
            float foreignSharedSpanRatio)
        {
            if (result.EdgeConflictVictimEdgeIndex >= 0)
            {
                return;
            }
            result.EdgeConflictVictimEdgeIndex = victimEdgeIndex;
            result.EdgeConflictForeignEdgeIndex = foreignEdgeIndex;
            result.EdgeConflictVertexIndex = vertexIndex;
            result.EdgeConflictVictimCoverageRatio =
                victimCoverageRatio;
            result.EdgeConflictForeignAxialParameter =
                foreignAxialParameter;
            result.EdgeConflictForeignSharedSpanRatio =
                foreignSharedSpanRatio;
        }

        private static List<PolygonFace> FindPlaneCutProvenanceFaces(
            List<PolygonFace> faces,
            PolygonFaceProvenanceKind provenanceKind,
            int provenanceIndex)
        {
            List<PolygonFace> matches = new List<PolygonFace>();
            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face.ProvenanceKind == provenanceKind &&
                    face.ProvenanceIndex == provenanceIndex)
                {
                    matches.Add(face);
                }
            }
            return matches;
        }

        private static bool TryFindSinglePlaneCutProvenanceFace(
            List<PolygonFace> faces,
            PolygonFaceProvenanceKind provenanceKind,
            int provenanceIndex,
            out PolygonFace match)
        {
            match = null;
            int matchCount = 0;
            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face.ProvenanceKind != provenanceKind ||
                    face.ProvenanceIndex != provenanceIndex)
                {
                    continue;
                }
                match = face;
                matchCount++;
            }
            return matchCount == 1;
        }

        private static bool TryFindPlaneCutAdjacentFace(
            List<PolygonFace> faces,
            PolygonFace sourceFace,
            Vector3 start,
            Vector3 end,
            out PolygonFace adjacent)
        {
            adjacent = null;
            EdgeKey target = new EdgeKey(start, end);
            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (object.ReferenceEquals(face, sourceFace))
                {
                    continue;
                }
                for (int edgeIndex = 0;
                     edgeIndex < face.Vertices.Count;
                     edgeIndex++)
                {
                    EdgeKey candidate = new EdgeKey(
                        face.Vertices[edgeIndex],
                        face.Vertices[
                            (edgeIndex + 1) % face.Vertices.Count]);
                    if (candidate.Equals(target))
                    {
                        adjacent = face;
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool DoAcceptedPlaneCutJunctionCapsSurvive(
            List<PolygonFace> faces,
            List<PlaneCutVertexJunctionCandidate> acceptedJunctions)
        {
            for (int junctionIndex = 0;
                 junctionIndex < acceptedJunctions.Count;
                 junctionIndex++)
            {
                PlaneCutVertexJunctionCandidate junction =
                    acceptedJunctions[junctionIndex];
                if (!TryFindSinglePlaneCutCap(
                        faces,
                        junction.Plane,
                        junction.PlaneTolerance,
                        out _))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsPlaneCutJunctionTrialGeometryValid(
            List<PolygonFace> trialFaces,
            ChamferTopologyContext context,
            List<PlaneCutBevelCandidate> activeEdges,
            List<PlaneCutVertexJunctionCandidate> junctions,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            ref PlaneCutSolveMetrics metrics,
            out int bandFailureVertex,
            out string blocker)
        {
            bandFailureVertex = -1;
            blocker = string.Empty;
            metrics.PolygonAudits++;
            List<PolygonFace> trialClone =
                ClonePolygonFacesForPlaneCutAudit(trialFaces);
            if (!TryPreparePlaneCutPreviewFaces(
                    trialClone,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    out List<PolygonFace> prepared,
                    out _,
                    out _,
                    out _,
                    out _))
            {
                blocker = "the complete junction state failed preview preparation";
                return false;
            }

            EdgeWearTopologyStats topology = AuditEdgeWearTopology(
                prepared,
                minimumStableEdgeLength);
            PlaneCutBevelAuditResult qualityAudit =
                new PlaneCutBevelAuditResult();
            AuditPlaneCutFaceQuality(
                prepared,
                junctions,
                minimumStableEdgeLength,
                ref qualityAudit);
            AuditPlaneCutBandIntegrity(
                prepared,
                context,
                activeEdges,
                junctions,
                minimumStableEdgeLength,
                ref qualityAudit,
                out bandFailureVertex,
                out blocker);
            if (topology.OpenEdgeCount > 0 ||
                topology.NonManifoldEdgeCount > 0 ||
                topology.TJunctionCount > 0 ||
                CountInvalidPlaneCutFaces(
                    prepared,
                    minimumStableFaceArea) > 0 ||
                qualityAudit.FaceQualityNonPlanarCount > 0 ||
                qualityAudit.FaceQualityElongatedJunctionCount > 0 ||
                qualityAudit.BandSplitCount > 0 ||
                qualityAudit.BandInterruptedCount > 0 ||
                qualityAudit.BandForeignCutCount > 0 ||
                qualityAudit.BandOverlongJunctionCount > 0 ||
                qualityAudit.BandCollapsedCount > 0)
            {
                if (string.IsNullOrEmpty(blocker))
                {
                    blocker =
                        "the complete junction state failed exact topology, face-quality, bevel-band, or triangle certification";
                }
                return false;
            }

            TriangleSoup soup = TriangulatePlaneCutPreviewFaces(prepared);
            if (soup == null)
            {
                return false;
            }
            metrics.TriangleAudits++;
            PlaneCutBevelAuditResult audit =
                new PlaneCutBevelAuditResult();
            AuditPlaneCutPreviewTriangleSoup(
                soup,
                prepared,
                minimumStableEdgeLength,
                ref audit);
            return audit.PreviewGeometryValid == 1;
        }

        private static bool TryBuildPlaneCutSystemFaces(
            List<PolygonFace> sourceFaces,
            List<PlaneCutBevelCandidate> activeEdges,
            List<PlaneCutVertexJunctionCandidate> junctions,
            out List<PolygonFace> faces,
            out int edgeCapsBuilt,
            out string blocker)
        {
            faces = ClonePolygonFacesForPlaneCutAudit(
                sourceFaces,
                true);
            edgeCapsBuilt = 0;
            blocker = string.Empty;
            for (int edgeIndex = 0;
                 edgeIndex < activeEdges.Count;
                 edgeIndex++)
            {
                PlaneCutBevelCandidate candidate = activeEdges[edgeIndex];
                int before = CountMatchingPlaneCutCaps(faces, candidate);
                if (before == 0 &&
                    IsPlaneCutCandidateAlreadySatisfied(faces, candidate))
                {
                    continue;
                }

                ClipPolyhedron(
                    faces,
                    candidate.Plane,
                    PolygonFaceFeature.ConvexEdgeWear,
                    candidate.Strength,
                    true,
                    candidate.ClipEpsilon,
                    true,
                    PolygonFaceProvenanceKind.EdgeBevelPlane,
                    candidate.SourceEdgeIndex);
                int after = CountMatchingPlaneCutCaps(faces, candidate);
                if (before == 0 && after == 1)
                {
                    edgeCapsBuilt++;
                }
                else if (after > 1)
                {
                    blocker = "a bevel plane emitted duplicate caps";
                    return false;
                }
            }

            for (int junctionIndex = 0;
                 junctionIndex < junctions.Count;
                 junctionIndex++)
            {
                PlaneCutVertexJunctionCandidate junction =
                    junctions[junctionIndex];
                ClipPolyhedron(
                    faces,
                    junction.Plane,
                    PolygonFaceFeature.ConvexEdgeWear,
                    junction.Strength,
                    true,
                    junction.ClipEpsilon,
                    true,
                    PolygonFaceProvenanceKind.VertexJunctionPlane,
                    junction.VertexIndex);
                if (!TryFindSinglePlaneCutCap(
                        faces,
                        junction.Plane,
                        junction.PlaneTolerance,
                        out _))
                {
                    blocker = "a selected vertex-junction plane emitted no unique cap";
                    return false;
                }
            }

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
                        out _))
                {
                    blocker = "a later vertex-junction cut consumed an earlier junction cap";
                    return false;
                }
            }
            return true;
        }

        private static List<PlaneCutJunctionNormalOption>
            BuildPlaneCutJunctionNormalOptions(
                ChamferTopologyContext context,
                int vertexIndex,
                List<PlaneCutBevelCandidate> incident)
        {
            List<PlaneCutJunctionNormalOption> options =
                new List<PlaneCutJunctionNormalOption>();
            Vector3 sourceVertex =
                context.Graph.Vertices[vertexIndex].Position;
            Vector3 centroid = CalculatePlaneCutGraphCentroid(context);
            Vector3 radial = sourceVertex - centroid;
            Vector3 bevelSum = Vector3.zero;
            for (int incidentIndex = 0;
                 incidentIndex < incident.Count;
                 incidentIndex++)
            {
                bevelSum += incident[incidentIndex].Plane.Normal;
            }
            AddPlaneCutJunctionNormalOption(
                options,
                bevelSum,
                radial,
                0,
                true);

            Vector3 faceSum = CalculatePlaneCutCornerFaceNormalSum(
                context,
                vertexIndex);
            AddPlaneCutJunctionNormalOption(
                options,
                faceSum,
                radial,
                1,
                false);
            AddPlaneCutJunctionNormalOption(
                options,
                radial,
                radial,
                2,
                false);
            AddPlaneCutJunctionNormalOption(
                options,
                Vector3.Lerp(faceSum, radial, 0.25f),
                radial,
                3,
                false);
            AddPlaneCutJunctionNormalOption(
                options,
                Vector3.Lerp(faceSum, radial, 0.50f),
                radial,
                4,
                false);
            AddPlaneCutJunctionNormalOption(
                options,
                Vector3.Lerp(faceSum, radial, 0.75f),
                radial,
                5,
                false);
            AddPlaneCutJunctionNormalOption(
                options,
                Vector3.Lerp(bevelSum, faceSum, 0.50f),
                radial,
                6,
                false);
            return options;
        }

        private static void AddPlaneCutJunctionNormalOption(
            List<PlaneCutJunctionNormalOption> options,
            Vector3 normal,
            Vector3 outwardReference,
            int rank,
            bool isDirect)
        {
            if (!IsFinite(normal) ||
                normal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return;
            }
            normal.Normalize();
            if (outwardReference.sqrMagnitude > MinimumEdgeLengthSqr &&
                Vector3.Dot(normal, outwardReference) < 0f)
            {
                normal = -normal;
            }
            for (int optionIndex = 0;
                 optionIndex < options.Count;
                 optionIndex++)
            {
                if (Vector3.Dot(
                        options[optionIndex].Normal,
                        normal) > 0.9995f)
                {
                    return;
                }
            }
            options.Add(new PlaneCutJunctionNormalOption(
                normal,
                rank,
                isDirect));
        }

        private static Vector3 CalculatePlaneCutGraphCentroid(
            ChamferTopologyContext context)
        {
            Vector3 centroid = Vector3.zero;
            if (context.Graph.Vertices.Count == 0)
            {
                return centroid;
            }
            for (int vertexIndex = 0;
                 vertexIndex < context.Graph.Vertices.Count;
                 vertexIndex++)
            {
                centroid += context.Graph.Vertices[vertexIndex].Position;
            }
            return centroid / context.Graph.Vertices.Count;
        }

        private static Vector3 CalculatePlaneCutCornerFaceNormalSum(
            ChamferTopologyContext context,
            int vertexIndex)
        {
            Vector3 sum = Vector3.zero;
            EdgeWearGraphVertex vertex = context.Graph.Vertices[vertexIndex];
            for (int incidentFaceIndex = 0;
                 incidentFaceIndex < vertex.FaceIndices.Count;
                 incidentFaceIndex++)
            {
                int faceIndex = vertex.FaceIndices[incidentFaceIndex];
                if (faceIndex < 0 ||
                    faceIndex >= context.Graph.Faces.Count)
                {
                    continue;
                }
                EdgeWearGraphFace face = context.Graph.Faces[faceIndex];
                int cornerIndex = face.VertexIndices.IndexOf(vertexIndex);
                float weight = 1f;
                if (cornerIndex >= 0 && face.VertexIndices.Count >= 3)
                {
                    int previousIndex = face.VertexIndices[
                        (cornerIndex - 1 + face.VertexIndices.Count) %
                        face.VertexIndices.Count];
                    int nextIndex = face.VertexIndices[
                        (cornerIndex + 1) % face.VertexIndices.Count];
                    Vector3 previous =
                        context.Graph.Vertices[previousIndex].Position -
                        vertex.Position;
                    Vector3 next =
                        context.Graph.Vertices[nextIndex].Position -
                        vertex.Position;
                    if (previous.sqrMagnitude > MinimumEdgeLengthSqr &&
                        next.sqrMagnitude > MinimumEdgeLengthSqr)
                    {
                        weight = Mathf.Max(
                            0.01f,
                            Vector3.Angle(previous, next) * Mathf.Deg2Rad);
                    }
                }
                sum += face.SourceFace.Normal * weight;
            }
            return sum;
        }

        private static bool TryBuildPlaneCutVertexJunctionCandidate(
            List<PolygonFace> currentFaces,
            ChamferTopologyContext context,
            int vertexIndex,
            List<PlaneCutBevelCandidate> incident,
            Vector3 normal,
            int normalRank,
            bool isDirect,
            float depthFactor,
            float minimumStableEdgeLength,
            out PlaneCutVertexJunctionCandidate junction)
        {
            junction = default;
            float minimumWidth = float.PositiveInfinity;
            float maximumWidth = 0f;
            float strengthSum = 0f;
            for (int incidentIndex = 0;
                 incidentIndex < incident.Count;
                 incidentIndex++)
            {
                PlaneCutBevelCandidate edgeCandidate = incident[incidentIndex];
                minimumWidth = Mathf.Min(minimumWidth, edgeCandidate.Width);
                maximumWidth = Mathf.Max(maximumWidth, edgeCandidate.Width);
                strengthSum += edgeCandidate.Strength;
            }

            if (!IsFinite(normal) ||
                normal.sqrMagnitude <= MinimumEdgeLengthSqr ||
                float.IsNaN(minimumWidth) ||
                float.IsInfinity(minimumWidth) ||
                minimumWidth <= PointMergeDistance)
            {
                return false;
            }
            normal.Normalize();

            Vector3 sourceVertex =
                context.Graph.Vertices[vertexIndex].Position;
            float sourceSupport = Vector3.Dot(normal, sourceVertex);
            float unrelatedSupport = float.NegativeInfinity;
            for (int sourceVertexIndex = 0;
                 sourceVertexIndex < context.Graph.Vertices.Count;
                 sourceVertexIndex++)
            {
                if (sourceVertexIndex == vertexIndex)
                {
                    continue;
                }
                unrelatedSupport = Mathf.Max(
                    unrelatedSupport,
                    Vector3.Dot(
                        normal,
                        context.Graph.Vertices[sourceVertexIndex].Position));
            }

            float guardMargin = Mathf.Max(
                PointMergeDistance * 2f,
                minimumStableEdgeLength * 0.001f);
            if (float.IsNaN(unrelatedSupport) ||
                float.IsInfinity(unrelatedSupport) ||
                sourceSupport <= unrelatedSupport + guardMargin)
            {
                return false;
            }

            float currentSupport = GetCurrentSupport(currentFaces, normal);
            float targetCutback = Mathf.Max(
                minimumStableEdgeLength * 0.15f,
                minimumWidth * depthFactor);
            float minimumDistance = unrelatedSupport + guardMargin;
            float planeDistance = Mathf.Max(
                minimumDistance,
                currentSupport - targetCutback);
            float junctionRemoval = currentSupport - planeDistance;
            float minimumRemoval = Mathf.Max(
                PointMergeDistance * 2f,
                minimumStableEdgeLength * 0.005f);
            if (junctionRemoval <= minimumRemoval ||
                sourceSupport - planeDistance <= minimumRemoval)
            {
                return false;
            }

            float localRadius = Mathf.Max(
                minimumStableEdgeLength * 4f,
                Mathf.Max(maximumWidth * 5f, targetCutback * 3f));
            float localRadiusSqr = localRadius * localRadius;
            CutPlane plane = new CutPlane(normal, planeDistance);
            bool removesCurrentVertex = false;
            for (int faceIndex = 0;
                 faceIndex < currentFaces.Count;
                 faceIndex++)
            {
                List<Vector3> vertices = currentFaces[faceIndex].Vertices;
                for (int currentVertexIndex = 0;
                     currentVertexIndex < vertices.Count;
                     currentVertexIndex++)
                {
                    Vector3 currentVertex = vertices[currentVertexIndex];
                    if (plane.SignedDistance(currentVertex) <= minimumRemoval)
                    {
                        continue;
                    }
                    removesCurrentVertex = true;
                    if ((currentVertex - sourceVertex).sqrMagnitude >
                        localRadiusSqr)
                    {
                        return false;
                    }
                }
            }
            if (!removesCurrentVertex)
            {
                return false;
            }

            float planeTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                minimumStableEdgeLength * 0.02f);
            float clipEpsilon = Mathf.Min(
                PlaneEpsilon,
                Mathf.Max(
                    PointMergeDistance * 0.25f,
                    junctionRemoval * 0.25f));
            junction = new PlaneCutVertexJunctionCandidate(
                vertexIndex,
                plane,
                strengthSum / incident.Count,
                planeTolerance,
                clipEpsilon,
                localRadius,
                junctionRemoval,
                0f,
                0f,
                0,
                normalRank,
                isDirect);
            return true;
        }

        private static bool DoesPlaneCutJunctionJoinIncidentBevels(
            List<PolygonFace> faces,
            PlaneCutVertexJunctionCandidate junction,
            List<PlaneCutBevelCandidate> incident)
        {
            for (int incidentIndex = 0;
                 incidentIndex < incident.Count;
                 incidentIndex++)
            {
                PlaneCutBevelCandidate edge = incident[incidentIndex];
                bool joined = false;
                for (int faceIndex = 0;
                     faceIndex < faces.Count && !joined;
                     faceIndex++)
                {
                    PolygonFace face = faces[faceIndex];
                    if (face.Feature != PolygonFaceFeature.ConvexEdgeWear ||
                        Vector3.Dot(
                            face.Normal,
                            edge.Plane.Normal) < 0.999f)
                    {
                        continue;
                    }

                    bool onEdgePlane = true;
                    bool touchesJunction = false;
                    float tolerance = Mathf.Max(
                        edge.PlaneTolerance,
                        junction.PlaneTolerance);
                    for (int vertexIndex = 0;
                         vertexIndex < face.Vertices.Count;
                         vertexIndex++)
                    {
                        Vector3 vertex = face.Vertices[vertexIndex];
                        if (Mathf.Abs(edge.Plane.SignedDistance(vertex)) >
                            tolerance)
                        {
                            onEdgePlane = false;
                            break;
                        }
                        if (Mathf.Abs(
                                junction.Plane.SignedDistance(vertex)) <=
                            tolerance * 1.5f)
                        {
                            touchesJunction = true;
                        }
                    }
                    joined = onEdgePlane && touchesJunction;
                }
                if (!joined)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool TryFindSinglePlaneCutCap(
            List<PolygonFace> faces,
            CutPlane plane,
            float planeTolerance,
            out PolygonFace cap)
        {
            cap = null;
            int count = 0;
            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face.Feature != PolygonFaceFeature.ConvexEdgeWear ||
                    Vector3.Dot(face.Normal, plane.Normal) < 0.999f)
                {
                    continue;
                }

                bool onPlane = true;
                for (int vertexIndex = 0;
                     vertexIndex < face.Vertices.Count;
                     vertexIndex++)
                {
                    if (Mathf.Abs(plane.SignedDistance(
                            face.Vertices[vertexIndex])) > planeTolerance)
                    {
                        onPlane = false;
                        break;
                    }
                }
                if (!onPlane)
                {
                    continue;
                }
                count++;
                cap = face;
            }
            return count == 1;
        }

        private static bool IsStablePlaneCutVertexJunctionCap(
            PolygonFace cap,
            Vector3 sourceVertex,
            float localRadius,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            out float compactness,
            out float aspectRatio)
        {
            const float minimumCompactness = 0.06f;
            const float maximumAspectRatio = 12f;
            compactness = 0f;
            aspectRatio = float.PositiveInfinity;
            if (cap == null ||
                cap.Vertices.Count < 3 ||
                CalculatePolygonArea(cap.Vertices) <= minimumStableFaceArea)
            {
                return false;
            }

            float perimeter = 0f;
            float minimumEdge = float.PositiveInfinity;
            float maximumRadiusSqr = localRadius * localRadius * 2.25f;
            for (int vertexIndex = 0;
                 vertexIndex < cap.Vertices.Count;
                 vertexIndex++)
            {
                Vector3 start = cap.Vertices[vertexIndex];
                Vector3 end = cap.Vertices[
                    (vertexIndex + 1) % cap.Vertices.Count];
                float edgeLength = Vector3.Distance(start, end);
                perimeter += edgeLength;
                minimumEdge = Mathf.Min(minimumEdge, edgeLength);
                if ((start - sourceVertex).sqrMagnitude > maximumRadiusSqr)
                {
                    return false;
                }
            }

            float minimumCapEdge = Mathf.Max(
                PointMergeDistance * 2f,
                minimumStableEdgeLength * 0.05f);
            if (minimumEdge <= minimumCapEdge ||
                perimeter <= minimumCapEdge * 3f)
            {
                return false;
            }

            float area = CalculatePolygonArea(cap.Vertices);
            compactness = 4f * Mathf.PI * area /
                Mathf.Max(perimeter * perimeter, 0.0000000001f);
            aspectRatio = CalculatePlaneCutPolygonAspectRatio(
                cap.Vertices,
                area);
            return compactness >= minimumCompactness &&
                aspectRatio <= maximumAspectRatio;
        }

        private static float CalculatePlaneCutPolygonAspectRatio(
            List<Vector3> vertices,
            float area)
        {
            float maximumChordSqr = 0f;
            for (int leftIndex = 0;
                 leftIndex < vertices.Count;
                 leftIndex++)
            {
                for (int rightIndex = leftIndex + 1;
                     rightIndex < vertices.Count;
                     rightIndex++)
                {
                    maximumChordSqr = Mathf.Max(
                        maximumChordSqr,
                        (vertices[rightIndex] - vertices[leftIndex])
                            .sqrMagnitude);
                }
            }
            return maximumChordSqr /
                Mathf.Max(area, 0.0000000001f);
        }

        private static bool IsBetterPlaneCutVertexJunction(
            PlaneCutVertexJunctionCandidate candidate,
            PlaneCutVertexJunctionCandidate current)
        {
            if (candidate.AspectRatio <
                current.AspectRatio - 0.05f)
            {
                return true;
            }
            if (candidate.AspectRatio >
                current.AspectRatio + 0.05f)
            {
                return false;
            }
            if (candidate.Compactness >
                current.Compactness + 0.0025f)
            {
                return true;
            }
            if (candidate.Compactness <
                current.Compactness - 0.0025f)
            {
                return false;
            }
            if (candidate.CapVertexCount != current.CapVertexCount)
            {
                return candidate.CapVertexCount <
                    current.CapVertexCount;
            }
            if (candidate.CutDepth <
                current.CutDepth - PointMergeDistance)
            {
                return true;
            }
            if (candidate.CutDepth >
                current.CutDepth + PointMergeDistance)
            {
                return false;
            }
            return candidate.NormalRank < current.NormalRank;
        }

        private static Dictionary<int, List<PlaneCutBevelCandidate>>
            BuildPlaneCutIncidentMap(
                List<PlaneCutBevelCandidate> candidates)
        {
            Dictionary<int, List<PlaneCutBevelCandidate>> incidentByVertex =
                new Dictionary<int, List<PlaneCutBevelCandidate>>();
            for (int candidateIndex = 0;
                 candidateIndex < candidates.Count;
                 candidateIndex++)
            {
                PlaneCutBevelCandidate candidate = candidates[candidateIndex];
                AddPlaneCutIncidentCandidate(
                    incidentByVertex,
                    candidate.VertexA,
                    candidate);
                AddPlaneCutIncidentCandidate(
                    incidentByVertex,
                    candidate.VertexB,
                    candidate);
            }
            return incidentByVertex;
        }

        private static void AddPlaneCutIncidentCandidate(
            Dictionary<int, List<PlaneCutBevelCandidate>> incidentByVertex,
            int vertexIndex,
            PlaneCutBevelCandidate candidate)
        {
            if (!incidentByVertex.TryGetValue(
                    vertexIndex,
                    out List<PlaneCutBevelCandidate> incident))
            {
                incident = new List<PlaneCutBevelCandidate>();
                incidentByVertex.Add(vertexIndex, incident);
            }
            for (int incidentIndex = 0;
                 incidentIndex < incident.Count;
                 incidentIndex++)
            {
                if (incident[incidentIndex].SourceEdgeIndex ==
                    candidate.SourceEdgeIndex)
                {
                    return;
                }
            }
            incident.Add(candidate);
        }

        private static int CountPlaneCutJunctionCandidateVertices(
            ChamferTopologyContext context,
            List<PlaneCutBevelCandidate> candidates,
            HashSet<int> deferredSourceEdges)
        {
            int count = 0;
            Dictionary<int, List<PlaneCutBevelCandidate>> map =
                BuildPlaneCutIncidentMap(candidates);
            for (int vertexIndex = 0;
                 vertexIndex < context.Graph.Vertices.Count;
                 vertexIndex++)
            {
                if (!map.TryGetValue(
                        vertexIndex,
                        out List<PlaneCutBevelCandidate> incident))
                {
                    continue;
                }
                int activeCount = 0;
                for (int incidentIndex = 0;
                     incidentIndex < incident.Count;
                     incidentIndex++)
                {
                    if (deferredSourceEdges == null ||
                        !deferredSourceEdges.Contains(
                            incident[incidentIndex].SourceEdgeIndex))
                    {
                        activeCount++;
                    }
                }
                if (activeCount >= 2)
                {
                    count++;
                }
            }
            return count;
        }

        private static void PopulatePlaneCutJunctionResultCounts(
            ChamferTopologyContext context,
            List<PlaneCutBevelCandidate> initialEdges,
            PlaneCutJunctionSolveOutcome outcome,
            ref PlaneCutBevelAuditResult result)
        {
            Dictionary<int, List<PlaneCutBevelCandidate>> initialMap =
                BuildPlaneCutIncidentMap(initialEdges);
            Dictionary<int, List<PlaneCutBevelCandidate>> finalMap =
                BuildPlaneCutIncidentMap(outcome.ActiveEdges);
            Dictionary<int, PlaneCutVertexJunctionCandidate> junctionByVertex =
                new Dictionary<int, PlaneCutVertexJunctionCandidate>();
            for (int junctionIndex = 0;
                 junctionIndex < outcome.Junctions.Count;
                 junctionIndex++)
            {
                PlaneCutVertexJunctionCandidate junction =
                    outcome.Junctions[junctionIndex];
                junctionByVertex[junction.VertexIndex] = junction;
                bool backtracked = false;
                if (initialMap.TryGetValue(
                        junction.VertexIndex,
                        out List<PlaneCutBevelCandidate> originalIncident))
                {
                    for (int incidentIndex = 0;
                         incidentIndex < originalIncident.Count;
                         incidentIndex++)
                    {
                        if (outcome.DeferredSourceEdges.Contains(
                                originalIncident[incidentIndex].SourceEdgeIndex))
                        {
                            backtracked = true;
                            break;
                        }
                    }
                }

                if (backtracked)
                {
                    result.VertexJunctionBacktrackBuiltCount++;
                }
                else if (junction.IsDirect)
                {
                    result.VertexJunctionDirectBuiltCount++;
                }
                else
                {
                    result.VertexJunctionAdaptiveBuiltCount++;
                }

                if (junction.CapVertexCount == 3)
                {
                    result.VertexJunctionTriangleCapCount++;
                }
                else if (junction.CapVertexCount == 4)
                {
                    result.VertexJunctionQuadCapCount++;
                }
                else
                {
                    result.VertexJunctionLargerCapCount++;
                }
            }

            for (int vertexIndex = 0;
                 vertexIndex < context.Graph.Vertices.Count;
                 vertexIndex++)
            {
                if (!initialMap.TryGetValue(
                        vertexIndex,
                        out List<PlaneCutBevelCandidate> initialIncident) ||
                    initialIncident.Count < 2)
                {
                    continue;
                }
                int finalCount = finalMap.TryGetValue(
                    vertexIndex,
                    out List<PlaneCutBevelCandidate> finalIncident)
                    ? finalIncident.Count
                    : 0;
                if (finalCount < 2 &&
                    !junctionByVertex.ContainsKey(vertexIndex))
                {
                    result.VertexJunctionCleanSharpCount++;
                }
            }
        }

        private static int CountLocalizedPlaneCutCandidates(
            List<PlaneCutBevelCandidate> candidates)
        {
            int count = 0;
            for (int candidateIndex = 0;
                 candidateIndex < candidates.Count;
                 candidateIndex++)
            {
                if (candidates[candidateIndex].WasLocalized)
                {
                    count++;
                }
            }
            return count;
        }

        private static List<PlaneCutBevelCandidate>
            GetActivePlaneCutIncidentCandidates(
                List<PlaneCutBevelCandidate> candidates,
                int vertexIndex)
        {
            List<PlaneCutBevelCandidate> incident =
                new List<PlaneCutBevelCandidate>();
            for (int candidateIndex = 0;
                 candidateIndex < candidates.Count;
                 candidateIndex++)
            {
                PlaneCutBevelCandidate candidate = candidates[candidateIndex];
                if (candidate.VertexA == vertexIndex ||
                    candidate.VertexB == vertexIndex)
                {
                    incident.Add(candidate);
                }
            }
            return incident;
        }

        private static int ComparePlaneCutBacktrackCandidates(
            PlaneCutBevelCandidate left,
            PlaneCutBevelCandidate right,
            ChamferTopologyContext context)
        {
            if (left.WasLocalized != right.WasLocalized)
            {
                return left.WasLocalized ? -1 : 1;
            }
            int strength = left.Strength.CompareTo(right.Strength);
            if (strength != 0)
            {
                return strength;
            }
            int selection = left.SelectionScore.CompareTo(
                right.SelectionScore);
            if (selection != 0)
            {
                return selection;
            }
            int width = left.Width.CompareTo(right.Width);
            if (width != 0)
            {
                return width;
            }
            float leftLength = CalculatePlaneCutSourceEdgeLength(
                left,
                context);
            float rightLength = CalculatePlaneCutSourceEdgeLength(
                right,
                context);
            int length = leftLength.CompareTo(rightLength);
            if (length != 0)
            {
                return length;
            }
            return left.SourceEdgeIndex.CompareTo(right.SourceEdgeIndex);
        }

        private static float CalculatePlaneCutSourceEdgeLength(
            PlaneCutBevelCandidate candidate,
            ChamferTopologyContext context)
        {
            if (candidate.VertexA < 0 ||
                candidate.VertexA >= context.Graph.Vertices.Count ||
                candidate.VertexB < 0 ||
                candidate.VertexB >= context.Graph.Vertices.Count)
            {
                return 0f;
            }
            return Vector3.Distance(
                context.Graph.Vertices[candidate.VertexA].Position,
                context.Graph.Vertices[candidate.VertexB].Position);
        }

        private static List<int> AddPlaneCutDeferredSourceEdge(
            List<int> existing,
            int sourceEdgeIndex)
        {
            List<int> result = new List<int>(existing);
            if (!result.Contains(sourceEdgeIndex))
            {
                result.Add(sourceEdgeIndex);
                result.Sort();
            }
            return result;
        }

        private static string BuildPlaneCutDeferredSetKey(
            List<int> deferred)
        {
            if (deferred.Count == 0)
            {
                return string.Empty;
            }
            System.Text.StringBuilder builder =
                new System.Text.StringBuilder(deferred.Count * 5);
            for (int index = 0; index < deferred.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append(',');
                }
                builder.Append(deferred[index]);
            }
            return builder.ToString();
        }

        private static bool IsBetterPlaneCutPartialOutcome(
            PlaneCutJunctionSolveOutcome candidate,
            PlaneCutJunctionSolveOutcome current)
        {
            if (candidate == null)
            {
                return false;
            }
            if (current == null)
            {
                return true;
            }
            if (candidate.HardFailure != current.HardFailure)
            {
                return !candidate.HardFailure;
            }
            if (candidate.UnresolvedVertices.Count !=
                current.UnresolvedVertices.Count)
            {
                return candidate.UnresolvedVertices.Count <
                    current.UnresolvedVertices.Count;
            }
            if (candidate.DeferredSourceEdges.Count !=
                current.DeferredSourceEdges.Count)
            {
                return candidate.DeferredSourceEdges.Count <
                    current.DeferredSourceEdges.Count;
            }
            return candidate.QualityScore > current.QualityScore;
        }


        #endregion
    }
}
