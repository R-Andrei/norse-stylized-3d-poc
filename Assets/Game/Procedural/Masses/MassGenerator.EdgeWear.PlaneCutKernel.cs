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
            public readonly CutPlane Plane;
            public readonly float Strength;
            public readonly float PlaneTolerance;
            public readonly Vector3 SourceStart;
            public readonly Vector3 SourceEnd;

            public PlaneCutBevelCandidate(
                CutPlane plane,
                float strength,
                float planeTolerance,
                Vector3 sourceStart,
                Vector3 sourceEnd)
            {
                Plane = plane;
                Strength = strength;
                PlaneTolerance = planeTolerance;
                SourceStart = sourceStart;
                SourceEnd = sourceEnd;
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

        private struct PlaneCutBevelAuditResult
        {
            public int SelectedEdgeCount;
            public int ActiveEdgeCount;
            public int PlanesBuilt;
            public int PlanesRejected;
            public int CapsBuilt;
            public int CapsMissing;
            public int CapsRedundant;
            public int ConformalSplitCount;
            public int OpenEdgeCount;
            public int NonManifoldEdgeCount;
            public int TJunctionCount;
            public int InvalidFaceCount;
            public int GeometryValid;
            public string Diagnostic;
        }

        private static PlaneCutBevelAuditResult AuditPlaneCutBevelKernel(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            ChamferCornerSolution solution,
            float minimumStableEdgeLength,
            float minimumStableFaceArea)
        {
            PlaneCutBevelAuditResult result =
                new PlaneCutBevelAuditResult
                {
                    SelectedEdgeCount = context.SelectedSourceEdges.Count
                };

            List<EdgeWearSelectedGraphEdge> orderedSelected =
                new List<EdgeWearSelectedGraphEdge>(context.SelectedEdges);
            orderedSelected.Sort((left, right) =>
                left.GraphEdgeIndex.CompareTo(right.GraphEdgeIndex));

            List<PlaneCutBevelCandidate> planeCandidates =
                new List<PlaneCutBevelCandidate>(orderedSelected.Count);
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
                        out string blocker))
                {
                    result.PlanesRejected++;
                    SetPlaneCutBevelDiagnostic(
                        ref result.Diagnostic,
                        blocker);
                    continue;
                }

                planeCandidates.Add(candidate);
                result.PlanesBuilt++;
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
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    "no valid bevel cut plane");
                return result;
            }

            List<PolygonFace> clonedFaces =
                ClonePolygonFacesForPlaneCutAudit(sourceFaces);
            Bounds sourceBounds = CalculateFaceBounds(sourceFaces);
            double sourceVolume = CalculatePlaneCutPolyhedronVolume(sourceFaces);

            for (int candidateIndex = 0;
                 candidateIndex < planeCandidates.Count;
                 candidateIndex++)
            {
                PlaneCutBevelCandidate candidate =
                    planeCandidates[candidateIndex];
                int before = CountMatchingPlaneCutCaps(
                    clonedFaces,
                    candidate);
                ClipPolyhedron(
                    clonedFaces,
                    candidate.Plane,
                    PolygonFaceFeature.ConvexEdgeWear,
                    candidate.Strength,
                    true);
                int after = CountMatchingPlaneCutCaps(
                    clonedFaces,
                    candidate);
                if (before == 0 && after == 1)
                {
                    result.CapsBuilt++;
                }
                else if (after != 1)
                {
                    SetPlaneCutBevelDiagnostic(
                        ref result.Diagnostic,
                        after == 0
                            ? "a bevel plane emitted no cap"
                            : "a bevel plane emitted duplicate caps");
                }
            }

            result.ConformalSplitCount =
                ConformPlaneCutFaceBoundaries(
                    clonedFaces,
                    minimumStableEdgeLength);

            for (int candidateIndex = 0;
                 candidateIndex < planeCandidates.Count;
                 candidateIndex++)
            {
                PlaneCutBevelCandidate candidate =
                    planeCandidates[candidateIndex];
                int capCount = CountMatchingPlaneCutCaps(
                    clonedFaces,
                    candidate);
                if (capCount == 1)
                {
                    continue;
                }

                if (capCount == 0 &&
                    IsPlaneCutCandidateRedundant(
                        clonedFaces,
                        candidate,
                        minimumStableEdgeLength))
                {
                    result.CapsRedundant++;
                    continue;
                }

                result.CapsMissing++;
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    capCount == 0
                        ? "a bevel plane has no surviving cap and is not redundant"
                        : "a bevel plane retains duplicate caps");
            }

            EdgeWearTopologyStats topology = AuditEdgeWearTopology(
                clonedFaces,
                minimumStableEdgeLength);
            result.OpenEdgeCount = topology.OpenEdgeCount;
            result.NonManifoldEdgeCount = topology.NonManifoldEdgeCount;
            result.TJunctionCount = topology.TJunctionCount;
            result.InvalidFaceCount = CountInvalidPlaneCutFaces(
                clonedFaces,
                minimumStableFaceArea);

            double resultVolume =
                CalculatePlaneCutPolyhedronVolume(clonedFaces);
            bool volumeValid = sourceVolume > 0.000000001 &&
                resultVolume > sourceVolume * 0.5 &&
                resultVolume <= sourceVolume * 1.0001;
            bool boundsValid = ArePlaneCutBoundsContained(
                sourceBounds,
                CalculateFaceBounds(clonedFaces),
                Mathf.Max(
                    PlaneEpsilon * 1.25f,
                    Mathf.Max(
                        PointMergeDistance * 8f,
                        minimumStableEdgeLength * 0.02f)));

            if (!volumeValid)
            {
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    "the plane-cut clone has an invalid retained volume");
            }
            if (!boundsValid)
            {
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    "the plane-cut clone exceeds the source bounds");
            }
            if (result.OpenEdgeCount > 0)
            {
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    "the plane-cut clone has open edges");
            }
            if (result.NonManifoldEdgeCount > 0)
            {
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    "the plane-cut clone has non-manifold edges");
            }
            if (result.TJunctionCount > 0)
            {
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    "the plane-cut clone has T-junctions");
            }
            if (result.InvalidFaceCount > 0)
            {
                SetPlaneCutBevelDiagnostic(
                    ref result.Diagnostic,
                    "the plane-cut clone has invalid faces");
            }

            bool geometryValid =
                result.PlanesRejected == 0 &&
                result.PlanesBuilt == result.ActiveEdgeCount &&
                result.CapsBuilt == result.ActiveEdgeCount &&
                result.CapsMissing == 0 &&
                result.OpenEdgeCount == 0 &&
                result.NonManifoldEdgeCount == 0 &&
                result.TJunctionCount == 0 &&
                result.InvalidFaceCount == 0 &&
                volumeValid &&
                boundsValid &&
                clonedFaces.Count >= 4;
            result.GeometryValid = geometryValid ? 1 : 0;
            return result;
        }

        private static bool TryBuildPlaneCutBevelCandidate(
            ChamferTopologyContext context,
            ChamferCornerSolution solution,
            EdgeWearSelectedGraphEdge selected,
            float minimumStableEdgeLength,
            out PlaneCutBevelCandidate candidate,
            out string blocker)
        {
            candidate = default;
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

            float distance = (d0 + d1 + d2 + d3) * 0.25f;
            CutPlane plane = new CutPlane(normal, distance);
            Vector3 sourceA =
                context.Graph.Vertices[edge.VertexA].Position;
            Vector3 sourceB =
                context.Graph.Vertices[edge.VertexB].Position;
            float minimumRemoval = Mathf.Max(
                PointMergeDistance * 2f,
                minimumStableEdgeLength * 0.02f);
            if (plane.SignedDistance(sourceA) <= minimumRemoval ||
                plane.SignedDistance(sourceB) <= minimumRemoval)
            {
                blocker = "the solved bevel plane does not remove its source edge";
                return false;
            }

            candidate = new PlaneCutBevelCandidate(
                plane,
                selected.Candidate.Strength,
                planeTolerance,
                sourceA,
                sourceB);
            return true;
        }

        private static List<PolygonFace> ClonePolygonFacesForPlaneCutAudit(
            List<PolygonFace> sourceFaces)
        {
            List<PolygonFace> cloned =
                new List<PolygonFace>(sourceFaces.Count);
            for (int faceIndex = 0;
                 faceIndex < sourceFaces.Count;
                 faceIndex++)
            {
                PolygonFace source = sourceFaces[faceIndex];
                cloned.Add(new PolygonFace(
                    new List<Vector3>(source.Vertices),
                    source.Normal,
                    source.Feature,
                    source.FeatureStrength));
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

        private static bool IsPlaneCutCandidateRedundant(
            List<PolygonFace> faces,
            PlaneCutBevelCandidate candidate,
            float minimumStableEdgeLength)
        {
            float planeTolerance = Mathf.Max(
                candidate.PlaneTolerance,
                PlaneEpsilon * 1.25f);
            List<Vector3> contactPoints = new List<Vector3>();
            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                List<Vector3> vertices = faces[faceIndex].Vertices;
                for (int vertexIndex = 0;
                     vertexIndex < vertices.Count;
                     vertexIndex++)
                {
                    float distance = candidate.Plane.SignedDistance(
                        vertices[vertexIndex]);
                    if (distance > planeTolerance)
                    {
                        return false;
                    }
                    if (Mathf.Abs(distance) <= planeTolerance)
                    {
                        contactPoints.Add(vertices[vertexIndex]);
                    }
                }
            }

            List<Vector3> uniqueContacts =
                GetUniquePoints(contactPoints);
            if (uniqueContacts.Count >= 3)
            {
                PolygonFace contactFace = CreateOrientedFace(
                    candidate.Plane.Normal,
                    uniqueContacts.ToArray());
                if (CalculatePolygonArea(contactFace.Vertices) >
                    TinyFaceAreaEpsilon)
                {
                    return false;
                }
            }

            float sourceTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                minimumStableEdgeLength * 0.02f);
            float sourceToleranceSqr =
                sourceTolerance * sourceTolerance;
            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                List<Vector3> vertices = faces[faceIndex].Vertices;
                for (int edgeIndex = 0;
                     edgeIndex < vertices.Count;
                     edgeIndex++)
                {
                    Vector3 start = vertices[edgeIndex];
                    Vector3 end =
                        vertices[(edgeIndex + 1) % vertices.Count];
                    if (DoPlaneCutSegmentsOverlap(
                            candidate.SourceStart,
                            candidate.SourceEnd,
                            start,
                            end,
                            sourceToleranceSqr))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool DoPlaneCutSegmentsOverlap(
            Vector3 sourceStart,
            Vector3 sourceEnd,
            Vector3 testStart,
            Vector3 testEnd,
            float toleranceSqr)
        {
            Vector3 source = sourceEnd - sourceStart;
            float sourceLengthSqr = source.sqrMagnitude;
            if (sourceLengthSqr <= MinimumEdgeLengthSqr)
            {
                return false;
            }

            float startParameter = Vector3.Dot(
                testStart - sourceStart,
                source) / sourceLengthSqr;
            float endParameter = Vector3.Dot(
                testEnd - sourceStart,
                source) / sourceLengthSqr;
            Vector3 startClosest =
                sourceStart + source * startParameter;
            Vector3 endClosest =
                sourceStart + source * endParameter;
            if ((testStart - startClosest).sqrMagnitude > toleranceSqr ||
                (testEnd - endClosest).sqrMagnitude > toleranceSqr)
            {
                return false;
            }

            float overlapStart = Mathf.Max(
                0f,
                Mathf.Min(startParameter, endParameter));
            float overlapEnd = Mathf.Min(
                1f,
                Mathf.Max(startParameter, endParameter));
            float sourceLength = Mathf.Sqrt(sourceLengthSqr);
            return (overlapEnd - overlapStart) * sourceLength >
                Mathf.Sqrt(toleranceSqr);
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
