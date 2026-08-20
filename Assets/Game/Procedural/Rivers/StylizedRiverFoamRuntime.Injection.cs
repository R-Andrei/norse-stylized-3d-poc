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
        private void RecordMaterialBirthCommand()
        {
            double now = Time.realtimeSinceStartupAsDouble;
            birthCommandsThisFrame++;
            lastBirthCommandAt = now;
            if (!materialClockSessionActive)
            {
                materialClockSessionActive = true;
                materialClockSessionStartedAt = now;
                materialSimulatedSecondsSinceSession = 0f;
                materialStepCountSinceSession = 0;
            }
        }

        private void RecordMaterialSimulationStep(float deltaTime)
        {
            if (!materialClockSessionActive)
            {
                return;
            }

            materialSimulatedSecondsSinceSession += Mathf.Max(0f, deltaTime);
            materialStepCountSinceSession++;
        }

        private void PrepareBulkTransportSubstep(
            float substepDelta)
        {
            bulkTransportIntegerShift = 0;

            float spacing = Mathf.Max(
                0.0001f,
                gridDescriptor.ResolvedDxMetres);
            float flowSign = river.FlowDirection >= 0f ? 1f : -1f;
            float speed = ResolveBaseFoamDownstreamSpeedMetresPerSecond();
            bulkTransportPhaseCells +=
                flowSign * speed * Mathf.Max(0f, substepDelta) / spacing;

            if (bulkTransportPhaseCells >= 1f)
            {
                bulkTransportIntegerShift =
                    Mathf.FloorToInt(bulkTransportPhaseCells);
            }
            else if (bulkTransportPhaseCells <= -1f)
            {
                bulkTransportIntegerShift =
                    Mathf.CeilToInt(bulkTransportPhaseCells);
            }

            bulkTransportPhaseCells -= bulkTransportIntegerShift;
        }

        private void SimulateFullField(
            float materialStepDuration,
            int transportSubsteps)
        {
            if (currentState == null || writeState == null ||
                fieldWidth <= 0 || fieldHeight <= 0 ||
                transportSubsteps <= 0)
            {
                return;
            }

            bool recordMaterialWork = steadyStateWorkAccountingActive;
            long materialWorkStartedAt = recordMaterialWork
                ? CaptureWorkTimestamp()
                : 0L;
            int materialDispatchesBefore = recordMaterialWork
                ? lastUpdateDispatches
                : 0;
            long materialCellIterationsBefore = recordMaterialWork
                ? lastUpdateCellIterations
                : 0L;

            if (presentationPreviousState == null ||
                !presentationPreviousState.IsCreated())
            {
                return;
            }

            Graphics.CopyTexture(currentState, presentationPreviousState);
            previousState = presentationPreviousState;
            previousBulkTransportPhaseCells = bulkTransportPhaseCells;
            float transportMetricsInterval = 1f /
                TransportMetricsUpdateRate;
            transportMetricsAccumulator += materialStepDuration;
            bool captureTransportMetrics =
                transportMetricsAccumulator >= transportMetricsInterval &&
                BeginTransportMetricCapture();
            if (captureTransportMetrics)
            {
                transportMetricsAccumulator %= transportMetricsInterval;
            }

            float substepDelta = materialStepDuration /
                Mathf.Max(1, transportSubsteps);

            for (int substep = 0; substep < transportSubsteps; substep++)
            {
                // Topology maintenance kernels also use shared compute
                // parameters. Rebind every conservative substep immediately
                // before SimulateFoam so the packed read/write textures and
                // lifecycle delta are authoritative.
                bool finalSubstep = substep == transportSubsteps - 1;
                float lifecycleDeltaTime = finalSubstep
                    ? materialStepDuration
                    : 0f;
                PrepareBulkTransportSubstep(substepDelta);
                ConfigureSharedComputeParameters(
                    substepDelta,
                    lifecycleDeltaTime);
                ConfigureTransportSubstepDiagnostics(
                    captureTransportMetrics);
                DispatchSimulation(0, fieldWidth);
                (currentState, writeState) = (writeState, currentState);
            }

            // Births are merged into the completed transported state. They
            // begin moving on the following fixed material tick and therefore
            // cannot be written into an obsolete pre-advection texture.
            DispatchAutomaticFoamSourceEvents(
                currentState,
                materialStepDuration);
            RecordMaterialSimulationStep(materialStepDuration);

            if (captureTransportMetrics)
            {
                RequestTransportMetricReadback();
            }

            if (recordMaterialWork)
            {
                RecordMaterialSimulationWork(
                    materialWorkStartedAt,
                    materialDispatchesBefore,
                    materialCellIterationsBefore,
                    transportSubsteps);
            }
        }

        private void DispatchAutomaticFoamSourceEvents(
            RenderTexture target,
            float deltaTime)
        {
            if (activeAutomaticFoamSourceEventCount <= 0 || target == null ||
                computeShader == null || automaticFoamSourceEventBuffer == null ||
                rasterizeFoamSourceEventKernel < 0 || metricBuffer == null ||
                boundaryTexture == null || obstacleExclusionTexture == null ||
                currentShoreEdgesTexture == null || fieldWidth <= 0 ||
                fieldHeight <= 0)
            {
                return;
            }

            int activeCount = 0;
            for (int index = 0; index < automaticFoamSourceEvents.Length; index++)
            {
                AutomaticFoamSourceEvent sourceEvent =
                    automaticFoamSourceEvents[index];
                if (!sourceEvent.Active)
                {
                    automaticFoamSourceEventGpuData[index] = default;
                    continue;
                }

                float previousElapsed = sourceEvent.Elapsed;
                sourceEvent.Elapsed = Mathf.Min(
                    sourceEvent.Duration,
                    sourceEvent.Elapsed + Mathf.Max(0f, deltaTime));
                automaticFoamSourceEvents[index] = sourceEvent;
                automaticFoamSourceEventGpuData[index] =
                    BuildAutomaticFoamSourceGpuData(
                        sourceEvent,
                        previousElapsed);
                activeCount++;
            }

            if (activeCount <= 0)
            {
                activeAutomaticFoamSourceEventCount = 0;
                return;
            }

            automaticFoamSourceEventBuffer.SetData(automaticFoamSourceEventGpuData);

            bool objectContactFieldRequired = false;
            for (int index = 0; index < automaticFoamSourceEvents.Length; index++)
            {
                AutomaticFoamSourceEvent sourceEvent =
                    automaticFoamSourceEvents[index];
                if (!sourceEvent.Active)
                {
                    continue;
                }

                if (sourceEvent.Type == AutomaticFoamSourceEventType.ObjectContactFleck)
                {
                    objectContactFieldRequired = true;
                    break;
                }
            }

            if (objectContactFieldRequired)
            {
                BuildObjectContactField();
            }

            using (RasterizeAutomaticSourceProfilerMarker.Auto())
            {
                for (int index = 0; index < automaticFoamSourceEvents.Length; index++)
                {
                    AutomaticFoamSourceEvent sourceEvent =
                        automaticFoamSourceEvents[index];
                    if (!sourceEvent.Active)
                    {
                        continue;
                    }

                    FoamSourceEventGpuData gpuData =
                        automaticFoamSourceEventGpuData[index];
                    bool depositionPhaseChanged = Mathf.Abs(
                        gpuData.Header.y - gpuData.Deposit.x) > 0.0001f;
                    bool debugHeadRequired = IsAutomaticBirthSourcesDebugActive;
                    bool hasNewDeposition =
                        gpuData.Deposit.z < 0.5f ||
                        depositionPhaseChanged ||
                        gpuData.Header.z >
                            gpuData.Deposit.y + 0.000001f;

                    if (hasNewDeposition || debugHeadRequired)
                    {
                        int revealDispatchCount;
                        if (IsAutomaticObjectContactCycle(sourceEvent.Type) &&
                            depositionPhaseChanged)
                        {
                            revealDispatchCount =
                                DispatchAutomaticObjectRevealSlices(
                                    index,
                                    sourceEvent,
                                    gpuData,
                                    target);
                        }
                        else
                        {
                            DispatchAutomaticFoamSourceEvent(
                                index,
                                sourceEvent,
                                gpuData,
                                target,
                                debugHeadRequired);
                            revealDispatchCount = 1;
                        }

                        automaticSourceEventsRasterizedLastUpdate +=
                            revealDispatchCount;
                        if (hasNewDeposition && revealDispatchCount > 0)
                        {
                            injectedLastUpdate++;
                        }
                    }

                    if (sourceEvent.Elapsed >= sourceEvent.Duration - 0.00001f)
                    {
                        CompleteAutomaticShoreSourceEvent(sourceEvent);
                        CompleteAutomaticObjectSourceEvent(sourceEvent);
                        automaticFoamSourceEvents[index] = default;
                        automaticFoamSourceEventGpuData[index] = default;
                        activeAutomaticFoamSourceEventCount = Mathf.Max(
                            0,
                            activeAutomaticFoamSourceEventCount - 1);
                        if (sourceEvent.Type ==
                                AutomaticFoamSourceEventType.ShoreRibbon ||
                            sourceEvent.Type ==
                                AutomaticFoamSourceEventType.InwardWash)
                        {
                            activeAutomaticShoreSourceEventCount = Mathf.Max(
                                0,
                                activeAutomaticShoreSourceEventCount - 1);
                        }
                        foamCompositionCompletedCount++;
                    }
                }
            }
        }

        private FoamSourceEventGpuData BuildAutomaticFoamSourceGpuData(
            AutomaticFoamSourceEvent sourceEvent,
            float previousElapsed)
        {
            return BuildAutomaticFoamSourceGpuData(
                sourceEvent,
                previousElapsed,
                gridDescriptor);
        }

        private static bool IsAutomaticObjectContactCycle(
            AutomaticFoamSourceEventType sourceType)
        {
            return sourceType == AutomaticFoamSourceEventType.ObjectContactArc ||
                sourceType == AutomaticFoamSourceEventType.ObjectContactSemiArc;
        }

        private static void ResolveAutomaticSourceDepositionState(
            AutomaticFoamSourceEvent sourceEvent,
            float elapsed,
            out float phaseOrSide,
            out float headDistanceCells)
        {
            float speedCellsPerSecond = Mathf.Max(
                0.0001f,
                sourceEvent.RevealSpeedCellsPerSecond);
            if (!IsAutomaticObjectContactCycle(sourceEvent.Type))
            {
                phaseOrSide = sourceEvent.SideSign;
                headDistanceCells = ResolveAutomaticRevealHeadDistanceCells(
                    sourceEvent.RevealPathLengthCells,
                    speedCellsPerSecond,
                    elapsed);
                return;
            }

            float contactStrokeDuration = Mathf.Max(
                0.0001f,
                sourceEvent.ObjectContactStrokeDuration > 0f
                    ? sourceEvent.ObjectContactStrokeDuration
                    : sourceEvent.ObjectBuildDuration);
            if (sourceEvent.ObjectContactReinforcementOnly)
            {
                phaseOrSide = 1f;
                headDistanceCells = ResolveAutomaticRevealHeadDistanceCells(
                    ResolveAutomaticObjectContactPhasePathLengthCells(
                        sourceEvent,
                        phaseOrSide),
                    speedCellsPerSecond,
                    elapsed);
                return;
            }

            int strokeCount = Mathf.Clamp(
                sourceEvent.ObjectContactStrokeCount,
                1,
                3);
            float initialStrokeDuration = Mathf.Max(
                0.0001f,
                sourceEvent.ObjectBuildDuration);
            float totalDuration = initialStrokeDuration +
                Mathf.Max(0, strokeCount - 1) * contactStrokeDuration;
            float clampedElapsed = Mathf.Clamp(elapsed, 0f, totalDuration);
            float phaseElapsed;
            if (strokeCount <= 1 || clampedElapsed < initialStrokeDuration)
            {
                phaseOrSide = 0f;
                phaseElapsed = clampedElapsed;
            }
            else
            {
                float reinforcementElapsed =
                    clampedElapsed - initialStrokeDuration;
                int reinforcementIndex = Mathf.Min(
                    strokeCount - 2,
                    Mathf.FloorToInt(
                        reinforcementElapsed / contactStrokeDuration));
                phaseElapsed = reinforcementElapsed -
                    reinforcementIndex * contactStrokeDuration;
                phaseOrSide = reinforcementIndex + 1;
                if (clampedElapsed >= totalDuration - 0.000001f)
                {
                    phaseElapsed = contactStrokeDuration;
                }
            }

            headDistanceCells = ResolveAutomaticRevealHeadDistanceCells(
                ResolveAutomaticObjectContactPhasePathLengthCells(
                    sourceEvent,
                    phaseOrSide),
                speedCellsPerSecond,
                phaseElapsed);
        }

        private static float ResolveAutomaticObjectContactPhasePathLengthCells(
            AutomaticFoamSourceEvent sourceEvent,
            float phaseCode)
        {
            if (!IsAutomaticObjectContactCycle(sourceEvent.Type) ||
                phaseCode < 0.5f)
            {
                return Mathf.Max(
                    0.001f,
                    sourceEvent.RevealPathLengthCells);
            }

            return Mathf.Max(
                0.001f,
                sourceEvent.ObjectContactStrokePathLengthCells > 0f
                    ? sourceEvent.ObjectContactStrokePathLengthCells
                    : sourceEvent.RevealPathLengthCells);
        }

        private FoamSourceEventGpuData BuildAutomaticFoamSourceGpuData(
            AutomaticFoamSourceEvent sourceEvent,
            float previousElapsed,
            StylizedRiverFoamGridDescriptor descriptor)
        {
            float startStorageGlobal =
                WorldGlobalDistanceToFoamStorageGlobalDistance(
                    sourceEvent.StartGlobalDistance);
            float endStorageGlobal =
                WorldGlobalDistanceToFoamStorageGlobalDistance(
                    sourceEvent.EndGlobalDistance);
            bool objectContactCycle =
                sourceEvent.Type == AutomaticFoamSourceEventType.ObjectContactArc ||
                sourceEvent.Type == AutomaticFoamSourceEventType.ObjectContactSemiArc;
            float centreStorageGlobal = objectContactCycle
                ? WorldGlobalDistanceToFoamStorageGlobalDistance(
                    sourceEvent.ObjectCentreGlobalDistance)
                : (startStorageGlobal + endStorageGlobal) * 0.5f;
            ResolveAutomaticSourceDepositionState(
                sourceEvent,
                sourceEvent.Elapsed,
                out float phaseCode,
                out float headDistanceCells);
            ResolveAutomaticSourceDepositionState(
                sourceEvent,
                previousElapsed,
                out float previousPhaseCode,
                out float previousHeadDistanceCells);

            Vector4 distanceData;
            Vector4 shoreData;
            Vector4 variationData;
            Vector4 kinematicsData;
            Vector4 objectData;
            if (objectContactCycle)
            {
                distanceData = new Vector4(
                    sourceEvent.ObjectContactPoint0.x,
                    sourceEvent.ObjectContactPoint0.y,
                    centreStorageGlobal,
                    sourceEvent.ObjectContactPoint1.x);
                shoreData = new Vector4(
                    sourceEvent.ObjectContactPoint1.y,
                    sourceEvent.WakeLengthCells,
                    sourceEvent.ContactWidthCells,
                    sourceEvent.ObjectContactPoint2.x);
                variationData = new Vector4(
                    sourceEvent.ObjectContactNegativeFirstSegmentSplit,
                    sourceEvent.ObjectContactPoint2.y,
                    sourceEvent.ObjectContactPoint3.x,
                    sourceEvent.Curvature);
                kinematicsData = new Vector4(
                    sourceEvent.ObjectContactPoint3.y,
                    sourceEvent.ObjectContactPoint4.x,
                    sourceEvent.HeadLengthCells,
                    sourceEvent.ObjectContactPositiveFirstSegmentSplit);
                objectData = new Vector4(
                    sourceEvent.ObjectCentreAcrossMetres,
                    sourceEvent.ObjectContactPoint4.y,
                    sourceEvent.ObjectContactFrontSplit,
                    sourceEvent.WakeWidthCells);
            }
            else
            {
                distanceData = new Vector4(
                    startStorageGlobal,
                    endStorageGlobal,
                    centreStorageGlobal,
                    river != null && river.FlowDirection >= 0f ? 1f : -1f);
                bool cellExactShoreSource =
                    sourceEvent.Type == AutomaticFoamSourceEventType.ShoreRibbon ||
                    sourceEvent.Type == AutomaticFoamSourceEventType.InwardWash;
                shoreData = cellExactShoreSource
                    ? new Vector4(
                        sourceEvent.ShoreInsetMetres,
                        sourceEvent.WidthMetres,
                        sourceEvent.InwardReachMetres,
                        sourceEvent.FeatherMetres)
                    : new Vector4(
                        sourceEvent.ShoreInsetMetres,
                        sourceEvent.Type == AutomaticFoamSourceEventType.ShoreRibbon
                            ? (descriptor.IsCreated &&
                               descriptor.UsesFixedMetricLattice
                                ? sourceEvent.ShoreRibbonThicknessMetres
                                : sourceEvent.ShoreRibbonThicknessCells)
                            : sourceEvent.WidthMetres,
                        sourceEvent.InwardReachMetres,
                        sourceEvent.FeatherMetres);
                bool d8CellRecipe = cellExactShoreSource ||
                    sourceEvent.Type == AutomaticFoamSourceEventType.ObjectContactFleck ||
                    sourceEvent.Type == AutomaticFoamSourceEventType.FreeWaterLaceConnector ||
                    sourceEvent.Type == AutomaticFoamSourceEventType.FreeWaterCrossLaceConnector ||
                    sourceEvent.Type == AutomaticFoamSourceEventType.FreeWaterTornFragment;
                if (d8CellRecipe && !cellExactShoreSource)
                {
                    shoreData = new Vector4(
                        sourceEvent.ShoreInsetMetres,
                        sourceEvent.BodyWidthCells,
                        0f,
                        sourceEvent.HeadWidthCells);
                }
                variationData = new Vector4(
                    sourceEvent.SourceFillSeed,
                    0f,
                    0f,
                    d8CellRecipe ? sourceEvent.BendAmplitudeCells : sourceEvent.Curvature);
                kinematicsData = new Vector4(
                    sourceEvent.RevealSpeedCellsPerSecond,
                    sourceEvent.HeadLengthCells,
                    sourceEvent.RevealPathLengthCells,
                    d8CellRecipe
                        ? 1f
                        : Mathf.Clamp01(sourceEvent.SourceFillBlend));
                objectData = d8CellRecipe && !cellExactShoreSource
                    ? new Vector4(
                        sourceEvent.ObjectCentreAcrossMetres,
                        sourceEvent.BodyLengthCells,
                        sourceEvent.BendAmplitudeCells,
                        sourceEvent.ShoreInsetMetres)
                    : new Vector4(
                        sourceEvent.ObjectCentreAcrossMetres,
                        sourceEvent.ObjectAlongHalfLengthMetres,
                        sourceEvent.ObjectAcrossHalfWidthMetres,
                        sourceEvent.ObjectContactOffsetMetres);
            }

            return new FoamSourceEventGpuData
            {
                Header = new Vector4(
                    (float)sourceEvent.Type,
                    phaseCode,
                    headDistanceCells,
                    sourceEvent.ShapeSeed),
                Distance = distanceData,
                Shore = shoreData,
                Material = new Vector4(
                    sourceEvent.SourceAmount,
                    sourceEvent.RemainingLife,
                    sourceEvent.PatternSeed,
                    objectContactCycle ? sourceEvent.HeadWidthCells : sourceEvent.SourceFillFeatureSize),
                Variation = variationData,
                Kinematics = kinematicsData,
                ObjectData = objectData,
                Deposit = new Vector4(
                    previousPhaseCode,
                    previousHeadDistanceCells,
                    previousElapsed > 0.000001f ? 1f : 0f,
                    objectContactCycle
                        ? sourceEvent.ContactSpanCells
                        : 0f)
            };
        }

        private static FoamSourceEventGpuData WithAutomaticRevealState(
            FoamSourceEventGpuData gpuData,
            float phaseCode,
            float currentHeadDistanceCells,
            float previousHeadDistanceCells,
            bool previousValid)
        {
            gpuData.Header.y = phaseCode;
            gpuData.Header.z = Mathf.Max(0f, currentHeadDistanceCells);
            gpuData.Deposit.x = phaseCode;
            gpuData.Deposit.y = Mathf.Max(0f, previousHeadDistanceCells);
            gpuData.Deposit.z = previousValid ? 1f : 0f;
            return gpuData;
        }

        private int DispatchAutomaticObjectRevealSlices(
            int eventIndex,
            AutomaticFoamSourceEvent sourceEvent,
            FoamSourceEventGpuData endpointGpuData,
            RenderTexture target)
        {
            int previousPhase = Mathf.Max(
                0,
                Mathf.RoundToInt(endpointGpuData.Deposit.x));
            int currentPhase = Mathf.Max(
                previousPhase,
                Mathf.RoundToInt(endpointGpuData.Header.y));
            if (currentPhase <= previousPhase)
            {
                DispatchAutomaticFoamSourceEvent(
                    eventIndex,
                    sourceEvent,
                    endpointGpuData,
                    target,
                    IsAutomaticBirthSourcesDebugActive);
                return 1;
            }

            int dispatchCount = 0;
            float previousHead = Mathf.Max(0f, endpointGpuData.Deposit.y);
            for (int phase = previousPhase; phase <= currentPhase; phase++)
            {
                float phasePathLength =
                    ResolveAutomaticObjectContactPhasePathLengthCells(
                        sourceEvent,
                        phase);
                float currentHead = phase < currentPhase
                    ? phasePathLength
                    : Mathf.Clamp(
                        endpointGpuData.Header.z,
                        0f,
                        phasePathLength);
                float slicePreviousHead = phase == previousPhase
                    ? Mathf.Clamp(previousHead, 0f, phasePathLength)
                    : 0f;
                bool previousValid = phase == previousPhase
                    ? endpointGpuData.Deposit.z > 0.5f
                    : false;
                if (currentHead <= slicePreviousHead + 0.000001f &&
                    previousValid &&
                    !IsAutomaticBirthSourcesDebugActive)
                {
                    continue;
                }

                FoamSourceEventGpuData sliceGpuData =
                    WithAutomaticRevealState(
                        endpointGpuData,
                        phase,
                        currentHead,
                        slicePreviousHead,
                        previousValid);
                automaticFoamSourceEventGpuData[eventIndex] = sliceGpuData;
                automaticFoamSourceEventBuffer.SetData(
                    automaticFoamSourceEventGpuData,
                    eventIndex,
                    eventIndex,
                    1);
                DispatchAutomaticFoamSourceEvent(
                    eventIndex,
                    sourceEvent,
                    sliceGpuData,
                    target,
                    IsAutomaticBirthSourcesDebugActive);
                dispatchCount++;
            }

            automaticFoamSourceEventGpuData[eventIndex] = endpointGpuData;
            automaticFoamSourceEventBuffer.SetData(
                automaticFoamSourceEventGpuData,
                eventIndex,
                eventIndex,
                1);
            return dispatchCount;
        }

        private void DispatchAutomaticFoamSourceEvent(
            int eventIndex,
            AutomaticFoamSourceEvent sourceEvent,
            FoamSourceEventGpuData gpuData,
            RenderTexture target,
            bool includePersistentDebugHead)
        {
            ConfigureGridDescriptorComputeParameters();
            if (!TryResolveAutomaticSourceDispatchRange(
                    sourceEvent,
                    gpuData,
                    includePersistentDebugHead,
                    out P7SourceDispatchRange dispatchRange))
            {
                return;
            }

            int startX = dispatchRange.StartX;
            int countX = dispatchRange.CountX;
            int startY = dispatchRange.StartY;
            int countY = dispatchRange.CountY;

            bool debugWriteAvailable =
                IsAutomaticBirthSourcesDebugActive &&
                rasterizeFoamSourceEventDebugKernel >= 0 &&
                automaticBirthDebugTexture != null &&
                automaticBirthDebugCounterBuffer != null;
            int rasterKernel = debugWriteAvailable
                ? rasterizeFoamSourceEventDebugKernel
                : rasterizeFoamSourceEventKernel;

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
            computeShader.SetFloat("_FoamGlobalStart", river.Domain.GlobalDistanceMinimum);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetInt("_FoamRangeStart", startX);
            computeShader.SetInt("_FoamRangeCount", countX);
            computeShader.SetInt("_FoamRangeStartY", startY);
            computeShader.SetInt("_FoamRangeCountY", countY);
            computeShader.SetInt("_FoamSourceEventIndex", eventIndex);
            computeShader.SetInt("_FoamSourceEventDebugComponentMode", 0);
            computeShader.SetBuffer(
                rasterKernel,
                "_FoamMetricRows",
                metricBuffer);
            computeShader.SetBuffer(
                rasterKernel,
                "_FoamSourceEvents",
                automaticFoamSourceEventBuffer);
            computeShader.SetTexture(
                rasterKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetTexture(
                rasterKernel,
                "_FoamObstacleExclusionRead",
                obstacleExclusionTexture);
            computeShader.SetTexture(
                rasterKernel,
                "_FoamCurrentShoreEdgesRead",
                currentShoreEdgesTexture);
            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            RenderTexture staticPressureSource = disturbanceRuntime != null &&
                disturbanceRuntime.IsAllocated
                    ? disturbanceRuntime.StaticPressureTexture
                    : null;
            bool staticPressureAvailable = IsCreatedTexture(staticPressureSource);
            Texture staticPressureTexture = staticPressureAvailable
                ? (Texture)staticPressureSource
                : (neutralDisturbanceTexture != null
                    ? (Texture)neutralDisturbanceTexture
                    : Texture2D.blackTexture);
            Vector2Int staticPressureDimensions = staticPressureAvailable
                ? disturbanceRuntime.StaticPressureTextureDimensions
                : Vector2Int.one;
            computeShader.SetInts(
                "_FoamStaticPressureDimensions",
                staticPressureDimensions.x,
                staticPressureDimensions.y);
            computeShader.SetTexture(
                rasterKernel,
                "_FoamStaticPressureField",
                staticPressureTexture);
            Texture objectContactFieldReadTexture =
                IsCreatedTexture(objectContactFieldTexture)
                    ? (Texture)objectContactFieldTexture
                    : (IsCreatedTexture(neutralDisturbanceTexture)
                        ? (Texture)neutralDisturbanceTexture
                        : Texture2D.blackTexture);
            computeShader.SetTexture(
                rasterKernel,
                "_FoamObjectContactFieldRead",
                objectContactFieldReadTexture);
            computeShader.SetTexture(
                rasterKernel,
                "_FoamStateWrite",
                target);
            if (debugWriteAvailable)
            {
                computeShader.SetTexture(
                    rasterKernel,
                    "_FoamBirthDebugWrite",
                    automaticBirthDebugTexture);
                computeShader.SetBuffer(
                    rasterKernel,
                    "_FoamBirthDebugCounters",
                    automaticBirthDebugCounterBuffer);
            }

            Dispatch(rasterKernel, countX, countY);
        }

        private void BuildObjectContactField()
        {
            if (computeShader == null || buildObjectContactFieldKernel < 0 ||
                objectContactFieldTexture == null || !objectContactFieldTexture.IsCreated() ||
                boundaryTexture == null || obstacleExclusionTexture == null ||
                metricBuffer == null || fieldWidth <= 0 || fieldHeight <= 0)
            {
                return;
            }

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            RenderTexture staticPressureSource = disturbanceRuntime != null &&
                disturbanceRuntime.IsAllocated
                    ? disturbanceRuntime.StaticPressureTexture
                    : null;
            bool staticPressureAvailable = IsCreatedTexture(staticPressureSource);
            Texture staticPressureTexture = staticPressureAvailable
                ? (Texture)staticPressureSource
                : (neutralDisturbanceTexture != null
                    ? (Texture)neutralDisturbanceTexture
                    : Texture2D.blackTexture);
            Vector2Int staticPressureDimensions = staticPressureAvailable
                ? disturbanceRuntime.StaticPressureTextureDimensions
                : Vector2Int.one;

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetInts(
                "_FoamStaticPressureDimensions",
                staticPressureDimensions.x,
                staticPressureDimensions.y);
            computeShader.SetFloat("_FoamGlobalStart", river.Domain.GlobalDistanceMinimum);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat(
                "_FoamFlowDirection",
                river != null && river.FlowDirection >= 0f ? 1f : -1f);
            computeShader.SetBuffer(
                buildObjectContactFieldKernel,
                "_FoamMetricRows",
                metricBuffer);
            computeShader.SetTexture(
                buildObjectContactFieldKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetTexture(
                buildObjectContactFieldKernel,
                "_FoamObstacleExclusionRead",
                obstacleExclusionTexture);
            computeShader.SetTexture(
                buildObjectContactFieldKernel,
                "_FoamStaticPressureField",
                staticPressureTexture);
            computeShader.SetTexture(
                buildObjectContactFieldKernel,
                "_FoamObjectContactFieldWrite",
                objectContactFieldTexture);

            Dispatch(buildObjectContactFieldKernel, fieldWidth, fieldHeight);
        }


        private bool DispatchPendingIsolatedLifeProbe()
        {
            if (!pendingIsolatedLifeProbe)
            {
                return false;
            }

            bool absoluteAgingProbe = pendingIsolatedLifeProbeAbsoluteAging;
            pendingIsolatedLifeProbe = false;
            pendingIsolatedLifeProbeAbsoluteAging = false;
            ClearAutomaticFoamSourceEvents();

            if (computeShader == null || currentState == null ||
                writeState == null || presentationPreviousState == null ||
                boundaryTexture == null ||
                writeIsolatedLifeProbeKernel < 0 || fieldWidth <= 0 ||
                fieldHeight <= 0)
            {
                isolatedLifeProbeAbsoluteAgingActive = false;
                isolatedLifeProbeWrittenAt = -1.0;
                isolatedLifeProbeStatus =
                    "Failed: material resources are not ready";
                return false;
            }

            ResolveP7LifeProbeLayout(
                pendingIsolatedLifeProbeDistanceNormalized,
                pendingIsolatedLifeProbeAcrossNormalized,
                out int centreX,
                out int centreY,
                out int patchWidth,
                out int patchHeight,
                out int gap);
            int step = patchWidth + gap;

            LifeProbeRect rectA = MakeLifeProbeRect(
                centreX - step,
                centreY,
                patchWidth,
                patchHeight);
            LifeProbeRect rectB = MakeLifeProbeRect(
                centreX,
                centreY,
                patchWidth,
                patchHeight);
            LifeProbeRect rectC = MakeLifeProbeRect(
                centreX + step,
                centreY,
                patchWidth,
                patchHeight);

            DispatchIsolatedLifeProbeToState(currentState, rectA, rectB, rectC);
            DispatchIsolatedLifeProbeToState(writeState, rectA, rectB, rectC);
            DispatchIsolatedLifeProbeToState(
                presentationPreviousState,
                rectA,
                rectB,
                rectC);
            previousState = presentationPreviousState;
            simulationInterpolation = 1f;
            lastRenderInterpolationAlpha = simulationInterpolation;

            double now = Time.realtimeSinceStartupAsDouble;
            birthCommandsThisFrame++;
            lastBirthCommandAt = now;
            materialClockSessionActive = true;
            materialClockSessionStartedAt = now;
            materialSimulatedSecondsSinceSession = 0f;
            materialStepCountSinceSession = 0;
            isolatedLifeProbeAbsoluteAgingActive = absoluteAgingProbe;
            isolatedLifeProbeWrittenAt = now;
            materialLifetimeAuthorityActive = true;
            materialLifetimeEmptyMetricReadbacks = 0;
            lifetimeAuthorityStatus =
                "Remaining Life / isolated direct material probe";
            manualProofReferenceArea = 0f;
            manualProofReferencePending = true;
            simulationAccumulator = 0f;
            topologyMetricsAccumulator = 1f / Mathf.Max(1f, ResolveUpdateRate());
            isolatedLifeProbeStatus =
                absoluteAgingProbe
                    ? $"Wrote 3 isolated patches ({patchWidth}x{patchHeight} texels), absolute 1s aging"
                    : $"Wrote 3 isolated patches ({patchWidth}x{patchHeight} texels), configured aging";
            return true;
        }

        private readonly struct LifeProbeRect
        {
            public readonly int X;
            public readonly int Y;
            public readonly int Z;
            public readonly int W;

            public LifeProbeRect(int x, int y, int z, int w)
            {
                X = x;
                Y = y;
                Z = z;
                W = w;
            }
        }

        private LifeProbeRect MakeLifeProbeRect(
            int centreX,
            int centreY,
            int width,
            int height)
        {
            int halfWidth = Mathf.Max(1, width / 2);
            int halfHeight = Mathf.Max(1, height / 2);
            int xMin = Mathf.Clamp(centreX - halfWidth, 0, fieldWidth - 1);
            int xMax = Mathf.Clamp(centreX + halfWidth + 1, xMin + 1, fieldWidth);
            int yMin = Mathf.Clamp(centreY - halfHeight, 0, fieldHeight - 1);
            int yMax = Mathf.Clamp(centreY + halfHeight + 1, yMin + 1, fieldHeight);
            return new LifeProbeRect(xMin, yMin, xMax, yMax);
        }

        private void DispatchIsolatedLifeProbeToState(
            RenderTexture target,
            LifeProbeRect rectA,
            LifeProbeRect rectB,
            LifeProbeRect rectC)
        {
            if (target == null)
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
            computeShader.SetInts(
                "_FoamLifeProbeRectA",
                rectA.X,
                rectA.Y,
                rectA.Z,
                rectA.W);
            computeShader.SetInts(
                "_FoamLifeProbeRectB",
                rectB.X,
                rectB.Y,
                rectB.Z,
                rectB.W);
            computeShader.SetInts(
                "_FoamLifeProbeRectC",
                rectC.X,
                rectC.Y,
                rectC.Z,
                rectC.W);
            computeShader.SetTexture(
                writeIsolatedLifeProbeKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetTexture(
                writeIsolatedLifeProbeKernel,
                "_FoamStateWrite",
                target);
            Dispatch(writeIsolatedLifeProbeKernel, fieldWidth, fieldHeight);
        }

        private static float WorldGlobalDistanceToFoamStorageGlobalDistance(
            float globalDistance)
        {
            // Conservative local transport stores material in its actual
            // current grid cell. No global phase compensation remains.
            return globalDistance;
        }

    }
}
