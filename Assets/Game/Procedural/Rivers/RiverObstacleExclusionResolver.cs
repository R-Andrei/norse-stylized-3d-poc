using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    /// <summary>
    /// One full-resolution Foam texel whose displayed rectangle can be tested
    /// against cached exact-mesh solid intervals at runtime.
    /// </summary>
    public readonly struct RiverObstacleExclusionCell
    {
        public RiverObstacleExclusionCell(
            Vector2Int coordinate,
            int intervalOffset)
        {
            Coordinate = coordinate;
            IntervalOffset = intervalOffset;
        }

        public Vector2Int Coordinate { get; }
        public int IntervalOffset { get; }
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct RiverObstacleExclusionSample
    {
        // x/y = first solid interval, z/w = optional second interval.
        public Vector4 Intervals;

        // x = global river distance, y = lateral metres,
        // z = visible half-width, w = generated surface half-width.
        public Vector4 WaterParameters;
    }

    /// <summary>
    /// Exact-mesh preparation baker for the Stage 6 Obstacle Footprint.
    ///
    /// The current development fallback runs during staged pre-gameplay Foam
    /// preparation. The production owner should be the future procedural chunk
    /// generation/building/linking phase, after generated objects have their
    /// final placement, with the compact result cached beside the chunk/run.
    ///
    /// For every conservative sample inside a candidate Foam texel, the baker
    /// intersects a line through the actual transformed generated mesh along
    /// the local river Up direction. The resulting solid height intervals are
    /// cached relative to that sample's base water surface. Ordinary gameplay
    /// never scans triangles; it only compares the current Stage 3 displacement
    /// against these cached intervals.
    ///
    /// IMPORTANT FUTURE REFACTOR: Static Pressure still performs its own
    /// height-slice/contour scan. It should eventually derive its directional
    /// pressure envelope from this same exact solid-volume source data and then
    /// apply Pressure-specific shaping. Do not introduce a third mesh scanner.
    /// </summary>
    public static class RiverObstacleExclusionResolver
    {
        public const int SamplesPerAxis = 3;
        public const int SamplesPerCell = SamplesPerAxis * SamplesPerAxis;

        private const int MaximumStoredIntervalsPerSample = 2;
        private const float SampleEdgeInset = 0.001f;
        private const float TriangleIntersectionEpsilon = 0.000001f;
        private const float IntersectionMergeDistance = 0.0005f;
        private const float VerticalIntervalInset = 0.0015f;
        private const float MinimumIntervalThickness = 0.001f;

        private static readonly float[] SampleOffsets =
        {
            SampleEdgeInset,
            0.5f,
            1f - SampleEdgeInset
        };

        /// <summary>
        /// Appends conservative exact-mesh cells and their per-sample intervals.
        /// Each appended cell owns exactly SamplesPerCell consecutive float4
        /// entries in sampleOutput. X/Y are the first interval; Z/W are an
        /// optional second interval. Missing second intervals use Z > W.
        /// </summary>
        public static bool TryBake(
            StylizedRiver river,
            MeshFilter meshFilter,
            int fieldWidth,
            int fieldHeight,
            float fieldLength,
            List<RiverObstacleExclusionCell> cellOutput,
            List<RiverObstacleExclusionSample> sampleOutput,
            out string status)
        {
            if (river == null ||
                meshFilter == null ||
                meshFilter.sharedMesh == null ||
                cellOutput == null ||
                sampleOutput == null ||
                fieldWidth < 1 ||
                fieldHeight < 1 ||
                fieldLength <= 0.0001f ||
                !river.Domain.IsValid)
            {
                status = "A valid river, readable generated mesh, Foam field, and output lists are required.";
                return false;
            }

            if (!TryReadWorldMesh(
                    meshFilter,
                    out Vector3[] worldVertices,
                    out int[] triangles,
                    out string meshStatus))
            {
                status = meshStatus;
                return false;
            }

            if (!TryResolveCandidateRange(
                    river,
                    worldVertices,
                    fieldWidth,
                    fieldHeight,
                    fieldLength,
                    out int minimumX,
                    out int maximumX,
                    out float minimumAcross,
                    out float maximumAcross))
            {
                status = "The generated mesh could not be projected into this river's Foam field.";
                return false;
            }

            int initialCellCount = cellOutput.Count;
            int initialSampleCount = sampleOutput.Count;
            RiverObstacleExclusionSample[] cellSamples =
                new RiverObstacleExclusionSample[SamplesPerCell];
            // Reused across the complete one-time bake so exact triangle
            // intersection does not allocate temporary lists per sample.
            List<float> intersectionScratch = new(16);
            List<float> uniqueIntersectionScratch = new(16);

            for (int x = minimumX; x <= maximumX; x++)
            {
                float centreU = (x + 0.5f) / Mathf.Max(1f, fieldWidth);
                float centreGlobalDistance =
                    river.Domain.GlobalDistanceMinimum +
                    centreU * fieldLength;
                StylizedRiverSplineSample centreSample =
                    river.Domain.SampleAtGlobalDistance(
                        Mathf.Clamp(
                            centreGlobalDistance,
                            river.Domain.GlobalDistanceMinimum,
                            river.Domain.GlobalDistanceMaximum));
                float minimumAcross01 = AcrossMetresTo01(
                    minimumAcross,
                    centreSample.LeftSurfaceHalfWidth,
                    centreSample.RightSurfaceHalfWidth);
                float maximumAcross01 = AcrossMetresTo01(
                    maximumAcross,
                    centreSample.LeftSurfaceHalfWidth,
                    centreSample.RightSurfaceHalfWidth);
                int minimumY = Mathf.Clamp(
                    Mathf.FloorToInt(
                        Mathf.Min(minimumAcross01, maximumAcross01) *
                        fieldHeight) - 1,
                    0,
                    fieldHeight - 1);
                int maximumY = Mathf.Clamp(
                    Mathf.CeilToInt(
                        Mathf.Max(minimumAcross01, maximumAcross01) *
                        fieldHeight) + 1,
                    0,
                    fieldHeight - 1);

                for (int y = minimumY; y <= maximumY; y++)
                {
                    bool allSamplesHaveSolidIntervals = true;
                    int sampleIndex = 0;

                    for (int sampleY = 0;
                         sampleY < SamplesPerAxis &&
                         allSamplesHaveSolidIntervals;
                         sampleY++)
                    {
                        for (int sampleX = 0;
                             sampleX < SamplesPerAxis;
                             sampleX++)
                        {
                            float u = (x + SampleOffsets[sampleX]) /
                                Mathf.Max(1f, fieldWidth);
                            float v = (y + SampleOffsets[sampleY]) /
                                Mathf.Max(1f, fieldHeight);

                            if (!TryResolveBaseSurfaceSample(
                                    river,
                                    fieldLength,
                                    u,
                                    v,
                                    out Vector3 basePoint,
                                    out Vector3 up,
                                    out Vector4 waterParameters) ||
                                !TryResolveSolidIntervals(
                                    basePoint,
                                    up,
                                    worldVertices,
                                    triangles,
                                    intersectionScratch,
                                    uniqueIntersectionScratch,
                                    out Vector4 intervals))
                            {
                                allSamplesHaveSolidIntervals = false;
                                break;
                            }

                            cellSamples[sampleIndex++] =
                                new RiverObstacleExclusionSample
                                {
                                    Intervals = intervals,
                                    WaterParameters = waterParameters
                                };
                        }
                    }

                    if (!allSamplesHaveSolidIntervals ||
                        sampleIndex != SamplesPerCell)
                    {
                        continue;
                    }

                    int sampleOffset = sampleOutput.Count;
                    for (int index = 0; index < SamplesPerCell; index++)
                    {
                        sampleOutput.Add(cellSamples[index]);
                    }

                    cellOutput.Add(new RiverObstacleExclusionCell(
                        new Vector2Int(x, y),
                        sampleOffset));
                }
            }

            int addedCells = cellOutput.Count - initialCellCount;
            if (addedCells <= 0)
            {
                if (sampleOutput.Count > initialSampleCount)
                {
                    sampleOutput.RemoveRange(
                        initialSampleCount,
                        sampleOutput.Count - initialSampleCount);
                }

                status = "The exact mesh produced no conservative full-texel obstacle cells at the current Foam resolution.";
                return false;
            }

            status =
                $"Baked {addedCells} conservative full-resolution obstacle cells from the exact transformed generated mesh.";
            return true;
        }

        private static bool TryReadWorldMesh(
            MeshFilter meshFilter,
            out Vector3[] worldVertices,
            out int[] triangles,
            out string status)
        {
            worldVertices = Array.Empty<Vector3>();
            triangles = Array.Empty<int>();

            Mesh mesh = meshFilter.sharedMesh;
            if (mesh == null)
            {
                status = "The generated MeshFilter has no mesh.";
                return false;
            }

            try
            {
                Vector3[] localVertices = mesh.vertices;
                triangles = mesh.triangles;
                if (localVertices == null ||
                    localVertices.Length < 3 ||
                    triangles == null ||
                    triangles.Length < 3)
                {
                    status = "The generated mesh does not contain readable triangle geometry.";
                    return false;
                }

                Transform meshTransform = meshFilter.transform;
                worldVertices = new Vector3[localVertices.Length];
                for (int index = 0; index < localVertices.Length; index++)
                {
                    worldVertices[index] =
                        meshTransform.TransformPoint(localVertices[index]);
                }
            }
            catch (UnityException exception)
            {
                status =
                    $"The generated mesh is not CPU-readable: {exception.Message}";
                return false;
            }

            status = "Read exact transformed generated-mesh triangles.";
            return true;
        }

        private static bool TryResolveCandidateRange(
            StylizedRiver river,
            IReadOnlyList<Vector3> worldVertices,
            int fieldWidth,
            int fieldHeight,
            float fieldLength,
            out int minimumX,
            out int maximumX,
            out float minimumAcross,
            out float maximumAcross)
        {
            minimumX = 0;
            maximumX = -1;
            minimumAcross = float.PositiveInfinity;
            maximumAcross = float.NegativeInfinity;
            float minimumGlobal = float.PositiveInfinity;
            float maximumGlobal = float.NegativeInfinity;

            for (int index = 0; index < worldVertices.Count; index++)
            {
                if (!river.TryProjectWorldPoint(
                        worldVertices[index],
                        out StylizedRiverProjection projection))
                {
                    continue;
                }

                minimumGlobal = Mathf.Min(
                    minimumGlobal,
                    projection.GlobalDistance);
                maximumGlobal = Mathf.Max(
                    maximumGlobal,
                    projection.GlobalDistance);
                minimumAcross = Mathf.Min(
                    minimumAcross,
                    projection.AcrossMetres);
                maximumAcross = Mathf.Max(
                    maximumAcross,
                    projection.AcrossMetres);
            }

            if (float.IsInfinity(minimumGlobal) ||
                float.IsInfinity(maximumGlobal) ||
                float.IsInfinity(minimumAcross) ||
                float.IsInfinity(maximumAcross))
            {
                return false;
            }

            float globalStart = river.Domain.GlobalDistanceMinimum;
            minimumX = Mathf.Clamp(
                Mathf.FloorToInt(
                    (minimumGlobal - globalStart) /
                    fieldLength * fieldWidth) - 1,
                0,
                fieldWidth - 1);
            maximumX = Mathf.Clamp(
                Mathf.CeilToInt(
                    (maximumGlobal - globalStart) /
                    fieldLength * fieldWidth) + 1,
                0,
                fieldWidth - 1);

            return maximumX >= minimumX && fieldHeight > 0;
        }

        private static bool TryResolveBaseSurfaceSample(
            StylizedRiver river,
            float fieldLength,
            float u,
            float v,
            out Vector3 basePoint,
            out Vector3 up,
            out Vector4 waterParameters)
        {
            basePoint = default;
            up = Vector3.up;
            waterParameters = default;

            u = Mathf.Clamp01(u);
            v = Mathf.Clamp01(v);
            float globalDistance =
                river.Domain.GlobalDistanceMinimum + u * fieldLength;
            if (globalDistance < river.Domain.GlobalDistanceMinimum - 0.0001f ||
                globalDistance > river.Domain.GlobalDistanceMaximum + 0.0001f)
            {
                return false;
            }

            StylizedRiverSplineSample sample =
                river.Domain.SampleAtGlobalDistance(
                    Mathf.Clamp(
                        globalDistance,
                        river.Domain.GlobalDistanceMinimum,
                        river.Domain.GlobalDistanceMaximum));
            float acrossMetres = Across01ToMetres(
                v,
                sample.LeftSurfaceHalfWidth,
                sample.RightSurfaceHalfWidth);
            up = sample.Up.sqrMagnitude > 0.0001f
                ? sample.Up.normalized
                : Vector3.up;
            basePoint = sample.SurfacePoint + sample.Side * acrossMetres;
            float visibleHalfWidth = acrossMetres <= 0f
                ? sample.LeftHalfWidth
                : sample.RightHalfWidth;
            float surfaceHalfWidth = acrossMetres <= 0f
                ? sample.LeftSurfaceHalfWidth
                : sample.RightSurfaceHalfWidth;
            waterParameters = new Vector4(
                globalDistance,
                acrossMetres,
                visibleHalfWidth,
                surfaceHalfWidth);
            return true;
        }

        private static bool TryResolveSolidIntervals(
            Vector3 lineOrigin,
            Vector3 lineDirection,
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<int> triangles,
            List<float> intersections,
            List<float> uniqueIntersections,
            out Vector4 intervals)
        {
            intervals = new Vector4(0f, -1f, 0f, -1f);
            intersections.Clear();
            uniqueIntersections.Clear();

            for (int triangle = 0;
                 triangle + 2 < triangles.Count;
                 triangle += 3)
            {
                int index0 = triangles[triangle];
                int index1 = triangles[triangle + 1];
                int index2 = triangles[triangle + 2];
                if (index0 < 0 || index0 >= vertices.Count ||
                    index1 < 0 || index1 >= vertices.Count ||
                    index2 < 0 || index2 >= vertices.Count)
                {
                    continue;
                }

                if (TryIntersectInfiniteLineTriangle(
                        lineOrigin,
                        lineDirection,
                        vertices[index0],
                        vertices[index1],
                        vertices[index2],
                        out float lineHeight))
                {
                    intersections.Add(lineHeight);
                }
            }

            if (intersections.Count < 2)
            {
                return false;
            }

            intersections.Sort();
            for (int index = 0; index < intersections.Count; index++)
            {
                float value = intersections[index];
                if (uniqueIntersections.Count == 0 ||
                    Mathf.Abs(
                        value -
                        uniqueIntersections[uniqueIntersections.Count - 1]) >
                    IntersectionMergeDistance)
                {
                    uniqueIntersections.Add(value);
                }
            }

            // A closed mesh must enter and leave the solid in pairs. An odd
            // count signals an edge/tangent/open-mesh ambiguity; reject that
            // sample rather than inventing an interval that could protrude.
            if (uniqueIntersections.Count < 2 ||
                (uniqueIntersections.Count & 1) != 0)
            {
                return false;
            }

            float firstLower = 0f;
            float firstUpper = -1f;
            float secondLower = 0f;
            float secondUpper = -1f;
            int storedIntervalCount = 0;
            for (int index = 0;
                 index + 1 < uniqueIntersections.Count &&
                 storedIntervalCount < MaximumStoredIntervalsPerSample;
                 index += 2)
            {
                float lower =
                    uniqueIntersections[index] + VerticalIntervalInset;
                float upper =
                    uniqueIntersections[index + 1] - VerticalIntervalInset;
                if (upper <= lower + MinimumIntervalThickness)
                {
                    continue;
                }

                if (storedIntervalCount == 0)
                {
                    firstLower = lower;
                    firstUpper = upper;
                }
                else
                {
                    secondLower = lower;
                    secondUpper = upper;
                }

                storedIntervalCount++;
            }

            if (storedIntervalCount <= 0)
            {
                return false;
            }

            intervals = new Vector4(
                firstLower,
                firstUpper,
                secondLower,
                secondUpper);
            return true;
        }

        private static bool TryIntersectInfiniteLineTriangle(
            Vector3 lineOrigin,
            Vector3 lineDirection,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            out float lineHeight)
        {
            lineHeight = 0f;
            Vector3 edge1 = b - a;
            Vector3 edge2 = c - a;
            Vector3 p = Vector3.Cross(lineDirection, edge2);
            float determinant = Vector3.Dot(edge1, p);
            if (Mathf.Abs(determinant) <= TriangleIntersectionEpsilon)
            {
                return false;
            }

            float inverseDeterminant = 1f / determinant;
            Vector3 t = lineOrigin - a;
            float barycentricU = Vector3.Dot(t, p) * inverseDeterminant;
            if (barycentricU < -TriangleIntersectionEpsilon ||
                barycentricU > 1f + TriangleIntersectionEpsilon)
            {
                return false;
            }

            Vector3 q = Vector3.Cross(t, edge1);
            float barycentricV =
                Vector3.Dot(lineDirection, q) * inverseDeterminant;
            if (barycentricV < -TriangleIntersectionEpsilon ||
                barycentricU + barycentricV >
                1f + TriangleIntersectionEpsilon)
            {
                return false;
            }

            lineHeight = Vector3.Dot(edge2, q) * inverseDeterminant;
            return true;
        }

        private static float Across01ToMetres(
            float across01,
            float leftHalfWidth,
            float rightHalfWidth)
        {
            if (across01 <= 0.5f)
            {
                return -leftHalfWidth * (1f - across01 * 2f);
            }

            return rightHalfWidth * (across01 * 2f - 1f);
        }

        private static float AcrossMetresTo01(
            float acrossMetres,
            float leftHalfWidth,
            float rightHalfWidth)
        {
            if (acrossMetres <= 0f)
            {
                return Mathf.Clamp01(
                    0.5f + acrossMetres /
                    Mathf.Max(0.001f, leftHalfWidth * 2f));
            }

            return Mathf.Clamp01(
                0.5f + acrossMetres /
                Mathf.Max(0.001f, rightHalfWidth * 2f));
        }
    }
}
