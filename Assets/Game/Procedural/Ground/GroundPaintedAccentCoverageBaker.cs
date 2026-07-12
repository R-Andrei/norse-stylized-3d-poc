using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    public readonly struct GroundPaintedAccentCoverageDiagnostics
    {
        public GroundPaintedAccentCoverageDiagnostics(
            int textureWidth,
            int textureHeight,
            int glyphCount,
            int segmentCount,
            int coveredTexelCount,
            float coveredTexelFraction,
            float texelWorldSizeX,
            float texelWorldSizeZ,
            float minimumAuthoredHalfWidth,
            float minimumEffectiveHalfWidth)
        {
            TextureWidth = Mathf.Max(0, textureWidth);
            TextureHeight = Mathf.Max(0, textureHeight);
            GlyphCount = Mathf.Max(0, glyphCount);
            SegmentCount = Mathf.Max(0, segmentCount);
            CoveredTexelCount = Mathf.Max(0, coveredTexelCount);
            CoveredTexelFraction = Mathf.Clamp01(coveredTexelFraction);
            TexelWorldSizeX = Mathf.Max(0f, texelWorldSizeX);
            TexelWorldSizeZ = Mathf.Max(0f, texelWorldSizeZ);
            MinimumAuthoredHalfWidth = Mathf.Max(0f, minimumAuthoredHalfWidth);
            MinimumEffectiveHalfWidth = Mathf.Max(0f, minimumEffectiveHalfWidth);
        }

        public int TextureWidth { get; }
        public int TextureHeight { get; }
        public int GlyphCount { get; }
        public int SegmentCount { get; }
        public int CoveredTexelCount { get; }
        public float CoveredTexelFraction { get; }
        public float TexelWorldSizeX { get; }
        public float TexelWorldSizeZ { get; }
        public float MinimumAuthoredHalfWidth { get; }
        public float MinimumEffectiveHalfWidth { get; }

        public bool IsValid =>
            TextureWidth > 0 &&
            TextureHeight > 0 &&
            GlyphCount > 0 &&
            SegmentCount > 0;

        public static GroundPaintedAccentCoverageDiagnostics Empty => default;
    }

    public static class GroundPaintedAccentCoverageBaker
    {
        public const int Revision = 1;

        private const int MinimumResolution = 64;
        private const int MaximumResolution = 2048;
        private const int ResolutionAlignment = 8;
        private const float TargetTexelWorldSize = 0.0125f;
        private const float MinimumHalfWidthInTexels = 0.45f;
        private const float EdgeFeatherInTexels = 1.15f;
        private const float MinimumEndpointFadeFraction = 0.025f;
        private const float MaximumEndpointFadeFraction = 0.055f;
        private const byte CoveredTexelThreshold = 8;

        public static Texture2D Bake(
            Bounds localBounds,
            IReadOnlyList<GroundPaintedAccentProjectedGlyph> glyphs,
            out Vector4 originSize,
            out Vector4 texelSize,
            out GroundPaintedAccentCoverageDiagnostics diagnostics)
        {
            Vector3 boundsSize = localBounds.size;
            float sizeX = Mathf.Max(0.0001f, boundsSize.x);
            float sizeZ = Mathf.Max(0.0001f, boundsSize.z);
            originSize =
                new Vector4(
                    localBounds.min.x,
                    localBounds.min.z,
                    sizeX,
                    sizeZ);

            int width = ResolveResolution(sizeX);
            int height = ResolveResolution(sizeZ);
            float texelWorldSizeX = sizeX / width;
            float texelWorldSizeZ = sizeZ / height;
            float maximumTexelWorldSize =
                Mathf.Max(texelWorldSizeX, texelWorldSizeZ);
            texelSize =
                new Vector4(
                    1f / width,
                    1f / height,
                    width,
                    height);

            if (glyphs == null || glyphs.Count == 0)
            {
                diagnostics = GroundPaintedAccentCoverageDiagnostics.Empty;
                return CreateTexture(width, height, new byte[width * height]);
            }

            byte[] pixels = new byte[width * height];
            int validGlyphCount = 0;
            int segmentCount = 0;
            float minimumAuthoredHalfWidth = float.PositiveInfinity;
            float minimumEffectiveHalfWidth = float.PositiveInfinity;

            for (int glyphIndex = 0;
                 glyphIndex < glyphs.Count;
                 glyphIndex++)
            {
                GroundPaintedAccentProjectedGlyph glyph = glyphs[glyphIndex];
                if (!glyph.IsValid)
                {
                    continue;
                }

                validGlyphCount++;
                Vector3[] points = glyph.LocalSurfacePoints;
                float[] halfWidths = glyph.HalfWidths;
                int lastPointIndex = points.Length - 1;

                for (int pointIndex = 0;
                     pointIndex < lastPointIndex;
                     pointIndex++)
                {
                    Vector2 start =
                        new Vector2(
                            points[pointIndex].x,
                            points[pointIndex].z);
                    Vector2 end =
                        new Vector2(
                            points[pointIndex + 1].x,
                            points[pointIndex + 1].z);
                    Vector2 segment = end - start;
                    float segmentLengthSquared = segment.sqrMagnitude;
                    if (segmentLengthSquared <= 0.0000001f)
                    {
                        continue;
                    }

                    float authoredStartHalfWidth =
                        Mathf.Max(0.0001f, halfWidths[pointIndex]);
                    float authoredEndHalfWidth =
                        Mathf.Max(0.0001f, halfWidths[pointIndex + 1]);
                    float minimumRasterHalfWidth =
                        maximumTexelWorldSize * MinimumHalfWidthInTexels;
                    float effectiveStartHalfWidth =
                        Mathf.Max(
                            authoredStartHalfWidth,
                            minimumRasterHalfWidth);
                    float effectiveEndHalfWidth =
                        Mathf.Max(
                            authoredEndHalfWidth,
                            minimumRasterHalfWidth);
                    float maximumHalfWidth =
                        Mathf.Max(
                            effectiveStartHalfWidth,
                            effectiveEndHalfWidth);
                    float maximumFeather =
                        Mathf.Max(
                            maximumTexelWorldSize * EdgeFeatherInTexels,
                            maximumHalfWidth * 0.28f);
                    float expansion = maximumHalfWidth + maximumFeather;

                    minimumAuthoredHalfWidth =
                        Mathf.Min(
                            minimumAuthoredHalfWidth,
                            authoredStartHalfWidth,
                            authoredEndHalfWidth);
                    minimumEffectiveHalfWidth =
                        Mathf.Min(
                            minimumEffectiveHalfWidth,
                            effectiveStartHalfWidth,
                            effectiveEndHalfWidth);

                    ResolvePixelRange(
                        Mathf.Min(start.x, end.x) - expansion,
                        Mathf.Max(start.x, end.x) + expansion,
                        originSize.x,
                        texelWorldSizeX,
                        width,
                        out int minX,
                        out int maxX);
                    ResolvePixelRange(
                        Mathf.Min(start.y, end.y) - expansion,
                        Mathf.Max(start.y, end.y) + expansion,
                        originSize.y,
                        texelWorldSizeZ,
                        height,
                        out int minZ,
                        out int maxZ);

                    float startProgress =
                        pointIndex / (float)lastPointIndex;
                    float endProgress =
                        (pointIndex + 1) / (float)lastPointIndex;
                    float endpointFadeFraction =
                        Mathf.Clamp(
                            1.5f / lastPointIndex,
                            MinimumEndpointFadeFraction,
                            MaximumEndpointFadeFraction);

                    for (int z = minZ; z <= maxZ; z++)
                    {
                        float sampleZ =
                            originSize.y + (z + 0.5f) * texelWorldSizeZ;

                        for (int x = minX; x <= maxX; x++)
                        {
                            float sampleX =
                                originSize.x + (x + 0.5f) * texelWorldSizeX;
                            Vector2 sample = new Vector2(sampleX, sampleZ);
                            float segmentT =
                                Mathf.Clamp01(
                                    Vector2.Dot(sample - start, segment) /
                                    segmentLengthSquared);
                            Vector2 closest = start + segment * segmentT;
                            float distance = Vector2.Distance(sample, closest);
                            float halfWidth =
                                Mathf.Lerp(
                                    effectiveStartHalfWidth,
                                    effectiveEndHalfWidth,
                                    segmentT);
                            float feather =
                                Mathf.Max(
                                    maximumTexelWorldSize * EdgeFeatherInTexels,
                                    halfWidth * 0.28f);
                            float coverage =
                                1f - SmoothStep(
                                    halfWidth,
                                    halfWidth + feather,
                                    distance);

                            float progress =
                                Mathf.Lerp(
                                    startProgress,
                                    endProgress,
                                    segmentT);
                            float endpointEnvelope =
                                SmoothStep(
                                    0f,
                                    endpointFadeFraction,
                                    progress) *
                                (1f - SmoothStep(
                                    1f - endpointFadeFraction,
                                    1f,
                                    progress));
                            coverage *= endpointEnvelope;

                            if (coverage <= 0f)
                            {
                                continue;
                            }

                            byte candidate =
                                (byte)Mathf.Clamp(
                                    Mathf.RoundToInt(coverage * 255f),
                                    0,
                                    255);
                            int pixelIndex = z * width + x;
                            if (candidate > pixels[pixelIndex])
                            {
                                pixels[pixelIndex] = candidate;
                            }
                        }
                    }

                    segmentCount++;
                }
            }

            int coveredTexelCount = 0;
            for (int pixelIndex = 0;
                 pixelIndex < pixels.Length;
                 pixelIndex++)
            {
                if (pixels[pixelIndex] >= CoveredTexelThreshold)
                {
                    coveredTexelCount++;
                }
            }

            if (float.IsPositiveInfinity(minimumAuthoredHalfWidth))
            {
                minimumAuthoredHalfWidth = 0f;
            }

            if (float.IsPositiveInfinity(minimumEffectiveHalfWidth))
            {
                minimumEffectiveHalfWidth = 0f;
            }

            diagnostics =
                new GroundPaintedAccentCoverageDiagnostics(
                    width,
                    height,
                    validGlyphCount,
                    segmentCount,
                    coveredTexelCount,
                    pixels.Length > 0
                        ? coveredTexelCount / (float)pixels.Length
                        : 0f,
                    texelWorldSizeX,
                    texelWorldSizeZ,
                    minimumAuthoredHalfWidth,
                    minimumEffectiveHalfWidth);

            return CreateTexture(width, height, pixels);
        }

        public static Texture2D CreateNeutralTexture()
        {
            return CreateTexture(1, 1, new byte[1]);
        }

        private static int ResolveResolution(float worldSize)
        {
            int desired =
                Mathf.CeilToInt(
                    Mathf.Max(0.0001f, worldSize) /
                    TargetTexelWorldSize);
            desired =
                Mathf.Clamp(
                    desired,
                    MinimumResolution,
                    MaximumResolution);
            return Mathf.Min(
                MaximumResolution,
                RoundUpToMultiple(desired, ResolutionAlignment));
        }

        private static int RoundUpToMultiple(int value, int multiple)
        {
            int safeMultiple = Mathf.Max(1, multiple);
            return
                Mathf.CeilToInt(value / (float)safeMultiple) *
                safeMultiple;
        }

        private static void ResolvePixelRange(
            float minimumWorld,
            float maximumWorld,
            float origin,
            float texelWorldSize,
            int resolution,
            out int minimumPixel,
            out int maximumPixel)
        {
            float safeTexelWorldSize = Mathf.Max(0.000001f, texelWorldSize);
            minimumPixel =
                Mathf.Clamp(
                    Mathf.FloorToInt(
                        (minimumWorld - origin) /
                        safeTexelWorldSize),
                    0,
                    resolution - 1);
            maximumPixel =
                Mathf.Clamp(
                    Mathf.CeilToInt(
                        (maximumWorld - origin) /
                        safeTexelWorldSize),
                    0,
                    resolution - 1);
        }

        private static float SmoothStep(float edge0, float edge1, float value)
        {
            float denominator = Mathf.Max(0.000001f, edge1 - edge0);
            float t = Mathf.Clamp01((value - edge0) / denominator);
            return t * t * (3f - 2f * t);
        }

        private static Texture2D CreateTexture(
            int width,
            int height,
            byte[] pixels)
        {
            int safeWidth = Mathf.Max(1, width);
            int safeHeight = Mathf.Max(1, height);
            int requiredPixelCount = safeWidth * safeHeight;
            byte[] uploadPixels =
                pixels != null && pixels.Length == requiredPixelCount
                    ? pixels
                    : new byte[requiredPixelCount];

            Texture2D texture =
                new Texture2D(
                    safeWidth,
                    safeHeight,
                    TextureFormat.R8,
                    false,
                    true)
                {
                    name = "PS3D_GroundPaintedAccentCoverage_R8",
                    hideFlags = HideFlags.DontSave,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    anisoLevel = 0
                };

            texture.LoadRawTextureData(uploadPixels);
            texture.Apply(false, true);
            return texture;
        }
    }
}
