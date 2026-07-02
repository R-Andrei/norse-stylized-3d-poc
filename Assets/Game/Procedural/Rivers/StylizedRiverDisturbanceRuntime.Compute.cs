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
        private void SetValidDomainComputeParameters()
        {
            if (computeShader == null)
            {
                return;
            }

            computeShader.SetInt("_ValidFieldWidth", validFieldWidth);
            computeShader.SetInt("_ValidWakeWidth", validWakeFieldWidth);
        }

        private void DispatchRippleActiveRanges()
        {
            int groupStart = -1;
            for (int chunk = 0; chunk <= chunkCount; chunk++)
            {
                bool active = chunk < chunkCount && chunkActive[chunk];
                if (active && groupStart < 0)
                {
                    groupStart = chunk;
                }

                if (active || groupStart < 0)
                {
                    continue;
                }

                int groupCount = chunk - groupStart;
                int xOffset = groupStart * resolutionPerChunk;
                int width = groupCount * resolutionPerChunk;
                computeShader.SetInt("_DispatchXOffset", xOffset);
                computeShader.SetInt("_DispatchWidth", width);
                DispatchCompute(
                    simulateRippleKernel,
                    Mathf.CeilToInt(width / (float)ThreadGroupSize),
                    Mathf.CeilToInt(fieldHeight / (float)ThreadGroupSize),
                    1,
                    PerformanceDispatchCategory.RippleSimulation,
                    width,
                    fieldHeight);
                groupStart = -1;
            }
        }

        private void DispatchWakeActiveRanges()
        {
            int groupStart = -1;
            for (int chunk = 0; chunk <= chunkCount; chunk++)
            {
                bool active = chunk < chunkCount && wakeChunkActive[chunk];
                if (active && groupStart < 0)
                {
                    groupStart = chunk;
                }

                if (active || groupStart < 0)
                {
                    continue;
                }

                int groupCount = chunk - groupStart;
                int xOffset = groupStart * wakeResolutionPerChunk;
                int width = groupCount * wakeResolutionPerChunk;
                computeShader.SetInt("_WakeDispatchXOffset", xOffset);
                computeShader.SetInt("_WakeDispatchWidth", width);
                DispatchCompute(
                    simulateWakeKernel,
                    Mathf.CeilToInt(width / (float)ThreadGroupSize),
                    Mathf.CeilToInt(wakeFieldHeight / (float)ThreadGroupSize),
                    1,
                    PerformanceDispatchCategory.WakeSimulation,
                    width,
                    wakeFieldHeight);
                groupStart = -1;
            }
        }

        private void DispatchCompute(
            int kernel,
            int groupCountX,
            int groupCountY,
            int groupCountZ,
            PerformanceDispatchCategory category,
            int processedWidth,
            int processedHeight)
        {
            computeShader.Dispatch(
                kernel,
                groupCountX,
                groupCountY,
                groupCountZ);

            lastUpdateComputeDispatchCount++;
            long threadGroups =
                (long)Mathf.Max(0, groupCountX) *
                Mathf.Max(0, groupCountY) *
                Mathf.Max(0, groupCountZ);
            long cellIterations =
                (long)Mathf.Max(0, processedWidth) *
                Mathf.Max(0, processedHeight);
            lastUpdateThreadGroupCount += threadGroups;
            lastUpdateCellIterationCount += cellIterations;

            switch (category)
            {
                case PerformanceDispatchCategory.RippleSimulation:
                    lastUpdateRippleSimulationDispatchCount++;
                    break;
                case PerformanceDispatchCategory.WakeSimulation:
                    lastUpdateWakeSimulationDispatchCount++;
                    break;
                case PerformanceDispatchCategory.ImpactInjection:
                    lastUpdateImpactInjectionDispatchCount++;
                    break;
                case PerformanceDispatchCategory.WakeInjection:
                    lastUpdateWakeInjectionDispatchCount++;
                    break;
                case PerformanceDispatchCategory.StaticPressureBake:
                    lastUpdateStaticPressureBakeDispatchCount++;
                    break;
                case PerformanceDispatchCategory.StaticWakeBake:
                    lastUpdateStaticWakeBakeDispatchCount++;
                    break;
                case PerformanceDispatchCategory.RippleBoundaryBake:
                    lastUpdateRippleBoundaryBakeDispatchCount++;
                    break;
                case PerformanceDispatchCategory.Clear:
                    lastUpdateClearDispatchCount++;
                    break;
            }

            recentPeakComputeDispatchCount = Mathf.Max(
                recentPeakComputeDispatchCount,
                lastUpdateComputeDispatchCount);
            recentPeakThreadGroupCount = Math.Max(
                recentPeakThreadGroupCount,
                lastUpdateThreadGroupCount);
            recentPeakCellIterationCount = Math.Max(
                recentPeakCellIterationCount,
                lastUpdateCellIterationCount);
        }

        private void DispatchClear(
            RenderTexture texture,
            int textureWidth,
            int textureHeight,
            int xOffset,
            int width)
        {
            if (texture == null || computeShader == null || clearKernel < 0)
            {
                return;
            }

            int safeOffset = Mathf.Clamp(xOffset, 0, Mathf.Max(0, textureWidth - 1));
            int safeWidth = Mathf.Clamp(width, 0, textureWidth - safeOffset);
            if (safeWidth <= 0)
            {
                return;
            }

            computeShader.SetInts("_FieldSize", textureWidth, textureHeight);
            computeShader.SetInt("_DispatchXOffset", safeOffset);
            computeShader.SetInt("_DispatchWidth", safeWidth);
            computeShader.SetTexture(clearKernel, "_StateWrite", texture);
            DispatchCompute(
                clearKernel,
                Mathf.CeilToInt(safeWidth / (float)ThreadGroupSize),
                Mathf.CeilToInt(textureHeight / (float)ThreadGroupSize),
                1,
                PerformanceDispatchCategory.Clear,
                safeWidth,
                textureHeight);
        }
    }
}
