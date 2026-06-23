using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    /// <summary>
    /// Immutable local-space snapshot of generated ground after ordinary ground
    /// modifiers, but before river concealment is applied. Sampling mirrors the
    /// actual checkerboard triangle layout used by the generated ground mesh so
    /// corridor handoff vertices can meet that mesh instead of a bilinear
    /// approximation of it.
    /// </summary>
    public sealed class GroundHeightFieldSnapshot
    {
        private readonly float[] heights;
        private readonly int triangulationSeed;

        public GroundHeightFieldSnapshot(
            float[] heights,
            int resolution,
            float spacing,
            float halfSize,
            int triangulationSeed)
        {
            this.heights =
                heights != null
                    ? (float[])heights.Clone()
                    : Array.Empty<float>();

            Resolution = Mathf.Max(0, resolution);
            Spacing = Mathf.Max(0.0001f, spacing);
            HalfSize = Mathf.Max(0f, halfSize);
            this.triangulationSeed = triangulationSeed;
        }

        public static GroundHeightFieldSnapshot Empty { get; } =
            new GroundHeightFieldSnapshot(
                Array.Empty<float>(),
                0,
                1f,
                0f,
                0);

        public int Resolution { get; }
        public float Spacing { get; }
        public float HalfSize { get; }

        public bool IsValid =>
            Resolution >= 2 &&
            heights.Length == Resolution * Resolution;

        public bool TrySample(
            Vector2 localPoint,
            out float height,
            out Vector3 normal)
        {
            height = 0f;
            normal = Vector3.up;

            if (!IsValid)
            {
                return false;
            }

            float gridX = (localPoint.x + HalfSize) / Spacing;
            float gridZ = (localPoint.y + HalfSize) / Spacing;
            float maximum = Resolution - 1f;

            if (gridX < 0f ||
                gridZ < 0f ||
                gridX > maximum ||
                gridZ > maximum)
            {
                return false;
            }

            height = SampleTriangulated(gridX, gridZ);

            const float derivativeStep = 0.25f;
            float left = SampleTriangulated(gridX - derivativeStep, gridZ);
            float right = SampleTriangulated(gridX + derivativeStep, gridZ);
            float down = SampleTriangulated(gridX, gridZ - derivativeStep);
            float up = SampleTriangulated(gridX, gridZ + derivativeStep);
            float derivativeDistance =
                Mathf.Max(0.0001f, 2f * derivativeStep * Spacing);

            normal =
                new Vector3(
                    -(right - left) / derivativeDistance,
                    1f,
                    -(up - down) / derivativeDistance).normalized;

            return true;
        }

        private float SampleTriangulated(float gridX, float gridZ)
        {
            float maximum = Resolution - 1f;
            float x = Mathf.Clamp(gridX, 0f, maximum);
            float z = Mathf.Clamp(gridZ, 0f, maximum);

            int x0 = Mathf.Min(Resolution - 2, Mathf.FloorToInt(x));
            int z0 = Mathf.Min(Resolution - 2, Mathf.FloorToInt(z));
            int x1 = x0 + 1;
            int z1 = z0 + 1;

            float tx = Mathf.Clamp01(x - x0);
            float tz = Mathf.Clamp01(z - z0);

            float h00 = heights[z0 * Resolution + x0];
            float h10 = heights[z0 * Resolution + x1];
            float h01 = heights[z1 * Resolution + x0];
            float h11 = heights[z1 * Resolution + x1];

            bool alternate =
                ((x0 + z0 + triangulationSeed) & 1) == 0;

            if (alternate)
            {
                // Same diagonal and triangle pair used by GroundGenerator:
                // (a,c,b) and (b,c,d), diagonal from b to c.
                if (tx + tz <= 1f)
                {
                    return h00 +
                           tx * (h10 - h00) +
                           tz * (h01 - h00);
                }

                return (1f - tz) * h10 +
                       (1f - tx) * h01 +
                       (tx + tz - 1f) * h11;
            }

            // Same alternate pair used by GroundGenerator:
            // (a,d,b) and (a,c,d), diagonal from a to d.
            if (tx >= tz)
            {
                return (1f - tx) * h00 +
                       (tx - tz) * h10 +
                       tz * h11;
            }

            return (1f - tz) * h00 +
                   (tz - tx) * h01 +
                   tx * h11;
        }
    }
}
