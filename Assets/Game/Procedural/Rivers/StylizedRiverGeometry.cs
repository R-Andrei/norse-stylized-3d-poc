using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines;

namespace ProgrammaticStylized3D.Rivers
{
    public readonly struct StylizedRiverSplineSample
    {
        public StylizedRiverSplineSample(
            Vector3 centre,
            Vector3 tangent,
            Vector3 side,
            float distance,
            float normalizedTime)
        {
            Centre = centre;
            Tangent = tangent;
            Side = side;
            Distance = distance;
            NormalizedTime = normalizedTime;
        }

        public Vector3 Centre { get; }
        public Vector3 Tangent { get; }
        public Vector3 Side { get; }
        public float Distance { get; }
        public float NormalizedTime { get; }
    }

    public readonly struct StylizedRiverProjection
    {
        public StylizedRiverProjection(
            Vector3 centre,
            Vector3 tangent,
            Vector3 side,
            float distanceAlong,
            float acrossDistance)
        {
            Centre = centre;
            Tangent = tangent;
            Side = side;
            DistanceAlong = distanceAlong;
            AcrossDistance = acrossDistance;
        }

        public Vector3 Centre { get; }
        public Vector3 Tangent { get; }
        public Vector3 Side { get; }
        public float DistanceAlong { get; }
        public float AcrossDistance { get; }
    }

    public static class StylizedRiverGeometry
    {
        public static float BuildSplineSamples(
            SplineContainer container,
            float targetSpacing,
            List<StylizedRiverSplineSample> samples)
        {
            if (samples == null)
            {
                throw new ArgumentNullException(nameof(samples));
            }

            samples.Clear();

            if (container == null ||
                container.Splines.Count == 0)
            {
                return 0f;
            }

            float approximateLength =
                Mathf.Max(0.01f, container.CalculateLength());

            int sampleCount =
                Mathf.Max(
                    2,
                    Mathf.CeilToInt(
                        approximateLength /
                        Mathf.Max(0.05f, targetSpacing)) + 1);

            // This local cumulative spline distance is correct for one authored river slice. Future
            // procedural assembly should add a global distance offset per connected segment, and reversed
            // segments should use segmentLength - localDistance. Current-ribbon placement may later adopt
            // the same coordinate so distribution does not reset at chunk transitions.
            float cumulativeDistance = 0f;
            Vector3 previousCentre = Vector3.zero;
            Vector3 previousTangent = Vector3.forward;

            for (int index = 0; index < sampleCount; index++)
            {
                float t =
                    index / (float)(sampleCount - 1);

                float3 positionValue =
                    container.EvaluatePosition(t);

                float3 tangentValue =
                    container.EvaluateTangent(t);

                Vector3 centre =
                    new Vector3(
                        positionValue.x,
                        positionValue.y,
                        positionValue.z);

                Vector3 tangent =
                    new Vector3(
                        tangentValue.x,
                        0f,
                        tangentValue.z);

                if (tangent.sqrMagnitude < 0.000001f)
                {
                    tangent = previousTangent;
                }
                else
                {
                    tangent.Normalize();
                }

                previousTangent = tangent;

                Vector3 side =
                    Vector3.Cross(Vector3.up, tangent).normalized;

                if (index > 0)
                {
                    cumulativeDistance +=
                        Vector3.Distance(previousCentre, centre);
                }

                previousCentre = centre;

                samples.Add(
                    new StylizedRiverSplineSample(
                        centre,
                        tangent,
                        side,
                        cumulativeDistance,
                        t));
            }

            return cumulativeDistance;
        }

        public static void BuildSurfaceMesh(
            Transform owner,
            IReadOnlyList<StylizedRiverSplineSample> samples,
            float width,
            float edgeOverlap,
            int crossSegments,
            float surfaceOffset,
            Mesh mesh)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            if (mesh == null)
            {
                throw new ArgumentNullException(nameof(mesh));
            }

            mesh.Clear();

            if (samples == null ||
                samples.Count < 2)
            {
                return;
            }

            int acrossVertexCount =
                Mathf.Max(2, crossSegments + 1);

            int rowCount = samples.Count;
            int vertexCount = rowCount * acrossVertexCount;
            int triangleIndexCount =
                (rowCount - 1) *
                (acrossVertexCount - 1) *
                6;

