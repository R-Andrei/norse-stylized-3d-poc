using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private bool EnsureResources()
        {
            using var profilerScope = EnsureResourcesProfilerMarker.Auto();

            if (initializationPhase == InitializationPhase.Ready &&
                AreResourcesCompleteAndCurrent())
            {
                if (boundaryDirty)
                {
                    RequestBoundaryRebuild();
                }

                return true;
            }

            if (initializationPhase == InitializationPhase.Failed)
            {
                return false;
            }

            if (initializationPhase ==
                InitializationPhase.CachePreparationRequired)
            {
                if (fieldWidth > 0 || currentState != null ||
                    computeShader != null)
                {
                    ReleaseResources();
                    initializationPhase =
                        InitializationPhase.CachePreparationRequired;
                }
                return false;
            }

            if (initializationPhase == InitializationPhase.Ready)
            {
                BeginTopologyStartupValidation();
                PrepareDimensionChangingTopologyTransition();
                initializationPhase = InitializationPhase.ReleaseOldResources;
                // The transition capture is a queued GPU write. Keep all old
                // source textures alive through this frame and release them on
                // the next initialization step rather than immediately after
                // dispatching the capture.
                return false;
            }

            if (initializationPhase == InitializationPhase.NotStarted)
            {
                BeginTopologyStartupValidation();
                initializationPhase = InitializationPhase.ReleaseOldResources;
            }
            else if (InitializationInputsChanged())
            {
                RecordTopologyStartupRestartReasons();
                topologyStartupRestartCount++;
                BeginTopologyStartupValidation();
                // A domain or quality change invalidates every later phase.
                // Restart from release rather than allowing partially built
                // resources to become authoritative.
                initializationPhase = InitializationPhase.ReleaseOldResources;
            }

            InitializationPhase measuredPhase = initializationPhase;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            AdvanceInitializationPhase();
            stopwatch.Stop();
            RecordTopologyStartupStep(
                measuredPhase,
                stopwatch.Elapsed.TotalMilliseconds);
            if (initializationPhase == InitializationPhase.Ready ||
                initializationPhase == InitializationPhase.Failed ||
                initializationPhase ==
                    InitializationPhase.CachePreparationRequired)
            {
                CompleteTopologyStartupValidation();
            }

            return initializationPhase == InitializationPhase.Ready;
        }


        private bool AreResourcesCompleteAndCurrent()
        {
            return !resourcesDirty &&
                currentState != null &&
                previousState != null &&
                writeState != null &&
                presentationPreviousState != null &&
                presentationPreviousState.IsCreated() &&
                shapeMaskTexture != null &&
                shapeMaskTexture.IsCreated() &&
                filmSourceTexture != null &&
                filmSourceTexture.IsCreated() &&
                filmSupportTexture != null &&
                filmSupportTexture.IsCreated() &&
                visualOccupancyA != null &&
                visualOccupancyA.IsCreated() &&
                visualOccupancyB != null &&
                visualOccupancyB.IsCreated() &&
                currentVisualOccupancy != null &&
                writeVisualOccupancy != null &&
                topologyTexture != null &&
                topologySourcesTexture != null &&
                topologyGeneratedTexture != null &&
                evolvingMajorTexture != null &&
                evolvingHostedNegativeTexture != null &&
                evolvingFreeWaterNegativeTexture != null &&
                evolvingConnectorTexture != null &&
                evolvingWeakSpanNegativeTexture != null &&
                majorTopology != null &&
                connectorTopology != null &&
                pocketTopology != null &&
                currentShoreEdgesTexture != null &&
                obstacleExclusionTexture != null &&
                obstacleExclusionTexture.IsCreated() &&
                objectContactFieldTexture != null &&
                objectContactFieldTexture.IsCreated() &&
                motionLaneTexture != null &&
                obstacleRoutingTexture != null &&
                neutralDisturbanceTexture != null &&
                neutralDisturbanceTexture.IsCreated() &&
                boundaryTexture != null &&
                metricBuffer != null &&
                automaticFoamSourceEventBuffer != null &&
                automaticFoamSourceEventBuffer.count ==
                    automaticFoamSourceEventCapacity &&
                topologyMetricsBuffer != null &&
                transportMetricsBuffer != null &&
                domainVersion == river.Domain.Version &&
                allocatedQuality == river.Quality &&
                !FoamGridConfigurationChanged();
        }

        private bool FoamGridConfigurationChanged()
        {
            if (river == null || allocatedFoamGridMode != river.FoamGridMode)
            {
                return river != null;
            }

            return river.FoamGridMode == StylizedRiverFoamGridMode.FixedMetric &&
                !Mathf.Approximately(
                    allocatedFoamRequestedCellSizeMetres,
                    ResolveEffectiveFoamRequestedCellSizeMetres());
        }

        private void RecordTopologyStartupRestartReasons()
        {
            if (river == null)
            {
                topologyStartupDirtyReasons |=
                    TopologyStartupDirtyReason.RiverUnavailable;
                return;
            }

            if (!river.Domain.IsValid)
            {
                topologyStartupDirtyReasons |=
                    TopologyStartupDirtyReason.DomainInvalid;
            }
            if (domainVersion != river.Domain.Version)
            {
                topologyStartupDirtyReasons |=
                    TopologyStartupDirtyReason.DomainChanged;
            }
            if (allocatedQuality != river.Quality)
            {
                topologyStartupDirtyReasons |=
                    TopologyStartupDirtyReason.QualityChanged;
            }
            if (FoamGridConfigurationChanged())
            {
                topologyStartupDirtyReasons |=
                    TopologyStartupDirtyReason.GridConfigurationChanged;
            }
        }

        private bool InitializationInputsChanged()
        {
            if ((int)initializationPhase <=
                    (int)InitializationPhase.ResolveDimensions ||
                (int)initializationPhase >=
                    (int)InitializationPhase.Ready)
            {
                return false;
            }

            return river == null ||
                !river.Domain.IsValid ||
                domainVersion != river.Domain.Version ||
                allocatedQuality != river.Quality ||
                FoamGridConfigurationChanged();
        }

        private void AdvanceInitializationPhase()
        {
            switch (initializationPhase)
            {
                case InitializationPhase.ReleaseOldResources:
                    using (InitReleaseOldResourcesProfilerMarker.Auto())
                    {
                        if (!HasTopologyTransitionVisibleHold)
                        {
                            BindDisabled();
                        }

                        ReleaseResources(false);
                    }

                    initializationPhase = InitializationPhase.LoadCompute;
                    break;

                case InitializationPhase.LoadCompute:
                    using (InitLoadComputeProfilerMarker.Auto())
                    {
                        computeShader = Resources.Load<ComputeShader>(
                            ComputeResourcePath);
                    }

                    if (computeShader == null)
                    {
                        Debug.LogError(
                            $"Stage 6 Foam on '{name}' could not load Resources/{ComputeResourcePath}.compute.",
                            this);
                        initializationPhase = InitializationPhase.Failed;
                        break;
                    }

                    initializationPhase = InitializationPhase.ResolveKernels;
                    break;

                case InitializationPhase.ResolveKernels:
                    using (InitResolveKernelsProfilerMarker.Auto())
                    {
                        ResolveComputeKernels();
                    }

                    initializationPhase = InitializationPhase.ResolveDimensions;
                    break;

                case InitializationPhase.ResolveDimensions:
                    using (InitResolveDimensionsProfilerMarker.Auto())
                    {
                        if (!ResolveInitializationDimensions())
                        {
                            break;
                        }
                    }

                    initializationPhase =
                        InitializationPhase.AllocateMaterialTextures;
                    break;

                case InitializationPhase.AllocateMaterialTextures:
                    using (InitAllocateMaterialTexturesProfilerMarker.Auto())
                    {
                        stateA = CreateFieldTexture("PS3D_RiverFoam_A");
                        stateB = CreateFieldTexture("PS3D_RiverFoam_B");
                        presentationPreviousState = CreateFieldTexture(
                            "PS3D_RiverFoam_PreviousCommitted");
                        shapeMaskTexture = CreateShapeMaskTexture(
                            "PS3D_RiverFoam_ShapeMask");
                        filmSourceTexture = CreateFilmTexture(
                            "PS3D_RiverFoam_FilmSource");
                        filmSupportTexture = CreateFilmTexture(
                            "PS3D_RiverFoam_FilmSupport");
                        visualOccupancyA = CreateFilmTexture(
                            "PS3D_RiverFoam_VisualOccupancy_A");
                        visualOccupancyB = CreateFilmTexture(
                            "PS3D_RiverFoam_VisualOccupancy_B");
                    }

                    initializationPhase =
                        InitializationPhase.AllocateTopologyTextures;
                    break;

                case InitializationPhase.AllocateTopologyTextures:
                    using (InitAllocateTopologyTexturesProfilerMarker.Auto())
                    {
                        topologyTexture = CreateStructuralTexture(
                            "PS3D_RiverFoam_Topology");
                        topologySourcesTexture = CreateStructuralTexture(
                            "PS3D_RiverFoam_TopologySources");
                        // Deterministic prepared topology is uploaded here.
                        // Connector and independent-negative classes remain only
                        // as complete static fallbacks when their prepared
                        // runtime reconstruction is unavailable.
                        topologyGeneratedTexture = CreateStructuralTexture(
                            "PS3D_RiverFoam_TopologyGeneratedInput");
                        evolvingMajorTexture = CreateMajorEvolutionTexture(
                            "PS3D_RiverFoam_EvolvingMajorSupport");
                        evolvingHostedNegativeTexture =
                            CreateMajorEvolutionTexture(
                                "PS3D_RiverFoam_EvolvingHostedNegative");
                        evolvingFreeWaterNegativeTexture =
                            CreateMajorEvolutionTexture(
                                "PS3D_RiverFoam_EvolvingFreeWaterNegative");
                        evolvingConnectorTexture = CreateMajorEvolutionTexture(
                            "PS3D_RiverFoam_EvolvingConnectorSupport");
                        evolvingWeakSpanNegativeTexture =
                            CreateMajorEvolutionTexture(
                                "PS3D_RiverFoam_EvolvingWeakSpanNegative");
                        currentShoreEdgesTexture = CreateShoreEdgesTexture(
                            "PS3D_RiverFoam_CurrentShoreEdges");
                        obstacleExclusionTexture =
                            CreateObstacleExclusionTexture(
                                "PS3D_RiverFoam_ObstacleExclusion");
                        objectContactFieldTexture = CreateFieldTexture(
                            "PS3D_RiverFoam_ObjectContactField");
                        motionLaneTexture = CreateMotionLaneTexture();
                        obstacleRoutingTexture = CreateObstacleRoutingTexture();
                    }

                    initializationPhase =
                        InitializationPhase.AllocateAuxiliaryTextures;
                    break;

                case InitializationPhase.AllocateAuxiliaryTextures:
                    using (InitAllocateAuxiliaryTexturesProfilerMarker.Auto())
                    {
                        neutralDisturbanceTexture =
                            CreateNeutralDisturbanceTexture();
                        previousState = presentationPreviousState;
                        currentState = stateA;
                        writeState = stateB;
                        currentVisualOccupancy = visualOccupancyA;
                        writeVisualOccupancy = visualOccupancyB;
                    }

                    initializationPhase = InitializationPhase.ClearTopology;
                    break;

                case InitializationPhase.ClearTopology:
                    // These textures are sampled before their first authored
                    // write, so clear them explicitly rather than relying on
                    // allocation contents.
                    using (InitClearTopologyProfilerMarker.Auto())
                    {
                        ClearRenderTexture(topologyTexture);
                        ClearRenderTexture(topologySourcesTexture);
                        ClearRenderTexture(topologyGeneratedTexture);
                        ClearRenderTexture(evolvingMajorTexture);
                        ClearRenderTexture(evolvingHostedNegativeTexture);
                        ClearRenderTexture(evolvingFreeWaterNegativeTexture);
                        ClearRenderTexture(evolvingConnectorTexture);
                        ClearRenderTexture(evolvingWeakSpanNegativeTexture);
                        ClearRenderTexture(objectContactFieldTexture);
                    }

                    initializationPhase = InitializationPhase.AllocateBuffers;
                    break;

                case InitializationPhase.AllocateBuffers:
                    using (InitAllocateBuffersProfilerMarker.Auto())
                    {
                        topologyMetricsBuffer = new ComputeBuffer(
                            TopologyMetricCount,
                            sizeof(uint),
                            ComputeBufferType.Raw);
                        transportMetricsBuffer = new ComputeBuffer(
                            TransportMetricCount,
                            sizeof(uint),
                            ComputeBufferType.Raw);
                        automaticFoamSourceEventBuffer = new ComputeBuffer(
                            automaticFoamSourceEventCapacity,
                            Marshal.SizeOf<FoamSourceEventGpuData>(),
                            ComputeBufferType.Structured);
                    }

                    initializationPhase =
                        InitializationPhase.BuildMetricBuffer;
                    break;

                case InitializationPhase.BuildMetricBuffer:
                    BuildMetricBuffer();
                    initializationPhase = InitializationPhase.BuildBoundary;
                    break;

                case InitializationPhase.BuildBoundary:
                    RebuildBoundaryTexture(false);
                    initializationObstacleObservedVersion = int.MinValue;
                    initializationObstacleStableFrameCount = 0;
                    initializationPhase =
                        InitializationPhase.WaitForObstacleStability;
                    break;

                case InitializationPhase.WaitForObstacleStability:
                    using (InitWaitObstacleProfilerMarker.Auto())
                    {
                        disturbanceRuntime ??=
                            GetComponent<StylizedRiverDisturbanceRuntime>();
                        if (disturbanceRuntime != null &&
                            !disturbanceRuntime.GeneratedObstacleRegistryReady)
                        {
                            initializationObstacleObservedVersion =
                                int.MinValue;
                            initializationObstacleStableFrameCount = 0;
                            break;
                        }

                        int currentObstacleVersion = disturbanceRuntime != null
                            ? disturbanceRuntime.ObstacleGeometryVersion
                            : -1;

                        if (currentObstacleVersion !=
                            initializationObstacleObservedVersion)
                        {
                            initializationObstacleObservedVersion =
                                currentObstacleVersion;
                            initializationObstacleStableFrameCount = 0;
                        }
                        else
                        {
                            initializationObstacleStableFrameCount++;
                            if (initializationObstacleStableFrameCount >=
                                ObstacleRebuildStableFrameCount)
                            {
                                initializationPhase =
                                    DevelopmentTopologyGenerationInProgress
                                        ? InitializationPhase.BuildObstacleExclusion
                                        : InitializationPhase.ResolveTopologyCache;
                            }
                        }
                    }
                    break;

                case InitializationPhase.ResolveTopologyCache:
                    using (InitResolveTopologyCacheProfilerMarker.Auto())
                    {
                        pendingStartupCachePackage = null;
                        pendingStartupCacheStaleObstacles = false;
                        pendingStartupCacheStaleSettings = false;
                        if (P12CandidateSweepAllowsTransientTopologyGeneration)
                        {
                            BeginP12CandidateSweepTransientTopologyGeneration();
                            topologyCacheStartupState =
                                "P12 Sweep Transient Generation";
                            topologyCacheStartupSummary =
                                "The explicit P12 sweep is generating topology " +
                                "for the active test descriptor without reading, " +
                                "writing, or replacing the assigned cache asset.";
                            initializationPhase =
                                InitializationPhase.BuildObstacleExclusion;
                            break;
                        }

                        TopologyCacheStartupResolution resolution =
                            TryResolveAssignedTopologyCacheForStartup(
                                out StylizedRiverFoamTopologyCachePackage
                                    resolvedPackage,
                                out bool staleObstacles,
                                out bool staleSettings);
                        if (resolution != TopologyCacheStartupResolution.Miss)
                        {
                            pendingStartupCachePackage = resolvedPackage;
                            pendingStartupCacheStaleObstacles = staleObstacles;
                            pendingStartupCacheStaleSettings = staleSettings;
                            initializationPhase =
                                InitializationPhase.InstallCachedTopology;
                        }
                        else
                        {
                            initializationPhase =
                                InitializationPhase.CachePreparationRequired;
                        }
                    }
                    break;

                case InitializationPhase.CachePreparationRequired:
                    BindDisabled();
                    break;

                case InitializationPhase.InstallCachedTopology:
                    topologyStartupValidationCacheInstallCount++;
                    using (InitInstallTopologyCacheProfilerMarker.Auto())
                    {
                        StylizedRiverFoamTopologyCachePackage package =
                            pendingStartupCachePackage;
                        pendingStartupCachePackage = null;
                        bool installed = InstallStartupTopologyCache(
                            package,
                            pendingStartupCacheStaleObstacles,
                            pendingStartupCacheStaleSettings);
                        if (installed)
                        {
                            initializationPhase =
                                InitializationPhase.ClearMaterial;
                        }
                        else
                        {
                            initializationPhase =
                                InitializationPhase.CachePreparationRequired;
                        }
                    }
                    break;

                case InitializationPhase.BuildObstacleExclusion:
                    if (!DevelopmentTopologyGenerationInProgress)
                    {
                        topologyCacheStartupOutcome =
                            TopologyCacheStartupOutcome.PreparationRequired;
                        topologyCacheStartupState = "Preparation Required";
                        topologyCacheStartupSummary =
                            "Play Mode topology generation is disabled. Use " +
                            "Prepare / Rebuild Foam Topology Cache in Edit Mode.";
                        initializationPhase =
                            InitializationPhase.CachePreparationRequired;
                        break;
                    }
                    topologyStartupValidationObstacleBuildCount++;
                    RebuildObstacleExclusionCache();
                    initializationObstacleObservedVersion = int.MinValue;
                    initializationObstacleStableFrameCount = 0;
                    initializationPhase = InitializationPhase.BuildMajorTopology;
                    break;

                case InitializationPhase.BuildMajorTopology:
                    if (!DevelopmentTopologyGenerationInProgress)
                    {
                        topologyCacheStartupOutcome =
                            TopologyCacheStartupOutcome.PreparationRequired;
                        topologyCacheStartupState = "Preparation Required";
                        topologyCacheStartupSummary =
                            "Play Mode topology generation is disabled. Use " +
                            "Prepare / Rebuild Foam Topology Cache in Edit Mode.";
                        initializationPhase =
                            InitializationPhase.CachePreparationRequired;
                        break;
                    }
                    topologyStartupValidationMajorBuildCount++;
                    BuildMajorTopology();
                    initializationPhase =
                        InitializationPhase.BuildConnectorTopology;
                    break;

                case InitializationPhase.BuildConnectorTopology:
                    if (!DevelopmentTopologyGenerationInProgress)
                    {
                        topologyCacheStartupOutcome =
                            TopologyCacheStartupOutcome.PreparationRequired;
                        topologyCacheStartupState = "Preparation Required";
                        topologyCacheStartupSummary =
                            "Play Mode topology generation is disabled. Use " +
                            "Prepare / Rebuild Foam Topology Cache in Edit Mode.";
                        initializationPhase =
                            InitializationPhase.CachePreparationRequired;
                        break;
                    }
                    topologyStartupValidationConnectorBuildCount++;
                    BuildConnectorTopology();
                    initializationPhase =
                        InitializationPhase.BuildPocketTopology;
                    break;

                case InitializationPhase.BuildPocketTopology:
                    if (!DevelopmentTopologyGenerationInProgress)
                    {
                        topologyCacheStartupOutcome =
                            TopologyCacheStartupOutcome.PreparationRequired;
                        topologyCacheStartupState = "Preparation Required";
                        topologyCacheStartupSummary =
                            "Play Mode topology generation is disabled. Use " +
                            "Prepare / Rebuild Foam Topology Cache in Edit Mode.";
                        initializationPhase =
                            InitializationPhase.CachePreparationRequired;
                        break;
                    }
                    topologyStartupValidationPocketBuildCount++;
                    BuildPocketTopology();
                    initializationPhase = InitializationPhase.ClearMaterial;
                    break;

                case InitializationPhase.ClearMaterial:
                    using (InitClearMaterialProfilerMarker.Auto())
                    {
                        DispatchClear(stateA, 0, fieldWidth);
                        DispatchClear(stateB, 0, fieldWidth);
                        DispatchClear(
                            presentationPreviousState,
                            0,
                            fieldWidth);
                        ClearRenderTexture(shapeMaskTexture);
                        ClearRenderTexture(filmSourceTexture);
                        ClearRenderTexture(filmSupportTexture);
                        ClearRenderTexture(visualOccupancyA);
                        ClearRenderTexture(visualOccupancyB);
                    }

                    initializationPhase =
                        InitializationPhase.RefreshInitialTopologySources;
                    break;

                case InitializationPhase.RefreshInitialTopologySources:
                    RefreshDynamicTopologySources(true);
                    initializationPhase = InitializationPhase.Finalize;
                    break;

                case InitializationPhase.Finalize:
                    FinalizeInitialization();
                    break;
            }
        }

        private bool ResolveInitializationDimensions()
        {
            RiverDomainSnapshot domain = river.Domain;
            if (!domain.IsValid)
            {
                return false;
            }

            int maximumTextureSize = SystemInfo.maxTextureSize;
            StylizedRiverFoamGridDescriptor.ValidateFoundation();
            StylizedRiverFoamTopologyCacheCodec.ValidateFoundation();
            StylizedRiverFoamTopologyFieldSpace.ValidateFoundation();
            ResolveFixedMetricCandidateDescriptor(
                domain,
                maximumTextureSize);

            StylizedRiverFoamGridDescriptor activeDescriptor;
            if (river.FoamGridMode ==
                StylizedRiverFoamGridMode.FixedMetric)
            {
                if (!fixedMetricCandidateDescriptor.IsCreated)
                {
                    ReportFixedMetricAllocationFailure(maximumTextureSize);
                    initializationPhase = InitializationPhase.Failed;
                    return false;
                }

                activeDescriptor = fixedMetricCandidateDescriptor;
                fixedMetricCandidateFailureReason =
                    "Active fixed-metric selection";
            }
            else if (!TryResolveLegacyInitializationDescriptor(
                         domain,
                         maximumTextureSize,
                         out activeDescriptor))
            {
                ReportAllocationFailure(maximumTextureSize);
                initializationPhase = InitializationPhase.Failed;
                return false;
            }

            ApplyGridDescriptorDimensions(activeDescriptor);
            gridDescriptorGpuData = gridDescriptor.ToGpuData();

            float longitudinalSpacing = gridDescriptor.ResolvedDxMetres;
            // Keep one inert outflow sample beyond the exact endpoint so
            // bilinear rendering reaches the visible river end without
            // reintroducing a padded population domain.
            simulationFieldLength = Mathf.Min(
                fieldLength,
                validFieldLength + longitudinalSpacing);
            allocatedGlobalStart = domain.GlobalDistanceMinimum;
            allocationWarningReported = false;
            domainVersion = domain.Version;
            allocatedQuality = river.Quality;
            allocatedFoamGridMode = river.FoamGridMode;
            allocatedFoamRequestedCellSizeMetres =
                ResolveEffectiveFoamRequestedCellSizeMetres();
            initializationMotionTime =
                ResolveEffectiveFoamInitializationMotionTime();
            return true;
        }

        private bool TryResolveLegacyInitializationDescriptor(
            RiverDomainSnapshot domain,
            int maximumTextureSize,
            out StylizedRiverFoamGridDescriptor descriptor)
        {
            descriptor = default;
            int resolvedChunkCount = Mathf.Max(
                1,
                Mathf.CeilToInt(domain.LocalLength / ChunkLengthMetres));
            int desiredStructuralResolution =
                ResolveStructuralResolution(river.Quality);
            if (!TryResolveChunkedTextureWidth(
                    resolvedChunkCount,
                    desiredStructuralResolution,
                    16,
                    maximumTextureSize,
                    out int resolvedColumnsPerChunk,
                    out int resolvedFieldWidth))
            {
                chunkCount = resolvedChunkCount;
                return false;
            }

            int resolvedFieldHeight = desiredStructuralResolution;
            int resolvedFilmWidth = Mathf.Max(
                1,
                Mathf.CeilToInt(resolvedFieldWidth * 0.5f));
            int resolvedFilmHeight = Mathf.Max(
                1,
                Mathf.CeilToInt(resolvedFieldHeight * 0.5f));
            if (maximumTextureSize < 24 ||
                resolvedFieldHeight > maximumTextureSize)
            {
                chunkCount = resolvedChunkCount;
                return false;
            }

            descriptor = StylizedRiverFoamGridDescriptor.CreateLegacyNormalized(
                river.Quality,
                resolvedChunkCount,
                resolvedColumnsPerChunk,
                resolvedFieldWidth,
                resolvedFieldHeight,
                resolvedFilmWidth,
                resolvedFilmHeight,
                resolvedChunkCount * ChunkLengthMetres,
                domain.LocalLength);
            return true;
        }

        private void ResolveFixedMetricCandidateDescriptor(
            RiverDomainSnapshot domain,
            int maximumTextureSize)
        {
            fixedMetricCandidateDescriptor = default;
            fixedMetricCandidateFailureReason = "Not resolved";
            if (!TryResolveDomainSurfaceLateralRange(
                    domain,
                    out float lateralMinimumMetres,
                    out float lateralMaximumMetres))
            {
                fixedMetricCandidateFailureReason =
                    "River surface lateral bounds are unavailable.";
                return;
            }

            float requestedCellSize =
                ResolveEffectiveFoamRequestedCellSizeMetres();
            if (!StylizedRiverFoamGridDescriptor.TryCreateFixedMetricOneStrip(
                    river.Quality,
                    requestedCellSize,
                    requestedCellSize,
                    domain.LocalLength,
                    lateralMinimumMetres,
                    lateralMaximumMetres,
                    0f,
                    0,
                    maximumTextureSize,
                    out fixedMetricCandidateDescriptor,
                    out fixedMetricCandidateFailureReason))
            {
                return;
            }

            fixedMetricCandidateFailureReason =
                river.FoamGridMode == StylizedRiverFoamGridMode.FixedMetric
                    ? "Ready; selected for activation"
                    : "Ready; legacy compatibility selected";
        }

        private static bool TryResolveDomainSurfaceLateralRange(
            RiverDomainSnapshot domain,
            out float lateralMinimumMetres,
            out float lateralMaximumMetres)
        {
            lateralMinimumMetres = 0f;
            lateralMaximumMetres = 0f;
            if (domain == null || !domain.IsValid || domain.SampleCount < 1)
            {
                return false;
            }

            float maximumLeft = 0f;
            float maximumRight = 0f;
            IReadOnlyList<StylizedRiverSplineSample> samples = domain.Samples;
            for (int index = 0; index < samples.Count; index++)
            {
                maximumLeft = Mathf.Max(
                    maximumLeft,
                    samples[index].LeftSurfaceHalfWidth);
                maximumRight = Mathf.Max(
                    maximumRight,
                    samples[index].RightSurfaceHalfWidth);
            }

            if (maximumLeft <= 0f || maximumRight <= 0f ||
                float.IsNaN(maximumLeft) || float.IsNaN(maximumRight) ||
                float.IsInfinity(maximumLeft) ||
                float.IsInfinity(maximumRight))
            {
                return false;
            }

            lateralMinimumMetres = -Mathf.Max(0.05f, maximumLeft);
            lateralMaximumMetres = Mathf.Max(0.05f, maximumRight);
            return true;
        }

        private void ApplyGridDescriptorDimensions(
            StylizedRiverFoamGridDescriptor descriptor)
        {
            if (!descriptor.IsCreated)
            {
                throw new InvalidOperationException(
                    "Cannot allocate River Foam from an empty grid descriptor.");
            }

            gridDescriptor = descriptor;
            resolutionPerChunk = descriptor.ColumnsPerChunk;
            chunkCount = Mathf.Max(
                1,
                descriptor.ColumnCount /
                    Mathf.Max(1, descriptor.ColumnsPerChunk));
            fieldWidth = descriptor.ColumnCount;
            fieldHeight = descriptor.RowCount;
            filmFieldWidth = descriptor.FilmWidth;
            filmFieldHeight = descriptor.FilmHeight;
            structuralWidth = descriptor.ColumnCount;
            structuralHeight = descriptor.RowCount;
            fieldLength = descriptor.AllocatedLengthMetres;
            validFieldLength = descriptor.ValidLengthMetres;
            ConfigureAutomaticFoamSourceCapacities();
        }

        private void ConfigureAutomaticFoamSourceCapacities()
        {
            int shoreBucketCount = ResolveAutomaticShoreTotalSlotCount(
                validFieldLength);
            int resolvedEventCapacity =
                AutomaticFoamSourceBaseEventCapacity + shoreBucketCount;
            int resolvedReservationCapacity = Mathf.Max(
                resolvedEventCapacity,
                resolvedEventCapacity *
                    AutomaticFoamPacketReservationCapacityMultiplier);

            if (automaticFoamSourceEvents.Length == resolvedEventCapacity &&
                automaticFoamSourceEventGpuData.Length == resolvedEventCapacity &&
                automaticFoamPacketReservations.Length ==
                    resolvedReservationCapacity)
            {
                automaticFoamSourceEventCapacity = resolvedEventCapacity;
                automaticFoamPacketReservationCapacity =
                    resolvedReservationCapacity;
                return;
            }

            ClearAutomaticFoamSourceEvents();
            automaticFoamSourceEventCapacity = resolvedEventCapacity;
            automaticFoamPacketReservationCapacity =
                resolvedReservationCapacity;
            automaticFoamSourceEvents = new AutomaticFoamSourceEvent[
                automaticFoamSourceEventCapacity];
            automaticFoamSourceEventGpuData = new FoamSourceEventGpuData[
                automaticFoamSourceEventCapacity];
            automaticFoamPacketReservations =
                new AutomaticFoamPacketReservation[
                    automaticFoamPacketReservationCapacity];
        }

        private float ResolveInitializationMotionTime()
        {
            return (int)initializationPhase >
                    (int)InitializationPhase.ResolveDimensions &&
                (int)initializationPhase <
                    (int)InitializationPhase.Ready
                    ? initializationMotionTime
                    : river.MotionTime;
        }

        private void FinalizeInitialization()
        {
            if (boundaryDirty)
            {
                // A source changed after the boundary phase. Re-run the
                // boundary and accepted topology-source composition while
                // Foam remains disabled.
                initializationPhase = InitializationPhase.BuildBoundary;
                return;
            }

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            int currentObstacleGeometryVersion =
                disturbanceRuntime != null
                    ? disturbanceRuntime.ObstacleGeometryVersion
                    : -1;
            if (currentObstacleGeometryVersion != obstacleGeometryVersion)
            {
                // The disturbance runtime can settle generated sources over
                // several frames. Keep the partially initialized river hidden
                // and rebuild the obstacle-dependent tail once it stabilises.
                initializationObstacleObservedVersion = int.MinValue;
                initializationObstacleStableFrameCount = 0;
                initializationPhase = DevelopmentTopologyGenerationInProgress
                    ? InitializationPhase.WaitForObstacleStability
                    : InitializationPhase.ResolveTopologyCache;
                return;
            }

            if (!activeTopologyRequiresExplicitPreparation &&
                majorTopologyInputSignature !=
                    ResolveMajorTopologyInputSignature())
            {
                initializationPhase = DevelopmentTopologyGenerationInProgress
                    ? InitializationPhase.BuildMajorTopology
                    : InitializationPhase.ResolveTopologyCache;
                return;
            }

            if (!activeTopologyRequiresExplicitPreparation &&
                connectorTopologyInputSignature !=
                    ResolveConnectorTopologyInputSignature())
            {
                initializationPhase = DevelopmentTopologyGenerationInProgress
                    ? InitializationPhase.BuildConnectorTopology
                    : InitializationPhase.ResolveTopologyCache;
                return;
            }

            if (!activeTopologyRequiresExplicitPreparation &&
                pocketTopologyInputSignature !=
                    ResolvePocketTopologyInputSignature())
            {
                initializationPhase = DevelopmentTopologyGenerationInProgress
                    ? InitializationPhase.BuildPocketTopology
                    : InitializationPhase.ResolveTopologyCache;
                return;
            }

            resourcesDirty = false;
            ApplyPersistentStateReplacementBeforeVisibleHoldRelease();
            ReleaseTopologyTransitionVisibleHold();
            simulationAccumulator = 0f;
            lastMaximumTransportCfl = 0f;
            lastRequiredTransportSubsteps = 1;
            lastUsedTransportSubsteps = 1;
            transportSafetyLimitExceeded = false;
            transportSafetyStatus = "CFL transport ready";
            transportMetricsAccumulator = 0f;
            topologyMetricsAccumulator = 0f;
            simulationInterpolation = 1f;
            lastRenderInterpolationAlpha = simulationInterpolation;
            lastRuntimeTime = Time.realtimeSinceStartup;
            rebuildPhase = RebuildPhase.Idle;
            pendingBoundaryRebuild = false;
            pendingObstacleRebuild = false;
            pendingTopologyRefresh = false;
            pendingTopologyReplacementAfterMaintenance = false;
            pendingObstacleObservedVersion = int.MinValue;
            pendingObstacleStableFrameCount = 0;
            initializationObstacleObservedVersion = int.MinValue;
            initializationObstacleStableFrameCount = 0;
            initializationPhase = InitializationPhase.Ready;
            if (DevelopmentTopologyGenerationInProgress)
            {
                CompleteDevelopmentTopologyGeneration();
            }

        }

        private static int ResolveStructuralResolution(
            StylizedRiverQuality quality)
        {
            return quality switch
            {
                StylizedRiverQuality.Low => LowStructuralResolution,
                StylizedRiverQuality.Medium => MediumStructuralResolution,
                StylizedRiverQuality.High => HighStructuralResolution,
                _ => MediumStructuralResolution
            };
        }

        private static bool TryResolveChunkedTextureWidth(
            int chunks,
            int desiredResolutionPerChunk,
            int minimumResolutionPerChunk,
            int maximumTextureSize,
            out int resolvedResolutionPerChunk,
            out int resolvedWidth)
        {
            resolvedResolutionPerChunk = 0;
            resolvedWidth = 0;
            if (chunks < 1 ||
                maximumTextureSize < minimumResolutionPerChunk ||
                (long)chunks * minimumResolutionPerChunk > maximumTextureSize)
            {
                return false;
            }

            resolvedResolutionPerChunk = Math.Min(
                desiredResolutionPerChunk,
                maximumTextureSize / chunks);
            if (resolvedResolutionPerChunk < minimumResolutionPerChunk)
            {
                return false;
            }

            long width = (long)resolvedResolutionPerChunk * chunks;
            if (width < 1 || width > maximumTextureSize)
            {
                return false;
            }

            resolvedWidth = (int)width;
            return true;
        }

        private void ReportFixedMetricAllocationFailure(
            int maximumTextureSize)
        {
            if (allocationWarningReported)
            {
                return;
            }

            Debug.LogWarning(
                $"Stage 6 Foam on '{name}' is disabled because the selected " +
                $"fixed-metric field could not be allocated within the " +
                $"hardware texture limit of {maximumTextureSize} pixels. " +
                $"{fixedMetricCandidateFailureReason}",
                this);
            allocationWarningReported = true;
        }

        private void ReportAllocationFailure(int maximumTextureSize)
        {
            if (allocationWarningReported)
            {
                return;
            }

            Debug.LogWarning(
                $"Stage 6 Foam on '{name}' is disabled because the required " +
                $"textures for {chunkCount} chunks cannot fit within the " +
                $"hardware texture limit of {maximumTextureSize} pixels. " +
                "The field requires at least 16 columns per chunk.",
                this);
            allocationWarningReported = true;
        }

        private static void ClearRenderTexture(RenderTexture texture)
        {
            if (texture == null || !texture.IsCreated())
            {
                return;
            }

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = texture;
            GL.Clear(false, true, Color.clear);
            RenderTexture.active = previous;
        }

        private RenderTexture CreateFieldTexture(string textureName)
        {
            RenderTexture texture = new RenderTexture(
                fieldWidth,
                fieldHeight,
                0,
                RenderTextureFormat.ARGBHalf,
                RenderTextureReadWrite.Linear)
            {
                name = textureName,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            texture.Create();
            return texture;
        }

        private RenderTexture CreateShapeMaskTexture(string textureName)
        {
            RenderTexture texture = new RenderTexture(
                fieldWidth,
                fieldHeight,
                0,
                RenderTextureFormat.RHalf,
                RenderTextureReadWrite.Linear)
            {
                name = textureName,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            texture.Create();
            return texture;
        }

        private RenderTexture CreateFilmTexture(string textureName)
        {
            RenderTexture texture = new RenderTexture(
                filmFieldWidth,
                filmFieldHeight,
                0,
                RenderTextureFormat.RHalf,
                RenderTextureReadWrite.Linear)
            {
                name = textureName,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            texture.Create();
            return texture;
        }


        private RenderTexture CreateStructuralTexture(string textureName)
        {
            return CreateTopologyTexture(
                structuralWidth,
                structuralHeight,
                textureName);
        }

        private static RenderTexture CreateTopologyTexture(
            int width,
            int height,
            string textureName)
        {
            RenderTexture texture = new RenderTexture(
                Mathf.Max(1, width),
                Mathf.Max(1, height),
                0,
                RenderTextureFormat.ARGBHalf,
                RenderTextureReadWrite.Linear)
            {
                name = textureName,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            texture.Create();
            return texture;
        }

        private RenderTexture CreateShoreEdgesTexture(string textureName)
        {
            RenderTexture texture = new RenderTexture(
                structuralWidth,
                1,
                0,
                RenderTextureFormat.RGHalf,
                RenderTextureReadWrite.Linear)
            {
                name = textureName,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            texture.Create();
            return texture;
        }

        private RenderTexture CreateMajorEvolutionTexture(
            string textureName)
        {
            RenderTexture texture = new RenderTexture(
                structuralWidth,
                structuralHeight,
                0,
                RenderTextureFormat.RHalf,
                RenderTextureReadWrite.Linear)
            {
                name = textureName,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            texture.Create();
            return texture;
        }

        private RenderTexture CreateObstacleExclusionTexture(
            string textureName)
        {
            RenderTexture texture = new RenderTexture(
                fieldWidth,
                fieldHeight,
                0,
                RenderTextureFormat.RHalf,
                RenderTextureReadWrite.Linear)
            {
                name = textureName,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            texture.Create();
            return texture;
        }



        private Texture2D CreateMotionLaneTexture()
        {
            Texture2D texture = new Texture2D(
                fieldWidth,
                fieldHeight,
                TextureFormat.RHalf,
                false,
                true)
            {
                name = "T_PS3D_RiverFoamMotionLane_Runtime",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            return texture;
        }

        private Texture2D CreateObstacleRoutingTexture()
        {
            Texture2D texture = new Texture2D(
                fieldWidth,
                fieldHeight,
                TextureFormat.RGHalf,
                false,
                true)
            {
                name = "T_PS3D_RiverFoamObstacleRouting_Runtime",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            return texture;
        }

        private static RenderTexture CreateNeutralDisturbanceTexture()
        {
            RenderTexture texture = new RenderTexture(
                1,
                1,
                0,
                RenderTextureFormat.ARGBHalf,
                RenderTextureReadWrite.Linear)
            {
                name = "PS3D_RiverFoam_NeutralDisturbance",
                enableRandomWrite = false,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            texture.Create();

            RenderTexture previous = RenderTexture.active;
            try
            {
                RenderTexture.active = texture;
                GL.Clear(false, true, Color.clear);
            }
            finally
            {
                RenderTexture.active = previous;
            }

            return texture;
        }




        private void ReleaseResources(
            bool releaseTopologyTransition = true)
        {
            ReleaseTopologyReplacementBuild(true);
            if (releaseTopologyTransition)
            {
                ReleaseTopologyTransition(true);
            }
            ReleaseTexture(ref stateA);
            ReleaseTexture(ref stateB);
            ReleaseTexture(ref presentationPreviousState);
            ReleaseTexture(ref shapeMaskTexture);
            ReleaseTexture(ref filmSourceTexture);
            ReleaseTexture(ref filmSupportTexture);
            ReleaseTexture(ref visualOccupancyA);
            ReleaseTexture(ref visualOccupancyB);
            ReleaseAutomaticBirthDiagnosticResources();
            ReleaseTexture(ref topologyTexture);
            ReleaseTexture(ref topologySourcesTexture);
            ReleaseTexture(ref topologyGeneratedTexture);
            ReleaseTexture(ref evolvingMajorTexture);
            ReleaseTexture(ref evolvingHostedNegativeTexture);
            ReleaseTexture(ref evolvingFreeWaterNegativeTexture);
            ReleaseTexture(ref evolvingConnectorTexture);
            ReleaseTexture(ref evolvingWeakSpanNegativeTexture);
            ReleaseTexture(ref currentShoreEdgesTexture);
            ReleaseTexture(ref obstacleExclusionTexture);
            ReleaseTexture(ref objectContactFieldTexture);
            if (motionLaneTexture != null)
            {
                DestroyUnityObject(motionLaneTexture);
                motionLaneTexture = null;
            }
            if (obstacleRoutingTexture != null)
            {
                DestroyUnityObject(obstacleRoutingTexture);
                obstacleRoutingTexture = null;
            }
            ReleaseTexture(ref neutralDisturbanceTexture);
            previousState = null;
            currentState = null;
            writeState = null;
            currentVisualOccupancy = null;
            writeVisualOccupancy = null;
            shapeProductDebugActiveLastUpdate = false;

            ReleaseMajorEvolutionResources();

            if (boundaryTexture != null)
            {
                DestroyUnityObject(boundaryTexture);
                boundaryTexture = null;
            }

            if (topologyGeneratedUploadTexture != null)
            {
                DestroyUnityObject(topologyGeneratedUploadTexture);
                topologyGeneratedUploadTexture = null;
            }

            metricBuffer?.Release();
            metricBuffer = null;
            automaticFoamSourceEventBuffer?.Release();
            automaticFoamSourceEventBuffer = null;
            ReleaseObstacleExclusionBuffers();
            obstacleExclusionMeshFilters.Clear();
            topologyCacheFingerprintMeshFilters.Clear();
            topologyCachePreparedObstacleFingerprints.Clear();
            obstacleExclusionCells.Clear();
            obstacleExclusionSamples.Clear();
            obstacleExclusionGpuCells =
                Array.Empty<FoamObstacleIntervalCellData>();
            obstacleExclusionScalar = Array.Empty<float>();
            motionLaneHalfData = Array.Empty<ushort>();
            obstacleRoutingHalfData = Array.Empty<ushort>();
            motionLaneRawValues = Array.Empty<float>();
            obstacleRoutingOccupied = Array.Empty<bool>();
            obstacleRoutingVisited = Array.Empty<bool>();
            obstacleRoutingComponentIds = Array.Empty<int>();
            obstacleRoutingQueue = Array.Empty<int>();
            obstacleRoutingComponents.Clear();
            motionLaneFieldSignature = int.MinValue;
            obstacleRoutingFieldSignature = int.MinValue;
            motionLaneScrollCells = 0f;
            lastMotionLaneScrollCells = 0f;
            lastMotionLaneSignature = int.MinValue;
            lastObstacleRoutingSignature = int.MinValue;
            topologyCacheLoadedForActiveResources = false;
            obstacleExclusionUsesCachedScalar = false;
            activeTopologyObstacleStale = false;
            activeTopologyRequiresExplicitPreparation = false;
            pendingStartupCachePackage = null;
            pendingStartupCacheStaleObstacles = false;
            pendingStartupCacheStaleSettings = false;
            explicitTopologyGenerationInProgress =
                editorTopologyPreparationInProgress;
            automaticTopologyCacheWritePending = false;
            topologyGeneratedUploadPixels = Array.Empty<Color>();
            majorTopology = null;
            connectorTopology = null;
            pocketTopology = null;
            majorTopologyInputSignature = int.MinValue;
            connectorTopologyInputSignature = int.MinValue;
            pocketTopologyInputSignature = int.MinValue;
            obstacleGeometryVersion = -1;
            if (topologyMetricsReadbackPending)
            {
                // The request callback owns this retired buffer until the GPU
                // copy completes. Releasing it here can invalidate the request.
                topologyMetricsBuffer = null;
            }
            else
            {
                topologyMetricsBuffer?.Release();
                topologyMetricsBuffer = null;
            }

            topologyMetricsGeneration++;
            topologyMetricsReadbackPending = false;
            topologyMetricsAvailable = false;
            topologyMetricsLastCompletedAt = -1.0;

            if (transportMetricsReadbackPending)
            {
                transportMetricsBuffer = null;
            }
            else
            {
                transportMetricsBuffer?.Release();
                transportMetricsBuffer = null;
            }

            transportMetricsGeneration++;
            transportMetricsReadbackPending = false;
            transportMetricsAvailable = false;
            transportMetricsReadbackRequestedAt = -1.0;
            transportMetricsLastCompletedAt = -1.0;
            transportMetricsAccumulator = 0f;
            Array.Clear(
                latestTransportMetrics,
                0,
                latestTransportMetrics.Length);
            ResetTransportMetricValues();
            integratedPresenceArea = 0f;
            visiblePresenceCoreArea = 0f;
            materialLifetimeAuthorityActive = false;
            materialLifetimeEmptyMetricReadbacks = 0;
            lifetimeAuthorityStatus = "No live material known";
            materialClockSessionActive = false;
            materialClockSessionStartedAt = -1.0;
            materialSimulatedSecondsSinceSession = 0f;
            materialStepCountSinceSession = 0;
            pendingIsolatedLifeProbeAbsoluteAging = false;
            isolatedLifeProbeAbsoluteAgingActive = false;
            isolatedLifeProbeWrittenAt = -1.0;
            lastConfiguredFoamDeltaTime = 0f;
            lastConfiguredAbsoluteLifeProbeActive = false;
            birthCommandsThisFrame = 0;
            lastBirthCommandAt = -1.0;
            manualProofReferenceArea = 0f;
            manualProofReferencePending = false;
            Array.Clear(
                latestTopologyMetrics,
                0,
                latestTopologyMetrics.Length);
            metricRows = Array.Empty<FoamMetricRow>();
            computeShader = null;
            clearKernel = -1;
            rasterizeFoamSourceEventKernel = -1;
            rasterizeFoamSourceEventDebugKernel = -1;
            writeIsolatedLifeProbeKernel = -1;
            clearAutomaticBirthDebugAllKernel = -1;
            buildCurrentShoreEdgesKernel = -1;
            composeTopologyKernel = -1;
            captureGeneratedTopologyKernel = -1;
            buildEvolvingMajorSupportKernel = -1;
            clearObstacleExclusionKernel = -1;
            updateObstacleExclusionKernel = -1;
            buildObjectContactFieldKernel = -1;
            resetTopologyMetricsKernel = -1;
            measureTopologyMetricsKernel = -1;
            resetTransportMetricsKernel = -1;
            remapPersistentStateKernel = -1;
            simulateKernel = -1;
            bulkTransportPhaseCells = 0f;
            previousBulkTransportPhaseCells = 0f;
            bulkTransportIntegerShift = 0;
            buildFilmSourceKernel = -1;
            buildFilmSupportKernel = -1;
            evaluateShapeKernel = -1;
            applyBoundaryKernel = -1;
            fieldWidth = 0;
            fieldHeight = 0;
            filmFieldWidth = 0;
            filmFieldHeight = 0;
            structuralWidth = 0;
            structuralHeight = 0;
            chunkCount = 0;
            resolutionPerChunk = 0;
            gridDescriptor = default;
            gridDescriptorGpuData = default;
            fixedMetricCandidateDescriptor = default;
            fixedMetricCandidateFailureReason = "Not resolved";
            fieldLength = 0f;
            validFieldLength = 0f;
            simulationFieldLength = 0f;
            minimumTransportLongitudinalSpacing = 0f;
            minimumTransportLateralSpacing = 0f;
            minimumTransportCurvatureJacobian = 1f;
            minimumTransportRawJacobian = 1f;
            maximumAbsoluteTransportCurvatureLateralProduct = 0f;
            allocatedGlobalStart = 0f;
            initializationMotionTime = 0f;
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
            initializationPhase = InitializationPhase.NotStarted;
            topologyReplacementRequestSequence = 0;
            topologyReplacementRequestCount = 0;
            topologyReplacementCoalescedCount = 0;
            topologyReplacementCancelledCount = 0;
            topologyReplacementActivatedCount = 0;
            topologyReplacementIdenticalPreparedCount = 0;
            topologyReplacementLastReason = "None";
            if (releaseTopologyTransition)
            {
                topologyTransitionStartedCount = 0;
                topologyTransitionCompletedCount = 0;
                topologyTransitionRemappedCount = 0;
                topologyTransitionFlattenedCount = 0;
                persistentStateReplacementExactCount = 0;
                persistentStateReplacementClearCount = 0;
                lastPersistentStateReplacementPolicy =
                    PersistentStateReplacementPolicy.None;
                lastPersistentStateReplacementDetail = "None";
            }
            domainVersion = -1;
            topologyMetricsAccumulator = 0f;
        }


        private static bool IsCreatedTexture(RenderTexture texture)
        {
            return texture != null && texture.IsCreated();
        }

        private static long EstimateTextureBytes(Texture texture)
        {
            if (texture == null)
            {
                return 0L;
            }

            long bytesPerPixel = 4L;
            if (texture is RenderTexture renderTexture)
            {
                bytesPerPixel = renderTexture.format switch
                {
                    RenderTextureFormat.ARGBHalf => 8L,
                    RenderTextureFormat.RGHalf => 4L,
                    RenderTextureFormat.RHalf => 2L,
                    _ => 4L
                };
            }
            else if (texture is Texture2D texture2D)
            {
                bytesPerPixel = texture2D.format switch
                {
                    TextureFormat.RGBAHalf => 8L,
                    TextureFormat.RGHalf => 4L,
                    TextureFormat.RHalf => 2L,
                    _ => 4L
                };
            }
            else if (texture is Texture2DArray textureArray)
            {
                bytesPerPixel = textureArray.format switch
                {
                    TextureFormat.RGBAHalf => 8L,
                    TextureFormat.RGHalf => 4L,
                    TextureFormat.RHalf => 2L,
                    _ => 4L
                };
                return (long)texture.width * texture.height *
                    textureArray.depth * bytesPerPixel;
            }

            return (long)texture.width * texture.height * bytesPerPixel;
        }

        private static void ReleaseTexture(ref RenderTexture texture)
        {
            if (texture == null)
            {
                return;
            }

            // Explicit Editor cache preparation can finish while one of its
            // temporary targets is still bound as RenderTexture.active. Unity
            // warns when that target is released. Unbind only the exact target
            // being destroyed; unrelated active render targets remain intact.
            if (RenderTexture.active == texture)
            {
                RenderTexture.active = null;
            }

            texture.Release();
            DestroyUnityObject(texture);
            texture = null;
        }

        private static void DestroyUnityObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
