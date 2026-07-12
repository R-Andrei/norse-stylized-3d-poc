using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear graph utilities

        private static bool TryBuildEdgeWearTopologyGraph(
            List<PolygonFace> faces,
            out EdgeWearTopologyGraph graph,
            out EdgeWearGraphBuildStats stats)
        {
            graph = new EdgeWearTopologyGraph();
            stats = new EdgeWearGraphBuildStats();

            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face.Feature != PolygonFaceFeature.Base)
                {
                    continue;
                }

                List<int> vertexIndices = new List<int>(face.Vertices.Count);
                for (int vertexIndex = 0; vertexIndex < face.Vertices.Count; vertexIndex++)
                {
                    int graphVertexIndex = GetOrAddEdgeWearGraphVertex(
                        graph,
                        face.Vertices[vertexIndex]);
                    vertexIndices.Add(graphVertexIndex);
                }

                if (GetUniqueGraphVertexCount(vertexIndices) < 3)
                {
                    stats.InvalidFaceCount++;
                    continue;
                }

                List<int> edgeIndices = new List<int>(face.Vertices.Count);
                for (int vertexIndex = 0; vertexIndex < face.Vertices.Count; vertexIndex++)
                {
                    int startVertex = vertexIndices[vertexIndex];
                    int endVertex = vertexIndices[(vertexIndex + 1) % vertexIndices.Count];
                    if (startVertex == endVertex)
                    {
                        stats.InvalidEdgeCount++;
                        continue;
                    }

                    int edgeIndex = GetOrAddEdgeWearGraphEdge(
                        graph,
                        startVertex,
                        endVertex,
                        face.Vertices[vertexIndex],
                        face.Vertices[(vertexIndex + 1) % face.Vertices.Count]);
                    EdgeWearGraphEdge edge = graph.Edges[edgeIndex];
                    edge.TryAddFace(faceIndex);

                    edgeIndices.Add(edgeIndex);
                    graph.Vertices[startVertex].AddFace(faceIndex);
                    graph.Vertices[endVertex].AddFace(faceIndex);
                }

                graph.Faces.Add(
                    new EdgeWearGraphFace(
                        faceIndex,
                        face,
                        vertexIndices,
                        edgeIndices));
            }

            for (int edgeIndex = 0; edgeIndex < graph.Edges.Count; edgeIndex++)
            {
                EdgeWearGraphEdge edge = graph.Edges[edgeIndex];
                if (edge.FaceA < 0 || edge.FaceB < 0)
                {
                    stats.GraphBoundaryEdgeCount++;
                }

                if (edge.ExtraFaceCount > 0)
                {
                    stats.GraphNonManifoldEdgeCount = CountGraphNonManifoldEdges(graph);
                }
            }

            stats.GraphVertexCount = graph.Vertices.Count;
            stats.GraphEdgeCount = graph.Edges.Count;
            stats.GraphFaceCount = graph.Faces.Count;

            return stats.GraphFaceCount > 0 &&
                stats.GraphNonManifoldEdgeCount == 0 &&
                stats.InvalidFaceCount == 0 &&
                stats.InvalidEdgeCount == 0;
        }

        private static bool TryMapSelectedCandidatesToGraph(
            EdgeWearTopologyGraph graph,
            List<EdgeWearBevelCandidate> candidates,
            int selectedCount,
            out List<EdgeWearSelectedGraphEdge> selectedEdges,
            ref EdgeWearGraphBuildStats stats)
        {
            selectedEdges = new List<EdgeWearSelectedGraphEdge>(selectedCount);
            int limit = Mathf.Clamp(selectedCount, 0, candidates.Count);
            for (int i = 0; i < limit; i++)
            {
                EdgeWearBevelCandidate candidate = candidates[i];
                EdgeKey key = new EdgeKey(candidate.Start, candidate.End);
                if (!graph.EdgeByKey.TryGetValue(key, out int graphEdgeIndex))
                {
                    stats.MissingSelectedGraphEdgeCount++;
                    continue;
                }

                EdgeWearGraphEdge edge = graph.Edges[graphEdgeIndex];
                if (!GraphEdgeMatchesCandidateFaces(edge, candidate))
                {
                    stats.MismatchedSelectedGraphFaceCount++;
                    continue;
                }

                if (edge.Selected)
                {
                    stats.DuplicateSelectedGraphEdgeCount++;
                    continue;
                }

                edge.Selected = true;
                edge.CandidateIndex = candidate.CandidateIndex;
                selectedEdges.Add(
                    new EdgeWearSelectedGraphEdge(
                        graphEdgeIndex,
                        candidate.CandidateIndex,
                        candidate));
                stats.SelectedGraphEdgeCount++;
            }

            return stats.SelectedGraphEdgeCount == limit &&
                stats.MissingSelectedGraphEdgeCount == 0 &&
                stats.MismatchedSelectedGraphFaceCount == 0 &&
                stats.DuplicateSelectedGraphEdgeCount == 0;
        }
        private static int GetOrAddEdgeWearGraphVertex(
            EdgeWearTopologyGraph graph,
            Vector3 position)
        {
            VertexKey key = new VertexKey(position);
            if (graph.VertexByKey.TryGetValue(key, out int index))
            {
                return index;
            }

            index = graph.Vertices.Count;
            graph.VertexByKey.Add(key, index);
            graph.Vertices.Add(new EdgeWearGraphVertex(position, key));
            return index;
        }

        private static int GetOrAddEdgeWearGraphEdge(
            EdgeWearTopologyGraph graph,
            int vertexA,
            int vertexB,
            Vector3 start,
            Vector3 end)
        {
            EdgeKey key = new EdgeKey(start, end);
            if (graph.EdgeByKey.TryGetValue(key, out int index))
            {
                return index;
            }

            index = graph.Edges.Count;
            graph.EdgeByKey.Add(key, index);
            graph.Edges.Add(new EdgeWearGraphEdge(vertexA, vertexB));
            graph.Vertices[vertexA].AddEdge(index);
            graph.Vertices[vertexB].AddEdge(index);
            return index;
        }

        private static int GetUniqueGraphVertexCount(List<int> vertexIndices)
        {
            HashSet<int> unique = new HashSet<int>();
            for (int i = 0; i < vertexIndices.Count; i++)
            {
                unique.Add(vertexIndices[i]);
            }

            return unique.Count;
        }

        private static bool GraphEdgeMatchesCandidateFaces(
            EdgeWearGraphEdge edge,
            EdgeWearBevelCandidate candidate)
        {
            return
                (edge.FaceA == candidate.FaceA && edge.FaceB == candidate.FaceB) ||
                (edge.FaceA == candidate.FaceB && edge.FaceB == candidate.FaceA);
        }

        private static int CountGraphNonManifoldEdges(EdgeWearTopologyGraph graph)
        {
            int count = 0;
            for (int i = 0; i < graph.Edges.Count; i++)
            {
                if (graph.Edges[i].ExtraFaceCount > 0)
                {
                    count++;
                }
            }

            return count;
        }
        private static bool IsFinite(Vector3 value)
        {
            return !(float.IsNaN(value.x) ||
                float.IsNaN(value.y) ||
                float.IsNaN(value.z) ||
                float.IsInfinity(value.x) ||
                float.IsInfinity(value.y) ||
                float.IsInfinity(value.z));
        }