            Vector3[] vertices = new Vector3[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            Vector4[] tangents = new Vector4[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            Color[] colors = new Color[vertexCount];
            int[] triangles = new int[triangleIndexCount];

            float halfWidth =
                Mathf.Max(0.25f, width * 0.5f + edgeOverlap);

            Vector3 localUp =
                owner.InverseTransformDirection(Vector3.up).normalized;

            for (int row = 0; row < rowCount; row++)
            {
                StylizedRiverSplineSample sample = samples[row];

                Vector3 localTangent =
                    owner.InverseTransformDirection(sample.Tangent).normalized;

                for (int acrossIndex = 0; acrossIndex < acrossVertexCount; acrossIndex++)
                {
                    float across01 =
                        acrossIndex / (float)(acrossVertexCount - 1);

                    float acrossSigned = across01 * 2f - 1f;

                    Vector3 worldPosition =
                        sample.Centre +
                        sample.Side * (acrossSigned * halfWidth) +
                        Vector3.up * surfaceOffset;

                    int vertexIndex = row * acrossVertexCount + acrossIndex;

                    vertices[vertexIndex] =
                        owner.InverseTransformPoint(worldPosition);

                    normals[vertexIndex] = localUp;

                    tangents[vertexIndex] =
                        new Vector4(
                            localTangent.x,
                            localTangent.y,
                            localTangent.z,
                            1f);

                    uvs[vertexIndex] =
                        new Vector2(across01, sample.Distance);

                    colors[vertexIndex] =
                        new Color(across01, sample.NormalizedTime, 0f, 1f);
                }
            }

            int triangleCursor = 0;

            for (int row = 0; row < rowCount - 1; row++)
            {
                for (int acrossIndex = 0; acrossIndex < acrossVertexCount - 1; acrossIndex++)
                {
                    int a = row * acrossVertexCount + acrossIndex;
                    int b = a + 1;
                    int c = a + acrossVertexCount;
                    int d = c + 1;

                    triangles[triangleCursor++] = a;
                    triangles[triangleCursor++] = c;
                    triangles[triangleCursor++] = b;

                    triangles[triangleCursor++] = b;
                    triangles[triangleCursor++] = c;
                    triangles[triangleCursor++] = d;
                }
            }

            mesh.indexFormat =
                vertexCount > 65535
                    ? IndexFormat.UInt32
                    : IndexFormat.UInt16;

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.tangents = tangents;
            mesh.uv = uvs;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
        }

        public static void BuildCurrentAccentMesh(
            Transform owner,
            IReadOnlyList<StylizedRiverSplineSample> samples,
            float riverLength,
            float riverWidth,
            float yOffset,
            float density,
            float length,
            float lineWidth,
            float curvature,
            float speed,
            float animationDistance,
            int seed,
            int rowsPerAccent,
            Mesh mesh)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            if (mesh == null)
            {
                throw new ArgumentNullException(nameof(mesh));
            }

            mesh.Clear();

            if (samples == null ||
                samples.Count < 2 ||
                riverLength <= 0.01f ||
                density <= 0.001f ||
                length <= 0.05f ||
                lineWidth <= 0.005f)
            {
                return;
            }

            RibbonMeshBuilder builder = new RibbonMeshBuilder();

            float safeDensity = Mathf.Max(0.05f, density);
            int accentCount = Mathf.Clamp(
                Mathf.CeilToInt(riverLength * safeDensity),
                1,
                512);

            float safeLength = Mathf.Max(0.05f, length);
            float halfWidth = Mathf.Max(0.1f, riverWidth * 0.5f);
            float usableHalfWidth = Mathf.Max(0.05f, halfWidth - lineWidth * 1.5f);
            float loopLength = riverLength + safeLength * 1.35f;
            int safeRows = Mathf.Max(2, rowsPerAccent);

            for (int accentIndex = 0; accentIndex < accentCount; accentIndex++)
            {
                float baseOffset =
                    Hash01(seed, accentIndex, 11) * loopLength;

                float speedScale =
                    Mathf.Lerp(0.82f, 1.18f, Hash01(seed, accentIndex, 23));

                float accentLength =
                    safeLength * Mathf.Lerp(0.72f, 1.28f, Hash01(seed, accentIndex, 31));

                float accentWidth =
                    lineWidth * Mathf.Lerp(0.8f, 1.2f, Hash01(seed, accentIndex, 47));

                float acrossOffset =
                    (Hash01(seed, accentIndex, 59) * 2f - 1f) * usableHalfWidth * 0.82f;

                float phase =
                    Hash01(seed, accentIndex, 71) * Mathf.PI * 2f;

                float curveAmplitude =
                    curvature * Mathf.Lerp(0.08f, 0.35f, Hash01(seed, accentIndex, 89)) * safeLength;

                float along =
                    Mathf.Repeat(baseOffset + animationDistance * speed * speedScale, loopLength) - accentLength;

                float startDistance = along;
                float endDistance = along + accentLength;

                if (endDistance <= 0f || startDistance >= riverLength)
                {
                    continue;
                }

                float visibleStart = Mathf.Clamp(startDistance, 0f, riverLength);
                float visibleEnd = Mathf.Clamp(endDistance, 0f, riverLength);

                if (visibleEnd - visibleStart <= 0.02f)
                {
                    continue;
                }

                AddCurrentAccent(
                    owner,
                    samples,
                    yOffset,
                    visibleStart,
                    visibleEnd,
                    startDistance,
                    endDistance,
                    acrossOffset,
                    accentWidth,
                    curveAmplitude,
                    phase,
                    safeRows,
                    Hash01(seed, accentIndex, 97),
                    builder);
            }

            builder.Apply(mesh);
        }

