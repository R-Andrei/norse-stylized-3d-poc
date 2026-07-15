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

        private static List<PolygonFace>
            BuildEdgeWearMicroFeatureNormalizedFaces(
                List<PolygonFace> sourceFaces,
                float requestedWidth,
                float requiredFootprintLength,
                float minimumStableEdgeLength,
                float minimumStableFaceArea,
                out EdgeWearMicroFeatureNormalizationStats stats)
        {
            stats = new EdgeWearMicroFeatureNormalizationStats();
            stats.RawSourceEdgeCount = CountRawEdgeWearSourceEdges(
                sourceFaces);
            if (sourceFaces == null || sourceFaces.Count == 0 ||
                requestedWidth <= PointMergeDistance ||
                requiredFootprintLength <= PointMergeDistance)
            {
                return sourceFaces;
            }

            if (!TryBuildEdgeWearTopologyGraph(
                    sourceFaces,
                    out EdgeWearTopologyGraph rawGraph,
                    out EdgeWearGraphBuildStats rawGraphStats))
            {
                stats.FirstRejectionReason =
                    "source topology graph failed before micro-feature normalization";
                return sourceFaces;
            }

            float maximumMicroLength = Mathf.Min(
                requestedWidth,
                requiredFootprintLength * 0.5f);
            if (maximumMicroLength <= PointMergeDistance)
            {
                return sourceFaces;
            }

            Vector3 solidCentre =
                CalculatePlaneCutFaceVertexCentre(sourceFaces);
            float classificationTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                minimumStableEdgeLength * 0.02f);
            HashSet<int> qualifiedEdges = new HashSet<int>();
            for (int edgeIndex = 0;
                 edgeIndex < rawGraph.Edges.Count;
                 edgeIndex++)
            {
                EdgeWearGraphEdge edge = rawGraph.Edges[edgeIndex];
                float length = GetGraphEdgeLength(rawGraph, edgeIndex);
                if (length + PointMergeDistance >= requiredFootprintLength)
                {
                    continue;
                }

                stats.EvaluatedEdgeCount++;
                string rejection = string.Empty;
                if (length > maximumMicroLength + PointMergeDistance)
                {
                    rejection =
                        "sub-footprint edge exceeds the conservative normalization length";
                }
                else if (edge.FaceA < 0 || edge.FaceB < 0 ||
                    edge.ExtraFaceCount > 0)
                {
                    rejection =
                        "sub-footprint edge is not an internal manifold edge";
                }
                else if (!TryClassifyEdgeWearMicroFeatureEdge(
                        sourceFaces,
                        rawGraph,
                        edgeIndex,
                        solidCentre,
                        classificationTolerance,
                        out BoundedEdgeClassification classification) ||
                    classification != BoundedEdgeClassification.Convex)
                {
                    rejection =
                        "sub-footprint edge is not a proven convex micro-feature";
                }
                else if (!HasEdgeWearMicroFeatureMeaningfulContinuation(
                        rawGraph,
                        edgeIndex,
                        requiredFootprintLength))
                {
                    rejection =
                        "sub-footprint edge has no meaningful adjacent continuation";
                }

                if (string.IsNullOrEmpty(rejection))
                {
                    qualifiedEdges.Add(edgeIndex);
                    continue;
                }

                AddEdgeWearMicroFeatureNormalizationRejection(
                    stats,
                    edgeIndex,
                    edge,
                    rejection);
            }

            if (qualifiedEdges.Count == 0)
            {
                return sourceFaces;
            }

            List<List<int>> components =
                BuildEdgeWearMicroFeatureComponents(rawGraph, qualifiedEdges);
            Dictionary<VertexKey, Vector3> acceptedReplacements =
                new Dictionary<VertexKey, Vector3>();
            List<PolygonFace> currentFaces = sourceFaces;
            EdgeWearTopologyGraph currentGraph = rawGraph;
            EdgeWearGraphBuildStats currentGraphStats = rawGraphStats;

            for (int componentIndex = 0;
                 componentIndex < components.Count;
                 componentIndex++)
            {
                List<int> componentEdges = components[componentIndex];
                EdgeWearMicroFeatureNormalizationRecord record =
                    new EdgeWearMicroFeatureNormalizationRecord();
                record.SourceEdgeIndices.AddRange(componentEdges);
                HashSet<int> componentVertices =
                    CollectEdgeWearMicroFeatureComponentVertices(
                        rawGraph,
                        componentEdges);
                List<int> orderedVertices =
                    new List<int>(componentVertices);
                orderedVertices.Sort();
                record.SourceVertexIndices.AddRange(orderedVertices);

                string rejection = ValidateEdgeWearMicroFeatureChain(
                    rawGraph,
                    componentEdges,
                    componentVertices);
                Vector3 junction = default;
                float maximumDisplacement = 0f;
                float maximumPlaneResidual = 0f;
                if (string.IsNullOrEmpty(rejection) &&
                    !TrySolveEdgeWearMicroFeatureJunction(
                        sourceFaces,
                        rawGraph,
                        componentVertices,
                        requestedWidth,
                        minimumStableEdgeLength,
                        out junction,
                        out maximumDisplacement,
                        out maximumPlaneResidual,
                        out rejection))
                {
                    // The solver writes a specific rejection reason.
                }

                if (!string.IsNullOrEmpty(rejection))
                {
                    record.Reason = rejection;
                    stats.RejectedEdgeCount += componentEdges.Count;
                    RegisterEdgeWearMicroFeatureFirstRejection(
                        stats,
                        rejection);
                    stats.Records.Add(record);
                    continue;
                }

                Dictionary<VertexKey, Vector3> trialReplacements =
                    new Dictionary<VertexKey, Vector3>(
                        acceptedReplacements);
                foreach (KeyValuePair<VertexKey, int> pair in
                    rawGraph.VertexByKey)
                {
                    if (componentVertices.Contains(pair.Value))
                    {
                        trialReplacements[pair.Key] = junction;
                    }
                }

                List<PolygonFace> trialFaces =
                    BuildEdgeWearMicroFeatureFaceView(
                        sourceFaces,
                        trialReplacements);
                if (!ValidateEdgeWearMicroFeatureFaceView(
                        sourceFaces,
                        trialFaces,
                        minimumStableFaceArea,
                        out rejection) ||
                    !TryBuildEdgeWearTopologyGraph(
                        trialFaces,
                        out EdgeWearTopologyGraph trialGraph,
                        out EdgeWearGraphBuildStats trialGraphStats))
                {
                    if (string.IsNullOrEmpty(rejection))
                    {
                        rejection =
                            "normalized bevel graph failed topology validation";
                    }
                    record.Reason = rejection;
                    stats.RejectedEdgeCount += componentEdges.Count;
                    RegisterEdgeWearMicroFeatureFirstRejection(
                        stats,
                        rejection);
                    stats.Records.Add(record);
                    continue;
                }

                int expectedVertexCount = currentGraph.Vertices.Count -
                    Mathf.Max(0, componentVertices.Count - 1);
                int expectedEdgeCount = currentGraph.Edges.Count -
                    componentEdges.Count;
                bool topologyConserved =
                    trialGraphStats.GraphFaceCount ==
                        currentGraphStats.GraphFaceCount &&
                    trialGraphStats.GraphBoundaryEdgeCount ==
                        currentGraphStats.GraphBoundaryEdgeCount &&
                    trialGraphStats.GraphNonManifoldEdgeCount == 0 &&
                    trialGraphStats.InvalidFaceCount == 0 &&
                    trialGraphStats.InvalidEdgeCount == 0 &&
                    trialGraph.Vertices.Count == expectedVertexCount &&
                    trialGraph.Edges.Count == expectedEdgeCount;
                if (!topologyConserved)
                {
                    rejection =
                        "normalization changed more bevel-graph topology than the micro-feature chain owns";
                    record.Reason = rejection;
                    stats.RejectedEdgeCount += componentEdges.Count;
                    RegisterEdgeWearMicroFeatureFirstRejection(
                        stats,
                        rejection);
                    stats.Records.Add(record);
                    continue;
                }

                acceptedReplacements = trialReplacements;
                currentFaces = trialFaces;
                currentGraph = trialGraph;
                currentGraphStats = trialGraphStats;
                record.Normalized = true;
                record.Junction = junction;
                record.MaximumDisplacement = maximumDisplacement;
                record.MaximumPlaneResidual = maximumPlaneResidual;
                record.Reason = "normalized";
                stats.NormalizedComponentCount++;
                stats.NormalizedEdgeCount += componentEdges.Count;
                stats.MaximumDisplacement = Mathf.Max(
                    stats.MaximumDisplacement,
                    maximumDisplacement);
                stats.MaximumPlaneResidual = Mathf.Max(
                    stats.MaximumPlaneResidual,
                    maximumPlaneResidual);
                stats.Records.Add(record);
            }

            return currentFaces;
        }

        private static int CountRawEdgeWearSourceEdges(
            List<PolygonFace> sourceFaces)
        {
            if (sourceFaces == null)
            {
                return 0;
            }
            HashSet<EdgeKey> keys = new HashSet<EdgeKey>();
            for (int faceIndex = 0;
                 faceIndex < sourceFaces.Count;
                 faceIndex++)
            {
                PolygonFace face = sourceFaces[faceIndex];
                if (face == null ||
                    face.Feature != PolygonFaceFeature.Base ||
                    face.Vertices == null)
                {
                    continue;
                }
                for (int vertexIndex = 0;
                     vertexIndex < face.Vertices.Count;
                     vertexIndex++)
                {
                    Vector3 start = face.Vertices[vertexIndex];
                    Vector3 end = face.Vertices[
                        (vertexIndex + 1) % face.Vertices.Count];
                    if ((end - start).sqrMagnitude > MinimumEdgeLengthSqr)
                    {
                        keys.Add(new EdgeKey(start, end));
                    }
                }
            }
            return keys.Count;
        }

        private static bool TryClassifyEdgeWearMicroFeatureEdge(
            List<PolygonFace> sourceFaces,
            EdgeWearTopologyGraph graph,
            int edgeIndex,
            Vector3 solidCentre,
            float tolerance,
            out BoundedEdgeClassification classification)
        {
            classification = BoundedEdgeClassification.Ambiguous;
            if (sourceFaces == null || graph == null ||
                edgeIndex < 0 || edgeIndex >= graph.Edges.Count)
            {
                return false;
            }
            EdgeWearGraphEdge edge = graph.Edges[edgeIndex];
            if (edge.FaceA < 0 || edge.FaceB < 0 ||
                edge.FaceA >= graph.Faces.Count ||
                edge.FaceB >= graph.Faces.Count)
            {
                return false;
            }
            int sourceFaceA = graph.Faces[edge.FaceA].SourceFaceIndex;
            int sourceFaceB = graph.Faces[edge.FaceB].SourceFaceIndex;
            Vector3 start = graph.Vertices[edge.VertexA].Position;
            Vector3 end = graph.Vertices[edge.VertexB].Position;
            if (!TryClassifyEdgeWearStructuralEdge(
                    sourceFaces,
                    sourceFaceA,
                    sourceFaceB,
                    start,
                    end,
                    solidCentre,
                    tolerance,
                    out BoundedEdgeClassificationEvidence evidence))
            {
                return false;
            }
            classification = evidence.Classification;
            return true;
        }

        private static bool HasEdgeWearMicroFeatureMeaningfulContinuation(
            EdgeWearTopologyGraph graph,
            int microEdgeIndex,
            float requiredFootprintLength)
        {
            EdgeWearGraphEdge micro = graph.Edges[microEdgeIndex];
            int[] vertices = { micro.VertexA, micro.VertexB };
            for (int vertexOrdinal = 0;
                 vertexOrdinal < vertices.Length;
                 vertexOrdinal++)
            {
                List<int> connected =
                    graph.Vertices[vertices[vertexOrdinal]].EdgeIndices;
                for (int connectedIndex = 0;
                     connectedIndex < connected.Count;
                     connectedIndex++)
                {
                    int edgeIndex = connected[connectedIndex];
                    if (edgeIndex == microEdgeIndex)
                    {
                        continue;
                    }
                    EdgeWearGraphEdge edge = graph.Edges[edgeIndex];
                    if (edge.FaceA >= 0 && edge.FaceB >= 0 &&
                        edge.ExtraFaceCount == 0 &&
                        GetGraphEdgeLength(graph, edgeIndex) +
                            PointMergeDistance >= requiredFootprintLength)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static List<List<int>> BuildEdgeWearMicroFeatureComponents(
            EdgeWearTopologyGraph graph,
            HashSet<int> qualifiedEdges)
        {
            List<List<int>> components = new List<List<int>>();
            HashSet<int> remaining = new HashSet<int>(qualifiedEdges);
            while (remaining.Count > 0)
            {
                int first = int.MaxValue;
                foreach (int edgeIndex in remaining)
                {
                    first = Mathf.Min(first, edgeIndex);
                }
                Queue<int> queue = new Queue<int>();
                List<int> component = new List<int>();
                queue.Enqueue(first);
                remaining.Remove(first);
                while (queue.Count > 0)
                {
                    int edgeIndex = queue.Dequeue();
                    component.Add(edgeIndex);
                    EdgeWearGraphEdge edge = graph.Edges[edgeIndex];
                    int[] vertices = { edge.VertexA, edge.VertexB };
                    for (int vertexOrdinal = 0;
                         vertexOrdinal < vertices.Length;
                         vertexOrdinal++)
                    {
                        List<int> connected =
                            graph.Vertices[vertices[vertexOrdinal]].EdgeIndices;
                        for (int connectedIndex = 0;
                             connectedIndex < connected.Count;
                             connectedIndex++)
                        {
                            int neighbour = connected[connectedIndex];
                            if (remaining.Remove(neighbour) &&
                                qualifiedEdges.Contains(neighbour))
                            {
                                queue.Enqueue(neighbour);
                            }
                        }
                    }
                }
                component.Sort();
                components.Add(component);
            }
            components.Sort((left, right) => left[0].CompareTo(right[0]));
            return components;
        }

        private static HashSet<int>
            CollectEdgeWearMicroFeatureComponentVertices(
                EdgeWearTopologyGraph graph,
                List<int> componentEdges)
        {
            HashSet<int> vertices = new HashSet<int>();
            for (int edgeOrdinal = 0;
                 edgeOrdinal < componentEdges.Count;
                 edgeOrdinal++)
            {
                EdgeWearGraphEdge edge =
                    graph.Edges[componentEdges[edgeOrdinal]];
                vertices.Add(edge.VertexA);
                vertices.Add(edge.VertexB);
            }
            return vertices;
        }

        private static string ValidateEdgeWearMicroFeatureChain(
            EdgeWearTopologyGraph graph,
            List<int> componentEdges,
            HashSet<int> componentVertices)
        {
            if (componentEdges == null || componentEdges.Count == 0 ||
                componentVertices == null || componentVertices.Count < 2)
            {
                return "micro-feature component is empty";
            }
            if (componentEdges.Count != componentVertices.Count - 1)
            {
                return "micro-feature component is not an open chain";
            }
            int endpointCount = 0;
            foreach (int vertexIndex in componentVertices)
            {
                int degree = 0;
                List<int> connected = graph.Vertices[vertexIndex].EdgeIndices;
                for (int connectedIndex = 0;
                     connectedIndex < connected.Count;
                     connectedIndex++)
                {
                    if (componentEdges.Contains(connected[connectedIndex]))
                    {
                        degree++;
                    }
                }
                if (degree == 1)
                {
                    endpointCount++;
                }
                else if (degree != 2)
                {
                    return "micro-feature component branches at a source vertex";
                }
            }
            return endpointCount == 2
                ? string.Empty
                : "micro-feature component does not have two chain endpoints";
        }

        private static bool TrySolveEdgeWearMicroFeatureJunction(
            List<PolygonFace> sourceFaces,
            EdgeWearTopologyGraph graph,
            HashSet<int> componentVertices,
            float requestedWidth,
            float minimumStableEdgeLength,
            out Vector3 junction,
            out float maximumDisplacement,
            out float maximumPlaneResidual,
            out string rejection)
        {
            junction = default;
            maximumDisplacement = 0f;
            maximumPlaneResidual = 0f;
            rejection = string.Empty;
            HashSet<int> incidentGraphFaces = new HashSet<int>();
            Vector3 centroid = Vector3.zero;
            foreach (int vertexIndex in componentVertices)
            {
                centroid += graph.Vertices[vertexIndex].Position;
                List<int> faces = graph.Vertices[vertexIndex].FaceIndices;
                for (int faceOrdinal = 0;
                     faceOrdinal < faces.Count;
                     faceOrdinal++)
                {
                    incidentGraphFaces.Add(faces[faceOrdinal]);
                }
            }
            centroid /= Mathf.Max(1, componentVertices.Count);
            if (incidentGraphFaces.Count < 3 || incidentGraphFaces.Count > 8)
            {
                rejection =
                    "micro-feature chain does not define a bounded local face star";
                return false;
            }

            float a00 = 0f;
            float a01 = 0f;
            float a02 = 0f;
            float a11 = 0f;
            float a12 = 0f;
            float a22 = 0f;
            float b0 = 0f;
            float b1 = 0f;
            float b2 = 0f;
            foreach (int graphFaceIndex in incidentGraphFaces)
            {
                if (graphFaceIndex < 0 ||
                    graphFaceIndex >= graph.Faces.Count)
                {
                    rejection = "micro-feature face-star provenance is invalid";
                    return false;
                }
                int sourceFaceIndex =
                    graph.Faces[graphFaceIndex].SourceFaceIndex;
                if (sourceFaceIndex < 0 ||
                    sourceFaceIndex >= sourceFaces.Count)
                {
                    rejection = "micro-feature source face is unavailable";
                    return false;
                }
                PolygonFace face = sourceFaces[sourceFaceIndex];
                if (face == null || face.Vertices == null ||
                    face.Vertices.Count < 3 ||
                    !IsFinite(face.Normal) ||
                    face.Normal.sqrMagnitude <= MinimumEdgeLengthSqr)
                {
                    rejection = "micro-feature face plane is invalid";
                    return false;
                }
                Vector3 normal = face.Normal.normalized;
                float distance = Vector3.Dot(normal, face.Vertices[0]);
                a00 += normal.x * normal.x;
                a01 += normal.x * normal.y;
                a02 += normal.x * normal.z;
                a11 += normal.y * normal.y;
                a12 += normal.y * normal.z;
                a22 += normal.z * normal.z;
                b0 += normal.x * distance;
                b1 += normal.y * distance;
                b2 += normal.z * distance;
            }

            float regularization = Mathf.Max(
                0.01f,
                incidentGraphFaces.Count * 0.005f);
            a00 += regularization;
            a11 += regularization;
            a22 += regularization;
            b0 += centroid.x * regularization;
            b1 += centroid.y * regularization;
            b2 += centroid.z * regularization;
            if (!TrySolveEdgeWearMicroFeatureSymmetricSystem(
                    a00,
                    a01,
                    a02,
                    a11,
                    a12,
                    a22,
                    b0,
                    b1,
                    b2,
                    out junction) ||
                !IsFinite(junction))
            {
                rejection =
                    "micro-feature face planes have no stable common junction";
                return false;
            }

            foreach (int vertexIndex in componentVertices)
            {
                maximumDisplacement = Mathf.Max(
                    maximumDisplacement,
                    Vector3.Distance(
                        graph.Vertices[vertexIndex].Position,
                        junction));
            }
            foreach (int graphFaceIndex in incidentGraphFaces)
            {
                PolygonFace face = sourceFaces[
                    graph.Faces[graphFaceIndex].SourceFaceIndex];
                Vector3 normal = face.Normal.normalized;
                float distance = Vector3.Dot(normal, face.Vertices[0]);
                maximumPlaneResidual = Mathf.Max(
                    maximumPlaneResidual,
                    Mathf.Abs(Vector3.Dot(normal, junction) - distance));
            }

            float displacementLimit = Mathf.Max(
                PointMergeDistance * 8f,
                requestedWidth);
            float residualLimit = Mathf.Max(
                PointMergeDistance * 8f,
                minimumStableEdgeLength);
            if (maximumDisplacement >
                displacementLimit + PointMergeDistance)
            {
                rejection =
                    "micro-feature common junction exceeds the bevel displacement guard";
                return false;
            }
            if (maximumPlaneResidual >
                residualLimit + PointMergeDistance)
            {
                rejection =
                    "micro-feature common junction leaves its local face planes";
                return false;
            }
            return true;
        }

        private static bool TrySolveEdgeWearMicroFeatureSymmetricSystem(
            float a00,
            float a01,
            float a02,
            float a11,
            float a12,
            float a22,
            float b0,
            float b1,
            float b2,
            out Vector3 solution)
        {
            float c00 = a11 * a22 - a12 * a12;
            float c01 = a02 * a12 - a01 * a22;
            float c02 = a01 * a12 - a02 * a11;
            float c11 = a00 * a22 - a02 * a02;
            float c12 = a01 * a02 - a00 * a12;
            float c22 = a00 * a11 - a01 * a01;
            float determinant =
                a00 * c00 + a01 * c01 + a02 * c02;
            if (Mathf.Abs(determinant) <= 0.0000001f ||
                float.IsNaN(determinant) ||
                float.IsInfinity(determinant))
            {
                solution = default;
                return false;
            }
            float inverse = 1f / determinant;
            solution = new Vector3(
                (c00 * b0 + c01 * b1 + c02 * b2) * inverse,
                (c01 * b0 + c11 * b1 + c12 * b2) * inverse,
                (c02 * b0 + c12 * b1 + c22 * b2) * inverse);
            return IsFinite(solution);
        }

        private static List<PolygonFace> BuildEdgeWearMicroFeatureFaceView(
            List<PolygonFace> sourceFaces,
            Dictionary<VertexKey, Vector3> replacements)
        {
            List<PolygonFace> faces =
                new List<PolygonFace>(sourceFaces.Count);
            for (int faceIndex = 0;
                 faceIndex < sourceFaces.Count;
                 faceIndex++)
            {
                PolygonFace face = sourceFaces[faceIndex];
                if (face == null || face.Vertices == null ||
                    replacements == null || replacements.Count == 0)
                {
                    faces.Add(face);
                    continue;
                }
                List<Vector3> mapped =
                    new List<Vector3>(face.Vertices.Count);
                bool changed = false;
                for (int vertexIndex = 0;
                     vertexIndex < face.Vertices.Count;
                     vertexIndex++)
                {
                    Vector3 original = face.Vertices[vertexIndex];
                    Vector3 value = replacements.TryGetValue(
                            new VertexKey(original),
                            out Vector3 replacement)
                        ? replacement
                        : original;
                    changed |= !AreSamePoint(original, value);
                    if (mapped.Count == 0 ||
                        !AreSamePoint(mapped[mapped.Count - 1], value))
                    {
                        mapped.Add(value);
                    }
                }
                if (mapped.Count > 1 &&
                    AreSamePoint(mapped[0], mapped[mapped.Count - 1]))
                {
                    mapped.RemoveAt(mapped.Count - 1);
                }
                if (!changed)
                {
                    faces.Add(face);
                    continue;
                }
                faces.Add(new PolygonFace(
                    mapped,
                    face.Normal,
                    face.Feature,
                    face.FeatureStrength,
                    face.ProvenanceKind,
                    face.ProvenanceIndex));
            }
            return faces;
        }

        private static bool ValidateEdgeWearMicroFeatureFaceView(
            List<PolygonFace> sourceFaces,
            List<PolygonFace> normalizedFaces,
            float minimumStableFaceArea,
            out string rejection)
        {
            rejection = string.Empty;
            if (sourceFaces == null || normalizedFaces == null ||
                sourceFaces.Count != normalizedFaces.Count)
            {
                rejection = "micro-feature face view count changed";
                return false;
            }
            for (int faceIndex = 0;
                 faceIndex < normalizedFaces.Count;
                 faceIndex++)
            {
                PolygonFace face = normalizedFaces[faceIndex];
                PolygonFace source = sourceFaces[faceIndex];
                if (face == null || face.Vertices == null ||
                    face.Vertices.Count < 3)
                {
                    rejection =
                        "micro-feature normalization collapsed a source face";
                    return false;
                }
                HashSet<VertexKey> unique = new HashSet<VertexKey>();
                for (int vertexIndex = 0;
                     vertexIndex < face.Vertices.Count;
                     vertexIndex++)
                {
                    unique.Add(new VertexKey(face.Vertices[vertexIndex]));
                }
                if (unique.Count != face.Vertices.Count ||
                    CalculatePolygonArea(face.Vertices) <=
                        minimumStableFaceArea)
                {
                    rejection =
                        "micro-feature normalization created a repeated or unstable face loop";
                    return false;
                }
                Vector3 measured = CalculatePolygonNormal(face.Vertices);
                if (!IsFinite(measured) ||
                    measured.sqrMagnitude <= MinimumEdgeLengthSqr ||
                    source == null ||
                    Vector3.Dot(measured.normalized, source.Normal) < 0.95f)
                {
                    rejection =
                        "micro-feature normalization changed a source face winding";
                    return false;
                }
            }
            return true;
        }

        private static void AddEdgeWearMicroFeatureNormalizationRejection(
            EdgeWearMicroFeatureNormalizationStats stats,
            int edgeIndex,
            EdgeWearGraphEdge edge,
            string reason)
        {
            EdgeWearMicroFeatureNormalizationRecord record =
                new EdgeWearMicroFeatureNormalizationRecord();
            record.SourceEdgeIndices.Add(edgeIndex);
            record.SourceVertexIndices.Add(edge.VertexA);
            record.SourceVertexIndices.Add(edge.VertexB);
            record.Reason = reason ?? "micro-feature rejected";
            stats.RejectedEdgeCount++;
            RegisterEdgeWearMicroFeatureFirstRejection(
                stats,
                record.Reason);
            stats.Records.Add(record);
        }

        private static void RegisterEdgeWearMicroFeatureFirstRejection(
            EdgeWearMicroFeatureNormalizationStats stats,
            string reason)
        {
            if (stats != null &&
                string.IsNullOrEmpty(stats.FirstRejectionReason))
            {
                stats.FirstRejectionReason = reason ?? string.Empty;
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
