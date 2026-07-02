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
        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            ActiveRuntimes.Clear();
            GeneratedSourceDiagnostics.Clear();
            sourcePhaseSequence = 1;
        }

        public static bool TryGetGeneratedSourceDiagnostics(
            IGeneratedGeometrySource source,
            out GeneratedRiverDisturbanceDiagnostics diagnostics)
        {
            diagnostics = default;
            if (source == null)
            {
                return false;
            }

            MeshFilter meshFilter = source.GeometryMeshFilter;
            return meshFilter != null &&
                   GeneratedSourceDiagnostics.TryGetValue(
                       meshFilter.GetEntityId(),
                       out diagnostics);
        }

#if UNITY_EDITOR
        public static bool TryGetGeneratedSourcePressureProfileDebugData(
            IGeneratedGeometrySource source,
            out GeneratedRiverPressureProfileDebugData debugData)
        {
            debugData = default;
            if (source == null)
            {
                return false;
            }

            MeshFilter meshFilter = source.GeometryMeshFilter;
            if (meshFilter == null)
            {
                return false;
            }

            EntityId sourceId = meshFilter.GetEntityId();
            if (!GeneratedSourceDiagnostics.TryGetValue(
                    sourceId,
                    out GeneratedRiverDisturbanceDiagnostics diagnostics))
            {
                return false;
            }

            for (int runtimeIndex = 0;
                 runtimeIndex < ActiveRuntimes.Count;
                 runtimeIndex++)
            {
                StylizedRiverDisturbanceRuntime runtime =
                    ActiveRuntimes[runtimeIndex];
                if (runtime == null ||
                    runtime.river == null ||
                    runtime.river != diagnostics.River ||
                    !runtime.continuousSources.TryGetValue(
                        sourceId,
                        out ContinuousSource continuousSource))
                {
                    continue;
                }

                RiverDisturbancePressureBakeProfile baseProfile =
                    continuousSource.StaticPressureBaseProfile;
                RiverDisturbancePressureBakeProfile currentProfile =
                    continuousSource.StaticPressureProfile;
                if (!baseProfile.IsValid ||
                    !currentProfile.IsValid ||
                    baseProfile.LateralSampleCount !=
                        currentProfile.LateralSampleCount)
                {
                    return false;
                }

                int sampleCount = baseProfile.LateralSampleCount;
                Vector2 appliedMultiplierBounds = sampleCount >= 64
                    ? new Vector2(0.86f, 1.10f)
                    : sampleCount >= 32
                        ? new Vector2(0.82f, 1.12f)
                        : new Vector2(
                            StaticPressureMinimumProfileMultiplier,
                            MaximumStaticPressureModulation);
                float targetHeight = diagnostics.EffectiveAmplitude;
                if (!baseProfile.HasGeometryBounds ||
                    !currentProfile.HasGeometryBounds)
                {
                    return false;
                }

                const float protectedDownstreamStartFraction = 0.50f;
                const float insideGateDownstreamTailPixels = 0.45f;
                float cellSizeX = runtime.fieldLength /
                    Mathf.Max(1, runtime.fieldWidth);
                float pressureInsideOverlapMetres = Mathf.Clamp(
                    Mathf.Max(0.08f, cellSizeX * 0.35f),
                    0.08f,
                    0.16f);
                float pressureInsideOverlapPixels =
                    pressureInsideOverlapMetres /
                    Mathf.Max(0.001f, cellSizeX);
                float crestInsetPixels = sampleCount >= 64
                    ? 1.50f
                    : sampleCount >= 32
                        ? 1.00f
                        : 0.75f;
                float minimumInsideOverlapPixels = sampleCount >= 64
                    ? 3.5f
                    : sampleCount >= 32
                        ? 2.5f
                        : 1.5f;
                float requestedInsideOverlapPixels = Mathf.Max(
                    minimumInsideOverlapPixels,
                    pressureInsideOverlapPixels);
                List<float> validRowThicknesses = new();

                float baseMinimum = float.PositiveInfinity;
                float baseMaximum = float.NegativeInfinity;
                float currentMinimum = float.PositiveInfinity;
                float currentMaximum = float.NegativeInfinity;
                float ceilingMinimum = float.PositiveInfinity;
                float ceilingMaximum = float.NegativeInfinity;
                float multiplierMinimum = float.PositiveInfinity;
                float multiplierMaximum = float.NegativeInfinity;
                float interiorBaseMinimum = float.PositiveInfinity;
                float interiorBaseMaximum = float.NegativeInfinity;
                float interiorCeilingMinimum = float.PositiveInfinity;
                float interiorCeilingMaximum = float.NegativeInfinity;
                float maximumAdjacentBaseHeightDifference = 0f;
                float maximumAdjacentCurrentHeightDifference = 0f;
                float maximumAdjacentBaseContactShift = 0f;
                float maximumAdjacentCurrentContactShift = 0f;
                float previousBaseHeight = 0f;
                float previousCurrentHeight = 0f;
                float previousBaseContact = 0f;
                float previousCurrentContact = 0f;
                bool hasPreviousValidRow = false;
                int validRowCount = 0;
                int supportLimitedBelowTargetRowCount = 0;
                int endpointTaperRowCount = 0;
                int targetHeightRowCount = 0;
                float rowThicknessMinimum = float.PositiveInfinity;
                float rowThicknessMaximum = float.NegativeInfinity;
                float maximumResolvedCrestDepthPercent = 0f;
                float maximumResolvedPressureEndDepthPercent = 0f;
                int geometryClampedRowCount = 0;
                int protectedDownstreamRegionViolationRowCount = 0;

                for (int row = 0; row < sampleCount; row++)
                {
                    Vector4 baseSample = baseProfile.Samples[row];
                    Vector4 currentSample = currentProfile.Samples[row];
                    if (baseSample.z <= 0.0001f ||
                        baseSample.w <= 0.0001f)
                    {
                        hasPreviousValidRow = false;
                        continue;
                    }

                    validRowCount++;
                    baseMinimum = Mathf.Min(baseMinimum, baseSample.z);
                    baseMaximum = Mathf.Max(baseMaximum, baseSample.z);
                    currentMinimum = Mathf.Min(
                        currentMinimum,
                        currentSample.z);
                    currentMaximum = Mathf.Max(
                        currentMaximum,
                        currentSample.z);
                    ceilingMinimum = Mathf.Min(
                        ceilingMinimum,
                        baseSample.w);
                    ceilingMaximum = Mathf.Max(
                        ceilingMaximum,
                        baseSample.w);

                    float row01 = sampleCount > 1
                        ? row / (float)(sampleCount - 1)
                        : 0.5f;
                    float lateral01 = Mathf.Abs(row01 * 2f - 1f);
                    float endpointTaper = 1f - Mathf.SmoothStep(
                        0f,
                        1f,
                        Mathf.InverseLerp(0.82f, 1f, lateral01));
                    if (endpointTaper < 0.999f)
                    {
                        endpointTaperRowCount++;
                    }

                    float untaperedBaseHeight = endpointTaper > 0.0001f
                        ? baseSample.z / endpointTaper
                        : 0f;
                    float untaperedCeilingHeight = endpointTaper > 0.0001f
                        ? baseSample.w / endpointTaper
                        : 0f;

                    if (lateral01 <= 0.82f)
                    {
                        interiorBaseMinimum = Mathf.Min(
                            interiorBaseMinimum,
                            baseSample.z);
                        interiorBaseMaximum = Mathf.Max(
                            interiorBaseMaximum,
                            baseSample.z);
                        interiorCeilingMinimum = Mathf.Min(
                            interiorCeilingMinimum,
                            baseSample.w);
                        interiorCeilingMaximum = Mathf.Max(
                            interiorCeilingMaximum,
                            baseSample.w);
                    }

                    if (endpointTaper > 0.0001f &&
                        untaperedCeilingHeight <
                            targetHeight - 0.0005f)
                    {
                        supportLimitedBelowTargetRowCount++;
                    }

                    if (endpointTaper > 0.0001f &&
                        untaperedBaseHeight >= targetHeight - 0.0005f)
                    {
                        targetHeightRowCount++;
                    }

                    float multiplier = 1f;
                    if (continuousSource.
                            StaticPressureCurrentMultipliers != null &&
                        continuousSource.
                            StaticPressureCurrentMultipliers.Length ==
                            sampleCount)
                    {
                        multiplier = continuousSource.
                            StaticPressureCurrentMultipliers[row];
                    }
                    else if (baseSample.z > 0.0001f)
                    {
                        multiplier = currentSample.z / baseSample.z;
                    }

                    multiplierMinimum = Mathf.Min(
                        multiplierMinimum,
                        multiplier);
                    multiplierMaximum = Mathf.Max(
                        multiplierMaximum,
                        multiplier);

                    float baseContact =
                        baseSample.x + baseSample.y * baseSample.z;
                    float currentContact =
                        currentSample.x +
                        currentSample.y * currentSample.z;

                    float downstreamBoundary =
                        baseProfile.DownstreamBoundaries[row];
                    float rowThickness =
                        downstreamBoundary - baseSample.x;
                    if (rowThickness > 0.005f)
                    {
                        validRowThicknesses.Add(rowThickness);
                        rowThicknessMinimum = Mathf.Min(
                            rowThicknessMinimum,
                            rowThickness);
                        rowThicknessMaximum = Mathf.Max(
                            rowThicknessMaximum,
                            rowThickness);

                        float protectedDownstreamStart = Mathf.Lerp(
                            baseSample.x,
                            downstreamBoundary,
                            protectedDownstreamStartFraction);
                        float requestedCrest = baseSample.x +
                            Mathf.Max(
                                0f,
                                currentSample.y * currentSample.z) +
                            crestInsetPixels * cellSizeX;
                        float resolvedCrest = Mathf.Min(
                            requestedCrest,
                            protectedDownstreamStart);
                        float requestedPressureEnd = resolvedCrest +
                            (requestedInsideOverlapPixels +
                             insideGateDownstreamTailPixels) * cellSizeX;
                        float resolvedPressureEnd = Mathf.Min(
                            requestedPressureEnd,
                            protectedDownstreamStart);

                        if (requestedCrest >
                                protectedDownstreamStart + 0.0001f ||
                            requestedPressureEnd >
                                protectedDownstreamStart + 0.0001f)
                        {
                            geometryClampedRowCount++;
                        }

                        if (resolvedPressureEnd >
                            protectedDownstreamStart + 0.0001f)
                        {
                            protectedDownstreamRegionViolationRowCount++;
                        }

                        maximumResolvedCrestDepthPercent = Mathf.Max(
                            maximumResolvedCrestDepthPercent,
                            Mathf.Clamp01(
                                (resolvedCrest - baseSample.x) /
                                rowThickness) * 100f);
                        maximumResolvedPressureEndDepthPercent = Mathf.Max(
                            maximumResolvedPressureEndDepthPercent,
                            Mathf.Clamp01(
                                (resolvedPressureEnd - baseSample.x) /
                                rowThickness) * 100f);
                    }

                    if (hasPreviousValidRow)
                    {
                        maximumAdjacentBaseHeightDifference = Mathf.Max(
                            maximumAdjacentBaseHeightDifference,
                            Mathf.Abs(baseSample.z - previousBaseHeight));
                        maximumAdjacentCurrentHeightDifference = Mathf.Max(
                            maximumAdjacentCurrentHeightDifference,
                            Mathf.Abs(
                                currentSample.z -
                                previousCurrentHeight));
                        maximumAdjacentBaseContactShift = Mathf.Max(
                            maximumAdjacentBaseContactShift,
                            Mathf.Abs(baseContact - previousBaseContact));
                        maximumAdjacentCurrentContactShift = Mathf.Max(
                            maximumAdjacentCurrentContactShift,
                            Mathf.Abs(
                                currentContact -
                                previousCurrentContact));
                    }

                    previousBaseHeight = baseSample.z;
                    previousCurrentHeight = currentSample.z;
                    previousBaseContact = baseContact;
                    previousCurrentContact = currentContact;
                    hasPreviousValidRow = true;
                }

                if (validRowCount == 0)
                {
                    return false;
                }

                if (float.IsInfinity(interiorBaseMinimum))
                {
                    interiorBaseMinimum = baseMinimum;
                    interiorBaseMaximum = baseMaximum;
                }

                if (float.IsInfinity(interiorCeilingMinimum))
                {
                    interiorCeilingMinimum = ceilingMinimum;
                    interiorCeilingMaximum = ceilingMaximum;
                }

                int lateralFieldResolution = runtime.river.Quality switch
                {
                    StylizedRiverQuality.Low => 32,
                    StylizedRiverQuality.Medium => 48,
                    StylizedRiverQuality.High => 64,
                    _ => 48
                };
                float requestedProfileWidthPixels =
                    Mathf.Max(
                        0.10f,
                        continuousSource.
                            StaticPressureAcrossHalfWidth * 2f) /
                    Mathf.Max(0.10f, diagnostics.LocalRiverWidth) *
                    lateralFieldResolution;

                if (validRowThicknesses.Count == 0)
                {
                    return false;
                }

                validRowThicknesses.Sort();
                int middleIndex = validRowThicknesses.Count / 2;
                float medianRowThickness =
                    validRowThicknesses.Count % 2 == 0
                        ? (validRowThicknesses[middleIndex - 1] +
                           validRowThicknesses[middleIndex]) * 0.5f
                        : validRowThicknesses[middleIndex];

                debugData =
                    new GeneratedRiverPressureProfileDebugData(
                        runtime.river,
                        continuousSource.WorldPosition,
                        continuousSource.
                            StaticPressureAcrossHalfWidth,
                        requestedProfileWidthPixels,
                        sampleCount,
                        RiverDisturbanceFootprintResolver.
                            PressureSupportHeightSlices,
                        diagnostics.SupportInspectionHeight,
                        targetHeight,
                        MaximumStaticPressureModulation,
                        validRowCount,
                        supportLimitedBelowTargetRowCount,
                        endpointTaperRowCount,
                        targetHeightRowCount,
                        new Vector2(baseMinimum, baseMaximum),
                        new Vector2(currentMinimum, currentMaximum),
                        new Vector2(ceilingMinimum, ceilingMaximum),
                        new Vector2(
                            multiplierMinimum,
                            multiplierMaximum),
                        new Vector2(
                            interiorBaseMinimum,
                            interiorBaseMaximum),
                        new Vector2(
                            interiorCeilingMinimum,
                            interiorCeilingMaximum),
                        maximumAdjacentBaseHeightDifference,
                        maximumAdjacentCurrentHeightDifference,
                        maximumAdjacentBaseContactShift,
                        maximumAdjacentCurrentContactShift,
                        new Vector2(
                            rowThicknessMinimum,
                            rowThicknessMaximum),
                        medianRowThickness,
                        maximumResolvedCrestDepthPercent,
                        maximumResolvedPressureEndDepthPercent,
                        geometryClampedRowCount,
                        protectedDownstreamRegionViolationRowCount,
                        protectedDownstreamStartFraction * 100f,
                        appliedMultiplierBounds,
                        baseProfile.Samples,
                        currentProfile.Samples,
                        baseProfile.DownstreamBoundaries);
                return debugData.IsValid;
            }

            return false;
        }

#endif
    }
}
