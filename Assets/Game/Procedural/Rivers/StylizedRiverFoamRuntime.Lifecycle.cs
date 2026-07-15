using System;
using ProgrammaticStylized3D.Geometry;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private void OnEnable()
        {
            hideFlags = HideFlags.HideInInspector;
            river = GetComponent<StylizedRiver>();
            surfaceRenderer = river != null ? river.SurfaceRenderer : null;

            if (river != null)
            {
                river.DomainChanged += HandleDomainChanged;
            }

            GeneratedGeometryRegistry.SourceAdded += HandleGeneratedSourceAdded;
            GeneratedGeometryRegistry.SourceRemoved += HandleGeneratedSourceRemoved;
            GeneratedGeometryRegistry.SourceChanged += HandleGeneratedSourceChanged;

            lastRuntimeTime = Time.realtimeSinceStartup;
            resourcesDirty = true;
            boundaryDirty = true;
            rebuildPhase = RebuildPhase.Idle;
            pendingBoundaryRebuild = false;
            pendingObstacleRebuild = false;
            pendingTopologyRefresh = false;
            pendingTopologyReplacementAfterMaintenance = false;
            pendingObstacleObservedVersion = int.MinValue;
            pendingObstacleStableFrameCount = 0;
            initializationObstacleObservedVersion = int.MinValue;
            initializationObstacleStableFrameCount = 0;
            editorTopologyPreparationInProgress = false;
            activeTopologyRequiresExplicitPreparation = false;
            generatedSourceNotificationBurstPending = false;
            topologyCacheStartupOutcome = TopologyCacheStartupOutcome.NotEvaluated;
            topologyCacheStartupReasons = TopologyCacheStartupReason.None;
            topologyCacheStartupAttemptCount = 0;
            topologyCacheStartupHitCount = 0;
            topologyCacheStartupMissCount = 0;
            topologyCacheStartupPayloadBytes = 0;
            topologyCacheStartupPayloadHash = "—";
            topologyCacheStartupLoadMilliseconds = 0.0;
            topologyCacheLoadedForActiveResources = false;
            automaticTopologyCacheWritePending = false;
            automaticTopologyCacheWriteCount = 0;
            automaticTopologyCacheWriteSuccessCount = 0;
            automaticTopologyCachePersistenceState = "Disabled";
            automaticTopologyCachePersistenceSummary =
                "Play Mode cache persistence is disabled. Prepare the cache " +
                "explicitly in Edit Mode.";
            topologyStartupSourceAddedCount = 0;
            topologyStartupSourceRemovedCount = 0;
            topologyStartupSourceChangedCount = 0;
            topologyStartupDirtyCycleCount = 0;
            topologyStartupRestartCount = 0;
            topologyStartupCacheBuildAttemptCount = 0;
            topologyStartupReplacementAttemptCount = 0;
            topologyStartupCacheWriteAttemptCount = 0;
            topologyStartupCacheWriteSuccessCount = 0;
            topologyStartupDirtyReasons = TopologyStartupDirtyReason.None;
            topologyStartupDistinctGeneratedSources.Clear();
            topologyStartupSummaryLogged = false;
            ClearSteadyStateWorkAccounting(false);
            pendingStartupCacheStaleObstacles = false;
            pendingStartupCacheStaleSettings = false;
            topologyStartupValidationActive = false;
            topologyStartupValidationComplete = false;
            ReleaseTopologyReplacementBuild(true);
            ReleaseTopologyTransition(true);
            initializationPhase = InitializationPhase.NotStarted;
            BindDisabled();
        }

        private void OnDisable()
        {
            if (river != null)
            {
                river.DomainChanged -= HandleDomainChanged;
            }

            GeneratedGeometryRegistry.SourceAdded -= HandleGeneratedSourceAdded;
            GeneratedGeometryRegistry.SourceRemoved -= HandleGeneratedSourceRemoved;
            GeneratedGeometryRegistry.SourceChanged -= HandleGeneratedSourceChanged;

            BindDisabled();
            ReleaseResources();
            pendingInjections.Clear();
            pendingMaterialBirths.Clear();
            pendingIsolatedLifeProbe = false;
            pendingIsolatedLifeProbeAbsoluteAging = false;
            isolatedLifeProbeAbsoluteAgingActive = false;
            isolatedLifeProbeWrittenAt = -1.0;
            ClearFoamCompositionEvents();
            steadyStateWorkAccountingActive = false;
        }

        private void OnDestroy()
        {
            ClearFoamCompositionEvents();
            BindDisabled();
            ReleaseResources();
        }

        private void LateUpdate()
        {
            using var profilerScope = LateUpdateProfilerMarker.Auto();
            ResetLastUpdateDiagnostics();
            ReleaseRetiredTopologyTransitionResourcesIfReady();
            ReleaseRetiredGeneratedTopologyTextureIfReady();

            if (river == null)
            {
                river = GetComponent<StylizedRiver>();
            }

            if (river == null ||
                !river.isActiveAndEnabled ||
                !river.FoamEnabled)
            {
                BindDisabled();
                ReleaseResources();
                pendingInjections.Clear();
                pendingIsolatedLifeProbe = false;
                ClearFoamCompositionEvents();
                ResetManualInjectionSequence();
                return;
            }

            surfaceRenderer = river.SurfaceRenderer;

            if (!Application.isPlaying)
            {
                BindDisabled();
                return;
            }

            if (!IsSupported)
            {
                if (!supportWarningReported)
                {
                    Debug.LogWarning(
                        $"Stage 6 Foam on '{name}' is disabled because compute shaders, texture arrays, or required half-float texture formats are unavailable.",
                        this);
                    supportWarningReported = true;
                }

                BindDisabled();
                return;
            }

            supportWarningReported = false;

            bool fullyFrozen = river.FreezeAmount >= 0.999f;
            if (fullyFrozen)
            {
                pendingInjections.Clear();
                pendingIsolatedLifeProbe = false;
                ClearFoamCompositionEvents();
                ResetManualInjectionSequence();

                if (!fullyFrozenLastUpdate)
                {
                    ClearFoam();
                }

                fullyFrozenLastUpdate = true;
                BindDisabled();
                ReleaseResources();
                return;
            }

            fullyFrozenLastUpdate = false;

            bool topologyDebugActive = IsTopologyDebugActive;
            bool automaticBirthDebugActive =
                IsAutomaticBirthSourcesDebugActive;
            if (automaticBirthDebugActive &&
                !automaticBirthDebugActiveLastUpdate)
            {
                ResetAutomaticBirthDiagnosticSession();
            }
            automaticBirthDebugActiveLastUpdate =
                automaticBirthDebugActive;
            bool motionFieldDebugActive = IsMotionFieldDebugActive;
            bool shapeProductDebugActive = IsShapeProductDebugActive;
            bool shapeProductDebugEnteredThisUpdate =
                shapeProductDebugActive &&
                !shapeProductDebugActiveLastUpdate;
            if (shapeProductDebugEnteredThisUpdate &&
                !river.FoamStateHeld)
            {
                ClearRenderTexture(shapeMaskTexture);
                ClearRenderTexture(filmSourceTexture);
                ClearRenderTexture(filmSupportTexture);
                ClearRenderTexture(visualOccupancyA);
                ClearRenderTexture(visualOccupancyB);
                currentVisualOccupancy = visualOccupancyA;
                writeVisualOccupancy = visualOccupancyB;
            }
            shapeProductDebugActiveLastUpdate = shapeProductDebugActive;
            // Patch 4.11C.5.4d: no liveness scheduler remains. Once the
            // runtime owns allocated material textures, the lifecycle ticks the
            // full field every material step until an explicit ClearFoam,
            // disable, rebuild, or resource release. Metrics may observe
            // emptiness, but they no longer get to stop aging.
            bool materialWork =
                currentState != null ||
                materialLifetimeAuthorityActive ||
                pendingInjections.Count > 0 ||
                pendingMaterialBirths.Count > 0 ||
                pendingIsolatedLifeProbe ||
                activeFoamCompositionEventCount > 0 ||
                IsAutomaticSourcePopulationActive;
            bool hasWork = materialWork || topologyDebugActive ||
                automaticBirthDebugActive || motionFieldDebugActive ||
                shapeProductDebugActive;

            if (!hasWork && currentState == null &&
                !HasTopologyTransitionVisibleHold)
            {
                BindDisabled();
                return;
            }

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();

            if (!EnsureResources())
            {
                if (!BindTopologyTransitionHold())
                {
                    BindDisabled();
                }
                return;
            }

            if (river.FoamStateHeld)
            {
                // Exact-state rendering comparison: keep every allocated Foam
                // product untouched, discard elapsed wall time, and continue
                // binding the current textures plus live Layer E properties.
                // Queued injections and births remain pending until release.
                lastRuntimeTime = Time.realtimeSinceStartup;
                simulationAccumulator = 0f;
                simulationInterpolation = 1f;
                lastRenderInterpolationAlpha = simulationInterpolation;
                idleSince = 0.0;
                RecordSteadyStateWorkFrame(materialWork, true);
                BindField();
                UpdateRecentPeaks();
                return;
            }

            bool recordTopologyMaintenance =
                steadyStateWorkAccountingActive;
            long topologyMaintenanceStartedAt = recordTopologyMaintenance
                ? CaptureWorkTimestamp()
                : 0L;
            int topologyMaintenanceDispatchesBefore = recordTopologyMaintenance
                ? lastUpdateDispatches
                : 0;
            long topologyMaintenanceCellIterationsBefore =
                recordTopologyMaintenance
                    ? lastUpdateCellIterations
                    : 0L;
            bool queuedWorkBeforeDirtyEvaluation = HasQueuedRebuildWork;
            if (steadyStateWorkAccountingActive)
            {
                steadyStateWorkTopologyDirtyEvaluationCount += 2;
            }
            QueueObstacleRebuildIfNeeded();
            QueueMajorTopologyRebuildIfNeeded();
            if (steadyStateWorkAccountingActive &&
                !queuedWorkBeforeDirtyEvaluation && HasQueuedRebuildWork)
            {
                steadyStateWorkTopologyDirtyPositiveCount++;
            }
            bool rebuildPhaseAdvanced = AdvanceQueuedRebuild();
            bool topologyReplacementActivated = false;
            if (!rebuildPhaseAdvanced && !HasQueuedRebuildWork)
            {
                topologyReplacementActivated =
                    AdvanceTopologyReplacementBuild();
            }
            bool topologyMaintenanceBlocked =
                rebuildPhaseAdvanced || HasQueuedRebuildWork ||
                topologyReplacementActivated;
            if (recordTopologyMaintenance)
            {
                RecordTopologyMaintenanceWork(
                    topologyMaintenanceStartedAt,
                    topologyMaintenanceDispatchesBefore,
                    topologyMaintenanceCellIterationsBefore);
            }

            float now = Time.realtimeSinceStartup;
            float deltaTime = Mathf.Clamp(now - lastRuntimeTime, 0f, 0.1f);
            lastRuntimeTime = now;
            AdvanceMotionLaneScroll(deltaTime);
            EnsureMotionFieldsCurrent();

            bool recordTopologyEvolution =
                steadyStateWorkAccountingActive;
            long topologyEvolutionStartedAt = recordTopologyEvolution
                ? CaptureWorkTimestamp()
                : 0L;
            bool topologyTransitionChanged =
                AdvanceTopologyTransition(deltaTime);
            bool evolvingTopologyRebuilt = false;
            if (!topologyMaintenanceBlocked)
            {
                bool evolvingTopologyDirty =
                    AdvanceFreeWaterEvolution(deltaTime);
                evolvingTopologyDirty =
                    AdvanceMajorEvolution(deltaTime) ||
                    evolvingTopologyDirty;
                if (evolvingTopologyDirty && BuildEvolvingMajorField())
                {
                    evolvingTopologyRebuilt = true;
                }
            }

            if (recordTopologyEvolution)
            {
                RecordTopologyEvolutionWork(topologyEvolutionStartedAt);
            }
            if (evolvingTopologyRebuilt || topologyTransitionChanged)
            {
                RefreshDynamicTopologySources(false, false);
            }

            DispatchPendingIsolatedLifeProbe();

            bool manualInjectedThisUpdate = ProcessPendingInjections(now);

            float updateRate = ResolveUpdateRate();
            float stepDuration = 1f / Mathf.Max(1f, updateRate);
            lastMaterialStepDuration = stepDuration;
            ResolveTransportSubsteps(
                stepDuration,
                out int requiredTransportSubsteps,
                out int usedTransportSubsteps,
                out float maximumTransportCfl);
            lastRequiredTransportSubsteps = requiredTransportSubsteps;
            lastUsedTransportSubsteps = usedTransportSubsteps;
            lastMaximumTransportCfl = maximumTransportCfl;

            if (manualInjectedThisUpdate)
            {
                simulationAccumulator = Mathf.Max(
                    simulationAccumulator,
                    stepDuration);
            }

            simulationAccumulator = Mathf.Min(
                simulationAccumulator + deltaTime,
                stepDuration * 2f);

            bool visualShapeEvaluatedThisUpdate = false;
            while (simulationAccumulator >= stepDuration)
            {
                if (transportSafetyLimitExceeded)
                {
                    // Retain the full material tick, all queued births, and all
                    // persistent state. Running with fewer substeps would make
                    // the donor-cell solve unstable and silently change the
                    // accepted canonical velocity contract.
                    break;
                }

                simulationAccumulator -= stepDuration;
                lastMaterialStepsThisFrame++;
                if (automaticBirthDebugActive)
                {
                    BeginAutomaticBirthDebugStep();
                }

                bool foamCompositionDeposited =
                    AdvanceFoamCompositionEvents(stepDuration, now);
                bool automaticBirthDeposited =
                    AdvanceAutomaticBirthSources(stepDuration, now);
                bool materialStepActive = currentState != null ||
                    materialLifetimeAuthorityActive ||
                    foamCompositionDeposited || automaticBirthDeposited ||
                    activeFoamCompositionEventCount > 0 ||
                    activeAutomaticFoamSourceEventCount > 0 ||
                    pendingMaterialBirths.Count > 0 ||
                    IsAutomaticSourcePopulationActive;

                bool measureTopology = false;
                if ((materialStepActive || topologyDebugActive) &&
                    !topologyMaintenanceBlocked)
                {
                    // Material aging consumes the single composite topology
                    // field. Topology generation internals remain quarantined:
                    // they provide only this sampled input and never own
                    // material lifetime directly. Metrics are now measured
                    // after lifecycle/injection so the Inspector observes the
                    // same state that is rendered, not the previous tick.
                    bool footprintMetricsActive =
                        currentState != null ||
                        materialLifetimeAuthorityActive ||
                        topologyDebugActive ||
                        manualProofReferencePending ||
                        manualProofReferenceArea > 0.0001f;
                    float topologyMetricsInterval = 1f /
                        TopologyMetricsUpdateRate;
                    if (footprintMetricsActive)
                    {
                        topologyMetricsAccumulator += stepDuration;
                        measureTopology =
                            topologyMetricsAccumulator >=
                            topologyMetricsInterval;
                    }

                    if (!evolvingTopologyRebuilt)
                    {
                        RefreshDynamicTopologySources(false);
                    }
                }

                if (materialStepActive)
                {
                    SimulateFullField(
                        stepDuration,
                        usedTransportSubsteps);
                    if (measureTopology && !topologyMaintenanceBlocked)
                    {
                        MeasureTopologyMetrics();
                        topologyMetricsAccumulator %=
                            1f / TopologyMetricsUpdateRate;
                    }
                }
                else if (measureTopology && !topologyMaintenanceBlocked)
                {
                    MeasureTopologyMetrics();
                    topologyMetricsAccumulator %=
                        1f / TopologyMetricsUpdateRate;
                }

                if (shapeProductDebugActive)
                {
                    visualShapeEvaluatedThisUpdate |=
                        DispatchAdvanceVisualOccupancy(
                            stepDuration,
                            usedTransportSubsteps);
                }

                if (automaticBirthDebugActive)
                {
                    EndAutomaticBirthDebugStep();
                }
            }

            if (manualProofReferencePending &&
                !topologyMetricsReadbackPending)
            {
                MeasureTopologyMetrics(true);
            }

            // Production rendering consumes the current committed Layer C
            // state directly. Point-velocity residual prediction was rejected
            // because it visibly oscillated beside blocked rock and bank faces.
            simulationInterpolation = 1f;
            lastRenderInterpolationAlpha = simulationInterpolation;

            if (IsSleeping)
            {
                if (idleSince <= 0.0)
                {
                    idleSince = Time.realtimeSinceStartupAsDouble;
                }
                else if (Time.realtimeSinceStartupAsDouble - idleSince >=
                         ResourceReleaseDelaySeconds)
                {
                    BindDisabled();
                    ReleaseResources();
                    return;
                }
            }
            else
            {
                idleSince = 0.0;
            }

            if (shapeProductDebugEnteredThisUpdate &&
                !visualShapeEvaluatedThisUpdate)
            {
                // Seed the diagnostic products immediately on entry, then keep
                // all continuing Layer D work on the fixed material cadence.
                DispatchEvaluateShape();
            }

            RecordSteadyStateWorkFrame(materialWork, false);
            BindField();
            UpdateRecentPeaks();
        }

        private void ResolveTransportSubsteps(
            float materialStepDuration,
            out int requiredSubsteps,
            out int usedSubsteps,
            out float maximumCfl)
        {
            float baseSpeed =
                ResolveBaseFoamDownstreamSpeedMetresPerSecond();
            float longitudinalSpacing = Mathf.Max(
                0.0001f,
                minimumTransportLongitudinalSpacing);
            float lateralSpacing = Mathf.Max(
                0.0001f,
                minimumTransportLateralSpacing);
            float lateralSpeed = baseSpeed * Mathf.Max(
                0f,
                river != null ? river.FoamMaximumLateralSpeedRatio : 0f);

            lastEstimatedTransportCellsPerStep =
                baseSpeed * materialStepDuration / longitudinalSpacing;
            lastEstimatedLateralTransportCellsPerStep =
                lateralSpeed * materialStepDuration / lateralSpacing;
            maximumCfl = lastEstimatedTransportCellsPerStep +
                lastEstimatedLateralTransportCellsPerStep;
            requiredSubsteps = Mathf.Max(
                1,
                Mathf.CeilToInt(maximumCfl / TransportTargetCfl));

            transportSafetyLimitExceeded =
                requiredSubsteps > MaximumTransportSubsteps;
            usedSubsteps = transportSafetyLimitExceeded
                ? 0
                : requiredSubsteps;

            if (transportSafetyLimitExceeded)
            {
                transportSafetyStatus =
                    $"Blocked: CFL {maximumCfl:0.000} requires " +
                    $"{requiredSubsteps} substeps (limit " +
                    $"{MaximumTransportSubsteps})";
                return;
            }

            float cflPerSubstep = maximumCfl /
                Mathf.Max(1, usedSubsteps);
            transportSafetyStatus =
                $"CFL {cflPerSubstep:0.000} / " +
                $"{usedSubsteps} substep" +
                (usedSubsteps == 1 ? string.Empty : "s");
        }


        public bool EmitIsolatedLifeProbe(
            float distanceNormalized,
            float acrossNormalized,
            bool absoluteAging)
        {
            if (river == null)
            {
                river = GetComponent<StylizedRiver>();
            }

            if (river == null || !river.FoamEnabled ||
                river.FreezeAmount >= 0.999f ||
                !river.Domain.IsValid)
            {
                return false;
            }

            pendingIsolatedLifeProbe = true;
            pendingIsolatedLifeProbeAbsoluteAging = absoluteAging;
            pendingIsolatedLifeProbeDistanceNormalized =
                Mathf.Clamp01(distanceNormalized);
            pendingIsolatedLifeProbeAcrossNormalized =
                Mathf.Clamp(acrossNormalized, -0.85f, 0.85f);
            materialLifetimeAuthorityActive = true;
            materialLifetimeEmptyMetricReadbacks = 0;
            isolatedLifeProbeStatus = absoluteAging
                ? "Queued isolated direct material probe, absolute 1s aging"
                : "Queued isolated direct material probe, configured aging";
            lifetimeAuthorityStatus =
                "Queued isolated direct material probe";
            idleSince = 0.0;
            return true;
        }

        public void NotifyRiverChanged()
        {
            topologyCacheValidationState = "Not Evaluated";
            topologyCacheValidationSummary =
                "River inputs changed; validate the assigned cache again.";

            if (river == null)
            {
                river = GetComponent<StylizedRiver>();
            }

            if (river == null || !river.Domain.IsValid)
            {
                resourcesDirty = true;
                boundaryDirty = true;
                return;
            }

            bool domainChanged = domainVersion != river.Domain.Version;
            bool qualityChanged =
                currentState != null && allocatedQuality != river.Quality;
            resourcesDirty |= domainChanged || qualityChanged;
            boundaryDirty |= domainChanged;
            if (initializationPhase == InitializationPhase.Failed &&
                (domainChanged || qualityChanged))
            {
                initializationPhase = InitializationPhase.NotStarted;
            }
        }

        public bool EmitNormalized(
            float distanceNormalized,
            float acrossNormalized,
            float radius,
            float amount,
            float initialRemainingLife,
            float elongation)
        {
            if (river == null)
            {
                river = GetComponent<StylizedRiver>();
            }

            if (river == null || !river.FoamEnabled ||
                river.FreezeAmount >= 0.999f ||
                !river.Domain.IsValid)
            {
                return false;
            }

            float globalDistance = Mathf.Lerp(
                river.Domain.GlobalDistanceMinimum,
                river.Domain.GlobalDistanceMaximum,
                Mathf.Clamp01(distanceNormalized));

            float resolvedAmount = Mathf.Clamp01(amount);
            if (resolvedAmount <= 0.0001f)
            {
                return false;
            }

            int injectionIndex = ++manualInjectionSequence;
            float resolvedRemainingLife = Mathf.Clamp01(
                initialRemainingLife);
            float resolvedRadius = Mathf.Clamp(radius, 0.05f, 8f);
            float shapeSeed = river.VisualSeed + injectionIndex * 17.371f;
            float sourceFillSeed =
                river.VisualSeed * 0.431f +
                injectionIndex * 71.371f +
                ManualSourceFillSeedSalt;
            float sourceFillFeatureSize =
                ResolveSourceFillFeatureSize(resolvedRadius);
            float patternSeed =
                river.VisualSeed * 0.613f +
                injectionIndex * 109.731f +
                ManualPatternSeedSalt;

            pendingInjections.Add(
                new PendingInjection(
                    globalDistance,
                    Mathf.Clamp(acrossNormalized, -1f, 1f),
                    resolvedRadius,
                    resolvedAmount,
                    resolvedRemainingLife,
                    patternSeed,
                    Mathf.Clamp(elongation, 0.25f, 8f),
                    true,
                    sourceFillSeed,
                    sourceFillFeatureSize,
                    shapeSeed,
                    ManualTestShapeVariety,
                    false));
            idleSince = 0.0;
            return true;
        }

        public void ClearFoam()
        {
            pendingInjections.Clear();
            pendingMaterialBirths.Clear();
            pendingIsolatedLifeProbe = false;
            pendingIsolatedLifeProbeAbsoluteAging = false;
            isolatedLifeProbeAbsoluteAgingActive = false;
            isolatedLifeProbeWrittenAt = -1.0;
            ClearFoamCompositionEvents();
            ResetManualInjectionSequence();
            lastInjectionBoundaryCoverage = -1f;
            lastInjectionStateSynchronized = false;
            simulationAccumulator = 0f;
            lastEstimatedTransportCellsPerStep = 0f;
            lastEstimatedLateralTransportCellsPerStep = 0f;
            lastMaximumTransportCfl = 0f;
            lastRequiredTransportSubsteps = 1;
            lastUsedTransportSubsteps = 1;
            transportSafetyLimitExceeded = false;
            transportSafetyStatus = "CFL transport ready";
            transportMetricsAccumulator = 0f;
            ResetTransportMetricValues();
            transportMetricsAvailable = false;
            topologyMetricsAccumulator = 0f;
            integratedPresenceArea = 0f;
            visiblePresenceCoreArea = 0f;
            manualProofReferenceArea = 0f;
            manualProofReferencePending = false;
            materialLifetimeAuthorityActive = false;
            materialLifetimeEmptyMetricReadbacks = 0;
            lifetimeAuthorityStatus = "No live material known";
            materialClockSessionActive = false;
            materialClockSessionStartedAt = -1.0;
            materialSimulatedSecondsSinceSession = 0f;
            materialStepCountSinceSession = 0;
            lastConfiguredAbsoluteLifeProbeActive = false;
            birthCommandsThisFrame = 0;
            lastBirthCommandAt = -1.0;
            automaticShoreBirthAccumulator = 0f;
            automaticShoreBirthSubmittedLastUpdate = 0;
            automaticShoreBirthRejectedLastUpdate = 0;
            automaticShoreBirthStatus = river != null && river.FoamAutomaticBirthEnabled
                ? "Cleared / automatic source population remains enabled"
                : "Automatic source population disabled";
            simulationInterpolation = 1f;
            lastRenderInterpolationAlpha = simulationInterpolation;
            idleSince = Time.realtimeSinceStartupAsDouble;

            if (stateA != null)
            {
                DispatchClear(stateA, 0, fieldWidth);
            }

            if (stateB != null)
            {
                DispatchClear(stateB, 0, fieldWidth);
            }

            ClearRenderTexture(shapeMaskTexture);
            ClearRenderTexture(filmSourceTexture);
            ClearRenderTexture(filmSupportTexture);
            ClearRenderTexture(visualOccupancyA);
            ClearRenderTexture(visualOccupancyB);
            currentVisualOccupancy = visualOccupancyA;
            writeVisualOccupancy = visualOccupancyB;
        }

        public void ResetRecentPeaks()
        {
            recentPeakDispatches = lastUpdateDispatches;
            recentPeakCellIterations = lastUpdateCellIterations;
            recentPeakWindowEnd = Time.realtimeSinceStartupAsDouble + 5.0;
        }
    }
}
