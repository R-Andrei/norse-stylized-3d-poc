using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    /// <summary>
    /// Canonical CPU coordinate contract for Stage 6 Foam topology fields.
    ///
    /// Topology UV spans the complete field rectangle. Texels own equal UV
    /// rectangles and are sampled at their centres: (index + 0.5) / count.
    /// Cell positions use the same convention as the topology mask compute
    /// path: texel centres are 0.5, 1.5, ... count - 0.5.
    /// </summary>
    internal static class StylizedRiverFoamTopologyFieldSpace
    {
        private const float MinimumExtent = 0.0001f;
        private const float MinimumHalfWidth = 0.0001f;

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
