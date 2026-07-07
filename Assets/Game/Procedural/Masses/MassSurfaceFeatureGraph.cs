using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public enum MassSurfaceBoundaryKind
    {
        OpenBorder = 0,
        FlatInternal = 1,
        ConvexRidge = 2,
        ConcaveCrease = 3,
        Ambiguous = 4
    }

    /// <summary>
    /// Semantic surface graph for generated compact masses. The generated mass
    /// mesh uses duplicated triangle-corner vertices, so adjacency is rebuilt
    /// from quantized vertex positions rather than raw shared indices.
    ///
    /// This graph is the foundation for atlas features: patches own continuous
    /// surface regions, and patch boundaries carry convex/concave semantics.
    /// </summary>
    public sealed class MassSurfaceFeatureGraph
    {
        private const int PositionQuantization = 10000;
        private const float FlatAngleDegrees = 12f;
        private const float ClassificationPlaneEpsilon = 0.00002f;

        private MassSurfaceFeatureGraph(
            List<Triangle> triangles,
            List<Boundary> boundaries,
            List<Patch> patches,
            List<BoundaryChain> boundaryChains)
        {
            Triangles = triangles;
            Boundaries = boundaries;
            Patches = patches;
            BoundaryChains = boundaryChains;
        }

        public IReadOnlyList<Triangle> Triangles { get; }
        public IReadOnlyList<Boundary> Boundaries { get; }
        public IReadOnlyList<Patch> Patches { get; }
        public IReadOnlyList<BoundaryChain> BoundaryChains { get; }

        public sealed class Triangle
        {
            public Triangle(
                int index,
                int vertexA,
                int vertexB,
                int vertexC,
                Vector3 positionA,
                Vector3 positionB,
                Vector3 positionC,
                Vector3 normal,
                Vector3 center,
                float area)
            {
                Index = index;
                VertexA = vertexA;
                VertexB = vertexB;
                VertexC = vertexC;
                PositionA = positionA;
                PositionB = positionB;
                PositionC = positionC;
                Normal = normal;
                Center = center;
                Area = area;
                PatchIndex = -1;
            }

            public int Index { get; }
            public int VertexA { get; }
            public int VertexB { get; }
            public int VertexC { get; }
            public Vector3 PositionA { get; }
            public Vector3 PositionB { get; }
            public Vector3 PositionC { get; }
            public Vector3 Normal { get; }
            public Vector3 Center { get; }
            public float Area { get; }
            public int PatchIndex { get; internal set; }
        }

        public sealed class Boundary
        {
            public Boundary(
                int index,
                int triangleA,
                int triangleB,
                Vector3 start,
                Vector3 end,
                MassSurfaceBoundaryKind kind,
                float angleDegrees)
            {
                Index = index;
                TriangleA = triangleA;
                TriangleB = triangleB;
                Start = start;
                End = end;
                Kind = kind;
                AngleDegrees = angleDegrees;
                Length = Vector3.Distance(start, end);
                PatchA = -1;
                PatchB = -1;
                Score = 0f;
                ChainIndex = -1;
                ChainDistanceAtStart = 0f;
                ChainDistanceAtEnd = 0f;
            }

            public int Index { get; }
            public int TriangleA { get; }
            public int TriangleB { get; }
            public Vector3 Start { get; }
            public Vector3 End { get; }
            public MassSurfaceBoundaryKind Kind { get; }
            public float AngleDegrees { get; }
            public float Length { get; }
            public int PatchA { get; internal set; }
            public int PatchB { get; internal set; }
            public float Score { get; internal set; }
            public int ChainIndex { get; internal set; }
            public float ChainDistanceAtStart { get; internal set; }
            public float ChainDistanceAtEnd { get; internal set; }
        }

        public sealed class BoundaryChain
        {
            public BoundaryChain(int index, MassSurfaceBoundaryKind kind)
            {
                Index = index;
                Kind = kind;
                BoundaryIndices = new List<int>();
                Length = 0f;
                AverageScore = 0f;
                Salience = 0f;
            }

            public int Index { get; }
            public MassSurfaceBoundaryKind Kind { get; }
            public List<int> BoundaryIndices { get; }
            public float Length { get; internal set; }
            public float AverageScore { get; internal set; }
            public float Salience { get; internal set; }
        }

        public sealed class Patch
        {
            public Patch(int index)
            {
                Index = index;
                TriangleIndices = new List<int>();
                BoundaryIndices = new List<int>();
                Area = 0f;
                Center = Vector3.zero;
                Normal = Vector3.up;
            }

            public int Index { get; }
            public List<int> TriangleIndices { get; }
            public List<int> BoundaryIndices { get; }
            public float Area { get; internal set; }
            public Vector3 Center { get; internal set; }
            public Vector3 Normal { get; internal set; }
        }

        private readonly struct PositionKey : IEquatable<PositionKey>, IComparable<PositionKey>
        {
            public PositionKey(Vector3 position)
            {
                X = Mathf.RoundToInt(position.x * PositionQuantization);
                Y = Mathf.RoundToInt(position.y * PositionQuantization);
                Z = Mathf.RoundToInt(position.z * PositionQuantization);
            }

            private int X { get; }
            private int Y { get; }
            private int Z { get; }

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

            private PositionKey A { get; }
            private PositionKey B { get; }
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

        public static MassSurfaceFeatureGraph Build(
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<int> triangleIndices,
            Bounds bounds)
        {
            List<Triangle> triangles = BuildTriangles(vertices, triangleIndices);
            Dictionary<EdgeKey, EdgeRecord> edgeMap = BuildEdgeMap(vertices, triangleIndices);
            List<Boundary> boundaries = BuildBoundaries(triangles, edgeMap, bounds);
            List<Patch> patches = BuildPatches(triangles, boundaries);
            ResolveBoundaryPatchMembership(triangles, boundaries, patches, bounds);
            List<BoundaryChain> boundaryChains = BuildBoundaryChains(boundaries);
            return new MassSurfaceFeatureGraph(triangles, boundaries, patches, boundaryChains);
        }

        private static List<Triangle> BuildTriangles(
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<int> triangleIndices)
        {
            List<Triangle> result = new(triangleIndices.Count / 3);
            for (int i = 0; i < triangleIndices.Count; i += 3)
            {
                int a = triangleIndices[i];
                int b = triangleIndices[i + 1];
                int c = triangleIndices[i + 2];
                Vector3 positionA = vertices[a];
                Vector3 positionB = vertices[b];
                Vector3 positionC = vertices[c];
                Vector3 ab = positionB - positionA;
                Vector3 ac = positionC - positionA;
                Vector3 cross = Vector3.Cross(ab, ac);
                float area = cross.magnitude * 0.5f;
                Vector3 normal =
                    cross.sqrMagnitude > 0.0000001f
                        ? cross.normalized
                        : Vector3.up;
                Vector3 center = (positionA + positionB + positionC) / 3f;
                result.Add(
                    new Triangle(
                        i / 3,
                        a,
                        b,
                        c,
                        positionA,
                        positionB,
                        positionC,
                        normal,
                        center,
                        area));
            }

            return result;
        }

        private static Dictionary<EdgeKey, EdgeRecord> BuildEdgeMap(
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<int> triangleIndices)
        {
            Dictionary<EdgeKey, EdgeRecord> edgeMap = new();
            for (int i = 0; i < triangleIndices.Count; i += 3)
            {
                int triangleIndex = i / 3;
                AddEdge(edgeMap, vertices[triangleIndices[i]], vertices[triangleIndices[i + 1]], triangleIndex);
                AddEdge(edgeMap, vertices[triangleIndices[i + 1]], vertices[triangleIndices[i + 2]], triangleIndex);
                AddEdge(edgeMap, vertices[triangleIndices[i + 2]], vertices[triangleIndices[i]], triangleIndex);
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

        private static List<Boundary> BuildBoundaries(
            List<Triangle> triangles,
            Dictionary<EdgeKey, EdgeRecord> edgeMap,
            Bounds bounds)
        {
            List<Boundary> result = new(edgeMap.Count);
            float scale = Mathf.Max(0.0001f, Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z)));

            List<EdgeRecord> edgeRecords = new(edgeMap.Values);
            edgeRecords.Sort(CompareEdgeRecords);

            for (int edgeIndex = 0; edgeIndex < edgeRecords.Count; edgeIndex++)
            {
                EdgeRecord record = edgeRecords[edgeIndex];
                MassSurfaceBoundaryKind kind = MassSurfaceBoundaryKind.OpenBorder;
                float angleDegrees = 0f;

                if (record.TriangleA >= 0 && record.TriangleB >= 0)
                {
                    Triangle triangleA = triangles[record.TriangleA];
                    Triangle triangleB = triangles[record.TriangleB];
                    float normalDot = Mathf.Clamp(Vector3.Dot(triangleA.Normal, triangleB.Normal), -1f, 1f);
                    angleDegrees = Mathf.Acos(normalDot) * Mathf.Rad2Deg;

                    if (angleDegrees <= FlatAngleDegrees)
                    {
                        kind = MassSurfaceBoundaryKind.FlatInternal;
                    }
                    else
                    {
                        float epsilon = Mathf.Max(ClassificationPlaneEpsilon, scale * 0.00002f);
                        float sideAB = Vector3.Dot(triangleA.Normal, triangleB.Center - triangleA.Center);
                        float sideBA = Vector3.Dot(triangleB.Normal, triangleA.Center - triangleB.Center);

                        if (sideAB < -epsilon && sideBA < -epsilon)
                        {
                            kind = MassSurfaceBoundaryKind.ConvexRidge;
                        }
                        else if (sideAB > epsilon && sideBA > epsilon)
                        {
                            kind = MassSurfaceBoundaryKind.ConcaveCrease;
                        }
                        else
                        {
                            kind = MassSurfaceBoundaryKind.Ambiguous;
                        }
                    }
                }

                result.Add(
                    new Boundary(
                        result.Count,
                        record.TriangleA,
                        record.TriangleB,
                        record.Start,
                        record.End,
                        kind,
                        angleDegrees));
            }

            return result;
        }


        private static int CompareEdgeRecords(EdgeRecord a, EdgeRecord b)
        {
            PositionKey aStart = new(a.Start);
            PositionKey bStart = new(b.Start);
            int start = aStart.CompareTo(bStart);
            if (start != 0)
            {
                return start;
            }

            PositionKey aEnd = new(a.End);
            PositionKey bEnd = new(b.End);
            return aEnd.CompareTo(bEnd);
        }

        private static List<Patch> BuildPatches(
            List<Triangle> triangles,
            List<Boundary> boundaries)
        {
            List<int>[] flatNeighbors = new List<int>[triangles.Count];
            for (int i = 0; i < flatNeighbors.Length; i++)
            {
                flatNeighbors[i] = new List<int>();
            }

            for (int i = 0; i < boundaries.Count; i++)
            {
                Boundary boundary = boundaries[i];
                if (boundary.Kind != MassSurfaceBoundaryKind.FlatInternal ||
                    boundary.TriangleA < 0 ||
                    boundary.TriangleB < 0)
                {
                    continue;
                }

                flatNeighbors[boundary.TriangleA].Add(boundary.TriangleB);
                flatNeighbors[boundary.TriangleB].Add(boundary.TriangleA);
            }

            List<Patch> patches = new();
            Queue<int> queue = new();
            for (int i = 0; i < triangles.Count; i++)
            {
                if (triangles[i].PatchIndex >= 0)
                {
                    continue;
                }

                Patch patch = new(patches.Count);
                patches.Add(patch);
                triangles[i].PatchIndex = patch.Index;
                queue.Enqueue(i);

                while (queue.Count > 0)
                {
                    int triangleIndex = queue.Dequeue();
                    Triangle triangle = triangles[triangleIndex];
                    patch.TriangleIndices.Add(triangleIndex);

                    for (int n = 0; n < flatNeighbors[triangleIndex].Count; n++)
                    {
                        int neighborIndex = flatNeighbors[triangleIndex][n];
                        if (triangles[neighborIndex].PatchIndex >= 0)
                        {
                            continue;
                        }

                        triangles[neighborIndex].PatchIndex = patch.Index;
                        queue.Enqueue(neighborIndex);
                    }
                }
            }

            ResolvePatchTransforms(triangles, patches);
            return patches;
        }

        private static void ResolvePatchTransforms(
            List<Triangle> triangles,
            List<Patch> patches)
        {
            for (int i = 0; i < patches.Count; i++)
            {
                Patch patch = patches[i];
                float area = 0f;
                Vector3 weightedCenter = Vector3.zero;
                Vector3 weightedNormal = Vector3.zero;

                for (int t = 0; t < patch.TriangleIndices.Count; t++)
                {
                    Triangle triangle = triangles[patch.TriangleIndices[t]];
                    float triangleArea = Mathf.Max(0.000001f, triangle.Area);
                    area += triangleArea;
                    weightedCenter += triangle.Center * triangleArea;
                    weightedNormal += triangle.Normal * triangleArea;
                }

                patch.Area = area;
                patch.Center = area > 0.000001f ? weightedCenter / area : Vector3.zero;
                patch.Normal =
                    weightedNormal.sqrMagnitude > 0.0000001f
                        ? weightedNormal.normalized
                        : Vector3.up;
            }
        }

        private static void ResolveBoundaryPatchMembership(
            List<Triangle> triangles,
            List<Boundary> boundaries,
            List<Patch> patches,
            Bounds bounds)
        {
            float scale = Mathf.Max(0.0001f, Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z)));

            for (int i = 0; i < boundaries.Count; i++)
            {
                Boundary boundary = boundaries[i];
                int patchA =
                    boundary.TriangleA >= 0
                        ? triangles[boundary.TriangleA].PatchIndex
                        : -1;
                int patchB =
                    boundary.TriangleB >= 0
                        ? triangles[boundary.TriangleB].PatchIndex
                        : -1;

                boundary.PatchA = patchA;
                boundary.PatchB = patchB;

                if (patchA >= 0 && boundary.Kind != MassSurfaceBoundaryKind.FlatInternal)
                {
                    patches[patchA].BoundaryIndices.Add(boundary.Index);
                }

                if (patchB >= 0 && patchB != patchA && boundary.Kind != MassSurfaceBoundaryKind.FlatInternal)
                {
                    patches[patchB].BoundaryIndices.Add(boundary.Index);
                }

                boundary.Score = ResolveBoundaryScore(boundary, patches, bounds, scale);
            }
        }

        private static List<BoundaryChain> BuildBoundaryChains(List<Boundary> boundaries)
        {
            List<BoundaryChain> chains = new();
            Dictionary<PositionKey, List<int>> byEndpoint = new();
            for (int i = 0; i < boundaries.Count; i++)
            {
                Boundary boundary = boundaries[i];
                if (!CanChainBoundary(boundary))
                {
                    continue;
                }

                AddEndpointBoundary(byEndpoint, new PositionKey(boundary.Start), i);
                AddEndpointBoundary(byEndpoint, new PositionKey(boundary.End), i);
            }

            bool[] visited = new bool[boundaries.Count];

            // Trace open paths first. Endpoints and junctions intentionally split
            // chains so boundary-local coordinates do not smear across unrelated
            // ridge/crease branches at a corner.
            for (int i = 0; i < boundaries.Count; i++)
            {
                Boundary boundary = boundaries[i];
                if (visited[i] || !CanChainBoundary(boundary))
                {
                    continue;
                }

                PositionKey startKey = new(boundary.Start);
                PositionKey endKey = new(boundary.End);
                bool startBreak = CountEndpointBoundaries(
                    byEndpoint,
                    startKey,
                    boundaries,
                    boundary.Kind) != 2;
                bool endBreak = CountEndpointBoundaries(
                    byEndpoint,
                    endKey,
                    boundaries,
                    boundary.Kind) != 2;

                if (!startBreak && !endBreak)
                {
                    continue;
                }

                TraceBoundaryChain(
                    boundaries,
                    byEndpoint,
                    visited,
                    chains,
                    i,
                    startBreak ? startKey : endKey);
            }

            // Remaining boundaries are closed loops. Trace them from their stable
            // stored start endpoint so the coordinate field remains deterministic.
            for (int i = 0; i < boundaries.Count; i++)
            {
                Boundary boundary = boundaries[i];
                if (visited[i] || !CanChainBoundary(boundary))
                {
                    continue;
                }

                TraceBoundaryChain(
                    boundaries,
                    byEndpoint,
                    visited,
                    chains,
                    i,
                    new PositionKey(boundary.Start));
            }

            ResolveBoundaryChainSalience(chains, boundaries);
            return chains;
        }

        private static bool CanChainBoundary(Boundary boundary) =>
            boundary.Score > 0.0001f &&
            (boundary.Kind == MassSurfaceBoundaryKind.ConvexRidge ||
             boundary.Kind == MassSurfaceBoundaryKind.ConcaveCrease);

        private static void AddEndpointBoundary(
            Dictionary<PositionKey, List<int>> byEndpoint,
            PositionKey endpoint,
            int boundaryIndex)
        {
            if (!byEndpoint.TryGetValue(endpoint, out List<int> indices))
            {
                indices = new List<int>();
                byEndpoint.Add(endpoint, indices);
            }

            indices.Add(boundaryIndex);
        }

        private static int CountEndpointBoundaries(
            Dictionary<PositionKey, List<int>> byEndpoint,
            PositionKey endpoint,
            List<Boundary> boundaries,
            MassSurfaceBoundaryKind kind)
        {
            if (!byEndpoint.TryGetValue(endpoint, out List<int> indices))
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < indices.Count; i++)
            {
                Boundary boundary = boundaries[indices[i]];
                if (boundary.Kind == kind && CanChainBoundary(boundary))
                {
                    count++;
                }
            }

            return count;
        }

        private static void TraceBoundaryChain(
            List<Boundary> boundaries,
            Dictionary<PositionKey, List<int>> byEndpoint,
            bool[] visited,
            List<BoundaryChain> chains,
            int seedBoundaryIndex,
            PositionKey entryEndpoint)
        {
            Boundary seed = boundaries[seedBoundaryIndex];
            BoundaryChain chain = new(chains.Count, seed.Kind);
            chains.Add(chain);

            int currentIndex = seedBoundaryIndex;
            PositionKey currentEntry = entryEndpoint;
            float distance = 0f;

            while (currentIndex >= 0 && !visited[currentIndex])
            {
                Boundary boundary = boundaries[currentIndex];
                if (!CanChainBoundary(boundary) || boundary.Kind != chain.Kind)
                {
                    break;
                }

                visited[currentIndex] = true;
                boundary.ChainIndex = chain.Index;
                chain.BoundaryIndices.Add(currentIndex);

                PositionKey startKey = new(boundary.Start);
                PositionKey endKey = new(boundary.End);
                bool enteredAtStart = startKey.Equals(currentEntry) || !endKey.Equals(currentEntry);
                PositionKey exitKey = enteredAtStart ? endKey : startKey;
                float nextDistance = distance + Mathf.Max(0f, boundary.Length);

                if (enteredAtStart)
                {
                    boundary.ChainDistanceAtStart = distance;
                    boundary.ChainDistanceAtEnd = nextDistance;
                }
                else
                {
                    boundary.ChainDistanceAtStart = nextDistance;
                    boundary.ChainDistanceAtEnd = distance;
                }

                distance = nextDistance;

                if (CountEndpointBoundaries(
                        byEndpoint,
                        exitKey,
                        boundaries,
                        chain.Kind) != 2)
                {
                    break;
                }

                int nextIndex = FindNextBoundaryAtEndpoint(
                    byEndpoint,
                    exitKey,
                    boundaries,
                    visited,
                    chain.Kind);
                if (nextIndex < 0)
                {
                    break;
                }

                currentEntry = exitKey;
                currentIndex = nextIndex;
            }

            chain.Length = Mathf.Max(0.0001f, distance);
        }

        private static int FindNextBoundaryAtEndpoint(
            Dictionary<PositionKey, List<int>> byEndpoint,
            PositionKey endpoint,
            List<Boundary> boundaries,
            bool[] visited,
            MassSurfaceBoundaryKind kind)
        {
            if (!byEndpoint.TryGetValue(endpoint, out List<int> indices))
            {
                return -1;
            }

            int bestIndex = -1;
            for (int i = 0; i < indices.Count; i++)
            {
                int boundaryIndex = indices[i];
                if (visited[boundaryIndex])
                {
                    continue;
                }

                Boundary boundary = boundaries[boundaryIndex];
                if (boundary.Kind != kind || !CanChainBoundary(boundary))
                {
                    continue;
                }

                if (bestIndex < 0 || boundaryIndex < bestIndex)
                {
                    bestIndex = boundaryIndex;
                }
            }

            return bestIndex;
        }

        private static void ResolveBoundaryChainSalience(
            List<BoundaryChain> chains,
            List<Boundary> boundaries)
        {
            float maxConvexLength = 0.0001f;
            float maxConcaveLength = 0.0001f;
            for (int i = 0; i < chains.Count; i++)
            {
                BoundaryChain chain = chains[i];
                if (chain.Kind == MassSurfaceBoundaryKind.ConvexRidge)
                {
                    maxConvexLength = Mathf.Max(maxConvexLength, chain.Length);
                }
                else if (chain.Kind == MassSurfaceBoundaryKind.ConcaveCrease)
                {
                    maxConcaveLength = Mathf.Max(maxConcaveLength, chain.Length);
                }
            }

            for (int i = 0; i < chains.Count; i++)
            {
                BoundaryChain chain = chains[i];
                float weightedScore = 0f;
                float weightedLength = 0f;
                for (int b = 0; b < chain.BoundaryIndices.Count; b++)
                {
                    Boundary boundary = boundaries[chain.BoundaryIndices[b]];
                    float length = Mathf.Max(0.0001f, boundary.Length);
                    weightedScore += boundary.Score * length;
                    weightedLength += length;
                }

                float averageScore =
                    weightedLength > 0.0001f
                        ? weightedScore / weightedLength
                        : 0f;
                float lengthReference =
                    chain.Kind == MassSurfaceBoundaryKind.ConvexRidge
                        ? maxConvexLength
                        : maxConcaveLength;
                float lengthScore = Mathf.Clamp01(chain.Length / Mathf.Max(0.0001f, lengthReference));

                // Salience is a reusable structural boundary fact. It deliberately
                // mixes geometric score and chain length, but it is not an edge-wear
                // macro control; material features decide how to interpret it.
                chain.AverageScore = Mathf.Clamp01(averageScore);
                chain.Salience = Mathf.Clamp01(averageScore * 0.70f + lengthScore * 0.30f);
            }
        }

        private static float ResolveBoundaryScore(
            Boundary boundary,
            List<Patch> patches,
            Bounds bounds,
            float scale)
        {
            if (boundary.Length < scale * 0.018f)
            {
                return 0f;
            }

            float angleScore = Mathf.InverseLerp(14f, 100f, boundary.AngleDegrees);
            float lengthScore = Mathf.Clamp01(boundary.Length / (scale * 0.22f));
            Vector3 midpoint = (boundary.Start + boundary.End) * 0.5f;
            float height01 = Mathf.InverseLerp(bounds.min.y, bounds.max.y, midpoint.y);
            float lowerPenalty = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.035f, 0.18f, height01));
            Vector3 normal = Vector3.zero;
            int normalCount = 0;
            if (boundary.PatchA >= 0)
            {
                normal += patches[boundary.PatchA].Normal;
                normalCount++;
            }

            if (boundary.PatchB >= 0 && boundary.PatchB != boundary.PatchA)
            {
                normal += patches[boundary.PatchB].Normal;
                normalCount++;
            }

            if (normalCount > 0)
            {
                normal /= normalCount;
            }

            if (normal.sqrMagnitude <= 0.000001f)
            {
                normal = Vector3.up;
            }
            else
            {
                normal.Normalize();
            }

            float exposureScore = Mathf.Lerp(0.72f, 1.18f, Mathf.Clamp01(normal.y * 0.5f + 0.5f));
            return Mathf.Clamp01(Mathf.Max(0.12f, angleScore * 0.72f + lengthScore * 0.28f) * lowerPenalty * exposureScore);
        }
    }
}
