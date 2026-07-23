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
        private void ResetManualInjectionSequence()
        {
            manualInjectionSequence = 0;
        }

        private bool ProcessPendingInjections(float now)
        {
            if (pendingInjections.Count == 0)
            {
                return false;
            }

            bool manualQueued = false;
            for (int index = 0; index < pendingInjections.Count; index++)
            {
                PendingInjection injection = pendingInjections[index];
                QueueMaterialBirth(injection);
                if (injection.IsManual)
                {
                    manualProofReferenceArea = 0f;
                    manualProofReferencePending = true;
                    manualQueued = true;
                }

                injectedLastUpdate++;
            }

            pendingInjections.Clear();
            return manualQueued;
        }

        private void QueueMaterialBirth(PendingInjection injection)
        {
            isolatedLifeProbeAbsoluteAgingActive = false;
            isolatedLifeProbeWrittenAt = -1.0;
            pendingMaterialBirths.Add(injection);
            RecordMaterialBirthCommand();
            materialLifetimeAuthorityActive = true;
            materialLifetimeEmptyMetricReadbacks = 0;
            lifetimeAuthorityStatus =
                "Remaining Life / full-field direct simulation";
        }

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
            DispatchQueuedMaterialBirths(currentState);
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

        private void DispatchQueuedMaterialBirths(RenderTexture target)
        {
            if (pendingMaterialBirths.Count == 0 || target == null)
            {
                pendingMaterialBirths.Clear();
                return;
            }

            for (int index = 0; index < pendingMaterialBirths.Count; index++)
            {
                DispatchInjection(pendingMaterialBirths[index], target);
            }

            pendingMaterialBirths.Clear();
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
                    bool hasNewDeposition =
                        gpuData.Deposit.z < 0.5f ||
                        gpuData.Header.z >
                            gpuData.Deposit.y + 0.000001f;
                    if (hasNewDeposition)
                    {
                        DispatchAutomaticFoamSourceEvent(
                            index,
                            sourceEvent,
                            target);
                        automaticSourceEventsRasterizedLastUpdate++;
                        injectedLastUpdate++;
                    }

                    if (sourceEvent.Elapsed >= sourceEvent.Duration - 0.00001f)
                    {
                        CompleteAutomaticObjectSourceEvent(sourceEvent);
                        automaticFoamSourceEvents[index] = default;
                        automaticFoamSourceEventGpuData[index] = default;
                        activeAutomaticFoamSourceEventCount = Mathf.Max(
                            0,
                            activeAutomaticFoamSourceEventCount - 1);
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
            out float progress)
        {
            phaseOrSide = IsAutomaticObjectContactCycle(sourceEvent.Type)
                ? 0f
                : sourceEvent.SideSign;
            progress = Mathf.Clamp01(
                elapsed / Mathf.Max(0.0001f, sourceEvent.Duration));
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
                out float progress);
            ResolveAutomaticSourceDepositionState(
                sourceEvent,
                previousElapsed,
                out float previousPhaseCode,
                out float previousProgress);
            float materialStepProgress = Mathf.Clamp01(
                (1f / Mathf.Max(1f, ResolveUpdateRate())) /
                Mathf.Max(0.0001f, sourceEvent.Duration));

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
                    sourceEvent.ObjectWakeArmLengthMetres,
                    materialStepProgress,
                    sourceEvent.ObjectContactPoint2.x);
                variationData = new Vector4(
                    sourceEvent.ObjectContactNegativeFirstSegmentSplit,
                    sourceEvent.ObjectContactPoint2.y,
                    sourceEvent.ObjectContactPoint3.x,
                    sourceEvent.Curvature);
                kinematicsData = new Vector4(
                    sourceEvent.ObjectContactPoint3.y,
                    sourceEvent.ObjectContactPoint4.x,
                    sourceEvent.ObjectContactPathLengthMetres,
                    sourceEvent.ObjectContactPositiveFirstSegmentSplit);
                objectData = new Vector4(
                    sourceEvent.ObjectCentreAcrossMetres,
                    sourceEvent.ObjectContactPoint4.y,
                    sourceEvent.ObjectContactFrontSplit,
                    sourceEvent.ObjectSourceLateralCellSpacingMetres);
            }
            else
            {
                distanceData = new Vector4(
                    startStorageGlobal,
                    endStorageGlobal,
                    centreStorageGlobal,
                    river != null && river.FlowDirection >= 0f ? 1f : -1f);
                shoreData = new Vector4(
                    sourceEvent.ShoreInsetMetres,
                    sourceEvent.Type == AutomaticFoamSourceEventType.ShoreRibbon
                        ? (descriptor.IsCreated &&
                           descriptor.UsesFixedMetricLattice
                            ? sourceEvent.ShoreRibbonThicknessMetres
                            : sourceEvent.ShoreRibbonThicknessCells)
                        : sourceEvent.WidthMetres,
                    sourceEvent.InwardReachMetres,
                    sourceEvent.FeatherMetres);
                variationData = new Vector4(
                    sourceEvent.SourceFillSeed,
                    0f,
                    0f,
                    sourceEvent.Curvature);
                kinematicsData = new Vector4(
                    sourceEvent.FormationSpeedMetresPerSecond,
                    sourceEvent.HeadTrailMetres,
                    Mathf.Sqrt(
                        Mathf.Abs(endStorageGlobal - startStorageGlobal) *
                        Mathf.Abs(endStorageGlobal - startStorageGlobal) +
                        sourceEvent.InwardReachMetres *
                        sourceEvent.InwardReachMetres),
                    Mathf.Clamp01(sourceEvent.SourceFillBlend));
                objectData = new Vector4(
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
                    progress,
                    sourceEvent.ShapeSeed),
                Distance = distanceData,
                Shore = shoreData,
                Material = new Vector4(
                    sourceEvent.SourceAmount,
                    sourceEvent.RemainingLife,
                    sourceEvent.PatternSeed,
                    sourceEvent.SourceFillFeatureSize),
                Variation = variationData,
                Kinematics = kinematicsData,
                ObjectData = objectData,
                Deposit = new Vector4(
                    previousPhaseCode,
                    previousProgress,
                    previousElapsed > 0.000001f ? 1f : 0f,
                    0f)
            };
        }

        private void DispatchAutomaticFoamSourceEvent(
            int eventIndex,
            AutomaticFoamSourceEvent sourceEvent,
            RenderTexture target)
        {
            ConfigureGridDescriptorComputeParameters();
            if (!TryResolveAutomaticSourceDispatchRange(
                    sourceEvent,
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
            pendingInjections.Clear();
            pendingMaterialBirths.Clear();
            ClearFoamCompositionEvents();

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

        private void DispatchInjection(PendingInjection injection, RenderTexture target)
        {
            ConfigureGridDescriptorComputeParameters();
            float storageGlobalDistance =
                WorldGlobalDistanceToFoamStorageGlobalDistance(
                    injection.GlobalDistance);
            float storageSegmentStartGlobalDistance =
                WorldGlobalDistanceToFoamStorageGlobalDistance(
                    injection.SegmentStartGlobalDistance);
            float storageSegmentEndGlobalDistance =
                WorldGlobalDistanceToFoamStorageGlobalDistance(
                    injection.SegmentEndGlobalDistance);
            if (!TryResolveManualInjectionDispatchRange(
                    injection,
                    out P7SourceDispatchRange dispatchRange))
            {
                return;
            }

            int startX = dispatchRange.StartX;
            int countX = dispatchRange.CountX;
            int startY = dispatchRange.StartY;
            int countY = dispatchRange.CountY;

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
            computeShader.SetInt("_FoamRangeStart", startX);
            computeShader.SetInt("_FoamRangeCount", countX);
            computeShader.SetInt("_FoamRangeStartY", startY);
            computeShader.SetInt("_FoamRangeCountY", countY);
            computeShader.SetFloat("_FoamGlobalStart", river.Domain.GlobalDistanceMinimum);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat("_FoamInjectionGlobalDistance", storageGlobalDistance);
            computeShader.SetFloat(
                "_FoamInjectionAcrossNormalized",
                injection.AcrossNormalized);
            computeShader.SetFloat(
                "_FoamInjectionUsesMetricLateral",
                injection.UsesMetricLateral ? 1f : 0f);
            computeShader.SetFloat(
                "_FoamInjectionLateralMetres",
                injection.LateralMetres);
            computeShader.SetFloat("_FoamInjectionRadius", injection.Radius);
            computeShader.SetFloat("_FoamInjectionAmount", injection.SourceAmount);
            computeShader.SetFloat(
                "_FoamInjectionRemainingLife",
                injection.RemainingLife);
            computeShader.SetFloat(
                "_FoamInjectionPatternSeed",
                injection.PatternSeed);
            computeShader.SetFloat("_FoamInjectionElongation", injection.Elongation);
            computeShader.SetFloat(
                "_FoamInjectionSourceFillSeed",
                injection.SourceFillSeed);
            computeShader.SetFloat(
                "_FoamInjectionSourceFillFeatureSize",
                injection.SourceFillFeatureSize);
            computeShader.SetFloat("_FoamInjectionShapeSeed", injection.ShapeSeed);
            computeShader.SetFloat("_FoamInjectionShapeVariety", injection.ShapeVariety);
            computeShader.SetFloat(
                "_FoamInjectionCompound",
                injection.CompoundShape ? 1f : 0f);
            computeShader.SetFloat(
                "_FoamInjectionSegment",
                injection.SegmentShape ? 1f : 0f);
            computeShader.SetFloat(
                "_FoamInjectionSegmentStartGlobalDistance",
                storageSegmentStartGlobalDistance);
            computeShader.SetFloat(
                "_FoamInjectionSegmentStartAcrossNormalized",
                injection.SegmentStartAcrossNormalized);
            computeShader.SetFloat(
                "_FoamInjectionSegmentStartLateralMetres",
                injection.SegmentStartLateralMetres);
            computeShader.SetFloat(
                "_FoamInjectionSegmentStartRadius",
                injection.SegmentStartRadius);
            computeShader.SetFloat(
                "_FoamInjectionSegmentStartAmount",
                injection.SegmentStartSourceAmount);
            computeShader.SetFloat(
                "_FoamInjectionSegmentEndGlobalDistance",
                storageSegmentEndGlobalDistance);
            computeShader.SetFloat(
                "_FoamInjectionSegmentEndAcrossNormalized",
                injection.SegmentEndAcrossNormalized);
            computeShader.SetFloat(
                "_FoamInjectionSegmentEndLateralMetres",
                injection.SegmentEndLateralMetres);
            computeShader.SetFloat(
                "_FoamInjectionSegmentEndRadius",
                injection.SegmentEndRadius);
            computeShader.SetFloat(
                "_FoamInjectionSegmentEndAmount",
                injection.SegmentEndSourceAmount);
            computeShader.SetBuffer(injectKernel, "_FoamMetricRows", metricBuffer);
            computeShader.SetTexture(injectKernel, "_FoamBoundary", boundaryTexture);
            computeShader.SetTexture(
                injectKernel,
                "_FoamObstacleExclusionRead",
                obstacleExclusionTexture);

            DispatchInjectionToState(target, countX, countY);

            if (injection.IsManual)
            {
                lastInjectionStateSynchronized = true;
                lastInjectionBoundaryCoverage = SampleInjectionBoundaryCoverage(injection);
                simulationInterpolation = 1f;
            }
        }

        private void DispatchInjectionToState(
            RenderTexture target,
            int countX,
            int countY)
        {
            if (target == null)
            {
                return;
            }

            computeShader.SetTexture(injectKernel, "_FoamStateWrite", target);
            Dispatch(injectKernel, countX, countY);
        }

        private float SampleInjectionBoundaryCoverage(PendingInjection injection)
        {
            if (boundaryTexture == null || fieldLength <= 0.0001f)
            {
                return -1f;
            }

            float u;
            float v;
            if (gridDescriptor.IsCreated &&
                gridDescriptor.UsesFixedMetricLattice)
            {
                float localDistance = injection.GlobalDistance -
                    river.Domain.GlobalDistanceMinimum;
                u = Mathf.Clamp01(
                    (localDistance -
                        gridDescriptor.FieldOrStripStartMetres) /
                    Mathf.Max(
                        0.0001f,
                        gridDescriptor.AllocatedLengthMetres));
                float lateralMetres = injection.UsesMetricLateral
                    ? injection.LateralMetres
                    : ResolveSourceLateralMetres(
                        injection.GlobalDistance,
                        injection.AcrossNormalized);
                v = Mathf.Clamp01(
                    (lateralMetres -
                        gridDescriptor.RepresentedLateralMinimumMetres) /
                    Mathf.Max(
                        0.0001f,
                        gridDescriptor.RepresentedLateralMaximumMetres -
                        gridDescriptor.RepresentedLateralMinimumMetres));
            }
            else
            {
                u = Mathf.Clamp01(
                    (injection.GlobalDistance -
                        river.Domain.GlobalDistanceMinimum) /
                    fieldLength);
                v = Mathf.Clamp01(
                    injection.AcrossNormalized * 0.5f + 0.5f);
            }

            return Mathf.Clamp01(boundaryTexture.GetPixelBilinear(u, v).r);
        }
    }
}
