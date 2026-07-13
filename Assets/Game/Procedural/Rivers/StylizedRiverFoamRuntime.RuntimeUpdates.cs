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
        private void HandleDomainChanged(RiverDomainSnapshot _)
        {
            if (!topologyStartupValidationComplete)
            {
                topologyStartupDirtyReasons |=
                    TopologyStartupDirtyReason.DomainChanged;
            }
            resourcesDirty = true;
            boundaryDirty = true;
            if (initializationPhase == InitializationPhase.Failed ||
                initializationPhase ==
                    InitializationPhase.CachePreparationRequired)
            {
                initializationPhase = InitializationPhase.NotStarted;
                topologyStartupValidationComplete = false;
            }
        }

        private void HandleGeneratedSourceAdded(IGeneratedGeometrySource source)
        {
            if (!topologyStartupValidationComplete)
            {
                topologyStartupSourceAddedCount++;
                topologyStartupDirtyReasons |=
                    TopologyStartupDirtyReason.GeneratedSourceAdded;
                RecordGeneratedSourceForStartup(source);
            }
            MarkGeneratedSourceDirty();
        }

        private void HandleGeneratedSourceRemoved(IGeneratedGeometrySource source)
        {
            if (!topologyStartupValidationComplete)
            {
                topologyStartupSourceRemovedCount++;
                topologyStartupDirtyReasons |=
                    TopologyStartupDirtyReason.GeneratedSourceRemoved;
                RecordGeneratedSourceForStartup(source);
            }
            MarkGeneratedSourceDirty();
        }

        private void HandleGeneratedSourceChanged(IGeneratedGeometrySource source)
        {
            if (!topologyStartupValidationComplete)
            {
                topologyStartupSourceChangedCount++;
                topologyStartupDirtyReasons |=
                    TopologyStartupDirtyReason.GeneratedSourceChanged;
                RecordGeneratedSourceForStartup(source);
            }
            MarkGeneratedSourceDirty();
        }

        private void RecordGeneratedSourceForStartup(
            IGeneratedGeometrySource source)
        {
            if (source != null && !topologyStartupValidationComplete)
            {
                topologyStartupDistinctGeneratedSources.Add(source);
            }
        }

        private void MarkGeneratedSourceDirty()
        {
            if (generatedSourceNotificationBurstPending)
            {
                return;
            }

            generatedSourceNotificationBurstPending = true;
            if (!topologyStartupValidationComplete)
            {
                topologyStartupDirtyCycleCount++;
            }
        }

        private float ResolveUpdateRate()
        {
            float qualityRate = river == null
                ? MediumMaterialTemporalUpdateRate
                : river.Quality switch
                {
                    StylizedRiverQuality.Low => LowMaterialTemporalUpdateRate,
                    StylizedRiverQuality.Medium => MediumMaterialTemporalUpdateRate,
                    StylizedRiverQuality.High => HighMaterialTemporalUpdateRate,
                    _ => MediumMaterialTemporalUpdateRate
                };

            // Material cadence remains the accepted Low/Medium/High
            // 8/12/16 Hz contract. Conservative transport derives CFL-safe
            // substeps inside each material tick rather than changing the
            // authored simulation cadence.
            return qualityRate;
        }

        private float ResolveBaseFoamDownstreamSpeedMetresPerSecond()
        {
            if (river == null)
            {
                return 0f;
            }

            return Mathf.Max(
                0f,
                Mathf.Abs(river.FlowSpeedMetresPerSecond) *
                river.LiquidFactor *
                river.FoamDownstreamSpeedRatio);
        }

        private int GlobalDistanceToX(float globalDistance)
        {
            float localDistance =
                globalDistance - river.Domain.GlobalDistanceMinimum;
            return StylizedRiverFoamTopologyFieldSpace
                .LocalDistanceToNearestTexel(
                    localDistance,
                    fieldWidth,
                    fieldLength);
        }

        private void ResetLastUpdateDiagnostics()
        {
            lastUpdateDispatches = 0;
            lastUpdateCellIterations = 0;
            injectedLastUpdate = 0;
            lastMaterialStepsThisFrame = 0;
            birthCommandsThisFrame = 0;
            automaticShoreBirthSubmittedLastUpdate = 0;
            automaticShoreBirthRejectedLastUpdate = 0;
            automaticSourceEventsRasterizedLastUpdate = 0;
        }

        private void UpdateRecentPeaks()
        {
            double now = Time.realtimeSinceStartupAsDouble;
            if (now > recentPeakWindowEnd)
            {
                recentPeakDispatches = lastUpdateDispatches;
                recentPeakCellIterations = lastUpdateCellIterations;
                recentPeakWindowEnd = now + 5.0;
                return;
            }

            recentPeakDispatches = Mathf.Max(
                recentPeakDispatches,
                lastUpdateDispatches);
            recentPeakCellIterations = Math.Max(
                recentPeakCellIterations,
                lastUpdateCellIterations);
        }

        public void ResetSteadyStateWorkAccounting()
        {
            ClearSteadyStateWorkAccounting(true);
        }

        private void ClearSteadyStateWorkAccounting(bool activate)
        {
            steadyStateWorkAccountingGeneration++;
            steadyStateWorkAccountingActive =
                activate && Application.isPlaying;
            steadyStateWorkTopologyMetricGeneration = -1;
            steadyStateWorkTransportMetricGeneration = -1;
            steadyStateWorkAccountingStartedAt =
                Time.realtimeSinceStartupAsDouble;
            steadyStateWorkFrameCount = 0;
            steadyStateWorkVisibleFrameCount = 0;
            steadyStateWorkOffscreenFrameCount = 0;
            steadyStateWorkHeldFrameCount = 0;
            steadyStateWorkMaterialFrameCount = 0;
            steadyStateWorkTotalDispatchCount = 0;
            steadyStateWorkTotalCellIterations = 0;
            steadyStateWorkMaterialStepCount = 0;
            steadyStateWorkTransportSubstepCount = 0;
            steadyStateWorkMaximumTransportSubsteps = 0;
            steadyStateWorkMaximumTransportCfl = 0f;
            steadyStateWorkMaterialDispatchCount = 0;
            steadyStateWorkMaterialCellIterations = 0;
            steadyStateWorkMaterialCpuMilliseconds = 0.0;
            steadyStateWorkEmptyMaterialStepCount = 0;
            steadyStateWorkTopologyDirtyEvaluationCount = 0;
            steadyStateWorkTopologyDirtyPositiveCount = 0;
            steadyStateWorkTopologyMaintenanceCount = 0;
            steadyStateWorkTopologyEvolutionCount = 0;
            steadyStateWorkTopologyEvolutionCpuMilliseconds = 0.0;
            steadyStateWorkTopologyRefreshCount = 0;
            steadyStateWorkTopologyDispatchCount = 0;
            steadyStateWorkTopologyCellIterations = 0;
            steadyStateWorkTopologyCpuMilliseconds = 0.0;
            steadyStateWorkShapeEvaluationCount = 0;
            steadyStateWorkShapeDispatchCount = 0;
            steadyStateWorkShapeCellIterations = 0;
            steadyStateWorkShapeCpuMilliseconds = 0.0;
            steadyStateWorkTopologyMetricRequestCount = 0;
            steadyStateWorkTopologyMetricCompletionCount = 0;
            steadyStateWorkTopologyMetricErrorCount = 0;
            steadyStateWorkTopologyMetricTimeoutCount = 0;
            steadyStateWorkTransportMetricRequestCount = 0;
            steadyStateWorkTransportMetricCompletionCount = 0;
            steadyStateWorkTransportMetricErrorCount = 0;
        }

        public void LogSteadyStateWorkSummary()
        {
            Debug.Log(
                $"[River Foam P4] Work '{name}': " +
                BuildSteadyStateWorkSummary(),
                this);
        }

        private void RecordSteadyStateWorkFrame(
            bool materialWork,
            bool stateHeld)
        {
            if (!steadyStateWorkAccountingActive)
            {
                return;
            }

            steadyStateWorkFrameCount++;
            if (surfaceRenderer != null && surfaceRenderer.isVisible)
            {
                steadyStateWorkVisibleFrameCount++;
            }
            else
            {
                steadyStateWorkOffscreenFrameCount++;
            }

            if (materialWork)
            {
                steadyStateWorkMaterialFrameCount++;
            }
            if (stateHeld)
            {
                steadyStateWorkHeldFrameCount++;
            }
        }

        private static long CaptureWorkTimestamp()
        {
            return System.Diagnostics.Stopwatch.GetTimestamp();
        }

        private static double ResolveWorkMilliseconds(long startedAt)
        {
            long elapsed = System.Diagnostics.Stopwatch.GetTimestamp() -
                startedAt;
            return elapsed * 1000.0 /
                System.Diagnostics.Stopwatch.Frequency;
        }

        private void RecordMaterialSimulationWork(
            long startedAt,
            int dispatchesBefore,
            long cellIterationsBefore,
            int transportSubsteps)
        {
            if (!steadyStateWorkAccountingActive)
            {
                return;
            }

            steadyStateWorkMaterialStepCount++;
            steadyStateWorkTransportSubstepCount +=
                Mathf.Max(0, transportSubsteps);
            steadyStateWorkMaximumTransportSubsteps = Mathf.Max(
                steadyStateWorkMaximumTransportSubsteps,
                transportSubsteps);
            steadyStateWorkMaximumTransportCfl = Mathf.Max(
                steadyStateWorkMaximumTransportCfl,
                lastMaximumTransportCfl /
                Mathf.Max(1, transportSubsteps));
            steadyStateWorkMaterialDispatchCount +=
                Mathf.Max(0, lastUpdateDispatches - dispatchesBefore);
            steadyStateWorkMaterialCellIterations += Math.Max(
                0L,
                lastUpdateCellIterations - cellIterationsBefore);
            steadyStateWorkMaterialCpuMilliseconds +=
                ResolveWorkMilliseconds(startedAt);

            if (topologyMetricsAvailable && TopologyMetricsFresh &&
                integratedPresenceArea <= 0.0001f &&
                visiblePresenceCoreArea <= 0.0001f)
            {
                steadyStateWorkEmptyMaterialStepCount++;
            }
        }

        private void RecordTopologyRefreshWork(
            long startedAt,
            int dispatchesBefore,
            long cellIterationsBefore)
        {
            if (!steadyStateWorkAccountingActive)
            {
                return;
            }

            steadyStateWorkTopologyRefreshCount++;
            steadyStateWorkTopologyDispatchCount +=
                Mathf.Max(0, lastUpdateDispatches - dispatchesBefore);
            steadyStateWorkTopologyCellIterations += Math.Max(
                0L,
                lastUpdateCellIterations - cellIterationsBefore);
            steadyStateWorkTopologyCpuMilliseconds +=
                ResolveWorkMilliseconds(startedAt);
        }

        private void RecordTopologyMaintenanceWork(
            long startedAt,
            int dispatchesBefore,
            long cellIterationsBefore)
        {
            if (!steadyStateWorkAccountingActive)
            {
                return;
            }

            steadyStateWorkTopologyMaintenanceCount++;
            steadyStateWorkTopologyDispatchCount +=
                Mathf.Max(0, lastUpdateDispatches - dispatchesBefore);
            steadyStateWorkTopologyCellIterations += Math.Max(
                0L,
                lastUpdateCellIterations - cellIterationsBefore);
            steadyStateWorkTopologyCpuMilliseconds +=
                ResolveWorkMilliseconds(startedAt);
        }

        private void RecordTopologyEvolutionWork(long startedAt)
        {
            if (!steadyStateWorkAccountingActive)
            {
                return;
            }

            steadyStateWorkTopologyEvolutionCount++;
            steadyStateWorkTopologyEvolutionCpuMilliseconds +=
                ResolveWorkMilliseconds(startedAt);
        }

        private void RecordShapeEvaluationWork(
            long startedAt,
            int dispatchesBefore,
            long cellIterationsBefore)
        {
            if (!steadyStateWorkAccountingActive)
            {
                return;
            }

            steadyStateWorkShapeEvaluationCount++;
            steadyStateWorkShapeDispatchCount +=
                Mathf.Max(0, lastUpdateDispatches - dispatchesBefore);
            steadyStateWorkShapeCellIterations += Math.Max(
                0L,
                lastUpdateCellIterations - cellIterationsBefore);
            steadyStateWorkShapeCpuMilliseconds +=
                ResolveWorkMilliseconds(startedAt);
        }

        private string BuildSteadyStateWorkSummary()
        {
            double elapsed = SteadyStateWorkElapsedSeconds;
            double inverseElapsed = elapsed > 0.0001 ? 1.0 / elapsed : 0.0;
            double averageSubsteps = steadyStateWorkMaterialStepCount > 0
                ? steadyStateWorkTransportSubstepCount /
                  (double)steadyStateWorkMaterialStepCount
                : 0.0;
            double emptyPercent = steadyStateWorkMaterialStepCount > 0
                ? steadyStateWorkEmptyMaterialStepCount * 100.0 /
                  steadyStateWorkMaterialStepCount
                : 0.0;
            double visiblePercent = steadyStateWorkFrameCount > 0
                ? steadyStateWorkVisibleFrameCount * 100.0 /
                  steadyStateWorkFrameCount
                : 0.0;
            double topologyCpuMilliseconds =
                steadyStateWorkTopologyCpuMilliseconds +
                steadyStateWorkTopologyEvolutionCpuMilliseconds;

            return
                $"window={elapsed:0.000}s, " +
                $"frames={steadyStateWorkFrameCount:N0} " +
                $"(visible:{steadyStateWorkVisibleFrameCount:N0}/" +
                $"offscreen:{steadyStateWorkOffscreenFrameCount:N0}/" +
                $"{visiblePercent:0.0}% visible, " +
                $"material-active:{steadyStateWorkMaterialFrameCount:N0}), " +
                $"dispatches={steadyStateWorkTotalDispatchCount:N0} " +
                $"({steadyStateWorkTotalDispatchCount * inverseElapsed:0.00}/s), " +
                $"cells={steadyStateWorkTotalCellIterations:N0} " +
                $"({steadyStateWorkTotalCellIterations * inverseElapsed:0.00}/s), " +
                $"material={steadyStateWorkMaterialStepCount:N0} steps " +
                $"({steadyStateWorkMaterialStepCount * inverseElapsed:0.00}/s), " +
                $"substeps=avg:{averageSubsteps:0.00}/max:" +
                $"{steadyStateWorkMaximumTransportSubsteps}, " +
                $"CFL/substep max={steadyStateWorkMaximumTransportCfl:0.000}, " +
                $"materialWork=dispatch:{steadyStateWorkMaterialDispatchCount:N0}/" +
                $"cells:{steadyStateWorkMaterialCellIterations:N0}/" +
                $"CPU-submit:{steadyStateWorkMaterialCpuMilliseconds:0.000}ms, " +
                $"topology=maintenance:{steadyStateWorkTopologyMaintenanceCount:N0}/" +
                $"refresh:{steadyStateWorkTopologyRefreshCount:N0}/" +
                $"evolutionChecks:{steadyStateWorkTopologyEvolutionCount:N0}/" +
                $"dispatch:{steadyStateWorkTopologyDispatchCount:N0}/" +
                $"cells:{steadyStateWorkTopologyCellIterations:N0}/" +
                $"CPU-submit:{topologyCpuMilliseconds:0.000}ms, " +
                $"shape=eval:{steadyStateWorkShapeEvaluationCount:N0}/" +
                $"dispatch:{steadyStateWorkShapeDispatchCount:N0}/" +
                $"cells:{steadyStateWorkShapeCellIterations:N0}/" +
                $"CPU-submit:{steadyStateWorkShapeCpuMilliseconds:0.000}ms, " +
                $"dirtyChecks={steadyStateWorkTopologyDirtyEvaluationCount:N0}/" +
                $"positive:{steadyStateWorkTopologyDirtyPositiveCount:N0}, " +
                $"metrics=topology:{steadyStateWorkTopologyMetricRequestCount:N0}/" +
                $"{steadyStateWorkTopologyMetricCompletionCount:N0}/" +
                $"err:{steadyStateWorkTopologyMetricErrorCount:N0}/" +
                $"timeout:{steadyStateWorkTopologyMetricTimeoutCount:N0}, " +
                $"transport:{steadyStateWorkTransportMetricRequestCount:N0}/" +
                $"{steadyStateWorkTransportMetricCompletionCount:N0}/" +
                $"err:{steadyStateWorkTransportMetricErrorCount:N0}, " +
                $"emptySteps={steadyStateWorkEmptyMaterialStepCount:N0} " +
                $"({emptyPercent:0.0}%, latest-metric qualified), " +
                $"heldFrames={steadyStateWorkHeldFrameCount:N0}.";
        }
    }
}
