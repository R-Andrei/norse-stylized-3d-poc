using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ProgrammaticStylized3D.Geometry;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverDisturbanceRuntime
    {
        private void RebuildStaticWakeSource(double now)
        {
            if (staticWakeSource == null || computeShader == null)
            {
                return;
            }

            RecordFieldRebuild();
            DispatchClear(
                staticWakeSource,
                wakeFieldWidth,
                wakeFieldHeight,
                0,
                wakeFieldWidth);
            ReleaseStaticWakeChunkReservations(now);

            float absoluteFlowSpeed =
                Mathf.Abs(river.FlowSpeedMetresPerSecond);
            validStaticWakeSourceCount = 0;

            foreach (KeyValuePair<EntityId, ContinuousSource> pair in
                     continuousSources)
            {
                ContinuousSource source = pair.Value;
                if (!source.IsStatic ||
                    source.StaticWakeAmplitude <= 0.0001f ||
                    !river.TryProjectWorldPoint(
                        source.WorldPosition,
                        out StylizedRiverProjection projection) ||
                    !projection.IsInside)
                {
                    continue;
                }

                StylizedRiverSplineSample sample =
                    river.SampleAtLocalDistance(projection.LocalDistance);
                float surfaceHalfWidth = Mathf.Max(
                    0.05f,
                    sample.GetSurfaceHalfWidth(projection.AcrossMetres));
                float acrossNormalized = Mathf.Clamp(
                    projection.AcrossMetres / surfaceHalfWidth,
                    -1f,
                    1f);
                float wakeLength = ResolveObstructionWakeLength(
                    source.AcrossHalfWidth,
                    source.AlongHalfLength,
                    absoluteFlowSpeed) *
                    source.StaticWakeReachMultiplier;
                double releaseDurationSeconds =
                    ResolveStaticWakeReleaseDuration(
                        wakeLength,
                        source.StaticWakeReachMultiplier,
                        absoluteFlowSpeed);

                DispatchStaticWakeSourceBake(
                    projection.GlobalDistance,
                    acrossNormalized,
                    surfaceHalfWidth,
                    source);

                MarkStaticWakeRange(
                    projection.GlobalDistance,
                    source.AlongHalfLength,
                    wakeLength,
                    releaseDurationSeconds);
                validStaticWakeSourceCount++;
            }

            staticWakeSourceDirty = false;
            lastActivityTime = now;
        }

        private void MarkStaticWakeRange(
            float globalDistance,
            float alongHalfLength,
            float wakeLength,
            double releaseDurationSeconds)
        {
            float sourceLocal =
                globalDistance - river.Domain.GlobalDistanceMinimum;
            float upstreamReach = alongHalfLength * 0.80f;
            // Keep one full downstream chunk active beyond the authored
            // reach so advection and lateral diffusion cannot terminate at
            // the source-range boundary.
            float downstreamReach = Mathf.Max(
                wakeLength,
                alongHalfLength * 1.20f) +
                ChunkLengthMetres;
            float minimumLocal = Mathf.Clamp(
                sourceLocal - upstreamReach,
                0f,
                validFieldLength);
            float maximumLocal = Mathf.Clamp(
                sourceLocal + downstreamReach,
                0f,
                validFieldLength);
            int minimumChunk = Mathf.Clamp(
                Mathf.FloorToInt(minimumLocal / ChunkLengthMetres),
                0,
                chunkCount - 1);
            int maximumChunk = Mathf.Clamp(
                Mathf.FloorToInt(maximumLocal / ChunkLengthMetres),
                0,
                chunkCount - 1);

            for (int chunk = minimumChunk; chunk <= maximumChunk; chunk++)
            {
                if (!wakeChunkActive[chunk])
                {
                    int xOffset = chunk * wakeResolutionPerChunk;
                    DispatchClear(
                        wakeA,
                        wakeFieldWidth,
                        wakeFieldHeight,
                        xOffset,
                        wakeResolutionPerChunk);
                    DispatchClear(
                        wakeB,
                        wakeFieldWidth,
                        wakeFieldHeight,
                        xOffset,
                        wakeResolutionPerChunk);
                    wakeChunkActive[chunk] = true;
                }

                chunkHasStaticSource[chunk] = true;
                staticWakeChunkReleaseDuration[chunk] = Math.Max(
                    staticWakeChunkReleaseDuration[chunk],
                    releaseDurationSeconds);
            }
        }

        private void ReleaseStaticWakeChunkReservations(double now)
        {
            for (int chunk = 0; chunk < chunkCount; chunk++)
            {
                if (!chunkHasStaticSource[chunk])
                {
                    staticWakeChunkReleaseDuration[chunk] = 0.0;
                    continue;
                }

                chunkHasStaticSource[chunk] = false;
                wakeChunkActive[chunk] = true;
                wakeChunkActiveUntil[chunk] = Math.Max(
                    wakeChunkActiveUntil[chunk],
                    now + Math.Max(
                        1.5,
                        staticWakeChunkReleaseDuration[chunk]));
                staticWakeChunkReleaseDuration[chunk] = 0.0;
            }
        }

        private void MarkWakeActive(
            float globalDistance,
            float radius,
            double now)
        {
            float localDistance = Mathf.Clamp(
                globalDistance - river.Domain.GlobalDistanceMinimum,
                0f,
                validFieldLength);
            int centreChunk = Mathf.Clamp(
                Mathf.FloorToInt(localDistance / ChunkLengthMetres),
                0,
                chunkCount - 1);
            int radiusChunks = Mathf.CeilToInt(
                radius / ChunkLengthMetres) + 1;
            double activeDuration = Mathf.Lerp(
                2.0f,
                10.0f,
                Mathf.InverseLerp(0.25f, 3f, river.WakeReach));

            for (int chunk = centreChunk - radiusChunks;
                 chunk <= centreChunk + radiusChunks;
                 chunk++)
            {
                if (chunk < 0 || chunk >= chunkCount)
                {
                    continue;
                }

                if (!wakeChunkActive[chunk])
                {
                    int xOffset = chunk * wakeResolutionPerChunk;
                    DispatchClear(
                        wakeA,
                        wakeFieldWidth,
                        wakeFieldHeight,
                        xOffset,
                        wakeResolutionPerChunk);
                    DispatchClear(
                        wakeB,
                        wakeFieldWidth,
                        wakeFieldHeight,
                        xOffset,
                        wakeResolutionPerChunk);
                    wakeChunkActive[chunk] = true;
                }

                wakeChunkActiveUntil[chunk] = Math.Max(
                    wakeChunkActiveUntil[chunk],
                    now + activeDuration);
            }

            lastActivityTime = now;
        }

        private static float ResolveObstructionWakeLength(
            float acrossHalfWidth,
            float alongHalfLength,
            float absoluteFlowSpeed)
        {
            float footprintScale = Mathf.Max(
                acrossHalfWidth * 1.20f,
                alongHalfLength * 1.40f);
            return footprintScale *
                   (1f + Mathf.Min(3f, absoluteFlowSpeed) * 0.12f);
        }

        private static double ResolveStaticWakeReleaseDuration(
            float wakeLength,
            float wakeReachMultiplier,
            float absoluteFlowSpeed)
        {
            // Mirror the current persistent-wake decay envelope while
            // also retaining enough time to transport the resolved source
            // reach at the current flow speed.
            float persistence = Mathf.Clamp(
                wakeReachMultiplier,
                0.25f,
                3f) / 3f;
            float persistenceScale = Mathf.Lerp(
                0.72f,
                1.65f,
                Mathf.Clamp01(persistence));
            float decayTailSeconds =
                Mathf.Log(100f) * persistenceScale / 1.15f;
            float transportSeconds =
                Mathf.Max(0f, wakeLength) /
                Mathf.Max(0.25f, absoluteFlowSpeed);

            return Mathf.Clamp(
                Mathf.Max(decayTailSeconds, transportSeconds),
                1.5f,
                12f);
        }

        private static Vector2[] CopyStaticContour(
            IReadOnlyList<Vector2> contour)
        {
            if (contour == null || contour.Count < 3)
            {
                return Array.Empty<Vector2>();
            }

            int count = Mathf.Min(
                contour.Count,
                MaximumStaticContourPoints);
            Vector2[] result = new Vector2[count];
            for (int index = 0; index < count; index++)
            {
                result[index] = contour[index];
            }

            return result;
        }

        private void UpdateStaticWakeVariations(
            float deltaTime,
            double now)
        {
            if (river == null || deltaTime <= 0f)
            {
                return;
            }

            staticWakeVariationAccumulator += deltaTime;
            float updateInterval =
                1f / Mathf.Max(1f, StaticWakeVariationUpdateRate);
            if (staticWakeVariationAccumulator < updateInterval)
            {
                return;
            }

            float variationDeltaTime = Mathf.Min(
                staticWakeVariationAccumulator,
                0.25f);
            staticWakeVariationAccumulator = 0f;
            staticWakeVariationSourceIds.Clear();

            foreach (KeyValuePair<EntityId, ContinuousSource> pair in
                     continuousSources)
            {
                ContinuousSource source = pair.Value;
                if (source.IsStatic &&
                    source.StaticWakeAmplitude > 0.0001f)
                {
                    staticWakeVariationSourceIds.Add(pair.Key);
                }
            }

            bool anyVariationChanged = false;
            for (int sourceIndex = 0;
                 sourceIndex < staticWakeVariationSourceIds.Count;
                 sourceIndex++)
            {
                EntityId sourceId =
                    staticWakeVariationSourceIds[sourceIndex];
                if (!continuousSources.TryGetValue(
                        sourceId,
                        out ContinuousSource source))
                {
                    continue;
                }

                bool sourceChanged;
                if (source.StaticWakeVariation <= 0.0001f)
                {
                    sourceChanged = ResetStaticWakeVariation(ref source);
                }
                else
                {
                    float sourcePhase = source.Phase;
                    float variationAmount = source.StaticWakeVariation;
                    float intervalMin =
                        source.StaticWakeVariationIntervalMin;
                    float intervalMax =
                        source.StaticWakeVariationIntervalMax;
                    sourceChanged =
                        UpdateStaticWakeLeeVariation(
                            ref source.StaticWakeLeeVariation,
                            sourcePhase,
                            variationAmount,
                            intervalMin,
                            intervalMax,
                            now,
                            variationDeltaTime,
                            updateInterval) |
                        UpdateStaticWakeReleaseVariation(
                            ref source.StaticWakeLeftReleaseVariation,
                            sourcePhase,
                            variationAmount,
                            intervalMin,
                            intervalMax,
                            now,
                            variationDeltaTime,
                            updateInterval,
                            11.17f) |
                        UpdateStaticWakeReleaseVariation(
                            ref source.StaticWakeRightReleaseVariation,
                            sourcePhase,
                            variationAmount,
                            intervalMin,
                            intervalMax,
                            now,
                            variationDeltaTime,
                            updateInterval,
                            23.41f);
                }

                if (sourceChanged)
                {
                    anyVariationChanged = true;
                }

                continuousSources[sourceId] = source;
            }

            if (anyVariationChanged)
            {
                staticWakeSourceDirty = true;
            }
        }

        private static bool ResetStaticWakeVariation(
            ref ContinuousSource source)
        {
            bool changed =
                ResetStaticWakeLeeVariation(
                    ref source.StaticWakeLeeVariation) |
                ResetStaticWakeReleaseVariation(
                    ref source.StaticWakeLeftReleaseVariation) |
                ResetStaticWakeReleaseVariation(
                    ref source.StaticWakeRightReleaseVariation);
            return changed;
        }

        private static bool UpdateStaticWakeLeeVariation(
            ref StaticWakeLeeVariationState state,
            float sourcePhase,
            float variationAmount,
            float intervalMin,
            float intervalMax,
            double now,
            float deltaTime,
            float updateInterval)
        {
            if (!HasValidStaticWakeLeeVariationState(state))
            {
                return false;
            }

            if (!state.ScheduleInitialized)
            {
                state.SelectedInterval = ResolveStaticWakeVariationInterval(
                    sourcePhase,
                    intervalMin,
                    intervalMax,
                    state.EventIndex,
                    3.17f);
                state.NextEventTime = now + state.SelectedInterval;
                state.ScheduleInitialized = true;
            }
            else if (now >= state.NextEventTime &&
                     state.Transition >= 1f)
            {
                BeginStaticWakeLeeVariationTransition(
                    ref state,
                    sourcePhase,
                    variationAmount,
                    intervalMin,
                    intervalMax,
                    now,
                    updateInterval);
            }

            if (state.Transition >= 1f ||
                state.TransitionDuration <= 0.0001f)
            {
                return false;
            }

            state.Transition = Mathf.Min(
                1f,
                state.Transition + deltaTime / state.TransitionDuration);
            ApplyStaticWakeLeeVariationTransition(ref state);
            return true;
        }

        private static bool UpdateStaticWakeReleaseVariation(
            ref StaticWakeReleaseVariationState state,
            float sourcePhase,
            float variationAmount,
            float intervalMin,
            float intervalMax,
            double now,
            float deltaTime,
            float updateInterval,
            float scheduleSalt)
        {
            if (!state.ScheduleInitialized)
            {
                state.SelectedInterval = ResolveStaticWakeVariationInterval(
                    sourcePhase,
                    intervalMin,
                    intervalMax,
                    state.EventIndex,
                    scheduleSalt);
                state.NextEventTime = now + state.SelectedInterval;
                state.ScheduleInitialized = true;
            }
            else if (now >= state.NextEventTime &&
                     state.Transition >= 1f)
            {
                BeginStaticWakeReleaseVariationTransition(
                    ref state,
                    sourcePhase,
                    variationAmount,
                    intervalMin,
                    intervalMax,
                    now,
                    updateInterval,
                    scheduleSalt);
            }

            if (state.Transition >= 1f ||
                state.TransitionDuration <= 0.0001f)
            {
                return false;
            }

            state.Transition = Mathf.Min(
                1f,
                state.Transition + deltaTime / state.TransitionDuration);
            ApplyStaticWakeReleaseVariationTransition(ref state);
            return true;
        }

        private static void BeginStaticWakeLeeVariationTransition(
            ref StaticWakeLeeVariationState state,
            float sourcePhase,
            float variationAmount,
            float intervalMin,
            float intervalMax,
            double now,
            float updateInterval)
        {
            Array.Copy(
                state.CurrentDepthMultipliers,
                state.TransitionStartDepthMultipliers,
                state.SampleCount);
            Array.Copy(
                state.CurrentLengthMultipliers,
                state.TransitionStartLengthMultipliers,
                state.SampleCount);
            Array.Copy(
                state.CurrentTrailingEdgeOffsets,
                state.TransitionStartTrailingEdgeOffsets,
                state.SampleCount);

            state.EventIndex++;
            GenerateStaticWakeLeeTargetProfile(
                ref state,
                sourcePhase,
                variationAmount);
            state.Transition = 0f;
            state.SelectedInterval = ResolveStaticWakeVariationInterval(
                sourcePhase,
                intervalMin,
                intervalMax,
                state.EventIndex,
                4.73f);
            state.TransitionDuration = Mathf.Clamp(
                state.SelectedInterval *
                    StaticWakeVariationTransitionFraction,
                updateInterval,
                state.SelectedInterval);
            state.NextEventTime = now + state.SelectedInterval;
        }

        private static void BeginStaticWakeReleaseVariationTransition(
            ref StaticWakeReleaseVariationState state,
            float sourcePhase,
            float variationAmount,
            float intervalMin,
            float intervalMax,
            double now,
            float updateInterval,
            float scheduleSalt)
        {
            state.TransitionStartLateralOffset =
                state.CurrentLateralOffset;
            state.TransitionStartEnergyMultiplier =
                state.CurrentEnergyMultiplier;
            state.TransitionStartWidthMultiplier =
                state.CurrentWidthMultiplier;
            state.TransitionStartDownstreamOffset =
                state.CurrentDownstreamOffset;

            state.EventIndex++;
            GenerateStaticWakeReleaseTarget(
                ref state,
                sourcePhase,
                state.EventIndex,
                variationAmount,
                scheduleSalt);
            state.Transition = 0f;
            state.SelectedInterval = ResolveStaticWakeVariationInterval(
                sourcePhase,
                intervalMin,
                intervalMax,
                state.EventIndex,
                scheduleSalt + 2.31f);
            state.TransitionDuration = Mathf.Clamp(
                state.SelectedInterval *
                    StaticWakeVariationTransitionFraction,
                updateInterval,
                state.SelectedInterval);
            state.NextEventTime = now + state.SelectedInterval;
        }

        private static float ResolveStaticWakeVariationInterval(
            float sourcePhase,
            float authoredIntervalMin,
            float authoredIntervalMax,
            uint eventIndex,
            float salt)
        {
            float intervalMin = Mathf.Clamp(
                Mathf.Min(authoredIntervalMin, authoredIntervalMax),
                StylizedRiver.MinimumStaticWakeVariationInterval,
                StylizedRiver.MaximumStaticWakeVariationInterval);
            float intervalMax = Mathf.Clamp(
                Mathf.Max(authoredIntervalMin, authoredIntervalMax),
                StylizedRiver.MinimumStaticWakeVariationInterval,
                StylizedRiver.MaximumStaticWakeVariationInterval);
            return Mathf.Lerp(
                intervalMin,
                intervalMax,
                StaticWakeVariationRandom01(
                    sourcePhase,
                    eventIndex,
                    salt));
        }

        private static void GenerateStaticWakeLeeTargetProfile(
            ref StaticWakeLeeVariationState state,
            float sourcePhase,
            float variationAmount)
        {
            float variation = Mathf.Clamp01(variationAmount);
            int family = Mathf.Min(
                5,
                Mathf.FloorToInt(
                    StaticWakeVariationRandom01(
                        sourcePhase,
                        state.EventIndex,
                        0.31f) * 6f));
            state.ProfileFamily = family;

            GenerateStaticWakeVariationPattern(
                state.RawScratch,
                state.SmoothedScratch,
                state.SampleCount,
                sourcePhase,
                state.EventIndex,
                0.67f,
                family);
            for (int index = 0; index < state.SampleCount; index++)
            {
                state.TargetDepthMultipliers[index] = Mathf.Clamp(
                    1f + state.SmoothedScratch[index] *
                    0.20f * variation,
                    0.80f,
                    1.20f);
            }

            GenerateStaticWakeVariationPattern(
                state.RawScratch,
                state.SmoothedScratch,
                state.SampleCount,
                sourcePhase,
                state.EventIndex,
                1.13f,
                (family + 2) % 6);
            for (int index = 0; index < state.SampleCount; index++)
            {
                state.TargetLengthMultipliers[index] = Mathf.Clamp(
                    1f + state.SmoothedScratch[index] *
                    0.15f * variation,
                    0.85f,
                    1.15f);
            }

            GenerateStaticWakeVariationPattern(
                state.RawScratch,
                state.SmoothedScratch,
                state.SampleCount,
                sourcePhase,
                state.EventIndex,
                1.79f,
                (family + 4) % 6);
            for (int index = 0; index < state.SampleCount; index++)
            {
                state.TargetTrailingEdgeOffsets[index] =
                    state.SmoothedScratch[index] *
                    0.75f * variation;
            }
        }

        private static void GenerateStaticWakeVariationPattern(
            float[] raw,
            float[] smoothed,
            int sampleCount,
            float sourcePhase,
            uint eventIndex,
            float salt,
            int family)
        {
            float phaseA = StaticWakeVariationRandom01(
                sourcePhase,
                eventIndex,
                salt + 0.17f) * Mathf.PI * 2f;
            float phaseB = StaticWakeVariationRandom01(
                sourcePhase,
                eventIndex,
                salt + 0.53f) * Mathf.PI * 2f;
            float direction = StaticWakeVariationRandom01(
                sourcePhase,
                eventIndex,
                salt + 0.91f) >= 0.5f
                    ? 1f
                    : -1f;

            for (int index = 0; index < sampleCount; index++)
            {
                float across01 = sampleCount > 1
                    ? index / (float)(sampleCount - 1)
                    : 0.5f;
                float signedAcross = across01 * 2f - 1f;
                float centreShape =
                    1f - signedAcross * signedAcross;
                float edgeShape =
                    Mathf.Abs(signedAcross) * 2f - 1f;
                float shape = family switch
                {
                    0 =>
                        direction * signedAcross +
                        Mathf.Sin(
                            across01 * Mathf.PI * 2f + phaseA) *
                        0.18f,
                    1 =>
                        direction * centreShape +
                        Mathf.Sin(
                            across01 * Mathf.PI * 2f + phaseA) *
                        0.20f,
                    2 =>
                        direction * edgeShape +
                        Mathf.Cos(
                            across01 * Mathf.PI * 2f + phaseA) *
                        0.16f,
                    3 =>
                        Mathf.Cos(
                            across01 * Mathf.PI * 2f + phaseA) *
                        0.82f +
                        direction * signedAcross * 0.18f,
                    4 =>
                        Mathf.Sin(
                            across01 * Mathf.PI * 2f + phaseA) *
                        0.70f +
                        Mathf.Sin(
                            across01 * Mathf.PI * 4f + phaseB) *
                        0.22f,
                    _ =>
                        Mathf.Cos(
                            across01 * Mathf.PI * 2f + phaseA) *
                        0.56f +
                        Mathf.Sin(
                            across01 * Mathf.PI * 4f + phaseB) *
                        0.26f +
                        direction * signedAcross * 0.14f
                };
                float edgeInfluence = Mathf.Lerp(
                    0.38f,
                    1f,
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        1f - Mathf.Abs(signedAcross)));
                raw[index] = shape * edgeInfluence;
            }

            SmoothStaticWakeVariationPattern(
                raw,
                smoothed,
                sampleCount);
            SmoothStaticWakeVariationPattern(
                smoothed,
                raw,
                sampleCount);

            float mean = 0f;
            for (int index = 0; index < sampleCount; index++)
            {
                mean += raw[index];
            }
            mean /= Mathf.Max(1, sampleCount);

            float maximumMagnitude = 0f;
            for (int index = 0; index < sampleCount; index++)
            {
                smoothed[index] = raw[index] - mean;
                maximumMagnitude = Mathf.Max(
                    maximumMagnitude,
                    Mathf.Abs(smoothed[index]));
            }

            float normalization = maximumMagnitude > 0.0001f
                ? 1f / maximumMagnitude
                : 0f;
            for (int index = 0; index < sampleCount; index++)
            {
                smoothed[index] *= normalization;
            }
        }

        private static void SmoothStaticWakeVariationPattern(
            float[] source,
            float[] destination,
            int sampleCount)
        {
            for (int index = 0; index < sampleCount; index++)
            {
                float centre = source[index];
                float left = index > 0
                    ? source[index - 1]
                    : centre;
                float right = index + 1 < sampleCount
                    ? source[index + 1]
                    : centre;
                destination[index] =
                    (left + centre * 2f + right) * 0.25f;
            }
        }

        private static void GenerateStaticWakeReleaseTarget(
            ref StaticWakeReleaseVariationState state,
            float sourcePhase,
            uint eventIndex,
            float variationAmount,
            float salt)
        {
            float variation = Mathf.Clamp01(variationAmount);
            state.TargetLateralOffset =
                StaticWakeVariationRandomSigned(
                    sourcePhase,
                    eventIndex,
                    salt + 0.19f) *
                0.15f * variation;
            state.TargetEnergyMultiplier = Mathf.Clamp(
                1f + StaticWakeVariationRandomSigned(
                    sourcePhase,
                    eventIndex,
                    salt + 0.47f) *
                0.20f * variation,
                0.80f,
                1.20f);
            state.TargetWidthMultiplier = Mathf.Clamp(
                1f + StaticWakeVariationRandomSigned(
                    sourcePhase,
                    eventIndex,
                    salt + 0.83f) *
                0.12f * variation,
                0.88f,
                1.12f);
            state.TargetDownstreamOffset =
                StaticWakeVariationRandomSigned(
                    sourcePhase,
                    eventIndex,
                    salt + 1.21f) *
                0.50f * variation;
        }

        private static void ApplyStaticWakeLeeVariationTransition(
            ref StaticWakeLeeVariationState state)
        {
            float interpolation = Mathf.SmoothStep(
                0f,
                1f,
                state.Transition);
            for (int index = 0; index < state.SampleCount; index++)
            {
                state.CurrentDepthMultipliers[index] = Mathf.Lerp(
                    state.TransitionStartDepthMultipliers[index],
                    state.TargetDepthMultipliers[index],
                    interpolation);
                state.CurrentLengthMultipliers[index] = Mathf.Lerp(
                    state.TransitionStartLengthMultipliers[index],
                    state.TargetLengthMultipliers[index],
                    interpolation);
                state.CurrentTrailingEdgeOffsets[index] = Mathf.Lerp(
                    state.TransitionStartTrailingEdgeOffsets[index],
                    state.TargetTrailingEdgeOffsets[index],
                    interpolation);
            }
        }

        private static void ApplyStaticWakeReleaseVariationTransition(
            ref StaticWakeReleaseVariationState state)
        {
            float interpolation = Mathf.SmoothStep(
                0f,
                1f,
                state.Transition);
            state.CurrentLateralOffset = Mathf.Lerp(
                state.TransitionStartLateralOffset,
                state.TargetLateralOffset,
                interpolation);
            state.CurrentEnergyMultiplier = Mathf.Lerp(
                state.TransitionStartEnergyMultiplier,
                state.TargetEnergyMultiplier,
                interpolation);
            state.CurrentWidthMultiplier = Mathf.Lerp(
                state.TransitionStartWidthMultiplier,
                state.TargetWidthMultiplier,
                interpolation);
            state.CurrentDownstreamOffset = Mathf.Lerp(
                state.TransitionStartDownstreamOffset,
                state.TargetDownstreamOffset,
                interpolation);
        }

        private static bool ResetStaticWakeLeeVariation(
            ref StaticWakeLeeVariationState state)
        {
            if (!HasValidStaticWakeLeeVariationState(state))
            {
                return false;
            }

            bool changed = false;
            for (int index = 0; index < state.SampleCount; index++)
            {
                changed |=
                    Mathf.Abs(state.CurrentDepthMultipliers[index] - 1f) >
                        0.0001f ||
                    Mathf.Abs(state.CurrentLengthMultipliers[index] - 1f) >
                        0.0001f ||
                    Mathf.Abs(state.CurrentTrailingEdgeOffsets[index]) >
                        0.0001f;
                state.CurrentDepthMultipliers[index] = 1f;
                state.TransitionStartDepthMultipliers[index] = 1f;
                state.TargetDepthMultipliers[index] = 1f;
                state.CurrentLengthMultipliers[index] = 1f;
                state.TransitionStartLengthMultipliers[index] = 1f;
                state.TargetLengthMultipliers[index] = 1f;
                state.CurrentTrailingEdgeOffsets[index] = 0f;
                state.TransitionStartTrailingEdgeOffsets[index] = 0f;
                state.TargetTrailingEdgeOffsets[index] = 0f;
            }

            state.Transition = 1f;
            state.TransitionDuration = 0f;
            state.SelectedInterval = 0f;
            state.EventIndex = 0u;
            state.NextEventTime = 0.0;
            state.ScheduleInitialized = false;
            state.ProfileFamily = 0;
            return changed;
        }

        private static bool ResetStaticWakeReleaseVariation(
            ref StaticWakeReleaseVariationState state)
        {
            bool changed =
                Mathf.Abs(state.CurrentLateralOffset) > 0.0001f ||
                Mathf.Abs(state.CurrentEnergyMultiplier - 1f) > 0.0001f ||
                Mathf.Abs(state.CurrentWidthMultiplier - 1f) > 0.0001f ||
                Mathf.Abs(state.CurrentDownstreamOffset) > 0.0001f;
            state = CreateStaticWakeReleaseVariationState();
            return changed;
        }

        private static bool HasValidStaticWakeLeeVariationState(
            StaticWakeLeeVariationState state)
        {
            int sampleCount = state.SampleCount;
            return sampleCount > 0 &&
                   sampleCount <=
                       RiverDisturbanceFootprintResolver.
                           MaximumPressureSupportLateralSamples &&
                   state.CurrentDepthMultipliers != null &&
                   state.TransitionStartDepthMultipliers != null &&
                   state.TargetDepthMultipliers != null &&
                   state.CurrentLengthMultipliers != null &&
                   state.TransitionStartLengthMultipliers != null &&
                   state.TargetLengthMultipliers != null &&
                   state.CurrentTrailingEdgeOffsets != null &&
                   state.TransitionStartTrailingEdgeOffsets != null &&
                   state.TargetTrailingEdgeOffsets != null &&
                   state.RawScratch != null &&
                   state.SmoothedScratch != null &&
                   state.CurrentDepthMultipliers.Length == sampleCount &&
                   state.TransitionStartDepthMultipliers.Length ==
                       sampleCount &&
                   state.TargetDepthMultipliers.Length == sampleCount &&
                   state.CurrentLengthMultipliers.Length == sampleCount &&
                   state.TransitionStartLengthMultipliers.Length ==
                       sampleCount &&
                   state.TargetLengthMultipliers.Length == sampleCount &&
                   state.CurrentTrailingEdgeOffsets.Length == sampleCount &&
                   state.TransitionStartTrailingEdgeOffsets.Length ==
                       sampleCount &&
                   state.TargetTrailingEdgeOffsets.Length == sampleCount &&
                   state.RawScratch.Length == sampleCount &&
                   state.SmoothedScratch.Length == sampleCount;
        }

        private static float StaticWakeVariationRandom01(
            float sourcePhase,
            uint eventIndex,
            float salt)
        {
            float input =
                sourcePhase * 43.117f +
                eventIndex * 13.731f +
                salt * 23.419f;
            return Mathf.Repeat(
                Mathf.Sin(input) * 43758.5453f,
                1f);
        }

        private static float StaticWakeVariationRandomSigned(
            float sourcePhase,
            uint eventIndex,
            float salt)
        {
            return StaticWakeVariationRandom01(
                sourcePhase,
                eventIndex,
                salt) * 2f - 1f;
        }
    }
}