        public static bool TryProjectPoint(
            IReadOnlyList<StylizedRiverSplineSample> samples,
            Vector3 worldPoint,
            out StylizedRiverProjection projection)
        {
            projection = default;

            if (samples == null || samples.Count < 2)
            {
                return false;
            }

            Vector2 point = new Vector2(worldPoint.x, worldPoint.z);
            float bestDistanceSqr = float.PositiveInfinity;

            for (int index = 0; index < samples.Count - 1; index++)
            {
                StylizedRiverSplineSample a = samples[index];
                StylizedRiverSplineSample b = samples[index + 1];

                Vector2 a2 = new Vector2(a.Centre.x, a.Centre.z);
                Vector2 b2 = new Vector2(b.Centre.x, b.Centre.z);
                Vector2 segment = b2 - a2;
                float lengthSqr = segment.sqrMagnitude;

                float t =
                    lengthSqr > 0.000001f
                        ? Mathf.Clamp01(Vector2.Dot(point - a2, segment) / lengthSqr)
                        : 0f;

                Vector2 nearest = a2 + segment * t;
                float distanceSqr = (point - nearest).sqrMagnitude;

                if (distanceSqr >= bestDistanceSqr)
                {
                    continue;
                }

                bestDistanceSqr = distanceSqr;

                Vector3 centre = Vector3.Lerp(a.Centre, b.Centre, t);
                Vector3 tangent = Vector3.Slerp(a.Tangent, b.Tangent, t).normalized;
                Vector3 side = Vector3.Cross(Vector3.up, tangent).normalized;
                Vector3 delta = worldPoint - centre;

                projection =
                    new StylizedRiverProjection(
                        centre,
                        tangent,
                        side,
                        Mathf.Lerp(a.Distance, b.Distance, t),
                        Vector3.Dot(delta, side));
            }

            return !float.IsPositiveInfinity(bestDistanceSqr);
        }

        public static StylizedRiverSplineSample SampleAtDistance(
            IReadOnlyList<StylizedRiverSplineSample> samples,
            float distance)
        {
            if (samples == null || samples.Count == 0)
            {
                return default;
            }

            if (distance <= 0f)
            {
                return samples[0];
            }

            StylizedRiverSplineSample last = samples[samples.Count - 1];

            if (distance >= last.Distance)
            {
                return last;
            }

            for (int index = 0; index < samples.Count - 1; index++)
            {
                StylizedRiverSplineSample a = samples[index];
                StylizedRiverSplineSample b = samples[index + 1];

                if (distance > b.Distance)
                {
                    continue;
                }

                float t = Mathf.InverseLerp(a.Distance, b.Distance, distance);
                Vector3 centre = Vector3.Lerp(a.Centre, b.Centre, t);
                Vector3 tangent = Vector3.Slerp(a.Tangent, b.Tangent, t).normalized;
                Vector3 side = Vector3.Cross(Vector3.up, tangent).normalized;

                return new StylizedRiverSplineSample(
                    centre,
                    tangent,
                    side,
                    distance,
                    Mathf.Lerp(a.NormalizedTime, b.NormalizedTime, t));
            }

            return last;
        }

