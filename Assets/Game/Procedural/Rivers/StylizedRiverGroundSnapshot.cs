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
        private readonly Vector3[] sides;
        private readonly float[] leftVisibleHalfWidths;
        private readonly float[] rightVisibleHalfWidths;
        private readonly float[] leftSurfaceHalfWidths;
        private readonly float[] rightSurfaceHalfWidths;
        private readonly float maximumSurfaceHalfWidth;

        public StylizedRiverGroundSnapshot(
            Vector3[] points,
            Vector3[] sides,
            float[] leftVisibleHalfWidths,
            float[] rightVisibleHalfWidths,
            float[] leftSurfaceHalfWidths,
            float[] rightSurfaceHalfWidths,
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
            this.sides = ResolveSides(this.points, sides);
            this.leftVisibleHalfWidths =
                ResolveHalfWidths(this.points, leftVisibleHalfWidths);
            this.rightVisibleHalfWidths =
                ResolveHalfWidths(this.points, rightVisibleHalfWidths);
            this.leftSurfaceHalfWidths =
                ResolveSurfaceHalfWidths(
                    this.points,
                    leftSurfaceHalfWidths,
                    this.leftVisibleHalfWidths);
            this.rightSurfaceHalfWidths =
                ResolveSurfaceHalfWidths(
                    this.points,
                    rightSurfaceHalfWidths,
                    this.rightVisibleHalfWidths);

            maximumSurfaceHalfWidth =
                Mathf.Max(
                    FindMaximum(this.leftSurfaceHalfWidths),
                    FindMaximum(this.rightSurfaceHalfWidths));
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
            : this(
                points,
                null,
                visibleHalfWidths,
                visibleHalfWidths,
                surfaceHalfWidths,
                surfaceHalfWidths,
                bankBlend,
                depth,
                bedFlatness,
                bankProfile,
                terrainConformity,
                groundGridSpacing,
                wetClearance,
                bankCover,
                reservedDownwardDisplacement)
        {
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
            sides != null &&
            leftVisibleHalfWidths != null &&
            rightVisibleHalfWidths != null &&
            leftSurfaceHalfWidths != null &&
            rightSurfaceHalfWidths != null &&
            points.Length >= 2 &&
            sides.Length == points.Length &&
            leftVisibleHalfWidths.Length == points.Length &&
            rightVisibleHalfWidths.Length == points.Length &&
            leftSurfaceHalfWidths.Length == points.Length &&
            rightSurfaceHalfWidths.Length == points.Length;

        public float MaximumInfluenceDistance =>
            maximumSurfaceHalfWidth + BankBlend;

        public bool TryEvaluate(
            Vector2 point,
            out float distance,
            out float waterHeight,
            out float visibleHalfWidth,
            out float surfaceHalfWidth)
        {
            return TryEvaluate(
                point,
                out distance,
                out _,
                out waterHeight,
                out visibleHalfWidth,
                out surfaceHalfWidth);
        }

        public bool TryEvaluate(
            Vector2 point,
            out float distance,
            out float signedLateralDistance,
            out float waterHeight,
            out float visibleHalfWidth,
            out float surfaceHalfWidth)
        {
            distance = float.PositiveInfinity;
            signedLateralDistance = 0f;
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
                Vector2 delta = point - nearest;
                float candidateDistanceSqr = delta.sqrMagnitude;

                if (candidateDistanceSqr >= bestDistanceSqr)
                {
                    continue;
                }

                bestDistanceSqr = candidateDistanceSqr;
                waterHeight = Mathf.Lerp(a.y, b.y, t);

                Vector3 side3 =
                    Vector3.Slerp(sides[index], sides[index + 1], t);
                Vector2 side = new Vector2(side3.x, side3.z);

                if (side.sqrMagnitude <= 0.000001f)
                {
                    Vector2 tangent =
                        segment.sqrMagnitude > 0.000001f
                            ? segment.normalized
                            : Vector2.up;
                    side = new Vector2(tangent.y, -tangent.x);
                }
                else
                {
                    side.Normalize();
                }

                signedLateralDistance = Vector2.Dot(delta, side);
                bool useLeft = signedLateralDistance < 0f;

                visibleHalfWidth =
                    Mathf.Lerp(
                        useLeft
                            ? leftVisibleHalfWidths[index]
                            : rightVisibleHalfWidths[index],
                        useLeft
                            ? leftVisibleHalfWidths[index + 1]
                            : rightVisibleHalfWidths[index + 1],
                        t);

                surfaceHalfWidth =
                    Mathf.Lerp(
                        useLeft
                            ? leftSurfaceHalfWidths[index]
                            : rightSurfaceHalfWidths[index],
                        useLeft
                            ? leftSurfaceHalfWidths[index + 1]
                            : rightSurfaceHalfWidths[index + 1],
                        t);
            }

            if (float.IsPositiveInfinity(bestDistanceSqr))
            {
                return false;
            }

            distance = Mathf.Sqrt(bestDistanceSqr);
            return true;
        }

        public float ResolveHandoffHalfWidth(float surfaceHalfWidth)
        {
            return Mathf.Max(0f, surfaceHalfWidth) + BankBlend;
        }

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

        private static Vector3[] ResolveSides(
            Vector3[] sourcePoints,
            Vector3[] sourceSides)
        {
            int count = sourcePoints != null ? sourcePoints.Length : 0;
            Vector3[] result = new Vector3[count];

            for (int index = 0; index < count; index++)
            {
                Vector3 side =
                    sourceSides != null && index < sourceSides.Length
                        ? sourceSides[index]
                        : Vector3.zero;

                if (side.sqrMagnitude <= 0.000001f)
                {
                    Vector3 previous =
                        sourcePoints[Mathf.Max(0, index - 1)];
                    Vector3 next =
                        sourcePoints[Mathf.Min(count - 1, index + 1)];
                    Vector3 tangent = next - previous;
                    tangent.y = 0f;
                    side =
                        tangent.sqrMagnitude > 0.000001f
                            ? Vector3.Cross(Vector3.up, tangent.normalized)
                            : Vector3.right;
                }

                side.y = 0f;
                result[index] =
                    side.sqrMagnitude > 0.000001f
                        ? side.normalized
                        : Vector3.right;
            }

            return result;
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
