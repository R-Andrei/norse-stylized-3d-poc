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
            stats.CoincidentVertexReconciliationCount =
                graph.CoincidentVertexReconciliationCount;
            stats.CoincidentBoundarySeamPairCount =
                graph.CoincidentBoundarySeamPairCount;

            return stats.GraphFaceCount > 0 &&
                stats.GraphNonManifoldEdgeCount == 0 &&
                stats.InvalidFaceCount == 0 &&
                stats.InvalidEdgeCount == 0;
        }

        private static EdgeWearMicroTopologyNormalizationResult
            NormalizeEdgeWearMicroTopology(
                List<PolygonFace> sourceFaces,
                float maximumDimension,
                float minimumStyleWidth)
        {
            float minimumStableEdgeLength = maximumDimension * 0.0012f;
            float footprintGuard = Mathf.Max(
                PointMergeDistance * 4f,
                minimumStableEdgeLength * 0.01f);
            EdgeWearMicroTopologyNormalizationResult result =
                new EdgeWearMicroTopologyNormalizationResult
                {
                    Faces = sourceFaces,
                    Attempted = true,
                    Threshold = Mathf.Max(
                        PointMergeDistance * 4f,
                        minimumStyleWidth),
                    ComponentThreshold = Mathf.Max(
                        PointMergeDistance * 8f,
                        minimumStyleWidth *
                            EdgeWearMinimumFootprintLengthMultiplier +
                        footprintGuard)
                };
            System.Diagnostics.Stopwatch stopwatch =
                System.Diagnostics.Stopwatch.StartNew();

            if (sourceFaces == null || sourceFaces.Count < 4 ||
                !TryBuildEdgeWearTopologyGraph(
                    sourceFaces,
                    out EdgeWearTopologyGraph originalGraph,
                    out EdgeWearGraphBuildStats originalStats) ||
                originalStats.GraphBoundaryEdgeCount != 0)
            {
                result.Diagnostic =
                    "micro-topology source graph is unavailable or open";
                stopwatch.Stop();
                result.ElapsedMilliseconds =
                    stopwatch.Elapsed.TotalMilliseconds;
                return result;
            }

            result.OriginalGraph = originalGraph;
            result.OriginalVertexCount = originalGraph.Vertices.Count;
            result.OriginalEdgeCount = originalGraph.Edges.Count;
            result.OriginalFaceCount = originalGraph.Faces.Count;
            result.OriginalVolume =
                CalculatePlaneCutPolyhedronVolume(sourceFaces);
            if (double.IsNaN(result.OriginalVolume) ||
                double.IsInfinity(result.OriginalVolume) ||
                result.OriginalVolume <= 0.0)
            {
                PopulateEdgeWearMicroTopologyIdentityMap(
                    originalGraph,
                    result);
                result.NormalizedVertexCount = result.OriginalVertexCount;
                result.NormalizedEdgeCount = result.OriginalEdgeCount;
                result.NormalizedFaceCount = result.OriginalFaceCount;
                result.NormalizedVolume = result.OriginalVolume;
                result.Diagnostic =
                    "micro-topology source volume is invalid";
                stopwatch.Stop();
                result.ElapsedMilliseconds =
                    stopwatch.Elapsed.TotalMilliseconds;
                return result;
            }

            float tolerance = Mathf.Max(
                PointMergeDistance * 8f,
                result.Threshold * 0.001f);
            List<List<int>> components =
                BuildEdgeWearMicroTopologyComponents(
                    originalGraph,
                    result.Threshold + tolerance,
                    result.ComponentThreshold + tolerance);
            result.EligibleComponentCount = components.Count;
            if (components.Count == 0)
            {
                PopulateEdgeWearMicroTopologyIdentityMap(
                    originalGraph,
                    result);
                result.NormalizedVertexCount = result.OriginalVertexCount;
                result.NormalizedEdgeCount = result.OriginalEdgeCount;
                result.NormalizedFaceCount = result.OriginalFaceCount;
                result.NormalizedVolume = result.OriginalVolume;
                result.Diagnostic = "no eligible micro-topology components";
                stopwatch.Stop();
                result.ElapsedMilliseconds =
                    stopwatch.Elapsed.TotalMilliseconds;
                return result;
            }

            int[] vertexRemap = new int[originalGraph.Vertices.Count];
            for (int vertexIndex = 0;
                 vertexIndex < vertexRemap.Length;
                 vertexIndex++)
            {
                vertexRemap[vertexIndex] = vertexIndex;
            }
            HashSet<int> suppressedEdges = new HashSet<int>();
            List<PolygonFace> acceptedFaces = sourceFaces;
            EdgeWearTopologyGraph acceptedGraph = originalGraph;
            Dictionary<EdgeKey, int> acceptedMap =
                new Dictionary<EdgeKey, int>();
            HashSet<EdgeKey> acceptedGenerated = new HashSet<EdgeKey>();
            double acceptedVolume = result.OriginalVolume;

            for (int componentIndex = 0;
                 componentIndex < components.Count;
                 componentIndex++)
            {
                List<int> componentEdges = components[componentIndex];
                List<int> componentVertices =
                    CollectEdgeWearMicroTopologyVertices(
                        originalGraph,
                        componentEdges);
                float componentDiameter =
                    CalculateEdgeWearMicroTopologyDiameter(
                        originalGraph,
                        componentVertices);
                EdgeWearMicroTopologyComponentRecord componentRecord =
                    new EdgeWearMicroTopologyComponentRecord
                    {
                        Diameter = componentDiameter
                    };
                componentRecord.EdgeIndices.AddRange(componentEdges);
                componentRecord.VertexIndices.AddRange(componentVertices);
                for (int edgeIndex = 0;
                     edgeIndex < componentEdges.Count;
                     edgeIndex++)
                {
                    EdgeWearGraphEdge componentEdge =
                        originalGraph.Edges[componentEdges[edgeIndex]];
                    float componentEdgeLength =
                        (originalGraph.Vertices[componentEdge.VertexA].Position -
                         originalGraph.Vertices[componentEdge.VertexB].Position).
                            magnitude;
                    if (componentEdgeLength <= result.Threshold + tolerance)
                    {
                        componentRecord.SeedEdgeIndices.Add(
                            componentEdges[edgeIndex]);
                    }
                }
                result.Components.Add(componentRecord);
                if (componentVertices.Count < 2)
                {
                    componentRecord.Blocker =
                        "micro-topology component contains fewer than two vertices";
                    continue;
                }
                if (componentVertices.Count > 6)
                {
                    componentRecord.Blocker =
                        "micro-topology component exceeds the six-vertex bound";
                    continue;
                }
                if (componentEdges.Count > 8)
                {
                    componentRecord.Blocker =
                        "micro-topology component exceeds the eight-edge bound";
                    continue;
                }
                if (componentDiameter >
                    result.ComponentThreshold + tolerance)
                {
                    componentRecord.Blocker =
                        "micro-topology component exceeds its diameter bound";
                    continue;
                }
                componentRecord.CandidateEligible = true;

                List<PolygonFace> bestFaces = null;
                EdgeWearTopologyGraph bestGraph = null;
                Dictionary<EdgeKey, int> bestMap = null;
                HashSet<EdgeKey> bestGenerated = null;
                int bestCanonical = -1;
                double bestDisplacement = double.MaxValue;
                double bestVolumeLoss = double.MaxValue;
                double bestVolume = 0.0;

                componentVertices.Sort();
                for (int candidateIndex = 0;
                     candidateIndex < componentVertices.Count;
                     candidateIndex++)
                {
                    int canonicalVertex = componentVertices[candidateIndex];
                    int[] candidateRemap = (int[])vertexRemap.Clone();
                    for (int vertexIndex = 0;
                         vertexIndex < componentVertices.Count;
                         vertexIndex++)
                    {
                        candidateRemap[componentVertices[vertexIndex]] =
                            canonicalVertex;
                    }

                    HashSet<int> candidateSuppressed =
                        new HashSet<int>(suppressedEdges);
                    for (int edgeIndex = 0;
                         edgeIndex < componentEdges.Count;
                         edgeIndex++)
                    {
                        candidateSuppressed.Add(componentEdges[edgeIndex]);
                    }

                    double displacement = 0.0;
                    Vector3 canonicalPosition =
                        originalGraph.Vertices[canonicalVertex].Position;
                    for (int vertexIndex = 0;
                         vertexIndex < componentVertices.Count;
                         vertexIndex++)
                    {
                        Vector3 delta =
                            originalGraph.Vertices[
                                componentVertices[vertexIndex]].Position -
                            canonicalPosition;
                        displacement += delta.sqrMagnitude;
                    }
                    EdgeWearMicroTopologyCollapseAttemptRecord attempt =
                        new EdgeWearMicroTopologyCollapseAttemptRecord
                        {
                            CanonicalGraphVertexIndex = canonicalVertex,
                            CanonicalPosition = canonicalPosition,
                            SquaredDisplacement = displacement
                        };
                    componentRecord.Attempts.Add(attempt);

                    result.CandidateCollapseCount++;
                    if (!TryBuildEdgeWearMicroNormalizedFaces(
                            sourceFaces,
                            originalGraph,
                            candidateRemap,
                            candidateSuppressed,
                            maximumDimension,
                            result.OriginalVolume,
                            out List<PolygonFace> candidateFaces,
                            out EdgeWearTopologyGraph candidateGraph,
                            out Dictionary<EdgeKey, int> candidateMap,
                            out HashSet<EdgeKey> candidateGenerated,
                            out double candidateVolume,
                            out string candidateBlocker))
                    {
                        attempt.Blocker = candidateBlocker;
                        continue;
                    }

                    double volumeLoss = Math.Max(
                        0.0,
                        result.OriginalVolume - candidateVolume);
                    attempt.Succeeded = true;
                    attempt.NormalizedVolume = candidateVolume;
                    attempt.VolumeLoss = volumeLoss;
                    bool better = displacement <
                            bestDisplacement - 0.000000000001 ||
                        (Math.Abs(displacement - bestDisplacement) <=
                                0.000000000001 &&
                         (volumeLoss < bestVolumeLoss - 0.000000000001 ||
                          (Math.Abs(volumeLoss - bestVolumeLoss) <=
                                0.000000000001 &&
                           canonicalVertex < bestCanonical)));
                    if (!better)
                    {
                        continue;
                    }

                    bestFaces = candidateFaces;
                    bestGraph = candidateGraph;
                    bestMap = candidateMap;
                    bestGenerated = candidateGenerated;
                    bestCanonical = canonicalVertex;
                    bestDisplacement = displacement;
                    bestVolumeLoss = volumeLoss;
                    bestVolume = candidateVolume;
                }

                if (bestFaces == null)
                {
                    componentRecord.Blocker =
                        "micro-topology component has no certified collapse candidate";
                    continue;
                }

                componentRecord.Applied = true;
                componentRecord.SelectedCanonicalGraphVertexIndex =
                    bestCanonical;
                componentRecord.SelectedSquaredDisplacement =
                    bestDisplacement;
                componentRecord.SelectedVolumeLoss = bestVolumeLoss;

                for (int vertexIndex = 0;
                     vertexIndex < componentVertices.Count;
                     vertexIndex++)
                {
                    vertexRemap[componentVertices[vertexIndex]] =
                        bestCanonical;
                }
                for (int edgeIndex = 0;
                     edgeIndex < componentEdges.Count;
                     edgeIndex++)
                {
                    suppressedEdges.Add(componentEdges[edgeIndex]);
                }
                acceptedFaces = bestFaces;
                acceptedGraph = bestGraph;
                acceptedMap = bestMap;
                acceptedGenerated = bestGenerated;
                acceptedVolume = bestVolume;
                result.AppliedComponentCount++;
            }

            if (result.AppliedComponentCount > 0)
            {
                result.Applied = true;
                result.Faces = acceptedFaces;
                foreach (KeyValuePair<EdgeKey, int> pair in acceptedMap)
                {
                    result.OriginalSourceEdgeIndexByNormalizedKey[
                        pair.Key] = pair.Value;
                }
                foreach (EdgeKey key in acceptedGenerated)
                {
                    result.GeneratedTransitionKeys.Add(key);
                }
                PopulateEdgeWearMicroTopologyGraphIndexMap(
                    acceptedGraph,
                    result);
                List<int> orderedSuppressed =
                    new List<int>(suppressedEdges);
                orderedSuppressed.Sort();
                for (int i = 0; i < orderedSuppressed.Count; i++)
                {
                    int edgeIndex = orderedSuppressed[i];
                    EdgeWearGraphEdge edge = originalGraph.Edges[edgeIndex];
                    Vector3 start =
                        originalGraph.Vertices[edge.VertexA].Position;
                    Vector3 end =
                        originalGraph.Vertices[edge.VertexB].Position;
                    result.SuppressedEdges.Add(
                        new EdgeWearMicroTopologySuppressedEdge
                        {
                            OriginalSourceEdgeIndex = edgeIndex,
                            Start = start,
                            End = end,
                            Length = (end - start).magnitude
                        });
                }
                result.NormalizedVertexCount = acceptedGraph.Vertices.Count;
                result.NormalizedEdgeCount = acceptedGraph.Edges.Count;
                result.NormalizedFaceCount = acceptedGraph.Faces.Count;
                result.NormalizedVolume = acceptedVolume;
                result.VolumeLoss = Math.Max(
                    0.0,
                    result.OriginalVolume - acceptedVolume);
                result.VolumeLossFraction = result.OriginalVolume > 0.0
                    ? result.VolumeLoss / result.OriginalVolume
                    : 0.0;
                result.Diagnostic = "micro-topology normalization applied";
            }
            else
            {
                PopulateEdgeWearMicroTopologyIdentityMap(
                    originalGraph,
                    result);
                result.NormalizedVertexCount = result.OriginalVertexCount;
                result.NormalizedEdgeCount = result.OriginalEdgeCount;
                result.NormalizedFaceCount = result.OriginalFaceCount;
                result.NormalizedVolume = result.OriginalVolume;
                result.Diagnostic =
                    "eligible micro-topology components had no certified collapse";
            }

            stopwatch.Stop();
            result.ElapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            return result;
        }

        private static List<List<int>>
            BuildEdgeWearMicroTopologyComponents(
                EdgeWearTopologyGraph graph,
                float seedThreshold,
                float componentThreshold)
        {
            HashSet<int> seeds = new HashSet<int>();
            HashSet<int> eligible = new HashSet<int>();
            for (int edgeIndex = 0;
                 edgeIndex < graph.Edges.Count;
                 edgeIndex++)
            {
                EdgeWearGraphEdge edge = graph.Edges[edgeIndex];
                if (edge.FaceA < 0 || edge.FaceB < 0 ||
                    edge.ExtraFaceCount > 0)
                {
                    continue;
                }
                Vector3 start = graph.Vertices[edge.VertexA].Position;
                Vector3 end = graph.Vertices[edge.VertexB].Position;
                float length = (end - start).magnitude;
                if (length <= componentThreshold)
                {
                    eligible.Add(edgeIndex);
                }
                if (length <= seedThreshold)
                {
                    seeds.Add(edgeIndex);
                }
            }

            List<List<int>> components = new List<List<int>>();
            HashSet<int> visited = new HashSet<int>();
            List<int> orderedSeeds = new List<int>(seeds);
            orderedSeeds.Sort();
            for (int seedIndex = 0;
                 seedIndex < orderedSeeds.Count;
                 seedIndex++)
            {
                int seed = orderedSeeds[seedIndex];
                if (visited.Contains(seed))
                {
                    continue;
                }
                Queue<int> queue = new Queue<int>();
                List<int> component = new List<int>();
                queue.Enqueue(seed);
                visited.Add(seed);
                while (queue.Count > 0)
                {
                    int edgeIndex = queue.Dequeue();
                    component.Add(edgeIndex);
                    EdgeWearGraphEdge edge = graph.Edges[edgeIndex];
                    for (int candidateIndex = 0;
                         candidateIndex < graph.Edges.Count;
                         candidateIndex++)
                    {
                        if (!eligible.Contains(candidateIndex) ||
                            visited.Contains(candidateIndex))
                        {
                            continue;
                        }
                        EdgeWearGraphEdge candidate =
                            graph.Edges[candidateIndex];
                        if (candidate.VertexA == edge.VertexA ||
                            candidate.VertexA == edge.VertexB ||
                            candidate.VertexB == edge.VertexA ||
                            candidate.VertexB == edge.VertexB)
                        {
                            visited.Add(candidateIndex);
                            queue.Enqueue(candidateIndex);
                        }
                    }
                }
                component.Sort();
                components.Add(component);
            }
            return components;
        }

        private static List<int> CollectEdgeWearMicroTopologyVertices(
            EdgeWearTopologyGraph graph,
            List<int> edgeIndices)
        {
            HashSet<int> unique = new HashSet<int>();
            for (int i = 0; i < edgeIndices.Count; i++)
            {
                EdgeWearGraphEdge edge = graph.Edges[edgeIndices[i]];
                unique.Add(edge.VertexA);
                unique.Add(edge.VertexB);
            }
            List<int> vertices = new List<int>(unique);
            vertices.Sort();
            return vertices;
        }

        private static float CalculateEdgeWearMicroTopologyDiameter(
            EdgeWearTopologyGraph graph,
            List<int> vertexIndices)
        {
            float maximum = 0f;
            for (int a = 0; a < vertexIndices.Count - 1; a++)
            {
                for (int b = a + 1; b < vertexIndices.Count; b++)
                {
                    maximum = Mathf.Max(
                        maximum,
                        (graph.Vertices[vertexIndices[a]].Position -
                         graph.Vertices[vertexIndices[b]].Position).magnitude);
                }
            }
            return maximum;
        }

        private static bool TryBuildEdgeWearMicroNormalizedFaces(
            List<PolygonFace> sourceFaces,
            EdgeWearTopologyGraph originalGraph,
            int[] vertexRemap,
            HashSet<int> suppressedEdges,
            float maximumDimension,
            double originalVolume,
            out List<PolygonFace> normalizedFaces,
            out EdgeWearTopologyGraph normalizedGraph,
            out Dictionary<EdgeKey, int> originalIndexByNormalizedKey,
            out HashSet<EdgeKey> generatedTransitionKeys,
            out double normalizedVolume,
            out string blocker)
        {
            normalizedFaces = null;
            normalizedGraph = null;
            originalIndexByNormalizedKey = null;
            generatedTransitionKeys = null;
            normalizedVolume = 0.0;
            blocker = string.Empty;

            List<Vector3> points = new List<Vector3>();
            Dictionary<VertexKey, int> unique =
                new Dictionary<VertexKey, int>();
            for (int vertexIndex = 0;
                 vertexIndex < originalGraph.Vertices.Count;
                 vertexIndex++)
            {
                int mapped = vertexRemap[vertexIndex];
                Vector3 point = originalGraph.Vertices[mapped].Position;
                VertexKey key = new VertexKey(point);
                if (!unique.ContainsKey(key))
                {
                    unique.Add(key, points.Count);
                    points.Add(point);
                }
            }
            if (points.Count < 4)
            {
                blocker = "micro-topology collapse retained fewer than four points";
                return false;
            }

            float tolerance = Mathf.Max(
                PointMergeDistance * 8f,
                maximumDimension * 0.00001f);
            float minimumStableFaceArea =
                maximumDimension * maximumDimension * 0.000001f;
            BoundedAllEdgesAuditResult hullAudit =
                new BoundedAllEdgesAuditResult();
            if (!TryBuildBoundedConvexHullPlanes(
                    points,
                    CalculatePlaneCutFaceVertexCentre(sourceFaces),
                    tolerance,
                    hullAudit,
                    out List<BoundedHullPlane> planes,
                    out blocker))
            {
                return false;
            }

            normalizedFaces = new List<PolygonFace>(planes.Count);
            for (int planeIndex = 0;
                 planeIndex < planes.Count;
                 planeIndex++)
            {
                BoundedHullPlane plane = planes[planeIndex];
                if (!TryOrderBoundedHullFacet(
                        points,
                        plane,
                        out List<Vector3> ordered))
                {
                    blocker = "micro-topology hull facet ordering failed";
                    return false;
                }
                List<Vector3> sanitized = SanitizePolygon(
                    ordered,
                    plane.Normal);
                if (sanitized.Count < 3 ||
                    CalculatePolygonArea(sanitized) <= minimumStableFaceArea)
                {
                    blocker = "micro-topology hull facet collapsed";
                    return false;
                }
                Vector3 measured = CalculatePolygonNormal(sanitized);
                if (!IsFinite(measured) ||
                    measured.sqrMagnitude <= MinimumEdgeLengthSqr)
                {
                    blocker = "micro-topology hull facet normal is invalid";
                    return false;
                }
                if (Vector3.Dot(measured, plane.Normal) < 0f)
                {
                    sanitized.Reverse();
                    measured = -measured;
                }
                if (!IsBoundedPolygonConvex(
                        BuildBoundedConvexityCheckLoop(sanitized),
                        plane.Normal))
                {
                    blocker = "micro-topology hull facet is non-convex";
                    return false;
                }
                normalizedFaces.Add(new PolygonFace(
                    sanitized,
                    plane.Normal,
                    PolygonFaceFeature.Base,
                    0f,
                    PolygonFaceProvenanceKind.SourceFace,
                    planeIndex));
            }

            if (!TryBuildEdgeWearTopologyGraph(
                    normalizedFaces,
                    out normalizedGraph,
                    out EdgeWearGraphBuildStats stats) ||
                stats.GraphBoundaryEdgeCount != 0 ||
                stats.GraphNonManifoldEdgeCount != 0)
            {
                blocker = "micro-topology normalized graph is not closed manifold";
                return false;
            }

            normalizedVolume =
                CalculatePlaneCutPolyhedronVolume(normalizedFaces);
            if (double.IsNaN(normalizedVolume) ||
                double.IsInfinity(normalizedVolume))
            {
                blocker = "micro-topology normalized volume is non-finite";
                return false;
            }
            double volumeTolerance = Math.Max(
                0.000000001,
                originalVolume * 0.000001);
            double maximumVolumeLoss = Math.Max(
                volumeTolerance,
                originalVolume * 0.0025);
            if (normalizedVolume <= volumeTolerance ||
                normalizedVolume > originalVolume + volumeTolerance ||
                originalVolume - normalizedVolume > maximumVolumeLoss)
            {
                blocker = "micro-topology volume change exceeds its bounded allowance";
                return false;
            }

            Bounds originalBounds = CalculateFaceBounds(sourceFaces);
            Bounds normalizedBounds = CalculateFaceBounds(normalizedFaces);
            float boundsTolerance = Mathf.Max(
                tolerance,
                maximumDimension * 0.00001f);
            if (normalizedBounds.min.x < originalBounds.min.x - boundsTolerance ||
                normalizedBounds.min.y < originalBounds.min.y - boundsTolerance ||
                normalizedBounds.min.z < originalBounds.min.z - boundsTolerance ||
                normalizedBounds.max.x > originalBounds.max.x + boundsTolerance ||
                normalizedBounds.max.y > originalBounds.max.y + boundsTolerance ||
                normalizedBounds.max.z > originalBounds.max.z + boundsTolerance)
            {
                blocker = "micro-topology normalized bounds expand the source";
                return false;
            }

            originalIndexByNormalizedKey =
                new Dictionary<EdgeKey, int>();
            for (int edgeIndex = 0;
                 edgeIndex < originalGraph.Edges.Count;
                 edgeIndex++)
            {
                if (suppressedEdges.Contains(edgeIndex))
                {
                    continue;
                }
                EdgeWearGraphEdge edge = originalGraph.Edges[edgeIndex];
                Vector3 start = originalGraph.Vertices[
                    vertexRemap[edge.VertexA]].Position;
                Vector3 end = originalGraph.Vertices[
                    vertexRemap[edge.VertexB]].Position;
                if (AreSamePoint(start, end))
                {
                    blocker = "micro-topology collapse consumed a non-micro edge";
                    return false;
                }
                EdgeKey key = new EdgeKey(start, end);
                if (originalIndexByNormalizedKey.ContainsKey(key))
                {
                    blocker = "micro-topology collapse aliases non-micro edge identity";
                    return false;
                }
                originalIndexByNormalizedKey.Add(key, edgeIndex);
            }

            foreach (KeyValuePair<EdgeKey, int> pair in
                originalIndexByNormalizedKey)
            {
                if (!normalizedGraph.EdgeByKey.ContainsKey(pair.Key))
                {
                    blocker = "micro-topology collapse removed a non-micro source edge";
                    return false;
                }
            }

            generatedTransitionKeys = new HashSet<EdgeKey>();
            for (int edgeIndex = 0;
                 edgeIndex < normalizedGraph.Edges.Count;
                 edgeIndex++)
            {
                EdgeWearGraphEdge edge = normalizedGraph.Edges[edgeIndex];
                EdgeKey key = new EdgeKey(
                    normalizedGraph.Vertices[edge.VertexA].Position,
                    normalizedGraph.Vertices[edge.VertexB].Position);
                if (!originalIndexByNormalizedKey.ContainsKey(key))
                {
                    generatedTransitionKeys.Add(key);
                }
            }
            return true;
        }

        private static void PopulateEdgeWearMicroTopologyGraphIndexMap(
            EdgeWearTopologyGraph graph,
            EdgeWearMicroTopologyNormalizationResult result)
        {
            result.OriginalSourceEdgeIndexByNormalizedGraphEdge.Clear();
            for (int edgeIndex = 0;
                 edgeIndex < graph.Edges.Count;
                 edgeIndex++)
            {
                EdgeWearGraphEdge edge = graph.Edges[edgeIndex];
                EdgeKey key = new EdgeKey(
                    graph.Vertices[edge.VertexA].Position,
                    graph.Vertices[edge.VertexB].Position);
                if (result.OriginalSourceEdgeIndexByNormalizedKey.TryGetValue(
                        key,
                        out int originalIndex))
                {
                    result.OriginalSourceEdgeIndexByNormalizedGraphEdge[
                        edgeIndex] = originalIndex;
                }
            }
        }

        private static void PopulateEdgeWearMicroTopologyIdentityMap(
            EdgeWearTopologyGraph graph,
            EdgeWearMicroTopologyNormalizationResult result)
        {
            for (int edgeIndex = 0;
                 edgeIndex < graph.Edges.Count;
                 edgeIndex++)
            {
                EdgeWearGraphEdge edge = graph.Edges[edgeIndex];
                EdgeKey key = new EdgeKey(
                    graph.Vertices[edge.VertexA].Position,
                    graph.Vertices[edge.VertexB].Position);
                result.OriginalSourceEdgeIndexByNormalizedKey[key] =
                    edgeIndex;
                result.OriginalSourceEdgeIndexByNormalizedGraphEdge[
                    edgeIndex] = edgeIndex;
            }
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

            if (TryFindCoincidentEdgeWearGraphVertex(
                    graph,
                    position,
                    out index))
            {
                graph.VertexByKey.Add(key, index);
                graph.CoincidentVertexReconciliationCount++;
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

            if (TryFindReversedCoincidentBoundaryGraphEdge(
                    graph,
                    vertexA,
                    vertexB,
                    out index))
            {
                graph.EdgeByKey.Add(key, index);
                graph.CoincidentBoundarySeamPairCount++;
                return index;
            }

            index = graph.Edges.Count;
            graph.EdgeByKey.Add(key, index);
            graph.Edges.Add(new EdgeWearGraphEdge(vertexA, vertexB));
            graph.Vertices[vertexA].AddEdge(index);
            graph.Vertices[vertexB].AddEdge(index);
            return index;
        }

        private static bool TryFindCoincidentEdgeWearGraphVertex(
            EdgeWearTopologyGraph graph,
            Vector3 position,
            out int vertexIndex)
        {
            vertexIndex = -1;
            float bestDistanceSqr = float.PositiveInfinity;
            for (int index = 0; index < graph.Vertices.Count; index++)
            {
                float distanceSqr =
                    (graph.Vertices[index].Position - position).sqrMagnitude;
                if (distanceSqr > PointMergeDistanceSqr ||
                    distanceSqr >= bestDistanceSqr)
                {
                    continue;
                }

                vertexIndex = index;
                bestDistanceSqr = distanceSqr;
            }
            return vertexIndex >= 0;
        }

        private static bool TryFindReversedCoincidentBoundaryGraphEdge(
            EdgeWearTopologyGraph graph,
            int vertexA,
            int vertexB,
            out int edgeIndex)
        {
            edgeIndex = -1;
            List<int> connected = graph.Vertices[vertexA].EdgeIndices;
            for (int index = 0; index < connected.Count; index++)
            {
                int candidateIndex = connected[index];
                EdgeWearGraphEdge candidate = graph.Edges[candidateIndex];
                if (candidate.VertexA != vertexB ||
                    candidate.VertexB != vertexA ||
                    candidate.FaceA < 0 ||
                    candidate.FaceB >= 0 ||
                    candidate.ExtraFaceCount > 0)
                {
                    continue;
                }

                edgeIndex = candidateIndex;
                return true;
            }
            return false;
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
