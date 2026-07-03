using System;
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
            if (initializationPhase == InitializationPhase.Ready)
            {
                CompleteTopologyStartupValidation();
            }

            return initializationPhase == InitializationPhase.Ready;
        }


        private bool AreResourcesCompleteAndCurrent()
        {
            return !resourcesDirty &&
                currentState != null &&
                advectedState != null &&
                reverseState != null &&
                progressiveBirthSourceTexture != null &&
                progressiveBirthTransferDebugTexture != null &&
                guidanceTexture != null &&
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
                neutralDisturbanceTexture != null &&
                neutralDisturbanceTexture.IsCreated() &&
                boundaryTexture != null &&
                metricBuffer != null &&
                topologyMetricsBuffer != null &&
                domainVersion == river.Domain.Version &&
                allocatedQuality == river.Quality;
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
                allocatedQuality != river.Quality;
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
                        advectedState = CreateFieldTexture(
                            "PS3D_RiverFoam_Advected");
                        reverseState = CreateFieldTexture(
                            "PS3D_RiverFoam_Reverse");
                        progressiveBirthSourceTexture = CreateFieldTexture(
                            "PS3D_RiverFoam_ProgressiveBirthSource");
                        progressiveBirthTransferDebugTexture =
                            CreateFieldTexture(
                                "PS3D_RiverFoam_ProgressiveBirthTransferDebug");
                    }

                    initializationPhase =
                        InitializationPhase.AllocateTopologyTextures;
                    break;

                case InitializationPhase.AllocateTopologyTextures:
                    using (InitAllocateTopologyTexturesProfilerMarker.Auto())
                    {
                        guidanceTexture = CreateGuidanceTexture(
                            "PS3D_RiverFoam_Guidance");
                        topologyTexture = CreateGuidanceTexture(
                            "PS3D_RiverFoam_Topology");
                        topologySourcesTexture = CreateGuidanceTexture(
                            "PS3D_RiverFoam_TopologySources");
                        // Deterministic prepared topology is uploaded here.
                        // Connector and independent-negative classes remain only
                        // as complete static fallbacks when their prepared
                        // runtime reconstruction is unavailable.
                        topologyGeneratedTexture = CreateGuidanceTexture(
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
                    }

                    initializationPhase =
                        InitializationPhase.AllocateAuxiliaryTextures;
                    break;

                case InitializationPhase.AllocateAuxiliaryTextures:
                    using (InitAllocateAuxiliaryTexturesProfilerMarker.Auto())
                    {
                        neutralDisturbanceTexture =
                            CreateNeutralDisturbanceTexture();
                        previousState = stateA;
                        currentState = stateA;
                        writeState = stateB;
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

                        chunkActive = new bool[chunkCount];
                        chunkActiveUntil = new double[chunkCount];
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
                        else if (IsAutomaticDevelopmentCacheEnabled)
                        {
                            BeginAutomaticDevelopmentStartupGeneration();
                            initializationPhase =
                                InitializationPhase.BuildObstacleExclusion;
                        }
                        else
                        {
                            initializationPhase =
                                InitializationPhase
                                    .AwaitExplicitTopologyGeneration;
                        }
                    }
                    break;

                case InitializationPhase.AwaitExplicitTopologyGeneration:
                    if (explicitTopologyGenerationRequested)
                    {
                        explicitTopologyGenerationRequested = false;
                        initializationPhase =
                            InitializationPhase.BuildObstacleExclusion;
                    }
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
                        else if (IsAutomaticDevelopmentCacheEnabled)
                        {
                            BeginAutomaticDevelopmentStartupGeneration();
                            initializationPhase =
                                InitializationPhase.BuildObstacleExclusion;
                        }
                        else
                        {
                            initializationPhase = InitializationPhase
                                .AwaitExplicitTopologyGeneration;
                        }
                    }
                    break;

                case InitializationPhase.BuildObstacleExclusion:
                    topologyStartupValidationObstacleBuildCount++;
                    RebuildObstacleExclusionCache();
                    initializationObstacleObservedVersion = int.MinValue;
                    initializationObstacleStableFrameCount = 0;
                    initializationPhase = InitializationPhase.BuildMajorTopology;
                    break;

                case InitializationPhase.BuildMajorTopology:
                    topologyStartupValidationMajorBuildCount++;
                    BuildMajorTopology();
                    initializationPhase =
                        InitializationPhase.BuildConnectorTopology;
                    break;

                case InitializationPhase.BuildConnectorTopology:
                    topologyStartupValidationConnectorBuildCount++;
                    BuildConnectorTopology();
                    initializationPhase =
                        InitializationPhase.BuildPocketTopology;
                    break;

                case InitializationPhase.BuildPocketTopology:
                    topologyStartupValidationPocketBuildCount++;
                    BuildPocketTopology();
                    initializationPhase = InitializationPhase.ClearMaterial;
                    break;

                case InitializationPhase.ClearMaterial:
                    using (InitClearMaterialProfilerMarker.Auto())
                    {
                        DispatchClear(stateA, 0, fieldWidth);
                        DispatchClear(stateB, 0, fieldWidth);
                        DispatchClear(advectedState, 0, fieldWidth);
                        DispatchClear(reverseState, 0, fieldWidth);
                        DispatchClear(
                            progressiveBirthSourceTexture,
                            0,
                            fieldWidth);
                        DispatchClear(
                            progressiveBirthTransferDebugTexture,
                            0,
                            fieldWidth);
                    }

                    initializationPhase = InitializationPhase.BuildGuidance;
                    break;

                case InitializationPhase.BuildGuidance:
                    BuildGuidanceField(0f);
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

            chunkCount = Mathf.Max(
                1,
                Mathf.CeilToInt(domain.LocalLength / ChunkLengthMetres));
            int desiredStructuralResolution =
                ResolveStructuralResolution(river.Quality);
            resolutionPerChunk = desiredStructuralResolution;

            int maximumTextureSize = SystemInfo.maxTextureSize;
            if (!TryResolveChunkedTextureWidth(
                    chunkCount,
                    resolutionPerChunk,
                    16,
                    maximumTextureSize,
                    out resolutionPerChunk,
                    out fieldWidth))
            {
                ReportAllocationFailure(maximumTextureSize);
                initializationPhase = InitializationPhase.Failed;
                return false;
            }

            fieldHeight = desiredStructuralResolution;

            // Topology and guidance use the same structural grid as persistent
            // material so narrow structures are not authored on a coarser
            // hidden lattice and then upsampled.
            guidanceWidth = fieldWidth;
            guidanceHeight = fieldHeight;
            if (maximumTextureSize < 24 ||
                fieldHeight > maximumTextureSize ||
                guidanceHeight > maximumTextureSize)
            {
                ReportAllocationFailure(maximumTextureSize);
                initializationPhase = InitializationPhase.Failed;
                return false;
            }

            fieldLength = chunkCount * ChunkLengthMetres;
            validFieldLength = domain.LocalLength;
            float longitudinalSpacing =
                StylizedRiverFoamTopologyFieldSpace.TexelSpacing(
                    fieldLength,
                    fieldWidth);
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
            initializationMotionTime = river.MotionTime;
            return true;
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

            if (!pendingAutomaticDevelopmentRebuildAfterStartup &&
                majorTopologyInputSignature !=
                    ResolveMajorTopologyInputSignature())
            {
                initializationPhase = DevelopmentTopologyGenerationInProgress
                    ? InitializationPhase.BuildMajorTopology
                    : InitializationPhase.ResolveTopologyCache;
                return;
            }

            if (!pendingAutomaticDevelopmentRebuildAfterStartup &&
                connectorTopologyInputSignature !=
                    ResolveConnectorTopologyInputSignature())
            {
                initializationPhase = DevelopmentTopologyGenerationInProgress
                    ? InitializationPhase.BuildConnectorTopology
                    : InitializationPhase.ResolveTopologyCache;
                return;
            }

            if (!pendingAutomaticDevelopmentRebuildAfterStartup &&
                pocketTopologyInputSignature !=
                    ResolvePocketTopologyInputSignature())
            {
                initializationPhase = DevelopmentTopologyGenerationInProgress
                    ? InitializationPhase.BuildPocketTopology
                    : InitializationPhase.ResolveTopologyCache;
                return;
            }

            resourcesDirty = false;
            ReleaseTopologyTransitionVisibleHold();
            simulationAccumulator = 0f;
            guidanceAccumulator = 0f;
            topologyMetricsAccumulator = 0f;
            simulationInterpolation = 1f;
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
                CompleteDevelopmentTopologyGeneration(
                    "automatic staged startup generation");
            }

            BeginAutomaticDevelopmentRebuildAfterStartup();
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


        private RenderTexture CreateGuidanceTexture(string textureName)
        {
            return CreateTopologyTexture(
                guidanceWidth,
                guidanceHeight,
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
                guidanceWidth,
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
                guidanceWidth,
                guidanceHeight,
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
            ReleaseTexture(ref advectedState);
            ReleaseTexture(ref reverseState);
            ReleaseTexture(ref progressiveBirthSourceTexture);
            ReleaseTexture(ref progressiveBirthTransferDebugTexture);
            progressiveBirthSourceContainsData = false;
            ReleaseProgressiveBirthDiagnosticResources();
            ReleaseTexture(ref guidanceTexture);
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
            ReleaseTexture(ref neutralDisturbanceTexture);
            previousState = null;
            currentState = null;
            writeState = null;

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
            ReleaseObstacleExclusionBuffers();
            obstacleExclusionMeshFilters.Clear();
            topologyCacheFingerprintMeshFilters.Clear();
            topologyCachePreparedObstacleFingerprints.Clear();
            obstacleExclusionCells.Clear();
            obstacleExclusionSamples.Clear();
            obstacleExclusionGpuCells =
                Array.Empty<FoamObstacleIntervalCellData>();
            obstacleExclusionScalar = Array.Empty<float>();
            topologyCacheLoadedForActiveResources = false;
            obstacleExclusionUsesCachedScalar = false;
            activeTopologyObstacleStale = false;
            pendingStartupCachePackage = null;
            pendingStartupCacheStaleObstacles = false;
            pendingStartupCacheStaleSettings = false;
            pendingAutomaticDevelopmentRebuildAfterStartup = false;
            explicitTopologyGenerationRequested = false;
            explicitTopologyGenerationInProgress = false;
            automaticTopologyGenerationInProgress = false;
            automaticDevelopmentObservedSignature = int.MinValue;
            automaticDevelopmentRebuildReason =
                AutomaticDevelopmentRebuildReason.None;
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
            Array.Clear(
                latestTopologyMetrics,
                0,
                latestTopologyMetrics.Length);
            metricRows = Array.Empty<FoamMetricRow>();
            computeShader = null;
            clearKernel = -1;
            injectKernel = -1;
            buildGuidanceKernel = -1;
            buildCurrentShoreEdgesKernel = -1;
            composeTopologyKernel = -1;
            captureGeneratedTopologyKernel = -1;
            buildEvolvingMajorSupportKernel = -1;
            clearObstacleExclusionKernel = -1;
            updateObstacleExclusionKernel = -1;
            resetTopologyMetricsKernel = -1;
            measureTopologyMetricsKernel = -1;
            advectForwardKernel = -1;
            advectReverseKernel = -1;
            simulateKernel = -1;
            applyBoundaryKernel = -1;
            fieldWidth = 0;
            fieldHeight = 0;
            guidanceWidth = 0;
            guidanceHeight = 0;
            chunkCount = 0;
            resolutionPerChunk = 0;
            fieldLength = 0f;
            validFieldLength = 0f;
            simulationFieldLength = 0f;
            allocatedGlobalStart = 0f;
            initializationMotionTime = 0f;
            chunkActive = Array.Empty<bool>();
            chunkActiveUntil = Array.Empty<double>();
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
            }
            domainVersion = -1;
            guidanceAccumulator = 0f;
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
                    _ => 4L
                };
            }
            else if (texture is Texture2DArray textureArray)
            {
                bytesPerPixel = textureArray.format switch
                {
                    TextureFormat.RGBAHalf => 8L,
                    TextureFormat.RGHalf => 4L,
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
