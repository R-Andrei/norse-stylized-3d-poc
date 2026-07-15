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

            previousState = currentState;
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

                sourceEvent.Elapsed = Mathf.Min(
                    sourceEvent.Duration,
                    sourceEvent.Elapsed + Mathf.Max(0f, deltaTime));
                automaticFoamSourceEvents[index] = sourceEvent;
                automaticFoamSourceEventGpuData[index] =
                    BuildAutomaticFoamSourceGpuData(sourceEvent);
                activeCount++;
            }

            if (activeCount <= 0)
            {
                activeAutomaticFoamSourceEventCount = 0;
                return;
            }

            automaticFoamSourceEventBuffer.SetData(automaticFoamSourceEventGpuData);

            bool objectSourceActive = false;
            for (int index = 0; index < automaticFoamSourceEvents.Length; index++)
            {
                AutomaticFoamSourceEvent sourceEvent =
                    automaticFoamSourceEvents[index];
                if (!sourceEvent.Active)
                {
                    continue;
                }

                if (sourceEvent.Type == AutomaticFoamSourceEventType.ObjectContactArc ||
                    sourceEvent.Type == AutomaticFoamSourceEventType.ObjectContactFleck ||
                    sourceEvent.Type == AutomaticFoamSourceEventType.ObjectContactSemiArc)
                {
                    objectSourceActive = true;
                    break;
                }
            }

            if (objectSourceActive)
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

                    DispatchAutomaticFoamSourceEvent(index, sourceEvent, target);
                    automaticSourceEventsRasterizedLastUpdate++;
                    injectedLastUpdate++;

                    if (sourceEvent.Elapsed >= sourceEvent.Duration - 0.00001f)
                    {
                        CompleteAutomaticObjectContactCycle(sourceEvent);
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
            AutomaticFoamSourceEvent sourceEvent)
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
            float progress = Mathf.Clamp01(
                sourceEvent.Elapsed / Mathf.Max(0.0001f, sourceEvent.Duration));
            float phaseCode = sourceEvent.SideSign;
            if (objectContactCycle)
            {
                float buildEnd = Mathf.Max(0.0001f, sourceEvent.ObjectBuildDuration);
                float holdEnd = buildEnd + Mathf.Max(0f, sourceEvent.ObjectHoldDuration);
                if (sourceEvent.Elapsed < buildEnd)
                {
                    phaseCode = 0f;
                    progress = Mathf.Clamp01(sourceEvent.Elapsed / buildEnd);
                }
                else if (sourceEvent.Elapsed < holdEnd)
                {
                    phaseCode = 1f;
                    progress = sourceEvent.ObjectHoldDuration > 0.0001f
                        ? Mathf.Clamp01(
                            (sourceEvent.Elapsed - buildEnd) /
                            sourceEvent.ObjectHoldDuration)
                        : 1f;
                }
                else
                {
                    phaseCode = 2f;
                    progress = Mathf.Clamp01(
                        (sourceEvent.Elapsed - holdEnd) /
                        Mathf.Max(0.0001f, sourceEvent.ObjectReleaseDuration));
                }
            }

            float materialStepProgress = Mathf.Clamp01(
                (1f / Mathf.Max(1f, ResolveUpdateRate())) /
                Mathf.Max(
                    0.0001f,
                    objectContactCycle
                        ? sourceEvent.ObjectBuildDuration
                        : sourceEvent.Duration));

            return new FoamSourceEventGpuData
            {
                Header = new Vector4(
                    (float)sourceEvent.Type,
                    phaseCode,
                    progress,
                    sourceEvent.ShapeSeed),
                Distance = new Vector4(
                    startStorageGlobal,
                    endStorageGlobal,
                    centreStorageGlobal,
                    river != null && river.FlowDirection >= 0f ? 1f : -1f),
                Shore = new Vector4(
                    sourceEvent.ShoreInsetMetres,
                    sourceEvent.Type == AutomaticFoamSourceEventType.ShoreRibbon
                        ? sourceEvent.ShoreRibbonThicknessCells
                        : (objectContactCycle
                            ? sourceEvent.ObjectWakeArmLengthMetres
                            : sourceEvent.WidthMetres),
                    // Arc/Semi-Arc evaluators do not consume inward reach.
                    // Reuse this source-type-local lane for the normalized
                    // material-step duration so accumulated Build coverage
                    // advances by at least the first raster update.
                    objectContactCycle
                        ? materialStepProgress
                        : sourceEvent.InwardReachMetres,
                    sourceEvent.FeatherMetres),
                Material = new Vector4(
                    sourceEvent.SourceAmount,
                    sourceEvent.RemainingLife,
                    sourceEvent.PatternSeed,
                    sourceEvent.SourceFillFeatureSize),
                Variation = new Vector4(
                    sourceEvent.SourceFillSeed,
                    sourceEvent.BreakupScaleMetres,
                    sourceEvent.BreakupStrength,
                    sourceEvent.Curvature),
                Kinematics = new Vector4(
                    sourceEvent.FormationSpeedMetresPerSecond,
                    sourceEvent.HeadTrailMetres,
                    objectContactCycle
                        ? sourceEvent.ObjectContactPathLengthMetres
                        : Mathf.Sqrt(
                            Mathf.Abs(endStorageGlobal - startStorageGlobal) *
                            Mathf.Abs(endStorageGlobal - startStorageGlobal) +
                            sourceEvent.InwardReachMetres *
                            sourceEvent.InwardReachMetres),
                    Mathf.Clamp01(sourceEvent.SourceFillBlend)),
                ObjectData = new Vector4(
                    sourceEvent.ObjectCentreAcrossMetres,
                    sourceEvent.ObjectAlongHalfLengthMetres,
                    sourceEvent.ObjectAcrossHalfWidthMetres,
                    objectContactCycle
                        ? sourceEvent.ObjectSourceLateralCellSpacingMetres
                        : sourceEvent.ObjectContactOffsetMetres)
            };
        }

        private void DispatchAutomaticFoamSourceEvent(
            int eventIndex,
            AutomaticFoamSourceEvent sourceEvent,
            RenderTexture target)
        {
            float startStorageGlobal =
                WorldGlobalDistanceToFoamStorageGlobalDistance(
                    sourceEvent.StartGlobalDistance);
            float endStorageGlobal =
                WorldGlobalDistanceToFoamStorageGlobalDistance(
                    sourceEvent.EndGlobalDistance);
            float padding = Mathf.Max(
                sourceEvent.FeatherMetres * 2f,
                Mathf.Max(sourceEvent.WidthMetres, sourceEvent.InwardReachMetres) * 1.25f);
            if (sourceEvent.Type == AutomaticFoamSourceEventType.ObjectContactArc ||
                sourceEvent.Type == AutomaticFoamSourceEventType.ObjectContactSemiArc)
            {
                float longitudinalCellSpacing = fieldWidth > 0
                    ? Mathf.Max(0.01f, fieldLength / fieldWidth)
                    : 0.01f;
                // Arc/Semi-Arc start/end bounds already include the complete
                // upstream bridge and straight downstream wake arms. Keep only
                // a small raster safety margin instead of re-expanding by the
                // obstacle half-length on both sides.
                padding = Mathf.Max(
                    padding,
                    longitudinalCellSpacing * 2f);
            }
            else if (sourceEvent.Type == AutomaticFoamSourceEventType.ObjectContactFleck)
            {
                padding = Mathf.Max(
                    padding,
                    Mathf.Max(
                        sourceEvent.ObjectAlongHalfLengthMetres,
                        sourceEvent.ObjectAcrossHalfWidthMetres) +
                    sourceEvent.ObjectContactOffsetMetres +
                    sourceEvent.WidthMetres * 4f +
                    sourceEvent.FeatherMetres * 2f);
            }
            int startX = Mathf.Clamp(
                GlobalDistanceToX(Mathf.Min(startStorageGlobal, endStorageGlobal) - padding) - 2,
                0,
                fieldWidth - 1);
            int endX = Mathf.Clamp(
                GlobalDistanceToX(Mathf.Max(startStorageGlobal, endStorageGlobal) + padding) + 2,
                0,
                fieldWidth - 1);
            int countX = endX - startX + 1;
            if (countX <= 0)
            {
                return;
            }

            int startY = 0;
            int countY = fieldHeight;
            if (sourceEvent.Type == AutomaticFoamSourceEventType.FreeWaterLaceConnector ||
                sourceEvent.Type == AutomaticFoamSourceEventType.FreeWaterTornFragment ||
                sourceEvent.Type == AutomaticFoamSourceEventType.FreeWaterCrossLaceConnector)
            {
                float centreAcross01 = Mathf.Clamp01(
                    sourceEvent.CentreAcrossNormalized * 0.5f + 0.5f);
                int centreY = Mathf.Clamp(
                    Mathf.RoundToInt(centreAcross01 * Mathf.Max(0, fieldHeight - 1)),
                    0,
                    fieldHeight - 1);
                float centreWorldDistance =
                    (sourceEvent.StartGlobalDistance + sourceEvent.EndGlobalDistance) * 0.5f;
                StylizedRiverSplineSample sample = river != null && river.Domain.IsValid
                    ? river.Domain.SampleAtGlobalDistance(centreWorldDistance)
                    : default;
                float visibleHalfWidth = river != null && river.Domain.IsValid
                    ? sample.GetVisibleHalfWidth(
                        sourceEvent.CentreAcrossNormalized < 0f ? -1f : 1f)
                    : 1f;
                float normalizedPad = Mathf.Clamp01(
                    sourceEvent.LateralPaddingMetres / Mathf.Max(0.10f, visibleHalfWidth));
                int padY = Mathf.CeilToInt(normalizedPad * 0.5f * fieldHeight) + 3;
                startY = Mathf.Clamp(centreY - padY, 0, fieldHeight - 1);
                int endY = Mathf.Clamp(centreY + padY, 0, fieldHeight - 1);
                countY = Mathf.Max(1, endY - startY + 1);
            }

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
                writeState == null || boundaryTexture == null ||
                writeIsolatedLifeProbeKernel < 0 || fieldWidth <= 0 ||
                fieldHeight <= 0)
            {
                isolatedLifeProbeAbsoluteAgingActive = false;
                isolatedLifeProbeWrittenAt = -1.0;
                isolatedLifeProbeStatus =
                    "Failed: material resources are not ready";
                return false;
            }

            int patchWidth = Mathf.Clamp(
                Mathf.RoundToInt(fieldWidth * 0.035f),
                3,
                10);
            int patchHeight = Mathf.Clamp(
                Mathf.RoundToInt(fieldHeight * 0.075f),
                3,
                10);
            int gap = Mathf.Clamp(
                Mathf.RoundToInt(fieldWidth * 0.018f),
                2,
                8);
            int step = patchWidth + gap;
            int groupHalfWidth = patchWidth + gap + patchWidth / 2 + 2;

            int centreX = Mathf.RoundToInt(
                Mathf.Clamp01(pendingIsolatedLifeProbeDistanceNormalized) *
                Mathf.Max(0, fieldWidth - 1));
            centreX = Mathf.Clamp(
                centreX,
                groupHalfWidth,
                Mathf.Max(groupHalfWidth, fieldWidth - 1 - groupHalfWidth));

            int centreY = Mathf.RoundToInt(
                Mathf.Clamp01(
                    pendingIsolatedLifeProbeAcrossNormalized * 0.5f + 0.5f) *
                Mathf.Max(0, fieldHeight - 1));
            int halfHeight = Mathf.Max(1, patchHeight / 2);
            centreY = Mathf.Clamp(
                centreY,
                halfHeight + 1,
                Mathf.Max(halfHeight + 1, fieldHeight - halfHeight - 2));

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
            previousState = currentState;
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
            float minimumGlobal;
            float maximumGlobal;
            float storageGlobalDistance =
                WorldGlobalDistanceToFoamStorageGlobalDistance(
                    injection.GlobalDistance);
            float storageSegmentStartGlobalDistance =
                WorldGlobalDistanceToFoamStorageGlobalDistance(
                    injection.SegmentStartGlobalDistance);
            float storageSegmentEndGlobalDistance =
                WorldGlobalDistanceToFoamStorageGlobalDistance(
                    injection.SegmentEndGlobalDistance);

            if (injection.SegmentShape)
            {
                float segmentPadding = Mathf.Max(
                    injection.SegmentStartRadius,
                    injection.SegmentEndRadius);
                minimumGlobal = Mathf.Min(
                    storageSegmentStartGlobalDistance,
                    storageSegmentEndGlobalDistance) - segmentPadding;
                maximumGlobal = Mathf.Max(
                    storageSegmentStartGlobalDistance,
                    storageSegmentEndGlobalDistance) + segmentPadding;
            }
            else
            {
                float alongRadius = injection.Radius * injection.Elongation *
                    (injection.CompoundShape ? 1.25f : 1f);
                minimumGlobal = storageGlobalDistance - alongRadius;
                maximumGlobal = storageGlobalDistance + alongRadius;
            }

            int startX = Mathf.Clamp(
                GlobalDistanceToX(minimumGlobal) - 2,
                0,
                fieldWidth - 1);
            int endX = Mathf.Clamp(
                GlobalDistanceToX(maximumGlobal) + 2,
                0,
                fieldWidth - 1);
            int countX = endX - startX + 1;

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
            computeShader.SetInt("_FoamRangeStart", startX);
            computeShader.SetInt("_FoamRangeCount", countX);
            computeShader.SetFloat("_FoamGlobalStart", river.Domain.GlobalDistanceMinimum);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat("_FoamInjectionGlobalDistance", storageGlobalDistance);
            computeShader.SetFloat("_FoamInjectionAcrossNormalized", injection.AcrossNormalized);
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

            DispatchInjectionToState(target, countX);

            if (injection.IsManual)
            {
                lastInjectionStateSynchronized = true;
                lastInjectionBoundaryCoverage = SampleInjectionBoundaryCoverage(injection);
                simulationInterpolation = 1f;
            }
        }

        private void DispatchInjectionToState(RenderTexture target, int countX)
        {
            if (target == null)
            {
                return;
            }

            computeShader.SetTexture(injectKernel, "_FoamStateWrite", target);
            Dispatch(injectKernel, countX, fieldHeight);
        }

        private float SampleInjectionBoundaryCoverage(PendingInjection injection)
        {
            if (boundaryTexture == null || fieldLength <= 0.0001f)
            {
                return -1f;
            }

            float u = Mathf.Clamp01(
                (injection.GlobalDistance - river.Domain.GlobalDistanceMinimum) /
                fieldLength);
            float v = Mathf.Clamp01(injection.AcrossNormalized * 0.5f + 0.5f);
            return Mathf.Clamp01(boundaryTexture.GetPixelBilinear(u, v).r);
        }
    }
}
