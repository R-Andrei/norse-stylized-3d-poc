using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    internal static class StylizedRiverFoamTopologyFixture
    {
        public const int MaxDescriptorCount = 32;
        public const int DescriptorStrideBytes = 128;

        public enum DescriptorType
        {
            AdditivePath = 0,
            AdditiveRegion = 1,
            SubtractiveRegion = 2
        }

        public enum OutputChannel
        {
            Major = 0,
            Connector = 1,
            Pocket = 2,
            Boundary = 3
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Descriptor
        {
            public Vector4 Meta0;
            public Vector4 Meta1;
            public Vector4 P0;
            public Vector4 P1;
            public Vector4 P2;
            public Vector4 P3;
            public Vector4 P4;
            public Vector4 P5;
        }

        public static int Build(Descriptor[] descriptors)
        {
            if (descriptors == null ||
                descriptors.Length < MaxDescriptorCount)
            {
                throw new ArgumentException(
                    $"Golden Foam topology fixture requires at least {MaxDescriptorCount} descriptor slots.",
                    nameof(descriptors));
            }

            Array.Clear(descriptors, 0, descriptors.Length);
            int count = 0;

            // Additive major film masses and long contour ribbons.
            AddPath(descriptors, ref count, OutputChannel.Major,
                1f, 0.055f, 0.70f,
                P(0.02f, -0.62f, 0.18f),
                P(0.08f, -0.48f, 0.30f),
                P(0.16f, -0.34f, 0.28f),
                P(0.24f, -0.18f, 0.16f));
            AddPath(descriptors, ref count, OutputChannel.Major,
                0.88f, 0.036f, 0.95f,
                P(0.16f, 0.38f, 0.070f),
                P(0.26f, 0.20f, 0.085f),
                P(0.36f, 0.02f, 0.075f),
                P(0.43f, -0.10f, 0.050f));
            AddPath(descriptors, ref count, OutputChannel.Major,
                0.96f, 0.050f, 0.78f,
                P(0.34f, 0.18f, 0.18f),
                P(0.43f, 0.08f, 0.27f),
                P(0.53f, -0.05f, 0.24f),
                P(0.61f, -0.22f, 0.12f));
            AddPath(descriptors, ref count, OutputChannel.Major,
                0.82f, 0.032f, 1.0f,
                P(0.39f, 0.34f, 0.080f),
                P(0.48f, 0.43f, 0.095f),
                P(0.58f, 0.25f, 0.070f));
            AddPath(descriptors, ref count, OutputChannel.Major,
                0.80f, 0.032f, 1.0f,
                P(0.39f, -0.18f, 0.095f),
                P(0.50f, -0.36f, 0.090f),
                P(0.62f, -0.24f, 0.060f));
            AddPath(descriptors, ref count, OutputChannel.Major,
                0.86f, 0.036f, 0.92f,
                P(0.54f, 0.34f, 0.070f),
                P(0.66f, 0.10f, 0.105f),
                P(0.76f, -0.22f, 0.080f));
            AddPath(descriptors, ref count, OutputChannel.Major,
                1f, 0.052f, 0.72f,
                P(0.70f, -0.45f, 0.16f),
                P(0.80f, -0.35f, 0.29f),
                P(0.90f, -0.16f, 0.24f),
                P(0.97f, 0.02f, 0.050f));
            AddPath(descriptors, ref count, OutputChannel.Major,
                0.72f, 0.030f, 1.0f,
                P(0.78f, 0.18f, 0.065f),
                P(0.88f, 0.34f, 0.055f),
                P(0.96f, 0.42f, 0.035f));

            // Relational connectors, forks, and thin necks.
            AddPath(descriptors, ref count, OutputChannel.Connector,
                0.92f, 0.028f, 1.05f,
                P(0.22f, -0.16f, 0.040f),
                P(0.30f, 0.02f, 0.035f),
                P(0.36f, 0.07f, 0.028f));
            AddPath(descriptors, ref count, OutputChannel.Connector,
                0.84f, 0.026f, 1.1f,
                P(0.42f, -0.08f, 0.034f),
                P(0.48f, 0.14f, 0.030f),
                P(0.55f, 0.24f, 0.026f));
            AddPath(descriptors, ref count, OutputChannel.Connector,
                0.78f, 0.025f, 1.1f,
                P(0.58f, 0.21f, 0.032f),
                P(0.66f, 0.06f, 0.028f),
                P(0.72f, -0.12f, 0.024f));
            AddPath(descriptors, ref count, OutputChannel.Connector,
                0.70f, 0.024f, 1.15f,
                P(0.67f, 0.06f, 0.030f),
                P(0.73f, 0.28f, 0.024f),
                P(0.81f, 0.40f, 0.018f));
            AddPath(descriptors, ref count, OutputChannel.Connector,
                0.64f, 0.024f, 1.10f,
                P(0.44f, -0.16f, 0.034f),
                P(0.50f, -0.42f, 0.026f),
                P(0.57f, -0.56f, 0.018f));

            // Subtractive enclosed pockets and torn negative spaces.
            AddEllipse(descriptors, ref count, OutputChannel.Pocket,
                1f, 0.040f, 0.17f, 0.075f, 0.13f, -0.45f);
            AddEllipse(descriptors, ref count, OutputChannel.Pocket,
                0.92f, 0.036f, 0.20f, 0.060f, 0.21f, -0.25f);
            AddEllipse(descriptors, ref count, OutputChannel.Pocket,
                0.95f, 0.038f, 0.13f, 0.070f, 0.47f, 0.03f);
            AddEllipse(descriptors, ref count, OutputChannel.Pocket,
                0.95f, 0.040f, 0.15f, 0.080f, 0.84f, -0.31f);
            AddPath(descriptors, ref count, OutputChannel.Pocket,
                0.58f, 0.018f, 0.6f,
                P(0.78f, -0.17f, 0.030f),
                P(0.88f, -0.08f, 0.024f));

            // Contextual boundary accents that help the locked fixture read as
            // bank-attached film without turning the shore into an emitter.
            AddPath(descriptors, ref count, OutputChannel.Boundary,
                0.42f, 0.030f, 1.0f,
                P(0.04f, 0.74f, 0.035f),
                P(0.18f, 0.64f, 0.030f),
                P(0.30f, 0.52f, 0.022f));
            AddPath(descriptors, ref count, OutputChannel.Boundary,
                0.38f, 0.028f, 1.0f,
                P(0.68f, -0.72f, 0.034f),
                P(0.82f, -0.62f, 0.030f),
                P(0.94f, -0.50f, 0.020f));

            return count;
        }

        private static Vector4 P(float u, float lateral, float width)
        {
            return new Vector4(u, lateral, width, width * 1.35f);
        }

        private static void AddPath(
            Descriptor[] descriptors,
            ref int count,
            OutputChannel channel,
            float strength,
            float feather,
            float taperPower,
            params Vector4[] points)
        {
            AddDescriptor(
                descriptors,
                ref count,
                channel == OutputChannel.Major
                    ? DescriptorType.AdditiveRegion
                    : DescriptorType.AdditivePath,
                channel,
                strength,
                feather,
                taperPower,
                points);
        }

        private static void AddEllipse(
            Descriptor[] descriptors,
            ref int count,
            OutputChannel channel,
            float strength,
            float feather,
            float radiusU,
            float radiusLateral,
            float u,
            float lateral)
        {
            AddDescriptor(
                descriptors,
                ref count,
                DescriptorType.SubtractiveRegion,
                channel,
                strength,
                feather,
                1f,
                new Vector4(u, lateral, radiusLateral, radiusU));
        }

        private static void AddDescriptor(
            Descriptor[] descriptors,
            ref int count,
            DescriptorType type,
            OutputChannel channel,
            float strength,
            float feather,
            float taperPower,
            params Vector4[] points)
        {
            if (count >= MaxDescriptorCount)
            {
                return;
            }

            int pointCount = Mathf.Clamp(points?.Length ?? 0, 0, 6);
            Descriptor descriptor = default;
            descriptor.Meta0 = new Vector4(
                (float)type,
                (float)channel,
                pointCount,
                Mathf.Clamp01(strength));
            descriptor.Meta1 = new Vector4(
                Mathf.Max(0.001f, feather),
                Mathf.Max(0.05f, taperPower),
                0f,
                0f);

            if (pointCount > 0) descriptor.P0 = points[0];
            if (pointCount > 1) descriptor.P1 = points[1];
            if (pointCount > 2) descriptor.P2 = points[2];
            if (pointCount > 3) descriptor.P3 = points[3];
            if (pointCount > 4) descriptor.P4 = points[4];
            if (pointCount > 5) descriptor.P5 = points[5];

            descriptors[count++] = descriptor;
        }
    }
}
