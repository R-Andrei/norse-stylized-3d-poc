using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    public sealed class TreeBarkMeshTopologyAuditResult
    {
        public bool Passed { get; internal set; }
        public int NonFiniteVertexCount { get; internal set; }
        public int NonFiniteNormalCount { get; internal set; }
        public int NonFiniteTangentCount { get; internal set; }
        public int NonFiniteUvCount { get; internal set; }
        public int InvalidIndexCount { get; internal set; }
        public int DegenerateTriangleCount { get; internal set; }
        public int ZeroLengthRingSegmentCount { get; internal set; }
        public int InwardSideTriangleCount { get; internal set; }
        public int SideOrientationFailureCount { get; internal set; }
        public int CapOrientationFailureCount { get; internal set; }
        public int TangentBasisFailureCount { get; internal set; }
        public int RawBoundaryEdgeCount { get; internal set; }
        public int PositionWeldedSeamEdgeCount { get; internal set; }
        public int ExpectedEmbeddedRootLoopCount { get; internal set; }
        public int MatchedEmbeddedRootLoopCount { get; internal set; }
        public int PositionWeldedBoundaryLoopCount { get; internal set; }
        public int UnexpectedExposedBoundaryLoopCount { get; internal set; }
        public int NonManifoldEdgeCount { get; internal set; }
        public int OppositeSideRootEmergenceCount { get; internal set; }
        public int RootLoopOutsideParentCount { get; internal set; }
        public string InwardSideTriangleDetails { get; internal set; }
        public string UnexpectedBoundaryLoopDetails { get; internal set; }
        public string Report { get; internal set; }
    }

    internal sealed class TreeBarkMeshBranchAuditRecord
    {
        internal TreeBranchDefinition Branch;
        internal int SideTriangleStart;
        internal int SideTriangleCount;
        internal int RadialSegments;
        internal int RingCount;
        internal Vector3 RootCenter;
        internal float RootRadius;
        internal int ZeroLengthRingSegmentCount;
    }

    internal sealed class TreeBarkMeshCapAuditRecord
    {
        internal int TriangleStart;
        internal int TriangleCount;
        internal Vector3 ExpectedNormal;
    }

    internal static class TreeBarkMeshTopologyAudit
    {
        private const float PositionWeldTolerance = 0.00005f;
        private const float DegenerateAreaEpsilon = 0.0000000001f;
        private const float OrientationEpsilon = 0.00001f;

        private readonly struct EdgeKey : IEquatable<EdgeKey>
        {
            internal EdgeKey(int first, int second)
            {
                if (first <= second)
                {
                    A = first;
                    B = second;
                }
                else
                {
                    A = second;
                    B = first;
                }
            }

            internal int A { get; }
            internal int B { get; }

            public bool Equals(EdgeKey other)
            {
                return A == other.A && B == other.B;
            }

            public override bool Equals(object obj)
            {
                return obj is EdgeKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (A * 397) ^ B;
                }
            }
        }

        private readonly struct PositionKey : IEquatable<PositionKey>
        {
            internal PositionKey(Vector3 position)
                : this(
                    Mathf.FloorToInt(position.x / PositionWeldTolerance),
                    Mathf.FloorToInt(position.y / PositionWeldTolerance),
                    Mathf.FloorToInt(position.z / PositionWeldTolerance))
            {
            }

            private PositionKey(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            internal int X { get; }
            internal int Y { get; }
            internal int Z { get; }

            internal PositionKey Offset(int x, int y, int z)
            {
                return new PositionKey(X + x, Y + y, Z + z);
            }

            public bool Equals(PositionKey other)
            {
                return X == other.X && Y == other.Y && Z == other.Z;
            }

            public override bool Equals(object obj)
            {
                return obj is PositionKey other && Equals(other);
            }

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

        internal static TreeBarkMeshTopologyAuditResult Run(
            TreeDefinition definition,
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<Vector3> normals,
            IReadOnlyList<Vector4> tangents,
            IReadOnlyList<Vector2> uv0,
            IReadOnlyList<int> triangles,
            IReadOnlyList<TreeBarkMeshBranchAuditRecord> branchRecords,
            IReadOnlyList<TreeBarkMeshCapAuditRecord> capRecords)
        {
            var result = new TreeBarkMeshTopologyAuditResult();
            ValidateVertexStreams(vertices, normals, tangents, uv0, result);
            ValidateTriangles(vertices, normals, triangles, result);
            ValidateRingSegments(branchRecords, result);
            ValidateSideOrientation(vertices, normals, triangles, branchRecords, result);
            ValidateCapOrientation(vertices, triangles, capRecords, result);
            if (result.NonFiniteVertexCount == 0)
            {
                ValidateEdges(vertices, triangles, branchRecords, result);
                ValidateEmbeddedRoots(definition, branchRecords, result);
            }

            result.Passed =
                result.NonFiniteVertexCount == 0 &&
                result.NonFiniteNormalCount == 0 &&
                result.NonFiniteTangentCount == 0 &&
                result.NonFiniteUvCount == 0 &&
                result.InvalidIndexCount == 0 &&
                result.DegenerateTriangleCount == 0 &&
                result.ZeroLengthRingSegmentCount == 0 &&
                result.InwardSideTriangleCount == 0 &&
                result.SideOrientationFailureCount == 0 &&
                result.CapOrientationFailureCount == 0 &&
                result.TangentBasisFailureCount == 0 &&
                result.UnexpectedExposedBoundaryLoopCount == 0 &&
                result.NonManifoldEdgeCount == 0 &&
                result.OppositeSideRootEmergenceCount == 0 &&
                result.RootLoopOutsideParentCount == 0;
            result.Report = BuildReport(result);
            return result;
        }

        private static void ValidateVertexStreams(
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<Vector3> normals,
            IReadOnlyList<Vector4> tangents,
            IReadOnlyList<Vector2> uv0,
            TreeBarkMeshTopologyAuditResult result)
        {
            for (int index = 0; index < vertices.Count; index++)
            {
                if (!IsFinite(vertices[index]))
                {
                    result.NonFiniteVertexCount++;
                }

                Vector3 normal = index < normals.Count
                    ? normals[index]
                    : Vector3.zero;
                if (!IsFinite(normal) || normal.sqrMagnitude < 0.5f)
                {
                    result.NonFiniteNormalCount++;
                }

                Vector4 tangent = index < tangents.Count
                    ? tangents[index]
                    : Vector4.zero;
                if (!IsFinite(tangent))
                {
                    result.NonFiniteTangentCount++;
                }
                else
                {
                    Vector3 tangentDirection = new Vector3(
                        tangent.x,
                        tangent.y,
                        tangent.z);
                    if (tangentDirection.sqrMagnitude < 0.5f ||
                        Mathf.Abs(Vector3.Dot(
                            normal.normalized,
                            tangentDirection.normalized)) > 0.01f ||
                        Mathf.Abs(Mathf.Abs(tangent.w) - 1f) > 0.001f)
                    {
                        result.TangentBasisFailureCount++;
                    }
                }

                Vector2 uv = index < uv0.Count ? uv0[index] : Vector2.zero;
                if (!TreeDeterministicUtility.IsFinite(uv.x) ||
                    !TreeDeterministicUtility.IsFinite(uv.y))
                {
                    result.NonFiniteUvCount++;
                }
            }
        }

        private static void ValidateTriangles(
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<Vector3> normals,
            IReadOnlyList<int> triangles,
            TreeBarkMeshTopologyAuditResult result)
        {
            if (triangles.Count % 3 != 0)
            {
                result.InvalidIndexCount += triangles.Count % 3;
            }

            for (int index = 0; index + 2 < triangles.Count; index += 3)
            {
                int a = triangles[index];
                int b = triangles[index + 1];
                int c = triangles[index + 2];
                if (!IsValidIndex(a, vertices.Count) ||
                    !IsValidIndex(b, vertices.Count) ||
                    !IsValidIndex(c, vertices.Count))
                {
                    result.InvalidIndexCount++;
                    continue;
                }

                Vector3 cross = Vector3.Cross(
                    vertices[b] - vertices[a],
                    vertices[c] - vertices[a]);
                if (cross.sqrMagnitude <= DegenerateAreaEpsilon)
                {
                    result.DegenerateTriangleCount++;
                }
            }
        }

        private static void ValidateRingSegments(
            IReadOnlyList<TreeBarkMeshBranchAuditRecord> records,
            TreeBarkMeshTopologyAuditResult result)
        {
            for (int index = 0; index < records.Count; index++)
            {
                result.ZeroLengthRingSegmentCount +=
                    records[index].ZeroLengthRingSegmentCount;
            }
        }

        private static void ValidateSideOrientation(
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<Vector3> normals,
            IReadOnlyList<int> triangles,
            IReadOnlyList<TreeBarkMeshBranchAuditRecord> records,
            TreeBarkMeshTopologyAuditResult result)
        {
            var inwardDetails = new StringBuilder(1024);
            for (int recordIndex = 0; recordIndex < records.Count; recordIndex++)
            {
                TreeBarkMeshBranchAuditRecord record = records[recordIndex];
                int end = record.SideTriangleStart + record.SideTriangleCount;
                for (int triangleIndex = record.SideTriangleStart;
                     triangleIndex < end;
                     triangleIndex += 3)
                {
                    if (triangleIndex + 2 >= triangles.Count)
                    {
                        result.InvalidIndexCount++;
                        continue;
                    }

                    int a = triangles[triangleIndex];
                    int b = triangles[triangleIndex + 1];
                    int c = triangles[triangleIndex + 2];
                    if (!IsValidIndex(a, vertices.Count) ||
                        !IsValidIndex(b, vertices.Count) ||
                        !IsValidIndex(c, vertices.Count))
                    {
                        continue;
                    }

                    Vector3 faceNormal = Vector3.Cross(
                        vertices[b] - vertices[a],
                        vertices[c] - vertices[a]);
                    if (faceNormal.sqrMagnitude <= DegenerateAreaEpsilon)
                    {
                        continue;
                    }

                    Vector3 expected = normals[a] + normals[b] + normals[c];
                    if (expected.sqrMagnitude <= OrientationEpsilon)
                    {
                        result.SideOrientationFailureCount++;
                        continue;
                    }

                    float orientation = Vector3.Dot(
                        faceNormal.normalized,
                        expected.normalized);
                    if (orientation <= 0f)
                    {
                        result.InwardSideTriangleCount++;
                        int localTriangle =
                            (triangleIndex - record.SideTriangleStart) / 3;
                        int quadIndex = localTriangle / 2;
                        int ringIndex = record.RadialSegments > 0
                            ? quadIndex / record.RadialSegments
                            : -1;
                        int sideIndex = record.RadialSegments > 0
                            ? quadIndex % record.RadialSegments
                            : -1;
                        inwardDetails
                            .Append("- branch=")
                            .Append(record.Branch != null
                                ? record.Branch.StableBranchId
                                : "<null>")
                            .Append(" order=")
                            .Append(record.Branch != null
                                ? record.Branch.BranchOrder
                                : -1)
                            .Append(" ring=")
                            .Append(ringIndex)
                            .Append("/")
                            .Append(Mathf.Max(0, record.RingCount - 2))
                            .Append(" side=")
                            .Append(sideIndex)
                            .Append(" triangle=")
                            .Append(localTriangle % 2)
                            .Append(" orientation=")
                            .Append(orientation.ToString("F6"))
                            .AppendLine();
                    }
                    else if (orientation < 0.05f)
                    {
                        result.SideOrientationFailureCount++;
                    }
                }
            }

            result.InwardSideTriangleDetails = inwardDetails.ToString();
        }

        private static void ValidateCapOrientation(
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<int> triangles,
            IReadOnlyList<TreeBarkMeshCapAuditRecord> records,
            TreeBarkMeshTopologyAuditResult result)
        {
            for (int recordIndex = 0; recordIndex < records.Count; recordIndex++)
            {
                TreeBarkMeshCapAuditRecord record = records[recordIndex];
                int end = record.TriangleStart + record.TriangleCount;
                for (int triangleIndex = record.TriangleStart;
                     triangleIndex < end;
                     triangleIndex += 3)
                {
                    int a = triangles[triangleIndex];
                    int b = triangles[triangleIndex + 1];
                    int c = triangles[triangleIndex + 2];
                    if (!IsValidIndex(a, vertices.Count) ||
                        !IsValidIndex(b, vertices.Count) ||
                        !IsValidIndex(c, vertices.Count))
                    {
                        continue;
                    }

                    Vector3 faceNormal = Vector3.Cross(
                        vertices[b] - vertices[a],
                        vertices[c] - vertices[a]);
                    if (faceNormal.sqrMagnitude <= DegenerateAreaEpsilon ||
                        Vector3.Dot(
                            faceNormal.normalized,
                            record.ExpectedNormal.normalized) <= 0f)
                    {
                        result.CapOrientationFailureCount++;
                    }
                }
            }
        }

        private static void ValidateEdges(
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<int> triangles,
            IReadOnlyList<TreeBarkMeshBranchAuditRecord> branchRecords,
            TreeBarkMeshTopologyAuditResult result)
        {
            var rawCounts = new Dictionary<EdgeKey, int>();
            Dictionary<int, Vector3> weldedPositions;
            Dictionary<int, int> weldedVertexIds = BuildWeldedVertexIds(
                vertices,
                out weldedPositions);
            var weldedCounts = new Dictionary<EdgeKey, int>();
            for (int index = 0; index + 2 < triangles.Count; index += 3)
            {
                int a = triangles[index];
                int b = triangles[index + 1];
                int c = triangles[index + 2];
                if (!IsValidIndex(a, vertices.Count) ||
                    !IsValidIndex(b, vertices.Count) ||
                    !IsValidIndex(c, vertices.Count))
                {
                    continue;
                }

                AddEdge(rawCounts, a, b);
                AddEdge(rawCounts, b, c);
                AddEdge(rawCounts, c, a);
                AddEdge(weldedCounts, weldedVertexIds[a], weldedVertexIds[b]);
                AddEdge(weldedCounts, weldedVertexIds[b], weldedVertexIds[c]);
                AddEdge(weldedCounts, weldedVertexIds[c], weldedVertexIds[a]);
            }

            foreach (KeyValuePair<EdgeKey, int> pair in rawCounts)
            {
                if (pair.Value == 1)
                {
                    result.RawBoundaryEdgeCount++;
                }
            }

            var boundaryEdges = new List<EdgeKey>();
            foreach (KeyValuePair<EdgeKey, int> pair in weldedCounts)
            {
                if (pair.Key.A == pair.Key.B)
                {
                    continue;
                }

                if (pair.Value == 1)
                {
                    boundaryEdges.Add(pair.Key);
                }
                else if (pair.Value > 2)
                {
                    result.NonManifoldEdgeCount++;
                }
            }

            result.PositionWeldedSeamEdgeCount = Mathf.Max(
                0,
                result.RawBoundaryEdgeCount - boundaryEdges.Count);
            CollectBoundaryComponents(
                boundaryEdges,
                out List<List<int>> closedLoops,
                out int openComponents);
            result.PositionWeldedBoundaryLoopCount = closedLoops.Count;
            ClassifyEmbeddedRootLoops(
                closedLoops,
                weldedPositions,
                branchRecords,
                result);
            result.UnexpectedExposedBoundaryLoopCount += openComponents;
        }

        private static void ClassifyEmbeddedRootLoops(
            IReadOnlyList<List<int>> closedLoops,
            IReadOnlyDictionary<int, Vector3> weldedPositions,
            IReadOnlyList<TreeBarkMeshBranchAuditRecord> branchRecords,
            TreeBarkMeshTopologyAuditResult result)
        {
            var expectedRoots = new List<TreeBarkMeshBranchAuditRecord>();
            for (int index = 0; index < branchRecords.Count; index++)
            {
                TreeBarkMeshBranchAuditRecord record = branchRecords[index];
                if (record.Branch != null &&
                    record.Branch.ParentBranchIndex >= 0)
                {
                    expectedRoots.Add(record);
                }
            }

            result.ExpectedEmbeddedRootLoopCount = expectedRoots.Count;
            var matchedRoots = new HashSet<int>();
            var unexpectedDetails = new StringBuilder();
            for (int loopIndex = 0; loopIndex < closedLoops.Count; loopIndex++)
            {
                List<int> loop = closedLoops[loopIndex];
                Vector3 centroid = Vector3.zero;
                int positionCount = 0;
                for (int vertexIndex = 0; vertexIndex < loop.Count; vertexIndex++)
                {
                    if (weldedPositions.TryGetValue(
                            loop[vertexIndex],
                            out Vector3 position))
                    {
                        centroid += position;
                        positionCount++;
                    }
                }

                if (positionCount == 0)
                {
                    result.UnexpectedExposedBoundaryLoopCount++;
                    unexpectedDetails.Append("Loop ")
                        .Append(loopIndex)
                        .AppendLine(": no welded positions resolved.");
                    continue;
                }

                centroid /= positionCount;
                int bestRoot = -1;
                float bestDistance = float.PositiveInfinity;
                for (int rootIndex = 0; rootIndex < expectedRoots.Count; rootIndex++)
                {
                    if (matchedRoots.Contains(rootIndex))
                    {
                        continue;
                    }

                    TreeBarkMeshBranchAuditRecord root = expectedRoots[rootIndex];
                    float distance = Vector3.Distance(centroid, root.RootCenter);
                    float tolerance = Mathf.Max(
                        PositionWeldTolerance * 8f,
                        root.RootRadius * 0.2f);
                    if (distance <= tolerance && distance < bestDistance)
                    {
                        bestRoot = rootIndex;
                        bestDistance = distance;
                    }
                }

                if (bestRoot >= 0)
                {
                    matchedRoots.Add(bestRoot);
                    result.MatchedEmbeddedRootLoopCount++;
                }
                else
                {
                    result.UnexpectedExposedBoundaryLoopCount++;
                    float averageRadius = 0f;
                    for (int vertexIndex = 0;
                         vertexIndex < loop.Count;
                         vertexIndex++)
                    {
                        if (weldedPositions.TryGetValue(
                                loop[vertexIndex],
                                out Vector3 position))
                        {
                            averageRadius += Vector3.Distance(
                                centroid,
                                position);
                        }
                    }

                    averageRadius /= Mathf.Max(1, positionCount);
                    unexpectedDetails.Append("Loop ")
                        .Append(loopIndex)
                        .Append(" | vertices=")
                        .Append(loop.Count)
                        .Append(" | centroid=")
                        .Append(centroid.ToString("F5"))
                        .Append(" | averageRadius=")
                        .AppendLine(averageRadius.ToString("F5"));
                }
            }

            result.UnexpectedExposedBoundaryLoopCount +=
                expectedRoots.Count - matchedRoots.Count;
            result.UnexpectedBoundaryLoopDetails =
                unexpectedDetails.ToString();
        }

        private static void ValidateEmbeddedRoots(
            TreeDefinition definition,
            IReadOnlyList<TreeBarkMeshBranchAuditRecord> records,
            TreeBarkMeshTopologyAuditResult result)
        {
            IReadOnlyList<TreeBranchDefinition> branches = definition.Branches;
            for (int index = 0; index < records.Count; index++)
            {
                TreeBarkMeshBranchAuditRecord record = records[index];
                TreeBranchDefinition branch = record.Branch;
                if (branch.ParentBranchIndex < 0 ||
                    branch.ParentBranchIndex >= branches.Count)
                {
                    continue;
                }

                ParentFrame frame = EvaluateFrame(
                    branches[branch.ParentBranchIndex],
                    branch.ParentAttachmentDistance);
                Vector3 delta = record.RootCenter - frame.Position;
                Vector3 radial = Vector3.ProjectOnPlane(delta, frame.Tangent);
                if (Vector3.Dot(radial, branch.LocalReferenceAxis) < -0.0001f)
                {
                    result.OppositeSideRootEmergenceCount++;
                }

                float tolerance = Mathf.Max(0.001f, frame.Radius * 0.025f);
                if (radial.magnitude + record.RootRadius > frame.Radius + tolerance)
                {
                    result.RootLoopOutsideParentCount++;
                }
            }

        }

        private static Dictionary<int, int> BuildWeldedVertexIds(
            IReadOnlyList<Vector3> vertices,
            out Dictionary<int, Vector3> weldedPositions)
        {
            // A single rounded-cell lookup is not a tolerance weld: two
            // position-coincident seam vertices can fall on opposite sides of
            // a quantization boundary. Search neighbouring cells and verify
            // the actual Euclidean distance before reusing a welded ID.
            var buckets = new Dictionary<PositionKey, List<int>>();
            var result = new Dictionary<int, int>(vertices.Count);
            weldedPositions = new Dictionary<int, Vector3>();
            float toleranceSquared =
                PositionWeldTolerance * PositionWeldTolerance;
            int nextId = 0;
            for (int index = 0; index < vertices.Count; index++)
            {
                Vector3 position = vertices[index];
                var key = new PositionKey(position);
                int weldedId = -1;
                float bestDistanceSquared = float.PositiveInfinity;
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        for (int z = -1; z <= 1; z++)
                        {
                            PositionKey neighbourKey = key.Offset(x, y, z);
                            if (!buckets.TryGetValue(
                                    neighbourKey,
                                    out List<int> candidates))
                            {
                                continue;
                            }

                            for (int candidateIndex = 0;
                                 candidateIndex < candidates.Count;
                                 candidateIndex++)
                            {
                                int candidateId = candidates[candidateIndex];
                                float distanceSquared =
                                    (weldedPositions[candidateId] - position)
                                    .sqrMagnitude;
                                if (distanceSquared > toleranceSquared)
                                {
                                    continue;
                                }

                                if (distanceSquared < bestDistanceSquared ||
                                    (Mathf.Approximately(
                                         distanceSquared,
                                         bestDistanceSquared) &&
                                     candidateId < weldedId))
                                {
                                    weldedId = candidateId;
                                    bestDistanceSquared = distanceSquared;
                                }
                            }
                        }
                    }
                }

                if (weldedId < 0)
                {
                    weldedId = nextId++;
                    weldedPositions.Add(weldedId, position);
                    if (!buckets.TryGetValue(
                            key,
                            out List<int> cellCandidates))
                    {
                        cellCandidates = new List<int>();
                        buckets.Add(key, cellCandidates);
                    }

                    cellCandidates.Add(weldedId);
                }

                result[index] = weldedId;
            }

            return result;
        }

        private static void CollectBoundaryComponents(
            IReadOnlyList<EdgeKey> edges,
            out List<List<int>> closedLoops,
            out int openComponents)
        {
            closedLoops = new List<List<int>>();
            openComponents = 0;
            var adjacency = new Dictionary<int, List<int>>();
            for (int index = 0; index < edges.Count; index++)
            {
                EdgeKey edge = edges[index];
                AddNeighbour(adjacency, edge.A, edge.B);
                AddNeighbour(adjacency, edge.B, edge.A);
            }

            var visited = new HashSet<int>();
            foreach (KeyValuePair<int, List<int>> pair in adjacency)
            {
                if (!visited.Add(pair.Key))
                {
                    continue;
                }

                bool closed = true;
                var component = new List<int>();
                var stack = new Stack<int>();
                stack.Push(pair.Key);
                while (stack.Count > 0)
                {
                    int vertex = stack.Pop();
                    component.Add(vertex);
                    List<int> neighbours = adjacency[vertex];
                    if (neighbours.Count != 2)
                    {
                        closed = false;
                    }

                    for (int neighbourIndex = 0;
                         neighbourIndex < neighbours.Count;
                         neighbourIndex++)
                    {
                        int neighbour = neighbours[neighbourIndex];
                        if (visited.Add(neighbour))
                        {
                            stack.Push(neighbour);
                        }
                    }
                }

                if (closed)
                {
                    closedLoops.Add(component);
                }
                else
                {
                    openComponents++;
                }
            }
        }

        private static void AddNeighbour(
            Dictionary<int, List<int>> adjacency,
            int from,
            int to)
        {
            if (!adjacency.TryGetValue(from, out List<int> neighbours))
            {
                neighbours = new List<int>();
                adjacency.Add(from, neighbours);
            }

            neighbours.Add(to);
        }

        private static void AddEdge(
            Dictionary<EdgeKey, int> counts,
            int a,
            int b)
        {
            var key = new EdgeKey(a, b);
            counts.TryGetValue(key, out int count);
            counts[key] = count + 1;
        }

        private static bool IsValidIndex(int value, int count)
        {
            return value >= 0 && value < count;
        }

        private static bool IsFinite(Vector3 value)
        {
            return TreeDeterministicUtility.IsFinite(value.x) &&
                TreeDeterministicUtility.IsFinite(value.y) &&
                TreeDeterministicUtility.IsFinite(value.z);
        }

        private static bool IsFinite(Vector4 value)
        {
            return TreeDeterministicUtility.IsFinite(value.x) &&
                TreeDeterministicUtility.IsFinite(value.y) &&
                TreeDeterministicUtility.IsFinite(value.z) &&
                TreeDeterministicUtility.IsFinite(value.w);
        }

        private static ParentFrame EvaluateFrame(
            TreeBranchDefinition branch,
            float normalizedDistance)
        {
            IReadOnlyList<TreeCurveSample> samples = branch.Samples;
            float scaled = Mathf.Clamp01(normalizedDistance) * (samples.Count - 1);
            int lower = Mathf.Clamp(Mathf.FloorToInt(scaled), 0, samples.Count - 1);
            int upper = Mathf.Min(samples.Count - 1, lower + 1);
            float t = scaled - lower;
            TreeCurveSample a = samples[lower];
            TreeCurveSample b = samples[upper];
            Vector3 tangent = Vector3.Slerp(a.Tangent, b.Tangent, t).normalized;
            Vector3 normal = Vector3.Slerp(a.Normal, b.Normal, t);
            normal = Vector3.ProjectOnPlane(normal, tangent).normalized;
            return new ParentFrame
            {
                Position = Vector3.Lerp(a.Position, b.Position, t),
                Tangent = tangent,
                Radius = Mathf.Lerp(a.Radius, b.Radius, t)
            };
        }

        private static string BuildReport(TreeBarkMeshTopologyAuditResult result)
        {
            var report = new StringBuilder(2048);
            report.AppendLine("[TREE-GEN.2B Bark Topology Audit]");
            report.Append("Finite positions/normals/tangents/uv failures: ")
                .Append(result.NonFiniteVertexCount).Append("/")
                .Append(result.NonFiniteNormalCount).Append("/")
                .Append(result.NonFiniteTangentCount).Append("/")
                .AppendLine(result.NonFiniteUvCount.ToString());
            report.Append("Invalid indices / degenerate triangles / zero-length ring segments: ")
                .Append(result.InvalidIndexCount).Append(" / ")
                .Append(result.DegenerateTriangleCount).Append(" / ")
                .AppendLine(result.ZeroLengthRingSegmentCount.ToString());
            report.Append("Inward side / weak side / cap orientation failures: ")
                .Append(result.InwardSideTriangleCount).Append(" / ")
                .Append(result.SideOrientationFailureCount).Append(" / ")
                .AppendLine(result.CapOrientationFailureCount.ToString());
            report.Append("Tangent-basis failures: ")
                .AppendLine(result.TangentBasisFailureCount.ToString());
            report.Append("Raw boundary edges / welded seam edges: ")
                .Append(result.RawBoundaryEdgeCount).Append(" / ")
                .AppendLine(result.PositionWeldedSeamEdgeCount.ToString());
            report.Append("Expected/matched embedded roots / welded boundary loops / unexpected loops: ")
                .Append(result.ExpectedEmbeddedRootLoopCount).Append("/")
                .Append(result.MatchedEmbeddedRootLoopCount).Append(" / ")
                .Append(result.PositionWeldedBoundaryLoopCount).Append(" / ")
                .AppendLine(result.UnexpectedExposedBoundaryLoopCount.ToString());
            report.Append("Non-manifold / opposite-side roots / roots outside parent: ")
                .Append(result.NonManifoldEdgeCount).Append(" / ")
                .Append(result.OppositeSideRootEmergenceCount).Append(" / ")
                .AppendLine(result.RootLoopOutsideParentCount.ToString());
            if (!string.IsNullOrEmpty(result.InwardSideTriangleDetails))
            {
                report.AppendLine("Inward side triangle details:");
                report.Append(result.InwardSideTriangleDetails);
            }
            if (!string.IsNullOrEmpty(result.UnexpectedBoundaryLoopDetails))
            {
                report.AppendLine("Unexpected boundary loop details:");
                report.Append(result.UnexpectedBoundaryLoopDetails);
            }

            report.Append("Status: ").AppendLine(result.Passed ? "PASS" : "FAIL");
            return report.ToString();
        }

        private struct ParentFrame
        {
            internal Vector3 Position;
            internal Vector3 Tangent;
            internal float Radius;
        }
    }
}
