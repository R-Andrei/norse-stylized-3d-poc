using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    internal static class GroundPaintedAccentFoldFieldGenerator
    {
        public const int Resolution = 256;

        private const int MaxCandidates = 768;
        private const float MinimumFieldSize = 0.0001f;

        public static Texture2D Generate(
            Bounds localBounds,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            int shapeSeed,
            out Vector4 originSize,
            out Vector4 texelSize)
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

            float[] rawHeights = new float[Resolution * Resolution];
            float[] bodyHeights = new float[Resolution * Resolution];
            float[] smoothedHeights = new float[Resolution * Resolution];
            float[] supports = new float[Resolution * Resolution];

            List<FoldCandidate> candidates =
                BuildCandidates(
                    originSize,
                    feature,
                    shapeSeed);

            RasterizeCandidates(
                originSize,
                candidates,
                feature,
                rawHeights);

            ApplySemanticSupport(
                originSize,
                baseSurface,
                feature,
                rawHeights,
                bodyHeights,
                supports);

            SmoothHeightField(
                bodyHeights,
                smoothedHeights);

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
                BuildPixels(
                    smoothedHeights,
                    supports,
                    feature);

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private static List<FoldCandidate> BuildCandidates(
            Vector4 originSize,
            GroundSurfaceFeatureRecipe feature,
            int shapeSeed)
        {
            List<FoldCandidate> candidates =
                new List<FoldCandidate>(MaxCandidates);

            float strength =
                feature != null
                    ? Mathf.Clamp01(feature.Strength)
                    : 0f;
            float scale =
                feature != null
                    ? feature.Scale
                    : 5f;
            float contrast =
                feature != null
                    ? Mathf.Clamp01(feature.Contrast)
                    : 0.5f;
            Vector2 preferredDirection =
                feature != null
                    ? feature.Direction
                    : Vector2.right;

            if (preferredDirection.sqrMagnitude < 0.0001f)
            {
                preferredDirection = Vector2.right;
            }

            preferredDirection.Normalize();

            int seed =
                Hash(
                    shapeSeed,
                    feature != null ? feature.SeedOffset : 0,
                    0x2E1B);
            float cellSize =
                Mathf.Clamp(scale * 0.45f, 0.85f, 3.25f);
            float density =
                Mathf.Lerp(0.18f, 0.48f, strength);
            float directionBias =
                Mathf.Lerp(0.08f, 0.30f, contrast);
            int minCellX =
                Mathf.FloorToInt(originSize.x / cellSize) - 1;
            int maxCellX =
                Mathf.CeilToInt((originSize.x + originSize.z) / cellSize) + 1;
            int minCellZ =
                Mathf.FloorToInt(originSize.y / cellSize) - 1;
            int maxCellZ =
                Mathf.CeilToInt((originSize.y + originSize.w) / cellSize) + 1;

            for (int cellZ = minCellZ; cellZ <= maxCellZ; cellZ++)
            {
                for (int cellX = minCellX; cellX <= maxCellX; cellX++)
                {
                    if (candidates.Count >= MaxCandidates)
                    {
                        return candidates;
                    }

                    uint cellHash =
                        HashUInt(
                            cellX,
                            cellZ,
                            seed);

                    if (Hash01(cellHash, 11u) > density)
                    {
                        continue;
                    }

                    Vector2 randomDirection =
                        DirectionFromAngle(
                            Hash01(cellHash, 17u) * Mathf.PI * 2f);
                    Vector2 direction =
                        Vector2.Lerp(
                            randomDirection,
                            preferredDirection,
                            directionBias);
                    if (direction.sqrMagnitude < 0.0001f)
                    {
                        direction = randomDirection;
                    }

                    direction.Normalize();
                    Vector2 crossDirection =
                        new Vector2(-direction.y, direction.x);

                    Vector2 center =
                        new Vector2(
                            (cellX + Mathf.Lerp(0.18f, 0.82f, Hash01(cellHash, 23u))) *
                            cellSize,
                            (cellZ + Mathf.Lerp(0.18f, 0.82f, Hash01(cellHash, 29u))) *
                            cellSize);

                    float majorRadius =
                        cellSize *
                        Mathf.Lerp(0.42f, 1.05f, Hash01(cellHash, 31u));
                    float minorRadius =
                        majorRadius *
                        Mathf.Lerp(0.34f, 0.68f, Hash01(cellHash, 37u));
                    float amplitude =
                        Mathf.Lerp(0.52f, 1.00f, Hash01(cellHash, 41u)) *
                        Mathf.Lerp(0.82f, 1.18f, strength);
                    float warpStrength =
                        cellSize *
                        Mathf.Lerp(0.035f, 0.085f, contrast);

                    candidates.Add(
                        new FoldCandidate(
                            center,
                            direction,
                            crossDirection,
                            majorRadius,
                            minorRadius,
                            amplitude,
                            warpStrength,
                            Hash(cellX, cellZ, seed ^ 0x5F35)));
                }
            }

            return candidates;
        }

        private static void RasterizeCandidates(
            Vector4 originSize,
            List<FoldCandidate> candidates,
            GroundSurfaceFeatureRecipe feature,
            float[] rawHeights)
        {
            float contrast =
                feature != null
                    ? Mathf.Clamp01(feature.Contrast)
                    : 0.5f;
            float contrastPower =
                Mathf.Lerp(1.15f, 2.35f, contrast);

            foreach (FoldCandidate candidate in candidates)
            {
                float boundRadius =
                    candidate.MajorRadius + candidate.WarpStrength + 0.15f;
                int minX =
                    WorldToTexelX(candidate.Center.x - boundRadius, originSize);
                int maxX =
                    WorldToTexelX(candidate.Center.x + boundRadius, originSize);
                int minZ =
                    WorldToTexelZ(candidate.Center.y - boundRadius, originSize);
                int maxZ =
                    WorldToTexelZ(candidate.Center.y + boundRadius, originSize);

                for (int z = minZ; z <= maxZ; z++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        Vector2 localXZ =
                            TexelToLocalXZ(
                                x,
                                z,
                                originSize);
                        Vector2 warp =
                            ResolveWarp(
                                localXZ,
                                candidate);
                        Vector2 delta =
                            localXZ + warp * candidate.WarpStrength -
                            candidate.Center;
                        float along =
                            Vector2.Dot(delta, candidate.Direction);
                        float across =
                            Vector2.Dot(delta, candidate.CrossDirection);
                        float normalizedDistance =
                            along * along /
                            Mathf.Max(
                                0.0001f,
                                candidate.MajorRadius * candidate.MajorRadius) +
                            across * across /
                            Mathf.Max(
                                0.0001f,
                                candidate.MinorRadius * candidate.MinorRadius);

                        if (normalizedDistance >= 1.0f)
                        {
                            continue;
                        }

                        float candidateBody =
                            Smooth01(1f - Mathf.Clamp01(normalizedDistance));
                        candidateBody =
                            Mathf.Pow(candidateBody, contrastPower) *
                            candidate.Amplitude;

                        int index = z * Resolution + x;
                        if (candidateBody > rawHeights[index])
                        {
                            rawHeights[index] = candidateBody;
                        }
                    }
                }
            }
        }

        private static void ApplySemanticSupport(
            Vector4 originSize,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            float[] rawHeights,
            float[] bodyHeights,
            float[] supports)
        {
            float strength =
                feature != null
                    ? Mathf.Clamp01(feature.Strength)
                    : 0f;
            float maskInfluence =
                feature != null
                    ? Mathf.Clamp01(feature.MaskInfluence)
                    : 0f;
            float bodyScale =
                Mathf.Lerp(0.78f, 1.32f, strength);

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
                    supports[index] = support;
                    bodyHeights[index] =
                        Mathf.Clamp01(rawHeights[index] * support * bodyScale);
                }
            }
        }

        private static void SmoothHeightField(
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

                    float neighborAverage =
                        (source[z * Resolution + xm] +
                         source[z * Resolution + xp] +
                         source[zm * Resolution + x] +
                         source[zp * Resolution + x]) *
                        0.25f;

                    destination[index] =
                        Mathf.Clamp01(
                            source[index] * 0.65f +
                            neighborAverage * 0.35f);
                }
            }
        }

        private static Color32[] BuildPixels(
            float[] heights,
            float[] supports,
            GroundSurfaceFeatureRecipe feature)
        {
            Color32[] pixels =
                new Color32[Resolution * Resolution];

            float contrast =
                feature != null
                    ? Mathf.Clamp01(feature.Contrast)
                    : 0.5f;
            Vector2 direction =
                feature != null
                    ? feature.Direction
                    : Vector2.right;

            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector2.right;
            }

            direction.Normalize();

            float gradientScale =
                Mathf.Lerp(22f, 46f, contrast);
            float edgeScale =
                Mathf.Lerp(18f, 42f, contrast);

            for (int z = 0; z < Resolution; z++)
            {
                int zm = Mathf.Max(0, z - 1);
                int zp = Mathf.Min(Resolution - 1, z + 1);

                for (int x = 0; x < Resolution; x++)
                {
                    int xm = Mathf.Max(0, x - 1);
                    int xp = Mathf.Min(Resolution - 1, x + 1);
                    int index = z * Resolution + x;

                    float body = Mathf.Clamp01(heights[index]);
                    float dx =
                        heights[z * Resolution + xp] -
                        heights[z * Resolution + xm];
                    float dz =
                        heights[zp * Resolution + x] -
                        heights[zm * Resolution + x];
                    Vector2 gradient = new Vector2(dx, dz);
                    float gradientMagnitude = gradient.magnitude;
                    float gradientStrength =
                        Mathf.Clamp01(gradientMagnitude * gradientScale);
                    float signed = 0f;

                    if (gradientMagnitude > 0.00001f)
                    {
                        signed =
                            Vector2.Dot(
                                gradient / gradientMagnitude,
                                direction) *
                            gradientStrength;
                    }

                    float edge =
                        Mathf.Clamp01(gradientMagnitude * edgeScale);
                    float heightGate =
                        SmoothStep(0.08f, 0.28f, body) *
                        (1f - SmoothStep(0.86f, 1.0f, body));
                    float sideGate =
                        Mathf.Clamp01(0.5f + signed * 0.5f);
                    float line =
                        Mathf.Pow(
                            Mathf.Clamp01(edge * heightGate * sideGate),
                            1.20f);

                    pixels[index] =
                        new Color32(
                            ToByte(line),
                            ToByte(body),
                            ToByte(0.5f + signed * 0.5f),
                            ToByte(supports[index]));
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

        private static Vector2 ResolveWarp(
            Vector2 localXZ,
            FoldCandidate candidate)
        {
            float frequency = 0.72f;
            float x =
                ValueNoise(
                    localXZ.x * frequency + candidate.WarpSeed * 0.013f,
                    localXZ.y * frequency - candidate.WarpSeed * 0.017f,
                    candidate.WarpSeed) *
                2f -
                1f;
            float y =
                ValueNoise(
                    localXZ.x * frequency - candidate.WarpSeed * 0.019f,
                    localXZ.y * frequency + candidate.WarpSeed * 0.011f,
                    candidate.WarpSeed ^ 0x6A09) *
                2f -
                1f;

            return new Vector2(x, y);
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

        private static int WorldToTexelX(
            float localX,
            Vector4 originSize)
        {
            float u =
                (localX - originSize.x) /
                Mathf.Max(MinimumFieldSize, originSize.z);
            return Mathf.Clamp(
                Mathf.FloorToInt(u * Resolution),
                0,
                Resolution - 1);
        }

        private static int WorldToTexelZ(
            float localZ,
            Vector4 originSize)
        {
            float v =
                (localZ - originSize.y) /
                Mathf.Max(MinimumFieldSize, originSize.w);
            return Mathf.Clamp(
                Mathf.FloorToInt(v * Resolution),
                0,
                Resolution - 1);
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

        private static Vector2 DirectionFromAngle(float angle)
        {
            return new Vector2(
                Mathf.Cos(angle),
                Mathf.Sin(angle));
        }

        private static float Hash01(uint hash, uint salt)
        {
            uint value = hash ^ (salt * 0x9E3779B9u);
            value ^= value >> 16;
            value *= 0x7FEB352Du;
            value ^= value >> 15;
            value *= 0x846CA68Bu;
            value ^= value >> 16;
            return (value & 0x00FFFFFFu) / 16777215f;
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

        private readonly struct FoldCandidate
        {
            public FoldCandidate(
                Vector2 center,
                Vector2 direction,
                Vector2 crossDirection,
                float majorRadius,
                float minorRadius,
                float amplitude,
                float warpStrength,
                int warpSeed)
            {
                Center = center;
                Direction = direction;
                CrossDirection = crossDirection;
                MajorRadius = majorRadius;
                MinorRadius = minorRadius;
                Amplitude = amplitude;
                WarpStrength = warpStrength;
                WarpSeed = warpSeed;
            }

            public Vector2 Center { get; }
            public Vector2 Direction { get; }
            public Vector2 CrossDirection { get; }
            public float MajorRadius { get; }
            public float MinorRadius { get; }
            public float Amplitude { get; }
            public float WarpStrength { get; }
            public int WarpSeed { get; }
        }
    }
}
