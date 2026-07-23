using System;
using Unity.Profiling;
using UnityEngine;

namespace ProgrammaticStylized3D.Weather
{
    internal static class WeatherCloudShadowCookieGenerator
    {
        internal const string GeneratePixelsProfilerMarkerName =
            "Weather.CloudShadow.GeneratePixels";
        internal const string UploadPixelsProfilerMarkerName =
            "Weather.CloudShadow.UploadPixels";

        private static readonly ProfilerMarker GeneratePixelsProfilerMarker =
            new ProfilerMarker(GeneratePixelsProfilerMarkerName);
        private static readonly ProfilerMarker UploadPixelsProfilerMarker =
            new ProfilerMarker(UploadPixelsProfilerMarkerName);

        internal sealed class Workspace
        {
            public float[] PrimaryLattice;
            public float[] SecondaryLattice;
            public float[] FieldValues;
            public bool[] Visited;
            public int[] Queue;

            public void EnsureCapacity(
                int pixelCount,
                int primaryLatticeCount,
                int secondaryLatticeCount)
            {
                EnsureArray(ref PrimaryLattice, primaryLatticeCount);
                EnsureArray(ref SecondaryLattice, secondaryLatticeCount);
                EnsureArray(ref FieldValues, pixelCount);
                EnsureArray(ref Visited, pixelCount);
                EnsureArray(ref Queue, pixelCount);
            }

            private static void EnsureArray<T>(ref T[] array, int count)
            {
                if (array == null || array.Length < count)
                {
                    array = new T[count];
                }
            }
        }

        internal readonly struct Settings
        {
            public readonly int Resolution;
            public readonly int Seed;
            public readonly float WorldSizeMetres;
            public readonly float CloudCoverage;
            public readonly float PrimaryFeatureScaleMetres;
            public readonly float SecondaryFeatureScaleMetres;
            public readonly float SecondaryShapeWeight;
            public readonly float TransitionSoftnessMetres;
            public readonly float MinimumOpeningDiameterMetres;
            public readonly float ShadedTransmission;

            public Settings(
                int resolution,
                int seed,
                float worldSizeMetres,
                float cloudCoverage,
                float primaryFeatureScaleMetres,
                float secondaryFeatureScaleMetres,
                float secondaryShapeWeight,
                float transitionSoftnessMetres,
                float minimumOpeningDiameterMetres,
                float shadedTransmission)
            {
                Resolution = resolution;
                Seed = seed;
                WorldSizeMetres = worldSizeMetres;
                CloudCoverage = cloudCoverage;
                PrimaryFeatureScaleMetres = primaryFeatureScaleMetres;
                SecondaryFeatureScaleMetres = secondaryFeatureScaleMetres;
                SecondaryShapeWeight = secondaryShapeWeight;
                TransitionSoftnessMetres = transitionSoftnessMetres;
                MinimumOpeningDiameterMetres = minimumOpeningDiameterMetres;
                ShadedTransmission = shadedTransmission;
            }
        }

        public static int ResolveResolution(int requestedResolution)
        {
            return Mathf.Clamp(
                Mathf.ClosestPowerOfTwo(requestedResolution),
                64,
                1024);
        }

        public static int ResolvePixelCount(Settings settings)
        {
            int resolution = ResolveResolution(settings.Resolution);
            return resolution * resolution;
        }

        public static Texture2D Generate(Settings settings)
        {
            int pixelCount = ResolvePixelCount(settings);
            var pixels = new byte[pixelCount];
            var workspace = new Workspace();
            GeneratePixels(settings, pixels, workspace);
            return CreateTexture(settings, pixels);
        }

