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
            paintProgressiveBirthSourceSegmentKernel =
                computeShader.FindKernel("PaintProgressiveBirthSourceSegment");
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
            transportPredictorKernel =
                computeShader.FindKernel("TransportPredictor");
            transportCorrectorKernel =
                computeShader.FindKernel("TransportCorrector");
            compressTransportInterfaceKernel =
                computeShader.FindKernel("CompressTransportInterface");
            phaseCommitKernel = computeShader.FindKernel("CommitPhaseTransport");
            simulateKernel = computeShader.FindKernel("SimulateFoam");
            applyBoundaryKernel = computeShader.FindKernel("ApplyBoundary");
        }

        private void ConfigureSharedComputeParameters(float deltaTime)
        {
            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
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
            computeShader.SetFloat(
                "_FoamTransportMaximumAxisCourant",
                TransportMaximumAxisCourant);
            computeShader.SetVector(
                "_FoamWakeMotionInfluence",
                WakeMotionInfluence);
            computeShader.SetVector(
                "_FoamPressureMotionInfluence",
                PressureMotionInfluence);

            bool disturbanceAvailable =
                disturbanceRuntime != null &&
                disturbanceRuntime.IsAllocated;

            RenderTexture wakeSource = disturbanceAvailable
                ? disturbanceRuntime.CurrentWakeTexture
                : null;
            RenderTexture staticWakeSource = disturbanceAvailable
                ? disturbanceRuntime.StaticWakeSourceTexture
                : null;
            RenderTexture staticPressureSource = disturbanceAvailable
                ? disturbanceRuntime.StaticPressureTexture
                : null;

            bool wakeAvailable = IsCreatedTexture(wakeSource);
            bool staticWakeAvailable = IsCreatedTexture(staticWakeSource);
            bool staticPressureAvailable =
                IsCreatedTexture(staticPressureSource);

            Texture wakeTexture = wakeAvailable
                ? wakeSource
                : neutralDisturbanceTexture;
            Texture staticWakeTexture = staticWakeAvailable
                ? staticWakeSource
                : neutralDisturbanceTexture;
            Texture staticPressureTexture = staticPressureAvailable
                ? staticPressureSource
                : neutralDisturbanceTexture;

            Vector2Int wakeDimensions = wakeAvailable
                ? new Vector2Int(wakeSource.width, wakeSource.height)
                : Vector2Int.one;
            Vector2Int staticWakeDimensions = staticWakeAvailable
                ? new Vector2Int(
                    staticWakeSource.width,
                    staticWakeSource.height)
                : Vector2Int.one;
            Vector2Int staticPressureDimensions = staticPressureAvailable
                ? new Vector2Int(
                    staticPressureSource.width,
                    staticPressureSource.height)
                : Vector2Int.one;

            computeShader.SetFloat(
                "_FoamDisturbanceEnabled",
                disturbanceAvailable ? 1f : 0f);
            computeShader.SetInts(
                "_FoamWakeDimensions",
                wakeDimensions.x,
                wakeDimensions.y);
            computeShader.SetInts(
                "_FoamStaticWakeDimensions",
                staticWakeDimensions.x,
                staticWakeDimensions.y);
            computeShader.SetInts(
                "_FoamStaticPressureDimensions",
                staticPressureDimensions.x,
                staticPressureDimensions.y);

            BindTransportKernel(
                transportPredictorKernel,
                wakeTexture,
                staticWakeTexture,
                staticPressureTexture);
            BindTransportKernel(
                transportCorrectorKernel,
                wakeTexture,
                staticWakeTexture,
                staticPressureTexture);

            computeShader.SetTexture(
                transportPredictorKernel,
                "_FoamStateRead",
                currentState);
            computeShader.SetTexture(
                transportPredictorKernel,
                "_FoamTransportWrite",
                transportPredictorState);

            computeShader.SetTexture(
                transportCorrectorKernel,
                "_FoamStateRead",
                currentState);
            computeShader.SetTexture(
                transportCorrectorKernel,
                "_FoamTransportPredictorRead",
                transportPredictorState);
            computeShader.SetTexture(
                transportCorrectorKernel,
                "_FoamTransportWrite",
                transportCorrectedState);

            BindCompressionKernel();

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
                "_FoamTransportCorrectedRead",
                transportCorrectedState);
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
                "_FoamProgressiveBirthSourceRead",
                progressiveBirthSourceTexture);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamBirthTransferDebugWrite",
                progressiveBirthTransferDebugTexture);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamStateWrite",
                writeState);
        }

        private void BindTransportKernel(
            int kernel,
            Texture wakeTexture,
            Texture staticWakeTexture,
            Texture staticPressureTexture)
        {
            computeShader.SetBuffer(
                kernel,
                "_FoamMetricRows",
                metricBuffer);
            computeShader.SetTexture(
                kernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamObstacleExclusionRead",
                obstacleExclusionTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamWakeField",
                wakeTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamStaticWakeField",
                staticWakeTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamStaticPressureField",
                staticPressureTexture);
        }

        private void BindCompressionKernel()
        {
            computeShader.SetBuffer(
                compressTransportInterfaceKernel,
                "_FoamMetricRows",
                metricBuffer);
            computeShader.SetTexture(
                compressTransportInterfaceKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetTexture(
                compressTransportInterfaceKernel,
                "_FoamObstacleExclusionRead",
                obstacleExclusionTexture);
        }

        private void DispatchSimulation(int startX, int countX)
        {
            computeShader.SetInt("_FoamRangeStart", startX);
            computeShader.SetInt("_FoamRangeCount", countX);

            // SSP-RK2 conservative finite-volume transport now owns only the
            // retained disturbance-derived material motion. Patch 4.11C.5.2d
            // moved base downstream travel to phase transport plus integer
            // commits, so this path no longer spends material cadence on tiny
            // sub-cell downstream shifts.
            Dispatch(transportPredictorKernel, countX, fieldHeight);
            Dispatch(transportCorrectorKernel, countX, fieldHeight);
            DispatchInterfaceCompressionPasses(countX);
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

        private void DispatchInterfaceCompressionPasses(int countX)
        {
            // Conservative pairwise interface compression runs after transport
            // and before lifecycle/source transfer. It moves packed material
            // moments from lower-Presence cells toward adjacent higher-Presence
            // cells without changing total Presence, Remaining-Life moment, or
            // Pattern moment for each disjoint pair. This restores a resolved
            // visible contour to small proof sources without reintroducing the
            // deleted autonomous network or inventing material area.
            DispatchInterfaceCompressionPass(
                axis: 0,
                parity: 0,
                read: transportCorrectedState,
                write: transportPredictorState,
                width: countX,
                height: fieldHeight);
            DispatchInterfaceCompressionPass(
                axis: 0,
                parity: 1,
                read: transportPredictorState,
                write: transportCorrectedState,
                width: countX,
                height: fieldHeight);
            DispatchInterfaceCompressionPass(
                axis: 1,
                parity: 0,
                read: transportCorrectedState,
                write: transportPredictorState,
                width: countX,
                height: fieldHeight);
            DispatchInterfaceCompressionPass(
                axis: 1,
                parity: 1,
                read: transportPredictorState,
                write: transportCorrectedState,
                width: countX,
                height: fieldHeight);
        }

        private void DispatchInterfaceCompressionPass(
            int axis,
            int parity,
            RenderTexture read,
            RenderTexture write,
            int width,
            int height)
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            computeShader.SetInt("_FoamCompressionAxis", axis);
            computeShader.SetInt("_FoamCompressionParity", parity);
            computeShader.SetTexture(
                compressTransportInterfaceKernel,
                "_FoamCompressionStateRead",
                read);
            computeShader.SetTexture(
                compressTransportInterfaceKernel,
                "_FoamCompressionStateWrite",
                write);
            lastCompressionPassesUsed++;
            Dispatch(compressTransportInterfaceKernel, width, height);
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

        private void ClearChunk(int chunk)
        {
            if (stateA == null || stateB == null ||
                chunk < 0 || chunk >= chunkCount)
            {
                return;
            }

            int startX = chunk * resolutionPerChunk;
            int countX = Mathf.Min(resolutionPerChunk, fieldWidth - startX);
            DispatchClear(stateA, startX, countX);
            DispatchClear(stateB, startX, countX);
            DispatchClear(transportPredictorState, startX, countX);
            DispatchClear(transportCorrectedState, startX, countX);
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
