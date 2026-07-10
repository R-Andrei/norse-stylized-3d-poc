using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    internal static class GroundPaintedAccentFoldFieldGenerator
    {
        public const int Resolution = 256;

        private const float MinimumFieldSize = 0.0001f;

        public static Texture2D Generate(
            Bounds localBounds,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            int shapeSeed,
            out Vector4 originSize,
            out Vector4 texelSize,
            out float[] bodyValues)
        {
            originSize =
                new Vector4(
                    localBounds.min.x,
                    localBounds.min.z,
                    Mathf.Max(MinimumFieldSize, localBounds.size.x),
                    Mathf.Max(MinimumFieldSize, localBounds.size.z));
            texelSize =
                new Vector4(
                    1f / Resolution,
                    1f / Resolution,
                    Resolution,
                    Resolution);

            int pixelCount = Resolution * Resolution;
            float[] rawField = new float[pixelCount];
            float[] supportedField = new float[pixelCount];
            float[] bodyField = new float[pixelCount];
            float[] smoothedBody = new float[pixelCount];
            float[] supportField = new float[pixelCount];

            FieldSettings settings =
                FieldSettings.Create(
                    feature,
                    shapeSeed);

            GenerateRawContinuousField(
                originSize,
                settings,
                rawField);

            ApplySemanticSupport(
                originSize,
                baseSurface,
                feature,
                rawField,
                supportedField,
                supportField);

            ShapeBodyField(
                supportedField,
                settings,
                bodyField);

            SmoothBodyField(
                bodyField,
                smoothedBody);

            bodyValues = smoothedBody;

            Texture2D texture =
                new Texture2D(
                    Resolution,
                    Resolution,
                    TextureFormat.RGBA32,
                    false,
                    true)
                {
                    name = "GeneratedGround_PaintedAccentFoldField",
                    hideFlags = HideFlags.DontSave,
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };

            Color32[] pixels =
                BuildPixelsFromContinuousField(
                    originSize,
                    smoothedBody,
                    supportField,
                    settings);

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private static void GenerateRawContinuousField(
            Vector4 originSize,
            FieldSettings settings,
            float[] rawField)
        {
            for (int z = 0; z < Resolution; z++)
            {
                for (int x = 0; x < Resolution; x++)
                {
                    int index = z * Resolution + x;
                    Vector2 localXZ =
                        TexelToLocalXZ(
                            x,
                            z,
                            originSize);

                    Vector2 warpedXZ =
                        ResolveDomainWarpedPosition(
                            localXZ,
                            settings);

                    float broad =
                        FractalValueNoise(
                            warpedXZ / (settings.BaseScale * 2.35f),
                            settings.Seed + 101);
                    float medium =
                        FractalValueNoise(
                            warpedXZ / (settings.BaseScale * 1.05f),
                            settings.Seed + 211);
                    float ridgeSource =
                        FractalValueNoise(
                            warpedXZ / (settings.BaseScale * 0.72f),
                            settings.Seed + 307);

                    float ridge =
                        1f - Mathf.Abs(ridgeSource * 2f - 1f);
                    ridge =
                        Mathf.Pow(
                            Mathf.Clamp01(ridge),
                            settings.RidgePower);

                    float directional =
                        ResolveDirectionalContinuity(
                            warpedXZ,
                            settings);

                    float raw =
                        broad * 0.48f +
                        medium * 0.24f +
                        ridge * 0.20f +
                        directional * 0.08f;

                    rawField[index] =
                        Mathf.Pow(
                            Mathf.Clamp01(raw),
                            settings.RawPower);
                }
            }
        }

        private static Vector2 ResolveDomainWarpedPosition(
            Vector2 localXZ,
            FieldSettings settings)
        {
            Vector2 warpSampleXZ =
                localXZ / (settings.BaseScale * 1.75f);
            float warpX =
                FractalValueNoise(
                    warpSampleXZ,
                    settings.Seed + 401);
            float warpY =
                FractalValueNoise(
                    warpSampleXZ + new Vector2(17.31f, -9.73f),
                    settings.Seed + 503);

            Vector2 warp =
                new Vector2(
                    warpX * 2f - 1f,
                    warpY * 2f - 1f);

            return localXZ + warp * settings.WarpStrength;
        }

        private static float ResolveDirectionalContinuity(
            Vector2 warpedXZ,
            FieldSettings settings)
        {
            Vector2 alongAcross =
                new Vector2(
                    Vector2.Dot(warpedXZ, settings.Direction),
                    Vector2.Dot(warpedXZ, settings.CrossDirection));

            float lane =
                FractalValueNoise(
                    new Vector2(
                        alongAcross.x / (settings.BaseScale * 1.75f),
                        alongAcross.y / (settings.BaseScale * 0.58f)),
                    settings.Seed + 607);

            float ridge =
                1f - Mathf.Abs(lane * 2f - 1f);

            return Mathf.Pow(
                Mathf.Clamp01(ridge),
                Mathf.Lerp(1.60f, 3.10f, settings.Contrast));
        }

        private static void ApplySemanticSupport(
            Vector4 originSize,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            float[] rawField,
            float[] supportedField,
            float[] supportField)
        {
            float maskInfluence =
                feature != null
                    ? Mathf.Clamp01(feature.MaskInfluence)
                    : 0f;

            for (int z = 0; z < Resolution; z++)
            {
                for (int x = 0; x < Resolution; x++)
                {
                    int index = z * Resolution + x;
                    Vector2 localXZ =
                        TexelToLocalXZ(
                            x,
                            z,
                            originSize);
                    float support =
                        ResolveSemanticSupport(
                            baseSurface,
                            localXZ,
                            maskInfluence);

                    supportField[index] = support;
                    supportedField[index] = rawField[index] * support;
                }
            }
        }

        private static void ShapeBodyField(
            float[] supportedField,
            FieldSettings settings,
            float[] bodyField)
        {
            float threshold =
                ResolvePercentileThreshold(
                    supportedField,
                    1f - settings.TargetCoverage);

            float highThreshold =
                Mathf.Min(
                    1f,
                    threshold + settings.ThresholdSoftness);

            for (int index = 0; index < supportedField.Length; index++)
            {
                float body =
                    SmoothStep(
                        threshold,
                        highThreshold,
                        supportedField[index]);

                body =
                    Mathf.Pow(
                        Mathf.Clamp01(body),
                        settings.BodyPower) *
                    settings.BodyIntensity;

                bodyField[index] = Mathf.Clamp01(body);
            }
        }

        private static float ResolvePercentileThreshold(
            float[] values,
            float percentile)
        {
            if (values == null || values.Length == 0)
            {
                return 1f;
            }

            float[] sorted =
                (float[])values.Clone();
            Array.Sort(sorted);

            int index =
                Mathf.Clamp(
                    Mathf.RoundToInt(
                        Mathf.Clamp01(percentile) *
                        (sorted.Length - 1)),
                    0,
                    sorted.Length - 1);

            return sorted[index];
        }

        private static void SmoothBodyField(
            float[] source,
            float[] destination)
        {
            for (int z = 0; z < Resolution; z++)
            {
                int zm = Mathf.Max(0, z - 1);
                int zp = Mathf.Min(Resolution - 1, z + 1);

                for (int x = 0; x < Resolution; x++)
                {
                    int xm = Mathf.Max(0, x - 1);
                    int xp = Mathf.Min(Resolution - 1, x + 1);
                    int index = z * Resolution + x;

                    float cardinalAverage =
                        (source[z * Resolution + xm] +
                         source[z * Resolution + xp] +
                         source[zm * Resolution + x] +
                         source[zp * Resolution + x]) *
                        0.25f;

                    float diagonalAverage =
                        (source[zm * Resolution + xm] +
                         source[zm * Resolution + xp] +
                         source[zp * Resolution + xm] +
                         source[zp * Resolution + xp]) *
                        0.25f;

                    float neighborAverage =
                        cardinalAverage * 0.72f +
                        diagonalAverage * 0.28f;

                    destination[index] =
                        Mathf.Clamp01(
                            source[index] * 0.72f +
                            neighborAverage * 0.28f);
                }
            }
        }

        private static Color32[] BuildPixelsFromContinuousField(
            Vector4 originSize,
            float[] bodyField,
            float[] supportField,
            FieldSettings settings)
        {
            Color32[] pixels =
                new Color32[Resolution * Resolution];

            for (int z = 0; z < Resolution; z++)
            {
                int zm = Mathf.Max(0, z - 1);
                int zp = Mathf.Min(Resolution - 1, z + 1);

                for (int x = 0; x < Resolution; x++)
                {
                    int xm = Mathf.Max(0, x - 1);
                    int xp = Mathf.Min(Resolution - 1, x + 1);
                    int index = z * Resolution + x;

                    float body =
                        Mathf.Clamp01(bodyField[index]);
                    float dx =
                        bodyField[z * Resolution + xp] -
                        bodyField[z * Resolution + xm];
                    float dz =
                        bodyField[zp * Resolution + x] -
                        bodyField[zm * Resolution + x];
                    Vector2 gradient =
                        new Vector2(dx, dz);
                    float gradientMagnitude =
                        gradient.magnitude;
                    float signed = 0f;

                    if (gradientMagnitude > 0.00001f)
                    {
                        signed =
                            Vector2.Dot(
                                gradient / gradientMagnitude,
                                settings.Direction) *
                            Mathf.Clamp01(
                                gradientMagnitude *
                                settings.SignedGradientScale);
                    }

                    Vector2 localXZ =
                        TexelToLocalXZ(
                            x,
                            z,
                            originSize);
                    float edge =
                        Mathf.Clamp01(
                            gradientMagnitude *
                            settings.EdgeGradientScale);
                    float heightGate =
                        SmoothStep(0.10f, 0.32f, body) *
                        (1f - SmoothStep(0.88f, 1.0f, body));
                    float sideGate =
                        Mathf.Clamp01(0.5f + signed * 0.5f);
                    float breakNoise =
                        FractalValueNoise(
                            localXZ / (settings.BaseScale * 0.55f),
                            settings.Seed + 701);
                    float breakGate =
                        SmoothStep(0.28f, 0.62f, breakNoise);
                    float line =
                        Mathf.Pow(
                            Mathf.Clamp01(
                                edge *
                                heightGate *
                                sideGate *
                                breakGate),
                            1.25f);

                    pixels[index] =
                        new Color32(
                            ToByte(line),
                            ToByte(body),
                            ToByte(0.5f + signed * 0.5f),
                            ToByte(supportField[index]));
                }
            }

            return pixels;
        }

        private static float ResolveSemanticSupport(
            GroundHeightFieldSnapshot baseSurface,
            Vector2 localXZ,
            float maskInfluence)
        {
            if (baseSurface == null ||
                !baseSurface.IsValid ||
                !baseSurface.TrySample(localXZ, out GroundSurfaceSample sample))
            {
                return 1f;
            }

            float semanticSupport =
                0.22f +
                sample.DampDeposit * 0.22f +
                sample.VegetationSuitability * 0.24f +
                sample.Compaction * 0.24f +
                sample.ShoreInfluence * 0.10f +
                sample.RockyDry * 0.08f;

            return Mathf.Lerp(
                1f,
                Mathf.Clamp01(semanticSupport),
                maskInfluence);
        }

        private static Vector2 TexelToLocalXZ(
            int x,
            int z,
            Vector4 originSize)
        {
            float u = (x + 0.5f) / Resolution;
            float v = (z + 0.5f) / Resolution;

            return new Vector2(
                originSize.x + u * originSize.z,
                originSize.y + v * originSize.w);
        }

        private static float FractalValueNoise(
            Vector2 coordinate,
            int seed)
        {
            float value = 0f;
            float amplitude = 0.5f;
            float frequency = 1f;
            float amplitudeSum = 0f;

            for (int octave = 0; octave < 4; octave++)
            {
                value +=
                    ValueNoise(
                        coordinate.x * frequency,
                        coordinate.y * frequency,
                        seed + octave * 131) *
                    amplitude;
                amplitudeSum += amplitude;
                frequency *= 2.03f;
                amplitude *= 0.5f;
            }

            return value / Mathf.Max(0.0001f, amplitudeSum);
        }

        private static float ValueNoise(
            float x,
            float y,
            int seed)
        {
            int ix = Mathf.FloorToInt(x);
            int iy = Mathf.FloorToInt(y);
            float fx = x - ix;
            float fy = y - iy;
            float sx = Smooth01(fx);
            float sy = Smooth01(fy);

            float a = Hash01(HashUInt(ix, iy, seed), 3u);
            float b = Hash01(HashUInt(ix + 1, iy, seed), 5u);
            float c = Hash01(HashUInt(ix, iy + 1, seed), 7u);
            float d = Hash01(HashUInt(ix + 1, iy + 1, seed), 11u);

            return Mathf.Lerp(
                Mathf.Lerp(a, b, sx),
                Mathf.Lerp(c, d, sx),
                sy);
        }

        private static float Smooth01(float value)
        {
            value = Mathf.Clamp01(value);
            return value * value * (3f - 2f * value);
        }

        private static float SmoothStep(
            float edge0,
            float edge1,
            float value)
        {
            if (Mathf.Abs(edge1 - edge0) <= 0.00001f)
            {
                return value >= edge1 ? 1f : 0f;
            }

            return Smooth01((value - edge0) / (edge1 - edge0));
        }

        private static byte ToByte(float value)
        {
            return (byte)Mathf.Clamp(
                Mathf.RoundToInt(Mathf.Clamp01(value) * 255f),
                0,
                255);
        }

        private static int Hash(
            int a,
            int b,
            int salt)
        {
            unchecked
            {
                uint h = 2166136261u;
                h = (h ^ (uint)a) * 16777619u;
                h = (h ^ (uint)b) * 16777619u;
                h = (h ^ (uint)salt) * 16777619u;
                return (int)h;
            }
        }

        private static uint HashUInt(
            int x,
            int y,
            int seed)
        {
            unchecked
            {
                uint h = 2166136261u;
                h = (h ^ (uint)x) * 16777619u;
                h = (h ^ (uint)y) * 16777619u;
                h = (h ^ (uint)seed) * 16777619u;
                h ^= h >> 13;
                h *= 1274126177u;
                h ^= h >> 16;
                return h;
            }
        }

        private static float Hash01(
            uint hash,
            uint salt)
        {
            uint value = hash ^ (salt * 0x9E3779B9u);
            value ^= value >> 16;
            value *= 0x7FEB352Du;
            value ^= value >> 15;
            value *= 0x846CA68Bu;
            value ^= value >> 16;
            return (value & 0x00FFFFFFu) / 16777215f;
        }

        private readonly struct FieldSettings
        {
            private FieldSettings(
                int seed,
                float strength,
                float contrast,
                float baseScale,
                float targetCoverage,
                float thresholdSoftness,
                float bodyPower,
                float bodyIntensity,
                float ridgePower,
                float rawPower,
                float warpStrength,
                float signedGradientScale,
                float edgeGradientScale,
                Vector2 direction,
                Vector2 crossDirection)
            {
                Seed = seed;
                Strength = strength;
                Contrast = contrast;
                BaseScale = baseScale;
                TargetCoverage = targetCoverage;
                ThresholdSoftness = thresholdSoftness;
                BodyPower = bodyPower;
                BodyIntensity = bodyIntensity;
                RidgePower = ridgePower;
                RawPower = rawPower;
                WarpStrength = warpStrength;
                SignedGradientScale = signedGradientScale;
                EdgeGradientScale = edgeGradientScale;
                Direction = direction;
                CrossDirection = crossDirection;
            }

            public int Seed { get; }
            public float Strength { get; }
            public float Contrast { get; }
            public float BaseScale { get; }
            public float TargetCoverage { get; }
            public float ThresholdSoftness { get; }
            public float BodyPower { get; }
            public float BodyIntensity { get; }
            public float RidgePower { get; }
            public float RawPower { get; }
            public float WarpStrength { get; }
            public float SignedGradientScale { get; }
            public float EdgeGradientScale { get; }
            public Vector2 Direction { get; }
            public Vector2 CrossDirection { get; }

            public static FieldSettings Create(
                GroundSurfaceFeatureRecipe feature,
                int shapeSeed)
            {
                float strength =
                    feature != null
                        ? Mathf.Clamp01(feature.Strength)
                        : 0f;
                float contrast =
                    feature != null
                        ? Mathf.Clamp01(feature.Contrast)
                        : 0.5f;
                float scale =
                    feature != null
                        ? feature.Scale
                        : 5f;
                Vector2 direction =
                    feature != null
                        ? feature.Direction
                        : Vector2.right;

                if (direction.sqrMagnitude < 0.0001f)
                {
                    direction = Vector2.right;
                }

                direction.Normalize();

                Vector2 crossDirection =
                    new Vector2(-direction.y, direction.x);
                int seed =
                    Hash(
                        shapeSeed,
                        feature != null ? feature.SeedOffset : 0,
                        0x4466);
                float baseScale =
                    Mathf.Clamp(scale, 2.0f, 12.0f);
                float targetCoverage =
                    Mathf.Lerp(0.08f, 0.22f, strength);
                float thresholdSoftness =
                    Mathf.Lerp(0.18f, 0.07f, contrast);
                float bodyPower =
                    Mathf.Lerp(1.22f, 0.82f, strength);
                float bodyIntensity =
                    Mathf.Lerp(0.72f, 1.0f, strength);
                float ridgePower =
                    Mathf.Lerp(1.45f, 3.25f, contrast);
                float rawPower =
                    Mathf.Lerp(1.05f, 1.65f, contrast);
                float warpStrength =
                    baseScale * Mathf.Lerp(0.12f, 0.42f, contrast);
                float signedGradientScale =
                    Mathf.Lerp(18f, 42f, contrast);
                float edgeGradientScale =
                    Mathf.Lerp(18f, 44f, contrast);

                return new FieldSettings(
                    seed,
                    strength,
                    contrast,
                    baseScale,
                    targetCoverage,
                    thresholdSoftness,
                    bodyPower,
                    bodyIntensity,
                    ridgePower,
                    rawPower,
                    warpStrength,
                    signedGradientScale,
                    edgeGradientScale,
                    direction,
                    crossDirection);
            }
        }
    }
}
