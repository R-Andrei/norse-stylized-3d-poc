using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    /// <summary>
    /// Immutable, arc-length-resampled description of one authored river segment.
    /// This is the authoritative coordinate source for mesh generation, terrain,
    /// runtime projection, transport, interaction, foam, and later rendering systems.
    /// </summary>
    public sealed class RiverDomainSnapshot
    {
        private static readonly StylizedRiverSplineSample[] EmptySamples =
            Array.Empty<StylizedRiverSplineSample>();

        private readonly StylizedRiverSplineSample[] samples;

        public RiverDomainSnapshot(
            StylizedRiverSplineSample[] samples,
            float localLength,
            float requestedSampleSpacing,
            float connectedDistanceOffset,
            bool reverseFlow,
            int version)
        {
            this.samples =
                samples != null && samples.Length > 0
                    ? (StylizedRiverSplineSample[])samples.Clone()
                    : EmptySamples;
            LocalLength = Mathf.Max(0f, localLength);
            RequestedSampleSpacing = Mathf.Max(0.01f, requestedSampleSpacing);
            ConnectedDistanceOffset = connectedDistanceOffset;
            ReverseFlow = reverseFlow;
            Version = Mathf.Max(0, version);

            CalculateSpacingRange(
                this.samples,
                out float minimumSpacing,
                out float maximumSpacing);

            MinimumSampleSpacing = minimumSpacing;
            MaximumSampleSpacing = maximumSpacing;
            Bounds = CalculateBounds(this.samples);
        }

        public static RiverDomainSnapshot Empty { get; } =
            new RiverDomainSnapshot(
                EmptySamples,
                0f,
                0.5f,
                0f,
                false,
                0);

        public IReadOnlyList<StylizedRiverSplineSample> Samples => samples;
        public int SampleCount => samples.Length;
        public float LocalLength { get; }
        public float RequestedSampleSpacing { get; }
        public float MinimumSampleSpacing { get; }
        public float MaximumSampleSpacing { get; }
        public float ConnectedDistanceOffset { get; }
        public bool ReverseFlow { get; }
        public int Version { get; }
        public Bounds Bounds { get; }

        public float GlobalDistanceMinimum => ConnectedDistanceOffset;
        public float GlobalDistanceMaximum => ConnectedDistanceOffset + LocalLength;

        public bool IsValid =>
            samples.Length >= 2 &&
            LocalLength > 0.0001f;

        public bool ValidateContract(out string report)
        {
            StringBuilder builder = new StringBuilder();
            int errorCount = 0;

            if (!IsValid)
            {
                report = "River domain is invalid or contains fewer than two samples.";
                return false;
            }

            float expectedSpacing =
                LocalLength /
                Mathf.Max(1, SampleCount - 1);

            float spacingTolerance =
                Mathf.Max(0.001f, expectedSpacing * 0.02f);

            for (int index = 0; index < SampleCount; index++)
            {
                StylizedRiverSplineSample sample = samples[index];

                if (sample.HalfWidth <= 0f ||
                    sample.SurfaceHalfWidth < sample.HalfWidth)
                {
                    AppendError(
                        builder,
                        ref errorCount,
                        $"Sample {index} has an invalid width contract.");
                }

                if (Mathf.Abs(sample.Tangent.magnitude - 1f) > 0.01f ||
                    Mathf.Abs(sample.Side.magnitude - 1f) > 0.01f ||
                    Mathf.Abs(sample.Up.magnitude - 1f) > 0.01f)
                {
                    AppendError(
                        builder,
                        ref errorCount,
                        $"Sample {index} contains a non-normalized frame vector.");
                }

                if (Mathf.Abs(Vector3.Dot(sample.Tangent, sample.Side)) > 0.01f ||
                    Mathf.Abs(Vector3.Dot(sample.Tangent, sample.Up)) > 0.01f ||
                    Mathf.Abs(Vector3.Dot(sample.Side, sample.Up)) > 0.01f)
                {
                    AppendError(
                        builder,
                        ref errorCount,
                        $"Sample {index} contains a non-orthogonal frame.");
                }

                float expectedOriented =
                    ReverseFlow
                        ? LocalLength - sample.Distance
                        : sample.Distance;

                if (Mathf.Abs(sample.OrientedDistance - expectedOriented) > 0.002f ||
                    Mathf.Abs(
                        sample.GlobalDistance -
                        (ConnectedDistanceOffset + expectedOriented)) > 0.002f)
                {
                    AppendError(
                        builder,
                        ref errorCount,
                        $"Sample {index} violates the oriented/global distance contract.");
                }

                if (index == 0)
                {
                    continue;
                }

                float spacing =
                    sample.Distance -
                    samples[index - 1].Distance;

                if (spacing <= 0f)
                {
                    AppendError(
                        builder,
                        ref errorCount,
                        $"Local distance is not strictly increasing at sample {index}.");
                }
                else if (Mathf.Abs(spacing - expectedSpacing) > spacingTolerance)
                {
                    AppendError(
                        builder,
                        ref errorCount,
                        $"Sample spacing deviates at sample {index}: {spacing:0.0000} m.");
                }
            }

            if (errorCount > 0)
            {
                report =
                    $"River domain contract failed with {errorCount} issue(s).\n" +
                    builder;
                return false;
            }

            report =
                $"River domain contract passed: {SampleCount:N0} samples, " +
                $"{LocalLength:0.000} m long, {expectedSpacing:0.000} m uniform spacing, " +
                $"global range {GlobalDistanceMinimum:0.000}–{GlobalDistanceMaximum:0.000} m.";
            return true;
        }

        public StylizedRiverSplineSample SampleAtLocalDistance(
            float localDistance)
        {
            if (samples.Length == 0)
            {
                return default;
            }

            float clampedDistance =
                Mathf.Clamp(
                    localDistance,
                    0f,
                    LocalLength);

            int upperIndex = FindUpperSampleIndex(clampedDistance);

            if (upperIndex <= 0)
            {
                return samples[0];
            }

            if (upperIndex >= samples.Length)
            {
                return samples[samples.Length - 1];
            }

            StylizedRiverSplineSample a = samples[upperIndex - 1];
            StylizedRiverSplineSample b = samples[upperIndex];

            float t =
                Mathf.InverseLerp(
                    a.Distance,
                    b.Distance,
                    clampedDistance);

            return StylizedRiverGeometry.InterpolateSample(
                a,
                b,
                t,
                clampedDistance,
                LocalLength,
                ConnectedDistanceOffset,
                ReverseFlow);
        }

        public StylizedRiverSplineSample SampleAtOrientedDistance(
            float orientedDistance)
        {
            float clampedOriented =
                Mathf.Clamp(
                    orientedDistance,
                    0f,
                    LocalLength);

            float localDistance =
                ReverseFlow
                    ? LocalLength - clampedOriented
                    : clampedOriented;

            return SampleAtLocalDistance(localDistance);
        }

        public StylizedRiverSplineSample SampleAtGlobalDistance(
            float globalDistance)
        {
            return SampleAtOrientedDistance(
                globalDistance - ConnectedDistanceOffset);
        }

        public bool ContainsGlobalDistance(float globalDistance)
        {
            return
                globalDistance >= GlobalDistanceMinimum &&
                globalDistance <= GlobalDistanceMaximum;
        }

        public bool TryProjectWorldPoint(
            Vector3 worldPoint,
            out StylizedRiverProjection projection)
        {
            projection = default;

            if (!IsValid)
            {
                return false;
            }

            Vector2 point =
                new Vector2(
                    worldPoint.x,
                    worldPoint.z);

            float bestDistanceSqr = float.PositiveInfinity;
            int bestSegment = -1;
            float bestSegmentT = 0f;

            for (int index = 0;
                 index < samples.Length - 1;
                 index++)
            {
                StylizedRiverSplineSample a = samples[index];
                StylizedRiverSplineSample b = samples[index + 1];

                Vector2 a2 =
                    new Vector2(
                        a.Centre.x,
                        a.Centre.z);

                Vector2 b2 =
                    new Vector2(
                        b.Centre.x,
                        b.Centre.z);

                Vector2 segment = b2 - a2;
                float lengthSqr = segment.sqrMagnitude;

                float t =
                    lengthSqr > 0.000001f
                        ? Mathf.Clamp01(
                            Vector2.Dot(
                                point - a2,
                                segment) /
                            lengthSqr)
                        : 0f;

                Vector2 nearest = a2 + segment * t;
                float distanceSqr = (point - nearest).sqrMagnitude;

                if (distanceSqr >= bestDistanceSqr)
                {
                    continue;
                }

                bestDistanceSqr = distanceSqr;
                bestSegment = index;
                bestSegmentT = t;
            }

            if (bestSegment < 0)
            {
                return false;
            }

            StylizedRiverSplineSample segmentA = samples[bestSegment];
            StylizedRiverSplineSample segmentB = samples[bestSegment + 1];

            float localDistance =
                Mathf.Lerp(
                    segmentA.Distance,
                    segmentB.Distance,
                    bestSegmentT);

            StylizedRiverSplineSample sample =
                StylizedRiverGeometry.InterpolateSample(
                    segmentA,
                    segmentB,
                    bestSegmentT,
                    localDistance,
                    LocalLength,
                    ConnectedDistanceOffset,
                    ReverseFlow);

            Vector3 delta = worldPoint - sample.SurfacePoint;
            float acrossMetres = Vector3.Dot(delta, sample.Side);
            float acrossNormalized =
                sample.HalfWidth > 0.0001f
                    ? acrossMetres / sample.HalfWidth
                    : 0f;

            float distanceToNearestBank =
                sample.HalfWidth - Mathf.Abs(acrossMetres);

            projection =
                new StylizedRiverProjection(
                    sample.Centre,
                    sample.SurfacePoint,
                    sample.Tangent,
                    sample.Side,
                    sample.Up,
                    sample.Distance,
                    sample.OrientedDistance,
                    sample.GlobalDistance,
                    acrossMetres,
                    acrossNormalized,
                    distanceToNearestBank,
                    sample.HalfWidth,
                    Mathf.Abs(acrossMetres) <= sample.HalfWidth);

            return true;
        }

        public Vector3 RiverToWorld(
            float localDistance,
            float acrossMetres,
            float heightOffset = 0f)
        {
            StylizedRiverSplineSample sample =
                SampleAtLocalDistance(localDistance);

            return
                sample.SurfacePoint +
                sample.Side * acrossMetres +
                sample.Up * heightOffset;
        }

        public Vector3 OrientedRiverToWorld(
            float orientedDistance,
            float acrossMetres,
            float heightOffset = 0f)
        {
            StylizedRiverSplineSample sample =
                SampleAtOrientedDistance(orientedDistance);

            return
                sample.SurfacePoint +
                sample.Side * acrossMetres +
                sample.Up * heightOffset;
        }

        private int FindUpperSampleIndex(float localDistance)
        {
            int low = 0;
            int high = samples.Length - 1;

            while (low <= high)
            {
                int middle = low + (high - low) / 2;

                if (samples[middle].Distance < localDistance)
                {
                    low = middle + 1;
                }
                else
                {
                    high = middle - 1;
                }
            }

            return low;
        }

        private static void AppendError(
            StringBuilder builder,
            ref int errorCount,
            string message)
        {
            errorCount++;

            if (errorCount <= 12)
            {
                builder.Append("- ");
                builder.AppendLine(message);
            }
        }

        private static void CalculateSpacingRange(
            IReadOnlyList<StylizedRiverSplineSample> source,
            out float minimum,
            out float maximum)
        {
            minimum = 0f;
            maximum = 0f;

            if (source == null || source.Count < 2)
            {
                return;
            }

            minimum = float.PositiveInfinity;

            for (int index = 1; index < source.Count; index++)
            {
                float spacing =
                    source[index].Distance -
                    source[index - 1].Distance;

                minimum = Mathf.Min(minimum, spacing);
                maximum = Mathf.Max(maximum, spacing);
            }

            if (float.IsPositiveInfinity(minimum))
            {
                minimum = 0f;
            }
        }

        private static Bounds CalculateBounds(
            IReadOnlyList<StylizedRiverSplineSample> source)
        {
            if (source == null || source.Count == 0)
            {
                return default;
            }

            StylizedRiverSplineSample first = source[0];
            Bounds bounds =
                new Bounds(
                    first.SurfacePoint,
                    Vector3.zero);

            for (int index = 0; index < source.Count; index++)
            {
                StylizedRiverSplineSample sample = source[index];
                Vector3 sideOffset = sample.Side * sample.SurfaceHalfWidth;

                bounds.Encapsulate(sample.SurfacePoint + sideOffset);
                bounds.Encapsulate(sample.SurfacePoint - sideOffset);
            }

            return bounds;
        }
    }
}
