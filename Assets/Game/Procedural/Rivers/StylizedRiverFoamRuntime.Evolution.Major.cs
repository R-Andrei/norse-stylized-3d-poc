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
        private void InitializeMajorEvolution()
        {
            ReleaseMajorEvolutionResources();
            if (majorTopology == null || river == null ||
                !river.Domain.IsValid || computeShader == null ||
                evolvingMajorTexture == null ||
                buildEvolvingMajorSupportKernel < 0)
            {
                ClearRenderTexture(evolvingMajorTexture);
                ClearRenderTexture(evolvingHostedNegativeTexture);
                ClearRenderTexture(evolvingFreeWaterNegativeTexture);
                ClearRenderTexture(evolvingConnectorTexture);
                ClearRenderTexture(evolvingWeakSpanNegativeTexture);
                return;
            }

            IReadOnlyList<StylizedRiverFoamMajorRegion> regions =
                majorTopology.Regions;
            IReadOnlyList<StylizedRiverFoamPreparedMajorRegion> prepared =
                majorTopology.PreparedRegions;
            int slotCount = Mathf.Min(regions.Count, prepared.Count);
            if (slotCount <= 0)
            {
                ClearRenderTexture(evolvingMajorTexture);
                ClearRenderTexture(evolvingHostedNegativeTexture);
                ClearRenderTexture(evolvingFreeWaterNegativeTexture);
                ClearRenderTexture(evolvingConnectorTexture);
                ClearRenderTexture(evolvingWeakSpanNegativeTexture);
                return;
            }

            int maskResolution = prepared[0].MaskResolution;
            for (int index = 1; index < slotCount; index++)
            {
                if (prepared[index].MaskResolution != maskResolution)
                {
                    ClearRenderTexture(evolvingMajorTexture);
                    ClearRenderTexture(evolvingHostedNegativeTexture);
                    ClearRenderTexture(evolvingFreeWaterNegativeTexture);
                    ClearRenderTexture(evolvingConnectorTexture);
                    ClearRenderTexture(evolvingWeakSpanNegativeTexture);
                    return;
                }
            }

            majorEvolutionSlots = new MajorEvolutionSlot[slotCount];
            majorEvolutionGpuData = new FoamMajorEvolutionData[slotCount];
            majorMaskUploadPixels = new Color[
                maskResolution * maskResolution];
            majorMaskTextureArray = new Texture2DArray(
                maskResolution,
                maskResolution,
                slotCount,
                TextureFormat.RGBAHalf,
                false,
                true)
            {
                name = "T_PS3D_RiverFoamMajorMasks",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            for (int index = 0; index < slotCount; index++)
            {
                StylizedRiverFoamPreparedMajorRegion preparedRegion =
                    prepared[index];
                float[] source = preparedRegion.LocalSupportData;
                for (int pixel = 0;
                     pixel < majorMaskUploadPixels.Length;
                     pixel++)
                {
                    float value = pixel < source.Length
                        ? Mathf.Clamp01(source[pixel])
                        : 0f;
                    majorMaskUploadPixels[pixel] = new Color(
                        value,
                        0f,
                        0f,
                        0f);
                }

                majorMaskTextureArray.SetPixels(
                    majorMaskUploadPixels,
                    index,
                    0);

                StylizedRiverFoamMajorRegion region = regions[index];
                MajorEvolutionPose initialPose = new MajorEvolutionPose
                {
                    LocalDistance = Mathf.Clamp(
                        region.CentreGlobalDistance -
                            river.Domain.GlobalDistanceMinimum,
                        0f,
                        validFieldLength),
                    AcrossNormalized = Mathf.Clamp(
                        region.CentreAcrossNormalized,
                        -0.82f,
                        0.82f),
                    OrientationRadians = region.OrientationRadians,
                    MetresPerCandidateCell =
                        region.MetresPerCandidateCell,
                    ScaleAlong = 1f,
                    ScaleAcross = 1f,
                    Shear = 0f,
                    WarpAlong = 0f,
                    WarpAcross = 0f,
                    WarpPhaseA = HashMajorEvolution(
                        region.EvolutionSeed,
                        1u) * Mathf.PI * 2f,
                    WarpPhaseB = HashMajorEvolution(
                        region.EvolutionSeed,
                        2u) * Mathf.PI * 2f,
                    SupportScale = 1f
                };

                MajorEvolutionSlot slot = new MajorEvolutionSlot
                {
                    StableId = region.StableId,
                    PreparedIndex = index,
                    BaseMetresPerCandidateCell =
                        region.MetresPerCandidateCell,
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
                ResolveMajorOccurrenceBudget(ref slot);
                ResolveMajorDwell(ref slot, 10u);
                if (IsInMajorEgress(initialPose.LocalDistance))
                {
                    slot.DwellRemaining = Mathf.Min(
                        slot.DwellRemaining,
                        Mathf.Lerp(
                            0.25f,
                            0.85f,
                            HashMajorEvolution(
                                slot.StableId,
                                11u)));
                }

                majorEvolutionSlots[index] = slot;
            }

            majorMaskTextureArray.Apply(false, true);
            majorEvolutionBuffer = new ComputeBuffer(
                slotCount,
                sizeof(float) * 20,
                ComputeBufferType.Structured);
            majorEvolutionReady = true;
            majorEvolutionAccumulator = 0f;
            majorEvolutionLifetimeInputSignature =
                ResolveMajorEvolutionLifetimeInputSignature();
            InitializeHostedNegativeEvolution(false);
            InitializeFreeWaterEvolution(false);
            BuildEvolvingMajorField();
        }

        private bool TryResolveMajorEvolutionSlot(
            StylizedRiverFoamConnectorEndpointBinding binding,
            out int slotIndex)
        {
            slotIndex = -1;
            if (!binding.IsAvailable)
            {
                return false;
            }

            int preferredIndex = binding.MajorPreparedIndex;
            if (preferredIndex >= 0 &&
                preferredIndex < majorEvolutionSlots.Length)
            {
                MajorEvolutionSlot preferred =
                    majorEvolutionSlots[preferredIndex];
                if (preferred.PreparedIndex == binding.MajorPreparedIndex &&
                    preferred.StableId == binding.MajorStableId)
                {
                    slotIndex = preferredIndex;
                    return true;
                }
            }

            // Preparation-only fallback for defensive index drift. Runtime
            // evolution stores the resolved slot and performs no host search.
            for (int index = 0;
                 index < majorEvolutionSlots.Length;
                 index++)
            {
                MajorEvolutionSlot candidate = majorEvolutionSlots[index];
                if (candidate.PreparedIndex == binding.MajorPreparedIndex &&
                    candidate.StableId == binding.MajorStableId)
                {
                    slotIndex = index;
                    return true;
                }
            }

            return false;
        }

        private void ReleaseMajorEvolutionResources()
        {
            ReleaseHostedNegativeEvolutionResources();
            ReleaseFreeWaterEvolutionResources();
            ReleaseConnectorIdentityReconstructionResources();
            majorEvolutionBuffer?.Release();
            majorEvolutionBuffer = null;
            if (majorMaskTextureArray != null)
            {
                DestroyUnityObject(majorMaskTextureArray);
                majorMaskTextureArray = null;
            }

            majorEvolutionSlots = Array.Empty<MajorEvolutionSlot>();
            majorEvolutionGpuData = Array.Empty<FoamMajorEvolutionData>();
            majorMaskUploadPixels = Array.Empty<Color>();
            majorEvolutionAccumulator = 0f;
            majorEvolutionReady = false;
            majorEvolutionLifetimeInputSignature = int.MinValue;
            majorEvolutionReconstructionTicks = 0;
            majorEvolutionRecycleCount = 0;
            majorEvolutionCrowdedRecycleFallbackCount = 0;
            majorEvolutionUpstreamViolations = 0;
            majorEvolutionObservedMinimumDwell = float.PositiveInfinity;
            majorEvolutionObservedMaximumDwell = 0f;
            majorEvolutionObservedMinimumMove = float.PositiveInfinity;
            majorEvolutionObservedMaximumMove = 0f;
            majorEvolutionLastCpuMilliseconds = 0.0;
            majorEvolutionLastAllocatedBytes = 0L;
        }

        private bool AdvanceMajorEvolution(float deltaTime)
        {
            if (!majorEvolutionReady || deltaTime <= 0f ||
                majorEvolutionSlots.Length == 0)
            {
                return false;
            }

            using var profilerScope =
                MajorEvolutionAdvanceProfilerMarker.Auto();
            RefreshMajorEvolutionLifetimeBudgetsIfNeeded();
            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            double startTime = Time.realtimeSinceStartupAsDouble;
            bool immediateRebuild = false;
            bool anyMoving = false;

            for (int index = 0;
                 index < majorEvolutionSlots.Length;
                 index++)
            {
                ref MajorEvolutionSlot slot =
                    ref majorEvolutionSlots[index];
                slot.OccurrenceElapsed += deltaTime;

                if (slot.IsMoving)
                {
                    slot.MoveElapsed += deltaTime;
                    if (slot.MoveElapsed >= slot.MoveDuration)
                    {
                        if (slot.Target.LocalDistance + 0.0001f <
                            slot.Start.LocalDistance)
                        {
                            majorEvolutionUpstreamViolations++;
                        }

                        slot.Current = slot.Target;
                        slot.IsMoving = false;
                        slot.MoveElapsed = slot.MoveDuration;
                        slot.HopIndex++;
                        CompleteHostedNegativeMove(index);
                        immediateRebuild = true;

                        if (ShouldRecycleMajor(slot) ||
                            IsInMajorEgress(
                                slot.Current.LocalDistance))
                        {
                            RecycleMajor(index, ref slot);
                        }
                        else
                        {
                            ResolveMajorDwell(ref slot, 20u);
                        }
                    }
                    else
                    {
                        anyMoving = true;
                    }

                    continue;
                }

                // A dwelling occurrence may exhaust its combined lifetime
                // budget between hops. Recycle it immediately rather than
                // allowing the remaining dwell to create a persistent clump.
                if (ShouldRecycleMajor(slot))
                {
                    RecycleMajor(index, ref slot);
                    immediateRebuild = true;
                    continue;
                }

                slot.DwellRemaining -= deltaTime;
                if (slot.DwellRemaining > 0f)
                {
                    continue;
                }

                if (BeginMajorMove(index, ref slot))
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
                majorEvolutionAccumulator += deltaTime;
            }
            else
            {
                majorEvolutionAccumulator = 0f;
            }

            float tickInterval = 1f / MajorEvolutionTickRate;
            bool scheduledRebuild = anyMoving &&
                majorEvolutionAccumulator >= tickInterval;
            if (scheduledRebuild)
            {
                majorEvolutionAccumulator %= tickInterval;
            }

            bool reconstructionRequired =
                immediateRebuild || scheduledRebuild;

            majorEvolutionLastCpuMilliseconds =
                (Time.realtimeSinceStartupAsDouble - startTime) * 1000.0;
            majorEvolutionLastAllocatedBytes = Math.Max(
                0L,
                GC.GetAllocatedBytesForCurrentThread() - allocatedBefore);
            return reconstructionRequired;
        }

        private bool BeginMajorMove(
            int slotIndex,
            ref MajorEvolutionSlot slot)
        {
            uint cycleSeed = ResolveMajorCycleSeed(slot, 30u);
            float flowScale = Mathf.Lerp(
                0.78f,
                1.32f,
                Mathf.Clamp01(
                    river.FlowSpeedMetresPerSecond / 4.5f));
            float downstreamStep = Mathf.Lerp(
                MajorMinimumHopMetres,
                MajorMaximumHopMetres,
                HashMajorEvolution(cycleSeed, 1u)) * flowScale;
            float targetDistance = slot.Current.LocalDistance +
                downstreamStep;
            if (targetDistance >= ResolveMajorEgressStart())
            {
                RecycleMajor(slotIndex, ref slot);
                return false;
            }

            float lateralMagnitude = Mathf.Lerp(
                MajorMinimumLateralHop,
                MajorMaximumLateralHop,
                HashMajorEvolution(cycleSeed, 2u));
            float lateralSign = HashMajorEvolution(cycleSeed, 3u) < 0.5f
                ? -1f
                : 1f;
            float targetAcross = Mathf.Clamp(
                slot.Current.AcrossNormalized +
                    lateralMagnitude * lateralSign,
                -0.78f,
                0.78f);
            if (Mathf.Abs(
                    targetAcross - slot.Current.AcrossNormalized) < 0.02f)
            {
                targetAcross = Mathf.Clamp(
                    slot.Current.AcrossNormalized -
                        lateralMagnitude * lateralSign,
                    -0.78f,
                    0.78f);
            }

            float scaleAlong = Mathf.Lerp(
                0.80f,
                1.24f,
                HashMajorEvolution(cycleSeed, 4u));
            float scaleAcross = Mathf.Lerp(
                0.78f,
                1.22f,
                HashMajorEvolution(cycleSeed, 5u));
            float metresScale = Mathf.Lerp(
                0.96f,
                1.04f,
                HashMajorEvolution(cycleSeed, 7u));
            float areaScale = Mathf.Max(
                0.20f,
                scaleAlong * scaleAcross * metresScale * metresScale);
            float supportScale = Mathf.Clamp(
                1f / areaScale,
                0.72f,
                1.28f);

            slot.Start = slot.Current;
            slot.Target = new MajorEvolutionPose
            {
                LocalDistance = targetDistance,
                AcrossNormalized = targetAcross,
                OrientationRadians = WrapMajorAngle(
                    slot.Current.OrientationRadians +
                    Mathf.Lerp(
                        -0.30f,
                        0.30f,
                        HashMajorEvolution(cycleSeed, 6u))),
                MetresPerCandidateCell =
                    slot.BaseMetresPerCandidateCell * metresScale,
                ScaleAlong = scaleAlong,
                ScaleAcross = scaleAcross,
                Shear = Mathf.Lerp(
                    -0.18f,
                    0.18f,
                    HashMajorEvolution(cycleSeed, 8u)),
                WarpAlong = Mathf.Lerp(
                    0.035f,
                    0.115f,
                    HashMajorEvolution(cycleSeed, 9u)),
                WarpAcross = Mathf.Lerp(
                    0.045f,
                    0.145f,
                    HashMajorEvolution(cycleSeed, 10u)),
                WarpPhaseA = HashMajorEvolution(
                    cycleSeed,
                    11u) * Mathf.PI * 2f,
                WarpPhaseB = HashMajorEvolution(
                    cycleSeed,
                    12u) * Mathf.PI * 2f,
                SupportScale = supportScale
            };

            float dwellProgress = Mathf.InverseLerp(
                MajorMinimumDwellSeconds,
                MajorMaximumDwellSeconds,
                slot.LastDwellDuration);
            slot.MoveDuration = Mathf.Clamp(
                Mathf.Lerp(
                    MajorMinimumMoveSeconds,
                    MajorMaximumMoveSeconds,
                    dwellProgress * 0.72f +
                    HashMajorEvolution(cycleSeed, 13u) * 0.28f),
                MajorMinimumMoveSeconds,
                MajorMaximumMoveSeconds);
            slot.MoveElapsed = 0f;
            slot.IsMoving = true;
            majorEvolutionObservedMinimumMove = Mathf.Min(
                majorEvolutionObservedMinimumMove,
                slot.MoveDuration);
            majorEvolutionObservedMaximumMove = Mathf.Max(
                majorEvolutionObservedMaximumMove,
                slot.MoveDuration);
            BeginHostedNegativeMove(slotIndex, cycleSeed);
            return true;
        }

        private void RecycleMajor(
            int slotIndex,
            ref MajorEvolutionSlot slot)
        {
            IReadOnlyList<StylizedRiverFoamPreparedMajorRegion> prepared =
                majorTopology.PreparedRegions;
            if (slot.PreparedIndex < 0 ||
                slot.PreparedIndex >= prepared.Count)
            {
                return;
            }

            StylizedRiverFoamPreparedMajorRegion preparedRegion =
                prepared[slot.PreparedIndex];
            IReadOnlyList<StylizedRiverFoamMajorRecycleAnchor> anchors =
                preparedRegion.RecycleAnchors;
            if (anchors.Count == 0)
            {
                return;
            }

            slot.RecycleCount++;
            majorEvolutionRecycleCount++;
            uint recycleSeed = ResolveMajorCycleSeed(slot, 50u);
            int anchorIndex = ResolveMajorRecycleAnchorIndex(
                slot,
                preparedRegion,
                anchors,
                recycleSeed,
                out bool crowdedFallback);
            if (crowdedFallback)
            {
                majorEvolutionCrowdedRecycleFallbackCount++;
            }

            StylizedRiverFoamMajorRecycleAnchor anchor =
                anchors[anchorIndex];
            slot.LastAnchorIndex = anchorIndex;
            slot.BaseMetresPerCandidateCell =
                anchor.MetresPerCandidateCell;
            float scaleAlong = Mathf.Lerp(
                0.86f,
                1.18f,
                HashMajorEvolution(recycleSeed, 1u));
            float scaleAcross = Mathf.Lerp(
                0.84f,
                1.16f,
                HashMajorEvolution(recycleSeed, 2u));
            float areaScale = Mathf.Max(
                0.20f,
                scaleAlong * scaleAcross);
            MajorEvolutionPose recycledPose = new MajorEvolutionPose
            {
                LocalDistance = Mathf.Clamp(
                    anchor.CentreLocalDistance,
                    0f,
                    ResolveMajorEgressStart() - 0.01f),
                AcrossNormalized = anchor.CentreAcrossNormalized,
                OrientationRadians = WrapMajorAngle(
                    anchor.OrientationRadians +
                    Mathf.Lerp(
                        -0.18f,
                        0.18f,
                        HashMajorEvolution(recycleSeed, 3u))),
                MetresPerCandidateCell =
                    anchor.MetresPerCandidateCell,
                ScaleAlong = scaleAlong,
                ScaleAcross = scaleAcross,
                Shear = Mathf.Lerp(
                    -0.14f,
                    0.14f,
                    HashMajorEvolution(recycleSeed, 4u)),
                WarpAlong = Mathf.Lerp(
                    0.03f,
                    0.11f,
                    HashMajorEvolution(recycleSeed, 5u)),
                WarpAcross = Mathf.Lerp(
                    0.04f,
                    0.14f,
                    HashMajorEvolution(recycleSeed, 6u)),
                WarpPhaseA = HashMajorEvolution(
                    recycleSeed,
                    7u) * Mathf.PI * 2f,
                WarpPhaseB = HashMajorEvolution(
                    recycleSeed,
                    8u) * Mathf.PI * 2f,
                SupportScale = Mathf.Clamp(
                    1f / areaScale,
                    0.72f,
                    1.28f)
            };

            slot.Current = recycledPose;
            slot.Start = recycledPose;
            slot.Target = recycledPose;
            slot.MoveElapsed = 0f;
            slot.MoveDuration = 0f;
            slot.IsMoving = false;
            slot.OccurrenceElapsed = 0f;
            slot.HopIndex = 0;
            ResolveMajorOccurrenceBudget(ref slot);
            ResolveMajorDwell(ref slot, 60u);
            RecycleHostedNegativeSlots(slotIndex, recycleSeed);
        }

        private int ResolveMajorRecycleAnchorIndex(
            MajorEvolutionSlot slot,
            StylizedRiverFoamPreparedMajorRegion preparedRegion,
            IReadOnlyList<StylizedRiverFoamMajorRecycleAnchor> anchors,
            uint recycleSeed,
            out bool crowdedFallback)
        {
            crowdedFallback = false;
            if (anchors.Count <= 1)
            {
                crowdedFallback = anchors.Count == 1 &&
                    ResolveMajorAnchorMinimumSpacing(
                        slot.PreparedIndex,
                        preparedRegion,
                        anchors[0]) < 0.72f;
                return 0;
            }

            int startIndex = (int)(EvolutionMixBits(recycleSeed) %
                (uint)anchors.Count);
            int bestIndex = -1;
            float bestMinimumSpacing = float.NegativeInfinity;
            for (int offset = 0; offset < anchors.Count; offset++)
            {
                int anchorIndex = (startIndex + offset) % anchors.Count;
                if (anchorIndex == slot.LastAnchorIndex)
                {
                    continue;
                }

                StylizedRiverFoamMajorRecycleAnchor anchor =
                    anchors[anchorIndex];
                float minimumSpacing = ResolveMajorAnchorMinimumSpacing(
                    slot.PreparedIndex,
                    preparedRegion,
                    anchor);
                if (minimumSpacing > bestMinimumSpacing + 0.0001f)
                {
                    bestMinimumSpacing = minimumSpacing;
                    bestIndex = anchorIndex;
                }
            }

            if (bestIndex < 0)
            {
                bestIndex = startIndex;
                bestMinimumSpacing = ResolveMajorAnchorMinimumSpacing(
                    slot.PreparedIndex,
                    preparedRegion,
                    anchors[bestIndex]);
            }

            crowdedFallback = bestMinimumSpacing < 0.72f;
            return bestIndex;
        }

        private float ResolveMajorAnchorMinimumSpacing(
            int slotPreparedIndex,
            StylizedRiverFoamPreparedMajorRegion preparedRegion,
            StylizedRiverFoamMajorRecycleAnchor anchor)
        {
            StylizedRiverSplineSample anchorSample =
                river.Domain.SampleAtOrientedDistance(
                    anchor.CentreLocalDistance);
            float anchorAcrossMetres =
                StylizedRiverFoamTopologyFieldSpace
                    .SignedNormalizedToMetres(
                        anchor.CentreAcrossNormalized,
                        Mathf.Max(
                            0.05f,
                            anchorSample.LeftSurfaceHalfWidth),
                        Mathf.Max(
                            0.05f,
                            anchorSample.RightSurfaceHalfWidth));
            float anchorRadius = Mathf.Max(
                preparedRegion.MajorHalfExtentCells,
                preparedRegion.MinorHalfExtentCells) *
                anchor.MetresPerCandidateCell;
            float minimumNormalisedDistance = 4f;

            IReadOnlyList<StylizedRiverFoamPreparedMajorRegion> prepared =
                majorTopology.PreparedRegions;
            for (int index = 0; index < majorEvolutionSlots.Length; index++)
            {
                MajorEvolutionSlot otherSlot = majorEvolutionSlots[index];
                if (otherSlot.PreparedIndex == slotPreparedIndex ||
                    otherSlot.PreparedIndex < 0 ||
                    otherSlot.PreparedIndex >= prepared.Count)
                {
                    continue;
                }

                MajorEvolutionPose otherPose = ResolveMajorPose(otherSlot);
                float otherAcrossMetres =
                    StylizedRiverFoamTopologyFieldSpace
                        .SignedNormalizedToMetres(
                            otherPose.AcrossNormalized,
                            Mathf.Max(
                                0.05f,
                                anchorSample.LeftSurfaceHalfWidth),
                            Mathf.Max(
                                0.05f,
                                anchorSample.RightSurfaceHalfWidth));
                StylizedRiverFoamPreparedMajorRegion otherPrepared =
                    prepared[otherSlot.PreparedIndex];
                float otherRadius = Mathf.Max(
                    otherPrepared.MajorHalfExtentCells,
                    otherPrepared.MinorHalfExtentCells) *
                    otherPose.MetresPerCandidateCell *
                    Mathf.Max(otherPose.ScaleAlong, otherPose.ScaleAcross);
                float alongDistance = Mathf.Abs(
                    anchor.CentreLocalDistance - otherPose.LocalDistance);
                float acrossDistance = Mathf.Abs(
                    anchorAcrossMetres - otherAcrossMetres);
                float normalisedDistance = Mathf.Sqrt(
                    alongDistance * alongDistance +
                    acrossDistance * acrossDistance) /
                    Mathf.Max(
                        0.25f,
                        (anchorRadius + otherRadius) * 0.82f);
                minimumNormalisedDistance = Mathf.Min(
                    minimumNormalisedDistance,
                    normalisedDistance);
            }

            return minimumNormalisedDistance;
        }

        private void ResolveMajorOccurrenceBudget(
            ref MajorEvolutionSlot slot)
        {
            uint cycleSeed = ResolveMajorOccurrenceSeed(slot, 70u);
            float baseUnits = river != null
                ? river.FoamMajorLifetimeUnits
                : 6f;
            float deviation = river != null
                ? river.FoamMajorLifetimeUnitDeviation
                : 2f;
            float signedVariation = Mathf.Lerp(
                -deviation,
                deviation,
                HashMajorEvolution(cycleSeed, 1u));
            slot.LifetimeUnitBudget = Mathf.Max(
                1f,
                baseUnits + signedVariation);
            slot.MaximumOccurrenceSeconds =
                slot.LifetimeUnitBudget *
                MajorLifetimeSecondsPerUnit *
                MajorLifetimeSafetyMultiplier;
        }

        private void RefreshMajorEvolutionLifetimeBudgetsIfNeeded()
        {
            int signature = ResolveMajorEvolutionLifetimeInputSignature();
            if (signature == majorEvolutionLifetimeInputSignature)
            {
                return;
            }

            for (int index = 0;
                 index < majorEvolutionSlots.Length;
                 index++)
            {
                ResolveMajorOccurrenceBudget(
                    ref majorEvolutionSlots[index]);
            }

            majorEvolutionLifetimeInputSignature = signature;
        }

        private int ResolveMajorEvolutionLifetimeInputSignature()
        {
            if (river == null)
            {
                return int.MinValue;
            }

            unchecked
            {
                int hash = 31;
                hash = hash * 37 + Mathf.RoundToInt(
                    river.FoamMajorLifetimeUnits * 1000f);
                hash = hash * 37 + Mathf.RoundToInt(
                    river.FoamMajorLifetimeUnitDeviation * 1000f);
                return hash;
            }
        }

        private void ResolveMajorDwell(
            ref MajorEvolutionSlot slot,
            uint stream)
        {
            uint cycleSeed = ResolveMajorCycleSeed(slot, stream);
            float dwell = Mathf.Lerp(
                MajorMinimumDwellSeconds,
                MajorMaximumDwellSeconds,
                HashMajorEvolution(cycleSeed, 1u));
            slot.DwellRemaining = dwell;
            slot.LastDwellDuration = dwell;
            majorEvolutionObservedMinimumDwell = Mathf.Min(
                majorEvolutionObservedMinimumDwell,
                dwell);
            majorEvolutionObservedMaximumDwell = Mathf.Max(
                majorEvolutionObservedMaximumDwell,
                dwell);
        }

        private bool ShouldRecycleMajor(MajorEvolutionSlot slot)
        {
            // A normal five-second dwell-plus-move cycle and one completed hop
            // consume approximately one unit. Slow occurrences are therefore
            // charged by time, active occurrences by hops, and neither factor
            // can independently permit an unusually persistent local clump.
            float usedUnits =
                MajorLifetimeTimeWeight *
                    (slot.OccurrenceElapsed /
                        MajorLifetimeSecondsPerUnit) +
                MajorLifetimeHopWeight * slot.HopIndex;
            return usedUnits >= slot.LifetimeUnitBudget ||
                slot.OccurrenceElapsed >=
                    slot.MaximumOccurrenceSeconds;
        }

        private float ResolveMajorEgressStart()
        {
            return StylizedRiverFoamMajorTopology
                .ResolveEvolutionEgressStart(validFieldLength);
        }

        private bool IsInMajorEgress(float localDistance)
        {
            return localDistance >= ResolveMajorEgressStart();
        }

        private bool BuildEvolvingMajorField()
        {
            if (!majorEvolutionReady || computeShader == null ||
                majorEvolutionBuffer == null ||
                majorMaskTextureArray == null ||
                hostedNegativeEvolutionBuffer == null ||
                hostedNegativeMaskTextureArray == null ||
                freeWaterEvolutionBuffer == null ||
                freeWaterNegativeMaskTextureArray == null ||
                evolvingMajorTexture == null ||
                evolvingHostedNegativeTexture == null ||
                evolvingFreeWaterNegativeTexture == null ||
                evolvingConnectorTexture == null ||
                evolvingWeakSpanNegativeTexture == null ||
                topologyGeneratedTexture == null ||
                boundaryTexture == null ||
                obstacleExclusionTexture == null ||
                metricBuffer == null ||
                buildEvolvingMajorSupportKernel < 0 ||
                river == null || !river.Domain.IsValid)
            {
                return false;
            }

            IReadOnlyList<StylizedRiverFoamPreparedMajorRegion> prepared =
                majorTopology.PreparedRegions;
            for (int index = 0;
                 index < majorEvolutionSlots.Length;
                 index++)
            {
                MajorEvolutionSlot slot = majorEvolutionSlots[index];
                MajorEvolutionPose pose = ResolveMajorPose(slot);
                StylizedRiverFoamPreparedMajorRegion preparedRegion =
                    prepared[slot.PreparedIndex];
                majorEvolutionGpuData[index] =
                    new FoamMajorEvolutionData
                    {
                        CentreAndPlacement = new Vector4(
                            pose.LocalDistance,
                            pose.AcrossNormalized,
                            pose.OrientationRadians,
                            pose.MetresPerCandidateCell),
                        CandidateShape = new Vector4(
                            preparedRegion.CentroidCells.x,
                            preparedRegion.CentroidCells.y,
                            preparedRegion.PrincipalAngleRadians,
                            index),
                        CandidateExtents = new Vector4(
                            preparedRegion.MajorHalfExtentCells,
                            preparedRegion.MinorHalfExtentCells,
                            preparedRegion.MaskResolution,
                            pose.SupportScale),
                        Morph = new Vector4(
                            pose.ScaleAlong,
                            pose.ScaleAcross,
                            pose.Shear,
                            0f),
                        Warp = new Vector4(
                            pose.WarpAlong,
                            pose.WarpAcross,
                            pose.WarpPhaseA,
                            pose.WarpPhaseB)
                    };
            }

            IReadOnlyList<StylizedRiverFoamPreparedHostedNegativeRegion>
                hostedPrepared = pocketTopology != null
                    ? pocketTopology.PreparedHostedRegions
                    : Array.Empty<
                        StylizedRiverFoamPreparedHostedNegativeRegion>();
            for (int index = 0;
                 index < hostedNegativeEvolutionSlots.Length;
                 index++)
            {
                HostedNegativeEvolutionSlot slot =
                    hostedNegativeEvolutionSlots[index];
                HostedNegativeEvolutionPose pose =
                    ResolveHostedNegativePose(slot);
                StylizedRiverFoamPreparedHostedNegativeRegion preparedRegion =
                    hostedPrepared[slot.PreparedIndex];
                hostedNegativeEvolutionGpuData[index] =
                    new FoamHostedNegativeEvolutionData
                    {
                        HostAndMask = new Vector4(
                            slot.HostSlotIndex,
                            index,
                            pose.StrengthScale,
                            slot.RegionClass ==
                                StylizedRiverFoamNegativeRegionClass
                                    .EdgeCavity
                                ? 1f
                                : 0f),
                        CentreAndOffset = new Vector4(
                            preparedRegion.CentreCandidateCells.x,
                            preparedRegion.CentreCandidateCells.y,
                            pose.OffsetCells.x,
                            pose.OffsetCells.y),
                        Morph = new Vector4(
                            pose.ScaleAlong,
                            pose.ScaleAcross,
                            pose.RotationRadians,
                            0f)
                    };
            }

            IReadOnlyList<StylizedRiverFoamPreparedFreeWaterRegion>
                freeWaterPrepared = pocketTopology != null
                    ? pocketTopology.PreparedFreeWaterRegions
                    : Array.Empty<StylizedRiverFoamPreparedFreeWaterRegion>();
            for (int index = 0;
                 index < freeWaterEvolutionSlots.Length;
                 index++)
            {
                FreeWaterEvolutionSlot slot = freeWaterEvolutionSlots[index];
                FreeWaterEvolutionPose pose = ResolveFreeWaterPose(slot);
                StylizedRiverFoamPreparedFreeWaterRegion preparedRegion =
                    freeWaterPrepared[slot.PreparedIndex];
                freeWaterEvolutionGpuData[index] =
                    new FoamFreeWaterEvolutionData
                    {
                        CentreAndPlacement = new Vector4(
                            pose.LocalDistance,
                            pose.AcrossNormalized,
                            pose.OrientationRadians,
                            preparedRegion.MetresPerCell),
                        MaskAndStrength = new Vector4(
                            preparedRegion.MaskResolution * 0.5f,
                            preparedRegion.MaskResolution * 0.5f,
                            index,
                            pose.StrengthScale),
                        Morph = new Vector4(
                            0f,
                            0f,
                            pose.ScaleAlong,
                            pose.ScaleAcross)
                    };
            }

            UpdateConnectorEvolutionDescriptors(true);
            EnsureConnectorIdentityBuffers();

            using (MajorEvolutionUploadProfilerMarker.Auto())
            {
                majorEvolutionBuffer.SetData(majorEvolutionGpuData);
                if (hostedNegativeEvolutionSlots.Length > 0)
                {
                    hostedNegativeEvolutionBuffer.SetData(
                        hostedNegativeEvolutionGpuData,
                        0,
                        0,
                        hostedNegativeEvolutionSlots.Length);
                }
                if (freeWaterEvolutionSlots.Length > 0)
                {
                    freeWaterEvolutionBuffer.SetData(
                        freeWaterEvolutionGpuData,
                        0,
                        0,
                        freeWaterEvolutionSlots.Length);
                }
                if (connectorIdentityReconstructionReady)
                {
                    connectorIdentityBuffer.SetData(
                        connectorIdentityGpuData);
                    connectorPathPointBuffer.SetData(
                        connectorPathPointGpuData);
                }
            }

            using (MajorEvolutionBuildProfilerMarker.Auto())
            {
                ConfigureTopologyParameters(0f);
                computeShader.SetInt(
                    "_FoamMajorEvolutionCount",
                    majorEvolutionSlots.Length);
                computeShader.SetInt(
                    "_FoamHostedNegativeEvolutionCount",
                    hostedNegativeEvolutionSlots.Length);
                computeShader.SetInt(
                    "_FoamFreeWaterEvolutionCount",
                    freeWaterEvolutionSlots.Length);
                computeShader.SetInt(
                    "_FoamConnectorIdentityCount",
                    connectorIdentityReconstructionReady
                        ? connectorIdentityGpuData.Length
                        : 0);
                computeShader.SetInt(
                    "_FoamWeakSpanIdentityCount",
                    weakSpanIdentityReconstructionReady
                        ? weakSpanIdentityGpuData.Length
                        : 0);
                computeShader.SetInts(
                    "_FoamMajorMaskDimensions",
                    majorMaskTextureArray.width,
                    majorMaskTextureArray.height);
                computeShader.SetInts(
                    "_FoamHostedNegativeMaskDimensions",
                    hostedNegativeMaskTextureArray.width,
                    hostedNegativeMaskTextureArray.height);
                computeShader.SetInts(
                    "_FoamFreeWaterMaskDimensions",
                    freeWaterNegativeMaskTextureArray.width,
                    freeWaterNegativeMaskTextureArray.height);
                computeShader.SetBuffer(
                    buildEvolvingMajorSupportKernel,
                    "_FoamMajorEvolutionRecords",
                    majorEvolutionBuffer);
                computeShader.SetBuffer(
                    buildEvolvingMajorSupportKernel,
                    "_FoamMetricRows",
                    metricBuffer);
                computeShader.SetBuffer(
                    buildEvolvingMajorSupportKernel,
                    "_FoamHostedNegativeEvolutionRecords",
                    hostedNegativeEvolutionBuffer);
                computeShader.SetBuffer(
                    buildEvolvingMajorSupportKernel,
                    "_FoamFreeWaterEvolutionRecords",
                    freeWaterEvolutionBuffer);
                computeShader.SetBuffer(
                    buildEvolvingMajorSupportKernel,
                    "_FoamConnectorIdentityRecords",
                    connectorIdentityBuffer);
                computeShader.SetBuffer(
                    buildEvolvingMajorSupportKernel,
                    "_FoamConnectorPathPoints",
                    connectorPathPointBuffer);
                computeShader.SetBuffer(
                    buildEvolvingMajorSupportKernel,
                    "_FoamWeakSpanIdentityRecords",
                    weakSpanIdentityBuffer);
                computeShader.SetTexture(
                    buildEvolvingMajorSupportKernel,
                    "_FoamMajorMasks",
                    majorMaskTextureArray);
                computeShader.SetTexture(
                    buildEvolvingMajorSupportKernel,
                    "_FoamHostedNegativeMasks",
                    hostedNegativeMaskTextureArray);
                computeShader.SetTexture(
                    buildEvolvingMajorSupportKernel,
                    "_FoamFreeWaterNegativeMasks",
                    freeWaterNegativeMaskTextureArray);
                computeShader.SetTexture(
                    buildEvolvingMajorSupportKernel,
                    "_FoamTopologyGeneratedRead",
                    topologyGeneratedTexture);
                computeShader.SetTexture(
                    buildEvolvingMajorSupportKernel,
                    "_FoamBoundary",
                    boundaryTexture);
                computeShader.SetTexture(
                    buildEvolvingMajorSupportKernel,
                    "_FoamObstacleExclusionRead",
                    obstacleExclusionTexture);
                computeShader.SetTexture(
                    buildEvolvingMajorSupportKernel,
                    "_FoamEvolvingMajorWrite",
                    evolvingMajorTexture);
                computeShader.SetTexture(
                    buildEvolvingMajorSupportKernel,
                    "_FoamEvolvingHostedNegativeWrite",
                    evolvingHostedNegativeTexture);
                computeShader.SetTexture(
                    buildEvolvingMajorSupportKernel,
                    "_FoamEvolvingFreeWaterNegativeWrite",
                    evolvingFreeWaterNegativeTexture);
                computeShader.SetTexture(
                    buildEvolvingMajorSupportKernel,
                    "_FoamEvolvingConnectorWrite",
                    evolvingConnectorTexture);
                computeShader.SetTexture(
                    buildEvolvingMajorSupportKernel,
                    "_FoamEvolvingWeakSpanNegativeWrite",
                    evolvingWeakSpanNegativeTexture);
                Dispatch(
                    buildEvolvingMajorSupportKernel,
                    guidanceWidth,
                    guidanceHeight);
            }

            majorEvolutionReconstructionTicks++;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            RequestHostedNegativeInitialParityIfNeeded();
            RequestConnectorIdentityParityIfNeeded();
            RequestWeakSpanIdentityParityIfNeeded();
#endif
            return true;
        }

        private MajorEvolutionPose ResolveMajorPose(
            MajorEvolutionSlot slot)
        {
            if (!slot.IsMoving || slot.MoveDuration <= 0.0001f)
            {
                return slot.Current;
            }

            float t = Mathf.Clamp01(
                slot.MoveElapsed / slot.MoveDuration);
            t = t * t * (3f - 2f * t);
            return new MajorEvolutionPose
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
                MetresPerCandidateCell = Mathf.Lerp(
                    slot.Start.MetresPerCandidateCell,
                    slot.Target.MetresPerCandidateCell,
                    t),
                ScaleAlong = Mathf.Lerp(
                    slot.Start.ScaleAlong,
                    slot.Target.ScaleAlong,
                    t),
                ScaleAcross = Mathf.Lerp(
                    slot.Start.ScaleAcross,
                    slot.Target.ScaleAcross,
                    t),
                Shear = Mathf.Lerp(
                    slot.Start.Shear,
                    slot.Target.Shear,
                    t),
                WarpAlong = Mathf.Lerp(
                    slot.Start.WarpAlong,
                    slot.Target.WarpAlong,
                    t),
                WarpAcross = Mathf.Lerp(
                    slot.Start.WarpAcross,
                    slot.Target.WarpAcross,
                    t),
                WarpPhaseA = LerpFullAngle(
                    slot.Start.WarpPhaseA,
                    slot.Target.WarpPhaseA,
                    t),
                WarpPhaseB = LerpFullAngle(
                    slot.Start.WarpPhaseB,
                    slot.Target.WarpPhaseB,
                    t),
                SupportScale = Mathf.Lerp(
                    slot.Start.SupportScale,
                    slot.Target.SupportScale,
                    t)
            };
        }

        private int CountMovingMajorSlots()
        {
            int count = 0;
            for (int index = 0;
                 index < majorEvolutionSlots.Length;
                 index++)
            {
                if (majorEvolutionSlots[index].IsMoving)
                {
                    count++;
                }
            }

            return count;
        }

        private uint ResolveMajorCycleSeed(
            MajorEvolutionSlot slot,
            uint stream)
        {
            return EvolutionMixBits(
                slot.StableId ^
                ((uint)(slot.RecycleCount + 1) * 0x9E3779B9u) ^
                ((uint)(slot.HopIndex + 1) * 0x85EBCA6Bu) ^
                EvolutionMixBits(stream + 0xC2B2AE35u));
        }

        private uint ResolveMajorOccurrenceSeed(
            MajorEvolutionSlot slot,
            uint stream)
        {
            // Occurrence-level values must remain stable throughout every hop.
            // Excluding HopIndex also means Inspector changes preserve the same
            // deterministic deviation selector for the current occurrence.
            return EvolutionMixBits(
                slot.StableId ^
                ((uint)(slot.RecycleCount + 1) * 0x9E3779B9u) ^
                EvolutionMixBits(stream + 0xC2B2AE35u));
        }
    }
}
