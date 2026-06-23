using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public enum StylizedRiverBankProfile
    {
        Gentle,
        Natural,
        Steep,
        Square
    }

    /// <summary>
    /// Immutable ground-space river data used only to conceal the broad generated
    /// ground beneath the dedicated river corridor. Visible bed and shoreline
    /// geometry are owned by the corridor mesh, not by the ground grid.
    /// </summary>
    public readonly struct StylizedRiverGroundSnapshot
    {
        private readonly Vector3[] points;
        private readonly float[] visibleHalfWidths;
        private readonly float[] surfaceHalfWidths;
        private readonly float maximumSurfaceHalfWidth;

        public StylizedRiverGroundSnapshot(
            Vector3[] points,
            float[] visibleHalfWidths,
            float[] surfaceHalfWidths,
            float bankBlend,
            float depth,
            float bedFlatness,
            StylizedRiverBankProfile bankProfile,
            float terrainConformity,
            float groundGridSpacing,
            float wetClearance,
            float bankCover,
            float reservedDownwardDisplacement)
        {
            this.points = points ?? Array.Empty<Vector3>();
            this.visibleHalfWidths =
                ResolveHalfWidths(this.points, visibleHalfWidths);
            this.surfaceHalfWidths =
                ResolveSurfaceHalfWidths(
                    this.points,
                    surfaceHalfWidths,
                    this.visibleHalfWidths);

            maximumSurfaceHalfWidth = FindMaximum(this.surfaceHalfWidths);
            BankBlend = Mathf.Max(0.1f, bankBlend);
            Depth = Mathf.Max(0.05f, depth);
            BedFlatness = Mathf.Clamp01(bedFlatness);
            BankProfile = bankProfile;
            TerrainConformity = Mathf.Clamp01(terrainConformity);
            GroundGridSpacing = Mathf.Max(0.01f, groundGridSpacing);
            WetClearance = Mathf.Max(0.005f, wetClearance);
            BankCover = Mathf.Max(0.005f, bankCover);
            ReservedDownwardDisplacement =
                Mathf.Max(0f, reservedDownwardDisplacement);
        }

        public StylizedRiverGroundSnapshot(
            Vector3[] points,
            float[] halfWidths,
            float bankBlend,
            float depth,
            float bedFlatness,
            StylizedRiverBankProfile bankProfile,
            float strength)
            : this(
                points,
                halfWidths,
                halfWidths,
                bankBlend,
                depth,
                bedFlatness,
                bankProfile,
                strength,
                0.5f,
                0.04f,
                0.04f,
                0f)
        {
        }

        public StylizedRiverGroundSnapshot(
            Vector3[] points,
            float width,
            float bankBlend,
            float depth,
            float bedFlatness,
            StylizedRiverBankProfile bankProfile,
            float strength)
            : this(
                points,
                BuildUniformHalfWidths(
                    points,
                    Mathf.Max(0.5f, width) * 0.5f),
                bankBlend,
                depth,
                bedFlatness,
                bankProfile,
                strength)
        {
        }

        public float BankBlend { get; }
        public float Depth { get; }
        public float BedFlatness { get; }
        public StylizedRiverBankProfile BankProfile { get; }
        public float TerrainConformity { get; }
        public float Strength => TerrainConformity;
        public float GroundGridSpacing { get; }
        public float WetClearance { get; }
        public float BankCover { get; }
        public float ReservedDownwardDisplacement { get; }
        public float RequiredWetClearance =>
            WetClearance + ReservedDownwardDisplacement;

        public bool IsValid =>
            points != null &&
            visibleHalfWidths != null &&
            surfaceHalfWidths != null &&
            points.Length >= 2 &&
            visibleHalfWidths.Length == points.Length &&
            surfaceHalfWidths.Length == points.Length;

        public float MaximumInfluenceDistance =>
            maximumSurfaceHalfWidth + BankBlend;

        public bool TryEvaluate(
            Vector2 point,
            out float distance,
            out float waterHeight,
            out float visibleHalfWidth,
            out float surfaceHalfWidth)
        {
            distance = float.PositiveInfinity;
            waterHeight = 0f;
            visibleHalfWidth = 0f;
            surfaceHalfWidth = 0f;

            if (!IsValid)
            {
                return false;
            }

            float bestDistanceSqr = float.PositiveInfinity;

            for (int index = 0; index < points.Length - 1; index++)
            {
                Vector3 a = points[index];
                Vector3 b = points[index + 1];
                Vector2 a2 = new Vector2(a.x, a.z);
                Vector2 b2 = new Vector2(b.x, b.z);
                Vector2 segment = b2 - a2;
                float lengthSqr = segment.sqrMagnitude;

                float t =
                    lengthSqr > 0.000001f
                        ? Mathf.Clamp01(
                            Vector2.Dot(point - a2, segment) /
                            lengthSqr)
                        : 0f;

                Vector2 nearest = a2 + segment * t;
                float candidateDistanceSqr =
                    (point - nearest).sqrMagnitude;

                if (candidateDistanceSqr >= bestDistanceSqr)
                {
                    continue;
                }

                bestDistanceSqr = candidateDistanceSqr;
                waterHeight = Mathf.Lerp(a.y, b.y, t);
                visibleHalfWidth =
                    Mathf.Lerp(
                        visibleHalfWidths[index],
                        visibleHalfWidths[index + 1],
                        t);
                surfaceHalfWidth =
                    Mathf.Lerp(
                        surfaceHalfWidths[index],
                        surfaceHalfWidths[index + 1],
                        t);
            }

            if (float.IsPositiveInfinity(bestDistanceSqr))
            {
                return false;
            }

            distance = Mathf.Sqrt(bestDistanceSqr);
            return true;
        }

        /// <summary>
        /// Distance at which the visible corridor reaches the untouched base
        /// ground. The render mesh continues beyond this handoff as a buried
        /// integration apron; the broad ground is never modified past it.
        /// </summary>
        public float ResolveHandoffHalfWidth(float surfaceHalfWidth)
        {
            return Mathf.Max(0f, surfaceHalfWidth) + BankBlend;
        }

        // Compatibility name retained for older callers.
        public float ResolveOuterHalfWidth(float surfaceHalfWidth)
        {
            return ResolveHandoffHalfWidth(surfaceHalfWidth);
        }

        public float EvaluateConcealedGroundHeight(
            float originalHeight,
            float distance,
            float waterHeight,
            float surfaceHalfWidth)
        {
            float handoffHalfWidth =
                ResolveHandoffHalfWidth(surfaceHalfWidth);

            if (distance >= handoffHalfWidth)
            {
                return originalHeight;
            }

            // The broad ground is lowered decisively beneath the bed and inner
            // banks, then returns to its untouched height before the corridor
            // collider handoff. The separate buried render apron extends beyond
            // this point far enough to hide the coarse heightfield transition.
            float taperWidth =
                Mathf.Clamp(
                    Mathf.Max(
                        GroundGridSpacing * 1.75f,
                        BankBlend * 0.55f),
                    0.35f,
                    Mathf.Max(0.35f, BankBlend));

            float taperStart =
                Mathf.Max(
                    surfaceHalfWidth,
                    handoffHalfWidth - taperWidth);

            float concealWeight =
                1f - SmoothStep(
                    taperStart,
                    handoffHalfWidth,
                    distance);

            float concealDepth =
                Mathf.Max(
                    Depth + RequiredWetClearance + 0.15f,
                    0.25f);

            float concealedHeight =
                Mathf.Min(
                    originalHeight,
                    waterHeight - concealDepth);

            return Mathf.Lerp(
                originalHeight,
                concealedHeight,
                concealWeight);
        }

        private static float[] ResolveHalfWidths(
            Vector3[] sourcePoints,
            float[] sourceHalfWidths)
        {
            if (sourcePoints == null || sourcePoints.Length == 0)
            {
                return Array.Empty<float>();
            }

            if (sourceHalfWidths != null &&
                sourceHalfWidths.Length == sourcePoints.Length)
            {
                float[] copy = new float[sourceHalfWidths.Length];

                for (int index = 0; index < copy.Length; index++)
                {
                    copy[index] = Mathf.Max(0.25f, sourceHalfWidths[index]);
                }

                return copy;
            }

            return BuildUniformHalfWidths(sourcePoints, 0.5f);
        }

        private static float[] ResolveSurfaceHalfWidths(
            Vector3[] sourcePoints,
            float[] sourceSurfaceHalfWidths,
            float[] fallbackVisibleHalfWidths)
        {
            if (sourcePoints == null || sourcePoints.Length == 0)
            {
                return Array.Empty<float>();
            }

            float[] result = new float[sourcePoints.Length];

            for (int index = 0; index < result.Length; index++)
            {
                float visible =
                    fallbackVisibleHalfWidths != null &&
                    index < fallbackVisibleHalfWidths.Length
                        ? fallbackVisibleHalfWidths[index]
                        : 0.5f;

                float surface =
                    sourceSurfaceHalfWidths != null &&
                    index < sourceSurfaceHalfWidths.Length
                        ? sourceSurfaceHalfWidths[index]
                        : visible;

                result[index] = Mathf.Max(visible, surface);
            }

            return result;
        }

        private static float[] BuildUniformHalfWidths(
            Vector3[] sourcePoints,
            float halfWidth)
        {
            int count = sourcePoints != null ? sourcePoints.Length : 0;
            float[] result = new float[count];
            float resolved = Mathf.Max(0.25f, halfWidth);

            for (int index = 0; index < count; index++)
            {
                result[index] = resolved;
            }

            return result;
        }

        private static float FindMaximum(float[] values)
        {
            float maximum = 0f;

            if (values == null)
            {
                return maximum;
            }

            for (int index = 0; index < values.Length; index++)
            {
                maximum = Mathf.Max(maximum, values[index]);
            }

            return maximum;
        }

        private static float SmoothStep(
            float edge0,
            float edge1,
            float value)
        {
            float t = Mathf.InverseLerp(edge0, edge1, value);
            return t * t * (3f - 2f * t);
        }
    }
}
