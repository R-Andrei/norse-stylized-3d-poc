using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private bool IsProgressiveBirthSourceDebugActive =>
            river != null &&
            river.FoamDebugView ==
                StylizedRiverFoamDebugView.ProgressiveBirthSource;

        private void ResetProgressiveBirthDiagnosticSession()
        {
            foamCompositionEventUpdateCount = 0;
            foamCompositionSegmentDispatchAttemptCount = 0;
            foamCompositionSegmentDispatchSubmittedCount = 0;
            foamCompositionCumulativeCentrelineDistance = 0f;
            progressiveBirthDebugLatestAffectedTexels = 0;
            progressiveBirthDebugCumulativeAffectedTexels = 0;
            progressiveBirthDebugReadbackAvailable = false;
            progressiveBirthDebugSessionGeneration++;
            progressiveBirthDebugResetPending = true;
        }

        private void EnsureProgressiveBirthDiagnosticResources()
        {
            if (!IsProgressiveBirthSourceDebugActive ||
                computeShader == null ||
                fieldWidth <= 0 ||
                fieldHeight <= 0)
            {
                return;
            }

            if (progressiveBirthDebugTexture == null ||
                !progressiveBirthDebugTexture.IsCreated())
            {
                ReleaseTexture(ref progressiveBirthDebugTexture);
                progressiveBirthDebugTexture = CreateFieldTexture(
                    "PS3D_RiverFoam_ProgressiveBirthDebug");
                ClearRenderTexture(progressiveBirthDebugTexture);
                progressiveBirthDebugResetPending = true;
            }

            if (progressiveBirthDebugCounterBuffer == null)
            {
                progressiveBirthDebugCounterBuffer = new ComputeBuffer(
                    ProgressiveBirthDebugCounterCount,
                    sizeof(uint),
                    ComputeBufferType.Structured);
                System.Array.Clear(
                    progressiveBirthDebugCounterReadback,
                    0,
                    progressiveBirthDebugCounterReadback.Length);
                progressiveBirthDebugCounterBuffer.SetData(
                    progressiveBirthDebugCounterReadback);
                progressiveBirthDebugResourceGeneration++;
            }
        }

        private void BeginProgressiveBirthDebugStep()
        {
            if (!IsProgressiveBirthSourceDebugActive)
            {
                return;
            }

            EnsureProgressiveBirthDiagnosticResources();
            if (progressiveBirthDebugTexture == null ||
                progressiveBirthDebugCounterBuffer == null)
            {
                return;
            }

            int kernel = progressiveBirthDebugResetPending
                ? clearProgressiveBirthDebugAllKernel
                : clearProgressiveBirthDebugTransientKernel;
            if (kernel < 0)
            {
                return;
            }

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetInt("_FoamRangeStart", 0);
            computeShader.SetInt("_FoamRangeCount", fieldWidth);
            computeShader.SetTexture(
                kernel,
                "_FoamBirthDebugWrite",
                progressiveBirthDebugTexture);
            computeShader.SetBuffer(
                kernel,
                "_FoamBirthDebugCounters",
                progressiveBirthDebugCounterBuffer);
            Dispatch(kernel, fieldWidth, fieldHeight);

            if (progressiveBirthDebugResetPending)
            {
                progressiveBirthDebugResetPending = false;
                progressiveBirthDebugLatestAffectedTexels = 0;
                progressiveBirthDebugCumulativeAffectedTexels = 0;
                progressiveBirthDebugReadbackAvailable = false;
            }
        }

        private void EndProgressiveBirthDebugStep()
        {
            if (!IsProgressiveBirthSourceDebugActive ||
                progressiveBirthDebugCounterBuffer == null)
            {
                return;
            }

            RequestProgressiveBirthDebugReadback();
        }

        private void PrepareProgressiveBirthDebugEvent(
            ref FoamCompositionEvent ribbonEvent)
        {
            if (!IsProgressiveBirthSourceDebugActive ||
                !ribbonEvent.DebugTrajectoryPending)
            {
                return;
            }

            EnsureProgressiveBirthDiagnosticResources();
            if (progressiveBirthDebugTexture == null ||
                progressiveBirthDebugCounterBuffer == null)
            {
                return;
            }

            PaintPlannedProgressiveBirthTrajectory(ribbonEvent);
            ribbonEvent.DebugTrajectoryPending = false;
        }

        private void PaintPlannedProgressiveBirthTrajectory(
            FoamCompositionEvent ribbonEvent)
        {
            float longitudinalTexelSize = fieldWidth > 0
                ? simulationFieldLength / fieldWidth
                : 0.25f;
            int segmentCount = Mathf.Clamp(
                Mathf.CeilToInt(
                    ribbonEvent.TravelDistance /
                    Mathf.Max(0.05f, longitudinalTexelSize * 0.5f)),
                8,
                128);

            for (int segmentIndex = 0;
                 segmentIndex < segmentCount;
                 segmentIndex++)
            {
                float startProgress = segmentIndex /
                    (float)segmentCount;
                float endProgress = (segmentIndex + 1) /
                    (float)segmentCount;

                ResolveFoamCompositionHead(
                    ribbonEvent,
                    startProgress,
                    out float startGlobalDistance,
                    out float startAcrossNormalized);
                ResolveFoamCompositionHead(
                    ribbonEvent,
                    endProgress,
                    out float endGlobalDistance,
                    out float endAcrossNormalized);

                float startEnvelope =
                    ResolveProgressiveRibbonEnvelope(startProgress);
                float endEnvelope =
                    ResolveProgressiveRibbonEnvelope(endProgress);
                float startRadius = ResolveProgressiveRibbonRadius(
                    ribbonEvent.BaseRadius,
                    startProgress,
                    ribbonEvent.WidthPhase,
                    startEnvelope,
                    ribbonEvent.WidthVariation);
                float endRadius = ResolveProgressiveRibbonRadius(
                    ribbonEvent.BaseRadius,
                    endProgress,
                    ribbonEvent.WidthPhase,
                    endEnvelope,
                    ribbonEvent.WidthVariation);
                float startAmount = ribbonEvent.SourceAmount * startEnvelope;
                float endAmount = ribbonEvent.SourceAmount * endEnvelope;

                PendingInjection segment =
                    CreateProgressiveBirthDiagnosticSegment(
                        ribbonEvent,
                        startGlobalDistance,
                        startAcrossNormalized,
                        startRadius,
                        startAmount,
                        endGlobalDistance,
                        endAcrossNormalized,
                        endRadius,
                        endAmount);
                DispatchProgressiveBirthDebugSegment(segment, true);
            }
        }

        private void PaintProgressiveBirthDebugSegment(
            PendingInjection segment)
        {
            if (!IsProgressiveBirthSourceDebugActive)
            {
                return;
            }

            EnsureProgressiveBirthDiagnosticResources();
            DispatchProgressiveBirthDebugSegment(segment, false);
        }

        private PendingInjection CreateProgressiveBirthDiagnosticSegment(
            FoamCompositionEvent ribbonEvent,
            float startGlobalDistance,
            float startAcrossNormalized,
            float startRadius,
            float startAmount,
            float endGlobalDistance,
            float endAcrossNormalized,
            float endRadius,
            float endAmount)
        {
            return new PendingInjection(
                (startGlobalDistance + endGlobalDistance) * 0.5f,
                (startAcrossNormalized + endAcrossNormalized) * 0.5f,
                Mathf.Max(startRadius, endRadius),
                Mathf.Max(startAmount, endAmount),
                ribbonEvent.RemainingLife,
                ribbonEvent.PatternSeed,
                1f,
                false,
                ribbonEvent.SourceFillSeed,
                ribbonEvent.SourceFillFeatureSize,
                ribbonEvent.ShapeSeed,
                ribbonEvent.FragmentStrength,
                false,
                true,
                startGlobalDistance,
                startAcrossNormalized,
                startRadius,
                startAmount,
                endGlobalDistance,
                endAcrossNormalized,
                endRadius,
                endAmount,
                ribbonEvent.SheetStyle,
                ribbonEvent.Pattern,
                ribbonEvent.Complexity,
                ribbonEvent.Density);
        }

        private void DispatchProgressiveBirthDebugSegment(
            PendingInjection segment,
            bool plannedTrajectory)
        {
            if (computeShader == null ||
                progressiveBirthDebugTexture == null ||
                progressiveBirthDebugCounterBuffer == null ||
                paintProgressiveBirthDebugSegmentKernel < 0 ||
                obstacleExclusionTexture == null ||
                !segment.SegmentShape)
            {
                return;
            }

            float segmentPadding = Mathf.Max(
                segment.SegmentStartRadius,
                segment.SegmentEndRadius);
            float minimumGlobal = Mathf.Min(
                segment.SegmentStartGlobalDistance,
                segment.SegmentEndGlobalDistance) - segmentPadding;
            float maximumGlobal = Mathf.Max(
                segment.SegmentStartGlobalDistance,
                segment.SegmentEndGlobalDistance) + segmentPadding;
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
            computeShader.SetFloat(
                "_FoamGlobalStart",
                river.Domain.GlobalDistanceMinimum);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat(
                "_FoamInjectionSourceFillSeed",
                segment.SourceFillSeed);
            computeShader.SetFloat(
                "_FoamInjectionSourceFillFeatureSize",
                segment.SourceFillFeatureSize);
            computeShader.SetFloat(
                "_FoamInjectionShapeSeed",
                segment.ShapeSeed);
            computeShader.SetFloat(
                "_FoamInjectionShapeVariety",
                segment.ShapeVariety);
            computeShader.SetFloat(
                "_FoamInjectionCompound",
                segment.SegmentSheetStyle ? 1f : 0f);
            computeShader.SetFloat(
                "_FoamInjectionCompositionPattern",
                (float)segment.CompositionPattern);
            computeShader.SetFloat(
                "_FoamInjectionCompositionComplexity",
                segment.CompositionComplexity);
            computeShader.SetFloat(
                "_FoamInjectionCompositionDensity",
                segment.CompositionDensity);
            computeShader.SetFloat(
                "_FoamInjectionSegmentStartGlobalDistance",
                segment.SegmentStartGlobalDistance);
            computeShader.SetFloat(
                "_FoamInjectionSegmentStartAcrossNormalized",
                segment.SegmentStartAcrossNormalized);
            computeShader.SetFloat(
                "_FoamInjectionSegmentStartRadius",
                segment.SegmentStartRadius);
            computeShader.SetFloat(
                "_FoamInjectionSegmentStartAmount",
                segment.SegmentStartSourceAmount);
            computeShader.SetFloat(
                "_FoamInjectionSegmentEndGlobalDistance",
                segment.SegmentEndGlobalDistance);
            computeShader.SetFloat(
                "_FoamInjectionSegmentEndAcrossNormalized",
                segment.SegmentEndAcrossNormalized);
            computeShader.SetFloat(
                "_FoamInjectionSegmentEndRadius",
                segment.SegmentEndRadius);
            computeShader.SetFloat(
                "_FoamInjectionSegmentEndAmount",
                segment.SegmentEndSourceAmount);
            computeShader.SetFloat(
                "_FoamBirthDebugPaintMode",
                plannedTrajectory ? 1f : 0f);
            computeShader.SetBuffer(
                paintProgressiveBirthDebugSegmentKernel,
                "_FoamMetricRows",
                metricBuffer);
            computeShader.SetTexture(
                paintProgressiveBirthDebugSegmentKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetTexture(
                paintProgressiveBirthDebugSegmentKernel,
                "_FoamObstacleExclusionRead",
                obstacleExclusionTexture);
            computeShader.SetTexture(
                paintProgressiveBirthDebugSegmentKernel,
                "_FoamBirthDebugWrite",
                progressiveBirthDebugTexture);
            computeShader.SetBuffer(
                paintProgressiveBirthDebugSegmentKernel,
                "_FoamBirthDebugCounters",
                progressiveBirthDebugCounterBuffer);
            Dispatch(
                paintProgressiveBirthDebugSegmentKernel,
                countX,
                fieldHeight);
        }

        private void RequestProgressiveBirthDebugReadback()
        {
            if (progressiveBirthDebugReadbackPending ||
                progressiveBirthDebugCounterBuffer == null)
            {
                return;
            }

            if (!SystemInfo.supportsAsyncGPUReadback)
            {
                progressiveBirthDebugCounterBuffer.GetData(
                    progressiveBirthDebugCounterReadback);
                ApplyProgressiveBirthDebugReadback(
                    progressiveBirthDebugCounterReadback);
                return;
            }

            progressiveBirthDebugReadbackPending = true;
            int generation = progressiveBirthDebugResourceGeneration;
            int sessionGeneration =
                progressiveBirthDebugSessionGeneration;
            ComputeBuffer requestedBuffer =
                progressiveBirthDebugCounterBuffer;
            AsyncGPUReadback.Request(
                requestedBuffer,
                request =>
                {
                    if (this == null ||
                        generation !=
                            progressiveBirthDebugResourceGeneration)
                    {
                        requestedBuffer?.Release();
                        return;
                    }

                    progressiveBirthDebugReadbackPending = false;
                    if (sessionGeneration !=
                        progressiveBirthDebugSessionGeneration)
                    {
                        return;
                    }
                    if (request.hasError)
                    {
                        progressiveBirthDebugReadbackAvailable = false;
                        return;
                    }

                    var data = request.GetData<uint>();
                    int count = Mathf.Min(
                        data.Length,
                        progressiveBirthDebugCounterReadback.Length);
                    for (int index = 0; index < count; index++)
                    {
                        progressiveBirthDebugCounterReadback[index] =
                            data[index];
                    }

                    ApplyProgressiveBirthDebugReadback(
                        progressiveBirthDebugCounterReadback);
                });
        }

        private void ApplyProgressiveBirthDebugReadback(uint[] data)
        {
            if (data == null || data.Length <
                ProgressiveBirthDebugCounterCount)
            {
                progressiveBirthDebugReadbackAvailable = false;
                return;
            }

            progressiveBirthDebugLatestAffectedTexels = data[0];
            progressiveBirthDebugCumulativeAffectedTexels = data[1];
            progressiveBirthDebugReadbackAvailable = true;
        }

        private void ReleaseProgressiveBirthDiagnosticResources()
        {
            ReleaseTexture(ref progressiveBirthDebugTexture);
            progressiveBirthDebugResourceGeneration++;

            if (progressiveBirthDebugReadbackPending)
            {
                // The outstanding request callback owns this retired buffer
                // until the GPU copy completes.
                progressiveBirthDebugCounterBuffer = null;
            }
            else
            {
                progressiveBirthDebugCounterBuffer?.Release();
                progressiveBirthDebugCounterBuffer = null;
            }

            progressiveBirthDebugReadbackPending = false;
            progressiveBirthDebugReadbackAvailable = false;
            progressiveBirthDebugLatestAffectedTexels = 0;
            progressiveBirthDebugCumulativeAffectedTexels = 0;
            progressiveBirthDebugResetPending = true;
        }
    }
}
