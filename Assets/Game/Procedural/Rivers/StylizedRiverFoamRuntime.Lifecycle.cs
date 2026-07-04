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

            GeneratedGeometryRegistry.SourceAdded += HandleGeneratedSourceChanged;
            GeneratedGeometryRegistry.SourceRemoved += HandleGeneratedSourceChanged;
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
            automaticTopologyGenerationInProgress = false;
            automaticDevelopmentObservedSignature = int.MinValue;
            automaticDevelopmentRebuildReason =
                AutomaticDevelopmentRebuildReason.None;
            pendingAutomaticDevelopmentRebuildAfterStartup = false;
            pendingStartupCacheStaleObstacles = false;
            pendingStartupCacheStaleSettings = false;
            topologyStartupValidationActive = false;
            topologyStartupValidationComplete = false;
            if (!Application.isPlaying)
            {
                topologyCacheSessionPersistentInputKey = string.Empty;
                topologyCacheSessionPersistentInputCaptured = false;
                automaticTopologyCacheWritePending = false;
                automaticTopologyCachePersistenceState = "Idle";
                automaticTopologyCachePersistenceSummary =
                    "No automatic development cache write is pending.";
            }
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

            GeneratedGeometryRegistry.SourceAdded -= HandleGeneratedSourceChanged;
            GeneratedGeometryRegistry.SourceRemoved -= HandleGeneratedSourceChanged;
            GeneratedGeometryRegistry.SourceChanged -= HandleGeneratedSourceChanged;

            BindDisabled();
            ReleaseResources();
            pendingInjections.Clear();
            pendingMaterialBirths.Clear();
            ClearProgressiveRibbonEvents();
        }

        private void OnDestroy()
        {
            ClearProgressiveRibbonEvents();
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
                ClearProgressiveRibbonEvents();
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
                ClearProgressiveRibbonEvents();
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
            bool progressiveBirthDebugActive =
                IsProgressiveBirthSourceDebugActive;
            bool materialWork =
                materialLifetimeAuthorityActive ||
                pendingInjections.Count > 0 ||
                pendingMaterialBirths.Count > 0 ||
                activeProgressiveRibbonEventCount > 0;
            bool hasWork = materialWork || topologyDebugActive ||
                progressiveBirthDebugActive;

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

            QueueObstacleRebuildIfNeeded();
            QueueMajorTopologyRebuildIfNeeded();
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

            float now = Time.realtimeSinceStartup;
            float deltaTime = Mathf.Clamp(now - lastRuntimeTime, 0f, 0.1f);
            lastRuntimeTime = now;

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

            if (evolvingTopologyRebuilt || topologyTransitionChanged)
            {
                RefreshDynamicTopologySources(false, false);
            }

            bool manualInjectedThisUpdate = ProcessPendingInjections(now);

            float updateRate = ResolveUpdateRate();
            float stepDuration = 1f / Mathf.Max(1f, updateRate);
            lastMaterialStepDuration = stepDuration;
            float signedDownstreamSpeed = river != null
                ? river.FlowSpeedMetresPerSecond *
                  river.LiquidFactor *
                  river.FoamMaterialFlowSpeedMultiplier *
                  river.FlowDirection
                : 0f;
            float downstreamSpeed = Mathf.Abs(signedDownstreamSpeed);
            lastEstimatedTransportCellsPerStep =
                minimumTransportLongitudinalSpacing > 0.0001f
                    ? downstreamSpeed * stepDuration /
                      minimumTransportLongitudinalSpacing
                    : 0f;

            if (manualInjectedThisUpdate)
            {
                simulationAccumulator = Mathf.Max(
                    simulationAccumulator,
                    stepDuration);
            }

            bool phaseMaterialActive =
                materialLifetimeAuthorityActive ||
                activeProgressiveRibbonEventCount > 0 ||
                pendingMaterialBirths.Count > 0;
            AdvanceFoamPhaseTransport(
                deltaTime,
                signedDownstreamSpeed,
                phaseMaterialActive);

            simulationAccumulator = Mathf.Min(
                simulationAccumulator + deltaTime,
                stepDuration * 2f);

            while (simulationAccumulator >= stepDuration)
            {
                simulationAccumulator -= stepDuration;
                lastMaterialStepsThisFrame++;
                if (progressiveBirthDebugActive)
                {
                    BeginProgressiveBirthDebugStep();
                }

                bool progressiveRibbonDeposited =
                    AdvanceProgressiveRibbonEvents(stepDuration, now);
                bool materialStepActive =
                    materialLifetimeAuthorityActive ||
                    progressiveRibbonDeposited ||
                    activeProgressiveRibbonEventCount > 0 ||
                    pendingMaterialBirths.Count > 0;

                if (materialStepActive)
                {
                    ConfigureSharedComputeParameters(stepDuration);
                }

                if ((materialStepActive || topologyDebugActive) &&
                    !topologyMaintenanceBlocked)
                {
                    // Material aging consumes the single composite topology
                    // field. Topology generation internals remain quarantined:
                    // they provide only this sampled input and never own
                    // material lifetime directly.
                    bool footprintMetricsActive =
                        materialLifetimeAuthorityActive ||
                        topologyDebugActive ||
                        manualProofReferencePending ||
                        manualProofReferenceArea > 0.0001f;
                    bool measureTopology = false;
                    float topologyMetricsInterval = 1f /
                        TopologyMetricsUpdateRate;
                    if (footprintMetricsActive)
                    {
                        topologyMetricsAccumulator += stepDuration;
                        measureTopology =
                            topologyMetricsAccumulator >=
                            topologyMetricsInterval;
                    }

                    if (measureTopology)
                    {
                        if (evolvingTopologyRebuilt)
                        {
                            MeasureTopologyMetrics();
                        }
                        else
                        {
                            RefreshDynamicTopologySources(true);
                        }

                        topologyMetricsAccumulator %= topologyMetricsInterval;
                    }
                    else if (!evolvingTopologyRebuilt)
                    {
                        RefreshDynamicTopologySources(false);
                    }
                }

                if (materialStepActive)
                {
                    SimulateFullField(stepDuration);
                }

                if (progressiveBirthDebugActive)
                {
                    EndProgressiveBirthDebugStep();
                }
            }

            if (manualProofReferencePending &&
                !topologyMetricsReadbackPending)
            {
                MeasureTopologyMetrics(true);
            }

            // Normal Foam rendering presents the latest committed material
            // state plus the bounded residual phase. The residual resets only
            // when a whole material cell has been committed into the persistent
            // texture.
            simulationInterpolation = 1f;
            foamRenderTravelMetres = foamPhaseTransportMetres;
            lastRenderInterpolationAlpha = simulationInterpolation;
            lastFoamPhaseTransportMetres = foamPhaseTransportMetres;
            lastFoamRenderTravelMetres = foamRenderTravelMetres;
            lastFoamPhaseCellFraction =
                minimumTransportLongitudinalSpacing > 0.0001f
                    ? Mathf.Clamp01(
                        Mathf.Abs(foamPhaseTransportMetres) /
                        minimumTransportLongitudinalSpacing)
                    : 0f;

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

            BindField();
            UpdateRecentPeaks();
        }

        private void AdvanceFoamPhaseTransport(
            float deltaTime,
            float signedDownstreamSpeed,
            bool materialActive)
        {
            lastPhaseCommitCellsThisFrame = 0;
            phaseCommitCounterAccumulator += deltaTime;
            if (phaseCommitCounterAccumulator >= 1f)
            {
                lastPhaseCommitCellsThisSecond = phaseCommitCellsInCurrentSecond;
                phaseCommitCellsInCurrentSecond = 0;
                phaseCommitCounterAccumulator %= 1f;
            }

            if (!materialActive || minimumTransportLongitudinalSpacing <= 0.0001f ||
                Mathf.Abs(signedDownstreamSpeed) <= 0.0001f)
            {
                if (!materialActive)
                {
                    foamPhaseTransportMetres = 0f;
                }

                return;
            }

            foamPhaseTransportMetres += signedDownstreamSpeed * deltaTime;
            float cellLength = Mathf.Max(
                0.0001f,
                minimumTransportLongitudinalSpacing);
            int committedMagnitude = Mathf.FloorToInt(
                Mathf.Abs(foamPhaseTransportMetres) / cellLength);
            if (committedMagnitude <= 0)
            {
                return;
            }

            committedMagnitude = Mathf.Min(
                committedMagnitude,
                Mathf.Max(1, fieldWidth));
            int committedCells = foamPhaseTransportMetres >= 0f
                ? committedMagnitude
                : -committedMagnitude;
            if (!DispatchPhaseCommit(committedCells))
            {
                return;
            }

            foamPhaseTransportMetres -= committedCells * cellLength;
            lastPhaseCommitCellsThisFrame = Mathf.Abs(committedCells);
            phaseCommitCellsInCurrentSecond += lastPhaseCommitCellsThisFrame;
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
                    true));
            idleSince = 0.0;
            return true;
        }

        public void ClearFoam()
        {
            pendingInjections.Clear();
            pendingMaterialBirths.Clear();
            ClearProgressiveRibbonEvents();
            ResetManualInjectionSequence();
            lastInjectionBoundaryCoverage = -1f;
            lastInjectionStateSynchronized = false;
            simulationAccumulator = 0f;
            foamPhaseTransportMetres = 0f;
            foamRenderTravelMetres = 0f;
            lastFoamPhaseTransportMetres = 0f;
            lastFoamPhaseCellFraction = 0f;
            lastPhaseCommitCellsThisFrame = 0;
            lastPhaseCommitCellsThisSecond = 0;
            phaseCommitCellsInCurrentSecond = 0;
            phaseCommitCounterAccumulator = 0f;
            lastFoamRenderTravelMetres = 0f;
            topologyMetricsAccumulator = 0f;
            integratedPresenceArea = 0f;
            visiblePresenceCoreArea = 0f;
            manualProofReferenceArea = 0f;
            manualProofReferencePending = false;
            materialLifetimeAuthorityActive = false;
            materialLifetimeEmptyMetricReadbacks = 0;
            lifetimeAuthorityStatus = "No live material known";
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
        }

        public void ResetRecentPeaks()
        {
            recentPeakDispatches = lastUpdateDispatches;
            recentPeakCellIterations = lastUpdateCellIterations;
            recentPeakWindowEnd = Time.realtimeSinceStartupAsDouble + 5.0;
        }
    }
}
