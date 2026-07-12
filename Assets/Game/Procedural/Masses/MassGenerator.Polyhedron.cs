using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Plane-cut polyhedron utilities

        private static Bounds CalculateFaceBounds(List<PolygonFace> faces)
        {
            if (faces == null || faces.Count == 0)
            {
                return new Bounds(Vector3.zero, Vector3.one);
            }

            bool initialized = false;
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                List<Vector3> vertices = faces[faceIndex].Vertices;
                for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
                {
                    if (!initialized)
                    {
                        bounds = new Bounds(vertices[vertexIndex], Vector3.zero);
                        initialized = true;
                    }
                    else
                    {
                        bounds.Encapsulate(vertices[vertexIndex]);
                    }
                }
            }

            return initialized
                ? bounds
                : new Bounds(Vector3.zero, Vector3.one);
        }

        private static float GetCurrentSupport(
            List<PolygonFace> faces,
            Vector3 normal)
        {
            float support = float.NegativeInfinity;

            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                List<Vector3> vertices = faces[faceIndex].Vertices;

                for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
                {
                    support = Mathf.Max(
                        support,
                        Vector3.Dot(normal, vertices[vertexIndex]));
                }
            }

            return support;
        }

        private static void ClipPolyhedron(
            List<PolygonFace> faces,
            CutPlane plane,
            PolygonFaceFeature capFeature = PolygonFaceFeature.Base,
            float capFeatureStrength = 0f)
        {
            List<PolygonFace> clippedFaces = new List<PolygonFace>();
            List<Vector3> capPoints = new List<Vector3>();

            for (int i = 0; i < faces.Count; i++)
            {
                List<Vector3> clipped = ClipPolygon(
                    faces[i].Vertices,
                    plane,
                    capPoints);

                clipped = SanitizePolygon(
                    clipped,
                    faces[i].Normal);

                if (clipped.Count >= 3 &&
                    CalculatePolygonArea(clipped) > TinyFaceAreaEpsilon)
                {
                    clippedFaces.Add(
                        new PolygonFace(
                            clipped,
                            faces[i].Normal,
                            faces[i].Feature,
                            faces[i].FeatureStrength));
                }
            }

            List<Vector3> uniqueCapPoints =
                GetUniquePoints(capPoints);

            if (uniqueCapPoints.Count >= 3)
            {
                PolygonFace capFace = CreateOrientedFace(
                    plane.Normal,
                    capFeature,
                    capFeatureStrength,
                    uniqueCapPoints.ToArray());

                List<Vector3> sanitizedCap = SanitizePolygon(
                    capFace.Vertices,
                    capFace.Normal);

                if (sanitizedCap.Count >= 3 &&
                    CalculatePolygonArea(sanitizedCap) > TinyFaceAreaEpsilon)
                {
                    clippedFaces.Add(
                        new PolygonFace(
                            sanitizedCap,
                            capFace.Normal,
                            capFace.Feature,
                            capFace.FeatureStrength));
                }
            }

            WeldSharedVertices(clippedFaces);
            SanitizeAllFaces(clippedFaces);

            if (clippedFaces.Count >= 4)
            {
                faces.Clear();
                faces.AddRange(clippedFaces);
            }
        }

        private static bool TryClipPolyhedron(
            List<PolygonFace> faces,
            CutPlane plane,
            PolygonFaceFeature capFeature,
            float capFeatureStrength,
            float minimumStableFaceArea,
            float minimumStableEdgeLength)
        {
            if (faces.Count < 4)
            {
                return false;
            }

            List<PolygonFace> snapshot = ClonePolygonFaces(faces);
            int previousFeatureFaceCount = CountFeatureFaces(faces, capFeature);

            ClipPolyhedron(
                faces,
                plane,
                capFeature,
                capFeatureStrength);

            bool accepted =
                CountFeatureFaces(faces, capFeature) > previousFeatureFaceCount &&
                ValidatePolyhedronFaces(
                    faces,
                    minimumStableFaceArea,
                    minimumStableEdgeLength);

            if (accepted)
            {
                return true;
            }

            faces.Clear();
            faces.AddRange(snapshot);
            return false;
        }

        private static List<PolygonFace> ClonePolygonFaces(
            List<PolygonFace> faces)
        {
            List<PolygonFace> clone = new List<PolygonFace>(faces.Count);
            for (int i = 0; i < faces.Count; i++)
            {
                clone.Add(
                    new PolygonFace(
                        new List<Vector3>(faces[i].Vertices),
                        faces[i].Normal,
                        faces[i].Feature,
                        faces[i].FeatureStrength));
            }

            return clone;
        }

        private static int CountFeatureFaces(
            List<PolygonFace> faces,
            PolygonFaceFeature feature)
        {
            int count = 0;
            for (int i = 0; i < faces.Count; i++)
            {
                if (faces[i].Feature == feature)
                {
                    count++;
                }
            }

            return count;
        }

