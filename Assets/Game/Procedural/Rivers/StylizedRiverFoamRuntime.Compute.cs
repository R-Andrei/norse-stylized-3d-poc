using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private void ResolveComputeKernels()
        {
            clearKernel = computeShader.FindKernel("ClearRange");
            injectKernel = computeShader.FindKernel("InjectFoam");
            clearProgressiveBirthDebugAllKernel =
                computeShader.FindKernel("ClearProgressiveBirthDebugAll");
            clearProgressiveBirthDebugTransientKernel =
                computeShader.FindKernel("ClearProgressiveBirthDebugTransient");
            paintProgressiveBirthDebugSegmentKernel =
                computeShader.FindKernel("PaintProgressiveBirthDebugSegment");
            buildCurrentShoreEdgesKernel =
                computeShader.FindKernel("BuildCurrentShoreEdges");
            composeTopologyKernel =
                computeShader.FindKernel("ComposeTopology");
            captureGeneratedTopologyKernel =
                computeShader.FindKernel("CaptureGeneratedTopology");
            buildEvolvingMajorSupportKernel =
                computeShader.FindKernel("BuildEvolvingMajorSupport");
            clearObstacleExclusionKernel =
                computeShader.FindKernel("ClearObstacleExclusion");
            updateObstacleExclusionKernel =
                computeShader.FindKernel("UpdateObstacleExclusion");
            resetTopologyMetricsKernel =
                computeShader.FindKernel("ResetTopologyMetrics");
            measureTopologyMetricsKernel =
                computeShader.FindKernel("MeasureTopologyMetrics");
            phaseCommitKernel = computeShader.FindKernel("CommitPhaseTransport");
            simulateKernel = computeShader.FindKernel("SimulateFoam");
            applyBoundaryKernel = computeShader.FindKernel("ApplyBoundary");
        }

        private void ConfigureSharedComputeParameters(float deltaTime)
        {
            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetInts(
                "_FoamTopologyDimensions",
                structuralWidth,
                structuralHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
            computeShader.SetFloat("_FoamDeltaTime", deltaTime);
            computeShader.SetFloat(
                "_FoamPhaseTransportMetres",
                foamPhaseTransportMetres);
            computeShader.SetFloat(
                "_FoamGlobalStart",
                river.Domain.GlobalDistanceMinimum);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat(
                "_FoamFlowSpeed",
                river.FlowSpeedMetresPerSecond *
                river.LiquidFactor *
                river.FoamMaterialFlowSpeedMultiplier *
                river.FlowDirection);
            computeShader.SetFloat(
                "_FoamFlowDirection",
                river.FlowDirection);
            computeShader.SetFloat(
                "_FoamNeutralLifetime",
                river.FoamNeutralLifetime);
            computeShader.SetFloat(
                "_FoamPositiveAgeMultiplier",
                river.FoamSupportedAgingRate);
            computeShader.SetFloat(
                "_FoamNegativeAgeMultiplier",
                river.FoamNegativeAgingRate);
            computeShader.SetFloat("_FoamTime", river.MotionTime);
            computeShader.SetFloat("_FoamSeed", river.VisualSeed);
            computeShader.SetFloat(
                "_FoamPresenceMetricThreshold",
                PresenceMetricThreshold);

            computeShader.SetBuffer(
                simulateKernel,
                "_FoamMetricRows",
                metricBuffer);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamStateRead",
                currentState);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamTopologyRead",
                topologyTexture);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamTopologySourcesRead",
                topologySourcesTexture);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamObstacleExclusionRead",
                obstacleExclusionTexture);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamStateWrite",
                writeState);
        }

        private void DispatchSimulation(int startX, int countX)
        {
            computeShader.SetInt("_FoamRangeStart", startX);
            computeShader.SetInt("_FoamRangeCount", countX);

            // Patch 4.11C.5.4c removes the old predictor/corrector and
            // compression transport path. Persistent material now follows one
            // direct lifecycle pass across the requested range; base downstream
            // movement is handled only by phase transport and integer commits.
            Dispatch(simulateKernel, countX, fieldHeight);
        }

        private bool DispatchPhaseCommit(int committedCells)
        {
            if (computeShader == null || currentState == null ||
                writeState == null || boundaryTexture == null ||
                obstacleExclusionTexture == null || phaseCommitKernel < 0 ||
                fieldWidth <= 0 || fieldHeight <= 0 || committedCells == 0)
            {
                return false;
            }

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
            computeShader.SetInt("_FoamRangeStart", 0);
            computeShader.SetInt("_FoamRangeCount", fieldWidth);
            computeShader.SetInt("_FoamPhaseCommitCells", committedCells);
            computeShader.SetFloat(
                "_FoamPhaseTransportMetres",
                foamPhaseTransportMetres);
            computeShader.SetTexture(
                phaseCommitKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetTexture(
                phaseCommitKernel,
                "_FoamObstacleExclusionRead",
                obstacleExclusionTexture);
            computeShader.SetTexture(
                phaseCommitKernel,
                "_FoamStateRead",
                currentState);
            computeShader.SetTexture(
                phaseCommitKernel,
                "_FoamStateWrite",
                writeState);

            previousState = currentState;
            Dispatch(phaseCommitKernel, fieldWidth, fieldHeight);
            (currentState, writeState) = (writeState, currentState);
            return true;
        }

        private void DispatchClear(RenderTexture target, int startX, int countX)
        {
            if (computeShader == null || target == null ||
                clearKernel < 0 || countX <= 0)
            {
                return;
            }

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
            computeShader.SetInt("_FoamRangeStart", startX);
            computeShader.SetInt("_FoamRangeCount", countX);
            computeShader.SetTexture(clearKernel, "_FoamStateWrite", target);
            Dispatch(clearKernel, countX, fieldHeight);
        }

        private void ApplyBoundaryToState(RenderTexture target)
        {
            if (computeShader == null || target == null ||
                boundaryTexture == null || obstacleExclusionTexture == null ||
                applyBoundaryKernel < 0 || fieldWidth <= 0 || fieldHeight <= 0)
            {
                return;
            }

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
            computeShader.SetInt("_FoamRangeStart", 0);
            computeShader.SetInt("_FoamRangeCount", fieldWidth);
            computeShader.SetTexture(
                applyBoundaryKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetTexture(
                applyBoundaryKernel,
                "_FoamObstacleExclusionRead",
                obstacleExclusionTexture);
            computeShader.SetTexture(
                applyBoundaryKernel,
                "_FoamStateWrite",
                target);
            Dispatch(applyBoundaryKernel, fieldWidth, fieldHeight);
        }

        private void Dispatch(int kernel, int width, int height)
        {
            int groupsX = Mathf.CeilToInt(width / (float)ThreadGroupSize);
            int groupsY = Mathf.CeilToInt(height / (float)ThreadGroupSize);
            computeShader.Dispatch(kernel, groupsX, groupsY, 1);
            lastUpdateDispatches++;
            lastUpdateCellIterations += (long)width * height;
        }

        private void DispatchOneDimensional(
            int kernel,
            int count,
            int threadsPerGroup)
        {
            if (count <= 0)
            {
                return;
            }

            int groups = Mathf.CeilToInt(
                count / (float)Mathf.Max(1, threadsPerGroup));
            computeShader.Dispatch(kernel, groups, 1, 1);
            lastUpdateDispatches++;
            lastUpdateCellIterations += count;
        }
    }
}
