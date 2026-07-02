using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ProgrammaticStylized3D.Geometry;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private void RequestWeakSpanIdentityParityIfNeeded()
        {
            // Weak-Span identity parity remains debug-only. Normal gameplay
            // never reads the reconstructed pressure field back.
            if (!weakSpanIdentityParityPending ||
                weakSpanIdentityParityReadbackPending ||
                !IsTopologyDebugActive ||
                !SystemInfo.supportsAsyncGPUReadback ||
                evolvingWeakSpanNegativeTexture == null ||
                pocketTopology == null)
            {
                return;
            }

            weakSpanIdentityParityPending = false;
            weakSpanIdentityParityReadbackPending = true;
            int generation = weakSpanIdentityParityGeneration;
            StylizedRiverFoamPocketTopology requestedTopology = pocketTopology;
            AsyncGPUReadback.Request(
                evolvingWeakSpanNegativeTexture,
                0,
                TextureFormat.RFloat,
                request =>
                {
                    if (this == null ||
                        generation != weakSpanIdentityParityGeneration ||
                        requestedTopology != pocketTopology)
                    {
                        return;
                    }

                    weakSpanIdentityParityReadbackPending = false;
                    if (request.hasError)
                    {
                        weakSpanIdentityParityAvailable = false;
                        return;
                    }

                    var data = request.GetData<float>();
                    float[] expected =
                        requestedTopology.StaticIndependentPressureData;
                    int count = Mathf.Min(data.Length, expected.Length);
                    if (count <= 0)
                    {
                        weakSpanIdentityParityAvailable = false;
                        return;
                    }

                    double totalDifference = 0.0;
                    int relevantCount = 0;
                    float maximumDifference = 0f;
                    for (int index = 0; index < count; index++)
                    {
                        float reconstructed = Mathf.Clamp01(data[index]);
                        float expectedValue = Mathf.Clamp01(expected[index]);
                        float difference = Mathf.Abs(
                            reconstructed - expectedValue);
                        if (Mathf.Max(reconstructed, expectedValue) > 0.01f)
                        {
                            totalDifference += difference;
                            relevantCount++;
                        }
                        maximumDifference = Mathf.Max(
                            maximumDifference,
                            difference);
                    }

                    weakSpanIdentityParityMeanDifference =
                        relevantCount > 0
                            ? (float)(totalDifference / relevantCount)
                            : 0f;
                    weakSpanIdentityParityMaximumDifference =
                        maximumDifference;
                    weakSpanIdentityParityAvailable = true;
                });
        }

        private static float HashMajorEvolution(uint seed, uint stream)
        {
            return (EvolutionMixBits(
                seed ^ EvolutionMixBits(stream + 0x27D4EB2Fu)) &
                0x00FFFFFFu) / 16777216f;
        }

        private static uint EvolutionMixBits(uint value)
        {
            value ^= value >> 16;
            value *= 0x7FEB352Du;
            value ^= value >> 15;
            value *= 0x846CA68Bu;
            value ^= value >> 16;
            return value;
        }

        private static float HashConnectorIdentity(
            uint stableId,
            uint stream)
        {
            return (EvolutionMixBits(
                stableId ^ EvolutionMixBits(stream + 0xC2B2AE35u)) &
                0x00FFFFFFu) / 16777216f;
        }

        private static float WrapMajorAngle(float angle)
        {
            while (angle > Mathf.PI * 0.5f)
            {
                angle -= Mathf.PI;
            }
            while (angle < -Mathf.PI * 0.5f)
            {
                angle += Mathf.PI;
            }
            return angle;
        }

        private static float LerpMajorAngle(float from, float to, float t)
        {
            float delta = WrapMajorAngle(to - from);
            return WrapMajorAngle(from + delta * t);
        }

        private static float LerpFullAngle(float from, float to, float t)
        {
            float delta = Mathf.Repeat(
                to - from + Mathf.PI,
                Mathf.PI * 2f) - Mathf.PI;
            return from + delta * t;
        }
    }
}
