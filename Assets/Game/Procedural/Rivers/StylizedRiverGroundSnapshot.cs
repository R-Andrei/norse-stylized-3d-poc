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

    public readonly struct StylizedRiverGroundSnapshot
    {
        private readonly Vector3[] points;
        private readonly float[] halfWidths;
        private readonly float maximumHalfWidth;

        public StylizedRiverGroundSnapshot(
            Vector3[] points,
            float[] halfWidths,
            float bankBlend,
            float depth,
            float bedFlatness,
            StylizedRiverBankProfile bankProfile,
            float strength)
        {
            this.points = points ?? Array.Empty<Vector3>();
            this.halfWidths = ResolveHalfWidths(this.points, halfWidths);
            maximumHalfWidth = FindMaximum(this.halfWidths);
            BankBlend = Mathf.Max(0.05f, bankBlend);
            Depth = Mathf.Max(0.05f, depth);
            BedFlatness = Mathf.Clamp01(bedFlatness);
            BankProfile = bankProfile;
            Strength = Mathf.Clamp01(strength);
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
                BuildUniformHalfWidths(points, Mathf.Max(0.5f, width) * 0.5f),
                bankBlend,
                depth,
                bedFlatness,
                bankProfile,
                strength)
        {
        }

        public float Width => maximumHalfWidth * 2f;
        public float BankBlend { get; }
        public float Depth { get; }
        public float BedFlatness { get; }
        public StylizedRiverBankProfile BankProfile { get; }
        public float Strength { get; }

        public bool IsValid =>
            points != null &&
            halfWidths != null &&
            points.Length >= 2 &&
            halfWidths.Length == points.Length &&
            Strength > 0f;

        public float MaximumInfluenceDistance =>
            maximumHalfWidth + BankBlend;

        public bool TryEvaluate(
            Vector2 point,
            out float distance,
            out float waterHeight,
            out float halfWidth)
        {
            distance = float.PositiveInfinity;
            waterHeight = 0f;
            halfWidth = 0f;

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
                halfWidth =
                    Mathf.Lerp(
                        halfWidths[index],
                        halfWidths[index + 1],
                        t);
            }

            if (float.IsPositiveInfinity(bestDistanceSqr))
            {
                return false;
            }

            distance = Mathf.Sqrt(bestDistanceSqr);
            return true;
        }

        public bool TryEvaluate(
            Vector2 point,
            out float distance,
            out float waterHeight)
        {
            return TryEvaluate(
                point,
                out distance,
                out waterHeight,
                out _);
        }

        public float EvaluateInfluence(
            float distance,
            float halfWidth)
        {
            float resolvedHalfWidth = Mathf.Max(0.25f, halfWidth);

            if (distance <= resolvedHalfWidth)
            {
                return Strength;
            }

            float outside = distance - resolvedHalfWidth;

            if (outside >= BankBlend)
            {
                return 0f;
            }

            float t = Mathf.Clamp01(outside / BankBlend);

            float falloff =
                BankProfile switch
                {
                    StylizedRiverBankProfile.Gentle =>
                        1f - Smooth01(t),

                    StylizedRiverBankProfile.Natural =>
                        Mathf.Pow(1f - t, 1.35f),

                    StylizedRiverBankProfile.Steep =>
                        Mathf.Pow(1f - t, 0.55f),

                    StylizedRiverBankProfile.Square =>
                        t < 0.82f
                            ? 1f
                            : 1f - Smooth01(
                                Mathf.InverseLerp(
                                    0.82f,
                                    1f,
                                    t)),

                    _ =>
                        1f - Smooth01(t)
                };

            return Mathf.Clamp01(falloff) * Strength;
        }

        public float EvaluateInfluence(float distance)
        {
            return EvaluateInfluence(distance, maximumHalfWidth);
        }

        public float EvaluateTargetHeight(
            float distance,
            float waterHeight,
            float halfWidth)
        {
            float resolvedHalfWidth = Mathf.Max(0.25f, halfWidth);

            if (distance >= resolvedHalfWidth)
            {
                float outsideT =
                    Mathf.Clamp01(
                        (distance - resolvedHalfWidth) /
                        BankBlend);

                float edgeDepth =
                    Mathf.Lerp(
                        0.06f,
                        0f,
                        Smooth01(outsideT));

                return waterHeight - edgeDepth;
            }

            float flatRadius =
                resolvedHalfWidth *
                Mathf.Lerp(
                    0.05f,
                    0.72f,
                    BedFlatness);

            float depthFactor;

            if (distance <= flatRadius)
            {
                depthFactor = 1f;
            }
            else
            {
                float slopeT =
                    Mathf.InverseLerp(
                        flatRadius,
                        resolvedHalfWidth,
                        distance);

                depthFactor = 1f - Smooth01(slopeT);
            }

            return waterHeight - Depth * depthFactor;
        }

        public float EvaluateTargetHeight(
            float distance,
            float waterHeight)
        {
            return EvaluateTargetHeight(
                distance,
                waterHeight,
                maximumHalfWidth);
        }

        private static float[] ResolveHalfWidths(
            Vector3[] sourcePoints,
            float[] sourceHalfWidths)
        {
            int count = sourcePoints != null ? sourcePoints.Length : 0;

            if (count == 0)
            {
                return Array.Empty<float>();
            }

            if (sourceHalfWidths != null && sourceHalfWidths.Length == count)
            {
                float[] copy = new float[count];

                for (int index = 0; index < count; index++)
                {
                    copy[index] = Mathf.Max(0.25f, sourceHalfWidths[index]);
                }

                return copy;
            }

            return BuildUniformHalfWidths(sourcePoints, 0.25f);
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
            float maximum = 0.25f;

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

        private static float Smooth01(float value)
        {
            value = Mathf.Clamp01(value);

            return
                value *
                value *
                (3f - 2f * value);
        }
    }
}
