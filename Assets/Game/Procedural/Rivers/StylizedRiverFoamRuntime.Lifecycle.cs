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
            reservations.Clear();
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
                reservations.Clear();
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
                reservations.Clear();
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
            bool progressiveBirthTransferDebugActive =
                IsProgressiveBirthTransferDebugActive;
            bool materialWork =
                pendingInjections.Count > 0 ||
                activeProgressiveRibbonEventCount > 0 ||
                reservations.Count > 0 ||
                CountActiveChunks() > 0;
            bool hasWork = materialWork || topologyDebugActive ||
                progressiveBirthDebugActive ||
                progressiveBirthTransferDebugActive;

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

            if (manualInjectedThisUpdate)
            {
                // Manual diagnostics remain immediately visible, then enter
                // the same transport, guidance, boundary, lifecycle, and
                // disturbance-reinforcement solver on the following step.
                simulationAccumulator = 0f;
                simulationInterpolation = 1f;
            }
            else
            {
                simulationAccumulator = Mathf.Min(
                    simulationAccumulator + deltaTime,
                    stepDuration * 2f);

                while (simulationAccumulator >= stepDuration)
                {
                    simulationAccumulator -= stepDuration;
                    BeginProgressiveBirthSourceStep();
                    if (progressiveBirthDebugActive)
                    {
                        BeginProgressiveBirthDebugStep();
                    }

                    bool progressiveRibbonDeposited =
                        AdvanceProgressiveRibbonEvents(stepDuration, now);
                    bool materialStepActive =
                        progressiveRibbonDeposited ||
                        activeProgressiveRibbonEventCount > 0 ||
                        reservations.Count > 0 ||
                        CountActiveChunks() > 0;

                    if (materialStepActive)
                    {
                        UpdateReservations(stepDuration, now);
                        UpdateActiveChunks(now);
                        ConfigureSharedComputeParameters(stepDuration);
                    }

                    guidanceAccumulator += stepDuration;
                    float guidanceInterval = 1f /
                        Mathf.Max(1f, ResolveGuidanceUpdateRate());
                    if (guidanceAccumulator >= guidanceInterval)
                    {
                        if (materialStepActive)
                        {
                            BuildGuidanceField(guidanceAccumulator);
                        }

                        guidanceAccumulator %= guidanceInterval;
                    }

                    if (topologyDebugActive &&
                        !topologyMaintenanceBlocked)
                    {
                        // Patch 4.6 composes evolving Major Support with the
                        // accepted static Connector/negative fields and live
                        // anchored sources.
                        topologyMetricsAccumulator += stepDuration;
                        float topologyMetricsInterval = 1f /
                            TopologyMetricsUpdateRate;
                        bool measureTopology =
                            topologyMetricsAccumulator >=
                            topologyMetricsInterval;
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

                            topologyMetricsAccumulator %=
                                topologyMetricsInterval;
                        }
                        else if (!evolvingTopologyRebuilt)
                        {
                            RefreshDynamicTopologySources(false);
                        }
                    }

                    if (materialStepActive)
                    {
                        SimulateActiveChunks(stepDuration);
                    }

                    if (progressiveBirthDebugActive)
                    {
                        EndProgressiveBirthDebugStep();
                    }
                }

                simulationInterpolation = Mathf.Clamp01(
                    simulationAccumulator / Mathf.Max(0.0001f, stepDuration));
            }

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
            float freshness,
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
            // Keep the public parameter name for source compatibility; its
            // canonical Patch 4.11A meaning is normalized initial Remaining Life.
            float resolvedRemainingLife = Mathf.Clamp01(freshness);
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
                    ProvisionalMaterialShapeVariety,
                    true));
            idleSince = 0.0;
            return true;
        }

        public void ClearFoam()
        {
            pendingInjections.Clear();
            reservations.Clear();
            ClearProgressiveRibbonEvents();
            ResetManualInjectionSequence();
            lastInjectionBoundaryCoverage = -1f;
            lastInjectionStateSynchronized = false;
            simulationAccumulator = 0f;
            guidanceAccumulator = 0f;
            topologyMetricsAccumulator = 0f;
            simulationInterpolation = 1f;
            idleSince = Time.realtimeSinceStartupAsDouble;

            if (stateA != null)
            {
                DispatchClear(stateA, 0, fieldWidth);
            }

            if (stateB != null)
            {
                DispatchClear(stateB, 0, fieldWidth);
            }

            if (advectedState != null)
            {
                DispatchClear(advectedState, 0, fieldWidth);
            }

            if (reverseState != null)
            {
                DispatchClear(reverseState, 0, fieldWidth);
            }

            if (progressiveBirthSourceTexture != null)
            {
                DispatchClear(
                    progressiveBirthSourceTexture,
                    0,
                    fieldWidth);
            }

            if (progressiveBirthTransferDebugTexture != null)
            {
                DispatchClear(
                    progressiveBirthTransferDebugTexture,
                    0,
                    fieldWidth);
            }

            progressiveBirthSourceContainsData = false;
            Array.Clear(chunkActive, 0, chunkActive.Length);
            Array.Clear(chunkActiveUntil, 0, chunkActiveUntil.Length);
        }

        public void ResetRecentPeaks()
        {
            recentPeakDispatches = lastUpdateDispatches;
            recentPeakCellIterations = lastUpdateCellIterations;
            recentPeakWindowEnd = Time.realtimeSinceStartupAsDouble + 5.0;
        }
    }
}
