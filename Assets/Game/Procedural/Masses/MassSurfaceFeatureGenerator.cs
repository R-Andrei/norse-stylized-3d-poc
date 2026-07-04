using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public readonly struct MassSurfaceFeatureSettings
    {
        public MassSurfaceFeatureSettings(
            MassArchetype archetype,
            int surfaceSeed,
            float edgeWearAmount,
            float edgeWearWidth,
            float creaseAmount,
            float creaseWidth)
        {
            Archetype = archetype;
            SurfaceSeed = surfaceSeed;
            EdgeWearAmount = Mathf.Clamp(edgeWearAmount, 0f, 2f);
            EdgeWearWidth = Mathf.Clamp(edgeWearWidth, 0.25f, 2f);
            CreaseAmount = Mathf.Clamp(creaseAmount, 0f, 2f);
            CreaseWidth = Mathf.Clamp(creaseWidth, 0.25f, 2f);
        }

        public MassArchetype Archetype { get; }
        public int SurfaceSeed { get; }
        public float EdgeWearAmount { get; }
        public float EdgeWearWidth { get; }
        public float CreaseAmount { get; }
        public float CreaseWidth { get; }
    }

    /// <summary>
    /// Generates debug-only semantic line overlays for generated stone masses.
    /// Line features are represented as strips instead of interpolated scalar
    /// masks so they do not expose raw triangulation on coarse low-poly rocks.
    /// </summary>
    public static class MassSurfaceFeatureGenerator
    {
        private const float MinimumEdgeAngleDegrees = 24f;
        private const float MinimumCandidateHeight01 = 0.08f;
        private const float StripNormalOffsetFactor = 0.0035f;
        private const int MaximumEdgeWearSegments = 140;
        private const int MaximumCreaseSegments = 90;

        private readonly struct TriangleData
        {
            public TriangleData(int a, int b, int c, Vector3 normal, float area)
            {
                A = a;
                B = b;
                C = c;
                Normal = normal;
                Area = area;
            }

            public int A { get; }
            public int B { get; }
            public int C { get; }
            public Vector3 Normal { get; }
            public float Area { get; }
        }

        private readonly struct EdgeKey : IEquatable<EdgeKey>
        {
            public EdgeKey(int a, int b)
            {
                if (a < b)
                {
                    A = a;
                    B = b;
                }
                else
                {
                    A = b;
                    B = a;
                }
            }

            public int A { get; }
            public int B { get; }

            public bool Equals(EdgeKey other) => A == other.A && B == other.B;
            public override bool Equals(object obj) => obj is EdgeKey other && Equals(other);
            public override int GetHashCode()
            {
                unchecked
                {
                    return (A * 397) ^ B;
                }
            }
        }

        private sealed class EdgeRecord
        {
            public int TriangleA = -1;
            public int TriangleB = -1;

            public void AddTriangle(int triangleIndex)
            {
                if (TriangleA < 0)
                {
                    TriangleA = triangleIndex;
                    return;
                }

                if (TriangleB < 0 && TriangleA != triangleIndex)
                {
                    TriangleB = triangleIndex;
                }
            }
        }

        private readonly struct EdgeCandidate
        {
            public EdgeCandidate(Vector3 start, Vector3 end, Vector3 normal, float score)
            {
                Start = start;
                End = end;
                Normal = normal;
                Score = score;
            }

            public Vector3 Start { get; }
            public Vector3 End { get; }
            public Vector3 Normal { get; }
            public float Score { get; }
        }

        private readonly struct FaceCandidate
        {
            public FaceCandidate(TriangleData triangle, float score)
            {
                Triangle = triangle;
                Score = score;
            }

            public TriangleData Triangle { get; }
            public float Score { get; }
        }

        public static void Generate(Mesh sourceMesh, Mesh targetMesh, MassSurfaceFeatureSettings settings)
        {
            if (targetMesh == null)
            {
                return;
            }

            targetMesh.Clear();

            if (sourceMesh == null || sourceMesh.vertexCount == 0 ||
                sourceMesh.triangles == null || sourceMesh.triangles.Length < 3)
            {
                return;
            }

            Vector3[] vertices = sourceMesh.vertices;
            int[] triangles = sourceMesh.triangles;
            Bounds bounds = sourceMesh.bounds;
            float scale = Mathf.Max(0.0001f, Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z)));

            List<TriangleData> triangleData = BuildTriangleData(vertices, triangles);
            Dictionary<EdgeKey, EdgeRecord> edgeMap = BuildEdgeMap(triangles);

            List<Vector3> stripVertices = new();
            List<Vector3> stripNormals = new();
            List<Vector2> stripUv0 = new();
            List<Vector4> stripUv2 = new();
            List<Color> stripColors = new();
            List<int> stripTriangles = new();

            GenerateEdgeWearStrips(vertices, bounds, scale, triangleData, edgeMap, settings, stripVertices, stripNormals, stripUv0, stripUv2, stripColors, stripTriangles);
            GenerateCreaseStrips(vertices, bounds, scale, triangleData, settings, stripVertices, stripNormals, stripUv0, stripUv2, stripColors, stripTriangles);

            if (stripVertices.Count == 0)
            {
                return;
            }

            targetMesh.indexFormat = stripVertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            targetMesh.SetVertices(stripVertices);
            targetMesh.SetNormals(stripNormals);
            targetMesh.SetUVs(0, stripUv0);
            targetMesh.SetUVs(1, stripUv2);
            targetMesh.SetColors(stripColors);
            targetMesh.SetTriangles(stripTriangles, 0);
            targetMesh.RecalculateBounds();
        }

        private static List<TriangleData> BuildTriangleData(Vector3[] vertices, int[] triangles)
        {
            List<TriangleData> result = new(triangles.Length / 3);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int a = triangles[i];
                int b = triangles[i + 1];
                int c = triangles[i + 2];
                Vector3 ab = vertices[b] - vertices[a];
                Vector3 ac = vertices[c] - vertices[a];
                Vector3 cross = Vector3.Cross(ab, ac);
                float area = cross.magnitude * 0.5f;
                Vector3 normal = cross.sqrMagnitude > 0.0000001f ? cross.normalized : Vector3.up;
                result.Add(new TriangleData(a, b, c, normal, area));
            }

            return result;
        }

        private static Dictionary<EdgeKey, EdgeRecord> BuildEdgeMap(int[] triangles)
        {
            Dictionary<EdgeKey, EdgeRecord> edgeMap = new();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int triangleIndex = i / 3;
                AddEdge(edgeMap, triangles[i], triangles[i + 1], triangleIndex);
                AddEdge(edgeMap, triangles[i + 1], triangles[i + 2], triangleIndex);
                AddEdge(edgeMap, triangles[i + 2], triangles[i], triangleIndex);
            }

            return edgeMap;
        }

        private static void AddEdge(Dictionary<EdgeKey, EdgeRecord> edgeMap, int a, int b, int triangleIndex)
        {
            EdgeKey key = new(a, b);
            if (!edgeMap.TryGetValue(key, out EdgeRecord record))
            {
                record = new EdgeRecord();
                edgeMap.Add(key, record);
            }

            record.AddTriangle(triangleIndex);
        }

        private static void GenerateEdgeWearStrips(
            Vector3[] vertices,
            Bounds bounds,
            float scale,
            List<TriangleData> triangleData,
            Dictionary<EdgeKey, EdgeRecord> edgeMap,
            MassSurfaceFeatureSettings settings,
            List<Vector3> stripVertices,
            List<Vector3> stripNormals,
            List<Vector2> stripUv0,
            List<Vector4> stripUv2,
            List<Color> stripColors,
            List<int> stripTriangles)
        {
            if (settings.EdgeWearAmount <= 0.001f)
            {
                return;
            }

            List<EdgeCandidate> candidates = new();
            foreach (KeyValuePair<EdgeKey, EdgeRecord> pair in edgeMap)
            {
                EdgeRecord record = pair.Value;
                if (record.TriangleA < 0 || record.TriangleB < 0)
                {
                    continue;
                }

                TriangleData triangleA = triangleData[record.TriangleA];
                TriangleData triangleB = triangleData[record.TriangleB];
                float dot = Mathf.Clamp(Vector3.Dot(triangleA.Normal, triangleB.Normal), -1f, 1f);
                float angleDegrees = Mathf.Acos(dot) * Mathf.Rad2Deg;
                if (angleDegrees < MinimumEdgeAngleDegrees)
                {
                    continue;
                }

                Vector3 start = vertices[pair.Key.A];
                Vector3 end = vertices[pair.Key.B];
                float length = Vector3.Distance(start, end);
                if (length < scale * 0.035f)
                {
                    continue;
                }

                Vector3 midpoint = (start + end) * 0.5f;
                float height01 = Mathf.InverseLerp(bounds.min.y, bounds.max.y, midpoint.y);
                if (height01 < MinimumCandidateHeight01)
                {
                    continue;
                }

                Vector3 normal = (triangleA.Normal + triangleB.Normal).normalized;
                if (normal.sqrMagnitude < 0.000001f)
                {
                    normal = triangleA.Normal;
                }

                if (normal.y < -0.22f)
                {
                    continue;
                }

                float angleScore = Mathf.InverseLerp(24f, 92f, angleDegrees);
                float lengthScore = Mathf.Clamp01(length / (scale * 0.30f));
                float exposureScore = Mathf.Lerp(0.55f, 1.15f, Mathf.Clamp01(normal.y * 0.5f + 0.5f));
                float lowerPenalty = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.04f, 0.22f, height01));
                float score = angleScore * lengthScore * exposureScore * lowerPenalty;
                if (score <= 0.05f)
                {
                    continue;
                }

                candidates.Add(new EdgeCandidate(start, end, normal, score));
            }

            candidates.Sort((a, b) => b.Score.CompareTo(a.Score));
            int targetCount = Mathf.Clamp(Mathf.RoundToInt((18 + triangleData.Count * 0.17f) * settings.EdgeWearAmount), 0, MaximumEdgeWearSegments);
            float width = scale * 0.0065f * settings.EdgeWearWidth;
            float offset = scale * StripNormalOffsetFactor;
            int seed = settings.SurfaceSeed * 92821 + 101;
            int emitted = 0;

            for (int i = 0; i < candidates.Count && emitted < targetCount; i++)
            {
                EdgeCandidate candidate = candidates[i];
                float keep = Hash01(seed, i, 13);
                float keepThreshold = Mathf.Lerp(0.18f, 0.62f, Mathf.Clamp01(candidate.Score));
                if (keep > keepThreshold && emitted > targetCount * 0.35f)
                {
                    continue;
                }

                float startTrim = Mathf.Lerp(0.03f, 0.24f, Hash01(seed, i, 17));
                float endTrim = Mathf.Lerp(0.04f, 0.22f, Hash01(seed, i, 23));
                if (startTrim + endTrim > 0.72f)
                {
                    endTrim = 0.72f - startTrim;
                }

                AddStripSegment(
                    Vector3.Lerp(candidate.Start, candidate.End, startTrim),
                    Vector3.Lerp(candidate.Start, candidate.End, 1f - endTrim),
                    candidate.Normal,
                    width * Mathf.Lerp(0.72f, 1.18f, Hash01(seed, i, 29)),
                    offset,
                    new Color(0f, 0f, 0f, 1f),
                    Vector4.zero,
                    stripVertices,
                    stripNormals,
                    stripUv0,
                    stripUv2,
                    stripColors,
                    stripTriangles);
                emitted++;
            }
        }

        private static void GenerateCreaseStrips(
            Vector3[] vertices,
            Bounds bounds,
            float scale,
            List<TriangleData> triangleData,
            MassSurfaceFeatureSettings settings,
            List<Vector3> stripVertices,
            List<Vector3> stripNormals,
            List<Vector2> stripUv0,
            List<Vector4> stripUv2,
            List<Color> stripColors,
            List<int> stripTriangles)
        {
            if (settings.CreaseAmount <= 0.001f)
            {
                return;
            }

            List<FaceCandidate> candidates = new();
            float minimumArea = scale * scale * 0.0035f;
            for (int i = 0; i < triangleData.Count; i++)
            {
                TriangleData triangle = triangleData[i];
                if (triangle.Area < minimumArea)
                {
                    continue;
                }

                Vector3 center = (vertices[triangle.A] + vertices[triangle.B] + vertices[triangle.C]) / 3f;
                float height01 = Mathf.InverseLerp(bounds.min.y, bounds.max.y, center.y);
                if (height01 < 0.12f || height01 > 0.94f)
                {
                    continue;
                }

                if (triangle.Normal.y < -0.35f)
                {
                    continue;
                }

                float areaScore = Mathf.Clamp01(triangle.Area / (scale * scale * 0.055f));
                float sideScore = Mathf.Lerp(1.12f, 0.70f, Mathf.Clamp01(triangle.Normal.y));
                float heightScore = 1f - Mathf.Abs(height01 - 0.48f) * 0.55f;
                float archetypeBias = ResolveCreaseArchetypeBias(settings.Archetype);
                candidates.Add(new FaceCandidate(triangle, areaScore * sideScore * heightScore * archetypeBias));
            }

            candidates.Sort((a, b) => b.Score.CompareTo(a.Score));
            int targetCount = Mathf.Clamp(Mathf.RoundToInt((2 + triangleData.Count * 0.035f) * settings.CreaseAmount * ResolveCreaseArchetypeBias(settings.Archetype)), 0, Mathf.Max(1, Mathf.RoundToInt(MaximumCreaseSegments * settings.CreaseAmount)));
            float width = scale * 0.0046f * settings.CreaseWidth;
            float offset = scale * StripNormalOffsetFactor * 1.10f;
            int seed = settings.SurfaceSeed * 15731 + 701;
            int emitted = 0;

            for (int i = 0; i < candidates.Count && emitted < targetCount; i++)
            {
                FaceCandidate candidate = candidates[i];
                if (Hash01(seed, i, 41) > Mathf.Clamp01(candidate.Score * 0.95f + 0.20f) && emitted > targetCount * 0.35f)
                {
                    continue;
                }

                AddJaggedCrack(
                    vertices[candidate.Triangle.A],
                    vertices[candidate.Triangle.B],
                    vertices[candidate.Triangle.C],
                    candidate.Triangle.Normal,
                    width * Mathf.Lerp(0.75f, 1.20f, Hash01(seed, i, 43)),
                    offset,
                    seed,
                    i,
                    stripVertices,
                    stripNormals,
                    stripUv0,
                    stripUv2,
                    stripColors,
                    stripTriangles);
                emitted++;
            }
        }

        private static float ResolveCreaseArchetypeBias(MassArchetype archetype)
        {
            return archetype switch
            {
                MassArchetype.FracturedPillar => 1.45f,
                MassArchetype.BrokenChunk => 1.35f,
                MassArchetype.CarvedMarkerStone => 1.20f,
                MassArchetype.LayeredStone => 1.15f,
                MassArchetype.PolishedStone => 0.65f,
                MassArchetype.FlatSlab => 0.80f,
                _ => 1.0f
            };
        }

        private static void AddJaggedCrack(
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 normal,
            float width,
            float offset,
            int seed,
            int index,
            List<Vector3> stripVertices,
            List<Vector3> stripNormals,
            List<Vector2> stripUv0,
            List<Vector4> stripUv2,
            List<Color> stripColors,
            List<int> stripTriangles)
        {
            Vector3 p0 = Vector3.Lerp(a, b, Mathf.Lerp(0.18f, 0.82f, Hash01(seed, index, 53)));
            Vector3 p3 = Vector3.Lerp(c, Vector3.Lerp(a, b, Hash01(seed, index, 59)), Mathf.Lerp(0.10f, 0.32f, Hash01(seed, index, 61)));
            Vector3 direction = (p3 - p0).normalized;
            if (direction.sqrMagnitude < 0.000001f)
            {
                return;
            }

            Vector3 lateral = Vector3.Cross(normal, direction).normalized;
            if (lateral.sqrMagnitude < 0.000001f)
            {
                return;
            }

            Vector3 p1 = Vector3.Lerp(p0, p3, 0.34f) + lateral * width * Mathf.Lerp(-3.2f, 3.2f, Hash01(seed, index, 67));
            Vector3 p2 = Vector3.Lerp(p0, p3, 0.67f) + lateral * width * Mathf.Lerp(-3.4f, 3.4f, Hash01(seed, index, 71));
            Vector3[] points = { p0, p1, p2, p3 };
            for (int i = 0; i < points.Length - 1; i++)
            {
                AddStripSegment(
                    points[i],
                    points[i + 1],
                    normal,
                    width * Mathf.Lerp(0.76f, 1.08f, Hash01(seed, index, 83 + i)),
                    offset,
                    new Color(0f, 0f, 0f, 0f),
                    new Vector4(1f, 0f, 0f, 0f),
                    stripVertices,
                    stripNormals,
                    stripUv0,
                    stripUv2,
                    stripColors,
                    stripTriangles);
            }
        }

        private static void AddStripSegment(
            Vector3 start,
            Vector3 end,
            Vector3 normal,
            float width,
            float offset,
            Color color,
            Vector4 uv2,
            List<Vector3> stripVertices,
            List<Vector3> stripNormals,
            List<Vector2> stripUv0,
            List<Vector4> stripUv2,
            List<Color> stripColors,
            List<int> stripTriangles)
        {
            Vector3 direction = end - start;
            if (direction.sqrMagnitude < 0.000001f)
            {
                return;
            }

            direction.Normalize();
            normal = normal.sqrMagnitude > 0.000001f ? normal.normalized : Vector3.up;
            Vector3 across = Vector3.Cross(normal, direction);
            if (across.sqrMagnitude < 0.000001f)
            {
                across = Vector3.Cross(Vector3.up, direction);
            }

            if (across.sqrMagnitude < 0.000001f)
            {
                across = Vector3.right;
            }

            across.Normalize();
            Vector3 lift = normal * offset;
            Vector3 halfWidth = across * width;
            int baseIndex = stripVertices.Count;
            stripVertices.Add(start - halfWidth + lift);
            stripVertices.Add(start + halfWidth + lift);
            stripVertices.Add(end + halfWidth + lift);
            stripVertices.Add(end - halfWidth + lift);

            for (int i = 0; i < 4; i++)
            {
                stripNormals.Add(normal);
                stripColors.Add(color);
                stripUv2.Add(uv2);
            }

            stripUv0.Add(new Vector2(0f, 0f));
            stripUv0.Add(new Vector2(0f, 1f));
            stripUv0.Add(new Vector2(1f, 1f));
            stripUv0.Add(new Vector2(1f, 0f));

            stripTriangles.Add(baseIndex);
            stripTriangles.Add(baseIndex + 1);
            stripTriangles.Add(baseIndex + 2);
            stripTriangles.Add(baseIndex);
            stripTriangles.Add(baseIndex + 2);
            stripTriangles.Add(baseIndex + 3);
            stripTriangles.Add(baseIndex);
            stripTriangles.Add(baseIndex + 2);
            stripTriangles.Add(baseIndex + 1);
            stripTriangles.Add(baseIndex);
            stripTriangles.Add(baseIndex + 3);
            stripTriangles.Add(baseIndex + 2);
        }

        private static float Hash01(int seed, int a, int b)
        {
            unchecked
            {
                uint hash = (uint)seed;
                hash ^= (uint)(a + 0x9E3779B9) + (hash << 6) + (hash >> 2);
                hash ^= (uint)(b + 0x85EBCA6B) + (hash << 6) + (hash >> 2);
                hash ^= hash >> 16;
                hash *= 0x7FEB352D;
                hash ^= hash >> 15;
                hash *= 0x846CA68B;
                hash ^= hash >> 16;
                return (hash & 0x00FFFFFF) / 16777215f;
            }
        }
    }
}