private static EdgeWearTopologyStats AuditEdgeWearTopology(
            List<PolygonFace> faces,
            float minimumStableEdgeLength)
        {
            if (faces == null || faces.Count == 0)
            {
                return EdgeWearTopologyStats.Empty;
            }

            Dictionary<TopologyEdgeKey, int> edgeUseCounts =
                new Dictionary<TopologyEdgeKey, int>();
            List<TopologyEdgeSegment> edgeSegments =
                new List<TopologyEdgeSegment>();
            Dictionary<VertexKey, Vector3> uniqueVertices =
                new Dictionary<VertexKey, Vector3>();

            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                List<Vector3> vertices = faces[faceIndex].Vertices;
                if (vertices == null || vertices.Count < 3)
                {
                    continue;
                }

                for (int vertexIndex = 0;
                     vertexIndex < vertices.Count;
                     vertexIndex++)
                {
                    Vector3 start = vertices[vertexIndex];
                    Vector3 end = vertices[(vertexIndex + 1) % vertices.Count];

                    if (AreSamePoint(start, end))
                    {
                        continue;
                    }

                    VertexKey startKey = new VertexKey(start);
                    VertexKey endKey = new VertexKey(end);
                    if (!uniqueVertices.ContainsKey(startKey))
                    {
                        uniqueVertices.Add(startKey, start);
                    }

                    if (!uniqueVertices.ContainsKey(endKey))
                    {
                        uniqueVertices.Add(endKey, end);
                    }

                    TopologyEdgeKey edgeKey = new TopologyEdgeKey(
                        startKey,
                        endKey);
                    edgeUseCounts.TryGetValue(edgeKey, out int useCount);
                    edgeUseCounts[edgeKey] = useCount + 1;
                    edgeSegments.Add(
                        new TopologyEdgeSegment(
                            start,
                            end,
                            startKey,
                            endKey));
                }
            }

            int openEdges = 0;
            int nonManifoldEdges = 0;
            foreach (int useCount in edgeUseCounts.Values)
            {
                if (useCount == 1)
                {
                    openEdges++;
                }
                else if (useCount > 2)
                {
                    nonManifoldEdges++;
                }
            }

            int tJunctions = CountTopologyTJunctions(
                uniqueVertices,
                edgeSegments,
                minimumStableEdgeLength);

            return new EdgeWearTopologyStats(
                openEdges,
                nonManifoldEdges,
                tJunctions);
        }

        private static int CountTopologyTJunctions(
            Dictionary<VertexKey, Vector3> uniqueVertices,
            List<TopologyEdgeSegment> edgeSegments,
            float minimumStableEdgeLength)
        {
            if (uniqueVertices.Count == 0 || edgeSegments.Count == 0)
            {
                return 0;
            }

            float tolerance = CalculateTopologyTJunctionTolerance(
                minimumStableEdgeLength);
            float toleranceSqr = tolerance * tolerance;
            int count = 0;

            foreach (KeyValuePair<VertexKey, Vector3> vertex in uniqueVertices)
            {
                for (int edgeIndex = 0;
                     edgeIndex < edgeSegments.Count;
                     edgeIndex++)
                {
                    TopologyEdgeSegment edge = edgeSegments[edgeIndex];
                    if (vertex.Key.Equals(edge.StartKey) ||
                        vertex.Key.Equals(edge.EndKey))
                    {
                        continue;
                    }

                    if (IsPointOnSegmentInterior(
                            vertex.Value,
                            edge.Start,
                            edge.End,
                            toleranceSqr))
                    {
                        count++;
                        break;
                    }
                }
            }

            return count;
        }

        private static bool IsPointOnSegmentInterior(
            Vector3 point,
            Vector3 start,
            Vector3 end,
            float toleranceSqr)
        {
            Vector3 segment = end - start;
            float segmentLengthSqr = segment.sqrMagnitude;
            if (segmentLengthSqr <= MinimumEdgeLengthSqr)
            {
                return false;
            }

            if ((point - start).sqrMagnitude <= toleranceSqr ||
                (point - end).sqrMagnitude <= toleranceSqr)
            {
                return false;
            }

            float t = Vector3.Dot(point - start, segment) / segmentLengthSqr;
            if (t <= 0f || t >= 1f)
            {
                return false;
            }

            Vector3 closest = start + segment * t;
            return (point - closest).sqrMagnitude <= toleranceSqr;
        }

        private static float CalculateTopologyTJunctionTolerance(
            float minimumStableEdgeLength)
        {
            return Mathf.Max(
                PointMergeDistance * 4f,
                minimumStableEdgeLength * 0.04f);
        }

        private static bool AreSamePoint(Vector3 left, Vector3 right)
        {
            return (left - right).sqrMagnitude <= PointMergeDistanceSqr;
        }

        #endregion
    }
}
