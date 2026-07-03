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
            buildGuidanceKernel = computeShader.FindKernel("BuildGuidance");
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
            advectForwardKernel =
                computeShader.FindKernel("AdvectForward");
            advectReverseKernel =
                computeShader.FindKernel("AdvectReverse");
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
            computeShader.SetInts(
                "_FoamGuidanceDimensions",
                guidanceWidth,
                guidanceHeight);
            computeShader.SetInt(
                "_FoamResolutionPerChunk",
                resolutionPerChunk);
            computeShader.SetInt("_FoamChunkCount", chunkCount);
            computeShader.SetFloat("_FoamDeltaTime", deltaTime);
            computeShader.SetFloat(
                "_FoamGlobalStart",
                river.Domain.GlobalDistanceMinimum);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat(
                "_FoamFlowSpeed",
                river.FlowSpeedMetresPerSecond *
                ProvisionalMaterialFlowFollow *
                river.LiquidFactor);
            // Pressure Support builds its fail-closed upstream support
            // envelope from the exact obstacle mask. Flow direction selects
            // which side of each solid cell is the object-facing front on both
            // normal-flow and reverse-flow rivers.
            computeShader.SetFloat(
                "_FoamFlowDirection",
                river.FlowDirection);
            computeShader.SetFloat(
                "_FoamEvolution",
                ProvisionalMaterialEvolution);
            computeShader.SetFloat("_FoamSpread", ProvisionalMaterialSpread);
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
                "_FoamGuidanceStrength",
                ProvisionalMaterialGuidanceStrength);
            computeShader.SetFloat(
                "_FoamBoundaryAttraction",
                ProvisionalMaterialBoundaryAttraction);
            computeShader.SetFloat(
                "_FoamWakeMotionInfluence",
                ProvisionalWakeMotionInfluence);
            computeShader.SetFloat(
                "_FoamImpactMotionInfluence",
                ProvisionalImpactMotionInfluence);

            bool disturbanceAvailable =
                disturbanceRuntime != null &&
                disturbanceRuntime.IsAllocated;

            RenderTexture wakeSource = disturbanceAvailable
                ? disturbanceRuntime.CurrentWakeTexture
                : null;
            RenderTexture rippleSource = disturbanceAvailable
                ? disturbanceRuntime.CurrentRippleTexture
                : null;
            RenderTexture staticWakeSource = disturbanceAvailable
                ? disturbanceRuntime.StaticWakeSourceTexture
                : null;
            RenderTexture staticPressureSource = disturbanceAvailable
                ? disturbanceRuntime.StaticPressureTexture
                : null;

            bool wakeAvailable = IsCreatedTexture(wakeSource);
            bool rippleAvailable = IsCreatedTexture(rippleSource);
            bool staticWakeAvailable = IsCreatedTexture(staticWakeSource);
            bool staticPressureAvailable =
                IsCreatedTexture(staticPressureSource);

            // Every texture declared by a compute kernel must be bound for
            // every dispatch. Stage 5 allocates its optional fields
            // independently, so an allocated disturbance runtime does not
            // guarantee that each individual texture is already created.
            // Bind one explicit zero-valued RenderTexture for every missing
            // input rather than relying on a built-in Texture2D fallback.
            Texture wakeTexture = wakeAvailable
                ? wakeSource
                : neutralDisturbanceTexture;
            Texture rippleTexture = rippleAvailable
                ? rippleSource
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
            Vector2Int rippleDimensions = rippleAvailable
                ? new Vector2Int(rippleSource.width, rippleSource.height)
                : Vector2Int.one;
            Vector2Int staticWakeDimensions = staticWakeAvailable
                ? new Vector2Int(staticWakeSource.width, staticWakeSource.height)
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
                "_FoamRippleDimensions",
                rippleDimensions.x,
                rippleDimensions.y);
            computeShader.SetInts(
                "_FoamStaticWakeDimensions",
                staticWakeDimensions.x,
                staticWakeDimensions.y);
            computeShader.SetInts(
                "_FoamStaticPressureDimensions",
                staticPressureDimensions.x,
                staticPressureDimensions.y);

            BindMotionKernel(
                advectForwardKernel,
                wakeTexture,
                rippleTexture,
                staticWakeTexture,
                staticPressureTexture);
            BindMotionKernel(
                advectReverseKernel,
                wakeTexture,
                rippleTexture,
                staticWakeTexture,
                staticPressureTexture);
            BindMotionKernel(
                simulateKernel,
                wakeTexture,
                rippleTexture,
                staticWakeTexture,
                staticPressureTexture);

            computeShader.SetTexture(
                advectForwardKernel,
                "_FoamStateRead",
                currentState);
            computeShader.SetTexture(
                advectForwardKernel,
                "_FoamAdvectionWrite",
                advectedState);

            computeShader.SetTexture(
                advectReverseKernel,
                "_FoamAdvectedRead",
                advectedState);
            computeShader.SetTexture(
                advectReverseKernel,
                "_FoamAdvectionWrite",
                reverseState);

            computeShader.SetTexture(
                simulateKernel,
                "_FoamStateRead",
                currentState);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamAdvectedRead",
                advectedState);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamReverseRead",
                reverseState);
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

        private void BindMotionKernel(
            int kernel,
            Texture wakeTexture,
            Texture rippleTexture,
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
                "_FoamGuidanceRead",
                guidanceTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamWakeField",
                wakeTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamRippleField",
                rippleTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamStaticWakeField",
                staticWakeTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamStaticPressureField",
                staticPressureTexture);
        }

        private void DispatchSimulation(int startX, int countX)
        {
            computeShader.SetInt("_FoamRangeStart", startX);
            computeShader.SetInt("_FoamRangeCount", countX);

            // Corrected transport is intentionally a three-dispatch sequence:
            // forward advection, reverse error estimate, then bounded correction
            // plus lifecycle aging, topology response, capture, and reinforcement.
            Dispatch(advectForwardKernel, countX, fieldHeight);
            Dispatch(advectReverseKernel, countX, fieldHeight);
            Dispatch(simulateKernel, countX, fieldHeight);
        }

        private void DispatchClear(RenderTexture target, int startX, int countX)
        {
            if (computeShader == null || target == null || clearKernel < 0 || countX <= 0)
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
                boundaryTexture == null || applyBoundaryKernel < 0 ||
                fieldWidth <= 0 || fieldHeight <= 0)
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
                "_FoamStateWrite",
                target);
            Dispatch(applyBoundaryKernel, fieldWidth, fieldHeight);
        }

        private void ClearChunk(int chunk)
        {
            if (stateA == null || stateB == null || chunk < 0 || chunk >= chunkCount)
            {
                return;
            }

            int startX = chunk * resolutionPerChunk;
            int countX = Mathf.Min(resolutionPerChunk, fieldWidth - startX);
            DispatchClear(stateA, startX, countX);
            DispatchClear(stateB, startX, countX);
            DispatchClear(advectedState, startX, countX);
            DispatchClear(reverseState, startX, countX);

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
