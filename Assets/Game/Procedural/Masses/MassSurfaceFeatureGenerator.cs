using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public enum MassSurfaceFeatureKind
    {
        EdgeWear,
        Crease
    }

    public readonly struct MassSurfaceFeatureSettings
    {
        public MassSurfaceFeatureSettings(
            MassArchetype archetype,
            int surfaceSeed,
            float edgeWearAmount,
            float edgeWearWidth,
            float edgeWearCoverage,
            float creaseAmount,
            float creaseWidth,
            float creaseLength,
            float creaseBranching)
        {
            Archetype = archetype;
            SurfaceSeed = surfaceSeed;
            EdgeWearAmount = Mathf.Clamp(edgeWearAmount, 0f, 2f);
            EdgeWearWidth = Mathf.Clamp(edgeWearWidth, 0.25f, 2f);
            EdgeWearCoverage = Mathf.Clamp(edgeWearCoverage, 0.1f, 2f);
            CreaseAmount = Mathf.Clamp(creaseAmount, 0f, 2f);
            CreaseWidth = Mathf.Clamp(creaseWidth, 0.25f, 2f);
            CreaseLength = Mathf.Clamp(creaseLength, 0.25f, 2f);
            CreaseBranching = Mathf.Clamp(creaseBranching, 0f, 2f);
        }

        public MassArchetype Archetype { get; }
        public int SurfaceSeed { get; }
        public float EdgeWearAmount { get; }
        public float EdgeWearWidth { get; }
        public float EdgeWearCoverage { get; }
        public float CreaseAmount { get; }
        public float CreaseWidth { get; }
        public float CreaseLength { get; }
        public float CreaseBranching { get; }
    }

    /// <summary>
    /// Generates debug-only semantic line overlays for generated stone masses.
    /// Line features are represented as strips instead of interpolated scalar
    /// masks so they do not expose raw triangulation on coarse low-poly rocks.
    /// </summary>
    public static class MassSurfaceFeatureGenerator
    {
        private const float MinimumEdgeAngleDegrees = 15f;
        private const float MinimumCandidateHeight01 = 0.07f;
        private const float StripNormalOffsetFactor = 0.0035f;
        private const int MaximumEdgeWearSegments = 360;
        private const int MaximumCreasePaths = 48;
        private const int PositionQuantization = 10000;

        private readonly struct PositionKey : IEquatable<PositionKey>
        {
            public PositionKey(Vector3 position)
            {
                X = Mathf.RoundToInt(position.x * PositionQuantization);
                Y = Mathf.RoundToInt(position.y * PositionQuantization);
                Z = Mathf.RoundToInt(position.z * PositionQuantization);
            }

            public int X { get; }
            public int Y { get; }
            public int Z { get; }

            public int CompareTo(PositionKey other)
            {
                int x = X.CompareTo(other.X);
                if (x != 0)
                {
                    return x;
                }

                int y = Y.CompareTo(other.Y);
                if (y != 0)
                {
                    return y;
                }

                return Z.CompareTo(other.Z);
            }

            public bool Equals(PositionKey other) =>
                X == other.X &&
                Y == other.Y &&
                Z == other.Z;

            public override bool Equals(object obj) =>
                obj is PositionKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = X;
                    hash = (hash * 397) ^ Y;
                    hash = (hash * 397) ^ Z;
                    return hash;
                }
            }
        }

        private readonly struct EdgeKey : IEquatable<EdgeKey>
        {
            public EdgeKey(Vector3 a, Vector3 b)
            {
                PositionKey keyA = new(a);
                PositionKey keyB = new(b);
                if (keyA.CompareTo(keyB) <= 0)
                {
                    A = keyA;
                    B = keyB;
                    Start = a;
                    End = b;
                }
                else
                {
                    A = keyB;
                    B = keyA;
                    Start = b;
                    End = a;
                }
            }

            public PositionKey A { get; }
            public PositionKey B { get; }
            public Vector3 Start { get; }
            public Vector3 End { get; }

            public bool Equals(EdgeKey other) => A.Equals(other.A) && B.Equals(other.B);
            public override bool Equals(object obj) => obj is EdgeKey other && Equals(other);
            public override int GetHashCode()
            {
                unchecked
                {
                    return (A.GetHashCode() * 397) ^ B.GetHashCode();
                }
            }
        }

        private readonly struct TriangleData
        {
            public TriangleData(
                int a,
                int b,
                int c,
                Vector3 normal,
                float area,
                Vector3 center)
            {
                A = a;
                B = b;
                C = c;
                Normal = normal;
                Area = area;
                Center = center;
            }

            public int A { get; }
            public int B { get; }
            public int C { get; }
            public Vector3 Normal { get; }
            public float Area { get; }
            public Vector3 Center { get; }
        }

        private sealed class EdgeRecord
        {
            public int TriangleA = -1;
            public int TriangleB = -1;
            public Vector3 Start;
            public Vector3 End;

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
            public EdgeCandidate(
                Vector3 start,
                Vector3 end,
                Vector3 normal,
                Vector3 midpoint,
                float score,
                int bucket)
            {
                Start = start;
                End = end;
                StartKey = new PositionKey(start);
                EndKey = new PositionKey(end);
                Normal = normal;
                Midpoint = midpoint;
                Score = score;
                Bucket = bucket;
            }

            public Vector3 Start { get; }
            public Vector3 End { get; }
            public PositionKey StartKey { get; }
            public PositionKey EndKey { get; }
            public Vector3 Normal { get; }
            public Vector3 Midpoint { get; }
            public float Score { get; }
            public int Bucket { get; }
        }

        private readonly struct EdgeChainSegment
        {
            public EdgeChainSegment(EdgeCandidate candidate, bool reversed)
            {
                Candidate = candidate;
                Reversed = reversed;
            }

            public EdgeCandidate Candidate { get; }
            public bool Reversed { get; }

            public Vector3 Start => Reversed ? Candidate.End : Candidate.Start;
            public Vector3 End => Reversed ? Candidate.Start : Candidate.End;
            public PositionKey StartKey => Reversed ? Candidate.EndKey : Candidate.StartKey;
            public PositionKey EndKey => Reversed ? Candidate.StartKey : Candidate.EndKey;
            public Vector3 Normal => Candidate.Normal;
            public float Score => Candidate.Score;
            public int Bucket => Candidate.Bucket;

            public Vector3 Direction
            {
                get
                {
                    Vector3 direction = End - Start;
                    return direction.sqrMagnitude > 0.000001f
                        ? direction.normalized
                        : Vector3.forward;
                }
            }
        }

        private sealed class EdgeChain
        {
            public EdgeChain()
            {
                Segments = new List<EdgeChainSegment>();
            }

            public List<EdgeChainSegment> Segments { get; }
            public float Score { get; set; }
            public float Length { get; set; }
            public int Bucket { get; set; }
        }

        private readonly struct FaceCandidate
        {
            public FaceCandidate(TriangleData triangle, float score, int bucket)
            {
                Triangle = triangle;
                Score = score;
                Bucket = bucket;
            }

            public TriangleData Triangle { get; }
            public float Score { get; }
            public int Bucket { get; }
        }

        public static void Generate(
            Mesh sourceMesh,
            Mesh targetMesh,
            MassSurfaceFeatureSettings settings,
            MassSurfaceFeatureKind kind)
        {
            if (targetMesh == null)
            {
                return;
            }

            targetMesh.Clear();

            if (sourceMesh == null ||
                sourceMesh.vertexCount == 0 ||
                sourceMesh.triangles == null ||
                sourceMesh.triangles.Length < 3)
            {
                return;
            }

            Vector3[] vertices = sourceMesh.vertices;
            int[] triangles = sourceMesh.triangles;
            Bounds bounds = sourceMesh.bounds;
            float scale = Mathf.Max(
                0.0001f,
                Mathf.Max(
                    bounds.size.x,
                    Mathf.Max(bounds.size.y, bounds.size.z)));

            List<TriangleData> triangleData =
                BuildTriangleData(vertices, triangles);
            Dictionary<EdgeKey, EdgeRecord> edgeMap =
                BuildEdgeMap(vertices, triangles);

            List<Vector3> stripVertices = new();
            List<Vector3> stripNormals = new();
            List<Vector2> stripUv0 = new();
            List<Vector4> stripUv2 = new();
            List<Color> stripColors = new();
            List<int> stripTriangles = new();

            if (kind == MassSurfaceFeatureKind.EdgeWear)
            {
                GenerateEdgeWearStrips(
                    vertices,
                    bounds,
                    scale,
                    triangleData,
                    edgeMap,
                    settings,
                    stripVertices,
                    stripNormals,
                    stripUv0,
                    stripUv2,
                    stripColors,
                    stripTriangles);
            }
            else
            {
                GenerateCreaseStrips(
                    vertices,
                    bounds,
                    scale,
                    triangleData,
                    settings,
                    stripVertices,
                    stripNormals,
                    stripUv0,
                    stripUv2,
                    stripColors,
                    stripTriangles);
            }

            if (stripVertices.Count == 0)
            {
                return;
            }

            targetMesh.indexFormat =
                stripVertices.Count > 65000
                    ? IndexFormat.UInt32
                    : IndexFormat.UInt16;
            targetMesh.SetVertices(stripVertices);
            targetMesh.SetNormals(stripNormals);
            targetMesh.SetUVs(0, stripUv0);
            targetMesh.SetUVs(1, stripUv2);
            targetMesh.SetColors(stripColors);
            targetMesh.SetTriangles(stripTriangles, 0);
            targetMesh.RecalculateBounds();
        }

        private static List<TriangleData> BuildTriangleData(
            Vector3[] vertices,
            int[] triangles)
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
                Vector3 normal =
                    cross.sqrMagnitude > 0.0000001f
                        ? cross.normalized
                        : Vector3.up;
                Vector3 center =
                    (vertices[a] + vertices[b] + vertices[c]) / 3f;
                result.Add(new TriangleData(a, b, c, normal, area, center));
            }

            return result;
        }

        private static Dictionary<EdgeKey, EdgeRecord> BuildEdgeMap(
            Vector3[] vertices,
            int[] triangles)
        {
            Dictionary<EdgeKey, EdgeRecord> edgeMap = new();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int triangleIndex = i / 3;
                AddEdge(
                    edgeMap,
                    vertices[triangles[i]],
                    vertices[triangles[i + 1]],
                    triangleIndex);
                AddEdge(
                    edgeMap,
                    vertices[triangles[i + 1]],
                    vertices[triangles[i + 2]],
                    triangleIndex);
                AddEdge(
                    edgeMap,
                    vertices[triangles[i + 2]],
                    vertices[triangles[i]],
                    triangleIndex);
            }

            return edgeMap;
        }

        private static void AddEdge(
            Dictionary<EdgeKey, EdgeRecord> edgeMap,
            Vector3 a,
            Vector3 b,
            int triangleIndex)
        {
            EdgeKey key = new(a, b);
            if (!edgeMap.TryGetValue(key, out EdgeRecord record))
            {
                record = new EdgeRecord
                {
                    Start = key.Start,
                    End = key.End
                };
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

                float dot = Mathf.Clamp(
                    Vector3.Dot(triangleA.Normal, triangleB.Normal),
                    -1f,
                    1f);
                float angleDegrees = Mathf.Acos(dot) * Mathf.Rad2Deg;
                if (angleDegrees < MinimumEdgeAngleDegrees)
                {
                    continue;
                }

                Vector3 start = record.Start;
                Vector3 end = record.End;
                float length = Vector3.Distance(start, end);
                if (length < scale * 0.026f)
                {
                    continue;
                }

                Vector3 midpoint = (start + end) * 0.5f;
                float height01 = Mathf.InverseLerp(
                    bounds.min.y,
                    bounds.max.y,
                    midpoint.y);
                if (height01 < MinimumCandidateHeight01)
                {
                    continue;
                }

                Vector3 normal =
                    (triangleA.Normal + triangleB.Normal).normalized;
                if (normal.sqrMagnitude < 0.000001f)
                {
                    normal = triangleA.Normal;
                }

                if (normal.y < -0.42f)
                {
                    continue;
                }

                float angleScore = Mathf.InverseLerp(15f, 100f, angleDegrees);
                float lengthScore = Mathf.Clamp01(length / (scale * 0.34f));
                float exposureScore =
                    Mathf.Lerp(
                        0.65f,
                        1.20f,
                        Mathf.Clamp01(normal.y * 0.5f + 0.5f));
                float lowerPenalty =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        Mathf.InverseLerp(0.035f, 0.20f, height01));
                float score =
                    angleScore *
                    lengthScore *
                    exposureScore *
                    lowerPenalty;

                bool strongGeometricRidge =
                    angleDegrees >= 34f ||
                    length >= scale * 0.105f ||
                    (normal.y > 0.18f && angleDegrees >= 20f);
                bool weakButVisibleRidge =
                    angleDegrees >= 22f &&
                    length >= scale * 0.050f &&
                    height01 >= 0.055f;

                if (!strongGeometricRidge &&
                    !weakButVisibleRidge &&
                    score <= 0.030f)
                {
                    continue;
                }

                score = Mathf.Max(
                    score,
                    strongGeometricRidge
                        ? 0.115f
                        : weakButVisibleRidge
                            ? 0.065f
                            : 0.032f);

                candidates.Add(
                    new EdgeCandidate(
                        start,
                        end,
                        normal,
                        midpoint,
                        score,
                        ResolveSpatialBucket(midpoint, bounds, normal)));
            }

            int seed = settings.SurfaceSeed * 92821 + 101;
            List<EdgeChain> chains = BuildEdgeWearChains(candidates, seed);
            List<EdgeChain> selected =
                SelectDistributedEdgeChainsByAmount(
                    chains,
                    settings.EdgeWearAmount,
                    seed);

            float width = scale * 0.0065f * settings.EdgeWearWidth;
            float offset = scale * StripNormalOffsetFactor;

            for (int i = 0; i < selected.Count; i++)
            {
                AddEdgeWearChainCoverageSegments(
                    selected[i],
                    settings.EdgeWearCoverage,
                    width * Mathf.Lerp(0.72f, 1.16f, Hash01(seed, i, 29)),
                    offset,
                    seed,
                    i,
                    stripVertices,
                    stripNormals,
                    stripUv0,
                    stripUv2,
                    stripColors,
                    stripTriangles);
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
            float archetypeBias = ResolveCreaseArchetypeBias(settings.Archetype);
            for (int i = 0; i < triangleData.Count; i++)
            {
                TriangleData triangle = triangleData[i];
                if (triangle.Area < minimumArea)
                {
                    continue;
                }

                float height01 = Mathf.InverseLerp(
                    bounds.min.y,
                    bounds.max.y,
                    triangle.Center.y);
                if (height01 < 0.12f || height01 > 0.94f)
                {
                    continue;
                }

                if (triangle.Normal.y < -0.35f)
                {
                    continue;
                }

                float areaScore = Mathf.Clamp01(
                    triangle.Area / (scale * scale * 0.055f));
                float sideScore =
                    Mathf.Lerp(1.10f, 0.72f, Mathf.Clamp01(triangle.Normal.y));
                float heightScore = 1f - Mathf.Abs(height01 - 0.50f) * 0.48f;
                float score = areaScore * sideScore * heightScore * archetypeBias;

                candidates.Add(
                    new FaceCandidate(
                        triangle,
                        score,
                        ResolveSpatialBucket(
                            triangle.Center,
                            bounds,
                            triangle.Normal)));
            }

            int targetCount = Mathf.Clamp(
                Mathf.RoundToInt(
                    (4 + triangleData.Count * 0.035f) *
                    settings.CreaseAmount *
                    archetypeBias),
                0,
                Mathf.Max(
                    1,
                    Mathf.RoundToInt(
                        MaximumCreasePaths *
                        settings.CreaseAmount *
                        archetypeBias)));
            List<FaceCandidate> selected =
                SelectDistributedFacesBySector(
                    candidates,
                    targetCount,
                    settings.SurfaceSeed * 15731 + 701);

            float width = scale * 0.0046f * settings.CreaseWidth;
            float offset = scale * StripNormalOffsetFactor * 1.10f;
            int seed = settings.SurfaceSeed * 15731 + 701;

            for (int i = 0; i < selected.Count; i++)
            {
                FaceCandidate candidate = selected[i];
                AddJaggedCrack(
                    vertices[candidate.Triangle.A],
                    vertices[candidate.Triangle.B],
                    vertices[candidate.Triangle.C],
                    candidate.Triangle.Normal,
                    width * Mathf.Lerp(0.76f, 1.18f, Hash01(seed, i, 43)),
                    offset,
                    settings.CreaseLength,
                    settings.CreaseBranching,
                    seed,
                    i,
                    stripVertices,
                    stripNormals,
                    stripUv0,
                    stripUv2,
                    stripColors,
                    stripTriangles);
            }
        }

        private static List<EdgeChain> BuildEdgeWearChains(
            List<EdgeCandidate> candidates,
            int seed)
        {
            List<EdgeChain> result = new();
            if (candidates.Count == 0)
            {
                return result;
            }

            Dictionary<PositionKey, List<int>> endpointMap = new();
            for (int i = 0; i < candidates.Count; i++)
            {
                AddEndpointIndex(endpointMap, candidates[i].StartKey, i);
                AddEndpointIndex(endpointMap, candidates[i].EndKey, i);
            }

            List<int> order = new();
            for (int i = 0; i < candidates.Count; i++)
            {
                order.Add(i);
            }

            order.Sort((a, b) => candidates[b].Score.CompareTo(candidates[a].Score));

            bool[] used = new bool[candidates.Count];
            for (int i = 0; i < order.Count; i++)
            {
                int seedIndex = order[i];
                if (used[seedIndex])
                {
                    continue;
                }

                EdgeChain chain = new();
                EdgeChainSegment seedSegment =
                    new EdgeChainSegment(candidates[seedIndex], false);
                chain.Segments.Add(seedSegment);
                used[seedIndex] = true;

                ExtendEdgeChain(
                    chain,
                    true,
                    candidates,
                    endpointMap,
                    used);
                ExtendEdgeChain(
                    chain,
                    false,
                    candidates,
                    endpointMap,
                    used);

                ResolveEdgeChainScore(chain);
                if (chain.Segments.Count > 0)
                {
                    result.Add(chain);
                }
            }

            result.Sort(
                (a, b) =>
                {
                    float aScore =
                        a.Score *
                        Mathf.Lerp(0.94f, 1.06f, Hash01(seed, a.Bucket, 11));
                    float bScore =
                        b.Score *
                        Mathf.Lerp(0.94f, 1.06f, Hash01(seed, b.Bucket, 11));
                    return bScore.CompareTo(aScore);
                });
            return result;
        }

        private static void AddEndpointIndex(
            Dictionary<PositionKey, List<int>> endpointMap,
            PositionKey key,
            int index)
        {
            if (!endpointMap.TryGetValue(key, out List<int> list))
            {
                list = new List<int>();
                endpointMap.Add(key, list);
            }

            list.Add(index);
        }

        private static void ExtendEdgeChain(
            EdgeChain chain,
            bool append,
            List<EdgeCandidate> candidates,
            Dictionary<PositionKey, List<int>> endpointMap,
            bool[] used)
        {
            const int MaximumChainSegments = 12;
            while (chain.Segments.Count < MaximumChainSegments)
            {
                EdgeChainSegment endSegment =
                    append
                        ? chain.Segments[chain.Segments.Count - 1]
                        : chain.Segments[0];
                PositionKey endpoint =
                    append
                        ? endSegment.EndKey
                        : endSegment.StartKey;
                Vector3 desiredDirection =
                    append
                        ? endSegment.Direction
                        : -endSegment.Direction;
                Vector3 desiredNormal = endSegment.Normal;

                if (!endpointMap.TryGetValue(endpoint, out List<int> neighborIndices))
                {
                    return;
                }

                int bestIndex = -1;
                bool bestReversed = false;
                float bestScore = -1f;

                for (int i = 0; i < neighborIndices.Count; i++)
                {
                    int candidateIndex = neighborIndices[i];
                    if (used[candidateIndex])
                    {
                        continue;
                    }

                    EdgeCandidate candidate = candidates[candidateIndex];
                    bool connectsFromStart = candidate.StartKey.Equals(endpoint);
                    bool connectsFromEnd = candidate.EndKey.Equals(endpoint);
                    if (!connectsFromStart && !connectsFromEnd)
                    {
                        continue;
                    }

                    bool reversed =
                        append
                            ? connectsFromEnd
                            : connectsFromStart;
                    EdgeChainSegment candidateSegment =
                        new EdgeChainSegment(candidate, reversed);
                    Vector3 candidateDirection =
                        append
                            ? candidateSegment.Direction
                            : -candidateSegment.Direction;
                    float directionDot =
                        Vector3.Dot(desiredDirection, candidateDirection);
                    if (directionDot < -0.30f)
                    {
                        continue;
                    }

                    float normalDot =
                        Vector3.Dot(desiredNormal.normalized, candidate.Normal.normalized);
                    if (normalDot < -0.10f)
                    {
                        continue;
                    }

                    float turnPreference =
                        directionDot > 0.25f
                            ? 0.46f
                            : 0.24f;
                    float score =
                        candidate.Score * 1.40f +
                        Mathf.Clamp01(directionDot * 0.5f + 0.5f) * turnPreference +
                        Mathf.Clamp01(normalDot * 0.5f + 0.5f) * 0.22f;
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestIndex = candidateIndex;
                        bestReversed = reversed;
                    }
                }

                if (bestIndex < 0)
                {
                    return;
                }

                EdgeChainSegment bestSegment =
                    new EdgeChainSegment(candidates[bestIndex], bestReversed);
                if (append)
                {
                    chain.Segments.Add(bestSegment);
                }
                else
                {
                    chain.Segments.Insert(0, bestSegment);
                }

                used[bestIndex] = true;
            }
        }

        private static void ResolveEdgeChainScore(EdgeChain chain)
        {
            float weightedScore = 0f;
            float length = 0f;
            Vector3 center = Vector3.zero;

            for (int i = 0; i < chain.Segments.Count; i++)
            {
                EdgeChainSegment segment = chain.Segments[i];
                float segmentLength = Vector3.Distance(segment.Start, segment.End);
                length += segmentLength;
                weightedScore += segment.Score * Mathf.Max(0.0001f, segmentLength);
                center += segment.Candidate.Midpoint;
            }

            chain.Length = length;
            chain.Score =
                length > 0.0001f
                    ? weightedScore / length
                    : chain.Segments[0].Score;
            chain.Score *= Mathf.Lerp(1.0f, 1.25f, Mathf.Clamp01(chain.Segments.Count / 4.0f));
            center /= Mathf.Max(1, chain.Segments.Count);

            chain.Bucket = chain.Segments[0].Bucket;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < chain.Segments.Count; i++)
            {
                float distance =
                    Vector3.SqrMagnitude(chain.Segments[i].Candidate.Midpoint - center);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    chain.Bucket = chain.Segments[i].Bucket;
                }
            }
        }

        private static List<EdgeChain> SelectDistributedEdgeChainsByAmount(
            List<EdgeChain> chains,
            float edgeWearAmount,
            int seed)
        {
            List<EdgeChain> selected = new();
            if (edgeWearAmount <= 0.001f || chains.Count == 0)
            {
                return selected;
            }

            float amount01 = Mathf.Clamp01(edgeWearAmount * 0.5f);
            float scoreThreshold =
                amount01 >= 0.985f
                    ? 0.0f
                    : Mathf.Lerp(0.72f, 0.045f, amount01);

            List<EdgeChain> valid = new();
            for (int i = 0; i < chains.Count; i++)
            {
                if (chains[i].Score >= scoreThreshold)
                {
                    valid.Add(chains[i]);
                }
            }

            if (valid.Count == 0)
            {
                return selected;
            }

            int targetCount =
                amount01 >= 0.985f
                    ? valid.Count
                    : Mathf.RoundToInt(
                        Mathf.Lerp(
                            Mathf.Min(2, valid.Count),
                            valid.Count,
                            amount01));
            targetCount = Mathf.Clamp(targetCount, 0, valid.Count);

            Dictionary<int, List<EdgeChain>> byBucket = new();
            for (int i = 0; i < valid.Count; i++)
            {
                EdgeChain chain = valid[i];
                if (!byBucket.TryGetValue(chain.Bucket, out List<EdgeChain> bucket))
                {
                    bucket = new List<EdgeChain>();
                    byBucket.Add(chain.Bucket, bucket);
                }

                bucket.Add(chain);
            }

            foreach (List<EdgeChain> bucket in byBucket.Values)
            {
                bucket.Sort((a, b) => b.Score.CompareTo(a.Score));
            }

            List<int> buckets = new List<int>(byBucket.Keys);
            buckets.Sort(
                (a, b) =>
                    Hash01(seed, a, 71).CompareTo(Hash01(seed, b, 71)));

            int pass = 0;
            while (selected.Count < targetCount)
            {
                bool added = false;
                for (int i = 0; i < buckets.Count && selected.Count < targetCount; i++)
                {
                    List<EdgeChain> bucket = byBucket[buckets[i]];
                    if (pass >= bucket.Count)
                    {
                        continue;
                    }

                    selected.Add(bucket[pass]);
                    added = true;
                }

                if (!added)
                {
                    break;
                }

                pass++;
            }

            return selected;
        }

        private static void AddEdgeWearChainCoverageSegments(
            EdgeChain chain,
            float edgeWearCoverage,
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
            float coverage01 = Mathf.InverseLerp(0.1f, 2f, edgeWearCoverage);
            if (coverage01 >= 0.82f)
            {
                for (int i = 0; i < chain.Segments.Count; i++)
                {
                    EdgeChainSegment segment = chain.Segments[i];
                    float segmentLength =
                        Mathf.Max(
                            0.0001f,
                            Vector3.Distance(segment.Start, segment.End));
                    float overlap01 =
                        Mathf.Clamp(
                            width * 0.65f / segmentLength,
                            0.0f,
                            0.035f);
                    bool overlapStart =
                        i > 0 &&
                        Vector3.Dot(
                            chain.Segments[i - 1].Direction,
                            segment.Direction) > 0.35f;
                    bool overlapEnd =
                        i < chain.Segments.Count - 1 &&
                        Vector3.Dot(
                            segment.Direction,
                            chain.Segments[i + 1].Direction) > 0.35f;
                    float start01 = overlapStart ? -overlap01 : 0.0f;
                    float end01 = overlapEnd ? 1.0f + overlap01 : 1.0f;

                    AddStripSegment(
                        Vector3.LerpUnclamped(segment.Start, segment.End, start01),
                        Vector3.LerpUnclamped(segment.Start, segment.End, end01),
                        segment.Normal,
                        width,
                        offset,
                        new Color(0f, 0f, 0f, 1f),
                        Vector4.zero,
                        stripVertices,
                        stripNormals,
                        stripUv0,
                        stripUv2,
                        stripColors,
                        stripTriangles);
                }

                return;
            }

            for (int i = 0; i < chain.Segments.Count; i++)
            {
                EdgeChainSegment segment = chain.Segments[i];
                float keepChance = Mathf.Lerp(0.35f, 0.86f, coverage01);
                if (Hash01(seed, index, 31 + i) > keepChance)
                {
                    continue;
                }

                int fragmentCount =
                    coverage01 < 0.36f
                        ? 1
                        : coverage01 < 0.66f
                            ? 1 + Mathf.FloorToInt(Hash01(seed, index, 43 + i) * 2f)
                            : 2;
                float totalCoverage = Mathf.Lerp(0.20f, 0.76f, coverage01);
                float fragmentLength = totalCoverage / Mathf.Max(1, fragmentCount);
                float freeSpace = Mathf.Max(0.0f, 1.0f - totalCoverage);
                float cursor =
                    Mathf.Lerp(
                        0.02f,
                        Mathf.Max(0.02f, freeSpace * 0.55f),
                        Hash01(seed, index, 53 + i));

                for (int j = 0; j < fragmentCount; j++)
                {
                    float gap =
                        j == 0
                            ? 0f
                            : Mathf.Lerp(
                                freeSpace * 0.10f,
                                freeSpace * 0.34f,
                                Hash01(seed, index, 61 + i * 7 + j));
                    float start01 = Mathf.Clamp01(cursor + gap);
                    float end01 = Mathf.Clamp01(start01 + fragmentLength);
                    if (end01 - start01 > 0.035f)
                    {
                        AddStripSegment(
                            Vector3.Lerp(segment.Start, segment.End, start01),
                            Vector3.Lerp(segment.Start, segment.End, end01),
                            segment.Normal,
                            width,
                            offset,
                            new Color(0f, 0f, 0f, 1f),
                            Vector4.zero,
                            stripVertices,
                            stripNormals,
                            stripUv0,
                            stripUv2,
                            stripColors,
                            stripTriangles);
                    }

                    cursor = end01;
                }
            }
        }

        private static List<FaceCandidate> SelectDistributedFacesBySector(
            List<FaceCandidate> candidates,
            int targetCount,
            int seed)
        {
            List<FaceCandidate> selected = new();
            if (targetCount <= 0 || candidates.Count == 0)
            {
                return selected;
            }

            Dictionary<int, List<FaceCandidate>> bySector = new();
            for (int i = 0; i < candidates.Count; i++)
            {
                FaceCandidate candidate = candidates[i];
                int sector = candidate.Bucket % 4;
                if (!bySector.TryGetValue(sector, out List<FaceCandidate> sectorList))
                {
                    sectorList = new List<FaceCandidate>();
                    bySector.Add(sector, sectorList);
                }

                sectorList.Add(candidate);
            }

            foreach (List<FaceCandidate> sectorList in bySector.Values)
            {
                sectorList.Sort(
                    (a, b) =>
                    {
                        float aScore =
                            a.Score *
                            Mathf.Lerp(0.90f, 1.10f, Hash01(seed, a.Bucket, 19));
                        float bScore =
                            b.Score *
                            Mathf.Lerp(0.90f, 1.10f, Hash01(seed, b.Bucket, 19));
                        return bScore.CompareTo(aScore);
                    });
            }

            List<int> sectors = new(bySector.Keys);
            sectors.Sort(
                (a, b) =>
                    Hash01(seed, a, 31).CompareTo(Hash01(seed, b, 31)));

            int pass = 0;
            while (selected.Count < targetCount)
            {
                bool added = false;
                for (int i = 0; i < sectors.Count && selected.Count < targetCount; i++)
                {
                    List<FaceCandidate> sectorList = bySector[sectors[i]];
                    if (pass >= sectorList.Count)
                    {
                        continue;
                    }

                    selected.Add(sectorList[pass]);
                    added = true;
                }

                if (!added)
                {
                    break;
                }

                pass++;
            }

            return selected;
        }

        private static int ResolveSpatialBucket(
            Vector3 position,
            Bounds bounds,
            Vector3 normal)
        {
            Vector3 relative = position - bounds.center;
            int sector;
            if (Mathf.Abs(relative.x) > Mathf.Abs(relative.z))
            {
                sector = relative.x >= 0f ? 0 : 1;
            }
            else
            {
                sector = relative.z >= 0f ? 2 : 3;
            }

            float height01 = Mathf.InverseLerp(
                bounds.min.y,
                bounds.max.y,
                position.y);
            int heightBand =
                height01 < 0.34f
                    ? 0
                    : height01 < 0.68f
                        ? 1
                        : 2;
            int normalBand = normal.y > 0.52f ? 1 : 0;
            return sector + heightBand * 4 + normalBand * 12;
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
            float creaseLength,
            float creaseBranching,
            int seed,
            int index,
            List<Vector3> stripVertices,
            List<Vector3> stripNormals,
            List<Vector2> stripUv0,
            List<Vector4> stripUv2,
            List<Color> stripColors,
            List<int> stripTriangles)
        {
            int startEdge = Mathf.FloorToInt(Hash01(seed, index, 53) * 3f);
            int endEdge =
                (startEdge +
                 1 +
                 Mathf.FloorToInt(Hash01(seed, index, 57) * 2f)) %
                3;
            Vector3 p0 =
                ResolvePointOnTriangleEdge(
                    a,
                    b,
                    c,
                    startEdge,
                    Mathf.Lerp(0.14f, 0.86f, Hash01(seed, index, 59)));
            Vector3 p3 =
                ResolvePointOnTriangleEdge(
                    a,
                    b,
                    c,
                    endEdge,
                    Mathf.Lerp(0.14f, 0.86f, Hash01(seed, index, 61)));

            Vector3 center = (a + b + c) / 3f;
            float length01 = Mathf.InverseLerp(0.25f, 2f, creaseLength);
            p0 = Vector3.Lerp(center, p0, Mathf.Lerp(0.55f, 1.06f, length01));
            p3 = Vector3.Lerp(center, p3, Mathf.Lerp(0.55f, 1.08f, length01));

            Vector3 direction = p3 - p0;
            if (direction.sqrMagnitude < 0.000001f)
            {
                return;
            }

            direction.Normalize();
            normal = normal.sqrMagnitude > 0.000001f ? normal.normalized : Vector3.up;
            Vector3 lateral = Vector3.Cross(normal, direction);
            if (lateral.sqrMagnitude < 0.000001f)
            {
                return;
            }

            lateral.Normalize();
            float jaggedScale = Mathf.Lerp(2.4f, 4.2f, length01);
            Vector3 p1 =
                Vector3.Lerp(p0, p3, 0.30f) +
                lateral * width * Mathf.Lerp(-jaggedScale, jaggedScale, Hash01(seed, index, 67));
            Vector3 p2 =
                Vector3.Lerp(p0, p3, 0.66f) +
                lateral * width * Mathf.Lerp(-jaggedScale * 1.08f, jaggedScale * 1.08f, Hash01(seed, index, 71));
            Vector3[] points = { p0, p1, p2, p3 };

            for (int i = 0; i < points.Length - 1; i++)
            {
                AddStripSegment(
                    points[i],
                    points[i + 1],
                    normal,
                    width * Mathf.Lerp(0.74f, 1.08f, Hash01(seed, index, 83 + i)),
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

            float branching01 = Mathf.Clamp01(creaseBranching * 0.5f);
            float branchChance = Mathf.Lerp(0.0f, 0.95f, branching01);
            if (Hash01(seed, index, 97) < branchChance)
            {
                AddCrackBranch(
                    a,
                    b,
                    c,
                    endEdge,
                    p1,
                    p2,
                    normal,
                    width * Mathf.Lerp(0.68f, 0.86f, branching01),
                    offset,
                    length01,
                    seed,
                    index,
                    0,
                    stripVertices,
                    stripNormals,
                    stripUv0,
                    stripUv2,
                    stripColors,
                    stripTriangles);
            }

            if (branching01 > 0.55f &&
                Hash01(seed, index, 127) < Mathf.InverseLerp(0.55f, 1.0f, branching01) * 0.70f)
            {
                AddCrackBranch(
                    a,
                    b,
                    c,
                    startEdge,
                    p2,
                    p1,
                    normal,
                    width * 0.62f,
                    offset,
                    length01,
                    seed,
                    index,
                    1,
                    stripVertices,
                    stripNormals,
                    stripUv0,
                    stripUv2,
                    stripColors,
                    stripTriangles);
            }
        }

        private static void AddCrackBranch(
            Vector3 a,
            Vector3 b,
            Vector3 c,
            int referenceEdge,
            Vector3 preferredStart,
            Vector3 alternateStart,
            Vector3 normal,
            float width,
            float offset,
            float length01,
            int seed,
            int index,
            int branchIndex,
            List<Vector3> stripVertices,
            List<Vector3> stripNormals,
            List<Vector2> stripUv0,
            List<Vector4> stripUv2,
            List<Color> stripColors,
            List<int> stripTriangles)
        {
            int branchEdge =
                (referenceEdge +
                 1 +
                 Mathf.FloorToInt(Hash01(seed, index, 101 + branchIndex * 17) * 2f)) %
                3;
            Vector3 branchStart =
                Hash01(seed, index, 103 + branchIndex * 17) > 0.42f
                    ? preferredStart
                    : alternateStart;
            Vector3 branchEnd =
                Vector3.Lerp(
                    branchStart,
                    ResolvePointOnTriangleEdge(
                        a,
                        b,
                        c,
                        branchEdge,
                        Mathf.Lerp(
                            0.14f,
                            0.86f,
                            Hash01(seed, index, 107 + branchIndex * 17))),
                    Mathf.Lerp(
                        Mathf.Lerp(0.32f, 0.48f, length01),
                        Mathf.Lerp(0.56f, 0.82f, length01),
                        Hash01(seed, index, 109 + branchIndex * 17)));

            AddStripSegment(
                branchStart,
                branchEnd,
                normal,
                width,
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

        private static Vector3 ResolvePointOnTriangleEdge(
            Vector3 a,
            Vector3 b,
            Vector3 c,
            int edge,
            float t)
        {
            return edge switch
            {
                0 => Vector3.Lerp(a, b, t),
                1 => Vector3.Lerp(b, c, t),
                _ => Vector3.Lerp(c, a, t)
            };
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
            normal =
                normal.sqrMagnitude > 0.000001f
                    ? normal.normalized
                    : Vector3.up;
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

            // Duplicate the strip backside so debug validation is not affected
            // by camera side or winding.
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
