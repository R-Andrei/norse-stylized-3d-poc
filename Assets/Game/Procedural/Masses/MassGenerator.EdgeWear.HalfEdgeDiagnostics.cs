using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear directed and half-edge diagnostics





        private static ChamferHalfEdgeDecomposition
            BuildChamferAuthoritativeHalfEdgeDecomposition(
                List<ChamferProvisionalFaceRecord> faceRecords,
                bool treatCoDirectedAsBoundarySectors = false,
                bool includeVertexPatches = false)
        {
            ChamferHalfEdgeDecomposition result =
                new ChamferHalfEdgeDecomposition();
            Dictionary<TopologyEdgeKey, List<int>> usesByKey =
                new Dictionary<TopologyEdgeKey, List<int>>();
            for (int faceRecordIndex = 0;
                 faceRecordIndex < faceRecords.Count;
                 faceRecordIndex++)
            {
                ChamferProvisionalFaceRecord record =
                    faceRecords[faceRecordIndex];
                if ((!includeVertexPatches &&
                     record.Kind ==
                        ChamferProvisionalFaceKind.VertexPatch) ||
                    record.Face == null ||
                    record.Face.Vertices == null ||
                    record.Face.Vertices.Count < 2)
                {
                    continue;
                }
                int firstHalfEdgeIndex = result.HalfEdges.Count;
                int count = record.Face.Vertices.Count;
                for (int cornerIndex = 0;
                     cornerIndex < count;
                     cornerIndex++)
                {
                    Vector3 start = record.Face.Vertices[cornerIndex];
                    Vector3 end = record.Face.Vertices[
                        (cornerIndex + 1) % count];
                    ChamferProvisionalHalfEdge halfEdge =
                        new ChamferProvisionalHalfEdge(
                            result.HalfEdges.Count,
                            faceRecordIndex,
                            cornerIndex,
                            new VertexKey(start),
                            new VertexKey(end),
                            start,
                            end,
                            firstHalfEdgeIndex +
                                ((cornerIndex + 1) % count),
                            firstHalfEdgeIndex +
                                ((cornerIndex - 1 + count) % count),
                            record.Kind,
                            record.SourceFaceIndex,
                            record.SourceEdgeIndex);
                    result.HalfEdges.Add(halfEdge);
                    if (!usesByKey.TryGetValue(
                            halfEdge.UndirectedKey,
                            out List<int> uses))
                    {
                        uses = new List<int>();
                        usesByKey.Add(halfEdge.UndirectedKey, uses);
                    }
                    uses.Add(halfEdge.Index);
                }
            }

            foreach (KeyValuePair<TopologyEdgeKey, List<int>> pair
                     in usesByKey)
            {
                List<int> uses = pair.Value;
                for (int i = 0; i < uses.Count; i++)
                {
                    result.HalfEdges[uses[i]].UndirectedUseCount =
                        uses.Count;
                }
                if (uses.Count == 1)
                {
                    RegisterChamferBoundaryHalfEdge(
                        result,
                        pair.Key,
                        uses[0]);
                    continue;
                }
                if (uses.Count == 2)
                {
                    ChamferProvisionalHalfEdge first =
                        result.HalfEdges[uses[0]];
                    ChamferProvisionalHalfEdge second =
                        result.HalfEdges[uses[1]];
                    if (!first.StartKey.Equals(second.EndKey) ||
                        !first.EndKey.Equals(second.StartKey))
                    {
                        result.CoDirectedPairs.Add(
                            new ChamferCoDirectedHalfEdgePair(
                                first.Index,
                                second.Index));
                        if (treatCoDirectedAsBoundarySectors)
                        {
                            RegisterChamferBoundaryHalfEdge(
                                result,
                                pair.Key,
                                first.Index);
                            RegisterChamferBoundaryHalfEdge(
                                result,
                                pair.Key,
                                second.Index);
                        }
                        else
                        {
                            result.InternalDirectionFailureCount++;
                        }
                        continue;
                    }
                    first.TwinHalfEdgeIndex = second.Index;
                    second.TwinHalfEdgeIndex = first.Index;
                    continue;
                }
                result.InternalDirectionFailureCount++;
            }

            Dictionary<int, List<int>> predecessors =
                new Dictionary<int, List<int>>();
            for (int i = 0;
                 i < result.BoundaryHalfEdges.Count;
                 i++)
            {
                int boundaryHalfEdgeIndex =
                    result.BoundaryHalfEdges[i];
                if (TryFindChamferBoundarySuccessor(
                        boundaryHalfEdgeIndex,
                        result.HalfEdges,
                        out int successor))
                {
                    result.SuccessorByBoundaryHalfEdge[
                        boundaryHalfEdgeIndex] = successor;
                    if (!predecessors.TryGetValue(
                            successor,
                            out List<int> incoming))
                    {
                        incoming = new List<int>();
                        predecessors.Add(successor, incoming);
                    }
                    incoming.Add(boundaryHalfEdgeIndex);
                }
                else
                {
                    result.SuccessorByBoundaryHalfEdge[
                        boundaryHalfEdgeIndex] = -1;
                    result.SuccessorFailureCount++;
                }
            }

            Dictionary<VertexKey, int> incomingByVertex =
                new Dictionary<VertexKey, int>();
            Dictionary<VertexKey, int> outgoingByVertex =
                new Dictionary<VertexKey, int>();
            for (int i = 0;
                 i < result.BoundaryHalfEdges.Count;
                 i++)
            {
                ChamferProvisionalHalfEdge edge = result.HalfEdges[
                    result.BoundaryHalfEdges[i]];
                IncrementChamferVertexKeyCount(
                    outgoingByVertex,
                    edge.StartKey);
                IncrementChamferVertexKeyCount(
                    incomingByVertex,
                    edge.EndKey);
            }
            HashSet<VertexKey> boundaryVertices =
                new HashSet<VertexKey>(incomingByVertex.Keys);
            boundaryVertices.UnionWith(outgoingByVertex.Keys);
            foreach (VertexKey key in boundaryVertices)
            {
                incomingByVertex.TryGetValue(key, out int incoming);
                outgoingByVertex.TryGetValue(key, out int outgoing);
                if (incoming != 1 || outgoing != 1)
                {
                    result.PinchVertices.Add(key);
                }
            }

            Dictionary<int, List<int>> adjacency =
                new Dictionary<int, List<int>>();
            for (int i = 0;
                 i < result.BoundaryHalfEdges.Count;
                 i++)
            {
                int edgeIndex = result.BoundaryHalfEdges[i];
                if (!adjacency.ContainsKey(edgeIndex))
                {
                    adjacency[edgeIndex] = new List<int>();
                }
                int successor = result.SuccessorByBoundaryHalfEdge[
                    edgeIndex];
                if (successor >= 0)
                {
                    AddChamferIntegerAdjacency(
                        adjacency,
                        edgeIndex,
                        successor);
                }
            }

            HashSet<int> visited = new HashSet<int>();
            for (int i = 0;
                 i < result.BoundaryHalfEdges.Count;
                 i++)
            {
                int seed = result.BoundaryHalfEdges[i];
                if (!visited.Add(seed))
                {
                    continue;
                }
                List<int> nodes = new List<int>();
                Queue<int> queue = new Queue<int>();
                queue.Enqueue(seed);
                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    nodes.Add(current);
                    if (!adjacency.TryGetValue(
                            current,
                            out List<int> neighbours))
                    {
                        continue;
                    }
                    for (int neighbourIndex = 0;
                         neighbourIndex < neighbours.Count;
                         neighbourIndex++)
                    {
                        int neighbour = neighbours[neighbourIndex];
                        if (visited.Add(neighbour))
                        {
                            queue.Enqueue(neighbour);
                        }
                    }
                }

                bool ambiguous = false;
                bool closed = nodes.Count > 0;
                int start = -1;
                for (int nodeIndex = 0;
                     nodeIndex < nodes.Count;
                     nodeIndex++)
                {
                    int node = nodes[nodeIndex];
                    int successor = result.SuccessorByBoundaryHalfEdge[
                        node];
                    int incoming = predecessors.TryGetValue(
                        node,
                        out List<int> incomingEdges)
                        ? incomingEdges.Count
                        : 0;
                    if (successor < 0 || incoming != 1)
                    {
                        closed = false;
                    }
                    if (incoming > 1)
                    {
                        ambiguous = true;
                    }
                    if (incoming == 0 && start < 0)
                    {
                        start = node;
                    }
                }
                if (start < 0)
                {
                    start = GetMinimumChamferInteger(nodes);
                }

                List<int> ordered = OrderChamferBoundaryComponent(
                    start,
                    nodes,
                    result.SuccessorByBoundaryHalfEdge,
                    closed);
                ChamferHalfEdgeBoundaryComponentKind kind = ambiguous
                    ? ChamferHalfEdgeBoundaryComponentKind.
                        PinchedOrAmbiguous
                    : closed
                        ? ChamferHalfEdgeBoundaryComponentKind.Loop
                        : ChamferHalfEdgeBoundaryComponentKind.OpenChain;
                ChamferHalfEdgeBoundaryComponent component =
                    new ChamferHalfEdgeBoundaryComponent(
                        result.Components.Count,
                        kind,
                        ordered);
                result.Components.Add(component);
                if (kind == ChamferHalfEdgeBoundaryComponentKind.Loop)
                {
                    result.LoopCount++;
                }
                else if (kind ==
                    ChamferHalfEdgeBoundaryComponentKind.OpenChain)
                {
                    result.OpenChainCount++;
                }
                for (int nodeIndex = 0;
                     nodeIndex < nodes.Count;
                     nodeIndex++)
                {
                    int node = nodes[nodeIndex];
                    if (result.ComponentByHalfEdge.ContainsKey(node))
                    {
                        result.MultiAssignedBoundaryEdgeCount++;
                    }
                    else
                    {
                        result.ComponentByHalfEdge[node] =
                            component.ComponentIndex;
                    }
                }
            }

            for (int i = 0;
                 i < result.BoundaryHalfEdges.Count;
                 i++)
            {
                if (!result.ComponentByHalfEdge.ContainsKey(
                        result.BoundaryHalfEdges[i]))
                {
                    result.UnassignedBoundaryEdgeCount++;
                }
            }
            return result;
        }

        private static void RegisterChamferBoundaryHalfEdge(
            ChamferHalfEdgeDecomposition decomposition,
            TopologyEdgeKey key,
            int halfEdgeIndex)
        {
            ChamferProvisionalHalfEdge halfEdge =
                decomposition.HalfEdges[halfEdgeIndex];
            halfEdge.IsBoundaryHalfEdge = true;
            decomposition.BoundaryHalfEdges.Add(halfEdgeIndex);
            if (!decomposition.BoundaryHalfEdgesByKey.TryGetValue(
                    key,
                    out List<int> boundaryUses))
            {
                boundaryUses = new List<int>();
                decomposition.BoundaryHalfEdgesByKey.Add(
                    key,
                    boundaryUses);
            }
            boundaryUses.Add(halfEdgeIndex);
        }

        private static bool TryFindChamferBoundarySuccessor(
            int boundaryHalfEdgeIndex,
            List<ChamferProvisionalHalfEdge> halfEdges,
            out int successor)
        {
            successor = -1;
            if (boundaryHalfEdgeIndex < 0 ||
                boundaryHalfEdgeIndex >= halfEdges.Count)
            {
                return false;
            }
            int candidate = halfEdges[boundaryHalfEdgeIndex].NextInFace;
            HashSet<int> visitedSectors = new HashSet<int>();
            int guard = 0;
            while (candidate >= 0 &&
                   candidate < halfEdges.Count &&
                   guard++ <= halfEdges.Count)
            {
                if (!visitedSectors.Add(candidate))
                {
                    return false;
                }
                ChamferProvisionalHalfEdge edge = halfEdges[candidate];
                if (edge.IsBoundaryHalfEdge)
                {
                    successor = candidate;
                    return true;
                }
                if (edge.UndirectedUseCount != 2 ||
                    edge.TwinHalfEdgeIndex < 0 ||
                    edge.TwinHalfEdgeIndex >= halfEdges.Count)
                {
                    return false;
                }
                candidate = halfEdges[
                    edge.TwinHalfEdgeIndex].NextInFace;
            }
            return false;
        }

        private static List<int> OrderChamferBoundaryComponent(
            int start,
            List<int> nodes,
            Dictionary<int, int> successorByHalfEdge,
            bool closed)
        {
            HashSet<int> allowed = new HashSet<int>(nodes);
            HashSet<int> visited = new HashSet<int>();
            List<int> ordered = new List<int>(nodes.Count);
            int current = start;
            int guard = 0;
            while (allowed.Contains(current) &&
                   visited.Add(current) &&
                   guard++ <= nodes.Count)
            {
                ordered.Add(current);
                if (!successorByHalfEdge.TryGetValue(
                        current,
                        out int successor) ||
                    successor < 0)
                {
                    break;
                }
                current = successor;
                if (closed && current == start)
                {
                    break;
                }
            }
            if (ordered.Count != nodes.Count)
            {
                List<int> remaining = new List<int>();
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (!visited.Contains(nodes[i]))
                    {
                        remaining.Add(nodes[i]);
                    }
                }
                remaining.Sort();
                ordered.AddRange(remaining);
            }
            return ordered;
        }

        private static void AddChamferIntegerAdjacency(
            Dictionary<int, List<int>> adjacency,
            int first,
            int second)
        {
            if (!adjacency.TryGetValue(first, out List<int> firstList))
            {
                firstList = new List<int>();
                adjacency[first] = firstList;
            }
            if (!firstList.Contains(second))
            {
                firstList.Add(second);
            }
            if (!adjacency.TryGetValue(second, out List<int> secondList))
            {
                secondList = new List<int>();
                adjacency[second] = secondList;
            }
            if (!secondList.Contains(first))
            {
                secondList.Add(first);
            }
        }

        private static void IncrementChamferVertexKeyCount(
            Dictionary<VertexKey, int> counts,
            VertexKey key)
        {
            counts.TryGetValue(key, out int count);
            counts[key] = count + 1;
        }

        private static int GetMinimumChamferInteger(List<int> values)
        {
            int minimum = values.Count > 0 ? values[0] : -1;
            for (int i = 1; i < values.Count; i++)
            {
                minimum = Mathf.Min(minimum, values[i]);
            }
            return minimum;
        }

        private static int GetOnlyChamferInteger(HashSet<int> values)
        {
            foreach (int value in values)
            {
                return value;
            }
            return -1;
        }

        private static HashSet<TopologyEdgeKey>
            BuildChamferHalfEdgeComponentKeySet(
                ChamferHalfEdgeBoundaryComponent component,
                List<ChamferProvisionalHalfEdge> halfEdges)
        {
            HashSet<TopologyEdgeKey> keys =
                new HashSet<TopologyEdgeKey>();
            for (int i = 0;
                 i < component.OrderedHalfEdgeIndices.Count;
                 i++)
            {
                keys.Add(halfEdges[
                    component.OrderedHalfEdgeIndices[i]].UndirectedKey);
            }
            return keys;
        }

        private static bool DoesChamferHalfEdgeComponentMatchPatchOrder(
            List<TopologyEdgeKey> plannedKeys,
            ChamferHalfEdgeBoundaryComponent component,
            List<ChamferProvisionalHalfEdge> halfEdges)
        {
            if (plannedKeys.Count !=
                component.OrderedHalfEdgeIndices.Count)
            {
                return false;
            }
            List<TopologyEdgeKey> authoritative =
                new List<TopologyEdgeKey>(plannedKeys.Count);
            for (int i = 0;
                 i < component.OrderedHalfEdgeIndices.Count;
                 i++)
            {
                authoritative.Add(halfEdges[
                    component.OrderedHalfEdgeIndices[i]].UndirectedKey);
            }
            for (int offset = 0; offset < plannedKeys.Count; offset++)
            {
                bool match = true;
                for (int i = 0; i < plannedKeys.Count; i++)
                {
                    int authoritativeIndex =
                        (offset - i + authoritative.Count) %
                        authoritative.Count;
                    if (!plannedKeys[i].Equals(
                            authoritative[authoritativeIndex]))
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    return true;
                }
            }
            return false;
        }




        #endregion
    }
}
