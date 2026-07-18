using System;
using System.Diagnostics;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    /// <summary>
    /// Canonical CPU coordinate contract for Stage 6 Foam fields.
    ///
    /// Cell positions use the same convention as the compute path: texel
    /// centres are 0.5, 1.5, ... count - 0.5. LegacyNormalizedAcross retains
    /// per-row normalized Y only while the migration is staged. FixedMetricLattice
    /// resolves every cell centre through one descriptor-owned s/n lattice and
    /// classifies valid water independently from the allocated rectangle.
    /// </summary>
    internal static class StylizedRiverFoamTopologyFieldSpace
    {
        private const float MinimumExtent = 0.0001f;
        private const float MinimumHalfWidth = 0.0001f;
        private const float FixedMetricBoundaryFeatherWidthMetres = 0.10f;
        private static bool foundationValidated;

        public static float TexelCentreUv(int index, int count)
        {
            return (index + 0.5f) / Mathf.Max(1f, count);
        }

        public static Vector2 TexelCentreUv(
            int x,
            int y,
            int width,
            int height)
        {
            return new Vector2(
                TexelCentreUv(x, width),
                TexelCentreUv(y, height));
        }

        public static float TexelSpacing(float extent, int count)
        {
            return extent / Mathf.Max(1, count);
        }

        public static float LocalDistanceAtTexel(
            int x,
            int width,
            float fieldLength)
        {
            return TexelCentreUv(x, width) * fieldLength;
        }

        public static float ResolveLocalDistanceAtColumnCentre(
            StylizedRiverFoamGridDescriptor descriptor,
            int x)
        {
            return descriptor.UsesFixedMetricLattice
                ? descriptor.ResolveLocalDistanceAtColumnCentre(x)
                : LocalDistanceAtTexel(
                    x,
                    descriptor.ColumnCount,
                    descriptor.AllocatedLengthMetres);
        }

        public static int LocalDistanceToNearestTexel(
            float localDistance,
            StylizedRiverFoamGridDescriptor descriptor)
        {
            if (!descriptor.UsesFixedMetricLattice)
            {
                return LocalDistanceToNearestTexel(
                    localDistance,
                    descriptor.ColumnCount,
                    descriptor.AllocatedLengthMetres);
            }

            float cellPosition =
                (localDistance - descriptor.FieldOrStripStartMetres) /
                Mathf.Max(MinimumExtent, descriptor.ResolvedDxMetres);
            return Mathf.Clamp(
                Mathf.RoundToInt(cellPosition - 0.5f),
                0,
                descriptor.ColumnCount - 1);
        }

        public static int LocalDistanceToContainingTexel(
            float localDistance,
            StylizedRiverFoamGridDescriptor descriptor)
        {
            if (!descriptor.UsesFixedMetricLattice)
            {
                return LocalDistanceToContainingTexel(
                    localDistance,
                    descriptor.ColumnCount,
                    descriptor.AllocatedLengthMetres);
            }

            float cellPosition =
                (localDistance - descriptor.FieldOrStripStartMetres) /
                Mathf.Max(MinimumExtent, descriptor.ResolvedDxMetres);
            return Mathf.Clamp(
                Mathf.FloorToInt(cellPosition),
                0,
                descriptor.ColumnCount - 1);
        }

        public static int LocalDistanceToCeilingTexel(
            float localDistance,
            StylizedRiverFoamGridDescriptor descriptor)
        {
            if (!descriptor.UsesFixedMetricLattice)
            {
                return LocalDistanceToCeilingTexel(
                    localDistance,
                    descriptor.ColumnCount,
                    descriptor.AllocatedLengthMetres);
            }

            float cellPosition =
                (localDistance - descriptor.FieldOrStripStartMetres) /
                Mathf.Max(MinimumExtent, descriptor.ResolvedDxMetres);
            return Mathf.Clamp(
                Mathf.CeilToInt(cellPosition),
                0,
                descriptor.ColumnCount - 1);
        }

        public static float Across01AtTexel(int y, int height)
        {
            return TexelCentreUv(y, height);
        }

        public static float SignedAcrossNormalizedAtTexel(
            int y,
            int height)
        {
            return Across01AtTexel(y, height) * 2f - 1f;
        }

        public static Vector2 UvToCellPosition(
            Vector2 uv,
            int width,
            int height)
        {
            return new Vector2(uv.x * width, uv.y * height);
        }

        public static Vector2 CellPositionToUv(
            Vector2 cellPosition,
            int width,
            int height)
        {
            return new Vector2(
                cellPosition.x / Mathf.Max(1f, width),
                cellPosition.y / Mathf.Max(1f, height));
        }

        // Use for rectangle ownership and exact masks.
        public static int UvToContainingTexel(float uv, int count)
        {
            return Mathf.Clamp(
                Mathf.FloorToInt(Mathf.Clamp01(uv) * count),
                0,
                Mathf.Max(0, count - 1));
        }

        // Use when selecting the closest sampled texel centre.
        public static int UvToNearestTexel(float uv, int count)
        {
            return Mathf.Clamp(
                Mathf.RoundToInt(Mathf.Clamp01(uv) * count - 0.5f),
                0,
                Mathf.Max(0, count - 1));
        }

        public static int LocalDistanceToNearestTexel(
            float localDistance,
            int width,
            float fieldLength)
        {
            float uv = localDistance / Mathf.Max(MinimumExtent, fieldLength);
            return UvToNearestTexel(uv, width);
        }

        public static int LocalDistanceToContainingTexel(
            float localDistance,
            int width,
            float fieldLength)
        {
            float uv = localDistance / Mathf.Max(MinimumExtent, fieldLength);
            return UvToContainingTexel(uv, width);
        }

        public static int LocalDistanceToCeilingTexel(
            float localDistance,
            int width,
            float fieldLength)
        {
            float uv = Mathf.Clamp01(
                localDistance / Mathf.Max(MinimumExtent, fieldLength));
            return Mathf.Clamp(
                Mathf.CeilToInt(uv * width),
                0,
                Mathf.Max(0, width - 1));
        }

        public static int SignedAcrossNormalizedToContainingTexel(
            float acrossNormalized,
            int height)
        {
            return UvToContainingTexel(
                acrossNormalized * 0.5f + 0.5f,
                height);
        }

        public static int SignedAcrossNormalizedToNearestTexel(
            float acrossNormalized,
            int height)
        {
            return UvToNearestTexel(
                acrossNormalized * 0.5f + 0.5f,
                height);
        }

        public static int Across01ToNearestTexel(
            float across01,
            int height)
        {
            return UvToNearestTexel(across01, height);
        }

        public static float Across01ToMetres(
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

        public static float AcrossMetresTo01(
            float acrossMetres,
            float leftHalfWidth,
            float rightHalfWidth)
        {
            return acrossMetres <= 0f
                ? 0.5f + acrossMetres /
                    Mathf.Max(MinimumHalfWidth, leftHalfWidth * 2f)
                : 0.5f + acrossMetres /
                    Mathf.Max(MinimumHalfWidth, rightHalfWidth * 2f);
        }

        public static float AcrossMetresTo01Clamped(
            float acrossMetres,
            float leftHalfWidth,
            float rightHalfWidth)
        {
            return Mathf.Clamp01(AcrossMetresTo01(
                acrossMetres,
                leftHalfWidth,
                rightHalfWidth));
        }

        public static float SignedNormalizedToMetres(
            float acrossNormalized,
            float leftHalfWidth,
            float rightHalfWidth)
        {
            return acrossNormalized < 0f
                ? acrossNormalized * leftHalfWidth
                : acrossNormalized * rightHalfWidth;
        }

        public static float MetresToSignedNormalized(
            float acrossMetres,
            float leftHalfWidth,
            float rightHalfWidth)
        {
            return acrossMetres < 0f
                ? Mathf.Clamp(
                    acrossMetres /
                    Mathf.Max(MinimumHalfWidth, leftHalfWidth),
                    -1f,
                    0f)
                : Mathf.Clamp(
                    acrossMetres /
                    Mathf.Max(MinimumHalfWidth, rightHalfWidth),
                    0f,
                    1f);
        }

        public static float ResolveAcrossNormalized(
            RiverDomainSnapshot domain,
            Vector2 metricPosition)
        {
            if (domain == null || !domain.IsValid)
            {
                return 0f;
            }

            StylizedRiverSplineSample sample =
                domain.SampleAtOrientedDistance(Mathf.Clamp(
                    metricPosition.x,
                    0f,
                    domain.LocalLength));
            return MetresToSignedNormalized(
                metricPosition.y,
                Mathf.Max(0.05f, sample.LeftSurfaceHalfWidth),
                Mathf.Max(0.05f, sample.RightSurfaceHalfWidth));
        }

        public static float ResolveLateralMetresAtRowCentre(
            StylizedRiverFoamGridDescriptor descriptor,
            int y,
            float leftSurfaceHalfWidth,
            float rightSurfaceHalfWidth)
        {
            return descriptor.UsesFixedMetricLattice
                ? descriptor.ResolveLateralMetresAtRowCentre(y)
                : Across01ToMetres(
                    Across01AtTexel(y, descriptor.RowCount),
                    Mathf.Max(0.05f, leftSurfaceHalfWidth),
                    Mathf.Max(0.05f, rightSurfaceHalfWidth));
        }

        public static Vector2 ResolveMetricPositionAtCellCentre(
            RiverDomainSnapshot domain,
            StylizedRiverFoamGridDescriptor descriptor,
            int x,
            int y)
        {
            float localDistance = ResolveLocalDistanceAtColumnCentre(
                descriptor,
                x);
            if (descriptor.UsesFixedMetricLattice)
            {
                return new Vector2(
                    localDistance,
                    descriptor.ResolveLateralMetresAtRowCentre(y));
            }

            if (domain == null || !domain.IsValid)
            {
                return new Vector2(localDistance, 0f);
            }

            float maximumSampleDistance = Mathf.Min(
                domain.LocalLength,
                Mathf.Max(0f, descriptor.ValidLengthMetres));
            StylizedRiverSplineSample sample =
                domain.SampleAtOrientedDistance(Mathf.Clamp(
                    localDistance,
                    0f,
                    maximumSampleDistance));
            return new Vector2(
                localDistance,
                ResolveLateralMetresAtRowCentre(
                    descriptor,
                    y,
                    sample.LeftSurfaceHalfWidth,
                    sample.RightSurfaceHalfWidth));
        }

        public static float ResolveBoundaryFeatherWidthMetres(
            StylizedRiverFoamGridDescriptor descriptor,
            StylizedRiverQuality quality,
            float leftSurfaceHalfWidth,
            float rightSurfaceHalfWidth)
        {
            if (descriptor.UsesFixedMetricLattice)
            {
                return Mathf.Max(
                    0.05f,
                    FixedMetricBoundaryFeatherWidthMetres);
            }

            float edgeCells = quality switch
            {
                StylizedRiverQuality.Low => 1.5f,
                StylizedRiverQuality.Medium => 2f,
                StylizedRiverQuality.High => 2.5f,
                _ => 2f
            };
            return Mathf.Max(
                0.05f,
                TexelSpacing(
                    leftSurfaceHalfWidth + rightSurfaceHalfWidth,
                    descriptor.RowCount) * edgeCells);
        }

        public static bool TryMetricToNearestCell(
            RiverDomainSnapshot domain,
            StylizedRiverFoamGridDescriptor descriptor,
            Vector2 metricPosition,
            out Vector2Int cell)
        {
            cell = default;
            if (!TryMetricToCellPosition(
                    domain,
                    descriptor,
                    metricPosition,
                    out Vector2 cellPosition))
            {
                return false;
            }

            cell = new Vector2Int(
                Mathf.Clamp(
                    Mathf.RoundToInt(cellPosition.x - 0.5f),
                    0,
                    descriptor.ColumnCount - 1),
                Mathf.Clamp(
                    Mathf.RoundToInt(cellPosition.y - 0.5f),
                    0,
                    descriptor.RowCount - 1));
            return true;
        }

        public static Vector2[] BuildMetricPositions(
            RiverDomainSnapshot domain,
            StylizedRiverFoamGridDescriptor descriptor)
        {
            if (!descriptor.IsCreated)
            {
                return Array.Empty<Vector2>();
            }

            int width = descriptor.ColumnCount;
            int height = descriptor.RowCount;
            Vector2[] positions = new Vector2[width * height];
            float maximumSampleDistance = domain != null && domain.IsValid
                ? Mathf.Min(
                    domain.LocalLength,
                    Mathf.Max(0f, descriptor.ValidLengthMetres))
                : 0f;
            for (int x = 0; x < width; x++)
            {
                float localDistance = ResolveLocalDistanceAtColumnCentre(
                    descriptor,
                    x);
                float leftSurface = 0.05f;
                float rightSurface = 0.05f;
                if (!descriptor.UsesFixedMetricLattice &&
                    domain != null && domain.IsValid)
                {
                    StylizedRiverSplineSample sample =
                        domain.SampleAtOrientedDistance(Mathf.Clamp(
                            localDistance,
                            0f,
                            maximumSampleDistance));
                    leftSurface = Mathf.Max(
                        0.05f,
                        sample.LeftSurfaceHalfWidth);
                    rightSurface = Mathf.Max(
                        0.05f,
                        sample.RightSurfaceHalfWidth);
                }

                for (int y = 0; y < height; y++)
                {
                    positions[x + y * width] = new Vector2(
                        localDistance,
                        ResolveLateralMetresAtRowCentre(
                            descriptor,
                            y,
                            leftSurface,
                            rightSurface));
                }
            }

            return positions;
        }

        public static bool[] BuildValidWaterMask(
            RiverDomainSnapshot domain,
            StylizedRiverFoamGridDescriptor descriptor)
        {
            int cellCount = descriptor.IsCreated
                ? descriptor.ColumnCount * descriptor.RowCount
                : 0;
            bool[] validWater = new bool[Mathf.Max(0, cellCount)];
            FillValidWaterMask(domain, descriptor, validWater);
            return validWater;
        }

        public static int FillValidWaterMask(
            RiverDomainSnapshot domain,
            StylizedRiverFoamGridDescriptor descriptor,
            bool[] validWater)
        {
            int width = descriptor.ColumnCount;
            int height = descriptor.RowCount;
            int cellCount = Mathf.Max(0, width * height);
            if (validWater == null || validWater.Length < cellCount)
            {
                throw new ArgumentException(
                    "Foam valid-water output does not match the grid.",
                    nameof(validWater));
            }

            Array.Clear(validWater, 0, validWater.Length);
            if (domain == null || !domain.IsValid ||
                !descriptor.IsCreated || width <= 0 || height <= 0)
            {
                return 0;
            }

            Vector2[] positions = BuildMetricPositions(domain, descriptor);
            int validCount = 0;
            for (int index = 0; index < cellCount; index++)
            {
                bool isValid = IsMetricPositionWithinValidWater(
                    domain,
                    descriptor,
                    positions[index]);
                validWater[index] = isValid;
                if (isValid)
                {
                    validCount++;
                }
            }

            return validCount;
        }

        public static bool IsMetricPositionWithinValidWater(
            RiverDomainSnapshot domain,
            StylizedRiverFoamGridDescriptor descriptor,
            Vector2 metricPosition)
        {
            if (domain == null || !domain.IsValid ||
                !descriptor.IsCreated ||
                !descriptor.ContainsValidLocalDistance(metricPosition.x))
            {
                return false;
            }

            float localDistance = Mathf.Clamp(
                metricPosition.x,
                descriptor.FieldOrStripStartMetres,
                Mathf.Min(
                    domain.LocalLength,
                    descriptor.ValidLocalDistanceMaximumMetres));
            StylizedRiverSplineSample sample =
                domain.SampleAtOrientedDistance(localDistance);
            float leftSurface = Mathf.Max(
                0.05f,
                sample.LeftSurfaceHalfWidth);
            float rightSurface = Mathf.Max(
                0.05f,
                sample.RightSurfaceHalfWidth);
            return metricPosition.y >= -leftSurface - MinimumExtent &&
                metricPosition.y <= rightSurface + MinimumExtent;
        }

        public static bool TryMetricToAllocatedCellPosition(
            StylizedRiverFoamGridDescriptor descriptor,
            Vector2 metricPosition,
            out Vector2 cellPosition)
        {
            if (descriptor.UsesFixedMetricLattice)
            {
                return descriptor.TryMetricToFractionalCellPosition(
                    metricPosition,
                    out cellPosition);
            }

            cellPosition = default;
            return false;
        }

        public static bool TryMetricToCellPosition(
            RiverDomainSnapshot domain,
            StylizedRiverFoamGridDescriptor descriptor,
            Vector2 metricPosition,
            out Vector2 cellPosition)
        {
            if (!descriptor.UsesFixedMetricLattice)
            {
                return TryMetricToCellPosition(
                    domain,
                    metricPosition,
                    descriptor.ColumnCount,
                    descriptor.RowCount,
                    descriptor.AllocatedLengthMetres,
                    descriptor.ValidLengthMetres,
                    out cellPosition);
            }

            if (!IsMetricPositionWithinValidWater(
                    domain,
                    descriptor,
                    metricPosition))
            {
                cellPosition = default;
                return false;
            }

            return descriptor.TryMetricToFractionalCellPosition(
                metricPosition,
                out cellPosition);
        }

        public static Vector2[] BuildMetricPositions(
            RiverDomainSnapshot domain,
            int width,
            int height,
            float fieldLength,
            float validFieldLength)
        {
            Vector2[] positions = new Vector2[Mathf.Max(0, width * height)];
            if (domain == null || !domain.IsValid ||
                width <= 0 || height <= 0 || fieldLength <= MinimumExtent)
            {
                return positions;
            }

            float maximumSampleDistance = Mathf.Min(
                domain.LocalLength,
                Mathf.Max(0f, validFieldLength));
            for (int x = 0; x < width; x++)
            {
                float localDistance = LocalDistanceAtTexel(
                    x,
                    width,
                    fieldLength);
                StylizedRiverSplineSample sample =
                    domain.SampleAtOrientedDistance(Mathf.Clamp(
                        localDistance,
                        0f,
                        maximumSampleDistance));
                float leftSurface = Mathf.Max(
                    0.05f,
                    sample.LeftSurfaceHalfWidth);
                float rightSurface = Mathf.Max(
                    0.05f,
                    sample.RightSurfaceHalfWidth);

                for (int y = 0; y < height; y++)
                {
                    float across01 = Across01AtTexel(y, height);
                    positions[x + y * width] = new Vector2(
                        localDistance,
                        Across01ToMetres(
                            across01,
                            leftSurface,
                            rightSurface));
                }
            }

            return positions;
        }

        public static bool TryMetricToCellPosition(
            RiverDomainSnapshot domain,
            Vector2 metricPosition,
            int width,
            int height,
            float fieldLength,
            float validFieldLength,
            out Vector2 cellPosition)
        {
            cellPosition = default;
            if (domain == null || !domain.IsValid ||
                width <= 0 || height <= 0 ||
                fieldLength <= MinimumExtent ||
                metricPosition.x < 0f ||
                metricPosition.x > fieldLength + MinimumExtent)
            {
                return false;
            }

            float sampleDistance = Mathf.Clamp(
                metricPosition.x,
                0f,
                Mathf.Min(domain.LocalLength, validFieldLength));
            StylizedRiverSplineSample sample =
                domain.SampleAtOrientedDistance(sampleDistance);
            float across01 = AcrossMetresTo01(
                metricPosition.y,
                Mathf.Max(0.05f, sample.LeftSurfaceHalfWidth),
                Mathf.Max(0.05f, sample.RightSurfaceHalfWidth));
            if (across01 < 0f || across01 > 1f)
            {
                return false;
            }

            cellPosition = UvToCellPosition(
                new Vector2(
                    metricPosition.x /
                        Mathf.Max(MinimumExtent, fieldLength),
                    across01),
                width,
                height);
            return true;
        }

        public static float SampleScalarBilinear(
            float[] source,
            int width,
            int height,
            Vector2 cellPosition)
        {
            if (source == null || source.Length < width * height ||
                width <= 0 || height <= 0 ||
                cellPosition.x < 0f || cellPosition.y < 0f ||
                cellPosition.x > width || cellPosition.y > height)
            {
                return 0f;
            }

            float x = Mathf.Clamp(
                cellPosition.x - 0.5f,
                0f,
                width - 1f);
            float y = Mathf.Clamp(
                cellPosition.y - 0.5f,
                0f,
                height - 1f);
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            int x1 = Mathf.Min(x0 + 1, width - 1);
            int y1 = Mathf.Min(y0 + 1, height - 1);
            float tx = x - x0;
            float ty = y - y0;
            float a = source[x0 + y0 * width];
            float b = source[x1 + y0 * width];
            float c = source[x0 + y1 * width];
            float d = source[x1 + y1 * width];
            return Mathf.Lerp(
                Mathf.Lerp(a, b, tx),
                Mathf.Lerp(c, d, tx),
                ty);
        }

        public static float SampleScalarAtMetric(
            float[] source,
            StylizedRiverFoamGridDescriptor descriptor,
            Vector2 metricPosition,
            RiverDomainSnapshot domain)
        {
            return TryMetricToCellPosition(
                    domain,
                    descriptor,
                    metricPosition,
                    out Vector2 cellPosition)
                ? Mathf.Clamp01(SampleScalarBilinear(
                    source,
                    descriptor.ColumnCount,
                    descriptor.RowCount,
                    cellPosition))
                : 0f;
        }

        [Conditional("UNITY_EDITOR")]
        public static void ValidateFoundation()
        {
            if (foundationValidated)
            {
                return;
            }

            StylizedRiverSplineSample[] samples =
            {
                new StylizedRiverSplineSample(
                    Vector3.zero,
                    Vector3.zero,
                    Vector3.forward,
                    Vector3.right,
                    Vector3.up,
                    0f,
                    0f,
                    0f,
                    2f,
                    3f,
                    2f,
                    3f,
                    0f,
                    0f),
                new StylizedRiverSplineSample(
                    Vector3.forward * 32f,
                    Vector3.forward * 32f,
                    Vector3.forward,
                    Vector3.right,
                    Vector3.up,
                    32f,
                    32f,
                    32f,
                    1f,
                    4f,
                    1f,
                    4f,
                    1f,
                    1f)
            };
            RiverDomainSnapshot domain = new RiverDomainSnapshot(
                samples,
                32f,
                0.5f,
                0f,
                false,
                1);
            bool created =
                StylizedRiverFoamGridDescriptor.TryCreateFixedMetricOneStrip(
                    StylizedRiverQuality.Medium,
                    0.25f,
                    0.25f,
                    20f,
                    -2f,
                    4f,
                    0f,
                    0,
                    8192,
                    out StylizedRiverFoamGridDescriptor descriptor,
                    out string failureReason);
            AssertFoundation(
                created,
                "Foam CPU field-space candidate failed: " + failureReason);

            Vector2[] positions = BuildMetricPositions(domain, descriptor);
            AssertFoundation(
                positions.Length ==
                    descriptor.ColumnCount * descriptor.RowCount,
                "Foam CPU metric-position count changed.");
            int y = descriptor.RowCount / 2;
            Vector2 first = positions[y * descriptor.ColumnCount];
            Vector2 last = positions[
                y * descriptor.ColumnCount + descriptor.ColumnCount - 1];
            AssertFoundation(
                Mathf.Abs(first.y - last.y) <= 0.000001f,
                "Foam metric Y changed across longitudinal columns.");
            AssertFoundation(
                TryMetricToCellPosition(
                    domain,
                    descriptor,
                    first,
                    out Vector2 firstCell) &&
                Mathf.Abs(firstCell.x - 0.5f) <= 0.00001f &&
                Mathf.Abs(firstCell.y - (y + 0.5f)) <= 0.00001f,
                "Foam CPU metric-position round trip changed.");

            bool[] validWater = BuildValidWaterMask(domain, descriptor);
            int validCount = 0;
            int invalidCount = 0;
            for (int index = 0; index < validWater.Length; index++)
            {
                if (validWater[index])
                {
                    validCount++;
                }
                else
                {
                    invalidCount++;
                }
            }
            AssertFoundation(
                validCount > 0 && invalidCount > 0,
                "Foam valid-water mask no longer separates allocated and " +
                    "out-of-water cells.");
            Vector2 paddedCell = descriptor.ResolveMetricPositionAtCellCentre(
                descriptor.ColumnCount - 1,
                y);
            AssertFoundation(
                !IsMetricPositionWithinValidWater(
                    domain,
                    descriptor,
                    paddedCell),
                "Foam padded endpoint cell was classified as valid water.");

            bool fullCreated =
                StylizedRiverFoamGridDescriptor.TryCreateFixedMetricOneStrip(
                    StylizedRiverQuality.Medium,
                    0.25f,
                    0.25f,
                    32f,
                    -2f,
                    4f,
                    0f,
                    0,
                    8192,
                    out StylizedRiverFoamGridDescriptor fullDescriptor,
                    out string fullFailureReason);
            AssertFoundation(
                fullCreated,
                "Foam reverse-flow descriptor failed: " +
                    fullFailureReason);
            RiverDomainSnapshot reversedDomain = new RiverDomainSnapshot(
                samples,
                32f,
                0.5f,
                0f,
                true,
                2);
            bool[] forwardMask = BuildValidWaterMask(
                domain,
                fullDescriptor);
            bool[] reversedMask = BuildValidWaterMask(
                reversedDomain,
                fullDescriptor);
            int width = fullDescriptor.ColumnCount;
            int height = fullDescriptor.RowCount;
            for (int row = 0; row < height; row++)
            {
                for (int x = 0; x < width; x++)
                {
                    AssertFoundation(
                        forwardMask[x + row * width] ==
                            reversedMask[
                                width - 1 - x + row * width],
                        "Foam reverse-flow valid-water mapping changed.");
                }
            }
            float fixedBoundary = ResolveBoundaryFeatherWidthMetres(
                descriptor,
                StylizedRiverQuality.Medium,
                2f,
                3f);
            AssertFoundation(
                Mathf.Abs(fixedBoundary - 0.10f) <= 0.000001f,
                "Foam fixed-metric boundary feather changed.");
            AssertFoundation(
                TryMetricToNearestCell(
                    domain,
                    descriptor,
                    first,
                    out Vector2Int firstNearest) &&
                firstNearest.x == 0 && firstNearest.y == y,
                "Foam nearest metric cell changed.");

            foundationValidated = true;
        }

        private static void AssertFoundation(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        public static float SampleScalarAtMetric(
            float[] source,
            int width,
            int height,
            float fieldLength,
            float validFieldLength,
            Vector2 metricPosition,
            RiverDomainSnapshot domain)
        {
            return TryMetricToCellPosition(
                    domain,
                    metricPosition,
                    width,
                    height,
                    fieldLength,
                    validFieldLength,
                    out Vector2 cellPosition)
                ? Mathf.Clamp01(SampleScalarBilinear(
                    source,
                    width,
                    height,
                    cellPosition))
                : 0f;
        }
    }
}