        public static void GeneratePixels(
            Settings settings,
            byte[] destination,
            Workspace workspace)
        {
            GeneratePixelsProfilerMarker.Begin();
            try
            {
                if (destination == null)
                {
                    throw new ArgumentNullException(nameof(destination));
                }

                if (workspace == null)
                {
                    throw new ArgumentNullException(nameof(workspace));
                }

                int resolution = ResolveResolution(settings.Resolution);
                int pixelCount = resolution * resolution;
                if (destination.Length < pixelCount)
                {
                    throw new ArgumentException(
                        $"Weather cloud-shadow pixel destination requires at least {pixelCount} bytes.",
                        nameof(destination));
                }

                float worldSize = Mathf.Max(8f, settings.WorldSizeMetres);
                int primaryCells = ResolvePeriodicCellCount(
                    worldSize,
                    settings.PrimaryFeatureScaleMetres);
                int secondaryCells = ResolvePeriodicCellCount(
                    worldSize,
                    settings.SecondaryFeatureScaleMetres);
                int primaryLatticeCount = primaryCells * primaryCells;
                int secondaryLatticeCount = secondaryCells * secondaryCells;
                workspace.EnsureCapacity(
                    pixelCount,
                    primaryLatticeCount,
                    secondaryLatticeCount);

                float secondaryWeight = Mathf.Clamp01(
                    settings.SecondaryShapeWeight);
                float coverage = Mathf.Clamp(
                    settings.CloudCoverage,
                    0.05f,
                    0.95f);
                float softness = Mathf.Clamp(
                    settings.TransitionSoftnessMetres /
                    Mathf.Max(settings.PrimaryFeatureScaleMetres, 0.01f),
                    0.005f,
                    0.35f);
                float shadedTransmission = Mathf.Clamp(
                    settings.ShadedTransmission,
                    0.05f,
                    1f);

                FillLattice(
                    workspace.PrimaryLattice,
                    primaryCells,
                    settings.Seed);
                FillLattice(
                    workspace.SecondaryLattice,
                    secondaryCells,
                    settings.Seed ^ unchecked((int)0x6C8E9CF5u));

                float[] fieldValues = workspace.FieldValues;
                for (int y = 0; y < resolution; y++)
                {
                    float v = y / (float)resolution;
                    for (int x = 0; x < resolution; x++)
                    {
                        float u = x / (float)resolution;
                        float primary = SamplePeriodicValueNoise(
                            workspace.PrimaryLattice,
                            primaryCells,
                            u,
                            v);
                        float secondary = SamplePeriodicValueNoise(
                            workspace.SecondaryLattice,
                            secondaryCells,
                            u,
                            v);
                        fieldValues[y * resolution + x] = Mathf.Lerp(
                            primary,
                            primary * 0.68f + secondary * 0.32f,
                            secondaryWeight);
                    }
                }

                RemoveSmallOpenings(
                    fieldValues,
                    resolution,
                    worldSize,
                    coverage,
                    softness,
                    settings.MinimumOpeningDiameterMetres,
                    workspace.Visited,
                    workspace.Queue);

                for (int index = 0; index < pixelCount; index++)
                {
                    float clearTransmission = Mathf.InverseLerp(
                        coverage - softness,
                        coverage + softness,
                        fieldValues[index]);
                    clearTransmission =
                        clearTransmission *
                        clearTransmission *
                        (3f - 2f * clearTransmission);
                    float transmission = Mathf.Lerp(
                        shadedTransmission,
                        1f,
                        clearTransmission);
                    destination[index] = (byte)Mathf.RoundToInt(
                        Mathf.Clamp01(transmission) * 255f);
                }
            }
            finally
            {
                GeneratePixelsProfilerMarker.End();
            }
        }

        public static Texture2D CreateTexture(
            Settings settings,
            byte[] pixels)
        {
            if (!SystemInfo.SupportsTextureFormat(TextureFormat.R8))
            {
                throw new NotSupportedException(
                    "Weather cloud shadows require TextureFormat.R8 support.");
            }

            int resolution = ResolveResolution(settings.Resolution);
            int pixelCount = resolution * resolution;
            ValidatePixelBuffer(pixels, pixelCount);

            var texture = new Texture2D(
                resolution,
                resolution,
                TextureFormat.R8,
                false,
                true)
            {
                name = BuildTextureName(settings.Seed, resolution),
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Repeat,
                anisoLevel = 0,
                hideFlags = HideFlags.HideAndDontSave
            };
            UploadPixels(texture, pixels);
            return texture;
        }

        public static void UploadPixels(
            Texture2D texture,
            byte[] pixels)
        {
            UploadPixelsProfilerMarker.Begin();
            try
            {
                if (texture == null)
                {
                    throw new ArgumentNullException(nameof(texture));
                }

                int pixelCount = texture.width * texture.height;
                ValidatePixelBuffer(pixels, pixelCount);
                texture.SetPixelData(pixels, 0);
                texture.Apply(false, false);
            }
            finally
            {
                UploadPixelsProfilerMarker.End();
            }
        }

        public static void SetTextureSeedName(
            Texture2D texture,
            int seed)
        {
            if (texture != null)
            {
                texture.name = BuildTextureName(seed, texture.width);
            }
        }

        private static void ValidatePixelBuffer(
            byte[] pixels,
            int pixelCount)
        {
            if (pixels == null || pixels.Length < pixelCount)
            {
                throw new ArgumentException(
                    $"Weather cloud-shadow pixel buffer requires at least {pixelCount} bytes.",
                    nameof(pixels));
            }
        }

        private static string BuildTextureName(int seed, int resolution)
        {
            return $"PS3D_WeatherCloudCookie_{seed}_{resolution}";
        }