private static bool ValidatePolyhedronFaces(
            List<PolygonFace> faces,
            float minimumStableFaceArea,
            float minimumStableEdgeLength)
        {
            if (faces.Count < 4)
            {
                return false;
            }

            float minimumEdgeLengthSqr = minimumStableEdgeLength * minimumStableEdgeLength;
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                List<Vector3> vertices = faces[faceIndex].Vertices;
                if (vertices.Count < 3 ||
                    CalculatePolygonArea(vertices) <= minimumStableFaceArea)
                {
                    return false;
                }

                for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
                {
                    Vector3 start = vertices[vertexIndex];
                    Vector3 end = vertices[(vertexIndex + 1) % vertices.Count];
                    if ((end - start).sqrMagnitude <= minimumEdgeLengthSqr)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static List<Vector3> ClipPolygon(
            List<Vector3> vertices,
            CutPlane plane,
            List<Vector3> capPoints)
        {
            List<Vector3> result = new List<Vector3>();

            Vector3 previous = vertices[vertices.Count - 1];
            float previousDistance = plane.SignedDistance(previous);
            bool previousInside = previousDistance <= PlaneEpsilon;

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 current = vertices[i];
                float currentDistance = plane.SignedDistance(current);
                bool currentInside = currentDistance <= PlaneEpsilon;

                if (previousInside && currentInside)
                {
                    AddPointIfDifferent(result, current);
                }
                else if (previousInside && !currentInside)
                {
                    Vector3 intersection = IntersectEdge(
                        previous,
                        current,
                        previousDistance,
                        currentDistance);

                    AddPointIfDifferent(result, intersection);
                    capPoints.Add(intersection);
                }
                else if (!previousInside && currentInside)
                {
                    Vector3 intersection = IntersectEdge(
                        previous,
                        current,
                        previousDistance,
                        currentDistance);

                    AddPointIfDifferent(result, intersection);
                    AddPointIfDifferent(result, current);
                    capPoints.Add(intersection);
                }

                previous = current;
                previousDistance = currentDistance;
                previousInside = currentInside;
            }

            RemoveClosingDuplicate(result);
            return result;
        }

        private static Vector3 IntersectEdge(
            Vector3 start,
            Vector3 end,
            float startDistance,
            float endDistance)
        {
            float denominator = startDistance - endDistance;

            if (Mathf.Abs(denominator) <= PlaneEpsilon)
            {
                return start;
            }

            float t = startDistance / denominator;
            return Vector3.LerpUnclamped(start, end, t);
        }

        private static PolygonFace CreateOrientedFace(
            Vector3 outwardNormal,
            params Vector3[] points)
        {
            return CreateOrientedFace(
                outwardNormal,
                PolygonFaceFeature.Base,
                0f,
                points);
        }

        private static PolygonFace CreateOrientedFace(
            Vector3 outwardNormal,
            PolygonFaceFeature feature,
            float featureStrength,
            params Vector3[] points)
        {
            List<Vector3> ordered = new List<Vector3>(points);
            Vector3 centre = CalculateAverage(ordered);

            Vector3 tangent = Mathf.Abs(outwardNormal.y) < 0.9f
                ? Vector3.Cross(outwardNormal, Vector3.up).normalized
                : Vector3.Cross(outwardNormal, Vector3.right).normalized;

            Vector3 bitangent =
                Vector3.Cross(outwardNormal, tangent).normalized;

            ordered.Sort((left, right) =>
            {
                Vector3 leftOffset = left - centre;
                Vector3 rightOffset = right - centre;

                float leftAngle = Mathf.Atan2(
                    Vector3.Dot(leftOffset, bitangent),
                    Vector3.Dot(leftOffset, tangent));

                float rightAngle = Mathf.Atan2(
                    Vector3.Dot(rightOffset, bitangent),
                    Vector3.Dot(rightOffset, tangent));

                return leftAngle.CompareTo(rightAngle);
            });

            Vector3 calculatedNormal = CalculatePolygonNormal(ordered);

            if (Vector3.Dot(calculatedNormal, outwardNormal) < 0f)
            {
                ordered.Reverse();
            }

            return new PolygonFace(
                ordered,
                outwardNormal.normalized,
                feature,
                featureStrength);
        }

        private static Vector3 CalculatePolygonNormal(
            List<Vector3> vertices)
        {
            Vector3 normal = Vector3.zero;

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 current = vertices[i];
                Vector3 next = vertices[(i + 1) % vertices.Count];

                normal.x += (current.y - next.y) * (current.z + next.z);
                normal.y += (current.z - next.z) * (current.x + next.x);
                normal.z += (current.x - next.x) * (current.y + next.y);
            }

            float normalSqrMagnitude = normal.sqrMagnitude;

            return normalSqrMagnitude > MinimumEdgeLengthSqr
                ? normal / Mathf.Sqrt(normalSqrMagnitude)
                : Vector3.up;
        }

        private static TriangleSoup TriangulatePolyhedron(
            List<PolygonFace> faces,
            SurfaceFacetDensity density,
            EdgeCharacter edgeCharacter,
            int surfaceSeed)
        {
            TriangleSoup soup = new TriangleSoup();
            int edgeSegments = GetBoundarySegments(density);

            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace sourceFace = faces[faceIndex];

                List<Vector3> cleanFaceVertices = SanitizePolygon(
                    sourceFace.Vertices,
                    sourceFace.Normal);

                if (cleanFaceVertices.Count < 3 ||
                    CalculatePolygonArea(cleanFaceVertices) <= TinyFaceAreaEpsilon)
                {
                    continue;
                }

                PolygonFace face = new PolygonFace(
                    cleanFaceVertices,
                    sourceFace.Normal,
                    sourceFace.Feature,
                    sourceFace.FeatureStrength);

                if (density == SurfaceFacetDensity.Sparse ||
                    face.Feature == PolygonFaceFeature.ConvexEdgeWear)
                {
                    for (int i = 1; i < face.Vertices.Count - 1; i++)
                    {
                        AddOrientedTriangle(
                            soup,
                            face.Vertices[0],
                            face.Vertices[i],
                            face.Vertices[i + 1],
                            face.Normal,
                            face.Feature,
                            face.FeatureStrength);
                    }

                    continue;
                }

                List<Vector3> boundary = BuildSegmentedBoundary(
                    face.Vertices,
                    edgeSegments);

                Vector3 centre = CalculateAverage(boundary);
                float faceRadius = CalculateAverageRadius(boundary, centre);

                float relief = GetSurfaceRelief(density) * faceRadius;
                relief *= GetReliefMultiplier(edgeCharacter);

                float signedVariation =
                    HashSigned(surfaceSeed, faceIndex);

                centre += face.Normal * relief * signedVariation;

                for (int i = 0; i < boundary.Count; i++)
                {
                    Vector3 current = boundary[i];
                    Vector3 next = boundary[(i + 1) % boundary.Count];

                    AddOrientedTriangle(
                        soup,
                        centre,
                        current,
                        next,
                        face.Normal,
                        face.Feature,
                        face.FeatureStrength);
                }
            }

            return soup;
        }

private static List<Vector3> BuildSegmentedBoundary(
            List<Vector3> vertices,
            int segmentsPerEdge)
        {
            List<Vector3> result = new List<Vector3>();

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 start = vertices[i];
                Vector3 end = vertices[(i + 1) % vertices.Count];

                for (int segment = 0; segment < segmentsPerEdge; segment++)
                {
                    float t = segment / (float)segmentsPerEdge;
                    result.Add(Vector3.Lerp(start, end, t));
                }
            }

            return result;
        }

        #endregion
    }
}
