using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear contained-patch ownership utilities

        private static ChamferFaceIntersectionCache
            BuildChamferFaceIntersectionCache(
                List<ChamferProvisionalFaceRecord> records,
                float minimumPatchTriangleArea,
                ref ChamferEmissionStats stats)
        {
            List<List<ChamferDirectedTriangleGeometry>> fanTriangles =
                new List<List<ChamferDirectedTriangleGeometry>>(
                    records.Count);
            List<List<ChamferDirectedTriangleGeometry>>
                polygonTriangles =
                    new List<List<ChamferDirectedTriangleGeometry>>(
                        records.Count);
            List<List<ChamferBoundarySegment>> faceBoundarySegments =
                new List<List<ChamferBoundarySegment>>(records.Count);
            List<bool> polygonFailures = new List<bool>(records.Count);
            for (int faceIndex = 0;
                 faceIndex < records.Count;
                 faceIndex++)
            {
                PolygonFace renderFace = BuildChamferRenderFaithfulFace(
                    records[faceIndex].Face);
                fanTriangles.Add(BuildChamferDirectedFaceTriangles(
                    renderFace,
                    faceIndex,
                    minimumPatchTriangleArea));
                faceBoundarySegments.Add(
                    BuildChamferFaceBoundarySegments(renderFace));
                List<ChamferDirectedTriangleGeometry>
                    facePolygonTriangles =
                        BuildChamferPolygonAwareFaceTriangles(
                            renderFace,
                            faceIndex,
                            minimumPatchTriangleArea,
                            out bool failed);
                polygonTriangles.Add(facePolygonTriangles);
                polygonFailures.Add(failed);
            }
            return new ChamferFaceIntersectionCache(
                records,
                fanTriangles,
                polygonTriangles,
                faceBoundarySegments,
                polygonFailures);
        }

        private static bool AreChamferTrianglesCoplanar(
            ChamferDirectedTriangleGeometry first,
            ChamferDirectedTriangleGeometry second,
            float epsilon)
        {
            if (first == null || second == null)
            {
                return false;
            }
            return Mathf.Abs(Vector3.Dot(first.Normal, second.Normal)) >=
                    0.9999f &&
                Mathf.Abs(Vector3.Dot(
                    second.First - first.First,
                    first.Normal)) <= epsilon;
        }

        private static bool AreChamferTriangleSetsContained(
            List<ChamferDirectedTriangleGeometry> source,
            List<ChamferDirectedTriangleGeometry> container,
            float epsilon)
        {
            if (source == null || container == null ||
                source.Count == 0 || container.Count == 0)
            {
                return false;
            }
            for (int triangleIndex = 0;
                 triangleIndex < source.Count;
                 triangleIndex++)
            {
                ChamferDirectedTriangleGeometry triangle =
                    source[triangleIndex];
                Vector3[] samples =
                {
                    triangle.First,
                    triangle.Second,
                    triangle.Third,
                    (triangle.First + triangle.Second) * 0.5f,
                    (triangle.Second + triangle.Third) * 0.5f,
                    (triangle.Third + triangle.First) * 0.5f,
                    (triangle.First + triangle.Second + triangle.Third) / 3f
                };
                for (int sampleIndex = 0;
                     sampleIndex < samples.Length;
                     sampleIndex++)
                {
                    if (!IsChamferPointInsideTriangleSet(
                            samples[sampleIndex],
                            container,
                            epsilon))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool IsChamferPointInsideTriangleSet(
            Vector3 point,
            List<ChamferDirectedTriangleGeometry> triangles,
            float epsilon)
        {
            for (int i = 0; i < triangles.Count; i++)
            {
                if (IsChamferPointInsideOrOnTriangle(
                        point,
                        triangles[i],
                        epsilon))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsChamferPointInsideOrOnTriangle(
            Vector3 point,
            ChamferDirectedTriangleGeometry triangle,
            float epsilon)
        {
            if (triangle == null ||
                Mathf.Abs(Vector3.Dot(
                    point - triangle.First,
                    triangle.Normal)) > epsilon)
            {
                return false;
            }
            int dropAxis = GetChamferDirectedProjectionDropAxis(
                triangle.Normal);
            Vector2 projectedPoint = ProjectChamferDirectedPoint(
                point,
                dropAxis);
            Vector2 a = ProjectChamferDirectedPoint(
                triangle.First,
                dropAxis);
            Vector2 b = ProjectChamferDirectedPoint(
                triangle.Second,
                dropAxis);
            Vector2 c = ProjectChamferDirectedPoint(
                triangle.Third,
                dropAxis);
            float o1 = ChamferPatchCross2D(a, b, projectedPoint);
            float o2 = ChamferPatchCross2D(b, c, projectedPoint);
            float o3 = ChamferPatchCross2D(c, a, projectedPoint);
            float orientationEpsilon = epsilon * epsilon;
            bool hasNegative =
                o1 < -orientationEpsilon ||
                o2 < -orientationEpsilon ||
                o3 < -orientationEpsilon;
            bool hasPositive =
                o1 > orientationEpsilon ||
                o2 > orientationEpsilon ||
                o3 > orientationEpsilon;
            return !(hasNegative && hasPositive);
        }

        private static double CalculateChamferCoplanarTriangleOverlapArea(
            ChamferDirectedTriangleGeometry first,
            ChamferDirectedTriangleGeometry second)
        {
            int dropAxis = GetChamferDirectedProjectionDropAxis(
                first.Normal);
            List<Vector2> polygon = new List<Vector2>
            {
                ProjectChamferDirectedPoint(first.First, dropAxis),
                ProjectChamferDirectedPoint(first.Second, dropAxis),
                ProjectChamferDirectedPoint(first.Third, dropAxis)
            };
            Vector2[] clip =
            {
                ProjectChamferDirectedPoint(second.First, dropAxis),
                ProjectChamferDirectedPoint(second.Second, dropAxis),
                ProjectChamferDirectedPoint(second.Third, dropAxis)
            };
            float clipOrientation = Mathf.Sign(
                ChamferPatchCross2D(clip[0], clip[1], clip[2]));
            if (Mathf.Abs(clipOrientation) <= Mathf.Epsilon)
            {
                return 0.0;
            }
            for (int edgeIndex = 0; edgeIndex < 3; edgeIndex++)
            {
                Vector2 clipStart = clip[edgeIndex];
                Vector2 clipEnd = clip[(edgeIndex + 1) % 3];
                polygon = ClipChamferPolygonAgainstEdge(
                    polygon,
                    clipStart,
                    clipEnd,
                    clipOrientation);
                if (polygon.Count < 3)
                {
                    return 0.0;
                }
            }
            double twiceArea = 0.0;
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 current = polygon[i];
                Vector2 next = polygon[(i + 1) % polygon.Count];
                twiceArea +=
                    (double)current.x * next.y -
                    (double)next.x * current.y;
            }
            return Math.Abs(twiceArea) * 0.5;
        }

        private static List<Vector2> ClipChamferPolygonAgainstEdge(
            List<Vector2> source,
            Vector2 clipStart,
            Vector2 clipEnd,
            float clipOrientation)
        {
            List<Vector2> result = new List<Vector2>();
            if (source.Count == 0)
            {
                return result;
            }
            Vector2 previous = source[source.Count - 1];
            bool previousInside = IsChamferPointInsideClipEdge(
                previous,
                clipStart,
                clipEnd,
                clipOrientation);
            for (int i = 0; i < source.Count; i++)
            {
                Vector2 current = source[i];
                bool currentInside = IsChamferPointInsideClipEdge(
                    current,
                    clipStart,
                    clipEnd,
                    clipOrientation);
                if (currentInside != previousInside &&
                    TryGetChamferLineIntersection2D(
                        previous,
                        current,
                        clipStart,
                        clipEnd,
                        out Vector2 intersection))
                {
                    result.Add(intersection);
                }
                if (currentInside)
                {
                    result.Add(current);
                }
                previous = current;
                previousInside = currentInside;
            }
            return result;
        }

        private static bool IsChamferPointInsideClipEdge(
            Vector2 point,
            Vector2 edgeStart,
            Vector2 edgeEnd,
            float orientation)
        {
            float side = ChamferPatchCross2D(
                edgeStart,
                edgeEnd,
                point);
            return orientation > 0f ? side >= -1e-7f : side <= 1e-7f;
        }

        private static bool TryGetChamferLineIntersection2D(
            Vector2 firstStart,
            Vector2 firstEnd,
            Vector2 secondStart,
            Vector2 secondEnd,
            out Vector2 intersection)
        {
            intersection = Vector2.zero;
            Vector2 firstDirection = firstEnd - firstStart;
            Vector2 secondDirection = secondEnd - secondStart;
            float denominator =
                firstDirection.x * secondDirection.y -
                firstDirection.y * secondDirection.x;
            if (Mathf.Abs(denominator) <= 1e-12f)
            {
                return false;
            }
            Vector2 delta = secondStart - firstStart;
            float t =
                (delta.x * secondDirection.y -
                 delta.y * secondDirection.x) /
                denominator;
            intersection = firstStart + firstDirection * t;
            return true;
        }

        private static bool DoChamferBoundarySetsShareSegment(
            List<ChamferBoundarySegment> first,
            List<ChamferBoundarySegment> second,
            float epsilon)
        {
            if (first == null || second == null)
            {
                return false;
            }
            for (int i = 0; i < first.Count; i++)
            {
                if (IsChamferSegmentOnAnyBoundarySegment(
                        first[i].Start,
                        first[i].End,
                        second,
                        epsilon))
                {
                    return true;
                }
            }
            for (int i = 0; i < second.Count; i++)
            {
                if (IsChamferSegmentOnAnyBoundarySegment(
                        second[i].Start,
                        second[i].End,
                        first,
                        epsilon))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool
            IsChamferTriangleIntersectionConfinedToBoundary(
                ChamferDirectedTriangleGeometry first,
                ChamferDirectedTriangleGeometry second,
                List<ChamferBoundarySegment> firstBoundarySegments,
                List<ChamferBoundarySegment> secondBoundarySegments,
                float epsilon)
        {
            if (firstBoundarySegments == null ||
                secondBoundarySegments == null ||
                firstBoundarySegments.Count == 0 ||
                secondBoundarySegments.Count == 0)
            {
                return false;
            }
            float normalAlignment = Mathf.Abs(Vector3.Dot(
                first.Normal,
                second.Normal));
            float planeDistance = Mathf.Abs(Vector3.Dot(
                second.First - first.First,
                first.Normal));
            bool coplanar = normalAlignment >= 0.9999f &&
                planeDistance <= epsilon;
            if (coplanar)
            {
                return IsChamferCoplanarIntersectionConfinedToBoundary(
                    first,
                    second,
                    firstBoundarySegments,
                    secondBoundarySegments,
                    epsilon);
            }

            List<Vector3> points = new List<Vector3>();
            for (int edgeIndex = 0; edgeIndex < 3; edgeIndex++)
            {
                Vector3 start = first.GetPosition(edgeIndex);
                Vector3 end = first.GetPosition((edgeIndex + 1) % 3);
                if (TryChamferDirectedSegmentTriangleIntersection(
                        start,
                        end,
                        second,
                        epsilon,
                        out Vector3 point))
                {
                    points.Add(point);
                }
            }
            for (int edgeIndex = 0; edgeIndex < 3; edgeIndex++)
            {
                Vector3 start = second.GetPosition(edgeIndex);
                Vector3 end = second.GetPosition((edgeIndex + 1) % 3);
                if (TryChamferDirectedSegmentTriangleIntersection(
                        start,
                        end,
                        first,
                        epsilon,
                        out Vector3 point))
                {
                    points.Add(point);
                }
            }
            if (points.Count == 0)
            {
                return false;
            }
            for (int i = 0; i < points.Count; i++)
            {
                if (!IsChamferPointOnAnyBoundarySegment(
                        points[i],
                        firstBoundarySegments,
                        epsilon) ||
                    !IsChamferPointOnAnyBoundarySegment(
                        points[i],
                        secondBoundarySegments,
                        epsilon))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool
            IsChamferCoplanarIntersectionConfinedToBoundary(
                ChamferDirectedTriangleGeometry first,
                ChamferDirectedTriangleGeometry second,
                List<ChamferBoundarySegment> firstBoundarySegments,
                List<ChamferBoundarySegment> secondBoundarySegments,
                float epsilon)
        {
            int dropAxis = GetChamferDirectedProjectionDropAxis(first.Normal);
            Vector2[] firstProjected = new Vector2[3];
            Vector2[] secondProjected = new Vector2[3];
            for (int i = 0; i < 3; i++)
            {
                firstProjected[i] = ProjectChamferDirectedPoint(
                    first.GetPosition(i),
                    dropAxis);
                secondProjected[i] = ProjectChamferDirectedPoint(
                    second.GetPosition(i),
                    dropAxis);
            }
            for (int i = 0; i < 3; i++)
            {
                if (ChamferDirectedPointStrictlyInsideTriangle2D(
                        firstProjected[i],
                        secondProjected[0],
                        secondProjected[1],
                        secondProjected[2],
                        epsilon) &&
                    !IsChamferPointOnBothBoundarySets(
                        first.GetPosition(i),
                        firstBoundarySegments,
                        secondBoundarySegments,
                        epsilon))
                {
                    return false;
                }
                if (ChamferDirectedPointStrictlyInsideTriangle2D(
                        secondProjected[i],
                        firstProjected[0],
                        firstProjected[1],
                        firstProjected[2],
                        epsilon) &&
                    !IsChamferPointOnBothBoundarySets(
                        second.GetPosition(i),
                        firstBoundarySegments,
                        secondBoundarySegments,
                        epsilon))
                {
                    return false;
                }
            }
            bool sawBoundaryContact = false;
            for (int firstEdge = 0; firstEdge < 3; firstEdge++)
            {
                for (int secondEdge = 0; secondEdge < 3; secondEdge++)
                {
                    if (!TryGetChamferPatchSegmentIntersectionEvidence(
                            firstProjected[firstEdge],
                            firstProjected[(firstEdge + 1) % 3],
                            secondProjected[secondEdge],
                            secondProjected[(secondEdge + 1) % 3],
                            epsilon,
                            firstEdge,
                            secondEdge,
                            out ChamferPatchIntersectionEvidence evidence))
                    {
                        continue;
                    }
                    if (evidence.Type == ChamferPatchIntersectionType.Proper)
                    {
                        return false;
                    }
                    Vector3 firstStart = first.GetPosition(firstEdge);
                    Vector3 firstEnd = first.GetPosition(
                        (firstEdge + 1) % 3);
                    Vector3 secondStart = second.GetPosition(secondEdge);
                    Vector3 secondEnd = second.GetPosition(
                        (secondEdge + 1) % 3);
                    if (evidence.Type ==
                        ChamferPatchIntersectionType.CollinearOverlap)
                    {
                        if (!IsChamferSegmentOnAnyBoundarySegment(
                                firstStart,
                                firstEnd,
                                firstBoundarySegments,
                                epsilon) ||
                            !IsChamferSegmentOnAnyBoundarySegment(
                                secondStart,
                                secondEnd,
                                secondBoundarySegments,
                                epsilon))
                        {
                            return false;
                        }
                        sawBoundaryContact = true;
                        continue;
                    }
                    Vector3[] endpoints =
                    {
                        firstStart,
                        firstEnd,
                        secondStart,
                        secondEnd
                    };
                    bool endpointAllowed = false;
                    for (int endpointIndex = 0;
                         endpointIndex < endpoints.Length;
                         endpointIndex++)
                    {
                        if (IsChamferPointOnBothBoundarySets(
                                endpoints[endpointIndex],
                                firstBoundarySegments,
                                secondBoundarySegments,
                                epsilon))
                        {
                            endpointAllowed = true;
                            break;
                        }
                    }
                    if (!endpointAllowed)
                    {
                        return false;
                    }
                    sawBoundaryContact = true;
                }
            }
            return sawBoundaryContact;
        }

        private static bool IsChamferPointOnBothBoundarySets(
            Vector3 point,
            List<ChamferBoundarySegment> first,
            List<ChamferBoundarySegment> second,
            float epsilon)
        {
            return IsChamferPointOnAnyBoundarySegment(
                    point,
                    first,
                    epsilon) &&
                IsChamferPointOnAnyBoundarySegment(
                    point,
                    second,
                    epsilon);
        }

        private static bool IsChamferPointOnAnyBoundarySegment(
            Vector3 point,
            List<ChamferBoundarySegment> segments,
            float epsilon)
        {
            float epsilonSquared = epsilon * epsilon;
            for (int i = 0; i < segments.Count; i++)
            {
                if (DistanceChamferDirectedPointToSegmentSquared(
                        point,
                        segments[i].Start,
                        segments[i].End) <= epsilonSquared)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsChamferSegmentOnAnyBoundarySegment(
            Vector3 start,
            Vector3 end,
            List<ChamferBoundarySegment> segments,
            float epsilon)
        {
            Vector3 midpoint = (start + end) * 0.5f;
            float epsilonSquared = epsilon * epsilon;
            for (int i = 0; i < segments.Count; i++)
            {
                ChamferBoundarySegment segment = segments[i];
                if (DistanceChamferDirectedPointToSegmentSquared(
                        start,
                        segment.Start,
                        segment.End) <= epsilonSquared &&
                    DistanceChamferDirectedPointToSegmentSquared(
                        end,
                        segment.Start,
                        segment.End) <= epsilonSquared &&
                    DistanceChamferDirectedPointToSegmentSquared(
                        midpoint,
                        segment.Start,
                        segment.End) <= epsilonSquared)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool TryFindChamferIntersectingTriangle(
            ChamferDirectedTriangleGeometry candidate,
            List<ChamferDirectedTriangleGeometry> others,
            float epsilon,
            out ChamferDirectedTriangleGeometry hit)
        {
            hit = null;
            for (int i = 0; i < others.Count; i++)
            {
                if (!ChamferDirectedTrianglesIntersectImproperly(
                        candidate,
                        others[i],
                        epsilon))
                {
                    continue;
                }
                hit = others[i];
                return true;
            }
            return false;
        }

        private static List<ChamferDirectedTriangleGeometry>
            BuildChamferPolygonAwareFaceTriangles(
                PolygonFace face,
                int faceIndex,
                float minimumPatchTriangleArea,
                out bool failed)
        {
            failed = false;
            List<ChamferDirectedTriangleGeometry> result =
                new List<ChamferDirectedTriangleGeometry>();
            if (face == null || face.Vertices == null ||
                face.Vertices.Count < 3)
            {
                failed = true;
                return result;
            }
            List<Vector3> positions = new List<Vector3>(face.Vertices);
            Vector3 expectedNormal = face.Normal;
            if (!IsFinite(expectedNormal) ||
                expectedNormal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                expectedNormal = CalculatePolygonNormal(positions);
            }
            if (!IsFinite(expectedNormal) ||
                expectedNormal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                failed = true;
                return result;
            }
            expectedNormal.Normalize();
            if (positions.Count == 3)
            {
                if (TryCreateChamferDirectedTriangleGeometry(
                        positions[0],
                        positions[1],
                        positions[2],
                        faceIndex * 1024,
                        minimumPatchTriangleArea,
                        out ChamferDirectedTriangleGeometry triangle,
                        out _))
                {
                    result.Add(triangle);
                    return result;
                }
                failed = true;
                return result;
            }
            if (!TryProjectChamferPatchLoop(
                    positions,
                    expectedNormal,
                    out List<Vector2> projected,
                    out float projectedSignedArea,
                    out float projectionEpsilon))
            {
                failed = true;
                return result;
            }
            if (projectedSignedArea < 0f)
            {
                positions.Reverse();
                projected.Reverse();
                projectedSignedArea = -projectedSignedArea;
            }
            if (projectedSignedArea <=
                    projectionEpsilon * projectionEpsilon ||
                ChamferPatchPolygonSelfIntersects(
                    projected,
                    projectionEpsilon,
                    out _))
            {
                failed = true;
                return result;
            }
            if (!TryEarClipChamferPolygonSilently(
                    positions,
                    projected,
                    expectedNormal,
                    minimumPatchTriangleArea,
                    projectionEpsilon,
                    out List<List<Vector3>> triangles))
            {
                failed = true;
                return result;
            }
            for (int triangleIndex = 0;
                 triangleIndex < triangles.Count;
                 triangleIndex++)
            {
                List<Vector3> trianglePositions = triangles[triangleIndex];
                if (!TryCreateChamferDirectedTriangleGeometry(
                        trianglePositions[0],
                        trianglePositions[1],
                        trianglePositions[2],
                        faceIndex * 1024 + triangleIndex,
                        minimumPatchTriangleArea,
                        out ChamferDirectedTriangleGeometry triangle,
                        out _))
                {
                    failed = true;
                    result.Clear();
                    return result;
                }
                result.Add(triangle);
            }
            return result;
        }

        private static bool TryEarClipChamferPolygonSilently(
            List<Vector3> positions,
            List<Vector2> projected,
            Vector3 expectedNormal,
            float minimumPatchTriangleArea,
            float epsilon,
            out List<List<Vector3>> triangles)
        {
            triangles = new List<List<Vector3>>();
            List<int> remaining = new List<int>();
            for (int i = 0; i < positions.Count; i++)
            {
                remaining.Add(i);
            }
            while (remaining.Count > 3)
            {
                int selectedRemainingIndex = -1;
                int selectedOriginalIndex = int.MaxValue;
                for (int remainingIndex = 0;
                     remainingIndex < remaining.Count;
                     remainingIndex++)
                {
                    int previousIndex = remaining[
                        (remainingIndex - 1 + remaining.Count) %
                        remaining.Count];
                    int currentIndex = remaining[remainingIndex];
                    int nextIndex = remaining[
                        (remainingIndex + 1) % remaining.Count];
                    if (ChamferPatchCross2D(
                            projected[previousIndex],
                            projected[currentIndex],
                            projected[nextIndex]) <= epsilon * epsilon)
                    {
                        continue;
                    }
                    bool containsPoint = false;
                    for (int candidateIndex = 0;
                         candidateIndex < remaining.Count;
                         candidateIndex++)
                    {
                        int pointIndex = remaining[candidateIndex];
                        if (pointIndex == previousIndex ||
                            pointIndex == currentIndex ||
                            pointIndex == nextIndex)
                        {
                            continue;
                        }
                        if (ChamferPatchPointInOrOnTriangle(
                                projected[pointIndex],
                                projected[previousIndex],
                                projected[currentIndex],
                                projected[nextIndex],
                                epsilon))
                        {
                            containsPoint = true;
                            break;
                        }
                    }
                    if (containsPoint ||
                        ChamferPatchDiagonalIntersectsRemainingBoundary(
                            previousIndex,
                            nextIndex,
                            remaining,
                            projected,
                            epsilon))
                    {
                        continue;
                    }
                    List<Vector3> candidateTriangle = new List<Vector3>
                    {
                        positions[previousIndex],
                        positions[currentIndex],
                        positions[nextIndex]
                    };
                    if (CalculatePolygonArea(candidateTriangle) <=
                            minimumPatchTriangleArea ||
                        !TryCalculateChamferPatchTriangleNormal(
                            candidateTriangle[0],
                            candidateTriangle[1],
                            candidateTriangle[2],
                            out Vector3 normal) ||
                        Vector3.Dot(normal, expectedNormal) <= 0f)
                    {
                        continue;
                    }
                    if (currentIndex < selectedOriginalIndex)
                    {
                        selectedOriginalIndex = currentIndex;
                        selectedRemainingIndex = remainingIndex;
                    }
                }
                if (selectedRemainingIndex < 0)
                {
                    triangles.Clear();
                    return false;
                }
                int previousSelectedIndex = remaining[
                    (selectedRemainingIndex - 1 + remaining.Count) %
                    remaining.Count];
                int currentSelectedIndex = remaining[selectedRemainingIndex];
                int nextSelectedIndex = remaining[
                    (selectedRemainingIndex + 1) % remaining.Count];
                triangles.Add(new List<Vector3>
                {
                    positions[previousSelectedIndex],
                    positions[currentSelectedIndex],
                    positions[nextSelectedIndex]
                });
                remaining.RemoveAt(selectedRemainingIndex);
            }
            if (remaining.Count != 3)
            {
                triangles.Clear();
                return false;
            }
            triangles.Add(new List<Vector3>
            {
                positions[remaining[0]],
                positions[remaining[1]],
                positions[remaining[2]]
            });
            return true;
        }

        private static List<ChamferBoundarySegment>
            BuildChamferPatchBoundarySegments(
                List<ChamferProvisionalFaceRecord> records)
        {
            Dictionary<TopologyEdgeKey, int> counts =
                new Dictionary<TopologyEdgeKey, int>();
            Dictionary<TopologyEdgeKey, ChamferBoundarySegment> segments =
                new Dictionary<TopologyEdgeKey, ChamferBoundarySegment>();
            for (int recordIndex = 0;
                 recordIndex < records.Count;
                 recordIndex++)
            {
                PolygonFace face = records[recordIndex].Face;
                if (face == null || face.Vertices == null ||
                    face.Vertices.Count < 2)
                {
                    continue;
                }
                for (int edgeIndex = 0;
                     edgeIndex < face.Vertices.Count;
                     edgeIndex++)
                {
                    Vector3 start = face.Vertices[edgeIndex];
                    Vector3 end = face.Vertices[
                        (edgeIndex + 1) % face.Vertices.Count];
                    TopologyEdgeKey key = new TopologyEdgeKey(
                        new VertexKey(start),
                        new VertexKey(end));
                    counts.TryGetValue(key, out int count);
                    counts[key] = count + 1;
                    if (!segments.ContainsKey(key))
                    {
                        segments[key] = new ChamferBoundarySegment(
                            start,
                            end);
                    }
                }
            }
            List<ChamferBoundarySegment> result =
                new List<ChamferBoundarySegment>();
            foreach (KeyValuePair<TopologyEdgeKey, int> pair in counts)
            {
                if (pair.Value == 1)
                {
                    result.Add(segments[pair.Key]);
                }
            }
            return result;
        }

        private static List<ChamferBoundarySegment>
            BuildChamferFaceBoundarySegments(PolygonFace face)
        {
            List<ChamferBoundarySegment> result =
                new List<ChamferBoundarySegment>();
            if (face == null || face.Vertices == null ||
                face.Vertices.Count < 2)
            {
                return result;
            }
            for (int edgeIndex = 0;
                 edgeIndex < face.Vertices.Count;
                 edgeIndex++)
            {
                result.Add(new ChamferBoundarySegment(
                    face.Vertices[edgeIndex],
                    face.Vertices[(edgeIndex + 1) % face.Vertices.Count]));
            }
            return result;
        }

        private static PolygonFace BuildChamferRenderFaithfulFace(
            PolygonFace source)
        {
            if (source == null)
            {
                return null;
            }
            List<Vector3> sanitized = SanitizePolygon(
                source.Vertices,
                source.Normal);
            if (sanitized.Count < 3)
            {
                return null;
            }
            return new PolygonFace(
                sanitized,
                source.Normal,
                source.Feature,
                source.FeatureStrength);
        }

        private static bool TryBuildChamferCorrectedPatchTriangleGeometry(
            List<ChamferProvisionalFaceRecord> records,
            float minimumPatchTriangleArea,
            out List<ChamferDirectedTriangleGeometry> triangles)
        {
            triangles = new List<ChamferDirectedTriangleGeometry>(
                records.Count);
            for (int i = 0; i < records.Count; i++)
            {
                PolygonFace face = records[i].Face;
                if (face == null ||
                    face.Vertices == null ||
                    face.Vertices.Count != 3 ||
                    !TryCreateChamferDirectedTriangleGeometry(
                        face.Vertices[0],
                        face.Vertices[1],
                        face.Vertices[2],
                        i,
                        minimumPatchTriangleArea,
                        out ChamferDirectedTriangleGeometry triangle,
                        out _))
                {
                    triangles.Clear();
                    return false;
                }
                triangles.Add(triangle);
            }
            return true;
        }


        private static ChamferPatchOverlapClassification
            ClassifyChamferPatchOverlap(
                List<ChamferDirectedTriangleGeometry> patchTriangles,
                List<ChamferBoundarySegment> patchBoundarySegments,
                ChamferFaceIntersectionCache faceCache,
                float minimumStableEdgeLength)
        {
            ChamferPatchOverlapClassification result =
                new ChamferPatchOverlapClassification();
            float epsilon = Mathf.Max(
                PointMergeDistance * 2f,
                minimumStableEdgeLength * 0.001f);
            bool sawBevel = false;
            bool sawNonCoplanar = false;
            bool sawPartialCoplanar = false;
            bool sawPatchContained = false;
            bool sawFaceContained = false;
            double projectedOverlapArea = 0.0;
            int firstContainingBoundaryOwner = -1;

            for (int faceIndex = 0;
                 faceIndex < faceCache.Records.Count;
                 faceIndex++)
            {
                ChamferProvisionalFaceRecord faceRecord =
                    faceCache.Records[faceIndex];
                List<ChamferDirectedTriangleGeometry> faceTriangles =
                    faceCache.FanTriangles[faceIndex];
                List<ChamferBoundarySegment> faceBoundarySegments =
                    faceCache.FaceBoundarySegments[faceIndex];
                bool faceBlocking = false;
                bool faceCoplanar = true;
                double faceOverlapArea = 0.0;
                for (int patchTriangleIndex = 0;
                     patchTriangleIndex < patchTriangles.Count;
                     patchTriangleIndex++)
                {
                    ChamferDirectedTriangleGeometry patchTriangle =
                        patchTriangles[patchTriangleIndex];
                    for (int faceTriangleIndex = 0;
                         faceTriangleIndex < faceTriangles.Count;
                         faceTriangleIndex++)
                    {
                        ChamferDirectedTriangleGeometry faceTriangle =
                            faceTriangles[faceTriangleIndex];
                        if (!ChamferDirectedTrianglesIntersectImproperly(
                                patchTriangle,
                                faceTriangle,
                                epsilon) ||
                            IsChamferTriangleIntersectionConfinedToBoundary(
                                patchTriangle,
                                faceTriangle,
                                patchBoundarySegments,
                                faceBoundarySegments,
                                epsilon))
                        {
                            continue;
                        }
                        faceBlocking = true;
                        bool coplanar = AreChamferTrianglesCoplanar(
                            patchTriangle,
                            faceTriangle,
                            epsilon);
                        faceCoplanar &= coplanar;
                        if (coplanar)
                        {
                            faceOverlapArea +=
                                CalculateChamferCoplanarTriangleOverlapArea(
                                    patchTriangle,
                                    faceTriangle);
                        }
                    }
                }
                if (!faceBlocking)
                {
                    continue;
                }

                result.HasBlockingOverlap = true;
                projectedOverlapArea += faceOverlapArea;
                bool boundaryOwner = DoChamferBoundarySetsShareSegment(
                    patchBoundarySegments,
                    faceBoundarySegments,
                    epsilon);
                result.HasBoundaryOwner |= boundaryOwner;
                if (faceRecord.Kind ==
                    ChamferProvisionalFaceKind.BevelStrip)
                {
                    sawBevel = true;
                }
                else if (!faceCoplanar)
                {
                    sawNonCoplanar = true;
                }
                else
                {
                    bool patchContained =
                        AreChamferTriangleSetsContained(
                            patchTriangles,
                            faceTriangles,
                            epsilon);
                    bool faceContained =
                        AreChamferTriangleSetsContained(
                            faceTriangles,
                            patchTriangles,
                            epsilon);
                    if (patchContained)
                    {
                        result.ContainingReplacementCount++;
                        if (boundaryOwner)
                        {
                            result.ContainingBoundaryOwnerCount++;
                            if (firstContainingBoundaryOwner < 0)
                            {
                                firstContainingBoundaryOwner = faceIndex;
                            }
                        }
                    }
                    if (patchContained && !faceContained)
                    {
                        sawPatchContained = true;
                    }
                    else if (faceContained && !patchContained)
                    {
                        sawFaceContained = true;
                    }
                    else if (patchContained && faceContained)
                    {
                        sawPatchContained = true;
                    }
                    else
                    {
                        sawPartialCoplanar = true;
                    }
                }
            }

            if (!result.HasBlockingOverlap)
            {
                result.Kind = ChamferPatchOverlapKind.None;
            }
            else if (sawBevel)
            {
                result.Kind = ChamferPatchOverlapKind.BevelStripPenetration;
            }
            else if (sawNonCoplanar)
            {
                result.Kind = ChamferPatchOverlapKind.NonCoplanarPenetration;
            }
            else if (sawPartialCoplanar)
            {
                result.Kind = ChamferPatchOverlapKind.PartialCoplanarArea;
            }
            else if (sawFaceContained)
            {
                result.Kind =
                    ChamferPatchOverlapKind.ReplacementContainedInPatch;
            }
            else if (sawPatchContained)
            {
                result.Kind =
                    ChamferPatchOverlapKind.PatchContainedInReplacement;
            }
            else
            {
                result.Kind = ChamferPatchOverlapKind.Unclassified;
            }
            result.DeterministicOwnerRecordIndex =
                result.ContainingBoundaryOwnerCount == 1
                    ? firstContainingBoundaryOwner
                    : -1;
            result.ProjectedAreaNanounits =
                (long)Math.Round(projectedOverlapArea * 1000000000.0);
            return result;
        }

        private static List<ChamferContainedPatchCandidate>
            BuildChamferContainedPatchCandidates(
                List<ChamferProvisionalFaceRecord> successfulPatchRecords,
                List<ChamferProvisionalFaceRecord> prePatchFaceRecords,
                float minimumStableEdgeLength,
                float minimumPatchTriangleArea)
        {
            Dictionary<int, List<ChamferProvisionalFaceRecord>> byLoop =
                new Dictionary<int,
                    List<ChamferProvisionalFaceRecord>>();
            for (int i = 0; i < successfulPatchRecords.Count; i++)
            {
                ChamferProvisionalFaceRecord record =
                    successfulPatchRecords[i];
                if (!byLoop.TryGetValue(
                        record.PatchLoopIndex,
                        out List<ChamferProvisionalFaceRecord> records))
                {
                    records = new List<ChamferProvisionalFaceRecord>();
                    byLoop.Add(record.PatchLoopIndex, records);
                }
                records.Add(record);
            }

            ChamferEmissionStats ignoredStats = default;
            ChamferFaceIntersectionCache faceCache =
                BuildChamferFaceIntersectionCache(
                    prePatchFaceRecords,
                    minimumPatchTriangleArea,
                    ref ignoredStats);
            List<int> loopIndices = new List<int>(byLoop.Keys);
            loopIndices.Sort();
            List<ChamferContainedPatchCandidate> result =
                new List<ChamferContainedPatchCandidate>();
            for (int i = 0; i < loopIndices.Count; i++)
            {
                int loopIndex = loopIndices[i];
                List<ChamferProvisionalFaceRecord> records =
                    byLoop[loopIndex];
                if (!TryBuildChamferCorrectedPatchTriangleGeometry(
                        records,
                        minimumPatchTriangleArea,
                        out List<ChamferDirectedTriangleGeometry>
                            triangles))
                {
                    continue;
                }
                ChamferPatchOverlapClassification classification =
                    ClassifyChamferPatchOverlap(
                        triangles,
                        BuildChamferPatchBoundarySegments(records),
                        faceCache,
                        minimumStableEdgeLength);
                if (classification.Kind !=
                    ChamferPatchOverlapKind.PatchContainedInReplacement)
                {
                    continue;
                }
                result.Add(new ChamferContainedPatchCandidate(
                    loopIndex,
                    classification.DeterministicOwnerRecordIndex,
                    classification.ContainingReplacementCount,
                    classification.ContainingBoundaryOwnerCount));
            }
            return result;
        }

        #endregion
    }
}
