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

        public StylizedRiverGroundSnapshot(
            Vector3[] points,
            float width,
            float bankBlend,
            float depth,
            float bedFlatness,
            StylizedRiverBankProfile bankProfile,
            float strength)
        {
            this.points = points ?? Array.Empty<Vector3>();
            Width = Mathf.Max(0.5f, width);
            BankBlend = Mathf.Max(0.05f, bankBlend);
            Depth = Mathf.Max(0.05f, depth);
            BedFlatness = Mathf.Clamp01(bedFlatness);
            BankProfile = bankProfile;
            Strength = Mathf.Clamp01(strength);
        }

        public float Width { get; }
        public float BankBlend { get; }
        public float Depth { get; }
        public float BedFlatness { get; }
        public StylizedRiverBankProfile BankProfile { get; }
        public float Strength { get; }

        public bool IsValid =>
            points != null &&
            points.Length >= 2 &&
            Strength > 0f;

        public float MaximumInfluenceDistance =>
            Width * 0.5f + BankBlend;

        public bool TryEvaluate(
            Vector2 point,
            out float distance,
            out float waterHeight)
        {
            distance = float.PositiveInfinity;
            waterHeight = 0f;

            if (!IsValid)
            {
                return false;
            }

            float bestDistanceSqr =
                float.PositiveInfinity;

            for (int index = 0;
                 index < points.Length - 1;
                 index++)
            {
                Vector3 a = points[index];
                Vector3 b = points[index + 1];

                Vector2 a2 =
                    new Vector2(
                        a.x,
                        a.z);

                Vector2 b2 =
                    new Vector2(
                        b.x,
                        b.z);

                Vector2 segment =
                    b2 - a2;

                float lengthSqr =
                    segment.sqrMagnitude;

                float t =
                    lengthSqr > 0.000001f
                        ? Mathf.Clamp01(
                            Vector2.Dot(
                                point - a2,
                                segment) /
                            lengthSqr)
                        : 0f;

                Vector2 nearest =
                    a2 +
                    segment * t;

                float candidateDistanceSqr =
                    (point - nearest).sqrMagnitude;

                if (candidateDistanceSqr >=
                    bestDistanceSqr)
                {
                    continue;
                }

                bestDistanceSqr =
                    candidateDistanceSqr;

                waterHeight =
                    Mathf.Lerp(
                        a.y,
                        b.y,
                        t);
            }

            if (float.IsPositiveInfinity(
                    bestDistanceSqr))
            {
                return false;
            }

            distance =
                Mathf.Sqrt(
                    bestDistanceSqr);

            return true;
        }

        public float EvaluateInfluence(
            float distance)
        {
            float halfWidth =
                Width * 0.5f;

            if (distance <= halfWidth)
            {
                return Strength;
            }

            float outside =
                distance - halfWidth;

            if (outside >= BankBlend)
            {
                return 0f;
            }

            float t =
                Mathf.Clamp01(
                    outside /
                    BankBlend);

            float falloff =
                BankProfile switch
                {
                    StylizedRiverBankProfile.Gentle =>
                        1f - Smooth01(t),

                    StylizedRiverBankProfile.Natural =>
                        Mathf.Pow(
                            1f - t,
                            1.35f),

                    StylizedRiverBankProfile.Steep =>
                        Mathf.Pow(
                            1f - t,
                            0.55f),

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

            return
                Mathf.Clamp01(
                    falloff) *
                Strength;
        }

        public float EvaluateTargetHeight(
            float distance,
            float waterHeight)
        {
            float halfWidth =
                Width * 0.5f;

            if (distance >= halfWidth)
            {
                float outsideT =
                    Mathf.Clamp01(
                        (distance - halfWidth) /
                        BankBlend);

                float edgeDepth =
                    Mathf.Lerp(
                        0.06f,
                        0f,
                        Smooth01(outsideT));

                return
                    waterHeight -
                    edgeDepth;
            }

            float flatRadius =
                halfWidth *
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
                        halfWidth,
                        distance);

                depthFactor =
                    1f -
                    Smooth01(slopeT);
            }

            return
                waterHeight -
                Depth *
                depthFactor;
        }

        private static float Smooth01(
            float value)
        {
            value =
                Mathf.Clamp01(
                    value);

            return
                value *
                value *
                (3f -
                 2f *
                 value);
        }
    }
}
