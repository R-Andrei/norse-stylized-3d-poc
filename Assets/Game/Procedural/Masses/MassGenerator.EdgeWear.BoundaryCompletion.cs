using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear boundary completion

        private static void ResolveAndAuditChamferSourceBoundaryCompletions(
            List<ChamferVertexPatchComponent> components,
            List<ChamferSourceBoundaryRecord> sourceBoundaryRecords,
            Dictionary<TopologyEdgeKey, int> existingUseCounts,
            HashSet<TopologyEdgeKey> expectedVertexBoundaryEdges,
            HashSet<TopologyEdgeKey> expectedSourceBoundaryEdges,
            HashSet<TopologyEdgeKey> finalSourceBoundaryEdges,
            HashSet<TopologyEdgeKey> remainingVertexPatchBoundaryEdges,
            List<ChamferFinalSourceBoundaryLoop> finalSourceBoundaryLoops,
            ref ChamferEmissionStats stats)
        {
            Dictionary<int, List<ChamferSourceBoundaryRecord>> recordsByLoop =
                new Dictionary<int,
                    List<ChamferSourceBoundaryRecord>>();
            for (int i = 0; i < sourceBoundaryRecords.Count; i++)
            {
                ChamferSourceBoundaryRecord record =
                    sourceBoundaryRecords[i];
                if (!recordsByLoop.TryGetValue(
                        record.BoundaryLoopIndex,
                        out List<ChamferSourceBoundaryRecord> records))
                {
                    records = new List<ChamferSourceBoundaryRecord>();
                    recordsByLoop.Add(record.BoundaryLoopIndex, records);
                }
                records.Add(record);
            }

            Dictionary<int, List<ChamferVertexPatchComponent>>
                candidatesByLoop =
                    new Dictionary<int,
                        List<ChamferVertexPatchComponent>>();
            for (int i = 0; i < components.Count; i++)
            {
                ChamferVertexPatchComponent component = components[i];
                if (component.Closure !=
                        ChamferVertexPatchClosure.OpenChainUnresolved ||
                    !component.SourceFanOpen ||
                    !component.ProvenanceValid)
                {
                    continue;
                }

                HashSet<int> loops = new HashSet<int>();
                for (int recordIndex = 0;
                     recordIndex <
                        component.SourceContext.SourceBoundaryRecords.Count;
                     recordIndex++)
                {
                    loops.Add(component.SourceContext.
                        SourceBoundaryRecords[recordIndex].BoundaryLoopIndex);
                }
                if (loops.Count != 1)
                {
                    continue;
                }

                int loopIndex = -1;
                foreach (int value in loops)
                {
                    loopIndex = value;
                }
                if (!candidatesByLoop.TryGetValue(
                        loopIndex,
                        out List<ChamferVertexPatchComponent> loopCandidates))
                {
                    loopCandidates =
                        new List<ChamferVertexPatchComponent>();
                    candidatesByLoop.Add(loopIndex, loopCandidates);
                }
                loopCandidates.Add(component);
            }

            List<int> loopIndices =
                new List<int>(candidatesByLoop.Keys);
            loopIndices.Sort();
            for (int loopListIndex = 0;
                 loopListIndex < loopIndices.Count;
                 loopListIndex++)
            {
                int loopIndex = loopIndices[loopListIndex];
                List<ChamferVertexPatchComponent> candidates =
                    candidatesByLoop[loopIndex];
                candidates.Sort(CompareChamferVertexPatchComponents);

                recordsByLoop.TryGetValue(
                    loopIndex,
                    out List<ChamferSourceBoundaryRecord> loopRecords);
                loopRecords = loopRecords != null
                    ? new List<ChamferSourceBoundaryRecord>(loopRecords)
                    : new List<ChamferSourceBoundaryRecord>();
                loopRecords.Sort((left, right) =>
                {
                    int comparison = left.BoundaryOrder.CompareTo(
                        right.BoundaryOrder);
                    return comparison != 0
                        ? comparison
                        : left.SourceEdgeIndex.CompareTo(
                            right.SourceEdgeIndex);
                });

                List<ChamferBoundaryCompletionEdge> combinedEdges =
                    new List<ChamferBoundaryCompletionEdge>();
                int sourceNonOneUseEdges = 0;
                for (int recordIndex = 0;
                     recordIndex < loopRecords.Count;
                     recordIndex++)
                {
                    ChamferSourceBoundaryRecord record =
                        loopRecords[recordIndex];
                    for (int childIndex = 0;
                         childIndex < record.Children.Count;
                         childIndex++)
                    {
                        ChamferSourceBoundaryChild child =
                            record.Children[childIndex];
                        if (!expectedSourceBoundaryEdges.Contains(child.Key))
                        {
                            continue;
                        }
                        existingUseCounts.TryGetValue(
                            child.Key,
                            out int useCount);
                        if (useCount != 1)
                        {
                            sourceNonOneUseEdges++;
                        }
                        combinedEdges.Add(
                            new ChamferBoundaryCompletionEdge(
                                child.Key,
                                child.Start,
                                child.End,
                                false,
                                record.BoundaryLoopIndex,
                                record.BoundaryOrder,
                                record.SourceEdgeIndex,
                                childIndex,
                                -1,
                                null));
                    }
                }

                int candidateNonOneUseEdges = 0;
                for (int componentIndex = 0;
                     componentIndex < candidates.Count;
                     componentIndex++)
                {
                    ChamferVertexPatchComponent component =
                        candidates[componentIndex];
                    for (int edgeIndex = 0;
                         edgeIndex < component.OrderedBoundaries.Count;
                         edgeIndex++)
                    {
                        ChamferOrientedVertexBoundary boundary =
                            component.OrderedBoundaries[edgeIndex];
                        existingUseCounts.TryGetValue(
                            boundary.Boundary.Key,
                            out int useCount);
                        if (useCount != 1)
                        {
                            candidateNonOneUseEdges++;
                        }
                        combinedEdges.Add(
                            new ChamferBoundaryCompletionEdge(
                                boundary.Boundary.Key,
                                boundary.Start,
                                boundary.End,
                                true,
                                loopIndex,
                                -1,
                                boundary.Boundary.SourceEdgeIndex,
                                edgeIndex,
                                component.SourceVertexIndex,
                                component));
                    }
                }

                HashSet<TopologyEdgeKey> uniqueKeys =
                    new HashSet<TopologyEdgeKey>();
                Dictionary<VertexKey, List<int>> adjacency =
                    new Dictionary<VertexKey, List<int>>();
                bool duplicateFailure = false;
                bool ownershipConflict =
                    sourceNonOneUseEdges > 0 ||
                    candidateNonOneUseEdges > 0;
                for (int edgeIndex = 0;
                     edgeIndex < combinedEdges.Count;
                     edgeIndex++)
                {
                    ChamferBoundaryCompletionEdge edge =
                        combinedEdges[edgeIndex];
                    if (!uniqueKeys.Add(edge.Key))
                    {
                        duplicateFailure = true;
                    }
                    if (edge.IsCandidate)
                    {
                        ownershipConflict |=
                            !expectedVertexBoundaryEdges.Contains(edge.Key) ||
                            expectedSourceBoundaryEdges.Contains(edge.Key);
                    }
                    else
                    {
                        ownershipConflict |=
                            !expectedSourceBoundaryEdges.Contains(edge.Key) ||
                            expectedVertexBoundaryEdges.Contains(edge.Key);
                    }
                    AddChamferBoundaryCompletionAdjacency(
                        adjacency,
                        edge.StartKey,
                        edgeIndex);
                    AddChamferBoundaryCompletionAdjacency(
                        adjacency,
                        edge.EndKey,
                        edgeIndex);
                }

                int degreeOneVertices = 0;
                int branchVertices = 0;
                foreach (KeyValuePair<VertexKey, List<int>> pair in adjacency)
                {
                    if (pair.Value.Count == 1)
                    {
                        degreeOneVertices++;
                    }
                    else if (pair.Value.Count == 2)
                    {
                    }
                    else
                    {
                        branchVertices++;
                    }
                }
                bool degreeFailure =
                    degreeOneVertices > 0 || branchVertices > 0;
                int connectedComponents =
                    CountChamferBoundaryCompletionComponents(
                        combinedEdges,
                        adjacency);
                bool connectivityFailure = connectedComponents != 1;
                bool closedLoop =
                    combinedEdges.Count >= 3 &&
                    !degreeFailure &&
                    !connectivityFailure &&
                    !duplicateFailure &&
                    !ownershipConflict;



                if (!TryBuildChamferBoundaryCompletionCycles(
                        combinedEdges,
                        adjacency,
                        out List<ChamferBoundaryCompletionCycle> cycles,
                        out string cycleFailure))
                {
                    continue;
                }

                if (closedLoop && cycles.Count == 1)
                {
                    if (TryPromoteChamferSourceBoundaryCompletion(
                            loopIndex,
                            candidates,
                            cycles[0],
                            finalSourceBoundaryEdges,
                            remainingVertexPatchBoundaryEdges,
                            out ChamferFinalSourceBoundaryLoop finalLoop,
                            out string transferFailure))
                    {
                        finalSourceBoundaryLoops.Add(finalLoop);
                        for (int i = 0;
                             i < finalLoop.PromotedComponents.Count;
                             i++)
                        {
                            finalLoop.PromotedComponents[i].Closure =
                                ChamferVertexPatchClosure.OpenChainSourceBoundaryCompletionResolved;
                            finalLoop.PromotedComponents[i].FinalSourceBoundaryLoop =
                                finalLoop;
                        }
                    }
                    continue;
                }

                bool allCyclesClosed =
                    cycles.Count == connectedComponents &&
                    cycles.Count > 1 &&
                    !degreeFailure &&
                    !duplicateFailure &&
                    !ownershipConflict;
                if (allCyclesClosed)
                {
                    bool resolutionApplied =
                        TryResolveChamferSourceBoundaryMultiCycleOwnership(
                            recordsByLoop.Count,
                            loopIndex,
                            loopRecords,
                            candidates,
                            cycles,
                            existingUseCounts,
                            finalSourceBoundaryEdges,
                            remainingVertexPatchBoundaryEdges,
                            out ChamferFinalSourceBoundaryLoop resolvedLoop,
                            out ChamferVertexPatchCluster residualPatchCluster,
                            out int sourceCycleIndex,
                            out int residualPatchCycleIndex,
                            out string resolutionFailure,
                            ref stats);
                    if (resolutionApplied)
                    {
                        finalSourceBoundaryLoops.Add(resolvedLoop);
                    }
                }
            }
        }

        private static bool TryBuildChamferBoundaryCompletionCycles(
            List<ChamferBoundaryCompletionEdge> edges,
            Dictionary<VertexKey, List<int>> adjacency,
            out List<ChamferBoundaryCompletionCycle> cycles,
            out string failure)
        {
            cycles = new List<ChamferBoundaryCompletionCycle>();
            failure = string.Empty;
            HashSet<int> visited = new HashSet<int>();
            for (int seed = 0; seed < edges.Count; seed++)
            {
                if (!visited.Add(seed))
                {
                    continue;
                }
                List<int> componentEdges = new List<int>();
                Queue<int> queue = new Queue<int>();
                queue.Enqueue(seed);
                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    componentEdges.Add(current);
                    ChamferBoundaryCompletionEdge edge = edges[current];
                    EnqueueChamferBoundaryCompletionEdges(
                        edge.StartKey,
                        adjacency,
                        visited,
                        queue);
                    EnqueueChamferBoundaryCompletionEdges(
                        edge.EndKey,
                        adjacency,
                        visited,
                        queue);
                }
                componentEdges.Sort((left, right) =>
                    CompareChamferBoundaryCompletionEdges(
                        edges[left],
                        edges[right]));
                if (!TryOrderChamferBoundaryCompletionCycle(
                        edges,
                        adjacency,
                        componentEdges,
                        out ChamferBoundaryCompletionCycle cycle,
                        out failure))
                {
                    cycles.Clear();
                    return false;
                }
                cycles.Add(cycle);
            }
            cycles.Sort((left, right) =>
                left.MinimumVertex.CompareTo(right.MinimumVertex));
            return true;
        }

        private static bool TryOrderChamferBoundaryCompletionCycle(
            List<ChamferBoundaryCompletionEdge> edges,
            Dictionary<VertexKey, List<int>> adjacency,
            List<int> componentEdges,
            out ChamferBoundaryCompletionCycle cycle,
            out string failure)
        {
            cycle = null;
            failure = string.Empty;
            if (componentEdges.Count < 3)
            {
                failure = "insufficient-cycle-edges/count:" +
                    componentEdges.Count;
                return false;
            }

            HashSet<int> componentSet = new HashSet<int>(componentEdges);
            HashSet<VertexKey> vertices = new HashSet<VertexKey>();
            for (int i = 0; i < componentEdges.Count; i++)
            {
                ChamferBoundaryCompletionEdge edge =
                    edges[componentEdges[i]];
                vertices.Add(edge.StartKey);
                vertices.Add(edge.EndKey);
            }
            foreach (VertexKey vertex in vertices)
            {
                if (!adjacency.TryGetValue(
                        vertex,
                        out List<int> incident))
                {
                    failure = "missing-cycle-adjacency";
                    return false;
                }
                int degree = 0;
                for (int i = 0; i < incident.Count; i++)
                {
                    if (componentSet.Contains(incident[i]))
                    {
                        degree++;
                    }
                }
                if (degree != 2)
                {
                    failure = "cycle-degree/degree:" + degree;
                    return false;
                }
            }

            VertexKey start = GetMinimumChamferVertexKey(vertices);
            List<int> startIncident = adjacency[start];
            int firstEdge = -1;
            for (int i = 0; i < startIncident.Count; i++)
            {
                int candidate = startIncident[i];
                if (!componentSet.Contains(candidate))
                {
                    continue;
                }
                if (firstEdge < 0 ||
                    CompareChamferBoundaryCompletionEdges(
                        edges[candidate],
                        edges[firstEdge]) < 0)
                {
                    firstEdge = candidate;
                }
            }
            if (firstEdge < 0)
            {
                failure = "missing-cycle-first-edge";
                return false;
            }

            List<ChamferFinalSourceBoundaryEdge> ordered =
                new List<ChamferFinalSourceBoundaryEdge>(
                    componentEdges.Count);
            HashSet<int> used = new HashSet<int>();
            HashSet<ChamferVertexPatchComponent> candidateComponents =
                new HashSet<ChamferVertexPatchComponent>();
            HashSet<int> sourceOrders = new HashSet<int>();
            HashSet<int> sourceEdges = new HashSet<int>();
            HashSet<int> candidateSourceVertices = new HashSet<int>();
            VertexKey current = start;
            int currentEdge = firstEdge;
            int guard = 0;
            while (guard++ <= componentEdges.Count)
            {
                if (!used.Add(currentEdge))
                {
                    failure = "cycle-edge-reuse/edge:" + currentEdge;
                    return false;
                }
                ChamferBoundaryCompletionEdge edge = edges[currentEdge];
                bool forward;
                if (current.Equals(edge.StartKey))
                {
                    forward = true;
                }
                else if (current.Equals(edge.EndKey))
                {
                    forward = false;
                }
                else
                {
                    failure = "cycle-edge-disconnect/edge:" + currentEdge;
                    return false;
                }
                ordered.Add(new ChamferFinalSourceBoundaryEdge(
                    edge,
                    forward));
                if (edge.IsCandidate)
                {
                    candidateComponents.Add(edge.Component);
                    candidateSourceVertices.Add(edge.SourceVertexIndex);
                }
                else
                {
                    sourceOrders.Add(edge.BoundaryOrder);
                    sourceEdges.Add(edge.SourceEdgeIndex);
                }
                current = forward ? edge.EndKey : edge.StartKey;
                if (used.Count == componentEdges.Count)
                {
                    break;
                }

                List<int> incident = adjacency[current];
                int next = -1;
                for (int i = 0; i < incident.Count; i++)
                {
                    int candidate = incident[i];
                    if (!componentSet.Contains(candidate) ||
                        used.Contains(candidate))
                    {
                        continue;
                    }
                    if (next >= 0)
                    {
                        failure = "cycle-ambiguous-next";
                        return false;
                    }
                    next = candidate;
                }
                if (next < 0)
                {
                    failure = "cycle-missing-next";
                    return false;
                }
                currentEdge = next;
            }

            if (used.Count != componentEdges.Count ||
                !current.Equals(start))
            {
                failure = "cycle-not-closed/used:" + used.Count +
                    "/expected:" + componentEdges.Count;
                return false;
            }

            cycle = new ChamferBoundaryCompletionCycle(
                ordered,
                vertices,
                candidateComponents,
                sourceOrders,
                sourceEdges,
                candidateSourceVertices,
                start);
            return true;
        }

        private static int CompareChamferBoundaryCompletionEdges(
            ChamferBoundaryCompletionEdge left,
            ChamferBoundaryCompletionEdge right)
        {
            int comparison = left.IsCandidate.CompareTo(right.IsCandidate);
            if (comparison != 0)
            {
                return comparison;
            }
            comparison = left.BoundaryOrder.CompareTo(right.BoundaryOrder);
            if (comparison != 0)
            {
                return comparison;
            }
            comparison = left.SourceVertexIndex.CompareTo(
                right.SourceVertexIndex);
            if (comparison != 0)
            {
                return comparison;
            }
            comparison = left.SourceEdgeIndex.CompareTo(
                right.SourceEdgeIndex);
            return comparison != 0
                ? comparison
                : left.LocalIndex.CompareTo(right.LocalIndex);
        }

        private static bool TryPromoteChamferSourceBoundaryCompletion(
            int loopIndex,
            List<ChamferVertexPatchComponent> candidates,
            ChamferBoundaryCompletionCycle cycle,
            HashSet<TopologyEdgeKey> finalSourceBoundaryEdges,
            HashSet<TopologyEdgeKey> remainingVertexPatchBoundaryEdges,
            out ChamferFinalSourceBoundaryLoop finalLoop,
            out string failure)
        {
            finalLoop = null;
            failure = string.Empty;
            if (cycle.CandidateComponents.Count != candidates.Count)
            {
                failure = "candidate-component-consumption/expected:" +
                    candidates.Count + "/actual:" +
                    cycle.CandidateComponents.Count;
                return false;
            }
            for (int i = 0; i < candidates.Count; i++)
            {
                if (!cycle.CandidateComponents.Contains(candidates[i]))
                {
                    failure = "missing-candidate-component/sourceVertex:" +
                        candidates[i].SourceVertexIndex;
                    return false;
                }
            }

            HashSet<TopologyEdgeKey> promotedKeys =
                new HashSet<TopologyEdgeKey>();
            int promotedEdgeCount = 0;
            for (int i = 0; i < cycle.OrderedEdges.Count; i++)
            {
                ChamferFinalSourceBoundaryEdge edge =
                    cycle.OrderedEdges[i];
                if (!edge.IsPromoted)
                {
                    if (!finalSourceBoundaryEdges.Contains(edge.Key) ||
                        remainingVertexPatchBoundaryEdges.Contains(edge.Key))
                    {
                        failure = "invalid-surviving-source-ownership";
                        return false;
                    }
                    continue;
                }
                if (!promotedKeys.Add(edge.Key) ||
                    !remainingVertexPatchBoundaryEdges.Contains(edge.Key) ||
                    finalSourceBoundaryEdges.Contains(edge.Key))
                {
                    failure = "invalid-promoted-edge-ownership";
                    return false;
                }
                promotedEdgeCount++;
            }

            foreach (TopologyEdgeKey key in promotedKeys)
            {
                remainingVertexPatchBoundaryEdges.Remove(key);
                finalSourceBoundaryEdges.Add(key);
            }
            finalLoop = new ChamferFinalSourceBoundaryLoop(
                loopIndex,
                new List<ChamferFinalSourceBoundaryEdge>(
                    cycle.OrderedEdges),
                new List<ChamferVertexPatchComponent>(candidates),
                promotedEdgeCount);
            return true;
        }

        private static bool TryResolveChamferSourceBoundaryMultiCycleOwnership(
            int originalSourceBoundaryLoopCount,
            int loopIndex,
            List<ChamferSourceBoundaryRecord> loopRecords,
            List<ChamferVertexPatchComponent> candidates,
            List<ChamferBoundaryCompletionCycle> cycles,
            Dictionary<TopologyEdgeKey, int> existingUseCounts,
            HashSet<TopologyEdgeKey> finalSourceBoundaryEdges,
            HashSet<TopologyEdgeKey> remainingVertexPatchBoundaryEdges,
            out ChamferFinalSourceBoundaryLoop finalLoop,
            out ChamferVertexPatchCluster residualPatchCluster,
            out int sourceCycleIndex,
            out int residualPatchCycleIndex,
            out string failure,
            ref ChamferEmissionStats stats)
        {
            finalLoop = null;
            residualPatchCluster = null;
            sourceCycleIndex = -1;
            residualPatchCycleIndex = -1;
            failure = string.Empty;

            if (originalSourceBoundaryLoopCount != 1 ||
                cycles.Count != 2 ||
                candidates.Count != 2)
            {
                failure = "unsupported-multi-cycle-shape/sourceLoops:" +
                    originalSourceBoundaryLoopCount + "/cycles:" +
                    cycles.Count + "/candidates:" + candidates.Count;
                return false;
            }

            bool partitionConsecutiveRanges =
                DoChamferBoundaryCyclesPartitionConsecutiveRanges(
                    cycles,
                    loopRecords.Count);
            bool removedAliasConnectsCycles =
                DoesChamferRemovedBoundaryChildConnectCycles(
                    loopRecords,
                    cycles);
            HashSet<ChamferVertexPatchComponent> seenComponents =
                new HashSet<ChamferVertexPatchComponent>();
            bool lineageValid = true;
            for (int cycleIndex = 0;
                 cycleIndex < cycles.Count;
                 cycleIndex++)
            {
                ChamferBoundaryCompletionCycle cycle = cycles[cycleIndex];
                if (cycle.CandidateComponents.Count != 1)
                {
                    lineageValid = false;
                }
                foreach (ChamferVertexPatchComponent component
                         in cycle.CandidateComponents)
                {
                    if (!seenComponents.Add(component))
                    {
                        lineageValid = false;
                    }
                }
            }
            lineageValid &= seenComponents.Count == candidates.Count;
            if (!partitionConsecutiveRanges ||
                !removedAliasConnectsCycles ||
                !lineageValid)
            {
                failure = "multi-cycle-lineage-guard/partition:" +
                    (partitionConsecutiveRanges ? 1 : 0) +
                    "/removedAliasConnects:" +
                    (removedAliasConnectsCycles ? 1 : 0) +
                    "/lineageValid:" + (lineageValid ? 1 : 0);
                return false;
            }

            Vector3 originalNormal =
                CalculateChamferSourceBoundaryParentLoopNormal(loopRecords);
            if (!IsFinite(originalNormal) ||
                originalNormal.sqrMagnitude <= 0.000001f)
            {
                failure = "invalid-original-boundary-normal";
                return false;
            }

            float[] windingDots = new float[2];
            int strongPositiveCycles = 0;
            for (int cycleIndex = 0; cycleIndex < 2; cycleIndex++)
            {
                Vector3 cycleNormal =
                    CalculateChamferBoundaryCompletionCycleNormal(
                        cycles[cycleIndex]);
                if (!IsFinite(cycleNormal) ||
                    cycleNormal.sqrMagnitude <= 0.000001f)
                {
                    failure = "invalid-cycle-normal/cycle:" + cycleIndex;
                    return false;
                }
                windingDots[cycleIndex] = Vector3.Dot(
                    originalNormal.normalized,
                    cycleNormal.normalized);
                if (windingDots[cycleIndex] >= 0.95f)
                {
                    strongPositiveCycles++;
                    sourceCycleIndex = cycleIndex;
                }
            }

            if (strongPositiveCycles != 1)
            {
                failure = "ambiguous-source-cycle-winding/strongPositive:" +
                    strongPositiveCycles + "/dot0:" +
                    windingDots[0].ToString("F6") + "/dot1:" +
                    windingDots[1].ToString("F6");
                sourceCycleIndex = -1;
                return false;
            }

            residualPatchCycleIndex = 1 - sourceCycleIndex;
            float residualAlignment =
                Mathf.Abs(windingDots[residualPatchCycleIndex]);
            float alignmentSeparation =
                windingDots[sourceCycleIndex] - residualAlignment;
            if (residualAlignment > 0.25f ||
                alignmentSeparation < 0.50f)
            {
                failure = "insufficient-cycle-winding-separation/sourceDot:" +
                    windingDots[sourceCycleIndex].ToString("F6") +
                    "/residualAbsDot:" +
                    residualAlignment.ToString("F6") +
                    "/separation:" +
                    alignmentSeparation.ToString("F6");
                sourceCycleIndex = -1;
                residualPatchCycleIndex = -1;
                return false;
            }

            ChamferBoundaryCompletionCycle sourceCycle =
                cycles[sourceCycleIndex];
            ChamferBoundaryCompletionCycle residualCycle =
                cycles[residualPatchCycleIndex];
            HashSet<TopologyEdgeKey> promotedKeys =
                new HashSet<TopologyEdgeKey>();
            HashSet<TopologyEdgeKey> demotedKeys =
                new HashSet<TopologyEdgeKey>();
            if (!AuditChamferMultiCycleOwnershipBeforeSwap(
                    sourceCycle,
                    true,
                    existingUseCounts,
                    finalSourceBoundaryEdges,
                    remainingVertexPatchBoundaryEdges,
                    promotedKeys,
                    out failure) ||
                !AuditChamferMultiCycleOwnershipBeforeSwap(
                    residualCycle,
                    false,
                    existingUseCounts,
                    finalSourceBoundaryEdges,
                    remainingVertexPatchBoundaryEdges,
                    demotedKeys,
                    out failure) ||
                promotedKeys.Count != 1 ||
                demotedKeys.Count != 1)
            {
                if (string.IsNullOrEmpty(failure))
                {
                    failure = "unexpected-swap-edge-count/promoted:" +
                        promotedKeys.Count + "/demoted:" +
                        demotedKeys.Count;
                }
                return false;
            }

            foreach (TopologyEdgeKey key in promotedKeys)
            {
                if (demotedKeys.Contains(key))
                {
                    failure = "promoted-demoted-key-overlap";
                    return false;
                }
            }

            int originalSourceCount = finalSourceBoundaryEdges.Count;
            int originalPatchCount = remainingVertexPatchBoundaryEdges.Count;
            HashSet<TopologyEdgeKey> originalUnion =
                new HashSet<TopologyEdgeKey>(finalSourceBoundaryEdges);
            originalUnion.UnionWith(remainingVertexPatchBoundaryEdges);
            if (originalUnion.Count !=
                    originalSourceCount + originalPatchCount ||
                originalUnion.Count != stats.ProvisionalOpenEdgeCount)
            {
                failure = "pre-swap-derived-ownership-invariant/source:" +
                    originalSourceCount + "/patch:" + originalPatchCount +
                    "/union:" + originalUnion.Count +
                    "/provisionalOpen:" +
                    stats.ProvisionalOpenEdgeCount;
                return false;
            }

            HashSet<TopologyEdgeKey> proposedSource =
                new HashSet<TopologyEdgeKey>(finalSourceBoundaryEdges);
            HashSet<TopologyEdgeKey> proposedPatch =
                new HashSet<TopologyEdgeKey>(
                    remainingVertexPatchBoundaryEdges);
            foreach (TopologyEdgeKey key in promotedKeys)
            {
                proposedPatch.Remove(key);
                proposedSource.Add(key);
            }
            foreach (TopologyEdgeKey key in demotedKeys)
            {
                proposedSource.Remove(key);
                proposedPatch.Add(key);
            }

            HashSet<TopologyEdgeKey> proposedUnion =
                new HashSet<TopologyEdgeKey>(proposedSource);
            proposedUnion.UnionWith(proposedPatch);
            HashSet<TopologyEdgeKey> proposedOverlap =
                new HashSet<TopologyEdgeKey>(proposedSource);
            proposedOverlap.IntersectWith(proposedPatch);
            if (proposedSource.Count != originalSourceCount ||
                proposedPatch.Count != originalPatchCount ||
                proposedUnion.Count != originalUnion.Count ||
                proposedUnion.Count != stats.ProvisionalOpenEdgeCount ||
                proposedOverlap.Count != 0 ||
                !proposedUnion.SetEquals(originalUnion))
            {
                failure = "post-swap-count-invariant/sourceBefore:" +
                    originalSourceCount + "/sourceAfter:" +
                    proposedSource.Count + "/patchBefore:" +
                    originalPatchCount + "/patchAfter:" +
                    proposedPatch.Count + "/unionBefore:" +
                    originalUnion.Count + "/unionAfter:" +
                    proposedUnion.Count + "/overlap:" +
                    proposedOverlap.Count;
                return false;
            }

            finalSourceBoundaryEdges.Clear();
            finalSourceBoundaryEdges.UnionWith(proposedSource);
            remainingVertexPatchBoundaryEdges.Clear();
            remainingVertexPatchBoundaryEdges.UnionWith(proposedPatch);

            List<ChamferVertexPatchComponent> sourceComponents =
                new List<ChamferVertexPatchComponent>(
                    sourceCycle.CandidateComponents);
            sourceComponents.Sort(CompareChamferVertexPatchComponents);
            finalLoop = new ChamferFinalSourceBoundaryLoop(
                loopIndex,
                new List<ChamferFinalSourceBoundaryEdge>(
                    sourceCycle.OrderedEdges),
                sourceComponents,
                promotedKeys.Count);
            for (int i = 0; i < sourceComponents.Count; i++)
            {
                sourceComponents[i].Closure =
                    ChamferVertexPatchClosure.OpenChainSourceBoundaryMultiCycleResolved;
                sourceComponents[i].FinalSourceBoundaryLoop = finalLoop;
            }

            List<ChamferVertexPatchComponent> residualComponents =
                new List<ChamferVertexPatchComponent>(
                    residualCycle.CandidateComponents);
            residualComponents.Sort(CompareChamferVertexPatchComponents);
            residualPatchCluster = new ChamferVertexPatchCluster(
                residualComponents,
                new List<ChamferFinalSourceBoundaryEdge>(
                    residualCycle.OrderedEdges));
            for (int i = 0; i < residualComponents.Count; i++)
            {
                residualComponents[i].Closure =
                    ChamferVertexPatchClosure.OpenChainSourceBoundaryResidualPatchResolved;
                residualComponents[i].Cluster = residualPatchCluster;
            }

            return true;
        }

        private static bool AuditChamferMultiCycleOwnershipBeforeSwap(
            ChamferBoundaryCompletionCycle cycle,
            bool sourceCycle,
            Dictionary<TopologyEdgeKey, int> existingUseCounts,
            HashSet<TopologyEdgeKey> finalSourceBoundaryEdges,
            HashSet<TopologyEdgeKey> remainingVertexPatchBoundaryEdges,
            HashSet<TopologyEdgeKey> changedKeys,
            out string failure)
        {
            failure = string.Empty;
            for (int edgeIndex = 0;
                 edgeIndex < cycle.OrderedEdges.Count;
                 edgeIndex++)
            {
                ChamferFinalSourceBoundaryEdge edge =
                    cycle.OrderedEdges[edgeIndex];
                existingUseCounts.TryGetValue(edge.Key, out int useCount);
                if (useCount != 1)
                {
                    failure = "multi-cycle-non-one-use/cycleRole:" +
                        (sourceCycle ? "source" : "residual") +
                        "/edge:" + edgeIndex + "/uses:" + useCount;
                    return false;
                }

                bool sourceOwned =
                    finalSourceBoundaryEdges.Contains(edge.Key);
                bool patchOwned =
                    remainingVertexPatchBoundaryEdges.Contains(edge.Key);
                if (edge.IsPromoted)
                {
                    if (sourceOwned || !patchOwned)
                    {
                        failure = "invalid-candidate-ownership/cycleRole:" +
                            (sourceCycle ? "source" : "residual") +
                            "/edge:" + edgeIndex;
                        return false;
                    }
                    if (sourceCycle)
                    {
                        changedKeys.Add(edge.Key);
                    }
                }
                else
                {
                    if (!sourceOwned || patchOwned)
                    {
                        failure = "invalid-source-child-ownership/cycleRole:" +
                            (sourceCycle ? "source" : "residual") +
                            "/edge:" + edgeIndex;
                        return false;
                    }
                    if (!sourceCycle)
                    {
                        changedKeys.Add(edge.Key);
                    }
                }
            }
            return changedKeys.Count > 0;
        }

        private static bool DoChamferBoundaryCyclesPartitionConsecutiveRanges(
            List<ChamferBoundaryCompletionCycle> cycles,
            int recordCount)
        {
            HashSet<int> seen = new HashSet<int>();
            for (int i = 0; i < cycles.Count; i++)
            {
                HashSet<int> orders = cycles[i].SourceBoundaryOrders;
                if (orders.Count == 0 ||
                    !AreChamferBoundaryOrdersCyclicallyConsecutive(
                        orders,
                        recordCount))
                {
                    return false;
                }
                foreach (int order in orders)
                {
                    if (!seen.Add(order))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool AreChamferBoundaryOrdersCyclicallyConsecutive(
            HashSet<int> orders,
            int recordCount)
        {
            if (orders.Count <= 1)
            {
                return orders.Count == 1;
            }
            foreach (int start in orders)
            {
                bool valid = true;
                for (int offset = 0; offset < orders.Count; offset++)
                {
                    if (!orders.Contains((start + offset) % recordCount))
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool DoesChamferRemovedBoundaryChildConnectCycles(
            List<ChamferSourceBoundaryRecord> records,
            List<ChamferBoundaryCompletionCycle> cycles)
        {
            for (int recordIndex = 0;
                 recordIndex < records.Count;
                 recordIndex++)
            {
                ChamferSourceBoundaryRecord record = records[recordIndex];
                for (int removalIndex = 0;
                     removalIndex < record.RemovedChildren.Count;
                     removalIndex++)
                {
                    ChamferSourceBoundaryChildRemoval removal =
                        record.RemovedChildren[removalIndex];
                    int startCycle = FindChamferBoundaryCycleContainingVertex(
                        cycles,
                        new VertexKey(removal.Start));
                    int endCycle = FindChamferBoundaryCycleContainingVertex(
                        cycles,
                        new VertexKey(removal.End));
                    if (startCycle >= 0 &&
                        endCycle >= 0 &&
                        startCycle != endCycle)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static int FindChamferBoundaryCycleContainingVertex(
            List<ChamferBoundaryCompletionCycle> cycles,
            VertexKey vertex)
        {
            int match = -1;
            for (int i = 0; i < cycles.Count; i++)
            {
                if (!cycles[i].Vertices.Contains(vertex))
                {
                    continue;
                }
                if (match >= 0)
                {
                    return -2;
                }
                match = i;
            }
            return match;
        }

        private static Vector3 CalculateChamferSourceBoundaryParentLoopNormal(
            List<ChamferSourceBoundaryRecord> records)
        {
            List<Vector3> positions = new List<Vector3>();
            for (int i = 0; i < records.Count; i++)
            {
                positions.Add(records[i].ParentStart);
            }
            return positions.Count >= 3
                ? CalculatePolygonNormal(positions)
                : Vector3.zero;
        }

        private static Vector3 CalculateChamferBoundaryCompletionCycleNormal(
            ChamferBoundaryCompletionCycle cycle)
        {
            List<Vector3> positions = new List<Vector3>();
            for (int i = 0; i < cycle.OrderedEdges.Count; i++)
            {
                positions.Add(cycle.OrderedEdges[i].Start);
            }
            return positions.Count >= 3
                ? CalculatePolygonNormal(positions)
                : Vector3.zero;
        }



        private static void AddChamferBoundaryCompletionAdjacency(
            Dictionary<VertexKey, List<int>> adjacency,
            VertexKey vertex,
            int edgeIndex)
        {
            if (!adjacency.TryGetValue(vertex, out List<int> incident))
            {
                incident = new List<int>();
                adjacency.Add(vertex, incident);
            }
            incident.Add(edgeIndex);
        }

        private static int CountChamferBoundaryCompletionComponents(
            List<ChamferBoundaryCompletionEdge> edges,
            Dictionary<VertexKey, List<int>> adjacency)
        {
            int componentCount = 0;
            HashSet<int> visited = new HashSet<int>();
            for (int seed = 0; seed < edges.Count; seed++)
            {
                if (!visited.Add(seed))
                {
                    continue;
                }
                componentCount++;
                Queue<int> queue = new Queue<int>();
                queue.Enqueue(seed);
                while (queue.Count > 0)
                {
                    int edgeIndex = queue.Dequeue();
                    ChamferBoundaryCompletionEdge edge = edges[edgeIndex];
                    EnqueueChamferBoundaryCompletionEdges(
                        edge.StartKey,
                        adjacency,
                        visited,
                        queue);
                    EnqueueChamferBoundaryCompletionEdges(
                        edge.EndKey,
                        adjacency,
                        visited,
                        queue);
                }
            }
            return componentCount;
        }

        private static void EnqueueChamferBoundaryCompletionEdges(
            VertexKey vertex,
            Dictionary<VertexKey, List<int>> adjacency,
            HashSet<int> visited,
            Queue<int> queue)
        {
            if (!adjacency.TryGetValue(vertex, out List<int> incident))
            {
                return;
            }
            for (int i = 0; i < incident.Count; i++)
            {
                if (visited.Add(incident[i]))
                {
                    queue.Enqueue(incident[i]);
                }
            }
        }








        private static void IncrementChamferTopologyKeyCount(
            Dictionary<TopologyEdgeKey, int> counts,
            TopologyEdgeKey key)
        {
            counts.TryGetValue(key, out int count);
            counts[key] = count + 1;
        }

        private static int CompareChamferExpectedVertexBoundaryProvenance(
            ChamferExpectedVertexBoundary left,
            ChamferExpectedVertexBoundary right)
        {
            int comparison = left.Kind.CompareTo(right.Kind);
            if (comparison != 0)
            {
                return comparison;
            }
            comparison = left.SourceEdgeIndex.CompareTo(
                right.SourceEdgeIndex);
            if (comparison != 0)
            {
                return comparison;
            }
            comparison = left.FaceIndex.CompareTo(right.FaceIndex);
            if (comparison != 0)
            {
                return comparison;
            }
            comparison = left.Key.First.CompareTo(right.Key.First);
            if (comparison != 0)
            {
                return comparison;
            }
            return left.Key.Second.CompareTo(right.Key.Second);
        }

        private static string BuildChamferVertexPatchComponentDiagnostic(
            ChamferVertexPatchComponent component)
        {
            return
                "open-chain/vertex:" + component.SourceVertexIndex +
                "/edges:" + component.OrderedBoundaries.Count +
                "/sourceFanOpen:" +
                    (component.SourceFanOpen ? 1 : 0) +
                "/activeRuns:" + component.ActiveRunCount +
                "/activeEdges:" +
                    component.SourceContext.ActiveSourceEdges.Count +
                "/sourceBoundaryRecords:" +
                    component.SourceContext.SourceBoundaryRecords.Count +
                "/start:" +
                    component.OrderedPositions[0].ToString("F4") +
                "/end:" +
                    component.OrderedPositions[
                        component.OrderedPositions.Count - 1].ToString("F4") +
                "/startOwners:" +
                    component.StartSourceBoundaryOwnerCount +
                "/endOwners:" +
                    component.EndSourceBoundaryOwnerCount +
                "/startExistingSpokeUses:" +
                    component.StartExistingSpokeUseCount +
                "/startPlannedSpokeUses:" +
                    component.StartPlannedSpokeUseCount +
                "/endExistingSpokeUses:" +
                    component.EndExistingSpokeUseCount +
                "/endPlannedSpokeUses:" +
                    component.EndPlannedSpokeUseCount;
        }

        private static void AddChamferVertexPatchFailure(
            List<string> failures,
            string failure)
        {
            if (failures.Count < 3)
            {
                failures.Add(failure);
            }
        }

        #endregion
    }
}
