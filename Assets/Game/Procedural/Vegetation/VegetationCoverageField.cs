using System;
using ProgrammaticStylized3D.Geometry.Ground;
using UnityEngine;

namespace ProgrammaticStylized3D.Vegetation
{
    [Serializable]
    public sealed class VegetationCoverageField
    {
        public const int DefaultResolution = 128;
        public const int MinimumResolution = 8;
        public const int MaximumResolution = 512;

        [SerializeField]
        private int resolution = DefaultResolution;

        [SerializeField]
        private byte[] pixels = Array.Empty<byte>();

        [SerializeField]
        private int revision;

        [SerializeField]
        private bool initialized;

        public bool Initialized => initialized;
        public int Resolution => ResolveResolution();
        public int Revision => revision;
        public int ByteCount => pixels != null ? pixels.Length : 0;
        public bool StorageValid => !initialized || HasValidStorage();

        public void SetResolution(int value, bool preserveApproximateCoverage)
        {
            int nextResolution = Mathf.Clamp(
                value,
                MinimumResolution,
                MaximumResolution);
            int currentResolution = ResolveResolution();
            if (nextResolution == currentResolution)
            {
                resolution = nextResolution;
                return;
            }

            byte[] previousPixels = pixels;
            bool previousInitialized = initialized && HasValidStorage();
            resolution = nextResolution;
            pixels = new byte[nextResolution * nextResolution];

            if (preserveApproximateCoverage && previousInitialized)
            {
                for (int z = 0; z < nextResolution; z++)
                {
                    float normalizedZ = nextResolution > 1
                        ? z / (float)(nextResolution - 1)
                        : 0f;
                    float sourceZ = normalizedZ * (currentResolution - 1);
                    int z0 = Mathf.Clamp(
                        Mathf.FloorToInt(sourceZ),
                        0,
                        currentResolution - 1);
                    int z1 = Mathf.Min(z0 + 1, currentResolution - 1);
                    float tz = Mathf.Clamp01(sourceZ - z0);

                    for (int x = 0; x < nextResolution; x++)
                    {
                        float normalizedX = nextResolution > 1
                            ? x / (float)(nextResolution - 1)
                            : 0f;
                        float sourceX = normalizedX * (currentResolution - 1);
                        int x0 = Mathf.Clamp(
                            Mathf.FloorToInt(sourceX),
                            0,
                            currentResolution - 1);
                        int x1 = Mathf.Min(x0 + 1, currentResolution - 1);
                        float tx = Mathf.Clamp01(sourceX - x0);

                        float lower = Mathf.Lerp(
                            previousPixels[z0 * currentResolution + x0],
                            previousPixels[z0 * currentResolution + x1],
                            tx);
                        float upper = Mathf.Lerp(
                            previousPixels[z1 * currentResolution + x0],
                            previousPixels[z1 * currentResolution + x1],
                            tx);
                        pixels[z * nextResolution + x] = (byte)Mathf.Clamp(
                            Mathf.RoundToInt(Mathf.Lerp(lower, upper, tz)),
                            byte.MinValue,
                            byte.MaxValue);
                    }
                }
            }

            initialized = true;
            IncrementRevision();
        }

        public void Initialize(bool full)
        {
            int resolvedResolution = ResolveResolution();
            resolution = resolvedResolution;
            pixels = new byte[resolvedResolution * resolvedResolution];
            if (full)
            {
                for (int index = 0; index < pixels.Length; index++)
                {
                    pixels[index] = byte.MaxValue;
                }
            }

            initialized = true;
            IncrementRevision();
        }

        public void Fill(float value)
        {
            EnsureInitialized(false);
            byte byteValue = ToByte(value);
            bool changed = false;
            for (int index = 0; index < pixels.Length; index++)
            {
                if (pixels[index] == byteValue)
                {
                    continue;
                }

                pixels[index] = byteValue;
                changed = true;
            }

            if (changed)
            {
                IncrementRevision();
            }
        }

        public float CalculateAverageCoverage()
        {
            if (!initialized || !HasValidStorage() || pixels.Length == 0)
            {
                return 0f;
            }

            long sum = 0L;
            for (int index = 0; index < pixels.Length; index++)
            {
                sum += pixels[index];
            }

            return Mathf.Clamp01(
                (float)(sum / (pixels.Length * 255.0)));
        }

        public bool TrySample(
            GeneratedGround ground,
            Vector3 worldPosition,
            out float coverage)
        {
            coverage = 0f;
            if (ground == null ||
                !ground.TryWorldToSurfaceNormalizedXZ(
                    worldPosition,
                    out float normalizedX,
                    out float normalizedZ))
            {
                return false;
            }

            if (!initialized || !HasValidStorage())
            {
                return true;
            }

            coverage = SampleNormalized(normalizedX, normalizedZ);
            return true;
        }

