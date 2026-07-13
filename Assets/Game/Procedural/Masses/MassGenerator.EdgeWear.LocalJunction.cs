using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear bounded local-junction extraction

        private readonly struct LocalJunctionBoundarySegment
        {
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly VertexKey StartKey;
            public readonly VertexKey EndKey;

            public LocalJunctionBoundarySegment(
                Vector3 start,
                Vector3 end)
            {
                Start = start;
                End = end;
                StartKey = new VertexKey(start);
                EndKey = new VertexKey(end);
            }
        }

        private sealed class LocalJunctionBoundaryUse
        {
            public int Count;
            public LocalJunctionBoundarySegment Segment;

            public LocalJunctionBoundaryUse(
                LocalJunctionBoundarySegment segment)
            {
                Count = 1;
                Segment = segment;
            }
        }

        private static void AuditPlaneCutLocalJunctionStars(
            List<PolygonFace> edgeOnlyFaces,
            ChamferTopologyContext context,
            List<PlaneCutBevelCandidate> activeEdges,
            float minimumStableEdgeLength,
            ref PlaneCutBevelAuditResult result,
            out string blocker)
        {
            blocker = string.Empty;
            List<PlaneCutBevelCandidate> retainedEdges =
                new List<PlaneCutBevelCandidate>(activeEdges.Count);
            for (int edgeIndex = 0;
                 edgeIndex < activeEdges.Count;
                 edgeIndex++)
            {
                PlaneCutBevelCandidate candidate = activeEdges[edgeIndex];
                if (CountMatchingPlaneCutCaps(
                        edgeOnlyFaces,
                        candidate) == 1)
                {
                    retainedEdges.Add(candidate);
                }
            }
            Dictionary<int, List<PlaneCutBevelCandidate>> incidentByVertex =
                BuildPlaneCutIncidentMap(retainedEdges);

            for (int vertexIndex = 0;
                 vertexIndex < context.Graph.Vertices.Count;
                 vertexIndex++)
            {
                if (!incidentByVertex.TryGetValue(
                        vertexIndex,
                        out List<PlaneCutBevelCandidate> selectedIncident) ||
                    selectedIncident.Count < 2)
                {
                    continue;
                }

                result.LocalJunctionCandidateCount++;
                if (!TryExtractPlaneCutLocalJunctionStar(
                        edgeOnlyFaces,
                        context,
                        vertexIndex,
                        selectedIncident,
                        minimumStableEdgeLength,
                        out List<Vector3> orderedLoop,
                        out bool starExtracted,
                        out bool branched,
                        out bool selfIntersecting,
                        out bool foreignFace,
                        out bool missingIncidentBevel,
                        out bool duplicateIncidentBevel,
                        out float extentRatio,
                        out string starBlocker))
                {
                    if (starExtracted)
                    {
                        result.LocalJunctionStarsExtractedCount++;
                    }
                    if (branched)
                    {
                        result.LocalJunctionBranchedCount++;
                    }
                    if (selfIntersecting)
                    {
                        result.LocalJunctionSelfIntersectingCount++;
                    }
                    if (foreignFace)
                    {
                        result.LocalJunctionForeignFaceCount++;
                    }
                    if (missingIncidentBevel)
                    {
                        result.LocalJunctionMissingIncidentBevelCount++;
                    }
                    if (duplicateIncidentBevel)
                    {
                        result.LocalJunctionDuplicateIncidentBevelCount++;
                    }
                    result.LocalJunctionMaximumExtentRatio = Mathf.Max(
                        result.LocalJunctionMaximumExtentRatio,
                        extentRatio);
                    if (string.IsNullOrEmpty(blocker))
                    {
                        blocker = string.IsNullOrEmpty(starBlocker)
                            ? "local junction star v" + vertexIndex +
                                " could not produce one bounded loop"
                            : "local junction star v" + vertexIndex +
                                " " + starBlocker;
                    }
                    continue;
                }

                result.LocalJunctionStarsExtractedCount++;
                result.LocalJunctionClosedLoopCount++;
                result.LocalJunctionMaximumExtentRatio = Mathf.Max(
                    result.LocalJunctionMaximumExtentRatio,
                    extentRatio);
                int loopVertexCount = orderedLoop.Count;
                if (result.LocalJunctionMinimumLoopVertexCount == 0 ||
                    loopVertexCount <
                        result.LocalJunctionMinimumLoopVertexCount)
                {
                    result.LocalJunctionMinimumLoopVertexCount =
                        loopVertexCount;
                }
                result.LocalJunctionMaximumLoopVertexCount = Mathf.Max(
                    result.LocalJunctionMaximumLoopVertexCount,
                    loopVertexCount);
            }
        }

        private static bool TryExtractPlaneCutLocalJunctionStar(
            List<PolygonFace> edgeOnlyFaces,
            ChamferTopologyContext context,
            int vertexIndex,
            List<PlaneCutBevelCandidate> selectedIncident,
            float minimumStableEdgeLength,
            out List<Vector3> orderedLoop,
            out bool starExtracted,
            out bool branched,
            out bool selfIntersecting,
            out bool foreignFace,
            out bool missingIncidentBevel,
            out bool duplicateIncidentBevel,
            out float extentRatio,
            out string blocker)
        {
            orderedLoop = new List<Vector3>();
            starExtracted = false;
            branched = false;
            selfIntersecting = false;
            foreignFace = false;
            missingIncidentBevel = false;
            duplicateIncidentBevel = false;
            extentRatio = 0f;
            blocker = string.Empty;

            if (vertexIndex < 0 ||
                vertexIndex >= context.Graph.Vertices.Count)
            {
                blocker = "has invalid source-vertex provenance";
                return false;
            }

            EdgeWearGraphVertex sourceVertexRecord =
                context.Graph.Vertices[vertexIndex];
            Vector3 sourceVertex = sourceVertexRecord.Position;
            HashSet<int> allowedSourceFaces = new HashSet<int>(
                sourceVertexRecord.FaceIndices);
            HashSet<int> allowedBevelEdges = new HashSet<int>();
            Dictionary<int, float> selectedWidthByEdge =
                new Dictionary<int, float>();
            float maximumSelectedWidth = 0f;
            for (int incidentIndex = 0;
                 incidentIndex < selectedIncident.Count;
                 incidentIndex++)
            {
                PlaneCutBevelCandidate candidate =
                    selectedIncident[incidentIndex];
                allowedBevelEdges.Add(candidate.SourceEdgeIndex);
                selectedWidthByEdge[candidate.SourceEdgeIndex] =
                    candidate.Width;
                maximumSelectedWidth = Mathf.Max(
                    maximumSelectedWidth,
                    candidate.Width);
            }

            List<CutPlane> localBounds = new List<CutPlane>(
                sourceVertexRecord.EdgeIndices.Count);
            float maximumCutback = 0f;
            for (int incidentIndex = 0;
                 incidentIndex < sourceVertexRecord.EdgeIndices.Count;
                 incidentIndex++)
            {
                int graphEdgeIndex =
                    sourceVertexRecord.EdgeIndices[incidentIndex];
                if (graphEdgeIndex < 0 ||
                    graphEdgeIndex >= context.Graph.Edges.Count)
                {
                    continue;
                }

                EdgeWearGraphEdge edge =
                    context.Graph.Edges[graphEdgeIndex];
                int otherVertex = edge.VertexA == vertexIndex
                    ? edge.VertexB
                    : edge.VertexA;
                if (otherVertex < 0 ||
                    otherVertex >= context.Graph.Vertices.Count)
                {
                    continue;
                }

                Vector3 edgeVector =
                    context.Graph.Vertices[otherVertex].Position -
                    sourceVertex;
                float edgeLength = edgeVector.magnitude;
                if (edgeLength <= PointMergeDistance)
                {
                    continue;
                }

                Vector3 direction = edgeVector / edgeLength;
                float edgeWidth = selectedWidthByEdge.TryGetValue(
                    graphEdgeIndex,
                    out float selectedWidth)
                    ? selectedWidth
                    : maximumSelectedWidth;
                float minimumCutback = Mathf.Max(
                    PointMergeDistance * 12f,
                    minimumStableEdgeLength * 0.5f);
                float desiredCutback = Mathf.Max(
                    minimumCutback,
                    edgeWidth * 2.5f);
                float maximumAllowed = Mathf.Max(
                    PointMergeDistance * 4f,
                    edgeLength * 0.25f);
                float cutback = Mathf.Min(
                    desiredCutback,
                    maximumAllowed);
                maximumCutback = Mathf.Max(
                    maximumCutback,
                    cutback);
                localBounds.Add(new CutPlane(
                    direction,
                    Vector3.Dot(direction, sourceVertex) + cutback));
            }

            if (localBounds.Count < 3 ||
                maximumCutback <= PointMergeDistance)
            {
                blocker = "has insufficient incident-edge bounds";
                return false;
            }

            List<PolygonFace> boundedFaces =
                new List<PolygonFace>();
            Dictionary<int, int> bevelFaceCountByEdge =
                new Dictionary<int, int>();
            for (int faceIndex = 0;
                 faceIndex < edgeOnlyFaces.Count;
                 faceIndex++)
            {
                PolygonFace face = edgeOnlyFaces[faceIndex];
                List<Vector3> clipped = new List<Vector3>(face.Vertices);
                for (int boundIndex = 0;
                     boundIndex < localBounds.Count && clipped.Count >= 3;
                     boundIndex++)
                {
                    clipped = ClipPlaneCutLocalJunctionPolygon(
                        clipped,
                        face.Normal,
                        localBounds[boundIndex]);
                }
                if (clipped.Count < 3 ||
                    CalculatePolygonArea(clipped) <= TinyFaceAreaEpsilon)
                {
                    continue;
                }

                bool allowed = false;
                if (face.ProvenanceKind ==
                    PolygonFaceProvenanceKind.SourceFace)
                {
                    allowed = allowedSourceFaces.Contains(
                        face.ProvenanceIndex);
                }
                else if (face.ProvenanceKind ==
                    PolygonFaceProvenanceKind.EdgeBevelPlane)
                {
                    allowed = allowedBevelEdges.Contains(
                        face.ProvenanceIndex);
                    if (allowed)
                    {
                        bevelFaceCountByEdge.TryGetValue(
                            face.ProvenanceIndex,
                            out int faceCount);
                        bevelFaceCountByEdge[face.ProvenanceIndex] =
                            faceCount + 1;
                    }
                }

                if (!allowed)
                {
                    foreignFace = true;
                }

                boundedFaces.Add(new PolygonFace(
                    clipped,
                    face.Normal,
                    face.Feature,
                    face.FeatureStrength,
                    face.ProvenanceKind,
                    face.ProvenanceIndex));
            }

            if (boundedFaces.Count < 2)
            {
                blocker = "contains no bounded incident surface star";
                return false;
            }
            starExtracted = true;
            WeldSharedVertices(boundedFaces);

            foreach (int sourceEdgeIndex in allowedBevelEdges)
            {
                bevelFaceCountByEdge.TryGetValue(
                    sourceEdgeIndex,
                    out int count);
                if (count == 0)
                {
                    missingIncidentBevel = true;
                }
                else if (count > 1)
                {
                    duplicateIncidentBevel = true;
                }
            }

            Dictionary<EdgeKey, LocalJunctionBoundaryUse> edgeUses =
                new Dictionary<EdgeKey, LocalJunctionBoundaryUse>();
            for (int faceIndex = 0;
                 faceIndex < boundedFaces.Count;
                 faceIndex++)
            {
                List<Vector3> vertices = boundedFaces[faceIndex].Vertices;
                for (int edgeIndex = 0;
                     edgeIndex < vertices.Count;
                     edgeIndex++)
                {
                    Vector3 start = vertices[edgeIndex];
                    Vector3 end = vertices[
                        (edgeIndex + 1) % vertices.Count];
                    EdgeKey key = new EdgeKey(start, end);
                    if (edgeUses.TryGetValue(
                            key,
                            out LocalJunctionBoundaryUse use))
                    {
                        use.Count++;
                    }
                    else
                    {
                        edgeUses.Add(
                            key,
                            new LocalJunctionBoundaryUse(
                                new LocalJunctionBoundarySegment(
                                    start,
                                    end)));
                    }
                }
            }

            List<LocalJunctionBoundarySegment> boundarySegments =
                new List<LocalJunctionBoundarySegment>();
            foreach (LocalJunctionBoundaryUse use in edgeUses.Values)
            {
                if (use.Count == 1)
                {
                    boundarySegments.Add(use.Segment);
                }
            }

            if (!TryOrderPlaneCutLocalJunctionBoundary(
                    boundarySegments,
                    out orderedLoop,
                    out branched))
            {
                blocker = branched
                    ? "has a branched or disconnected boundary"
                    : "has no single closed boundary loop";
                return false;
            }

            float maximumExtent = 0f;
            for (int loopIndex = 0;
                 loopIndex < orderedLoop.Count;
                 loopIndex++)
            {
                maximumExtent = Mathf.Max(
                    maximumExtent,
                    Vector3.Distance(
                        sourceVertex,
                        orderedLoop[loopIndex]));
            }
            extentRatio = maximumCutback > PointMergeDistance
                ? maximumExtent / maximumCutback
                : 0f;

            selfIntersecting = IsPlaneCutLocalJunctionLoopSelfIntersecting(
                orderedLoop,
                context,
                vertexIndex,
                minimumStableEdgeLength);
            if (selfIntersecting)
            {
                blocker = "has a self-intersecting projected boundary";
                return false;
            }
            if (foreignFace)
            {
                blocker = "contains unrelated face provenance";
                return false;
            }
            if (missingIncidentBevel)
            {
                blocker = "is missing an incident bevel band";
                return false;
            }
            if (duplicateIncidentBevel)
            {
                blocker = "contains a duplicate incident bevel band";
                return false;
            }
            return orderedLoop.Count >= 3;
        }

        private static List<Vector3> ClipPlaneCutLocalJunctionPolygon(
            List<Vector3> source,
            Vector3 normal,
            CutPlane bound)
        {
            List<Vector3> capPoints = new List<Vector3>();
            List<Vector3> clipped = ClipPolygon(
                source,
                bound,
                capPoints,
                true,
                Mathf.Min(
                    PlaneEpsilon,
                    Mathf.Max(
                        PointMergeDistance * 0.25f,
                        PlaneEpsilon * 0.25f)),
                null);
            return SanitizePolygon(clipped, normal);
        }

        private static bool TryOrderPlaneCutLocalJunctionBoundary(
            List<LocalJunctionBoundarySegment> segments,
            out List<Vector3> ordered,
            out bool branched)
        {
            ordered = new List<Vector3>();
            branched = false;
            if (segments == null || segments.Count < 3)
            {
                return false;
            }

            Dictionary<VertexKey, Vector3> positionByKey =
                new Dictionary<VertexKey, Vector3>();
            Dictionary<VertexKey, List<VertexKey>> neighbours =
                new Dictionary<VertexKey, List<VertexKey>>();
            for (int segmentIndex = 0;
                 segmentIndex < segments.Count;
                 segmentIndex++)
            {
                LocalJunctionBoundarySegment segment =
                    segments[segmentIndex];
                positionByKey[segment.StartKey] = segment.Start;
                positionByKey[segment.EndKey] = segment.End;
                AddPlaneCutLocalJunctionNeighbour(
                    neighbours,
                    segment.StartKey,
                    segment.EndKey);
                AddPlaneCutLocalJunctionNeighbour(
                    neighbours,
                    segment.EndKey,
                    segment.StartKey);
            }

            VertexKey start = default;
            bool hasStart = false;
            foreach (KeyValuePair<VertexKey, List<VertexKey>> entry
                in neighbours)
            {
                if (entry.Value.Count != 2)
                {
                    branched = true;
                    return false;
                }
                if (!hasStart || entry.Key.CompareTo(start) < 0)
                {
                    start = entry.Key;
                    hasStart = true;
                }
            }
            if (!hasStart)
            {
                return false;
            }

            HashSet<VertexKey> visited = new HashSet<VertexKey>();
            VertexKey previous = default;
            bool hasPrevious = false;
            VertexKey current = start;
            int guard = neighbours.Count + 1;
            while (guard-- > 0)
            {
                if (visited.Contains(current))
                {
                    return current.Equals(start) &&
                        visited.Count == neighbours.Count;
                }
                visited.Add(current);
                ordered.Add(positionByKey[current]);

                List<VertexKey> currentNeighbours = neighbours[current];
                VertexKey next;
                if (!hasPrevious)
                {
                    next = currentNeighbours[0].CompareTo(
                        currentNeighbours[1]) <= 0
                        ? currentNeighbours[0]
                        : currentNeighbours[1];
                }
                else if (!currentNeighbours[0].Equals(previous))
                {
                    next = currentNeighbours[0];
                }
                else
                {
                    next = currentNeighbours[1];
                }

                previous = current;
                hasPrevious = true;
                current = next;
                if (current.Equals(start))
                {
                    return visited.Count == neighbours.Count &&
                        ordered.Count >= 3;
                }
            }
            return false;
        }

        private static void AddPlaneCutLocalJunctionNeighbour(
            Dictionary<VertexKey, List<VertexKey>> neighbours,
            VertexKey source,
            VertexKey target)
        {
            if (!neighbours.TryGetValue(
                    source,
                    out List<VertexKey> list))
            {
                list = new List<VertexKey>(2);
                neighbours.Add(source, list);
            }
            if (!list.Contains(target))
            {
                list.Add(target);
            }
        }

        private static bool IsPlaneCutLocalJunctionLoopSelfIntersecting(
            List<Vector3> loop,
            ChamferTopologyContext context,
            int vertexIndex,
            float minimumStableEdgeLength)
        {
            Vector3 normal = Vector3.zero;
            EdgeWearGraphVertex vertex = context.Graph.Vertices[vertexIndex];
            for (int faceIndex = 0;
                 faceIndex < vertex.FaceIndices.Count;
                 faceIndex++)
            {
                int sourceFaceIndex = vertex.FaceIndices[faceIndex];
                for (int graphFaceIndex = 0;
                     graphFaceIndex < context.Graph.Faces.Count;
                     graphFaceIndex++)
                {
                    EdgeWearGraphFace graphFace =
                        context.Graph.Faces[graphFaceIndex];
                    if (graphFace.SourceFaceIndex == sourceFaceIndex)
                    {
                        normal += graphFace.SourceFace.Normal;
                        break;
                    }
                }
            }
            if (!IsFinite(normal) ||
                normal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                normal = CalculatePolygonNormal(loop);
            }
            if (!IsFinite(normal) ||
                normal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return true;
            }
            normal.Normalize();

            Vector3 reference = Mathf.Abs(normal.y) < 0.9f
                ? Vector3.up
                : Vector3.right;
            Vector3 tangent = Vector3.Cross(reference, normal);
            if (tangent.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                reference = Vector3.forward;
                tangent = Vector3.Cross(reference, normal);
            }
            tangent.Normalize();
            Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;
            Vector3 origin = context.Graph.Vertices[vertexIndex].Position;
            List<Vector2> projected = new List<Vector2>(loop.Count);
            for (int loopIndex = 0;
                 loopIndex < loop.Count;
                 loopIndex++)
            {
                Vector3 offset = loop[loopIndex] - origin;
                projected.Add(new Vector2(
                    Vector3.Dot(offset, tangent),
                    Vector3.Dot(offset, bitangent)));
            }

            float epsilon = Mathf.Max(
                PointMergeDistance * 4f,
                minimumStableEdgeLength * 0.002f);
            return ChamferPatchPolygonSelfIntersects(
                projected,
                epsilon,
                out ChamferPatchIntersectionEvidence _);
        }

        #endregion
    }
}
