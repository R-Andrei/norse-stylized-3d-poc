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
        private void RebuildStaticPressureTarget(double now)
        {
            if (staticTarget == null || computeShader == null)
            {
                return;
            }

            RecordFieldRebuild();
            DispatchClear(
                staticTarget,
                fieldWidth,
                fieldHeight,
                0,
                fieldWidth);

            validStaticSourceCount = 0;

            foreach (KeyValuePair<EntityId, ContinuousSource> pair in
                     continuousSources)
            {
                ContinuousSource source = pair.Value;
                if (!source.IsStatic ||
                    source.StaticTargetHeightMetres <= 0.0001f ||
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

                DispatchStaticPressureBake(
                    projection.GlobalDistance,
                    acrossNormalized,
                    surfaceHalfWidth,
                    source.StaticPressureAcrossHalfWidth,
                    source.StaticPressureAlongHalfLength,
                    source.StaticTargetHeightMetres,
                    source.StaticContactSharpness,
                    source.StaticPressureProfile.IsValid
                        ? 0f
                        : source.StaticProfileVariation,
                    source.Phase,
                    source.StaticPressureContour,
                    source.StaticPressureProfile);
                validStaticSourceCount++;
            }

            if (validStaticSourceCount > 0)
            {
                computeShader.SetInts(
                    "_FieldSize",
                    fieldWidth,
                    fieldHeight);
                computeShader.SetVector(
                    "_StaticCellSize",
                    new Vector4(
                        fieldLength / Mathf.Max(1, fieldWidth),
                        averageSurfaceHalfWidth * 2f /
                        Mathf.Max(1, fieldHeight),
                        0f,
                        0f));
                computeShader.SetTexture(
                    finalizeStaticPressureKernel,
                    "_StaticPressureWrite",
                    staticTarget);
                DispatchCompute(
                    finalizeStaticPressureKernel,
                    Mathf.CeilToInt(fieldWidth / (float)ThreadGroupSize),
                    Mathf.CeilToInt(fieldHeight / (float)ThreadGroupSize),
                    1,
                    PerformanceDispatchCategory.StaticPressureBake,
                    fieldWidth,
                    fieldHeight);
            }

            staticPressureTargetDirty = false;
            lastActivityTime = now;
        }

        private void UpdateStaticPressureProfiles(
            float deltaTime,
            double now)
        {
            if (river == null || deltaTime <= 0f)
            {
                return;
            }

            staticPressureProfileAccumulator += deltaTime;
            float updateInterval =
                1f / Mathf.Max(1f, StaticPressureProfileUpdateRate);
            if (staticPressureProfileAccumulator < updateInterval)
            {
                return;
            }

            float profileDeltaTime = Mathf.Min(
                staticPressureProfileAccumulator,
                0.25f);
            staticPressureProfileAccumulator = 0f;
            staticPressureProfileSourceIds.Clear();

            foreach (KeyValuePair<EntityId, ContinuousSource> pair in
                     continuousSources)
            {
                ContinuousSource source = pair.Value;
                if (source.IsStatic &&
                    source.StaticTargetHeightMetres > 0.0001f &&
                    source.StaticPressureProfile.IsValid &&
                    source.StaticPressureBaseProfile.IsValid &&
                    HasValidPressureProfileState(source))
                {
                    staticPressureProfileSourceIds.Add(pair.Key);
                }
            }

            bool anyProfileChanged = false;
            for (int sourceIndex = 0;
                 sourceIndex < staticPressureProfileSourceIds.Count;
                 sourceIndex++)
            {
                EntityId sourceId =
                    staticPressureProfileSourceIds[sourceIndex];
                if (!continuousSources.TryGetValue(
                        sourceId,
                        out ContinuousSource source))
                {
                    continue;
                }

                if (source.StaticProfileVariation > 0.0001f)
                {
                    if (!source.StaticPressureProfileScheduleInitialized)
                    {
                        float initialInterval =
                            ResolveStaticPressureProfileChangeInterval(
                                source,
                                source.StaticPressureProfileEventIndex,
                                2.03f);
                        source.StaticPressureNextProfileEventTime =
                            now + initialInterval;
                        source.StaticPressureProfileScheduleInitialized = true;
                    }
                    else if (
                        now >= source.StaticPressureNextProfileEventTime &&
                        source.StaticPressureProfileTransition >= 1f)
                    {
                        BeginStaticPressureProfileTransition(
                            ref source,
                            now,
                            updateInterval);
                    }
                }
                else
                {
                    source.StaticPressureProfileScheduleInitialized = false;
                }

                if (source.StaticPressureProfileTransition < 1f &&
                    source.StaticPressureProfileTransitionDuration >
                        0.0001f)
                {
                    source.StaticPressureProfileTransition =
                        Mathf.Min(
                            1f,
                            source.StaticPressureProfileTransition +
                            profileDeltaTime /
                            source.StaticPressureProfileTransitionDuration);
                    ApplyStaticPressureProfileTransition(ref source);
                    anyProfileChanged = true;
                }

                continuousSources[sourceId] = source;
            }

            if (anyProfileChanged)
            {
                // The cached geometry remains unchanged. Only the compact
                // lateral height profiles are rebaked, once after all sources
                // have advanced this update.
                staticPressureTargetDirty = true;
            }
        }

        private void BeginStaticPressureProfileTransition(
            ref ContinuousSource source,
            double now,
            float updateInterval)
        {
            Array.Copy(
                source.StaticPressureCurrentMultipliers,
                source.StaticPressureTransitionStartMultipliers,
                source.StaticPressureCurrentMultipliers.Length);

            source.StaticPressureProfileEventIndex++;
            GenerateStaticPressureTargetProfile(ref source);
            source.StaticPressureProfileTransition = 0f;

            float selectedInterval =
                ResolveStaticPressureProfileChangeInterval(
                    source,
                    source.StaticPressureProfileEventIndex,
                    2.89f);
            source.StaticPressureProfileTransitionDuration = Mathf.Clamp(
                selectedInterval *
                    StaticPressureProfileTransitionFraction,
                updateInterval,
                selectedInterval);
            source.StaticPressureNextProfileEventTime =
                now + selectedInterval;
        }

        private static float ResolveStaticPressureProfileChangeInterval(
            ContinuousSource source,
            uint eventIndex,
            float salt)
        {
            float intervalMin = Mathf.Clamp(
                Mathf.Min(
                    source.StaticPressureProfileChangeIntervalMin,
                    source.StaticPressureProfileChangeIntervalMax),
                StylizedRiver.MinimumStaticPressureProfileChangeInterval,
                StylizedRiver.MaximumStaticPressureProfileChangeInterval);
            float intervalMax = Mathf.Clamp(
                Mathf.Max(
                    source.StaticPressureProfileChangeIntervalMin,
                    source.StaticPressureProfileChangeIntervalMax),
                StylizedRiver.MinimumStaticPressureProfileChangeInterval,
                StylizedRiver.MaximumStaticPressureProfileChangeInterval);
            return Mathf.Lerp(
                intervalMin,
                intervalMax,
                StaticPressureProfileRandom01(
                    source.Phase,
                    eventIndex,
                    salt));
        }

        private static void GenerateStaticPressureTargetProfile(
            ref ContinuousSource source)
        {
            Vector4[] baseSamples =
                source.StaticPressureBaseProfile.Samples;
            float[] target = source.StaticPressureTargetMultipliers;
            int sampleCount = baseSamples.Length;
            float response = Mathf.Clamp01(
                source.StaticProfileVariation * 0.75f);
            int family = Mathf.Min(
                4,
                Mathf.FloorToInt(
                    StaticPressureProfileRandom01(
                        source.Phase,
                        source.StaticPressureProfileEventIndex,
                        0.11f) *
                    5f));
            float phaseA =
                StaticPressureProfileRandom01(
                    source.Phase,
                    source.StaticPressureProfileEventIndex,
                    0.37f) *
                Mathf.PI * 2f;
            float phaseB =
                StaticPressureProfileRandom01(
                    source.Phase,
                    source.StaticPressureProfileEventIndex,
                    0.73f) *
                Mathf.PI * 2f;
            float direction =
                StaticPressureProfileRandom01(
                    source.Phase,
                    source.StaticPressureProfileEventIndex,
                    1.19f) >= 0.5f
                    ? 1f
                    : -1f;
            float centreDirection =
                StaticPressureProfileRandom01(
                    source.Phase,
                    source.StaticPressureProfileEventIndex,
                    1.61f) >= 0.5f
                    ? 1f
                    : -1f;
            float familyAmplitude = family == 0 ? 0.18f : 0.48f;
            float amplitude = familyAmplitude * response;
            float minimumProfileMultiplier = sampleCount >= 64
                ? 0.86f
                : sampleCount >= 32
                    ? 0.82f
                    : StaticPressureMinimumProfileMultiplier;
            float maximumProfileMultiplier = sampleCount >= 64
                ? 1.10f
                : sampleCount >= 32
                    ? 1.12f
                    : MaximumStaticPressureModulation;
            float[] raw = source.StaticPressureRawScratch;
            float[] smoothed = source.StaticPressureSmoothedScratch;
            float rawSum = 0f;
            int validCount = 0;

            for (int index = 0; index < sampleCount; index++)
            {
                if (baseSamples[index].w <= 0.0001f ||
                    baseSamples[index].z <= 0.0001f)
                {
                    raw[index] = 1f;
                    target[index] = 1f;
                    continue;
                }

                float across01 = sampleCount > 1
                    ? index / (float)(sampleCount - 1)
                    : 0.5f;
                float signedAcross = across01 * 2f - 1f;
                float centreShape =
                    1f - 4f *
                    (across01 - 0.5f) *
                    (across01 - 0.5f);
                float shape = family switch
                {
                    0 =>
                        Mathf.Sin(
                            across01 * Mathf.PI * 2f + phaseA) *
                        0.22f,
                    1 =>
                        direction * -signedAcross +
                        Mathf.Sin(
                            across01 * Mathf.PI * 2f + phaseA) *
                        0.18f,
                    2 =>
                        centreDirection * centreShape +
                        Mathf.Sin(
                            across01 * Mathf.PI * 2f + phaseA) *
                        0.16f,
                    3 =>
                        Mathf.Cos(
                            across01 * Mathf.PI * 4f + phaseA) *
                        0.70f +
                        direction * signedAcross * 0.18f,
                    _ =>
                        Mathf.Sin(
                            across01 * Mathf.PI * 4f + phaseA) *
                        0.52f +
                        Mathf.Sin(
                            across01 * Mathf.PI * 6f + phaseB) *
                        0.12f
                };

                raw[index] = Mathf.Max(0.05f, 1f + amplitude * shape);
                rawSum += raw[index];
                validCount++;
            }

            float rawMean = validCount > 0
                ? rawSum / validCount
                : 1f;
            for (int index = 0; index < sampleCount; index++)
            {
                if (baseSamples[index].w <= 0.0001f ||
                    baseSamples[index].z <= 0.0001f)
                {
                    smoothed[index] = 1f;
                    continue;
                }

                float centre = raw[index] / Mathf.Max(0.0001f, rawMean);
                float left = index > 0 &&
                             baseSamples[index - 1].w > 0.0001f
                    ? raw[index - 1] / Mathf.Max(0.0001f, rawMean)
                    : centre;
                float right = index + 1 < sampleCount &&
                              baseSamples[index + 1].w > 0.0001f
                    ? raw[index + 1] / Mathf.Max(0.0001f, rawMean)
                    : centre;
                smoothed[index] =
                    (left + centre * 2f + right) * 0.25f;
            }

            float smoothedSum = 0f;
            validCount = 0;
            for (int index = 0; index < sampleCount; index++)
            {
                if (baseSamples[index].w <= 0.0001f ||
                    baseSamples[index].z <= 0.0001f)
                {
                    continue;
                }

                smoothedSum += smoothed[index];
                validCount++;
            }

            float smoothedMean = validCount > 0
                ? smoothedSum / validCount
                : 1f;
            for (int index = 0; index < sampleCount; index++)
            {
                if (baseSamples[index].w <= 0.0001f ||
                    baseSamples[index].z <= 0.0001f)
                {
                    target[index] = 1f;
                    continue;
                }

                target[index] = Mathf.Clamp(
                    smoothed[index] /
                    Mathf.Max(0.0001f, smoothedMean),
                    minimumProfileMultiplier,
                    maximumProfileMultiplier);
            }
        }

        private static void ApplyStaticPressureProfileTransition(
            ref ContinuousSource source)
        {
            Vector4[] baseSamples =
                source.StaticPressureBaseProfile.Samples;
            Vector4[] animatedSamples =
                source.StaticPressureProfile.Samples;
            float interpolation = Mathf.SmoothStep(
                0f,
                1f,
                source.StaticPressureProfileTransition);

            for (int index = 0; index < baseSamples.Length; index++)
            {
                Vector4 baseSample = baseSamples[index];
                if (baseSample.w <= 0.0001f ||
                    baseSample.z <= 0.0001f)
                {
                    animatedSamples[index] = baseSample;
                    source.StaticPressureCurrentMultipliers[index] = 1f;
                    continue;
                }

                float multiplier = Mathf.Lerp(
                    source.StaticPressureTransitionStartMultipliers[index],
                    source.StaticPressureTargetMultipliers[index],
                    interpolation);
                source.StaticPressureCurrentMultipliers[index] = multiplier;
                baseSample.z = Mathf.Min(
                    baseSample.w,
                    baseSample.z * multiplier);
                animatedSamples[index] = baseSample;
            }
        }

        private static bool HasValidPressureProfileState(
            ContinuousSource source)
        {
            if (!source.StaticPressureBaseProfile.IsValid ||
                !source.StaticPressureProfile.IsValid)
            {
                return false;
            }

            int sampleCount =
                source.StaticPressureBaseProfile.Samples.Length;
            return sampleCount > 0 &&
                   source.StaticPressureProfile.Samples.Length ==
                       sampleCount &&
                   source.StaticPressureCurrentMultipliers != null &&
                   source.StaticPressureTransitionStartMultipliers != null &&
                   source.StaticPressureTargetMultipliers != null &&
                   source.StaticPressureRawScratch != null &&
                   source.StaticPressureSmoothedScratch != null &&
                   source.StaticPressureCurrentMultipliers.Length ==
                       sampleCount &&
                   source.StaticPressureTransitionStartMultipliers.Length ==
                       sampleCount &&
                   source.StaticPressureTargetMultipliers.Length ==
                       sampleCount &&
                   source.StaticPressureRawScratch.Length ==
                       sampleCount &&
                   source.StaticPressureSmoothedScratch.Length ==
                       sampleCount;
        }

        private static float[] CreateUnitPressureProfileMultipliers(
            RiverDisturbancePressureBakeProfile profile)
        {
            if (!profile.IsValid)
            {
                return Array.Empty<float>();
            }

            float[] multipliers = new float[profile.Samples.Length];
            for (int index = 0; index < multipliers.Length; index++)
            {
                multipliers[index] = 1f;
            }

            return multipliers;
        }

        private static float[] CreatePressureProfileScratch(
            RiverDisturbancePressureBakeProfile profile)
        {
            return profile.IsValid
                ? new float[profile.Samples.Length]
                : Array.Empty<float>();
        }

        private static float StaticPressureProfileRandom01(
            float sourcePhase,
            uint eventIndex,
            float salt)
        {
            float input =
                sourcePhase * 37.719f +
                eventIndex * 11.137f +
                salt * 19.913f;
            return Mathf.Repeat(
                Mathf.Sin(input) * 43758.5453f,
                1f);
        }
    }
}
