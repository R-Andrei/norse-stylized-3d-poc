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
        private void BuildGuidanceField(float deltaTime)
        {
            using var profilerScope = InitBuildGuidanceProfilerMarker.Auto();
            if (computeShader == null || guidanceTexture == null ||
                buildGuidanceKernel < 0)
            {
                return;
            }

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
            computeShader.SetInts(
                "_FoamGuidanceDimensions",
                guidanceWidth,
                guidanceHeight);
            computeShader.SetInt("_FoamChunkCount", chunkCount);
            computeShader.SetFloat(
                "_FoamGlobalStart",
                allocatedGlobalStart);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat("_FoamDeltaTime", deltaTime);
            computeShader.SetFloat(
                "_FoamTime",
                ResolveInitializationMotionTime());
            computeShader.SetFloat("_FoamSeed", river.VisualSeed);
            computeShader.SetFloat(
                "_FoamEvolution",
                ProvisionalMaterialEvolution);
            computeShader.SetBuffer(
                buildGuidanceKernel,
                "_FoamMetricRows",
                metricBuffer);
            computeShader.SetTexture(
                buildGuidanceKernel,
                "_FoamGuidanceWrite",
                guidanceTexture);
            Dispatch(buildGuidanceKernel, guidanceWidth, guidanceHeight);
        }
    }
}