        private static void AddCurrentAccent(
            Transform owner,
            IReadOnlyList<StylizedRiverSplineSample> samples,
            float yOffset,
            float visibleStart,
            float visibleEnd,
            float fullStart,
            float fullEnd,
            float baseAcrossOffset,
            float baseWidth,
            float curveAmplitude,
            float phase,
            int rowCount,
            float randomSeed,
            RibbonMeshBuilder builder)
        {
            int firstVertex = builder.VertexCount;

            float fullLength = Mathf.Max(0.001f, fullEnd - fullStart);

            for (int row = 0; row <= rowCount; row++)
            {
                float t = row / (float)rowCount;
                float distance = Mathf.Lerp(visibleStart, visibleEnd, t);

                StylizedRiverSplineSample sample =
                    SampleAtDistance(samples, distance);

                float fullT = Mathf.InverseLerp(fullStart, fullEnd, distance);

                float curve =
                    Mathf.Sin(fullT * Mathf.PI + phase) * curveAmplitude;

                float acrossOffset =
                    baseAcrossOffset + curve;

                float taper =
                    Mathf.Sin(fullT * Mathf.PI);

                taper = Mathf.Pow(Mathf.Clamp01(taper), 0.75f);

                float widthHere = Mathf.Max(0.006f, baseWidth * taper);

                Vector3 centre =
                    sample.Centre +
                    sample.Side * acrossOffset +
                    Vector3.up * yOffset;

                Vector3 side = sample.Side;

                Vector3 left = centre - side * widthHere * 0.5f;
                Vector3 right = centre + side * widthHere * 0.5f;

                Color metadata = new Color(randomSeed, fullT, taper, 1f);

                builder.AddVertex(owner.InverseTransformPoint(left), new Vector2(0f, fullT), metadata);
                builder.AddVertex(owner.InverseTransformPoint(right), new Vector2(1f, fullT), metadata);
            }

            for (int row = 0; row < rowCount; row++)
            {
                int baseIndex = firstVertex + row * 2;
                builder.AddQuad(baseIndex, baseIndex + 2, baseIndex + 1, baseIndex + 3);
            }
        }

        private static float Hash01(int seed, int index, int salt)
        {
            unchecked
            {
                uint hash = (uint)seed;
                hash ^= (uint)(index * 374761393);
                hash ^= (uint)(salt * 668265263);
                hash = (hash ^ (hash >> 13)) * 1274126177u;
                hash ^= hash >> 16;
                return (hash & 0x00FFFFFFu) / 16777215f;
            }
        }

        private sealed class RibbonMeshBuilder
        {
            private readonly List<Vector3> vertices = new List<Vector3>();
            private readonly List<Vector2> uvs = new List<Vector2>();
            private readonly List<Color> colors = new List<Color>();
            private readonly List<int> triangles = new List<int>();
            private readonly List<Vector3> normals = new List<Vector3>();
            private readonly List<Vector4> tangents = new List<Vector4>();

            public int VertexCount => vertices.Count;

            public void AddVertex(Vector3 position, Vector2 uv, Color color)
            {
                vertices.Add(position);
                uvs.Add(uv);
                colors.Add(color);
                normals.Add(Vector3.up);
                tangents.Add(new Vector4(1f, 0f, 0f, 1f));
            }

            public void AddQuad(int a, int b, int c, int d)
            {
                triangles.Add(a);
                triangles.Add(b);
                triangles.Add(c);

                triangles.Add(c);
                triangles.Add(b);
                triangles.Add(d);
            }

            public void Apply(Mesh mesh)
            {
                mesh.Clear();

                if (vertices.Count == 0)
                {
                    return;
                }

                mesh.indexFormat =
                    vertices.Count > 65535
                        ? IndexFormat.UInt32
                        : IndexFormat.UInt16;

                mesh.SetVertices(vertices);
                mesh.SetNormals(normals);
                mesh.SetTangents(tangents);
                mesh.SetUVs(0, uvs);
                mesh.SetColors(colors);
                mesh.SetTriangles(triangles, 0, true);
                mesh.RecalculateBounds();
            }
        }
    }
}
