using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                bool fragmentAware =
                    ShouldUsePlaneCutFragmentAwareBandCertification();
                bool validOwnedBand = edgeFaces.Count == 1;
                string ownedBandBlocker = string.Empty;
                if (fragmentAware && edgeFaces.Count > 0)
                {
                    validOwnedBand =
                        TryValidatePlaneCutOwnedBevelBandSet(
                            edgeFaces,
                            edge,
                            out ownedBandBlocker);
                }
                if (!validOwnedBand)
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
                            : !string.IsNullOrEmpty(ownedBandBlocker)
                                ? ownedBandBlocker
                                : "bevel-band edge " +
                                    edge.SourceEdgeIndex +
                                    " split into " + edgeFaces.Count +
                                    " owned faces";
                    }
                    continue;
                }
                if (edgeFaces.Count == 1)
                {
                    result.BandSingleFaceCount++;
                }
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
                for (int ownedFaceIndex = 0;
                     ownedFaceIndex < edgeFaces.Count;
                     ownedFaceIndex++)
                {
                    PolygonFace edgeFace = edgeFaces[ownedFaceIndex];
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
                            coverageRatio.ToString("G4") +
                            "; trace={" +
                            BuildPlaneCutCollapsedBandTrace(
                                faces,
                                edgeFaces,
                                edge,
                                sourceA,
                                sourceB,
                                minimumParameter,
                                maximumParameter) +
                            "}";
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

                for (int ownedFaceIndex = 0;
                     ownedFaceIndex < edgeFaces.Count;
                     ownedFaceIndex++)
                {
                    PolygonFace edgeFace = edgeFaces[ownedFaceIndex];
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
                                PolygonFaceProvenanceKind.EdgeBevelPlane &&
                            adjacent.ProvenanceIndex == edge.SourceEdgeIndex)
                        {
                            continue;
                        }
                        if ((adjacent.ProvenanceKind ==
                                 PolygonFaceProvenanceKind.
                                     VertexJunctionPlane ||
                             adjacent.ProvenanceKind ==
                                 PolygonFaceProvenanceKind.
                                     BoundedEndpointCap) &&
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
                            midpointParameter <
                                1f - endpointAllowance)
                        {
                            edgeForeignCut = true;
                            edgeInterrupted = true;
                            int foreignEdgeIndex =
                                adjacent.ProvenanceKind ==
                                    PolygonFaceProvenanceKind.
                                        EdgeBevelPlane
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
                                Mathf.Abs(
                                    endParameter - startParameter));
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

        private static bool
            ShouldUsePlaneCutFragmentAwareBandCertification()
        {
            if (!IsCornerDamageRecoveryTournamentActive())
            {
                return false;
            }
            CornerDamageRecoveryTournamentConfiguration configuration =
                ResolveCornerDamageRecoveryTournamentConfiguration();
            return configuration.Strategy !=
                CornerDamageRecoveryTournamentStrategy.
                    LegacyBoundedEndpointCell;
        }

        private static bool TryValidatePlaneCutOwnedBevelBandSet(
            List<PolygonFace> edgeFaces,
            PlaneCutBevelCandidate edge,
            out string blocker)
        {
            blocker = string.Empty;
            if (edgeFaces == null || edgeFaces.Count == 0)
            {
                blocker = "fragment-aware bevel band had no owned faces";
                return false;
            }

            HashSet<string> signatures = new HashSet<string>(
                StringComparer.Ordinal);
            Dictionary<EdgeKey, List<int>> owners =
                new Dictionary<EdgeKey, List<int>>();
            Dictionary<EdgeKey,
                List<KeyValuePair<VertexKey, VertexKey>>> directions =
                    new Dictionary<EdgeKey,
                        List<KeyValuePair<VertexKey, VertexKey>>>();
            float tolerance = Mathf.Max(
                edge.PlaneTolerance,
                PointMergeDistance * 4f);
            for (int faceIndex = 0;
                 faceIndex < edgeFaces.Count;
                 faceIndex++)
            {
                PolygonFace face = edgeFaces[faceIndex];
                if (face == null || face.Vertices == null ||
                    face.Vertices.Count < 3 ||
                    Vector3.Dot(face.Normal, edge.Plane.Normal) < 0.999f)
                {
                    blocker = "bevel-band edge " +
                        edge.SourceEdgeIndex +
                        " retained a non-coplanar or degenerate fragment";
                    return false;
                }
                string signature =
                    BuildPlaneCutEndpointPatchFaceSignature(face);
                if (!signatures.Add(signature))
                {
                    blocker = "bevel-band edge " +
                        edge.SourceEdgeIndex +
                        " retained duplicate polygon fragments";
                    return false;
                }
                for (int vertexIndex = 0;
                     vertexIndex < face.Vertices.Count;
                     vertexIndex++)
                {
                    if (Mathf.Abs(edge.Plane.SignedDistance(
                            face.Vertices[vertexIndex])) > tolerance)
                    {
                        blocker = "bevel-band edge " +
                            edge.SourceEdgeIndex +
                            " retained a fragment outside its defining plane";
                        return false;
                    }
                    EdgeKey key = new EdgeKey(
                        face.Vertices[vertexIndex],
                        face.Vertices[
                            (vertexIndex + 1) % face.Vertices.Count]);
                    if (!owners.TryGetValue(key, out List<int> list))
                    {
                        list = new List<int>();
                        owners.Add(key, list);
                        directions.Add(
                            key,
                            new List<KeyValuePair<
                                VertexKey, VertexKey>>());
                    }
                    list.Add(faceIndex);
                    directions[key].Add(
                        new KeyValuePair<VertexKey, VertexKey>(
                            new VertexKey(face.Vertices[vertexIndex]),
                            new VertexKey(face.Vertices[
                                (vertexIndex + 1) %
                                    face.Vertices.Count])));
                }
            }

            List<HashSet<int>> adjacency = new List<HashSet<int>>(
                edgeFaces.Count);
            for (int faceIndex = 0;
                 faceIndex < edgeFaces.Count;
                 faceIndex++)
            {
                adjacency.Add(new HashSet<int>());
            }
            foreach (KeyValuePair<EdgeKey, List<int>> pair in owners)
            {
                if (pair.Value.Count > 2)
                {
                    blocker = "bevel-band edge " +
                        edge.SourceEdgeIndex +
                        " retained a non-manifold internal fragment edge";
                    return false;
                }
                if (pair.Value.Count == 2)
                {
                    if (pair.Value[0] == pair.Value[1])
                    {
                        blocker = "bevel-band edge " +
                            edge.SourceEdgeIndex +
                            " retained a repeated edge inside one fragment";
                        return false;
                    }
                    List<KeyValuePair<VertexKey, VertexKey>>
                        edgeDirections = directions[pair.Key];
                    if (edgeDirections.Count != 2 ||
                        !edgeDirections[0].Key.Equals(
                            edgeDirections[1].Value) ||
                        !edgeDirections[0].Value.Equals(
                            edgeDirections[1].Key))
                    {
                        blocker = "bevel-band edge " +
                            edge.SourceEdgeIndex +
                            " retained a same-winding internal fragment seam";
                        return false;
                    }
                    adjacency[pair.Value[0]].Add(pair.Value[1]);
                    adjacency[pair.Value[1]].Add(pair.Value[0]);
                }
            }

            HashSet<int> visited = new HashSet<int>();
            Queue<int> queue = new Queue<int>();
            int componentCount = 0;
            for (int seed = 0; seed < edgeFaces.Count; seed++)
            {
                if (!visited.Add(seed))
                {
                    continue;
                }
                componentCount++;
                queue.Enqueue(seed);
                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    foreach (int neighbor in adjacency[current])
                    {
                        if (visited.Add(neighbor))
                        {
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }
            if (componentCount != 1)
            {
                blocker = "bevel-band edge " +
                    edge.SourceEdgeIndex +
                    " split into " + componentCount +
                    " disconnected fragment components";
                return false;
            }
            return true;
        }

        private static string BuildPlaneCutCollapsedBandTrace(
            List<PolygonFace> allFaces,
            List<PolygonFace> ownedFaces,
            PlaneCutBevelCandidate edge,
            Vector3 sourceA,
            Vector3 sourceB,
            float minimumParameter,
            float maximumParameter)
        {
            StringBuilder builder = new StringBuilder();
            Vector3 axis = sourceB - sourceA;
            builder.Append("sourceA=");
            builder.Append(sourceA.ToString("G9"));
            builder.Append(",sourceB=");
            builder.Append(sourceB.ToString("G9"));
            builder.Append(",sourceLength=");
            builder.Append(axis.magnitude.ToString("G9"));
            builder.Append(",retainedInterval=");
            builder.Append(minimumParameter.ToString("G9"));
            builder.Append("..");
            builder.Append(maximumParameter.ToString("G9"));
            builder.Append(",ownedVertices={");
            if (ownedFaces == null || ownedFaces.Count == 0)
            {
                builder.Append("none");
            }
            else
            {
                bool firstVertex = true;
                for (int faceIndex = 0;
                     faceIndex < ownedFaces.Count;
                     faceIndex++)
                {
                    PolygonFace face = ownedFaces[faceIndex];
                    if (face == null || face.Vertices == null)
                    {
                        continue;
                    }
                    for (int vertexIndex = 0;
                         vertexIndex < face.Vertices.Count;
                         vertexIndex++)
                    {
                        if (!firstVertex)
                        {
                            builder.Append('/');
                        }
                        firstVertex = false;
                        Vector3 vertex = face.Vertices[vertexIndex];
                        float parameter = axis.sqrMagnitude <=
                                PointMergeDistance * PointMergeDistance
                            ? 0f
                            : Vector3.Dot(vertex - sourceA, axis) /
                                axis.sqrMagnitude;
                        builder.Append(vertex.ToString("G9"));
                        builder.Append('@');
                        builder.Append(parameter.ToString("G9"));
                    }
                }
            }
            builder.Append("},intersectingGeneratedPlanes={");
            bool firstPlane = true;
            if (allFaces != null && axis.sqrMagnitude >
                PointMergeDistance * PointMergeDistance)
            {
                for (int faceIndex = 0;
                     faceIndex < allFaces.Count;
                     faceIndex++)
                {
                    PolygonFace face = allFaces[faceIndex];
                    if (face == null || face.Vertices == null ||
                        face.Vertices.Count == 0 ||
                        (face.ProvenanceKind ==
                            PolygonFaceProvenanceKind.EdgeBevelPlane &&
                         face.ProvenanceIndex == edge.SourceEdgeIndex))
                    {
                        continue;
                    }
                    float denominator = Vector3.Dot(face.Normal, axis);
                    if (Mathf.Abs(denominator) <= 1e-7f)
                    {
                        continue;
                    }
                    float parameter = Vector3.Dot(
                        face.Normal,
                        face.Vertices[0] - sourceA) / denominator;
                    if (parameter < -0.1f || parameter > 1.1f)
                    {
                        continue;
                    }
                    if (!firstPlane)
                    {
                        builder.Append('/');
                    }
                    firstPlane = false;
                    builder.Append(face.ProvenanceKind);
                    builder.Append(':');
                    builder.Append(face.ProvenanceIndex);
                    builder.Append('@');
                    builder.Append(parameter.ToString("G9"));
                    builder.Append("[n=");
                    builder.Append(face.Normal.ToString("G9"));
                    builder.Append("]");
                }
            }
            if (firstPlane)
            {
                builder.Append("none");
            }
            builder.Append('}');
            return builder.ToString();
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

        private static string BuildPlaneCutTopologyFailureDiagnostic(
            List<PolygonFace> faces,
            EdgeWearTopologyStats topology)
        {
            Dictionary<EdgeKey, List<int>> edgeOwners =
                new Dictionary<EdgeKey, List<int>>();
            Dictionary<EdgeKey, string> edgeSignatures =
                new Dictionary<EdgeKey, string>();
            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                for (int edgeIndex = 0;
                     edgeIndex < face.Vertices.Count;
                     edgeIndex++)
                {
                    Vector3 start = face.Vertices[edgeIndex];
                    Vector3 end = face.Vertices[
                        (edgeIndex + 1) % face.Vertices.Count];
                    EdgeKey key = new EdgeKey(start, end);
                    if (!edgeOwners.TryGetValue(
                            key,
                            out List<int> owners))
                    {
                        owners = new List<int>();
                        edgeOwners.Add(key, owners);
                        string startSignature =
                            BuildPlaneCutEndpointPatchPointSignature(start);
                        string endSignature =
                            BuildPlaneCutEndpointPatchPointSignature(end);
                        edgeSignatures.Add(
                            key,
                            string.CompareOrdinal(
                                    startSignature,
                                    endSignature) <= 0
                                ? startSignature + "->" + endSignature
                                : endSignature + "->" + startSignature);
                    }
                    owners.Add(faceIndex);
                }
            }

            List<KeyValuePair<EdgeKey, List<int>>> defects =
                edgeOwners.Where(pair => pair.Value.Count != 2).
                    OrderBy(pair => edgeSignatures[pair.Key],
                        StringComparer.Ordinal).
                    ToList();
            string firstDefect = "none";
            if (defects.Count > 0)
            {
                KeyValuePair<EdgeKey, List<int>> defect = defects[0];
                List<string> ownerSignatures = defect.Value.Select(
                    faceIndex =>
                        ((int)faces[faceIndex].ProvenanceKind).ToString() +
                        ":" +
                        faces[faceIndex].ProvenanceIndex.ToString()).
                    OrderBy(value => value, StringComparer.Ordinal).
                    ToList();
                firstDefect = edgeSignatures[defect.Key] +
                    "@owners=" + defect.Value.Count +
                    "@faces=" + string.Join("/", ownerSignatures);
            }
            else if (topology.TJunctionCount > 0)
            {
                firstDefect = "t-junction-vertex-unresolved";
            }
            return "topology(open=" + topology.OpenEdgeCount +
                ",nonManifold=" + topology.NonManifoldEdgeCount +
                ",tJunction=" + topology.TJunctionCount +
                ",first=" + firstDefect + ")";
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

        private static bool TryPrepareCornerDamageRecoveryTournamentStrategy(
            CornerDamageIntegrationPlan plan,
            List<PlaneCutBevelCandidate> preparedCandidates,
            List<PlaneCutBevelCandidate> minimumCandidates,
            PlaneCutBevelCandidate victim,
            PlaneCutBevelCandidate foreign,
            int conflictVertexIndex,
            CornerDamageRecoveryTournamentConfiguration configuration)
        {
            if (configuration.Strategy ==
                CornerDamageRecoveryTournamentStrategy.
                    LegacyBoundedEndpointCell)
            {
                return TryPrepareCornerDamageEndpointPatchRecovery(
                    plan,
                    preparedCandidates,
                    minimumCandidates,
                    victim,
                    foreign,
                    conflictVertexIndex);
            }

            PlaneCutBevelTerminationOwnership ownership =
                PlaneCutBevelTerminationOwnership.EndpointStar;
            PlaneCutBevelTerminationClosure closure =
                PlaneCutBevelTerminationClosure.ConformingNormalizedCavity;
            PlaneCutBevelTerminationPreconditioner preconditioner =
                PlaneCutBevelTerminationPreconditioner.None;
            PlaneCutRemoteComponentSelection remoteSelection =
                PlaneCutRemoteComponentSelection.FurthestAxialReach;
            float taperTipFraction = 0.35f;
            float primaryWidthScale = 1f;
            float favoredWidthScale = 1f;
            int selectiveIdentityMode = -1;
            int widthFavoredIdentityMode = -1;
            bool widthScaleSelectedOnly = false;
            bool selectAllExceptIdentity = false;
            bool allowSingleIncident = false;
            bool fragmentAwareBandCertification = true;
            bool ownLimitIncidentPartition = false;
            bool requireSimpleClosureCycles = false;
            bool directSimpleCycleTriangles = false;
            bool conformBeforeClosureDecision = false;
            bool postClosureFixedPointConformance = false;

            switch (configuration.Strategy)
            {
                case CornerDamageRecoveryTournamentStrategy.
                    RemoteComponentConforming:
                    remoteSelection = configuration.VariantIndex == 1
                        ? PlaneCutRemoteComponentSelection.LargestArea
                        : configuration.VariantIndex == 2
                            ? PlaneCutRemoteComponentSelection.
                                NearestRemoteEndpoint
                            : PlaneCutRemoteComponentSelection.
                                FurthestAxialReach;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    RemoteComponentFixedPoint:
                    remoteSelection = configuration.VariantIndex == 1
                        ? PlaneCutRemoteComponentSelection.LargestArea
                        : configuration.VariantIndex == 2
                            ? PlaneCutRemoteComponentSelection.
                                NearestRemoteEndpoint
                            : PlaneCutRemoteComponentSelection.
                                FurthestAxialReach;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    RemoteComponentSimpleCycleFixedPoint:
                    requireSimpleClosureCycles = true;
                    directSimpleCycleTriangles = true;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    RemoteComponentSourceStripsFixedPoint:
                    closure = PlaneCutBevelTerminationClosure.
                        SourceFaceTransitionStrips;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    RemoteComponentHalfEdgeFixedPoint:
                    closure = PlaneCutBevelTerminationClosure.
                        OrientedHalfEdgeCavity;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    RemoteComponentCellFanFixedPoint:
                    closure = PlaneCutBevelTerminationClosure.
                        BoundaryEdgeCellFan;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    RemoteComponentAxialTransitionFixedPoint:
                    closure = PlaneCutBevelTerminationClosure.
                        AxialCapsAndTransitionLoops;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    RemoteComponentTaperTransitionFixedPoint:
                    closure = PlaneCutBevelTerminationClosure.
                        TaperFansAndTransitionLoops;
                    taperTipFraction = Mathf.Clamp(
                        configuration.PrimaryParameter,
                        0.05f,
                        0.95f);
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    RemoteComponentRawEdgeFanFixedPoint:
                    closure = PlaneCutBevelTerminationClosure.
                        RawEdgeCavityFan;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    OwnLimitFixedPoint:
                    remoteSelection = PlaneCutRemoteComponentSelection.None;
                    ownLimitIncidentPartition = true;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    OwnLimitSimpleCycleFixedPoint:
                    remoteSelection = PlaneCutRemoteComponentSelection.None;
                    ownLimitIncidentPartition = true;
                    requireSimpleClosureCycles = true;
                    directSimpleCycleTriangles = true;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    OwnLimitSourceStripsFixedPoint:
                    remoteSelection = PlaneCutRemoteComponentSelection.None;
                    ownLimitIncidentPartition = true;
                    closure = PlaneCutBevelTerminationClosure.
                        SourceFaceTransitionStrips;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    OwnLimitHalfEdgeFixedPoint:
                    remoteSelection = PlaneCutRemoteComponentSelection.None;
                    ownLimitIncidentPartition = true;
                    closure = PlaneCutBevelTerminationClosure.
                        OrientedHalfEdgeCavity;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    OwnLimitCellFanFixedPoint:
                    remoteSelection = PlaneCutRemoteComponentSelection.None;
                    ownLimitIncidentPartition = true;
                    closure = PlaneCutBevelTerminationClosure.
                        BoundaryEdgeCellFan;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    OwnLimitRawEdgeFanFixedPoint:
                    remoteSelection = PlaneCutRemoteComponentSelection.None;
                    ownLimitIncidentPartition = true;
                    closure = PlaneCutBevelTerminationClosure.
                        RawEdgeCavityFan;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    OwnLimitAxialTransitionFixedPoint:
                    remoteSelection = PlaneCutRemoteComponentSelection.None;
                    ownLimitIncidentPartition = true;
                    closure = PlaneCutBevelTerminationClosure.
                        AxialCapsAndTransitionLoops;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    OwnLimitTaperTransitionFixedPoint:
                    remoteSelection = PlaneCutRemoteComponentSelection.None;
                    ownLimitIncidentPartition = true;
                    closure = PlaneCutBevelTerminationClosure.
                        TaperFansAndTransitionLoops;
                    taperTipFraction = Mathf.Clamp(
                        configuration.PrimaryParameter,
                        0.05f,
                        0.95f);
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    WidthPreconditionedRemoteComponentFixedPoint:
                    preconditioner =
                        PlaneCutBevelTerminationPreconditioner.
                            WidthRedistribution;
                    primaryWidthScale = Mathf.Clamp(
                        configuration.PrimaryParameter,
                        0.05f,
                        1f);
                    favoredWidthScale = Mathf.Clamp(
                        configuration.SecondaryParameter,
                        primaryWidthScale,
                        1f);
                    widthScaleSelectedOnly =
                        configuration.VariantIndex >= 10;
                    widthFavoredIdentityMode = widthScaleSelectedOnly
                        ? configuration.VariantIndex - 10
                        : configuration.VariantIndex;
                    requireSimpleClosureCycles = true;
                    directSimpleCycleTriangles = true;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    SingleBandSuppressionFixedPoint:
                    selectiveIdentityMode = 2 +
                        Mathf.Max(0, configuration.VariantIndex);
                    allowSingleIncident = true;
                    requireSimpleClosureCycles = true;
                    directSimpleCycleTriangles = true;
                    conformBeforeClosureDecision = true;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    AllButOneBandSuppressionFixedPoint:
                    selectiveIdentityMode = 2 +
                        Mathf.Max(0, configuration.VariantIndex);
                    selectAllExceptIdentity = true;
                    allowSingleIncident = true;
                    requireSimpleClosureCycles = true;
                    directSimpleCycleTriangles = true;
                    conformBeforeClosureDecision = true;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    AllBandSuppressionFixedPoint:
                    selectiveIdentityMode = -1;
                    allowSingleIncident = true;
                    requireSimpleClosureCycles = true;
                    directSimpleCycleTriangles = true;
                    conformBeforeClosureDecision = true;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    GeometricCellRemoteComponentFixedPoint:
                    ownership =
                        PlaneCutBevelTerminationOwnership.GeometricCell;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    GeometricCellRemoteComponentSimpleCycleFixedPoint:
                    ownership =
                        PlaneCutBevelTerminationOwnership.GeometricCell;
                    requireSimpleClosureCycles = true;
                    directSimpleCycleTriangles = true;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    GeometricCellRemoteComponentSourceStripsFixedPoint:
                    ownership =
                        PlaneCutBevelTerminationOwnership.GeometricCell;
                    closure = PlaneCutBevelTerminationClosure.
                        SourceFaceTransitionStrips;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    GeometricCellRemoteComponentHalfEdgeFixedPoint:
                    ownership =
                        PlaneCutBevelTerminationOwnership.GeometricCell;
                    closure = PlaneCutBevelTerminationClosure.
                        OrientedHalfEdgeCavity;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    GeometricCellRemoteComponentCellFanFixedPoint:
                    ownership =
                        PlaneCutBevelTerminationOwnership.GeometricCell;
                    closure = PlaneCutBevelTerminationClosure.
                        BoundaryEdgeCellFan;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    GeometricCellRemoteComponentRawEdgeFanFixedPoint:
                    ownership =
                        PlaneCutBevelTerminationOwnership.GeometricCell;
                    closure = PlaneCutBevelTerminationClosure.
                        RawEdgeCavityFan;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    GeometricCellRemoteComponentAxialTransitionFixedPoint:
                    ownership =
                        PlaneCutBevelTerminationOwnership.GeometricCell;
                    closure = PlaneCutBevelTerminationClosure.
                        AxialCapsAndTransitionLoops;
                    postClosureFixedPointConformance = true;
                    break;
                case CornerDamageRecoveryTournamentStrategy.
                    GeometricCellRemoteComponentTaperTransitionFixedPoint:
                    ownership =
                        PlaneCutBevelTerminationOwnership.GeometricCell;
                    closure = PlaneCutBevelTerminationClosure.
                        TaperFansAndTransitionLoops;
                    taperTipFraction = Mathf.Clamp(
                        configuration.PrimaryParameter,
                        0.05f,
                        0.95f);
                    postClosureFixedPointConformance = true;
                    break;
            }

            return TryPrepareCornerDamageBevelTerminationRecovery(
                plan,
                preparedCandidates,
                minimumCandidates,
                victim,
                foreign,
                conflictVertexIndex,
                new PlaneCutBevelTerminationOptions(
                    ownership,
                    closure,
                    preconditioner,
                    remoteSelection,
                    1f,
                    taperTipFraction,
                    primaryWidthScale,
                    favoredWidthScale,
                    selectiveIdentityMode,
                    widthFavoredIdentityMode,
                    widthScaleSelectedOnly,
                    selectAllExceptIdentity,
                    allowSingleIncident,
                    fragmentAwareBandCertification,
                    ownLimitIncidentPartition,
                    requireSimpleClosureCycles,
                    directSimpleCycleTriangles,
                    conformBeforeClosureDecision,
                    postClosureFixedPointConformance,
                    configuration.Name));
        }

        private static bool TryPrepareCornerDamageBevelTerminationRecovery(
            CornerDamageIntegrationPlan plan,
            List<PlaneCutBevelCandidate> preparedCandidates,
            List<PlaneCutBevelCandidate> minimumCandidates,
            PlaneCutBevelCandidate victim,
            PlaneCutBevelCandidate foreign,
            int conflictVertexIndex)
        {
            if (IsCornerDamageRecoveryTournamentActive())
            {
                return TryPrepareCornerDamageRecoveryTournamentStrategy(
                    plan,
                    preparedCandidates,
                    minimumCandidates,
                    victim,
                    foreign,
                    conflictVertexIndex,
                    ResolveCornerDamageRecoveryTournamentConfiguration());
            }
            return TryPrepareCornerDamageBevelTerminationRecovery(
                plan,
                preparedCandidates,
                minimumCandidates,
                victim,
                foreign,
                conflictVertexIndex,
                PlaneCutBevelTerminationOptions.Production);
        }

        private static bool TryPrepareCornerDamageBevelTerminationRecovery(
            CornerDamageIntegrationPlan plan,
            List<PlaneCutBevelCandidate> preparedCandidates,
            List<PlaneCutBevelCandidate> minimumCandidates,
            PlaneCutBevelCandidate victim,
            PlaneCutBevelCandidate foreign,
            int conflictVertexIndex,
            PlaneCutBevelTerminationOptions options)
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
                    conflictVertexIndex < 0 ||
                    conflictVertexIndex >=
                        solvedPlan.Context.Graph.Vertices.Count)
                {
                    RecordEndpointPatchRecoveryRejection(
                        plan,
                        PlaneCutEndpointPatchRejectionKind.PatchExtraction,
                        "conflict-local bevel termination was unavailable because prepared topology was incomplete");
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
                        "conflict-local bevel termination requires the victim and foreign bands to share the implicated endpoint");
                    return false;
                }

                List<PlaneCutBevelCandidate> effectivePreparedCandidates =
                    preparedCandidates;
                List<PlaneCutBevelCandidate> effectiveMinimumCandidates =
                    minimumCandidates;
                if (options.Preconditioner ==
                    PlaneCutBevelTerminationPreconditioner.WidthRedistribution)
                {
                    if (!TryBuildPlaneCutBoundaryTournamentRedistribution(
                            solvedPlan.Context,
                            preparedCandidates,
                            minimumCandidates,
                            sharedVertexIndex,
                            victim,
                            foreign,
                            options,
                            solvedPlan.MinimumStableEdgeLength,
                            out effectivePreparedCandidates,
                            out effectiveMinimumCandidates,
                            out string redistributionBlocker))
                    {
                        RecordEndpointPatchRecoveryRejection(
                            plan,
                            PlaneCutEndpointPatchRejectionKind.BandIntegrity,
                            redistributionBlocker);
                        return false;
                    }
                }

                List<PlaneCutBevelCandidate> preparedIncident =
                    GetActivePlaneCutIncidentCandidates(
                        effectivePreparedCandidates,
                        sharedVertexIndex);
                List<PlaneCutBevelCandidate> minimumIncident =
                    GetActivePlaneCutIncidentCandidates(
                        effectiveMinimumCandidates,
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
                        "conflict-local bevel termination supports one complete retained two/three-band endpoint star");
                    return false;
                }

                if (!TryResolvePlaneCutBevelTerminationIncidentSubset(
                        preparedIncident,
                        minimumIncident,
                        victim,
                        foreign,
                        options,
                        out List<PlaneCutBevelCandidate>
                            terminationPreparedIncident,
                        out List<PlaneCutBevelCandidate>
                            terminationMinimumIncident,
                        out string incidentSubsetBlocker))
                {
                    RecordEndpointPatchRecoveryRejection(
                        plan,
                        PlaneCutEndpointPatchRejectionKind.UnsupportedStar,
                        incidentSubsetBlocker);
                    return false;
                }
                plan.EndpointPatchRecoveryIncidentBandCount =
                    terminationPreparedIncident.Count;

                List<PlaneCutVertexJunctionCandidate> noJunctions =
                    new List<PlaneCutVertexJunctionCandidate>();
                if (!TryBuildPlaneCutSystemFaces(
                        solvedPlan.SourceFaces,
                        effectivePreparedCandidates,
                        noJunctions,
                        out List<PolygonFace> preparedFullFaces,
                        out _,
                        out string preparedFullBlocker,
                        new PlaneCutNumericalRepairTelemetry()))
                {
                    RecordEndpointPatchRecoveryRejection(
                        plan,
                        PlaneCutEndpointPatchRejectionKind.PatchExtraction,
                        "prepared ordinary shell was unavailable: " +
                        preparedFullBlocker);
                    return false;
                }
                if (!TryBuildPlaneCutSystemFaces(
                        solvedPlan.SourceFaces,
                        effectiveMinimumCandidates,
                        noJunctions,
                        out List<PolygonFace> minimumFullFaces,
                        out _,
                        out string minimumFullBlocker,
                        new PlaneCutNumericalRepairTelemetry()))
                {
                    RecordEndpointPatchRecoveryRejection(
                        plan,
                        PlaneCutEndpointPatchRejectionKind.PatchExtraction,
                        "legal-minimum ordinary shell was unavailable: " +
                        minimumFullBlocker);
                    return false;
                }

                HashSet<int> terminatedIdentities = new HashSet<int>();
                for (int index = 0; index < terminationPreparedIncident.Count; index++)
                {
                    terminatedIdentities.Add(
                        terminationPreparedIncident[index].SourceEdgeIndex);
                }
                List<PlaneCutBevelCandidate> preparedPocketCandidates =
                    BuildPlaneCutCandidatesExcludingIdentities(
                        effectivePreparedCandidates,
                        terminatedIdentities);
                List<PlaneCutBevelCandidate> minimumPocketCandidates =
                    BuildPlaneCutCandidatesExcludingIdentities(
                        effectiveMinimumCandidates,
                        terminatedIdentities);
                if (!TryBuildPlaneCutSystemFaces(
                        solvedPlan.SourceFaces,
                        preparedPocketCandidates,
                        noJunctions,
                        out List<PolygonFace> preparedPocketFaces,
                        out _,
                        out string preparedPocketBlocker,
                        new PlaneCutNumericalRepairTelemetry()))
                {
                    RecordEndpointPatchRecoveryRejection(
                        plan,
                        PlaneCutEndpointPatchRejectionKind.PatchExtraction,
                        "prepared endpoint source-pocket shell was unavailable: " +
                        preparedPocketBlocker);
                    return false;
                }
                if (!TryBuildPlaneCutSystemFaces(
                        solvedPlan.SourceFaces,
                        minimumPocketCandidates,
                        noJunctions,
                        out List<PolygonFace> minimumPocketFaces,
                        out _,
                        out string minimumPocketBlocker,
                        new PlaneCutNumericalRepairTelemetry()))
                {
                    RecordEndpointPatchRecoveryRejection(
                        plan,
                        PlaneCutEndpointPatchRejectionKind.PatchExtraction,
                        "legal-minimum endpoint source-pocket shell was unavailable: " +
                        minimumPocketBlocker);
                    return false;
                }

                if (!TryBuildPlaneCutBevelTerminationLimits(
                        solvedPlan.Context,
                        sharedVertexIndex,
                        terminationPreparedIncident,
                        solvedPlan.MinimumStableEdgeLength,
                        options,
                        out PlaneCutEndpointCellLimit[] limits,
                        out string limitSignature,
                        out string limitBlocker))
                {
                    RecordEndpointPatchRecoveryRejection(
                        plan,
                        PlaneCutEndpointPatchRejectionKind.Locality,
                        limitBlocker);
                    return false;
                }

                plan.EndpointPatchRecoveryTrialCount++;
                if (!TryBuildPlaneCutBevelTerminationReplacement(
                        preparedFullFaces,
                        preparedPocketFaces,
                        solvedPlan.Context,
                        effectivePreparedCandidates,
                        terminationPreparedIncident,
                        sharedVertexIndex,
                        limits,
                        limitSignature,
                        solvedPlan.MinimumStableEdgeLength,
                        solvedPlan.MinimumStableFaceArea,
                        options,
                        out PlaneCutEndpointPatchReplacement prepared,
                        out PlaneCutEndpointCellEvidence preparedEvidence,
                        out PlaneCutEndpointPatchRejectionKind rejection,
                        out string blocker))
                {
                    ApplyEndpointCellEvidence(plan, preparedEvidence);
                    RecordEndpointPatchRecoveryRejection(
                        plan,
                        rejection,
                        blocker);
                    return false;
                }
                ApplyEndpointCellEvidence(plan, preparedEvidence);

                if (!TryBuildPlaneCutBevelTerminationReplacement(
                        minimumFullFaces,
                        minimumPocketFaces,
                        solvedPlan.Context,
                        effectiveMinimumCandidates,
                        terminationMinimumIncident,
                        sharedVertexIndex,
                        limits,
                        limitSignature,
                        solvedPlan.MinimumStableEdgeLength,
                        solvedPlan.MinimumStableFaceArea,
                        options,
                        out PlaneCutEndpointPatchReplacement minimum,
                        out PlaneCutEndpointCellEvidence minimumEvidence,
                        out rejection,
                        out blocker))
                {
                    ApplyEndpointCellEvidence(plan, minimumEvidence);
                    RecordEndpointPatchRecoveryRejection(
                        plan,
                        rejection,
                        "legal-minimum termination rejected: " + blocker);
                    return false;
                }
                if (!DoPlaneCutBevelTerminationReplacementsMatch(
                        prepared,
                        minimum))
                {
                    RecordEndpointPatchRecoveryRejection(
                        plan,
                        PlaneCutEndpointPatchRejectionKind.PreparedMinimumParity,
                        "prepared and legal-minimum bevel terminations produced different identity, loop, or replacement topology");
                    return false;
                }

                solvedPlan.PreparedJunctions ??=
                    new List<PlaneCutVertexJunctionCandidate>();
                solvedPlan.PreparedJunctions.Clear();
                if (options.Preconditioner ==
                    PlaneCutBevelTerminationPreconditioner.WidthRedistribution)
                {
                    solvedPlan.RetainedCandidates =
                        effectivePreparedCandidates;
                }
                solvedPlan.PreparedEndpointPatch = prepared;
                plan.EndpointPatchRecoveryPrepared = true;
                plan.EndpointPatchRecoveryRejection =
                    PlaneCutEndpointPatchRejectionKind.None;
                plan.EndpointPatchRecoveryVertexIndex = sharedVertexIndex;
                plan.EndpointPatchRecoverySelectedFaceCount =
                    prepared.SelectedFaceCount;
                plan.EndpointPatchRecoveryBoundaryVertexCount =
                    prepared.BoundaryVertexCount;
                plan.EndpointPatchRecoveryBoundarySignature =
                    prepared.TerminationLoopSignature;
                plan.EndpointPatchRecoveryCellLimitSignature =
                    prepared.CellLimitSignature;
                plan.EndpointPatchRecoveryFacesSubdivided =
                    prepared.FacesSubdivided;
                plan.EndpointPatchRecoveryLocalFragmentCount =
                    prepared.RestoredPocketFaceCount;
                plan.EndpointPatchRecoveryRemoteRemainderCount =
                    prepared.RemoteRemainderCount;
                plan.EndpointPatchRecoveryCellFaceCount =
                    prepared.CellFaceCount;
                plan.EndpointPatchRecoveryCapVertexCount =
                    prepared.CapVertexCount;
                plan.EndpointPatchRecoveryDiagnostic =
                    (string.IsNullOrEmpty(options.StrategyName)
                        ? "conflict-local termination"
                        : options.StrategyName) +
                    " certified for incident edges {" +
                    string.Join("/", prepared.TerminatedSourceEdgeIndices) +
                    "}; remote bevel identities retained and endpoint source pocket restored";
                return true;
            }
            finally
            {
                stopwatch.Stop();
                plan.EndpointPatchRecoveryMilliseconds =
                    stopwatch.Elapsed.TotalMilliseconds;
            }
        }

        private static bool TryResolvePlaneCutBevelTerminationIncidentSubset(
            List<PlaneCutBevelCandidate> preparedIncident,
            List<PlaneCutBevelCandidate> minimumIncident,
            PlaneCutBevelCandidate victim,
            PlaneCutBevelCandidate foreign,
            PlaneCutBevelTerminationOptions options,
            out List<PlaneCutBevelCandidate> preparedSubset,
            out List<PlaneCutBevelCandidate> minimumSubset,
            out string blocker)
        {
            preparedSubset = new List<PlaneCutBevelCandidate>();
            minimumSubset = new List<PlaneCutBevelCandidate>();
            blocker = string.Empty;
            if (preparedIncident == null || minimumIncident == null ||
                preparedIncident.Count != minimumIncident.Count ||
                preparedIncident.Count == 0)
            {
                blocker = "termination incident subsets were unavailable";
                return false;
            }
            if (options.SelectiveIdentityMode < 0 &&
                options.SelectiveIdentityMode != -2)
            {
                preparedSubset.AddRange(preparedIncident);
                minimumSubset.AddRange(minimumIncident);
                return true;
            }

            List<PlaneCutBevelCandidate> ordered =
                new List<PlaneCutBevelCandidate>(preparedIncident);
            ordered.Sort((left, right) =>
                left.SourceEdgeIndex.CompareTo(right.SourceEdgeIndex));
            int selectedIdentity;
            if (options.SelectiveIdentityMode == -2)
            {
                PlaneCutBevelCandidate weakest = ordered[0];
                for (int index = 1; index < ordered.Count; index++)
                {
                    PlaneCutBevelCandidate candidate = ordered[index];
                    if (candidate.Width < weakest.Width - 0.0000001f ||
                        (Mathf.Abs(candidate.Width - weakest.Width) <=
                             0.0000001f &&
                         candidate.SelectionScore <
                             weakest.SelectionScore - 0.0000001f) ||
                        (Mathf.Abs(candidate.Width - weakest.Width) <=
                             0.0000001f &&
                         Mathf.Abs(candidate.SelectionScore -
                             weakest.SelectionScore) <= 0.0000001f &&
                         candidate.SourceEdgeIndex <
                             weakest.SourceEdgeIndex))
                    {
                        weakest = candidate;
                    }
                }
                selectedIdentity = weakest.SourceEdgeIndex;
            }
            else if (options.SelectiveIdentityMode == 0)
            {
                selectedIdentity = victim.SourceEdgeIndex;
            }
            else if (options.SelectiveIdentityMode == 1)
            {
                selectedIdentity = foreign.SourceEdgeIndex;
            }
            else
            {
                int ordinal = Mathf.Abs(
                    options.SelectiveIdentityMode - 2) %
                    ordered.Count;
                selectedIdentity = ordered[ordinal].SourceEdgeIndex;
            }
            if (!TryFindPlaneCutCandidateBySourceEdge(
                    preparedIncident,
                    selectedIdentity,
                    out PlaneCutBevelCandidate prepared) ||
                !TryFindPlaneCutCandidateBySourceEdge(
                    minimumIncident,
                    selectedIdentity,
                    out PlaneCutBevelCandidate minimum))
            {
                blocker =
                    "selective termination could not resolve incident identity " +
                    selectedIdentity;
                return false;
            }
            if (options.SelectAllExceptIdentity)
            {
                for (int index = 0; index < ordered.Count; index++)
                {
                    int identity = ordered[index].SourceEdgeIndex;
                    if (identity == selectedIdentity)
                    {
                        continue;
                    }
                    if (!TryFindPlaneCutCandidateBySourceEdge(
                            preparedIncident,
                            identity,
                            out PlaneCutBevelCandidate preparedOther) ||
                        !TryFindPlaneCutCandidateBySourceEdge(
                            minimumIncident,
                            identity,
                            out PlaneCutBevelCandidate minimumOther))
                    {
                        blocker =
                            "complement termination could not resolve incident identity " +
                            identity;
                        return false;
                    }
                    preparedSubset.Add(preparedOther);
                    minimumSubset.Add(minimumOther);
                }
                if (preparedSubset.Count == 0)
                {
                    blocker =
                        "complement termination selected no incident identities";
                    return false;
                }
                return true;
            }

            preparedSubset.Add(prepared);
            minimumSubset.Add(minimum);
            return true;
        }

        private static bool TryBuildPlaneCutBoundaryTournamentRedistribution(
            ChamferTopologyContext context,
            List<PlaneCutBevelCandidate> preparedCandidates,
            List<PlaneCutBevelCandidate> minimumCandidates,
            int sharedVertexIndex,
            PlaneCutBevelCandidate victim,
            PlaneCutBevelCandidate foreign,
            PlaneCutBevelTerminationOptions options,
            float minimumStableEdgeLength,
            out List<PlaneCutBevelCandidate> redistributedPrepared,
            out List<PlaneCutBevelCandidate> redistributedMinimum,
            out string blocker)
        {
            redistributedPrepared = preparedCandidates;
            redistributedMinimum = minimumCandidates;
            blocker = string.Empty;
            if (context == null || context.Graph == null ||
                preparedCandidates == null || minimumCandidates == null)
            {
                blocker =
                    "boundary tournament width redistribution inputs were incomplete";
                return false;
            }
            List<PlaneCutBevelCandidate> incident =
                GetActivePlaneCutIncidentCandidates(
                    preparedCandidates,
                    sharedVertexIndex);
            if (incident.Count < 2)
            {
                blocker =
                    "boundary tournament width redistribution requires a complete incident star";
                return false;
            }

            List<PlaneCutBevelCandidate> orderedIncident =
                new List<PlaneCutBevelCandidate>(incident);
            orderedIncident.Sort((left, right) =>
                left.SourceEdgeIndex.CompareTo(right.SourceEdgeIndex));
            int favoredIdentity = victim.SourceEdgeIndex;
            if (options.WidthFavoredIdentityMode == 1)
            {
                favoredIdentity = foreign.SourceEdgeIndex;
            }
            else if (options.WidthFavoredIdentityMode >= 2)
            {
                int ordinal = (options.WidthFavoredIdentityMode - 2) %
                    orderedIncident.Count;
                favoredIdentity =
                    orderedIncident[ordinal].SourceEdgeIndex;
            }
            Dictionary<int, float> scaleByEdge =
                new Dictionary<int, float>();
            for (int index = 0; index < preparedCandidates.Count; index++)
            {
                PlaneCutBevelCandidate prepared = preparedCandidates[index];
                float scale = 1f;
                if (incident.Any(candidate =>
                        candidate.SourceEdgeIndex ==
                            prepared.SourceEdgeIndex))
                {
                    scale = options.WidthScaleSelectedOnly
                        ? prepared.SourceEdgeIndex == favoredIdentity
                            ? options.PrimaryWidthScale
                            : options.FavoredWidthScale
                        : prepared.SourceEdgeIndex == favoredIdentity
                            ? options.FavoredWidthScale
                            : options.PrimaryWidthScale;
                    if (TryFindPlaneCutCandidateBySourceEdge(
                            minimumCandidates,
                            prepared.SourceEdgeIndex,
                            out PlaneCutBevelCandidate minimum) &&
                        prepared.Width > 0.0000001f)
                    {
                        scale = Mathf.Max(
                            scale,
                            minimum.Width / prepared.Width);
                    }
                    scale = Mathf.Clamp01(scale);
                }
                scaleByEdge[prepared.SourceEdgeIndex] = scale;
            }
            redistributedPrepared = BuildScaledPlaneCutCandidates(
                preparedCandidates,
                context,
                scaleByEdge,
                minimumStableEdgeLength);
            if (redistributedPrepared == null ||
                redistributedPrepared.Count != preparedCandidates.Count)
            {
                blocker =
                    "boundary tournament width redistribution could not rebuild the prepared candidate set";
                return false;
            }
            return true;
        }

        private static bool TryPrepareCornerDamageWidthRedistributionRecovery(
            CornerDamageIntegrationPlan plan,
            List<PlaneCutBevelCandidate> preparedCandidates,
            List<PlaneCutBevelCandidate> minimumCandidates,
            PlaneCutBevelCandidate victim,
            PlaneCutBevelCandidate foreign,
            int conflictVertexIndex,
            CornerDamageRecoveryTournamentConfiguration configuration)
        {
            if (plan == null || plan.SolvedPlan == null ||
                plan.SolvedPlan.Context == null ||
                preparedCandidates == null || minimumCandidates == null)
            {
                return false;
            }
            ResetEndpointPatchRecoveryAttempt(
                plan,
                victim.SourceEdgeIndex,
                foreign.SourceEdgeIndex);
            PlaneCutBevelSolvedPlan solvedPlan = plan.SolvedPlan;
            List<PlaneCutBevelCandidate> incident =
                GetActivePlaneCutIncidentCandidates(
                    preparedCandidates,
                    conflictVertexIndex);
            if (!IsSupportedEndpointPatchRecoverySet(
                    incident,
                    victim.SourceEdgeIndex,
                    foreign.SourceEdgeIndex))
            {
                RecordEndpointPatchRecoveryRejection(
                    plan,
                    PlaneCutEndpointPatchRejectionKind.UnsupportedStar,
                    "width redistribution requires a complete two/three-band incident star");
                return false;
            }

            float primary = Mathf.Clamp(
                configuration.PrimaryParameter,
                0.05f,
                1f);
            float secondary = Mathf.Clamp(
                configuration.SecondaryParameter,
                0.05f,
                1f);
            Dictionary<int, float> scaleByEdge =
                new Dictionary<int, float>();
            for (int index = 0; index < preparedCandidates.Count; index++)
            {
                scaleByEdge[preparedCandidates[index].SourceEdgeIndex] = 1f;
            }
            List<PlaneCutBevelCandidate> ordered =
                new List<PlaneCutBevelCandidate>(incident);
            ordered.Sort((left, right) =>
                left.SourceEdgeIndex.CompareTo(right.SourceEdgeIndex));
            for (int index = 0; index < ordered.Count; index++)
            {
                scaleByEdge[ordered[index].SourceEdgeIndex] = primary;
            }
            switch (configuration.VariantIndex)
            {
                case 1:
                    scaleByEdge[victim.SourceEdgeIndex] = secondary;
                    break;
                case 2:
                    scaleByEdge[foreign.SourceEdgeIndex] = secondary;
                    break;
                default:
                    if (configuration.VariantIndex >= 3)
                    {
                        int ordinal = (configuration.VariantIndex - 3) %
                            ordered.Count;
                        scaleByEdge[ordered[ordinal].SourceEdgeIndex] =
                            secondary;
                    }
                    break;
            }

            List<PlaneCutBevelCandidate> redistributed =
                BuildScaledPlaneCutCandidates(
                    preparedCandidates,
                    solvedPlan.Context,
                    scaleByEdge,
                    solvedPlan.MinimumStableEdgeLength);
            List<PlaneCutVertexJunctionCandidate> noJunctions =
                new List<PlaneCutVertexJunctionCandidate>();
            plan.EndpointPatchRecoveryTrialCount++;
            if (!TryBuildPlaneCutSystemFaces(
                    solvedPlan.SourceFaces,
                    redistributed,
                    noJunctions,
                    out List<PolygonFace> trialFaces,
                    out _,
                    out string blocker,
                    new PlaneCutNumericalRepairTelemetry()))
            {
                RecordEndpointPatchRecoveryRejection(
                    plan,
                    PlaneCutEndpointPatchRejectionKind.PatchExtraction,
                    "width redistribution shell failed: " + blocker);
                return false;
            }
            PlaneCutSolveMetrics metrics = new PlaneCutSolveMetrics();
            if (!IsPlaneCutJunctionTrialGeometryValid(
                    trialFaces,
                    solvedPlan.Context,
                    redistributed,
                    noJunctions,
                    solvedPlan.MinimumStableEdgeLength,
                    solvedPlan.MinimumStableFaceArea,
                    ref metrics,
                    out _,
                    out blocker))
            {
                RecordEndpointPatchRecoveryRejection(
                    plan,
                    PlaneCutEndpointPatchRejectionKind.BandIntegrity,
                    "width redistribution certification failed: " + blocker);
                return false;
            }

            solvedPlan.RetainedCandidates = redistributed;
            solvedPlan.PreparedEndpointPatch = null;
            solvedPlan.PreparedJunctions ??=
                new List<PlaneCutVertexJunctionCandidate>();
            solvedPlan.PreparedJunctions.Clear();
            plan.EndpointPatchRecoveryDiagnostic =
                configuration.Name +
                " certified a legal-width redistribution schedule";
            return true;
        }

        private static List<PlaneCutBevelCandidate>
            BuildPlaneCutCandidatesExcludingIdentities(
                List<PlaneCutBevelCandidate> candidates,
                HashSet<int> excluded)
        {
            List<PlaneCutBevelCandidate> retained =
                new List<PlaneCutBevelCandidate>();
            if (candidates == null)
            {
                return retained;
            }
            for (int index = 0; index < candidates.Count; index++)
            {
                PlaneCutBevelCandidate candidate = candidates[index];
                if (excluded == null ||
                    !excluded.Contains(candidate.SourceEdgeIndex))
                {
                    retained.Add(candidate);
                }
            }
            return retained;
        }

        private static bool TryBuildPlaneCutBevelTerminationLimits(
            ChamferTopologyContext context,
            int sharedVertexIndex,
            List<PlaneCutBevelCandidate> incident,
            float minimumStableEdgeLength,
            PlaneCutBevelTerminationOptions options,
            out PlaneCutEndpointCellLimit[] limits,
            out string signature,
            out string blocker)
        {
            limits = Array.Empty<PlaneCutEndpointCellLimit>();
            signature = string.Empty;
            blocker = string.Empty;
            if (context == null || context.Graph == null ||
                sharedVertexIndex < 0 ||
                sharedVertexIndex >= context.Graph.Vertices.Count ||
                incident == null ||
                incident.Count < (options.AllowSingleIncident ? 1 : 2))
            {
                blocker = "bevel-termination axial-limit inputs were incomplete";
                return false;
            }
            List<PlaneCutBevelCandidate> ordered =
                new List<PlaneCutBevelCandidate>(incident);
            ordered.Sort((left, right) =>
                left.SourceEdgeIndex.CompareTo(right.SourceEdgeIndex));
            Vector3 origin = context.Graph.Vertices[
                sharedVertexIndex].Position;
            limits = new PlaneCutEndpointCellLimit[ordered.Count];
            List<string> evidence = new List<string>();
            for (int index = 0; index < ordered.Count; index++)
            {
                PlaneCutBevelCandidate edge = ordered[index];
                int otherVertexIndex = edge.VertexA == sharedVertexIndex
                    ? edge.VertexB
                    : edge.VertexB == sharedVertexIndex
                        ? edge.VertexA
                        : -1;
                if (otherVertexIndex < 0 ||
                    otherVertexIndex >= context.Graph.Vertices.Count)
                {
                    blocker =
                        "bevel termination encountered a non-incident source edge";
                    return false;
                }
                Vector3 axis = context.Graph.Vertices[
                    otherVertexIndex].Position - origin;
                float sourceLength = axis.magnitude;
                if (sourceLength <= PointMergeDistance)
                {
                    blocker =
                        "bevel termination encountered a degenerate source edge";
                    return false;
                }
                axis /= sourceLength;
                float allowed = Mathf.Clamp(
                    Mathf.Max(
                        edge.Width * 4f,
                        minimumStableEdgeLength * 0.5f) *
                        options.AxialDistanceScale,
                    sourceLength * (options.IsProduction ? 0.03f : 0.02f),
                    sourceLength * (options.IsProduction ? 0.25f : 0.35f));
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

        private static bool TryBuildPlaneCutBevelTerminationReplacement(
            List<PolygonFace> fullFaces,
            List<PolygonFace> pocketFaces,
            ChamferTopologyContext context,
            List<PlaneCutBevelCandidate> activeCandidates,
            List<PlaneCutBevelCandidate> incident,
            int sharedVertexIndex,
            PlaneCutEndpointCellLimit[] limits,
            string limitSignature,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            PlaneCutBevelTerminationOptions options,
            out PlaneCutEndpointPatchReplacement replacement,
            out PlaneCutEndpointCellEvidence evidence,
            out PlaneCutEndpointPatchRejectionKind rejection,
            out string blocker)
        {
            replacement = null;
            evidence = new PlaneCutEndpointCellEvidence
            {
                CellLimitSignature = limitSignature
            };
            rejection = PlaneCutEndpointPatchRejectionKind.PatchExtraction;
            blocker = string.Empty;
            if (fullFaces == null || pocketFaces == null ||
                context == null || context.Graph == null ||
                activeCandidates == null || incident == null ||
                incident.Count < (options.AllowSingleIncident ? 1 : 2) ||
                limits == null ||
                limits.Length != incident.Count)
            {
                blocker = "bevel-termination replacement inputs were incomplete";
                return false;
            }

            HashSet<int> incidentIdentities = new HashSet<int>();
            Dictionary<int, PlaneCutBevelCandidate> incidentByIdentity =
                new Dictionary<int, PlaneCutBevelCandidate>();
            for (int index = 0; index < incident.Count; index++)
            {
                incidentIdentities.Add(incident[index].SourceEdgeIndex);
                incidentByIdentity[incident[index].SourceEdgeIndex] =
                    incident[index];
            }
            HashSet<int> endpointStarSourceFaces =
                BuildPlaneCutBevelTerminationEndpointStarSourceFaces(
                    context,
                    sharedVertexIndex);

            Dictionary<TopologyEdgeKey, Vector3>[] fullCaches =
                BuildPlaneCutTerminationIntersectionCaches(limits.Length);
            Dictionary<TopologyEdgeKey, Vector3>[] pocketCaches =
                BuildPlaneCutTerminationIntersectionCaches(limits.Length);
            List<int> selectedFullIndices = new List<int>();
            HashSet<int> selectedIncidentIdentities = new HashSet<int>();
            List<PolygonFace> remoteRemainders = new List<PolygonFace>();
            List<PolygonFace> restoredPocket = new List<PolygonFace>();
            List<string> selectedProvenance = new List<string>();
            List<string> remoteSignatures = new List<string>();
            List<string> pocketSignatures = new List<string>();
            List<Vector3> splitPoints = new List<Vector3>();

            for (int faceIndex = 0; faceIndex < fullFaces.Count; faceIndex++)
            {
                PolygonFace face = fullFaces[faceIndex];
                if (!TryPartitionPlaneCutBevelTerminationFace(
                        face,
                        limits,
                        minimumStableFaceArea,
                        fullCaches,
                        options,
                        out PolygonFace localFragment,
                        out List<PolygonFace> remoteFragments,
                        out List<Vector3> faceSplitPoints,
                        out blocker))
                {
                    rejection =
                        PlaneCutEndpointPatchRejectionKind.PatchExtraction;
                    return false;
                }
                if (localFragment == null)
                {
                    continue;
                }
                if (!IsPlaneCutBevelTerminationFaceOwned(
                        face,
                        incidentIdentities,
                        endpointStarSourceFaces,
                        options.Ownership))
                {
                    continue;
                }
                if (face.ProvenanceKind ==
                        PolygonFaceProvenanceKind.EdgeBevelPlane &&
                    !incidentIdentities.Contains(face.ProvenanceIndex))
                {
                    rejection =
                        PlaneCutEndpointPatchRejectionKind.PatchExtraction;
                    blocker =
                        "bevel termination endpoint cell reached unrelated bevel identity " +
                        face.ProvenanceIndex;
                    return false;
                }
                selectedFullIndices.Add(faceIndex);
                if (face.ProvenanceKind ==
                    PolygonFaceProvenanceKind.EdgeBevelPlane)
                {
                    selectedIncidentIdentities.Add(face.ProvenanceIndex);
                }
                selectedProvenance.Add(
                    ((int)face.ProvenanceKind).ToString() + ":" +
                    face.ProvenanceIndex.ToString());
                splitPoints.AddRange(faceSplitPoints);
                for (int remoteIndex = 0;
                     remoteIndex < remoteFragments.Count;
                     remoteIndex++)
                {
                    PolygonFace remote = remoteFragments[remoteIndex];
                    remoteRemainders.Add(remote);
                    remoteSignatures.Add(
                        BuildPlaneCutEndpointPatchFaceSignature(remote));
                }
            }
            if (!TrySelectPlaneCutRemoteBevelComponents(
                    remoteRemainders,
                    incident,
                    selectedIncidentIdentities,
                    context,
                    sharedVertexIndex,
                    options.RemoteComponentSelection,
                    evidence,
                    out blocker))
            {
                rejection =
                    PlaneCutEndpointPatchRejectionKind.IncidentBandJoin;
                return false;
            }

            if (selectedFullIndices.Count == 0)
            {
                rejection =
                    PlaneCutEndpointPatchRejectionKind.NoLocalRemoval;
                blocker =
                    "bevel termination axial cell selected no ordinary-shell subfaces";
                return false;
            }

            for (int faceIndex = 0;
                 faceIndex < pocketFaces.Count;
                 faceIndex++)
            {
                PolygonFace face = pocketFaces[faceIndex];
                if (!TryPartitionPlaneCutBevelTerminationFace(
                        face,
                        limits,
                        minimumStableFaceArea,
                        pocketCaches,
                        options,
                        out PolygonFace localFragment,
                        out _,
                        out List<Vector3> faceSplitPoints,
                        out blocker))
                {
                    rejection =
                        PlaneCutEndpointPatchRejectionKind.PatchExtraction;
                    return false;
                }
                if (localFragment == null)
                {
                    continue;
                }
                if (!IsPlaneCutBevelTerminationFaceOwned(
                        localFragment,
                        incidentIdentities,
                        endpointStarSourceFaces,
                        options.Ownership))
                {
                    continue;
                }
                if (localFragment.ProvenanceKind ==
                    PolygonFaceProvenanceKind.EdgeBevelPlane)
                {
                    rejection =
                        PlaneCutEndpointPatchRejectionKind.PatchExtraction;
                    blocker =
                        "restored endpoint pocket would alter unrelated bevel identity " +
                        localFragment.ProvenanceIndex;
                    return false;
                }
                restoredPocket.Add(localFragment);
                pocketSignatures.Add(
                    BuildPlaneCutEndpointPatchFaceSignature(localFragment));
                splitPoints.AddRange(faceSplitPoints);
            }
            if (restoredPocket.Count == 0)
            {
                rejection =
                    PlaneCutEndpointPatchRejectionKind.NoLocalRemoval;
                blocker =
                    "bevel termination produced no endpoint source-face pocket";
                return false;
            }

            HashSet<int> selected = new HashSet<int>(selectedFullIndices);
            List<PolygonFace> hybrid = new List<PolygonFace>();
            List<int> hybridOriginalFaceIndices = new List<int>();
            for (int faceIndex = 0; faceIndex < fullFaces.Count; faceIndex++)
            {
                if (!selected.Contains(faceIndex))
                {
                    hybrid.Add(ClonePlaneCutPolygonFace(fullFaces[faceIndex]));
                    hybridOriginalFaceIndices.Add(faceIndex);
                }
            }
            int remoteHybridStart = hybrid.Count;
            for (int index = 0; index < remoteRemainders.Count; index++)
            {
                hybrid.Add(ClonePlaneCutPolygonFace(remoteRemainders[index]));
                hybridOriginalFaceIndices.Add(-1);
            }
            int pocketHybridStart = hybrid.Count;
            for (int index = 0; index < restoredPocket.Count; index++)
            {
                hybrid.Add(ClonePlaneCutPolygonFace(restoredPocket[index]));
                hybridOriginalFaceIndices.Add(-1);
            }

            List<PolygonFace> terminationCaps =
                new List<PolygonFace>();
            string loopSignature = string.Empty;
            int loopVertexCount = 0;
            if (options.IsProduction)
            {
                if (!TryBuildPlaneCutBevelTerminationCaps(
                        hybrid,
                        limits,
                        incidentByIdentity,
                        sharedVertexIndex,
                        minimumStableFaceArea,
                        context,
                        options,
                        out terminationCaps,
                        out loopSignature,
                        out loopVertexCount,
                        out blocker))
                {
                    rejection =
                        PlaneCutEndpointPatchRejectionKind.CapCreation;
                    return false;
                }
            }
            else if (!TryBuildPlaneCutBoundaryTournamentClosure(
                    hybrid,
                    hybridOriginalFaceIndices,
                    context,
                    sharedVertexIndex,
                    limits,
                    incidentByIdentity,
                    minimumStableFaceArea,
                    options,
                    evidence,
                    out terminationCaps,
                    out loopSignature,
                    out loopVertexCount,
                    out blocker))
            {
                rejection =
                    PlaneCutEndpointPatchRejectionKind.CapCreation;
                return false;
            }

            int capHybridStart = hybrid.Count;
            for (int index = 0; index < terminationCaps.Count; index++)
            {
                hybrid.Add(terminationCaps[index]);
                hybridOriginalFaceIndices.Add(-1);
            }
            if (options.PostClosureFixedPointConformance &&
                !TryConformPlaneCutLocalShellFixedPoint(
                    hybrid,
                    context,
                    sharedVertexIndex,
                    limits,
                    evidence,
                    out blocker))
            {
                rejection =
                    PlaneCutEndpointPatchRejectionKind.StitchTopology;
                evidence.FailureSource = blocker;
                return false;
            }

            for (int index = 0; index < remoteRemainders.Count; index++)
            {
                remoteRemainders[index] = ClonePlaneCutPolygonFace(
                    hybrid[remoteHybridStart + index]);
            }
            for (int index = 0; index < restoredPocket.Count; index++)
            {
                restoredPocket[index] = ClonePlaneCutPolygonFace(
                    hybrid[pocketHybridStart + index]);
            }
            for (int index = 0; index < terminationCaps.Count; index++)
            {
                terminationCaps[index] = ClonePlaneCutPolygonFace(
                    hybrid[capHybridStart + index]);
            }
            foreach (int hybridFaceIndex in
                evidence.MutatedHybridFaceIndices.OrderBy(value => value))
            {
                if (hybridFaceIndex < 0 ||
                    hybridFaceIndex >= hybrid.Count ||
                    hybridFaceIndex >= hybridOriginalFaceIndices.Count)
                {
                    rejection =
                        PlaneCutEndpointPatchRejectionKind.PatchExtraction;
                    blocker =
                        "conforming boundary reconstruction recorded an invalid owner-face index";
                    evidence.FailureSource = blocker;
                    return false;
                }
                int originalFaceIndex =
                    hybridOriginalFaceIndices[hybridFaceIndex];
                if (originalFaceIndex < 0 ||
                    !selected.Add(originalFaceIndex))
                {
                    continue;
                }
                selectedFullIndices.Add(originalFaceIndex);
                PolygonFace original = fullFaces[originalFaceIndex];
                selectedProvenance.Add(
                    ((int)original.ProvenanceKind).ToString() + ":" +
                    original.ProvenanceIndex.ToString());
                remoteRemainders.Add(ClonePlaneCutPolygonFace(
                    hybrid[hybridFaceIndex]));
            }

            remoteSignatures.Clear();
            for (int index = 0; index < remoteRemainders.Count; index++)
            {
                remoteSignatures.Add(
                    BuildPlaneCutEndpointPatchFaceSignature(
                        remoteRemainders[index]));
            }
            pocketSignatures.Clear();
            for (int index = 0; index < restoredPocket.Count; index++)
            {
                pocketSignatures.Add(
                    BuildPlaneCutEndpointPatchFaceSignature(
                        restoredPocket[index]));
            }

            evidence.TransitionFaceCount = terminationCaps.Count;
            evidence.ResidualOpenEdgeCount =
                CollectPlaneCutOpenEdges(hybrid).Count;
            if (evidence.ResidualOpenEdgeCount > 0)
            {
                rejection =
                    PlaneCutEndpointPatchRejectionKind.StitchTopology;
                blocker =
                    (string.IsNullOrEmpty(options.StrategyName)
                        ? "boundary reconstruction"
                        : options.StrategyName) +
                    " left " + evidence.ResidualOpenEdgeCount +
                    " residual open edges";
                evidence.FailureSource = blocker;
                return false;
            }

            EdgeWearTopologyStats topology = AuditEdgeWearTopology(
                hybrid,
                minimumStableEdgeLength);
            if (topology.OpenEdgeCount > 0 ||
                topology.NonManifoldEdgeCount > 0 ||
                topology.TJunctionCount > 0)
            {
                rejection =
                    PlaneCutEndpointPatchRejectionKind.StitchTopology;
                blocker = BuildPlaneCutTopologyFailureDiagnostic(
                    hybrid,
                    topology);
                evidence.FailureSource = blocker;
                evidence.MechanismSignature += ":" + blocker;
                return false;
            }

            int remoteIncidentCount = 0;
            for (int index = 0; index < incident.Count; index++)
            {
                PlaneCutBevelCandidate edge = incident[index];
                List<PolygonFace> owned = FindPlaneCutProvenanceFaces(
                    hybrid,
                    PolygonFaceProvenanceKind.EdgeBevelPlane,
                    edge.SourceEdgeIndex);
                bool validRemoteBand = owned.Count == 1;
                string remoteBandBlocker = string.Empty;
                if (options.FragmentAwareBandCertification &&
                    owned.Count > 0)
                {
                    validRemoteBand = TryValidatePlaneCutOwnedBevelBandSet(
                        owned,
                        edge,
                        out remoteBandBlocker);
                }
                if (!validRemoteBand)
                {
                    rejection =
                        PlaneCutEndpointPatchRejectionKind.IncidentBandJoin;
                    blocker = owned.Count == 0
                        ? "terminated bevel identity " +
                            edge.SourceEdgeIndex +
                            " lost its remote band"
                        : !string.IsNullOrEmpty(remoteBandBlocker)
                            ? remoteBandBlocker
                            : "terminated bevel identity " +
                                edge.SourceEdgeIndex +
                                " split into multiple remote bands";
                    return false;
                }
                if (options.FragmentAwareBandCertification)
                {
                    evidence.MechanismSignature +=
                        ":band" + edge.SourceEdgeIndex +
                        "x" + owned.Count;
                }
                remoteIncidentCount++;
            }

            PlaneCutSolveMetrics metrics = new PlaneCutSolveMetrics();
            if (!IsPlaneCutJunctionTrialGeometryValid(
                    hybrid,
                    context,
                    activeCandidates,
                    new List<PlaneCutVertexJunctionCandidate>(),
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    ref metrics,
                    out _,
                    out blocker))
            {
                rejection =
                    PlaneCutEndpointPatchRejectionKind.BandIntegrity;
                return false;
            }

            if (!TryBuildPlaneCutEndpointCellSelectedBoundary(
                    fullFaces,
                    selected,
                    out Vector3[] boundaryLoop,
                    out string boundaryTopologySignature,
                    out string boundaryPositionSignature,
                    out blocker))
            {
                rejection =
                    PlaneCutEndpointPatchRejectionKind.BoundaryLoop;
                return false;
            }

            string[] selectedSignatures =
                new string[selectedFullIndices.Count];
            for (int index = 0;
                 index < selectedFullIndices.Count;
                 index++)
            {
                selectedSignatures[index] =
                    BuildPlaneCutEndpointPatchFaceSignature(
                        fullFaces[selectedFullIndices[index]]);
            }
            Array.Sort(selectedSignatures, StringComparer.Ordinal);
            selectedProvenance.Sort(StringComparer.Ordinal);
            remoteSignatures.Sort(StringComparer.Ordinal);
            pocketSignatures.Sort(StringComparer.Ordinal);
            int[] terminated = new int[incident.Count];
            for (int index = 0; index < incident.Count; index++)
            {
                terminated[index] = incident[index].SourceEdgeIndex;
            }
            Array.Sort(terminated);
            List<Vector3> uniqueSplitPoints = GetUniquePoints(splitPoints);
            int capVertexCount = 0;
            List<string> terminationCapSignatures = new List<string>();
            for (int index = 0; index < terminationCaps.Count; index++)
            {
                capVertexCount += terminationCaps[index].Vertices.Count;
                terminationCapSignatures.Add(
                    BuildPlaneCutEndpointPatchFaceSignature(
                        terminationCaps[index]));
            }
            terminationCapSignatures.Sort(StringComparer.Ordinal);

            evidence.FacesSubdivided = selectedFullIndices.Count;
            evidence.LocalFragmentCount = restoredPocket.Count;
            evidence.RemoteRemainderCount = remoteRemainders.Count;
            evidence.SyntheticIncidentFragmentCount = terminationCaps.Count;
            evidence.SyntheticIncidentIdentities =
                string.Join("/", terminated);
            evidence.CellVertexCount = uniqueSplitPoints.Count;
            evidence.CellFaceCount = remoteRemainders.Count +
                restoredPocket.Count + terminationCaps.Count;
            evidence.CellSplitSignature =
                BuildPlaneCutEndpointCellPointSetSignature(uniqueSplitPoints);
            evidence.LocalFragmentSignature =
                string.Join("/", pocketSignatures);
            evidence.RemoteRemainderSignature =
                string.Join("/", remoteSignatures);
            evidence.FailureSource = string.Empty;

            replacement = new PlaneCutEndpointPatchReplacement
            {
                VertexIndex = sharedVertexIndex,
                Strength = 1f,
                SourceVertexPosition = context.Graph.Vertices[
                    sharedVertexIndex].Position,
                IncidentSourceEdgeIndices = terminated,
                SelectedFaceSignatures = selectedSignatures,
                SelectedProvenanceSignature =
                    string.Join("/", selectedProvenance),
                BoundaryTopologySignature =
                    boundaryTopologySignature,
                BoundaryPositionSignature =
                    boundaryPositionSignature,
                BoundaryLoop = boundaryLoop,
                ReplacementFaces = remoteRemainders
                    .Concat(restoredPocket)
                    .Concat(terminationCaps)
                    .Select(ClonePlaneCutPolygonFace)
                    .ToList(),
                SelectedFaceCount = selectedFullIndices.Count,
                BoundaryVertexCount = boundaryLoop.Length,
                CapVertexCount = capVertexCount,
                CellLimits = limits,
                CellLimitSignature = limitSignature,
                LocalFragmentSignature =
                    string.Join("/", pocketSignatures),
                RemoteRemainderSignature =
                    string.Join("/", remoteSignatures),
                CellSplitSignature =
                    evidence.CellSplitSignature,
                FacesSubdivided = selectedFullIndices.Count,
                LocalFragmentCount = restoredPocket.Count,
                RemoteRemainderCount = remoteRemainders.Count,
                CellVertexCount = uniqueSplitPoints.Count,
                CellFaceCount = evidence.CellFaceCount,
                ConflictLocalTermination = true,
                TerminatedSourceEdgeIndices = terminated,
                TerminationLoopSignature = loopSignature,
                TerminationCapSignature =
                    string.Join("/", terminationCapSignatures),
                RemoteIncidentBevelCount = remoteIncidentCount,
                RestoredPocketFaceCount = restoredPocket.Count,
                TerminationCapCount = terminationCaps.Count,
                BoundaryComponentCount = evidence.BoundaryComponentCount,
                ClosedCycleCount = evidence.ClosedCycleCount,
                OpenChainCount = evidence.OpenChainCount,
                BranchVertexCount = evidence.BranchVertexCount,
                TransitionFaceCount = evidence.TransitionFaceCount,
                ResidualOpenEdgeCount = evidence.ResidualOpenEdgeCount,
                MechanismSignature = evidence.MechanismSignature,
                ModifiedIdentitySignature =
                    evidence.ModifiedIdentitySignature,
                ClosurelessAccepted = evidence.ClosurelessAccepted
            };
            rejection = PlaneCutEndpointPatchRejectionKind.None;
            return true;
        }

        private static Dictionary<TopologyEdgeKey, Vector3>[]
            BuildPlaneCutTerminationIntersectionCaches(int count)
        {
            Dictionary<TopologyEdgeKey, Vector3>[] result =
                new Dictionary<TopologyEdgeKey, Vector3>[count];
            for (int index = 0; index < count; index++)
            {
                result[index] =
                    new Dictionary<TopologyEdgeKey, Vector3>();
            }
            return result;
        }

        private static HashSet<int>
            BuildPlaneCutBevelTerminationEndpointStarSourceFaces(
                ChamferTopologyContext context,
                int sharedVertexIndex)
        {
            HashSet<int> result = new HashSet<int>();
            if (context == null || context.Graph == null ||
                sharedVertexIndex < 0 ||
                sharedVertexIndex >= context.Graph.Vertices.Count)
            {
                return result;
            }
            EdgeWearGraphVertex vertex =
                context.Graph.Vertices[sharedVertexIndex];
            for (int index = 0; index < vertex.FaceIndices.Count; index++)
            {
                int graphFaceIndex = vertex.FaceIndices[index];
                if (graphFaceIndex < 0 ||
                    graphFaceIndex >= context.Graph.Faces.Count)
                {
                    continue;
                }
                result.Add(graphFaceIndex);
                result.Add(context.Graph.Faces[graphFaceIndex].SourceFaceIndex);
            }
            return result;
        }

        private static bool IsPlaneCutBevelTerminationFaceOwned(
            PolygonFace face,
            HashSet<int> incidentIdentities,
            HashSet<int> endpointStarSourceFaces,
            PlaneCutBevelTerminationOwnership ownership)
        {
            if (face == null)
            {
                return false;
            }
            if (ownership ==
                PlaneCutBevelTerminationOwnership.GeometricCell)
            {
                return true;
            }
            if (face.ProvenanceKind ==
                PolygonFaceProvenanceKind.EdgeBevelPlane)
            {
                return incidentIdentities != null &&
                    incidentIdentities.Contains(face.ProvenanceIndex);
            }
            if (face.ProvenanceKind ==
                PolygonFaceProvenanceKind.SourceFace)
            {
                return endpointStarSourceFaces != null &&
                    endpointStarSourceFaces.Contains(face.ProvenanceIndex);
            }
            return face.ProvenanceKind ==
                    PolygonFaceProvenanceKind.CornerDamageCap ||
                face.ProvenanceKind ==
                    PolygonFaceProvenanceKind.BoundedEndpointCap;
        }

        private readonly struct PlaneCutBoundarySegment
        {
            public readonly int FaceIndex;
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly VertexKey StartKey;
            public readonly VertexKey EndKey;
            public readonly EdgeKey EdgeKey;

            public PlaneCutBoundarySegment(
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

            public VertexKey Other(VertexKey key)
            {
                return StartKey.Equals(key) ? EndKey : StartKey;
            }

            public Vector3 Position(VertexKey key)
            {
                return StartKey.Equals(key) ? Start : End;
            }
        }

        private readonly struct PlaneCutBoundaryPairKey :
            IEquatable<PlaneCutBoundaryPairKey>
        {
            public readonly VertexKey Vertex;
            public readonly int EdgeIndex;

            public PlaneCutBoundaryPairKey(
                VertexKey vertex,
                int edgeIndex)
            {
                Vertex = vertex;
                EdgeIndex = edgeIndex;
            }

            public bool Equals(PlaneCutBoundaryPairKey other)
            {
                return Vertex.Equals(other.Vertex) &&
                    EdgeIndex == other.EdgeIndex;
            }

            public override bool Equals(object obj)
            {
                return obj is PlaneCutBoundaryPairKey other &&
                    Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return Vertex.GetHashCode() * 397 ^ EdgeIndex;
                }
            }
        }

        private sealed class PlaneCutBoundaryCycleResult
        {
            public readonly List<Vector3[]> Cycles =
                new List<Vector3[]>();
            public int ComponentCount;
            public int OpenChainCount;
            public int BranchVertexCount;
            public string Signature = string.Empty;
        }

        private static bool TryBuildPlaneCutBoundaryTournamentClosure(
            List<PolygonFace> hybrid,
            List<int> hybridOriginalFaceIndices,
            ChamferTopologyContext context,
            int sharedVertexIndex,
            PlaneCutEndpointCellLimit[] limits,
            Dictionary<int, PlaneCutBevelCandidate> incidentByIdentity,
            float minimumStableFaceArea,
            PlaneCutBevelTerminationOptions options,
            PlaneCutEndpointCellEvidence evidence,
            out List<PolygonFace> closureFaces,
            out string signature,
            out int loopVertexCount,
            out string blocker)
        {
            closureFaces = new List<PolygonFace>();
            signature = string.Empty;
            loopVertexCount = 0;
            blocker = string.Empty;
            evidence ??= new PlaneCutEndpointCellEvidence();
            evidence.MechanismSignature =
                options.StrategyName +
                (string.IsNullOrEmpty(evidence.MechanismSignature)
                    ? string.Empty
                    : evidence.MechanismSignature);
            evidence.ModifiedIdentitySignature = string.Join(
                "/",
                incidentByIdentity.Keys.OrderBy(value => value));

            switch (options.Closure)
            {
                case PlaneCutBevelTerminationClosure.Closureless:
                    return TryAcceptPlaneCutClosurelessBoundary(
                        hybrid,
                        evidence,
                        out closureFaces,
                        out signature,
                        out loopVertexCount,
                        out blocker);
                case PlaneCutBevelTerminationClosure.RawEdgeCavityFan:
                    return TryBuildPlaneCutRawEdgeCavityFanClosure(
                        hybrid,
                        context,
                        sharedVertexIndex,
                        minimumStableFaceArea,
                        evidence,
                        out closureFaces,
                        out signature,
                        out loopVertexCount,
                        out blocker);
                case PlaneCutBevelTerminationClosure.
                    ConformingNormalizedCavity:
                    return TryBuildPlaneCutConformingNormalizedClosure(
                        hybrid,
                        hybridOriginalFaceIndices,
                        context,
                        sharedVertexIndex,
                        minimumStableFaceArea,
                        options,
                        evidence,
                        out closureFaces,
                        out signature,
                        out loopVertexCount,
                        out blocker);
                case PlaneCutBevelTerminationClosure.
                    OrientedHalfEdgeCavity:
                    return TryBuildPlaneCutOrientedHalfEdgeClosure(
                        hybrid,
                        context,
                        sharedVertexIndex,
                        minimumStableFaceArea,
                        false,
                        options.RequireSimpleClosureCycles,
                        options.DirectSimpleCycleTriangles,
                        evidence,
                        out closureFaces,
                        out signature,
                        out loopVertexCount,
                        out blocker);
                case PlaneCutBevelTerminationClosure.
                    SourceFaceTransitionStrips:
                    return TryBuildPlaneCutSourceFaceStripClosure(
                        hybrid,
                        context,
                        sharedVertexIndex,
                        minimumStableFaceArea,
                        evidence,
                        out closureFaces,
                        out signature,
                        out loopVertexCount,
                        out blocker);
                case PlaneCutBevelTerminationClosure.
                    BoundaryEdgeCellFan:
                    return TryBuildPlaneCutBoundaryEdgeCellFanClosure(
                        hybrid,
                        context,
                        sharedVertexIndex,
                        minimumStableFaceArea,
                        evidence,
                        out closureFaces,
                        out signature,
                        out loopVertexCount,
                        out blocker);
                case PlaneCutBevelTerminationClosure.
                    AxialCapsAndTransitionLoops:
                case PlaneCutBevelTerminationClosure.
                    TaperFansAndTransitionLoops:
                    return TryBuildPlaneCutAxialTransitionClosure(
                        hybrid,
                        context,
                        sharedVertexIndex,
                        limits,
                        incidentByIdentity,
                        minimumStableFaceArea,
                        options,
                        evidence,
                        out closureFaces,
                        out signature,
                        out loopVertexCount,
                        out blocker);
                default:
                    blocker =
                        "boundary tournament selected no supported reconstruction mechanism";
                    evidence.FailureSource = blocker;
                    return false;
            }
        }

        private static bool TryBuildPlaneCutAxialTransitionClosure(
            List<PolygonFace> hybrid,
            ChamferTopologyContext context,
            int sharedVertexIndex,
            PlaneCutEndpointCellLimit[] limits,
            Dictionary<int, PlaneCutBevelCandidate> incidentByIdentity,
            float minimumStableFaceArea,
            PlaneCutBevelTerminationOptions options,
            PlaneCutEndpointCellEvidence evidence,
            out List<PolygonFace> closureFaces,
            out string signature,
            out int loopVertexCount,
            out string blocker)
        {
            closureFaces = new List<PolygonFace>();
            signature = string.Empty;
            loopVertexCount = 0;
            blocker = string.Empty;
            if (!TryBuildPlaneCutBevelTerminationCaps(
                    hybrid,
                    limits,
                    incidentByIdentity,
                    sharedVertexIndex,
                    minimumStableFaceArea,
                    context,
                    options,
                    out List<PolygonFace> axialFaces,
                    out string axialSignature,
                    out int axialVertices,
                    out blocker))
            {
                evidence.FailureSource = blocker;
                return false;
            }
            List<PolygonFace> working = hybrid
                .Select(ClonePlaneCutPolygonFace)
                .ToList();
            working.AddRange(axialFaces);
            if (!TryBuildPlaneCutBevelTransitionClosures(
                    working,
                    context,
                    sharedVertexIndex,
                    minimumStableFaceArea,
                    out List<PolygonFace> transitionFaces,
                    out string transitionSignature,
                    out blocker))
            {
                evidence.FailureSource = blocker;
                return false;
            }
            closureFaces.AddRange(axialFaces);
            closureFaces.AddRange(transitionFaces);
            loopVertexCount = axialVertices;
            evidence.TransitionFaceCount = closureFaces.Count;
            evidence.ResidualOpenEdgeCount = CollectPlaneCutOpenEdges(
                working.Concat(transitionFaces).ToList()).Count;
            signature = axialSignature + ":transition=" +
                transitionSignature;
            return closureFaces.Count > 0;
        }

        private static bool TryAcceptPlaneCutClosurelessBoundary(
            List<PolygonFace> hybrid,
            PlaneCutEndpointCellEvidence evidence,
            out List<PolygonFace> closureFaces,
            out string signature,
            out int loopVertexCount,
            out string blocker)
        {
            closureFaces = new List<PolygonFace>();
            signature = string.Empty;
            loopVertexCount = 0;
            blocker = string.Empty;
            List<PlaneCutOpenEdgeRecord> open =
                CollectPlaneCutOpenEdges(hybrid);
            evidence.ResidualOpenEdgeCount = open.Count;
            evidence.TransitionFaceCount = 0;
            if (open.Count != 0)
            {
                List<PlaneCutBoundarySegment> segments = open.Select(edge =>
                    new PlaneCutBoundarySegment(
                        edge.FaceIndex,
                        edge.Start,
                        edge.End)).ToList();
                evidence.BoundaryComponentCount =
                    BuildPlaneCutBoundaryComponents(segments).Count;
                blocker =
                    "closureless endpoint transaction retained " +
                    open.Count + " open edges";
                evidence.FailureSource = blocker;
                return false;
            }
            evidence.ClosurelessAccepted = true;
            evidence.MechanismSignature =
                (string.IsNullOrEmpty(evidence.MechanismSignature)
                    ? "closureless"
                    : evidence.MechanismSignature + ":closureless") +
                ":open=0";
            signature = "closureless:open=0";
            return true;
        }

        private static bool TryBuildPlaneCutRawEdgeCavityFanClosure(
            List<PolygonFace> hybrid,
            ChamferTopologyContext context,
            int sharedVertexIndex,
            float minimumStableFaceArea,
            PlaneCutEndpointCellEvidence evidence,
            out List<PolygonFace> closureFaces,
            out string signature,
            out int loopVertexCount,
            out string blocker)
        {
            List<PlaneCutOpenEdgeRecord> open =
                CollectPlaneCutOpenEdges(hybrid);
            List<PlaneCutBoundarySegment> segments = open.Select(edge =>
                new PlaneCutBoundarySegment(
                    edge.FaceIndex,
                    edge.Start,
                    edge.End)).ToList();
            segments.Sort(ComparePlaneCutBoundarySegments);
            return TryBuildPlaneCutBoundaryFanClosureFromSegments(
                hybrid,
                context,
                sharedVertexIndex,
                minimumStableFaceArea,
                segments,
                "raw-edge-cavity-fan",
                evidence,
                out closureFaces,
                out signature,
                out loopVertexCount,
                out blocker);
        }

        private static bool TryBuildPlaneCutBoundaryFanClosureFromSegments(
            List<PolygonFace> hybrid,
            ChamferTopologyContext context,
            int sharedVertexIndex,
            float minimumStableFaceArea,
            List<PlaneCutBoundarySegment> segments,
            string mechanismName,
            PlaneCutEndpointCellEvidence evidence,
            out List<PolygonFace> closureFaces,
            out string signature,
            out int loopVertexCount,
            out string blocker)
        {
            closureFaces = new List<PolygonFace>();
            signature = string.Empty;
            loopVertexCount = 0;
            blocker = string.Empty;
            if (segments == null || segments.Count < 3)
            {
                blocker = mechanismName +
                    " found fewer than three boundary segments";
                evidence.FailureSource = blocker;
                return false;
            }

            List<List<PlaneCutBoundarySegment>> components =
                BuildPlaneCutBoundaryComponents(segments);
            evidence.BoundaryComponentCount = components.Count;
            Vector3 endpoint = context.Graph.Vertices[
                sharedVertexIndex].Position;
            List<string> componentSignatures = new List<string>();
            for (int componentIndex = 0;
                 componentIndex < components.Count;
                 componentIndex++)
            {
                List<PlaneCutBoundarySegment> component =
                    components[componentIndex];
                Dictionary<VertexKey, Vector3> positions =
                    new Dictionary<VertexKey, Vector3>();
                Dictionary<VertexKey, int> degree =
                    new Dictionary<VertexKey, int>();
                Vector3 normalSum = Vector3.zero;
                for (int segmentIndex = 0;
                     segmentIndex < component.Count;
                     segmentIndex++)
                {
                    PlaneCutBoundarySegment segment = component[segmentIndex];
                    positions[segment.StartKey] = segment.Start;
                    positions[segment.EndKey] = segment.End;
                    degree.TryGetValue(segment.StartKey, out int startDegree);
                    degree[segment.StartKey] = startDegree + 1;
                    degree.TryGetValue(segment.EndKey, out int endDegree);
                    degree[segment.EndKey] = endDegree + 1;
                    if (segment.FaceIndex >= 0 &&
                        segment.FaceIndex < hybrid.Count)
                    {
                        normalSum += hybrid[segment.FaceIndex].Normal;
                    }
                }
                int oddVertices = degree.Count(pair =>
                    (pair.Value & 1) != 0);
                int branches = degree.Count(pair => pair.Value > 2);
                evidence.OpenChainCount += oddVertices;
                evidence.BranchVertexCount += branches;

                Vector3 centroid = Vector3.zero;
                List<VertexKey> orderedKeys = positions.Keys.ToList();
                orderedKeys.Sort((left, right) => left.CompareTo(right));
                for (int index = 0; index < orderedKeys.Count; index++)
                {
                    centroid += positions[orderedKeys[index]];
                }
                centroid /= Mathf.Max(1, positions.Count);
                Vector3 averageNormal = normalSum.sqrMagnitude >
                    MinimumEdgeLengthSqr
                    ? normalSum.normalized
                    : Vector3.up;
                float offset = Mathf.Max(
                    PointMergeDistance * 8f,
                    Mathf.Sqrt(Mathf.Max(
                        TinyFaceAreaEpsilon,
                        minimumStableFaceArea)) * 0.2f);
                Vector3[] candidateCentres =
                {
                    centroid,
                    Vector3.Lerp(centroid, endpoint, 0.35f),
                    centroid + averageNormal * offset,
                    centroid - averageNormal * offset
                };

                List<PolygonFace> bestFaces = null;
                string bestSignature = string.Empty;
                int bestResidual = int.MaxValue;
                for (int centreIndex = 0;
                     centreIndex < candidateCentres.Length;
                     centreIndex++)
                {
                    Vector3 centre = candidateCentres[centreIndex];
                    List<PolygonFace> trialFaces = new List<PolygonFace>();
                    List<string> trialSignatures = new List<string>();
                    bool valid = true;
                    for (int segmentIndex = 0;
                         segmentIndex < component.Count;
                         segmentIndex++)
                    {
                        PlaneCutBoundarySegment segment =
                            component[segmentIndex];
                        List<Vector3> triangle = new List<Vector3>
                        {
                            segment.End,
                            segment.Start,
                            centre
                        };
                        Vector3 triangleNormal =
                            CalculatePolygonNormal(triangle);
                        if (!IsFinite(triangleNormal) ||
                            triangleNormal.sqrMagnitude <= 0.000001f ||
                            CalculatePolygonArea(triangle) <= Mathf.Max(
                                TinyFaceAreaEpsilon,
                                minimumStableFaceArea * 0.005f))
                        {
                            valid = false;
                            break;
                        }
                        PolygonFace oriented = CreateOrientedFace(
                            triangleNormal,
                            PolygonFaceFeature.Base,
                            0f,
                            triangle.ToArray());
                        trialFaces.Add(new PolygonFace(
                            oriented.Vertices,
                            oriented.Normal,
                            PolygonFaceFeature.Base,
                            0f,
                            PolygonFaceProvenanceKind.BoundedEndpointCap,
                            sharedVertexIndex));
                        trialSignatures.Add(string.Join(
                            "|",
                            triangle.Select(
                                BuildPlaneCutEndpointPatchPointSignature)));
                    }
                    if (!valid || trialFaces.Count == 0)
                    {
                        continue;
                    }
                    List<PolygonFace> trialShell = hybrid
                        .Select(ClonePlaneCutPolygonFace)
                        .ToList();
                    trialShell.AddRange(trialFaces);
                    int residual = CollectPlaneCutOpenEdges(trialShell).Count;
                    if (residual < bestResidual)
                    {
                        bestResidual = residual;
                        bestFaces = trialFaces;
                        bestSignature =
                            centreIndex + ":" +
                            string.Join("/", trialSignatures);
                    }
                }
                if (bestFaces == null)
                {
                    blocker = mechanismName +
                        " produced no non-degenerate boundary cell";
                    evidence.FailureSource = blocker;
                    return false;
                }
                closureFaces.AddRange(bestFaces);
                loopVertexCount += component.Count;
                componentSignatures.Add(
                    "c" + componentIndex + ":r" + bestResidual +
                    ":" + bestSignature);
            }
            evidence.ClosedCycleCount = components.Count(component =>
            {
                Dictionary<VertexKey, int> degree =
                    new Dictionary<VertexKey, int>();
                for (int index = 0; index < component.Count; index++)
                {
                    degree.TryGetValue(
                        component[index].StartKey,
                        out int startDegree);
                    degree[component[index].StartKey] = startDegree + 1;
                    degree.TryGetValue(
                        component[index].EndKey,
                        out int endDegree);
                    degree[component[index].EndKey] = endDegree + 1;
                }
                return degree.Values.All(value => value == 2);
            });
            evidence.TransitionFaceCount = closureFaces.Count;
            signature = mechanismName + ":" +
                string.Join("/", componentSignatures);
            if (closureFaces.Count == 0)
            {
                blocker = mechanismName + " produced no closure faces";
                evidence.FailureSource = blocker;
                return false;
            }
            return true;
        }

        private static bool TryBuildPlaneCutConformingNormalizedClosure(
            List<PolygonFace> hybrid,
            List<int> hybridOriginalFaceIndices,
            ChamferTopologyContext context,
            int sharedVertexIndex,
            float minimumStableFaceArea,
            PlaneCutBevelTerminationOptions options,
            PlaneCutEndpointCellEvidence evidence,
            out List<PolygonFace> closureFaces,
            out string signature,
            out int loopVertexCount,
            out string blocker)
        {
            closureFaces = new List<PolygonFace>();
            signature = string.Empty;
            loopVertexCount = 0;
            blocker = string.Empty;
            List<PlaneCutOpenEdgeRecord> initialOpen =
                CollectPlaneCutOpenEdges(hybrid);
            if (initialOpen.Count == 0)
            {
                evidence.ClosurelessAccepted = true;
                evidence.ResidualOpenEdgeCount = 0;
                evidence.MechanismSignature += ":already-closed";
                return true;
            }

            List<PlaneCutBoundarySegment> normalized;
            int conformedFaceCount = 0;
            int insertedVertexCount = 0;
            if (options.ConformBeforeClosureDecision)
            {
                if (!TryConformPlaneCutHybridBoundaryOwners(
                        hybrid,
                        hybridOriginalFaceIndices,
                        initialOpen,
                        evidence,
                        out conformedFaceCount,
                        out insertedVertexCount,
                        out blocker))
                {
                    evidence.FailureSource = blocker;
                    return false;
                }
                List<PlaneCutOpenEdgeRecord> conformedBeforeDecision =
                    CollectPlaneCutOpenEdges(hybrid);
                evidence.MechanismSignature +=
                    ":preconform=" + conformedFaceCount +
                    "/" + insertedVertexCount +
                    ":open=" + conformedBeforeDecision.Count;
                if (conformedBeforeDecision.Count == 0)
                {
                    evidence.ClosurelessAccepted = true;
                    evidence.ResidualOpenEdgeCount = 0;
                    return true;
                }
                normalized = NormalizePlaneCutBoundarySegments(
                    conformedBeforeDecision);
                if (normalized.Count < 3)
                {
                    blocker =
                        "preconformed cavity retained fewer than three true boundary segments: " +
                        normalized.Count;
                    evidence.FailureSource = blocker;
                    evidence.ResidualOpenEdgeCount =
                        conformedBeforeDecision.Count;
                    return false;
                }
            }
            else
            {
                normalized = NormalizePlaneCutBoundarySegments(initialOpen);
                if (normalized.Count < 3)
                {
                    blocker =
                        "conforming normalized cavity found fewer than three boundary segments";
                    evidence.FailureSource = blocker;
                    return false;
                }
                if (!TryConformPlaneCutHybridBoundaryOwners(
                        hybrid,
                        hybridOriginalFaceIndices,
                        initialOpen,
                        evidence,
                        out conformedFaceCount,
                        out insertedVertexCount,
                        out blocker))
                {
                    evidence.FailureSource = blocker;
                    return false;
                }

                List<PlaneCutOpenEdgeRecord> conformedOpen =
                    CollectPlaneCutOpenEdges(hybrid);
                HashSet<EdgeKey> expected = new HashSet<EdgeKey>(
                    normalized.Select(segment => segment.EdgeKey));
                HashSet<EdgeKey> actual = new HashSet<EdgeKey>(
                    conformedOpen.Select(edge => edge.EdgeKey));
                if (expected.Count != actual.Count ||
                    !expected.SetEquals(actual))
                {
                    blocker =
                        "conforming normalized cavity owner edges did not match the normalized boundary: expected=" +
                        expected.Count + ",actual=" + actual.Count;
                    evidence.FailureSource = blocker;
                    evidence.ResidualOpenEdgeCount = conformedOpen.Count;
                    return false;
                }
            }

            evidence.MechanismSignature =
                (string.IsNullOrEmpty(evidence.MechanismSignature)
                    ? "conforming-normalized-cavity"
                    : evidence.MechanismSignature) +
                ":faces=" + conformedFaceCount +
                ":inserted=" + insertedVertexCount +
                ":segments=" + normalized.Count;
            return TryBuildPlaneCutOrientedHalfEdgeClosure(
                hybrid,
                context,
                sharedVertexIndex,
                minimumStableFaceArea,
                false,
                options.RequireSimpleClosureCycles,
                options.DirectSimpleCycleTriangles,
                evidence,
                out closureFaces,
                out signature,
                out loopVertexCount,
                out blocker);
        }

        private static bool TryConformPlaneCutHybridBoundaryOwners(
            List<PolygonFace> hybrid,
            List<int> hybridOriginalFaceIndices,
            List<PlaneCutOpenEdgeRecord> openEdges,
            PlaneCutEndpointCellEvidence evidence,
            out int conformedFaceCount,
            out int insertedVertexCount,
            out string blocker)
        {
            conformedFaceCount = 0;
            insertedVertexCount = 0;
            blocker = string.Empty;
            if (hybrid == null || hybridOriginalFaceIndices == null ||
                hybrid.Count != hybridOriginalFaceIndices.Count ||
                openEdges == null || openEdges.Count == 0)
            {
                blocker =
                    "conforming normalized cavity owner inputs were incomplete";
                return false;
            }

            List<Vector3> boundaryPoints = new List<Vector3>();
            Dictionary<int, HashSet<EdgeKey>> openKeysByFace =
                new Dictionary<int, HashSet<EdgeKey>>();
            for (int index = 0; index < openEdges.Count; index++)
            {
                PlaneCutOpenEdgeRecord open = openEdges[index];
                AddPointIfDifferent(boundaryPoints, open.Start);
                AddPointIfDifferent(boundaryPoints, open.End);
                if (!openKeysByFace.TryGetValue(
                        open.FaceIndex,
                        out HashSet<EdgeKey> keys))
                {
                    keys = new HashSet<EdgeKey>();
                    openKeysByFace.Add(open.FaceIndex, keys);
                }
                keys.Add(open.EdgeKey);
            }
            float tolerance = Mathf.Max(
                PointMergeDistance * 4f,
                0.00002f);
            foreach (KeyValuePair<int, HashSet<EdgeKey>> pair
                in openKeysByFace.OrderBy(entry => entry.Key))
            {
                int faceIndex = pair.Key;
                if (faceIndex < 0 || faceIndex >= hybrid.Count)
                {
                    blocker =
                        "conforming normalized cavity referenced an invalid owner face";
                    return false;
                }
                PolygonFace face = hybrid[faceIndex];
                List<Vector3> rebuilt = new List<Vector3>();
                int faceInserted = 0;
                for (int vertexIndex = 0;
                     vertexIndex < face.Vertices.Count;
                     vertexIndex++)
                {
                    Vector3 start = face.Vertices[vertexIndex];
                    Vector3 end = face.Vertices[
                        (vertexIndex + 1) % face.Vertices.Count];
                    AddPointIfDifferent(rebuilt, start);
                    EdgeKey key = new EdgeKey(start, end);
                    if (!pair.Value.Contains(key))
                    {
                        continue;
                    }
                    Vector3 axis = end - start;
                    float lengthSqr = axis.sqrMagnitude;
                    if (lengthSqr <= MinimumEdgeLengthSqr)
                    {
                        continue;
                    }
                    List<KeyValuePair<float, Vector3>> splits =
                        new List<KeyValuePair<float, Vector3>>();
                    for (int pointIndex = 0;
                         pointIndex < boundaryPoints.Count;
                         pointIndex++)
                    {
                        Vector3 point = boundaryPoints[pointIndex];
                        float parameter = Vector3.Dot(
                            point - start,
                            axis) / lengthSqr;
                        if (parameter <= 0.000001f ||
                            parameter >= 0.999999f)
                        {
                            continue;
                        }
                        Vector3 closest = start + axis * parameter;
                        if (Vector3.Distance(closest, point) <= tolerance)
                        {
                            splits.Add(new KeyValuePair<float, Vector3>(
                                parameter,
                                point));
                        }
                    }
                    splits.Sort((left, right) =>
                        left.Key.CompareTo(right.Key));
                    for (int splitIndex = 0;
                         splitIndex < splits.Count;
                         splitIndex++)
                    {
                        int before = rebuilt.Count;
                        AddPointIfDifferent(rebuilt, splits[splitIndex].Value);
                        if (rebuilt.Count > before)
                        {
                            faceInserted++;
                        }
                    }
                }
                if (faceInserted == 0)
                {
                    continue;
                }
                if (rebuilt.Count < 3)
                {
                    blocker =
                        "conforming normalized cavity collapsed an owner face";
                    return false;
                }
                hybrid[faceIndex] = new PolygonFace(
                    rebuilt,
                    face.Normal,
                    face.Feature,
                    face.FeatureStrength,
                    face.ProvenanceKind,
                    face.ProvenanceIndex);
                evidence.MutatedHybridFaceIndices.Add(faceIndex);
                conformedFaceCount++;
                insertedVertexCount += faceInserted;
            }
            return true;
        }

        private static bool TryBuildPlaneCutOrientedHalfEdgeClosure(
            List<PolygonFace> hybrid,
            ChamferTopologyContext context,
            int sharedVertexIndex,
            float minimumStableFaceArea,
            bool useCellApex,
            bool requireSimpleCycles,
            bool directSimpleCycleTriangles,
            PlaneCutEndpointCellEvidence evidence,
            out List<PolygonFace> closureFaces,
            out string signature,
            out int loopVertexCount,
            out string blocker)
        {
            closureFaces = new List<PolygonFace>();
            signature = string.Empty;
            loopVertexCount = 0;
            blocker = string.Empty;
            if (!TryBuildPlaneCutNormalizedBoundaryCycles(
                    hybrid,
                    out PlaneCutBoundaryCycleResult cycles,
                    out blocker))
            {
                ApplyPlaneCutBoundaryCycleEvidence(evidence, cycles);
                evidence.FailureSource = blocker;
                return false;
            }
            ApplyPlaneCutBoundaryCycleEvidence(evidence, cycles);
            if (requireSimpleCycles &&
                !TryResolvePlaneCutSimpleBoundaryCycles(
                    cycles,
                    evidence,
                    out blocker))
            {
                evidence.FailureSource = blocker;
                return false;
            }
            Vector3 endpoint = context.Graph.Vertices[
                sharedVertexIndex].Position;
            List<string> signatures = new List<string>();
            for (int cycleIndex = 0;
                 cycleIndex < cycles.Cycles.Count;
                 cycleIndex++)
            {
                Vector3[] cycle = cycles.Cycles[cycleIndex];
                if (!TryTriangulatePlaneCutBoundaryCycle(
                        cycle,
                        endpoint,
                        sharedVertexIndex,
                        minimumStableFaceArea,
                        useCellApex,
                        directSimpleCycleTriangles,
                        out List<PolygonFace> cycleFaces,
                        out string cycleSignature,
                        out blocker))
                {
                    evidence.FailureSource = blocker;
                    return false;
                }
                closureFaces.AddRange(cycleFaces);
                loopVertexCount += cycle.Length;
                signatures.Add(cycleSignature);
            }
            evidence.ClosedCycleCount = cycles.Cycles.Count;
            evidence.TransitionFaceCount = closureFaces.Count;
            signature = string.Join("/", signatures);
            if (closureFaces.Count == 0)
            {
                blocker =
                    "oriented half-edge reconstruction produced no closure faces";
                evidence.FailureSource = blocker;
                return false;
            }
            return true;
        }

        private static bool TryResolvePlaneCutSimpleBoundaryCycles(
            PlaneCutBoundaryCycleResult cycles,
            PlaneCutEndpointCellEvidence evidence,
            out string blocker)
        {
            blocker = string.Empty;
            if (cycles == null || cycles.Cycles.Count == 0)
            {
                blocker = "simple-cycle reconstruction received no cycles";
                return false;
            }

            List<Vector3[]> resolved = new List<Vector3[]>();
            int repeatedVertexCount = 0;
            for (int cycleIndex = 0;
                 cycleIndex < cycles.Cycles.Count;
                 cycleIndex++)
            {
                if (!TrySplitPlaneCutBoundaryWalkIntoSimpleCycles(
                        cycles.Cycles[cycleIndex],
                        resolved,
                        ref repeatedVertexCount,
                        out blocker))
                {
                    return false;
                }
            }

            Dictionary<EdgeKey, int> edgeOwners =
                new Dictionary<EdgeKey, int>();
            Dictionary<VertexKey, int> vertexOwners =
                new Dictionary<VertexKey, int>();
            int sharedEdgeCount = 0;
            for (int cycleIndex = 0;
                 cycleIndex < resolved.Count;
                 cycleIndex++)
            {
                Vector3[] cycle = resolved[cycleIndex];
                HashSet<VertexKey> cycleVertices = new HashSet<VertexKey>();
                for (int vertexIndex = 0;
                     vertexIndex < cycle.Length;
                     vertexIndex++)
                {
                    VertexKey vertexKey = new VertexKey(cycle[vertexIndex]);
                    if (!cycleVertices.Add(vertexKey))
                    {
                        blocker =
                            "simple-cycle reconstruction retained a repeated cycle vertex";
                        return false;
                    }
                    EdgeKey edgeKey = new EdgeKey(
                        cycle[vertexIndex],
                        cycle[(vertexIndex + 1) % cycle.Length]);
                    edgeOwners.TryGetValue(edgeKey, out int edgeCount);
                    edgeOwners[edgeKey] = edgeCount + 1;
                    if (edgeCount > 0)
                    {
                        sharedEdgeCount++;
                    }
                }
                foreach (VertexKey vertexKey in cycleVertices)
                {
                    vertexOwners.TryGetValue(vertexKey, out int ownerCount);
                    vertexOwners[vertexKey] = ownerCount + 1;
                }
            }
            if (sharedEdgeCount > 0)
            {
                blocker = "simple-cycle reconstruction produced " +
                    sharedEdgeCount + " overlapping cycle edges";
                return false;
            }

            int sharedVertexCount = vertexOwners.Count(pair =>
                pair.Value > 1);
            cycles.Cycles.Clear();
            cycles.Cycles.AddRange(resolved);
            cycles.Signature +=
                ":simple=" + resolved.Count +
                ":repeated=" + repeatedVertexCount +
                ":sharedVertices=" + sharedVertexCount +
                ":sharedEdges=0";
            evidence.ClosedCycleCount = resolved.Count;
            evidence.MechanismSignature =
                string.IsNullOrEmpty(evidence.MechanismSignature)
                    ? cycles.Signature
                    : evidence.MechanismSignature + ":" +
                        cycles.Signature;
            return true;
        }

        private static bool TrySplitPlaneCutBoundaryWalkIntoSimpleCycles(
            Vector3[] walk,
            List<Vector3[]> resolved,
            ref int repeatedVertexCount,
            out string blocker)
        {
            blocker = string.Empty;
            if (walk == null || walk.Length < 3)
            {
                blocker = "simple-cycle reconstruction received a degenerate walk";
                return false;
            }
            Queue<List<Vector3>> pending = new Queue<List<Vector3>>();
            pending.Enqueue(new List<Vector3>(walk));
            int guard = walk.Length * 4 + 16;
            while (pending.Count > 0 && guard-- > 0)
            {
                List<Vector3> current = pending.Dequeue();
                Dictionary<VertexKey, int> firstIndex =
                    new Dictionary<VertexKey, int>();
                int repeatedStart = -1;
                int repeatedEnd = -1;
                for (int index = 0; index < current.Count; index++)
                {
                    VertexKey key = new VertexKey(current[index]);
                    if (firstIndex.TryGetValue(key, out int first))
                    {
                        repeatedStart = first;
                        repeatedEnd = index;
                        break;
                    }
                    firstIndex.Add(key, index);
                }
                if (repeatedStart < 0)
                {
                    if (current.Count < 3)
                    {
                        blocker =
                            "simple-cycle reconstruction produced a cycle with fewer than three vertices";
                        return false;
                    }
                    resolved.Add(current.ToArray());
                    continue;
                }

                repeatedVertexCount++;
                List<Vector3> firstCycle = current.GetRange(
                    repeatedStart,
                    repeatedEnd - repeatedStart);
                List<Vector3> secondCycle = new List<Vector3>();
                secondCycle.AddRange(current.GetRange(
                    repeatedEnd,
                    current.Count - repeatedEnd));
                if (repeatedStart > 0)
                {
                    secondCycle.AddRange(current.GetRange(
                        0,
                        repeatedStart));
                }
                if (firstCycle.Count < 3 || secondCycle.Count < 3)
                {
                    blocker =
                        "simple-cycle reconstruction split a self-touching walk into a degenerate cycle";
                    return false;
                }
                pending.Enqueue(firstCycle);
                pending.Enqueue(secondCycle);
            }
            if (pending.Count > 0)
            {
                blocker = "simple-cycle reconstruction exceeded its split guard";
                return false;
            }
            return true;
        }

        private static bool TryBuildPlaneCutBoundaryEdgeCellFanClosure(
            List<PolygonFace> hybrid,
            ChamferTopologyContext context,
            int sharedVertexIndex,
            float minimumStableFaceArea,
            PlaneCutEndpointCellEvidence evidence,
            out List<PolygonFace> closureFaces,
            out string signature,
            out int loopVertexCount,
            out string blocker)
        {
            closureFaces = new List<PolygonFace>();
            signature = string.Empty;
            loopVertexCount = 0;
            blocker = string.Empty;
            List<PlaneCutBoundarySegment> segments =
                NormalizePlaneCutBoundarySegments(
                    CollectPlaneCutOpenEdges(hybrid));
            if (segments.Count < 3)
            {
                blocker =
                    "boundary-edge cell-fan reconstruction found fewer than three boundary segments";
                evidence.FailureSource = blocker;
                return false;
            }

            List<List<PlaneCutBoundarySegment>> components =
                BuildPlaneCutBoundaryComponents(segments);
            evidence.BoundaryComponentCount = components.Count;
            Vector3 endpoint = context.Graph.Vertices[
                sharedVertexIndex].Position;
            List<string> componentSignatures = new List<string>();
            for (int componentIndex = 0;
                 componentIndex < components.Count;
                 componentIndex++)
            {
                List<PlaneCutBoundarySegment> component =
                    components[componentIndex];
                Dictionary<VertexKey, Vector3> positions =
                    new Dictionary<VertexKey, Vector3>();
                Dictionary<VertexKey, int> degree =
                    new Dictionary<VertexKey, int>();
                Vector3 normalSum = Vector3.zero;
                for (int segmentIndex = 0;
                     segmentIndex < component.Count;
                     segmentIndex++)
                {
                    PlaneCutBoundarySegment segment =
                        component[segmentIndex];
                    positions[segment.StartKey] = segment.Start;
                    positions[segment.EndKey] = segment.End;
                    degree.TryGetValue(segment.StartKey, out int startDegree);
                    degree[segment.StartKey] = startDegree + 1;
                    degree.TryGetValue(segment.EndKey, out int endDegree);
                    degree[segment.EndKey] = endDegree + 1;
                    if (segment.FaceIndex >= 0 &&
                        segment.FaceIndex < hybrid.Count)
                    {
                        normalSum += hybrid[segment.FaceIndex].Normal;
                    }
                }
                int oddVertices = degree.Count(pair =>
                    (pair.Value & 1) != 0);
                int branches = degree.Count(pair => pair.Value > 2);
                evidence.OpenChainCount += oddVertices;
                evidence.BranchVertexCount += branches;

                Vector3 centroid = Vector3.zero;
                List<VertexKey> orderedPositionKeys =
                    positions.Keys.ToList();
                orderedPositionKeys.Sort((left, right) =>
                    left.CompareTo(right));
                for (int positionIndex = 0;
                     positionIndex < orderedPositionKeys.Count;
                     positionIndex++)
                {
                    centroid += positions[
                        orderedPositionKeys[positionIndex]];
                }
                centroid /= Mathf.Max(1, positions.Count);
                Vector3 averageNormal = normalSum.sqrMagnitude >
                    MinimumEdgeLengthSqr
                    ? normalSum.normalized
                    : Vector3.up;
                float offset = Mathf.Max(
                    PointMergeDistance * 8f,
                    Mathf.Sqrt(Mathf.Max(
                        TinyFaceAreaEpsilon,
                        minimumStableFaceArea)) * 0.2f);
                Vector3[] candidateCentres =
                {
                    Vector3.Lerp(centroid, endpoint, 0.35f),
                    centroid + averageNormal * offset,
                    centroid - averageNormal * offset
                };

                List<PolygonFace> bestFaces = null;
                string bestSignature = string.Empty;
                int bestResidual = int.MaxValue;
                for (int centreIndex = 0;
                     centreIndex < candidateCentres.Length;
                     centreIndex++)
                {
                    Vector3 centre = candidateCentres[centreIndex];
                    List<PolygonFace> trialFaces =
                        new List<PolygonFace>();
                    List<string> trialSignatures =
                        new List<string>();
                    bool valid = true;
                    for (int segmentIndex = 0;
                         segmentIndex < component.Count;
                         segmentIndex++)
                    {
                        PlaneCutBoundarySegment segment =
                            component[segmentIndex];
                        List<Vector3> triangle = new List<Vector3>
                        {
                            segment.End,
                            segment.Start,
                            centre
                        };
                        Vector3 triangleNormal =
                            CalculatePolygonNormal(triangle);
                        if (!IsFinite(triangleNormal) ||
                            triangleNormal.sqrMagnitude <= 0.000001f ||
                            CalculatePolygonArea(triangle) <= Mathf.Max(
                                TinyFaceAreaEpsilon,
                                minimumStableFaceArea * 0.005f))
                        {
                            valid = false;
                            break;
                        }
                        PolygonFace oriented = CreateOrientedFace(
                            triangleNormal,
                            PolygonFaceFeature.Base,
                            0f,
                            triangle.ToArray());
                        trialFaces.Add(new PolygonFace(
                            oriented.Vertices,
                            oriented.Normal,
                            PolygonFaceFeature.Base,
                            0f,
                            PolygonFaceProvenanceKind.BoundedEndpointCap,
                            sharedVertexIndex));
                        trialSignatures.Add(string.Join(
                            "|",
                            triangle.Select(
                                BuildPlaneCutEndpointPatchPointSignature)));
                    }
                    if (!valid || trialFaces.Count == 0)
                    {
                        continue;
                    }
                    List<PolygonFace> trialShell = hybrid
                        .Select(ClonePlaneCutPolygonFace)
                        .ToList();
                    trialShell.AddRange(trialFaces);
                    int residual = CollectPlaneCutOpenEdges(
                        trialShell).Count;
                    if (residual < bestResidual)
                    {
                        bestResidual = residual;
                        bestFaces = trialFaces;
                        bestSignature =
                            centreIndex + ":" +
                            string.Join("/", trialSignatures);
                    }
                }
                if (bestFaces == null)
                {
                    blocker =
                        "boundary-edge cell-fan reconstruction produced no non-degenerate boundary-edge cell";
                    evidence.FailureSource = blocker;
                    return false;
                }
                closureFaces.AddRange(bestFaces);
                loopVertexCount += component.Count;
                componentSignatures.Add(
                    "c" + componentIndex + ":r" + bestResidual +
                    ":" + bestSignature);
            }
            evidence.ClosedCycleCount = components.Count(component =>
            {
                Dictionary<VertexKey, int> degree =
                    new Dictionary<VertexKey, int>();
                for (int index = 0; index < component.Count; index++)
                {
                    degree.TryGetValue(
                        component[index].StartKey,
                        out int startDegree);
                    degree[component[index].StartKey] = startDegree + 1;
                    degree.TryGetValue(
                        component[index].EndKey,
                        out int endDegree);
                    degree[component[index].EndKey] = endDegree + 1;
                }
                return degree.Values.All(value => value == 2);
            });
            evidence.TransitionFaceCount = closureFaces.Count;
            signature = string.Join("/", componentSignatures);
            if (closureFaces.Count == 0)
            {
                blocker =
                    "boundary-edge cell-fan reconstruction produced no closure faces";
                evidence.FailureSource = blocker;
                return false;
            }
            return true;
        }

        private static bool TryBuildPlaneCutSourceFaceStripClosure(
            List<PolygonFace> hybrid,
            ChamferTopologyContext context,
            int sharedVertexIndex,
            float minimumStableFaceArea,
            PlaneCutEndpointCellEvidence evidence,
            out List<PolygonFace> closureFaces,
            out string signature,
            out int loopVertexCount,
            out string blocker)
        {
            closureFaces = new List<PolygonFace>();
            signature = string.Empty;
            loopVertexCount = 0;
            blocker = string.Empty;
            List<PlaneCutOpenEdgeRecord> open =
                CollectPlaneCutOpenEdges(hybrid);
            List<PlaneCutBoundarySegment> normalized =
                NormalizePlaneCutBoundarySegments(open);
            if (normalized.Count == 0)
            {
                blocker =
                    "source-face strip reconstruction found no open boundary";
                evidence.FailureSource = blocker;
                return false;
            }

            Dictionary<int, List<PlaneCutBoundarySegment>> bySourceFace =
                new Dictionary<int, List<PlaneCutBoundarySegment>>();
            for (int index = 0; index < normalized.Count; index++)
            {
                PlaneCutBoundarySegment segment = normalized[index];
                if (segment.FaceIndex < 0 ||
                    segment.FaceIndex >= hybrid.Count)
                {
                    continue;
                }
                PolygonFace owner = hybrid[segment.FaceIndex];
                if (owner.ProvenanceKind !=
                    PolygonFaceProvenanceKind.SourceFace)
                {
                    continue;
                }
                if (!bySourceFace.TryGetValue(
                        owner.ProvenanceIndex,
                        out List<PlaneCutBoundarySegment> group))
                {
                    group = new List<PlaneCutBoundarySegment>();
                    bySourceFace.Add(owner.ProvenanceIndex, group);
                }
                group.Add(segment);
            }

            List<PolygonFace> working = hybrid
                .Select(ClonePlaneCutPolygonFace)
                .ToList();
            List<string> signatures = new List<string>();
            foreach (KeyValuePair<int, List<PlaneCutBoundarySegment>> pair
                in bySourceFace.OrderBy(entry => entry.Key))
            {
                if (!TryBuildPlaneCutSourceFaceTransitionFaces(
                        working,
                        pair.Key,
                        pair.Value,
                        sharedVertexIndex,
                        minimumStableFaceArea,
                        out List<PolygonFace> sourceFaces,
                        out int sourceLoopVertices,
                        out int sourceComponents,
                        out int sourceChains,
                        out int sourceBranches,
                        out string sourceSignature,
                        out blocker))
                {
                    evidence.BoundaryComponentCount += sourceComponents;
                    evidence.OpenChainCount += sourceChains;
                    evidence.BranchVertexCount += sourceBranches;
                    evidence.FailureSource = blocker;
                    return false;
                }
                evidence.BoundaryComponentCount += sourceComponents;
                evidence.OpenChainCount += sourceChains;
                evidence.BranchVertexCount += sourceBranches;
                closureFaces.AddRange(sourceFaces);
                working.AddRange(sourceFaces);
                loopVertexCount += sourceLoopVertices;
                signatures.Add(pair.Key + ":" + sourceSignature);
            }

            PlaneCutEndpointCellEvidence residualEvidence =
                new PlaneCutEndpointCellEvidence();
            if (!TryBuildPlaneCutOrientedHalfEdgeClosure(
                    working,
                    context,
                    sharedVertexIndex,
                    minimumStableFaceArea,
                    false,
                    false,
                    false,
                    residualEvidence,
                    out List<PolygonFace> residualFaces,
                    out string residualSignature,
                    out int residualVertices,
                    out blocker))
            {
                evidence.BoundaryComponentCount +=
                    residualEvidence.BoundaryComponentCount;
                evidence.ClosedCycleCount +=
                    residualEvidence.ClosedCycleCount;
                evidence.OpenChainCount +=
                    residualEvidence.OpenChainCount;
                evidence.BranchVertexCount +=
                    residualEvidence.BranchVertexCount;
                evidence.FailureSource = blocker;
                return false;
            }
            closureFaces.AddRange(residualFaces);
            loopVertexCount += residualVertices;
            evidence.BoundaryComponentCount +=
                residualEvidence.BoundaryComponentCount;
            evidence.ClosedCycleCount +=
                residualEvidence.ClosedCycleCount;
            evidence.OpenChainCount +=
                residualEvidence.OpenChainCount;
            evidence.BranchVertexCount +=
                residualEvidence.BranchVertexCount;
            evidence.TransitionFaceCount = closureFaces.Count;
            signatures.Add("residual:" + residualSignature);
            signature = string.Join("/", signatures);
            return closureFaces.Count > 0;
        }

        private static bool TryBuildPlaneCutSourceFaceTransitionFaces(
            List<PolygonFace> hybrid,
            int sourceFaceIdentity,
            List<PlaneCutBoundarySegment> segments,
            int sharedVertexIndex,
            float minimumStableFaceArea,
            out List<PolygonFace> faces,
            out int loopVertexCount,
            out int componentCount,
            out int openChainCount,
            out int branchVertexCount,
            out string signature,
            out string blocker)
        {
            faces = new List<PolygonFace>();
            loopVertexCount = 0;
            componentCount = 0;
            openChainCount = 0;
            branchVertexCount = 0;
            signature = string.Empty;
            blocker = string.Empty;
            List<List<PlaneCutBoundarySegment>> components =
                BuildPlaneCutBoundaryComponents(segments);
            componentCount = components.Count;
            List<string> signatures = new List<string>();
            for (int componentIndex = 0;
                 componentIndex < components.Count;
                 componentIndex++)
            {
                List<PlaneCutBoundarySegment> component =
                    components[componentIndex];
                if (!TryOrderPlaneCutBoundaryPath(
                        component,
                        out Vector3[] ordered,
                        out bool closed,
                        out int branches,
                        out blocker))
                {
                    branchVertexCount += branches;
                    return false;
                }
                branchVertexCount += branches;
                if (!closed)
                {
                    openChainCount++;
                }
                if (ordered.Length < 3)
                {
                    blocker =
                        "source-face transition component had fewer than three vertices";
                    return false;
                }
                List<Vector3> polygon = new List<Vector3>(ordered);
                polygon.Reverse();
                Vector3 normal = Vector3.zero;
                for (int faceIndex = 0;
                     faceIndex < hybrid.Count;
                     faceIndex++)
                {
                    PolygonFace candidate = hybrid[faceIndex];
                    if (candidate.ProvenanceKind ==
                            PolygonFaceProvenanceKind.SourceFace &&
                        candidate.ProvenanceIndex == sourceFaceIdentity)
                    {
                        normal = candidate.Normal;
                        break;
                    }
                }
                if (!IsFinite(normal) ||
                    normal.sqrMagnitude <= 0.000001f)
                {
                    blocker =
                        "source-face transition strip could not resolve its owner normal";
                    return false;
                }
                polygon = SanitizePolygon(polygon, normal);
                if (polygon.Count < 3 ||
                    CalculatePolygonArea(polygon) <= Mathf.Max(
                        TinyFaceAreaEpsilon,
                        minimumStableFaceArea * 0.02f))
                {
                    blocker =
                        "source-face transition strip was degenerate";
                    return false;
                }
                PolygonFace oriented = CreateOrientedFace(
                    normal,
                    PolygonFaceFeature.Base,
                    0f,
                    polygon.ToArray());
                faces.Add(new PolygonFace(
                    oriented.Vertices,
                    oriented.Normal,
                    PolygonFaceFeature.Base,
                    0f,
                    PolygonFaceProvenanceKind.BoundedEndpointCap,
                    sharedVertexIndex));
                loopVertexCount += polygon.Count;
                signatures.Add(
                    polygon.Count + ":" +
                    string.Join("|", polygon.Select(
                        BuildPlaneCutEndpointPatchPointSignature)));
            }
            signature = string.Join("/", signatures);
            return true;
        }

        private static bool TryBuildPlaneCutNormalizedBoundaryCycles(
            List<PolygonFace> faces,
            out PlaneCutBoundaryCycleResult result,
            out string blocker)
        {
            result = new PlaneCutBoundaryCycleResult();
            blocker = string.Empty;
            List<PlaneCutBoundarySegment> segments =
                NormalizePlaneCutBoundarySegments(
                    CollectPlaneCutOpenEdges(faces));
            if (segments.Count < 3)
            {
                blocker =
                    "normalized boundary contained fewer than three segments";
                return false;
            }
            List<List<PlaneCutBoundarySegment>> components =
                BuildPlaneCutBoundaryComponents(segments);
            result.ComponentCount = components.Count;
            List<string> signatures = new List<string>();
            for (int componentIndex = 0;
                 componentIndex < components.Count;
                 componentIndex++)
            {
                if (!TryDecomposePlaneCutBoundaryComponentIntoCycles(
                        components[componentIndex],
                        out List<Vector3[]> cycles,
                        out int openChains,
                        out int branches,
                        out string componentSignature,
                        out blocker))
                {
                    result.OpenChainCount += openChains;
                    result.BranchVertexCount += branches;
                    return false;
                }
                result.OpenChainCount += openChains;
                result.BranchVertexCount += branches;
                result.Cycles.AddRange(cycles);
                signatures.Add(componentSignature);
            }
            result.Signature = string.Join("/", signatures);
            if (result.Cycles.Count == 0)
            {
                blocker = "normalized boundary produced no closed cycles";
                return false;
            }
            return true;
        }

        private static List<PlaneCutBoundarySegment>
            NormalizePlaneCutBoundarySegments(
                List<PlaneCutOpenEdgeRecord> openEdges)
        {
            List<PlaneCutBoundarySegment> result =
                new List<PlaneCutBoundarySegment>();
            if (openEdges == null || openEdges.Count == 0)
            {
                return result;
            }
            List<Vector3> points = new List<Vector3>();
            for (int index = 0; index < openEdges.Count; index++)
            {
                AddPointIfDifferent(points, openEdges[index].Start);
                AddPointIfDifferent(points, openEdges[index].End);
            }
            float tolerance = Mathf.Max(
                PointMergeDistance * 4f,
                0.00002f);
            Dictionary<EdgeKey, PlaneCutBoundarySegment> segmentByKey =
                new Dictionary<EdgeKey, PlaneCutBoundarySegment>();
            Dictionary<EdgeKey, int> useCount =
                new Dictionary<EdgeKey, int>();
            for (int edgeIndex = 0;
                 edgeIndex < openEdges.Count;
                 edgeIndex++)
            {
                PlaneCutOpenEdgeRecord edge = openEdges[edgeIndex];
                Vector3 axis = edge.End - edge.Start;
                float lengthSqr = axis.sqrMagnitude;
                if (lengthSqr <= MinimumEdgeLengthSqr)
                {
                    continue;
                }
                List<KeyValuePair<float, Vector3>> splits =
                    new List<KeyValuePair<float, Vector3>>
                    {
                        new KeyValuePair<float, Vector3>(0f, edge.Start),
                        new KeyValuePair<float, Vector3>(1f, edge.End)
                    };
                for (int pointIndex = 0;
                     pointIndex < points.Count;
                     pointIndex++)
                {
                    Vector3 point = points[pointIndex];
                    float parameter = Vector3.Dot(
                        point - edge.Start,
                        axis) / lengthSqr;
                    if (parameter <= 0.000001f ||
                        parameter >= 0.999999f)
                    {
                        continue;
                    }
                    Vector3 closest = edge.Start + axis * parameter;
                    if (Vector3.Distance(closest, point) <= tolerance)
                    {
                        splits.Add(new KeyValuePair<float, Vector3>(
                            parameter,
                            point));
                    }
                }
                splits.Sort((left, right) =>
                    left.Key.CompareTo(right.Key));
                for (int splitIndex = 0;
                     splitIndex < splits.Count - 1;
                     splitIndex++)
                {
                    Vector3 start = splits[splitIndex].Value;
                    Vector3 end = splits[splitIndex + 1].Value;
                    if (AreSamePoint(start, end))
                    {
                        continue;
                    }
                    PlaneCutBoundarySegment segment =
                        new PlaneCutBoundarySegment(
                            edge.FaceIndex,
                            start,
                            end);
                    useCount.TryGetValue(segment.EdgeKey, out int count);
                    useCount[segment.EdgeKey] = count + 1;
                    if (!segmentByKey.ContainsKey(segment.EdgeKey))
                    {
                        segmentByKey.Add(segment.EdgeKey, segment);
                    }
                }
            }
            foreach (KeyValuePair<EdgeKey, PlaneCutBoundarySegment> pair
                in segmentByKey)
            {
                if ((useCount[pair.Key] & 1) != 0)
                {
                    result.Add(pair.Value);
                }
            }
            result.Sort(ComparePlaneCutBoundarySegments);
            return result;
        }

        private static List<List<PlaneCutBoundarySegment>>
            BuildPlaneCutBoundaryComponents(
                List<PlaneCutBoundarySegment> segments)
        {
            List<List<PlaneCutBoundarySegment>> result =
                new List<List<PlaneCutBoundarySegment>>();
            if (segments == null || segments.Count == 0)
            {
                return result;
            }
            Dictionary<VertexKey, List<int>> byVertex =
                new Dictionary<VertexKey, List<int>>();
            for (int index = 0; index < segments.Count; index++)
            {
                AddPlaneCutBoundaryEdgeIndex(
                    byVertex,
                    segments[index].StartKey,
                    index);
                AddPlaneCutBoundaryEdgeIndex(
                    byVertex,
                    segments[index].EndKey,
                    index);
            }
            HashSet<int> remaining = new HashSet<int>(
                Enumerable.Range(0, segments.Count));
            while (remaining.Count > 0)
            {
                int seed = remaining.Min();
                Queue<int> queue = new Queue<int>();
                List<PlaneCutBoundarySegment> component =
                    new List<PlaneCutBoundarySegment>();
                queue.Enqueue(seed);
                remaining.Remove(seed);
                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    PlaneCutBoundarySegment segment = segments[current];
                    component.Add(segment);
                    VertexKey[] keys =
                    {
                        segment.StartKey,
                        segment.EndKey
                    };
                    for (int keyIndex = 0;
                         keyIndex < keys.Length;
                         keyIndex++)
                    {
                        if (!byVertex.TryGetValue(
                                keys[keyIndex],
                                out List<int> connected))
                        {
                            continue;
                        }
                        for (int connectedIndex = 0;
                             connectedIndex < connected.Count;
                             connectedIndex++)
                        {
                            if (remaining.Remove(connected[connectedIndex]))
                            {
                                queue.Enqueue(connected[connectedIndex]);
                            }
                        }
                    }
                }
                result.Add(component);
            }
            return result;
        }

        private static void AddPlaneCutBoundaryEdgeIndex(
            Dictionary<VertexKey, List<int>> byVertex,
            VertexKey key,
            int edgeIndex)
        {
            if (!byVertex.TryGetValue(key, out List<int> indices))
            {
                indices = new List<int>();
                byVertex.Add(key, indices);
            }
            indices.Add(edgeIndex);
        }

        private static int ComparePlaneCutBoundarySegments(
            PlaneCutBoundarySegment left,
            PlaneCutBoundarySegment right)
        {
            int start = left.StartKey.CompareTo(right.StartKey);
            if (start != 0)
            {
                return start;
            }
            int end = left.EndKey.CompareTo(right.EndKey);
            if (end != 0)
            {
                return end;
            }
            return left.FaceIndex.CompareTo(right.FaceIndex);
        }

        private static VertexKey GetMinimumPlaneCutBoundaryVertexKey(
            IEnumerable<VertexKey> keys)
        {
            bool found = false;
            VertexKey minimum = default;
            foreach (VertexKey key in keys)
            {
                if (!found || key.CompareTo(minimum) < 0)
                {
                    minimum = key;
                    found = true;
                }
            }
            return minimum;
        }

        private static bool TryDecomposePlaneCutBoundaryComponentIntoCycles(
            List<PlaneCutBoundarySegment> component,
            out List<Vector3[]> cycles,
            out int openChainCount,
            out int branchVertexCount,
            out string signature,
            out string blocker)
        {
            cycles = new List<Vector3[]>();
            openChainCount = 0;
            branchVertexCount = 0;
            signature = string.Empty;
            blocker = string.Empty;
            Dictionary<VertexKey, List<int>> byVertex =
                new Dictionary<VertexKey, List<int>>();
            Dictionary<VertexKey, Vector3> positions =
                new Dictionary<VertexKey, Vector3>();
            for (int index = 0; index < component.Count; index++)
            {
                PlaneCutBoundarySegment segment = component[index];
                AddPlaneCutBoundaryEdgeIndex(
                    byVertex,
                    segment.StartKey,
                    index);
                AddPlaneCutBoundaryEdgeIndex(
                    byVertex,
                    segment.EndKey,
                    index);
                positions[segment.StartKey] = segment.Start;
                positions[segment.EndKey] = segment.End;
            }
            List<VertexKey> orderedVertexKeys = byVertex.Keys.ToList();
            orderedVertexKeys.Sort((left, right) =>
                left.CompareTo(right));
            for (int vertexIndex = 0;
                 vertexIndex < orderedVertexKeys.Count;
                 vertexIndex++)
            {
                List<int> incidentEdges = byVertex[
                    orderedVertexKeys[vertexIndex]];
                if ((incidentEdges.Count & 1) != 0)
                {
                    openChainCount++;
                }
                if (incidentEdges.Count > 2)
                {
                    branchVertexCount++;
                }
            }
            if (openChainCount > 0)
            {
                blocker =
                    "normalized boundary retained " + openChainCount +
                    " odd-degree chain endpoints";
                return false;
            }

            Dictionary<PlaneCutBoundaryPairKey, int> pairedEdge =
                new Dictionary<PlaneCutBoundaryPairKey, int>();
            for (int vertexIndex = 0;
                 vertexIndex < orderedVertexKeys.Count;
                 vertexIndex++)
            {
                VertexKey vertexKey = orderedVertexKeys[vertexIndex];
                List<int> available = new List<int>(
                    byVertex[vertexKey]);
                available.Sort();
                while (available.Count > 0)
                {
                    int first = available[0];
                    available.RemoveAt(0);
                    int bestPosition = -1;
                    float bestDot = float.PositiveInfinity;
                    Vector3 firstDirection = (
                        component[first].Position(
                            component[first].Other(vertexKey)) -
                        positions[vertexKey]).normalized;
                    for (int candidatePosition = 0;
                         candidatePosition < available.Count;
                         candidatePosition++)
                    {
                        int candidate = available[candidatePosition];
                        Vector3 candidateDirection = (
                            component[candidate].Position(
                                component[candidate].Other(vertexKey)) -
                            positions[vertexKey]).normalized;
                        float dot = Vector3.Dot(
                            firstDirection,
                            candidateDirection);
                        if (dot < bestDot - 0.000001f ||
                            (Mathf.Abs(dot - bestDot) <= 0.000001f &&
                             candidate < (bestPosition < 0
                                 ? int.MaxValue
                                 : available[bestPosition])))
                        {
                            bestDot = dot;
                            bestPosition = candidatePosition;
                        }
                    }
                    if (bestPosition < 0)
                    {
                        blocker =
                            "boundary branch pairing could not resolve an even-degree vertex";
                        return false;
                    }
                    int second = available[bestPosition];
                    available.RemoveAt(bestPosition);
                    pairedEdge[BuildPlaneCutBoundaryPairKey(
                        vertexKey,
                        first)] = second;
                    pairedEdge[BuildPlaneCutBoundaryPairKey(
                        vertexKey,
                        second)] = first;
                }
            }

            HashSet<int> unused = new HashSet<int>(
                Enumerable.Range(0, component.Count));
            List<string> signatures = new List<string>();
            while (unused.Count > 0)
            {
                int startEdge = unused.Min();
                PlaneCutBoundarySegment seed = component[startEdge];
                VertexKey startVertex = seed.StartKey.CompareTo(
                    seed.EndKey) <= 0
                    ? seed.StartKey
                    : seed.EndKey;
                VertexKey currentVertex = startVertex;
                int currentEdge = startEdge;
                List<VertexKey> keys = new List<VertexKey>();
                List<int> traversedEdges = new List<int>();
                int guard = component.Count * 2 + 4;
                while (guard-- > 0)
                {
                    if (!unused.Remove(currentEdge))
                    {
                        blocker =
                            "boundary cycle traversal repeated an edge";
                        return false;
                    }
                    keys.Add(currentVertex);
                    traversedEdges.Add(currentEdge);
                    VertexKey nextVertex =
                        component[currentEdge].Other(currentVertex);
                    if (!pairedEdge.TryGetValue(
                            BuildPlaneCutBoundaryPairKey(
                                nextVertex,
                                currentEdge),
                            out int nextEdge))
                    {
                        blocker =
                            "boundary cycle traversal lost its paired successor";
                        return false;
                    }
                    currentVertex = nextVertex;
                    currentEdge = nextEdge;
                    if (currentVertex.Equals(startVertex) &&
                        currentEdge == startEdge)
                    {
                        break;
                    }
                }
                if (!currentVertex.Equals(startVertex) ||
                    currentEdge != startEdge || keys.Count < 3)
                {
                    blocker =
                        "paired boundary component did not close into a cycle";
                    return false;
                }
                List<Vector3> ordered = keys.Select(key =>
                    positions[key]).ToList();
                int matchingDirection = 0;
                for (int index = 0; index < ordered.Count; index++)
                {
                    Vector3 a = ordered[index];
                    Vector3 b = ordered[(index + 1) % ordered.Count];
                    PlaneCutBoundarySegment segment =
                        component[traversedEdges[index]];
                    if (AreSamePoint(segment.Start, a) &&
                        AreSamePoint(segment.End, b))
                    {
                        matchingDirection++;
                    }
                }
                if (matchingDirection * 2 >= ordered.Count)
                {
                    ordered.Reverse();
                }
                cycles.Add(ordered.ToArray());
                signatures.Add(
                    ordered.Count + ":" +
                    string.Join("|", ordered.Select(
                        BuildPlaneCutEndpointPatchPointSignature)));
            }
            signature = string.Join("/", signatures);
            return true;
        }

        private static PlaneCutBoundaryPairKey
            BuildPlaneCutBoundaryPairKey(
                VertexKey key,
                int edgeIndex)
        {
            return new PlaneCutBoundaryPairKey(key, edgeIndex);
        }

        private static bool TryOrderPlaneCutBoundaryPath(
            List<PlaneCutBoundarySegment> component,
            out Vector3[] ordered,
            out bool closed,
            out int branchVertexCount,
            out string blocker)
        {
            ordered = Array.Empty<Vector3>();
            closed = false;
            branchVertexCount = 0;
            blocker = string.Empty;
            Dictionary<VertexKey, List<int>> byVertex =
                new Dictionary<VertexKey, List<int>>();
            Dictionary<VertexKey, Vector3> positions =
                new Dictionary<VertexKey, Vector3>();
            for (int index = 0; index < component.Count; index++)
            {
                AddPlaneCutBoundaryEdgeIndex(
                    byVertex,
                    component[index].StartKey,
                    index);
                AddPlaneCutBoundaryEdgeIndex(
                    byVertex,
                    component[index].EndKey,
                    index);
                positions[component[index].StartKey] =
                    component[index].Start;
                positions[component[index].EndKey] =
                    component[index].End;
            }
            List<VertexKey> endpoints = new List<VertexKey>();
            foreach (KeyValuePair<VertexKey, List<int>> pair in byVertex)
            {
                if (pair.Value.Count == 1)
                {
                    endpoints.Add(pair.Key);
                }
                else if (pair.Value.Count != 2)
                {
                    branchVertexCount++;
                }
            }
            if (branchVertexCount > 0 ||
                (endpoints.Count != 0 && endpoints.Count != 2))
            {
                blocker =
                    "source-face boundary component retained branched topology";
                return false;
            }
            closed = endpoints.Count == 0;
            VertexKey start = closed
                ? GetMinimumPlaneCutBoundaryVertexKey(byVertex.Keys)
                : GetMinimumPlaneCutBoundaryVertexKey(endpoints);
            List<Vector3> result = new List<Vector3>();
            HashSet<int> used = new HashSet<int>();
            VertexKey current = start;
            int guard = component.Count + 2;
            while (guard-- > 0)
            {
                result.Add(positions[current]);
                int nextEdge = -1;
                List<int> incident = byVertex[current];
                for (int index = 0; index < incident.Count; index++)
                {
                    if (!used.Contains(incident[index]))
                    {
                        nextEdge = incident[index];
                        break;
                    }
                }
                if (nextEdge < 0)
                {
                    break;
                }
                used.Add(nextEdge);
                current = component[nextEdge].Other(current);
                if (closed && current.Equals(start))
                {
                    break;
                }
            }
            if (used.Count != component.Count || result.Count < 3)
            {
                blocker =
                    "source-face boundary path did not consume its component";
                return false;
            }
            ordered = result.ToArray();
            return true;
        }

        private static bool TryTriangulatePlaneCutBoundaryCycle(
            Vector3[] cycle,
            Vector3 endpoint,
            int sharedVertexIndex,
            float minimumStableFaceArea,
            bool useCellApex,
            bool directSimpleCycleTriangles,
            out List<PolygonFace> faces,
            out string signature,
            out string blocker)
        {
            faces = new List<PolygonFace>();
            signature = string.Empty;
            blocker = string.Empty;
            List<Vector3> sanitized = SanitizePolygon(
                new List<Vector3>(cycle),
                CalculatePolygonNormal(new List<Vector3>(cycle)));
            if (sanitized.Count < 3)
            {
                blocker =
                    "boundary reconstruction produced a cycle with fewer than three vertices";
                return false;
            }
            if (directSimpleCycleTriangles && sanitized.Count == 3)
            {
                Vector3 triangleNormal = CalculatePolygonNormal(sanitized);
                if (!IsFinite(triangleNormal) ||
                    triangleNormal.sqrMagnitude <= 0.000001f ||
                    CalculatePolygonArea(sanitized) <= Mathf.Max(
                        TinyFaceAreaEpsilon,
                        minimumStableFaceArea * 0.005f))
                {
                    blocker =
                        "simple-cycle reconstruction produced a degenerate direct triangle";
                    return false;
                }
                PolygonFace orientedTriangle = CreateOrientedFace(
                    triangleNormal,
                    PolygonFaceFeature.Base,
                    0f,
                    sanitized.ToArray());
                faces.Add(new PolygonFace(
                    orientedTriangle.Vertices,
                    orientedTriangle.Normal,
                    PolygonFaceFeature.Base,
                    0f,
                    PolygonFaceProvenanceKind.BoundedEndpointCap,
                    sharedVertexIndex));
                signature = string.Join(
                    "|",
                    sanitized.Select(
                        BuildPlaneCutEndpointPatchPointSignature));
                return true;
            }

            Vector3 centroid = Vector3.zero;
            for (int index = 0; index < sanitized.Count; index++)
            {
                centroid += sanitized[index];
            }
            centroid /= sanitized.Count;
            Vector3 normal = CalculatePolygonNormal(sanitized);
            if (!IsFinite(normal) || normal.sqrMagnitude <= 0.000001f)
            {
                blocker =
                    "boundary reconstruction cycle had no finite orientation";
                return false;
            }
            Vector3 centre = centroid;
            if (useCellApex)
            {
                Vector3 radial = centroid - endpoint;
                float offset = Mathf.Max(
                    PointMergeDistance * 8f,
                    Mathf.Sqrt(Mathf.Max(
                        TinyFaceAreaEpsilon,
                        minimumStableFaceArea)) * 0.15f);
                Vector3 side = radial.sqrMagnitude > MinimumEdgeLengthSqr
                    ? radial.normalized
                    : normal;
                centre = centroid + (normal + side * 0.25f).normalized *
                    offset;
            }
            List<string> triangleSignatures = new List<string>();
            for (int index = 0; index < sanitized.Count; index++)
            {
                List<Vector3> triangle = new List<Vector3>
                {
                    sanitized[index],
                    sanitized[(index + 1) % sanitized.Count],
                    centre
                };
                Vector3 triangleNormal = CalculatePolygonNormal(triangle);
                if (!IsFinite(triangleNormal) ||
                    triangleNormal.sqrMagnitude <= 0.000001f ||
                    CalculatePolygonArea(triangle) <= Mathf.Max(
                        TinyFaceAreaEpsilon,
                        minimumStableFaceArea * 0.005f))
                {
                    blocker =
                        "boundary reconstruction produced a degenerate closure triangle";
                    return false;
                }
                PolygonFace oriented = CreateOrientedFace(
                    triangleNormal,
                    PolygonFaceFeature.Base,
                    0f,
                    triangle.ToArray());
                faces.Add(new PolygonFace(
                    oriented.Vertices,
                    oriented.Normal,
                    PolygonFaceFeature.Base,
                    0f,
                    PolygonFaceProvenanceKind.BoundedEndpointCap,
                    sharedVertexIndex));
                triangleSignatures.Add(
                    string.Join("|", triangle.Select(
                        BuildPlaneCutEndpointPatchPointSignature)));
            }
            signature = string.Join("/", triangleSignatures);
            return true;
        }

        private static void ApplyPlaneCutBoundaryCycleEvidence(
            PlaneCutEndpointCellEvidence evidence,
            PlaneCutBoundaryCycleResult cycles)
        {
            if (evidence == null || cycles == null)
            {
                return;
            }
            evidence.BoundaryComponentCount = cycles.ComponentCount;
            evidence.ClosedCycleCount = cycles.Cycles.Count;
            evidence.OpenChainCount = cycles.OpenChainCount;
            evidence.BranchVertexCount = cycles.BranchVertexCount;
            evidence.MechanismSignature =
                string.IsNullOrEmpty(evidence.MechanismSignature)
                    ? cycles.Signature
                    : evidence.MechanismSignature + ":" +
                        cycles.Signature;
        }


        private static bool TryBuildPlaneCutBevelTransitionClosures(
            List<PolygonFace> hybrid,
            ChamferTopologyContext context,
            int sharedVertexIndex,
            float minimumStableFaceArea,
            out List<PolygonFace> transitionFaces,
            out string signature,
            out string blocker)
        {
            transitionFaces = new List<PolygonFace>();
            signature = string.Empty;
            blocker = string.Empty;
            if (context == null || context.Graph == null ||
                sharedVertexIndex < 0 ||
                sharedVertexIndex >= context.Graph.Vertices.Count)
            {
                blocker =
                    "transition closure endpoint context was unavailable";
                return false;
            }
            Vector3 endpointPosition = context.Graph.Vertices[
                sharedVertexIndex].Position;
            List<PlaneCutOpenEdgeRecord> openEdges =
                CollectPlaneCutOpenEdges(hybrid);
            if (openEdges.Count == 0)
            {
                return true;
            }

            Dictionary<VertexKey, List<int>> edgesByVertex =
                new Dictionary<VertexKey, List<int>>();
            for (int edgeIndex = 0;
                 edgeIndex < openEdges.Count;
                 edgeIndex++)
            {
                PlaneCutOpenEdgeRecord edge = openEdges[edgeIndex];
                if (!edgesByVertex.TryGetValue(
                        edge.StartKey,
                        out List<int> startEdges))
                {
                    startEdges = new List<int>();
                    edgesByVertex.Add(edge.StartKey, startEdges);
                }
                if (!edgesByVertex.TryGetValue(
                        edge.EndKey,
                        out List<int> endEdges))
                {
                    endEdges = new List<int>();
                    edgesByVertex.Add(edge.EndKey, endEdges);
                }
                startEdges.Add(edgeIndex);
                endEdges.Add(edgeIndex);
            }

            HashSet<int> remaining = new HashSet<int>(
                Enumerable.Range(0, openEdges.Count));
            List<string> signatures = new List<string>();
            while (remaining.Count > 0)
            {
                int seed = remaining.First();
                Queue<int> queue = new Queue<int>();
                List<PlaneCutOpenEdgeRecord> component =
                    new List<PlaneCutOpenEdgeRecord>();
                queue.Enqueue(seed);
                remaining.Remove(seed);
                while (queue.Count > 0)
                {
                    int edgeIndex = queue.Dequeue();
                    PlaneCutOpenEdgeRecord edge = openEdges[edgeIndex];
                    component.Add(edge);
                    VertexKey[] keys =
                    {
                        edge.StartKey,
                        edge.EndKey
                    };
                    for (int keyIndex = 0;
                         keyIndex < keys.Length;
                         keyIndex++)
                    {
                        if (!edgesByVertex.TryGetValue(
                                keys[keyIndex],
                                out List<int> connected))
                        {
                            continue;
                        }
                        for (int connectedIndex = 0;
                             connectedIndex < connected.Count;
                             connectedIndex++)
                        {
                            int candidate = connected[connectedIndex];
                            if (remaining.Remove(candidate))
                            {
                                queue.Enqueue(candidate);
                            }
                        }
                    }
                }

                if (!TryOrderPlaneCutBevelTerminationLoop(
                        component,
                        out Vector3[] loop,
                        out string topologySignature,
                        out blocker))
                {
                    blocker =
                        "transition closure could not order an open-edge component: " +
                        blocker;
                    return false;
                }
                List<Vector3> rawLoop = new List<Vector3>(loop);
                List<Vector3> sanitized = SanitizePolygon(
                    rawLoop,
                    CalculatePolygonNormal(rawLoop));
                Vector3 normal = CalculatePolygonNormal(sanitized);
                if (sanitized.Count < 3 || !IsFinite(normal) ||
                    normal.sqrMagnitude <= 0.000001f ||
                    CalculatePolygonArea(sanitized) <= Mathf.Max(
                        TinyFaceAreaEpsilon,
                        minimumStableFaceArea * 0.02f))
                {
                    blocker =
                        "transition closure produced a degenerate polygon";
                    return false;
                }
                Vector3 centroid = Vector3.zero;
                for (int index = 0; index < sanitized.Count; index++)
                {
                    centroid += sanitized[index];
                }
                centroid /= sanitized.Count;
                if (Vector3.Dot(
                        normal,
                        centroid - endpointPosition) < 0f)
                {
                    normal = -normal;
                }
                PolygonFace oriented = CreateOrientedFace(
                    normal,
                    PolygonFaceFeature.Base,
                    0f,
                    sanitized.ToArray());
                transitionFaces.Add(new PolygonFace(
                    oriented.Vertices,
                    oriented.Normal,
                    PolygonFaceFeature.Base,
                    0f,
                    PolygonFaceProvenanceKind.BoundedEndpointCap,
                    sharedVertexIndex));
                signatures.Add(topologySignature);
            }
            signature = string.Join("/", signatures);
            return true;
        }

        private static bool TryPartitionPlaneCutBevelTerminationFace(
            PolygonFace face,
            PlaneCutEndpointCellLimit[] limits,
            float minimumStableFaceArea,
            Dictionary<TopologyEdgeKey, Vector3>[] caches,
            PlaneCutBevelTerminationOptions options,
            out PolygonFace localFragment,
            out List<PolygonFace> remoteFragments,
            out List<Vector3> splitPoints,
            out string blocker)
        {
            localFragment = null;
            remoteFragments = new List<PolygonFace>();
            splitPoints = new List<Vector3>();
            blocker = string.Empty;
            if (face == null || face.Vertices == null ||
                face.Vertices.Count < 3 || limits == null ||
                caches == null || caches.Length != limits.Length)
            {
                blocker = "bevel-termination face partition inputs were incomplete";
                return false;
            }
            float minimumArea = Mathf.Max(
                TinyFaceAreaEpsilon,
                minimumStableFaceArea * 0.05f);
            List<int> effectiveLimitIndices = new List<int>();
            if (options.OwnLimitIncidentPartition &&
                face.ProvenanceKind ==
                    PolygonFaceProvenanceKind.EdgeBevelPlane)
            {
                for (int limitIndex = 0;
                     limitIndex < limits.Length;
                     limitIndex++)
                {
                    if (limits[limitIndex].SourceEdgeIndex ==
                        face.ProvenanceIndex)
                    {
                        effectiveLimitIndices.Add(limitIndex);
                    }
                }
                if (effectiveLimitIndices.Count == 0)
                {
                    for (int limitIndex = 0;
                         limitIndex < limits.Length;
                         limitIndex++)
                    {
                        effectiveLimitIndices.Add(limitIndex);
                    }
                }
                else if (effectiveLimitIndices.Count != 1)
                {
                    blocker =
                        "own-limit partition resolved duplicate axial limits for bevel identity " +
                        face.ProvenanceIndex;
                    return false;
                }
            }
            else
            {
                for (int limitIndex = 0;
                     limitIndex < limits.Length;
                     limitIndex++)
                {
                    effectiveLimitIndices.Add(limitIndex);
                }
            }

            List<Vector3> local = new List<Vector3>(face.Vertices);
            for (int effectiveIndex = 0;
                 effectiveIndex < effectiveLimitIndices.Count;
                 effectiveIndex++)
            {
                int limitIndex = effectiveLimitIndices[effectiveIndex];
                if (!TrySplitPlaneCutEndpointCellPolygon(
                        local,
                        limits[limitIndex].Plane,
                        PointMergeDistance * 2f,
                        caches[limitIndex],
                        out List<Vector3> inside,
                        out List<Vector3> outside,
                        out List<Vector3> intersections))
                {
                    blocker = "bevel-termination axial polygon split failed";
                    return false;
                }
                List<Vector3> sanitizedOutside = SanitizePolygon(
                    outside,
                    face.Normal);
                if (sanitizedOutside.Count >= 3 &&
                    CalculatePolygonArea(sanitizedOutside) > minimumArea)
                {
                    remoteFragments.Add(new PolygonFace(
                        sanitizedOutside,
                        face.Normal,
                        face.Feature,
                        face.FeatureStrength,
                        face.ProvenanceKind,
                        face.ProvenanceIndex));
                }
                splitPoints.AddRange(intersections);
                local = SanitizePolygon(inside, face.Normal);
                if (local.Count < 3 ||
                    CalculatePolygonArea(local) <= minimumArea)
                {
                    localFragment = null;
                    remoteFragments.Clear();
                    splitPoints.Clear();
                    return true;
                }
            }
            localFragment = new PolygonFace(
                local,
                face.Normal,
                face.Feature,
                face.FeatureStrength,
                face.ProvenanceKind,
                face.ProvenanceIndex);
            return true;
        }

        private static bool TrySelectPlaneCutRemoteBevelComponents(
            List<PolygonFace> remoteRemainders,
            List<PlaneCutBevelCandidate> incident,
            HashSet<int> selectedIncidentIdentities,
            ChamferTopologyContext context,
            int sharedVertexIndex,
            PlaneCutRemoteComponentSelection selection,
            PlaneCutEndpointCellEvidence evidence,
            out string blocker)
        {
            blocker = string.Empty;
            if (selection == PlaneCutRemoteComponentSelection.None)
            {
                return true;
            }
            if (remoteRemainders == null || incident == null ||
                selectedIncidentIdentities == null ||
                context == null || context.Graph == null ||
                sharedVertexIndex < 0 ||
                sharedVertexIndex >= context.Graph.Vertices.Count)
            {
                blocker =
                    "remote-component selection inputs were incomplete";
                return false;
            }

            Vector3 origin = context.Graph.Vertices[
                sharedVertexIndex].Position;
            HashSet<int> remove = new HashSet<int>();
            List<string> signatures = new List<string>();
            List<PlaneCutBevelCandidate> orderedIncident =
                new List<PlaneCutBevelCandidate>(incident);
            orderedIncident.Sort((left, right) =>
                left.SourceEdgeIndex.CompareTo(right.SourceEdgeIndex));
            for (int incidentIndex = 0;
                 incidentIndex < orderedIncident.Count;
                 incidentIndex++)
            {
                PlaneCutBevelCandidate edge =
                    orderedIncident[incidentIndex];
                if (!selectedIncidentIdentities.Contains(
                        edge.SourceEdgeIndex))
                {
                    signatures.Add(edge.SourceEdgeIndex + ":untouched");
                    continue;
                }
                List<int> owned = new List<int>();
                for (int faceIndex = 0;
                     faceIndex < remoteRemainders.Count;
                     faceIndex++)
                {
                    PolygonFace face = remoteRemainders[faceIndex];
                    if (face.ProvenanceKind ==
                            PolygonFaceProvenanceKind.EdgeBevelPlane &&
                        face.ProvenanceIndex == edge.SourceEdgeIndex)
                    {
                        owned.Add(faceIndex);
                    }
                }
                if (owned.Count == 0)
                {
                    blocker = "remote-component selection lost bevel identity " +
                        edge.SourceEdgeIndex;
                    return false;
                }

                Dictionary<EdgeKey, List<int>> owners =
                    new Dictionary<EdgeKey, List<int>>();
                for (int localIndex = 0;
                     localIndex < owned.Count;
                     localIndex++)
                {
                    PolygonFace face = remoteRemainders[
                        owned[localIndex]];
                    for (int vertexIndex = 0;
                         vertexIndex < face.Vertices.Count;
                         vertexIndex++)
                    {
                        EdgeKey key = new EdgeKey(
                            face.Vertices[vertexIndex],
                            face.Vertices[(vertexIndex + 1) %
                                face.Vertices.Count]);
                        if (!owners.TryGetValue(
                                key,
                                out List<int> edgeOwners))
                        {
                            edgeOwners = new List<int>();
                            owners.Add(key, edgeOwners);
                        }
                        if (!edgeOwners.Contains(localIndex))
                        {
                            edgeOwners.Add(localIndex);
                        }
                    }
                }

                List<HashSet<int>> adjacency = new List<HashSet<int>>();
                for (int index = 0; index < owned.Count; index++)
                {
                    adjacency.Add(new HashSet<int>());
                }
                foreach (KeyValuePair<EdgeKey, List<int>> pair in owners)
                {
                    if (pair.Value.Count > 2)
                    {
                        blocker = "remote-component selection found a non-manifold fragment seam for bevel identity " +
                            edge.SourceEdgeIndex;
                        return false;
                    }
                    if (pair.Value.Count == 2)
                    {
                        adjacency[pair.Value[0]].Add(pair.Value[1]);
                        adjacency[pair.Value[1]].Add(pair.Value[0]);
                    }
                }

                List<List<int>> components = new List<List<int>>();
                HashSet<int> visited = new HashSet<int>();
                for (int seed = 0; seed < owned.Count; seed++)
                {
                    if (!visited.Add(seed))
                    {
                        continue;
                    }
                    Queue<int> queue = new Queue<int>();
                    List<int> component = new List<int>();
                    queue.Enqueue(seed);
                    while (queue.Count > 0)
                    {
                        int current = queue.Dequeue();
                        component.Add(current);
                        foreach (int neighbour in adjacency[current])
                        {
                            if (visited.Add(neighbour))
                            {
                                queue.Enqueue(neighbour);
                            }
                        }
                    }
                    component.Sort();
                    components.Add(component);
                }
                if (components.Count == 1)
                {
                    signatures.Add(edge.SourceEdgeIndex + ":1");
                    continue;
                }

                int otherVertexIndex = edge.VertexA == sharedVertexIndex
                    ? edge.VertexB
                    : edge.VertexB == sharedVertexIndex
                        ? edge.VertexA
                        : -1;
                if (otherVertexIndex < 0 ||
                    otherVertexIndex >= context.Graph.Vertices.Count)
                {
                    blocker =
                        "remote-component selection encountered a non-incident bevel";
                    return false;
                }
                Vector3 remote = context.Graph.Vertices[
                    otherVertexIndex].Position;
                Vector3 axis = remote - origin;
                float sourceLength = axis.magnitude;
                if (sourceLength <= PointMergeDistance)
                {
                    blocker =
                        "remote-component selection encountered a degenerate source edge";
                    return false;
                }
                axis /= sourceLength;

                int selectedComponent = -1;
                float selectedPrimary = float.NegativeInfinity;
                float selectedReach = float.NegativeInfinity;
                float selectedArea = float.NegativeInfinity;
                string selectedSignature = string.Empty;
                for (int componentIndex = 0;
                     componentIndex < components.Count;
                     componentIndex++)
                {
                    float reach = float.NegativeInfinity;
                    float area = 0f;
                    float nearestRemote = float.PositiveInfinity;
                    List<string> faceSignatures = new List<string>();
                    for (int memberIndex = 0;
                         memberIndex < components[componentIndex].Count;
                         memberIndex++)
                    {
                        PolygonFace face = remoteRemainders[owned[
                            components[componentIndex][memberIndex]]];
                        area += CalculatePolygonArea(face.Vertices);
                        faceSignatures.Add(
                            BuildPlaneCutEndpointPatchFaceSignature(face));
                        for (int vertexIndex = 0;
                             vertexIndex < face.Vertices.Count;
                             vertexIndex++)
                        {
                            Vector3 vertex = face.Vertices[vertexIndex];
                            reach = Mathf.Max(
                                reach,
                                Vector3.Dot(vertex - origin, axis) /
                                    sourceLength);
                            nearestRemote = Mathf.Min(
                                nearestRemote,
                                (vertex - remote).sqrMagnitude);
                        }
                    }
                    faceSignatures.Sort(StringComparer.Ordinal);
                    string componentSignature = string.Join(
                        "/", faceSignatures);
                    float primary = selection ==
                        PlaneCutRemoteComponentSelection.LargestArea
                        ? area
                        : selection == PlaneCutRemoteComponentSelection.
                            NearestRemoteEndpoint
                            ? -nearestRemote
                            : reach;
                    bool better = selectedComponent < 0 ||
                        primary > selectedPrimary + 0.0000001f ||
                        (Mathf.Abs(primary - selectedPrimary) <=
                             0.0000001f &&
                         (reach > selectedReach + 0.0000001f ||
                          (Mathf.Abs(reach - selectedReach) <=
                               0.0000001f &&
                           (area > selectedArea + 0.0000001f ||
                            (Mathf.Abs(area - selectedArea) <=
                                 0.0000001f &&
                             string.CompareOrdinal(
                                 componentSignature,
                                 selectedSignature) < 0)))));
                    if (better)
                    {
                        selectedComponent = componentIndex;
                        selectedPrimary = primary;
                        selectedReach = reach;
                        selectedArea = area;
                        selectedSignature = componentSignature;
                    }
                }

                for (int componentIndex = 0;
                     componentIndex < components.Count;
                     componentIndex++)
                {
                    if (componentIndex == selectedComponent)
                    {
                        continue;
                    }
                    for (int memberIndex = 0;
                         memberIndex < components[componentIndex].Count;
                         memberIndex++)
                    {
                        remove.Add(owned[components[componentIndex][
                            memberIndex]]);
                    }
                }
                signatures.Add(
                    edge.SourceEdgeIndex + ":" + components.Count +
                    ":keep=" + selectedComponent);
            }

            foreach (int faceIndex in remove.OrderByDescending(value => value))
            {
                remoteRemainders.RemoveAt(faceIndex);
            }
            evidence.MechanismSignature +=
                ":remote{" + string.Join("|", signatures) + "}";
            return true;
        }

        private static bool TryConformPlaneCutLocalShellFixedPoint(
            List<PolygonFace> hybrid,
            ChamferTopologyContext context,
            int sharedVertexIndex,
            PlaneCutEndpointCellLimit[] limits,
            PlaneCutEndpointCellEvidence evidence,
            out string blocker)
        {
            blocker = string.Empty;
            if (hybrid == null || context == null ||
                context.Graph == null || limits == null ||
                sharedVertexIndex < 0 ||
                sharedVertexIndex >= context.Graph.Vertices.Count)
            {
                blocker =
                    "fixed-point local conformance inputs were incomplete";
                return false;
            }
            Vector3 endpoint = context.Graph.Vertices[
                sharedVertexIndex].Position;
            float localRadius = PointMergeDistance * 32f;
            for (int index = 0; index < limits.Length; index++)
            {
                localRadius = Mathf.Max(
                    localRadius,
                    limits[index].AxialLimit * 1.75f);
            }
            float localRadiusSqr = localRadius * localRadius;
            float tolerance = Mathf.Max(
                PointMergeDistance * 4f,
                0.00001f);
            float toleranceSqr = tolerance * tolerance;
            int totalInserted = 0;
            int totalFaces = 0;
            int iteration = 0;
            const int maximumIterations = 8;
            for (; iteration < maximumIterations; iteration++)
            {
                Dictionary<VertexKey, Vector3> candidates =
                    new Dictionary<VertexKey, Vector3>();
                for (int faceIndex = 0;
                     faceIndex < hybrid.Count;
                     faceIndex++)
                {
                    PolygonFace face = hybrid[faceIndex];
                    for (int vertexIndex = 0;
                         vertexIndex < face.Vertices.Count;
                         vertexIndex++)
                    {
                        Vector3 vertex = face.Vertices[vertexIndex];
                        if ((vertex - endpoint).sqrMagnitude <=
                            localRadiusSqr)
                        {
                            candidates[new VertexKey(vertex)] = vertex;
                        }
                    }
                }

                List<KeyValuePair<VertexKey, Vector3>>
                    orderedCandidatePairs = candidates.ToList();
                orderedCandidatePairs.Sort((left, right) =>
                    left.Key.CompareTo(right.Key));
                List<Vector3> orderedCandidates =
                    orderedCandidatePairs
                        .Select(pair => pair.Value)
                        .ToList();
                int insertedThisIteration = 0;
                for (int faceIndex = 0;
                     faceIndex < hybrid.Count;
                     faceIndex++)
                {
                    PolygonFace face = hybrid[faceIndex];
                    List<Vector3> rebuilt = new List<Vector3>();
                    int faceInserted = 0;
                    for (int vertexIndex = 0;
                         vertexIndex < face.Vertices.Count;
                         vertexIndex++)
                    {
                        Vector3 start = face.Vertices[vertexIndex];
                        Vector3 end = face.Vertices[
                            (vertexIndex + 1) % face.Vertices.Count];
                        AddPointIfDifferent(rebuilt, start);
                        Vector3 segment = end - start;
                        float lengthSqr = segment.sqrMagnitude;
                        if (lengthSqr <= MinimumEdgeLengthSqr ||
                            DistancePlaneCutEndpointPatchPointToSegmentSquared(
                                endpoint, start, end) > localRadiusSqr)
                        {
                            continue;
                        }
                        List<KeyValuePair<float, Vector3>> insertions =
                            new List<KeyValuePair<float, Vector3>>();
                        foreach (Vector3 candidate in orderedCandidates)
                        {
                            if ((candidate - start).sqrMagnitude <=
                                    toleranceSqr ||
                                (candidate - end).sqrMagnitude <=
                                    toleranceSqr)
                            {
                                continue;
                            }
                            float parameter = Vector3.Dot(
                                candidate - start,
                                segment) / lengthSqr;
                            if (parameter <= 0.00001f ||
                                parameter >= 0.99999f)
                            {
                                continue;
                            }
                            Vector3 projected = start +
                                segment * parameter;
                            if ((candidate - projected).sqrMagnitude >
                                toleranceSqr)
                            {
                                continue;
                            }
                            insertions.Add(
                                new KeyValuePair<float, Vector3>(
                                    parameter,
                                    candidate));
                        }
                        insertions.Sort((left, right) =>
                        {
                            int parameterOrder =
                                left.Key.CompareTo(right.Key);
                            if (parameterOrder != 0)
                            {
                                return parameterOrder;
                            }
                            return new VertexKey(left.Value).CompareTo(
                                new VertexKey(right.Value));
                        });
                        VertexKey lastKey = default;
                        bool hasLast = false;
                        for (int insertionIndex = 0;
                             insertionIndex < insertions.Count;
                             insertionIndex++)
                        {
                            VertexKey key = new VertexKey(
                                insertions[insertionIndex].Value);
                            if (hasLast && key.Equals(lastKey))
                            {
                                continue;
                            }
                            int before = rebuilt.Count;
                            AddPointIfDifferent(
                                rebuilt,
                                insertions[insertionIndex].Value);
                            lastKey = key;
                            hasLast = true;
                            if (rebuilt.Count > before)
                            {
                                faceInserted++;
                            }
                        }
                    }
                    if (faceInserted == 0)
                    {
                        continue;
                    }
                    if (rebuilt.Count < 3)
                    {
                        blocker =
                            "fixed-point local conformance collapsed a polygon";
                        return false;
                    }
                    hybrid[faceIndex] = new PolygonFace(
                        rebuilt,
                        face.Normal,
                        face.Feature,
                        face.FeatureStrength,
                        face.ProvenanceKind,
                        face.ProvenanceIndex);
                    evidence.MutatedHybridFaceIndices.Add(faceIndex);
                    insertedThisIteration += faceInserted;
                    totalFaces++;
                }
                totalInserted += insertedThisIteration;
                if (insertedThisIteration == 0)
                {
                    evidence.MechanismSignature +=
                        ":fixedpoint=" + (iteration + 1) +
                        "/faces=" + totalFaces +
                        "/inserted=" + totalInserted;
                    return true;
                }
            }
            blocker =
                "fixed-point local conformance exceeded " +
                maximumIterations + " iterations";
            return false;
        }

        private static bool TryBuildPlaneCutBevelTerminationCaps(
            List<PolygonFace> hybrid,
            PlaneCutEndpointCellLimit[] limits,
            Dictionary<int, PlaneCutBevelCandidate> incidentByIdentity,
            int sharedVertexIndex,
            float minimumStableFaceArea,
            ChamferTopologyContext context,
            PlaneCutBevelTerminationOptions options,
            out List<PolygonFace> caps,
            out string loopSignature,
            out int totalLoopVertices,
            out string blocker)
        {
            caps = new List<PolygonFace>();
            loopSignature = string.Empty;
            totalLoopVertices = 0;
            blocker = string.Empty;
            List<PlaneCutOpenEdgeRecord> openEdges =
                CollectPlaneCutOpenEdges(hybrid);
            if (openEdges.Count == 0)
            {
                blocker =
                    "bevel termination hybrid shell exposed no termination loops";
                return false;
            }
            Dictionary<int, List<PlaneCutOpenEdgeRecord>> byIdentity =
                new Dictionary<int, List<PlaneCutOpenEdgeRecord>>();
            for (int index = 0; index < limits.Length; index++)
            {
                byIdentity[limits[index].SourceEdgeIndex] =
                    new List<PlaneCutOpenEdgeRecord>();
            }
            float tolerance = Mathf.Max(
                PointMergeDistance * 8f,
                0.0001f);
            for (int edgeIndex = 0;
                 edgeIndex < openEdges.Count;
                 edgeIndex++)
            {
                PlaneCutOpenEdgeRecord open = openEdges[edgeIndex];
                int selectedIdentity = -1;
                float selectedResidual = float.PositiveInfinity;
                for (int limitIndex = 0;
                     limitIndex < limits.Length;
                     limitIndex++)
                {
                    float startResidual = Mathf.Abs(
                        limits[limitIndex].Plane.SignedDistance(open.Start));
                    float endResidual = Mathf.Abs(
                        limits[limitIndex].Plane.SignedDistance(open.End));
                    float residual = startResidual + endResidual;
                    if (startResidual <= tolerance &&
                        endResidual <= tolerance &&
                        (residual < selectedResidual - 0.0000001f ||
                         (Mathf.Abs(residual - selectedResidual) <=
                              0.0000001f &&
                          limits[limitIndex].SourceEdgeIndex <
                              selectedIdentity)))
                    {
                        selectedIdentity =
                            limits[limitIndex].SourceEdgeIndex;
                        selectedResidual = residual;
                    }
                }
                if (selectedIdentity < 0)
                {
                    if (options.Closure ==
                        PlaneCutBevelTerminationClosure.AxialCaps)
                    {
                        blocker =
                            "bevel termination exposed an open edge outside every incident axial plane";
                        return false;
                    }
                    continue;
                }
                byIdentity[selectedIdentity].Add(open);
            }

            List<string> signatures = new List<string>();
            for (int limitIndex = 0;
                 limitIndex < limits.Length;
                 limitIndex++)
            {
                PlaneCutEndpointCellLimit limit = limits[limitIndex];
                if (!byIdentity.TryGetValue(
                        limit.SourceEdgeIndex,
                        out List<PlaneCutOpenEdgeRecord> loopEdges) ||
                    loopEdges.Count < 3 ||
                    !TryOrderPlaneCutBevelTerminationLoop(
                        loopEdges,
                        out Vector3[] loop,
                        out string topologySignature,
                        out blocker))
                {
                    blocker = string.IsNullOrEmpty(blocker)
                        ? "bevel termination did not expose one closed loop for source edge " +
                            limit.SourceEdgeIndex
                        : blocker;
                    return false;
                }
                List<Vector3> sanitized = SanitizePolygon(
                    new List<Vector3>(loop),
                    -limit.Plane.Normal);
                if (sanitized.Count < 3 ||
                    CalculatePolygonArea(sanitized) <= Mathf.Max(
                        TinyFaceAreaEpsilon,
                        minimumStableFaceArea * 0.05f))
                {
                    blocker =
                        "bevel termination cap was degenerate for source edge " +
                        limit.SourceEdgeIndex;
                    return false;
                }
                if (!incidentByIdentity.TryGetValue(
                        limit.SourceEdgeIndex,
                        out PlaneCutBevelCandidate edge))
                {
                    blocker =
                        "bevel termination cap lost its incident candidate identity";
                    return false;
                }
                if (options.Closure ==
                    PlaneCutBevelTerminationClosure.
                        TaperFansAndTransitionLoops)
                {
                    if (context == null || context.Graph == null ||
                        sharedVertexIndex < 0 ||
                        sharedVertexIndex >=
                            context.Graph.Vertices.Count ||
                        limit.OtherVertexIndex < 0 ||
                        limit.OtherVertexIndex >=
                            context.Graph.Vertices.Count)
                    {
                        blocker =
                            "tapered termination lacked its source-edge axis";
                        return false;
                    }
                    Vector3 origin = context.Graph.Vertices[
                        sharedVertexIndex].Position;
                    Vector3 axis = context.Graph.Vertices[
                        limit.OtherVertexIndex].Position - origin;
                    if (axis.sqrMagnitude <= MinimumEdgeLengthSqr)
                    {
                        blocker =
                            "tapered termination encountered a degenerate source edge";
                        return false;
                    }
                    Vector3 tip = origin + axis.normalized *
                        (limit.AxialLimit * options.TaperTipFraction);
                    for (int vertexIndex = 0;
                         vertexIndex < sanitized.Count;
                         vertexIndex++)
                    {
                        Vector3 a = sanitized[vertexIndex];
                        Vector3 b = sanitized[
                            (vertexIndex + 1) % sanitized.Count];
                        List<Vector3> triangle = new List<Vector3>
                        {
                            a,
                            b,
                            tip
                        };
                        Vector3 triangleNormal =
                            CalculatePolygonNormal(triangle);
                        if (!IsFinite(triangleNormal) ||
                            triangleNormal.sqrMagnitude <= 0.000001f ||
                            CalculatePolygonArea(triangle) <= Mathf.Max(
                                TinyFaceAreaEpsilon,
                                minimumStableFaceArea * 0.02f))
                        {
                            blocker =
                                "tapered termination produced a degenerate transition triangle";
                            return false;
                        }
                        PolygonFace tapered = CreateOrientedFace(
                            triangleNormal,
                            PolygonFaceFeature.ConvexEdgeWear,
                            edge.Strength,
                            triangle.ToArray());
                        caps.Add(new PolygonFace(
                            tapered.Vertices,
                            tapered.Normal,
                            PolygonFaceFeature.ConvexEdgeWear,
                            edge.Strength,
                            PolygonFaceProvenanceKind.BoundedEndpointCap,
                            sharedVertexIndex));
                    }
                }
                else
                {
                    PolygonFace oriented = CreateOrientedFace(
                        -limit.Plane.Normal,
                        PolygonFaceFeature.ConvexEdgeWear,
                        edge.Strength,
                        sanitized.ToArray());
                    caps.Add(new PolygonFace(
                        oriented.Vertices,
                        oriented.Normal,
                        PolygonFaceFeature.ConvexEdgeWear,
                        edge.Strength,
                        PolygonFaceProvenanceKind.BoundedEndpointCap,
                        sharedVertexIndex));
                }
                totalLoopVertices += sanitized.Count;
                signatures.Add(
                    limit.SourceEdgeIndex.ToString() + ":" +
                    topologySignature);
            }
            loopSignature = string.Join("/", signatures);
            return signatures.Count == limits.Length;
        }

        private static bool TryOrderPlaneCutBevelTerminationLoop(
            List<PlaneCutOpenEdgeRecord> edges,
            out Vector3[] loop,
            out string topologySignature,
            out string blocker)
        {
            loop = Array.Empty<Vector3>();
            topologySignature = string.Empty;
            blocker = string.Empty;
            Dictionary<VertexKey, List<VertexKey>> adjacency =
                new Dictionary<VertexKey, List<VertexKey>>();
            Dictionary<VertexKey, Vector3> positions =
                new Dictionary<VertexKey, Vector3>();
            HashSet<EdgeKey> expectedEdges = new HashSet<EdgeKey>();
            for (int index = 0; index < edges.Count; index++)
            {
                PlaneCutOpenEdgeRecord edge = edges[index];
                if (edge.StartKey.Equals(edge.EndKey))
                {
                    continue;
                }
                if (!adjacency.TryGetValue(
                        edge.StartKey,
                        out List<VertexKey> startNeighbors))
                {
                    startNeighbors = new List<VertexKey>();
                    adjacency.Add(edge.StartKey, startNeighbors);
                    positions.Add(edge.StartKey, edge.Start);
                }
                if (!adjacency.TryGetValue(
                        edge.EndKey,
                        out List<VertexKey> endNeighbors))
                {
                    endNeighbors = new List<VertexKey>();
                    adjacency.Add(edge.EndKey, endNeighbors);
                    positions.Add(edge.EndKey, edge.End);
                }
                if (!startNeighbors.Contains(edge.EndKey))
                {
                    startNeighbors.Add(edge.EndKey);
                }
                if (!endNeighbors.Contains(edge.StartKey))
                {
                    endNeighbors.Add(edge.StartKey);
                }
                expectedEdges.Add(edge.EdgeKey);
            }
            if (adjacency.Count < 3)
            {
                blocker = "termination loop had fewer than three vertices";
                return false;
            }
            foreach (KeyValuePair<VertexKey, List<VertexKey>> pair in adjacency)
            {
                if (pair.Value.Count != 2)
                {
                    blocker =
                        "termination loop was branched or open at degree " +
                        pair.Value.Count;
                    return false;
                }
            }

            VertexKey start = adjacency.Keys.First();
            foreach (VertexKey candidate in adjacency.Keys)
            {
                Vector3 a = positions[candidate];
                Vector3 b = positions[start];
                if (a.x < b.x ||
                    (Mathf.Approximately(a.x, b.x) &&
                     (a.y < b.y ||
                      (Mathf.Approximately(a.y, b.y) && a.z < b.z))))
                {
                    start = candidate;
                }
            }
            List<Vector3> ordered = new List<Vector3>();
            HashSet<EdgeKey> visited = new HashSet<EdgeKey>();
            VertexKey current = start;
            VertexKey previous = default;
            bool hasPrevious = false;
            for (int step = 0; step <= expectedEdges.Count; step++)
            {
                ordered.Add(positions[current]);
                List<VertexKey> neighbors = adjacency[current];
                VertexKey next;
                if (!hasPrevious)
                {
                    Vector3 first = positions[neighbors[0]];
                    Vector3 second = positions[neighbors[1]];
                    next = first.x < second.x ||
                        (Mathf.Approximately(first.x, second.x) &&
                         (first.y < second.y ||
                          (Mathf.Approximately(first.y, second.y) &&
                           first.z <= second.z)))
                        ? neighbors[0]
                        : neighbors[1];
                }
                else
                {
                    next = neighbors[0].Equals(previous)
                        ? neighbors[1]
                        : neighbors[0];
                }
                EdgeKey traversed = new EdgeKey(
                    positions[current],
                    positions[next]);
                if (!expectedEdges.Contains(traversed) ||
                    !visited.Add(traversed))
                {
                    blocker =
                        "termination loop traversal repeated or left its edge set";
                    return false;
                }
                previous = current;
                current = next;
                hasPrevious = true;
                if (current.Equals(start))
                {
                    break;
                }
            }
            if (!current.Equals(start) ||
                visited.Count != expectedEdges.Count ||
                ordered.Count < 3)
            {
                blocker =
                    "termination edges did not form one connected closed loop";
                return false;
            }
            loop = ordered.ToArray();
            topologySignature =
                ordered.Count.ToString() + ":" +
                string.Join("|", ordered.Select(
                    BuildPlaneCutEndpointPatchPointSignature));
            return true;
        }

        private static bool DoPlaneCutBevelTerminationReplacementsMatch(
            PlaneCutEndpointPatchReplacement prepared,
            PlaneCutEndpointPatchReplacement minimum)
        {
            if (prepared == null || minimum == null ||
                !prepared.ConflictLocalTermination ||
                !minimum.ConflictLocalTermination ||
                prepared.VertexIndex != minimum.VertexIndex ||
                prepared.SelectedFaceCount != minimum.SelectedFaceCount ||
                prepared.TerminationCapCount != minimum.TerminationCapCount ||
                prepared.RemoteIncidentBevelCount !=
                    minimum.RemoteIncidentBevelCount ||
                prepared.RestoredPocketFaceCount !=
                    minimum.RestoredPocketFaceCount ||
                prepared.CellFaceCount != minimum.CellFaceCount ||
                prepared.BoundaryComponentCount !=
                    minimum.BoundaryComponentCount ||
                prepared.ClosedCycleCount != minimum.ClosedCycleCount ||
                prepared.OpenChainCount != minimum.OpenChainCount ||
                prepared.BranchVertexCount != minimum.BranchVertexCount ||
                prepared.TransitionFaceCount !=
                    minimum.TransitionFaceCount ||
                prepared.ResidualOpenEdgeCount !=
                    minimum.ResidualOpenEdgeCount ||
                prepared.ClosurelessAccepted !=
                    minimum.ClosurelessAccepted ||
                !string.Equals(
                    prepared.MechanismSignature,
                    minimum.MechanismSignature,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    prepared.ModifiedIdentitySignature,
                    minimum.ModifiedIdentitySignature,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    prepared.CellLimitSignature,
                    minimum.CellLimitSignature,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    prepared.SelectedProvenanceSignature,
                    minimum.SelectedProvenanceSignature,
                    StringComparison.Ordinal) ||
                prepared.TerminatedSourceEdgeIndices.Length !=
                    minimum.TerminatedSourceEdgeIndices.Length)
            {
                return false;
            }
            for (int index = 0;
                 index < prepared.TerminatedSourceEdgeIndices.Length;
                 index++)
            {
                if (prepared.TerminatedSourceEdgeIndices[index] !=
                    minimum.TerminatedSourceEdgeIndices[index])
                {
                    return false;
                }
            }
            return true;
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
            plan.EndpointPatchRecoveryBoundaryComponentCount = 0;
            plan.EndpointPatchRecoveryClosedCycleCount = 0;
            plan.EndpointPatchRecoveryOpenChainCount = 0;
            plan.EndpointPatchRecoveryBranchVertexCount = 0;
            plan.EndpointPatchRecoveryTransitionFaceCount = 0;
            plan.EndpointPatchRecoveryResidualOpenEdgeCount = 0;
            plan.EndpointPatchRecoveryMechanismSignature = string.Empty;
            plan.EndpointPatchRecoveryModifiedIdentitySignature = string.Empty;
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
            plan.EndpointPatchRecoveryBoundaryComponentCount =
                evidence.BoundaryComponentCount;
            plan.EndpointPatchRecoveryClosedCycleCount =
                evidence.ClosedCycleCount;
            plan.EndpointPatchRecoveryOpenChainCount =
                evidence.OpenChainCount;
            plan.EndpointPatchRecoveryBranchVertexCount =
                evidence.BranchVertexCount;
            plan.EndpointPatchRecoveryTransitionFaceCount =
                evidence.TransitionFaceCount;
            plan.EndpointPatchRecoveryResidualOpenEdgeCount =
                evidence.ResidualOpenEdgeCount;
            plan.EndpointPatchRecoveryMechanismSignature =
                evidence.MechanismSignature ?? string.Empty;
            plan.EndpointPatchRecoveryModifiedIdentitySignature =
                evidence.ModifiedIdentitySignature ?? string.Empty;
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
            List<string> expectedCaps = new List<string>();
            if (replacement.ConflictLocalTermination &&
                !string.IsNullOrEmpty(replacement.TerminationCapSignature))
            {
                expectedCaps.AddRange(
                    replacement.TerminationCapSignature.Split('/'));
            }
            List<string> actual = new List<string>();
            List<string> actualCaps = new List<string>();
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
                    actualCaps.Add(
                        BuildPlaneCutEndpointPatchFaceSignature(face));
                    continue;
                }
                actual.Add(
                    BuildPlaneCutEndpointPatchFaceSignature(face));
            }
            expected.RemoveAll(string.IsNullOrEmpty);
            expectedCaps.RemoveAll(string.IsNullOrEmpty);
            expected.Sort(StringComparer.Ordinal);
            expectedCaps.Sort(StringComparer.Ordinal);
            actual.Sort(StringComparer.Ordinal);
            actualCaps.Sort(StringComparer.Ordinal);
            int expectedCapCount = replacement.ConflictLocalTermination
                ? replacement.TerminationCapCount
                : 1;
            if (capCount != expectedCapCount ||
                expected.Count != actual.Count ||
                (replacement.ConflictLocalTermination &&
                 expectedCaps.Count != actualCaps.Count))
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
            for (int index = 0; index < expectedCaps.Count; index++)
            {
                if (!string.Equals(
                        expectedCaps[index],
                        actualCaps[index],
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
            bool experimentalTermination =
                replacement.ConflictLocalTermination &&
                !string.IsNullOrEmpty(replacement.MechanismSignature);
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
                (replacement.ConflictLocalTermination &&
                 (replacement.TerminatedSourceEdgeIndices == null ||
                  replacement.TerminatedSourceEdgeIndices.Length == 0 ||
                  replacement.TerminationCapCount < 0 ||
                  (!replacement.ClosurelessAccepted &&
                   replacement.TerminationCapCount <= 0) ||
                  (replacement.ClosurelessAccepted &&
                   replacement.TerminationCapCount != 0) ||
                  (!experimentalTermination &&
                   replacement.TerminationCapCount !=
                       replacement.TerminatedSourceEdgeIndices.Length) ||
                  (experimentalTermination &&
                   replacement.ResidualOpenEdgeCount != 0) ||
                  string.IsNullOrEmpty(
                      replacement.TerminationLoopSignature) ||
                  (!replacement.ClosurelessAccepted &&
                   string.IsNullOrEmpty(
                       replacement.TerminationCapSignature)))) ||
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
            if (replacement.ConflictLocalTermination)
            {
                int capCount = 0;
                for (int faceIndex = 0;
                     faceIndex < spliced.Count;
                     faceIndex++)
                {
                    PolygonFace face = spliced[faceIndex];
                    if (face.ProvenanceKind ==
                            PolygonFaceProvenanceKind.BoundedEndpointCap &&
                        face.ProvenanceIndex == replacement.VertexIndex)
                    {
                        capCount++;
                    }
                }
                if (capCount != replacement.TerminationCapCount)
                {
                    blocker =
                        "authoritative bevel termination emitted the wrong bounded-cap count";
                    return false;
                }
            }
            else if (!TryFindSinglePlaneCutProvenanceFace(
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
