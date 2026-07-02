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
        private void InitializeHostedNegativeEvolution(
            bool rebuildField = true)
        {
            ReleaseHostedNegativeEvolutionResources();
            if (!majorEvolutionReady || majorMaskTextureArray == null ||
                evolvingHostedNegativeTexture == null)
            {
                ClearRenderTexture(evolvingHostedNegativeTexture);
                return;
            }

            int maskResolution = majorMaskTextureArray.width;
            IReadOnlyList<StylizedRiverFoamPreparedHostedNegativeRegion>
                prepared = pocketTopology != null
                    ? pocketTopology.PreparedHostedRegions
                    : Array.Empty<
                        StylizedRiverFoamPreparedHostedNegativeRegion>();
            int validCount = 0;
            for (int index = 0; index < prepared.Count; index++)
            {
                if (prepared[index].MaskResolution == maskResolution &&
                    prepared[index].HostPreparedIndex >= 0 &&
                    prepared[index].HostPreparedIndex <
                        majorEvolutionSlots.Length)
                {
                    validCount++;
                }
            }

            int sliceCount = Mathf.Max(1, validCount);
            hostedNegativeMaskUploadPixels = new Color[
                maskResolution * maskResolution];
            hostedNegativeMaskTextureArray = new Texture2DArray(
                maskResolution,
                maskResolution,
                sliceCount,
                TextureFormat.RGBAHalf,
                false,
                true)
            {
                name = "T_PS3D_RiverFoamHostedNegativeMasks",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            hostedNegativeEvolutionSlots =
                new HostedNegativeEvolutionSlot[validCount];
            hostedNegativeEvolutionGpuData =
                new FoamHostedNegativeEvolutionData[Mathf.Max(1, validCount)];
            int writeIndex = 0;
            for (int index = 0; index < prepared.Count; index++)
            {
                StylizedRiverFoamPreparedHostedNegativeRegion region =
                    prepared[index];
                if (region.MaskResolution != maskResolution ||
                    region.HostPreparedIndex < 0 ||
                    region.HostPreparedIndex >= majorEvolutionSlots.Length)
                {
                    continue;
                }

                float[] source = region.LocalPressureData;
                for (int pixel = 0;
                     pixel < hostedNegativeMaskUploadPixels.Length;
                     pixel++)
                {
                    float value = pixel < source.Length
                        ? Mathf.Clamp01(source[pixel])
                        : 0f;
                    hostedNegativeMaskUploadPixels[pixel] = new Color(
                        value,
                        0f,
                        0f,
                        0f);
                }

                hostedNegativeMaskTextureArray.SetPixels(
                    hostedNegativeMaskUploadPixels,
                    writeIndex,
                    0);
                HostedNegativeEvolutionPose initialPose =
                    CreateIdentityHostedNegativePose();
                hostedNegativeEvolutionSlots[writeIndex] =
                    new HostedNegativeEvolutionSlot
                    {
                        StableId = region.StableId,
                        PreparedIndex = index,
                        HostSlotIndex = region.HostPreparedIndex,
                        RegionClass = region.RegionClass,
                        CurrentVariantIndex = 0,
                        TargetVariantIndex = 0,
                        Current = initialPose,
                        Start = initialPose,
                        Target = initialPose
                    };
                writeIndex++;
            }

            if (validCount == 0)
            {
                hostedNegativeMaskTextureArray.SetPixels(
                    hostedNegativeMaskUploadPixels,
                    0,
                    0);
            }
            hostedNegativeMaskTextureArray.Apply(false, true);
            hostedNegativeEvolutionBuffer = new ComputeBuffer(
                Mathf.Max(1, validCount),
                sizeof(float) * 12,
                ComputeBufferType.Structured);
            if (validCount == 0)
            {
                hostedNegativeEvolutionBuffer.SetData(
                    hostedNegativeEvolutionGpuData);
            }
            hostedNegativeEvolutionReady = validCount > 0;
            hostedNegativeLocalChangeCount = 0;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            hostedNegativeInitialParityPending =
                hostedNegativeEvolutionReady &&
                IsTopologyDebugActive &&
                SystemInfo.supportsAsyncGPUReadback;
            hostedNegativeInitialParityReadbackPending = false;
            hostedNegativeInitialParityAvailable = false;
            hostedNegativeInitialParityMeanDifference = 0f;
            hostedNegativeInitialParityMaximumDifference = 0f;
#endif
            if (rebuildField)
            {
                BuildEvolvingMajorField();
            }
        }

        private void ReleaseHostedNegativeEvolutionResources()
        {
            hostedNegativeEvolutionBuffer?.Release();
            hostedNegativeEvolutionBuffer = null;
            if (hostedNegativeMaskTextureArray != null)
            {
                DestroyUnityObject(hostedNegativeMaskTextureArray);
                hostedNegativeMaskTextureArray = null;
            }

            hostedNegativeEvolutionSlots =
                Array.Empty<HostedNegativeEvolutionSlot>();
            hostedNegativeEvolutionGpuData =
                Array.Empty<FoamHostedNegativeEvolutionData>();
            hostedNegativeMaskUploadPixels = Array.Empty<Color>();
            hostedNegativeEvolutionReady = false;
            hostedNegativeLocalChangeCount = 0;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            hostedNegativeInitialParityGeneration++;
            hostedNegativeInitialParityPending = false;
            hostedNegativeInitialParityReadbackPending = false;
            hostedNegativeInitialParityAvailable = false;
            hostedNegativeInitialParityMeanDifference = 0f;
            hostedNegativeInitialParityMaximumDifference = 0f;
#endif
        }

        private void BeginHostedNegativeMove(
            int hostSlotIndex,
            uint hostCycleSeed)
        {
            if (!hostedNegativeEvolutionReady)
            {
                return;
            }

            IReadOnlyList<StylizedRiverFoamPreparedHostedNegativeRegion>
                prepared = pocketTopology.PreparedHostedRegions;
            for (int index = 0;
                 index < hostedNegativeEvolutionSlots.Length;
                 index++)
            {
                ref HostedNegativeEvolutionSlot slot =
                    ref hostedNegativeEvolutionSlots[index];
                if (slot.HostSlotIndex != hostSlotIndex ||
                    slot.PreparedIndex < 0 ||
                    slot.PreparedIndex >= prepared.Count)
                {
                    continue;
                }

                uint seed = EvolutionMixBits(
                    slot.StableId ^ hostCycleSeed ^
                    ((uint)(majorEvolutionSlots[hostSlotIndex].HopIndex + 1) *
                        0x27D4EB2Fu));
                float changeProbability = slot.RegionClass ==
                    StylizedRiverFoamNegativeRegionClass.InteriorPocket
                    ? HostedInteriorChangeProbability
                    : HostedCavityChangeProbability;
                slot.Start = slot.Current;
                if (HashMajorEvolution(seed, 1u) > changeProbability)
                {
                    slot.Target = slot.Current;
                    slot.TargetVariantIndex = slot.CurrentVariantIndex;
                    continue;
                }

                slot.Target = ResolveHostedNegativeTarget(
                    prepared[slot.PreparedIndex],
                    seed,
                    slot.CurrentVariantIndex,
                    out int targetVariantIndex);
                slot.TargetVariantIndex = targetVariantIndex;
                if (targetVariantIndex != slot.CurrentVariantIndex)
                {
                    hostedNegativeLocalChangeCount++;
                }
            }
        }

        private void CompleteHostedNegativeMove(int hostSlotIndex)
        {
            for (int index = 0;
                 index < hostedNegativeEvolutionSlots.Length;
                 index++)
            {
                ref HostedNegativeEvolutionSlot slot =
                    ref hostedNegativeEvolutionSlots[index];
                if (slot.HostSlotIndex != hostSlotIndex)
                {
                    continue;
                }

                slot.Current = slot.Target;
                slot.Start = slot.Target;
                slot.CurrentVariantIndex = slot.TargetVariantIndex;
            }
        }

        private void RecycleHostedNegativeSlots(
            int hostSlotIndex,
            uint recycleSeed)
        {
            if (!hostedNegativeEvolutionReady)
            {
                return;
            }

            IReadOnlyList<StylizedRiverFoamPreparedHostedNegativeRegion>
                prepared = pocketTopology.PreparedHostedRegions;
            for (int index = 0;
                 index < hostedNegativeEvolutionSlots.Length;
                 index++)
            {
                ref HostedNegativeEvolutionSlot slot =
                    ref hostedNegativeEvolutionSlots[index];
                if (slot.HostSlotIndex != hostSlotIndex ||
                    slot.PreparedIndex < 0 ||
                    slot.PreparedIndex >= prepared.Count)
                {
                    continue;
                }

                uint seed = EvolutionMixBits(
                    slot.StableId ^ recycleSeed ^ 0xA24BAED5u);
                HostedNegativeEvolutionPose pose =
                    ResolveHostedNegativeTarget(
                        prepared[slot.PreparedIndex],
                        seed,
                        slot.CurrentVariantIndex,
                        out int targetVariantIndex);
                if (targetVariantIndex != slot.CurrentVariantIndex)
                {
                    hostedNegativeLocalChangeCount++;
                }
                slot.CurrentVariantIndex = targetVariantIndex;
                slot.TargetVariantIndex = targetVariantIndex;
                slot.Current = pose;
                slot.Start = pose;
                slot.Target = pose;
            }
        }

        private HostedNegativeEvolutionPose ResolveHostedNegativePose(
            HostedNegativeEvolutionSlot slot)
        {
            if (slot.HostSlotIndex < 0 ||
                slot.HostSlotIndex >= majorEvolutionSlots.Length)
            {
                return slot.Current;
            }

            MajorEvolutionSlot host =
                majorEvolutionSlots[slot.HostSlotIndex];
            if (!host.IsMoving || host.MoveDuration <= 0.0001f)
            {
                return slot.Current;
            }

            float t = Mathf.Clamp01(host.MoveElapsed / host.MoveDuration);
            t = t * t * (3f - 2f * t);
            return new HostedNegativeEvolutionPose
            {
                OffsetCells = Vector2.Lerp(
                    slot.Start.OffsetCells,
                    slot.Target.OffsetCells,
                    t),
                RotationRadians = Mathf.Lerp(
                    slot.Start.RotationRadians,
                    slot.Target.RotationRadians,
                    t),
                ScaleAlong = Mathf.Lerp(
                    slot.Start.ScaleAlong,
                    slot.Target.ScaleAlong,
                    t),
                ScaleAcross = Mathf.Lerp(
                    slot.Start.ScaleAcross,
                    slot.Target.ScaleAcross,
                    t),
                StrengthScale = Mathf.Lerp(
                    slot.Start.StrengthScale,
                    slot.Target.StrengthScale,
                    t)
            };
        }

        private void RequestHostedNegativeInitialParityIfNeeded()
        {
            // This comparison is intentionally restricted to Editor and
            // development diagnostics. Normal runs do not read the evolving
            // field back or pay any parity-validation cost.
            if (!hostedNegativeInitialParityPending ||
                hostedNegativeInitialParityReadbackPending ||
                !IsTopologyDebugActive ||
                !SystemInfo.supportsAsyncGPUReadback ||
                evolvingHostedNegativeTexture == null ||
                pocketTopology == null)
            {
                return;
            }

            hostedNegativeInitialParityPending = false;
            hostedNegativeInitialParityReadbackPending = true;
            int generation = hostedNegativeInitialParityGeneration;
            StylizedRiverFoamPocketTopology requestedTopology = pocketTopology;
            AsyncGPUReadback.Request(
                evolvingHostedNegativeTexture,
                0,
                TextureFormat.RFloat,
                request =>
                {
                    if (this == null ||
                        generation != hostedNegativeInitialParityGeneration ||
                        requestedTopology != pocketTopology)
                    {
                        return;
                    }

                    hostedNegativeInitialParityReadbackPending = false;
                    if (request.hasError)
                    {
                        hostedNegativeInitialParityAvailable = false;
                        return;
                    }

                    var data = request.GetData<float>();
                    float[] expected = requestedTopology.HostedPressureData;
                    float[] fallback =
                        requestedTopology.HostedFallbackPressureData;
                    int count = Mathf.Min(
                        data.Length,
                        Mathf.Min(expected.Length, fallback.Length));
                    if (count <= 0)
                    {
                        hostedNegativeInitialParityAvailable = false;
                        return;
                    }

                    double totalDifference = 0.0;
                    int relevantCount = 0;
                    float maximumDifference = 0f;
                    for (int index = 0; index < count; index++)
                    {
                        float reconstructed = Mathf.Max(
                            Mathf.Clamp01(data[index]),
                            Mathf.Clamp01(fallback[index]));
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

                    hostedNegativeInitialParityMeanDifference =
                        relevantCount > 0
                            ? (float)(totalDifference / relevantCount)
                            : 0f;
                    hostedNegativeInitialParityMaximumDifference =
                        maximumDifference;
                    hostedNegativeInitialParityAvailable = true;
                });
        }

        private int CountHostedNegativeSlots(
            StylizedRiverFoamNegativeRegionClass regionClass)
        {
            int count = 0;
            for (int index = 0;
                 index < hostedNegativeEvolutionSlots.Length;
                 index++)
            {
                if (hostedNegativeEvolutionSlots[index].RegionClass ==
                    regionClass)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
