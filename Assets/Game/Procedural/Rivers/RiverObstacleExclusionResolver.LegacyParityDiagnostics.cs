#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    /// <summary>
    /// Frozen pre-P5 obstacle-raster reference and exhaustive same-input parity
    /// diagnostics. This file is Editor-only and is never called by gameplay,
    /// cache startup, automatic preparation, or production topology generation.
    /// </summary>
    public static partial class RiverObstacleExclusionResolver
    {
        internal static bool TryBuildLegacyRasterParityReport(
            StylizedRiver river,
            MeshFilter meshFilter,
            StylizedRiverFoamGridDescriptor descriptor,
            out bool exact,
            out string report,
            out string error)
        {
            exact = false;
            report = string.Empty;
            error = string.Empty;
            if (river == null ||
                meshFilter == null ||
                meshFilter.sharedMesh == null ||
                !river.Domain.IsValid)
            {
                error =
                    "A valid river and readable obstacle MeshFilter are required.";
                return false;
            }

            if (descriptor.Mapping !=
                StylizedRiverFoamGridMapping.LegacyNormalizedAcross)
            {
                error =
                    "Legacy parity is defined only for the active normalized-" +
                    "lateral mapping. Fixed-metric activation has not occurred.";
                return false;
            }

            int fieldWidth = descriptor.ColumnCount;
            int fieldHeight = descriptor.RowCount;
            float fieldLength = descriptor.AllocatedLengthMetres;
            if (!TryReadWorldMesh(
                    meshFilter,
                    out Vector3[] worldVertices,
                    out int[] triangles,
                    out string meshStatus))
            {
                error = meshStatus;
                return false;
            }

            bool legacyRangeAvailable =
                TryResolveCandidateRangeLegacyReference(
                    river,
                    worldVertices,
                    fieldWidth,
                    fieldHeight,
                    fieldLength,
                    out int legacyMinimumX,
                    out int legacyMaximumX,
                    out float legacyMinimumAcross,
                    out float legacyMaximumAcross);
            bool descriptorRangeAvailable =
                TryResolveCandidateRange(
                    river,
                    worldVertices,
                    descriptor,
                    out int descriptorMinimumX,
                    out int descriptorMaximumX,
                    out float descriptorMinimumAcross,
                    out float descriptorMaximumAcross);

            List<RiverObstacleExclusionCell> legacyCells = new();
            List<RiverObstacleExclusionSample> legacySamples = new();
            bool legacyBuilt = TryBakeLegacyReference(
                river,
                meshFilter,
                fieldWidth,
                fieldHeight,
                fieldLength,
                legacyCells,
                legacySamples,
                out string legacyStatus);

            List<RiverObstacleExclusionCell> descriptorCells = new();
            List<RiverObstacleExclusionSample> descriptorSamples = new();
            bool descriptorBuilt = TryBake(
                river,
                meshFilter,
                descriptor,
                descriptorCells,
                descriptorSamples,
                out string descriptorStatus);

            StringBuilder builder = new(8192);
            builder.AppendLine("LEGACY OBSTACLE RASTER PARITY");
            builder.AppendLine($"Source: {meshFilter.name}");
            builder.AppendLine($"Mesh: {meshFilter.sharedMesh.name}");
            builder.AppendLine(
                $"Descriptor: {descriptor.Mapping} / " +
                $"{descriptor.ColumnCount}x{descriptor.RowCount} / " +
                $"allocated={descriptor.AllocatedLengthMetres:R}");
            builder.AppendLine(
                $"Reference build: {(legacyBuilt ? "SUCCESS" : "NO OUTPUT")} — " +
                legacyStatus);
            builder.AppendLine(
                $"Descriptor build: {(descriptorBuilt ? "SUCCESS" : "NO OUTPUT")} — " +
                descriptorStatus);
            builder.AppendLine(
                $"Reference range: available={legacyRangeAvailable}, " +
                $"x={legacyMinimumX}..{legacyMaximumX}, " +
                $"across={FormatFloatExact(legacyMinimumAcross)}.." +
                FormatFloatExact(legacyMaximumAcross));
            builder.AppendLine(
                $"Descriptor range: available={descriptorRangeAvailable}, " +
                $"x={descriptorMinimumX}..{descriptorMaximumX}, " +
                $"across={FormatFloatExact(descriptorMinimumAcross)}.." +
                FormatFloatExact(descriptorMaximumAcross));

            bool rangeExact =
                legacyRangeAvailable == descriptorRangeAvailable &&
                legacyMinimumX == descriptorMinimumX &&
                legacyMaximumX == descriptorMaximumX &&
                FloatBitsEqual(legacyMinimumAcross, descriptorMinimumAcross) &&
                FloatBitsEqual(legacyMaximumAcross, descriptorMaximumAcross);
            bool buildStateExact = legacyBuilt == descriptorBuilt;
            int cellMismatchCount = 0;
            int sampleMismatchCount = 0;
            string firstCellMismatch = string.Empty;
            string firstSampleMismatch = string.Empty;

            int maximumCells = Mathf.Max(
                legacyCells.Count,
                descriptorCells.Count);
            for (int index = 0; index < maximumCells; index++)
            {
                bool hasLegacy = index < legacyCells.Count;
                bool hasDescriptor = index < descriptorCells.Count;
                if (!hasLegacy || !hasDescriptor)
                {
                    cellMismatchCount++;
                    if (string.IsNullOrEmpty(firstCellMismatch))
                    {
                        firstCellMismatch =
                            $"index={index}; reference=" +
                            (hasLegacy
                                ? DescribeCell(legacyCells[index])
                                : "<missing>") +
                            "; descriptor=" +
                            (hasDescriptor
                                ? DescribeCell(descriptorCells[index])
                                : "<missing>");
                    }
                    continue;
                }

                RiverObstacleExclusionCell legacy = legacyCells[index];
                RiverObstacleExclusionCell current = descriptorCells[index];
                if (legacy.Coordinate != current.Coordinate ||
                    legacy.IntervalOffset != current.IntervalOffset)
                {
                    cellMismatchCount++;
                    if (string.IsNullOrEmpty(firstCellMismatch))
                    {
                        firstCellMismatch =
                            $"index={index}; reference={DescribeCell(legacy)}; " +
                            $"descriptor={DescribeCell(current)}";
                    }
                }
            }

            int maximumSamples = Mathf.Max(
                legacySamples.Count,
                descriptorSamples.Count);
            for (int index = 0; index < maximumSamples; index++)
            {
                bool hasLegacy = index < legacySamples.Count;
                bool hasDescriptor = index < descriptorSamples.Count;
                if (!hasLegacy || !hasDescriptor)
                {
                    sampleMismatchCount++;
                    if (string.IsNullOrEmpty(firstSampleMismatch))
                    {
                        firstSampleMismatch =
                            $"sample={index}; reference=" +
                            (hasLegacy
                                ? DescribeSample(river, legacySamples[index])
                                : "<missing>") +
                            "; descriptor=" +
                            (hasDescriptor
                                ? DescribeSample(river, descriptorSamples[index])
                                : "<missing>");
                    }
                    continue;
                }

                RiverObstacleExclusionSample legacy = legacySamples[index];
                RiverObstacleExclusionSample current = descriptorSamples[index];
                if (!Vector4BitsEqual(legacy.Intervals, current.Intervals) ||
                    !Vector4BitsEqual(
                        legacy.WaterParameters,
                        current.WaterParameters))
                {
                    sampleMismatchCount++;
                    if (string.IsNullOrEmpty(firstSampleMismatch))
                    {
                        int cellOrdinal = index / SamplesPerCell;
                        int withinCell = index % SamplesPerCell;
                        Vector2Int coordinate = cellOrdinal < legacyCells.Count
                            ? legacyCells[cellOrdinal].Coordinate
                            : cellOrdinal < descriptorCells.Count
                                ? descriptorCells[cellOrdinal].Coordinate
                                : new Vector2Int(-1, -1);
                        firstSampleMismatch =
                            $"sample={index}; cell={coordinate}; " +
                            $"sampleXY={withinCell % SamplesPerAxis}," +
                            $"{withinCell / SamplesPerAxis}; reference=" +
                            DescribeSample(river, legacy) +
                            "; descriptor=" +
                            DescribeSample(river, current);
                    }
                }
            }

            int cellCount = fieldWidth * fieldHeight;
            bool[] legacyScalar = new bool[cellCount];
            bool[] descriptorScalar = new bool[cellCount];
            int legacyDuplicates = PopulateCpuScalar(
                legacyCells,
                fieldWidth,
                fieldHeight,
                legacyScalar);
            int descriptorDuplicates = PopulateCpuScalar(
                descriptorCells,
                fieldWidth,
                fieldHeight,
                descriptorScalar);
            int scalarMismatchCount = 0;
            string firstScalarMismatch = string.Empty;
            for (int index = 0; index < cellCount; index++)
            {
                if (legacyScalar[index] == descriptorScalar[index])
                {
                    continue;
                }

                scalarMismatchCount++;
                if (string.IsNullOrEmpty(firstScalarMismatch))
                {
                    int x = index % fieldWidth;
                    int y = index / fieldWidth;
                    firstScalarMismatch =
                        $"cell=({x},{y}); reference=" +
                        (legacyScalar[index] ? "1" : "0") +
                        "; descriptor=" +
                        (descriptorScalar[index] ? "1" : "0");
                }
            }

            exact =
                rangeExact &&
                buildStateExact &&
                cellMismatchCount == 0 &&
                sampleMismatchCount == 0 &&
                scalarMismatchCount == 0;
            builder.AppendLine(
                $"Candidate range parity: {(rangeExact ? "EXACT" : "DIFFERENT")}");
            builder.AppendLine(
                $"Build-state parity: {(buildStateExact ? "EXACT" : "DIFFERENT")}");
            builder.AppendLine(
                $"Cells: reference={legacyCells.Count:N0}, " +
                $"descriptor={descriptorCells.Count:N0}, " +
                $"mismatches={cellMismatchCount:N0}, " +
                $"duplicates={legacyDuplicates:N0}/{descriptorDuplicates:N0}");
            builder.AppendLine(
                $"Samples: reference={legacySamples.Count:N0}, " +
                $"descriptor={descriptorSamples.Count:N0}, " +
                $"mismatches={sampleMismatchCount:N0}");
            builder.AppendLine(
                $"CPU scalar mismatches: {scalarMismatchCount:N0}");
            if (!string.IsNullOrEmpty(firstCellMismatch))
            {
                builder.AppendLine("First cell mismatch: " + firstCellMismatch);
            }
            if (!string.IsNullOrEmpty(firstSampleMismatch))
            {
                builder.AppendLine(
                    "First accepted-sample mismatch: " +
                    firstSampleMismatch);
            }
            if (!string.IsNullOrEmpty(firstScalarMismatch))
            {
                builder.AppendLine(
                    "First CPU scalar mismatch: " + firstScalarMismatch);
            }
            builder.AppendLine(
                $"VERDICT: {(exact ? "EXACT" : "DIFFERENT")}");
            report = builder.ToString();
            return true;
        }

        private static bool TryBakeLegacyReference(
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
                status =
                    "A valid river, readable generated mesh, Foam field, " +
                    "and output lists are required.";
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

            if (!TryResolveCandidateRangeLegacyReference(
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
                status =
                    "The generated mesh could not be projected into this " +
                    "river's Foam field.";
                return false;
            }

            int initialCellCount = cellOutput.Count;
            int initialSampleCount = sampleOutput.Count;
            RiverObstacleExclusionSample[] cellSamples =
                new RiverObstacleExclusionSample[SamplesPerCell];
            List<float> intersectionScratch = new(16);
            List<float> uniqueIntersectionScratch = new(16);

            for (int x = minimumX; x <= maximumX; x++)
            {
                float centreU =
                    StylizedRiverFoamTopologyFieldSpace.TexelCentreUv(
                        x,
                        fieldWidth);
                float centreGlobalDistance =
                    river.Domain.GlobalDistanceMinimum +
                    centreU * fieldLength;
                StylizedRiverSplineSample centreSample =
                    river.Domain.SampleAtGlobalDistance(
                        Mathf.Clamp(
                            centreGlobalDistance,
                            river.Domain.GlobalDistanceMinimum,
                            river.Domain.GlobalDistanceMaximum));
                float minimumAcross01 =
                    StylizedRiverFoamTopologyFieldSpace
                        .AcrossMetresTo01Clamped(
                            minimumAcross,
                            centreSample.LeftSurfaceHalfWidth,
                            centreSample.RightSurfaceHalfWidth);
                float maximumAcross01 =
                    StylizedRiverFoamTopologyFieldSpace
                        .AcrossMetresTo01Clamped(
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
                            if (!TryResolveBaseSurfaceSampleLegacyReference(
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
                status =
                    "The exact mesh produced no conservative full-texel " +
                    "obstacle cells at the current Foam resolution.";
                return false;
            }

            status =
                $"Baked {addedCells} conservative full-resolution obstacle " +
                "cells from the exact transformed generated mesh.";
            return true;
        }

        private static bool TryResolveCandidateRangeLegacyReference(
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
                StylizedRiverFoamTopologyFieldSpace
                    .LocalDistanceToContainingTexel(
                        minimumGlobal - globalStart,
                        fieldWidth,
                        fieldLength) - 1,
                0,
                fieldWidth - 1);
            maximumX = Mathf.Clamp(
                StylizedRiverFoamTopologyFieldSpace
                    .LocalDistanceToCeilingTexel(
                        maximumGlobal - globalStart,
                        fieldWidth,
                        fieldLength) + 1,
                0,
                fieldWidth - 1);
            return maximumX >= minimumX && fieldHeight > 0;
        }

        private static bool TryResolveBaseSurfaceSampleLegacyReference(
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
            if (globalDistance <
                    river.Domain.GlobalDistanceMinimum - 0.0001f ||
                globalDistance >
                    river.Domain.GlobalDistanceMaximum + 0.0001f)
            {
                return false;
            }

            StylizedRiverSplineSample sample =
                river.Domain.SampleAtGlobalDistance(
                    Mathf.Clamp(
                        globalDistance,
                        river.Domain.GlobalDistanceMinimum,
                        river.Domain.GlobalDistanceMaximum));
            float acrossMetres =
                StylizedRiverFoamTopologyFieldSpace.Across01ToMetres(
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

        private static int PopulateCpuScalar(
            IReadOnlyList<RiverObstacleExclusionCell> cells,
            int fieldWidth,
            int fieldHeight,
            bool[] scalar)
        {
            int duplicates = 0;
            for (int index = 0; index < cells.Count; index++)
            {
                Vector2Int coordinate = cells[index].Coordinate;
                if (coordinate.x < 0 || coordinate.x >= fieldWidth ||
                    coordinate.y < 0 || coordinate.y >= fieldHeight)
                {
                    continue;
                }
                int flat = coordinate.y * fieldWidth + coordinate.x;
                if (scalar[flat])
                {
                    duplicates++;
                }
                scalar[flat] = true;
            }
            return duplicates;
        }

        private static bool FloatBitsEqual(float left, float right)
        {
            return BitConverter.SingleToInt32Bits(left) ==
                BitConverter.SingleToInt32Bits(right);
        }

        private static bool Vector4BitsEqual(Vector4 left, Vector4 right)
        {
            return FloatBitsEqual(left.x, right.x) &&
                FloatBitsEqual(left.y, right.y) &&
                FloatBitsEqual(left.z, right.z) &&
                FloatBitsEqual(left.w, right.w);
        }

        private static string FormatFloatExact(float value)
        {
            return $"{value:R}[0x{BitConverter.SingleToInt32Bits(value):X8}]";
        }

        private static string DescribeCell(RiverObstacleExclusionCell cell)
        {
            return $"coordinate={cell.Coordinate}, offset={cell.IntervalOffset}";
        }

        private static string DescribeSample(
            StylizedRiver river,
            RiverObstacleExclusionSample sample)
        {
            Vector4 water = sample.WaterParameters;
            StylizedRiverSplineSample spline =
                river.Domain.SampleAtGlobalDistance(
                    Mathf.Clamp(
                        water.x,
                        river.Domain.GlobalDistanceMinimum,
                        river.Domain.GlobalDistanceMaximum));
            Vector3 basePoint = spline.SurfacePoint + spline.Side * water.y;
            Vector3 up = spline.Up.sqrMagnitude > 0.0001f
                ? spline.Up.normalized
                : Vector3.up;
            return
                $"intervals={FormatVector4Exact(sample.Intervals)}, " +
                $"water={FormatVector4Exact(water)}, " +
                $"base={FormatVector3Exact(basePoint)}, " +
                $"up={FormatVector3Exact(up)}";
        }

        private static string FormatVector3Exact(Vector3 value)
        {
            return
                $"({FormatFloatExact(value.x)}, " +
                $"{FormatFloatExact(value.y)}, " +
                $"{FormatFloatExact(value.z)})";
        }

        private static string FormatVector4Exact(Vector4 value)
        {
            return
                $"({FormatFloatExact(value.x)}, " +
                $"{FormatFloatExact(value.y)}, " +
                $"{FormatFloatExact(value.z)}, " +
                $"{FormatFloatExact(value.w)})";
        }
    }
}
#endif
