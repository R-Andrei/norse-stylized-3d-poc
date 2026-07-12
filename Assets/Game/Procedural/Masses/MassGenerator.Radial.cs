using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Radial polished mass

        private static TriangleSoup BuildRadialMass(MassRecipe recipe)
        {
            int frequency = GetSurfaceFrequency(recipe.SurfaceFacetDensity);
            Topology topology = BuildGeodesicTopology(frequency);

            Quaternion samplingRotation = CreateSamplingRotation(recipe.SurfaceSeed);
            Vector3[] directions = new Vector3[topology.Directions.Count];

            for (int i = 0; i < directions.Length; i++)
            {
                directions[i] = samplingRotation * topology.Directions[i];
            }

            float[] radii = GenerateRadialRadii(
                directions,
                topology.Neighbours,
                recipe);

            TriangleSoup soup = new TriangleSoup();

            for (int i = 0; i < topology.Triangles.Count; i += 3)
            {
                int a = topology.Triangles[i];
                int b = topology.Triangles[i + 1];
                int c = topology.Triangles[i + 2];

                Vector3 positionA = directions[a] * radii[a];
                Vector3 positionB = directions[b] * radii[b];
                Vector3 positionC = directions[c] * radii[c];

                AddOutwardTriangle(
                    soup,
                    positionA,
                    positionB,
                    positionC);
            }

            return soup;
        }

        private static float[] GenerateRadialRadii(
            Vector3[] directions,
            List<int>[] neighbours,
            MassRecipe recipe)
        {
            System.Random random =
                CreateRandom(recipe.ShapeSeed, 0x6E624EB7);

            float amplitude = recipe.ShapeDiversity switch
            {
                ShapeDiversity.Restrained => 0.055f,
                ShapeDiversity.Broad => 0.11f,
                ShapeDiversity.Wild => 0.17f,
                _ => 0.11f
            };

            int lobeCount = recipe.FormComplexity switch
            {
                FormComplexity.Primitive => 3,
                FormComplexity.Simple => 4,
                FormComplexity.Moderate => 6,
                FormComplexity.Complex => 8,
                FormComplexity.HighlyComplex => 10,
                _ => 6
            };

            DeformationLobe[] lobes = new DeformationLobe[lobeCount];

            for (int i = 0; i < lobeCount; i++)
            {
                lobes[i] = new DeformationLobe(
                    RandomUnitVector(random),
                    RandomRange(random, -amplitude, amplitude),
                    RandomRange(random, -0.35f, 0.20f),
                    RandomRange(random, 1.2f, 2.6f));
            }

            float[] radii = new float[directions.Length];
            float total = 0f;

            for (int i = 0; i < directions.Length; i++)
            {
                float radius = 1f;

                for (int lobeIndex = 0; lobeIndex < lobes.Length; lobeIndex++)
                {
                    DeformationLobe lobe = lobes[lobeIndex];
                    float alignment = Vector3.Dot(directions[i], lobe.Direction);

                    float influence = Mathf.InverseLerp(
                        lobe.FalloffStart,
                        1f,
                        alignment);

                    influence = Mathf.Pow(
                        Mathf.Clamp01(influence),
                        lobe.Power);

                    radius += lobe.Strength * influence;
                }

                radius = Mathf.Clamp(radius, 0.72f, 1.28f);
                radii[i] = radius;
                total += radius;
            }

            float average = total / radii.Length;

            for (int i = 0; i < radii.Length; i++)
            {
                radii[i] /= average;
            }

            GetRadialRegularization(
                recipe.EdgeCharacter,
                out int passes,
                out float strength,
                out float localDifference);

            RelaxRadii(radii, neighbours, passes, strength);
            LimitLocalPointiness(radii, neighbours, localDifference);

            return radii;
        }

        private static void RelaxRadii(
            float[] radii,
            List<int>[] neighbours,
            int passCount,
            float strength)
        {
            float[] working = new float[radii.Length];

            for (int pass = 0; pass < passCount; pass++)
            {
                for (int i = 0; i < radii.Length; i++)
                {
                    float neighbourAverage =
                        CalculateNeighbourAverage(radii, neighbours[i]);

                    working[i] = Mathf.Lerp(
                        radii[i],
                        neighbourAverage,
                        strength);
                }

                Array.Copy(working, radii, radii.Length);
            }
        }

        private static void LimitLocalPointiness(
            float[] radii,
            List<int>[] neighbours,
            float maximumDifference)
        {
            float[] working = new float[radii.Length];

            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < radii.Length; i++)
                {
                    float neighbourAverage =
                        CalculateNeighbourAverage(radii, neighbours[i]);

                    working[i] = Mathf.Clamp(
                        radii[i],
                        neighbourAverage - maximumDifference,
                        neighbourAverage + maximumDifference);
                }

                Array.Copy(working, radii, radii.Length);
            }
        }

        private static float CalculateNeighbourAverage(
            float[] values,
            List<int> neighbours)
        {
            if (neighbours.Count == 0)
            {
                return 1f;
            }

            float total = 0f;

            for (int i = 0; i < neighbours.Count; i++)
            {
                total += values[neighbours[i]];
            }

            return total / neighbours.Count;
        }

        #endregion
    }
}