        private static void RemoveSmallOpenings(
            float[] fieldValues,
            int resolution,
            float worldSizeMetres,
            float coverageThreshold,
            float transitionSoftness,
            float minimumOpeningDiameterMetres,
            bool[] visited,
            int[] queue)
        {
            if (minimumOpeningDiameterMetres <= 0f)
            {
                return;
            }

            int pixelCount = resolution * resolution;
            Array.Clear(visited, 0, pixelCount);

            float pixelsPerMetre = resolution / worldSizeMetres;
            float minimumRadiusPixels =
                minimumOpeningDiameterMetres * 0.5f * pixelsPerMetre;
            int minimumCoreAreaPixels = Mathf.Max(
                1,
                Mathf.CeilToInt(
                    Mathf.PI *
                    minimumRadiusPixels *
                    minimumRadiusPixels));
            float componentThreshold =
                coverageThreshold - transitionSoftness;

            for (int start = 0; start < pixelCount; start++)
            {
                if (visited[start] ||
                    fieldValues[start] <= componentThreshold)
                {
                    continue;
                }

                int head = 0;
                int tail = 0;
                int coreAreaPixels = 0;
                visited[start] = true;
                queue[tail++] = start;

                while (head < tail)
                {
                    int index = queue[head++];
                    if (fieldValues[index] > coverageThreshold)
                    {
                        coreAreaPixels++;
                    }

                    int x = index % resolution;
                    int y = index / resolution;
                    EnqueueOpeningNeighbour(
                        WrapCoordinate(x - 1, resolution),
                        y,
                        resolution,
                        componentThreshold,
                        fieldValues,
                        visited,
                        queue,
                        ref tail);
                    EnqueueOpeningNeighbour(
                        WrapCoordinate(x + 1, resolution),
                        y,
                        resolution,
                        componentThreshold,
                        fieldValues,
                        visited,
                        queue,
                        ref tail);
                    EnqueueOpeningNeighbour(
                        x,
                        WrapCoordinate(y - 1, resolution),
                        resolution,
                        componentThreshold,
                        fieldValues,
                        visited,
                        queue,
                        ref tail);
                    EnqueueOpeningNeighbour(
                        x,
                        WrapCoordinate(y + 1, resolution),
                        resolution,
                        componentThreshold,
                        fieldValues,
                        visited,
                        queue,
                        ref tail);
                }

                if (coreAreaPixels >= minimumCoreAreaPixels)
                {
                    continue;
                }

                for (int componentIndex = 0;
                    componentIndex < tail;
                    componentIndex++)
                {
                    fieldValues[queue[componentIndex]] = componentThreshold;
                }
            }
        }

        private static void EnqueueOpeningNeighbour(
            int x,
            int y,
            int resolution,
            float componentThreshold,
            float[] fieldValues,
            bool[] visited,
            int[] queue,
            ref int tail)
        {
            int index = y * resolution + x;
            if (visited[index] || fieldValues[index] <= componentThreshold)
            {
                return;
            }

            visited[index] = true;
            queue[tail++] = index;
        }

        private static int WrapCoordinate(int value, int resolution)
        {
            if (value < 0)
            {
                return resolution - 1;
            }

            return value >= resolution ? 0 : value;
        }

        private static int ResolvePeriodicCellCount(
            float worldSizeMetres,
            float featureScaleMetres)
        {
            return Mathf.Clamp(
                Mathf.RoundToInt(
                    worldSizeMetres /
                    Mathf.Max(1f, featureScaleMetres)),
                2,
                64);
        }

        private static void FillLattice(
            float[] lattice,
            int cellCount,
            int seed)
        {
            for (int y = 0; y < cellCount; y++)
            {
                for (int x = 0; x < cellCount; x++)
                {
                    lattice[y * cellCount + x] = Hash01(x, y, seed);
                }
            }
        }

        private static float SamplePeriodicValueNoise(
            float[] lattice,
            int cellCount,
            float u,
            float v)
        {
            float x = u * cellCount;
            float y = v * cellCount;
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            int x1 = (x0 + 1) % cellCount;
            int y1 = (y0 + 1) % cellCount;
            x0 %= cellCount;
            y0 %= cellCount;

            float tx = x - Mathf.Floor(x);
            float ty = y - Mathf.Floor(y);
            tx = tx * tx * (3f - 2f * tx);
            ty = ty * ty * (3f - 2f * ty);

            float a = lattice[y0 * cellCount + x0];
            float b = lattice[y0 * cellCount + x1];
            float c = lattice[y1 * cellCount + x0];
            float d = lattice[y1 * cellCount + x1];
            return Mathf.Lerp(
                Mathf.Lerp(a, b, tx),
                Mathf.Lerp(c, d, tx),
                ty);
        }

        private static float Hash01(int x, int y, int seed)
        {
            uint value = unchecked((uint)seed);
            value ^= unchecked((uint)x) * 0x9E3779B9u;
            value ^= unchecked((uint)y) * 0x85EBCA6Bu;
            value ^= value >> 16;
            value *= 0x7FEB352Du;
            value ^= value >> 15;
            value *= 0x846CA68Bu;
            value ^= value >> 16;
            return (value & 0x00FFFFFFu) / 16777215f;
        }
    }
}
