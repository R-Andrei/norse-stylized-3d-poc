using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    /// <summary>
    /// Geometry and rendering metadata sampled from the generated ground. Height
    /// and Normal describe the immutable pre-river surface used for corridor
    /// fitting. RenderNormal describes the visible broad-ground shading normal.
    /// Hidden river-concealment geometry may change vertex positions without
    /// changing this visible normal field.
    /// </summary>
    public readonly struct GroundSurfaceSample
    {
        public GroundSurfaceSample(
            float height,
            Vector3 normal,
            Vector3 renderNormal,
            float surfaceVariation,
            float exposure,
            float dampDeposit,
            float vegetationSuitability,
            float materialClassification)
        {
            Height = height;
            Normal = ResolveNormal(normal);
            RenderNormal = ResolveNormal(renderNormal);
            SurfaceVariation = Mathf.Clamp01(surfaceVariation);
            Exposure = Mathf.Clamp01(exposure);
            DampDeposit = Mathf.Clamp01(dampDeposit);
            VegetationSuitability = Mathf.Clamp01(vegetationSuitability);
            MaterialClassification = materialClassification;
        }

        public GroundSurfaceSample(
            float height,
            Vector3 normal,
            Vector3 renderNormal,
            float surfaceVariation,
            float materialClassification)
            : this(
                height,
                normal,
                renderNormal,
                surfaceVariation,
                0.5f,
                0.5f,
                1f,
                materialClassification)
        {
        }

        public GroundSurfaceSample(
            float height,
            Vector3 normal,
            float surfaceVariation,
            float materialClassification)
            : this(
                height,
                normal,
                normal,
                surfaceVariation,
                materialClassification)
        {
        }

        public float Height { get; }
        public Vector3 Normal { get; }
        public Vector3 RenderNormal { get; }

        /// <summary>
        /// Vertex Color R. Broad tonal patch variation. Kept as the historical
        /// surface variation value so older shader/material assumptions remain
        /// compatible.
        /// </summary>
        public float SurfaceVariation { get; }

        /// <summary>
        /// Vertex Color G. Up/high/exposed places that can hold snow, frost, or
        /// light surface accumulation in future systems.
        /// </summary>
        public float Exposure { get; }

        /// <summary>
        /// Vertex Color B. Low/flat/shore-biased places that can collect damp
        /// deposits, dark mud, or waterlogging in future systems.
        /// </summary>
        public float DampDeposit { get; }

        /// <summary>
        /// Vertex Color A. Static suitability for future grass, moss, or other
        /// low vegetation. Runtime trampling/compression should remain separate.
        /// </summary>
        public float VegetationSuitability { get; }

        public float MaterialClassification { get; }

        private static Vector3 ResolveNormal(Vector3 value)
        {
            return value.sqrMagnitude > 0.000001f
                ? value.normalized
                : Vector3.up;
        }
    }

    /// <summary>
    /// Immutable local-space snapshot of generated ground after ordinary ground
    /// modifiers. It retains the pre-river geometry, the visible broad-ground
    /// normal field, and material metadata. Sampling mirrors the checkerboard
    /// triangle layout used by the generated ground mesh.
    /// </summary>
    public sealed class GroundHeightFieldSnapshot
    {
        private readonly float[] baseHeights;
        private readonly Vector3[] baseNormals;
        private readonly Vector3[] renderNormals;
        private readonly float[] surfaceVariations;
        private readonly float[] exposureMasks;
        private readonly float[] dampDepositMasks;
        private readonly float[] vegetationSuitabilityMasks;
        private readonly float[] materialClassifications;
        private readonly int triangulationSeed;

        public GroundHeightFieldSnapshot(
            float[] baseHeights,
            Vector3[] baseNormals,
            Vector3[] renderNormals,
            float[] surfaceVariations,
            float[] materialClassifications,
            int resolution,
            float spacing,
            float halfSize,
            int triangulationSeed)
            : this(
                baseHeights,
                baseNormals,
                renderNormals,
                surfaceVariations,
                CreateFilled(baseHeights, 0.5f),
                CreateFilled(baseHeights, 0.5f),
                CreateFilled(baseHeights, 1f),
                materialClassifications,
                resolution,
                spacing,
                halfSize,
                triangulationSeed)
        {
        }

        public GroundHeightFieldSnapshot(
            float[] baseHeights,
            Vector3[] baseNormals,
            Vector3[] renderNormals,
            float[] surfaceVariations,
            float[] exposureMasks,
            float[] dampDepositMasks,
            float[] vegetationSuitabilityMasks,
            float[] materialClassifications,
            int resolution,
            float spacing,
            float halfSize,
            int triangulationSeed)
        {
            this.baseHeights = CloneOrEmpty(baseHeights);
            this.baseNormals = CloneOrEmpty(baseNormals);
            this.renderNormals = CloneOrEmpty(renderNormals);
            this.surfaceVariations = CloneOrEmpty(surfaceVariations);
            this.exposureMasks = CloneOrEmpty(exposureMasks);
            this.dampDepositMasks = CloneOrEmpty(dampDepositMasks);
            this.vegetationSuitabilityMasks =
                CloneOrEmpty(vegetationSuitabilityMasks);
            this.materialClassifications = CloneOrEmpty(materialClassifications);

            Resolution = Mathf.Max(0, resolution);
            Spacing = Mathf.Max(0.0001f, spacing);
            HalfSize = Mathf.Max(0f, halfSize);
            this.triangulationSeed = triangulationSeed;
        }

        public static GroundHeightFieldSnapshot Empty { get; } =
            new GroundHeightFieldSnapshot(
                Array.Empty<float>(),
                Array.Empty<Vector3>(),
                Array.Empty<Vector3>(),
                Array.Empty<float>(),
                Array.Empty<float>(),
                Array.Empty<float>(),
                Array.Empty<float>(),
                Array.Empty<float>(),
                0,
                1f,
                0f,
                0);

        public int Resolution { get; }
        public float Spacing { get; }
        public float HalfSize { get; }

        public bool IsValid
        {
            get
            {
                int expected = Resolution * Resolution;
                return Resolution >= 2 &&
                       baseHeights.Length == expected &&
                       baseNormals.Length == expected &&
                       renderNormals.Length == expected &&
                       surfaceVariations.Length == expected &&
                       exposureMasks.Length == expected &&
                       dampDepositMasks.Length == expected &&
                       vegetationSuitabilityMasks.Length == expected &&
                       materialClassifications.Length == expected;
            }
        }

        public bool TrySample(
            Vector2 localPoint,
            out float height,
            out Vector3 normal)
        {
            bool succeeded = TrySample(localPoint, out GroundSurfaceSample sample);
            height = sample.Height;
            normal = sample.Normal;
            return succeeded;
        }

        public bool TrySample(
            Vector2 localPoint,
            out GroundSurfaceSample sample)
        {
            sample = new GroundSurfaceSample(0f, Vector3.up, 0.5f, 0f);

            if (!TryResolveGridPoint(
                    localPoint,
                    out float gridX,
                    out float gridZ))
            {
                return false;
            }

            sample = new GroundSurfaceSample(
                SampleTriangulated(baseHeights, gridX, gridZ),
                SampleTriangulated(baseNormals, gridX, gridZ),
                SampleTriangulated(renderNormals, gridX, gridZ),
                SampleTriangulated(surfaceVariations, gridX, gridZ),
                SampleTriangulated(exposureMasks, gridX, gridZ),
                SampleTriangulated(dampDepositMasks, gridX, gridZ),
                SampleTriangulated(vegetationSuitabilityMasks, gridX, gridZ),
                SampleTriangulated(materialClassifications, gridX, gridZ));

            return true;
        }

        private bool TryResolveGridPoint(
            Vector2 localPoint,
            out float gridX,
            out float gridZ)
        {
            gridX = 0f;
            gridZ = 0f;

            if (!IsValid)
            {
                return false;
            }

            gridX = (localPoint.x + HalfSize) / Spacing;
            gridZ = (localPoint.y + HalfSize) / Spacing;
            float maximum = Resolution - 1f;

            return gridX >= 0f &&
                   gridZ >= 0f &&
                   gridX <= maximum &&
                   gridZ <= maximum;
        }

        private float SampleTriangulated(
            float[] values,
            float gridX,
            float gridZ)
        {
            ResolveTriangle(
                gridX,
                gridZ,
                out int i0,
                out int i1,
                out int i2,
                out Vector3 weights);

            return values[i0] * weights.x +
                   values[i1] * weights.y +
                   values[i2] * weights.z;
        }

        private Vector3 SampleTriangulated(
            Vector3[] values,
            float gridX,
            float gridZ)
        {
            ResolveTriangle(
                gridX,
                gridZ,
                out int i0,
                out int i1,
                out int i2,
                out Vector3 weights);

            Vector3 value =
                values[i0] * weights.x +
                values[i1] * weights.y +
                values[i2] * weights.z;

            return value.sqrMagnitude > 0.000001f
                ? value.normalized
                : Vector3.up;
        }

        private void ResolveTriangle(
            float gridX,
            float gridZ,
            out int i0,
            out int i1,
            out int i2,
            out Vector3 weights)
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

            int a = z0 * Resolution + x0;
            int b = z0 * Resolution + x1;
            int c = z1 * Resolution + x0;
            int d = z1 * Resolution + x1;

            bool alternate =
                ((x0 + z0 + triangulationSeed) & 1) == 0;

            if (alternate)
            {
                if (tx + tz <= 1f)
                {
                    i0 = a;
                    i1 = b;
                    i2 = c;
                    weights = new Vector3(1f - tx - tz, tx, tz);
                    return;
                }

                i0 = b;
                i1 = c;
                i2 = d;
                weights = new Vector3(1f - tz, 1f - tx, tx + tz - 1f);
                return;
            }

            if (tx >= tz)
            {
                i0 = a;
                i1 = b;
                i2 = d;
                weights = new Vector3(1f - tx, tx - tz, tz);
                return;
            }

            i0 = a;
            i1 = c;
            i2 = d;
            weights = new Vector3(1f - tz, tz - tx, tx);
        }

        private static float[] CloneOrEmpty(float[] source)
        {
            return source != null
                ? (float[])source.Clone()
                : Array.Empty<float>();
        }

        private static Vector3[] CloneOrEmpty(Vector3[] source)
        {
            return source != null
                ? (Vector3[])source.Clone()
                : Array.Empty<Vector3>();
        }

        private static float[] CreateFilled(float[] source, float value)
        {
            int length = source != null ? source.Length : 0;
            float[] result = new float[length];

            for (int index = 0; index < result.Length; index++)
            {
                result[index] = value;
            }

            return result;
        }
    }
}
