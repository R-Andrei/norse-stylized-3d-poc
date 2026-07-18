using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private void ResolveComputeKernels()
        {
            clearKernel = computeShader.FindKernel("ClearRange");
            injectKernel = computeShader.FindKernel("InjectFoam");
            rasterizeFoamSourceEventKernel =
                computeShader.FindKernel("RasterizeFoamSourceEvent");
            rasterizeFoamSourceEventDebugKernel =
                computeShader.FindKernel("RasterizeFoamSourceEventDebug");
            writeIsolatedLifeProbeKernel =
                computeShader.FindKernel("WriteIsolatedLifeProbe");
            clearAutomaticBirthDebugAllKernel =
                computeShader.FindKernel("ClearAutomaticBirthDebugAll");
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
            buildObjectContactFieldKernel =
                computeShader.FindKernel("BuildFoamObjectContactField");
            resetTopologyMetricsKernel =
                computeShader.FindKernel("ResetTopologyMetrics");
            measureTopologyMetricsKernel =
                computeShader.FindKernel("MeasureTopologyMetrics");
            resetTransportMetricsKernel =
                computeShader.FindKernel("ResetTransportMetrics");
            remapPersistentStateKernel =
                computeShader.FindKernel("RemapPersistentFoamState");
            simulateKernel = computeShader.FindKernel("SimulateFoam");
            buildFilmSourceKernel = computeShader.FindKernel("BuildFoamFilmSource");
            buildFilmSupportKernel = computeShader.FindKernel("BuildFoamFilmSupport");
            advanceVisualOccupancyKernel =
                computeShader.FindKernel("AdvanceFoamVisualOccupancy");
            evaluateShapeKernel = computeShader.FindKernel("EvaluateFoamShape");
            applyBoundaryKernel = computeShader.FindKernel("ApplyBoundary");
        }

        private void ConfigureGridDescriptorComputeParameters()
        {
            if (computeShader == null)
            {
                return;
            }

            computeShader.SetVector(
                FoamGridDescriptorContractId,
                gridDescriptorGpuData.Contract);
            computeShader.SetVector(
                FoamGridDescriptorSpacingId,
                gridDescriptorGpuData.Spacing);
            computeShader.SetVector(
                FoamGridDescriptorLateralId,
                gridDescriptorGpuData.Lateral);
            computeShader.SetVector(
                FoamGridDescriptorLongitudinalId,
                gridDescriptorGpuData.Longitudinal);
            computeShader.SetVector(
                FoamGridDescriptorExtentId,
                gridDescriptorGpuData.Extent);
        }

        private void ConfigureSharedComputeParameters(
            float transportDeltaTime,
            float lifecycleDeltaTime)
        {
            ConfigureGridDescriptorComputeParameters();
            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetInts(
                "_FoamFilmDimensions",
                filmFieldWidth,
                filmFieldHeight);
            computeShader.SetInts(
                "_FoamTopologyDimensions",
                structuralWidth,
                structuralHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
            lastConfiguredFoamDeltaTime = lifecycleDeltaTime;
            lastConfiguredFoamNeutralLifetime = river.FoamNeutralLifetime;
            lastConfiguredFoamPositiveAgeMultiplier =
                river.FoamSupportedAgingRate;
            lastConfiguredFoamNegativeAgeMultiplier =
                river.FoamNegativeAgingRate;
            lastConfiguredAbsoluteLifeProbeActive =
                isolatedLifeProbeAbsoluteAgingActive;

            computeShader.SetFloat(
                "_FoamDeltaTime",
                transportDeltaTime);
            computeShader.SetFloat(
                "_FoamLifecycleDeltaTime",
                lifecycleDeltaTime);
            computeShader.SetFloat(
                "_FoamDebugAbsoluteLifeProbeActive",
                isolatedLifeProbeAbsoluteAgingActive ? 1f : 0f);
            computeShader.SetFloat(
                "_FoamGlobalStart",
                river.Domain.GlobalDistanceMinimum);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat(
                "_FoamFlowDirection",
                river.FlowDirection);
            computeShader.SetFloat(
                "_FoamNeutralLifetime",
                lastConfiguredFoamNeutralLifetime);
            computeShader.SetFloat(
                "_FoamPositiveAgeMultiplier",
                lastConfiguredFoamPositiveAgeMultiplier);
            computeShader.SetFloat(
                "_FoamFullSupportedAgingAt",
                river.FoamFullSupportedAgingAt);
            computeShader.SetFloat(
                "_FoamNegativeAgeMultiplier",
                lastConfiguredFoamNegativeAgeMultiplier);
            computeShader.SetFloat("_FoamTime", river.MotionTime);
            computeShader.SetFloat("_FoamSeed", river.VisualSeed);
            computeShader.SetFloat(
                "_FoamPresenceMetricThreshold",
                PresenceMetricThreshold);
            computeShader.SetFloat(
                "_FoamBaseDownstreamSpeed",
                ResolveBaseFoamDownstreamSpeedMetresPerSecond());
            computeShader.SetFloat(
                "_FoamMaximumLateralSpeedRatio",
                river.FoamMaximumLateralSpeedRatio);
            computeShader.SetFloat(
                "_FoamObstacleSlowdownStrength",
                river.FoamObstacleSlowdownStrength);
            computeShader.SetFloat(
                "_FoamObstacleMinimumDownstreamFactor",
                river.FoamObstacleMinimumDownstreamFactor);
            computeShader.SetFloat(
                "_FoamMotionLaneScrollCells",
                motionLaneScrollCells);
            computeShader.SetFloat(
                "_FoamTransportMetricFixedPointScale",
                TransportMetricFixedPointScale);
            computeShader.SetFloat(
                "_FoamVisualOccupancyBuildTime",
                river.FoamVisualOccupancyBuildTime);
            computeShader.SetFloat(
                "_FoamVisualOccupancyReleaseTime",
                river.FoamVisualOccupancyReleaseTime);

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            bool disturbanceAllocated =
                disturbanceRuntime != null && disturbanceRuntime.IsAllocated;

            RenderTexture rippleSource = disturbanceAllocated
                ? disturbanceRuntime.CurrentRippleTexture
                : null;
            RenderTexture wakeSource = disturbanceAllocated
                ? disturbanceRuntime.CurrentWakeTexture
                : null;
            RenderTexture staticWakeSource = disturbanceAllocated
                ? disturbanceRuntime.StaticWakeSourceTexture
                : null;
            RenderTexture staticPressureSource = disturbanceAllocated
                ? disturbanceRuntime.StaticPressureTexture
                : null;

            bool rippleAvailable = IsCreatedTexture(rippleSource);
            bool wakeAvailable = IsCreatedTexture(wakeSource);
            bool staticWakeAvailable = IsCreatedTexture(staticWakeSource);
            bool staticPressureAvailable = IsCreatedTexture(staticPressureSource);
            Texture neutralSurfaceTexture = neutralDisturbanceTexture != null
                ? (Texture)neutralDisturbanceTexture
                : Texture2D.blackTexture;

            Texture rippleTexture = rippleAvailable
                ? (Texture)rippleSource
                : neutralSurfaceTexture;
            Texture wakeTexture = wakeAvailable
                ? (Texture)wakeSource
                : neutralSurfaceTexture;
            Texture staticWakeTexture = staticWakeAvailable
                ? (Texture)staticWakeSource
                : neutralSurfaceTexture;
            Texture staticPressureTexture = staticPressureAvailable
                ? (Texture)staticPressureSource
                : neutralSurfaceTexture;

            Vector2Int rippleDimensions = rippleAvailable
                ? disturbanceRuntime.RippleTextureDimensions
                : Vector2Int.one;
            Vector2Int wakeDimensions = wakeAvailable
                ? disturbanceRuntime.WakeTextureDimensions
                : Vector2Int.one;
            Vector2Int staticWakeDimensions = staticWakeAvailable
                ? disturbanceRuntime.StaticWakeTextureDimensions
                : Vector2Int.one;
            Vector2Int staticPressureDimensions = staticPressureAvailable
                ? disturbanceRuntime.StaticPressureTextureDimensions
                : Vector2Int.one;

            computeShader.SetInts(
                "_FoamRippleDimensions",
                rippleDimensions.x,
                rippleDimensions.y);
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
            computeShader.SetFloat(
                "_FoamDisturbanceEnabled",
                rippleAvailable || wakeAvailable || staticWakeAvailable ||
                staticPressureAvailable ? 1f : 0f);

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
                "_FoamRippleField",
                rippleTexture);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamWakeField",
                wakeTexture);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamStaticWakeField",
                staticWakeTexture);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamStaticPressureField",
                staticPressureTexture);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamObstacleExclusionRead",
                obstacleExclusionTexture);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamMotionLaneRead",
                motionLaneTexture != null
                    ? (Texture)motionLaneTexture
                    : Texture2D.blackTexture);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamObstacleRoutingRead",
                obstacleRoutingTexture != null
                    ? (Texture)obstacleRoutingTexture
                    : Texture2D.blackTexture);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamStateWrite",
                writeState);
            if (transportMetricsBuffer != null)
            {
                computeShader.SetBuffer(
                    simulateKernel,
                    "_FoamTransportMetrics",
                    transportMetricsBuffer);
            }
        }

        private void ConfigureTransportSubstepDiagnostics(
            bool captureMetrics)
        {
            computeShader.SetInt(
                "_FoamTransportMetricsEnabled",
                captureMetrics ? 1 : 0);
        }

        private void DispatchSimulation(int startX, int countX)
        {
            computeShader.SetInt("_FoamRangeStart", startX);
            computeShader.SetInt("_FoamRangeCount", countX);

            // Patch 4.11C.5.16B combines conservative donor-cell advection and
            // lifecycle aging in one pass. The caller may repeat this kernel for
            // CFL-safe substeps; every pass reads one committed packed state and
            // writes the next ping-pong state.
            Dispatch(simulateKernel, countX, fieldHeight);
        }

        private bool CanDispatchVisualShape()
        {
            return computeShader != null && currentState != null &&
                shapeMaskTexture != null && filmSourceTexture != null &&
                filmSupportTexture != null && currentVisualOccupancy != null &&
                writeVisualOccupancy != null && boundaryTexture != null &&
                obstacleExclusionTexture != null && topologyTexture != null &&
                topologySourcesTexture != null && metricBuffer != null &&
                buildFilmSourceKernel >= 0 && buildFilmSupportKernel >= 0 &&
                advanceVisualOccupancyKernel >= 0 && evaluateShapeKernel >= 0 &&
                fieldWidth > 0 && fieldHeight > 0 && filmFieldWidth > 0 &&
                filmFieldHeight > 0;
        }

        private void ConfigureVisualShapeParameters(float deltaTime)
        {
            ConfigureGridDescriptorComputeParameters();
            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetInts(
                "_FoamFilmDimensions",
                filmFieldWidth,
                filmFieldHeight);
            computeShader.SetInts(
                "_FoamTopologyDimensions",
                structuralWidth,
                structuralHeight);
            computeShader.SetFloat("_FoamDeltaTime", Mathf.Max(0f, deltaTime));
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat("_FoamFlowDirection", river.FlowDirection);
            computeShader.SetFloat(
                "_FoamBaseDownstreamSpeed",
                ResolveBaseFoamDownstreamSpeedMetresPerSecond());
            computeShader.SetFloat(
                "_FoamMaximumLateralSpeedRatio",
                river.FoamMaximumLateralSpeedRatio);
            computeShader.SetFloat(
                "_FoamObstacleSlowdownStrength",
                river.FoamObstacleSlowdownStrength);
            computeShader.SetFloat(
                "_FoamObstacleMinimumDownstreamFactor",
                river.FoamObstacleMinimumDownstreamFactor);
            computeShader.SetFloat(
                "_FoamMotionLaneScrollCells",
                motionLaneScrollCells);
            computeShader.SetFloat(
                "_FoamVisualOccupancyBuildTime",
                river.FoamVisualOccupancyBuildTime);
            computeShader.SetFloat(
                "_FoamVisualOccupancyReleaseTime",
                river.FoamVisualOccupancyReleaseTime);
            computeShader.SetInt("_FoamRangeStart", 0);
            computeShader.SetInt("_FoamRangeCount", fieldWidth);
        }

        private void DispatchBuildFilmProducts()
        {
            using (BuildFilmSourceProfilerMarker.Auto())
            {
                computeShader.SetBuffer(
                    buildFilmSourceKernel,
                    "_FoamMetricRows",
                    metricBuffer);
                computeShader.SetTexture(
                    buildFilmSourceKernel,
                    "_FoamBoundary",
                    boundaryTexture);
                computeShader.SetTexture(
                    buildFilmSourceKernel,
                    "_FoamObstacleExclusionRead",
                    obstacleExclusionTexture);
                computeShader.SetTexture(
                    buildFilmSourceKernel,
                    "_FoamStateRead",
                    currentState);
                computeShader.SetTexture(
                    buildFilmSourceKernel,
                    "_FoamTopologyRead",
                    topologyTexture);
                computeShader.SetTexture(
                    buildFilmSourceKernel,
                    "_FoamTopologySourcesRead",
                    topologySourcesTexture);
                computeShader.SetTexture(
                    buildFilmSourceKernel,
                    "_FoamFilmSourceWrite",
                    filmSourceTexture);

                Dispatch(buildFilmSourceKernel, filmFieldWidth, filmFieldHeight);
            }

            using (BuildFilmSupportProfilerMarker.Auto())
            {
                computeShader.SetTexture(
                    buildFilmSupportKernel,
                    "_FoamFilmSourceRead",
                    filmSourceTexture);
                computeShader.SetTexture(
                    buildFilmSupportKernel,
                    "_FoamTopologyRead",
                    topologyTexture);
                computeShader.SetTexture(
                    buildFilmSupportKernel,
                    "_FoamTopologySourcesRead",
                    topologySourcesTexture);
                computeShader.SetTexture(
                    buildFilmSupportKernel,
                    "_FoamFilmSupportWrite",
                    filmSupportTexture);

                Dispatch(buildFilmSupportKernel, filmFieldWidth, filmFieldHeight);
            }
        }

        private void DispatchVisualOccupancySubstep(float deltaTime)
        {
            ConfigureVisualShapeParameters(deltaTime);
            computeShader.SetBuffer(
                advanceVisualOccupancyKernel,
                "_FoamMetricRows",
                metricBuffer);
            computeShader.SetTexture(
                advanceVisualOccupancyKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetTexture(
                advanceVisualOccupancyKernel,
                "_FoamObstacleExclusionRead",
                obstacleExclusionTexture);
            computeShader.SetTexture(
                advanceVisualOccupancyKernel,
                "_FoamMotionLaneRead",
                motionLaneTexture != null
                    ? (Texture)motionLaneTexture
                    : Texture2D.blackTexture);
            computeShader.SetTexture(
                advanceVisualOccupancyKernel,
                "_FoamObstacleRoutingRead",
                obstacleRoutingTexture != null
                    ? (Texture)obstacleRoutingTexture
                    : Texture2D.blackTexture);
            computeShader.SetTexture(
                advanceVisualOccupancyKernel,
                "_FoamFilmSourceRead",
                filmSourceTexture);
            computeShader.SetTexture(
                advanceVisualOccupancyKernel,
                "_FoamFilmSupportRead",
                filmSupportTexture);
            computeShader.SetTexture(
                advanceVisualOccupancyKernel,
                "_FoamVisualOccupancyRead",
                currentVisualOccupancy);
            computeShader.SetTexture(
                advanceVisualOccupancyKernel,
                "_FoamVisualOccupancyWrite",
                writeVisualOccupancy);

            Dispatch(
                advanceVisualOccupancyKernel,
                filmFieldWidth,
                filmFieldHeight);
            (currentVisualOccupancy, writeVisualOccupancy) =
                (writeVisualOccupancy, currentVisualOccupancy);
        }

        private void DispatchEvaluateShapeMask()
        {
            using var profilerScope = EvaluateShapeProfilerMarker.Auto();

            computeShader.SetTexture(
                evaluateShapeKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetTexture(
                evaluateShapeKernel,
                "_FoamObstacleExclusionRead",
                obstacleExclusionTexture);
            computeShader.SetTexture(
                evaluateShapeKernel,
                "_FoamStateRead",
                currentState);
            computeShader.SetTexture(
                evaluateShapeKernel,
                "_FoamVisualOccupancyRead",
                currentVisualOccupancy);
            computeShader.SetTexture(
                evaluateShapeKernel,
                "_FoamShapeMaskWrite",
                shapeMaskTexture);

            Dispatch(evaluateShapeKernel, fieldWidth, fieldHeight);
        }

        private bool DispatchAdvanceVisualOccupancy(
            float materialStepDuration,
            int transportSubsteps)
        {
            if (!CanDispatchVisualShape() || transportSubsteps <= 0)
            {
                return false;
            }

            bool recordShapeWork = steadyStateWorkAccountingActive;
            long shapeWorkStartedAt = recordShapeWork
                ? CaptureWorkTimestamp()
                : 0L;
            int shapeDispatchesBefore = recordShapeWork
                ? lastUpdateDispatches
                : 0;
            long shapeCellIterationsBefore = recordShapeWork
                ? lastUpdateCellIterations
                : 0L;

            ConfigureVisualShapeParameters(materialStepDuration);
            DispatchBuildFilmProducts();

            float substepDelta = materialStepDuration /
                Mathf.Max(1, transportSubsteps);
            using (AdvanceVisualOccupancyProfilerMarker.Auto())
            {
                for (int substep = 0; substep < transportSubsteps; substep++)
                {
                    DispatchVisualOccupancySubstep(substepDelta);
                }
            }

            DispatchEvaluateShapeMask();
            if (recordShapeWork)
            {
                RecordShapeEvaluationWork(
                    shapeWorkStartedAt,
                    shapeDispatchesBefore,
                    shapeCellIterationsBefore);
            }
            return true;
        }

        private void DispatchEvaluateShape()
        {
            if (!CanDispatchVisualShape())
            {
                return;
            }

            bool recordShapeWork = steadyStateWorkAccountingActive;
            long shapeWorkStartedAt = recordShapeWork
                ? CaptureWorkTimestamp()
                : 0L;
            int shapeDispatchesBefore = recordShapeWork
                ? lastUpdateDispatches
                : 0;
            long shapeCellIterationsBefore = recordShapeWork
                ? lastUpdateCellIterations
                : 0L;

            ConfigureVisualShapeParameters(0f);
            DispatchBuildFilmProducts();
            DispatchEvaluateShapeMask();
            if (recordShapeWork)
            {
                RecordShapeEvaluationWork(
                    shapeWorkStartedAt,
                    shapeDispatchesBefore,
                    shapeCellIterationsBefore);
            }
        }

        private bool BeginTransportMetricCapture()
        {
            if (computeShader == null || transportMetricsBuffer == null ||
                resetTransportMetricsKernel < 0 ||
                transportMetricsReadbackPending ||
                !SystemInfo.supportsAsyncGPUReadback)
            {
                return false;
            }

            computeShader.SetBuffer(
                resetTransportMetricsKernel,
                "_FoamTransportMetrics",
                transportMetricsBuffer);
            DispatchOneDimensional(
                resetTransportMetricsKernel,
                TransportMetricCount,
                64);
            return true;
        }

        private void RequestTransportMetricReadback()
        {
            if (transportMetricsBuffer == null ||
                transportMetricsReadbackPending)
            {
                return;
            }

            if (!SystemInfo.supportsAsyncGPUReadback)
            {
                // Diagnostics must never introduce a synchronous GPU wait into
                // operational material transport. Unsupported platforms simply
                // omit conservation readback while retaining the same solve.
                transportMetricsAvailable = false;
                return;
            }

            transportMetricsReadbackPending = true;
            transportMetricsReadbackRequestedAt =
                Time.realtimeSinceStartupAsDouble;
            if (steadyStateWorkAccountingActive)
            {
                steadyStateWorkTransportMetricRequestCount++;
                steadyStateWorkTransportMetricGeneration =
                    steadyStateWorkAccountingGeneration;
            }
            int generation = transportMetricsGeneration;
            ComputeBuffer requestedBuffer = transportMetricsBuffer;
            AsyncGPUReadback.Request(
                requestedBuffer,
                request =>
                {
                    if (this == null || generation != transportMetricsGeneration)
                    {
                        requestedBuffer?.Release();
                        return;
                    }

                    transportMetricsReadbackPending = false;
                    transportMetricsReadbackRequestedAt = -1.0;
                    if (request.hasError)
                    {
                        transportMetricsAvailable = false;
                        if (steadyStateWorkAccountingActive &&
                            steadyStateWorkTransportMetricGeneration ==
                                steadyStateWorkAccountingGeneration)
                        {
                            steadyStateWorkTransportMetricErrorCount++;
                        }
                        return;
                    }

                    var data = request.GetData<uint>();
                    int count = Mathf.Min(data.Length, TransportMetricCount);
                    for (int index = 0; index < count; index++)
                    {
                        latestTransportMetrics[index] = data[index];
                    }

                    if (count == TransportMetricCount)
                    {
                        ApplyTransportMetricReadback(latestTransportMetrics);
                        if (steadyStateWorkAccountingActive &&
                            steadyStateWorkTransportMetricGeneration ==
                                steadyStateWorkAccountingGeneration)
                        {
                            steadyStateWorkTransportMetricCompletionCount++;
                        }
                    }
                    else if (steadyStateWorkAccountingActive &&
                        steadyStateWorkTransportMetricGeneration ==
                            steadyStateWorkAccountingGeneration)
                    {
                        steadyStateWorkTransportMetricErrorCount++;
                    }
                });
        }

        private void ApplyTransportMetricReadback(uint[] values)
        {
            if (values == null || values.Length < TransportMetricCount)
            {
                transportMetricsAvailable = false;
                return;
            }

            float inverseScale = 1f / TransportMetricFixedPointScale;
            transportPresenceBefore =
                values[TransportPresenceBeforeMetricIndex] * inverseScale;
            transportLifeMomentBefore =
                values[TransportLifeBeforeMetricIndex] * inverseScale;
            transportPatternMomentBefore =
                values[TransportPatternBeforeMetricIndex] * inverseScale;
            transportPresenceAfter =
                values[TransportPresenceAfterMetricIndex] * inverseScale;
            transportLifeMomentAfter =
                values[TransportLifeAfterMetricIndex] * inverseScale;
            transportPatternMomentAfter =
                values[TransportPatternAfterMetricIndex] * inverseScale;
            transportPresenceBoundaryOutflow =
                values[TransportPresenceOutflowMetricIndex] * inverseScale;
            transportLifeBoundaryOutflow =
                values[TransportLifeOutflowMetricIndex] * inverseScale;
            transportPatternBoundaryOutflow =
                values[TransportPatternOutflowMetricIndex] * inverseScale;
            transportPresenceClampLoss =
                values[TransportPresenceClampMetricIndex] * inverseScale;
            transportLifeClampLoss =
                values[TransportLifeClampMetricIndex] * inverseScale;
            transportPatternClampLoss =
                values[TransportPatternClampMetricIndex] * inverseScale;
            transportPresenceUnitCapacityLoss =
                values[TransportPresenceUnitCapacityLossMetricIndex] *
                inverseScale;
            transportPresenceBoundaryCapacityLoss =
                values[TransportPresenceBoundaryCapacityLossMetricIndex] *
                inverseScale;
            transportPresenceObstacleCapacityLoss =
                values[TransportPresenceObstacleCapacityLossMetricIndex] *
                inverseScale;
            transportPresenceStateValidityLoss =
                values[TransportPresenceStateValidityLossMetricIndex] *
                inverseScale;
            transportPresenceMinimumCutoffLoss =
                values[TransportPresenceMinimumCutoffLossMetricIndex] *
                inverseScale;
            transportMaximumRawPresence =
                values[TransportMaximumRawPresenceMetricIndex] * inverseScale;
            transportMaximumLocalCapacityExcess =
                values[TransportMaximumLocalCapacityExcessMetricIndex] *
                inverseScale;
            transportTotalCapacityHitCount =
                values[TransportTotalCapacityHitCountMetricIndex];
            transportUnitCapacityHitCount =
                values[TransportUnitCapacityHitCountMetricIndex];
            transportBoundaryCapacityHitCount =
                values[TransportBoundaryCapacityHitCountMetricIndex];
            transportObstacleCapacityHitCount =
                values[TransportObstacleCapacityHitCountMetricIndex];

            float attributedPresenceLoss =
                transportPresenceUnitCapacityLoss +
                transportPresenceBoundaryCapacityLoss +
                transportPresenceObstacleCapacityLoss +
                transportPresenceStateValidityLoss +
                transportPresenceMinimumCutoffLoss;
            transportPresenceAttributionResidual =
                transportPresenceClampLoss - attributedPresenceLoss;

            transportPresenceUnaccountedError =
                transportPresenceBefore - transportPresenceAfter -
                transportPresenceBoundaryOutflow -
                transportPresenceClampLoss;
            transportLifeUnaccountedError =
                transportLifeMomentBefore - transportLifeMomentAfter -
                transportLifeBoundaryOutflow - transportLifeClampLoss;
            transportPatternUnaccountedError =
                transportPatternMomentBefore - transportPatternMomentAfter -
                transportPatternBoundaryOutflow - transportPatternClampLoss;

            transportPresenceUnaccountedErrorRatio = ResolveTransportErrorRatio(
                transportPresenceUnaccountedError,
                transportPresenceBefore);
            transportLifeUnaccountedErrorRatio = ResolveTransportErrorRatio(
                transportLifeUnaccountedError,
                transportLifeMomentBefore);
            transportPatternUnaccountedErrorRatio = ResolveTransportErrorRatio(
                transportPatternUnaccountedError,
                transportPatternMomentBefore);
            transportPresenceClampLossRatio = ResolveTransportErrorRatio(
                transportPresenceClampLoss,
                transportPresenceBefore);
            transportMetricsAvailable = true;
            transportMetricsLastCompletedAt =
                Time.realtimeSinceStartupAsDouble;
        }

        private static float ResolveTransportErrorRatio(
            float error,
            float reference)
        {
            return Mathf.Abs(error) / Mathf.Max(0.000001f, Mathf.Abs(reference));
        }

        private void ResetTransportMetricValues()
        {
            transportPresenceBefore = 0f;
            transportPresenceAfter = 0f;
            transportLifeMomentBefore = 0f;
            transportLifeMomentAfter = 0f;
            transportPatternMomentBefore = 0f;
            transportPatternMomentAfter = 0f;
            transportPresenceBoundaryOutflow = 0f;
            transportLifeBoundaryOutflow = 0f;
            transportPatternBoundaryOutflow = 0f;
            transportPresenceClampLoss = 0f;
            transportLifeClampLoss = 0f;
            transportPatternClampLoss = 0f;
            transportPresenceUnitCapacityLoss = 0f;
            transportPresenceBoundaryCapacityLoss = 0f;
            transportPresenceObstacleCapacityLoss = 0f;
            transportPresenceStateValidityLoss = 0f;
            transportPresenceMinimumCutoffLoss = 0f;
            transportPresenceAttributionResidual = 0f;
            transportMaximumRawPresence = 0f;
            transportMaximumLocalCapacityExcess = 0f;
            transportTotalCapacityHitCount = 0u;
            transportUnitCapacityHitCount = 0u;
            transportBoundaryCapacityHitCount = 0u;
            transportObstacleCapacityHitCount = 0u;
            transportPresenceUnaccountedError = 0f;
            transportLifeUnaccountedError = 0f;
            transportPatternUnaccountedError = 0f;
            transportPresenceUnaccountedErrorRatio = 0f;
            transportLifeUnaccountedErrorRatio = 0f;
            transportPatternUnaccountedErrorRatio = 0f;
            transportPresenceClampLossRatio = 0f;
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

            ConfigureGridDescriptorComputeParameters();
            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
            computeShader.SetFloat("_FoamTime", river.MotionTime);
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
            long cellIterations = (long)width * height;
            lastUpdateCellIterations += cellIterations;
            if (steadyStateWorkAccountingActive)
            {
                steadyStateWorkTotalDispatchCount++;
                steadyStateWorkTotalCellIterations += cellIterations;
            }
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
            if (steadyStateWorkAccountingActive)
            {
                steadyStateWorkTotalDispatchCount++;
                steadyStateWorkTotalCellIterations += count;
            }
        }
    }
}
