using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    /// <summary>
    /// Immutable authored controls for deterministic static channel variation.
    /// All sampling uses river-space metres so results remain independent from
    /// mesh tessellation and spline-knot count.
    /// </summary>
    public readonly struct StylizedRiverNaturalVariationSettings
    {
        public StylizedRiverNaturalVariationSettings(
            int seed,
            float bedRoughness,
            float bedRoughnessScale,
            float bedRoughnessReach,
            float shorelineIrregularity,
            float shorelineIrregularityScale,
            float bankAsymmetry)
        {
            Seed = seed;
            BedRoughness = Mathf.Max(0f, bedRoughness);
            BedRoughnessScale = Mathf.Max(0.5f, bedRoughnessScale);
            BedRoughnessReach = Mathf.Clamp01(bedRoughnessReach);
            ShorelineIrregularity = Mathf.Max(0f, shorelineIrregularity);
            ShorelineIrregularityScale =
                Mathf.Max(1.5f, shorelineIrregularityScale);
            BankAsymmetry = Mathf.Clamp01(bankAsymmetry);
        }

        public static StylizedRiverNaturalVariationSettings None { get; } =
            new StylizedRiverNaturalVariationSettings(
                0,
                0f,
                6f,
                0f,
                0f,
                10f,
                0.5f);
                

        public int Seed { get; }
        public float BedRoughness { get; }
        public float BedRoughnessScale { get; }
        public float BedRoughnessReach { get; }
        public float ShorelineIrregularity { get; }
        public float ShorelineIrregularityScale { get; }
        public float BankAsymmetry { get; }

        public float ResolveSafeShorelineAmplitude(float baseHalfWidth)
        {
            float safeMaximum = Mathf.Max(0f, baseHalfWidth * 0.45f);
            return Mathf.Min(ShorelineIrregularity, safeMaximum);
        }

        public float ResolveSafeBedRoughness(
            float depth,
            float requiredWetClearance)
        {
            float availableDepth =
                Mathf.Max(0f, depth - requiredWetClearance - 0.05f);

            return Mathf.Min(
                BedRoughness,
                availableDepth * 0.45f);
        }

        /// <summary>
        /// Maps the artist-facing 0–1 reach to a protected portion of the
        /// authored bed-slope elevation. The upper 18% always remains smooth so
        /// the shoreline, hidden cover, and future wave-damping band stay safe.
        /// </summary>
        public float ResolveSafeBedSlopeReach()
        {
            return Mathf.Lerp(0f, 0.82f, BedRoughnessReach);
        }

        /// <summary>
        /// Returns roughness influence for a normalized authored bed-profile
        /// height, where zero is the floor and one is the water-level edge.
        /// </summary>
        public float EvaluateBedSlopeRoughnessMask(
            float normalizedProfileHeight)
        {
            float reach = ResolveSafeBedSlopeReach();

            if (reach <= 0.0001f)
            {
                return 0f;
            }

            float profileHeight =
                Mathf.Clamp01(normalizedProfileHeight);
            float fadeStart = reach * 0.55f;

            if (profileHeight <= fadeStart)
            {
                return 1f;
            }

            if (profileHeight >= reach)
            {
                return 0f;
            }

            float t =
                Mathf.InverseLerp(
                    fadeStart,
                    reach,
                    profileHeight);

            t = t * t * (3f - 2f * t);
            return 1f - t;
        }
    }

    /// <summary>
    /// Shared deterministic variation sampler for domain, water surface, and
    /// corridor generation. It contains no mutable state and performs no
    /// per-frame work.
    /// </summary>
    public static class StylizedRiverNaturalVariation
    {
        public static void ResolveShoreWidths(
            float baseHalfWidth,
            float hiddenOverlap,
            float riverDistance,
            StylizedRiverNaturalVariationSettings settings,
            out float leftVisibleHalfWidth,
            out float rightVisibleHalfWidth,
            out float leftSurfaceHalfWidth,
            out float rightSurfaceHalfWidth)
        {
            float resolvedBase = Mathf.Max(0.25f, baseHalfWidth);
            float amplitude =
                settings.ResolveSafeShorelineAmplitude(resolvedBase);

            if (amplitude <= 0.0001f)
            {
                leftVisibleHalfWidth = resolvedBase;
                rightVisibleHalfWidth = resolvedBase;
                leftSurfaceHalfWidth =
                    resolvedBase + Mathf.Max(0f, hiddenOverlap);
                rightSurfaceHalfWidth = leftSurfaceHalfWidth;
                return;
            }

            float scale = settings.ShorelineIrregularityScale;
            float coordinate = riverDistance / scale;

            float common =
                SampleFractal1D(
                    coordinate,
                    settings.Seed,
                    0x21A7);

            float leftIndependent =
                SampleFractal1D(
                    coordinate,
                    settings.Seed,
                    0x4B53);

            float rightIndependent =
                SampleFractal1D(
                    coordinate,
                    settings.Seed,
                    0x79D1);

            float asymmetry = settings.BankAsymmetry;
            float leftNoise =
                Mathf.Lerp(common, leftIndependent, asymmetry);
            float rightNoise =
                Mathf.Lerp(common, rightIndependent, asymmetry);

            leftVisibleHalfWidth =
                Mathf.Max(
                    resolvedBase * 0.55f,
                    resolvedBase + leftNoise * amplitude);

            rightVisibleHalfWidth =
                Mathf.Max(
                    resolvedBase * 0.55f,
                    resolvedBase + rightNoise * amplitude);

            float overlap = Mathf.Max(0f, hiddenOverlap);
            leftSurfaceHalfWidth = leftVisibleHalfWidth + overlap;
            rightSurfaceHalfWidth = rightVisibleHalfWidth + overlap;
        }

        public static float EvaluateBedNoise(
            float riverDistance,
            float lateralMetres,
            StylizedRiverNaturalVariationSettings settings)
        {
            if (settings.BedRoughness <= 0.0001f)
            {
                return 0f;
            }

            float scale = settings.BedRoughnessScale;
            float longitudinal = riverDistance / scale;
            float lateral = lateralMetres / scale;

            float primary =
                SampleCenteredPerlin2D(
                    longitudinal,
                    lateral * 1.35f,
                    settings.Seed,
                    0x13C7);

            float secondary =
                SampleCenteredPerlin2D(
                    longitudinal * 2.07f,
                    lateral * 2.41f,
                    settings.Seed,
                    0x5E91);

            return Mathf.Clamp(
                primary * 0.74f + secondary * 0.26f,
                -1f,
                1f);
        }

        private static float SampleFractal1D(
            float coordinate,
            int seed,
            int salt)
        {
            float primary =
                SampleCenteredPerlin2D(
                    coordinate,
                    0f,
                    seed,
                    salt);

            float secondary =
                SampleCenteredPerlin2D(
                    coordinate * 2.17f,
                    0.37f,
                    seed,
                    salt ^ 0x6D2B);

            return Mathf.Clamp(
                primary * 0.78f + secondary * 0.22f,
                -1f,
                1f);
        }

        private static float SampleCenteredPerlin2D(
            float x,
            float y,
            int seed,
            int salt)
        {
            float offsetX = Hash01(seed, salt) * 2048f + 31.7f;
            float offsetY =
                Hash01(seed, salt ^ 0x5BD1) * 2048f + 79.3f;

            return
                Mathf.PerlinNoise(
                    x + offsetX,
                    y + offsetY) *
                2f -
                1f;
        }

        private static float Hash01(int seed, int value)
        {
            unchecked
            {
                uint hash = (uint)seed;
                hash ^= (uint)value + 0x9E3779B9u +
                        (hash << 6) +
                        (hash >> 2);
                hash ^= hash >> 16;
                hash *= 0x7FEB352Du;
                hash ^= hash >> 15;
                hash *= 0x846CA68Bu;
                hash ^= hash >> 16;

                return
                    (hash & 0x00FFFFFFu) /
                    16777215f;
            }
        }
    }
}
