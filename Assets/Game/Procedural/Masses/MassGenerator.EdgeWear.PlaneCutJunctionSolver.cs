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
            if (!TryFindSinglePlaneCutConnectorFace(
                    faces,
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
                        !TryFindSinglePlaneCutConnectorFace(
                            faces,
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
                    if ((adjacent.ProvenanceKind ==
                             PolygonFaceProvenanceKind.VertexJunctionPlane ||
                         adjacent.ProvenanceKind ==
                             PolygonFaceProvenanceKind.BoundedEndpointCap) &&
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
            PlaneCutBevelAuditResult qualityAudit =
                new PlaneCutBevelAuditResult();
            if (!TryPreparePlaneCutPreviewFaces(
                    trialClone,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    ref qualityAudit,
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

        private static bool TryFindSinglePlaneCutConnectorFace(
            List<PolygonFace> faces,
            int vertexIndex,
            out PolygonFace connectorFace)
        {
            connectorFace = null;
            int count = 0;
            if (faces == null || vertexIndex < 0)
            {
                return false;
            }
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face == null ||
                    face.ProvenanceIndex != vertexIndex ||
                    (face.ProvenanceKind !=
                         PolygonFaceProvenanceKind.VertexJunctionPlane &&
                     face.ProvenanceKind !=
                         PolygonFaceProvenanceKind.BoundedEndpointCap))
                {
                    continue;
                }
                connectorFace = face;
                count++;
            }
            return count == 1;
        }

        private static bool TryBuildPlaneCutSystemFaces(
            List<PolygonFace> sourceFaces,
            List<PlaneCutBevelCandidate> activeEdges,
            List<PlaneCutVertexJunctionCandidate> junctions,
            out List<PolygonFace> faces,
            out int edgeCapsBuilt,
            out string blocker,
            PlaneCutNumericalRepairTelemetry numericalRepairs = null,
            PlaneCutEndpointPatchReplacement endpointPatch = null)
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

                int exactFailureCountBefore = numericalRepairs == null
                    ? 0
                    : numericalRepairs.ExactConstructionFailureCount;
                ClipPolyhedron(
                    faces,
                    candidate.Plane,
                    PolygonFaceFeature.ConvexEdgeWear,
                    candidate.Strength,
                    true,
                    candidate.ClipEpsilon,
                    true,
                    PolygonFaceProvenanceKind.EdgeBevelPlane,
                    candidate.SourceEdgeIndex,
                    numericalRepairs != null,
                    numericalRepairs != null,
                    numericalRepairs);
                if (numericalRepairs != null &&
                    numericalRepairs.ExactConstructionFailureCount >
                        exactFailureCountBefore)
                {
                    blocker =
                        "strict plane-classification or intersection invariant failed";
                    return false;
                }
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

            if (endpointPatch != null)
            {
                if (!TryApplyPlaneCutEndpointPatchReplacement(
                        faces,
                        endpointPatch,
                        out blocker))
                {
                    return false;
                }
                return true;
            }

            for (int junctionIndex = 0;
                 junctionIndex < junctions.Count;
                 junctionIndex++)
            {
                PlaneCutVertexJunctionCandidate junction =
                    junctions[junctionIndex];
                int exactFailureCountBefore = numericalRepairs == null
                    ? 0
                    : numericalRepairs.ExactConstructionFailureCount;
                ClipPolyhedron(
                    faces,
                    junction.Plane,
                    PolygonFaceFeature.ConvexEdgeWear,
                    junction.Strength,
                    true,
                    junction.ClipEpsilon,
                    true,
                    PolygonFaceProvenanceKind.VertexJunctionPlane,
                    junction.VertexIndex,
                    numericalRepairs != null,
                    numericalRepairs != null,
                    numericalRepairs);
                if (numericalRepairs != null &&
                    numericalRepairs.ExactConstructionFailureCount >
                        exactFailureCountBefore)
                {
                    blocker =
                        "strict vertex-junction clipping invariant failed";
                    return false;
                }
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

        private static bool TryPrepareCornerDamageEndpointPatchRecovery(
            CornerDamageIntegrationPlan plan,
            List<PlaneCutBevelCandidate> preparedCandidates,
            List<PlaneCutBevelCandidate> minimumCandidates,
            PlaneCutBevelCandidate victim,
            PlaneCutBevelCandidate foreign,
            int conflictVertexIndex)
        {
            if (plan == null)
            {
                return false;
            }

            System.Diagnostics.Stopwatch stopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            ResetEndpointPatchRecoveryAttempt(
                plan,
                victim.SourceEdgeIndex,
                foreign.SourceEdgeIndex);
            try
            {
                PlaneCutBevelSolvedPlan solvedPlan = plan.SolvedPlan;
                if (solvedPlan == null || solvedPlan.Context == null ||
                    solvedPlan.Context.Graph == null ||
                    solvedPlan.SourceFaces == null ||
                    preparedCandidates == null ||
                    minimumCandidates == null ||
                    preparedCandidates.Count < 2 ||
                    minimumCandidates.Count < 2 ||
                    conflictVertexIndex < 0 ||
                    conflictVertexIndex >=
                        solvedPlan.Context.Graph.Vertices.Count)
                {
                    RecordEndpointPatchRecoveryRejection(
                        plan,
                        PlaneCutEndpointPatchRejectionKind.PatchExtraction,
                        "bounded endpoint-patch recovery was unavailable because prepared topology was incomplete");
                    return false;
                }

                int sharedVertexIndex = ResolvePlaneCutEndpointPatchVertex(
                    victim,
                    foreign);
                if (sharedVertexIndex < 0 ||
                    sharedVertexIndex != conflictVertexIndex)
                {
                    RecordEndpointPatchRecoveryRejection(
                        plan,
                        PlaneCutEndpointPatchRejectionKind.UnsupportedStar,
                        "bounded endpoint-patch recovery requires the victim and foreign bands to share the implicated endpoint");
                    return false;
                }

                List<PlaneCutBevelCandidate> preparedIncident =
                    GetActivePlaneCutIncidentCandidates(
                        preparedCandidates,
                        sharedVertexIndex);
                List<PlaneCutBevelCandidate> minimumIncident =
                    GetActivePlaneCutIncidentCandidates(
                        minimumCandidates,
                        sharedVertexIndex);
                plan.EndpointPatchRecoveryIncidentBandCount =
                    preparedIncident.Count;
                if (!IsSupportedEndpointPatchRecoverySet(
                        preparedIncident,
                        victim.SourceEdgeIndex,
                        foreign.SourceEdgeIndex) ||
                    !AreMatchingEndpointPatchRecoverySets(
                        preparedIncident,
                        minimumIncident))
                {
                    RecordEndpointPatchRecoveryRejection(
                        plan,
                        PlaneCutEndpointPatchRejectionKind.UnsupportedStar,
                        "bounded endpoint-patch recovery supports one complete retained source-vertex star containing two or three bevel bands");
                    return false;
                }

                List<PlaneCutVertexJunctionCandidate> noJunctions =
                    new List<PlaneCutVertexJunctionCandidate>();
                PlaneCutNumericalRepairTelemetry preparedRepairs =
                    new PlaneCutNumericalRepairTelemetry();
                if (!TryBuildPlaneCutSystemFaces(
                        solvedPlan.SourceFaces,
                        preparedCandidates,
                        noJunctions,
                        out List<PolygonFace> preparedEdgeOnlyFaces,
                        out _,
                        out string preparedShellBlocker,
                        preparedRepairs))
                {
                    RecordEndpointPatchRecoveryRejection(
                        plan,
                        PlaneCutEndpointPatchRejectionKind.PatchExtraction,
                        "prepared-width edge-only shell was unavailable: " +
                        preparedShellBlocker);
                    return false;
                }
                PlaneCutNumericalRepairTelemetry minimumRepairs =
                    new PlaneCutNumericalRepairTelemetry();
                if (!TryBuildPlaneCutSystemFaces(
                        solvedPlan.SourceFaces,
                        minimumCandidates,
                        noJunctions,
                        out List<PolygonFace> minimumEdgeOnlyFaces,
                        out _,
                        out string minimumShellBlocker,
                        minimumRepairs))
                {
                    RecordEndpointPatchRecoveryRejection(
                        plan,
                        PlaneCutEndpointPatchRejectionKind.PatchExtraction,
                        "legal-minimum edge-only shell was unavailable: " +
                        minimumShellBlocker);
                    return false;
                }

                ChamferTopologyContext context = solvedPlan.Context;
                List<PlaneCutJunctionNormalOption> normalOptions =
                    BuildPlaneCutJunctionNormalOptions(
                        context,
                        sharedVertexIndex,
                        preparedIncident);
                if (normalOptions.Count == 0)
                {
                    RecordEndpointPatchRecoveryRejection(
                        plan,
                        PlaneCutEndpointPatchRejectionKind.NoLocalRemoval,
                        "bounded endpoint-patch recovery produced no finite local boundary normal");
                    return false;
                }

                float[] depthFactors = { 0.60f, 0.90f };
                int normalLimit = Mathf.Min(3, normalOptions.Count);
                PlaneCutEndpointPatchRejectionKind lastRejection =
                    PlaneCutEndpointPatchRejectionKind.NoLocalRemoval;
                string lastBlocker = string.Empty;
                for (int normalIndex = 0;
                     normalIndex < normalLimit;
                     normalIndex++)
                {
                    PlaneCutJunctionNormalOption option =
                        normalOptions[normalIndex];
                    plan.EndpointPatchRecoveryNormalRank = option.Rank;
                    for (int depthIndex = 0;
                         depthIndex < depthFactors.Length;
                         depthIndex++)
                    {
                        plan.EndpointPatchRecoveryTrialCount++;
                        if (!TryBuildPlaneCutEndpointPatchCandidate(
                                preparedEdgeOnlyFaces,
                                context,
                                sharedVertexIndex,
                                preparedIncident,
                                option.Normal,
                                option.Rank,
                                option.IsDirect,
                                depthFactors[depthIndex],
                                solvedPlan.MinimumStableEdgeLength,
                                out PlaneCutVertexJunctionCandidate candidate,
                                out PlaneCutEndpointPatchSupportEvidence supportEvidence,
                                out lastRejection,
                                out lastBlocker))
                        {
                            ApplyEndpointPatchSupportEvidence(
                                plan,
                                supportEvidence);
                            continue;
                        }
                        ApplyEndpointPatchSupportEvidence(
                            plan,
                            supportEvidence);

                        if (!TryBuildPlaneCutEndpointPatchReplacement(
                                preparedEdgeOnlyFaces,
                                solvedPlan.SourceFaces,
                                context,
                                preparedIncident,
                                candidate,
                                solvedPlan.MinimumStableEdgeLength,
                                solvedPlan.MinimumStableFaceArea,
                                null,
                                out PlaneCutEndpointPatchReplacement preparedPatch,
                                out _,
                                out PlaneCutEndpointPatchLocalityEvidence preparedLocality,
                                out PlaneCutEndpointPatchAxialEvidence preparedAxial,
                                out PlaneCutEndpointCellEvidence preparedCellEvidence,
                                out lastRejection,
                                out lastBlocker))
                        {
                            ApplyEndpointPatchLocalityEvidence(
                                plan,
                                preparedLocality);
                            ApplyEndpointPatchAxialEvidence(
                                plan,
                                preparedAxial);
                            ApplyEndpointCellEvidence(
                                plan,
                                preparedCellEvidence);
                            continue;
                        }
                        ApplyEndpointPatchLocalityEvidence(
                            plan,
                            preparedLocality);
                        ApplyEndpointPatchAxialEvidence(
                            plan,
                            preparedAxial);
                        ApplyEndpointCellEvidence(
                            plan,
                            preparedCellEvidence);
                        if (!TryBuildPlaneCutEndpointPatchReplacement(
                                minimumEdgeOnlyFaces,
                                solvedPlan.SourceFaces,
                                context,
                                minimumIncident,
                                candidate,
                                solvedPlan.MinimumStableEdgeLength,
                                solvedPlan.MinimumStableFaceArea,
                                preparedPatch.CellLimits,
                                out PlaneCutEndpointPatchReplacement minimumPatch,
                                out _,
                                out PlaneCutEndpointPatchLocalityEvidence minimumLocality,
                                out PlaneCutEndpointPatchAxialEvidence minimumAxial,
                                out PlaneCutEndpointCellEvidence minimumCellEvidence,
                                out lastRejection,
                                out lastBlocker))
                        {
                            ApplyEndpointPatchLocalityEvidence(
                                plan,
                                minimumLocality);
                            ApplyEndpointPatchAxialEvidence(
                                plan,
                                minimumAxial);
                            ApplyEndpointCellEvidence(
                                plan,
                                minimumCellEvidence);
                            lastBlocker =
                                "legal-minimum endpoint patch rejected: " +
                                lastBlocker;
                            continue;
                        }
                        if (!DoPlaneCutEndpointPatchReplacementsMatch(
                                preparedPatch,
                                minimumPatch))
                        {
                            lastRejection =
                                PlaneCutEndpointPatchRejectionKind.PreparedMinimumParity;
                            lastBlocker =
                                "prepared and legal-minimum endpoint patches produced different selected-face or stitch-boundary topology";
                            continue;
                        }

                        PlaneCutVertexJunctionCandidate certified =
                            new PlaneCutVertexJunctionCandidate(
                                candidate.VertexIndex,
                                candidate.Plane,
                                candidate.Strength,
                                candidate.PlaneTolerance,
                                candidate.ClipEpsilon,
                                candidate.LocalRadius,
                                candidate.CutDepth,
                                Mathf.Min(
                                    preparedPatch.Compactness,
                                    minimumPatch.Compactness),
                                Mathf.Max(
                                    preparedPatch.AspectRatio,
                                    minimumPatch.AspectRatio),
                                Mathf.Max(
                                    preparedPatch.CapVertexCount,
                                    minimumPatch.CapVertexCount),
                                candidate.NormalRank,
                                candidate.IsDirect);
                        solvedPlan.PreparedJunctions ??=
                            new List<PlaneCutVertexJunctionCandidate>();
                        solvedPlan.PreparedJunctions.Clear();
                        solvedPlan.PreparedJunctions.Add(certified);
                        solvedPlan.PreparedEndpointPatch = preparedPatch;
                        plan.EndpointPatchRecoveryPrepared = true;
                        plan.EndpointPatchRecoveryRejection =
                            PlaneCutEndpointPatchRejectionKind.None;
                        plan.EndpointPatchRecoveryVertexIndex =
                            sharedVertexIndex;
                        plan.EndpointPatchRecoveryNormalRank =
                            certified.NormalRank;
                        plan.EndpointPatchRecoveryCapVertexCount =
                            certified.CapVertexCount;
                        plan.EndpointPatchRecoveryCutDepth =
                            certified.CutDepth;
                        plan.EndpointPatchRecoveryCompactness =
                            certified.Compactness;
                        plan.EndpointPatchRecoveryAspectRatio =
                            certified.AspectRatio;
                        plan.EndpointPatchRecoverySelectedFaceCount =
                            preparedPatch.SelectedFaceCount;
                        plan.EndpointPatchRecoveryBoundaryVertexCount =
                            preparedPatch.BoundaryVertexCount;
                        plan.EndpointPatchRecoveryBoundarySignature =
                            preparedPatch.BoundaryTopologySignature;
                        plan.EndpointPatchRecoveryMaximumRemovedVertexRadius =
                            preparedPatch.MaximumRemovedVertexRadius;
                        plan.EndpointPatchRecoveryMaximumIntersectionRadius =
                            preparedPatch.MaximumIntersectionRadius;
                        plan.EndpointPatchRecoveryMaximumReplacementVertexRadius =
                            preparedPatch.MaximumReplacementVertexRadius;
                        plan.EndpointPatchRecoveryRetainedOutsideRadiusCount =
                            preparedPatch.RetainedOutsideRadiusCount;
                        plan.EndpointPatchRecoverySelectedFaceCountBeforeLocalFilter =
                            preparedPatch.SelectedFaceCountBeforeLocalFilter;
                        plan.EndpointPatchRecoverySelectedFaceCountAfterLocalFilter =
                            preparedPatch.SelectedFaceCountAfterLocalFilter;
                        plan.EndpointPatchRecoveryLocalityFailureSource =
                            string.Empty;
                        plan.EndpointPatchRecoveryMaximumAxialInfluence =
                            preparedPatch.MaximumAxialInfluence;
                        plan.EndpointPatchRecoveryMinimumAllowedAxialInfluence =
                            preparedPatch.MinimumAllowedAxialInfluence;
                        plan.EndpointPatchRecoveryAxialInfluenceSignature =
                            preparedPatch.AxialInfluenceSignature;
                        plan.EndpointPatchRecoveryDiagnostic =
                            "bounded endpoint-cell " +
                            preparedIncident.Count +
                            "-band subface reconstruction certified at prepared and legal-minimum widths";
                        return true;
                    }
                }

                RecordEndpointPatchRecoveryRejection(
                    plan,
                    lastRejection,
                    string.IsNullOrEmpty(lastBlocker)
                        ? "no bounded local endpoint face patch passed exact dual-width certification"
                        : lastBlocker);
                return false;
            }
            finally
            {
                stopwatch.Stop();
                plan.EndpointPatchRecoveryMilliseconds =
                    stopwatch.Elapsed.TotalMilliseconds;
            }
        }

        private static void ResetEndpointPatchRecoveryAttempt(
            CornerDamageIntegrationPlan plan,
            int victimEdgeIndex,
            int foreignEdgeIndex)
        {
            plan.EndpointPatchRecoveryAttempted = true;
            plan.EndpointPatchRecoveryAttemptCount++;
            plan.EndpointPatchRecoveryPrepared = false;
            plan.EndpointPatchRecoveryApplied = false;
            plan.EndpointPatchRecoveryFalsePositive = false;
            plan.EndpointPatchRecoveryVertexIndex = -1;
            plan.EndpointPatchRecoveryVictimEdgeIndex = victimEdgeIndex;
            plan.EndpointPatchRecoveryForeignEdgeIndex = foreignEdgeIndex;
            plan.EndpointPatchRecoveryIncidentBandCount = 0;
            plan.EndpointPatchRecoveryNormalRank = -1;
            plan.EndpointPatchRecoveryCapVertexCount = 0;
            plan.EndpointPatchRecoveryCutDepth = 0f;
            plan.EndpointPatchRecoveryCompactness = 0f;
            plan.EndpointPatchRecoveryAspectRatio = 0f;
            plan.EndpointPatchRecoveryRejection =
                PlaneCutEndpointPatchRejectionKind.None;
            plan.EndpointPatchRecoverySelectedFaceCount = 0;
            plan.EndpointPatchRecoveryBoundaryVertexCount = 0;
            plan.EndpointPatchRecoveryBoundarySignature = string.Empty;
            plan.EndpointPatchRecoveryMaximumRemovedVertexRadius = 0f;
            plan.EndpointPatchRecoveryMaximumIntersectionRadius = 0f;
            plan.EndpointPatchRecoveryMaximumReplacementVertexRadius = 0f;
            plan.EndpointPatchRecoveryRetainedOutsideRadiusCount = 0;
            plan.EndpointPatchRecoverySelectedFaceCountBeforeLocalFilter = 0;
            plan.EndpointPatchRecoverySelectedFaceCountAfterLocalFilter = 0;
            plan.EndpointPatchRecoveryLocalityFailureSource = string.Empty;
            plan.EndpointPatchRecoveryLocalSupportSampleCount = 0;
            plan.EndpointPatchRecoveryMinimumSamplesPerIncident = 0;
            plan.EndpointPatchRecoverySamplesPerIncident = string.Empty;
            plan.EndpointPatchRecoveryLocalSupportRadius = 0f;
            plan.EndpointPatchRecoveryLocalSupportProjection = 0f;
            plan.EndpointPatchRecoveryGlobalSupportProjection = 0f;
            plan.EndpointPatchRecoveryGlobalMinusLocalSupportDelta = 0f;
            plan.EndpointPatchRecoveryControllingSupportEdgeIndex = -1;
            plan.EndpointPatchRecoveryControllingSupportRadius = 0f;
            plan.EndpointPatchRecoverySupportFailureSource = string.Empty;
            plan.EndpointPatchRecoveryMaximumAxialInfluence = 0f;
            plan.EndpointPatchRecoveryMinimumAllowedAxialInfluence = 0f;
            plan.EndpointPatchRecoveryAxialRejectedEdgeIndex = -1;
            plan.EndpointPatchRecoveryAxialRejectedEndpointVertexIndex = -1;
            plan.EndpointPatchRecoveryAxialInfluenceSignature = string.Empty;
            plan.EndpointPatchRecoveryCellLimitSignature = string.Empty;
            plan.EndpointPatchRecoveryFacesSubdivided = 0;
            plan.EndpointPatchRecoveryLocalFragmentCount = 0;
            plan.EndpointPatchRecoveryRemoteRemainderCount = 0;
            plan.EndpointPatchRecoverySyntheticIncidentFragmentCount = 0;
            plan.EndpointPatchRecoverySyntheticIncidentIdentities = string.Empty;
            plan.EndpointPatchRecoveryCellVertexCount = 0;
            plan.EndpointPatchRecoveryCellFaceCount = 0;
            plan.EndpointPatchRecoveryCellSplitSignature = string.Empty;
            plan.EndpointPatchRecoveryLocalFragmentSignature = string.Empty;
            plan.EndpointPatchRecoveryRemoteRemainderSignature = string.Empty;
            plan.EndpointPatchRecoveryCellFailureSource = string.Empty;
            plan.EndpointPatchRecoveryDiagnostic = string.Empty;
        }

        private static void ApplyEndpointPatchLocalityEvidence(
            CornerDamageIntegrationPlan plan,
            PlaneCutEndpointPatchLocalityEvidence evidence)
        {
            if (plan == null || evidence == null)
            {
                return;
            }
            plan.EndpointPatchRecoveryMaximumRemovedVertexRadius =
                evidence.MaximumRemovedVertexRadius;
            plan.EndpointPatchRecoveryMaximumIntersectionRadius =
                evidence.MaximumIntersectionRadius;
            plan.EndpointPatchRecoveryMaximumReplacementVertexRadius =
                evidence.MaximumReplacementVertexRadius;
            plan.EndpointPatchRecoveryRetainedOutsideRadiusCount =
                evidence.RetainedOutsideRadiusCount;
            plan.EndpointPatchRecoverySelectedFaceCountBeforeLocalFilter =
                evidence.SelectedFaceCountBeforeLocalFilter;
            plan.EndpointPatchRecoverySelectedFaceCountAfterLocalFilter =
                evidence.SelectedFaceCountAfterLocalFilter;
            plan.EndpointPatchRecoveryLocalityFailureSource =
                evidence.FailureSource ?? string.Empty;
        }

        private static void ApplyEndpointPatchSupportEvidence(
            CornerDamageIntegrationPlan plan,
            PlaneCutEndpointPatchSupportEvidence evidence)
        {
            if (plan == null || evidence == null)
            {
                return;
            }
            plan.EndpointPatchRecoveryLocalSupportSampleCount =
                evidence.TotalSampleCount;
            plan.EndpointPatchRecoveryMinimumSamplesPerIncident =
                evidence.MinimumSamplesPerIncident;
            plan.EndpointPatchRecoverySamplesPerIncident =
                evidence.SamplesPerIncident ?? string.Empty;
            plan.EndpointPatchRecoveryLocalSupportRadius =
                evidence.LocalSupportRadius;
            plan.EndpointPatchRecoveryLocalSupportProjection =
                evidence.LocalSupportProjection;
            plan.EndpointPatchRecoveryGlobalSupportProjection =
                evidence.GlobalSupportProjection;
            plan.EndpointPatchRecoveryGlobalMinusLocalSupportDelta =
                evidence.GlobalMinusLocalSupportDelta;
            plan.EndpointPatchRecoveryControllingSupportEdgeIndex =
                evidence.ControllingSourceEdgeIndex;
            plan.EndpointPatchRecoveryControllingSupportRadius =
                evidence.ControllingSupportRadius;
            plan.EndpointPatchRecoverySupportFailureSource =
                evidence.FailureSource ?? string.Empty;
        }

        private static void ApplyEndpointPatchAxialEvidence(
            CornerDamageIntegrationPlan plan,
            PlaneCutEndpointPatchAxialEvidence evidence)
        {
            if (plan == null || evidence == null)
            {
                return;
            }
            plan.EndpointPatchRecoveryMaximumAxialInfluence =
                evidence.MaximumInfluence;
            plan.EndpointPatchRecoveryMinimumAllowedAxialInfluence =
                float.IsInfinity(evidence.MinimumAllowedInfluence)
                    ? 0f
                    : evidence.MinimumAllowedInfluence;
            plan.EndpointPatchRecoveryAxialRejectedEdgeIndex =
                evidence.RejectedSourceEdgeIndex;
            plan.EndpointPatchRecoveryAxialRejectedEndpointVertexIndex =
                evidence.RejectedEndpointVertexIndex;
            plan.EndpointPatchRecoveryAxialInfluenceSignature =
                evidence.InfluenceSignature ?? string.Empty;
        }

        private static void ApplyEndpointCellEvidence(
            CornerDamageIntegrationPlan plan,
            PlaneCutEndpointCellEvidence evidence)
        {
            if (plan == null || evidence == null)
            {
                return;
            }
            plan.EndpointPatchRecoveryCellLimitSignature =
                evidence.CellLimitSignature ?? string.Empty;
            plan.EndpointPatchRecoveryFacesSubdivided =
                evidence.FacesSubdivided;
            plan.EndpointPatchRecoveryLocalFragmentCount =
                evidence.LocalFragmentCount;
            plan.EndpointPatchRecoveryRemoteRemainderCount =
                evidence.RemoteRemainderCount;
            plan.EndpointPatchRecoverySyntheticIncidentFragmentCount =
                evidence.SyntheticIncidentFragmentCount;
            plan.EndpointPatchRecoverySyntheticIncidentIdentities =
                evidence.SyntheticIncidentIdentities ?? string.Empty;
            plan.EndpointPatchRecoveryCellVertexCount =
                evidence.CellVertexCount;
            plan.EndpointPatchRecoveryCellFaceCount =
                evidence.CellFaceCount;
            plan.EndpointPatchRecoveryCellSplitSignature =
                evidence.CellSplitSignature ?? string.Empty;
            plan.EndpointPatchRecoveryLocalFragmentSignature =
                evidence.LocalFragmentSignature ?? string.Empty;
            plan.EndpointPatchRecoveryRemoteRemainderSignature =
                evidence.RemoteRemainderSignature ?? string.Empty;
            plan.EndpointPatchRecoveryCellFailureSource =
                evidence.FailureSource ?? string.Empty;
        }

        private static void RecordEndpointPatchRecoveryRejection(
            CornerDamageIntegrationPlan plan,
            PlaneCutEndpointPatchRejectionKind rejection,
            string diagnostic)
        {
            if (plan == null)
            {
                return;
            }
            plan.EndpointPatchRecoveryRejection = rejection;
            plan.EndpointPatchRecoveryDiagnostic = diagnostic ?? string.Empty;
            switch (rejection)
            {
                case PlaneCutEndpointPatchRejectionKind.UnsupportedStar:
                    plan.EndpointPatchRecoveryUnsupportedStarCount++;
                    break;
                case PlaneCutEndpointPatchRejectionKind.PatchExtraction:
                    plan.EndpointPatchRecoveryPatchExtractionCount++;
                    break;
                case PlaneCutEndpointPatchRejectionKind.DisconnectedPatch:
                    plan.EndpointPatchRecoveryDisconnectedPatchCount++;
                    break;
                case PlaneCutEndpointPatchRejectionKind.BoundaryLoop:
                    plan.EndpointPatchRecoveryBoundaryLoopCount++;
                    break;
                case PlaneCutEndpointPatchRejectionKind.BoundaryCrossing:
                    plan.EndpointPatchRecoveryBoundaryCrossingCount++;
                    break;
                case PlaneCutEndpointPatchRejectionKind.NoLocalRemoval:
                    plan.EndpointPatchRecoveryNoLocalRemovalCount++;
                    break;
                case PlaneCutEndpointPatchRejectionKind.CapCreation:
                    plan.EndpointPatchRecoveryCapCreationCount++;
                    break;
                case PlaneCutEndpointPatchRejectionKind.IncidentBandJoin:
                    plan.EndpointPatchRecoveryIncidentBandJoinCount++;
                    break;
                case PlaneCutEndpointPatchRejectionKind.StitchTopology:
                    plan.EndpointPatchRecoveryStitchTopologyCount++;
                    break;
                case PlaneCutEndpointPatchRejectionKind.Locality:
                    plan.EndpointPatchRecoveryLocalityCount++;
                    break;
                case PlaneCutEndpointPatchRejectionKind.BandIntegrity:
                    plan.EndpointPatchRecoveryBandIntegrityCount++;
                    break;
                case PlaneCutEndpointPatchRejectionKind.PreparedMinimumParity:
                    plan.EndpointPatchRecoveryPreparedMinimumParityCount++;
                    break;
                case PlaneCutEndpointPatchRejectionKind.MaterializationSignature:
                    plan.EndpointPatchRecoveryMaterializationSignatureCount++;
                    break;
            }
        }

        private static int ResolvePlaneCutEndpointPatchVertex(
            PlaneCutBevelCandidate first,
            PlaneCutBevelCandidate second)
        {
            int shared = -1;
            if (first.VertexA == second.VertexA ||
                first.VertexA == second.VertexB)
            {
                shared = first.VertexA;
            }
            if (first.VertexB == second.VertexA ||
                first.VertexB == second.VertexB)
            {
                if (shared >= 0 && shared != first.VertexB)
                {
                    return -1;
                }
                shared = first.VertexB;
            }
            return shared;
        }

        private static bool IsSupportedEndpointPatchRecoverySet(
            List<PlaneCutBevelCandidate> incident,
            int victimEdgeIndex,
            int foreignEdgeIndex)
        {
            if (incident == null ||
                incident.Count < 2 ||
                incident.Count > 3)
            {
                return false;
            }
            bool victimFound = false;
            bool foreignFound = false;
            HashSet<int> identities = new HashSet<int>();
            for (int index = 0; index < incident.Count; index++)
            {
                int identity = incident[index].SourceEdgeIndex;
                if (!identities.Add(identity))
                {
                    return false;
                }
                victimFound |= identity == victimEdgeIndex;
                foreignFound |= identity == foreignEdgeIndex;
            }
            return victimFound && foreignFound;
        }

        private static bool AreMatchingEndpointPatchRecoverySets(
            List<PlaneCutBevelCandidate> prepared,
            List<PlaneCutBevelCandidate> minimum)
        {
            if (prepared == null || minimum == null ||
                prepared.Count != minimum.Count)
            {
                return false;
            }
            HashSet<int> identities = new HashSet<int>();
            for (int index = 0; index < prepared.Count; index++)
            {
                identities.Add(prepared[index].SourceEdgeIndex);
            }
            for (int index = 0; index < minimum.Count; index++)
            {
                if (!identities.Remove(
                        minimum[index].SourceEdgeIndex))
                {
                    return false;
                }
            }
            return identities.Count == 0;
        }
        private static bool TryBuildPlaneCutEndpointPatchCandidate(
            List<PolygonFace> currentFaces,
            ChamferTopologyContext context,
            int vertexIndex,
            List<PlaneCutBevelCandidate> incident,
            Vector3 normal,
            int normalRank,
            bool isDirect,
            float depthFactor,
            float minimumStableEdgeLength,
            out PlaneCutVertexJunctionCandidate candidate,
            out PlaneCutEndpointPatchSupportEvidence supportEvidence,
            out PlaneCutEndpointPatchRejectionKind rejection,
            out string blocker)
        {
            candidate = default;
            supportEvidence = new PlaneCutEndpointPatchSupportEvidence();
            rejection = PlaneCutEndpointPatchRejectionKind.NoLocalRemoval;
            blocker = string.Empty;
            if (currentFaces == null || context == null ||
                context.Graph == null || incident == null ||
                incident.Count < 2 || !IsFinite(normal) ||
                normal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                blocker = "endpoint-local support inputs were incomplete";
                return false;
            }
            normal.Normalize();
            float minimumWidth = float.PositiveInfinity;
            float maximumWidth = 0f;
            float strengthSum = 0f;
            for (int index = 0; index < incident.Count; index++)
            {
                minimumWidth = Mathf.Min(minimumWidth, incident[index].Width);
                maximumWidth = Mathf.Max(maximumWidth, incident[index].Width);
                strengthSum += incident[index].Strength;
            }
            if (float.IsNaN(minimumWidth) ||
                float.IsInfinity(minimumWidth) ||
                minimumWidth <= PointMergeDistance)
            {
                blocker = "endpoint-local support had no stable incident width";
                return false;
            }

            float targetCutback = Mathf.Max(
                minimumStableEdgeLength * 0.15f,
                minimumWidth * depthFactor);
            float localRadius = Mathf.Max(
                minimumStableEdgeLength * 4f,
                Mathf.Max(maximumWidth * 6f, targetCutback * 4f));
            float radiusTolerance = Mathf.Max(
                PointMergeDistance * 2f,
                localRadius * 0.001f);
            float allowedRadius = localRadius + radiusTolerance;
            Vector3 sourceVertex =
                context.Graph.Vertices[vertexIndex].Position;
            float localSupport = float.NegativeInfinity;
            float globalSupport = float.NegativeInfinity;
            int controllingIdentity = -1;
            float controllingRadius = 0f;
            int totalSamples = 0;
            int minimumSamples = int.MaxValue;
            List<string> sampleEvidence = new List<string>();
            List<PlaneCutBevelCandidate> orderedIncident =
                new List<PlaneCutBevelCandidate>(incident);
            orderedIncident.Sort((left, right) =>
                left.SourceEdgeIndex.CompareTo(right.SourceEdgeIndex));

            for (int incidentIndex = 0;
                 incidentIndex < orderedIncident.Count;
                 incidentIndex++)
            {
                PlaneCutBevelCandidate incidentCandidate =
                    orderedIncident[incidentIndex];
                int sourceEdgeIndex =
                    incidentCandidate.SourceEdgeIndex;
                int sampleCount = 0;
                bool syntheticSupport = false;
                for (int faceIndex = 0;
                     faceIndex < currentFaces.Count;
                     faceIndex++)
                {
                    PolygonFace face = currentFaces[faceIndex];
                    if (face == null ||
                        face.ProvenanceKind !=
                            PolygonFaceProvenanceKind.EdgeBevelPlane ||
                        face.ProvenanceIndex != sourceEdgeIndex)
                    {
                        continue;
                    }
                    for (int vertexIndexInFace = 0;
                         vertexIndexInFace < face.Vertices.Count;
                         vertexIndexInFace++)
                    {
                        Vector3 start = face.Vertices[vertexIndexInFace];
                        Vector3 end = face.Vertices[
                            (vertexIndexInFace + 1) %
                            face.Vertices.Count];
                        globalSupport = Mathf.Max(
                            globalSupport,
                            Vector3.Dot(normal, start));
                        AddPlaneCutEndpointPatchSupportSample(
                            start,
                            sourceVertex,
                            allowedRadius,
                            normal,
                            sourceEdgeIndex,
                            ref localSupport,
                            ref controllingIdentity,
                            ref controllingRadius,
                            ref sampleCount);
                        Vector3 segment = end - start;
                        float segmentLengthSqr = segment.sqrMagnitude;
                        if (segmentLengthSqr >
                            MinimumEdgeLengthSqr)
                        {
                            float closestParameter = Mathf.Clamp01(
                                Vector3.Dot(
                                    sourceVertex - start,
                                    segment) /
                                segmentLengthSqr);
                            AddPlaneCutEndpointPatchSupportSample(
                                start + segment * closestParameter,
                                sourceVertex,
                                allowedRadius,
                                normal,
                                sourceEdgeIndex,
                                ref localSupport,
                                ref controllingIdentity,
                                ref controllingRadius,
                                ref sampleCount);
                            if (TryGetPlaneCutEndpointPatchSphereSegmentInterval(
                                    start,
                                    end,
                                    sourceVertex,
                                    allowedRadius,
                                    out float intervalStart,
                                    out float intervalEnd))
                            {
                                AddPlaneCutEndpointPatchSupportSample(
                                    Vector3.Lerp(
                                        start,
                                        end,
                                        intervalStart),
                                    sourceVertex,
                                    allowedRadius,
                                    normal,
                                    sourceEdgeIndex,
                                    ref localSupport,
                                    ref controllingIdentity,
                                    ref controllingRadius,
                                    ref sampleCount);
                                if (intervalEnd >
                                    intervalStart + 0.000001f)
                                {
                                    AddPlaneCutEndpointPatchSupportSample(
                                        Vector3.Lerp(
                                            start,
                                            end,
                                            intervalEnd),
                                        sourceVertex,
                                        allowedRadius,
                                        normal,
                                        sourceEdgeIndex,
                                        ref localSupport,
                                        ref controllingIdentity,
                                        ref controllingRadius,
                                        ref sampleCount);
                                }
                            }
                        }
                    }
                }
                if (sampleCount <= 0)
                {
                    float signedDistance =
                        incidentCandidate.Plane.SignedDistance(sourceVertex);
                    Vector3 syntheticPoint = sourceVertex -
                        incidentCandidate.Plane.Normal * signedDistance;
                    if (IsFinite(syntheticPoint) &&
                        Vector3.Distance(syntheticPoint, sourceVertex) <=
                            allowedRadius)
                    {
                        AddPlaneCutEndpointPatchSupportSample(
                            syntheticPoint,
                            sourceVertex,
                            allowedRadius,
                            normal,
                            sourceEdgeIndex,
                            ref localSupport,
                            ref controllingIdentity,
                            ref controllingRadius,
                            ref sampleCount);
                        globalSupport = Mathf.Max(
                            globalSupport,
                            Vector3.Dot(normal, syntheticPoint));
                        syntheticSupport = sampleCount > 0;
                    }
                }
                if (sampleCount <= 0)
                {
                    supportEvidence.FailureSource =
                        "missing-incident-support";
                    supportEvidence.LocalSupportRadius = localRadius;
                    supportEvidence.SamplesPerIncident =
                        string.Join("/", sampleEvidence);
                    rejection =
                        PlaneCutEndpointPatchRejectionKind.PatchExtraction;
                    blocker =
                        "endpoint-local and synthetic plane support were absent for incident bevel edge " +
                        sourceEdgeIndex.ToString();
                    return false;
                }
                totalSamples += sampleCount;
                minimumSamples = Mathf.Min(
                    minimumSamples,
                    sampleCount);
                sampleEvidence.Add(
                    sourceEdgeIndex.ToString() + ":" +
                    sampleCount.ToString() +
                    (syntheticSupport ? "s" : string.Empty));
            }

            supportEvidence.TotalSampleCount = totalSamples;
            supportEvidence.MinimumSamplesPerIncident =
                minimumSamples == int.MaxValue ? 0 : minimumSamples;
            supportEvidence.SamplesPerIncident =
                string.Join("/", sampleEvidence);
            supportEvidence.LocalSupportRadius = localRadius;
            supportEvidence.LocalSupportProjection = localSupport;
            supportEvidence.GlobalSupportProjection = globalSupport;
            supportEvidence.GlobalMinusLocalSupportDelta =
                Mathf.Max(0f, globalSupport - localSupport);
            supportEvidence.ControllingSourceEdgeIndex =
                controllingIdentity;
            supportEvidence.ControllingSupportRadius =
                controllingRadius;
            if (float.IsNaN(localSupport) ||
                float.IsInfinity(localSupport) ||
                controllingIdentity < 0 ||
                controllingRadius > allowedRadius)
            {
                supportEvidence.FailureSource =
                    "invalid-controlling-support";
                rejection =
                    PlaneCutEndpointPatchRejectionKind.PatchExtraction;
                blocker =
                    "endpoint-local support did not produce one finite local controller";
                return false;
            }

            float planeDistance = localSupport - targetCutback;
            float removal = localSupport - planeDistance;
            float minimumRemoval = Mathf.Max(
                PointMergeDistance * 2f,
                minimumStableEdgeLength * 0.005f);
            if (removal <= minimumRemoval)
            {
                supportEvidence.FailureSource = "insufficient-removal";
                blocker =
                    "endpoint-local support produced no stable local removal";
                return false;
            }
            float planeTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                minimumStableEdgeLength * 0.02f);
            float clipEpsilon = Mathf.Min(
                PlaneEpsilon,
                Mathf.Max(
                    PointMergeDistance * 0.25f,
                    removal * 0.25f));
            CutPlane plane = new CutPlane(normal, planeDistance);
            if (removal <= clipEpsilon)
            {
                supportEvidence.FailureSource = "no-local-removal";
                blocker =
                    "endpoint-local support boundary removed no stable endpoint-local shell region";
                return false;
            }

            candidate = new PlaneCutVertexJunctionCandidate(
                vertexIndex,
                plane,
                strengthSum / incident.Count,
                planeTolerance,
                clipEpsilon,
                localRadius,
                removal,
                0f,
                0f,
                0,
                normalRank,
                isDirect);
            return true;
        }

        private static void AddPlaneCutEndpointPatchSupportSample(
            Vector3 sample,
            Vector3 sourceVertex,
            float allowedRadius,
            Vector3 normal,
            int sourceEdgeIndex,
            ref float localSupport,
            ref int controllingIdentity,
            ref float controllingRadius,
            ref int sampleCount)
        {
            float radius = Vector3.Distance(sample, sourceVertex);
            if (!IsFinite(sample) || radius > allowedRadius)
            {
                return;
            }
            sampleCount++;
            float projection = Vector3.Dot(normal, sample);
            if (projection > localSupport)
            {
                localSupport = projection;
                controllingIdentity = sourceEdgeIndex;
                controllingRadius = radius;
            }
        }

        private static bool
            TryGetPlaneCutEndpointPatchSphereSegmentInterval(
                Vector3 start,
                Vector3 end,
                Vector3 center,
                float radius,
                out float intervalStart,
                out float intervalEnd)
        {
            intervalStart = 0f;
            intervalEnd = 0f;
            Vector3 segment = end - start;
            float a = Vector3.Dot(segment, segment);
            if (a <= MinimumEdgeLengthSqr)
            {
                return (start - center).sqrMagnitude <=
                    radius * radius;
            }
            Vector3 offset = start - center;
            float b = 2f * Vector3.Dot(offset, segment);
            float c = Vector3.Dot(offset, offset) -
                radius * radius;
            float discriminant = b * b - 4f * a * c;
            if (discriminant < 0f)
            {
                return false;
            }
            float root = Mathf.Sqrt(Mathf.Max(0f, discriminant));
            float inverse = 0.5f / a;
            float first = Mathf.Min(
                (-b - root) * inverse,
                (-b + root) * inverse);
            float second = Mathf.Max(
                (-b - root) * inverse,
                (-b + root) * inverse);
            if (second < 0f || first > 1f)
            {
                return false;
            }
            intervalStart = Mathf.Clamp01(first);
            intervalEnd = Mathf.Clamp01(second);
            return intervalEnd + 0.000001f >= intervalStart;
        }

        private static bool TryBuildPlaneCutEndpointPatchReplacement(
            List<PolygonFace> edgeOnlyFaces,
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            List<PlaneCutBevelCandidate> incident,
            PlaneCutVertexJunctionCandidate boundary,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            PlaneCutEndpointCellLimit[] requiredCellLimits,
            out PlaneCutEndpointPatchReplacement replacement,
            out List<PolygonFace> patchedFaces,
            out PlaneCutEndpointPatchLocalityEvidence localityEvidence,
            out PlaneCutEndpointPatchAxialEvidence axialEvidence,
            out PlaneCutEndpointCellEvidence cellEvidence,
            out PlaneCutEndpointPatchRejectionKind rejection,
            out string blocker)
        {
            replacement = null;
            patchedFaces = null;
            localityEvidence = new PlaneCutEndpointPatchLocalityEvidence();
            axialEvidence = new PlaneCutEndpointPatchAxialEvidence();
            cellEvidence = new PlaneCutEndpointCellEvidence();
            rejection = PlaneCutEndpointPatchRejectionKind.PatchExtraction;
            blocker = string.Empty;
            if (edgeOnlyFaces == null || sourceFaces == null ||
                context == null || context.Graph == null ||
                incident == null || incident.Count < 2)
            {
                blocker = "endpoint-cell reconstruction inputs were incomplete";
                return false;
            }

            PlaneCutEndpointCellLimit[] cellLimits;
            string cellLimitSignature;
            if (requiredCellLimits != null &&
                requiredCellLimits.Length > 0)
            {
                cellLimits = new PlaneCutEndpointCellLimit[
                    requiredCellLimits.Length];
                Array.Copy(
                    requiredCellLimits,
                    cellLimits,
                    requiredCellLimits.Length);
                cellLimitSignature =
                    BuildPlaneCutEndpointCellLimitSignature(cellLimits);
            }
            else if (!TryBuildPlaneCutEndpointCellLimits(
                    context,
                    boundary,
                    incident,
                    minimumStableEdgeLength,
                    out cellLimits,
                    out cellLimitSignature,
                    out blocker))
            {
                rejection = PlaneCutEndpointPatchRejectionKind.Locality;
                cellEvidence.FailureSource = "cell-limits";
                return false;
            }

            cellEvidence.CellLimitSignature = cellLimitSignature;

            Dictionary<TopologyEdgeKey, Vector3>[] cellCaches =
                new Dictionary<TopologyEdgeKey, Vector3>[cellLimits.Length];
            for (int index = 0; index < cellCaches.Length; index++)
            {
                cellCaches[index] =
                    new Dictionary<TopologyEdgeKey, Vector3>();
            }
            Dictionary<TopologyEdgeKey, Vector3> junctionCache =
                new Dictionary<TopologyEdgeKey, Vector3>();
            Vector3 sourceVertex = context.Graph.Vertices[
                boundary.VertexIndex].Position;
            HashSet<int> incidentIdentities = new HashSet<int>();
            for (int index = 0; index < incident.Count; index++)
            {
                incidentIdentities.Add(incident[index].SourceEdgeIndex);
            }

            List<int> selectedFaceIndices = new List<int>();
            List<PolygonFace> replacementFaces = new List<PolygonFace>();
            List<Vector3> capPoints = new List<Vector3>();
            List<Vector3> cellSplitPoints = new List<Vector3>();
            List<Vector3> localInfluencePoints = new List<Vector3>();
            HashSet<int> joinedIncident = new HashSet<int>();
            List<string> selectedProvenance = new List<string>();
            List<string> localFragmentSignatures = new List<string>();
            List<string> remoteRemainderSignatures = new List<string>();
            int facesSubdivided = 0;
            int localFragmentCount = 0;
            int remoteRemainderCount = 0;

            for (int faceIndex = 0;
                 faceIndex < edgeOnlyFaces.Count;
                 faceIndex++)
            {
                PolygonFace face = edgeOnlyFaces[faceIndex];
                if (!TryPartitionPlaneCutEndpointCellFace(
                        face,
                        cellLimits,
                        boundary,
                        sourceVertex,
                        minimumStableFaceArea,
                        cellCaches,
                        junctionCache,
                        true,
                        out PlaneCutEndpointCellFacePartition partition,
                        out blocker))
                {
                    rejection = PlaneCutEndpointPatchRejectionKind.PatchExtraction;
                    cellEvidence.FailureSource = "face-partition";
                    return false;
                }
                if (!partition.Changed)
                {
                    continue;
                }
                if (face.ProvenanceKind ==
                        PolygonFaceProvenanceKind.EdgeBevelPlane &&
                    !incidentIdentities.Contains(face.ProvenanceIndex))
                {
                    rejection = PlaneCutEndpointPatchRejectionKind.PatchExtraction;
                    cellEvidence.FailureSource = "non-incident-bevel";
                    blocker =
                        "bounded endpoint cell reached a non-incident bevel face";
                    return false;
                }

                selectedFaceIndices.Add(faceIndex);
                facesSubdivided++;
                selectedProvenance.Add(
                    ((int)face.ProvenanceKind).ToString() + ":" +
                    face.ProvenanceIndex.ToString());
                remoteRemainderCount += partition.RemoteRemainders.Count;
                localFragmentCount += partition.LocalFragments.Count;
                localityEvidence.MaximumRemovedVertexRadius = Mathf.Max(
                    localityEvidence.MaximumRemovedVertexRadius,
                    partition.MaximumRemovedVertexRadius);
                for (int index = 0;
                     index < partition.RemoteRemainders.Count;
                     index++)
                {
                    PolygonFace piece = partition.RemoteRemainders[index];
                    replacementFaces.Add(piece);
                    remoteRemainderSignatures.Add(
                        BuildPlaneCutEndpointPatchFaceSignature(piece));
                }
                for (int index = 0;
                     index < partition.LocalFragments.Count;
                     index++)
                {
                    PolygonFace piece = partition.LocalFragments[index];
                    replacementFaces.Add(piece);
                    localFragmentSignatures.Add(
                        BuildPlaneCutEndpointPatchFaceSignature(piece));
                    if (piece.ProvenanceKind ==
                            PolygonFaceProvenanceKind.EdgeBevelPlane &&
                        incidentIdentities.Contains(piece.ProvenanceIndex))
                    {
                        joinedIncident.Add(piece.ProvenanceIndex);
                    }
                }
                capPoints.AddRange(partition.JunctionCapPoints);
                cellSplitPoints.AddRange(partition.CellSplitPoints);
                localInfluencePoints.AddRange(partition.LocalInfluencePoints);
            }

            if (selectedFaceIndices.Count == 0)
            {
                rejection = PlaneCutEndpointPatchRejectionKind.NoLocalRemoval;
                cellEvidence.FailureSource = "no-local-subface";
                blocker =
                    "bounded endpoint cell removed no local shell subface";
                return false;
            }

            List<int> syntheticIncidentIdentities = new List<int>();
            for (int incidentIndex = 0;
                 incidentIndex < incident.Count;
                 incidentIndex++)
            {
                PlaneCutBevelCandidate edge = incident[incidentIndex];
                if (joinedIncident.Contains(edge.SourceEdgeIndex))
                {
                    continue;
                }
                if (!TryBuildPlaneCutEndpointCellSyntheticIncidentFragment(
                        sourceFaces,
                        edge,
                        cellLimits,
                        boundary,
                        sourceVertex,
                        minimumStableFaceArea,
                        cellCaches,
                        junctionCache,
                        out PlaneCutEndpointCellFacePartition synthetic,
                        out blocker))
                {
                    rejection = PlaneCutEndpointPatchRejectionKind.PatchExtraction;
                    cellEvidence.FailureSource = "synthetic-build";
                    return false;
                }
                if (synthetic.LocalFragments.Count == 0)
                {
                    rejection = PlaneCutEndpointPatchRejectionKind.IncidentBandJoin;
                    cellEvidence.FailureSource = "synthetic-incident";
                    blocker =
                        "bounded endpoint cell could not reconstruct incident bevel edge " +
                        edge.SourceEdgeIndex.ToString();
                    return false;
                }
                for (int index = 0;
                     index < synthetic.LocalFragments.Count;
                     index++)
                {
                    PolygonFace piece = synthetic.LocalFragments[index];
                    replacementFaces.Add(piece);
                    localFragmentSignatures.Add(
                        BuildPlaneCutEndpointPatchFaceSignature(piece));
                }
                capPoints.AddRange(synthetic.JunctionCapPoints);
                cellSplitPoints.AddRange(synthetic.CellSplitPoints);
                localInfluencePoints.AddRange(synthetic.LocalInfluencePoints);
                localityEvidence.MaximumRemovedVertexRadius = Mathf.Max(
                    localityEvidence.MaximumRemovedVertexRadius,
                    synthetic.MaximumRemovedVertexRadius);
                joinedIncident.Add(edge.SourceEdgeIndex);
                syntheticIncidentIdentities.Add(edge.SourceEdgeIndex);
                localFragmentCount += synthetic.LocalFragments.Count;
            }

            if (joinedIncident.Count != incidentIdentities.Count)
            {
                rejection = PlaneCutEndpointPatchRejectionKind.IncidentBandJoin;
                cellEvidence.FailureSource = "missing-incident-identity";
                blocker =
                    "bounded endpoint cell did not retain or reconstruct every incident bevel identity";
                return false;
            }

            List<Vector3> uniqueCapPoints = GetUniquePoints(capPoints);
            if (uniqueCapPoints.Count < 3)
            {
                rejection = PlaneCutEndpointPatchRejectionKind.CapCreation;
                cellEvidence.FailureSource = "cap-points";
                blocker =
                    "bounded endpoint cell produced no connecting cap polygon";
                return false;
            }
            PolygonFace oriented = CreateOrientedFace(
                boundary.Plane.Normal,
                PolygonFaceFeature.ConvexEdgeWear,
                boundary.Strength,
                uniqueCapPoints.ToArray());
            List<Vector3> sanitizedCap = SanitizePolygon(
                oriented.Vertices,
                oriented.Normal);
            if (sanitizedCap.Count < 3 ||
                CalculatePolygonArea(sanitizedCap) <=
                    Mathf.Max(
                        TinyFaceAreaEpsilon,
                        minimumStableFaceArea * 0.05f))
            {
                rejection = PlaneCutEndpointPatchRejectionKind.CapCreation;
                cellEvidence.FailureSource = "cap-degenerate";
                blocker = "bounded endpoint-cell cap was degenerate";
                return false;
            }
            PolygonFace cap = new PolygonFace(
                sanitizedCap,
                oriented.Normal,
                PolygonFaceFeature.ConvexEdgeWear,
                boundary.Strength,
                PolygonFaceProvenanceKind.BoundedEndpointCap,
                boundary.VertexIndex);
            replacementFaces.Add(cap);
            localInfluencePoints.AddRange(cap.Vertices);
            for (int index = 0; index < cap.Vertices.Count; index++)
            {
                float radius = Vector3.Distance(
                    cap.Vertices[index],
                    sourceVertex);
                localityEvidence.MaximumIntersectionRadius = Mathf.Max(
                    localityEvidence.MaximumIntersectionRadius,
                    radius);
                localityEvidence.MaximumReplacementVertexRadius = Mathf.Max(
                    localityEvidence.MaximumReplacementVertexRadius,
                    radius);
            }

            if (!TryBuildPlaneCutEndpointCellSelectedBoundary(
                    edgeOnlyFaces,
                    new HashSet<int>(selectedFaceIndices),
                    out Vector3[] boundaryLoop,
                    out string boundaryTopologySignature,
                    out string boundaryPositionSignature,
                    out blocker))
            {
                rejection = PlaneCutEndpointPatchRejectionKind.BoundaryLoop;
                cellEvidence.FailureSource = "selected-boundary";
                return false;
            }

            patchedFaces = new List<PolygonFace>();
            HashSet<int> selected = new HashSet<int>(selectedFaceIndices);
            for (int faceIndex = 0;
                 faceIndex < edgeOnlyFaces.Count;
                 faceIndex++)
            {
                if (!selected.Contains(faceIndex))
                {
                    patchedFaces.Add(ClonePlaneCutPolygonFace(
                        edgeOnlyFaces[faceIndex]));
                }
            }
            for (int faceIndex = 0;
                 faceIndex < replacementFaces.Count;
                 faceIndex++)
            {
                patchedFaces.Add(ClonePlaneCutPolygonFace(
                    replacementFaces[faceIndex]));
            }

            EdgeWearTopologyStats patchTopology = AuditEdgeWearTopology(
                patchedFaces,
                minimumStableEdgeLength);
            if (patchTopology.OpenEdgeCount > 0 ||
                patchTopology.NonManifoldEdgeCount > 0 ||
                patchTopology.TJunctionCount > 0)
            {
                rejection = PlaneCutEndpointPatchRejectionKind.StitchTopology;
                cellEvidence.FailureSource = "stitch-topology";
                blocker =
                    "bounded endpoint-cell splice failed closed-manifold topology";
                return false;
            }
            if (!DoesPlaneCutJunctionJoinIncidentBevels(
                    patchedFaces,
                    boundary,
                    incident))
            {
                rejection = PlaneCutEndpointPatchRejectionKind.IncidentBandJoin;
                cellEvidence.FailureSource = "incident-join";
                blocker =
                    "bounded endpoint-cell cap did not join every incident bevel fragment";
                return false;
            }

            Vector3[] influencePoints = GetUniquePoints(
                localInfluencePoints).ToArray();
            if (!IsPlaneCutEndpointPatchAxiallyLocal(
                    influencePoints,
                    cap,
                    context,
                    boundary,
                    incident,
                    minimumStableEdgeLength,
                    axialEvidence,
                    out blocker))
            {
                rejection = PlaneCutEndpointPatchRejectionKind.Locality;
                cellEvidence.FailureSource = "axial-locality";
                return false;
            }

            PlaneCutSolveMetrics metrics = new PlaneCutSolveMetrics();
            List<PlaneCutVertexJunctionCandidate> junctions =
                new List<PlaneCutVertexJunctionCandidate> { boundary };
            if (!IsPlaneCutJunctionTrialGeometryValid(
                    patchedFaces,
                    context,
                    incident,
                    junctions,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    ref metrics,
                    out _,
                    out blocker))
            {
                rejection = PlaneCutEndpointPatchRejectionKind.BandIntegrity;
                cellEvidence.FailureSource = "band-integrity";
                return false;
            }
            if (!IsStablePlaneCutVertexJunctionCap(
                    cap,
                    sourceVertex,
                    boundary.LocalRadius,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    out float compactness,
                    out float aspectRatio))
            {
                rejection = PlaneCutEndpointPatchRejectionKind.CapCreation;
                cellEvidence.FailureSource = "cap-stability";
                blocker =
                    "bounded endpoint-cell cap was geometrically unstable";
                return false;
            }

            string[] selectedSignatures =
                new string[selectedFaceIndices.Count];
            for (int index = 0;
                 index < selectedFaceIndices.Count;
                 index++)
            {
                selectedSignatures[index] =
                    BuildPlaneCutEndpointPatchFaceSignature(
                        edgeOnlyFaces[selectedFaceIndices[index]]);
            }
            Array.Sort(selectedSignatures, StringComparer.Ordinal);
            selectedProvenance.Sort(StringComparer.Ordinal);
            localFragmentSignatures.Sort(StringComparer.Ordinal);
            remoteRemainderSignatures.Sort(StringComparer.Ordinal);
            syntheticIncidentIdentities.Sort();
            int[] incidentIdentityArray = new int[incident.Count];
            for (int index = 0; index < incident.Count; index++)
            {
                incidentIdentityArray[index] =
                    incident[index].SourceEdgeIndex;
            }
            Array.Sort(incidentIdentityArray);
            List<Vector3> uniqueCellSplitPoints = GetUniquePoints(
                cellSplitPoints);
            List<Vector3> allCellVertices = new List<Vector3>(
                uniqueCellSplitPoints);
            allCellVertices.AddRange(uniqueCapPoints);
            List<Vector3> uniqueCellVertices = GetUniquePoints(
                allCellVertices);
            localityEvidence.SelectedFaceCountBeforeLocalFilter =
                selectedFaceIndices.Count;
            localityEvidence.SelectedFaceCountAfterLocalFilter =
                selectedFaceIndices.Count;

            cellEvidence.FacesSubdivided = facesSubdivided;
            cellEvidence.LocalFragmentCount = localFragmentCount;
            cellEvidence.RemoteRemainderCount = remoteRemainderCount;
            cellEvidence.SyntheticIncidentFragmentCount =
                syntheticIncidentIdentities.Count;
            cellEvidence.SyntheticIncidentIdentities =
                string.Join("/", syntheticIncidentIdentities);
            cellEvidence.CellVertexCount = uniqueCellVertices.Count;
            cellEvidence.CellFaceCount = localFragmentCount +
                remoteRemainderCount + 1;
            cellEvidence.CellSplitSignature =
                BuildPlaneCutEndpointCellPointSetSignature(
                    uniqueCellSplitPoints);
            cellEvidence.LocalFragmentSignature =
                string.Join("/", localFragmentSignatures);
            cellEvidence.RemoteRemainderSignature =
                string.Join("/", remoteRemainderSignatures);
            cellEvidence.FailureSource = string.Empty;

            replacement = new PlaneCutEndpointPatchReplacement
            {
                VertexIndex = boundary.VertexIndex,
                Plane = boundary.Plane,
                Strength = boundary.Strength,
                PlaneTolerance = boundary.PlaneTolerance,
                ClipEpsilon = boundary.ClipEpsilon,
                LocalRadius = boundary.LocalRadius,
                CutDepth = boundary.CutDepth,
                NormalRank = boundary.NormalRank,
                SourceVertexPosition = sourceVertex,
                IncidentSourceEdgeIndices = incidentIdentityArray,
                SelectedFaceSignatures = selectedSignatures,
                SelectedProvenanceSignature =
                    string.Join("/", selectedProvenance),
                BoundaryTopologySignature =
                    boundaryTopologySignature,
                BoundaryPositionSignature =
                    boundaryPositionSignature,
                BoundaryLoop = boundaryLoop,
                ReplacementFaces = replacementFaces,
                SelectedFaceCount = selectedFaceIndices.Count,
                BoundaryVertexCount = boundaryLoop.Length,
                CapVertexCount = cap.Vertices.Count,
                Compactness = compactness,
                AspectRatio = aspectRatio,
                MaximumRemovedVertexRadius =
                    localityEvidence.MaximumRemovedVertexRadius,
                MaximumIntersectionRadius =
                    localityEvidence.MaximumIntersectionRadius,
                MaximumReplacementVertexRadius =
                    localityEvidence.MaximumReplacementVertexRadius,
                RetainedOutsideRadiusCount =
                    localityEvidence.RetainedOutsideRadiusCount,
                SelectedFaceCountBeforeLocalFilter =
                    localityEvidence.SelectedFaceCountBeforeLocalFilter,
                SelectedFaceCountAfterLocalFilter =
                    localityEvidence.SelectedFaceCountAfterLocalFilter,
                MaximumAxialInfluence = axialEvidence.MaximumInfluence,
                MinimumAllowedAxialInfluence =
                    axialEvidence.MinimumAllowedInfluence,
                AxialInfluenceSignature =
                    axialEvidence.InfluenceSignature,
                CellLimits = cellLimits,
                CellLimitSignature = cellLimitSignature,
                LocalFragmentSignature =
                    string.Join("/", localFragmentSignatures),
                RemoteRemainderSignature =
                    string.Join("/", remoteRemainderSignatures),
                CellSplitSignature =
                    BuildPlaneCutEndpointCellPointSetSignature(
                        uniqueCellSplitPoints),
                FacesSubdivided = facesSubdivided,
                LocalFragmentCount = localFragmentCount,
                RemoteRemainderCount = remoteRemainderCount,
                SyntheticIncidentFragmentCount =
                    syntheticIncidentIdentities.Count,
                SyntheticIncidentSourceEdgeIndices =
                    syntheticIncidentIdentities.ToArray(),
                CellVertexCount = uniqueCellVertices.Count,
                CellFaceCount = localFragmentCount +
                    remoteRemainderCount + 1
            };
            rejection = PlaneCutEndpointPatchRejectionKind.None;
            return true;
        }

        private static bool IsPlaneCutEndpointPatchAxiallyLocal(
            Vector3[] boundaryLoop,
            PolygonFace cap,
            ChamferTopologyContext context,
            PlaneCutVertexJunctionCandidate boundary,
            List<PlaneCutBevelCandidate> incident,
            float minimumStableEdgeLength,
            PlaneCutEndpointPatchAxialEvidence evidence,
            out string blocker)
        {
            blocker = string.Empty;
            evidence ??= new PlaneCutEndpointPatchAxialEvidence();
            if (boundaryLoop == null || boundaryLoop.Length < 3 ||
                cap == null || cap.Vertices == null ||
                cap.Vertices.Count < 3 ||
                context == null || context.Graph == null ||
                incident == null || incident.Count < 2)
            {
                blocker =
                    "bounded endpoint patch axial evidence was incomplete";
                return false;
            }

            List<string> signature = new List<string>();
            List<PlaneCutBevelCandidate> ordered =
                new List<PlaneCutBevelCandidate>(incident);
            ordered.Sort((left, right) =>
                left.SourceEdgeIndex.CompareTo(right.SourceEdgeIndex));
            for (int incidentIndex = 0;
                 incidentIndex < ordered.Count;
                 incidentIndex++)
            {
                PlaneCutBevelCandidate edge = ordered[incidentIndex];
                int otherVertexIndex;
                if (edge.VertexA == boundary.VertexIndex)
                {
                    otherVertexIndex = edge.VertexB;
                }
                else if (edge.VertexB == boundary.VertexIndex)
                {
                    otherVertexIndex = edge.VertexA;
                }
                else
                {
                    evidence.RejectedSourceEdgeIndex =
                        edge.SourceEdgeIndex;
                    evidence.RejectedEndpointVertexIndex =
                        boundary.VertexIndex;
                    blocker =
                        "bounded endpoint patch axial certification found a non-incident source edge";
                    return false;
                }
                Vector3 origin = context.Graph.Vertices[
                    boundary.VertexIndex].Position;
                Vector3 other = context.Graph.Vertices[
                    otherVertexIndex].Position;
                Vector3 axis = other - origin;
                float sourceLength = axis.magnitude;
                if (sourceLength <= PointMergeDistance)
                {
                    evidence.RejectedSourceEdgeIndex =
                        edge.SourceEdgeIndex;
                    evidence.RejectedEndpointVertexIndex =
                        otherVertexIndex;
                    blocker =
                        "bounded endpoint patch axial certification found a degenerate source edge";
                    return false;
                }
                axis /= sourceLength;
                float maximumAxial = 0f;
                float minimumAxial = 0f;
                for (int pointIndex = 0;
                     pointIndex < boundaryLoop.Length;
                     pointIndex++)
                {
                    float axial = Vector3.Dot(
                        boundaryLoop[pointIndex] - origin,
                        axis);
                    maximumAxial = Mathf.Max(maximumAxial, axial);
                    minimumAxial = Mathf.Min(minimumAxial, axial);
                }
                for (int pointIndex = 0;
                     pointIndex < cap.Vertices.Count;
                     pointIndex++)
                {
                    float axial = Vector3.Dot(
                        cap.Vertices[pointIndex] - origin,
                        axis);
                    maximumAxial = Mathf.Max(maximumAxial, axial);
                    minimumAxial = Mathf.Min(minimumAxial, axial);
                }
                float allowed = Mathf.Clamp(
                    Mathf.Max(
                        edge.Width * 4f,
                        Mathf.Max(
                            boundary.CutDepth * 2f,
                            minimumStableEdgeLength * 0.5f)),
                    sourceLength * 0.03f,
                    sourceLength * 0.25f);
                float tolerance = Mathf.Max(
                    PointMergeDistance * 4f,
                    edge.PlaneTolerance * 2f);
                evidence.MaximumInfluence = Mathf.Max(
                    evidence.MaximumInfluence,
                    maximumAxial);
                evidence.MinimumAllowedInfluence = Mathf.Min(
                    evidence.MinimumAllowedInfluence,
                    allowed);
                signature.Add(
                    edge.SourceEdgeIndex.ToString() + ":" +
                    maximumAxial.ToString("R") + "/" +
                    allowed.ToString("R"));
                if (minimumAxial < -tolerance ||
                    maximumAxial > allowed + tolerance ||
                    maximumAxial >=
                        sourceLength - tolerance)
                {
                    evidence.RejectedSourceEdgeIndex =
                        edge.SourceEdgeIndex;
                    evidence.RejectedEndpointVertexIndex =
                        otherVertexIndex;
                    evidence.InfluenceSignature =
                        string.Join("/", signature);
                    blocker =
                        "bounded endpoint patch exceeded patch-native axial influence on source edge " +
                        edge.SourceEdgeIndex.ToString();
                    return false;
                }
            }
            if (float.IsInfinity(
                    evidence.MinimumAllowedInfluence))
            {
                evidence.MinimumAllowedInfluence = 0f;
            }
            evidence.InfluenceSignature =
                string.Join("/", signature);
            return true;
        }

        private static bool TryExtractPlaneCutEndpointPatch(
            List<PolygonFace> faces,
            Vector3 sourceVertex,
            float localRadius,
            CutPlane plane,
            float clipEpsilon,
            List<PlaneCutBevelCandidate> incident,
            out List<int> selectedFaceIndices,
            out Vector3[] boundaryLoop,
            out string selectedProvenanceSignature,
            out string boundaryTopologySignature,
            out string boundaryPositionSignature,
            PlaneCutEndpointPatchLocalityEvidence localityEvidence,
            out PlaneCutEndpointPatchRejectionKind rejection,
            out string blocker)
        {
            selectedFaceIndices = new List<int>();
            boundaryLoop = Array.Empty<Vector3>();
            selectedProvenanceSignature = string.Empty;
            boundaryTopologySignature = string.Empty;
            boundaryPositionSignature = string.Empty;
            localityEvidence ??= new PlaneCutEndpointPatchLocalityEvidence();
            rejection = PlaneCutEndpointPatchRejectionKind.PatchExtraction;
            blocker = string.Empty;
            HashSet<int> incidentIdentities = new HashSet<int>();
            for (int index = 0; index < incident.Count; index++)
            {
                incidentIdentities.Add(incident[index].SourceEdgeIndex);
            }

            Dictionary<TopologyEdgeKey, List<int>> owners =
                new Dictionary<TopologyEdgeKey, List<int>>();
            Dictionary<TopologyEdgeKey, TopologyEdgeSegment> segments =
                new Dictionary<TopologyEdgeKey, TopologyEdgeSegment>();
            bool[] affected = new bool[faces.Count];
            List<int> seeds = new List<int>();
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
                bool faceAffected = false;
                for (int vertexIndex = 0;
                     vertexIndex < face.Vertices.Count;
                     vertexIndex++)
                {
                    faceAffected |= plane.SignedDistance(
                        face.Vertices[vertexIndex]) > clipEpsilon;
                    Vector3 start = face.Vertices[vertexIndex];
                    Vector3 end = face.Vertices[
                        (vertexIndex + 1) % face.Vertices.Count];
                    VertexKey startKey = new VertexKey(start);
                    VertexKey endKey = new VertexKey(end);
                    TopologyEdgeKey key = new TopologyEdgeKey(
                        startKey,
                        endKey);
                    if (!owners.TryGetValue(
                            key,
                            out List<int> edgeOwners))
                    {
                        edgeOwners = new List<int>();
                        owners.Add(key, edgeOwners);
                        segments.Add(
                            key,
                            new TopologyEdgeSegment(
                                start,
                                end,
                                startKey,
                                endKey));
                    }
                    edgeOwners.Add(faceIndex);
                }
                affected[faceIndex] = faceAffected;
                if (faceAffected &&
                    face.ProvenanceKind ==
                        PolygonFaceProvenanceKind.EdgeBevelPlane &&
                    incidentIdentities.Contains(face.ProvenanceIndex))
                {
                    seeds.Add(faceIndex);
                }
            }
            if (seeds.Count == 0)
            {
                rejection =
                    PlaneCutEndpointPatchRejectionKind.NoLocalRemoval;
                blocker =
                    "local boundary did not cross an incident bevel face";
                return false;
            }

            HashSet<int> unfiltered = new HashSet<int>();
            Queue<int> unfilteredQueue = new Queue<int>();
            unfiltered.Add(seeds[0]);
            unfilteredQueue.Enqueue(seeds[0]);
            while (unfilteredQueue.Count > 0)
            {
                int faceIndex = unfilteredQueue.Dequeue();
                PolygonFace face = faces[faceIndex];
                for (int edgeIndex = 0;
                     edgeIndex < face.Vertices.Count;
                     edgeIndex++)
                {
                    TopologyEdgeKey key = new TopologyEdgeKey(
                        new VertexKey(face.Vertices[edgeIndex]),
                        new VertexKey(face.Vertices[
                            (edgeIndex + 1) % face.Vertices.Count]));
                    if (!owners.TryGetValue(
                            key,
                            out List<int> edgeOwners))
                    {
                        continue;
                    }
                    for (int ownerIndex = 0;
                         ownerIndex < edgeOwners.Count;
                         ownerIndex++)
                    {
                        int neighbor = edgeOwners[ownerIndex];
                        if (affected[neighbor] &&
                            unfiltered.Add(neighbor))
                        {
                            unfilteredQueue.Enqueue(neighbor);
                        }
                    }
                }
            }
            localityEvidence.SelectedFaceCountBeforeLocalFilter =
                unfiltered.Count;

            HashSet<int> selected = new HashSet<int>();
            Queue<int> queue = new Queue<int>();
            selected.Add(seeds[0]);
            queue.Enqueue(seeds[0]);
            while (queue.Count > 0)
            {
                int faceIndex = queue.Dequeue();
                PolygonFace face = faces[faceIndex];
                for (int edgeIndex = 0;
                     edgeIndex < face.Vertices.Count;
                     edgeIndex++)
                {
                    TopologyEdgeKey key = new TopologyEdgeKey(
                        new VertexKey(face.Vertices[edgeIndex]),
                        new VertexKey(face.Vertices[
                            (edgeIndex + 1) % face.Vertices.Count]));
                    if (!owners.TryGetValue(
                            key,
                            out List<int> edgeOwners) ||
                        !segments.TryGetValue(
                            key,
                            out TopologyEdgeSegment segment) ||
                        !IsPlaneCutEndpointPatchEdgeLocallyAffected(
                            segment.Start,
                            segment.End,
                            sourceVertex,
                            localRadius,
                            plane,
                            clipEpsilon))
                    {
                        continue;
                    }
                    for (int ownerIndex = 0;
                         ownerIndex < edgeOwners.Count;
                         ownerIndex++)
                    {
                        int neighbor = edgeOwners[ownerIndex];
                        if (affected[neighbor] && selected.Add(neighbor))
                        {
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }
            localityEvidence.SelectedFaceCountAfterLocalFilter =
                selected.Count;
            for (int seedIndex = 0;
                 seedIndex < seeds.Count;
                 seedIndex++)
            {
                if (!selected.Contains(seeds[seedIndex]))
                {
                    rejection =
                        PlaneCutEndpointPatchRejectionKind.DisconnectedPatch;
                    blocker =
                        "incident bevel faces crossed by the boundary formed disconnected cut-local components";
                    return false;
                }
            }

            float radiusTolerance = Mathf.Max(
                PointMergeDistance * 2f,
                localRadius * 0.001f);
            float allowedRadius = localRadius + radiusTolerance;
            HashSet<int> joinedIncident = new HashSet<int>();
            List<string> provenance = new List<string>();
            foreach (int faceIndex in selected)
            {
                PolygonFace face = faces[faceIndex];
                if (face.ProvenanceKind ==
                        PolygonFaceProvenanceKind.EdgeBevelPlane)
                {
                    if (!incidentIdentities.Contains(face.ProvenanceIndex))
                    {
                        rejection =
                            PlaneCutEndpointPatchRejectionKind.PatchExtraction;
                        blocker =
                            "local patch reached a non-incident bevel face";
                        return false;
                    }
                    joinedIncident.Add(face.ProvenanceIndex);
                }
                provenance.Add(
                    ((int)face.ProvenanceKind).ToString() + ":" +
                    face.ProvenanceIndex.ToString());
                for (int vertexIndex = 0;
                     vertexIndex < face.Vertices.Count;
                     vertexIndex++)
                {
                    Vector3 vertex = face.Vertices[vertexIndex];
                    float radius = Vector3.Distance(
                        vertex,
                        sourceVertex);
                    if (plane.SignedDistance(vertex) > clipEpsilon)
                    {
                        localityEvidence.MaximumRemovedVertexRadius =
                            Mathf.Max(
                                localityEvidence.MaximumRemovedVertexRadius,
                                radius);
                        if (radius > allowedRadius)
                        {
                            localityEvidence.FailureSource =
                                "removed-vertex";
                            rejection =
                                PlaneCutEndpointPatchRejectionKind.Locality;
                            blocker =
                                "local cut would remove a vertex outside the endpoint-star radius";
                            return false;
                        }
                    }
                    else if (radius > allowedRadius)
                    {
                        localityEvidence.RetainedOutsideRadiusCount++;
                    }
                }
            }
            if (joinedIncident.Count != incidentIdentities.Count)
            {
                rejection =
                    PlaneCutEndpointPatchRejectionKind.IncidentBandJoin;
                blocker =
                    "local patch did not include every incident bevel identity";
                return false;
            }

            Dictionary<VertexKey, List<VertexKey>> boundaryAdjacency =
                new Dictionary<VertexKey, List<VertexKey>>();
            Dictionary<VertexKey, Vector3> boundaryPositions =
                new Dictionary<VertexKey, Vector3>();
            int boundaryEdgeCount = 0;
            foreach (KeyValuePair<TopologyEdgeKey, List<int>> pair in owners)
            {
                int selectedOwners = 0;
                for (int ownerIndex = 0;
                     ownerIndex < pair.Value.Count;
                     ownerIndex++)
                {
                    selectedOwners += selected.Contains(
                        pair.Value[ownerIndex]) ? 1 : 0;
                }
                if (selectedOwners == 0 ||
                    selectedOwners == pair.Value.Count)
                {
                    continue;
                }
                if (pair.Value.Count != 2 || selectedOwners != 1)
                {
                    rejection =
                        PlaneCutEndpointPatchRejectionKind.BoundaryLoop;
                    blocker =
                        "local patch boundary crossed an open or non-manifold source edge";
                    return false;
                }
                TopologyEdgeSegment segment = segments[pair.Key];
                if (plane.SignedDistance(segment.Start) > clipEpsilon ||
                    plane.SignedDistance(segment.End) > clipEpsilon)
                {
                    rejection =
                        PlaneCutEndpointPatchRejectionKind.BoundaryCrossing;
                    blocker =
                        "local clipping plane crossed the untouched stitch boundary";
                    return false;
                }
                AddPlaneCutEndpointPatchBoundaryNeighbor(
                    boundaryAdjacency,
                    segment.StartKey,
                    segment.EndKey);
                AddPlaneCutEndpointPatchBoundaryNeighbor(
                    boundaryAdjacency,
                    segment.EndKey,
                    segment.StartKey);
                boundaryPositions[segment.StartKey] = segment.Start;
                boundaryPositions[segment.EndKey] = segment.End;
                boundaryEdgeCount++;
            }
            if (boundaryEdgeCount < 3 ||
                !TryOrderPlaneCutEndpointPatchBoundary(
                    boundaryAdjacency,
                    boundaryPositions,
                    out boundaryLoop))
            {
                rejection =
                    PlaneCutEndpointPatchRejectionKind.BoundaryLoop;
                blocker =
                    "local patch did not expose exactly one closed non-branching stitch loop";
                return false;
            }

            selectedFaceIndices.AddRange(selected);
            selectedFaceIndices.Sort();
            provenance.Sort(StringComparer.Ordinal);
            selectedProvenanceSignature = string.Join("/", provenance);
            boundaryTopologySignature =
                "faces=" + selectedFaceIndices.Count +
                ";edges=" + boundaryEdgeCount +
                ";vertices=" + boundaryLoop.Length;
            boundaryPositionSignature =
                BuildPlaneCutEndpointPatchLoopSignature(boundaryLoop);
            return true;
        }

        private static bool IsPlaneCutEndpointPatchEdgeLocallyAffected(
            Vector3 start,
            Vector3 end,
            Vector3 sourceVertex,
            float localRadius,
            CutPlane plane,
            float clipEpsilon)
        {
            float startDistance = plane.SignedDistance(start);
            float endDistance = plane.SignedDistance(end);
            if (startDistance <= clipEpsilon &&
                endDistance <= clipEpsilon)
            {
                return false;
            }
            float radiusTolerance = Mathf.Max(
                PointMergeDistance * 2f,
                localRadius * 0.001f);
            float allowedRadiusSqr =
                (localRadius + radiusTolerance) *
                (localRadius + radiusTolerance);
            if (startDistance > clipEpsilon &&
                endDistance > clipEpsilon)
            {
                return DistancePlaneCutEndpointPatchPointToSegmentSquared(
                    sourceVertex,
                    start,
                    end) <= allowedRadiusSqr;
            }
            float denominator = startDistance - endDistance;
            if (Mathf.Abs(denominator) <= PointMergeDistance)
            {
                return false;
            }
            float parameter = Mathf.Clamp01(
                startDistance / denominator);
            Vector3 intersection = Vector3.Lerp(
                start,
                end,
                parameter);
            return (intersection - sourceVertex).sqrMagnitude <=
                allowedRadiusSqr;
        }

        private static float
            DistancePlaneCutEndpointPatchPointToSegmentSquared(
                Vector3 point,
                Vector3 start,
                Vector3 end)
        {
            Vector3 segment = end - start;
            float lengthSqr = segment.sqrMagnitude;
            if (lengthSqr <= MinimumEdgeLengthSqr)
            {
                return (point - start).sqrMagnitude;
            }
            float parameter = Mathf.Clamp01(
                Vector3.Dot(point - start, segment) / lengthSqr);
            return (point - (start + segment * parameter)).sqrMagnitude;
        }

        private static void AddPlaneCutEndpointPatchBoundaryNeighbor(
            Dictionary<VertexKey, List<VertexKey>> adjacency,
            VertexKey key,
            VertexKey neighbor)
        {
            if (!adjacency.TryGetValue(
                    key,
                    out List<VertexKey> neighbors))
            {
                neighbors = new List<VertexKey>();
                adjacency.Add(key, neighbors);
            }
            if (!neighbors.Contains(neighbor))
            {
                neighbors.Add(neighbor);
            }
        }

        private static bool TryOrderPlaneCutEndpointPatchBoundary(
            Dictionary<VertexKey, List<VertexKey>> adjacency,
            Dictionary<VertexKey, Vector3> positions,
            out Vector3[] loop)
        {
            loop = Array.Empty<Vector3>();
            if (adjacency == null || adjacency.Count < 3)
            {
                return false;
            }
            VertexKey start = default;
            bool hasStart = false;
            foreach (KeyValuePair<VertexKey, List<VertexKey>> pair in adjacency)
            {
                if (pair.Value.Count != 2)
                {
                    return false;
                }
                pair.Value.Sort((left, right) => left.CompareTo(right));
                if (!hasStart || pair.Key.CompareTo(start) < 0)
                {
                    start = pair.Key;
                    hasStart = true;
                }
            }

            List<Vector3> ordered = new List<Vector3>();
            HashSet<VertexKey> visited = new HashSet<VertexKey>();
            VertexKey previous = default;
            bool hasPrevious = false;
            VertexKey current = start;
            for (int guard = 0; guard <= adjacency.Count; guard++)
            {
                if (visited.Contains(current))
                {
                    if (current.Equals(start) &&
                        visited.Count == adjacency.Count)
                    {
                        loop = ordered.ToArray();
                        return loop.Length >= 3;
                    }
                    return false;
                }
                visited.Add(current);
                ordered.Add(positions[current]);
                List<VertexKey> neighbors = adjacency[current];
                VertexKey next = !hasPrevious ||
                    !neighbors[0].Equals(previous)
                        ? neighbors[0]
                        : neighbors[1];
                previous = current;
                hasPrevious = true;
                current = next;
            }
            return false;
        }

        private static bool TryClipPlaneCutEndpointPatchFaces(
            List<PolygonFace> sourceFaces,
            List<int> selectedFaceIndices,
            PlaneCutVertexJunctionCandidate boundary,
            Vector3 sourceVertex,
            float localRadius,
            PlaneCutEndpointPatchLocalityEvidence localityEvidence,
            float minimumStableFaceArea,
            out List<PolygonFace> replacementFaces,
            out PolygonFace cap,
            out PlaneCutEndpointPatchRejectionKind rejection,
            out string blocker)
        {
            replacementFaces = new List<PolygonFace>();
            cap = null;
            rejection = PlaneCutEndpointPatchRejectionKind.PatchExtraction;
            blocker = string.Empty;
            localityEvidence ??= new PlaneCutEndpointPatchLocalityEvidence();
            float radiusTolerance = Mathf.Max(
                PointMergeDistance * 2f,
                localRadius * 0.001f);
            float allowedRadius = localRadius + radiusTolerance;
            HashSet<VertexKey> retainedOriginalVertices =
                new HashSet<VertexKey>();
            for (int selectedIndex = 0;
                 selectedIndex < selectedFaceIndices.Count;
                 selectedIndex++)
            {
                PolygonFace sourceFace = sourceFaces[
                    selectedFaceIndices[selectedIndex]];
                for (int vertexIndex = 0;
                     vertexIndex < sourceFace.Vertices.Count;
                     vertexIndex++)
                {
                    Vector3 vertex = sourceFace.Vertices[vertexIndex];
                    if (boundary.Plane.SignedDistance(vertex) <=
                        boundary.ClipEpsilon)
                    {
                        retainedOriginalVertices.Add(
                            new VertexKey(vertex));
                    }
                }
            }
            List<Vector3> capPoints = new List<Vector3>();
            Dictionary<EdgeKey, Vector3> intersectionCache =
                new Dictionary<EdgeKey, Vector3>();
            PlaneCutNumericalRepairTelemetry numericalRepairs =
                new PlaneCutNumericalRepairTelemetry();
            for (int selectedIndex = 0;
                 selectedIndex < selectedFaceIndices.Count;
                 selectedIndex++)
            {
                PolygonFace face = sourceFaces[
                    selectedFaceIndices[selectedIndex]];
                List<Vector3> clipped = ClipPolygon(
                    face.Vertices,
                    face.Normal,
                    CalculateAuthoredFacePlaneDistance(face),
                    face.ProvenanceKind,
                    face.ProvenanceIndex,
                    boundary.Plane,
                    PolygonFaceProvenanceKind.BoundedEndpointCap,
                    boundary.VertexIndex,
                    capPoints,
                    true,
                    boundary.ClipEpsilon,
                    intersectionCache,
                    true,
                    numericalRepairs,
                    out bool succeeded);
                if (!succeeded ||
                    numericalRepairs.ExactConstructionFailureCount > 0)
                {
                    rejection =
                        PlaneCutEndpointPatchRejectionKind.PatchExtraction;
                    blocker =
                        "exact selected-face clipping failed during local endpoint replacement";
                    return false;
                }
                List<Vector3> sanitized = SanitizePolygon(
                    clipped,
                    face.Normal);
                if (sanitized.Count >= 3 &&
                    CalculatePolygonArea(sanitized) >
                        Mathf.Max(TinyFaceAreaEpsilon,
                            minimumStableFaceArea * 0.05f))
                {
                    for (int vertexIndex = 0;
                         vertexIndex < sanitized.Count;
                         vertexIndex++)
                    {
                        Vector3 vertex = sanitized[vertexIndex];
                        if (retainedOriginalVertices.Contains(
                                new VertexKey(vertex)))
                        {
                            continue;
                        }
                        float radius = Vector3.Distance(
                            vertex,
                            sourceVertex);
                        localityEvidence.MaximumReplacementVertexRadius =
                            Mathf.Max(
                                localityEvidence.MaximumReplacementVertexRadius,
                                radius);
                        if (radius > allowedRadius)
                        {
                            localityEvidence.FailureSource =
                                "replacement";
                            rejection =
                                PlaneCutEndpointPatchRejectionKind.Locality;
                            blocker =
                                "local replacement introduced a vertex outside the endpoint-star radius";
                            return false;
                        }
                    }
                    replacementFaces.Add(
                        new PolygonFace(
                            sanitized,
                            face.Normal,
                            face.Feature,
                            face.FeatureStrength,
                            face.ProvenanceKind,
                            face.ProvenanceIndex));
                }
            }

            List<Vector3> uniqueCapPoints = GetUniquePoints(capPoints);
            for (int pointIndex = 0;
                 pointIndex < uniqueCapPoints.Count;
                 pointIndex++)
            {
                float radius = Vector3.Distance(
                    uniqueCapPoints[pointIndex],
                    sourceVertex);
                localityEvidence.MaximumIntersectionRadius = Mathf.Max(
                    localityEvidence.MaximumIntersectionRadius,
                    radius);
                if (radius > allowedRadius)
                {
                    localityEvidence.FailureSource = "intersection";
                    rejection =
                        PlaneCutEndpointPatchRejectionKind.Locality;
                    blocker =
                        "local cut produced an intersection outside the endpoint-star radius";
                    return false;
                }
            }
            if (uniqueCapPoints.Count < 3)
            {
                rejection = PlaneCutEndpointPatchRejectionKind.CapCreation;
                blocker =
                    "selected local faces produced no bounded endpoint cap";
                return false;
            }
            PolygonFace oriented = CreateOrientedFace(
                boundary.Plane.Normal,
                PolygonFaceFeature.ConvexEdgeWear,
                boundary.Strength,
                uniqueCapPoints.ToArray());
            List<Vector3> sanitizedCap = SanitizePolygon(
                oriented.Vertices,
                oriented.Normal);
            if (sanitizedCap.Count < 3 ||
                CalculatePolygonArea(sanitizedCap) <=
                    Mathf.Max(TinyFaceAreaEpsilon,
                        minimumStableFaceArea * 0.05f))
            {
                rejection = PlaneCutEndpointPatchRejectionKind.CapCreation;
                blocker = "bounded endpoint cap was degenerate";
                return false;
            }
            for (int vertexIndex = 0;
                 vertexIndex < sanitizedCap.Count;
                 vertexIndex++)
            {
                float radius = Vector3.Distance(
                    sanitizedCap[vertexIndex],
                    sourceVertex);
                localityEvidence.MaximumReplacementVertexRadius = Mathf.Max(
                    localityEvidence.MaximumReplacementVertexRadius,
                    radius);
                if (radius > allowedRadius)
                {
                    localityEvidence.FailureSource = "cap";
                    rejection =
                        PlaneCutEndpointPatchRejectionKind.Locality;
                    blocker =
                        "bounded endpoint cap exceeded the endpoint-star radius";
                    return false;
                }
            }
            cap = new PolygonFace(
                sanitizedCap,
                oriented.Normal,
                PolygonFaceFeature.ConvexEdgeWear,
                boundary.Strength,
                PolygonFaceProvenanceKind.BoundedEndpointCap,
                boundary.VertexIndex);
            replacementFaces.Add(cap);
            return true;
        }

        private static bool TryBuildPlaneCutEndpointCellLimits(
            ChamferTopologyContext context,
            PlaneCutVertexJunctionCandidate boundary,
            List<PlaneCutBevelCandidate> incident,
            float minimumStableEdgeLength,
            out PlaneCutEndpointCellLimit[] limits,
            out string signature,
            out string blocker)
        {
            limits = Array.Empty<PlaneCutEndpointCellLimit>();
            signature = string.Empty;
            blocker = string.Empty;
            if (context == null || context.Graph == null ||
                incident == null || incident.Count < 2 ||
                boundary.VertexIndex < 0 ||
                boundary.VertexIndex >= context.Graph.Vertices.Count)
            {
                blocker = "endpoint-cell axial-limit inputs were incomplete";
                return false;
            }
            Vector3 origin = context.Graph.Vertices[
                boundary.VertexIndex].Position;
            List<PlaneCutBevelCandidate> ordered =
                new List<PlaneCutBevelCandidate>(incident);
            ordered.Sort((left, right) =>
                left.SourceEdgeIndex.CompareTo(right.SourceEdgeIndex));
            limits = new PlaneCutEndpointCellLimit[ordered.Count];
            List<string> evidence = new List<string>();
            for (int index = 0; index < ordered.Count; index++)
            {
                PlaneCutBevelCandidate edge = ordered[index];
                int otherVertexIndex;
                if (edge.VertexA == boundary.VertexIndex)
                {
                    otherVertexIndex = edge.VertexB;
                }
                else if (edge.VertexB == boundary.VertexIndex)
                {
                    otherVertexIndex = edge.VertexA;
                }
                else
                {
                    blocker =
                        "endpoint-cell limit encountered a non-incident source edge";
                    return false;
                }
                Vector3 other = context.Graph.Vertices[
                    otherVertexIndex].Position;
                Vector3 axis = other - origin;
                float sourceLength = axis.magnitude;
                if (sourceLength <= PointMergeDistance)
                {
                    blocker =
                        "endpoint-cell limit encountered a degenerate source edge";
                    return false;
                }
                axis /= sourceLength;
                float allowed = Mathf.Clamp(
                    Mathf.Max(
                        edge.Width * 4f,
                        Mathf.Max(
                            boundary.CutDepth * 2f,
                            minimumStableEdgeLength * 0.5f)),
                    sourceLength * 0.03f,
                    sourceLength * 0.25f);
                CutPlane plane = new CutPlane(
                    axis,
                    Vector3.Dot(axis, origin) + allowed);
                limits[index] = new PlaneCutEndpointCellLimit(
                    edge.SourceEdgeIndex,
                    otherVertexIndex,
                    plane,
                    allowed,
                    sourceLength);
                evidence.Add(
                    edge.SourceEdgeIndex.ToString() + ":" +
                    allowed.ToString("R"));
            }
            signature = string.Join("/", evidence);
            return true;
        }

        private static string BuildPlaneCutEndpointCellLimitSignature(
            PlaneCutEndpointCellLimit[] limits)
        {
            if (limits == null || limits.Length == 0)
            {
                return "none";
            }
            List<string> evidence = new List<string>();
            for (int index = 0; index < limits.Length; index++)
            {
                evidence.Add(
                    limits[index].SourceEdgeIndex.ToString() + ":" +
                    limits[index].AxialLimit.ToString("R"));
            }
            return string.Join("/", evidence);
        }

        private static bool TryPartitionPlaneCutEndpointCellFace(
            PolygonFace face,
            PlaneCutEndpointCellLimit[] cellLimits,
            PlaneCutVertexJunctionCandidate boundary,
            Vector3 sourceVertex,
            float minimumStableFaceArea,
            Dictionary<TopologyEdgeKey, Vector3>[] cellCaches,
            Dictionary<TopologyEdgeKey, Vector3> junctionCache,
            bool preserveRemoteRemainders,
            out PlaneCutEndpointCellFacePartition partition,
            out string blocker)
        {
            partition = new PlaneCutEndpointCellFacePartition();
            blocker = string.Empty;
            if (face == null || face.Vertices == null ||
                face.Vertices.Count < 3 || cellLimits == null ||
                cellCaches == null ||
                cellCaches.Length != cellLimits.Length)
            {
                blocker = "endpoint-cell face partition inputs were incomplete";
                return false;
            }
            float minimumArea = Mathf.Max(
                TinyFaceAreaEpsilon,
                minimumStableFaceArea * 0.05f);
            List<Vector3> local = new List<Vector3>(face.Vertices);
            List<List<Vector3>> remotePolygons =
                new List<List<Vector3>>();
            List<Vector3> cellSplitPoints = new List<Vector3>();
            for (int limitIndex = 0;
                 limitIndex < cellLimits.Length;
                 limitIndex++)
            {
                if (!TrySplitPlaneCutEndpointCellPolygon(
                        local,
                        cellLimits[limitIndex].Plane,
                        boundary.ClipEpsilon,
                        cellCaches[limitIndex],
                        out List<Vector3> inside,
                        out List<Vector3> outside,
                        out List<Vector3> intersections))
                {
                    blocker =
                        "endpoint-cell axial face split failed";
                    return false;
                }
                List<Vector3> sanitizedOutside = SanitizePolygon(
                    outside,
                    face.Normal);
                if (preserveRemoteRemainders &&
                    sanitizedOutside.Count >= 3 &&
                    CalculatePolygonArea(sanitizedOutside) > minimumArea)
                {
                    remotePolygons.Add(sanitizedOutside);
                }
                cellSplitPoints.AddRange(intersections);
                local = SanitizePolygon(inside, face.Normal);
                if (local.Count < 3 ||
                    CalculatePolygonArea(local) <= minimumArea)
                {
                    return true;
                }
            }

            bool removesLocalGeometry = false;
            for (int index = 0; index < local.Count; index++)
            {
                if (boundary.Plane.SignedDistance(local[index]) >
                    boundary.ClipEpsilon)
                {
                    removesLocalGeometry = true;
                    break;
                }
            }
            if (!removesLocalGeometry)
            {
                return true;
            }

            if (!TrySplitPlaneCutEndpointCellPolygon(
                    local,
                    boundary.Plane,
                    boundary.ClipEpsilon,
                    junctionCache,
                    out List<Vector3> retainedLocal,
                    out List<Vector3> removedLocal,
                    out List<Vector3> junctionIntersections))
            {
                blocker =
                    "endpoint-cell junction face split failed";
                return false;
            }
            List<Vector3> sanitizedRemoved = SanitizePolygon(
                removedLocal,
                face.Normal);
            if (sanitizedRemoved.Count >= 3 &&
                CalculatePolygonArea(sanitizedRemoved) > minimumArea)
            {
                for (int index = 0;
                     index < sanitizedRemoved.Count;
                     index++)
                {
                    partition.MaximumRemovedVertexRadius = Mathf.Max(
                        partition.MaximumRemovedVertexRadius,
                        Vector3.Distance(
                            sanitizedRemoved[index],
                            sourceVertex));
                }
            }
            for (int index = 0; index < remotePolygons.Count; index++)
            {
                partition.RemoteRemainders.Add(
                    new PolygonFace(
                        remotePolygons[index],
                        face.Normal,
                        face.Feature,
                        face.FeatureStrength,
                        face.ProvenanceKind,
                        face.ProvenanceIndex));
            }
            List<Vector3> sanitizedRetained = SanitizePolygon(
                retainedLocal,
                face.Normal);
            if (sanitizedRetained.Count >= 3 &&
                CalculatePolygonArea(sanitizedRetained) > minimumArea)
            {
                PolygonFace localFace = new PolygonFace(
                    sanitizedRetained,
                    face.Normal,
                    face.Feature,
                    face.FeatureStrength,
                    face.ProvenanceKind,
                    face.ProvenanceIndex);
                partition.LocalFragments.Add(localFace);
                partition.LocalInfluencePoints.AddRange(
                    sanitizedRetained);
            }
            partition.JunctionCapPoints.AddRange(
                junctionIntersections);
            partition.CellSplitPoints.AddRange(cellSplitPoints);
            partition.Changed = true;
            return true;
        }

        private static bool TrySplitPlaneCutEndpointCellPolygon(
            List<Vector3> vertices,
            CutPlane plane,
            float epsilon,
            Dictionary<TopologyEdgeKey, Vector3> intersectionCache,
            out List<Vector3> inside,
            out List<Vector3> outside,
            out List<Vector3> intersections)
        {
            inside = new List<Vector3>();
            outside = new List<Vector3>();
            intersections = new List<Vector3>();
            if (vertices == null || vertices.Count < 3)
            {
                return true;
            }
            Vector3 previous = vertices[vertices.Count - 1];
            float previousDistance = plane.SignedDistance(previous);
            bool previousInside = previousDistance <= epsilon;
            for (int index = 0; index < vertices.Count; index++)
            {
                Vector3 current = vertices[index];
                float currentDistance = plane.SignedDistance(current);
                bool currentInside = currentDistance <= epsilon;
                if (previousInside)
                {
                    AddPointIfDifferent(inside, previous);
                }
                else
                {
                    AddPointIfDifferent(outside, previous);
                }
                if (previousInside != currentInside)
                {
                    TopologyEdgeKey key = new TopologyEdgeKey(
                        new VertexKey(previous),
                        new VertexKey(current));
                    if (!intersectionCache.TryGetValue(
                            key,
                            out Vector3 intersection))
                    {
                        float denominator =
                            previousDistance - currentDistance;
                        if (Mathf.Abs(denominator) <= 0.0000001f)
                        {
                            return false;
                        }
                        float parameter = Mathf.Clamp01(
                            previousDistance / denominator);
                        intersection = Vector3.Lerp(
                            previous,
                            current,
                            parameter);
                        intersectionCache.Add(key, intersection);
                    }
                    AddPointIfDifferent(inside, intersection);
                    AddPointIfDifferent(outside, intersection);
                    AddPointIfDifferent(intersections, intersection);
                }
                previous = current;
                previousDistance = currentDistance;
                previousInside = currentInside;
            }
            RemoveClosingDuplicate(inside);
            RemoveClosingDuplicate(outside);
            return true;
        }

        private static bool TryBuildPlaneCutEndpointCellSyntheticIncidentFragment(
            List<PolygonFace> sourceFaces,
            PlaneCutBevelCandidate incident,
            PlaneCutEndpointCellLimit[] cellLimits,
            PlaneCutVertexJunctionCandidate boundary,
            Vector3 sourceVertex,
            float minimumStableFaceArea,
            Dictionary<TopologyEdgeKey, Vector3>[] cellCaches,
            Dictionary<TopologyEdgeKey, Vector3> junctionCache,
            out PlaneCutEndpointCellFacePartition partition,
            out string blocker)
        {
            partition = null;
            blocker = string.Empty;
            List<PlaneCutBevelCandidate> isolatedEdges =
                new List<PlaneCutBevelCandidate> { incident };
            List<PlaneCutVertexJunctionCandidate> noJunctions =
                new List<PlaneCutVertexJunctionCandidate>();
            PlaneCutNumericalRepairTelemetry repairs =
                new PlaneCutNumericalRepairTelemetry();
            if (!TryBuildPlaneCutSystemFaces(
                    sourceFaces,
                    isolatedEdges,
                    noJunctions,
                    out List<PolygonFace> isolatedFaces,
                    out _,
                    out blocker,
                    repairs))
            {
                blocker =
                    "synthetic incident bevel shell failed: " + blocker;
                return false;
            }
            PolygonFace isolatedBevel = null;
            for (int index = 0; index < isolatedFaces.Count; index++)
            {
                PolygonFace face = isolatedFaces[index];
                if (face.ProvenanceKind ==
                        PolygonFaceProvenanceKind.EdgeBevelPlane &&
                    face.ProvenanceIndex == incident.SourceEdgeIndex)
                {
                    if (isolatedBevel != null)
                    {
                        blocker =
                            "synthetic incident bevel emitted duplicate local source faces";
                        return false;
                    }
                    isolatedBevel = face;
                }
            }
            if (isolatedBevel == null)
            {
                blocker =
                    "synthetic incident bevel face was unavailable for edge " +
                    incident.SourceEdgeIndex.ToString();
                return false;
            }
            if (!TryPartitionPlaneCutEndpointCellFace(
                    isolatedBevel,
                    cellLimits,
                    boundary,
                    sourceVertex,
                    minimumStableFaceArea,
                    cellCaches,
                    junctionCache,
                    false,
                    out partition,
                    out blocker))
            {
                return false;
            }
            if (!partition.Changed)
            {
                blocker =
                    "synthetic incident bevel did not intersect the bounded endpoint cell";
                return false;
            }
            return true;
        }

        private static bool TryBuildPlaneCutEndpointCellSelectedBoundary(
            List<PolygonFace> faces,
            HashSet<int> selected,
            out Vector3[] loop,
            out string topologySignature,
            out string positionSignature,
            out string blocker)
        {
            loop = Array.Empty<Vector3>();
            topologySignature = string.Empty;
            positionSignature = string.Empty;
            blocker = string.Empty;
            Dictionary<TopologyEdgeKey, List<int>> owners =
                new Dictionary<TopologyEdgeKey, List<int>>();
            Dictionary<TopologyEdgeKey, TopologyEdgeSegment> segments =
                new Dictionary<TopologyEdgeKey, TopologyEdgeSegment>();
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                for (int edgeIndex = 0;
                     edgeIndex < face.Vertices.Count;
                     edgeIndex++)
                {
                    Vector3 start = face.Vertices[edgeIndex];
                    Vector3 end = face.Vertices[
                        (edgeIndex + 1) % face.Vertices.Count];
                    VertexKey startKey = new VertexKey(start);
                    VertexKey endKey = new VertexKey(end);
                    TopologyEdgeKey key = new TopologyEdgeKey(
                        startKey,
                        endKey);
                    if (!owners.TryGetValue(
                            key,
                            out List<int> edgeOwners))
                    {
                        edgeOwners = new List<int>();
                        owners.Add(key, edgeOwners);
                        segments.Add(
                            key,
                            new TopologyEdgeSegment(
                                start,
                                end,
                                startKey,
                                endKey));
                    }
                    edgeOwners.Add(faceIndex);
                }
            }
            Dictionary<VertexKey, List<VertexKey>> adjacency =
                new Dictionary<VertexKey, List<VertexKey>>();
            Dictionary<VertexKey, Vector3> positions =
                new Dictionary<VertexKey, Vector3>();
            int boundaryEdgeCount = 0;
            foreach (KeyValuePair<TopologyEdgeKey, List<int>> pair in owners)
            {
                int selectedOwners = 0;
                for (int index = 0; index < pair.Value.Count; index++)
                {
                    selectedOwners += selected.Contains(pair.Value[index])
                        ? 1
                        : 0;
                }
                if (selectedOwners == 0 ||
                    selectedOwners == pair.Value.Count)
                {
                    continue;
                }
                if (pair.Value.Count != 2 || selectedOwners != 1)
                {
                    blocker =
                        "endpoint-cell selected source faces exposed an open or non-manifold boundary";
                    return false;
                }
                TopologyEdgeSegment segment = segments[pair.Key];
                AddPlaneCutEndpointPatchBoundaryNeighbor(
                    adjacency,
                    segment.StartKey,
                    segment.EndKey);
                AddPlaneCutEndpointPatchBoundaryNeighbor(
                    adjacency,
                    segment.EndKey,
                    segment.StartKey);
                positions[segment.StartKey] = segment.Start;
                positions[segment.EndKey] = segment.End;
                boundaryEdgeCount++;
            }
            if (!TryOrderPlaneCutEndpointPatchBoundary(
                    adjacency,
                    positions,
                    out loop))
            {
                blocker =
                    "endpoint-cell selected source faces did not form one closed authoritative boundary";
                return false;
            }
            topologySignature =
                "faces=" + selected.Count +
                ";edges=" + boundaryEdgeCount +
                ";vertices=" + loop.Length;
            positionSignature =
                BuildPlaneCutEndpointPatchLoopSignature(loop);
            return true;
        }

        private static string BuildPlaneCutEndpointCellPointSetSignature(
            List<Vector3> points)
        {
            if (points == null || points.Count == 0)
            {
                return "none";
            }
            List<string> signatures = new List<string>();
            for (int index = 0; index < points.Count; index++)
            {
                signatures.Add(
                    BuildPlaneCutEndpointPatchPointSignature(points[index]));
            }
            signatures.Sort(StringComparer.Ordinal);
            return string.Join("/", signatures);
        }

        private static PolygonFace ClonePlaneCutPolygonFace(
            PolygonFace face)
        {
            return new PolygonFace(
                new List<Vector3>(face.Vertices),
                face.Normal,
                face.Feature,
                face.FeatureStrength,
                face.ProvenanceKind,
                face.ProvenanceIndex);
        }

        private static string BuildPlaneCutEndpointPatchFaceSignature(
            PolygonFace face)
        {
            List<string> vertices = new List<string>();
            for (int index = 0; index < face.Vertices.Count; index++)
            {
                vertices.Add(
                    BuildPlaneCutEndpointPatchPointSignature(
                        face.Vertices[index]));
            }
            vertices.Sort(StringComparer.Ordinal);
            return ((int)face.ProvenanceKind).ToString() + ":" +
                face.ProvenanceIndex.ToString() + ":" +
                ((int)face.Feature).ToString() + ":" +
                Mathf.RoundToInt(
                    face.FeatureStrength * 100000f).ToString() + ":" +
                BuildPlaneCutEndpointPatchPointSignature(face.Normal) +
                ":" + string.Join(",", vertices);
        }

        private static string BuildPlaneCutEndpointPatchLoopSignature(
            Vector3[] loop)
        {
            if (loop == null || loop.Length == 0)
            {
                return "none";
            }
            string[] points = new string[loop.Length];
            for (int index = 0; index < loop.Length; index++)
            {
                points[index] =
                    BuildPlaneCutEndpointPatchPointSignature(loop[index]);
            }
            return string.Join("/", points);
        }

        private static string BuildPlaneCutEndpointPatchPointSignature(
            Vector3 point)
        {
            return Mathf.RoundToInt(point.x * 100000f).ToString() + "," +
                Mathf.RoundToInt(point.y * 100000f).ToString() + "," +
                Mathf.RoundToInt(point.z * 100000f).ToString();
        }

        private static bool DoPlaneCutEndpointPatchReplacementsMatch(
            PlaneCutEndpointPatchReplacement prepared,
            PlaneCutEndpointPatchReplacement minimum)
        {
            if (prepared == null || minimum == null ||
                prepared.VertexIndex != minimum.VertexIndex ||
                prepared.IncidentSourceEdgeIndices.Length !=
                    minimum.IncidentSourceEdgeIndices.Length ||
                prepared.SyntheticIncidentSourceEdgeIndices.Length !=
                    minimum.SyntheticIncidentSourceEdgeIndices.Length ||
                prepared.SelectedFaceCount != minimum.SelectedFaceCount ||
                prepared.BoundaryVertexCount != minimum.BoundaryVertexCount ||
                prepared.CapVertexCount != minimum.CapVertexCount ||
                prepared.FacesSubdivided != minimum.FacesSubdivided ||
                prepared.LocalFragmentCount !=
                    minimum.LocalFragmentCount ||
                prepared.RemoteRemainderCount !=
                    minimum.RemoteRemainderCount ||
                prepared.CellVertexCount != minimum.CellVertexCount ||
                prepared.CellFaceCount != minimum.CellFaceCount ||
                !string.Equals(
                    prepared.SelectedProvenanceSignature,
                    minimum.SelectedProvenanceSignature,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    prepared.BoundaryTopologySignature,
                    minimum.BoundaryTopologySignature,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    prepared.CellLimitSignature,
                    minimum.CellLimitSignature,
                    StringComparison.Ordinal))
            {
                return false;
            }
            for (int index = 0;
                 index < prepared.IncidentSourceEdgeIndices.Length;
                 index++)
            {
                if (prepared.IncidentSourceEdgeIndices[index] !=
                    minimum.IncidentSourceEdgeIndices[index])
                {
                    return false;
                }
            }
            for (int index = 0;
                 index < prepared.SyntheticIncidentSourceEdgeIndices.Length;
                 index++)
            {
                if (prepared.SyntheticIncidentSourceEdgeIndices[index] !=
                    minimum.SyntheticIncidentSourceEdgeIndices[index])
                {
                    return false;
                }
            }
            return true;
        }

        private static bool TryVerifyPlaneCutEndpointPatchStitchBoundary(
            List<PolygonFace> faces,
            HashSet<int> selected,
            string expectedTopologySignature,
            string expectedPositionSignature,
            out string blocker)
        {
            blocker = string.Empty;
            Dictionary<TopologyEdgeKey, List<int>> owners =
                new Dictionary<TopologyEdgeKey, List<int>>();
            Dictionary<TopologyEdgeKey, TopologyEdgeSegment> segments =
                new Dictionary<TopologyEdgeKey, TopologyEdgeSegment>();
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                for (int edgeIndex = 0;
                     edgeIndex < face.Vertices.Count;
                     edgeIndex++)
                {
                    Vector3 start = face.Vertices[edgeIndex];
                    Vector3 end = face.Vertices[
                        (edgeIndex + 1) % face.Vertices.Count];
                    VertexKey startKey = new VertexKey(start);
                    VertexKey endKey = new VertexKey(end);
                    TopologyEdgeKey key = new TopologyEdgeKey(
                        startKey,
                        endKey);
                    if (!owners.TryGetValue(
                            key,
                            out List<int> edgeOwners))
                    {
                        edgeOwners = new List<int>();
                        owners.Add(key, edgeOwners);
                        segments.Add(
                            key,
                            new TopologyEdgeSegment(
                                start,
                                end,
                                startKey,
                                endKey));
                    }
                    edgeOwners.Add(faceIndex);
                }
            }

            Dictionary<VertexKey, List<VertexKey>> adjacency =
                new Dictionary<VertexKey, List<VertexKey>>();
            Dictionary<VertexKey, Vector3> positions =
                new Dictionary<VertexKey, Vector3>();
            int boundaryEdgeCount = 0;
            foreach (KeyValuePair<TopologyEdgeKey, List<int>> pair in owners)
            {
                int selectedOwners = 0;
                for (int index = 0; index < pair.Value.Count; index++)
                {
                    selectedOwners += selected.Contains(pair.Value[index])
                        ? 1
                        : 0;
                }
                if (selectedOwners == 0 ||
                    selectedOwners == pair.Value.Count)
                {
                    continue;
                }
                if (pair.Value.Count != 2 || selectedOwners != 1)
                {
                    blocker =
                        "authoritative endpoint patch stitch boundary became open or non-manifold";
                    return false;
                }
                TopologyEdgeSegment segment = segments[pair.Key];
                AddPlaneCutEndpointPatchBoundaryNeighbor(
                    adjacency,
                    segment.StartKey,
                    segment.EndKey);
                AddPlaneCutEndpointPatchBoundaryNeighbor(
                    adjacency,
                    segment.EndKey,
                    segment.StartKey);
                positions[segment.StartKey] = segment.Start;
                positions[segment.EndKey] = segment.End;
                boundaryEdgeCount++;
            }
            if (!TryOrderPlaneCutEndpointPatchBoundary(
                    adjacency,
                    positions,
                    out Vector3[] loop))
            {
                blocker =
                    "authoritative endpoint patch stitch boundary no longer formed one closed loop";
                return false;
            }
            string topologySignature =
                "faces=" + selected.Count +
                ";edges=" + boundaryEdgeCount +
                ";vertices=" + loop.Length;
            string positionSignature =
                BuildPlaneCutEndpointPatchLoopSignature(loop);
            if (!string.Equals(
                    topologySignature,
                    expectedTopologySignature,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    positionSignature,
                    expectedPositionSignature,
                    StringComparison.Ordinal))
            {
                blocker =
                    "authoritative endpoint patch stitch-boundary signature did not match preparation";
                return false;
            }
            return true;
        }

        private static bool DoesPlaneCutEndpointCellReplacementSignatureMatch(
            PlaneCutEndpointPatchReplacement replacement)
        {
            if (replacement == null || replacement.ReplacementFaces == null)
            {
                return false;
            }
            List<string> expected = new List<string>();
            if (!string.IsNullOrEmpty(replacement.LocalFragmentSignature))
            {
                expected.AddRange(
                    replacement.LocalFragmentSignature.Split('/'));
            }
            if (!string.IsNullOrEmpty(replacement.RemoteRemainderSignature))
            {
                expected.AddRange(
                    replacement.RemoteRemainderSignature.Split('/'));
            }
            List<string> actual = new List<string>();
            int capCount = 0;
            for (int index = 0;
                 index < replacement.ReplacementFaces.Count;
                 index++)
            {
                PolygonFace face = replacement.ReplacementFaces[index];
                if (face.ProvenanceKind ==
                        PolygonFaceProvenanceKind.BoundedEndpointCap &&
                    face.ProvenanceIndex == replacement.VertexIndex)
                {
                    capCount++;
                    continue;
                }
                actual.Add(
                    BuildPlaneCutEndpointPatchFaceSignature(face));
            }
            expected.RemoveAll(string.IsNullOrEmpty);
            expected.Sort(StringComparer.Ordinal);
            actual.Sort(StringComparer.Ordinal);
            if (capCount != 1 || expected.Count != actual.Count)
            {
                return false;
            }
            for (int index = 0; index < expected.Count; index++)
            {
                if (!string.Equals(
                        expected[index],
                        actual[index],
                        StringComparison.Ordinal))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool TryApplyPlaneCutEndpointPatchReplacement(
            List<PolygonFace> faces,
            PlaneCutEndpointPatchReplacement replacement,
            out string blocker)
        {
            blocker = string.Empty;
            if (replacement == null)
            {
                return true;
            }
            if (faces == null || replacement.ReplacementFaces == null ||
                replacement.SelectedFaceSignatures == null ||
                replacement.SelectedFaceSignatures.Length == 0 ||
                replacement.CellLimits == null ||
                replacement.CellLimits.Length == 0 ||
                string.IsNullOrEmpty(replacement.CellLimitSignature) ||
                string.IsNullOrEmpty(replacement.LocalFragmentSignature) ||
                replacement.CellFaceCount !=
                    replacement.ReplacementFaces.Count ||
                replacement.SyntheticIncidentSourceEdgeIndices == null ||
                replacement.SyntheticIncidentFragmentCount !=
                    replacement.SyntheticIncidentSourceEdgeIndices.Length ||
                !string.Equals(
                    replacement.CellLimitSignature,
                    BuildPlaneCutEndpointCellLimitSignature(
                        replacement.CellLimits),
                    StringComparison.Ordinal) ||
                !DoesPlaneCutEndpointCellReplacementSignatureMatch(
                    replacement))
            {
                blocker =
                    "prepared endpoint-cell replacement was incomplete or its stored subface signature changed";
                return false;
            }

            Dictionary<string, Queue<int>> bySignature =
                new Dictionary<string, Queue<int>>(StringComparer.Ordinal);
            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                string signature =
                    BuildPlaneCutEndpointPatchFaceSignature(faces[faceIndex]);
                if (!bySignature.TryGetValue(
                        signature,
                        out Queue<int> indices))
                {
                    indices = new Queue<int>();
                    bySignature.Add(signature, indices);
                }
                indices.Enqueue(faceIndex);
            }
            HashSet<int> selected = new HashSet<int>();
            for (int index = 0;
                 index < replacement.SelectedFaceSignatures.Length;
                 index++)
            {
                string signature =
                    replacement.SelectedFaceSignatures[index];
                if (!bySignature.TryGetValue(
                        signature,
                        out Queue<int> indices) ||
                    indices.Count == 0)
                {
                    blocker =
                        "authoritative endpoint patch selected-face signature did not match the prepared shell";
                    return false;
                }
                selected.Add(indices.Dequeue());
            }
            if (selected.Count != replacement.SelectedFaceCount)
            {
                blocker =
                    "authoritative endpoint patch selected-face cardinality changed";
                return false;
            }
            if (!TryVerifyPlaneCutEndpointPatchStitchBoundary(
                    faces,
                    selected,
                    replacement.BoundaryTopologySignature,
                    replacement.BoundaryPositionSignature,
                    out blocker))
            {
                return false;
            }

            List<PolygonFace> spliced = new List<PolygonFace>();
            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                if (!selected.Contains(faceIndex))
                {
                    spliced.Add(ClonePlaneCutPolygonFace(faces[faceIndex]));
                }
            }
            for (int faceIndex = 0;
                 faceIndex < replacement.ReplacementFaces.Count;
                 faceIndex++)
            {
                spliced.Add(ClonePlaneCutPolygonFace(
                    replacement.ReplacementFaces[faceIndex]));
            }
            if (!TryFindSinglePlaneCutProvenanceFace(
                    spliced,
                    PolygonFaceProvenanceKind.BoundedEndpointCap,
                    replacement.VertexIndex,
                    out _))
            {
                blocker =
                    "authoritative endpoint patch splice emitted no unique bounded cap";
                return false;
            }
            faces.Clear();
            faces.AddRange(spliced);
            return true;
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