        public bool Paint(
            GeneratedGround ground,
            Vector3 worldPosition,
            float brushRadiusMetres,
            float brushStrength,
            bool erase)
        {
            if (ground == null ||
                !ground.TryGetSurfaceDomain(
                    out float ignoredHalfSize,
                    out float domainSize) ||
                !ground.TryWorldToSurfaceNormalizedXZ(
                    worldPosition,
                    out float normalizedX,
                    out float normalizedZ))
            {
                return false;
            }

            EnsureInitialized(false);
            int resolvedResolution = ResolveResolution();
            float radiusNormalized = Mathf.Max(0.0001f, brushRadiusMetres) /
                Mathf.Max(0.0001f, domainSize);
            float radiusTexels = radiusNormalized * (resolvedResolution - 1);
            float centerX = normalizedX * (resolvedResolution - 1);
            float centerZ = normalizedZ * (resolvedResolution - 1);
            int minimumX = Mathf.Clamp(
                Mathf.FloorToInt(centerX - radiusTexels),
                0,
                resolvedResolution - 1);
            int maximumX = Mathf.Clamp(
                Mathf.CeilToInt(centerX + radiusTexels),
                0,
                resolvedResolution - 1);
            int minimumZ = Mathf.Clamp(
                Mathf.FloorToInt(centerZ - radiusTexels),
                0,
                resolvedResolution - 1);
            int maximumZ = Mathf.Clamp(
                Mathf.CeilToInt(centerZ + radiusTexels),
                0,
                resolvedResolution - 1);
            float signedStrength = Mathf.Clamp01(brushStrength) *
                (erase ? -1f : 1f);
            bool changed = false;

            for (int z = minimumZ; z <= maximumZ; z++)
            {
                for (int x = minimumX; x <= maximumX; x++)
                {
                    float deltaX = x - centerX;
                    float deltaZ = z - centerZ;
                    float distance = Mathf.Sqrt(
                        deltaX * deltaX + deltaZ * deltaZ);
                    if (distance > radiusTexels)
                    {
                        continue;
                    }

                    float falloff = 1f - Mathf.Clamp01(
                        distance / Mathf.Max(radiusTexels, 0.0001f));
                    int index = z * resolvedResolution + x;
                    float current = pixels[index] / 255f;
                    float next = Mathf.Clamp01(
                        current + signedStrength * falloff);
                    byte nextByte = ToByte(next);
                    if (nextByte == pixels[index])
                    {
                        continue;
                    }

                    pixels[index] = nextByte;
                    changed = true;
                }
            }

            if (changed)
            {
                IncrementRevision();
            }

            return changed;
        }

#if UNITY_EDITOR
        public bool TryGetTexelWorldPosition(
            GeneratedGround ground,
            int x,
            int z,
            out Vector3 worldPosition,
            out float coverage)
        {
            worldPosition = Vector3.zero;
            coverage = 0f;
            int resolvedResolution = ResolveResolution();
            if (ground == null ||
                x < 0 || x >= resolvedResolution ||
                z < 0 || z >= resolvedResolution ||
                !ground.TryGetSurfaceWorldPosition(
                    x / (float)(resolvedResolution - 1),
                    z / (float)(resolvedResolution - 1),
                    out worldPosition))
            {
                return false;
            }

            if (initialized && HasValidStorage())
            {
                coverage = pixels[z * resolvedResolution + x] / 255f;
            }

            return true;
        }
#endif

        private void EnsureInitialized(bool full)
        {
            if (!initialized || !HasValidStorage())
            {
                Initialize(full);
            }
        }

        private float SampleNormalized(float normalizedX, float normalizedZ)
        {
            int resolvedResolution = ResolveResolution();
            float gridX = Mathf.Clamp01(normalizedX) *
                (resolvedResolution - 1);
            float gridZ = Mathf.Clamp01(normalizedZ) *
                (resolvedResolution - 1);
            int x0 = Mathf.Clamp(
                Mathf.FloorToInt(gridX),
                0,
                resolvedResolution - 1);
            int z0 = Mathf.Clamp(
                Mathf.FloorToInt(gridZ),
                0,
                resolvedResolution - 1);
            int x1 = Mathf.Min(x0 + 1, resolvedResolution - 1);
            int z1 = Mathf.Min(z0 + 1, resolvedResolution - 1);
            float tx = Mathf.Clamp01(gridX - x0);
            float tz = Mathf.Clamp01(gridZ - z0);
            float lower = Mathf.Lerp(
                pixels[z0 * resolvedResolution + x0],
                pixels[z0 * resolvedResolution + x1],
                tx);
            float upper = Mathf.Lerp(
                pixels[z1 * resolvedResolution + x0],
                pixels[z1 * resolvedResolution + x1],
                tx);
            return Mathf.Lerp(lower, upper, tz) / 255f;
        }

        private int ResolveResolution()
        {
            return Mathf.Clamp(
                resolution,
                MinimumResolution,
                MaximumResolution);
        }

        private bool HasValidStorage()
        {
            int resolvedResolution = ResolveResolution();
            return pixels != null &&
                pixels.Length == resolvedResolution * resolvedResolution;
        }

        private void IncrementRevision()
        {
            revision = revision == int.MaxValue ? 1 : revision + 1;
        }

        private static byte ToByte(float value)
        {
            return (byte)Mathf.Clamp(
                Mathf.RoundToInt(Mathf.Clamp01(value) * 255f),
                byte.MinValue,
                byte.MaxValue);
        }
    }
}
