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
        private void InitializeFreeWaterEvolution(bool rebuildField = true)
        {
            ReleaseFreeWaterEvolutionResources();
            if (!majorEvolutionReady || evolvingFreeWaterNegativeTexture == null)
            {
                ClearRenderTexture(evolvingFreeWaterNegativeTexture);
                ClearRenderTexture(evolvingConnectorTexture);
                ClearRenderTexture(evolvingWeakSpanNegativeTexture);
                return;
            }

            IReadOnlyList<StylizedRiverFoamPreparedFreeWaterRegion> prepared =
                pocketTopology != null
                    ? pocketTopology.PreparedFreeWaterRegions
                    : Array.Empty<StylizedRiverFoamPreparedFreeWaterRegion>();
            int acceptedCount = pocketTopology != null
                ? pocketTopology.AcceptedFreeWaterEventCount
                : 0;
            bool allAcceptedPrepared = acceptedCount > 0 &&
                prepared.Count == acceptedCount;
            int maskResolution = allAcceptedPrepared && prepared.Count > 0
                ? prepared[0].MaskResolution
                : 1;
            bool consistentMasks = allAcceptedPrepared;
            for (int index = 1; index < prepared.Count; index++)
            {
                if (prepared[index].MaskResolution != maskResolution)
                {
                    consistentMasks = false;
                    break;
                }
            }

            int validCount = consistentMasks ? prepared.Count : 0;
            int sliceCount = Mathf.Max(1, validCount);
            freeWaterNegativeMaskUploadPixels = new Color[
                maskResolution * maskResolution];
            freeWaterNegativeMaskTextureArray = new Texture2DArray(
                maskResolution,
                maskResolution,
                sliceCount,
                TextureFormat.RGBAHalf,
                false,
                true)
            {
                name = "T_PS3D_RiverFoamFreeWaterNegativeMasks",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            freeWaterEvolutionSlots =
                new FreeWaterEvolutionSlot[validCount];
            freeWaterEvolutionGpuData =
                new FoamFreeWaterEvolutionData[Mathf.Max(1, validCount)];
            for (int index = 0; index < validCount; index++)
            {
                StylizedRiverFoamPreparedFreeWaterRegion region =
                    prepared[index];
                float[] source = region.LocalPressureData;
                for (int pixel = 0;
                     pixel < freeWaterNegativeMaskUploadPixels.Length;
                     pixel++)
                {
                    float value = pixel < source.Length
                        ? Mathf.Clamp01(source[pixel])
                        : 0f;
                    freeWaterNegativeMaskUploadPixels[pixel] = new Color(
                        value,
                        0f,
                        0f,
                        0f);
                }

                freeWaterNegativeMaskTextureArray.SetPixels(
                    freeWaterNegativeMaskUploadPixels,
                    index,
                    0);
                FreeWaterEvolutionPose initialPose =
                    CreateInitialFreeWaterPose(region);
                FreeWaterEvolutionSlot slot =
                    new FreeWaterEvolutionSlot
                    {
                        StableId = region.StableId,
                        PreparedIndex = index,
                        Current = initialPose,
                        Start = initialPose,
                        Target = initialPose,
                        MoveElapsed = 0f,
                        MoveDuration = 0f,
                        OccurrenceElapsed = 0f,
                        HopIndex = 0,
                        RecycleCount = 0,
                        LastAnchorIndex = -1,
                        IsMoving = false
                    };
                ResolveFreeWaterOccurrenceBudget(ref slot);
                ResolveFreeWaterDwell(ref slot, 0u);
                if (IsInFreeWaterEgress(initialPose.LocalDistance))
                {
                    slot.DwellRemaining = Mathf.Min(
                        slot.DwellRemaining,
                        Mathf.Lerp(
                            0.5f,
                            1.5f,
                            HashMajorEvolution(
                                slot.StableId,
                                1u)));
                }

                freeWaterEvolutionSlots[index] = slot;
            }

            if (validCount == 0)
            {
                freeWaterNegativeMaskTextureArray.SetPixels(
                    freeWaterNegativeMaskUploadPixels,
                    0,
                    0);
            }
            freeWaterNegativeMaskTextureArray.Apply(false, true);
            freeWaterEvolutionBuffer = new ComputeBuffer(
                Mathf.Max(1, validCount),
                sizeof(float) * 12,
                ComputeBufferType.Structured);
            if (validCount == 0)
            {
                freeWaterEvolutionBuffer.SetData(freeWaterEvolutionGpuData);
            }
            freeWaterEvolutionReady = validCount > 0;
            freeWaterEvolutionAccumulator = 0f;
            freeWaterMoveCount = 0;
            freeWaterRecycleCount = 0;
            freeWaterUpstreamViolationCount = 0;
            freeWaterObservedMinimumDwell = float.PositiveInfinity;
            freeWaterObservedMaximumDwell = 0f;
            freeWaterObservedMinimumMove = float.PositiveInfinity;
            freeWaterObservedMaximumMove = 0f;
            for (int index = 0; index < freeWaterEvolutionSlots.Length; index++)
            {
                float dwell = freeWaterEvolutionSlots[index]
                    .LastDwellDuration;
                freeWaterObservedMinimumDwell = Mathf.Min(
                    freeWaterObservedMinimumDwell,
                    dwell);
                freeWaterObservedMaximumDwell = Mathf.Max(
                    freeWaterObservedMaximumDwell,
                    dwell);
            }
            if (rebuildField)
            {
                BuildEvolvingMajorField();
            }
        }

        private void ReleaseFreeWaterEvolutionResources()
        {
            freeWaterEvolutionBuffer?.Release();
            freeWaterEvolutionBuffer = null;
            if (freeWaterNegativeMaskTextureArray != null)
            {
                DestroyUnityObject(freeWaterNegativeMaskTextureArray);
                freeWaterNegativeMaskTextureArray = null;
            }

            freeWaterEvolutionSlots = Array.Empty<FreeWaterEvolutionSlot>();
            freeWaterEvolutionGpuData =
                Array.Empty<FoamFreeWaterEvolutionData>();
            freeWaterNegativeMaskUploadPixels = Array.Empty<Color>();
            freeWaterEvolutionReady = false;
            freeWaterEvolutionAccumulator = 0f;
            freeWaterMoveCount = 0;
            freeWaterRecycleCount = 0;
            freeWaterUpstreamViolationCount = 0;
            freeWaterObservedMinimumDwell = float.PositiveInfinity;
            freeWaterObservedMaximumDwell = 0f;
            freeWaterObservedMinimumMove = float.PositiveInfinity;
            freeWaterObservedMaximumMove = 0f;
        }

        private bool AdvanceFreeWaterEvolution(float deltaTime)
        {
            if (!freeWaterEvolutionReady || deltaTime <= 0f ||
                freeWaterEvolutionSlots.Length == 0)
            {
                return false;
            }

            bool immediateRebuild = false;
            bool anyMoving = false;
            for (int index = 0;
                 index < freeWaterEvolutionSlots.Length;
                 index++)
            {
                ref FreeWaterEvolutionSlot slot =
                    ref freeWaterEvolutionSlots[index];
                slot.OccurrenceElapsed += deltaTime;

                if (slot.IsMoving)
                {
                    slot.MoveElapsed += deltaTime;
                    if (slot.MoveElapsed >= slot.MoveDuration)
                    {
                        if (slot.Target.LocalDistance + 0.0001f <
                            slot.Start.LocalDistance)
                        {
                            freeWaterUpstreamViolationCount++;
                        }

                        slot.Current = slot.Target;
                        slot.Start = slot.Target;
                        slot.IsMoving = false;
                        slot.MoveElapsed = slot.MoveDuration;
                        slot.HopIndex++;
                        freeWaterMoveCount++;
                        immediateRebuild = true;

                        if (ShouldRecycleFreeWater(slot) ||
                            IsInFreeWaterEgress(
                                slot.Current.LocalDistance))
                        {
                            RecycleFreeWater(ref slot);
                        }
                        else
                        {
                            ResolveFreeWaterDwell(ref slot, 20u);
                        }
                    }
                    else
                    {
                        anyMoving = true;
                    }

                    continue;
                }

                if (ShouldRecycleFreeWater(slot))
                {
                    RecycleFreeWater(ref slot);
                    immediateRebuild = true;
                    continue;
                }

                slot.DwellRemaining -= deltaTime;
                if (slot.DwellRemaining > 0f)
                {
                    continue;
                }

                if (BeginFreeWaterMove(ref slot))
                {
                    anyMoving = true;
                }
                else
                {
                    immediateRebuild = true;
                }
            }

            if (anyMoving)
            {
                freeWaterEvolutionAccumulator += deltaTime;
            }
            else
            {
                freeWaterEvolutionAccumulator = 0f;
            }

            float tickInterval = 1f / FreeWaterEvolutionTickRate;
            bool scheduledRebuild = anyMoving &&
                freeWaterEvolutionAccumulator >= tickInterval;
            if (scheduledRebuild)
            {
                freeWaterEvolutionAccumulator %= tickInterval;
            }

            return immediateRebuild || scheduledRebuild;
        }

        private bool BeginFreeWaterMove(ref FreeWaterEvolutionSlot slot)
        {
            uint cycleSeed = ResolveFreeWaterCycleSeed(slot, 30u);
            float flowScale = Mathf.Lerp(
                0.72f,
                1.22f,
                Mathf.Clamp01(
                    river.FlowSpeedMetresPerSecond / 4.5f));
            float downstreamStep = Mathf.Lerp(
                FreeWaterMinimumHopMetres,
                FreeWaterMaximumHopMetres,
                HashMajorEvolution(cycleSeed, 1u)) * flowScale;
            float targetDistance = slot.Current.LocalDistance +
                downstreamStep;
            if (targetDistance >= ResolveFreeWaterEgressStart())
            {
                RecycleFreeWater(ref slot);
                return false;
            }

            float lateralMagnitude = Mathf.Lerp(
                FreeWaterMinimumLateralHop,
                FreeWaterMaximumLateralHop,
                HashMajorEvolution(cycleSeed, 2u));
            float lateralSign = HashMajorEvolution(cycleSeed, 3u) < 0.5f
                ? -1f
                : 1f;
            float targetAcross = Mathf.Clamp(
                slot.Current.AcrossNormalized +
                    lateralMagnitude * lateralSign,
                -0.84f,
                0.84f);
            if (Mathf.Abs(
                    targetAcross - slot.Current.AcrossNormalized) < 0.025f)
            {
                targetAcross = Mathf.Clamp(
                    slot.Current.AcrossNormalized -
                        lateralMagnitude * lateralSign,
                    -0.84f,
                    0.84f);
            }

            float scaleAlong = Mathf.Lerp(
                0.82f,
                1.22f,
                HashMajorEvolution(cycleSeed, 4u));
            float scaleAcross = Mathf.Lerp(
                0.82f,
                1.22f,
                HashMajorEvolution(cycleSeed, 5u));
            float areaScale = Mathf.Max(
                0.25f,
                scaleAlong * scaleAcross);
            slot.Start = slot.Current;
            slot.Target = new FreeWaterEvolutionPose
            {
                LocalDistance = targetDistance,
                AcrossNormalized = targetAcross,
                OrientationRadians = WrapMajorAngle(
                    slot.Current.OrientationRadians +
                    Mathf.Lerp(
                        -0.22f,
                        0.22f,
                        HashMajorEvolution(cycleSeed, 6u))),
                ScaleAlong = scaleAlong,
                ScaleAcross = scaleAcross,
                StrengthScale = Mathf.Clamp(
                    1f / Mathf.Sqrt(areaScale),
                    0.88f,
                    1.12f)
            };

            float dwellProgress = Mathf.InverseLerp(
                FreeWaterMinimumDwellSeconds,
                FreeWaterMaximumDwellSeconds,
                slot.LastDwellDuration);
            slot.MoveDuration = Mathf.Clamp(
                Mathf.Lerp(
                    FreeWaterMinimumMoveSeconds,
                    FreeWaterMaximumMoveSeconds,
                    dwellProgress * 0.72f +
                    HashMajorEvolution(cycleSeed, 7u) * 0.28f),
                FreeWaterMinimumMoveSeconds,
                FreeWaterMaximumMoveSeconds);
            slot.MoveElapsed = 0f;
            slot.IsMoving = true;
            freeWaterObservedMinimumMove = Mathf.Min(
                freeWaterObservedMinimumMove,
                slot.MoveDuration);
            freeWaterObservedMaximumMove = Mathf.Max(
                freeWaterObservedMaximumMove,
                slot.MoveDuration);
            return true;
        }

        private FreeWaterEvolutionPose ResolveFreeWaterPose(
            FreeWaterEvolutionSlot slot)
        {
            if (!slot.IsMoving || slot.MoveDuration <= 0.0001f)
            {
                return slot.Current;
            }

            float t = Mathf.Clamp01(slot.MoveElapsed / slot.MoveDuration);
            t = t * t * (3f - 2f * t);
            return new FreeWaterEvolutionPose
            {
                LocalDistance = Mathf.Lerp(
                    slot.Start.LocalDistance,
                    slot.Target.LocalDistance,
                    t),
                AcrossNormalized = Mathf.Lerp(
                    slot.Start.AcrossNormalized,
                    slot.Target.AcrossNormalized,
                    t),
                OrientationRadians = LerpMajorAngle(
                    slot.Start.OrientationRadians,
                    slot.Target.OrientationRadians,
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

        private static FreeWaterEvolutionPose CreateInitialFreeWaterPose(
            StylizedRiverFoamPreparedFreeWaterRegion prepared)
        {
            return new FreeWaterEvolutionPose
            {
                LocalDistance = prepared.CentreLocalDistance,
                AcrossNormalized = prepared.CentreAcrossNormalized,
                OrientationRadians = prepared.OrientationRadians,
                ScaleAlong = 1f,
                ScaleAcross = 1f,
                StrengthScale = 1f
            };
        }

        private void ResolveFreeWaterDwell(
            ref FreeWaterEvolutionSlot slot,
            uint stream)
        {
            uint seed = ResolveFreeWaterCycleSeed(slot, stream);
            float dwell = Mathf.Lerp(
                FreeWaterMinimumDwellSeconds,
                FreeWaterMaximumDwellSeconds,
                HashMajorEvolution(seed, 1u));
            slot.DwellRemaining = dwell;
            slot.LastDwellDuration = dwell;
            freeWaterObservedMinimumDwell = Mathf.Min(
                freeWaterObservedMinimumDwell,
                dwell);
            freeWaterObservedMaximumDwell = Mathf.Max(
                freeWaterObservedMaximumDwell,
                dwell);
        }

        private void ResolveFreeWaterOccurrenceBudget(
            ref FreeWaterEvolutionSlot slot)
        {
            uint seed = ResolveFreeWaterOccurrenceSeed(slot, 70u);
            slot.LifetimeUnitBudget = Mathf.Lerp(
                FreeWaterMinimumLifetimeUnits,
                FreeWaterMaximumLifetimeUnits,
                HashMajorEvolution(seed, 1u));
            slot.MaximumOccurrenceSeconds =
                slot.LifetimeUnitBudget *
                FreeWaterLifetimeSecondsPerUnit *
                FreeWaterLifetimeSafetyMultiplier;
        }

        private bool ShouldRecycleFreeWater(FreeWaterEvolutionSlot slot)
        {
            float usedUnits =
                FreeWaterLifetimeTimeWeight *
                    (slot.OccurrenceElapsed /
                        FreeWaterLifetimeSecondsPerUnit) +
                FreeWaterLifetimeHopWeight * slot.HopIndex;
            return usedUnits >= slot.LifetimeUnitBudget ||
                slot.OccurrenceElapsed >= slot.MaximumOccurrenceSeconds;
        }

        private float ResolveFreeWaterEgressStart()
        {
            return StylizedRiverFoamMajorTopology
                .ResolveEvolutionEgressStart(validFieldLength);
        }

        private bool IsInFreeWaterEgress(float localDistance)
        {
            return localDistance >= ResolveFreeWaterEgressStart();
        }

        private void RecycleFreeWater(ref FreeWaterEvolutionSlot slot)
        {
            IReadOnlyList<StylizedRiverFoamPreparedFreeWaterRegion> prepared =
                pocketTopology != null
                    ? pocketTopology.PreparedFreeWaterRegions
                    : Array.Empty<StylizedRiverFoamPreparedFreeWaterRegion>();
            if (slot.PreparedIndex < 0 ||
                slot.PreparedIndex >= prepared.Count)
            {
                return;
            }

            StylizedRiverFoamPreparedFreeWaterRegion preparedRegion =
                prepared[slot.PreparedIndex];
            IReadOnlyList<StylizedRiverFoamFreeWaterRecycleAnchor> anchors =
                preparedRegion.RecycleAnchors;
            if (anchors.Count == 0)
            {
                return;
            }

            slot.RecycleCount++;
            freeWaterRecycleCount++;
            uint recycleSeed = ResolveFreeWaterCycleSeed(slot, 50u);
            int anchorIndex = (int)(EvolutionMixBits(recycleSeed) %
                (uint)anchors.Count);
            if (anchors.Count > 1 && anchorIndex == slot.LastAnchorIndex)
            {
                anchorIndex = (anchorIndex + 1) % anchors.Count;
            }

            StylizedRiverFoamFreeWaterRecycleAnchor anchor =
                anchors[anchorIndex];
            slot.LastAnchorIndex = anchorIndex;
            float scaleAlong = Mathf.Lerp(
                0.86f,
                1.18f,
                HashMajorEvolution(recycleSeed, 1u));
            float scaleAcross = Mathf.Lerp(
                0.86f,
                1.18f,
                HashMajorEvolution(recycleSeed, 2u));
            float areaScale = Mathf.Max(
                0.25f,
                scaleAlong * scaleAcross);
            FreeWaterEvolutionPose recycledPose =
                new FreeWaterEvolutionPose
                {
                    LocalDistance = anchor.CentreLocalDistance,
                    AcrossNormalized = anchor.CentreAcrossNormalized,
                    OrientationRadians = WrapMajorAngle(
                        anchor.OrientationRadians +
                        Mathf.Lerp(
                            -0.12f,
                            0.12f,
                            HashMajorEvolution(recycleSeed, 3u))),
                    ScaleAlong = scaleAlong,
                    ScaleAcross = scaleAcross,
                    StrengthScale = Mathf.Clamp(
                        1f / Mathf.Sqrt(areaScale),
                        0.90f,
                        1.10f)
                };

            slot.Current = recycledPose;
            slot.Start = recycledPose;
            slot.Target = recycledPose;
            slot.MoveElapsed = 0f;
            slot.MoveDuration = 0f;
            slot.IsMoving = false;
            slot.OccurrenceElapsed = 0f;
            slot.HopIndex = 0;
            ResolveFreeWaterOccurrenceBudget(ref slot);
            ResolveFreeWaterDwell(ref slot, 60u);
        }

        private int CountMovingFreeWaterSlots()
        {
            int count = 0;
            for (int index = 0;
                 index < freeWaterEvolutionSlots.Length;
                 index++)
            {
                if (freeWaterEvolutionSlots[index].IsMoving)
                {
                    count++;
                }
            }

            return count;
        }

        private uint ResolveFreeWaterCycleSeed(
            FreeWaterEvolutionSlot slot,
            uint stream)
        {
            return EvolutionMixBits(
                slot.StableId ^
                ((uint)(slot.RecycleCount + 1) * 0x9E3779B9u) ^
                ((uint)(slot.HopIndex + 1) * 0x85EBCA6Bu) ^
                EvolutionMixBits(stream + 0x7F4A7C15u));
        }

        private uint ResolveFreeWaterOccurrenceSeed(
            FreeWaterEvolutionSlot slot,
            uint stream)
        {
            return EvolutionMixBits(
                slot.StableId ^
                ((uint)(slot.RecycleCount + 1) * 0x9E3779B9u) ^
                EvolutionMixBits(stream + 0x7F4A7C15u));
        }
    }
}
