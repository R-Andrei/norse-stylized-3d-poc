using System;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private const string ComputeResourcePath =
            "PS3DRiver/Compute/CS_RiverFoam";
        private const float ChunkLengthMetres = 32f;
        // Stage 6 structural resolution is shared by persistent material,
        // topology, guidance, and the authoritative obstacle mask. Medium is
        // the standard project tier; Low and High preserve the same physical
        // topology scale while changing only spatial precision and workload.
        private const int LowStructuralResolution = 64;
        private const int MediumStructuralResolution = 96;
        private const int HighStructuralResolution = 128;
        private const float ResourceReleaseDelaySeconds = 2f;
        private const float MaximumManualReservationSeconds = 300f;
        private const float DecayToFivePercent = 2.995732f;
        private const float EndOfLifeDissipationSeconds = 4f;
        private const float EndOfLifeDissipationStart = 0.35f;
        private const float TopologyMetricsUpdateRate = 2f;
        private const int ProgressiveRibbonEventCapacity = 8;
        private const int ProgressiveBirthDebugCounterCount = 2;
        private const float ProgressiveRibbonMinimumDuration = 0.5f;
        private const float ProgressiveRibbonMaximumDuration = 3f;
        private const float ProgressiveRibbonMinimumTravelDistance = 0.5f;
        private const float ProgressiveRibbonMaximumTravelDistance = 8f;
        private const float ProgressiveRibbonMaximumBendAcross = 0.35f;
        private const float ProgressiveRibbonWidthVariation = 0.20f;
        private const float ProgressiveRibbonRampInEnd = 0.18f;
        private const float ProgressiveRibbonTaperStart = 0.72f;
        private const float ProgressiveRibbonMinimumHalfWidth = 0.05f;
        private const float ProgressiveRibbonMaximumHalfWidth = 1f;
        // Patch 4.11C.3 converts source Amount into a deterministic spatial
        // subset. The feature scale is physical, source-relative, and is later
        // clamped against the actual field spacing in HLSL so Low quality does
        // not collapse the fill into one-texel checkerboard fragments.
        private const float SourceFillMinimumFeatureSizeMetres = 0.20f;
        private const float SourceFillFeatureSizeRadiusMultiplier = 0.65f;
        private const float ProgressiveSourceFillSeedSalt = 83.173f;
        private const float ManualSourceFillSeedSalt = 141.919f;
        private const int ThreadGroupSize = 8;
        private const int TopologyMetricCount = 16;
        // Canonical Stage 6 Shore Support band measured inward from the
        // instantaneous Stage 3 visible water edge. These metric widths remain
        // fixed while the accepted anchored-support contract is retained.
        private const float ShoreSupportCoreWidthMetres = 0.228127f;
        private const float ShoreSupportFadeWidthMetres = 0.03f;
        private const int ObstacleRebuildStableFrameCount = 2;
        // Patch 4.8B uses one bounded generated-topology transition. This is
        // an internal proof duration rather than a public authoring control.
        private const float TopologyReplacementTransitionSeconds = 1.0f;
        // Patch 4.9C.1 waits briefly after a development setting change before
        // preparing a replacement, so dragging one Inspector control does not
        // launch a chain of immediately obsolete generator requests.
        private const double AutomaticDevelopmentRebuildDebounceSeconds = 0.35;

        // Patch 4.6 Major Support evolution. These are provisional internal
        // coefficients, not public authoring controls.
        private const float MajorEvolutionTickRate = 5f;
        private const float MajorMinimumDwellSeconds = 2f;
        private const float MajorMaximumDwellSeconds = 5f;
        private const float MajorMinimumMoveSeconds = 1f;
        private const float MajorMaximumMoveSeconds = 2f;
        private const float MajorMinimumHopMetres = 0.35f;
        private const float MajorMaximumHopMetres = 1.20f;
        private const float MajorMinimumLateralHop = 0.045f;
        private const float MajorMaximumLateralHop = 0.20f;

        // Time-only expiry let slow-looking occurrences occupy one neighbourhood
        // for too long, while hop-only expiry let rapidly changing occurrences
        // consume many topology states before recycling. Patch 4.6.2 charges
        // both elapsed time and completed hops against one predictable budget.
        private const float MajorLifetimeSecondsPerUnit = 5f;
        private const float MajorLifetimeTimeWeight = 0.60f;
        private const float MajorLifetimeHopWeight = 0.40f;
        private const float MajorLifetimeSafetyMultiplier = 1.25f;

        // Patch 4.7A local variation remains deliberately subordinate to the
        // host transform. Interior Pockets vary more often; Edge Cavities use
        // tighter boundary-tangent changes so their breach side stays coherent.
        private const float HostedInteriorChangeProbability = 0.60f;
        private const float HostedCavityChangeProbability = 0.38f;

        // Patch 4.7B.1 gives every independent Free-Water event the same
        // single-instance downstream/lifetime/recycle state pattern as Major
        // Support, with deliberately slower timings and no public controls.
        private const float FreeWaterMinimumDwellSeconds = 5f;
        private const float FreeWaterMaximumDwellSeconds = 10f;
        private const float FreeWaterMinimumMoveSeconds = 2f;
        private const float FreeWaterMaximumMoveSeconds = 4f;
        private const float FreeWaterMinimumHopMetres = 0.28f;
        private const float FreeWaterMaximumHopMetres = 0.90f;
        private const float FreeWaterMinimumLateralHop = 0.07f;
        private const float FreeWaterMaximumLateralHop = 0.28f;
        private const float FreeWaterLifetimeSecondsPerUnit = 10f;
        private const float FreeWaterLifetimeTimeWeight = 0.60f;
        private const float FreeWaterLifetimeHopWeight = 0.40f;
        private const float FreeWaterLifetimeSafetyMultiplier = 1.25f;
        private const float FreeWaterMinimumLifetimeUnits = 2.5f;
        private const float FreeWaterMaximumLifetimeUnits = 4.5f;
        private const float FreeWaterEvolutionTickRate = 2f;

        // Patch 4.7C.3 keeps Connector evolution subordinate to its two Major
        // hosts. The accepted or prevalidated replacement polyline is warped
        // only by endpoint motion plus a small bounded interior displacement.
        // These are internal proof coefficients, not authoring controls.
        private const float ConnectorMinimumInteriorDeformationMetres = 0.018f;
        private const float ConnectorMaximumInteriorDeformationMetres = 0.085f;
        private const int ConnectorEndpointWarpSolveIterations = 4;

        // Patch 4.7C.3.3 keeps relationship concentration probabilistic rather
        // than imposing a per-Major degree ceiling. Every occupied endpoint
        // sharply reduces selection weight, while a non-zero tail preserves the
        // possibility of occasional hubs. Crowded relationships also turn over
        // more readily when either host begins a new occurrence.
        private const float ConnectorLoadPenaltyBase = 0.22f;
        private const float ConnectorHubPenaltyBase = 0.60f;
        private const float ConnectorTurnoverBaseProbability = 0.50f;
        private const float ConnectorTurnoverCrowdingIncrement = 0.15f;
        private const float ConnectorTurnoverMaximumProbability = 0.90f;
        private const float ConnectorLengthPreferenceWeightStrength = 1.35f;
        private const float ConnectorSelectionJitterMinimum = 0.90f;
        private const float ConnectorSelectionJitterMaximum = 1.10f;

        // The old broad Foam authoring controls were removed in Patch 3.4.
        // Persistent material behaviour remains on one fixed provisional
        // baseline matching the supplied project state until the dedicated
        // lifecycle pass proves and exposes a new canonical control set. These
        // are implementation values, not public
        // topology authoring controls.
        private const float ProvisionalMaterialStrength = 1.342259f;
        private const float ProvisionalMaterialCoverage = 0.394613f;
        private const float ProvisionalMaterialSharpness = 0.78f;
        private const float ProvisionalMaterialDetailScale = 0.5796f;
        private const float ProvisionalMaterialDetailStrength = 0.637799f;
        private const float ProvisionalMaterialShapeVariety = 0.763854f;
        private const float ProvisionalMaterialFlowFollow = 1.02818f;
        private const float ProvisionalMaterialEvolution = 1.26135f;
        private const float ProvisionalMaterialSpread = 0.42858f;
        private const float ProvisionalMaterialCohesion = 0.056401f;
        private const float ProvisionalMaterialConnectivity = 0.228127f;
        private const float ProvisionalMaterialShoreRetention = 1.35f;
        private const float ProvisionalMaterialVisibleThreshold = 0.16432f;
        private const float ProvisionalMaterialGuidanceStrength = 1.15646f;
        private const float ProvisionalMaterialBoundaryAttraction = 1.951f;
        private const float ProvisionalMaterialWakeReinforcement = 1.047333f;
        private const float ProvisionalMaterialImpactReinforcement = 0f;

        private enum TopologyCacheStartupResolution
        {
            Miss,
            ExactHit,
            StaleCompatible
        }

        private enum AutomaticDevelopmentRebuildReason
        {
            None,
            StartupMiss,
            Settings,
            Obstacles
        }

        private enum InitializationPhase
        {
            NotStarted,
            ReleaseOldResources,
            LoadCompute,
            ResolveKernels,
            ResolveDimensions,
            AllocateMaterialTextures,
            AllocateTopologyTextures,
            AllocateAuxiliaryTextures,
            ClearTopology,
            AllocateBuffers,
            BuildMetricBuffer,
            BuildBoundary,
            WaitForObstacleStability,
            ResolveTopologyCache,
            AwaitExplicitTopologyGeneration,
            InstallCachedTopology,
            BuildObstacleExclusion,
            BuildMajorTopology,
            BuildConnectorTopology,
            BuildPocketTopology,
            ClearMaterial,
            BuildGuidance,
            RefreshInitialTopologySources,
            Finalize,
            Ready,
            Failed
        }

        private enum RebuildPhase
        {
            Idle,
            BuildBoundary,
            ApplyBoundary,
            WaitForObstacleStability,
            BuildObstacleExclusion,
            RefreshTopologySources
        }

        private enum TopologyReplacementPhase
        {
            Idle,
            BuildMajorTopology,
            BuildConnectorTopology,
            BuildPocketTopology,
            PrepareGeneratedTexture,
            ReadyToActivate
        }

        [Flags]
        private enum TopologyReplacementReason
        {
            None = 0,
            Settings = 1 << 0,
            Boundary = 1 << 1,
            Obstacle = 1 << 2,
            IdenticalValidation = 1 << 3
        }

        private sealed class TopologyReplacementBuild
        {
            public int RequestId;
            public int TargetSignature;
            public int MajorInputSignature;
            public int ConnectorInputSignature;
            public int PocketInputSignature;
            public TopologyReplacementReason Reason;
            public bool IsIdenticalValidation;
            public RiverDomainSnapshot Domain;
            public StylizedRiverQuality Quality;
            public int FieldWidth;
            public int FieldHeight;
            public float FieldLength;
            public float ValidFieldLength;
            public float ShoreMotion;
            public float MajorAmount;
            public float MajorSize;
            public float MajorSizeVariation;
            public float MajorRecycleTerritoryDeviationPercent;
            public int MajorSeed;
            public float ConnectorAmount;
            public float ConnectorDirectness;
            public float ConnectorLengthPreference;
            public float InteriorPocketAmount;
            public float EdgeCavityAmount;
            public float ConnectorWeakSpanAmount;
            public float FreeWaterEventAmount;
            public float[] ObstacleExclusion = Array.Empty<float>();
            public StylizedRiverFoamMajorTopology MajorTopology;
            public StylizedRiverFoamConnectorTopology ConnectorTopology;
            public StylizedRiverFoamPocketTopology PocketTopology;
            public RenderTexture GeneratedTexture;
        }

        private sealed class TopologyTransitionSnapshot
        {
            public RenderTexture GeneratedTexture;
            public ComputeBuffer MetricBuffer;
            public int Width;
            public int Height;
            public int DomainVersion;
            public float GlobalStart;
            public float FieldLength;
            public float ValidFieldLength;

            // Dimension-changing transitions keep the last complete renderer
            // bindings alive while the replacement resource set initializes.
            public bool HoldsVisibleResources;
            public RenderTexture PreviousState;
            public RenderTexture CurrentState;
            public RenderTexture Guidance;
            public RenderTexture Topology;
            public RenderTexture TopologySources;
            public RenderTexture ObstacleExclusion;
            public Texture2D Boundary;
            public float Interpolation;
        }

        // The accepted profiler instrumentation remains active throughout the
        // topology proof. Initialization advances through an explicit per-river
        // phase machine, but each
        // existing operation keeps its own marker so cold-start work remains
        // independently measurable while scheduling changes are validated.
        private static readonly ProfilerMarker LateUpdateProfilerMarker =
            new ProfilerMarker("RiverFoam.LateUpdate");
        private static readonly ProfilerMarker EnsureResourcesProfilerMarker =
            new ProfilerMarker("RiverFoam.EnsureResources.Total");
        private static readonly ProfilerMarker InitReleaseOldResourcesProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.ReleaseOldResources");
        private static readonly ProfilerMarker InitLoadComputeProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.LoadCompute");
        private static readonly ProfilerMarker InitResolveKernelsProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.ResolveKernels");
        private static readonly ProfilerMarker InitResolveDimensionsProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.ResolveDimensions");
        private static readonly ProfilerMarker InitAllocateMaterialTexturesProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.AllocateMaterialTextures");
        private static readonly ProfilerMarker InitAllocateTopologyTexturesProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.AllocateTopologyTextures");
        private static readonly ProfilerMarker InitAllocateAuxiliaryTexturesProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.AllocateAuxiliaryTextures");
        private static readonly ProfilerMarker InitClearTopologyProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.ClearTopology");
        private static readonly ProfilerMarker InitAllocateBuffersProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.AllocateBuffers");
        private static readonly ProfilerMarker InitBuildMetricBufferProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.BuildMetricBuffer");
        private static readonly ProfilerMarker InitBuildBoundaryProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.BuildBoundary");
        private static readonly ProfilerMarker InitWaitObstacleProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.WaitObstacleStability");
        private static readonly ProfilerMarker InitResolveTopologyCacheProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.ResolveTopologyCache");
        private static readonly ProfilerMarker InitInstallTopologyCacheProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.InstallTopologyCache");
        private static readonly ProfilerMarker InitBuildObstacleExclusionProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.BuildObstacleExclusion");
        private static readonly ProfilerMarker InitClearMaterialProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.ClearMaterial");
        private static readonly ProfilerMarker InitBuildGuidanceProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.BuildGuidance");
        private static readonly ProfilerMarker TopologyBuildMajorProfilerMarker =
            new ProfilerMarker("RiverFoam.Topology.BuildMajor");
        private static readonly ProfilerMarker TopologyBuildConnectorProfilerMarker =
            new ProfilerMarker("RiverFoam.Topology.BuildConnector");
        private static readonly ProfilerMarker TopologyBuildPocketProfilerMarker =
            new ProfilerMarker("RiverFoam.Topology.BuildPocket");
        private static readonly ProfilerMarker TopologyRefreshSourcesProfilerMarker =
            new ProfilerMarker("RiverFoam.Topology.RefreshSources");
        private static readonly ProfilerMarker TopologyComposeProfilerMarker =
            new ProfilerMarker("RiverFoam.Topology.Compose");
        private static readonly ProfilerMarker MajorEvolutionAdvanceProfilerMarker =
            new ProfilerMarker("RiverFoam.Evolution.AdvanceMajorSlots");
        private static readonly ProfilerMarker MajorEvolutionUploadProfilerMarker =
            new ProfilerMarker("RiverFoam.Evolution.UploadMajorDescriptors");
        private static readonly ProfilerMarker MajorEvolutionBuildProfilerMarker =
            new ProfilerMarker("RiverFoam.Evolution.BuildMajorField");
        private static readonly ProfilerMarker DiagnosticsMeasureTopologyProfilerMarker =
            new ProfilerMarker("RiverFoam.Diagnostics.MeasureTopology");
        private static readonly ProfilerMarker RebuildBuildBoundaryProfilerMarker =
            new ProfilerMarker("RiverFoam.Rebuild.BuildBoundary");
        private static readonly ProfilerMarker RebuildApplyBoundaryProfilerMarker =
            new ProfilerMarker("RiverFoam.Rebuild.ApplyBoundary");
        private static readonly ProfilerMarker RebuildWaitObstacleProfilerMarker =
            new ProfilerMarker("RiverFoam.Rebuild.WaitObstacleStability");
        private static readonly ProfilerMarker RebuildBuildObstacleProfilerMarker =
            new ProfilerMarker("RiverFoam.Rebuild.BuildObstacleExclusion");
        private static readonly ProfilerMarker RebuildRefreshSourcesProfilerMarker =
            new ProfilerMarker("RiverFoam.Rebuild.RefreshTopologySources");
        private static readonly ProfilerMarker ReplacementBuildMajorProfilerMarker =
            new ProfilerMarker("RiverFoam.Replacement.BuildMajor");
        private static readonly ProfilerMarker ReplacementBuildConnectorProfilerMarker =
            new ProfilerMarker("RiverFoam.Replacement.BuildConnector");
        private static readonly ProfilerMarker ReplacementBuildPocketProfilerMarker =
            new ProfilerMarker("RiverFoam.Replacement.BuildPocket");
        private static readonly ProfilerMarker ReplacementPrepareTextureProfilerMarker =
            new ProfilerMarker("RiverFoam.Replacement.PrepareGeneratedTexture");
        private static readonly ProfilerMarker ReplacementActivateProfilerMarker =
            new ProfilerMarker("RiverFoam.Replacement.Activate");
        private static readonly ProfilerMarker TopologyTransitionCaptureProfilerMarker =
            new ProfilerMarker("RiverFoam.Replacement.CaptureTransition");

        private static readonly int FoamEnabledId =
            Shader.PropertyToID("_FoamEnabled");
        private static readonly int FoamPreviousId =
            Shader.PropertyToID("_FoamPrevious");
        private static readonly int FoamCurrentId =
            Shader.PropertyToID("_FoamCurrent");
        private static readonly int FoamBirthDebugId =
            Shader.PropertyToID("_FoamBirthDebug");
        private static readonly int FoamBirthTransferDebugId =
            Shader.PropertyToID("_FoamBirthTransferDebug");
        private static readonly int FoamGuidanceId =
            Shader.PropertyToID("_FoamGuidance");
        private static readonly int FoamTopologyId =
            Shader.PropertyToID("_FoamTopology");
        private static readonly int FoamTopologySourcesId =
            Shader.PropertyToID("_FoamTopologySources");
        private static readonly int FoamBoundaryId =
            Shader.PropertyToID("_FoamBoundary");
        private static readonly int FoamObstacleExclusionId =
            Shader.PropertyToID("_FoamObstacleExclusion");
        private static readonly int FoamInterpolationId =
            Shader.PropertyToID("_FoamInterpolation");
        private static readonly int FoamGlobalStartId =
            Shader.PropertyToID("_FoamGlobalStart");
        private static readonly int FoamFieldLengthId =
            Shader.PropertyToID("_FoamFieldLength");
        private static readonly int FoamColourId =
            Shader.PropertyToID("_FoamColour");
        private static readonly int FoamStrengthId =
            Shader.PropertyToID("_FoamStrength");
        private static readonly int FoamCoverageId =
            Shader.PropertyToID("_FoamCoverage");
        private static readonly int FoamSharpnessId =
            Shader.PropertyToID("_FoamSharpness");
        private static readonly int FoamDetailScaleId =
            Shader.PropertyToID("_FoamDetailScale");
        private static readonly int FoamDetailStrengthId =
            Shader.PropertyToID("_FoamDetailStrength");
        private static readonly int FoamDebugViewId =
            Shader.PropertyToID("_FoamDebugView");
        private static readonly int FoamSeedId =
            Shader.PropertyToID("_FoamSeed");
    }
}
